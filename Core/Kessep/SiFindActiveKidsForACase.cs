// Program: SI_FIND_ACTIVE_KIDS_FOR_A_CASE, ID: 371785571, model: 746.
// Short name: SWE01783
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_FIND_ACTIVE_KIDS_FOR_A_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This AB checks for active kids for a case.
/// </para>
/// </summary>
[Serializable]
public partial class SiFindActiveKidsForACase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FIND_ACTIVE_KIDS_FOR_A_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFindActiveKidsForACase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFindActiveKidsForACase.
  /// </summary>
  public SiFindActiveKidsForACase(IContext context, Import import, Export export)
    :
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
    // 		M A I N T E N A N C E   L O G
    // Date		Developer		Description
    // 11-23-96	Ken Evans		Initial Dev
    // ------------------------------------------------------------
    foreach(var item in ReadCaseRole())
    {
      try
      {
        UpdateCaseRole();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ROLE_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_ROLE_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.CaseRole.StartDate.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 8);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var relToAr = "CH";

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.RelToAr = relToAr;
    entities.CaseRole.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
