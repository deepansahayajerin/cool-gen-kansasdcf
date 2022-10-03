// Program: CAB_BUSINESS_REPORT_11, ID: 371411825, model: 746.
// Short name: SWE02078
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_BUSINESS_REPORT_11.
/// </summary>
[Serializable]
public partial class CabBusinessReport11: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_BUSINESS_REPORT_11 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabBusinessReport11(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabBusinessReport11.
  /// </summary>
  public CabBusinessReport11(IContext context, Import import, Export export):
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
    // : This action block is same as the other business report action block. 
    // But, this returns back the report line number,
    //    lines remaining on the report and report page nbr.
    switch(TrimEnd(import.EabFileHandling.Action))
    {
      case "OPEN":
        local.EabReportSend.Assign(local.Initialize);
        MoveEabReportSend(import.NeededToOpen, local.EabReportSend);
        local.EabFileHandling.Action = "OPEN";
        local.EabReportSend.ReportNumber = 1;
        UseEabWriteReport1();
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
        UseEabWriteReport1();
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

    UseEabWriteReport2();
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

  private void UseEabWriteReport1()
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

  private void UseEabWriteReport2()
  {
    var useImport = new EabWriteReport.Import();
    var useExport = new EabWriteReport.Export();

    useImport.EabReportSend.Assign(local.EabReportSend);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;
    useExport.EabReportReturn.Assign(export.EabReportReturn);

    Call(EabWriteReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.EabReportReturn.Assign(useExport.EabReportReturn);
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

    /// <summary>
    /// A value of EabReportReturn.
    /// </summary>
    [JsonPropertyName("eabReportReturn")]
    public EabReportReturn EabReportReturn
    {
      get => eabReportReturn ??= new();
      set => eabReportReturn = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportReturn eabReportReturn;
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
