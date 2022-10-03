// Program: FN_RETRIEVE_LEGL_FOR_REC_AND_FEE, ID: 372159446, model: 746.
// Short name: SWE02258
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RETRIEVE_LEGL_FOR_REC_AND_FEE.
/// </summary>
[Serializable]
public partial class FnRetrieveLeglForRecAndFee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RETRIEVE_LEGL_FOR_REC_AND_FEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRetrieveLeglForRecAndFee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRetrieveLeglForRecAndFee.
  /// </summary>
  public FnRetrieveLeglForRecAndFee(IContext context, Import import,
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
    // =================================================
    // 12/11/1998 - bud adams  -  Read properties set
    // 03/09/2004  GVandy	PR202338  Correct abend due to lack of currency on 
    // obligor cse_person.
    // =================================================
    if (ReadLegalAction())
    {
      // =================================================
      // 12/11/98 - B Adams  -  This is not the proper way to determine
      //   Interstate-related data.  Currently, this is being handled by
      //   FN_Retrieve_Interstate_Request.
      // =================================================
      export.LegalAction.Assign(entities.LegalAction);
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (ReadLegalActionDetail())
    {
      export.ObligationTransaction.Amount =
        entities.LegalActionDetail.JudgementAmount.GetValueOrDefault();
      export.DebtDetail.DueDt = entities.LegalActionDetail.EffectiveDate;
      export.LegalActionDetail.JudgementAmount =
        entities.LegalActionDetail.JudgementAmount;

      // ** If there is a Frequency_Period code, set up the 
      // obligation_payment_schedule fields to create it in the CAB
      // CREATE_OBLIGATION
      if (!IsEmpty(entities.LegalActionDetail.FreqPeriodCode))
      {
        export.ObligationPaymentSchedule.Amount =
          entities.LegalActionDetail.ArrearsAmount;
        export.ObligationPaymentSchedule.DayOfMonth1 =
          entities.LegalActionDetail.DayOfMonth1;
        export.ObligationPaymentSchedule.DayOfMonth2 =
          entities.LegalActionDetail.DayOfMonth2;
        export.ObligationPaymentSchedule.DayOfWeek =
          entities.LegalActionDetail.DayOfWeek;
        export.ObligationPaymentSchedule.StartDt =
          entities.LegalActionDetail.EffectiveDate;
        export.ObligationPaymentSchedule.EndDt =
          entities.LegalActionDetail.EndDate;
        export.ObligationPaymentSchedule.FrequencyCode =
          entities.LegalActionDetail.FreqPeriodCode ?? Spaces(2);
        UseFnSetFrequencyTextField();
        export.ObligationPaymentSchedule.PeriodInd =
          entities.LegalActionDetail.PeriodInd;
      }

      // =================================================
      // 5/24/99 - Bud Adams  -  If an obligation has already been
      //   created for this detail, then we want the procedure to
      //   DISPLAY it; otherwise, we want it to BYPASS.
      // 1/11/00 - Bud Adams  -  PR# 83926: If obligation is deactive,
      //   meaning Debt_Detail Retired_Dt = NULL date, then do not
      //   consider it.
      // =================================================
      if (ReadObligation())
      {
        export.DebtExists.Flag = "Y";
        MoveObligation(entities.Obligation, local.Obligation);
      }
      else
      {
        // ***---  OK; optional
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    // *** Get the Obligor for this Debt
    if (ReadCsePerson3())
    {
      export.CpaObligorOrObligee.Type1 = import.HcCpaObligor.Type1;
    }
    else
    {
      // #################################################
      // #################################################
      // IF it turns out that LOPS is changed so that both Fees and
      // Recoveries can only have one Obligor, then this will be
      // changed back to be only one read, and to not qualify on the
      // value of CPA Type.
      // #################################################
      // #################################################
      ExitState = "CSE_PERSON_NF";

      return;
    }

    MoveCsePerson(entities.Obligor, export.ObligorCsePerson);
    export.ObligorCsePersonsWorkSet.Number = entities.Obligor.Number;
    UseEabReadCsePerson();

    if (!IsEmpty(local.Eab.Type1))
    {
      ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";

      return;
    }

    UseSiFormatCsePersonName();

    // ** Obligation Type must exist and must have classification of "recovery" 
    // or "fee". ***
    if (ReadObligationType())
    {
      export.ObligationType.Assign(entities.ObligationType);
    }
    else
    {
      ExitState = "OBLIGATION_TYPE_NF";

      return;
    }

    if (ReadInterstateRequest())
    {
      export.InterstateRequest.Assign(entities.InterstateRequest);
    }
    else
    {
      // -----------------------------------------------------------------------
      //           O.K.     Obligation needs to be added.
      // --------------------------------------------------------------------
    }

    if (local.Obligation.SystemGeneratedIdentifier > 0)
    {
      UseFnGetObligationAssignment();
    }

    // -------------------------------------------------------------------------
    // Check if there is Manual Distribution associated with this Obligation.
    // ------------------------------------------------------------------------
    if (ReadManualDistributionAudit())
    {
      export.ManualDistributionInd.Flag = "Y";
    }
    else
    {
      export.ManualDistributionInd.Flag = "N";
    }

    // =================================================
    // 1/27/99 - B Adams  -  Well, now legal IS going to enter this
    //   information.  Code re-activated.  The way it will work, for now, is:
    //    * If legal enters an alternate billing location, then ALL debts
    //      associated with that legal action will use it.  Period.
    //    * if not, then each debt can have a different alternate billing
    //      location entered from the Debt screen.
    //    * if this happens, and later an alternate billing location is
    //      entered in LACT, then each existing alternate billing location
    //      for each debt will be changed to the one just entered.
    // =================================================
    // =================================================
    // 1/4/99 - B Adams  -   Legal isn't going to enter this at all.  Code 
    // deactivated.
    // =================================================
    // =================================================
    // 12/29/98 - b adams  -  If an alternate billing location was
    //   entered by Legal then it must be picked up and displayed
    // =================================================
    if (ReadCsePerson2())
    {
      export.Alternate.Number = entities.AlternateAddr.Number;
      export.Alternate.Char2 = "LE";
      UseSiReadCsePerson();

      if (!IsEmpty(local.Eab.Type1))
      {
        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }
    else
    {
      // ------------------------------------------------------------------------------
      // Check if associated to an Obligation  ( may be this is entered in 
      // FINANCE).
      // ----------------------------------------------------------------------------
      if (ReadCsePerson1())
      {
        export.Alternate.Number = entities.AlternateAddr.Number;
        export.Alternate.Char2 = "FN";
        UseSiReadCsePerson();

        if (!IsEmpty(local.Eab.Type1))
        {
          ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
        }
      }
      else
      {
        // ***--- OK to continue ...
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;
    useExport.AbendData.Assign(local.Eab);
    MoveCsePersonsWorkSet2(export.ObligorCsePersonsWorkSet,
      useExport.CsePersonsWorkSet);

    Call(EabReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
  }

  private void UseFnGetObligationAssignment()
  {
    var useImport = new FnGetObligationAssignment.Import();
    var useExport = new FnGetObligationAssignment.Export();

    useImport.CsePerson.Number = entities.Obligor.Number;
    useImport.Current.Date = import.Current.Date;
    useImport.CsePersonAccount.Type1 = import.HcCpaObligor.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      local.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnGetObligationAssignment.Execute, useImport, useExport);

    export.AssignServiceProvider.Assign(useExport.ServiceProvider);
    export.AssignCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.ObligorCsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.ObligorCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Alternate.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, export.Alternate);
  }

  private bool ReadCsePerson1()
  {
    entities.AlternateAddr.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", local.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", export.CpaObligorOrObligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.AlternateAddr.Number = db.GetString(reader, 0);
        entities.AlternateAddr.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.AlternateAddr.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AlternateAddr.Number = db.GetString(reader, 0);
        entities.AlternateAddr.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligor.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "accountType", import.HcLapObligorAcctType.AccountType ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Obligor.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.OrganizationName = db.GetNullableString(reader, 2);
        entities.Obligor.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          local.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 5);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.Type1 = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 8);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 12);
        entities.LegalActionDetail.Description = db.GetString(reader, 13);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadManualDistributionAudit()
  {
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
        db.SetInt32(
          command, "obgGeneratedId",
          local.Obligation.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "retiredDt", local.Blank.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 5);
        entities.Obligation.Description = db.GetNullableString(reader, 6);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 7);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 8);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 9);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 13);
        entities.Obligation.CreatedBy = db.GetString(reader, 14);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 17);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 18);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 19);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 20);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(
          command, "debtTypClass", import.HcOtRecoveryOrFee.Classification);
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of HcLapObligorAcctType.
    /// </summary>
    [JsonPropertyName("hcLapObligorAcctType")]
    public LegalActionPerson HcLapObligorAcctType
    {
      get => hcLapObligorAcctType ??= new();
      set => hcLapObligorAcctType = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of HcOtRecoveryOrFee.
    /// </summary>
    [JsonPropertyName("hcOtRecoveryOrFee")]
    public ObligationType HcOtRecoveryOrFee
    {
      get => hcOtRecoveryOrFee ??= new();
      set => hcOtRecoveryOrFee = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of HcCpaObligee.
    /// </summary>
    [JsonPropertyName("hcCpaObligee")]
    public CsePersonAccount HcCpaObligee
    {
      get => hcCpaObligee ??= new();
      set => hcCpaObligee = value;
    }

    private LegalActionPerson hcLapObligorAcctType;
    private LegalAction legalAction;
    private DateWorkArea current;
    private CsePersonAccount hcCpaObligor;
    private ObligationType hcOtRecoveryOrFee;
    private LegalActionDetail legalActionDetail;
    private CsePersonAccount hcCpaObligee;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
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
    /// A value of CpaObligorOrObligee.
    /// </summary>
    [JsonPropertyName("cpaObligorOrObligee")]
    public CsePersonAccount CpaObligorOrObligee
    {
      get => cpaObligorOrObligee ??= new();
      set => cpaObligorOrObligee = value;
    }

    /// <summary>
    /// A value of DebtExists.
    /// </summary>
    [JsonPropertyName("debtExists")]
    public Common DebtExists
    {
      get => debtExists ??= new();
      set => debtExists = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of AssignServiceProvider.
    /// </summary>
    [JsonPropertyName("assignServiceProvider")]
    public ServiceProvider AssignServiceProvider
    {
      get => assignServiceProvider ??= new();
      set => assignServiceProvider = value;
    }

    /// <summary>
    /// A value of AssignCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("assignCsePersonsWorkSet")]
    public CsePersonsWorkSet AssignCsePersonsWorkSet
    {
      get => assignCsePersonsWorkSet ??= new();
      set => assignCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    private CsePersonsWorkSet alternate;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private LegalAction legalAction;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private ObligationType obligationType;
    private CsePersonAccount cpaObligorOrObligee;
    private Common debtExists;
    private InterstateRequest interstateRequest;
    private ServiceProvider assignServiceProvider;
    private CsePersonsWorkSet assignCsePersonsWorkSet;
    private Common manualDistributionInd;
    private FrequencyWorkSet frequencyWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public CsePerson Alt
    {
      get => alt ??= new();
      set => alt = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private DateWorkArea blank;
    private AbendData eab;
    private Obligation obligation;
    private CsePerson alt;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
    }

    /// <summary>
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
    }

    /// <summary>
    /// A value of AlternateAddr.
    /// </summary>
    [JsonPropertyName("alternateAddr")]
    public CsePerson AlternateAddr
    {
      get => alternateAddr ??= new();
      set => alternateAddr = value;
    }

    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
    private LegalActionDetail legalActionDetail;
    private CsePerson obligor;
    private CsePersonAccount csePersonAccount;
    private LegalActionPerson legalActionPerson;
    private ObligationType obligationType;
    private InterstateRequest interstateRequest;
    private InterstateRequestObligation interstateRequestObligation;
    private ManualDistributionAudit manualDistributionAudit;
    private CsePerson alternateAddr;
  }
#endregion
}
