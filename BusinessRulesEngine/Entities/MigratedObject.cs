using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessRulesEngine.Entities
{
    public abstract class MigratedObject
    {
        public int Id { get; set; }
        public List<ValidationLog> validationLogs = new List<ValidationLog>();
    }
}
