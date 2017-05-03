using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class DiskContents
    {
        public PhysicalContents physical;
        public LogicalContents logical;
    }

    public class PhysicalContents
    {
        public byte diskformat { get; set; }        
        public List<TrackInfo> tracks {get;set;}

        public PhysicalContents()
        {
            tracks = new List<TrackInfo>();
        }
    }

    public class LogicalContents
    {
        public List<LogicalEntity> entities;    
    }

    

    

    

    

}
