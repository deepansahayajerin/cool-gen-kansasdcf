// Program: FN_CREATE_PAYMENT_STATUS, ID: 371839449, model: 746.
// Short name: SWE00384
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_PAYMENT_STATUS.
/// </summary>
[Serializable]
public partial class FnCreatePaymentStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_PAYMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreatePaymentStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreatePaymentStatus.
  /// </summary>
  public FnCreatePaymentStatus(IContext context, Import import, Export export):
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
    export.PaymentStatus.Assign(import.PaymentStatus);

    // ***********************
    //     Validate input
    // ***********************
    // Check for existing record to prevent adding an existing record
    if (ReadPaymentStatus())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

      // If Discontinue date is blank, then default it to max date
      if (Equal(import.PaymentStatus.DiscontinueDate, null))
      {
        local.PaymentStatus.DiscontinueDate = local.Maximum.Date;
      }
      else
      {
        local.PaymentStatus.DiscontinueDate =
          import.PaymentStatus.DiscontinueDate;
      }

      // ***** MAIN-LINE AREA *****
      try
      {
        CreatePaymentStatus();
        export.PaymentStatus.Assign(entities.PaymentStatus);

        if (Equal(export.PaymentStatus.DiscontinueDate, local.Maximum.Date))
        {
          export.PaymentStatus.DiscontinueDate = local.Zero.Date;
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PYMNT_STATUS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PYMNT_STAT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseFnAssignPaymentStatusId()
  {
    var useImport = new FnAssignPaymentStatusId.Import();
    var useExport = new FnAssignPaymentStatusId.Export();

    Call(FnAssignPaymentStatusId.Execute, useImport, useExport);

    return useExport.PaymentStatus.SystemGeneratedIdentifier;
  }

  private void CreatePaymentStatus()
  {
    var systemGeneratedIdentifier = UseFnAssignPaymentStatusId();
    var code = import.PaymentStatus.Code;
    var name = import.PaymentStatus.Name;
    var effectiveDate = import.PaymentStatus.EffectiveDate;
    var discontinueDate = local.PaymentStatus.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var description = import.PaymentStatus.Description ?? "";

    entities.PaymentStatus.Populated = false;
    Update("CreatePaymentStatus",
      (db, command) =>
      {
        db.SetInt32(command, "paymentStatusId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdateBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.PaymentStatus.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatus.Code = code;
    entities.PaymentStatus.Name = name;
    entities.PaymentStatus.EffectiveDate = effectiveDate;
    entities.PaymentStatus.DiscontinueDate = discontinueDate;
    entities.PaymentStatus.CreatedBy = createdBy;
    entities.PaymentStatus.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatus.Description = description;
    entities.PaymentStatus.Populated = true;
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
        db.SetDate(
          command, "effectiveDate1",
          export.PaymentStatus.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          export.PaymentStatus.DiscontinueDate.GetValueOrDefault());
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
        entities.PaymentStatus.Description = db.GetNullableString(reader, 7);
        entities.PaymentStatus.Populated = true;
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

    private PaymentStatus paymentStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Used.
    /// </summary>
    [JsonPropertyName("used")]
    public DateWorkArea Used
    {
      get => used ??= new();
      set => used = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkArea used;
    private DateWorkArea zero;
    private DateWorkArea maximum;
    private PaymentStatus paymentStatus;
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
