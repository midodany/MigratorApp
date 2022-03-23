using System.IO;
using Microsoft.Extensions.Configuration;

namespace Logger
{
    public class ConnectionStringManager
    {
        public string GetConnectionString(string connectionStringName)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, reloadOnChange: true);
            return builder.Build().GetSection("ConnectionStrings").GetSection(connectionStringName).Value;
        }
    }
}
