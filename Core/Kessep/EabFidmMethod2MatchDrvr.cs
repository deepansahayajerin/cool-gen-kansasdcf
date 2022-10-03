// Program: EAB_FIDM_METHOD2_MATCH_DRVR, ID: 374405044, model: 746.
// Short name: SWEXEE37
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_FIDM_METHOD2_MATCH_DRVR.
/// </summary>
[Serializable]
public partial class EabFidmMethod2MatchDrvr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FIDM_METHOD2_MATCH_DRVR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFidmMethod2MatchDrvr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFidmMethod2MatchDrvr.
  /// </summary>
  public EabFidmMethod2MatchDrvr(IContext context, Import import, Export export):
    
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
      "SWEXEE37", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private FinancialInstitutionDataMatch restart;
    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "TextReturnCode", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RecordTypeReturned.
    /// </summary>
    [JsonPropertyName("recordTypeReturned")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea RecordTypeReturned
    {
      get => recordTypeReturned ??= new();
      set => recordTypeReturned = value;
    }

    /// <summary>
    /// A value of AccountOwnerRec.
    /// </summary>
    [JsonPropertyName("accountOwnerRec")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
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
      "SecondPayeeSsn"
    })]
    public FinancialInstitutionDataMatch AccountOwnerRec
    {
      get => accountOwnerRec ??= new();
      set => accountOwnerRec = value;
    }

    /// <summary>
    /// A value of FinancialInstitutionRec.
    /// </summary>
    [JsonPropertyName("financialInstitutionRec")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "InstitutionTin",
      "MatchRunDate",
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
    public FinancialInstitutionDataMatch FinancialInstitutionRec
    {
      get => financialInstitutionRec ??= new();
      set => financialInstitutionRec = value;
    }

    private External external;
    private TextWorkArea recordTypeReturned;
    private FinancialInstitutionDataMatch accountOwnerRec;
    private FinancialInstitutionDataMatch financialInstitutionRec;
  }
#endregion
}
