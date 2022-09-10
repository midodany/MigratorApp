using System;
using System.Collections.Generic;
using System.Data;
using BusinessRulesEngine.Entities;
using System.Linq;

namespace BusinessRulesEngine.Rules
{
    internal class RelationRule : RuleMaster
    {
        internal override void Apply(List<MigratedObject> migratedObjects, DataRowCollection brRows)
        {
            if (relationObjects == null || relationRuleObject == null)
                return;
            int RuleId = relationRuleObject.id;
            foreach (MigratedObject migratedObject in migratedObjects)
            { 
                int resultCount = relationObjects.Count(item => item.RelationSourceId == migratedObject.MigrationId);

                int count = relationRuleObject.Count;

                if (relationRuleObject.Type == 1)//exactly equals
                {
                    if (resultCount != count)
                    {
                        migratedObject.ValidationLogs.Add(new ValidationLog { objectId = migratedObject.MigrationId, ruleId = RuleId, validationMessage = "Relation Rule violated. Description: " + relationRuleObject.Description });
                    }
                }
                else if (relationRuleObject.Type == 2)//at most
                {
                    if (resultCount > count)
                    {
                        migratedObject.ValidationLogs.Add(new ValidationLog { objectId = migratedObject.MigrationId, ruleId = RuleId, validationMessage = "Relation Rule violated. Description: " + relationRuleObject.Description });
                    }
                }
                else if (relationRuleObject.Type == 3)//at least
                {
                    if (resultCount < count)
                    {
                        migratedObject.ValidationLogs.Add(new ValidationLog { objectId = migratedObject.MigrationId, ruleId = RuleId, validationMessage = "Relation Rule violated. Description: " + relationRuleObject.Description });
                    }
                }

            }
        }
    }
}
