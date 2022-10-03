// Program: EAB_MSFIDM_DATA_MATCH_DRVR, ID: 374389261, model: 746.
// Short name: SWEXEE36
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_MSFIDM_DATA_MATCH_DRVR.
/// </para>
/// <para>
/// Controls input of the MSFIDM Batch File
/// </para>
/// </summary>
[Serializable]
public partial class EabMsfidmDataMatchDrvr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_MSFIDM_DATA_MATCH_DRVR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabMsfidmDataMatchDrvr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabMsfidmDataMatchDrvr.
  /// </summary>
  public EabMsfidmDataMatchDrvr(IContext context, Import import, Export export):
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
      "SWEXEE36", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

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
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    [Member(Index = 2, AccessFields = false, Members = new[]
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
      "TransmitterZip3",
      "CreatedBy",
      "CreatedTimestamp",
      "AccountStatusIndicator"
    })]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    private External external;
    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
  }
#endregion
}
