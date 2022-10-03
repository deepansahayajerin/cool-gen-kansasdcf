// Program: OE_FIDM_FINANCIAL_DATA_MATCH, ID: 1902618849, model: 746.
// Short name: SWEFIDMP
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
/// A program: OE_FIDM_FINANCIAL_DATA_MATCH.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Provides for adding or changing information relating to the AP or AR's 
/// resources.  This is used in the Locate function and also in the
/// determination of support requirements.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFidmFinancialDataMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FIDM_FINANCIAL_DATA_MATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFidmFinancialDataMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFidmFinancialDataMatch.
  /// </summary>
  public OeFidmFinancialDataMatch(IContext context, Import import, Export export)
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
    // ******************************
    // MAINTENANCE LOG
    // ******************************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // P.Phinney	05/10/00   Initial Code
    // G.Vandy		05/25/00   Modify the data returned from the display cab
    // 			   when payee indicator='1'.
    // G.Vandy		07/18/00   PR 98226 - Modifications for the FIDM identifier 
    // change.
    // P.Phinney	03/25/02  I0140394  Add Account_Status_Indicator to FIDM Entity
    // DDupree       12/28/2005  WR00258947 FCR minor release
    // Added new matched account status and definition.
    // J. Harden   09/12/2017    CQ58517  Add a note line to the FIDM screen.
    // **************************** END MAINTENANCE LOG 
    // ****************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    // Move imports to exports.
    // ---------------------------------------------
    export.PassToResoResourceLocationAddress.Assign(
      import.ResourceLocationAddress);
    export.Hidden.Assign(import.Hidden);
    export.FinancialInstitutionDataMatch.Assign(
      import.FinancialInstitutionDataMatch);
    export.ReceivedKey.Assign(import.ReceivedKey);
    export.FidmAccountType.Description = import.FidmAccountType.Description;
    export.FidmAccountType2.Description = import.FidmAccountType2.Description;
    export.FidmResourceType.Description = import.FidmResourceType.Description;
    export.FidmBalanceIndicator.Description =
      import.FidmBalanceIndicator.Description;
    export.FidmTrustFundType.Description = import.FidmTrustFundType.Description;
    export.PayeeIndicator.Flag = import.PayeeIndicator.Flag;
    export.ForeignCountry.Flag = import.ForeignCountry.Flag;
    MoveCommon(import.PrimarySecondaryOwner1, export.PrimarySecondaryOwner1);
    MoveCommon(import.PrimarySecondaryOwner2, export.PrimarySecondaryOwner2);
    MoveCsePersonsWorkSet1(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.DisplayStatus1.Text30 = import.DisplayStatus1.Text30;
    export.DisplayStatus2.Text30 = import.DisplayStatus2.Text30;

    if (IsExitState("ACO_NE0000_RETURN"))
    {
      global.Command = "";
      ExitState = "BXP_RETURN";

      return;
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CsePersonNumber =
        export.FinancialInstitutionDataMatch.CsePersonNumber;
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
      ExitState = "OE_SELECT_FROM_FIDL";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
      export.HiddenPrevUserAction.Command = global.Command;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "ADDR") || Equal(global.Command, "RESO") || Equal
      (global.Command, "DISPLAY"))
    {
      export.PassCsePerson.Number = import.ReceivedKey.CsePersonNumber;
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        // * * * * * * * * * *
        // *  PF12  SIGNOFF
        // * * * * * * * * * *
        UseScCabSignoff();

        break;
      case "ADDR":
        // * * * * * * * * * *
        // *  PF15  ADDR
        // * * * * * * * * * *
        if (IsEmpty(export.FinancialInstitutionDataMatch.CsePersonNumber))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        export.CsePersonsWorkSet.Number =
          export.FinancialInstitutionDataMatch.CsePersonNumber;
        local.CountCases.Count = 0;
        local.Current.Date = Now().Date;

        foreach(var item in ReadCsePersonCaseRoleCase2())
        {
          ++local.CountCases.Count;
          export.PassCase.Number = entities.Case1.Number;
        }

        if (local.CountCases.Count > 1)
        {
          ExitState = "OE_MULTI_CASE_AP";

          return;
        }

        if (local.CountCases.Count == 0)
        {
          if (ReadCsePersonCaseRoleCase1())
          {
            export.PassCase.Number = entities.Case1.Number;
          }
        }

        export.PassToAddr.Street1 =
          export.FinancialInstitutionDataMatch.MatchedPayeeStreetAddress;
        export.PassToAddr.City =
          export.FinancialInstitutionDataMatch.MatchedPayeeCity ?? "";
        export.PassToAddr.State =
          export.FinancialInstitutionDataMatch.MatchedPayeeState ?? "";
        export.PassToAddr.ZipCode =
          export.FinancialInstitutionDataMatch.MatchedPayeeZipCode ?? "";
        export.PassToAddr.Zip4 =
          export.FinancialInstitutionDataMatch.MatchedPayeeZip4 ?? "";
        ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";

        break;
      case "RESL":
        // * * * * * * * * * *
        // *  PF17  RESL
        // * * * * * * * * * *
        if (IsEmpty(export.FinancialInstitutionDataMatch.CsePersonNumber))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        export.PassCsePerson.Number =
          export.FinancialInstitutionDataMatch.CsePersonNumber;
        ExitState = "ECO_LNK_TO_RESL_RESOURCE_LIST";

        break;
      case "RESO":
        // * * * * * * * * * *
        // *  PF18  RESO
        // * * * * * * * * * *
        if (IsEmpty(export.FinancialInstitutionDataMatch.CsePersonNumber))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        export.PassCsePerson.Number =
          export.FinancialInstitutionDataMatch.CsePersonNumber;
        export.PassToResoCsePersonResource.AccountNumber =
          export.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber;

        if (AsChar(export.FinancialInstitutionDataMatch.PayeeIndicator) == '2')
        {
          export.PassToResoCsePersonResource.CoOwnerName =
            export.FinancialInstitutionDataMatch.SecondPayeeName ?? "";
        }

        export.PassToResoCsePersonResource.Location =
          export.FinancialInstitutionDataMatch.InstitutionName ?? "";
        export.PassToResoCsePersonResource.Type1 =
          Substring(export.FidmAccountType2.Description, 1, 2);
        export.PassToResoCsePersonResource.Value =
          export.FinancialInstitutionDataMatch.AccountBalance.
            GetValueOrDefault();
        export.PassToResoResourceLocationAddress.Street1 =
          export.FinancialInstitutionDataMatch.InstitutionStreetAddress ?? "";
        export.PassToResoResourceLocationAddress.City =
          export.FinancialInstitutionDataMatch.InstitutionCity ?? "";
        export.PassToResoResourceLocationAddress.State =
          export.FinancialInstitutionDataMatch.InstitutionState ?? "";
        export.PassToResoResourceLocationAddress.ZipCode5 =
          export.FinancialInstitutionDataMatch.InstitutionZipCode ?? "";
        export.PassToResoResourceLocationAddress.ZipCode4 =
          export.FinancialInstitutionDataMatch.InstitutionZip4 ?? "";
        export.PassToResoResourceLocationAddress.EffectiveDate = Now().Date;
        ExitState = "ECO_LNK_TO_RESOURCE_LIST1";

        break;
      case "UPDATE":
        // CQ 58517 add a note line to the FIDM screen
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        // this date was change to display on the screen
        export.FinancialInstitutionDataMatch.MatchRunDate =
          export.ReceivedKey.MatchRunDate;
        UseFnUpdateFinDataMatch();

        // change date back from display on screen
        export.FinancialInstitutionDataMatch.MatchRunDate =
          import.FinancialInstitutionDataMatch.MatchRunDate;

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          var field =
            GetField(export.FinancialInstitutionDataMatch, "csePersonNumber");

          field.Error = true;
        }

        break;
      case "DISPLAY":
        // * * * * * * * * * *
        // * Initial entry from FIDL Screen
        // * PF2 has been DISABLED
        // * * * * * * * * * *
        UseOeDisplayFidmInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.FinancialInstitutionDataMatch.MatchRunDate =
            Substring(export.FinancialInstitutionDataMatch.MatchRunDate,
            FinancialInstitutionDataMatch.MatchRunDate_MaxLength, 5, 2) + Substring
            (export.FinancialInstitutionDataMatch.MatchRunDate,
            FinancialInstitutionDataMatch.MatchRunDate_MaxLength, 1, 4);

          // P.Phinney	03/25/02  I0140394  Add Account_Status_Indicator to FIDM 
          // Entity
          // **********************************************************************************************
          // 11/09/2005                      DDupree              WR00258947
          // For case '2' changed text to 'inactive' and added case '9' which 
          // has a text of 'not provided'
          // **********************************************************************************************
          if (Equal(export.FinancialInstitutionDataMatch.CreatedBy, "SWEEB425"))
          {
            export.DisplayStatus1.Text30 = "Account Status";

            switch(AsChar(export.FinancialInstitutionDataMatch.
              AccountStatusIndicator))
            {
              case '0':
                export.DisplayStatus2.Text30 = "OPEN";

                break;
              case '1':
                export.DisplayStatus2.Text30 = "CLOSED";

                break;
              case '2':
                export.DisplayStatus2.Text30 = "INACTIVE";

                break;
              case '9':
                export.DisplayStatus2.Text30 = "NOT PROVIDED";

                break;
              case ' ':
                export.DisplayStatus2.Text30 = "NOT PROVIDED";

                break;
              default:
                export.DisplayStatus2.Text30 = "";

                break;
            }
          }
          else
          {
            export.FinancialInstitutionDataMatch.AccountStatusIndicator = "";
            export.DisplayStatus1.Text30 = "";
            export.DisplayStatus2.Text30 = "";
          }

          ExitState = "ACO_NI0000_DISPLAY_OK";
        }
        else
        {
        }

        break;
      case "RETURN":
        // * * * * * * * * * *
        // *  PF9   Return to Linked program
        // * * * * * * * * * *
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "CMD08163":
        global.Command = "";
        ExitState = "BXP_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveFinancialInstitutionDataMatch1(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.InstitutionTin = source.InstitutionTin;
    target.MatchedPayeeAccountNumber = source.MatchedPayeeAccountNumber;
    target.MatchRunDate = source.MatchRunDate;
    target.MatchedPayeeSsn = source.MatchedPayeeSsn;
    target.MatchedPayeeName = source.MatchedPayeeName;
    target.MatchedPayeeStreetAddress = source.MatchedPayeeStreetAddress;
    target.MatchedPayeeCity = source.MatchedPayeeCity;
    target.MatchedPayeeState = source.MatchedPayeeState;
    target.MatchedPayeeZipCode = source.MatchedPayeeZipCode;
    target.MatchedPayeeZip4 = source.MatchedPayeeZip4;
    target.MatchedPayeeZip3 = source.MatchedPayeeZip3;
    target.PayeeForeignCountryIndicator = source.PayeeForeignCountryIndicator;
    target.MatchFlag = source.MatchFlag;
    target.AccountBalance = source.AccountBalance;
    target.AccountType = source.AccountType;
    target.TrustFundIndicator = source.TrustFundIndicator;
    target.AccountBalanceIndicator = source.AccountBalanceIndicator;
    target.DateOfBirth = source.DateOfBirth;
    target.PayeeIndicator = source.PayeeIndicator;
    target.AccountFullLegalTitle = source.AccountFullLegalTitle;
    target.PrimarySsn = source.PrimarySsn;
    target.SecondPayeeName = source.SecondPayeeName;
    target.SecondPayeeSsn = source.SecondPayeeSsn;
    target.MsfidmIndicator = source.MsfidmIndicator;
    target.InstitutionName = source.InstitutionName;
    target.InstitutionStreetAddress = source.InstitutionStreetAddress;
    target.InstitutionCity = source.InstitutionCity;
    target.InstitutionState = source.InstitutionState;
    target.InstitutionZipCode = source.InstitutionZipCode;
    target.InstitutionZip4 = source.InstitutionZip4;
    target.InstitutionZip3 = source.InstitutionZip3;
    target.SecondInstitutionName = source.SecondInstitutionName;
    target.TransmitterTin = source.TransmitterTin;
    target.TransmitterName = source.TransmitterName;
    target.TransmitterStreetAddress = source.TransmitterStreetAddress;
    target.TransmitterCity = source.TransmitterCity;
    target.TransmitterState = source.TransmitterState;
    target.TransmitterZipCode = source.TransmitterZipCode;
    target.TransmitterZip4 = source.TransmitterZip4;
    target.TransmitterZip3 = source.TransmitterZip3;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.AccountStatusIndicator = source.AccountStatusIndicator;
    target.Note = source.Note;
  }

  private static void MoveFinancialInstitutionDataMatch2(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.InstitutionTin = source.InstitutionTin;
    target.MatchedPayeeAccountNumber = source.MatchedPayeeAccountNumber;
    target.MatchRunDate = source.MatchRunDate;
    target.MatchedPayeeSsn = source.MatchedPayeeSsn;
    target.MatchedPayeeName = source.MatchedPayeeName;
    target.MatchedPayeeStreetAddress = source.MatchedPayeeStreetAddress;
    target.MatchedPayeeCity = source.MatchedPayeeCity;
    target.MatchedPayeeState = source.MatchedPayeeState;
    target.MatchedPayeeZipCode = source.MatchedPayeeZipCode;
    target.MatchedPayeeZip4 = source.MatchedPayeeZip4;
    target.AccountBalance = source.AccountBalance;
    target.AccountType = source.AccountType;
    target.DateOfBirth = source.DateOfBirth;
    target.AccountFullLegalTitle = source.AccountFullLegalTitle;
    target.PrimarySsn = source.PrimarySsn;
    target.SecondPayeeName = source.SecondPayeeName;
    target.SecondPayeeSsn = source.SecondPayeeSsn;
    target.InstitutionName = source.InstitutionName;
    target.InstitutionStreetAddress = source.InstitutionStreetAddress;
    target.InstitutionCity = source.InstitutionCity;
    target.InstitutionState = source.InstitutionState;
    target.InstitutionZipCode = source.InstitutionZipCode;
    target.InstitutionZip4 = source.InstitutionZip4;
    target.SecondInstitutionName = source.SecondInstitutionName;
    target.CreatedBy = source.CreatedBy;
    target.AccountStatusIndicator = source.AccountStatusIndicator;
    target.Note = source.Note;
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

  private void UseFnUpdateFinDataMatch()
  {
    var useImport = new FnUpdateFinDataMatch.Import();
    var useExport = new FnUpdateFinDataMatch.Export();

    MoveFinancialInstitutionDataMatch1(export.FinancialInstitutionDataMatch,
      useImport.FinancialInstitutionDataMatch);

    Call(FnUpdateFinDataMatch.Execute, useImport, useExport);

    export.FinancialInstitutionDataMatch.Assign(useExport.Exports);
  }

  private void UseOeDisplayFidmInfo()
  {
    var useImport = new OeDisplayFidmInfo.Import();
    var useExport = new OeDisplayFidmInfo.Export();

    useImport.FidmKey.Assign(import.ReceivedKey);

    Call(OeDisplayFidmInfo.Execute, useImport, useExport);

    export.FidmBalanceIndicator.Description =
      useExport.FidmBalanceIndicator.Description;
    export.FidmTrustFundType.Description =
      useExport.FidmTrustFundType.Description;
    export.FidmAccountType.Description = useExport.FidmAccountType.Description;
    export.FidmAccountType2.Description =
      useExport.FidmAccountType2.Description;
    export.FidmResourceType.Description =
      useExport.FidmResourceTypeDesc.Description;
    export.PayeeIndicator.Flag = useExport.PayeeIndicator.Flag;
    export.ForeignCountry.Flag = useExport.ForeignCountry.Flag;
    MoveCommon(useExport.PrimarySecondaryOwner1, export.PrimarySecondaryOwner1);
    MoveCommon(useExport.PrimarySecondaryOwner2, export.PrimarySecondaryOwner2);
    MoveFinancialInstitutionDataMatch2(useExport.
      PassFinancialInstitutionDataMatch, export.FinancialInstitutionDataMatch);
    MoveCsePersonsWorkSet2(useExport.PassCsePersonsWorkSet,
      export.CsePersonsWorkSet);
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

    useImport.Case1.Number = export.PassCase.Number;
    useImport.CsePerson.Number = export.PassCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCsePersonCaseRoleCase1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCsePersonCaseRoleCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CaseRole.CasNumber = db.GetString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 10);
        entities.Case1.Status = db.GetNullableString(reader, 11);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 12);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 13);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 14);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 17);
        entities.Case1.Note = db.GetNullableString(reader, 18);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRoleCase2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCsePersonCaseRoleCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CaseRole.CasNumber = db.GetString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 10);
        entities.Case1.Status = db.GetNullableString(reader, 11);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 12);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 13);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 14);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 17);
        entities.Case1.Note = db.GetNullableString(reader, 18);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
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
    /// A value of DisplayStatus2.
    /// </summary>
    [JsonPropertyName("displayStatus2")]
    public TextWorkArea DisplayStatus2
    {
      get => displayStatus2 ??= new();
      set => displayStatus2 = value;
    }

    /// <summary>
    /// A value of DisplayStatus1.
    /// </summary>
    [JsonPropertyName("displayStatus1")]
    public TextWorkArea DisplayStatus1
    {
      get => displayStatus1 ??= new();
      set => displayStatus1 = value;
    }

    /// <summary>
    /// A value of FidmBalanceIndicator.
    /// </summary>
    [JsonPropertyName("fidmBalanceIndicator")]
    public CodeValue FidmBalanceIndicator
    {
      get => fidmBalanceIndicator ??= new();
      set => fidmBalanceIndicator = value;
    }

    /// <summary>
    /// A value of FidmTrustFundType.
    /// </summary>
    [JsonPropertyName("fidmTrustFundType")]
    public CodeValue FidmTrustFundType
    {
      get => fidmTrustFundType ??= new();
      set => fidmTrustFundType = value;
    }

    /// <summary>
    /// A value of FidmAccountType.
    /// </summary>
    [JsonPropertyName("fidmAccountType")]
    public CodeValue FidmAccountType
    {
      get => fidmAccountType ??= new();
      set => fidmAccountType = value;
    }

    /// <summary>
    /// A value of FidmAccountType2.
    /// </summary>
    [JsonPropertyName("fidmAccountType2")]
    public CodeValue FidmAccountType2
    {
      get => fidmAccountType2 ??= new();
      set => fidmAccountType2 = value;
    }

    /// <summary>
    /// A value of FidmResourceType.
    /// </summary>
    [JsonPropertyName("fidmResourceType")]
    public CodeValue FidmResourceType
    {
      get => fidmResourceType ??= new();
      set => fidmResourceType = value;
    }

    /// <summary>
    /// A value of PayeeIndicator.
    /// </summary>
    [JsonPropertyName("payeeIndicator")]
    public Common PayeeIndicator
    {
      get => payeeIndicator ??= new();
      set => payeeIndicator = value;
    }

    /// <summary>
    /// A value of ForeignCountry.
    /// </summary>
    [JsonPropertyName("foreignCountry")]
    public Common ForeignCountry
    {
      get => foreignCountry ??= new();
      set => foreignCountry = value;
    }

    /// <summary>
    /// A value of PrimarySecondaryOwner1.
    /// </summary>
    [JsonPropertyName("primarySecondaryOwner1")]
    public Common PrimarySecondaryOwner1
    {
      get => primarySecondaryOwner1 ??= new();
      set => primarySecondaryOwner1 = value;
    }

    /// <summary>
    /// A value of PrimarySecondaryOwner2.
    /// </summary>
    [JsonPropertyName("primarySecondaryOwner2")]
    public Common PrimarySecondaryOwner2
    {
      get => primarySecondaryOwner2 ??= new();
      set => primarySecondaryOwner2 = value;
    }

    /// <summary>
    /// A value of ReceivedKey.
    /// </summary>
    [JsonPropertyName("receivedKey")]
    public FinancialInstitutionDataMatch ReceivedKey
    {
      get => receivedKey ??= new();
      set => receivedKey = value;
    }

    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
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
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    private TextWorkArea displayStatus2;
    private TextWorkArea displayStatus1;
    private CodeValue fidmBalanceIndicator;
    private CodeValue fidmTrustFundType;
    private CodeValue fidmAccountType;
    private CodeValue fidmAccountType2;
    private CodeValue fidmResourceType;
    private Common payeeIndicator;
    private Common foreignCountry;
    private Common primarySecondaryOwner1;
    private Common primarySecondaryOwner2;
    private FinancialInstitutionDataMatch receivedKey;
    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
    private Common hiddenDisplayPerformed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private NextTranInfo hidden;
    private Standard standard;
    private ResourceLocationAddress resourceLocationAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisplayStatus2.
    /// </summary>
    [JsonPropertyName("displayStatus2")]
    public TextWorkArea DisplayStatus2
    {
      get => displayStatus2 ??= new();
      set => displayStatus2 = value;
    }

    /// <summary>
    /// A value of DisplayStatus1.
    /// </summary>
    [JsonPropertyName("displayStatus1")]
    public TextWorkArea DisplayStatus1
    {
      get => displayStatus1 ??= new();
      set => displayStatus1 = value;
    }

    /// <summary>
    /// A value of PassToAddr.
    /// </summary>
    [JsonPropertyName("passToAddr")]
    public CsePersonAddress PassToAddr
    {
      get => passToAddr ??= new();
      set => passToAddr = value;
    }

    /// <summary>
    /// A value of FidmBalanceIndicator.
    /// </summary>
    [JsonPropertyName("fidmBalanceIndicator")]
    public CodeValue FidmBalanceIndicator
    {
      get => fidmBalanceIndicator ??= new();
      set => fidmBalanceIndicator = value;
    }

    /// <summary>
    /// A value of FidmTrustFundType.
    /// </summary>
    [JsonPropertyName("fidmTrustFundType")]
    public CodeValue FidmTrustFundType
    {
      get => fidmTrustFundType ??= new();
      set => fidmTrustFundType = value;
    }

    /// <summary>
    /// A value of FidmAccountType.
    /// </summary>
    [JsonPropertyName("fidmAccountType")]
    public CodeValue FidmAccountType
    {
      get => fidmAccountType ??= new();
      set => fidmAccountType = value;
    }

    /// <summary>
    /// A value of FidmAccountType2.
    /// </summary>
    [JsonPropertyName("fidmAccountType2")]
    public CodeValue FidmAccountType2
    {
      get => fidmAccountType2 ??= new();
      set => fidmAccountType2 = value;
    }

    /// <summary>
    /// A value of FidmResourceType.
    /// </summary>
    [JsonPropertyName("fidmResourceType")]
    public CodeValue FidmResourceType
    {
      get => fidmResourceType ??= new();
      set => fidmResourceType = value;
    }

    /// <summary>
    /// A value of PayeeIndicator.
    /// </summary>
    [JsonPropertyName("payeeIndicator")]
    public Common PayeeIndicator
    {
      get => payeeIndicator ??= new();
      set => payeeIndicator = value;
    }

    /// <summary>
    /// A value of ForeignCountry.
    /// </summary>
    [JsonPropertyName("foreignCountry")]
    public Common ForeignCountry
    {
      get => foreignCountry ??= new();
      set => foreignCountry = value;
    }

    /// <summary>
    /// A value of PrimarySecondaryOwner1.
    /// </summary>
    [JsonPropertyName("primarySecondaryOwner1")]
    public Common PrimarySecondaryOwner1
    {
      get => primarySecondaryOwner1 ??= new();
      set => primarySecondaryOwner1 = value;
    }

    /// <summary>
    /// A value of PrimarySecondaryOwner2.
    /// </summary>
    [JsonPropertyName("primarySecondaryOwner2")]
    public Common PrimarySecondaryOwner2
    {
      get => primarySecondaryOwner2 ??= new();
      set => primarySecondaryOwner2 = value;
    }

    /// <summary>
    /// A value of ReceivedKey.
    /// </summary>
    [JsonPropertyName("receivedKey")]
    public FinancialInstitutionDataMatch ReceivedKey
    {
      get => receivedKey ??= new();
      set => receivedKey = value;
    }

    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
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
    /// A value of PassCase.
    /// </summary>
    [JsonPropertyName("passCase")]
    public Case1 PassCase
    {
      get => passCase ??= new();
      set => passCase = value;
    }

    /// <summary>
    /// A value of PassToResoCsePersonResource.
    /// </summary>
    [JsonPropertyName("passToResoCsePersonResource")]
    public CsePersonResource PassToResoCsePersonResource
    {
      get => passToResoCsePersonResource ??= new();
      set => passToResoCsePersonResource = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public CsePersonVehicle Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of PassToResoResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("passToResoResourceLocationAddress")]
    public ResourceLocationAddress PassToResoResourceLocationAddress
    {
      get => passToResoResourceLocationAddress ??= new();
      set => passToResoResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PassCsePerson.
    /// </summary>
    [JsonPropertyName("passCsePerson")]
    public CsePerson PassCsePerson
    {
      get => passCsePerson ??= new();
      set => passCsePerson = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    private TextWorkArea displayStatus2;
    private TextWorkArea displayStatus1;
    private CsePersonAddress passToAddr;
    private CodeValue fidmBalanceIndicator;
    private CodeValue fidmTrustFundType;
    private CodeValue fidmAccountType;
    private CodeValue fidmAccountType2;
    private CodeValue fidmResourceType;
    private Common payeeIndicator;
    private Common foreignCountry;
    private Common primarySecondaryOwner1;
    private Common primarySecondaryOwner2;
    private FinancialInstitutionDataMatch receivedKey;
    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 passCase;
    private CsePersonResource passToResoCsePersonResource;
    private CsePersonVehicle dummy;
    private ResourceLocationAddress passToResoResourceLocationAddress;
    private Common hiddenPrevUserAction;
    private CsePerson passCsePerson;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePersonsWorkSet search;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CountCases.
    /// </summary>
    [JsonPropertyName("countCases")]
    public Common CountCases
    {
      get => countCases ??= new();
      set => countCases = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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

    private Common countCases;
    private NextTranInfo null1;
    private Common returnCode;
    private TextWorkArea zeroFill;
    private DateWorkArea nullDate;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private DateWorkArea dateWorkArea;
    private Common userAction;
    private DateWorkArea current;
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

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
