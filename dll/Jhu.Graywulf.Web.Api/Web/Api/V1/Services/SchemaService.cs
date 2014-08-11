﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.ServiceModel.Security;
using System.Security.Permissions;
using System.ComponentModel;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Web.Services;

namespace Jhu.Graywulf.Web.Api.V1
{
    [ServiceContract]
    [Description("Gives access to the datasets and database schema.")]
    public interface ISchemaService
    {
        [OperationContract]
        [DynamicResponseFormat]
        [WebGet(UriTemplate = "/datasets")]
        [Description("Returns a list of all available datasets.")]
        DatasetListResponse ListDatasets();

        [OperationContract]
        [DynamicResponseFormat]
        [WebGet(UriTemplate = "/datasets/{datasetName}")]
        [Description("Returns information about a single dataset")]
        Dataset GetDataset(
            [Description("Name of the dataset.")]
            string datasetName);

        [OperationContract]
        [DynamicResponseFormat]
        [WebGet(UriTemplate = "/datasets/{datasetName}/tables")]
        [Description("Returns a list of the tables of a dataset.")]
        TableListResponse ListTables(
            [Description("Name of the dataset.")]
            string datasetName);

        [OperationContract]
        [DynamicResponseFormat]
        [WebGet(UriTemplate = "/datasets/{datasetName}/tables/{tableName}")]
        [Description("Returns information about a single table.")]
        Table GetTable(
            [Description("Name of the dataset.")]
            string datasetName,
            [Description("Name of the table.")]
            string tableName);

        [OperationContract]
        [DynamicResponseFormat]
        [WebGet(UriTemplate = "/datasets/{datasetName}/tables/{tableName}/columns")]
        [Description("Returns the list of columns of a table")]
        ColumnListResponse ListTableColumns(
            [Description("Name of the dataset.")]
            string datasetName,
            [Description("Name of the table.")]
            string tableName);
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
    [RestServiceBehavior]
    public class SchemaService : RestServiceBase, ISchemaService
    {
        private Schema.DatasetBase GetDatasetInternal(string datasetName)
        {
            return FederationContext.SchemaManager.Datasets[datasetName];
        }

        private Schema.Table GetTableInternal(string datasetName, string tableName)
        {
            var parts = tableName.Split('.');
            var dataset = FederationContext.SchemaManager.Datasets[datasetName];
            return dataset.Tables[dataset.DatabaseName, parts[0], parts[1]];
        }

        public DatasetListResponse ListDatasets()
        {
            return new DatasetListResponse(FederationContext.SchemaManager.Datasets.Values);
        }

        public Dataset GetDataset(string datasetName)
        {
            return new Dataset(GetDatasetInternal(datasetName));
        }

        public TableListResponse ListTables(string datasetName)
        {
            var dataset = GetDatasetInternal(datasetName);
            dataset.Tables.LoadAll();

            return new TableListResponse(dataset.Tables.Values);
        }

        public Table GetTable(string datasetName, string tableName)
        {
            var table = GetTableInternal(datasetName, tableName);
            return new Table(table);
        }

        public ColumnListResponse ListTableColumns(string datasetName, string tableName)
        {
            var table = GetTableInternal(datasetName, tableName);
            return new ColumnListResponse(table.Columns.Values);
        }
    }
}