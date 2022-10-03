// Program: SI_IIIN_INCOMING_INTERSTATE_INFO, ID: 372503529, model: 746.
// Short name: SWEIIINP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IIIN_INCOMING_INTERSTATE_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIiinIncomingInterstateInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIIN_INCOMING_INTERSTATE_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIiinIncomingInterstateInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIiinIncomingInterstateInfo.
  /// </summary>
  public SiIiinIncomingInterstateInfo(IContext context, Import import,
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
    // Date	  Developer Name	Description
    //  	  Sid Chowdhary		Initial development
    // 11/04/96  G. Lofton - MTW	Add new security and removed old.
    // 1/31/97	  Sid Chowdhary		Completion.
    // 11/30/98  C Deghand             Added check for max date and added
    //                                 
    // State and Zip to the address
    // views
    //                                 
    // (PR # 41060).
    // 12/29/98  C Deghand             Changed the state field to be
    //                                 
    // enterable and added prompt.
    // 1/27/99   C Deghand             Removed OE Cab Check Case Member
    //                                 
    // and replaced it with SI Read
    // Case
    //                                 
    // Header Information.
    // 2/4/99    C Deghand             Moved the IF for the clear command
    //                                 
    // to the top so all the data would
    //                                 
    // clear from the screen.
    // 2/11/99   C Deghand             Added the case IIFI.
    // 2/18/99   C Deghand             Added an IF to clear the state
    //                                 
    // abbreviation when a new case
    // number
    //                                 
    // is displayed.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Allow the user to clear the screen
      // ---------------------------------------------
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveCase3(import.DisplayOnly, export.DisplayOnly);
    export.Hduplicate.DuplicateCaseIndicator =
      import.Hduplicate.DuplicateCaseIndicator;
    export.H.Assign(import.H);
    export.HotherState.StateAbbreviation = import.HotherState.StateAbbreviation;
    export.Next.Number = import.Next.Number;
    export.OspServiceProvider.LastName = import.OspServiceProvider.LastName;
    MoveOffice(import.OspOffice, export.OspOffice);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
    export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
    export.InterstateContact1.Assign(import.InterstateContact1);
    MoveInterstateContactAddress2(import.InterstateContactAddress,
      export.InterstateContactAddress);
    MoveInterstatePaymentAddress(import.InterstatePaymentAddress,
      export.InterstatePaymentAddress);
    MoveInterstateRequest1(import.InterstateRequest, export.InterstateRequest);
    export.AttachmentRcvd.SelectChar = import.AttachmentRcvd.SelectChar;
    export.CaseClosureDesc.Description = import.CaseClosureDesc.Description;
    export.InterstateContact2.FormattedNameText =
      import.InterstateContact2.FormattedNameText;
    export.OtherStateFips1.Assign(import.OtherState);
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    export.IreqStatePrompt.SelectChar = import.IreqStatePrompt.SelectChar;
    export.ForeignInfoMsg.Text12 = import.ForeignInfoMsg.Text12;

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!Equal(export.Next.Number, export.DisplayOnly.Number) && !
      IsEmpty(export.DisplayOnly.Number))
    {
      export.OtherStateFips1.StateAbbreviation = "";
      export.ApCsePersonsWorkSet.Number = "";
      export.ApCsePersonsWorkSet.FormattedName = "";
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!IsEmpty(export.Next.Number))
    {
      local.ZeroFill.Text10 = export.Next.Number;
      UseEabPadLeftWithZeros();
      export.DisplayOnly.Number = local.ZeroFill.Text10;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.RetcompInd.Flag = "N";

    if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.ApCsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
        MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
      }

      export.PromptPerson.SelectChar = "";
      local.RetcompInd.Flag = "Y";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETIREQ"))
    {
      if (!IsEmpty(import.OtherState.StateAbbreviation))
      {
        export.OtherStateFips1.StateAbbreviation =
          import.OtherState.StateAbbreviation;
      }
      else
      {
        export.OtherStateFips1.StateAbbreviation =
          export.HotherState.StateAbbreviation;
      }

      export.IreqStatePrompt.SelectChar = "";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Next.Number;
      export.Hidden.CsePersonNumberAp = export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

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
        export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // 12/29/98  C Deghand  Logic added for return from state code value table.
    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(export.IreqStatePrompt.SelectChar))
      {
        export.OtherStateFips1.StateAbbreviation =
          import.SelectedCodeValue.Cdvalue;
        export.IreqStatePrompt.SelectChar = "";

        if (!IsEmpty(export.OtherStateFips1.StateAbbreviation))
        {
          local.StateCommon.State = export.OtherStateFips1.StateAbbreviation;
          UseSiValidateStateFips();

          if (AsChar(local.FipsError.Flag) == 'Y')
          {
            var field = GetField(export.OtherStateFips1, "stateAbbreviation");

            field.Error = true;

            return;
          }
          else
          {
            export.InterstateRequest.OtherStateFips =
              export.OtherStateFips1.State;
            export.OtherStateFips2.CountyFips =
              NumberToString(export.OtherStateFips1.County, 3);
            export.OtherStateFips2.LocationFips =
              NumberToString(export.OtherStateFips1.Location, 2);
          }
        }
      }

      return;
    }

    if (!IsEmpty(export.OtherStateFips1.StateAbbreviation))
    {
      local.StateCodeValueCodeValue.Cdvalue =
        export.OtherStateFips1.StateAbbreviation;
      local.StateCodeValueCode.CodeName = "STATE CODE";
      UseCabValidateCodeValue();

      if (AsChar(local.StateCodeValueCommon.Flag) == 'N')
      {
        var field = GetField(export.OtherStateFips1, "stateAbbreviation");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_STATE_CODE";

        return;
      }
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
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
    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "IIOI":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_LNK_TO_IIOI";

        break;
      case "IREQ":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_LNK_TO_IREQ";

        break;
      case "IIFI":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_LNK_TO_IIFI";

        break;
      case "LIST":
        if (!IsEmpty(export.PromptPerson.SelectChar))
        {
          if (AsChar(export.PromptPerson.SelectChar) == 'S')
          {
            if (IsEmpty(export.Next.Number))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;

              ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";
            }
            else
            {
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
            }

            return;
          }
          else
          {
            var field = GetField(export.PromptPerson, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        // 12/29/98   C Deghand  Logic added for link to code value.
        if (!IsEmpty(export.IreqStatePrompt.SelectChar))
        {
          if (AsChar(export.IreqStatePrompt.SelectChar) == 'S')
          {
            export.Prompt.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
          else
          {
            var field = GetField(export.IreqStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        break;
      case "UPDATE":
        UseSiIiinUpdateIncomingIsInfo();

        break;
      case "DISPLAY":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          export.DisplayOnly.Assign(local.RefreshCase);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
          export.InterstateRequest.Assign(local.RefreshInterstateRequest);
          export.AttachmentRcvd.SelectChar = "";
          export.OtherStateFips1.StateAbbreviation = "";
          export.InterstateContact1.Assign(local.RefreshInterstateContact);
          MoveInterstateContactAddress1(local.RefreshInterstateContactAddress,
            export.InterstateContactAddress);
          export.InterstatePaymentAddress.Assign(
            local.RefreshInterstatePaymentAddress);
          export.InterstateContact2.FormattedNameText =
            local.RefreshContact.FormattedNameText;
          export.OtherStateFips1.Assign(local.RefreshFips);
          MoveOeWorkGroup(local.RefreshOeWorkGroup, export.OtherStateFips2);
          export.OtherStateFips1.StateAbbreviation = "";
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          local.ZeroFill.Text10 = export.Next.Number;
          UseEabPadLeftWithZeros();
          export.Next.Number = local.ZeroFill.Text10;

          // 1/27/99  C Deghand  Removed OE Cab Check Case Member and replaced 
          // it with Si Read Case Header.
          UseSiReadCaseHeaderInformation();
          local.Ap.FormattedName = export.ApCsePersonsWorkSet.FormattedName;
          local.Ar.FormattedName = export.Ar.FormattedName;

          if (AsChar(local.RetcompInd.Flag) == 'Y' && IsEmpty
            (export.ApCsePersonsWorkSet.FormattedName) && !
            IsEmpty(import.SelectedCsePersonsWorkSet.Number))
          {
            MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
              export.ApCsePersonsWorkSet);
          }
          else if (AsChar(local.RetcompInd.Flag) == 'Y')
          {
            MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
          }
          else if (AsChar(local.RetcompInd.Flag) == 'N' && Equal
            (import.DisplayOnly.Number, export.DisplayOnly.Number))
          {
            MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
          }

          if (AsChar(local.CaseOpen.Flag) == 'N')
          {
            export.DisplayOnly.Number = export.Next.Number;
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }

        UseSiReadOfficeOspHeader();

        if (IsExitState("DISPLAY_OK_FOR_CLOSED_CASE"))
        {
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(export.Next.Number, export.DisplayOnly.Number) && !
          IsEmpty(export.DisplayOnly.Number))
        {
          export.ApCsePersonsWorkSet.Number = "";
          export.ApCsePersonsWorkSet.FormattedName = "";
          export.OtherStateFips1.StateAbbreviation = "";
          export.IreqStatePrompt.SelectChar = "";
        }

        UseSiIiinReadIncomingIsInfo();

        if (AsChar(local.ForeignInd.Flag) == 'Y')
        {
          export.ForeignInfoMsg.Text12 = "Foreign Info";
        }
        else
        {
          export.ForeignInfoMsg.Text12 = "";
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          if (IsExitState("FIPS_NF"))
          {
            var field = GetField(export.OtherStateFips1, "stateAbbreviation");

            field.Error = true;
          }

          if (IsEmpty(export.OtherStateFips1.StateAbbreviation))
          {
            export.OtherStateFips1.Assign(local.RefreshFips);
            export.HotherState.StateAbbreviation =
              local.RefreshFips.StateAbbreviation;
            export.OtherStateFips2.CountyFips = "";
            export.OtherStateFips2.LocationFips = "";
          }

          // 12/17/98  C Deghand  Added this IF statement.
          if (IsExitState("CASE_NOT_INTERSTATE"))
          {
            return;
          }

          MoveInterstateRequest3(local.RefreshInterstateRequest, export.H);
          export.InterstateContact1.Assign(local.RefreshInterstateContact);
          MoveInterstateContactAddress1(local.RefreshInterstateContactAddress,
            export.InterstateContactAddress);
          export.InterstatePaymentAddress.Assign(
            local.RefreshInterstatePaymentAddress);
        }
        else
        {
          export.OtherStateFips2.CountyFips =
            NumberToString(export.OtherStateFips1.County, 3);
          export.OtherStateFips2.LocationFips =
            NumberToString(export.OtherStateFips1.Location, 2);
          export.HotherState.StateAbbreviation =
            export.OtherStateFips1.StateAbbreviation;
          MoveInterstateRequest3(export.InterstateRequest, export.H);

          if (IsEmpty(export.OtherStateFips1.StateAbbreviation))
          {
            export.OtherStateFips1.Assign(local.RefreshFips);
            export.HotherState.StateAbbreviation =
              local.RefreshFips.StateAbbreviation;
            export.OtherStateFips2.CountyFips = "";
            export.OtherStateFips2.LocationFips = "";
          }
        }

        if (IsExitState("SI0000_MULTIPLE_IR_EXISTS_FOR_AP"))
        {
          var field = GetField(export.OtherStateFips1, "stateAbbreviation");

          field.Error = true;

          return;
        }

        export.Hduplicate.DuplicateCaseIndicator =
          export.DisplayOnly.DuplicateCaseIndicator ?? "";

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          export.ApCsePersonsWorkSet.FormattedName = local.Ap.FormattedName;
          export.Ar.FormattedName = local.Ar.FormattedName;

          switch(local.InterstateRequestCount.Count)
          {
            case 0:
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";

              break;
            case 1:
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";

              break;
            default:
              if (!Equal(export.OtherStateFips1.StateAbbreviation,
                import.OtherState.StateAbbreviation) || Equal
                (export.OtherStateFips1.StateAbbreviation,
                import.OtherState.StateAbbreviation) && IsEmpty
                (import.OtherState.StateAbbreviation))
              {
                export.InterstateRequest.Assign(local.RefreshInterstateRequest);
                export.InterstateContact1.
                  Assign(local.RefreshInterstateContact);
                MoveInterstateContactAddress1(local.
                  RefreshInterstateContactAddress,
                  export.InterstateContactAddress);
                export.InterstatePaymentAddress.Assign(
                  local.RefreshInterstatePaymentAddress);
                export.OtherStateFips1.Assign(local.RefreshFips);
                MoveOeWorkGroup(local.RefreshOeWorkGroup, export.OtherStateFips2);
                  
                export.InterstateContact2.FormattedNameText =
                  local.RefreshContact.FormattedNameText;
                export.AttachmentRcvd.SelectChar = "";
              }

              ExitState = "SI0000_DISP_OK_CLOSED_INTERSTATE";

              break;
          }

          return;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "IATT":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }

        if (AsChar(export.AttachmentRcvd.SelectChar) != 'Y')
        {
          ExitState = "SI0000_NO_ATTACHMENT_EXISTS";

          return;
        }

        ExitState = "ECO_XFR_TO_IATT";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(export.InterstateRequest.OtherStateCaseClosureDate, local.Max.Date))
      
    {
      export.InterstateRequest.OtherStateCaseClosureDate = local.Zero.Date;
    }

    if (AsChar(export.DisplayOnly.DuplicateCaseIndicator) != 'Y')
    {
      export.DisplayOnly.DuplicateCaseIndicator = "N";
    }
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
    target.CseOpenDate = source.CseOpenDate;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.CseOpenDate = source.CseOpenDate;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInterstateContactAddress1(
    InterstateContactAddress source, InterstateContactAddress target)
  {
    target.LocationType = source.LocationType;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveInterstateContactAddress2(
    InterstateContactAddress source, InterstateContactAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstatePaymentAddress(
    InterstatePaymentAddress source, InterstatePaymentAddress target)
  {
    target.LocationType = source.LocationType;
    target.PayableToName = source.PayableToName;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Zip5 = source.Zip5;
    target.FipsCounty = source.FipsCounty;
    target.FipsState = source.FipsState;
    target.FipsLocation = source.FipsLocation;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.KsCaseInd = source.KsCaseInd;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
    target.OtherStateCaseClosureDate = source.OtherStateCaseClosureDate;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
  }

  private static void MoveInterstateRequest3(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveOeWorkGroup(OeWorkGroup source, OeWorkGroup target)
  {
    target.LocationFips = source.LocationFips;
    target.CountyFips = source.CountyFips;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
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

    useImport.Code.CodeName = local.StateCodeValueCode.CodeName;
    useImport.CodeValue.Cdvalue = local.StateCodeValueCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.StateCodeValueCommon.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
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

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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
    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiIiinReadIncomingIsInfo()
  {
    var useImport = new SiIiinReadIncomingIsInfo.Import();
    var useExport = new SiIiinReadIncomingIsInfo.Export();

    useImport.RetcompInd.Flag = local.RetcompInd.Flag;
    useImport.Case1.Number = export.Next.Number;
    useImport.OtherState.Assign(export.OtherStateFips1);
    MoveInterstateRequest2(export.InterstateRequest, useImport.InterstateRequest);
      
    MoveCsePersonsWorkSet(export.Ar, useImport.Ar);
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);

    Call(SiIiinReadIncomingIsInfo.Execute, useImport, useExport);

    local.ForeignInd.Flag = useExport.ForeignInd.Flag;
    local.InterstateRequestCount.Count = useExport.InterstateRequestCount.Count;
    MoveCase2(useExport.Case1, export.DisplayOnly);
    export.OtherStateFips1.Assign(useExport.OtherState);
    export.InterstateContact1.Assign(useExport.InterstateContact1);
    export.InterstateRequest.Assign(useExport.InterstateRequest);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    MoveInterstatePaymentAddress(useExport.InterstatePaymentAddress,
      export.InterstatePaymentAddress);
    MoveInterstateContactAddress2(useExport.InterstateContactAddress,
      export.InterstateContactAddress);
    export.CaseClosureDesc.Description = useExport.CaseClosureDesc.Description;
    export.AttachmentRcvd.SelectChar = useExport.AttachmentRcvd.SelectChar;
    export.InterstateContact2.FormattedNameText =
      useExport.InterstateContact2.FormattedNameText;
  }

  private void UseSiIiinUpdateIncomingIsInfo()
  {
    var useImport = new SiIiinUpdateIncomingIsInfo.Import();
    var useExport = new SiIiinUpdateIncomingIsInfo.Export();

    useImport.InterstateContact.Assign(export.InterstateContact1);
    useImport.InterstateRequest.Assign(export.InterstateRequest);

    Call(SiIiinUpdateIncomingIsInfo.Execute, useImport, useExport);

    export.InterstateContact1.Assign(useExport.InterstateContact);
    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.OspServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.OspOffice);
  }

  private void UseSiValidateStateFips()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    useImport.Common.State = local.StateCommon.State;

    Call(SiValidateStateFips.Execute, useImport, useExport);

    local.FipsError.Flag = useExport.Error.Flag;
    export.OtherStateFips1.Assign(useExport.Fips);
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
    /// A value of HotherState.
    /// </summary>
    [JsonPropertyName("hotherState")]
    public Fips HotherState
    {
      get => hotherState ??= new();
      set => hotherState = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public InterstateRequest H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of ForeignInfoMsg.
    /// </summary>
    [JsonPropertyName("foreignInfoMsg")]
    public WorkArea ForeignInfoMsg
    {
      get => foreignInfoMsg ??= new();
      set => foreignInfoMsg = value;
    }

    /// <summary>
    /// A value of Hduplicate.
    /// </summary>
    [JsonPropertyName("hduplicate")]
    public Case1 Hduplicate
    {
      get => hduplicate ??= new();
      set => hduplicate = value;
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
    /// A value of IreqStatePrompt.
    /// </summary>
    [JsonPropertyName("ireqStatePrompt")]
    public Common IreqStatePrompt
    {
      get => ireqStatePrompt ??= new();
      set => ireqStatePrompt = value;
    }

    /// <summary>
    /// A value of OtherStateFips.
    /// </summary>
    [JsonPropertyName("otherStateFips")]
    public OeWorkGroup OtherStateFips
    {
      get => otherStateFips ??= new();
      set => otherStateFips = value;
    }

    /// <summary>
    /// A value of CaseClosureDesc.
    /// </summary>
    [JsonPropertyName("caseClosureDesc")]
    public CodeValue CaseClosureDesc
    {
      get => caseClosureDesc ??= new();
      set => caseClosureDesc = value;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of InterstateContact1.
    /// </summary>
    [JsonPropertyName("interstateContact1")]
    public InterstateContact InterstateContact1
    {
      get => interstateContact1 ??= new();
      set => interstateContact1 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of AttachmentRcvd.
    /// </summary>
    [JsonPropertyName("attachmentRcvd")]
    public Common AttachmentRcvd
    {
      get => attachmentRcvd ??= new();
      set => attachmentRcvd = value;
    }

    /// <summary>
    /// A value of InterstateContact2.
    /// </summary>
    [JsonPropertyName("interstateContact2")]
    public OeWorkGroup InterstateContact2
    {
      get => interstateContact2 ??= new();
      set => interstateContact2 = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
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
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
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

    private Fips hotherState;
    private InterstateRequest h;
    private WorkArea foreignInfoMsg;
    private Case1 hduplicate;
    private CodeValue selectedCodeValue;
    private Common ireqStatePrompt;
    private OeWorkGroup otherStateFips;
    private CodeValue caseClosureDesc;
    private Fips otherState;
    private Case1 displayOnly;
    private Case1 next;
    private InterstateContact interstateContact1;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private Common attachmentRcvd;
    private OeWorkGroup interstateContact2;
    private Common promptPerson;
    private Standard standard;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private NextTranInfo hidden;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ForeignInfoMsg.
    /// </summary>
    [JsonPropertyName("foreignInfoMsg")]
    public WorkArea ForeignInfoMsg
    {
      get => foreignInfoMsg ??= new();
      set => foreignInfoMsg = value;
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
    /// A value of IreqStatePrompt.
    /// </summary>
    [JsonPropertyName("ireqStatePrompt")]
    public Common IreqStatePrompt
    {
      get => ireqStatePrompt ??= new();
      set => ireqStatePrompt = value;
    }

    /// <summary>
    /// A value of OtherStateOeWorkGroup.
    /// </summary>
    [JsonPropertyName("otherStateOeWorkGroup")]
    public OeWorkGroup OtherStateOeWorkGroup
    {
      get => otherStateOeWorkGroup ??= new();
      set => otherStateOeWorkGroup = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of OtherStateFips1.
    /// </summary>
    [JsonPropertyName("otherStateFips1")]
    public Fips OtherStateFips1
    {
      get => otherStateFips1 ??= new();
      set => otherStateFips1 = value;
    }

    /// <summary>
    /// A value of InterstateContact1.
    /// </summary>
    [JsonPropertyName("interstateContact1")]
    public InterstateContact InterstateContact1
    {
      get => interstateContact1 ??= new();
      set => interstateContact1 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of CaseClosureDesc.
    /// </summary>
    [JsonPropertyName("caseClosureDesc")]
    public CodeValue CaseClosureDesc
    {
      get => caseClosureDesc ??= new();
      set => caseClosureDesc = value;
    }

    /// <summary>
    /// A value of AttachmentRcvd.
    /// </summary>
    [JsonPropertyName("attachmentRcvd")]
    public Common AttachmentRcvd
    {
      get => attachmentRcvd ??= new();
      set => attachmentRcvd = value;
    }

    /// <summary>
    /// A value of InterstateContact2.
    /// </summary>
    [JsonPropertyName("interstateContact2")]
    public OeWorkGroup InterstateContact2
    {
      get => interstateContact2 ??= new();
      set => interstateContact2 = value;
    }

    /// <summary>
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of OtherStateFips2.
    /// </summary>
    [JsonPropertyName("otherStateFips2")]
    public OeWorkGroup OtherStateFips2
    {
      get => otherStateFips2 ??= new();
      set => otherStateFips2 = value;
    }

    /// <summary>
    /// A value of HotherState.
    /// </summary>
    [JsonPropertyName("hotherState")]
    public Fips HotherState
    {
      get => hotherState ??= new();
      set => hotherState = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public InterstateRequest H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of ContactPhone.
    /// </summary>
    [JsonPropertyName("contactPhone")]
    public InterstateWorkArea ContactPhone
    {
      get => contactPhone ??= new();
      set => contactPhone = value;
    }

    /// <summary>
    /// A value of Hduplicate.
    /// </summary>
    [JsonPropertyName("hduplicate")]
    public Case1 Hduplicate
    {
      get => hduplicate ??= new();
      set => hduplicate = value;
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

    private WorkArea foreignInfoMsg;
    private Code prompt;
    private Common ireqStatePrompt;
    private OeWorkGroup otherStateOeWorkGroup;
    private Case1 displayOnly;
    private Case1 next;
    private Fips otherStateFips1;
    private InterstateContact interstateContact1;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private CodeValue caseClosureDesc;
    private Common attachmentRcvd;
    private OeWorkGroup interstateContact2;
    private Common promptPerson;
    private Standard standard;
    private CsePerson apCsePerson;
    private NextTranInfo hidden;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private OeWorkGroup otherStateFips2;
    private Fips hotherState;
    private InterstateRequest h;
    private CodeValue country;
    private InterstateWorkArea contactPhone;
    private Case1 hduplicate;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of StateCodeValueCode.
    /// </summary>
    [JsonPropertyName("stateCodeValueCode")]
    public Code StateCodeValueCode
    {
      get => stateCodeValueCode ??= new();
      set => stateCodeValueCode = value;
    }

    /// <summary>
    /// A value of StateCodeValueCodeValue.
    /// </summary>
    [JsonPropertyName("stateCodeValueCodeValue")]
    public CodeValue StateCodeValueCodeValue
    {
      get => stateCodeValueCodeValue ??= new();
      set => stateCodeValueCodeValue = value;
    }

    /// <summary>
    /// A value of StateCodeValueCommon.
    /// </summary>
    [JsonPropertyName("stateCodeValueCommon")]
    public Common StateCodeValueCommon
    {
      get => stateCodeValueCommon ??= new();
      set => stateCodeValueCommon = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of RetcompInd.
    /// </summary>
    [JsonPropertyName("retcompInd")]
    public Common RetcompInd
    {
      get => retcompInd ??= new();
      set => retcompInd = value;
    }

    /// <summary>
    /// A value of ForeignInd.
    /// </summary>
    [JsonPropertyName("foreignInd")]
    public Common ForeignInd
    {
      get => foreignInd ??= new();
      set => foreignInd = value;
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
    /// A value of Validation.
    /// </summary>
    [JsonPropertyName("validation")]
    public CodeValue Validation
    {
      get => validation ??= new();
      set => validation = value;
    }

    /// <summary>
    /// A value of RefreshFips.
    /// </summary>
    [JsonPropertyName("refreshFips")]
    public Fips RefreshFips
    {
      get => refreshFips ??= new();
      set => refreshFips = value;
    }

    /// <summary>
    /// A value of RefreshOeWorkGroup.
    /// </summary>
    [JsonPropertyName("refreshOeWorkGroup")]
    public OeWorkGroup RefreshOeWorkGroup
    {
      get => refreshOeWorkGroup ??= new();
      set => refreshOeWorkGroup = value;
    }

    /// <summary>
    /// A value of StateCode.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public Code StateCode
    {
      get => stateCode ??= new();
      set => stateCode = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of RefreshContact.
    /// </summary>
    [JsonPropertyName("refreshContact")]
    public OeWorkGroup RefreshContact
    {
      get => refreshContact ??= new();
      set => refreshContact = value;
    }

    /// <summary>
    /// A value of RefreshInterstateContact.
    /// </summary>
    [JsonPropertyName("refreshInterstateContact")]
    public InterstateContact RefreshInterstateContact
    {
      get => refreshInterstateContact ??= new();
      set => refreshInterstateContact = value;
    }

    /// <summary>
    /// A value of RefreshInterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("refreshInterstatePaymentAddress")]
    public InterstatePaymentAddress RefreshInterstatePaymentAddress
    {
      get => refreshInterstatePaymentAddress ??= new();
      set => refreshInterstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of RefreshInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("refreshInterstateContactAddress")]
    public InterstateContactAddress RefreshInterstateContactAddress
    {
      get => refreshInterstateContactAddress ??= new();
      set => refreshInterstateContactAddress = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RefreshInterstateRequest.
    /// </summary>
    [JsonPropertyName("refreshInterstateRequest")]
    public InterstateRequest RefreshInterstateRequest
    {
      get => refreshInterstateRequest ??= new();
      set => refreshInterstateRequest = value;
    }

    /// <summary>
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of StateCommon.
    /// </summary>
    [JsonPropertyName("stateCommon")]
    public Common StateCommon
    {
      get => stateCommon ??= new();
      set => stateCommon = value;
    }

    /// <summary>
    /// A value of FipsError.
    /// </summary>
    [JsonPropertyName("fipsError")]
    public Common FipsError
    {
      get => fipsError ??= new();
      set => fipsError = value;
    }

    /// <summary>
    /// A value of RefreshPhone.
    /// </summary>
    [JsonPropertyName("refreshPhone")]
    public InterstateWorkArea RefreshPhone
    {
      get => refreshPhone ??= new();
      set => refreshPhone = value;
    }

    /// <summary>
    /// A value of InterstateRequestCount.
    /// </summary>
    [JsonPropertyName("interstateRequestCount")]
    public Common InterstateRequestCount
    {
      get => interstateRequestCount ??= new();
      set => interstateRequestCount = value;
    }

    private Code stateCodeValueCode;
    private CodeValue stateCodeValueCodeValue;
    private Common stateCodeValueCommon;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Common multipleAps;
    private Common retcompInd;
    private Common foreignInd;
    private Common caseOpen;
    private CodeValue validation;
    private Fips refreshFips;
    private OeWorkGroup refreshOeWorkGroup;
    private Code stateCode;
    private DateWorkArea max;
    private DateWorkArea zero;
    private OeWorkGroup refreshContact;
    private InterstateContact refreshInterstateContact;
    private InterstatePaymentAddress refreshInterstatePaymentAddress;
    private InterstateContactAddress refreshInterstateContactAddress;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private InterstateRequest refreshInterstateRequest;
    private Case1 refreshCase;
    private Common error;
    private TextWorkArea zeroFill;
    private Common stateCommon;
    private Common fipsError;
    private InterstateWorkArea refreshPhone;
    private Common interstateRequestCount;
  }
#endregion
}
