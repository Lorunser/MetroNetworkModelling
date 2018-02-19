using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// class libraries
using Diction;

namespace Unit_Tests
{
    /// <summary>
    /// Tests BaseDictionary public methods
    /// </summary>
    [TestClass] 
    public class BaseDictionaryTests // passed on 2/1/2017
    {
        // normal and boundary data checks
        [TestMethod]
        public void BasicGetKeyTest()
        {
            string alpha = "abcdefghijklmnopqrstuvwxyz";
            List<string> myList = new List<string>();

            for (int a = 0; a < alpha.Count(); a++)
            {
                myList.Add(alpha.Substring(a, 1));
            }

            BD d = new BD(myList);

            Assert.AreEqual(0, d.GetKey("a"));
            Assert.AreEqual(7, d.GetKey("h"));
            Assert.AreEqual(22, d.GetKey("w"));
            Assert.AreEqual(25, d.GetKey("z"));
        }

        [TestMethod]
        public void BasicGetValueTest()
        {
            string alpha = "abcdefghijklmnopqrstuvwxyz";
            List<string> myList = new List<string>();

            for (int a = 0; a < alpha.Count(); a++)
            {
                myList.Add(alpha.Substring(a, 1));
            }

            BD d = new BD(myList);

            Assert.AreEqual("a", d.GetValue(0));
            Assert.AreEqual("h", d.GetValue(7));
            Assert.AreEqual("w", d.GetValue(22));
            Assert.AreEqual("z", d.GetValue(25));
            Assert.AreEqual(26, d.N);
        }

        [TestMethod]
        public void AdvancedGetKeyTest()
        {
            List<string> myList = new List<string> { "Acton Town", "Lancaster Gate", "St John's Wood", "Waterloo", "Westminster" };
            BD d = new BD(myList);

            Assert.AreEqual(0, d.GetKey("Acton Town"));
            Assert.AreEqual(1, d.GetKey("Lancaster Gate"));
            Assert.AreEqual(2, d.GetKey("St John's Wood"));
            Assert.AreEqual(3, d.GetKey("Waterloo"));
            Assert.AreEqual(4, d.GetKey("Westminster"));
        }

        [TestMethod]
        public void AdvancedAlphabeticalOrderTest()
        {
            List<string> myList = new List<string> { "hello", "hello0", "hell0", "hellod" };
            BD d = new BD(myList);

            Assert.AreEqual(0, d.GetKey("hell0"));
            Assert.AreEqual(1, d.GetKey("hello"));
            Assert.AreEqual(2, d.GetKey("hello0"));
        }

        // erroneous data checks
        [TestMethod]
        public void DuplicateDataTest()
        {
            // dictionary should not allow there to be multiple identical values
            List<string> myList = new List<string> { "Acton Town", "Acton Town", "Lancaster Gate", "St John's Wood", "Waterloo", "Westminster" };
            BD d = new BD(myList);

            Assert.AreEqual(0, d.GetKey("Acton Town"));
            Assert.AreEqual(1, d.GetKey("Lancaster Gate"));
        }

        [TestMethod]
        public void InvalidKeyTest()
        {
            List<string> myList = new List<string> {"Acton Town", "Lancaster Gate", "St John's Wood", "Waterloo", "Westminster" };
            BD d = new BD(myList);

            Assert.IsNull(d.GetValue(-1)); // less than minimum key
            Assert.IsNull(d.GetValue(5)); // greater than maximum key
        }

        [TestMethod]
        public void AbsentValueTest()
        {
            List<string> myList = new List<string> { "Acton Town", "Lancaster Gate", "St John's Wood", "Waterloo", "Westminster" };
            BD d = new BD(myList);

            Assert.AreEqual(-1, d.GetKey("These aren't the driods you're looking for")); // return -1 if value is absent
            Assert.AreEqual(-1, d.GetKey("It's a trap"));
        }

        [TestMethod]
        public void CapitalisationTest()
        {
            List<string> myList = new List<string> { "Acton Town", "Lancaster Gate", "St John's Wood", "Waterloo", "Westminster" };
            BD d = new BD(myList);

            int lancasterId = d.GetKey("Lancaster Gate");

            Assert.AreEqual(lancasterId, d.GetKey("lancaster gate"));
            Assert.AreEqual(lancasterId, d.GetKey("lancaster Gate"));
            Assert.AreEqual(lancasterId, d.GetKey("lAnCaSTer gAtE"));
        }
    }          

    class BD : BaseDictionary // inherits from BaseDictionary to allow abstract class to be used
    {
        public BD(List<string> vals) : base(vals) { }
    }
}
