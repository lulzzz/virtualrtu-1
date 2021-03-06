﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace VirtualRtu.Configuration
{
    [Serializable]
    [JsonObject]
    public class VrtuConfig : VConfig
    {
        public VrtuConfig()
        {
        }

        [JsonProperty("symmetricKey")]
        public string SymmetricKey { get; set; }

        [JsonProperty("lifetimeMinutes")]
        public double? LifetimeMinutes { get; set; }        

        [JsonProperty("storageConnectionString")]
        public string StorageConnectionString { get; set; }

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

    }
}
