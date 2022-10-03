// Program: OE_B493_HOUSEKEEPING, ID: 372871044, model: 746.
// Short name: SWE02471
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B493_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB493Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B493_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB493Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB493Housekeeping.
  /// </summary>
  public OeB493Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = "SWEEB493";

    if (ReadProgramProcessingInfo())
    {
      export.LastRun.Timestamp =
        Timestamp(
          Substring(entities.ProgramProcessingInfo.ParameterList, 10, 26));
      local.Process.Date = entities.ProgramProcessingInfo.ProcessDate;
      export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
      local.PpiFound.Flag = "Y";
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF";

      return;
    }

    // **********************************************************
    // GET TABLES TO BE USED LATER
    // **********************************************************
    local.Code.CodeName = "EDS RELATIONSHIPS";

    if (ReadCode())
    {
      export.EdsRelationshipCode.Id = entities.Code.Id;
    }
    else
    {
      ExitState = "CODE_NF";

      return;
    }

    local.Code.CodeName = "EDS COVERAGES";

    if (ReadCode())
    {
      export.EdsCoverageCode.Id = entities.Code.Id;
    }
    else
    {
      ExitState = "CODE_NF";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.Process.Date;
    local.EabReportSend.ProgramName = entities.ProgramProcessingInfo.Name;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT BUSINESS REPORT 01
    // **********************************************************
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.Process.Date;
    UseCabFormatDate();
    local.NeededToOpen.RptHeading3 =
      "      HEALTH INSURANCE COVERAGE ACTIVITY FOR " + local
      .FormattedDate.Text10 + " ";
    local.NeededToOpen.NumberOfColHeadings = 3;
    local.NeededToOpen.BlankLineAfterHeading = "Y";
    local.NeededToOpen.BlankLineAfterColHead = "N";
    local.NeededToOpen.ColHeading1 =
      "CHILD'S    POLICY HOLDER'S                         CARRIER                        COVERAGE   COVERAGE   COVERAGE";
      
    local.NeededToOpen.ColHeading2 =
      "NUMBER     SSN         NAME                        CODE    POLICY ID    GROUP     BEGIN DATE END DATE   CODES";
      
    local.NeededToOpen.ColHeading3 =
      "========== =========== =========================== ======= ============ ========= ========== ========== ============================";
      
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT FILE EAB
    // **********************************************************
    UseEabWriteHinsCoverageChanges();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToOpen.Assign(local.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.Process.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseEabWriteHinsCoverageChanges()
  {
    var useImport = new EabWriteHinsCoverageChanges.Import();
    var useExport = new EabWriteHinsCoverageChanges.Export();

    useImport.DateWorkArea.Date = local.Process.Date;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteHinsCoverageChanges.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCode()
  {
    entities.Code.Populated = false;

    return Read("ReadCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", local.Code.CodeName);
        db.SetDate(
          command, "effectiveDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.Code.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
    }

    /// <summary>
    /// A value of EdsCoverageCode.
    /// </summary>
    [JsonPropertyName("edsCoverageCode")]
    public Code EdsCoverageCode
    {
      get => edsCoverageCode ??= new();
      set => edsCoverageCode = value;
    }

    /// <summary>
    /// A value of EdsRelationshipCode.
    /// </summary>
    [JsonPropertyName("edsRelationshipCode")]
    public Code EdsRelationshipCode
    {
      get => edsRelationshipCode ??= new();
      set => edsRelationshipCode = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea lastRun;
    private Code edsCoverageCode;
    private Code edsRelationshipCode;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
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

    private DateWorkArea process;
    private WorkArea formattedDate;
    private Code code;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo programProcessingInfo;
    private Common ppiFound;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private Code code;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
