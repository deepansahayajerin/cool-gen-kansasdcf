// Program: SI_CLOSE_CASE_UNITS, ID: 371785570, model: 746.
// Short name: SWE01782
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
/// A program: SI_CLOSE_CASE_UNITS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCloseCaseUnits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CLOSE_CASE_UNITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCloseCaseUnits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCloseCaseUnits.
  /// </summary>
  public SiCloseCaseUnits(IContext context, Import import, Export export):
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
    // Date	  Developer		Description
    // 11-26-96  Ken Evans		Initial Dev
    // 12/30/96  G. Lofton - MTW	Add event logic
    // 03/15/97  G. Lofton - MTW	Add Case Unit Function
    // 				Assignment closure.
    // 05/05/97  J. Rookard - MTW      Debug
    // 06/11/97  J. Rookard - MTW      Remove creation of Infrastructure 
    // occurrences.  Remove code supporting "Case Of AR" since AR can never be
    // updated via the UPDATE command.
    // 06/19/97  J. Rookard - MTW      Reinsert creation of Infrastructure 
    // occurrences (events) for closure of Case Units per instructions of G.
    // Voegeli.
    // 12/09/97  J. Rookard - MTW      Remove control table usage by 
    // Infrastructure for performance tuning.
    // ------------------------------------------------------------
    // 06/24/99 M.Lachowicz  Change property of READ
    //                        (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 01/19/00 M.Lachowicz  Change code to affect all closing
    //                       case units.
    // ------------------------------------------------------------
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    local.Hold.Date = import.CaseRole.EndDate;

    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      if (ReadInterstateRequest())
      {
        local.Infrastructure.InitiatingStateCode = "OS";
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

    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // Populate the non-variable attributes of the local Infrastructure 
    // occurrence.
    local.Infrastructure.BusinessObjectCd = "CAU";
    local.Infrastructure.CaseNumber = entities.Case1.Number;
    local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
    local.Infrastructure.EventId = 11;
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.ReasonCode = "CU_DISCONTINUED";
    local.Infrastructure.UserId = "ROLE";
    local.Infrastructure.ReferenceDate = local.Current.Date;
    UseCabConvertDate2String();

    switch(TrimEnd(import.CaseRole.Type1))
    {
      case "AP":
        foreach(var item in ReadCaseUnit1())
        {
          try
          {
            UpdateCaseUnit1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_UNIT_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_UNIT_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          foreach(var item1 in ReadCaseUnitFunctionAssignmt())
          {
            try
            {
              UpdateCaseUnitFunctionAssignmt();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_NU_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // Raise the Case Unit Discontinued Infrastructure occurrence.
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          local.HoldCu.Text15 = NumberToString(entities.CaseUnit.CuNumber, 15);
          local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
          local.Infrastructure.Detail = "Case Unit " + TrimEnd
            (local.HoldCu.Text15);
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " disc effc ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .Local8.Text8;
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }
        }

        break;
      case "CH":
        // 01/19/00 M.L Start
        foreach(var item in ReadCaseUnit2())
        {
          try
          {
            UpdateCaseUnit2();

            foreach(var item1 in ReadCaseUnitFunctionAssignmt())
            {
              try
              {
                UpdateCaseUnitFunctionAssignmt();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_NU_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // Raise the Case Unit Discontinued Infrastructure occurrence.
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.HoldCu.Text15 =
              NumberToString(entities.CaseUnit.CuNumber, 15);
            local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
            local.Infrastructure.Detail = "Case Unit " + TrimEnd
              (local.HoldCu.Text15);
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " disc effc ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.Local8.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_UNIT_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_UNIT_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        // 01/19/00 M.L End
        break;
      default:
        break;
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Hold.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.Local8.Text8 = useExport.TextWorkArea.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 4);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Case1.Note = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
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
          command, "closureDate", import.CaseRole.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CaseUnit.CasNo = db.GetString(reader, 6);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 7);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 8);
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
        db.SetNullableString(command, "cspNoChild", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CaseUnit.CasNo = db.GetString(reader, 6);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 7);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 8);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate",
          import.CaseRole.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 5);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 6);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 7);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 8);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 9);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 10);
        entities.CaseUnitFunctionAssignmt.Populated = true;

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

  private void UpdateCaseUnit1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var closureDate = import.CaseRole.EndDate;
    var closureReasonCode = "AP";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit1",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.ClosureReasonCode = closureReasonCode;
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
  }

  private void UpdateCaseUnit2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var closureDate = import.CaseRole.EndDate;
    var closureReasonCode = "CH";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit2",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.ClosureReasonCode = closureReasonCode;
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseUnitFunctionAssignmt.Populated);

    var discontinueDate = import.CaseRole.EndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(command, "offId", entities.CaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(command, "csuNo", entities.CaseUnitFunctionAssignmt.CsuNo);
        db.SetString(command, "casNo", entities.CaseUnitFunctionAssignmt.CasNo);
      });

    entities.CaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CaseUnitFunctionAssignmt.Populated = true;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseUnit caseUnit;
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
    /// A value of Local8.
    /// </summary>
    [JsonPropertyName("local8")]
    public TextWorkArea Local8
    {
      get => local8 ??= new();
      set => local8 = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public DateWorkArea Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of HoldCu.
    /// </summary>
    [JsonPropertyName("holdCu")]
    public WorkArea HoldCu
    {
      get => holdCu ??= new();
      set => holdCu = value;
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
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

    private TextWorkArea local8;
    private DateWorkArea hold;
    private WorkArea holdCu;
    private DateWorkArea current;
    private Common update;
    private CaseUnit caseUnit;
    private DateWorkArea maxDate;
    private Infrastructure infrastructure;
    private DateWorkArea date;
    private TextWorkArea detailText30;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
  }
#endregion
}
