//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class EtransMessageTextCrud {
		///<summary>Gets one EtransMessageText object from the database using the primary key.  Returns null if not found.</summary>
		public static EtransMessageText SelectOne(long etransMessageTextNum){
			string command="SELECT * FROM etransmessagetext "
				+"WHERE EtransMessageTextNum = "+POut.Long(etransMessageTextNum);
			List<EtransMessageText> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one EtransMessageText object from the database using a query.</summary>
		public static EtransMessageText SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<EtransMessageText> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of EtransMessageText objects from the database using a query.</summary>
		public static List<EtransMessageText> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<EtransMessageText> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<EtransMessageText> TableToList(DataTable table){
			List<EtransMessageText> retVal=new List<EtransMessageText>();
			EtransMessageText etransMessageText;
			foreach(DataRow row in table.Rows) {
				etransMessageText=new EtransMessageText();
				etransMessageText.EtransMessageTextNum= PIn.Long  (row["EtransMessageTextNum"].ToString());
				etransMessageText.MessageText         = PIn.String(row["MessageText"].ToString());
				retVal.Add(etransMessageText);
			}
			return retVal;
		}

		///<summary>Converts a list of EtransMessageText into a DataTable.</summary>
		public static DataTable ListToTable(List<EtransMessageText> listEtransMessageTexts,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="EtransMessageText";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("EtransMessageTextNum");
			table.Columns.Add("MessageText");
			foreach(EtransMessageText etransMessageText in listEtransMessageTexts) {
				table.Rows.Add(new object[] {
					POut.Long  (etransMessageText.EtransMessageTextNum),
					            etransMessageText.MessageText,
				});
			}
			return table;
		}

		///<summary>Inserts one EtransMessageText into the database.  Returns the new priKey.</summary>
		public static long Insert(EtransMessageText etransMessageText){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				etransMessageText.EtransMessageTextNum=DbHelper.GetNextOracleKey("etransmessagetext","EtransMessageTextNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(etransMessageText,true);
					}
					catch(Oracle.ManagedDataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							etransMessageText.EtransMessageTextNum++;
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
				return Insert(etransMessageText,false);
			}
		}

		///<summary>Inserts one EtransMessageText into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(EtransMessageText etransMessageText,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				etransMessageText.EtransMessageTextNum=ReplicationServers.GetKey("etransmessagetext","EtransMessageTextNum");
			}
			string command="INSERT INTO etransmessagetext (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="EtransMessageTextNum,";
			}
			command+="MessageText) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(etransMessageText.EtransMessageTextNum)+",";
			}
			command+=
				     DbHelper.ParamChar+"paramMessageText)";
			if(etransMessageText.MessageText==null) {
				etransMessageText.MessageText="";
			}
			OdSqlParameter paramMessageText=new OdSqlParameter("paramMessageText",OdDbType.Text,POut.StringParam(etransMessageText.MessageText));
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramMessageText);
			}
			else {
				etransMessageText.EtransMessageTextNum=Db.NonQ(command,true,"EtransMessageTextNum","etransMessageText",paramMessageText);
			}
			return etransMessageText.EtransMessageTextNum;
		}

		///<summary>Inserts one EtransMessageText into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(EtransMessageText etransMessageText){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(etransMessageText,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					etransMessageText.EtransMessageTextNum=DbHelper.GetNextOracleKey("etransmessagetext","EtransMessageTextNum"); //Cacheless method
				}
				return InsertNoCache(etransMessageText,true);
			}
		}

		///<summary>Inserts one EtransMessageText into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(EtransMessageText etransMessageText,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO etransmessagetext (";
			if(!useExistingPK && isRandomKeys) {
				etransMessageText.EtransMessageTextNum=ReplicationServers.GetKeyNoCache("etransmessagetext","EtransMessageTextNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="EtransMessageTextNum,";
			}
			command+="MessageText) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(etransMessageText.EtransMessageTextNum)+",";
			}
			command+=
				     DbHelper.ParamChar+"paramMessageText)";
			if(etransMessageText.MessageText==null) {
				etransMessageText.MessageText="";
			}
			OdSqlParameter paramMessageText=new OdSqlParameter("paramMessageText",OdDbType.Text,POut.StringParam(etransMessageText.MessageText));
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command,paramMessageText);
			}
			else {
				etransMessageText.EtransMessageTextNum=Db.NonQ(command,true,"EtransMessageTextNum","etransMessageText",paramMessageText);
			}
			return etransMessageText.EtransMessageTextNum;
		}

		///<summary>Updates one EtransMessageText in the database.</summary>
		public static void Update(EtransMessageText etransMessageText){
			string command="UPDATE etransmessagetext SET "
				+"MessageText         =  "+DbHelper.ParamChar+"paramMessageText "
				+"WHERE EtransMessageTextNum = "+POut.Long(etransMessageText.EtransMessageTextNum);
			if(etransMessageText.MessageText==null) {
				etransMessageText.MessageText="";
			}
			OdSqlParameter paramMessageText=new OdSqlParameter("paramMessageText",OdDbType.Text,POut.StringParam(etransMessageText.MessageText));
			Db.NonQ(command,paramMessageText);
		}

		///<summary>Updates one EtransMessageText in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(EtransMessageText etransMessageText,EtransMessageText oldEtransMessageText){
			string command="";
			if(etransMessageText.MessageText != oldEtransMessageText.MessageText) {
				if(command!=""){ command+=",";}
				command+="MessageText = "+DbHelper.ParamChar+"paramMessageText";
			}
			if(command==""){
				return false;
			}
			if(etransMessageText.MessageText==null) {
				etransMessageText.MessageText="";
			}
			OdSqlParameter paramMessageText=new OdSqlParameter("paramMessageText",OdDbType.Text,POut.StringParam(etransMessageText.MessageText));
			command="UPDATE etransmessagetext SET "+command
				+" WHERE EtransMessageTextNum = "+POut.Long(etransMessageText.EtransMessageTextNum);
			Db.NonQ(command,paramMessageText);
			return true;
		}

		///<summary>Returns true if Update(EtransMessageText,EtransMessageText) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(EtransMessageText etransMessageText,EtransMessageText oldEtransMessageText) {
			if(etransMessageText.MessageText != oldEtransMessageText.MessageText) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one EtransMessageText from the database.</summary>
		public static void Delete(long etransMessageTextNum){
			string command="DELETE FROM etransmessagetext "
				+"WHERE EtransMessageTextNum = "+POut.Long(etransMessageTextNum);
			Db.NonQ(command);
		}

	}
}