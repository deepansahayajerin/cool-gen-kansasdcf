// Program: SP_CAB_UPDATE_CASE_ASSGN, ID: 372318290, model: 746.
// Short name: SWE01873
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_CASE_ASSGN.
/// </summary>
[Serializable]
public partial class SpCabUpdateCaseAssgn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_CASE_ASSGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateCaseAssgn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateCaseAssgn.
  /// </summary>
  public SpCabUpdateCaseAssgn(IContext context, Import import, Export export):
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
    // 10/24/96 Rick Delgado               Initial Development
    // 11/03/97  Siraj Konkader            Performance tuning. Replaced 
    // persistent views, moved edits to PRAD.
    // ------------------------------------------------------------
    if (ReadCaseAssignment())
    {
      try
      {
        UpdateCaseAssignment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ASSIGNMENT_NU";

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
      ExitState = "CASE_ASSIGNMENT_NF";
    }
  }

  private bool ReadCaseAssignment()
  {
    entities.ExistingCaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          import.CaseAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ExistingCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 7);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 8);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 9);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 10);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 11);
        entities.ExistingCaseAssignment.Populated = true;
      });
  }

  private void UpdateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseAssignment.Populated);

    var reasonCode = import.CaseAssignment.ReasonCode;
    var overrideInd = import.CaseAssignment.OverrideInd;
    var discontinueDate = import.CaseAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingCaseAssignment.Populated = false;
    Update("UpdateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCaseAssignment.CreatedTimestamp.GetValueOrDefault());
          
        db.SetInt32(command, "spdId", entities.ExistingCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ExistingCaseAssignment.OffId);
        db.
          SetString(command, "ospCode", entities.ExistingCaseAssignment.OspCode);
          
        db.SetDate(
          command, "ospDate",
          entities.ExistingCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.ExistingCaseAssignment.CasNo);
      });

    entities.ExistingCaseAssignment.ReasonCode = reasonCode;
    entities.ExistingCaseAssignment.OverrideInd = overrideInd;
    entities.ExistingCaseAssignment.DiscontinueDate = discontinueDate;
    entities.ExistingCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCaseAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCaseAssignment.Populated = true;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    private Case1 case1;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
    private CaseAssignment existingCaseAssignment;
  }
#endregion
}
