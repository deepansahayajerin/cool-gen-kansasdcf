// Program: RESOLVE_ADMINISTRATIVE_APPEAL, ID: 372605738, model: 746.
// Short name: SWE01073
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: RESOLVE_ADMINISTRATIVE_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates ADMINISTRATIVE APPEAL.
/// </para>
/// </summary>
[Serializable]
public partial class ResolveAdministrativeAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RESOLVE_ADMINISTRATIVE_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ResolveAdministrativeAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ResolveAdministrativeAppeal.
  /// </summary>
  public ResolveAdministrativeAppeal(IContext context, Import import,
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
    local.Current.Timestamp = Now();

    // -------------------------------------------------------------
    // Read the Appeal to be updated.
    // -------------------------------------------------------------
    if (ReadAdministrativeAppeal1())
    {
      if (AsChar(import.AdministrativeAppeal.Type1) != AsChar
        (entities.AdministrativeAppeal.Type1) || !
        Equal(import.AdministrativeAppeal.RequestDate,
        entities.AdministrativeAppeal.RequestDate))
      {
        // -------------------------------------------------------------
        // Key business fields have changed.
        // Need to test for a potential duplicate.
        // -------------------------------------------------------------
        if (ReadAdministrativeAppeal2())
        {
          // -------------------------------------------------------------
          // A duplicate appeal was found.  Disallow the change.
          // -------------------------------------------------------------
          ExitState = "LE0000_APPEAL_AE_W_TYP_AND_REQ_D";

          return;
        }
        else
        {
          // Requested change does not duplicate another existing
          // appeal.  OK to continue.
        }
      }

      try
      {
        UpdateAdministrativeAppeal();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NU";

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
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
    }
  }

  private bool ReadAdministrativeAppeal1()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.RequestDate = db.GetDate(reader, 3);
        entities.AdministrativeAppeal.ReceivedDate = db.GetDate(reader, 4);
        entities.AdministrativeAppeal.Respondent = db.GetString(reader, 5);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.AppellantRelationship =
          db.GetNullableString(reader, 10);
        entities.AdministrativeAppeal.Date = db.GetNullableDate(reader, 11);
        entities.AdministrativeAppeal.AdminOrderDate =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeAppeal.WithdrawDate =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeAppeal.RequestFurtherReviewDate =
          db.GetNullableDate(reader, 14);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.AdministrativeAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 16);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 18);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 22);
        entities.AdministrativeAppeal.AdminReviewState =
          db.GetString(reader, 23);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadAdministrativeAppeal2()
  {
    entities.Duplicate.Populated = false;

    return Read("ReadAdministrativeAppeal2",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAppeal.Type1);
        db.SetDate(
          command, "requestDt",
          import.AdministrativeAppeal.RequestDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Duplicate.Identifier = db.GetInt32(reader, 0);
        entities.Duplicate.Number = db.GetNullableString(reader, 1);
        entities.Duplicate.Type1 = db.GetString(reader, 2);
        entities.Duplicate.RequestDate = db.GetDate(reader, 3);
        entities.Duplicate.Populated = true;
      });
  }

  private void UpdateAdministrativeAppeal()
  {
    var number = import.AdministrativeAppeal.Number ?? "";
    var requestDate = import.AdministrativeAppeal.RequestDate;
    var receivedDate = import.AdministrativeAppeal.ReceivedDate;
    var respondent = import.AdministrativeAppeal.Respondent;
    var appellantLastName = import.AdministrativeAppeal.AppellantLastName ?? "";
    var appellantFirstName = import.AdministrativeAppeal.AppellantFirstName ?? ""
      ;
    var appellantMiddleInitial =
      import.AdministrativeAppeal.AppellantMiddleInitial ?? "";
    var appellantSuffix = import.AdministrativeAppeal.AppellantSuffix ?? "";
    var appellantRelationship =
      import.AdministrativeAppeal.AppellantRelationship ?? "";
    var date = import.AdministrativeAppeal.Date;
    var adminOrderDate = import.AdministrativeAppeal.AdminOrderDate;
    var withdrawDate = import.AdministrativeAppeal.WithdrawDate;
    var requestFurtherReviewDate =
      import.AdministrativeAppeal.RequestFurtherReviewDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = local.Current.Timestamp;
    var judicialReviewInd = import.AdministrativeAppeal.JudicialReviewInd ?? "";
    var reason = import.AdministrativeAppeal.Reason;
    var outcome = import.AdministrativeAppeal.Outcome ?? "";
    var reviewOutcome = import.AdministrativeAppeal.ReviewOutcome ?? "";
    var withdrawReason = import.AdministrativeAppeal.WithdrawReason ?? "";
    var requestFurtherReview =
      import.AdministrativeAppeal.RequestFurtherReview ?? "";
    var adminReviewState = import.AdministrativeAppeal.AdminReviewState;

    entities.AdministrativeAppeal.Populated = false;
    Update("UpdateAdministrativeAppeal",
      (db, command) =>
      {
        db.SetNullableString(command, "adminAppealNo", number);
        db.SetDate(command, "requestDt", requestDate);
        db.SetDate(command, "receivedDt", receivedDate);
        db.SetString(command, "respondent", respondent);
        db.SetNullableString(command, "appellantLastNm", appellantLastName);
        db.SetNullableString(command, "appellantFirstNm", appellantFirstName);
        db.SetNullableString(command, "appellantMi", appellantMiddleInitial);
        db.SetNullableString(command, "appellantSuffix", appellantSuffix);
        db.SetNullableString(command, "appellantRel", appellantRelationship);
        db.SetNullableDate(command, "adminAppealDt", date);
        db.SetNullableDate(command, "adminOrderDt", adminOrderDate);
        db.SetNullableDate(command, "withdrawDt", withdrawDate);
        db.
          SetNullableDate(command, "reqFurtherRevDt", requestFurtherReviewDate);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "judReviewInd", judicialReviewInd);
        db.SetString(command, "reason", reason);
        db.SetNullableString(command, "outcome", outcome);
        db.SetNullableString(command, "reviewOutcome", reviewOutcome);
        db.SetNullableString(command, "withdrawReason", withdrawReason);
        db.SetNullableString(command, "reqFurtherReview", requestFurtherReview);
        db.SetString(command, "adminReviewState", adminReviewState);
        db.SetInt32(
          command, "adminAppealId", entities.AdministrativeAppeal.Identifier);
      });

    entities.AdministrativeAppeal.Number = number;
    entities.AdministrativeAppeal.RequestDate = requestDate;
    entities.AdministrativeAppeal.ReceivedDate = receivedDate;
    entities.AdministrativeAppeal.Respondent = respondent;
    entities.AdministrativeAppeal.AppellantLastName = appellantLastName;
    entities.AdministrativeAppeal.AppellantFirstName = appellantFirstName;
    entities.AdministrativeAppeal.AppellantMiddleInitial =
      appellantMiddleInitial;
    entities.AdministrativeAppeal.AppellantSuffix = appellantSuffix;
    entities.AdministrativeAppeal.AppellantRelationship = appellantRelationship;
    entities.AdministrativeAppeal.Date = date;
    entities.AdministrativeAppeal.AdminOrderDate = adminOrderDate;
    entities.AdministrativeAppeal.WithdrawDate = withdrawDate;
    entities.AdministrativeAppeal.RequestFurtherReviewDate =
      requestFurtherReviewDate;
    entities.AdministrativeAppeal.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeAppeal.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.AdministrativeAppeal.JudicialReviewInd = judicialReviewInd;
    entities.AdministrativeAppeal.Reason = reason;
    entities.AdministrativeAppeal.Outcome = outcome;
    entities.AdministrativeAppeal.ReviewOutcome = reviewOutcome;
    entities.AdministrativeAppeal.WithdrawReason = withdrawReason;
    entities.AdministrativeAppeal.RequestFurtherReview = requestFurtherReview;
    entities.AdministrativeAppeal.AdminReviewState = adminReviewState;
    entities.AdministrativeAppeal.Populated = true;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdministrativeAppeal administrativeAppeal;
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
    /// A value of Duplicate.
    /// </summary>
    [JsonPropertyName("duplicate")]
    public AdministrativeAppeal Duplicate
    {
      get => duplicate ??= new();
      set => duplicate = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdministrativeAppeal duplicate;
    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}
