﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CodeBase;

namespace OpenDentBusiness {
	public class Procedures {
		#region Global Update Fees Variables
		///<summary>Queue to hold batches for FIFO processing.  A batch is a DataTable of TPd procs.  One thread fills the queue with db data while the
		///main thread processes the batches of data.  Make sure to use _lockObjQueueBatchData when manipulating this queue.</summary>
		private static Queue<DataTable> _queueBatchData;
		///<summary>Lock object to keep the queue thread safe.</summary>
		private static object _lockObjQueueBatchData=new object();
		///<summary>False until the filling thread has added the last batch of data to the queue.  Once true AND the queue is empty, the main thread is
		///finished as well.</summary>
		private static bool _isQueueDataThreadDone;
		///<summary>Number of ProcNums the filling thread uses for each batch of data.  The processing takes longer than filling, so we can keep this
		///number relatively small to reduce total program memory consumption.</summary>
		private const int PROCNUM_BATCH_MAX_SIZE=10000;
		private const int UPDATE_PROCNUM_IN_MAX_SIZE=1000;
		///<summary>If this thread is not null then GlobalUpdateFees is in the middle of running.</summary>
		private static ODThread _odThreadQueueData;
		///<summary></summary>
		private static List<long> _listProcNumMaxPerGroup;
		private static int _totCount;
		#endregion Global Update Fees Variables
		#region Get Methods

		///<summary>Gets all procedures for a single planned appointment.  Does not include deleted procedures.</summary>
		public static List<Procedure> GetForPlanned(long patNum,long plannedAptNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,plannedAptNum);
			}
			if(patNum==0 || plannedAptNum==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)
				+" AND PlannedAptNum="+POut.Long(plannedAptNum)
				+" AND ProcStatus !="+POut.Int((int)ProcStat.D);//don't include deleted
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets a list of all tp'd procedures for a specified clinic.</summary>
		public static List<Procedure> GetAllTp(long clinicNum=0) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),clinicNum);
			}
			string command="SELECT * FROM procedurelog WHERE procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP);
			if(clinicNum > 0) {
				command+=" AND procedurelog.ClinicNum="+clinicNum;
			}
			return Crud.ProcedureCrud.SelectMany(command);
		}

		#endregion

		#region Modification Methods

		#region Insert
		#endregion

		#region Update
		#endregion

		#region Delete
		#endregion

		#endregion

		#region Misc Methods
		#endregion

		///<summary>Gets all procedures for a single patient, without notes.  Does not include deleted procedures.</summary>
		public static List<Procedure> Refresh(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)
				+" AND ProcStatus !="+POut.Int((int)ProcStat.D)//don't include deleted
				+" ORDER BY ProcDate";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all procedures for a single patient, without notes.  Does not include deleted procedures.</summary>
		public static List<Procedure> RefreshForStatus(long patNum,ProcStat procStatus,bool isNotOnApt=true) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,procStatus,isNotOnApt);
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)+" "
				+"AND ProcStatus ="+POut.Int((int)procStatus)+" "
				+(isNotOnApt?"AND AptNum=0":"");
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all procedures with a code num in listProcCodeNums for a single patient, without notes.  Does not include deleted procedures.</summary>
		public static List<Procedure> RefreshForProcCodeNums(long patNum,List<long> listProcCodeNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,listProcCodeNums);
			}
			if(listProcCodeNums==null || listProcCodeNums.Count==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)+" "+
				"AND CodeNum IN ("+String.Join(",",listProcCodeNums)+") "+
				"AND ProcStatus !="+POut.Int((int)ProcStat.D)+" "+//don't include deleted
				"ORDER BY ProcDate";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all completed procedures without notes for a list of patients.  Used when making auto splits.
		///Also returns any procedures attached to payplans that the current patient is responsible for.</summary>
		public static List<Procedure> GetCompleteForPats(List<long> listPatNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listPatNums);
			}
			if(listPatNums==null || listPatNums.Count < 1) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM ( "
				+" SELECT procedurelog.*, procedurecode.ProcCode "
				+" FROM patient "
				+" LEFT JOIN procedurelog ON procedurelog.PatNum = patient.Patnum "
				+" INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum "
				+" WHERE patient.PatNum IN ("+String.Join(",",listPatNums)+") "
				+" AND ProcStatus = "+POut.Int((int)ProcStat.C)+" "
				+" AND procedurelog.ProcNum IS NOT NULL "
				+" UNION "
				+" SELECT procedurelog.*, procedurecode.ProcCode "
				+" FROM patient "
				+" LEFT JOIN payplan ON payplan.Guarantor = patient.PatNum "
				+" LEFT JOIN payplancharge ON payplancharge.PayPlanNum = payplan.PayPlanNum "
				+" LEFT JOIN procedurelog ON procedurelog.ProcNum = payplancharge.ProcNum "
				+" INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum "
				+" WHERE patient.PatNum IN ("+String.Join(",",listPatNums)+") "
				+" AND ProcStatus = "+POut.Int((int)ProcStat.C)+" "
				+" AND procedurelog.ProcNum IS NOT NULL "
			+" )A "
			+" ORDER BY A.ProcDate";
			DataTable table=Db.GetTable(command);
			List<DataRow> listSortedRows=new List<DataRow>();
			for(int i=0;i<table.Rows.Count;i++) {//need to make new copy of each row so they don't belong to the old table any more.  Or make a fresh list of datarows.
				listSortedRows.Add(table.Rows[i]);
			}
			listSortedRows.Sort(ProcedureComparer);
			DataTable tableSorted=table.Clone();
			tableSorted.Rows.Clear();
			for(int i=0;i<listSortedRows.Count;i++) {
				tableSorted.Rows.Add(listSortedRows[i].ItemArray);
			}
			return Crud.ProcedureCrud.TableToList(tableSorted);
		}

		///<summary>Gets a limited procedure list.</summary>
		public static List<Procedure> GetForProcTPs(List<ProcTP> listProcTP,params ProcStat[] procStats) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listProcTP,procStats);
			}
			if(listProcTP.Count == 0) {
				return new List<Procedure>();
			}
			string command = "SELECT ProcNum,CodeNum,AptNum,ProcDate,ClinicNum,ProcStatus,ProcFee,BaseUnits,UnitQty FROM procedurelog "
				+"WHERE procedurelog.ProcNum IN ("+string.Join(",",listProcTP.Select(x => x.ProcNumOrig).ToList())+") "
				+"AND procedurelog.ProcStatus IN (" +string.Join(",",procStats.Select(x => (int)x))+ ")";
			DataTable table = Db.GetTable(command);
			List<Procedure> listProcs = new List<Procedure>();
			foreach(DataRow row in table.Rows) {
				Procedure proc = new Procedure();
				proc.ProcNum=PIn.Long(row["ProcNum"].ToString());
				proc.CodeNum=PIn.Long(row["CodeNum"].ToString());
				proc.AptNum=PIn.Long(row["AptNum"].ToString());
				proc.ProcDate=PIn.Date(row["ProcDate"].ToString());
				proc.ClinicNum=PIn.Long(row["ClinicNum"].ToString());
				proc.ProcStatus=(ProcStat)PIn.Int(row["ProcStatus"].ToString());
				proc.ProcFee=PIn.Double(row["ProcFee"].ToString());
				proc.BaseUnits=PIn.Int(row["BaseUnits"].ToString());
				proc.UnitQty=PIn.Int(row["UnitQty"].ToString());
				listProcs.Add(proc);
			}
			return listProcs;
		}

		///<summary>Pass in a list of guarantors. 
		///Gets all procedures that have a remaining balance on them for any member of the guarantor's family.</summary>
		public static List<RpUnearnedIncome.UnearnedProc> GetRemainingProcsForFamilies(List<long> listGuarantorNums) {
			if(listGuarantorNums.Count == 0) {
				return new List<RpUnearnedIncome.UnearnedProc>();
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<RpUnearnedIncome.UnearnedProc>>(MethodBase.GetCurrentMethod(),listGuarantorNums);
			}
			List<long> listAllFamilyPatNums = Patients.GetAllFamilyPatNums(listGuarantorNums);
			/*given a list of families, get all procedures with a remaining pat port for those families.*/
			string command = @"
			SELECT patient.Guarantor,
			(procedurelog.ProcFee *(procedurelog.BaseUnits + procedurelog.UnitQty)) + COALESCE(adj.AdjAmt,0)
				- (COALESCE(cp.WriteOff,0) + COALESCE(cp.InsPay,0) + COALESCE(cp.InsEst,0) + COALESCE(patpay.Amt,0)) RemAmt,
			procedurelog.* 
			FROM procedurelog
			LEFT JOIN (
				SELECT claimproc.ProcNum, 
				SUM(CASE WHEN claimproc.Status IN ("
					+POut.Int((int)ClaimProcStatus.NotReceived)+","
					+POut.Int((int)ClaimProcStatus.Received)+","
					+POut.Int((int)ClaimProcStatus.Supplemental)+","
					+POut.Int((int)ClaimProcStatus.CapComplete)
				+@") THEN claimproc.WriteOff END) AS WriteOff,
				SUM(CASE WHEN claimproc.Status IN ("
					+POut.Int((int)ClaimProcStatus.Received)+","
					+POut.Int((int)ClaimProcStatus.Supplemental)
				+@") THEN claimproc.InsPayAmt END) AS InsPay,
				SUM(CASE WHEN claimproc.Status = "+POut.Int((int)ClaimProcStatus.NotReceived)+@" THEN claimproc.InsPayEst END) AS InsEst
				FROM claimproc
				WHERE claimproc.Status IN ("
					+POut.Int((int)ClaimProcStatus.NotReceived)+","
					+POut.Int((int)ClaimProcStatus.Received)+","
					+POut.Int((int)ClaimProcStatus.Supplemental)+","
					+POut.Int((int)ClaimProcStatus.CapComplete)
				+@")
				AND claimproc.PatNum IN ("+string.Join(",",listAllFamilyPatNums.Select(x => POut.Long(x)))+@")
				AND claimproc.ProcNum != 0
				GROUP BY claimproc.ProcNum
			)cp ON cp.ProcNum = procedurelog.ProcNum
			LEFT JOIN (
				SELECT adjustment.ProcNum, SUM(adjustment.AdjAmt) AdjAmt
				FROM adjustment
				WHERE adjustment.PatNum IN ("+string.Join(",",listAllFamilyPatNums.Select(x => POut.Long(x)))+@")
				AND adjustment.ProcNum != 0
				GROUP BY adjustment.ProcNum
			)adj ON adj.ProcNum = procedurelog.ProcNum
			LEFT JOIN (
				SELECT paysplit.ProcNum, SUM(paysplit.SplitAmt) Amt
				FROM paysplit
				WHERE paysplit.PatNum IN ("+string.Join(",",listAllFamilyPatNums.Select(x => POut.Long(x)))+@")
				AND paysplit.ProcNum != 0
				GROUP BY paysplit.ProcNum
			)patpay ON patpay.ProcNum = procedurelog.ProcNum
			INNER JOIN patient ON patient.PatNum = procedurelog.PatNum
			WHERE procedurelog.ProcStatus = "+POut.Int((int)ProcStat.C)+@"
			AND procedurelog.PatNum IN ("+string.Join(",",listAllFamilyPatNums.Select(x => POut.Long(x)))+@")
			AND (procedurelog.ProcFee *(procedurelog.BaseUnits + procedurelog.UnitQty)) + COALESCE(adj.AdjAmt,0)
				- (COALESCE(cp.WriteOff,0) + COALESCE(cp.InsPay,0) + COALESCE(cp.InsEst,0) + COALESCE(patpay.Amt,0)) > 0.005";
			DataTable table = Db.GetTable(command);
			List<RpUnearnedIncome.UnearnedProc> retVal = new List<RpUnearnedIncome.UnearnedProc>();
			List<Procedure> listProcs = Crud.ProcedureCrud.TableToList(table);
			for(int i = 0;i < listProcs.Count;i++) {
				retVal.Add(new RpUnearnedIncome.UnearnedProc(listProcs[i],PIn.Long(table.Rows[i]["Guarantor"].ToString())
					,PIn.Decimal(table.Rows[i]["RemAmt"].ToString())));
			}
			return retVal;
		}

		///<summary>Gets all completed and TP procedures for a family.</summary>
		public static List<Procedure> GetCompAndTpForPats(List<long> listPatNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listPatNums);
			}
			string command="SELECT * from procedurelog WHERE PatNum IN("+String.Join(", ",listPatNums)+") "
				+"AND ProcStatus IN("+(int)ProcStat.C+","+(int)ProcStat.TP+") "
				+"ORDER BY ProcDate";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary></summary>
		public static long Insert(Procedure procedure) {
			if(RemotingClient.RemotingRole!=RemotingRole.ServerWeb) {
				procedure.SecUserNumEntry=Security.CurUser.UserNum;//must be before normal remoting role check to get user at workstation
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				procedure.ProcNum=Meth.GetLong(MethodBase.GetCurrentMethod(),procedure);
				return procedure.ProcNum;
			}
			if(procedure.ProcStatus==ProcStat.C) {
				procedure.DateComplete=DateTime.Today;
			}
			else {//In case someone tried to programmatically set the DateComplete when they shouldn't have.
				procedure.DateComplete=DateTime.MinValue;
			}
			Crud.ProcedureCrud.Insert(procedure);
			if(procedure.Note!="") {
				ProcNote note=new ProcNote();
				note.PatNum=procedure.PatNum;
				note.ProcNum=procedure.ProcNum;
				note.UserNum=procedure.UserNum;
				note.Note=procedure.Note;
				ProcNotes.Insert(note);
			}
			if(procedure.ProcStatus==ProcStat.C) {
				Adjustments.CreateAdjustmentForDiscountPlan(procedure);//Inserting completed procedure.
			}
			return procedure.ProcNum;
		}

		///<summary>Updates only the changed columns.</summary>
		public static bool Update(Procedure procedure,Procedure oldProcedure,bool isPaySplit=false) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),procedure,oldProcedure,isPaySplit);
			}
			if(oldProcedure.ProcStatus!=ProcStat.C && procedure.ProcStatus==ProcStat.C) {
				Adjustments.CreateAdjustmentForDiscountPlan(procedure);
			}
			//Setting a discount procedure to complete.
			if(oldProcedure.ProcStatus!=ProcStat.C && procedure.ProcStatus==ProcStat.C && !procedure.Discount.IsZero()) {
				Adjustments.CreateAdjustmentForDiscount(procedure);
			}
			//Setting the procedure to complete.
			if((oldProcedure.ProcStatus!=ProcStat.C && procedure.ProcStatus==ProcStat.C)
				|| (oldProcedure.ProcStatus==ProcStat.C && procedure.ProcStatus!=ProcStat.C))
			{
				PayPlanCharges.UpdateAttachedPayPlanCharges(procedure);//does nothing if there are none.
			}
			//Setting a completed procedure to TP.
			if(oldProcedure.ProcStatus==ProcStat.C && procedure.ProcStatus!=ProcStat.C) {
				Adjustments.DeleteForProcedure(procedure.ProcNum);
			}
			if(procedure.ProcStatus==ProcStat.C && procedure.DateComplete.Year<1880) {
				procedure.DateComplete=DateTime.Today;
			}
			else if(procedure.ProcStatus!=ProcStat.C && procedure.DateComplete.Date==DateTime.Today.Date) {
				procedure.DateComplete=DateTime.MinValue;//db only field used by one customer and this is how they requested it.  PatNum #19191
			}
			if(isPaySplit) {
				PaySplits.UpdateAttachedPaySplits(procedure);
			}
			bool result=Crud.ProcedureCrud.Update(procedure,oldProcedure);
			if(procedure.Note!=oldProcedure.Note
				|| procedure.UserNum!=oldProcedure.UserNum
				|| procedure.SigIsTopaz!=oldProcedure.SigIsTopaz
				|| procedure.Signature!=oldProcedure.Signature) 
			{
				ProcNote note=new ProcNote();
				note.PatNum=procedure.PatNum;
				note.ProcNum=procedure.ProcNum;
				note.UserNum=procedure.UserNum;
				note.Note=procedure.Note;
				note.SigIsTopaz=procedure.SigIsTopaz;
				note.Signature=procedure.Signature;
				ProcNotes.Insert(note);
			}
			return result;
		}

		///<summary>Counts the number of patients who have had completed procedures in the date range. D9986 and D9987 are not counted in this query.
		///</summary>
		public static int GetCountPatsComplete(DateTime dateStart,DateTime dateEnd) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetInt(MethodBase.GetCurrentMethod(),dateStart,dateEnd);
			}
			string command=@"SELECT COUNT(DISTINCT PatNum) 
				FROM procedurelog 
				INNER JOIN procedurecode ON procedurecode.CodeNum=procedurelog.CodeNum
					AND procedurecode.ProcCode NOT IN('D9986','D9987')
				WHERE procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+@"
				AND procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd);
			return PIn.Int(Db.GetCount(command));
		}

		///<summary>Gets all procedures with a specific StatementNum.  Currently, procedurelog.StatementNum is only used for invoices.</summary>
		public static List<long> GetForInvoice(long statementNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<long>>(MethodBase.GetCurrentMethod(),statementNum);
			}
			if(statementNum==0) {
				return new List<long>();
			}
			string command="SELECT ProcNum FROM procedurelog WHERE procedurelog.StatementNum = "+POut.Long(statementNum);
			return Db.GetListLong(command);
		}

		///<summary>Throws an exception if the given procedure cannot be deleted safely.</summary>
		public static void ValidateDelete(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum);
				return;
			}
			//Test to see if the procedure is attached to a claim (excluding pre-auths)
			string command="SELECT COUNT(*) FROM claimproc WHERE ProcNum="+POut.Long(procNum)
				+" AND ClaimNum > 0 AND Status!="+POut.Int((int)ClaimProcStatus.Preauth);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to a claim."));
			}
			//Test to see if any payment at all has been received for this proc
			command="SELECT COUNT(*) FROM claimproc WHERE ProcNum="+POut.Long(procNum)
				+" AND InsPayAmt > 0 AND Status IN ("+POut.Int((int)ClaimProcStatus.Received)+","+POut.Int((int)ClaimProcStatus.Supplemental)+","
					+POut.Int((int)ClaimProcStatus.CapClaim)+","+POut.Int((int)ClaimProcStatus.CapComplete)+")";
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to an insurance payment."));
			}
			//Test to see if any referrals exist for this proc
			command="SELECT COUNT(*) FROM refattach WHERE ProcNum="+POut.Long(procNum);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure with referrals attached."));
			}
			//Test to see if any paysplits are attached to this proc
			command="SELECT COUNT(*) FROM paysplit WHERE ProcNum="+POut.Long(procNum);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to a patient payment."));
			}
			command="SELECT COUNT(*) FROM adjustment WHERE ProcNum="+POut.Long(procNum);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to an adjustment."));
			}
			command="SELECT COUNT(*) FROM rxpat WHERE ProcNum="+POut.Long(procNum);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to a prescription."));
			}
		}
		
		///<summary>If not allowed to delete, then it throws an exception, so surround it with a try catch. 
		///Also deletes any claimProcs, adjustments, and payplancharge credits.  
		///This does not actually delete the procedure, but just changes the status to deleted.</summary>
		///<param name="forceDelete">If true, forcefully deletes all objects attached to the procedure.</param>
		public static void Delete(long procNum,bool forceDelete=false) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,forceDelete);
				return;
			}
			if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
				DeleteCanadianLabFeesForProcCode(procNum);//Deletes lab fees attached to current procedures.
			}
			string command;
			if(forceDelete) {
				//Delete referral attaches
				command="DELETE FROM refattach WHERE ProcNum="+POut.Long(procNum);
				Db.NonQ(command);
				//Remove the procedure from the pay split
				command="UPDATE paysplit SET ProcNum=0 WHERE ProcNum="+POut.Long(procNum);
				Db.NonQ(command);
				//Claimprocs deleted below
			}
			else {
				ValidateDelete(procNum);
			}
			//delete adjustments, audit logs added from Adjustments.DeleteForProcedure()
			Adjustments.DeleteForProcedure(procNum);
			//delete claimprocs
			command="DELETE from claimproc WHERE ProcNum = '"+POut.Long(procNum)+"'";
			Db.NonQ(command);
			//detach procedure labs
			command="UPDATE procedurelog SET ProcNumLab=0 WHERE ProcNumLab='"+POut.Long(procNum)+"'";
			Db.NonQ(command);
			PayPlanCharges.DeleteForProc(procNum);
			command="SELECT AptNum,PlannedAptNum,DateComplete FROM procedurelog WHERE ProcNum = "+POut.Long(procNum);
			DataTable table = Db.GetTable(command);
			DateTime dateComplete = PIn.Date(table.Rows[0]["DateComplete"].ToString());
			long aptNum = PIn.Long(table.Rows[0]["AptNum"].ToString());
			long plannedAptNum = PIn.Long(table.Rows[0]["PlannedAptNum"].ToString());
			//set the procedure deleted-----------------------------------------------------------------------------------------
			command="UPDATE procedurelog SET ProcStatus = "+POut.Int((int)ProcStat.D)+", "
				+"AptNum=0, "
				+"PlannedAptNum=0";
			if(dateComplete.Date==DateTime.Today.Date) {
				command+=", DateComplete="+POut.Date(DateTime.MinValue);
			}
			command+=" WHERE ProcNum="+POut.Long(procNum);
			Db.NonQ(command);
			//resynch appointment description-------------------------------------------------------------------------------------
			if(aptNum != 0) {
				Appointment apt = Appointments.GetOneApt(aptNum);
				Appointment aptOld = apt.Copy();
				Appointments.SetProcDescript(apt);
				Appointments.Update(apt,aptOld);
			}
			if(plannedAptNum != 0) {
				Appointment plannedApt = Appointments.GetOneApt(plannedAptNum);
				Appointment plannedAptOld = plannedApt.Copy();
				Appointments.SetProcDescript(plannedApt);
				Appointments.Update(plannedApt,plannedAptOld);
			}
		}

		///<summary>Creates a new procedure with the patient, surface, toothnum, and status for the specified procedure code.
		///Make sure to make a security log after calling this method.  This method requires that Security.CurUser be set prior to invoking.
		///Returns null procedure if one was not created for the patient.</summary>
		public static Procedure CreateProcForPat(long patNum,long codeNum,string surf,string toothNum,ProcStat procStatus,long provNum) {
			//No need to check RemotingRole; no call to db.
			Patient pat=Patients.GetPat(patNum);
			return CreateProcForPat(pat,codeNum,surf,toothNum,procStatus,provNum);
		}

		///<summary>Creates a new procedure with the patient, surface, toothnum, and status for the specified procedure code.
		///Make sure to make a security log after calling this method.  This method requires that Security.CurUser be set prior to invoking.
		///Returns null procedure if one was not created for the patient.</summary>
		public static Procedure CreateProcForPat(Patient pat,long codeNum,string surf,string toothNum,ProcStat procStatus,long provNum,long aptNum=0
			,List<InsSub> subList=null,List<InsPlan> insPlanList=null,List<PatPlan> patPlanList=null,List<Benefit> benefitList=null) 
		{
			//No need to check RemotingRole; no call to db.
			if(codeNum < 1) {
				return null;
			}
			if(provNum==0) {
				provNum=Patients.GetProvNum(pat);
			}
			Procedure proc=new Procedure();
			proc.PatNum=pat.PatNum;
			proc.ClinicNum=Clinics.ClinicNum;
			proc.ProcStatus=procStatus;
			proc.ProvNum=provNum;
			proc.AptNum=aptNum;
			if(surf=="") {
				proc.Surf="";
			}
			else { 
				proc.Surf=surf; //Note: Sealant code D1351 is not a surface code by default, but can be manually set.  For screens they will be surface specific.
			}
			if(toothNum=="") {
				proc.ToothNum="";
			}
			else {
				proc.ToothNum=toothNum;
			}
			proc.UserNum=Security.CurUser.UserNum;
			proc.CodeNum=codeNum;
			proc.ProcDate=DateTime.Today;
			proc.DateTP=DateTime.Today;
			//The below logic is a trimmed down version of the code existing in ContrChart.AddQuick()
			InsPlan insPlanPrimary=null;
			InsSub insSubPrimary=null;
			if(subList==null) {
				subList=InsSubs.RefreshForFam(Patients.GetFamily(pat.PatNum));
			}
			if(insPlanList==null) {
				insPlanList=InsPlans.RefreshForSubList(subList);
			}
			if(patPlanList==null) {
				patPlanList=PatPlans.Refresh(pat.PatNum);
			}
			if(benefitList==null) {
				benefitList=Benefits.Refresh(patPlanList,subList);
			}
			if(patPlanList.Count>0) {
				insSubPrimary=InsSubs.GetSub(patPlanList[0].InsSubNum,subList);
				insPlanPrimary=InsPlans.GetPlan(insSubPrimary.PlanNum,insPlanList);
			}
			//Get fee schedule and fee amount for dental or medical.
			long feeSch=FeeScheds.GetFeeSched(pat,insPlanList,patPlanList,subList,provNum);
			proc.ProcFee=Fees.GetAmount0(proc.CodeNum,feeSch,proc.ClinicNum,provNum);
			if(insPlanPrimary!=null && insPlanPrimary.PlanType=="p") {//PPO
				double provFee=Fees.GetAmount0(proc.CodeNum,Providers.GetProv(provNum).FeeSched,proc.ClinicNum,provNum);
				proc.ProcFee=Math.Max(proc.ProcFee,provFee);//use greater of standard fee or ins fee
			}
			ProcedureCode procCodeCur=ProcedureCodes.GetProcCode(proc.CodeNum);
			proc.BaseUnits=procCodeCur.BaseUnits;
			proc.SiteNum=pat.SiteNum;
			proc.RevCode=procCodeCur.RevenueCodeDefault;
			proc.DateEntryC=DateTime.Now;
			proc.PlaceService=(PlaceOfService)PrefC.GetInt(PrefName.DefaultProcedurePlaceService);//Default Proc Place of Service for the Practice is used.
			proc.ProcNum=Procedures.Insert(proc);
			Procedures.ComputeEstimates(proc,pat.PatNum,new List<ClaimProc>(),true,insPlanList,patPlanList,benefitList,pat.Age,subList);
			return proc;
		}

		///<summary>Creates a new procedure for every proc code passed in.  Make sure to make a security log after calling this method.
		///This method requires that Security.CurUser be set prior to invoking.  Returns an empty list if none were created for the patient.</summary>
		public static List<Procedure> CreateProcsForPat(long patNum,List<long> listProcCodeNums,string surf,string toothNum,ProcStat procStatus
			,long provNum,long aptNum) 
		{
			//No need to check RemotingRole; no call to db.
			List<Procedure> listProcedures=new List<Procedure>();
			Patient patient=Patients.GetPat(patNum);
			List<InsSub> subList=InsSubs.RefreshForFam(Patients.GetFamily(patNum));
			List<InsPlan> insPlanList=InsPlans.RefreshForSubList(subList);
			List<PatPlan> patPlanList=PatPlans.Refresh(patNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlanList,subList);
			foreach(long codeNum in listProcCodeNums) {
				Procedure proc=CreateProcForPat(patient,codeNum,surf,toothNum,procStatus,provNum,aptNum,subList,insPlanList,patPlanList,benefitList);
				if(proc!=null) {
					listProcedures.Add(proc);
				}
			}
			return listProcedures;
		}

		///<summary>Creates the auto ortho procedure for the passed in patient </summary>
		public static Procedure CreateOrthoAutoProcsForPat(long patNum,long codeNum,long provNum,long clinicNum, DateTime procDate) {
			//No need to check RemotingRole; no call to db.
			Procedure proc=new Procedure {
				PatNum=patNum,
				CodeNum=codeNum,
				ProvNum=provNum,
				ClinicNum=clinicNum,
				ProcStatus=ProcStat.C,
				Surf="",
				ToothNum="",
				UserNum=Security.CurUser.UserNum,
				ProcDate=procDate,
				DateEntryC=DateTime.Today,
				SecDateEntry=DateTime.Today,
				ProcFee=0,
				PlaceService=(PlaceOfService)PrefC.GetInt(PrefName.DefaultProcedurePlaceService)//Default Proc Place of Service for the Practice is used. 
			};
			proc.ProcNum = Procedures.Insert(proc);
			return proc;
		}

		public static void UpdateAptNum(long procNum,long newAptNum) {
			UpdateAptNums(new List<long>() { procNum },newAptNum);
		}

		public static void UpdateAptNums(List<long> listProcNums,long newAptNum,bool isPlannedAptNum=false) {
			if(listProcNums==null || listProcNums.Count==0) {
				return;
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listProcNums,newAptNum,isPlannedAptNum);
				return;
			}
			string command="UPDATE procedurelog "
				+"SET "+(isPlannedAptNum?"PlannedAptNum =":"AptNum =")+POut.Long(newAptNum)+" "
				+"WHERE ProcNum IN ("+string.Join(",",listProcNums.Select(x => POut.Long(x)))+")";
			Db.NonQ(command);
		}

		//public static void UpdatePlannedAptNum(long procNum,long newPlannedAptNum) {
		//	if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
		//		Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newPlannedAptNum);
		//		return;
		//	}
		//	string command="UPDATE procedurelog SET PlannedAptNum = "+POut.Long(newPlannedAptNum)
		//		+" WHERE ProcNum = "+POut.Long(procNum);
		//	Db.NonQ(command);
		//}

		public static void UpdatePriority(long procNum,long newPriority) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newPriority);
				return;
			}
			string command="UPDATE procedurelog SET Priority = "+POut.Long(newPriority)
				+" WHERE ProcNum = "+POut.Long(procNum);
			Db.NonQ(command);
		}

		public static void UpdateFee(long procNum,double newFee) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newFee);
				return;
			}
			string command="UPDATE procedurelog SET ProcFee = "+POut.Double(newFee)
				+" WHERE ProcNum = "+POut.Long(procNum);
			Db.NonQ(command);
		}

		///<summary>Updates IsCpoe column in the procedurelog table with the passed in value for the corresponding procedure.
		///This method explicitly used instead of the generic Update method because this (and only this) field can get updated when a user cancels out
		///of the Procedure Edit window and no other changes should accidentally make their way to the database.</summary>
		public static void UpdateCpoeForProc(long procNum,bool isCpoe) {
			//No need to check RemotingRole; no call to db.
			UpdateCpoeForProcs(new List<long>() { procNum },isCpoe);
		}

		///<summary>Updates IsCpoe column in the procedurelog table with the passed in value for the corresponding procedures.
		///This method explicitly used instead of the generic Update method because this (and only this) field can get updated when a user cancels out
		///of the Procedure Edit window and no other changes should accidentally make their way to the database.</summary>
		public static void UpdateCpoeForProcs(List<long> listProcNums,bool isCpoe) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listProcNums,isCpoe);
				return;
			}
			if(listProcNums==null || listProcNums.Count < 1) {
				return;
			}
			string command="UPDATE procedurelog SET IsCpoe = "+POut.Bool(isCpoe)
				+" WHERE ProcNum IN ("+string.Join(",",listProcNums)+")";
			Db.NonQ(command);
		}

		///<summary>Gets one procedure directly from the db. Option to include the note.
		///If the procNum is 0 or if the procNum does not exist in the database, this will return a new Procedure object with uninitialized fields.  
		///If a new Procedure object is sent through the middle tier with an uninitialized ProcStatus=0, this will fail validation since the ProcStatus 
		///enum starts with 1.  Make sure to handle a new Procedure object with uninitialized fields.</summary>
		public static Procedure GetOneProc(long procNum,bool includeNote) {
			//Doing this before remoting role check because Middle Tier can't serialize a Procedure with ProcStatus=0.
			if(procNum==0) {
				return new Procedure();
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Procedure>(MethodBase.GetCurrentMethod(),procNum,includeNote);
			}
			Procedure proc=Crud.ProcedureCrud.SelectOne(procNum);
			if(proc==null) {
				return new Procedure();//This will throw if Middle Tier. Haven't come up with a good solution yet.
			}
			if(!includeNote) {
				return proc;
			}
			string command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(procNum)+" ORDER BY EntryDateTime DESC";
			DbHelper.LimitOrderBy(command,1);
			DataTable table=Db.GetTable(command);
			if(table.Rows.Count==0) {
				return proc;
			}
			proc.UserNum   =PIn.Long(table.Rows[0]["UserNum"].ToString());
			proc.Note      =PIn.String(table.Rows[0]["Note"].ToString());
			proc.SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
			proc.Signature =PIn.String(table.Rows[0]["Signature"].ToString());
			return proc;
		}

		///<summary>Gets one procedure directly from the db.  Option to include the note.  If the procNum is 0 or if the procNum does not exist in the database, this will return a new Procedure object with uninitialized fields.  If, for example, a new Procedure object is sent through the middle tier with an uninitialized ProcStatus=0, this will fail validation since the ProcStatus enum starts with 1.  Make sure to handle a new Procedure object with uninitialized fields.</summary>
		public static List<Procedure> GetManyProc(List<long> listProcNums,bool includeNote) {
			if(listProcNums==null || listProcNums.Count==0) {
				return new List<Procedure>();
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listProcNums,includeNote);
			}
			string command="";
			if(!includeNote) {
				command="SELECT * FROM procedurelog WHERE ProcNum IN ("+string.Join(",",listProcNums)+")";
				return Crud.ProcedureCrud.SelectMany(command);
			}
			command="SELECT procedurelog.*"
					+",procnoterow.UserNum NoteUserNum,procnoterow.Note NoteNote,procnoterow.SigIsTopaz NoteSigIsTopaz,procnoterow.Signature NoteSignature "
				+"FROM procedurelog "
				+"LEFT JOIN (SELECT ProcNum,MAX(EntryDateTime) EntryDateTime FROM procnote GROUP BY ProcNum) procnotemax ON procnotemax.ProcNum=procedurelog.ProcNum "
				+"LEFT JOIN procnote procnoterow ON procnoterow.ProcNum=procedurelog.ProcNum AND procnoterow.EntryDateTime=procnotemax.EntryDateTime "
				+"WHERE procedurelog.ProcNum IN ("+string.Join(",",listProcNums)+")";
			//ProcNote stuff
			DataTable table=Db.GetTable(command);
			List<Procedure> listProcs=Crud.ProcedureCrud.TableToList(table);
			for(int i=0;i<table.Rows.Count;i++) {
				if(table.Rows[i]["NoteNote"].ToString()=="") {
					continue;
				}
				listProcs[i].UserNum=PIn.Long(table.Rows[i]["NoteUserNum"].ToString());
				listProcs[i].Note=PIn.String(table.Rows[i]["NoteNote"].ToString());
				listProcs[i].SigIsTopaz=PIn.Bool(table.Rows[i]["NoteSigIsTopaz"].ToString());
				listProcs[i].Signature=PIn.String(table.Rows[i]["NoteSignature"].ToString());
			}
			return listProcs;
		}

		///<summary>Gets Procedures for a single appointment directly from the database</summary>
		public static List<Procedure> GetProcsForSingle(long aptNum,bool isPlanned) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),aptNum,isPlanned);
			}
			string command;
			if(isPlanned) {
				command = "SELECT * from procedurelog WHERE PlannedAptNum = '"+POut.Long(aptNum)+"'";
			}
			else {
				command = "SELECT * from procedurelog WHERE AptNum = '"+POut.Long(aptNum)+"'";
			}
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all Procedures that need to be displayed in FormApptEdit.</summary>
		public static List<Procedure> GetProcsForApptEdit(Appointment appt) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),appt);
			}
			string command="SELECT procedurelog.* FROM procedurelog "
				+"LEFT JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
				+"WHERE procedurelog.PatNum="+POut.Long(appt.PatNum)+" "
				+"AND (procedurelog.ProcStatus="+POut.Long((int)ProcStat.TP)+" ";
			if(appt.AptNum!=0) {//Filling grid for a new appt
				command+="OR ";
				if(appt.AptStatus==ApptStatus.Planned) {
					command+="procedurelog.PlannedAptNum="+POut.Long(appt.AptNum)+" ";
				}
				else {//Scheduled
					command+="procedurelog.AptNum="+POut.Long(appt.AptNum)+" ";
				}
			}
			if(appt.AptStatus==ApptStatus.Scheduled || appt.AptStatus==ApptStatus.Complete 
				|| appt.AptStatus==ApptStatus.Broken)
			{
					command+="OR (procedurelog.AptNum=0 AND procedurelog.ProcStatus="+POut.Long((int)ProcStat.C)+" AND "
						+DbHelper.DtimeToDate("procedurelog.ProcDate")+"="+POut.Date(appt.AptDateTime)+") ";
			}
			command+=") AND procedurelog.ProcStatus != "+POut.Long((int)ProcStat.D)+" AND procedurecode.IsCanadianLab=0";
			List<Procedure> result=Crud.ProcedureCrud.SelectMany(command);
			for(int i=0;i<result.Count;i++){
				command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(result[i].ProcNum)+" ORDER BY EntryDateTime DESC";
				command=DbHelper.LimitOrderBy(command,1);
				DataTable table=Db.GetTable(command);
				if(table.Rows.Count==0) {
					continue;
				}
				result[i].UserNum   =PIn.Long(table.Rows[0]["UserNum"].ToString());
				result[i].Note      =PIn.String(table.Rows[0]["Note"].ToString());
				result[i].SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
				result[i].Signature =PIn.String(table.Rows[0]["Signature"].ToString());
			}
			result.Sort(ProcedureLogic.CompareProcedures);
			return result;
		}

		///<summary>Gets all Procedures for a single date for the specified patient directly from the database.  Excludes deleted procs.</summary>
		public static List<Procedure> GetProcsForPatByDate(long patNum,DateTime date) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,date);
			}
			string command="SELECT * FROM procedurelog "
				+"WHERE PatNum="+POut.Long(patNum)+" "
				+"AND (ProcDate="+POut.Date(date)+" OR DateEntryC="+POut.Date(date)+") "
				+"AND ProcStatus!="+POut.Int((int)ProcStat.D);//exclude deleted procs
			List<Procedure> result=Crud.ProcedureCrud.SelectMany(command);
			for(int i=0;i<result.Count;i++){
				command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(result[i].ProcNum)+" ORDER BY EntryDateTime DESC";
				command=DbHelper.LimitOrderBy(command,1);
				DataTable table=Db.GetTable(command);
				if(table.Rows.Count==0) {
					continue;
				}
				result[i].UserNum   =PIn.Long(table.Rows[0]["UserNum"].ToString());
				result[i].Note      =PIn.String(table.Rows[0]["Note"].ToString());
				result[i].SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
				result[i].Signature =PIn.String(table.Rows[0]["Signature"].ToString());
			}
			return result;
		}

		///<summary>Gets all procedures associated with corresponding claimprocs. Returns empty procedure list if an empty list was passed in.</summary>
		public static List<Procedure> GetProcsFromClaimProcs(List<ClaimProc> listClaimProc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listClaimProc);
			}
			if(listClaimProc.Count==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE ProcNum IN (";
			for(int i=0;i<listClaimProc.Count;i++) {
				if(i>0) {
					command+=",";
				}
				command+=listClaimProc[i].ProcNum;
			}
			command+=")";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets a list of TP procedures that are attached to scheduled appointments that are not flagged as CPOE.</summary>
		public static List<Procedure> GetProcsNonCpoeAttachedToApptsForProv(long provNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),provNum);
			}
			if(provNum==0) {
				return new List<Procedure>();
			}
			string command="SELECT procedurelog.* "
				+"FROM procedurelog "
				+"INNER JOIN appointment ON procedurelog.AptNum=appointment.AptNum "
				+"INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
				+"WHERE procedurecode.IsRadiology=1 "
				+"AND appointment.AptStatus="+POut.Int((int)ApptStatus.Scheduled)+" "
				+"AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP)+" "
				+"AND procedurelog.IsCpoe=0 "
				+"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
				+"AND "+DbHelper.DtimeToDate("appointment.AptDateTime")+" >= "+DbHelper.Curdate()+" "
				+"ORDER BY appointment.AptDateTime";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets count of non-CPOE radiology procedures that are TP'd and attached to scheduled appointments for every provider who has ever had
		///an ehrprovkey.  Only used by the OpenDentalService AlertRadiologyProceduresThread.</summary>
		public static SerializableDictionary<long,long> GetCountNonCpoeProcsAttachedToAppts() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetSerializableDictionary<long,long>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT procedurelog.ProvNum,COUNT(*) procCount "
				+"FROM procedurelog USE INDEX (RadiologyProcs) "
				+"INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum AND procedurecode.IsRadiology=1 "
				+"INNER JOIN appointment ON appointment.AptNum=procedurelog.AptNum AND appointment.AptStatus="+POut.Int((int)ApptStatus.Scheduled)+" AND appointment.AptDateTime>=CURDATE() "
				+"WHERE procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP)+" "
				+"AND procedurelog.IsCpoe=0 "
				+"AND procedurelog.ProvNum IN("
					+"SELECT ProvNum FROM provider "
					+"WHERE provider.LName!='' "//SQL standard says ''=='  ', an empty string is equal to a string composed entirely of any number of spaces
					+"AND provider.FName!='' "//so no need to trim LName or FName
					+"AND EXISTS("
						+"SELECT * FROM ehrprovkey "
						+"WHERE provider.LName=ehrprovkey.LName "
						+"AND provider.FName=ehrprovkey.FName "
					+")"
				+") "
				+"GROUP BY procedurelog.ProvNum";
			return Db.GetTable(command).Select().ToSerializableDictionary(x => PIn.Long(x["ProvNum"].ToString()),x => PIn.Long(x["procCount"].ToString()));
		}

		///<summary>Gets a list of TP or C procedures starting a year into the past that are flagged as IsRadiology and IsCpoe for the specified patient.
		///Primarily used for showing patient specific MU data in the EHR dashboard.</summary>
		public static List<Procedure> GetProcsRadiologyCpoeForPat(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum);
			}
			//Since this is used for the dashboard and not directly used in any reporting calculations, we do not need to worry about the date that the
			// office updated past v15.4.1.
			DateTime dateStart=new DateTime(DateTime.Now.Year,1,1);//January first of this year.
			DateTime dateEnd=dateStart.AddYears(1).AddDays(-1);//Last day in December of this year.
			string command="SELECT procedurelog.* "
				+"FROM procedurelog "
				+"INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum AND procedurecode.IsRadiology=1 "
				+"WHERE procedurelog.ProcStatus IN ("+POut.Int((int)ProcStat.C)+","+POut.Int((int)ProcStat.TP)+") "
				+"AND procedurelog.PatNum="+POut.Long(patNum)+" "
				+"AND procedurelog.IsCpoe=1 "
				+"AND procedurelog.DateEntryC BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd);
			return Crud.ProcedureCrud.SelectMany(command);
		}

		public static List<Procedure> GetProcsByStatusForPat(long patNum,params ProcStat[] procStatuses) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,procStatuses);
			}
			if(procStatuses==null || procStatuses.Length==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)+" AND ProcStatus IN ("+string.Join(",",procStatuses.Select(x => (int)x))+")";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets a string in M/yy format for the most recent completed procedure in the specified code range.  Gets directly from the database.</summary>
		public static string GetRecentProcDateString(long patNum,DateTime aptDate,string procCodeRange) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetString(MethodBase.GetCurrentMethod(),patNum,aptDate,procCodeRange);
			}
			if(aptDate.Year<1880) {
				aptDate=DateTime.Today;
			}
			string code1;
			string code2;
			if(procCodeRange.Contains("-")) {
				string[] codeSplit=procCodeRange.Split('-');
				code1=codeSplit[0].Trim();
				code2=codeSplit[1].Trim();
			}
			else {
				code1=procCodeRange.Trim();
				code2=procCodeRange.Trim();
			}
			string command="SELECT MAX(ProcDate) FROM procedurelog "
				+"LEFT JOIN procedurecode ON procedurecode.CodeNum=procedurelog.CodeNum "
				+"WHERE PatNum="+POut.Long(patNum)+" "
				//+"AND CodeNum="+POut.Long(codeNum)+" "
				+"AND ProcDate < "+POut.Date(aptDate)+" "
				+"AND (ProcStatus ="+POut.Int((int)ProcStat.C)+" "
				+"OR ProcStatus ="+POut.Int((int)ProcStat.EC)+" "
				+"OR ProcStatus ="+POut.Int((int)ProcStat.EO)+") "
				+"AND procedurecode.ProcCode >= '"+POut.String(code1)+"' "
				+"AND procedurecode.ProcCode <= '"+POut.String(code2)+"' ";
			DateTime date=PIn.Date(Db.GetScalar(command));
			if(date.Year<1880) {
				return "";
			}
			return date.ToString("M/yy");
		}

		///<summary>Gets the first completed procedure within the family.  Used to determine the earliest date the family became a customer.</summary>
		public static Procedure GetFirstCompletedProcForFamily(long guarantor) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Procedure>(MethodBase.GetCurrentMethod(),guarantor);
			}
			string command="SELECT procedurelog.* FROM procedurelog "
				+"LEFT JOIN patient ON procedurelog.PatNum=patient.PatNum AND patient.Guarantor="+POut.Long(guarantor)+" "
				+"WHERE "+DbHelper.Year("procedurelog.ProcDate")+">1 "
				+"AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+" "
				+"ORDER BY procedurelog.ProcDate";
			command=DbHelper.LimitOrderBy(command,1);
			return Crud.ProcedureCrud.SelectOne(command);
		}

		///<summary>Gets a list (procsMultApts is a struct of type ProcDesc(aptNum, string[], and production) of all the procedures attached to the specified appointments.  Then, use GetProcsOneApt to pull procedures for one appointment from this list or GetProductionOneApt.  This process requires only one call to the database.  "myAptNums" is the list of appointments to get procedures for.  isForNext gets procedures for a list of next appointments rather than regular appointments.</summary>
		public static List<Procedure> GetProcsMultApts(List<long> myAptNums,bool isForPlanned=false) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),myAptNums,isForPlanned);
			}
			if(myAptNums.Count==0) {
				return new List<Procedure>();
			}
			string strAptNums="";
			for(int i=0;i<myAptNums.Count;i++) {
				if(i>0) {
					strAptNums+=" OR";
				}
				if(isForPlanned) {
					strAptNums+=" PlannedAptNum='"+POut.Long(myAptNums[i])+"'";
				}
				else {
					strAptNums+=" AptNum='"+POut.Long(myAptNums[i])+"'";
				}
			}
			string command = "SELECT * FROM procedurelog WHERE"+strAptNums;
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets procedures for one appointment by looping through the procsMultApts which was filled previously from GetProcsMultApts.</summary>
		public static Procedure[] GetProcsOneApt(long myAptNum,List<Procedure> procsMultApts) {
			//No need to check RemotingRole; no call to db.
			ArrayList al=new ArrayList();
			for(int i=0;i<procsMultApts.Count;i++) {
				if(procsMultApts[i].AptNum==myAptNum) {
					al.Add(procsMultApts[i].Copy());
				}
			}
			Procedure[] retVal=new Procedure[al.Count];
			al.CopyTo(retVal);
			return retVal;
		}

		/////<summary>Gets the production for one appointment by looping through the procsMultApts which was filled previously from GetProcsMultApts.</summary>
		//public static double GetProductionOneApt(long myAptNum,Procedure[] procsMultApts,bool isPlanned) {
		//	//No need to check RemotingRole; no call to db.
		//	double retVal=0;
		//	for(int i=0;i<procsMultApts.Length;i++) {
		//		if(isPlanned && procsMultApts[i].PlannedAptNum==myAptNum) {
		//			retVal+=procsMultApts[i].ProcFee*(procsMultApts[i].BaseUnits+procsMultApts[i].UnitQty);
		//		}
		//		if(!isPlanned && procsMultApts[i].AptNum==myAptNum) {
		//			retVal+=procsMultApts[i].ProcFee*(procsMultApts[i].BaseUnits+procsMultApts[i].UnitQty);
		//		}
		//	}
		//	return retVal;
		//}

		///<summary>Used in FormClaimEdit,FormClaimPrint,FormClaimPayTotal,ContrAccount etc to get description of procedure. Procedure list needs to include the procedure we are looking for.  If procNum could be 0 (e.g. total payment claimprocs) or if the list does not contain the procNum, this will return a new Procedure with uninitialized fields.  If, for example, a new Procedure object is sent through the middle tier with an uninitialized ProcStatus=0, this will fail validation since the ProcStatus enum starts with 1.  Make sure to handle a new Procedure object with uninitialized fields.</summary>
		public static Procedure GetProcFromList(List<Procedure> list,long procNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<list.Count;i++) {
				if(procNum==list[i].ProcNum) {
					return list[i];
				}
			}
			//MessageBox.Show("Error. Procedure not found");
			return new Procedure();
		}

		///<summary>Sets the patient.DateFirstVisit if necessary. A visitDate is required to be passed in because it may not be today's date. This is triggered by:
		///1. When any procedure is inserted regardless of status. From Chart or appointment. If no C procs and date blank, changes date.
		///2. When updating a procedure to status C. If no C procs, update visit date. Ask user first?
		///  #2 was recently changed to only happen if date is blank or less than 7 days old.
		///3. When an appointment is deleted. If no C procs, clear visit date.
		///  #3 was recently changed to not occur at all unless appt is of type IsNewPatient
		///4. Changing an appt date of type IsNewPatient. If no C procs, change visit date.
		///Old: when setting a procedure complete in the Chart module or the ProcEdit window.  Also when saving an appointment that is marked IsNewPat.</summary>
		public static void SetDateFirstVisit(DateTime visitDate,int situation,Patient pat) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),visitDate,situation,pat);
				return;
			}
			if(situation==1) {
				if(pat.DateFirstVisit.Year>1880) {
					return;//a date has already been set.
				}
			}
			if(situation==2) {
				if(pat.DateFirstVisit.Year>1880 && pat.DateFirstVisit<DateTime.Now.AddDays(-7)) {
					return;//a date has already been set.
				}
			}
			string command="SELECT COUNT(*) from procedurelog "
				+"INNER JOIN procedurecode on procedurecode.CodeNum = procedurelog.CodeNum "
					+"AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
				+"WHERE PatNum = '"+POut.Long(pat.PatNum)+"' "
				+"AND ProcStatus = '2'";
			DataTable table=Db.GetTable(command);
			if(PIn.Long(table.Rows[0][0].ToString())>0) {
				return;//there are already completed procs (for all situations)
			}
			if(situation==2) {
				//ask user first?
			}
			if(situation==3) {
				command="UPDATE patient SET DateFirstVisit ="+POut.Date(new DateTime(0001,01,01))
					+" WHERE PatNum ='"
					+POut.Long(pat.PatNum)+"'";
			}
			else {
				command="UPDATE patient SET DateFirstVisit ="
					+POut.Date(visitDate)+" WHERE PatNum ='"
					+POut.Long(pat.PatNum)+"'";
			}
			//MessageBox.Show(cmd.CommandText);
			//dcon.NonQ(command);
			Db.NonQ(command);
		}

		///<summary>Gets all completed procedures within a date range with optional ProcCodeNum and PatientNum filters. Date range is inclusive.  
		///If including GroupNotes, make sure to include the GroupNote code in the list of ProcCodeNums when explicitly specifying code nums.</summary>
		public static List<Procedure> GetCompletedForDateRange(DateTime dateStart,DateTime dateStop,List<long> listProcCodeNums=null
			,List<long> listPatNums=null,bool includeNote=false,bool includeGroupNote=false) 
		{
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),dateStart,dateStop,listProcCodeNums,listPatNums,includeNote
					,includeGroupNote);
			}
			string command="";
			string whereClause="WHERE procedurelog.ProcStatus IN("+POut.Int((int)ProcStat.C);
			if(includeGroupNote) {
				whereClause+=","+POut.Int((int)ProcStat.EC);
			}
			whereClause+=") AND procedurelog.ProcDate>="+POut.Date(dateStart)+" AND procedurelog.ProcDate<="+POut.Date(dateStop);
			if (listProcCodeNums!=null && listProcCodeNums.Count > 0) {
				whereClause+=" AND procedurelog.CodeNum IN ("+string.Join(",", listProcCodeNums)+")";
			}
			if(listPatNums!=null && listPatNums.Count > 0) {
				whereClause+=" AND procedurelog.PatNum IN ("+string.Join(",",listPatNums)+")";
			}
			command="SELECT * FROM procedurelog "+whereClause;
			List<Procedure> listProcs=Crud.ProcedureCrud.SelectMany(command);
			if(!includeNote || listProcs.Count==0) {
				return listProcs;
			}
			//ProcNote stuff
			command="SELECT procnote.ProcNum,procnote.UserNum,procnote.Note,procnote.SigIsTopaz,procnote.Signature "
				+"FROM procnote "
				+"INNER JOIN ("
					+"SELECT procnote.ProcNum,MAX(procnote.EntryDateTime) EntryDateTime "
					+"FROM procnote "
					+"WHERE procnote.ProcNum IN("+string.Join(",",listProcs.Select(x => x.ProcNum))+") "
					+"GROUP BY procnote.ProcNum"
				+") procnotemax ON procnote.ProcNum=procnotemax.ProcNum AND procnote.EntryDateTime=procnotemax.EntryDateTime";
			Dictionary<long,DataRow> dictProcNoteRows=Db.GetTable(command).Select().ToDictionary(x => PIn.Long(x["ProcNum"].ToString()));
			if(dictProcNoteRows.Count==0) {//no notes for the procs, just return the list of procs
				return listProcs;
			}
			foreach(Procedure proc in listProcs) {
				DataRow row;
				if(!dictProcNoteRows.TryGetValue(proc.ProcNum,out row) || string.IsNullOrEmpty(row["Note"].ToString())) {
					continue;
				}
				proc.UserNum=PIn.Long(row["UserNum"].ToString());
				proc.Note=PIn.String(row["Note"].ToString());
				proc.SigIsTopaz=PIn.Bool(row["SigIsTopaz"].ToString());
				proc.Signature=PIn.String(row["Signature"].ToString());
			}
			return listProcs;
		}

		///<summary>Gets all completed procedures having procedurelog.DateComplete within the date range. Date range is inclusive.</summary>
		public static List<Procedure> GetCompletedByDateCompleteForDateRange(DateTime dateStart,DateTime dateStop) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),dateStart,dateStop);
			}
			string command="SELECT * FROM procedurelog WHERE ProcStatus="+POut.Int((int)ProcStat.C)
				+" AND DateComplete>="+POut.Date(dateStart)
				+" AND DateComplete<="+POut.Date(dateStop);
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Determines what the ProcFee should be based on the given inputs.</summary>
		public static double GetProcFee(Patient pat,List<PatPlan> listPatPlans,List<InsSub> listInsSubs,List<InsPlan> listInsPlans,
			long procCodeNum,long procProvNum,long procClinicNum,string procMedicalCode)
		{
			//No need to check RemotingRole; no call to db.
			double procFeeRet;
			InsPlan insPlanPrimary=null;
			if(listPatPlans.Count>0) {
				InsSub insSubPrimary=InsSubs.GetSub(listPatPlans[0].InsSubNum,listInsSubs);
				insPlanPrimary=InsPlans.GetPlan(insSubPrimary.PlanNum,listInsPlans);
			}
			//Get fee schedule and fee amount for medical or dental.
			if(PrefC.GetBool(PrefName.MedicalFeeUsedForNewProcs) && !string.IsNullOrEmpty(procMedicalCode)) {
				long feeSch=FeeScheds.GetMedFeeSched(pat,listInsPlans,listPatPlans,listInsSubs,procProvNum);
				procFeeRet=Fees.GetAmount0(ProcedureCodes.GetProcCode(procMedicalCode).CodeNum,feeSch,procClinicNum,procProvNum);
			}
			else {
				long feeSch=FeeScheds.GetFeeSched(pat,listInsPlans,listPatPlans,listInsSubs,procProvNum);
				procFeeRet=Fees.GetAmount0(procCodeNum,feeSch,procClinicNum,procProvNum);
			}
			if(insPlanPrimary!=null && insPlanPrimary.PlanType=="p") {//PPO
				double ucrFee=Fees.GetAmount0(procCodeNum,Providers.GetProv(Patients.GetProvNum(pat)).FeeSched,procClinicNum,procProvNum);
				if(procFeeRet < ucrFee || PrefC.GetBool(PrefName.InsPpoAlwaysUseUcrFee)) {
					procFeeRet=ucrFee;
				}
			}
			return procFeeRet;
		}

		///<summary>Called from FormApptsOther when creating a new appointment.  Returns true if there are any procedures marked complete for this patient.  The result is that the NewPt box on the appointment won't be checked.</summary>
		public static bool AreAnyComplete(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT COUNT(*) FROM procedurelog "
				+"INNER JOIN procedurecode on procedurecode.CodeNum = procedurelog.CodeNum "
					+"AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
				+"WHERE PatNum="+patNum.ToString()
				+" AND ProcStatus=2";
			DataTable table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()=="0") {
				return false;
			}
			else return true;
		}

		///<summary>Called from AutoCodeItems.  Makes a call to the database to determine whether the specified tooth has been extracted or will be extracted. This could then trigger a pontic code.</summary>
		public static bool WillBeMissing(string toothNum,long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),toothNum,patNum);
			}
			//first, check for missing teeth
			string command="SELECT COUNT(*) FROM toothinitial "
				+"WHERE ToothNum='"+toothNum+"' "
				+"AND PatNum="+POut.Long(patNum)
				+" AND InitialType=0";//missing
			DataTable table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()!="0") {
				return true;
			}
			//then, check for a planned extraction
			command="SELECT COUNT(*) FROM procedurelog,procedurecode "
				+"WHERE procedurelog.CodeNum=procedurecode.CodeNum "
				+"AND procedurelog.ToothNum='"+toothNum+"' "
				+"AND procedurelog.PatNum="+patNum.ToString()+" "
				+"AND procedurelog.ProcStatus <> "+POut.Int((int)ProcStat.D)+" "//Not deleted procedures
				+"AND procedurecode.PaintType=1";//extraction
			table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()!="0") {
				return true;
			}
			return false;
		}

		public static void AttachToApt(long procNum,long aptNum,bool isPlanned) {
			//No need to check RemotingRole; no call to db.
			List<long> procNums=new List<long>();
			procNums.Add(procNum);
			AttachToApt(procNums,aptNum,isPlanned);
		}

		public static void AttachToApt(List<long> procNums,long aptNum,bool isPlanned) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNums,aptNum,isPlanned);
				return;
			}
			if(procNums.Count==0) {
				return;
			}
			string command="UPDATE procedurelog SET ";
			if(isPlanned) {
				command+="PlannedAptNum";
			}
			else {
				command+="AptNum";
			}
			command+="="+POut.Long(aptNum)+" WHERE ";
			for(int i=0;i<procNums.Count;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="ProcNum="+POut.Long(procNums[i]);
			}
			Db.NonQ(command);
		}

		public static void DetachFromApt(List<long> procNums,bool isPlanned) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNums,isPlanned);
				return;
			}
			if(procNums.Count==0) {
				return;
			}
			string command="UPDATE procedurelog SET ";
			if(isPlanned) {
				command+="PlannedAptNum";
			}
			else {
				command+="AptNum";
			}
			command+="=0 WHERE ";
			for(int i=0;i<procNums.Count;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="ProcNum="+POut.Long(procNums[i]);
			}
			Db.NonQ(command);
		}

		public static void DetachFromInvoice(long statementNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),statementNum);
				return;
			}
			string command="UPDATE procedurelog SET StatementNum=0 WHERE StatementNum="+POut.Long(statementNum);
			Db.NonQ(command);
		}

		public static void DetachAllFromInvoices(List<long> listStatementNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listStatementNums);
				return;
			}
			if(listStatementNums==null || listStatementNums.Count==0) {
				return;
			}
			string command="UPDATE procedurelog SET StatementNum=0 WHERE StatementNum IN ("+string.Join(",",listStatementNums.Select(x => POut.Long(x)))+")";
			Db.NonQ(command);
		}

		public static Procedure GetProcForPatByToothSurfStat(long patNum,int toothNum,string surf,ProcStat procStat) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Procedure>(MethodBase.GetCurrentMethod(),patNum,toothNum,surf,procStat);
			}
			string command="SELECT * FROM procedurelog "
				+"WHERE PatNum="+POut.Long(patNum)+" "
				+"AND Surf='"+POut.String(surf)+"' "
				+"AND ToothNum='"+POut.Int(toothNum)+"' "
				+"AND ProcStatus="+POut.Int((int)procStat);
			return Crud.ProcedureCrud.SelectOne(command);
		}

		///<summary>Returns table of patients with completed procedure count and most recent completed procedure ProcDate for each provider.  Used for
		///reassigning patients PriProv to their most used provider with most recent ProcDate as a tie-breaker.  Excludes hidding providers.</summary>
		public static DataTable GetTablePatProvUsed(List<long> listPatNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),listPatNums);
			}
			string command="SELECT PatNum,procedurelog.ProvNum,MAX(ProcDate) maxProcDate,COUNT(ProcNum) procCount "
				+"FROM procedurelog "
				+"INNER JOIN provider ON procedurelog.ProvNum=provider.ProvNum AND provider.IsHidden=0 "
				+"WHERE PatNum IN ("+string.Join(",",listPatNums)+") "
				+"AND ProcStatus="+POut.Int((int)ProcStat.C)+" "
				+"GROUP BY procedurelog.ProvNum,PatNum";
			return Db.GetTable(command);
		}


		//--------------------Taken from Procedure class--------------------------------------------------


		/*
		///<summary>Gets allowedOverride for this procedure based on supplied claimprocs. Includes all claimproc types.  Only used in main TP module when calculating PPOs. The claimProc array typically includes all claimProcs for the patient, but must at least include all claimprocs for this proc.</summary>
		public static double GetAllowedOverride(Procedure proc,ClaimProc[] claimProcs,int priPlanNum) {
			//double retVal=0;
			for(int i=0;i<claimProcs.Length;i++) {
				if(claimProcs[i].ProcNum==proc.ProcNum && claimProcs[i].PlanNum==priPlanNum) {
					return claimProcs[i].AllowedOverride;
					//retVal+=claimProcs[i].WriteOff;
				}
			}
			return 0;//retVal;
		}*/

		/*
		///<summary>Gets total writeoff for this procedure based on supplied claimprocs. Includes all claimproc types.  Only used in main TP module. The claimProc array typically includes all claimProcs for the patient, but must at least include all claimprocs for this proc.</summary>
		public static double GetWriteOff(Procedure proc,List<ClaimProc> claimProcs) {
			//No need to check RemotingRole; no call to db.
			double retVal=0;
			for(int i=0;i<claimProcs.Count;i++) {
				if(claimProcs[i].ProcNum==proc.ProcNum) {
					retVal+=claimProcs[i].WriteOff;
				}
			}
			return retVal;
		}*/

		///<summary>Used in ContrAccount.CreateClaim when validating selected procedures. Returns true if there is any claimproc for this procedure and plan which is marked NoBillIns.  The claimProcList can be all claimProcs for the patient or only those attached to this proc. Will be true if any claimProcs attached to this procedure are set NoBillIns.</summary>
		public static bool NoBillIns(Procedure proc,List<ClaimProc> claimProcList,long planNum) {
			//No need to check RemotingRole; no call to db.
			if(proc==null) {
				return false;
			}
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==proc.ProcNum
					&& claimProcList[i].PlanNum==planNum
					&& claimProcList[i].NoBillIns) {
					return true;
				}
			}
			return false;
		}

		///<summary>Called from FormProcEdit to signal when to disable much of the editing in that form.  If the procedure is 'AttachedToClaim' then user
		///should not change it very much.  Also prevents user from Invalidating a locked procedure if attached to a claim.  The claimProcList can be all
		///claimProcs for the patient or only those attached to this proc.  Ignore preauth claims by setting isPreauthIncluded to false.</summary>
		public static bool IsAttachedToClaim(Procedure proc,List<ClaimProc> claimProcList,bool isPreauthIncluded=true) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==proc.ProcNum
					&& claimProcList[i].ClaimNum>0
					&& (claimProcList[i].Status==ClaimProcStatus.CapClaim
					|| claimProcList[i].Status==ClaimProcStatus.NotReceived
					|| (claimProcList[i].Status==ClaimProcStatus.Preauth && isPreauthIncluded)
					|| claimProcList[i].Status==ClaimProcStatus.Received
					|| claimProcList[i].Status==ClaimProcStatus.Supplemental
					)) {
					return true;
				}
			}
			return false;
		}

		///<summary>Only called from FormProcEdit.  When attached  to a claim and user clicks Edit Anyway, we need to know the oldest claim date for security reasons.  The claimProcsForProc should only be claimprocs for this procedure.</summary>
		public static DateTime GetOldestClaimDate(List<ClaimProc> claimProcsForProc) {
			//No need to check RemotingRole; no call to db.
			Claim claim;
			DateTime retVal=DateTime.Today;
			for(int i=0;i<claimProcsForProc.Count;i++) {
				if(claimProcsForProc[i].ClaimNum==0){
					continue;
				}
				if(claimProcsForProc[i].Status==ClaimProcStatus.CapClaim
					|| claimProcsForProc[i].Status==ClaimProcStatus.NotReceived
					|| claimProcsForProc[i].Status==ClaimProcStatus.Preauth
					|| claimProcsForProc[i].Status==ClaimProcStatus.Received
					|| claimProcsForProc[i].Status==ClaimProcStatus.Supplemental
					) 
				{
					claim=Claims.GetClaim(claimProcsForProc[i].ClaimNum);
					if(claim.DateSent<retVal){
						retVal=claim.DateSent;
					}
				}
			}
			return retVal;
		}

		///<summary>Only called from FormProcEditAll to signal when to disable much of the editing in that form. If the procedure is 'AttachedToClaim' then user should not change it very much.  The claimProcList can be all claimProcs for the patient or only those attached to this proc.</summary>
		public static bool IsAttachedToClaim(List<Procedure> procList,List<ClaimProc> claimprocList) {
			//No need to check RemotingRole; no call to db.
			for(int j=0;j<procList.Count;j++) {
				if(IsAttachedToClaim(procList[j],claimprocList)) {
					return true;
				}
			}
			return false;
		}

		///<summary>Queries the database to determine if this procedure is attached to a claim already.</summary>
		public static bool IsAttachedToClaim(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),procNum);
			}
			string command="SELECT COUNT(*) FROM claimproc "
				+"WHERE ProcNum="+POut.Long(procNum)+" "
				+"AND ClaimNum>0";
			DataTable table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()=="0") {
				return false;
			}
			return true;
		}

		///<summary>Used in ContrAccount.CreateClaim to validate that procedure is not already attached to a claim for this specific insPlan.  The claimProcList can be all claimProcs for the patient or only those attached to this proc.</summary>
		public static bool IsAlreadyAttachedToClaim(Procedure proc,List<ClaimProc> claimProcList,long insSubNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==proc.ProcNum
					&& claimProcList[i].InsSubNum==insSubNum
					&& claimProcList[i].ClaimNum>0
					&& claimProcList[i].Status!=ClaimProcStatus.Preauth) {
					return true;
				}
			}
			return false;
		}

		public static bool IsReferralAttached(long referralNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),referralNum);
			}
			string command="SELECT COUNT(*) FROM procedurelog WHERE OrderingReferralNum="+POut.Long(referralNum);
 			if(Db.GetCount(command)=="0") {
				return false;
			}
			return true;
		}

		///<summary>Only used in ContrAccount.OnInsClick to automate selection of procedures.  Returns true if this procedure should be selected.  This happens if there is at least one claimproc attached for this inssub that is an estimate, and it is not set to NoBillIns.  The list can be all ClaimProcs for patient, or just those for this procedure. The plan is the primary plan.</summary>
		public static bool NeedsSent(long procNum,long insSubNum,List<ClaimProc> claimProcList) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==procNum
					&& !claimProcList[i].NoBillIns
					&& claimProcList[i].InsSubNum==insSubNum
					&& claimProcList[i].Status==ClaimProcStatus.Estimate) 
				{
					return true;
				}
			}
			return false;
		}

		///<summary>Only used in ContrAccount.CreateClaim and FormRepeatChargeUpdate.CreateClaim to decide whether a given procedure has an estimate that can be used to attach to a claim for the specified plan.  Returns a valid claimProc if this procedure has an estimate attached that is not set to NoBillIns.  The list can be all ClaimProcs for patient, or just those for this procedure. Returns null if there are no claimprocs that would work.</summary>
		public static ClaimProc GetClaimProcEstimate(long procNum,List<ClaimProc> claimProcList,InsPlan plan,long insSubNum) {
			//No need to check RemotingRole; no call to db.
			//bool matchOfWrongType=false;
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==procNum
					&& !claimProcList[i].NoBillIns
					&& claimProcList[i].PlanNum==plan.PlanNum
					&& claimProcList[i].InsSubNum==insSubNum) 
				{
					if(plan.PlanType=="c") {
						if(claimProcList[i].Status==ClaimProcStatus.CapComplete) {
							return claimProcList[i];
						}
					}
					else {//any type except capitation
						if(claimProcList[i].Status==ClaimProcStatus.Estimate) {
							return claimProcList[i];
						}
					}
				}
			}
			return null;
		}

		/// <summary>Used by GetProcsForSingle and GetProcsMultApts to generate a short string description of a procedure.</summary>
		public static string ConvertProcToString(long codeNum,string surf,string toothNum,bool forAccount) {
			//No need to check RemotingRole; no call to db.
			string strLine="";
			ProcedureCode code=ProcedureCodes.GetProcCode(codeNum);
			switch(code.TreatArea) {
				case TreatmentArea.Surf:
					if(!forAccount) {
						strLine+="#"+Tooth.ToInternat(toothNum)+"-";//"#12-"
					}
					strLine+=Tooth.SurfTidyFromDbToDisplay(surf,toothNum);//"MOD-"
				break;
				case TreatmentArea.Tooth:
					if(!forAccount) {
						strLine+="#"+Tooth.ToInternat(toothNum)+"-";//"#12-"
					}
					break;
				default://area 3 or 0 (mouth)
					break;
				case TreatmentArea.Quad:
					strLine+=surf+"-";//"UL-"
					break;
				case TreatmentArea.Sextant:
					strLine+="S"+surf+"-";//"S2-"
					break;
				case TreatmentArea.Arch:
					strLine+=surf+"-";//"U-"
					break;
				case TreatmentArea.ToothRange:
					//strLine+=table.Rows[j][13].ToString()+" ";//don't show range
					break;
			}//end switch
			if(!forAccount) {
				strLine+=" "+code.AbbrDesc;
			}
			else if(code.LaymanTerm!=""){
				strLine+=" "+code.LaymanTerm;
			}
			else{
				strLine+=" "+code.Descript;
			}
			return strLine;
		}

		///<summary>Used to display procedure descriptions on appointments. The returned string also includes surf and toothNum.</summary>
		public static string GetDescription(Procedure proc) {
			//No need to check RemotingRole; no call to db.
			return ConvertProcToString(proc.CodeNum,proc.Surf,proc.ToothNum,false);
		}

		///<Summary>Supply the list of procedures attached to the appointment.  It will loop through each and assign the correct provider.
		///Also sets clinic.  Also sets procDate for TP procs.  js 7/24/12 This is not supposed to be called if the appointment is complete.
		///When isUpdatingFees is true, we also update the ProcFee based on PrefName.ProcFeeUpdatePrompt</Summary>
		public static void SetProvidersInAppointment(Appointment apt,List<Procedure> listProcOrig,bool isUpdatingFees) {
			//No need to check RemotingRole; no call to db.
			List<Procedure> listProcNew=new List<Procedure>();
			Procedure changedProc;
			for(int i=0;i<listProcOrig.Count;i++) {
				changedProc=listProcOrig[i].Copy();
				listProcNew.Add(changedProc);
				if(!IsProcComplEditAuthorized(changedProc)) {
					continue;
				}
				changedProc=UpdateProcInAppointment(apt,changedProc);
			}
			if(isUpdatingFees) {
				foreach(Procedure procCur in listProcNew) {
					if(!IsProcComplEditAuthorized(procCur)) {
						continue;
					}
					procCur.ProcFee=Fees.GetAmount0(procCur.CodeNum,Providers.GetProv(procCur.ProvNum).FeeSched,procCur.ClinicNum
						,procCur.ProvNum);
				}
			}
			Sync(listProcNew,listProcOrig,Security.CurUser.UserNum);
		}

		///<summary>Sets the provider and clinic for a proc based on the appt to which it is attached.  Also sets ProcDate for TP procs.  Changes are
		///reflected in proc returned, but not saved to the db (for synch later).</summary>
		public static Procedure UpdateProcInAppointment(Appointment apt,Procedure proc) {
			//No need to check RemotingRole; no call to db.
			if(!IsProcComplEditAuthorized(proc)) {
				//This check is redundant but helps future calls to not miss the security check.
				return proc;//Don't make any changes to procedure.
			}
			if(proc.ProcStatus!=ProcStat.C) {
				ProcedureCode procCode=ProcedureCodes.GetProcCode(proc.CodeNum);
				proc.ProvNum=GetProvNumFromAppointment(apt,proc,procCode);
			}
			proc.ClinicNum=apt.ClinicNum;
			if(proc.ProcStatus==ProcStat.TP) {
				proc.ProcDate=apt.AptDateTime;
			}
			return proc;
		}

		public static bool IsProcComplEditAuthorized(Procedure proc,bool includeCodeNumAndFee=false) {
			//No need to check RemotingRole; no call to db.
			if(!proc.ProcStatus.In(ProcStat.C,ProcStat.EO,ProcStat.EC)) {
				return true;//Don't check security if the procedure isn't completed (or EO/EC).
			}
			Permissions perm=Permissions.ProcComplEdit;
			if(proc.ProcStatus.In(ProcStat.EO,ProcStat.EC)) {
				perm=Permissions.ProcExistingEdit;
			}
			if(includeCodeNumAndFee) {
				return Security.IsAuthorized(perm,proc.ProcDate,proc.CodeNum,proc.ProcFee);
			}
			else {
				return Security.IsAuthorized(perm,proc.ProcDate,true);
			}
		}

		///<summary>Gets the ProvNum from the appointment that will be used on the procedure passed in.</summary>
		public static long GetProvNumFromAppointment(Appointment apt,Procedure proc,ProcedureCode procCode) {
			//No need to check RemotingRole; no call to db.
			long provNum;
			if(procCode.ProvNumDefault!=0) {//Override provider for procedures with a default provider
				provNum=procCode.ProvNumDefault;
			}
			else if(apt.ProvHyg==0 || !procCode.IsHygiene) {//either no hygiene prov on the appt or the proc is not a hygiene proc
				provNum=apt.ProvNum;
			}
			else {//appointment has a hygiene prov and the proc IsHygiene
				provNum=apt.ProvHyg;
			}
			return provNum;
		}

		///<summary>Returns true when we want to allow procedure fees to be changed.
		///Depending on PrefName.ProcFeeUpdatePrompt, may prompt user for input. We will need to show a MsgBox to the user when promptText is not empty after returing true.
		///Should only be called after identifying a procedurelog or appointment provider change.</summary>
		public static bool FeeUpdatePromptHelper(List<Procedure> listNewProcs,List<Procedure> listOldProcs,InsPlan insPlan,ref string promptText) {
			//No need to check RemotingRole; no call to db.
			//We do not want to update the fees when changing providers if the insplan is cat% and it has a fee sched set.
			if(insPlan!=null && insPlan.PlanType=="" && insPlan.FeeSched!=0) {
				return false;
			}
			switch(PrefC.GetInt(PrefName.ProcFeeUpdatePrompt)) {
				case 0:
					return false;
				case 1:
					//No prompt or check required. Equivilent to clicking "Yes" in some sense. Returns true.
					return true;
				case 2:
					List<ClaimProc> listClaimProcs=ClaimProcs.GetForProcs(listNewProcs.Select(x => x.ProcNum).ToList());
					List<Adjustment> listAdjustments=Adjustments.GetForProcs(listNewProcs.Select(x => x.ProcNum).ToList());
					foreach(Procedure proc in listNewProcs) {
						Procedure procOld=listOldProcs.FirstOrDefault(x => x.ProcNum==proc.ProcNum);
						if(procOld==null) {
							continue;
						}
						proc.ProcFee=Fees.GetAmount0(proc.CodeNum,Providers.GetProv(proc.ProvNum).FeeSched,proc.ClinicNum,proc.ProvNum);
						double procCurPatPortion=ClaimProcs.GetPatPortion(proc,listClaimProcs,listAdjustments);
						double procOldPatPortion=ClaimProcs.GetPatPortion(procOld,listClaimProcs,listAdjustments);
						if(procCurPatPortion!=procOldPatPortion) {
							promptText=Lans.g("FormProcEdit","The procedure's newly selected provider will change the fee.  Would you like to update the "
								+"procedure's fee to the newly selected provider's fee?");
							return true;
						}
					}
					return false;
				case 3:
					promptText=Lans.g("FormProcEdit","Would you like to update procedure fee amounts to the newly selected provider's fees?");
					return true;
			}
			return false;
		}

		///<summary>Gets a list of procedures representing extracted teeth.  Status of C,EC,orEO. Includes procs with toothNum "1"-"32".  Will not include procs with procdate before 1880.  Used for Canadian e-claims instead of the usual ToothInitials.GetMissingOrHiddenTeeth, because Canada requires dates on the extracted teeth.  Supply all procedures for the patient.</summary>
		public static List<Procedure> GetCanadianExtractedTeeth(List<Procedure> procList) {
			//No need to check RemotingRole; no call to db.
			List<Procedure> extracted=new List<Procedure>();
			ProcedureCode procCode;
			for(int i=0;i<procList.Count;i++) {
				if(procList[i].ProcStatus!=ProcStat.C && procList[i].ProcStatus!=ProcStat.EC && procList[i].ProcStatus!=ProcStat.EO) {
					continue;
				}
				if(!Tooth.IsValidDB(procList[i].ToothNum)) {
					continue;
				}
				if(Tooth.IsSuperNum(procList[i].ToothNum)) {
					continue;
				}
				if(Tooth.IsPrimary(procList[i].ToothNum)) {
					continue;
				}
				if(procList[i].ProcDate.Year<1880) {
					continue;
				}
				procCode=ProcedureCodes.GetProcCode(procList[i].CodeNum);
				if(procCode.TreatArea!=TreatmentArea.Tooth) {
					continue;
				}
				if(procCode.PaintType!=ToothPaintingType.Extraction) {
					continue;
				}
#if DEBUG //Needed for certification so that we can manually change the order that extrated teeth are sent, even throuh this won't matter in production.
				int j=0;
				while(j<extracted.Count) {
					if(extracted[j].DateTStamp>=procList[i].DateTStamp) {
						break;
					}
					j++;
				}
				extracted.Insert(j,procList[i].Copy());
#endif
			}
			return extracted;
		}

		///<summary>Takes the list of all procedures for the patient, and finds any that are attached as lab procs to that proc.</summary>
		public static List<Procedure> GetCanadianLabFees(long procNumLab,List<Procedure> procList){
			//No need to check RemotingRole; no call to db.
			List<Procedure> retVal=new List<Procedure>();
			if(procNumLab==0) {//Ignore regular procedures.
				return retVal;
			}
			for(int i=0;i<procList.Count;i++) {
				if(procList[i].ProcNumLab==procNumLab) {
					retVal.Add(procList[i]);
				}
			}
			return retVal;
		}

		///<summary>Pulls the lab fees for the given procnums directly from the database.</summary>
		public static List<Procedure> GetCanadianLabFees(List<long> listProcNums){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listProcNums);
			}
			if(listProcNums.Count==0) {
				return new List<Procedure>();
			}
			return Crud.ProcedureCrud.SelectMany("SELECT * FROM procedurelog WHERE ProcStatus<>"+POut.Int((int)ProcStat.D)+" AND ProcNumLab IN ("+string.Join(",",listProcNums)+")");
		}

		///<summary>Pulls the lab fees for the given procnum directly from the database.</summary>
		public static List<Procedure> GetCanadianLabFees(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),procNum);
			}
			if(procNum==0) {//By Total payment rows do not have labs.
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE ProcStatus<>"+POut.Int((int)ProcStat.D)+" AND ProcNumLab="+POut.Long(procNum);
			return Crud.ProcedureCrud.SelectMany(command);
		}
		
		///<summary>Uses similar logic to ComputeEstimates() to find old estimates which need to be recomputed.</summary>
		public static List<Procedure> GetProcsWithOldEstimates() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod());
			}
			//only claimprocs which are estimate or capestimate for all procedures which are not Canadian labs.
			string command=@"SELECT procedurelog.*
				FROM procedurelog
				INNER JOIN claimProc ON claimProc.ProcNum=procedurelog.ProcNum
					AND claimProc.PlanNum!=0
					AND claimProc.Status IN (6,8)
					AND (claimProc.InsSubNum,claimProc.PlanNum) NOT IN (SELECT patPlan.InsSubNum,inssub.PlanNum FROM patPlan INNER JOIN inssub ON inssub.InsSubNum=patPlan.InsSubNum WHERE patPlan.PatNum=claimProc.PatNum)
				WHERE procedurelog.ProcNumLab=0
				GROUP BY procedurelog.ProcNum";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets patients treatment planned procedures associated to future scheduled appointments including today.
		///Returns an empty list if listPatNum or listCodeNums is empty.</summary>
		public static List<Procedure> GetProcsAttachedToFutureAppt(List<long> listPatNums,List<long> listCodeNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listPatNums,listCodeNums);
			}
			if(listPatNums.Count==0 || listCodeNums.Count==0) {
				return new List<Procedure>();
			}
			string command="SELECT procedurelog.* "
				+"FROM procedurelog "
				+"INNER JOIN appointment ON appointment.AptNum=procedurelog.AptNum "
				+"WHERE appointment.PatNum IN ("+String.Join(",",listPatNums)+") "
				+"AND procedurelog.CodeNum IN ("+String.Join(",",listCodeNums)+") "
				+"AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP)+" "
				//All appts today or later
				+"AND "+DbHelper.DateTConditionColumn("appointment.AptDateTime",ConditionOperator.GreaterThanOrEqual,MiscData.GetNowDateTime())+" "
				+"AND appointment.AptStatus="+POut.Int((int)ApptStatus.Scheduled);
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Goes through the database looking for TP procedures that might need their proc fee updated.
		///Only updates the TP proc fee, does not compute estimates.  Returns number of fees changed.
		///Pass in a valid clinic num in order to only update TP proc fees associated to that particular clinic.</summary>
		public static long GlobalUpdateFees(List<Fee> listFees,long clinicNumGlobal=-1,string progressText="Updating...") {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),listFees,clinicNumGlobal,progressText);
			}
			if(_odThreadQueueData!=null) {
				throw new ApplicationException(Lans.g("FeeSchedTools","Global update fees tool is already running."));
			}
			FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(("Getting table of fees to update...")
				,progressBarEventType:ProgBarEventType.TextMsg)));
			#if DEBUG
			Stopwatch s=new Stopwatch();
			s.Start();
			#endif
			#region Create Queue Batch Data Thread
			_odThreadQueueData=new ODThread(QueueDataBatches,clinicNumGlobal);
			_odThreadQueueData.Name="GlobalUpdateFeesQueueDataThread";
			_odThreadQueueData.AddExceptionHandler(new ODThread.ExceptionDelegate((Exception ex) => { _isQueueDataThreadDone=true; }));
			_isQueueDataThreadDone=false;
			lock(_lockObjQueueBatchData) {
				_queueBatchData=new Queue<DataTable>();
			}
			_listProcNumMaxPerGroup=GetProcNumMaxForGroups(PROCNUM_BATCH_MAX_SIZE,new List<ProcStat>() { ProcStat.TP },clinicNumGlobal);
			if(_totCount==0 || _listProcNumMaxPerGroup.Count==0) {//not likely to happen, this would mean there are 0 TP procedures in the db, nothing to do
				_odThreadQueueData=null;
				return 0;
			}
			_odThreadQueueData.Start(true);
			#endregion Create Queue Batch Data Thread
			#region Get Medical Fee Sched Dict
			bool isMedFeeUsedForNewProcs=PrefC.GetBool(PrefName.MedicalFeeUsedForNewProcs);
			Dictionary<long,long> dictPatNumMedFeeSchedNum=new Dictionary<long,long>();
			if(isMedFeeUsedForNewProcs) {
				string command="SELECT patplan.PatNum,MAX(insplan.FeeSched) medFeeSched "
					+"FROM patplan "
					+"INNER JOIN ("
						+"SELECT patplan.PatNum,MIN(patplan.Ordinal) Ordinal "
						+"FROM patplan "
						+"INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
						+"INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum AND insplan.IsMedical "
						+"GROUP BY patplan.PatNum ";
				if(DataConnection.DBtype==DatabaseType.MySql) {
					command+="ORDER BY NULL";
				}
				command+=") medplan ON medplan.PatNum=patplan.PatNum AND medplan.Ordinal=patplan.Ordinal "
					+"INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
					+"INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum "
					+"GROUP BY patplan.PatNum ";
				if(DataConnection.DBtype==DatabaseType.MySql) {
					command+="ORDER BY NULL";
				}
				dictPatNumMedFeeSchedNum=Db.GetTable(command).Select()
					.ToDictionary(x => PIn.Long(x["PatNum"].ToString()),x => PIn.Long(x["medFeeSched"].ToString()));
			}
			#endregion Get Medical Fee Sched Dict
			#region Get Variables Used By All Batches
			string lanThis="FormFeeSchedTools";
			int rowSkippedCount=0;//used to update progress bar
			int procFeesUpdatedCount=0;//used to report number of fees updated to calling form
			bool isInsPpoAlwaysUseUcrFee=PrefC.GetBool(PrefName.InsPpoAlwaysUseUcrFee);
			FeeCache feeCache=new FeeCache(listFees);
			long practDefaultProvNum=PrefC.GetLong(PrefName.PracticeDefaultProv);
			long practDefaultProvFeeSched=Providers.GetFirstOrDefault(x => x.ProvNum==practDefaultProvNum)?.FeeSched??0;//default to 0 if prov is not found
			long firstNonHiddenProvFeeSched=Providers.GetFirstOrDefault(x => !x.IsHidden)?.FeeSched??0;//default to 0 if all provs hidden (not likely to happen)
			Dictionary<long,long> dictProvFeeSched=Providers.GetDeepCopy().ToDictionary(x => x.ProvNum,x => x.FeeSched);
			//dictionary of fee key linked to a list of lists of longs in order to keep each update statement limited to updating 1000 procedures per query
			Dictionary<double,List<List<long>>> dictFeeListCodes=new Dictionary<double,List<List<long>>>();
			DataTable table=new DataTable();
			long batchNumber=0;
			#endregion Get Variables Used By All Batches
			List<long> listProcNums=new List<long>();
			try {
				while(!_isQueueDataThreadDone || _queueBatchData.Count>0) {//if batch thread is done and queue is empty, loop is finished
					if(_queueBatchData.Count==0) {
						//queueBatchThread must not be finished gathering batches but the queue is empty, give the batch thread time to catch up
						continue;
					}
					try {
						lock(_lockObjQueueBatchData) {
							table=_queueBatchData.Dequeue();
							#if DEBUG
							Console.WriteLine("Main thread, dequeue batch, queue count: "+_queueBatchData.Count);
							#endif
						}
					}
					catch(Exception ex) {//queue must be empty even though we just checked it before entering the while loop, just loop again and wait if necessary
						ex.DoNothing();
						continue;
					}
					batchNumber++;
					double currentRowCount=0; //keeps track of both rows skipped and unskipped
					foreach(DataRow rowCur in table.Rows) {
						#region Get Variables from DataRow
						long codeNum=PIn.Long(rowCur["CodeNum"].ToString());
						long clinicNum=PIn.Long(rowCur["ClinicNum"].ToString());
						long procProvNum=PIn.Long(rowCur["ProvNum"].ToString());
						long patPriProv=PIn.Long(rowCur["PriProv"].ToString());
						long patFeeSched=PIn.Long(rowCur["patFeeSched"].ToString());
						long medCodeNum=PIn.Long(rowCur["medCodeNum"].ToString());
						long procNum=PIn.Long(rowCur["ProcNum"].ToString());
						long procNumLab=PIn.Long(rowCur["ProcNumLab"].ToString());
						double procFeeCur=PIn.Double(rowCur["ProcFee"].ToString());
						long patNum=PIn.Long(rowCur["PatNum"].ToString());
						long patPriPlanFeeSchedNum=PIn.Long(rowCur["planFeeSched"].ToString());
						double percentComplete=Math.Ceiling(((double)currentRowCount/table.Rows.Count)*100);
						long feeSchedCur=0;
						double newFee;
						if(CultureInfo.CurrentCulture.Name.EndsWith("CA")
							&& procNumLab!=0) 
						{
							currentRowCount++;
							FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(progressText,(int)percentComplete+"%"
								,(int)percentComplete,100,ProgBarStyle.Blocks,"Clinic"
								,labelTop: Lans.g(lanThis,"Batch")+" "+batchNumber+"/"+_listProcNumMaxPerGroup.Count)));
							continue;//The proc fee for a lab is derived from the lab fee on the parent procedure.
						}
						#endregion Get Variables from DataRow
						#region Med Fee Used and Proc Has Med CodeNum
						if(isMedFeeUsedForNewProcs && medCodeNum>0) {
							if(dictPatNumMedFeeSchedNum.TryGetValue(patNum,out feeSchedCur)) {//use med plan fee sched first
								if(feeSchedCur==0 && patFeeSched>0) {//if med plan fee sched is 0, use pat fee sched second
									feeSchedCur=patFeeSched;
								}
								if(feeSchedCur==0 && patPriProv>0) {//if no pat fee sched, use pat pri prov fee sched third
									dictProvFeeSched.TryGetValue(patPriProv,out feeSchedCur);
								}
								if(feeSchedCur==0) {//if no pat pri prov fee sched, use first non-hidden prov fee sched last
									feeSchedCur=firstNonHiddenProvFeeSched;
								}
							}
						}
						#endregion Med Fee Used and Proc Has Med CodeNum
						#region Dental Fee Sched
						if(feeSchedCur==0) {//not using med plan fees or no med plan for pat or no med codeNum for proc, basically no fee sched found yet
							feeSchedCur=patPriPlanFeeSchedNum;//use pri plan fee sched first
						}
						if(feeSchedCur==0 && patFeeSched>0) {//no pri plan fee sched, use pat fee sched second
							feeSchedCur=patFeeSched;
						}
						if(feeSchedCur==0 && procProvNum>0) {//no pat fee sched, use proc prov fee sched third
							dictProvFeeSched.TryGetValue(procProvNum,out feeSchedCur);
						}
						if(feeSchedCur==0 && patPriProv>0) {//no proc prov fee sched, use pat pri prov fee sched last
							dictProvFeeSched.TryGetValue(patPriProv,out feeSchedCur);
						}
						#endregion Dental Fee Sched
						if(isMedFeeUsedForNewProcs && medCodeNum>0) {
							newFee=feeCache.GetAmount0(medCodeNum,feeSchedCur,clinicNum,procProvNum);
						}
						else {
							newFee=feeCache.GetAmount0(codeNum,feeSchedCur,clinicNum,procProvNum);
						}
						#region PPO Plan, Might Use UCR Fee
						if(rowCur["PlanType"].ToString()=="p") {//PPO plan, might use UCR fee
							feeSchedCur=0;
							if(patPriProv>0) {//use pat pri prov fee sched first
								dictProvFeeSched.TryGetValue(patPriProv,out feeSchedCur);
							}
							if(feeSchedCur==0 && practDefaultProvFeeSched>0) {//no pat pri prov fee sched, use practice default prov fee sched second
								feeSchedCur=practDefaultProvFeeSched;
							}
							if(feeSchedCur==0) {//no practice default prov fee sched, use first non-hidden prov fee sched last
								feeSchedCur=firstNonHiddenProvFeeSched;
							}
							double ucrFee=feeCache.GetAmount0(codeNum,feeSchedCur,clinicNum,procProvNum);
							if(newFee<ucrFee || isInsPpoAlwaysUseUcrFee) {
								newFee=ucrFee;
							}
						}
						#endregion PPO Plan, Might Use UCR Fee
						if(newFee.IsEqual(procFeeCur)) {
							rowSkippedCount++;
							currentRowCount++;
							FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(progressText,(int)percentComplete+"%"
								,(int)percentComplete,100,ProgBarStyle.Blocks,"Clinic"
								,labelTop:Lans.g(lanThis,"Batch")+" "+batchNumber+"/"+_listProcNumMaxPerGroup.Count)));
							continue;
						}
						newFee=PIn.Double(POut.Double(newFee));//using POut.Double in order to perform the culture specific rounding before adding to dictionary
						if(!dictFeeListCodes.ContainsKey(newFee)) {
							dictFeeListCodes[newFee]=new List<List<long>>() { new List<long>(UPDATE_PROCNUM_IN_MAX_SIZE) };
						}
						if(dictFeeListCodes[newFee].Last().Count>=UPDATE_PROCNUM_IN_MAX_SIZE) {
							dictFeeListCodes[newFee].Add(new List<long>(UPDATE_PROCNUM_IN_MAX_SIZE));
						}
						dictFeeListCodes[newFee].Last().Add(procNum);
						currentRowCount++;
						//update batch label for progress bar
						FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(progressText,(int)percentComplete+"%"
							,(int)percentComplete,100,ProgBarStyle.Blocks,"Clinic",
							labelTop:Lans.g(lanThis,"Batch")+" "+batchNumber+"/"+_listProcNumMaxPerGroup.Count)));
					}//end of foreach loop. Done with one batch of procedures.
					FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(
						Lans.g(lanThis,"Batch")+" "+batchNumber+"/"+_listProcNumMaxPerGroup.Count+" fee process completed"
						,progressBarEventType:ProgBarEventType.TextMsg)));
				}//end of while loop. Done with all batches of procedures.
				FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(progressText,100+"%"
						,100,100,ProgBarStyle.Blocks,"Clinic",labelTop:Lans.g(lanThis,"Batch")+" "+batchNumber+"/"+_listProcNumMaxPerGroup.Count)));
				FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(
					Lans.g(lanThis,"Procedure fees processed")+" "+progressText+" "+_totCount+"/"+_totCount
					,progressBarEventType:ProgBarEventType.TextMsg)));
				if(dictFeeListCodes.Count==0) {
					return 0;//no procedure fees updated, all skipped
				}
				FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(
					Lans.g(lanThis,"Updating fees..."),progressBarEventType:ProgBarEventType.TextMsg)));
				#region Create List of Actions			
				List<Action> listActions=dictFeeListCodes.SelectMany(x => x.Value.Select(y => new Action(() => {
					string command="UPDATE procedurelog SET ProcFee="+x.Key+" WHERE ProcNum IN ("+string.Join(",",y)+")";
					#if DEBUG
					Stopwatch s1=new Stopwatch();
					s1.Start();
					#endif
					Db.NonQ(command);
					#if DEBUG
					s1.Stop();
					Console.WriteLine("Updated "+y.Count+" procedures, runtime: "+s1.Elapsed.TotalSeconds+" sec");
					#endif
					listProcNums.AddRange(y);
					procFeesUpdatedCount+=y.Count;
				}))).ToList();
				#endregion Create List of Actions
				ODThread.RunParallel(listActions,TimeSpan.FromMinutes(30)
					,onException:new ODThread.ExceptionDelegate((ex) => {
						//Notify the user what went wrong via the text box.
						FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper("Error updating ProcFee: "+ex.Message
							,progressBarEventType:ProgBarEventType.TextMsg)));
				}));//each group of actions gets X minutes.
				FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper(Lans.g("FeeSchedTools","Fees Updated Successfully"),
						progressBarEventType:ProgBarEventType.TextMsg)));
			}//end of try
			catch(Exception ex) {
				ex.DoNothing();
			}
			finally {
				_odThreadQueueData?.QuitAsync();
				_odThreadQueueData=null;
			}
			#if DEBUG
			s.Stop();
			Console.WriteLine("Runtime: "+s.Elapsed.Minutes+" min "+(s.Elapsed.TotalSeconds-(s.Elapsed.Minutes*60))+" sec");
			#endif
			return procFeesUpdatedCount;
		}

		///<summary>Thread that gets batches of data to put into a queue for another thread to process.</summary>
		private static void QueueDataBatches(ODThread odThread) {
			//No need to check RemotingRole; private method.
			#if DEBUG
			Stopwatch s=new Stopwatch();
			s.Start();
			#endif
			try {
				bool isMedFeeUsedForNewProcs=PrefC.GetBool(PrefName.MedicalFeeUsedForNewProcs);
				long clinicNumGlobal=(long)odThread.Parameters[0];
				List<string> listQueries=new List<string>();
				for(int i=0;i<_listProcNumMaxPerGroup.Count;i++) {
					#region Get ProcNum Range and ClinicNum Where Clauses
					List<string> listWhereAnds=new List<string>();
					if(i>0) {
						listWhereAnds.Add("procedurelog.ProcNum>"+_listProcNumMaxPerGroup[i-1]);
					}
					if(i<_listProcNumMaxPerGroup.Count-1) {
						listWhereAnds.Add("procedurelog.ProcNum<="+_listProcNumMaxPerGroup[i]);
					}
					if(clinicNumGlobal>-1) { //only add clinic restriction if a clinic was passed in. Defaults to -1.
						listWhereAnds.Add("procedurelog.ClinicNum="+clinicNumGlobal);
					}
					#endregion Get ProcNum Range and ClinicNum Where Clauses
					#region Get Query String
					//Get all TP procedures in this batch's ProcNum range.
					string command="SELECT procedurelog.PatNum,procedurelog.ProcNum,procedurelog.CodeNum,procedurelog.ClinicNum,procedurelog.ProvNum,"
						+"procedurelog.ProcFee,patient.PriProv,patient.FeeSched patFeeSched,procedurelog.ProcNumLab,"
						+(isMedFeeUsedForNewProcs?"COALESCE(procedurecode.CodeNum,0) medCodeNum,":"0 medCodeNum,")
						+"COALESCE(MAX(CASE WHEN p.PatNum IS NOT NULL THEN insplan.PlanType ELSE '' END),'') PlanType,"
						+"COALESCE(MAX(CASE WHEN patplan.Ordinal=1 THEN insplan.FeeSched ELSE 0 END),0) planFeeSched "
						+"FROM procedurelog "
						+"INNER JOIN patient ON patient.PatNum=procedurelog.PatNum "
						+"LEFT JOIN ("
							+"SELECT patplan.PatNum,MIN(patplan.Ordinal) minOrdinal "
							+"FROM patplan "
							+"INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
							+"INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum AND !insplan.IsMedical "
							+"GROUP BY patplan.PatNum"
						+") p ON patient.PatNum=p.PatNum "
						+"LEFT JOIN patplan ON patplan.PatNum=patient.PatNum AND (patplan.Ordinal=p.MinOrdinal OR patplan.Ordinal=1) "
						+"LEFT JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
						+"LEFT JOIN insplan ON insplan.PlanNum=inssub.PlanNum "
						+(isMedFeeUsedForNewProcs?"LEFT JOIN procedurecode ON procedurecode.ProcCode=procedurelog.MedicalCode ":"")
						+"WHERE procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP)+" "
						+(listWhereAnds.Count>0?("AND "+string.Join(" AND ",listWhereAnds)+" "):"");
					if(DataConnection.DBtype==DatabaseType.MySql) {
						//because sometimes a pat can have more than one primary patplan and DBM only has "manual fix needed" for this problem
						command+="GROUP BY procedurelog.ProcNum "
							+"ORDER BY NULL";
					}
					else {//oracle
						//because sometimes a pat can have more than one primary patplan and DBM only has "manual fix needed" for this problem
						command+="GROUP BY procedurelog.PatNum,procedurelog.ProcNum,procedurelog.CodeNum,procedurelog.ClinicNum,procedurelog.ProvNum,"
							+"procedurelog.ProcFee,patient.PriProv,patient.FeeSched";
					}
					listQueries.Add(command);
					#endregion Get Query String
				}
				#region Create List of Actions
				List<Action> listActions=listQueries.Select(x => new Action(() => {
					#if DEBUG
					Stopwatch s1=new Stopwatch();
					s1.Start();
					#endif
					DataTable table=Db.GetTable(x);
					#if DEBUG
					s1.Stop();
					#endif
					if(table.Rows.Count>0) {
						while(_queueBatchData.Count>5) {
							//wait until queue is at reasonable size before queueing more. We don't want to hold more than 5 datatables in memory at once. 
							Thread.Sleep(1);
						}
						lock(_lockObjQueueBatchData) {
							_queueBatchData.Enqueue(table);
							#if DEBUG
							Console.WriteLine(odThread.Name+" - enqueue batch, queue count: "+_queueBatchData.Count+", runtime: "+s1.Elapsed.TotalSeconds+" sec");
							#endif
						}
					}
				})).ToList();
				#endregion Create List of Actions
				ODThread.RunParallel(listActions,TimeSpan.FromMinutes(30),onException:new ODThread.ExceptionDelegate((ex) => {
						//Notify the user what went wrong via the text box.
						FeeSchedEvent.Fire(new ODEventArgs("FeeSchedEvent",new ProgressBarHelper("Error getting TP procedures batch: "+ex.Message
							,progressBarEventType:ProgBarEventType.TextMsg)));
				}));
			}
			catch(Exception ex) {
				ex.DoNothing();//if error happens, just swallow the error and kill the thread
			}
			finally {//always make sure to notify the main thread that the thread is done so the main thread doesn't wait for eternity
				_isQueueDataThreadDone=true;
				#if DEBUG
				s.Stop();
				Console.WriteLine(odThread.Name+" - Done, enqueue total count: "+_listProcNumMaxPerGroup.Count
					+", thread runtime: "+s.Elapsed.Minutes+" min "+(s.Elapsed.TotalSeconds-(s.Elapsed.Minutes*60))+" sec");
				#endif
			}
		}
		
		///<summary>Returns list of ProcNums such that each ProcNum is the max ProcNum in it's group of numPerGroup ProcNums.
		///Example: If there are 1000 procedures in the db with sequential ProcNums and each ProcStatus is in the list of ProcStatuses and the numPerGroup
		///is 500, the returned list would have 2 values in it, 500 and 1000. Each number is the max ProcNum such that if you selected the procedures with
		///ProcNum greater than the previous entry (or greater than 0 if it is the first entry) and less than or equal to the current entry you would get
		///at most numPerGroup procedures (the last group could, of course, have fewer in it).</summary>
		public static List<long> GetProcNumMaxForGroups(int numPerGroup,List<ProcStat> listProcStatuses,long clinicNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<long>>(MethodBase.GetCurrentMethod(),numPerGroup,listProcStatuses,clinicNum);
			}
			_totCount=0;
			List<long> retval=new List<long>();
			if(numPerGroup<1) {
				return retval;
			}
			List<string> listWhereClauses=new List<string>();
			if(listProcStatuses!=null && listProcStatuses.Count>0) {
				listWhereClauses.Add("ProcStatus IN("+string.Join(",",listProcStatuses.Select(x => POut.Int((int)x)))+")");
			}
			if(clinicNum>-1) {
				listWhereClauses.Add("ClinicNum="+POut.Long(clinicNum));
			}
			string whereClause="";
			if(listWhereClauses.Count>0) {
				whereClause="WHERE "+string.Join(" AND ",listWhereClauses)+" ";
			}
			if(DataConnection.DBtype==DatabaseType.MySql) {
				string command="SET @row=0,@maxProcNum=0;"
					+"SELECT procNum,@row totalCount FROM ("
						+"SELECT @row:=@row+1 rowNum,@maxProcNum:=ProcNum procNum FROM ("
							+"SELECT ProcNum FROM procedurelog "+whereClause+"ORDER BY ProcNum"
						+") a"
					+") b "
					+"WHERE procNum=@maxProcNum OR rowNum%"+numPerGroup+"=0";
				DataTable tableCur=Db.GetTable(command);
				if(tableCur.Rows.Count>0) {
					_totCount=PIn.Int(tableCur.Rows[0]["totalCount"].ToString());
					retval=tableCur.Select().Select(x => PIn.Long(x["procNum"].ToString())).ToList();
				}
			}
			else {//oracle
				//different strategy for oracle, but not used for MySQL because it's much slower
				long groupMaxPatNum;
				int groupNum=0;
				do {
					groupMaxPatNum=0;
					string command="SELECT MAX(ProcNum) procNumMax,COUNT(*) groupCount FROM ("
						+DbHelper.LimitOrderByOffset("SELECT ProcNum FROM procedurelog "+whereClause+"ORDER BY ProcNum",numPerGroup,groupNum)+") patNumGroup";
					DataTable tableCur=Db.GetTable(command);//increase timeout to 5 minutes for this query
					if(tableCur.Rows.Count==0) {
						break;
					}
					groupMaxPatNum=PIn.Long(tableCur.Rows[0]["procNumMax"].ToString());
					if(groupMaxPatNum>0) {
						retval.Add(groupMaxPatNum);
					}
					_totCount+=PIn.Int(tableCur.Rows[0]["groupCount"].ToString());
					groupNum+=numPerGroup;
				} while(groupMaxPatNum>0);
			}
			return retval;
		}

		///<summary>Used from TP to get a list of all TP procs, ordered by their treatment plan's priority, (conditionally) toothnum.</summary>
		public static Procedure[] GetListTPandTPi(List<Procedure> procList,List<TreatPlanAttach> listTreatPlanAttaches=null) {
			//No need to check RemotingRole; no call to db.
			return SortListByTreatPlanPriority(procList.FindAll(x => x.ProcStatus==ProcStat.TP || x.ProcStatus==ProcStat.TPi),listTreatPlanAttaches);
		}
		
		///<summary>Sorts the given list based on the procedure's priority, tooth, date, and procnum.
		///SortListByTreatPlanPriority() should be the only method of sorting procedures that need to emulate the treatment plan module.
		///This is to prevent recurring bugs due to different sort methodology.</summary>
		public static Procedure[] SortListByTreatPlanPriority(List<Procedure> listProcs,List<TreatPlanAttach> listTreatPlanAttaches=null) {
			//No need to check RemotingRole; no call to db.
			Dictionary<long,int> dictPriorities=new Dictionary<long, int>();
			Dictionary<long,int> dictProcNumPriority=new Dictionary<long, int>();
			bool hasTreatPlanAttaches=false;
			//Check to see if a list of TreatPlanAttaches was passed in.  If so, use the treatplanattach priorities instead of the priority of the procedure that's in the database.
			//This is used for multiple active treatment plans where we need to display a sorted priority even though that procedure might not have the same priority in another
			//saved treatment plan and thus would have a different priority in the database.
			if(listTreatPlanAttaches!=null) {
				hasTreatPlanAttaches=true;
				dictPriorities=Defs.GetDefsForCategory(DefCat.TxPriorities).ToDictionary(x=>x.DefNum,x=>x.ItemOrder);
				listTreatPlanAttaches.ForEach(x => dictProcNumPriority[x.ProcNum]=(x.Priority==0 ? -1 : dictPriorities[x.Priority]));
			}
			List<Procedure> listLabProcs=listProcs.Where(x => x.ProcNumLab!=0).Select(x => x.Copy()).ToList();//Canadian Lab Procs
			List<long> listLabProcNums=listLabProcs.Select(x => x.ProcNum).ToList();
			listProcs.RemoveAll(x => listLabProcNums.Contains(x.ProcNum));//Remove all labs from this list if any.  Labs are always below their parent proc.
			//Procedure code is purposefully not included in the sorting of this list.  It will ruin PrefName.TreatPlanSortByTooth sorting.
			List<Procedure> listOrderedProcs=listProcs
				.OrderBy(x => (hasTreatPlanAttaches ? dictProcNumPriority[x.ProcNum] : x.PriorityOrder)<0)
				.ThenBy(x => (hasTreatPlanAttaches ? dictProcNumPriority[x.ProcNum] : x.PriorityOrder))
				//.ThenBy(x => PrefC.IsTreatPlanSortByTooth ? x.ToothRange : 0)
				.ThenBy(x => PrefC.IsTreatPlanSortByTooth ? Tooth.ToInt(x.ToothNum) : 0)//Sorting by a constant causes nothing to happen.
				.ThenBy(x => x.ProcDate)
				.ThenBy(x => x.ProcNum)//This is necessary in case isTreatPlanSortByTooth is false, and to break any ties between the other sorting methods.
				.ToList();
			List<long> listParentProcNums=listLabProcs.Select(x => x.ProcNumLab).Distinct().ToList();//There can be up to 2 lab procs for each parent proc.
			for(int i=listOrderedProcs.Count-1;i>=0;i--) {//Loop backward so we can insert as we go without affecting the index.
				if(!listParentProcNums.Contains(listOrderedProcs[i].ProcNum)) {
					continue;//Not a parent proc.
				}
				listOrderedProcs.InsertRange(i+1,listLabProcs.Where(x => x.ProcNumLab==listOrderedProcs[i].ProcNum).ToList());//Insert labs below parent proc.
			}
			return listOrderedProcs.ToArray();
		}

		///<summary>Checks for frequency conflicts with the passed-in list of procedures.
		///Returns empty string if there are no conflicts, new line delimited list of proc codes if there are.  Throws exceptions.</summary>
		public static string CheckFrequency(List<Procedure> procList,long patNum,DateTime aptDateTime) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetString(MethodBase.GetCurrentMethod(),procList,patNum,aptDateTime);
			}
			if(procList==null) {
				throw new ArgumentException("Invalid procedure list passed in.","procList");
			}
			Patient pat=Patients.GetPat(patNum);
			if(pat==null) {
				throw new ArgumentException("Patient not found in database.","patNum");
			}
			if(aptDateTime==null) {
				throw new ArgumentException("Appointment Date not present.","aptDateTime");
			}
			List<Procedure> procListNew=procList.Select(x => x.Copy()).ToList();//Because we're modifying the procedures in this method
			string frequencyConflicts="";
			List<PatPlan> listPatPlans=PatPlans.GetPatPlansForPat(patNum);
			if(!PatPlans.IsPatPlanListValid(listPatPlans)) {
				//need to validate due to call to GetHistList below
				listPatPlans=PatPlans.Refresh(patNum);
			}
			if(listPatPlans.Count<1) {
				return "";
			}
			List<InsSub> listInsSubs=InsSubs.GetMany(listPatPlans.Select(x => x.InsSubNum).ToList());
			List<InsPlan> listInsPlans=InsPlans.GetByInsSubs(listInsSubs.Select(x => x.InsSubNum).ToList());
			List<Benefit> listBenefits=Benefits.Refresh(listPatPlans,listInsSubs);
			listBenefits.AddRange(Benefits.GetForPatPlansAndProcs(listPatPlans.Select(x => x.PatPlanNum).ToList(),procListNew.Select(x => x.CodeNum).ToList()));
			List<ClaimProcHist> listClaimProcsHist=ClaimProcs.GetHistList(patNum,listBenefits,listPatPlans,listInsPlans,DateTime.Now,listInsSubs);
			List<ClaimProc> listClaimProcsForProcs=ClaimProcs.GetForProcs(procListNew.Select(x => x.ProcNum).ToList());
			if(aptDateTime!=DateTime.MinValue) {
				foreach(Procedure proc in procListNew) {
					proc.ProcDate=aptDateTime;
				}
				foreach(ClaimProc claimProc in listClaimProcsForProcs) {
					claimProc.ProcDate=aptDateTime;
				}
			}
			foreach(Procedure proc in procListNew) {
				ComputeEstimates(proc,pat.PatNum,ref listClaimProcsForProcs,false,listInsPlans,listPatPlans,listBenefits,listClaimProcsHist,null,false,pat.Age
					,listInsSubs);
				ClaimProc claimProc=listClaimProcsForProcs.Find(x => x.ProcNum==proc.ProcNum);
				if(claimProc!=null && !string.IsNullOrEmpty(claimProc.EstimateNote) && claimProc.EstimateNote.Contains("Frequency Limitation")) {
					if(frequencyConflicts!="") {
						frequencyConflicts+="\r\n";
					}
					frequencyConflicts+=ProcedureCodes.GetStringProcCode(proc.CodeNum);
				}
			}
			return frequencyConflicts;
		}

		public static void ComputeEstimates(Procedure proc,long patNum,List<ClaimProc> claimProcs,bool isInitialEntry,List<InsPlan> planList,
			List<PatPlan> patPlans,List<Benefit> benefitList,int patientAge,List<InsSub> subList) 
		{
			//No need to check RemotingRole; no call to db.
			ComputeEstimates(proc,patNum,ref claimProcs,isInitialEntry,planList,patPlans,benefitList,null,null,true,patientAge,subList);
		}
		
		///<summary>Used whenever a procedure changes or a plan changes.  All estimates for a given procedure must be updated. This frequently includes 
		///adding claimprocs, but can also just edit the appropriate existing claimprocs. Skips status=Adjustment,CapClaim,Preauth,Supplemental.  
		///Also fixes date,status,and provnum if appropriate.  The claimProc list only needs to include claimprocs for this proc, although it can 
		///include more.  Only set isInitialEntry true from Chart module; it is for cap procs.  loopList only contains information about procedures that 
		///come before this one in a list such as TP or claim.</summary>
		///<param name="listClaimProcsAll">This list holds all claim procs, even ones that are received. The purpose of this list is to hold all
		///claim procs for reference. This list will not be modified.</param>
		public static void ComputeEstimates(Procedure proc,long patNum,ref List<ClaimProc> claimProcs,bool isInitialEntry,List<InsPlan> planList,
			List<PatPlan> patPlans,List<Benefit> benefitList,List<ClaimProcHist> histList,List<ClaimProcHist> loopList,bool saveToDb,int patientAge,
			List<InsSub> subList,List<ClaimProc> listClaimProcsAll=null,bool isClaimProcRemoveNeeded=false,
			bool useProcDateOnProc=false,List<SubstitutionLink> listSubLinks=null,bool isForOrtho=false) 
		{
			//No need to check RemotingRole; no call to db.
			listClaimProcsAll=listClaimProcsAll??claimProcs;
			bool isHistorical=false;
			if(proc.ProcDate<DateTime.Today && proc.ProcStatus==ProcStat.C) {
				isHistorical=true;//Don't automatically create an estimate for completed procedures, especially if they are older than today.  Very important after a conversion from another software.
				//Special logic in place only for capitation plans:
				if(planList.Any(x => x.PlanType=="c") //11/19/2012 js We had a specific complaint where changing plan type to capitation automatically added WOs to historical procs.
					&& !listClaimProcsAll.Any(x => x.ProcNum==proc.ProcNum 
						&& x.Status.In(ClaimProcStatus.CapClaim,ClaimProcStatus.CapComplete,ClaimProcStatus.CapEstimate))) 
				{
					//If there are any capitation plans but no capitation claimproc.statuses then return.
					//04/02/2013 Jason- To relax this filter for offices that enter treatment a few days after it's done, we will see if any capitation statuses exist.
					return;//There are no capitation claimprocs for this procedure, therefore we don't want to touch/damage this proc.
				}
			}
			//first test to see if each estimate matches an existing patPlan (current coverage),
			//delete any other estimates
			List<long> listDeletedClaimProcNums=new List<long>();
			for(int i=0;i<claimProcs.Count;i++) {
				if(claimProcs[i].ProcNum!=proc.ProcNum) {
					continue;
				}
				if(claimProcs[i].PlanNum==0) {
					continue;
				}
				if(claimProcs[i].Status==ClaimProcStatus.CapClaim
					||claimProcs[i].Status==ClaimProcStatus.Preauth
					||claimProcs[i].Status==ClaimProcStatus.Supplemental) {
					continue;
					//ignored: adjustment
					//included: capComplete,CapEstimate,Estimate,NotReceived,Received
				}
				if(claimProcs[i].Status!=ClaimProcStatus.Estimate && claimProcs[i].Status!=ClaimProcStatus.CapEstimate) {
					continue;
				}
				bool planIsCurrent=false;
				for(int p=0;p<patPlans.Count;p++) {
					if(patPlans[p].InsSubNum==claimProcs[i].InsSubNum
						&& InsSubs.GetSub(patPlans[p].InsSubNum,subList).PlanNum==claimProcs[i].PlanNum) 
					{
						planIsCurrent=true;
						break;
					}
				}
				//If claimProc estimate is for a plan that is not current, delete it
				if(!planIsCurrent) {
					if(saveToDb) {
						ClaimProcs.Delete(claimProcs[i]);
					}
					else {
						claimProcs[i].DoDelete=true;
					}
					listDeletedClaimProcNums.Add(claimProcs[i].ClaimProcNum);
				}
			}
			if(isClaimProcRemoveNeeded) {
				//Remove all claimProcs which were deleted.
				//This prevents Canadian lab procedures from generating claimProcs for deleted parent claimProcs. 
				claimProcs.RemoveAll(x => listDeletedClaimProcNums.Contains(x.ClaimProcNum));
			}
			InsPlan planCur;
			InsSub subCur;
			//bool estExists;
			bool cpAdded=false;
			//loop through all patPlans (current coverage), and add any missing estimates
			for(int p=0;p<patPlans.Count;p++) {//typically, loop will only have length of 1 or 2
				//Don't automatically create an estimate for completed procedures, especially if they are older than today.
				//However, we have an optional preference for users that knowingly accept this danger and have a workflow that requires this.
				if(isHistorical && !(PrefC.GetBool(PrefName.ClaimProcsAllowedToBackdate) || isForOrtho)) {
						break;
				}
				PatPlan patPlanCur=patPlans[p];
				//test to see if estimate exists
				if(listClaimProcsAll.Any(x => x.ProcNum==proc.ProcNum && x.PlanNum!=0 && x.InsSubNum==patPlanCur.InsSubNum
					&& !new[] {ClaimProcStatus.CapClaim,ClaimProcStatus.Preauth,ClaimProcStatus.Supplemental }.Contains(x.Status)))
				{
					continue;//estimate exists
				}
				//estimate is missing, so add it.
				subCur=InsSubs.GetSub(patPlanCur.InsSubNum,subList);
				planCur=InsPlans.GetPlan(subCur.PlanNum,planList);
				if(planCur==null){//subCur can never be null) {//??
					continue;//??
				}
				ClaimProc cp=new ClaimProc();
				cp.ProcNum=proc.ProcNum;
				cp.PatNum=patNum;
				cp.ProvNum=proc.ProvNum;
				if(planCur.PlanType=="c") {
					if(proc.ProcStatus==ProcStat.C) {
						cp.Status=ClaimProcStatus.CapComplete;
					}
					else {
						cp.Status=ClaimProcStatus.CapEstimate;//this may be changed below
					}
				}
				else {
					cp.Status=ClaimProcStatus.Estimate;
				}
				cp.PlanNum=planCur.PlanNum;
				cp.InsSubNum=subCur.InsSubNum;
				cp.DateCP=proc.ProcDate;
				cp.AllowedOverride=-1;
				cp.PercentOverride=-1;
				cp.NoBillIns=ProcedureCodes.GetProcCode(proc.CodeNum).NoBillIns;
				cp.PaidOtherIns=-1;
				cp.CopayOverride=-1;
				cp.ProcDate=proc.ProcDate;
				cp.BaseEst=0;
				cp.InsEstTotal=0;
				cp.InsEstTotalOverride=-1;
				cp.DedEst=-1;
				cp.DedEstOverride=-1;
				cp.PaidOtherInsOverride=-1;
				cp.WriteOffEst=-1;
				cp.WriteOffEstOverride=-1;
				//ComputeBaseEst will fill AllowedOverride,Percentage,CopayAmt,BaseEst
				if(saveToDb) {
					ClaimProcs.Insert(cp);
				}
				else {
					claimProcs.Add(cp);//this newly added cp has no ClaimProcNum and is not yet in the db.
				}
				cpAdded=true;
			}
			//if any were added, refresh the list
			if(cpAdded && saveToDb) {//no need to refresh the list if !saveToDb, because list already made current.
				claimProcs=ClaimProcs.Refresh(patNum);
			}
			double paidOtherInsEstTotal=0;
			double paidOtherInsBaseEst=0;
			double writeOffEstOtherIns=0;
			listSubLinks=listSubLinks??SubstitutionLinks.GetAllForPlans(planList);
			//because secondary claimproc might come before primary claimproc in the list, we cannot simply loop through the claimprocs
			ComputeForOrdinal(1,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge,subList,listSubLinks,useProcDateOnProc);
			ComputeForOrdinal(2,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge,subList,listSubLinks,useProcDateOnProc);
			ComputeForOrdinal(3,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge,subList,listSubLinks,useProcDateOnProc);
			ComputeForOrdinal(4,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge,subList,listSubLinks,useProcDateOnProc);
			//At this point, for a PPO with secondary, the sum of all estimates plus primary writeoff might be greater than fee.
			if(patPlans.Count>1){
				subCur=InsSubs.GetSub(patPlans[0].InsSubNum,subList);
				planCur=InsPlans.GetPlan(subCur.PlanNum,planList);
				if(planCur.PlanType=="p") {
					//claimProcs=ClaimProcs.Refresh(patNum);
					//ClaimProc priClaimProc=null;
					int priClaimProcIdx=-1;
					double sumPay=0;//Either actual or estimate
					for(int i=0;i<claimProcs.Count;i++){
						if(claimProcs[i].ProcNum!=proc.ProcNum){
							continue;
						}
						if(claimProcs[i].Status==ClaimProcStatus.Adjustment
							|| claimProcs[i].Status==ClaimProcStatus.CapClaim
							|| claimProcs[i].Status==ClaimProcStatus.CapComplete
							|| claimProcs[i].Status==ClaimProcStatus.CapEstimate
							|| claimProcs[i].Status==ClaimProcStatus.Preauth)
						{
							continue;
						}
						if(claimProcs[i].PlanNum==planCur.PlanNum && claimProcs[i].WriteOffEst>0){
							priClaimProcIdx=i;
						}
						if(claimProcs[i].Status==ClaimProcStatus.Received
							|| claimProcs[i].Status==ClaimProcStatus.Supplemental ){
							sumPay+=claimProcs[i].InsPayAmt;
						}
						if(claimProcs[i].Status==ClaimProcStatus.Estimate){
							if(!claimProcs[i].InsEstTotalOverride.IsEqual(-1)){
								sumPay+=claimProcs[i].InsEstTotalOverride;
							}
							else{
								sumPay+=claimProcs[i].InsEstTotal;
							}
						}
						if(claimProcs[i].Status==ClaimProcStatus.NotReceived){
							sumPay+=claimProcs[i].InsPayEst;
						}
					}
					//Alter primary WO if needed.
					if(priClaimProcIdx!=-1){
						double procFee=proc.ProcFee*Math.Max(1,proc.BaseUnits+proc.UnitQty);
						if(sumPay+claimProcs[priClaimProcIdx].WriteOffEst > procFee) {
							double writeOffEst=procFee-sumPay;
							if(writeOffEst<0) {
								writeOffEst=0;
							}
							claimProcs[priClaimProcIdx].WriteOffEst=writeOffEst;
							if(saveToDb){
								ClaimProcs.Update(claimProcs[priClaimProcIdx]);
							}
						}
					}
				}
			}
		}

		///<summary>Passing in 4 will compute for 4 as well as any other situation such as dropped plan.</summary>
		private static void ComputeForOrdinal(int ordinal,List<ClaimProc> claimProcs,Procedure proc,List<InsPlan> planList,bool isInitialEntry,
			ref double paidOtherInsEstTotal,ref double paidOtherInsBaseEst,ref double writeOffEstOtherIns,
			List<PatPlan> patPlans,List<Benefit> benefitList,List<ClaimProcHist> histList,List<ClaimProcHist> loopList,bool saveToDb,int patientAge,
			List<InsSub> listInsSubs,List<SubstitutionLink> listSubLinks,bool useProcDateOnProc=false)
		{
			//No need to check RemotingRole; no call to db.
			InsPlan PlanCur;
			PatPlan patplan;
			ProcedureCode procCode=ProcedureCodes.GetProcCode(proc.CodeNum);
			List<long> listBWCodeNums=ProcedureCodes.ListBWCodeNums;
			List<long> listPanoFMXCodeNums=ProcedureCodes.ListPanoFMXCodeNums;
			List<long> listExamCodeNums=ProcedureCodes.ListExamCodeNums;
			for(int i=0;i<claimProcs.Count;i++) {
				if(claimProcs[i].ProcNum!=proc.ProcNum) {
					continue;
				}
				PlanCur=InsPlans.GetPlan(claimProcs[i].PlanNum,planList);
				if(PlanCur==null) {
					continue;//in older versions it still did a couple of small things even if plan was null, but don't know why
					//example:cap estimate changed to cap complete, and if estimate, then provnum set
					//but I don't see how PlanCur could ever be null
				}
				patplan=PatPlans.GetFromList(patPlans,claimProcs[i].InsSubNum);
				//capitation estimates are always forced to follow the status of the procedure
				if(PlanCur.PlanType=="c"
					&& (claimProcs[i].Status==ClaimProcStatus.CapComplete	|| claimProcs[i].Status==ClaimProcStatus.CapEstimate)) 
				{
					if(isInitialEntry) {
						//this will be switched to CapComplete further down if applicable.
						//This makes ComputeBaseEst work properly on new cap procs w status Complete
						claimProcs[i].Status=ClaimProcStatus.CapEstimate;
					}
					else if(proc.ProcStatus==ProcStat.C) {
						claimProcs[i].Status=ClaimProcStatus.CapComplete;
					}
					else {
						claimProcs[i].Status=ClaimProcStatus.CapEstimate;
					}
				}
				//ignored: adjustment
				//ComputeBaseEst automatically skips: capComplete,Preauth,capClaim,Supplemental
				//does recalc est on: CapEstimate,Estimate,NotReceived,Received
				//the cp is altered within ComputeBaseEst, but not saved unless cp is associated
				//to a lab then the sibling labs claimProcs are updated in CanadianLabBaseEstHelper(...)
				if(patplan==null) {//the plan for this claimproc was dropped 
					if(ordinal!=4) {//only process on the fourth round
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,0,
						benefitList,histList,loopList,patPlans,0,0,patientAge,0,planList,listInsSubs,listSubLinks,useProcDateOnProc);
				}
				else if(patplan.Ordinal==1){
					if(ordinal!=1) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns,planList,listInsSubs,
						listSubLinks,useProcDateOnProc);
				}
				else if(patplan.Ordinal==2){
					if(ordinal!=2) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns,planList,listInsSubs,
						listSubLinks,useProcDateOnProc);
				}
				else if(patplan.Ordinal==3) {
					if(ordinal!=3) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns,planList,listInsSubs,
						listSubLinks,useProcDateOnProc);
				}
				else{//patplan.Ordinal is 4 or greater.  Estimate won't be accurate if more than 4 insurances.
					if(ordinal!=4) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns,planList,listInsSubs,
						listSubLinks,useProcDateOnProc);
				}
				//This was a longstanding problem. I hope there are not other consequences for commenting it out.
				//claimProcs[i].DateCP=proc.ProcDate;
				claimProcs[i].ProcDate=proc.ProcDate;
				claimProcs[i].ClinicNum=proc.ClinicNum;
				//Wish we could do this, but it might change history.  It's needed when changing a completed proc to a different provider.
				//Can't do it here, though, because some people intentionally set provider different on claimprocs.
				//claimProcs[i].ProvNum=proc.ProvNum;
				if(isInitialEntry
					&&claimProcs[i].Status==ClaimProcStatus.CapEstimate
					&&proc.ProcStatus==ProcStat.C) 
				{
					claimProcs[i].Status=ClaimProcStatus.CapComplete;
				}
				//prov only updated if still an estimate
				if(claimProcs[i].Status==ClaimProcStatus.Estimate
					||claimProcs[i].Status==ClaimProcStatus.CapEstimate) {
					claimProcs[i].ProvNum=proc.ProvNum;
				}
				#region Frequencies
				if(histList!=null && benefitList!=null && (PrefC.GetBool(PrefName.InsChecksFrequency)) && proc.ProcDate.Year>=1880) {
					//The frequency will be compared to the loopList of ClaimProcs. Use it to check if any procedures have been completed for the patient within 
					//the frequency period. If this would put them over the limit then continue past the zero out the claimproc and continue. 
					//If no frequency information has been set then skip this logic for the current claimproc.
					//It is assumed for now that all benefits for a group will have the same quantity and quantityqualifier
					#region BW, Pano, and Exam Frequencies
					//NOTE:  Any changes to detecting the benefits in this section of code needs to be reflected in Benefits.GetFrequencyDisplay()--------------
					Benefit bwBenefit=benefitList.Find(x => x.CodeNum==ProcedureCodes.GetCodeNum("D0274") 
						&& x.BenefitType==InsBenefitType.Limitations
						&& x.MonetaryAmt==-1
						&& x.PatPlanNum==0
						&& x.Percent==-1
						&& x.PlanNum==PlanCur.PlanNum
						&& (x.QuantityQualifier==BenefitQuantity.Months
								|| x.QuantityQualifier==BenefitQuantity.Years
								|| x.QuantityQualifier==BenefitQuantity.NumberOfServices));
					Benefit panoBenefit=benefitList.Find(x => x.CodeNum==ProcedureCodes.GetCodeNum("D0330") 
						&& x.BenefitType==InsBenefitType.Limitations
						&& x.MonetaryAmt==-1
						&& x.PatPlanNum==0
						&& x.Percent==-1
						&& x.PlanNum==PlanCur.PlanNum
						&& (x.QuantityQualifier==BenefitQuantity.Months
								|| x.QuantityQualifier==BenefitQuantity.Years
								|| x.QuantityQualifier==BenefitQuantity.NumberOfServices));
					Benefit examBenefit=null;
					if(CovCats.GetForEbenCat(EbenefitCategory.RoutinePreventive)!=null){
						examBenefit=benefitList.Find(x => x.CodeNum==0 
							&& x.BenefitType==InsBenefitType.Limitations
							&& x.CovCatNum==CovCats.GetForEbenCat(EbenefitCategory.RoutinePreventive).CovCatNum
							&& x.MonetaryAmt==-1
							&& x.PatPlanNum==0
							&& x.Percent==-1
							&& x.PlanNum==PlanCur.PlanNum
							&& (x.QuantityQualifier==BenefitQuantity.Months
									|| x.QuantityQualifier==BenefitQuantity.Years
									|| x.QuantityQualifier==BenefitQuantity.NumberOfServices));
					}
					#endregion
					#region Benefits For Group
					List<Benefit> listBensForGroup=new List<Benefit>();
					if(listBWCodeNums.Contains(procCode.CodeNum)) {//The proc is part of the BW group
						//Check to see if the patient actually has the limitation benefit for BW's
						if(bwBenefit!=null ) {
							for(int j=0;j<listBWCodeNums.Count;j++) {//They have the limitation benefit.  
								Benefit benefit=bwBenefit.Copy();
								benefit.CodeNum=(long)listBWCodeNums[j];//will never be null
								listBensForGroup.Add(benefit);
							}
						}
					}
					else if(listPanoFMXCodeNums.Contains(procCode.CodeNum)) {//The proc is part of the Pano/FMX group
						//Check to see if the patient actually has the limitation benefit for Pano/FMX
						if(panoBenefit!=null ) {
							for(int j=0;j<listPanoFMXCodeNums.Count;j++) {//They have the limitation benefit.  
								Benefit benefit=panoBenefit.Copy();
								benefit.CodeNum=(long)listPanoFMXCodeNums[j];//will never be null
								listBensForGroup.Add(benefit);
							}
						}
					}
					else if(listExamCodeNums.Contains(procCode.CodeNum)) {//The proc is part of the Pano/FMX group
						//Check to see if the patient actually has the limitation benefit for Pano/FMX
						if(examBenefit!=null ) {
							for(int j=0;j<listExamCodeNums.Count;j++) {//They have the limitation benefit.  
								Benefit benefit=examBenefit.Copy();
								benefit.CodeNum=(long)listExamCodeNums[j];//will never be null
								listBensForGroup.Add(benefit);
							}
						}
					}
					else {//Everything else
						listBensForGroup=benefitList.FindAll(x => x.BenefitType==InsBenefitType.Limitations
							&& x.CodeNum==proc.CodeNum
							&& x.PlanNum==PlanCur.PlanNum
							&& (panoBenefit==null || x.BenefitNum!=panoBenefit.BenefitNum)
							&& (bwBenefit==null || x.BenefitNum!=bwBenefit.BenefitNum));//Takes care of Canadian codes (should only find one benefit generally)
					}
					if(listBensForGroup.Count==0) {//Benefit not found for BW/Pano/Exam groups, look to see if it's covered by a span
						listBensForGroup=benefitList.FindAll(x => x.BenefitType==InsBenefitType.Limitations 
							&& x.CovCatNum!=0
							&& x.PlanNum==PlanCur.PlanNum
							&& (examBenefit==null || x.BenefitNum!=examBenefit.BenefitNum)
							&& CovSpans.IsCodeInSpans(procCode.ProcCode,CovSpans.GetForCat(x.CovCatNum)));
					}
					#endregion
					//Look at its frequency, then look through loopList to see if the frequency has been met
					foreach(Benefit ben in listBensForGroup) {
						if(ben.QuantityQualifier==BenefitQuantity.Months || ben.QuantityQualifier==BenefitQuantity.Years) {
							DateTime datePast=new DateTime();
							if(ben.QuantityQualifier==BenefitQuantity.Months) {
								datePast=proc.ProcDate.AddMonths(0-ben.Quantity);
							}
							else {//Years
								datePast=proc.ProcDate.AddYears(0-ben.Quantity);
							}
							//Find received claimproc for the code/s wanted that's >= datePast
							string benCodeStr=ProcedureCodes.GetStringProcCode(ben.CodeNum);
							ClaimProcHist claimProcHist=histList.Find(x => x.StrProcCode==benCodeStr 
								&& x.Status==ClaimProcStatus.Received 
								&& x.ProcDate>=datePast 
								&& x.PatNum==proc.PatNum
								&& x.InsSubNum==claimProcs[i].InsSubNum
								&& IsSameProcedureArea(x.ToothNum,proc.ToothNum,x.ToothRange,proc.ToothRange,x.Surf,proc.Surf));
							if(claimProcHist!=null) {
								claimProcs[i].BaseEst=0;
								claimProcs[i].InsEstTotal=0;
								claimProcs[i].DedEst=0;
								claimProcs[i].DedEstOverride=0;
								claimProcs[i].DedApplied=0;
								if(claimProcs[i].EstimateNote!="") {
									claimProcs[i].EstimateNote+=", ";
								}
								claimProcs[i].EstimateNote+=Lans.g("Procedures","Frequency Limitation");
								break;
							}
						}
						if(ben.QuantityQualifier==BenefitQuantity.NumberOfServices) {
							DateTime datePast=new DateTime();
							if(ben.TimePeriod==BenefitTimePeriod.CalendarYear) {
								datePast=new DateTime(proc.ProcDate.Year,1,1);
							}
							else if(ben.TimePeriod==BenefitTimePeriod.ServiceYear) {
								InsPlan insPlan=planList.Find(x => x.PlanNum==ben.PlanNum);
								datePast=new DateTime(proc.ProcDate.Year,Math.Max(insPlan.MonthRenew,(byte)1),1);
								if(datePast>DateTime.Today) {
									datePast.AddYears(-1);
								}
							}
							//We are assuming all benefits for the group have the same number of services
							List<ClaimProcHist> claimProcHistList=histList.FindAll(x => x.Status==ClaimProcStatus.Received 
								&& x.ProcDate>=datePast 
								&& x.ProcDate<=datePast.AddYears(1) 
								&& listBensForGroup.Exists(y => ProcedureCodes.GetStringProcCode(y.CodeNum)==x.StrProcCode)
								&& x.PatNum==proc.PatNum
								&& x.InsSubNum==claimProcs[i].InsSubNum
								&& IsSameProcedureArea(x.ToothNum,proc.ToothNum,x.ToothRange,proc.ToothRange,x.Surf,proc.Surf));
							if(claimProcHistList!=null && claimProcHistList.Count>=ben.Quantity) {
								claimProcs[i].BaseEst=0;
								claimProcs[i].InsEstTotal=0;
								claimProcs[i].DedEst=0;
								claimProcs[i].DedEstOverride=0;
								claimProcs[i].DedApplied=0;
								if(claimProcs[i].EstimateNote!="") {
									claimProcs[i].EstimateNote+=", ";
								}
								claimProcs[i].EstimateNote+=Lans.g("Procedures","Frequency Limitation");
								break;
							}
						}
					}
				}
				#endregion
				//If patplan.Ordinal is 1, 2, or 3 and the claim proc in question is not a preauth.
				//There is no such thing as having "paid by other ins" when dealing with preauths.
				if(patplan!=null && new[] { 1,2,3 }.Contains(patplan.Ordinal) && claimProcs[i].Status!=ClaimProcStatus.Preauth) {
					if(claimProcs[i].Status==ClaimProcStatus.Received || claimProcs[i].Status==ClaimProcStatus.Supplemental) {
						paidOtherInsEstTotal+=claimProcs[i].InsPayAmt;
						paidOtherInsBaseEst+=claimProcs[i].InsPayAmt;
						writeOffEstOtherIns+=claimProcs[i].WriteOff;
					}
					else {
						if(claimProcs[i].InsEstTotalOverride!=-1) {
							paidOtherInsEstTotal+=claimProcs[i].InsEstTotalOverride;
						}
						else {
							paidOtherInsEstTotal+=claimProcs[i].InsEstTotal;
						}
						paidOtherInsBaseEst+=claimProcs[i].BaseEst;
						writeOffEstOtherIns+=ClaimProcs.GetWriteOffEstimate(claimProcs[i]);
					}
				}
				//Calculations done, copy over estimates from InsEstTotal into InsPayEst.  
				//This was already done in ComputeBaseEst but frequencies could have changed it
				//This could potentially be limited to claimprocs status Recieved or NotReceived, but there likely is no harm in doing it for all claimprocs.
				if(!claimProcs[i].InsEstTotalOverride.IsEqual(-1)) {
					claimProcs[i].InsPayEst=claimProcs[i].InsEstTotalOverride;
				}
				else {
					claimProcs[i].InsPayEst=claimProcs[i].InsEstTotal;
				}
				if(saveToDb) {
					ClaimProcs.Update(claimProcs[i]);
				}
			}
		}

		///<summary>This method is a helper for checking frequency limitations in ComputeForOrdinal(). The below booleans pain stakingly vet the passed in nums/ranges for handling empty strings.
		///Because we store toothrange,toothnum, and surf as strings they default to be empty which results in false positives in our logic. Checking for empty strings whilst doing our logic
		///in ComputeForOrdinal() results in a very ugly and unreadable linq statement. This method simply pulls out that logic and simplifies the linq statement above.</summary>
		public static bool IsSameProcedureArea(string histToothNum,string procCurToothNum,string histToothRangeStr,string procCurToothRangeStr,string histSurf,string procCurSurf) {
			//Procedures like exams and BW's do not ever specify a toothnum, toothrange, or surface.
			if(histToothNum=="" 
				&& procCurToothNum=="" 
				&& histToothRangeStr=="" 
				&& procCurToothRangeStr=="" 
				&& histSurf=="" 
				&& procCurSurf=="") 
			{
				return true;
			}
			//No need to check RemotingRole; no call to db.
			string[] histToothRange=histToothRangeStr.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries);
			string[] procCurToothRange=procCurToothRangeStr.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries);
			bool hasToothRangeOverlap=histToothRange.Intersect(procCurToothRange).Count() > 0;
			bool hasCurToothInHistRange=histToothRange.Any(y => y!="" && y==procCurToothNum);
			bool hasHistToothInCurRange=procCurToothRange.Contains(histToothNum);
			bool hasSameToothNum=!string.IsNullOrWhiteSpace(procCurToothNum) && histToothNum==procCurToothNum;
			bool hasSameSurface=histSurf==procCurSurf;
			if(hasSameSurface
					&& (hasSameToothNum
						|| hasHistToothInCurRange
						|| hasCurToothInHistRange
						|| hasToothRangeOverlap))//Not sure if this should be a "Contains" due to "UL" and "U".  The job was written to be Surf==Surf. 
			{
				return true;
			}
			return false;
		}

		///<summary>After changing important coverage plan info, this is called to recompute estimates for all procedures for this patient.</summary>
		public static void ComputeEstimatesForAll(long patNum,List<ClaimProc> claimProcs,List<Procedure> procs,List<InsPlan> planList,
			List<PatPlan> patPlans,List<Benefit> benefitList,int patientAge,List<InsSub> subList,List<ClaimProc> listClaimProcsAll=null,bool isClaimProcRemoveNeeded=false) 
		{
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<procs.Count;i++) {
				ComputeEstimates(procs[i],patNum,ref claimProcs,false,planList,patPlans,benefitList,null,null,true,patientAge,subList,useProcDateOnProc:false,
					listClaimProcsAll: listClaimProcsAll,isClaimProcRemoveNeeded: isClaimProcRemoveNeeded);
			}
		}

		///<summary>Loops through each proc in the dictionary.  Dictionary key is the index in the list of procs for appointment edit, only used if called
		///from FormApptEdit.  Does not add notes to a procedure that already has notes. Only called from ProcedureL.SetCompleteInAppt, security checked
		///before calling this.  Also sets provider for each proc and claimproc.  Returns dictIndexInListForProc with changes made to the procs.</summary>
		public static List<Procedure> SetCompleteInAppt(Appointment apt,List<InsPlan> planList,List<PatPlan> patPlans,long siteNum,int patientAge,
			List<Procedure> listProcsForAppt,List<InsSub> subList,Userod curUser)
		{
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),apt,planList,patPlans,siteNum,patientAge,listProcsForAppt,subList,curUser);
			}
			if(listProcsForAppt.Count==0) {
				return listProcsForAppt;//Nothing to do.
			}
			List<ClaimProc> claimProcList=ClaimProcs.Refresh(apt.PatNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlans,subList);
			//most recent note will be first in list.
			string command="SELECT * FROM procnote "
				+"WHERE ProcNum IN("+string.Join(",",listProcsForAppt.Select(x => x.ProcNum))+") "
				+"ORDER BY EntryDateTime DESC";
			DataTable rawNotes=Db.GetTable(command);
			ProcedureCode procCode;
			Procedure procOld;
			List<long> encounterProvNums=new List<long>();//for auto-inserting default encounters
			foreach(Procedure procCur in listProcsForAppt) {
				//Should only be procs for this appointment
				//attach the note, if it exists.
				foreach(DataRow row in rawNotes.Rows) {
					if(procCur.ProcNum.ToString()!=row["ProcNum"].ToString()) {
						continue;
					}
					procCur.UserNum=PIn.Long(row["UserNum"].ToString());
					procCur.Note=PIn.String(row["Note"].ToString());
					procCur.SigIsTopaz=PIn.Bool(row["SigIsTopaz"].ToString());
					procCur.Signature=PIn.String(row["Signature"].ToString());
					break;//out of note loop.
				}
				procOld=procCur.Copy();
				procCode=ProcedureCodes.GetProcCode(procCur.CodeNum);
				if(procCode.PaintType==ToothPaintingType.Extraction) {//if an extraction, then mark previous procs hidden
					//SetHideGraphical(procCur);//might not matter anymore
					ToothInitials.SetValue(apt.PatNum,procCur.ToothNum,ToothInitialType.Missing);
				}
				procCur.ProcStatus=ProcStat.C;
				if(procOld.ProcStatus!=ProcStat.C) {
					procCur.ProcDate=apt.AptDateTime.Date;//only change date to match appt if not already complete.
					if(procCur.ProcDate.Year<1880) {
						procCur.ProcDate=MiscData.GetNowDateTime().Date;//Change procdate to today if the appointment date was invalid
					}
					procCur.DateEntryC=DateTime.Now;//this triggers it to set to server time NOW().
					if(procCur.DiagnosticCode=="") {
						procCur.DiagnosticCode=PrefC.GetString(PrefName.ICD9DefaultForNewProcs);
					}
				}
				procCur.PlaceService=(PlaceOfService)PrefC.GetLong(PrefName.DefaultProcedurePlaceService);
				procCur.ClinicNum=apt.ClinicNum;
				procCur.SiteNum=siteNum;
				procCur.PlaceService=Clinics.GetPlaceService(apt.ClinicNum);
				procCur.ProvNum=GetProvNumFromAppointment(apt,procCur,procCode);
				//if procedure was already complete, then don't add more notes.
				if(procOld.ProcStatus!=ProcStat.C) {
					string procNoteDefault = ProcCodeNotes.GetNote(procCur.ProvNum,procCur.CodeNum,procCur.ProcStatus);
					if(procCur.Note!="" && procNoteDefault!="") {
						procCur.Note+="\r\n"; //add a new line if there was already a ProcNote on the procedure.
					}
					procCur.Note+=procNoteDefault;
				}
				if(Userods.IsUserCpoe(curUser)) {
					//Only change the status of IsCpoe to true.  Never set it back to false for any reason.  Once true, always true.
					procCur.IsCpoe=true;
				}
				SetOrthoProcComplete(procCur,procCode);
				if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {//Canada
					SetCanadianLabFeesCompleteForProc(procCur);
				}
				Plugins.HookAddCode(null,"Procedures.SetCompleteInAppt_procLoop",procCur,procOld);
				Update(procCur,procOld);//Updates payplan charges for the procedure if it went from any status to complete.
				ComputeEstimates(procCur,apt.PatNum,claimProcList,false,planList,patPlans,benefitList,patientAge,subList);
				ClaimProcs.SetProvForProc(procCur,claimProcList);
				//Add provnum to list to create an encounter later. Done to limit calls to DB from Encounters.InsertDefaultEncounter().
				if(procOld.ProcStatus!=ProcStat.C) {//check for distinct later.
					encounterProvNums.Add(procCur.ProvNum);
				}
			}
			//Auto-insert default encounters for the providers that did work on this appointment
			encounterProvNums.Distinct().ToList().ForEach(x => Encounters.InsertDefaultEncounter(apt.PatNum,x,apt.AptDateTime));
			Recalls.Synch(apt.PatNum);
			return listProcsForAppt;
			//Patient pt=Patients.GetPat(apt.PatNum);
			//jsparks-See notes within this method:
			//Reporting.Allocators.AllocatorCollection.CallAll_Allocators(pt.Guarantor);
		}

		///<summary>Returns all the unique diagnostic codes in the list.  If there is less than 12 unique codes then it will pad the list with empty
		///entries if isPadded is true.  Will always place the principal diagnosis as the first item in the list.</summary>
		public static List<string> GetUniqueDiagnosticCodes(List<Procedure> listProcs,bool isPadded) {
			return GetUniqueDiagnosticCodes(listProcs,isPadded,new List<byte>());
		}

		///<summary>Returns all the unique diagnostic codes in the list.  If there is less than 12 unique codes then it will pad the list with empty
		///entries if isPadded is true.  Will always place the principal diagnosis as the first item in the list.  The returned list and
		///listDiagnosticVersions will be the same length upon return.  When returning, listDiagnosticVersions will contain the diagnostic code versions
		///of each code in the returned list, used for allowing the user to mix diagnostic code versions on a single claim.  The listDiagnosticVersions
		///must be a valid list (not null).</summary>
		public static List<string> GetUniqueDiagnosticCodes(List<Procedure> listProcs,bool isPadded,List<byte> listDiagnosticVersions) {
			//No need to check RemotingRole; no call to db.
			List<string> listDiagnosticCodes=new List<string>();
			listDiagnosticVersions.Clear();
			for(int i=0;i<listProcs.Count;i++) {//Ensure that the principal diagnosis is first in the list.
				Procedure proc=listProcs[i];
				if(proc.IcdVersion==9) {
					continue;
				}
				if(proc.IsPrincDiag && proc.DiagnosticCode!="") {
					listDiagnosticCodes.Add(proc.DiagnosticCode);
					listDiagnosticVersions.Add(proc.IcdVersion);
					break;
				}
			}
			for(int i=0;i<listProcs.Count;i++) {
				Procedure proc=listProcs[i];
				if(proc.IcdVersion==9) {//don't return icd9 codes.
					continue;
				}
				if(proc.DiagnosticCode!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
				if(proc.DiagnosticCode2!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode2,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode2);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
				if(proc.DiagnosticCode3!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode3,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode3);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
				if(proc.DiagnosticCode4!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode4,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode4);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
			}
			while(isPadded && listDiagnosticCodes.Count<12) {//Pad to at least 12 items.  Simplifies claim printing logic.
				listDiagnosticCodes.Add("");
				listDiagnosticVersions.Add(0);
			}
			return listDiagnosticCodes;
		}

		///<summary>Both listDiagCodes and listDiagVersions must be the same length and not null.</summary>
		private static bool ExistsDiagnosticCode(List<string> listDiagCodes,List<byte> listDiagVersions,string diagnosticCode,byte diagnosticVersion)	{
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<listDiagCodes.Count;i++) {
				if(listDiagCodes[i]==diagnosticCode && listDiagVersions[i]==diagnosticVersion) {
					return true;
				}
			}
			return false;
		}

		///<summary></summary>
		public static long GetClinicNum(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),procNum);
			}
			string command="SELECT ClinicNum FROM procedurelog WHERE ProcNum="+POut.Long(procNum);
			return PIn.Long(Db.GetScalar(command));
		}

		///<summary>Returns the given procs proc fee but considers BaseUnits and UnitQty</summary>
		public static double CalculateProcCharge(Procedure proc) {
			return proc.ProcFee*Math.Max(1,proc.BaseUnits+proc.UnitQty);
		}

		//public static bool IsUsingCode(long codeNum) {
		//	if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
		//		return Meth.GetBool(MethodBase.GetCurrentMethod(),codeNum);
		//	}
		//	string command="SELECT COUNT(*) FROM procedurelog WHERE CodeNum="+POut.Long(codeNum);
		//	if(Db.GetCount(command)=="0") {
		//		return false;
		//	}
		//	return true;
		//}

		public static void SetCanadianLabFeesCompleteForProc(Procedure proc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),proc);
				return;
			}
			//If this gets run on a lab fee itself, nothing will happen because result will be zero procs.
			string command="SELECT * FROM procedurelog WHERE ProcNumLab="+proc.ProcNum+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
			List<Procedure> labFeesForProc=Crud.ProcedureCrud.SelectMany(command);
			if(proc.ProcNumLab==0) {//Regular procedure, not a lab.
				for(int i=0;i<labFeesForProc.Count;i++) {
					Procedure labFeeNew=labFeesForProc[i];
					Procedure labFeeOld=labFeeNew.Copy();
					labFeeNew.AptNum=proc.AptNum;
					labFeeNew.CanadianTypeCodes=proc.CanadianTypeCodes;
					labFeeNew.ClinicNum=proc.ClinicNum;
					labFeeNew.DateEntryC=proc.DateEntryC;
					labFeeNew.PlaceService=proc.PlaceService;
					labFeeNew.ProcDate=proc.ProcDate;
					labFeeNew.ProcStatus=ProcStat.C;
					labFeeNew.ProvNum=proc.ProvNum;
					labFeeNew.SiteNum=proc.SiteNum;
					labFeeNew.UserNum=proc.UserNum;
					Update(labFeeNew,labFeeOld);
				}
			}
			else {//Lab fee.  Set complete, set the parent procedure as well as any other lab fees complete.
				command="SELECT * FROM procedurelog WHERE ProcNum="+proc.ProcNumLab+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
				Procedure procParent=Crud.ProcedureCrud.SelectOne(command);
				SetCanadianLabFeesCompleteForProc(procParent);
				Procedure parentProcNew=procParent;
				Procedure parentProcOld=procParent.Copy();
				parentProcNew.ProcStatus=ProcStat.C;
				Update(parentProcNew,parentProcOld);
			}
		}

		public static void SetCanadianLabFeesStatusForProc(Procedure proc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),proc);
				return;
			}
			//If this gets run on a lab fee itself, nothing will happen because result will be zero procs.
			string command="SELECT * FROM procedurelog WHERE ProcNumLab="+proc.ProcNum+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
			List<Procedure> labFeesForProc=Crud.ProcedureCrud.SelectMany(command);
			if(proc.ProcNumLab==0) {//Regular procedure, not a lab.
				for(int i=0;i<labFeesForProc.Count;i++) {
					Procedure labFeeNew=labFeesForProc[i];
					Procedure labFeeOld=labFeeNew.Copy();
					labFeeNew.ProcStatus=proc.ProcStatus;
					Update(labFeeNew,labFeeOld);
				}
			}
			else {//Lab fee.  If lab is set back to any status other than complete, set the parent procedure as well as any other lab fees back to that status.
				command="SELECT * FROM procedurelog WHERE ProcNum="+proc.ProcNumLab+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
				Procedure procParent=Crud.ProcedureCrud.SelectOne(command);
				Procedure parentProcNew=procParent;
				Procedure parentProcOld=procParent.Copy();
				parentProcNew.ProcStatus=proc.ProcStatus;
				SetCanadianLabFeesStatusForProc(parentProcNew);
				Update(parentProcNew,parentProcOld);
			}
		}

		public static void DeleteCanadianLabFeesForProcCode(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum);
				return;
			}
			string command="SELECT * FROM procedurelog WHERE ProcNumLab="+procNum+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
			List<Procedure> labFeeProcs=Crud.ProcedureCrud.SelectMany(command);
			for(int i=0;i<labFeeProcs.Count;i++) {
				Delete(labFeeProcs[i].ProcNum);
			}
		}

		/////<summary>Gets the number of procedures attached to a claim.</summary>
		//public static int GetCountForClaim(long claimNum) {
		//	if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
		//		return Meth.GetInt(MethodBase.GetCurrentMethod(),claimNum);
		//	}
		//	string command=
		//		"SELECT COUNT(*) FROM procedurelog "
		//		+"WHERE ProcNum IN "
		//		+"(SELECT claimproc.ProcNum FROM claimproc "
		//		+" WHERE ClaimNum="+claimNum+")";
		//	return PIn.Int(Db.GetCount(command));
		//}

		///<summary>Gets a list of procedures for </summary>
		public static DataTable GetReferred(DateTime dateFrom, DateTime dateTo, bool complete) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateFrom,dateTo,complete);
			}
			string command=
				"SELECT procedurelog.CodeNum,procedurelog.PatNum,LName,FName,MName,RefDate,DateProcComplete,refattach.Note,RefToStatus "
				+"FROM procedurelog "
				+"JOIN refattach ON procedurelog.ProcNum=refattach.ProcNum "
				+"JOIN referral ON refattach.ReferralNum=referral.ReferralNum "
				+"WHERE RefDate>="+POut.Date(dateFrom)+" "
				+"AND RefDate<="+POut.Date(dateTo)+" ";
			if(!complete) {
				command+="AND DateProcComplete="+POut.Date(DateTime.MinValue)+" ";
			}
			command+="ORDER BY RefDate";
			return Db.GetTable(command);
		}

		///<summary></summary>
		public static void Lock(DateTime date1, DateTime date2) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),date1,date2);
				return;
			}
			string command="UPDATE procedurelog SET IsLocked=1 "
				+"WHERE (ProcStatus="+POut.Int((int)ProcStat.C)+" "//completed
				+"OR CodeNum="+POut.Long(ProcedureCodes.GetCodeNum(ProcedureCodes.GroupProcCode))+") "//or group note
				+"AND ProcDate >= "+POut.Date(date1)+" "
				+"AND ProcDate <= "+POut.Date(date2);
			Db.NonQ(command);
		}

		///<summary>Inserts, updates, or deletes database rows to match supplied list.  Must always pass in two lists.</summary>
		public static void Sync(List<Procedure> listNew,List<Procedure> listOld,long userNum=0) {
			if(RemotingClient.RemotingRole!=RemotingRole.ServerWeb) {
				userNum=Security.CurUser.UserNum;
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listNew,listOld,userNum);
				return;
			}
			Crud.ProcedureCrud.Sync(listNew,listOld,userNum);
		}

		public static void SetTPActive(long patNum,List<long> listProcNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum,listProcNums); //Never pass DB list through the web service (Note: Why?  Our proc list is special, it doesn't contain all procs so we shouldn't code this method to always use our limited list of procs........)
				return;
			}
			string command="UPDATE procedurelog SET ProcStatus="+POut.Int((int)ProcStat.TPi)+" WHERE PatNum="+POut.Long(patNum)+" "+
			  "AND ProcStatus="+POut.Int((int)ProcStat.TP)+" ";
			if(listProcNums.Count==0) {
				Db.NonQ(command);
				return; //no procedures left on active plan
			}
			command+="AND ProcNum NOT IN ("+string.Join(",",listProcNums)+") ";
			Db.NonQ(command);
			command="UPDATE procedurelog SET ProcStatus="+POut.Int((int)ProcStat.TP)+" WHERE PatNum="+POut.Long(patNum)+" "+
			  "AND ProcStatus="+POut.Int((int)ProcStat.TPi)+" AND ProcNum IN ("+string.Join(",",listProcNums)+") ";
			Db.NonQ(command);
		}

		///<summary>Gets the placement date for ortho patients.
		///Takes the patient's patientNote.DateOrthoPlacementOverride and preference.OrthoPlacementProcsList into account.
		///Uses the first D8* procedure if neither of the above are found.  Returns DateTime.MinValue if no proc found.</summary>
		public static DateTime GetFirstOrthoProcDate(PatientNote patNoteCur) {
			//No need to check RemotingRole; no call to db.
			if(patNoteCur.DateOrthoPlacementOverride != DateTime.MinValue) {
				//if an override is set, use that.
				return patNoteCur.DateOrthoPlacementOverride;
			}
			DateTime firstOrthoProcDate = DateTime.MinValue;
			List<long> listOrthoProcNums = ProcedureCodes.GetOrthoBandingCodeNums();
			//otherwise, use the proc of one of the codes specified in the pref.
			Procedure proc=Procedures.GetProcsByStatusForPat(patNoteCur.PatNum,ProcStat.C)
				.Where(x => listOrthoProcNums.Contains(x.CodeNum))
				.OrderBy(x => x.ProcDate)//Earliest ortho placement first.
				.FirstOrDefault();
			if(proc!=null) {
				firstOrthoProcDate=proc.ProcDate;
			}
			return firstOrthoProcDate;
		}

		///<summary>Does nothing if not an ortho proc (dictated by the pref OrthoPlacementProcsList or a code starting with D8 if pref not set).
		///PatPlan table needs to be updated when an ortho placement procedure is set complete.
		///Only updates the date if no OrthoAutoNextClaimDate is set on the corresponding PatPlan. Updates PatientNote.OrthoMonthsTreatOverride
		///if this is the first Ortho placement proc set complete.</summary>
		public static void SetOrthoProcComplete(Procedure procCur,ProcedureCode procCode) {
			if(procCode == null || procCur == null) { //this should never happen unless they have some corruption
				return;
			}
			List<long> listOrthoPlacementCodeNums = ProcedureCodes.GetOrthoBandingCodeNums();
			if(listOrthoPlacementCodeNums.Count > 0) {
				if(!listOrthoPlacementCodeNums.Contains(procCode.CodeNum)) {
					return;
				}
			}
			else if(!procCode.ProcCode.StartsWith("D8080") && !procCode.ProcCode.StartsWith("D8090")) {
				return;
			}
			List<PatPlan> listPatPlans = PatPlans.GetPatPlansForPat(procCur.PatNum);
			foreach(PatPlan patPlanCur in listPatPlans) {
				if(patPlanCur.OrthoAutoNextClaimDate.Date != DateTime.MinValue.Date) {
					continue;
				}
				InsPlan insPlanCur = InsPlans.GetByInsSubs(new List<long> { patPlanCur.InsSubNum }).FirstOrDefault();
				if(insPlanCur == null || insPlanCur.OrthoType != OrthoClaimType.InitialPlusPeriodic) {
					continue;
				}
				TimeSpan waitDays = TimeSpan.FromDays(0);
				waitDays = TimeSpan.FromDays(insPlanCur.OrthoAutoClaimDaysWait);
				DateTime procWaitDays = procCur.ProcDate + waitDays;
				procWaitDays=procWaitDays.AddMonths(1);//Always push the next claim date out a month, even after the "wait days" preference.
				patPlanCur.OrthoAutoNextClaimDate = new DateTime(procWaitDays.Year,procWaitDays.Month,1);
				PatPlans.Update(patPlanCur);
			}
			if(procCur.ProcStatus==ProcStat.C) {//Only make this change if Complete procedure.(Currently, will always be true, but for safety).
				Patient patCur=Patients.GetLim(procCur.PatNum);//GetLim because we just need pat.Guarantor.
				PatientNote patNoteCur=PatientNotes.Refresh(patCur.PatNum,patCur.Guarantor);//Inserts PatientNote rows if one does not exists for PatNum AND Guarantor.
				//First time completing an Ortho placement procedure, so we don't have an override in place yet. Any subsequent Ortho procs will use the same
				//override as the first Ortho proc.
				Byte defaultMonths=PrefC.GetByte(PrefName.OrthoDefaultMonthsTreat);
				//Only set the override if one has not already been set.
				if(patNoteCur.OrthoMonthsTreatOverride==-1) {
					//Set OrthoMonthsTreatOverride to PrefName.OrthoDefaultMonthsTreat, so we don't overwrite it if the practice default changes later.
					patNoteCur.OrthoMonthsTreatOverride=defaultMonths;//Use current practice default.
					PatientNotes.Update(patNoteCur,patCur.Guarantor);
				}
			}
		}

		///<summary>Decides the base description to send to insurance based off of the claimproc.
		///This function mimics logic from ContrAccount.CreateClaim() when setting CodeSent.
		///If the logic in this function changes, then consider changing ContrAccount.CreateClaim() as well.</summary>
		public static string GetClaimDescript(ClaimProc claimProcCur,ProcedureCode procCodeSent,Procedure procCur,ProcedureCode procCodeCur,InsPlan planCur=null) {
			//No need to check RemotingRole; no call to db.
			string descript=procCodeSent.Descript;
			if(PrefC.GetBool(PrefName.ClaimPrintProcChartedDesc)) {
				if(planCur==null) {
					planCur=InsPlans.GetPlan(claimProcCur.PlanNum,null);
				}
				//If the proccode was not overridden by a alternate code or a medical code,
				//then use the orignal procedure code description instead of the codesent description.
				if((procCodeCur.AlternateCode1=="" || !planCur.UseAltCode) 
					&& (procCodeCur.MedicalCode=="" || !planCur.IsMedical))
				{
					descript=procCodeCur.Descript;
				}
			}
			return descript;
		}

		///<summary>Gets all completed procedures within a date range with optional ProcCodeNum and PatientNum filters. Date range is inclusive.</summary>
		public static List<Procedure> GetCompletedForDateRangeLimited(DateTime dateStart,DateTime dateStop,List<long> listClinicNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),dateStart,dateStop,listClinicNums);
			}
			string command = "SELECT ProcNum,ProcFee,UnitQty,BaseUnits,ClinicNum,CodeNum,ProcDate "
				+"FROM procedurelog WHERE ProcStatus="+POut.Int((int)ProcStat.C)+" "
				+"AND ProcDate>="+POut.Date(dateStart)+" AND ProcDate<="+POut.Date(dateStop);
			if(listClinicNums != null && listClinicNums.Count > 0) {
				command+=" AND ClinicNum IN ("+string.Join(",",listClinicNums)+")";
			}
			DataTable table = Db.GetTable(command);
			List<Procedure> listProcsLim = new List<Procedure>();
			foreach(DataRow row in table.Rows) {
				Procedure proc = new Procedure();
				proc.ProcNum=PIn.Long(row["ProcNum"].ToString());
				proc.ProcFee=PIn.Double(row["ProcFee"].ToString());
				proc.UnitQty=PIn.Int(row["UnitQty"].ToString());
				proc.BaseUnits=PIn.Int(row["BaseUnits"].ToString());
				proc.ClinicNum=PIn.Long(row["ClinicNum"].ToString());
				proc.CodeNum=PIn.Long(row["CodeNum"].ToString());
				proc.ProcDate=PIn.Date(row["ProcDate"].ToString());
				listProcsLim.Add(proc);
			}
			return listProcsLim;
		}

		///<summary>Helper method that returns a list of helper ProcExtended objects that will aggregate and sum up things based on the lists passed in.</summary>
		public static List<ProcExtended> GetProcExtendedEntriesFromProcedures(List<Procedure> listProcs,List<Adjustment> listAdjs,
			List<PaySplit> listPaySplits,List<ClaimProc> listClaimProcs,List<PayPlanCharge> listPayPlanCharges,List<PaySplit> listSplitsCur=null, 
			params ProcAttachTypes[] excludedTypes) 
		{
			//No need to check RemotingRole; no call to db.
			List<ProcExtended> retVal = new List<ProcExtended>();
			foreach(Procedure proc in listProcs) {
				ProcExtended procE = new ProcExtended() {
					Proc=proc,
					Adjustments=listAdjs.Where(x => x.ProcNum == proc.ProcNum).ToList(),
					PaySplits=listPaySplits.Where(x => x.ProcNum == proc.ProcNum).ToList(),
					ClaimProcs=listClaimProcs.Where(x => x.ProcNum == proc.ProcNum).ToList(),
					PayPlanCredits=listPayPlanCharges.Where(x => x.ProcNum == proc.ProcNum).ToList(),
					ExcludedTypes=excludedTypes.ToList(),
					SplitsCur=listSplitsCur.Where(x => x.ProcNum == proc.ProcNum).ToList(),
				};
				retVal.Add(procE);
			}
			return retVal;
		}

		///<summary>Sets either the AptNum or Planned AptNum for given procs.
		///Uses listSelectedRows and listProcNumsAttachedStart to determine if procs are attaching to or detaching from AptCur.
		///When moving proc from another appt, other appt descriptions are updated.</summary>
		public static void ProcsAptNumHelper(List<Procedure> listProcs,Appointment AptCur,List<Appointment> listAppointments,
			List<int> listSelectedRows,List<long> listProcNumsAttachedStart,bool isAptPlanned=false,LogSources logSource=LogSources.None)
		{
			//No need to check RemotingRole; no call to db.
			if(listProcs==null || AptCur==null || listAppointments==null || listSelectedRows==null || listProcNumsAttachedStart==null) {
				return;
			}
			for(int i=0;i<listProcs.Count;i++) {
				Procedure proc=listProcs[i];
				Procedure procOld=proc.Copy();
				bool isAttaching=listSelectedRows.Contains(i);
				bool isDetaching=(!isAttaching);
				bool isAttachedStart=listProcNumsAttachedStart.Contains(proc.ProcNum);
				bool isDetachedStart=(!isAttachedStart);
				if(isDetaching && isAptPlanned && proc.PlannedAptNum==AptCur.AptNum) {//Detatching from this planned appointment.
					proc.PlannedAptNum=0;
				}
				else if(isDetaching && !isAptPlanned && proc.AptNum==AptCur.AptNum) {//Detatching from this appointment.
					proc.AptNum=0;
				}
				else if(isDetachedStart && isAttaching && isAptPlanned) {//Attaching to this planned appointment.
					if(proc.PlannedAptNum!=0 && proc.PlannedAptNum != AptCur.AptNum) {//Currently attached to another planned appointment.
						Appointment apptOldPlanned=listAppointments.FirstOrDefault(x => x.AptNum==proc.PlannedAptNum && x.AptStatus==ApptStatus.Planned);
						string apptOldPlannedDateStr=(apptOldPlanned==null ? "[INVALID #"+proc.PlannedAptNum+"]" : apptOldPlanned.AptDateTime.ToShortDateString());
						SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,AptCur.PatNum,Lans.g("AppointmentEdit","Procedure")+" "
							+ProcedureCodes.GetProcCode(proc.CodeNum).AbbrDesc+" "+Lans.g("AppointmentEdit","moved from planned appointment created on")+" "
							+apptOldPlannedDateStr+" "+Lans.g("AppointmentEdit","to planned appointment created on")+" "
							+AptCur.AptDateTime.ToShortDateString(),logSource);
						UpdateOtherApptDesc(proc,AptCur,isAptPlanned,listAppointments,listProcs);
					}
					proc.PlannedAptNum=AptCur.AptNum;
				}
				else if(isDetachedStart && isAttaching && !isAptPlanned) {//Attaching to this appointment.
					if(proc.AptNum!=0 && proc.AptNum != AptCur.AptNum) {//Currently attached to another appointment.
						Appointment apptOld=listAppointments.FirstOrDefault(x => x.AptNum==proc.AptNum);
						string apptOldDateStr=(apptOld==null ? "[INVALID #"+proc.AptNum+"]" : apptOld.AptDateTime.ToShortDateString());
						SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,AptCur.PatNum,Lans.g("AppointmentEdit","Procedure")+" "
							+ProcedureCodes.GetProcCode(proc.CodeNum).AbbrDesc+" "+Lans.g("AppointmentEdit","moved from appointment on")+" "+apptOldDateStr
							+" "+Lans.g("AppointmentEdit","to appointment on")+" "+AptCur.AptDateTime,logSource);
						UpdateOtherApptDesc(proc,AptCur,isAptPlanned,listAppointments,listProcs);
					}
					proc.AptNum=AptCur.AptNum;
				}
				else {
					continue;//No changes were made to the current procedure.
				}
				Procedures.Update(proc,procOld);//Update above changes to db.
			}
		}
		
		///<summary>Updates the procs description and AptNum of the both the old appointment and appt.
		///listAppts should contain the procs previous appt.
		///ListProcsForAppt and listProcCodes can be null.</summary>
		public static void UpdateOtherApptDesc(Procedure proc,Appointment appt,bool isApptPlanned,List<Appointment> listAppts,List<Procedure> listProcsForAppt) {
			Appointment apptPrevious;
			if(isApptPlanned) {
				apptPrevious=listAppts.FirstOrDefault(x => x.AptNum==proc.PlannedAptNum);
				proc.PlannedAptNum=appt.AptNum;
			}
			else {
				apptPrevious=listAppts.FirstOrDefault(x => x.AptNum==proc.AptNum);
				proc.AptNum=appt.AptNum;
			}
			if(apptPrevious!=null) {
				//apptPrevious gets updated in memory which causes listAppts to contain the changes.
				Appointments.SetProcDescript(apptPrevious,listProcsForAppt);
			}
		}
		
		///<summary>Creates securitylog entry for a completed procedure.  Set toothNum to empty string and it will be omitted from the log entry. toothNums can be null or empty.</summary>
		public static void LogProcComplCreate(long patNum,Procedure procCur,string toothNums) {
			//No need to check RemotingRole; no call to db.
			if(procCur==null) {
				return;//Nothing to do.  Should never happen.
			}
			ProcedureCode procCode=ProcedureCodes.GetProcCode(procCur.CodeNum);
			string logText=procCode.ProcCode+", ";
			if(toothNums!=null && toothNums.Trim()!="") {
				logText+=Lans.g("Procedures","Teeth")+": "+toothNums+", ";
			}
			logText+=Lans.g("Procedures","Fee")+": "+procCur.ProcFee.ToString("F")+", "+procCode.Descript;
			SecurityLogs.MakeLogEntry(Permissions.ProcComplCreate,patNum,logText);
		}

		///<summary>Creates securitylog entry for completed procedure where appointment ProvNum is different than the procedures provnum.</summary>
		private static void LogProcComplEdit(Procedure proc,Procedure procOld,List<ProcedureCode> listProcedureCodes=null) {
			ProcedureCode procCode=ProcedureCodes.GetProcCode(proc.CodeNum,listProcedureCodes);
			string logText=Lans.g("Procedures","Completed procedure")+" "+procCode.ProcCode.ToString()+" "
				+Lans.g("Procedures","edited by setting appointment complete.");
			if(proc.ProvNum!=procOld.ProvNum) {
				logText+=" "+Lans.g("Procedures","Provider was changed from")+" "+Providers.GetAbbr(procOld.ProvNum)+" "+Lans.g("Procedures","to")+" "+
					Providers.GetAbbr(proc.ProvNum)+".";
			}
			SecurityLogs.MakeLogEntry(Permissions.ProcComplEdit,proc.PatNum,logText,proc.ProcNum,LogSources.None,procOld.DateTStamp);
		}

		///<summary>Returns true when automation needed.
		///Sets the provider and clinic for a proc based on the appt to which it is attached.
		///Also sets ProcDate for TP procs. Will automatically set procs in listProcs to complete and make securitylogs.</summary>
		public static bool UpdateProcsInApptHelper(List<Procedure> listProcsForAppt,Patient pat,Appointment AptCur,Appointment AptOld,
			List<InsPlan> PlanList,List<InsSub> SubList,List<int> listProcSelectedIndices,bool removeCompletedProcs,bool doUpdateProcFees=false,
			LogSources logSource=LogSources.None)
		{ 
			if(AptCur.AptStatus!=ApptStatus.Complete) {//appt is not set complete, just update necessary fields like ProvNum, ProcDate, and ClinicNum
				foreach(int index in listProcSelectedIndices) {
					//We only want to change the fields that just changed.  We don't want to undo any changes that are being made outside this window.  Note
					//that if we make any other changes to this proc that are not in this section we should consolidate update statements.
					Procedure procCur=listProcsForAppt[index];
					Procedure procOld=procCur.Copy();
					Permissions perm=Permissions.ProcComplEdit;
					if(procOld.ProcStatus.In(ProcStat.EO,ProcStat.EC)) {
						perm=Permissions.ProcExistingEdit;
					}
					if(procOld.ProcStatus.In(ProcStat.C,ProcStat.EO,ProcStat.EC) && !Security.IsAuthorized(perm,procOld.ProcDate,true)) {
						continue;
					}
					UpdateProcInAppointment(AptCur,procCur);//Doesn't update DB
					if(doUpdateProcFees) {
						procCur.ProcFee=Fees.GetAmount0(procCur.CodeNum,Providers.GetProv(procCur.ProvNum).FeeSched,procCur.ClinicNum,procCur.ProvNum);
					}
					Update(procCur,procOld);//Update fields if needed.
				}
			}
			else if(listProcSelectedIndices.Any(x => listProcsForAppt[x].ProcStatus!=ProcStat.C)) {
				//if appointment is marked complete and any procedures are not, then set the remaining procedures complete.
				List<PatPlan> listPatPlans=PatPlans.Refresh(AptCur.PatNum);
				SetCompleteInAppt(AptCur,PlanList,listPatPlans,pat.SiteNum,pat.Age,SubList,removeCompletedProcs);
				if(AptOld.AptStatus==ApptStatus.Complete) {//seperate log entry for completed appointments
					SecurityLogs.MakeLogEntry(Permissions.AppointmentCompleteEdit,pat.PatNum,AptCur.AptDateTime.ToShortDateString()
						+", "+AptCur.ProcDescript+", Procedures automatically set complete due to appt being set complete",AptCur.AptNum,logSource,
						AptOld.DateTStamp);
				}
				else {
					SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,pat.PatNum,AptCur.AptDateTime.ToShortDateString()
						+", "+AptCur.ProcDescript+", Procedures automatically set complete due to appt being set complete",AptCur.AptNum,logSource,
						AptOld.DateTStamp);
				}
				return true;
			}
			return false;
		}

		///<summary>Sets all procedures for apt complete.  Flags procedures as CPOE as needed (when prov logged in).  Makes a log entry for each completed proc.</summary>
		public static List<Procedure> SetCompleteInAppt(Appointment apt,List<InsPlan> PlanList,List<PatPlan> patPlans,long siteNum,int patientAge,List<InsSub> subList,
			bool removeCompletedProcs) 
		{
			//Get all procs attached to the appointment and go through the set complete logic.
			//We must go through all procedures. Remove completed procs if removeCompletedProcs is set to true. We don't want to change completed procedures 
			//unless user wants/has permissions.The permission check should be done before calling this method.
			List<Procedure> listProcsInAppt=GetProcsForSingle(apt.AptNum,false);
			if(removeCompletedProcs) {
				//Remove already completed procedures.
				listProcsInAppt.RemoveAll(x => x.ProcStatus==ProcStat.C);
			}
			if(listProcsInAppt.Count==0) {
				return listProcsInAppt;//Nothing to do.
			}
			List<Procedure> listProcsOld=listProcsInAppt.Select(x => x.Copy()).ToList();
			listProcsInAppt=SetCompleteInAppt(apt,PlanList,patPlans,siteNum,patientAge,listProcsInAppt,subList,Security.CurUser);
			List<Procedure> listProcsCompleted=listProcsInAppt.FindAll(x => listProcsOld.Any(y => y.ProcNum==x.ProcNum && y.ProcStatus!=ProcStat.C));
			listProcsCompleted.ForEach(x => LogProcComplCreate(apt.PatNum,x,x.ToothNum));
			List<Procedure> listProcsAlreadyComplete=listProcsInAppt.FindAll(x => listProcsOld.Any(y => y.ProcNum==x.ProcNum && y.ProcStatus==ProcStat.C));
			foreach(Procedure proc in listProcsAlreadyComplete) {
				Procedure procOld=listProcsOld.FirstOrDefault(x => x.ProcNum==proc.ProcNum);
				LogProcComplEdit(proc,procOld);
			}
			if(Programs.UsingOrion) {
				OrionProcs.SetCompleteInAppt(listProcsInAppt);
			}
			return listProcsInAppt;
		}

		///<summary>Constructs a procedure from a passed-in codenum. Does not prompt you to fill in info like toothNum, etc. 
		///Does NOT insert the procedure into the DB, just returns it.</summary>
		public static Procedure ConstructProcedureForAppt(long codeNum,Appointment appt,Patient pat,List<PatPlan> listPatPlans,
			List<InsPlan> listInsPlans,List<InsSub> linstInsSubs)
		{
			//No need to check RemotingRole; no call to db.
			Procedure proc=new Procedure();
			proc.CodeNum=codeNum;
			proc.PatNum=appt.PatNum;
			proc.ProcDate=DateTime.Today;
			proc.DateTP=proc.ProcDate;
			proc.ToothRange="";
			//surf
			proc.Priority=0;
			proc.ProcStatus=ProcStat.TP;
			ProcedureCode procCodeCur=ProcedureCodes.GetProcCode(proc.CodeNum);
			#region ProvNum
			proc.ProvNum=appt.ProvNum;
			if(procCodeCur.ProvNumDefault!=0) {//Override provider for procedures with a default provider
				//This provider might be restricted to a different clinic than this user.
				proc.ProvNum=procCodeCur.ProvNumDefault;
			}
			else if(procCodeCur.IsHygiene && appt.ProvHyg!=0) {
				proc.ProvNum=appt.ProvHyg;
			}
			#endregion ProvNum
			proc.ClinicNum=appt.ClinicNum;
			proc.MedicalCode=procCodeCur.MedicalCode;
			proc.ProcFee=Procedures.GetProcFee(pat,listPatPlans,linstInsSubs,listInsPlans,proc.CodeNum,proc.ProvNum,proc.ClinicNum,proc.MedicalCode);
			proc.Note=ProcCodeNotes.GetNote(proc.ProvNum,proc.CodeNum,proc.ProcStatus); //get the TP note.
			//dx
			//nextaptnum
			proc.DateEntryC=DateTime.Now;
			proc.BaseUnits=procCodeCur.BaseUnits;
			proc.SiteNum=pat.SiteNum;
			proc.RevCode=procCodeCur.RevenueCodeDefault;
			proc.DiagnosticCode=PrefC.GetString(PrefName.ICD9DefaultForNewProcs);
			proc.PlaceService=(PlaceOfService)PrefC.GetInt(PrefName.DefaultProcedurePlaceService);//Default proc place of service for the Practice is used. 
			if(Userods.IsUserCpoe(Security.CurUser)) {
				//This procedure is considered CPOE because the provider is the one that has added it.
				proc.IsCpoe=true;
			}
			return proc;
		}

		///<summary>Procedure comparer to sort procedures specifically like how they display in the Account Module.  DataRows should be sorted via ProcDate first.</summary>
		private static int ProcedureComparer(DataRow x,DataRow y) {
			return ProcedureLogic.CompareProcedures(x,y);
		}

		//public static ProcExtended GetProcExtendedEntry(Procedure proc,params ProcAttachTypes[] excludedTypes) {
		//	//No need to check RemotingRole; no call to db.
		//	ProcExtended procE = new ProcExtended() {
		//		Proc=proc,
		//		Adjustments=Adjustments.GetForProc(proc.ProcNum),
		//		PaySplits=PaySplits.GetPaySplitsFromProc(proc.ProcNum),
		//		ClaimProcs=ClaimProcs.RefreshForProc(proc.ProcNum),
		//		PayPlanCredits=PayPlanCharges.GetFromProc(proc.ProcNum),
		//		AmountOriginal=proc.ProcFee * (proc.UnitQty + proc.BaseUnits),
		//		ExcludedTypes = excludedTypes.ToList()
		//	};
		//	procE.AmountEnd=procE.AmountStart;
		//	return procE;
		//}

	}

	/*================================================================================================================
	=========================================== class ProcedureComparer =============================================*/

	///<summary>This sorts procedures based on priority, then tooth number, then code (but if Canadian lab code, uses proc code here instead of lab code).  Finally, if comparing a proc and its Canadian lab code, it puts the lab code after the proc.  It does not care about dates or status.  Currently used in TP module only.  The Chart module, Account module, and appointments use Procedurelog.CompareProcedures().</summary>
	public class ProcedureComparer:IComparer {
		///<summary>This sorts procedures based on priority, then tooth number.  It does not care about dates or status.  Currently used in TP module and Chart module sorting.</summary>
		int IComparer.Compare(Object objx,Object objy) {
			Procedure x=(Procedure)objx;
			Procedure y=(Procedure)objy;
			//first, by priority
			if(x.Priority!=y.Priority) {//if priorities are different
				if(x.Priority==0) {
					return 1;//x is greater than y. Priorities always come first.
				}
				if(y.Priority==0) {
					return -1;//x is less than y. Priorities always come first.
				}
				return Defs.GetOrder(DefCat.TxPriorities,x.Priority).CompareTo(Defs.GetOrder(DefCat.TxPriorities,y.Priority));
			}
			//priorities are the same, so sort by toothrange
			if(x.ToothRange!=y.ToothRange) {
				//empty toothranges come before filled toothrange values
				return x.ToothRange.CompareTo(y.ToothRange);
			}
			//toothranges are the same (usually empty), so compare toothnumbers
			if(x.ToothNum!=y.ToothNum) {
				//this also puts invalid or empty toothnumbers before the others.
				return Tooth.ToInt(x.ToothNum).CompareTo(Tooth.ToInt(y.ToothNum));
			}
			//priority and toothnums are the same, so sort by code.
			/*string adaX=x.Code;
			if(x.ProcNumLab !=0){//if x is a Canadian lab proc
				//then use the Code of the procedure instead of the lab code
				adaX=Procedures.GetOneProc(
			}
			string adaY=y.Code;*/
			return ProcedureCodes.GetStringProcCode(x.CodeNum).CompareTo(ProcedureCodes.GetStringProcCode(y.CodeNum));
			//return x.Code.CompareTo(y.Code);
			//return 0;//priority, tooth number, and code are all the same
		}
	}

	///<summary>Helper class that contains properties that give specific results based on data that is currently set.</summary>
	public class ProcExtended {
		private static long _procExtendedAutoIncrementValue = 1;
		///<summary>No matter which constructor is used, the AccountEntryNum will be unique and automatically assigned.</summary>
		public long ProcExtendedEntryNum = (_procExtendedAutoIncrementValue++);
		//Read only data.  Do not modify.
		public Procedure Proc=null;
		//Variables below will be changed as needed.
		public List<Adjustment> Adjustments=new List<Adjustment>();
		public List<PaySplit> PaySplits= new List<PaySplit>();
		public List<ClaimProc> ClaimProcs = new List<ClaimProc>();
		public List<PayPlanCharge> PayPlanCredits = new List<PayPlanCharge>();
		public List<ProcAttachTypes> ExcludedTypes = new List<ProcAttachTypes>();
		public List<PaySplit> SplitsCur = new List<PaySplit>();

		public double NegativeAdjTotals {
			get { return Adjustments.Where(x => x.AdjAmt < 0).Sum(x => x.AdjAmt); }
		}

		public double PositiveAdjTotal {
			get { return Adjustments.Where(x => x.AdjAmt > 0).Sum(x => x.AdjAmt); }
		}

		public double InsPayTotal {
			get { return ClaimProcs.Where(x => x.Status.In(ClaimProcStatus.Received,ClaimProcStatus.Supplemental)).Sum(x => x.InsPayAmt); }
		}

		public double WriteOffTotal {
			get { return ClaimProcs.Where(x => x.Status.In(ClaimProcStatus.Received,ClaimProcStatus.Supplemental)).Sum(x => x.WriteOff); }
		}

		public double WriteOffEstTotal {
			get {
				return ClaimProcs.Where(x => x.Status.In(ClaimProcStatus.NotReceived,ClaimProcStatus.Estimate))
			.Sum(x => (x.WriteOffEstOverride == -1 ? (x.WriteOffEst == -1 ? 0 : x.WriteOffEst) : x.WriteOffEstOverride));
			}
		}

		public double InsEstTotal {
			get {
				return ClaimProcs.Where(x => x.Status.In(ClaimProcStatus.NotReceived,ClaimProcStatus.Estimate))
			.Sum(x => (x.InsEstTotalOverride == -1 ? (x.InsEstTotal == -1 ? 0 : x.InsEstTotal) : x.InsEstTotalOverride));
			}
		}

		public double PaySplitTotal {
			get { return PaySplits.Sum(x => x.SplitAmt); }
		}

		public double SplitsCurTotal {
			get { return SplitsCur.Sum(x => x.SplitAmt); }
		}

		public double PayPlanCreditTotal {
			get { return PayPlanCredits.Sum(x => x.Principal); }
		}

		public double AmountOriginal {
			get {
				return Proc.ProcFee * (Proc.UnitQty + Proc.BaseUnits);
			}
		}

		public double AmountStart {
			get {
				double amt = AmountOriginal;
				if(!ExcludedTypes.Contains(ProcAttachTypes.Adjustments)) {
					amt+=PositiveAdjTotal + NegativeAdjTotals;
				}
				if(!ExcludedTypes.Contains(ProcAttachTypes.InsEsts)) {
					amt-=InsEstTotal;
				}
				if(!ExcludedTypes.Contains(ProcAttachTypes.InsPays)) {
					amt-=InsPayTotal;
				}
				if(!ExcludedTypes.Contains(ProcAttachTypes.PayPlanCredits)) {
					amt-=PayPlanCreditTotal;
				}
				if(!ExcludedTypes.Contains(ProcAttachTypes.PaySplits)) {
					amt-=PaySplitTotal;
				}
				if(!ExcludedTypes.Contains(ProcAttachTypes.WriteOffEsts)) {
					amt-=WriteOffEstTotal;
				}
				if(!ExcludedTypes.Contains(ProcAttachTypes.WriteOffs)) {
					amt-=WriteOffTotal;
				}
				return amt;
			}
		}

		public double AmountEnd {
			get {
				return AmountStart - SplitsCurTotal;
			}
		}

	}

	public enum ProcAttachTypes {
		PaySplits,
		Adjustments,
		InsPays,
		InsEsts,
		WriteOffs,
		WriteOffEsts,
		PayPlanCredits,
	}

	public enum CreditCalcType {
		///<summary>Used to be called 'FIFO'.</summary>
		IncludeAll,
		///<summary>Used to be called 'ExplicitOnly'.</summary>
		AllocatedOnly,
		ExcludeAll
	}

}
