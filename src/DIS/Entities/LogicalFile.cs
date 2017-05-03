using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    public class LogicalFile : LogicalEntity
    {
        byte[] contents;
        DateTime timestamp;
        int attributes;

        public override List<LogicalEntity> GetItems()
        {
            throw new NotImplementedException();
        }
    }
}
