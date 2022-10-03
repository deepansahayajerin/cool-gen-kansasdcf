// Program: OE_MAINTAIN_IM_HOUSEHOLD, ID: 374449490, model: 746.
// Short name: SWE02874
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_MAINTAIN_IM_HOUSEHOLD.
/// </summary>
[Serializable]
public partial class OeMaintainImHousehold: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MAINTAIN_IM_HOUSEHOLD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMaintainImHousehold(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMaintainImHousehold.
  /// </summary>
  public OeMaintainImHousehold(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************************************************
    // Date          Developer	Description
    // 05/04/2000      SWSRDCV    	Initial Creation
    // *********************************************************************************
    // ****************************************************************************
    // 
    // Initialize all fields used throughout this module.
    // 
    // ****************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    switch(TrimEnd(import.Action.ActionEntry))
    {
      case "AD":
        try
        {
          CreateImHousehold();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "IM_HOUSEHOLD_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "OE0000_IM_HOUSEHOLD_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "UP":
        if (ReadImHousehold())
        {
          try
          {
            UpdateImHousehold();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "IM_HOUSEHOLD_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OE0000_IM_HOUSEHOLD_PV";

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
          ExitState = "OE0000_IM_HOUSEHOLD_NF";
        }

        break;
      default:
        break;
    }
  }

  private void CreateImHousehold()
  {
    var aeCaseNo = import.ImHousehold.AeCaseNo;
    var caseStatus = import.ImHousehold.CaseStatus;
    var statusDate = import.ImHousehold.StatusDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.Initialized.LastUpdatedTimestamp;
    var firstBenefitDate = import.ImHousehold.FirstBenefitDate;

    entities.ImHousehold.Populated = false;
    Update("CreateImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", aeCaseNo);
        db.SetNullableInt32(command, "householdSize", 0);
        db.SetString(command, "caseStatus", caseStatus);
        db.SetDate(command, "statusDate", statusDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableDate(command, "firstBenDate", firstBenefitDate);
        db.SetNullableString(command, "type", "");
        db.SetNullableString(command, "calculateFlag", "");
        db.SetNullableString(command, "multiCaseInd", "");
      });

    entities.ImHousehold.AeCaseNo = aeCaseNo;
    entities.ImHousehold.ZdelHouseholdSize = 0;
    entities.ImHousehold.CaseStatus = caseStatus;
    entities.ImHousehold.StatusDate = statusDate;
    entities.ImHousehold.CreatedBy = createdBy;
    entities.ImHousehold.CreatedTimestamp = createdTimestamp;
    entities.ImHousehold.LastUpdatedBy = "";
    entities.ImHousehold.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ImHousehold.FirstBenefitDate = firstBenefitDate;
    entities.ImHousehold.ZdelType = "";
    entities.ImHousehold.ZdelCalculateFlag = "";
    entities.ImHousehold.ZdelMultiCaseIndicator = "";
    entities.ImHousehold.Populated = true;
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.ZdelHouseholdSize = db.GetNullableInt32(reader, 1);
        entities.ImHousehold.CaseStatus = db.GetString(reader, 2);
        entities.ImHousehold.StatusDate = db.GetDate(reader, 3);
        entities.ImHousehold.CreatedBy = db.GetString(reader, 4);
        entities.ImHousehold.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.ImHousehold.LastUpdatedBy = db.GetString(reader, 6);
        entities.ImHousehold.LastUpdatedTimestamp = db.GetDateTime(reader, 7);
        entities.ImHousehold.FirstBenefitDate = db.GetNullableDate(reader, 8);
        entities.ImHousehold.ZdelType = db.GetNullableString(reader, 9);
        entities.ImHousehold.ZdelCalculateFlag =
          db.GetNullableString(reader, 10);
        entities.ImHousehold.ZdelMultiCaseIndicator =
          db.GetNullableString(reader, 11);
        entities.ImHousehold.Populated = true;
      });
  }

  private void UpdateImHousehold()
  {
    var caseStatus = import.ImHousehold.CaseStatus;
    var statusDate = import.ImHousehold.StatusDate;
    var firstBenefitDate = import.ImHousehold.FirstBenefitDate;

    entities.ImHousehold.Populated = false;
    Update("UpdateImHousehold",
      (db, command) =>
      {
        db.SetString(command, "caseStatus", caseStatus);
        db.SetDate(command, "statusDate", statusDate);
        db.SetNullableDate(command, "firstBenDate", firstBenefitDate);
        db.SetString(command, "aeCaseNo", entities.ImHousehold.AeCaseNo);
      });

    entities.ImHousehold.CaseStatus = caseStatus;
    entities.ImHousehold.StatusDate = statusDate;
    entities.ImHousehold.FirstBenefitDate = firstBenefitDate;
    entities.ImHousehold.Populated = true;
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
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public Common Action
    {
      get => action ??= new();
      set => action = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private Common action;
    private ImHousehold imHousehold;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ImHousehold Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private ImHousehold initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private ImHousehold imHousehold;
  }
#endregion
}
