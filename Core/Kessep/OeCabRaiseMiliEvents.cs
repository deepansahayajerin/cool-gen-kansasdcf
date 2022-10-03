// Program: OE_CAB_RAISE_MILI_EVENTS, ID: 373323733, model: 746.
// Short name: SWE00718
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_CAB_RAISE_MILI_EVENTS.
/// </summary>
[Serializable]
public partial class OeCabRaiseMiliEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_RAISE_MILI_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabRaiseMiliEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabRaiseMiliEvents.
  /// </summary>
  public OeCabRaiseMiliEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    // Initial Development:
    // Vithal                  WR# 000261                  03/08/2001
    // This CAB is used only by SWEEJAIL to raise events when a person is 
    // incarcerated or released from JAIL. Based on the case_role for each case
    // different events will be raised for AP and AR.
    // For AP case_role raise events at case_unit level. If case_role is AR 
    // raise event for the first case_unit only.
    // --------------------------------------------------------------------------------
    local.Infrastructure.Assign(import.Infrastructure);

    // ----------------------------------------------------------------------------
    // Assign global infrastructure attribute values.
    // 
    // --------------------------------------------------------------------------------
    local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    foreach(var item in ReadCaseUnit1())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (ReadCase())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;
        local.Infrastructure.ReasonCode = "APINMIL";

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
      }

      UseSpCabCreateInfrastructure();
      export.Infrastructure.Assign(local.Infrastructure);
    }

    foreach(var item in ReadCaseUnit2())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (ReadCase())
      {
        if (Equal(entities.Case1.Number, local.Previous.Number))
        {
          continue;
        }
        else
        {
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Previous.Number = entities.Case1.Number;
        }

        local.Infrastructure.ReasonCode = "ARINMIL";

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
      }

      UseSpCabCreateInfrastructure();
      export.Infrastructure.Assign(local.Infrastructure);
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CasNo);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
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
        entities.CsePerson.Type1 = db.GetString(reader, 1);
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
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

    private Infrastructure infrastructure;
    private WorkArea raiseEventFlag;
    private CsePerson csePerson;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private Infrastructure infrastructure;
    private WorkArea raiseEventFlag;
    private Case1 previous;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private CsePerson csePerson;
    private CaseUnit caseUnit;
    private Case1 case1;
    private InterstateRequest interstateRequest;
  }
#endregion
}
