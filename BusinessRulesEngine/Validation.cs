using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using BusinessRulesEngine.Entities;
using BusinessRulesEngine.Rules;
using Logger;
using System.Linq;

namespace BusinessRulesEngine
{
    public class Validation
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();

        public List<MigratedObject> ValidateMigratedObjects(List<MigratedObject> migratedObjects, string entity,
            DomainEnum domainEnum)
        {
            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            var query = "SELECT Rules.Id AS RuleId, Name PropertyName, IsRequired, RegEx, Description " +
                        "FROM dbo.Entity " +
                        "INNER JOIN dbo.Property ON Property.EntityId = Entity.Id " +
                        "INNER JOIN dbo.Rules ON Rules.PropertyId = Property.Id " +
                        "WHERE Rules.IsActive = 1 AND Origin = '" + domainEnum.ToString() + "' AND TableName = '" + entity + "'; ";

            using var myCommand = new SqlCommand(query, myCon);
            myCon.Open();

            var myReader = myCommand.ExecuteReader();
            var objResult = new DataTable();
            objResult.Load(myReader);

            ValidateObjectsForAllRules(migratedObjects, objResult.Rows);

            return migratedObjects;
        }

        public List<MigratedObject> ValidateRelationForMigratedObject(List<MigratedObject> migratedObjects, string entity, List<RelationObject> relations)
        {
            List<RelationRuleObject> relationRuleObjects = GetRelationRulesForEntity(entity);


            foreach (var relationRuleObject in relationRuleObjects)
            {
                List<RelationObject> currentRelations = relations.Where(r => r.RelationTable == relationRuleObject.RelationTable).ToList();
                RelationRule relationRule = new RelationRule();
                relationRule.relationRuleObject = relationRuleObject;
                relationRule.relationObjects = currentRelations;
                relationRule.Apply(migratedObjects, null);
            }

            return migratedObjects;
        }

        private void ValidateObjectsForAllRules(List<MigratedObject> migratedObjects, DataRowCollection brRows)
        {
            ValidateObjectsForRule(new IsRequiredRule(), migratedObjects, brRows);
            ValidateObjectsForRule(new RegExRule(), migratedObjects, brRows);
        }

        private void ValidateObjectsForRule(RuleMaster rule, List<MigratedObject> migratedObjects,
            DataRowCollection brRows)
        {
            rule.Apply(migratedObjects, brRows);
        }

        private List<RelationRuleObject> GetRelationRulesForEntity(string entityName)
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            string query = $"SELECT rr.id RuleId,rr.RelationId,rr.[Type],rr.[Count],rr.[Description],r.id,r.RelationTable,r.RelationSourceTable,r.RelationTargetTable,r.RelationSourceColumn,r.RelationTargetColumn " +
                            "FROM RelationRule rr " +
                            "INNER JOIN Relation r ON rr.RelationId = r.id " +
                            $"WHERE r.RelationSourceTable = '{entityName}' AND rr.IsActive = 1";

            var objResult = new DataTable();
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var relationRules = (from DataRow dr in objResult.Rows
                             select new RelationRuleObject
                             {
                                 id = int.Parse(dr["RuleId"].ToString()),
                                 RelationId = int.Parse(dr["RelationId"].ToString()),
                                 Type = int.Parse(dr["Type"].ToString()),
                                 Count = int.Parse(dr["Count"].ToString()),
                                 Description = dr["Description"].ToString(),
                                 RelationTable = dr["RelationTable"].ToString(),
                                 RelationSourceTable = dr["RelationSourceTable"].ToString(),
                                 RelationTargetTable = dr["RelationTargetTable"].ToString(),
                                 RelationSourceColumn = dr["RelationSourceColumn"].ToString(),
                                 RelationTargetColumn = dr["RelationTargetColumn"].ToString()
                             }).Distinct().ToList();
            return relationRules;
        }

    }
}
