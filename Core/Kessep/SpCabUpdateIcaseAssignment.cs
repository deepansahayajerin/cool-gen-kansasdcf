// Program: SP_CAB_UPDATE_ICASE_ASSIGNMENT, ID: 372757456, model: 746.
// Short name: SWE01999
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_ICASE_ASSIGNMENT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This acblk updates the Monitored Activity Assignment entity
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateIcaseAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ICASE_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateIcaseAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateIcaseAssignment.
  /// </summary>
  public SpCabUpdateIcaseAssignment(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 06/18/1999 swsrkeh               Initial Development
    // ------------------------------------------------------------
    if (ReadInterstateCaseAssignment())
    {
      try
      {
        UpdateInterstateCaseAssignment();
        export.InterstateCaseAssignment.
          Assign(entities.InterstateCaseAssignment);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_ICASE_ASSGMNT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_ICASE_ASSGMNT_PV";

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
      ExitState = "SP0000_ICASE_ASGNMT_NF";
    }
  }

  private bool ReadInterstateCaseAssignment()
  {
    entities.InterstateCaseAssignment.Populated = false;

    return Read("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "icsDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(command, "icsNo", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetDateTime(
          command, "createdTimestamp",
          import.InterstateCaseAssignment.CreatedTimestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.InterstateCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateCaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 10);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 12);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 13);
        entities.InterstateCaseAssignment.Populated = true;
      });
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);

    var reasonCode = import.InterstateCaseAssignment.ReasonCode;
    var overrideInd = import.InterstateCaseAssignment.OverrideInd;
    var effectiveDate = import.InterstateCaseAssignment.EffectiveDate;
    var discontinueDate = import.InterstateCaseAssignment.DiscontinueDate;
    var createdBy = import.InterstateCaseAssignment.CreatedBy;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.InterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.InterstateCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.InterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.InterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.InterstateCaseAssignment.IcsDate.GetValueOrDefault());
        db.SetInt64(command, "icsNo", entities.InterstateCaseAssignment.IcsNo);
      });

    entities.InterstateCaseAssignment.ReasonCode = reasonCode;
    entities.InterstateCaseAssignment.OverrideInd = overrideInd;
    entities.InterstateCaseAssignment.EffectiveDate = effectiveDate;
    entities.InterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.InterstateCaseAssignment.CreatedBy = createdBy;
    entities.InterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateCaseAssignment.Populated = true;
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
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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

    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase interstateCase;
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
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    private InterstateCaseAssignment interstateCaseAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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

    private InterstateCase interstateCase;
    private InterstateCaseAssignment interstateCaseAssignment;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
