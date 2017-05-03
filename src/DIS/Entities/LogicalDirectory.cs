using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class LogicalDirectory : LogicalEntity
    {
        public List<LogicalEntity> entities { get; set; }

        public LogicalDirectory()
        {
            entities = new List<LogicalEntity>();
        }

        public override List<LogicalEntity> GetItems()
        {
            List<LogicalEntity> retVal = null;
            DirEntry entry = data as DirEntry;
            if (entry != null)
            {
                retVal = diskImage.GetFilesnames(entry.startCluster); 
            }
            return retVal;
        }
    }
}
