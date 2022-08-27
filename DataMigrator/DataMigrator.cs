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
        public void StartDataMigrator(string BatchId)
        {
            MigrateCourses(BatchId);
            MigrateInstructors(BatchId);
        }

        private void MigrateCourses(string BatchId)
        {
            var courseIntermediate = GetIntermediateCourses(BatchId);
            WriteCourses(courseIntermediate);
        }

        private void MigrateInstructors(string BatchId)
        {
            var instructorIntermediate = GetIntermediateInstructors(BatchId);
            WriteInstructors(instructorIntermediate);
        }

        private List<CourseIntermediate> GetIntermediateCourses(string BatchId)
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            var query = "SELECT Id, Title, Description, ExternalId, TargetId, ToBeDeleted " +
                        "FROM dbo.Course " +
                        "Where (ToBeDeleted = 0 OR IsDeleted = 0) ";
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
                    MigrationId = dr["ExternalId"].ToString(),
                    Id = dr.Field<int?>("Id"),
                    Title = dr["Title"].ToString(),
                    Description = dr["Description"].ToString(),
                    ExternalId = dr["ExternalId"].ToString(),
                    TargetId = dr.Field<int?>("TargetId"),
                    ToBeDeleted = dr.Field<bool>("ToBeDeleted")
                }).ToList();

            var ValidatedCourses = _engineValidator.ValidateMigratedObjects(courses.ToList<MigratedObject>(), "Course",DomainEnum.Target);

            var acceptedCourses = CommonFunctions.ApplyFilter(BatchId,DomainEnum.Target,ValidatedCourses);

            return acceptedCourses.OfType<CourseIntermediate>().ToList();
        }

        private List<InstructorIntermediate> GetIntermediateInstructors(string BatchId)
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            var query = "SELECT Id, FirstName, LastName, Email, BirthDate, Gender, ToBeDeleted, ExternalId ,TargetId " +
                        "FROM dbo.Instructor " +
                        "Where (ToBeDeleted = 0 OR IsDeleted = 0) ";
            var objResult = new DataTable();
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var instructors = (from DataRow dr in objResult.Rows
                           select new InstructorIntermediate
                           {
                               MigrationId = dr["ExternalId"].ToString(),
                               Id = dr.Field<int?>("Id"),
                               FirstName = dr["FirstName"].ToString(),
                               LastName = dr["LastName"].ToString(),
                               Email = dr["Email"].ToString(),
                               BirthDate = DateTime.Parse(dr["BirthDate"].ToString()),
                               Gender = dr["Gender"].ToString(),
                               ExternalId = dr["ExternalId"].ToString(),
                               TargetId = dr.Field<int?>("TargetId"),
                               ToBeDeleted = dr.Field<bool>("ToBeDeleted")
                           }).ToList();

            var ValidatedInstructors = _engineValidator.ValidateMigratedObjects(instructors.ToList<MigratedObject>(), "Instructors", DomainEnum.Target);

            var acceptedInstructors = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Target, ValidatedInstructors);

            return acceptedInstructors.OfType<InstructorIntermediate>().ToList();
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

        private void WriteInstructors(List<InstructorIntermediate> inputInstructors)
        {
            var toBeDeletedInstructors = inputInstructors.Where(c => c.ToBeDeleted && c.TargetId != null).ToList();
            var toBeAddedInstructors = inputInstructors.Where(c => !c.ToBeDeleted && c.TargetId == null).ToList();
            var toBeUpdatedInstructors = inputInstructors.Where(c => !c.ToBeDeleted && c.TargetId != null).ToList();


            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("MigratedConnectionString"));
            myCon.Open();
            foreach (var instructor in toBeDeletedInstructors)
            {
                var query = "DELETE dbo.Instructor "
                        + "WHERE Id = " + instructor.TargetId;
                using var myCommand = new SqlCommand(query, myCon);
                myCommand.ExecuteNonQuery();
            }

            foreach (var instructor in toBeAddedInstructors)
            {
                var objResult = new DataTable();
                var query = "INSERT INTO dbo.Instructor " +
                        "(FirstName, LastName, Email, BirthDate, Gender, Rating) " +
                        "output INSERTED.Id V " +
                        "VALUES('" + instructor.FirstName + "', N'" + instructor.LastName + "', N'" + instructor.Email + "', N'" + instructor.BirthDate.ToString("MM/dd/yyyy") + "', N'" + instructor.Gender + "', N'" + instructor.Rating + "') ";
                using var myCommand = new SqlCommand(query, myCon);
                var myReader = myCommand.ExecuteReader();
                objResult.Load(myReader);

                myReader.Close();
                var insertedId = objResult.Rows[0].Field<int>("V");
                instructor.TargetId = insertedId;
            }

            foreach (var instructor in toBeUpdatedInstructors)
            {
                var query = "UPDATE dbo.Instructor " +
                            "SET FirstName = '" + instructor.FirstName + "', LastName = '" + instructor.LastName + "', Email = '" + instructor.Email +
                             "', BirthDate = '" + instructor.BirthDate.ToString("MM/dd/yyyy") + "', Gender = '" + instructor.Gender + "' " +
                            "WHERE Id = " + instructor.TargetId;
                using var myCommand = new SqlCommand(query, myCon);
                myCommand.ExecuteNonQuery();
            }

            myCon.Close();

            using var intermediateCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            intermediateCon.Open();

            foreach (var instructor in toBeDeletedInstructors)
            {
                var query = "UPDATE dbo.Instructor " +
                            "SET IsDeleted = 1 " +
                            "WHERE TargetId = " + instructor.TargetId;
                using var myCommand = new SqlCommand(query, intermediateCon);
                myCommand.ExecuteNonQuery();
            }

            foreach (var instructor in toBeAddedInstructors)
            {
                var query = "UPDATE dbo.Instructor " +
                            "SET TargetId = " + instructor.TargetId +
                            " WHERE Id = " + instructor.Id;
                using var myCommand = new SqlCommand(query, intermediateCon);
                myCommand.ExecuteNonQuery();
            }

            intermediateCon.Close();
        }


    }
}
