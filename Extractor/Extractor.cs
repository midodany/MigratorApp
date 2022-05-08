using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BusinessRulesEngine;
using BusinessRulesEngine.Entities;
using DataMigrator;
using DataMigrator.Entities;
using DataMigrator.Interfaces;
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
        }

        private void ExtractCourses(String BatchId)
        {
            var courseIntermediate = GetCourses(BatchId);
            WriteCourses(courseIntermediate);
        }

        private List<CourseIntermediate> GetCourses(String BatchId)
        {
            var sourceCourses = _dataReader.GetCourses();

            var ValidatedCourses = _engineValidator.ValidateMigratedObjects(sourceCourses.ToList<MigratedObject>(), "Subject", DomainEnum.Source);

            var acceptedCourses = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Source, ValidatedCourses);

            return acceptedCourses.OfType<CourseIntermediate>().ToList();

        }

        private void WriteCourses(List<CourseIntermediate> courses)
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
                select new CourseIntermediate
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
                            "VALUES('" + course.Title + "', N'" + course.Description + "', N'" + course.ExternalId + "', NULL, 0)";
                }

                using var updateCommand = new SqlCommand(query, myCon);
                updateCommand.ExecuteNonQuery();
            }

            myCon.Close();
        }
    }
}