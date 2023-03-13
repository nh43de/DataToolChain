using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DataPowerTools.Connectivity;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;
using Irony.Parsing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.FormulaParsing;
using XLParser;

namespace DataToolChain.Ui.ExcelVlookupRemover
{
    public class VlookupRangeSource 
    {
        public ExcelRangeBase SourceRange { get; set; }
        public string VlookupFormula { get; set; }
        //public string CleanedFormula { get; set; }
        public int Index { get; set; }
    }

    public class ExcelVlookupRemoverHelpers
    {
        public ExcelVlookupRemoverHelpers()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private static string GetSheetRangeReference(string sheetName, string cellReference)
        {
            return $"'{sheetName}'!{cellReference}";
        }

        public void SanitizeVlookupFormulas(string srcFile, string outputFile)
        {
            var templateFile = new FileInfo(srcFile);
            var newFile = new FileInfo(outputFile);

            using var package = new ExcelPackage(newFile, templateFile);

            var d = new FormulaSanitizerWalker(package);

            d.Go();
            

            //loop over all cells again (except temp) and re-substitute calculated values

            //delete __data
            //package.Workbook.Worksheets.Delete("__data");



            //worksheet.InsertRow(5, 2);

            //worksheet.Cells["A5"].Value = "12010";

            //worksheet.Cells["E2:E6"].FormulaR1C1 = "RC[-2]*RC[-1]";

            //var name = worksheet.Names.Add("SubTotalName", worksheet.Cells["C7:E7"]);
            //name.Style.Font.Italic = true;
            //name.Formula = "SUBTOTAL(9,C2:C6)";

            ////Format the new rows
            //worksheet.Cells["C5:C6"].Style.Numberformat.Format = "#,##0";

            //var valueAddress = new ExcelAddress(2, 5, 6, 5);


            //worksheet.View.PageLayoutView = false;

        }




        public class FormulaSanitizerWalker //: TreeWalker<IEnumerable<VlookupRangeSource>>
        {
            private readonly ExcelPackage _package;
            private int _i;
            private readonly IList<VlookupRangeSource> _vlookups = new List<VlookupRangeSource>();
            private readonly ExcelWorksheet tempWs;



            public FormulaSanitizerWalker(ExcelPackage package)
            {
                _package = package;
                //create new worksheet __data
                tempWs = _package.Workbook.Worksheets.Add("__data");
            }

            public void Go()
            {
                _package.Workbook.CalcMode = ExcelCalcMode.Manual;

                //Open the first worksheet
                foreach (var worksheet in Enumerable.Except(_package.Workbook.Worksheets, new[] { tempWs }))
                {
                    var thisSheetName = worksheet.Name;

                    //loop over all cells and replace vlookup references with reference to a cell in the new sheet __data!A[0-9]+
                    foreach (var cell in worksheet.Cells)
                    {
                        var formula = cell.Formula;

                        if (string.IsNullOrWhiteSpace(formula) || formula.ToLower().Contains("vlookup") == false)
                            continue;

                        cell.Formula = SanitizeFormula(cell, thisSheetName); //this removes vlookup references and replaces them with __data sheet references
                    }
                }

                //now look back over all cells *again* and change the __data sheet references to static calculated references

                //call calculate() to calc values on the new sheet
                _package.Workbook.Worksheets["__data"].Calculate();
                
                //foreach (var vlookupRangeGroup in _vlookups.GroupBy(v => v.SourceRange))
                //{
                    //var cell = vlookupRangeGroup.Key;
                    //var vlookups = vlookupRangeGroup.ToArray();
                    
                    //cell.Formula = ResubstituteCalculatedValues(cell);
                //}

                // save our new workbook and we are done!
                _package.Save();
            }

            public string SanitizeFormula(ExcelRangeBase cell, string currentSheetName)
            {
                //var output = new StringBuilder();
                var cellFormula = cell.Formula;

                //parse formula
                var parsedFormula = ExcelFormulaParser.ParseToTree(cellFormula);

                //traverse tree and look for vlookup function calls, when one is found, add it to output list and clean formula
                var parseTree = parsedFormula.Root;

                var stack = new Stack<ParseTreeNode>();
                stack.Push(parseTree);

                //var allNodes = Traverse(item2, node => node.ChildNodes).ToArray();

                while (stack.Any())
                {
                    var next = stack.Pop();

                    //if it's a vlookup then substitute reference and make a note of it
                    if (next.IsFunction() || next.IsBuiltinFunction())
                    {
                        var functionName = next.GetFunction();
                        
                        if (functionName.ToLower() == "vlookup")
                        {
                            _i++;

                            var newRefAddress = $"A{_i}";

                            var newRef = GetSheetRangeReference("__data", newRefAddress); 

                            var parsedNewRef = ExcelFormulaParser.ParseToTree(newRef);

                            //var prefix = parsedNewRef.Root.ChildNodes[0].ChildNodes[0];

                            MakeReferencesAbsolute(next, currentSheetName);

                            //substitute vlookup for __data sheet reference using parse trees
                            var parent = next.Parent(parseTree);
                            parent.ChildNodes.Clear();
                            parent.ChildNodes.Add(parsedNewRef.Root);

                            var vlookupFormula = next.Print();

                            //add the source
                            var v = new VlookupRangeSource()
                            {
                                Index = _i,
                                VlookupFormula = vlookupFormula,
                                SourceRange = cell
                            };
                            _vlookups.Add(v);

                            //also need to copy to the temp sheet
                            tempWs.Cells[newRefAddress].Formula = vlookupFormula;

                            continue; //don't visit children
                        }
                    }
                    
                    //not a vlookup node, so search children
                    var children = next.ChildNodes.ToList();

                    for (var childId = children.Count - 1; childId >= 0; childId--)
                    {
                        stack.Push(children[childId]);
                    }
                }

                //pretty-print modified parse tree
                return parseTree.Print();
            }


            ////needs to be re-written in walker format (e.g. when find a vrange don't search child tokens)
            //private void MakeReferencesAbsolute(ParseTreeNode tree, string currentSheetName)
            //{
            //    //var allReferences = tree.GetReferenceNodes().ToArray();//.AllNodes().Where(node => node.Is(GrammarNames.Reference));
            //    var allReferences = tree.AllNodes().Where(node => node.Is(GrammarNames.Reference)).ToArray();

            //    foreach (var reference in allReferences)
            //    {
            //        var t = ExcelFormulaParser.ParseToTree("$D4:E16");

            //        var t2 = ExcelFormulaParser.ParseToTree("'ZmReport'!$D4:E16");

            //        if (reference.ChildNodes.Count() == 1 && reference.ChildNodes[0].IsFunction())
            //            continue;

            //        //var sheetRefNode = reference.AllNodes().FirstOrDefault(node => node.Is(GrammarNames.TokenSheetQuoted));
            //        var sheetRefNode = reference.SkipToRelevant().ChildNodes.FirstOrDefault(node => node.Is(GrammarNames.Prefix));

            //        if (sheetRefNode == null)
            //        {
            //            //var parent = reference.Parent(tree);
            //            var newRef = GetSheetRangeReference(currentSheetName, reference.Print());

            //            var parsedNewRef = ExcelFormulaParser.ParseToTree(newRef);

            //            reference.ChildNodes.Clear();
            //            reference.ChildNodes.Add(parsedNewRef.Root);

            //            //parent.ChildNodes.Prepend(sheetPrefix);
            //        }
            //    }
            //}

            public static string MakeReferencesAbsolute(ParseTreeNode parseTree, string currentSheetName)
            {
                //traverse tree and look for references
                var stack = new Stack<ParseTreeNode>();
                stack.Push(parseTree);

                while (stack.Any())
                {
                    var next = stack.Pop();


                    string nextStr;
                    try
                    {
                        nextStr = next.Print();
                    }
                    catch (Exception e)
                    {
                        //
                    }

                    //if it's a vlookup then substitute reference and make a note of it
                    if (next.IsRange() || next.IsBinaryReferenceOperation() || next.Is(GrammarNames.Reference))
                    {
                        var sheetRefNode = next.AllNodes().FirstOrDefault(node => node.Is(GrammarNames.Prefix));

                        if (sheetRefNode == null)
                        {
                            //var parent = reference.Parent(tree);
                            var newRef = GetSheetRangeReference(currentSheetName, next.Print());

                            var parsedNewRef = ExcelFormulaParser.ParseToTree(newRef);

                            next.ChildNodes.Clear();
                            next.ChildNodes.Add(parsedNewRef.Root);
                            //parent.ChildNodes.Prepend(sheetPrefix);
                        }

                        continue;
                    }

                    //not a vlookup node, so search children
                    var children = next.ChildNodes.ToList();

                    for (var childId = children.Count - 1; childId >= 0; childId--)
                    {
                        stack.Push(children[childId]);
                    }
                }

                //pretty-print modified parse tree
                return parseTree.Print();
            }



            private string ResubstituteCalculatedValues(ExcelRangeBase cell)
            {
                //var output = new StringBuilder();
                var cellFormula = cell.Formula;

                //parse formula
                var parsedFormula = ExcelFormulaParser.ParseToTree(cellFormula);

                var allReferences = parsedFormula.Root.AllNodes().Where(node => node.Is(GrammarNames.Reference)).ToArray();

                foreach (var reference in allReferences)
                {
                    var sheetRefNode = reference.ChildNodes.FirstOrDefault(node => node.Is(GrammarNames.Prefix));

                    if (sheetRefNode != null)
                    {
                        //var parent = reference.Parent(tree);

                        if (sheetRefNode.Print() != "'__data'!")
                            continue;

                        var cellAddress = reference.ChildNodes[1].Print();

                        tempWs.Cells[cellAddress].Calculate();
                        var newRef = tempWs.Cells[cellAddress].Value.ToString();

                        var parsedNewRef = ExcelFormulaParser.ParseToTree(newRef);

                        reference.ChildNodes.Clear();
                        reference.ChildNodes.Add(parsedNewRef.Root);

                        //parent.ChildNodes.Prepend(sheetPrefix);
                    }
                }

                return parsedFormula.Root.Print();
            }



            /// <summary>
            /// Converts parser nodes tree to flat collection
            /// </summary>
            /// <param name="item"></param>
            /// <param name="childSelector"></param>
            /// <returns></returns>
            public static IEnumerable<ParseTreeNode> Traverse(ParseTreeNode item, Func<ParseTreeNode, IEnumerable<ParseTreeNode>> childSelector)
            {
                var stack = new Stack<ParseTreeNode>();
                stack.Push(item);
                while (stack.Any())
                {
                    var next = stack.Pop();
                    yield return next;

                    var childs = childSelector(next).ToList();
                    for (var childId = childs.Count - 1; childId >= 0; childId--)
                    {
                        stack.Push(childs[childId]);
                    }
                }
            }

        }

        public abstract class TreeWalker<T>
        {
            private readonly ParseTreeNode _rootNode;
            private readonly T _seed;

            protected TreeWalker(ParseTreeNode rootNode, T seed)
            {
                _rootNode = rootNode;
                _seed = seed;

            }

            public T Walk(T seed)
            {
                var r = Traverse(_rootNode, seed);

                return seed;
            }

            protected abstract T Accumulate(T existing, ParseTreeNode context);

            protected abstract IEnumerable<ParseTreeNode> ChildSelector(ParseTreeNode item);

            /// <summary>
            /// Converts parser nodes tree to flat collection
            /// </summary>
            /// <param name="item"></param>
            /// <param name="seed"></param>
            /// <returns></returns>
            public IEnumerable<ParseTreeNode> Traverse(ParseTreeNode item, T seed)
            {
                var stack = new Stack<ParseTreeNode>();
                stack.Push(item);

                while (stack.Any())
                {
                    var next = stack.Pop();
                    yield return next;

                    var childs = ChildSelector(next).ToList();
                    seed = Accumulate(seed, next);

                    for (var childId = childs.Count - 1; childId >= 0; childId--)
                    {
                        stack.Push(childs[childId]);
                    }
                }
            }



        }
}

}
