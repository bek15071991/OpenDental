//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class ProcButtonItemCrud {
		///<summary>Gets one ProcButtonItem object from the database using the primary key.  Returns null if not found.</summary>
		public static ProcButtonItem SelectOne(long procButtonItemNum){
			string command="SELECT * FROM procbuttonitem "
				+"WHERE ProcButtonItemNum = "+POut.Long(procButtonItemNum);
			List<ProcButtonItem> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one ProcButtonItem object from the database using a query.</summary>
		public static ProcButtonItem SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<ProcButtonItem> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of ProcButtonItem objects from the database using a query.</summary>
		public static List<ProcButtonItem> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<ProcButtonItem> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<ProcButtonItem> TableToList(DataTable table){
			List<ProcButtonItem> retVal=new List<ProcButtonItem>();
			ProcButtonItem procButtonItem;
			foreach(DataRow row in table.Rows) {
				procButtonItem=new ProcButtonItem();
				procButtonItem.ProcButtonItemNum= PIn.Long  (row["ProcButtonItemNum"].ToString());
				procButtonItem.ProcButtonNum    = PIn.Long  (row["ProcButtonNum"].ToString());
				procButtonItem.OldCode          = PIn.String(row["OldCode"].ToString());
				procButtonItem.AutoCodeNum      = PIn.Long  (row["AutoCodeNum"].ToString());
				procButtonItem.CodeNum          = PIn.Long  (row["CodeNum"].ToString());
				procButtonItem.ItemOrder        = PIn.Long  (row["ItemOrder"].ToString());
				retVal.Add(procButtonItem);
			}
			return retVal;
		}

		///<summary>Converts a list of ProcButtonItem into a DataTable.</summary>
		public static DataTable ListToTable(List<ProcButtonItem> listProcButtonItems,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="ProcButtonItem";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("ProcButtonItemNum");
			table.Columns.Add("ProcButtonNum");
			table.Columns.Add("OldCode");
			table.Columns.Add("AutoCodeNum");
			table.Columns.Add("CodeNum");
			table.Columns.Add("ItemOrder");
			foreach(ProcButtonItem procButtonItem in listProcButtonItems) {
				table.Rows.Add(new object[] {
					POut.Long  (procButtonItem.ProcButtonItemNum),
					POut.Long  (procButtonItem.ProcButtonNum),
					            procButtonItem.OldCode,
					POut.Long  (procButtonItem.AutoCodeNum),
					POut.Long  (procButtonItem.CodeNum),
					POut.Long  (procButtonItem.ItemOrder),
				});
			}
			return table;
		}

		///<summary>Inserts one ProcButtonItem into the database.  Returns the new priKey.</summary>
		public static long Insert(ProcButtonItem procButtonItem){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				procButtonItem.ProcButtonItemNum=DbHelper.GetNextOracleKey("procbuttonitem","ProcButtonItemNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(procButtonItem,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							procButtonItem.ProcButtonItemNum++;
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
				return Insert(procButtonItem,false);
			}
		}

		///<summary>Inserts one ProcButtonItem into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(ProcButtonItem procButtonItem,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				procButtonItem.ProcButtonItemNum=ReplicationServers.GetKey("procbuttonitem","ProcButtonItemNum");
			}
			string command="INSERT INTO procbuttonitem (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="ProcButtonItemNum,";
			}
			command+="ProcButtonNum,OldCode,AutoCodeNum,CodeNum,ItemOrder) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(procButtonItem.ProcButtonItemNum)+",";
			}
			command+=
				     POut.Long  (procButtonItem.ProcButtonNum)+","
				+"'"+POut.String(procButtonItem.OldCode)+"',"
				+    POut.Long  (procButtonItem.AutoCodeNum)+","
				+    POut.Long  (procButtonItem.CodeNum)+","
				+    POut.Long  (procButtonItem.ItemOrder)+")";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				procButtonItem.ProcButtonItemNum=Db.NonQ(command,true,"ProcButtonItemNum","procButtonItem");
			}
			return procButtonItem.ProcButtonItemNum;
		}

		///<summary>Inserts one ProcButtonItem into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(ProcButtonItem procButtonItem){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(procButtonItem,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					procButtonItem.ProcButtonItemNum=DbHelper.GetNextOracleKey("procbuttonitem","ProcButtonItemNum"); //Cacheless method
				}
				return InsertNoCache(procButtonItem,true);
			}
		}

		///<summary>Inserts one ProcButtonItem into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(ProcButtonItem procButtonItem,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO procbuttonitem (";
			if(!useExistingPK && isRandomKeys) {
				procButtonItem.ProcButtonItemNum=ReplicationServers.GetKeyNoCache("procbuttonitem","ProcButtonItemNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="ProcButtonItemNum,";
			}
			command+="ProcButtonNum,OldCode,AutoCodeNum,CodeNum,ItemOrder) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(procButtonItem.ProcButtonItemNum)+",";
			}
			command+=
				     POut.Long  (procButtonItem.ProcButtonNum)+","
				+"'"+POut.String(procButtonItem.OldCode)+"',"
				+    POut.Long  (procButtonItem.AutoCodeNum)+","
				+    POut.Long  (procButtonItem.CodeNum)+","
				+    POut.Long  (procButtonItem.ItemOrder)+")";
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				procButtonItem.ProcButtonItemNum=Db.NonQ(command,true,"ProcButtonItemNum","procButtonItem");
			}
			return procButtonItem.ProcButtonItemNum;
		}

		///<summary>Updates one ProcButtonItem in the database.</summary>
		public static void Update(ProcButtonItem procButtonItem){
			string command="UPDATE procbuttonitem SET "
				+"ProcButtonNum    =  "+POut.Long  (procButtonItem.ProcButtonNum)+", "
				+"OldCode          = '"+POut.String(procButtonItem.OldCode)+"', "
				+"AutoCodeNum      =  "+POut.Long  (procButtonItem.AutoCodeNum)+", "
				+"CodeNum          =  "+POut.Long  (procButtonItem.CodeNum)+", "
				+"ItemOrder        =  "+POut.Long  (procButtonItem.ItemOrder)+" "
				+"WHERE ProcButtonItemNum = "+POut.Long(procButtonItem.ProcButtonItemNum);
			Db.NonQ(command);
		}

		///<summary>Updates one ProcButtonItem in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(ProcButtonItem procButtonItem,ProcButtonItem oldProcButtonItem){
			string command="";
			if(procButtonItem.ProcButtonNum != oldProcButtonItem.ProcButtonNum) {
				if(command!=""){ command+=",";}
				command+="ProcButtonNum = "+POut.Long(procButtonItem.ProcButtonNum)+"";
			}
			if(procButtonItem.OldCode != oldProcButtonItem.OldCode) {
				if(command!=""){ command+=",";}
				command+="OldCode = '"+POut.String(procButtonItem.OldCode)+"'";
			}
			if(procButtonItem.AutoCodeNum != oldProcButtonItem.AutoCodeNum) {
				if(command!=""){ command+=",";}
				command+="AutoCodeNum = "+POut.Long(procButtonItem.AutoCodeNum)+"";
			}
			if(procButtonItem.CodeNum != oldProcButtonItem.CodeNum) {
				if(command!=""){ command+=",";}
				command+="CodeNum = "+POut.Long(procButtonItem.CodeNum)+"";
			}
			if(procButtonItem.ItemOrder != oldProcButtonItem.ItemOrder) {
				if(command!=""){ command+=",";}
				command+="ItemOrder = "+POut.Long(procButtonItem.ItemOrder)+"";
			}
			if(command==""){
				return false;
			}
			command="UPDATE procbuttonitem SET "+command
				+" WHERE ProcButtonItemNum = "+POut.Long(procButtonItem.ProcButtonItemNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(ProcButtonItem,ProcButtonItem) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(ProcButtonItem procButtonItem,ProcButtonItem oldProcButtonItem) {
			if(procButtonItem.ProcButtonNum != oldProcButtonItem.ProcButtonNum) {
				return true;
			}
			if(procButtonItem.OldCode != oldProcButtonItem.OldCode) {
				return true;
			}
			if(procButtonItem.AutoCodeNum != oldProcButtonItem.AutoCodeNum) {
				return true;
			}
			if(procButtonItem.CodeNum != oldProcButtonItem.CodeNum) {
				return true;
			}
			if(procButtonItem.ItemOrder != oldProcButtonItem.ItemOrder) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one ProcButtonItem from the database.</summary>
		public static void Delete(long procButtonItemNum){
			string command="DELETE FROM procbuttonitem "
				+"WHERE ProcButtonItemNum = "+POut.Long(procButtonItemNum);
			Db.NonQ(command);
		}

	}
}