using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DataMigrator.Entities;

namespace DataMigrator
{
    public class DataMigrator
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        public void StartDataMigrator()
        {
            MigrateCourses();
        }

        private void MigrateCourses()
        {
            var courseIntermediate = GetIntermediateCourses();
            WriteCourses(courseIntermediate);
        }



        private List<CourseIntermediate> GetIntermediateCourses()
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            var query = "SELECT Id, Title, Description, ExternalId, TargetId, ToBeDeleted " +
                        "FROM dbo.Course " +
                        "Where ToBeDeleted = 0 OR (ToBeDeleted = 1 AND IsDeleted = 1) ";
            var objResult = new DataTable();
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var courses = (from DataRow dr in objResult.Rows
                select new CourseIntermediate
                {
                    Id = dr.Field<int>("Id"),
                    Title = dr["Title"].ToString(),
                    Description = dr["Description"].ToString(),
                    ExternalId = dr["ExternalId"].ToString(),
                    TargetId = dr.Field<int?>("TargetId"),
                    ToBeDeleted = dr.Field<bool>("ToBeDeleted")
                }).ToList();

            var validCourses = ValidateCourses(courses);

            return validCourses;
        }

        private List<CourseIntermediate> ValidateCourses(List<CourseIntermediate> courses)
        {
            var validCourses = new List<CourseIntermediate>();


            foreach (var course in courses)
            {

                var entity = "Course";
                using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
                var query = "SELECT Rules.Id, Name PropertyName, IsRequired, RegEx, Description " +
                            "FROM dbo.Entity " +
                            "INNER JOIN dbo.Property ON Property.EntityId = Entity.Id " +
                            "INNER JOIN dbo.Rules ON Rules.PropertyId = Property.Id " +
                            "WHERE Origin = 'Target' AND TableName = '" + entity + "'; ";

                using var myCommand = new SqlCommand(query, myCon);
                myCon.Open();

                var myReader = myCommand.ExecuteReader();
                var objResult = new DataTable();
                objResult.Load(myReader);

                var notValidReason = ValidateCourse(course, objResult.Rows);

                if (notValidReason == "")
                {
                    validCourses.Add(course);
                }
                else
                {
                    //add to not valid courses
                }

            }

            return validCourses;
        }

        private string ValidateCourse(CourseIntermediate course, DataRowCollection BRRows)
        {
            var reason = "";
            
            foreach (DataRow dr in BRRows)
            {
                
                var propertyValue = GetThePropertyValue(course, dr["PropertyName"].ToString());
                var isRequiredValid = !(bool) dr["IsRequired"] ||
                                        !string.IsNullOrEmpty(propertyValue);
                var isRegexValid = string.IsNullOrEmpty(dr["RegEx"].ToString()) ||
                                         ValidateRegex(dr["RegEx"].ToString(), propertyValue);
                reason += !(isRequiredValid && isRegexValid) ? dr["Description"] + "; " : "";
            }

            return reason;
        }

        private bool ValidateRegex(string strRegex, string input)
        {
            Regex re = new Regex(strRegex);

            return (input != null && re.IsMatch(input));
        }

        private string GetThePropertyValue(object instance, string propertyName)
        {
            Type type = instance.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            return propertyInfo != null ? propertyInfo.GetValue(instance, null).ToString() : "";
        }

        private void WriteCourses(List<CourseIntermediate> inputCourses)
        {
            var toBeDeletedCourses = inputCourses.Where(c => c.ToBeDeleted && c.TargetId != null).ToList();
            var toBeAddedCourses = inputCourses.Where(c => !c.ToBeDeleted && c.TargetId == null).ToList();
            var toBeUpdatedCourses = inputCourses.Where(c => !c.ToBeDeleted && c.TargetId != null).ToList();


            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("MigratedConnectionString"));
            myCon.Open();
            foreach (var course in toBeDeletedCourses)
            {
                var query = "DELETE dbo.Course "
                        + "WHERE Id = " + course.TargetId;
                using var myCommand = new SqlCommand(query, myCon);
                myCommand.ExecuteNonQuery();
            }

            foreach (var course in toBeAddedCourses)
            {
                var objResult = new DataTable();
                var query = "INSERT INTO dbo.Course " +
                        "(Title, Description) " +
                        "output INSERTED.Id V " +
                        "VALUES('" + course.Title + "', N'" + course.Description + "') " ;
                using var myCommand = new SqlCommand(query, myCon);
                var myReader = myCommand.ExecuteReader();
                objResult.Load(myReader);

                myReader.Close();
                var insertedId = objResult.Rows[0].Field<int>("V");
                course.TargetId = insertedId;
            }

            foreach (var course in toBeUpdatedCourses)
            {
                var query = "UPDATE dbo.Course " +
                            "SET Title = '"+course.Title+"', Description = '"+course.Description+"' " + 
                            "WHERE Id = " + course.TargetId;
                using var myCommand = new SqlCommand(query, myCon);
                myCommand.ExecuteNonQuery();
            }

            myCon.Close();

            using var IntermediateCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            IntermediateCon.Open();

            foreach (var course in toBeDeletedCourses)
            {
                var query = "UPDATE dbo.Course "+
                            "SET IsDeleted = 1 "+
                            "WHERE TargetId = " + course.TargetId;
                using var myCommand = new SqlCommand(query, IntermediateCon);
                myCommand.ExecuteNonQuery();
            }

            foreach (var course in toBeAddedCourses)
            {
                var query = "UPDATE dbo.Course " +
                            "SET TargetId = " + course.TargetId +
                            " WHERE Id = " + course.Id;
                using var myCommand = new SqlCommand(query, IntermediateCon);
                myCommand.ExecuteNonQuery();
            }

            IntermediateCon.Close();
        }

    }
}
