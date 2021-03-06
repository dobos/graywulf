﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using Jhu.Graywulf.Components;

namespace Jhu.Graywulf.Sql.Schema
{
    /// <summary>
    /// Reflects a table-valued function
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "")]
    public class TableValuedFunction : DatabaseObject, IColumns, IParameters, ICloneable
    {
        [NonSerialized]
        private LazyProperty<ConcurrentDictionary<string, Column>> columns;

        [NonSerialized]
        private LazyProperty<QuantityIndex> quantities;

        [NonSerialized]
        private LazyProperty<ConcurrentDictionary<string, Parameter>> parameters;

        /// <summary>
        /// Gets or sets the name of the table-valued function
        /// </summary>
        [IgnoreDataMember]
        public string FunctionName
        {
            get { return ObjectName; }
            set { ObjectName = value; }
        }

        /// <summary>
        /// Gets the column collection
        /// </summary>
        [IgnoreDataMember]
        public ConcurrentDictionary<string, Column> Columns
        {
            get { return columns.Value; }
        }


        /// <summary>
        /// Gets or sets the quantity indexes
        /// </summary>
        [IgnoreDataMember]
        public QuantityIndex Quantities
        {
            get { return quantities.Value; }
            set { quantities.Value = value; }
        }

        /// <summary>
        /// Gets the parameter collection
        /// </summary>
        [IgnoreDataMember]
        public ConcurrentDictionary<string, Parameter> Parameters
        {
            get { return parameters.Value; }
        }

        #region Constructors and initializers

        /// <summary>
        /// Default constructor
        /// </summary>
        public TableValuedFunction()
            : base()
        {
            InitializeMembers(new StreamingContext());
        }

        /// <summary>
        /// Creates a table-valued function and initializes its dataset
        /// </summary>
        /// <param name="dataset"></param>
        public TableValuedFunction(DatasetBase dataset)
            : base(dataset)
        {
            InitializeMembers(new StreamingContext());
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="old"></param>
        public TableValuedFunction(TableValuedFunction old)
            : base(old)
        {
            CopyMembers(old);
        }

        /// <summary>
        /// Initializes member variables to their default values
        /// </summary>
        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
            this.ObjectType = DatabaseObjectType.TableValuedFunction;

            this.columns = new LazyProperty<ConcurrentDictionary<string, Column>>(LoadColumns);
            this.quantities = new LazyProperty<QuantityIndex>(LoadQuantities);
            this.parameters = new LazyProperty<ConcurrentDictionary<string, Parameter>>(LoadParameters);
        }

        /// <summary>
        /// Copies member variables
        /// </summary>
        /// <param name="old"></param>
        private void CopyMembers(TableValuedFunction old)
        {
            this.ObjectType = old.ObjectType;

            this.columns = new LazyProperty<ConcurrentDictionary<string, Column>>(LoadColumns);
            this.quantities = new LazyProperty<QuantityIndex>(LoadQuantities);
            this.parameters = new LazyProperty<ConcurrentDictionary<string, Parameter>>(LoadParameters);
        }

        /// <summary>
        /// Returns a copy of this table
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new TableValuedFunction(this);
        }

        protected QuantityIndex LoadQuantities()
        {
            return new QuantityIndex(Columns.Values);
        }

        #endregion
    }
}
