using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Models.Home
{
    public class HomeIndexModel
    {
        public List<string> autocompleteSource { get; set; }

        public HomeIndexModel()
        {
            this.autocompleteSource = new List<string>();
        }

        public HomeIndexModel(List<string> arrNodeName)
        {
            this.autocompleteSource = new List<string>();
            foreach (var item in arrNodeName)
            {
                this.autocompleteSource.Add(item.Split("/resource/")[1]);
            }
        }
    }
}
