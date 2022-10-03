// Program: FN_UPD_COLLECTION_DISB_SUPPRESS, ID: 371751823, model: 746.
// Short name: SWE02273
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
/// A program: FN_UPD_COLLECTION_DISB_SUPPRESS.
/// </para>
/// <para>
/// Updates Collection Disbursement Suppression records.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdCollectionDisbSuppress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPD_COLLECTION_DISB_SUPPRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdCollectionDisbSuppress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdCollectionDisbSuppress.
  /// </summary>
  public FnUpdCollectionDisbSuppress(IContext context, Import import,
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
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    local.DateWorkArea.Date = Now().Date;

    if (ReadDisbSuppressionStatusHistory1())
    {
      // *****  changes for WR 040796
      if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'O')
      {
        if (!ReadLegalAction1())
        {
          ExitState = "LEGAL_ACTION_NF_RB";

          return;
        }
      }

      // *****  changes for WR 040796
    }
    else
    {
      ExitState = "FN0000_DISB_SUPP_STAT_NF";

      return;
    }

    // *******************************************
    // You can't update a discontinued record.
    // *******************************************
    if (Lt(entities.DisbSuppressionStatusHistory.DiscontinueDate,
      local.DateWorkArea.Date))
    {
      ExitState = "FN0000_CANT_UPDATE_DISCONTINUED";

      return;
    }

    // ***************************************************************
    // Can't change the discontinue date to be less than the current date.
    // ***************************************************************
    if (Lt(import.DisbSuppressionStatusHistory.DiscontinueDate,
      local.DateWorkArea.Date))
    {
      ExitState = "FN0000_SUPPR_MUST_RELE_IN_FUTURE";

      return;
    }

    // *******************************************************
    // Can't change the effective date on an active record.
    // *******************************************************
    if (!Equal(entities.DisbSuppressionStatusHistory.EffectiveDate,
      import.DisbSuppressionStatusHistory.EffectiveDate) && !
      Lt(local.DateWorkArea.Date,
      entities.DisbSuppressionStatusHistory.EffectiveDate))
    {
      ExitState = "FN0000_CANNOT_CHG_EFF_DT";

      return;
    }

    if (!ReadCollectionType())
    {
      ExitState = "FN0000_COLLECTION_TYPE_NF";

      return;
    }

    // *****  changes for WR 040796
    if (!IsEmpty(import.LegalAction.StandardNumber))
    {
      if (ReadLegalAction3())
      {
        // continue
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF_RB";

        return;
      }
    }

    // *****  changes for WR 040796
    // *****  changes for WR 040796
    if (!Lt(Now().Date, entities.DisbSuppressionStatusHistory.EffectiveDate) &&
      !Lt(entities.DisbSuppressionStatusHistory.DiscontinueDate, Now().Date))
    {
      // *****  this rule is currently in effect
      // *******************************************************
      // Can't change the suppression type on an active record.
      // *******************************************************
      if (entities.LegalAction.Populated && IsEmpty
        (import.LegalAction.StandardNumber) || !
        entities.LegalAction.Populated && !
        IsEmpty(import.LegalAction.StandardNumber))
      {
        ExitState = "FN0000_TYPE_CHANGE_ON_ACTIVE_RUL";

        return;
      }

      // *******************************************************
      // Can't change the Court Order Number an active record.
      // *******************************************************
      if (entities.LegalAction.Populated)
      {
        if (!Equal(import.LegalAction.StandardNumber,
          entities.LegalAction.StandardNumber))
        {
          ExitState = "FN0000_CO_NUMBER_CHANGE_ACTIVE_R";

          return;
        }
      }
    }

    // *****  changes for WR 040796
    // *****  changes for WR 040796
    if (!Equal(import.DisbSuppressionStatusHistory.EffectiveDate,
      entities.DisbSuppressionStatusHistory.EffectiveDate) || !
      Equal(import.DisbSuppressionStatusHistory.DiscontinueDate,
      entities.DisbSuppressionStatusHistory.DiscontinueDate))
    {
      // ***************************************************************
      // Check for a date overlap if the Collection Type is the same. Different 
      // Collection types can overlap each others dates
      // ***************************************************************
      foreach(var item in ReadDisbSuppressionStatusHistory2())
      {
        // *****  If the collection type is the same then there is an overlap 
        // error - unless they have different court orders
        if (AsChar(import.DisbSuppressionStatusHistory.Type1) == 'O' && AsChar
          (entities.ForOverlapDisbSuppressionStatusHistory.Type1) == 'O')
        {
          if (ReadLegalAction2())
          {
            if (!Equal(entities.ForOverlapLegalAction.StandardNumber,
              import.LegalAction.StandardNumber))
            {
              // *****  court orders are different so go on & look for next rule
              // that might be an overlap
              continue;
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF_RB";

            return;
          }
        }

        ExitState = "FN0000_DATE_OVERLAP_ERROR";

        return;
      }

      // *****  changes for WR 040796
    }

    // *****  changes for WR 040796
    local.New1.Type1 = import.DisbSuppressionStatusHistory.Type1;

    if (!IsEmpty(import.LegalAction.StandardNumber))
    {
      local.New1.Type1 = "O";
    }
    else
    {
      local.New1.Type1 = "C";
    }

    // *****  changes for WR 040796
    // *****  changes for WR 040796
    try
    {
      UpdateDisbSuppressionStatusHistory();

      // *****  changes for WR 040796
      export.DisbSuppressionStatusHistory.Assign(
        entities.DisbSuppressionStatusHistory);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_SUPP_STAT_NU";

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

    // *****  changes for WR 040796
    // ********************************************
    // Change of suppression type
    // ********************************************
    if (entities.LegalAction.Populated && IsEmpty
      (import.LegalAction.StandardNumber))
    {
      DisassociateLegalAction();
    }

    if (!entities.LegalAction.Populated && !
      IsEmpty(import.LegalAction.StandardNumber))
    {
      if (ReadLegalAction3())
      {
        AssociateLegalAction();
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF_RB";

        return;
      }
    }

    // ********************************************
    // Change of court order number
    // ********************************************
    if (entities.LegalAction.Populated && !
      IsEmpty(import.LegalAction.StandardNumber))
    {
      if (!Equal(import.LegalAction.StandardNumber,
        entities.LegalAction.StandardNumber))
      {
        DisassociateLegalAction();

        if (ReadLegalAction3())
        {
          AssociateLegalAction();
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF_RB";

          return;
        }
      }
    }

    // *****  changes for WR 040796
    // all exits must go here for editing
    if (Equal(export.DisbSuppressionStatusHistory.DiscontinueDate,
      local.Maximum.Date))
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate = null;
    }
  }

  private void AssociateLegalAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var lgaIdentifier = entities.New1.Identifier;

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

  private void DisassociateLegalAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("DisassociateLegalAction",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

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
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dssGeneratedId",
          import.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 9);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 10);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistory2()
  {
    entities.ForOverlapDisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetDate(
          command, "discontinueDate",
          import.DisbSuppressionStatusHistory.EffectiveDate.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.DisbSuppressionStatusHistory.DiscontinueDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "dssGeneratedId",
          import.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.ForOverlapDisbSuppressionStatusHistory.CpaType =
          db.GetString(reader, 0);
        entities.ForOverlapDisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.ForOverlapDisbSuppressionStatusHistory.
          SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ForOverlapDisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.ForOverlapDisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ForOverlapDisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.ForOverlapDisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.ForOverlapDisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ForOverlapDisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ForOverlapDisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ForOverlapDisbSuppressionStatusHistory.Type1 =
          db.GetString(reader, 10);
        entities.ForOverlapDisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 11);
        entities.ForOverlapDisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ForOverlapDisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.ForOverlapDisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.ForOverlapDisbSuppressionStatusHistory.Type1);

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
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

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ForOverlapDisbSuppressionStatusHistory.Populated);
    entities.ForOverlapLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ForOverlapDisbSuppressionStatusHistory.LgaIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ForOverlapLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ForOverlapLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ForOverlapLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.New1.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.New1.Identifier = db.GetInt32(reader, 0);
        entities.New1.StandardNumber = db.GetNullableString(reader, 1);
        entities.New1.Populated = true;
      });
  }

  private void UpdateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var effectiveDate = import.DisbSuppressionStatusHistory.EffectiveDate;
    var discontinueDate = import.DisbSuppressionStatusHistory.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var type1 = local.New1.Type1;
    var reasonText = import.DisbSuppressionStatusHistory.ReasonText ?? "";

    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("UpdateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.LastUpdatedBy = lastUpdatedBy;
    entities.DisbSuppressionStatusHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbSuppressionStatusHistory.Type1 = type1;
    entities.DisbSuppressionStatusHistory.ReasonText = reasonText;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public DisbSuppressionStatusHistory Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private CollectionType collectionType;
    private DisbSuppressionStatusHistory hidden;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbSuppressionStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkArea maximum;
    private DisbSuppressionStatusHistory new1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ForOverlapLegalAction.
    /// </summary>
    [JsonPropertyName("forOverlapLegalAction")]
    public LegalAction ForOverlapLegalAction
    {
      get => forOverlapLegalAction ??= new();
      set => forOverlapLegalAction = value;
    }

    /// <summary>
    /// A value of ForOverlapCollectionType.
    /// </summary>
    [JsonPropertyName("forOverlapCollectionType")]
    public CollectionType ForOverlapCollectionType
    {
      get => forOverlapCollectionType ??= new();
      set => forOverlapCollectionType = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LegalAction New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of ForOverlapDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("forOverlapDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory ForOverlapDisbSuppressionStatusHistory
    {
      get => forOverlapDisbSuppressionStatusHistory ??= new();
      set => forOverlapDisbSuppressionStatusHistory = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private LegalAction forOverlapLegalAction;
    private CollectionType forOverlapCollectionType;
    private LegalAction legalAction;
    private LegalAction new1;
    private CollectionType collectionType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbSuppressionStatusHistory forOverlapDisbSuppressionStatusHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
