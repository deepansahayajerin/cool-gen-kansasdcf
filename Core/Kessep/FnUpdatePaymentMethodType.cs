// Program: FN_UPDATE_PAYMENT_METHOD_TYPE, ID: 371828232, model: 746.
// Short name: SWE00669
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_PAYMENT_METHOD_TYPE.
/// </summary>
[Serializable]
public partial class FnUpdatePaymentMethodType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_PAYMENT_METHOD_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdatePaymentMethodType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdatePaymentMethodType.
  /// </summary>
  public FnUpdatePaymentMethodType(IContext context, Import import,
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
    local.Update.Flag = "";
    export.PaymentMethodType.Assign(import.PaymentMethodType);

    // ***** EDIT AREA *****
    // If discontinue date is blank, then default it to max date
    if (Equal(import.PaymentMethodType.DiscontinueDate, null))
    {
      local.PaymentMethodType.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.PaymentMethodType.DiscontinueDate =
        import.PaymentMethodType.DiscontinueDate;
    }

    // ---------------------
    // Check for an existing record before you can update
    // ---------------------
    if (ReadPaymentMethodType1())
    {
      local.Update.Flag = "Y";
    }

    ReadPaymentMethodType3();
    ReadPaymentMethodType4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.PaymentMethodType.DiscontinueDate) || !
      Lt(import.PaymentMethodType.EffectiveDate, entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(import.PaymentMethodType.DiscontinueDate,
      entities.Test2.DiscontinueDate) && !
      Lt(entities.Test1.EffectiveDate, import.PaymentMethodType.EffectiveDate) ||
      !
      Lt(import.PaymentMethodType.DiscontinueDate,
      entities.Test2.DiscontinueDate) && import
      .PaymentMethodType.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier || !
      Lt(entities.Test1.EffectiveDate, import.PaymentMethodType.EffectiveDate) &&
      import.PaymentMethodType.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadPaymentMethodType5())
      {
        if (Equal(import.PaymentMethodType.DiscontinueDate,
          entities.PaymentMethodType.DiscontinueDate) || Equal
          (import.PaymentMethodType.EffectiveDate,
          entities.PaymentMethodType.EffectiveDate) || Equal
          (import.PaymentMethodType.DiscontinueDate,
          entities.PaymentMethodType.EffectiveDate) || Equal
          (import.PaymentMethodType.EffectiveDate,
          entities.PaymentMethodType.DiscontinueDate) || Lt
          (entities.PaymentMethodType.EffectiveDate,
          import.PaymentMethodType.EffectiveDate) && Lt
          (import.PaymentMethodType.EffectiveDate,
          entities.PaymentMethodType.DiscontinueDate) || Lt
          (entities.PaymentMethodType.EffectiveDate,
          import.PaymentMethodType.DiscontinueDate) && Lt
          (import.PaymentMethodType.DiscontinueDate,
          entities.PaymentMethodType.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }
        else
        {
          continue;
        }
      }
    }

    if (AsChar(local.Update.Flag) == 'Y')
    {
      // *****************************
      //       Main Line Areas
      // *****************************
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

      if (ReadPaymentMethodType2())
      {
        try
        {
          UpdatePaymentMethodType();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PYMNT_MTHD_TYPE_NU";

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
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    useImport.TypeStatusAudit.Assign(export.TypeStatusAudit);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadPaymentMethodType1()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.PaymentMethodType.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
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
        entities.PaymentMethodType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.PaymentMethodType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 9);
        entities.PaymentMethodType.Populated = true;
      });
  }

  private bool ReadPaymentMethodType2()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymntMethTypId",
          import.PaymentMethodType.SystemGeneratedIdentifier);
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
        entities.PaymentMethodType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.PaymentMethodType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 9);
        entities.PaymentMethodType.Populated = true;
      });
  }

  private bool ReadPaymentMethodType3()
  {
    entities.Test1.Populated = false;

    return Read("ReadPaymentMethodType3",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
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
        entities.Test1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Test1.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Test1.Description = db.GetNullableString(reader, 9);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadPaymentMethodType4()
  {
    entities.Test2.Populated = false;

    return Read("ReadPaymentMethodType4",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
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
        entities.Test2.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Test2.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Test2.Description = db.GetNullableString(reader, 9);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentMethodType5()
  {
    entities.PaymentMethodType.Populated = false;

    return ReadEach("ReadPaymentMethodType5",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
        db.SetInt32(
          command, "paymntMethTypId",
          import.PaymentMethodType.SystemGeneratedIdentifier);
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
        entities.PaymentMethodType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.PaymentMethodType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 9);
        entities.PaymentMethodType.Populated = true;

        return true;
      });
  }

  private void UpdatePaymentMethodType()
  {
    var code = import.PaymentMethodType.Code;
    var name = import.PaymentMethodType.Name;
    var effectiveDate = import.PaymentMethodType.EffectiveDate;
    var discontinueDate = import.PaymentMethodType.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var description = import.PaymentMethodType.Description ?? "";

    entities.PaymentMethodType.Populated = false;
    Update("UpdatePaymentMethodType",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "paymntMethTypId",
          entities.PaymentMethodType.SystemGeneratedIdentifier);
      });

    entities.PaymentMethodType.Code = code;
    entities.PaymentMethodType.Name = name;
    entities.PaymentMethodType.EffectiveDate = effectiveDate;
    entities.PaymentMethodType.DiscontinueDate = discontinueDate;
    entities.PaymentMethodType.LastUpdatedBy = lastUpdatedBy;
    entities.PaymentMethodType.LastUpdatedTmst = lastUpdatedTmst;
    entities.PaymentMethodType.Description = description;
    entities.PaymentMethodType.Populated = true;
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

    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private PaymentMethodType paymentMethodType;
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
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private Common update;
    private PaymentMethodType paymentMethodType;
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
    public PaymentMethodType Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public PaymentMethodType Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType test2;
    private PaymentMethodType test1;
    private PaymentMethodType paymentMethodType;
  }
#endregion
}
