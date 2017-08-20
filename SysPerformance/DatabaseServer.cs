using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SystemPerformance
{
    //class to track basic 
    public class DatabaseServer
    {
        //check whether the connection to the database is established or not
        public bool IsConnected { get; set; }

        //check whether the specified database is locked or not
        public bool IsDatabaseLocked { get; set; }

        //track error rising through connection/queries to the database
        public string Error { get; set; }

        //can either call DatabaseServer(SQLConnectionString) or
        //call DatabaseServer(SQLConnectionString, DatabaseName). The DatabaseName paramter is optional here.
        public DatabaseServer(string SQLConnection_String, string DatabaseName = "")
        {

            if (DatabaseName == "")
                Error = "No database name provided.";

            IsConnected = isServerConnected(SQLConnection_String);

            if (DatabaseName.Trim() != "")
                IsDatabaseLocked = isDatabaseLocked(SQLConnection_String, DatabaseName);

        }

        private bool isServerConnected(string SQLConnection_String)
        {
            using (var db_Connection = new SqlConnection(SQLConnection_String))
            {
                try
                {
                    db_Connection.Open();
                    Error = "";
                    return true;
                }
                catch (SqlException err)
                {
                    Error = Error + " Server Connection Error: " + err.Message.ToString() + ".";
                    return false;
                }
            }
        }

        private bool isDatabaseLocked(string SQLConnection_String, string DatabaseName)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(SQLConnection_String))
                using (SqlCommand sqlCmd = new SqlCommand())
                {
                    sqlCmd.Connection = sqlConnection;
                    sqlCmd.CommandText =
                        @"select count(*)
                from sys.dm_tran_locks
                where resource_database_id = db_id(@database_name);";
                    sqlCmd.Parameters.Add(new SqlParameter("@database_name", SqlDbType.NVarChar, 128)
                    {
                        Value = DatabaseName
                    });

                    sqlConnection.Open();
                    int sessionCount = Convert.ToInt32(sqlCmd.ExecuteScalar());

                    if (sessionCount > 0)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception err)
            {
                Error = Error + " Database Error: " + err.Message.ToString() + ".";
                return false;
            }
        }

    }
}
