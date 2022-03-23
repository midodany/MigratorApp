using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DataMigrator;
using DataMigrator.Entities;
using DataMigrator.Interfaces;
using Logger;
using SQLDataReader.Entities;

namespace SQLDataReader
{
    public class SqlDataReader : IDataExtractor
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();
        private readonly Validator _validator = new Validator();

        public List<CourseIntermediate> GetCourses()
        {
            var entitiesToValidate = new List<string> { EntitiesEnum.Subject };
            var validatorEntity = _validator.GetValidatorQuery(entitiesToValidate);
            var objResult = new DataTable();
            var query = "SELECT Id, Title, Description, TeacherId " +
                                "FROM dbo.Subject ";
            if(!string.IsNullOrEmpty(validatorEntity.WhereClause))
                query += "WHERE " + validatorEntity.WhereClause;
            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("DataSourceConnectionString"));
            myCon.Open();
            using var myCommand = new SqlCommand(query, myCon);
            var myReader = myCommand.ExecuteReader();
            objResult.Load(myReader);

            myReader.Close();
            myCon.Close();

            var courses = (from DataRow dr in objResult.Rows
                select new CourseIntermediate
                {
                    Title = dr["Title"].ToString(),
                    Description = dr["Description"].ToString(),
                    ExternalId = dr["Id"].ToString()
                }).ToList();
            return courses;
        }
    }
}