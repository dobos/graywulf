﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Sql.Parsing
{
    [TestClass]
    public class FunctionIdentifierTest
    {
        private ScalarFunctionCall ExpressionTestHelper(string query)
        {
            var p = new SqlParser();
            return p.Execute<ScalarFunctionCall>(query);
        }

        [TestMethod]
        public void SimpleFunctionNameTest()
        {
            var sql = "function(a)";
            var exp = ExpressionTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("function", exp.FunctionReference.FunctionName);
        }

        [TestMethod]
        public void UdfNameTest()
        {
            var sql = "schema.function(a)";
            var exp = ExpressionTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("schema", exp.FunctionReference.SchemaName);
            Assert.AreEqual("function", exp.FunctionReference.FunctionName);
        }

        [TestMethod]
        public void UdfNameWithDatabaseNameTest()
        {
            var sql = "database.schema.function(a)";
            var exp = ExpressionTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("database", exp.FunctionReference.DatabaseName);
            Assert.AreEqual("schema", exp.FunctionReference.SchemaName);
            Assert.AreEqual("function", exp.FunctionReference.FunctionName);
        }

        [TestMethod]
        public void UdfNameWithEverythingTest()
        {
            var sql = "dataset:database.schema.function(a)";
            var exp = ExpressionTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("dataset", exp.FunctionReference.DatasetName);
            Assert.AreEqual("database", exp.FunctionReference.DatabaseName);
            Assert.AreEqual("schema", exp.FunctionReference.SchemaName);
            Assert.AreEqual("function", exp.FunctionReference.FunctionName);
        }

        [TestMethod]
        public void UdfNameWhitespaceTest()
        {
            var sql = "database . schema . function(a)";
            var exp = ExpressionTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("database", exp.FunctionReference.DatabaseName);
            Assert.AreEqual("schema", exp.FunctionReference.SchemaName);
            Assert.AreEqual("function", exp.FunctionReference.FunctionName);
        }

        [TestMethod]
        [ExpectedException(typeof(NameResolution.NameResolverException))]
        public void NameTooLongTest()
        {
            var sql = "dataset:database.schema.bla.bla.function(a)";
            var exp = ExpressionTestHelper(sql);
        }
    }
}
