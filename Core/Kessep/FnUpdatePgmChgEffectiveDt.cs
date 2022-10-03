// Program: FN_UPDATE_PGM_CHG_EFFECTIVE_DT, ID: 372264675, model: 746.
// Short name: SWE00672
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_PGM_CHG_EFFECTIVE_DT.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This common action block updates the 'CHANGE DATE' attribute of 
/// Person Program when the Person Program Change processing is completed.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdatePgmChgEffectiveDt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_PGM_CHG_EFFECTIVE_DT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdatePgmChgEffectiveDt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdatePgmChgEffectiveDt.
  /// </summary>
  public FnUpdatePgmChgEffectiveDt(IContext context, Import import,
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
    try
    {
      UpdateSupported();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PERSON_PROGRAM_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PERSON_PROGRAM_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UpdateSupported()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);

    var lastUpdatedBy = import.Supported.LastUpdatedBy ?? "";
    var lastUpdatedTmst = import.Supported.LastUpdatedTmst;
    var pgmChgEffectiveDate = import.Supported.PgmChgEffectiveDate;
    var triggerType = import.Supported.TriggerType ?? "";

    import.Pers.Populated = false;
    Update("UpdateSupported",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", triggerType);
        db.SetString(command, "cspNumber", import.Pers.CspNumber);
        db.SetString(command, "type", import.Pers.Type1);
      });

    import.Pers.LastUpdatedBy = lastUpdatedBy;
    import.Pers.LastUpdatedTmst = lastUpdatedTmst;
    import.Pers.PgmChgEffectiveDate = pgmChgEffectiveDate;
    import.Pers.TriggerType = triggerType;
    import.Pers.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Pers.
    /// </summary>
    [JsonPropertyName("pers")]
    public CsePersonAccount Pers
    {
      get => pers ??= new();
      set => pers = value;
    }

    private CsePersonAccount supported;
    private CsePersonAccount pers;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
