// Program: SI_CHANGE_AR_FV_INDICATOR, ID: 374428709, model: 746.
// Short name: SWE00291
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CHANGE_AR_FV_INDICATOR.
/// </summary>
[Serializable]
public partial class SiChangeArFvIndicator: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHANGE_AR_FV_INDICATOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiChangeArFvIndicator(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiChangeArFvIndicator.
  /// </summary>
  public SiChangeArFvIndicator(IContext context, Import import, Export export):
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
    //   Date   Developer	Description
    // -------- -------------- 
    // -------------------------------
    // 07/07/00 M.Lachowicz    Set FV indicator of AR
    //                         to FV indicator of import person.
    //                         PR #98145 & PR #98146.
    // 12/14/10 J. Huss        CQ# 9690.  Set FVI_SET_DATE and FVI_UPDATED_BY 
    // when setting FVI.
    // -----------------------------------------------------
    local.CurrentDate.Date = Now().Date;
    local.CurrentDate.Timestamp = Now();

    foreach(var item in ReadCsePerson())
    {
      if (AsChar(entities.ArCsePerson.FamilyViolenceIndicator) == AsChar
        (import.CsePerson.FamilyViolenceIndicator) || !
        IsEmpty(entities.ArCsePerson.FamilyViolenceIndicator))
      {
      }
      else
      {
        if (IsEmpty(entities.ArCsePerson.FamilyViolenceIndicator))
        {
          local.GenerateEvent.Flag = "Y";
        }

        try
        {
          UpdateCsePerson();
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

      if (AsChar(local.GenerateEvent.Flag) == 'Y')
      {
        UseSiFvEvent();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
  }

  private void UseSiFvEvent()
  {
    var useImport = new SiFvEvent.Import();
    var useExport = new SiFvEvent.Export();

    useImport.CsePerson.Assign(entities.ArCsePerson);

    Call(SiFvEvent.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.ArCsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.CurrentDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArCsePerson.Number = db.GetString(reader, 0);
        entities.ArCsePerson.Type1 = db.GetString(reader, 1);
        entities.ArCsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.ArCsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.ArCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.ArCsePerson.FviSetDate = db.GetNullableDate(reader, 5);
        entities.ArCsePerson.FviUpdatedBy = db.GetNullableString(reader, 6);
        entities.ArCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);

        return true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = local.CurrentDate.Timestamp;
    var lastUpdatedBy = global.UserId;
    var familyViolenceIndicator = import.CsePerson.FamilyViolenceIndicator ?? ""
      ;
    var fviSetDate = local.CurrentDate.Date;

    entities.ArCsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "familyViolInd", familyViolenceIndicator);
        db.SetNullableDate(command, "fviSetDate", fviSetDate);
        db.SetNullableString(command, "fviUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.ArCsePerson.Number);
      });

    entities.ArCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ArCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ArCsePerson.FamilyViolenceIndicator = familyViolenceIndicator;
    entities.ArCsePerson.FviSetDate = fviSetDate;
    entities.ArCsePerson.FviUpdatedBy = lastUpdatedBy;
    entities.ArCsePerson.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Case1 case1;
    private CsePerson csePerson;
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
    /// A value of GenerateEvent.
    /// </summary>
    [JsonPropertyName("generateEvent")]
    public Common GenerateEvent
    {
      get => generateEvent ??= new();
      set => generateEvent = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    private Common generateEvent;
    private DateWorkArea currentDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    private CsePerson chCsePerson;
    private Case1 case1;
    private CaseRole chCaseRole;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
  }
#endregion
}
