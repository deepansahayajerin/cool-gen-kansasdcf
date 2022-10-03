// Program: OE_B492_GET_BENEFICIARY_INFO, ID: 371176058, model: 746.
// Short name: SWE02641
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B492_GET_BENEFICIARY_INFO.
/// </summary>
[Serializable]
public partial class OeB492GetBeneficiaryInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B492_GET_BENEFICIARY_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB492GetBeneficiaryInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB492GetBeneficiaryInfo.
  /// </summary>
  public OeB492GetBeneficiaryInfo(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.PersonFound.Flag = "N";
    export.OpenCaseFound.Flag = "N";

    // **********************************************************************
    // *  Find Bene on the case
    // 
    // *
    // **********************************************************************
    if (ReadCsePerson())
    {
      export.PersonFound.Flag = "Y";
      export.PolicyHolder.Number = entities.Beneficiary.Number;

      // **********************************************************************
      // * Determine if bene  is tied to any open cases.
      // **********************************************************************
      foreach(var item in ReadCaseRoleCase())
      {
        if (AsChar(entities.Case1.Status) == 'O')
        {
          export.OpenCaseFound.Flag = "Y";
        }
      }
    }
    else
    {
      export.PersonFound.Flag = "N";
      export.PolicyHolder.Number = import.Beneficiary.Number;
    }
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Beneficiary.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Beneficiary.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Beneficiary.Number);
      },
      (db, reader) =>
      {
        entities.Beneficiary.Number = db.GetString(reader, 0);
        entities.Beneficiary.Populated = true;
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
    /// A value of Beneficiary.
    /// </summary>
    [JsonPropertyName("beneficiary")]
    public CsePersonsWorkSet Beneficiary
    {
      get => beneficiary ??= new();
      set => beneficiary = value;
    }

    private CsePersonsWorkSet beneficiary;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OpenCaseFound.
    /// </summary>
    [JsonPropertyName("openCaseFound")]
    public Common OpenCaseFound
    {
      get => openCaseFound ??= new();
      set => openCaseFound = value;
    }

    /// <summary>
    /// A value of PersonFound.
    /// </summary>
    [JsonPropertyName("personFound")]
    public Common PersonFound
    {
      get => personFound ??= new();
      set => personFound = value;
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

    private Common openCaseFound;
    private Common personFound;
    private CsePersonsWorkSet policyHolder;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    /// <summary>
    /// A value of AmountRecovered.
    /// </summary>
    [JsonPropertyName("amountRecovered")]
    public Common AmountRecovered
    {
      get => amountRecovered ??= new();
      set => amountRecovered = value;
    }

    /// <summary>
    /// A value of OriginalIcn.
    /// </summary>
    [JsonPropertyName("originalIcn")]
    public WorkArea OriginalIcn
    {
      get => originalIcn ??= new();
      set => originalIcn = value;
    }

    /// <summary>
    /// A value of BirthExpenseCaseNum.
    /// </summary>
    [JsonPropertyName("birthExpenseCaseNum")]
    public WorkArea BirthExpenseCaseNum
    {
      get => birthExpenseCaseNum ??= new();
      set => birthExpenseCaseNum = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private Common amountRecovered;
    private WorkArea originalIcn;
    private WorkArea birthExpenseCaseNum;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Beneficiary.
    /// </summary>
    [JsonPropertyName("beneficiary")]
    public CsePerson Beneficiary
    {
      get => beneficiary ??= new();
      set => beneficiary = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson beneficiary;
  }
#endregion
}
