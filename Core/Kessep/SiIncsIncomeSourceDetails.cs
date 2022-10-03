// Program: SI_INCS_INCOME_SOURCE_DETAILS, ID: 371763080, model: 746.
// Short name: SWEINCSP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INCS_INCOME_SOURCE_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes details about an income source of a
/// CSE PERSON.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIncsIncomeSourceDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCS_INCOME_SOURCE_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIncsIncomeSourceDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIncsIncomeSourceDetails.
  /// </summary>
  public SiIncsIncomeSourceDetails(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	  Author	Reason
    // 01/22/96  Lewis		Initial Development
    // 05/02/96  Rao		Changes to Import
    // 			View and Menu Transfer
    // 07/26/96  Mike Ramirez	Added Print Function
    // 09/10/96  G. Lofton 	Unemployment ind moved from
    // 			income source to cse person.
    // 11/03/96  G. Lofton 	Add new security and removed old.
    // 12/25/96  Raju		Event Insertion
    // 01/21/97  Raju		next tran logic
    // 01/23/97  Raju		close monitored document
    // 03/24/97  Sid           Add changes to UCI and Fed Ind.
    // 04/10/97  Sid		Cleanup and problemn reports.
    // 04/29/97  JeHoward      Current date fix.
    // 07/02/97  Sid		Unprotect fields after prompt.
    // 11/19/97  Sid		Fix for Outdoc Ret addr not
    //                         being set for Add.
    // 10/07/98  W. Campbell   Modified 3 ESCAPES so that
    //                         they escaped all the way out of
    //                         the AB instead of only part of the
    //                         way out.
    // 10/08/98 W. Campbell    USE statements for DOCUMENT
    //                         handling were removed from the procedure
    //                         in order to let Mike Remeriz have them for
    //                         rework.  These USE statements will have
    //                         to be re-inserted into the logic when he
    //                         has finished his work.
    // 10/09/98  W. Campbell   Removed and replaced
    //                         LAST QTR field on the screen to fix
    //                         a consistency check error dealing with
    //                         this field on the screen.
    // 10/12/98  W. Campbell   Code added to fix protected
    //                         field problems.
    // 10/13/98  W. Campbell   Added code to validate that
    //                         End Date is not in the future.
    // 10/15/98  W. Campbell   Modified a case stmt to use
    //                         the import view of income_source
    //                         type rather than the export view.
    //                         This was to fix a problem associated
    //                         with the use of the 'clear' command
    //                         which ultimately caused the export
    //                         view of income source to be set to
    //                         defaults.
    // 10/16/98 W. Campbell    Added an IF and Read stmt in CAB
    //                         SI_READ_INCOME_SOURCE_DETAILS
    //                         for CSE_PERSON_RESOURCE to obtain
    //                         the resource_no.
    // 10/17/98  W. Campbell   Added logic to validate that
    //   thru                  most address information is input
    // 10/20/98                for type 'O' or type 'R' income.
    // 10/20/98  W. Campbell   Fixed problems dealing with
    //                         non_empl_income_source_address
    //                         and type 'O' or type 'R' income
    //                         inside CAB
    //                         SI_INCS_ASSOC_INCOME_SOURCE_ADDR.
    // 10/20/98 W. Campbell    Fixed problems in order to get
    //                         ADD logic to work correctly
    //                         for type 'O' and type 'R' income.
    // 10/21/98 W. Campbell    New Code added to keep
    //                         from ADDing more than one
    //                         income source for
    //                         types 'E', 'M' and 'R'.
    // 12/30/1998	M Ramirez	Revised print process.
    // 12/30/1998	M Ramirez	Changed security to check CRUD actions only.
    // -----------------------------------------------------------
    // 01/04/99  W. Campbell   Disabled code to
    //                         validate that End Date
    //                         is not in the future.
    //                         i.e. End Date can now be
    //                         a future date.
    // ---------------------------------------------
    // 02/06/1999  M Ramirez	Added creation of document trigger.
    // ---------------------------------------------
    // 02/18/99 W.Campbell     Replaced use of
    //                         DATENUM(0) with view
    //                         local_default date_work_area date.
    // ---------------------------------------------
    // 02/18/99 M Ramirez & W.Campbell
    //         Disabled statements
    //         dealing with closing monitored documents,
    //         as it has been determined that the best way
    //         to handle them will be in Batch.
    // ---------------------------------------------
    // 05/11/99 W.Campbell       Added a
    //                           USE eab_rollback_cics
    //                           statement to rollback
    //                           any updates if the document
    //                           software has a problem.
    // --------------------------------------
    // 05/20/99 W.Campbell       Replaced
    //                           ZDelete exit states.
    // ---------------------------------------------
    // 05/20/99 W.Campbell       Added logic to process
    //                           interstate events for Employer
    //                           confirmed (verified)  (LSCEM).
    // ---------------------------------------------
    // 05/20/99 W.Campbell       Inserted logic to save
    //                           and restore the exit state
    //                           when dealing with
    //                           interstate processing.
    // ---------------------------------------------
    // 06/04/99 M.Lachowicz      Verify that all screen dates are not
    //                           greater than max date.
    // ---------------------------------------------
    // 06/14/99 M.Lachowicz      Change rules for sending letter
    //                           when paternity is issued and when
    //                           paternity is not issued.
    //                           Employment verification letter
    //                           requirements for INCS:
    //                           It depends on whether an obligation
    //                           exist:
    //                           If the person is an AP (currently active
    //                           or not) on any case then check to see
    //                           if the "is obligated flag" is = "Y"or "N"
    //                           (this is at the case unit),  if it is
    //                           then send EMPVERCO.
    //                           Otherwise, send EMPVERIF.
    // ---------------------------------------------
    // 10/20/99 M.Lachowicz      Change code to be able to send
    //                           an employment letter on a verified
    //                           employer without removing
    //                           'Return Date' field that existed
    //                           prevoiusly.
    //                           PR# 77436.
    // ---------------------------------------------
    // 10/25/99 M.Lachowicz      Select case when you send
    //                           an employment letter.
    //                           PR# 74954.
    //                           Fixed another problem which caused that
    //                           sometimes employment letter was not
    //                           generated.
    // ---------------------------------------------
    // *** November 17, 1999	  David Lowry. PR80543.  Made the select char error
    // with an underscore.
    // ----------------------------------------------
    // 01/06/2000 M Ramirez      83300	NEXT TRAN needs to be cleared before
    // 			invoking print process
    // ---------------------------------------------
    // 02/24/2000 M Lachowicz    86751	Sent date needs to
    //                           be current date.
    // ---------------------------------------------
    // 02/29/2000 M Lachowicz    87961	Display case
    //                           numbers when control comes from INCL.
    // 08/31/00 W.Campbell       Modified NEXTTRAN
    //                           logic so that a 'normal' nexttran coming
    //                           from HIST or MONA (CICS trancodes
    //                           SRPT and SRPU) will work correctly.
    //                           Work done on WR #000193-A.
    // 11/15/00   SWSRPRM	WO # 246; sending alerts to legal providers for locate
    // events
    // -------------------------------------------------------------------------
    // 12/28/2000 M Lachowicz    PR109556	Do not trigger
    //                           an Employer verification letter
    //                           when Type = M, O or R.
    // ---------------------------------------------
    // 01/25/01 SWSRCHF 000238 Return Super NEXTTRAN to ALRT
    // ---------------------------------------------
    // 02/21/01  GVandy	WR187 - Generate Automatic IWOs
    // -----------------------------------------------------------------------------------------------
    // 05/18/01  GVandy	PR 120 - Do not send auto IWO on an update
    // 			if the return code and end date are not changed.
    // -------------------------------------------------------------------------------------
    // 06/04/2001 M Lachowicz    PR1113085 PF11
    //                           does not clear work.
    // -------------------------------------------------------------------------------------
    // 06/04/01  GVandy	PR 116636 - Make area code mandatory if contact or fax 
    // number is entered.
    // 06/04/01  GVandy	PR 118131 - Only allow an Add if the person has an AP or
    // AR case role.
    // 				    On an Update, do not allow the send date to be changed if
    // 				    the person does not have and AP or AR case role.
    // 06/04/01  GVandy	WR 10352 - Do not allow end date with return codes E, R,
    // and A.
    // --------------------------------------------------------------------------------------
    // 01/03/02       Vithal Madhira              PR# 121249 Segment-D
    // Fixed the code for Family violence. If the CSE_Person has 
    // family_violence, do not display the locate (address)  and employer info.
    // --------------------------------------------------------------------------------------
    // 03/18/02  GVandy	PR 136217 - Cancel pending auto IWOs when type code/
    // return code combination
    // 				    is changed.
    // --------------------------------------------------------------------------------------
    // 06/06/02  KDoshi	PR 147570  - Also create HIST record for Retired 
    // Military.
    // --------------------------------------------------------------------------------------
    // 07/08/02  MLachowicz	PR 146759  - Fixed porblem of INCS
    //                         without case number.
    // --------------------------------------------------------------------------------------
    // 02/25/2005 M.J. Quinn  PR 235461.  Don't read each 
    // CASE_UNIT.  Just read one CASE_UNIT
    // 
    // looking for the specific AP and case number.  Also
    // moved code so that it followed the
    // assigning of sp_doc_key
    // key_case.
    // --------------------------------------------------------------------------------------
    // 12/03/2007  LSS  PR#169424  CQ383   Added edit for Start Date and End -
    //                                     
    // can not be 6 months greater than
    // current date.
    // --------------------------------------------------------------------------------------
    // 09/05/2008  Arun Mathias   CQ#493 Force user to select the case number, 
    // if they want
    //                                   
    // to print a document
    // 11/01/2011  RMathews  CQ30191  Revise employer verification letter rules.
    // 1) Read active case
    //                                
    // units and if any have N or Y '
    // is obligated' state, generate
    // EMPVERCO.
    //                                
    // If there are only U's, generate
    // EMPVERIF.  2) If there are no
    // active
    //                                
    // case units, read the inactive
    // records for N or Y obligated
    // state,
    //                                
    // and if found generate EMPVERCO.
    // Otherwise generate EMPVERIF.
    // -------------------------------------------------------------------------------------
    // 08/02/18  GVandy	CQ61457		Update SVES and 'O' type employer to work
    // 					with eIWO for SSA.
    local.Current.Date = Now().Date;

    // 12/03/2007  LSS  PR#169424 CQ383   Added SET statement
    local.TodayPlus6Months.Date = Now().Date.AddMonths(6);
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();

    // ---------------------------------------------
    // 02/18/99 W.Campbell - Disabled statements
    // dealing with closing monitored documents,
    // as it has been determined that the best way
    // to handle them will be in Batch.
    // ---------------------------------------------
    local.Max.Date = new DateTime(2099, 12, 31);

    // 06/04/01 M.L Start
    if (Equal(global.Command, "CLEAR") && IsEmpty(import.IncomeSource.Name))
    {
      return;
    }

    // 06/04/01 M.L End
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Next.Number = import.Next.Number;
    export.CsePerson.Assign(import.CsePerson);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    // ***** CQ#493 Changes Begin Here *****
    export.PrintSelectedFor.Number = import.PrintSelectedFor.Number;

    // ***** CQ#493 Changes End   Here *****
    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Detail.Number = import.Import1.Item.Detail.Number;

      // 10/25/99 M.L Start
      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;

      var field = GetField(export.Export1.Item.Common, "selectChar");

      field.Protected = false;

      // 10/25/99 M.L End
    }

    import.Import1.CheckIndex();
    export.Standard.Assign(import.Standard);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.ScreenPrevCommand.Command = import.ScreenPrevCommand.Command;
    export.IncomeSource.Assign(import.IncomeSource);
    export.Employer.Assign(import.Employer);
    export.EmployerAddress.Note = import.EmployerAddress.Note;
    export.HiddenEmployer.Identifier = import.HiddenEmployer.Identifier;
    export.CsePersonResource.ResourceNo = import.CsePersonResource.ResourceNo;
    export.NonEmployIncomeSourceAddress.Assign(
      import.NonEmployIncomeSourceAddress);
    export.Fax.Assign(import.Fax);
    export.Phone.Assign(import.Phone);
    export.PersonPrompt.SelectChar = import.PersonPrompt.SelectChar;
    export.IncSrcTypePrompt.SelectChar = import.IncSrcTypePrompt.SelectChar;
    export.ReturnCdPrompt.SelectChar = import.ReturnCdPrompt.SelectChar;
    export.StatePrompt.SelectChar = import.StatePrompt.SelectChar;
    export.OtherCodePrompt.SelectChar = import.OtherCodePrompt.SelectChar;
    export.SendToPrompt.SelectChar = import.SendToPrompt.SelectChar;

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    export.HiddenIncomeSource.Assign(import.HiddenIncomeSource);

    // *** Work request 000238
    // *** 01/25/01 swsrchf
    export.Incl.Text4 = import.Incl.Text4;

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKey.Number =
        import.HiddenPageKeys.Item.HiddenPageKey.Number;
    }

    import.HiddenPageKeys.CheckIndex();
    export.CsePerson.Number = export.CsePersonsWorkSet.Number;
    export.ToMenu.Number = export.CsePersonsWorkSet.Number;

    // ---------------------------------------------
    // Start of code : Raju Dec 25,1996 1400 hrs CST
    // 
    // ---------------------------------------------
    export.LastReadHidden.Assign(import.LastReadHidden);

    // ---------------------------------------------
    // End   of code
    // ---------------------------------------------
    if (export.HiddenStandard.PageNumber == 0)
    {
      export.HiddenStandard.PageNumber = 1;
    }

    if (IsEmpty(export.ScreenPrevCommand.Command))
    {
      export.ScreenPrevCommand.Command = "BLANKS";
    }

    // ------------------------------------------------------------
    // If the person number has changed and the screen state is
    // not BLANK, set the screen state to BLANK to initialize the
    // screen.
    // ------------------------------------------------------------
    if (!Equal(export.CsePersonsWorkSet.Number,
      export.HiddenCsePersonsWorkSet.Number) && !
      Equal(export.ScreenPrevCommand.Command, "BLANKS"))
    {
      export.ScreenPrevCommand.Command = "BLANKS";
      export.HiddenCsePersonsWorkSet.Number = "";
      MoveCsePersonsWorkSet(local.BlankCsePersonsWorkSet,
        export.CsePersonsWorkSet);
      export.Export1.Count = 0;
      export.HiddenPageKeys.Count = 0;
      MoveEmployer(local.BlankEmployer, export.Employer);
      export.EmployerAddress.Note = local.BlankEmployerAddress.Note;
      export.IncomeSource.Assign(local.BlankIncomeSource);
      export.Fax.Assign(local.BlankIncomeSourceContact);
      MoveIncomeSourceContact1(local.BlankIncomeSourceContact, export.Phone);
      export.NonEmployIncomeSourceAddress.Assign(
        local.BlankNonEmployIncomeSourceAddress);
      export.CsePersonResource.ResourceNo =
        local.BlankCsePersonResource.ResourceNo;
    }

    if (Equal(global.Command, "RETEMPL"))
    {
      // ------------------------------------------------------------
      // If an employer was selected from EMPL, display the employer
      // and clear other fields.
      // ------------------------------------------------------------
      if (import.SelectedEmployer.Identifier > 0)
      {
        export.Employer.Assign(import.SelectedEmployer);
        export.HiddenEmployer.Identifier = export.Employer.Identifier;
        export.IncomeSource.Assign(local.BlankIncomeSource);
        export.Fax.Assign(local.BlankIncomeSourceContact);
        MoveIncomeSourceContact1(local.BlankIncomeSourceContact, export.Phone);
        export.IncomeSource.Name = import.SelectedEmployer.Name ?? "";
        export.Phone.AreaCode =
          import.SelectedEmployer.AreaCode.GetValueOrDefault();
        export.Phone.Number =
          (int?)StringToNumber(import.SelectedEmployer.PhoneNo);
        export.NonEmployIncomeSourceAddress.Street1 =
          import.SelectedEmployerAddress.Street1 ?? "";
        export.NonEmployIncomeSourceAddress.Street2 =
          import.SelectedEmployerAddress.Street2 ?? "";
        export.NonEmployIncomeSourceAddress.City =
          import.SelectedEmployerAddress.City ?? "";
        export.NonEmployIncomeSourceAddress.State =
          import.SelectedEmployerAddress.State ?? "";
        export.NonEmployIncomeSourceAddress.ZipCode =
          import.SelectedEmployerAddress.ZipCode ?? "";
        export.NonEmployIncomeSourceAddress.Zip4 =
          import.SelectedEmployerAddress.Zip4 ?? "";

        if (ReadEmployerAddress())
        {
          export.EmployerAddress.Note = entities.EmployerAddress.Note;
        }

        // ------------------------------------------
        // If the Income Source Address is from
        // Employer, protect from update.
        // ------------------------------------------
        var field1 = GetField(export.IncomeSource, "name");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.NonEmployIncomeSourceAddress, "street1");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.NonEmployIncomeSourceAddress, "street2");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.NonEmployIncomeSourceAddress, "city");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.NonEmployIncomeSourceAddress, "state");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.StatePrompt, "selectChar");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.NonEmployIncomeSourceAddress, "zipCode");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.NonEmployIncomeSourceAddress, "zip4");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.IncomeSource, "type1");

        field9.Protected = false;
        field9.Focused = true;

        if (Equal(export.Employer.EiwoEndDate, local.Max.Date))
        {
          export.Employer.EiwoEndDate = local.NullDateWorkArea.Date;
        }

        return;
      }
    }

    // ------------------------------------------
    // If the Income Source Address is from
    // Employer, protect from update.
    // ------------------------------------------
    if (export.Employer.Identifier != 0)
    {
      var field1 = GetField(export.IncomeSource, "name");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.NonEmployIncomeSourceAddress, "street1");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.NonEmployIncomeSourceAddress, "street2");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.NonEmployIncomeSourceAddress, "city");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.NonEmployIncomeSourceAddress, "state");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.StatePrompt, "selectChar");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.NonEmployIncomeSourceAddress, "zipCode");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.NonEmployIncomeSourceAddress, "zip4");

      field8.Color = "cyan";
      field8.Protected = true;
    }

    // ------------------------------------------------------
    // 10/12/98 W. Campbell - Code added to fix protected field
    // problems.
    // ------------------------------------------------------
    if (AsChar(export.IncomeSource.Type1) == 'E' || AsChar
      (export.IncomeSource.Type1) == 'M' || AsChar
      (export.IncomeSource.Type1) == 'R' || AsChar
      (export.IncomeSource.Type1) == 'O')
    {
      var field1 = GetField(export.IncomeSource, "type1");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.IncSrcTypePrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      if (AsChar(export.IncomeSource.Type1) != 'O')
      {
        var field3 = GetField(export.IncomeSource, "code");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.OtherCodePrompt, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;
      }
    }

    // ------------------------------------------------------
    // 10/12/98 W. Campbell - End of code added to fix
    // protected field problems.
    // ------------------------------------------------------
    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
      export.CsePersonsWorkSet.Number =
        export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      UseCabZeroFillNumber2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      // Start of Code (Raju 01/20/97:1035 hrs CST)
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        // -------------------------------------------------------------------------
        // 08/31/00 W.Campbell - Modified NEXTTRAN
        // logic so that a 'normal' nexttran coming
        // from HIST or MONA (CICS trancodes SRPT
        // and SRPU) will work correctly.
        // Work done on WR #000193-A.
        // -------------------------------------------------------------------------
        if (export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault() == 0)
        {
          goto Test1;
        }

        // -------------------------------------------------------------------------
        // 08/31/00 W.Campbell - End of Modified NEXTTRAN
        // logic so that a 'normal' nexttran coming
        // from HIST or MONA (CICS trancodes SRPT
        // and SRPU) will work correctly.
        // Work done on WR #000193-A.
        // -------------------------------------------------------------------------
        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.CsePersonsWorkSet.Number = local.LastTran.CsePersonNumber ?? Spaces
          (10);
        export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.IncomeSource.Identifier = local.LastTran.DenormTimestamp;
      }

Test1:

      // End  of Code
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.CsePersonsWorkSet.Number = import.FromMenu.Number;
      global.Command = "DISPLAY";
    }

    UseCabZeroFillNumber1();

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    // mjr
    // ---------------------------------------------
    // 12/30/1998
    // Changed security to check CRUD actions only
    // ----------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ------------------------------------------
    // If control is returned from a list screen,
    // populate the appropriate fields.
    // ------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.PersonPrompt.SelectChar) == 'S')
      {
        export.PersonPrompt.SelectChar = "";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.IncSrcTypePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          export.IncomeSource.Type1 = import.SelectedCodeValue.Cdvalue;
        }

        export.IncSrcTypePrompt.SelectChar = "";

        var field = GetField(export.IncomeSource, "type1");

        field.Protected = false;
        field.Focused = true;

        return;
      }

      if (AsChar(export.OtherCodePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          export.IncomeSource.Code = import.SelectedCodeValue.Cdvalue;
        }

        export.OtherCodePrompt.SelectChar = "";

        var field = GetField(export.IncomeSource, "code");

        field.Protected = false;
        field.Focused = true;

        return;
      }

      if (AsChar(export.StatePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          export.NonEmployIncomeSourceAddress.State =
            import.SelectedCodeValue.Cdvalue;
        }

        export.StatePrompt.SelectChar = "";

        var field = GetField(export.NonEmployIncomeSourceAddress, "state");

        field.Protected = false;
        field.Focused = true;

        return;
      }

      if (AsChar(export.SendToPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          export.IncomeSource.SendTo = import.SelectedCodeValue.Cdvalue;
        }

        export.SendToPrompt.SelectChar = "";

        var field = GetField(export.IncomeSource, "sendTo");

        field.Protected = false;
        field.Focused = true;

        return;
      }

      if (AsChar(export.ReturnCdPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          export.IncomeSource.ReturnCd = import.SelectedCodeValue.Cdvalue;
        }

        export.ReturnCdPrompt.SelectChar = "";

        var field = GetField(export.IncomeSource, "returnCd");

        field.Protected = false;
        field.Focused = true;

        return;
      }
    }

    if (Equal(global.Command, "CREATE") && !
      Equal(export.ScreenPrevCommand.Command, "DISPLAY") && !
      Equal(export.ScreenPrevCommand.Command, "BLANKS"))
    {
      ExitState = "ACO_NE0000_DISPLAY_FIRST";

      return;
    }

    if (Equal(global.Command, "UPDATE") && !
      Equal(export.ScreenPrevCommand.Command, "DISPLAY"))
    {
      ExitState = "ACO_NE0000_DISPLAY_FIRST";

      return;
    }

    // -------------------------------
    // Perform Common Validation
    // -------------------------------
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      if (ReadCsePerson())
      {
        ReadCaseRole();

        if (Equal(global.Command, "CREATE"))
        {
          // -----------------------------------------------------------------------------------------------
          // 06/04/01 PR118131  GVandy Cannot add an income source for a person 
          // without an active AP or AR case role.
          // -----------------------------------------------------------------------------------------------
          if (local.NumberOfApAndArRoles.Count == 0)
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "SI0000_MUST_HAVE_AP_OR_AR_ROLE";

            return;
          }
        }
      }
      else
      {
        // -- Continue.  This error will be taken care of in the remaining 
        // edits.
      }

      // ---------------------------
      // Validate Income Source Type
      // ---------------------------
      if (IsEmpty(export.IncomeSource.Type1))
      {
        var field = GetField(export.IncomeSource, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }
      else
      {
        local.Code.CodeName = "INCOME SOURCE TYPE";
        local.CodeValue.Cdvalue = export.IncomeSource.Type1;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidValue.Flag) == 'N')
        {
          var field = GetField(export.IncomeSource, "type1");

          field.Error = true;

          ExitState = "SI0000_INVALID_INCOME_SOURCE_TYP";

          return;
        }
      }

      // --------------------------------------------------
      // If this is an Employer or Military Income Source,
      // ensure that an Employer Address has been selected.
      // If an Employer Address has been selected, ensure
      // that the type is Employer or Military.
      // --------------------------------------------------
      if (AsChar(export.IncomeSource.Type1) == 'E' || AsChar
        (export.IncomeSource.Type1) == 'M')
      {
        if (export.Employer.Identifier == 0)
        {
          var field = GetField(export.IncomeSource, "name");

          field.Error = true;

          ExitState = "SI0000_EMPLOYER_MUST_BE_SELECTED";

          return;
        }
      }
      else if (AsChar(export.IncomeSource.Type1) == 'O' && Equal
        (export.IncomeSource.Code, "SA"))
      {
        // 08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to work 
        // with eIWO for SSA.
        // For Type O, Code SA, must select the main Social Security 
        // Administration employer, EIN 070000000.
        if (export.Employer.Identifier == 0)
        {
          var field = GetField(export.IncomeSource, "name");

          field.Error = true;

          ExitState = "SI0000_MUST_SELECT_SSA_EMPLOYER";

          return;
        }
        else if (!ReadEmployer())
        {
          var field = GetField(export.IncomeSource, "name");

          field.Error = true;

          ExitState = "SI0000_MUST_SELECT_SSA_EMPLOYER";

          return;
        }
      }
      else
      {
        if (export.Employer.Identifier > 0)
        {
          var field = GetField(export.IncomeSource, "type1");

          field.Error = true;

          ExitState = "SI0000_INVALID_TYPE_FOR_EMPLOYER";

          return;
        }

        // -------------------------------------------
        // 10/17/98 W. Campbell  The following validation
        // logic was added.
        // -------------------------------------------
        // -------------------------------------------
        // Validate common items for Resource and Other,
        // (income_source type 'R' and type 'O').
        // -------------------------------------------
        // -----------------------------------
        // Validate that an income_source name
        // has been entered.
        // -----------------------------------
        if (!IsEmpty(export.IncomeSource.Name))
        {
          // -----------------------------------
          // All OK, keep on going.
          // -----------------------------------
        }
        else
        {
          var field = GetField(export.IncomeSource, "name");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        // -----------------------------------
        // Address validation -  We
        // must have at least street1,
        // city, state and zip code.
        // -----------------------------------
        if (!IsEmpty(export.NonEmployIncomeSourceAddress.Street1))
        {
          // -----------------------------------
          // All OK, keep on going.
          // -----------------------------------
        }
        else
        {
          var field = GetField(export.NonEmployIncomeSourceAddress, "street1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsEmpty(export.NonEmployIncomeSourceAddress.City))
        {
          // -----------------------------------
          // All OK, keep on going.
          // -----------------------------------
        }
        else
        {
          var field = GetField(export.NonEmployIncomeSourceAddress, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsEmpty(export.NonEmployIncomeSourceAddress.State))
        {
          // -----------------------------------
          // All OK, keep on going.
          // STATE code will be validated later.
          // -----------------------------------
        }
        else
        {
          var field = GetField(export.NonEmployIncomeSourceAddress, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        // -----------------------------------
        // Validate that the 5 digit part of
        // ZIP CODE is present and is
        // numeric.
        // -----------------------------------
        if (Verify(export.NonEmployIncomeSourceAddress.ZipCode, "0123456789") >
          0)
        {
          var field = GetField(export.NonEmployIncomeSourceAddress, "zipCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsEmpty(export.NonEmployIncomeSourceAddress.ZipCode))
        {
          for(local.CheckZip.Count = 1; local.CheckZip.Count <= 5; ++
            local.CheckZip.Count)
          {
            local.CheckZip.Flag =
              Substring(export.NonEmployIncomeSourceAddress.ZipCode,
              local.CheckZip.Count, 1);

            if (AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              var field =
                GetField(export.NonEmployIncomeSourceAddress, "zipCode");

              field.Error = true;

              ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
            }
          }
        }

        // -----------------------------------
        // Validate that if the 4 digit part of
        // ZIP CODE is entered then it must
        // be numeric.
        // -----------------------------------
        if (!IsEmpty(export.NonEmployIncomeSourceAddress.Zip4))
        {
          if (IsEmpty(export.NonEmployIncomeSourceAddress.ZipCode))
          {
            var field =
              GetField(export.NonEmployIncomeSourceAddress, "zipCode");

            field.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          if (Verify(export.NonEmployIncomeSourceAddress.Zip4, "0123456789") > 0
            )
          {
            // -----------------------------------
            // Validate that if the 4 digit part of
            // ZIP CODE is entered then it must
            // be numeric.
            // -----------------------------------
            var field =
              GetField(export.NonEmployIncomeSourceAddress, "zipCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          for(local.CheckZip.Count = 1; local.CheckZip.Count <= 4; ++
            local.CheckZip.Count)
          {
            local.CheckZip.Flag =
              Substring(export.NonEmployIncomeSourceAddress.Zip4,
              local.CheckZip.Count, 1);

            if (AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              var field = GetField(export.NonEmployIncomeSourceAddress, "zip4");

              field.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------------
        // 10/17/98 W. Campbell  End of validation
        // logic added.
        // -------------------------------------------
        // --------------
        // Validate State
        // --------------
        if (!IsEmpty(export.NonEmployIncomeSourceAddress.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue =
            export.NonEmployIncomeSourceAddress.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidValue.Flag) == 'N')
          {
            var field = GetField(export.NonEmployIncomeSourceAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
        }
      }

      if (!IsEmpty(export.Phone.EmailAddress))
      {
        local.EmailVerify.Count = 0;

        do
        {
          if (local.CurrentPosition.Count >= 60)
          {
            break;
          }

          local.Postion.Text1 =
            Substring(export.Phone.EmailAddress, local.CurrentPosition.Count, 1);
            

          if (AsChar(local.Postion.Text1) == '@')
          {
            if (local.CurrentPosition.Count <= 1)
            {
              var field = GetField(export.Phone, "emailAddress");

              field.Error = true;

              ExitState = "INVALID_EMAIL_ADDRESS";

              return;
            }

            local.EmailVerify.Count = local.CurrentPosition.Count + 5;

            if (IsEmpty(Substring(
              export.Phone.EmailAddress, local.EmailVerify.Count, 1)))
            {
              var field = GetField(export.Phone, "emailAddress");

              field.Error = true;

              ExitState = "INVALID_EMAIL_ADDRESS";

              return;
            }

            break;
          }

          ++local.CurrentPosition.Count;
        }
        while(!Equal(global.Command, "COMMAND"));

        if (local.EmailVerify.Count <= 0)
        {
          var field = GetField(export.Phone, "emailAddress");

          field.Error = true;

          ExitState = "INVALID_EMAIL_ADDRESS";

          return;
        }
      }

      // ---------------------------------
      // Validate Other Income Source Code
      // ---------------------------------
      if (!IsEmpty(export.IncomeSource.Code))
      {
        // ------------------
        // Type must be Other
        // ------------------
        if (AsChar(export.IncomeSource.Type1) == 'O')
        {
          export.NonEmployIncomeSourceAddress.LocationType = "D";
        }
        else
        {
          var field = GetField(export.IncomeSource, "code");

          field.Error = true;

          ExitState = "SI0000_CODE_VALID_FOR_OTHER_TYPE";

          return;
        }

        local.Code.CodeName = "INCOME SOURCE OTHER INCOME";
        local.CodeValue.Cdvalue = export.IncomeSource.Code ?? Spaces(10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidValue.Flag) == 'N')
        {
          var field = GetField(export.IncomeSource, "code");

          field.Error = true;

          ExitState = "SI0000_INVALID_OTHER_INC_SRC_COD";

          return;
        }
      }
      else if (AsChar(export.IncomeSource.Type1) == 'R')
      {
        export.NonEmployIncomeSourceAddress.LocationType = "D";
      }
      else
      {
        // -----------------------
        // Required for Other Type
        // -----------------------
        if (AsChar(export.IncomeSource.Type1) == 'O')
        {
          var field = GetField(export.IncomeSource, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
      }

      // ------------------
      // Validate Send Date
      // ------------------
      if (Equal(global.Command, "UPDATE"))
      {
        if (!Equal(export.IncomeSource.SentDt, export.HiddenIncomeSource.SentDt))
          
        {
          // -----------------------------------------------------------------------------------------------
          // 06/04/01 PR118131  GVandy Cannot generate an employer verification 
          // letter for a person
          // without an active AP or AR case role.
          // -----------------------------------------------------------------------------------------------
          if (local.NumberOfApAndArRoles.Count == 0 && entities
            .CsePerson.Populated)
          {
            var field1 = GetField(export.CsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.IncomeSource, "sentDt");

            field2.Error = true;

            ExitState = "SI0000_MUST_HAVE_AP_OR_AR_ROLE_2";

            return;
          }
        }
      }

      local.SendDateUpdated.Flag = "N";

      // 02/24/00 M.L Start
      if (Lt(local.BlankIncomeSource.SentDt, export.IncomeSource.SentDt) && !
        Equal(export.IncomeSource.SentDt, local.Current.Date) && !
        Equal(export.IncomeSource.SentDt, export.HiddenIncomeSource.SentDt))
      {
        var field = GetField(export.IncomeSource, "sentDt");

        field.Error = true;

        ExitState = "SI0000_SEND_DATE_NOTEQUAL_TO_CU";

        return;
      }

      // 02/24/00 M.L End
      // ------------------------------
      // If Send Date has been changed, set the changed flag.
      // ------------------------------
      // 10/25/99 M.L Start
      // 10/25/99 M.L End
      // ------------------
      // Validate Send To
      // ------------------
      if (!IsEmpty(export.IncomeSource.SendTo))
      {
        if (Equal(export.IncomeSource.SentDt, local.BlankIncomeSource.SentDt))
        {
          var field = GetField(export.IncomeSource, "sentDt");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        local.Code.CodeName = "INCOME SOURCE SEND TO";
        local.CodeValue.Cdvalue = export.IncomeSource.SendTo ?? Spaces(10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidValue.Flag) == 'N')
        {
          var field = GetField(export.IncomeSource, "sendTo");

          field.Error = true;

          ExitState = "SI0000_INVALID_INC_SRC_SEND_TO";

          return;
        }

        if (!Equal(export.IncomeSource.SendTo, export.HiddenIncomeSource.SendTo))
          
        {
          export.IncomeSource.WorkerId = global.UserId;
        }
      }
      else if (!Equal(export.IncomeSource.SentDt, local.BlankIncomeSource.SentDt))
        
      {
        var field = GetField(export.IncomeSource, "sendTo");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      // --------------------
      // Validate Return Date
      // --------------------
      if (!Equal(export.IncomeSource.SentDt, local.BlankIncomeSource.SentDt) &&
        !
        Equal(export.IncomeSource.ReturnDt, local.BlankIncomeSource.ReturnDt) &&
        Lt(export.IncomeSource.ReturnDt, export.IncomeSource.SentDt))
      {
        // 10/20/99 M.L Start
        if (Equal(export.HiddenIncomeSource.ReturnDt,
          export.IncomeSource.ReturnDt))
        {
          goto Test2;
        }

        // 10/20/99 M.L Start
        var field = GetField(export.IncomeSource, "returnDt");

        field.Error = true;

        ExitState = "SI0000_RETURN_DT_LESSTHN_SEND_DT";

        return;
      }
      else
      {
        // -------------------------------------------
        // Return Date cannot be a future date.
        // -------------------------------------------
        if (Lt(local.Current.Date, export.IncomeSource.ReturnDt))
        {
          var field = GetField(export.IncomeSource, "returnDt");

          field.Error = true;

          ExitState = "SI0000_RETURN_DT_CANT_B_FUTUR_DT";

          return;
        }

        // ------------------------------
        // If Return Date has been entered,
        // set Worker Id.
        // ------------------------------
        if (!Equal(export.IncomeSource.ReturnDt,
          export.HiddenIncomeSource.ReturnDt))
        {
          export.IncomeSource.WorkerId = global.UserId;
        }
      }

Test2:

      // 10/25/99 M.L Start
      // 10/25/99 M.L End
      // --------------------
      // Validate Return Code
      // --------------------
      if (!IsEmpty(export.IncomeSource.ReturnCd))
      {
        if (Equal(export.IncomeSource.ReturnDt, local.BlankIncomeSource.ReturnDt))
          
        {
          var field = GetField(export.IncomeSource, "returnDt");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (AsChar(export.IncomeSource.Type1) == 'E')
        {
          local.Code.CodeName = "EMPLOYMENT RETURN";
        }
        else if (AsChar(export.IncomeSource.Type1) == 'M')
        {
          local.Code.CodeName = "MILITARY RETURN";
        }
        else if (AsChar(export.IncomeSource.Type1) == 'R')
        {
          local.Code.CodeName = "RESOURCE RETURN";
        }
        else if (AsChar(export.IncomeSource.Type1) == 'O')
        {
          local.Code.CodeName = "OTHER RETURN";
        }
        else
        {
          var field = GetField(export.ReturnCdPrompt, "selectChar");

          field.Error = true;

          ExitState = "SI0000_NO_RETURN_CODES_FOR_TYPE";

          return;
        }

        local.CodeValue.Cdvalue = export.IncomeSource.ReturnCd ?? Spaces(10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidValue.Flag) == 'N')
        {
          var field = GetField(export.IncomeSource, "returnCd");

          field.Error = true;

          ExitState = "SI0000_INVALID_INC_SRC_RETURN_CD";

          return;
        }

        if (AsChar(export.IncomeSource.ReturnCd) != AsChar
          (export.HiddenIncomeSource.ReturnCd))
        {
          export.IncomeSource.WorkerId = global.UserId;
        }
      }
      else if (!Equal(export.IncomeSource.ReturnDt,
        local.BlankIncomeSource.ReturnDt))
      {
        var field = GetField(export.IncomeSource, "returnCd");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      // ---------------------------------------------------------------
      // 12/03/2007  PR1169424  CQ383   LSS
      // Validate Start Date <= Current Date + 6 months
      // ---------------------------------------------------------------
      if (Lt(local.TodayPlus6Months.Date, export.IncomeSource.StartDt))
      {
        var field = GetField(export.IncomeSource, "startDt");

        field.Error = true;

        ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

        return;
      }

      // -------------------------------
      // Validate End Date >= Start Date
      // -------------------------------
      if (!Equal(export.IncomeSource.StartDt, local.BlankIncomeSource.StartDt) &&
        !Equal(export.IncomeSource.EndDt, local.BlankIncomeSource.EndDt) && Lt
        (export.IncomeSource.EndDt, export.IncomeSource.StartDt))
      {
        var field = GetField(export.IncomeSource, "endDt");

        field.Error = true;

        ExitState = "SI0000_END_DT_LESS_THAN_START_DT";

        return;
      }

      // ------------------------------------------------------------
      // 12/03/2007  PR169424  CQ383  LSS
      // Validate End Date <= Current Date + 6 months
      // ------------------------------------------------------------
      if (Lt(local.TodayPlus6Months.Date, export.IncomeSource.EndDt))
      {
        var field = GetField(export.IncomeSource, "endDt");

        field.Error = true;

        ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

        return;
      }

      if (AsChar(export.IncomeSource.Type1) == 'E' && AsChar
        (export.IncomeSource.ReturnCd) == 'E' || AsChar
        (export.IncomeSource.Type1) == 'M' && (
          AsChar(export.IncomeSource.ReturnCd) == 'A' || AsChar
        (export.IncomeSource.ReturnCd) == 'R'))
      {
        // -----------------------------------------------------------------------------------------------
        // 06/04/01 WR10352	GVandy
        // End date is not allow with return codes E, R, and A.  Commented out 
        // the code below checking
        // End Date >= Current Date and End Date >= Return Date.
        // -----------------------------------------------------------------------------------------------
        if (!Equal(export.IncomeSource.EndDt, local.BlankIncomeSource.EndDt))
        {
          var field1 = GetField(export.IncomeSource, "returnCd");

          field1.Error = true;

          var field2 = GetField(export.IncomeSource, "endDt");

          field2.Error = true;

          ExitState = "SI0000_END_DATE_NOT_ALLOWED";

          return;
        }
      }

      // ---------------------------------------------
      // 01/04/99  W. Campbell - Disabled code to
      // validate that End Date is not in the future.
      // ---------------------------------------------
      // ---------------------------------------------
      // 06/04/99  M.Lachowicz - Check that all screen dates
      // don't have value greater than max date.
      // ---------------------------------------------
      UseCabSetMaximumDiscontinueDate();

      if (Lt(local.MaxDate.Date, export.IncomeSource.EndDt))
      {
        var field = GetField(export.IncomeSource, "endDt");

        field.Error = true;

        ExitState = "ACO_NI0000_INVALID_DATE";

        return;
      }

      if (Lt(local.MaxDate.Date, export.IncomeSource.StartDt))
      {
        var field = GetField(export.IncomeSource, "startDt");

        field.Error = true;

        ExitState = "ACO_NI0000_INVALID_DATE";

        return;
      }

      if (Lt(local.MaxDate.Date, export.IncomeSource.SentDt))
      {
        var field = GetField(export.IncomeSource, "sentDt");

        field.Error = true;

        ExitState = "ACO_NI0000_INVALID_DATE";

        return;
      }

      // -----------------------------------------------------------------------------------------------
      // 06/04/01 PR116636  GVandy Make area code mandatory if contact or fax 
      // number is entered.
      // -----------------------------------------------------------------------------------------------
      if (export.Phone.Number.GetValueOrDefault() != 0 && export
        .Phone.AreaCode.GetValueOrDefault() == 0)
      {
        var field = GetField(export.Phone, "areaCode");

        field.Error = true;

        ExitState = "SI0000_AREA_CODE_REQUIRED";

        return;
      }

      if (export.Fax.Number.GetValueOrDefault() != 0 && export
        .Fax.AreaCode.GetValueOrDefault() == 0)
      {
        var field = GetField(export.Fax, "areaCode");

        field.Error = true;

        ExitState = "SI0000_AREA_CODE_REQUIRED";

        return;
      }

      // ------------------------------------------
      // Set the phone type for each contact number
      // HP for the Phone Number
      // HF for the Fax Number
      // ------------------------------------------
      if (IsEmpty(export.Phone.Type1))
      {
        export.Phone.Type1 = "HP";
      }

      if (IsEmpty(export.Fax.Type1))
      {
        export.Fax.Type1 = "HF";
      }

      // ------------------------------------------
      // Validate the Federal Ind.
      // ------------------------------------------
      if (!IsEmpty(export.CsePerson.FederalInd))
      {
        if (AsChar(export.CsePerson.FederalInd) != 'Y' && AsChar
          (export.CsePerson.FederalInd) != 'N')
        {
          var field = GetField(export.CsePerson, "federalInd");

          field.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          return;
        }
      }
      else
      {
        export.CsePerson.FederalInd = "N";
      }

      // 10/25/99 M.L Start
      if (Equal(global.Command, "UPDATE") && !
        Equal(export.IncomeSource.SentDt, export.HiddenIncomeSource.SentDt) && Lt
        (local.BlankIncomeSource.SentDt, export.IncomeSource.SentDt))
      {
        export.IncomeSource.WorkerId = global.UserId;
        local.SendDateUpdated.Flag = "Y";
      }

      if (Equal(global.Command, "CREATE") && Lt
        (local.BlankIncomeSource.SentDt, export.IncomeSource.SentDt))
      {
        export.IncomeSource.WorkerId = global.UserId;
        local.SendDateUpdated.Flag = "Y";
      }

      // 10/25/99 M.L Start
      if (AsChar(local.SendDateUpdated.Flag) == 'Y')
      {
        if (export.Export1.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            switch(AsChar(export.Export1.Item.Common.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                ++local.SelectionsNumber.Count;
                local.SpDocKey.KeyCase = export.Export1.Item.Detail.Number;

                break;
              default:
                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                return;
            }
          }

          export.Export1.CheckIndex();

          // *** November 17, 1999 David Lowry.  PR80543.  If no selection or 
          // more than 1 selection is made, make the selection char an error.
          switch(local.SelectionsNumber.Count)
          {
            case 0:
              export.Export1.Index = 0;
              export.Export1.CheckSize();

              var field1 = GetField(export.Export1.Item.Common, "selectChar");

              field1.Error = true;

              switch(TrimEnd(global.Command))
              {
                case "UPDATE":
                  ExitState = "ACO_NE0000_NO_SELECTION_UPDATE";

                  break;
                case "CREATE":
                  ExitState = "ACO_NE0000_NO_SELECTION_CREATE";

                  break;
                default:
                  break;
              }

              return;
            case 1:
              // *** This is correct.
              break;
            default:
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;
                }
                else
                {
                }
              }

              export.Export1.CheckIndex();

              export.Export1.Index = 0;
              export.Export1.CheckSize();

              var field2 = GetField(export.Export1.Item.Common, "selectChar");

              field2.Error = true;

              ExitState = "SI0000_SELECT_ONE_CASE_ONLY";

              return;
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.CheckSize();

          local.SpDocKey.KeyCase = export.Export1.Item.Detail.Number;
        }
      }

      // 10/25/99 M.L End
      // 08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to work with
      // eIWO for SSA.
      // Add a confirmation message if start date is left blank.
      if (AsChar(export.IncomeSource.Type1) == 'E' && AsChar
        (export.IncomeSource.ReturnCd) == 'E')
      {
        if (Equal(export.IncomeSource.StartDt, local.NullDateWorkArea.Date))
        {
          if (AsChar(import.StartDateConfirmation.Flag) == 'Y')
          {
            // --User was previously given confirmation message and they chose 
            // to leave the start
            //   date blank.  Default the start date to the current date.
            export.IncomeSource.StartDt = Now().Date;
          }
          else
          {
            export.StartDateConfirmation.Flag = "Y";

            var field = GetField(export.IncomeSource, "startDt");

            field.Error = true;

            switch(TrimEnd(global.Command))
            {
              case "CREATE":
                ExitState = "SI0000_NULL_START_DT_CREATE";

                break;
              case "UPDATE":
                ExitState = "SI0000_NULL_START_DT_UPDATE";

                break;
              default:
                break;
            }

            return;
          }
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "HELP":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "PRINT":
        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenCsePersonsWorkSet.Number) && export
          .Employer.Identifier != export.HiddenEmployer.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
        }
        else
        {
          // ***** CQ#493 Changes Begin Here *****
          // ***** Code is isolated from the batch printing selection *****
          if (export.Export1.Count > 1)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              switch(AsChar(export.Export1.Item.Common.SelectChar))
              {
                case ' ':
                  break;
                case 'S':
                  ++local.SelectionsNumber.Count;
                  export.PrintSelectedFor.Number =
                    export.Export1.Item.Detail.Number;

                  break;
                default:
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                  return;
              }
            }

            export.Export1.CheckIndex();

            switch(local.SelectionsNumber.Count)
            {
              case 0:
                export.Export1.Index = 0;
                export.Export1.CheckSize();

                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                if (Equal(global.Command, "PRINT"))
                {
                  ExitState = "ACO_NE0000_NO_SELECTION_PRINT";
                }
                else
                {
                }

                return;
              case 1:
                // *** This is correct.
                break;
              default:
                for(export.Export1.Index = 0; export.Export1.Index < export
                  .Export1.Count; ++export.Export1.Index)
                {
                  if (!export.Export1.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
                  {
                    var field =
                      GetField(export.Export1.Item.Common, "selectChar");

                    field.Error = true;
                  }
                  else
                  {
                  }
                }

                export.Export1.CheckIndex();

                export.Export1.Index = 0;
                export.Export1.CheckSize();

                var field2 = GetField(export.Export1.Item.Common, "selectChar");

                field2.Error = true;

                ExitState = "SI0000_SELECT_ONE_CASE_ONLY";

                return;
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.CheckSize();

            export.PrintSelectedFor.Number = export.Export1.Item.Detail.Number;
          }

          // ***** CQ#493 Changes End Here *****
          export.DocmProtectFilter.Flag = "Y";
          export.Filter.Type1 = "INCS";
          ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";
        }

        return;
      case "RETDOCM":
        if (IsEmpty(import.Document.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          return;
        }

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before invoking print process
        // -------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.NullNextTranInfo);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + import.Document.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdPrNumber) + export
          .CsePersonsWorkSet.Number;
        local.BatchTimestampWorkArea.IefTimestamp =
          export.IncomeSource.Identifier;
        UseLeCabConvertTimestamp();
        local.Print.Text50 = TrimEnd(local.SpDocLiteral.IdIncomeSource) + local
          .BatchTimestampWorkArea.TextTimestamp;
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(export.HiddenNextTranInfo.MiscText1) + ";" + local
          .Print.Text50;

        // ***** CQ#493 Changes Begin Here *****
        export.HiddenNextTranInfo.CaseNumber = export.PrintSelectedFor.Number;

        // ***** CQ#493 Changes End   Here *****
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // ---------------------------------------------------------
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ----------------------------------------------------
        // Extract identifiers from next tran
        // -------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.CsePersonsWorkSet.Number =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          7, 10);
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdIncomeSource));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchTimestampWorkArea.TextTimestamp =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          5, 26);
        export.IncomeSource.Identifier =
          Timestamp(local.BatchTimestampWorkArea.TextTimestamp);
        global.Command = "DISPLAY";

        break;
      case "RETURN":
        // *** Work request 000238
        // *** 01/25/01 swsrchf
        // *** start
        if (Equal(export.Incl.Text4, "INCS"))
        {
          UseScCabNextTranGet();
        }

        // *** end
        // *** 01/25/01 swsrchf
        // *** Work request 000238
        // *** Work request 000238
        // *** 01/25/01 swsrchf
        // *** added check for 'SRPQ'
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPQ"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        return;
      case "INCH":
        ExitState = "ECO_XFR_TO_INCOME_HISTORY";

        return;
      case "EMPL":
        // 02/29/00 M.L Start
        if (import.HiddenPageKeys.Count == 0)
        {
          export.Export1.Count = 0;
          local.Local1.Count = 0;

          export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
          export.HiddenPageKeys.CheckSize();

          UseSiReadCasesByPerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            // -----------------------------------------------------------
            // 10/07/98  W. Campbell -  Modified the
            // following ESCAPE so that it escapes
            // all the way out of the AB instead of
            // only part of the way out.
            // -----------------------------------------------------------
            return;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            // -----------------------------------------------------------
            // 10/07/98  W. Campbell -  Modified the
            // following ESCAPE so that it escapes
            // all the way out of the AB instead of
            // only part of the way out.
            // -----------------------------------------------------------
            return;
          }
          else
          {
            export.HiddenCsePersonsWorkSet.Number =
              export.CsePersonsWorkSet.Number;
          }

          // 10/25/99 M.L Start
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            export.Export1.Index = local.Local1.Index;
            export.Export1.CheckSize();

            export.Export1.Update.Detail.Number =
              local.Local1.Item.Detail.Number;

            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Protected = false;
          }

          local.Local1.CheckIndex();

          if (local.Local1.Count == 1)
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Protected = true;
          }

          if (!IsEmpty(local.Page.Number))
          {
            export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
            export.HiddenPageKeys.CheckSize();

            export.HiddenPageKeys.Update.HiddenPageKey.Number =
              local.Local1.Item.Detail.Number;
          }
        }

        // 02/29/00 M.L End
        // -------------------------------
        // Employer Selection not allowed
        // for Income Source Type of Other
        // of Resource.
        // -------------------------------
        if ((AsChar(export.IncomeSource.Type1) == 'O' && Equal
          (export.IncomeSource.Code, "SA") || AsChar
          (export.IncomeSource.Type1) != 'O') && AsChar
          (export.IncomeSource.Type1) != 'R')
        {
          ExitState = "ECO_LNK_TO_EMPLOYER";

          return;
        }
        else
        {
          ExitState = "SI0000_EMPL_XFER_NOT_ALLOWD_4_TY";
        }

        break;
      case "RETEMPL":
        break;
      case "LIST":
        // ---------------------------------------------
        // This command allows the user to link to a
        // selection list and retrieve the appropriate
        // value, not losing any of the data already
        // entered.
        // ---------------------------------------------
        switch(AsChar(export.PersonPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.PersonPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.IncSrcTypePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "INCOME SOURCE TYPE";
            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.IncSrcTypePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.OtherCodePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (AsChar(export.IncomeSource.Type1) != 'O')
            {
              var field1 = GetField(export.IncomeSource, "type1");

              field1.Error = true;

              var field2 = GetField(export.OtherCodePrompt, "selectChar");

              field2.Error = true;

              ExitState = "SI0000_PROMPT_VALD_ONLY_WTH_OTHR";

              return;
            }

            export.Prompt.CodeName = "INCOME SOURCE OTHER INCOME";
            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.OtherCodePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.StatePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.StatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.SendToPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "INCOME SOURCE SEND TO";
            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.SendToPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.ReturnCdPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (AsChar(export.IncomeSource.Type1) == 'E')
            {
              export.Prompt.CodeName = "EMPLOYMENT RETURN";
            }
            else if (AsChar(export.IncomeSource.Type1) == 'M')
            {
              export.Prompt.CodeName = "MILITARY RETURN";
            }
            else if (AsChar(export.IncomeSource.Type1) == 'R')
            {
              export.Prompt.CodeName = "RESOURCE RETURN";
            }
            else if (AsChar(export.IncomeSource.Type1) == 'O')
            {
              export.Prompt.CodeName = "OTHER RETURN";
            }
            else
            {
              ++local.InvalidSel.Count;

              var field1 = GetField(export.ReturnCdPrompt, "selectChar");

              field1.Error = true;

              ExitState = "SI0000_NO_RETURN_CODES_FOR_TYPE";

              break;
            }

            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.ReturnCdPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(local.InvalidSel.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            if (AsChar(export.PersonPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PersonPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.IncSrcTypePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.IncSrcTypePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.OtherCodePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.OtherCodePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.StatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.StatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.SendToPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.SendToPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.ReturnCdPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ReturnCdPrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        break;
      case "CREATE":
        // -------------------------------------------------------
        // CREATE CREATE CREATE CREATE CREATE CREATE CREATE CREATE
        // -------------------------------------------------------
        // ---------------------------------------------
        // Start of code : Raju Dec 25, 1996 1410hrs CST
        // 
        // ---------------------------------------------
        // ---------------------------------------------
        // 02/18/99 W.Campbell - Replaced use of
        // DATENUM(0) with view
        // local_default date_work_area date.
        // ---------------------------------------------
        export.LastReadHidden.EndDt = local.Default1.Date;
        export.LastReadHidden.ReturnDt = local.Default1.Date;
        export.LastReadHidden.ReturnCd = "";

        // ---------------------------------------------
        // End   of code
        // ---------------------------------------------
        // ---------------------------------------------
        // 10/21/98 W. Campbell New Code added to
        // keep from adding more than one income
        // source for types 'E', 'M' and 'R'.
        // ---------------------------------------------
        switch(AsChar(export.IncomeSource.Type1))
        {
          case 'E':
            if (export.Employer.Identifier == 0)
            {
              ExitState = "EMPLOYER_NF";

              goto Test6;
            }

            if (ReadIncomeSource2())
            {
              ExitState = "ACTIVE_EMPLOYMENT_ALREADY_EXIST";

              goto Test6;
            }
            else
            {
              // ---------------------------------------------
              // All OK, keep on going.
              // ---------------------------------------------
            }

            break;
          case 'M':
            if (ReadIncomeSource4())
            {
              ExitState = "SI0000_ACTIVE_MILITARY_INCS_EXST";

              goto Test6;
            }

            break;
          case 'R':
            if (export.CsePersonResource.ResourceNo == 0)
            {
              ExitState = "EMPLOYER_NF";

              goto Test6;
            }

            if (ReadIncomeSource3())
            {
              ExitState = "ACTIVE_RESOURCE_ALREADY_EXIST";

              goto Test6;
            }
            else
            {
              // ---------------------------------------------
              // All OK, keep on going.
              // ---------------------------------------------
            }

            break;
          case 'O':
            // ---------------------------------------------
            // Can't do anything abuout this one
            // to prevent duplicates.  Keep on going.
            // ---------------------------------------------
            break;
          default:
            ExitState = "SI0000_INVALID_INCOME_SOURCE_TYP";

            goto Test6;
        }

        // ---------------------------------------------
        // 10/21/98 W. Campbell End of new
        // code added to keep from adding
        // more than one income source for
        // types 'E', 'M' and 'R'.
        // ---------------------------------------------
        // -------------------------------------
        // Create Income Source and all related
        // objects.
        // -------------------------------------
        UseSiIncsCreateIncomeSource();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiIncsEndDtPriorEmployment();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiIncsAssocIncomeSourceAddr();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiIncsCreateIncomeSrcContct2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiIncsCreateIncomeSrcContct1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        // 08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to work 
        // with eIWO for SSA.
        // Call the auto IWO cab for Type O, Code SA, Return code V.  Also call 
        // auto IWO cab for
        // Type O, Code WC, Return code V.
        if (AsChar(export.IncomeSource.Type1) == 'E' && AsChar
          (export.IncomeSource.ReturnCd) == 'E' || AsChar
          (export.IncomeSource.Type1) == 'M' && (
            AsChar(export.IncomeSource.ReturnCd) == 'A' || AsChar
          (export.IncomeSource.ReturnCd) == 'R') || AsChar
          (export.IncomeSource.Type1) == 'O' && (
            Equal(export.IncomeSource.Code, "SA") || Equal
          (export.IncomeSource.Code, "WC")) && AsChar
          (export.IncomeSource.ReturnCd) == 'V')
        {
          if (!Lt(local.Current.Date, export.IncomeSource.EndDt) && !
            Equal(export.IncomeSource.EndDt, local.Default1.Date))
          {
            goto Test3;
          }

          // ----------------------------------------------------------
          // 2/22/01 WR187	GVandy
          // Automatically generate an IWO.
          // ----------------------------------------------------------
          UseLeAutomaticIwoGeneration1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            break;
          }
        }

Test3:

        MoveIncomeSource5(export.IncomeSource, export.HiddenIncomeSource);
        export.ScreenPrevCommand.Command = "DISPLAY";
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // -------------------------------------------------------
        // UPDATE UPDATE UPDATE UPDATE UPDATE UPDATE UPDATE UPDATE
        // -------------------------------------------------------
        UseSiIncsUpdateIncomeSource();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiIncsEndDtPriorEmployment();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiIncsAssocIncomeSourceAddr();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiUpdateIncomeSourceContact2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseSiUpdateIncomeSourceContact1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        // -- 03/18/02  GVandy PR 136217  Cancel pending auto IWOs when type 
        // code/return code combination is changed.
        if (AsChar(export.HiddenIncomeSource.Type1) == 'E' && AsChar
          (export.HiddenIncomeSource.ReturnCd) == 'E' || AsChar
          (export.HiddenIncomeSource.Type1) == 'M' && (
            AsChar(export.HiddenIncomeSource.ReturnCd) == 'A' || AsChar
          (export.HiddenIncomeSource.ReturnCd) == 'R') || AsChar
          (export.HiddenIncomeSource.Type1) == 'O' && (
            Equal(export.HiddenIncomeSource.Code, "SA") || Equal
          (export.HiddenIncomeSource.Code, "WC")) && AsChar
          (export.HiddenIncomeSource.ReturnCd) == 'V')
        {
          if (AsChar(export.IncomeSource.ReturnCd) == AsChar
            (export.HiddenIncomeSource.ReturnCd) && (
              AsChar(export.HiddenIncomeSource.Type1) != 'O' || AsChar
            (export.HiddenIncomeSource.Type1) == 'O' && Equal
            (export.HiddenIncomeSource.Code, export.IncomeSource.Code)))
          {
            // -- The type code/return code combination is still valid for auto 
            // IWO.
            //    Do not cancel any documents.
          }
          else
          {
            // -- The type/return code combination has changed to a combination 
            // for
            // which an auto IWO should not be sent.  Cancel any queued up auto 
            // IWO
            // documents which have not already processed.
            // -- Cancel any autoiwo documents that might be queued up awaiting 
            // batch mailing or e-mailing.
            UseLeCancelAutoIwoDocuments();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              break;
            }
          }
        }

        // 08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to work 
        // with eIWO for SSA.
        // Call the auto IWO cab for Type O, Code SA, Return code V.  Also call 
        // auto IWO cab for
        // Type O, Code WC, Return code V.
        if (AsChar(export.IncomeSource.Type1) == 'E' && AsChar
          (export.IncomeSource.ReturnCd) == 'E' || AsChar
          (export.IncomeSource.Type1) == 'M' && (
            AsChar(export.IncomeSource.ReturnCd) == 'A' || AsChar
          (export.IncomeSource.ReturnCd) == 'R') || AsChar
          (export.IncomeSource.Type1) == 'O' && (
            Equal(export.IncomeSource.Code, "SA") || Equal
          (export.IncomeSource.Code, "WC")) && AsChar
          (export.IncomeSource.ReturnCd) == 'V')
        {
          if (!Lt(local.Current.Date, export.IncomeSource.EndDt) && !
            Equal(export.IncomeSource.EndDt, local.Default1.Date))
          {
            // -- Due to changes in end date edits the following scenario can 
            // never occur.  The cancellation of auto IWO documents is now
            // implemented in the preceeding IF logic.
            goto Test5;
          }

          if (AsChar(export.IncomeSource.ReturnCd) == AsChar
            (export.HiddenIncomeSource.ReturnCd))
          {
            if (!Equal(export.IncomeSource.EndDt,
              export.HiddenIncomeSource.EndDt) && !
              Lt(local.Current.Date, export.HiddenIncomeSource.EndDt))
            {
              // -- Send the IWO if the end date is removed (seasonal employment
              // ).
              goto Test4;
            }

            if (!Equal(export.IncomeSource.ReturnDt,
              export.HiddenIncomeSource.ReturnDt))
            {
              // -- Do not send the IWO if the a new return date is entered (re-
              // verification of employment)
              goto Test5;
            }

            if (Equal(export.IncomeSource.EndDt, export.HiddenIncomeSource.EndDt))
              
            {
              // 05/18/01  GVandy PR 120 - Do not send auto IWO on an update if 
              // the return code and end date are not changed.
              goto Test5;
            }
          }

Test4:

          // ----------------------------------------------------------
          // 2/22/01 WR187	GVandy
          // Automatically generate an IWO.
          // ----------------------------------------------------------
          UseLeAutomaticIwoGeneration2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            break;
          }
        }

Test5:

        MoveIncomeSource5(export.IncomeSource, export.HiddenIncomeSource);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "TYPE":
        // ------------------------------------------
        // 10/15/98 W.Campbell - Modified the following
        // case stmt to use the import view of
        // income_source type rather than the export view.
        // This was to fix a problem associated with the
        // use of the 'clear' command which ultimately
        // caused the export view of income source to
        // be set to defaults.
        // ------------------------------------------
        switch(AsChar(import.IncomeSource.Type1))
        {
          case 'M':
            ExitState = "ECO_XFR_TO_MILI_MILITARY_HIST";

            return;
          case 'R':
            ExitState = "ECO_XFR_TO_RESO_PERSON_RESOURCE";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }

        break;
      case "INCL":
        // *** Work request 000238
        // *** 01/25/01 swsrchf
        // *** start
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
        {
          export.Incl.Text4 = "INCS";
        }

        // *** end
        // *** 01/25/01 swsrchf
        // *** Work request 000238
        ExitState = "ECO_XFR_TO_INCOME_SOURCE_LIST";

        return;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Export
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKey.Number))
        {
          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }
        else
        {
          // 02/29/00 M.L Start
          --export.HiddenStandard.PageNumber;

          // 02/29/00 M.L End
        }

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "CSLN":
        // --If more than one case, then a case selection is required.
        if (export.Export1.Count == 1)
        {
          export.Export1.Index = 0;
          export.Export1.CheckSize();

          export.Selected.Number = export.Export1.Item.Detail.Number;
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            switch(AsChar(export.Export1.Item.Common.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                ++local.Common.Count;

                break;
              default:
                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                break;
            }
          }

          export.Export1.CheckIndex();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          switch(local.Common.Count)
          {
            case 0:
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                var field = GetField(export.Export1.Item.Common, "selectChar");

                field.Error = true;
              }

              export.Export1.CheckIndex();
              ExitState = "SI0000_CASE_SELECT_REQUIRED";

              return;
            case 1:
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
                {
                  export.Selected.Number = export.Export1.Item.Detail.Number;
                }
                else
                {
                }
              }

              export.Export1.CheckIndex();

              break;
            default:
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;
                }
                else
                {
                }
              }

              export.Export1.CheckIndex();
              ExitState = "SI0000_CASE_SELECT_REQUIRED";

              return;
          }
        }

        ExitState = "ECO_LNK_TO_CSLN";

        break;
      case "RETCSLN":
        break;
      default:
        break;
    }

Test6:

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      // 10/25/99 M.L Start
      // 10/25/99 M.L End
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        // -----------------------------------------------------------
        // 10/07/98  W. Campbell -  Modified the
        // following ESCAPE so that it escapes
        // all the way out of the AB instead of
        // only part of the way out.
        // -----------------------------------------------------------
        return;
      }

      // --------------------------------------
      // Retrieve Person Details and associated
      // Cases.
      // --------------------------------------
      export.Export1.Count = 0;
      local.Local1.Count = 0;

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      UseSiReadCasesByPerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        // -----------------------------------------------------------
        // 10/07/98  W. Campbell -  Modified the
        // following ESCAPE so that it escapes
        // all the way out of the AB instead of
        // only part of the way out.
        // -----------------------------------------------------------
        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        // -----------------------------------------------------------
        // 10/07/98  W. Campbell -  Modified the
        // following ESCAPE so that it escapes
        // all the way out of the AB instead of
        // only part of the way out.
        // -----------------------------------------------------------
        return;
      }
      else
      {
        export.HiddenCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
      }

      // 10/25/99 M.L Start
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        export.Export1.Index = local.Local1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.Detail.Number = local.Local1.Item.Detail.Number;
        export.Export1.Update.Common.SelectChar = "";

        var field = GetField(export.Export1.Item.Common, "selectChar");

        field.Protected = false;
      }

      local.Local1.CheckIndex();

      // 10/25/99 M.L End
      // 02/29/00 M.L start
      if (!IsEmpty(local.Page.Number))
      {
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.HiddenPageKey.Number =
          local.Local1.Item.Detail.Number;
      }

      if (local.Local1.Count == 1 && export.HiddenStandard.PageNumber == 1)
      {
        var field = GetField(export.Export1.Item.Common, "selectChar");

        field.Protected = true;
      }

      // 02/29/00 M.L End
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (Equal(export.ScreenPrevCommand.Command, "BLANKS"))
      {
        // ------------------------------
        // If a Resource was passed, read
        // the associated Income Source
        // if it exists.
        // ------------------------------
        if (import.CsePersonResource.ResourceNo != 0)
        {
          if (ReadIncomeSource1())
          {
            export.IncomeSource.Identifier = entities.IncomeSource.Identifier;
            UseSiReadIncomeSourceDetails();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test7;
            }

            MoveIncomeSource5(export.IncomeSource, export.HiddenIncomeSource);
            export.ScreenPrevCommand.Command = "DISPLAY";
          }
          else
          {
            export.IncomeSource.Type1 = "R";
            export.IncomeSource.Name = import.Reso.Description;
            export.NonEmployIncomeSourceAddress.Street1 =
              import.ResourceLocationAddress.Street1 ?? "";
            export.NonEmployIncomeSourceAddress.Street2 =
              import.ResourceLocationAddress.Street2 ?? "";
            export.NonEmployIncomeSourceAddress.City =
              import.ResourceLocationAddress.City ?? "";
            export.NonEmployIncomeSourceAddress.State =
              import.ResourceLocationAddress.State ?? "";
            export.NonEmployIncomeSourceAddress.ZipCode =
              import.ResourceLocationAddress.ZipCode5 ?? "";
            export.NonEmployIncomeSourceAddress.Zip4 =
              import.ResourceLocationAddress.ZipCode4 ?? "";
            export.ScreenPrevCommand.Command = "DISPLAY";
          }
        }
        else if (!Equal(export.IncomeSource.Identifier,
          local.BlankIncomeSource.Identifier))
        {
          // -----------------------------------------
          // An Income Source has been passed from the
          // Income Source List.
          // -----------------------------------------
          UseSiReadIncomeSourceDetails();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test7;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          MoveIncomeSource5(export.IncomeSource, export.HiddenIncomeSource);
          export.ScreenPrevCommand.Command = "DISPLAY";
        }
        else
        {
          // ----------------------------------
          // Check for existing income sources.
          // ----------------------------------
          UseSiIncsCountIncomeSourceRecs();

          switch(TrimEnd(local.IncSourceRecordResult.Command))
          {
            case "NO RECORDS":
              ExitState = "SI0000_NO_INCOME_SOURCES_EXIST";
              export.ScreenPrevCommand.Command = "DISPLAY";

              break;
            case "ONE RECORD":
              UseSiReadIncomeSourceDetails();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test7;
              }

              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
              MoveIncomeSource5(export.IncomeSource, export.HiddenIncomeSource);
              export.ScreenPrevCommand.Command = "DISPLAY";

              break;
            case "MORE THAN ONE RECORD":
              ExitState = "SI0000_INCOME_SOURCES_EXIST";
              export.ScreenPrevCommand.Command = "DISPLAY";

              break;
            default:
              break;
          }
        }

        export.HiddenCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
      }
      else
      {
        // -------------------------------
        // The request was made to display
        // existing Income Sources.
        // -------------------------------
        UseSiIncsCountIncomeSourceRecs();

        switch(TrimEnd(local.IncSourceRecordResult.Command))
        {
          case "NO RECORDS":
            ExitState = "SI0000_NO_INCOME_SOURCES_EXIST";

            break;
          case "ONE RECORD":
            UseSiReadIncomeSourceDetails();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

            break;
          case "MORE THAN ONE RECORD":
            // *** Work request 000238
            // *** 01/25/01 swsrchf
            // *** start
            if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
            {
              export.Incl.Text4 = "INCS";
            }

            // *** end
            // *** 01/25/01 swsrchf
            // *** Work request 000238
            ExitState = "ECO_XFR_TO_INCOME_SOURCE_LIST";

            break;
          default:
            break;
        }
      }

      if (Equal(export.Employer.EiwoEndDate, local.Max.Date))
      {
        export.Employer.EiwoEndDate = local.NullDateWorkArea.Date;
      }

      MoveIncomeSource5(export.IncomeSource, export.HiddenIncomeSource);
      export.ScreenPrevCommand.Command = "DISPLAY";
    }

Test7:

    // ---------------------------------------------
    // Start of code : Raju Dec 25, 1996 1420hrs CST
    // 
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
    {
      // mjr
      // -----------------------------------------------
      // 12/15/1998
      // Added check for an exitstate returned from Print
      // ------------------------------------------------------------
      local.Position.Count =
        Find(String(
          export.HiddenNextTranInfo.MiscText2,
        NextTranInfo.MiscText2_MaxLength),
        TrimEnd(local.SpDocLiteral.IdDocument));

      if (local.Position.Count > 0)
      {
        // mjr---> Determines the appropriate exitstate for the Print process
        local.Print.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces(50);
        UseSpPrintDecodeReturnCode();
        export.HiddenNextTranInfo.MiscText2 = local.Print.Text50;
      }

      export.LastReadHidden.EndDt = export.IncomeSource.EndDt;
      export.LastReadHidden.ReturnDt = export.IncomeSource.ReturnDt;
      export.LastReadHidden.ReturnCd = export.IncomeSource.ReturnCd ?? "";
    }

    // ---------------------------------------------
    // End   of code
    // ---------------------------------------------
    // ------------------------------------------
    // If the Income Source Address is from
    // Employer, protect from update.
    // ------------------------------------------
    if (export.Employer.Identifier != 0)
    {
      var field1 = GetField(export.IncomeSource, "name");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.NonEmployIncomeSourceAddress, "street1");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.NonEmployIncomeSourceAddress, "street2");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.NonEmployIncomeSourceAddress, "city");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.NonEmployIncomeSourceAddress, "state");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.StatePrompt, "selectChar");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.NonEmployIncomeSourceAddress, "zipCode");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.NonEmployIncomeSourceAddress, "zip4");

      field8.Color = "cyan";
      field8.Protected = true;
    }

    // --------------------------------------------------------------------------
    // Call the CAB to check Family Violence.
    //                                                   
    // Vithal (01/03/02)
    // --------------------------------------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseScSecurityValidAuthForFv();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.IncomeSource.Assign(local.BlankIncomeSource);
        export.NonEmployIncomeSourceAddress.Assign(
          local.BlankNonEmployIncomeSourceAddress);
        MoveIncomeSourceContact1(local.BlankIncomeSourceContact, export.Phone);
        export.Fax.Assign(local.BlankIncomeSourceContact);
        export.CsePerson.FederalInd = "";
        export.CsePerson.UnemploymentInd = "";
      }
      else
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
    }

    // ------------------------------------------------------
    // 10/12/98 W. Campbell - Code added to fix protected field
    // problems.
    // ------------------------------------------------------
    if (AsChar(export.IncomeSource.Type1) == 'E' || AsChar
      (export.IncomeSource.Type1) == 'M' || AsChar
      (export.IncomeSource.Type1) == 'R' || AsChar
      (export.IncomeSource.Type1) == 'O')
    {
      var field1 = GetField(export.IncomeSource, "type1");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.IncSrcTypePrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      if (AsChar(export.IncomeSource.Type1) != 'O')
      {
        var field3 = GetField(export.IncomeSource, "code");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.OtherCodePrompt, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;
      }
    }

    // ------------------------------------------------------
    // 10/12/98 W. Campbell - End of code added to fix
    // protected field problems.
    // ------------------------------------------------------
    // ---------------------------------------------
    // Code added by Raju  Dec 24, 1996:0545 hrs CST
    // The oe cab raise event will be called from
    //   here case of update.
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      // 06/14/99 M. Lachowicz Start
      if (AsChar(local.SendDateUpdated.Flag) == 'Y')
      {
        // 12//28/00 M.L Start
        if (AsChar(export.IncomeSource.Type1) == 'M' || AsChar
          (export.IncomeSource.Type1) == 'O' || AsChar
          (export.IncomeSource.Type1) == 'R')
        {
          goto Test8;
        }

        // 12//28/00 M.L End
        // 07/08/2002 M. Lachowicz Start
        if (IsEmpty(local.SpDocKey.KeyCase))
        {
          import.Import1.Index = 0;
          import.Import1.CheckSize();

          if (IsEmpty(import.Import1.Item.Detail.Number))
          {
            ExitState = "SI0000_INCS_ERROR";
            UseEabRollbackCics();

            return;
          }

          local.SpDocKey.KeyCase = import.Import1.Item.Detail.Number;
        }

        // 07/08/2002 M. Lachowicz End
        // Added creation of document trigger
        // -------------------------------------------------------------
        // 11/01/2011 CQ30190 Revised rules for employer verification letter.
        local.Document.Name = "EMPVERIF";
        local.CuFound.Flag = "N";

        // CQ30191 Read active case units first to obtain obligated state.
        foreach(var item in ReadCaseUnit1())
        {
          local.CuFound.Flag = "Y";

          if (CharAt(entities.CaseUnit.State, 5) == 'Y' || CharAt
            (entities.CaseUnit.State, 5) == 'N')
          {
            local.Document.Name = "EMPVERCO";

            break;
          }
        }

        // CQ30191 Read inactive case units only if obligated state not yet 
        // determined.
        if (AsChar(local.CuFound.Flag) == 'N')
        {
          foreach(var item in ReadCaseUnit2())
          {
            if (CharAt(entities.CaseUnit.State, 5) == 'Y' || CharAt
              (entities.CaseUnit.State, 5) == 'N')
            {
              local.Document.Name = "EMPVERCO";

              break;
            }
          }
        }

        local.SpDocKey.KeyPerson = export.CsePerson.Number;
        local.SpDocKey.KeyIncomeSource = export.IncomeSource.Identifier;
        UseSpCreateDocumentInfrastruct();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }

          if (Equal(global.Command, "CREATE"))
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
        }
        else
        {
          return;
        }
      }

Test8:

      // 06/14/99 M. Lachowicz End
      local.Infrastructure.UserId = "INCS";

      // ------------
      // Beg WO # 246
      // ------------
      local.Infrastructure.BusinessObjectCd = "CAU";

      // ------------
      // End WO # 246
      // ------------
      local.Ap.Number = export.CsePersonsWorkSet.Number;
      local.Infrastructure.DenormTimestamp = export.IncomeSource.Identifier;
      local.Infrastructure.SituationNumber = 0;

      for(local.NumberOfEvents.TotalInteger = 1; local
        .NumberOfEvents.TotalInteger <= 2; ++local.NumberOfEvents.TotalInteger)
      {
        local.RaiseEventFlag.Text1 = "N";

        if (local.NumberOfEvents.TotalInteger == 1)
        {
          // ---------------------------------------------
          // 02/18/99 W.Campbell - Replaced use of
          // DATENUM(0) with view
          // local_default date_work_area date.
          // ---------------------------------------------
          if (!Equal(export.IncomeSource.EndDt, export.LastReadHidden.EndDt) &&
            Lt(local.Default1.Date, export.IncomeSource.EndDt))
          {
            local.Infrastructure.EventId = 10;
            local.Infrastructure.ReasonCode = "INCSEXPD";
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReferenceDate = export.IncomeSource.EndDt;
            local.DetailText30.Text30 = "Income Source :";
            local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + TrimEnd
              (export.IncomeSource.Name);
            local.DetailText30.Text30 = " , Ended :";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (local.DetailText30.Text30);
            local.Date.Date = local.Infrastructure.ReferenceDate;
            local.DetailText10.Text10 = UseCabConvertDate2String();
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + TrimEnd
              (local.DetailText10.Text10);
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 2)
        {
          // ---------------------------------------------
          // 02/18/99 W.Campbell - Replaced use of
          // DATENUM(0) with view
          // local_default date_work_area date.
          // ---------------------------------------------
          if (Lt(local.Default1.Date, export.IncomeSource.ReturnDt) && !
            IsEmpty(export.IncomeSource.ReturnCd))
          {
            if (!Equal(export.IncomeSource.ReturnDt,
              export.LastReadHidden.ReturnDt) || AsChar
              (export.IncomeSource.ReturnCd) != AsChar
              (export.LastReadHidden.ReturnCd))
            {
              // ---------------------------------------------
              // 02/18/99 W.Campbell - Disabled statements
              // dealing with closing monitored documents,
              // as it has been determined that the best way
              // to handle them will be in Batch.
              // ---------------------------------------------
              local.Infrastructure.EventId = 10;
              local.Infrastructure.ReasonCode = "INCSVRFD";

              switch(AsChar(export.IncomeSource.Type1))
              {
                case 'E':
                  if (AsChar(export.IncomeSource.ReturnCd) == 'E')
                  {
                    local.RaiseEventFlag.Text1 = "Y";
                  }

                  break;
                case 'M':
                  if (AsChar(export.IncomeSource.ReturnCd) == 'A' || AsChar
                    (export.IncomeSource.ReturnCd) == 'R')
                  {
                    local.RaiseEventFlag.Text1 = "Y";
                  }

                  break;
                case 'R':
                  if (AsChar(export.IncomeSource.ReturnCd) == 'V')
                  {
                    local.RaiseEventFlag.Text1 = "Y";
                  }

                  break;
                case 'O':
                  if (AsChar(export.IncomeSource.ReturnCd) == 'V')
                  {
                    local.RaiseEventFlag.Text1 = "Y";
                  }

                  break;
                default:
                  break;
              }

              if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
              {
                // ---------------------------------------------
                // There is no need to call string functions
                //   unnecessarily if the event is not to be
                //   raised
                // ---------------------------------------------
                local.Infrastructure.ReferenceDate =
                  export.IncomeSource.ReturnDt;
                local.DetailText30.Text30 = "Income Source :";
                local.Infrastructure.Detail =
                  TrimEnd(local.DetailText30.Text30) + TrimEnd
                  (export.IncomeSource.Name);
                local.DetailText30.Text30 = " , Verified :";
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + TrimEnd
                  (local.DetailText30.Text30);
                local.Date.Date = local.Infrastructure.ReferenceDate;
                local.DetailText10.Text10 = UseCabConvertDate2String();
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + TrimEnd
                  (local.DetailText10.Text10);

                // ---------------------------------------------
                // Begin of Code - External Alert
                //   Raju : 01/07/97;1000 hrs CST
                // ---------------------------------------------
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // - Action Remaining to be taken
                //   : Check if employer source is not 'DHR'
                // - The external alert is to be raised only for
                //   type "E" and "M".
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (AsChar(export.IncomeSource.Type1) == 'E' || AsChar
                  (export.IncomeSource.Type1) == 'M')
                {
                  local.InterfaceAlert.AlertCode = "46";
                  UseSpIncsExternalAlert();

                  if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") && !
                    IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                  {
                    UseEabRollbackCics();

                    return;
                  }
                }

                // ---------------------------------------------
                // End   of Code
                // ---------------------------------------------
              }

              // ---------------------------------------------
              // Begin of Code - Close monitored document
              //   Raju : 01/23/97;1430 hrs CST
              // Sid : Close monitored documents for all type
              //       of return codes. 8/21/97
              // ---------------------------------------------
              // ---------------------------------------------
              // 02/18/99 W.Campbell - Disabled statements
              // dealing with closing monitored documents,
              // as it has been determined that the best way
              // to handle them will be in Batch.
              // ---------------------------------------------
              // ---------------------------------------------
              // End   of Code
              // ---------------------------------------------
              // ---------------------------------------------
              // 05/20/99 W.Campbell - Added logic to
              // process interstate event for Employer
              // confirmed (verified)  (LSCEM).
              // ---------------------------------------------
              if ((AsChar(export.IncomeSource.Type1) == 'E' && AsChar
                (export.IncomeSource.ReturnCd) == 'E' || AsChar
                (export.IncomeSource.Type1) == 'M' && AsChar
                (export.IncomeSource.ReturnCd) == 'A') && Equal
                (export.IncomeSource.EndDt, local.Default1.Date))
              {
                // ---------------------------------------------
                // 05/20/99 W.Campbell - Save the state
                // of the current exit state.
                // ---------------------------------------------
                if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                {
                  local.Save.State = "AD";
                }
                else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                  local.Save.State = "UP";
                }
                else
                {
                  UseEabRollbackCics();

                  return;
                }

                local.ScreenIdentification.Command = "INCS";
                local.Csenet.Number = export.CsePersonsWorkSet.Number;
                UseSiCreateAutoCsenetTrans();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // ---------------------------------------------
                  // 05/20/99 W.Campbell - Restore the
                  // exit state from the saved state.
                  // ---------------------------------------------
                  switch(TrimEnd(local.Save.State))
                  {
                    case "AD":
                      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

                      break;
                    case "UP":
                      ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                      break;
                    default:
                      ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";
                      UseEabRollbackCics();

                      return;
                  }
                }
                else
                {
                  UseEabRollbackCics();

                  return;
                }
              }

              // ---------------------------------------------
              // 05/20/99 W.Campbell - End of logic added
              // to process interstate event for Employer
              // confirmed (verified)  (LSCEM).
              // ---------------------------------------------
            }
          }
        }
        else
        {
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          UseOeCabRaiseEvent();

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") && !
            IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            UseEabRollbackCics();

            return;
          }
        }
      }

      export.LastReadHidden.EndDt = export.IncomeSource.EndDt;
      export.LastReadHidden.ReturnDt = export.IncomeSource.ReturnDt;
      export.LastReadHidden.ReturnCd = export.IncomeSource.ReturnCd ?? "";
    }

    // ---------------------------------------------
    // End   of code
    // ---------------------------------------------
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
    target.EiwoEndDate = source.EiwoEndDate;
    target.EiwoStartDate = source.EiwoStartDate;
  }

  private static void MoveExport1ToLocal1(SiReadCasesByPerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Detail.Number = source.Detail.Number;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnCd = source.ReturnCd;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource3(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnCd = source.ReturnCd;
    target.StartDt = source.StartDt;
  }

  private static void MoveIncomeSource4(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnCd = source.ReturnCd;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveIncomeSource5(IncomeSource source, IncomeSource target)
    
  {
    target.Type1 = source.Type1;
    target.SentDt = source.SentDt;
    target.SendTo = source.SendTo;
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.EndDt = source.EndDt;
    target.Code = source.Code;
  }

  private static void MoveIncomeSourceContact1(IncomeSourceContact source,
    IncomeSourceContact target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.ExtensionNo = source.ExtensionNo;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveIncomeSourceContact2(IncomeSourceContact source,
    IncomeSourceContact target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveNonEmployIncomeSourceAddress(
    NonEmployIncomeSourceAddress source, NonEmployIncomeSourceAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyPerson = source.KeyPerson;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdIncomeSource = source.IdIncomeSource;
    target.IdPrNumber = source.IdPrNumber;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidValue.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
    export.Next.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeAutomaticIwoGeneration1()
  {
    var useImport = new LeAutomaticIwoGeneration.Import();
    var useExport = new LeAutomaticIwoGeneration.Export();

    MoveIncomeSource3(export.IncomeSource, useImport.IncomeSource);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(LeAutomaticIwoGeneration.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseLeAutomaticIwoGeneration2()
  {
    var useImport = new LeAutomaticIwoGeneration.Import();
    var useExport = new LeAutomaticIwoGeneration.Export();

    MoveIncomeSource3(export.IncomeSource, useImport.IncomeSource);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(LeAutomaticIwoGeneration.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseLeCancelAutoIwoDocuments()
  {
    var useImport = new LeCancelAutoIwoDocuments.Import();
    var useExport = new LeCancelAutoIwoDocuments.Export();

    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(LeCancelAutoIwoDocuments.Execute, useImport, useExport);
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.CsePerson.Number = local.Ap.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.CsePerson.Number = local.Csenet.Number;
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiIncsAssocIncomeSourceAddr()
  {
    var useImport = new SiIncsAssocIncomeSourceAddr.Import();
    var useExport = new SiIncsAssocIncomeSourceAddr.Export();

    useImport.CsePersonResource.ResourceNo =
      export.CsePersonResource.ResourceNo;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Employer.Identifier = export.Employer.Identifier;
    useImport.NonEmployIncomeSourceAddress.Assign(
      export.NonEmployIncomeSourceAddress);
    MoveIncomeSource1(export.IncomeSource, useImport.IncomeSource);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiIncsAssocIncomeSourceAddr.Execute, useImport, useExport);

    export.NonEmployIncomeSourceAddress.Assign(
      useImport.NonEmployIncomeSourceAddress);
    export.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseSiIncsCountIncomeSourceRecs()
  {
    var useImport = new SiIncsCountIncomeSourceRecs.Import();
    var useExport = new SiIncsCountIncomeSourceRecs.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiIncsCountIncomeSourceRecs.Execute, useImport, useExport);

    local.IncSourceRecordResult.Command = useExport.Result.Command;
    MoveIncomeSource1(useExport.IncomeSource, export.IncomeSource);
  }

  private void UseSiIncsCreateIncomeSource()
  {
    var useImport = new SiIncsCreateIncomeSource.Import();
    var useExport = new SiIncsCreateIncomeSource.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Assign(export.IncomeSource);
    useImport.CsePerson.Assign(export.CsePerson);

    Call(SiIncsCreateIncomeSource.Execute, useImport, useExport);

    export.IncomeSource.Assign(useImport.IncomeSource);
    export.CsePerson.Assign(useImport.CsePerson);
  }

  private void UseSiIncsCreateIncomeSrcContct1()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveIncomeSourceContact2(export.Fax, useImport.IncomeSourceContact);

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    export.CsePerson.Number = useImport.CsePerson.Number;
    export.Fax.Assign(useImport.IncomeSourceContact);
  }

  private void UseSiIncsCreateIncomeSrcContct2()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSourceContact.Assign(export.Phone);
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    export.Phone.Assign(useImport.IncomeSourceContact);
    export.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseSiIncsEndDtPriorEmployment()
  {
    var useImport = new SiIncsEndDtPriorEmployment.Import();
    var useExport = new SiIncsEndDtPriorEmployment.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveIncomeSource4(export.IncomeSource, useImport.IncomeSource);

    Call(SiIncsEndDtPriorEmployment.Execute, useImport, useExport);
  }

  private void UseSiIncsUpdateIncomeSource()
  {
    var useImport = new SiIncsUpdateIncomeSource.Import();
    var useExport = new SiIncsUpdateIncomeSource.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Assign(export.IncomeSource);
    useImport.CsePerson.Assign(export.CsePerson);

    Call(SiIncsUpdateIncomeSource.Execute, useImport, useExport);

    export.IncomeSource.Assign(useImport.IncomeSource);
    export.CsePerson.Assign(useImport.CsePerson);
  }

  private void UseSiReadCasesByPerson()
  {
    var useImport = new SiReadCasesByPerson.Import();
    var useExport = new SiReadCasesByPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    useImport.Page.Number = export.HiddenPageKeys.Item.HiddenPageKey.Number;

    Call(SiReadCasesByPerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
    local.Page.Number = useExport.Page.Number;
    local.AbendData.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    export.CsePerson.Assign(useExport.CsePerson);
  }

  private void UseSiReadIncomeSourceDetails()
  {
    var useImport = new SiReadIncomeSourceDetails.Import();
    var useExport = new SiReadIncomeSourceDetails.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;

    Call(SiReadIncomeSourceDetails.Execute, useImport, useExport);

    export.CsePersonResource.ResourceNo =
      useExport.CsePersonResource.ResourceNo;
    export.Employer.Assign(useExport.Employer);
    export.Fax.Assign(useExport.Fax);
    export.Phone.Assign(useExport.Phone);
    export.NonEmployIncomeSourceAddress.Assign(
      useExport.NonEmployIncomeSourceAddress);
    export.IncomeSource.Assign(useExport.IncomeSource);
    export.CsePerson.Assign(useExport.CsePerson);
    export.EmployerAddress.Note = useExport.EmployerAddress.Note;
  }

  private void UseSiUpdateIncomeSourceContact1()
  {
    var useImport = new SiUpdateIncomeSourceContact.Import();
    var useExport = new SiUpdateIncomeSourceContact.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveIncomeSourceContact2(export.Fax, useImport.IncomeSourceContact);
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiUpdateIncomeSourceContact.Execute, useImport, useExport);

    export.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseSiUpdateIncomeSourceContact2()
  {
    var useImport = new SiUpdateIncomeSourceContact.Import();
    var useExport = new SiUpdateIncomeSourceContact.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSourceContact.Assign(export.Phone);
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiUpdateIncomeSourceContact.Execute, useImport, useExport);

    export.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpIncsExternalAlert()
  {
    var useImport = new SpIncsExternalAlert.Import();
    var useExport = new SpIncsExternalAlert.Export();

    useImport.CsePerson.Number = local.Ap.Number;
    useImport.Employer.Identifier = export.Employer.Identifier;
    MoveNonEmployIncomeSourceAddress(export.NonEmployIncomeSourceAddress,
      useImport.NonEmployIncomeSourceAddress);
    MoveIncomeSource2(export.IncomeSource, useImport.IncomeSource);

    Call(SpIncsExternalAlert.Execute, useImport, useExport);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.Print.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.Print.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadCaseRole()
  {
    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.NumberOfApAndArRoles.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", export.CsePerson.Number);
        db.SetString(command, "casNo", local.SpDocKey.KeyCase);
        db.SetNullableDate(
          command, "closureDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", export.CsePerson.Number);
        db.SetString(command, "casNo", local.SpDocKey.KeyCase);
        db.SetNullableDate(
          command, "closureDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadEmployer()
  {
    entities.Ssa.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Ssa.Identifier = db.GetInt32(reader, 0);
        entities.Ssa.Ein = db.GetNullableString(reader, 1);
        entities.Ssa.Populated = true;
      });
  }

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "empId", import.SelectedEmployer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 1);
        entities.EmployerAddress.Note = db.GetNullableString(reader, 2);
        entities.EmployerAddress.Populated = true;
      });
  }

  private bool ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cprResourceNo", import.CsePersonResource.ResourceNo);
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.CprResourceNo = db.GetNullableInt32(reader, 2);
        entities.IncomeSource.CspNumber = db.GetNullableString(reader, 3);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadIncomeSource2()
  {
    entities.Re.Populated = false;

    return Read("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
        db.SetNullableInt32(command, "empId", export.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Re.Identifier = db.GetDateTime(reader, 0);
        entities.Re.Type1 = db.GetString(reader, 1);
        entities.Re.CspINumber = db.GetString(reader, 2);
        entities.Re.CprResourceNo = db.GetNullableInt32(reader, 3);
        entities.Re.CspNumber = db.GetNullableString(reader, 4);
        entities.Re.EmpId = db.GetNullableInt32(reader, 5);
        entities.Re.EndDt = db.GetNullableDate(reader, 6);
        entities.Re.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Re.Type1);
      });
  }

  private bool ReadIncomeSource3()
  {
    entities.Re.Populated = false;

    return Read("ReadIncomeSource3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
        db.SetNullableInt32(
          command, "cprResourceNo", export.CsePersonResource.ResourceNo);
      },
      (db, reader) =>
      {
        entities.Re.Identifier = db.GetDateTime(reader, 0);
        entities.Re.Type1 = db.GetString(reader, 1);
        entities.Re.CspINumber = db.GetString(reader, 2);
        entities.Re.CprResourceNo = db.GetNullableInt32(reader, 3);
        entities.Re.CspNumber = db.GetNullableString(reader, 4);
        entities.Re.EmpId = db.GetNullableInt32(reader, 5);
        entities.Re.EndDt = db.GetNullableDate(reader, 6);
        entities.Re.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Re.Type1);
      });
  }

  private bool ReadIncomeSource4()
  {
    entities.Re.Populated = false;

    return Read("ReadIncomeSource4",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Re.Identifier = db.GetDateTime(reader, 0);
        entities.Re.Type1 = db.GetString(reader, 1);
        entities.Re.CspINumber = db.GetString(reader, 2);
        entities.Re.CprResourceNo = db.GetNullableInt32(reader, 3);
        entities.Re.CspNumber = db.GetNullableString(reader, 4);
        entities.Re.EmpId = db.GetNullableInt32(reader, 5);
        entities.Re.EndDt = db.GetNullableDate(reader, 6);
        entities.Re.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Re.Type1);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public Case1 HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 hiddenPageKey;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Case1 Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common common;
      private Case1 detail;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of FromMenu.
    /// </summary>
    [JsonPropertyName("fromMenu")]
    public CsePerson FromMenu
    {
      get => fromMenu ??= new();
      set => fromMenu = value;
    }

    /// <summary>
    /// A value of HiddenEmployer.
    /// </summary>
    [JsonPropertyName("hiddenEmployer")]
    public Employer HiddenEmployer
    {
      get => hiddenEmployer ??= new();
      set => hiddenEmployer = value;
    }

    /// <summary>
    /// A value of HiddenIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenIncomeSource")]
    public IncomeSource HiddenIncomeSource
    {
      get => hiddenIncomeSource ??= new();
      set => hiddenIncomeSource = value;
    }

    /// <summary>
    /// A value of ScreenPrevCommand.
    /// </summary>
    [JsonPropertyName("screenPrevCommand")]
    public Common ScreenPrevCommand
    {
      get => screenPrevCommand ??= new();
      set => screenPrevCommand = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of SelectedEmployerAddress.
    /// </summary>
    [JsonPropertyName("selectedEmployerAddress")]
    public EmployerAddress SelectedEmployerAddress
    {
      get => selectedEmployerAddress ??= new();
      set => selectedEmployerAddress = value;
    }

    /// <summary>
    /// A value of Fax.
    /// </summary>
    [JsonPropertyName("fax")]
    public IncomeSourceContact Fax
    {
      get => fax ??= new();
      set => fax = value;
    }

    /// <summary>
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public IncomeSourceContact Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    /// <summary>
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ReturnCdPrompt.
    /// </summary>
    [JsonPropertyName("returnCdPrompt")]
    public Common ReturnCdPrompt
    {
      get => returnCdPrompt ??= new();
      set => returnCdPrompt = value;
    }

    /// <summary>
    /// A value of SendToPrompt.
    /// </summary>
    [JsonPropertyName("sendToPrompt")]
    public Common SendToPrompt
    {
      get => sendToPrompt ??= new();
      set => sendToPrompt = value;
    }

    /// <summary>
    /// A value of IncSrcTypePrompt.
    /// </summary>
    [JsonPropertyName("incSrcTypePrompt")]
    public Common IncSrcTypePrompt
    {
      get => incSrcTypePrompt ??= new();
      set => incSrcTypePrompt = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Common PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of OtherCodePrompt.
    /// </summary>
    [JsonPropertyName("otherCodePrompt")]
    public Common OtherCodePrompt
    {
      get => otherCodePrompt ??= new();
      set => otherCodePrompt = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Reso.
    /// </summary>
    [JsonPropertyName("reso")]
    public CodeValue Reso
    {
      get => reso ??= new();
      set => reso = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of SelectedEmployer.
    /// </summary>
    [JsonPropertyName("selectedEmployer")]
    public Employer SelectedEmployer
    {
      get => selectedEmployer ??= new();
      set => selectedEmployer = value;
    }

    /// <summary>
    /// A value of LastReadHidden.
    /// </summary>
    [JsonPropertyName("lastReadHidden")]
    public IncomeSource LastReadHidden
    {
      get => lastReadHidden ??= new();
      set => lastReadHidden = value;
    }

    /// <summary>
    /// A value of Incl.
    /// </summary>
    [JsonPropertyName("incl")]
    public WorkArea Incl
    {
      get => incl ??= new();
      set => incl = value;
    }

    /// <summary>
    /// A value of PrintSelectedFor.
    /// </summary>
    [JsonPropertyName("printSelectedFor")]
    public Case1 PrintSelectedFor
    {
      get => printSelectedFor ??= new();
      set => printSelectedFor = value;
    }

    /// <summary>
    /// A value of StartDateConfirmation.
    /// </summary>
    [JsonPropertyName("startDateConfirmation")]
    public Common StartDateConfirmation
    {
      get => startDateConfirmation ??= new();
      set => startDateConfirmation = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private Document document;
    private CsePerson fromMenu;
    private Employer hiddenEmployer;
    private IncomeSource hiddenIncomeSource;
    private Common screenPrevCommand;
    private CsePersonResource csePersonResource;
    private ResourceLocationAddress resourceLocationAddress;
    private CodeValue selectedCodeValue;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard hiddenStandard;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<ImportGroup> import1;
    private Case1 next;
    private Employer employer;
    private EmployerAddress selectedEmployerAddress;
    private IncomeSourceContact fax;
    private IncomeSourceContact phone;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private IncomeSource incomeSource;
    private Standard standard;
    private Common returnCdPrompt;
    private Common sendToPrompt;
    private Common incSrcTypePrompt;
    private Common personPrompt;
    private Common statePrompt;
    private Common otherCodePrompt;
    private NextTranInfo hiddenNextTranInfo;
    private CodeValue reso;
    private CsePerson csePerson;
    private Employer selectedEmployer;
    private IncomeSource lastReadHidden;
    private WorkArea incl;
    private Case1 printSelectedFor;
    private Common startDateConfirmation;
    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public Case1 HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 hiddenPageKey;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Case1 Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common common;
      private Case1 detail;
    }

    /// <summary>
    /// A value of DocmProtectFilter.
    /// </summary>
    [JsonPropertyName("docmProtectFilter")]
    public Common DocmProtectFilter
    {
      get => docmProtectFilter ??= new();
      set => docmProtectFilter = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of ToMenu.
    /// </summary>
    [JsonPropertyName("toMenu")]
    public CsePerson ToMenu
    {
      get => toMenu ??= new();
      set => toMenu = value;
    }

    /// <summary>
    /// A value of HiddenEmployer.
    /// </summary>
    [JsonPropertyName("hiddenEmployer")]
    public Employer HiddenEmployer
    {
      get => hiddenEmployer ??= new();
      set => hiddenEmployer = value;
    }

    /// <summary>
    /// A value of HiddenIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenIncomeSource")]
    public IncomeSource HiddenIncomeSource
    {
      get => hiddenIncomeSource ??= new();
      set => hiddenIncomeSource = value;
    }

    /// <summary>
    /// A value of ScreenPrevCommand.
    /// </summary>
    [JsonPropertyName("screenPrevCommand")]
    public Common ScreenPrevCommand
    {
      get => screenPrevCommand ??= new();
      set => screenPrevCommand = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of Fax.
    /// </summary>
    [JsonPropertyName("fax")]
    public IncomeSourceContact Fax
    {
      get => fax ??= new();
      set => fax = value;
    }

    /// <summary>
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public IncomeSourceContact Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    /// <summary>
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ReturnCdPrompt.
    /// </summary>
    [JsonPropertyName("returnCdPrompt")]
    public Common ReturnCdPrompt
    {
      get => returnCdPrompt ??= new();
      set => returnCdPrompt = value;
    }

    /// <summary>
    /// A value of SendToPrompt.
    /// </summary>
    [JsonPropertyName("sendToPrompt")]
    public Common SendToPrompt
    {
      get => sendToPrompt ??= new();
      set => sendToPrompt = value;
    }

    /// <summary>
    /// A value of IncSrcTypePrompt.
    /// </summary>
    [JsonPropertyName("incSrcTypePrompt")]
    public Common IncSrcTypePrompt
    {
      get => incSrcTypePrompt ??= new();
      set => incSrcTypePrompt = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Common PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of OtherCodePrompt.
    /// </summary>
    [JsonPropertyName("otherCodePrompt")]
    public Common OtherCodePrompt
    {
      get => otherCodePrompt ??= new();
      set => otherCodePrompt = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LastReadHidden.
    /// </summary>
    [JsonPropertyName("lastReadHidden")]
    public IncomeSource LastReadHidden
    {
      get => lastReadHidden ??= new();
      set => lastReadHidden = value;
    }

    /// <summary>
    /// A value of Incl.
    /// </summary>
    [JsonPropertyName("incl")]
    public WorkArea Incl
    {
      get => incl ??= new();
      set => incl = value;
    }

    /// <summary>
    /// A value of PrintSelectedFor.
    /// </summary>
    [JsonPropertyName("printSelectedFor")]
    public Case1 PrintSelectedFor
    {
      get => printSelectedFor ??= new();
      set => printSelectedFor = value;
    }

    /// <summary>
    /// A value of Empty.
    /// </summary>
    [JsonPropertyName("empty")]
    public CsePersonEmailAddress Empty
    {
      get => empty ??= new();
      set => empty = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Case1 Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of StartDateConfirmation.
    /// </summary>
    [JsonPropertyName("startDateConfirmation")]
    public Common StartDateConfirmation
    {
      get => startDateConfirmation ??= new();
      set => startDateConfirmation = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private Common docmProtectFilter;
    private Document filter;
    private CsePerson toMenu;
    private Employer hiddenEmployer;
    private IncomeSource hiddenIncomeSource;
    private Common screenPrevCommand;
    private CsePersonResource csePersonResource;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard hiddenStandard;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<ExportGroup> export1;
    private Case1 next;
    private Employer employer;
    private IncomeSourceContact fax;
    private IncomeSourceContact phone;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private IncomeSource incomeSource;
    private Standard standard;
    private Common returnCdPrompt;
    private Common sendToPrompt;
    private Common incSrcTypePrompt;
    private Common personPrompt;
    private Common statePrompt;
    private Common otherCodePrompt;
    private Code prompt;
    private NextTranInfo hiddenNextTranInfo;
    private CsePerson csePerson;
    private IncomeSource lastReadHidden;
    private WorkArea incl;
    private Case1 printSelectedFor;
    private CsePersonEmailAddress empty;
    private Case1 selected;
    private Common startDateConfirmation;
    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Case1 Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 detail;
    }

    /// <summary>
    /// A value of BlankEmployerAddress.
    /// </summary>
    [JsonPropertyName("blankEmployerAddress")]
    public EmployerAddress BlankEmployerAddress
    {
      get => blankEmployerAddress ??= new();
      set => blankEmployerAddress = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of CuFound.
    /// </summary>
    [JsonPropertyName("cuFound")]
    public Common CuFound
    {
      get => cuFound ??= new();
      set => cuFound = value;
    }

    /// <summary>
    /// A value of NumberOfApAndArRoles.
    /// </summary>
    [JsonPropertyName("numberOfApAndArRoles")]
    public Common NumberOfApAndArRoles
    {
      get => numberOfApAndArRoles ??= new();
      set => numberOfApAndArRoles = value;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of SelectionsNumber.
    /// </summary>
    [JsonPropertyName("selectionsNumber")]
    public Common SelectionsNumber
    {
      get => selectionsNumber ??= new();
      set => selectionsNumber = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Csenet.
    /// </summary>
    [JsonPropertyName("csenet")]
    public CsePerson Csenet
    {
      get => csenet ??= new();
      set => csenet = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public WorkArea Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public DateWorkArea Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BlankCsePersonResource.
    /// </summary>
    [JsonPropertyName("blankCsePersonResource")]
    public CsePersonResource BlankCsePersonResource
    {
      get => blankCsePersonResource ??= new();
      set => blankCsePersonResource = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of ValidValue.
    /// </summary>
    [JsonPropertyName("validValue")]
    public Common ValidValue
    {
      get => validValue ??= new();
      set => validValue = value;
    }

    /// <summary>
    /// A value of BlankEmployer.
    /// </summary>
    [JsonPropertyName("blankEmployer")]
    public Employer BlankEmployer
    {
      get => blankEmployer ??= new();
      set => blankEmployer = value;
    }

    /// <summary>
    /// A value of BlankIncomeSourceContact.
    /// </summary>
    [JsonPropertyName("blankIncomeSourceContact")]
    public IncomeSourceContact BlankIncomeSourceContact
    {
      get => blankIncomeSourceContact ??= new();
      set => blankIncomeSourceContact = value;
    }

    /// <summary>
    /// A value of BlankNonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("blankNonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress BlankNonEmployIncomeSourceAddress
    {
      get => blankNonEmployIncomeSourceAddress ??= new();
      set => blankNonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of BlankIncomeSource.
    /// </summary>
    [JsonPropertyName("blankIncomeSource")]
    public IncomeSource BlankIncomeSource
    {
      get => blankIncomeSource ??= new();
      set => blankIncomeSource = value;
    }

    /// <summary>
    /// A value of BlankPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("blankPersonIncomeHistory")]
    public PersonIncomeHistory BlankPersonIncomeHistory
    {
      get => blankPersonIncomeHistory ??= new();
      set => blankPersonIncomeHistory = value;
    }

    /// <summary>
    /// A value of IncSourceRecordResult.
    /// </summary>
    [JsonPropertyName("incSourceRecordResult")]
    public Common IncSourceRecordResult
    {
      get => incSourceRecordResult ??= new();
      set => incSourceRecordResult = value;
    }

    /// <summary>
    /// A value of InvalidSel.
    /// </summary>
    [JsonPropertyName("invalidSel")]
    public Common InvalidSel
    {
      get => invalidSel ??= new();
      set => invalidSel = value;
    }

    /// <summary>
    /// A value of MoreThanOneSrcRecord.
    /// </summary>
    [JsonPropertyName("moreThanOneSrcRecord")]
    public Common MoreThanOneSrcRecord
    {
      get => moreThanOneSrcRecord ??= new();
      set => moreThanOneSrcRecord = value;
    }

    /// <summary>
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public Case1 Page
    {
      get => page ??= new();
      set => page = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of SendDateUpdated.
    /// </summary>
    [JsonPropertyName("sendDateUpdated")]
    public Common SendDateUpdated
    {
      get => sendDateUpdated ??= new();
      set => sendDateUpdated = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
    }

    /// <summary>
    /// A value of NumberOfEvents.
    /// </summary>
    [JsonPropertyName("numberOfEvents")]
    public Common NumberOfEvents
    {
      get => numberOfEvents ??= new();
      set => numberOfEvents = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of CloseMonitoredDoc.
    /// </summary>
    [JsonPropertyName("closeMonitoredDoc")]
    public WorkArea CloseMonitoredDoc
    {
      get => closeMonitoredDoc ??= new();
      set => closeMonitoredDoc = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public Common Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    /// <summary>
    /// A value of TodayPlus6Months.
    /// </summary>
    [JsonPropertyName("todayPlus6Months")]
    public DateWorkArea TodayPlus6Months
    {
      get => todayPlus6Months ??= new();
      set => todayPlus6Months = value;
    }

    /// <summary>
    /// A value of EmailVerify.
    /// </summary>
    [JsonPropertyName("emailVerify")]
    public Common EmailVerify
    {
      get => emailVerify ??= new();
      set => emailVerify = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    private EmployerAddress blankEmployerAddress;
    private Common common;
    private DateWorkArea max;
    private DateWorkArea nullDateWorkArea;
    private Common cuFound;
    private Common numberOfApAndArRoles;
    private NextTranInfo nullNextTranInfo;
    private Array<LocalGroup> local1;
    private Common selectionsNumber;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea maxDate;
    private CsePerson csenet;
    private Common screenIdentification;
    private MonitoredDocument monitoredDocument;
    private Document document;
    private SpDocKey spDocKey;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private WorkArea print;
    private Common position;
    private SpDocLiteral spDocLiteral;
    private DateWorkArea default1;
    private DateWorkArea current;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private CsePersonResource blankCsePersonResource;
    private CodeValue codeValue;
    private Code code;
    private Common validValue;
    private Employer blankEmployer;
    private IncomeSourceContact blankIncomeSourceContact;
    private NonEmployIncomeSourceAddress blankNonEmployIncomeSourceAddress;
    private IncomeSource blankIncomeSource;
    private PersonIncomeHistory blankPersonIncomeHistory;
    private Common incSourceRecordResult;
    private Common invalidSel;
    private Common moreThanOneSrcRecord;
    private Case1 page;
    private AbendData abendData;
    private Common sendDateUpdated;
    private Infrastructure infrastructure;
    private CsePerson ap;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private TextWorkArea detailText30;
    private DateWorkArea date;
    private TextWorkArea detailText10;
    private InterfaceAlert interfaceAlert;
    private Infrastructure lastTran;
    private WorkArea closeMonitoredDoc;
    private Common save;
    private Common checkZip;
    private DateWorkArea todayPlus6Months;
    private Common emailVerify;
    private Common currentPosition;
    private TextWorkArea postion;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Ssa.
    /// </summary>
    [JsonPropertyName("ssa")]
    public Employer Ssa
    {
      get => ssa ??= new();
      set => ssa = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Re.
    /// </summary>
    [JsonPropertyName("re")]
    public IncomeSource Re
    {
      get => re ??= new();
      set => re = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private EmployerAddress employerAddress;
    private Employer ssa;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CaseRole caseRole;
    private Employer employer;
    private IncomeSource incomeSource;
    private IncomeSource re;
    private CsePersonResource csePersonResource;
    private CsePerson csePerson;
  }
#endregion
}
