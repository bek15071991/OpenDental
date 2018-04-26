//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class JobLogCrud {
		///<summary>Gets one JobLog object from the database using the primary key.  Returns null if not found.</summary>
		public static JobLog SelectOne(long jobLogNum){
			string command="SELECT * FROM joblog "
				+"WHERE JobLogNum = "+POut.Long(jobLogNum);
			List<JobLog> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one JobLog object from the database using a query.</summary>
		public static JobLog SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<JobLog> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of JobLog objects from the database using a query.</summary>
		public static List<JobLog> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<JobLog> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<JobLog> TableToList(DataTable table){
			List<JobLog> retVal=new List<JobLog>();
			JobLog jobLog;
			foreach(DataRow row in table.Rows) {
				jobLog=new JobLog();
				jobLog.JobLogNum      = PIn.Long  (row["JobLogNum"].ToString());
				jobLog.JobNum         = PIn.Long  (row["JobNum"].ToString());
				jobLog.UserNumChanged = PIn.Long  (row["UserNumChanged"].ToString());
				jobLog.UserNumExpert  = PIn.Long  (row["UserNumExpert"].ToString());
				jobLog.UserNumEngineer= PIn.Long  (row["UserNumEngineer"].ToString());
				jobLog.DateTimeEntry  = PIn.DateT (row["DateTimeEntry"].ToString());
				jobLog.Description    = PIn.String(row["Description"].ToString());
				jobLog.MainRTF        = PIn.String(row["MainRTF"].ToString());
				jobLog.Title          = PIn.String(row["Title"].ToString());
				jobLog.RequirementsRTF= PIn.String(row["RequirementsRTF"].ToString());
				retVal.Add(jobLog);
			}
			return retVal;
		}

		///<summary>Converts a list of JobLog into a DataTable.</summary>
		public static DataTable ListToTable(List<JobLog> listJobLogs,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="JobLog";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("JobLogNum");
			table.Columns.Add("JobNum");
			table.Columns.Add("UserNumChanged");
			table.Columns.Add("UserNumExpert");
			table.Columns.Add("UserNumEngineer");
			table.Columns.Add("DateTimeEntry");
			table.Columns.Add("Description");
			table.Columns.Add("MainRTF");
			table.Columns.Add("Title");
			table.Columns.Add("RequirementsRTF");
			foreach(JobLog jobLog in listJobLogs) {
				table.Rows.Add(new object[] {
					POut.Long  (jobLog.JobLogNum),
					POut.Long  (jobLog.JobNum),
					POut.Long  (jobLog.UserNumChanged),
					POut.Long  (jobLog.UserNumExpert),
					POut.Long  (jobLog.UserNumEngineer),
					POut.DateT (jobLog.DateTimeEntry,false),
					            jobLog.Description,
					            jobLog.MainRTF,
					            jobLog.Title,
					            jobLog.RequirementsRTF,
				});
			}
			return table;
		}

		///<summary>Inserts one JobLog into the database.  Returns the new priKey.</summary>
		public static long Insert(JobLog jobLog){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				jobLog.JobLogNum=DbHelper.GetNextOracleKey("joblog","JobLogNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(jobLog,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							jobLog.JobLogNum++;
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
				return Insert(jobLog,false);
			}
		}

		///<summary>Inserts one JobLog into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(JobLog jobLog,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				jobLog.JobLogNum=ReplicationServers.GetKey("joblog","JobLogNum");
			}
			string command="INSERT INTO joblog (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="JobLogNum,";
			}
			command+="JobNum,UserNumChanged,UserNumExpert,UserNumEngineer,DateTimeEntry,Description,MainRTF,Title,RequirementsRTF) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(jobLog.JobLogNum)+",";
			}
			command+=
				     POut.Long  (jobLog.JobNum)+","
				+    POut.Long  (jobLog.UserNumChanged)+","
				+    POut.Long  (jobLog.UserNumExpert)+","
				+    POut.Long  (jobLog.UserNumEngineer)+","
				+    DbHelper.Now()+","
				+"'"+POut.String(jobLog.Description)+"',"
				+    DbHelper.ParamChar+"paramMainRTF,"
				+"'"+POut.String(jobLog.Title)+"',"
				+    DbHelper.ParamChar+"paramRequirementsRTF)";
			if(jobLog.MainRTF==null) {
				jobLog.MainRTF="";
			}
			OdSqlParameter paramMainRTF=new OdSqlParameter("paramMainRTF",OdDbType.Text,POut.StringParam(jobLog.MainRTF));
			if(jobLog.RequirementsRTF==null) {
				jobLog.RequirementsRTF="";
			}
			OdSqlParameter paramRequirementsRTF=new OdSqlParameter("paramRequirementsRTF",OdDbType.Text,POut.StringParam(jobLog.RequirementsRTF));
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramMainRTF,paramRequirementsRTF);
			}
			else {
				jobLog.JobLogNum=Db.NonQ(command,true,"JobLogNum","jobLog",paramMainRTF,paramRequirementsRTF);
			}
			return jobLog.JobLogNum;
		}

		///<summary>Inserts one JobLog into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(JobLog jobLog){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(jobLog,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					jobLog.JobLogNum=DbHelper.GetNextOracleKey("joblog","JobLogNum"); //Cacheless method
				}
				return InsertNoCache(jobLog,true);
			}
		}

		///<summary>Inserts one JobLog into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(JobLog jobLog,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO joblog (";
			if(!useExistingPK && isRandomKeys) {
				jobLog.JobLogNum=ReplicationServers.GetKeyNoCache("joblog","JobLogNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="JobLogNum,";
			}
			command+="JobNum,UserNumChanged,UserNumExpert,UserNumEngineer,DateTimeEntry,Description,MainRTF,Title,RequirementsRTF) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(jobLog.JobLogNum)+",";
			}
			command+=
				     POut.Long  (jobLog.JobNum)+","
				+    POut.Long  (jobLog.UserNumChanged)+","
				+    POut.Long  (jobLog.UserNumExpert)+","
				+    POut.Long  (jobLog.UserNumEngineer)+","
				+    DbHelper.Now()+","
				+"'"+POut.String(jobLog.Description)+"',"
				+    DbHelper.ParamChar+"paramMainRTF,"
				+"'"+POut.String(jobLog.Title)+"',"
				+    DbHelper.ParamChar+"paramRequirementsRTF)";
			if(jobLog.MainRTF==null) {
				jobLog.MainRTF="";
			}
			OdSqlParameter paramMainRTF=new OdSqlParameter("paramMainRTF",OdDbType.Text,POut.StringParam(jobLog.MainRTF));
			if(jobLog.RequirementsRTF==null) {
				jobLog.RequirementsRTF="";
			}
			OdSqlParameter paramRequirementsRTF=new OdSqlParameter("paramRequirementsRTF",OdDbType.Text,POut.StringParam(jobLog.RequirementsRTF));
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command,paramMainRTF,paramRequirementsRTF);
			}
			else {
				jobLog.JobLogNum=Db.NonQ(command,true,"JobLogNum","jobLog",paramMainRTF,paramRequirementsRTF);
			}
			return jobLog.JobLogNum;
		}

		///<summary>Updates one JobLog in the database.</summary>
		public static void Update(JobLog jobLog){
			string command="UPDATE joblog SET "
				+"JobNum         =  "+POut.Long  (jobLog.JobNum)+", "
				+"UserNumChanged =  "+POut.Long  (jobLog.UserNumChanged)+", "
				+"UserNumExpert  =  "+POut.Long  (jobLog.UserNumExpert)+", "
				+"UserNumEngineer=  "+POut.Long  (jobLog.UserNumEngineer)+", "
				//DateTimeEntry not allowed to change
				+"Description    = '"+POut.String(jobLog.Description)+"', "
				+"MainRTF        =  "+DbHelper.ParamChar+"paramMainRTF, "
				+"Title          = '"+POut.String(jobLog.Title)+"', "
				+"RequirementsRTF=  "+DbHelper.ParamChar+"paramRequirementsRTF "
				+"WHERE JobLogNum = "+POut.Long(jobLog.JobLogNum);
			if(jobLog.MainRTF==null) {
				jobLog.MainRTF="";
			}
			OdSqlParameter paramMainRTF=new OdSqlParameter("paramMainRTF",OdDbType.Text,POut.StringParam(jobLog.MainRTF));
			if(jobLog.RequirementsRTF==null) {
				jobLog.RequirementsRTF="";
			}
			OdSqlParameter paramRequirementsRTF=new OdSqlParameter("paramRequirementsRTF",OdDbType.Text,POut.StringParam(jobLog.RequirementsRTF));
			Db.NonQ(command,paramMainRTF,paramRequirementsRTF);
		}

		///<summary>Updates one JobLog in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(JobLog jobLog,JobLog oldJobLog){
			string command="";
			if(jobLog.JobNum != oldJobLog.JobNum) {
				if(command!=""){ command+=",";}
				command+="JobNum = "+POut.Long(jobLog.JobNum)+"";
			}
			if(jobLog.UserNumChanged != oldJobLog.UserNumChanged) {
				if(command!=""){ command+=",";}
				command+="UserNumChanged = "+POut.Long(jobLog.UserNumChanged)+"";
			}
			if(jobLog.UserNumExpert != oldJobLog.UserNumExpert) {
				if(command!=""){ command+=",";}
				command+="UserNumExpert = "+POut.Long(jobLog.UserNumExpert)+"";
			}
			if(jobLog.UserNumEngineer != oldJobLog.UserNumEngineer) {
				if(command!=""){ command+=",";}
				command+="UserNumEngineer = "+POut.Long(jobLog.UserNumEngineer)+"";
			}
			//DateTimeEntry not allowed to change
			if(jobLog.Description != oldJobLog.Description) {
				if(command!=""){ command+=",";}
				command+="Description = '"+POut.String(jobLog.Description)+"'";
			}
			if(jobLog.MainRTF != oldJobLog.MainRTF) {
				if(command!=""){ command+=",";}
				command+="MainRTF = "+DbHelper.ParamChar+"paramMainRTF";
			}
			if(jobLog.Title != oldJobLog.Title) {
				if(command!=""){ command+=",";}
				command+="Title = '"+POut.String(jobLog.Title)+"'";
			}
			if(jobLog.RequirementsRTF != oldJobLog.RequirementsRTF) {
				if(command!=""){ command+=",";}
				command+="RequirementsRTF = "+DbHelper.ParamChar+"paramRequirementsRTF";
			}
			if(command==""){
				return false;
			}
			if(jobLog.MainRTF==null) {
				jobLog.MainRTF="";
			}
			OdSqlParameter paramMainRTF=new OdSqlParameter("paramMainRTF",OdDbType.Text,POut.StringParam(jobLog.MainRTF));
			if(jobLog.RequirementsRTF==null) {
				jobLog.RequirementsRTF="";
			}
			OdSqlParameter paramRequirementsRTF=new OdSqlParameter("paramRequirementsRTF",OdDbType.Text,POut.StringParam(jobLog.RequirementsRTF));
			command="UPDATE joblog SET "+command
				+" WHERE JobLogNum = "+POut.Long(jobLog.JobLogNum);
			Db.NonQ(command,paramMainRTF,paramRequirementsRTF);
			return true;
		}

		///<summary>Returns true if Update(JobLog,JobLog) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(JobLog jobLog,JobLog oldJobLog) {
			if(jobLog.JobNum != oldJobLog.JobNum) {
				return true;
			}
			if(jobLog.UserNumChanged != oldJobLog.UserNumChanged) {
				return true;
			}
			if(jobLog.UserNumExpert != oldJobLog.UserNumExpert) {
				return true;
			}
			if(jobLog.UserNumEngineer != oldJobLog.UserNumEngineer) {
				return true;
			}
			//DateTimeEntry not allowed to change
			if(jobLog.Description != oldJobLog.Description) {
				return true;
			}
			if(jobLog.MainRTF != oldJobLog.MainRTF) {
				return true;
			}
			if(jobLog.Title != oldJobLog.Title) {
				return true;
			}
			if(jobLog.RequirementsRTF != oldJobLog.RequirementsRTF) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one JobLog from the database.</summary>
		public static void Delete(long jobLogNum){
			string command="DELETE FROM joblog "
				+"WHERE JobLogNum = "+POut.Long(jobLogNum);
			Db.NonQ(command);
		}

		///<summary>Inserts, updates, or deletes database rows to match supplied list.  Returns true if db changes were made.</summary>
		public static bool Sync(List<JobLog> listNew,List<JobLog> listDB) {
			//Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
			List<JobLog> listIns    =new List<JobLog>();
			List<JobLog> listUpdNew =new List<JobLog>();
			List<JobLog> listUpdDB  =new List<JobLog>();
			List<JobLog> listDel    =new List<JobLog>();
			listNew.Sort((JobLog x,JobLog y) => { return x.JobLogNum.CompareTo(y.JobLogNum); });//Anonymous function, sorts by compairing PK.  Lambda expressions are not allowed, this is the one and only exception.  JS approved.
			listDB.Sort((JobLog x,JobLog y) => { return x.JobLogNum.CompareTo(y.JobLogNum); });//Anonymous function, sorts by compairing PK.  Lambda expressions are not allowed, this is the one and only exception.  JS approved.
			int idxNew=0;
			int idxDB=0;
			int rowsUpdatedCount=0;
			JobLog fieldNew;
			JobLog fieldDB;
			//Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
			//If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
			while(idxNew<listNew.Count || idxDB<listDB.Count) {
				fieldNew=null;
				if(idxNew<listNew.Count) {
					fieldNew=listNew[idxNew];
				}
				fieldDB=null;
				if(idxDB<listDB.Count) {
					fieldDB=listDB[idxDB];
				}
				//begin compare
				if(fieldNew!=null && fieldDB==null) {//listNew has more items, listDB does not.
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew==null && fieldDB!=null) {//listDB has more items, listNew does not.
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				else if(fieldNew.JobLogNum<fieldDB.JobLogNum) {//newPK less than dbPK, newItem is 'next'
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew.JobLogNum>fieldDB.JobLogNum) {//dbPK less than newPK, dbItem is 'next'
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				//Both lists contain the 'next' item, update required
				listUpdNew.Add(fieldNew);
				listUpdDB.Add(fieldDB);
				idxNew++;
				idxDB++;
			}
			//Commit changes to DB
			for(int i=0;i<listIns.Count;i++) {
				Insert(listIns[i]);
			}
			for(int i=0;i<listUpdNew.Count;i++) {
				if(Update(listUpdNew[i],listUpdDB[i])){
					rowsUpdatedCount++;
				}
			}
			for(int i=0;i<listDel.Count;i++) {
				Delete(listDel[i].JobLogNum);
			}
			if(rowsUpdatedCount>0 || listIns.Count>0 || listDel.Count>0) {
				return true;
			}
			return false;
		}

	}
}