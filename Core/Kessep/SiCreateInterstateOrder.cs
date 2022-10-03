// Program: SI_CREATE_INTERSTATE_ORDER, ID: 371084084, model: 746.
// Short name: SWE02624
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_INTERSTATE_ORDER.
/// </summary>
[Serializable]
public partial class SiCreateInterstateOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_INTERSTATE_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateInterstateOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateInterstateOrder.
  /// </summary>
  public SiCreateInterstateOrder(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // --------------------------------------------------------------------------
    // 2001/04/16	M Ramirez			Initial Development
    // --------------------------------------------------------------------------
    if (import.InterstateSupportOrder.SystemGeneratedSequenceNum > 9)
    {
      ExitState = "INTERSTATE_SUPPORT_ORDER_PV";

      return;
    }

    if (!ReadInterstateCase())
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    try
    {
      CreateInterstateSupportOrder();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_SUPPORT_ORDER_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "INTERSTATE_SUPPORT_ORDER_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateInterstateSupportOrder()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var systemGeneratedSequenceNum =
      import.InterstateSupportOrder.SystemGeneratedSequenceNum;
    var ccaTranSerNum = entities.InterstateCase.TransSerialNumber;
    var fipsState = import.InterstateSupportOrder.FipsState;
    var fipsCounty = import.InterstateSupportOrder.FipsCounty ?? "";
    var fipsLocation = import.InterstateSupportOrder.FipsLocation ?? "";
    var number = import.InterstateSupportOrder.Number;
    var orderFilingDate = import.InterstateSupportOrder.OrderFilingDate;
    var type1 = import.InterstateSupportOrder.Type1 ?? "";
    var debtType = import.InterstateSupportOrder.DebtType;
    var paymentFreq = import.InterstateSupportOrder.PaymentFreq ?? "";
    var amountOrdered =
      import.InterstateSupportOrder.AmountOrdered.GetValueOrDefault();
    var effectiveDate = import.InterstateSupportOrder.EffectiveDate;
    var endDate = import.InterstateSupportOrder.EndDate;
    var cancelDate = import.InterstateSupportOrder.CancelDate;
    var arrearsFreq = import.InterstateSupportOrder.ArrearsFreq ?? "";
    var arrearsFreqAmount =
      import.InterstateSupportOrder.ArrearsFreqAmount.GetValueOrDefault();
    var arrearsTotalAmount =
      import.InterstateSupportOrder.ArrearsTotalAmount.GetValueOrDefault();
    var arrearsAfdcFromDate = import.InterstateSupportOrder.ArrearsAfdcFromDate;
    var arrearsAfdcThruDate = import.InterstateSupportOrder.ArrearsAfdcThruDate;
    var arrearsAfdcAmount =
      import.InterstateSupportOrder.ArrearsAfdcAmount.GetValueOrDefault();
    var arrearsNonAfdcFromDate =
      import.InterstateSupportOrder.ArrearsNonAfdcFromDate;
    var arrearsNonAfdcThruDate =
      import.InterstateSupportOrder.ArrearsNonAfdcThruDate;
    var arrearsNonAfdcAmount =
      import.InterstateSupportOrder.ArrearsNonAfdcAmount.GetValueOrDefault();
    var fosterCareFromDate = import.InterstateSupportOrder.FosterCareFromDate;
    var fosterCareThruDate = import.InterstateSupportOrder.FosterCareThruDate;
    var fosterCareAmount =
      import.InterstateSupportOrder.FosterCareAmount.GetValueOrDefault();
    var medicalFromDate = import.InterstateSupportOrder.MedicalFromDate;
    var medicalThruDate = import.InterstateSupportOrder.MedicalThruDate;
    var medicalAmount =
      import.InterstateSupportOrder.MedicalAmount.GetValueOrDefault();
    var medicalOrderedInd = import.InterstateSupportOrder.MedicalOrderedInd ?? ""
      ;
    var tribunalCaseNumber =
      import.InterstateSupportOrder.TribunalCaseNumber ?? "";
    var dateOfLastPayment = import.InterstateSupportOrder.DateOfLastPayment;
    var controllingOrderFlag =
      import.InterstateSupportOrder.ControllingOrderFlag ?? "";
    var newOrderFlag = import.InterstateSupportOrder.NewOrderFlag ?? "";
    var docketNumber = import.InterstateSupportOrder.DocketNumber ?? "";

    entities.InterstateSupportOrder.Populated = false;
    Update("CreateInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetInt64(command, "ccaTranSerNum", ccaTranSerNum);
        db.SetString(command, "fipsState", fipsState);
        db.SetNullableString(command, "fipsCounty", fipsCounty);
        db.SetNullableString(command, "fipsLocation", fipsLocation);
        db.SetString(command, "number", number);
        db.SetDate(command, "orderFilingDate", orderFilingDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "debtType", debtType);
        db.SetNullableString(command, "paymentFreq", paymentFreq);
        db.SetNullableDecimal(command, "amountOrdered", amountOrdered);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableDate(command, "cancelDate", cancelDate);
        db.SetNullableString(command, "arrearsFreq", arrearsFreq);
        db.SetNullableDecimal(command, "arrearsFrqAmt", arrearsFreqAmount);
        db.SetNullableDecimal(command, "arrearsTotalAmt", arrearsTotalAmount);
        db.SetNullableDate(command, "arrsAfdcFromDte", arrearsAfdcFromDate);
        db.SetNullableDate(command, "arrsAfdcThruDte", arrearsAfdcThruDate);
        db.SetNullableDecimal(command, "arrearsAfdcAmt", arrearsAfdcAmount);
        db.SetNullableDate(command, "arrNafdcFromDte", arrearsNonAfdcFromDate);
        db.SetNullableDate(command, "arrNafdcThruDte", arrearsNonAfdcThruDate);
        db.SetNullableDecimal(command, "arrNafdcAmt", arrearsNonAfdcAmount);
        db.SetNullableDate(command, "fostCareFromDte", fosterCareFromDate);
        db.SetNullableDate(command, "fostCareThruDte", fosterCareThruDate);
        db.SetNullableDecimal(command, "fosterCareAmount", fosterCareAmount);
        db.SetNullableDate(command, "medicalFromDate", medicalFromDate);
        db.SetNullableDate(command, "medicalThruDate", medicalThruDate);
        db.SetNullableDecimal(command, "medicalAmount", medicalAmount);
        db.SetNullableString(command, "medicalOrderedIn", medicalOrderedInd);
        db.SetNullableString(command, "tribunalCaseNum", tribunalCaseNumber);
        db.SetNullableDate(command, "dateOfLastPay", dateOfLastPayment);
        db.SetNullableString(command, "cntrlOrderFlag", controllingOrderFlag);
        db.SetNullableString(command, "newOrderFlag", newOrderFlag);
        db.SetNullableString(command, "docketNumber", docketNumber);
        db.SetNullableInt32(command, "legalActionId", 0);
      });

    entities.InterstateSupportOrder.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateSupportOrder.CcaTranSerNum = ccaTranSerNum;
    entities.InterstateSupportOrder.FipsState = fipsState;
    entities.InterstateSupportOrder.FipsCounty = fipsCounty;
    entities.InterstateSupportOrder.FipsLocation = fipsLocation;
    entities.InterstateSupportOrder.Number = number;
    entities.InterstateSupportOrder.OrderFilingDate = orderFilingDate;
    entities.InterstateSupportOrder.Type1 = type1;
    entities.InterstateSupportOrder.DebtType = debtType;
    entities.InterstateSupportOrder.PaymentFreq = paymentFreq;
    entities.InterstateSupportOrder.AmountOrdered = amountOrdered;
    entities.InterstateSupportOrder.EffectiveDate = effectiveDate;
    entities.InterstateSupportOrder.EndDate = endDate;
    entities.InterstateSupportOrder.CancelDate = cancelDate;
    entities.InterstateSupportOrder.ArrearsFreq = arrearsFreq;
    entities.InterstateSupportOrder.ArrearsFreqAmount = arrearsFreqAmount;
    entities.InterstateSupportOrder.ArrearsTotalAmount = arrearsTotalAmount;
    entities.InterstateSupportOrder.ArrearsAfdcFromDate = arrearsAfdcFromDate;
    entities.InterstateSupportOrder.ArrearsAfdcThruDate = arrearsAfdcThruDate;
    entities.InterstateSupportOrder.ArrearsAfdcAmount = arrearsAfdcAmount;
    entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
      arrearsNonAfdcFromDate;
    entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
      arrearsNonAfdcThruDate;
    entities.InterstateSupportOrder.ArrearsNonAfdcAmount = arrearsNonAfdcAmount;
    entities.InterstateSupportOrder.FosterCareFromDate = fosterCareFromDate;
    entities.InterstateSupportOrder.FosterCareThruDate = fosterCareThruDate;
    entities.InterstateSupportOrder.FosterCareAmount = fosterCareAmount;
    entities.InterstateSupportOrder.MedicalFromDate = medicalFromDate;
    entities.InterstateSupportOrder.MedicalThruDate = medicalThruDate;
    entities.InterstateSupportOrder.MedicalAmount = medicalAmount;
    entities.InterstateSupportOrder.MedicalOrderedInd = medicalOrderedInd;
    entities.InterstateSupportOrder.TribunalCaseNumber = tribunalCaseNumber;
    entities.InterstateSupportOrder.DateOfLastPayment = dateOfLastPayment;
    entities.InterstateSupportOrder.ControllingOrderFlag = controllingOrderFlag;
    entities.InterstateSupportOrder.NewOrderFlag = newOrderFlag;
    entities.InterstateSupportOrder.DocketNumber = docketNumber;
    entities.InterstateSupportOrder.Populated = true;
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
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
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateSupportOrder interstateSupportOrder;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateSupportOrder interstateSupportOrder;
    private InterstateCase interstateCase;
  }
#endregion
}
