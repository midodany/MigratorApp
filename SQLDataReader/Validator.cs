using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DataMigrator;
using Logger;

namespace SQLDataReader
{
    public class Validator
    {
        private readonly ConnectionStringManager _connectionStringManager = new ConnectionStringManager();

        public ValidatorQuery GetValidatorQuery(List<string> entities)
        {
            ValidatorQuery result = new ValidatorQuery();
            StringBuilder whereClause = new StringBuilder();

            using var myCon = new SqlConnection(_connectionStringManager.GetConnectionString("BRSourceConnectionString"));
            myCon.Open();
            foreach (var entity in entities)
            {
                var query = "SELECT Name PropertyName, IsRequired, RegEx " +
                                    "FROM dbo.Entity " +
                                    "INNER JOIN dbo.Property ON Property.EntityId = Entity.Id " +
                                    "INNER JOIN dbo.Rules ON Rules.PropertyId = Property.Id " +
                                    "WHERE Origin = 'Source' AND TableName = '" + entity + "'; ";

                using var myCommand = new SqlCommand(query, myCon);
                var myReader = myCommand.ExecuteReader();
                var objResult = new DataTable();
                objResult.Load(myReader);

                foreach (DataRow dr in objResult.Rows)
                {
                    if ((bool) dr["IsRequired"])
                    {
                        whereClause.Append(" " + entity + "." + dr["PropertyName"] + " IS NOT NULL AND TRIM(CAST(" + entity + "." + dr["PropertyName"] + " AS NVARCHAR(MAX))) <> '' AND ");
                    }
                }


                myReader.Close();
            }

            if (whereClause.Length > 0)
            {
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            
            myCon.Close();

            result.WhereClause = whereClause.ToString();

            return result;
        }

    }
}
