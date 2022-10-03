// Program: FN_CRTE_UPD_COLL_DISB_SUPPR, ID: 371364872, model: 746.
// Short name: SWE03593
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CRTE_UPD_COLL_DISB_SUPPR.
/// </para>
/// <para>
/// This action diagram is to create a disbursement suppression history if there
/// is no date overlap, or update the disbursement suppression history if one
/// is found. The discontinue date is the attribute/column to be updated, and
/// the date is to be set as the current date plus 7 months.
/// </para>
/// </summary>
[Serializable]
public partial class FnCrteUpdCollDisbSuppr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRTE_UPD_COLL_DISB_SUPPR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrteUpdCollDisbSuppr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrteUpdCollDisbSuppr.
  /// </summary>
  public FnCrteUpdCollDisbSuppr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveCollectionType(import.CollectionType, export.CollectionType);
    MoveDisbSuppressionStatusHistory(import.DisbSuppressionStatusHistory,
      export.DisbSuppressionStatusHistory);
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCollectionType())
    {
      MoveCollectionType(entities.CollectionType, export.CollectionType);
    }
    else
    {
      ExitState = "FN0000_COLLECTION_TYPE_NF";

      return;
    }

    if (ReadCsePersonAccount())
    {
      // **  OK  **
    }
    else if (ReadCsePerson())
    {
      try
      {
        CreateCsePersonAccount();

        // **  OK  **
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // now check for date overlap
    foreach(var item in ReadDisbSuppressionStatusHistory())
    {
      // The collection type can be the same as long as the court orders are 
      // diferent.
      if (AsChar(import.DisbSuppressionStatusHistory.Type1) == 'O' && AsChar
        (entities.DisbSuppressionStatusHistory.Type1) == 'O')
      {
        if (ReadLegalAction2())
        {
          // continue
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF_RB";

          return;
        }

        if (!Equal(import.LegalAction.StandardNumber,
          entities.LegalAction.StandardNumber))
        {
          // The court order numbers are different so  go on & look for the next
          // overlapping rule.
          continue;
        }
      }

      if (Equal(entities.DisbSuppressionStatusHistory.DiscontinueDate,
        local.Maximum.Date))
      {
        export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;

        return;
      }

      try
      {
        UpdateDisbSuppressionStatusHistory();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_SUPP_STAT_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_SUPP_STAT_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // No overlap, a creation is is allowed.
    try
    {
      CreateDisbSuppressionStatusHistory();
      export.DisbSuppressionStatusHistory.Assign(
        entities.DisbSuppressionStatusHistory);

      if (Equal(entities.DisbSuppressionStatusHistory.DiscontinueDate,
        local.Maximum.Date))
      {
        export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
      }

      if (IsEmpty(entities.DisbSuppressionStatusHistory.LastUpdatedBy))
      {
        export.DisbSuppressionStatusHistory.LastUpdatedBy =
          entities.DisbSuppressionStatusHistory.CreatedBy;
      }

      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        if (ReadLegalAction1())
        {
          AssociateLegalAction();
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF_RB";

          return;
        }
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_SUPP_STAT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_SUPP_STAT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveDisbSuppressionStatusHistory(
    DisbSuppressionStatusHistory source, DisbSuppressionStatusHistory target)
  {
    target.Type1 = source.Type1;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ReasonText = source.ReasonText;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseFnAssignDisbSupprId()
  {
    var useImport = new FnAssignDisbSupprId.Import();
    var useExport = new FnAssignDisbSupprId.Export();

    useImport.Persistent.Assign(entities.CsePersonAccount);

    Call(FnAssignDisbSupprId.Execute, useImport, useExport);

    entities.CsePersonAccount.Type1 = useImport.Persistent.Type1;

    return useExport.DisbSuppressionStatusHistory.SystemGeneratedIdentifier;
  }

  private void AssociateLegalAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var lgaIdentifier = entities.LegalAction.Identifier;

    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("AssociateLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.LgaIdentifier = lgaIdentifier;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.CsePerson.Number;
    var type1 = import.CsePersonAccount.Type1;
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.CsePersonAccount.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.CsePersonAccount.CspNumber = cspNumber;
    entities.CsePersonAccount.Type1 = type1;
    entities.CsePersonAccount.CreatedBy = createdBy;
    entities.CsePersonAccount.CreatedTmst = createdTmst;
    entities.CsePersonAccount.Populated = true;
  }

  private void CreateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignDisbSupprId();
    var cltSequentialId = entities.CollectionType.SequentialIdentifier;
    var effectiveDate = import.DisbSuppressionStatusHistory.EffectiveDate;
    var discontinueDate = import.DisbSuppressionStatusHistory.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = import.DisbSuppressionStatusHistory.Type1;
    var reasonText = import.DisbSuppressionStatusHistory.ReasonText ?? "";

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("CreateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetNullableInt32(command, "cltSequentialId", cltSequentialId);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.DisbSuppressionStatusHistory.CpaType = cpaType;
    entities.DisbSuppressionStatusHistory.CspNumber = cspNumber;
    entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbSuppressionStatusHistory.CltSequentialId = cltSequentialId;
    entities.DisbSuppressionStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.CreatedBy = createdBy;
    entities.DisbSuppressionStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbSuppressionStatusHistory.LastUpdatedBy = "";
    entities.DisbSuppressionStatusHistory.LastUpdatedTmst = null;
    entities.DisbSuppressionStatusHistory.Type1 = type1;
    entities.DisbSuppressionStatusHistory.ReasonText = reasonText;
    entities.DisbSuppressionStatusHistory.LgaIdentifier = null;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.CreatedBy = db.GetString(reader, 2);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 3);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          import.DisbSuppressionStatusHistory.DiscontinueDate.
            GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          import.DisbSuppressionStatusHistory.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 10);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.DisbSuppressionStatusHistory.LgaIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private void UpdateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var discontinueDate = Now().Date.AddMonths(7);

    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("UpdateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.Populated = true;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CollectionType collectionType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private CollectionType collectionType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of CurrentSuppression.
    /// </summary>
    [JsonPropertyName("currentSuppression")]
    public Common CurrentSuppression
    {
      get => currentSuppression ??= new();
      set => currentSuppression = value;
    }

    private DateWorkArea zero;
    private DateWorkArea dateWorkArea;
    private DateWorkArea maximum;
    private Common currentSuppression;
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

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private LegalAction legalAction;
    private CollectionType collectionType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private CsePersonAccount obligee;
  }
#endregion
}
