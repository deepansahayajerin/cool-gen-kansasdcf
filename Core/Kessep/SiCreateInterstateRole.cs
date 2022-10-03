// Program: SI_CREATE_INTERSTATE_ROLE, ID: 372506616, model: 746.
// Short name: SWE02137
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_INTERSTATE_ROLE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateInterstateRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_INTERSTATE_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateInterstateRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateInterstateRole.
  /// </summary>
  public SiCreateInterstateRole(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadInterstateRequest())
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CASE_ROLE_CHILD_NF";

      return;
    }

    ReadInterstateRole();

    // >>
    // Do not create a another role for the same person.
    if (entities.InterstateRole.Populated)
    {
      return;
    }

    local.InterstateRole.Identifier = entities.InterstateRole.Identifier + 1;
    local.InterstateRole.Type1 = "CH";
    local.InterstateRole.StartDate = Now().Date;
    local.InterstateRole.EndDate = UseCabSetMaximumDiscontinueDate();

    try
    {
      CreateInterstateRole();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SI0000_INTERSTAT_ROLE_ADD_ERROR";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTAT_ROLE_ADD_ERROR";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateInterstateRole()
  {
    var identifier = local.InterstateRole.Identifier;
    var type1 = local.InterstateRole.Type1;
    var startDate = local.InterstateRole.StartDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var endDate = local.InterstateRole.EndDate;
    var cspNumber = entities.Child.Number;
    var intId = entities.InterstateRequest.IntHGeneratedId;

    entities.InterstateRole.Populated = false;
    Update("CreateInterstateRole",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "startDate", startDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTstamp);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "intId", intId);
      });

    entities.InterstateRole.Identifier = identifier;
    entities.InterstateRole.Type1 = type1;
    entities.InterstateRole.StartDate = startDate;
    entities.InterstateRole.CreatedBy = createdBy;
    entities.InterstateRole.CreatedTstamp = createdTstamp;
    entities.InterstateRole.LastUpdatedBy = createdBy;
    entities.InterstateRole.LastUpdatedTstamp = createdTstamp;
    entities.InterstateRole.EndDate = endDate;
    entities.InterstateRole.CspNumber = cspNumber;
    entities.InterstateRole.IntId = intId;
    entities.InterstateRole.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRole()
  {
    entities.InterstateRole.Populated = false;

    return Read("ReadInterstateRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Child.Number);
        db.
          SetInt32(command, "intId", entities.InterstateRequest.IntHGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.InterstateRole.Identifier = db.GetInt32(reader, 0);
        entities.InterstateRole.Type1 = db.GetString(reader, 1);
        entities.InterstateRole.StartDate = db.GetDate(reader, 2);
        entities.InterstateRole.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRole.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.InterstateRole.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.InterstateRole.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.InterstateRole.EndDate = db.GetNullableDate(reader, 7);
        entities.InterstateRole.CspNumber = db.GetString(reader, 8);
        entities.InterstateRole.IntId = db.GetInt32(reader, 9);
        entities.InterstateRole.Populated = true;
      });
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private CsePerson child;
    private InterstateRequest interstateRequest;
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
    /// A value of InterstateRole.
    /// </summary>
    [JsonPropertyName("interstateRole")]
    public InterstateRole InterstateRole
    {
      get => interstateRole ??= new();
      set => interstateRole = value;
    }

    private InterstateRole interstateRole;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRole.
    /// </summary>
    [JsonPropertyName("interstateRole")]
    public InterstateRole InterstateRole
    {
      get => interstateRole ??= new();
      set => interstateRole = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRole interstateRole;
    private CsePerson child;
    private InterstateRequest interstateRequest;
  }
#endregion
}
