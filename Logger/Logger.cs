using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Logger
{
    public static class Logger
    {
        private static readonly ConnectionStringManager ConnectionStringManager = new ConnectionStringManager();
        public static void Log(string BatchId, string Domain, List<LogObject> logObjects)
        {
            DataTable tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("BatchId", typeof(string)));
            tbl.Columns.Add(new DataColumn("Domain", typeof(string)));
            tbl.Columns.Add(new DataColumn("RuleId", typeof(int)));
            tbl.Columns.Add(new DataColumn("ValidationMessage", typeof(string)));
            tbl.Columns.Add(new DataColumn("ObjectId", typeof(int)));

            foreach (var logObject in logObjects)
            {
                DataRow dr = tbl.NewRow();
                dr["BatchId"] = BatchId;
                dr["Domain"] = Domain;
                dr["RuleId"] = logObject.RuleId;
                dr["ValidationMessage"] = logObject.ValidationMessage;
                dr["ObjectId"] = logObject.objectId;

                tbl.Rows.Add(dr);
            }

            string connection = ConnectionStringManager.GetConnectionString("BRSourceConnectionString");
            SqlConnection con = new SqlConnection(connection);
            
            SqlBulkCopy objbulk = new SqlBulkCopy(con);

            //assign Destination table name  
            objbulk.DestinationTableName = "Log";


            objbulk.ColumnMappings.Add("BatchId", "BatchId");
            objbulk.ColumnMappings.Add("Domain", "Domain");
            objbulk.ColumnMappings.Add("RuleId", "RuleId");
            objbulk.ColumnMappings.Add("ValidationMessage", "ValidationMessage");
            objbulk.ColumnMappings.Add("ObjectId", "ObjectId");

            con.Open();
             
            objbulk.WriteToServerAsync(tbl);
            con.Close();

        }

        public static List<LogObject> GetLogObjects(string batchId)
        {
            var objResult = new DataTable();
            using var myCon = new SqlConnection(ConnectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand("GetLogWithBatchId", myCon);
            myCommand.CommandType = CommandType.StoredProcedure;
            
            myCommand.Parameters.Add("@BatchId", SqlDbType.NVarChar).Value = batchId;
            
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var batches = (from DataRow dr in objResult.Rows
                select new LogObject
                {
                    RuleId = dr.Field<int?>("RuleId"),
                    Domain = dr["Domain"].ToString(),
                    ValidationMessage = dr["ValidationMessage"].ToString(),
                    TableName = dr["TableName"].ToString(),
                    PropertyName = dr["PropertyName"].ToString(),
                    objectId = dr["objectId"].ToString(),
                    IsActive = dr.Field<bool>("IsActive"),
                    RuleType = dr.Field<int>("RuleType")
                }).ToList();
            return batches;
        }

    }
}
