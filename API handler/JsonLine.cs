using System.Collections.Generic;

namespace API
{
    public class JsonLine // response format for line list request
    {
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string modeName { get; set; }
        public List<string> disruptions { get; set; }
        public string created { get; set; }
        public string modified { get; set; }
        public List<string> lineStatuses { get; set; }
        public List<string> routeSections { get; set; }
        public List<ServiceType> serviceTypes { get; set; }

        public class ServiceType
        {
            public string type { get; set; }
            public string name { get; set; }
            public string uri { get; set; }
        }
    }
}
