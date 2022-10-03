// Program: SI_FV_EVENT, ID: 374428719, model: 746.
// Short name: SWE00440
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_FV_EVENT.
/// </summary>
[Serializable]
public partial class SiFvEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FV_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFvEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFvEvent.
  /// </summary>
  public SiFvEvent(IContext context, Import import, Export export):
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
    // 07/10/00 M.Lachowicz    Raise event for change of
    //                         FV indicator of import person.
    //                         PR #98145 & PR #98146.
    // -----------------------------------------------------
    local.Current.Date = Now().Date;

    foreach(var item in ReadCaseCaseRoleCsePerson())
    {
      if (Equal(entities.Case1.Number, local.Previous.Number))
      {
        continue;
      }

      local.Previous.Number = entities.Case1.Number;

      if (ReadInterstateRequest())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 11;
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.CaseUnitNumber = 0;
      local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
      local.Infrastructure.ReasonCode = "FAMVIOLENCESET";
      local.Infrastructure.Detail = "FV turned on with value(";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + (
        import.CsePerson.FamilyViolenceIndicator ?? "") + ") for Person #:";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " " +
        import.CsePerson.Number;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case #: " +
        entities.Case1.Number;

      // -----------------------------------------------
      // Write out the infrastructure record.
      // -----------------------------------------------
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 5);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 6);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 8);
        entities.Case1.Note = db.GetNullableString(reader, 9);
        entities.CaseRole.CspNumber = db.GetString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.CaseRole.Type1 = db.GetString(reader, 11);
        entities.CaseRole.Identifier = db.GetInt32(reader, 12);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 13);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 14);
        entities.CsePerson.Type1 = db.GetString(reader, 15);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 18);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 19);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Case1 previous;
    private Infrastructure infrastructure;
    private DateWorkArea current;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
