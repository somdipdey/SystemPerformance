using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SystemPerformance
{
    //SQL Performance Counters-->
    /* Summary:
     sys.dm_os_performance_counters (Transact-SQL): https://docs.microsoft.com/en-us/sql/relational-databases/system-dynamic-management-views/sys-dm-os-performance-counters-transact-sql
     * Returns a row per performance counter maintained by the server.
     * Permission Required:
        On SQL Server, requires VIEW SERVER STATE permission.
        On SQL Database Premium Tiers, requires the VIEW DATABASE STATE permission in the database. On SQL Database Standard and Basic Tiers, requires the Server admin or an Azure Active Directory admin account.
     */
    public class SQL_Performance_Counters
    {

        //check if Performance Counter is Enabled or not
        public bool IsDisabled { get; set; }

        //fetch the list of Performance counters
        public List<SQL_Individual_Performance_Counter> Performance_Counters = new List<SQL_Individual_Performance_Counter>();



        public SQL_Performance_Counters(string SQLConnectionString)
        {

            Performance_Counters = fetch_Performance_Counters(SQLConnectionString);



            if (Performance_Counters.Count > 0)

                IsDisabled = false;

            else

                IsDisabled = true;

        }


        //fetch all the performance counters in SQL
        private List<SQL_Individual_Performance_Counter> fetch_Performance_Counters(string SQLConnectionString)
        {

            List<SQL_Individual_Performance_Counter> fetched_Counters_List = new List<SQL_Individual_Performance_Counter>();



            SqlConnection conn = new SqlConnection(SQLConnectionString);

            DataTable dt = new DataTable();

            SqlCommand comm = conn.CreateCommand();

            try
            {

                string SPROC = " SELECT object_name, counter_name, instance_name, cntr_value, cntr_type FROM sys.dm_os_performance_counters ";

                comm.CommandType = CommandType.Text;

                comm.CommandText = SPROC;

                comm.CommandTimeout = 500;

                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                adapter.Fill(dt);



                if (dt.Rows.Count > 0)
                {

                    fetched_Counters_List.Clear();

                    foreach (DataRow dr in dt.Rows)
                    {

                        SQL_Individual_Performance_Counter fetched_query = new SQL_Individual_Performance_Counter

                        {

                            object_name = dr["object_name"].ToString(),

                            counter_name = dr["counter_name"].ToString(),

                            instance_name = dr["instance_name"].ToString(),

                            cntr_value = long.Parse(dr["cntr_value"].ToString()),

                            cntr_type = int.Parse(dr["cntr_type"].ToString())

                        };

                        fetched_Counters_List.Add(fetched_query);

                    }

                }

            }

            catch (Exception ex) { }



            return fetched_Counters_List;

        }

    }


    /*Summary Details of SQL Performance Counter:
     * Column name    	Data type	       Description
       object_name	    nchar(128)	       Category to which this counter belongs.
       counter_name	    nchar(128)	       Name of the counter. To get more information about a counter, this is the name of the topic to select from the list of counters in Use SQL Server Objects.
       instance_name	nchar(128)	       Name of the specific instance of the counter. Often contains the database name.
       cntr_value	    bigint	           Current value of the counter.

        Note: For per-second counters, this value is cumulative. The rate value must be calculated by sampling the value at discrete time intervals. The difference between any two successive sample values is equal to the rate for the time interval used.
        cntr_type	int	Type of counter as defined by the Windows performance architecture. See WMI Performance Counter Types on MSDN or your Windows Server documentation for more information on performance counter types.
     */
    public class SQL_Individual_Performance_Counter
    {

        public string object_name { get; set; }

        public string counter_name { get; set; }

        public string instance_name { get; set; }

        public long cntr_value { get; set; }

        public int cntr_type { get; set; }

    }

    //<--SQL Performance Counters
}
