using System;
using BusinessRulesEngine.Entities;

namespace DataMigrator.Entities
{
    public class StudentIntermediate : MigratedObject
    {
        public int? Id { get; set; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Email { set; get; }
        public DateTime BirthDate { set; get; }
        public string Gender { set; get; }
        public int Score { set; get; }
        public string ExternalId { set; get; }
        public int? TargetId { set; get; }
        public bool ToBeDeleted { set; get; }
        public bool IsDeleted { set; get; }

        public override string GetUniqueExternalId()
        {
            return ExternalId;
        }
    }
}

