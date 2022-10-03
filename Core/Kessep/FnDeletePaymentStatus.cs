// Program: FN_DELETE_PAYMENT_STATUS, ID: 371839450, model: 746.
// Short name: SWE00426
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_PAYMENT_STATUS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will delete unused payment statuses.
/// </para>
/// </summary>
[Serializable]
public partial class FnDeletePaymentStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_PAYMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeletePaymentStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeletePaymentStatus.
  /// </summary>
  public FnDeletePaymentStatus(IContext context, Import import, Export export):
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
    if (ReadPaymentStatus())
    {
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "PAYMENT_STATUS";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.PaymentStatus.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.PaymentStatus.Code;
      export.TypeStatusAudit.Description = entities.PaymentStatus.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.PaymentStatus.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.PaymentStatus.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy = entities.PaymentStatus.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.PaymentStatus.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.PaymentStatus.LastUpdateBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.PaymentStatus.LastUpdateTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      DeletePaymentStatus();
    }
    else
    {
      export.PaymentStatus.Assign(import.PaymentStatus);
      ExitState = "PAYMENT_STATUS_NF";
    }
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    useImport.TypeStatusAudit.Assign(export.TypeStatusAudit);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private void DeletePaymentStatus()
  {
    Update("DeletePaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          entities.PaymentStatus.SystemGeneratedIdentifier);
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          import.PaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatus.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentStatus.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.PaymentStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentStatus.Description = db.GetNullableString(reader, 9);
        entities.PaymentStatus.Populated = true;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private PaymentStatus paymentStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private PaymentStatus paymentStatus;
    private TypeStatusAudit typeStatusAudit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private PaymentStatus paymentStatus;
  }
#endregion
}
