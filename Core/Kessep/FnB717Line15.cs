// Program: FN_B717_LINE_15, ID: 373358672, model: 746.
// Short name: SWE03006
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_15.
/// </summary>
[Serializable]
public partial class FnB717Line15: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_15 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line15(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line15.
  /// </summary>
  public FnB717Line15(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.Create.YearMonth = import.StatsReport.YearMonth.GetValueOrDefault();
    local.Create.FirstRunNumber =
      import.StatsReport.FirstRunNumber.GetValueOrDefault();
    local.PrevReportEndDate.Date = AddDays(import.ReportStartDate.Date, -1);

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "15 "))
    {
      local.RestartOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 4));
      local.RestartServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 8, 5));
      local.RestartCase.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 13, 10);
    }

    // -------------------------------------------------------------------
    // Read cases where assignment was ended during report month.
    // --------------------------------------------------------------------
    foreach(var item in ReadCaseCaseAssignmentOfficeOfficeServiceProvider())
    {
      if (Equal(entities.Case1.Number, local.PrevCase.Number))
      {
        continue;
      }

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() || entities
        .ServiceProvider.SystemGeneratedId != local
        .PrevServiceProvider.SystemGeneratedId)
      {
        UseFnB717UpdateCounts();
        UseFnB717ClearViews();
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "15 " + NumberToString
          (local.PrevOffice.SystemGeneratedId, 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.PrevServiceProvider.SystemGeneratedId, 11, 5);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .PrevCase.Number;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Error.LineNumber = 15;
          local.Error.CaseNumber = entities.Case1.Number;
          local.Error.OfficeId = entities.Office.SystemGeneratedId;
          UseFnB717WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      // -------------------------------------------------------------------
      // Case must be closed as of end of report month.
      // -------------------------------------------------------------------
      if (AsChar(entities.Case1.Status) == 'O')
      {
        continue;
      }

      // -------------------------------------------------------------------
      // Skip if Case is later assigned to another worker in the same month.
      // -------------------------------------------------------------------
      if (ReadCaseAssignmentServiceProvider())
      {
        continue;
      }

      local.CaseClosureDate.Date = entities.CaseAssignment.DiscontinueDate;
      UseFnBuildTimestampFrmDateTime();
      local.CaseClosureDate.Timestamp =
        AddHours(AddDays(local.CaseClosureDate.Timestamp, 1), 7);
      UseFnB717ProgHierarchy4Case();

      // -------------------------------------------------------------------
      // Case must be open as of last day of prev month or
      // it must be a new case.
      // -------------------------------------------------------------------
      if (!ReadCaseAssignment1())
      {
        if (ReadCaseAssignment2())
        {
          continue;
        }
        else
        {
          // ------------------------
          // New case.
          // ------------------------
        }
      }

      local.PrevCase.Number = entities.Case1.Number;
      local.PrevOffice.SystemGeneratedId = entities.Office.SystemGeneratedId;
      local.PrevServiceProvider.SystemGeneratedId =
        entities.ServiceProvider.SystemGeneratedId;
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        local.PrevOfficeServiceProvider);
      local.Subscript.Count = 15;
      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.CaseNumber = entities.Case1.Number;
        local.Create.CaseWrkRole = entities.OfficeServiceProvider.RoleCode;
        local.Create.ServicePrvdrId =
          entities.ServiceProvider.SystemGeneratedId;
        local.Create.OfficeId = entities.Office.SystemGeneratedId;
        local.Create.ProgramType = local.Program.Code;
        UseFnB717GetSupervisorSp();
        local.Create.LineNumber = 15;
        local.Create.ParentId = local.Sup.SystemGeneratedId;
        UseFnB717CreateStatsVerifi();
      }

      ++local.CommitCnt.Count;
    }

    UseFnB717UpdateCounts();
    UseFnB717ClearViews();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "17  " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Error.LineNumber = 15;
      UseFnB717WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveGroup1(FnB717ClearViews.Export.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(FnB717InflateGv.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup3(Import.GroupGroup source,
    FnB717UpdateCounts.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup4(Import.GroupGroup source,
    FnB717InflateGv.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStatsReport(StatsReport source, StatsReport target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
  }

  private static void MoveStatsVerifi(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.LineNumber = source.LineNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.CaseWrkRole = source.CaseWrkRole;
    target.ParentId = source.ParentId;
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
  }

  private void UseFnB717ClearViews()
  {
    var useImport = new FnB717ClearViews.Import();
    var useExport = new FnB717ClearViews.Export();

    Call(FnB717ClearViews.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private void UseFnB717CreateStatsVerifi()
  {
    var useImport = new FnB717CreateStatsVerifi.Import();
    var useExport = new FnB717CreateStatsVerifi.Export();

    MoveStatsVerifi(local.Create, useImport.StatsVerifi);

    Call(FnB717CreateStatsVerifi.Execute, useImport, useExport);
  }

  private void UseFnB717GetSupervisorSp()
  {
    var useImport = new FnB717GetSupervisorSp.Import();
    var useExport = new FnB717GetSupervisorSp.Export();

    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;

    Call(FnB717GetSupervisorSp.Execute, useImport, useExport);

    local.Sup.SystemGeneratedId = useExport.Sup.SystemGeneratedId;
  }

  private void UseFnB717InflateGv()
  {
    var useImport = new FnB717InflateGv.Import();
    var useExport = new FnB717InflateGv.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup4);
    MoveProgram(local.Program, useImport.Program);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup2);
  }

  private void UseFnB717ProgHierarchy4Case()
  {
    var useImport = new FnB717ProgHierarchy4Case.Import();
    var useExport = new FnB717ProgHierarchy4Case.Export();

    useImport.Case1.Assign(entities.Case1);
    MoveDateWorkArea(local.CaseClosureDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Case.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717UpdateCounts()
  {
    var useImport = new FnB717UpdateCounts.Import();
    var useExport = new FnB717UpdateCounts.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup3);
    MoveStatsReport(import.StatsReport, useImport.StatsReport);
    useImport.Office.SystemGeneratedId = local.PrevOffice.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      local.PrevServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(local.PrevOfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(FnB717UpdateCounts.Execute, useImport, useExport);
  }

  private void UseFnB717WriteError()
  {
    var useImport = new FnB717WriteError.Import();
    var useExport = new FnB717WriteError.Export();

    useImport.Error.Assign(local.Error);

    Call(FnB717WriteError.Execute, useImport, useExport);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.CaseClosureDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.CaseClosureDate);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCaseAssignment1()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.PrevReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseAssignment2()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.NextCaseAssignment.Populated = false;
    entities.NextServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetInt32(command, "spdId", entities.ServiceProvider.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate1",
          entities.CaseAssignment.DiscontinueDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NextCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.NextCaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.NextCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.NextCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.NextCaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.NextServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.NextCaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.NextCaseAssignment.OspCode = db.GetString(reader, 6);
        entities.NextCaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.NextCaseAssignment.CasNo = db.GetString(reader, 8);
        entities.NextCaseAssignment.Populated = true;
        entities.NextServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseAssignmentOfficeOfficeServiceProvider()
  {
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadCaseCaseAssignmentOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.
          SetInt32(command, "officeId1", local.RestartOffice.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdId", local.RestartServiceProvider.SystemGeneratedId);
        db.SetString(command, "numb", local.RestartCase.Number);
        db.SetNullableInt32(
          command, "officeId2", import.From.OfficeId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "officeId3", import.To.OfficeId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 2);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 6);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 6);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 7);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 7);
        entities.CaseAssignment.OspCode = db.GetString(reader, 8);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 8);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 9);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 9);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 10);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 11);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 35;

      private StatsReport statsReport;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsReport To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsReport From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    private StatsReport to;
    private StatsReport from;
    private DateWorkArea reportStartDate;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<GroupGroup> group;
    private StatsReport statsReport;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PrevReportEndDate.
    /// </summary>
    [JsonPropertyName("prevReportEndDate")]
    public DateWorkArea PrevReportEndDate
    {
      get => prevReportEndDate ??= new();
      set => prevReportEndDate = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
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

    /// <summary>
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
    }

    /// <summary>
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
    }

    /// <summary>
    /// A value of RestartServiceProvider.
    /// </summary>
    [JsonPropertyName("restartServiceProvider")]
    public ServiceProvider RestartServiceProvider
    {
      get => restartServiceProvider ??= new();
      set => restartServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevServiceProvider.
    /// </summary>
    [JsonPropertyName("prevServiceProvider")]
    public ServiceProvider PrevServiceProvider
    {
      get => prevServiceProvider ??= new();
      set => prevServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("prevOfficeServiceProvider")]
    public OfficeServiceProvider PrevOfficeServiceProvider
    {
      get => prevOfficeServiceProvider ??= new();
      set => prevOfficeServiceProvider = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of CaseClosureDate.
    /// </summary>
    [JsonPropertyName("caseClosureDate")]
    public DateWorkArea CaseClosureDate
    {
      get => caseClosureDate ??= new();
      set => caseClosureDate = value;
    }

    /// <summary>
    /// A value of Sup.
    /// </summary>
    [JsonPropertyName("sup")]
    public ServiceProvider Sup
    {
      get => sup ??= new();
      set => sup = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public StatsVerifi Error
    {
      get => error ??= new();
      set => error = value;
    }

    private DateWorkArea prevReportEndDate;
    private Case1 prevCase;
    private Case1 restartCase;
    private Common commitCnt;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Office prevOffice;
    private Office restartOffice;
    private ServiceProvider restartServiceProvider;
    private ServiceProvider prevServiceProvider;
    private OfficeServiceProvider prevOfficeServiceProvider;
    private Program program;
    private Common subscript;
    private StatsVerifi create;
    private DateWorkArea caseClosureDate;
    private ServiceProvider sup;
    private StatsVerifi error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NextCaseAssignment.
    /// </summary>
    [JsonPropertyName("nextCaseAssignment")]
    public CaseAssignment NextCaseAssignment
    {
      get => nextCaseAssignment ??= new();
      set => nextCaseAssignment = value;
    }

    /// <summary>
    /// A value of NextOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("nextOfficeServiceProvider")]
    public OfficeServiceProvider NextOfficeServiceProvider
    {
      get => nextOfficeServiceProvider ??= new();
      set => nextOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NextServiceProvider.
    /// </summary>
    [JsonPropertyName("nextServiceProvider")]
    public ServiceProvider NextServiceProvider
    {
      get => nextServiceProvider ??= new();
      set => nextServiceProvider = value;
    }

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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private CaseAssignment nextCaseAssignment;
    private OfficeServiceProvider nextOfficeServiceProvider;
    private ServiceProvider nextServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
  }
#endregion
}
