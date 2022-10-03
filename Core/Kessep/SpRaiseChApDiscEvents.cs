// Program: SP_RAISE_CH_AP_DISC_EVENTS, ID: 371785564, model: 746.
// Short name: SWE02065
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
/// A program: SP_RAISE_CH_AP_DISC_EVENTS.
/// </para>
/// <para>
/// This action block generates Infrastructure occurrences regarding the 
/// discontinuing of Mother or Father Case Roles when the appropriate conditions
/// are met.
/// </para>
/// </summary>
[Serializable]
public partial class SpRaiseChApDiscEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_RAISE_CH_AP_DISC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpRaiseChApDiscEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpRaiseChApDiscEvents.
  /// </summary>
  public SpRaiseChApDiscEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial Development - June 19, 1997
    // Developer - Jack Rookard, MTW
    // Infrastructure Performance Enhancement - Nov 20, 1997 - Jack Rookard, MTW
    // 06/25/99  M. Lachowicz     Change property of READ
    //                            (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 01/19/00  M. Lachowicz     Change code to fix -811
    //                            SQLCODE and process all closing case
    //                            units.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.DateWorkArea.Date = import.CaseRole.EndDate;

    // 06/25/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // 01/19/00 M.L Start
    if (ReadCaseRole1())
    {
      local.CurrentAr.StartDate = entities.CurrentAr.StartDate;
    }
    else
    {
      ExitState = "CASE_ROLE_AR_NF";

      return;
    }

    // 01/19/00 M.L End
    // 06/25/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadCaseRole2())
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

    if (Equal(entities.Disc.Type1, "CH"))
    {
      // 01/19/00 M.L Start
      local.CaseUnitFound.Flag = "N";

      foreach(var item in ReadCaseUnit1())
      {
        local.CaseUnitFound.Flag = "Y";
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.ReasonCode = "CH_DISCONTINUED";
        local.Infrastructure.Detail = "Child " + entities.CsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " disc effec ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        foreach(var item1 in ReadMonitoredActivity1())
        {
          try
          {
            UpdateMonitoredActivity();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "MONITORED_ACTIVITY_AE_WITH_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "MONITORED_ACTIVITY_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          foreach(var item2 in ReadMonitoredActivityAssignment())
          {
            try
            {
              UpdateMonitoredActivityAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        if (Lt(entities.CaseUnit.ClosureDate, entities.CaseUnit.StartDate))
        {
          DeleteCaseUnit();
          local.Infrastructure.ReasonCode = "DELETECAU";
          local.Infrastructure.Detail =
            "Case unit created in error has been deleted.";
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }

      if (AsChar(local.CaseUnitFound.Flag) == 'N')
      {
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CaseUnitNumber = 0;
        local.Infrastructure.ReasonCode = "CH_DISCONTINUED";
        local.Infrastructure.Detail = "Child " + entities.CsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " disc effec ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        foreach(var item in ReadMonitoredActivity2())
        {
          try
          {
            UpdateMonitoredActivity();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "MONITORED_ACTIVITY_AE_WITH_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "MONITORED_ACTIVITY_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          foreach(var item1 in ReadMonitoredActivityAssignment())
          {
            try
            {
              UpdateMonitoredActivityAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }

      // 01/19/00 M.L End
      return;
    }

    if (Equal(entities.Disc.Type1, "AP"))
    {
      foreach(var item in ReadCaseUnit2())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.ReasonCode = "AP_DISCONTINUED";
        local.Infrastructure.Detail = "AP " + entities.CsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " disc effec ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        foreach(var item1 in ReadMonitoredActivity1())
        {
          try
          {
            UpdateMonitoredActivity();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "MONITORED_ACTIVITY_AE_WITH_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "MONITORED_ACTIVITY_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          foreach(var item2 in ReadMonitoredActivityAssignment())
          {
            try
            {
              UpdateMonitoredActivityAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
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

  private void DeleteCaseUnit()
  {
    Update("DeleteCaseUnit#1",
      (db, command) =>
      {
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    Update("DeleteCaseUnit#2",
      (db, command) =>
      {
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });
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
    entities.CurrentAr.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrentAr.CasNumber = db.GetString(reader, 0);
        entities.CurrentAr.CspNumber = db.GetString(reader, 1);
        entities.CurrentAr.Type1 = db.GetString(reader, 2);
        entities.CurrentAr.Identifier = db.GetInt32(reader, 3);
        entities.CurrentAr.StartDate = db.GetNullableDate(reader, 4);
        entities.CurrentAr.EndDate = db.GetNullableDate(reader, 5);
        entities.CurrentAr.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CurrentAr.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Disc.Populated = false;

    return Read("ReadCaseRole2",
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

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoChild", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate", import.CaseRole.EndDate.GetValueOrDefault());
        db.SetDate(
          command, "startDate", local.CurrentAr.StartDate.GetValueOrDefault());
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

  private IEnumerable<bool> ReadMonitoredActivity1()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableInt32(command, "caseUnitNum", entities.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 4);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 7);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity2()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableString(command, "csePersonNum", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 4);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 7);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment()
  {
    entities.MonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          import.CaseRole.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 0);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 4);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 5);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 7);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 8);
        entities.MonitoredActivityAssignment.Populated = true;

        return true;
      });
  }

  private void UpdateMonitoredActivity()
  {
    var closureDate = import.CaseRole.EndDate;
    var closureReasonCode = "SYS";
    var caseUnitClosedInd = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivity.Populated = false;
    Update("UpdateMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "caseUnitClosedI", caseUnitClosedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
      });

    entities.MonitoredActivity.ClosureDate = closureDate;
    entities.MonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.MonitoredActivity.CaseUnitClosedInd = caseUnitClosedInd;
    entities.MonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivity.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredActivity.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.MonitoredActivityAssignment.Populated);

    var discontinueDate = entities.Disc.EndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.MonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.
          SetInt32(command, "spdId", entities.MonitoredActivityAssignment.SpdId);
          
        db.
          SetInt32(command, "offId", entities.MonitoredActivityAssignment.OffId);
          
        db.SetString(
          command, "ospCode", entities.MonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.MonitoredActivityAssignment.OspDate.GetValueOrDefault());
        db.
          SetInt32(command, "macId", entities.MonitoredActivityAssignment.MacId);
          
      });

    entities.MonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.MonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.MonitoredActivityAssignment.Populated = true;
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
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
    }

    /// <summary>
    /// A value of CurrentAr.
    /// </summary>
    [JsonPropertyName("currentAr")]
    public CaseRole CurrentAr
    {
      get => currentAr ??= new();
      set => currentAr = value;
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

    /// <summary>
    /// A value of HoldCu.
    /// </summary>
    [JsonPropertyName("holdCu")]
    public WorkArea HoldCu
    {
      get => holdCu ??= new();
      set => holdCu = value;
    }

    private Common caseUnitFound;
    private CaseRole currentAr;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private WorkArea holdCu;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CurrentAr.
    /// </summary>
    [JsonPropertyName("currentAr")]
    public CaseRole CurrentAr
    {
      get => currentAr ??= new();
      set => currentAr = value;
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
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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

    private CaseRole currentAr;
    private Infrastructure infrastructure;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private MonitoredActivity monitoredActivity;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private CaseRole disc;
    private Case1 case1;
  }
#endregion
}
