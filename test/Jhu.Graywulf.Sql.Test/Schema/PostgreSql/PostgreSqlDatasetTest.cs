﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.Schema.PostgreSql;

namespace Jhu.Graywulf.Schema.PostgreSql.Test
{

    [TestClass]
    public class PostgreSqlDatasetTest
    {

        private PostgreSqlDataset CreateTestDataset()
        {
            var csb = new NpgsqlConnectionStringBuilder(Jhu.Graywulf.Schema.Test.AppSettings.PostgreSqlConnectionString);

            var ds = new PostgreSqlDataset(Jhu.Graywulf.Test.Constants.TestDatasetName, csb.ConnectionString)
            {
                DatabaseName = csb.Database
            };

            return ds;
        }

        #region Dataset tests

        [TestMethod]
        public void GetAnyObjectTest()
        {
            var ds = CreateTestDataset();

            Assert.IsInstanceOfType(ds.GetObject(ds.DatabaseName, "", "TableWithPrimaryKey"), typeof(Table));
            Assert.IsInstanceOfType(ds.GetObject(ds.DatabaseName, "", "ViewWithStar"), typeof(View));

            // Postgres unifies all types of functions under stored procedures
            Assert.IsInstanceOfType(ds.GetObject(ds.DatabaseName, "", "ScalarFunction"), typeof(StoredProcedure));
        }

        /*
         * TODO: statistics are not implemented on non-SQL Server platforms
        [TestMethod]
        public void GetStatistics()
        {
            var ds = CreateTestDataset();

            Assert.IsTrue(ds.Statistics.DataSpace > 0);
        }
        */

        #endregion
        #region Table tests

        [TestMethod]
        public void GetSingleTableTest()
        {
            var ds = CreateTestDataset();

            var t = ds.Tables[ds.DatabaseName, "public", "Author"];

            Assert.IsTrue(ds.Tables.Count == 1);
        }

        [TestMethod]
        public void GetSingleTableWithoutSchemaNameTest()
        {
            var ds = CreateTestDataset();

            var t1 = ds.Tables[ds.DatabaseName, "public", "Author"];
            var t2 = ds.Tables[ds.DatabaseName, "", "Author"];

            Assert.IsTrue(ds.Tables.Count == 1);

            Assert.AreEqual(t1, t2);
        }


        [TestMethod]
        public void GetMultipleTableTest()
        {
            var ds = CreateTestDataset();

            var t1 = ds.Tables[ds.DatabaseName, "public", "Author"];
            var t2 = ds.Tables[ds.DatabaseName, "", "Author"];
            Table t3 = ds.Tables[ds.DatabaseName, "", "Book"];

            Assert.IsTrue(ds.Tables.Count == 2);
            Assert.AreNotEqual(t1, t3);
            Assert.AreEqual("public", t3.SchemaName);
        }

        [TestMethod]
        public void GetNonexistentTableTest()
        {
            var ds = CreateTestDataset();

            try
            {
                var t = ds.Tables[ds.DatabaseName, "", "NonExistentTable"];
                Assert.Fail();
            }
            catch (KeyNotFoundException)
            {
            }
        }

        [TestMethod]
        public void LoadAllTablesTest()
        {
            var ds = CreateTestDataset();
            ds.Tables.LoadAll();

            Assert.AreEqual(5, ds.Tables.Count);    // Update this if test database schema changes
            Assert.IsTrue(ds.Tables.IsAllLoaded);
        }

        [TestMethod]
        public void GetTableColumnsTest()
        {
            var ds = CreateTestDataset();

            // Get a single table
            var t1 = ds.Tables[ds.DatabaseName, "", "Author"];

            Assert.IsTrue(t1.Columns.Count == 2);
            Assert.IsTrue(t1.Columns["ID"].DataType.TypeName == "bigint");

            // Test cache
            Assert.AreEqual(t1.Columns, ds.Tables[ds.DatabaseName, "", "Author"].Columns);
            Assert.AreEqual(t1.Columns["ID"], ds.Tables[ds.DatabaseName, "", "Author"].Columns["ID"]);
        }

        [TestMethod]
        public void GetTableIndexesTest()
        {
            var ds = CreateTestDataset();

            // Get a single table
            var t1 = ds.Tables[ds.DatabaseName, "", "Author"];

            Assert.IsTrue(t1.Indexes.Count == 1);
            //Assert.IsTrue(t1.Indexes["PK_Author"].IsPrimaryKey);  // Can't tell for PG

            // Test cache
            Assert.AreEqual(t1.Indexes, ds.Tables[ds.DatabaseName, "", "Author"].Indexes);
            Assert.AreEqual(t1.Indexes["PK_Author"], ds.Tables[ds.DatabaseName, "", "Author"].Indexes["PK_Author"]);

            // Table with two indices
            var t2 = ds.Tables[ds.DatabaseName, "", "TableWithIndexes"];
            Assert.AreEqual(2, t2.Indexes.Count);
        }

        [TestMethod]
        public void GetTableIndexColumnsTest()
        {
            var ds = CreateTestDataset();

            // Get a single table
            var ic = ds.Tables[ds.DatabaseName, "", "Author"].Indexes["PK_Author"].Columns["ID"];
            Assert.IsTrue(ic.Ordering == IndexColumnOrdering.Ascending);

            // Test cache
            Assert.AreEqual(ic, ds.Tables[ds.DatabaseName, "", "Author"].Indexes["PK_Author"].Columns["ID"]);

            // Non-primary key
            // TODO: postgre doesn't offer a way to query (non-PK) index columns
            //var ic2 = ds.Tables[ds.DatabaseName, "", "TableWithIndexes"].Indexes["IX_TableWithIndexes"].Columns["Data1"];
        }

        /* Statistics not implemented for MySQL
        [TestMethod]
        public void TableStatisticsTest()
        {
            
        }
         * */

        #endregion
        #region View tests

        [TestMethod]
        public void GetViewTest()
        {
            var ds = CreateTestDataset();

            var v = ds.Views[ds.DatabaseName, "public", "ViewWithStar"];

            Assert.AreEqual(1, ds.Views.Count);

            Assert.AreEqual(4, v.Columns.Count);
            Assert.AreEqual("int", v.Columns["ID"].DataType.TypeName);
        }

        [TestMethod]
        public void LoadAllViewsTest()
        {
            var ds = CreateTestDataset();
            ds.Views.LoadAll();

            Assert.AreEqual(5, ds.Views.Count);    // Update this if test database schema changes
            Assert.IsTrue(ds.Views.IsAllLoaded);
        }

        [TestMethod]
        public void GetNonexistentViewTest()
        {
            var ds = CreateTestDataset();

            try
            {
                var t = ds.Views[ds.DatabaseName, "public", "NonExistentView"];
                Assert.Fail();
            }
            catch (KeyNotFoundException)
            {
            }
        }

        #endregion
        #region Scalar function tests

        [TestMethod]
        public void GetStoredProcedureTest()
        {
            var ds = CreateTestDataset();

            var f = ds.StoredProcedures[ds.DatabaseName, "", "ScalarFunction"];

            Assert.AreEqual(2, f.Parameters.Count);
        }

        [TestMethod]
        public void LoadAllStoredProceduresTest()
        {
            var ds = CreateTestDataset();

            ds.StoredProcedures.LoadAll();
            Assert.AreEqual(2, ds.StoredProcedures.Count);    // Update this if test database schema changes
        }

        [TestMethod]
        public void GetNonexistentStoredProcedureTest()
        {
            var ds = CreateTestDataset();

            try
            {
                var t = ds.StoredProcedures[ds.DatabaseName, "", "NonExistentFunction"];
                Assert.Fail();
            }
            catch (KeyNotFoundException)
            {
            }
        }

        #endregion

#if false
       

        [TestMethod]
        public void MetaObjectsTest()
        {
            PostgreSqlDataset target = CreateTarget();

            Table t1 = target.Tables["GraywulfSchemaTest", "", "author"];
            Assert.IsTrue(t1.Metadata.Summary == "this is my own table comment");

            Table t2 = target.Tables["GraywulfSchemaTest", "", "book"];
            Assert.IsTrue(t2.Metadata.Summary == "");
        }

        [TestMethod]
        public void MetaColumnsTest()
        {
            PostgreSqlDataset target = CreateTarget();

            Table t1 = target.Tables["GraywulfSchemaTest", "public", "book"];
            Column c1 = t1.Columns["id"];
            Assert.IsTrue(c1.Metadata.Summary == "id of user");
        }

        [TestMethod]
        public void MetaParametersTest()
        {
            PostgreSqlDataset target = CreateTarget();

            StoredProcedure sp = target.StoredProcedures["GraywulfSchemaTest", "public", "sptest"];
            Parameter p = sp.Parameters["hello"];
            Assert.AreEqual(p.Metadata.Summary, "spTestComment");
            Assert.AreNotEqual(sp.Parameters["hello"], "");
        }

        [TestMethod]
        public void TableColumnsDataTypeTest()
        {
            PostgreSqlDataset target = CreateTarget();

            //Get a single table
            Table t1 = target.Tables["GraywulfSchemaTest", "", "sampledata"];

            Assert.IsTrue(t1.Columns.Count == 49);
            Assert.IsTrue(t1.Columns["column_smallint"].DataType.Name == "smallint");
            Assert.IsTrue(t1.Columns["column_integer"].DataType.Name == "int");
            Assert.IsTrue(t1.Columns["column_bigint"].DataType.Name == "bigint");
            Assert.IsTrue(t1.Columns["column_int"].DataType.Name == "int");
            Assert.IsTrue(t1.Columns["column_decimal"].DataType.Name == "numeric");
            Assert.IsTrue(t1.Columns["column_numeric"].DataType.Name == "numeric");
            Assert.IsTrue(t1.Columns["column_real"].DataType.Name == "real");
            Assert.IsTrue(t1.Columns["column_doubleprecision"].DataType.Name == "real");
            Assert.IsTrue(t1.Columns["column_smallserial"].DataType.Name == "smallint");
            Assert.IsTrue(t1.Columns["column_serial"].DataType.Name == "int");
            Assert.IsTrue(t1.Columns["column_bigserial"].DataType.Name == "bigint");
            Assert.IsTrue(t1.Columns["column_money"].DataType.Name == "money");
            Assert.IsTrue(t1.Columns["column_charactervarying"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_varchar"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_character"].DataType.Name == "char");
            Assert.IsTrue(t1.Columns["column_char"].DataType.Name == "char");
            Assert.IsTrue(t1.Columns["column_text"].DataType.Name == "text");
            Assert.IsTrue(t1.Columns["column_bytea"].DataType.Name == "binary");
            Assert.IsTrue(t1.Columns["column_timestamp"].DataType.Name == "timestamp");
            Assert.IsTrue(t1.Columns["column_timestampwithtimezone"].DataType.Name == "timestamp");
            Assert.IsTrue(t1.Columns["column_date"].DataType.Name == "date");
            Assert.IsTrue(t1.Columns["column_time"].DataType.Name == "datetime");
            //Assert.IsTrue(t1.Columns["column_timewithtimezone"].DataType.Name == "datetime");
            Assert.IsTrue(t1.Columns["column_interval"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_bool"].DataType.Name == "bit");
            Assert.IsTrue(t1.Columns["column_point"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_line"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_lseg"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_box"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_path"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_polygon"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_circle"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_cidr"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_inet"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_macaddr"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_bit"].DataType.Name == "bit");
            Assert.IsTrue(t1.Columns["column_bitvarying"].DataType.Name == "bit");
            Assert.IsTrue(t1.Columns["column_tsvector"].DataType.Name == "varchar");
            Assert.IsTrue(t1.Columns["column_xml"].DataType.Name == "xml");
            Assert.IsTrue(t1.Columns["column_json"].DataType.Name == "text");
            Assert.IsTrue(t1.Columns["column_arrayinteger"].DataType.Name == "unknown");
            Assert.IsTrue(t1.Columns["column_int4range"].DataType.Name == "int");
            Assert.IsTrue(t1.Columns["column_int8range"].DataType.Name == "bigint");
            Assert.IsTrue(t1.Columns["column_numrange"].DataType.Name == "numeric");
            Assert.IsTrue(t1.Columns["column_tsrange"].DataType.Name == "datetime");
            Assert.IsTrue(t1.Columns["column_tstzrange"].DataType.Name == "datetime");
            Assert.IsTrue(t1.Columns["column_daterange"].DataType.Name == "date");
            Assert.IsTrue(t1.Columns["column_oid"].DataType.Name == "varchar");

            //Test cache
            Assert.AreEqual(t1.Columns, target.Tables["GraywulfSchemaTest", "", "sampledata"].Columns);
            Assert.AreEqual(t1.Columns["column_smallint"], target.Tables["GraywulfSchemaTest", "", "sampledata"].Columns["column_smallint"]);
        }
#endif
    }
}
