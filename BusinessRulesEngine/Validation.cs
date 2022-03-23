using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using BusinessRulesEngine.Entities;
using BusinessRulesEngine.Rules;
using Logger;
using NRules;
using NRules.Fluent;

namespace BusinessRulesEngine
{
    public class Validation
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        public List<MigratedObject> ValidateMigratedObjects(List<MigratedObject> migratedObjects,string entity)
        {
            var validObjects = new List<MigratedObject>();
            
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            var query = "SELECT Rules.Id, Name PropertyName, IsRequired, RegEx, Description " +
                        "FROM dbo.Entity " +
                        "INNER JOIN dbo.Property ON Property.EntityId = Entity.Id " +
                        "INNER JOIN dbo.Rules ON Rules.PropertyId = Property.Id " +
                        "WHERE Origin = 'Target' AND TableName = '" + entity + "'; ";

            using var myCommand = new SqlCommand(query, myCon);
            myCon.Open();

            var myReader = myCommand.ExecuteReader();
            var objResult = new DataTable();
            objResult.Load(myReader);

            foreach (var migratedObject in migratedObjects)
            {
                var notValidReason = ValidateObjects(migratedObject, objResult.Rows);

                if (notValidReason == "")
                {
                    validObjects.Add(migratedObject);
                }
                else
                {
                    //add to not valid objects
                }

            }

            return validObjects;
        }

        private string ValidateObjects(MigratedObject migratedObject, DataRowCollection brRows)
        {
            var reason = "";

            var repository = new RuleRepository();
            repository.Load(x => x.From(typeof(IsRequiredRule).Assembly));

            //Compile rules
            var factory = repository.Compile();
            
            foreach (DataRow dr in brRows)
            {
                var session = factory.CreateSession();

                session.Insert(migratedObject);
                session.Insert(dr);

                session.Fire();
            }

            foreach (DataRow dr in brRows)
            {

                var propertyValue = GetThePropertyValue(migratedObject, dr["PropertyName"].ToString());
                var isRequiredValid = !(bool)dr["IsRequired"] ||
                                        !string.IsNullOrEmpty(propertyValue);
                var isRegexValid = string.IsNullOrEmpty(dr["RegEx"].ToString()) ||
                                         ValidateRegex(dr["RegEx"].ToString(), propertyValue);
                reason += !(isRequiredValid && isRegexValid) ? dr["Description"] + "; " : "";
            }

            return reason;
        }

        private bool ValidateRegex(string strRegex, string input)
        {
            Regex re = new Regex(strRegex);

            return (input != null && re.IsMatch(input));
        }

        private string GetThePropertyValue(object instance, string propertyName)
        {
            Type type = instance.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            return propertyInfo != null ? propertyInfo.GetValue(instance, null)?.ToString() : "";
        }
    }
}
