using System;
namespace BusinessRulesEngine.Entities
{
    public class RelationObject
    {
        public string RelationTable { set; get; }
        public string RelationSourceId { get; set; }
        public string RelationTargetId { get; set; }
    }
}

