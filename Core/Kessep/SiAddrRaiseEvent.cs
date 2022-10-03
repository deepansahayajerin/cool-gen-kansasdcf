// Program: SI_ADDR_RAISE_EVENT, ID: 371735363, model: 746.
// Short name: SWE01851
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
/// A program: SI_ADDR_RAISE_EVENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// Written by Raju     : Jan 02 1997
/// This action block raises the event(s) for Infrastructure for ADDR prad only
/// </para>
/// </summary>
[Serializable]
public partial class SiAddrRaiseEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ADDR_RAISE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAddrRaiseEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAddrRaiseEvent.
  /// </summary>
  public SiAddrRaiseEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 12/11/96	raju	initial creation
    // 01/08/97	raju	initiating state code made 2 chars KS/OS
    // 08/13/97	Sid	Raise events for no case units.
    // --------------------------------------------
    // 05/25/99 W.Campbell     Replaced zd exit state.
    // --------------------------------------------------
    // 07/13/99  Marek Lachowicz   Change property of READ (Select Only)
    // --------------------------------------------
    // Assigning global infrastructure attribute
    //   values
    // --------------------------------------------
    local.Current.Date = Now().Date;
    MoveInfrastructure2(import.Infrastructure, local.Infrastructure);
    local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";

    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (!ReadCsePerson())
    {
      // --------------------------------------------
      // 05/25/99 W.Campbell - Replaced zd exit state.
      // --------------------------------------------
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Raju 01/02/1997 : 1000 hrs CST
    //   - Refer Jack's note dated 01/02/1997
    //     subject : Case Closure and Reopen,
    //               Case Unit Deactivation and
    //               Reactivation
    // Conclusion drawn from note :
    // The check for raising events only for active
    //   case units is to be removed from all
    //   raise event cabs.
    // The event processor will handle this.
    // ---------------------------------------------
    switch(AsChar(import.AparSelection.Text1))
    {
      case 'P':
        foreach(var item in ReadCaseUnit1())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

          // 07/13/99  M.L 	Change property of READ (Select Only)
          if (ReadCase1())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;

            // ---------------------------------------------
            // This is another important piece of code.
            //   - reason codes are not unique but the
            //     combination of reason code , initiating
            //     state code is unique and is used to get
            //     the correct event detail record.
            // ---------------------------------------------
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

            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              return;
            }
          }
          else
          {
            ExitState = "CASE_NF";

            return;
          }
        }

        if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0)
        {
          // ------------------------------------
          // No Case Unit exists for this Person.
          // Raise the events at the Case level.
          // ------------------------------------
          foreach(var item in ReadCase2())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;

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

            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              return;
            }
          }
        }

        break;
      case 'R':
        foreach(var item in ReadCaseUnit2())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

          // 07/13/99  M.L 	Change property of READ (Select Only)
          if (ReadCase1())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;

            // ---------------------------------------------
            // This is another important piece of code.
            //   - reason codes are not unique but the
            //     combination of reason code , initiating
            //     state code is unique and is used to get
            //     the correct event detail record.
            // ---------------------------------------------
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
          }
          else
          {
            ExitState = "CASE_NF";

            return;
          }

          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
            ("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
          }
          else
          {
            return;
          }
        }

        if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0)
        {
          // ------------------------------------
          // No Case Unit exists for this Person.
          // Raise the events at the Case level.
          // ------------------------------------
          foreach(var item in ReadCase3())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;

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

            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              return;
            }
          }
        }

        break;
      default:
        break;
    }

    MoveInfrastructure1(local.Infrastructure, export.Infrastructure);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadCase1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CasNo);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAr", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private WorkArea aparSelection;
    private CsePerson csePerson;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    private InterstateRequest interstateRequest;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
