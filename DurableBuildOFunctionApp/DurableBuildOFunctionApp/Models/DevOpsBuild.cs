using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBuildOFunctionApp.Models
{
    public class DevOpsBuild
    {
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "parameters", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Parameters { get; set; }

        [JsonProperty(PropertyName = "definition")]
        public Definition Definition { get; set; }

        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public BuildStatus Status { get; set; }

        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public BuildResult Result { get; set; }

        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }

    public enum BuildStatus
    {
        all,
        cancelling,
        completed,
        inProgress,
        none = 0,
        notStarted,
        postponed
    }

    public enum BuildResult
    {
        canceled,
        failed,
        none = 0,
        partiallySucceeded,
        succeeded
    }

    public class Definition
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }
}
