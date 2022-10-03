// Program: FN_CREATE_PAYMENT_METHOD_TYPE, ID: 371828225, model: 746.
// Short name: SWE00382
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_PAYMENT_METHOD_TYPE.
/// </summary>
[Serializable]
public partial class FnCreatePaymentMethodType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_PAYMENT_METHOD_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreatePaymentMethodType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreatePaymentMethodType.
  /// </summary>
  public FnCreatePaymentMethodType(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    export.PaymentMethodType.Assign(import.PaymentMethodType);

    // *** Validate input
    // If discontinue date is blank, then default it to max date
    // ------------------------------------------------
    // Check for existing record's date range  to prevent from adding an 
    // existing record with the same date range
    // ------------------------------------------------
    if (ReadPaymentMethodType())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      try
      {
        CreatePaymentMethodType();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PYMNT_MTHD_TYPE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PYMNT_MTHD_TYPE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private int UseFnAssignPmntMethodTypeId()
  {
    var useImport = new FnAssignPmntMethodTypeId.Import();
    var useExport = new FnAssignPmntMethodTypeId.Export();

    Call(FnAssignPmntMethodTypeId.Execute, useImport, useExport);

    return useExport.PaymentMethodType.SystemGeneratedIdentifier;
  }

  private void CreatePaymentMethodType()
  {
    var systemGeneratedIdentifier = UseFnAssignPmntMethodTypeId();
    var code = import.PaymentMethodType.Code;
    var name = import.PaymentMethodType.Name;
    var effectiveDate = import.PaymentMethodType.EffectiveDate;
    var discontinueDate = import.PaymentMethodType.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var description = import.PaymentMethodType.Description ?? "";

    entities.PaymentMethodType.Populated = false;
    Update("CreatePaymentMethodType",
      (db, command) =>
      {
        db.SetInt32(command, "paymntMethTypId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.PaymentMethodType.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentMethodType.Code = code;
    entities.PaymentMethodType.Name = name;
    entities.PaymentMethodType.EffectiveDate = effectiveDate;
    entities.PaymentMethodType.DiscontinueDate = discontinueDate;
    entities.PaymentMethodType.CreatedBy = createdBy;
    entities.PaymentMethodType.CreatedTimestamp = createdTimestamp;
    entities.PaymentMethodType.Description = description;
    entities.PaymentMethodType.Populated = true;
  }

  private bool ReadPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
        db.SetDate(
          command, "effectiveDate1",
          import.PaymentMethodType.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.PaymentMethodType.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Code = db.GetString(reader, 1);
        entities.PaymentMethodType.Name = db.GetString(reader, 2);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentMethodType.CreatedBy = db.GetString(reader, 5);
        entities.PaymentMethodType.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 7);
        entities.PaymentMethodType.Populated = true;
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
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType paymentMethodType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType paymentMethodType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType paymentMethodType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType paymentMethodType;
  }
#endregion
}
