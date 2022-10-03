// Program: QA_QAPD_QUICK_AP_DATA, ID: 372228163, model: 746.
// Short name: SWEQAPDP
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
/// A program: QA_QAPD_QUICK_AP_DATA.
/// </para>
/// <para>
/// RESP:   Quality Assurance.
/// QA Quick Reference AP Data Screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class QaQapdQuickApData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the QA_QAPD_QUICK_AP_DATA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new QaQapdQuickApData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of QaQapdQuickApData.
  /// </summary>
  public QaQapdQuickApData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------
    //                      Maintenance Log
    // Date       Developer                Description
    // 12/03/98   JF Caillouet             Initial Development
    // --------------------------------------------------------
    // 04/05/00 W.Campbell         Made changes so that
    //                             entering via NEXTTRAN will
    //                             perform a DISPLAY.  Also,
    //                             modified the view matching
    //                             for the call to SECURITY so
    //                             that it passes the export next
    //                             case number instead of the export
    //                             case number into the security cab.
    //                             Work done on WR# 00162
    //                             for PRWORA - Family Violence.
    // --------------------------------------------------------
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // 02/16/2001     Vithal Madhira                 PR# 113678
    // Need to display the latest active attorney for the AP and for the case.  
    // (Per SME: Snan Beall).
    // 10/28/02 K.Doshi           Fix screen Help.
    // --------------------------------------------------------
    // PR158234. Install change to links to make the screens flow correctly for 
    // PF19 and pf20. 12-20-02. LJB
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    UseCabSetMaximumDiscontinueDate();

    // ---  Move Section  --
    MoveCase2(import.Case1, export.Case1);
    MoveCaseFuncWorkSet(import.CaseFuncWorkSet, export.CaseFuncWorkSet);
    MoveCodeValue(import.CaseCloseRsn, export.CaseCloseRsn);
    MoveHealthInsuranceViability(import.HealthInsuranceViability,
      export.HealthInsuranceViability);
    export.Program.Code = import.Program.Code;
    export.ProgCodeDescription.Description =
      import.ProgCodeDescription.Description;
    MoveServiceProvider(import.ServiceProvider, export.ServiceProvider);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCaseRole(import.ApCaseRole, export.ApCaseRole);
    export.ApClient.Assign(import.ApClient);
    export.ApCsePersonAddress.Assign(import.ApCsePersonAddress);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.Ar.Assign(import.ArCsePersonsWorkSet);
    export.Bankruptcy.Flag = import.Bankruptcy.Flag;
    export.Hidden.Assign(import.Hidden);
    export.Incarceration.Flag = import.Incarceration.Flag;
    export.Military.Flag = import.Military.Flag;
    export.MultipleAps.Flag = import.MultipleAps.Flag;
    export.ApMultiCases.Flag = import.ApMultiCases.Flag;
    export.Next.Number = import.Next.Number;
    export.OtherChilderen.Flag = import.OtherChildren.Flag;
    export.Uci.Flag = import.Uci.Flag;
    export.ArMultiCases.Flag = import.ArMultiCases.Flag;
    export.AltSsnAlias.Text30 = import.AltSsnAlias.Text30;
    MoveOffice(import.Office, export.Office);
    export.ApMultiCasesPrompt.Flag = import.ApMultiCasesPrompt.Flag;
    local.ArAltOccur.Flag = import.ArAltOccur.Flag;
    export.ArMultiCasesPrompt.Flag = import.ArMultiCasesPrompt.Flag;
    export.ComnLink.Assign(import.ComnLink);
    export.MaritalStatDescription.Description =
      import.MaritalStatDescription.Description;

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    // WR20202  12-19-2001 by L. Bachura. This WR provides that the QAPD screen 
    // displays a closed case.  All info will display except case program. Per
    // Karen Buchelle, it is not necessary to display case program.  The change
    // is effected by deleting the comparison of the discontinue date for the
    // case role to the local current date in the display section of the code.
    // The deleted statment is "and desired xx discontinue date is >
    // local_current_date work area date."  This check was to verify that the
    // discontinue date was 2099-12-13.
    // ---  Next Tran and Security Logic  --
    if (!IsEmpty(export.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

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
      export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
        (10);
      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        // --------------------------------------------------------
        // 04/05/00 W.Campbell - Made changes so that
        // entering via NEXTTRAN will perform a DISPLAY.
        // Moved the following ESCAPE statement from
        // below this ELSE to inside it.
        // Work done on WR# 00162
        // for PRWORA - Family Violence.
        // --------------------------------------------------------
        return;
      }

      // --------------------------------------------------------
      // 04/05/00 W.Campbell - Made changes so that
      // entering via NEXTTRAN will perform a DISPLAY.
      // Added the following COMMAND IS DISPLAY statement.
      // Work done on WR# 00162
      // for PRWORA - Family Violence.
      // --------------------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // --------------------------------------------------------
      // 04/05/00 W.Campbell -  Modified the
      // view matching for the call to SECURITY
      // so that it passes the export next case
      // number instead of the export case number
      // into the security cab.
      // Work done on WR# 00162
      // for PRWORA - Family Violence.
      // --------------------------------------------------------
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    local.Current.Date = Now().Date;

    // ***  Set Prompt Screen Characteristics  ***
    if (AsChar(export.ApMultiCases.Flag) == 'Y')
    {
      var field = GetField(export.ApMultiCasesPrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.ArMultiCases.Flag) == 'Y')
    {
      var field = GetField(export.ArMultiCasesPrompt, "flag");

      field.Color = "green";
      field.Protected = false;
    }

    if (AsChar(export.MultipleAps.Flag) == 'Y')
    {
      var field = GetField(export.ApPrompt, "selectChar");

      field.Color = "green";
      field.Protected = false;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETCOMN":
        if (AsChar(export.ArMultiCasesPrompt.Flag) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            export.Ar.Assign(export.ComnLink);
          }
        }
        else if (AsChar(export.ApMultiCasesPrompt.Flag) == 'S')
        {
          if (IsEmpty(export.Next.Number))
          {
            export.Next.Number = export.Case1.Number;
          }
          else
          {
            export.ApCsePersonsWorkSet.Assign(export.ComnLink);
          }
        }

        global.Command = "DISPLAY";

        break;
      case "RETCOMP":
        var field = GetField(export.ApPrompt, "selectChar");

        field.Color = "green";
        field.Protected = false;

        if (IsEmpty(import.FromComp.Number))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field1 = GetField(export.ApCsePersonsWorkSet, "number");

          field1.Error = true;

          return;
        }

        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        break;
      case "LIST":
        // Each prompt should have the following IF Statement type
        switch(AsChar(export.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field1 = GetField(export.ApPrompt, "selectChar");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.ApMultiCasesPrompt.Flag))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            MoveCsePersonsWorkSet2(export.ApCsePersonsWorkSet, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field1 = GetField(export.ApMultiCasesPrompt, "flag");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.ArMultiCasesPrompt.Flag))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            MoveCsePersonsWorkSet3(export.Ar, export.ComnLink);
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field1 = GetField(export.ArMultiCasesPrompt, "flag");

            field1.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            // Each prompt should have the following IF Statement type
            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            break;
        }

        break;
      case "APDS":
        ExitState = "ECO_LNK_TO_AP_DETAILS";

        break;
      case "JAIL":
        ExitState = "ECO_LNK_TO_JAIL";

        break;
      case "ALTS":
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        break;
      case "ADDR":
        ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";

        break;
      case "PVSCR":
        ExitState = "ECO_XFR_TO_PREV";

        break;
      case "NXSCR":
        ExitState = "ECO_XFR_TO_NEXT_SCRN";

        break;
      case "HICV":
        ExitState = "ECO_LNK_TO_HICV";

        break;
      case "BKRP":
        ExitState = "ECO_LNK_TO_BKRP_BANKRUPTCY";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.ApPrompt.SelectChar = "";
      export.ArMultiCasesPrompt.Flag = "";
      export.ApMultiCasesPrompt.Flag = "";

      if (IsEmpty(export.Next.Number) || !
        Equal(export.Next.Number, export.Case1.Number))
      {
        export.Office.Name = "";
        export.Office.TypeCode = "";
        export.ServiceProvider.LastName = "";
        export.ServiceProvider.FirstName = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseFuncWorkSet.FuncDate = local.NullDateWorkArea.Date;
        export.CaseUnitFunctionAssignmt.Function = "";
        export.Military.Flag = "";
        export.Uci.Flag = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.MaritalStatDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CaseCloseRsn.Cdvalue = "";
        export.CaseFuncWorkSet.FuncText3 = "";
        export.CaseUnitFunctionAssignmt.Function = "";
        export.AltSsnAlias.Text30 = "";
        export.HealthInsuranceViability.HinsViableInd = "";
        export.ApCsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);
        export.Ar.Assign(local.NullCsePersonsWorkSet);
        MoveCsePersonAddress(local.NullCsePersonAddress,
          export.ApCsePersonAddress);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.Case1.Assign(local.NullCase);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;
      }

      if (IsEmpty(export.Next.Number))
      {
        ExitState = "CASE_NUMBER_REQUIRED";

        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      if (!Equal(export.Next.Number, export.Case1.Number))
      {
        UseCabZeroFillNumber();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        export.Case1.Number = export.Next.Number;
        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        // ***********************************************************
        // ***  CASE DETAILS
        // 
        // ***
        // ***********************************************************
        // *** READ CASE  ***
        local.CaseOpen.Flag = "N";

        if (ReadCase2())
        {
          MoveCase3(entities.Case1, export.Case1);

          if (AsChar(export.Case1.Status) == 'O')
          {
            export.Case1.StatusDate = local.NullDateWorkArea.Date;
            local.CaseOpen.Flag = "Y";
          }
        }
        else
        {
          ExitState = "CASE_NF";

          return;
        }

        if (!IsEmpty(export.Case1.ClosureReason))
        {
          if (ReadCodeValue1())
          {
            export.CaseCloseRsn.Description = entities.CodeValue.Description;
          }
          else
          {
            var field = GetField(export.CaseCloseRsn, "description");

            field.Error = true;
          }
        }
        else
        {
          export.CaseCloseRsn.Cdvalue = "";
        }

        // ***********************************************************
        // ***  AR DETAILS
        // 
        // ***
        // ***********************************************************
        // *** Find AR  ***
        // PR139491 on 2-25-2002. Installed flag and read modification so that 
        // can get open AR on a case when there are both open and closed AR's on
        // the same case. LBachura.
        if (AsChar(local.CaseOpen.Flag) == 'Y')
        {
          if (ReadCsePerson2())
          {
            export.Ar.Number = entities.CsePerson.Number;
          }
          else
          {
            ExitState = "CASE_ROLE_AR_NF";

            return;
          }
        }

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          if (ReadCsePerson3())
          {
            export.Ar.Number = entities.CsePerson.Number;
          }
          else
          {
            ExitState = "CASE_ROLE_AR_NF";

            return;
          }
        }

        // *** AR on more than one case?  ***
        if (ReadCase1())
        {
          export.ArMultiCases.Flag = "Y";

          var field = GetField(export.ArMultiCasesPrompt, "flag");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          export.ArMultiCases.Flag = "N";

          var field = GetField(export.ArMultiCasesPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;
        }

        // *** Get AR details  ***
        UseSiReadCsePerson();

        if (!IsEmpty(local.AbendData.Type1) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(local.AbendData.Type1))
          {
            var field = GetField(export.Ar, "number");

            field.Error = true;

            ExitState = "CO0000_AR_NF";
          }

          return;
        }

        // *** Determine multiple AP's     ***
        export.MultipleAps.Flag = "";

        foreach(var item in ReadCsePerson4())
        {
          if (IsEmpty(export.MultipleAps.Flag))
          {
            export.MultipleAps.Flag = "N";
            export.ApCsePersonsWorkSet.Number = entities.CsePerson.Number;
          }
          else
          {
            export.MultipleAps.Flag = "Y";
            export.ApCsePersonsWorkSet.Number = "";

            break;
          }
        }

        if (IsEmpty(export.MultipleAps.Flag))
        {
          ExitState = "AP_FOR_CASE_NF";

          return;
        }
      }

      // ***********************************************************
      // ***  AP DETAILS
      // 
      // ***
      // ***********************************************************
      if (AsChar(export.MultipleAps.Flag) == 'Y')
      {
        if (IsEmpty(import.FromComp.Number) && IsEmpty
          (export.ApCsePersonsWorkSet.Number))
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        var field = GetField(export.ApPrompt, "selectChar");

        field.Color = "green";
        field.Protected = false;
      }

      if (!Equal(import.FromComp.Number, export.ApCsePersonsWorkSet.Number) || !
        IsEmpty(import.FromComp.Number))
      {
        export.Military.Flag = "";
        export.Uci.Flag = "";
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.MaritalStatDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.AltSsnAlias.Text30 = "";
        export.HealthInsuranceViability.HinsViableInd = "";
        MoveCsePersonAddress(local.NullCsePersonAddress,
          export.ApCsePersonAddress);
        MoveCsePerson(local.NullCsePerson, export.ApClient);
        export.ApClient.DateOfDeath = local.NullDateWorkArea.Date;

        if (!IsEmpty(import.FromComp.Number))
        {
          export.ApCsePersonsWorkSet.Number = import.FromComp.Number;
        }

        // *** Get AP details  ***
        UseSiReadApCaseRoleDetails();

        // PR140816. Installed the following logic so that the inactive AP's 
        // would not be displayed on 4-2-02. Lbachura
        // PR146446 0n 8-5-02 by L. Bachura. The two reads hadnle situations 
        // where there is an inactive ap role or if the same ap was inactive and
        // is now active.
        local.ApOpen.Flag = "N";

        if (AsChar(export.MultipleAps.Flag) == 'Y')
        {
          if (ReadCsePerson1())
          {
            local.ApOpen.Flag = "Y";
          }
        }

        if (AsChar(export.MultipleAps.Flag) == 'N' && !
          Lt(Now().Date, export.ApCaseRole.EndDate))
        {
          if (ReadCsePerson1())
          {
            local.ApOpen.Flag = "Y";
          }
        }

        // End PR146446 code. LJB
        if (AsChar(local.ApOpen.Flag) == 'N' && !
          Lt(Now().Date, export.ApCaseRole.EndDate))
        {
          export.ApCsePersonsWorkSet.FormattedName = "";
          export.ApCsePersonsWorkSet.Number = "";
          export.ApCsePersonsWorkSet.Ssn = "";
          export.ApCsePersonsWorkSet.Sex = "";
          export.ApCsePersonsWorkSet.Dob = local.NullCsePersonsWorkSet.Dob;
          export.ApClient.NameMaiden = "";
          export.ApClient.CurrentSpouseLastName = "";
          export.ApClient.CurrentSpouseFirstName = "";
          export.ApClient.CurrentSpouseMi = "";
          export.ApClient.AeCaseNumber = "";
          export.ApClient.DateOfDeath = local.NullCsePerson.DateOfDeath;
          export.ApClient.HomePhoneAreaCode =
            local.NullCsePerson.HomePhoneAreaCode.GetValueOrDefault();
          export.ApClient.HomePhone =
            local.NullCsePerson.HomePhone.GetValueOrDefault();
          export.ApClient.WorkPhoneAreaCode =
            local.NullCsePerson.WorkPhoneAreaCode.GetValueOrDefault();
          export.ApClient.WorkPhone =
            local.NullCsePerson.WorkPhone.GetValueOrDefault();
          export.ApClient.WorkPhoneExt = "";
          export.ApMultiCases.Flag = "";
          export.OtherChilderen.Flag = "";
          export.HealthInsuranceViability.HinsViableInd = "";
          export.Uci.Flag = "";
          export.Military.Flag = "";
          export.Incarceration.Flag = "";
          export.FplsLocateRequest.TransactionStatus = "";
          export.FplsLocateRequest.ZdelReqCreatDt = local.NullDateWorkArea.Date;
          export.Bankruptcy.Flag = "";
          export.PersonPrivateAttorney.FirstName = "";
          export.PersonPrivateAttorney.LastName = "";
          export.PersonPrivateAttorney.MiddleInitial = "";
          export.PersonPrivateAttorney.PhoneExt = "";
          export.PersonPrivateAttorney.PhoneAreaCode =
            local.NullCsePerson.WorkPhoneAreaCode.GetValueOrDefault();
          export.PersonPrivateAttorney.Phone =
            local.NullCsePerson.WorkPhone.GetValueOrDefault();

          var field = GetField(export.ApMultiCasesPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "OE0000_AP_INACTIVE_ON_THIS_CASE";

          return;
        }

        if (!IsEmpty(local.AbendData.Type1) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(local.AbendData.Type1))
          {
            var field = GetField(export.ApCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "CO0000_AP_NF";
          }

          return;
        }

        // *** AP on more than one case?  ***
        if (AsChar(export.ApMultiCases.Flag) == 'Y')
        {
          var field = GetField(export.ApMultiCasesPrompt, "flag");

          field.Color = "green";
          field.Protected = false;
        }
        else
        {
          var field = GetField(export.ApMultiCasesPrompt, "flag");

          field.Color = "cyan";
          field.Protected = true;
        }

        UseSiAltsBuildAliasAndSsn();

        if (AsChar(local.ArAltOccur.Flag) == 'Y' || AsChar
          (local.ApAltOccur.Flag) == 'Y')
        {
          export.AltSsnAlias.Text30 = "Alt SSN/Alias";
        }
        else
        {
          export.AltSsnAlias.Text30 = "";
        }

        // *** Set blank SSN's to spaces  ***
        if (Equal(export.ApCsePersonsWorkSet.Ssn, "000000000"))
        {
          export.ApCsePersonsWorkSet.Ssn = "";
        }

        if (Equal(export.Ar.Ssn, "000000000"))
        {
          export.Ar.Ssn = "";
        }

        if (ReadHealthInsuranceViability())
        {
          export.HealthInsuranceViability.HinsViableInd =
            entities.HealthInsuranceViability.HinsViableInd;
        }

        // -------------------------------------------------------------------------------------
        // Per PR#85029 the following code is commented and new code(CAB 
        // SI_GET_CSE_PERSON_MAILING_ADDR)  is added to display the correct
        // address.
        //                                              
        // ------- Vithal Madhira(01-13-2000)
        // -------------------------------------------------------------------------------------
        UseSiGetCsePersonMailingAddr();

        if (Equal(export.ApCsePersonAddress.EndDate, local.Max.Date))
        {
          export.ApCsePersonAddress.EndDate = local.NullDateWorkArea.Date;
        }

        if (Equal(export.ApCsePersonAddress.VerifiedDate, local.Max.Date))
        {
          export.ApCsePersonAddress.VerifiedDate = local.NullDateWorkArea.Date;
        }

        // ---------------------------------------------------------------------------------
        //  PR# 113678 :  Need to display the latest active attorney for the AP 
        // and for the case.  (Per SME: Snan Beall).
        //         USE   READ EACH
        //                    WHERE DATE_DISMISSED = '2099-12-31'
        //                    SORT BY IDENTIFIER ASC
        //                                                
        // --Vithal Madhira (02/16/2001)
        //    PS:  For some reason,  if Date_Dismissed is not entered on screen 
        // it is set to '0001-01-01' in some previous cases. Now it will be set
        // to '2099-12-31'. SME updated CKT_PRSN_PRIV_ATTR in production to set
        // the Date_dismissed to '2099-12-31' where Date_dismissed is '0001-01-
        // 01'.
        // Also during conversion the date_retained is set to '0001-01-01'. SME 
        // set this to date if conversion ie. '1999-09-04'.
        // ------------------------------------------------------------------------------------
        if (ReadPersonPrivateAttorney())
        {
          export.PersonPrivateAttorney.Assign(entities.PersonPrivateAttorney);
        }

        if (!IsEmpty(export.ApClient.CurrentMaritalStatus))
        {
          if (ReadCodeValue3())
          {
            export.MaritalStatDescription.Description =
              entities.CodeValue.Description;
          }
          else
          {
            var field = GetField(export.MaritalStatDescription, "description");

            field.Error = true;
          }
        }
        else
        {
          export.MaritalStatDescription.Description =
            Spaces(CodeValue.Description_MaxLength);
        }
      }

      UseSiCadsReadCaseDetails();

      if (!IsEmpty(export.Program.Code))
      {
        if (ReadCodeValue2())
        {
          export.ProgCodeDescription.Description =
            entities.CodeValue.Description;
        }
        else
        {
          var field = GetField(export.ProgCodeDescription, "description");

          field.Error = true;
        }
      }
      else
      {
        export.ProgCodeDescription.Description =
          Spaces(CodeValue.Description_MaxLength);
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -----------------------------------------------------------------------
        // Call the SECURITY CAB to check for Family_Violence.
        // Since this is a person (AP) driven screen, pass the Cse_Person number
        // only to the CAB.
        // ------------------------------------------------------------------------
        UseScSecurityCheckForFv();

        if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
        {
          MoveCsePersonAddress(local.NullCsePersonAddress,
            export.ApCsePersonAddress);
          export.ApClient.HomePhone = 0;
          export.ApClient.HomePhoneAreaCode = 0;
          export.ApClient.WorkPhone = 0;
          export.ApClient.WorkPhoneAreaCode = 0;
          export.ApClient.WorkPhoneExt = "";

          return;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      if (AsChar(export.Case1.Status) == 'C')
      {
        ExitState = "ACO_NI0000_DISPLAY_CLOSED_CASE";
      }
    }
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.OfficeIdentifier = source.OfficeIdentifier;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
    target.ClosureLetterDate = source.ClosureLetterDate;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText3 = source.FuncText3;
    target.FuncDate = source.FuncDate;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.NumberOfChildren = source.NumberOfChildren;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveHealthInsuranceViability(
    HealthInsuranceViability source, HealthInsuranceViability target)
  {
    target.Identifier = source.Identifier;
    target.HinsViableInd = source.HinsViableInd;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    MoveCsePersonsWorkSet1(export.ApCsePersonsWorkSet, useImport.Ap1);
    MoveCsePersonsWorkSet1(export.Ar, useImport.Ar1);

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    local.ApAltOccur.Flag = useExport.ApOccur.Flag;
    local.ArAltOccur.Flag = useExport.ArOccur.Flag;
  }

  private void UseSiCadsReadCaseDetails()
  {
    var useImport = new SiCadsReadCaseDetails.Import();
    var useExport = new SiCadsReadCaseDetails.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.Ar.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiCadsReadCaseDetails.Execute, useImport, useExport);

    export.Program.Code = useExport.Program.Code;
    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, export.CaseFuncWorkSet);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = export.ApClient.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, export.ApCsePersonAddress);
  }

  private void UseSiReadApCaseRoleDetails()
  {
    var useImport = new SiReadApCaseRoleDetails.Import();
    var useExport = new SiReadApCaseRoleDetails.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.Ar.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiReadApCaseRoleDetails.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.ApCaseRole.Assign(useExport.ApCaseRole);
    export.Uci.Flag = useExport.Uci.Flag;
    export.Military.Flag = useExport.Military.Flag;
    export.Incarceration.Flag = useExport.Incarceration.Flag;
    export.Bankruptcy.Flag = useExport.Bankruptcy.Flag;
    export.OtherChilderen.Flag = useExport.OtherChildren.Flag;
    export.ApMultiCases.Flag = useExport.MultipleCases.Flag;
    MoveCsePerson(useExport.ApCsePerson, export.ApClient);
    MoveCsePersonsWorkSet5(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet4(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "numb", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Next.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "closureReason", export.Case1.ClosureReason ?? "");
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.SetString(command, "code", export.Program.Code);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue3()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "currentMaritalStatus",
          export.ApClient.CurrentMaritalStatus ?? "");
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetString(command, "numb", export.ApClient.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadHealthInsuranceViability()
  {
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Next.Number);
        db.SetNullableString(
          command, "cspNum", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private bool ReadPersonPrivateAttorney()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", export.Next.Number);
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
        db.
          SetDate(command, "dateDismissed", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.DateRetained = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 4);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.PersonPrivateAttorney.Phone = db.GetNullableInt32(reader, 9);
        entities.PersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.PersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 11);
        entities.PersonPrivateAttorney.Populated = true;
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
    /// <summary>
    /// A value of FromComp.
    /// </summary>
    [JsonPropertyName("fromComp")]
    public CsePersonsWorkSet FromComp
    {
      get => fromComp ??= new();
      set => fromComp = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePersonsWorkSet Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of MaritalStatDescription.
    /// </summary>
    [JsonPropertyName("maritalStatDescription")]
    public CodeValue MaritalStatDescription
    {
      get => maritalStatDescription ??= new();
      set => maritalStatDescription = value;
    }

    /// <summary>
    /// A value of MaritalStatus.
    /// </summary>
    [JsonPropertyName("maritalStatus")]
    public CodeValue MaritalStatus
    {
      get => maritalStatus ??= new();
      set => maritalStatus = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of ComnLink.
    /// </summary>
    [JsonPropertyName("comnLink")]
    public CsePersonsWorkSet ComnLink
    {
      get => comnLink ??= new();
      set => comnLink = value;
    }

    /// <summary>
    /// A value of ProgCodeDescription.
    /// </summary>
    [JsonPropertyName("progCodeDescription")]
    public CodeValue ProgCodeDescription
    {
      get => progCodeDescription ??= new();
      set => progCodeDescription = value;
    }

    /// <summary>
    /// A value of ArMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("arMultiCasesPrompt")]
    public Common ArMultiCasesPrompt
    {
      get => arMultiCasesPrompt ??= new();
      set => arMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ApMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("apMultiCasesPrompt")]
    public Common ApMultiCasesPrompt
    {
      get => apMultiCasesPrompt ??= new();
      set => apMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ApAltOccur.
    /// </summary>
    [JsonPropertyName("apAltOccur")]
    public Common ApAltOccur
    {
      get => apAltOccur ??= new();
      set => apAltOccur = value;
    }

    /// <summary>
    /// A value of ArAltOccur.
    /// </summary>
    [JsonPropertyName("arAltOccur")]
    public Common ArAltOccur
    {
      get => arAltOccur ??= new();
      set => arAltOccur = value;
    }

    /// <summary>
    /// A value of ArMultiCases.
    /// </summary>
    [JsonPropertyName("arMultiCases")]
    public Common ArMultiCases
    {
      get => arMultiCases ??= new();
      set => arMultiCases = value;
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
    /// A value of ArOtherCases.
    /// </summary>
    [JsonPropertyName("arOtherCases")]
    public Common ArOtherCases
    {
      get => arOtherCases ??= new();
      set => arOtherCases = value;
    }

    /// <summary>
    /// A value of ApOtherCases.
    /// </summary>
    [JsonPropertyName("apOtherCases")]
    public Common ApOtherCases
    {
      get => apOtherCases ??= new();
      set => apOtherCases = value;
    }

    /// <summary>
    /// A value of AltSsnAlias.
    /// </summary>
    [JsonPropertyName("altSsnAlias")]
    public TextWorkArea AltSsnAlias
    {
      get => altSsnAlias ??= new();
      set => altSsnAlias = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of OtherChildren.
    /// </summary>
    [JsonPropertyName("otherChildren")]
    public Common OtherChildren
    {
      get => otherChildren ??= new();
      set => otherChildren = value;
    }

    /// <summary>
    /// A value of ApMultiCases.
    /// </summary>
    [JsonPropertyName("apMultiCases")]
    public Common ApMultiCases
    {
      get => apMultiCases ??= new();
      set => apMultiCases = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of ApClient.
    /// </summary>
    [JsonPropertyName("apClient")]
    public CsePerson ApClient
    {
      get => apClient ??= new();
      set => apClient = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CaseCloseRsn.
    /// </summary>
    [JsonPropertyName("caseCloseRsn")]
    public CodeValue CaseCloseRsn
    {
      get => caseCloseRsn ??= new();
      set => caseCloseRsn = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
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

    private CsePersonsWorkSet fromComp;
    private CsePersonsWorkSet ch;
    private CaseRole arCaseRole;
    private CodeValue maritalStatDescription;
    private CodeValue maritalStatus;
    private PersonPrivateAttorney personPrivateAttorney;
    private FplsLocateRequest fplsLocateRequest;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arMultiCasesPrompt;
    private Common apMultiCasesPrompt;
    private Common apAltOccur;
    private Common arAltOccur;
    private Common arMultiCases;
    private Office office;
    private Common arOtherCases;
    private Common apOtherCases;
    private TextWorkArea altSsnAlias;
    private Standard standard;
    private Common multipleAps;
    private Common uci;
    private Common military;
    private Common incarceration;
    private Common bankruptcy;
    private Common otherChildren;
    private Common apMultiCases;
    private CaseRole apCaseRole;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson apClient;
    private CsePersonAddress apCsePersonAddress;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private NextTranInfo hidden;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private ServiceProvider serviceProvider;
    private CodeValue caseCloseRsn;
    private Case1 case1;
    private Case1 next;
    private Security2 security;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ZdelExportCh.
    /// </summary>
    [JsonPropertyName("zdelExportCh")]
    public CsePersonsWorkSet ZdelExportCh
    {
      get => zdelExportCh ??= new();
      set => zdelExportCh = value;
    }

    /// <summary>
    /// A value of ZdelExportAr.
    /// </summary>
    [JsonPropertyName("zdelExportAr")]
    public CaseRole ZdelExportAr
    {
      get => zdelExportAr ??= new();
      set => zdelExportAr = value;
    }

    /// <summary>
    /// A value of MaritalStatDescription.
    /// </summary>
    [JsonPropertyName("maritalStatDescription")]
    public CodeValue MaritalStatDescription
    {
      get => maritalStatDescription ??= new();
      set => maritalStatDescription = value;
    }

    /// <summary>
    /// A value of ZdelExportMaritalStatus.
    /// </summary>
    [JsonPropertyName("zdelExportMaritalStatus")]
    public CodeValue ZdelExportMaritalStatus
    {
      get => zdelExportMaritalStatus ??= new();
      set => zdelExportMaritalStatus = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of ComnLink.
    /// </summary>
    [JsonPropertyName("comnLink")]
    public CsePersonsWorkSet ComnLink
    {
      get => comnLink ??= new();
      set => comnLink = value;
    }

    /// <summary>
    /// A value of ProgCodeDescription.
    /// </summary>
    [JsonPropertyName("progCodeDescription")]
    public CodeValue ProgCodeDescription
    {
      get => progCodeDescription ??= new();
      set => progCodeDescription = value;
    }

    /// <summary>
    /// A value of ArMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("arMultiCasesPrompt")]
    public Common ArMultiCasesPrompt
    {
      get => arMultiCasesPrompt ??= new();
      set => arMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ApMultiCasesPrompt.
    /// </summary>
    [JsonPropertyName("apMultiCasesPrompt")]
    public Common ApMultiCasesPrompt
    {
      get => apMultiCasesPrompt ??= new();
      set => apMultiCasesPrompt = value;
    }

    /// <summary>
    /// A value of ZdelExportApAltOccur.
    /// </summary>
    [JsonPropertyName("zdelExportApAltOccur")]
    public Common ZdelExportApAltOccur
    {
      get => zdelExportApAltOccur ??= new();
      set => zdelExportApAltOccur = value;
    }

    /// <summary>
    /// A value of ZdelExportArAltOccur.
    /// </summary>
    [JsonPropertyName("zdelExportArAltOccur")]
    public Common ZdelExportArAltOccur
    {
      get => zdelExportArAltOccur ??= new();
      set => zdelExportArAltOccur = value;
    }

    /// <summary>
    /// A value of ArMultiCases.
    /// </summary>
    [JsonPropertyName("arMultiCases")]
    public Common ArMultiCases
    {
      get => arMultiCases ??= new();
      set => arMultiCases = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ZdelExportArOtherCases.
    /// </summary>
    [JsonPropertyName("zdelExportArOtherCases")]
    public Common ZdelExportArOtherCases
    {
      get => zdelExportArOtherCases ??= new();
      set => zdelExportArOtherCases = value;
    }

    /// <summary>
    /// A value of ZdelExportApOtherCases.
    /// </summary>
    [JsonPropertyName("zdelExportApOtherCases")]
    public Common ZdelExportApOtherCases
    {
      get => zdelExportApOtherCases ??= new();
      set => zdelExportApOtherCases = value;
    }

    /// <summary>
    /// A value of AltSsnAlias.
    /// </summary>
    [JsonPropertyName("altSsnAlias")]
    public TextWorkArea AltSsnAlias
    {
      get => altSsnAlias ??= new();
      set => altSsnAlias = value;
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
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of OtherChilderen.
    /// </summary>
    [JsonPropertyName("otherChilderen")]
    public Common OtherChilderen
    {
      get => otherChilderen ??= new();
      set => otherChilderen = value;
    }

    /// <summary>
    /// A value of ApMultiCases.
    /// </summary>
    [JsonPropertyName("apMultiCases")]
    public Common ApMultiCases
    {
      get => apMultiCases ??= new();
      set => apMultiCases = value;
    }

    /// <summary>
    /// A value of ZdelCaseRole.
    /// </summary>
    [JsonPropertyName("zdelCaseRole")]
    public CaseRole ZdelCaseRole
    {
      get => zdelCaseRole ??= new();
      set => zdelCaseRole = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of ApClient.
    /// </summary>
    [JsonPropertyName("apClient")]
    public CsePerson ApClient
    {
      get => apClient ??= new();
      set => apClient = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of CaseCloseRsn.
    /// </summary>
    [JsonPropertyName("caseCloseRsn")]
    public CodeValue CaseCloseRsn
    {
      get => caseCloseRsn ??= new();
      set => caseCloseRsn = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of ZdelSecurity.
    /// </summary>
    [JsonPropertyName("zdelSecurity")]
    public Security2 ZdelSecurity
    {
      get => zdelSecurity ??= new();
      set => zdelSecurity = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private CsePersonsWorkSet zdelExportCh;
    private CaseRole zdelExportAr;
    private CodeValue maritalStatDescription;
    private CodeValue zdelExportMaritalStatus;
    private PersonPrivateAttorney personPrivateAttorney;
    private FplsLocateRequest fplsLocateRequest;
    private CsePersonsWorkSet comnLink;
    private CodeValue progCodeDescription;
    private Common arMultiCasesPrompt;
    private Common apMultiCasesPrompt;
    private Common zdelExportApAltOccur;
    private Common zdelExportArAltOccur;
    private Common arMultiCases;
    private CaseRole apCaseRole;
    private Common zdelExportArOtherCases;
    private Common zdelExportApOtherCases;
    private TextWorkArea altSsnAlias;
    private Common multipleAps;
    private Common uci;
    private Common military;
    private Common incarceration;
    private Common bankruptcy;
    private Common otherChilderen;
    private Common apMultiCases;
    private CaseRole zdelCaseRole;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson apClient;
    private CsePersonAddress apCsePersonAddress;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apPrompt;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet ar;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private Office office;
    private ServiceProvider serviceProvider;
    private CodeValue caseCloseRsn;
    private Case1 case1;
    private Case1 next;
    private Security2 zdelSecurity;
    private Standard standard;
    private NextTranInfo hidden;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ApOpen.
    /// </summary>
    [JsonPropertyName("apOpen")]
    public Common ApOpen
    {
      get => apOpen ??= new();
      set => apOpen = value;
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
    /// A value of ApAltOccur.
    /// </summary>
    [JsonPropertyName("apAltOccur")]
    public Common ApAltOccur
    {
      get => apAltOccur ??= new();
      set => apAltOccur = value;
    }

    /// <summary>
    /// A value of ArAltOccur.
    /// </summary>
    [JsonPropertyName("arAltOccur")]
    public Common ArAltOccur
    {
      get => arAltOccur ??= new();
      set => arAltOccur = value;
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
    /// A value of NullCase.
    /// </summary>
    [JsonPropertyName("nullCase")]
    public Case1 NullCase
    {
      get => nullCase ??= new();
      set => nullCase = value;
    }

    /// <summary>
    /// A value of NullCsePerson.
    /// </summary>
    [JsonPropertyName("nullCsePerson")]
    public CsePerson NullCsePerson
    {
      get => nullCsePerson ??= new();
      set => nullCsePerson = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullCsePersonAddress.
    /// </summary>
    [JsonPropertyName("nullCsePersonAddress")]
    public CsePersonAddress NullCsePersonAddress
    {
      get => nullCsePersonAddress ??= new();
      set => nullCsePersonAddress = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of ZdelLocalMultiArCases.
    /// </summary>
    [JsonPropertyName("zdelLocalMultiArCases")]
    public Common ZdelLocalMultiArCases
    {
      get => zdelLocalMultiArCases ??= new();
      set => zdelLocalMultiArCases = value;
    }

    /// <summary>
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
    }

    /// <summary>
    /// A value of NoAps.
    /// </summary>
    [JsonPropertyName("noAps")]
    public Common NoAps
    {
      get => noAps ??= new();
      set => noAps = value;
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

    private Common apOpen;
    private Common caseOpen;
    private Common apAltOccur;
    private Common arAltOccur;
    private Common multipleAps;
    private Case1 nullCase;
    private CsePerson nullCsePerson;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private CsePersonAddress nullCsePersonAddress;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private Common zdelLocalMultiArCases;
    private Common invalid;
    private Common noAps;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseRole child;
    private HealthInsuranceViability healthInsuranceViability;
    private PersonPrivateAttorney personPrivateAttorney;
    private CsePersonAddress csePersonAddress;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
