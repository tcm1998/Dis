using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIS
{
    public abstract class FloppyImage : FATBasedImage
    {
        public FloppyImage(string filename)
            : base(filename)
        {            
        }

        public FloppyImage(string filename, DiskContents contents)
            : base(filename, contents)
        {              
        }       
        
        protected int FindMaxTrackSize()
        {
            List<int> sizes = new List<int>();
            int MaxTrackSize = 0;
            foreach (TrackInfo ti in Contents.physical.tracks)
            {
                foreach (TrackSide side in ti.sides)
                {
                    int size = GetTrackSize(side);
                    sizes.Add(size);
                    if (size > MaxTrackSize)
                    {
                        MaxTrackSize = size;
                    }
                }
            }
            if (MaxTrackSize > 6250)
            {
                MaxTrackSize = 6250;    // physical limit
            }
            return MaxTrackSize;
        }

        private int GetTrackSize(TrackSide track)
        {
            int trackSize = 16 + track.gaps[0].Size + track.gaps[1].Size + track.gaps[4].Size;
            foreach (SectorInfo sect in track.sectors)
            {
                trackSize += (40 + track.gaps[2].Size + track.gaps[3].Size + sect.contents.Length);
            }
            return trackSize;
        }

        protected Gap[] GetNormalizedGaps(TrackSide track, int maxSize)
        {            
            Gap[] gaps = new Gap[5];            
            track.gaps[0].Size += 4;    // adding 4 equals the gap to openMSX/DskPro                        
            for (int i = 0; i < 5; i++)
            {
                gaps[i] = new Gap(track.gaps[i]);
            }
            int trackSize = GetTrackSize(track);          
            int oversize = trackSize - maxSize;            
            if (oversize > 0)
            {
                oversize -= reduceGaps(oversize, gaps, new int[] { 4 }, new int[] { 100 });                         
            }
            if (oversize > 0)
            {
                int sectors = track.sectors.Count;
                if (sectors > 0)
                {
                    oversize -= (sectors * reduceGaps((oversize / sectors) + 1, gaps, new int[] { 3, 2 }, new int[] { 20, 20 }));
                }
            }
            if (oversize > 0)
            {
                // Utils.Multiprint(String.Format("Can't reduce track {0}, head {1} from {2} to {3}", track.physicalTrackNr, track.HeadNr, trackSize, maxSize),true);
            }       
            if (oversize < 0)
            {
                gaps[4].Size -= oversize;   // just put any oversize in the lead out gap
            }
            return gaps;
        }

        private int reduceGaps(int reduction, Gap[] gaps, int[]gapsToChange, int[]minSize)
        {
            int totalReduced = 0;
            if (reduction > 0)
            { 
                int[] reductions = new int[5];                
                int totalReduce = 0;
                int index = 0;
                foreach (int changeGap in gapsToChange)
                {
                    if (gaps[changeGap].Size > minSize[index])
                    { 
                        reductions[changeGap] = gaps[changeGap].Size - minSize[index];
                        totalReduce += reductions[changeGap];
                    }
                    index++;
                }
                if (totalReduce > 0)
                { 
                    int toReduce = totalReduce;
                    if (totalReduce > reduction)
                    {
                        toReduce = reduction;
                    }                                   
                    foreach (int changeGap in gapsToChange)
                    {
                        if (totalReduced < toReduce)
                        { 
                            int thisReduction = ((reductions[changeGap] * toReduce) / totalReduce);
                            if (reductions[changeGap] > thisReduction)
                            {
                                thisReduction++;
                            }                       
                            gaps[changeGap].Size -= thisReduction;
                            totalReduced += thisReduction;      // we add up what's actually reduced. This can be less, because of round of errors   
                        }                             
                    }
                }                
            }            
            return totalReduced;
        }



        protected void WriteGap(BinaryWriter writer, Gap gap)
        {
            WriteBytes(writer, gap.Filler, gap.Size);
        }

        protected byte[] GetChecksum(SectorInfo sector, bool header)
        { 
            ushort crc = 0;
            int mask = (sector.errorCode & 0x18) | (header ? 1 : 0);
            switch (mask)
            {
                case 0:
                case 0x18:
                    crc = sector.calcDataCRC;
                    break;
                case 1:
                case 0x9:
                    crc = sector.calcHeaderCRC;
                    break;
                case 0x8:
                    crc = sector.imgDataCRC;
                    break;
                case 0x19:
                    crc = sector.imgHeaderCRC;
                    break;
            }
            byte[] crcBytes = BitConverter.GetBytes(crc);
            return crcBytes;
        }


        protected int WriteChecksum(BinaryWriter writer, SectorInfo sector, bool header)
        {
            byte[] crcBytes = GetChecksum(sector, header);    
            writer.Write(crcBytes[1]);
            writer.Write(crcBytes[0]);  // in Big Endian
            return 2;
        }

        public void WriteExtractedSector(String filename, int track, int head, int blockOrSector, bool isSectorNumber)
        {
            if (blockOrSector == 0)
            {
                WriteExractedTrack(filename, track, head);
            }
            else if (isSectorNumber)
            {
                int count = 0;
                foreach (SectorInfo sector in Contents.physical.tracks[track].sides[head].sectors)
                {
                    if (sector.sector == blockOrSector)
                    {
                        WriteExtractedSector(filename, count, sector);
                        count++;
                    }
                }
            }
            else
            {
                SectorInfo sector = Contents.physical.tracks[track].sides[head].sectors[blockOrSector];
                WriteExtractedSector(filename, 0, sector);
            }
        }

        public void WriteExtractedSector(string filename, int num, SectorInfo sector)
        {
            string outputName = filename;
            if (num > 0)
            {
                outputName += num.ToString();
            }
            FileStream outputStream = new FileStream(outputName, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(outputStream);
            writer.Write(sector.contents, 0, SectorSizeFromCode(sector.sizecode));
            writer.Close();
        }

        protected bool IsSingleSidedOnly()
        {
            bool SS = true;
            int lastTrack = FindLastTrack();
            for (int trk = 0; SS && trk < lastTrack; trk++)
            {
                if ((Contents.physical.tracks[trk].sides.Count > 1) && (Contents.physical.tracks[trk].sides[1].sectors.Count > 0))
                {
                    SS = false; // if we find even 1 sector on side 1, then it's not single sided;
                }
            }
            return SS;
        }

        protected int FindLastTrack()
        {
            int numTracks = Contents.physical.tracks.Count;
            bool done = false;
            while (!done && ((numTracks % 40) != 0))
            {
                foreach (TrackSide side in Contents.physical.tracks[numTracks - 1].sides)
                {
                    if (side.sectors.Count > 0)
                    {
                        done = true;
                    }
                }
                if (!done)
                {
                    numTracks--;
                }
            }
            return numTracks;
        }

        //public override void PrintAnalysis()
        //{            
        //    bool Track80 = (_contents.diskformat & 0x04) == 0;
        //    bool DS = (_contents.diskformat & 0x01) != 0;

        //    Console.WriteLine("Disk Format {0:X2} ({1} tracks, {2})", _contents.diskformat,
        //                                                                Track80 ? 80 : 40,
        //                                                                DS ? "Dual sided" : "Single sided");
        //    Utils.Multiprint("",true);
        //    foreach (TrackInfo info in _contents.tracks)
        //    {
        //        foreach (TrackSide side in info.sides)
        //        {
        //            Utils.Multiprint(String.Format("Physical Track {0}, Head {1}, ", side.physicalTrackNr, side.HeadNr), false);
        //            if (side.diskformat != _contents.diskformat)
        //            {
        //                Utils.Multiprint(String.Format("FT={0:X2}, ", side.diskformat),false);
        //            }
        //            Utils.Multiprint(String.Format(" Flag={0}, ", side.flag),false);
        //            Utils.Multiprint(" Gaps: ",false);
        //            for (int k = 0; k < 5; k++)
        //            {
        //                if (k != 0)
        //                {
        //                    Utils.Multiprint(",",false);
        //                }
        //                Utils.Multiprint(String.Format("{0}x{1:X2}", side.gaps[k].Size, side.gaps[k].Filler),false);
        //            }
        //            Utils.Multiprint(String.Format(" Tracksize = {0}", GetTrackSize(side)),false);
        //            Utils.Multiprint("",true);
        //            foreach (SectorInfo sectorInfo in side.sectors)
        //            {
        //                Utils.Multiprint(String.Format("T:{0}", sectorInfo.track),false);
        //                Utils.Multiprint(String.Format(" H:{0}", sectorInfo.head),false);
        //                Utils.Multiprint(String.Format(" S:{0}", sectorInfo.sector),false);
        //                int size = SectorSizeFromCode(sectorInfo.sizecode);
        //                Utils.Multiprint(String.Format(" Size:{0}", size),false);
        //                if (sectorInfo.contents.Length != size)
        //                {
        //                    Utils.Multiprint(String.Format(" (RS: {0})", sectorInfo.contents.Length),false);
        //                }
        //                Utils.Multiprint(String.Format(" CRChi:{0:X4}", sectorInfo.imgHeaderCRC),false);
        //                Utils.Multiprint(String.Format(" CRChc:{0:X4}", sectorInfo.calcHeaderCRC),false);
        //                Utils.Multiprint(String.Format(" E:{0}", sectorInfo.errorCode),false);
        //                Utils.Multiprint(String.Format(" CRCdi:{0:X4}", sectorInfo.imgDataCRC),false);
        //                Utils.Multiprint(String.Format(" CRCdc:{0:X4}", sectorInfo.calcDataCRC),false);
        //                bool dataMismatch = (sectorInfo.imgDataCRC != sectorInfo.calcDataCRC);
        //                bool headerMismatch = (sectorInfo.imgHeaderCRC != sectorInfo.calcHeaderCRC);
        //                if (dataMismatch || headerMismatch)
        //                {
        //                    Utils.Multiprint(String.Format(" CRCM: {0}{1}", headerMismatch ? "-" : "h",
        //                                                                 dataMismatch ? "-" : "d"),false);
        //                }
        //                Utils.Multiprint("",true);
        //            }
        //        }
        //    }
        //    Utils.Multiprint("", true);
        //    Utils.Multiprint("", true);
        //}
                

        public override byte[] readSector(int sectorNumber)
        {
            return readSector(GetPhysicalSector(sectorNumber));
        }

        private Tuple<int, int, byte> GetPhysicalSector(int sector)
        {
            int track = 0;
            int side = 0;
            byte sectnum = 1;
            if (sector != 0)
            {
                track = sector / (_geometry.NumberOfSides * _geometry.SectorsPerTrack);
                side = (sector / _geometry.SectorsPerTrack) % 2;
                sectnum = (byte)((sector % _geometry.SectorsPerTrack) + 1);
            }
            return new Tuple<int, int, byte>(track, side, sectnum);
        }

        private byte[] readSector(Tuple<int, int, byte> location)
        {
            return readSector(location.Item1, location.Item2, location.Item3);
        }

        private byte[] readSector(int track, int head, byte sector)
        {
            byte[] retVal = null;
            TrackSide ts = Contents.physical.tracks[track].sides[head];
            int numSectors = ts.sectors.Count;
            for (int sect = 0; (retVal == null) && (sect < numSectors); sect++)
            {
                if (ts.sectors[sect].sector == sector)
                {
                    retVal = ts.sectors[sect].contents;
                }
            }
            return retVal;
        }

        public override PhysicalContents PerformRead()
        {
            throw new NotImplementedException();
        }



        public override List<LogicalEntity> GetContainedItems()
        {
            if (_geometry.BytesPerSector == 0)
            {
                Read();
            }
            return GetFilesnames(0);           
        }
    }    
}
