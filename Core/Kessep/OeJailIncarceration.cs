// Program: OE_JAIL_INCARCERATION, ID: 371794467, model: 746.
// Short name: SWEJAILP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_JAIL_INCARCERATION.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeJailIncarceration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_JAIL_INCARCERATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeJailIncarceration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeJailIncarceration.
  /// </summary>
  public OeJailIncarceration(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------
    // Date      Author          Reason
    // Jan 1995  Rebecca Grimes  Initial Development
    // 02/21/95  Sid             Rework  and
    //                                   
    // Completion.
    // 06/03/96  Siraj Konkader  Print functions
    // 06/03/96  siraj konkader  prompt to name list
    // 06/26/96  Regan Welborn   Left Pad EAB Call
    // 11/04/96  Regan Welborn   Exclude CSE Person from CLEAR
    // 11/08/96  R. Marchman     Add new security and next tran
    // 12/18/96  Raju		  event insertion
    // 01/18/97  Raju		  upon return from HIST screen,
    // 			  display rec which created event
    // 06/12/97  R Grey	  Add Parole/Probation to detail line text
    // 12/30/1998	M Ramirez	Revised print process.
    // 12/30/1998	M Ramirez	Changed security to check CRUD actions only.
    // 12/30/1998	M Ramirez	Removed command "Enter".  Will be handled in
    // 				Case of "Otherwise" in main case of command.
    // 10/14/99	PMcElderry
    // Logic to protect kansas jail information during add / update
    // and allow freeform for out-of-state jails.
    // PR 77964 - force USER to CLEAR screen before an add is
    // performed
    // 01/05/99  Sree Veettil
    //  PR00083770- Added conditions after adding new correctional facility 
    // LABETTE COUNTY CORRECTIONAL FACILITY.
    // 01/06/2000	M Ramirez	83300		NEXT TRAN needs to be cleared before print 
    // process is invoked
    // ------------------------------------------------------------------
    // 04/04/00 W.Campbell      Disabled existing call to
    //                          Security Cab and added a
    //                          new call with view matching
    //                          changed to match the export
    //                          views of case and cse_person.
    //                          Work done on WR#000162
    //                          for PRWORA - Family Violence.
    // 12/01/2000     VITHAL MADHIRA            PR# 107790  New edits for '
    // Release date', 'Incarceration_date' and fixed UPDATE problems.
    // 01/25/01  SWSRCHF 000238   Return Super NEXTTRAN to ALRT
    // --------------------------------------------------------
    // =======================================================================================
    // 03/08/2001                 Vithal Madhira                  WR# 000261
    // When adding or changing the Jail address automatically display that 
    // address on ADDR screen.
    // 03/08/2001                  Vithal Madhira                  WR# 000273
    // Add an indicator to JAIL screen that demonstrates the person was not 
    // found or not known at the facility by parole/probation officer. Workers
    // have no way of recording that a person was not found there, and the
    // business rules require a release date before entering a new jail/prison
    // record. This indicator can be used as an ending event just like release
    // date if the the person is not found or not known.
    // ======================================================================================
    // 04/09/01  SWSRCHF I00116862   Reset NEXT TRAN INFO to the imported values
    // from ALRT,
    //                               
    // before returning there (ALRT)
    // ---------------------------------------------------------------------------------------
    // 3/31/17   JHarden     CQ55280  Inmate number flow to ADDR
    // 
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    UseSpDocSetLiterals();

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);
    export.Alrt.Assign(import.Alrt);

    // **** end   group A ****
    // ----------------------------------------------------
    // Per Kim Williams, a change that ensures CLEAR won't
    // erase the CSE Person Number from the screen.  RVW
    // 11/4/96
    // ----------------------------------------------------
    export.CsePerson.Number = import.CsePerson.Number;
    export.H.Number = import.H.Number;
    export.ClearPerfrmedBeforeAdd.Command =
      import.ClearPerfrmedBeforeAdd.Command;

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Allow the user to clear the screen
      // ---------------------------------------------
      if (!IsEmpty(import.WorkNameCsePersonsWorkSet.FormattedName))
      {
        export.WorkNameCsePersonsWorkSet.FormattedName =
          import.WorkNameCsePersonsWorkSet.FormattedName;
      }

      export.ClearPerfrmedBeforeAdd.Command = "";
      export.Facl.Flag = "";

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.PersonPrompt.SelectChar = import.PersonPrompt.SelectChar;
    export.HiddenSelectedPrison.Assign(import.HiddenSelectedPrison);
    export.Facl.Flag = import.Facl.Flag;
    export.WorkNameOeWorkGroup.FormattedNameText =
      import.WorkNameOeWorkGroup.FormattedNameText;
    export.WorkNameCsePersonsWorkSet.Assign(import.WorkNameCsePersonsWorkSet);
    export.JailCommon.SelectChar = import.JailCommon.SelectChar;
    export.Parole.SelectChar = import.Parole.SelectChar;
    export.Prison.SelectChar = import.Prison.SelectChar;
    export.ProbationCommon.SelectChar = import.ProbationCommon.SelectChar;
    export.HiddenJailCommon.SelectChar = import.HiddenJailCommon.SelectChar;
    export.HiddenParole.SelectChar = import.HiddenParole.SelectChar;
    export.HiddenPrison.SelectChar = import.HiddenPrison.SelectChar;
    export.HiddenProbationCommon.SelectChar =
      import.HiddenProbationCommon.SelectChar;
    export.PromptFacility.SelectChar = import.PromptFacility.SelectChar;
    export.PromptState1.SelectChar = import.PromptState1.SelectChar;
    export.PromptState2.SelectChar = import.PromptState2.SelectChar;
    export.JailIncarceration.Assign(import.JailIncarceration);
    export.ProbationIncarceration.Assign(import.ProbationIncarceration);
    MoveIncarcerationAddress2(import.JailIncarcerationAddress,
      export.JailIncarcerationAddress);
    export.ProbationIncarcerationAddress.Assign(
      import.ProbationIncarcerationAddress);
    export.HiddenJailIncarcerationAddress.Assign(
      import.HiddenJailIncarcerationAddress);
    export.HiddenProbationIncarcerationAddress.Assign(
      import.HiddenProbationIncarcerationAddress);
    local.TextWorkArea.Text10 = import.Case1.Number;
    UseEabPadLeftWithZeros();
    export.Case1.Number = local.TextWorkArea.Text10;
    local.TextWorkArea.Text10 = import.CsePerson.Number;
    UseEabPadLeftWithZeros();
    export.CsePerson.Number = local.TextWorkArea.Text10;

    // -----------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // -----------------------------------------------
    export.H.Number = import.H.Number;
    export.HiddenProbationIncarceration.Assign(
      import.HiddenProbationIncarceration);
    export.HiddenJailIncarceration.Assign(import.HiddenJailIncarceration);
    export.ClearPerfrmedBeforeAdd.Command =
      import.ClearPerfrmedBeforeAdd.Command;

    // --------------------------------------------
    // statement added by Raju : 12:25 hrs CST
    // --------------------------------------------
    export.LastReadHidden.StartDate = import.LastReadHidden.StartDate;

    // ---------------------------------------------
    //           N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    // ---------------------------------------------
    //   S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // --------------------------------------------------------------
    // if the next tran info is not equal to spaces, this implies the
    // user requested a next tran action. now validate
    // --------------------------------------------------------------
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // -------------------------------------------------------------
      // this is where you would set the local next_tran_info
      // attributes to the import view attributes for the data to be
      // passed to the next transaction
      // -------------------------------------------------------------
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      export.Hidden.CaseNumber = export.Case1.Number;
      UseScCabNextTranPut1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      // -------------------------------------------------------------
      // this is where you set your export value to the export hidden
      // next tran values if the user is comming into this procedure
      // on a next tran action.
      // -------------------------------------------------------------
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);

      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1035 hrs CST)
      // ---------------------------------------------
      // *** Problem report I00116862
      // *** 04/09/01 swsrchf
      // *** start
      if (Equal(export.Hidden.LastTran, "SRPQ"))
      {
        export.Alrt.Assign(export.Hidden);
      }

      // *** end
      // *** 04/09/01 swsrchf
      // *** Problem report I00116862
      if (Equal(export.Hidden.LastTran, "SRPT") || Equal
        (export.Hidden.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.Hidden.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.CsePerson.Number = local.LastTran.CsePersonNumber ?? Spaces(10);
        export.JailIncarceration.Identifier =
          (int)local.LastTran.DenormNumeric12.GetValueOrDefault();
      }

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ------------------------------------------------------
      // this is where you set your command to do whatever is
      // necessary to do on a flow from the menu, maybe just
      // escape....
      // -------------------------------------------------------
      // -----------------------------------------------------------------
      // You should get this information from the Dialog Flow
      // Diagram.  It is the SEND CMD on the propertis for a Transfer
      // from one  procedure to another.
      // -----------------------------------------------------------------
      // -------------------------------------------------------
      // the statement would read COMMAND IS display
      // -------------------------------------------------------
      // ----------------------------------------------------------
      // if the dialog flow property was display first, just add an
      // escape completely out of the procedure
      // ----------------------------------------------------------
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // ---------------------------------------------
    //     P F K E Y    P R O C E S S I N G
    // ---------------------------------------------
    // ---------------------------------------------------------
    // If the end-date is not entered, SET the end date to a max
    // date of 2199/12/31.
    // ---------------------------------------------------------
    UseOeCabSetMnemonics();

    // --------------------------------------------------------------
    // If the Institution Address is left blank and the State entered
    // as KS, default the address to the Kansas DOC.
    // --------------------------------------------------------------
    if (IsEmpty(export.JailIncarceration.InstitutionName) && IsEmpty
      (export.JailIncarcerationAddress.Street1) && IsEmpty
      (export.JailIncarcerationAddress.Street2) && IsEmpty
      (export.JailIncarcerationAddress.City) && Equal
      (export.JailIncarcerationAddress.State, "KS"))
    {
      export.JailIncarceration.InstitutionName =
        "KANSAS DEPARTMENT OF CORRECTIONS";
      export.JailIncarceration.PhoneAreaCode = 913;
      export.JailIncarceration.Phone = 2963317;
      export.JailIncarceration.PhoneExt = "";
      export.JailIncarcerationAddress.Street1 = "LANDON STATE OFFICE BLDG.";
      export.JailIncarcerationAddress.Street2 = "900 SW JACKSON, RM 404-N";
      export.JailIncarcerationAddress.City = "TOPEKA";
      export.JailIncarcerationAddress.ZipCode5 = "66612";
      export.JailIncarcerationAddress.ZipCode4 = "1284";

      return;
    }

    if (!Equal(export.CsePerson.Number, export.H.Number))
    {
      export.Facl.Flag = "";
    }

    if (Equal(export.CsePerson.Number, export.H.Number))
    {
    }
    else if (Equal(global.Command, "ADDJAIL") || Equal
      (global.Command, "ADDPAROL") || Equal(global.Command, "CHANJAIL") || Equal
      (global.Command, "CHANPROB"))
    {
      if (IsEmpty(export.H.Number))
      {
      }
      else
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "OE0000_KEY_CHANGE_NA";

        return;
      }
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    switch(TrimEnd(global.Command))
    {
      case "RETCOMP":
        export.PersonPrompt.SelectChar = "";

        if (AsChar(export.PromptFacility.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptFacility, "selectChar");

          field1.Error = true;
        }

        if (AsChar(export.PromptState1.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState1, "selectChar");

          field1.Error = true;
        }

        if (AsChar(export.PromptState2.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState2, "selectChar");

          field1.Error = true;
        }

        if (IsEmpty(export.WorkNameCsePersonsWorkSet.Number))
        {
          return;
        }

        export.CsePerson.Number = export.WorkNameCsePersonsWorkSet.Number;
        global.Command = "DISPLAY";

        break;
      case "FROMNAME":
        // Siraj Konkader 6/3/96
        // Added logic for Link to Name list on prompt
        export.PersonPrompt.SelectChar = "";

        if (AsChar(export.PromptFacility.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptFacility, "selectChar");

          field1.Error = true;
        }

        if (AsChar(export.PromptState1.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState1, "selectChar");

          field1.Error = true;
        }

        if (AsChar(export.PromptState2.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState2, "selectChar");

          field1.Error = true;
        }

        if (IsEmpty(export.WorkNameCsePersonsWorkSet.Number))
        {
          return;
        }

        export.CsePerson.Number = export.WorkNameCsePersonsWorkSet.Number;
        global.Command = "DISPLAY";

        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "RETURN":
        // *** Problem report I00116862
        // *** 04/09/01 swsrchf
        // *** start
        if (Equal(export.Hidden.LastTran, "SRPQ"))
        {
          export.Hidden.Assign(export.Alrt);
        }

        // *** end
        // *** 04/09/01 swsrchf
        // *** Problem report I00116862
        // *** Work request 000238
        // *** 01/25/01 swsrchf
        // *** added check for 'SRPQ'
        if (Equal(export.Hidden.LastTran, "SRPT") || Equal
          (export.Hidden.LastTran, "SRPU") || Equal
          (export.Hidden.LastTran, "SRPQ"))
        {
          global.NextTran = (export.Hidden.LastTran ?? "") + " " + "XXNEXTXX";
        }
        else
        {
          // **** begin group E ****
          ExitState = "ACO_NE0000_RETURN";

          // **** end   group E ****
        }

        break;
      case "RETCDVL":
        if (AsChar(export.PromptFacility.SelectChar) == 'S')
        {
          export.PromptFacility.SelectChar = "";

          var field1 = GetField(export.Parole, "selectChar");

          field1.Protected = false;
          field1.Focused = true;
        }
        else if (AsChar(export.PromptState1.SelectChar) == 'S')
        {
          export.JailIncarcerationAddress.State = import.SelectedState.Cdvalue;
          export.PromptState1.SelectChar = "";

          var field1 = GetField(export.JailIncarcerationAddress, "zipCode5");

          field1.Protected = false;
          field1.Focused = true;
        }
        else if (AsChar(export.PromptState2.SelectChar) == 'S')
        {
          export.ProbationIncarcerationAddress.State =
            import.SelectedState.Cdvalue;
          export.PromptState2.SelectChar = "";

          var field1 =
            GetField(export.ProbationIncarcerationAddress, "zipCode5");

          field1.Protected = false;
          field1.Focused = true;
        }

        if (AsChar(export.PromptState1.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState1, "selectChar");

          field1.Error = true;
        }

        if (AsChar(export.PromptState2.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState2, "selectChar");

          field1.Error = true;
        }

        return;
      case "DISPPAGE":
        // ---------------------------------------------------------
        // USER may create and update non KS and KS correctional
        // facilities not brought over from FACL - otherwise data is
        // protected
        // ---------------------------------------------------------
        if (!IsEmpty(import.SelectedPrisonIncarceration.InstitutionName))
        {
          export.Facl.Flag = "Y";

          // ---------------------------------------------------------
          // The control has been returned from the Correction Address
          // screen. Populate the screen with the address details and
          // protect fields.
          // ---------------------------------------------------------
          export.JailIncarceration.InstitutionName =
            import.SelectedPrisonIncarceration.InstitutionName ?? "";
          export.JailIncarceration.PhoneAreaCode =
            import.SelectedPrisonIncarceration.PhoneAreaCode.
              GetValueOrDefault();
          export.JailIncarceration.Phone =
            import.SelectedPrisonIncarceration.Phone.GetValueOrDefault();
          export.JailIncarceration.PhoneExt =
            import.SelectedPrisonIncarceration.PhoneExt ?? "";
          export.JailIncarcerationAddress.Street1 =
            import.SelectedPrisonIncarcerationAddress.Street1 ?? "";
          export.JailIncarcerationAddress.Street2 =
            import.SelectedPrisonIncarcerationAddress.Street2 ?? "";
          export.JailIncarcerationAddress.State =
            import.SelectedPrisonIncarcerationAddress.State ?? "";
          export.JailIncarcerationAddress.City =
            import.SelectedPrisonIncarcerationAddress.City ?? "";
          export.JailIncarcerationAddress.ZipCode5 =
            import.SelectedPrisonIncarcerationAddress.ZipCode5 ?? "";
          export.JailIncarcerationAddress.ZipCode4 =
            import.SelectedPrisonIncarcerationAddress.ZipCode4 ?? "";

          var field1 = GetField(export.PromptState1, "selectChar");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        export.PromptFacility.SelectChar = "";

        var field = GetField(export.Parole, "selectChar");

        field.Protected = false;
        field.Focused = true;

        if (AsChar(export.PromptState1.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState1, "selectChar");

          field1.Error = true;
        }

        if (AsChar(export.PromptState2.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptState2, "selectChar");

          field1.Error = true;
        }

        if (IsEmpty(export.Facl.Flag))
        {
          // -----------------------------------------------------------------------
          // This ensures proper fields will be protected if USER has a
          // facility that was supposed to be protected, went to FACL but
          // didnt select anything
          // ------------------------------------------------------------------------
          switch(TrimEnd(export.JailIncarceration.InstitutionName ?? ""))
          {
            case "ELLSWORTH CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "EL DORADO CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "HUTCHINSON CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "LANSING CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "LARNED CORRECTIONAL MENTAL HEALTH":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "NORTON CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "TOPEKA CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "WICHITA WORK RELEASE FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "WINFIELD CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "LABETTE CORRECTIONAL FACILITY":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "LABETTE CORR CONSERVATION CAMP":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            case "LABETTE WOMENS CORR CAMP":
              if (Equal(export.JailIncarcerationAddress.State, "KS"))
              {
                export.Facl.Flag = "Y";
              }

              break;
            default:
              // ------------------------------------------------------------
              // Continue processing - do not protect fields.  Information
              // provided was from a non FACL facility
              // -------------------------------------------------------------
              export.Facl.Flag = "";

              break;
          }
        }

        if (!IsEmpty(export.Facl.Flag))
        {
          var field1 = GetField(export.JailIncarceration, "institutionName");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.JailIncarceration, "phoneAreaCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.JailIncarceration, "phone");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.JailIncarceration, "phoneExt");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.JailIncarcerationAddress, "street1");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.JailIncarcerationAddress, "street2");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.JailIncarcerationAddress, "state");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.JailIncarcerationAddress, "city");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.JailIncarcerationAddress, "zipCode5");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.JailIncarcerationAddress, "zipCode4");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.PromptState1, "selectChar");

          field11.Color = "cyan";
          field11.Protected = true;
        }

        return;
      case "LIST":
        // **** begin group D ****
        // -------------------------------------------------------------
        // for the cases where you link from 1 procedure to another
        // procedure, you must set the export_hidden security
        // link_indicator to "L".
        // this will tell the called procedure that we are on a link and
        // not a transfer.  Don't forget to do the view matching on the
        // dialog design screen.
        // -------------------------------------------------------------
        // **** end   group D ****
        local.ReturnCode.Count = 0;

        if (!IsEmpty(export.PromptState2.SelectChar))
        {
          ++local.ReturnCode.Count;

          if (AsChar(export.PromptState2.SelectChar) != 'S')
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.PromptState2, "selectChar");

            field1.Error = true;
          }
        }

        if (!IsEmpty(export.PromptState1.SelectChar))
        {
          ++local.ReturnCode.Count;

          if (AsChar(export.PromptState1.SelectChar) != 'S')
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.PromptState1, "selectChar");

            field1.Error = true;
          }
        }

        if (!IsEmpty(export.PromptFacility.SelectChar))
        {
          ++local.ReturnCode.Count;

          if (AsChar(export.PromptFacility.SelectChar) != 'S')
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.PromptFacility, "selectChar");

            field1.Error = true;
          }
        }

        if (!IsEmpty(export.PersonPrompt.SelectChar))
        {
          ++local.ReturnCode.Count;

          if (AsChar(export.PersonPrompt.SelectChar) != 'S')
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.PersonPrompt, "selectChar");

            field1.Error = true;
          }
        }

        if (IsEmpty(export.PersonPrompt.SelectChar) && IsEmpty
          (export.PromptFacility.SelectChar) && IsEmpty
          (export.PromptState1.SelectChar) && IsEmpty
          (export.PromptState2.SelectChar))
        {
          var field1 = GetField(export.PromptState2, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptState1, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.PromptFacility, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.PersonPrompt, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.PersonPrompt.SelectChar) == 'S')
        {
          if (IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
          else
          {
            ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";
          }

          return;
        }

        if (AsChar(export.Prison.SelectChar) != 'Y' && AsChar
          (export.PromptFacility.SelectChar) == 'Y')
        {
          ExitState = "OE0000_CANT_FLOW_TO_LIST_ADDRESS";

          var field1 = GetField(export.Prison, "selectChar");

          field1.Error = true;

          break;
        }

        if (AsChar(export.PromptFacility.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CORRCTN_FACILITY_FACL";

          return;
        }

        if (AsChar(export.PromptState2.SelectChar) == 'S' || AsChar
          (export.PromptState1.SelectChar) == 'S')
        {
          export.State.CodeName = local.State.CodeName;
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        break;
      case "ADDJAIL":
        if (IsEmpty(export.ClearPerfrmedBeforeAdd.Command))
        {
        }
        else
        {
          ExitState = "FN0000_CLEAR_BEFORE_ADD_2";

          break;
        }

        // -------------------------------------------------------------
        // If the jail/prison records are selected, the following
        // validation must take place.
        // -------------------------------------------------------------
        if (!IsEmpty(export.JailIncarcerationAddress.State))
        {
          local.CodeValue.Cdvalue = export.JailIncarcerationAddress.State ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (local.ReturnCode.Count != 0)
          {
            var field1 = GetField(export.JailIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        // -------------------------------------------------------------
        // If the Address is entered, then the Address details should
        // be complete.
        // -------------------------------------------------------------
        local.FlowAddress.Flag = "N";

        if (!IsEmpty(import.JailIncarcerationAddress.Street1) || !
          IsEmpty(import.JailIncarcerationAddress.Street2) || !
          IsEmpty(import.JailIncarcerationAddress.City) || !
          IsEmpty(import.JailIncarcerationAddress.State) || !
          IsEmpty(import.JailIncarcerationAddress.ZipCode5))
        {
          local.FlowAddress.Flag = "Y";

          if (IsEmpty(import.JailIncarcerationAddress.ZipCode5))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            do
            {
              ++local.CheckZip.Count;
              local.CheckZip.Flag =
                Substring(import.JailIncarcerationAddress.ZipCode5,
                local.CheckZip.Count, 1);

              if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                (local.CheckZip.Flag) > '9')
              {
                var field1 =
                  GetField(export.JailIncarcerationAddress, "zipCode5");

                field1.Error = true;

                ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";

                break;
              }
            }
            while(local.CheckZip.Count < 5);
          }

          if (IsEmpty(import.JailIncarcerationAddress.State))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.JailIncarcerationAddress.City))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "city");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.JailIncarcerationAddress.Street1))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "street1");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(import.JailIncarcerationAddress.ZipCode4))
        {
        }
        else
        {
          if (IsEmpty(import.JailIncarcerationAddress.ZipCode5))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          local.CheckZip.Count = 0;
          local.CheckZip.Flag = "";

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.JailIncarcerationAddress.ZipCode4,
              local.CheckZip.Count, 1);

            if (AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              var field1 =
                GetField(export.JailIncarcerationAddress, "zipCode4");

              field1.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";

              break;
            }
          }
          while(local.CheckZip.Count < 4);
        }

        // ---------------------------------------------
        // Check for complete Phone Number and Area Code.
        // ---------------------------------------------
        if (export.JailIncarceration.PhoneAreaCode.GetValueOrDefault() != 0 || export
          .JailIncarceration.Phone.GetValueOrDefault() != 0 || !
          IsEmpty(export.JailIncarceration.PhoneExt))
        {
          if (export.JailIncarceration.PhoneAreaCode.GetValueOrDefault() == 0)
          {
            var field1 = GetField(export.JailIncarceration, "phoneAreaCode");

            field1.Error = true;

            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }

          if (export.JailIncarceration.Phone.GetValueOrDefault() == 0)
          {
            var field1 = GetField(export.JailIncarceration, "phone");

            field1.Error = true;

            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        // -------------------------------------------------------------
        // The institution name is required before any information can
        // be entered on the CSE Person.
        // -------------------------------------------------------------
        if (IsEmpty(import.JailIncarceration.InstitutionName))
        {
          var field1 = GetField(export.JailIncarceration, "institutionName");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // -------------------------------------------------------------
        // If the work release indicator is not equal to "Y" or "N",
        // highlight the indicator and send an appropriate message
        // back to the user indicating that the indicator must be a "Y"
        // or "N".
        // -------------------------------------------------------------
        if (IsEmpty(import.JailIncarceration.WorkReleaseInd))
        {
          export.JailIncarceration.WorkReleaseInd = "N";
        }
        else if (AsChar(export.JailIncarceration.WorkReleaseInd) != 'Y' && AsChar
          (export.JailIncarceration.WorkReleaseInd) != 'N')
        {
          var field1 = GetField(export.JailIncarceration, "workReleaseInd");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";
        }

        // -------------------------------------------------------
        // The date the CSE Person is released from incarceration
        // cannot be greater than the current date.
        // -------------------------------------------------------
        if (Lt(Now().Date, import.JailIncarceration.EndDate) && !
          Equal(import.JailIncarceration.EndDate, local.MaxDate.ExpirationDate))
        {
          var field1 = GetField(export.JailIncarceration, "endDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // The date the CSE Person entered incarceration cannot be
        // greater than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.JailIncarceration.StartDate))
        {
          var field1 = GetField(export.JailIncarceration, "startDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // The date the CSE Person is eligible for parole cannot be
        // less than the current date.
        // -------------------------------------------------------------
        if (import.JailIncarceration.ParoleEligibilityDate != null && Lt
          (import.JailIncarceration.ParoleEligibilityDate, Now().Date))
        {
          var field1 =
            GetField(export.JailIncarceration, "paroleEligibilityDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";
        }

        // -------------------------------------------------------------
        // The date the prison/jail record is verified cannot be greater
        // than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.JailIncarceration.VerifiedDate))
        {
          var field1 = GetField(export.JailIncarceration, "verifiedDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // There cannot be a "Y" in both the prison and jail indicator
        // fields.  The CSE Person is either in jail or in prison.
        // -------------------------------------------------------------
        if (AsChar(import.JailCommon.SelectChar) == 'Y' && (
          AsChar(import.Prison.SelectChar) == 'N' || IsEmpty
          (import.Prison.SelectChar)) || (
            AsChar(import.JailCommon.SelectChar) == 'N' || IsEmpty
          (import.JailCommon.SelectChar)) && AsChar
          (import.Prison.SelectChar) == 'Y')
        {
        }
        else
        {
          var field1 = GetField(export.JailCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.Prison, "selectChar");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";
        }

        // --------------------------------------------------------------------
        // Per WR# 273,  the following edits are needed.
        //                                                      
        // Vithal.
        // --------------------------------------------------------------------
        switch(AsChar(export.JailIncarceration.Incarcerated))
        {
          case ' ':
            if (Lt(local.Blank.Date, export.JailIncarceration.VerifiedDate))
            {
              var field2 = GetField(export.JailIncarceration, "incarcerated");

              field2.Error = true;

              ExitState = "ACO_NI0000_ENTER_Y_OR_N";
            }

            break;
          case 'Y':
            if (Equal(export.JailIncarceration.VerifiedDate, local.Blank.Date))
            {
              var field2 = GetField(export.JailIncarceration, "verifiedDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            break;
          case 'N':
            if (Equal(export.JailIncarceration.VerifiedDate, local.Blank.Date))
            {
              export.JailIncarceration.VerifiedDate = local.Current.Date;
            }

            break;
          default:
            var field1 = GetField(export.JailIncarceration, "incarcerated");

            field1.Error = true;

            ExitState = "INVALID_VALUE";

            break;
        }

        // --------------------------------------------------------------------
        // Set the incarceration type according to the flag selection.  If
        // the prison flag is selected type is "P", jail type is "J",
        // parole type is "R", and probation type is "R".
        // ---------------------------------------------------------------------
        if (AsChar(import.JailCommon.SelectChar) == 'Y')
        {
          export.JailIncarceration.Type1 = "J";
        }
        else if (AsChar(import.Prison.SelectChar) == 'Y')
        {
          export.JailIncarceration.Type1 = "P";
        }

        if (IsEmpty(import.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          UseOeCabCheckCaseMember1();

          if (!IsEmpty(local.WorkError.Flag))
          {
            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            if (Equal(export.CsePerson.Number, export.H.Number))
            {
              export.WorkNameCsePersonsWorkSet.Assign(
                import.WorkNameCsePersonsWorkSet);
              export.WorkNameOeWorkGroup.FormattedNameText =
                import.WorkNameOeWorkGroup.FormattedNameText;
            }

            ExitState = "CSE_PERSON_NF";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Equal(export.CsePerson.Number, export.H.Number))
          {
            export.WorkNameCsePersonsWorkSet.Assign(
              import.WorkNameCsePersonsWorkSet);
            export.WorkNameOeWorkGroup.FormattedNameText =
              import.WorkNameOeWorkGroup.FormattedNameText;
          }

          local.FlowAddress.Flag = "N";
          global.Command = "BYPASS";
        }
        else
        {
          MoveIncarceration2(export.JailIncarceration, local.StartIncarceration);
            
          MoveIncarcerationAddress1(export.JailIncarcerationAddress,
            local.StartIncarcerationAddress);

          if (Lt(local.Blank.Date, export.JailIncarceration.VerifiedDate))
          {
            if (AsChar(export.JailIncarceration.Incarcerated) == 'Y')
            {
              local.RaiseEventFlag.Text1 = "V";
            }
          }

          if (Lt(local.Blank.Date, export.JailIncarceration.EndDate))
          {
            if (AsChar(export.JailIncarceration.Incarcerated) == 'Y')
            {
              local.RaiseEventFlag.Text1 = "E";
            }
          }

          global.Command = "CREATE";
        }

        break;
      case "ADDPAROL":
        // ---------------------------------------------
        // If the parole/probation records are selected,
        // the following validation must take place.
        // ---------------------------------------------
        if (!IsEmpty(export.ProbationIncarcerationAddress.State))
        {
          local.CodeValue.Cdvalue =
            export.ProbationIncarcerationAddress.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.ReturnCode.Count != 0)
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        // -------------------------------------------------------------
        // If the Address is entered, then the Address details should
        // be complete.
        // -------------------------------------------------------------
        local.FlowAddress.Flag = "N";

        if (!IsEmpty(import.ProbationIncarcerationAddress.Street1) || !
          IsEmpty(import.ProbationIncarcerationAddress.Street2) || !
          IsEmpty(import.ProbationIncarcerationAddress.City) || !
          IsEmpty(import.ProbationIncarcerationAddress.State) || !
          IsEmpty(import.ProbationIncarcerationAddress.ZipCode5))
        {
          local.FlowAddress.Flag = "Y";

          if (IsEmpty(import.ProbationIncarcerationAddress.ZipCode5))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            do
            {
              ++local.CheckZip.Count;
              local.CheckZip.Flag =
                Substring(import.ProbationIncarcerationAddress.ZipCode5,
                local.CheckZip.Count, 1);

              if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                (local.CheckZip.Flag) > '9')
              {
                var field1 =
                  GetField(export.ProbationIncarcerationAddress, "zipCode5");

                field1.Error = true;

                ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";

                break;
              }
            }
            while(local.CheckZip.Count < 5);
          }

          if (IsEmpty(import.ProbationIncarcerationAddress.State))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.ProbationIncarcerationAddress.City))
          {
            var field1 = GetField(export.ProbationIncarcerationAddress, "city");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.ProbationIncarcerationAddress.Street1))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "street1");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(import.ProbationIncarcerationAddress.ZipCode4))
        {
        }
        else
        {
          if (IsEmpty(import.ProbationIncarcerationAddress.ZipCode5))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          local.CheckZip.Count = 0;
          local.CheckZip.Flag = "";

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.ProbationIncarcerationAddress.ZipCode4,
              local.CheckZip.Count, 1);

            if (AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              var field1 =
                GetField(export.ProbationIncarcerationAddress, "zipCode4");

              field1.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";

              break;
            }
          }
          while(local.CheckZip.Count < 4);
        }

        // ---------------------------------------------
        // Check for complete Phone Number and Area Code.
        // ---------------------------------------------
        if (export.ProbationIncarceration.PhoneAreaCode.GetValueOrDefault() != 0
          || export.ProbationIncarceration.Phone.GetValueOrDefault() != 0 || !
          IsEmpty(export.ProbationIncarceration.PhoneExt))
        {
          if (export.ProbationIncarceration.PhoneAreaCode.GetValueOrDefault() ==
            0)
          {
            var field1 =
              GetField(export.ProbationIncarceration, "phoneAreaCode");

            field1.Error = true;

            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }

          if (export.ProbationIncarceration.Phone.GetValueOrDefault() == 0)
          {
            var field1 = GetField(export.ProbationIncarceration, "phone");

            field1.Error = true;

            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        // -------------------------------------------------------------
        // The parole/probation officer's first and last name are
        // required in order to create an incarceration record.
        // -------------------------------------------------------------
        if (IsEmpty(import.ProbationIncarceration.ParoleOfficerLastName) && IsEmpty
          (import.ProbationIncarceration.ParoleOfficerFirstName))
        {
          var field1 =
            GetField(export.ProbationIncarceration, "paroleOfficerLastName");

          field1.Error = true;

          var field2 =
            GetField(export.ProbationIncarceration, "paroleOfficerFirstName");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsEmpty(import.ProbationIncarceration.ParoleOfficerFirstName) || !
          IsEmpty(import.ProbationIncarceration.ParoleOfficerLastName) || !
          IsEmpty(import.ProbationIncarceration.ParoleOfficerMiddleInitial))
        {
          if (IsEmpty(import.ProbationIncarceration.ParoleOfficerFirstName))
          {
            var field1 =
              GetField(export.ProbationIncarceration, "paroleOfficerFirstName");
              

            field1.Error = true;

            ExitState = "CONTACT_OFFICER_FIRST_NAME_REQD";
          }

          if (IsEmpty(import.ProbationIncarceration.ParoleOfficerLastName))
          {
            var field1 =
              GetField(export.ProbationIncarceration, "paroleOfficerLastName");

            field1.Error = true;

            ExitState = "OE0188_CONTACT_OFFICER_LASTNAME";
          }
        }

        // -------------------------------------------------------------
        // The date the CSE Person is released from incarceration
        // cannot be greater than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, export.ProbationIncarceration.EndDate) && !
          Equal(export.ProbationIncarceration.EndDate,
          local.MaxDate.ExpirationDate))
        {
          var field1 = GetField(export.ProbationIncarceration, "endDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // The date the parole/probation record is verified cannot be
        // greater than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.ProbationIncarceration.VerifiedDate))
        {
          var field1 = GetField(export.ProbationIncarceration, "verifiedDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // There cannot be a "Y" in both the prison and jail indicator
        // fields.  The CSE Person is either in jail or in prison.
        // -------------------------------------------------------------
        if (AsChar(import.Parole.SelectChar) == 'Y' && (
          AsChar(import.ProbationCommon.SelectChar) == 'N' || IsEmpty
          (import.ProbationCommon.SelectChar)) || (
            AsChar(import.Parole.SelectChar) == 'N' || IsEmpty
          (import.Parole.SelectChar)) && AsChar
          (import.ProbationCommon.SelectChar) == 'Y')
        {
        }
        else
        {
          var field1 = GetField(export.Parole, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ProbationCommon, "selectChar");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";
        }

        // --------------------------------------------------------------------
        // Per WR# 273,  the following edits are needed.
        //                                                      
        // Vithal.
        // --------------------------------------------------------------------
        switch(AsChar(export.ProbationIncarceration.Incarcerated))
        {
          case ' ':
            if (Lt(local.Blank.Date, export.ProbationIncarceration.VerifiedDate))
              
            {
              var field2 =
                GetField(export.ProbationIncarceration, "incarcerated");

              field2.Error = true;

              ExitState = "ACO_NI0000_ENTER_Y_OR_N";
            }

            break;
          case 'Y':
            if (Equal(export.ProbationIncarceration.VerifiedDate,
              local.Blank.Date))
            {
              var field2 =
                GetField(export.ProbationIncarceration, "verifiedDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            break;
          case 'N':
            if (Equal(export.ProbationIncarceration.VerifiedDate,
              local.Blank.Date))
            {
              export.ProbationIncarceration.VerifiedDate = local.Current.Date;
            }

            break;
          default:
            var field1 =
              GetField(export.ProbationIncarceration, "incarcerated");

            field1.Error = true;

            ExitState = "INVALID_VALUE";

            break;
        }

        // -----------------------------------------------------------------
        // Set the incarceration type according to the flag selection.  If
        // the prison flag is selected type is "P", jail type is "J",
        // parole type is "R", and probation type is "T"
        // -----------------------------------------------------------------
        if (AsChar(import.Parole.SelectChar) == 'Y')
        {
          export.ProbationIncarceration.Type1 = "R";
        }
        else if (AsChar(import.ProbationCommon.SelectChar) == 'Y')
        {
          export.ProbationIncarceration.Type1 = "T";
        }

        if (IsEmpty(import.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          UseOeCabCheckCaseMember2();

          if (!IsEmpty(local.WorkError.Flag))
          {
            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            ExitState = "CSE_PERSON_NF";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "BYPASS";
          local.FlowAddress.Flag = "N";
        }
        else
        {
          MoveIncarceration3(export.ProbationIncarceration,
            local.StartIncarceration);
          MoveIncarcerationAddress2(export.ProbationIncarcerationAddress,
            local.StartIncarcerationAddress);
          global.Command = "CREATE";
        }

        break;
      case "CHANJAIL":
        if (!Equal(export.CsePerson.Number, export.H.Number) || IsEmpty
          (export.H.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "OE0013_DISP_REC_BEFORE_UPD";

          return;
        }

        // -------------------------------------------------------------
        // Check for any modified data. If data is not changed, no need to 
        // update.
        //                                             
        // ---- Vithal (12/02/2000)
        // -------------------------------------------------------------
        if (AsChar(export.Prison.SelectChar) == AsChar
          (export.HiddenPrison.SelectChar) && AsChar
          (export.JailCommon.SelectChar) == AsChar
          (export.HiddenJailCommon.SelectChar) && Equal
          (export.JailIncarceration.VerifiedDate,
          export.HiddenJailIncarceration.VerifiedDate) && Equal
          (export.JailIncarceration.InmateNumber,
          export.HiddenJailIncarceration.InmateNumber) && Equal
          (export.JailIncarceration.StartDate,
          export.HiddenJailIncarceration.StartDate) && Equal
          (export.JailIncarceration.ParoleEligibilityDate,
          export.HiddenJailIncarceration.ParoleEligibilityDate) && Equal
          (export.JailIncarceration.EndDate,
          export.HiddenJailIncarceration.EndDate) && AsChar
          (export.JailIncarceration.WorkReleaseInd) == AsChar
          (export.HiddenJailIncarceration.WorkReleaseInd) && Equal
          (export.JailIncarceration.ConditionsForRelease,
          export.HiddenJailIncarceration.ConditionsForRelease) && Equal
          (export.JailIncarceration.InstitutionName,
          export.HiddenJailIncarceration.InstitutionName) && export
          .JailIncarceration.PhoneAreaCode.GetValueOrDefault() == export
          .HiddenJailIncarceration.PhoneAreaCode.GetValueOrDefault() && export
          .JailIncarceration.Phone.GetValueOrDefault() == export
          .HiddenJailIncarceration.Phone.GetValueOrDefault() && Equal
          (export.JailIncarceration.PhoneExt,
          export.HiddenJailIncarceration.PhoneExt) && AsChar
          (export.JailIncarceration.Incarcerated) == AsChar
          (export.HiddenJailIncarceration.Incarcerated) && Equal
          (export.JailIncarcerationAddress.Street1,
          export.HiddenJailIncarcerationAddress.Street1) && Equal
          (export.JailIncarcerationAddress.Street2,
          export.HiddenJailIncarcerationAddress.Street2) && Equal
          (export.JailIncarcerationAddress.City,
          export.HiddenJailIncarcerationAddress.City) && Equal
          (export.JailIncarcerationAddress.State,
          export.HiddenJailIncarcerationAddress.State) && Equal
          (export.JailIncarcerationAddress.ZipCode5,
          export.HiddenJailIncarcerationAddress.ZipCode5) && Equal
          (export.JailIncarcerationAddress.ZipCode4,
          export.HiddenJailIncarcerationAddress.ZipCode4))
        {
          local.JailInfoModified.Flag = "N";
        }
        else
        {
          local.JailInfoModified.Flag = "Y";
        }

        if (AsChar(local.JailInfoModified.Flag) == 'N')
        {
          ExitState = "FN0000_NO_CHANGE_TO_UPDATE";
          global.Command = "BYPASS";

          break;
        }

        // -------------------------------------------------------------
        // If the jail/prison records are selected, the following
        // validation must take place.
        // -------------------------------------------------------------
        if (!IsEmpty(export.JailIncarcerationAddress.State))
        {
          local.CodeValue.Cdvalue = export.JailIncarcerationAddress.State ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (local.ReturnCode.Count != 0)
          {
            var field1 = GetField(export.JailIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        // -------------------------------------------------------------
        // If the Address is entered, then the Address details should
        // be complete.
        // -------------------------------------------------------------
        if (!IsEmpty(import.JailIncarcerationAddress.Street1) || !
          IsEmpty(import.JailIncarcerationAddress.Street2) || !
          IsEmpty(import.JailIncarcerationAddress.City) || !
          IsEmpty(import.JailIncarcerationAddress.State) || !
          IsEmpty(import.JailIncarcerationAddress.ZipCode5))
        {
          if (IsEmpty(import.JailIncarcerationAddress.ZipCode5))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            do
            {
              ++local.CheckZip.Count;
              local.CheckZip.Flag =
                Substring(import.JailIncarcerationAddress.ZipCode5,
                local.CheckZip.Count, 1);

              if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                (local.CheckZip.Flag) > '9')
              {
                var field1 =
                  GetField(export.JailIncarcerationAddress, "zipCode5");

                field1.Error = true;

                ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";

                break;
              }
            }
            while(local.CheckZip.Count < 5);
          }

          if (IsEmpty(import.JailIncarcerationAddress.State))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.JailIncarcerationAddress.City))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "city");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.JailIncarcerationAddress.Street1))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "street1");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(import.JailIncarcerationAddress.ZipCode4))
        {
        }
        else
        {
          if (IsEmpty(import.JailIncarcerationAddress.ZipCode5))
          {
            var field1 = GetField(export.JailIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          local.CheckZip.Count = 0;
          local.CheckZip.Flag = "";

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.JailIncarcerationAddress.ZipCode4,
              local.CheckZip.Count, 1);

            if (AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              var field1 =
                GetField(export.JailIncarcerationAddress, "zipCode4");

              field1.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";

              break;
            }
          }
          while(local.CheckZip.Count < 4);
        }

        // ---------------------------------------------
        // Check for complete Phone Number and Area Code.
        // ---------------------------------------------
        if (export.JailIncarceration.PhoneAreaCode.GetValueOrDefault() != 0 || export
          .JailIncarceration.Phone.GetValueOrDefault() != 0 || !
          IsEmpty(export.JailIncarceration.PhoneExt))
        {
          if (export.JailIncarceration.PhoneAreaCode.GetValueOrDefault() == 0)
          {
            var field1 = GetField(export.JailIncarceration, "phoneAreaCode");

            field1.Error = true;

            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }

          if (export.JailIncarceration.Phone.GetValueOrDefault() == 0)
          {
            var field1 = GetField(export.JailIncarceration, "phone");

            field1.Error = true;

            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        // -------------------------------------------------------------
        // The institution name is required before any information can
        // be entered on the CSE Person.
        // -------------------------------------------------------------
        if (IsEmpty(import.JailIncarceration.InstitutionName))
        {
          var field1 = GetField(export.JailIncarceration, "institutionName");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // ---------------------------------------------
        // If the work release indicator is not equal to
        // "Y" or "N", highlight the indicator and send
        // an appropriate message back to the user
        // indicating that the indicator must be a "Y"
        // or "N".
        // ---------------------------------------------
        if (IsEmpty(import.JailIncarceration.WorkReleaseInd))
        {
          export.JailIncarceration.WorkReleaseInd = "N";
        }
        else if (AsChar(export.JailIncarceration.WorkReleaseInd) != 'Y' && AsChar
          (export.JailIncarceration.WorkReleaseInd) != 'N')
        {
          var field1 = GetField(export.JailIncarceration, "workReleaseInd");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";
        }

        // -------------------------------------------------------------
        // The date the CSE Person is released from incarceration
        // cannot be greater than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.JailIncarceration.EndDate) && !
          Equal(import.JailIncarceration.EndDate, local.MaxDate.ExpirationDate))
        {
          var field1 = GetField(export.JailIncarceration, "endDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // The date the CSE Person entered incarceration cannot be
        // greater than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.JailIncarceration.StartDate))
        {
          var field1 = GetField(export.JailIncarceration, "startDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // The date the CSE Person is eligible for parole cannot be
        // less than the current date.
        // -------------------------------------------------------------
        if (!Equal(import.JailIncarceration.ParoleEligibilityDate,
          export.HiddenJailIncarceration.ParoleEligibilityDate))
        {
          if (import.JailIncarceration.ParoleEligibilityDate != null)
          {
            if (Lt(import.JailIncarceration.ParoleEligibilityDate, Now().Date))
            {
              var field1 =
                GetField(export.JailIncarceration, "paroleEligibilityDate");

              field1.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";
            }
          }
        }

        // -------------------------------------------------------------
        // The date the prison/jail record is verified cannot be greater
        // than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.JailIncarceration.VerifiedDate))
        {
          var field1 = GetField(export.JailIncarceration, "verifiedDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // There cannot be a "Y" in both the prison and jail indicator
        // fields.  The CSE Person is either in jail or in prison.
        // -------------------------------------------------------------
        if (AsChar(import.JailCommon.SelectChar) == 'Y' && (
          AsChar(import.Prison.SelectChar) == 'N' || IsEmpty
          (import.Prison.SelectChar)) || (
            AsChar(import.JailCommon.SelectChar) == 'N' || IsEmpty
          (import.JailCommon.SelectChar)) && AsChar
          (import.Prison.SelectChar) == 'Y')
        {
        }
        else
        {
          var field1 = GetField(export.JailCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.Prison, "selectChar");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";
        }

        // --------------------------------------------------------------------
        // Per WR# 273,  the following edits are needed.
        //                                                      
        // Vithal.
        // --------------------------------------------------------------------
        switch(AsChar(export.JailIncarceration.Incarcerated))
        {
          case ' ':
            if (Lt(local.Blank.Date, export.JailIncarceration.VerifiedDate))
            {
              var field2 = GetField(export.JailIncarceration, "incarcerated");

              field2.Error = true;

              ExitState = "ACO_NI0000_ENTER_Y_OR_N";
            }

            break;
          case 'Y':
            if (Equal(export.JailIncarceration.VerifiedDate, local.Blank.Date))
            {
              var field2 = GetField(export.JailIncarceration, "verifiedDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            break;
          case 'N':
            if (Equal(export.JailIncarceration.VerifiedDate, local.Blank.Date))
            {
              export.JailIncarceration.VerifiedDate = local.Current.Date;
            }

            break;
          default:
            var field1 = GetField(export.JailIncarceration, "incarcerated");

            field1.Error = true;

            ExitState = "INVALID_VALUE";

            break;
        }

        // ----------------------------------------------------------------
        // Set the incarceration type according to the flag selection.  If
        // the prison flag is selected type is "P", jail type is "J",
        // parole type is "R", and probation type is "R".
        // ----------------------------------------------------------------
        if (AsChar(import.JailCommon.SelectChar) == 'Y')
        {
          export.JailIncarceration.Type1 = "J";
        }
        else if (AsChar(import.Prison.SelectChar) == 'Y')
        {
          export.JailIncarceration.Type1 = "P";
        }

        if (IsEmpty(import.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "BYPASS";
        }
        else
        {
          MoveIncarceration2(export.JailIncarceration, local.StartIncarceration);
            
          MoveIncarcerationAddress1(export.JailIncarcerationAddress,
            local.StartIncarcerationAddress);

          if (Lt(local.Blank.Date, export.JailIncarceration.VerifiedDate) && !
            Equal(export.JailIncarceration.VerifiedDate,
            export.HiddenJailIncarceration.VerifiedDate))
          {
            if (AsChar(export.JailIncarceration.Incarcerated) == 'Y')
            {
              local.RaiseEventFlag.Text1 = "V";
            }
          }

          if (Lt(local.Blank.Date, export.JailIncarceration.EndDate) && !
            Equal(export.JailIncarceration.EndDate,
            export.HiddenJailIncarceration.EndDate))
          {
            if (AsChar(export.JailIncarceration.Incarcerated) == 'Y')
            {
              local.RaiseEventFlag.Text1 = "E";
            }
          }

          global.Command = "UPDATE";
        }

        break;
      case "CHANPROB":
        if (!Equal(export.CsePerson.Number, export.H.Number) || IsEmpty
          (export.H.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "OE0013_DISP_REC_BEFORE_UPD";

          return;
        }

        // -------------------------------------------------------------
        // Check for any modified data. If data is not changed, no need to 
        // update.
        //                                             
        // ---- Vithal (12/02/2000)
        // -------------------------------------------------------------
        if (AsChar(export.Parole.SelectChar) == AsChar
          (export.HiddenParole.SelectChar) && AsChar
          (export.ProbationCommon.SelectChar) == AsChar
          (export.HiddenProbationCommon.SelectChar) && Equal
          (export.ProbationIncarceration.VerifiedDate,
          export.HiddenProbationIncarceration.VerifiedDate) && Equal
          (export.ProbationIncarceration.EndDate,
          export.HiddenProbationIncarceration.EndDate) && Equal
          (export.ProbationIncarceration.ConditionsForRelease,
          export.HiddenProbationIncarceration.ConditionsForRelease) && Equal
          (export.ProbationIncarceration.ParoleOfficerLastName,
          export.HiddenProbationIncarceration.ParoleOfficerLastName) && Equal
          (export.ProbationIncarceration.ParoleOfficerFirstName,
          export.HiddenProbationIncarceration.ParoleOfficerFirstName) && AsChar
          (export.ProbationIncarceration.ParoleOfficerMiddleInitial) == AsChar
          (export.HiddenProbationIncarceration.ParoleOfficerMiddleInitial) && Equal
          (export.ProbationIncarceration.ParoleOfficersTitle,
          export.HiddenProbationIncarceration.ParoleOfficersTitle) && export
          .ProbationIncarceration.PhoneAreaCode.GetValueOrDefault() == export
          .HiddenProbationIncarceration.PhoneAreaCode.GetValueOrDefault() && export
          .ProbationIncarceration.Phone.GetValueOrDefault() == export
          .HiddenProbationIncarceration.Phone.GetValueOrDefault() && Equal
          (export.ProbationIncarceration.PhoneExt,
          export.HiddenProbationIncarceration.PhoneExt) && AsChar
          (export.ProbationIncarceration.Incarcerated) == AsChar
          (export.HiddenProbationIncarceration.Incarcerated) && Equal
          (export.ProbationIncarcerationAddress.Street1,
          export.HiddenProbationIncarcerationAddress.Street1) && Equal
          (export.ProbationIncarcerationAddress.Street2,
          export.ProbationIncarcerationAddress.Street2) && Equal
          (export.ProbationIncarcerationAddress.City,
          export.HiddenProbationIncarcerationAddress.City) && Equal
          (export.ProbationIncarcerationAddress.State,
          export.HiddenProbationIncarcerationAddress.State) && Equal
          (export.ProbationIncarcerationAddress.ZipCode5,
          export.HiddenProbationIncarcerationAddress.ZipCode5) && Equal
          (export.ProbationIncarcerationAddress.ZipCode4,
          export.HiddenProbationIncarcerationAddress.ZipCode4))
        {
          local.ParoleInfoModified.Flag = "N";
        }
        else
        {
          local.ParoleInfoModified.Flag = "Y";
        }

        if (AsChar(local.ParoleInfoModified.Flag) == 'N')
        {
          ExitState = "FN0000_NO_CHANGE_TO_UPDATE";
          global.Command = "BYPASS";

          break;
        }

        // -------------------------------------------------------------
        // If the parole/probation records are selected, the following
        // validation must take place.
        // -------------------------------------------------------------
        if (!IsEmpty(export.ProbationIncarcerationAddress.State))
        {
          local.CodeValue.Cdvalue =
            export.ProbationIncarcerationAddress.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.ReturnCode.Count != 0)
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        // -------------------------------------------------------------
        // If the Address is entered, then the Address details should
        // be complete.
        // -------------------------------------------------------------
        if (!IsEmpty(import.ProbationIncarcerationAddress.Street1) || !
          IsEmpty(import.ProbationIncarcerationAddress.Street2) || !
          IsEmpty(import.ProbationIncarcerationAddress.City) || !
          IsEmpty(import.ProbationIncarcerationAddress.State) || !
          IsEmpty(import.ProbationIncarcerationAddress.ZipCode5))
        {
          if (IsEmpty(import.ProbationIncarcerationAddress.ZipCode5))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            do
            {
              ++local.CheckZip.Count;
              local.CheckZip.Flag =
                Substring(import.ProbationIncarcerationAddress.ZipCode5,
                local.CheckZip.Count, 1);

              if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                (local.CheckZip.Flag) > '9')
              {
                var field1 =
                  GetField(export.ProbationIncarcerationAddress, "zipCode5");

                field1.Error = true;

                ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";

                break;
              }
            }
            while(local.CheckZip.Count < 5);
          }

          if (IsEmpty(import.ProbationIncarcerationAddress.State))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "state");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.ProbationIncarcerationAddress.City))
          {
            var field1 = GetField(export.ProbationIncarcerationAddress, "city");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.ProbationIncarcerationAddress.Street1))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "street1");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(import.ProbationIncarcerationAddress.ZipCode4))
        {
        }
        else
        {
          if (IsEmpty(import.ProbationIncarcerationAddress.ZipCode5))
          {
            var field1 =
              GetField(export.ProbationIncarcerationAddress, "zipCode5");

            field1.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          local.CheckZip.Count = 0;
          local.CheckZip.Flag = "";

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.ProbationIncarcerationAddress.ZipCode4,
              local.CheckZip.Count, 1);

            if (AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              var field1 =
                GetField(export.ProbationIncarcerationAddress, "zipCode4");

              field1.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";

              break;
            }
          }
          while(local.CheckZip.Count < 4);
        }

        // ---------------------------------------------
        // Check for complete Phone Number and Area Code.
        // ---------------------------------------------
        if (export.ProbationIncarceration.PhoneAreaCode.GetValueOrDefault() != 0
          || export.ProbationIncarceration.Phone.GetValueOrDefault() != 0 || !
          IsEmpty(export.ProbationIncarceration.PhoneExt))
        {
          if (export.ProbationIncarceration.PhoneAreaCode.GetValueOrDefault() ==
            0)
          {
            var field1 =
              GetField(export.ProbationIncarceration, "phoneAreaCode");

            field1.Error = true;

            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }

          if (export.ProbationIncarceration.Phone.GetValueOrDefault() == 0)
          {
            var field1 = GetField(export.ProbationIncarceration, "phone");

            field1.Error = true;

            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        // -------------------------------------------------------------
        // The parole/probation officer's first and last name are
        // required in order to create an incarceration record.
        // -------------------------------------------------------------
        if (IsEmpty(import.ProbationIncarceration.ParoleOfficerLastName) && IsEmpty
          (import.ProbationIncarceration.ParoleOfficerFirstName))
        {
          var field1 =
            GetField(export.ProbationIncarceration, "paroleOfficerLastName");

          field1.Error = true;

          var field2 =
            GetField(export.ProbationIncarceration, "paroleOfficerFirstName");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsEmpty(import.ProbationIncarceration.ParoleOfficerFirstName) || !
          IsEmpty(import.ProbationIncarceration.ParoleOfficerLastName) || !
          IsEmpty(import.ProbationIncarceration.ParoleOfficerMiddleInitial))
        {
          if (IsEmpty(import.ProbationIncarceration.ParoleOfficerFirstName))
          {
            var field1 =
              GetField(export.ProbationIncarceration, "paroleOfficerFirstName");
              

            field1.Error = true;

            ExitState = "CONTACT_OFFICER_FIRST_NAME_REQD";
          }

          if (IsEmpty(import.ProbationIncarceration.ParoleOfficerLastName))
          {
            var field1 =
              GetField(export.ProbationIncarceration, "paroleOfficerLastName");

            field1.Error = true;

            ExitState = "OE0188_CONTACT_OFFICER_LASTNAME";
          }
        }

        // -------------------------------------------------------------
        // The date the CSE Person is released from probation
        // cannot be greater than the current date.
        // ------------------------------------------------------------
        // Per SME changed the exitstate to display appropriate message.
        //                                                
        // Vithal ( 12/04/2000)
        // -------------------------------------------------------------
        if (Lt(Now().Date, export.ProbationIncarceration.EndDate) && !
          Equal(export.ProbationIncarceration.EndDate,
          local.MaxDate.ExpirationDate))
        {
          var field1 = GetField(export.ProbationIncarceration, "endDate");

          field1.Error = true;

          ExitState = "OE0000_PROBATION_RELEASE_DT_ERR";
        }

        // -------------------------------------------------------------
        // The date the parole/probation record is verified cannot be
        // greater than the current date.
        // -------------------------------------------------------------
        if (Lt(Now().Date, import.ProbationIncarceration.VerifiedDate))
        {
          var field1 = GetField(export.ProbationIncarceration, "verifiedDate");

          field1.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // -------------------------------------------------------------
        // There cannot be a "Y" in both the prison and jail indicator
        // fields.  The CSE Person is either in jail or in prison.
        // -------------------------------------------------------------
        if (AsChar(import.Parole.SelectChar) == 'Y' && (
          AsChar(import.ProbationCommon.SelectChar) == 'N' || IsEmpty
          (import.ProbationCommon.SelectChar)) || (
            AsChar(import.Parole.SelectChar) == 'N' || IsEmpty
          (import.Parole.SelectChar)) && AsChar
          (import.ProbationCommon.SelectChar) == 'Y')
        {
        }
        else
        {
          var field1 = GetField(export.Parole, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ProbationCommon, "selectChar");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";
        }

        // --------------------------------------------------------------------
        // Per WR# 273,  the following edits are needed.
        //                                                      
        // Vithal.
        // --------------------------------------------------------------------
        switch(AsChar(export.ProbationIncarceration.Incarcerated))
        {
          case ' ':
            if (Lt(local.Blank.Date, export.ProbationIncarceration.VerifiedDate))
              
            {
              var field2 =
                GetField(export.ProbationIncarceration, "incarcerated");

              field2.Error = true;

              ExitState = "ACO_NI0000_ENTER_Y_OR_N";
            }

            break;
          case 'Y':
            if (Equal(export.ProbationIncarceration.VerifiedDate,
              local.Blank.Date))
            {
              var field2 =
                GetField(export.ProbationIncarceration, "verifiedDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            break;
          case 'N':
            if (Equal(export.ProbationIncarceration.VerifiedDate,
              local.Blank.Date))
            {
              export.ProbationIncarceration.VerifiedDate = local.Current.Date;
            }

            break;
          default:
            var field1 =
              GetField(export.ProbationIncarceration, "incarcerated");

            field1.Error = true;

            ExitState = "INVALID_VALUE";

            break;
        }

        // ----------------------------------------------------------------
        // Set the incarceration type according to the flag selection.  If
        // the prison flag is selected type is "P", jail type is "J",
        // parole type is "R", and probation type is "T"
        // ----------------------------------------------------------------
        if (AsChar(import.Parole.SelectChar) == 'Y')
        {
          export.ProbationIncarceration.Type1 = "R";
        }
        else if (AsChar(import.ProbationCommon.SelectChar) == 'Y')
        {
          export.ProbationIncarceration.Type1 = "T";
        }

        if (IsEmpty(import.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "BYPASS";
        }
        else
        {
          MoveIncarceration3(export.ProbationIncarceration,
            local.StartIncarceration);
          MoveIncarcerationAddress2(export.ProbationIncarcerationAddress,
            local.StartIncarcerationAddress);
          global.Command = "UPDATE";
        }

        break;
      default:
        break;
    }

    // **** begin group C ****
    // to validate action level security
    // mjr
    // -------------------------------------------
    // 12/30/1998
    // Changed security to check CRUD actions only
    // --------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "PRINT"))
    {
      // --------------------------------------------------------
      // 04/04/00 W.Campbell - Disabled existing call to
      // Security Cab and added a new call with view
      // matching changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
      UseScCabTestSecurity();

      // --------------------------------------------------------
      // 04/04/00 W.Campbell - End of change to
      // disable existing call to Security Cab and
      // added a new call with view matching
      // changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else if (IsExitState("ACO_NE0000_RETURN") || IsExitState
      ("FN0000_CLEAR_BEFORE_ADD_2") || IsExitState
      ("FN0000_NO_CHANGE_TO_UPDATE"))
    {
    }
    else
    {
      global.Command = "BYPASS";
    }

    // **** end   group C ****
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      // -----------------------------------------------------------------------
      // The 'Release date'  must be greater than 'Incarceration date'.
      //                                                  
      // ----- Vithal (12/01/2000)
      // -----------------------------------------------------------------------
      if (Lt(local.Blank.Date, export.JailIncarceration.StartDate) && Lt
        (local.Blank.Date, export.JailIncarceration.EndDate) && Lt
        (export.JailIncarceration.EndDate, export.JailIncarceration.StartDate))
      {
        ExitState = "OE0000_START_END_DATE_ERROR";

        var field1 = GetField(export.JailIncarceration, "endDate");

        field1.Error = true;

        var field2 = GetField(export.JailIncarceration, "startDate");

        field2.Error = true;

        global.Command = "BYPASS";
      }

      // -----------------------------------------------------------------------
      // The Probation  'release date'  must be less than  JAIL 'incarceration 
      // date' or greater than 'Release date'.
      //                                                  
      // ----- Vithal (12/04/2000)
      // -----------------------------------------------------------------------
      if (Lt(local.Blank.Date, export.JailIncarceration.StartDate) && Lt
        (local.Blank.Date, export.JailIncarceration.EndDate) && Lt
        (local.Blank.Date, export.ProbationIncarceration.EndDate))
      {
        if (Lt(export.JailIncarceration.StartDate,
          export.ProbationIncarceration.EndDate) && Lt
          (export.ProbationIncarceration.EndDate,
          export.JailIncarceration.EndDate))
        {
          var field1 = GetField(export.JailIncarceration, "startDate");

          field1.Error = true;

          var field2 = GetField(export.JailIncarceration, "endDate");

          field2.Error = true;

          var field3 = GetField(export.ProbationIncarceration, "endDate");

          field3.Error = true;

          ExitState = "OE0000_PROBATION_RELEASE_DATE_ER";
          global.Command = "BYPASS";
        }
      }

      if (AsChar(export.JailIncarceration.Incarcerated) == 'N' && Lt
        (local.Blank.Date, export.JailIncarceration.EndDate))
      {
        var field = GetField(export.JailIncarceration, "endDate");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_JAIL_END_DATE";
        global.Command = "BYPASS";
      }

      if (AsChar(export.ProbationIncarceration.Incarcerated) == 'N' && Lt
        (local.Blank.Date, export.ProbationIncarceration.EndDate))
      {
        var field = GetField(export.ProbationIncarceration, "endDate");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROB_END_DATE";
        global.Command = "BYPASS";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ADDJAIL":
        // -------------------
        // Continue processing
        // -------------------
        break;
      case "RETURN":
        // -------------------
        // Continue processing
        // -------------------
        break;
      case "PRINT":
        if (!Equal(export.H.Number, export.CsePerson.Number) || IsEmpty
          (export.H.Number) || !
          Equal(export.HiddenJailIncarceration.ConditionsForRelease,
          export.JailIncarceration.ConditionsForRelease) || !
          Equal(export.HiddenJailIncarceration.EndDate,
          export.JailIncarceration.EndDate) || export
          .HiddenJailIncarceration.Identifier != export
          .JailIncarceration.Identifier || !
          Equal(export.HiddenJailIncarceration.InmateNumber,
          export.JailIncarceration.InmateNumber) || !
          Equal(export.HiddenJailIncarceration.InstitutionName,
          export.JailIncarceration.InstitutionName) || !
          Equal(export.HiddenJailIncarceration.ParoleEligibilityDate,
          export.JailIncarceration.ParoleEligibilityDate) || export
          .HiddenJailIncarceration.Phone.GetValueOrDefault() != export
          .JailIncarceration.Phone.GetValueOrDefault() || !
          Equal(export.HiddenJailIncarceration.PhoneExt,
          export.JailIncarceration.PhoneExt) || !
          Equal(export.HiddenJailIncarceration.StartDate,
          export.JailIncarceration.StartDate) || !
          Equal(export.HiddenJailIncarceration.VerifiedDate,
          export.JailIncarceration.VerifiedDate) || AsChar
          (export.HiddenJailIncarceration.WorkReleaseInd) != AsChar
          (export.JailIncarceration.WorkReleaseInd) || !
          Equal(export.HiddenProbationIncarceration.ConditionsForRelease,
          export.ProbationIncarceration.ConditionsForRelease) || !
          Equal(export.HiddenProbationIncarceration.EndDate,
          export.ProbationIncarceration.EndDate) || !
          Equal(export.HiddenProbationIncarceration.ParoleOfficerFirstName,
          export.ProbationIncarceration.ParoleOfficerFirstName) || !
          Equal(export.HiddenProbationIncarceration.ParoleOfficerLastName,
          export.ProbationIncarceration.ParoleOfficerLastName) || AsChar
          (export.HiddenProbationIncarceration.ParoleOfficerMiddleInitial) != AsChar
          (export.ProbationIncarceration.ParoleOfficerMiddleInitial) || !
          Equal(export.HiddenProbationIncarceration.ParoleOfficersTitle,
          export.ProbationIncarceration.ParoleOfficersTitle) || export
          .HiddenProbationIncarceration.Phone.GetValueOrDefault() != export
          .ProbationIncarceration.Phone.GetValueOrDefault() || export
          .HiddenProbationIncarceration.PhoneAreaCode.GetValueOrDefault() != export
          .ProbationIncarceration.PhoneAreaCode.GetValueOrDefault() || !
          Equal(export.HiddenProbationIncarceration.PhoneExt,
          export.ProbationIncarceration.PhoneExt) || !
          Equal(export.HiddenProbationIncarceration.StartDate,
          export.ProbationIncarceration.StartDate) || !
          Equal(export.HiddenProbationIncarceration.VerifiedDate,
          export.ProbationIncarceration.VerifiedDate))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";

          break;
        }
        else
        {
          export.DocmProtectFilter.Flag = "Y";
          export.Print.Type1 = "JAIL";
          ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";
        }

        return;
      case "RETDOCM":
        if (IsEmpty(import.Print.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          break;
        }

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before print process is invoked
        // -------------------------------------------------------
        export.Hidden.Assign(local.Null1);
        export.Standard.NextTransaction = "DKEY";
        export.Hidden.MiscText2 = TrimEnd(local.SpDocLiteral.IdDocument) + import
          .Print.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.Hidden.MiscText1 = TrimEnd(local.SpDocLiteral.IdPrNumber) + export
          .CsePerson.Number;

        if (export.ProbationIncarceration.Identifier > 0)
        {
          local.Incarceration.Identifier =
            export.ProbationIncarceration.Identifier;
        }
        else
        {
          local.Incarceration.Identifier = export.JailIncarceration.Identifier;
        }

        local.Print.Text50 = TrimEnd(local.SpDocLiteral.IdJail) + NumberToString
          (local.Incarceration.Identifier, 14, 2);
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/30/1998
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
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.CsePerson.Number =
          Substring(export.Hidden.MiscText1, local.Position.Count + 7, 10);
        global.Command = "DISPLAY";

        break;
      case "HELP":
        // ---------------------------------------------
        // All logic pertaining to using the IEF help
        // facility should be placed here.
        // At this time, this is not available.
        // ---------------------------------------------
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "EXIT":
        // -------------------------------------------------------------
        // Allows the user to flow back to the previous screen.
        // -------------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "CREATE":
        local.StartIncarcerationAddress.EffectiveDate =
          export.JailIncarceration.VerifiedDate;

        // CQ55280  added view local_start1 incarceration and export_srart1 
        // incarceration
        UseOeJailCreateIncarceratDetls();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("ALREADY_INCARCERATED"))
          {
            if (AsChar(local.Common.Flag) == 'P')
            {
              var field = GetField(export.ProbationIncarceration, "endDate");

              field.Error = true;

              if (export.ProbationIncarceration.Identifier == 0)
              {
                MoveIncarceration3(local.StartIncarceration,
                  export.ProbationIncarceration);
                MoveIncarceration3(local.StartIncarceration,
                  export.HiddenProbationIncarceration);
                export.ProbationIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);
                export.HiddenProbationIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);

                if (Equal(export.ProbationIncarceration.EndDate,
                  local.MaxDate.ExpirationDate))
                {
                  export.ProbationIncarceration.EndDate = local.Blank.Date;
                  export.HiddenProbationIncarceration.EndDate =
                    local.Blank.Date;
                }

                if (AsChar(export.ProbationIncarceration.Type1) == 'T')
                {
                  export.Parole.SelectChar = "N";
                  export.HiddenParole.SelectChar = "N";
                  export.ProbationCommon.SelectChar = "Y";
                  export.HiddenProbationCommon.SelectChar = "Y";
                }
                else if (AsChar(export.ProbationIncarceration.Type1) == 'R')
                {
                  export.Parole.SelectChar = "Y";
                  export.HiddenParole.SelectChar = "Y";
                  export.ProbationCommon.SelectChar = "N";
                  export.HiddenProbationCommon.SelectChar = "N";
                }
              }
            }
            else
            {
              var field = GetField(export.JailIncarceration, "endDate");

              field.Error = true;

              if (export.JailIncarceration.Identifier == 0)
              {
                MoveIncarceration2(local.StartIncarceration,
                  export.JailIncarceration);
                MoveIncarceration2(local.StartIncarceration,
                  export.HiddenJailIncarceration);
                export.JailIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);
                export.HiddenJailIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);

                if (!IsEmpty(export.JailIncarceration.Incarcerated))
                {
                  var field1 =
                    GetField(export.JailIncarceration, "incarcerated");

                  field1.Color = "cyan";
                  field1.Protected = true;
                }

                if (Equal(export.JailIncarceration.EndDate,
                  local.MaxDate.ExpirationDate))
                {
                  export.JailIncarceration.EndDate = local.Blank.Date;
                  export.HiddenJailIncarceration.EndDate = local.Blank.Date;
                }

                if (AsChar(export.JailIncarceration.Type1) == 'J')
                {
                  export.JailCommon.SelectChar = "Y";
                  export.HiddenJailCommon.SelectChar = "Y";
                  export.Prison.SelectChar = "N";
                  export.HiddenPrison.SelectChar = "N";
                }
                else if (AsChar(export.JailIncarceration.Type1) == 'P')
                {
                  export.JailCommon.SelectChar = "N";
                  export.HiddenJailCommon.SelectChar = "N";
                  export.Prison.SelectChar = "Y";
                  export.HiddenPrison.SelectChar = "Y";
                }
              }
            }
          }
          else
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated) && AsChar
            (export.HiddenProbationIncarceration.Incarcerated) == AsChar
            (export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.JailIncarceration.Incarcerated) && AsChar
            (export.HiddenJailIncarceration.Incarcerated) == AsChar
            (export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          break;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!Equal(export.JailIncarceration.VerifiedDate,
            export.HiddenJailIncarceration.VerifiedDate) && Lt
            (local.Blank.Date, export.JailIncarceration.VerifiedDate))
          {
            // ------------------------------------------------------------------------------------
            // Per WR# 000261,  if a person is incarcerated (INCARCERATED = Y) 
            // and  if a verified date is entered on the JAIL screen, then read
            // all previous verified mailing and/or residential addresses from '
            // CSE_Person_Address' table for the person# on JAIL screen and
            // update them with an end_date of 'current_date'  and an end_code
            // of 'IC'. Also raise an event with reason code ADDREXPDAP'  if
            // the person is AP on the case or ADDREXPDAR' if the person is AR
            // on the case. We need to add the new address record to 
            // CSE_Person_Address' table with an address source of PR' and
            // address type to M'. The attributes will be set (mapped)
            // according to business rules. Also raise an event with reason code
            // ADDRVRFDAP'  if the person is AP on the case or ADDRVRFDAR' if
            // the person is AR on the case. On ADDR screen, protect the Send
            // Dt' field and give the worker an error message: Not able to send
            // PM letter on a Jail/Prison address.
            // ---------------------------------------------------------------------------------------
            if (AsChar(export.JailIncarceration.Incarcerated) == 'N')
            {
              // ------------------------------------------------------------------------------------
              // Per WR# 000261, Not incarcerated . So no need to flow the 
              // address to ADDR. Just escape.
              // ---------------------------------------------------------------------------------------
              goto Test1;
            }

            // CQ55280 Added the local_start1 incarceration
            if (AsChar(local.FlowAddress.Flag) == 'Y')
            {
              local.FlowCommand.Command = "CREATE";
              UseOeFlowAddressFrmJailToAddr1();
            }
          }
          else if (Equal(export.JailIncarceration.VerifiedDate, local.Blank.Date))
            
          {
            // ------------------------------------------------------------------------------------
            // Per WR# 000261, If all the fields, except Verified date',  are 
            // entered, then no Jail/Prison record will be populated on the ADDR
            // screen.
            // ---------------------------------------------------------------------------------------
          }
        }

Test1:

        // ---------------------------------------------
        // Raju : 12:15 hrs CST : set statement added
        // ---------------------------------------------
        export.LastReadHidden.StartDate = null;

        if (AsChar(local.StartIncarceration.Type1) == 'P' || AsChar
          (local.StartIncarceration.Type1) == 'J')
        {
          MoveIncarceration2(local.StartIncarceration, export.JailIncarceration);
            
          MoveIncarceration2(local.StartIncarceration,
            export.HiddenJailIncarceration);
          export.JailIncarcerationAddress.
            Assign(local.StartIncarcerationAddress);
          export.H.Number = export.CsePerson.Number;
          ExitState = "PRISON_JAIL_ADDED";

          if (!IsEmpty(export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated) && AsChar
            (export.HiddenProbationIncarceration.Incarcerated) == AsChar
            (export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
        else
        {
          MoveIncarceration3(local.StartIncarceration,
            export.ProbationIncarceration);
          MoveIncarceration3(local.StartIncarceration,
            export.HiddenProbationIncarceration);
          export.ProbationIncarcerationAddress.Assign(
            local.StartIncarcerationAddress);
          export.H.Number = export.CsePerson.Number;
          ExitState = "PAROLE_PROBATION_ADDED";

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.JailIncarceration.Incarcerated) && AsChar
            (export.HiddenJailIncarceration.Incarcerated) == AsChar
            (export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        break;
      case "DISPLAY":
        export.LastReadHidden.StartDate = null;
        export.Parole.SelectChar = "";
        export.JailCommon.SelectChar = "";
        export.Prison.SelectChar = "";
        export.ProbationCommon.SelectChar = "";
        export.HiddenParole.SelectChar = "";
        export.HiddenJailCommon.SelectChar = "";
        export.HiddenPrison.SelectChar = "";
        export.HiddenProbationCommon.SelectChar = "";
        UseOeCabCheckCaseMember3();

        switch(AsChar(local.Work.Flag))
        {
          case 'C':
            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            ExitState = "CASE_NF";

            break;
          case 'P':
            var field2 = GetField(export.CsePerson, "number");

            field2.Error = true;

            ExitState = "CSE_PERSON_NF";

            break;
          case 'R':
            var field3 = GetField(export.CsePerson, "number");

            field3.Error = true;

            var field4 = GetField(export.Case1, "number");

            field4.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";

            break;
          default:
            break;
        }

        if (!IsEmpty(local.Work.Flag))
        {
          MoveIncarceration3(local.InitializedIncarceration,
            export.ProbationIncarceration);
          MoveIncarceration2(local.InitializedIncarceration,
            export.JailIncarceration);
          MoveIncarceration2(local.InitializedIncarceration,
            export.HiddenJailIncarceration);
          MoveIncarceration3(local.InitializedIncarceration,
            export.HiddenProbationIncarceration);
          export.JailIncarcerationAddress.Assign(
            local.InitializedIncarcerationAddress);
          export.ProbationIncarcerationAddress.Assign(
            local.InitializedIncarcerationAddress);
          export.JailCommon.SelectChar = "";
          export.Parole.SelectChar = "";
          export.Prison.SelectChar = "";
          export.ProbationCommon.SelectChar = "";
          export.HiddenJailCommon.SelectChar = "";
          export.HiddenParole.SelectChar = "";
          export.HiddenPrison.SelectChar = "";
          export.HiddenProbationCommon.SelectChar = "";

          break;
        }

        // -------------------------------------------------------------
        // Insert the USE statement here that calls the READ action
        // block.
        // -------------------------------------------------------------
        UseOeJailReadIncarceratDetls();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          export.H.Number = export.CsePerson.Number;

          // ---------------------------------------------
          // Raju : 12:15 hrs CST : set statement added
          // ---------------------------------------------
          export.LastReadHidden.StartDate = export.JailIncarceration.StartDate;
          export.HiddenJailIncarceration.Assign(export.JailIncarceration);
          export.HiddenJailIncarcerationAddress.Assign(
            export.JailIncarcerationAddress);
          export.HiddenProbationIncarceration.Assign(
            export.ProbationIncarceration);
          export.HiddenProbationIncarcerationAddress.Assign(
            export.ProbationIncarcerationAddress);

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated) && AsChar
            (export.HiddenProbationIncarceration.Incarcerated) == AsChar
            (export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.JailIncarceration.Incarcerated) && AsChar
            (export.HiddenJailIncarceration.Incarcerated) == AsChar
            (export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
        else
        {
          // ---------------------------------------------
          // Raju : 12:15 hrs CST : set statement added
          // ---------------------------------------------
          export.LastReadHidden.StartDate = null;
          export.H.Number = "";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        global.Command = "";

        break;
      case "UPDATE":
        local.StartIncarcerationAddress.EffectiveDate =
          export.JailIncarceration.VerifiedDate;
        UseOeJailUpdateIncarceratDetls();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("ALREADY_INCARCERATED"))
          {
            if (AsChar(local.Common.Flag) == 'P')
            {
              var field = GetField(export.ProbationIncarceration, "endDate");

              field.Error = true;

              if (export.ProbationIncarceration.Identifier == 0)
              {
                MoveIncarceration3(local.StartIncarceration,
                  export.ProbationIncarceration);
                MoveIncarceration3(local.StartIncarceration,
                  export.HiddenProbationIncarceration);
                export.ProbationIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);
                export.HiddenProbationIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);

                if (Equal(export.ProbationIncarceration.EndDate,
                  local.MaxDate.ExpirationDate))
                {
                  export.ProbationIncarceration.EndDate = local.Blank.Date;
                  export.HiddenProbationIncarceration.EndDate =
                    local.Blank.Date;
                }

                if (AsChar(export.ProbationIncarceration.Type1) == 'T')
                {
                  export.Parole.SelectChar = "N";
                  export.HiddenParole.SelectChar = "N";
                  export.ProbationCommon.SelectChar = "Y";
                  export.HiddenProbationCommon.SelectChar = "Y";
                }
                else if (AsChar(export.ProbationIncarceration.Type1) == 'R')
                {
                  export.Parole.SelectChar = "Y";
                  export.HiddenParole.SelectChar = "Y";
                  export.ProbationCommon.SelectChar = "N";
                  export.HiddenProbationCommon.SelectChar = "N";
                }
              }
            }
            else
            {
              var field = GetField(export.JailIncarceration, "endDate");

              field.Error = true;

              if (export.JailIncarceration.Identifier == 0)
              {
                MoveIncarceration2(local.StartIncarceration,
                  export.JailIncarceration);
                MoveIncarceration2(local.StartIncarceration,
                  export.HiddenJailIncarceration);
                export.JailIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);
                export.HiddenJailIncarcerationAddress.Assign(
                  local.StartIncarcerationAddress);

                if (Equal(export.JailIncarceration.EndDate,
                  local.MaxDate.ExpirationDate))
                {
                  export.JailIncarceration.EndDate = local.Blank.Date;
                  export.HiddenJailIncarceration.EndDate = local.Blank.Date;
                }

                if (AsChar(export.JailIncarceration.Type1) == 'J')
                {
                  export.JailCommon.SelectChar = "Y";
                  export.HiddenJailCommon.SelectChar = "Y";
                  export.Prison.SelectChar = "N";
                  export.HiddenPrison.SelectChar = "N";
                }
                else if (AsChar(export.JailIncarceration.Type1) == 'P')
                {
                  export.JailCommon.SelectChar = "N";
                  export.HiddenJailCommon.SelectChar = "N";
                  export.Prison.SelectChar = "Y";
                  export.HiddenPrison.SelectChar = "Y";
                }
              }
            }
          }
          else
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated) && AsChar
            (export.HiddenProbationIncarceration.Incarcerated) == AsChar
            (export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.JailIncarceration.Incarcerated) && AsChar
            (export.HiddenJailIncarceration.Incarcerated) == AsChar
            (export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          break;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!Equal(export.JailIncarceration.EndDate,
            export.HiddenJailIncarceration.EndDate) && Lt
            (local.Blank.Date, export.JailIncarceration.EndDate) && Lt
            (export.JailIncarceration.EndDate, local.MaxDate.ExpirationDate))
          {
            // ------------------------------------------------------------------------------------
            // Per WR# 000261 if the JAIL record is updated with a Release_Date(
            // End_Date), the corresponding CSE_Pesron_Addr on ADDR screen must
            // be  End_dated (= Release_date ) and End_Coded ( = RL).
            // ---------------------------------------------------------------------------------------
            local.FlowCommand.Command = "UPDATE";
            UseOeFlowAddressFrmJailToAddr2();
          }
          else if (Lt(local.Blank.Date, export.JailIncarceration.VerifiedDate) &&
            !
            Equal(export.JailIncarceration.VerifiedDate,
            export.HiddenJailIncarceration.VerifiedDate) || !
            Equal(export.JailIncarceration.InstitutionName,
            export.HiddenJailIncarceration.InstitutionName))
          {
            // ------------------------------------------------------------------------------------
            // Per WR# 000261,  if a person is incarcerated (INCARCERATED = Y) 
            // and  if verified_date OR institution_name are updated, end_date
            // the previous addresses on ADDR with an end_code of IC. and create
            // the the new address on the ADDR screen.
            // ---------------------------------------------------------------------------------------
            if (AsChar(export.JailIncarceration.Incarcerated) == 'N')
            {
              // ------------------------------------------------------------------------------------
              // Per WR# 000261,  Not incarcerated . So no need to flow the 
              // address to ADDR. Just escape.
              // ---------------------------------------------------------------------------------------
              goto Test2;
            }

            local.FlowCommand.Command = "CREATE";
            UseOeFlowAddressFrmJailToAddr2();
          }
        }

Test2:

        if (AsChar(local.StartIncarceration.Type1) == 'P' || AsChar
          (local.StartIncarceration.Type1) == 'J')
        {
          MoveIncarceration2(local.StartIncarceration, export.JailIncarceration);
            
          MoveIncarceration2(local.StartIncarceration,
            export.HiddenJailIncarceration);
          export.JailIncarcerationAddress.
            Assign(local.StartIncarcerationAddress);
          export.H.Number = export.CsePerson.Number;
          ExitState = "PRISON_JAIL_UPDATED";

          if (!IsEmpty(export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated) && AsChar
            (export.HiddenProbationIncarceration.Incarcerated) == AsChar
            (export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
        else
        {
          MoveIncarceration3(local.StartIncarceration,
            export.ProbationIncarceration);
          MoveIncarceration3(local.StartIncarceration,
            export.HiddenProbationIncarceration);
          export.ProbationIncarcerationAddress.Assign(
            local.StartIncarcerationAddress);
          export.H.Number = export.CsePerson.Number;
          ExitState = "PAROLE_PROBATION_UPDATED";

          if (!IsEmpty(export.ProbationIncarceration.Incarcerated))
          {
            var field = GetField(export.ProbationIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.JailIncarceration.Incarcerated) && AsChar
            (export.HiddenJailIncarceration.Incarcerated) == AsChar
            (export.JailIncarceration.Incarcerated))
          {
            var field = GetField(export.JailIncarceration, "incarcerated");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        break;
      case "BYPASS":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // mjr
    // -------------------------------------------
    // 12/30/1998
    // Pulled Display of of main case of command for after return from Print.
    // --------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      export.Parole.SelectChar = "";
      export.JailCommon.SelectChar = "";
      export.Prison.SelectChar = "";
      export.ProbationCommon.SelectChar = "";
      export.HiddenJailCommon.SelectChar = "";
      export.HiddenParole.SelectChar = "";
      export.HiddenPrison.SelectChar = "";
      export.HiddenProbationCommon.SelectChar = "";
      UseOeCabCheckCaseMember4();

      switch(AsChar(local.Work.Flag))
      {
        case 'C':
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          ExitState = "CASE_NF";

          break;
        case 'P':
          var field2 = GetField(export.CsePerson, "number");

          field2.Error = true;

          ExitState = "CSE_PERSON_NF";

          break;
        case 'R':
          var field3 = GetField(export.CsePerson, "number");

          field3.Error = true;

          var field4 = GetField(export.Case1, "number");

          field4.Error = true;

          ExitState = "OE0000_CASE_MEMBER_NE";

          break;
        default:
          break;
      }

      if (!IsEmpty(local.Work.Flag))
      {
        goto Test3;
      }

      UseOeJailReadIncarceratDetls();

      if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
      {
        export.H.Number = export.CsePerson.Number;

        // ---------------------------------------------
        // Raju : 12:15 hrs CST : set statement added
        // ---------------------------------------------
        export.LastReadHidden.StartDate = export.JailIncarceration.StartDate;
        export.HiddenJailIncarceration.Assign(export.JailIncarceration);
        export.HiddenJailIncarcerationAddress.Assign(
          export.JailIncarcerationAddress);
        export.HiddenProbationIncarceration.
          Assign(export.ProbationIncarceration);
        export.HiddenProbationIncarcerationAddress.Assign(
          export.ProbationIncarcerationAddress);

        if (!IsEmpty(export.JailIncarceration.Incarcerated))
        {
          var field = GetField(export.JailIncarceration, "incarcerated");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (!IsEmpty(export.ProbationIncarceration.Incarcerated))
        {
          var field = GetField(export.ProbationIncarceration, "incarcerated");

          field.Color = "cyan";
          field.Protected = true;
        }

        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count > 0)
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.Print.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          export.Hidden.MiscText2 = local.Print.Text50;
        }
      }
      else
      {
        // ---------------------------------------------
        // Raju : 12:15 hrs CST : set statement added
        // ---------------------------------------------
        export.LastReadHidden.StartDate = null;
        export.H.Number = "";

        var field = GetField(export.CsePerson, "number");

        field.Error = true;
      }
    }

Test3:

    export.WorkNameCsePersonsWorkSet.FormattedName =
      export.WorkNameOeWorkGroup.FormattedNameText;

    if (export.JailIncarceration.Identifier != 0)
    {
      if (Equal(global.Command, "BYPASS") && !IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.HiddenJailIncarceration.Assign(export.JailIncarceration);
      }

      if (AsChar(export.Facl.Flag) == 'Y')
      {
        var field1 = GetField(export.JailIncarceration, "institutionName");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.JailIncarceration, "phoneAreaCode");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.JailIncarceration, "phone");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.JailIncarceration, "phoneExt");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.JailIncarcerationAddress, "street1");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.JailIncarcerationAddress, "street2");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.JailIncarcerationAddress, "state");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.JailIncarcerationAddress, "city");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.JailIncarcerationAddress, "zipCode5");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.JailIncarcerationAddress, "zipCode4");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.PromptState1, "selectChar");

        field11.Color = "cyan";
        field11.Protected = true;
      }

      if (AsChar(export.JailIncarceration.Type1) == 'J')
      {
        export.JailCommon.SelectChar = "Y";
        export.HiddenJailCommon.SelectChar = "Y";
        export.Prison.SelectChar = "N";
        export.HiddenPrison.SelectChar = "N";
      }
      else if (AsChar(export.JailIncarceration.Type1) == 'P')
      {
        export.JailCommon.SelectChar = "N";
        export.HiddenJailCommon.SelectChar = "N";
        export.Prison.SelectChar = "Y";
        export.HiddenPrison.SelectChar = "Y";
      }
    }

    if (export.ProbationIncarceration.Identifier != 0)
    {
      if (Equal(global.Command, "BYPASS") && !IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.HiddenProbationIncarceration.
          Assign(export.ProbationIncarceration);
      }

      if (AsChar(export.ProbationIncarceration.Type1) == 'T')
      {
        export.Parole.SelectChar = "N";
        export.HiddenParole.SelectChar = "N";
        export.ProbationCommon.SelectChar = "Y";
        export.HiddenProbationCommon.SelectChar = "Y";
      }
      else if (AsChar(export.ProbationIncarceration.Type1) == 'R')
      {
        export.Parole.SelectChar = "Y";
        export.HiddenParole.SelectChar = "Y";
        export.ProbationCommon.SelectChar = "N";
        export.HiddenProbationCommon.SelectChar = "N";
      }
    }

    // ----------------------------------------------------------------
    // Per WR# 261, SME added two new events with reason_code 'ARINCARC' (
    // AR_Incarcerated), 'ARINCARRELEASED' (AR_Released_From _Incarceration).
    // Now we need to raise above events when AR is incarcerated and/or
    // released. New CAB is added to process the events for both AP and AR.
    //                                                     
    // Vithal Madhira(03/08/2001)
    // ----------------------------------------------------------------
    if (IsExitState("PRISON_JAIL_ADDED") || IsExitState("PRISON_JAIL_UPDATED"))
    {
      local.Infrastructure.SituationNumber = 0;

      if (AsChar(local.RaiseEventFlag.Text1) == 'E')
      {
        // *******************************************************************
        //   The "E" indicates that the incarceration has ended.
        // *******************************************************************
        local.ActionEvent.Text1 = "E";
        local.Infrastructure.UserId = "JAIL";
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReferenceDate = export.JailIncarceration.EndDate;
        local.WorkDate.Date = local.Infrastructure.ReferenceDate;
        local.Infrastructure.BusinessObjectCd = "INC";
        local.Infrastructure.DenormNumeric12 =
          export.JailIncarceration.Identifier;

        // -------------------------
        // formation of detail line
        // -------------------------
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
        local.DetailText30.Text30 = "Released Incarceration Type:";

        switch(AsChar(export.JailIncarceration.Type1))
        {
          case 'J':
            local.DetailText10.Text10 = " Jail";

            break;
          case 'P':
            local.DetailText10.Text10 = " Prison";

            break;
          default:
            break;
        }

        local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + TrimEnd
          (local.DetailText10.Text10);
        local.DetailText30.Text30 = ", Incarceration Release Date :";
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .DetailText30.Text30;
        local.DetailText10.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .DetailText10.Text10;
        local.RaiseEventFlag.Text1 = "Y";
      }
      else if (AsChar(local.RaiseEventFlag.Text1) == 'V')
      {
        // *******************************************************************
        //   The "V" indicates that the incarceration has been verified.
        // *******************************************************************
        local.ActionEvent.Text1 = "V";
        local.Infrastructure.UserId = "JAIL";
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReferenceDate = export.JailIncarceration.StartDate;
        local.WorkDate.Date = local.Infrastructure.ReferenceDate;
        local.Infrastructure.BusinessObjectCd = "INC";
        local.Infrastructure.DenormNumeric12 =
          export.JailIncarceration.Identifier;

        // -------------------------
        // formation of detail line
        // -------------------------
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
        local.DetailText30.Text30 = "Entered Incarceration Type :";

        switch(AsChar(export.JailIncarceration.Type1))
        {
          case 'J':
            local.DetailText10.Text10 = "Jail";

            break;
          case 'P':
            local.DetailText10.Text10 = "Prison";

            break;
          default:
            break;
        }

        local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + TrimEnd
          (local.DetailText10.Text10);
        local.DetailText30.Text30 = ", Incarceration Entered Date :";
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .DetailText30.Text30;
        local.DetailText10.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .DetailText10.Text10;
        local.RaiseEventFlag.Text1 = "Y";
      }

      if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
      {
        // --------------------------------------------
        // This is to aid the event processor to
        //    gather events from a single situation
        // This is an extremely important piece of code
        // --------------------------------------------
        UseOeCabRaiseJailEvents();
      }

      export.LastReadHidden.StartDate = export.JailIncarceration.StartDate;
    }

    switch(TrimEnd(export.JailIncarceration.InstitutionName ?? ""))
    {
      case "LABETTE CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "ELLSWORTH CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "EL DORADO CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "HUTCHINSON CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "LANSING CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "LARNED CORRECTIONAL MENTAL HEALTH":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "NORTON CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "TOPEKA CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "WICHITA WORK RELEASE FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "WINFIELD CORRECTIONAL FACILITY":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "LABETTE CORR CONSERVATION CAMP":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      case "LABETTE WOMENS CORR CAMP":
        if (Equal(export.JailIncarcerationAddress.State, "KS"))
        {
          export.Facl.Flag = "Y";
        }

        break;
      default:
        // ------------------------------------------------------------
        // Continue processing - do not protect fields.  Information
        // provided was from a non FACL facility
        // -------------------------------------------------------------
        export.Facl.Flag = "";

        break;
    }

    if (Equal(global.Command, "BYPASS"))
    {
      if (!IsEmpty(export.JailIncarceration.Incarcerated) && AsChar
        (export.JailIncarceration.Incarcerated) == AsChar
        (export.HiddenJailIncarceration.Incarcerated))
      {
        var field = GetField(export.JailIncarceration, "incarcerated");

        field.Color = "cyan";
        field.Protected = true;
      }

      if (!IsEmpty(export.ProbationIncarceration.Incarcerated) && AsChar
        (export.ProbationIncarceration.Incarcerated) == AsChar
        (export.HiddenProbationIncarceration.Incarcerated))
      {
        var field = GetField(export.ProbationIncarceration, "incarcerated");

        field.Color = "cyan";
        field.Protected = true;
      }
    }
    else if (!IsEmpty(export.JailIncarceration.InstitutionName))
    {
      export.ClearPerfrmedBeforeAdd.Command = "NOT YET";
    }
    else
    {
      export.ClearPerfrmedBeforeAdd.Command = "";
    }

    if (!IsEmpty(export.Facl.Flag))
    {
      var field1 = GetField(export.JailIncarceration, "institutionName");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.JailIncarceration, "phoneAreaCode");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.JailIncarceration, "phone");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.JailIncarceration, "phoneExt");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.JailIncarcerationAddress, "street1");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.JailIncarcerationAddress, "street2");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.JailIncarcerationAddress, "state");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.JailIncarcerationAddress, "city");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.JailIncarcerationAddress, "zipCode5");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.JailIncarcerationAddress, "zipCode4");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.PromptState1, "selectChar");

      field11.Color = "cyan";
      field11.Protected = true;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveIncarceration1(Incarceration source,
    Incarceration target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Identifier = source.Identifier;
    target.VerifiedUserId = source.VerifiedUserId;
    target.VerifiedDate = source.VerifiedDate;
    target.InmateNumber = source.InmateNumber;
    target.ParoleEligibilityDate = source.ParoleEligibilityDate;
    target.WorkReleaseInd = source.WorkReleaseInd;
    target.ConditionsForRelease = source.ConditionsForRelease;
    target.ParoleOfficersTitle = source.ParoleOfficersTitle;
    target.Phone = source.Phone;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.Type1 = source.Type1;
    target.InstitutionName = source.InstitutionName;
    target.ParoleOfficerLastName = source.ParoleOfficerLastName;
    target.ParoleOfficerFirstName = source.ParoleOfficerFirstName;
    target.ParoleOfficerMiddleInitial = source.ParoleOfficerMiddleInitial;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Incarcerated = source.Incarcerated;
  }

  private static void MoveIncarceration2(Incarceration source,
    Incarceration target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Identifier = source.Identifier;
    target.VerifiedDate = source.VerifiedDate;
    target.InmateNumber = source.InmateNumber;
    target.ParoleEligibilityDate = source.ParoleEligibilityDate;
    target.WorkReleaseInd = source.WorkReleaseInd;
    target.ConditionsForRelease = source.ConditionsForRelease;
    target.Phone = source.Phone;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.Type1 = source.Type1;
    target.InstitutionName = source.InstitutionName;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.Incarcerated = source.Incarcerated;
  }

  private static void MoveIncarceration3(Incarceration source,
    Incarceration target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Identifier = source.Identifier;
    target.VerifiedDate = source.VerifiedDate;
    target.ConditionsForRelease = source.ConditionsForRelease;
    target.ParoleOfficersTitle = source.ParoleOfficersTitle;
    target.Phone = source.Phone;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.Type1 = source.Type1;
    target.ParoleOfficerLastName = source.ParoleOfficerLastName;
    target.ParoleOfficerFirstName = source.ParoleOfficerFirstName;
    target.ParoleOfficerMiddleInitial = source.ParoleOfficerMiddleInitial;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.Incarcerated = source.Incarcerated;
  }

  private static void MoveIncarcerationAddress1(IncarcerationAddress source,
    IncarcerationAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
  }

  private static void MoveIncarcerationAddress2(IncarcerationAddress source,
    IncarcerationAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdJail = source.IdJail;
    target.IdPrNumber = source.IdPrNumber;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.WorkDate.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.State.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCabCheckCaseMember1()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.WorkNameCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.WorkNameOeWorkGroup.FormattedNameText =
      useExport.WorkName.FormattedNameText;
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckCaseMember2()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckCaseMember3()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
    export.WorkNameCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.WorkNameOeWorkGroup.FormattedNameText =
      useExport.WorkName.FormattedNameText;
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckCaseMember4()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
    export.WorkNameCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.WorkNameOeWorkGroup.FormattedNameText =
      useExport.WorkName.FormattedNameText;
  }

  private void UseOeCabRaiseJailEvents()
  {
    var useImport = new OeCabRaiseJailEvents.Import();
    var useExport = new OeCabRaiseJailEvents.Export();

    useImport.RaiseEventFlag.Text1 = local.ActionEvent.Text1;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabRaiseJailEvents.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.State.CodeName = useExport.State.CodeName;
    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeFlowAddressFrmJailToAddr1()
  {
    var useImport = new OeFlowAddressFrmJailToAddr.Import();
    var useExport = new OeFlowAddressFrmJailToAddr.Export();

    MoveCommon(local.FlowCommand, useImport.Common);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.IncarcerationAddress.Assign(local.StartIncarcerationAddress);
    MoveIncarceration1(local.Start1, useImport.Incarceration);

    Call(OeFlowAddressFrmJailToAddr.Execute, useImport, useExport);
  }

  private void UseOeFlowAddressFrmJailToAddr2()
  {
    var useImport = new OeFlowAddressFrmJailToAddr.Import();
    var useExport = new OeFlowAddressFrmJailToAddr.Export();

    MoveCommon(local.FlowCommand, useImport.Common);
    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveIncarceration1(local.StartIncarceration, useImport.Incarceration);
    useImport.IncarcerationAddress.Assign(local.StartIncarcerationAddress);

    Call(OeFlowAddressFrmJailToAddr.Execute, useImport, useExport);
  }

  private void UseOeJailCreateIncarceratDetls()
  {
    var useImport = new OeJailCreateIncarceratDetls.Import();
    var useExport = new OeJailCreateIncarceratDetls.Export();

    useImport.IncarcerationAddress.Assign(local.StartIncarcerationAddress);
    useImport.Incarceration.Assign(local.StartIncarceration);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeJailCreateIncarceratDetls.Execute, useImport, useExport);

    local.Common.Flag = useExport.Common.Flag;
    local.StartIncarcerationAddress.Assign(useExport.IncarcerationAddress);
    local.StartIncarceration.Assign(useExport.Incarceration);
    export.CsePerson.Number = useExport.CsePerson.Number;
    local.Start1.Assign(useExport.Start1);
  }

  private void UseOeJailReadIncarceratDetls()
  {
    var useImport = new OeJailReadIncarceratDetls.Import();
    var useExport = new OeJailReadIncarceratDetls.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeJailReadIncarceratDetls.Execute, useImport, useExport);

    export.CsePerson.Number = useExport.CsePerson.Number;
    export.JailIncarcerationAddress.Assign(useExport.JailIncarcerationAddress);
    export.ProbationIncarcerationAddress.Assign(
      useExport.ProbationIncarcerationAddress);
    MoveIncarceration2(useExport.JailIncarceration, export.JailIncarceration);
    export.ProbationIncarceration.Assign(useExport.ProbationIncarceration);
  }

  private void UseOeJailUpdateIncarceratDetls()
  {
    var useImport = new OeJailUpdateIncarceratDetls.Import();
    var useExport = new OeJailUpdateIncarceratDetls.Export();

    useImport.IncarcerationAddress.Assign(local.StartIncarcerationAddress);
    useImport.Incarceration.Assign(local.StartIncarceration);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeJailUpdateIncarceratDetls.Execute, useImport, useExport);

    local.Common.Flag = useExport.Common.Flag;
    local.StartIncarcerationAddress.Assign(useExport.IncarcerationAddress);
    local.StartIncarceration.Assign(useExport.Incarceration);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.Hidden);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of LastReadHidden.
    /// </summary>
    [JsonPropertyName("lastReadHidden")]
    public Incarceration LastReadHidden
    {
      get => lastReadHidden ??= new();
      set => lastReadHidden = value;
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
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Common PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of WorkNameCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workNameCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkNameCsePersonsWorkSet
    {
      get => workNameCsePersonsWorkSet ??= new();
      set => workNameCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedState.
    /// </summary>
    [JsonPropertyName("selectedState")]
    public CodeValue SelectedState
    {
      get => selectedState ??= new();
      set => selectedState = value;
    }

    /// <summary>
    /// A value of PromptState2.
    /// </summary>
    [JsonPropertyName("promptState2")]
    public Common PromptState2
    {
      get => promptState2 ??= new();
      set => promptState2 = value;
    }

    /// <summary>
    /// A value of PromptState1.
    /// </summary>
    [JsonPropertyName("promptState1")]
    public Common PromptState1
    {
      get => promptState1 ??= new();
      set => promptState1 = value;
    }

    /// <summary>
    /// A value of PromptFacility.
    /// </summary>
    [JsonPropertyName("promptFacility")]
    public Common PromptFacility
    {
      get => promptFacility ??= new();
      set => promptFacility = value;
    }

    /// <summary>
    /// A value of SelectedPrisonIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("selectedPrisonIncarcerationAddress")]
    public IncarcerationAddress SelectedPrisonIncarcerationAddress
    {
      get => selectedPrisonIncarcerationAddress ??= new();
      set => selectedPrisonIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of SelectedPrisonIncarceration.
    /// </summary>
    [JsonPropertyName("selectedPrisonIncarceration")]
    public Incarceration SelectedPrisonIncarceration
    {
      get => selectedPrisonIncarceration ??= new();
      set => selectedPrisonIncarceration = value;
    }

    /// <summary>
    /// A value of WorkNameOeWorkGroup.
    /// </summary>
    [JsonPropertyName("workNameOeWorkGroup")]
    public OeWorkGroup WorkNameOeWorkGroup
    {
      get => workNameOeWorkGroup ??= new();
      set => workNameOeWorkGroup = value;
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
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public CsePerson H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of JailIncarceration.
    /// </summary>
    [JsonPropertyName("jailIncarceration")]
    public Incarceration JailIncarceration
    {
      get => jailIncarceration ??= new();
      set => jailIncarceration = value;
    }

    /// <summary>
    /// A value of HiddenJailIncarceration.
    /// </summary>
    [JsonPropertyName("hiddenJailIncarceration")]
    public Incarceration HiddenJailIncarceration
    {
      get => hiddenJailIncarceration ??= new();
      set => hiddenJailIncarceration = value;
    }

    /// <summary>
    /// A value of ProbationIncarceration.
    /// </summary>
    [JsonPropertyName("probationIncarceration")]
    public Incarceration ProbationIncarceration
    {
      get => probationIncarceration ??= new();
      set => probationIncarceration = value;
    }

    /// <summary>
    /// A value of HiddenProbationIncarceration.
    /// </summary>
    [JsonPropertyName("hiddenProbationIncarceration")]
    public Incarceration HiddenProbationIncarceration
    {
      get => hiddenProbationIncarceration ??= new();
      set => hiddenProbationIncarceration = value;
    }

    /// <summary>
    /// A value of JailIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("jailIncarcerationAddress")]
    public IncarcerationAddress JailIncarcerationAddress
    {
      get => jailIncarcerationAddress ??= new();
      set => jailIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of ProbationIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("probationIncarcerationAddress")]
    public IncarcerationAddress ProbationIncarcerationAddress
    {
      get => probationIncarcerationAddress ??= new();
      set => probationIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of Parole.
    /// </summary>
    [JsonPropertyName("parole")]
    public Common Parole
    {
      get => parole ??= new();
      set => parole = value;
    }

    /// <summary>
    /// A value of ProbationCommon.
    /// </summary>
    [JsonPropertyName("probationCommon")]
    public Common ProbationCommon
    {
      get => probationCommon ??= new();
      set => probationCommon = value;
    }

    /// <summary>
    /// A value of JailCommon.
    /// </summary>
    [JsonPropertyName("jailCommon")]
    public Common JailCommon
    {
      get => jailCommon ??= new();
      set => jailCommon = value;
    }

    /// <summary>
    /// A value of Prison.
    /// </summary>
    [JsonPropertyName("prison")]
    public Common Prison
    {
      get => prison ??= new();
      set => prison = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of Facl.
    /// </summary>
    [JsonPropertyName("facl")]
    public Common Facl
    {
      get => facl ??= new();
      set => facl = value;
    }

    /// <summary>
    /// A value of HiddenSelectedPrison.
    /// </summary>
    [JsonPropertyName("hiddenSelectedPrison")]
    public IncarcerationAddress HiddenSelectedPrison
    {
      get => hiddenSelectedPrison ??= new();
      set => hiddenSelectedPrison = value;
    }

    /// <summary>
    /// A value of ClearPerfrmedBeforeAdd.
    /// </summary>
    [JsonPropertyName("clearPerfrmedBeforeAdd")]
    public Common ClearPerfrmedBeforeAdd
    {
      get => clearPerfrmedBeforeAdd ??= new();
      set => clearPerfrmedBeforeAdd = value;
    }

    /// <summary>
    /// A value of HiddenParole.
    /// </summary>
    [JsonPropertyName("hiddenParole")]
    public Common HiddenParole
    {
      get => hiddenParole ??= new();
      set => hiddenParole = value;
    }

    /// <summary>
    /// A value of HiddenProbationCommon.
    /// </summary>
    [JsonPropertyName("hiddenProbationCommon")]
    public Common HiddenProbationCommon
    {
      get => hiddenProbationCommon ??= new();
      set => hiddenProbationCommon = value;
    }

    /// <summary>
    /// A value of HiddenJailCommon.
    /// </summary>
    [JsonPropertyName("hiddenJailCommon")]
    public Common HiddenJailCommon
    {
      get => hiddenJailCommon ??= new();
      set => hiddenJailCommon = value;
    }

    /// <summary>
    /// A value of HiddenPrison.
    /// </summary>
    [JsonPropertyName("hiddenPrison")]
    public Common HiddenPrison
    {
      get => hiddenPrison ??= new();
      set => hiddenPrison = value;
    }

    /// <summary>
    /// A value of HiddenJailIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("hiddenJailIncarcerationAddress")]
    public IncarcerationAddress HiddenJailIncarcerationAddress
    {
      get => hiddenJailIncarcerationAddress ??= new();
      set => hiddenJailIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of HiddenProbationIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("hiddenProbationIncarcerationAddress")]
    public IncarcerationAddress HiddenProbationIncarcerationAddress
    {
      get => hiddenProbationIncarcerationAddress ??= new();
      set => hiddenProbationIncarcerationAddress = value;
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

    private Incarceration lastReadHidden;
    private Case1 case1;
    private Common personPrompt;
    private Document print;
    private CsePersonsWorkSet workNameCsePersonsWorkSet;
    private CodeValue selectedState;
    private Common promptState2;
    private Common promptState1;
    private Common promptFacility;
    private IncarcerationAddress selectedPrisonIncarcerationAddress;
    private Incarceration selectedPrisonIncarceration;
    private OeWorkGroup workNameOeWorkGroup;
    private CsePerson csePerson;
    private CsePerson h;
    private Incarceration jailIncarceration;
    private Incarceration hiddenJailIncarceration;
    private Incarceration probationIncarceration;
    private Incarceration hiddenProbationIncarceration;
    private IncarcerationAddress jailIncarcerationAddress;
    private IncarcerationAddress probationIncarcerationAddress;
    private Common parole;
    private Common probationCommon;
    private Common jailCommon;
    private Common prison;
    private NextTranInfo hidden;
    private Standard standard;
    private Common facl;
    private IncarcerationAddress hiddenSelectedPrison;
    private Common clearPerfrmedBeforeAdd;
    private Common hiddenParole;
    private Common hiddenProbationCommon;
    private Common hiddenJailCommon;
    private Common hiddenPrison;
    private IncarcerationAddress hiddenJailIncarcerationAddress;
    private IncarcerationAddress hiddenProbationIncarcerationAddress;
    private NextTranInfo alrt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of LastReadHidden.
    /// </summary>
    [JsonPropertyName("lastReadHidden")]
    public Incarceration LastReadHidden
    {
      get => lastReadHidden ??= new();
      set => lastReadHidden = value;
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
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Common PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of WorkNameCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workNameCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkNameCsePersonsWorkSet
    {
      get => workNameCsePersonsWorkSet ??= new();
      set => workNameCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of PromptState2.
    /// </summary>
    [JsonPropertyName("promptState2")]
    public Common PromptState2
    {
      get => promptState2 ??= new();
      set => promptState2 = value;
    }

    /// <summary>
    /// A value of PromptState1.
    /// </summary>
    [JsonPropertyName("promptState1")]
    public Common PromptState1
    {
      get => promptState1 ??= new();
      set => promptState1 = value;
    }

    /// <summary>
    /// A value of PromptFacility.
    /// </summary>
    [JsonPropertyName("promptFacility")]
    public Common PromptFacility
    {
      get => promptFacility ??= new();
      set => promptFacility = value;
    }

    /// <summary>
    /// A value of WorkNameOeWorkGroup.
    /// </summary>
    [JsonPropertyName("workNameOeWorkGroup")]
    public OeWorkGroup WorkNameOeWorkGroup
    {
      get => workNameOeWorkGroup ??= new();
      set => workNameOeWorkGroup = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public CsePerson H
    {
      get => h ??= new();
      set => h = value;
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
    /// A value of JailIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("jailIncarcerationAddress")]
    public IncarcerationAddress JailIncarcerationAddress
    {
      get => jailIncarcerationAddress ??= new();
      set => jailIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of ProbationIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("probationIncarcerationAddress")]
    public IncarcerationAddress ProbationIncarcerationAddress
    {
      get => probationIncarcerationAddress ??= new();
      set => probationIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of HiddenJailIncarceration.
    /// </summary>
    [JsonPropertyName("hiddenJailIncarceration")]
    public Incarceration HiddenJailIncarceration
    {
      get => hiddenJailIncarceration ??= new();
      set => hiddenJailIncarceration = value;
    }

    /// <summary>
    /// A value of JailIncarceration.
    /// </summary>
    [JsonPropertyName("jailIncarceration")]
    public Incarceration JailIncarceration
    {
      get => jailIncarceration ??= new();
      set => jailIncarceration = value;
    }

    /// <summary>
    /// A value of HiddenProbationIncarceration.
    /// </summary>
    [JsonPropertyName("hiddenProbationIncarceration")]
    public Incarceration HiddenProbationIncarceration
    {
      get => hiddenProbationIncarceration ??= new();
      set => hiddenProbationIncarceration = value;
    }

    /// <summary>
    /// A value of ProbationIncarceration.
    /// </summary>
    [JsonPropertyName("probationIncarceration")]
    public Incarceration ProbationIncarceration
    {
      get => probationIncarceration ??= new();
      set => probationIncarceration = value;
    }

    /// <summary>
    /// A value of JailCommon.
    /// </summary>
    [JsonPropertyName("jailCommon")]
    public Common JailCommon
    {
      get => jailCommon ??= new();
      set => jailCommon = value;
    }

    /// <summary>
    /// A value of Prison.
    /// </summary>
    [JsonPropertyName("prison")]
    public Common Prison
    {
      get => prison ??= new();
      set => prison = value;
    }

    /// <summary>
    /// A value of Parole.
    /// </summary>
    [JsonPropertyName("parole")]
    public Common Parole
    {
      get => parole ??= new();
      set => parole = value;
    }

    /// <summary>
    /// A value of ProbationCommon.
    /// </summary>
    [JsonPropertyName("probationCommon")]
    public Common ProbationCommon
    {
      get => probationCommon ??= new();
      set => probationCommon = value;
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
    /// A value of Facl.
    /// </summary>
    [JsonPropertyName("facl")]
    public Common Facl
    {
      get => facl ??= new();
      set => facl = value;
    }

    /// <summary>
    /// A value of HiddenSelectedPrison.
    /// </summary>
    [JsonPropertyName("hiddenSelectedPrison")]
    public IncarcerationAddress HiddenSelectedPrison
    {
      get => hiddenSelectedPrison ??= new();
      set => hiddenSelectedPrison = value;
    }

    /// <summary>
    /// A value of ClearPerfrmedBeforeAdd.
    /// </summary>
    [JsonPropertyName("clearPerfrmedBeforeAdd")]
    public Common ClearPerfrmedBeforeAdd
    {
      get => clearPerfrmedBeforeAdd ??= new();
      set => clearPerfrmedBeforeAdd = value;
    }

    /// <summary>
    /// A value of HiddenParole.
    /// </summary>
    [JsonPropertyName("hiddenParole")]
    public Common HiddenParole
    {
      get => hiddenParole ??= new();
      set => hiddenParole = value;
    }

    /// <summary>
    /// A value of HiddenProbationCommon.
    /// </summary>
    [JsonPropertyName("hiddenProbationCommon")]
    public Common HiddenProbationCommon
    {
      get => hiddenProbationCommon ??= new();
      set => hiddenProbationCommon = value;
    }

    /// <summary>
    /// A value of HiddenJailCommon.
    /// </summary>
    [JsonPropertyName("hiddenJailCommon")]
    public Common HiddenJailCommon
    {
      get => hiddenJailCommon ??= new();
      set => hiddenJailCommon = value;
    }

    /// <summary>
    /// A value of HiddenPrison.
    /// </summary>
    [JsonPropertyName("hiddenPrison")]
    public Common HiddenPrison
    {
      get => hiddenPrison ??= new();
      set => hiddenPrison = value;
    }

    /// <summary>
    /// A value of HiddenJailIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("hiddenJailIncarcerationAddress")]
    public IncarcerationAddress HiddenJailIncarcerationAddress
    {
      get => hiddenJailIncarcerationAddress ??= new();
      set => hiddenJailIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of HiddenProbationIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("hiddenProbationIncarcerationAddress")]
    public IncarcerationAddress HiddenProbationIncarcerationAddress
    {
      get => hiddenProbationIncarcerationAddress ??= new();
      set => hiddenProbationIncarcerationAddress = value;
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

    private Common docmProtectFilter;
    private Incarceration lastReadHidden;
    private Case1 case1;
    private Common personPrompt;
    private Document print;
    private CsePersonsWorkSet workNameCsePersonsWorkSet;
    private NextTranInfo hidden;
    private Code state;
    private Common promptState2;
    private Common promptState1;
    private Common promptFacility;
    private OeWorkGroup workNameOeWorkGroup;
    private CsePerson h;
    private CsePerson csePerson;
    private IncarcerationAddress jailIncarcerationAddress;
    private IncarcerationAddress probationIncarcerationAddress;
    private Incarceration hiddenJailIncarceration;
    private Incarceration jailIncarceration;
    private Incarceration hiddenProbationIncarceration;
    private Incarceration probationIncarceration;
    private Common jailCommon;
    private Common prison;
    private Common parole;
    private Common probationCommon;
    private Standard standard;
    private Common facl;
    private IncarcerationAddress hiddenSelectedPrison;
    private Common clearPerfrmedBeforeAdd;
    private Common hiddenParole;
    private Common hiddenProbationCommon;
    private Common hiddenJailCommon;
    private Common hiddenPrison;
    private IncarcerationAddress hiddenJailIncarcerationAddress;
    private IncarcerationAddress hiddenProbationIncarcerationAddress;
    private NextTranInfo alrt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Start1.
    /// </summary>
    [JsonPropertyName("start1")]
    public Incarceration Start1
    {
      get => start1 ??= new();
      set => start1 = value;
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
    /// A value of InitializedIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("initializedIncarcerationAddress")]
    public IncarcerationAddress InitializedIncarcerationAddress
    {
      get => initializedIncarcerationAddress ??= new();
      set => initializedIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of InitializedIncarceration.
    /// </summary>
    [JsonPropertyName("initializedIncarceration")]
    public Incarceration InitializedIncarceration
    {
      get => initializedIncarceration ??= new();
      set => initializedIncarceration = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of WorkDate.
    /// </summary>
    [JsonPropertyName("workDate")]
    public DateWorkArea WorkDate
    {
      get => workDate ??= new();
      set => workDate = value;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
    }

    /// <summary>
    /// A value of StartIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("startIncarcerationAddress")]
    public IncarcerationAddress StartIncarcerationAddress
    {
      get => startIncarcerationAddress ??= new();
      set => startIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of StartIncarceration.
    /// </summary>
    [JsonPropertyName("startIncarceration")]
    public Incarceration StartIncarceration
    {
      get => startIncarceration ??= new();
      set => startIncarceration = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
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
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of JailInfoModified.
    /// </summary>
    [JsonPropertyName("jailInfoModified")]
    public Common JailInfoModified
    {
      get => jailInfoModified ??= new();
      set => jailInfoModified = value;
    }

    /// <summary>
    /// A value of ParoleInfoModified.
    /// </summary>
    [JsonPropertyName("paroleInfoModified")]
    public Common ParoleInfoModified
    {
      get => paroleInfoModified ??= new();
      set => paroleInfoModified = value;
    }

    /// <summary>
    /// A value of FlowCommand.
    /// </summary>
    [JsonPropertyName("flowCommand")]
    public Common FlowCommand
    {
      get => flowCommand ??= new();
      set => flowCommand = value;
    }

    /// <summary>
    /// A value of ActionEvent.
    /// </summary>
    [JsonPropertyName("actionEvent")]
    public WorkArea ActionEvent
    {
      get => actionEvent ??= new();
      set => actionEvent = value;
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
    /// A value of FlowAddress.
    /// </summary>
    [JsonPropertyName("flowAddress")]
    public Common FlowAddress
    {
      get => flowAddress ??= new();
      set => flowAddress = value;
    }

    private Incarceration start1;
    private NextTranInfo null1;
    private IncarcerationAddress initializedIncarcerationAddress;
    private Incarceration initializedIncarceration;
    private Common work;
    private Incarceration incarceration;
    private WorkArea print;
    private Common position;
    private SpDocLiteral spDocLiteral;
    private Common common;
    private DateWorkArea blank;
    private DateWorkArea workDate;
    private TextWorkArea detailText10;
    private Code state;
    private Common workError;
    private IncarcerationAddress startIncarcerationAddress;
    private Incarceration startIncarceration;
    private Code maxDate;
    private CodeValue codeValue;
    private Common returnCode;
    private Common checkZip;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private TextWorkArea detailText30;
    private WorkArea raiseEventFlag;
    private Infrastructure lastTran;
    private Common jailInfoModified;
    private Common paroleInfoModified;
    private Common flowCommand;
    private WorkArea actionEvent;
    private DateWorkArea current;
    private Common flowAddress;
  }
#endregion
}
