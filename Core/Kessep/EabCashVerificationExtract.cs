// Program: EAB_CASH_VERIFICATION_EXTRACT, ID: 372927609, model: 746.
// Short name: SWEXG715
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CASH_VERIFICATION_EXTRACT.
/// </summary>
[Serializable]
public partial class EabCashVerificationExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CASH_VERIFICATION_EXTRACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCashVerificationExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCashVerificationExtract.
  /// </summary>
  public EabCashVerificationExtract(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXG715", context, import, export, EabOptions.Hpvp);
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of CashVerificationExtract.
    /// </summary>
    [JsonPropertyName("cashVerificationExtract")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "DetailsForLine",
      "DdDueDt",
      "DdCreatedTmst",
      "DdCreatedBy",
      "ObTranIdentifier",
      "ObTranType",
      "ObTranAmount",
      "ObTranCreatedBy",
      "ObTranCreatedTmst",
      "ObTranDebtAdjustmentDt",
      "ObTranDebtAdjustmentType",
      "ObTypeCode",
      "ObTypeClassification",
      "CpaType",
      "CpaCreatedTmst",
      "CpType",
      "CpCreatedTimestamp",
      "CpNumber",
      "PpCreatedTimestamp",
      "PpEffDate",
      "PpDiscDate",
      "Pcode",
      "PeffDate",
      "PdiscDate",
      "OtrIdentifier",
      "OtrCreatedTmst",
      "CollProgramAppliedTo",
      "CollIdentifier",
      "CollAmount",
      "CollAppliedToCode",
      "CollAdjustedInd",
      "CollConcurrentInd",
      "CollCreatedTmst",
      "CrtIdentifier",
      "ObIdentifier",
      "ObCreatedTmst"
    })]
    public CashVerificationExtract CashVerificationExtract
    {
      get => cashVerificationExtract ??= new();
      set => cashVerificationExtract = value;
    }

    private ReportParms reportParms;
    private CashVerificationExtract cashVerificationExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
