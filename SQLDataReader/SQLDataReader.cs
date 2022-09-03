using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DataMigrator;
using DataMigrator.Entities;
using ExtractorRepo;
using ExtractorRepo.Entities;
using Logger;

namespace SQLDataReader
{
    public class SqlDataReader : IDataExtractor
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();

        public List<Subject> GetCourses()
        {
            var objResult = new DataTable();
            var query = "SELECT Id, Title, Description " +
                                "FROM dbo.Subject ";

            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("DataSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var courses = (from DataRow dr in objResult.Rows
                           select new Subject
                           {
                               MigrationId = dr["Id"].ToString(),
                               Title = dr["Title"].ToString(),
                               Description = dr["Description"].ToString(),
                               ExternalId = dr["Id"].ToString()
                           }).ToList();
            return courses;
        }

        public List<Teacher> GetTeachers()
        {
            var objResult = new DataTable();
            var query = "SELECT Id, NationalId, Name, BirthDate, Gender  " +
                        "FROM dbo.Teacher ";

            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("DataSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            DateTime birthDate;

            var teachers = (from DataRow dr in objResult.Rows
                            select new Teacher()
                            {
                                MigrationId = dr["Id"].ToString(),
                                NationalId = dr["NationalId"].ToString(),
                                Gender = dr["Gender"].ToString(),
                                Name = dr["Name"].ToString(),
                                BirthDate = DateTime.TryParse(dr["BirthDate"].ToString(), out birthDate) ? birthDate : DateTime.MinValue,
                                ExternalId = dr["Id"].ToString()
                            }).ToList();
            return teachers;
        }

        public List<Student> GetStudents()
        {
            var objResult = new DataTable();
            var query = "SELECT Id, NationalId, Name, BirthDate, Gender  " +
                        "FROM dbo.Student ";

            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("DataSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            DateTime birthDate;

            var students = (from DataRow dr in objResult.Rows
                            select new Student()
                            {
                                MigrationId = dr["Id"].ToString(),
                                NationalId = dr["NationalId"].ToString(),
                                Gender = dr["Gender"].ToString(),
                                Name = dr["Name"].ToString(),
                                BirthDate = DateTime.TryParse(dr["BirthDate"].ToString(), out birthDate) ? birthDate : DateTime.MinValue,
                                ExternalId = dr["Id"].ToString()
                            }).ToList();
            return students;
        }

        public List<StudentSubject> GetStudentSubject()
        {
            var objResult = new DataTable();
            var query = "SELECT s.Id, sc.SubjectId FROM Student s " +
                        "INNER JOIN SubjectClass sc ON sc.ClassId = s.ClassId ";

            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("DataSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var studentSubject = (from DataRow dr in objResult.Rows
                            select new StudentSubject()
                            {
                                ExternalStudentId = dr["Id"].ToString(),
                                ExternalCourseId = dr["SubjectId"].ToString()
                            }).ToList();
            return studentSubject;
        }
    }
}