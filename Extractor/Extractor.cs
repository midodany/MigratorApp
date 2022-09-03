using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
            ExtractStudents(BatchId);
            ExtractStudentCourse(BatchId);
        }

        private void ExtractCourses(String BatchId)
        {
            var subjects = GetSourceEntities<Subject>(BatchId, _dataReader.GetCourses);
            var properties = new List<string>() { "Title", "Description" };
            WriteEntities(subjects.OfType<MigratedObject>().ToList(), "Course", properties);
        }

        private void ExtractTeachers(String BatchId)
        {
            var teachers = GetSourceEntities<Teacher>(BatchId, _dataReader.GetTeachers);
            foreach (var teacher in teachers)
            {
                teacher.FirstName = teacher.Name.Substring(0, teacher.Name.IndexOf(" ", StringComparison.Ordinal));
                teacher.LastName = teacher.Name.Substring(teacher.Name.IndexOf(" ", StringComparison.Ordinal));
                teacher.Email = $"{teacher.NationalId}@ourProduct.com";
            }
            var properties = new List<string>() { "FirstName", "LastName", "Email", "BirthDate", "Gender" };
            WriteEntities(teachers.OfType<MigratedObject>().ToList(), "Instructor", properties);
        }

        private void ExtractStudents(String BatchId)
        {
            var students = GetSourceEntities<Student>(BatchId, _dataReader.GetStudents);
            foreach (var student in students)
            {
                student.FirstName = student.Name.Substring(0, student.Name.IndexOf(" ", StringComparison.Ordinal));
                student.LastName = student.Name.Substring(student.Name.IndexOf(" ", StringComparison.Ordinal));
                student.Email = $"{student.NationalId}@ourProduct.com";
            }
            var properties = new List<string>() { "FirstName", "LastName", "Email", "BirthDate", "Gender" };
            WriteEntities(students.OfType<MigratedObject>().ToList(), "Student", properties);
        }

        private void ExtractStudentCourse(String BatchId)
        {
            var studentCourse = GetSourceEntities<StudentSubject>(BatchId, _dataReader.GetStudentSubject);
            WriteEntities(studentCourse.OfType<MigratedObject>().ToList(), "StudentCourse", null, "ExternalStudentId,ExternalCourseId", "TargetStudentId,TargetCourseId");
        }

        private List<T> GetSourceEntities<T>(string BatchId, Func<List<T>> dataReaderFunction)
        {
            Console.WriteLine($"Extracting: {typeof(T).Name}");
            var sourceEntities = dataReaderFunction();

            var ValidatedEntities = _engineValidator.ValidateMigratedObjects(sourceEntities.OfType<MigratedObject>().ToList(), typeof(T).Name, DomainEnum.Source);

            var acceptedEntities = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Source, ValidatedEntities);

            return acceptedEntities.OfType<T>().ToList();
        }

        private void WriteEntities(List<MigratedObject> entities, string IntermediateEntityName, List<string> properties, string ExternalIdColumn = "ExternalId", string TargetIdColumn = "TargetId")
        {
            Console.WriteLine($"Transforming: {IntermediateEntityName}");
            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));

            var query = $"UPDATE {IntermediateEntityName} "
                        + "SET ToBeDeleted = 1 ";
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            myCommand.ExecuteNonQuery();

            query = $"SELECT {ExternalIdColumn} " +
                    $"FROM {IntermediateEntityName} ";

            using var selectCommand = new SqlCommand(query, myCon);
            var entitysReader = selectCommand.ExecuteReader();
            var entitysResult = new DataTable();
            entitysResult.Load(entitysReader);
            entitysReader.Close();

            List<string> existingEntities;
            if (ExternalIdColumn == "ExternalId")
            {
                existingEntities = (from DataRow dr in entitysResult.Rows
                                    select dr[ExternalIdColumn].ToString()).ToList();
            }
            else
            {
                existingEntities = (from DataRow dr in entitysResult.Rows
                                    select dr[ExternalIdColumn.Split(',')[0]].ToString() + "," + dr[ExternalIdColumn.Split(',')[1]].ToString()).ToList();
            }


            var iteration = 1;
            foreach (var entity in entities)
            {
                StringBuilder updateProperties = new StringBuilder();
                StringBuilder insertProperties = new StringBuilder();
                StringBuilder insertValues = new StringBuilder();
                StringBuilder whereValue = new StringBuilder();
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        var propertyValue = CommonFunctions.GetThePropertyValue(entity, property);
                        updateProperties.Append($" {property} = N'{propertyValue}' ,");

                        insertProperties.Append($" {property} ,");

                        insertValues.Append($" N'{propertyValue}' ,");
                    }
                }
                
                if (ExternalIdColumn == "ExternalId")
                {
                    updateProperties.Append($" {ExternalIdColumn} = N'{entity.GetUniqueExternalId()}' ,");

                    insertProperties.Append($" {ExternalIdColumn} , TargetId,");

                    insertValues.Append($" N'{entity.GetUniqueExternalId()}' ,");
                    insertValues.Append($" NULL, ");

                    whereValue.Append($"{ExternalIdColumn} =  '{entity.GetUniqueExternalId()}'");

                }
                else
                {
                    updateProperties.Append($" {ExternalIdColumn.Split(',')[0]} = N'{entity.GetUniqueExternalId().Split(',')[0]}' ,");
                    updateProperties.Append($" {ExternalIdColumn.Split(',')[1]} = N'{entity.GetUniqueExternalId().Split(',')[1]}' ,");

                    insertProperties.Append($" {ExternalIdColumn} ,");
                    insertProperties.Append($" {TargetIdColumn}, ");

                    insertValues.Append($" N'{entity.GetUniqueExternalId().Split(',')[0]}' ,");
                    insertValues.Append($" N'{entity.GetUniqueExternalId().Split(',')[1]}' ,");
                    insertValues.Append($" NULL, NULL, ");

                    whereValue.Append($"{ExternalIdColumn.Split(',')[0]} = '{entity.GetUniqueExternalId().Split(',')[0]}' ");
                    whereValue.Append($" AND {ExternalIdColumn.Split(',')[1]} = '{entity.GetUniqueExternalId().Split(',')[1]}' ");
                }


                if (existingEntities.Contains(entity.GetUniqueExternalId()))
                {
                    query = $"UPDATE {IntermediateEntityName} " +
                            $"SET {updateProperties} ToBeDeleted = 0 " +
                            $"WHERE {whereValue} ";
                }
                else
                {
                    query = $"INSERT INTO {IntermediateEntityName} " +
                            $"({insertProperties}  ToBeDeleted,IsDeleted) " +
                            $"VALUES({insertValues} 0, 0)";
                }

                using var updateCommand = new SqlCommand(query, myCon);
                updateCommand.ExecuteNonQuery();
                Console.WriteLine($"{IntermediateEntityName}: {iteration++}");
            }

            myCon.Close();
        }
    }
}