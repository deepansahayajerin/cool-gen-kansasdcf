// Program: LE_LAPP_CREATE_LEGAL_APPEAL, ID: 371974000, model: 746.
// Short name: SWE00787
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LAPP_CREATE_LEGAL_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates Legal APPEAL entity.
/// </para>
/// </summary>
[Serializable]
public partial class LeLappCreateLegalAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPP_CREATE_LEGAL_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLappCreateLegalAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLappCreateLegalAppeal.
  /// </summary>
  public LeLappCreateLegalAppeal(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------
    // CHANGE LOG
    // 12/11/98	P McElderry
    // Restructed code.
    // -------------------------------------------------------
    if (ReadLegalAction())
    {
      if (ReadTribunal())
      {
        export.Tribunal.Assign(entities.ExistingAppealedTo);

        if (ReadAppeal())
        {
          ExitState = "LE0000_APPEAL_AE_WITH_SAME_DETLS";
        }
      }
      else
      {
        ExitState = "TRIBUNAL_NF";
      }
    }
    else
    {
      ExitState = "ZD_LEGAL_ACTION_NF_2";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    local.Dummy.Flag = "";

    if (IsEmpty(local.Dummy.Flag))
    {
      for(local.NoOfRetries.Count = 1; local.NoOfRetries.Count <= 10; ++
        local.NoOfRetries.Count)
      {
        // ----------------------------------------------------------------
        // Every APPEAL record will have its own
        // LEGAL_ACTION_APPEAL record. We are not using the same
        // APPEAL record if an APPEAL is filed by the same person
        // against more than one legal action.
        // LACT	LEGAL ACTION APPEAL	APPEAL
        // LA1	LA-LAPP-1		LAPP-DKT-1
        // LA2	LA-LAPP-2		LAPP-DKT-1
        // Though this was not the way the data model was defined, it
        // does not pose any problem.
        // --------------------------------------------------------------------
        try
        {
          CreateAppeal();
          export.Appeal.Identifier = entities.NewAppeal.Identifier;

          try
          {
            CreateLegalActionAppeal();

            // -------------------
            // Continue processing
            // -------------------
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "LEGAL_ACTION_APPEAL_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              continue;
            case ErrorCode.PermittedValueViolation:
              ExitState = "APPEAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        return;
      }

      ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateAppeal()
  {
    var identifier = UseGenerate9DigitRandomNumber();
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
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var lastUpdatedTstamp = local.InitialisedToZeros.LastUpdatedTstamp;
    var trbId = entities.ExistingAppealedTo.Identifier;
    var decisionResult = import.Appeal.DecisionResult ?? "";

    entities.NewAppeal.Populated = false;
    Update("CreateAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "appealId", identifier);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetNullableString(command, "decisionResult", decisionResult);
      });

    entities.NewAppeal.Identifier = identifier;
    entities.NewAppeal.DocketNumber = docketNumber;
    entities.NewAppeal.FiledByFirstName = filedByFirstName;
    entities.NewAppeal.FiledByMi = filedByMi;
    entities.NewAppeal.FiledByLastName = filedByLastName;
    entities.NewAppeal.AppealDate = appealDate;
    entities.NewAppeal.DocketingStmtFiledDate = docketingStmtFiledDate;
    entities.NewAppeal.AttorneyLastName = attorneyLastName;
    entities.NewAppeal.AttorneyFirstName = attorneyFirstName;
    entities.NewAppeal.AttorneyMiddleInitial = attorneyMiddleInitial;
    entities.NewAppeal.AttorneySuffix = attorneySuffix;
    entities.NewAppeal.AppellantBriefDate = appellantBriefDate;
    entities.NewAppeal.ReplyBriefDate = replyBriefDate;
    entities.NewAppeal.OralArgumentDate = oralArgumentDate;
    entities.NewAppeal.DecisionDate = decisionDate;
    entities.NewAppeal.FurtherAppealIndicator = furtherAppealIndicator;
    entities.NewAppeal.ExtentionReqGrantedDate = extentionReqGrantedDate;
    entities.NewAppeal.DateExtensionGranted = dateExtensionGranted;
    entities.NewAppeal.CreatedBy = createdBy;
    entities.NewAppeal.CreatedTstamp = createdTstamp;
    entities.NewAppeal.LastUpdatedBy = "";
    entities.NewAppeal.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.NewAppeal.TrbId = trbId;
    entities.NewAppeal.DecisionResult = decisionResult;
    entities.NewAppeal.Populated = true;
  }

  private void CreateLegalActionAppeal()
  {
    var identifier = 1;
    var aplId = entities.NewAppeal.Identifier;
    var lgaId = entities.ExistingAppealedAgainst.Identifier;
    var createdBy = global.UserId;
    var createdTmst = Now();

    entities.NewLegalActionAppeal.Populated = false;
    Update("CreateLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "laAppealId", identifier);
        db.SetInt32(command, "aplId", aplId);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
      });

    entities.NewLegalActionAppeal.Identifier = identifier;
    entities.NewLegalActionAppeal.AplId = aplId;
    entities.NewLegalActionAppeal.LgaId = lgaId;
    entities.NewLegalActionAppeal.CreatedBy = createdBy;
    entities.NewLegalActionAppeal.CreatedTmst = createdTmst;
    entities.NewLegalActionAppeal.Populated = true;
  }

  private bool ReadAppeal()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaId", entities.ExistingAppealedAgainst.Identifier);
        db.SetString(command, "docketNo", import.Appeal.DocketNumber);
        db.SetDate(
          command, "appealDt", import.Appeal.AppealDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "appellantBriefDt",
          import.Appeal.AppellantBriefDate.GetValueOrDefault());
        db.
          SetString(command, "attorneyFirstNm", import.Appeal.AttorneyFirstName);
          
        db.SetString(command, "attorneyLastNm", import.Appeal.AttorneyLastName);
        db.SetNullableString(
          command, "attorneyMi", import.Appeal.AttorneyMiddleInitial ?? "");
        db.SetNullableString(
          command, "attorneySuffix", import.Appeal.AttorneySuffix ?? "");
        db.SetNullableDate(
          command, "dtExtGranted",
          import.Appeal.DateExtensionGranted.GetValueOrDefault());
        db.SetNullableDate(
          command, "decisionDt",
          import.Appeal.DecisionDate.GetValueOrDefault());
        db.SetNullableString(
          command, "decisionResult", import.Appeal.DecisionResult ?? "");
        db.SetDate(
          command, "docketFiledDt",
          import.Appeal.DocketingStmtFiledDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "extReqGrantedDt",
          import.Appeal.ExtentionReqGrantedDate.GetValueOrDefault());
        db.SetNullableString(
          command, "filedByFirst", import.Appeal.FiledByFirstName ?? "");
        db.SetString(command, "filedByLastName", import.Appeal.FiledByLastName);
        db.
          SetNullableString(command, "filedByMi", import.Appeal.FiledByMi ?? "");
          
        db.SetNullableString(
          command, "furtherAppealInd", import.Appeal.FurtherAppealIndicator ?? ""
          );
        db.SetNullableDate(
          command, "oralArgumentDt",
          import.Appeal.OralArgumentDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "replyBriefDt",
          import.Appeal.ReplyBriefDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.DocketNumber = db.GetString(reader, 1);
        entities.ExistingAppeal.FiledByFirstName =
          db.GetNullableString(reader, 2);
        entities.ExistingAppeal.FiledByMi = db.GetNullableString(reader, 3);
        entities.ExistingAppeal.FiledByLastName = db.GetString(reader, 4);
        entities.ExistingAppeal.AppealDate = db.GetDate(reader, 5);
        entities.ExistingAppeal.DocketingStmtFiledDate = db.GetDate(reader, 6);
        entities.ExistingAppeal.AttorneyLastName = db.GetString(reader, 7);
        entities.ExistingAppeal.AttorneyFirstName = db.GetString(reader, 8);
        entities.ExistingAppeal.AttorneyMiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingAppeal.AttorneySuffix =
          db.GetNullableString(reader, 10);
        entities.ExistingAppeal.AppellantBriefDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingAppeal.ReplyBriefDate = db.GetNullableDate(reader, 12);
        entities.ExistingAppeal.OralArgumentDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingAppeal.DecisionDate = db.GetNullableDate(reader, 14);
        entities.ExistingAppeal.FurtherAppealIndicator =
          db.GetNullableString(reader, 15);
        entities.ExistingAppeal.ExtentionReqGrantedDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingAppeal.DateExtensionGranted =
          db.GetNullableDate(reader, 17);
        entities.ExistingAppeal.CreatedBy = db.GetString(reader, 18);
        entities.ExistingAppeal.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.ExistingAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingAppeal.DecisionResult =
          db.GetNullableString(reader, 22);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.ExistingAppealedAgainst.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppealedAgainst.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppealedAgainst.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.ExistingAppealedTo.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.To.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppealedTo.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingAppealedTo.Name = db.GetString(reader, 1);
        entities.ExistingAppealedTo.JudicialDistrict = db.GetString(reader, 2);
        entities.ExistingAppealedTo.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAppealedTo.Populated = true;
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
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Tribunal To
    {
      get => to ??= new();
      set => to = value;
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

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Tribunal to;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Appeal appeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Tribunal tribunal;
    private Appeal appeal;
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
    /// A value of LastLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("lastLegalActionAppeal")]
    public LegalActionAppeal LastLegalActionAppeal
    {
      get => lastLegalActionAppeal ??= new();
      set => lastLegalActionAppeal = value;
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
    /// A value of LastAppeal.
    /// </summary>
    [JsonPropertyName("lastAppeal")]
    public Appeal LastAppeal
    {
      get => lastAppeal ??= new();
      set => lastAppeal = value;
    }

    private Common dummy;
    private Common noOfRetries;
    private LegalActionAppeal lastLegalActionAppeal;
    private Appeal initialisedToZeros;
    private Appeal lastAppeal;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("existingLegalActionAppeal")]
    public LegalActionAppeal ExistingLegalActionAppeal
    {
      get => existingLegalActionAppeal ??= new();
      set => existingLegalActionAppeal = value;
    }

    /// <summary>
    /// A value of ExistingAppeal.
    /// </summary>
    [JsonPropertyName("existingAppeal")]
    public Appeal ExistingAppeal
    {
      get => existingAppeal ??= new();
      set => existingAppeal = value;
    }

    /// <summary>
    /// A value of ExistingLastLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("existingLastLegalActionAppeal")]
    public LegalActionAppeal ExistingLastLegalActionAppeal
    {
      get => existingLastLegalActionAppeal ??= new();
      set => existingLastLegalActionAppeal = value;
    }

    /// <summary>
    /// A value of NewLegalActionAppeal.
    /// </summary>
    [JsonPropertyName("newLegalActionAppeal")]
    public LegalActionAppeal NewLegalActionAppeal
    {
      get => newLegalActionAppeal ??= new();
      set => newLegalActionAppeal = value;
    }

    /// <summary>
    /// A value of ExistingLastAppeal.
    /// </summary>
    [JsonPropertyName("existingLastAppeal")]
    public Appeal ExistingLastAppeal
    {
      get => existingLastAppeal ??= new();
      set => existingLastAppeal = value;
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
    /// A value of ExistingAppealedTo.
    /// </summary>
    [JsonPropertyName("existingAppealedTo")]
    public Tribunal ExistingAppealedTo
    {
      get => existingAppealedTo ??= new();
      set => existingAppealedTo = value;
    }

    /// <summary>
    /// A value of NewAppeal.
    /// </summary>
    [JsonPropertyName("newAppeal")]
    public Appeal NewAppeal
    {
      get => newAppeal ??= new();
      set => newAppeal = value;
    }

    private LegalActionAppeal existingLegalActionAppeal;
    private Appeal existingAppeal;
    private LegalActionAppeal existingLastLegalActionAppeal;
    private LegalActionAppeal newLegalActionAppeal;
    private Appeal existingLastAppeal;
    private LegalAction existingAppealedAgainst;
    private Tribunal existingAppealedTo;
    private Appeal newAppeal;
  }
#endregion
}
