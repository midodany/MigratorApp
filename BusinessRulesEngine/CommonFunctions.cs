using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
    }
}
