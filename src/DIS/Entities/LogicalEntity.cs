using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public abstract class LogicalEntity
    {
        public bool dirty { get; set; }

        public DiskImage diskImage {get;set;}        
        public object data {get;set;}
        public String name { get; set; }
        public long startOffset { get; set; }
        public long length { get; set; }

        public abstract List<LogicalEntity> GetItems();        
    }
}
