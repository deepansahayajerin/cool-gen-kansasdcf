// Program: FN_B651_SPLIT_DUP_DISB_TO_NET, ID: 373295288, model: 746.
// Short name: SWE02702
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_SPLIT_DUP_DISB_TO_NET.
/// </summary>
[Serializable]
public partial class FnB651SplitDupDisbToNet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_SPLIT_DUP_DISB_TO_NET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651SplitDupDisbToNet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651SplitDupDisbToNet.
  /// </summary>
  public FnB651SplitDupDisbToNet(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // 04-16-01  PR 118495  Fangman - Created code to break a disbursement into 
    // 2 pieces so that part of it can be released to net with negative
    // disbursements and the other part can still be applied to recoveries or
    // suppressed for a reason of 'D'.
    // 01/31/02  WR 000235  Fangman - PSUM redesign.  Added code to keep track 
    // of the type of monthly totals created in the AB.
    // ****************************************************************
    UseFnB651ApplySuppressionRules();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ForCreateDisbursementTransaction.Type1 = "D";
    local.ForCreateDisbursementTransaction.Amount =
      import.AmtOfDupToSplitToNet.TotalCurrency;
    local.ForCreateDisbursementTransaction.DisbursementDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.ForCreateDisbursementTransaction.ProcessDate = local.Initialized.Date;
    local.ForCreateDisbursementTransaction.CashNonCashInd =
      import.PerCashReceiptType.CategoryIndicator;

    if (Lt(local.Initialized.Date, local.HighestSuppressionDate.Date))
    {
      local.Determined.SystemGeneratedIdentifier =
        import.Per3Suppressed.SystemGeneratedIdentifier;
    }
    else
    {
      local.Determined.SystemGeneratedIdentifier =
        import.Per1Released.SystemGeneratedIdentifier;
    }

    UseFnCreateDisbursementNew();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Lt(local.Initialized.Date, local.HighestSuppressionDate.Date))
      {
        export.ImpExpToUpdate.DisbursementsSuppressed =
          export.ImpExpToUpdate.DisbursementsSuppressed.GetValueOrDefault();
      }
      else if (AsChar(import.PerDisbursementTransaction.ExcessUraInd) == 'Y')
      {
        export.ImpExpToUpdate.TotExcessUraAmt =
          export.ImpExpToUpdate.TotExcessUraAmt.GetValueOrDefault();
      }
      else if (ReadDisbursementType())
      {
        if (Equal(entities.DisbursementType.ProgramCode, "AF"))
        {
          export.ImpExpToUpdate.AdcReimbursedAmount =
            export.ImpExpToUpdate.AdcReimbursedAmount.GetValueOrDefault();
        }
        else
        {
          export.ImpExpToUpdate.CollectionsDisbursedToAr =
            export.ImpExpToUpdate.CollectionsDisbursedToAr.GetValueOrDefault();
        }
      }
      else
      {
        ExitState = "FN0000_DISB_TYPE_NF";
      }

      import.ExpAmtRemainingToDisburs.Amount -= import.AmtOfDupToSplitToNet.
        TotalCurrency;

      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.UnformattedAmt.Number112 =
          import.AmtOfDupToSplitToNet.TotalCurrency;
        UseCabFormat112AmtFieldTo8();
        local.EabReportSend.RptDetail =
          "  D Suppr:  Amt of split disb to release to net " + local
          .FormattedAmt.Text9;
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
    }
  }

  private static void MoveApEvent1(FnB651ApplySuppressionRules.Export.
    ApEventGroup source, Export.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveApEvent2(Export.ApEventGroup source,
    FnB651ApplySuppressionRules.Export.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormat112AmtFieldTo8()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.UnformattedAmt.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseFnB651ApplySuppressionRules()
  {
    var useImport = new FnB651ApplySuppressionRules.Import();
    var useExport = new FnB651ApplySuppressionRules.Export();

    useImport.PerObligeeCsePerson.Assign(import.PerObligeeCsePerson);
    useImport.PerObligeeCsePersonAccount.Assign(
      import.PerObligeeCsePersonAccount);
    useImport.PerDisbursementTransaction.Assign(
      import.PerDisbursementTransaction);
    useImport.PerCollection.Assign(import.PerCollection);
    useImport.PerCollectionType.Assign(import.PerCollectionType);
    useImport.PerCashReceiptType.Assign(import.PerCashReceiptType);
    useImport.PerObligor.Assign(import.PerObligor);
    useImport.PerObligationType.Assign(import.PerObligationType);
    useImport.PerDebtDetail.Assign(import.PerDebtDetail);
    useImport.PerSupported.Assign(import.PerSupported);
    useImport.Per3Suppressed.Assign(import.Per3Suppressed);
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.UraSuppressionLength.LastUsedNumber =
      import.UraSuppressionLength.LastUsedNumber;
    useImport.ExpDatabaseUpdated.Flag = import.ExpDatabaseUpdated.Flag;
    useImport.TestDisplayInd.Flag = import.TestDisplayInd.Flag;
    export.ApEvent.CopyTo(useExport.ApEvent, MoveApEvent2);

    Call(FnB651ApplySuppressionRules.Execute, useImport, useExport);

    import.PerObligeeCsePerson.Assign(useImport.PerObligeeCsePerson);
    import.PerObligeeCsePersonAccount.Assign(
      useImport.PerObligeeCsePersonAccount);
    import.PerDisbursementTransaction.Assign(
      useImport.PerDisbursementTransaction);
    import.PerCollection.Assign(useImport.PerCollection);
    import.PerCollectionType.Assign(useImport.PerCollectionType);
    import.PerCashReceiptType.Assign(useImport.PerCashReceiptType);
    import.PerObligor.Assign(useImport.PerObligor);
    import.PerObligationType.Assign(useImport.PerObligationType);
    import.PerDebtDetail.Assign(useImport.PerDebtDetail);
    import.PerSupported.Assign(useImport.PerSupported);
    import.Per3Suppressed.Assign(useImport.Per3Suppressed);
    import.ExpDatabaseUpdated.Flag = useImport.ExpDatabaseUpdated.Flag;
    local.HighestSuppressionDate.Date = useExport.HighestSuppressionDate.Date;
    local.ForCreateDisbursementStatusHistory.SuppressionReason =
      useExport.ForCreate.SuppressionReason;
    useExport.ApEvent.CopyTo(export.ApEvent, MoveApEvent1);
  }

  private void UseFnCreateDisbursementNew()
  {
    var useImport = new FnCreateDisbursementNew.Import();
    var useExport = new FnCreateDisbursementNew.Export();

    useImport.PerObligee.Assign(import.PerObligeeCsePersonAccount);
    useImport.PerCredit.Assign(import.PerDisbursementTransaction);
    useImport.DisbursementType.SystemGeneratedIdentifier =
      import.DisbursementType.SystemGeneratedIdentifier;
    useImport.Per1AfCcs.Assign(import.Per1AfCcs);
    useImport.Per2AfAcs.Assign(import.Per2AfAcs);
    useImport.Per4NaCcs.Assign(import.Per4NaCcs);
    useImport.Per5NaAcs.Assign(import.Per5NaAcs);
    useImport.Per73CrFee.Assign(import.Per73CrFee);
    useImport.Per1.Assign(import.Per1);
    useImport.Per1Released.Assign(import.Per1Released);
    useImport.Per3Suppressed.Assign(import.Per3Suppressed);
    useImport.ExpDatabaseUpdated.Flag = import.ExpDatabaseUpdated.Flag;
    useImport.Max.Date = import.Max.Date;
    useImport.TestDisplayInd.Flag = import.TestDisplayInd.Flag;
    useImport.HighestSuppressionDate.Date = local.HighestSuppressionDate.Date;
    useImport.DisbursementStatus.SystemGeneratedIdentifier =
      local.Determined.SystemGeneratedIdentifier;
    useImport.ForCreate.SuppressionReason =
      local.ForCreateDisbursementStatusHistory.SuppressionReason;
    useImport.New1.Assign(local.ForCreateDisbursementTransaction);

    Call(FnCreateDisbursementNew.Execute, useImport, useExport);

    import.PerObligeeCsePersonAccount.Assign(useImport.PerObligee);
    import.PerDisbursementTransaction.Assign(useImport.PerCredit);
    import.Per1AfCcs.Assign(useImport.Per1AfCcs);
    import.Per2AfAcs.Assign(useImport.Per2AfAcs);
    import.Per4NaCcs.Assign(useImport.Per4NaCcs);
    import.Per5NaAcs.Assign(useImport.Per5NaAcs);
    import.Per73CrFee.Assign(useImport.Per73CrFee);
    import.Per1.Assign(useImport.Per1);
    import.Per1Released.Assign(useImport.Per1Released);
    import.Per3Suppressed.Assign(useImport.Per3Suppressed);
    import.ExpDatabaseUpdated.Flag = useImport.ExpDatabaseUpdated.Flag;
  }

  private bool ReadDisbursementType()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          import.DisbursementType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
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
    /// A value of AmtOfDupToSplitToNet.
    /// </summary>
    [JsonPropertyName("amtOfDupToSplitToNet")]
    public Common AmtOfDupToSplitToNet
    {
      get => amtOfDupToSplitToNet ??= new();
      set => amtOfDupToSplitToNet = value;
    }

    /// <summary>
    /// A value of ExpAmtRemainingToDisburs.
    /// </summary>
    [JsonPropertyName("expAmtRemainingToDisburs")]
    public DisbursementTransaction ExpAmtRemainingToDisburs
    {
      get => expAmtRemainingToDisburs ??= new();
      set => expAmtRemainingToDisburs = value;
    }

    /// <summary>
    /// A value of PerObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("perObligeeCsePerson")]
    public CsePerson PerObligeeCsePerson
    {
      get => perObligeeCsePerson ??= new();
      set => perObligeeCsePerson = value;
    }

    /// <summary>
    /// A value of PerObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("perObligeeCsePersonAccount")]
    public CsePersonAccount PerObligeeCsePersonAccount
    {
      get => perObligeeCsePersonAccount ??= new();
      set => perObligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PerDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("perDisbursementTransaction")]
    public DisbursementTransaction PerDisbursementTransaction
    {
      get => perDisbursementTransaction ??= new();
      set => perDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PerCollection.
    /// </summary>
    [JsonPropertyName("perCollection")]
    public Collection PerCollection
    {
      get => perCollection ??= new();
      set => perCollection = value;
    }

    /// <summary>
    /// A value of PerCollectionType.
    /// </summary>
    [JsonPropertyName("perCollectionType")]
    public CollectionType PerCollectionType
    {
      get => perCollectionType ??= new();
      set => perCollectionType = value;
    }

    /// <summary>
    /// A value of PerCashReceiptType.
    /// </summary>
    [JsonPropertyName("perCashReceiptType")]
    public CashReceiptType PerCashReceiptType
    {
      get => perCashReceiptType ??= new();
      set => perCashReceiptType = value;
    }

    /// <summary>
    /// A value of PerObligor.
    /// </summary>
    [JsonPropertyName("perObligor")]
    public CsePerson PerObligor
    {
      get => perObligor ??= new();
      set => perObligor = value;
    }

    /// <summary>
    /// A value of PerObligationType.
    /// </summary>
    [JsonPropertyName("perObligationType")]
    public ObligationType PerObligationType
    {
      get => perObligationType ??= new();
      set => perObligationType = value;
    }

    /// <summary>
    /// A value of PerDebtDetail.
    /// </summary>
    [JsonPropertyName("perDebtDetail")]
    public DebtDetail PerDebtDetail
    {
      get => perDebtDetail ??= new();
      set => perDebtDetail = value;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Per1AfCcs.
    /// </summary>
    [JsonPropertyName("per1AfCcs")]
    public DisbursementType Per1AfCcs
    {
      get => per1AfCcs ??= new();
      set => per1AfCcs = value;
    }

    /// <summary>
    /// A value of Per2AfAcs.
    /// </summary>
    [JsonPropertyName("per2AfAcs")]
    public DisbursementType Per2AfAcs
    {
      get => per2AfAcs ??= new();
      set => per2AfAcs = value;
    }

    /// <summary>
    /// A value of Per4NaCcs.
    /// </summary>
    [JsonPropertyName("per4NaCcs")]
    public DisbursementType Per4NaCcs
    {
      get => per4NaCcs ??= new();
      set => per4NaCcs = value;
    }

    /// <summary>
    /// A value of Per5NaAcs.
    /// </summary>
    [JsonPropertyName("per5NaAcs")]
    public DisbursementType Per5NaAcs
    {
      get => per5NaAcs ??= new();
      set => per5NaAcs = value;
    }

    /// <summary>
    /// A value of Per73CrFee.
    /// </summary>
    [JsonPropertyName("per73CrFee")]
    public DisbursementType Per73CrFee
    {
      get => per73CrFee ??= new();
      set => per73CrFee = value;
    }

    /// <summary>
    /// A value of Per1.
    /// </summary>
    [JsonPropertyName("per1")]
    public DisbursementTranRlnRsn Per1
    {
      get => per1 ??= new();
      set => per1 = value;
    }

    /// <summary>
    /// A value of Per1Released.
    /// </summary>
    [JsonPropertyName("per1Released")]
    public DisbursementStatus Per1Released
    {
      get => per1Released ??= new();
      set => per1Released = value;
    }

    /// <summary>
    /// A value of Per3Suppressed.
    /// </summary>
    [JsonPropertyName("per3Suppressed")]
    public DisbursementStatus Per3Suppressed
    {
      get => per3Suppressed ??= new();
      set => per3Suppressed = value;
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
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
    }

    /// <summary>
    /// A value of ExpDatabaseUpdated.
    /// </summary>
    [JsonPropertyName("expDatabaseUpdated")]
    public Common ExpDatabaseUpdated
    {
      get => expDatabaseUpdated ??= new();
      set => expDatabaseUpdated = value;
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
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private Common amtOfDupToSplitToNet;
    private DisbursementTransaction expAmtRemainingToDisburs;
    private CsePerson perObligeeCsePerson;
    private CsePersonAccount perObligeeCsePersonAccount;
    private DisbursementTransaction perDisbursementTransaction;
    private Collection perCollection;
    private CollectionType perCollectionType;
    private CashReceiptType perCashReceiptType;
    private CsePerson perObligor;
    private ObligationType perObligationType;
    private DebtDetail perDebtDetail;
    private CsePerson perSupported;
    private DisbursementType disbursementType;
    private DisbursementType per1AfCcs;
    private DisbursementType per2AfAcs;
    private DisbursementType per4NaCcs;
    private DisbursementType per5NaAcs;
    private DisbursementType per73CrFee;
    private DisbursementTranRlnRsn per1;
    private DisbursementStatus per1Released;
    private DisbursementStatus per3Suppressed;
    private ProgramProcessingInfo programProcessingInfo;
    private ControlTable uraSuppressionLength;
    private Common expDatabaseUpdated;
    private DateWorkArea max;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ApEventGroup group.</summary>
    [Serializable]
    public class ApEventGroup
    {
      /// <summary>
      /// A value of ApGrpDtl.
      /// </summary>
      [JsonPropertyName("apGrpDtl")]
      public CsePerson ApGrpDtl
      {
        get => apGrpDtl ??= new();
        set => apGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraGrpDtl")]
      public Common RegUraGrpDtl
      {
        get => regUraGrpDtl ??= new();
        set => regUraGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraAdjGrpDtl")]
      public Common RegUraAdjGrpDtl
      {
        get => regUraAdjGrpDtl ??= new();
        set => regUraAdjGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraGrpDtl")]
      public Common MedUraGrpDtl
      {
        get => medUraGrpDtl ??= new();
        set => medUraGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraAdjGrpDtl")]
      public Common MedUraAdjGrpDtl
      {
        get => medUraAdjGrpDtl ??= new();
        set => medUraAdjGrpDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson apGrpDtl;
      private Common regUraGrpDtl;
      private Common regUraAdjGrpDtl;
      private Common medUraGrpDtl;
      private Common medUraAdjGrpDtl;
    }

    /// <summary>
    /// A value of ImpExpToUpdate.
    /// </summary>
    [JsonPropertyName("impExpToUpdate")]
    public MonthlyObligeeSummary ImpExpToUpdate
    {
      get => impExpToUpdate ??= new();
      set => impExpToUpdate = value;
    }

    /// <summary>
    /// Gets a value of ApEvent.
    /// </summary>
    [JsonIgnore]
    public Array<ApEventGroup> ApEvent => apEvent ??= new(
      ApEventGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApEvent for json serialization.
    /// </summary>
    [JsonPropertyName("apEvent")]
    [Computed]
    public IList<ApEventGroup> ApEvent_Json
    {
      get => apEvent;
      set => ApEvent.Assign(value);
    }

    private MonthlyObligeeSummary impExpToUpdate;
    private Array<ApEventGroup> apEvent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HighestSuppressionDate.
    /// </summary>
    [JsonPropertyName("highestSuppressionDate")]
    public DateWorkArea HighestSuppressionDate
    {
      get => highestSuppressionDate ??= new();
      set => highestSuppressionDate = value;
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

    /// <summary>
    /// A value of Determined.
    /// </summary>
    [JsonPropertyName("determined")]
    public DisbursementStatus Determined
    {
      get => determined ??= new();
      set => determined = value;
    }

    /// <summary>
    /// A value of ForCreateDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("forCreateDisbursementStatusHistory")]
    public DisbursementStatusHistory ForCreateDisbursementStatusHistory
    {
      get => forCreateDisbursementStatusHistory ??= new();
      set => forCreateDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of ForCreateDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("forCreateDisbursementTransaction")]
    public DisbursementTransaction ForCreateDisbursementTransaction
    {
      get => forCreateDisbursementTransaction ??= new();
      set => forCreateDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of UnformattedAmt.
    /// </summary>
    [JsonPropertyName("unformattedAmt")]
    public NumericWorkSet UnformattedAmt
    {
      get => unformattedAmt ??= new();
      set => unformattedAmt = value;
    }

    /// <summary>
    /// A value of FormattedAmt.
    /// </summary>
    [JsonPropertyName("formattedAmt")]
    public WorkArea FormattedAmt
    {
      get => formattedAmt ??= new();
      set => formattedAmt = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private DateWorkArea highestSuppressionDate;
    private DateWorkArea initialized;
    private DisbursementStatus determined;
    private DisbursementStatusHistory forCreateDisbursementStatusHistory;
    private DisbursementTransaction forCreateDisbursementTransaction;
    private NumericWorkSet unformattedAmt;
    private WorkArea formattedAmt;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType disbursementType;
  }
#endregion
}
