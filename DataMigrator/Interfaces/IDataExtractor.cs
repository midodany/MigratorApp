using System.Collections.Generic;
using DataMigrator.Entities;

namespace DataMigrator.Interfaces
{
    public interface IDataExtractor
    {
        List<CourseIntermediate> GetCourses();
    }
}