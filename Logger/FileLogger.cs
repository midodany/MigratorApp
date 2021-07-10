using System;
using System.IO;
using System.Threading.Tasks;

namespace Logger
{
    public static class FileLogger
    {
        public static async Task Log(string messege)
        {
            await using StreamWriter file = new StreamWriter("Log.txt", append: true);
            await file.WriteLineAsync(DateTime.Now + ": " +messege);
        }
    }
}
