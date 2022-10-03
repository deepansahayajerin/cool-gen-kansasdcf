// Program: FN_B664_DETERMINE_AR, ID: 371233488, model: 746.
// Short name: SWE02022
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B664_DETERMINE_AR.
/// </summary>
[Serializable]
public partial class FnB664DetermineAr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B664_DETERMINE_AR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB664DetermineAr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB664DetermineAr.
  /// </summary>
  public FnB664DetermineAr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 02/17/05  GVandy	PR233867	Initial Development.  New business rules for AR
    // statements.
    // -------------------------------------------------------------------------------------------------------------------------
    // --  Use a modified version of the Non ADC rules from B650 to determine 
    // the AR.
    if (ReadCsePerson())
    {
      // Continue
    }
    else
    {
      return;
    }

    if (import.ObligationType.SystemGeneratedIdentifier == import
      .Voluntary.SystemGeneratedIdentifier)
    {
      // All voluntary obligations will use the collection created date to find 
      // the most recent Obligee.
      if (!ReadCaseCsePersonApplicantRecipientChild())
      {
        ReadCaseApplicantRecipientCsePerson();
      }
    }
    else
    {
      // If the obligation is for spousal support then the supported person is 
      // the AR.
      if (import.ObligationType.SystemGeneratedIdentifier == import
        .SpousalSupport.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == import
        .SpousalArrearsJudgement.SystemGeneratedIdentifier)
      {
        // Ensure the supported person is an AR on a case with the obligor.
        if (ReadCaseApplicantRecipient())
        {
          MoveCsePerson(entities.Supported2, export.Ar);
        }
      }
      else
      {
        // Supported person is child.
        // Try to find the case by the debt detail due date.
        if (ReadCase())
        {
          // Now find the AR for the case & debt detail due date.
          if (ReadCsePersonApplicantRecipient1())
          {
            MoveCsePerson(entities.Ar, export.Ar);
          }
        }
        else
        {
          // The case was not found so try to find the case by going BACK IN 
          // TIME from the debt detail due date using the child's case role end
          // date.
          if (ReadCaseChild2())
          {
            // Now find the AR for the case & child's end date.
            if (ReadCsePersonApplicantRecipient2())
            {
              MoveCsePerson(entities.Ar, export.Ar);
            }

            return;
          }

          // The case was not found so try to find the case by going FORWARD IN 
          // TIME from the debt detail due date using the child's case role
          // start date.
          if (ReadCaseChild1())
          {
            // Now find the AR for the case & child's start date.
            if (ReadCsePersonApplicantRecipient3())
            {
              MoveCsePerson(entities.Ar, export.Ar);
            }
          }
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetNullableDate(
          command, "statusDate",
          import.ReportingPeriodEnding.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", import.DueDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseApplicantRecipient()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCaseApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 3);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 4);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 5);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 7);
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
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetNullableDate(
          command, "startDate", import.DueDate.Date.GetValueOrDefault());
        db.SetString(command, "numb", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 3);
        entities.Ar.Number = db.GetString(reader, 3);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 4);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 5);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 7);
        entities.Ar.Type1 = db.GetString(reader, 8);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCaseChild1()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseChild1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", import.DueDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "statusDate",
          import.ReportingPeriodEnding.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Child.CspNumber = db.GetString(reader, 3);
        entities.Child.Type1 = db.GetString(reader, 4);
        entities.Child.Identifier = db.GetInt32(reader, 5);
        entities.Child.StartDate = db.GetNullableDate(reader, 6);
        entities.Child.EndDate = db.GetNullableDate(reader, 7);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
      });
  }

  private bool ReadCaseChild2()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseChild2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "endDate", import.DueDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "statusDate",
          import.ReportingPeriodEnding.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Child.CspNumber = db.GetString(reader, 3);
        entities.Child.Type1 = db.GetString(reader, 4);
        entities.Child.Identifier = db.GetInt32(reader, 5);
        entities.Child.StartDate = db.GetNullableDate(reader, 6);
        entities.Child.EndDate = db.GetNullableDate(reader, 7);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
      });
  }

  private bool ReadCaseCsePersonApplicantRecipientChild()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseCsePersonApplicantRecipientChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", import.DueDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Ar.Number = db.GetString(reader, 3);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 3);
        entities.Ar.Type1 = db.GetString(reader, 4);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 5);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 6);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 7);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 8);
        entities.Child.CspNumber = db.GetString(reader, 9);
        entities.Child.Type1 = db.GetString(reader, 10);
        entities.Child.Identifier = db.GetInt32(reader, 11);
        entities.Child.StartDate = db.GetNullableDate(reader, 12);
        entities.Child.EndDate = db.GetNullableDate(reader, 13);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        entities.Child.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Persistent.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Type1 = db.GetString(reader, 1);
        entities.Supported2.Populated = true;
      });
  }

  private bool ReadCsePersonApplicantRecipient1()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonApplicantRecipient1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.DueDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePersonApplicantRecipient2()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonApplicantRecipient2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", entities.Child.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePersonApplicantRecipient3()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonApplicantRecipient3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", entities.Child.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of ReportingPeriodEnding.
    /// </summary>
    [JsonPropertyName("reportingPeriodEnding")]
    public DateWorkArea ReportingPeriodEnding
    {
      get => reportingPeriodEnding ??= new();
      set => reportingPeriodEnding = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public ObligationTransaction Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    /// <summary>
    /// A value of SpousalSupport.
    /// </summary>
    [JsonPropertyName("spousalSupport")]
    public ObligationType SpousalSupport
    {
      get => spousalSupport ??= new();
      set => spousalSupport = value;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of Voluntary.
    /// </summary>
    [JsonPropertyName("voluntary")]
    public ObligationType Voluntary
    {
      get => voluntary ??= new();
      set => voluntary = value;
    }

    private DateWorkArea reportingPeriodEnding;
    private DateWorkArea dueDate;
    private ObligationTransaction persistent;
    private ObligationType obligationType;
    private CsePerson obligor;
    private ObligationType spousalSupport;
    private ObligationType spousalArrearsJudgement;
    private ObligationType voluntary;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private CsePerson ar;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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

    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson obligor;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private CsePersonAccount obligee;
    private CaseRole child;
    private CsePersonAccount supported1;
    private CsePerson supported2;
  }
#endregion
}
