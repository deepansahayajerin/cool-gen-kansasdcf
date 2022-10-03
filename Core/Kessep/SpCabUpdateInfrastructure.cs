// Program: SP_CAB_UPDATE_INFRASTRUCTURE, ID: 372132419, model: 746.
// Short name: SWE02238
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_INFRASTRUCTURE.
/// </summary>
[Serializable]
public partial class SpCabUpdateInfrastructure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_INFRASTRUCTURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateInfrastructure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateInfrastructure.
  /// </summary>
  public SpCabUpdateInfrastructure(IContext context, Import import,
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
    // -------------------------------------------------------
    // 09/27/2006    J Bahre    PR285545    Replaced
    // Exit State 'Infrastructure record not found' with a new Exit State so 
    // problem
    // with error on screen can be located more easily.
    // -------------------------------------------------------
    if (Equal(import.Infrastructure.LastUpdatedTimestamp,
      local.Infrastructure.LastUpdatedTimestamp) || IsEmpty
      (import.Infrastructure.LastUpdatedBy))
    {
      local.Infrastructure.LastUpdatedBy = global.UserId;
      local.Infrastructure.LastUpdatedTimestamp = Now();
    }
    else
    {
      local.Infrastructure.LastUpdatedBy =
        import.Infrastructure.LastUpdatedBy ?? "";
      local.Infrastructure.LastUpdatedTimestamp =
        import.Infrastructure.LastUpdatedTimestamp;
    }

    if (ReadInfrastructure())
    {
      try
      {
        UpdateInfrastructure();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            // -------------------------------------------------------
            // JLB  PR285545    09/27/2006 Added new exit states.
            // -------------------------------------------------------
            ExitState = "SP0000_INFRASTRUCTURE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_INFRASTRUCTURE_PV";

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
      ExitState = "INFRASTRUCTURE_NF_6";
    }
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 5);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 6);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 7);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 9);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 10);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 11);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 12);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 13);
        entities.Infrastructure.UserId = db.GetString(reader, 14);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 17);
        entities.Infrastructure.Function = db.GetNullableString(reader, 18);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 19);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 20);
        entities.Infrastructure.Populated = true;
      });
  }

  private void UpdateInfrastructure()
  {
    var situationNumber = import.Infrastructure.SituationNumber;
    var processStatus = import.Infrastructure.ProcessStatus;
    var reasonCode = import.Infrastructure.ReasonCode;
    var businessObjectCd = import.Infrastructure.BusinessObjectCd;
    var denormNumeric12 =
      import.Infrastructure.DenormNumeric12.GetValueOrDefault();
    var denormText12 = import.Infrastructure.DenormText12 ?? "";
    var denormDate = import.Infrastructure.DenormDate;
    var denormTimestamp = import.Infrastructure.DenormTimestamp;
    var initiatingStateCode = import.Infrastructure.InitiatingStateCode;
    var csenetInOutCode = import.Infrastructure.CsenetInOutCode;
    var caseNumber = import.Infrastructure.CaseNumber ?? "";
    var csePersonNumber = import.Infrastructure.CsePersonNumber ?? "";
    var caseUnitNumber =
      import.Infrastructure.CaseUnitNumber.GetValueOrDefault();
    var userId = import.Infrastructure.UserId;
    var lastUpdatedBy = local.Infrastructure.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Infrastructure.LastUpdatedTimestamp;
    var referenceDate = import.Infrastructure.ReferenceDate;
    var function = import.Infrastructure.Function ?? "";
    var caseUnitState = import.Infrastructure.CaseUnitState ?? "";
    var detail = import.Infrastructure.Detail ?? "";

    entities.Infrastructure.Populated = false;
    Update("UpdateInfrastructure",
      (db, command) =>
      {
        db.SetInt32(command, "situationNumber", situationNumber);
        db.SetString(command, "processStatus", processStatus);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "businessObjectCd", businessObjectCd);
        db.SetNullableInt64(command, "denormNumeric12", denormNumeric12);
        db.SetNullableString(command, "denormText12", denormText12);
        db.SetNullableDate(command, "denormDate", denormDate);
        db.SetNullableDateTime(command, "denormTimestamp", denormTimestamp);
        db.SetString(command, "initiatingStCd", initiatingStateCode);
        db.SetString(command, "csenetInOutCode", csenetInOutCode);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "csePersonNum", csePersonNumber);
        db.SetNullableInt32(command, "caseUnitNum", caseUnitNumber);
        db.SetString(command, "userId", userId);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableDate(command, "referenceDate", referenceDate);
        db.SetNullableString(command, "function", function);
        db.SetNullableString(command, "caseUnitState", caseUnitState);
        db.SetNullableString(command, "detail", detail);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.SituationNumber = situationNumber;
    entities.Infrastructure.ProcessStatus = processStatus;
    entities.Infrastructure.ReasonCode = reasonCode;
    entities.Infrastructure.BusinessObjectCd = businessObjectCd;
    entities.Infrastructure.DenormNumeric12 = denormNumeric12;
    entities.Infrastructure.DenormText12 = denormText12;
    entities.Infrastructure.DenormDate = denormDate;
    entities.Infrastructure.DenormTimestamp = denormTimestamp;
    entities.Infrastructure.InitiatingStateCode = initiatingStateCode;
    entities.Infrastructure.CsenetInOutCode = csenetInOutCode;
    entities.Infrastructure.CaseNumber = caseNumber;
    entities.Infrastructure.CsePersonNumber = csePersonNumber;
    entities.Infrastructure.CaseUnitNumber = caseUnitNumber;
    entities.Infrastructure.UserId = userId;
    entities.Infrastructure.LastUpdatedBy = lastUpdatedBy;
    entities.Infrastructure.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Infrastructure.ReferenceDate = referenceDate;
    entities.Infrastructure.Function = function;
    entities.Infrastructure.CaseUnitState = caseUnitState;
    entities.Infrastructure.Detail = detail;
    entities.Infrastructure.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }
#endregion
}
