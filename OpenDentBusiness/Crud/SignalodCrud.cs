//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;

namespace OpenDentBusiness.Crud{
	public class SignalodCrud {
		///<summary>Gets one Signalod object from the database using the primary key.  Returns null if not found.</summary>
		public static Signalod SelectOne(long signalNum){
			string command="SELECT * FROM signalod "
				+"WHERE SignalNum = "+POut.Long(signalNum);
			List<Signalod> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one Signalod object from the database using a query.</summary>
		public static Signalod SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<Signalod> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of Signalod objects from the database using a query.</summary>
		public static List<Signalod> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<Signalod> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<Signalod> TableToList(DataTable table){
			List<Signalod> retVal=new List<Signalod>();
			Signalod signalod;
			foreach(DataRow row in table.Rows) {
				signalod=new Signalod();
				signalod.SignalNum  = PIn.Long  (row["SignalNum"].ToString());
				signalod.DateViewing= PIn.Date  (row["DateViewing"].ToString());
				signalod.SigDateTime= PIn.DateT (row["SigDateTime"].ToString());
				signalod.FKey       = PIn.Long  (row["FKey"].ToString());
				string fKeyType=row["FKeyType"].ToString();
				if(fKeyType==""){
					signalod.FKeyType =(KeyType)0;
				}
				else try{
					signalod.FKeyType =(KeyType)Enum.Parse(typeof(KeyType),fKeyType);
				}
				catch{
					signalod.FKeyType =(KeyType)0;
				}
				signalod.IType      = (OpenDentBusiness.InvalidType)PIn.Int(row["IType"].ToString());
				signalod.RemoteRole = (OpenDentBusiness.RemotingRole)PIn.Int(row["RemoteRole"].ToString());
				signalod.MsgValue   = PIn.String(row["MsgValue"].ToString());
				retVal.Add(signalod);
			}
			return retVal;
		}

		///<summary>Converts a list of Signalod into a DataTable.</summary>
		public static DataTable ListToTable(List<Signalod> listSignalods,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="Signalod";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("SignalNum");
			table.Columns.Add("DateViewing");
			table.Columns.Add("SigDateTime");
			table.Columns.Add("FKey");
			table.Columns.Add("FKeyType");
			table.Columns.Add("IType");
			table.Columns.Add("RemoteRole");
			table.Columns.Add("MsgValue");
			foreach(Signalod signalod in listSignalods) {
				table.Rows.Add(new object[] {
					POut.Long  (signalod.SignalNum),
					POut.DateT (signalod.DateViewing,false),
					POut.DateT (signalod.SigDateTime,false),
					POut.Long  (signalod.FKey),
					POut.Int   ((int)signalod.FKeyType),
					POut.Int   ((int)signalod.IType),
					POut.Int   ((int)signalod.RemoteRole),
					            signalod.MsgValue,
				});
			}
			return table;
		}

		///<summary>Inserts one Signalod into the database.  Returns the new priKey.</summary>
		public static long Insert(Signalod signalod){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				signalod.SignalNum=DbHelper.GetNextOracleKey("signalod","SignalNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(signalod,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							signalod.SignalNum++;
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
				return Insert(signalod,false);
			}
		}

		///<summary>Inserts one Signalod into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(Signalod signalod,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				signalod.SignalNum=ReplicationServers.GetKey("signalod","SignalNum");
			}
			string command="INSERT INTO signalod (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="SignalNum,";
			}
			command+="DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(signalod.SignalNum)+",";
			}
			command+=
				     POut.Date  (signalod.DateViewing)+","
				+    POut.DateT (signalod.SigDateTime)+","
				+    POut.Long  (signalod.FKey)+","
				+"'"+POut.String(signalod.FKeyType.ToString())+"',"
				+    POut.Int   ((int)signalod.IType)+","
				+    POut.Int   ((int)signalod.RemoteRole)+","
				+    DbHelper.ParamChar+"paramMsgValue)";
			if(signalod.MsgValue==null) {
				signalod.MsgValue="";
			}
			OdSqlParameter paramMsgValue=new OdSqlParameter("paramMsgValue",OdDbType.Text,POut.StringParam(signalod.MsgValue));
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramMsgValue);
			}
			else {
				signalod.SignalNum=Db.NonQ(command,true,"SignalNum","signalod",paramMsgValue);
			}
			return signalod.SignalNum;
		}

		///<summary>Inserts many Signalods into the database.  Provides option to use the existing priKey.</summary>
		public static void InsertMany(List <Signalod> listSignalods){
			if(DataConnection.DBtype==DatabaseType.Oracle || PrefC.RandomKeys) {
				foreach(Signalod signalod in listSignalods) {
					Insert(signalod);
				}
			}
			else {
				StringBuilder sbCommands=null;
				int index=0;
				while(index < listSignalods.Count) {
					Signalod signalod=listSignalods[index];
					StringBuilder sbRow=new StringBuilder("(");
					bool hasComma=false;
					if(sbCommands==null) {
						sbCommands=new StringBuilder();
						sbCommands.Append("INSERT INTO signalod (");
						sbCommands.Append("DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES ");
					}
					else {
						hasComma=true;
					}
					sbRow.Append(POut.Date(signalod.DateViewing)); sbRow.Append(",");
					sbRow.Append(POut.DateT(signalod.SigDateTime)); sbRow.Append(",");
					sbRow.Append(POut.Long(signalod.FKey)); sbRow.Append(",");
					sbRow.Append("'"+POut.String(signalod.FKeyType.ToString())+"'"); sbRow.Append(",");
					sbRow.Append(POut.Int((int)signalod.IType)); sbRow.Append(",");
					sbRow.Append(POut.Int((int)signalod.RemoteRole)); sbRow.Append(",");
					sbRow.Append("'"+POut.String(signalod.MsgValue)+"'"); sbRow.Append(")");
					if(sbCommands.Length+sbRow.Length+1 > TableBase.MaxAllowedPacketCount) {
						Db.NonQ(sbCommands.ToString());
						sbCommands=null;
					}
					else {
						if(hasComma) {
							sbCommands.Append(",");
						}
						sbCommands.Append(sbRow.ToString());
						if(index==listSignalods.Count-1) {
							Db.NonQ(sbCommands.ToString());
						}
						index++;
					}
				}
			}
		}

		///<summary>Inserts one Signalod into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(Signalod signalod){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(signalod,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					signalod.SignalNum=DbHelper.GetNextOracleKey("signalod","SignalNum"); //Cacheless method
				}
				return InsertNoCache(signalod,true);
			}
		}

		///<summary>Inserts one Signalod into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(Signalod signalod,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO signalod (";
			if(!useExistingPK && isRandomKeys) {
				signalod.SignalNum=ReplicationServers.GetKeyNoCache("signalod","SignalNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="SignalNum,";
			}
			command+="DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(signalod.SignalNum)+",";
			}
			command+=
				     POut.Date  (signalod.DateViewing)+","
				+    POut.DateT (signalod.SigDateTime)+","
				+    POut.Long  (signalod.FKey)+","
				+"'"+POut.String(signalod.FKeyType.ToString())+"',"
				+    POut.Int   ((int)signalod.IType)+","
				+    POut.Int   ((int)signalod.RemoteRole)+","
				+    DbHelper.ParamChar+"paramMsgValue)";
			if(signalod.MsgValue==null) {
				signalod.MsgValue="";
			}
			OdSqlParameter paramMsgValue=new OdSqlParameter("paramMsgValue",OdDbType.Text,POut.StringParam(signalod.MsgValue));
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command,paramMsgValue);
			}
			else {
				signalod.SignalNum=Db.NonQ(command,true,"SignalNum","signalod",paramMsgValue);
			}
			return signalod.SignalNum;
		}

		///<summary>Updates one Signalod in the database.</summary>
		public static void Update(Signalod signalod){
			string command="UPDATE signalod SET "
				+"DateViewing=  "+POut.Date  (signalod.DateViewing)+", "
				+"SigDateTime=  "+POut.DateT (signalod.SigDateTime)+", "
				+"FKey       =  "+POut.Long  (signalod.FKey)+", "
				+"FKeyType   = '"+POut.String(signalod.FKeyType.ToString())+"', "
				+"IType      =  "+POut.Int   ((int)signalod.IType)+", "
				+"RemoteRole =  "+POut.Int   ((int)signalod.RemoteRole)+", "
				+"MsgValue   =  "+DbHelper.ParamChar+"paramMsgValue "
				+"WHERE SignalNum = "+POut.Long(signalod.SignalNum);
			if(signalod.MsgValue==null) {
				signalod.MsgValue="";
			}
			OdSqlParameter paramMsgValue=new OdSqlParameter("paramMsgValue",OdDbType.Text,POut.StringParam(signalod.MsgValue));
			Db.NonQ(command,paramMsgValue);
		}

		///<summary>Updates one Signalod in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(Signalod signalod,Signalod oldSignalod){
			string command="";
			if(signalod.DateViewing.Date != oldSignalod.DateViewing.Date) {
				if(command!=""){ command+=",";}
				command+="DateViewing = "+POut.Date(signalod.DateViewing)+"";
			}
			if(signalod.SigDateTime != oldSignalod.SigDateTime) {
				if(command!=""){ command+=",";}
				command+="SigDateTime = "+POut.DateT(signalod.SigDateTime)+"";
			}
			if(signalod.FKey != oldSignalod.FKey) {
				if(command!=""){ command+=",";}
				command+="FKey = "+POut.Long(signalod.FKey)+"";
			}
			if(signalod.FKeyType != oldSignalod.FKeyType) {
				if(command!=""){ command+=",";}
				command+="FKeyType = '"+POut.String(signalod.FKeyType.ToString())+"'";
			}
			if(signalod.IType != oldSignalod.IType) {
				if(command!=""){ command+=",";}
				command+="IType = "+POut.Int   ((int)signalod.IType)+"";
			}
			if(signalod.RemoteRole != oldSignalod.RemoteRole) {
				if(command!=""){ command+=",";}
				command+="RemoteRole = "+POut.Int   ((int)signalod.RemoteRole)+"";
			}
			if(signalod.MsgValue != oldSignalod.MsgValue) {
				if(command!=""){ command+=",";}
				command+="MsgValue = "+DbHelper.ParamChar+"paramMsgValue";
			}
			if(command==""){
				return false;
			}
			if(signalod.MsgValue==null) {
				signalod.MsgValue="";
			}
			OdSqlParameter paramMsgValue=new OdSqlParameter("paramMsgValue",OdDbType.Text,POut.StringParam(signalod.MsgValue));
			command="UPDATE signalod SET "+command
				+" WHERE SignalNum = "+POut.Long(signalod.SignalNum);
			Db.NonQ(command,paramMsgValue);
			return true;
		}

		///<summary>Returns true if Update(Signalod,Signalod) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(Signalod signalod,Signalod oldSignalod) {
			if(signalod.DateViewing.Date != oldSignalod.DateViewing.Date) {
				return true;
			}
			if(signalod.SigDateTime != oldSignalod.SigDateTime) {
				return true;
			}
			if(signalod.FKey != oldSignalod.FKey) {
				return true;
			}
			if(signalod.FKeyType != oldSignalod.FKeyType) {
				return true;
			}
			if(signalod.IType != oldSignalod.IType) {
				return true;
			}
			if(signalod.RemoteRole != oldSignalod.RemoteRole) {
				return true;
			}
			if(signalod.MsgValue != oldSignalod.MsgValue) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one Signalod from the database.</summary>
		public static void Delete(long signalNum){
			string command="DELETE FROM signalod "
				+"WHERE SignalNum = "+POut.Long(signalNum);
			Db.NonQ(command);
		}

	}
}