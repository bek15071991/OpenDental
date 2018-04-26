//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class InsVerifyCrud {
		///<summary>Gets one InsVerify object from the database using the primary key.  Returns null if not found.</summary>
		public static InsVerify SelectOne(long insVerifyNum){
			string command="SELECT * FROM insverify "
				+"WHERE InsVerifyNum = "+POut.Long(insVerifyNum);
			List<InsVerify> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one InsVerify object from the database using a query.</summary>
		public static InsVerify SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<InsVerify> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of InsVerify objects from the database using a query.</summary>
		public static List<InsVerify> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<InsVerify> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<InsVerify> TableToList(DataTable table){
			List<InsVerify> retVal=new List<InsVerify>();
			InsVerify insVerify;
			foreach(DataRow row in table.Rows) {
				insVerify=new InsVerify();
				insVerify.InsVerifyNum                 = PIn.Long  (row["InsVerifyNum"].ToString());
				insVerify.DateLastVerified             = PIn.Date  (row["DateLastVerified"].ToString());
				insVerify.UserNum                      = PIn.Long  (row["UserNum"].ToString());
				insVerify.VerifyType                   = (OpenDentBusiness.VerifyTypes)PIn.Int(row["VerifyType"].ToString());
				insVerify.FKey                         = PIn.Long  (row["FKey"].ToString());
				insVerify.DefNum                       = PIn.Long  (row["DefNum"].ToString());
				insVerify.DateLastAssigned             = PIn.Date  (row["DateLastAssigned"].ToString());
				insVerify.Note                         = PIn.String(row["Note"].ToString());
				insVerify.DateTimeEntry                = PIn.DateT (row["DateTimeEntry"].ToString());
				insVerify.HoursAvailableForVerification= PIn.Double(row["HoursAvailableForVerification"].ToString());
				retVal.Add(insVerify);
			}
			return retVal;
		}

		///<summary>Converts a list of InsVerify into a DataTable.</summary>
		public static DataTable ListToTable(List<InsVerify> listInsVerifys,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="InsVerify";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("InsVerifyNum");
			table.Columns.Add("DateLastVerified");
			table.Columns.Add("UserNum");
			table.Columns.Add("VerifyType");
			table.Columns.Add("FKey");
			table.Columns.Add("DefNum");
			table.Columns.Add("DateLastAssigned");
			table.Columns.Add("Note");
			table.Columns.Add("DateTimeEntry");
			table.Columns.Add("HoursAvailableForVerification");
			foreach(InsVerify insVerify in listInsVerifys) {
				table.Rows.Add(new object[] {
					POut.Long  (insVerify.InsVerifyNum),
					POut.DateT (insVerify.DateLastVerified,false),
					POut.Long  (insVerify.UserNum),
					POut.Int   ((int)insVerify.VerifyType),
					POut.Long  (insVerify.FKey),
					POut.Long  (insVerify.DefNum),
					POut.DateT (insVerify.DateLastAssigned,false),
					            insVerify.Note,
					POut.DateT (insVerify.DateTimeEntry,false),
					POut.Double(insVerify.HoursAvailableForVerification),
				});
			}
			return table;
		}

		///<summary>Inserts one InsVerify into the database.  Returns the new priKey.</summary>
		public static long Insert(InsVerify insVerify){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				insVerify.InsVerifyNum=DbHelper.GetNextOracleKey("insverify","InsVerifyNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(insVerify,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							insVerify.InsVerifyNum++;
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
				return Insert(insVerify,false);
			}
		}

		///<summary>Inserts one InsVerify into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(InsVerify insVerify,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				insVerify.InsVerifyNum=ReplicationServers.GetKey("insverify","InsVerifyNum");
			}
			string command="INSERT INTO insverify (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="InsVerifyNum,";
			}
			command+="DateLastVerified,UserNum,VerifyType,FKey,DefNum,DateLastAssigned,Note,DateTimeEntry,HoursAvailableForVerification) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(insVerify.InsVerifyNum)+",";
			}
			command+=
				     POut.Date  (insVerify.DateLastVerified)+","
				+    POut.Long  (insVerify.UserNum)+","
				+    POut.Int   ((int)insVerify.VerifyType)+","
				+    POut.Long  (insVerify.FKey)+","
				+    POut.Long  (insVerify.DefNum)+","
				+    POut.Date  (insVerify.DateLastAssigned)+","
				+    DbHelper.ParamChar+"paramNote,"
				+    DbHelper.Now()+","
				+"'"+POut.Double(insVerify.HoursAvailableForVerification)+"')";
			if(insVerify.Note==null) {
				insVerify.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,POut.StringNote(insVerify.Note));
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramNote);
			}
			else {
				insVerify.InsVerifyNum=Db.NonQ(command,true,"InsVerifyNum","insVerify",paramNote);
			}
			return insVerify.InsVerifyNum;
		}

		///<summary>Inserts one InsVerify into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(InsVerify insVerify){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(insVerify,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					insVerify.InsVerifyNum=DbHelper.GetNextOracleKey("insverify","InsVerifyNum"); //Cacheless method
				}
				return InsertNoCache(insVerify,true);
			}
		}

		///<summary>Inserts one InsVerify into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(InsVerify insVerify,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO insverify (";
			if(!useExistingPK && isRandomKeys) {
				insVerify.InsVerifyNum=ReplicationServers.GetKeyNoCache("insverify","InsVerifyNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="InsVerifyNum,";
			}
			command+="DateLastVerified,UserNum,VerifyType,FKey,DefNum,DateLastAssigned,Note,DateTimeEntry,HoursAvailableForVerification) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(insVerify.InsVerifyNum)+",";
			}
			command+=
				     POut.Date  (insVerify.DateLastVerified)+","
				+    POut.Long  (insVerify.UserNum)+","
				+    POut.Int   ((int)insVerify.VerifyType)+","
				+    POut.Long  (insVerify.FKey)+","
				+    POut.Long  (insVerify.DefNum)+","
				+    POut.Date  (insVerify.DateLastAssigned)+","
				+    DbHelper.ParamChar+"paramNote,"
				+    DbHelper.Now()+","
				+"'"+POut.Double(insVerify.HoursAvailableForVerification)+"')";
			if(insVerify.Note==null) {
				insVerify.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,POut.StringNote(insVerify.Note));
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command,paramNote);
			}
			else {
				insVerify.InsVerifyNum=Db.NonQ(command,true,"InsVerifyNum","insVerify",paramNote);
			}
			return insVerify.InsVerifyNum;
		}

		///<summary>Updates one InsVerify in the database.</summary>
		public static void Update(InsVerify insVerify){
			string command="UPDATE insverify SET "
				+"DateLastVerified             =  "+POut.Date  (insVerify.DateLastVerified)+", "
				+"UserNum                      =  "+POut.Long  (insVerify.UserNum)+", "
				+"VerifyType                   =  "+POut.Int   ((int)insVerify.VerifyType)+", "
				+"FKey                         =  "+POut.Long  (insVerify.FKey)+", "
				+"DefNum                       =  "+POut.Long  (insVerify.DefNum)+", "
				+"DateLastAssigned             =  "+POut.Date  (insVerify.DateLastAssigned)+", "
				+"Note                         =  "+DbHelper.ParamChar+"paramNote, "
				//DateTimeEntry not allowed to change
				+"HoursAvailableForVerification= '"+POut.Double(insVerify.HoursAvailableForVerification)+"' "
				+"WHERE InsVerifyNum = "+POut.Long(insVerify.InsVerifyNum);
			if(insVerify.Note==null) {
				insVerify.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,POut.StringNote(insVerify.Note));
			Db.NonQ(command,paramNote);
		}

		///<summary>Updates one InsVerify in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(InsVerify insVerify,InsVerify oldInsVerify){
			string command="";
			if(insVerify.DateLastVerified.Date != oldInsVerify.DateLastVerified.Date) {
				if(command!=""){ command+=",";}
				command+="DateLastVerified = "+POut.Date(insVerify.DateLastVerified)+"";
			}
			if(insVerify.UserNum != oldInsVerify.UserNum) {
				if(command!=""){ command+=",";}
				command+="UserNum = "+POut.Long(insVerify.UserNum)+"";
			}
			if(insVerify.VerifyType != oldInsVerify.VerifyType) {
				if(command!=""){ command+=",";}
				command+="VerifyType = "+POut.Int   ((int)insVerify.VerifyType)+"";
			}
			if(insVerify.FKey != oldInsVerify.FKey) {
				if(command!=""){ command+=",";}
				command+="FKey = "+POut.Long(insVerify.FKey)+"";
			}
			if(insVerify.DefNum != oldInsVerify.DefNum) {
				if(command!=""){ command+=",";}
				command+="DefNum = "+POut.Long(insVerify.DefNum)+"";
			}
			if(insVerify.DateLastAssigned.Date != oldInsVerify.DateLastAssigned.Date) {
				if(command!=""){ command+=",";}
				command+="DateLastAssigned = "+POut.Date(insVerify.DateLastAssigned)+"";
			}
			if(insVerify.Note != oldInsVerify.Note) {
				if(command!=""){ command+=",";}
				command+="Note = "+DbHelper.ParamChar+"paramNote";
			}
			//DateTimeEntry not allowed to change
			if(insVerify.HoursAvailableForVerification != oldInsVerify.HoursAvailableForVerification) {
				if(command!=""){ command+=",";}
				command+="HoursAvailableForVerification = '"+POut.Double(insVerify.HoursAvailableForVerification)+"'";
			}
			if(command==""){
				return false;
			}
			if(insVerify.Note==null) {
				insVerify.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,POut.StringNote(insVerify.Note));
			command="UPDATE insverify SET "+command
				+" WHERE InsVerifyNum = "+POut.Long(insVerify.InsVerifyNum);
			Db.NonQ(command,paramNote);
			return true;
		}

		///<summary>Returns true if Update(InsVerify,InsVerify) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(InsVerify insVerify,InsVerify oldInsVerify) {
			if(insVerify.DateLastVerified.Date != oldInsVerify.DateLastVerified.Date) {
				return true;
			}
			if(insVerify.UserNum != oldInsVerify.UserNum) {
				return true;
			}
			if(insVerify.VerifyType != oldInsVerify.VerifyType) {
				return true;
			}
			if(insVerify.FKey != oldInsVerify.FKey) {
				return true;
			}
			if(insVerify.DefNum != oldInsVerify.DefNum) {
				return true;
			}
			if(insVerify.DateLastAssigned.Date != oldInsVerify.DateLastAssigned.Date) {
				return true;
			}
			if(insVerify.Note != oldInsVerify.Note) {
				return true;
			}
			//DateTimeEntry not allowed to change
			if(insVerify.HoursAvailableForVerification != oldInsVerify.HoursAvailableForVerification) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one InsVerify from the database.</summary>
		public static void Delete(long insVerifyNum){
			string command="DELETE FROM insverify "
				+"WHERE InsVerifyNum = "+POut.Long(insVerifyNum);
			Db.NonQ(command);
		}

	}
}