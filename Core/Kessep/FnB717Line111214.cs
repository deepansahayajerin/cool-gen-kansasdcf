// Program: FN_B717_LINE_11_12_14, ID: 373354044, model: 746.
// Short name: SWE03005
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_11_12_14.
/// </summary>
[Serializable]
public partial class FnB717Line111214: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_11_12_14 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line111214(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line111214.
  /// </summary>
  public FnB717Line111214(IContext context, Import import, Export export):
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
    local.FirstTime.Flag = "Y";

    // ---------------------------------------
    // CH role active during report period.
    // ---------------------------------------
    foreach(var item in ReadCaseRoleCsePersonCaseRoleCsePerson())
    {
      if (!Lt(local.Prev.Number, entities.ChCsePerson.Number))
      {
        continue;
      }

      foreach(var item1 in ReadLegalActionDetailLegalAction())
      {
        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          local.FirstTime.Flag = "N";
          local.Subscript.Count = 12;
          UseFnB717InflateGv1();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 12;
            local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
            local.Create.CaseRoleType = entities.ChCaseRole.Type1;
            local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
            local.Create.ObligationType = "HIC";
            local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
            UseFnB717CreateStatsVerifi();
          }
        }

        UseFnB717ProgHierarchy4Person();
        local.Subscript.Count = 11;
        UseFnB717InflateGv2();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 11;
          local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
          local.Create.CaseRoleType = entities.ChCaseRole.Type1;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.ObligationType = "HIC";
          local.Create.CourtOrderNumber = entities.LegalAction.StandardNumber;
          local.Create.ProgramType = local.Program.Code;
          UseFnB717CreateStatsVerifi();
        }

        if (Lt(local.Null1.Date, entities.ChCaseRole.DateOfEmancipation) && Lt
          (entities.ChCaseRole.DateOfEmancipation, import.ReportStartDate.Date))
        {
          local.Subscript.Count = 14;
          UseFnB717InflateGv2();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 14;
            UseFnB717CreateStatsVerifi();
          }
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
    target.ObligationType = source.ObligationType;
    target.CaseRoleType = source.CaseRoleType;
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

  private void UseFnB717InflateGv1()
  {
    var useImport = new FnB717InflateGv.Import();
    var useExport = new FnB717InflateGv.Export();

    MoveProgram(import.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private void UseFnB717InflateGv2()
  {
    var useImport = new FnB717InflateGv.Import();
    var useExport = new FnB717InflateGv.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.Subscript.Count = local.Subscript.Count;
    MoveProgram(local.Program, useImport.Program);

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private void UseFnB717ProgHierarchy4Person()
  {
    var useImport = new FnB717ProgHierarchy4Person.Import();
    var useExport = new FnB717ProgHierarchy4Person.Export();

    useImport.CsePerson.Assign(entities.ChCsePerson);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717ProgHierarchy4Person.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

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
        entities.ChCsePerson.Type1 = db.GetString(reader, 13);
        entities.ChCsePerson.BornOutOfWedlock =
          db.GetNullableString(reader, 14);
        entities.ChCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 15);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;

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
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
        db.SetDate(
          command, "effectiveDt",
          import.ReportEndDate.Date.GetValueOrDefault());
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
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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

    private Case1 case1;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
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
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private CsePerson prev;
    private Common firstTime;
    private Common subscript;
    private Program program;
    private DateWorkArea null1;
    private StatsVerifi create;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private LegalActionCaseRole apLegalActionCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
  }
#endregion
}
