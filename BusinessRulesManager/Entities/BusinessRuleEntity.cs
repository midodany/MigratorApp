namespace BusinessRulesManager.Entities
{
    public class BusinessRuleEntity
    {
        public int? RuleId { set; get; }
        public string EntityName { set; get; }
        public string PropertyName { set; get; }
        public bool? IsRequired { set; get; }
        public string? RegEx { set; get; }
        public string? Description { set; get; }
        public string Origin { set; get; }

    }
}
