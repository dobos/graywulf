﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jhu.Graywulf.Schema
{
    public class UnavailableDataset : DatasetBase
    {
        public override string ProviderName
        {
            get { return "Unavailable"; }
        }

        public override string DatabaseName
        {
            get { return null; }
            set { }
        }

        public static UnavailableDataset Create(string name, Exception ex)
        {
            return new UnavailableDataset()
            {
                Name = name,
                IsInError = true,
                LastException = ex
            };
        }

        public override string QuoteIdentifier(string identifier)
        {
            throw new NotImplementedException();
        }

        public override string GetObjectFullyResolvedName(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        protected override void OnLoadDatabaseObject<T>(T databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override bool OnIsObjectExisting(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<KeyValuePair<string, T>> OnLoadAllObjects<T>()
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<KeyValuePair<string, Column>> OnLoadColumns(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<KeyValuePair<string, IndexColumn>> OnLoadIndexColumns(Index index)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<KeyValuePair<string, Index>> OnLoadIndexes(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<KeyValuePair<string, Parameter>> OnLoadParameters(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        protected override DatasetMetadata OnLoadDatasetMetadata()
        {
            throw new NotImplementedException();
        }

        protected internal override DatabaseObjectMetadata OnLoadDatabaseObjectMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override void OnSaveDatabaseObjectMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override void OnDropDatabaseObjectMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        protected override void OnLoadAllColumnMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        protected override void OnLoadAllParameterMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override void OnSaveAllVariableMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        internal override void OnDropAllVariableMetadata(DatabaseObject databaseObject)
        {
            throw new NotImplementedException();
        }

        protected override DatasetStatistics OnLoadDatasetStatistics()
        {
            throw new NotImplementedException();
        }

        internal override TableStatistics OnLoadTableStatistics(TableOrView tableOrView)
        {
            throw new NotImplementedException();
        }

        internal override void OnRenameObject(DatabaseObject obj, string schemaName, string objectName)
        {
            throw new NotImplementedException();
        }

        internal override void OnCreateTable(Table table, bool createPrimaryKey, bool createIndexes)
        {
            throw new NotImplementedException();
        }

        internal override void OnDropObject(DatabaseObject obj)
        {
            throw new NotImplementedException();
        }

        internal override void OnCreateIndex(Index index)
        {
            throw new NotImplementedException();
        }

        internal override void OnDropIndex(Index index)
        {
            throw new NotImplementedException();
        }

        internal override void OnTruncateTable(Table table)
        {
            throw new NotImplementedException();
        }

        public override string GetSpecializedConnectionString(string connectionString, bool integratedSecurity, string username, string password, bool enlist)
        {
            throw new NotImplementedException();
        }

        protected override DataType MapDataType(DataRow dr)
        {
            return base.MapDataType(dr);
        }

        protected override DataType MapDataType(string name)
        {
            throw new NotImplementedException();
        }

        public override IDbConnection OpenConnection()
        {
            throw new NotImplementedException();
        }
    }
}