﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Jhu.Graywulf.ParserLib;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.SqlParser;

namespace Jhu.Graywulf.SqlCodeGen.SqlServer
{
    public class SqlServerCodeGenerator : SqlCodeGeneratorBase
    {
        public static string GetCode(Node node, bool resolvedNames)
        {
            var sw = new StringWriter();
            var cg = new SqlServerCodeGenerator();
            cg.ResolveNames = resolvedNames;
            cg.Execute(sw, node);
            return sw.ToString();
        }

        public SqlServerCodeGenerator()
        {
        }

        #region Identifier formatting functions

        protected override string QuoteIdentifier(string identifier)
        {
            return String.Format("[{0}]", identifier);
        }

        protected override string GetResolvedTableName(string databaseName, string schemaName, string tableName)
        {
            string res = String.Empty;

            if (!String.IsNullOrWhiteSpace(databaseName))
            {
                res += QuoteIdentifier(databaseName) + ".";
            }

            if (!String.IsNullOrWhiteSpace(schemaName))
            {
                res += QuoteIdentifier(schemaName);
            }

            // If no schema name is specified but there's a database name,
            // SQL Server uses the database..table syntax
            if (res != String.Empty)
            {
                res += ".";
            }

            res += QuoteIdentifier(tableName);

            return res;
        }

        protected override string GetResolvedFunctionName(string databaseName, string schemaName, string functionName)
        {
            string res = String.Empty;


            if (databaseName != null)
            {
                res += QuoteIdentifier(databaseName) + ".";
            }

            // SQL Server function must always have the schema name specified
            res += QuoteIdentifier(schemaName) + ".";
            res += QuoteIdentifier(functionName);

            return res;
        }

        #endregion
        #region Complete query generators

        public override string GenerateSelectStarQuery(TableOrView tableOrView, int top)
        {
            return String.Format(
                "SELECT {0} * FROM {1}",
                GenerateTopExpression(top),
                GetResolvedTableName(tableOrView.DatabaseName, tableOrView.SchemaName, tableOrView.ObjectName));
        }

        protected override string GenerateTopExpression(int top)
        {
            var topstr = String.Empty;
            if (top != 0)
            {
                topstr = String.Format("TOP {0}", top);
            }

            return topstr;
        }

        public override string GenerateMostRestrictiveTableQuery(TableReference table, bool includePrimaryKey, int top)
        {
            // Normalize search conditions and extract where clause
            var cn = new SearchConditionNormalizer();
            cn.NormalizeQuerySpecification(((TableSource)table.Node).QuerySpecification);
            var where = cn.GenerateWhereClauseSpecificToTable(table);

            // Build table specific query
            var sql = new StringWriter();

            sql.Write("SELECT ");

            if (top > 0)
            {
                sql.Write("TOP {0} ", top);
            }

            // Now write the referenced columns
            var referencedcolumns = new HashSet<string>(Jhu.Graywulf.Schema.SqlServer.SqlServerSchemaManager.Comparer);

            int q = 0;
            if (includePrimaryKey)
            {
                var t = table.DatabaseObject as Jhu.Graywulf.Schema.Table;
                foreach (var cr in t.PrimaryKey.Columns.Values)
                {
                    var columnname = String.Format(
                        "{0}.{1}",
                        QuoteIdentifier(table.Alias),
                        QuoteIdentifier(cr.ColumnName));

                    if (!referencedcolumns.Contains(columnname))
                    {
                        if (q != 0)
                        {
                            sql.Write(", ");
                        }
                        sql.Write(columnname);
                        q++;

                        referencedcolumns.Add(columnname);
                    }
                }
            }


            foreach (var cr in table.ColumnReferences.Where(c => c.IsReferenced))
            {
                var columnname = GetResolvedColumnName(cr);     // TODO: verify

                if (!referencedcolumns.Contains(columnname))
                {
                    if (q != 0)
                    {
                        sql.Write(", ");
                    }
                    sql.Write(columnname);
                    q++;

                    referencedcolumns.Add(columnname);
                }
            }

            // From cluse
            sql.Write(" FROM {0} ", GetResolvedTableName(table));
            if (!String.IsNullOrWhiteSpace(table.Alias))
            {
                sql.Write("AS {0} ", QuoteIdentifier(table.Alias));
            }

            if (where != null)
            {
                Execute(sql, where);
            }

            return sql.ToString();
        }

        public string GenerateTableStatisticsQuery(TableReference table)
        {
            if (table.Statistics == null)
            {
                throw new InvalidOperationException();
            }

            // Build table specific where clause
            var cnr = new SearchConditionNormalizer();
            cnr.NormalizeQuerySpecification(((TableSource)table.Node).QuerySpecification);
            var wh = cnr.GenerateWhereClauseSpecificToTable(table);

            var where = new StringWriter();
            if (wh != null)
            {
                var cg = new SqlServerCodeGenerator();
                cg.Execute(where, wh);
            };

            //*** TODO: move into resource
            string sql = String.Format(@"
IF OBJECT_ID('tempdb..##keys_{4}') IS NOT NULL
DROP TABLE ##keys_{4}

SELECT CAST({2} AS float) AS __key
INTO ##keys_{4}
FROM {0} {1}
{3};

DECLARE @count bigint = @@ROWCOUNT;
DECLARE @step bigint = @count / @bincount;

IF (@step = 0) SET @step = NULL;

WITH q AS
(
	SELECT __key, ROW_NUMBER() OVER (ORDER BY __key) __rn
	FROM ##keys_{4}
)
SELECT __key, __rn
FROM q
WHERE __rn % @step = 1 OR __rn = @count;

DROP TABLE ##keys_{4};
",
         GetResolvedTableName(table),
         table.Alias == null ? "" : String.Format(" AS {0} ", QuoteIdentifier(table.Alias)),
         QuoteIdentifier(table.Statistics.KeyColumn),
         where.ToString(),
         Guid.NewGuid().ToString().Replace('-', '_'));

            return sql;
        }

        #endregion
    }
}