using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DataMigrator.Entities;
using BusinessRulesEngine;
using BusinessRulesEngine.Entities;
using Logger;

namespace DataMigrator
{
    public class DataMigrator
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        private readonly Validation _engineValidator = new Validation();
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

            var validCourses = _engineValidator.ValidateMigratedObjects(courses.ToList<MigratedObject>(), "Course").OfType<CourseIntermediate>().ToList();

            return validCourses;
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

            using var intermediateCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            intermediateCon.Open();

            foreach (var course in toBeDeletedCourses)
            {
                var query = "UPDATE dbo.Course "+
                            "SET IsDeleted = 1 "+
                            "WHERE TargetId = " + course.TargetId;
                using var myCommand = new SqlCommand(query, intermediateCon);
                myCommand.ExecuteNonQuery();
            }

            foreach (var course in toBeAddedCourses)
            {
                var query = "UPDATE dbo.Course " +
                            "SET TargetId = " + course.TargetId +
                            " WHERE Id = " + course.Id;
                using var myCommand = new SqlCommand(query, intermediateCon);
                myCommand.ExecuteNonQuery();
            }

            intermediateCon.Close();
        }

    }
}
