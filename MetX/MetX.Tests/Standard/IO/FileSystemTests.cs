﻿using System.IO;
using MetX.Standard.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetX.Tests.Standard.IO
{
    [TestClass]
    public class FileSystemTests
    {
        [TestMethod]
        public void FindExecutable_Simple()
        {
            var expected = @"c:\windows\system32\xcopy.exe";
            var actual = FileSystem.FindExecutableAlongPath(@"D:\A\B\xcopy.exe");

            Assert.IsNotNull(actual);
            actual = actual.ToLower();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeepContents_PathShouldNotHaveTheFilenameInIt()
        {
            var actual = FileSystem.DeepContents(@"Standard\Generation\CSharp\Project\Pieces\");
            Assert.IsFalse(actual.Files[0].Path.Contains(actual.Files[0].Name));
        }
    }
}