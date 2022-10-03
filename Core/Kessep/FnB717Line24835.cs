// Program: FN_B717_LINE_2_4_8_35, ID: 373354046, model: 746.
// Short name: SWE03014
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_2_4_8_35.
/// </summary>
[Serializable]
public partial class FnB717Line24835: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_2_4_8_35 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line24835(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line24835.
  /// </summary>
  public FnB717Line24835(IContext context, Import import, Export export):
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
    export.AccrInstOpen.Flag = "N";

    foreach(var item in ReadCsePersonCsePerson())
    {
      if (Equal(entities.ChCsePerson.Number, local.Prev.Number))
      {
        continue;
      }

      foreach(var item1 in ReadAccrualInstructionsObligationType())
      {
        export.AccrInstOpen.Flag = "Y";

        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          local.FirstTime.Flag = "N";
          local.Subscript.Count = 4;
          UseFnB717InflateGv1();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 4;
            local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
            local.Create.CaseRoleType = entities.ChCaseRole.Type1;
            local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
            local.Create.ObligationType = entities.ObligationType.Code;
            UseFnB717CreateStatsVerifi();
          }

          local.Subscript.Count = 2;
          UseFnB717InflateGv1();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 2;
            UseFnB717CreateStatsVerifi();
          }

          // -------------------------------------------------------------------
          // Line 35 - Not reported. Used to store eligible cases for line 5
          // --------------------------------------------------------------------
          local.Subscript.Count = 35;
          UseFnB717InflateGv1();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.Create.LineNumber = 35;
            UseFnB717CreateStatsVerifi();
          }
        }

        // ************
        // !@#!@#@!$@!$
        // This cab will check if we have already derived the program
        // category for this person. Needs to be tweaked!!!
        // ************
        UseFnB717ProgHierarchy4Person();
        local.Subscript.Count = 8;
        UseFnB717InflateGv2();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 8;
          local.Create.ProgramType = local.Program.Code;
          local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
          local.Create.CaseRoleType = entities.ChCaseRole.Type1;
          local.Create.ObligorPersonNbr = entities.ApCsePerson.Number;
          local.Create.ObligationType = entities.ObligationType.Code;
          UseFnB717CreateStatsVerifi();
        }

        local.Prev.Number = entities.ChCsePerson.Number;

        goto ReadEach;
      }

ReadEach:
      ;
    }

    if (AsChar(local.FirstTime.Flag) == 'Y')
    {
      // -------------------------------------------------------------
      // Case not included in line 4. Check if eligible for line 5.
      // ------------------------------------------------------------
      if (!ReadCase())
      {
        return;
      }

      if (Equal(entities.Case1.PaMedicalService, "MO"))
      {
        return;
      }

      foreach(var item in ReadCsePersonCaseRole())
      {
        if (Lt(local.Null1.Date, entities.ChCaseRole.DateOfEmancipation) && !
          Lt(import.ReportStartDate.Date, entities.ChCaseRole.DateOfEmancipation))
          
        {
          continue;
        }

        local.Subscript.Count = 35;
        UseFnB717InflateGv1();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 35;
          local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
          local.Create.CaseRoleType = "CH";
          UseFnB717CreateStatsVerifi();
        }

        return;
      }
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
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.Dddd = source.Dddd;
    target.DebtDetailBaldue = source.DebtDetailBaldue;
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CollCreatedDate = source.CollCreatedDate;
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
    MoveProgram(local.Program, useImport.Program);
    useImport.Subscript.Count = local.Subscript.Count;

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

  private IEnumerable<bool> ReadAccrualInstructionsObligationType()
  {
    entities.AccrualInstructions.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadAccrualInstructionsObligationType",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(
          command, "asOfDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ObligationType.Code = db.GetString(reader, 10);
        entities.AccrualInstructions.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.ChCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.ChCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCsePerson",
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
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Number = db.GetString(reader, 1);
        entities.ChCsePerson.Populated = true;
        entities.ApCsePerson.Populated = true;

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
    /// <summary>
    /// A value of AccrInstOpen.
    /// </summary>
    [JsonPropertyName("accrInstOpen")]
    public Common AccrInstOpen
    {
      get => accrInstOpen ??= new();
      set => accrInstOpen = value;
    }

    private Common accrInstOpen;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    private DateWorkArea null1;
    private Common firstTime;
    private Program program;
    private CsePerson prev;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public LegalActionPerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
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
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public LegalActionPerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    private Case1 case1;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount supported;
    private CsePerson chCsePerson;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private LegalActionPerson obligor1;
    private CsePerson apCsePerson;
    private LegalActionPerson supp;
    private CaseRole apCaseRole;
    private CaseRole chCaseRole;
    private CsePersonAccount obligor2;
  }
#endregion
}
