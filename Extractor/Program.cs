using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Extractor
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {
            Logger.Info("Starting Program...");
            
            //Use EncryptedConnectionString for an encrypted string.
            //Use ConnectionString for cleartext string
            string connString = GetConfig("ConnectionString");
            string cryptoConnString = GetConfig("EncryptedConnectionString");

            //Check if encrypted string is used.  If cleartext is used console print encrypted string.
            if (connString != "") Console.WriteLine(Crypto.EncryptString(connString));
            if (cryptoConnString != "") connString = Crypto.DecryptString(cryptoConnString);
            
            //get the data
            GetData(connString);

            Logger.Info("...End Program");
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Main|" + ex.Message);
            }
        }
        static string GetConfig(string keyStr)
        {
            string retval = "";
            try
            {
                IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("AppSettings.json", true, true)
              .Build();

                if (config[keyStr] != null)
                    retval = config[keyStr].ToString();
                
                return retval;
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "GetConfig|" + ex.Message);
                return retval;
            }

        }
        static void GetData(string connectStr)
        {
            try
            {
                using SqlConnection sql = new SqlConnection(connectStr);
                using SqlCommand cmd = new SqlCommand("spGetCaseAll", sql);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                sql.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    string filePath = GetConfig("DataPath");
                    string fileName = GetFileName();
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath,fileName)))
                    {
                        while (reader.Read())
                        {
                            string tempstring = DataToCSV(reader) + "\n";
                            outputFile.Write(tempstring);
                            Console.Write(tempstring); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetData|" + ex.Message);
            }
        }
        private static string DataToCSV(SqlDataReader reader)
        {
            string tempstring = "";
            
            for (int i = 0;i<reader.FieldCount; i++)
            {
                switch (reader[i].GetType().Name)
                {
                    case "DateTime": tempstring += "\"" + reader[i].ToString() + "\","; break;
                    case "Int32": tempstring += reader[i].ToString() + ","; break;
                    case "Int64": tempstring += reader[i].ToString() + ","; break;
                    case "Single": tempstring += reader[i].ToString() + ","; break;
                    case "Decimal": tempstring += reader[i].ToString() + ","; break;
                    case "Double": tempstring += reader[i].ToString() + ","; break;
                    case "Boolean": tempstring += reader[i].ToString() + ","; break;
                    case "String": tempstring += "\"" + reader[i].ToString() + "\","; break;
                    default:  break;
                }
            }
            return tempstring.Substring(0,tempstring.Length-1);
        }
        private static string GetFileName()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HHmm") + ".csv";
        }
    }
}
