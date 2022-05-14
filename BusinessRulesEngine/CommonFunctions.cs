using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BusinessRulesEngine.Entities;
using Logger;

namespace BusinessRulesEngine
{
    public static class CommonFunctions
    {
        public static string GetThePropertyValue(object instance, string propertyName)
        {
            Type type = instance.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            return propertyInfo != null ? propertyInfo.GetValue(instance, null)?.ToString() : "";
        }


        public static List<MigratedObject> ApplyFilter(string BatchId,DomainEnum domainEnum, List<MigratedObject> migratedObjects)
        {
            var rejectedObjects = migratedObjects.Where(c => c.ValidationLogs.Count > 0).ToList();

            var validationLogs = rejectedObjects.SelectMany(v => v.ValidationLogs)
                .Select(vl => new LogObject { objectId = vl.objectId, RuleId = vl.ruleId, ValidationMessage = vl.validationMessage })
                .ToList();

            Logger.Logger.Log(BatchId, domainEnum.ToString(), validationLogs);

            return migratedObjects.Where(c => c.ValidationLogs.Count == 0).ToList();
        }
    }
}
