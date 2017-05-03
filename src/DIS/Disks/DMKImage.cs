using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIS
{
    class DMKImage : FloppyImage
    {
        private byte[] indexMark = new byte[] { 0xc2, 0xc2, 0xfc };
        private byte[] addressMark = new byte[] { 0xa1, 0xa1, 0xfe };
        private byte[] dataMark = new byte[] { 0xa1, 0xa1}; // can end with f8 or fb
        
        public DMKImage(string filename)
            : base(filename)
        {
            
        }

        public DMKImage(string filename, DiskContents contents)
            : base(filename, contents)
        {
            
        }

        public override PhysicalContents PerformRead()
        {            
            return Analyze();            
        }

        private PhysicalContents Analyze()
        {
            PhysicalContents _contents = new PhysicalContents();            
            byte[] contents = File.ReadAllBytes(_filename);
            int numTracks = contents[0x01];
            int trackSize = contents[0x02] + (contents[0x03] << 8);
            bool SSOnly = (contents[0x04] & 0x10) == 0x10;
            // ignore the rest for now
            for (int t = 0; t < numTracks; t++)
            {
                TrackInfo track = new TrackInfo();                
                for (int s = 0; s < 2; s++)
                {
                    TrackSide side = new TrackSide();
                    side.physicalTrackNr = t;
                    side.HeadNr = s;
                    int headerOffset = ((t*2)+s) * trackSize + 0x10;                    
                    AnalyzeTrack(side, contents, headerOffset, trackSize);
                    track.sides.Add(side);                    
                }
                _contents.tracks.Add(track);                
            }
            TrackSide track0 = _contents.tracks[0].sides[0];
            _contents.diskformat = 0xFC;
            foreach (SectorInfo sector in track0.sectors)
            {
                if (sector.sector == 1) // found the boot
                {
                    _contents.diskformat = sector.contents[0x15];
                }                
            }
            if (SSOnly)
            {
                _contents.diskformat |= 0x01; // force double sided
            }
            if (numTracks > 42)
            {
                _contents.diskformat &= 0xFB; // force 80 tracks
            }
            foreach (TrackInfo track in _contents.tracks)
            {
                foreach (TrackSide side in track.sides)
                {
                    side.diskformat = _contents.diskformat;
                }
            }            
            return _contents;        
        }

        private void AnalyzeTrack(TrackSide track, byte[] contents, int offset, int tracksize)
        {
            int markPos;
            int startTrack = offset;
            List<int> IDAMS = new List<int>();
            int index = 0;
            while ((index < 64) && GetMultiByteVal(contents,offset + (index * 2),2,false) != 0)
             {
                int idam = (int)GetMultiByteVal(contents, offset + (index * 2), 2, false);
                IDAMS.Add(idam);
                index++;    
            }
            IDAMS.Add(0);   // add end marker
            index = offset + 128;    // end of IDAM block
            int trackstart = index;
            int pos = FindMark(contents, index, indexMark, startTrack+(IDAMS[0] & 0x3FFF),out markPos);
            if (pos != -1)
            { 
                track.gaps[0].Size = pos;
                track.gaps[0].Filler = FindMostOccuringByte(contents, index, pos);
                index += markPos + 4;
            }            
            int count = 0;
            foreach (int idam in IDAMS)
            {
                if (idam != 0)
                {
                    int nextIdamOffset = 0;
                    if (IDAMS[count + 1] > 0)
                    {
                        nextIdamOffset = (IDAMS[count + 1] & 0x3FFF) + startTrack;
                    }
                    else
                    {
                        nextIdamOffset = (startTrack + tracksize) * -1; // negative to indicate start of next track instead
                    }
                    SectorInfo sector = AnalyzeSector(contents, track,ref index, count+1, nextIdamOffset);
                    if (sector != null)
                    {
                        track.sectors.Add(sector);
                    }
                    count++;
                }                
            }
            if (count > 0)
            {
                track.gaps[2].Size /= count;
            }
            if (count > 1)
            {
                track.gaps[3].Size /= (count-1);
            }
            else
            {
                track.gaps[3].Size = 54;   // set to default
            }
            if (track.sectors.Count > 0)
            {
                track.sectors[track.sectors.Count - 1].gaps[1] = track.gaps[3].Size;
                track.sectors[track.sectors.Count - 1].gapFillers[1] = FindMostOccuringByte(contents, index, track.gaps[3].Size);
            }            
            if ((index + track.gaps[3].Size) < (startTrack + tracksize))
            { 
                index += track.gaps[3].Size;
            }            
            track.gaps[4].Size = tracksize - (index - trackstart) - 128; 
            track.gaps[4].Filler = FindMostOccuringByte(contents, index, (tracksize + startTrack) - index);
            if (track.gaps[0].Size > 4)
            {
                track.gaps[0].Size -= 4;    
            }
            FixOverlappedGap(track);
        }

        private void FixOverlappedGap(TrackSide track)
        {
            bool done = false;
            int numSectors = track.sectors.Count;
            for (int i = 0; !done && (i < numSectors); i++)
            {
                SectorInfo sector = track.sectors[i];
                if (sector.contents.Length != SectorSizeFromCode(sector.sizecode))
                {
                    track.gaps[3].Size = sector.gaps[1];
                    track.gaps[3].Filler = sector.gapFillers[1];
                    if (track.sectors.Count > (i + 1))
                    { 
                        track.gaps[2].Size = track.sectors[i +1].gaps[0];
                        track.gaps[2].Filler = track.sectors[i +1].gapFillers[0];
                    }                    
                    done = true;
                }
            }
        }

        private SectorInfo AnalyzeSector(byte[] contents, TrackSide track, ref int index, int sectorBlock, int nextIdamOffset)
        {
            int markPos;            
            SectorInfo sector = new SectorInfo();            
            int offset = 0;
            int pos = FindMark(contents, index + offset, addressMark, 0, out markPos);                
            if (sectorBlock == 1)
            {
                track.gaps[1].Size = pos;
                track.gaps[1].Filler = FindMostOccuringByte(contents, index, pos);
            }
            else
            {
                track.gaps[3].Size += (pos);
                byte filler = FindMostOccuringByte(contents, index, pos);
                if (track.sectors.Count > (sectorBlock - 2))
                { 
                    track.sectors[sectorBlock - 2].gaps[1] = pos;
                    track.sectors[sectorBlock - 2].gapFillers[1] = filler;
                }
                if (sectorBlock == 2) // if we're at the 2nd sector, we're actually looking for the endgap of the first sector
                {
                    track.gaps[3].Filler = filler;
                }
            }
            index += markPos + 4;            
            sector.track = contents[index++];
            sector.head = contents[index++];
            sector.sector = contents[index++];
            sector.sizecode = contents[index++];
            int size = SectorSizeFromCode(sector.sizecode);
            sector.calcHeaderCRC = CalcCRC16(new byte[] { 0xa1, 0xa1, 0xa1, 0xfe, sector.track, sector.head, sector.sector, sector.sizecode }, 0, 8);           
            sector.imgHeaderCRC = (ushort)GetMultiByteVal(contents, index, 2, true);            
            index += 2;            
            pos = FindDataMark(contents, sector, index, out markPos);
            if  ((nextIdamOffset <= 0) || ((index + pos) < nextIdamOffset))
            {
                sector.dam = contents[index + markPos + 3];
                track.gaps[2].Size += pos;
                sector.gaps[0] = pos;
                sector.gapFillers[0] = FindMostOccuringByte(contents, index, pos);                
                if (sectorBlock == 1)
                {
                    track.gaps[2].Filler = sector.gapFillers[0];
                }
                index += markPos + 4;
                ushort dataCRC = CalcCRC16(new byte[] { 0xa1, 0xa1, 0xa1, sector.dam }, 0, 4);
                CalcCRC16(contents, index, size, ref dataCRC);
                sector.calcDataCRC = dataCRC;
                int realSize = FindRealDataSize(contents, size, index, nextIdamOffset);
                sector.contents = new byte[realSize];
                Array.Copy(contents, index, sector.contents, 0, realSize);
                sector.imgDataCRC = (ushort)GetMultiByteVal(contents, index + size, 2, true);
                index += realSize;
                if (sector.calcHeaderCRC != sector.imgHeaderCRC)
                {
                    sector.errorCode |= 0x18;
                }
                else if (sector.calcDataCRC != sector.imgDataCRC)
                {
                    sector.errorCode |= 0x08;
                }
                index += 2;  // checksum
            }
            else
            {
                sector = null;  // ignore this one for now.
            }            
            return sector;
        }

        private int FindMark(byte[]contents, int index, byte[] mark, int maxIndex, out int markPos)
        {            
            int searchStart = index;
            bool notFound = false;
            int pos = -1;
            do
            {                
                markPos = FindBytes(contents, searchStart, mark, maxIndex);
                if (markPos != -1)
                {
                    int mark2Pos = FindBytes(contents, index + markPos + 1, mark, index+markPos + mark.Length);
                    if (mark2Pos == -1)
                    {
                         markPos--;  // the algoritm finds the 2nd byte of the mark, we want the first  
                    }                   
                }
                markPos += (searchStart - index);   // convert offset to a position    
                pos = markPos;                             
                if (pos == -1)
                {
                    notFound = true;
                }
                else
                {
                    pos--;
                    pos = FindStartOfLeader(contents, index + pos);
                    pos -= index;
                    if ((pos < 0) && (markPos > 10))
                    {
                        pos = 0;
                    }
                }
                searchStart += (markPos+4);
            } while (!notFound && (pos < 0));            
            return pos;
        }

        private int FindDataMark(byte[] contents, SectorInfo sector, int offset, out int markPos)
        {            
            byte mark = 0xfb;
            int pos = -1;
            bool done = false;
            int searchOffset = offset;
            do
            {
                pos = FindMark(contents, searchOffset, dataMark, 0, out markPos);                                
                mark = contents[searchOffset + markPos + 3];             
                if ((mark & 0xfc) == 0xf8)
                {
                    done = true;
                    if (mark == 0xf8)
                    {
                        sector.errorCode |= 0x20;   // dam deleted
                    }
                }
                else
                { 
                    searchOffset += (markPos+4);
                }                
            } while (!done) ;
            markPos += (searchOffset - offset);
            return pos + (searchOffset - offset);
        }

        private int FindRealDataSize(byte[] contents, int sectorSize, int startOffset, int nextIDAMOffset)
        {
            int realSize = sectorSize;
            if (nextIDAMOffset > 0)
            {
                if ((startOffset + sectorSize + 20) > nextIDAMOffset) // overlap detected
                {
                    int endGap = nextIDAMOffset - 16; // 12 zero's and 3 0xa1's
                    int startGap = FindStartOfGap(contents, endGap);
                    realSize = startGap - startOffset - 2;  // TODO DSKPro clearly ignores the wrongly placed checksum
                }
            }
            else
            {
                int nextTrackOffset = -nextIDAMOffset;
                if (startOffset + sectorSize > nextTrackOffset)
                {
                    int start = FindStartOfGap(contents, nextTrackOffset - 1);
                    realSize = start - startOffset - 2;   // TODO DSKPro clearly ignores the wrongly placed checksum
                }               
            }
            if (realSize < 0)
            {
                realSize = 0;
            }
            return realSize;                            
        }

        private int FindStartOfGap(byte[] contents, int startOffset)
        {
            int offset = startOffset;
            byte val = contents[offset];
            while (contents[offset - 1] == val)
            {
                offset--;
            }
            if (contents[offset - 1] == 0xFE)
            {
                offset--;
            }
            return offset;
        }

        private int FindStartOfLeader(byte[] contents, int startOffset)
        {            
            int offset = startOffset;
            byte reference = contents[offset];
            byte val = reference;
            if (bitDistance(reference,0,8) < 2)
            {
                reference = 0;
            }
            else 
            {
                reference = 0xFF;
            }          
            bool valid = true;
            int count = 0;
            while ((valid) && (count <13))  // should be 12, but 13 is possible through misreading
            {
                valid = false;
                if (bitDistance(val, reference, 8) < 2)
                {
                    valid = true;
                    offset--;
                    count++;
                }
                val = contents[offset];
            }              
            return offset+1;    
        }

        private int bitDistance(int a, int b, int numBits)
        {
            int retVal = 0;
            int val1 = a;
            int val2 = b;
            for (int i = 0; i < numBits; i++)
            { 
                if ((val1 & 1) != (val2 & 1))
                {
                    retVal++;    
                }
                val1 >>= 1;
                val2 >>= 1;
            }
            return retVal;
        }

        public override void Write()
        {
            WriteDMK(_filename);
        }

        private void WriteDMK(string DMKName)
        {
            int maxTrackSize = FindMaxTrackSize();
            int lastTrack = FindLastTrack();
            bool SSOnly = IsSingleSidedOnly();
            FileStream outputStream = new FileStream(DMKName, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(outputStream);
            WriteDMKHeader(writer, SSOnly);
            foreach (TrackInfo track in Contents.physical.tracks)
            {
                if (track.sides[0].physicalTrackNr < lastTrack)
                {
                    foreach (TrackSide side in track.sides)
                    {
                        WriteTrack(writer, side, maxTrackSize);
                    }                    
                    if (!SSOnly && (track.sides.Count < 2))
                    {
                        WriteTrack(writer, null, maxTrackSize);
                    }
                }
            }
            writer.Close();
        }

        private void WriteDMKHeader(BinaryWriter writer, bool SSOnly)
        {
            writer.Write((byte)0);  // assume not write protected            
            writer.Write((byte)FindLastTrack());
            MaxTrackSize = FindMaxTrackSize();
            writer.Write((ushort)(MaxTrackSize + 128));
            int flags = 0;
            if (SSOnly)
            {
                flags |= 0x10; // set SS Only flag
            }            
            writer.Write((byte)flags);
            WriteBytes(writer, 0, 7); // reserved for future use
            WriteBytes(writer, 0, 4); // Emulator native format
        }

        private void WriteTrack(BinaryWriter writer, TrackSide track, int tracksize)
        {
            if (track == null)
            {
                WriteBytes(writer, 0, tracksize+128);
            }
            else
            {
                Gap[] gaps = GetNormalizedGaps(track, MaxTrackSize);
                WriteIDAMBlock(writer, track, tracksize, gaps);
                WriteTrackData(writer, track, tracksize, gaps);
            }
        }

        private void WriteIDAMBlock(BinaryWriter writer, TrackSide track, int tracksize, Gap[] gaps)
        {
            int sectBlock = 0;
            int totalSectorContent = 0;
            foreach (SectorInfo sect in track.sectors)
            {
                int IDAMpos = totalSectorContent +
                              +128    // IDAM pointer block
                              + gaps[0].Size + gaps[1].Size
                              + 16   // 12x0 + 3xF6 + 1xFC 
                              + 15   // IDAM position within the "per sector" block
                              +
                              (sectBlock *
                                (gaps[2].Size + gaps[3].Size
                                + 40 // 12x0 + 3*A1 + 1xFE + track/head/sector/size(4) + 2*checksum(4) + 12x0 + 3xA1 + 1xFB                                    
                                )
                              );
                writer.Write((ushort)(IDAMpos | 0x8000));
                sectBlock++;
                totalSectorContent += sect.contents.Length;
            }
            WriteBytes(writer, 0, (64 - sectBlock) * 2);  // rest of IDAM block       
        }

        private void WriteTrackData(BinaryWriter writer, TrackSide track, int tracksize, Gap[] gaps)
        {
            List<int> dataStarts = new List<int>();
            byte[] trackData = new byte[tracksize];
            MemoryStream stream = new MemoryStream(trackData);
            BinaryWriter memWriter = new BinaryWriter(stream);            
            WriteGap(memWriter, gaps[0]);
            WriteBytes(memWriter, 0, 12);
            WriteBytes(memWriter, 0xC2, 3);
            WriteBytes(memWriter, 0xFC, 1);    // index mark
            WriteGap(memWriter, gaps[1]);            
            foreach (SectorInfo sect in track.sectors)
            {
                int dam = WriteSector(memWriter, sect, gaps);
                dataStarts.Add(dam);
            }
            WriteGap(memWriter, gaps[4]);
            long testPos = stream.Position;
            memWriter.Close();
            UpdateCRCs(track, trackData, dataStarts);
            writer.Write(trackData);
        }

        private int WriteSector(BinaryWriter writer, SectorInfo sector, Gap[] gaps)
        {
            int DamOffset = -1;
            Stream stream = writer.BaseStream;
            WriteBytes(writer, 0, 12);
            WriteBytes(writer, 0xA1, 3);
            WriteBytes(writer, 0xFE, 1);    // address mark
            WriteBytes(writer, sector.track, 1);
            WriteBytes(writer, sector.head, 1);
            WriteBytes(writer, sector.sector, 1);
            WriteBytes(writer, sector.sizecode, 1);
            WriteChecksum(writer, sector, true);
            WriteGap(writer, gaps[2]);
            WriteBytes(writer, 0, 12);
            WriteBytes(writer, 0xA1, 3);
            if ((sector.errorCode & 0x20) == 0)
            {
                WriteBytes(writer, 0xFB, 1);    // data mark
            }
            else
            {
                WriteBytes(writer, 0xF8, 1);    // deleted data mark
            }
            DamOffset = (int)stream.Position;            
            writer.Write(sector.contents, 0, sector.contents.Length);
            if (sector.contents.Length != SectorSizeFromCode(sector.sizecode))
            {
                WriteBytes(writer, 0, 2);    // just use zero, it's not the right position for the checksum                        
            }
            else
            {
                WriteChecksum(writer, sector, false);
            }
            WriteGap(writer, gaps[3]);
            return DamOffset;
        }

        private bool UpdateCRCs(TrackSide track, byte[] trackData, List<int> dataStarts)
        {
            Dictionary<SectorInfo, bool> sectorsToUpdate = new Dictionary<SectorInfo, bool>();
            int sectNum = 0;            
            int numSect = track.sectors.Count;
            bool succes = true;
            foreach (SectorInfo sector in track.sectors)
            {
                int sectorOffset = 1;
                bool done = false;
                int SectorSize = SectorSizeFromCode(sector.sizecode);
                bool overlapCRC = (sector.contents.Length != SectorSize) && ((sector.errorCode & 0x18) == 0);                
                if  ((overlapCRC) ||  (sectorsToUpdate.ContainsKey(sector)))
                {
                    ushort crc = CalcCRC16(trackData, dataStarts[sectNum]-4, SectorSize + 4);
                    if (overlapCRC)
                    {
                        while (!done)
                        {
                            SectorInfo nextSector = track.sectors[sectNum + sectorOffset];
                            if ((SectorSize + dataStarts[sectNum]) < dataStarts[sectNum + sectorOffset] + nextSector.contents.Length)   // falls within this sector?   
                            {
                                if ((SectorSize + dataStarts[sectNum]) >= dataStarts[sectNum + sectorOffset])
                                {
                                    sectorsToUpdate[nextSector] = true;
                                }
                                else
                                { 
                                    // to do, use gap info
                                    succes = false;
                                }
                                done = true;          
                            }
                            else
                            {
                                sectorOffset++;
                                if ((sectNum + sectorOffset) == numSect)    // lead out gap
                                {
                                    done = true;
                                }
                            }
                        }
                    }
                    if (succes)
                    { 
                        trackData[dataStarts[sectNum] + SectorSize] = (byte)((crc & 0xFF00) >> 8);
                        trackData[dataStarts[sectNum] + SectorSize+1] = (byte)((crc & 0xFF));               
                    }
                      
                }
                sectNum++;
            }
            return succes;
        }

        protected override void WriteExractedTrack(string filename, int trackNr, int headNr)
        {
            int maxTrackSize = FindMaxTrackSize();
            FileStream outputStream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(outputStream);
            TrackSide track = Contents.physical.tracks[trackNr].sides[headNr];
            WriteTrackData(writer, track, maxTrackSize, GetNormalizedGaps(track, maxTrackSize));
        }

        public void WriteExtractedTrack(string filename_in, string filename_out, int trackNr, int HeadNr)
        {
            FileStream inputStream = new FileStream(filename_in, FileMode.Open);
            BinaryReader reader = new BinaryReader(inputStream);
            byte[] header = reader.ReadBytes(16);
            int offset = 0;
            int trackSize = header[0x02] + (header[0x03] << 8);
            if ((header[0x04] & 0x10) != 0)
            {
                offset = trackNr * trackSize;
            }
            else
            { 
                offset = ((trackNr * 2) + HeadNr) * trackSize;
            }
            offset += 16; // skip header
            reader.BaseStream.Seek(offset + 128, SeekOrigin.Begin); // skip IDAM block
            byte[] trackData = reader.ReadBytes(trackSize - 128);
            reader.Close();
            FileStream outputStream = new FileStream(filename_out, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(outputStream);
            writer.Write(trackData);
            writer.Close();
        }
    }
}
