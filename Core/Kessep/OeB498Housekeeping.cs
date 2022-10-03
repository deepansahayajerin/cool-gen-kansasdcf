// Program: OE_B498_HOUSEKEEPING, ID: 371179798, model: 746.
// Short name: SWE01977
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B498_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB498Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B498_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB498Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB498Housekeeping.
  /// </summary>
  public OeB498Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *   Process Ended Health Insurance Company - Housekeeping.            *
    // ***********************************************************************
    // *              M A I N T E N A N C E   L O G                          *
    // ***********************************************************************
    // *Date		Name      Work Req  		Description           *
    // ***********************************************************************
    // * 02/25/2003  	Ed Lyman  WR020311  	Initial Coding
    // * 08/25/2003	E.Shirk	  WR20311	Complete initial coding.
    // ***********************************************************************
    export.ProgramProcessingInfo.Name = "SWEEB498";
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = export.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = Now().Date;
    export.Max.Date = new DateTime(2099, 12, 31);

    // ****************************************************************************
    // **           Open error report.
    // ****************************************************************************
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ****************************************************************************
    // **           Open control report.
    // ****************************************************************************
    local.NeededToOpen.ProcessDate = entities.ProgramProcessingInfo.ProcessDate;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ****************************************************************************
    // **         Open report Rpt01.
    // ****************************************************************************
    local.NeededToOpen.ProgramName = export.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.RptHeading3 =
      "     HEALTH INSURANCE POLICIES TERMINATED WHEN COMPANY ENDED" + "" + ""
      + "";
    local.NeededToOpen.NumberOfColHeadings = 3;
    local.NeededToOpen.BlankLineAfterHeading = "Y";
    local.NeededToOpen.BlankLineAfterColHead = "N";
    local.NeededToOpen.ColHeading1 = "  RESP.    CARRIER   POLICY          END";
    local.NeededToOpen.ColHeading2 =
      " PERSON     CODE     NUMBER          DATE    RESULT";
    local.NeededToOpen.ColHeading3 =
      "========== ======= ================ ======== ===================================================================";
      
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **************************************************************************
    // **    Get program processing information.
    // **************************************************************************
    if (ReadProgramProcessingInfo())
    {
      MoveProgramProcessingInfo(entities.ProgramProcessingInfo,
        export.ProgramProcessingInfo);
      export.HealthInsuranceCompany.CarrierCode =
        Substring(entities.ProgramProcessingInfo.ParameterList, 1, 7);
      local.PpiFound.Flag = "Y";
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      local.NeededToWrite.RptDetail =
        "Program Processing Record is missing for " + export
        .ProgramProcessingInfo.Name;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // **************************************************************************
    // **    Get program checkpoint information.
    // **************************************************************************
    local.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Program Checkpoint Restart Record is missing for " + export
        .ProgramProcessingInfo.Name;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **************************************************************************
    // **    Edit carrier code.
    // **************************************************************************
    if (Lt(export.HealthInsuranceCompany.CarrierCode, "0000001") || Lt
      ("9999999", export.HealthInsuranceCompany.CarrierCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "PPI Record (columns 1 thru 7) does not contain a valid carrier:  " + (
          export.HealthInsuranceCompany.CarrierCode ?? "");
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", export.ProgramProcessingInfo.Name);
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea max;
    private HealthInsuranceCompany healthInsuranceCompany;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private Common ppiFound;
    private EabReportSend neededToWrite;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
