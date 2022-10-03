// Program: LE_DELETE_HEARING, ID: 372012093, model: 746.
// Short name: SWE00753
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_HEARING.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes Hearing.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteHearing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_HEARING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteHearing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteHearing.
  /// </summary>
  public LeDeleteHearing(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/30/95	Stephen Benton			Initial Code
    // ------------------------------------------------------------
    if (ReadHearing())
    {
      DeleteHearing();
    }
    else
    {
      ExitState = "HEARING_NF";
    }
  }

  private void DeleteHearing()
  {
    Update("DeleteHearing#1",
      (db, command) =>
      {
        db.SetInt32(
          command, "hrgGeneratedId",
          entities.Hearing.SystemGeneratedIdentifier);
      });

    Update("DeleteHearing#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "hrgGeneratedId",
          entities.Hearing.SystemGeneratedIdentifier);
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetInt32(
          command, "hearingId", import.Hearing.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.Populated = true;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private Hearing hearing;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private Hearing hearing;
  }
#endregion
}
