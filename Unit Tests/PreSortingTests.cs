using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// additional imports
using System.ComponentModel;

// class libraries
using Diction;
using PreSorting;
using API;

namespace Unit_Tests
{
    /// <summary>
    /// Tests PreSorting class works as expected
    /// </summary>
    [TestClass]
    public class PreSortingTests
    {
        private StationDictionary sd { get; set; }

        public PreSortingTests()
        {
            sd = new StationDictionary(APIHandler.GetStationList());
        }

        [TestMethod]
        public void SortednessTest()
        {
            // WARNING: this test takes a long time ~ 15 seconds
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            Sorting s = new Sorting(@"C:\Users\ltray\Documents\Computing\Project Code\User Interface\bin\Debug\SourceData.csv");
            s.Sort(sd, bgw);
            List<FinalLine> recs = s.GetRecordsList();
            FinalLine oldRec = recs[0];

            foreach (var rec in recs)
            {
                if (oldRec.StartStn < rec.StartStn) // primary ordering
                {
                    // all good
                }
                else if (oldRec.StartStn == rec.StartStn)
                {
                    if (oldRec.EndStn < rec.EndStn) // secondary ordering
                    {
                        // all good
                    }
                    else if (oldRec.EndStn == rec.EndStn)
                    {
                        if (oldRec.StartTime <= rec.StartTime) // tertiary ordering
                        {
                            // is passing so far
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                }
                else
                {
                    Assert.Fail();
                }

                oldRec = rec;
            }

            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void CorrectReadingTest()
        {
            string line = "4,Wed,LUL,Richmond,Lancaster Gate,0,00:00,10,00:10,Z0110,TKT,N,0,0,XX,Freedom Pass (Elderly)";
            FinalLine f = new FinalLine();
            f.TryInitialise(line, sd);

            Assert.AreEqual(sd.GetKey("Richmond"), f.StartStn);
            Assert.AreEqual(sd.GetKey("Lancaster Gate"), f.EndStn);
            Assert.AreEqual(0, f.StartTime);
            Assert.AreEqual(10, f.EndTime);
        }

        [TestMethod]
        public void InvalidFileTest()
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            Sorting s = new Sorting(@"C:\Users\ltray\Documents\Computing\Project Code\User Interface\bin\Debug\Test.csv");
            try
            {
                s.Sort(sd, bgw);
                Assert.Fail();
            }
            catch
            {
                Assert.AreEqual(1, 1);
            }
        }
    }
}
