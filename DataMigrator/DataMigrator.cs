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
using System.Text;

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
            var properties = new List<string>() { "Id", "Title", "Description" };
            var courseIntermediate = GetIntermediateEnteties<CourseIntermediate>(BatchId, "Course", properties);

            properties.RemoveAt(0);

            WriteEntities(courseIntermediate.OfType<MigratedObject>().ToList(), "Course", properties);
        }

        private void MigrateInstructors(string BatchId)
        {
            var properties = new List<string>() { "Id", "FirstName", "LastName", "Email", "BirthDate", "Gender" };
            var instructorIntermediate = GetIntermediateEnteties<InstructorIntermediate>(BatchId, "Instructor", properties);

            properties.RemoveAt(0);

            WriteEntities(instructorIntermediate.OfType<MigratedObject>().ToList(), "Instructor", properties);
        }

        private List<T> GetIntermediateEnteties<T>(string BatchId, string IntermediateEntityName, List<string> properties,
            string ExternalIdColumn = "ExternalId", string TargetIdColumn = "TargetId", string[,] tablesToJoinColumns = null) where T : MigratedObject, new()
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            var ExternalIdColumnNames = ExternalIdColumn.Split(',');
            var TargetIdColumnNames = TargetIdColumn.Split(',');
            string joinQuery = string.Empty;
            string targetIdSelectQuery = string.Empty;
            if (tablesToJoinColumns != null && ExternalIdColumnNames.Length > 1 && TargetIdColumnNames.Length > 1)
            {
                targetIdSelectQuery = $", t1.TargetId {TargetIdColumnNames[0]}, t2.TargetId {TargetIdColumnNames[1]}";
                joinQuery = $" LEFT JOIN {tablesToJoinColumns[0, 0] } t1 ON a.{ExternalIdColumnNames[0]} = t1.{tablesToJoinColumns[0, 1]} ";
                joinQuery += $" LEFT JOIN {tablesToJoinColumns[1, 0] } t2 ON a.{ExternalIdColumnNames[1]} = t2.{tablesToJoinColumns[1, 1]} ";
            }
            else
            {
                targetIdSelectQuery = TargetIdColumn;
            }

            var query = $"SELECT {string.Join(",", properties.Select(p => "a." + p))}, {string.Join(",", ExternalIdColumnNames.Select(c => "a." + c))}, {targetIdSelectQuery}, a.ToBeDeleted " +
                        $"FROM {IntermediateEntityName} a " +
                        $"{joinQuery} " +
                        "Where (ToBeDeleted = 0 OR IsDeleted = 0) ";
            var objResult = new DataTable();
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var entities = new List<T>();

            foreach (DataRow row in objResult.Rows)
            {
                var entity = new T();
                foreach (var property in properties)
                {
                    CommonFunctions.SetThePropertyValue(entity, property, row[property].ToString());
                }
                CommonFunctions.SetThePropertyValue(entity, ExternalIdColumnNames[0], row[ExternalIdColumnNames[0]]);
                CommonFunctions.SetThePropertyValue(entity, TargetIdColumnNames[0], row[TargetIdColumnNames[0]]);

                if (ExternalIdColumnNames.Length > 1 && TargetIdColumnNames.Length > 1)
                {
                    CommonFunctions.SetThePropertyValue(entity, ExternalIdColumnNames[1], row[ExternalIdColumnNames[1]]);
                    CommonFunctions.SetThePropertyValue(entity, TargetIdColumnNames[1], row[TargetIdColumnNames[1]]);
                }

                CommonFunctions.SetThePropertyValue(entity, "ToBeDeleted", row["ToBeDeleted"]);
                entity.MigrationId = row["ExternalId"].ToString();
                entities.Add(entity);
            }

            var ValidatedEntities = _engineValidator.ValidateMigratedObjects(entities.ToList<MigratedObject>(), IntermediateEntityName, DomainEnum.Target);

            var acceptedEntities = CommonFunctions.ApplyFilter(BatchId, DomainEnum.Target, ValidatedEntities);

            return acceptedEntities.OfType<T>().ToList();
        }

        private void WriteEntities(List<MigratedObject> inputEntities, string IntermediateEntityName, List<string> properties, string ExternalIdColumn = "ExternalId",
            string TargetIdColumn = "TargetId", string MigratedIdColumn = "Id")
        {
            List<string> ExternalIdColumnsList = ExternalIdColumn.Split(',').ToList();
            List<string> TargetIdColumnsList = TargetIdColumn.Split(',').ToList();
            List<string> MigratedIdColumnsList = MigratedIdColumn.Split(',').ToList();

            var toBeDeletedEntities = inputEntities.Where(e => bool.Parse(CommonFunctions.GetThePropertyValue(e, "ToBeDeleted")) && int.TryParse(CommonFunctions.GetThePropertyValue(e, TargetIdColumnsList[0]), out _)).ToList();
            var toBeAddedEntities = inputEntities.Where(e => !bool.Parse(CommonFunctions.GetThePropertyValue(e, "ToBeDeleted")) && !int.TryParse(CommonFunctions.GetThePropertyValue(e, TargetIdColumnsList[0]), out _)).ToList();
            var toBeUpdatedEntities = inputEntities.Where(e => !bool.Parse(CommonFunctions.GetThePropertyValue(e, "ToBeDeleted")) && int.TryParse(CommonFunctions.GetThePropertyValue(e, TargetIdColumnsList[0]), out _)).ToList();


            using var myCon =
                new SqlConnection(_connectionStringManager.GetConnectionString("MigratedConnectionString"));
            myCon.Open();

            //reference Entity
            if (MigratedIdColumnsList.Count > 1)
            {
                toBeDeletedEntities.AddRange(toBeUpdatedEntities);
                toBeAddedEntities = toBeUpdatedEntities;
            }

            foreach (var entity in toBeDeletedEntities)
            {
                string whereCondition = $" WHERE {MigratedIdColumnsList[0]} = {CommonFunctions.GetThePropertyValue(entity, TargetIdColumnsList[0])} ";

                if (MigratedIdColumnsList.Count > 1)
                {
                    whereCondition += $" AND  {MigratedIdColumnsList[1]} = {CommonFunctions.GetThePropertyValue(entity, TargetIdColumnsList[1])} ";
                }

                var query = $"DELETE {IntermediateEntityName} "
                        + whereCondition;
                using var myCommand = new SqlCommand(query, myCon);
                myCommand.ExecuteNonQuery();
            }

            string insertQuery = string.Empty;
            var outputId = string.Empty;

            if (MigratedIdColumnsList.Count > 1)
            {
                if (properties.Count > 0)
                {
                    insertQuery = $" ({string.Join(',', properties)}, {MigratedIdColumn}) ";
                }
                else
                {
                    insertQuery += $" ({MigratedIdColumn}) ";
                }
            }
            else
            {
                insertQuery = $" ({string.Join(',', properties)}) ";
                outputId = "output INSERTED.Id V ";
            }

            foreach (var entity in toBeAddedEntities)
            {
                StringBuilder insertValues = new StringBuilder(" VALUES(");
                foreach (var property in properties)
                {
                    var propertyValue = CommonFunctions.GetThePropertyValue(entity, property);

                    insertValues.Append($" N'{propertyValue}' ");
                }
                if (properties.Count > 0)
                {
                    insertValues.Append(", ");
                }
                if (MigratedIdColumnsList.Count > 1)
                {
                    insertValues.Append($" N'{CommonFunctions.GetThePropertyValue(entity, MigratedIdColumnsList[0])}',  ");
                    insertValues.Append($" N'{CommonFunctions.GetThePropertyValue(entity, MigratedIdColumnsList[1])}' ");
                }
                foreach (var MigratedIdColumnName in MigratedIdColumnsList)
                {
                    insertValues.Append($" N'{CommonFunctions.GetThePropertyValue(entity, MigratedIdColumnName)}' ");
                }
                insertValues.Append(") ");

                var objResult = new DataTable();
                var query = $"INSERT INTO {IntermediateEntityName} " +
                        insertQuery +
                        outputId +
                        insertValues;
                using var myCommand = new SqlCommand(query, myCon);
                var myReader = myCommand.ExecuteReader();
                objResult.Load(myReader);

                myReader.Close();
                if (TargetIdColumnsList.Count > 1)
                {
                    CommonFunctions.SetThePropertyValue(entity, TargetIdColumnsList[0], insertValues);
                    CommonFunctions.SetThePropertyValue(entity, TargetIdColumnsList[1], insertValues);
                }
                else
                {
                    var insertedId = objResult.Rows[0].Field<int>("V");
                    CommonFunctions.SetThePropertyValue(entity, TargetIdColumnsList[0], insertValues);
                }
            }

            if (MigratedIdColumnsList.Count == 1)
            {
                foreach (var entity in toBeUpdatedEntities)
                {
                    StringBuilder updatequery = new StringBuilder();
                    foreach (var property in properties)
                    {
                        updatequery.Append($" {property} = '{CommonFunctions.GetThePropertyValue(entity, property)}' ,");
                    }
                    updatequery.Length--;


                    var query = $"UPDATE {IntermediateEntityName} " +
                                $"SET {updatequery} " +
                                $"WHERE {MigratedIdColumn} = " + CommonFunctions.GetThePropertyValue(entity, TargetIdColumn);
                    using var myCommand = new SqlCommand(query, myCon);
                    myCommand.ExecuteNonQuery();
                }

                myCon.Close();

                using var intermediateCon =
                    new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
                intermediateCon.Open();

                foreach (var entity in toBeDeletedEntities)
                {
                    var query = $"UPDATE {IntermediateEntityName} " +
                                "SET IsDeleted = 1 " +
                                $"WHERE {TargetIdColumn} = {CommonFunctions.GetThePropertyValue(entity, TargetIdColumn)} ";
                    using var myCommand = new SqlCommand(query, intermediateCon);
                    myCommand.ExecuteNonQuery();
                }

                foreach (var entity in toBeAddedEntities)
                {
                    var query = $"UPDATE {IntermediateEntityName} " +
                                $"SET {TargetIdColumn} = {CommonFunctions.GetThePropertyValue(entity, TargetIdColumn)} " +
                                $"WHERE {MigratedIdColumn} = " + CommonFunctions.GetThePropertyValue(entity, MigratedIdColumn);
                    using var myCommand = new SqlCommand(query, intermediateCon);
                    myCommand.ExecuteNonQuery();
                }

                intermediateCon.Close();
            }
            else
            {
                myCon.Close();

                using var intermediateCon =
                    new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
                intermediateCon.Open();

                var query = $"UPDATE {toBeUpdatedEntities} " +
                            "SET IsDeleted = 1 ";
                using var myCommand = new SqlCommand(query, intermediateCon);
                myCommand.ExecuteNonQuery();


                foreach (var entity in toBeAddedEntities)
                {
                    query = $"UPDATE {toBeUpdatedEntities} " +
                                $"SET IsDeleted = 0 " +
                                $"WHERE {TargetIdColumnsList[0]}  =  {CommonFunctions.GetThePropertyValue(entity, TargetIdColumnsList[0])} " +
                                $"AND  {TargetIdColumnsList[1]}  =  {CommonFunctions.GetThePropertyValue(entity, TargetIdColumnsList[1])} ";
                    using var updateCommand = new SqlCommand(query, intermediateCon);
                    updateCommand.ExecuteNonQuery();
                }

                intermediateCon.Close();
            }

        }

    }
}
