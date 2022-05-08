using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessRulesEngine.Entities
{
    public class ValidationLog
    {
        public string objectId { set; get; }
        public int ruleId { set; get; }
        public string validationMessage { set; get; }
    }
}
