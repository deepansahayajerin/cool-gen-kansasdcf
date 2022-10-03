// Program: FN_B643_COUPON_PAYMENT_ADDRESS, ID: 372685957, model: 746.
// Short name: SWE02389
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_COUPON_PAYMENT_ADDRESS.
/// </summary>
[Serializable]
public partial class FnB643CouponPaymentAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_COUPON_PAYMENT_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643CouponPaymentAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643CouponPaymentAddress.
  /// </summary>
  public FnB643CouponPaymentAddress(IContext context, Import import,
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
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * #84048  01-05-2000  Ed Lyman  Suppress if address not verified.  *
    // *
    // 
    // *
    // * #KPC    11-02-2000  Ed Lyman  Hardcode the address for KPC.      *
    // ********************************************************************
    export.AddressNotFound.Flag = "";

    if (AsChar(import.Obligation.OrderTypeCode) == 'I')
    {
      // *** Interstate Obligation
      if (ReadInterstateRequestObligationInterstateRequest())
      {
        export.PaymentAddressPayTo.Text30 =
          entities.InterstatePaymentAddress.PayableToName ?? Spaces(30);
        export.CsePersonAddress.Street1 =
          entities.InterstatePaymentAddress.Street1;
        export.CsePersonAddress.Street2 =
          entities.InterstatePaymentAddress.Street2;
        export.CsePersonAddress.City = entities.InterstatePaymentAddress.City;
        export.CsePersonAddress.State = entities.InterstatePaymentAddress.State;
        export.CsePersonAddress.Street3 =
          entities.InterstatePaymentAddress.Street3;
        export.CsePersonAddress.Street4 =
          entities.InterstatePaymentAddress.Street4;
        export.CsePersonAddress.ZipCode =
          entities.InterstatePaymentAddress.ZipCode;
        export.CsePersonAddress.Zip4 = entities.InterstatePaymentAddress.Zip4;
        export.CsePersonAddress.Zip3 = entities.InterstatePaymentAddress.Zip3;
      }
      else
      {
        export.AddressNotFound.Flag = "Y";
        export.EabReportSend.RptDetail =
          "Interstate Payment Address Not Found for this Obligation Id :  " + NumberToString
          (import.Obligation.SystemGeneratedIdentifier, 13, 3);
      }
    }
    else
    {
      export.PaymentAddressPayTo.Text30 = "KANSAS PAY CENTER";
      export.CsePersonAddress.Street1 = "PO BOX 758599";
      export.CsePersonAddress.Street2 = "";
      export.CsePersonAddress.Street3 = "";
      export.CsePersonAddress.Street4 = "";
      export.CsePersonAddress.City = "TOPEKA";
      export.CsePersonAddress.State = "KS";
      export.CsePersonAddress.ZipCode = "66675";
      export.CsePersonAddress.Zip4 = "8599";
      export.CsePersonAddress.Zip3 = "";
    }
  }

  private bool ReadInterstateRequestObligationInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.InterstateRequestObligation.Populated = false;
    entities.InterstateRequest.Populated = false;
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstateRequestObligationInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDate(
          command, "addressStartDate",
          import.CouponBegin.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequestObligation.OtyType = db.GetInt32(reader, 0);
        entities.InterstateRequestObligation.CpaType = db.GetString(reader, 1);
        entities.InterstateRequestObligation.CspNumber =
          db.GetString(reader, 2);
        entities.InterstateRequestObligation.ObgGeneratedId =
          db.GetInt32(reader, 3);
        entities.InterstateRequestObligation.IntGeneratedId =
          db.GetInt32(reader, 4);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 4);
        entities.InterstateRequestObligation.OrderEffectiveDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateRequestObligation.OrderEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 7);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 8);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 9);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 10);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 11);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 12);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 15);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 16);
        entities.InterstatePaymentAddress.Zip3 =
          db.GetNullableString(reader, 17);
        entities.InterstatePaymentAddress.Street3 =
          db.GetNullableString(reader, 18);
        entities.InterstatePaymentAddress.Street4 =
          db.GetNullableString(reader, 19);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 20);
        entities.InterstateRequestObligation.Populated = true;
        entities.InterstateRequest.Populated = true;
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstateRequestObligation>("CpaType",
          entities.InterstateRequestObligation.CpaType);
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);
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
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of CouponBegin.
    /// </summary>
    [JsonPropertyName("couponBegin")]
    public DateWorkArea CouponBegin
    {
      get => couponBegin ??= new();
      set => couponBegin = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private CsePerson client;
    private DateWorkArea couponBegin;
    private LegalAction legalAction;
    private Obligation obligation;
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
    /// A value of AddressNotFound.
    /// </summary>
    [JsonPropertyName("addressNotFound")]
    public Common AddressNotFound
    {
      get => addressNotFound ??= new();
      set => addressNotFound = value;
    }

    /// <summary>
    /// A value of PaymentAddressPayTo.
    /// </summary>
    [JsonPropertyName("paymentAddressPayTo")]
    public TextWorkArea PaymentAddressPayTo
    {
      get => paymentAddressPayTo ??= new();
      set => paymentAddressPayTo = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private EabReportSend eabReportSend;
    private Common addressNotFound;
    private TextWorkArea paymentAddressPayTo;
    private CsePersonAddress csePersonAddress;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of ActiveAddrFound.
    /// </summary>
    [JsonPropertyName("activeAddrFound")]
    public Common ActiveAddrFound
    {
      get => activeAddrFound ??= new();
      set => activeAddrFound = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePersonsWorkSet Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of LastProcessed.
    /// </summary>
    [JsonPropertyName("lastProcessed")]
    public DriverTable LastProcessed
    {
      get => lastProcessed ??= new();
      set => lastProcessed = value;
    }

    /// <summary>
    /// A value of CpnPmntAddrId.
    /// </summary>
    [JsonPropertyName("cpnPmntAddrId")]
    public Common CpnPmntAddrId
    {
      get => cpnPmntAddrId ??= new();
      set => cpnPmntAddrId = value;
    }

    /// <summary>
    /// A value of TypeOfObligation.
    /// </summary>
    [JsonPropertyName("typeOfObligation")]
    public Common TypeOfObligation
    {
      get => typeOfObligation ??= new();
      set => typeOfObligation = value;
    }

    /// <summary>
    /// A value of DocSupForObligation.
    /// </summary>
    [JsonPropertyName("docSupForObligation")]
    public Common DocSupForObligation
    {
      get => docSupForObligation ??= new();
      set => docSupForObligation = value;
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

    private DateWorkArea null1;
    private ProgramError programError;
    private Common activeAddrFound;
    private DateWorkArea current;
    private CsePersonsWorkSet work;
    private DriverTable lastProcessed;
    private Common cpnPmntAddrId;
    private Common typeOfObligation;
    private Common docSupForObligation;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private InterstateRequestObligation interstateRequestObligation;
    private InterstateRequest interstateRequest;
    private InterstatePaymentAddress interstatePaymentAddress;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private OfficeAddress officeAddress;
    private Office office;
  }
#endregion
}
