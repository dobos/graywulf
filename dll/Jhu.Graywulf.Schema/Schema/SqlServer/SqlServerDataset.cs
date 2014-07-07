﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Jhu.Graywulf.Schema;

namespace Jhu.Graywulf.Schema.SqlServer
{
    /// <summary>
    /// Implements schema reflection functions for MS SQL Server
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "")]
    public class SqlServerDataset : DatasetBase
    {
        #region Private members

        [NonSerialized]
        protected bool isOnLinkedServer;

        [NonSerialized]
        protected bool isRemoteDataset;

        #endregion
        #region Properties

        /// <summary>
        /// Gets or sets the value determining if the data is available
        /// via a linked SQL server.
        /// </summary>
        [DataMember]
        public bool IsOnLinkedServer
        {
            get { return isOnLinkedServer; }
            set { isOnLinkedServer = value; }
        }

        [IgnoreDataMember]
        public override string ProviderName
        {
            get { return Constants.SqlServerProviderName; }
        }

        /// <summary>
        /// Gets or sets the database name associated with this dataset.
        /// </summary>
        /// <remarks>
        /// The database name only refers to the schema prototype, not
        /// the actual database instances!
        /// </remarks>
        [IgnoreDataMember]
        public override string DatabaseName
        {
            get
            {
                var csb = new SqlConnectionStringBuilder(ConnectionString);
                return csb.InitialCatalog;
            }
            set
            {
                var csb = new SqlConnectionStringBuilder(ConnectionString);
                csb.InitialCatalog = value;
                ConnectionString = csb.ConnectionString;
            }
        }

        [IgnoreDataMember]
        public string Host
        {
            get
            {
                var csb = new SqlConnectionStringBuilder(ConnectionString);
                int i = csb.DataSource.IndexOf('\\');
                if (i > -1)
                {
                    return csb.DataSource.Substring(i);
                }
                else
                {
                    return csb.DataSource;
                }
            }
        }

        #endregion
        #region Constructors and initializers

        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlServerDataset()
            : base()
        {
            InitializeMembers(new StreamingContext());
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="old"></param>
        public SqlServerDataset(DatasetBase old)
            : base(old)
        {
            InitializeMembers(new StreamingContext());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        public SqlServerDataset(string name, string connectionString)
            : base()
        {
            InitializeMembers(new StreamingContext());

            Name = name;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="old"></param>
        public SqlServerDataset(SqlServerDataset old)
            : base(old)
        {
            CopyMembers(old);
        }

        /// <summary>
        /// Initializes private member variables to their default values.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
            this.DefaultSchemaName = "dbo";

            this.isOnLinkedServer = false;
            this.isRemoteDataset = false;
        }

        /// <summary>
        /// Copies private member variables form another instance
        /// </summary>
        /// <param name="old"></param>
        private void CopyMembers(SqlServerDataset old)
        {
            this.DefaultSchemaName = old.DefaultSchemaName;

            this.isOnLinkedServer = old.isOnLinkedServer;
            this.isRemoteDataset = old.isRemoteDataset;
        }

        public override object Clone()
        {
            return new SqlServerDataset(this);
        }

        #endregion
        #region Fully resolved names and keys

        /// <summary>
        /// Return a quoted version of an identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        protected override string QuoteIdentifier(string identifier)
        {
            return String.Format("[{0}]", identifier);
        }

        /// <summary>
        /// Returns the fully resolved name of a database object
        /// belonging to the dataset.
        /// </summary>
        /// <param name="databaseObject"></param>
        /// <returns></returns>
        /// <remarks>
        /// The function returns the object name as it can be referenced
        /// 
        /// </remarks>
        public override string GetObjectFullyResolvedName(DatabaseObject databaseObject)
        {
            // If schema name is empty, use default schema name

            var databaseName = GetFullyResolvedName();
            var schemaName =
                String.IsNullOrWhiteSpace(databaseObject.SchemaName) ?
                    QuoteIdentifier(DefaultSchemaName) :
                    QuoteIdentifier(databaseObject.SchemaName);
            var objectName = QuoteIdentifier(databaseObject.ObjectName);

            return String.Format("{0}.{1}.{2}", databaseName, schemaName, objectName);
        }

        /// <summary>
        /// Returns the unique string key of a database object belonging to the dataset.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="datasetName"></param>
        /// <param name="databaseName"></param>
        /// <param name="schemaName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public override string GetObjectUniqueKey(DatabaseObjectType objectType, string datasetName, string databaseName, string schemaName, string objectName)
        {
            // If schema name is empty, use default schema name
            schemaName =
                String.IsNullOrWhiteSpace(schemaName) ?
                DefaultSchemaName :
                schemaName;

            return base.GetObjectUniqueKey(objectType, datasetName, databaseName, schemaName, objectName);
        }

        #endregion
        #region Type conversion function

        // TODO: delete

        /*
        protected override DataType GetTypeFromProviderSpecificName(string name)
        {
            switch (name.ToLowerInvariant().Trim())
            {
                case Constants.TypeNameTinyInt:
                    return DataTypes.SqlTinyInt;
                case Constants.TypeNameSmallInt:
                    return DataTypes.SqlSmallInt;
                case Constants.TypeNameInt:
                    return DataTypes.SqlInt;
                case Constants.TypeNameBigInt:
                    return DataTypes.SqlBigInt;
                case Constants.TypeNameBit:
                    return DataTypes.SqlBit;
                case Constants.TypeNameDecimal:
                    return DataTypes.SqlDecimal;
                case Constants.TypeNameSmallMoney:
                    return DataTypes.SqlSmallMoney;
                case Constants.TypeNameMoney:
                    return DataTypes.SqlMoney;
                case Constants.TypeNameNumeric:
                    return DataTypes.SqlNumeric;
                case Constants.TypeNameReal:
                    return DataTypes.SqlReal;
                case Constants.TypeNameFloat:
                    return DataTypes.SqlFloat;
                case Constants.TypeNameDate:
                    return DataTypes.SqlDate;
                case Constants.TypeNameTime:
                    return DataTypes.SqlTime;
                case Constants.TypeNameSmallDateTime:
                    return DataTypes.SqlSmallDateTime;
                case Constants.TypeNameDateTime:
                    return DataTypes.SqlDateTime;
                case Constants.TypeNameDateTime2:
                    return DataTypes.SqlDateTime2;
                case Constants.TypeNameDateTimeOffset:
                    return DataTypes.SqlDateTimeOffset;
                case Constants.TypeNameChar:
                    return DataTypes.SqlChar;
                case Constants.TypeNameVarChar:
                    return DataTypes.SqlVarChar;
                case Constants.TypeNameText:
                    return DataTypes.SqlText;
                case Constants.TypeNameNChar:
                    return DataTypes.SqlNChar;
                case Constants.TypeNameNVarChar:
                    return DataTypes.SqlNVarChar;
                case Constants.TypeNameNText:
                    return DataTypes.SqlNText;
                case Constants.TypeNameXml:
                    return DataTypes.SqlXml;
                case Constants.TypeNameBinary:
                    return DataTypes.SqlBinary;
                case Constants.TypeNameVarBinary:
                    return DataTypes.SqlVarBinary;
                case Constants.TypeNameImage:
                    return DataTypes.SqlImage;
                case Constants.TypeNameSqlVariant:
                    return DataTypes.SqlVariant;
                case Constants.TypeNameTimestamp:
                    return DataTypes.SqlTimestamp;
                case Constants.TypeNameUniqueIdentifier:
                    return DataTypes.SqlUniqueIdentifier;
                default:
                    throw new ArgumentOutOfRangeException("name");
            }
        }
        */

        #endregion
        #region Schema objects

        /// <summary>
        /// Loads the schema of a database object belonging to the dataset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        protected override void LoadObject<T>(T obj)
        {
            var sql = @"
SELECT s.name, o.name, o.type
FROM sys.objects o
INNER JOIN sys.schemas s
	ON s.schema_id = o.schema_id
WHERE o.type IN ({0}) AND
    (s.name = @schemaName OR @schemaName IS NULL) AND o.name = @objectName
";

            sql = String.Format(sql, GetObjectTypeIdListString(obj.ObjectType));

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(obj.SchemaName) ? (object)DBNull.Value : (object)obj.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar, 128).Value = obj.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        int q = 0;
                        while (dr.Read())
                        {
                            obj.Dataset = this;
                            obj.DatabaseName = DatabaseName;
                            obj.SchemaName = dr.GetString(0);
                            obj.ObjectName = dr.GetString(1);
                            obj.ObjectType = Constants.SqlServerObjectTypeIds[dr.GetString(2).Trim()];

                            q++;
                        }

                        // No records
                        if (q == 0)
                        {
                            ThrowInvalidObjectNameException(obj);
                        }
                        else if (q > 1)
                        {
                            throw new SchemaException("ambigous name"); // TODO
                        }
                    }
                }
            }
        }

        internal override bool IsObjectExisting(DatabaseObject databaseObject)
        {
            var sql = String.Format(
                @"SELECT OBJECT_ID('{0}')",
                GetObjectFullyResolvedName(databaseObject));

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        /// <summary>
        /// Loads all database objects of type T belonging to the dataset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        protected override IEnumerable<KeyValuePair<string, T>> LoadAllObjects<T>(string databaseName)
        {
            var sql = @"
SELECT s.name, o.name, o.type
FROM sys.objects o
INNER JOIN sys.schemas s
	ON s.schema_id = o.schema_id
WHERE o.type IN ({0})
";

            sql = String.Format(sql, GetObjectTypeIdListString(Schema.Constants.DatabaseObjectTypes[typeof(T)]));

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            T obj = new T()
                            {
                                Dataset = this,
                                DatabaseName = DatabaseName,
                                SchemaName = dr.GetString(0),
                                ObjectName = dr.GetString(1),
                                ObjectType = Constants.SqlServerObjectTypeIds[dr.GetString(2).Trim()],
                            };

                            yield return new KeyValuePair<string, T>(GetObjectUniqueKey(obj), obj);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads columns of a database object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal override IEnumerable<KeyValuePair<string, Column>> LoadColumns(DatabaseObject obj)
        {
            var sql = @"
SELECT c.column_id, c.name, t.name, c.max_length, c.scale, c.precision, c.is_nullable, c.is_identity
FROM sys.columns c
INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
INNER JOIN sys.objects o ON o.object_id = c.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE s.name = @schemaName AND o.name = @objectName
ORDER BY c.column_id";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(obj.SchemaName) ? (object)DBNull.Value : (object)obj.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar, 128).Value = obj.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var cd = new Column(obj)
                            {
                                ID = dr.GetInt32(0),
                                Name = dr.GetString(1),
                                IsIdentity = dr.GetBoolean(7)
                            };

                            cd.DataType = CreateDataType(
                                dr.GetString(2),
                                Convert.ToInt32(dr.GetValue(3)),
                                Convert.ToByte(dr.GetValue(4)),
                                Convert.ToByte(dr.GetValue(5)),
                                dr.GetBoolean(6));

                            yield return new KeyValuePair<string, Column>(cd.Name, cd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads indexes of a database object.
        /// </summary>
        /// <param name="databaseObject"></param>
        /// <returns></returns>
        internal override IEnumerable<KeyValuePair<string, Index>> LoadIndexes(DatabaseObject databaseObject)
        {
            var sql = @"
SELECT i.index_id, i.name, i.type, i.is_unique, i.is_primary_key
FROM sys.indexes i
INNER JOIN sys.objects o ON o.object_id = i.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE i.type IN (1, 2) AND
s.name = @schemaName AND o.name = @objectName";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(databaseObject.SchemaName) ? (object)DBNull.Value : (object)databaseObject.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar, 128).Value = databaseObject.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var idx = new Index((TableOrView)databaseObject)
                            {
                                IndexId = dr.GetInt32(0),
                                IndexName = dr.GetString(1),
                                IsClustered = (dr.GetByte(2) == 1),
                                IsUnique = dr.GetBoolean(3),
                                IsPrimaryKey = dr.GetBoolean(4),
                            };

                            yield return new KeyValuePair<string, Index>(idx.IndexName, idx);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads columns of an index of a database object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal override IEnumerable<KeyValuePair<string, IndexColumn>> LoadIndexColumns(Index index)
        {
            var sql = @"
SELECT ic.column_id, ic.key_ordinal, c.name, ic.is_descending_key, t.name, c.max_length, c.scale, c.precision, c.is_nullable, c.is_identity
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic 
    ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns AS c 
    ON ic.object_id = c.object_id AND c.column_id = ic.column_id
INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE i.name = @indexName
ORDER BY ic.key_ordinal";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@indexName", SqlDbType.NVarChar, 128).Value = index.IndexName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var ic = new IndexColumn()
                            {
                                ID = dr.GetInt32(0),
                                KeyOrdinal = dr.GetByte(1),
                                Name = dr.GetString(2),
                                Ordering = dr.GetBoolean(3) ? IndexColumnOrdering.Descending : IndexColumnOrdering.Ascending,
                                IsIdentity = dr.GetBoolean(8),
                            };

                            ic.DataType = CreateDataType(
                                dr.GetString(4),
                                Convert.ToInt32(dr.GetValue(5)),
                                Convert.ToByte(dr.GetValue(6)),
                                Convert.ToByte(dr.GetValue(7)),
                                dr.GetBoolean(9));

                            yield return new KeyValuePair<string, IndexColumn>(ic.Name, ic);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads parameters of a database object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal override IEnumerable<KeyValuePair<string, Parameter>> LoadParameters(DatabaseObject obj)
        {
            var sql = @"
SELECT p.parameter_id, p.name, p.is_output, t.name, p.max_length, p.scale, p.precision , p.has_default_value, p.default_value
FROM sys.parameters p
INNER JOIN sys.objects o ON o.object_id = p.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
INNER JOIN sys.types t ON t.user_type_id = p.user_type_id
WHERE s.name = @schemaName AND o.name = @objectName
ORDER BY p.parameter_id";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(obj.SchemaName) ? (object)DBNull.Value : (object)obj.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar, 128).Value = obj.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // TODO: check this for input only parameters!
                            var dir = String.IsNullOrEmpty(dr.GetString(1)) ? ParameterDirection.ReturnValue : ParameterDirection.InputOutput;

                            var par = new Parameter()
                            {
                                ID = dr.GetInt32(0),
                                Name = dr.GetString(1),
                                Direction = dir,
                                HasDefaultValue = dr.GetBoolean(7),
                                DefaultValue = dr.IsDBNull(8) ? null : dr.GetValue(8),
                            };

                            par.DataType = CreateDataType(
                                dr.GetString(3),
                                Convert.ToInt32(dr.GetValue(4)),
                                Convert.ToByte(dr.GetValue(5)),
                                Convert.ToByte(dr.GetValue(6)),
                                false);

                            yield return new KeyValuePair<string, Parameter>(par.Name, par);
                        }
                    }
                }
            }
        }
        #endregion
        #region Metadata

        /// <summary>
        /// Loads metadata of a database object.
        /// </summary>
        /// <param name="databaseObject"></param>
        /// <returns></returns>
        internal override DatabaseObjectMetadata LoadDatabaseObjectMetadata(DatabaseObject databaseObject)
        {
            var sql = @"
SELECT p.name, p.value
FROM sys.extended_properties p
WHERE p.class = 1 -- OBJECT_OR_COLUMN
    AND p.major_id = OBJECT_ID(@schemaName + '.' + @objectName)
    AND p.minor_id = 0  -- only objects
    AND p.name LIKE 'meta.%'
ORDER BY p.name";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar).Value = databaseObject.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar).Value = databaseObject.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        var meta = new DatabaseObjectMetadata();

                        while (dr.Read())
                        {
                            var name = dr.GetString(0);
                            var value = dr.GetString(1);

                            switch (name)
                            {
                                case Constants.MetaSummary:
                                    meta.Summary = value;
                                    break;
                                case Constants.MetaRemarks:
                                    meta.Remarks = value;
                                    break;
                                case Constants.MetaExample:
                                    meta.Example = value;
                                    break;
                                default:
                                    break;
                            }
                        }

                        return meta;
                    }
                }
            }
        }

        internal override void DropDatabaseObjectMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override void SaveDatabaseObjectMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads column metadata for every column of a database object.
        /// </summary>
        /// <param name="databaseObject"></param>
        protected override void LoadAllColumnMetadata(DatabaseObject databaseObject)
        {
            var sql = @"
SELECT c.name, p.name metaname, p.value
FROM sys.columns c
INNER JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.column_id
WHERE p.major_id = OBJECT_ID(@schemaName + '.' + @objectName)
    AND p.name LIKE 'meta.%'
ORDER BY c.name, p.name";

            LoadAllVariableMetadata(sql, databaseObject, ((IColumns)databaseObject).Columns);
        }

        /// <summary>
        /// Loads parameter metadata of all parameters belonging to an object.
        /// </summary>
        /// <param name="databaseObject"></param>
        protected override void LoadAllParameterMetadata(DatabaseObject databaseObject)
        {
            var sql = @"
SELECT c.name, p.name metaname, p.value
FROM sys.parameters c
INNER JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.parameter_id
WHERE p.major_id = OBJECT_ID(@schemaName + '.' + @objectName)
    AND p.name LIKE 'meta.%'
ORDER BY c.name, p.name";

            LoadAllVariableMetadata(sql, databaseObject, ((IParameters)databaseObject).Parameters);
        }

        private void LoadAllVariableMetadata(string sql, DatabaseObject databaseObject, IDictionary variables)
        {
            // Make sure all will be marked as loaded
            foreach (Variable v in variables.Values)
            {
                v.Metadata = new VariableMetadata();
            }

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar).Value = databaseObject.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar).Value = databaseObject.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        string variablename = null;
                        VariableMetadata meta = null;

                        while (dr.Read())
                        {
                            string name = dr.GetString(0);
                            string metaname = dr.GetString(1);
                            string value = dr.GetString(2);

                            if (name != variablename)
                            {
                                meta = (VariableMetadata)((Variable)variables[name]).Metadata;
                                variablename = name;
                            }

                            switch (metaname)
                            {
                                case Constants.MetaSummary:
                                    meta.Summary = value;
                                    break;
                                case Constants.MetaContent:
                                    meta.Content = value;
                                    break;
                                case Constants.MetaUnit:
                                    meta.Unit = value;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        internal override void DropAllVariableMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override void SaveAllVariableMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        #endregion
        #region Statistics


        /// <summary>
        /// Loads statistics of the dataset.
        /// </summary>
        /// <returns></returns>
        protected override DatasetStatistics LoadDatasetStatistics()
        {
            var sql = @"
-- Raw data space in 8K pages
SELECT SUM(f.size)
FROM sys.database_files f
WHERE f.type = 0

-- Total used space
SELECT SUM(a.total_pages), SUM(a.used_pages), SUM(a.data_pages)
FROM sys.allocation_units a

-- Log space
SELECT SUM(size)
FROM sys.database_files f
WHERE f.type = 1
";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        var stats = new DatasetStatistics();

                        // Data space
                        dr.Read();
                        stats.DataSpace = dr.IsDBNull(0) ? 0L : (long)dr.GetInt32(0) * 0x2000;    // 8K pages

                        // Index space
                        dr.NextResult();
                        dr.Read();
                        stats.UsedSpace = dr.IsDBNull(0) ? 0L : dr.GetInt64(0) * 0x2000;    // 8K pages

                        // Row count
                        dr.NextResult();
                        dr.Read();
                        stats.LogSpace = dr.IsDBNull(0) ? 0L : (long)dr.GetInt32(0) * 0x2000;    // 8K pages

                        return stats;
                    }
                }
            }
        }

        protected override DatasetMetadata LoadDatasetMetadata()
        {
            return new DatasetMetadata();      // *** TODO: implement
        }

        /// <summary>
        /// Loads statistics of a table.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal override TableStatistics LoadTableStatistics(TableOrView obj)
        {
            var sql = @"
-- Data space
SELECT SUM(a.total_pages), SUM(a.used_pages), SUM(a.data_pages)
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
INNER JOIN sys.indexes i ON i.object_id = o.object_id AND i.type IN (0, 1)  -- heap and clustered
INNER JOIN sys.partitions p	ON p.object_id = i.object_id AND p.index_id = i.index_id
INNER JOIN sys.allocation_units a ON a.container_id = p.hobt_id
WHERE s.name = @schemaName AND o.name = @objectName

-- Index space
SELECT SUM(a.total_pages), SUM(a.used_pages), SUM(a.data_pages)
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
INNER JOIN sys.indexes i ON i.object_id = o.object_id AND i.type IN (2)  -- non-clustered
INNER JOIN sys.partitions p	ON p.object_id = i.object_id AND p.index_id = i.index_id
INNER JOIN sys.allocation_units a ON a.container_id = p.hobt_id
WHERE s.name = @schemaName AND o.name = @objectName

-- Row count
SELECT SUM(p.rows)
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
INNER JOIN sys.indexes i ON i.object_id = o.object_id AND i.type IN (0, 1)
INNER JOIN sys.partitions p	ON p.object_id = i.object_id AND p.index_id = i.index_id
WHERE s.name = @schemaName AND o.name = @objectName
";

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(obj.SchemaName) ? (object)DBNull.Value : (object)obj.SchemaName;
                    cmd.Parameters.Add("@objectName", SqlDbType.NVarChar, 128).Value = obj.ObjectName;

                    using (var dr = cmd.ExecuteReader())
                    {
                        var stat = new TableStatistics();

                        // Data space
                        dr.Read();
                        stat.DataSpace = dr.IsDBNull(0) ? 0L : dr.GetInt64(0) * 0x2000;    // 8K pages

                        // Index space
                        dr.NextResult();
                        dr.Read();
                        stat.IndexSpace = dr.IsDBNull(0) ? 0L : dr.GetInt64(0) * 0x2000;    // 8K pages

                        // Row count
                        dr.NextResult();
                        dr.Read();
                        stat.RowCount = dr.IsDBNull(0) ? 0L : dr.GetInt64(0);

                        return stat;
                    }
                }
            }
        }

        #endregion

        internal override void RenameObject(DatabaseObject obj, string objectName)
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException();
            }

            // The stored procedure sp_name expects the old name
            // the the schema.objectname or objectname format.
            // No database name should be specified.

            var sql = @"sp_rename";

            // FullyQualifiedName cannot be used here because that contains DB name.
            string oldname;
            if (String.IsNullOrEmpty(obj.SchemaName))
            {
                oldname = QuoteIdentifier(obj.ObjectName);
            }
            else
            {
                oldname = String.Format(
                    "{0}.{1}",
                    QuoteIdentifier(obj.SchemaName),
                    QuoteIdentifier(obj.ObjectName));
            }

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@objname", SqlDbType.NVarChar, 776).Value = oldname;
                    cmd.Parameters.Add("@newname", SqlDbType.NVarChar, 776).Value = objectName;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal override void DropObject(DatabaseObject obj)
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException();
            }

            var sql = String.Format(@"
IF (OBJECT_ID('{1}') IS NOT NULL)
BEGIN
DROP {0} {1}
END",
                Constants.SqlServerObjectTypeNames[obj.ObjectType],
                GetObjectFullyResolvedName(obj));

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal override void CreateTable(Table table)
        {
            // TODO: move to codegen

            if (!IsMutable)
            {
                throw new InvalidOperationException();
            }

            if (table.Columns.Count == 0)
            {
                throw new InvalidOperationException();
            }

            // Build column list
            var cols = new StringBuilder();

            int q = 0;
            foreach (var c in table.Columns.Values.OrderBy(ci => ci.ID))
            {
                if (q > 0)
                {
                    cols.AppendLine(",");
                }

                cols.AppendFormat("{0} {1} {2}NULL",
                    QuoteIdentifier(c.Name),
                    c.DataType.NameWithLength,
                    c.DataType.IsNullable ? "" : "NOT ");

                q++;
            }

            var sql = String.Format(
                "CREATE TABLE {0} (\r\n{1}\r\n)",
                GetObjectFullyResolvedName(table),
                cols.ToString());

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal override void TruncateTable(Table table)
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException();
            }

            var sql = String.Format(
                "TRUNCATE TABLE {0}",
                GetObjectFullyResolvedName(table));

            using (var cn = OpenConnection())
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected SqlConnection OpenConnection()
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(ConnectionString);
            csb.Enlist = false;

            SqlConnection cn = new SqlConnection(csb.ConnectionString);
            cn.Open();
            return cn;
        }

        private string GetObjectTypeIdListString(DatabaseObjectType objectType)
        {
            string res = String.Empty;

            foreach (var t in Constants.SqlServerObjectTypeIds)
            {
                if (objectType.HasFlag(t.Key))
                {
                    res += String.Format(",'{0}'", Constants.SqlServerObjectTypeIds[t.Key]);
                }
            }

            return res.Substring(1);
        }

        public override string GetSpecializedConnectionString(string connectionString, bool integratedSecurity, string username, string password, bool enlist)
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(connectionString);
            csb.IntegratedSecurity = integratedSecurity;
            csb.UserID = username;
            csb.Password = password;
            csb.Enlist = enlist;

            return csb.ConnectionString;
        }

        #region Data type mapping functions

        /// <summary>
        /// Creates a data type based on a schema table row
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        protected override DataType CreateDataType(DataRow dr)
        {
            Type type;
            string name;
            int length;
            byte precision, scale;
            bool isNullable;

            GetDataTypeDetails(dr, out type, out name, out length, out precision, out scale, out isNullable);

            return CreateDataType(name, length, scale, precision, isNullable);
        }

        protected override DataType CreateDataType(string name)
        {
            SqlDbType sqltype;
            if (Enum.TryParse<SqlDbType>(name, true, out sqltype))
            {
                // This can be interpreted as a SQL Server type
                return CreateDataType(sqltype);
            }
            else
            {
                return base.CreateDataType(name);
            }
        }

        private DataType CreateDataType(SqlDbType type)
        {
            DataType dt;

            switch (type)
            {
                case System.Data.SqlDbType.Bit:
                    dt = DataTypes.SqlBit;
                    break;
                case System.Data.SqlDbType.TinyInt:
                    dt = DataTypes.SqlTinyInt;
                    break;
                case System.Data.SqlDbType.SmallInt:
                    dt = DataTypes.SqlSmallInt;
                    break;
                case System.Data.SqlDbType.Int:
                    dt = DataTypes.SqlInt;
                    break;
                case System.Data.SqlDbType.BigInt:
                    dt = DataTypes.SqlBigInt;
                    break;
                case System.Data.SqlDbType.Real:
                    dt = DataTypes.SqlReal;
                    break;
                case System.Data.SqlDbType.Float:
                    dt = DataTypes.SqlFloat;
                    break;
                case System.Data.SqlDbType.Image:
                    dt = DataTypes.SqlImage;
                    break;
                case System.Data.SqlDbType.Binary:
                    dt = DataTypes.SqlBinary;
                    break;
                case System.Data.SqlDbType.VarBinary:
                    dt = DataTypes.SqlVarBinary;
                    break;
                case System.Data.SqlDbType.Text:
                    dt = DataTypes.SqlText;
                    break;
                case System.Data.SqlDbType.Char:
                    dt = DataTypes.SqlChar;
                    break;
                case System.Data.SqlDbType.VarChar:
                    dt = DataTypes.SqlVarChar;
                    break;
                case System.Data.SqlDbType.NText:
                    dt = DataTypes.SqlNText;
                    break;
                case System.Data.SqlDbType.NChar:
                    dt = DataTypes.SqlNChar;
                    break;
                case System.Data.SqlDbType.NVarChar:
                    dt = DataTypes.SqlNVarChar;
                    break;
                case System.Data.SqlDbType.Date:
                    dt = DataTypes.SqlDate;
                    break;
                case System.Data.SqlDbType.DateTime:
                    dt = DataTypes.SqlDateTime;
                    break;
                case System.Data.SqlDbType.DateTime2:
                    dt = DataTypes.SqlDateTime2;
                    break;
                case System.Data.SqlDbType.DateTimeOffset:
                    dt = DataTypes.SqlDateTimeOffset;
                    break;
                case System.Data.SqlDbType.SmallDateTime:
                    dt = DataTypes.SqlSmallDateTime;
                    break;
                case System.Data.SqlDbType.Time:
                    dt = DataTypes.SqlTime;
                    break;
                case System.Data.SqlDbType.Timestamp:
                    dt = DataTypes.SqlTimestamp;
                    break;
                case System.Data.SqlDbType.Decimal:
                    dt = DataTypes.SqlDecimal;
                    break;
                case System.Data.SqlDbType.SmallMoney:
                    dt = DataTypes.SqlSmallMoney;
                    break;
                case System.Data.SqlDbType.Money:
                    dt = DataTypes.SqlMoney;
                    break;
                case System.Data.SqlDbType.UniqueIdentifier:
                    dt = DataTypes.SqlUniqueIdentifier;
                    break;
                case System.Data.SqlDbType.Variant:
                    dt = DataTypes.SqlVariant;
                    break;
                case System.Data.SqlDbType.Xml:
                    dt = DataTypes.SqlXml;
                    break;
                case System.Data.SqlDbType.Structured:
                    throw new NotImplementedException();
                case System.Data.SqlDbType.Udt:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            return dt;
        }

        public DataType CreateDataType(SqlDbType type, int length, byte precision, byte scale, bool isNullable)
        {
            var dt = CreateDataType(type);

            if (dt.HasLength)
            {
                dt.Length = length;
            }

            if (dt.HasPrecision)
            {
                dt.Precision = precision;
            }

            if (dt.HasScale)
            {
                dt.Scale = scale;
            }

            dt.IsNullable = isNullable;

            return dt;
        }

        #endregion

    }
}
