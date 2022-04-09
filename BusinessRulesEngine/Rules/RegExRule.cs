using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using BusinessRulesEngine.Entities;

namespace BusinessRulesEngine.Rules
{
    internal class RegExRule : RuleMaster
    {
        internal override void Apply(List<MigratedObject> migratedObjects, DataRowCollection brRows)
        {
            int RuleId;
            foreach (var migratedObject in migratedObjects)
            {
                foreach (DataRow brRow in brRows)
                {
                    string propertyValue =
                        CommonFunctions.GetThePropertyValue(migratedObject, brRow["PropertyName"].ToString());
                    if (!string.IsNullOrEmpty(brRow["RegEx"].ToString()) &&
                        !ValidateRegex(brRow["RegEx"].ToString(), propertyValue))
                    {
                        int.TryParse(brRow["RuleId"].ToString(), out RuleId);
                        migratedObject.validationLogs.Add(new ValidationLog{objectId = migratedObject.Id, ruleId = RuleId, validationMessage = "RuleId: " + brRow["RuleId"] + ". Regular Expression not correct: " + brRow["PropertyName"]});
                    }
                }
            }
        }
        private bool ValidateRegex(string strRegex, string input)
        {
            Regex re = new Regex(strRegex);

            return (input != null && re.IsMatch(input));
        }
    }
}
