// Program: FN_UPDATE_FIN_DATA_MATCH, ID: 1902618467, model: 746.
// Short name: SWE01241
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_FIN_DATA_MATCH.
/// </summary>
[Serializable]
public partial class FnUpdateFinDataMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_FIN_DATA_MATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateFinDataMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateFinDataMatch.
  /// </summary>
  public FnUpdateFinDataMatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 09/19/2017       Joyce Harden           CQ58517         Add a note line 
    // to FIDM screen
    // ---------------------------------------------------------------------------------------
    export.Exports.Assign(import.FinancialInstitutionDataMatch);

    if (ReadFinancialInstitutionDataMatch())
    {
      try
      {
        UpdateFinancialInstitutionDataMatch();
        export.Exports.Assign(entities.FinancialInstitutionDataMatch);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FINANCIAL_INSTITUTION_DATA_MA_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FINANCIAL_INSTITUTION_DATA_MA_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FINANCIAL_INSTITUTION_DATA_MA_NF";
    }
  }

  private bool ReadFinancialInstitutionDataMatch()
  {
    entities.FinancialInstitutionDataMatch.Populated = false;

    return Read("ReadFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetString(
          command, "cseNumber",
          import.FinancialInstitutionDataMatch.CsePersonNumber);
        db.SetString(
          command, "institutionTin",
          import.FinancialInstitutionDataMatch.InstitutionTin);
        db.SetString(
          command, "matchPayAcctNum",
          import.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber);
        db.SetString(
          command, "matchRunDate",
          import.FinancialInstitutionDataMatch.MatchRunDate);
        db.SetString(
          command, "accountType",
          import.FinancialInstitutionDataMatch.AccountType);
      },
      (db, reader) =>
      {
        entities.FinancialInstitutionDataMatch.CsePersonNumber =
          db.GetString(reader, 0);
        entities.FinancialInstitutionDataMatch.InstitutionTin =
          db.GetString(reader, 1);
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
          db.GetString(reader, 2);
        entities.FinancialInstitutionDataMatch.MatchRunDate =
          db.GetString(reader, 3);
        entities.FinancialInstitutionDataMatch.MatchedPayeeSsn =
          db.GetString(reader, 4);
        entities.FinancialInstitutionDataMatch.MatchedPayeeName =
          db.GetNullableString(reader, 5);
        entities.FinancialInstitutionDataMatch.MatchedPayeeStreetAddress =
          db.GetString(reader, 6);
        entities.FinancialInstitutionDataMatch.MatchedPayeeCity =
          db.GetNullableString(reader, 7);
        entities.FinancialInstitutionDataMatch.MatchedPayeeState =
          db.GetNullableString(reader, 8);
        entities.FinancialInstitutionDataMatch.MatchedPayeeZipCode =
          db.GetNullableString(reader, 9);
        entities.FinancialInstitutionDataMatch.MatchedPayeeZip4 =
          db.GetNullableString(reader, 10);
        entities.FinancialInstitutionDataMatch.MatchedPayeeZip3 =
          db.GetNullableString(reader, 11);
        entities.FinancialInstitutionDataMatch.PayeeForeignCountryIndicator =
          db.GetNullableString(reader, 12);
        entities.FinancialInstitutionDataMatch.MatchFlag =
          db.GetNullableString(reader, 13);
        entities.FinancialInstitutionDataMatch.AccountBalance =
          db.GetNullableDecimal(reader, 14);
        entities.FinancialInstitutionDataMatch.AccountType =
          db.GetString(reader, 15);
        entities.FinancialInstitutionDataMatch.TrustFundIndicator =
          db.GetNullableString(reader, 16);
        entities.FinancialInstitutionDataMatch.AccountBalanceIndicator =
          db.GetNullableString(reader, 17);
        entities.FinancialInstitutionDataMatch.DateOfBirth =
          db.GetNullableDate(reader, 18);
        entities.FinancialInstitutionDataMatch.PayeeIndicator =
          db.GetNullableString(reader, 19);
        entities.FinancialInstitutionDataMatch.AccountFullLegalTitle =
          db.GetNullableString(reader, 20);
        entities.FinancialInstitutionDataMatch.PrimarySsn =
          db.GetNullableString(reader, 21);
        entities.FinancialInstitutionDataMatch.SecondPayeeName =
          db.GetNullableString(reader, 22);
        entities.FinancialInstitutionDataMatch.SecondPayeeSsn =
          db.GetNullableString(reader, 23);
        entities.FinancialInstitutionDataMatch.MsfidmIndicator =
          db.GetNullableString(reader, 24);
        entities.FinancialInstitutionDataMatch.InstitutionName =
          db.GetNullableString(reader, 25);
        entities.FinancialInstitutionDataMatch.InstitutionStreetAddress =
          db.GetNullableString(reader, 26);
        entities.FinancialInstitutionDataMatch.InstitutionCity =
          db.GetNullableString(reader, 27);
        entities.FinancialInstitutionDataMatch.InstitutionState =
          db.GetNullableString(reader, 28);
        entities.FinancialInstitutionDataMatch.InstitutionZipCode =
          db.GetNullableString(reader, 29);
        entities.FinancialInstitutionDataMatch.InstitutionZip4 =
          db.GetNullableString(reader, 30);
        entities.FinancialInstitutionDataMatch.InstitutionZip3 =
          db.GetNullableString(reader, 31);
        entities.FinancialInstitutionDataMatch.SecondInstitutionName =
          db.GetNullableString(reader, 32);
        entities.FinancialInstitutionDataMatch.TransmitterTin =
          db.GetNullableString(reader, 33);
        entities.FinancialInstitutionDataMatch.TransmitterName =
          db.GetNullableString(reader, 34);
        entities.FinancialInstitutionDataMatch.TransmitterStreetAddress =
          db.GetNullableString(reader, 35);
        entities.FinancialInstitutionDataMatch.TransmitterCity =
          db.GetNullableString(reader, 36);
        entities.FinancialInstitutionDataMatch.TransmitterState =
          db.GetNullableString(reader, 37);
        entities.FinancialInstitutionDataMatch.TransmitterZipCode =
          db.GetNullableString(reader, 38);
        entities.FinancialInstitutionDataMatch.TransmitterZip4 =
          db.GetNullableString(reader, 39);
        entities.FinancialInstitutionDataMatch.TransmitterZip3 =
          db.GetNullableString(reader, 40);
        entities.FinancialInstitutionDataMatch.CreatedBy =
          db.GetNullableString(reader, 41);
        entities.FinancialInstitutionDataMatch.CreatedTimestamp =
          db.GetNullableDateTime(reader, 42);
        entities.FinancialInstitutionDataMatch.AccountStatusIndicator =
          db.GetNullableString(reader, 43);
        entities.FinancialInstitutionDataMatch.Note =
          db.GetNullableString(reader, 44);
        entities.FinancialInstitutionDataMatch.LastUpdatedBy =
          db.GetNullableString(reader, 45);
        entities.FinancialInstitutionDataMatch.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 46);
        entities.FinancialInstitutionDataMatch.Populated = true;
      });
  }

  private void UpdateFinancialInstitutionDataMatch()
  {
    var matchedPayeeSsn = import.FinancialInstitutionDataMatch.MatchedPayeeSsn;
    var matchedPayeeName =
      import.FinancialInstitutionDataMatch.MatchedPayeeName ?? "";
    var matchedPayeeStreetAddress =
      import.FinancialInstitutionDataMatch.MatchedPayeeStreetAddress;
    var matchedPayeeCity =
      import.FinancialInstitutionDataMatch.MatchedPayeeCity ?? "";
    var matchedPayeeState =
      import.FinancialInstitutionDataMatch.MatchedPayeeState ?? "";
    var matchedPayeeZipCode =
      import.FinancialInstitutionDataMatch.MatchedPayeeZipCode ?? "";
    var matchedPayeeZip4 =
      import.FinancialInstitutionDataMatch.MatchedPayeeZip4 ?? "";
    var matchedPayeeZip3 =
      import.FinancialInstitutionDataMatch.MatchedPayeeZip3 ?? "";
    var payeeForeignCountryIndicator =
      import.FinancialInstitutionDataMatch.PayeeForeignCountryIndicator ?? "";
    var matchFlag = import.FinancialInstitutionDataMatch.MatchFlag ?? "";
    var accountBalance =
      import.FinancialInstitutionDataMatch.AccountBalance.GetValueOrDefault();
    var trustFundIndicator =
      import.FinancialInstitutionDataMatch.TrustFundIndicator ?? "";
    var accountBalanceIndicator =
      import.FinancialInstitutionDataMatch.AccountBalanceIndicator ?? "";
    var dateOfBirth = import.FinancialInstitutionDataMatch.DateOfBirth;
    var payeeIndicator =
      import.FinancialInstitutionDataMatch.PayeeIndicator ?? "";
    var accountFullLegalTitle =
      import.FinancialInstitutionDataMatch.AccountFullLegalTitle ?? "";
    var primarySsn = import.FinancialInstitutionDataMatch.PrimarySsn ?? "";
    var secondPayeeName =
      import.FinancialInstitutionDataMatch.SecondPayeeName ?? "";
    var secondPayeeSsn =
      import.FinancialInstitutionDataMatch.SecondPayeeSsn ?? "";
    var msfidmIndicator =
      import.FinancialInstitutionDataMatch.MsfidmIndicator ?? "";
    var institutionName =
      import.FinancialInstitutionDataMatch.InstitutionName ?? "";
    var institutionStreetAddress =
      import.FinancialInstitutionDataMatch.InstitutionStreetAddress ?? "";
    var institutionCity =
      import.FinancialInstitutionDataMatch.InstitutionCity ?? "";
    var institutionState =
      import.FinancialInstitutionDataMatch.InstitutionState ?? "";
    var institutionZipCode =
      import.FinancialInstitutionDataMatch.InstitutionZipCode ?? "";
    var institutionZip4 =
      import.FinancialInstitutionDataMatch.InstitutionZip4 ?? "";
    var institutionZip3 =
      import.FinancialInstitutionDataMatch.InstitutionZip3 ?? "";
    var secondInstitutionName =
      import.FinancialInstitutionDataMatch.SecondInstitutionName ?? "";
    var transmitterTin =
      import.FinancialInstitutionDataMatch.TransmitterTin ?? "";
    var transmitterName =
      import.FinancialInstitutionDataMatch.TransmitterName ?? "";
    var transmitterStreetAddress =
      import.FinancialInstitutionDataMatch.TransmitterStreetAddress ?? "";
    var transmitterCity =
      import.FinancialInstitutionDataMatch.TransmitterCity ?? "";
    var transmitterState =
      import.FinancialInstitutionDataMatch.TransmitterState ?? "";
    var transmitterZipCode =
      import.FinancialInstitutionDataMatch.TransmitterZipCode ?? "";
    var transmitterZip4 =
      import.FinancialInstitutionDataMatch.TransmitterZip4 ?? "";
    var transmitterZip3 =
      import.FinancialInstitutionDataMatch.TransmitterZip3 ?? "";
    var accountStatusIndicator =
      import.FinancialInstitutionDataMatch.AccountStatusIndicator ?? "";
    var note = import.FinancialInstitutionDataMatch.Note ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.FinancialInstitutionDataMatch.Populated = false;
    Update("UpdateFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetString(command, "matchedPayeeSsn", matchedPayeeSsn);
        db.SetNullableString(command, "matchedPayeeName", matchedPayeeName);
        db.SetString(command, "matchPayStrAddr", matchedPayeeStreetAddress);
        db.SetNullableString(command, "matchedPayeeCity", matchedPayeeCity);
        db.SetNullableString(command, "matchedPayeeStat", matchedPayeeState);
        db.SetNullableString(command, "matchPayZipCode", matchedPayeeZipCode);
        db.SetNullableString(command, "matchedPayeeZip4", matchedPayeeZip4);
        db.SetNullableString(command, "matchedPayeeZip3", matchedPayeeZip3);
        db.SetNullableString(
          command, "payeeForCtryInd", payeeForeignCountryIndicator);
        db.SetNullableString(command, "matchFlag", matchFlag);
        db.SetNullableDecimal(command, "accountBalance", accountBalance);
        db.SetNullableString(command, "trustFundInd", trustFundIndicator);
        db.SetNullableString(command, "accountBalInd", accountBalanceIndicator);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "payeeIndicator", payeeIndicator);
        db.SetNullableString(command, "acctFullLglTtl", accountFullLegalTitle);
        db.SetNullableString(command, "primarySsn", primarySsn);
        db.SetNullableString(command, "secondPayeeName", secondPayeeName);
        db.SetNullableString(command, "secondPayeeSsn", secondPayeeSsn);
        db.SetNullableString(command, "msfidmIndicator", msfidmIndicator);
        db.SetNullableString(command, "institutionName", institutionName);
        db.SetNullableString(command, "instStrAddr", institutionStreetAddress);
        db.SetNullableString(command, "institutionCity", institutionCity);
        db.SetNullableString(command, "institutionState", institutionState);
        db.SetNullableString(command, "instZipCode", institutionZipCode);
        db.SetNullableString(command, "institutionZip4", institutionZip4);
        db.SetNullableString(command, "institutionZip3", institutionZip3);
        db.SetNullableString(command, "secondInstName", secondInstitutionName);
        db.SetNullableString(command, "transmitterTin", transmitterTin);
        db.SetNullableString(command, "transmitterName", transmitterName);
        db.SetNullableString(command, "transStrAddr", transmitterStreetAddress);
        db.SetNullableString(command, "transmitterCity", transmitterCity);
        db.SetNullableString(command, "transmitterState", transmitterState);
        db.SetNullableString(command, "transZipCode", transmitterZipCode);
        db.SetNullableString(command, "transmitterZip4", transmitterZip4);
        db.SetNullableString(command, "transmitterZip3", transmitterZip3);
        db.SetNullableString(command, "accountStatInd", accountStatusIndicator);
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(
          command, "cseNumber",
          entities.FinancialInstitutionDataMatch.CsePersonNumber);
        db.SetString(
          command, "institutionTin",
          entities.FinancialInstitutionDataMatch.InstitutionTin);
        db.SetString(
          command, "matchPayAcctNum",
          entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber);
        db.SetString(
          command, "matchRunDate",
          entities.FinancialInstitutionDataMatch.MatchRunDate);
        db.SetString(
          command, "accountType",
          entities.FinancialInstitutionDataMatch.AccountType);
      });

    entities.FinancialInstitutionDataMatch.MatchedPayeeSsn = matchedPayeeSsn;
    entities.FinancialInstitutionDataMatch.MatchedPayeeName = matchedPayeeName;
    entities.FinancialInstitutionDataMatch.MatchedPayeeStreetAddress =
      matchedPayeeStreetAddress;
    entities.FinancialInstitutionDataMatch.MatchedPayeeCity = matchedPayeeCity;
    entities.FinancialInstitutionDataMatch.MatchedPayeeState =
      matchedPayeeState;
    entities.FinancialInstitutionDataMatch.MatchedPayeeZipCode =
      matchedPayeeZipCode;
    entities.FinancialInstitutionDataMatch.MatchedPayeeZip4 = matchedPayeeZip4;
    entities.FinancialInstitutionDataMatch.MatchedPayeeZip3 = matchedPayeeZip3;
    entities.FinancialInstitutionDataMatch.PayeeForeignCountryIndicator =
      payeeForeignCountryIndicator;
    entities.FinancialInstitutionDataMatch.MatchFlag = matchFlag;
    entities.FinancialInstitutionDataMatch.AccountBalance = accountBalance;
    entities.FinancialInstitutionDataMatch.TrustFundIndicator =
      trustFundIndicator;
    entities.FinancialInstitutionDataMatch.AccountBalanceIndicator =
      accountBalanceIndicator;
    entities.FinancialInstitutionDataMatch.DateOfBirth = dateOfBirth;
    entities.FinancialInstitutionDataMatch.PayeeIndicator = payeeIndicator;
    entities.FinancialInstitutionDataMatch.AccountFullLegalTitle =
      accountFullLegalTitle;
    entities.FinancialInstitutionDataMatch.PrimarySsn = primarySsn;
    entities.FinancialInstitutionDataMatch.SecondPayeeName = secondPayeeName;
    entities.FinancialInstitutionDataMatch.SecondPayeeSsn = secondPayeeSsn;
    entities.FinancialInstitutionDataMatch.MsfidmIndicator = msfidmIndicator;
    entities.FinancialInstitutionDataMatch.InstitutionName = institutionName;
    entities.FinancialInstitutionDataMatch.InstitutionStreetAddress =
      institutionStreetAddress;
    entities.FinancialInstitutionDataMatch.InstitutionCity = institutionCity;
    entities.FinancialInstitutionDataMatch.InstitutionState = institutionState;
    entities.FinancialInstitutionDataMatch.InstitutionZipCode =
      institutionZipCode;
    entities.FinancialInstitutionDataMatch.InstitutionZip4 = institutionZip4;
    entities.FinancialInstitutionDataMatch.InstitutionZip3 = institutionZip3;
    entities.FinancialInstitutionDataMatch.SecondInstitutionName =
      secondInstitutionName;
    entities.FinancialInstitutionDataMatch.TransmitterTin = transmitterTin;
    entities.FinancialInstitutionDataMatch.TransmitterName = transmitterName;
    entities.FinancialInstitutionDataMatch.TransmitterStreetAddress =
      transmitterStreetAddress;
    entities.FinancialInstitutionDataMatch.TransmitterCity = transmitterCity;
    entities.FinancialInstitutionDataMatch.TransmitterState = transmitterState;
    entities.FinancialInstitutionDataMatch.TransmitterZipCode =
      transmitterZipCode;
    entities.FinancialInstitutionDataMatch.TransmitterZip4 = transmitterZip4;
    entities.FinancialInstitutionDataMatch.TransmitterZip3 = transmitterZip3;
    entities.FinancialInstitutionDataMatch.AccountStatusIndicator =
      accountStatusIndicator;
    entities.FinancialInstitutionDataMatch.Note = note;
    entities.FinancialInstitutionDataMatch.LastUpdatedBy = lastUpdatedBy;
    entities.FinancialInstitutionDataMatch.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.FinancialInstitutionDataMatch.Populated = true;
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
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Exports.
    /// </summary>
    [JsonPropertyName("exports")]
    public FinancialInstitutionDataMatch Exports
    {
      get => exports ??= new();
      set => exports = value;
    }

    private FinancialInstitutionDataMatch exports;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }
#endregion
}
