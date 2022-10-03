// Program: FN_UPDATE_PAYMENT_STATUS, ID: 371839451, model: 746.
// Short name: SWE00670
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_PAYMENT_STATUS.
/// </summary>
[Serializable]
public partial class FnUpdatePaymentStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_PAYMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdatePaymentStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdatePaymentStatus.
  /// </summary>
  public FnUpdatePaymentStatus(IContext context, Import import, Export export):
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

    // ------------------------------
    // If Discontinue date is blank, then default to max date
    // ------------------------------
    local.Update.Flag = "";
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate2();

    if (Equal(import.PaymentStatus.DiscontinueDate, null))
    {
      local.PaymentStatus.DiscontinueDate = UseCabSetMaximumDiscontinueDate1();
      local.PaymentStatus.DiscontinueDate = local.Maximum.Date;
    }
    else
    {
      local.PaymentStatus.DiscontinueDate =
        import.PaymentStatus.DiscontinueDate;
    }

    // --------------------
    // Check for an existing record before you can update.
    // --------------------
    if (ReadPaymentStatus1())
    {
      local.Update.Flag = "Y";
    }

    ReadPaymentStatus3();
    ReadPaymentStatus4();

    if (!Lt(entities.Test1.DiscontinueDate, import.PaymentStatus.DiscontinueDate)
      || !Lt(import.PaymentStatus.EffectiveDate, entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate, import.PaymentStatus.EffectiveDate) &&
      !
      Lt(import.PaymentStatus.DiscontinueDate, entities.Test2.DiscontinueDate) ||
      !Lt(entities.Test1.EffectiveDate, import.PaymentStatus.EffectiveDate) && import
      .PaymentStatus.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier || !
      Lt(import.PaymentStatus.DiscontinueDate, entities.Test2.DiscontinueDate) &&
      import.PaymentStatus.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadPaymentStatus5())
      {
        if (Equal(import.PaymentStatus.EffectiveDate,
          entities.PaymentStatus.EffectiveDate) || Equal
          (import.PaymentStatus.DiscontinueDate,
          entities.PaymentStatus.DiscontinueDate) || Equal
          (import.PaymentStatus.DiscontinueDate,
          entities.PaymentStatus.EffectiveDate) || Equal
          (import.PaymentStatus.EffectiveDate,
          entities.PaymentStatus.DiscontinueDate) || Lt
          (entities.PaymentStatus.EffectiveDate,
          import.PaymentStatus.DiscontinueDate) && Lt
          (import.PaymentStatus.DiscontinueDate,
          entities.PaymentStatus.DiscontinueDate) || Lt
          (entities.PaymentStatus.EffectiveDate,
          import.PaymentStatus.EffectiveDate) && Lt
          (import.PaymentStatus.EffectiveDate,
          entities.PaymentStatus.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }
        else
        {
          continue;
        }
      }

      local.Update.Flag = "Y";
    }

    if (AsChar(local.Update.Flag) == 'Y')
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

      if (ReadPaymentStatus2())
      {
        try
        {
          UpdatePaymentStatus();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PYMNT_STAT_NU";

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
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    useImport.TypeStatusAudit.Assign(export.TypeStatusAudit);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadPaymentStatus1()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
        db.SetDate(
          command, "effectiveDate",
          import.PaymentStatus.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.PaymentStatus.DiscontinueDate.GetValueOrDefault());
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

  private bool ReadPaymentStatus2()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus2",
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

  private bool ReadPaymentStatus3()
  {
    entities.Test1.Populated = false;

    return Read("ReadPaymentStatus3",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test1.Code = db.GetString(reader, 1);
        entities.Test1.Name = db.GetString(reader, 2);
        entities.Test1.EffectiveDate = db.GetDate(reader, 3);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Test1.CreatedBy = db.GetString(reader, 5);
        entities.Test1.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Test1.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.Test1.LastUpdateTmst = db.GetNullableDateTime(reader, 8);
        entities.Test1.Description = db.GetNullableString(reader, 9);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadPaymentStatus4()
  {
    entities.Test2.Populated = false;

    return Read("ReadPaymentStatus4",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test2.Code = db.GetString(reader, 1);
        entities.Test2.Name = db.GetString(reader, 2);
        entities.Test2.EffectiveDate = db.GetDate(reader, 3);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Test2.CreatedBy = db.GetString(reader, 5);
        entities.Test2.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Test2.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.Test2.LastUpdateTmst = db.GetNullableDateTime(reader, 8);
        entities.Test2.Description = db.GetNullableString(reader, 9);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatus5()
  {
    entities.PaymentStatus.Populated = false;

    return ReadEach("ReadPaymentStatus5",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
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

        return true;
      });
  }

  private void UpdatePaymentStatus()
  {
    var code = import.PaymentStatus.Code;
    var name = import.PaymentStatus.Name;
    var effectiveDate = import.PaymentStatus.EffectiveDate;
    var discontinueDate = import.PaymentStatus.DiscontinueDate;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = Now();
    var description = import.PaymentStatus.Description ?? "";

    entities.PaymentStatus.Populated = false;
    Update("UpdatePaymentStatus",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "paymentStatusId",
          entities.PaymentStatus.SystemGeneratedIdentifier);
      });

    entities.PaymentStatus.Code = code;
    entities.PaymentStatus.Name = name;
    entities.PaymentStatus.EffectiveDate = effectiveDate;
    entities.PaymentStatus.DiscontinueDate = discontinueDate;
    entities.PaymentStatus.LastUpdateBy = lastUpdateBy;
    entities.PaymentStatus.LastUpdateTmst = lastUpdateTmst;
    entities.PaymentStatus.Description = description;
    entities.PaymentStatus.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

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

    private Common update;
    private DateWorkArea dateWorkArea;
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
    /// A value of Test2.
    /// </summary>
    [JsonPropertyName("test2")]
    public PaymentStatus Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public PaymentStatus Test1
    {
      get => test1 ??= new();
      set => test1 = value;
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

    private PaymentStatus test2;
    private PaymentStatus test1;
    private PaymentStatus paymentStatus;
  }
#endregion
}
