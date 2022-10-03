// Program: CAB_ERROR_REPORT, ID: 371787298, model: 746.
// Short name: SWE02278
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_ERROR_REPORT.
/// </summary>
[Serializable]
public partial class CabErrorReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_ERROR_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabErrorReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabErrorReport.
  /// </summary>
  public CabErrorReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *** 07-13-2012   AHockman    cq SR 33636 Segment E  Agency name change.
    switch(TrimEnd(import.EabFileHandling.Action))
    {
      case "OPEN":
        local.EabReportSend.Assign(local.Initialize);
        local.EabFileHandling.Action = "OPEN";
        local.EabReportSend.ReportNumber = 99;
        UseEabWriteReport();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "HEADING";
        local.EabReportSend.ProgramName = import.NeededToOpen.ProgramName;
        local.EabReportSend.ReportNoPart2 = "99";
        local.EabReportSend.ProcessDate = import.NeededToOpen.ProcessDate;
        local.EabReportSend.RunDate = Now().Date;
        local.EabReportSend.RunTime = Time(Now());
        local.EabReportSend.RptHeading1 =
          "           KANSAS DEPARTMENT FOR CHILDREN AND FAMILIES";
        local.EabReportSend.RptHeading2 =
          "                      CHILD SUPPORT SERVICES";

        if (ReadPgmNameTable())
        {
          local.EabReportSend.RptHeading3 = "               " + TrimEnd
            (Substring(entities.PgmNameTable.PgmDescription, 1, 45)) + " - ERROR REPORT";
            
        }
        else
        {
          local.EabReportSend.RptHeading3 =
            "PROGRAM NAME IS NOT IN THE PGM_NAME_TABLE - ERROR REPORT";
        }

        local.EabReportSend.BlankLineAfterHeading = "Y";

        break;
      case "CLOSE":
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "CLOSING";
        local.EabReportSend.ReportNumber = 99;
        UseEabWriteReport();
        local.EabFileHandling.Action = "CLOSE";
        local.EabReportSend.ReportNumber = 99;

        break;
      case "WRITE":
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "DETAIL";
        local.EabReportSend.ReportNumber = 99;
        local.EabReportSend.RptDetail = import.NeededToWrite.RptDetail;

        break;
      default:
        break;
    }

    UseEabWriteReport();
  }

  private void UseEabWriteReport()
  {
    var useImport = new EabWriteReport.Import();
    var useExport = new EabWriteReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.Assign(local.EabReportSend);
    useExport.EabReportReturn.Assign(local.EabReportReturn);
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabWriteReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadPgmNameTable()
  {
    entities.PgmNameTable.Populated = false;

    return Read("ReadPgmNameTable",
      (db, command) =>
      {
        db.SetString(command, "programName", import.NeededToOpen.ProgramName);
      },
      (db, reader) =>
      {
        entities.PgmNameTable.PgmName = db.GetString(reader, 0);
        entities.PgmNameTable.PgmDescription = db.GetString(reader, 1);
        entities.PgmNameTable.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
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
    /// A value of EabReportReturn.
    /// </summary>
    [JsonPropertyName("eabReportReturn")]
    public EabReportReturn EabReportReturn
    {
      get => eabReportReturn ??= new();
      set => eabReportReturn = value;
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
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public EabReportSend Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      eabFileHandling = null;
      eabReportReturn = null;
      initialize = null;
    }

    private EabFileHandling eabFileHandling;
    private EabReportReturn eabReportReturn;
    private EabReportSend eabReportSend;
    private EabReportSend initialize;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PgmNameTable.
    /// </summary>
    [JsonPropertyName("pgmNameTable")]
    public PgmNameTable PgmNameTable
    {
      get => pgmNameTable ??= new();
      set => pgmNameTable = value;
    }

    private PgmNameTable pgmNameTable;
  }
#endregion
}
