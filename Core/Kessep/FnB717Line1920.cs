// Program: FN_B717_LINE_19_20, ID: 373354061, model: 746.
// Short name: SWE03008
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_LINE_19_20.
/// </summary>
[Serializable]
public partial class FnB717Line1920: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_LINE_19_20 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717Line1920(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717Line1920.
  /// </summary>
  public FnB717Line1920(IContext context, Import import, Export export):
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

    foreach(var item in ReadLegalReferral())
    {
      local.Create.LineNumber = 19;

      if (Equal(entities.LegalReferral.ReferralReason1, "ADM") || Equal
        (entities.LegalReferral.ReferralReason1, "EST") || Equal
        (entities.LegalReferral.ReferralReason1, "PAT"))
      {
        local.Subscript.Count = 19;
      }
      else if (Equal(entities.LegalReferral.ReferralReason2, "ADM") || Equal
        (entities.LegalReferral.ReferralReason2, "EST") || Equal
        (entities.LegalReferral.ReferralReason2, "PAT"))
      {
        local.Subscript.Count = 19;
      }
      else if (Equal(entities.LegalReferral.ReferralReason3, "ADM") || Equal
        (entities.LegalReferral.ReferralReason3, "EST") || Equal
        (entities.LegalReferral.ReferralReason3, "PAT"))
      {
        local.Subscript.Count = 19;
      }
      else if (Equal(entities.LegalReferral.ReferralReason4, "ADM") || Equal
        (entities.LegalReferral.ReferralReason4, "EST") || Equal
        (entities.LegalReferral.ReferralReason4, "PAT"))
      {
        local.Subscript.Count = 19;
      }
      else if (Equal(entities.LegalReferral.ReferralReason5, "ADM") || Equal
        (entities.LegalReferral.ReferralReason5, "EST") || Equal
        (entities.LegalReferral.ReferralReason5, "PAT"))
      {
        local.Subscript.Count = 19;
      }
      else if (Equal(entities.LegalReferral.ReferralReason1, "ENF") || Equal
        (entities.LegalReferral.ReferralReason2, "ENF") || Equal
        (entities.LegalReferral.ReferralReason3, "ENF") || Equal
        (entities.LegalReferral.ReferralReason4, "ENF") || Equal
        (entities.LegalReferral.ReferralReason5, "ENF"))
      {
        local.Create.LineNumber = 20;
        local.Subscript.Count = 20;
      }
      else
      {
        continue;
      }

      UseFnB717InflateGv();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.Create.CourtOrderNumber = entities.LegalReferral.CourtCaseNumber;
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
    target.ChiefId = source.ChiefId;
    target.CaseNumber = source.CaseNumber;
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

    MoveProgram(import.Program, useImport.Program);
    import.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.Subscript.Count = local.Subscript.Count;

    Call(FnB717InflateGv.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 8);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 9);
        entities.LegalReferral.Populated = true;

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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of ChOrAr.
    /// </summary>
    [JsonPropertyName("chOrAr")]
    public CaseRole ChOrAr
    {
      get => chOrAr ??= new();
      set => chOrAr = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private LegalReferral legalReferral;
    private CsePerson supp;
    private CaseRole chOrAr;
    private CsePerson ap;
    private ObligationType obligationType;
  }
#endregion
}
