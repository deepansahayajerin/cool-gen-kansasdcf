// Program: FN_B717_LINE_7_9_10_14, ID: 373354043, model: 746.
// Short name: SWE03017
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_7_9_10_14.
/// </summary>
[Serializable]
public partial class FnB717Line791014: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_7_9_10_14 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line791014(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line791014.
  /// </summary>
  public FnB717Line791014(IContext context, Import import, Export export):
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

    // -------------------------------------
    // CH roles active during report period,
    // -------------------------------------
    foreach(var item in ReadCsePersonCaseRole())
    {
      // -------------------------------------
      // Count each child only once per case.
      // -------------------------------------
      if (Equal(entities.ChCsePerson.Number, local.Prev.Number))
      {
        continue;
      }

      local.Prev.Number = entities.ChCsePerson.Number;
      UseFnB717ProgHierarchy4Person();
      local.Subscript.Count = 7;
      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.LineNumber = 7;
        local.Create.SuppPersonNumber = entities.ChCsePerson.Number;
        local.Create.ProgramType = local.Program.Code;
        UseFnB717CreateStatsVerifi();
      }

      // -----------------------------------------------------------
      // line 14 - not reported. Used to store eligible cases for line 13.
      // In this CAB count non-emancipated children.
      // If eman children have HIC, we'll count in line 11 cab.
      // -----------------------------------------------------------
      if (Equal(entities.ChCaseRole.DateOfEmancipation, local.Null1.Date) || !
        Lt(entities.ChCaseRole.DateOfEmancipation, import.ReportStartDate.Date))
      {
        local.Subscript.Count = 14;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 14;
          UseFnB717CreateStatsVerifi();
        }
      }

      if (AsChar(entities.ChCsePerson.PaternityEstablishedIndicator) == 'Y')
      {
        local.Subscript.Count = 9;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 9;
          UseFnB717CreateStatsVerifi();
        }
      }
      else if (AsChar(entities.ChCsePerson.PaternityEstablishedIndicator) == 'N'
        && AsChar(entities.ChCsePerson.BornOutOfWedlock) == 'Y')
      {
        local.Subscript.Count = 10;
        UseFnB717InflateGv();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.Create.LineNumber = 10;
          UseFnB717CreateStatsVerifi();
        }
      }

      UseFnB717Line30();
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

  private static void MoveGroup3(Import.GroupGroup source,
    FnB717Line30.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup4(FnB717Line30.Import.GroupGroup source,
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

    import.Group.CopyTo(useImport.Group, MoveGroup2);
    MoveProgram(local.Program, useImport.Program);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private void UseFnB717Line30()
  {
    var useImport = new FnB717Line30.Import();
    var useExport = new FnB717Line30.Export();

    useImport.Ch.Assign(entities.ChCsePerson);
    useImport.Create.Assign(local.Create);
    useImport.Case1.Assign(import.Case1);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    import.Group.CopyTo(useImport.Group, MoveGroup3);
    MoveProgram(local.Program, useImport.Program);
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;

    Call(FnB717Line30.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup4);
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

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChCsePerson.BornOutOfWedlock = db.GetNullableString(reader, 2);
        entities.ChCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 4);
        entities.ChCaseRole.Type1 = db.GetString(reader, 5);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 6);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 9);
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;

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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private DateWorkArea null1;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
    private Case1 case1;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }
#endregion
}
