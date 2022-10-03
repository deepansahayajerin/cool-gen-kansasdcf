// Program: DELETE_COST_RECOVERY_FEE_INFO, ID: 371810382, model: 746.
// Short name: SWE00175
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DELETE_COST_RECOVERY_FEE_INFO.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class DeleteCostRecoveryFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_COST_RECOVERY_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteCostRecoveryFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteCostRecoveryFeeInfo.
  /// </summary>
  public DeleteCostRecoveryFeeInfo(IContext context, Import import,
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
    if (ReadTribunal())
    {
      if (ReadTribunalFeeInformation())
      {
        DeleteTribunalFeeInformation();
      }
      else
      {
        ExitState = "TRIBUNAL_FEE_INFORMATION_NF";
      }
    }
    else
    {
      ExitState = "TRIBUNAL_NF";
    }
  }

  private void DeleteTribunalFeeInformation()
  {
    Update("DeleteTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.TribunalFeeInformation.TrbId);
        db.SetInt32(
          command, "tribunalFeeId",
          entities.TribunalFeeInformation.SystemGeneratedIdentifier);
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(
          command, "tribunalFeeId",
          import.TribunalFeeInformation.SystemGeneratedIdentifier);
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.TribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.TribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TribunalFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.TribunalFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.TribunalFeeInformation.Populated = true;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private Tribunal tribunal;
    private TribunalFeeInformation tribunalFeeInformation;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private Tribunal tribunal;
    private TribunalFeeInformation tribunalFeeInformation;
  }
#endregion
}
