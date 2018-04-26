//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class FormPatCrud {
		///<summary>Gets one FormPat object from the database using the primary key.  Returns null if not found.</summary>
		public static FormPat SelectOne(long formPatNum){
			string command="SELECT * FROM formpat "
				+"WHERE FormPatNum = "+POut.Long(formPatNum);
			List<FormPat> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one FormPat object from the database using a query.</summary>
		public static FormPat SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<FormPat> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of FormPat objects from the database using a query.</summary>
		public static List<FormPat> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<FormPat> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<FormPat> TableToList(DataTable table){
			List<FormPat> retVal=new List<FormPat>();
			FormPat formPat;
			foreach(DataRow row in table.Rows) {
				formPat=new FormPat();
				formPat.FormPatNum  = PIn.Long  (row["FormPatNum"].ToString());
				formPat.PatNum      = PIn.Long  (row["PatNum"].ToString());
				formPat.FormDateTime= PIn.DateT (row["FormDateTime"].ToString());
				retVal.Add(formPat);
			}
			return retVal;
		}

		///<summary>Converts a list of FormPat into a DataTable.</summary>
		public static DataTable ListToTable(List<FormPat> listFormPats,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="FormPat";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("FormPatNum");
			table.Columns.Add("PatNum");
			table.Columns.Add("FormDateTime");
			foreach(FormPat formPat in listFormPats) {
				table.Rows.Add(new object[] {
					POut.Long  (formPat.FormPatNum),
					POut.Long  (formPat.PatNum),
					POut.DateT (formPat.FormDateTime,false),
				});
			}
			return table;
		}

		///<summary>Inserts one FormPat into the database.  Returns the new priKey.</summary>
		public static long Insert(FormPat formPat){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				formPat.FormPatNum=DbHelper.GetNextOracleKey("formpat","FormPatNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(formPat,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							formPat.FormPatNum++;
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
				return Insert(formPat,false);
			}
		}

		///<summary>Inserts one FormPat into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(FormPat formPat,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				formPat.FormPatNum=ReplicationServers.GetKey("formpat","FormPatNum");
			}
			string command="INSERT INTO formpat (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="FormPatNum,";
			}
			command+="PatNum,FormDateTime) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(formPat.FormPatNum)+",";
			}
			command+=
				     POut.Long  (formPat.PatNum)+","
				+    POut.DateT (formPat.FormDateTime)+")";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				formPat.FormPatNum=Db.NonQ(command,true,"FormPatNum","formPat");
			}
			return formPat.FormPatNum;
		}

		///<summary>Inserts one FormPat into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(FormPat formPat){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(formPat,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					formPat.FormPatNum=DbHelper.GetNextOracleKey("formpat","FormPatNum"); //Cacheless method
				}
				return InsertNoCache(formPat,true);
			}
		}

		///<summary>Inserts one FormPat into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(FormPat formPat,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO formpat (";
			if(!useExistingPK && isRandomKeys) {
				formPat.FormPatNum=ReplicationServers.GetKeyNoCache("formpat","FormPatNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="FormPatNum,";
			}
			command+="PatNum,FormDateTime) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(formPat.FormPatNum)+",";
			}
			command+=
				     POut.Long  (formPat.PatNum)+","
				+    POut.DateT (formPat.FormDateTime)+")";
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				formPat.FormPatNum=Db.NonQ(command,true,"FormPatNum","formPat");
			}
			return formPat.FormPatNum;
		}

		///<summary>Updates one FormPat in the database.</summary>
		public static void Update(FormPat formPat){
			string command="UPDATE formpat SET "
				+"PatNum      =  "+POut.Long  (formPat.PatNum)+", "
				+"FormDateTime=  "+POut.DateT (formPat.FormDateTime)+" "
				+"WHERE FormPatNum = "+POut.Long(formPat.FormPatNum);
			Db.NonQ(command);
		}

		///<summary>Updates one FormPat in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(FormPat formPat,FormPat oldFormPat){
			string command="";
			if(formPat.PatNum != oldFormPat.PatNum) {
				if(command!=""){ command+=",";}
				command+="PatNum = "+POut.Long(formPat.PatNum)+"";
			}
			if(formPat.FormDateTime != oldFormPat.FormDateTime) {
				if(command!=""){ command+=",";}
				command+="FormDateTime = "+POut.DateT(formPat.FormDateTime)+"";
			}
			if(command==""){
				return false;
			}
			command="UPDATE formpat SET "+command
				+" WHERE FormPatNum = "+POut.Long(formPat.FormPatNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(FormPat,FormPat) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(FormPat formPat,FormPat oldFormPat) {
			if(formPat.PatNum != oldFormPat.PatNum) {
				return true;
			}
			if(formPat.FormDateTime != oldFormPat.FormDateTime) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one FormPat from the database.</summary>
		public static void Delete(long formPatNum){
			string command="DELETE FROM formpat "
				+"WHERE FormPatNum = "+POut.Long(formPatNum);
			Db.NonQ(command);
		}

	}
}