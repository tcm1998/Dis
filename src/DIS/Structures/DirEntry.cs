using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class DirEntry
    {
        public byte[] contents;
        public String filename;
        public int startCluster;
        public int length;
        public int attributes;
    }
}
