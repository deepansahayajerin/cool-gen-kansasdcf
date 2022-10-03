// Program: FN_ESTABLISH_ACCR_OBLIG_DETAIL, ID: 372084603, model: 746.
// Short name: SWE00453
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
/// A program: FN_ESTABLISH_ACCR_OBLIG_DETAIL.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block manages creates of Obligation Transaction, Accrual 
/// Instructions, and Legal Action Detail Obligation entities.
/// </para>
/// </summary>
[Serializable]
public partial class FnEstablishAccrObligDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ESTABLISH_ACCR_OBLIG_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEstablishAccrObligDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEstablishAccrObligDetail.
  /// </summary>
  public FnEstablishAccrObligDetail(IContext context, Import import,
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 01/13/97 HOOKS	   raise events
    // 09/23/98  B Adams    deleted fn-hardcoded-debt-distribution and
    // 		   imported the attributes.
    // ---------------------------------------------
    // ================================================
    // Deleted Read obligor cse_person and included it in Read of Obligation 
    // below.
    // READ properties set
    // 11/12/99 - bud adams  -  Removed persistent Read actions
    //   and all references to those views.  Unnecessary 'intent'
    //   locks will result; used action diagrams only need imported
    //   data.
    // ================================================
    MoveInfrastructure(import.Infrastructure, local.Infrastructure);

    for(import.Export1.Index = 0; import.Export1.Index < import.Export1.Count; ++
      import.Export1.Index)
    {
      // =================================================
      // 5/5/99 - Bud Adams  -  Non-Case-Related persons are not
      //   supposed to have Debt_Details or Obligation_Transactions
      //   created for them.  They are supposed to be displayed only.
      // =================================================
      if (Equal(import.Export1.Item.ExportHidden.ProgramTypeInd, "Z"))
      {
        continue;
      }

      if (!IsEmpty(import.Export1.Item.ExportGrpSupportedCsePerson.Number))
      {
        local.SetAccruing.Flag = "Y";
        UseFnCreateObligationTransaction1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        import.Export1.Update.ExportHidden.ProgramTypeInd = local.Program.Text3;
        import.Export1.Update.ExportGrpPrev.Amount =
          import.Export1.Item.ExportObligationTransaction.Amount;

        // *********************************************
        // * This is logic to raise events for the     *
        // * Event Processor Infrastructure            *
        // *********************************************
        // : Aug 6, 1999, mfb - Added set of legal attributes - events were not 
        // working because the business object code needs to be set to 'LEA' in
        // order to end the legal monitored activity.
        local.Infrastructure.EventId = 45;
        local.Infrastructure.UserId = "OACC";
        local.Infrastructure.CsePersonNumber = import.Obligor.Number;

        if (import.LegalAction.Identifier == 0)
        {
          local.Infrastructure.BusinessObjectCd = "OBL";
          local.Infrastructure.DenormNumeric12 =
            import.Export1.Item.ExportObligationTransaction.
              SystemGeneratedIdentifier;
          local.Infrastructure.DenormText12 = import.HcOtrnTDebt.Type1;
          local.Infrastructure.ReferenceDate = import.Current.Date;
        }
        else
        {
          local.Infrastructure.BusinessObjectCd = "LEA";
          local.Infrastructure.DenormNumeric12 = import.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            import.LegalAction.CourtCaseNumber ?? "";
          local.Infrastructure.ReferenceDate = import.LegalAction.FiledDate;
        }

        UseFnCabRaiseEvent1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // If there is a concurrent obligation, must set up a relationship
        // between the obligation transaction entities.
        if (!IsEmpty(import.ConcurrentCsePerson.Number))
        {
          local.SetAccruing.Flag = "Y";
          UseFnCreateObligationTransaction2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseFnCreateObligationTranRln();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // *********************************************
          // * This is logic to raise events for the     *
          // * Event Processor Infrastructure            *
          // *********************************************
          local.Infrastructure.EventId = 45;
          local.Infrastructure.UserId = "OACC";
          local.Infrastructure.CsePersonNumber =
            import.ConcurrentCsePerson.Number;

          if (import.LegalAction.Identifier == 0)
          {
            local.Infrastructure.BusinessObjectCd = "OBL";
            local.Infrastructure.DenormNumeric12 =
              import.Export1.Item.ExportObligationTransaction.
                SystemGeneratedIdentifier;
            local.Infrastructure.DenormText12 = import.HcOtrnTDebt.Type1;
            local.Infrastructure.ReferenceDate = import.Current.Date;
          }
          else
          {
            local.Infrastructure.BusinessObjectCd = "LEA";
            local.Infrastructure.DenormNumeric12 =
              import.LegalAction.Identifier;
            local.Infrastructure.DenormText12 =
              import.LegalAction.CourtCaseNumber ?? "";
            local.Infrastructure.ReferenceDate = import.LegalAction.FiledDate;
          }

          UseFnCabRaiseEvent2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }

      import.Export1.Update.ExportGrpSel.SelectChar = "";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnCabRaiseEvent1()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Supported.Number =
      import.Export1.Item.ExportGrpSupportedCsePerson.Number;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      import.Export1.Item.ExportObligationTransaction.SystemGeneratedIdentifier;
      
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.ObligationType.Assign(import.ObligationType);

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCabRaiseEvent2()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Supported.Number =
      import.Export1.Item.ExportGrpSupportedCsePerson.Number;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      import.Export1.Item.ExportGrpConcurrent.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.ObligationType.Assign(import.ObligationType);

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCreateObligationTranRln()
  {
    var useImport = new FnCreateObligationTranRln.Import();
    var useExport = new FnCreateObligationTranRln.Export();

    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      import.Export1.Item.ExportObligationTransaction.SystemGeneratedIdentifier;
      
    useImport.ConcurrentObligationTransaction.SystemGeneratedIdentifier =
      import.Export1.Item.ExportGrpConcurrent.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.OtrrConcurrentObligatio.SystemGeneratedIdentifier =
      import.OtrrConcurrentObligatio.SystemGeneratedIdentifier;
    useImport.CpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.CsePerson.Number = import.Obligor.Number;
    useImport.ConcurrentCsePerson.Number = import.ConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.ConcurrentObligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;

    Call(FnCreateObligationTranRln.Execute, useImport, useExport);
  }

  private void UseFnCreateObligationTransaction1()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.Supported.Number =
      import.Export1.Item.ExportGrpSupportedCsePerson.Number;
    useImport.ObligationTransaction.Amount =
      import.Export1.Item.ExportObligationTransaction.Amount;
    useImport.AccrualInstructions.DiscontinueDt =
      import.Export1.Item.ExportAccrualInstructions.DiscontinueDt;
    useImport.HardcodeObligorLap.AccountType =
      import.HardcodeObligorLap.AccountType;
    useImport.HcOtrnTDebt.Type1 = import.HcOtrnTDebt.Type1;
    MoveObligationTransaction(import.HcOtrnDtAccrualInstru,
      useImport.HcOtrnDtAccrual);
    useImport.HcDdshActiveStatus.Code = import.HcDdshActiveStatus.Code;
    MoveObligationTransaction(import.HcOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    useImport.HcCpaSupportedPerson.Type1 = import.HcCpaSupportedPerson.Type1;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.HcOtCFeesClassificati.Classification =
      import.HcOtCFeesClassificati.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      import.HcOtCRecoverClassific.Classification;
    MoveObligationTransaction(import.HcOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      import.HcOt718BUraJudgement.SystemGeneratedIdentifier;
    useImport.HcOtCVoluntaryClassif.Classification =
      import.HcOtCVoluntaryClassif.Classification;
    useImport.Max.Date = import.Max.Date;
    MoveDateWorkArea(import.Current, useImport.Current);
    useImport.HistoryIndicator.Flag = import.HistoryIndicator.Flag;
    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.Accruing.Flag = local.SetAccruing.Flag;
    useImport.Obligor.Number = import.Obligor.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    MoveObligationPaymentSchedule(import.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);
    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    MoveObligationTransaction(import.HcOtrnDtAccrualInstru, useImport.Hardcoded);
      

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    import.Export1.Update.ExportObligationTransaction.
      SystemGeneratedIdentifier =
        useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void UseFnCreateObligationTransaction2()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.Supported.Number =
      import.Export1.Item.ExportGrpSupportedCsePerson.Number;
    useImport.ObligationTransaction.Amount =
      import.Export1.Item.ExportObligationTransaction.Amount;
    useImport.AccrualInstructions.DiscontinueDt =
      import.Export1.Item.ExportAccrualInstructions.DiscontinueDt;
    useImport.HardcodeObligorLap.AccountType =
      import.HardcodeObligorLap.AccountType;
    useImport.HcOtrnTDebt.Type1 = import.HcOtrnTDebt.Type1;
    MoveObligationTransaction(import.HcOtrnDtAccrualInstru,
      useImport.HcOtrnDtAccrual);
    useImport.HcDdshActiveStatus.Code = import.HcDdshActiveStatus.Code;
    MoveObligationTransaction(import.HcOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    useImport.HcCpaSupportedPerson.Type1 = import.HcCpaSupportedPerson.Type1;
    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.HcOtCFeesClassificati.Classification =
      import.HcOtCFeesClassificati.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      import.HcOtCRecoverClassific.Classification;
    MoveObligationTransaction(import.HcOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      import.HcOt718BUraJudgement.SystemGeneratedIdentifier;
    useImport.HcOtCVoluntaryClassif.Classification =
      import.HcOtCVoluntaryClassif.Classification;
    useImport.Max.Date = import.Max.Date;
    MoveDateWorkArea(import.Current, useImport.Current);
    useImport.HistoryIndicator.Flag = import.HistoryIndicator.Flag;
    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.Accruing.Flag = local.SetAccruing.Flag;
    useImport.Obligor.Number = import.ConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    MoveObligationPaymentSchedule(import.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);
    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    MoveObligationTransaction(import.HcOtrnDtAccrualInstru, useImport.Hardcoded);
      

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    import.Export1.Update.ExportGrpConcurrent.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
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
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of ExportProgram.
      /// </summary>
      [JsonPropertyName("exportProgram")]
      public Program ExportProgram
      {
        get => exportProgram ??= new();
        set => exportProgram = value;
      }

      /// <summary>
      /// A value of ExportGrpSel.
      /// </summary>
      [JsonPropertyName("exportGrpSel")]
      public Common ExportGrpSel
      {
        get => exportGrpSel ??= new();
        set => exportGrpSel = value;
      }

      /// <summary>
      /// A value of ExportGrpSupportedCsePerson.
      /// </summary>
      [JsonPropertyName("exportGrpSupportedCsePerson")]
      public CsePerson ExportGrpSupportedCsePerson
      {
        get => exportGrpSupportedCsePerson ??= new();
        set => exportGrpSupportedCsePerson = value;
      }

      /// <summary>
      /// A value of ExportGrpSupportdPrmpt.
      /// </summary>
      [JsonPropertyName("exportGrpSupportdPrmpt")]
      public Common ExportGrpSupportdPrmpt
      {
        get => exportGrpSupportdPrmpt ??= new();
        set => exportGrpSupportdPrmpt = value;
      }

      /// <summary>
      /// A value of ExportGrpSupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("exportGrpSupportedCsePersonsWorkSet")]
      public CsePersonsWorkSet ExportGrpSupportedCsePersonsWorkSet
      {
        get => exportGrpSupportedCsePersonsWorkSet ??= new();
        set => exportGrpSupportedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ExportCase.
      /// </summary>
      [JsonPropertyName("exportCase")]
      public Case1 ExportCase
      {
        get => exportCase ??= new();
        set => exportCase = value;
      }

      /// <summary>
      /// A value of ExportGrpProratePercnt.
      /// </summary>
      [JsonPropertyName("exportGrpProratePercnt")]
      public Common ExportGrpProratePercnt
      {
        get => exportGrpProratePercnt ??= new();
        set => exportGrpProratePercnt = value;
      }

      /// <summary>
      /// A value of ExportObligationTransaction.
      /// </summary>
      [JsonPropertyName("exportObligationTransaction")]
      public ObligationTransaction ExportObligationTransaction
      {
        get => exportObligationTransaction ??= new();
        set => exportObligationTransaction = value;
      }

      /// <summary>
      /// A value of ExportHidden.
      /// </summary>
      [JsonPropertyName("exportHidden")]
      public ProgramScreenAttributes ExportHidden
      {
        get => exportHidden ??= new();
        set => exportHidden = value;
      }

      /// <summary>
      /// A value of ExportServiceProvider.
      /// </summary>
      [JsonPropertyName("exportServiceProvider")]
      public ServiceProvider ExportServiceProvider
      {
        get => exportServiceProvider ??= new();
        set => exportServiceProvider = value;
      }

      /// <summary>
      /// A value of ExportAccrualInstructions.
      /// </summary>
      [JsonPropertyName("exportAccrualInstructions")]
      public AccrualInstructions ExportAccrualInstructions
      {
        get => exportAccrualInstructions ??= new();
        set => exportAccrualInstructions = value;
      }

      /// <summary>
      /// A value of ExportGrpConcurrent.
      /// </summary>
      [JsonPropertyName("exportGrpConcurrent")]
      public ObligationTransaction ExportGrpConcurrent
      {
        get => exportGrpConcurrent ??= new();
        set => exportGrpConcurrent = value;
      }

      /// <summary>
      /// A value of ExportGrpPrev.
      /// </summary>
      [JsonPropertyName("exportGrpPrev")]
      public ObligationTransaction ExportGrpPrev
      {
        get => exportGrpPrev ??= new();
        set => exportGrpPrev = value;
      }

      /// <summary>
      /// A value of ExportGrpSuspendAcrual.
      /// </summary>
      [JsonPropertyName("exportGrpSuspendAcrual")]
      public CsePersonsWorkSet ExportGrpSuspendAcrual
      {
        get => exportGrpSuspendAcrual ??= new();
        set => exportGrpSuspendAcrual = value;
      }

      /// <summary>
      /// A value of ExportGrpHid.
      /// </summary>
      [JsonPropertyName("exportGrpHid")]
      public AccrualInstructions ExportGrpHid
      {
        get => exportGrpHid ??= new();
        set => exportGrpHid = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program exportProgram;
      private Common exportGrpSel;
      private CsePerson exportGrpSupportedCsePerson;
      private Common exportGrpSupportdPrmpt;
      private CsePersonsWorkSet exportGrpSupportedCsePersonsWorkSet;
      private Case1 exportCase;
      private Common exportGrpProratePercnt;
      private ObligationTransaction exportObligationTransaction;
      private ProgramScreenAttributes exportHidden;
      private ServiceProvider exportServiceProvider;
      private AccrualInstructions exportAccrualInstructions;
      private ObligationTransaction exportGrpConcurrent;
      private ObligationTransaction exportGrpPrev;
      private CsePersonsWorkSet exportGrpSuspendAcrual;
      private AccrualInstructions exportGrpHid;
    }

    /// <summary>
    /// A value of HardcodeObligorLap.
    /// </summary>
    [JsonPropertyName("hardcodeObligorLap")]
    public LegalActionPerson HardcodeObligorLap
    {
      get => hardcodeObligorLap ??= new();
      set => hardcodeObligorLap = value;
    }

    /// <summary>
    /// A value of OtrrConcurrentObligatio.
    /// </summary>
    [JsonPropertyName("otrrConcurrentObligatio")]
    public ObligationTransactionRlnRsn OtrrConcurrentObligatio
    {
      get => otrrConcurrentObligatio ??= new();
      set => otrrConcurrentObligatio = value;
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
    /// A value of HcOtCVoluntaryClassif.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntaryClassif")]
    public ObligationType HcOtCVoluntaryClassif
    {
      get => hcOtCVoluntaryClassif ??= new();
      set => hcOtCVoluntaryClassif = value;
    }

    /// <summary>
    /// A value of HcOt718BUraJudgement.
    /// </summary>
    [JsonPropertyName("hcOt718BUraJudgement")]
    public ObligationType HcOt718BUraJudgement
    {
      get => hcOt718BUraJudgement ??= new();
      set => hcOt718BUraJudgement = value;
    }

    /// <summary>
    /// A value of HcOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hcOtrnDtDebtDetail")]
    public ObligationTransaction HcOtrnDtDebtDetail
    {
      get => hcOtrnDtDebtDetail ??= new();
      set => hcOtrnDtDebtDetail = value;
    }

    /// <summary>
    /// A value of HcOtCRecoverClassific.
    /// </summary>
    [JsonPropertyName("hcOtCRecoverClassific")]
    public ObligationType HcOtCRecoverClassific
    {
      get => hcOtCRecoverClassific ??= new();
      set => hcOtCRecoverClassific = value;
    }

    /// <summary>
    /// A value of HcOtCFeesClassificati.
    /// </summary>
    [JsonPropertyName("hcOtCFeesClassificati")]
    public ObligationType HcOtCFeesClassificati
    {
      get => hcOtCFeesClassificati ??= new();
      set => hcOtCFeesClassificati = value;
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
    /// A value of HcCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hcCpaSupportedPerson")]
    public CsePersonAccount HcCpaSupportedPerson
    {
      get => hcCpaSupportedPerson ??= new();
      set => hcCpaSupportedPerson = value;
    }

    /// <summary>
    /// A value of HcOtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtrnDtVoluntary")]
    public ObligationTransaction HcOtrnDtVoluntary
    {
      get => hcOtrnDtVoluntary ??= new();
      set => hcOtrnDtVoluntary = value;
    }

    /// <summary>
    /// A value of HcDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hcDdshActiveStatus")]
    public DebtDetailStatusHistory HcDdshActiveStatus
    {
      get => hcDdshActiveStatus ??= new();
      set => hcDdshActiveStatus = value;
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
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HcOtrnDtAccrualInstru.
    /// </summary>
    [JsonPropertyName("hcOtrnDtAccrualInstru")]
    public ObligationTransaction HcOtrnDtAccrualInstru
    {
      get => hcOtrnDtAccrualInstru ??= new();
      set => hcOtrnDtAccrualInstru = value;
    }

    /// <summary>
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public WorkArea Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private LegalActionPerson hardcodeObligorLap;
    private ObligationTransactionRlnRsn otrrConcurrentObligatio;
    private DateWorkArea max;
    private ObligationType hcOtCVoluntaryClassif;
    private ObligationType hcOt718BUraJudgement;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private ObligationType hcOtCRecoverClassific;
    private ObligationType hcOtCFeesClassificati;
    private CsePersonAccount hcCpaObligor;
    private CsePersonAccount hcCpaSupportedPerson;
    private ObligationTransaction hcOtrnDtVoluntary;
    private DebtDetailStatusHistory hcDdshActiveStatus;
    private DateWorkArea current;
    private ObligationTransaction hcOtrnTDebt;
    private ObligationTransaction hcOtrnDtAccrualInstru;
    private Common historyIndicator;
    private Infrastructure infrastructure;
    private LegalActionDetail legalActionDetail;
    private WorkArea program;
    private CsePerson obligor;
    private CsePerson concurrentCsePerson;
    private Obligation obligation;
    private ObligationType obligationType;
    private Obligation concurrentObligation;
    private LegalAction legalAction;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Array<ExportGroup> export1;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public WorkArea Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of SetAccruing.
    /// </summary>
    [JsonPropertyName("setAccruing")]
    public Common SetAccruing
    {
      get => setAccruing ??= new();
      set => setAccruing = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      program = null;
    }

    private WorkArea program;
    private Infrastructure infrastructure;
    private Common setAccruing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Obligation Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
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
    /// A value of ConcurrentObligor.
    /// </summary>
    [JsonPropertyName("concurrentObligor")]
    public CsePerson ConcurrentObligor
    {
      get => concurrentObligor ??= new();
      set => concurrentObligor = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    private ObligationType obligationType;
    private CsePersonAccount obligor1;
    private Obligation concurrent;
    private Obligation obligation;
    private CsePerson concurrentObligor;
    private CsePerson supported;
    private CsePerson obligor2;
  }
#endregion
}
