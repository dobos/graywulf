﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jhu.Graywulf.Parsing;
using Jhu.Graywulf.Sql.Schema;
using Jhu.Graywulf.Sql.Parsing;

namespace Jhu.Graywulf.Sql.NameResolution
{
    public class DataTypeReference : DatabaseObjectReference, IColumnReferences
    {
        #region Property storage variables

        private List<ColumnReference> columnReferences;

        #endregion
        #region Properties

        public Schema.DataType DataType
        {
            get { return (Schema.DataType)DatabaseObject; }
            set { DatabaseObject = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Never use this in query generation!
        /// </remarks>
        public override string UniqueName
        {
            get
            {
                if (!IsSystem)
                {
                    return base.UniqueName;
                }
                else
                {
                    return base.DatabaseObjectName;
                }
            }
        }

        public IList<ColumnReference> ColumnReferences
        {
            get { return columnReferences; }
        }

        #endregion
        #region Constructors and initializers

        public DataTypeReference()
        {
            InitializeMembers();
        }

        public DataTypeReference(DataTypeReference old)
        {
            CopyMembers(old);
        }

        public DataTypeReference(Schema.DataType dataType)
            :base(dataType)
        {
            InitializeMembers();
        }
        
        private void InitializeMembers()
        {
            this.columnReferences = new List<ColumnReference>();
        }

        private void CopyMembers(DataTypeReference old)
        {
            // Deep copy of column references
            this.columnReferences = new List<ColumnReference>();

            foreach (var cr in old.columnReferences)
            {
                var ncr = new ColumnReference(this, cr);
                this.columnReferences.Add(ncr);
            }
        }

        public override object Clone()
        {
            return new DataTypeReference(this);
        }

        #endregion

        public static DataTypeReference Interpret(DataTypeIdentifier di)
        {
            var mpi = di.FindDescendant<MultiPartIdentifier>();

            if (mpi.PartCount > 3)
            {
                throw NameResolutionError.DataTypeIdentifierTooManyParts(di);
            }

            var dr = new DataTypeReference()
            {
                SchemaName = RemoveIdentifierQuotes(mpi.NamePart2),
                DatabaseObjectName = RemoveIdentifierQuotes(mpi.NamePart1),
            };

            if (dr.DatasetName == null &&  dr.DatabaseName == null && dr.SchemaName == null &&
                Schema.SqlServer.Constants.SqlDataTypes.ContainsKey(dr.DatabaseObjectName))
            {
                // System type, this needs to be resolved here to have
                // access to precision, scale and length
                var sqltype = Schema.SqlServer.Constants.SqlDataTypes[dr.DatabaseObjectName];
                var dt = Schema.DataType.Create(sqltype, di.Length, di.Precision, di.Scale, false);

                dr.IsUserDefined = false;
                dr.DatabaseObject = dt;
                dr.DatabaseObjectName = dt.TypeNameWithLength;  // Needs update for system types
            }
            else
            {
                dr.IsUserDefined = true;
            }

            return dr;
        }
    }
}
