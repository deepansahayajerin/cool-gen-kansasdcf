// Program: SI_RMAD_CREATE_UPDATE_CASE_UNIT, ID: 373476814, model: 746.
// Short name: SWE01856
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
/// A program: SI_RMAD_CREATE_UPDATE_CASE_UNIT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiRmadCreateUpdateCaseUnit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_CREATE_UPDATE_CASE_UNIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadCreateUpdateCaseUnit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadCreateUpdateCaseUnit.
  /// </summary>
  public SiRmadCreateUpdateCaseUnit(IContext context, Import import,
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
    // ------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 06-06-01  M.Lachowicz		Initial Dev
    // ------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------------------
    // 01/21/2007	Raj S		PR 299791 	Modified to fix the  VIEW OVERFLOW problem 
    // for group
    //                                                 
    // view GROUP_LOCAL_VALIDATE in SI_RMAD_SAVE_DATA.
    // The
    //                                                 
    // following group view sized are increased as
    // below.
    //                                                 
    // Group_import_validate from 15 to 100.
    //                                                 
    // LOCAL_GRP_ALL_AP      from 15 to 50.
    //                                                 
    // LOCAL_GRP_ALL_CH      from 15 to 50.
    //                                                 
    // LOCAL_GRP_ALL_Ar      from 15 to 50.
    // 						GROUP LOCAL ALL CASE UNITS from 25 to 100.
    // ------------------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.AllAp.Index = -1;
    local.AllAr.Index = -1;
    local.AllCh.Index = -1;
    local.AllCaseUnits.Index = -1;
    import.Validate.Index = -1;

    for(import.Validate.Index = 0; import.Validate.Index < import
      .Validate.Count; ++import.Validate.Index)
    {
      if (!import.Validate.CheckSize())
      {
        break;
      }

      switch(TrimEnd(import.Validate.Item.CaseRoles.Type1))
      {
        case "AR":
          ++local.AllAr.Index;
          local.AllAr.CheckSize();

          local.AllAr.Update.ArAllWork.Number =
            import.Validate.Item.AllPersons.Number;
          local.AllAr.Update.AllArCaseRoles.Assign(
            import.Validate.Item.CaseRoles);
          local.AllAr.Update.ArAllWorkOperation.OneChar =
            import.Validate.Item.AllWorkOperation.OneChar;
          MoveCaseRole(import.Validate.Item.Original,
            local.AllAr.Update.OriginalAllArCaseRoles);

          break;
        case "AP":
          ++local.AllAp.Index;
          local.AllAp.CheckSize();

          local.AllAp.Update.ApAllWork.Number =
            import.Validate.Item.AllPersons.Number;
          local.AllAp.Update.AllApCaseRoles.Assign(
            import.Validate.Item.CaseRoles);
          local.AllAp.Update.ApAllWorkOperation.OneChar =
            import.Validate.Item.AllWorkOperation.OneChar;
          MoveCaseRole(import.Validate.Item.Original,
            local.AllAp.Update.OriginalAllApCaseRoles);

          break;
        case "CH":
          ++local.AllCh.Index;
          local.AllCh.CheckSize();

          local.AllCh.Update.ChAllWork.Number =
            import.Validate.Item.AllPersons.Number;
          local.AllCh.Update.AllChCaseRoles.Assign(
            import.Validate.Item.CaseRoles);
          local.AllCh.Update.ChAllWorkOperation.OneChar =
            import.Validate.Item.AllWorkOperation.OneChar;
          MoveCaseRole(import.Validate.Item.Original,
            local.AllCh.Update.OriginalAllChCaseRole);

          break;
        default:
          break;
      }
    }

    import.Validate.CheckIndex();

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
      {
        local.New1.State = "LENUU";
      }
      else if (AsChar(entities.Case1.FullServiceWithMedInd) == 'Y')
      {
        local.New1.State = "LFNUU";
      }
      else if (AsChar(entities.Case1.FullServiceWithoutMedInd) == 'Y')
      {
        local.New1.State = "LFNUU";
      }
      else if (AsChar(entities.Case1.LocateInd) == 'Y')
      {
        local.New1.State = "LLNUU";
      }
    }
    else
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    foreach(var item in ReadCaseUnitCsePersonCsePersonCsePerson())
    {
      if (IsEmpty(local.First.Flag))
      {
        local.First.Flag = "Y";
        local.LastCase.CuNumber = entities.CaseUnit.CuNumber + 1;
      }

      ++local.AllCaseUnits.Index;
      local.AllCaseUnits.CheckSize();

      MoveCaseUnit(entities.CaseUnit, local.AllCaseUnits.Update.CaseUnits);
      local.AllCaseUnits.Update.CuAp.Number = entities.Ap.Number;
      local.AllCaseUnits.Update.CuAr.Number = entities.Ar.Number;
      local.AllCaseUnits.Update.CuCh.Number = entities.Child.Number;
    }

    for(local.AllAp.Index = 0; local.AllAp.Index < local.AllAp.Count; ++
      local.AllAp.Index)
    {
      if (!local.AllAp.CheckSize())
      {
        break;
      }

      if (!ReadCsePerson1())
      {
        ExitState = "AP_NF_RB";

        return;
      }

      switch(AsChar(local.AllAp.Item.ApAllWorkOperation.OneChar))
      {
        case 'A':
          // 09/05/2001 Disabled code
          // 09/05/2001 Disabled code
          break;
        case 'C':
          // 09/11/01 Start disabled code
          // 09/11/01 End disabled code
          // 09/05/01 M.L Disabled code start
          // 09/05/01 M.L Disabled code end
          for(local.AllCaseUnits.Index = 0; local.AllCaseUnits.Index < local
            .AllCaseUnits.Count; ++local.AllCaseUnits.Index)
          {
            if (!local.AllCaseUnits.CheckSize())
            {
              break;
            }

            if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
              local.AllAp.Item.OriginalAllApCaseRoles.StartDate) && !
              Lt(local.AllAp.Item.OriginalAllApCaseRoles.EndDate,
              local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
              (local.AllAp.Item.ApAllWork.Number,
              local.AllCaseUnits.Item.CuAp.Number))
            {
              local.AllCaseUnits.Update.CaseUnitRequiredAction.Flag = "U";
              local.AllCaseUnits.Update.Updated.ClosureDate =
                local.AllAp.Item.AllApCaseRoles.EndDate;
              local.AllCaseUnits.Update.Updated.StartDate =
                local.AllAp.Item.AllApCaseRoles.StartDate;

              for(local.AllCh.Index = 0; local.AllCh.Index < local.AllCh.Count; ++
                local.AllCh.Index)
              {
                if (!local.AllCh.CheckSize())
                {
                  break;
                }

                if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
                  local.AllCh.Item.OriginalAllChCaseRole.StartDate) && !
                  Lt(local.AllCh.Item.OriginalAllChCaseRole.EndDate,
                  local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
                  (local.AllCh.Item.ChAllWork.Number,
                  local.AllCaseUnits.Item.CuCh.Number))
                {
                  if (Lt(local.AllCaseUnits.Item.Updated.StartDate,
                    local.AllCh.Item.AllChCaseRoles.StartDate))
                  {
                    local.AllCaseUnits.Update.Updated.StartDate =
                      local.AllCh.Item.AllChCaseRoles.StartDate;
                  }

                  if (Lt(local.AllCh.Item.AllChCaseRoles.EndDate,
                    local.AllCaseUnits.Item.Updated.ClosureDate))
                  {
                    local.AllCaseUnits.Update.Updated.ClosureDate =
                      local.AllCh.Item.AllChCaseRoles.EndDate;
                  }
                }
              }

              local.AllCh.CheckIndex();

              for(local.AllAr.Index = 0; local.AllAr.Index < local.AllAr.Count; ++
                local.AllAr.Index)
              {
                if (!local.AllAr.CheckSize())
                {
                  break;
                }

                if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
                  local.AllAr.Item.OriginalAllArCaseRoles.StartDate) && !
                  Lt(local.AllAr.Item.OriginalAllArCaseRoles.EndDate,
                  local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
                  (local.AllAr.Item.ArAllWork.Number,
                  local.AllCaseUnits.Item.CuAr.Number))
                {
                  if (Lt(local.AllCaseUnits.Item.Updated.StartDate,
                    local.AllAr.Item.AllArCaseRoles.StartDate))
                  {
                    local.AllCaseUnits.Update.Updated.StartDate =
                      local.AllAr.Item.AllArCaseRoles.StartDate;
                  }

                  if (Lt(local.AllAr.Item.AllArCaseRoles.EndDate,
                    local.AllCaseUnits.Item.Updated.ClosureDate))
                  {
                    local.AllCaseUnits.Update.Updated.ClosureDate =
                      local.AllAr.Item.AllArCaseRoles.EndDate;
                  }
                }
              }

              local.AllAr.CheckIndex();
            }
          }

          local.AllCaseUnits.CheckIndex();

          // 09/05/01 M.L Disabled code start
          // 09/05/01 M.L Disabled code end
          break;
        default:
          break;
      }
    }

    local.AllAp.CheckIndex();

    for(local.AllCh.Index = 0; local.AllCh.Index < local.AllCh.Count; ++
      local.AllCh.Index)
    {
      if (!local.AllCh.CheckSize())
      {
        break;
      }

      if (!ReadCsePerson3())
      {
        ExitState = "CHILD_NF_RB";

        return;
      }

      switch(AsChar(local.AllCh.Item.ChAllWorkOperation.OneChar))
      {
        case 'A':
          // 09/05/2001 Disabled code
          // 09/05/2001 Disabled code
          break;
        case 'C':
          // 09/11/01 Start Disabled code
          // 09/11/01 Start Disabled code
          for(local.AllCaseUnits.Index = 0; local.AllCaseUnits.Index < local
            .AllCaseUnits.Count; ++local.AllCaseUnits.Index)
          {
            if (!local.AllCaseUnits.CheckSize())
            {
              break;
            }

            if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
              local.AllCh.Item.OriginalAllChCaseRole.StartDate) && !
              Lt(local.AllCh.Item.OriginalAllChCaseRole.EndDate,
              local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
              (local.AllCh.Item.ChAllWork.Number,
              local.AllCaseUnits.Item.CuCh.Number))
            {
              local.AllCaseUnits.Update.CaseUnitRequiredAction.Flag = "U";
              local.AllCaseUnits.Update.Updated.ClosureDate =
                local.AllCh.Item.AllChCaseRoles.EndDate;
              local.AllCaseUnits.Update.Updated.StartDate =
                local.AllCh.Item.AllChCaseRoles.StartDate;

              for(local.AllAp.Index = 0; local.AllAp.Index < local.AllAp.Count; ++
                local.AllAp.Index)
              {
                if (!local.AllAp.CheckSize())
                {
                  break;
                }

                if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
                  local.AllAp.Item.OriginalAllApCaseRoles.StartDate) && !
                  Lt(local.AllAp.Item.OriginalAllApCaseRoles.EndDate,
                  local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
                  (local.AllAp.Item.ApAllWork.Number,
                  local.AllCaseUnits.Item.CuAp.Number))
                {
                  if (Lt(local.AllCaseUnits.Item.Updated.StartDate,
                    local.AllAp.Item.AllApCaseRoles.StartDate))
                  {
                    local.AllCaseUnits.Update.Updated.StartDate =
                      local.AllAp.Item.AllApCaseRoles.StartDate;
                  }

                  if (Lt(local.AllAp.Item.AllApCaseRoles.EndDate,
                    local.AllCaseUnits.Item.Updated.ClosureDate))
                  {
                    local.AllCaseUnits.Update.Updated.ClosureDate =
                      local.AllAp.Item.AllApCaseRoles.EndDate;
                  }
                }
              }

              local.AllAp.CheckIndex();

              for(local.AllAr.Index = 0; local.AllAr.Index < local.AllAr.Count; ++
                local.AllAr.Index)
              {
                if (!local.AllAr.CheckSize())
                {
                  break;
                }

                if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
                  local.AllAr.Item.OriginalAllArCaseRoles.StartDate) && !
                  Lt(local.AllAr.Item.OriginalAllArCaseRoles.EndDate,
                  local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
                  (local.AllAr.Item.ArAllWork.Number,
                  local.AllCaseUnits.Item.CuAr.Number))
                {
                  if (Lt(local.AllCaseUnits.Item.Updated.StartDate,
                    local.AllAr.Item.AllArCaseRoles.StartDate))
                  {
                    local.AllCaseUnits.Update.Updated.StartDate =
                      local.AllAr.Item.AllArCaseRoles.StartDate;
                  }

                  if (Lt(local.AllAr.Item.AllArCaseRoles.EndDate,
                    local.AllCaseUnits.Item.Updated.ClosureDate))
                  {
                    local.AllCaseUnits.Update.Updated.ClosureDate =
                      local.AllAr.Item.AllArCaseRoles.EndDate;
                  }
                }
              }

              local.AllAr.CheckIndex();
            }
          }

          local.AllCaseUnits.CheckIndex();

          break;
        default:
          break;
      }
    }

    local.AllCh.CheckIndex();

    for(local.AllAr.Index = 0; local.AllAr.Index < local.AllAr.Count; ++
      local.AllAr.Index)
    {
      if (!local.AllAr.CheckSize())
      {
        break;
      }

      if (!ReadCsePerson2())
      {
        ExitState = "AR_NF_RB";

        return;
      }

      switch(AsChar(local.AllAr.Item.ArAllWorkOperation.OneChar))
      {
        case 'A':
          // 09/05/2001 Disabled code
          // 09/05/2001 Disabled code
          break;
        case 'C':
          // 09/11/01 Code Disabled.  Start
          // 09/11/01 Code Disabled.  End
          for(local.AllCaseUnits.Index = 0; local.AllCaseUnits.Index < local
            .AllCaseUnits.Count; ++local.AllCaseUnits.Index)
          {
            if (!local.AllCaseUnits.CheckSize())
            {
              break;
            }

            if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
              local.AllAr.Item.OriginalAllArCaseRoles.StartDate) && !
              Lt(local.AllAr.Item.OriginalAllArCaseRoles.EndDate,
              local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
              (local.AllAr.Item.ArAllWork.Number,
              local.AllCaseUnits.Item.CuAr.Number))
            {
              local.AllCaseUnits.Update.CaseUnitRequiredAction.Flag = "U";
              local.AllCaseUnits.Update.Updated.ClosureDate =
                local.AllAr.Item.AllArCaseRoles.EndDate;
              local.AllCaseUnits.Update.Updated.StartDate =
                local.AllAr.Item.AllArCaseRoles.StartDate;

              for(local.AllAp.Index = 0; local.AllAp.Index < local.AllAp.Count; ++
                local.AllAp.Index)
              {
                if (!local.AllAp.CheckSize())
                {
                  break;
                }

                if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
                  local.AllAp.Item.OriginalAllApCaseRoles.StartDate) && !
                  Lt(local.AllAp.Item.OriginalAllApCaseRoles.EndDate,
                  local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
                  (local.AllAp.Item.ApAllWork.Number,
                  local.AllCaseUnits.Item.CuAp.Number))
                {
                  if (Lt(local.AllCaseUnits.Item.Updated.StartDate,
                    local.AllAp.Item.AllApCaseRoles.StartDate))
                  {
                    local.AllCaseUnits.Update.Updated.StartDate =
                      local.AllAp.Item.AllApCaseRoles.StartDate;
                  }

                  if (Lt(local.AllAp.Item.AllApCaseRoles.EndDate,
                    local.AllCaseUnits.Item.Updated.ClosureDate))
                  {
                    local.AllCaseUnits.Update.Updated.ClosureDate =
                      local.AllAp.Item.AllApCaseRoles.EndDate;
                  }
                }
              }

              local.AllAp.CheckIndex();

              for(local.AllCh.Index = 0; local.AllCh.Index < local.AllCh.Count; ++
                local.AllCh.Index)
              {
                if (!local.AllCh.CheckSize())
                {
                  break;
                }

                if (!Lt(local.AllCaseUnits.Item.CaseUnits.StartDate,
                  local.AllCh.Item.OriginalAllChCaseRole.StartDate) && !
                  Lt(local.AllCh.Item.OriginalAllChCaseRole.EndDate,
                  local.AllCaseUnits.Item.CaseUnits.ClosureDate) && Equal
                  (local.AllCh.Item.ChAllWork.Number,
                  local.AllCaseUnits.Item.CuCh.Number))
                {
                  if (Lt(local.AllCaseUnits.Item.Updated.StartDate,
                    local.AllCh.Item.AllChCaseRoles.StartDate))
                  {
                    local.AllCaseUnits.Update.Updated.StartDate =
                      local.AllCh.Item.AllChCaseRoles.StartDate;
                  }

                  if (Lt(local.AllCh.Item.AllChCaseRoles.EndDate,
                    local.AllCaseUnits.Item.Updated.ClosureDate))
                  {
                    local.AllCaseUnits.Update.Updated.ClosureDate =
                      local.AllCh.Item.AllChCaseRoles.EndDate;
                  }
                }
              }

              local.AllCh.CheckIndex();
            }
          }

          local.AllCaseUnits.CheckIndex();

          break;
        default:
          break;
      }
    }

    local.AllAr.CheckIndex();
    local.Current.Timestamp = Now();

    for(local.AllCaseUnits.Index = 0; local.AllCaseUnits.Index < local
      .AllCaseUnits.Count; ++local.AllCaseUnits.Index)
    {
      if (!local.AllCaseUnits.CheckSize())
      {
        break;
      }

      if (AsChar(local.AllCaseUnits.Item.CaseUnitRequiredAction.Flag) == 'U')
      {
        if (ReadCaseUnit())
        {
          try
          {
            UpdateCaseUnit();
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
        else
        {
          ExitState = "CASE_UNIT_NF";

          return;
        }
      }
      else
      {
      }
    }

    local.AllCaseUnits.CheckIndex();
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCaseUnit(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
    target.StartDate = source.StartDate;
    target.ClosureDate = source.ClosureDate;
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
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 1);
        entities.Case1.LocateInd = db.GetNullableString(reader, 2);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 4);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetInt32(
          command, "cuNumber", local.AllCaseUnits.Item.CaseUnits.CuNumber);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 10);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 11);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 12);
        entities.CaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitCsePersonCsePersonCsePerson()
  {
    entities.CaseUnit.Populated = false;
    entities.Child.Populated = false;
    entities.Ap.Populated = false;
    entities.Ar.Populated = false;

    return ReadEach("ReadCaseUnitCsePersonCsePersonCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 10);
        entities.Ar.Number = db.GetString(reader, 10);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 11);
        entities.Ap.Number = db.GetString(reader, 11);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 12);
        entities.Child.Number = db.GetString(reader, 12);
        entities.CaseUnit.Populated = true;
        entities.Child.Populated = true;
        entities.Ap.Populated = true;
        entities.Ar.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.AllAp.Item.ApAllWork.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.AllAr.Item.ArAllWork.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", local.AllCh.Item.ChAllWork.Number);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Populated = true;
      });
  }

  private void UpdateCaseUnit()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var startDate = local.AllCaseUnits.Item.Updated.StartDate;
    var closureDate = local.AllCaseUnits.Item.Updated.ClosureDate;
    var lastUpdatedBy = "SWEIRMAD";
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit",
      (db, command) =>
      {
        db.SetDate(command, "startDate", startDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.StartDate = startDate;
    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
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
    /// <summary>A ValidateGroup group.</summary>
    [Serializable]
    public class ValidateGroup
    {
      /// <summary>
      /// A value of RowNumber.
      /// </summary>
      [JsonPropertyName("rowNumber")]
      public Common RowNumber
      {
        get => rowNumber ??= new();
        set => rowNumber = value;
      }

      /// <summary>
      /// A value of AllWorkOperation.
      /// </summary>
      [JsonPropertyName("allWorkOperation")]
      public Standard AllWorkOperation
      {
        get => allWorkOperation ??= new();
        set => allWorkOperation = value;
      }

      /// <summary>
      /// A value of AllPersons.
      /// </summary>
      [JsonPropertyName("allPersons")]
      public CsePersonsWorkSet AllPersons
      {
        get => allPersons ??= new();
        set => allPersons = value;
      }

      /// <summary>
      /// A value of CaseRoles.
      /// </summary>
      [JsonPropertyName("caseRoles")]
      public CaseRole CaseRoles
      {
        get => caseRoles ??= new();
        set => caseRoles = value;
      }

      /// <summary>
      /// A value of Original.
      /// </summary>
      [JsonPropertyName("original")]
      public CaseRole Original
      {
        get => original ??= new();
        set => original = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common rowNumber;
      private Standard allWorkOperation;
      private CsePersonsWorkSet allPersons;
      private CaseRole caseRoles;
      private CaseRole original;
    }

    /// <summary>
    /// Gets a value of Validate.
    /// </summary>
    [JsonIgnore]
    public Array<ValidateGroup> Validate => validate ??= new(
      ValidateGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Validate for json serialization.
    /// </summary>
    [JsonPropertyName("validate")]
    [Computed]
    public IList<ValidateGroup> Validate_Json
    {
      get => validate;
      set => Validate.Assign(value);
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Array<ValidateGroup> validate;
    private Case1 case1;
    private CsePerson child;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CaseUnit caseUnit;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AllApGroup group.</summary>
    [Serializable]
    public class AllApGroup
    {
      /// <summary>
      /// A value of ApAllWorkOperation.
      /// </summary>
      [JsonPropertyName("apAllWorkOperation")]
      public Standard ApAllWorkOperation
      {
        get => apAllWorkOperation ??= new();
        set => apAllWorkOperation = value;
      }

      /// <summary>
      /// A value of ApAllWork.
      /// </summary>
      [JsonPropertyName("apAllWork")]
      public CsePersonsWorkSet ApAllWork
      {
        get => apAllWork ??= new();
        set => apAllWork = value;
      }

      /// <summary>
      /// A value of AllApCaseRoles.
      /// </summary>
      [JsonPropertyName("allApCaseRoles")]
      public CaseRole AllApCaseRoles
      {
        get => allApCaseRoles ??= new();
        set => allApCaseRoles = value;
      }

      /// <summary>
      /// A value of OriginalAllApCaseRoles.
      /// </summary>
      [JsonPropertyName("originalAllApCaseRoles")]
      public CaseRole OriginalAllApCaseRoles
      {
        get => originalAllApCaseRoles ??= new();
        set => originalAllApCaseRoles = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Standard apAllWorkOperation;
      private CsePersonsWorkSet apAllWork;
      private CaseRole allApCaseRoles;
      private CaseRole originalAllApCaseRoles;
    }

    /// <summary>A AllChGroup group.</summary>
    [Serializable]
    public class AllChGroup
    {
      /// <summary>
      /// A value of ChAllWorkOperation.
      /// </summary>
      [JsonPropertyName("chAllWorkOperation")]
      public Standard ChAllWorkOperation
      {
        get => chAllWorkOperation ??= new();
        set => chAllWorkOperation = value;
      }

      /// <summary>
      /// A value of ChAllWork.
      /// </summary>
      [JsonPropertyName("chAllWork")]
      public CsePersonsWorkSet ChAllWork
      {
        get => chAllWork ??= new();
        set => chAllWork = value;
      }

      /// <summary>
      /// A value of AllChCaseRoles.
      /// </summary>
      [JsonPropertyName("allChCaseRoles")]
      public CaseRole AllChCaseRoles
      {
        get => allChCaseRoles ??= new();
        set => allChCaseRoles = value;
      }

      /// <summary>
      /// A value of OriginalAllChCaseRole.
      /// </summary>
      [JsonPropertyName("originalAllChCaseRole")]
      public CaseRole OriginalAllChCaseRole
      {
        get => originalAllChCaseRole ??= new();
        set => originalAllChCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Standard chAllWorkOperation;
      private CsePersonsWorkSet chAllWork;
      private CaseRole allChCaseRoles;
      private CaseRole originalAllChCaseRole;
    }

    /// <summary>A AllArGroup group.</summary>
    [Serializable]
    public class AllArGroup
    {
      /// <summary>
      /// A value of ArAllWorkOperation.
      /// </summary>
      [JsonPropertyName("arAllWorkOperation")]
      public Standard ArAllWorkOperation
      {
        get => arAllWorkOperation ??= new();
        set => arAllWorkOperation = value;
      }

      /// <summary>
      /// A value of ArAllWork.
      /// </summary>
      [JsonPropertyName("arAllWork")]
      public CsePersonsWorkSet ArAllWork
      {
        get => arAllWork ??= new();
        set => arAllWork = value;
      }

      /// <summary>
      /// A value of AllArCaseRoles.
      /// </summary>
      [JsonPropertyName("allArCaseRoles")]
      public CaseRole AllArCaseRoles
      {
        get => allArCaseRoles ??= new();
        set => allArCaseRoles = value;
      }

      /// <summary>
      /// A value of OriginalAllArCaseRoles.
      /// </summary>
      [JsonPropertyName("originalAllArCaseRoles")]
      public CaseRole OriginalAllArCaseRoles
      {
        get => originalAllArCaseRoles ??= new();
        set => originalAllArCaseRoles = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Standard arAllWorkOperation;
      private CsePersonsWorkSet arAllWork;
      private CaseRole allArCaseRoles;
      private CaseRole originalAllArCaseRoles;
    }

    /// <summary>A AllCaseUnitsGroup group.</summary>
    [Serializable]
    public class AllCaseUnitsGroup
    {
      /// <summary>
      /// A value of CaseUnitRequiredAction.
      /// </summary>
      [JsonPropertyName("caseUnitRequiredAction")]
      public Common CaseUnitRequiredAction
      {
        get => caseUnitRequiredAction ??= new();
        set => caseUnitRequiredAction = value;
      }

      /// <summary>
      /// A value of CuAp.
      /// </summary>
      [JsonPropertyName("cuAp")]
      public CsePerson CuAp
      {
        get => cuAp ??= new();
        set => cuAp = value;
      }

      /// <summary>
      /// A value of CuCh.
      /// </summary>
      [JsonPropertyName("cuCh")]
      public CsePerson CuCh
      {
        get => cuCh ??= new();
        set => cuCh = value;
      }

      /// <summary>
      /// A value of CuAr.
      /// </summary>
      [JsonPropertyName("cuAr")]
      public CsePerson CuAr
      {
        get => cuAr ??= new();
        set => cuAr = value;
      }

      /// <summary>
      /// A value of CaseUnits.
      /// </summary>
      [JsonPropertyName("caseUnits")]
      public CaseUnit CaseUnits
      {
        get => caseUnits ??= new();
        set => caseUnits = value;
      }

      /// <summary>
      /// A value of Updated.
      /// </summary>
      [JsonPropertyName("updated")]
      public CaseUnit Updated
      {
        get => updated ??= new();
        set => updated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common caseUnitRequiredAction;
      private CsePerson cuAp;
      private CsePerson cuCh;
      private CsePerson cuAr;
      private CaseUnit caseUnits;
      private CaseUnit updated;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseUnit New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// Gets a value of AllAp.
    /// </summary>
    [JsonIgnore]
    public Array<AllApGroup> AllAp => allAp ??= new(AllApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AllAp for json serialization.
    /// </summary>
    [JsonPropertyName("allAp")]
    [Computed]
    public IList<AllApGroup> AllAp_Json
    {
      get => allAp;
      set => AllAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of AllCh.
    /// </summary>
    [JsonIgnore]
    public Array<AllChGroup> AllCh => allCh ??= new(AllChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AllCh for json serialization.
    /// </summary>
    [JsonPropertyName("allCh")]
    [Computed]
    public IList<AllChGroup> AllCh_Json
    {
      get => allCh;
      set => AllCh.Assign(value);
    }

    /// <summary>
    /// Gets a value of AllAr.
    /// </summary>
    [JsonIgnore]
    public Array<AllArGroup> AllAr => allAr ??= new(AllArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AllAr for json serialization.
    /// </summary>
    [JsonPropertyName("allAr")]
    [Computed]
    public IList<AllArGroup> AllAr_Json
    {
      get => allAr;
      set => AllAr.Assign(value);
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public Common First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// Gets a value of AllCaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<AllCaseUnitsGroup> AllCaseUnits => allCaseUnits ??= new(
      AllCaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AllCaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("allCaseUnits")]
    [Computed]
    public IList<AllCaseUnitsGroup> AllCaseUnits_Json
    {
      get => allCaseUnits;
      set => AllCaseUnits.Assign(value);
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of LastCase.
    /// </summary>
    [JsonPropertyName("lastCase")]
    public CaseUnit LastCase
    {
      get => lastCase ??= new();
      set => lastCase = value;
    }

    private CaseUnit new1;
    private Array<AllApGroup> allAp;
    private Array<AllChGroup> allCh;
    private Array<AllArGroup> allAr;
    private Common first;
    private Array<AllCaseUnitsGroup> allCaseUnits;
    private DateWorkArea current;
    private CaseUnit caseUnit;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private CaseUnit lastCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ControlTable Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private ControlTable zdel;
    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePerson child;
    private CsePerson ap;
    private CsePerson ar;
  }
#endregion
}
