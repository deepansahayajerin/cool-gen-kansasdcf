// Program: SP_CAB_CREATE_ICASE_ASSIGNMENT, ID: 372757457, model: 746.
// Short name: SWE01705
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_ICASE_ASSIGNMENT.
/// </summary>
[Serializable]
public partial class SpCabCreateIcaseAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ICASE_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateIcaseAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateIcaseAssignment.
  /// </summary>
  public SpCabCreateIcaseAssignment(IContext context, Import import,
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
    if (ReadInterstateCase())
    {
      if (ReadOfficeServiceProvider())
      {
        try
        {
          CreateInterstateCaseAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
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
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private void CreateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var reasonCode = import.InterstateCaseAssignment.ReasonCode;
    var overrideInd = import.InterstateCaseAssignment.OverrideInd;
    var effectiveDate = import.InterstateCaseAssignment.EffectiveDate;
    var discontinueDate = import.InterstateCaseAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.InterstateCaseAssignment.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      import.InterstateCaseAssignment.LastUpdatedTimestamp;
    var spdId = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospDate = entities.ExistingOfficeServiceProvider.EffectiveDate;
    var icsDate = entities.ExistingInterstateCase.TransactionDate;
    var icsNo = entities.ExistingInterstateCase.TransSerialNumber;

    entities.New1.Populated = false;
    Update("CreateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetDate(command, "icsDate", icsDate);
        db.SetInt64(command, "icsNo", icsNo);
      });

    entities.New1.ReasonCode = reasonCode;
    entities.New1.OverrideInd = overrideInd;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = lastUpdatedBy;
    entities.New1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.IcsDate = icsDate;
    entities.New1.IcsNo = icsNo;
    entities.New1.Populated = true;
  }

  private bool ReadInterstateCase()
  {
    entities.ExistingInterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.ExistingInterstateCase.TransSerialNumber =
          db.GetInt64(reader, 0);
        entities.ExistingInterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.ExistingInterstateCase.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    private InterstateCase interstateCase;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private InterstateCaseAssignment interstateCaseAssignment;
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
    /// A value of ExistingInterstateCase.
    /// </summary>
    [JsonPropertyName("existingInterstateCase")]
    public InterstateCase ExistingInterstateCase
    {
      get => existingInterstateCase ??= new();
      set => existingInterstateCase = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateCaseAssignment New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private InterstateCase existingInterstateCase;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private InterstateCaseAssignment new1;
  }
#endregion
}
