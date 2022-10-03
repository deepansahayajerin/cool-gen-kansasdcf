// Program: SP_CAB_UPDATE_OSP_ALERT_OPT_FLAG, ID: 372068722, model: 746.
// Short name: SWE01763
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_OSP_ALERT_OPT_FLAG.
/// </summary>
[Serializable]
public partial class SpCabUpdateOspAlertOptFlag: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_OSP_ALERT_OPT_FLAG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateOspAlertOptFlag(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateOspAlertOptFlag.
  /// </summary>
  public SpCabUpdateOspAlertOptFlag(IContext context, Import import,
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
    if (ReadOfficeServiceProviderAlert())
    {
      try
      {
        UpdateOfficeServiceProviderAlert();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_OSP_ALERT_AE";

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
    else
    {
      ExitState = "SP0000_OSP_ALERT_NF";
    }
  }

  private bool ReadOfficeServiceProviderAlert()
  {
    entities.Existing.Populated = false;

    return Read("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.OptimizedFlag = db.GetNullableString(reader, 1);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateOfficeServiceProviderAlert()
  {
    var optimizedFlag = import.OfficeServiceProviderAlert.OptimizedFlag ?? "";

    entities.Existing.Populated = false;
    Update("UpdateOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetNullableString(command, "optimizedFlag", optimizedFlag);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Existing.SystemGeneratedIdentifier);
      });

    entities.Existing.OptimizedFlag = optimizedFlag;
    entities.Existing.Populated = true;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    private OfficeServiceProviderAlert officeServiceProviderAlert;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public OfficeServiceProviderAlert Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private OfficeServiceProviderAlert existing;
  }
#endregion
}
