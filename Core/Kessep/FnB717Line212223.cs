// Program: FN_B717_LINE_21_22_23, ID: 373354062, model: 746.
// Short name: SWE03010
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_21_22_23.
/// </summary>
[Serializable]
public partial class FnB717Line212223: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_21_22_23 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line212223(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line212223.
  /// </summary>
  public FnB717Line212223(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveStatsVerifi2(import.Create, local.Create);
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.IncludeInLine22.Flag = "N";

    foreach(var item in ReadMonitoredActivityInfrastructure())
    {
      if (Equal(entities.Infrastructure.ReasonCode, "OSPRVWS"))
      {
        if (!Lt(Date(entities.MonitoredActivity.CreatedTimestamp),
          import.ReportStartDate.Date) && !
          Lt(import.ReportEndDate.Date,
          Date(entities.MonitoredActivity.CreatedTimestamp)))
        {
          local.Subscript.Count = 21;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 21;
            UseFnB717CreateStatsVerifi();
          }

          return;
        }
        else if (Equal(entities.MonitoredActivity.ClosureDate,
          local.MaxDate.Date) && Lt
          (entities.MonitoredActivity.OtherNonComplianceDate,
          import.ReportEndDate.Date))
        {
          local.Subscript.Count = 22;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 22;
            UseFnB717CreateStatsVerifi();
          }

          return;
        }
      }

      if (Equal(entities.MonitoredActivity.ClosureDate, local.MaxDate.Date) && Lt
        (entities.MonitoredActivity.OtherNonComplianceDate,
        import.ReportEndDate.Date))
      {
        local.IncludeInLine22.Flag = "Y";
      }
    }

    if (AsChar(local.IncludeInLine22.Flag) == 'Y')
    {
      local.Subscript.Count = 22;
      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 22;
        UseFnB717CreateStatsVerifi();
      }

      return;
    }

    if (ReadInfrastructure())
    {
      return;
    }

    // ----------------------------------
    // Is case more than a year old?
    // ----------------------------------
    ReadCaseAssignment();

    if (!Lt(entities.CaseAssignment.EffectiveDate,
      AddYears(import.ReportEndDate.Date, -1)))
    {
      return;
    }

    local.Subscript.Count = 23;
    UseFnB717InflateGv();

    if (AsChar(import.DisplayInd.Flag) == 'Y')
    {
      local.Create.LineNumber = 23;
      UseFnB717CreateStatsVerifi();
    }
  }

  private static void MoveGroup1(FnB717InflateGv.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(Import.GroupGroup source,
    FnB717InflateGv.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStatsVerifi1(StatsVerifi source, StatsVerifi target)
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
  }

  private static void MoveStatsVerifi2(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.ProgramType = source.ProgramType;
    target.ServicePrvdrId = source.ServicePrvdrId;
    target.OfficeId = source.OfficeId;
    target.CaseWrkRole = source.CaseWrkRole;
    target.ParentId = source.ParentId;
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
  }

  private void UseFnB717CreateStatsVerifi()
  {
    var useImport = new FnB717CreateStatsVerifi.Import();
    var useExport = new FnB717CreateStatsVerifi.Export();

    MoveStatsVerifi1(local.Create, useImport.StatsVerifi);

    Call(FnB717CreateStatsVerifi.Execute, useImport, useExport);
  }

  private void UseFnB717InflateGv()
  {
    var useImport = new FnB717InflateGv.Import();
    var useExport = new FnB717InflateGv.Export();

    MoveProgram(import.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.KeyOnlyInfrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.KeyOnlyInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.KeyOnlyInfrastructure.CaseNumber =
          db.GetNullableString(reader, 1);
        entities.KeyOnlyInfrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityInfrastructure()
  {
    entities.MonitoredActivity.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 3);
        entities.MonitoredActivity.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 5);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Infrastructure.EventId = db.GetInt32(reader, 6);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 7);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 8);
        entities.MonitoredActivity.Populated = true;
        entities.Infrastructure.Populated = true;

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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
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

    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Case1 case1;
    private Program program;
    private Array<GroupGroup> group;
    private StatsVerifi create;
    private Common displayInd;
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
    /// <summary>
    /// A value of IncludeInLine22.
    /// </summary>
    [JsonPropertyName("includeInLine22")]
    public Common IncludeInLine22
    {
      get => includeInLine22 ??= new();
      set => includeInLine22 = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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

    private Common includeInLine22;
    private DateWorkArea maxDate;
    private Common subscript;
    private StatsVerifi create;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyOnlyMonitoredActivity.
    /// </summary>
    [JsonPropertyName("keyOnlyMonitoredActivity")]
    public MonitoredActivity KeyOnlyMonitoredActivity
    {
      get => keyOnlyMonitoredActivity ??= new();
      set => keyOnlyMonitoredActivity = value;
    }

    /// <summary>
    /// A value of KeyOnlyInfrastructure.
    /// </summary>
    [JsonPropertyName("keyOnlyInfrastructure")]
    public Infrastructure KeyOnlyInfrastructure
    {
      get => keyOnlyInfrastructure ??= new();
      set => keyOnlyInfrastructure = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    private MonitoredActivity keyOnlyMonitoredActivity;
    private Infrastructure keyOnlyInfrastructure;
    private CaseAssignment caseAssignment;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private LegalReferral legalReferral;
  }
#endregion
}
