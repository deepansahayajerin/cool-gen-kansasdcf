// Program: LE_LAPP_UPDATE_LEGAL_APPEAL, ID: 371973999, model: 746.
// Short name: SWE00790
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LAPP_UPDATE_LEGAL_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block updates legal APPEAL.
/// </para>
/// </summary>
[Serializable]
public partial class LeLappUpdateLegalAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPP_UPDATE_LEGAL_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLappUpdateLegalAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLappUpdateLegalAppeal.
  /// </summary>
  public LeLappUpdateLegalAppeal(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadAppeal())
    {
      if (ReadTribunal1())
      {
        if (ReadLegalAction())
        {
          if (ReadTribunal2())
          {
            try
            {
              UpdateAppeal();

              if (Equal(import.NewTribunal.JudicialDistrict,
                entities.ExistingAppealedTo.JudicialDistrict) || Equal
                (import.NewTribunal.JudicialDivision,
                import.NewTribunal.JudicialDivision))
              {
              }
              else
              {
                try
                {
                  UpdateTribunal();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
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

              export.Appeal.Assign(entities.Existing);
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "APPEAL_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "APPEAL_PV";

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
            ExitState = "TRIBUNAL_NF";
          }
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";
        }
      }
      else
      {
        ExitState = "TRIBUNAL_NF";
      }
    }
    else
    {
      ExitState = "APPEAL_NF";
    }
  }

  private bool ReadAppeal()
  {
    entities.Existing.Populated = false;

    return Read("ReadAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "appealId", import.Appeal.Identifier);
      },
      (db, reader) =>
      {
        entities.Existing.Identifier = db.GetInt32(reader, 0);
        entities.Existing.DocketNumber = db.GetString(reader, 1);
        entities.Existing.FiledByFirstName = db.GetNullableString(reader, 2);
        entities.Existing.FiledByMi = db.GetNullableString(reader, 3);
        entities.Existing.FiledByLastName = db.GetString(reader, 4);
        entities.Existing.AppealDate = db.GetDate(reader, 5);
        entities.Existing.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.Existing.AttorneyLastName = db.GetString(reader, 7);
        entities.Existing.AttorneyFirstName = db.GetString(reader, 8);
        entities.Existing.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.Existing.AttorneySuffix = db.GetNullableString(reader, 10);
        entities.Existing.AppellantBriefDate = db.GetNullableDate(reader, 11);
        entities.Existing.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.Existing.OralArgumentDate = db.GetNullableDate(reader, 13);
        entities.Existing.DecisionDate = db.GetNullableDate(reader, 14);
        entities.Existing.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.Existing.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.Existing.DateExtensionGranted = db.GetNullableDate(reader, 17);
        entities.Existing.CreatedBy = db.GetString(reader, 18);
        entities.Existing.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Existing.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.Existing.TrbId = db.GetNullableInt32(reader, 22);
        entities.Existing.DecisionResult = db.GetNullableString(reader, 23);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.NewAppealedAgainst.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.NewLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.NewAppealedAgainst.Identifier = db.GetInt32(reader, 0);
        entities.NewAppealedAgainst.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingAppealedTo.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.Existing.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAppealedTo.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingAppealedTo.JudicialDistrict = db.GetString(reader, 1);
        entities.ExistingAppealedTo.Identifier = db.GetInt32(reader, 2);
        entities.ExistingAppealedTo.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.NewAppealedTo.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.NewTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.NewAppealedTo.Identifier = db.GetInt32(reader, 0);
        entities.NewAppealedTo.Populated = true;
      });
  }

  private void UpdateAppeal()
  {
    var docketNumber = import.Appeal.DocketNumber;
    var filedByFirstName = import.Appeal.FiledByFirstName ?? "";
    var filedByMi = import.Appeal.FiledByMi ?? "";
    var filedByLastName = import.Appeal.FiledByLastName;
    var appealDate = import.Appeal.AppealDate;
    var docketingStmtFiledDate = import.Appeal.DocketingStmtFiledDate;
    var attorneyLastName = import.Appeal.AttorneyLastName;
    var attorneyFirstName = import.Appeal.AttorneyFirstName;
    var attorneyMiddleInitial = import.Appeal.AttorneyMiddleInitial ?? "";
    var attorneySuffix = import.Appeal.AttorneySuffix ?? "";
    var appellantBriefDate = import.Appeal.AppellantBriefDate;
    var replyBriefDate = import.Appeal.ReplyBriefDate;
    var oralArgumentDate = import.Appeal.OralArgumentDate;
    var decisionDate = import.Appeal.DecisionDate;
    var furtherAppealIndicator = import.Appeal.FurtherAppealIndicator ?? "";
    var extentionReqGrantedDate = import.Appeal.ExtentionReqGrantedDate;
    var dateExtensionGranted = import.Appeal.DateExtensionGranted;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var trbId = entities.NewAppealedTo.Identifier;
    var decisionResult = import.Appeal.DecisionResult ?? "";

    entities.Existing.Populated = false;
    Update("UpdateAppeal",
      (db, command) =>
      {
        db.SetString(command, "docketNo", docketNumber);
        db.SetNullableString(command, "filedByFirst", filedByFirstName);
        db.SetNullableString(command, "filedByMi", filedByMi);
        db.SetString(command, "filedByLastName", filedByLastName);
        db.SetDate(command, "appealDt", appealDate);
        db.SetDate(command, "docketFiledDt", docketingStmtFiledDate);
        db.SetString(command, "attorneyLastNm", attorneyLastName);
        db.SetString(command, "attorneyFirstNm", attorneyFirstName);
        db.SetNullableString(command, "attorneyMi", attorneyMiddleInitial);
        db.SetNullableString(command, "attorneySuffix", attorneySuffix);
        db.SetNullableDate(command, "appellantBriefDt", appellantBriefDate);
        db.SetNullableDate(command, "replyBriefDt", replyBriefDate);
        db.SetNullableDate(command, "oralArgumentDt", oralArgumentDate);
        db.SetNullableDate(command, "decisionDt", decisionDate);
        db.
          SetNullableString(command, "furtherAppealInd", furtherAppealIndicator);
          
        db.SetNullableDate(command, "extReqGrantedDt", extentionReqGrantedDate);
        db.SetNullableDate(command, "dtExtGranted", dateExtensionGranted);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetNullableString(command, "decisionResult", decisionResult);
        db.SetInt32(command, "appealId", entities.Existing.Identifier);
      });

    entities.Existing.DocketNumber = docketNumber;
    entities.Existing.FiledByFirstName = filedByFirstName;
    entities.Existing.FiledByMi = filedByMi;
    entities.Existing.FiledByLastName = filedByLastName;
    entities.Existing.AppealDate = appealDate;
    entities.Existing.DocketingStmtFiledDate = docketingStmtFiledDate;
    entities.Existing.AttorneyLastName = attorneyLastName;
    entities.Existing.AttorneyFirstName = attorneyFirstName;
    entities.Existing.AttorneyMiddleInitial = attorneyMiddleInitial;
    entities.Existing.AttorneySuffix = attorneySuffix;
    entities.Existing.AppellantBriefDate = appellantBriefDate;
    entities.Existing.ReplyBriefDate = replyBriefDate;
    entities.Existing.OralArgumentDate = oralArgumentDate;
    entities.Existing.DecisionDate = decisionDate;
    entities.Existing.FurtherAppealIndicator = furtherAppealIndicator;
    entities.Existing.ExtentionReqGrantedDate = extentionReqGrantedDate;
    entities.Existing.DateExtensionGranted = dateExtensionGranted;
    entities.Existing.LastUpdatedBy = lastUpdatedBy;
    entities.Existing.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Existing.TrbId = trbId;
    entities.Existing.DecisionResult = decisionResult;
    entities.Existing.Populated = true;
  }

  private void UpdateTribunal()
  {
    var judicialDivision = import.NewTribunal.JudicialDivision ?? "";
    var judicialDistrict = import.NewTribunal.JudicialDistrict;

    entities.ExistingAppealedTo.Populated = false;
    Update("UpdateTribunal",
      (db, command) =>
      {
        db.SetNullableString(command, "judicialDivision", judicialDivision);
        db.SetString(command, "judicialDistrict", judicialDistrict);
        db.SetInt32(
          command, "identifier", entities.ExistingAppealedTo.Identifier);
      });

    entities.ExistingAppealedTo.JudicialDivision = judicialDivision;
    entities.ExistingAppealedTo.JudicialDistrict = judicialDistrict;
    entities.ExistingAppealedTo.Populated = true;
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
    /// A value of NewLegalAction.
    /// </summary>
    [JsonPropertyName("newLegalAction")]
    public LegalAction NewLegalAction
    {
      get => newLegalAction ??= new();
      set => newLegalAction = value;
    }

    /// <summary>
    /// A value of NewTribunal.
    /// </summary>
    [JsonPropertyName("newTribunal")]
    public Tribunal NewTribunal
    {
      get => newTribunal ??= new();
      set => newTribunal = value;
    }

    /// <summary>
    /// A value of Appeal.
    /// </summary>
    [JsonPropertyName("appeal")]
    public Appeal Appeal
    {
      get => appeal ??= new();
      set => appeal = value;
    }

    private LegalAction newLegalAction;
    private Tribunal newTribunal;
    private Appeal appeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Appeal.
    /// </summary>
    [JsonPropertyName("appeal")]
    public Appeal Appeal
    {
      get => appeal ??= new();
      set => appeal = value;
    }

    private Appeal appeal;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LastAppeal.
    /// </summary>
    [JsonPropertyName("lastAppeal")]
    public Appeal LastAppeal
    {
      get => lastAppeal ??= new();
      set => lastAppeal = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public Appeal InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of LastLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("lastLegalActionAppeal")]
    public LegalActionAppeal LastLegalActionAppeal
    {
      get => lastLegalActionAppeal ??= new();
      set => lastLegalActionAppeal = value;
    }

    private Appeal lastAppeal;
    private Appeal initialisedToZeros;
    private LegalActionAppeal lastLegalActionAppeal;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LegalActionAppeal New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingAppealedAgainst.
    /// </summary>
    [JsonPropertyName("existingAppealedAgainst")]
    public LegalAction ExistingAppealedAgainst
    {
      get => existingAppealedAgainst ??= new();
      set => existingAppealedAgainst = value;
    }

    /// <summary>
    /// A value of NewAppealedAgainst.
    /// </summary>
    [JsonPropertyName("newAppealedAgainst")]
    public LegalAction NewAppealedAgainst
    {
      get => newAppealedAgainst ??= new();
      set => newAppealedAgainst = value;
    }

    /// <summary>
    /// A value of ExistingAppealedTo.
    /// </summary>
    [JsonPropertyName("existingAppealedTo")]
    public Tribunal ExistingAppealedTo
    {
      get => existingAppealedTo ??= new();
      set => existingAppealedTo = value;
    }

    /// <summary>
    /// A value of NewAppealedTo.
    /// </summary>
    [JsonPropertyName("newAppealedTo")]
    public Tribunal NewAppealedTo
    {
      get => newAppealedTo ??= new();
      set => newAppealedTo = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Appeal Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public LegalActionAppeal ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    private LegalActionAppeal new1;
    private LegalAction existingAppealedAgainst;
    private LegalAction newAppealedAgainst;
    private Tribunal existingAppealedTo;
    private Tribunal newAppealedTo;
    private Appeal existing;
    private LegalActionAppeal existingLast;
  }
#endregion
}
