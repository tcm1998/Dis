using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIS
{
    class DSKImage : FloppyImage
    {
        public DSKImage(string filename)
            : base(filename)
        {         
        }

        public DSKImage(string filename, DiskContents contents): 
            base (filename, contents)
        {             
        }

        //public override void PrintAnalysis()
        //{
        //    Utils.Multiprint("Analysis of non-protected disks is not (yet) supported",true);
        //}

        public override void Write()
        {
            WriteDSK(_filename);
        }

        public override PhysicalContents PerformRead()
        {
            return Analyse();            
        }

        private PhysicalContents Analyse()
        {
            PhysicalContents _contents = new PhysicalContents();
            byte[] contents = File.ReadAllBytes(_filename);
            int bytesPerSector = contents[11] + (contents[12] << 8);
            int sectorsPerTrack = contents[24] + (contents[25] << 8);
            int sides = contents[26] + (contents[27] << 8);
            byte diskFormat = contents[21];
            int trackSize = bytesPerSector * sectorsPerTrack;
            int numTracks = contents.Length / (trackSize * sides);
            for (int trk = 0; trk < numTracks; trk++)
            {
                TrackInfo info = new TrackInfo();
                for (int side = 0; side < sides; side++)
                {
                    TrackSide ts = new TrackSide();

                    for (int sector = 0; sector < sectorsPerTrack; sector++)
                    {
                        SectorInfo sec = new SectorInfo();
                        sec.contents = new byte[bytesPerSector];
                        int index = (((trk * sides) + side) * trackSize) + (sector * bytesPerSector);
                        Array.Copy(contents, index, sec.contents, 0, bytesPerSector);
                        ts.sectors.Add(sec);
                    }
                    info.sides.Add(ts);
                }
                _contents.tracks.Add(info);
            }
            FillPhysicalDefaults(_contents, bytesPerSector, diskFormat);
            return _contents;
        }


        private void FillPhysicalDefaults(PhysicalContents _contents, int bytesPerSector, byte diskFormat)
        {
            byte[] sizecodes = new byte[] { 0, 1, 0, 2, 0, 0, 0, 3 };
            _contents.diskformat = diskFormat;
            byte trk = 0;
            byte head = 0;
            byte sec = 1;
            foreach (TrackInfo track in _contents.tracks)
            {
                head = 0;
                foreach (TrackSide side in track.sides)
                {
                    sec = 1;    
                    foreach (SectorInfo sector in side.sectors)
                    {
                        sector.dam = 0xFB;
                        sector.gapFillers = new byte[2]{0x4E,0x4E};
                        sector.gaps = new int[2] { 22, 54 };
                        sector.head = head;
                        sector.track = trk;
                        sector.sector = sec;
                        sector.sizecode = sizecodes[((bytesPerSector / 128) - 1)];
                        sector.calcHeaderCRC = CalcCRC16(new byte[]{0xA1, 0xA1, 0xA1, 0xFE, trk, head, sec, sector.sizecode},0,8);
                        ushort dataCRC = CalcCRC16(new byte[] { 0xA1, 0xA1, 0xA1, 0xFB }, 0, 4);
                        CalcCRC16(sector.contents, 0, bytesPerSector, ref dataCRC);
                        sector.calcDataCRC = dataCRC;
                        sec++;
                    }
                    side.physicalTrackNr = trk;
                    side.HeadNr = head;
                    int[] gapsizes = new int[] { 80, 50, 22, 54, 598 };
                    for (int g = 0; g < 5; g++)
                    {
                        side.gaps[g] = new Gap(gapsizes[g], 0x4E);
                    }
                    side.flag = 1;
                    side.diskformat = diskFormat;
                    head++;
                }
                trk++;
            }
        }

        private void WriteDSK(string DSKName)
        {
            FileStream outputStream = new FileStream(DSKName, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(outputStream);
            int numTracks = ((Contents.physical.tracks.Count) / 40) * 40;
            int numSides = (Contents.physical.tracks[0].sides[0].diskformat & 1) + 1;
            for (int t = 0; t < numTracks; t++)
            {
                for (int h = 0; h < numSides; h++)
                {
                    for (int s = 0; s < 9; s++)
                    {
                        WriteDSKSector(writer, t, h, s+1);
                    }
                }
            }
        }

        private void WriteDSKSector(BinaryWriter writer, int t, int h, int s)
        {
            int block = -1;
            int count = 0;
            foreach (SectorInfo sector in Contents.physical.tracks[t].sides[h].sectors)
            {
                if ((sector.track == t) && (sector.head == h) && (sector.sector == s))
                {
                    block = count;
                }
                count++;
            }
            if (block == -1)    // not found
            {
                WriteBytes(writer, 0, 512);
            }
            else
            {
                byte[] contents = Contents.physical.tracks[t].sides[h].sectors[block].contents;
                if (contents.Length >= 512)
                {
                    writer.Write(contents, 0, 512);
                }
                else
                {
                    writer.Write(contents, 0, contents.Length);
                    WriteBytes(writer, 0, (512 - contents.Length));
                }
            }
        }

        protected override void WriteExractedTrack(string filename, int track, int head)
        {
            throw new NotImplementedException();
        }
    }
}
