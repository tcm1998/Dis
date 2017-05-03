using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class SectorInfo
    {
        public byte track { get; set; }
        public byte head{ get; set; }
        public byte sector{ get; set; }
        public byte sizecode{ get; set; }        
        public ushort calcHeaderCRC{ get; set; }
        public ushort imgHeaderCRC{ get; set; }
        public byte errorCode{ get; set; }
        public ushort calcDataCRC{ get; set; }
        public ushort imgDataCRC{ get; set; }
        public byte[] contents{ get; set; }
        public byte dam { get; set; }
        public int[] gaps { get; set; }
        public byte[] gapFillers { get; set; }

        public SectorInfo()
        {
            gaps = new int[2];
            gapFillers = new byte[2];
        }

    }
}
