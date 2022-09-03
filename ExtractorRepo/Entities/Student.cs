using System;
using DataMigrator.Entities;

namespace ExtractorRepo.Entities
{
    public class Student : StudentIntermediate
    {
        public string NationalId { set; get; }
        public string Name { set; get; }
    }
}

