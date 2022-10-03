// Program: FN_B717_MAINTAIN_LINE_27, ID: 373361367, model: 746.
// Short name: SWE03051
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_MAINTAIN_LINE_27.
/// </summary>
[Serializable]
public partial class FnB717MaintainLine27: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_MAINTAIN_LINE_27 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717MaintainLine27(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717MaintainLine27.
  /// </summary>
  public FnB717MaintainLine27(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider())
    {
      return;
    }

    local.Program.Code = import.Collection.ProgramAppliedTo;

    if (Equal(local.Program.Code, "FCI"))
    {
      local.Program.Code = "AFI";
    }

    if (IsEmpty(local.Program.Code))
    {
      local.Program.Code = "AF";
    }

    switch(TrimEnd(local.Program.Code))
    {
      case "AF":
        local.Program.SystemGeneratedIdentifier = 1;

        break;
      case "FC":
        local.Program.SystemGeneratedIdentifier = 5;

        break;
      case "NF":
        local.Program.SystemGeneratedIdentifier = 6;

        break;
      case "NC":
        local.Program.SystemGeneratedIdentifier = 7;

        break;
      case "NA":
        local.Program.SystemGeneratedIdentifier = 8;

        break;
      case "AFI":
        local.Program.SystemGeneratedIdentifier = 10;

        break;
      case "NAI":
        local.Program.SystemGeneratedIdentifier = 9;

        break;
      default:
        break;
    }

    if (Lt(import.Collection.CreatedTmst, import.ReportStartDate.Timestamp))
    {
      local.Increment.Column1 = (long?)(-(import.Collection.Amount * 100));
    }
    else
    {
      local.Increment.Column1 = (long?)(import.Collection.Amount * 100);
    }

    local.LineNumber.Count = 27;
    UseFnB717CrudStatsRptByCount();

    if (AsChar(import.DisplayInd.Flag) == 'Y')
    {
      MoveStatsVerifi2(import.Create, local.Create);
      local.Create.CaseNumber = import.Case1.Number;
      local.Create.CaseWrkRole = entities.OfficeServiceProvider.RoleCode;
      local.Create.ServicePrvdrId = entities.ServiceProvider.SystemGeneratedId;
      local.Create.OfficeId = entities.Office.SystemGeneratedId;
      local.Create.ProgramType = local.Program.Code;
      UseFnB717GetSupervisorSp();
      local.Create.ParentId = local.Sup.SystemGeneratedId;
      UseFnB717CreateStatsVerifi();
    }
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
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CollCreatedDate = source.CollCreatedDate;
  }

  private static void MoveStatsVerifi2(StatsVerifi source, StatsVerifi target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.ObligationType = source.ObligationType;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.CollCreatedDate = source.CollCreatedDate;
  }

  private void UseFnB717CreateStatsVerifi()
  {
    var useImport = new FnB717CreateStatsVerifi.Import();
    var useExport = new FnB717CreateStatsVerifi.Export();

    MoveStatsVerifi1(local.Create, useImport.StatsVerifi);

    Call(FnB717CreateStatsVerifi.Execute, useImport, useExport);
  }

  private void UseFnB717CrudStatsRptByCount()
  {
    var useImport = new FnB717CrudStatsRptByCount.Import();
    var useExport = new FnB717CrudStatsRptByCount.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    MoveStatsReport(import.StatsReport, useImport.StatsReport);
    useImport.Increment.Column1 = local.Increment.Column1;
    MoveProgram(local.Program, useImport.Program);
    useImport.LineNumber.Count = local.LineNumber.Count;

    Call(FnB717CrudStatsRptByCount.Execute, useImport, useExport);
  }

  private void UseFnB717GetSupervisorSp()
  {
    var useImport = new FnB717GetSupervisorSp.Import();
    var useExport = new FnB717GetSupervisorSp.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;

    Call(FnB717GetSupervisorSp.Execute, useImport, useExport);

    local.Sup.SystemGeneratedId = useExport.Sup.SystemGeneratedId;
  }

  private bool ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.DistributionDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of DistributionDate.
    /// </summary>
    [JsonPropertyName("distributionDate")]
    public DateWorkArea DistributionDate
    {
      get => distributionDate ??= new();
      set => distributionDate = value;
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
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    private DateWorkArea reportStartDate;
    private StatsVerifi create;
    private Common displayInd;
    private Collection collection;
    private DateWorkArea distributionDate;
    private Case1 case1;
    private StatsReport statsReport;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of LineNumber.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    public Common LineNumber
    {
      get => lineNumber ??= new();
      set => lineNumber = value;
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
    /// A value of Sup.
    /// </summary>
    [JsonPropertyName("sup")]
    public ServiceProvider Sup
    {
      get => sup ??= new();
      set => sup = value;
    }

    private StatsReport increment;
    private Program program;
    private Common lineNumber;
    private StatsVerifi create;
    private ServiceProvider sup;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private CaseAssignment caseAssignment;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private CsePerson ch;
    private CsePerson ap;
    private LegalAction legalAction;
  }
#endregion
}
