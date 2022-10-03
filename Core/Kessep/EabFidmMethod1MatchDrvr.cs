// Program: EAB_FIDM_METHOD1_MATCH_DRVR, ID: 374406855, model: 746.
// Short name: SWEXEE38
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_FIDM_METHOD1_MATCH_DRVR.
/// </summary>
[Serializable]
public partial class EabFidmMethod1MatchDrvr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FIDM_METHOD1_MATCH_DRVR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFidmMethod1MatchDrvr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFidmMethod1MatchDrvr.
  /// </summary>
  public EabFidmMethod1MatchDrvr(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXEE38", context, import, export, EabOptions.Hpvp);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "InstitutionTin",
      "MatchRunDate",
      "InstitutionName"
    })]
    public FinancialInstitutionDataMatch Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private FinancialInstitutionDataMatch restart;
    private External passArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReturnedExternal.
    /// </summary>
    [JsonPropertyName("returnedExternal")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "NumericReturnCode",
      "TextLine80",
      "TextReturnCode"
    })]
    public External ReturnedExternal
    {
      get => returnedExternal ??= new();
      set => returnedExternal = value;
    }

    /// <summary>
    /// A value of ReturnedRecType.
    /// </summary>
    [JsonPropertyName("returnedRecType")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea ReturnedRecType
    {
      get => returnedRecType ??= new();
      set => returnedRecType = value;
    }

    /// <summary>
    /// A value of ReturnedFinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("returnedFinancialInstitutionDataMatch")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
      "InstitutionTin",
      "MatchedPayeeAccountNumber",
      "MatchRunDate",
      "MatchedPayeeSsn",
      "MatchedPayeeName",
      "MatchedPayeeStreetAddress",
      "MatchedPayeeCity",
      "MatchedPayeeState",
      "MatchedPayeeZipCode",
      "MatchedPayeeZip4",
      "MatchedPayeeZip3",
      "PayeeForeignCountryIndicator",
      "MatchFlag",
      "AccountBalance",
      "AccountType",
      "TrustFundIndicator",
      "AccountBalanceIndicator",
      "DateOfBirth",
      "PayeeIndicator",
      "AccountFullLegalTitle",
      "PrimarySsn",
      "SecondPayeeName",
      "SecondPayeeSsn",
      "MsfidmIndicator",
      "InstitutionName",
      "InstitutionStreetAddress",
      "InstitutionCity",
      "InstitutionState",
      "InstitutionZipCode",
      "InstitutionZip4",
      "InstitutionZip3",
      "SecondInstitutionName",
      "TransmitterTin",
      "TransmitterName",
      "TransmitterStreetAddress",
      "TransmitterCity",
      "TransmitterState",
      "TransmitterZipCode",
      "TransmitterZip4",
      "TransmitterZip3"
    })]
    public FinancialInstitutionDataMatch ReturnedFinancialInstitutionDataMatch
    {
      get => returnedFinancialInstitutionDataMatch ??= new();
      set => returnedFinancialInstitutionDataMatch = value;
    }

    private External returnedExternal;
    private TextWorkArea returnedRecType;
    private FinancialInstitutionDataMatch returnedFinancialInstitutionDataMatch;
  }
#endregion
}
