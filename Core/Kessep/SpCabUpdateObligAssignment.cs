// Program: SP_CAB_UPDATE_OBLIG_ASSIGNMENT, ID: 372318284, model: 746.
// Short name: SWE02144
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_OBLIG_ASSIGNMENT.
/// </para>
/// <para>
/// This CAB is meant to be used as an elementary process to update an 
/// Obligaiton Assignment.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateObligAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_OBLIG_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateObligAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateObligAssignment.
  /// </summary>
  public SpCabUpdateObligAssignment(IContext context, Import import,
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
    if (ReadObligationAssignment())
    {
      try
      {
        UpdateObligationAssignment();
        MoveObligationAssignment(entities.ObligationAssignment,
          export.ObligationAssignment);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OBLIGATION_ASSIGNMENT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OBLIGATION_ASSIGNMENT_PV";

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
      ExitState = "OBLIGATION_ASSIGNMENT_NF";
    }
  }

  private static void MoveObligationAssignment(ObligationAssignment source,
    ObligationAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private bool ReadObligationAssignment()
  {
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.ObligationAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNo", import.CsePerson.Number);
        db.SetInt32(
          command, "otyId", import.ObligationType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ObligationAssignment.CreatedBy = db.GetString(reader, 4);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ObligationAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ObligationAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 8);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 10);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 11);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 13);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 14);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 15);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private void UpdateObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);

    var reasonCode = import.ObligationAssignment.ReasonCode;
    var overrideInd = import.ObligationAssignment.OverrideInd;
    var discontinueDate = import.ObligationAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ObligationAssignment.Populated = false;
    Update("UpdateObligationAssignment",
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
          entities.ObligationAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.ObligationAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ObligationAssignment.OffId);
        db.SetString(command, "ospCode", entities.ObligationAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.ObligationAssignment.OtyId);
        db.SetString(command, "cpaType", entities.ObligationAssignment.CpaType);
        db.SetString(command, "cspNo", entities.ObligationAssignment.CspNo);
        db.SetInt32(command, "obgId", entities.ObligationAssignment.ObgId);
      });

    entities.ObligationAssignment.ReasonCode = reasonCode;
    entities.ObligationAssignment.OverrideInd = overrideInd;
    entities.ObligationAssignment.DiscontinueDate = discontinueDate;
    entities.ObligationAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ObligationAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ObligationAssignment.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationAssignment obligationAssignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private ObligationAssignment obligationAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationAssignment obligationAssignment;
  }
#endregion
}
