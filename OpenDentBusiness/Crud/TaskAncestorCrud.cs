//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class TaskAncestorCrud {
		///<summary>Gets one TaskAncestor object from the database using the primary key.  Returns null if not found.</summary>
		public static TaskAncestor SelectOne(long taskAncestorNum){
			string command="SELECT * FROM taskancestor "
				+"WHERE TaskAncestorNum = "+POut.Long(taskAncestorNum);
			List<TaskAncestor> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one TaskAncestor object from the database using a query.</summary>
		public static TaskAncestor SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<TaskAncestor> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of TaskAncestor objects from the database using a query.</summary>
		public static List<TaskAncestor> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<TaskAncestor> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<TaskAncestor> TableToList(DataTable table){
			List<TaskAncestor> retVal=new List<TaskAncestor>();
			TaskAncestor taskAncestor;
			foreach(DataRow row in table.Rows) {
				taskAncestor=new TaskAncestor();
				taskAncestor.TaskAncestorNum= PIn.Long  (row["TaskAncestorNum"].ToString());
				taskAncestor.TaskNum        = PIn.Long  (row["TaskNum"].ToString());
				taskAncestor.TaskListNum    = PIn.Long  (row["TaskListNum"].ToString());
				retVal.Add(taskAncestor);
			}
			return retVal;
		}

		///<summary>Converts a list of TaskAncestor into a DataTable.</summary>
		public static DataTable ListToTable(List<TaskAncestor> listTaskAncestors,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="TaskAncestor";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("TaskAncestorNum");
			table.Columns.Add("TaskNum");
			table.Columns.Add("TaskListNum");
			foreach(TaskAncestor taskAncestor in listTaskAncestors) {
				table.Rows.Add(new object[] {
					POut.Long  (taskAncestor.TaskAncestorNum),
					POut.Long  (taskAncestor.TaskNum),
					POut.Long  (taskAncestor.TaskListNum),
				});
			}
			return table;
		}

		///<summary>Inserts one TaskAncestor into the database.  Returns the new priKey.</summary>
		public static long Insert(TaskAncestor taskAncestor){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				taskAncestor.TaskAncestorNum=DbHelper.GetNextOracleKey("taskancestor","TaskAncestorNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(taskAncestor,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							taskAncestor.TaskAncestorNum++;
							loopcount++;
						}
						else{
							throw ex;
						}
					}
				}
				throw new ApplicationException("Insert failed.  Could not generate primary key.");
			}
			else {
				return Insert(taskAncestor,false);
			}
		}

		///<summary>Inserts one TaskAncestor into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(TaskAncestor taskAncestor,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				taskAncestor.TaskAncestorNum=ReplicationServers.GetKey("taskancestor","TaskAncestorNum");
			}
			string command="INSERT INTO taskancestor (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="TaskAncestorNum,";
			}
			command+="TaskNum,TaskListNum) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(taskAncestor.TaskAncestorNum)+",";
			}
			command+=
				     POut.Long  (taskAncestor.TaskNum)+","
				+    POut.Long  (taskAncestor.TaskListNum)+")";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				taskAncestor.TaskAncestorNum=Db.NonQ(command,true,"TaskAncestorNum","taskAncestor");
			}
			return taskAncestor.TaskAncestorNum;
		}

		///<summary>Inserts one TaskAncestor into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(TaskAncestor taskAncestor){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(taskAncestor,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					taskAncestor.TaskAncestorNum=DbHelper.GetNextOracleKey("taskancestor","TaskAncestorNum"); //Cacheless method
				}
				return InsertNoCache(taskAncestor,true);
			}
		}

		///<summary>Inserts one TaskAncestor into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(TaskAncestor taskAncestor,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO taskancestor (";
			if(!useExistingPK && isRandomKeys) {
				taskAncestor.TaskAncestorNum=ReplicationServers.GetKeyNoCache("taskancestor","TaskAncestorNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="TaskAncestorNum,";
			}
			command+="TaskNum,TaskListNum) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(taskAncestor.TaskAncestorNum)+",";
			}
			command+=
				     POut.Long  (taskAncestor.TaskNum)+","
				+    POut.Long  (taskAncestor.TaskListNum)+")";
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				taskAncestor.TaskAncestorNum=Db.NonQ(command,true,"TaskAncestorNum","taskAncestor");
			}
			return taskAncestor.TaskAncestorNum;
		}

		///<summary>Updates one TaskAncestor in the database.</summary>
		public static void Update(TaskAncestor taskAncestor){
			string command="UPDATE taskancestor SET "
				+"TaskNum        =  "+POut.Long  (taskAncestor.TaskNum)+", "
				+"TaskListNum    =  "+POut.Long  (taskAncestor.TaskListNum)+" "
				+"WHERE TaskAncestorNum = "+POut.Long(taskAncestor.TaskAncestorNum);
			Db.NonQ(command);
		}

		///<summary>Updates one TaskAncestor in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(TaskAncestor taskAncestor,TaskAncestor oldTaskAncestor){
			string command="";
			if(taskAncestor.TaskNum != oldTaskAncestor.TaskNum) {
				if(command!=""){ command+=",";}
				command+="TaskNum = "+POut.Long(taskAncestor.TaskNum)+"";
			}
			if(taskAncestor.TaskListNum != oldTaskAncestor.TaskListNum) {
				if(command!=""){ command+=",";}
				command+="TaskListNum = "+POut.Long(taskAncestor.TaskListNum)+"";
			}
			if(command==""){
				return false;
			}
			command="UPDATE taskancestor SET "+command
				+" WHERE TaskAncestorNum = "+POut.Long(taskAncestor.TaskAncestorNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(TaskAncestor,TaskAncestor) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(TaskAncestor taskAncestor,TaskAncestor oldTaskAncestor) {
			if(taskAncestor.TaskNum != oldTaskAncestor.TaskNum) {
				return true;
			}
			if(taskAncestor.TaskListNum != oldTaskAncestor.TaskListNum) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one TaskAncestor from the database.</summary>
		public static void Delete(long taskAncestorNum){
			string command="DELETE FROM taskancestor "
				+"WHERE TaskAncestorNum = "+POut.Long(taskAncestorNum);
			Db.NonQ(command);
		}

	}
}