// Program: FN_UPDATE_DISBURSEMENT_TYPE, ID: 371831996, model: 746.
// Short name: SWE00649
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
/// A program: FN_UPDATE_DISBURSEMENT_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateDisbursementType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_DISBURSEMENT_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateDisbursementType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateDisbursementType.
  /// </summary>
  public FnUpdateDisbursementType(IContext context, Import import, Export export)
    :
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

    // ***** EDIT AREA *****
    local.ForCompare.EffectiveDate = import.DisbursementType.EffectiveDate;
    local.ForCompare.ExpirationDate = import.DisbursementType.DiscontinueDate;
    UseCabCompareEffecAndDiscDates();

    if (AsChar(local.ForCompare.EffectiveDateIsZero) == 'Y')
    {
      ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

      return;
    }
    else if (AsChar(local.ForCompare.ExpirationDateIsZero) == 'N' && AsChar
      (local.ForCompare.ExpirationDateLtEffectiveDat) == 'Y')
    {
      ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";

      return;
    }
    else if (AsChar(local.ForCompare.ExpirationDateIsLtCurrent) == 'Y')
    {
      ExitState = "EXPIRATION_DATE_PRIOR_TO_CURRENT";

      return;
    }

    if (Equal(import.DisbursementType.DiscontinueDate, null))
    {
      local.DisbursementType.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.DisbursementType.DiscontinueDate =
        import.DisbursementType.DiscontinueDate;
    }

    if (ReadDisbursementType1())
    {
      local.Update.Flag = "Y";
    }

    ReadDisbursementType3();
    ReadDisbursementType4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.DisbursementType.DiscontinueDate) || !
      Lt(import.DisbursementType.EffectiveDate, entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate, import.DisbursementType.EffectiveDate) &&
      !
      Lt(import.DisbursementType.DiscontinueDate, entities.Test2.DiscontinueDate)
      || !
      Lt(import.DisbursementType.DiscontinueDate, entities.Test2.DiscontinueDate)
      && import.DisbursementType.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier || !
      Lt(entities.Test1.EffectiveDate, import.DisbursementType.EffectiveDate) &&
      import.DisbursementType.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadDisbursementType5())
      {
        if (Lt(entities.DisbursementType.EffectiveDate,
          import.DisbursementType.DiscontinueDate) && Lt
          (import.DisbursementType.DiscontinueDate,
          entities.DisbursementType.DiscontinueDate) || Lt
          (entities.DisbursementType.EffectiveDate,
          import.DisbursementType.EffectiveDate) && Lt
          (import.DisbursementType.EffectiveDate,
          entities.DisbursementType.DiscontinueDate) || Equal
          (entities.DisbursementType.DiscontinueDate,
          import.DisbursementType.EffectiveDate) || Equal
          (entities.DisbursementType.EffectiveDate,
          import.DisbursementType.DiscontinueDate) || Equal
          (entities.DisbursementType.DiscontinueDate,
          import.DisbursementType.DiscontinueDate) || Equal
          (entities.DisbursementType.EffectiveDate,
          import.DisbursementType.EffectiveDate))
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
      export.TypeStatusAudit.TableName = "DISBURSEMENT_TYPE";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementType.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementType.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementType.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementType.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementType.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy = entities.DisbursementType.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementType.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementType.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementType.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (ReadDisbursementType2())
      {
        try
        {
          UpdateDisbursementType();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_TYP_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TYP_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "FN0000_DISB_TYP_NF";
      }
    }
  }

  private static void MoveExpireEffectiveDateAttributes1(
    ExpireEffectiveDateAttributes source, ExpireEffectiveDateAttributes target)
  {
    target.ExpirationDate = source.ExpirationDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveExpireEffectiveDateAttributes2(
    ExpireEffectiveDateAttributes source, ExpireEffectiveDateAttributes target)
  {
    target.EffectiveDateIsZero = source.EffectiveDateIsZero;
    target.ExpirationDateIsZero = source.ExpirationDateIsZero;
    target.EffectiveDateIsLtCurrent = source.EffectiveDateIsLtCurrent;
    target.ExpirationDateIsLtCurrent = source.ExpirationDateIsLtCurrent;
    target.ExpirationDateLtEffectiveDat = source.ExpirationDateLtEffectiveDat;
  }

  private void UseCabCompareEffecAndDiscDates()
  {
    var useImport = new CabCompareEffecAndDiscDates.Import();
    var useExport = new CabCompareEffecAndDiscDates.Export();

    MoveExpireEffectiveDateAttributes1(local.ForCompare,
      useImport.ExpireEffectiveDateAttributes);

    Call(CabCompareEffecAndDiscDates.Execute, useImport, useExport);

    MoveExpireEffectiveDateAttributes2(useExport.ExpireEffectiveDateAttributes,
      local.ForCompare);
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

  private bool ReadDisbursementType1()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementType.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.DisbursementType.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Name = db.GetString(reader, 2);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 3);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 4);
        entities.DisbursementType.CashNonCashInd =
          db.GetNullableString(reader, 5);
        entities.DisbursementType.EffectiveDate = db.GetDate(reader, 6);
        entities.DisbursementType.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementType.CreatedBy = db.GetString(reader, 8);
        entities.DisbursementType.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.DisbursementType.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.DisbursementType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 12);
        entities.DisbursementType.Description =
          db.GetNullableString(reader, 13);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadDisbursementType2()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          import.DisbursementType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Name = db.GetString(reader, 2);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 3);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 4);
        entities.DisbursementType.CashNonCashInd =
          db.GetNullableString(reader, 5);
        entities.DisbursementType.EffectiveDate = db.GetDate(reader, 6);
        entities.DisbursementType.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementType.CreatedBy = db.GetString(reader, 8);
        entities.DisbursementType.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.DisbursementType.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.DisbursementType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 12);
        entities.DisbursementType.Description =
          db.GetNullableString(reader, 13);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadDisbursementType3()
  {
    entities.Test1.Populated = false;

    return Read("ReadDisbursementType3",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementType.Code);
      },
      (db, reader) =>
      {
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test1.Code = db.GetString(reader, 1);
        entities.Test1.Name = db.GetString(reader, 2);
        entities.Test1.CurrentArrearsInd = db.GetNullableString(reader, 3);
        entities.Test1.RecaptureInd = db.GetNullableString(reader, 4);
        entities.Test1.CashNonCashInd = db.GetNullableString(reader, 5);
        entities.Test1.EffectiveDate = db.GetDate(reader, 6);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Test1.CreatedBy = db.GetString(reader, 8);
        entities.Test1.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.Test1.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Test1.LastUpdatedTmst = db.GetNullableDateTime(reader, 11);
        entities.Test1.ProgramCode = db.GetString(reader, 12);
        entities.Test1.RecoveryType = db.GetNullableString(reader, 13);
        entities.Test1.Description = db.GetNullableString(reader, 14);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadDisbursementType4()
  {
    entities.Test2.Populated = false;

    return Read("ReadDisbursementType4",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementType.Code);
      },
      (db, reader) =>
      {
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test2.Code = db.GetString(reader, 1);
        entities.Test2.Name = db.GetString(reader, 2);
        entities.Test2.CurrentArrearsInd = db.GetNullableString(reader, 3);
        entities.Test2.RecaptureInd = db.GetNullableString(reader, 4);
        entities.Test2.CashNonCashInd = db.GetNullableString(reader, 5);
        entities.Test2.EffectiveDate = db.GetDate(reader, 6);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Test2.CreatedBy = db.GetString(reader, 8);
        entities.Test2.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.Test2.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Test2.LastUpdatedTmst = db.GetNullableDateTime(reader, 11);
        entities.Test2.ProgramCode = db.GetString(reader, 12);
        entities.Test2.RecoveryType = db.GetNullableString(reader, 13);
        entities.Test2.Description = db.GetNullableString(reader, 14);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementType5()
  {
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadDisbursementType5",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementType.Code);
        db.SetInt32(
          command, "disbTypeId",
          import.DisbursementType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Name = db.GetString(reader, 2);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 3);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 4);
        entities.DisbursementType.CashNonCashInd =
          db.GetNullableString(reader, 5);
        entities.DisbursementType.EffectiveDate = db.GetDate(reader, 6);
        entities.DisbursementType.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementType.CreatedBy = db.GetString(reader, 8);
        entities.DisbursementType.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.DisbursementType.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.DisbursementType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 12);
        entities.DisbursementType.Description =
          db.GetNullableString(reader, 13);
        entities.DisbursementType.Populated = true;

        return true;
      });
  }

  private void UpdateDisbursementType()
  {
    var code = import.DisbursementType.Code;
    var name = import.DisbursementType.Name;
    var currentArrearsInd = import.DisbursementType.CurrentArrearsInd ?? "";
    var recaptureInd = import.DisbursementType.RecaptureInd ?? "";
    var cashNonCashInd = import.DisbursementType.CashNonCashInd ?? "";
    var effectiveDate = import.DisbursementType.EffectiveDate;
    var discontinueDate = local.DisbursementType.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var programCode = import.DisbursementType.ProgramCode;
    var description = import.DisbursementType.Description ?? "";

    entities.DisbursementType.Populated = false;
    Update("UpdateDisbursementType",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetNullableString(command, "currentArrearsIn", currentArrearsInd);
        db.SetNullableString(command, "recaptureInd", recaptureInd);
        db.SetNullableString(command, "cashNonCashInd", cashNonCashInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "programCode", programCode);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "disbTypeId",
          entities.DisbursementType.SystemGeneratedIdentifier);
      });

    entities.DisbursementType.Code = code;
    entities.DisbursementType.Name = name;
    entities.DisbursementType.CurrentArrearsInd = currentArrearsInd;
    entities.DisbursementType.RecaptureInd = recaptureInd;
    entities.DisbursementType.CashNonCashInd = cashNonCashInd;
    entities.DisbursementType.EffectiveDate = effectiveDate;
    entities.DisbursementType.DiscontinueDate = discontinueDate;
    entities.DisbursementType.LastUpdatedBy = lastUpdatedBy;
    entities.DisbursementType.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbursementType.ProgramCode = programCode;
    entities.DisbursementType.Description = description;
    entities.DisbursementType.Populated = true;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType disbursementType;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of ForCompare.
    /// </summary>
    [JsonPropertyName("forCompare")]
    public ExpireEffectiveDateAttributes ForCompare
    {
      get => forCompare ??= new();
      set => forCompare = value;
    }

    private Common update;
    private DisbursementType disbursementType;
    private ExpireEffectiveDateAttributes forCompare;
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
    public DisbursementType Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public DisbursementType Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType test2;
    private DisbursementType test1;
    private DisbursementType disbursementType;
  }
#endregion
}
