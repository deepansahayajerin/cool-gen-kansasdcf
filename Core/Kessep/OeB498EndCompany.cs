// Program: OE_B498_END_COMPANY, ID: 371179379, model: 746.
// Short name: SWE01979
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B498_END_COMPANY.
/// </summary>
[Serializable]
public partial class OeB498EndCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B498_END_COMPANY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB498EndCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB498EndCompany.
  /// </summary>
  public OeB498EndCompany(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *          End Health Ins company and all related coverages.          *
    // ***********************************************************************
    // ***********************************************************************
    // *              M A I N T E N A N C E   L O G                          *
    // ***********************************************************************
    // *Date		Name      Work Req  		Description           *
    // ***********************************************************************
    // * 02/25/2003  	Ed Lyman  WR020311  	Initial Coding
    // * 08/25/2003	E.Shirk	  WR20311	Pre-production rewrite.
    // ***********************************************************************
    // ***********************************************************************
    // **         Initialize process variables.
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.CommitCount.Count = 0;
    export.TotHiCoveragesExpired.Count = 0;
    export.TotPersHiExpired.Count = 0;
    local.TextExpire.TextDate =
      NumberToString(DateToInt(import.ProgramProcessingInfo.ProcessDate), 8, 8);
      

    // ***********************************************************************
    // **         Get health insurance company.
    // ***********************************************************************
    if (!ReadHealthInsuranceCompany())
    {
      return;
    }

    // ***********************************************************************
    // **         Get health insurance coverages tied to company.
    // ***********************************************************************
    foreach(var item in ReadHealthInsuranceCoverageCsePerson())
    {
      // ***********************************************************************
      // **        Get any personal health insurance tied to coverage.
      // ***********************************************************************
      foreach(var item1 in ReadPersonalHealthInsurance())
      {
        // ***********************************************************************
        // **        Expire personal health insurance if not already done.
        // ***********************************************************************
        if (Lt(import.ProgramProcessingInfo.ProcessDate,
          entities.PersonalHealthInsurance.CoverageEndDate))
        {
          try
          {
            UpdatePersonalHealthInsurance();
            ++export.TotPersHiExpired.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PERSONAL_HEALTH_INSURANCE_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PERSONAL_HEALTH_INSURANCE_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      // ***********************************************************************
      // **        Expire health insurance coverage if not already done.
      // ***********************************************************************
      if (Lt(import.ProgramProcessingInfo.ProcessDate,
        entities.HealthInsuranceCoverage.PolicyExpirationDate))
      {
        try
        {
          UpdateHealthInsuranceCoverage();
          ++export.TotHiCoveragesExpired.Count;
          ++local.CommitCount.Count;

          if (local.CommitCount.Count > import
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            UseExtToDoACommit();
            local.CommitCount.Count = 0;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "HEALTH_INSURANCE_COVERAGE_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "HEALTH_INSURANCE_COVERAGE_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // ***********************************************************************
      // **        Send alert to each case/service provider the AP is tied.
      // ***********************************************************************
      foreach(var item1 in ReadCase())
      {
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CaseNumber = entities.Case1.Number;
        local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;
        local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.UserId = import.ProgramProcessingInfo.Name;
        local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;
        local.Infrastructure.EventId = 45;
        local.Infrastructure.ReasonCode = "HLTHINSCOENDED";
        local.Infrastructure.Detail =
          entities.HealthInsuranceCompany.CarrierCode + " " + TrimEnd
          (entities.HealthInsuranceCompany.InsurancePolicyCarrier) + " company ended on " +
          local.TextExpire.TextDate;
        UseSpCabCreateInfrastructure();

        // ***********************************************************************
        // **        Send INSUINFO document to AR.
        // ***********************************************************************
        if (ReadApplicantRecipient())
        {
          local.Document.Name = "INSUINFO";
          local.SpDocKey.KeyChild = entities.CsePerson.Number;
          local.SpDocKey.KeyCase = entities.Case1.Number;
          local.SpDocKey.KeyHealthInsCoverage =
            entities.HealthInsuranceCoverage.Identifier;
          local.FindDoc.Flag = "Y";
          UseSpCabDetermineInterstateDoc();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }

    // ***********************************************************************
    // **        Expire health insurance company.
    // ***********************************************************************
    if (Lt(import.ProgramProcessingInfo.ProcessDate,
      entities.HealthInsuranceCompany.EndDate))
    {
      try
      {
        UpdateHealthInsuranceCompany();
        UseExtToDoACommit();
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
    }
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyChild = source.KeyChild;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.FindDoc.Flag = local.FindDoc.Flag;

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private bool ReadApplicantRecipient()
  {
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadHealthInsuranceCompany()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.HealthInsuranceCompany.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.LastUpdatedBy = db.GetString(reader, 3);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 5);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverageCsePerson()
  {
    entities.HealthInsuranceCoverage.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverageCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "hicIdentifier", entities.HealthInsuranceCompany.Identifier);
          
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 4);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.HealthInsuranceCoverage.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return ReadEach("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.LastUpdatedBy =
          db.GetString(reader, 3);
        entities.PersonalHealthInsurance.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.PersonalHealthInsurance.Populated = true;

        return true;
      });
  }

  private void UpdateHealthInsuranceCompany()
  {
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();
    var endDate = import.ProgramProcessingInfo.ProcessDate;

    entities.HealthInsuranceCompany.Populated = false;
    Update("UpdateHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetDate(command, "endDate", endDate);
        db.SetInt32(
          command, "identifier", entities.HealthInsuranceCompany.Identifier);
      });

    entities.HealthInsuranceCompany.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCompany.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.HealthInsuranceCompany.EndDate = endDate;
    entities.HealthInsuranceCompany.Populated = true;
  }

  private void UpdateHealthInsuranceCoverage()
  {
    var policyExpirationDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    entities.HealthInsuranceCoverage.Populated = false;
    Update("UpdateHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableDate(command, "policyExpDate", policyExpirationDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt64(
          command, "identifier", entities.HealthInsuranceCoverage.Identifier);
      });

    entities.HealthInsuranceCoverage.PolicyExpirationDate =
      policyExpirationDate;
    entities.HealthInsuranceCoverage.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.HealthInsuranceCoverage.Populated = true;
  }

  private void UpdatePersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.PersonalHealthInsurance.Populated);

    var coverageEndDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    entities.PersonalHealthInsurance.Populated = false;
    Update("UpdatePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetNullableDate(command, "coverEndDate", coverageEndDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt64(command, "hcvId", entities.PersonalHealthInsurance.HcvId);
        db.SetString(
          command, "cspNumber", entities.PersonalHealthInsurance.CspNumber);
      });

    entities.PersonalHealthInsurance.CoverageEndDate = coverageEndDate;
    entities.PersonalHealthInsurance.LastUpdatedBy = lastUpdatedBy;
    entities.PersonalHealthInsurance.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.PersonalHealthInsurance.Populated = true;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private HealthInsuranceCompany healthInsuranceCompany;
    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotPersHiExpired.
    /// </summary>
    [JsonPropertyName("totPersHiExpired")]
    public Common TotPersHiExpired
    {
      get => totPersHiExpired ??= new();
      set => totPersHiExpired = value;
    }

    /// <summary>
    /// A value of TotHiCoveragesExpired.
    /// </summary>
    [JsonPropertyName("totHiCoveragesExpired")]
    public Common TotHiCoveragesExpired
    {
      get => totHiCoveragesExpired ??= new();
      set => totHiCoveragesExpired = value;
    }

    private Common totPersHiExpired;
    private Common totHiCoveragesExpired;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of TextExpire.
    /// </summary>
    [JsonPropertyName("textExpire")]
    public DateWorkArea TextExpire
    {
      get => textExpire ??= new();
      set => textExpire = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    private Common commitCount;
    private Infrastructure infrastructure;
    private DateWorkArea textExpire;
    private Document document;
    private SpDocKey spDocKey;
    private Common findDoc;
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

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private CaseRole caseRole;
  }
#endregion
}
