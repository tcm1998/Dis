using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIS
{
    class LogicalDisk : LogicalEntity
    {        
        public override List<LogicalEntity> GetItems()
        {
            return diskImage.GetContainedItems();
        }
    }
}
