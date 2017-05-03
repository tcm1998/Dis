using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class TrackSide
    {
        public List<SectorInfo> sectors { get; set; }
        public Gap[] gaps { get; set; }
        public int physicalTrackNr { get; set; }
        public int HeadNr { get; set; }
        public byte diskformat { get; set; }
        public byte flag { get; set; }

        public TrackSide()
        {
            gaps = new Gap[5];
            for (int i = 0; i < 5; i++)
            {
                gaps[i] = new Gap();
            }
            sectors = new List<SectorInfo>();
        }
    }
}
