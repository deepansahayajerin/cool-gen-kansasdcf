// Program: OE_CREATE_URA_ADJUSTMENT, ID: 374478093, model: 746.
// Short name: SWE02535
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_CREATE_URA_ADJUSTMENT.
/// </summary>
[Serializable]
public partial class OeCreateUraAdjustment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_URA_ADJUSTMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateUraAdjustment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateUraAdjustment.
  /// </summary>
  public OeCreateUraAdjustment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
    {
      local.ImHouseholdMbrMnthlySum.UraAmount =
        import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
    }
    else
    {
      local.ImHouseholdMbrMnthlySum.UraMedicalAmount =
        import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
    }

    try
    {
      CreateImHouseholdMbrMnthlyAdj();

      try
      {
        UpdateImHouseholdMbrMnthlySum();
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0000_UPDATE_HH_MTHLY_SUM_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OE0000_IM_HH_MBR_MNTHLY_SUM_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OE0000_URA_ADJUSTMENTS_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OE0000_CREATE_ADJST_FAIL_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateImHouseholdMbrMnthlyAdj()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);

    var type1 = import.ImHouseholdMbrMnthlyAdj.Type1;
    var adjustmentAmount = import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount;
    var levelAppliedTo = import.ImHouseholdMbrMnthlyAdj.LevelAppliedTo;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = import.Per.ImhAeCaseNo;
    var cspNumber = import.Per.CspNumber;
    var imsMonth = import.Per.Month;
    var imsYear = import.Per.Year;
    var adjustmentReason = import.ImHouseholdMbrMnthlyAdj.AdjustmentReason;

    entities.ImHouseholdMbrMnthlyAdj.Populated = false;
    Update("CreateImHouseholdMbrMnthlyAdj",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "adjustmentAmt", adjustmentAmount);
        db.SetString(command, "levelAppliedTo", levelAppliedTo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "imsMonth", imsMonth);
        db.SetInt32(command, "imsYear", imsYear);
        db.SetString(command, "adjustmentReason", adjustmentReason);
      });

    entities.ImHouseholdMbrMnthlyAdj.Type1 = type1;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount = adjustmentAmount;
    entities.ImHouseholdMbrMnthlyAdj.LevelAppliedTo = levelAppliedTo;
    entities.ImHouseholdMbrMnthlyAdj.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlyAdj.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlyAdj.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlyAdj.ImsMonth = imsMonth;
    entities.ImHouseholdMbrMnthlyAdj.ImsYear = imsYear;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentReason = adjustmentReason;
    entities.ImHouseholdMbrMnthlyAdj.Populated = true;
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);

    var uraAmount =
      import.Per.UraAmount.GetValueOrDefault() +
      local.ImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
    var uraMedicalAmount =
      import.Per.UraMedicalAmount.GetValueOrDefault() +
      local.ImHouseholdMbrMnthlySum.UraMedicalAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    import.Per.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableDecimal(command, "uraMedicalAmount", uraMedicalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "year0", import.Per.Year);
        db.SetInt32(command, "month0", import.Per.Month);
        db.SetString(command, "imhAeCaseNo", import.Per.ImhAeCaseNo);
        db.SetString(command, "cspNumber", import.Per.CspNumber);
      });

    import.Per.UraAmount = uraAmount;
    import.Per.UraMedicalAmount = uraMedicalAmount;
    import.Per.LastUpdatedBy = lastUpdatedBy;
    import.Per.LastUpdatedTmst = lastUpdatedTmst;
    import.Per.Populated = true;
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
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public ImHouseholdMbrMnthlySum Per
    {
      get => per ??= new();
      set => per = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    private ImHouseholdMbrMnthlySum per;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
  }
#endregion
}
