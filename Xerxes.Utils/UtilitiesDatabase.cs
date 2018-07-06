using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace Xerxes.Utils
{
    public class UtilitiesDatabase
    {
       public static void SaveGetFromDB(string pathToDatabase, string externalIP)
        {
            using (var connection = new SqliteConnection("" +
                new SqliteConnectionStringBuilder
                {
                    DataSource = pathToDatabase
                }))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var insertCommand = connection.CreateCommand();
                    insertCommand.Transaction = transaction;
                    insertCommand.CommandText = "INSERT INTO PEERS ( IPAddress, DateAdded ) VALUES ( $ipAddress, $dateAdded )";
                    insertCommand.Parameters.AddWithValue("$ipAddress", externalIP);
                    insertCommand.Parameters.AddWithValue("$dateAdded", DateTime.Now.ToString());
                    insertCommand.ExecuteNonQuery();

                    var selectCommand = connection.CreateCommand();
                    selectCommand.Transaction = transaction;
                    selectCommand.CommandText = "SELECT IPAddress FROM PEERS";
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = reader.GetString(0);
                            Console.WriteLine(message);
                        }
                    }

                    transaction.Commit();
                }
            }
        }

    }
}