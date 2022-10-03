// Program: OE_TERMINATE_HEALTH_INS_POLICY, ID: 371178452, model: 746.
// Short name: SWE01980
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_TERMINATE_HEALTH_INS_POLICY.
/// </summary>
[Serializable]
public partial class OeTerminateHealthInsPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_TERMINATE_HEALTH_INS_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeTerminateHealthInsPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeTerminateHealthInsPolicy.
  /// </summary>
  public OeTerminateHealthInsPolicy(IContext context, Import import,
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
    local.Termination.TextDate =
      NumberToString(DateToInt(import.ProgramProcessingInfo.ProcessDate), 8, 8);
      
    local.CloseCoverage.Flag = "Y";

    // ******************************************************************
    // *   End date the insurance for the bene passed in.
    // *  Keep track of whether any other bene's are actively tied to this 
    // coverage.
    // *  If so, then do not close coverage.
    // ******************************************************************
    foreach(var item in ReadPersonalHealthInsuranceCsePerson())
    {
      if (Equal(entities.CsePerson.Number, import.CsePersonsWorkSet.Number))
      {
        try
        {
          UpdatePersonalHealthInsurance();
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

        // ******************************************************************
        // **  Send alert to each case the bene is tied.
        // ******************************************************************
        foreach(var item1 in ReadCase())
        {
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = import.ProgramProcessingInfo.Name;
          local.Infrastructure.CreatedBy = import.ProgramProcessingInfo.Name;

          switch(TrimEnd(import.ProgramProcessingInfo.Name))
          {
            case "SWEEB497":
              // ********** Process Terminated Policies ********************
              local.Infrastructure.EventId = 80;
              local.Infrastructure.ReasonCode = "HLTHINSENDED";
              local.Infrastructure.Detail =
                (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + TrimEnd
                (import.HealthInsuranceCompany.InsurancePolicyCarrier) + " coverage ended on " +
                local.Termination.TextDate;

              break;
            case "SWEEB498":
              // **********   Process Ended Companies   ********************
              local.Infrastructure.EventId = 45;
              local.Infrastructure.ReasonCode = "HLTHINSCOENDED";
              local.Infrastructure.Detail =
                (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + TrimEnd
                (import.HealthInsuranceCompany.InsurancePolicyCarrier) + " company ended on " +
                local.Termination.TextDate;

              break;
            default:
              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
          }

          UseSpCabCreateInfrastructure();

          // ---------------------------------------------
          // 11/14/02  MCA -  Send INSUINFO letter. Only send letter if AR is a
          // "C"lient.
          // ---------------------------------------------
          if (ReadApplicantRecipient())
          {
            local.Document.Name = "INSUINFO";
            local.SpDocKey.KeyChild = entities.CsePerson.Number;
            local.SpDocKey.KeyCase = entities.Case1.Number;
            local.SpDocKey.KeyHealthInsCoverage = import.Pers.Identifier;
            local.FindDoc.Flag = "Y";
            UseSpCabDetermineInterstateDoc();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
      }
      else
      {
        local.CloseCoverage.Flag = "N";
      }
    }

    // ******************************************************************
    // *   End date coverage if no open bene's are tied.
    // ******************************************************************
    if (AsChar(local.CloseCoverage.Flag) == 'Y')
    {
      try
      {
        UpdateHealthInsuranceCoverage();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HEALTH_INSURANCE_COVERAGE_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "HEALTH_INSURANCE_COVERAGE_PV_RB";

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

  private IEnumerable<bool> ReadPersonalHealthInsuranceCsePerson()
  {
    entities.PersonalHealthInsurance.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadPersonalHealthInsuranceCsePerson",
      (db, command) =>
      {
        db.SetInt64(command, "hcvId", import.Pers.Identifier);
        db.SetNullableDate(
          command, "coverEndDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.LastUpdatedBy =
          db.GetString(reader, 3);
        entities.PersonalHealthInsurance.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CsePerson.Type1 = db.GetString(reader, 5);
        entities.PersonalHealthInsurance.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private void UpdateHealthInsuranceCoverage()
  {
    var policyExpirationDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    import.Pers.Populated = false;
    Update("UpdateHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableDate(command, "policyExpDate", policyExpirationDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt64(command, "identifier", import.Pers.Identifier);
      });

    import.Pers.PolicyExpirationDate = policyExpirationDate;
    import.Pers.LastUpdatedBy = lastUpdatedBy;
    import.Pers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    import.Pers.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Pers.
    /// </summary>
    [JsonPropertyName("pers")]
    public HealthInsuranceCoverage Pers
    {
      get => pers ??= new();
      set => pers = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private ProgramProcessingInfo programProcessingInfo;
    private HealthInsuranceCoverage pers;
    private HealthInsuranceCompany healthInsuranceCompany;
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
    /// A value of CloseCoverage.
    /// </summary>
    [JsonPropertyName("closeCoverage")]
    public Common CloseCoverage
    {
      get => closeCoverage ??= new();
      set => closeCoverage = value;
    }

    /// <summary>
    /// A value of Termination.
    /// </summary>
    [JsonPropertyName("termination")]
    public DateWorkArea Termination
    {
      get => termination ??= new();
      set => termination = value;
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

    private Common closeCoverage;
    private DateWorkArea termination;
    private Infrastructure infrastructure;
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

    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private CaseRole caseRole;
  }
#endregion
}
