using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Models
{
    public class Edge
    {
        public int from { get; set; }
        public int to { get; set; }
        public int length { get; set; }
        public string color { get; set; }
        public string label { get; set; }
    }
}
