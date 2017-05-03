using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIS
{
    class HDImage : FATBasedImage
    {
        private class PartitionInfo
        {
            public bool bootFlag { get; set; }
            public int startHead { get; set; }
            public int startCylinder { get; set; }
            public int startSector { get; set; }
            public int endHead { get; set; }
            public int endCylinder { get; set; }
            public int endTrack { get; set; }
            public long StartTotalSector { get; set; }
            public long numSectors { get; set; }
            public int partType { get; set; }
        }
              

        public HDImage(string filename)
            : base(filename)
        {
            
        }

        public HDImage(string filename, DiskContents contents)
            : base(filename, contents)
        {
            
        }        

        public override PhysicalContents PerformRead()
        {
            return null;    // HD images don't have physical contants
        }

        public override byte[] readSector(int sectorNumber)
        {
            long offset = 0;
            int size = _geometry.BytesPerSector;
            if (sectorNumber == 0)
            {
                offset = fileOffset;
                size = 512;
            }
            else
            {
                if (size == 0)
                {
                    SetGeometry();
                }
                offset = (sectorNumber * _geometry.BytesPerSector) + fileOffset;
            }
            byte[] result = new byte[size];
            FileStream stream = new FileStream(_filename,FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(offset, SeekOrigin.Begin);
            reader.Read(result, 0, size);
            reader.Close();
            return result;
        }

        public override List<LogicalEntity> GetContainedItems()
        {
            byte[] sectorData = readSector(0);
            return GetPartitions(sectorData);
        }

        private List<LogicalEntity> GetPartitions(byte[] contents)
        {
            List<LogicalEntity> entities = new List<LogicalEntity>();
            char partLetter = 'A';
            bool done = false;
            for (int i = 0x1EE; !done && (i > 0x0D); i -= 0x10)
            {
                int numSect = contents[i + 12] + (contents[i + 13] << 8) + (contents[i + 14] << 16) + (contents[i + 15] << 24);
                if (numSect != 0)
                {
                    PartitionInfo part = new PartitionInfo();
                    part.numSectors = numSect;
                    part.bootFlag = ((contents[i] & 0x80) != 0);
                    part.startHead = contents[i + 1];
                    part.startSector = contents[i + 2];
                    part.startCylinder = contents[i + 3];
                    part.partType = contents[i + 4];
                    part.endHead = contents[i + 5];
                    part.endTrack = contents[i + 6];
                    part.endCylinder = contents[i + 7];
                    part.StartTotalSector = contents[i + 8] + (contents[i + 9] << 8) + (contents[i + 10] << 16) + (contents[i + 11] << 24);
                    LogicalEntity newPartition = new LogicalPartition();
                    newPartition.diskImage = this;
                    newPartition.name = "Partition " + partLetter.ToString();
                    newPartition.startOffset = (part.StartTotalSector * 512);
                    newPartition.data = part;
                    entities.Add(newPartition);
                    partLetter++;                   
                }
                else
                {
                    done = true;
                }
            }
            return entities;
        }
    }
}
