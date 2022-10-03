// Program: LE_CAB_UPDATE_LEG_ACT_PERS_REC, ID: 371999619, model: 746.
// Short name: SWE02133
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_UPDATE_LEG_ACT_PERS_REC.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action updates the given LEGAL ACTION PERSON record.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabUpdateLegActPersRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_UPDATE_LEG_ACT_PERS_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabUpdateLegActPersRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabUpdateLegActPersRec.
  /// </summary>
  public LeCabUpdateLegActPersRec(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 103097	govind		Initial code
    // ---------------------------------------------
    if (!ReadLegalActionPerson())
    {
      ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

      return;
    }

    try
    {
      UpdateLegalActionPerson();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_LEGAL_ACTION_PERSON_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", import.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionPerson.Role = db.GetString(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 4);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 7);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private void UpdateLegalActionPerson()
  {
    var effectiveDate = import.LegalActionPerson.EffectiveDate;
    var role = import.LegalActionPerson.Role;
    var endDate = import.LegalActionPerson.EndDate;
    var endReason = import.LegalActionPerson.EndReason ?? "";
    var accountType = import.LegalActionPerson.AccountType ?? "";
    var arrearsAmount =
      import.LegalActionPerson.ArrearsAmount.GetValueOrDefault();
    var currentAmount =
      import.LegalActionPerson.CurrentAmount.GetValueOrDefault();
    var judgementAmount =
      import.LegalActionPerson.JudgementAmount.GetValueOrDefault();

    entities.LegalActionPerson.Populated = false;
    Update("UpdateLegalActionPerson",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetString(command, "role", role);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "endReason", endReason);
        db.SetNullableString(command, "accountType", accountType);
        db.SetNullableDecimal(command, "arrearsAmount", arrearsAmount);
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "judgementAmount", judgementAmount);
        db.
          SetInt32(command, "laPersonId", entities.LegalActionPerson.Identifier);
          
      });

    entities.LegalActionPerson.EffectiveDate = effectiveDate;
    entities.LegalActionPerson.Role = role;
    entities.LegalActionPerson.EndDate = endDate;
    entities.LegalActionPerson.EndReason = endReason;
    entities.LegalActionPerson.AccountType = accountType;
    entities.LegalActionPerson.ArrearsAmount = arrearsAmount;
    entities.LegalActionPerson.CurrentAmount = currentAmount;
    entities.LegalActionPerson.JudgementAmount = judgementAmount;
    entities.LegalActionPerson.Populated = true;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalActionPerson legalActionPerson;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalActionPerson legalActionPerson;
  }
#endregion
}
