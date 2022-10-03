// Program: FN_B717_CREATE_OR_UPDT_STATS_RPT, ID: 373349238, model: 746.
// Short name: SWE03047
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_CREATE_OR_UPDT_STATS_RPT.
/// </summary>
[Serializable]
public partial class FnB717CreateOrUpdtStatsRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_CREATE_OR_UPDT_STATS_RPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717CreateOrUpdtStatsRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717CreateOrUpdtStatsRpt.
  /// </summary>
  public FnB717CreateOrUpdtStatsRpt(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadStatsReport())
    {
      MoveStatsReport(entities.StatsReport, local.StatsReport);

      switch(import.Program.SystemGeneratedIdentifier)
      {
        case 1:
          local.StatsReport.Column1 =
            local.StatsReport.Column1.GetValueOrDefault() + 1;

          break;
        case 2:
          local.StatsReport.Column2 =
            local.StatsReport.Column2.GetValueOrDefault() + 1;

          break;
        case 3:
          local.StatsReport.Column3 =
            local.StatsReport.Column3.GetValueOrDefault() + 1;

          break;
        case 4:
          local.StatsReport.Column4 =
            local.StatsReport.Column4.GetValueOrDefault() + 1;

          break;
        case 5:
          local.StatsReport.Column5 =
            local.StatsReport.Column5.GetValueOrDefault() + 1;

          break;
        case 6:
          local.StatsReport.Column6 =
            local.StatsReport.Column6.GetValueOrDefault() + 1;

          break;
        case 7:
          local.StatsReport.Column7 =
            local.StatsReport.Column7.GetValueOrDefault() + 1;

          break;
        case 8:
          local.StatsReport.Column8 =
            local.StatsReport.Column8.GetValueOrDefault() + 1;

          break;
        case 9:
          local.StatsReport.Column9 =
            local.StatsReport.Column9.GetValueOrDefault() + 1;

          break;
        case 10:
          local.StatsReport.Column10 =
            local.StatsReport.Column10.GetValueOrDefault() + 1;

          break;
        case 11:
          local.StatsReport.Column11 =
            local.StatsReport.Column11.GetValueOrDefault() + 1;

          break;
        case 12:
          local.StatsReport.Column12 =
            local.StatsReport.Column12.GetValueOrDefault() + 1;

          break;
        case 13:
          local.StatsReport.Column13 =
            local.StatsReport.Column13.GetValueOrDefault() + 1;

          break;
        case 14:
          local.StatsReport.Column14 =
            local.StatsReport.Column14.GetValueOrDefault() + 1;

          break;
        case 15:
          local.StatsReport.Column15 =
            local.StatsReport.Column15.GetValueOrDefault() + 1;

          break;
        default:
          break;
      }

      try
      {
        UpdateStatsReport();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_STATS_REPORT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_STATS_REPORT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      switch(import.Program.SystemGeneratedIdentifier)
      {
        case 1:
          local.StatsReport.Column1 =
            local.StatsReport.Column1.GetValueOrDefault() + 1;

          break;
        case 2:
          local.StatsReport.Column2 =
            local.StatsReport.Column2.GetValueOrDefault() + 1;

          break;
        case 3:
          local.StatsReport.Column3 =
            local.StatsReport.Column3.GetValueOrDefault() + 1;

          break;
        case 4:
          local.StatsReport.Column4 =
            local.StatsReport.Column4.GetValueOrDefault() + 1;

          break;
        case 5:
          local.StatsReport.Column5 =
            local.StatsReport.Column5.GetValueOrDefault() + 1;

          break;
        case 6:
          local.StatsReport.Column6 =
            local.StatsReport.Column6.GetValueOrDefault() + 1;

          break;
        case 7:
          local.StatsReport.Column7 =
            local.StatsReport.Column7.GetValueOrDefault() + 1;

          break;
        case 8:
          local.StatsReport.Column8 =
            local.StatsReport.Column8.GetValueOrDefault() + 1;

          break;
        case 9:
          local.StatsReport.Column9 =
            local.StatsReport.Column9.GetValueOrDefault() + 1;

          break;
        case 10:
          local.StatsReport.Column10 =
            local.StatsReport.Column10.GetValueOrDefault() + 1;

          break;
        case 11:
          local.StatsReport.Column11 =
            local.StatsReport.Column11.GetValueOrDefault() + 1;

          break;
        case 12:
          local.StatsReport.Column12 =
            local.StatsReport.Column12.GetValueOrDefault() + 1;

          break;
        case 13:
          local.StatsReport.Column13 =
            local.StatsReport.Column13.GetValueOrDefault() + 1;

          break;
        case 14:
          local.StatsReport.Column14 =
            local.StatsReport.Column14.GetValueOrDefault() + 1;

          break;
        case 15:
          local.StatsReport.Column15 =
            local.StatsReport.Column15.GetValueOrDefault() + 1;

          break;
        default:
          break;
      }

      local.Retry.Count = 0;

      do
      {
        try
        {
          CreateStatsReport();

          return;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.Retry.Count;

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      while(local.Retry.Count < 5);
    }
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveStatsReport(StatsReport source, StatsReport target)
  {
    target.Column1 = source.Column1;
    target.Column2 = source.Column2;
    target.Column3 = source.Column3;
    target.Column4 = source.Column4;
    target.Column5 = source.Column5;
    target.Column6 = source.Column6;
    target.Column7 = source.Column7;
    target.Column8 = source.Column8;
    target.Column9 = source.Column9;
    target.Column10 = source.Column10;
    target.Column11 = source.Column11;
    target.Column12 = source.Column12;
    target.Column13 = source.Column13;
    target.Column14 = source.Column14;
    target.Column15 = source.Column15;
  }

  private int UseFnB717GetSupervisorSp()
  {
    var useImport = new FnB717GetSupervisorSp.Import();
    var useExport = new FnB717GetSupervisorSp.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      import.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(FnB717GetSupervisorSp.Execute, useImport, useExport);

    return useExport.Sup.SystemGeneratedId;
  }

  private void CreateStatsReport()
  {
    var yearMonth = import.StatsReport.YearMonth.GetValueOrDefault();
    var firstRunNumber = import.StatsReport.FirstRunNumber.GetValueOrDefault();
    var lineNumber = import.LineNumber.Count;
    var createdTimestamp = Now();
    var servicePrvdrId = import.ServiceProvider.SystemGeneratedId;
    var officeId = import.Office.SystemGeneratedId;
    var caseWrkRole = import.OfficeServiceProvider.RoleCode;
    var caseEffDate = import.OfficeServiceProvider.EffectiveDate;
    var parentId = UseFnB717GetSupervisorSp();
    var column1 = local.StatsReport.Column1.GetValueOrDefault();
    var column2 = local.StatsReport.Column2.GetValueOrDefault();
    var column3 = local.StatsReport.Column3.GetValueOrDefault();
    var column4 = local.StatsReport.Column4.GetValueOrDefault();
    var column5 = local.StatsReport.Column5.GetValueOrDefault();
    var column6 = local.StatsReport.Column6.GetValueOrDefault();
    var column7 = local.StatsReport.Column7.GetValueOrDefault();
    var column8 = local.StatsReport.Column8.GetValueOrDefault();
    var column9 = local.StatsReport.Column9.GetValueOrDefault();
    var column10 = local.StatsReport.Column10.GetValueOrDefault();
    var column11 = local.StatsReport.Column11.GetValueOrDefault();
    var column12 = local.StatsReport.Column12.GetValueOrDefault();
    var column13 = local.StatsReport.Column13.GetValueOrDefault();
    var column14 = local.StatsReport.Column14.GetValueOrDefault();
    var column15 = local.StatsReport.Column15.GetValueOrDefault();

    entities.StatsReport.Populated = false;
    Update("CreateStatsReport",
      (db, command) =>
      {
        db.SetNullableInt32(command, "yearMonth", yearMonth);
        db.SetNullableInt32(command, "firstRunNumber", firstRunNumber);
        db.SetNullableInt32(command, "lineNumber", lineNumber);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "servicePrvdrId", servicePrvdrId);
        db.SetNullableInt32(command, "officeId", officeId);
        db.SetNullableString(command, "caseWrkRole", caseWrkRole);
        db.SetNullableDate(command, "caseEffDate", caseEffDate);
        db.SetNullableInt32(command, "parentId", parentId);
        db.SetNullableInt32(command, "chiefId", 0);
        db.SetNullableInt64(command, "column1", column1);
        db.SetNullableInt64(command, "column2", column2);
        db.SetNullableInt64(command, "column3", column3);
        db.SetNullableInt64(command, "column4", column4);
        db.SetNullableInt64(command, "column5", column5);
        db.SetNullableInt64(command, "column6", column6);
        db.SetNullableInt64(command, "column7", column7);
        db.SetNullableInt64(command, "column8", column8);
        db.SetNullableInt64(command, "column9", column9);
        db.SetNullableInt64(command, "column10", column10);
        db.SetNullableInt64(command, "column11", column11);
        db.SetNullableInt64(command, "column12", column12);
        db.SetNullableInt64(command, "column13", column13);
        db.SetNullableInt64(command, "column14", column14);
        db.SetNullableInt64(command, "column15", column15);
      });

    entities.StatsReport.YearMonth = yearMonth;
    entities.StatsReport.FirstRunNumber = firstRunNumber;
    entities.StatsReport.LineNumber = lineNumber;
    entities.StatsReport.CreatedTimestamp = createdTimestamp;
    entities.StatsReport.ServicePrvdrId = servicePrvdrId;
    entities.StatsReport.OfficeId = officeId;
    entities.StatsReport.CaseWrkRole = caseWrkRole;
    entities.StatsReport.CaseEffDate = caseEffDate;
    entities.StatsReport.ParentId = parentId;
    entities.StatsReport.ChiefId = 0;
    entities.StatsReport.Column1 = column1;
    entities.StatsReport.Column2 = column2;
    entities.StatsReport.Column3 = column3;
    entities.StatsReport.Column4 = column4;
    entities.StatsReport.Column5 = column5;
    entities.StatsReport.Column6 = column6;
    entities.StatsReport.Column7 = column7;
    entities.StatsReport.Column8 = column8;
    entities.StatsReport.Column9 = column9;
    entities.StatsReport.Column10 = column10;
    entities.StatsReport.Column11 = column11;
    entities.StatsReport.Column12 = column12;
    entities.StatsReport.Column13 = column13;
    entities.StatsReport.Column14 = column14;
    entities.StatsReport.Column15 = column15;
    entities.StatsReport.Populated = true;
  }

  private bool ReadStatsReport()
  {
    entities.StatsReport.Populated = false;

    return Read("ReadStatsReport",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "servicePrvdrId", import.ServiceProvider.SystemGeneratedId);
        db.
          SetNullableInt32(command, "officeId", import.Office.SystemGeneratedId);
          
        db.SetNullableInt32(
          command, "yearMonth",
          import.StatsReport.YearMonth.GetValueOrDefault());
        db.SetNullableInt32(
          command, "firstRunNumber",
          import.StatsReport.FirstRunNumber.GetValueOrDefault());
        db.SetInt32(command, "count", import.LineNumber.Count);
      },
      (db, reader) =>
      {
        entities.StatsReport.YearMonth = db.GetNullableInt32(reader, 0);
        entities.StatsReport.FirstRunNumber = db.GetNullableInt32(reader, 1);
        entities.StatsReport.LineNumber = db.GetNullableInt32(reader, 2);
        entities.StatsReport.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.StatsReport.ServicePrvdrId = db.GetNullableInt32(reader, 4);
        entities.StatsReport.OfficeId = db.GetNullableInt32(reader, 5);
        entities.StatsReport.CaseWrkRole = db.GetNullableString(reader, 6);
        entities.StatsReport.CaseEffDate = db.GetNullableDate(reader, 7);
        entities.StatsReport.ParentId = db.GetNullableInt32(reader, 8);
        entities.StatsReport.ChiefId = db.GetNullableInt32(reader, 9);
        entities.StatsReport.Column1 = db.GetNullableInt64(reader, 10);
        entities.StatsReport.Column2 = db.GetNullableInt64(reader, 11);
        entities.StatsReport.Column3 = db.GetNullableInt64(reader, 12);
        entities.StatsReport.Column4 = db.GetNullableInt64(reader, 13);
        entities.StatsReport.Column5 = db.GetNullableInt64(reader, 14);
        entities.StatsReport.Column6 = db.GetNullableInt64(reader, 15);
        entities.StatsReport.Column7 = db.GetNullableInt64(reader, 16);
        entities.StatsReport.Column8 = db.GetNullableInt64(reader, 17);
        entities.StatsReport.Column9 = db.GetNullableInt64(reader, 18);
        entities.StatsReport.Column10 = db.GetNullableInt64(reader, 19);
        entities.StatsReport.Column11 = db.GetNullableInt64(reader, 20);
        entities.StatsReport.Column12 = db.GetNullableInt64(reader, 21);
        entities.StatsReport.Column13 = db.GetNullableInt64(reader, 22);
        entities.StatsReport.Column14 = db.GetNullableInt64(reader, 23);
        entities.StatsReport.Column15 = db.GetNullableInt64(reader, 24);
        entities.StatsReport.Populated = true;
      });
  }

  private void UpdateStatsReport()
  {
    var column1 = local.StatsReport.Column1.GetValueOrDefault();
    var column2 = local.StatsReport.Column2.GetValueOrDefault();
    var column3 = local.StatsReport.Column3.GetValueOrDefault();
    var column4 = local.StatsReport.Column4.GetValueOrDefault();
    var column5 = local.StatsReport.Column5.GetValueOrDefault();
    var column6 = local.StatsReport.Column6.GetValueOrDefault();
    var column7 = local.StatsReport.Column7.GetValueOrDefault();
    var column8 = local.StatsReport.Column8.GetValueOrDefault();
    var column9 = local.StatsReport.Column9.GetValueOrDefault();
    var column10 = local.StatsReport.Column10.GetValueOrDefault();
    var column11 = local.StatsReport.Column11.GetValueOrDefault();
    var column12 = local.StatsReport.Column12.GetValueOrDefault();
    var column13 = local.StatsReport.Column13.GetValueOrDefault();
    var column14 = local.StatsReport.Column14.GetValueOrDefault();
    var column15 = local.StatsReport.Column15.GetValueOrDefault();

    entities.StatsReport.Populated = false;
    Update("UpdateStatsReport",
      (db, command) =>
      {
        db.SetNullableInt64(command, "column1", column1);
        db.SetNullableInt64(command, "column2", column2);
        db.SetNullableInt64(command, "column3", column3);
        db.SetNullableInt64(command, "column4", column4);
        db.SetNullableInt64(command, "column5", column5);
        db.SetNullableInt64(command, "column6", column6);
        db.SetNullableInt64(command, "column7", column7);
        db.SetNullableInt64(command, "column8", column8);
        db.SetNullableInt64(command, "column9", column9);
        db.SetNullableInt64(command, "column10", column10);
        db.SetNullableInt64(command, "column11", column11);
        db.SetNullableInt64(command, "column12", column12);
        db.SetNullableInt64(command, "column13", column13);
        db.SetNullableInt64(command, "column14", column14);
        db.SetNullableInt64(command, "column15", column15);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.StatsReport.CreatedTimestamp.GetValueOrDefault());
      });

    entities.StatsReport.Column1 = column1;
    entities.StatsReport.Column2 = column2;
    entities.StatsReport.Column3 = column3;
    entities.StatsReport.Column4 = column4;
    entities.StatsReport.Column5 = column5;
    entities.StatsReport.Column6 = column6;
    entities.StatsReport.Column7 = column7;
    entities.StatsReport.Column8 = column8;
    entities.StatsReport.Column9 = column9;
    entities.StatsReport.Column10 = column10;
    entities.StatsReport.Column11 = column11;
    entities.StatsReport.Column12 = column12;
    entities.StatsReport.Column13 = column13;
    entities.StatsReport.Column14 = column14;
    entities.StatsReport.Column15 = column15;
    entities.StatsReport.Populated = true;
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
    /// A value of LineNumber.
    /// </summary>
    [JsonPropertyName("lineNumber")]
    public Common LineNumber
    {
      get => lineNumber ??= new();
      set => lineNumber = value;
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

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private Common lineNumber;
    private StatsReport statsReport;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private Program program;
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
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    /// <summary>
    /// A value of LineNbr.
    /// </summary>
    [JsonPropertyName("lineNbr")]
    public Common LineNbr
    {
      get => lineNbr ??= new();
      set => lineNbr = value;
    }

    /// <summary>
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
    }

    private StatsReport statsReport;
    private Common lineNbr;
    private Common retry;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private StatsReport statsReport;
  }
#endregion
}
