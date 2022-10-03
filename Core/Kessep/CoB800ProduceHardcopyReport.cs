// Program: CO_B800_PRODUCE_HARDCOPY_REPORT, ID: 371136859, model: 746.
// Short name: SWEE800B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CO_B800_PRODUCE_HARDCOPY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class CoB800ProduceHardcopyReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_B800_PRODUCE_HARDCOPY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoB800ProduceHardcopyReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoB800ProduceHardcopyReport.
  /// </summary>
  public CoB800ProduceHardcopyReport(IContext context, Import import,
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
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 12/11/00  Alan Doty              Initial Development
    // --------------------------------------------------------------------
    // *****************************************************************
    // resp:  Common
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.EndOfReportMsg.LineText = "END OF REPORT";

    // *****************************************************************
    // Get the SYSIN Parm Values
    // *****************************************************************
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.Sysin.ParameterList))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.Job.Name = Substring(local.Sysin.ParameterList, 1, 8);

    if (!ReadJob())
    {
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
      ExitState = "CO0000_JOB_RUN_NF_AB";

      return;
    }

    local.MaxLinesPerPage.Count =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 20, 3));

    if (local.MaxLinesPerPage.Count == 0)
    {
      local.MaxLinesPerPage.Count = 60;
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    try
    {
      UpdateJobRun3();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_JOB_RUN_NU_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_JOB_RUN_PV_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Perform a DB2 Commit to Free Up the JOB_RUN row.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    // *****************************************************************
    // Build the Header Lines
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    UseCoB800HardcopyReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      try
      {
        UpdateJobRun2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // : Perform a DB2 Commit to Free Up the JOB_RUN row.
      UseExtToDoACommit();

      if (local.External.NumericReturnCode != 0)
      {
        ExitState = "FN0000_SYSIN_PARM_ERROR_A";

        return;
      }

      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // *****************************************************************
    // Build the Header Lines
    // *****************************************************************
    local.Header.Index = 0;
    local.Header.Clear();

    foreach(var item in ReadReportData2())
    {
      local.Header.Update.Header1.Assign(entities.ExistingReportData);
      local.Header.Next();
    }

    UseCoB800PrintHeading2();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****************************************************************
    // Process the Report Detail Lines
    // *****************************************************************
    local.EabFileHandling.Action = "WRITE";

    foreach(var item in ReadReportData1())
    {
      // : New page needed?
      if (local.NextLineNo.Count > local.MaxLinesPerPage.Count)
      {
        local.ReportData.LineControl = "PG";
      }
      else
      {
        local.ReportData.LineControl = entities.ExistingReportData.LineControl;
      }

      // : Check the Line Control (New Page, Skip Lines, etc.)
      switch(TrimEnd(local.ReportData.LineControl))
      {
        case "":
          break;
        case "PG":
          UseCoB800PrintHeading1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          break;
        default:
          if (Verify(local.ReportData.LineControl, "0123456789") == 0)
          {
            local.BlankLine.Count =
              (int)StringToNumber(local.ReportData.LineControl);
            local.Tmp.Count = 1;

            for(var limit = local.BlankLine.Count; local.Tmp.Count <= limit; ++
              local.Tmp.Count)
            {
              if (local.NextLineNo.Count > local.MaxLinesPerPage.Count)
              {
                UseCoB800PrintHeading1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                goto Test;
              }

              UseCoB800HardcopyReport4();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ERROR_WRITING_TO_REPORT_AB";

                return;
              }

              ++local.NextLineNo.Count;
            }
          }

          break;
      }

Test:

      if (local.NextLineNo.Count > local.MaxLinesPerPage.Count)
      {
        UseCoB800PrintHeading1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      UseCoB800HardcopyReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      ++local.NextLineNo.Count;
    }

    // : Print the report completion message
    if (local.NextLineNo.Count >= local.MaxLinesPerPage.Count)
    {
      UseCoB800PrintHeading1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      UseCoB800HardcopyReport4();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }
    }

    UseCoB800HardcopyReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_WRITING_TO_REPORT_AB";

      return;
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    try
    {
      UpdateJobRun1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_JOB_RUN_NU_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_JOB_RUN_PV_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Perform a DB2 Commit to Free Up the JOB_RUN row.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCoB800HardcopyReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_WRITING_TO_REPORT_AB";
    }
  }

  private static void MoveHeader(Local.HeaderGroup source,
    CoB800PrintHeading.Import.HeaderGroup target)
  {
    target.Header1.Assign(source.Header1);
  }

  private void UseCoB800HardcopyReport1()
  {
    var useImport = new CoB800HardcopyReport.Import();
    var useExport = new CoB800HardcopyReport.Export();

    useImport.ReportData.LineText = entities.ExistingReportData.LineText;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(CoB800HardcopyReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCoB800HardcopyReport2()
  {
    var useImport = new CoB800HardcopyReport.Import();
    var useExport = new CoB800HardcopyReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(CoB800HardcopyReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCoB800HardcopyReport3()
  {
    var useImport = new CoB800HardcopyReport.Import();
    var useExport = new CoB800HardcopyReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ReportData.LineText = local.EndOfReportMsg.LineText;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(CoB800HardcopyReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCoB800HardcopyReport4()
  {
    var useImport = new CoB800HardcopyReport.Import();
    var useExport = new CoB800HardcopyReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ReportData.LineText = local.NullReportData.LineText;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(CoB800HardcopyReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCoB800PrintHeading1()
  {
    var useImport = new CoB800PrintHeading.Import();
    var useExport = new CoB800PrintHeading.Export();

    useImport.MaxLinesPerPage.Count = local.MaxLinesPerPage.Count;
    useImport.PageNo.Count = local.PageNo.Count;
    useImport.NextLineNo.Count = local.NextLineNo.Count;
    local.Header.CopyTo(useImport.Header, MoveHeader);

    Call(CoB800PrintHeading.Execute, useImport, useExport);

    local.PageNo.Count = useExport.PageNo.Count;
    local.NextLineNo.Count = useExport.NextLineNo.Count;
  }

  private void UseCoB800PrintHeading2()
  {
    var useImport = new CoB800PrintHeading.Import();
    var useExport = new CoB800PrintHeading.Export();

    useImport.MaxLinesPerPage.Count = local.MaxLinesPerPage.Count;
    local.Header.CopyTo(useImport.Header, MoveHeader);

    Call(CoB800PrintHeading.Execute, useImport, useExport);

    local.PageNo.Count = useExport.PageNo.Count;
    local.NextLineNo.Count = useExport.NextLineNo.Count;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList = local.Sysin.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.Sysin.ParameterList = useExport.ProgramProcessingInfo.ParameterList;
  }

  private bool ReadJob()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", local.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Populated = true;
      });
  }

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetInt32(command, "systemGenId", local.JobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 3);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 4);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 5);
        entities.ExistingJobRun.JobName = db.GetString(reader, 6);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 7);
        entities.ExistingJobRun.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);
    entities.ExistingReportData.Populated = false;

    return ReadEach("ReadReportData1",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.SetInt32(
          command, "jruSystemGenId", entities.ExistingJobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingReportData.Type1 = db.GetString(reader, 0);
        entities.ExistingReportData.SequenceNumber = db.GetInt32(reader, 1);
        entities.ExistingReportData.FirstPageOnlyInd =
          db.GetNullableString(reader, 2);
        entities.ExistingReportData.LineControl = db.GetString(reader, 3);
        entities.ExistingReportData.LineText = db.GetString(reader, 4);
        entities.ExistingReportData.JobName = db.GetString(reader, 5);
        entities.ExistingReportData.JruSystemGenId = db.GetInt32(reader, 6);
        entities.ExistingReportData.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadReportData2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    return ReadEach("ReadReportData2",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.SetInt32(
          command, "jruSystemGenId", entities.ExistingJobRun.SystemGenId);
      },
      (db, reader) =>
      {
        if (local.Header.IsFull)
        {
          return false;
        }

        entities.ExistingReportData.Type1 = db.GetString(reader, 0);
        entities.ExistingReportData.SequenceNumber = db.GetInt32(reader, 1);
        entities.ExistingReportData.FirstPageOnlyInd =
          db.GetNullableString(reader, 2);
        entities.ExistingReportData.LineControl = db.GetString(reader, 3);
        entities.ExistingReportData.LineText = db.GetString(reader, 4);
        entities.ExistingReportData.JobName = db.GetString(reader, 5);
        entities.ExistingReportData.JruSystemGenId = db.GetInt32(reader, 6);
        entities.ExistingReportData.Populated = true;

        return true;
      });
  }

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var endTimestamp = Now();
    var status = "COMPLETE";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "endTimestamp", endTimestamp);
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.EndTimestamp = endTimestamp;
    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "ERROR";
    var errorMsg = "ERROR: Problem Opening the Report File";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetNullableString(command, "errorMsg", errorMsg);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.ErrorMsg = errorMsg;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "HARDCOPY";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun3",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of Header1.
      /// </summary>
      [JsonPropertyName("header1")]
      public ReportData Header1
      {
        get => header1 ??= new();
        set => header1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ReportData header1;
    }

    /// <summary>
    /// A value of MaxLinesPerPage.
    /// </summary>
    [JsonPropertyName("maxLinesPerPage")]
    public Common MaxLinesPerPage
    {
      get => maxLinesPerPage ??= new();
      set => maxLinesPerPage = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of BlankLine.
    /// </summary>
    [JsonPropertyName("blankLine")]
    public Common BlankLine
    {
      get => blankLine ??= new();
      set => blankLine = value;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
    }

    /// <summary>
    /// A value of PageNo.
    /// </summary>
    [JsonPropertyName("pageNo")]
    public Common PageNo
    {
      get => pageNo ??= new();
      set => pageNo = value;
    }

    /// <summary>
    /// A value of NextLineNo.
    /// </summary>
    [JsonPropertyName("nextLineNo")]
    public Common NextLineNo
    {
      get => nextLineNo ??= new();
      set => nextLineNo = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of EndOfReportMsg.
    /// </summary>
    [JsonPropertyName("endOfReportMsg")]
    public ReportData EndOfReportMsg
    {
      get => endOfReportMsg ??= new();
      set => endOfReportMsg = value;
    }

    /// <summary>
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
    }

    /// <summary>
    /// A value of Sysin.
    /// </summary>
    [JsonPropertyName("sysin")]
    public ProgramProcessingInfo Sysin
    {
      get => sysin ??= new();
      set => sysin = value;
    }

    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
    }

    /// <summary>
    /// A value of OrderBy.
    /// </summary>
    [JsonPropertyName("orderBy")]
    public TextWorkArea OrderBy
    {
      get => orderBy ??= new();
      set => orderBy = value;
    }

    private Common maxLinesPerPage;
    private Common tmp;
    private Common blankLine;
    private ReportData reportData;
    private Common pageNo;
    private Common nextLineNo;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private External external;
    private EabFileHandling eabFileHandling;
    private ReportData endOfReportMsg;
    private ReportData nullReportData;
    private Array<HeaderGroup> header;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private TextWorkArea orderBy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingJob.
    /// </summary>
    [JsonPropertyName("existingJob")]
    public Job ExistingJob
    {
      get => existingJob ??= new();
      set => existingJob = value;
    }

    /// <summary>
    /// A value of ExistingJobRun.
    /// </summary>
    [JsonPropertyName("existingJobRun")]
    public JobRun ExistingJobRun
    {
      get => existingJobRun ??= new();
      set => existingJobRun = value;
    }

    /// <summary>
    /// A value of ExistingReportData.
    /// </summary>
    [JsonPropertyName("existingReportData")]
    public ReportData ExistingReportData
    {
      get => existingReportData ??= new();
      set => existingReportData = value;
    }

    private Job existingJob;
    private JobRun existingJobRun;
    private ReportData existingReportData;
  }
#endregion
}
