using System;
using System.Collections.Generic;
using System.Text;
using DataMigrator.Entities;

namespace ExtractorRepo.Entities
{
    public class Teacher : InstructorIntermediate
    {
        public string NationalId { set; get; }
        public string Name { set; get; }
    }
}
