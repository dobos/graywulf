﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Parsing;

namespace Jhu.Graywulf.Sql.Parsing
{
    public partial class LogicalOperator
    {
        public override int Precedence
        {
            get
            {
                if (IsAnd)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }

        public override bool IsLeftAssociative
        {
            get { return false; }
        }

        public bool IsOr
        {
            get
            {
                return SqlParser.ComparerInstance.Compare(Stack.First.Value, "OR") == 0;
            }
        }

        public bool IsAnd
        {
            get
            {
                return SqlParser.ComparerInstance.Compare(Stack.First.Value, "AND") == 0;
            }
        }

        public static LogicalOperator Create(string keyword)
        {
            var lop = new LogicalOperator();

            lop.Stack.AddLast(Keyword.Create(keyword));

            return lop;
        }

        public static LogicalOperator CreateOr()
        {
            return Create("OR");
        }

        public static LogicalOperator CreateAnd()
        {
            return Create("AND");
        }

        public LogicalOperator CreateInverse()
        {
            if (IsOr)
            {
                return CreateAnd();
            }
            else
            {
                return CreateOr();
            }
        }
    }
}
