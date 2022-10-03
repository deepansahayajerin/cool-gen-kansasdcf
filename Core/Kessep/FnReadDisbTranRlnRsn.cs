// Program: FN_READ_DISB_TRAN_RLN_RSN, ID: 371834965, model: 746.
// Short name: SWE00557
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DISB_TRAN_RLN_RSN.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process results in reading a disbursement transaction relation reason.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDisbTranRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DISB_TRAN_RLN_RSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDisbTranRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDisbTranRlnRsn.
  /// </summary>
  public FnReadDisbTranRlnRsn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (AsChar(import.Flag.Flag) == 'Y')
    {
      if (ReadDisbursementTranRlnRsn1())
      {
        export.DisbursementTranRlnRsn.Assign(entities.DisbursementTranRlnRsn);
      }
      else
      {
        ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF";
      }
    }
    else
    {
      if (ReadDisbursementTranRlnRsn2())
      {
        export.DisbursementTranRlnRsn.Assign(entities.DisbursementTranRlnRsn);

        return;
      }

      ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF";
    }
  }

  private bool ReadDisbursementTranRlnRsn1()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn1",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementTranRlnRsn.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTranRlnRsn.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn2()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn2",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementTranRlnRsn.Populated = true;
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private Common flag;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }
#endregion
}
