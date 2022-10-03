// Program: FN_READ_LEGAL_DETAIL_FOR_OBLIG, ID: 372084597, model: 746.
// Short name: SWE01590
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_READ_LEGAL_DETAIL_FOR_OBLIG.
/// </summary>
[Serializable]
public partial class FnReadLegalDetailForOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_LEGAL_DETAIL_FOR_OBLIG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadLegalDetailForOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadLegalDetailForOblig.
  /// </summary>
  public FnReadLegalDetailForOblig(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ===============================================
    // ==
    // ==		This is used only by OACC
    // ==
    // ===============================================
    // ********
    //  8/26/98   Bud Adams	Added org-name to Obligor and Supported_
    // 			Person CSE_Person so they view match OK
    // 9/8/98   Bud Adams	SETs to export-grp-detail program code
    // 			changed to export-grp program code;
    // 			different group
    // ********
    // *** 9/9/98  B Adams   "pgm" local views were never SET; replaced with 
    // imports from PrAD  ***
    // ****************************************************************
    // 11/3/99 - E. Parker	Put fn_read_case_no_and_worker_id back in as we don't
    // need to find a specific case, we just need to check for any case.
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadLegalAction())
    {
      export.LegalAction.Assign(entities.LegalAction);

      // =================================================
      // 2/4/00 - b adams  -  PR# 85965: The determination if a debt
      //   is interstate or not is left to the user; it is not dependent on
      //   information entered via Legal screens.  Also, the read of
      //   FIPS was unnecessary.
      //   Code Deleted.
      // =================================================
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // *** If an Alternate Billing Address exists, retrieve it and pass it to 
    // the PRAD ***
    //  *-- Sumanta - 04/28/97
    //      The read for FIPS has been deleted.
    //      Instead the follwoing read (cse person)has been added..
    if (ReadCsePerson())
    {
      if (!IsEmpty(entities.CsePerson.Number))
      {
        // =================================================
        // 2/17/1999 - Bud Adams  -  If alternate billing address has
        //   been designated via the LACT screen, then all debts for that
        //   Legal_Action must use that address; it must be protected
        //   on the screen.
        // =================================================
        export.Alternate.Char2 = "LE";
        export.Alternate.Number = entities.CsePerson.Number;
      }
    }

    // *** 9/9/98  B Adams   Combined Read of obligation-type from below  ***
    if (ReadLegalActionDetailObligationType())
    {
      // =================================================
      // PR# 79463: 11/12/99 - b adams  -  Now, discontinue dates
      //   in legal will have no connection with finance.  So, initially,
      //   the obligation discontinue date will be blank.
      // =================================================
      if (Equal(export.Discontinue.Date, import.Maximum.Date))
      {
        export.Discontinue.Date = local.Zero.Date;
      }

      export.ObligationPaymentSchedule.FrequencyCode =
        entities.LegalActionDetail.FreqPeriodCode ?? Spaces(2);
      export.ObligationType.Assign(entities.ObligationType);
      UseFnSetFrequencyTextField();

      // =================================================
      // 4/8/99 - bud adams  -  At one time, LDET was putting the DOW
      //   value in either DOM1 or DOM2, and Converted debts had it
      //   where it was supposed to be, in DOW of L_A_D.  After we
      //   got them to change that, all the logic that was here to figure
      //   out when the O_P_S DOW gets populated could be gotten
      //   rid of.  Deactivated the code.
      // 2/4/00 - b adams - Deleted that code.
      // =================================================
      export.ObligationPaymentSchedule.DayOfWeek =
        entities.LegalActionDetail.DayOfWeek;
      export.ObligationPaymentSchedule.DayOfMonth1 =
        entities.LegalActionDetail.DayOfMonth1;
      export.ObligationPaymentSchedule.DayOfMonth2 =
        entities.LegalActionDetail.DayOfMonth2;
      export.ObligationPaymentSchedule.StartDt =
        entities.LegalActionDetail.EffectiveDate;
      export.ObligationPaymentSchedule.Amount =
        entities.LegalActionDetail.CurrentAmount;
    }
    else
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    // *** 9/9/98  B Adams    Combined Read of obligation-type from here with 
    // Read above.  ***
    local.Common.Count = 0;

    foreach(var item in ReadLegalActionPersonCsePerson1())
    {
      if (local.Common.Count == 2)
      {
        break;
      }

      ++local.Common.Count;

      // =================================================
      // 2/25/1999 - B Adams  -  If a user only partially updates conversion
      //   debts on LDET but not LOPS, then we must exit, tell them
      //   that they've got to update LOPS
      // =================================================
      switch(local.Common.Count)
      {
        case 1:
          export.ObligorCsePerson.Number = entities.Obligor.Number;
          local.Common.TotalCurrency =
            entities.LegalActionPerson.JudgementAmount.GetValueOrDefault() + entities
            .LegalActionPerson.ArrearsAmount.GetValueOrDefault() + entities
            .LegalActionPerson.CurrentAmount.GetValueOrDefault();

          break;
        case 2:
          export.ConcurrentObligorCsePerson.Number = entities.Obligor.Number;
          local.Common.TotalCurrency =
            entities.LegalActionPerson.JudgementAmount.GetValueOrDefault() + entities
            .LegalActionPerson.ArrearsAmount.GetValueOrDefault() + entities
            .LegalActionPerson.CurrentAmount.GetValueOrDefault();

          break;
        default:
          break;
      }

      if (local.Common.TotalCurrency == 0 && Equal
        (entities.LegalActionPerson.CreatedBy, "CONVERSN"))
      {
        ExitState = "FN0000_CONVRSN_OBLIG_UPDATE_LOPS";

        return;
      }
    }

    if (local.Common.Count == 0)
    {
      ExitState = "FN0008_OBLIGOR_NOT_PRESENT";

      return;
    }

    local.Common.Count = 0;

    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadLegalActionPersonCsePerson2())
    {
      export.Group.Update.SupportedCsePerson.Number = entities.Supported.Number;
      export.Group.Update.ObligationTransaction.Amount =
        entities.LegalActionPerson.CurrentAmount.GetValueOrDefault();

      // =================================================
      // PR# 79463: 11/12/99 - b adams  -  Now, discontinue dates
      //   in finance will have no connection with legal.  So, initially,
      //   all supported person disc. dates will be blank.
      // =================================================
      local.Discontinue.Date = new DateTime(1960, 1, 1);
      UseFnReadCaseNoAndWorkerId();

      // ***---  Exit state is from FN_Read_Case_NO_And_Worker_ID
      if (Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
        import.Maximum.Date))
      {
        export.Group.Update.AccrualInstructions.DiscontinueDt = local.Zero.Date;
      }

      // =================================================
      // 2/3/98 - B Adams  -  Non-case related persons must
      //   not have their Case Number, Service_Provider User_ID, or
      //   Program Code displayed.
      // 5/5/99 - B Adams  -  But they are not supposed to have debt_
      //   details or obligation_transactions created for them.  So this
      //   "Z" will indicate to Establish_Accr_Oblig_Detail that this
      //   person is non-case, so don't create those things.
      // =================================================
      if (IsExitState("NO_CASE_RL_FOUND_FOR_SUPP_PERSON"))
      {
        export.Group.Update.ProgramScreenAttributes.ProgramTypeInd = "Z";
        ExitState = "ACO_NN0000_ALL_OK";
        export.Group.Next();

        continue;
      }

      // =================================================
      // PR# 75248: 10/5/99 - Bud Adams  -  If the Legal_Action_
      //   Person is not yet ended, then the Accrual_Instruction
      //   Discontinue_Date for that person must be unprotected.
      //   ALSO, this will allow those dates to be set to the same
      //   date as the Obligation_Payment_Schedule End_Date (Disc
      //   Date for the obligation on the screen).
      // PR# 79463: 11/12/99 - b adams  -  Now, discontinue dates
      //   in finance will have no connection with legal.  So, initially,
      //   all supported person disc. dates will be unprotected.
      // =================================================
      export.Group.Update.Sel.Flag = "U";
      ++local.Common.Count;

      // **** get the program codes for the supported person ****
      // =================================================
      // 2/11/1999 - bud adams  -  replaced the Reads, etc., with this CAB
      // =================================================
      // =================================================
      // 4/2/99 - b adams  -  FN_Read_Program_For_Supp_Person
      //   removed.  Prgram code for supported person has no value
      //   and no real meaning; only causes confusion.
      // =================================================
      export.Group.Next();
    }

    if (local.Common.Count == 0)
    {
      ExitState = "FN0000_NO_SUPPORTD_PRSN_ON_LDET";

      return;
    }

    export.ManualDistb.Flag = "N";
    export.SuspendInt.Flag = "N";
    export.SuspendAccrual.Flag = "N";
    ExitState = "ACO_NN0000_ALL_OK";
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
  }

  private void UseFnReadCaseNoAndWorkerId()
  {
    var useImport = new FnReadCaseNoAndWorkerId.Import();
    var useExport = new FnReadCaseNoAndWorkerId.Export();

    useImport.Supported.Number = entities.Supported.Number;
    useImport.SearchDiscontinue.Date = local.Discontinue.Date;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnReadCaseNoAndWorkerId.Execute, useImport, useExport);

    export.Group.Update.Case1.Number = useExport.Case1.Number;
    export.Group.Update.ServiceProvider.UserId =
      useExport.ServiceProvider.UserId;
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    MoveObligationPaymentSchedule(export.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.FrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 4);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetailObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 6);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.DayOfMonth2 = db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligationType.Code = db.GetString(reader, 11);
        entities.ObligationType.Name = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligor.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "accountType", import.HcLapObligor.AccountType ?? "");
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.Obligor.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 5);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.Obligor.Type1 = db.GetString(reader, 14);
        entities.Obligor.OrganizationName = db.GetNullableString(reader, 15);
        entities.Obligor.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Obligor.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "accountType", import.HcLapSupported.AccountType ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.Supported.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 5);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.Supported.Type1 = db.GetString(reader, 14);
        entities.Supported.OrganizationName = db.GetNullableString(reader, 15);
        entities.Supported.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Supported.Type1);

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of HcLapSupported.
    /// </summary>
    [JsonPropertyName("hcLapSupported")]
    public LegalActionPerson HcLapSupported
    {
      get => hcLapSupported ??= new();
      set => hcLapSupported = value;
    }

    /// <summary>
    /// A value of HcLapObligor.
    /// </summary>
    [JsonPropertyName("hcLapObligor")]
    public LegalActionPerson HcLapObligor
    {
      get => hcLapObligor ??= new();
      set => hcLapObligor = value;
    }

    /// <summary>
    /// A value of HcOpsCWeeklyFreqCode.
    /// </summary>
    [JsonPropertyName("hcOpsCWeeklyFreqCode")]
    public ObligationPaymentSchedule HcOpsCWeeklyFreqCode
    {
      get => hcOpsCWeeklyFreqCode ??= new();
      set => hcOpsCWeeklyFreqCode = value;
    }

    /// <summary>
    /// A value of HcPgmNonAdcFosterCar.
    /// </summary>
    [JsonPropertyName("hcPgmNonAdcFosterCar")]
    public ProgramScreenAttributes HcPgmNonAdcFosterCar
    {
      get => hcPgmNonAdcFosterCar ??= new();
      set => hcPgmNonAdcFosterCar = value;
    }

    /// <summary>
    /// A value of HdPgmAdc.
    /// </summary>
    [JsonPropertyName("hdPgmAdc")]
    public ProgramScreenAttributes HdPgmAdc
    {
      get => hdPgmAdc ??= new();
      set => hdPgmAdc = value;
    }

    /// <summary>
    /// A value of HcPgmAdcFosterCare.
    /// </summary>
    [JsonPropertyName("hcPgmAdcFosterCare")]
    public ProgramScreenAttributes HcPgmAdcFosterCare
    {
      get => hcPgmAdcFosterCare ??= new();
      set => hcPgmAdcFosterCare = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalActionPerson hcLapSupported;
    private LegalActionPerson hcLapObligor;
    private ObligationPaymentSchedule hcOpsCWeeklyFreqCode;
    private ProgramScreenAttributes hcPgmNonAdcFosterCar;
    private ProgramScreenAttributes hdPgmAdc;
    private ProgramScreenAttributes hcPgmAdcFosterCare;
    private DateWorkArea current;
    private DateWorkArea maximum;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public Program Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of SupportedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedCsePerson")]
      public CsePerson SupportedCsePerson
      {
        get => supportedCsePerson ??= new();
        set => supportedCsePerson = value;
      }

      /// <summary>
      /// A value of SupportedPrompt.
      /// </summary>
      [JsonPropertyName("supportedPrompt")]
      public Common SupportedPrompt
      {
        get => supportedPrompt ??= new();
        set => supportedPrompt = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ProratePercentage.
      /// </summary>
      [JsonPropertyName("proratePercentage")]
      public Common ProratePercentage
      {
        get => proratePercentage ??= new();
        set => proratePercentage = value;
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
      /// A value of ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("programScreenAttributes")]
      public ProgramScreenAttributes ProgramScreenAttributes
      {
        get => programScreenAttributes ??= new();
        set => programScreenAttributes = value;
      }

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
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of HiddenConcurrent.
      /// </summary>
      [JsonPropertyName("hiddenConcurrent")]
      public ObligationTransaction HiddenConcurrent
      {
        get => hiddenConcurrent ??= new();
        set => hiddenConcurrent = value;
      }

      /// <summary>
      /// A value of Previous.
      /// </summary>
      [JsonPropertyName("previous")]
      public ObligationTransaction Previous
      {
        get => previous ??= new();
        set => previous = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpDesigPayee.
      /// </summary>
      [JsonPropertyName("zdelExportGrpDesigPayee")]
      public CsePersonsWorkSet ZdelExportGrpDesigPayee
      {
        get => zdelExportGrpDesigPayee ??= new();
        set => zdelExportGrpDesigPayee = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public AccrualInstructions Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdel;
      private Common sel;
      private CsePerson supportedCsePerson;
      private Common supportedPrompt;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private Common proratePercentage;
      private ObligationTransaction obligationTransaction;
      private ProgramScreenAttributes programScreenAttributes;
      private ServiceProvider serviceProvider;
      private AccrualInstructions accrualInstructions;
      private ObligationTransaction hiddenConcurrent;
      private ObligationTransaction previous;
      private CsePersonsWorkSet zdelExportGrpDesigPayee;
      private AccrualInstructions hidden;
    }

    /// <summary>
    /// A value of InterstateInd.
    /// </summary>
    [JsonPropertyName("interstateInd")]
    public Obligation InterstateInd
    {
      get => interstateInd ??= new();
      set => interstateInd = value;
    }

    /// <summary>
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
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
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
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
    /// A value of ConcurrentObligorCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePerson")]
    public CsePerson ConcurrentObligorCsePerson
    {
      get => concurrentObligorCsePerson ??= new();
      set => concurrentObligorCsePerson = value;
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
    /// A value of ManualDistb.
    /// </summary>
    [JsonPropertyName("manualDistb")]
    public Common ManualDistb
    {
      get => manualDistb ??= new();
      set => manualDistb = value;
    }

    /// <summary>
    /// A value of SuspendAccrual.
    /// </summary>
    [JsonPropertyName("suspendAccrual")]
    public Common SuspendAccrual
    {
      get => suspendAccrual ??= new();
      set => suspendAccrual = value;
    }

    /// <summary>
    /// A value of SuspendInt.
    /// </summary>
    [JsonPropertyName("suspendInt")]
    public Common SuspendInt
    {
      get => suspendInt ??= new();
      set => suspendInt = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentObligorCsePersonsWorkSet
    {
      get => concurrentObligorCsePersonsWorkSet ??= new();
      set => concurrentObligorCsePersonsWorkSet = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    private Obligation interstateInd;
    private DateWorkArea discontinue;
    private LegalAction legalAction;
    private FrequencyWorkSet frequencyWorkSet;
    private ObligationType obligationType;
    private CsePerson concurrentObligorCsePerson;
    private CsePerson obligorCsePerson;
    private Common manualDistb;
    private Common suspendAccrual;
    private Common suspendInt;
    private CsePersonsWorkSet concurrentObligorCsePersonsWorkSet;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Array<GroupGroup> group;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePersonsWorkSet alternate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      zero = null;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
    private DateWorkArea zero;
    private DateWorkArea discontinue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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

    private CsePerson csePerson;
    private CsePerson supported;
    private CsePerson obligor;
    private ObligationType obligationType;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}
