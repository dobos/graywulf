﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Parsing;
using Jhu.Graywulf.Sql.Schema;
using Jhu.Graywulf.Sql.Parsing;

namespace Jhu.Graywulf.Sql.NameResolution
{
    public class SqlNameResolver : SqlQueryVisitorSink
    {
        #region Constants 
        private static readonly HashSet<string> SystemFunctionNames = new HashSet<string>(Schema.SchemaManager.Comparer)
        {
            "COALESCE", "NULLIF",

            "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML",

            "AVG", "MIN", "CHECKSUM_AGG", "SUM", "COUNT", "STDEV", "COUNT_BIG", "STDEVP", "GROUPING", "VAR", "GROUPING_ID", "VARP", "MAX",

            "RANK", "NTILE", "DENSE_RANK", "ROW_NUMBER",

            "CAST", "CONVERT", "PARSE", "TRY_CAST", "TRY_CONVERT", "TRY_PARSE",

            "SYSDATETIME", "SYSDATETIMEOFFSET", "SYSUTCDATETIME", "CURRENT_TIMESTAMP", "GETDATE", "GETUTCDATE", "DATENAME", "DATEPART", "DAY", "MONTH", "YEAR",
            "DATEFROMPARTS", "DATETIME2FROMPARTS", "DATETIMEFROMPARTS", "DATETIMEOFFSETFROMPARTS", "SMALLDATETIMEFROMPARTS", "TIMEFROMPARTS",
            "DATEDIFF", "DATEADD", "EOMONTH", "SWITCHOFFSET", "TODATETIMEOFFSET",

            "CHOOSE", "IIF",

            "ABS", "DEGREES", "RAND", "ACOS", "EXP", "ROUND", "ASIN", "FLOOR", "SIGN", "ATAN", "LOG", "SIN", "ATN2", "LOG10", "SQRT", "CEILING",
            "PI", "SQUARE", "COS", "POWER", "TAN", "COT", "RADIANS",

            "INDEX_COL", "APP_NAME", "INDEXKEY_PROPERTY", "APPLOCK_MODE", "INDEXPROPERTY", "APPLOCK_TEST", "ASSEMBLYPROPERTY", "OBJECT_DEFINITION",
            "COL_LENGTH", "OBJECT_ID", "COL_NAME", "OBJECT_NAME", "COLUMNPROPERTY", "OBJECT_SCHEMA_NAME", "DATABASE_PRINCIPAL_ID", "OBJECTPROPERTY",
            "DATABASEPROPERTYEX", "OBJECTPROPERTYEX", "DB_ID", "ORIGINAL_DB_NAME", "DB_NAME", "PARSENAME", "FILE_ID", "SCHEMA_ID", "FILE_IDEX",
            "SCHEMA_NAME", "FILE_NAME", "SCOPE_IDENTITY", "FILEGROUP_ID", "SERVERPROPERTY", "FILEGROUP_NAME", "STATS_DATE", "FILEGROUPPROPERTY",
            "TYPE_ID", "FILEPROPERTY", "TYPE_NAME", "FULLTEXTCATALOGPROPERTY", "TYPEPROPERTY", "FULLTEXTSERVICEPROPERTY",

            "CERTENCODED", "PWDCOMPARE", "CERTPRIVATEKEY", "PWDENCRYPT", "CURRENT_USER", "SCHEMA_ID", "DATABASE_PRINCIPAL_ID", "SCHEMA_NAME",
            "SESSION_USER", "SUSER_ID", "SUSER_SID", "HAS_PERMS_BY_NAME", "SUSER_SNAME", "IS_MEMBER", "SYSTEM_USER", "IS_ROLEMEMBER", "SUSER_NAME",
            "IS_SRVROLEMEMBER", "USER_ID", "ORIGINAL_LOGIN", "USER_NAME", "PERMISSIONS",

            "ASCII", "LTRIM", "SOUNDEX", "CHAR", "NCHAR", "SPACE", "CHARINDEX", "PATINDEX", "STR", "CONCAT", "QUOTENAME", "STUFF", "DIFFERENCE",
            "REPLACE", "SUBSTRING", "FORMAT", "REPLICATE", "UNICODE", "LEFT", "REVERSE", "UPPER", "LEN", "RIGHT", "LOWER", "RTRIM",

            "ERROR_SEVERITY", "ERROR_STATE", "FORMATMESSAGE", "GETANSINULL", "GET_FILESTREAM_TRANSACTION_CONTEXT", "HOST_ID", "BINARY_CHECKSUM",
            "HOST_NAME", "CHECKSUM", "ISNULL", "CONNECTIONPROPERTY", "ISNUMERIC", "CONTEXT_INFO", "MIN_ACTIVE_ROWVERSION", "CURRENT_REQUEST_ID",
            "NEWID", "ERROR_LINE", "NEWSEQUENTIALID", "ERROR_MESSAGE", "ROWCOUNT_BIG", "ERROR_NUMBER", "XACT_STATE", "ERROR_PROCEDURE",

            "PATINDEX", "TEXTVALID", "TEXTPTR",
        };

        private static readonly HashSet<string> SystemVariableNames = new HashSet<string>(Schema.SchemaManager.Comparer)
        {
            "ERROR", "IDENTITY", "ROWCOUNT", "FETCH_STATUS",

            "CONNECTION", "CPU_BUSY", "IDLE", "IO_BUSY", "PACK_SENT", "PACK_RECEIVED", "PACKET_ERRORS",
            "TIMETICKS", "TOTAL_ERRORS", "TOTAL_READ", "TOTAL_WRITER",
            "TRANCOUNT",
            "CURSOR_ROWS", "DATEFIRST", "DBTS", "DEF_SORTORDER_ID", "DEFAULT_LANGID",
            "FETCH_STATUS", "LANGID", "LANGUAGE", "LOCK_TIMEOUT", "MAX_CONNECTION",
            "MAX_PRECISION", "MICROSOFTVERSION", "NESTLEVEL", "OPTIONS",
            "PROCID", "REMSERVER", "SERVERNAME", "SERVICENAME", "SPID",
            "TEXTSIZE", "VERSION",

            // Custom system variables
            Constants.SystemVariableNamePartCount,
            Constants.SystemVariableNamePartId,
        };

        #endregion
        #region Private member variables

        private SqlQueryVisitor visitor;

        // The schema manager is used to resolve identifiers that are not local to the details,
        // i.e. database, table, columns etc. names
        private SchemaManager schemaManager;
        private QueryDetails details;

        private string defaultTableDatasetName;
        private string defaultFunctionDatasetName;
        private string defaultDataTypeDatasetName;
        private string defaultOutputDatasetName;

        #endregion
        #region Properties

        private SqlQueryVisitor Visitor
        {
            get { return visitor; }
        }

        /// <summary>
        /// Gets or sets the schema manager to be used by the name resolver
        /// </summary>
        public SchemaManager SchemaManager
        {
            get { return schemaManager; }
            set { schemaManager = value; }
        }

        /// <summary>
        /// Gets or sets the default dataset name to be assumed when no
        /// dataset is specified
        /// </summary>
        public string DefaultTableDatasetName
        {
            get { return defaultTableDatasetName; }
            set { defaultTableDatasetName = value; }
        }

        public string DefaultFunctionDatasetName
        {
            get { return defaultFunctionDatasetName; }
            set { defaultFunctionDatasetName = value; }
        }

        public string DefaultDataTypeDatasetName
        {
            get { return defaultDataTypeDatasetName; }
            set { defaultDataTypeDatasetName = value; }
        }

        public string DefaultOutputDatasetName
        {
            get { return defaultOutputDatasetName; }
            set { defaultOutputDatasetName = value; }
        }

        #endregion
        #region Constructors and initializers

        /// <summary>
        /// Creates a new SqlNameResolver
        /// </summary>
        public SqlNameResolver()
        {
            InitializeMembers();
        }

        /// <summary>
        /// Initializez member variables
        /// </summary>
        private void InitializeMembers()
        {
            this.visitor = new SqlQueryVisitor(this);

            this.schemaManager = null;
            this.details = null;

            this.defaultTableDatasetName = String.Empty;
            this.defaultFunctionDatasetName = String.Empty;
            this.defaultDataTypeDatasetName = String.Empty;
            this.defaultOutputDatasetName = String.Empty;
        }


        #endregion

        /// <summary>
        /// Executes the name resolution over a query
        /// </summary>
        /// <param name="selectStatement"></param>
        public QueryDetails Execute(StatementBlock statementBlock)
        {
            // SqlParser builds the parsing tree and tags many nodes with TableReference and ColumnReference objects.
            // At this point these references only contain information directly available from the query, but names are
            // not verified against the database schema.

            // A query consists of a set of query specifications combined using set operators (UNION, EXCEPT etc.)
            // Each query specification has a FROM clause with a complex table expression. A table expression is a
            // set of table sources combined with join operators. A table source can be any of the following four:
            // a table (or view), a table-valued function, a table-valued variable or a subquery.
            // The WHERE clause may contain additional semi-join criteria which contain subqueries.
            // Furthermore, certain extensions to the SQL grammar may contain one or more table sources.

            // Steps of name resolution:

            // 1. Identify all query specifications and execute name resolution on each of them
            // 2. For each query specification, substitute default values of database name, schema, etc.
            //    if null is found
            // 3. Collect column descriptions from table sources
            // 4. Resolve column aliases
            // 5. Resolve table aliases
            // 6. Assign default column aliases

            var details = new QueryDetails();
            details.ParsingTree = statementBlock;
            Execute(details);
            return details;
        }

        public void Execute(QueryDetails details)
        {
            this.details = details;
            Visitor.Execute(details.ParsingTree);
            details.IsResolved = true;
        }

        #region Identifiers

        public override void VisitSystemVariable(SystemVariable node)
        {
            ResolveScalarVariableReference(node);
        }

        public override void VisitUserVariable(UserVariable node)
        {
            if (Visitor.ColumnContext.HasFlag(ColumnContext.Expression) ||
                Visitor.ColumnContext.HasFlag(ColumnContext.SelectList))
            {
                ResolveScalarVariableReference(node);
            }
        }

        public override void VisitColumnIdentifier(ColumnIdentifier node)
        {
            ResolveColumnReference(node);
        }

        public override void VisitFunctionIdentifier(FunctionIdentifier node)
        {
            SubstituteFunctionDefaults(node);
            ResolveFunctionReference(node);
            CollectFunctionReference(node);
        }

        public override void VisitDataTypeIdentifier(DataTypeIdentifier node)
        {
            SubstituteDataTypeDefaults(node.DataTypeReference);
            ResolveDataTypeReference(node.DataTypeReference);
        }

        #endregion
        #region Queries

        public override void VisitCommonTableSpecification(CommonTableSpecification cts)
        {
            Visitor.CommonTableExpression.CommonTableReferences.Add(cts.TableReference.Alias, cts.TableReference);
        }

        public override void VisitQuerySpecification(QuerySpecification qs)
        {
            ResolveResultsTableReference(qs);
        }

        public override void VisitVariableTableSource(VariableTableSource node)
        {
            ResolveTableVariableReference(node);
        }

        public override void VisitFunctionTableSource(FunctionTableSource node)
        {
            // Branch landing here:
            // - SELECT ... FROM

            var sourceTableCollection =
                Visitor.ParentQuerySpecification as ISourceTableCollection ??
                Visitor.ParentStatement as ISourceTableCollection;

            var targetTableProvider =
                Visitor.ParentStatement as ITargetTableProvider;

            node.TableReference.TableContext |= Visitor.TableContext;

            // SELECT ... FROM ...()
            SubstituteFunctionDefaults(node);
            ResolveFunctionReference(node);
            CollectFunctionReference(node);
            CollectSourceTableReference(sourceTableCollection, node);
        }

        public override void VisitSubqueryTableSource(SubqueryTableSource node)
        {
            ResolveTableReference(node, null);
        }

        public override void VisitTableOrViewIdentifier(TableOrViewIdentifier node)
        {
            ResolveTableReference(node, null);
        }

        protected void ResolveTableReference(ITableReference node, TableDefinition td)
        {
            var sourceTableCollection =
                Visitor.ParentQuerySpecification as ISourceTableCollection ??
                Visitor.ParentStatement as ISourceTableCollection;

            var targetTableProvider =
                Visitor.ParentStatement as ITargetTableProvider;

            // Set it on the original reference, later will be set on the resolved one too
            node.TableReference.TableContext |= Visitor.TableContext;

            if ((Visitor.TableContext & TableContext.Output) != 0)
            {
                // SELECT INTO or CREATE TABLE
                // Table is resolved in dedicated callback
                SubstituteOutputTableDefaults(node);

                if (td != null)
                {
                    // TODO: do we know the columns of the SELECT INTO here?
                    // names are known, but types are not
                    ResolveTableDefinition(td, node);
                }
                else
                {
                    ResolveOutputTableReference(Visitor.ParentQuerySpecification, node);
                }

                CollectOutputTableReference(targetTableProvider, node);
            }
            else if ((Visitor.TableContext & TableContext.Target) != 0)
            {
                // INSERT, UPDATE, DELETE, etc.
                SubstituteSourceTableDefaults(sourceTableCollection, node);
                ResolveSourceTableReference(sourceTableCollection, node);
                CollectTargetTableReference(targetTableProvider, node);
            }
            else if ((Visitor.TableContext & TableContext.Subquery) != 0)
            {
                // SELECT ... FROM (SELECT ...) sq
                // Everything is resolved, just copy to source tables
                CollectSourceTableReference(sourceTableCollection, node);
            }
            else if ((Visitor.TableContext & TableContext.From) != 0)
            {
                // SELECT ... FROM
                SubstituteSourceTableDefaults(sourceTableCollection, node);
                ResolveSourceTableReference(sourceTableCollection, node);
                CollectSourceTableReference(sourceTableCollection, node);
            }
            else
            {
                // Source table (table.* syntax only)
                SubstituteSourceTableDefaults(sourceTableCollection, node);
                ResolveColumnTableReference(sourceTableCollection, node);
            }

            // Set context on the resolved table also
            node.TableReference.TableContext |= Visitor.TableContext;
        }

        #endregion
        #region DML statements



        #endregion
        #region Scalar and table-valued variables

        public override void VisitVariableDeclaration(VariableDeclaration node)
        {
            var vr = node.VariableReference;
            var variable = CreateVariable(vr);

            CollectVariableReference(node);
        }

        public override void VisitTableDeclaration(TableDeclaration node)
        {
            var vr = node.Variable.VariableReference;
            var dr = vr.DataTypeReference;

            ResolveTableDefinition(node.TableDefinition, dr);

            vr.Variable = CreateVariable(vr);
            dr.DataType = CreateDataType(dr);
            CollectVariableReference(node);
        }

        public override void VisitCreateTableStatement(CreateTableStatement node)
        {
            var tr = node.TargetTable.TableReference;
            var td = node.TableDefinition;
            ResolveTableReference(node, td);
        }

        private void ResolveOutputTableReference(QuerySpecification qs, ITableReference node)
        {
            var tr = node.TableReference;
            tr.CopyColumnReferences(qs.ResultsTableReference.ColumnReferences);
            tr.DatabaseObject = CreateTable(tr);
        }

        private void ResolveTableDefinition(TableDefinition td, ITableReference node)
        {
            var tr = node.TableReference;

            foreach (var item in td.TableDefinitionList.EnumerateTableDefinitionItems())
            {
                var cd = item.ColumnDefinition;
                var tc = item.TableConstraint;
                var ti = item.TableIndex;

                if (cd != null)
                {
                    var ncr = new ColumnReference(tr, null, cd.ColumnReference, cd.DataTypeIdentifier.DataTypeReference);
                    if (tr != null)
                    {
                        tr.ColumnReferences.Add(ncr);
                    }
                }

                if (tc != null)
                {
                    // TODO
                }

                if (ti != null)
                {
                    // TODO
                }
            }

            tr.DatabaseObject = CreateTable(tr);
        }

        private void ResolveTableDefinition(TableDefinition td, DataTypeReference dr)
        {
            foreach (var item in td.TableDefinitionList.EnumerateTableDefinitionItems())
            {
                var cd = item.ColumnDefinition;

                if (cd != null)
                {
                    var ncr = new ColumnReference(null, dr, cd.ColumnReference, cd.DataTypeIdentifier.DataTypeReference);
                    if (dr != null)
                    {
                        dr.ColumnReferences.Add(ncr);
                    }
                }
            }

            dr.DataType = CreateDataType(dr);
        }

        public override void VisitColumnDefinition(ColumnDefinition node)
        {
            node.DataTypeIdentifier.DataTypeReference.DataType.IsNullable = node.IsNullable;
        }

        public override void VisitDropTableStatement(DropTableStatement node)
        {
            var table = (Schema.Table)node.TargetTable.TableReference.DatabaseObject;

            if (!table.Dataset.Tables.TryRemove(table.UniqueKey, out var t))
            {
                throw NameResolutionError.TableDoesNotExists(node.TargetTable);
            }
        }

        public override void VisitCreateIndexStatement(CreateIndexStatement node)
        {
            // TODO maybe add index to schema?
        }

        #endregion
        #region Scalar and table-valued variables

        private Schema.Variable CreateVariable(VariableReference vr)
        {
            var variable = new Schema.Variable()
            {
                Name = vr.VariableName,
                DataType = vr.DataTypeReference.DataType,
            };

            return variable;
        }


        private DataType CreateDataType(DataTypeReference dr)
        {
            var dataType = new Schema.DataType()
            {
                IsTableType = dr.ColumnReferences.Count > 0,
                IsUserDefined = true,
            };


            foreach (var cr in dr.ColumnReferences)
            {
                var col = CreateColumn(cr);
                dataType.Columns.TryAdd(col.ColumnName, col);
            }

            // TODO:
            // indexes
            // primary key
            // metadata

            return dataType;
        }

        #endregion
        #region Table DDL


        private Table CreateTable(TableReference tr)
        {
            var ds = SchemaManager.Datasets[tr.DatasetName];

            var table = new Schema.Table(ds)
            {
                DatabaseName = tr.DatabaseName,
                SchemaName = tr.SchemaName,
                TableName = tr.DatabaseObjectName
            };

            foreach (var cr in tr.ColumnReferences)
            {
                var col = CreateColumn(cr);
                table.Columns.TryAdd(col.ColumnName, col);
            }

            // TODO:
            // indexes
            // primary key
            // metadata

            return table;
        }

        private Column CreateColumn(ColumnReference cr)
        {
            var column = new Column(cr.ColumnName, cr.DataTypeReference.DataType);
            return column;
        }

        private Table AlterTable(TableReference tr)
        {
            throw new NotImplementedException();
        }

        // TODO: add alter table here

        #endregion
        #region Reference resolution

        protected virtual bool IsSystemFunctionName(string name)
        {
            return SystemFunctionNames.Contains(name);
        }

        private void ResolveFunctionReference(IFunctionReference node)
        {
            if (!node.FunctionReference.IsUserDefined)
            {
                if (!IsSystemFunctionName(node.FunctionReference.FunctionName))
                {
                    throw NameResolutionError.UnknownFunctionName(node);
                }
            }
            else
            {
                // Check if dataset specified and make sure it's valid
                if (node.FunctionReference.DatasetName != null)
                {
                    if (!schemaManager.Datasets.ContainsKey(node.FunctionReference.DatasetName))
                    {
                        throw NameResolutionError.UnresolvableDatasetReference(node.FunctionReference);
                    }
                }

                var ds = schemaManager.Datasets[node.FunctionReference.DatasetName];

                var dbo = ds.GetObject(node.FunctionReference.DatabaseName, node.FunctionReference.SchemaName, node.FunctionReference.DatabaseObjectName);

                if (dbo == null)
                {
                    throw NameResolutionError.UnresolvableFunctionReference(node);
                }

                node.FunctionReference.DatabaseObject = dbo;
            }
        }

        private void ResolveScalarVariableReference(IVariableReference node)
        {
            var vr = node.VariableReference;
            // TODO: extend this to UDTs, including member access

            if (!vr.IsUserDefined)
            {
                var name = vr.VariableName.TrimStart('@');

                if (!SystemVariableNames.Contains(name))
                {
                    throw NameResolutionError.UnresolvableVariableReference(node);
                }
            }
            else if (details.VariableReferences.ContainsKey(vr.VariableName))
            {
                vr = node.VariableReference = details.VariableReferences[vr.VariableName];

                if (vr.VariableContext != VariableContext.Scalar)
                {
                    throw NameResolutionError.ScalarVariableExpected(node);
                }
            }
            else
            {
                throw NameResolutionError.UnresolvableVariableReference(node);
            }
        }

        private void ResolveTableVariableReference(IVariableReference node)
        {
            var vr = node.VariableReference;

            if (details.VariableReferences.ContainsKey(vr.VariableName))
            {
                vr = node.VariableReference = details.VariableReferences[vr.VariableName];

                if (vr.VariableContext != VariableContext.Table)
                {
                    throw NameResolutionError.TableVariableExpected(node);
                }
            }
            else
            {
                throw NameResolutionError.UnresolvableVariableReference(node);
            }
        }

        private void ResolveColumnReference(IColumnReference cr)
        {
            var sourceTables =
                (ISourceTableCollection)Visitor.ParentQuerySpecification ??
                (ISourceTableCollection)Visitor.ParentStatement;

            // Star columns cannot be resolved, treat them separately
            if (!cr.ColumnReference.IsResolved && !cr.ColumnReference.IsStar && !cr.ColumnReference.IsComplexExpression)
            {
                ColumnReference ncr = null;
                int q = 0;

                if (cr.ColumnReference.TableReference == null || cr.ColumnReference.TableReference.IsUndefined)
                {
                    // This has an empty table reference (only column name specified),
                    // or column is referenced by a multi-part identifier which needs to be resolved now

                    // Look for a match based on column name only
                    foreach (var tr in sourceTables.ResolvedSourceTableReferences.Values)
                    {
                        foreach (var ccr in tr.ColumnReferences)
                        {
                            if (cr.ColumnReference.TryMatch(tr, ccr))
                            {
                                if (q != 0)
                                {
                                    throw NameResolutionError.AmbigousColumnReference(cr);
                                }

                                ncr = ccr;
                                q++;
                            }
                        }
                    }
                }
                else
                {
                    // This has a table reference already so only check
                    // columns of that particular table
                    foreach (var ccr in cr.ColumnReference.TableReference.ColumnReferences)
                    {
                        if (cr.ColumnReference.TryMatch(cr.ColumnReference.TableReference, ccr))
                        {
                            if (q != 0)
                            {
                                throw NameResolutionError.AmbigousColumnReference(cr);
                            }

                            ncr = ccr;
                            q++;
                        }
                    }
                }

                if (q == 0)
                {
                    if (Visitor.ColumnContext.HasFlag(ColumnContext.Insert))
                    {
                        throw NameResolutionError.ColumnNotPartOfTargetTable((Node)cr);
                    }
                    else
                    {
                        throw NameResolutionError.UnresolvableColumnReference(cr);
                    }
                }

                UpdateColumnReference(cr.ColumnReference, ncr, Visitor.ColumnContext);
            }
        }

        private void UpdateColumnReference(ColumnReference cr, ColumnReference ncr, ColumnContext context)
        {
            // Update column context of the referenced column
            ncr.ColumnContext |= context;

            cr.IsResolved = true;
            cr.TableReference = ncr.TableReference;
            cr.ParentDataTypeReference = ncr.ParentDataTypeReference;

            if (cr.IsMultiPartIdentifier)
            {
                cr.ColumnName = cr.NameParts[cr.ColumnNamePartIndex];
            }
        }

        #endregion





        #region Default substitution logic

        /// <summary>
        /// Substitutes table defaults for all tables that are supposed to exist during query execution
        /// </summary>
        /// <param name="resolvedSourceTables"></param>
        /// <param name="tr"></param>
        protected void SubstituteSourceTableDefaults(ISourceTableCollection resolvedSourceTables, ITableReference node)
        {
            var tr = node.TableReference;

            try
            {
                if (tr.IsPossiblyAlias &&
                    (Visitor.CommonTableExpression != null && Visitor.CommonTableExpression.CommonTableReferences.ContainsKey(tr.DatabaseObjectName) ||
                     resolvedSourceTables != null && resolvedSourceTables.ResolvedSourceTableReferences.ContainsKey(tr.DatabaseObjectName)))
                {
                    // Don't do any substitution if referencing a common table or anything that aliased
                }
                else if (tr.TableContext.HasFlag(TableContext.UserDefinedFunction))
                {
                    tr.SubstituteDefaults(SchemaManager, defaultFunctionDatasetName);
                }
                else if (tr.TableContext.HasFlag(TableContext.Variable))
                {
                    // No substitution
                }
                else
                {
                    tr.SubstituteDefaults(SchemaManager, defaultTableDatasetName);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw NameResolutionError.UnresolvableDatasetReference(ex, tr);
            }
        }

        /// <summary>
        /// Substitutes table default for tables that are created during query execution 
        /// </summary>
        /// <param name="tr"></param>
        private void SubstituteOutputTableDefaults(ITableReference node)
        {
            try
            {
                node.TableReference.SubstituteDefaults(SchemaManager, defaultOutputDatasetName);
            }
            catch (KeyNotFoundException ex)
            {
                throw NameResolutionError.UnresolvableDatasetReference(ex, node.TableReference);
            }
        }

        /// <summary>
        /// Substitutes dataset and schema defaults into function references.
        /// </summary>
        /// <remarks>
        /// This is non-standard SQL as SQL requires the schema name to be specified and the database is
        /// always taken from the current context. In applications, like SkyQuery, functions are always
        /// taken from the CODE database.
        /// </remarks>
        /// <param name="node"></param>
        private void SubstituteFunctionDefaults(IFunctionReference node)
        {
            if (!node.FunctionReference.IsSystem)
            {
                try
                {
                    node.FunctionReference.SubstituteDefaults(SchemaManager, defaultFunctionDatasetName);
                }
                catch (KeyNotFoundException ex)
                {
                    throw NameResolutionError.UnresolvableDatasetReference(ex, node.FunctionReference);
                }
            }
        }

        private void SubstituteDataTypeDefaults(DataTypeReference dr)
        {
            if (!dr.IsSystem)
            {
                try
                {
                    dr.SubstituteDefaults(SchemaManager, defaultDataTypeDatasetName);
                }
                catch (KeyNotFoundException ex)
                {
                    throw NameResolutionError.UnresolvableDatasetReference(ex, dr);
                }
            }
        }

        #endregion
        #region Identifier resolution logic

        /// <summary>
        /// Looks up the table among CTE and table sources and direct schema objects
        /// </summary>
        /// <param name="cte"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        public void ResolveSourceTableReference(ISourceTableCollection resolvedSourceTables, ITableReference node)
        {
            TableReference tr = node.TableReference;
            TableReference ntr;

            if (tr.TableContext.HasFlag(TableContext.Variable))
            {
                tr.VariableReference = details.VariableReferences[tr.VariableName];
                tr.LoadColumnReferences(null);
                ntr = tr;
            }
            else if (tr.TableContext.HasFlag(TableContext.Subquery))
            {
                ntr = tr;
                ntr.TableContext |= TableContext.Subquery;
            }
            else if (tr.IsPossiblyAlias &&
                Visitor.CommonTableExpression != null &&
                Visitor.CommonTableExpression.CommonTableReferences.ContainsKey(tr.TableName))
            {
                // This is a reference to a CTE query
                ntr = Visitor.CommonTableExpression.CommonTableReferences[tr.DatabaseObjectName];
            }
            else if (tr.IsPossiblyAlias && resolvedSourceTables.ResolvedSourceTableReferences.ContainsKey(tr.TableName))
            {
                // This a reference from a target table to an already resolved source table
                // This happens with UPDATE etc.
                ntr = resolvedSourceTables.ResolvedSourceTableReferences[tr.TableName];
            }
            else if (!tr.IsComputed)
            {
                // This is a direct reference to a table or a view but not to a function or subquery
                ntr = tr;

                // Load table description from underlying schema
                // Attempt to load dataset and throw exception of name cannot be resolved
                DatasetBase ds;

                try
                {
                    ds = schemaManager.Datasets[ntr.DatasetName];
                }
                catch (KeyNotFoundException ex)
                {
                    throw NameResolutionError.UnresolvableDatasetReference(ex, ntr);
                }
                catch (SchemaException ex)
                {
                    throw NameResolutionError.UnresolvableDatasetReference(ex, ntr);
                }

                ntr.DatabaseObject = ds.GetObject(ntr.DatabaseName, ntr.SchemaName, ntr.DatabaseObjectName);

                if (ntr.DatabaseObject is TableOrView)
                {
                    ntr.TableContext |= TableContext.TableOrView;
                }
                
                // Load column descriptions for the table
                ntr.LoadColumnReferences(schemaManager);
            }
            else
            {
                throw new NotImplementedException();

                // TODO: this is not necessary here
                // This is a reference to a subquery with an obligatory alias
                ntr = new TableReference(tr);
            }

            ntr.IsResolved = true;

            node.TableReference = ntr;
        }

#if false
        /* TODO: delete
        private void ResolveOutputTableReference(TableReference tr)
        {
            DatasetBase ds;

            try
            {
                ds = schemaManager.Datasets[tr.DatasetName];
            }
            catch (KeyNotFoundException ex)
            {
                throw NameResolutionError.UnresolvableDatasetReference(ex, tr);
            }
            catch (SchemaException ex)
            {
                throw NameResolutionError.UnresolvableDatasetReference(ex, tr);
            }

            /* TODO: move to query validation
            if (!ds.IsMutable)
            {
                throw NameResolutionError.TargetDatasetReadOnly((ITableReference)tr.Node);
            }
            */

        tr.DatabaseObject = ds.GetObject(tr.DatabaseName, tr.SchemaName, tr.DatabaseObjectName);

            if (tr.DatabaseObject == null)
            {
                tr.DatabaseObject = new Table(ds)
                {
                    DatabaseName = tr.DatabaseName ?? ds.DatabaseName,
                    SchemaName = tr.SchemaName ?? ds.DefaultSchemaName,
                    TableName = tr.DatabaseObjectName,
                };
            }
            else
            {
                tr.LoadColumnReferences(schemaManager);
            }
        }
#endif

        /// <summary>
        /// Resolves a table reference to a table listed in SourceTableReferences
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="tr"></param>
        private void ResolveColumnTableReference(ISourceTableCollection resolvedSourceTables, ITableReference node)
        {
            // Try to resolve the table alias part of a table reference
            // If and alias or table name is specified, this can be done based on
            // the already collected table sources.
            // If no table or alias is specified and the current node is a column reference,
            // where the column is not a complex expression, resolution might be successful by
            // column name only.

            // TODO: add support for variables

            if (node.TableReference != null && !node.TableReference.IsUndefined && !node.TableReference.IsResolved)
            {
                TableReference ntr = null;
                string alias = null;

                if (node.TableReference.Alias != null)
                {
                    // if table alias found explicitly
                    alias = node.TableReference.Alias;
                }
                else if (node.TableReference.DatasetName == null &&
                        node.TableReference.DatabaseName == null &&
                        node.TableReference.SchemaName == null &&
                        node.TableReference.DatabaseObjectName != null &&
                        resolvedSourceTables.ResolvedSourceTableReferences.ContainsKey(node.TableReference.DatabaseObjectName))
                {
                    // if only table name found and that's an alias
                    alias = node.TableReference.DatabaseObjectName;
                }

                if (alias != null)
                {
                    ntr = resolvedSourceTables.ResolvedSourceTableReferences[alias];
                }
                else
                {
                    // Check if dataset specified and make sure it's valid
                    if (node.TableReference.DatasetName != null)
                    {
                        if (!schemaManager.Datasets.ContainsKey(node.TableReference.DatasetName))
                        {
                            throw NameResolutionError.UnresolvableDatasetReference(node.TableReference);
                        }
                    }

                    // if only a table name found and that's not an alias -> must be a table
                    int q = 0;
                    foreach (var tr in resolvedSourceTables.ResolvedSourceTableReferences.Values)
                    {
                        if (tr.TryMatch(node.TableReference))
                        {
                            if (q != 0)
                            {
                                throw NameResolutionError.AmbigousTableReference(node);
                            }

                            ntr = tr;
                            q++;
                        }
                    }
                }

                if (ntr == null)
                {
                    throw NameResolutionError.UnresolvableTableReference(node);
                }

                ntr.TableContext |= Visitor.TableContext;
                ntr.IsResolved = true;

                node.TableReference = ntr;
            }

            // If we are inside a table hint, make sure the reference is to the current table
            if (Visitor.ColumnContext == ColumnContext.Hint)
            {
                // In this case a column reference appears inside a table hint (WITH clause)
                // If the table reference is undefined it must refer to the table itself
                // otherwise we must make sure it is indeed referencing the table itself

                var ts = ((Node)node).FindAscendant<SimpleTableSource>();

                if (node.TableReference.IsUndefined)
                {
                    node.TableReference = ts.TableReference;
                }
                else if (node.TableReference != ts.TableReference)
                {
                    throw CreateException(ExceptionMessages.DifferentTableReferenceInHintNotAllowed, (Node)node);
                }
            }
        }

        private void ResolveDataTypeReference(DataTypeReference dr)
        {
            if (!dr.IsResolved && dr.IsUserDefined)
            {
                // Load table description from underlying schema
                // Attempt to load dataset and throw exception of name cannot be resolved
                DatasetBase ds;

                try
                {
                    ds = schemaManager.Datasets[dr.DatasetName];
                }
                catch (KeyNotFoundException ex)
                {
                    throw NameResolutionError.UnresolvableDatasetReference(ex, dr);
                }
                catch (SchemaException ex)
                {
                    throw NameResolutionError.UnresolvableDatasetReference(ex, dr);
                }

                // Because this is the base type only, create a copy here since
                // properties like IsNullable will be overwritten later
                var dt = (DataType)ds.GetObject(dr.DatabaseName, dr.SchemaName, dr.DatabaseObjectName);
                dr.DatabaseObject = new DataType(dt);

                // TODO: load data type columns if necessary
            }

            dr.IsResolved = true;
        }

        #endregion
        #region Collect tables

        private void CollectFunctionReference(IFunctionReference node)
        {
            if (node.FunctionReference.DatabaseObject != null)
            {
                var uniqueKey = node.FunctionReference.DatabaseObject.UniqueKey;

                if (!details.FunctionReferences.ContainsKey(uniqueKey))
                {
                    details.FunctionReferences.Add(uniqueKey, new List<FunctionReference>());
                }

                details.FunctionReferences[uniqueKey].Add(node.FunctionReference);
            }
        }

        private void CollectVariableReference(IVariableReference node)
        {
            var vr = node.VariableReference;

            // Add to query details
            if (!details.VariableReferences.ContainsKey(vr.VariableName))
            {
                details.VariableReferences.Add(vr.VariableName, vr);
            }
            else
            {
                throw NameResolutionError.DuplicateVariableName(vr);
            }
        }

        private void CollectSourceTableReference(ISourceTableCollection resolvedSourceTables, ITableReference node)
        {
            var tr = node.TableReference;
            string exportedName = tr.Alias ?? tr.VariableName ?? tr.TableName;

            if (exportedName == null)
            {
                throw new InvalidOperationException();
            }

            // Make sure that table key is used only once, unless it is the target table of DELETE, INSERT or UPDATE
            // where it can also appear in the FROM clause with the same name
            if (resolvedSourceTables.ResolvedSourceTableReferences.ContainsKey(exportedName) &&
            !resolvedSourceTables.ResolvedSourceTableReferences[exportedName].TableContext.HasFlag(TableContext.Target))
            {
                throw NameResolutionError.DuplicateTableAlias(exportedName, tr.Node);
            }

            if (!resolvedSourceTables.ResolvedSourceTableReferences.ContainsKey(exportedName))
            {
                // Save the table in the query specification
                resolvedSourceTables.ResolvedSourceTableReferences.Add(exportedName, tr);
            }

            if (tr.DatabaseObject is TableOrView)
            {
                // Collect in the global store
                var uniquekey = tr.DatabaseObject.UniqueKey;

                if (!details.SourceTableReferences.ContainsKey(uniquekey))
                {
                    details.SourceTableReferences.Add(uniquekey, new List<TableReference>());
                }

                details.SourceTableReferences[uniquekey].Add(tr);

                tr.TableSource.UniqueKey = String.Format("{0}_{1}_{2}", uniquekey, tr.Alias, details.SourceTableReferences[uniquekey].Count - 1);
            }
        }

        private void CollectTargetTableReference(ITargetTableProvider targetTable, ITableReference node)
        {
            // Save it to the global store
            var tr = node.TableReference;
            var uniqueKey = tr.DatabaseObject.UniqueKey;

            if (!details.TargetTableReferences.ContainsKey(uniqueKey))
            {
                details.TargetTableReferences.Add(uniqueKey, new List<TableReference>());
            }

            details.TargetTableReferences[uniqueKey].Add(tr);
        }

        private void CollectOutputTableReference(ITargetTableProvider targetTable, ITableReference node)
        {
            var tr = node.TableReference;
            var table = (Table)tr.DatabaseObject;
            var uniqueKey = table.UniqueKey;

            // Add to schema
            if (!table.Dataset.Tables.TryAdd(uniqueKey, table))
            {
                throw NameResolutionError.TableAlreadyExists(tr.Node);
            }

            // Add to query details
            if (!details.OutputTableReferences.ContainsKey(uniqueKey))
            {
                details.OutputTableReferences.Add(uniqueKey, new List<TableReference>());
            }
            else
            {
                throw NameResolutionError.DuplicateOutputTable(tr);
            }

            details.OutputTableReferences[uniqueKey].Add(tr);
        }

        protected void ResolveResultsTableReference(QuerySpecification qs)
        {
            if (Visitor.QueryContext.HasFlag(QueryContext.Subquery))
            {
                qs.ResultsTableReference.TableContext |= TableContext.Subquery;
            }

            // ColumnExpression are rendered with original table name by the code generator
            // but are referenced by outer queries by table alias. For this reason, at
            // this point we need to make a copy of all column references and update the
            // table reference
            CopyResultTableColumns(qs);
        }

        protected void CopyResultTableColumns(QuerySpecification qs)
        {
            var tr = qs.ResultsTableReference;
            int index = 0;
            foreach (var ce in qs.SelectList.EnumerateColumnExpressions())
            {
                var cr = ce.ColumnReference;

                if (cr.IsStar)
                {
                    if (cr.TableReference.IsUndefined)
                    {
                        foreach (var ts in qs.EnumerateSourceTables(false))
                        {
                            foreach (var ccr in ts.TableReference.ColumnReferences)
                            {
                                CopyResultTableColumn(ccr, tr, index++);
                            }
                        }
                    }
                    else
                    {
                        var ts = qs.ResolvedSourceTableReferences[cr.TableReference.Alias ?? cr.TableReference.TableName];
                        foreach (var ccr in ts.ColumnReferences)
                        {
                            CopyResultTableColumn(ccr, tr, index++);
                        }
                    }
                }
                else
                {
                    CopyResultTableColumn(cr, tr, index++);
                }
            }
        }

        protected ColumnReference CopyResultTableColumn(ColumnReference cr, TableReference tr, int index)
        {
            cr.ColumnContext |= ColumnContext.SelectList;
            var ncr = new ColumnReference(tr, cr)
            {
                SelectListIndex = index,
                ColumnAlias = null,
                ColumnName = cr.ColumnAlias ?? cr.ColumnName
            };
            tr.ColumnReferences.Add(ncr);
            return ncr;
        }

        #endregion

        /// <summary>
        /// Creates and parameterizes and exception to be thrown by the name resolver.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected Exception CreateException(string message, Token token)
        {
            string msg;

            msg = String.Format(message, null, token.Line + 1, token.Pos + 1);

            NameResolverException ex = new NameResolverException(msg);
            ex.Token = token;

            return ex;
        }
    }
}
