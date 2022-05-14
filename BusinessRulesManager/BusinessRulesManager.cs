using BusinessRulesManager.Entities;
using DataMigrator;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Logger;

namespace BusinessRulesManager
{
    public class BusinessRulesManager
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        public List<BusinessRuleEntity> GetBusinessRules(string origin)
        {

            var objResult = new DataTable();
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand("GetBusinessRules", myCon);
            myCommand.CommandType = CommandType.StoredProcedure;
            
            myCommand.Parameters.Add("@Origin", SqlDbType.NVarChar).Value = origin;

            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var bRs = (from DataRow dr in objResult.Rows
                           select new BusinessRuleEntity
                           {
                               RuleId = dr.Field<int?>("Id"),
                               EntityName = dr["EntityName"].ToString(),
                               PropertyName = dr["PropertyName"].ToString(),
                               IsRequired = dr.Field<bool?>("IsRequired") ?? false,
                               RegEx = dr["RegEx"].ToString(),
                               Description = dr["Description"].ToString(),
                               Origin = dr["Origin"].ToString(),
                               IsActive = dr.Field<bool?>("IsActive") ?? false
                           }).ToList();
            return bRs;
        }

        public List<BusinessRuleEntity> GetInActiveBusinessRules(string origin)
        {

            var objResult = new DataTable();
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand("GetInActiveBusinessRules", myCon);
            myCommand.CommandType = CommandType.StoredProcedure;

            myCommand.Parameters.Add("@Origin", SqlDbType.NVarChar).Value = origin;

            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var bRs = (from DataRow dr in objResult.Rows
                select new BusinessRuleEntity
                {
                    RuleId = dr.Field<int?>("Id"),
                    EntityName = dr["EntityName"].ToString(),
                    PropertyName = dr["PropertyName"].ToString(),
                    IsRequired = dr.Field<bool?>("IsRequired") ?? false,
                    RegEx = dr["RegEx"].ToString(),
                    Description = dr["Description"].ToString(),
                    Origin = dr["Origin"].ToString(),
                    IsActive = dr.Field<bool?>("IsActive") ?? false
                }).ToList();
            return bRs;
        }

        public void SaveBusinessRules(List<BusinessRuleEntity> businessRules)
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();



            using var myCommand = new SqlCommand("GetBusinessRules", myCon);
        }

        private List<BusinessRuleEntity> GetExistingRulesIds()
        {
            var objResult = new DataTable();
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand("GetBusinessRules", myCon);
            myCommand.CommandType = CommandType.StoredProcedure;

            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var bRs = (from DataRow dr in objResult.Rows
                select new BusinessRuleEntity
                {
                    RuleId = dr.Field<int?>("Id"),
                    EntityName = dr["EntityName"].ToString(),
                    PropertyName = dr["PropertyName"].ToString(),
                    IsRequired = dr.Field<bool?>("IsRequired") ?? false,
                    RegEx = dr["RegEx"].ToString(),
                    Description = dr["Description"].ToString(),
                    Origin = dr["Origin"].ToString()
                }).ToList();
            return bRs;
        }
    }
}
