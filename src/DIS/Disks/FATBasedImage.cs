using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{

    public abstract class FATBasedImage: DiskImage
    {
        protected Geometry _geometry;

        public FATBasedImage(string filename)
            : base(filename)
        {
            _geometry = new Geometry();
        }

        public FATBasedImage(string filename, DiskContents contents)
            : base(filename, contents)
        {
            _geometry = new Geometry();
        }

        protected void SetGeometry()
        {            
            byte[] boot = ReadBootsector();
            _geometry = new Geometry();
            _geometry.BytesPerSector = boot[11] + (boot[12] << 8);
            _geometry.SectorsPerCluster = boot[13];
            _geometry.ReservedSectors = boot[14] + (boot[15] << 8);
            _geometry.NumberOfFATs = boot[16];
            _geometry.MaxDirEntries = boot[17] + (boot[18] << 8);
            _geometry.NumSectors = boot[19] + (boot[20] << 8);
            _geometry.MediaDescriptor = boot[21];
            _geometry.SectorsPerFAT = boot[22] + (boot[23] << 8);
            _geometry.SectorsPerTrack = boot[24] + (boot[25] << 8);
            _geometry.NumberOfSides = boot[26] + (boot[27] << 8);
            _geometry.HiddenSectors = boot[28] + (boot[29] << 8);
            _geometry.FATOffset = _geometry.BytesPerSector;
            _geometry.startDataSector = (_geometry.SectorsPerFAT * _geometry.NumberOfFATs) + _geometry.ReservedSectors + 7;            
        }

        protected abstract byte[] ReadBootsector();

        public override List<LogicalEntity> GetFilesnames()
        {
            List<DirEntry> dirEntries = new List<DirEntry>();
            List<LogicalEntity> names = new List<LogicalEntity>();
            //try
            {
                int dirSectorOffset = ((_geometry.SectorsPerFAT * _geometry.NumberOfFATs) + _geometry.ReservedSectors);
                int numDirSectors = (_geometry.MaxDirEntries * 32) / _geometry.BytesPerSector;
                bool done = false;
                for (int dirSect = 0; !done && (dirSect < numDirSectors); dirSect++)
                {
                    byte[] data = readSector(dirSectorOffset + dirSect);
                    GetFilenames(data, dirEntries, ref done);
                }
            }
            //catch
            {

            }
            foreach (DirEntry entry in dirEntries)
            {
                LogicalFile lf = new LogicalFile();
                lf.diskImage = this;
                lf.name = entry.filename;
                names.Add(lf);                
            }
            return names;
        }

        private void GetFilenames(byte[] contents, List<DirEntry> results, ref bool done)
        {            
            done = false;
            int dirOffset = 0;
            int numEntries = contents.Length / 32;
            for (int i = 0; i < numEntries; i++)
            {
                byte firstByte = contents[dirOffset + i * 32];
                if ((firstByte != 0xE5) && (firstByte != 0))
                {
                    DirEntry entry = new DirEntry();
                    entry.attributes = contents[dirOffset + i * 32 + 11];
                    entry.startCluster = contents[dirOffset + i * 32 + 26] + (contents[dirOffset + i * 32 + 27] << 8);
                    entry.length = contents[dirOffset + i * 32 + 28] + (contents[dirOffset + i * 32 + 29] << 8) + (contents[dirOffset + i * 32 + 30] << 16) + (contents[dirOffset + i * 32 + 31] << 24);
                    if ((entry.startCluster > 1) && (entry.length >= 0) && (entry.length < ((720 * 1024) - _geometry.startDataSector)))
                    {
                        string filename = ASCIIEncoding.UTF8.GetString(contents, dirOffset + i * 32, 11);
                        entry.filename = cleanFilename(filename);
                        results.Add(entry);
                    }
                }
                else if (firstByte == 0)
                {
                    done = true;
                }
            }
        }

        private string cleanFilename(string filename)
        {
            string retVal = "";
            retVal = filename.Substring(0, 8).Trim();
            if (filename.Length > 8)
            {
                string ext = filename.Substring(8, filename.Length - 8).Trim();
                retVal += ("." + ext);
            }
            return retVal;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        protected override void WriteExractedTrack(string filename, int track, int head)
        {
            throw new NotImplementedException();
        }

        public override PhysicalContents Read()
        {
            if (Contents == null)
            {
                Contents = new DiskContents();
            }
            Contents.physical = PerformRead();
            SetGeometry();
            return Contents.physical;
        }
        
    }
}
