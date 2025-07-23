using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Models
{
    public class Node
    {
        public int id { get; set; }
        public string label { get; set; }
        public string source { get; set; }
        public bool clicked { get; set; }
        public string color { get; set; }
        public int? idDad { get; set; }
        public string shape { get; set; } = "ellipse";
        public bool isResource { get; set; } = false;
    }
}
