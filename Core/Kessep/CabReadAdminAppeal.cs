// Program: CAB_READ_ADMIN_APPEAL, ID: 372582882, model: 746.
// Short name: SWE00073
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADMIN_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block reads for a given Administrative Appeal.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdminAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADMIN_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdminAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdminAppeal.
  /// </summary>
  public CabReadAdminAppeal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveAdministrativeAppeal1(import.AdministrativeAppeal,
      export.AdministrativeAppeal);
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    if (import.AdministrativeAppeal.Identifier != 0)
    {
      // -------------------------------------------------------
      // This will be true with call from AAPP when admin appeal
      // number is not known but admin appeal has been selected
      // from AAPS.
      // -------------------------------------------------------
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
    else if (!IsEmpty(import.AdministrativeAppeal.Number))
    {
      if (ReadAdministrativeAppeal4())
      {
        MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
          export.AdministrativeAppeal);

        goto Test;
      }

      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }
    else
    {
      if (ReadAdministrativeAppeal3())
      {
        MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
          export.AdministrativeAppeal);

        goto Test;
      }

      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }

Test:

    if (ReadAdministrativeAppeal1())
    {
      MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
        export.AdministrativeAppeal);

      if (ReadAdministrativeActionObligationAdministrativeAction())
      {
        MoveAdministrativeAction(entities.AdministrativeAction,
          export.AdministrativeAction);
        export.DateWorkArea.Date =
          entities.ObligationAdministrativeAction.TakenDate;
      }
      else if (ReadAdministrativeActCertification())
      {
        export.DateWorkArea.Date =
          entities.AdministrativeActCertification.TakenDate;

        if (ReadAdministrativeAction())
        {
          MoveAdministrativeAction(entities.AdministrativeAction,
            export.AdministrativeAction);
        }
        else
        {
          ExitState = "ADMINISTRATIVE_ACTION_NF";
        }
      }

      if (ReadCsePerson())
      {
        // *********************************************
        // Call EAB to retrieve information about a CSE
        // PERSON from ADABAS.
        // *********************************************
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
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
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.Ae.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
    entities.ObligationAdministrativeAction.Populated = false;
    entities.AdministrativeAction.Populated = false;

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
        entities.ObligationAdministrativeAction.Populated = true;
        entities.AdministrativeAction.Populated = true;
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
          command, "adminAppealId", export.AdministrativeAppeal.Identifier);
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
        entities.AdministrativeAppeal.CreatedBy = db.GetString(reader, 15);
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 22);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 24);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 26);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 27);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 28);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 30);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 33);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 35);
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
        entities.AdministrativeAppeal.CreatedBy = db.GetString(reader, 15);
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 22);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 24);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 26);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 27);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 28);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 30);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 33);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 35);
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
        db.SetNullableString(
          command, "cspQNumber", import.CsePersonsWorkSet.Number);
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
        entities.AdministrativeAppeal.CreatedBy = db.GetString(reader, 15);
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 22);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 24);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 26);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 27);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 28);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 30);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 33);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 35);
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
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "adminAppealNo", import.AdministrativeAppeal.Number ?? "");
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
        entities.AdministrativeAppeal.CreatedBy = db.GetString(reader, 15);
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 21);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 22);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 24);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 26);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 27);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 28);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 30);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 33);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 34);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 35);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.AdministrativeAppeal.CspQNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 23);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
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

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common noOfAdminAppealsFound;
    private Common returnCode;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private CsePerson csePerson;
    private AdministrativeActCertification administrativeActCertification;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private AdministrativeAction administrativeAction;
    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}
