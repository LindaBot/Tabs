using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tabs
{
    class Todoitem
    {
        [JsonProperty(PropertyName = "Text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "ID")]
        public float ID { get; set; }
    }
}
