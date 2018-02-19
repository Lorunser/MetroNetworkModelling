using System;
using System.Collections.Generic;

namespace API
{
    public class JsonStopPoint // response format for station list request
    {
        public string naptanId { get; set; }
        public string platformName { get; set; }
        public string indicator { get; set; }
        public string stopLetter { get; set; }
        public List<string> modes { get; set; }
        public string icsCode { get; set; }
        public string smsCode { get; set; }
        public string stopType { get; set; }
        public string accessibilitySummary { get; set; }
        public string hubNaptanCode { get; set; }
        public List<Line> lines { get; set; }

        public class Line
        {
            string id { get; set; }
            public string name { get; set; }
            public string uri { get; set; }
            public string fullName { get; set; }
            public string type { get; set; }
        }

        public List<LineGroup> lineGroup { get; set; }

        public class LineGroup
        {
            public string naptanIdReference { get; set; }
            public string stationAtcoCode { get; set; }
            public List<string> lineIdentifier { get; set; }

        }

        public List<LineModeGroup> lineModeGroups { get; set; }

        public class LineModeGroup
        {
            public string modeName { get; set; }
            public List<string> lineIdentifier { get; set; }
        }

        public string fullName { get; set; }
        public string naptanMode { get; set; }
        public bool status { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string commonName { get; set; }
        public double distance { get; set; }
        public string placeType { get; set; }
        public List<AdditionalProperty> additionalProperties { get; set; }

        public class AdditionalProperty
        {
            public string category { get; set; }
            public string key { get; set; }
            public string sourceSystemKey { get; set; }
            public string value { get; set; }
            public DateTime modified { get; set; }
        }

        public List<Child> children { get; set; }

        public class Child
        {
            public string id { get; set; }
            public string url { get; set; }
            public string commonName { get; set; }
            public double distance { get; set; }
            public string placeType { get; set; }
            public List<AdditionalProperty> additionalProperties { get; set; }
            public List<string> children { get; set; }
            public List<string> childrenUrls { get; set; }
            public double lat;
            public double lon;
        }

        public List<string> childrenUrls { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
    }
}
