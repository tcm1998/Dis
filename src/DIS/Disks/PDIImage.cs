using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIS
{
    class PDIImage : FloppyImage
    {
        public PDIImage(string filename) : base (filename)
        {            
        }

        public PDIImage(string filename, DiskContents contents)
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
            int NumTrackBlocks = contents.Length / 0x2B00;
            _contents.diskformat = contents[0x08];
            bool DS = ((_contents.diskformat & 1) == 1);
            for (int i = 0; i < NumTrackBlocks; i++)
            {
                TrackInfo trackInfo = new TrackInfo();
                for (int j = 0; j < 2; j++)
                {                    
                    TrackSide trackside = new TrackSide();
                    if (!DS || j == 0)
                    {
                        trackInfo = new TrackInfo();
                        trackInfo.sides = new List<TrackSide>();
                        _contents.tracks.Add(trackInfo);
                    }
                    trackInfo.sides.Add(trackside);
                    int PhysTrack = DS ? i : i/2;
                    int PhysHead = DS ? j : 0;                   
                    trackInfo.sides[PhysHead] = trackside;
                    trackside.physicalTrackNr = PhysTrack;
                    trackside.HeadNr = PhysHead;
                    trackside.sectors = new List<SectorInfo>();                    
                    long adr = (i * 0x2B00) + j * 0x180;
                    trackside.diskformat = contents[adr + 0x08];
                    int Sectors = contents[adr + 0x09];
                    trackside.flag = contents[adr + 0x0A];
                    trackside.gaps = new Gap[5];
                    for (int k = 0; k < 5; k++)
                    {
                        trackside.gaps[k] = new Gap(((contents[adr + (k * 2) + 0x11]) << 8) + (contents[adr + (k * 2) + 0x10]),
                                                      contents[adr + k + 0x0B]);                      
                    }                    
                    long dataAddr = (i * 0x2B00) + 0x300 + (j * 0x1400);
                    long sectOffset = 0;
                    for (int sect = 0; sect < Sectors; sect++)
                    {
                        SectorInfo sectInfo = AnalyzeSector(contents, dataAddr + sectOffset, adr, sect);
                        sectOffset += sectInfo.contents.Length;
                        trackside.sectors.Add(sectInfo);
                    }                    
                }
            }
            return _contents;
        }

        private SectorInfo AnalyzeSector(byte[] contents, long dataOffset, long adr, int sect)
        {
            SectorInfo sectorData = new SectorInfo();
            sectorData.track = contents[adr + 0x1A + sect];
            sectorData.head = contents[adr + 0x38 + sect];
            sectorData.sector = contents[adr + 0x56 + sect];
            sectorData.sizecode = contents[adr + 0x74 + sect];
            int sectsize = SectorSizeFromCode(sectorData.sizecode);
            sectorData.imgHeaderCRC = (ushort)(((contents[adr + 0x92 + sect] << 8)) + contents[adr + 0xb0 + sect]);
            sectorData.errorCode = contents[adr + 0xce + sect];
            int realsize = contents[adr + 0xec + (sect * 2)] + ((contents[adr + 0xed + (sect * 2)]) << 8);
            int sectorLength = sectsize;
            if (realsize != 0)
            {
                sectorLength = realsize;
            }
            sectorData.contents = new byte[sectorLength];            
            sectorData.dam = 0xfb;
            if ((sectorData.errorCode & 0x20) != 0) // dam deleted ?
            {
                sectorData.dam = 0xf8;    
            }
            ushort crc = 0xffff;
            CalcCRC16(new byte[] {0xa1,0xa1,0xa1,sectorData.dam}, 0, 4, ref crc);
            CalcCRC16(contents, dataOffset, sectsize, ref crc);
            sectorData.calcDataCRC = crc;
            crc = 0xffff;
            CalcCRC16(new byte[] { 0xa1, 0xa1, 0xa1, 0xfe, sectorData.track, sectorData.head, sectorData.sector, sectorData.sizecode }, 0, 8, ref crc);
            sectorData.calcHeaderCRC = crc;
            Array.Copy(contents, dataOffset, sectorData.contents, 0, sectorLength);
            sectorData.imgDataCRC = (ushort)(((contents[adr + 0x128 + (sect * 2)]) << 8) + contents[adr + 0x129 + (sect * 2)]);                        
            return sectorData;   
        }

        public override void Write()
        {
            WritePDI(_filename);
        }

        private void WritePDI(string filename)
        {
            PhysicalContents _contents = Contents.physical;
            byte[] buffer = new byte[0x180];
            FileStream outputStream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(outputStream);
            int numTracks = _contents.tracks.Count;
            int numSides = (_contents.diskformat & 1) + 1;
            if ((numSides == 1) && ((numTracks % 2) == 1))  // we can't have an odd track
            {
                TrackInfo track = new TrackInfo();
                track.sides.Add(new TrackSide());
                _contents.tracks.Add(track);
            }
            int numBlocks = numTracks * numSides;
            for (int block = 0; block < numBlocks; block += 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    Array.Clear(buffer, 0, 0x180);
                    int track = (block+i) / numSides;
                    int side = (block+i) % numSides;
                    TrackSide ts = _contents.tracks[track].sides[side];                    
                    Array.Copy(new ASCIIEncoding().GetBytes("PDI-MSX"), 0, buffer, 0, 7);
                    buffer[0x07] = 0x20;
                    buffer[0x08] = _contents.diskformat;
                    buffer[0x09] = (byte)ts.sectors.Count;
                    buffer[0x0a] = (byte)1;
                    for (int g = 0; g < 5; g++)
                    {
                        buffer[0x0b + g] = (byte)ts.gaps[g].Filler;
                        buffer[0x10 + (g * 2)] = (byte)(ts.gaps[g].Size & 0xFF);
                        buffer[0x11 + (g * 2)] = (byte)((ts.gaps[g].Size & 0xFF00) >> 8);
                    }
                    int count = 0;
                    foreach (SectorInfo sector in ts.sectors)
                    {
                        buffer[0x1A + count] = sector.track;
                        buffer[0x38 + count] = sector.head;
                        buffer[0x56 + count] = sector.sector;
                        buffer[0x74 + count] = sector.sizecode;
                        buffer[0x92 + count] = (byte)((sector.imgHeaderCRC & 0xFF00) >> 8);
                        buffer[0xb0 + count] = (byte)(sector.imgHeaderCRC & 0xFF);
                        buffer[0xce + count] = sector.errorCode;
                        int dataSize = SectorSizeFromCode(sector.sizecode);
                        int realSize = 0;
                        if (sector.contents.Length != dataSize)
                        {
                            realSize = sector.contents.Length;
                            if (realSize == 0)
                            {
                                realSize = 1;   // PDI doesn't support a zero size sector.
                            }
                        }
                        buffer[0xec + (2 * count)] = (byte)(realSize & 0xFF);
                        buffer[0xed + (2 * count)] = (byte)((realSize & 0xFF00) >> 8);                        
                        buffer[0x128 + (2 * count)] = (byte)(sector.imgDataCRC & 0xFF);
                        buffer[0x129 + (2 * count)] = (byte)((sector.imgDataCRC & 0xFF00) >> 8);
                        count++;
                    }
                    writer.Write(buffer,0,0x180);
                }
                for (int i = 0; i < 2; i++)
                {
                    int track = (block + i) / numSides;
                    int side = (block + i) % numSides;
                    TrackSide ts = _contents.tracks[track].sides[side];
                    int byteCount = 0;
                    foreach (SectorInfo sector in ts.sectors)
                    {
                        writer.Write(sector.contents, 0, sector.contents.Length);
                        byteCount += sector.contents.Length;
                    }
                    WriteBytes(writer, 0xF7, (0x1400 - byteCount));
                }
            }
        }

        protected override void WriteExractedTrack(string filename, int track, int head)
        {
            throw new NotImplementedException();
        }
    }
}
