using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessRulesEngine.Entities
{
    public class BatchEntity
    {
        public string BatchId { set; get; }
        public DateTime StartRunTime { set; get; }
        public bool FinishedSuccessfully { set; get; }
    }
}
