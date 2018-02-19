using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Web.Script.Serialization;

// custom libraries
using Diction;

namespace API
{
    /// <summary>
    /// Class that makes and parses API requests
    /// </summary>
    public static class APIHandler
    {
        private static HttpClient Client = new HttpClient(); // declared static to prevent clogging up of ports

        private static string Authentication;

        private const string AppID = "f67e294c"; // specific App ID as a registered open data user
        private const string AppKey = "21213af9d01ae9393f03fe9cf6aa9b7f"; // used to authenticate requests

        private const string BaseURL = "https://api.tfl.gov.uk/"; // baseUrl for api requests

        private const string LineListRequest = "Line/Mode/tube?"; // appendix for requesting list of lines
        private const string RouteRequest = "Line/{lineId}/Route/Sequence/inbound?serviceTypes=Regular&excludeCrowding=true"; // appendix for requetsing network link data
        private const string StationListRequest = "StopPoint/Type/NaptanMetroStation"; // appendix for requesting list of stations

        static APIHandler() // defines authentication string
        {
            Authentication = "&app_id=" + AppID + "&app_key=" + AppKey;
        }

        // offline methods

        public static void WriteStationData() // writes JSON file from list of stations
        {
            StreamReader r = new StreamReader("stations.txt");
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<JStation> stations = new List<JStation>();
            string name, JSONString;

            while (!r.EndOfStream)
            {
                name = r.ReadLine();
                Console.WriteLine(name);
                stations.Add(new JStation(name));
            }

            JSONString = ser.Serialize(stations);
            File.WriteAllText("stations.json", JSONString);
        }

        private static List<string> GetStationListOffline() // reads JSON file created above and returns list of stations
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<JStation> jStations = new List<JStation>();
            List<string> stationNames = new List<string>();
            string JSONString;

            JSONString = File.ReadAllText("stations.json");
            jStations = ser.Deserialize <List<JStation>>(JSONString);

            foreach (var s in jStations)
            {
                stationNames.Add(s.commonName);
            }

            return stationNames;
        }

        public static void WriteLineColorData() // writes JSON file from static list of line colours
        {
            StreamReader r = new StreamReader("lineColors.txt");
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<JLineColor> lineColors = new List<JLineColor>();
            string col, JSONString;

            while (!r.EndOfStream)
            {
                col = r.ReadLine();
                lineColors.Add(new JLineColor(col));
            }

            JSONString = ser.Serialize(lineColors);
            File.WriteAllText("lineColors.json", JSONString);
        }

        private static List<string> GetLineColorsOffline() // reads JSON file made above and returns list of Line Colours
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<JLineColor> JLineColors = new List<JLineColor>();
            List<string> colors = new List<string>();
            string JSONString;          

            JSONString = File.ReadAllText("lineColors.json");
            JLineColors = ser.Deserialize<List<JLineColor>>(JSONString);

            foreach (var l in JLineColors)
            {
                colors.Add(l.Color);
            }

            return colors;
        }

        // API requests

        public static List<string> GetStationList(bool apiIsSlow = true) // api times out whenever this request is made => better to use offline version
        {
            if (apiIsSlow)
            {
                return GetStationListOffline();
            }

            // api times out whenever this request is made => better to use offline version, called above ^^^

            string JSONString;
            List<string> stationList = new List<string>();
            JavaScriptSerializer ser = new JavaScriptSerializer(); // object to parse json

            try
            {
                JSONString = Client.GetStringAsync(GenQueryURI(StationListRequest)).Result; // make api request (Result appendix makes method synchronous)
                File.WriteAllText("stationsAPI.json", JSONString); // update offline copy
            }

            catch // if api unreachable then use offline copy
            {
                JSONString = File.ReadAllText("stationsAPI.json");
            }

            List<JsonStopPoint> jStops = ser.Deserialize<List<JsonStopPoint>>(JSONString); // parses json into desired format

            foreach (var stop in jStops)
            {
                stationList.Add(stop.commonName); // check common name is correct attribute
            }

            return stationList;
        }

        public static List<JsonRoute> GetRoutes(LineDictionary lineDiction) // returns metro network link data
        {
            string query, JSONString;
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<JsonRoute> routes = new List<JsonRoute>();

            for (int i = 0; i < lineDiction.N; i++)
            {
                query = RouteRequest.Replace("{lineId}", lineDiction.GetValue(i));

                try
                {
                    JSONString = Client.GetStringAsync(GenQueryURI(query)).Result;
                    File.WriteAllText(lineDiction.GetValue(i) + ".json", JSONString); // update latest saved version
                }

                catch // unable to reach api >> use latest saved version
                {
                    JSONString = File.ReadAllText(lineDiction.GetValue(i) + ".json");
                }
                routes.Add(ser.Deserialize<JsonRoute>(JSONString)); // parse json
            }

            return routes;
        }

        public static List<string> GetLineList() // returns list of line names
        {
            string JSONString;
            List<string> lineList = new List<string>();
            JavaScriptSerializer ser = new JavaScriptSerializer();

            try
            {
                JSONString = Client.GetStringAsync(GenQueryURI(LineListRequest)).Result;
                File.WriteAllText("lines.json", JSONString); // update offline copy
            }

            catch // unable to reach api
            {
                JSONString = File.ReadAllText("lines.json"); // use offline copy
            }

            List<JsonLine> jLines = ser.Deserialize<List<JsonLine>>(JSONString); // parse json

            foreach (var line in jLines)
            {
                lineList.Add(line.id);
            }

            return lineList;
        }

        public static List<string> GetLineColors() // returns alphabetically ordered list of colours of each line
        {
            // the api does not hold the colour of each line >> must use offline version
            // response format: #RRGGBB in hex
            return GetLineColorsOffline();
        }

        // Uri generation methods

        private static Uri GenQueryURI(string query) // generates uri for query
        {
            // example query: https://api.tfl.gov.uk/Line/Mode/tube/Route?serviceTypes=Regular&app_id=f67e294c&app_key=21213af9d01ae9393f03fe9cf6aa9b7f
            Uri queryUri = new Uri(GenQueryURIstring(query));
            return queryUri;
        }

        private static string GenQueryURIstring(string query)
        {
            string uriString;
            uriString = BaseURL + query + Authentication;
            return uriString;
        }
    }       
}
