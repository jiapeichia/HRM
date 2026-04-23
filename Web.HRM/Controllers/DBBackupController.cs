using System.Web.Mvc;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Collections.Generic;
using System;

namespace Web.HRM.Controllers
{
    public class DBBackupController : AuthController
    {
        public void BackupDatabase()
        {
            string connectionString = WebConfigurationManager.ConnectionStrings["WebConnectionString"].ToString();// "Data Source=YourServer;Initial Catalog=master;Integrated Security=True";
            string databaseName = "BeYou";
            List<string> backupFilePaths = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Assuming you have a table named BackupPaths with a column named Path
                string query = "SELECT * FROM BackupPath";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string path = reader["Path"].ToString() + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day 
                            + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + "_" 
                            + reader["FileName"].ToString() + ".bak";
                        backupFilePaths.Add(path);
                    }
                }
            }
            foreach (string backupFilePath in backupFilePaths)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    
                    string backupScript = $"BACKUP DATABASE {databaseName} TO DISK = '{backupFilePath}' WITH INIT, FORMAT, NAME = 'BeYou'";

                    using (SqlCommand command = new SqlCommand(backupScript, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}