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
            MigrateStudents(BatchId);
            MigrateStudentCourse(BatchId);
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

        private void MigrateStudents(string BatchId)
        {
            var properties = new List<string>() { "Id", "FirstName", "LastName", "Email", "BirthDate", "Gender" };
            var relations = GetRelationObjectByEntityName("StudentCourse", "ExternalStudentId", "ExternalCourseId");
            var studentIntermediate = GetIntermediateEnteties<StudentIntermediate>(BatchId, "Student", properties, relations: relations);

            properties.RemoveAt(0);

            WriteEntities(studentIntermediate.OfType<MigratedObject>().ToList(), "Student", properties);
        }

        private void MigrateStudentCourse(string BatchId)
        {
            string[,] tablesToJoinColumns = { { "Student", "ExternalId" }, { "Course", "ExternalId" } };
            var studentCourseIntermediate = GetIntermediateEnteties<StudentCourseIntermediate>(BatchId, "StudentCourse", null, "ExternalStudentId,ExternalCourseId", "TargetStudentId,TargetCourseId",tablesToJoinColumns);


            WriteEntities(studentCourseIntermediate.OfType<MigratedObject>().ToList(), "StudentCourse", null, "ExternalStudentId,ExternalCourseId", "TargetStudentId,TargetCourseId", "StudentId,CourseId");
        }

        private List<T> GetIntermediateEnteties<T>(string BatchId, string IntermediateEntityName, List<string> properties,
            string ExternalIdColumn = "ExternalId", string TargetIdColumn = "TargetId", string[,] tablesToJoinColumns = null, List<RelationObject> relations = null) where T : MigratedObject, new()
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            var ExternalIdColumnNames = ExternalIdColumn.Split(',');
            var TargetIdColumnNames = TargetIdColumn.Split(',');
            string selectPropertiesQuery = string.Empty;
            string joinQuery = string.Empty;
            string whereJoinQuery = string.Empty;
            string targetIdSelectQuery = string.Empty;
            if (tablesToJoinColumns != null && ExternalIdColumnNames.Length > 1 && TargetIdColumnNames.Length > 1)
            {
                targetIdSelectQuery = $" t1.TargetId {TargetIdColumnNames[0]}, t2.TargetId {TargetIdColumnNames[1]}";
                joinQuery = $" INNER JOIN {tablesToJoinColumns[0, 0] } t1 ON a.{ExternalIdColumnNames[0]} = t1.{tablesToJoinColumns[0, 1]} ";
                joinQuery += $" INNER JOIN {tablesToJoinColumns[1, 0] } t2 ON a.{ExternalIdColumnNames[1]} = t2.{tablesToJoinColumns[1, 1]} ";
                whereJoinQuery = $" AND t1.ToBeDeleted = 0 AND t1.TargetId IS NOT NULL AND t2.ToBeDeleted = 0 AND t2.TargetId IS NOT NULL ";
            }
            else
            {
                targetIdSelectQuery = TargetIdColumn;
                if(properties != null)
                    selectPropertiesQuery = $"{string.Join(",", properties.Select(p => "a." + p))},";
            }

            var query = $"SELECT {selectPropertiesQuery} {string.Join(",", ExternalIdColumnNames.Select(c => "a." + c))}, {targetIdSelectQuery}, a.ToBeDeleted, a.IsDeleted " +
                        $"FROM {IntermediateEntityName} a " +
                        $"{joinQuery} " +
                        $"Where (a.ToBeDeleted = 0 OR a.IsDeleted = 0) {whereJoinQuery}";
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
                if(properties != null)
                {
                    foreach (var property in properties)
                    {
                        CommonFunctions.SetThePropertyValue(entity, property, row[property].ToString());
                    }
                }
                
                CommonFunctions.SetThePropertyValue(entity, ExternalIdColumnNames[0], row[ExternalIdColumnNames[0]]);
                CommonFunctions.SetThePropertyValue(entity, TargetIdColumnNames[0], row[TargetIdColumnNames[0]]);

                if (ExternalIdColumnNames.Length > 1 && TargetIdColumnNames.Length > 1)
                {
                    CommonFunctions.SetThePropertyValue(entity, ExternalIdColumnNames[1], row[ExternalIdColumnNames[1]]);
                    CommonFunctions.SetThePropertyValue(entity, TargetIdColumnNames[1], row[TargetIdColumnNames[1]]);
                }

                CommonFunctions.SetThePropertyValue(entity, "ToBeDeleted", row["ToBeDeleted"]);
                CommonFunctions.SetThePropertyValue(entity, "IsDeleted", row["IsDeleted"]);
                entity.MigrationId = entity.GetUniqueExternalId();
                entities.Add(entity);
            }

            
            var ValidatedEntities = _engineValidator.ValidateMigratedObjects(entities.ToList<MigratedObject>(), IntermediateEntityName, DomainEnum.Target);

            if(relations != null)
            {
                ValidatedEntities = _engineValidator.ValidateRelationForMigratedObject(ValidatedEntities, IntermediateEntityName, relations);
            }

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
            var toBeAddedEntities = inputEntities.Where(e => !bool.Parse(CommonFunctions.GetThePropertyValue(e, "ToBeDeleted")) && (!int.TryParse(CommonFunctions.GetThePropertyValue(e, TargetIdColumnsList[0]), out _) || bool.Parse(CommonFunctions.GetThePropertyValue(e, "IsDeleted"))) ).ToList();
            var toBeUpdatedEntities = inputEntities.Where(e => !bool.Parse(CommonFunctions.GetThePropertyValue(e, "ToBeDeleted")) && int.TryParse(CommonFunctions.GetThePropertyValue(e, TargetIdColumnsList[0]), out _) && !bool.Parse(CommonFunctions.GetThePropertyValue(e, "IsDeleted"))).ToList();


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
                if (properties != null && properties.Count > 0)
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
            var iterator = 1;
            foreach (var entity in toBeAddedEntities)
            {
                Console.WriteLine($"Adding {IntermediateEntityName}: {iterator++}");
                StringBuilder insertValues = new StringBuilder(" VALUES(");
                if(properties != null)
                {
                    foreach (var property in properties)
                    {
                        var propertyValue = CommonFunctions.GetThePropertyValue(entity, property);

                        insertValues.Append($" N'{propertyValue}',");
                    }
                }
                
                if (MigratedIdColumnsList.Count > 1)
                {
                    foreach (var TargetIdColumnsName in TargetIdColumnsList)
                    {
                        insertValues.Append($" N'{CommonFunctions.GetThePropertyValue(entity, TargetIdColumnsName)}',");
                    }
                }
                    
                insertValues.Length--;
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
                if (TargetIdColumnsList.Count == 1)
                {
                    var insertedId = objResult.Rows[0].Field<int>("V");
                    CommonFunctions.SetThePropertyValue(entity, TargetIdColumnsList[0], insertedId);
                }
            }

            if (MigratedIdColumnsList.Count == 1)
            {
                iterator = 1;
                foreach (var entity in toBeUpdatedEntities)
                {
                    Console.WriteLine($"Updating {IntermediateEntityName}: {iterator++}");
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
                                "SET IsDeleted = 1, ToBeDeleted = 1 " +
                                $"WHERE {TargetIdColumn} = {CommonFunctions.GetThePropertyValue(entity, TargetIdColumn)} ";
                    using var myCommand = new SqlCommand(query, intermediateCon);
                    myCommand.ExecuteNonQuery();
                }

                foreach (var entity in toBeAddedEntities)
                {
                    var query = $"UPDATE {IntermediateEntityName} " +
                                $"SET {TargetIdColumn} = {CommonFunctions.GetThePropertyValue(entity, TargetIdColumn)}, IsDeleted = 0 " +
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

                var query = $"UPDATE {IntermediateEntityName} " +
                            "SET IsDeleted = 1 ,ToBeDeleted = 1 ";
                using var myCommand = new SqlCommand(query, intermediateCon);
                myCommand.ExecuteNonQuery();

                var isDeletedWhereQuery = new StringBuilder();

                foreach (var entity in toBeAddedEntities)
                {
                    isDeletedWhereQuery.Append($" ({ExternalIdColumnsList[0]} = {CommonFunctions.GetThePropertyValue(entity, ExternalIdColumnsList[0])}");
                    isDeletedWhereQuery.Append($" AND {ExternalIdColumnsList[1]} = {CommonFunctions.GetThePropertyValue(entity, ExternalIdColumnsList[1])}) OR");
                }
                isDeletedWhereQuery.Length -= 2;

                query = $"UPDATE {IntermediateEntityName} " +
                            $"SET IsDeleted = 0 " +
                            $"WHERE {isDeletedWhereQuery} ";
                using var updateCommand = new SqlCommand(query, intermediateCon);
                updateCommand.ExecuteNonQuery();

                intermediateCon.Close();
            }

        }

        private List<RelationObject> GetRelationObjectByEntityName(string entityName,string sourceColumnName, string targetColumnName)
        {
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("IntermediateConnectionString"));
            string query = $"SELECT {sourceColumnName},{targetColumnName} FROM {entityName} ";

            var objResult = new DataTable();
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var relationRules = (from DataRow dr in objResult.Rows
                                 select new RelationObject
                                 {
                                     RelationTable = entityName,
                                     RelationSourceId = dr[sourceColumnName].ToString(),
                                     RelationTargetId = dr[targetColumnName].ToString()
                                 }).ToList();
            return relationRules;
        }

    }
}
