using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using BusinessRulesEngine.Entities;
using BusinessRulesEngine.Rules;
using Logger;

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

    }
}
