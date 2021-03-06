﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jhu.Graywulf.Parsing
{
    /// <summary>
    /// Represents a token that matches keywords
    /// </summary>
    public class Literal : Token, ICloneable
    {
        #region Private members variables

        private static readonly Regex pattern = new Regex(@"\G[a-zA-Z][a-zA-Z0-9_]*", RegexOptions.Compiled);
        private string literalText;

        #endregion
        #region Properties

        protected Regex Pattern
        {
            get { return pattern; }
        }

        protected string LiteralText
        {
            get { return literalText; }
            set { literalText = value; }
        }

        #endregion
        #region Constructors and initializers

        public Literal()
            : base()
        {
            InitializeMembers();
        }

        public Literal(Literal old)
            :base(old)
        {
            CopyMembers(old);
        }

        public Literal(string literalText)
            : this()
        {
            Value = this.literalText = literalText;
        }

        private void InitializeMembers()
        {
            this.literalText = null;
        }

        private void CopyMembers(Literal old)
        {
            this.literalText = old.literalText;
        }

        public override object Clone()
        {
            return new Literal(this);
        }

        #endregion

        public static Literal Create(string literalText)
        {
            var res = new Literal();

            res.literalText = res.Value = literalText;

            return res;
        }

        public override bool Match(Parser parser)
        {
            Match m = this.Pattern.Match(parser.Code, parser.Pos);

            if (m.Success && parser.Comparer.Compare(m.Value, literalText) == 0)
            {
                Value = m.Value;
                parser.GetLineCol(parser.Pos, out line, out col);
                parser.Advance(m.Length);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
