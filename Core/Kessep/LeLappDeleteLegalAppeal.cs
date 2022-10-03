// Program: LE_LAPP_DELETE_LEGAL_APPEAL, ID: 371973998, model: 746.
// Short name: SWE00788
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LAPP_DELETE_LEGAL_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes legal APPEAL.
/// </para>
/// </summary>
[Serializable]
public partial class LeLappDeleteLegalAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAPP_DELETE_LEGAL_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLappDeleteLegalAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLappDeleteLegalAppeal.
  /// </summary>
  public LeLappDeleteLegalAppeal(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Read legal action that appeal is against
    // ---------------------------------------------
    if (!ReadLegalAction())
    {
      ExitState = "ZD_LEGAL_ACTION_NF_2";
    }

    // ---------------------------------------------
    // Read the appeal along with the legal action
    // appeal associative entity.
    // ---------------------------------------------
    if (!ReadAppealLegalActionAppeal())
    {
      ExitState = "APPEAL_NF";

      return;
    }

    // ---------------------------------------------
    // Delete the association between legal action
    // and appeal.
    // ---------------------------------------------
    DeleteLegalActionAppeal();

    // ---------------------------------------------
    // If the appeal is no longer associated to any
    // legal actions (the deleted associative was
    // the last remaining one for this appeal), then
    // delete the appeal.
    // ---------------------------------------------
    if (ReadLegalActionAppeal())
    {
      return;
    }

    DeleteAppeal();
  }

  private void DeleteAppeal()
  {
    Update("DeleteAppeal#1",
      (db, command) =>
      {
        db.SetInt32(command, "aplId", entities.ExistingAppeal.Identifier);
      });

    Update("DeleteAppeal#2",
      (db, command) =>
      {
        db.SetInt32(command, "aplId", entities.ExistingAppeal.Identifier);
      });
  }

  private void DeleteLegalActionAppeal()
  {
    Update("DeleteLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "laAppealId", entities.ExistingLegalActionAppeal.Identifier);
          
        db.SetInt32(command, "aplId", entities.ExistingLegalActionAppeal.AplId);
        db.SetInt32(command, "lgaId", entities.ExistingLegalActionAppeal.LgaId);
      });
  }

  private bool ReadAppealLegalActionAppeal()
  {
    entities.ExistingLegalActionAppeal.Populated = false;
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppealLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "aplId", import.Appeal.Identifier);
        db.SetInt32(
          command, "lgaId", entities.ExistingAppealedAgainst.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionAppeal.AplId = db.GetInt32(reader, 0);
        entities.ExistingLegalActionAppeal.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLegalActionAppeal.LgaId = db.GetInt32(reader, 2);
        entities.ExistingLegalActionAppeal.Populated = true;
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.ExistingAppealedAgainst.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.
          SetInt32(command, "legalActionId", import.AppealedAgainst.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingAppealedAgainst.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppealedAgainst.Populated = true;
      });
  }

  private bool ReadLegalActionAppeal()
  {
    entities.ExistingLegalActionAppeal.Populated = false;

    return Read("ReadLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "aplId", entities.ExistingAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionAppeal.AplId = db.GetInt32(reader, 1);
        entities.ExistingLegalActionAppeal.LgaId = db.GetInt32(reader, 2);
        entities.ExistingLegalActionAppeal.Populated = true;
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
    /// A value of AppealedAgainst.
    /// </summary>
    [JsonPropertyName("appealedAgainst")]
    public LegalAction AppealedAgainst
    {
      get => appealedAgainst ??= new();
      set => appealedAgainst = value;
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

    private LegalAction appealedAgainst;
    private Appeal appeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ExistingAppealedAgainst.
    /// </summary>
    [JsonPropertyName("existingAppealedAgainst")]
    public LegalAction ExistingAppealedAgainst
    {
      get => existingAppealedAgainst ??= new();
      set => existingAppealedAgainst = value;
    }

    private LegalActionAppeal existingLegalActionAppeal;
    private Appeal existingAppeal;
    private LegalAction existingAppealedAgainst;
  }
#endregion
}
