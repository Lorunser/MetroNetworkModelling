using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

// custom libraries
using Diction;
using Routing;
using API;

namespace Unit_Tests
{
    /// <summary>
    /// Tests Dijkstra works
    /// </summary>
    [TestClass]
    public class RouteFindingTests
    {
        private BaseDictionary[] dictionaries { get; set; }
        private StationDictionary sd { get; set; }
        private LineDictionary ld { get; set; }
        private RouteFinding rf { get; set; }

        public RouteFindingTests()
        {
            // makes necessary api requests
            dictionaries = InitialiseMetroNetwork();
            sd = (StationDictionary)dictionaries[0];
            ld = (LineDictionary)dictionaries[1];
            rf = new RouteFinding(sd.N);
        }

        // basic graph traversal tests
        [TestMethod]
        public void GenChildrenFromIdTest()
        {
            var children = MetroNetwork.GenChildrenFromStationId(sd.GetKey("hammersmith"));
            List<int> childrenIds = new List<int>();

            childrenIds.Add(sd.GetKey("Goldhawk Road"));
            childrenIds.Add(sd.GetKey("Barons Court"));
            childrenIds.Add(sd.GetKey("Ravenscourt Park"));
            childrenIds.Add(sd.GetKey("Turnham Green"));

            foreach (var child in children)
            {
                Assert.IsTrue(childrenIds.Contains(child.StationId));
            }
        }

        // normal data
        [TestMethod]
        public void NoLineChangeTest()
        {
            int originId, destinationId;

            originId = sd.GetKey("notting hill gate");
            destinationId = sd.GetKey("oxford circus");

            rf.SetupFresh(originId);
            PathClass p = rf.Retrace(destinationId);

            /* Expected Path
             * 
             * CENTRAL
             * notting hill gate
             * queensway
             * lancaster gate
             * marble arch
             * bond street
             * oxford circus
             * 
             * */

            CheckCorrectValues(p, sd, ld, 0, 0, "notting hill gate");
            CheckCorrectValues(p, sd, ld, 1, 1, "queensway", "central");
            CheckCorrectValues(p, sd, ld, 2, 2, "lancaster gate", "central");
            CheckCorrectValues(p, sd, ld, 3, 3, "marble arch", "central");
            CheckCorrectValues(p, sd, ld, 4, 4, "bond street", "central");
            CheckCorrectValues(p, sd, ld, 5, 5, "oxford circus", "central");
        }

        [TestMethod]
        public void OneLineChangeTest()
        {
            int originId, destinationId;

            originId = sd.GetKey("Lancaster Gate");
            destinationId = sd.GetKey("Swiss Cottage");

            rf.SetupFresh(originId);
            PathClass p = rf.Retrace(destinationId);

            /* expected path:
             * 
             * CENTRAL LINE
             * lancaster gate
             * marble arch
             * bond street
             * 
             * JUBILEE LINE
             * baker street
             * st. john's wood
             * swiss cottage
             * 
             * */

            CheckCorrectValues(p, sd, ld, 0, 0, "lancaster gate");
            CheckCorrectValues(p, sd, ld, 1, 1, "marble arch", "central");
            CheckCorrectValues(p, sd, ld, 2, 2, "bond street", "central");
            CheckCorrectValues(p, sd, ld, 3, 6, "baker street", "jubilee");
            CheckCorrectValues(p, sd, ld, 4, 7, "st. john's wood", "jubilee");
            CheckCorrectValues(p, sd, ld, 5, 8, "swiss cottage", "jubilee");
        }

        [TestMethod]
        public void TwoLineChangeTest()
        {
            int originId, destinationId;

            originId = sd.GetKey("lambeth north");
            destinationId = sd.GetKey("st. paul's");

            rf.SetupFresh(originId);
            PathClass p = rf.Retrace(destinationId);

            /* expected path:
             * 
             * BAKERLOO
             * lambeth north
             * waterloo
             * 
             * WATERLOO-CITY
             * Bank
             * 
             * CENTRAL
             * st. paul's
             * */

            CheckCorrectValues(p, sd, ld, 0, 0, "lambeth north");
            CheckCorrectValues(p, sd, ld, 1, 1, "waterloo", "bakerloo");
            CheckCorrectValues(p, sd, ld, 2, 5, "bank", "waterloo-city");
            CheckCorrectValues(p, sd, ld, 3, 9, "st. paul's", "central");
        }

        [TestMethod]
        public void ThreeLineChangeTest()
        {
            int originId, destinationId;

            originId = sd.GetKey("marble arch");
            destinationId = sd.GetKey("kingsbury");

            rf.SetupFresh(originId);
            PathClass p = rf.Retrace(destinationId);

            /*
             * Expected path:
             * 
             * CENTRAL
             * marble arch
             * bond street
             * 
             * 
             * JUBILEE
             * baker street
             * st. john's wood
             * swiss cottage
             * finchley road
             * 
             * METROPOLITAN
             * wembley park
             * 
             * JUBILEE
             * kingsbury
             * 
             * */

            CheckCorrectValues(p, sd, ld, 0, 0, "marble arch");
            CheckCorrectValues(p, sd, ld, 1, 1, "bond street", "central");
            CheckCorrectValues(p, sd, ld, 2, 5, "baker street", "jubilee");
            CheckCorrectValues(p, sd, ld, 3, 6, "st. john's wood", "jubilee");
            CheckCorrectValues(p, sd, ld, 4, 7, "swiss cottage", "jubilee");
            CheckCorrectValues(p, sd, ld, 5, 8, "finchley road", "jubilee");
            CheckCorrectValues(p, sd, ld, 6, 12, "wembley park", "metropolitan");
            CheckCorrectValues(p, sd, ld, 7, 16, "kingsbury", "jubilee");
        }

        // I cannot find a four line test

        // boundary data
        [TestMethod]
        public void EndToEndOfLineTest()
        {
            int originId, destinationId;

            originId = sd.GetKey("amersham");
            destinationId = sd.GetKey("aldgate");

            rf.SetupFresh(originId);
            PathClass p = rf.Retrace(destinationId);

            /*
             * expected path:
             * 
             * NORTHERN
             * amersham
             * lots of stations (do not want to count)
             * aldgate
             * */

            CheckCorrectValues(p, sd, ld, 0, 0, "Amersham");
            CheckCorrectValues(p, sd, ld, p.Path.Count() - 1, p.Path.Count() - 1, "Aldgate", "Metropolitan");
        }

        [TestMethod]
        public void CheckLineEndpointsFunction()
        {
            PathClass p;

            rf.SetupFresh(sd.GetKey("Amersham"));
            p = rf.Retrace(sd.GetKey("Edgware"));

            CheckCorrectValues(p, sd, ld, 1, 1, "chalfont & latimer", "metropolitan");

            rf.SetupFresh(sd.GetKey("Uxbridge"));
            p = rf.Retrace(sd.GetKey("West Ruislip"));

            CheckCorrectValues(p, sd, ld, 1, 1, "hillingdon");

            rf.SetupFresh(sd.GetKey("Ealing Broadway"));
            p = rf.Retrace(sd.GetKey("Heathrow Terminal 5"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Ealing Common");

            rf.SetupFresh(sd.GetKey("Richmond"));
            p = rf.Retrace(sd.GetKey("Wimbledon"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Kew Gardens", "District");

            rf.SetupFresh(sd.GetKey("Morden"));
            p = rf.Retrace(sd.GetKey("Brixton"));

            CheckCorrectValues(p, sd, ld, 1, 1, "South Wimbledon", "Northern");

            rf.SetupFresh(sd.GetKey("Stanmore"));
            p = rf.Retrace(sd.GetKey("Stratford"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Canons Park", "Jubilee");

            rf.SetupFresh(sd.GetKey("Upminster"));
            p = rf.Retrace(sd.GetKey("Epping"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Upminster Bridge", "District");

            rf.SetupFresh(sd.GetKey("Cockfosters"));
            p = rf.Retrace(sd.GetKey("Aldgate"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Oakwood", "Piccadilly");

            rf.SetupFresh(sd.GetKey("High Barnet"));
            p = rf.Retrace(sd.GetKey("Mill Hill East"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Totteridge & Whetstone", "Northern");

            rf.SetupFresh(sd.GetKey("Walthamstow Central"));
            p = rf.Retrace(sd.GetKey("Brixton"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Blackhorse Road", "Victoria");

            rf.SetupFresh(sd.GetKey("Barking"));
            p = rf.Retrace(sd.GetKey("Hammersmith"));

            CheckCorrectValues(p, sd, ld, 1, 1, "East Ham");

            rf.SetupFresh(sd.GetKey("Harrow & Wealdstone"));
            p = rf.Retrace(sd.GetKey("Edgware Road"));

            CheckCorrectValues(p, sd, ld, 1, 1, "Kenton", "Bakerloo");
        }

        [TestMethod]
        public void NonDeterminismCheck()
        {
            // two routes with identical scoring
            // this procedure tests if either can be selected
            PathClass p;

            rf.SetupFresh(sd.GetKey("Tottenham Court Road"));
            p = rf.Retrace(sd.GetKey("Baker Street"));

            /*
             * 2 possible paths with same score
             * 
             * 1:
             * 
             * CENTRAL
             * Tottenham Court Road
             * Oxford Circus
             * 
             * BAKERLOO
             * Regent's Park
             * Baker Street
             * 
             * 2:
             * 
             * CENTRAL
             * Tottenham Court Road
             * Oxford Circus
             * Bond Street
             * 
             * JUBILEE
             * Baker Street
             * 
             * 
             * Easy difference is that lineId of stations 1,2 are either equal or different
             * */

            if (p.Path.ElementAt(1).LineIndex == p.Path.ElementAt(2).LineIndex)
            {
                for (int i = 0; i < 100; i++)
                {
                    rf.SetupFresh(sd.GetKey("Tottenham Court Road"));
                    p = rf.Retrace(sd.GetKey("Baker Street"));
                    if (!(p.Path.ElementAt(1).LineIndex == p.Path.ElementAt(2).LineIndex))
                    {
                        Assert.AreEqual(1, 1); // passed test
                        return;
                    }
                }
            }

            else
            {
                for (int i = 0; i < 100; i++)
                {
                    rf.SetupFresh(sd.GetKey("Tottenham Court Road"));
                    p = rf.Retrace(sd.GetKey("Baker Street"));
                    if (p.Path.ElementAt(1).LineIndex == p.Path.ElementAt(2).LineIndex)
                    {
                        Assert.AreEqual(1, 1); // passed test
                        return;
                    }
                }
            }

            Assert.Fail();
        }

        // helper methods
        private void CheckCorrectValues(PathClass p, StationDictionary stationDiction, LineDictionary lineDiction, int index, int score, string stationName, string lineName = "unspecified")
        {
            int stationId = stationDiction.GetKey(stationName);
            int lineId = lineDiction.GetKey(lineName);
            Node n = p.Path.ElementAt(index);

            Assert.AreEqual(score, n.Score);

            if (stationId != -1)
            {
                Assert.AreEqual(stationId, n.StationId);
            }
            else
            {
                throw new ArgumentException(stationName + " is misspelt");
            }

            if (lineId != -1)
            {
                Assert.AreEqual(lineId, n.LineIndex);
            }
        }

        private BaseDictionary[] InitialiseMetroNetwork()
        {
            StationDictionary stationDiction = new StationDictionary(APIHandler.GetStationList());
            LineDictionary lineDiction = new LineDictionary(APIHandler.GetLineList(), APIHandler.GetLineColors());

            MetroNetwork.Initialise(stationDiction, lineDiction);

            BaseDictionary[] dictionaries = new BaseDictionary[] { stationDiction, lineDiction };
            return dictionaries;
        }
    }
}
