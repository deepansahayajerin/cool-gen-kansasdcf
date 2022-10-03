// Program: OE_CREATE_FIDM_DATA, ID: 374389320, model: 746.
// Short name: SWE02606
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CREATE_FIDM_DATA.
/// </para>
/// <para>
/// Create the FINANCIAL_INSTITUTION_DATA_MATCH table using data from the Input 
/// Flat File.
/// </para>
/// </summary>
[Serializable]
public partial class OeCreateFidmData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_FIDM_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateFidmData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateFidmData.
  /// </summary>
  public OeCreateFidmData(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      CreateFinancialInstitutionDataMatch();
      export.FinancialInstitutionDataMatch.CreatedTimestamp =
        entities.FinancialInstitutionDataMatch.CreatedTimestamp;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FINANCIAL_INSTITUTION_DATA_MA_AE";

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

  private void CreateFinancialInstitutionDataMatch()
  {
    var csePersonNumber = import.FinancialInstitutionDataMatch.CsePersonNumber;
    var institutionTin = import.FinancialInstitutionDataMatch.InstitutionTin;
    var matchedPayeeAccountNumber =
      import.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber;
    var matchRunDate = import.FinancialInstitutionDataMatch.MatchRunDate;
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
    var accountType = import.FinancialInstitutionDataMatch.AccountType;
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
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var accountStatusIndicator =
      import.FinancialInstitutionDataMatch.AccountStatusIndicator ?? "";

    entities.FinancialInstitutionDataMatch.Populated = false;
    Update("CreateFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetString(command, "cseNumber", csePersonNumber);
        db.SetString(command, "institutionTin", institutionTin);
        db.SetString(command, "matchPayAcctNum", matchedPayeeAccountNumber);
        db.SetString(command, "matchRunDate", matchRunDate);
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
        db.SetString(command, "accountType", accountType);
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
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "accountStatInd", accountStatusIndicator);
        db.SetNullableString(command, "note", "");
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.FinancialInstitutionDataMatch.CsePersonNumber = csePersonNumber;
    entities.FinancialInstitutionDataMatch.InstitutionTin = institutionTin;
    entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
      matchedPayeeAccountNumber;
    entities.FinancialInstitutionDataMatch.MatchRunDate = matchRunDate;
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
    entities.FinancialInstitutionDataMatch.AccountType = accountType;
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
    entities.FinancialInstitutionDataMatch.CreatedBy = createdBy;
    entities.FinancialInstitutionDataMatch.CreatedTimestamp = createdTimestamp;
    entities.FinancialInstitutionDataMatch.AccountStatusIndicator =
      accountStatusIndicator;
    entities.FinancialInstitutionDataMatch.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
