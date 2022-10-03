// Program: EAB_CASE_VERIFICATION_EXTRACT, ID: 372927548, model: 746.
// Short name: SWEXF715
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CASE_VERIFICATION_EXTRACT.
/// </summary>
[Serializable]
public partial class EabCaseVerificationExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CASE_VERIFICATION_EXTRACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCaseVerificationExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCaseVerificationExtract.
  /// </summary>
  public EabCaseVerificationExtract(IContext context, Import import,
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
      "SWEXF715", context, import, export, EabOptions.Hpvp);
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
    /// A value of CaseVerificationExtract.
    /// </summary>
    [JsonPropertyName("caseVerificationExtract")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "DetailsForLine",
      "Cnumber",
      "Cstatus",
      "CstatusDate",
      "CopenDate",
      "CrIdentifier",
      "CrType",
      "CrStartDate",
      "CrEndDate",
      "CpType",
      "CpNumber",
      "PpCreatedTimestamp",
      "PpEffDate",
      "PpDiscDate",
      "Pcode",
      "PeffDate",
      "PdiscDate",
      "LacrCreatedTstamp",
      "LaIdentifier",
      "LaActionTaken",
      "LaCreatedTstamp",
      "LaFiledDate",
      "LadNumber",
      "LadDetailType",
      "LadCreatedTstamp",
      "LadNonFinOblgType",
      "OtCode",
      "OtClassification",
      "DdDueDt",
      "DdBalanceDueAmt",
      "DdInterestBalanceDueAmt",
      "DdCreatedTmst",
      "CpaType",
      "CpaCreatedTmst",
      "ObTranIdentifier",
      "ObTranType",
      "ObTranCreatedTmst",
      "ObIdentifier",
      "ObCreatedTmst",
      "CollIdentifier",
      "CollAppliedToCode",
      "CollAdjustedInd",
      "CollConcurrentInd",
      "CollCreatedTmst"
    })]
    public CaseVerificationExtract CaseVerificationExtract
    {
      get => caseVerificationExtract ??= new();
      set => caseVerificationExtract = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private CaseVerificationExtract caseVerificationExtract;
    private ReportParms reportParms;
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
