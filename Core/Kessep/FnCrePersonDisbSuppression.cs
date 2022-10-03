// Program: FN_CRE_PERSON_DISB_SUPPRESSION, ID: 371755038, model: 746.
// Short name: SWE00335
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CRE_PERSON_DISB_SUPPRESSION.
/// </summary>
[Serializable]
public partial class FnCrePersonDisbSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRE_PERSON_DISB_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrePersonDisbSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrePersonDisbSuppression.
  /// </summary>
  public FnCrePersonDisbSuppression(IContext context, Import import,
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
    //                  Developed for KESSEP by MTW
    //                   D. M. Nilsen  09/06/95
    // *******************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    MoveDisbSuppressionStatusHistory(import.DisbSuppressionStatusHistory,
      export.DisbSuppressionStatusHistory);

    // ***** EDIT AREA *****
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(export.DisbSuppressionStatusHistory.DiscontinueDate,
      local.Maximum.Date))
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate = local.Zero.Date;
    }

    if (ReadCsePersonAccount())
    {
      // now check for overlap dates
      // ****************************************************************
      // Put in the OR condition in the READ EACH below to cover the rest of the
      // possible timeframes. RK 10/15/1998
      // ****************************************************************
      foreach(var item in ReadDisbSuppressionStatusHistory())
      {
        // *****  changes for WR 040796
        if (AsChar(entities.DisbSuppressionStatusHistory.Type1) == 'O')
        {
          // Found overlapping O rule.
          if (ReadLegalAction2())
          {
            if (IsEmpty(import.LegalAction.StandardNumber))
            {
              ExitState = "FN0000_DATE_OVERLAP_ERROR";

              return;
            }
            else if (Equal(entities.LegalAction.StandardNumber,
              import.LegalAction.StandardNumber))
            {
              ExitState = "FN0000_SUPPR_LEGAL_ACT_OVERLAP";

              return;
            }
            else
            {
              // O types can overlap as long as the legal action is different.
              continue;
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF_RB";

            return;
          }
        }
        else
        {
          // Found overlapping P rule.
          ExitState = "FN0000_DATE_OVERLAP_ERROR";

          return;
        }

        // *****  changes for WR 040796
      }

      // *****  changes for WR 040796
      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        local.DisbSuppressionStatusHistory.Type1 = "O";

        if (ReadLegalAction1())
        {
          // continue
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF_RB";

          return;
        }
      }
      else
      {
        local.DisbSuppressionStatusHistory.Type1 = "P";
      }

      // *****  changes for WR 040796
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

        // *****  changes for WR 040796
        if (!IsEmpty(import.LegalAction.StandardNumber))
        {
          AssociateLegalAction();
        }

        // *****  changes for WR 040796
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
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";
    }
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

    useImport.DateWorkArea.Date = local.Maximum.Date;

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

  private void CreateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignDisbSupprId();
    var effectiveDate = import.DisbSuppressionStatusHistory.EffectiveDate;
    var discontinueDate = import.DisbSuppressionStatusHistory.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = local.DisbSuppressionStatusHistory.Type1;
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
          command, "discontinueDate",
          import.DisbSuppressionStatusHistory.EffectiveDate.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          import.DisbSuppressionStatusHistory.DiscontinueDate.
            GetValueOrDefault());
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

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

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DateWorkArea zero;
    private DateWorkArea dateWorkArea;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private CsePersonAccount obligee;
    private CollectionType collectionType;
    private LegalAction legalAction;
  }
#endregion
}
