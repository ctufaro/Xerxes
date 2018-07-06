using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace Xerxes.Utils
{
    public class UtilitiesDatabase
    {
       public static void Lazy(string pathToDatabase)
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
                    insertCommand.CommandText = "INSERT INTO message ( text ) VALUES ( $text )";
                    insertCommand.Parameters.AddWithValue("$text", "Hello, World!");
                    insertCommand.ExecuteNonQuery();

                    var selectCommand = connection.CreateCommand();
                    selectCommand.Transaction = transaction;
                    selectCommand.CommandText = "SELECT text FROM message";
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