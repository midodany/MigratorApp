using System.Collections.Generic;
using ExtractorRepo.Entities;

namespace ExtractorRepo
{
    public interface IDataExtractor
    {
        List<Subject> GetCourses();
        List<Teacher> GetTeachers();
    }
}