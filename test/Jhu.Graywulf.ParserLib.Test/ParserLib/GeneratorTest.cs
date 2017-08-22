﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CSharp;

namespace Jhu.Graywulf.ParserLib.Test
{
    [TestClass]
    public class GeneratorTest
    {
        private string GenerateParser(Type grammar)
        {
            var g = new ParserGenerator();

            var sw = new StringWriter();
            g.Execute(sw, new GrammarInfo(grammar));

            return sw.ToString();
        }

        private void AddInheritedGrammarReference(CompilerParameters cp, Type grammar)
        {
            var g = new GrammarInfo(grammar);
            var i = g.InheritedGrammar;

            if (i != null)
            {
                var code = GenerateParser(i.GrammarType);
                var a = BuildParser(code, i.GrammarType);
                cp.ReferencedAssemblies.Add(a.Location);

                AddInheritedGrammarReference(cp, i.GrammarType);
            }
        }

        private Assembly BuildParser(string code, Type grammar)
        {
            var source = Path.Combine(Path.GetDirectoryName(typeof(Grammar).Assembly.Location), grammar.Name + ".cs");
            var output = Path.Combine(Path.GetDirectoryName(typeof(Grammar).Assembly.Location), grammar.Name + ".dll");
            File.WriteAllText(source, code);
            File.Delete(output);

            var cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add(typeof(Grammar).Assembly.Location);
            AddInheritedGrammarReference(cp, grammar);

            cp.OutputAssembly = output;

            cp.IncludeDebugInformation = true;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = false;

            var csp = new CSharpCodeProvider();
            var cr = csp.CompileAssemblyFromFile(cp, source);

            if (cr.Errors.Count > 0)
            {
                throw new Exception(cr.Errors[0].ErrorText);
            }
            else
            {
                return Assembly.LoadFile(output);
            }
        }

        private Parser CreateParser(Type grammar)
        {
            var g = new GrammarInfo(grammar);
            var code = GenerateParser(grammar);
            var a = BuildParser(code, grammar);

            var p = (Parser)Activator.CreateInstance(a.GetType(String.Format("{0}.{1}", g.Namespace, g.ParserClassName)));

            return p;
        }

        [TestMethod]
        public void ParserExecuteTest()
        {
            var p = CreateParser(typeof(TestGrammar));
            var code = "one, two,three ,four";
            var t = p.Execute(code);
        }

        [TestMethod]
        public void ParserExecuteTest2()
        {
            var p = CreateParser(typeof(InheritedGrammar));
            var code = "one, two,three ,four";
            var t = p.Execute(code);
        }

    }
}
