using System;
using System.Collections.Generic;
using System.Text;

namespace Logger
{
    public  class LogObject
    {
        public string objectId { set; get; }
        public int? RuleId { set; get; }
        public string ValidationMessage { set; get; }
        public string Domain { set; get; }
        public  string TableName { set; get; }
        public  string PropertyName { set; get; }
        public bool IsActive { set; get; }
        public int? RuleType { set; get; }
    }
}
