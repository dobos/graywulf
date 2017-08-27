﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Sql.Parsing
{
    [TestClass]
    public class TableOrViewNameTest
    {
        private TableOrViewName TablenameTestHelper(string query)
        {
            var p = new SqlParser();
            return p.Execute<TableOrViewName>(query);
        }

        [TestMethod]
        public void SimpleTableNameTest()
        {
            var sql = "table1";
            var exp = TablenameTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("table1", exp.FindDescendantRecursive<TableName>().Value);
        }

        [TestMethod]
        public void TableNameWithSchemaTest()
        {
            var sql = "schema1.table1";
            var exp = TablenameTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("schema1", exp.FindDescendantRecursive<SchemaName>().Value);
            Assert.AreEqual("table1", exp.FindDescendantRecursive<TableName>().Value);
        }

        [TestMethod]
        public void TableNameWithDatabaseNameTest()
        {
            var sql = "database1.schema1.table1";
            var exp = TablenameTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("database1", exp.FindDescendantRecursive<DatabaseName>().Value);
            Assert.AreEqual("schema1", exp.FindDescendantRecursive<SchemaName>().Value);
            Assert.AreEqual("table1", exp.FindDescendantRecursive<TableName>().Value);
        }

        [TestMethod]
        public void TableNameWithDatabaseNameWithoutSchemaNameTest()
        {
            var sql = "database1..table1";
            var exp = TablenameTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("database1", exp.FindDescendantRecursive<DatabaseName>().Value);
            Assert.AreEqual("table1", exp.FindDescendantRecursive<TableName>().Value);
        }

        [TestMethod]
        public void TableNameWithDatasetPrefixTest()
        {
            var sql = "dataset:database1.schema1.table1";
            var exp = TablenameTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("dataset", exp.FindDescendantRecursive<DatasetName>().Value);
            Assert.AreEqual("database1", exp.FindDescendantRecursive<DatabaseName>().Value);
            Assert.AreEqual("schema1", exp.FindDescendantRecursive<SchemaName>().Value);
            Assert.AreEqual("table1", exp.FindDescendantRecursive<TableName>().Value);
        }

        [TestMethod]
        public void TableNameWhitespacesTest()
        {
            var sql = "dataset : database1 . schema1 . table1";
            var exp = TablenameTestHelper(sql);
            Assert.AreEqual(sql, exp.Value);
            Assert.AreEqual("dataset", exp.FindDescendantRecursive<DatasetName>().Value);
            Assert.AreEqual("database1", exp.FindDescendantRecursive<DatabaseName>().Value);
            Assert.AreEqual("schema1", exp.FindDescendantRecursive<SchemaName>().Value);
            Assert.AreEqual("table1", exp.FindDescendantRecursive<TableName>().Value);
        }
    }
}
