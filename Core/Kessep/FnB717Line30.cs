// Program: FN_B717_LINE_30, ID: 373354361, model: 746.
// Short name: SWE03039
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_30.
/// </summary>
[Serializable]
public partial class FnB717Line30: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_30 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line30(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line30.
  /// </summary>
  public FnB717Line30(IContext context, Import import, Export export):
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

    if (ReadLegalActionCsePersonLegalActionDetail())
    {
      local.Subscript.Count = 30;
      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 30;
        local.Create.SuppPersonNumber = import.Ch.Number;
        local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
        local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
        UseFnB717CreateStatsVerifi();
      }
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
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
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

    import.Group.CopyTo(useImport.Group, MoveGroup2);
    MoveProgram(import.Program, useImport.Program);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private bool ReadLegalActionCsePersonLegalActionDetail()
  {
    entities.LegalAction.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionCsePersonLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDtEntredOn", local.Blank.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", import.Ch.Number);
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
        entities.LegalActionDetail.Number = db.GetInt32(reader, 8);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 10);
        entities.LegalAction.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    private CsePerson ch;
    private StatsVerifi create;
    private Case1 case1;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Array<GroupGroup> group;
    private Program program;
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

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private StatsVerifi create;
    private Common subscript;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private LegalAction legalAction;
    private CsePerson apCsePerson;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CaseRole apCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
    private CaseRole chCaseRole;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
  }
#endregion
}
