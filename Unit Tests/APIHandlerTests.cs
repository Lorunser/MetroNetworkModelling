using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

// custom libraries
using API;
using Diction;

namespace Unit_Tests
{
/// <summary>
/// Tests the API Handler class public methods
/// </summary>
    [TestClass]
    public class APIHandlerTests // passed on 1/3/2017
    {
        // normal data tests
        [TestMethod]
        public void GetStationListTest()
        {
            List<string> stations = APIHandler.GetStationList();

            Assert.IsTrue(stations.Contains("Acton Town"));
            Assert.IsTrue(stations.Contains("Lancaster Gate"));
            Assert.IsTrue(stations.Contains("Westminster"));
        }

        [TestMethod]
        public void GetLineListTest()
        {
            List<string> lines = APIHandler.GetLineList();

            Assert.IsTrue(lines.Contains("bakerloo"));
            Assert.IsTrue(lines.Contains("central"));
            Assert.IsTrue(lines.Contains("northern"));
        }

        [TestMethod]
        public void GetRoutesTest()
        {
            var lineDiction = new LineDictionary(APIHandler.GetLineList(), APIHandler.GetLineColors());
            var routes = APIHandler.GetRoutes(lineDiction);

            Assert.IsNotNull(routes); // this method can only be fully test tested within routefinding class
        }

        [TestMethod]
        public void GetLineColorsTest()
        {
            var colors = APIHandler.GetLineColors();

            Assert.IsNotNull(colors); // this method can only be fully tested visually in display of map
        }

        // erroneous data

        [TestMethod]
        public void GetExceptionRoutesTest()
        {
            var lineDiction = new LineDictionary(new List<string> { "this is not a valid line id" }, APIHandler.GetLineColors());
            try
            {
                var routes = APIHandler.GetRoutes(lineDiction);
                Assert.Fail(); // are expecting an exception
            }
            catch (FileNotFoundException)
            {
                Assert.AreEqual(1, 1); // only passes if excpetion is of type FileNotFound
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
