// Program: FN_UPDATE_OBLIGEE_TOTALS, ID: 372544588, model: 746.
// Short name: SWE00667
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_OBLIGEE_TOTALS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will update the totals for an Obligee.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateObligeeTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_OBLIGEE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateObligeeTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateObligeeTotals.
  /// </summary>
  public FnUpdateObligeeTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // 10-10-00  PR 98039 - Removed unused attributes.
    // ****************************************************************
    if (ReadObligee())
    {
      try
      {
        UpdateObligee();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGEE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGEE_PV";

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
      ExitState = "FN0000_OBLIGEE_NF";
    }
  }

  private bool ReadObligee()
  {
    entities.Obligee.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Obligee.LastUpdatedTmst = db.GetNullableDateTime(reader, 3);
        entities.Obligee.TotNonAdcDisbAmt = db.GetNullableDecimal(reader, 4);
        entities.Obligee.TotAdcDisbAmt = db.GetNullableDecimal(reader, 5);
        entities.Obligee.TotMedDisbAmt = db.GetNullableDecimal(reader, 6);
        entities.Obligee.TotPassthruDisbAmt = db.GetNullableDecimal(reader, 7);
        entities.Obligee.TotNonAdcUndisbAmt = db.GetNullableDecimal(reader, 8);
        entities.Obligee.TotAdcUndisbAmt = db.GetNullableDecimal(reader, 9);
        entities.Obligee.LastDisbAmt = db.GetNullableDecimal(reader, 10);
        entities.Obligee.LastDisbDt = db.GetNullableDate(reader, 11);
        entities.Obligee.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);
      });
  }

  private void UpdateObligee()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var totNonAdcDisbAmt =
      import.Obligee.TotNonAdcDisbAmt.GetValueOrDefault() +
      entities.Obligee.TotNonAdcDisbAmt.GetValueOrDefault();
    var totAdcDisbAmt =
      import.Obligee.TotAdcDisbAmt.GetValueOrDefault() +
      entities.Obligee.TotAdcDisbAmt.GetValueOrDefault();
    var totMedDisbAmt =
      import.Obligee.TotMedDisbAmt.GetValueOrDefault() +
      entities.Obligee.TotMedDisbAmt.GetValueOrDefault();
    var totPassthruDisbAmt =
      import.Obligee.TotPassthruDisbAmt.GetValueOrDefault() +
      entities.Obligee.TotPassthruDisbAmt.GetValueOrDefault();
    var totNonAdcUndisbAmt =
      import.Obligee.TotNonAdcUndisbAmt.GetValueOrDefault() +
      entities.Obligee.TotNonAdcUndisbAmt.GetValueOrDefault();
    var totAdcUndisbAmt =
      import.Obligee.TotAdcUndisbAmt.GetValueOrDefault() +
      entities.Obligee.TotAdcUndisbAmt.GetValueOrDefault();
    var lastDisbAmt = import.Obligee.LastDisbAmt.GetValueOrDefault();
    var lastDisbDt = import.Obligee.LastDisbDt;

    entities.Obligee.Populated = false;
    Update("UpdateObligee",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "totNonAdcDisbA", totNonAdcDisbAmt);
        db.SetNullableDecimal(command, "totAdcDisbAmt", totAdcDisbAmt);
        db.SetNullableDecimal(command, "totMedDisbAmt", totMedDisbAmt);
        db.SetNullableDecimal(command, "totPassthruDisb", totPassthruDisbAmt);
        db.SetNullableDecimal(command, "totNonAdcUndisb", totNonAdcUndisbAmt);
        db.SetNullableDecimal(command, "totAdcUndisbAmt", totAdcUndisbAmt);
        db.SetNullableDecimal(command, "lastDisbAmt", lastDisbAmt);
        db.SetNullableDate(command, "lastDisbDt", lastDisbDt);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
        db.SetString(command, "type", entities.Obligee.Type1);
      });

    entities.Obligee.LastUpdatedBy = lastUpdatedBy;
    entities.Obligee.LastUpdatedTmst = lastUpdatedTmst;
    entities.Obligee.TotNonAdcDisbAmt = totNonAdcDisbAmt;
    entities.Obligee.TotAdcDisbAmt = totAdcDisbAmt;
    entities.Obligee.TotMedDisbAmt = totMedDisbAmt;
    entities.Obligee.TotPassthruDisbAmt = totPassthruDisbAmt;
    entities.Obligee.TotNonAdcUndisbAmt = totNonAdcUndisbAmt;
    entities.Obligee.TotAdcUndisbAmt = totAdcUndisbAmt;
    entities.Obligee.LastDisbAmt = lastDisbAmt;
    entities.Obligee.LastDisbDt = lastDisbDt;
    entities.Obligee.Populated = true;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount obligee;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount obligee;
    private CsePerson csePerson;
  }
#endregion
}
