using System;
using BusinessRulesEngine.Entities;

namespace DataMigrator.Entities
{
    public class StudentCourseIntermediate : MigratedObject
    {
        public string ExternalStudentId { set; get; }
        public string ExternalCourseId { set; get; }
        public int? TargetStudentId { set; get; }
        public int? TargetCourseId { set; get; }
        public bool ToBeDeleted { set; get; }
        public bool IsDeleted { set; get; }

        public override string GetUniqueExternalId()
        {
            return ExternalStudentId + "," + ExternalCourseId;
        }
    }
}

