using System.Collections.Generic;

namespace API
{
    public class JsonRoute // response format for route request
    {
        public string lineId { get; set; }
        public string lineName { get; set; }
        public string direction { get; set; }
        public bool isOutboundOnly { get; set; }
        public string mode { get; set; }
        public List<string> lineStrings { get; set; }
        public List<Station> stations { get; set; }

        public class Station
        {
            //public string type { get; set; } // find $
            public int routeId { get; set; } // may be unnecessary
            public string parentId { get; set; } // ^^^
            public string stationId { get; set; }
            public string icsId { get; set; }
            public string topMostParentId { get; set; }
            public string direction { get; set; }
            public string towards { get; set; }
            public List<string> modes { get; set; }
            public string stopType { get; set; }
            public string stopLetter { get; set; }
            public string zone { get; set; }
            public string accessibilitySummary { get; set; }
            public bool hasDisruption { get; set; }
            public List<Line> lines { get; set; }

            public class Line
            {
                //public string dollartype { get; set; } // find $
                public string id { get; set; }
                public string name { get; set; }
                public string uri { get; set; }
                public string fullName { get; set; }
                public string type { get; set; }
            }

            public bool status { get; set; }
            public string id { get; set; }
            public string url { get; set; }
            public string name { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public List<StopPointSequence> stopPointSequences { get; set; }

        public class StopPointSequence
        {
            public string lineId { get; set; }
            public string lineName { get; set; }
            public string direction { get; set; }
            public int branchId { get; set; }
            public List<int> nextBranchIds { get; set; }
            public List<int> prevBranchIds { get; set; }
            public List<StopPoint> stopPoint { get; set; }

            public class StopPoint
            {
                public int routeId { get; set; }
                public string parentId { get; set; }
                public string stationId { get; set; }
                public string icsId { get; set; }
                public string topMostParentId { get; set; }
                public string direction { get; set; }
                public string towards { get; set; }
                public List<string> modes { get; set; }
                public string stopType { get; set; }
                public string stopLetter { get; set; }
                public string zone { get; set; }
                public string accessibilitySummary { get; set; }
                public bool hasDisruption { get; set; }
                public List<Line> lines;

                public class Line
                {
                    public string id { get; set; }
                    public string name { get; set; }
                    public string uri { get; set; }
                    public string fullName { get; set; }
                    public string type { get; set; }
                }

                public bool status { get; set; }
                public string id { get; set; }
                public string url { get; set; }
                public string name { get; set; }
                public double lat { get; set; }
                public double lon { get; set; }
            }

            public string serviceType { get; set; }
        }

        public List<OrderedLineRoute> orderedLineRoutes { get; set; }

        public class OrderedLineRoute
        {
            //public string type { get; set; } // find $
            public string name { get; set; }
            public List<string> naptanIds { get; set; } // route sequence
            public string serviceType { get; set; }
        }
    }
}