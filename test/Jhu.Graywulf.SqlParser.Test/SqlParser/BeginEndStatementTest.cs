﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Jhu.Graywulf.Sql.Parsing
{
    [TestClass]
    public class BeginEndStatementTest
    {
        private StatementBlock Parse(string query)
        {
            var p = new SqlParser();
            return (StatementBlock)p.Execute(new StatementBlock(), query);
        }

        [TestMethod]
        public void SimpleBeginTest()
        {
            var sql =
@"
DECLARE @a int

BEGIN
    DECLARE @b int
END
";
            var sb = Parse(sql);
        }

        [TestMethod]
        public void NestedBeginEndTest()
        {
            var sql =
@"
DECLARE @a int

BEGIN
    BEGIN
        DECLARE @b int
    END
    BEGIN BEGIN END END
END
";
            var sb = Parse(sql);
        }

    }
}
