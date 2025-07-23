using Newtonsoft.Json;
using System.Collections.Generic;
using DBPediaNetwork.Models;

namespace DBPediaNetwork.Services.DBPedia.Models
{
    public class ResourceResult : ResultBase
    {
        public Head head { get; set; }
        public Results results { get; set; }
    }
}
