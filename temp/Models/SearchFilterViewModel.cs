using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Models
{
    public class SearchFilterViewModel
    {
        public string pesquisa { get; set; }
        public int qtdRerouces { get; set; }
        public int qtdLiterais { get; set; }
        public bool refresh { get; set; }
    }
}
