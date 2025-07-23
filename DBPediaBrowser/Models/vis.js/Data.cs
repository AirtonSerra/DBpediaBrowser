using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Models.vis.js
{
    public class Data
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }

        public Data()
        {
            this.nodes = new List<Node>();
            this.edges = new List<Edge>();
        }

        public int getNodeId()
        {
            return this.nodes.Count() > 0? this.nodes.Aggregate((x, y) => x.id > y.id ? x : y).id + 1 : this.nodes.Count() + 1;
        }
    }
}
