// Program: OE_B493_GET_POLICY_HOLDER_INFO, ID: 371176777, model: 746.
// Short name: SWE02483
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B493_GET_POLICY_HOLDER_INFO.
/// </summary>
[Serializable]
public partial class OeB493GetPolicyHolderInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B493_GET_POLICY_HOLDER_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB493GetPolicyHolderInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB493GetPolicyHolderInfo.
  /// </summary>
  public OeB493GetPolicyHolderInfo(IContext context, Import import,
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
    if (ReadContact())
    {
      export.PolicyHolder.Ssn = "";
      export.PolicyHolder.Number = "";
      export.PolicyHolder.FirstName = entities.Contact.NameFirst ?? Spaces(12);
      export.PolicyHolder.LastName = entities.Contact.NameLast ?? Spaces(17);

      if (ReadCodeValue())
      {
        export.RelationToBeneficiary.SelectChar =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        export.RelationToBeneficiary.SelectChar = "Z";
      }
    }
    else if (ReadCsePerson())
    {
      export.PolicyHolder.Number = entities.PolicyHolderCsePerson.Number;
      UseEabReadCsePersonBatch();

      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";
              local.NeededToWrite.RptDetail =
                "Adabas response code 113, person not found for " + entities
                .PolicyHolderCsePerson.Number;

              break;
            case "0148":
              ExitState = "ADABAS_UNAVAILABLE_RB";
              local.NeededToWrite.RptDetail =
                "Adabas response code 148, unavailable fetching person " + entities
                .PolicyHolderCsePerson.Number;

              break;
            default:
              ExitState = "ADABAS_READ_UNSUCCESSFUL";
              local.NeededToWrite.RptDetail = "Adabas error. Type = " + local
                .AbendData.Type1 + " File number = " + local
                .AbendData.AdabasFileNumber + " File action = " + local
                .AbendData.AdabasFileAction + " Response code = " + local
                .AbendData.AdabasResponseCd + " Person number = " + entities
                .PolicyHolderCsePerson.Number;

              break;
          }

          break;
        case 'C':
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
            local.NeededToWrite.RptDetail =
              "CICS error fetching person number  " + entities
              .PolicyHolderCsePerson.Number;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ADABAS_INVALID_RETURN_CODE";
          local.NeededToWrite.RptDetail =
            "Unknown error fetching person number  " + entities
            .PolicyHolderCsePerson.Number;

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        return;
      }

      foreach(var item in ReadCaseRole())
      {
        switch(TrimEnd(entities.PolicyHolderCaseRole.Type1))
        {
          case "FA":
            export.RelationToBeneficiary.SelectChar = "A";

            goto ReadEach;
          case "MO":
            export.RelationToBeneficiary.SelectChar = "B";

            goto ReadEach;
          case "AP":
            export.RelationToBeneficiary.SelectChar = "F";

            break;
          default:
            break;
        }
      }

ReadEach:

      if (IsEmpty(export.RelationToBeneficiary.SelectChar))
      {
        export.RelationToBeneficiary.SelectChar = "Z";
      }

      if (ReadIncomeSource())
      {
        if (ReadEmployerEmployerAddress())
        {
          MoveEmployer(entities.Employer, export.Employer);
          export.EmployerAddress.Assign(entities.EmployerAddress);
        }
      }
    }
    else
    {
      // **************************************
      // Report as error? Don't send?
      // **************************************
    }
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.PolicyHolder.Number;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(export.PolicyHolder);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.PolicyHolder.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.PolicyHolderCaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber1", entities.PolicyHolderCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", import.ProcessDate.Date.GetValueOrDefault());
        db.
          SetString(command, "cspNumber2", import.PersistentBeneficiary.Number);
          
      },
      (db, reader) =>
      {
        entities.PolicyHolderCaseRole.CasNumber = db.GetString(reader, 0);
        entities.PolicyHolderCaseRole.CspNumber = db.GetString(reader, 1);
        entities.PolicyHolderCaseRole.Type1 = db.GetString(reader, 2);
        entities.PolicyHolderCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.PolicyHolderCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.PolicyHolderCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.PolicyHolderCaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.EdsRelationshipTable.Id);
        db.SetNullableString(
          command, "relationshipToCsePerson",
          entities.Contact.RelationshipToCsePerson ?? "");
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadContact()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber",
          import.Persistent.ConHNumber.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Persistent.CspHNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Contact.NameLast = db.GetNullableString(reader, 3);
        entities.Contact.NameFirst = db.GetNullableString(reader, 4);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.PolicyHolderCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Persistent.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.PolicyHolderCsePerson.Number = db.GetString(reader, 0);
        entities.PolicyHolderCsePerson.Populated = true;
      });
  }

  private bool ReadEmployerEmployerAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Employer.Populated = false;
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.EmployerAddress.LocationType = db.GetString(reader, 3);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 4);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.City = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 7);
        entities.EmployerAddress.State = db.GetNullableString(reader, 8);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 9);
        entities.Employer.Populated = true;
        entities.EmployerAddress.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.Persistent.IsrIdentifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", import.Persistent.CseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 2);
        entities.IncomeSource.CspINumber = db.GetString(reader, 3);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Populated = true;
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
    /// A value of EdsRelationshipTable.
    /// </summary>
    [JsonPropertyName("edsRelationshipTable")]
    public Code EdsRelationshipTable
    {
      get => edsRelationshipTable ??= new();
      set => edsRelationshipTable = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of PersistentBeneficiary.
    /// </summary>
    [JsonPropertyName("persistentBeneficiary")]
    public CsePerson PersistentBeneficiary
    {
      get => persistentBeneficiary ??= new();
      set => persistentBeneficiary = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public HealthInsuranceCoverage Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private Code edsRelationshipTable;
    private DateWorkArea processDate;
    private CsePerson persistentBeneficiary;
    private HealthInsuranceCoverage persistent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of RelationToBeneficiary.
    /// </summary>
    [JsonPropertyName("relationToBeneficiary")]
    public Common RelationToBeneficiary
    {
      get => relationToBeneficiary ??= new();
      set => relationToBeneficiary = value;
    }

    /// <summary>
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    public CsePersonsWorkSet PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
    }

    private Employer employer;
    private EmployerAddress employerAddress;
    private Common relationToBeneficiary;
    private CsePersonsWorkSet policyHolder;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private AbendData abendData;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of PolicyHolderCsePerson.
    /// </summary>
    [JsonPropertyName("policyHolderCsePerson")]
    public CsePerson PolicyHolderCsePerson
    {
      get => policyHolderCsePerson ??= new();
      set => policyHolderCsePerson = value;
    }

    /// <summary>
    /// A value of PolicyHolderCaseRole.
    /// </summary>
    [JsonPropertyName("policyHolderCaseRole")]
    public CaseRole PolicyHolderCaseRole
    {
      get => policyHolderCaseRole ??= new();
      set => policyHolderCaseRole = value;
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
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    public CaseRole CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
    }

    private CodeValue codeValue;
    private Code code;
    private IncomeSource incomeSource;
    private Employer employer;
    private EmployerAddress employerAddress;
    private Contact contact;
    private CsePerson policyHolderCsePerson;
    private CaseRole policyHolderCaseRole;
    private Case1 case1;
    private CaseRole coveredPerson;
  }
#endregion
}
