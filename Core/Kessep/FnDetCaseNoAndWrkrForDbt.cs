// Program: FN_DET_CASE_NO_AND_WRKR_FOR_DBT, ID: 372910407, model: 746.
// Short name: SWE00555
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DET_CASE_NO_AND_WRKR_FOR_DBT.
/// </para>
/// <para>
/// RESP: FNCLMGMT
/// This action block will determine the Obligee for a Collection.
/// </para>
/// </summary>
[Serializable]
public partial class FnDetCaseNoAndWrkrForDbt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DET_CASE_NO_AND_WRKR_FOR_DBT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetCaseNoAndWrkrForDbt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetCaseNoAndWrkrForDbt.
  /// </summary>
  public FnDetCaseNoAndWrkrForDbt(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Date	By		Description
    // ---------------------------------------------
    // This Action Block was built using a copy of 
    // fn_det_obligee_for_oblig_coll.  Out of PR77622, 76960, and 77437 users
    // made decision to return Case No and Worker ID in a manner similar to
    // Disbursements. - E. Parker
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    UseFnHardcodedDebtDistribution();

    if (import.ObligationType.SystemGeneratedIdentifier == local
      .HardcodedVoluntary.SystemGeneratedIdentifier)
    {
      // All voluntary obligations will use the current date to find the most 
      // recent Case.
      if (ReadCaseCsePersonApplicantRecipientCsePersonChild())
      {
        export.Case1.Number = entities.Case1.Number;
      }
      else if (ReadCaseApplicantRecipientCsePerson())
      {
        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        // Continue
      }
    }
    else
    {
      // All non-voluntary obligations will have a debt_detail due_date 
      // available to use to find the Case.
      // If the obligation is for spousal support then the supported person is 
      // the AR.
      if (import.ObligationType.SystemGeneratedIdentifier == local
        .HardcodedSpousalSupport.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == local
        .HardcodedSpArrearsJudgmt.SystemGeneratedIdentifier)
      {
        // Check to make sure the case is not closed.
        if (ReadCaseApplicantRecipient())
        {
          export.Case1.Number = entities.Case1.Number;
        }
        else
        {
          // Continue
        }

        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        // Supported person is child.
        if (ReadCsePersonCaseCsePersonApplicantRecipient())
        {
          // Continue
        }
        else
        {
          if (ReadCaseChildCsePersonCsePersonApplicantRecipient2())
          {
            goto Read;
          }

          if (ReadCaseChildCsePersonCsePersonApplicantRecipient1())
          {
            goto Read;
          }

          goto Test;
        }

Read:

        export.Case1.Number = entities.Case1.Number;
      }
    }

Test:

    UseSpCabDetOspAssgndToCsecase();
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedVoluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.HardcodedSpArrearsJudgmt.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.HardcodedSpousalSupport.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
  }

  private void UseSpCabDetOspAssgndToCsecase()
  {
    var useImport = new SpCabDetOspAssgndToCsecase.Import();
    var useExport = new SpCabDetOspAssgndToCsecase.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SpCabDetOspAssgndToCsecase.Execute, useImport, useExport);

    MoveServiceProvider(useExport.ServiceProvider, export.ServiceProvider);
  }

  private bool ReadCaseApplicantRecipient()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCaseApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", import.Supported.Number);
        db.SetNullableDate(
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
      });
  }

  private bool ReadCaseApplicantRecipientCsePerson()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCaseApplicantRecipientCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCaseChildCsePersonCsePersonApplicantRecipient1()
  {
    entities.Child1.Populated = false;
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;
    entities.Child2.Populated = false;

    return Read("ReadCaseChildCsePersonCsePersonApplicantRecipient1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Supported.Number);
        db.SetNullableDate(
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child2.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child2.CspNumber = db.GetString(reader, 2);
        entities.Child1.Number = db.GetString(reader, 2);
        entities.Child2.Type1 = db.GetString(reader, 3);
        entities.Child2.Identifier = db.GetInt32(reader, 4);
        entities.Child2.StartDate = db.GetNullableDate(reader, 5);
        entities.Child2.EndDate = db.GetNullableDate(reader, 6);
        entities.Ar.Number = db.GetString(reader, 7);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 7);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 8);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 9);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 10);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 11);
        entities.Child1.Populated = true;
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        entities.Child2.Populated = true;
      });
  }

  private bool ReadCaseChildCsePersonCsePersonApplicantRecipient2()
  {
    entities.Child1.Populated = false;
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;
    entities.Child2.Populated = false;

    return Read("ReadCaseChildCsePersonCsePersonApplicantRecipient2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Supported.Number);
        db.SetNullableDate(
          command, "endDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child2.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child2.CspNumber = db.GetString(reader, 2);
        entities.Child1.Number = db.GetString(reader, 2);
        entities.Child2.Type1 = db.GetString(reader, 3);
        entities.Child2.Identifier = db.GetInt32(reader, 4);
        entities.Child2.StartDate = db.GetNullableDate(reader, 5);
        entities.Child2.EndDate = db.GetNullableDate(reader, 6);
        entities.Ar.Number = db.GetString(reader, 7);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 7);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 8);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 9);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 10);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 11);
        entities.Child1.Populated = true;
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        entities.Child2.Populated = true;
      });
  }

  private bool ReadCaseCsePersonApplicantRecipientCsePersonChild()
  {
    entities.Child1.Populated = false;
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;
    entities.Child2.Populated = false;

    return Read("ReadCaseCsePersonApplicantRecipientCsePersonChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Child2.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Child1.Number = db.GetString(reader, 7);
        entities.Child2.CspNumber = db.GetString(reader, 7);
        entities.Child2.Type1 = db.GetString(reader, 8);
        entities.Child2.Identifier = db.GetInt32(reader, 9);
        entities.Child2.StartDate = db.GetNullableDate(reader, 10);
        entities.Child2.EndDate = db.GetNullableDate(reader, 11);
        entities.Child1.Populated = true;
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        entities.Child2.Populated = true;
      });
  }

  private bool ReadCsePersonCaseCsePersonApplicantRecipient()
  {
    entities.Child1.Populated = false;
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonCaseCsePersonApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", import.Supported.Number);
        db.SetNullableDate(
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Child1.Number = db.GetString(reader, 3);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 4);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 5);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 7);
        entities.Child1.Populated = true;
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private DebtDetail debtDetail;
    private CsePerson supported;
    private ObligationType obligationType;
    private CsePerson obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private ServiceProvider serviceProvider;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodedSpArrearsJudgmt.
    /// </summary>
    [JsonPropertyName("hardcodedSpArrearsJudgmt")]
    public ObligationType HardcodedSpArrearsJudgmt
    {
      get => hardcodedSpArrearsJudgmt ??= new();
      set => hardcodedSpArrearsJudgmt = value;
    }

    /// <summary>
    /// A value of HardcodedSpousalSupport.
    /// </summary>
    [JsonPropertyName("hardcodedSpousalSupport")]
    public ObligationType HardcodedSpousalSupport
    {
      get => hardcodedSpousalSupport ??= new();
      set => hardcodedSpousalSupport = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private ObligationType hardcodedVoluntary;
    private ObligationType hardcodedSpArrearsJudgmt;
    private ObligationType hardcodedSpousalSupport;
    private DateWorkArea current;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CsePerson Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CaseRole Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    private CsePerson child1;
    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson obligor;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private CsePersonAccount obligee;
    private CaseRole child2;
    private CsePersonAccount supported1;
    private CsePerson supported2;
  }
#endregion
}
