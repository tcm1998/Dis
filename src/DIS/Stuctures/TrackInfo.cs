using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDT
{
    public class TrackInfo
    {
        public List<TrackSide> sides{get;set;}

        public TrackInfo()
        {
            sides = new List<TrackSide>();
        }
    }
}
