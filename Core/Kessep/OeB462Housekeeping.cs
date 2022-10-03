// Program: OE_B462_HOUSEKEEPING, ID: 945103320, model: 746.
// Short name: SWE03667
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B462_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB462Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B462_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB462Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB462Housekeeping.
  /// </summary>
  public OeB462Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************
    // *Get the run parameters for this program.     *
    // ***********************************************
    local.ProgramProcessingInfo.Name = "SWEFB462";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // * Call external to OPEN the output file.       *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabWriteApDebtDerivation2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered opening kdwpout file.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // * Call external to Write the headings for the output file.  *
    // ************************************************
    local.CsePersonsWorkSet.FirstName = "FIRST NAME";
    local.CsePersonsWorkSet.LastName = "LAST NAME";
    local.CsePersonsWorkSet.Number = "PERSON #";
    local.LegalAction.StandardNumber = "COURT ORDER NUMBER";
    local.Local1.Number = "CASE #1";
    local.Local2.Number = "CASE #2";
    local.Local3.Number = "CASE #3";
    local.Local4.Number = "CASE #4";
    local.Local5.Number = "CASE #5";
    local.Cases.Text60 = local.Local1.Number + " " + local.Local2.Number + " " +
      local.Local3.Number + " " + local.Local4.Number + " " + local
      .Local5.Number;
    local.SummedAmount.Text11 = "SUMMED AMT";
    local.CsePersonAddress.Street1 = "STREET 1";
    local.CsePersonAddress.Street2 = "STREET 2";
    local.CsePersonAddress.City = "CITY";
    local.CsePersonAddress.State = "ST";
    local.CsePersonAddress.ZipCode = "ZIP";
    local.CsePersonAddress.Zip4 = "ZIP4";
    local.PassArea.FileInstruction = "WRITE";
    UseOeEabWriteApDebtDerivation1();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered opening kdwpout file.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeEabWriteApDebtDerivation1()
  {
    var useImport = new OeEabWriteApDebtDerivation.Import();
    var useExport = new OeEabWriteApDebtDerivation.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.Cases.Text60 = local.Cases.Text60;
    useImport.CsePersonAddress.Assign(local.CsePersonAddress);
    MoveExternal(local.PassArea, useImport.External);
    useImport.SummedAmount.Text11 = local.SummedAmount.Text11;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteApDebtDerivation.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteApDebtDerivation2()
  {
    var useImport = new OeEabWriteApDebtDerivation.Import();
    var useExport = new OeEabWriteApDebtDerivation.Export();

    MoveExternal(local.PassArea, useImport.External);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteApDebtDerivation.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of SummedAmount.
    /// </summary>
    [JsonPropertyName("summedAmount")]
    public WorkArea SummedAmount
    {
      get => summedAmount ??= new();
      set => summedAmount = value;
    }

    /// <summary>
    /// A value of Cases.
    /// </summary>
    [JsonPropertyName("cases")]
    public WorkArea Cases
    {
      get => cases ??= new();
      set => cases = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of A03eports.
    /// </summary>
    [JsonPropertyName("a03eports")]
    public EabReportSend A03eports
    {
      get => a03eports ??= new();
      set => a03eports = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of CasesFound.
    /// </summary>
    [JsonPropertyName("casesFound")]
    public WorkArea CasesFound
    {
      get => casesFound ??= new();
      set => casesFound = value;
    }

    /// <summary>
    /// A value of Local1.
    /// </summary>
    [JsonPropertyName("local1")]
    public Case1 Local1
    {
      get => local1 ??= new();
      set => local1 = value;
    }

    /// <summary>
    /// A value of Local2.
    /// </summary>
    [JsonPropertyName("local2")]
    public Case1 Local2
    {
      get => local2 ??= new();
      set => local2 = value;
    }

    /// <summary>
    /// A value of Local3.
    /// </summary>
    [JsonPropertyName("local3")]
    public Case1 Local3
    {
      get => local3 ??= new();
      set => local3 = value;
    }

    /// <summary>
    /// A value of Local4.
    /// </summary>
    [JsonPropertyName("local4")]
    public Case1 Local4
    {
      get => local4 ??= new();
      set => local4 = value;
    }

    /// <summary>
    /// A value of Local5.
    /// </summary>
    [JsonPropertyName("local5")]
    public Case1 Local5
    {
      get => local5 ??= new();
      set => local5 = value;
    }

    private CsePersonAddress csePersonAddress;
    private WorkArea summedAmount;
    private WorkArea cases;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EabReportSend a03eports;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private WorkArea casesFound;
    private Case1 local1;
    private Case1 local2;
    private Case1 local3;
    private Case1 local4;
    private Case1 local5;
  }
#endregion
}
