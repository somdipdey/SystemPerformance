using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace SystemPerformance
{

    /*This class tracks performance of different components
     */
    public class DB_Server_PerformanceTracker
    {
        //holds the top 20 SQL Queries which uses the most resources on SQL Server
        public List<Expensive_Query> Top20_Expensive_Queries = new List<Expensive_Query>();

        //holds a list of SQL Queries which uses the most resources on SQL Server. The number of items in the list is set in the DB_Server_PerformanceTracker class constructor
        public List<Expensive_Query> TopCustom_Expensive_Queries = new List<Expensive_Query>();

        //holds a list of sys.dm_os_memory_clerks objects using different resouces
        public List<SQLServer_Memory_Clerks> All_SQLServer_Memory_Clerks = new List<SQLServer_Memory_Clerks>();

        //holds a list of sys.dm_os_buffer_descriptors to view/track memory (buffer) usage by SQL server
        public List<SQLServer_Buffer_Usage> SQLServer_Memory_Buffer_Usage = new List<SQLServer_Buffer_Usage>();



        public DB_Server_PerformanceTracker(string SQLConnectionString, int Num_Query_Returned = 40)
        {

            Top20_Expensive_Queries = fetch_Top20_Expensive_Queries(SQLConnectionString);

            TopCustom_Expensive_Queries = fetch_TopCustom_Expensive_Queries(SQLConnectionString, Num_Query_Returned);

            All_SQLServer_Memory_Clerks = fetch_All_SQLServer_Memory_Clerks(SQLConnectionString);

            SQLServer_Memory_Buffer_Usage = fetch_SQLServer_Buffer_Usage(SQLConnectionString);

        }


        //returns a list of top 20 SQL queries which are using the most resources.
        private List<Expensive_Query> fetch_Top20_Expensive_Queries(string SQLConnectionString)
        {

            List<Expensive_Query> fetched_Query_list = new List<Expensive_Query>();



            SqlConnection conn = new SqlConnection(SQLConnectionString);

            DataTable dt = new DataTable();

            SqlCommand comm = conn.CreateCommand();

            try
            {

                string SPROC = " SELECT TOP 20 " +

                                       " qs.sql_handle," +

                                       " qs.execution_count," +

                                       " qs.total_worker_time AS Total_CPU," +

                                       " total_CPU_inSeconds = " +

                                       " qs.total_worker_time/1000000," +

                                       " average_CPU_inSeconds = " +

                                       " (qs.total_worker_time/1000000) / qs.execution_count," +

                                       " qs.total_elapsed_time," +

                                       " total_elapsed_time_inSeconds = " +

                                       " qs.total_elapsed_time/1000000," +

                                       " st.text," +

                                       " qp.query_plan" +

                                   " FROM" +

                                       " sys.dm_exec_query_stats AS qs" +

                                           " CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st" +

                                           " CROSS apply sys.dm_exec_query_plan (qs.plan_handle) AS qp" +

                                   " ORDER BY qs.total_worker_time DESC";

                comm.CommandType = CommandType.Text;

                comm.CommandText = SPROC;

                comm.CommandTimeout = 500;

                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                adapter.Fill(dt);



                if (dt.Rows.Count > 0)
                {

                    fetched_Query_list.Clear();



                    foreach (DataRow dr in dt.Rows)
                    {

                        Expensive_Query fetched_query = new Expensive_Query

                        {

                            Sql_Handle = Encoding.ASCII.GetBytes(dr["sql_handle"].ToString()),

                            Execution_Count = long.Parse(dr["execution_count"].ToString().Trim()),

                            Total_CPU_inMicroSeconds = long.Parse(dr["Total_CPU"].ToString().Trim()),

                            Total_CPU_inSeconds = long.Parse(dr["total_CPU_inSeconds"].ToString().Trim()),

                            Average_CPU_inSeconds = float.Parse(dr["average_CPU_inSeconds"].ToString().Trim()),

                            Total_Elapsed_Time_inMicroSeconds = long.Parse(dr["total_elapsed_time"].ToString().Trim()),

                            Total_Elapsed_Time_inSeconds = long.Parse(dr["total_elapsed_time_inSeconds"].ToString().Trim()),

                            Sql_Text = dr["text"].ToString(),

                            Query_Plan = dr["query_plan"].ToString()

                        };

                        fetched_Query_list.Add(fetched_query);

                    }

                }

            }

            catch (Exception ex) { }



            return fetched_Query_list;

        }


        //returns a list of SQL queries which are using the most resources. The number of queries returned is user definable and passed through parameter: Num_Query_Returned
        private List<Expensive_Query> fetch_TopCustom_Expensive_Queries(string SQLConnectionString, int Num_Query_Returned = 40)
        {

            List<Expensive_Query> fetched_Query_list = new List<Expensive_Query>();



            SqlConnection conn = new SqlConnection(SQLConnectionString);

            DataTable dt = new DataTable();

            SqlCommand comm = conn.CreateCommand();

            try
            {

                string SPROC = " SELECT TOP " + Num_Query_Returned.ToString().Trim() + " " +

                                       " qs.sql_handle," +

                                       " qs.execution_count," +

                                       " qs.total_worker_time AS Total_CPU," +

                                       " total_CPU_inSeconds = " +

                                       " qs.total_worker_time/1000000," +

                                       " average_CPU_inSeconds = " +

                                       " (qs.total_worker_time/1000000) / qs.execution_count," +

                                       " qs.total_elapsed_time," +

                                       " total_elapsed_time_inSeconds = " +

                                       " qs.total_elapsed_time/1000000," +

                                       " st.text," +

                                       " qp.query_plan" +

                                   " FROM" +

                                       " sys.dm_exec_query_stats AS qs" +

                                           " CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st" +

                                           " CROSS apply sys.dm_exec_query_plan (qs.plan_handle) AS qp" +

                                   " ORDER BY qs.total_worker_time DESC";

                comm.CommandType = CommandType.Text;

                comm.CommandText = SPROC;

                comm.CommandTimeout = 500;

                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                adapter.Fill(dt);



                if (dt.Rows.Count > 0)
                {

                    fetched_Query_list.Clear();



                    foreach (DataRow dr in dt.Rows)
                    {

                        Expensive_Query fetched_query = new Expensive_Query

                        {

                            Sql_Handle = Encoding.ASCII.GetBytes(dr["sql_handle"].ToString()),

                            Execution_Count = long.Parse(dr["execution_count"].ToString().Trim()),

                            Total_CPU_inMicroSeconds = long.Parse(dr["Total_CPU"].ToString().Trim()),

                            Total_CPU_inSeconds = long.Parse(dr["total_CPU_inSeconds"].ToString().Trim()),

                            Average_CPU_inSeconds = float.Parse(dr["average_CPU_inSeconds"].ToString().Trim()),

                            Total_Elapsed_Time_inMicroSeconds = long.Parse(dr["total_elapsed_time"].ToString().Trim()),

                            Total_Elapsed_Time_inSeconds = long.Parse(dr["total_elapsed_time_inSeconds"].ToString().Trim()),

                            Sql_Text = dr["text"].ToString(),

                            Query_Plan = dr["query_plan"].ToString()

                        };

                        fetched_Query_list.Add(fetched_query);

                    }

                }

            }

            catch (Exception ex) { }



            return fetched_Query_list;

        }



        /*Summary: 
         sys.dm_os_memory_clerks (Transact-SQL)
         Returns the set of all memory clerks that are currently active in the instance of SQL Server.
         Permissions Required:
         On SQL Server, requires VIEW SERVER STATE permission.
         On SQL Database Premium Tiers, requires the VIEW DATABASE STATE permission in the database. On SQL Database Standard and Basic Tiers, requires the Server admin or an Azure Active Directory admin account.
         
         * 
         * Column name	                Data type	    Description
           memory_clerk_address	        varbinary(8)	Specifies the unique memory address of the memory clerk. This is the primary key column. Is not nullable.
           type	                        nvarchar(60)	Specifies the type of memory clerk. Every clerk has a specific type, such as CLR Clerks MEMORYCLERK_SQLCLR. Is not nullable.
           name	                        nvarchar(256)	Specifies the internally assigned name of this memory clerk. A component can have several memory clerks of a specific type. A component might choose to use specific names to identify memory clerks of the same type. Is not nullable.
           memory_node_id	            smallint	    Specifies the ID of the memory node. Not nullable.
           single_pages_kb	            bigint	        Applies to: SQL Server 2008 through SQL Server 2008 R2.
           pages_kb	                    bigint	        Applies to: SQL Server 2012 through SQL Server 2017.
                                                        Specifies the amount of page memory allocated in kilobytes (KB) for this memory clerk. Is not nullable.
           
           multi_pages_kb	            bigint	        Applies to: SQL Server 2008 through SQL Server 2008 R2.
                                                        Amount of multipage memory allocated in KB. This is the amount of memory allocated by using the multiple page allocator of the memory nodes. This memory is allocated outside the buffer pool and takes advantage of the virtual allocator of the memory nodes. Is not nullable.

           virtual_memory_reserved_kb   bigint	        Specifies the amount of virtual memory that is reserved by a memory clerk. Is not nullable.
           virtual_memory_committed_kb  bigint	        Specifies the amount of virtual memory that is committed by a memory clerk. The amount of committed memory should always be less than the amount of reserved memory. Is not nullable.
           awe_allocated_kb	            bigint	        Specifies the amount of memory in kilobytes (KB) locked in the physical memory and not paged out by the operating system. Is not nullable.
           shared_memory_reserved_kb    bigint	        Specifies the amount of shared memory that is reserved by a memory clerk. The amount of memory reserved for use by shared memory and file mapping. Is not nullable.
           shared_memory_committed_kb   bigint	        Specifies the amount of shared memory that is committed by the memory clerk. Is not nullable.
           page_size_in_bytes	        bigint	        Specifies the granularity of the page allocation for this memory clerk. Is not nullable.
           page_allocator_address	    varbinary(8)	Specifies the address of the page allocator. This address is unique for a memory clerk and can be used in sys.dm_os_memory_objects to locate memory objects that are bound to this clerk. Is not nullable.
           host_address	                varbinary(8)	Specifies the memory address of the host for this memory clerk. For more information, see sys.dm_os_hosts (Transact-SQL). Components, such as Microsoft SQL Server Native Client, access SQL Server memory resources through the host interface.
                                                        0x00000000 = Memory clerk belongs to SQL Server.
                                                        Is not nullable.
         */
        private List<SQLServer_Memory_Clerks> fetch_All_SQLServer_Memory_Clerks(string SQLConnectionString)
        {

            List<SQLServer_Memory_Clerks> fetched_list = new List<SQLServer_Memory_Clerks>();



            SqlConnection conn = new SqlConnection(SQLConnectionString);

            DataTable dt = new DataTable();

            SqlCommand comm = conn.CreateCommand();

            try
            {

                string SPROC = " SELECT * FROM sys.dm_os_memory_clerks ORDER BY (pages_kb + virtual_memory_committed_kb + virtual_memory_reserved_kb +  awe_allocated_kb) desc ";

                comm.CommandType = CommandType.Text;

                comm.CommandText = SPROC;

                comm.CommandTimeout = 500;

                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                adapter.Fill(dt);



                if (dt.Rows.Count > 0)
                {

                    fetched_list.Clear();



                    foreach (DataRow dr in dt.Rows)
                    {

                        SQLServer_Memory_Clerks fetched_item = new SQLServer_Memory_Clerks

                        {

                            memory_clerk_address = dr.Field<byte[]>("memory_clerk_address"),

                            type = dr.Field<string>("type"),

                            name = dr.Field<string>("name"),

                            memory_node_id = int.Parse(dr["memory_node_id"].ToString().Trim()),

                            pages_kb = dr.Field<long>("pages_kb"),

                            virtual_memory_reserved_kb = dr.Field<long>("virtual_memory_reserved_kb"),

                            virtual_memory_committed_kb = dr.Field<long>("virtual_memory_committed_kb"),

                            awe_allocated_kb = dr.Field<long>("awe_allocated_kb"),

                            shared_memory_reserved_kb = dr.Field<long>("shared_memory_reserved_kb"),

                            shared_memory_committed_kb = dr.Field<long>("shared_memory_committed_kb"),

                            page_size_in_bytes = dr.Field<long>("page_size_in_bytes"),

                            page_allocator_address = dr.Field<byte[]>("page_allocator_address"),

                            host_address = dr.Field<byte[]>("host_address")

                        };

                        fetched_list.Add(fetched_item);

                    }

                }

            }

            catch (Exception ex) { }



            return fetched_list;

        }


        /*Summary:
         Querying sys.dm_os_buffer_descriptors will show how much memory (buffer) is used by SQL Server.
         * Permissions Required: VIEW_SERVER_STATE
         * This method fetches all the details of database instances using memory.
         */
        private List<SQLServer_Buffer_Usage> fetch_SQLServer_Buffer_Usage(string SQLConnectionString)
        {

            List<SQLServer_Buffer_Usage> fetched_list = new List<SQLServer_Buffer_Usage>();



            SqlConnection conn = new SqlConnection(SQLConnectionString);

            DataTable dt = new DataTable();

            SqlCommand comm = conn.CreateCommand();

            try
            {

                string SPROC = " DECLARE @total_buffer INT; " +



                               " SELECT @total_buffer = cntr_value " +

                               " FROM sys.dm_os_performance_counters  " +

                               " WHERE RTRIM([object_name]) LIKE '%Buffer Manager' " +

                               " AND counter_name = 'Database Pages'; " +



                               " ;WITH src AS " +

                               " ( " +

                               " SELECT  " +

                               " database_id, db_buffer_pages = COUNT_BIG(*) " +

                               " FROM sys.dm_os_buffer_descriptors " +

                               " GROUP BY database_id " +

                               " ) " +

                               " SELECT " +

                               " [db_name] = CASE [database_id] WHEN 32767  " +

                               " THEN 'Resource DB'  " +

                               " ELSE DB_NAME([database_id]) END, " +

                               " db_buffer_pages, " +

                               " db_buffer_MB = db_buffer_pages / 128, " +

                               " db_buffer_percent = CONVERT(DECIMAL(6,3),  " +

                               " db_buffer_pages * 100.0 / @total_buffer) " +

                               " FROM src " +

                               " ORDER BY db_buffer_MB DESC;  ";

                comm.CommandType = CommandType.Text;

                comm.CommandText = SPROC;

                comm.CommandTimeout = 500;

                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(comm);

                adapter.Fill(dt);



                if (dt.Rows.Count > 0)
                {

                    fetched_list.Clear();



                    foreach (DataRow dr in dt.Rows)
                    {

                        SQLServer_Buffer_Usage fetched_item = new SQLServer_Buffer_Usage

                        {

                            db_name = dr.Field<string>("db_name"),

                            db_buffer_pages = long.Parse(dr["db_buffer_pages"].ToString().Trim()),

                            db_buffer_MB = long.Parse(dr["db_buffer_MB"].ToString().Trim()),

                            db_buffer_percent = float.Parse(dr["db_buffer_percent"].ToString().Trim())

                        };

                        fetched_list.Add(fetched_item);

                    }

                }

            }

            catch (Exception ex) { }



            return fetched_list;

        }

    }

    //Expensive_Query class holds all the relevant variables required to view/track SQL Queries which use most resources
    public class Expensive_Query
    {

        public byte[] Sql_Handle { get; set; }

        public long Execution_Count { get; set; }

        public long Total_CPU_inMicroSeconds { get; set; }

        public long Total_CPU_inSeconds { get; set; }

        public float Average_CPU_inSeconds { get; set; }

        public long Total_Elapsed_Time_inMicroSeconds { get; set; }

        public long Total_Elapsed_Time_inSeconds { get; set; }

        public string Sql_Text { get; set; }

        public string Query_Plan { get; set; }

    }


    //class to hold all the relavant variables of os_memory_clerks
    public class SQLServer_Memory_Clerks
    {

        public byte[] memory_clerk_address { get; set; }

        public string type { get; set; }

        public string name { get; set; }

        public int memory_node_id { get; set; }

        public long pages_kb { get; set; }

        public long virtual_memory_reserved_kb { get; set; }

        public long virtual_memory_committed_kb { get; set; }

        public long awe_allocated_kb { get; set; }

        public long shared_memory_reserved_kb { get; set; }

        public long shared_memory_committed_kb { get; set; }

        public long page_size_in_bytes { get; set; }

        public byte[] page_allocator_address { get; set; }

        public byte[] host_address { get; set; }

    }


    //class to hold all the relevant variables of os_buffer_descriptors
    public class SQLServer_Buffer_Usage
    {

        public string db_name { get; set; }

        public long db_buffer_pages { get; set; }

        public long db_buffer_MB { get; set; }

        public float db_buffer_percent { get; set; }

    }

}
