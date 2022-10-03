// Program: FN_B720_OCSE_396_REPORT, ID: 372733988, model: 746.
// Short name: SWEF720B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B720_OCSE_396_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB720Ocse396Report: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B720_OCSE_396_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB720Ocse396Report(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB720Ocse396Report.
  /// </summary>
  public FnB720Ocse396Report(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    // ***
    // *** get Program Processing info, where NAME is 'SWEFB720'
    // ***
    ReadProgramProcessingInfo();

    //   ***
    //  ***
    // *** Build the report quarter using the System Date
    // *** This value is used in retrieving the OCSE34 entity
    //  ***
    //   ***
    if (Month(entities.ProgramProcessingInfo.ProcessDate) == 1 || Month
      (entities.ProgramProcessingInfo.ProcessDate) == 2 || Month
      (entities.ProgramProcessingInfo.ProcessDate) == 3)
    {
      // *** Run month is January, February or March
      // ***
      // *** Format for the reporting period is YYYY04
      // *** 4th quarter of prior year
      local.Work.Period =
        Year(AddYears(entities.ProgramProcessingInfo.ProcessDate, -1)) * 100 + 4
        ;
    }
    else if (Month(entities.ProgramProcessingInfo.ProcessDate) == 4 || Month
      (entities.ProgramProcessingInfo.ProcessDate) == 5 || Month
      (entities.ProgramProcessingInfo.ProcessDate) == 6)
    {
      // *** Run month is April, May or June
      // ***
      // *** Format for the reporting period is YYYY01
      // *** 1st quarter of current year
      local.Work.Period = Year(entities.ProgramProcessingInfo.ProcessDate) * 100
        + 1;
    }
    else if (Month(entities.ProgramProcessingInfo.ProcessDate) == 7 || Month
      (entities.ProgramProcessingInfo.ProcessDate) == 8 || Month
      (entities.ProgramProcessingInfo.ProcessDate) == 9)
    {
      // *** Run month is July, August or September
      // ***
      // *** Format for the reporting period is YYYY02
      // *** 2nd quarter of current year
      local.Work.Period = Year(entities.ProgramProcessingInfo.ProcessDate) * 100
        + 2;
    }
    else
    {
      // *** Run month is October, November or December
      // ***
      // *** Format for thr reporting period is YYYY03
      // *** 3rd quarter of current year
      local.Work.Period = Year(entities.ProgramProcessingInfo.ProcessDate) * 100
        + 3;
    }

    //   ***
    //  ***
    // *** get each OCSE34 using the reporting quarter previously created
    //  ***
    //   ***
    if (ReadOcse34())
    {
      export.Ocse34.Assign(entities.Ocse34);

      //   ***
      //  ***
      // *** Create total for line 5 columns A + B+ C, required for
      // *** Part 3 line 15 of the report
      //  ***
      //   ***
      export.Ocse34.OtherStatesCurrentIvaAmount =
        entities.Ocse34.OtherStatesCurrentIvaAmount.GetValueOrDefault() + entities
        .Ocse34.OtherStatesCurrentIveAmount.GetValueOrDefault() + entities
        .Ocse34.OtherstateFormerAssistAmount.GetValueOrDefault();
    }

    //   ***
    //  ***
    // *** Open OCSE-396A report
    //  ***
    //   ***
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabOcse396();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // ***
      // *** Error opening report OCSE-396A
      // ***
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-396A report (Part 1 Page 1)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "MAIN";
    UseEabOcse396();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // ***
      // *** Error writing to report OCSE-396A (Part 1 Page 1)
      // ***
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-396A report (Part 1 Page 2)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "P1P2";
    UseEabOcse396();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // ***
      // *** Error writing to report OCSE-396A (Part 1 Page 2)
      // ***
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-396A report (Part 2)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "P2";
    UseEabOcse396();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // ***
      // *** Error writing to report OCSE-396A (Part 2)
      // ***
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    //   ***
    //  ***
    // *** Generate OCSE-396A report (Part 3)
    //  ***
    //   ***
    local.ReportParms.Parm1 = "GR";
    local.ReportParms.Parm2 = "";
    local.ReportParms.SubreportCode = "P3";
    UseEabOcse396();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // ***
      // *** Error writing to report OCSE-396A (Part 3)
      // ***
      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    //   ***
    //  ***
    // *** Close OCSE-396A report
    //  ***
    //   ***
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabOcse396();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // ***
      // *** Error closing report OCSE-396A
      // ***
      ExitState = "FILE_WRITE_ERROR_RB";
    }
  }

  private void UseEabOcse396()
  {
    var useImport = new EabOcse396.Import();
    var useExport = new EabOcse396.Export();

    useImport.ReportParms.Assign(local.ReportParms);
    useImport.Ocse34.Assign(export.Ocse34);
    useExport.ReportParms.Assign(local.ReportParms);

    Call(EabOcse396.Execute, useImport, useExport);

    local.ReportParms.Assign(useExport.ReportParms);
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "period", local.Work.Period);
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.OtherStatesCurrentIvaAmount =
          db.GetNullableInt32(reader, 1);
        entities.Ocse34.OtherStatesCurrentIveAmount =
          db.GetNullableInt32(reader, 2);
        entities.Ocse34.OtherstateFormerAssistAmount =
          db.GetNullableInt32(reader, 3);
        entities.Ocse34.OtherStateNeverAssistAmount =
          db.GetNullableInt32(reader, 4);
        entities.Ocse34.TotalDistributedIvaAmount =
          db.GetNullableInt32(reader, 5);
        entities.Ocse34.TotalDistributedIveAmount =
          db.GetNullableInt32(reader, 6);
        entities.Ocse34.TotalDistributedFormerAmount =
          db.GetNullableInt32(reader, 7);
        entities.Ocse34.TotalDistributedNeverAmount =
          db.GetNullableInt32(reader, 8);
        entities.Ocse34.TotalDistributedAmount = db.GetNullableInt32(reader, 9);
        entities.Ocse34.NetFederalShareAmount = db.GetNullableInt32(reader, 10);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.Ocse34.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
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
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Ocse34 Work
    {
      get => work ??= new();
      set => work = value;
    }

    private ReportParms reportParms;
    private Ocse34 work;
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

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Ocse34 ocse34;
  }
#endregion
}
