// Program: SP_RAISE_FA_MO_DISC_EVENTS, ID: 371785566, model: 746.
// Short name: SWE02050
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
/// A program: SP_RAISE_FA_MO_DISC_EVENTS.
/// </para>
/// <para>
/// This action block generates Infrastructure occurrences regarding the 
/// discontinuing of Mother or Father Case Roles when the appropriate conditions
/// are met.
/// </para>
/// </summary>
[Serializable]
public partial class SpRaiseFaMoDiscEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_RAISE_FA_MO_DISC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpRaiseFaMoDiscEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpRaiseFaMoDiscEvents.
  /// </summary>
  public SpRaiseFaMoDiscEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial Development - June 11, 1997
    // Developer - Jack Rookard, MTW
    // Infrastructure Performance Enhancement - Nov 20, 1997 - Jack Rookard, MTW
    // 06/25/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.DateWorkArea.Date = import.CaseRole.EndDate;
    local.CaseUnitFound.Flag = "N";
    local.ApFound.Flag = "N";

    // 06/25/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // 06/25/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // 06/25/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    // 08/06/99 M.L
    //              Change back property of the following READ to generate
    //              SELECT ONLY and CURSOR
    if (!ReadCaseRole1())
    {
      ExitState = "CASE_ROLE_NF";

      return;
    }

    if (ReadInterstateRequest())
    {
      local.Infrastructure.InitiatingStateCode = "OS";
    }
    else
    {
      local.Infrastructure.InitiatingStateCode = "KS";
    }

    // Populate the non-variable Infrastructure attributes.
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.BusinessObjectCd = "CAU";
    local.Infrastructure.CaseNumber = entities.Case1.Number;
    local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
    local.Infrastructure.ReferenceDate = local.Current.Date;
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.EventId = 11;
    local.Infrastructure.UserId = "ROLE";
    UseCabConvertDate2String();

    // 06/25/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCaseRole2())
    {
      local.ApFound.Flag = "Y";

      // The FAther or MOther Case Role has been discontinued.  Determine if the
      // FAther or MOther is also currently participating in an AP Case Role
      // for the current Case.  If so, raise the event APNOTFA for FAther and
      // APNOTMO for MOther.
      foreach(var item in ReadCaseUnit1())
      {
        local.CaseUnitFound.Flag = "Y";
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

        switch(TrimEnd(import.CaseRole.Type1))
        {
          case "FA":
            local.Infrastructure.ReasonCode = "APNOTFA";
            local.Infrastructure.Detail = "FA participating as AP " + entities
              .CsePerson.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " disc effec ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          case "MO":
            local.Infrastructure.ReasonCode = "APNOTMO";
            local.Infrastructure.Detail = "MO participating as AP " + entities
              .CsePerson.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " disc effec ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
        }
      }

      if (AsChar(local.CaseUnitFound.Flag) == 'Y')
      {
      }
      else
      {
        local.Infrastructure.CaseUnitNumber = 0;
        local.Infrastructure.BusinessObjectCd = "CAS";

        switch(TrimEnd(import.CaseRole.Type1))
        {
          case "FA":
            local.Infrastructure.ReasonCode = "APNOTFA";
            local.Infrastructure.Detail = "FA participating as AP " + entities
              .CsePerson.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " disc effec ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          case "MO":
            local.Infrastructure.ReasonCode = "APNOTMO";
            local.Infrastructure.Detail = "MO participating as AP " + entities
              .CsePerson.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " disc effec ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
        }
      }
    }

    if (AsChar(local.ApFound.Flag) == 'Y')
    {
    }
    else
    {
      // 06/25/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseRole3())
      {
        // The FAther or MOther Case Role has been discontinued.  Determine if 
        // the FAther or MOther is also currently participating in an AR Case
        // Role for the current Case.  If so, raise the event ARNOTFA for FAther
        // and ARNOTMO for MOther.
        foreach(var item in ReadCaseUnit2())
        {
          local.CaseUnitFound.Flag = "Y";
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "FA":
              local.Infrastructure.ReasonCode = "ARNOTFA";
              local.Infrastructure.Detail = "FA participating as AR " + entities
                .CsePerson.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " disc effec ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              break;
            case "MO":
              local.Infrastructure.ReasonCode = "ARNOTMO";
              local.Infrastructure.Detail = "MO participating as AR " + entities
                .CsePerson.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " disc effec ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
          }
        }

        if (AsChar(local.CaseUnitFound.Flag) == 'Y')
        {
        }
        else
        {
          local.Infrastructure.CaseUnitNumber = 0;
          local.Infrastructure.BusinessObjectCd = "CAS";

          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "FA":
              local.Infrastructure.ReasonCode = "ARNOTFA";
              local.Infrastructure.Detail = "FA participating as AR " + entities
                .CsePerson.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " disc effec ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
              }

              break;
            case "MO":
              local.Infrastructure.ReasonCode = "ARNOTMO";
              local.Infrastructure.Detail = "MO participating as AR " + entities
                .CsePerson.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " disc effec ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
              }

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_CODE";

              break;
          }
        }
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.Disc.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.Disc.CasNumber = db.GetString(reader, 0);
        entities.Disc.CspNumber = db.GetString(reader, 1);
        entities.Disc.Type1 = db.GetString(reader, 2);
        entities.Disc.Identifier = db.GetInt32(reader, 3);
        entities.Disc.StartDate = db.GetNullableDate(reader, 4);
        entities.Disc.EndDate = db.GetNullableDate(reader, 5);
        entities.Disc.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Disc.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Ap.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.Ap.CspNumber = db.GetString(reader, 1);
        entities.Ap.Type1 = db.GetString(reader, 2);
        entities.Ap.Identifier = db.GetInt32(reader, 3);
        entities.Ap.StartDate = db.GetNullableDate(reader, 4);
        entities.Ap.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.Ar.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.CasNumber = db.GetString(reader, 0);
        entities.Ar.CspNumber = db.GetString(reader, 1);
        entities.Ar.Type1 = db.GetString(reader, 2);
        entities.Ar.Identifier = db.GetInt32(reader, 3);
        entities.Ar.StartDate = db.GetNullableDate(reader, 4);
        entities.Ar.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
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
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAr", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
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
    /// A value of ApFound.
    /// </summary>
    [JsonPropertyName("apFound")]
    public Common ApFound
    {
      get => apFound ??= new();
      set => apFound = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
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

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Common apFound;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
    private Common caseUnitFound;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Disc.
    /// </summary>
    [JsonPropertyName("disc")]
    public CaseRole Disc
    {
      get => disc ??= new();
      set => disc = value;
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

    private CaseRole ar;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private CaseRole ap;
    private CsePerson csePerson;
    private CaseRole disc;
    private Case1 case1;
  }
#endregion
}
