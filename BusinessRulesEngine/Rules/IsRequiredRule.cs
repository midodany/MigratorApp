using System.Collections.Generic;
using System.Data;
using BusinessRulesEngine.Entities;

namespace BusinessRulesEngine.Rules
{
    internal class IsRequiredRule : RuleMaster
    {
        internal override void Apply(List<MigratedObject> migratedObjects, DataRowCollection brRows)
        {
            int RuleId;
            foreach (DataRow brRow in brRows)
            {
                foreach (var migratedObject in migratedObjects)
                {
                    string propertyValue =
                        CommonFunctions.GetThePropertyValue(migratedObject, brRow["PropertyName"].ToString());
                    if ((bool)brRow["IsRequired"] && string.IsNullOrEmpty(propertyValue))
                    {
                        int.TryParse(brRow["RuleId"].ToString(), out RuleId);
                        migratedObject.ValidationLogs.Add(new ValidationLog{ objectId = migratedObject.MigrationId, ruleId = RuleId, validationMessage = "Required Rule violated. Description: " + brRow["Description"] });
                    }
                }
            }
        }
    }
}
