﻿using MetX.Five;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetX.Tests
{
    [TestClass]
    public class AreaTests
    {
        [TestMethod]
        public void New_TemplateInstruction_TableQuestionMark()
        {
            var processingAlreadyBeganPreviously = false;
            var area = new Area("TemplateName", "~~TemplateName: Table ?", ref processingAlreadyBeganPreviously);
            Assert.AreEqual(InstructionType.Table, area.InstructionType);
            Assert.AreEqual(2, area.Arguments.Count);
            Assert.AreEqual(TemplateType.Table, area.TemplateType);
            Assert.AreEqual("?", area.Target);
        }

    }
}

