﻿using System;
using System.Collections;
using System.Collections.Generic;
using MetX.Standard.Library;
using MetX.Standard.Library.Generics;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Frameworks;

namespace MetX.Tests.Standard.Library
{
    [TestClass]
    public class AssocArrayTests
    {
        [TestMethod]
        public void AssocArray_Simple()
        {
            var data = new AssocArray("Mary") {["Fred"] = {Value = "George"}};
            Assert.AreEqual("George", data["Fred"].Value);
        }

        [TestMethod] [Ignore("Not going to implement this yet (need to find a good non-xml text format)")]
        public void AssocArray_ToString()
        {
            var arrayId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var itemId2 = Guid.NewGuid();

            var data = new AssocArray
            {
                new("Item Key", "Item Value", itemId, "Item Name"),
                new("Item Key2", "Item Value2", itemId2, "Item Name2"),
            };

            var expected = 
$@"
~~~~~~~~~ {arrayId:N}   Array Key
Name:     Array Name
Category: Array Category
Array Value

~~~~      {itemId:N}    Item Key
Name:     Item Name
Item Value

~~~~      {itemId2:N}   Item Key2
Name:     Item Name2
Item Value2

~~~~
";
            var actual = data.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod] [Ignore("XmlSerializer doesn't need testing")]
        public void AssocArray_ToXml()
        {
            var itemId = Guid.NewGuid();
            var data = new AssocArray()
            {
                new("Item Key", "Item Value", itemId, "Item Name") {Number = 1 },
            };

            var expected = 
$@"<AssocArray>
\t<AssocItem Key=""Item Key"" Value=""Item Value"" Name=""Item Name"" ID=""{itemId:D}"" Number=""1"" />
</AssocArray>
";
            var actual = data.ToXml();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssocArray_ValuesArray_Simple()
        {
            var data = new AssocArray {["Fred"] = {Value = "Henry"}, ["George"] = {Value = "Mary"}};
            CollectionAssert.AreEqual(new string[] { "Henry", "Mary"}, data.Values);
        }

        [TestMethod]
        public void AssocArray_KeysArray_Simple()
        {
            var data = new AssocArray {["Fred"] = {Value = "Henry"}, ["George"] = {Value = "Mary"}};
            Assert.IsNotNull(data.Keys);
            CollectionAssert.AreEqual(new string[] { "Fred", "George"}, data.Keys);
        }

        [TestMethod]
        public void AssocArray_NumbersArray_Simple()
        {
            var data = new AssocArray
            {
                ["Fred"] = {Number = int.MaxValue}, 
                ["George"] = {Number = int.MinValue}
            };
            Assert.IsNotNull(data.Numbers);
            CollectionAssert.AreEqual(new[] {int.MaxValue, int.MinValue }, data.Numbers);
        }

        [TestMethod]
        public void AssocArray_NamesArray_Simple()
        {
            var data = new AssocArray {["Fred"] = {Name = "Henry"}, ["George"] = {Name = "Mary"}};
            string[] dataNames = data.Names;
            Assert.IsNotNull(dataNames);

            string dataNameString = string.Join(", ", dataNames);
            Assert.AreEqual(2, dataNames.Length);
            CollectionAssert.AreEqual(new string[] { "Henry", "Mary"}, dataNames, dataNameString);
        }

        [TestMethod]
        public void AssocArray_IDsArray_Simple()
        {
            var data = new AssocArray {["Fred"] = {Name = "Henry"}, ["George"] = {Name = "Mary"}};
            Assert.IsNotNull(data.Ids);
            Assert.AreEqual(2, data.Ids.Length);
            Assert.AreEqual(data.Ids[0], data["Fred"].ID);
            Assert.AreEqual(data.Ids[1], data["George"].ID);
        }

        [TestMethod]
        public void AssocArray_IterableList()
        {
            var assocArray = new AssocArray
            {
                ["Fred"] = {Value = "George"}, 
                ["Mary"] = {Value = "Beth"}, 
                ["Henry"] = {Value = "Greg"},
            };

            Assert.AreEqual(3, assocArray.Count);
            Assert.AreEqual("George", assocArray[0].Value);
            Assert.AreEqual("Beth", assocArray[1].Value);
            Assert.AreEqual("Greg", assocArray[2].Value);
        }
    }
}

