// Program: LE_CAB_READ_ADMIN_APPEAL, ID: 372605741, model: 746.
// Short name: SWE02446
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_READ_ADMIN_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block reads for a given Administrative Appeal.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabReadAdminAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_READ_ADMIN_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabReadAdminAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabReadAdminAppeal.
  /// </summary>
  public LeCabReadAdminAppeal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    MoveAdministrativeAppeal1(import.AdministrativeAppeal,
      export.AdministrativeAppeal);

    if (ReadCsePerson())
    {
      // -->  Continue processing
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (import.AdministrativeAppeal.Identifier > 0)
    {
      // -------------------------------------------------------
      // This will be true with call from AAPP when admin appeal
      // number is not known but admin appeal has been selected
      // from AAPS.
      // -------------------------------------------------------
      if (ReadAdministrativeAppeal1())
      {
        MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
          export.AdministrativeAppeal);
      }
      else
      {
        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

        return;
      }
    }
    else if (!IsEmpty(import.AdministrativeAppeal.Number))
    {
      ReadAdministrativeAppeal4();

      if (local.NoOfAdminAppealsFound.Count == 0)
      {
        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

        return;
      }
      else if (local.NoOfAdminAppealsFound.Count > 1)
      {
        ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

        return;
      }

      if (ReadAdministrativeAppeal2())
      {
        MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
          export.AdministrativeAppeal);
      }
      else
      {
        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

        return;
      }
    }
    else
    {
      ReadAdministrativeAppeal5();

      if (local.NoOfAdminAppealsFound.Count == 0)
      {
        ExitState = "LE0000_NO_ADMIN_APPLS_4_PRSN";

        return;
      }
      else if (local.NoOfAdminAppealsFound.Count > 1)
      {
        ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

        return;
      }

      if (ReadAdministrativeAppeal3())
      {
        MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
          export.AdministrativeAppeal);
      }
      else
      {
        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

        return;
      }
    }

    if (ReadAdministrativeActionObligationAdministrativeAction())
    {
      MoveAdministrativeAction(entities.AdministrativeAction,
        export.AdministrativeAction);
      export.ActionTaken.Date =
        entities.ObligationAdministrativeAction.TakenDate;
    }
    else if (ReadAdministrativeActCertification())
    {
      if (ReadAdministrativeAction())
      {
        export.ActionTaken.Date =
          entities.AdministrativeActCertification.TakenDate;
        MoveAdministrativeAction(entities.AdministrativeAction,
          export.AdministrativeAction);
      }
      else
      {
        // -------------------------------------------------------
        // An Appeal may or may not have an associated
        // Administrative Action.  This is not an error.
        // JLK 01/13/99
        // -------------------------------------------------------
      }
    }
    else
    {
      // -------------------------------------------------------
      // An Appeal may or may not have an associated
      // Administrative Action.  This is not an error.
      // JLK 01/13/99
      // -------------------------------------------------------
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveAdministrativeAppeal1(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveAdministrativeAppeal2(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.RequestDate = source.RequestDate;
    target.ReceivedDate = source.ReceivedDate;
    target.Reason = source.Reason;
    target.Respondent = source.Respondent;
    target.AppellantLastName = source.AppellantLastName;
    target.AppellantFirstName = source.AppellantFirstName;
    target.AppellantMiddleInitial = source.AppellantMiddleInitial;
    target.AppellantSuffix = source.AppellantSuffix;
    target.AppellantRelationship = source.AppellantRelationship;
    target.Outcome = source.Outcome;
    target.ReviewOutcome = source.ReviewOutcome;
    target.Date = source.Date;
    target.AdminOrderDate = source.AdminOrderDate;
    target.WithdrawDate = source.WithdrawDate;
    target.WithdrawReason = source.WithdrawReason;
    target.RequestFurtherReview = source.RequestFurtherReview;
    target.RequestFurtherReviewDate = source.RequestFurtherReviewDate;
    target.JudicialReviewInd = source.JudicialReviewInd;
    target.AdminReviewState = source.AdminReviewState;
  }

  private bool ReadAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(
          command, "tanfCode", entities.AdministrativeAppeal.AacTanfCode ?? ""
          );
        db.SetDate(
          command, "takenDt",
          entities.AdministrativeAppeal.AacRTakenDate.GetValueOrDefault());
        db.SetString(
          command, "cpaType", entities.AdministrativeAppeal.CpaRType ?? "");
        db.SetString(
          command, "type", entities.AdministrativeAppeal.AacRType ?? "");
        db.SetString(
          command, "cspNumber", entities.AdministrativeAppeal.CspRNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 5);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeActCertification.Populated);
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(
          command, "type1", entities.AdministrativeActCertification.AatType ?? ""
          );
        db.SetString(
          command, "type2", entities.AdministrativeActCertification.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeActionObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.AdministrativeAction.Populated = false;
    entities.ObligationAdministrativeAction.Populated = false;

    return Read("ReadAdministrativeActionObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.AdministrativeAppeal.OtyId.GetValueOrDefault());
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAppeal.AatType ?? "");
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.AdministrativeAppeal.ObgGeneratedId.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", entities.AdministrativeAppeal.CspNumber ?? "");
        db.SetNullableString(
          command, "cpaType", entities.AdministrativeAppeal.CpaType ?? "");
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.AdministrativeAppeal.OaaTakenDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 2);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 3);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 5);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 6);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 7);
        entities.AdministrativeAction.Populated = true;
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
      });
  }

  private bool ReadAdministrativeAppeal1()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
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
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 15);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 16);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 20);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 22);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 24);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 25);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 26);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 27);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 28);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 30);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.AdminReviewState =
          db.GetString(reader, 33);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private bool ReadAdministrativeAppeal2()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "adminAppealNo", import.AdministrativeAppeal.Number ?? "");
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
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
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 15);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 16);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 20);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 22);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 24);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 25);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 26);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 27);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 28);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 30);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.AdminReviewState =
          db.GetString(reader, 33);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private bool ReadAdministrativeAppeal3()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal3",
      (db, command) =>
      {
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
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
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 15);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 16);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 20);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 22);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 24);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 25);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 26);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 27);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 28);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 30);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.AdminReviewState =
          db.GetString(reader, 33);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private bool ReadAdministrativeAppeal4()
  {
    return Read("ReadAdministrativeAppeal4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "adminAppealNo", import.AdministrativeAppeal.Number ?? "");
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        local.NoOfAdminAppealsFound.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadAdministrativeAppeal5()
  {
    return Read("ReadAdministrativeAppeal5",
      (db, command) =>
      {
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        local.NoOfAdminAppealsFound.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public DateWorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
    private DateWorkArea actionTaken;
    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoOfAdminAppealsFound.
    /// </summary>
    [JsonPropertyName("noOfAdminAppealsFound")]
    public Common NoOfAdminAppealsFound
    {
      get => noOfAdminAppealsFound ??= new();
      set => noOfAdminAppealsFound = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private Common noOfAdminAppealsFound;
    private Common returnCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    private CsePerson csePerson;
    private AdministrativeAppeal administrativeAppeal;
    private AdministrativeAction administrativeAction;
    private AdministrativeActCertification administrativeActCertification;
    private ObligationAdministrativeAction obligationAdministrativeAction;
  }
#endregion
}
