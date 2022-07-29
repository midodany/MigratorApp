using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BusinessRulesEngine;
using BusinessRulesEngine.Entities;
using DataMigrator;
using DataMigrator.Entities;
using ExtractorRepo;
using ExtractorRepo.Entities;
using Logger;

namespace Extractor
{
    public class Extractor
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        private readonly IDataExtractor _dataReader = new SQLDataReader.SqlDataReader();
        private readonly Validation _engineValidator = new Validation();

        public void StartExtractor(String BatchId)
        {
            ExtractCourses(BatchId);
            ExtractTeachers(BatchId);
        }

        private void ExtractCourses(String BatchId)
        {
            var subjects = GetCourses(BatchId);
            WriteCourses(subjects);
        }

        private void ExtractTeachers(String BatchId)
        {
            var teachers = GetTeachers(BatchId);
            WriteTeachers(teachers);
        }

        private List<Subject> GetCourses(String BatchId)
        {
            var sourceCourses = _dataReader.GetCourses();

            var ValidatedCourses = _engineValidator.ValidateMigratedObjects(sourceCourses.ToList<MigratedObject>(), "Subject", DomainEnum.Source);

            var acceptedCourses = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Source, ValidatedCourses);

            return acceptedCourses.OfType<Subject>().ToList();

        }

        private List<Teacher> GetTeachers(String BatchId)
        {
            var sourceTeachers = _dataReader.GetTeachers();

            var ValidatedTeachers = _engineValidator.ValidateMigratedObjects(sourceTeachers.ToList<MigratedObject>(), "Teacher", DomainEnum.Source);

            var acceptedTeachers = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Source, ValidatedTeachers);

            return acceptedTeachers.OfType<Teacher>().ToList();

        }

        private void WriteCourses(List<Subject> courses)
        {
            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));

            var query = "UPDATE dbo.Course "
                        + "SET ToBeDeleted = 1 ";
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            myCommand.ExecuteNonQuery();

            query = "SELECT ExternalId " +
                    "FROM dbo.Course " +
                    "WHERE IsDeleted = 0";

            using var selectCommand = new SqlCommand(query, myCon);
            var coursesReader = selectCommand.ExecuteReader();
            var coursesResult = new DataTable();
            coursesResult.Load(coursesReader);
            coursesReader.Close();

            var existingCourses = (from DataRow dr in coursesResult.Rows
                select new Subject
                {
                    ExternalId = dr["ExternalId"].ToString()
                }).ToList();

            foreach (var course in courses)
            {
                if (existingCourses.Count(c => c.ExternalId == course.ExternalId) > 0)
                {
                    query = "UPDATE dbo.Course " +
                            "SET Title = '" + course.Title + "', Description = '" + course.Description +
                            "', ToBeDeleted = 0 " +
                            "WHERE ExternalId = " + course.ExternalId + " ";
                }
                else
                {
                    query = "INSERT INTO dbo.Course " +
                            "(Title, Description, ExternalId, TargetId, ToBeDeleted) " +
                            "VALUES('" + course.Title + "', N'" + course.Description + "', N'" + course.ExternalId + "', NULL, 0, 0)";
                }

                using var updateCommand = new SqlCommand(query, myCon);
                updateCommand.ExecuteNonQuery();
            }

            myCon.Close();
        }
        private void WriteTeachers(List<Teacher> teachers)
        {
            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));

            var query = "UPDATE dbo.Instructor "
                        + "SET ToBeDeleted = 1 ";
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            myCommand.ExecuteNonQuery();

            query = "SELECT ExternalId " +
                    "FROM dbo.Instructor " +
                    "WHERE IsDeleted = 0";

            using var selectCommand = new SqlCommand(query, myCon);
            var teachersReader = selectCommand.ExecuteReader();
            var teachersResult = new DataTable();
            teachersResult.Load(teachersReader);
            teachersReader.Close();

            var existingTeachers = (from DataRow dr in teachersResult.Rows
                                   select new Teacher
                                   {
                                       ExternalId = dr["ExternalId"].ToString()
                                   }).ToList();

            foreach (var teacher in teachers)
            {
                if (existingTeachers.Count(c => c.ExternalId == teacher.ExternalId) > 0)
                {
                    query = "UPDATE dbo.Instructor " +
                            "SET FirstName = '" + teacher.Name.Substring(0,teacher.Name.IndexOf(" ", StringComparison.Ordinal)) +
                            "', LastName = '" + teacher.Name.Substring(teacher.Name.IndexOf(" ", StringComparison.Ordinal)) +
                            "', Email = '" + teacher.Id + "@ourProduct.com" +
                            "', BirthDate = '" + teacher.BirthDate +
                            "', Gender = '" + teacher.Gender +
                            "', ToBeDeleted = 0 " +
                            "WHERE ExternalId = " + teacher.ExternalId + " ";
                }
                else
                {
                    query = "INSERT INTO dbo.Instructor " +
                            "(Title, Description, ExternalId, TargetId, ToBeDeleted) " +
                            "VALUES('" + teacher.Name.Substring(0, teacher.Name.IndexOf(" ", StringComparison.Ordinal)) 
                            + "', N'" + teacher.Name.Substring(teacher.Name.IndexOf(" ", StringComparison.Ordinal)) 
                            + "', N'" + teacher.Id + "@ourProduct.com" 
                            + "', N'" + teacher.BirthDate 
                            + "', N'" + teacher.Gender 
                            + "', N'" + "0" 
                            + "', N'" + teacher.ExternalId + "', NULL, 0, 0)";
                }

                using var updateCommand = new SqlCommand(query, myCon);
                updateCommand.ExecuteNonQuery();
            }

            myCon.Close();
        }
    }
}