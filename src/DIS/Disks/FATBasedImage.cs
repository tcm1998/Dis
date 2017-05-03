using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{

    public abstract class FATBasedImage: DiskImage
    {
        protected Geometry _geometry;
        private byte[] FATContents;

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
            byte[] boot = readSector(0);
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

        private void GetFAT()
        {
            int FATSize = _geometry.SectorsPerFAT * _geometry.BytesPerSector;
            FATContents = new byte[FATSize];
            for (int iFATSector = 0; iFATSector < _geometry.SectorsPerFAT; iFATSector++)
            {
                byte[] data = readSector(1 + iFATSector);
                Array.Copy(data, 0, FATContents, iFATSector * _geometry.BytesPerSector, _geometry.BytesPerSector);
            }
        }
        
        public override List<LogicalEntity> GetFilesnames(int startCluster)
        {
            List<DirEntry> dirEntries = new List<DirEntry>();
            List<LogicalEntity> names = new List<LogicalEntity>();
            //try            
            int numDirSectors = 0;
            int sector;
            if (startCluster == 0) // rootdir           
            {
                sector = ((_geometry.SectorsPerFAT * _geometry.NumberOfFATs) + _geometry.ReservedSectors);
                numDirSectors = (_geometry.MaxDirEntries * 32) / _geometry.BytesPerSector;                
            }
            else
            {
                sector = _geometry.startDataSector + ((startCluster - 2) * _geometry.SectorsPerCluster);                
            }
            bool done = false;                        
            int cluster = startCluster;
            int sectorCount = 0;
            while (!done)
            {   
                byte[] data = readSector(sector);
                GetFilenames(data, dirEntries, ref done);
                GetNextSector(ref cluster, ref sector);                
                sectorCount++;
                if ((numDirSectors > 0) && (sectorCount > numDirSectors))
                {
                    done = true;
                }
            }         
            //catch
            {

            }
            foreach (DirEntry entry in dirEntries)
            {
                LogicalEntity entity = null;
                if ((entry.attributes & 0x10) != 0)
                {
                    entity = new LogicalDirectory();
                }
                else
                {
                    entity = new LogicalFile();
                }
                entity.diskImage = this;
                entity.name = entry.filename;
                entity.data = entry;
                names.Add(entity);                
            }
            return names;
        }

        private void GetNextSector(ref int cluster, ref int sector)
        {
            if (cluster == 0)
            {
                sector++;
            }
            else
            {
                int firstSector = _geometry.startDataSector + ((cluster - 2) * _geometry.SectorsPerCluster);
                if ((sector - firstSector) > _geometry.SectorsPerCluster)
                {
                    cluster = GetNextCluster(FATContents, cluster);
                    sector = _geometry.startDataSector + ((cluster - 2) * _geometry.SectorsPerCluster);
                }
                else
                {
                    sector++;
                }
                        
            }
        }

        private int GetNextCluster(byte[] contents, int cluster)
        {
            int newCluster;
            {
                if ((cluster & 1) == 0)
                {
                    newCluster = contents[((cluster >> 1) * 3)] + ((contents[((cluster >> 1) * 3) + 1] & 0x0F) << 8);
                }
                else
                {
                    newCluster = ((contents[(((cluster - 1) >> 1) * 3) + 1] & 0xF0) >> 4) + (contents[(((cluster - 1) >> 1) * 3) + 2] << 4);
                }
            }
            return newCluster;
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
                    if (((entry.startCluster == 0) && (entry.length == 0)) || ((entry.startCluster > 1) && (entry.length >= 0)))
                    {
                        if (entry.length < ((720 * 1024) - _geometry.startDataSector))
                        {
                            string filename = ASCIIEncoding.UTF8.GetString(contents, dirOffset + i * 32, 11);
                            entry.filename = cleanFilename(filename);
                            if (entry.filename != ".")
                            { 
                                results.Add(entry);
                            }                            
                        }
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
            if (retVal.Length > 8)
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
