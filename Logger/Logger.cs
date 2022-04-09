using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
    }
}
