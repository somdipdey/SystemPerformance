# SystemPerformance
Track System Performance (CPU / RAM / Memory / SQL Server) using this NuGet Package. Requires .Net 3.5+ framework. It is an Open Source project under MIT License.

#Permissions Required::
To monitor performance of SQL Server Objects you need the following permissions:
On SQL Server, requires VIEW SERVER STATE permission.
On SQL Database Premium Tiers, requires the VIEW DATABASE STATE permission in the database. On SQL Database Standard and Basic Tiers, requires the Server admin or an Azure Active Directory admin account.

#Using The Package::

To fetch performance of the device on which the assembly is running use the PerformanceTracker class. Refer to following example:

	var thisDevice = new PerformanceTracker();
	thisDevice.Current_CPU_Usage; //returns a floating number denoting CPU usage of the device on which the assembly is being executed
	thisDevice.Percent_RAM_Used; //returns a floating number to denote the percentage of RAM being used on the device on which the assembly is being executed	

Database Server Performance Tracker Example:

	var track_DBServer_Performance = new DB_Server_PerformanceTracker("MyConnectionString");
	track_DBServer_Performance.Top20_Expensive_Queries; //returns a list of top 20 most expensive QUeries on the database server

Check the state of the Database Server using DatabaseServer class as follows:

	var myDatabase = new DatabaseServer("MyConnectionString", "MyDatabaseName");
	myDatabase.IsConnected; //returns a boolean to denote whether the database connection is alright or not
	myDatabase.IsDatabaseLocked; //returns a boolean to denoted whether the database is locked for use or not

Check each class in the project to discover many more features and ways to keep track of CPU / RAM / Disk (Memory) / SQL Server performance, all from one package only.