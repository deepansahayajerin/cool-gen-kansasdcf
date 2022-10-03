// Program: SI_READ_CSENET_SUPPORT_ORDERS, ID: 372519081, model: 746.
// Short name: SWE01218
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
/// A program: SI_READ_CSENET_SUPPORT_ORDERS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// The PAB reads CSENet Referral Case records.  Then, it reads all the Support 
/// Orders attached to each. (Up to nine)
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCsenetSupportOrders: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSENET_SUPPORT_ORDERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsenetSupportOrders(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsenetSupportOrders.
  /// </summary>
  public SiReadCsenetSupportOrders(IContext context, Import import,
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
    // *********************************************
    // This action block retrieves all the Support
    // Order records attached to a Case
    // *********************************************
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    export.Export1.Index = -1;

    foreach(var item in ReadInterstateSupportOrder())
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.G.Assign(entities.InterstateSupportOrder);

      if (Equal(export.Export1.Item.G.ArrearsAfdcThruDate, local.MaxDate.Date))
      {
        export.Export1.Update.G.ArrearsAfdcThruDate = local.MinDate.Date;
      }

      if (Equal(export.Export1.Item.G.ArrearsNonAfdcThruDate, local.MaxDate.Date))
        
      {
        export.Export1.Update.G.ArrearsNonAfdcThruDate = local.MinDate.Date;
      }

      if (Equal(export.Export1.Item.G.MedicalThruDate, local.MaxDate.Date))
      {
        export.Export1.Update.G.MedicalThruDate = local.MinDate.Date;
      }

      if (Equal(export.Export1.Item.G.FosterCareThruDate, local.MaxDate.Date))
      {
        export.Export1.Update.G.FosterCareThruDate = local.MinDate.Date;
      }

      if (Equal(export.Export1.Item.G.EndDate, local.MaxDate.Date))
      {
        export.Export1.Update.G.EndDate = local.MinDate.Date;
      }

      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        return;
      }
    }
  }

  private IEnumerable<bool> ReadInterstateSupportOrder()
  {
    entities.InterstateSupportOrder.Populated = false;

    return ReadEach("ReadInterstateSupportOrder",
      (db, command) =>
      {
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
        entities.InterstateSupportOrder.Populated = true;

        return true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MinDate.
    /// </summary>
    [JsonPropertyName("minDate")]
    public DateWorkArea MinDate
    {
      get => minDate ??= new();
      set => minDate = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DateWorkArea minDate;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    private InterstateCase interstateCase;
    private InterstateSupportOrder interstateSupportOrder;
  }
#endregion
}
