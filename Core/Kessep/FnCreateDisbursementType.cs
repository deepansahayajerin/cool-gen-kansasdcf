// Program: FN_CREATE_DISBURSEMENT_TYPE, ID: 371831997, model: 746.
// Short name: SWE00365
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_DISBURSEMENT_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDisbursementType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISBURSEMENT_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDisbursementType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDisbursementType.
  /// </summary>
  public FnCreateDisbursementType(IContext context, Import import, Export export)
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

    if (ReadDisbursementType())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      try
      {
        CreateDisbursementType();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_TYP_AE";

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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseFnAssignDisbursementTypeId()
  {
    var useImport = new FnAssignDisbursementTypeId.Import();
    var useExport = new FnAssignDisbursementTypeId.Export();

    Call(FnAssignDisbursementTypeId.Execute, useImport, useExport);

    return useExport.DisbursementType.SystemGeneratedIdentifier;
  }

  private void CreateDisbursementType()
  {
    var systemGeneratedIdentifier = UseFnAssignDisbursementTypeId();
    var code = import.DisbursementType.Code;
    var name = import.DisbursementType.Name;
    var currentArrearsInd = import.DisbursementType.CurrentArrearsInd ?? "";
    var recaptureInd = import.DisbursementType.RecaptureInd ?? "";
    var cashNonCashInd = import.DisbursementType.CashNonCashInd ?? "";
    var effectiveDate = import.DisbursementType.EffectiveDate;
    var discontinueDate = local.DisbursementType.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var programCode = import.DisbursementType.ProgramCode;
    var description = import.DisbursementType.Description ?? "";

    entities.DisbursementType.Populated = false;
    Update("CreateDisbursementType",
      (db, command) =>
      {
        db.SetInt32(command, "disbTypeId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetNullableString(command, "currentArrearsIn", currentArrearsInd);
        db.SetNullableString(command, "recaptureInd", recaptureInd);
        db.SetNullableString(command, "cashNonCashInd", cashNonCashInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "programCode", programCode);
        db.SetNullableString(command, "recoveryType", "");
        db.SetNullableString(command, "description", description);
      });

    entities.DisbursementType.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementType.Code = code;
    entities.DisbursementType.Name = name;
    entities.DisbursementType.CurrentArrearsInd = currentArrearsInd;
    entities.DisbursementType.RecaptureInd = recaptureInd;
    entities.DisbursementType.CashNonCashInd = cashNonCashInd;
    entities.DisbursementType.EffectiveDate = effectiveDate;
    entities.DisbursementType.DiscontinueDate = discontinueDate;
    entities.DisbursementType.CreatedBy = createdBy;
    entities.DisbursementType.CreatedTimestamp = createdTimestamp;
    entities.DisbursementType.ProgramCode = programCode;
    entities.DisbursementType.Description = description;
    entities.DisbursementType.Populated = true;
  }

  private bool ReadDisbursementType()
  {
    entities.Existing.Populated = false;

    return Read("ReadDisbursementType",
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
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.Code = db.GetString(reader, 1);
        entities.Existing.EffectiveDate = db.GetDate(reader, 2);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Existing.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of ForCompare.
    /// </summary>
    [JsonPropertyName("forCompare")]
    public ExpireEffectiveDateAttributes ForCompare
    {
      get => forCompare ??= new();
      set => forCompare = value;
    }

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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public DisbursementType Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    private DisbursementType existing;
    private DisbursementType disbursementType;
  }
#endregion
}
