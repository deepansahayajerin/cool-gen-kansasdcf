// Program: FN_B717_LINE_26, ID: 373350375, model: 746.
// Short name: SWE03012
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_26.
/// </summary>
[Serializable]
public partial class FnB717Line2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_26 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line2.
  /// </summary>
  public FnB717Line2(IContext context, Import import, Export export):
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

    foreach(var item in ReadCaseRoleCsePersonCaseRoleCsePerson())
    {
      if (!Lt(local.Prev.Number, entities.ChCsePerson.Number))
      {
        continue;
      }

      foreach(var item1 in ReadLegalActionDetailLegalAction())
      {
        if (Lt(local.Blank.Date, import.CaseClosureDate.Date))
        {
          UseFnB717ProgHierarchy4Person2();
        }
        else
        {
          UseFnB717ProgHierarchy4Person1();
        }

        local.Subscript.Count = 26;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 26;
          local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
          local.Create.ProgramType = local.Program.Code;
          UseFnB717CreateStatsVerifi();
        }

        local.Prev.Number = entities.ChCsePerson.Number;

        goto ReadEach;
      }

ReadEach:
      ;
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveGroup1(Import.GroupGroup source,
    FnB717InflateGv.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(FnB717InflateGv.Import.GroupGroup source,
    Import.GroupGroup target)
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

    useImport.Subscript.Count = local.Subscript.Count;
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    MoveProgram(local.Program, useImport.Program);

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup2);
  }

  private void UseFnB717ProgHierarchy4Person1()
  {
    var useImport = new FnB717ProgHierarchy4Person.Import();
    var useExport = new FnB717ProgHierarchy4Person.Export();

    useImport.CsePerson.Assign(entities.ChCsePerson);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Person.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnB717ProgHierarchy4Person2()
  {
    var useImport = new FnB717ProgHierarchy4Person.Import();
    var useExport = new FnB717ProgHierarchy4Person.Export();

    useImport.CsePerson.Assign(entities.ChCsePerson);
    MoveDateWorkArea(import.CaseClosureDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Person.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ApCaseRole.Populated = false;
    entities.ChCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 6);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 7);
        entities.ChCsePerson.Number = db.GetString(reader, 7);
        entities.ChCaseRole.Type1 = db.GetString(reader, 8);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 9);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 12);
        entities.ChCsePerson.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.ChCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier1", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApCaseRole.CasNumber);
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Blank.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.ActionTaken = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 10);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 12);
        entities.LegalAction.FiledDtEntredOn = db.GetNullableDate(reader, 13);
        entities.LegalActionDetail.Populated = true;
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

    /// <summary>
    /// A value of CaseClosureDate.
    /// </summary>
    [JsonPropertyName("caseClosureDate")]
    public DateWorkArea CaseClosureDate
    {
      get => caseClosureDate ??= new();
      set => caseClosureDate = value;
    }

    private Array<GroupGroup> group;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Case1 case1;
    private StatsVerifi create;
    private Common displayInd;
    private DateWorkArea caseClosureDate;
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
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    private DateWorkArea blank;
    private CsePerson prev;
    private Program program;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    private CsePerson chCsePerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CaseRole apCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
    private CaseRole chCaseRole;
    private CsePerson apCsePerson;
  }
#endregion
}
