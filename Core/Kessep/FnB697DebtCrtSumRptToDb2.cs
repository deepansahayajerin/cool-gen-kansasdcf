// Program: FN_B697_DEBT_CRT_SUM_RPT_TO_DB2, ID: 371132914, model: 746.
// Short name: SWEF697B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B697_DEBT_CRT_SUM_RPT_TO_DB2.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB697DebtCrtSumRptToDb2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B697_DEBT_CRT_SUM_RPT_TO_DB2 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB697DebtCrtSumRptToDb2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB697DebtCrtSumRptToDb2.
  /// </summary>
  public FnB697DebtCrtSumRptToDb2(IContext context, Import import, Export export)
    :
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
    // 12/15/00  Maureen Brown              Initial Development
    // --------------------------------------------------------------------
    // *****************************************************************
    // resp:  Finance
    // This procedure reads a file of report records and writes it to the DB2 
    // REPORT_DATA table.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Group.Index = -1;
    local.Error.Action = "OPEN";
    UseCabErrorReport1();
    local.Error.Action = "WRITE";
    local.EabFileHandling.Action = "OPEN";
    UseFnSwex0003ReadReportFile2();
    local.EabFileHandling.Action = "READ";

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
      local.NeededToWrite.RptDetail = local.Sysin.ParameterList ?? Spaces(132);
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
      local.NeededToWrite.RptDetail = "Job Run Not Found";
      UseCabErrorReport2();
      ExitState = "CO0000_JOB_RUN_NF_AB";

      return;
    }

    // *****************************************************************
    // Mainline Process
    // *****************************************************************
    do
    {
      UseFnSwex0003ReadReportFile1();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
      }
      else if (Equal(local.EabFileHandling.Status, "EOF"))
      {
        break;
      }
      else
      {
        local.NeededToWrite.RptDetail =
          "Problem reading input file.  Status is: " + local
          .EabFileHandling.Status;
        UseCabErrorReport3();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      try
      {
        CreateReportData();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_REPORT_DATA_AE_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_REPORT_DATA_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(Equal(local.EabFileHandling.Status, "OK"));

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    if (Equal(entities.ExistingJobRun.OutputType, "ONLINE"))
    {
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

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
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

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.Error.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Error.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Error.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.ProgramProcessingInfo.ParameterList = local.Sysin.ParameterList;
    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.Sysin.ParameterList = useExport.ProgramProcessingInfo.ParameterList;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnSwex0003ReadReportFile1()
  {
    var useImport = new FnSwex0003ReadReportFile.Import();
    var useExport = new FnSwex0003ReadReportFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.ReportData.Assign(local.ReportData);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnSwex0003ReadReportFile.Execute, useImport, useExport);

    local.ReportData.Assign(useExport.ReportData);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnSwex0003ReadReportFile2()
  {
    var useImport = new FnSwex0003ReadReportFile.Import();
    var useExport = new FnSwex0003ReadReportFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnSwex0003ReadReportFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void CreateReportData()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = local.ReportData.Type1;
    var sequenceNumber = local.ReportData.SequenceNumber;
    var firstPageOnlyInd = local.ReportData.FirstPageOnlyInd ?? "";
    var lineControl = local.ReportData.LineControl;
    var lineText = local.ReportData.LineText;
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = lineControl;
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
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
        db.SetString(command, "jobName", entities.ExistingJob.Name);
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

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "COMPLETE";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun1",
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

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "WAIT";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun2",
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePersonsWorkSet Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>
      /// A value of Ar.
      /// </summary>
      [JsonPropertyName("ar")]
      public CsePersonsWorkSet Ar
      {
        get => ar ??= new();
        set => ar = value;
      }

      /// <summary>
      /// A value of CaseAssignment.
      /// </summary>
      [JsonPropertyName("caseAssignment")]
      public CaseAssignment CaseAssignment
      {
        get => caseAssignment ??= new();
        set => caseAssignment = value;
      }

      /// <summary>
      /// A value of Cau.
      /// </summary>
      [JsonPropertyName("cau")]
      public Common Cau
      {
        get => cau ??= new();
        set => cau = value;
      }

      /// <summary>
      /// A value of Func.
      /// </summary>
      [JsonPropertyName("func")]
      public TextWorkArea Func
      {
        get => func ??= new();
        set => func = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private Case1 case1;
      private CsePersonsWorkSet ap;
      private CsePersonsWorkSet ar;
      private CaseAssignment caseAssignment;
      private Common cau;
      private TextWorkArea func;
      private Program program;
    }

    /// <summary>A AGroup group.</summary>
    [Serializable]
    public class AGroup
    {
      /// <summary>
      /// A value of Acase.
      /// </summary>
      [JsonPropertyName("acase")]
      public Case1 Acase
      {
        get => acase ??= new();
        set => acase = value;
      }

      /// <summary>
      /// A value of ApA.
      /// </summary>
      [JsonPropertyName("apA")]
      public CsePersonsWorkSet ApA
      {
        get => apA ??= new();
        set => apA = value;
      }

      /// <summary>
      /// A value of ArA.
      /// </summary>
      [JsonPropertyName("arA")]
      public CsePersonsWorkSet ArA
      {
        get => arA ??= new();
        set => arA = value;
      }

      /// <summary>
      /// A value of AcaseAssignment.
      /// </summary>
      [JsonPropertyName("acaseAssignment")]
      public CaseAssignment AcaseAssignment
      {
        get => acaseAssignment ??= new();
        set => acaseAssignment = value;
      }

      /// <summary>
      /// A value of Acau.
      /// </summary>
      [JsonPropertyName("acau")]
      public Common Acau
      {
        get => acau ??= new();
        set => acau = value;
      }

      /// <summary>
      /// A value of Afunc.
      /// </summary>
      [JsonPropertyName("afunc")]
      public TextWorkArea Afunc
      {
        get => afunc ??= new();
        set => afunc = value;
      }

      /// <summary>
      /// A value of Aprogram.
      /// </summary>
      [JsonPropertyName("aprogram")]
      public Program Aprogram
      {
        get => aprogram ??= new();
        set => aprogram = value;
      }

      private Case1 acase;
      private CsePersonsWorkSet apA;
      private CsePersonsWorkSet arA;
      private CaseAssignment acaseAssignment;
      private Common acau;
      private TextWorkArea afunc;
      private Program aprogram;
    }

    /// <summary>A BGroup group.</summary>
    [Serializable]
    public class BGroup
    {
      /// <summary>
      /// A value of Bcase.
      /// </summary>
      [JsonPropertyName("bcase")]
      public Case1 Bcase
      {
        get => bcase ??= new();
        set => bcase = value;
      }

      /// <summary>
      /// A value of ApB.
      /// </summary>
      [JsonPropertyName("apB")]
      public CsePersonsWorkSet ApB
      {
        get => apB ??= new();
        set => apB = value;
      }

      /// <summary>
      /// A value of ArB.
      /// </summary>
      [JsonPropertyName("arB")]
      public CsePersonsWorkSet ArB
      {
        get => arB ??= new();
        set => arB = value;
      }

      /// <summary>
      /// A value of BcaseAssignment.
      /// </summary>
      [JsonPropertyName("bcaseAssignment")]
      public CaseAssignment BcaseAssignment
      {
        get => bcaseAssignment ??= new();
        set => bcaseAssignment = value;
      }

      /// <summary>
      /// A value of Bcau.
      /// </summary>
      [JsonPropertyName("bcau")]
      public Common Bcau
      {
        get => bcau ??= new();
        set => bcau = value;
      }

      /// <summary>
      /// A value of Bfunc.
      /// </summary>
      [JsonPropertyName("bfunc")]
      public TextWorkArea Bfunc
      {
        get => bfunc ??= new();
        set => bfunc = value;
      }

      /// <summary>
      /// A value of Bprogram.
      /// </summary>
      [JsonPropertyName("bprogram")]
      public Program Bprogram
      {
        get => bprogram ??= new();
        set => bprogram = value;
      }

      private Case1 bcase;
      private CsePersonsWorkSet apB;
      private CsePersonsWorkSet arB;
      private CaseAssignment bcaseAssignment;
      private Common bcau;
      private TextWorkArea bfunc;
      private Program bprogram;
    }

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
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
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

    /// <summary>
    /// A value of ParmServiceProvider.
    /// </summary>
    [JsonPropertyName("parmServiceProvider")]
    public ServiceProvider ParmServiceProvider
    {
      get => parmServiceProvider ??= new();
      set => parmServiceProvider = value;
    }

    /// <summary>
    /// A value of ParmApFi.
    /// </summary>
    [JsonPropertyName("parmApFi")]
    public TextWorkArea ParmApFi
    {
      get => parmApFi ??= new();
      set => parmApFi = value;
    }

    /// <summary>
    /// A value of ParmArFi.
    /// </summary>
    [JsonPropertyName("parmArFi")]
    public TextWorkArea ParmArFi
    {
      get => parmArFi ??= new();
      set => parmArFi = value;
    }

    /// <summary>
    /// A value of ParmAr.
    /// </summary>
    [JsonPropertyName("parmAr")]
    public WorkArea ParmAr
    {
      get => parmAr ??= new();
      set => parmAr = value;
    }

    /// <summary>
    /// A value of ParmApWorkArea.
    /// </summary>
    [JsonPropertyName("parmApWorkArea")]
    public WorkArea ParmApWorkArea
    {
      get => parmApWorkArea ??= new();
      set => parmApWorkArea = value;
    }

    /// <summary>
    /// A value of ParmProgram.
    /// </summary>
    [JsonPropertyName("parmProgram")]
    public Program ParmProgram
    {
      get => parmProgram ??= new();
      set => parmProgram = value;
    }

    /// <summary>
    /// A value of ParmCase.
    /// </summary>
    [JsonPropertyName("parmCase")]
    public Case1 ParmCase
    {
      get => parmCase ??= new();
      set => parmCase = value;
    }

    /// <summary>
    /// A value of ParmCaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("parmCaseFuncWorkSet")]
    public CaseFuncWorkSet ParmCaseFuncWorkSet
    {
      get => parmCaseFuncWorkSet ??= new();
      set => parmCaseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of AsgnCount.
    /// </summary>
    [JsonPropertyName("asgnCount")]
    public Common AsgnCount
    {
      get => asgnCount ??= new();
      set => asgnCount = value;
    }

    /// <summary>
    /// A value of FormattedServiceProvider.
    /// </summary>
    [JsonPropertyName("formattedServiceProvider")]
    public CsePersonsWorkSet FormattedServiceProvider
    {
      get => formattedServiceProvider ??= new();
      set => formattedServiceProvider = value;
    }

    /// <summary>
    /// A value of FormattedAp.
    /// </summary>
    [JsonPropertyName("formattedAp")]
    public CsePersonsWorkSet FormattedAp
    {
      get => formattedAp ??= new();
      set => formattedAp = value;
    }

    /// <summary>
    /// A value of FormattedAr.
    /// </summary>
    [JsonPropertyName("formattedAr")]
    public CsePersonsWorkSet FormattedAr
    {
      get => formattedAr ??= new();
      set => formattedAr = value;
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
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApNameLength.
    /// </summary>
    [JsonPropertyName("apNameLength")]
    public Common ApNameLength
    {
      get => apNameLength ??= new();
      set => apNameLength = value;
    }

    /// <summary>
    /// A value of ArNameLength.
    /// </summary>
    [JsonPropertyName("arNameLength")]
    public Common ArNameLength
    {
      get => arNameLength ??= new();
      set => arNameLength = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Program Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of ApCompare.
    /// </summary>
    [JsonPropertyName("apCompare")]
    public CsePersonsWorkSet ApCompare
    {
      get => apCompare ??= new();
      set => apCompare = value;
    }

    /// <summary>
    /// A value of LoopAgain.
    /// </summary>
    [JsonPropertyName("loopAgain")]
    public Common LoopAgain
    {
      get => loopAgain ??= new();
      set => loopAgain = value;
    }

    /// <summary>
    /// Gets a value of A.
    /// </summary>
    [JsonPropertyName("a")]
    public AGroup A
    {
      get => a ?? (a = new());
      set => a = value;
    }

    /// <summary>
    /// Gets a value of B.
    /// </summary>
    [JsonPropertyName("b")]
    public BGroup B
    {
      get => b ?? (b = new());
      set => b = value;
    }

    /// <summary>
    /// A value of HardcodedOpen.
    /// </summary>
    [JsonPropertyName("hardcodedOpen")]
    public Case1 HardcodedOpen
    {
      get => hardcodedOpen ??= new();
      set => hardcodedOpen = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of ParmLegalReferral.
    /// </summary>
    [JsonPropertyName("parmLegalReferral")]
    public LegalReferral ParmLegalReferral
    {
      get => parmLegalReferral ??= new();
      set => parmLegalReferral = value;
    }

    /// <summary>
    /// A value of ParmReferralReason.
    /// </summary>
    [JsonPropertyName("parmReferralReason")]
    public TextWorkArea ParmReferralReason
    {
      get => parmReferralReason ??= new();
      set => parmReferralReason = value;
    }

    /// <summary>
    /// A value of ParmShowOnly.
    /// </summary>
    [JsonPropertyName("parmShowOnly")]
    public TextWorkArea ParmShowOnly
    {
      get => parmShowOnly ??= new();
      set => parmShowOnly = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity, 0);

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
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
    }

    /// <summary>
    /// A value of ParmApTextWorkArea.
    /// </summary>
    [JsonPropertyName("parmApTextWorkArea")]
    public TextWorkArea ParmApTextWorkArea
    {
      get => parmApTextWorkArea ??= new();
      set => parmApTextWorkArea = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public WorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public EabFileHandling Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private ReportData reportData;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private TextWorkArea orderBy;
    private ServiceProvider parmServiceProvider;
    private TextWorkArea parmApFi;
    private TextWorkArea parmArFi;
    private WorkArea parmAr;
    private WorkArea parmApWorkArea;
    private Program parmProgram;
    private Case1 parmCase;
    private CaseFuncWorkSet parmCaseFuncWorkSet;
    private Common asgnCount;
    private CsePersonsWorkSet formattedServiceProvider;
    private CsePersonsWorkSet formattedAp;
    private CsePersonsWorkSet formattedAr;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private Common apNameLength;
    private Common arNameLength;
    private Program tmp;
    private External external;
    private Array<GroupGroup> group;
    private CsePersonsWorkSet apCompare;
    private Common loopAgain;
    private AGroup a;
    private BGroup b;
    private Case1 hardcodedOpen;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Program program;
    private LegalReferral parmLegalReferral;
    private TextWorkArea parmReferralReason;
    private TextWorkArea parmShowOnly;
    private Array<HeaderGroup> header;
    private ReportData nullReportData;
    private TextWorkArea parmApTextWorkArea;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private WorkArea textDate;
    private EabReportSend neededToOpen;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling error;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
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
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public CsePerson ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ReportData New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Job existingJob;
    private JobRun existingJobRun;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private CsePerson existingKeyOnly;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
    private CaseUnit existingCaseUnit;
    private ReportData new1;
  }
#endregion
}
