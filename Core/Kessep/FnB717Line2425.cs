// Program: FN_B717_LINE_24_25, ID: 373354064, model: 746.
// Short name: SWE03011
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_24_25.
/// </summary>
[Serializable]
public partial class FnB717Line2425: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_24_25 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line2425(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line2425.
  /// </summary>
  public FnB717Line2425(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------
    // 6/1/02
    // - Include FADDRVRFDAP.
    // - Exclude APINCARC since JAIL also logs ADDRVRFPAP.
    // - Skip duplicate records.i.e.Same reason code, situation #.
    // --------------------------------------------------------
    MoveStatsVerifi2(import.Create, local.Create);

    foreach(var item in ReadInfrastructure())
    {
      if (Equal(entities.Infrastructure.ReasonCode, local.Prev.ReasonCode) && entities
        .Infrastructure.SituationNumber == local.Prev.SituationNumber && !
        Equal(entities.Infrastructure.CaseUnitNumber,
        local.Prev.CaseUnitNumber.GetValueOrDefault()))
      {
        continue;
      }

      if (Equal(entities.Infrastructure.ReasonCode, "INCSVRFD"))
      {
        foreach(var item1 in ReadIncomeSource())
        {
          if (Equal(entities.IncomeSource.EndDt, local.Blank.Date) || Lt
            (entities.IncomeSource.ReturnDt, entities.IncomeSource.EndDt))
          {
          }
          else
          {
            continue;
          }

          switch(AsChar(entities.IncomeSource.Type1))
          {
            case 'E':
              if (AsChar(entities.IncomeSource.ReturnCd) != 'E')
              {
                continue;
              }

              break;
            case 'M':
              if (AsChar(entities.IncomeSource.ReturnCd) != 'A' && AsChar
                (entities.IncomeSource.ReturnCd) != 'R')
              {
                continue;
              }

              break;
            case 'O':
              if (AsChar(entities.IncomeSource.ReturnCd) != 'V')
              {
                continue;
              }

              break;
            default:
              continue;
          }

          local.Subscript.Count = 24;
          UseFnB717InflateGv();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 24;
            local.Create.ObligorPersonNbr =
              entities.Infrastructure.CsePersonNumber;
            UseFnB717CreateStatsVerifi();
          }

          goto Test;
        }
      }
      else if (Equal(entities.Infrastructure.ReasonCode, "ADDRVRFDAP") || Equal
        (entities.Infrastructure.ReasonCode, "APDEAD") || Equal
        (entities.Infrastructure.ReasonCode, "FADDRVRFDAP"))
      {
        local.Subscript.Count = 25;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 25;
          local.Create.ObligorPersonNbr =
            entities.Infrastructure.CsePersonNumber;
          UseFnB717CreateStatsVerifi();
        }
      }

Test:

      local.Prev.Assign(entities.Infrastructure);
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
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

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(
          command, "cspINumber", entities.Infrastructure.CsePersonNumber ?? ""
          );
        db.SetNullableDate(
          command, "returnDt",
          entities.Infrastructure.ReferenceDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.EventId = db.GetInt32(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 6);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 9);
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Infrastructure Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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

    private Infrastructure prev;
    private DateWorkArea blank;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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

    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
  }
#endregion
}
