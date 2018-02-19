using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// class libraries
using Diction;
using API;
using Routing;

namespace Unit_Tests
{
    /// <summary>
    /// Tests Editing of Graph works as expected
    /// </summary>
    [TestClass]
    public class MetroNetworkEditingTests
    {
        // WARNING: this class should not be run before FindingRoutesTests
        // as then edited routes interfere

        private BaseDictionary[] dictionaries { get; set; }
        private StationDictionary sd { get; set; }
        private LineDictionary ld { get; set; }
        private RouteFinding rf { get; set; }

        public MetroNetworkEditingTests()
        {
            // makes necessary api requests
            dictionaries = InitialiseMetroNetwork();
            sd = (StationDictionary)dictionaries[0];
            ld = (LineDictionary)dictionaries[1];
            rf = new RouteFinding(sd.N);
        }

        // normal data
        [TestMethod]
        public void InsertBreakTest()
        {
            int parentId = sd.GetKey("lancaster gate");
            var children = MetroNetwork.GenChildrenFromStationId(parentId);
            Assert.IsTrue(CheckContains(children, "queensway"));
            InsertBreakBetween("lancaster gate", "queensway");
            children = MetroNetwork.GenChildrenFromStationId(parentId);
            Assert.IsFalse(CheckContains(children, "queensway"));
        }

        [TestMethod]
        public void EnsureRouteHasChanged()
        {
            int parentId = sd.GetKey("green park");
            int victoriaId = sd.GetKey("victoria");

            rf.SetupFresh(parentId);
            PathClass p = rf.Retrace(victoriaId);

            /* standard path
             * 
             * VICTORIA
             * green park
             * victoria
             * 
             * */

            Assert.AreEqual(2, p.Path.Count());

            InsertBreakBetween("green park", "victoria");

            rf.SetupFresh(parentId);
            p = rf.Retrace(victoriaId);

            /*
             * new path
             * 
             * JUBILEE
             * green park
             * westminster
             * 
             * DISTRICT/CIRCLE
             * st. james's' park
             * victoria
             * 
             * */

            Assert.AreEqual(4, p.Path.Count()); // path length has doubled due to break

        }

        // disconnected graph
        [TestMethod]
        public void DisconnectTest()
        {
            InsertBreakBetween("St. John's Wood", "Swiss Cottage");
            InsertBreakBetween("St. John's Wood", "Baker Street");

            rf.SetupFresh(sd.GetKey("St. John's Wood"));
            PathClass p = rf.Retrace(sd.GetKey("Hammersmith"));

            Assert.AreEqual(1, p.Path.Count()); // only hammersmith is in path >> no route exists
        }

        // helper methods
        private BaseDictionary[] InitialiseMetroNetwork()
        {
            StationDictionary stationDiction = new StationDictionary(APIHandler.GetStationList());
            LineDictionary lineDiction = new LineDictionary(APIHandler.GetLineList(), APIHandler.GetLineColors());

            MetroNetwork.Initialise(stationDiction, lineDiction);

            BaseDictionary[] dictionaries = new BaseDictionary[] { stationDiction, lineDiction };
            return dictionaries;
        }

        private void InsertBreakBetween(string stationA, string stationB)
        {
            int aId = sd.GetKey(stationA);
            int bId = sd.GetKey(stationB);

            if (aId == -1)
            {
                throw new ArgumentException(stationA + " is misspelt");
            }

            if (bId == -1)
            {
                throw new ArgumentException(stationB + " is misspelt");
            }

            var children = MetroNetwork.GenChildrenFromStationId(aId);

            for (int i = 0; i < children.Count(); i++)
            {
                if (children[i].StationId == bId)
                {
                    MetroNetwork.InsertBreakBetween(aId, children[i]);
                    break;
                }
            }
        }

        public bool CheckContains(List<Node> nodes, string name)
        {
            int id = sd.GetKey(name);
            foreach (var node in nodes)
            {
                if (id == node.StationId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
