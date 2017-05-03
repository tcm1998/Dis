using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class DiskFactory
    {
        public enum DiskType
        { 
            PDI,
            DMK,
            DSK,
            SVI,
            HD,
            UNSPECIFIED
        }

        private static Dictionary<string, string> DiskExtensions;

        static DiskFactory()
        {
            String[][] names = new String[][] { new String[]{ "DSK", "D1", "D2" }, new String[] { "DMK" }, new String[]{ "SVI" }, new String[]{ "PDI" } };
            DiskExtensions = new Dictionary<string, string>();
            foreach (String[] aliases in names)
            {
                String main = "";
                bool first = true;
                foreach (string ext in aliases)
                {
                    if (first)
                    {
                        main = ext;                        
                    }
                    first = false;
                    DiskExtensions[ext] = main;
                }
            }
        }


        public static LogicalEntity CreateDisk(string filename, DiskType diskType)
        {
            return CreateDisk(filename, diskType, null);
        }

        public static LogicalEntity CreateDisk(string filename, DiskType diskType, DiskContents contents)
        {
            DiskImage created = null;
            DiskType typeToCreate = diskType;
            if (typeToCreate == DiskType.UNSPECIFIED)
            {
                string ext = System.IO.Path.GetExtension(filename).ToUpper();
                if (ext.Length > 1)
                {
                    ext = ext.Substring(1); // remove the dot
                    if (DiskExtensions.ContainsKey(ext))
                    {
                        ext = DiskExtensions[ext];  // convert to standard extension
                    }
                    if (!Enum.TryParse(ext, out typeToCreate))
                    {
                        typeToCreate = DiskType.UNSPECIFIED;    
                    }     
                }
                
            }
            switch (typeToCreate)
            { 
                case DiskType.DMK:
                    created = new DMKImage(filename, contents);   
                    break;
                case DiskType.DSK:
                    created = new DSKImage(filename, contents);
                    break;
                case DiskType.HD:
                    created = new HDImage(filename, contents);
                    break;
                case DiskType.PDI:
                    created = new PDIImage(filename, contents);
                    break;
                case DiskType.SVI:                    
                    break;       
            }         
            LogicalEntity entity = new LogicalDisk();
            entity.diskImage = created;
            return entity;
        }
    }
}
