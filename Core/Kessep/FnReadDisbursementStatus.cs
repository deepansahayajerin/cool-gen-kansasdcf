// Program: FN_READ_DISBURSEMENT_STATUS, ID: 371830096, model: 746.
// Short name: SWE00559
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DISBURSEMENT_STATUS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process results in reading a disbursement status row.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDisbursementStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DISBURSEMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDisbursementStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDisbursementStatus.
  /// </summary>
  public FnReadDisbursementStatus(IContext context, Import import, Export export)
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
    MoveDisbursementStatus(import.DisbursementStatus, export.DisbursementStatus);
      

    if (AsChar(import.Flag.Flag) == 'Y')
    {
      if (ReadDisbursementStatus1())
      {
        export.DisbursementStatus.Assign(entities.DisbursementStatus);
      }
      else
      {
        ExitState = "FN0000_DISB_STAT_NF";
      }
    }
    else
    {
      if (ReadDisbursementStatus2())
      {
        export.DisbursementStatus.Assign(entities.DisbursementStatus);

        return;
      }

      ExitState = "FN0000_DISB_STAT_NF";
    }
  }

  private static void MoveDisbursementStatus(DisbursementStatus source,
    DisbursementStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private bool ReadDisbursementStatus1()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementStatus.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Name = db.GetString(reader, 2);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementStatus2()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus2",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Name = db.GetString(reader, 2);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatus.Populated = true;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private Common flag;
    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus disbursementStatus;
  }
#endregion
}
