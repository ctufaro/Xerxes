using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using System.Net;

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
                            //Console.WriteLine(message);
                        }
                    }

                    transaction.Commit();
                }
            }
        }
    
        public static List<IPEndPoint> GetSavedConnectionsFromDB(string pathToDatabase)
        {   
            List<IPEndPoint> ipEndPoints = new List<IPEndPoint>();
            
            using (var connection = new SqliteConnection("" + new SqliteConnectionStringBuilder { DataSource = pathToDatabase }))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    //TODO: refine select statement
                    var selectCommand = connection.CreateCommand();
                    selectCommand.Transaction = transaction;
                    selectCommand.CommandText = "SELECT IPAddress, Port FROM PEERS";
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(reader.GetString(0)), reader.GetInt32(1));
                            ipEndPoints.Add(endPoint);
                        }
                    }

                    transaction.Commit();
                }
            }

            return ipEndPoints;      
        }
    
    }
}