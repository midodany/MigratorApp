using System;
namespace BusinessRulesEngine.Entities
{
    public class RelationRuleObject
    {
        public int id { set; get; }
        public int RelationId { set; get; }
        public int Type { set; get; }
        public int Count { set; get; }
        public bool IsActive { set; get; }
        public string Description { set; get; }
        public string RelationTable { set; get; }
        public string RelationSourceTable { set; get; }
        public string RelationTargetTable { set; get; }
        public string RelationSourceColumn { get; set; }
        public string RelationTargetColumn { get; set; }
    }
}

