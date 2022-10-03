// Program: FN_B717_LINE_29_31, ID: 373350410, model: 746.
// Short name: SWE03038
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_29_31.
/// </summary>
[Serializable]
public partial class FnB717Line2931: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_29_31 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line2931(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line2931.
  /// </summary>
  public FnB717Line2931(IContext context, Import import, Export export):
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

    foreach(var item in ReadLegalActionCsePersonCsePersonLegalActionDetail())
    {
      if (Equal(entities.LegalAction.StandardNumber, local.Prev.StandardNumber))
      {
        continue;
      }

      if (Equal(entities.LegalAction.ActionTaken, "CONMODJ") || Equal
        (entities.LegalAction.ActionTaken, "CUSTODY") || Equal
        (entities.LegalAction.ActionTaken, "EMPIWOJ") || Equal
        (entities.LegalAction.ActionTaken, "EMPMWOJ") || Equal
        (entities.LegalAction.ActionTaken, "JDGFRMJ") || Equal
        (entities.LegalAction.ActionTaken, "JEFBC") || Equal
        (entities.LegalAction.ActionTaken, "JEFMOD") || Equal
        (entities.LegalAction.ActionTaken, "MODBC") || Equal
        (entities.LegalAction.ActionTaken, "MODSUPPO") || Equal
        (entities.LegalAction.ActionTaken, "RECOVRYJ") || Equal
        (entities.LegalAction.ActionTaken, "REGENFNJ") || Equal
        (entities.LegalAction.ActionTaken, "REGENFO") || Equal
        (entities.LegalAction.ActionTaken, "REGMODNJ") || Equal
        (entities.LegalAction.ActionTaken, "REGMODO") || Equal
        (entities.LegalAction.ActionTaken, "SETARRSJ") || Equal
        (entities.LegalAction.ActionTaken, "SETARRXJ"))
      {
        continue;
      }

      local.Subscript.Count = 29;
      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 29;
        local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
        local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
        local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
        local.Create.TranAmount = 0;
        UseFnB717CreateStatsVerifi();
      }

      switch(TrimEnd(entities.LegalActionDetail.FreqPeriodCode))
      {
        case "M":
          local.Increment.Column1 =
            (long?)(entities.LegalActionDetail.CurrentAmount.
              GetValueOrDefault() * 100);

          break;
        case "SM":
          local.Increment.Column1 =
            (long?)(entities.LegalActionDetail.CurrentAmount.
              GetValueOrDefault() * 200);

          break;
        case "W":
          local.Increment.Column1 =
            (long?)(entities.LegalActionDetail.CurrentAmount.
              GetValueOrDefault() * 100 * 52 / 12);

          break;
        case "BW":
          local.Increment.Column1 =
            (long?)(entities.LegalActionDetail.CurrentAmount.
              GetValueOrDefault() * 100 * 26 / 12);

          break;
        default:
          break;
      }

      local.Subscript.Count = 31;
      UseFnB717InflateGvByCounter();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 31;
        local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
        local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
        local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
        local.Create.TranAmount =
          local.Increment.Column1.GetValueOrDefault() / (decimal)100;
        UseFnB717CreateStatsVerifi();
      }

      local.Prev.StandardNumber = entities.LegalAction.StandardNumber;
    }
  }

  private static void MoveGroup1(Import.GroupGroup source,
    FnB717InflateGv.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(Import.GroupGroup source,
    FnB717InflateGvByCounter.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup3(FnB717InflateGv.Import.GroupGroup source,
    Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup4(FnB717InflateGvByCounter.Import.
    GroupGroup source, Import.GroupGroup target)
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
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.TranAmount = source.TranAmount;
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

    useImport.Subscript.Count = local.Subscript.Count;
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    MoveProgram(import.Program, useImport.Program);

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup3);
  }

  private void UseFnB717InflateGvByCounter()
  {
    var useImport = new FnB717InflateGvByCounter.Import();
    var useExport = new FnB717InflateGvByCounter.Export();

    useImport.Increment.Column1 = local.Increment.Column1;
    useImport.Subscript.Count = local.Subscript.Count;
    import.Group.CopyTo(useImport.Group, MoveGroup2);
    MoveProgram(import.Program, useImport.Program);

    Call(FnB717InflateGvByCounter.Execute, useImport, useExport);

    local.Increment.Column1 = useImport.Increment.Column1;
    useImport.Group.CopyTo(import.Group, MoveGroup4);
  }

  private IEnumerable<bool> ReadLegalActionCsePersonCsePersonLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionCsePersonCsePersonLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDtEntredOn", local.Blank.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 5);
        entities.LegalAction.FiledDtEntredOn = db.GetNullableDate(reader, 6);
        entities.ApCsePerson.Number = db.GetString(reader, 7);
        entities.ChCsePerson.Number = db.GetString(reader, 8);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 9);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 12);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;
        entities.LegalAction.Populated = true;

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
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public StatsVerifi Create
    {
      get => create ??= new();
      set => create = value;
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    private StatsVerifi create;
    private Case1 case1;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Program program;
    private Array<GroupGroup> group;
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
    /// A value of Increment.
    /// </summary>
    [JsonPropertyName("increment")]
    public StatsReport Increment
    {
      get => increment ??= new();
      set => increment = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    private StatsReport increment;
    private DateWorkArea blank;
    private LegalAction prev;
    private StatsVerifi create;
    private Common subscript;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
    private LegalAction legalAction;
    private LegalActionCaseRole apLegalActionCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
  }
#endregion
}
