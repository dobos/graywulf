﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jhu.Graywulf.Sql.Parsing
{
    public class SqlQueryVisitorOptions
    {
        #region Private member variables

        private ExpressionTraversalMode expressionTraversal;
        private ExpressionTraversalMode booleanExpressionTraversal;
        private bool visitExpressionSubqueries;
        private bool visitSchemaReferences;

        #endregion
        #region Properties

        public ExpressionTraversalMode ExpressionTraversal
        {
            get { return expressionTraversal; }
            set { expressionTraversal = value; }
        }

        public ExpressionTraversalMode BooleanExpressionTraversal
        {
            get { return booleanExpressionTraversal; }
            set { booleanExpressionTraversal = value; }
        }

        public bool VisitExpressionSubqueries
        {
            get { return visitExpressionSubqueries; }
            set { visitExpressionSubqueries = value; }
        }

        public bool VisitSchemaReferences
        {
            get { return visitSchemaReferences; }
            set { visitSchemaReferences = value; }
        }

        #endregion
        #region Constructors and initializers

        public SqlQueryVisitorOptions()
        {
            InitializeMembers();
        }

        public SqlQueryVisitorOptions(SqlQueryVisitorOptions old)
        {
            CopyMembers(old);
        }

        private void InitializeMembers()
        {
            this.expressionTraversal = ExpressionTraversalMode.Infix;
            this.booleanExpressionTraversal = ExpressionTraversalMode.Infix;
            this.visitExpressionSubqueries = true;
            this.visitSchemaReferences = false;
        }

        private void CopyMembers(SqlQueryVisitorOptions old)
        {
            this.expressionTraversal = old.expressionTraversal;
            this.booleanExpressionTraversal = old.booleanExpressionTraversal;
            this.visitExpressionSubqueries = old.visitExpressionSubqueries;
            this.visitSchemaReferences = old.visitSchemaReferences;
        }

        #endregion
    }
}
