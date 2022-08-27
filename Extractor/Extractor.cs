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
            //ExtractInstructorCourses(BatchId);
        }

        private void ExtractCourses(String BatchId)
        {
            var subjects = GetSourceEntities<Subject>(BatchId, _dataReader.GetCourses);
            var properties = new List<string>() { "Title", "Description" };
            WriteEntities(subjects.OfType<MigratedObject>().ToList(), "Course", properties);
            //WriteCourses(subjects);
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
            //WriteTeachers(teachers);
        }

        private List<T> GetSourceEntities<T>(string BatchId, Func<List<T>> dataReaderFunction)
        {
            var sourceEntities = dataReaderFunction();

            var ValidatedEntities = _engineValidator.ValidateMigratedObjects(sourceEntities.OfType<MigratedObject>().ToList(), typeof(T).Name, DomainEnum.Source);

            var acceptedEntities = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Source, ValidatedEntities);

            return acceptedEntities.OfType<T>().ToList();
        }

        private void WriteEntities(List<MigratedObject> entities, string IntermediateEntityName, List<string> properties, string ExternalIdColumn = "ExternalId")
        {
            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));

            var query = $"UPDATE {IntermediateEntityName} "
                        + "SET ToBeDeleted = 1 ";
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            myCommand.ExecuteNonQuery();

            query = $"SELECT {ExternalIdColumn} " +
                    $"FROM {IntermediateEntityName} " +
                    "WHERE IsDeleted = 0";

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



            foreach (var entity in entities)
            {
                StringBuilder updateProperties = new StringBuilder();
                StringBuilder insertProperties = new StringBuilder();
                StringBuilder insertValues = new StringBuilder();
                foreach (var property in properties)
                {
                    var propertyValue = CommonFunctions.GetThePropertyValue(entity, property);
                    updateProperties.Append($" {property} = N'{propertyValue}' ,");

                    insertProperties.Append($" {property} ,");

                    insertValues.Append($" N'{propertyValue}' ,");
                }

                if (ExternalIdColumn == "ExternalId")
                {
                    updateProperties.Append($" {ExternalIdColumn} = N'{entity.GetUniqueExternalId()}' ,");

                    insertProperties.Append($" {ExternalIdColumn} ,");

                    insertValues.Append($" N'{entity.GetUniqueExternalId()}' ,");
                }
                else
                {
                    updateProperties.Append($" {ExternalIdColumn.Split(',')[0]} = N'{entity.GetUniqueExternalId().Split(',')[0]}' ,");
                    updateProperties.Append($" {ExternalIdColumn.Split(',')[1]} = N'{entity.GetUniqueExternalId().Split(',')[1]}' ,");

                    insertProperties.Append($" {ExternalIdColumn} ,");

                    insertValues.Append($" N'{entity.GetUniqueExternalId().Split(',')[0]}' ,");
                    insertValues.Append($" N'{entity.GetUniqueExternalId().Split(',')[1]}' ,");
                }


                if (existingEntities.Contains(entity.GetUniqueExternalId()))
                {
                    query = $"UPDATE {IntermediateEntityName} " +
                            $"SET {updateProperties} ToBeDeleted = 0 " +
                            $"WHERE ExternalId =  '{entity.GetUniqueExternalId()}' ";
                }
                else
                {
                    query = $"INSERT INTO {IntermediateEntityName} " +
                            $"({insertProperties}  TargetId, ToBeDeleted) " +
                            $"VALUES({insertValues} , NULL, 0, 0)";
                }

                using var updateCommand = new SqlCommand(query, myCon);
                updateCommand.ExecuteNonQuery();
            }

            myCon.Close();
        }
    }
}