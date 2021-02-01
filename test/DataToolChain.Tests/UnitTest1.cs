using DataToolChain.Ui.ExcelVlookupRemover;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XLParser;

namespace DataToolChain.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tree = ExcelFormulaParser.ParseToTree("VLOOKUP(E11,F:I,2,FALSE)+VLOOKUP(E11,$F:I,2,FALSE)+VLOOKUP(E11,F:$I,2,FALSE)+VLOOKUP(E11,1:5,2,FALSE)+VLOOKUP(E11,$1:5,2,FALSE)+VLOOKUP(E11,Sheet1!$1:5,2,FALSE)+VLOOKUP(E11,Sheet1!F:$I,2,FALSE)+VLOOKUP(E11,Sheet1!F1:$I2,2,FALSE)+VLOOKUP(E11,F1:$I2,2,FALSE)");

            ExcelVlookupRemoverHelpers.FormulaSanitizerWalker.MakeReferencesAbsolute(tree.Root, "_test");

            var r = tree.Root.Print();

            Assert.AreEqual("VLOOKUP('_test'!E11,'_test'!F:I,2,FALSE) + VLOOKUP('_test'!E11,'_test'!$F:I,2,FALSE) + VLOOKUP('_test'!E11,'_test'!F:$I,2,FALSE) + VLOOKUP('_test'!E11,'_test'!1:5,2,FALSE) + VLOOKUP('_test'!E11,'_test'!$1:5,2,FALSE) + VLOOKUP('_test'!E11,Sheet1!$1:5,2,FALSE) + VLOOKUP('_test'!E11,Sheet1!F:$I,2,FALSE) + VLOOKUP('_test'!E11,Sheet1!F1:$I2,2,FALSE) + VLOOKUP('_test'!E11,'_test'!F1:$I2,2,FALSE)", r);

        }

        //

        [TestMethod]
        public void TestMethod2()
        {
            var tree = ExcelFormulaParser.ParseToTree(@"VLOOKUP(CONCATENATE($D$12,""_"",$D13),ZMReport!$F:$AG,MATCH('Stress Results'!J$7,ZMReport!$F$4:$AG$4,0),0)/1000");

            ExcelVlookupRemoverHelpers.FormulaSanitizerWalker.MakeReferencesAbsolute(tree.Root, "Stress Results");

            var r = tree.Root.Print();

            Assert.AreEqual("VLOOKUP(CONCATENATE('Stress Results'!$D$12,\"_\",'Stress Results'!$D13),ZMReport!$F:$AG,MATCH('Stress Results'!J$7,ZMReport!$F$4:$AG$4,0),0) / 1000", r);

        }

        //
    }
}
