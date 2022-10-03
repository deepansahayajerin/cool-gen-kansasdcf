// Program: OE_B496_FIX_HINS_COMPANY, ID: 371173772, model: 746.
// Short name: SWEE496B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B496_FIX_HINS_COMPANY.
/// </para>
/// <para>
/// This program writes the CSE Person numbers of those who are APs or ARs, to a
/// sequential file, using external SWEXEE27.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB496FixHinsCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B496_FIX_HINS_COMPANY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB496FixHinsCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB496FixHinsCompany.
  /// </summary>
  public OeB496FixHinsCompany(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Current.Timestamp = Now();
    UseOeB496Housekeeping();

    foreach(var item in ReadHealthInsuranceCompany())
    {
      if (Equal(entities.HealthInsuranceCompany.CarrierCode, 1, 4, "CONV"))
      {
        local.HealthInsuranceCompany.CarrierCode = "0000000";
      }
      else if (IsEmpty(Substring(
        entities.HealthInsuranceCompany.CarrierCode, 5, 3)))
      {
        local.HealthInsuranceCompany.CarrierCode = "000" + entities
          .HealthInsuranceCompany.CarrierCode;
      }
      else if (Lt(local.Null1.Date, entities.HealthInsuranceCompany.StartDate))
      {
        continue;
      }
      else
      {
        local.HealthInsuranceCompany.CarrierCode =
          entities.HealthInsuranceCompany.CarrierCode;
      }

      try
      {
        UpdateHealthInsuranceCompany();
        ++local.CompaniesUpdated.Count;
        ++local.CommitCount.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HEALTH_INSURANCE_COMPANY_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "HEALTH_INSURANCE_COMPANY_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (local.CommitCount.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.CommitCount.Count = 0;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      foreach(var item in ReadHealthInsuranceCoverage())
      {
        local.HealthInsuranceCoverage.Assign(entities.HealthInsuranceCoverage);
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode1 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode1 = local.CovCode.ActionEntry;
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode2 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode2 = local.CovCode.ActionEntry;
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode3 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode3 = local.CovCode.ActionEntry;
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode4 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode4 = local.CovCode.ActionEntry;
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode5 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode5 = local.CovCode.ActionEntry;
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode6 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode6 = local.CovCode.ActionEntry;
        local.CovCode.ActionEntry =
          entities.HealthInsuranceCoverage.CoverageCode7 ?? Spaces(2);
        UseOeB496FixCoverageCode();
        local.HealthInsuranceCoverage.CoverageCode7 = local.CovCode.ActionEntry;

        try
        {
          UpdateHealthInsuranceCoverage();
          ++local.CoveragesUpdated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "HEALTH_INSURANCE_COVERAGE_NU_RB";

              goto Test;
            case ErrorCode.PermittedValueViolation:
              ExitState = "HEALTH_INSURANCE_COVERAGE_PV_RB";

              goto Test;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (local.CommitCount.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.CommitCount.Count = 0;
      }
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB496Close();
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB496Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseOeB496Close()
  {
    var useImport = new OeB496Close.Import();
    var useExport = new OeB496Close.Export();

    useImport.CompaniesUpdated.Count = local.CompaniesUpdated.Count;
    useImport.CoveragesUpdated.Count = local.CoveragesUpdated.Count;

    Call(OeB496Close.Execute, useImport, useExport);
  }

  private void UseOeB496FixCoverageCode()
  {
    var useImport = new OeB496FixCoverageCode.Import();
    var useExport = new OeB496FixCoverageCode.Export();

    useImport.CovCode.ActionEntry = local.CovCode.ActionEntry;

    Call(OeB496FixCoverageCode.Execute, useImport, useExport);

    local.CovCode.ActionEntry = useExport.CovCode.ActionEntry;
  }

  private void UseOeB496Housekeeping()
  {
    var useImport = new OeB496Housekeeping.Import();
    var useExport = new OeB496Housekeeping.Export();

    Call(OeB496Housekeeping.Execute, useImport, useExport);

    local.Start.Date = useExport.Start.Date;
    local.Max.Date = useExport.Max.Date;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private IEnumerable<bool> ReadHealthInsuranceCompany()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return ReadEach("ReadHealthInsuranceCompany",
      null,
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.LastUpdatedBy = db.GetString(reader, 2);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 4);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 5);
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage",
      null,
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 8);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private void UpdateHealthInsuranceCompany()
  {
    var carrierCode = local.HealthInsuranceCompany.CarrierCode ?? "";
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var startDate = local.Start.Date;
    var endDate = local.Max.Date;

    entities.HealthInsuranceCompany.Populated = false;
    Update("UpdateHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetNullableString(command, "carrierCode", carrierCode);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetDate(command, "startDate", startDate);
        db.SetDate(command, "endDate", endDate);
        db.SetInt32(
          command, "identifier", entities.HealthInsuranceCompany.Identifier);
      });

    entities.HealthInsuranceCompany.CarrierCode = carrierCode;
    entities.HealthInsuranceCompany.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCompany.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.HealthInsuranceCompany.StartDate = startDate;
    entities.HealthInsuranceCompany.EndDate = endDate;
    entities.HealthInsuranceCompany.Populated = true;
  }

  private void UpdateHealthInsuranceCoverage()
  {
    var coverageCode1 = local.HealthInsuranceCoverage.CoverageCode1 ?? "";
    var coverageCode2 = local.HealthInsuranceCoverage.CoverageCode2 ?? "";
    var coverageCode3 = local.HealthInsuranceCoverage.CoverageCode3 ?? "";
    var coverageCode4 = local.HealthInsuranceCoverage.CoverageCode4 ?? "";
    var coverageCode5 = local.HealthInsuranceCoverage.CoverageCode5 ?? "";
    var coverageCode6 = local.HealthInsuranceCoverage.CoverageCode6 ?? "";
    var coverageCode7 = local.HealthInsuranceCoverage.CoverageCode7 ?? "";
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.HealthInsuranceCoverage.Populated = false;
    Update("UpdateHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableString(command, "coverageCode1", coverageCode1);
        db.SetNullableString(command, "coverageCode2", coverageCode2);
        db.SetNullableString(command, "coverageCode3", coverageCode3);
        db.SetNullableString(command, "coverageCode4", coverageCode4);
        db.SetNullableString(command, "coverageCode5", coverageCode5);
        db.SetNullableString(command, "coverageCode6", coverageCode6);
        db.SetNullableString(command, "coverageCode7", coverageCode7);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.CoverageCode1 = coverageCode1;
    entities.HealthInsuranceCoverage.CoverageCode2 = coverageCode2;
    entities.HealthInsuranceCoverage.CoverageCode3 = coverageCode3;
    entities.HealthInsuranceCoverage.CoverageCode4 = coverageCode4;
    entities.HealthInsuranceCoverage.CoverageCode5 = coverageCode5;
    entities.HealthInsuranceCoverage.CoverageCode6 = coverageCode6;
    entities.HealthInsuranceCoverage.CoverageCode7 = coverageCode7;
    entities.HealthInsuranceCoverage.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.HealthInsuranceCoverage.Populated = true;
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
    /// A value of CovCode.
    /// </summary>
    [JsonPropertyName("covCode")]
    public Common CovCode
    {
      get => covCode ??= new();
      set => covCode = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of CompaniesUpdated.
    /// </summary>
    [JsonPropertyName("companiesUpdated")]
    public Common CompaniesUpdated
    {
      get => companiesUpdated ??= new();
      set => companiesUpdated = value;
    }

    /// <summary>
    /// A value of CoveragesUpdated.
    /// </summary>
    [JsonPropertyName("coveragesUpdated")]
    public Common CoveragesUpdated
    {
      get => coveragesUpdated ??= new();
      set => coveragesUpdated = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    private Common covCode;
    private DateWorkArea max;
    private DateWorkArea start;
    private DateWorkArea null1;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private DateWorkArea current;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common commitCount;
    private Common companiesUpdated;
    private Common coveragesUpdated;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend neededToWrite;
    private HealthInsuranceCompany healthInsuranceCompany;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
  }
#endregion
}
