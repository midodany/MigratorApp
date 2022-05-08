using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessRulesEngine.Entities
{
    public abstract class MigratedObject
    {
        public string MigrationId { get; set; }
        public List<ValidationLog> validationLogs = new List<ValidationLog>();
    }
}
