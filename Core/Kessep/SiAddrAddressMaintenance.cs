// Program: SI_ADDR_ADDRESS_MAINTENANCE, ID: 371735324, model: 746.
// Short name: SWEADDRP
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
/// A program: SI_ADDR_ADDRESS_MAINTENANCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes
/// addresses for APs and ARs.  It also allows requests for postmaster letters 
/// and updates any information pertaining to these letters.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiAddrAddressMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ADDR_ADDRESS_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAddrAddressMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAddrAddressMaintenance.
  /// </summary>
  public SiAddrAddressMaintenance(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------
    //       M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 08-01-95  H Sharland		Initial development
    // 09-06-95  K Evans		Continue development
    // 05-01-96  R Mulpuri		Changes to Prompts
    // 				IDCR# 127 & 132
    // 09/22/96  G. Lofton - MTW	Add county code IDCR# 210
    // 11/04/96  G. Lofton - MTW	Add new security.
    // 11/24/96  G. Lofton - MTW	Create a infrastructure rec
    // 				when send date changes.
    // 01/02/97  Raju      - MTW	event insertion code
    // 01/21/97  Raju	    - MTW	next tran code
    // 01/23/97  Raju	    - MTW	close monitored document
    // 06/05/97  Sid			Cleanup and fixes.
    // 06/09/97  M. D. Wheaton         Delete Datenum Function
    // 07/02/97  Sid			Modify Event Generation code.
    // -------------------------------------------------------------------------
    // 12/17/98  W.Campbell            Added an
    //                                 
    // IF - ELSE stmt so that on
    //                                 
    // a DISPLAY command the
    //                                 
    // EXPORT_EMPTY_ADDR views
    //                                 
    // will not be populated from
    //                                 
    // the import view.
    //                                 
    // Work done on IDCR454.
    // -------------------------------------------------------------------------
    // 12/17/98  W.Campbell            Made several changes
    //                                 
    // to the screen and logic
    // including:
    //                                 
    // Removed START_DATE and
    //                                 
    // VERIFIED_CODE from the Screen
    //                                 
    // and logic.  Added LAST_UPDATE
    //                                 
    // timestamp to the screen and
    // logic.
    //                                 
    // START_DATE and VERIFIED_CODE
    //                                 
    // are to be removed from the model
    //                                 
    // under IDCR454.
    // -------------------------------------------------------------------------
    // 12/21/98  W.Campbell            Logic to prevent
    //                                 
    // ADD, UPDATE or COPY
    //                                 
    // on a closed CASE or an
    //                                 
    // inactive AP was disabled.
    //                                 
    // Work done on IDCR454.
    // -------------------------------------------------------------------------
    // 12/21/98 W.Campbell             Logic added to
    //                                 
    // validate that the user has
    //                                 
    // entered one or more of
    //                                 
    // SEND DATE, VERIFIED DATE
    //                                 
    // or END DATE on an ADD
    //                                 
    // or UPDATE.
    //                                 
    // Work done on IDCR454.
    // -------------------------------------------------------------------------
    // 12/21/98 W.Campbell             Logic to prevent
    //                                 
    // input of VERIFIED DATE in
    //                                 
    // the future was disabled.
    //                                 
    // Work done on IDCR454.
    // -------------------------------------------------------------------------
    // 01/04/99 W.Campbell             Logic added to make
    //                                 
    // sure END CODE is entered
    //                                 
    // with END DATE.
    //                                 
    // Work done on IDCR454.
    // -------------------------------------------------------------------------
    // 02/06/1999 M Ramirez		Added creation of document
    // 				trigger.
    // -------------------------------------------------------------------------
    // 02/18/99 M Ramirez & W.Campbell Disabled statements
    //                                 
    // dealing with closing monitored
    //                                 
    // documents, as it has been
    //                                 
    // determined that the best way
    //                                 
    // to handle them will be in Batch.
    // -------------------------------------------------------------------------
    // 02/22/98 W.Campbell             Added else statement
    //                                 
    // to ensure that the AP
    //                                 
    // select_char is set to 'S'
    //                                 
    // when a new CASE is displayed,
    //                                 
    // if needed.
    // -------------------------------------------------------------------------
    // 03/29/99 W.Campbell             Added attribute to
    //                                 
    // local sp_doc_key view for the
    //                                 
    // address identifier and a set
    //                                 
    // statement to set the attribute
    //                                 
    // to the address identifier for
    //                                 
    // passing it to the called CAB -
    //                                 
    // SP_CREATE_DOCUMENT_INFRASTRUCT.
    //                                 
    // This was to fix a problem with
    //                                 
    // producing the POSTMASTER
    //                                 
    // letter.
    // -------------------------------------------------------------------------
    // 05/03/99 W.Campbell             Added code to send
    //                                 
    // selection needed msg to COMP.
    //                                 
    // BE SURE TO MATCH
    //                                 
    // next_tran_info ON THE
    //                                 
    // DIALOG FLOW.
    // ---------------------------------------------------------------------------
    // 05/12/99 W.Campbell             Added
    //                                 
    // USE eab_rollback_cics statement
    //                                 
    // to rollback DB/2 updates.  See
    //                                 
    // this note in code.
    // -------------------------------------------------------------------------
    // 05/19/99 W.Campbell             Added code to
    //                                 
    // process interstate event
    //                                 
    // for either AP address
    //                                 
    // located with state code = KS
    //                                 
    // (Kansas) (LSADR)
    //                                  
    // - OR -
    //                                 
    // for AP address located
    //                                 
    // with state code NOT = KS
    //                                 
    // (Kansas) (LSOUT).
    // -------------------------------------------------------------------------
    // 05/19/99 W.Campbell             Added code to
    //                                 
    // process interstate event
    //                                 
    // for AR address located (GSPAD).
    // -------------------------------------------------------------------------
    // 05/20/99 W.Campbell             Replaced
    //                                 
    // ZDelete exit states.
    // -------------------------------------------------------------------------
    // 05/20/99 W.Campbell             Inserted logic to save
    //                                 
    // and restore the exit state
    //                                 
    // when dealing with
    //                                 
    // interstate (CSENET) processing.
    // -------------------------------------------------------------------------
    // 06/17/99 W.Campbell             Disabled one line
    //                                 
    // of code and replaced it
    //                                 
    // with statements to reinitialize
    //                                 
    // the text_1 attribute to contain
    //                                 
    // the correct value of "P" or "R"
    //                                 
    // for AR or AP based on the
    //                                 
    // input values.  This bug was
    //                                 
    // causing a problem with
    //                                 
    // generating events correctly
    //                                 
    // for both normal event
    //                                 
    // processing and CSENET
    //                                 
    // event processing.
    // -------------------------------------------------------------------------
    // 07/20/99 M.Lachowicz            Add address validation for COPY command.
    // ---------------------------------------------
    // 08/06/99 M.Lachowicz            Place '*' for every processed row.
    // -------------------------------------------------------------------------
    // 08/06/99 M.Lachowicz            Allow to process one row only.
    // -------------------------------------------------------------------------
    // 09/14/99 M.Lachowicz            Fix problem which causes that
    //                                 
    // event Address AP located in
    // other
    //                                 
    // state' is raised when
    // confirmation
    //                                 
    // date is not entered.
    //                                 
    // Problem RPT # - H00073534
    // -------------------------------------------------------------------------
    // 11/01/99 M.Lachowicz            Fix problem which causes that
    //                                 
    // more events are raised than
    //                                 
    // necessary .
    //                                 
    // Problem RPT # - H00077709.
    // -------------------------------------------------------------------------
    // 11/02/99 M.Lachowicz            Fix problem which causes that
    //                                 
    // wrong message is displayed for
    //                                 
    // raised event.
    //                                 
    // Problem RPT # - H00078537.
    // -------------------------------------------------------------------------
    // 11/08/99 M Ramirez		Changed POSTMAST from a batch document
    // 				to an online document.
    //                                 
    // Problem RPT # - xxxxx.
    // Revised the printing process to check for the following things:
    // - send date can only be updated to CURRENT DATE or NULL DATE
    // - Print on ADD or UPDATE
    // - No document for an AR if the AR is an Organization.
    // Also, effecting other action blocks:
    // - Activity Start Stop needs to be implemented for documents 
    // infrastructure, due
    //   to one being attached to the POSTMAST event detail
    //   (this will be part of another PR)
    // - For closed cases, the SP on the POSTMAST must be a Recovery worker.
    // -------------------------------------------------------------------------
    // 01/14/2000 M Ramirez		Clear NEXT TRAN before invoking
    // 				print process
    //                                 
    // Problem RPT # - 83300.
    // 01/27/00	PMcElderry	PR84721- CSEnet action code
    // 				LSADR not attaching correct address info
    // -------------------------------------------------------------------------
    // 02/18/00 M.Lachowicz            Added new event which is
    //                                 
    // generated when verified date is
    // blank
    //                                 
    // out.
    //                                 
    // Problem RPT # - H00085883.
    // -------------------------------------------------------------------------
    // 02/22/00 M.Lachowicz            Verified date can not be
    //                                 
    // greater than end date.
    //                                 
    // Problem RPT # - H00088799.
    // -------------------------------------------------------------------------
    // 03/29/00 W.Campbell             Changed view matching
    //                                 
    // for the USE of
    //                                 
    // SC_CAB_TEST_SECURITY.
    //                                 
    // Changed view matching for
    //                                 
    // the cab's inport case to the
    //                                 
    // Pstep's export_next case.
    //                                 
    // It previously was to the Pstep's
    //                                 
    // inport case.  Work done on
    //                                 
    // WR#000162 for PRWORA
    //                                 
    // Family Violence Indicator.
    // ---------------------------------------------
    // 07/03/00 M.Lachowicz           It should be able
    //                                
    // to sign off on cases with FV.
    //                                
    // PR #98338.
    // -------------------------------------------------------------------------
    // 07/11/00 M.Lachowicz           Do not allow
    //                                
    // to add/update address of
    //                                
    // organization.
    //                                
    // PR #90510.
    // -------------------------------------------------------------------------
    // 07/12/00 M.Lachowicz           Clear screen fo FV.
    //                                
    // PR #98962.
    // -------------------------------------------------------------------------
    // 07/20/00 M.Lachowicz           Call CSNET any time
    //                                
    // when valid address is changed..
    //                                
    // PR #91860.
    // -------------------------------------------------------------------------
    // 08/31/00 W.Campbell            Modified NEXTTRAN logic
    //                                
    // so that it will also pass the
    // selected
    //                                
    // person number via the nexttran
    // view.
    //                                
    // Also  modified NEXTTRAN logic
    //                                
    // so that a 'normal' nexttran
    // coming
    //                                
    // from HIST or MONA (CICS
    // trancodes
    //                                
    // SRPT and SRPU) will work
    // correctly.
    //                                
    // Work done on WR #000193-A.
    // -------------------------------------------------------------------------
    // 10/25/00 M.Lachowicz           Removed address from
    //                                
    // infrastructure record.
    // -------------------------------------------------------------------------
    // 11/06/00 M.Lachowicz           Unable to add address on
    //                                
    // ADDR. PR106827.
    // -------------------------------------------------------------------------
    // 11/17/00 M.Lachowicz           Changed business_object_cd
    //                                
    // to 'CAS'.  WR246.
    // -------------------------------------------------------------------------
    // 11/22/00 M.Lachowicz           Changed header line.
    //                                
    // WR298.
    // -------------------------------------------------------------------------
    // 01/02/01 SWSRCHF 000238        Added check for "SRPQ" (ALRT) to return 
    // logic
    // -------------------------------------------------------------------------
    // 2/21/01 M.Lachowicz            Do not display address
    //                                
    // if particular person has family
    // violence.
    //                                
    // PR106233.
    // -------------------------------------------------------------------------
    // 04/09/01 SWSRCHF I00116862    When arriving at ADDR from ALRT save the
    //                               
    // NEXT_TRAN_INFO. Reset
    // NEXT_TRAN_INFO to it's
    //                               
    // saved values prior to
    // returning to ALRT.
    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Per WR# 261, when a Prison/Jail record was added on JAIL screen for a 
    // person, that address will be populated on ADDR screen. On ADDR screen the
    // 'Send_Dt' for that Prison/Jail address must be protected and a message :
    // " Not able to send PM letter on a Prison/Jail address", will be
    // displayed on the ADDR screen,
    //                                                 
    // Vithal Madhira (04/09/2001)
    // ------------------------------------------------------------------------------------
    // 04/11/02 M.Lachowicz           Do not allow future date greater than
    //                                
    // current date plus 6 months.
    // WR283.
    // ---------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------------------
    // 02/26/2007	Raj S		PR 00302259 	Modified to fix the view overflow problem 
    // by increasing
    //                                                 
    // the screen page group view IMPORT_GROUP_PAGENUM
    // and
    //                                                 
    // EXPORT_GROUP_PAGENUM from 8 to 20.
    // 04/27/2007	LSS		PR 00209846	Added new business edit for zip4.
    // ------------------------------------------------------------------------------------------------------
    // 10/17/2007      LSS	  PR 00309441 / CQ579   Added MOVE statement to Case
    // "NEXT" and Case "PREV"
    //                                                 
    // so that address records get populated when
    // scroll the
    // 					        pages with the associated PF Keys on the ADDR screen.
    // ------------------------------------------------------------------------------------------------------
    // 12/07/2007      M. Fan	  PR 00187149 / CQ416   Made dialog flow view 
    // mapping change for 'ADDR to FADS'.
    // ------------------------------------------------------------------------------------------------------
    // 02/19/2008   LSS   PR327278 / CQ2628            Added MOVE statement to 
    // initialize AE address when there is
    //                                                 
    // family violence because it Escapes instead of
    // going into the build
    //                                                 
    // address action block where it would otherwise
    // get initialized.
    // --------------------------------------------------------------------------------------------------------------------------------------------------
    // 09/17/2008     Arun Mathias          CQ#6285    When the State / Zip code
    // is changed, populate the associated county for that zip code
    // --------------------------------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    UseSpDocSetLiterals();

    // --------------------------------------------
    // Start of Code (RAJU 01/23/97:11:55 hrs CST)
    // --------------------------------------------
    // ---------------------------------------------
    // 02/18/99 M Ramirez & W.Campbell
    // Disabled statements
    // dealing with closing monitored documents,
    // as it has been determined that the best way
    // to handle them will be in Batch.
    // ---------------------------------------------
    local.Addr4CloseDoc.Assign(local.BlankAddr4CloseDoc);

    // --------------------------------------------
    // End of Code
    // --------------------------------------------
    // 04/11/02 M.L Start
    local.TodayPlus6Months.Date = AddMonths(local.Current.Date, 6);

    // 04/11/02 M.L End
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Alrt.Assign(import.Alrt);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    export.HiddenNext.Number = import.HiddenNext.Number;
    export.ApCommon.SelectChar = import.ApCommon.SelectChar;
    export.ArCommon.SelectChar = import.ArCommon.SelectChar;
    export.ApActive.Flag = import.ApActive.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.Case1.Number = import.Case1.Number;
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    MoveCsePersonsWorkSet(import.ArFromCaseRole, export.ArFromCaseRole);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.HiddenAp.Number = import.HiddenAp.Number;

    // ---------------------------------------------
    // 12/17/98 W.Campbell - Added the following
    // IF - ELSE stmt so that on a DISPLAY command
    // the EXPORT_EMPTY_ADDR views will not be
    // populated from the import view.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.EmptyAddrSelect.SelectChar = import.EmptyAddrSelect.SelectChar;
      export.EmptyAddr.Assign(import.EmptyAddr);
      export.MtEnddtCdPrmt.Text1 = import.MtEnddtCdPrmt.Text1;
      export.MtSrcePrmt.Text1 = import.MtSrcePrmt.Text1;
      export.MtStatePrmt.Text1 = import.MtStatePrmt.Text1;
      export.MtCntyPrmt.Text1 = import.MtCntyPrmt.Text1;
      export.MtTypePrmt.Text1 = import.MtTypePrmt.Text1;
    }

    export.AeAddr.Assign(import.AeAddr);
    export.Minus.Text1 = import.Minus.Text1;
    export.Plus.Text1 = import.Plus.Text1;
    export.SaveSubscript.Subscript = import.SaveSubscript.Subscript;
    MoveCsePersonAddress10(import.LastAddr, export.LastAddr);
    export.Save.Number = import.Save.Number;
    export.PromptCode.CodeName = import.PromptCode.CodeName;
    MoveCodeValue(import.PromptCodeValue, export.PromptCodeValue);
    export.ForeignAddress.AddressLine1 = import.ForeignAddress.AddressLine1;
    export.ForeignAddr.Text1 = import.ForeignAddr.Text1;
    MoveStandard(import.Standard, export.Standard);
    export.ApList.Text1 = import.ApList.Text1;
    export.ArList.Text1 = import.ArList.Text1;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;

    // 11/22/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/22/00 M.L End
    if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.DetCommon.SelectChar =
          import.Group.Item.DetCommon.SelectChar;
        export.Group.Update.DetCsePersonAddress.Assign(
          import.Group.Item.DetCsePersonAddress);
        export.Group.Update.EnddtCdPrmt.Text1 =
          import.Group.Item.EnddtCdPrmt.Text1;
        export.Group.Update.SrcePrmt.Text1 = import.Group.Item.SrcePrmt.Text1;
        export.Group.Update.StatePrmt.Text1 = import.Group.Item.StatePrmt.Text1;
        export.Group.Update.CntyPrmt.Text1 = import.Group.Item.CntyPrmt.Text1;
        export.Group.Update.TypePrmt.Text1 = import.Group.Item.TypePrmt.Text1;

        // ---------------------------------------------
        // Start of code - Raju 01/02/1997 0800 hrs CST
        // ---------------------------------------------
        export.Group.Update.HiddenDet.Assign(import.Group.Item.HiddenDet);

        // ---------------------------------------------
        // End of code
        // ---------------------------------------------
      }

      import.Group.CheckIndex();
    }

    if (!import.Hidden.IsEmpty)
    {
      for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
        import.Hidden.Index)
      {
        if (!import.Hidden.CheckSize())
        {
          break;
        }

        export.Hidden.Index = import.Hidden.Index;
        export.Hidden.CheckSize();

        export.Hidden.Update.HsendDate.SendDate =
          import.Hidden.Item.HsendDate.SendDate;
      }

      import.Hidden.CheckIndex();
    }

    if (!import.Pagenum.IsEmpty)
    {
      for(import.Pagenum.Index = 0; import.Pagenum.Index < import
        .Pagenum.Count; ++import.Pagenum.Index)
      {
        if (!import.Pagenum.CheckSize())
        {
          break;
        }

        export.Pagenum.Index = import.Pagenum.Index;
        export.Pagenum.CheckSize();

        MoveCsePersonAddress10(import.Pagenum.Item.LastAddr,
          export.Pagenum.Update.LastAddr);
      }

      import.Pagenum.CheckIndex();
    }

    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;

      // -------------------------------------------------------------------------
      // 08/31/00 W.Campbell - Modified NEXTTRAN logic
      // so that it will also pass the selected
      // person number via the nexttran view.
      // Work done on WR #000193-A.
      // -------------------------------------------------------------------------
      if (AsChar(export.ApCommon.SelectChar) == 'S')
      {
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ApCsePersonsWorkSet.Number;
        export.HiddenNextTranInfo.CsePersonNumberAp =
          export.ApCsePersonsWorkSet.Number;
      }
      else if (AsChar(export.ArCommon.SelectChar) == 'S')
      {
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ApCsePersonsWorkSet.Number;
      }

      // -------------------------------------------------------------------------
      // 08/31/00 W.Campbell - End of Modified NEXTTRAN
      // logic so that it will also pass the selected
      // person number via the nexttran view.
      // Work done on WR #000193-A.
      // -------------------------------------------------------------------------
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ApCsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      // *** Problem report I00116862
      // *** 04/09/01 swsrchf
      // *** start
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
      {
        export.Alrt.Assign(export.HiddenNextTranInfo);
      }

      // *** end
      // *** 04/09/01 swsrchf
      // *** Problem report I00116862
      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1035 hrs CST)
      // ---------------------------------------------
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

        if (ReadCaseRole())
        {
          switch(TrimEnd(entities.CaseRole.Type1))
          {
            case "AP":
              export.ApCsePersonsWorkSet.Number =
                local.LastTran.CsePersonNumber ?? Spaces(10);

              break;
            case "AR":
              export.ArCsePersonsWorkSet.Number =
                local.LastTran.CsePersonNumber ?? Spaces(10);

              break;
            default:
              break;
          }
        }

        export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
      }

Test1:

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    // mjr
    // ------------------------------------------------
    // 11/09/1999
    // Changed security to only check crud actions
    // ------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "COPY") || Equal(global.Command, "UPDATE"))
    {
      // ---------------------------------------------
      // 03/29/00 W.Campbell - Changed view matching
      // for the USE of SC_CAB_TEST_SECURITY.
      // Changed view matching for the cab's inport case
      // to the Pstep's export_next case.  It previously
      // was to the Pstep's inport case.  Work done on
      // WR#000162 for PRWORA Family Violence Indicator.
      // ---------------------------------------------
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // 02/21/01 M.L Start
        if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
        {
          if (Equal(global.Command, "DISPLAY"))
          {
            local.FvCheck.Flag = "Y";
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test2;
          }
          else
          {
            if (AsChar(export.ArCommon.SelectChar) == 'S')
            {
              local.FvTest.Number = export.ArCsePersonsWorkSet.Number;
            }
            else
            {
              local.FvTest.Number = export.ApCsePersonsWorkSet.Number;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            UseScSecurityValidAuthForFv1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          goto Test2;
        }

        // 02/21/01 M.L End
        // 07/12/00 M.L Start
        export.ApCsePersonsWorkSet.Number = "";
        export.ApCsePersonsWorkSet.FormattedName = "";
        export.ArCsePersonsWorkSet.Number = "";
        export.ArCsePersonsWorkSet.FormattedName = "";
        export.ArCommon.SelectChar = "";
        export.ApCommon.SelectChar = "";
        export.Group.Count = 0;

        // 07/12/00 M.L End
        return;
      }
    }

Test2:

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ---------------------------------------------
    // 12/21/98 W.Campbell - Logic to prevent
    // ADD, UPDATE or COPY on a closed CASE
    // or an inactive AP was disabled.
    // Work done on IDCR454.
    // ---------------------------------------------
    // 08/06/99  M.L  Error if both AP and AR are selected
    if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
      (export.ArCommon.SelectChar) == 'S')
    {
      var field1 = GetField(export.ApCommon, "selectChar");

      field1.Error = true;

      var field2 = GetField(export.ArCommon, "selectChar");

      field2.Error = true;

      ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

      return;
    }

    // 08/06/99  M.L  End
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Group.Item.DetCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Select.Count;

          break;
        case '*':
          export.Group.Update.DetCommon.SelectChar = "";

          break;
        default:
          var field = GetField(export.Group.Item.DetCommon, "selectChar");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Group.Item.TypePrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Group.Item.TypePrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Group.Item.EnddtCdPrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Group.Item.EnddtCdPrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Group.Item.SrcePrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Group.Item.SrcePrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }

      switch(AsChar(export.Group.Item.StatePrmt.Text1))
      {
        case 'S':
          ++local.Prmpt.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Group.Item.StatePrmt, "text1");

          field.Error = true;

          ++local.Error.Count;

          break;
      }
    }

    export.Group.CheckIndex();

    if (local.Error.Count > 0)
    {
      // ---------------------------------------------
      // 05/20/99 W.Campbell - Replaced
      // ZDelete exit states.
      // ---------------------------------------------
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

    if (local.Prmpt.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

      return;
    }

    if (local.Select.Count > 0 && (Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    // 08/06/99 M.L Start
    if (local.Select.Count > 1 && (Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE")))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.DetCommon.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.DetCommon, "selectChar");

          field.Error = true;
        }
      }

      export.Group.CheckIndex();
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    // 08/06/99 M.L End
    // 07/03/2000 M.L Start
    // Do not check if command is sign off.
    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      // Pseudo return to HIST, MONA or ALRT. User had pressed PF15 Detail to 
      // flow here and wants to return
      // *** Work Request 000238
      // *** 01/02/01 swsrchf
      // *** added check for "SRPQ"
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPU") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPQ"))
      {
        // *** Problem report I00116862
        // *** 04/09/01 swsrchf
        // *** start
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPQ"))
        {
          export.HiddenNextTranInfo.Assign(export.Alrt);
        }

        // *** end
        // *** 04/09/01 swsrchf
        // *** Problem report I00116862
        global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
          ;

        return;
      }
      else
      {
        ExitState = "ACO_NE0000_RETURN";

        return;
      }
    }

    // 07/03/2000 M.L End
    if (!Equal(export.HiddenNext.Number, export.Next.Number))
    {
      if (IsEmpty(export.Next.Number))
      {
        export.Next.Number = import.HiddenNext.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        // ----------------------------------------
        // WR292
        // Beginning of code
        // Flow to ADDR screen with Returned Address.
        // ------------------------------------------
        if (Equal(global.Command, "FPLS"))
        {
          if (!IsEmpty(export.ApCsePersonsWorkSet.Number) && !
            IsEmpty(export.Next.Number) && !IsEmpty(export.EmptyAddr.City) && !
            IsEmpty(export.EmptyAddr.State) && !
            IsEmpty(export.EmptyAddr.ZipCode))
          {
            export.EmptyAddr.Type1 = "M";
            local.FplsDisplayFlag.Flag = "Y";
            global.Command = "DISPLAY";

            goto Test3;
          }
          else
          {
            // ---------------------------------------------------
            // WR # 292
            // If it is 'F' type address format then only
            // Zip code is passed thru FPLS screen.
            // ---------------------------------------------------
            if (!IsEmpty(export.EmptyAddr.ZipCode))
            {
              export.EmptyAddr.Type1 = "M";
              local.FplsDisplayFlag.Flag = "Y";
              global.Command = "DISPLAY";

              goto Test3;
            }
            else
            {
              global.Command = "DISPLAY";

              goto Test3;
            }
          }
        }

        // ----------------------------------------
        // WR292
        // End of code.
        // ------------------------------------------
        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
      else if (Equal(export.ApCsePersonsWorkSet.Number, export.HiddenAp.Number))
      {
        export.ApCsePersonsWorkSet.Number = "";
        export.HiddenAp.Number = "";
      }
    }

Test3:

    if (!Equal(export.HiddenAp.Number, export.ApCsePersonsWorkSet.Number))
    {
      if (IsEmpty(export.ApCsePersonsWorkSet.Number))
      {
        export.ApCsePersonsWorkSet.Number = export.HiddenAp.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        // ----------------------------------------
        // WR292
        // Beginning of code
        // Flow to ADDR screen with Returned Address.
        // ------------------------------------------
        if (Equal(global.Command, "FPLS"))
        {
          if (!IsEmpty(export.ApCsePersonsWorkSet.Number) && !
            IsEmpty(export.Next.Number) && !IsEmpty(export.EmptyAddr.City) && !
            IsEmpty(export.EmptyAddr.State) && !
            IsEmpty(export.EmptyAddr.ZipCode))
          {
            export.EmptyAddr.Type1 = "M";
            local.FplsDisplayFlag.Flag = "Y";
            global.Command = "DISPLAY";

            goto Test4;
          }
          else
          {
            // ---------------------------------------------------
            // WR # 292
            // If it is 'F' type address format then only
            // Zip code is passed thru FPLS screen.
            // ---------------------------------------------------
            if (!IsEmpty(export.EmptyAddr.ZipCode))
            {
              export.EmptyAddr.Type1 = "M";
              local.FplsDisplayFlag.Flag = "Y";
              global.Command = "DISPLAY";

              goto Test4;
            }
            else
            {
              global.Command = "DISPLAY";

              goto Test4;
            }
          }
        }

        // ----------------------------------------
        // WR292
        // End of code.
        // ------------------------------------------
        var field = GetField(export.ApCsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

Test4:

    // mjr
    // -----------------------------------------------------
    // 11/09/1999
    // Added for after the Print process has completed.
    // ------------------------------------------------------------------
    if (Equal(global.Command, "PRINTRET"))
    {
      // mjr
      // -----------------------------------------------
      // 11/09/1999
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
      export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
      local.Position.Count =
        Find(String(
          export.HiddenNextTranInfo.MiscText1,
        NextTranInfo.MiscText1_MaxLength),
        TrimEnd(local.SpDocLiteral.IdPrNumber));

      if (local.Position.Count > 0)
      {
        local.SpDocKey.KeyPerson =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          7, 10);
      }

      local.Position.Count =
        Find(String(
          export.HiddenNextTranInfo.MiscText1,
        NextTranInfo.MiscText1_MaxLength),
        TrimEnd(local.SpDocLiteral.IdPersonAddress));

      if (local.Position.Count > 0)
      {
        local.BatchTimestampWorkArea.TextTimestamp =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          6, 26);
        local.Position.Count =
          Verify(local.BatchTimestampWorkArea.TextTimestamp, "0123456789.-");

        if (local.Position.Count <= 0)
        {
          local.Asterisk.Identifier =
            Timestamp(local.BatchTimestampWorkArea.TextTimestamp);
        }
      }

      export.ApCsePersonsWorkSet.Number =
        export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);

      if (Equal(export.ApCsePersonsWorkSet.Number, local.SpDocKey.KeyPerson))
      {
        export.ApCommon.SelectChar = "S";
        export.ArCommon.SelectChar = "";
      }
      else
      {
        export.ApCommon.SelectChar = "";
        export.ArCommon.SelectChar = "S";
      }

      export.HiddenNext.Number = export.Next.Number;
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        export.Case1.Number = export.Next.Number;
        UseSiReadCaseHeaderInformation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            if (AsChar(export.ArCommon.SelectChar) != 'S')
            {
              export.ApCommon.SelectChar = "";
              export.ArCommon.SelectChar = "S";
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            return;
          }
        }
        else
        {
          // --------------------------------
          // 02/22/98 W.Campbell - Added else statement
          // to ensure that the AP select_char is set to 'S'
          // when a new CASE is displayed, if needed.
          // --------------------------------
          if (!Equal(export.HiddenNext.Number, export.Next.Number))
          {
            // --------------------------------
            // The user wants a new case displayed
            // therefore reset for the AP to be
            // selected for display by default,
            // if the new case has an AP.
            // --------------------------------
            if (!IsEmpty(export.ApCsePersonsWorkSet.Number))
            {
              export.ApCommon.SelectChar = "S";
              export.ArCommon.SelectChar = "";
            }
          }
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y' && IsEmpty
          (export.ApCsePersonsWorkSet.Number))
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.ArFromCaseRole.Number))
        {
          if (Equal(export.Next.Number, export.HiddenNext.Number))
          {
            UseSiReadArInformation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            export.ArFromCaseRole.Number = "";
          }
        }

        export.ApList.Text1 = "";
        export.ArList.Text1 = "";
        export.HiddenNext.Number = export.Next.Number;
        export.HiddenAp.Number = export.ApCsePersonsWorkSet.Number;

        switch(AsChar(export.ArCommon.SelectChar))
        {
          case 'S':
            break;
          case ' ':
            break;
          default:
            var field = GetField(export.ArCommon, "selectChar");

            field.Error = true;

            // ---------------------------------------------
            // 05/20/99 W.Campbell - Replaced
            // ZDelete exit states.
            // ---------------------------------------------
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(export.ApCommon.SelectChar))
        {
          case 'S':
            break;
          case ' ':
            break;
          default:
            var field = GetField(export.ApCommon, "selectChar");

            field.Error = true;

            // ---------------------------------------------
            // 05/20/99 W.Campbell - Replaced
            // ZDelete exit states.
            // ---------------------------------------------
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            export.Group.Update.HiddenDet.EndDate = local.Zero.Date;
            export.Group.Update.HiddenDet.VerifiedDate = local.Zero.Date;
            export.Group.Update.HiddenDet.State = "";
          }

          export.Group.CheckIndex();

          return;
        }
        else
        {
          local.BuildAddressList.Flag = "Y";
          export.Minus.Text1 = "";

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            export.Group.Update.HiddenDet.Assign(
              export.Group.Item.DetCsePersonAddress);
          }

          export.Group.CheckIndex();
        }

        break;
      case "NEXT":
        if (IsEmpty(import.Plus.Text1))
        {
          // -------------------------------------------------------------------------------------
          // Per WR# 261, when a Prison/Jail record was added on JAIL screen for
          // a person, that address will be populated on ADDR screen. On ADDR
          // screen the 'Send_Dt' for that Prison/Jail address must be protected
          // and a message :  " Not able to send PM letter on a Prison/Jail
          // address", will be displayed on the ADDR screen,
          //                                                 
          // Vithal Madhira (04/09/2001)
          // ------------------------------------------------------------------------------------
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
            {
              var field1 =
                GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DetCsePersonAddress, "source");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

              field3.Color = "cyan";
              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.Standard.PageNumber;

        export.Pagenum.Index = export.Standard.PageNumber - 1;
        export.Pagenum.CheckSize();

        MoveCsePersonAddress10(export.Pagenum.Item.LastAddr, export.LastAddr);
        local.Command.Command = global.Command;
        UseSiBuildAddresses2();

        if (export.Group.IsFull)
        {
          export.Plus.Text1 = "+";

          ++export.Pagenum.Index;
          export.Pagenum.CheckSize();

          export.Group.Index = Export.GroupGroup.Capacity - 2;
          export.Group.CheckSize();

          export.Pagenum.Update.LastAddr.Identifier =
            export.Group.Item.DetCsePersonAddress.Identifier;
          export.Pagenum.Update.LastAddr.EndDate =
            export.Group.Item.DetCsePersonAddress.EndDate;
        }
        else
        {
          export.Plus.Text1 = "";
        }

        export.Minus.Text1 = "-";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.DetCsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Group.Update.DetCsePersonAddress.EndDate = local.Zero.Date;
          }

          export.Hidden.Index = export.Group.Index;
          export.Hidden.CheckSize();

          export.Hidden.Update.HsendDate.SendDate =
            export.Group.Item.DetCsePersonAddress.SendDate;

          // ------------------------------------------------------------------------------------------------------
          // 10/17/2007      LSS       PR 00309441 / CQ579
          // ------------------------------------------------------------------------------------------------------
          export.Group.Update.HiddenDet.Assign(
            export.Group.Item.DetCsePersonAddress);
        }

        export.Group.CheckIndex();

        // -------------------------------------------------------------------------------------
        // Per WR# 261, when a Prison/Jail record was added on JAIL screen for a
        // person, that address will be populated on ADDR screen. On ADDR
        // screen the 'Send_Dt' for that Prison/Jail address must be protected
        // and a message :  " Not able to send PM letter on a Prison/Jail
        // address", will be displayed on the ADDR screen,
        //                                                 
        // Vithal Madhira (04/09/2001)
        // ------------------------------------------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
          {
            var field1 =
              GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.DetCsePersonAddress, "source");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

            field3.Color = "cyan";
            field3.Protected = true;

            ExitState = "ACO_NI0000_NO_PM_LETTER";
          }
        }

        export.Group.CheckIndex();

        break;
      case "PREV":
        if (IsEmpty(import.Minus.Text1))
        {
          // -------------------------------------------------------------------------------------
          // Per WR# 261, when a Prison/Jail record was added on JAIL screen for
          // a person, that address will be populated on ADDR screen. On ADDR
          // screen the 'Send_Dt' for that Prison/Jail address must be protected
          // and a message :  " Not able to send PM letter on a Prison/Jail
          // address", will be displayed on the ADDR screen,
          //                                                 
          // Vithal Madhira (04/09/2001)
          // ------------------------------------------------------------------------------------
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
            {
              var field1 =
                GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.DetCsePersonAddress, "source");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

              field3.Color = "cyan";
              field3.Protected = true;
            }
          }

          export.Group.CheckIndex();

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Standard.PageNumber;

        export.Pagenum.Index = export.Standard.PageNumber - 1;
        export.Pagenum.CheckSize();

        MoveCsePersonAddress10(export.Pagenum.Item.LastAddr, export.LastAddr);
        local.Command.Command = global.Command;
        UseSiBuildAddresses2();

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.Text1 = "-";
        }
        else
        {
          export.Minus.Text1 = "";
        }

        export.Plus.Text1 = "+";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.DetCsePersonAddress.EndDate,
            local.Maximum.Date))
          {
            export.Group.Update.DetCsePersonAddress.EndDate = local.Zero.Date;
          }

          export.Hidden.Index = export.Group.Index;
          export.Hidden.CheckSize();

          export.Hidden.Update.HsendDate.SendDate =
            export.Group.Item.DetCsePersonAddress.SendDate;

          // ------------------------------------------------------------------------------------------------------
          // 10/17/2007      LSS       PR 00309441 / CQ579
          // ------------------------------------------------------------------------------------------------------
          export.Group.Update.HiddenDet.Assign(
            export.Group.Item.DetCsePersonAddress);
        }

        export.Group.CheckIndex();

        // -------------------------------------------------------------------------------------
        // Per WR# 261, when a Prison/Jail record was added on JAIL screen for a
        // person, that address will be populated on ADDR screen. On ADDR
        // screen the 'Send_Dt' for that Prison/Jail address must be protected
        // and a message :  " Not able to send PM letter on a Prison/Jail
        // address", will be displayed on the ADDR screen,
        //                                                 
        // Vithal Madhira (04/09/2001)
        // ------------------------------------------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
          {
            var field1 =
              GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.DetCsePersonAddress, "source");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

            field3.Color = "cyan";
            field3.Protected = true;

            ExitState = "ACO_NI0000_NO_PM_LETTER";
          }
        }

        export.Group.CheckIndex();

        break;
      case "ADD":
        // ---------------------------------------------
        // Start of code - Raju 01/01/1997 0815 hrs CST
        // ---------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.HiddenDet.State = "";
          export.Group.Update.HiddenDet.EndDate = local.Zero.Date;
          export.Group.Update.HiddenDet.VerifiedDate = local.Zero.Date;
        }

        export.Group.CheckIndex();

        // ---------------------------------------------
        // End of code
        // ---------------------------------------------
        // 07/11/00 M.L Start
        if (AsChar(export.ArCommon.SelectChar) == 'S' && CharAt
          (export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
          var field = GetField(export.ArCommon, "selectChar");

          field.Error = true;

          ExitState = "SI0000_DO_NOT_CHANGE_ORGZ_ADDR";

          return;
        }

        // 07/11/00 M.L End
        // *********************************************
        // Add action invalid on existing addresses
        // *********************************************
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.DetCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              var field = GetField(export.Group.Item.DetCommon, "selectChar");

              field.Error = true;

              ++local.Select.Count;

              break;
            default:
              break;
          }

          if (local.Select.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_ACTION";

            return;
          }
        }

        export.Group.CheckIndex();

        if (AsChar(export.EmptyAddrSelect.SelectChar) != 'S')
        {
          var field = GetField(export.EmptyAddrSelect, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
          (export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
        else if (!IsEmpty(export.ApCommon.SelectChar) && !
          IsEmpty(export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        // *********************************************
        // Field validations
        // *********************************************
        if (IsEmpty(export.EmptyAddr.Street1))
        {
          var field = GetField(export.EmptyAddr, "street1");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.EmptyAddr.City))
        {
          var field = GetField(export.EmptyAddr, "city");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.EmptyAddr.State))
        {
          var field = GetField(export.EmptyAddr, "state");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.EmptyAddr.ZipCode))
        {
          var field = GetField(export.EmptyAddr, "zipCode");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }
        else
        {
          local.CheckZip.ZipCode = export.EmptyAddr.ZipCode ?? "";
          UseSiCheckZipIsNumeric();

          if (AsChar(local.NumericZip.Flag) == 'N')
          {
            var field = GetField(export.EmptyAddr, "zipCode");

            field.Error = true;

            return;
          }
        }

        // lss 04/27/2007 PR# 00209846 Following statements are added to verify 
        // the zip4 field.
        if (Length(TrimEnd(export.EmptyAddr.ZipCode)) > 0 && Length
          (TrimEnd(export.EmptyAddr.Zip4)) > 0)
        {
          if (Length(TrimEnd(export.EmptyAddr.Zip4)) < 4)
          {
            var field = GetField(export.EmptyAddr, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.EmptyAddr.Zip4, "0123456789") != 0)
          {
            var field = GetField(export.EmptyAddr, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.SendDateUpdated.Flag = "N";

        if (Equal(export.EmptyAddr.SendDate, local.Zero.Date))
        {
        }
        else if (Lt(export.EmptyAddr.SendDate, local.Current.Date))
        {
          var field = GetField(export.EmptyAddr, "sendDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

          return;
        }
        else if (Lt(local.Current.Date, export.EmptyAddr.SendDate))
        {
          // mjr
          // -------------------------------------------------
          // 11/08/1999
          // Send date can only be set to CURRENT DATE or NULL DATE
          // --------------------------------------------------------------
          var field = GetField(export.EmptyAddr, "sendDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }
        else
        {
          local.SendDateUpdated.Flag = "Y";
        }

        if (!IsEmpty(export.EmptyAddr.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.EmptyAddr.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.EmptyAddr, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
        }

        // *** CQ#6285/CQ#7848 For Kansas, retrieve county for the Zip code ***
        // **** CQ#6285/CQ#7848 Changes Begin here ***
        // *** Added State check too ***
        if (IsEmpty(export.EmptyAddr.County) || Equal
          (export.EmptyAddr.State, "KS"))
        {
          // **** CQ#6285/CQ#7848 Changes End here ***
          // *** CQ#6285/CQ#7848 Call the external to retrieve county for Kansas
          if (Equal(import.EmptyAddr.State, "KS"))
          {
            UseEabReturnKsCountyByZip1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
        else
        {
          local.Code.CodeName = "COUNTY CODE";
          local.CodeValue.Cdvalue = import.EmptyAddr.County ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.EmptyAddr, "county");

            field.Error = true;

            ExitState = "INVALID_COUNTY";

            return;
          }
        }

        if (IsEmpty(export.EmptyAddr.Type1))
        {
          var field = GetField(export.EmptyAddr, "type1");

          field.Error = true;

          ExitState = "SI0000_ADDRESS_TYPE_REQUIRED";

          return;
        }
        else
        {
          if (AsChar(export.EmptyAddr.Type1) == 'S')
          {
            var field = GetField(export.EmptyAddr, "type1");

            field.Error = true;

            ExitState = "INVALID_ADDRESS_TYPE";

            return;
          }

          local.Code.CodeName = "ADDRESS TYPE";
          local.CodeValue.Cdvalue = export.EmptyAddr.Type1 ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.EmptyAddr, "type1");

            field.Error = true;

            ExitState = "INVALID_TYPE_CODE";

            return;
          }
        }

        if (!IsEmpty(export.EmptyAddr.Source))
        {
          local.Code.CodeName = "ADDRESS SOURCE";
          local.CodeValue.Cdvalue = import.EmptyAddr.Source ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.EmptyAddr, "source");

            field.Error = true;

            ExitState = "INVALID_SOURCE";

            return;
          }
        }

        // ---------------------------------------------
        // 12/21/98 W.Campbell - Logic to prevent
        // input of VERIFIED DATE in the future
        // was disabled.  Work done on IDCR454.
        // ---------------------------------------------
        if (!IsEmpty(export.EmptyAddr.EndCode))
        {
          if (Equal(export.EmptyAddr.EndDate, local.Zero.Date))
          {
            var field1 = GetField(export.EmptyAddr, "endCode");

            field1.Error = true;

            var field2 = GetField(export.EmptyAddr, "endDate");

            field2.Error = true;

            ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

            return;
          }

          local.Code.CodeName = "ADDRESS END";
          local.CodeValue.Cdvalue = import.EmptyAddr.EndCode ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field = GetField(export.EmptyAddr, "endCode");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
          }
        }
        else
        {
          // ---------------------------------------------
          // 01/04/99 W.Campbell - Logic added to make
          // sure END CODE is entered with END DATE.
          // Work done on IDCR454.
          // ---------------------------------------------
          if (!Equal(export.EmptyAddr.EndDate, local.Zero.Date))
          {
            var field1 = GetField(export.EmptyAddr, "endCode");

            field1.Error = true;

            var field2 = GetField(export.EmptyAddr, "endDate");

            field2.Error = true;

            ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

            return;
          }
        }

        // ---------------------------------------------
        // 12/21/98 W.Campbell - Logic added to
        // validate that the user has entered one
        // or more of SEND DATE, VERIFIED DATE
        // or END DATE on an ADD or UPDATE.
        // Work done on IDCR454.
        // ---------------------------------------------
        if (Equal(export.EmptyAddr.EndDate, local.Zero.Date) && Equal
          (export.EmptyAddr.SendDate, local.Zero.Date) && Equal
          (export.EmptyAddr.VerifiedDate, local.Zero.Date))
        {
          var field1 = GetField(export.EmptyAddr, "endDate");

          field1.Error = true;

          var field2 = GetField(export.EmptyAddr, "sendDate");

          field2.Error = true;

          var field3 = GetField(export.EmptyAddr, "verifiedDate");

          field3.Error = true;

          ExitState = "MUST_ENTER_SEND_VERIFY_OR_END_DT";

          return;
        }

        // 11/06/00 M.L Start
        if (!Equal(export.EmptyAddr.EndDate, local.Zero.Date) && Lt
          (export.EmptyAddr.EndDate, export.EmptyAddr.VerifiedDate))
        {
          var field1 = GetField(export.EmptyAddr, "endDate");

          field1.Error = true;

          var field2 = GetField(export.EmptyAddr, "verifiedDate");

          field2.Error = true;

          ExitState = "SI0000_VER_DT_GREATER_END_DATE";

          return;
        }

        // 11/06/00 M.L End
        // 04/11/02 M.L Start
        // 11/13/02 MCA Deleted "...and end date < max date". PR142608
        if (Lt(local.TodayPlus6Months.Date, export.EmptyAddr.EndDate))
        {
          ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

          var field = GetField(export.EmptyAddr, "endDate");

          field.Error = true;

          return;
        }

        if (Lt(local.TodayPlus6Months.Date, export.EmptyAddr.VerifiedDate))
        {
          ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

          var field = GetField(export.EmptyAddr, "verifiedDate");

          field.Error = true;

          return;
        }

        // 04/11/02 M.L End
        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          local.CsePerson.Number = import.ApCsePersonsWorkSet.Number;
          export.ArCommon.SelectChar = "";
        }
        else
        {
          local.CsePerson.Number = import.ArCsePersonsWorkSet.Number;
        }

        export.HiddenCsePerson.Number = local.CsePerson.Number;
        export.EmptyAddr.LocationType = "D";
        export.EmptyAddr.WorkerId = global.UserId;
        UseSiCreateCsePersonAddress2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Asterisk.Identifier = export.EmptyAddr.Identifier;

          if (AsChar(local.SendDateUpdated.Flag) == 'Y')
          {
            // mjr
            // -----------------------------------------------------
            // 02/06/1999
            // Added creation of document trigger
            // ------------------------------------------------------------------
            // mjr
            // -----------------------------------------------------
            // 11/08/1999
            // Changed document from batch to online
            // ------------------------------------------------------------------
            // ---------------------------------------------
            // 09/22/99 W.Campbell -
            // ---------------------------------------------
            // mjr
            // -----------------------------------------------------
            // 11/08/1999
            // Changed document from batch to online
            // ------------------------------------------------------------------
            // ---------------------------------------------
            // 03/29/99 W.Campbell - Added attribute to
            // local sp_doc_key view for the address identifier
            // and a set statement to set the attribute to the
            // address identifier for passing it to the called CAB -
            // SP_CREATE_DOCUMENT_INFRASTRUCT.
            // This was to fix a problem with producing
            // the POSTMASTER letter.
            // ---------------------------------------------
            // mjr
            // -----------------------------------------------------
            // 11/08/1999
            // Changed document from batch to online
            // ------------------------------------------------------------------
            // mjr
            // -----------------------------------------------------
            // 11/08/1999
            // Changed document from batch to online
            // ------------------------------------------------------------------
            local.Document.Name = "POSTMAST";
            local.SpDocKey.KeyCase = export.Case1.Number;
            local.SpDocKey.KeyPerson = local.CsePerson.Number;
            local.SpDocKey.KeyPersonAddress = export.EmptyAddr.Identifier;
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }

          export.EmptyAddrSelect.SelectChar = local.BlankCommon.SelectChar;
          MoveCsePersonAddress2(local.BlankCsePersonAddress, export.EmptyAddr);
          local.BuildAddressList.Flag = "Y";
        }

        break;
      case "UPDATE":
        if (AsChar(export.EmptyAddrSelect.SelectChar) == 'S')
        {
          var field = GetField(export.EmptyAddrSelect, "selectChar");

          field.Error = true;

          ExitState = "INVALID_UPDATE_THIS_ENTRY";

          return;
        }

        // 07/11/00 M.L Start
        if (AsChar(export.ArCommon.SelectChar) == 'S' && CharAt
          (export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
          var field = GetField(export.ArCommon, "selectChar");

          field.Error = true;

          ExitState = "SI0000_DO_NOT_CHANGE_ORGZ_ADDR";

          return;
        }

        // 07/11/00 M.L End
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.DetCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              // ---------------------------------------------
              // Code added by Raju : 01/06/97;0900 hrs CST
              //   - after update the updated rows are marked
              //     with a *. In this prad, there is a bug
              //     which prevents this hapenning. Since,
              //     there were no cascading effects, no one
              //     did anything to correct it.
              //     This code is a workaround to correct this
              //     without affecting any other prad/ab.
              // ---------------------------------------------
              // ---------------------------------------------
              // Start of Code
              // ---------------------------------------------
              local.SaveUpdates.Index = export.Group.Index;
              local.SaveUpdates.CheckSize();

              local.SaveUpdates.Update.AddrId.Identifier =
                export.Group.Item.DetCsePersonAddress.Identifier;

              // ---------------------------------------------
              // End   of Code
              // ---------------------------------------------
              ++local.Select.Count;

              if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
                (export.ArCommon.SelectChar))
              {
                var field1 = GetField(export.ApCommon, "selectChar");

                field1.Error = true;

                var field2 = GetField(export.ArCommon, "selectChar");

                field2.Error = true;

                ExitState = "ACO_NE0000_NO_SELECTION_MADE";

                return;
              }
              else if (!IsEmpty(export.ApCommon.SelectChar) && !
                IsEmpty(export.ArCommon.SelectChar))
              {
                var field1 = GetField(export.ApCommon, "selectChar");

                field1.Error = true;

                var field2 = GetField(export.ArCommon, "selectChar");

                field2.Error = true;

                // ---------------------------------------------
                // 05/20/99 W.Campbell - Replaced
                // ZDelete exit states.
                // ---------------------------------------------
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              export.Hidden.Index = export.Group.Index;
              export.Hidden.CheckSize();

              // *********************************************
              // Field validations
              // *********************************************
              if (IsEmpty(export.Group.Item.DetCsePersonAddress.Street1))
              {
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "street1");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.DetCsePersonAddress.City))
              {
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "city");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.DetCsePersonAddress.State))
              {
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "state");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }

              if (IsEmpty(export.Group.Item.DetCsePersonAddress.ZipCode))
              {
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "zipCode");

                field.Error = true;

                ExitState = "ADDRESS_INCOMPLETE";
              }
              else
              {
                local.CheckZip.ZipCode =
                  export.Group.Item.DetCsePersonAddress.ZipCode ?? "";
                UseSiCheckZipIsNumeric();

                if (AsChar(local.NumericZip.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "zipCode");

                  field.Error = true;

                  return;
                }
              }

              // lss 04/27/2007 PR# 00209846 Following statements are added to 
              // verify the zip4 field.
              if (Length(TrimEnd(export.Group.Item.DetCsePersonAddress.ZipCode)) >
                0 && Length
                (TrimEnd(export.Group.Item.DetCsePersonAddress.Zip4)) > 0)
              {
                if (Length(TrimEnd(export.Group.Item.DetCsePersonAddress.Zip4)) <
                  4)
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "zip4");

                  field.Error = true;

                  ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

                  return;
                }
                else if (Verify(export.Group.Item.DetCsePersonAddress.Zip4,
                  "0123456789") != 0)
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "zip4");

                  field.Error = true;

                  ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                  return;
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                return;
              }

              if (Equal(export.Group.Item.DetCsePersonAddress.SendDate,
                local.Zero.Date) || Equal
                (export.Group.Item.DetCsePersonAddress.SendDate,
                export.Hidden.Item.HsendDate.SendDate))
              {
                local.SendDateUpdated.Flag = "N";
              }
              else if (Lt(export.Group.Item.DetCsePersonAddress.SendDate,
                Now().Date))
              {
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

                return;
              }
              else if (Lt(local.Current.Date,
                export.Group.Item.DetCsePersonAddress.SendDate))
              {
                // mjr
                // -------------------------------------------------
                // 11/08/1999
                // Send date can only be updated to CURRENT DATE or NULL DATE
                // --------------------------------------------------------------
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                return;
              }
              else
              {
                local.SendDateUpdated.Flag = "Y";
              }

              if (!IsEmpty(export.Group.Item.DetCsePersonAddress.State))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.DetCsePersonAddress.State ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "state");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_STATE_CODE";

                  return;
                }
              }

              // **** CQ#6285 Added the two OR conditions below, Retrieve the 
              // county when the user changes the State/Zip code changes for
              // Kansas State ****
              if (IsEmpty(export.Group.Item.DetCsePersonAddress.County) || Equal
                (export.Group.Item.DetCsePersonAddress.State, "KS") && !
                Equal(export.Group.Item.DetCsePersonAddress.State,
                export.Group.Item.HiddenDet.State) || Equal
                (export.Group.Item.DetCsePersonAddress.State, "KS") && !
                Equal(export.Group.Item.DetCsePersonAddress.ZipCode,
                export.Group.Item.HiddenDet.ZipCode))
              {
                if (Equal(export.Group.Item.DetCsePersonAddress.State, "KS"))
                {
                  UseEabReturnKsCountyByZip2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }
              }
              else
              {
                local.Code.CodeName = "COUNTY CODE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.DetCsePersonAddress.County ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "county");

                  field.Error = true;

                  ExitState = "INVALID_COUNTY";

                  return;
                }
              }

              if (IsEmpty(export.Group.Item.DetCsePersonAddress.Type1))
              {
                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "type1");

                field.Error = true;

                ExitState = "SI0000_ADDRESS_TYPE_REQUIRED";

                return;
              }
              else
              {
                if (AsChar(export.Group.Item.DetCsePersonAddress.Type1) != 'R'
                  && AsChar(export.Group.Item.DetCsePersonAddress.Type1) != 'M'
                  )
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "type1");

                  field.Error = true;

                  ExitState = "INVALID_ADDRESS_TYPE";

                  return;
                }

                local.Code.CodeName = "ADDRESS TYPE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.DetCsePersonAddress.Type1 ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "type1");

                  field.Error = true;

                  ExitState = "LE0000_INVALID_ADDRESS_TYPE";

                  return;
                }
              }

              if (!IsEmpty(export.Group.Item.DetCsePersonAddress.Source))
              {
                local.Code.CodeName = "ADDRESS SOURCE";
                local.CodeValue.Cdvalue =
                  export.Group.Item.DetCsePersonAddress.Source ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "source");

                  field.Error = true;

                  ExitState = "INVALID_SOURCE";

                  return;
                }
              }

              // ---------------------------------------------
              // 12/21/98 W.Campbell - Logic to prevent
              // input of VERIFIED DATE in the future
              // was disabled.  Work done on IDCR454.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.DetCsePersonAddress.EndCode))
              {
                if (Equal(export.Group.Item.DetCsePersonAddress.EndDate,
                  local.Zero.Date))
                {
                  var field1 =
                    GetField(export.Group.Item.DetCsePersonAddress, "endCode");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Group.Item.DetCsePersonAddress, "endDate");

                  field2.Error = true;

                  ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                  return;
                }

                local.Code.CodeName = "ADDRESS END";
                local.CodeValue.Cdvalue =
                  export.Group.Item.DetCsePersonAddress.EndCode ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnCode.Flag) == 'N')
                {
                  var field =
                    GetField(export.Group.Item.DetCsePersonAddress, "endCode");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_CODE";

                  return;
                }
              }
              else
              {
                // ---------------------------------------------
                // 01/04/99 W.Campbell - Logic added to make
                // sure END CODE is entered with END DATE.
                // Work done on IDCR454.
                // ---------------------------------------------
                if (!Equal(export.Group.Item.DetCsePersonAddress.EndDate,
                  local.Zero.Date))
                {
                  var field1 =
                    GetField(export.Group.Item.DetCsePersonAddress, "endCode");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Group.Item.DetCsePersonAddress, "endDate");

                  field2.Error = true;

                  ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                  return;
                }
              }

              // ---------------------------------------------
              // 12/21/98 W.Campbell - Logic added to
              // validate that the user has entered one
              // or more of SEND DATE, VERIFIED DATE
              // or END DATE on an ADD or UPDATE.
              // Work done on IDCR454.
              // ---------------------------------------------
              if (Equal(export.Group.Item.DetCsePersonAddress.EndDate,
                local.Zero.Date) && Equal
                (export.Group.Item.DetCsePersonAddress.SendDate, local.Zero.Date)
                && Equal
                (export.Group.Item.DetCsePersonAddress.VerifiedDate,
                local.Zero.Date))
              {
                var field1 =
                  GetField(export.Group.Item.DetCsePersonAddress, "endDate");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.DetCsePersonAddress, "verifiedDate");
                  

                field3.Error = true;

                ExitState = "MUST_ENTER_SEND_VERIFY_OR_END_DT";

                return;
              }

              // 02/22/00 M.L Start
              if (!Equal(export.Group.Item.DetCsePersonAddress.EndDate,
                local.Zero.Date) && Lt
                (export.Group.Item.DetCsePersonAddress.EndDate,
                export.Group.Item.DetCsePersonAddress.VerifiedDate))
              {
                var field1 =
                  GetField(export.Group.Item.DetCsePersonAddress, "endDate");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.DetCsePersonAddress, "verifiedDate");
                  

                field2.Error = true;

                ExitState = "SI0000_VER_DT_GREATER_END_DATE";

                return;
              }

              // 02/22/00 M.L End
              // 04/11/02 M.L Start
              // 11/13/02 MCA Deleted "...and end date < max date". PR142608
              if (Lt(local.TodayPlus6Months.Date,
                export.Group.Item.DetCsePersonAddress.EndDate))
              {
                ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "endDate");

                field.Error = true;

                return;
              }

              if (Lt(local.TodayPlus6Months.Date,
                export.Group.Item.DetCsePersonAddress.VerifiedDate))
              {
                ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

                var field =
                  GetField(export.Group.Item.DetCsePersonAddress, "verifiedDate");
                  

                field.Error = true;

                return;
              }

              // 04/11/02 M.L End
              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
                if (AsChar(export.ApCommon.SelectChar) == 'S')
                {
                  local.CsePerson.Number = import.ApCsePersonsWorkSet.Number;
                }
                else
                {
                  local.CsePerson.Number = import.ArCsePersonsWorkSet.Number;
                }

                export.HiddenCsePerson.Number = local.CsePerson.Number;
                export.Group.Update.DetCsePersonAddress.WorkerId =
                  global.UserId;
                UseSiUpdateCsePersonAddress();

                // 08/06/1999    M.L  Added ACO_NI0000_SUCCESSFUL_UPDATE to 
                // place '*' for all processed rows.
                if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                  ("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                  export.Group.Update.DetCommon.SelectChar = "*";
                  local.Asterisk.Identifier =
                    export.Group.Item.DetCsePersonAddress.Identifier;

                  if (AsChar(local.SendDateUpdated.Flag) == 'Y')
                  {
                    // mjr
                    // -----------------------------------------------------
                    // 02/06/1999
                    // Added creation of document trigger
                    // ------------------------------------------------------------------
                    // mjr
                    // -----------------------------------------------------
                    // 11/08/1999
                    // Changed document from batch to online
                    // ------------------------------------------------------------------
                    // ---------------------------------------------
                    // 09/22/99 W.Campbell -
                    // ---------------------------------------------
                    // mjr
                    // -----------------------------------------------------
                    // 11/08/1999
                    // Changed document from batch to online
                    // ------------------------------------------------------------------
                    // ---------------------------------------------
                    // 03/29/99 W.Campbell - Added attribute to
                    // local sp_doc_key view for the address identifier
                    // and a set statement to set the attribute to the
                    // address identifier for passing it to the called CAB -
                    // SP_CREATE_DOCUMENT_INFRASTRUCT.
                    // This was to fix a problem with producing
                    // the POSTMASTER letter.
                    // ---------------------------------------------
                    // mjr
                    // -----------------------------------------------------
                    // 11/08/1999
                    // Changed document from batch to online
                    // ------------------------------------------------------------------
                    // mjr
                    // -----------------------------------------------------
                    // 11/08/1999
                    // Changed document from batch to online
                    // ------------------------------------------------------------------
                    local.Document.Name = "POSTMAST";
                    local.SpDocKey.KeyCase = export.Case1.Number;
                    local.SpDocKey.KeyPerson = local.CsePerson.Number;
                    local.SpDocKey.KeyPersonAddress =
                      export.Group.Item.DetCsePersonAddress.Identifier;
                  }

                  ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                  // 07/20/00 M.L Start
                  if (!Lt(export.Group.Item.DetCsePersonAddress.EndDate,
                    local.Current.Date) || Equal
                    (export.Group.Item.DetCsePersonAddress.EndDate,
                    local.Zero.Date))
                  {
                    if (Equal(export.Group.Item.DetCsePersonAddress.Street1,
                      export.Group.Item.HiddenDet.Street1) && Equal
                      (export.Group.Item.DetCsePersonAddress.Street2,
                      export.Group.Item.HiddenDet.Street2) && Equal
                      (export.Group.Item.DetCsePersonAddress.ZipCode,
                      export.Group.Item.HiddenDet.ZipCode) && Equal
                      (export.Group.Item.DetCsePersonAddress.Zip4,
                      export.Group.Item.HiddenDet.Zip4) && Equal
                      (export.Group.Item.DetCsePersonAddress.City,
                      export.Group.Item.HiddenDet.City) && Equal
                      (export.Group.Item.HiddenDet.County,
                      export.Group.Item.DetCsePersonAddress.County) && Equal
                      (export.Group.Item.DetCsePersonAddress.State,
                      export.Group.Item.HiddenDet.State) && Equal
                      (export.Group.Item.DetCsePersonAddress.Source,
                      export.Group.Item.HiddenDet.Source))
                    {
                    }
                    else
                    {
                      local.Csnet.Flag = "Y";
                    }
                  }

                  // 07/20/00 M.L End
                }
              }
              else
              {
                return;
              }

              break;
            default:
              break;
          }
        }

        export.Group.CheckIndex();

        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        // -------------------------------------------------------------------------------------
        // Per WR# 261, when a Prison/Jail record was added on JAIL screen for a
        // person, that address will be populated on ADDR screen. On ADDR
        // screen the 'Send_Dt' for that Prison/Jail address must be protected
        // and a message :  " Not able to send PM letter on a Prison/Jail
        // address", will be displayed on the ADDR screen,
        //                                                 
        // Vithal Madhira (04/09/2001)
        // ------------------------------------------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
          {
            var field1 =
              GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.DetCsePersonAddress, "source");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

            field3.Color = "cyan";
            field3.Protected = true;
          }
        }

        export.Group.CheckIndex();

        break;
      case "COPY":
        // *********************************************
        // Copy action invalid on existing addresses
        // *********************************************
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.DetCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              var field = GetField(export.Group.Item.DetCommon, "selectChar");

              field.Error = true;

              ++local.Select.Count;

              break;
            default:
              break;
          }

          if (local.Select.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_ACTION";

            return;
          }
        }

        export.Group.CheckIndex();

        if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
          (export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
        else if (!IsEmpty(export.ApCommon.SelectChar) && !
          IsEmpty(export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        // 07/20/99 M.L Start
        //              Add code  for address validation
        // *********************************************
        // Field validations
        // *********************************************
        if (IsEmpty(export.AeAddr.Street1))
        {
          ExitState = "ADDRESS_AE_INVALID";

          return;
        }

        if (IsEmpty(export.AeAddr.City))
        {
          ExitState = "ADDRESS_AE_INVALID";

          return;
        }

        if (IsEmpty(export.AeAddr.State))
        {
          ExitState = "ADDRESS_AE_INVALID";

          return;
        }

        if (IsEmpty(export.AeAddr.ZipCode))
        {
          ExitState = "ADDRESS_AE_INVALID";

          return;
        }
        else
        {
          local.CheckZip.ZipCode = export.AeAddr.ZipCode ?? "";
          UseSiCheckZipIsNumeric();

          if (AsChar(local.NumericZip.Flag) == 'N')
          {
            ExitState = "ADDRESS_AE_INVALID";

            return;
          }

          // lss 04/27/2007 PR# 00209846 Following statements are added to 
          // verify the zip4 field.
          if (Length(TrimEnd(export.AeAddr.ZipCode)) > 0 && Length
            (TrimEnd(export.AeAddr.Zip4)) > 0)
          {
            if (Length(TrimEnd(export.AeAddr.Zip4)) < 4)
            {
              ExitState = "ADDRESS_AE_INVALID";

              return;
            }
            else if (Verify(export.AeAddr.Zip4, "0123456789") != 0)
            {
              ExitState = "ADDRESS_AE_INVALID";

              return;
            }
          }
        }

        if (!IsEmpty(export.AeAddr.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.AeAddr.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            ExitState = "ADDRESS_AE_INVALID";

            return;
          }
        }

        // 07/20/99 M.L End
        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          local.CsePerson.Number = import.ApCsePersonsWorkSet.Number;
        }
        else
        {
          local.CsePerson.Number = import.ArCsePersonsWorkSet.Number;
        }

        export.HiddenCsePerson.Number = local.CsePerson.Number;
        local.CsePersonAddress.Assign(export.AeAddr);
        export.HiddenCsePerson.Number = local.CsePerson.Number;

        if (IsEmpty(export.AeAddr.Type1))
        {
          local.CsePersonAddress.Type1 = "R";
        }

        local.CsePersonAddress.LocationType = "D";
        local.CsePersonAddress.Source = "AE";
        local.CsePersonAddress.VerifiedDate = Now().Date;
        UseSiCreateCsePersonAddress1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.BuildAddressList.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "FADS":
        ExitState = "ECO_LNK_TO_FOREIGN_ADDRESS_MAINT";

        break;
      case "LIST":
        if (AsChar(import.ArList.Text1) == 'S' && AsChar
          (import.ApList.Text1) == 'S')
        {
          var field1 = GetField(export.ApList, "text1");

          field1.Error = true;

          var field2 = GetField(export.ArList, "text1");

          field2.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(import.ApList.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else if (AsChar(import.ArList.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_ROLE_MAINTENANCE";

          return;
        }
        else
        {
          if (!IsEmpty(import.ApList.Text1))
          {
            var field = GetField(export.ApList, "text1");

            field.Error = true;

            // ---------------------------------------------
            // 05/20/99 W.Campbell - Replaced
            // ZDelete exit states.
            // ---------------------------------------------
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (!IsEmpty(import.ArList.Text1))
          {
            var field = GetField(export.ArList, "text1");

            field.Error = true;

            // ---------------------------------------------
            // 05/20/99 W.Campbell - Replaced
            // ZDelete exit states.
            // ---------------------------------------------
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (IsEmpty(import.ArList.Text1) && IsEmpty(import.ApList.Text1))
          {
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (AsChar(export.EmptyAddrSelect.SelectChar) == 'S')
        {
          if (AsChar(export.MtEnddtCdPrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "ADDRESS END";
          }
          else if (AsChar(export.MtSrcePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "ADDRESS SOURCE";
          }
          else if (AsChar(export.MtStatePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "STATE CODE";
          }
          else if (AsChar(export.MtCntyPrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "COUNTY CODE";
          }
          else if (AsChar(export.MtTypePrmt.Text1) == 'S')
          {
            export.PromptCode.CodeName = "ADDRESS TYPE";
          }
          else
          {
            var field = GetField(export.EmptyAddrSelect, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          }

          ExitState = "ECO_LNK_TO_CODE_TABLES";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.DetCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.EnddtCdPrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "ADDRESS END";
            }
            else if (AsChar(export.Group.Item.SrcePrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "ADDRESS SOURCE";
            }
            else if (AsChar(export.Group.Item.StatePrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "STATE CODE";
            }
            else if (AsChar(export.Group.Item.CntyPrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "COUNTY CODE";
            }
            else if (AsChar(export.Group.Item.TypePrmt.Text1) == 'S')
            {
              export.PromptCode.CodeName = "ADDRESS TYPE";
            }
            else
            {
              var field = GetField(export.Group.Item.DetCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "RTLIST":
        if (AsChar(export.EmptyAddrSelect.SelectChar) == 'S')
        {
          if (AsChar(export.MtEnddtCdPrmt.Text1) == 'S')
          {
            export.MtEnddtCdPrmt.Text1 = "";

            var field = GetField(export.MtEnddtCdPrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.EmptyAddr.EndCode = import.PromptCodeValue.Cdvalue;

              return;
            }
          }
          else if (AsChar(export.MtSrcePrmt.Text1) == 'S')
          {
            export.MtSrcePrmt.Text1 = "";

            var field = GetField(export.MtSrcePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.EmptyAddr.Source = import.PromptCodeValue.Cdvalue;

              return;
            }
          }
          else if (AsChar(export.MtStatePrmt.Text1) == 'S')
          {
            export.MtStatePrmt.Text1 = "";

            var field = GetField(export.MtStatePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.EmptyAddr.State = import.PromptCodeValue.Cdvalue;

              return;
            }
          }
          else if (AsChar(export.MtCntyPrmt.Text1) == 'S')
          {
            export.MtCntyPrmt.Text1 = "";

            var field = GetField(export.MtCntyPrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.EmptyAddr.County = import.PromptCodeValue.Cdvalue;

              return;
            }
          }
          else if (AsChar(export.MtTypePrmt.Text1) == 'S')
          {
            export.MtTypePrmt.Text1 = "";

            var field = GetField(export.MtTypePrmt, "text1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.PromptCodeValue.Cdvalue))
            {
              export.EmptyAddr.Type1 = import.PromptCodeValue.Cdvalue;

              if (AsChar(export.EmptyAddr.Type1) == 'S')
              {
                var field1 = GetField(export.EmptyAddr, "type1");

                field1.Error = true;

                ExitState = "INVALID_ADDRESS_TYPE";
              }

              return;
            }
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.DetCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.EnddtCdPrmt.Text1) == 'S')
            {
              export.Group.Update.EnddtCdPrmt.Text1 = "";

              var field = GetField(export.Group.Item.EnddtCdPrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.DetCsePersonAddress.EndCode =
                  import.PromptCodeValue.Cdvalue;

                return;
              }
            }
            else if (AsChar(export.Group.Item.SrcePrmt.Text1) == 'S')
            {
              export.Group.Update.SrcePrmt.Text1 = "";

              var field = GetField(export.Group.Item.SrcePrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.DetCsePersonAddress.Source =
                  import.PromptCodeValue.Cdvalue;

                return;
              }
            }
            else if (AsChar(export.Group.Item.StatePrmt.Text1) == 'S')
            {
              export.Group.Update.StatePrmt.Text1 = "";

              var field = GetField(export.Group.Item.StatePrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.DetCsePersonAddress.State =
                  import.PromptCodeValue.Cdvalue;

                return;
              }
            }
            else if (AsChar(export.Group.Item.CntyPrmt.Text1) == 'S')
            {
              export.Group.Update.CntyPrmt.Text1 = "";

              var field = GetField(export.Group.Item.CntyPrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.DetCsePersonAddress.County =
                  import.PromptCodeValue.Cdvalue;

                return;
              }
            }
            else if (AsChar(export.Group.Item.TypePrmt.Text1) == 'S')
            {
              export.Group.Update.TypePrmt.Text1 = "";

              var field = GetField(export.Group.Item.TypePrmt, "text1");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.PromptCodeValue.Cdvalue))
              {
                export.Group.Update.DetCsePersonAddress.Type1 =
                  import.PromptCodeValue.Cdvalue;

                if (AsChar(export.Group.Item.DetCsePersonAddress.Type1) == 'S')
                {
                  var field1 =
                    GetField(export.Group.Item.DetCsePersonAddress, "type1");

                  field1.Error = true;

                  ExitState = "INVALID_ADDRESS_TYPE";
                }

                return;
              }
            }
          }
          else
          {
          }
        }

        export.Group.CheckIndex();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // --------------------------------
    // Display Address List as required
    // by above functions.
    // --------------------------------
    if (AsChar(local.BuildAddressList.Flag) == 'Y')
    {
      if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
        (export.ArCommon.SelectChar) == 'S')
      {
        var field1 = GetField(export.ApCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.ArCommon, "selectChar");

        field2.Error = true;

        ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

        return;
      }
      else if (AsChar(export.ArCommon.SelectChar) == 'S')
      {
        export.Save.Number = export.ArCsePersonsWorkSet.Number;
      }
      else
      {
        export.ApCommon.SelectChar = "S";
        export.Save.Number = export.ApCsePersonsWorkSet.Number;
      }

      export.Group.Count = 0;
      export.Hidden.Count = 0;
      export.Pagenum.Count = 0;

      // 02/21/01 M.L Start
      if (AsChar(local.FvCheck.Flag) == 'Y')
      {
        UseScSecurityValidAuthForFv2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenNext.Number = export.Case1.Number;

          // 02/19/08  LSS  Added MOVE statement
          export.AeAddr.Assign(export.EmptyAddr);

          return;
        }
      }

      // 02/21/01 M.L End
      export.LastAddr.Identifier = Now().AddDays(1);
      export.LastAddr.EndDate = UseCabSetMaximumDiscontinueDate();
      local.Command.Command = global.Command;
      UseSiBuildAddresses1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr
        // -----------------------------------------------
        // 11/09/1999
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
          local.Print.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
            (50);
          UseSpPrintDecodeReturnCode();
          export.HiddenNextTranInfo.MiscText2 = local.Print.Text50;
        }
        else if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }
        else if (AsChar(export.ApActive.Flag) == 'N' && AsChar
          (export.ApCommon.SelectChar) == 'S')
        {
          ExitState = "DISPLAY_OK_FOR_INACTIVE_AP";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (export.Group.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
          }
        }
      }

      if (AsChar(export.ForeignAddr.Text1) == 'Y')
      {
        export.ForeignAddress.AddressLine1 = "Foreign Address";
      }
      else
      {
        export.ForeignAddress.AddressLine1 = "";
      }

      export.Standard.PageNumber = 1;

      // *********************************************
      // Set up group pagenum keys with the first
      // and the 4th occurrence of the address
      // identifiers
      // *********************************************
      if (export.Group.IsFull)
      {
        export.Plus.Text1 = "+";

        export.Pagenum.Index = 0;
        export.Pagenum.CheckSize();

        export.Group.Index = 0;
        export.Group.CheckSize();

        export.Pagenum.Update.LastAddr.Identifier = Now().AddDays(1);
        export.Pagenum.Update.LastAddr.EndDate =
          export.Group.Item.DetCsePersonAddress.EndDate;

        ++export.Pagenum.Index;
        export.Pagenum.CheckSize();

        export.Group.Index = Export.GroupGroup.Capacity - 2;
        export.Group.CheckSize();

        export.Pagenum.Update.LastAddr.Identifier =
          export.Group.Item.DetCsePersonAddress.Identifier;
        export.Pagenum.Update.LastAddr.EndDate =
          export.Group.Item.DetCsePersonAddress.EndDate;
      }
      else
      {
        export.Plus.Text1 = "";
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.DetCsePersonAddress.EndDate,
          local.Maximum.Date))
        {
          export.Group.Update.DetCsePersonAddress.EndDate = local.Zero.Date;
        }

        if (Equal(export.Group.Item.DetCsePersonAddress.Identifier,
          local.Asterisk.Identifier))
        {
          export.Group.Update.DetCommon.SelectChar = "*";
        }

        export.Hidden.Index = export.Group.Index;
        export.Hidden.CheckSize();

        export.Hidden.Update.HsendDate.SendDate =
          export.Group.Item.DetCsePersonAddress.SendDate;

        if (Equal(global.Command, "DISPLAY"))
        {
          export.Group.Update.HiddenDet.Assign(
            export.Group.Item.DetCsePersonAddress);
        }
      }

      export.Group.CheckIndex();

      // -------------------------------------------------------------------------------------
      // Per WR# 261, when a Prison/Jail record was added on JAIL screen for a 
      // person, that address will be populated on ADDR screen. On ADDR screen
      // the 'Send_Dt' for that Prison/Jail address must be protected and a
      // message :  " Not able to send PM letter on a Prison/Jail address", will
      // be displayed on the ADDR screen,
      //                                                 
      // Vithal Madhira (04/09/2001)
      // ------------------------------------------------------------------------------------
      if (Equal(global.Command, "DISPLAY"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
          {
            var field1 =
              GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.DetCsePersonAddress, "source");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

            field3.Color = "cyan";
            field3.Protected = true;

            ExitState = "ACO_NI0000_NO_PM_LETTER";
          }
        }

        export.Group.CheckIndex();
      }

      // ----------------------------------------
      // WR292
      // Beginning of code
      // Flow to ADDR screen with Returned Address.
      // ------------------------------------------
      if (AsChar(local.FplsDisplayFlag.Flag) == 'Y')
      {
        export.EmptyAddrSelect.SelectChar = "S";
        ExitState = "ACO_NE0000_SEND_OR_VERIFIED_DATE";
      }

      // ----------------------------------------
      // WR292
      // End of code
      // ------------------------------------------
    }

    // ---------------------------------------------
    // Code added by Raju  Jan 02, 1997:0730 hrs CST
    // The oe cab raise event will be called from
    //   here case of add / update when the
    //   address verified and/or end date is
    //   added/changed
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      // --------------------------------------------
      // The below code may be redundant, however it
      //   is to ensure that either ap/ar values are
      //    selected and not both.
      // If you remove it, pls test for it under
      //   add/update conditions that only one
      //   ap/ar is always selected. I am not risking
      //   anything in this complex prad.
      // --------------------------------------------
      if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
        (export.ArCommon.SelectChar) == 'S')
      {
        var field1 = GetField(export.ApCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.ArCommon, "selectChar");

        field2.Error = true;

        ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

        // ---------------------------------------------
        // 05/12/99 W.Campbell - Added
        // USE eab_rollback_cics statement
        // to rollback DB/2 updates for this
        // error.
        // ---------------------------------------------
        UseEabRollbackCics();

        return;
      }

      local.Infrastructure.UserId = "ADDR";
      local.Infrastructure.EventId = 10;

      // 11/17/00 M.L Start
      // This change was requested by Patrick.
      local.Infrastructure.BusinessObjectCd = "CAU";

      // 11/17/00 M.L
      local.Infrastructure.SituationNumber = 0;
      local.DetailText1.Text1 = ",";
      export.Group.Index = 0;

      for(var limit = export.Group.Count; export.Group.Index < limit; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          local.AparSelection.Text1 = "P";
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          local.AparSelection.Text1 = "R";
        }

        if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*')
        {
          local.Infrastructure.DenormTimestamp =
            export.Group.Item.DetCsePersonAddress.Identifier;

          for(local.NumberOfEvents.TotalInteger = 1; local
            .NumberOfEvents.TotalInteger <= 3; ++
            local.NumberOfEvents.TotalInteger)
          {
            local.RaiseEventFlag.Text1 = "N";
            local.Infrastructure.Detail =
              Spaces(Infrastructure.Detail_MaxLength);

            if (local.NumberOfEvents.TotalInteger == 1)
            {
              if (!IsEmpty(export.ApCommon.SelectChar))
              {
                // 09/14/99 M.L   Check if verified date was changed
                if (!Equal(export.Group.Item.HiddenDet.State,
                  export.Group.Item.DetCsePersonAddress.State) && !
                  Equal(export.Group.Item.DetCsePersonAddress.State, "KS") && !
                  IsEmpty(export.Group.Item.DetCsePersonAddress.State) && !
                  Equal(export.Group.Item.DetCsePersonAddress.VerifiedDate,
                  export.Group.Item.HiddenDet.VerifiedDate))
                {
                  local.RaiseEventFlag.Text1 = "Y";
                  local.Infrastructure.ReasonCode = "ADDROSKS";
                  local.DetailText30.Text30 = "AP Address located in :";
                  local.DetailText10.Text10 =
                    export.Group.Item.DetCsePersonAddress.State ?? Spaces(10);
                  local.Infrastructure.Detail =
                    TrimEnd(local.DetailText30.Text30) + TrimEnd
                    (local.DetailText10.Text10);
                  local.CsePerson.Number = export.ApCsePersonsWorkSet.Number;
                  export.HiddenCsePerson.Number = local.CsePerson.Number;
                }
              }
            }
            else if (local.NumberOfEvents.TotalInteger == 2)
            {
              if (!Equal(export.Group.Item.DetCsePersonAddress.EndDate,
                export.Group.Item.HiddenDet.EndDate) && Lt
                (local.Zero.Date, export.Group.Item.DetCsePersonAddress.EndDate))
                
              {
                // --------------------------------------------
                // Close monitored flag code added 1/23/97:11:55
                // --------------------------------------------
                // ---------------------------------------------
                // 02/18/99 M Ramirez & W.Campbell
                // Disabled statements
                // dealing with closing monitored documents,
                // as it has been determined that the best way
                // to handle them will be in Batch.
                // ---------------------------------------------
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReferenceDate =
                  export.Group.Item.DetCsePersonAddress.EndDate;

                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.CsePerson.Number = export.ApCsePersonsWorkSet.Number;
                    export.HiddenCsePerson.Number = local.CsePerson.Number;
                    local.Infrastructure.ReasonCode = "ADDREXPDAP";
                    local.DetailText30.Text30 = "Address ended for AP :";

                    break;
                  case 'R':
                    local.CsePerson.Number = export.ArCsePersonsWorkSet.Number;
                    export.HiddenCsePerson.Number = local.CsePerson.Number;
                    local.Infrastructure.ReasonCode = "ADDREXPDAR";
                    local.DetailText30.Text30 = "Address ended for AR :";

                    break;
                  default:
                    break;
                }

                // 10/25/00 M.L Start
                local.Infrastructure.Detail =
                  Spaces(Infrastructure.Detail_MaxLength);

                // 10/25/00 M.L End
              }
            }
            else if (local.NumberOfEvents.TotalInteger == 3)
            {
              if (!Equal(export.Group.Item.DetCsePersonAddress.VerifiedDate,
                export.Group.Item.HiddenDet.VerifiedDate))
              {
                // --------------------------------------------
                // Close monitored flag code added 1/23/97:11:55
                // --------------------------------------------
                // ---------------------------------------------
                // 02/18/99 M Ramirez & W.Campbell
                // Disabled statements
                // dealing with closing monitored documents,
                // as it has been determined that the best way
                // to handle them will be in Batch.
                // ---------------------------------------------
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReferenceDate =
                  export.Group.Item.DetCsePersonAddress.VerifiedDate;

                // 02/18/00 M.L Generate blankoutdate event for the situation
                //                when verified date is blank out.
                if (Equal(export.Group.Item.DetCsePersonAddress.VerifiedDate,
                  local.Zero.Date))
                {
                  // 02/18/00 M.L Generate blankoutdate event for the situation
                  //                when verified date is blank out.
                  //                This code was added on 02/18/00.
                  switch(AsChar(local.AparSelection.Text1))
                  {
                    case 'P':
                      local.CsePerson.Number =
                        export.ApCsePersonsWorkSet.Number;
                      export.HiddenCsePerson.Number = local.CsePerson.Number;
                      local.Infrastructure.ReasonCode = "BLANKOUTDATE";
                      local.DetailText30.Text30 =
                        "Verified date removed for AP :";

                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - Added code to
                      // process interstate event for either
                      // AP address located with state code = KS
                      // (Kansas) (LSADR)
                      //          - OR -
                      // for AP address located with state code
                      // NOT = KS (Kansas) (LSOUT).
                      // ---------------------------------------------
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

                      if (Equal(export.Group.Item.DetCsePersonAddress.State,
                        "KS"))
                      {
                        // ---------------------------------------------
                        // 05/19/99 W.Campbell - Added code to
                        // process interstate event for AP address
                        // located with state code = KS  (LSADR).
                        // ---------------------------------------------
                        local.ScreenIdentification.Command = "ADDR LSADR";
                        local.Csenet.Number = export.ApCsePersonsWorkSet.Number;

                        // ------------
                        // Beg PR 84721
                        // ------------
                        if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*'
                          )
                        {
                          local.Miscellaneous.Timestamp =
                            export.Group.Item.DetCsePersonAddress.Identifier;
                        }
                        else
                        {
                          // -------------------
                          // continue processing
                          // -------------------
                        }

                        UseSiCreateAutoCsenetTrans2();

                        // ------------
                        // End PR 84721
                        // ------------
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

                        // ---------------------------------------------
                        // 05/19/99 W.Campbell - End of code added to
                        // process interstate event for AP address
                        // located with state code = KS  (LSADR).
                        // ---------------------------------------------
                      }
                      else
                      {
                        // ---------------------------------------------
                        // 05/19/99 W.Campbell - Added code to
                        // process interstate event for AP address
                        // located with state code NOT = KS  (LSOUT).
                        // ---------------------------------------------
                        local.ScreenIdentification.Command = "ADDR LSOUT";
                        local.Csenet.Number = export.ApCsePersonsWorkSet.Number;

                        // ------------
                        // Beg PR 84721
                        // ------------
                        if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*'
                          )
                        {
                          local.Miscellaneous.Timestamp =
                            export.Group.Item.DetCsePersonAddress.Identifier;
                        }
                        else
                        {
                          // -------------------
                          // continue processing
                          // -------------------
                        }

                        UseSiCreateAutoCsenetTrans2();

                        // ------------
                        // End PR 84721
                        // ------------
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

                        // ---------------------------------------------
                        // 05/19/99 W.Campbell - End of code added to
                        // process interstate event for AP address
                        // located with state code NOT = KS  (LSOUT).
                        // ---------------------------------------------
                      }

                      break;
                    case 'R':
                      local.CsePerson.Number =
                        export.ArCsePersonsWorkSet.Number;
                      export.HiddenCsePerson.Number = local.CsePerson.Number;
                      local.Infrastructure.ReasonCode = "BLANKOUTDATE";
                      local.DetailText30.Text30 =
                        "Verified date removed for AR :";

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

                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - Added code to
                      // process interstate event for AR address
                      // located (GSPAD).
                      // ---------------------------------------------
                      local.ScreenIdentification.Command = "ADDR GSPAD";
                      local.Csenet.Number = export.ArCsePersonsWorkSet.Number;

                      // ------------
                      // Beg PR 84721
                      // ------------
                      if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*')
                      {
                        local.Miscellaneous.Timestamp =
                          export.Group.Item.DetCsePersonAddress.Identifier;
                      }
                      else
                      {
                        // -------------------
                        // continue processing
                        // -------------------
                      }

                      UseSiCreateAutoCsenetTrans1();

                      // ------------
                      // End PR 84721
                      // ------------
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

                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - End of code added to
                      // process interstate event for AR address
                      // located (GSPAD).
                      // ---------------------------------------------
                      break;
                    default:
                      break;
                  }

                  // 02/18/00 M.L Generate blankoutdate event for the situation
                  //                when verified date is blank out.
                  //                End of code was added on 02/18/00.
                }
                else
                {
                  switch(AsChar(local.AparSelection.Text1))
                  {
                    case 'P':
                      local.CsePerson.Number =
                        export.ApCsePersonsWorkSet.Number;
                      export.HiddenCsePerson.Number = local.CsePerson.Number;
                      local.Infrastructure.ReasonCode = "ADDRVRFDAP";
                      local.DetailText30.Text30 = " Address Verified for AP :";

                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - Added code to
                      // process interstate event for either
                      // AP address located with state code = KS
                      // (Kansas) (LSADR)
                      //          - OR -
                      // for AP address located with state code
                      // NOT = KS (Kansas) (LSOUT).
                      // ---------------------------------------------
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

                      // 02/22/00 M.L Check if verified date is different that 
                      // end date.
                      // This was added to bypass CSENET when these two dates 
                      // are equal.
                      if (!Equal(export.Group.Item.DetCsePersonAddress.
                        VerifiedDate,
                        export.Group.Item.DetCsePersonAddress.EndDate))
                      {
                        // 07/20/00 M.L Start
                        local.Csnet.Count = 1;

                        // 07/20/00 M.L End
                        if (Equal(export.Group.Item.DetCsePersonAddress.State,
                          "KS"))
                        {
                          // ---------------------------------------------
                          // 05/19/99 W.Campbell - Added code to
                          // process interstate event for AP address
                          // located with state code = KS  (LSADR).
                          // ---------------------------------------------
                          local.ScreenIdentification.Command = "ADDR LSADR";
                          local.Csenet.Number =
                            export.ApCsePersonsWorkSet.Number;

                          // ------------
                          // Beg PR 84721
                          // ------------
                          if (AsChar(export.Group.Item.DetCommon.SelectChar) ==
                            '*')
                          {
                            local.Miscellaneous.Timestamp =
                              export.Group.Item.DetCsePersonAddress.Identifier;
                          }
                          else
                          {
                            // -------------------
                            // continue processing
                            // -------------------
                          }

                          UseSiCreateAutoCsenetTrans2();

                          // ------------
                          // End PR 84721
                          // ------------
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

                          // ---------------------------------------------
                          // 05/19/99 W.Campbell - End of code added to
                          // process interstate event for AP address
                          // located with state code = KS  (LSADR).
                          // ---------------------------------------------
                        }
                        else
                        {
                          // ---------------------------------------------
                          // 05/19/99 W.Campbell - Added code to
                          // process interstate event for AP address
                          // located with state code NOT = KS  (LSOUT).
                          // ---------------------------------------------
                          local.ScreenIdentification.Command = "ADDR LSOUT";
                          local.Csenet.Number =
                            export.ApCsePersonsWorkSet.Number;

                          // ------------
                          // Beg PR 84721
                          // ------------
                          if (AsChar(export.Group.Item.DetCommon.SelectChar) ==
                            '*')
                          {
                            local.Miscellaneous.Timestamp =
                              export.Group.Item.DetCsePersonAddress.Identifier;
                          }
                          else
                          {
                            // -------------------
                            // continue processing
                            // -------------------
                          }

                          UseSiCreateAutoCsenetTrans2();

                          // ------------
                          // End PR 84721
                          // ------------
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

                          // ---------------------------------------------
                          // 05/19/99 W.Campbell - End of code added to
                          // process interstate event for AP address
                          // located with state code NOT = KS  (LSOUT).
                          // ---------------------------------------------
                        }
                      }

                      break;
                    case 'R':
                      local.CsePerson.Number =
                        export.ArCsePersonsWorkSet.Number;
                      export.HiddenCsePerson.Number = local.CsePerson.Number;
                      local.Infrastructure.ReasonCode = "ADDRVRFDAR";
                      local.DetailText30.Text30 = " Address Verified for AR :";

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

                      // 02/22/00 M.L Check if verified date is different that 
                      // end date.
                      // This was added to bypass CSENET when these two dates 
                      // are equal.
                      if (!Equal(export.Group.Item.DetCsePersonAddress.
                        VerifiedDate,
                        export.Group.Item.DetCsePersonAddress.EndDate))
                      {
                        // ---------------------------------------------
                        // 05/19/99 W.Campbell - Added code to
                        // process interstate event for AR address
                        // located (GSPAD).
                        // ---------------------------------------------
                        local.ScreenIdentification.Command = "ADDR GSPAD";
                        local.Csenet.Number = export.ArCsePersonsWorkSet.Number;

                        // ------------
                        // Beg PR 84721
                        // ------------
                        if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*'
                          )
                        {
                          local.Miscellaneous.Timestamp =
                            export.Group.Item.DetCsePersonAddress.Identifier;
                        }
                        else
                        {
                          // -------------------
                          // continue processing
                          // -------------------
                        }

                        // 07/20/00 M.L Start
                        local.Csnet.Count = 1;

                        // 07/20/00 M.L End
                        UseSiCreateAutoCsenetTrans1();

                        // ------------
                        // End PR 84721
                        // ------------
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

                        // ---------------------------------------------
                        // 05/19/99 W.Campbell - End of code added to
                        // process interstate event for AR address
                        // located (GSPAD).
                        // ---------------------------------------------
                      }

                      break;
                    default:
                      break;
                  }
                }

                // 10/25/00 M.L Start
                local.Infrastructure.Detail =
                  Spaces(Infrastructure.Detail_MaxLength);

                // 10/25/00 M.L End
                // ---------------------------------------------
                // Begin of Code - External Alert
                //   Raju : 01/06/97;1510 hrs CST
                // ---------------------------------------------
                if (!Equal(export.Group.Item.DetCsePersonAddress.Source, "AE"))
                {
                  local.InterfaceAlert.AlertCode = "45";
                  UseSpAddrExternalAlert();
                }

                // ---------------------------------------------
                // End   of Code
                // ---------------------------------------------
              }

              // 07/20/00 M.L start
              if (AsChar(local.Csnet.Flag) == 'Y' && local.Csnet.Count == 0)
              {
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

                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'R':
                    local.ScreenIdentification.Command = "ADDR GSPAD";
                    local.Csenet.Number = export.ArCsePersonsWorkSet.Number;

                    // ------------
                    // Beg PR 84721
                    // ------------
                    if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*')
                    {
                      local.Miscellaneous.Timestamp =
                        export.Group.Item.DetCsePersonAddress.Identifier;
                    }
                    else
                    {
                      // -------------------
                      // continue processing
                      // -------------------
                    }

                    UseSiCreateAutoCsenetTrans1();

                    // ------------
                    // End PR 84721
                    // ------------
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

                    break;
                  case 'P':
                    if (Equal(export.Group.Item.DetCsePersonAddress.State, "KS"))
                      
                    {
                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - Added code to
                      // process interstate event for AP address
                      // located with state code = KS  (LSADR).
                      // ---------------------------------------------
                      local.ScreenIdentification.Command = "ADDR LSADR";
                      local.Csenet.Number = export.ApCsePersonsWorkSet.Number;

                      // ------------
                      // Beg PR 84721
                      // ------------
                      if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*')
                      {
                        local.Miscellaneous.Timestamp =
                          export.Group.Item.DetCsePersonAddress.Identifier;
                      }
                      else
                      {
                        // -------------------
                        // continue processing
                        // -------------------
                      }

                      UseSiCreateAutoCsenetTrans2();

                      // ------------
                      // End PR 84721
                      // ------------
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

                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - End of code added to
                      // process interstate event for AP address
                      // located with state code = KS  (LSADR).
                      // ---------------------------------------------
                    }
                    else
                    {
                      // ---------------------------------------------
                      // 05/19/99 W.Campbell - Added code to
                      // process interstate event for AP address
                      // located with state code NOT = KS  (LSOUT).
                      // ---------------------------------------------
                      local.ScreenIdentification.Command = "ADDR LSOUT";
                      local.Csenet.Number = export.ApCsePersonsWorkSet.Number;

                      // ------------
                      // Beg PR 84721
                      // ------------
                      if (AsChar(export.Group.Item.DetCommon.SelectChar) == '*')
                      {
                        local.Miscellaneous.Timestamp =
                          export.Group.Item.DetCsePersonAddress.Identifier;
                      }
                      else
                      {
                        // -------------------
                        // continue processing
                        // -------------------
                      }

                      UseSiCreateAutoCsenetTrans2();

                      // ------------
                      // End PR 84721
                      // ------------
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

                    break;
                  default:
                    break;
                }
              }

              // 07/20/00 M.L end
            }
            else
            {
              local.RaiseEventFlag.Text1 = "N";
            }

            if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
            {
              UseSiAddrRaiseEvent();

              if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                UseEabRollbackCics();

                return;
              }

              // --------------------------------------------
              // Locate is person specific event. So the event
              // has to be raised for all Case Units that the
              // Located Person participates as an AP and an AR.
              // --------------------------------------------
              if (local.NumberOfEvents.TotalInteger == 3)
              {
                // 02/18/00 M.L Generate blankoutdate event for the situation
                //                when verified date is blank out.
                if (Equal(export.Group.Item.DetCsePersonAddress.VerifiedDate,
                  local.Zero.Date))
                {
                  // 02/18/00 M.L Generate blankoutdate event for the situation
                  //                when verified date is blank out.
                  //                This code was added on 02/18/00.
                  switch(AsChar(local.AparSelection.Text1))
                  {
                    case 'P':
                      local.Infrastructure.ReasonCode = "BLANKOUTDATE";
                      local.DetailText30.Text30 =
                        "Verified date removed for AR :";

                      // 10/25/00 M.L Start
                      local.Infrastructure.Detail =
                        Spaces(Infrastructure.Detail_MaxLength);

                      // 10/25/00 M.L End
                      local.AparSelection.Text1 = "R";

                      break;
                    case 'R':
                      local.Infrastructure.ReasonCode = "BLANKOUTDATE";
                      local.DetailText30.Text30 =
                        "Verified date removed for AP :";

                      // 10/25/00 M.L Start
                      local.Infrastructure.Detail =
                        Spaces(Infrastructure.Detail_MaxLength);

                      // 10/25/00 M.L End
                      local.AparSelection.Text1 = "P";

                      break;
                    default:
                      break;
                  }

                  // 02/18/00 M.L Generate blankoutdate event for the situation
                  //                when verified date is blank out.
                  //                This code was added on 02/18/00.
                }
                else
                {
                  switch(AsChar(local.AparSelection.Text1))
                  {
                    case 'P':
                      local.Infrastructure.ReasonCode = "ADDRVRFDAR";
                      local.DetailText30.Text30 = " Address Verified for AR :";

                      // 10/25/00 M.L Start
                      local.Infrastructure.Detail =
                        Spaces(Infrastructure.Detail_MaxLength);

                      // 10/25/00 M.L End
                      local.AparSelection.Text1 = "R";

                      break;
                    case 'R':
                      local.Infrastructure.ReasonCode = "ADDRVRFDAP";
                      local.DetailText30.Text30 = " Address Verified for AP :";

                      // 10/25/00 M.L Start
                      local.Infrastructure.Detail =
                        Spaces(Infrastructure.Detail_MaxLength);

                      // 10/25/00 M.L End
                      local.AparSelection.Text1 = "P";

                      break;
                    default:
                      break;
                  }
                }

                UseSiAddrRaiseEvent();

                if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                  ("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                }
                else
                {
                  UseEabRollbackCics();

                  return;
                }
              }
              else if (local.NumberOfEvents.TotalInteger == 2)
              {
                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.Infrastructure.ReasonCode = "ADDREXPDAR";
                    local.DetailText30.Text30 = "Address ended for AR :";

                    // 10/25/00 M.L Start
                    local.Infrastructure.Detail =
                      Spaces(Infrastructure.Detail_MaxLength);

                    // 10/25/00 M.L End
                    local.AparSelection.Text1 = "R";

                    break;
                  case 'R':
                    local.Infrastructure.ReasonCode = "ADDREXPDAP";
                    local.DetailText30.Text30 = "Address ended for AP :";

                    // 10/25/00 M.L Start
                    local.Infrastructure.Detail =
                      Spaces(Infrastructure.Detail_MaxLength);

                    // 10/25/00 M.L End
                    local.AparSelection.Text1 = "P";

                    break;
                  default:
                    break;
                }

                UseSiAddrRaiseEvent();

                if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                  ("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                }
                else
                {
                  UseEabRollbackCics();

                  return;
                }
              }
              else
              {
              }

              // Reset the field values for the next loop.
              // ---------------------------------------------
              // 06/17/99 W.Campbell - Disabled one line
              // of code and replaced it with statements
              // to reinitialize the text_1 attribute to contain
              // the correct value of "P" or "R" for AR or AP
              // based on the input values.
              // ---------------------------------------------
              if (AsChar(export.ApCommon.SelectChar) == 'S')
              {
                local.AparSelection.Text1 = "P";
              }

              if (AsChar(export.ArCommon.SelectChar) == 'S')
              {
                local.AparSelection.Text1 = "R";
              }

              // ---------------------------------------------
              // 06/17/99 W.Campbell - End of change made
              // to disable one line of code and replace it
              // with statements to reinitialize the text_1
              // attribute to contain the correct value of
              // "P" or "R" for AR or AP based on the
              // input values.
              // ---------------------------------------------
              local.Infrastructure.ReasonCode = "";
              local.DetailText30.Text30 = "";
            }
          }

          // --------------------------------------------
          // Code to call cab to close monitored doc
          // --------------------------------------------
          // ---------------------------------------------
          // 02/18/99 M Ramirez & W.Campbell
          // Disabled statements
          // dealing with closing monitored documents,
          // as it has been determined that the best way
          // to handle them will be in Batch.
          // ---------------------------------------------
          // --------------------------------------------
          // End of Code
          // --------------------------------------------
        }
        else
        {
        }
      }

      export.Group.CheckIndex();

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        // 11/01/99 M.L Start
        export.Hidden.Index = export.Group.Index;
        export.Hidden.CheckSize();

        export.Group.Update.HiddenDet.Assign(
          export.Group.Item.DetCsePersonAddress);
        export.Hidden.Update.HsendDate.SendDate =
          export.Group.Item.DetCsePersonAddress.SendDate;

        // 11/01/99 M.L End
      }

      export.Group.CheckIndex();

      // -------------------------------------------------------------------------------------
      // Per WR# 261, when a Prison/Jail record was added on JAIL screen for a 
      // person, that address will be populated on ADDR screen. On ADDR screen
      // the 'Send_Dt' for that Prison/Jail address must be protected and a
      // message :  " Not able to send PM letter on a Prison/Jail address", will
      // be displayed on the ADDR screen,
      //                                                 
      // Vithal Madhira (04/09/2001)
      // ------------------------------------------------------------------------------------
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.DetCsePersonAddress.Source, "JAIL"))
        {
          var field1 =
            GetField(export.Group.Item.DetCsePersonAddress, "sendDate");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Group.Item.DetCsePersonAddress, "source");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Group.Item.SrcePrmt, "text1");

          field3.Color = "cyan";
          field3.Protected = true;
        }
      }

      export.Group.CheckIndex();
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
    // mjr
    // -----------------------------------------------------
    // 11/08/1999
    // Changed document from batch to online
    // ------------------------------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      if (AsChar(local.SendDateUpdated.Flag) == 'Y')
      {
        // mjr
        // -----------------------------------------------------
        // 11/08/1999
        // Verify that the AR is a client
        // ------------------------------------------------------------------
        if (Equal(local.SpDocKey.KeyPerson, export.ArCsePersonsWorkSet.Number))
        {
          if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
          {
            return;
          }
        }

        // mjr
        // -----------------------------------------------------
        // 11/15/1999
        // Case must be open
        // ------------------------------------------------------------------
        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          return;
        }

        // mjr
        // -----------------------------------------------------
        // 01/14/2000
        // Clear NEXT TRAN before invoking print process
        // ------------------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.Null1);

        // mjr
        // -----------------------------------------------------
        // 11/08/1999
        // Changed document from batch to online
        // ------------------------------------------------------------------
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + local.Document.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.HiddenNextTranInfo.CaseNumber = local.SpDocKey.KeyCase;
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdPrNumber) + local.SpDocKey.KeyPerson;
        local.BatchTimestampWorkArea.IefTimestamp =
          local.SpDocKey.KeyPersonAddress;
        UseLeCabConvertTimestamp();
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(export.HiddenNextTranInfo.MiscText1) + ";" + TrimEnd
          (local.SpDocLiteral.IdPersonAddress) + local
          .BatchTimestampWorkArea.TextTimestamp;

        // mjr
        // ---------------------------------------------------------------------
        // Place AP in next tran for Re-Display upon return from Print process
        // ------------------------------------------------------------------------
        export.HiddenNextTranInfo.CsePersonNumberAp =
          export.ApCsePersonsWorkSet.Number;
        local.PrintProcess.Flag = "Y";
        local.PrintProcess.Command = "PRINT";
        UseScCabNextTranPut2();
      }
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress5(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress6(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.VerifiedDate = source.VerifiedDate;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress7(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress8(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.County = source.County;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress9(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress10(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.Identifier = source.Identifier;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroup(SiBuildAddresses.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.DetCommon.SelectChar = source.GdetailCommon.SelectChar;
    target.DetCsePersonAddress.Assign(source.GdetailCsePersonAddress);
    target.StatePrmt.Text1 = source.GstatePrmt.Text1;
    target.EnddtCdPrmt.Text1 = source.GenddtCdPrmt.Text1;
    target.SrcePrmt.Text1 = source.GsrcePrmt.Text1;
    target.ReturnPrmt.Text1 = source.GreturnPrmpt.Text1;
    target.TypePrmt.Text1 = source.GtypePrmpt.Text1;
    target.CntyPrmt.Text1 = source.GcntyPrmpt.Text1;
    target.HiddenDet.Assign(source.Ghidden);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdPersonAddress = source.IdPersonAddress;
    target.IdPrNumber = source.IdPrNumber;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabReturnKsCountyByZip1()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress9(export.EmptyAddr, useImport.CsePersonAddress);
    MoveCsePersonAddress8(export.EmptyAddr, useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress8(useExport.CsePersonAddress, export.EmptyAddr);
  }

  private void UseEabReturnKsCountyByZip2()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress9(export.Group.Item.DetCsePersonAddress,
      useImport.CsePersonAddress);
    MoveCsePersonAddress8(export.Group.Item.DetCsePersonAddress,
      useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress8(useExport.CsePersonAddress,
      export.Group.Update.DetCsePersonAddress);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
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

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
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

    useImport.CsePersonsWorkSet.Number = import.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv1()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = local.FvTest.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv2()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.Save.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.AparSelection.Text1 = local.AparSelection.Text1;

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiBuildAddresses1()
  {
    var useImport = new SiBuildAddresses.Import();
    var useExport = new SiBuildAddresses.Export();

    useImport.Common.Command = local.Command.Command;
    useImport.CsePersonsWorkSet.Number = export.Save.Number;
    MoveCsePersonAddress10(export.LastAddr, useImport.LastAddr);

    Call(SiBuildAddresses.Execute, useImport, useExport);

    export.ForeignAddr.Text1 = useExport.ForeignAddr.Text1;
    MoveCsePersonAddress7(useExport.Ae, export.AeAddr);
    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseSiBuildAddresses2()
  {
    var useImport = new SiBuildAddresses.Import();
    var useExport = new SiBuildAddresses.Export();

    useImport.Common.Command = local.Command.Command;
    useImport.CsePersonsWorkSet.Number = export.Save.Number;
    MoveCsePersonAddress10(export.LastAddr, useImport.LastAddr);

    Call(SiBuildAddresses.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseSiCheckZipIsNumeric()
  {
    var useImport = new SiCheckZipIsNumeric.Import();
    var useExport = new SiCheckZipIsNumeric.Export();

    MoveCsePersonAddress9(local.CheckZip, useImport.CsePersonAddress);

    Call(SiCheckZipIsNumeric.Execute, useImport, useExport);

    local.NumericZip.Flag = useExport.NumericZip.Flag;
  }

  private void UseSiCreateAutoCsenetTrans1()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.CsePerson.Number = local.Csenet.Number;
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.Misc.Timestamp = local.Miscellaneous.Timestamp;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans2()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.Misc.Timestamp = local.Miscellaneous.Timestamp;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress1()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    MoveCsePersonAddress4(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    local.Asterisk.Identifier = useExport.CsePersonAddress.Identifier;
  }

  private void UseSiCreateCsePersonAddress2()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress3(export.EmptyAddr, useImport.CsePersonAddress);

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, export.EmptyAddr);
  }

  private void UseSiReadArInformation()
  {
    var useImport = new SiReadArInformation.Import();
    var useExport = new SiReadArInformation.Export();

    useImport.Ar.Number = export.ArFromCaseRole.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadArInformation.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.ApActive.Flag = useExport.ApActive.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void UseSiUpdateCsePersonAddress()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress5(export.Group.Item.DetCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);

    export.Group.Update.DetCsePersonAddress.LastUpdatedTimestamp =
      useExport.CsePersonAddress.LastUpdatedTimestamp;
  }

  private void UseSpAddrExternalAlert()
  {
    var useImport = new SpAddrExternalAlert.Import();
    var useExport = new SpAddrExternalAlert.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    MoveCsePersonAddress6(export.Group.Item.DetCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SpAddrExternalAlert.Execute, useImport, useExport);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
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
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", local.LastTran.CsePersonNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detCsePersonAddress")]
      public CsePersonAddress DetCsePersonAddress
      {
        get => detCsePersonAddress ??= new();
        set => detCsePersonAddress = value;
      }

      /// <summary>
      /// A value of StatePrmt.
      /// </summary>
      [JsonPropertyName("statePrmt")]
      public WorkArea StatePrmt
      {
        get => statePrmt ??= new();
        set => statePrmt = value;
      }

      /// <summary>
      /// A value of EnddtCdPrmt.
      /// </summary>
      [JsonPropertyName("enddtCdPrmt")]
      public WorkArea EnddtCdPrmt
      {
        get => enddtCdPrmt ??= new();
        set => enddtCdPrmt = value;
      }

      /// <summary>
      /// A value of SrcePrmt.
      /// </summary>
      [JsonPropertyName("srcePrmt")]
      public WorkArea SrcePrmt
      {
        get => srcePrmt ??= new();
        set => srcePrmt = value;
      }

      /// <summary>
      /// A value of ReturnPrmt.
      /// </summary>
      [JsonPropertyName("returnPrmt")]
      public WorkArea ReturnPrmt
      {
        get => returnPrmt ??= new();
        set => returnPrmt = value;
      }

      /// <summary>
      /// A value of TypePrmt.
      /// </summary>
      [JsonPropertyName("typePrmt")]
      public WorkArea TypePrmt
      {
        get => typePrmt ??= new();
        set => typePrmt = value;
      }

      /// <summary>
      /// A value of CntyPrmt.
      /// </summary>
      [JsonPropertyName("cntyPrmt")]
      public WorkArea CntyPrmt
      {
        get => cntyPrmt ??= new();
        set => cntyPrmt = value;
      }

      /// <summary>
      /// A value of HiddenDet.
      /// </summary>
      [JsonPropertyName("hiddenDet")]
      public CsePersonAddress HiddenDet
      {
        get => hiddenDet ??= new();
        set => hiddenDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detCommon;
      private CsePersonAddress detCsePersonAddress;
      private WorkArea statePrmt;
      private WorkArea enddtCdPrmt;
      private WorkArea srcePrmt;
      private WorkArea returnPrmt;
      private WorkArea typePrmt;
      private WorkArea cntyPrmt;
      private CsePersonAddress hiddenDet;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HsendDate.
      /// </summary>
      [JsonPropertyName("hsendDate")]
      public CsePersonAddress HsendDate
      {
        get => hsendDate ??= new();
        set => hsendDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePersonAddress hsendDate;
    }

    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of LastAddr.
      /// </summary>
      [JsonPropertyName("lastAddr")]
      public CsePersonAddress LastAddr
      {
        get => lastAddr ??= new();
        set => lastAddr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonAddress lastAddr;
    }

    /// <summary>
    /// A value of PrevCommandPerson.
    /// </summary>
    [JsonPropertyName("prevCommandPerson")]
    public WorkArea PrevCommandPerson
    {
      get => prevCommandPerson ??= new();
      set => prevCommandPerson = value;
    }

    /// <summary>
    /// A value of PrevCommandCase.
    /// </summary>
    [JsonPropertyName("prevCommandCase")]
    public WorkArea PrevCommandCase
    {
      get => prevCommandCase ??= new();
      set => prevCommandCase = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of ArFromCaseRole.
    /// </summary>
    [JsonPropertyName("arFromCaseRole")]
    public CsePersonsWorkSet ArFromCaseRole
    {
      get => arFromCaseRole ??= new();
      set => arFromCaseRole = value;
    }

    /// <summary>
    /// A value of MtReturnPrmt.
    /// </summary>
    [JsonPropertyName("mtReturnPrmt")]
    public WorkArea MtReturnPrmt
    {
      get => mtReturnPrmt ??= new();
      set => mtReturnPrmt = value;
    }

    /// <summary>
    /// A value of MtTypePrmt.
    /// </summary>
    [JsonPropertyName("mtTypePrmt")]
    public WorkArea MtTypePrmt
    {
      get => mtTypePrmt ??= new();
      set => mtTypePrmt = value;
    }

    /// <summary>
    /// A value of ForeignAddress.
    /// </summary>
    [JsonPropertyName("foreignAddress")]
    public LocalWorkAddr ForeignAddress
    {
      get => foreignAddress ??= new();
      set => foreignAddress = value;
    }

    /// <summary>
    /// A value of ForeignAddr.
    /// </summary>
    [JsonPropertyName("foreignAddr")]
    public WorkArea ForeignAddr
    {
      get => foreignAddr ??= new();
      set => foreignAddr = value;
    }

    /// <summary>
    /// A value of PromptCode.
    /// </summary>
    [JsonPropertyName("promptCode")]
    public Code PromptCode
    {
      get => promptCode ??= new();
      set => promptCode = value;
    }

    /// <summary>
    /// A value of PromptCodeValue.
    /// </summary>
    [JsonPropertyName("promptCodeValue")]
    public CodeValue PromptCodeValue
    {
      get => promptCodeValue ??= new();
      set => promptCodeValue = value;
    }

    /// <summary>
    /// A value of MtStatePrmt.
    /// </summary>
    [JsonPropertyName("mtStatePrmt")]
    public WorkArea MtStatePrmt
    {
      get => mtStatePrmt ??= new();
      set => mtStatePrmt = value;
    }

    /// <summary>
    /// A value of MtEnddtCdPrmt.
    /// </summary>
    [JsonPropertyName("mtEnddtCdPrmt")]
    public WorkArea MtEnddtCdPrmt
    {
      get => mtEnddtCdPrmt ??= new();
      set => mtEnddtCdPrmt = value;
    }

    /// <summary>
    /// A value of MtSrcePrmt.
    /// </summary>
    [JsonPropertyName("mtSrcePrmt")]
    public WorkArea MtSrcePrmt
    {
      get => mtSrcePrmt ??= new();
      set => mtSrcePrmt = value;
    }

    /// <summary>
    /// A value of MtCntyPrmt.
    /// </summary>
    [JsonPropertyName("mtCntyPrmt")]
    public WorkArea MtCntyPrmt
    {
      get => mtCntyPrmt ??= new();
      set => mtCntyPrmt = value;
    }

    /// <summary>
    /// A value of ApList.
    /// </summary>
    [JsonPropertyName("apList")]
    public WorkArea ApList
    {
      get => apList ??= new();
      set => apList = value;
    }

    /// <summary>
    /// A value of ArList.
    /// </summary>
    [JsonPropertyName("arList")]
    public WorkArea ArList
    {
      get => arList ??= new();
      set => arList = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CsePersonsWorkSet Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of EmptyAddrSelect.
    /// </summary>
    [JsonPropertyName("emptyAddrSelect")]
    public Common EmptyAddrSelect
    {
      get => emptyAddrSelect ??= new();
      set => emptyAddrSelect = value;
    }

    /// <summary>
    /// A value of EmptyAddr.
    /// </summary>
    [JsonPropertyName("emptyAddr")]
    public CsePersonAddress EmptyAddr
    {
      get => emptyAddr ??= new();
      set => emptyAddr = value;
    }

    /// <summary>
    /// A value of AeAddr.
    /// </summary>
    [JsonPropertyName("aeAddr")]
    public CsePersonAddress AeAddr
    {
      get => aeAddr ??= new();
      set => aeAddr = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// Gets a value of Pagenum.
    /// </summary>
    [JsonIgnore]
    public Array<PagenumGroup> Pagenum => pagenum ??= new(
      PagenumGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pagenum for json serialization.
    /// </summary>
    [JsonPropertyName("pagenum")]
    [Computed]
    public IList<PagenumGroup> Pagenum_Json
    {
      get => pagenum;
      set => Pagenum.Assign(value);
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
    /// A value of Alrt.
    /// </summary>
    [JsonPropertyName("alrt")]
    public NextTranInfo Alrt
    {
      get => alrt ??= new();
      set => alrt = value;
    }

    private WorkArea prevCommandPerson;
    private WorkArea prevCommandCase;
    private WorkArea headerLine;
    private CsePerson hiddenCsePerson;
    private Common caseOpen;
    private Common apActive;
    private CsePersonsWorkSet arFromCaseRole;
    private WorkArea mtReturnPrmt;
    private WorkArea mtTypePrmt;
    private LocalWorkAddr foreignAddress;
    private WorkArea foreignAddr;
    private Code promptCode;
    private CodeValue promptCodeValue;
    private WorkArea mtStatePrmt;
    private WorkArea mtEnddtCdPrmt;
    private WorkArea mtSrcePrmt;
    private WorkArea mtCntyPrmt;
    private WorkArea apList;
    private WorkArea arList;
    private CsePersonsWorkSet save;
    private Common saveSubscript;
    private CsePersonAddress lastAddr;
    private WorkArea plus;
    private WorkArea minus;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 next;
    private Case1 case1;
    private Common apCommon;
    private Common arCommon;
    private Common emptyAddrSelect;
    private CsePersonAddress emptyAddr;
    private CsePersonAddress aeAddr;
    private Case1 hiddenNext;
    private CsePersonsWorkSet hiddenAp;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<GroupGroup> group;
    private Array<HiddenGroup> hidden;
    private Array<PagenumGroup> pagenum;
    private NextTranInfo hiddenNextTranInfo;
    private NextTranInfo alrt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detCsePersonAddress")]
      public CsePersonAddress DetCsePersonAddress
      {
        get => detCsePersonAddress ??= new();
        set => detCsePersonAddress = value;
      }

      /// <summary>
      /// A value of StatePrmt.
      /// </summary>
      [JsonPropertyName("statePrmt")]
      public WorkArea StatePrmt
      {
        get => statePrmt ??= new();
        set => statePrmt = value;
      }

      /// <summary>
      /// A value of EnddtCdPrmt.
      /// </summary>
      [JsonPropertyName("enddtCdPrmt")]
      public WorkArea EnddtCdPrmt
      {
        get => enddtCdPrmt ??= new();
        set => enddtCdPrmt = value;
      }

      /// <summary>
      /// A value of SrcePrmt.
      /// </summary>
      [JsonPropertyName("srcePrmt")]
      public WorkArea SrcePrmt
      {
        get => srcePrmt ??= new();
        set => srcePrmt = value;
      }

      /// <summary>
      /// A value of ReturnPrmt.
      /// </summary>
      [JsonPropertyName("returnPrmt")]
      public WorkArea ReturnPrmt
      {
        get => returnPrmt ??= new();
        set => returnPrmt = value;
      }

      /// <summary>
      /// A value of TypePrmt.
      /// </summary>
      [JsonPropertyName("typePrmt")]
      public WorkArea TypePrmt
      {
        get => typePrmt ??= new();
        set => typePrmt = value;
      }

      /// <summary>
      /// A value of CntyPrmt.
      /// </summary>
      [JsonPropertyName("cntyPrmt")]
      public WorkArea CntyPrmt
      {
        get => cntyPrmt ??= new();
        set => cntyPrmt = value;
      }

      /// <summary>
      /// A value of HiddenDet.
      /// </summary>
      [JsonPropertyName("hiddenDet")]
      public CsePersonAddress HiddenDet
      {
        get => hiddenDet ??= new();
        set => hiddenDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detCommon;
      private CsePersonAddress detCsePersonAddress;
      private WorkArea statePrmt;
      private WorkArea enddtCdPrmt;
      private WorkArea srcePrmt;
      private WorkArea returnPrmt;
      private WorkArea typePrmt;
      private WorkArea cntyPrmt;
      private CsePersonAddress hiddenDet;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HsendDate.
      /// </summary>
      [JsonPropertyName("hsendDate")]
      public CsePersonAddress HsendDate
      {
        get => hsendDate ??= new();
        set => hsendDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePersonAddress hsendDate;
    }

    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of LastAddr.
      /// </summary>
      [JsonPropertyName("lastAddr")]
      public CsePersonAddress LastAddr
      {
        get => lastAddr ??= new();
        set => lastAddr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonAddress lastAddr;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of MtCntyPrmt.
    /// </summary>
    [JsonPropertyName("mtCntyPrmt")]
    public WorkArea MtCntyPrmt
    {
      get => mtCntyPrmt ??= new();
      set => mtCntyPrmt = value;
    }

    /// <summary>
    /// A value of ArFromCaseRole.
    /// </summary>
    [JsonPropertyName("arFromCaseRole")]
    public CsePersonsWorkSet ArFromCaseRole
    {
      get => arFromCaseRole ??= new();
      set => arFromCaseRole = value;
    }

    /// <summary>
    /// A value of ForeignAddress.
    /// </summary>
    [JsonPropertyName("foreignAddress")]
    public LocalWorkAddr ForeignAddress
    {
      get => foreignAddress ??= new();
      set => foreignAddress = value;
    }

    /// <summary>
    /// A value of ForeignAddr.
    /// </summary>
    [JsonPropertyName("foreignAddr")]
    public WorkArea ForeignAddr
    {
      get => foreignAddr ??= new();
      set => foreignAddr = value;
    }

    /// <summary>
    /// A value of PromptCodeValue.
    /// </summary>
    [JsonPropertyName("promptCodeValue")]
    public CodeValue PromptCodeValue
    {
      get => promptCodeValue ??= new();
      set => promptCodeValue = value;
    }

    /// <summary>
    /// A value of PromptCode.
    /// </summary>
    [JsonPropertyName("promptCode")]
    public Code PromptCode
    {
      get => promptCode ??= new();
      set => promptCode = value;
    }

    /// <summary>
    /// A value of MtStatePrmt.
    /// </summary>
    [JsonPropertyName("mtStatePrmt")]
    public WorkArea MtStatePrmt
    {
      get => mtStatePrmt ??= new();
      set => mtStatePrmt = value;
    }

    /// <summary>
    /// A value of MtEnddtCdPrmt.
    /// </summary>
    [JsonPropertyName("mtEnddtCdPrmt")]
    public WorkArea MtEnddtCdPrmt
    {
      get => mtEnddtCdPrmt ??= new();
      set => mtEnddtCdPrmt = value;
    }

    /// <summary>
    /// A value of MtSrcePrmt.
    /// </summary>
    [JsonPropertyName("mtSrcePrmt")]
    public WorkArea MtSrcePrmt
    {
      get => mtSrcePrmt ??= new();
      set => mtSrcePrmt = value;
    }

    /// <summary>
    /// A value of MtReturnPrmt.
    /// </summary>
    [JsonPropertyName("mtReturnPrmt")]
    public WorkArea MtReturnPrmt
    {
      get => mtReturnPrmt ??= new();
      set => mtReturnPrmt = value;
    }

    /// <summary>
    /// A value of MtTypePrmt.
    /// </summary>
    [JsonPropertyName("mtTypePrmt")]
    public WorkArea MtTypePrmt
    {
      get => mtTypePrmt ??= new();
      set => mtTypePrmt = value;
    }

    /// <summary>
    /// A value of ApList.
    /// </summary>
    [JsonPropertyName("apList")]
    public WorkArea ApList
    {
      get => apList ??= new();
      set => apList = value;
    }

    /// <summary>
    /// A value of ArList.
    /// </summary>
    [JsonPropertyName("arList")]
    public WorkArea ArList
    {
      get => arList ??= new();
      set => arList = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CsePersonsWorkSet Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of EmptyAddrSelect.
    /// </summary>
    [JsonPropertyName("emptyAddrSelect")]
    public Common EmptyAddrSelect
    {
      get => emptyAddrSelect ??= new();
      set => emptyAddrSelect = value;
    }

    /// <summary>
    /// A value of EmptyAddr.
    /// </summary>
    [JsonPropertyName("emptyAddr")]
    public CsePersonAddress EmptyAddr
    {
      get => emptyAddr ??= new();
      set => emptyAddr = value;
    }

    /// <summary>
    /// A value of AeAddr.
    /// </summary>
    [JsonPropertyName("aeAddr")]
    public CsePersonAddress AeAddr
    {
      get => aeAddr ??= new();
      set => aeAddr = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// Gets a value of Pagenum.
    /// </summary>
    [JsonIgnore]
    public Array<PagenumGroup> Pagenum => pagenum ??= new(
      PagenumGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pagenum for json serialization.
    /// </summary>
    [JsonPropertyName("pagenum")]
    [Computed]
    public IList<PagenumGroup> Pagenum_Json
    {
      get => pagenum;
      set => Pagenum.Assign(value);
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
    /// A value of Alrt.
    /// </summary>
    [JsonPropertyName("alrt")]
    public NextTranInfo Alrt
    {
      get => alrt ??= new();
      set => alrt = value;
    }

    private WorkArea headerLine;
    private CsePerson hiddenCsePerson;
    private Common caseOpen;
    private Common apActive;
    private WorkArea mtCntyPrmt;
    private CsePersonsWorkSet arFromCaseRole;
    private LocalWorkAddr foreignAddress;
    private WorkArea foreignAddr;
    private CodeValue promptCodeValue;
    private Code promptCode;
    private WorkArea mtStatePrmt;
    private WorkArea mtEnddtCdPrmt;
    private WorkArea mtSrcePrmt;
    private WorkArea mtReturnPrmt;
    private WorkArea mtTypePrmt;
    private WorkArea apList;
    private WorkArea arList;
    private CsePersonsWorkSet save;
    private Common saveSubscript;
    private CsePersonAddress lastAddr;
    private WorkArea plus;
    private WorkArea minus;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 next;
    private Case1 case1;
    private Common apCommon;
    private Common arCommon;
    private Common emptyAddrSelect;
    private CsePersonAddress emptyAddr;
    private CsePersonAddress aeAddr;
    private CsePersonsWorkSet hiddenAp;
    private Case1 hiddenNext;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<GroupGroup> group;
    private Array<HiddenGroup> hidden;
    private Array<PagenumGroup> pagenum;
    private NextTranInfo hiddenNextTranInfo;
    private NextTranInfo alrt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SaveUpdatesGroup group.</summary>
    [Serializable]
    public class SaveUpdatesGroup
    {
      /// <summary>
      /// A value of AddrId.
      /// </summary>
      [JsonPropertyName("addrId")]
      public CsePersonAddress AddrId
      {
        get => addrId ??= new();
        set => addrId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePersonAddress addrId;
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
    /// A value of Stored.
    /// </summary>
    [JsonPropertyName("stored")]
    public NextTranInfo Stored
    {
      get => stored ??= new();
      set => stored = value;
    }

    /// <summary>
    /// A value of FplsDisplayFlag.
    /// </summary>
    [JsonPropertyName("fplsDisplayFlag")]
    public Common FplsDisplayFlag
    {
      get => fplsDisplayFlag ??= new();
      set => fplsDisplayFlag = value;
    }

    /// <summary>
    /// A value of PrevCommandPerson.
    /// </summary>
    [JsonPropertyName("prevCommandPerson")]
    public WorkArea PrevCommandPerson
    {
      get => prevCommandPerson ??= new();
      set => prevCommandPerson = value;
    }

    /// <summary>
    /// A value of PrevCommandCase.
    /// </summary>
    [JsonPropertyName("prevCommandCase")]
    public WorkArea PrevCommandCase
    {
      get => prevCommandCase ??= new();
      set => prevCommandCase = value;
    }

    /// <summary>
    /// A value of Csnet.
    /// </summary>
    [JsonPropertyName("csnet")]
    public Common Csnet
    {
      get => csnet ??= new();
      set => csnet = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
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
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
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
    /// A value of NumericZip.
    /// </summary>
    [JsonPropertyName("numericZip")]
    public Common NumericZip
    {
      get => numericZip ??= new();
      set => numericZip = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public CsePersonAddress CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// Gets a value of SaveUpdates.
    /// </summary>
    [JsonIgnore]
    public Array<SaveUpdatesGroup> SaveUpdates => saveUpdates ??= new(
      SaveUpdatesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SaveUpdates for json serialization.
    /// </summary>
    [JsonPropertyName("saveUpdates")]
    [Computed]
    public IList<SaveUpdatesGroup> SaveUpdates_Json
    {
      get => saveUpdates;
      set => SaveUpdates.Assign(value);
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Asterisk.
    /// </summary>
    [JsonPropertyName("asterisk")]
    public CsePersonAddress Asterisk
    {
      get => asterisk ??= new();
      set => asterisk = value;
    }

    /// <summary>
    /// A value of BlankCommon.
    /// </summary>
    [JsonPropertyName("blankCommon")]
    public Common BlankCommon
    {
      get => blankCommon ??= new();
      set => blankCommon = value;
    }

    /// <summary>
    /// A value of BlankCsePersonAddress.
    /// </summary>
    [JsonPropertyName("blankCsePersonAddress")]
    public CsePersonAddress BlankCsePersonAddress
    {
      get => blankCsePersonAddress ??= new();
      set => blankCsePersonAddress = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of BuildAddressList.
    /// </summary>
    [JsonPropertyName("buildAddressList")]
    public Common BuildAddressList
    {
      get => buildAddressList ??= new();
      set => buildAddressList = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of Prmpt.
    /// </summary>
    [JsonPropertyName("prmpt")]
    public Common Prmpt
    {
      get => prmpt ??= new();
      set => prmpt = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
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
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
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
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
    }

    /// <summary>
    /// A value of DetailText1.
    /// </summary>
    [JsonPropertyName("detailText1")]
    public WorkArea DetailText1
    {
      get => detailText1 ??= new();
      set => detailText1 = value;
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
    /// A value of Addr4CloseDoc.
    /// </summary>
    [JsonPropertyName("addr4CloseDoc")]
    public CsePersonAddress Addr4CloseDoc
    {
      get => addr4CloseDoc ??= new();
      set => addr4CloseDoc = value;
    }

    /// <summary>
    /// A value of BlankAddr4CloseDoc.
    /// </summary>
    [JsonPropertyName("blankAddr4CloseDoc")]
    public CsePersonAddress BlankAddr4CloseDoc
    {
      get => blankAddr4CloseDoc ??= new();
      set => blankAddr4CloseDoc = value;
    }

    /// <summary>
    /// A value of Miscellaneous.
    /// </summary>
    [JsonPropertyName("miscellaneous")]
    public DateWorkArea Miscellaneous
    {
      get => miscellaneous ??= new();
      set => miscellaneous = value;
    }

    /// <summary>
    /// A value of FvCheck.
    /// </summary>
    [JsonPropertyName("fvCheck")]
    public Common FvCheck
    {
      get => fvCheck ??= new();
      set => fvCheck = value;
    }

    /// <summary>
    /// A value of FvTest.
    /// </summary>
    [JsonPropertyName("fvTest")]
    public CsePersonsWorkSet FvTest
    {
      get => fvTest ??= new();
      set => fvTest = value;
    }

    private DateWorkArea todayPlus6Months;
    private NextTranInfo stored;
    private Common fplsDisplayFlag;
    private WorkArea prevCommandPerson;
    private WorkArea prevCommandCase;
    private Common csnet;
    private NextTranInfo null1;
    private WorkArea print;
    private Common position;
    private Common printProcess;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private SpDocLiteral spDocLiteral;
    private Common length;
    private Common save;
    private CsePerson csenet;
    private Common screenIdentification;
    private Document document;
    private SpDocKey spDocKey;
    private Common numericZip;
    private CsePersonAddress checkZip;
    private DateWorkArea maximum;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Array<SaveUpdatesGroup> saveUpdates;
    private Infrastructure infrastructure;
    private CsePersonAddress csePersonAddress;
    private CsePersonAddress asterisk;
    private Common blankCommon;
    private CsePersonAddress blankCsePersonAddress;
    private DateWorkArea dateWorkArea;
    private Common buildAddressList;
    private Common command;
    private Common prmpt;
    private CsePerson csePerson;
    private Common returnCode;
    private Code code;
    private CodeValue codeValue;
    private Common multipleAps;
    private Common error;
    private Common select;
    private Common sendDateUpdated;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private TextWorkArea detailText30;
    private TextWorkArea detailText10;
    private DateWorkArea date;
    private WorkArea aparSelection;
    private WorkArea detailText1;
    private InterfaceAlert interfaceAlert;
    private Infrastructure lastTran;
    private WorkArea closeMonitoredDoc;
    private CsePersonAddress addr4CloseDoc;
    private CsePersonAddress blankAddr4CloseDoc;
    private DateWorkArea miscellaneous;
    private Common fvCheck;
    private CsePersonsWorkSet fvTest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
