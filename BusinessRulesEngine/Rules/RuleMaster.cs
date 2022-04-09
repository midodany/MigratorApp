using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BusinessRulesEngine.Entities;

namespace BusinessRulesEngine.Rules
{
    internal abstract class RuleMaster
    {
        internal abstract void Apply(List<MigratedObject> migratedObject, DataRowCollection brRow);
    }
}
