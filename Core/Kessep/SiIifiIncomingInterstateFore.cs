// Program: SI_IIFI_INCOMING_INTERSTATE_FORE, ID: 372499937, model: 746.
// Short name: SWEIIFIP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IIFI_INCOMING_INTERSTATE_FORE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIifiIncomingInterstateFore: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIFI_INCOMING_INTERSTATE_FORE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIifiIncomingInterstateFore(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIifiIncomingInterstateFore.
  /// </summary>
  public SiIifiIncomingInterstateFore(IContext context, Import import,
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
    // 02/05/99  Curtis Scroggins	Initial development
    // ------------------------------------------------------------
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // SWSRKXD PR149011 08/16/2002
    // - Fix screen Help Id.
    // *********************************************
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
    export.Next.Number = import.Next.Number;
    export.OspServiceProvider.LastName = import.OspServiceProvider.LastName;
    MoveOffice(import.OspOffice, export.OspOffice);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
    export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
    export.InterstateContact1.Assign(import.InterstateContact1);
    MoveInterstateContactAddress2(import.InterstateContactAddress,
      export.InterstateContactAddress);
    MoveInterstatePaymentAddress2(import.InterstatePaymentAddress,
      export.InterstatePaymentAddress);
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.AttachmentRcvd.SelectChar = import.AttachmentRcvd.SelectChar;
    export.CaseClosureDesc.Description = import.CaseClosureDesc.Description;
    export.InterstateContact2.FormattedNameText =
      import.InterstateContact2.FormattedNameText;
    export.OtherState.Assign(import.OtherState);
    export.PromptPerson.SelectChar = import.PromptPerson.SelectChar;
    export.OtherState.Assign(import.OtherState);
    export.IreqStatePrompt.SelectChar = import.IreqStatePrompt.SelectChar;
    export.IreqCountryPrompt.SelectChar = import.IreqCountryPrompt.SelectChar;
    export.StateInfoMsg.Text13 = import.StateInfoMsg.Text13;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    export.Case1.Number = import.Case1.Number;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!IsEmpty(export.Next.Number))
    {
      if (!Equal(export.Next.Number, export.DisplayOnly.Number))
      {
        export.InterstateRequest.Country = "";
      }

      local.ZeroFill.Text10 = export.Next.Number;
      UseEabPadLeftWithZeros2();
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
        export.InterstateRequest.Country = import.OtherState.StateAbbreviation;
      }

      export.IreqCountryPrompt.SelectChar = "";
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

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(export.IreqCountryPrompt.SelectChar))
      {
        export.InterstateRequest.Country = import.SelectedCodeValue.Cdvalue;
        export.IreqCountryPrompt.SelectChar = "";
      }

      return;
    }

    if (!IsEmpty(export.InterstateRequest.Country))
    {
      local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces(10);
      local.Code.CodeName = "COUNTRY CODE";
      UseCabValidateCodeValue();

      if (AsChar(local.Common.Flag) == 'N')
      {
        var field = GetField(export.InterstateRequest, "country");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE";

        return;
      }
    }

    // ---------------------------------------------
    // Check for multiple prompts selected.
    // ---------------------------------------------
    if (!IsEmpty(export.IreqCountryPrompt.SelectChar) && !
      IsEmpty(export.PromptPerson.SelectChar))
    {
      var field1 = GetField(export.IreqCountryPrompt, "selectChar");

      field1.Error = true;

      var field2 = GetField(export.PromptPerson, "selectChar");

      field2.Error = true;

      ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "DISPLAY"))
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
      case "UPDATE":
        UseSiIifiUpdateIncomingFoInfo();

        return;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        return;
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

        ExitState = "ECO_XFR_TO_IIOI";

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
      case "IIIN":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_XFR_TO_IIIN";

        break;
      case "IIMC":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_XFR_TO_SI_IIMC";

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

        if (!IsEmpty(export.IreqCountryPrompt.SelectChar))
        {
          if (AsChar(export.IreqCountryPrompt.SelectChar) == 'S')
          {
            export.Prompt.CodeName = "COUNTRY CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
          else
          {
            var field = GetField(export.IreqCountryPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        break;
      case "DISPLAY":
        if (!IsEmpty(export.Next.Number))
        {
          local.TextWorkArea.Text10 = export.Next.Number;
          UseEabPadLeftWithZeros1();
          export.Next.Number = local.TextWorkArea.Text10;
        }

        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          export.DisplayOnly.Assign(local.RefreshCase);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
          export.InterstateRequest.Assign(local.RefreshInterstateRequest);
          export.InterstateRequest.Country = "";
          export.AttachmentRcvd.SelectChar = "";
          export.InterstateContact1.Assign(local.RefreshInterstateContact);
          MoveInterstateContactAddress1(local.RefreshInterstateContactAddress,
            export.InterstateContactAddress);
          export.InterstatePaymentAddress.Assign(
            local.RefreshInterstatePaymentAddress);
          export.InterstateContact2.FormattedNameText =
            local.RefreshContact.FormattedNameText;
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          local.ZeroFill.Text10 = export.Next.Number;
          UseEabPadLeftWithZeros2();
          export.Next.Number = local.ZeroFill.Text10;
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
          export.InterstateRequest.Country = "";
          export.IreqCountryPrompt.SelectChar = "";
        }

        UseSiIifiReadIncomingForInfo();

        if (AsChar(local.StateInd.Flag) == 'Y')
        {
          export.StateInfoMsg.Text13 = "State Info";

          var field = GetField(export.StateInfoMsg, "text13");

          field.Color = "yellow";
          field.Protected = true;
        }
        else
        {
          export.StateInfoMsg.Text13 = "";
        }

        if (AsChar(local.ForeignInd.Flag) == 'N')
        {
          export.InterstateContact1.Assign(local.RefreshInterstateContact);
          export.InterstateRequest.Assign(local.RefreshInterstateRequest);
          export.InterstateContact2.FormattedNameText =
            local.RefreshContact.FormattedNameText;
          MoveOeWorkGroup(local.RefreshOeWorkGroup, export.OtherStateFips);
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          if (IsExitState("CASE_NOT_INTERSTATE"))
          {
            return;
          }

          MoveInterstateRequest2(local.RefreshInterstateRequest, export.H);
          export.InterstateContact1.Assign(local.RefreshInterstateContact);
          MoveInterstateContactAddress1(local.RefreshInterstateContactAddress,
            export.InterstateContactAddress);
          export.InterstatePaymentAddress.Assign(
            local.RefreshInterstatePaymentAddress);
          MoveInterstateWorkArea(local.RefreshPhone, export.ContactPhone);
        }
        else
        {
          MoveInterstateRequest2(export.InterstateRequest, export.H);
        }

        if (!IsEmpty(export.InterstateContactAddress.State) && IsEmpty
          (export.InterstateContactAddress.Country))
        {
          MoveInterstateContactAddress1(local.RefreshInterstateContactAddress,
            export.InterstateContactAddress);
        }

        if (!IsEmpty(export.InterstatePaymentAddress.State) && IsEmpty
          (export.InterstatePaymentAddress.Country))
        {
          export.InterstatePaymentAddress.Assign(
            local.RefreshInterstatePaymentAddress);
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
              if (!Equal(import.InterstateRequest.Country,
                export.InterstateRequest.Country))
              {
                export.InterstateRequest.Assign(local.RefreshInterstateRequest);
                export.InterstateContact1.
                  Assign(local.RefreshInterstateContact);
                MoveInterstateContactAddress1(local.
                  RefreshInterstateContactAddress,
                  export.InterstateContactAddress);
                export.InterstatePaymentAddress.Assign(
                  local.RefreshInterstatePaymentAddress);
                export.OtherState.Assign(local.RefreshFips);
                MoveOeWorkGroup(local.RefreshOeWorkGroup, export.OtherStateFips);
                  
                export.InterstateContact2.FormattedNameText =
                  local.RefreshContact.FormattedNameText;
                export.AttachmentRcvd.SelectChar = "";
                ExitState = "SI0000_DISP_OK_CLOSED_INTERSTATE";
              }

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
        // ---------------------------------------------
        // Commented this code out, per the SME's directions. CLS. 04/12/99.
        // ---------------------------------------------
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
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstatePaymentAddress1(
    InterstatePaymentAddress source, InterstatePaymentAddress target)
  {
    target.LocationType = source.LocationType;
    target.PayableToName = source.PayableToName;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Zip5 = source.Zip5;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstatePaymentAddress2(
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
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
  }

  private static void MoveInterstateWorkArea(InterstateWorkArea source,
    InterstateWorkArea target)
  {
    target.PhoneNumber = source.PhoneNumber;
    target.PhoneAreaCode = source.PhoneAreaCode;
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

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Count = useExport.ReturnCode.Count;
    local.Common.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabPadLeftWithZeros1()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabPadLeftWithZeros2()
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

  private void UseSiIifiReadIncomingForInfo()
  {
    var useImport = new SiIifiReadIncomingForInfo.Import();
    var useExport = new SiIifiReadIncomingForInfo.Export();

    useImport.RetcompInd.Flag = local.RetcompInd.Flag;
    MoveCsePersonsWorkSet(export.Ar, useImport.Ar);
    useImport.OtherState.Assign(export.OtherState);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.Case1.Number = export.Next.Number;
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);

    Call(SiIifiReadIncomingForInfo.Execute, useImport, useExport);

    local.InterstateRequestCount.Count = useExport.InterstateRequestCount.Count;
    local.StateInd.Flag = useExport.StateInd.Flag;
    local.ForeignInd.Flag = useExport.ForeignInd.Flag;
    MoveCase2(useExport.Case1, export.DisplayOnly);
    export.OtherState.Assign(useExport.OtherState);
    export.InterstateContact1.Assign(useExport.InterstateContact1);
    export.InterstateRequest.Assign(useExport.InterstateRequest);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    MoveInterstatePaymentAddress1(useExport.InterstatePaymentAddress,
      export.InterstatePaymentAddress);
    MoveInterstateContactAddress2(useExport.InterstateContactAddress,
      export.InterstateContactAddress);
    export.CaseClosureDesc.Description = useExport.CaseClosureDesc.Description;
    export.AttachmentRcvd.SelectChar = useExport.AttachmentRcvd.SelectChar;
    export.InterstateContact2.FormattedNameText =
      useExport.InterstateContact2.FormattedNameText;
  }

  private void UseSiIifiUpdateIncomingFoInfo()
  {
    var useImport = new SiIifiUpdateIncomingFoInfo.Import();
    var useExport = new SiIifiUpdateIncomingFoInfo.Export();

    useImport.InterstateContact.Assign(export.InterstateContact1);
    useImport.InterstateRequest.Assign(export.InterstateRequest);

    Call(SiIifiUpdateIncomingFoInfo.Execute, useImport, useExport);

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
    /// A value of StateInfoMsg.
    /// </summary>
    [JsonPropertyName("stateInfoMsg")]
    public WorkArea StateInfoMsg
    {
      get => stateInfoMsg ??= new();
      set => stateInfoMsg = value;
    }

    /// <summary>
    /// A value of IreqCountryPrompt.
    /// </summary>
    [JsonPropertyName("ireqCountryPrompt")]
    public Common IreqCountryPrompt
    {
      get => ireqCountryPrompt ??= new();
      set => ireqCountryPrompt = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of Hduplicate.
    /// </summary>
    [JsonPropertyName("hduplicate")]
    public Case1 Hduplicate
    {
      get => hduplicate ??= new();
      set => hduplicate = value;
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
    /// A value of InterstateContact1.
    /// </summary>
    [JsonPropertyName("interstateContact1")]
    public InterstateContact InterstateContact1
    {
      get => interstateContact1 ??= new();
      set => interstateContact1 = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
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
    /// A value of AttachmentRcvd.
    /// </summary>
    [JsonPropertyName("attachmentRcvd")]
    public Common AttachmentRcvd
    {
      get => attachmentRcvd ??= new();
      set => attachmentRcvd = value;
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
    /// A value of InterstateContact2.
    /// </summary>
    [JsonPropertyName("interstateContact2")]
    public OeWorkGroup InterstateContact2
    {
      get => interstateContact2 ??= new();
      set => interstateContact2 = value;
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
    /// A value of PromptPerson.
    /// </summary>
    [JsonPropertyName("promptPerson")]
    public Common PromptPerson
    {
      get => promptPerson ??= new();
      set => promptPerson = value;
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
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
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

    private WorkArea stateInfoMsg;
    private Common ireqCountryPrompt;
    private Case1 case1;
    private NextTranInfo hidden;
    private Case1 displayOnly;
    private Case1 hduplicate;
    private Case1 next;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private InterstateContact interstateContact1;
    private InterstateContactAddress interstateContactAddress;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateRequest interstateRequest;
    private Common attachmentRcvd;
    private CodeValue caseClosureDesc;
    private OeWorkGroup interstateContact2;
    private Fips otherState;
    private Common promptPerson;
    private Common ireqStatePrompt;
    private Standard standard;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private CodeValue selectedCodeValue;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of StateInfoMsg.
    /// </summary>
    [JsonPropertyName("stateInfoMsg")]
    public WorkArea StateInfoMsg
    {
      get => stateInfoMsg ??= new();
      set => stateInfoMsg = value;
    }

    /// <summary>
    /// A value of IreqCountryPrompt.
    /// </summary>
    [JsonPropertyName("ireqCountryPrompt")]
    public Common IreqCountryPrompt
    {
      get => ireqCountryPrompt ??= new();
      set => ireqCountryPrompt = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of Hduplicate.
    /// </summary>
    [JsonPropertyName("hduplicate")]
    public Case1 Hduplicate
    {
      get => hduplicate ??= new();
      set => hduplicate = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of CaseClosureDesc.
    /// </summary>
    [JsonPropertyName("caseClosureDesc")]
    public CodeValue CaseClosureDesc
    {
      get => caseClosureDesc ??= new();
      set => caseClosureDesc = value;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
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
    /// A value of IreqStatePrompt.
    /// </summary>
    [JsonPropertyName("ireqStatePrompt")]
    public Common IreqStatePrompt
    {
      get => ireqStatePrompt ??= new();
      set => ireqStatePrompt = value;
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
    /// A value of OtherStateFips.
    /// </summary>
    [JsonPropertyName("otherStateFips")]
    public OeWorkGroup OtherStateFips
    {
      get => otherStateFips ??= new();
      set => otherStateFips = value;
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
    /// A value of ContactPhone.
    /// </summary>
    [JsonPropertyName("contactPhone")]
    public InterstateWorkArea ContactPhone
    {
      get => contactPhone ??= new();
      set => contactPhone = value;
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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private WorkArea stateInfoMsg;
    private Common ireqCountryPrompt;
    private Case1 case1;
    private NextTranInfo hidden;
    private Case1 displayOnly;
    private Case1 hduplicate;
    private Case1 next;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson apCsePerson;
    private InterstateContact interstateContact1;
    private InterstateContactAddress interstateContactAddress;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateRequest interstateRequest;
    private Common attachmentRcvd;
    private CodeValue caseClosureDesc;
    private OeWorkGroup interstateContact2;
    private Fips otherState;
    private Common promptPerson;
    private Common ireqStatePrompt;
    private Standard standard;
    private OeWorkGroup otherStateFips;
    private Fips hotherState;
    private InterstateRequest h;
    private InterstateWorkArea contactPhone;
    private Code prompt;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of ForeignInd.
    /// </summary>
    [JsonPropertyName("foreignInd")]
    public Common ForeignInd
    {
      get => foreignInd ??= new();
      set => foreignInd = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of StateInd.
    /// </summary>
    [JsonPropertyName("stateInd")]
    public Common StateInd
    {
      get => stateInd ??= new();
      set => stateInd = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Common State
    {
      get => state ??= new();
      set => state = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of RefreshInterstateContact.
    /// </summary>
    [JsonPropertyName("refreshInterstateContact")]
    public InterstateContact RefreshInterstateContact
    {
      get => refreshInterstateContact ??= new();
      set => refreshInterstateContact = value;
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
    /// A value of RefreshInterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("refreshInterstatePaymentAddress")]
    public InterstatePaymentAddress RefreshInterstatePaymentAddress
    {
      get => refreshInterstatePaymentAddress ??= new();
      set => refreshInterstatePaymentAddress = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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

    private Common common;
    private CodeValue codeValue;
    private Code code;
    private Common foreignInd;
    private Common interstateRequestCount;
    private Common multipleAps;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Common stateInd;
    private Common retcompInd;
    private TextWorkArea textWorkArea;
    private DateWorkArea max;
    private TextWorkArea zeroFill;
    private Common state;
    private Common fipsError;
    private DateWorkArea zero;
    private Case1 refreshCase;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private InterstateRequest refreshInterstateRequest;
    private InterstateContact refreshInterstateContact;
    private InterstateContactAddress refreshInterstateContactAddress;
    private InterstatePaymentAddress refreshInterstatePaymentAddress;
    private OeWorkGroup refreshContact;
    private Fips refreshFips;
    private OeWorkGroup refreshOeWorkGroup;
    private Common caseOpen;
    private InterstateWorkArea refreshPhone;
  }
#endregion
}
