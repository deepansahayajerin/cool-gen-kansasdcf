// Program: LE_LAPP_VALIDATE_LEGAL_APPEAL, ID: 371973997, model: 746.
// Short name: SWE00791
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
/// A program: LE_LAPP_VALIDATE_LEGAL_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block validates the legal appeal details
/// </para>
/// </summary>
[Serializable]
public partial class LeLappValidateLegalAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPP_VALIDATE_LEGAL_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLappValidateLegalAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLappValidateLegalAppeal.
  /// </summary>
  public LeLappValidateLegalAppeal(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Change log:
    // When		Who
    // Why
    // 12/17/98	P McElderry
    // Made attorney's first name a mandatory field requirement.
    // Fixed  "Docket Number", "Docketing Stmt Filed Date", "Ext
    // Request Date", "Date Ext Granted", "Oral Argument Date",
    // and "Decision Date" field logic requirements.
    // Commented out appellant brief date and reply brief date as
    // these were specified having no validation logic.
    // --------------------------------------------------------------
    export.Errors.Index = -1;

    if (import.LegalAction.Identifier == 0)
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 1;
      export.LastErrorEntry.Count = export.Errors.Index + 1;

      return;
    }

    if (!ReadLegalAction())
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 18;
      export.LastErrorEntry.Count = export.Errors.Index + 1;

      return;
    }

    if (Equal(import.UserAction.Command, "UPDATE") || Equal
      (import.UserAction.Command, "DELETE"))
    {
      if (!ReadAppeal())
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 16;
        export.LastErrorEntry.Count = export.Errors.Index + 1;

        return;
      }
    }

    if (Equal(import.UserAction.Command, "DELETE"))
    {
      // -----------------------------
      // No other validation required
      // -----------------------------
      return;
    }

    if (IsEmpty(import.To.JudicialDivision) || IsEmpty
      (import.To.JudicialDivision) || IsEmpty(import.To.Name))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 22;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (Lt(new DateTime(1, 1, 1), import.Appeal.DocketingStmtFiledDate))
    {
      if (IsEmpty(import.Appeal.DocketNumber))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 2;
        export.LastErrorEntry.Count = export.Errors.Index + 1;
      }
    }
    else if (!IsEmpty(import.Appeal.DocketNumber))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 3;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (Lt(Now().Date, import.Appeal.DocketingStmtFiledDate))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 3;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    // ------------------------------------------------------------------
    // Error codes 4 and 17 have been deleted from here. 4 was
    // for tribunal name required and 17 was for tribunal division
    // requirement.
    // ---------------------------------------------------------------------
    if (IsEmpty(import.Tribunal.Name))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 4;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }
    else if (!ReadTribunal())
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 4;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (IsEmpty(import.Appeal.FiledByLastName))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 5;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (IsEmpty(import.Appeal.FiledByFirstName))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 20;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (IsEmpty(import.Appeal.AttorneyLastName))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 6;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (IsEmpty(import.Appeal.AttorneyFirstName))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 7;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (!Lt(new DateTime(1, 1, 1), import.Appeal.AppealDate))
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 8;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }

    if (Lt(new DateTime(1, 1, 1), import.Appeal.ExtentionReqGrantedDate))
    {
      if (Lt(Now().Date, import.Appeal.ExtentionReqGrantedDate) || Lt
        (import.Appeal.ExtentionReqGrantedDate, import.LegalAction.FiledDate))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 9;
        export.LastErrorEntry.Count = export.Errors.Index + 1;
      }
    }

    if (Lt(new DateTime(1, 1, 1), import.Appeal.DateExtensionGranted))
    {
      if (Lt(Now().Date, import.Appeal.DateExtensionGranted) || Lt
        (import.Appeal.DateExtensionGranted, import.LegalAction.FiledDate) || Lt
        (import.Appeal.DateExtensionGranted,
        import.Appeal.ExtentionReqGrantedDate))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 10;
        export.LastErrorEntry.Count = export.Errors.Index + 1;
      }
    }

    if (Lt(new DateTime(1, 1, 1), import.Appeal.OralArgumentDate))
    {
      if (Lt(Now().Date, import.Appeal.OralArgumentDate) || Lt
        (import.Appeal.OralArgumentDate, import.LegalAction.FiledDate))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 13;
        export.LastErrorEntry.Count = export.Errors.Index + 1;
      }
    }

    if (Lt(new DateTime(1, 1, 1), import.Appeal.DecisionDate))
    {
      if (Lt(Now().Date, import.Appeal.DecisionDate) || Lt
        (import.Appeal.DecisionDate, import.LegalAction.FiledDate))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 14;
        export.LastErrorEntry.Count = export.Errors.Index + 1;
      }
    }

    if (!IsEmpty(import.Appeal.FurtherAppealIndicator) && AsChar
      (import.Appeal.FurtherAppealIndicator) != 'Y' && AsChar
      (import.Appeal.FurtherAppealIndicator) != 'N')
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.DetailErrorCode.Count = 15;
      export.LastErrorEntry.Count = export.Errors.Index + 1;
    }
  }

  private bool ReadAppeal()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "appealId", import.Appeal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetString(command, "tribunalNm", import.Tribunal.Name);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
        entities.Tribunal.Populated = true;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Tribunal To
    {
      get => to ??= new();
      set => to = value;
    }

    private Common userAction;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private Appeal appeal;
    private Tribunal to;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of LastErrorEntry.
    /// </summary>
    [JsonPropertyName("lastErrorEntry")]
    public Common LastErrorEntry
    {
      get => lastErrorEntry ??= new();
      set => lastErrorEntry = value;
    }

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
    }

    private Common lastErrorEntry;
    private Array<ErrorsGroup> errors;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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

    private Tribunal tribunal;
    private LegalAction existingLegalAction;
    private Appeal existingAppeal;
  }
#endregion
}
