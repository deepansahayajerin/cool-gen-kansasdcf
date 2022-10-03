// Program: RECEIVE_ADMINISTRATIVE_APPEAL, ID: 372605739, model: 746.
// Short name: SWE01043
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: RECEIVE_ADMINISTRATIVE_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates ADMINISTRATIVE APPEAL and ADMINISTRATIVE APPEAL ADDRESS
/// and associates ADMINISTRATIVE APPEAL to CSE PERSON and OBLIGATION
/// ADMINISTRATIVE APPEAL.
/// </para>
/// </summary>
[Serializable]
public partial class ReceiveAdministrativeAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RECEIVE_ADMINISTRATIVE_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReceiveAdministrativeAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReceiveAdministrativeAppeal.
  /// </summary>
  public ReceiveAdministrativeAppeal(IContext context, Import import,
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
    // -------------------------------------------------------------
    // Read imported CSE Person.
    // -------------------------------------------------------------
    if (ReadCsePerson())
    {
      // ---> Continue processing
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // -------------------------------------------------------------
    // Do not allow the Appeal to be created if there is already an
    // appeal on the database for the imported Appeal Type and
    // Request Date.   JLK  01-14-99
    // -------------------------------------------------------------
    if (ReadAdministrativeAppeal())
    {
      ExitState = "LE0000_ADMIN_APPEAL_PRESENT";

      return;
    }
    else
    {
      // ---> An appeal for the imported type and request date does
      // not exist.  OK to create.
    }

    // -------------------------------------------------------------
    // Set up local views.
    // -------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Dummy.Flag = "";

    // -------------------------------------------------------------
    // Create the Administrative Appeal for the imported CSE Person.
    // -------------------------------------------------------------
    if (IsEmpty(local.Dummy.Flag))
    {
      for(local.NoOfRetries.Count = 1; local.NoOfRetries.Count <= 10; ++
        local.NoOfRetries.Count)
      {
        local.SystemGenerated.Attribute9DigitRandomNumber =
          UseGenerate9DigitRandomNumber();

        try
        {
          CreateAdministrativeAppeal();

          goto Test;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              continue;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

      return;
    }

Test:

    // ----------------------------------------------------------
    // If an Administrative Action Type is imported, read the
    // Administrative Act Certification or the Obligation
    // Administrative Action and associate with the Appeal.
    // JLK  01/13/99
    // ----------------------------------------------------------
    if (!IsEmpty(import.AdministrativeAction.Type1))
    {
      if (Equal(import.AdministrativeAction.Type1, "FDSO") || Equal
        (import.AdministrativeAction.Type1, "SDSO") || Equal
        (import.AdministrativeAction.Type1, "CRED") || Equal
        (import.AdministrativeAction.Type1, "KDWP"))
      {
        if (ReadAdministrativeActCertification())
        {
          AssociateAdministrativeActCertification();
        }
        else
        {
          ExitState = "ADMINISTRATIVE_ACTN_TAKEN_NF_RB";

          return;
        }
      }
      else if (ReadObligationAdministrativeAction())
      {
        AssociateObligationAdministrativeAction();
      }
      else
      {
        ExitState = "ADMINISTRATIVE_ACTN_TAKEN_NF_RB";
      }
    }

    MoveAdministrativeAppeal(entities.AdministrativeAppeal,
      export.AdministrativeAppeal);
  }

  private static void MoveAdministrativeAppeal(AdministrativeAppeal source,
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

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void AssociateAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeActCertification.Populated);

    var cpaRType = entities.AdministrativeActCertification.CpaType;
    var cspRNumber = entities.AdministrativeActCertification.CspNumber;
    var aacRType = entities.AdministrativeActCertification.Type1;
    var aacRTakenDate = entities.AdministrativeActCertification.TakenDate;
    var aacTanfCode = entities.AdministrativeActCertification.TanfCode;

    CheckValid<AdministrativeAppeal>("CpaRType", cpaRType);
    CheckValid<AdministrativeAppeal>("AacRType", aacRType);
    entities.AdministrativeAppeal.Populated = false;
    Update("AssociateAdministrativeActCertification",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaRType", cpaRType);
        db.SetNullableString(command, "cspRNumber", cspRNumber);
        db.SetNullableString(command, "aacRType", aacRType);
        db.SetNullableDate(command, "aacRTakenDate", aacRTakenDate);
        db.SetNullableString(command, "aacTanfCode", aacTanfCode);
        db.SetInt32(
          command, "adminAppealId", entities.AdministrativeAppeal.Identifier);
      });

    entities.AdministrativeAppeal.CpaRType = cpaRType;
    entities.AdministrativeAppeal.CspRNumber = cspRNumber;
    entities.AdministrativeAppeal.AacRType = aacRType;
    entities.AdministrativeAppeal.AacRTakenDate = aacRTakenDate;
    entities.AdministrativeAppeal.AacTanfCode = aacTanfCode;
    entities.AdministrativeAppeal.Populated = true;
  }

  private void AssociateObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.ObligationAdministrativeAction.Populated);

    var aatType = entities.ObligationAdministrativeAction.AatType;
    var obgGeneratedId = entities.ObligationAdministrativeAction.ObgGeneratedId;
    var cspNumber = entities.ObligationAdministrativeAction.CspNumber;
    var cpaType = entities.ObligationAdministrativeAction.CpaType;
    var oaaTakenDate = entities.ObligationAdministrativeAction.TakenDate;
    var otyId = entities.ObligationAdministrativeAction.OtyType;

    CheckValid<AdministrativeAppeal>("CpaType", cpaType);
    entities.AdministrativeAppeal.Populated = false;
    Update("AssociateObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "cpaType", cpaType);
        db.SetNullableDate(command, "oaaTakenDate", oaaTakenDate);
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetInt32(
          command, "adminAppealId", entities.AdministrativeAppeal.Identifier);
      });

    entities.AdministrativeAppeal.AatType = aatType;
    entities.AdministrativeAppeal.ObgGeneratedId = obgGeneratedId;
    entities.AdministrativeAppeal.CspNumber = cspNumber;
    entities.AdministrativeAppeal.CpaType = cpaType;
    entities.AdministrativeAppeal.OaaTakenDate = oaaTakenDate;
    entities.AdministrativeAppeal.OtyId = otyId;
    entities.AdministrativeAppeal.Populated = true;
  }

  private void CreateAdministrativeAppeal()
  {
    var identifier = local.SystemGenerated.Attribute9DigitRandomNumber;
    var number = import.AdministrativeAppeal.Number ?? "";
    var type1 = import.AdministrativeAppeal.Type1;
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
    var createdBy = global.UserId;
    var createdTstamp = local.Current.Timestamp;
    var cspQNumber = entities.CsePerson.Number;
    var cpaRType = GetImplicitValue<AdministrativeAppeal, string>("CpaRType");
    var cpaType = GetImplicitValue<AdministrativeAppeal, string>("CpaType");
    var judicialReviewInd = import.AdministrativeAppeal.JudicialReviewInd ?? "";
    var reason = import.AdministrativeAppeal.Reason;
    var outcome = import.AdministrativeAppeal.Outcome ?? "";
    var reviewOutcome = import.AdministrativeAppeal.ReviewOutcome ?? "";
    var withdrawReason = import.AdministrativeAppeal.WithdrawReason ?? "";
    var requestFurtherReview =
      import.AdministrativeAppeal.RequestFurtherReview ?? "";
    var adminReviewState = import.AdministrativeAppeal.AdminReviewState;

    CheckValid<AdministrativeAppeal>("CpaRType", cpaRType);
    CheckValid<AdministrativeAppeal>("CpaType", cpaType);
    entities.AdministrativeAppeal.Populated = false;
    Update("CreateAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "adminAppealId", identifier);
        db.SetNullableString(command, "adminAppealNo", number);
        db.SetString(command, "type", type1);
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
          
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "cspQNumber", cspQNumber);
        db.SetNullableString(command, "cpaRType", cpaRType);
        db.SetNullableString(command, "cpaType", cpaType);
        db.SetNullableString(command, "judReviewInd", judicialReviewInd);
        db.SetString(command, "reason", reason);
        db.SetNullableString(command, "outcome", outcome);
        db.SetNullableString(command, "reviewOutcome", reviewOutcome);
        db.SetNullableString(command, "withdrawReason", withdrawReason);
        db.SetNullableString(command, "reqFurtherReview", requestFurtherReview);
        db.SetString(command, "adminReviewState", adminReviewState);
      });

    entities.AdministrativeAppeal.Identifier = identifier;
    entities.AdministrativeAppeal.Number = number;
    entities.AdministrativeAppeal.Type1 = type1;
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
    entities.AdministrativeAppeal.CreatedBy = createdBy;
    entities.AdministrativeAppeal.CreatedTstamp = createdTstamp;
    entities.AdministrativeAppeal.CspQNumber = cspQNumber;
    entities.AdministrativeAppeal.CpaRType = cpaRType;
    entities.AdministrativeAppeal.CspRNumber = null;
    entities.AdministrativeAppeal.AacRType = null;
    entities.AdministrativeAppeal.AacRTakenDate = null;
    entities.AdministrativeAppeal.AatType = null;
    entities.AdministrativeAppeal.ObgGeneratedId = null;
    entities.AdministrativeAppeal.CspNumber = null;
    entities.AdministrativeAppeal.CpaType = cpaType;
    entities.AdministrativeAppeal.OaaTakenDate = null;
    entities.AdministrativeAppeal.OtyId = null;
    entities.AdministrativeAppeal.JudicialReviewInd = judicialReviewInd;
    entities.AdministrativeAppeal.Reason = reason;
    entities.AdministrativeAppeal.Outcome = outcome;
    entities.AdministrativeAppeal.ReviewOutcome = reviewOutcome;
    entities.AdministrativeAppeal.WithdrawReason = withdrawReason;
    entities.AdministrativeAppeal.RequestFurtherReview = requestFurtherReview;
    entities.AdministrativeAppeal.AdminReviewState = adminReviewState;
    entities.AdministrativeAppeal.AacTanfCode = null;
    entities.AdministrativeAppeal.Populated = true;
  }

  private bool ReadAdministrativeActCertification()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
        db.SetDate(
          command, "takenDt", import.ActionTaken.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
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
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 4);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetNullableString(command, "cspQNumber", entities.CsePerson.Number);
        db.SetString(command, "type", import.AdministrativeAppeal.Type1);
        db.SetDate(
          command, "requestDt",
          import.AdministrativeAppeal.RequestDate.GetValueOrDefault());
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
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 21);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 22);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 23);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 24);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 26);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 27);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 28);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 29);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 30);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 33);
        entities.AdministrativeAppeal.AdminReviewState =
          db.GetString(reader, 34);
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
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadObligationAdministrativeAction()
  {
    entities.ObligationAdministrativeAction.Populated = false;

    return Read("ReadObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetDate(
          command, "takenDt", import.ActionTaken.Date.GetValueOrDefault());
        db.SetString(command, "aatType", import.AdministrativeAction.Type1);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 0);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 1);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 4);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 5);
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of NoOfRetries.
    /// </summary>
    [JsonPropertyName("noOfRetries")]
    public Common NoOfRetries
    {
      get => noOfRetries ??= new();
      set => noOfRetries = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private Common dummy;
    private Common noOfRetries;
    private DateWorkArea current;
    private SystemGenerated systemGenerated;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private CsePerson csePerson;
    private AdministrativeAction administrativeAction;
    private AdministrativeAppeal administrativeAppeal;
    private AdministrativeActCertification administrativeActCertification;
    private CsePersonAccount obligor;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private Obligation obligation;
  }
#endregion
}
