// Program: FN_DETERMINE_CASE_AND_AR2, ID: 945112053, model: 746.
// Short name: SWE03673
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
/// A program: FN_DETERMINE_CASE_AND_AR2.
/// </para>
/// <para>
/// RESP: FNCLMGMT
/// This action block will determine the Obligee and case for a give debt, 
/// supported person, and obligor.
/// </para>
/// </summary>
[Serializable]
public partial class FnDetermineCaseAndAr2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_CASE_AND_AR2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineCaseAndAr2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineCaseAndAr2.
  /// </summary>
  public FnDetermineCaseAndAr2(IContext context, Import import, Export export):
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
    // July, 2000 Maureen Brown      Updated logic to take out read qualifier 
    // for an open case.
    // ---------------------------------------------
    local.TheStatesPersonNumber.Number = "000000017O";
    export.Obligee.Number = "";
    export.EabReportSend.RptDetail = "";

    // : VOLUNTARY
    if (import.Hardcode.HardcodeVoluntary.SystemGeneratedIdentifier == import
      .ObligationType.SystemGeneratedIdentifier)
    {
      if (ReadCaseCsePerson1())
      {
        export.Case1.Number = entities.Case1.Number;
        export.Obligee.Number = entities.Ar.Number;
      }
      else
      {
        // : See if the AR is the supported for this voluntary.
        if (ReadCaseCsePerson2())
        {
          export.Case1.Number = entities.Case1.Number;
          export.Obligee.Number = entities.Ar.Number;
        }
        else
        {
          export.Condition.Text8 = "VOL NF";
          export.EabReportSend.RptDetail = "Voluntary - Case not found";
          ExitState = "CASE_NF";
        }
      }

      return;
    }

    // If the obligation is for spousal support then the supported person is the
    // AR.
    if (import.ObligationType.SystemGeneratedIdentifier == import
      .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier || import
      .ObligationType.SystemGeneratedIdentifier == import
      .Hardcode.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier)
    {
      export.Obligee.Number = import.PerSupported.Number;

      // : Supported person is child.(copied from child now targeting spousal)
      if (ReadCase2())
      {
        export.Case1.Number = entities.Case1.Number;

        // Now find the AR for the case & debt detail due date.
        if (ReadCsePersonApplicantRecipient1())
        {
          export.Obligee.Number = entities.Ar.Number;

          return;
        }
        else
        {
          export.Condition.Text8 = "NO AR";
          export.EabReportSend.RptDetail =
            "Spousal support - AR nf on Debt Dtl Due Dt:";
          ExitState = "FN0000_COULD_NOT_DET_OBLGEE";

          return;
        }
      }
      else
      {
        // : continue
      }

      // The case was not found so try to find the case by going BACK IN TIME 
      // from the debt detail due date using the child's case role end date.
      foreach(var item in ReadCaseApplicantRecipient2())
      {
        export.Case1.Number = entities.Case1.Number;

        // Now find the AR for the case & spousal's end date.
        if (ReadCsePerson())
        {
          export.Obligee.Number = entities.Ar.Number;

          return;
        }
        else
        {
          export.Condition.Text8 = "NO AR";
          ExitState = "FN0000_COULD_NOT_DET_OBLGEE";
          export.EabReportSend.RptDetail =
            "Spousal support - AR nf going backward in time:";

          return;
        }
      }

      // The case was not found so try to find the case by going FORWARD IN TIME
      // from the debt detail due date using the spousal's case role start
      // date.
      foreach(var item in ReadCaseApplicantRecipient1())
      {
        export.Case1.Number = entities.Case1.Number;

        // Now find the AR for the case & spousal's start date.
        if (ReadCsePerson())
        {
          export.Obligee.Number = entities.Ar.Number;

          return;
        }
        else
        {
          export.Condition.Text8 = "NO AR";
          ExitState = "FN0000_COULD_NOT_DET_OBLGEE";
          export.EabReportSend.RptDetail =
            "Spousal support - AR nf going forward in time:";

          return;
        }
      }

      // : Case was not found
      export.Condition.Text8 = "SPOUS NF";
      export.EabReportSend.RptDetail = "Spousal support - case nf:";
      ExitState = "CASE_NF";

      return;
    }

    // : Supported person is child.
    if (ReadCase1())
    {
      export.Case1.Number = entities.Case1.Number;

      // Now find the AR for the case & debt detail due date.
      if (ReadCsePersonApplicantRecipient1())
      {
        export.Obligee.Number = entities.Ar.Number;

        return;
      }
      else
      {
        export.Condition.Text8 = "NO AR";
        ExitState = "FN0000_COULD_NOT_DET_OBLGEE";
        export.EabReportSend.RptDetail =
          "Child support - AR nf on Debt Dtl Due Dt:";

        return;
      }
    }
    else
    {
      // : continue
    }

    // The case was not found so try to find the case by going BACK IN TIME from
    // the debt detail due date using the child's case role end date.
    foreach(var item in ReadCaseChild2())
    {
      export.Case1.Number = entities.Case1.Number;

      // Now find the AR for the case & child's end date.
      if (ReadCsePersonApplicantRecipient2())
      {
        export.Obligee.Number = entities.Ar.Number;

        return;
      }
      else
      {
        export.Condition.Text8 = "NO AR";
        ExitState = "FN0000_COULD_NOT_DET_OBLGEE";
        export.EabReportSend.RptDetail =
          "Child support - AR nf going backward in time:";

        return;
      }
    }

    // The case was not found so try to find the case by going FORWARD IN TIME 
    // from the debt detail due date using the child's case role start date.
    foreach(var item in ReadCaseChild1())
    {
      export.Case1.Number = entities.Case1.Number;

      // Now find the AR for the case & child's start date.
      if (ReadCsePersonApplicantRecipient3())
      {
        export.Obligee.Number = entities.Ar.Number;

        return;
      }
      else
      {
        export.Condition.Text8 = "NO AR";
        ExitState = "FN0000_COULD_NOT_DET_OBLGEE";
        export.EabReportSend.RptDetail =
          "Child support - AR nf going forward in time:";

        return;
      }
    }

    // : If we get to here, we did not find a case, so  set up a message.
    if (IsEmpty(export.Case1.Number))
    {
      export.Condition.Text8 = "CS NF";
      ExitState = "CASE_NF";
      export.EabReportSend.RptDetail = "Child support - case nf:";
    }
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", import.PerSupported.Number);
        db.SetNullableDate(
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.PerSupported.Number);
        db.SetNullableDate(
          command, "endDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseApplicantRecipient1()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return ReadEach("ReadCaseApplicantRecipient1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.PerSupported.Number);
        db.SetNullableDate(
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
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
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseApplicantRecipient2()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return ReadEach("ReadCaseApplicantRecipient2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.PerSupported.Number);
        db.SetNullableDate(
          command, "endDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
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
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseChild1()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseChild1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.PerSupported.Number);
        db.SetNullableDate(
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseChild2()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseChild2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.PerSupported.Number);
        db.SetNullableDate(
          command, "endDate", import.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private bool ReadCaseCsePerson1()
  {
    entities.Case1.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCaseCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", import.PerSupported.Number);
        db.SetNullableDate(
          command, "startDate", import.Eom.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.Bom.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.Case1.Populated = true;
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCaseCsePerson2()
  {
    entities.Case1.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCaseCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", import.PerSupported.Number);
        db.SetNullableDate(
          command, "startDate", import.Eom.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.Bom.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.Case1.Populated = true;
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ApplicantRecipient.Populated);
    entities.Ar.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ApplicantRecipient.CspNumber);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
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
          command, "startDate", import.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
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
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
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
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
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
    /// <summary>A HardcodeGroup group.</summary>
    [Serializable]
    public class HardcodeGroup
    {
      /// <summary>
      /// A value of HardcodeVoluntary.
      /// </summary>
      [JsonPropertyName("hardcodeVoluntary")]
      public ObligationType HardcodeVoluntary
      {
        get => hardcodeVoluntary ??= new();
        set => hardcodeVoluntary = value;
      }

      /// <summary>
      /// A value of HardcodeSpousalSupport.
      /// </summary>
      [JsonPropertyName("hardcodeSpousalSupport")]
      public ObligationType HardcodeSpousalSupport
      {
        get => hardcodeSpousalSupport ??= new();
        set => hardcodeSpousalSupport = value;
      }

      /// <summary>
      /// A value of HardcodeSpArrearsJudgmt.
      /// </summary>
      [JsonPropertyName("hardcodeSpArrearsJudgmt")]
      public ObligationType HardcodeSpArrearsJudgmt
      {
        get => hardcodeSpArrearsJudgmt ??= new();
        set => hardcodeSpArrearsJudgmt = value;
      }

      private ObligationType hardcodeVoluntary;
      private ObligationType hardcodeSpousalSupport;
      private ObligationType hardcodeSpArrearsJudgmt;
    }

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
    /// A value of PerSupported.
    /// </summary>
    [JsonPropertyName("perSupported")]
    public CsePerson PerSupported
    {
      get => perSupported ??= new();
      set => perSupported = value;
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
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    /// <summary>
    /// Gets a value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public HardcodeGroup Hardcode
    {
      get => hardcode ?? (hardcode = new());
      set => hardcode = value;
    }

    private DebtDetail debtDetail;
    private CsePerson perSupported;
    private ObligationType obligationType;
    private CsePerson obligor;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private HardcodeGroup hardcode;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of Condition.
    /// </summary>
    [JsonPropertyName("condition")]
    public TextWorkArea Condition
    {
      get => condition ??= new();
      set => condition = value;
    }

    private EabReportSend eabReportSend;
    private CsePerson obligee;
    private Case1 case1;
    private TextWorkArea condition;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber")]
    public CsePerson TheStatesPersonNumber
    {
      get => theStatesPersonNumber ??= new();
      set => theStatesPersonNumber = value;
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

    private Common caseCount;
    private CsePerson theStatesPersonNumber;
    private DateWorkArea initialized;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson obligor;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private CaseRole child;
    private CsePersonAccount supported;
  }
#endregion
}
