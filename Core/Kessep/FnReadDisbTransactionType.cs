// Program: FN_READ_DISB_TRANSACTION_TYPE, ID: 371837539, model: 746.
// Short name: SWE00558
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DISB_TRANSACTION_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will read the disbursement transaction type information for a 
/// particular disbursement transaction type.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDisbTransactionType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DISB_TRANSACTION_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDisbTransactionType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDisbTransactionType.
  /// </summary>
  public FnReadDisbTransactionType(IContext context, Import import,
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
    MoveDisbursementTransactionType(import.DisbursementTransactionType,
      export.DisbursementTransactionType);

    if (AsChar(import.Flag.Flag) == 'Y')
    {
      if (ReadDisbursementTransactionType1())
      {
        export.DisbursementTransactionType.Assign(
          entities.DisbursementTransactionType);
      }
      else
      {
        ExitState = "FN0000_DISB_TRANS_TYP_NF";
      }
    }
    else
    {
      if (ReadDisbursementTransactionType2())
      {
        export.DisbursementTransactionType.Assign(
          entities.DisbursementTransactionType);

        return;
      }

      ExitState = "FN0000_DISB_TRANS_TYP_NF";
    }
  }

  private static void MoveDisbursementTransactionType(
    DisbursementTransactionType source, DisbursementTransactionType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private bool ReadDisbursementTransactionType1()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementTransactionType.EffectiveDate.GetValueOrDefault());
          
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
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransactionType.Populated = true;
      });
  }

  private bool ReadDisbursementTransactionType2()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
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
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransactionType.Populated = true;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
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

    private Common flag;
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
