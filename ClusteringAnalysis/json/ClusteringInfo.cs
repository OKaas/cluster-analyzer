using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zcu.Graphics.Clustering
{
    public class ClusteringInfo
    {
        [JsonProperty("date")]
        public DateTime date { get; set; }

        [JsonProperty("inputFile")]
        public string inputFile { get; set; }

        [JsonProperty("outputFile")]
        public string outputFile { get; set; }
    }
}
