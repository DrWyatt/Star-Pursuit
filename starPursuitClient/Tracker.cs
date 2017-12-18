using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starPursuitShared
{
    [Serializable]
    public class Tracker
    {
        public int TrackerID { get; set; }
        public int entity { get; set; }
        public bool ai { get; set; }
    }
}
