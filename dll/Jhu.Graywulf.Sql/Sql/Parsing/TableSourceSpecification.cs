﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Parsing;
using Jhu.Graywulf.Sql.NameResolution;

namespace Jhu.Graywulf.Sql.Parsing
{
    public partial class TableSourceSpecification : ITableReference
    {
        public virtual TableSource SpecificTableSource
        {
            get { return FindDescendant<TableSource>(); }
        }

        public DatabaseObjectReference DatabaseObjectReference
        {
            get { return SpecificTableSource.TableReference; }
        }

        public virtual TableReference TableReference
        {
            get { return SpecificTableSource.TableReference; }
            set { SpecificTableSource.TableReference = value; }
        }

        public QuerySpecification QuerySpecification
        {
            get { return FindAscendant<QuerySpecification>(); }
        }

        public static TableSourceSpecification Create(SimpleTableSource sts)
        {
            var ts = new TableSourceSpecification();
            ts.Stack.AddLast(sts);
            return ts;
        }

        public static TableSourceSpecification Create(FunctionTableSource fts)
        {
            var ts = new TableSourceSpecification();
            ts.Stack.AddLast(fts);
            return ts;
        }

        public static TableSourceSpecification Create(ComputedTableSource ts)
        {
            var res = new TableSourceSpecification();
            res.Stack.AddLast(ts);
            return res;
        }

        public static TableSourceSpecification Create(TableReference tr)
        {
            var ts = new TableSourceSpecification();
            var sts = SimpleTableSource.Create(tr);
            ts.Stack.AddLast(sts);
            return ts;
        }
    }
}
