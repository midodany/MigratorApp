using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BusinessRulesEngine.Entities;
using Logger;

namespace BusinessRulesEngine
{
    public class BatchManager
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        public string CreateNewBatch()
        {
            string BatchId = Guid.NewGuid().ToString();
            string StartRunTime = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");

            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            var query = "INSERT INTO dbo.Batch VALUES ('" + BatchId + "', '" + StartRunTime + "', 0)";
            using var myCommand = new SqlCommand(query, myCon);
            myCommand.ExecuteNonQuery();

            return BatchId;
        }

        public void FinishBatch(string BatchId)
        {
            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            var query = "UPDATE dbo.Batch SET FinishedSuccessfully = 1 WHERE BatchId =  '" + BatchId + "';";
            using var myCommand = new SqlCommand(query, myCon);
            myCommand.ExecuteNonQuery();
        }

        public List<BatchEntity> GetBatches()
        {
            var objResult = new DataTable();
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand("GetBatches", myCon);
            myCommand.CommandType = CommandType.StoredProcedure;
            
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var batches = (from DataRow dr in objResult.Rows
                select new BatchEntity
                {
                    BatchId = dr["BatchId"].ToString(),
                    StartRunTime = DateTime.Parse(dr["StartRunTime"].ToString() ?? string.Empty),
                    FinishedSuccessfully = dr.Field<bool?>("FinishedSuccessfully") ?? false
                }).ToList();
            return batches;
        }
    }
}
