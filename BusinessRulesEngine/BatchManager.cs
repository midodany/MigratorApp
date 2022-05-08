using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
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
    }
}
