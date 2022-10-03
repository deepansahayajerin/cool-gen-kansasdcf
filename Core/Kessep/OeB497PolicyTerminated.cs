// Program: OE_B497_POLICY_TERMINATED, ID: 371178270, model: 746.
// Short name: SWE01976
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B497_POLICY_TERMINATED.
/// </summary>
[Serializable]
public partial class OeB497PolicyTerminated: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B497_POLICY_TERMINATED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB497PolicyTerminated(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB497PolicyTerminated.
  /// </summary>
  public OeB497PolicyTerminated(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************
    // * Errors are reported to Report 01 -- Report all errors.      *
    // ***************************************************************
    export.CompanyInfoValid.Flag = "Y";
    local.EabFileHandling.Action = "WRITE";
    export.AlreadyProcessed.Count = import.AlreadyProcessed.Count;
    export.PoliciesTerminated.Count = import.PoliciesTerminated.Count;
    export.PoliciesNotFound.Count = import.PoliciesNotFound.Count;
    local.Termination.TextDate =
      NumberToString(DateToInt(import.PolicyTerminated.Date), 8, 8);

    if (Lt(import.HealthInsuranceCompany.CarrierCode, "0000001") || Lt
      ("9999999", import.HealthInsuranceCompany.CarrierCode))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail = import.CsePersonsWorkSet.Number + " " + (
        import.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
        (import.HealthInsuranceCoverage.InsurancePolicyNumber, 20, 1, 16) + " " +
        local.Termination.TextDate + " " + "ERROR:" + " Carrier code is either not greater than zero or not numeric.";
        
      UseCabBusinessReport01();
    }

    if (IsEmpty(import.HealthInsuranceCoverage.InsurancePolicyNumber))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail = import.CsePersonsWorkSet.Number + " " + (
        import.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
        (import.HealthInsuranceCoverage.InsurancePolicyNumber, 20, 1, 16) + " " +
        local.Termination.TextDate + " " + "ERROR:" + " Policy number is missing.";
        
      UseCabBusinessReport01();
    }

    if (Equal(import.CsePersonsWorkSet.Number, "0000000000"))
    {
      export.CompanyInfoValid.Flag = "N";
      local.NeededToWrite.RptDetail = import.CsePersonsWorkSet.Number + " " + (
        import.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
        (import.HealthInsuranceCoverage.InsurancePolicyNumber, 20, 1, 16) + " " +
        local.Termination.TextDate + " " + "ERROR:" + " Insured person number is missing or not numeric.";
        
      UseCabBusinessReport01();
    }

    // ************************************************************************
    // **        Bypass record if errors detected.
    // ************************************************************************
    if (AsChar(export.CompanyInfoValid.Flag) != 'Y')
    {
      return;
    }

    if (ReadHealthInsuranceCoverageHealthInsuranceCompany())
    {
      if (Lt(entities.HealthInsuranceCoverage.PolicyExpirationDate,
        import.Max.Date))
      {
        ++export.AlreadyProcessed.Count;

        if (Equal(entities.HealthInsuranceCoverage.PolicyExpirationDate,
          import.PolicyTerminated.Date))
        {
          local.NeededToWrite.RptDetail = import.CsePersonsWorkSet.Number + " " +
            (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
            (import.HealthInsuranceCoverage.InsurancePolicyNumber, 20, 1, 16) +
            " " + local.Termination.TextDate + " " + "" + " Already processed.";
            
        }
        else
        {
          local.PolicyExpiration.TextDate =
            NumberToString(DateToInt(
              entities.HealthInsuranceCoverage.PolicyExpirationDate), 8, 8);
          local.NeededToWrite.RptDetail = import.CsePersonsWorkSet.Number + " " +
            (import.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
            (import.HealthInsuranceCoverage.InsurancePolicyNumber, 20, 1, 16) +
            " " + local.Termination.TextDate + " " + "DIFFERENCE:  CSE shows policy expired on: " +
            local.PolicyExpiration.TextDate;
        }

        UseCabBusinessReport01();
      }
      else
      {
        UseOeTerminateHealthInsPolicy();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.PoliciesTerminated.Count;
      }
    }
    else
    {
      ++export.PoliciesNotFound.Count;
      local.NeededToWrite.RptDetail = import.CsePersonsWorkSet.Number + " " + (
        import.HealthInsuranceCompany.CarrierCode ?? "") + " " + Substring
        (import.HealthInsuranceCoverage.InsurancePolicyNumber, 20, 1, 16) + " " +
        local.Termination.TextDate + " " + "ERROR:" + " Policy not found.";
      UseCabBusinessReport01();
    }
  }

  private static void MoveHealthInsuranceCompany(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeTerminateHealthInsPolicy()
  {
    var useImport = new OeTerminateHealthInsPolicy.Import();
    var useExport = new OeTerminateHealthInsPolicy.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Pers.Assign(entities.HealthInsuranceCoverage);
    MoveHealthInsuranceCompany(entities.HealthInsuranceCompany,
      useImport.HealthInsuranceCompany);

    Call(OeTerminateHealthInsPolicy.Execute, useImport, useExport);

    MoveHealthInsuranceCoverage(useImport.Pers, entities.HealthInsuranceCoverage);
      
  }

  private bool ReadHealthInsuranceCoverageHealthInsuranceCompany()
  {
    entities.HealthInsuranceCompany.Populated = false;
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverageHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyNumber",
          import.HealthInsuranceCoverage.InsurancePolicyNumber ?? "");
        db.SetNullableString(
          command, "carrierCode", import.HealthInsuranceCompany.CarrierCode ?? ""
          );
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 2);
        entities.HealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 3);
        entities.HealthInsuranceCoverage.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 5);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompany.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    /// <summary>
    /// A value of PoliciesTerminated.
    /// </summary>
    [JsonPropertyName("policiesTerminated")]
    public Common PoliciesTerminated
    {
      get => policiesTerminated ??= new();
      set => policiesTerminated = value;
    }

    /// <summary>
    /// A value of PoliciesNotFound.
    /// </summary>
    [JsonPropertyName("policiesNotFound")]
    public Common PoliciesNotFound
    {
      get => policiesNotFound ??= new();
      set => policiesNotFound = value;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of PolicyTerminated.
    /// </summary>
    [JsonPropertyName("policyTerminated")]
    public DateWorkArea PolicyTerminated
    {
      get => policyTerminated ??= new();
      set => policyTerminated = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common alreadyProcessed;
    private Common policiesTerminated;
    private Common policiesNotFound;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private DateWorkArea policyTerminated;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PoliciesNotFound.
    /// </summary>
    [JsonPropertyName("policiesNotFound")]
    public Common PoliciesNotFound
    {
      get => policiesNotFound ??= new();
      set => policiesNotFound = value;
    }

    /// <summary>
    /// A value of PoliciesTerminated.
    /// </summary>
    [JsonPropertyName("policiesTerminated")]
    public Common PoliciesTerminated
    {
      get => policiesTerminated ??= new();
      set => policiesTerminated = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    /// <summary>
    /// A value of CompanyInfoValid.
    /// </summary>
    [JsonPropertyName("companyInfoValid")]
    public Common CompanyInfoValid
    {
      get => companyInfoValid ??= new();
      set => companyInfoValid = value;
    }

    private Common policiesNotFound;
    private Common policiesTerminated;
    private Common alreadyProcessed;
    private Common companyInfoValid;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PolicyExpiration.
    /// </summary>
    [JsonPropertyName("policyExpiration")]
    public DateWorkArea PolicyExpiration
    {
      get => policyExpiration ??= new();
      set => policyExpiration = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    private DateWorkArea policyExpiration;
    private DateWorkArea termination;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePerson csePerson;
  }
#endregion
}
