using System;
using System.Collections.Generic;
using System.Text;
using BusinessRulesEngine.Entities;

namespace DataMigrator.Entities
{
    public class InstructorIntermediate : MigratedObject
    {
        public int? Id { get; set; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Email { set; get; }
        public DateTime BirthDate { set; get; }
        public string Gender {set; get; }
        public int Rating { set; get; }
        public string ExternalId { set; get; }
        public int? TargetId { set; get; }
        public bool ToBeDeleted { set; get; }
    }
}
