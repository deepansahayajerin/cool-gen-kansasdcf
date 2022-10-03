// Program: SP_CAB_UPDATE_LACT_ASSIGNMENT, ID: 372318285, model: 746.
// Short name: SWE01902
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_LACT_ASSIGNMENT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This acblk updates the Monitored Activity Assignment entity
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateLactAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_LACT_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateLactAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateLactAssignment.
  /// </summary>
  public SpCabUpdateLactAssignment(IContext context, Import import,
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
    // *********************************************
    // Date	By	IDCR #	Description
    // xxxxxx	??????		Initial creation.
    // 110397  Siraj		Move edit for overlapping assignment to PRAD
    //  			Replaced extended read on LA, OSP and LA Assignment w/ single READ 
    // for LA Assignment
    // *********************************************
    if (ReadLegalActionAssigment())
    {
      try
      {
        UpdateLegalActionAssigment();
        export.LegalActionAssigment.Assign(entities.LegalActionAssigment);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_ASSIGNMENT_NU";

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
      ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
    }
  }

  private bool ReadLegalActionAssigment()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.LegalActionAssigment.CreatedTimestamp.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "ospRoleCode", import.OfficeServiceProvider.RoleCode);
        db.SetNullableInt32(
          command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private void UpdateLegalActionAssigment()
  {
    var discontinueDate = import.LegalActionAssigment.DiscontinueDate;
    var reasonCode = import.LegalActionAssigment.ReasonCode;
    var overrideInd = import.LegalActionAssigment.OverrideInd;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalActionAssigment.Populated = false;
    Update("UpdateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.LegalActionAssigment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.LegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.LegalActionAssigment.ReasonCode = reasonCode;
    entities.LegalActionAssigment.OverrideInd = overrideInd;
    entities.LegalActionAssigment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionAssigment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalActionAssigment.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    private LegalActionAssigment legalActionAssigment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
    private Case1 case1;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
