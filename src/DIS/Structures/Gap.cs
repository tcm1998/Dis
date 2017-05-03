using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class Gap
    {
        public int Size { get; set; }
        public byte Filler {get;set;}

        public Gap()
        {
        }

        public Gap(int size, byte filler)
        {
            Size = size;
            Filler = filler;
        }

        public Gap(Gap gap)
        {
            Size = gap.Size;
            Filler = gap.Filler;
        }
      
    }
}
