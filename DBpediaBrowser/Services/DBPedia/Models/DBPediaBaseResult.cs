using Newtonsoft.Json;
using System.Collections.Generic;
using DBPediaNetwork.Models;

namespace DBPediaNetwork.Services.DBPedia.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Binding
    {
        public Value value { get; set; }
        public Label label { get; set; }
    }

    public class Head
    {
        public List<object> link { get; set; }
        public List<string> vars { get; set; }
    }

    public class Label
    {
        public string type { get; set; }

        [JsonProperty("xml:lang")]
        public string XmlLang { get; set; }
        public string value { get; set; }
    }

    public class Results
    {
        public bool distinct { get; set; }
        public bool ordered { get; set; }
        public List<Binding> bindings { get; set; }
    }

    public class Value
    {
        public string type { get; set; }
        public string value { get; set; }
    }
}
