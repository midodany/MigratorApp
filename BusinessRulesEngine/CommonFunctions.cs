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
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    return Convert.ToDateTime(propertyInfo.GetValue(instance, null)).ToString("MM/dd/yyyy");
                }
                else
                    return propertyInfo.GetValue(instance, null)?.ToString();
            }
            else
                return "";
            
        }

        public static void SetThePropertyValue(object instance,string propertyName, object propertyValue)
        {
            Type type = instance.GetType();

            PropertyInfo prop = type.GetProperty(propertyName);
            
            if(prop.PropertyType == typeof(int?))
            {
                int resultValue;
                prop.SetValue(instance, int.TryParse(propertyValue.ToString(), out resultValue) ? resultValue : null );
            }
            else if (prop.PropertyType == typeof(int))
            {
                int resultValue;
                prop.SetValue(instance, int.TryParse(propertyValue.ToString(), out resultValue) ? resultValue : 0);
            }
            else if (prop.PropertyType == typeof(bool))
            {
                bool resultValue;
                prop.SetValue(instance, bool.TryParse(propertyValue.ToString(), out resultValue) ? resultValue : false);
            }
            else if (prop.PropertyType == typeof(DateTime))
            {
                DateTime resultValue;
                prop.SetValue(instance, DateTime.TryParse(propertyValue.ToString(), out resultValue) ? resultValue : null);
            }
            else
            {
                prop.SetValue(instance, propertyValue);
            }
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
