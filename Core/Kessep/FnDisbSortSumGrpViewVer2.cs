// Program: FN_DISB_SORT_SUM_GRP_VIEW_VER2, ID: 372803867, model: 746.
// Short name: SWE02801
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
/// A program: FN_DISB_SORT_SUM_GRP_VIEW_VER2.
/// </para>
/// <para>
/// This action block will sort the import group view on Disb_date(A),Reference 
/// number(A) and Disb_Type(A) using buble sort algorithm.
/// </para>
/// </summary>
[Serializable]
public partial class FnDisbSortSumGrpViewVer2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISB_SORT_SUM_GRP_VIEW_VER2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisbSortSumGrpViewVer2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisbSortSumGrpViewVer2.
  /// </summary>
  public FnDisbSortSumGrpViewVer2(IContext context, Import import, Export export)
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
    // ***********************************************
    // Developed By : R.B.Mohapatra
    //                MTW Consulting
    // 07/16/99   SWSRKXD Phase 2 changes
    //            : Add pay_req.# to the summation criteria.
    // 6/3/2000 PRWORA Work Order # 164
    // - Flag URA disbursements with an 'X' in the Disb Type field.
    // - Rename views to make them standard.
    // 4/11/2001 SWSRKXD PR# 117667
    // - Inflate GVs.
    // - Add checks for GV overflow to avoid an abend.
    // ***********************************************
    // **** Move IMPORT Gr. View to LOCAL Gr. View ****
    local.Group.Index = -1;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (Equal(import.Group.Item.DisbDate.DisbDate, null) && IsEmpty
        (import.Group.Item.DetDisbursementTransactionType.Code) && IsEmpty
        (import.Group.Item.DisbType.Text10) && IsEmpty
        (import.Group.Item.DetDisbursementTransaction.ReferenceNumber))
      {
        continue;
      }

      if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
      {
        ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

        return;
      }

      ++local.Group.Index;
      local.Group.CheckSize();

      local.Group.Update.GrDisbDate.DisbDate =
        import.Group.Item.DisbDate.DisbDate;
      local.Group.Update.GrCreditDet.Amount =
        import.Group.Item.CreditDet.Amount;
      local.Group.Update.GrDebitDet.Amount = import.Group.Item.DebitDet.Amount;
      local.Group.Update.GrDetDisbursementTransactionType.Code =
        import.Group.Item.DetDisbursementTransactionType.Code;
      local.Group.Update.DisbType.Text10 = import.Group.Item.DisbType.Text10;
      local.Group.Update.GrDetPaymentRequest.Assign(
        import.Group.Item.DetPaymentRequest);
      MoveDisbursementTransaction(import.Group.Item.DetDisbursementTransaction,
        local.Group.Update.GrDetDisbursementTransaction);
    }

    local.ChangedFlag.Flag = "T";

    while(AsChar(local.ChangedFlag.Flag) == 'T')
    {
      local.ChangedFlag.Flag = "F";

      local.Group.Index = 0;
      local.Group.CheckSize();

      while(local.Group.Index + 1 < local.Group.Count)
      {
        local.SwapFlag.Flag = "N";

        // **** MOVE the (i)th element to Local-Temp1 ****
        local.Temp1LocalFinanceWorkArea.DisbDate =
          local.Group.Item.GrDisbDate.DisbDate;
        local.Temp1DisbursementTransactionType.Code =
          local.Group.Item.GrDetDisbursementTransactionType.Code;
        local.Temp1DisbType.Text10 = local.Group.Item.DisbType.Text10;
        local.Temp1PaymentRequest.Assign(local.Group.Item.GrDetPaymentRequest);
        local.Temp1Credit.Amount = local.Group.Item.GrCreditDet.Amount;
        local.Temp1PositiveDebit.Amount = local.Group.Item.GrDebitDet.Amount;
        MoveDisbursementTransaction(local.Group.Item.
          GrDetDisbursementTransaction, local.Temp1DisbursementTransaction);

        // **** Move the (i+1)th element to Local-Temp2 ****
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Temp2LocalFinanceWorkArea.DisbDate =
          local.Group.Item.GrDisbDate.DisbDate;
        local.Temp2DisbursementTransactionType.Code =
          local.Group.Item.GrDetDisbursementTransactionType.Code;
        local.Temp2DisbType.Text10 = local.Group.Item.DisbType.Text10;
        local.Temp2PaymentRequest.Assign(local.Group.Item.GrDetPaymentRequest);
        local.Temp2Credit.Amount = local.Group.Item.GrCreditDet.Amount;
        local.Temp2Debit.Amount = local.Group.Item.GrDebitDet.Amount;
        MoveDisbursementTransaction(local.Group.Item.
          GrDetDisbursementTransaction, local.Temp2DisbursementTransaction);

        // **** COMPARE and SWAP (if necessary) the (i)th & (i+1)th elements ***
        // *
        if (Lt(local.Temp2LocalFinanceWorkArea.DisbDate,
          local.Temp1LocalFinanceWorkArea.DisbDate))
        {
          local.SwapFlag.Flag = "Y";
        }
        else if (Equal(local.Temp2LocalFinanceWorkArea.DisbDate,
          local.Temp1LocalFinanceWorkArea.DisbDate) && Lt
          (local.Temp2DisbursementTransaction.ReferenceNumber,
          local.Temp1DisbursementTransaction.ReferenceNumber))
        {
          local.SwapFlag.Flag = "Y";
        }
        else if (Equal(local.Temp2LocalFinanceWorkArea.DisbDate,
          local.Temp1LocalFinanceWorkArea.DisbDate) && Equal
          (local.Temp2DisbursementTransaction.ReferenceNumber,
          local.Temp1DisbursementTransaction.ReferenceNumber) && Lt
          (local.Temp2DisbType.Text10, local.Temp1DisbType.Text10))
        {
          local.SwapFlag.Flag = "Y";

          // --------------------------------------------------------
          // SWSRKXD - 7/16/99
          // Add payment_request.number to sort criteria
          // -------------------------------------------------------
        }
        else if (Equal(local.Temp2LocalFinanceWorkArea.DisbDate,
          local.Temp1LocalFinanceWorkArea.DisbDate) && Equal
          (local.Temp2DisbursementTransaction.ReferenceNumber,
          local.Temp1DisbursementTransaction.ReferenceNumber) && Equal
          (local.Temp2DisbType.Text10, local.Temp1DisbType.Text10) && Lt
          (local.Temp2PaymentRequest.Number, local.Temp1PaymentRequest.Number))
        {
          local.SwapFlag.Flag = "Y";
        }

        if (AsChar(local.SwapFlag.Flag) == 'Y')
        {
          --local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.GrDisbDate.DisbDate =
            local.Temp2LocalFinanceWorkArea.DisbDate;
          local.Group.Update.GrDetDisbursementTransactionType.Code =
            local.Temp2DisbursementTransactionType.Code;
          local.Group.Update.DisbType.Text10 = local.Temp2DisbType.Text10;
          local.Group.Update.GrDetPaymentRequest.Assign(
            local.Temp2PaymentRequest);
          local.Group.Update.GrDebitDet.Amount = local.Temp2Debit.Amount;
          local.Group.Update.GrCreditDet.Amount = local.Temp2Credit.Amount;
          MoveDisbursementTransaction(local.Temp2DisbursementTransaction,
            local.Group.Update.GrDetDisbursementTransaction);

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.GrDisbDate.DisbDate =
            local.Temp1LocalFinanceWorkArea.DisbDate;
          local.Group.Update.GrDetDisbursementTransactionType.Code =
            local.Temp1DisbursementTransactionType.Code;
          local.Group.Update.DisbType.Text10 = local.Temp1DisbType.Text10;
          local.Group.Update.GrDetPaymentRequest.Assign(
            local.Temp1PaymentRequest);
          local.Group.Update.GrDebitDet.Amount =
            local.Temp1PositiveDebit.Amount;
          local.Group.Update.GrCreditDet.Amount = local.Temp1Credit.Amount;
          MoveDisbursementTransaction(local.Temp1DisbursementTransaction,
            local.Group.Update.GrDetDisbursementTransaction);
          local.ChangedFlag.Flag = "T";
        }
      }
    }

    // *** SUMMING-UP THE SORTED DETAIL LINES AS PER THE CRITERIA ***
    // ==> Initialize here the local-temp1 variables as we are going to use them
    // in the Summing Process
    local.Temp1DisbursementTransactionType.Code = "";
    local.Temp1DisbType.Text10 = "";
    local.Temp1LocalFinanceWorkArea.DisbDate = null;
    local.Temp1PaymentRequest.Type1 = "";
    local.Temp1PaymentRequest.Number = "";
    local.Temp1PaymentRequest.SystemGeneratedIdentifier = 0;
    local.Temp1Credit.Amount = 0;
    local.Temp1PositiveDebit.Amount = 0;
    local.Temp1NegativeDb.Amount = 0;
    local.Temp1DisbursementTransaction.SystemGeneratedIdentifier = 0;
    local.Temp1DisbursementTransaction.ReferenceNumber =
      Spaces(DisbursementTransaction.ReferenceNumber_MaxLength);
    export.Group.Index = -1;
    local.Firsttime.Flag = "Y";

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      // --------------------------------------------------------
      // SWSRKXD - 7/16/99
      // Add payment_request.number to the group criteria
      // -------------------------------------------------------
      if (Equal(local.Group.Item.GrDisbDate.DisbDate,
        local.Temp1LocalFinanceWorkArea.DisbDate) && Equal
        (local.Group.Item.GrDetDisbursementTransaction.ReferenceNumber,
        local.Temp1DisbursementTransaction.ReferenceNumber) && Equal
        (local.Group.Item.DisbType.Text10, local.Temp1DisbType.Text10) && Equal
        (local.Group.Item.GrDetPaymentRequest.Number,
        local.Temp1PaymentRequest.Number))
      {
        if (local.Group.Item.GrDebitDet.Amount < 0)
        {
          local.Temp1NegativeDb.Amount += local.Group.Item.GrDebitDet.Amount;
        }
        else
        {
          local.Temp1PositiveDebit.Amount += local.Group.Item.GrDebitDet.Amount;
        }
      }
      else
      {
        if (AsChar(local.Firsttime.Flag) != 'Y')
        {
          // **** MOVE LOCAL_TEMP1 TO EXPORT ****
          if (local.Temp1NegativeDb.Amount < 0)
          {
            if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
            {
              ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

              return;
            }

            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.DisbDate.DisbDate =
              local.Temp1LocalFinanceWorkArea.DisbDate;
            export.Group.Update.DetDisbursementTransactionType.Code =
              local.Temp1DisbursementTransactionType.Code;
            export.Group.Update.DisbType.Text10 = local.Temp1DisbType.Text10;
            export.Group.Update.DetPaymentRequest.Assign(
              local.Temp1PaymentRequest);
            export.Group.Update.CreditDet.Amount = local.Temp1Credit.Amount;
            MoveDisbursementTransaction(local.Temp1DisbursementTransaction,
              export.Group.Update.DetDisbursementTransaction);
            export.Group.Update.DebitDet.Amount = local.Temp1NegativeDb.Amount;

            // **** Reset amount to 0 ****
            local.Temp1NegativeDb.Amount = 0;
          }

          if (local.Temp1PositiveDebit.Amount > 0)
          {
            if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
            {
              ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

              return;
            }

            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.DisbDate.DisbDate =
              local.Temp1LocalFinanceWorkArea.DisbDate;
            export.Group.Update.DetDisbursementTransactionType.Code =
              local.Temp1DisbursementTransactionType.Code;
            export.Group.Update.DisbType.Text10 = local.Temp1DisbType.Text10;
            export.Group.Update.DetPaymentRequest.Assign(
              local.Temp1PaymentRequest);
            export.Group.Update.CreditDet.Amount = local.Temp1Credit.Amount;
            MoveDisbursementTransaction(local.Temp1DisbursementTransaction,
              export.Group.Update.DetDisbursementTransaction);
            export.Group.Update.DebitDet.Amount =
              local.Temp1PositiveDebit.Amount;

            // **** Reset amount to 0 ****
            local.Temp1PositiveDebit.Amount = 0;
          }
        }

        // **** INITIALIZE LOCAL_TEMP1 WITH CURRENT LOCAL ****
        local.Temp1LocalFinanceWorkArea.DisbDate =
          local.Group.Item.GrDisbDate.DisbDate;
        local.Temp1DisbursementTransactionType.Code =
          local.Group.Item.GrDetDisbursementTransactionType.Code;
        local.Temp1DisbType.Text10 = local.Group.Item.DisbType.Text10;
        local.Temp1PaymentRequest.Assign(local.Group.Item.GrDetPaymentRequest);
        local.Temp1Credit.Amount = local.Group.Item.GrCreditDet.Amount;

        if (local.Group.Item.GrDebitDet.Amount < 0)
        {
          local.Temp1NegativeDb.Amount = local.Group.Item.GrDebitDet.Amount;
        }
        else
        {
          local.Temp1PositiveDebit.Amount = local.Group.Item.GrDebitDet.Amount;
        }

        MoveDisbursementTransaction(local.Group.Item.
          GrDetDisbursementTransaction, local.Temp1DisbursementTransaction);
        local.Firsttime.Flag = "N";
      }
    }

    local.Group.CheckIndex();

    if (local.Temp1NegativeDb.Amount < 0)
    {
      if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
      {
        ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

        return;
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.DebitDet.Amount = local.Temp1NegativeDb.Amount;
      export.Group.Update.DisbDate.DisbDate =
        local.Temp1LocalFinanceWorkArea.DisbDate;
      export.Group.Update.DetDisbursementTransactionType.Code =
        local.Temp1DisbursementTransactionType.Code;
      export.Group.Update.DisbType.Text10 = local.Temp1DisbType.Text10;
      export.Group.Update.DetPaymentRequest.Assign(local.Temp1PaymentRequest);
      export.Group.Update.CreditDet.Amount = local.Temp1Credit.Amount;
      MoveDisbursementTransaction(local.Temp1DisbursementTransaction,
        export.Group.Update.DetDisbursementTransaction);
    }

    if (local.Temp1PositiveDebit.Amount > 0)
    {
      if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
      {
        ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

        return;
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.DebitDet.Amount = local.Temp1PositiveDebit.Amount;
      export.Group.Update.DisbDate.DisbDate =
        local.Temp1LocalFinanceWorkArea.DisbDate;
      export.Group.Update.DetDisbursementTransactionType.Code =
        local.Temp1DisbursementTransactionType.Code;
      export.Group.Update.DisbType.Text10 = local.Temp1DisbType.Text10;
      export.Group.Update.DetPaymentRequest.Assign(local.Temp1PaymentRequest);
      export.Group.Update.CreditDet.Amount = local.Temp1Credit.Amount;
      MoveDisbursementTransaction(local.Temp1DisbursementTransaction,
        export.Group.Update.DetDisbursementTransaction);
    }
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DetCashReceipt.
      /// </summary>
      [JsonPropertyName("detCashReceipt")]
      public CashReceipt DetCashReceipt
      {
        get => detCashReceipt ??= new();
        set => detCashReceipt = value;
      }

      /// <summary>
      /// A value of DetCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("detCrdCrComboNo")]
      public CrdCrComboNo DetCrdCrComboNo
      {
        get => detCrdCrComboNo ??= new();
        set => detCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of DisbDate.
      /// </summary>
      [JsonPropertyName("disbDate")]
      public LocalFinanceWorkArea DisbDate
      {
        get => disbDate ??= new();
        set => disbDate = value;
      }

      /// <summary>
      /// A value of DetDisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("detDisbursementTransactionType")]
      public DisbursementTransactionType DetDisbursementTransactionType
      {
        get => detDisbursementTransactionType ??= new();
        set => detDisbursementTransactionType = value;
      }

      /// <summary>
      /// A value of DetDisbursementType.
      /// </summary>
      [JsonPropertyName("detDisbursementType")]
      public DisbursementType DetDisbursementType
      {
        get => detDisbursementType ??= new();
        set => detDisbursementType = value;
      }

      /// <summary>
      /// A value of DetPaymentRequest.
      /// </summary>
      [JsonPropertyName("detPaymentRequest")]
      public PaymentRequest DetPaymentRequest
      {
        get => detPaymentRequest ??= new();
        set => detPaymentRequest = value;
      }

      /// <summary>
      /// A value of CreditDet.
      /// </summary>
      [JsonPropertyName("creditDet")]
      public DisbursementTransaction CreditDet
      {
        get => creditDet ??= new();
        set => creditDet = value;
      }

      /// <summary>
      /// A value of DebitDet.
      /// </summary>
      [JsonPropertyName("debitDet")]
      public DisbursementTransaction DebitDet
      {
        get => debitDet ??= new();
        set => debitDet = value;
      }

      /// <summary>
      /// A value of DetDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("detDisbursementTransaction")]
      public DisbursementTransaction DetDisbursementTransaction
      {
        get => detDisbursementTransaction ??= new();
        set => detDisbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private TextWorkArea disbType;
      private CashReceipt detCashReceipt;
      private CrdCrComboNo detCrdCrComboNo;
      private LocalFinanceWorkArea disbDate;
      private DisbursementTransactionType detDisbursementTransactionType;
      private DisbursementType detDisbursementType;
      private PaymentRequest detPaymentRequest;
      private DisbursementTransaction creditDet;
      private DisbursementTransaction debitDet;
      private DisbursementTransaction detDisbursementTransaction;
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

    private Array<GroupGroup> group;
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
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DetCashReceipt.
      /// </summary>
      [JsonPropertyName("detCashReceipt")]
      public CashReceipt DetCashReceipt
      {
        get => detCashReceipt ??= new();
        set => detCashReceipt = value;
      }

      /// <summary>
      /// A value of DetCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("detCrdCrComboNo")]
      public CrdCrComboNo DetCrdCrComboNo
      {
        get => detCrdCrComboNo ??= new();
        set => detCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of DisbDate.
      /// </summary>
      [JsonPropertyName("disbDate")]
      public LocalFinanceWorkArea DisbDate
      {
        get => disbDate ??= new();
        set => disbDate = value;
      }

      /// <summary>
      /// A value of DetDisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("detDisbursementTransactionType")]
      public DisbursementTransactionType DetDisbursementTransactionType
      {
        get => detDisbursementTransactionType ??= new();
        set => detDisbursementTransactionType = value;
      }

      /// <summary>
      /// A value of DetDisbursementType.
      /// </summary>
      [JsonPropertyName("detDisbursementType")]
      public DisbursementType DetDisbursementType
      {
        get => detDisbursementType ??= new();
        set => detDisbursementType = value;
      }

      /// <summary>
      /// A value of DetPaymentRequest.
      /// </summary>
      [JsonPropertyName("detPaymentRequest")]
      public PaymentRequest DetPaymentRequest
      {
        get => detPaymentRequest ??= new();
        set => detPaymentRequest = value;
      }

      /// <summary>
      /// A value of CreditDet.
      /// </summary>
      [JsonPropertyName("creditDet")]
      public DisbursementTransaction CreditDet
      {
        get => creditDet ??= new();
        set => creditDet = value;
      }

      /// <summary>
      /// A value of DebitDet.
      /// </summary>
      [JsonPropertyName("debitDet")]
      public DisbursementTransaction DebitDet
      {
        get => debitDet ??= new();
        set => debitDet = value;
      }

      /// <summary>
      /// A value of DetDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("detDisbursementTransaction")]
      public DisbursementTransaction DetDisbursementTransaction
      {
        get => detDisbursementTransaction ??= new();
        set => detDisbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private TextWorkArea disbType;
      private CashReceipt detCashReceipt;
      private CrdCrComboNo detCrdCrComboNo;
      private LocalFinanceWorkArea disbDate;
      private DisbursementTransactionType detDisbursementTransactionType;
      private DisbursementType detDisbursementType;
      private PaymentRequest detPaymentRequest;
      private DisbursementTransaction creditDet;
      private DisbursementTransaction debitDet;
      private DisbursementTransaction detDisbursementTransaction;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of GrDisbDate.
      /// </summary>
      [JsonPropertyName("grDisbDate")]
      public LocalFinanceWorkArea GrDisbDate
      {
        get => grDisbDate ??= new();
        set => grDisbDate = value;
      }

      /// <summary>
      /// A value of GrDetDisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("grDetDisbursementTransactionType")]
      public DisbursementTransactionType GrDetDisbursementTransactionType
      {
        get => grDetDisbursementTransactionType ??= new();
        set => grDetDisbursementTransactionType = value;
      }

      /// <summary>
      /// A value of GrDetDisbursementType.
      /// </summary>
      [JsonPropertyName("grDetDisbursementType")]
      public DisbursementType GrDetDisbursementType
      {
        get => grDetDisbursementType ??= new();
        set => grDetDisbursementType = value;
      }

      /// <summary>
      /// A value of GrDetPaymentRequest.
      /// </summary>
      [JsonPropertyName("grDetPaymentRequest")]
      public PaymentRequest GrDetPaymentRequest
      {
        get => grDetPaymentRequest ??= new();
        set => grDetPaymentRequest = value;
      }

      /// <summary>
      /// A value of GrCreditDet.
      /// </summary>
      [JsonPropertyName("grCreditDet")]
      public DisbursementTransaction GrCreditDet
      {
        get => grCreditDet ??= new();
        set => grCreditDet = value;
      }

      /// <summary>
      /// A value of GrDebitDet.
      /// </summary>
      [JsonPropertyName("grDebitDet")]
      public DisbursementTransaction GrDebitDet
      {
        get => grDebitDet ??= new();
        set => grDebitDet = value;
      }

      /// <summary>
      /// A value of GrDetFnReferenceNumber.
      /// </summary>
      [JsonPropertyName("grDetFnReferenceNumber")]
      public FnReferenceNumber GrDetFnReferenceNumber
      {
        get => grDetFnReferenceNumber ??= new();
        set => grDetFnReferenceNumber = value;
      }

      /// <summary>
      /// A value of GrDetDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("grDetDisbursementTransaction")]
      public DisbursementTransaction GrDetDisbursementTransaction
      {
        get => grDetDisbursementTransaction ??= new();
        set => grDetDisbursementTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private TextWorkArea disbType;
      private LocalFinanceWorkArea grDisbDate;
      private DisbursementTransactionType grDetDisbursementTransactionType;
      private DisbursementType grDetDisbursementType;
      private PaymentRequest grDetPaymentRequest;
      private DisbursementTransaction grCreditDet;
      private DisbursementTransaction grDebitDet;
      private FnReferenceNumber grDetFnReferenceNumber;
      private DisbursementTransaction grDetDisbursementTransaction;
    }

    /// <summary>
    /// A value of Temp2DisbType.
    /// </summary>
    [JsonPropertyName("temp2DisbType")]
    public TextWorkArea Temp2DisbType
    {
      get => temp2DisbType ??= new();
      set => temp2DisbType = value;
    }

    /// <summary>
    /// A value of Temp1DisbType.
    /// </summary>
    [JsonPropertyName("temp1DisbType")]
    public TextWorkArea Temp1DisbType
    {
      get => temp1DisbType ??= new();
      set => temp1DisbType = value;
    }

    /// <summary>
    /// A value of Temp1NegativeDb.
    /// </summary>
    [JsonPropertyName("temp1NegativeDb")]
    public DisbursementTransaction Temp1NegativeDb
    {
      get => temp1NegativeDb ??= new();
      set => temp1NegativeDb = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of Temp1LocalFinanceWorkArea.
    /// </summary>
    [JsonPropertyName("temp1LocalFinanceWorkArea")]
    public LocalFinanceWorkArea Temp1LocalFinanceWorkArea
    {
      get => temp1LocalFinanceWorkArea ??= new();
      set => temp1LocalFinanceWorkArea = value;
    }

    /// <summary>
    /// A value of Temp1DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("temp1DisbursementTransactionType")]
    public DisbursementTransactionType Temp1DisbursementTransactionType
    {
      get => temp1DisbursementTransactionType ??= new();
      set => temp1DisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of Temp1DisbursementType.
    /// </summary>
    [JsonPropertyName("temp1DisbursementType")]
    public DisbursementType Temp1DisbursementType
    {
      get => temp1DisbursementType ??= new();
      set => temp1DisbursementType = value;
    }

    /// <summary>
    /// A value of Temp1PaymentRequest.
    /// </summary>
    [JsonPropertyName("temp1PaymentRequest")]
    public PaymentRequest Temp1PaymentRequest
    {
      get => temp1PaymentRequest ??= new();
      set => temp1PaymentRequest = value;
    }

    /// <summary>
    /// A value of Temp1Credit.
    /// </summary>
    [JsonPropertyName("temp1Credit")]
    public DisbursementTransaction Temp1Credit
    {
      get => temp1Credit ??= new();
      set => temp1Credit = value;
    }

    /// <summary>
    /// A value of Temp1PositiveDebit.
    /// </summary>
    [JsonPropertyName("temp1PositiveDebit")]
    public DisbursementTransaction Temp1PositiveDebit
    {
      get => temp1PositiveDebit ??= new();
      set => temp1PositiveDebit = value;
    }

    /// <summary>
    /// A value of Temp1DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("temp1DisbursementTransaction")]
    public DisbursementTransaction Temp1DisbursementTransaction
    {
      get => temp1DisbursementTransaction ??= new();
      set => temp1DisbursementTransaction = value;
    }

    /// <summary>
    /// A value of Firsttime.
    /// </summary>
    [JsonPropertyName("firsttime")]
    public Common Firsttime
    {
      get => firsttime ??= new();
      set => firsttime = value;
    }

    /// <summary>
    /// A value of ChangedFlag.
    /// </summary>
    [JsonPropertyName("changedFlag")]
    public Common ChangedFlag
    {
      get => changedFlag ??= new();
      set => changedFlag = value;
    }

    /// <summary>
    /// A value of Temp2LocalFinanceWorkArea.
    /// </summary>
    [JsonPropertyName("temp2LocalFinanceWorkArea")]
    public LocalFinanceWorkArea Temp2LocalFinanceWorkArea
    {
      get => temp2LocalFinanceWorkArea ??= new();
      set => temp2LocalFinanceWorkArea = value;
    }

    /// <summary>
    /// A value of Temp2DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("temp2DisbursementTransactionType")]
    public DisbursementTransactionType Temp2DisbursementTransactionType
    {
      get => temp2DisbursementTransactionType ??= new();
      set => temp2DisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of Temp2DisbursementType.
    /// </summary>
    [JsonPropertyName("temp2DisbursementType")]
    public DisbursementType Temp2DisbursementType
    {
      get => temp2DisbursementType ??= new();
      set => temp2DisbursementType = value;
    }

    /// <summary>
    /// A value of Temp2PaymentRequest.
    /// </summary>
    [JsonPropertyName("temp2PaymentRequest")]
    public PaymentRequest Temp2PaymentRequest
    {
      get => temp2PaymentRequest ??= new();
      set => temp2PaymentRequest = value;
    }

    /// <summary>
    /// A value of Temp2Credit.
    /// </summary>
    [JsonPropertyName("temp2Credit")]
    public DisbursementTransaction Temp2Credit
    {
      get => temp2Credit ??= new();
      set => temp2Credit = value;
    }

    /// <summary>
    /// A value of Temp2Debit.
    /// </summary>
    [JsonPropertyName("temp2Debit")]
    public DisbursementTransaction Temp2Debit
    {
      get => temp2Debit ??= new();
      set => temp2Debit = value;
    }

    /// <summary>
    /// A value of Temp2DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("temp2DisbursementTransaction")]
    public DisbursementTransaction Temp2DisbursementTransaction
    {
      get => temp2DisbursementTransaction ??= new();
      set => temp2DisbursementTransaction = value;
    }

    /// <summary>
    /// A value of SwapFlag.
    /// </summary>
    [JsonPropertyName("swapFlag")]
    public Common SwapFlag
    {
      get => swapFlag ??= new();
      set => swapFlag = value;
    }

    private TextWorkArea temp2DisbType;
    private TextWorkArea temp1DisbType;
    private DisbursementTransaction temp1NegativeDb;
    private Array<GroupGroup> group;
    private LocalFinanceWorkArea temp1LocalFinanceWorkArea;
    private DisbursementTransactionType temp1DisbursementTransactionType;
    private DisbursementType temp1DisbursementType;
    private PaymentRequest temp1PaymentRequest;
    private DisbursementTransaction temp1Credit;
    private DisbursementTransaction temp1PositiveDebit;
    private DisbursementTransaction temp1DisbursementTransaction;
    private Common firsttime;
    private Common changedFlag;
    private LocalFinanceWorkArea temp2LocalFinanceWorkArea;
    private DisbursementTransactionType temp2DisbursementTransactionType;
    private DisbursementType temp2DisbursementType;
    private PaymentRequest temp2PaymentRequest;
    private DisbursementTransaction temp2Credit;
    private DisbursementTransaction temp2Debit;
    private DisbursementTransaction temp2DisbursementTransaction;
    private Common swapFlag;
  }
#endregion
}
