using System;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Extractor
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            Logger.Info("Starting Program...");
            
            //Use EncryptedConnectionString for an encrypted string.
            //Use ConnectionString for cleartext string
            string connString = GetConfig("ConnectionString");
            string cryptoConnString = GetConfig("EncryptedConnectionString");

            //Check if encrypted string is used.  If cleartext is used console print encrypted string.
            if (connString != "") Console.WriteLine(Crypto.EncryptString(connString));
            if (cryptoConnString != "") connString = Crypto.DecryptString(cryptoConnString);
            
            //console print the data
            GetData(connString);
            Logger.Info("...End Program");
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
                Logger.Error(ex, "GetConfig");
                return retval;
            }

        }
        static void GetData(string connectStr)
        {
            try
            {
                using SqlConnection sql = new SqlConnection(connectStr);
                using SqlCommand cmd = new SqlCommand("spGetProviderAll", sql);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                sql.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.Write(DataToString(reader) + "\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetData");
            }
        }
        private static string DataToString(SqlDataReader reader)
        {
            return reader["Provider_ID"].ToString() + "," +
                reader["Provider"].ToString() + "," +
                reader["Practice"].ToString() + "," +
                reader["Specialty_ID"].ToString() + "," +
                reader["Specialty_Name"].ToString() + "," +
                reader["Address_1"].ToString() + "," +
                reader["Address_2"].ToString() + "," +
                reader["City"].ToString() + "," +
                reader["State"].ToString() + "," +
                reader["Postal_Code"].ToString() + "," +
                reader["Attention"].ToString() + "," +
                reader["Notes"].ToString() + "," +
                reader["Inactive"].ToString();

        }
    }
}
