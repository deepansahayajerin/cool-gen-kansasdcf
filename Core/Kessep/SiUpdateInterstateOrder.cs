// Program: SI_UPDATE_INTERSTATE_ORDER, ID: 373441272, model: 746.
// Short name: SWE02760
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_INTERSTATE_ORDER.
/// </summary>
[Serializable]
public partial class SiUpdateInterstateOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_INTERSTATE_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateInterstateOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateInterstateOrder.
  /// </summary>
  public SiUpdateInterstateOrder(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadInterstateSupportOrder())
    {
      try
      {
        UpdateInterstateSupportOrder();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_SUPPORT_ORDER_NU";

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
    else if (ReadInterstateCase())
    {
      ExitState = "INTERSTATE_SUPPORT_ORDER_NF";
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
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

  private bool ReadInterstateSupportOrder()
  {
    entities.InterstateSupportOrder.Populated = false;

    return Read("ReadInterstateSupportOrder",
      (db, command) =>
      {
        db.SetInt32(
          command, "sysGeneratedId",
          import.InterstateSupportOrder.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTranSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateSupportOrder.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateSupportOrder.CcaTranSerNum = db.GetInt64(reader, 2);
        entities.InterstateSupportOrder.FipsState = db.GetString(reader, 3);
        entities.InterstateSupportOrder.FipsCounty =
          db.GetNullableString(reader, 4);
        entities.InterstateSupportOrder.FipsLocation =
          db.GetNullableString(reader, 5);
        entities.InterstateSupportOrder.Number = db.GetString(reader, 6);
        entities.InterstateSupportOrder.OrderFilingDate = db.GetDate(reader, 7);
        entities.InterstateSupportOrder.Type1 = db.GetNullableString(reader, 8);
        entities.InterstateSupportOrder.DebtType = db.GetString(reader, 9);
        entities.InterstateSupportOrder.PaymentFreq =
          db.GetNullableString(reader, 10);
        entities.InterstateSupportOrder.AmountOrdered =
          db.GetNullableDecimal(reader, 11);
        entities.InterstateSupportOrder.EffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.InterstateSupportOrder.EndDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateSupportOrder.CancelDate =
          db.GetNullableDate(reader, 14);
        entities.InterstateSupportOrder.ArrearsFreq =
          db.GetNullableString(reader, 15);
        entities.InterstateSupportOrder.ArrearsFreqAmount =
          db.GetNullableDecimal(reader, 16);
        entities.InterstateSupportOrder.ArrearsTotalAmount =
          db.GetNullableDecimal(reader, 17);
        entities.InterstateSupportOrder.ArrearsAfdcFromDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateSupportOrder.ArrearsAfdcThruDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateSupportOrder.ArrearsAfdcAmount =
          db.GetNullableDecimal(reader, 20);
        entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateSupportOrder.ArrearsNonAfdcAmount =
          db.GetNullableDecimal(reader, 23);
        entities.InterstateSupportOrder.FosterCareFromDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateSupportOrder.FosterCareThruDate =
          db.GetNullableDate(reader, 25);
        entities.InterstateSupportOrder.FosterCareAmount =
          db.GetNullableDecimal(reader, 26);
        entities.InterstateSupportOrder.MedicalFromDate =
          db.GetNullableDate(reader, 27);
        entities.InterstateSupportOrder.MedicalThruDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateSupportOrder.MedicalAmount =
          db.GetNullableDecimal(reader, 29);
        entities.InterstateSupportOrder.MedicalOrderedInd =
          db.GetNullableString(reader, 30);
        entities.InterstateSupportOrder.TribunalCaseNumber =
          db.GetNullableString(reader, 31);
        entities.InterstateSupportOrder.DateOfLastPayment =
          db.GetNullableDate(reader, 32);
        entities.InterstateSupportOrder.ControllingOrderFlag =
          db.GetNullableString(reader, 33);
        entities.InterstateSupportOrder.NewOrderFlag =
          db.GetNullableString(reader, 34);
        entities.InterstateSupportOrder.DocketNumber =
          db.GetNullableString(reader, 35);
        entities.InterstateSupportOrder.LegalActionId =
          db.GetNullableInt32(reader, 36);
        entities.InterstateSupportOrder.Populated = true;
      });
  }

  private void UpdateInterstateSupportOrder()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateSupportOrder.Populated);

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
    var legalActionId =
      import.InterstateSupportOrder.LegalActionId.GetValueOrDefault();

    entities.InterstateSupportOrder.Populated = false;
    Update("UpdateInterstateSupportOrder",
      (db, command) =>
      {
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
        db.SetNullableInt32(command, "legalActionId", legalActionId);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateSupportOrder.CcaTransactionDt.GetValueOrDefault());
          
        db.SetInt32(
          command, "sysGeneratedId",
          entities.InterstateSupportOrder.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTranSerNum",
          entities.InterstateSupportOrder.CcaTranSerNum);
      });

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
    entities.InterstateSupportOrder.LegalActionId = legalActionId;
    entities.InterstateSupportOrder.Populated = true;
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
