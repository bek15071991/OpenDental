//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Mobile.Crud{
	internal class MedicationPatmCrud {
		///<summary>Gets one MedicationPatm object from the database using primaryKey1(CustomerNum) and primaryKey2.  Returns null if not found.</summary>
		internal static MedicationPatm SelectOne(long customerNum,long medicationPatNum){
			string command="SELECT * FROM medicationpatm "
				+"WHERE CustomerNum = "+POut.Long(customerNum)+" AND MedicationPatNum = "+POut.Long(medicationPatNum);
			List<MedicationPatm> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one MedicationPatm object from the database using a query.</summary>
		internal static MedicationPatm SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<MedicationPatm> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of MedicationPatm objects from the database using a query.</summary>
		internal static List<MedicationPatm> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<MedicationPatm> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		internal static List<MedicationPatm> TableToList(DataTable table){
			List<MedicationPatm> retVal=new List<MedicationPatm>();
			MedicationPatm medicationPatm;
			for(int i=0;i<table.Rows.Count;i++) {
				medicationPatm=new MedicationPatm();
				medicationPatm.CustomerNum     = PIn.Long  (table.Rows[i]["CustomerNum"].ToString());
				medicationPatm.MedicationPatNum= PIn.Long  (table.Rows[i]["MedicationPatNum"].ToString());
				medicationPatm.PatNum          = PIn.Long  (table.Rows[i]["PatNum"].ToString());
				medicationPatm.MedicationNum   = PIn.Long  (table.Rows[i]["MedicationNum"].ToString());
				medicationPatm.PatNote         = PIn.String(table.Rows[i]["PatNote"].ToString());
				medicationPatm.DateStart       = PIn.Date  (table.Rows[i]["DateStart"].ToString());
				medicationPatm.DateStop        = PIn.Date  (table.Rows[i]["DateStop"].ToString());
				retVal.Add(medicationPatm);
			}
			return retVal;
		}

		///<summary>Usually set useExistingPK=true.  Inserts one MedicationPatm into the database.</summary>
		internal static long Insert(MedicationPatm medicationPatm,bool useExistingPK){
			if(!useExistingPK) {
				medicationPatm.MedicationPatNum=ReplicationServers.GetKey("medicationpatm","MedicationPatNum");
			}
			string command="INSERT INTO medicationpatm (";
			command+="MedicationPatNum,";
			command+="CustomerNum,PatNum,MedicationNum,PatNote,DateStart,DateStop) VALUES(";
			command+=POut.Long(medicationPatm.MedicationPatNum)+",";
			command+=
				     POut.Long  (medicationPatm.CustomerNum)+","
				+    POut.Long  (medicationPatm.PatNum)+","
				+    POut.Long  (medicationPatm.MedicationNum)+","
				+"'"+POut.String(medicationPatm.PatNote)+"',"
				+    POut.Date  (medicationPatm.DateStart)+","
				+    POut.Date  (medicationPatm.DateStop)+")";
			Db.NonQ(command);//There is no autoincrement in the mobile server.
			return medicationPatm.MedicationPatNum;
		}

		///<summary>Updates one MedicationPatm in the database.</summary>
		internal static void Update(MedicationPatm medicationPatm){
			string command="UPDATE medicationpatm SET "
				+"PatNum          =  "+POut.Long  (medicationPatm.PatNum)+", "
				+"MedicationNum   =  "+POut.Long  (medicationPatm.MedicationNum)+", "
				+"PatNote         = '"+POut.String(medicationPatm.PatNote)+"', "
				+"DateStart       =  "+POut.Date  (medicationPatm.DateStart)+", "
				+"DateStop        =  "+POut.Date  (medicationPatm.DateStop)+" "
				+"WHERE CustomerNum = "+POut.Long(medicationPatm.CustomerNum)+" AND MedicationPatNum = "+POut.Long(medicationPatm.MedicationPatNum);
			Db.NonQ(command);
		}

		///<summary>Deletes one MedicationPatm from the database.</summary>
		internal static void Delete(long customerNum,long medicationPatNum){
			string command="DELETE FROM medicationpatm "
				+"WHERE CustomerNum = "+POut.Long(customerNum)+" AND MedicationPatNum = "+POut.Long(medicationPatNum);
			Db.NonQ(command);
		}

		///<summary>Converts one MedicationPat object to its mobile equivalent.  Warning! CustomerNum will always be 0.</summary>
		internal static MedicationPatm ConvertToM(MedicationPat medicationPat){
			MedicationPatm medicationPatm=new MedicationPatm();
			//CustomerNum cannot be set.  Remains 0.
			medicationPatm.MedicationPatNum=medicationPat.MedicationPatNum;
			medicationPatm.PatNum          =medicationPat.PatNum;
			medicationPatm.MedicationNum   =medicationPat.MedicationNum;
			medicationPatm.PatNote         =medicationPat.PatNote;
			medicationPatm.DateStart       =medicationPat.DateStart;
			medicationPatm.DateStop        =medicationPat.DateStop;
			return medicationPatm;
		}

	}
}