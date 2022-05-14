using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessRulesEngine.Entities
{
    public abstract class MigratedObject
    {
        public string MigrationId { get; set; }
        public readonly List<ValidationLog> ValidationLogs = new List<ValidationLog>();
    }
}
