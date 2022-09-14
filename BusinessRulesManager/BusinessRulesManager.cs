using BusinessRulesManager.Entities;
using DataMigrator;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Logger;
using BusinessRulesEngine.Entities;

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
                               IsActive = dr.Field<bool?>("IsActive") ?? false,
                               PropertyId = dr.Field<int?>("PropertyId")
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
                    IsActive = dr.Field<bool?>("IsActive") ?? false,
                    PropertyId = dr.Field<int?>("PropertyId")
                }).ToList();
            return bRs;
        }

        public List<RelationRuleObject> GetRelationRules()
        {

            var objResult = new DataTable();
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand("GetRelationRules", myCon);
            myCommand.CommandType = CommandType.StoredProcedure;

            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var bRs = (from DataRow dr in objResult.Rows
                       select new RelationRuleObject
                       {
                           id = dr.Field<int>("id"),
                           RelationId = dr.Field<int>("RelationId"),
                           Type = dr.Field<int>("Type"),
                           Count = dr.Field<int>("Count"),
                           IsActive = dr.Field<bool?>("IsActive") ?? false,
                           Description = dr["Description"].ToString(),
                           RelationSourceTable = dr["RelationSourceTable"].ToString(),
                           RelationTargetTable = dr["RelationTargetTable"].ToString(),
                           RelationTable = dr["RelationTable"].ToString(),
                       }).ToList();
            return bRs;
        }
        
        public void SaveBusinessRules(List<BusinessRuleEntity> businessRules)
        {
            foreach (var businessRuleEntity in businessRules)
            {
                using var myCon =
                    new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));

                using var myCommand = new SqlCommand("AddAdjustBusinessRule", myCon);
                {

                    myCommand.CommandType = CommandType.StoredProcedure;

                    myCommand.Parameters.Add("@RuleId", SqlDbType.VarChar).Value = businessRuleEntity.RuleId;
                    myCommand.Parameters.Add("@PropertyId", SqlDbType.VarChar).Value = businessRuleEntity.PropertyId;
                    myCommand.Parameters.Add("@IsRequired", SqlDbType.VarChar).Value = businessRuleEntity.IsRequired;
                    myCommand.Parameters.Add("@RegEx", SqlDbType.VarChar).Value = businessRuleEntity.RegEx;
                    myCommand.Parameters.Add("@Description", SqlDbType.VarChar).Value = businessRuleEntity.Description;
                    myCommand.Parameters.Add("@IsActive", SqlDbType.VarChar).Value = businessRuleEntity.IsActive;


                    myCon.Open();
                    myCommand.ExecuteNonQuery();
                    myCon.Close();
                }

            }

        }

        public void SaveRelationRules(List<RelationRuleObject> relationRules)
        {
            foreach (var relationRuleEntity in relationRules)
            {
                using var myCon =
                    new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));

                using var myCommand = new SqlCommand("AddAdjustRelationRule", myCon);
                {

                    myCommand.CommandType = CommandType.StoredProcedure;

                    myCommand.Parameters.Add("@id", SqlDbType.VarChar).Value = relationRuleEntity.id;
                    myCommand.Parameters.Add("@RelationId", SqlDbType.VarChar).Value = relationRuleEntity.RelationId;
                    myCommand.Parameters.Add("@Type", SqlDbType.VarChar).Value = relationRuleEntity.Type;
                    myCommand.Parameters.Add("@IsActive", SqlDbType.VarChar).Value = relationRuleEntity.IsActive;
                    myCommand.Parameters.Add("@Description", SqlDbType.VarChar).Value = relationRuleEntity.Description;
                    myCommand.Parameters.Add("@Count", SqlDbType.VarChar).Value = relationRuleEntity.Count;


                    myCon.Open();
                    myCommand.ExecuteNonQuery();
                    myCon.Close();
                }

            }

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
