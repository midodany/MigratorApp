using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BusinessRulesEngine.Entities;
using NRules.Fluent.Dsl;
using Rule = NRules.Fluent.Dsl.Rule;

namespace BusinessRulesEngine.Rules
{
    public class IsRequiredRule : Rule
    {
        public override void Define()
        {
            MigratedObject migratedObject = default;
            DataRow brRow = default;

            When()
                .Match<MigratedObject>(() => migratedObject)
                .Match<DataRow>(() => brRow, b => (bool)b["IsRequired"], b => string.IsNullOrEmpty(
                    CommonFunctions.GetThePropertyValue(migratedObject, b["PropertyName"].ToString())));

            Then()
                .Do(ctx => migratedObject.validationMessage.Add("Required Field not found: " + brRow["PropertyName"]));
            
            
        }
    }
}
