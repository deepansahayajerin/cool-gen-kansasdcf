// Program: FN_CREATE_DISBURSEMENT_TRAN_TYPE, ID: 371837536, model: 746.
// Short name: SWE00364
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_DISBURSEMENT_TRAN_TYPE.
/// </para>
/// <para>
/// RESP: FINCLMGNT
/// This PAD will create Disbursement Transaction Type.  If Discontinue date is 
/// zero or blank, then set it as maximum date.
/// An example of different tran type:
///   Collection
///   Disbursement
///   Recapture
///   Disbursement Fee
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDisbursementTranType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISBURSEMENT_TRAN_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDisbursementTranType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDisbursementTranType.
  /// </summary>
  public FnCreateDisbursementTranType(IContext context, Import import,
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
    export.DisbursementTransactionType.
      Assign(import.DisbursementTransactionType);

    // Effective date cannot be less than current date. Also noted that
    // the following IF EFFECTIVE DATE IS < CURRENT DATE statement must
    // follows the IF EFFECTIVE DATE IS BLANK... statement.
    // If Discontinue date is blank, then default it to max date
    // Validate the discontinue date, discontinue date can not be prior to 
    // effective date
    // ***************************
    //      Validate inputs
    // ***************************
    // ----------------------------------------------------------------
    // Naveen - 11/24/1998
    // Check for any existing record with the same date ranges to prevent adding
    // an existing record. If there are multiple records for the same code
    // value but their date ranges do not overlap, create another record for the
    // same code value
    // -----------------------------------------------------------------
    if (ReadDisbursementTransactionType())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      // ***** MAIN-LINE AREA *****
      try
      {
        CreateDisbursementTransactionType();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.DisbursementTransactionType.Assign(
              entities.DisbursementTransactionType);

            if (Equal(export.DisbursementTransactionType.DiscontinueDate,
              local.Maximum.Date))
            {
              export.DisbursementTransactionType.DiscontinueDate =
                local.Zero.Date;
            }

            ExitState = "FN0000_DISB_TRANS_TYP_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_TYP_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private int UseFnAssignDisbTranTypeId()
  {
    var useImport = new FnAssignDisbTranTypeId.Import();
    var useExport = new FnAssignDisbTranTypeId.Export();

    Call(FnAssignDisbTranTypeId.Execute, useImport, useExport);

    return useExport.DisbursementTransactionType.SystemGeneratedIdentifier;
  }

  private void CreateDisbursementTransactionType()
  {
    var systemGeneratedIdentifier = UseFnAssignDisbTranTypeId();
    var code = import.DisbursementTransactionType.Code;
    var name = import.DisbursementTransactionType.Name;
    var effectiveDate = import.DisbursementTransactionType.EffectiveDate;
    var discontinueDate = import.DisbursementTransactionType.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var description = import.DisbursementTransactionType.Description ?? "";

    entities.DisbursementTransactionType.Populated = false;
    Update("CreateDisbursementTransactionType",
      (db, command) =>
      {
        db.SetInt32(command, "disbTranTypeId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.DisbursementTransactionType.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransactionType.Code = code;
    entities.DisbursementTransactionType.Name = name;
    entities.DisbursementTransactionType.EffectiveDate = effectiveDate;
    entities.DisbursementTransactionType.DiscontinueDate = discontinueDate;
    entities.DisbursementTransactionType.CreatedBy = createdBy;
    entities.DisbursementTransactionType.CreatedTmst = createdTmst;
    entities.DisbursementTransactionType.Description = description;
    entities.DisbursementTransactionType.Populated = true;
  }

  private bool ReadDisbursementTransactionType()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
        db.SetDate(
          command, "effectiveDate1",
          import.DisbursementTransactionType.EffectiveDate.GetValueOrDefault());
          
        db.SetDate(
          command, "effectiveDate2",
          import.DisbursementTransactionType.DiscontinueDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.Name = db.GetString(reader, 2);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.Populated = true;
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
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkArea maximum;
    private DateWorkArea zero;
    private DisbursementTransactionType disbursementTransactionType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }
#endregion
}
