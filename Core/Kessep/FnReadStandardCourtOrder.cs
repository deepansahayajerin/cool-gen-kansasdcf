// Program: FN_READ_STANDARD_COURT_ORDER, ID: 372063206, model: 746.
// Short name: SWE00584
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_STANDARD_COURT_ORDER.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will read for a legal action using the standard court 
/// order number.
/// The legal action may or may not be the court order.  If you specifically 
/// want a court order, supply the court order value on input.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadStandardCourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_STANDARD_COURT_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadStandardCourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadStandardCourtOrder.
  /// </summary>
  public FnReadStandardCourtOrder(IContext context, Import import, Export export)
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
    // *****  Still unsure about exactly how this should work given that 
    // multiple legal actions of both the same classification and different
    // classifications will have the same standard number.
    // For my (Leslie's) intent on 6/22/95 the fact that any legal action exists
    // for the standard number is adequate.
    if (AsChar(import.LegalAction.Classification) == 'J')
    {
      if (ReadLegalAction1())
      {
        ++export.ImportNumberOfReads.Count;
        export.LegalAction.Assign(entities.LegalAction);
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
      }
    }
    else if (ReadLegalAction2())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
        db.SetString(
          command, "classification", import.LegalAction.Classification);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.AttorneyApproval = db.GetNullableString(reader, 2);
        entities.LegalAction.ApprovalSentDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.PetitionerApproval =
          db.GetNullableString(reader, 4);
        entities.LegalAction.ApprovalReceivedDate =
          db.GetNullableDate(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.Type1 = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.ForeignOrderRegistrationDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.UresaSentDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.UresaAcknowledgedDate =
          db.GetNullableDate(reader, 12);
        entities.LegalAction.UifsaSentDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.UifsaAcknowledgedDate =
          db.GetNullableDate(reader, 14);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 15);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 16);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 17);
        entities.LegalAction.RespondingCounty =
          db.GetNullableString(reader, 18);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 19);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 20);
        entities.LegalAction.RefileDate = db.GetNullableDate(reader, 21);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 23);
        entities.LegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 24);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 25);
        entities.LegalAction.LongArmStatuteIndicator =
          db.GetNullableString(reader, 26);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 27);
        entities.LegalAction.CreatedBy = db.GetString(reader, 28);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 29);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 30);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 31);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 32);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.LastModificationReviewDate =
          db.GetNullableDate(reader, 1);
        entities.LegalAction.AttorneyApproval = db.GetNullableString(reader, 2);
        entities.LegalAction.ApprovalSentDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.PetitionerApproval =
          db.GetNullableString(reader, 4);
        entities.LegalAction.ApprovalReceivedDate =
          db.GetNullableDate(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.ActionTaken = db.GetString(reader, 7);
        entities.LegalAction.Type1 = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.ForeignOrderRegistrationDate =
          db.GetNullableDate(reader, 10);
        entities.LegalAction.UresaSentDate = db.GetNullableDate(reader, 11);
        entities.LegalAction.UresaAcknowledgedDate =
          db.GetNullableDate(reader, 12);
        entities.LegalAction.UifsaSentDate = db.GetNullableDate(reader, 13);
        entities.LegalAction.UifsaAcknowledgedDate =
          db.GetNullableDate(reader, 14);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 15);
        entities.LegalAction.InitiatingCounty =
          db.GetNullableString(reader, 16);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 17);
        entities.LegalAction.RespondingCounty =
          db.GetNullableString(reader, 18);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 19);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 20);
        entities.LegalAction.RefileDate = db.GetNullableDate(reader, 21);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 22);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 23);
        entities.LegalAction.DismissedWithoutPrejudiceInd =
          db.GetNullableString(reader, 24);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 25);
        entities.LegalAction.LongArmStatuteIndicator =
          db.GetNullableString(reader, 26);
        entities.LegalAction.DismissalCode = db.GetNullableString(reader, 27);
        entities.LegalAction.CreatedBy = db.GetString(reader, 28);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 29);
        entities.LegalAction.LastUpdatedBy = db.GetNullableString(reader, 30);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 31);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 32);
        entities.LegalAction.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private Common importNumberOfReads;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }
#endregion
}
