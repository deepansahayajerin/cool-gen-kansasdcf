// Program: FN_READ_DISBURSEMENT_TYPE, ID: 371831994, model: 746.
// Short name: SWE00560
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DISBURSEMENT_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process reads the information relevant to a specific disbursement type.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDisbursementType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DISBURSEMENT_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDisbursementType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDisbursementType.
  /// </summary>
  public FnReadDisbursementType(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************
    // A.Kinney  05/01/97	Changed Current_Date
    // ****************************************************
    local.Current.Date = Now().Date;

    if (AsChar(import.Flag.Flag) == 'Y')
    {
      if (ReadDisbursementType1())
      {
        export.DisbursementType.Assign(entities.DisbursementType);
      }
      else
      {
        ExitState = "FN0000_DISB_TYP_NF";
      }
    }
    else
    {
      if (ReadDisbursementType2())
      {
        export.DisbursementType.Assign(entities.DisbursementType);

        return;
      }

      ExitState = "FN0000_DISB_TYP_NF";
    }
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
        entities.DisbursementType.ProgramCode = db.GetString(reader, 8);
        entities.DisbursementType.Description = db.GetNullableString(reader, 9);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadDisbursementType2()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementType.Code);
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
        entities.DisbursementType.ProgramCode = db.GetString(reader, 8);
        entities.DisbursementType.Description = db.GetNullableString(reader, 9);
        entities.DisbursementType.Populated = true;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
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

    private Common flag;
    private DisbursementType disbursementType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
