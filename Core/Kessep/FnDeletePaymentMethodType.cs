// Program: FN_DELETE_PAYMENT_METHOD_TYPE, ID: 371828231, model: 746.
// Short name: SWE00425
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_PAYMENT_METHOD_TYPE.
/// </summary>
[Serializable]
public partial class FnDeletePaymentMethodType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_PAYMENT_METHOD_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeletePaymentMethodType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeletePaymentMethodType.
  /// </summary>
  public FnDeletePaymentMethodType(IContext context, Import import,
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

    // ***** EDIT AREA *****
    if (ReadPaymentMethodType())
    {
      // ***** MAIN-LINE AREA *****
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "PAYMENT METHOD TYPE";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.PaymentMethodType.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.PaymentMethodType.Code;
      export.TypeStatusAudit.Description =
        entities.PaymentMethodType.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.PaymentMethodType.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.PaymentMethodType.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy = entities.PaymentMethodType.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.PaymentMethodType.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.PaymentMethodType.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.PaymentMethodType.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      DeletePaymentMethodType();
    }
    else
    {
      ExitState = "FN0000_PYMNT_MTHD_TYPE_NF";
    }
  }

  private static void MoveTypeStatusAudit(TypeStatusAudit source,
    TypeStatusAudit target)
  {
    target.StringOfOthers = source.StringOfOthers;
    target.TableName = source.TableName;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    MoveTypeStatusAudit(export.TypeStatusAudit, useImport.TypeStatusAudit);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private void DeletePaymentMethodType()
  {
    Update("DeletePaymentMethodType",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymntMethTypId",
          entities.PaymentMethodType.SystemGeneratedIdentifier);
      });
  }

  private bool ReadPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.PaymentMethodType.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Code = db.GetString(reader, 1);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 2);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaymentMethodType.CreatedBy = db.GetString(reader, 4);
        entities.PaymentMethodType.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PaymentMethodType.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.PaymentMethodType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 8);
        entities.PaymentMethodType.Populated = true;
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
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private TypeStatusAudit typeStatusAudit;
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
