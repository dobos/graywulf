﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Parsing;
using Jhu.Graywulf.Sql.NameResolution;

namespace Jhu.Graywulf.Sql.Parsing
{
    public partial class FunctionTableSource : ITableSource
    {
        public TableValuedFunctionCall FunctionCall
        {
            get { return FindDescendant<TableValuedFunctionCall>(); }
        }

        public DatabaseObjectReference DatabaseObjectReference
        {
            get { return FunctionCall.TableReference; }
        }

        public TableReference TableReference
        {
            get { return FunctionCall.TableReference; }
            set { FunctionCall.TableReference = value; }
        }

        public bool IsSubquery
        {
            get { return false; }
        }

        public bool IsMultiTable
        {
            get { return false; }
        }

        public static FunctionTableSource Create(TableValuedFunctionCall functionCall, string tableAlias)
        {
            var fts = new FunctionTableSource();

            fts.Stack.AddLast(functionCall);
            fts.Stack.AddLast(Whitespace.Create());
            fts.Stack.AddLast(Keyword.Create("AS"));
            fts.Stack.AddLast(Whitespace.Create());
            fts.Stack.AddLast(TableAlias.Create(tableAlias));

            functionCall.TableReference.Alias = tableAlias;

            return fts;
        }

        public override void Interpret()
        {
            base.Interpret();

            TableReference.InterpretTableSource(this);
        }

        public IEnumerable<ITableSource> EnumerateSubqueryTableSources(bool recursive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITableSource> EnumerateMultiTableSources()
        {
            throw new NotImplementedException();
        }
    }
}
