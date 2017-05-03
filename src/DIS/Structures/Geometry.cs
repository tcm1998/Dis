using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class Geometry
    {
        public int BytesPerSector { get; set; }
        public int SectorsPerCluster { get; set; }
        public int ReservedSectors { get; set; }
        public int NumberOfFATs { get; set; }
        public int MaxDirEntries { get; set; }
        public int NumSectors { get; set; }
        public int MediaDescriptor { get; set; }
        public int SectorsPerFAT { get; set; }
        public int HiddenSectors { get; set; }
        public int FATOffset { get; set; }
        public int startDataSector { get; set; }
        public int BytesPerTrack { get; set; }
        public int SectorsPerTrack { get; set; }
        public int NumberOfSides { get; set; }        
    }
}
