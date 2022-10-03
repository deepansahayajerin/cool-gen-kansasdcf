// Program: FN_CAB_VALIDATE_LEGAL_ACTION, ID: 371739418, model: 746.
// Short name: SWE00300
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_VALIDATE_LEGAL_ACTION.
/// </summary>
[Serializable]
public partial class FnCabValidateLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_VALIDATE_LEGAL_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabValidateLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabValidateLegalAction.
  /// </summary>
  public FnCabValidateLegalAction(IContext context, Import import, Export export)
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
    // *********************************************
    // This acblk needs a complete cleanup.
    // 7/6/99 - b adams  -  read properties set
    // *********************************************
    if (import.LegalAction.Identifier != 0)
    {
      if (ReadLegalAction1())
      {
        export.LegalAction.Assign(import.LegalAction);

        if (AsChar(entities.LegalAction.Classification) != 'J')
        {
          ExitState = "FN0000_LEG_ACT_MUST_BE_JUDGEMENT";

          return;
        }

        // ----------------------------------------------------
        // Determine if legal action has at least one detail.
        // If no details exist, send back warning message.
        // JLK  09/22/99
        // ----------------------------------------------------
        if (!ReadLegalActionDetail())
        {
          ExitState = "LEGAL_ACTION_DETAIL_NF";
        }
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";
      }

      return;
    }

    foreach(var item in ReadLegalAction2())
    {
      // ----------------------------------------------------
      // Determine if legal action has at least one detail.
      // If no details exist, send back warning message.
      // JLK  09/22/99
      // ----------------------------------------------------
      if (ReadLegalActionDetail())
      {
        export.LegalAction.Assign(entities.LegalAction);
        ExitState = "ACO_NN0000_ALL_OK";

        return;
      }
      else
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        continue;
      }
    }

    if (!IsExitState("LEGAL_ACTION_DETAIL_NF"))
    {
      ExitState = "FN0000_LEGAL_ACTION_J_NF";
    }
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of HardcodedJudgement.
    /// </summary>
    [JsonPropertyName("hardcodedJudgement")]
    public LegalAction HardcodedJudgement
    {
      get => hardcodedJudgement ??= new();
      set => hardcodedJudgement = value;
    }

    private LegalAction legalAction;
    private LegalAction hardcodedJudgement;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
