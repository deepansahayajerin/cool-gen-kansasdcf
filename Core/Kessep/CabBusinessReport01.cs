// Program: CAB_BUSINESS_REPORT_01, ID: 372264670, model: 746.
// Short name: SWE02351
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_BUSINESS_REPORT_01.
/// </summary>
[Serializable]
public partial class CabBusinessReport01: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_BUSINESS_REPORT_01 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabBusinessReport01(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabBusinessReport01.
  /// </summary>
  public CabBusinessReport01(IContext context, Import import, Export export):
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
    switch(TrimEnd(import.EabFileHandling.Action))
    {
      case "OPEN":
        local.EabReportSend.Assign(local.Initialize);
        MoveEabReportSend(import.NeededToOpen, local.EabReportSend);
        local.EabFileHandling.Action = "OPEN";
        local.EabReportSend.ReportNumber = 1;
        UseEabWriteReport();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "HEADING";
        local.EabReportSend.ProgramName = import.NeededToOpen.ProgramName;
        local.EabReportSend.ReportNoPart2 = "01";
        local.EabReportSend.ProcessDate = import.NeededToOpen.ProcessDate;
        local.EabReportSend.RunDate = Now().Date;
        local.EabReportSend.RunTime = Time(Now());
        local.EabReportSend.RptHeading1 =
          "     KANSAS DEPARTMENT OF SOCIAL AND REHABILITATION SERVICES";
        local.EabReportSend.RptHeading2 =
          "                    CHILD SUPPORT ENFORCEMENT";

        if (IsEmpty(local.EabReportSend.RptHeading3))
        {
          if (ReadPgmNameTable())
          {
            local.EabReportSend.RptHeading3 = "            " + entities
              .PgmNameTable.PgmDescription;
          }
          else
          {
            local.EabReportSend.RptHeading3 = "            " + "Program Name not in PGM NAME TABLE";
              
          }
        }

        break;
      case "CLOSE":
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "CLOSING";
        local.EabReportSend.ReportNumber = 1;
        UseEabWriteReport();
        local.EabFileHandling.Action = "CLOSE";
        local.EabReportSend.ReportNumber = 1;

        break;
      case "WRITE":
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "DETAIL";
        local.EabReportSend.RptDetail = import.NeededToWrite.RptDetail;
        local.EabReportSend.ReportNumber = 1;

        break;
      case "NEWPAGE":
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "HEADING";

        break;
      default:
        break;
    }

    UseEabWriteReport();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
    target.ColHeading3 = source.ColHeading3;
  }

  private void UseEabWriteReport()
  {
    var useImport = new EabWriteReport.Import();
    var useExport = new EabWriteReport.Export();

    useImport.EabReportSend.Assign(local.EabReportSend);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
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
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public EabReportSend Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      initialize = null;
      eabFileHandling = null;
      eabReportReturn = null;
    }

    private EabReportSend initialize;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private EabReportReturn eabReportReturn;
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
