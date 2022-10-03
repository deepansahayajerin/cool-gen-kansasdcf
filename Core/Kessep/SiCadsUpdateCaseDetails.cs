// Program: SI_CADS_UPDATE_CASE_DETAILS, ID: 371731796, model: 746.
// Short name: SWE01243
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CADS_UPDATE_CASE_DETAILS.
/// </summary>
[Serializable]
public partial class SiCadsUpdateCaseDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_UPDATE_CASE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsUpdateCaseDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsUpdateCaseDetails.
  /// </summary>
  public SiCadsUpdateCaseDetails(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------
    // 10/20/98  C Deghand  Added SET's for service type indicators for
    //                      update of case.
    // 10/28/98  C Deghand  Added checks for service type change events.
    //                      Added event processing for service type
    //                      events.
    // ----------------------------------------------------------
    // 03/03/99 W.Campbell  Added logic to validate
    //                      that Service Type cannot be changed
    //                      if Expedited Paternity = 'Y' on DB.
    // ----------------------------------------------------------
    // 03/04/99 W.Campbell  Added logic to
    //                      validate that both Service Type and
    //                      Expedited Paternity cannot be
    //                      changed at the same time.
    // ----------------------------------------------------------
    // 06/22/99 W.Campbell  Modified the properties
    //                      of 12 READ statements to
    //                      Select Only.
    // ---------------------------------------------------------
    // 04/22/02 T.Bobb PR00129829  Added check when trying
    //                              to close to check the interstate_rqst
    //                              KS_CSE_IND for spaces. If spaces,
    //                              close intrerstae portion.
    // ---------------------------------------------------------
    // 05/04/06 GVandy  WR230751    Add No Jurisdiction code.
    // ---------------------------------------------------------
    // 07/26/10 JHuss  CQ 376    Added infrastructure record in the event no 
    // case
    // 			  unit is found when adding non-coop.
    // ---------------------------------------------------------
    export.Ar.Assign(import.ArCaseRole);
    export.Case1.Assign(import.Case1);
    local.Current.Date = Now().Date;

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (ReadCase1())
    {
      // ---------------------------------------------------------------
      // 10/28/98  Added checks to see if events need to be raised for service 
      // type changes and set the appropriate infrastructure records.
      // ---------------------------------------------------------------
      if (AsChar(entities.Case1.FullServiceWithMedInd) == 'Y' && AsChar
        (export.Case1.LocateInd) == 'Y')
      {
        // ----------------------------------------------------------
        // 03/03/99 W.Campbell  Added logic to
        // validate that Service Type cannot be
        // changed if Expedited Paternity = 'Y'.
        // ----------------------------------------------------------
        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          ExitState = "INVALID_SERV_CHG_WITH_EXP_PAT";

          return;
        }

        local.RaiseServTypeEvent.Flag = "Y";
        local.Infrastructure.ReasonCode = "FULLNOWLOCONLY";
        local.Text.Text50 = "Full Service with Medical changed to locate only";
      }

      if (AsChar(entities.Case1.FullServiceWithoutMedInd) == 'Y' && AsChar
        (export.Case1.LocateInd) == 'Y')
      {
        // ----------------------------------------------------------
        // 03/03/99 W.Campbell  Added logic to
        // validate that Service Type cannot be
        // changed if Expedited Paternity = 'Y'.
        // ----------------------------------------------------------
        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          ExitState = "INVALID_SERV_CHG_WITH_EXP_PAT";

          return;
        }

        local.RaiseServTypeEvent.Flag = "Y";
        local.Infrastructure.ReasonCode = "FULLNOWLOCONLY";
        local.Text.Text50 = "Full Service w/o Medical changed to locate only";
      }

      if (AsChar(entities.Case1.LocateInd) == 'Y' && AsChar
        (export.Case1.FullServiceWithMedInd) == 'Y')
      {
        // ----------------------------------------------------------
        // 03/03/99 W.Campbell  Added logic to
        // validate that Service Type cannot be
        // changed if Expedited Paternity = 'Y'.
        // ----------------------------------------------------------
        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          ExitState = "INVALID_SERV_CHG_WITH_EXP_PAT";

          return;
        }

        local.RaiseServTypeEvent.Flag = "Y";
        local.Infrastructure.ReasonCode = "LOCONLYNOWFULL";
        local.Text.Text50 = "Locate only changed to Full Service with Medical";
      }

      if (AsChar(entities.Case1.LocateInd) == 'Y' && AsChar
        (export.Case1.FullServiceWithoutMedInd) == 'Y')
      {
        // ----------------------------------------------------------
        // 03/03/99 W.Campbell  Added logic to
        // validate that Service Type cannot be
        // changed if Expedited Paternity = 'Y'.
        // ----------------------------------------------------------
        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          ExitState = "INVALID_SERV_CHG_WITH_EXP_PAT";

          return;
        }

        local.RaiseServTypeEvent.Flag = "Y";
        local.Infrastructure.ReasonCode = "LOCONLYNOWFULL";
        local.Text.Text50 = "Locate only changed to Full Service w/o Medical";
      }

      if (AsChar(entities.Case1.FullServiceWithMedInd) == 'Y' && AsChar
        (export.Case1.FullServiceWithoutMedInd) == 'Y')
      {
        // ----------------------------------------------------------
        // 03/03/99 W.Campbell  Added logic to
        // validate that Service Type cannot be
        // changed if Expedited Paternity = 'Y'.
        // ----------------------------------------------------------
        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          ExitState = "INVALID_SERV_CHG_WITH_EXP_PAT";

          return;
        }

        // : NO EVENTS REQUIRED
      }

      if (AsChar(entities.Case1.FullServiceWithoutMedInd) == 'Y' && AsChar
        (export.Case1.FullServiceWithMedInd) == 'Y')
      {
        // ----------------------------------------------------------
        // 03/03/99 W.Campbell  Added logic to
        // validate that Service Type cannot be
        // changed if Expedited Paternity = 'Y'.
        // ----------------------------------------------------------
        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          ExitState = "INVALID_SERV_CHG_WITH_EXP_PAT";

          return;
        }

        // : NO EVENTS REQUIRED
      }

      if (AsChar(entities.Case1.ExpeditedPaternityInd) != AsChar
        (import.Case1.ExpeditedPaternityInd))
      {
        local.RaiseExpPatEvent.Flag = "Y";

        if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
        {
          local.PreviousExpPat.Flag = "Y";
        }
        else
        {
          local.PreviousExpPat.Flag = "N";
        }
      }

      // ----------------------------------------------------------
      // 03/04/99 W.Campbell  Added logic to
      // validate that both Service Type and
      // Expedited Paternity cannot be
      // changed at the same time.
      // ----------------------------------------------------------
      if (AsChar(local.RaiseExpPatEvent.Flag) == 'Y' && AsChar
        (local.RaiseServTypeEvent.Flag) == 'Y')
      {
        ExitState = "INVALID_SERV_TYP_AND_EXP_PAT_CHG";

        return;
      }

      if (AsChar(entities.Case1.DuplicateCaseIndicator) != AsChar
        (import.Case1.DuplicateCaseIndicator))
      {
        local.RaiseNotDuplicateEvent.Flag = "Y";
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    ReadCsePersonCaseRole();

    if (!entities.ArCaseRole.Populated)
    {
      ExitState = "AR_NF_RB";

      return;
    }

    // ***	Check for Open Interstate Case, Before Case Closure	***
    // 04/22/02 T.BobbAadded ckeck for ks_case_ind = spaces.
    //                  Also changed read to a read each because there
    //                  can be more than one interstate requests.
    if (AsChar(import.Case1.Status) == 'C')
    {
      foreach(var item in ReadInterstateRequest2())
      {
        if (IsEmpty(entities.InterstateRequest.KsCaseInd))
        {
          try
          {
            UpdateInterstateRequest();
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
          ExitState = "INTERSTATE_CASE_HAS_TO_BE_CLOSED";

          return;
        }
      }
    }

    import.Gc.Index = 1;

    for(var limit = import.Gc.Count; import.Gc.Index < limit; ++import.Gc.Index)
    {
      if (!import.Gc.CheckSize())
      {
        break;
      }

      export.Gc.Index = import.Gc.Index;
      export.Gc.CheckSize();

      export.Gc.Update.GgcCommon.SelectChar =
        import.Gc.Item.GgcCommon.SelectChar;
      export.Gc.Update.GgcApCsePersonsWorkSet.Number =
        import.Gc.Item.GgcApCsePersonsWorkSet.Number;
      MoveCaseRole1(import.Gc.Item.GgcApCaseRole, export.Gc.Update.GgcApCaseRole);
        
      export.Gc.Update.GgcPromptSel.SelectChar = "";
      export.Gc.Update.GgcGoodCause.Assign(import.Gc.Item.GgcGoodCause);

      if (AsChar(export.Gc.Item.GgcCommon.SelectChar) == 'S')
      {
        if (!IsEmpty(export.Gc.Item.GgcApCsePersonsWorkSet.Number))
        {
          // ---------------------------------------------------------
          // 06/22/99 W.Campbell - Modified the properties
          // of the following READ statement to Select Only.
          // ---------------------------------------------------------
          if (ReadCsePerson1())
          {
            if (AsChar(entities.Case1.Status) == 'C')
            {
              if (Equal(export.Gc.Item.GgcGoodCause.Code, "CO") || Equal
                (export.Gc.Item.GgcGoodCause.Code, "GC"))
              {
              }
              else
              {
                ExitState = "CLOSED_CASE_GOOD_CAUSE";

                return;
              }
            }

            foreach(var item in ReadGoodCause())
            {
              try
              {
                UpdateGoodCause();

                if (!IsEmpty(export.Gc.Item.GgcApCsePersonsWorkSet.Number))
                {
                  if (ReadCaseRole1())
                  {
                    AssociateGoodCause();

                    goto Test1;
                  }

                  ExitState = "AP_FOR_CASE_NF";

                  return;
                }

Test1:

                local.DateWorkArea.Date =
                  import.Gc.Item.GgcGoodCause.EffectiveDate;

                // ***	Begin Event insertion	***
                if (ReadInterstateRequest1())
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
                local.Infrastructure.EventId = 46;
                local.Infrastructure.BusinessObjectCd = "CAU";
                local.Infrastructure.CaseNumber = import.Case1.Number;
                local.Infrastructure.UserId = "CADS";
                local.Infrastructure.ReferenceDate =
                  import.Gc.Item.GgcGoodCause.EffectiveDate;

                if (Equal(import.Gc.Item.GgcGoodCause.Code, "CO"))
                {
                  local.Infrastructure.ReasonCode = "GOODCAUSETOCOOP";
                }
                else if (Equal(import.Gc.Item.GgcGoodCause.Code, "PD"))
                {
                  local.Infrastructure.ReasonCode = "GOODCAUSEPD";
                }
                else if (Equal(import.Gc.Item.GgcGoodCause.Code, "GC"))
                {
                  local.Infrastructure.ReasonCode = "GOODCAUSE";
                }

                UseCabConvertDate2String2();

                if (!IsEmpty(export.Gc.Item.GgcApCsePersonsWorkSet.Number))
                {
                  local.TextWorkArea.Text30 = "Good Cause determined for AP :";
                  local.Infrastructure.CsePersonNumber =
                    entities.ApCsePerson.Number;
                  local.TextWorkArea.Text10 = "; Type :";
                  local.Infrastructure.Detail =
                    TrimEnd(local.TextWorkArea.Text30) + entities
                    .ApCsePerson.Number + TrimEnd(local.TextWorkArea.Text10) + TrimEnd
                    (import.Gc.Item.GgcGoodCause.Code) + "; Date :" + local
                    .TextWorkArea.Text8;
                  local.Infrastructure.CreatedBy = global.UserId;
                  local.Infrastructure.CreatedTimestamp = Now();
                  local.CaseUnitFound.Flag = "N";

                  foreach(var item1 in ReadCaseUnit1())
                  {
                    local.Infrastructure.CaseUnitNumber =
                      entities.CaseUnit.CuNumber;
                    local.CaseUnitFound.Flag = "Y";
                    UseSpCabCreateInfrastructure();

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                    }
                    else
                    {
                      return;
                    }
                  }

                  // 07/26/10 JHuss CQ# 376	Added check to create infrastructure
                  // if no case unit found
                  if (AsChar(local.CaseUnitFound.Flag) == 'N')
                  {
                    UseSpCabCreateInfrastructure();
                  }
                }
                else
                {
                  local.TextWorkArea.Text30 = "Good Cause established for AR:";
                  local.Infrastructure.CsePersonNumber =
                    entities.ArCsePerson.Number;
                  local.TextWorkArea.Text10 = "; Type :";
                  local.Infrastructure.Detail =
                    TrimEnd(local.TextWorkArea.Text30) + entities
                    .ArCsePerson.Number + TrimEnd(local.TextWorkArea.Text10) + TrimEnd
                    (import.Gc.Item.GgcGoodCause.Code) + "; Date :" + local
                    .TextWorkArea.Text8;
                  local.Infrastructure.CreatedBy = global.UserId;
                  local.Infrastructure.CreatedTimestamp = Now();
                  local.CaseUnitFound.Flag = "N";

                  foreach(var item1 in ReadCaseUnit2())
                  {
                    local.Infrastructure.CaseUnitNumber =
                      entities.CaseUnit.CuNumber;
                    local.CaseUnitFound.Flag = "Y";
                    UseSpCabCreateInfrastructure();

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                    }
                    else
                    {
                      return;
                    }
                  }

                  // 07/26/10 JHuss CQ# 376	Added check to create infrastructure
                  // if no case unit found
                  if (AsChar(local.CaseUnitFound.Flag) == 'N')
                  {
                    UseSpCabCreateInfrastructure();
                  }
                }

                // ***	End Event insertion	***
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "GOOD_CAUSE_ALREADY_EXISTS";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "GOOD_CAUSE_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else
          {
            ExitState = "AP_FOR_CASE_NF";

            return;
          }
        }

        // ---------------------------------------------------------
        // 06/22/99 W.Campbell - Modified the properties
        // of the following READ statement to Select Only.
        // ---------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
      }
    }

    import.Gc.CheckIndex();
    import.Nc.Index = 1;

    for(var limit = import.Nc.Count; import.Nc.Index < limit; ++import.Nc.Index)
    {
      if (!import.Nc.CheckSize())
      {
        break;
      }

      export.Nc.Index = import.Nc.Index;
      export.Nc.CheckSize();

      export.Nc.Update.GncCommon.SelectChar = "";
      export.Nc.Update.GncPromptSel.SelectChar = "";
      export.Nc.Update.GncRsnPrompt.SelectChar = "";
      export.Nc.Update.GncApCsePersonsWorkSet.Number =
        import.Nc.Item.GncApCsePersonsWorkSet.Number;
      MoveCaseRole1(import.Nc.Item.GncApCaseRole, export.Nc.Update.GncApCaseRole);
        
      export.Nc.Update.GncNonCooperation.
        Assign(import.Nc.Item.GncNonCooperation);

      if (AsChar(import.Nc.Item.GncCommon.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Nc.Item.GncApCsePersonsWorkSet.Number))
        {
          // ---------------------------------------------------------
          // 06/22/99 W.Campbell - Modified the properties
          // of the following READ statement to Select Only.
          // ---------------------------------------------------------
          if (ReadCsePerson2())
          {
            // ---------------------------------------------------------
            // 06/22/99 W.Campbell - Modified the properties
            // of the following READ statement to Select Only.
            // ---------------------------------------------------------
            if (AsChar(entities.Case1.Status) == 'C')
            {
              if (AsChar(export.Nc.Item.GncNonCooperation.Code) == 'C' || AsChar
                (export.Nc.Item.GncNonCooperation.Code) == 'N')
              {
              }
              else
              {
                ExitState = "CASE_CLOSED_NON_COOP";

                return;
              }
            }

            foreach(var item in ReadNonCooperation())
            {
              local.DateWorkArea.Date =
                export.Nc.Item.GncNonCooperation.EffectiveDate;
              local.NcChangedToN.Flag = "N";
              local.NcChangedToY.Flag = "N";

              if (AsChar(export.Nc.Item.GncNonCooperation.Code) == 'Y')
              {
                if (AsChar(entities.NonCooperation.Code) != 'Y')
                {
                  local.NcChangedToY.Flag = "Y";
                }
              }

              if (AsChar(export.Nc.Item.GncNonCooperation.Code) != 'Y')
              {
                if (AsChar(entities.NonCooperation.Code) == 'Y')
                {
                  local.NcChangedToN.Flag = "Y";
                }
              }

              try
              {
                UpdateNonCooperation();

                if (!IsEmpty(export.Nc.Item.GncApCsePersonsWorkSet.Number))
                {
                  if (ReadCaseRole1())
                  {
                    AssociateNonCooperation();

                    goto Test2;
                  }

                  ExitState = "AP_FOR_CASE_NF";

                  return;

                  // added the above read to associate it with the ap
                }

Test2:

                local.DateWorkArea.Date =
                  import.Nc.Item.GncNonCooperation.EffectiveDate;

                // ***	Begin Event insertion	***
                if (ReadInterstateRequest1())
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
                local.Infrastructure.EventId = 46;
                local.Infrastructure.BusinessObjectCd = "CAU";
                local.Infrastructure.CaseNumber = import.Case1.Number;
                local.Infrastructure.UserId = "CADS";
                local.Infrastructure.ReferenceDate = local.DateWorkArea.Date;
                UseCabConvertDate2String2();

                if (AsChar(import.Nc.Item.GncNonCooperation.Code) == 'C')
                {
                  local.Infrastructure.ReasonCode = "NONCOOPSETTOC";
                }
                else if (AsChar(import.Nc.Item.GncNonCooperation.Code) == 'Y')
                {
                  local.Infrastructure.ReasonCode = "NONCOOPSETTOY";
                }
                else if (AsChar(import.Nc.Item.GncNonCooperation.Code) == 'N')
                {
                  local.Infrastructure.ReasonCode = "NONCOOPSETTON";
                }

                if (!IsEmpty(export.Nc.Item.GncApCsePersonsWorkSet.Number))
                {
                  local.TextWorkArea.Text30 = "Non Coop determined for AP :";
                  local.Infrastructure.CsePersonNumber =
                    entities.ApCsePerson.Number;
                  local.Infrastructure.Detail =
                    TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                    (entities.ApCsePerson.Number) + "; " + TrimEnd("Type :") + TrimEnd
                    (import.Nc.Item.GncNonCooperation.Code) + "; Reason :" + (
                      import.Nc.Item.GncNonCooperation.Reason ?? "") + "; Date :" +
                    local.TextWorkArea.Text8;
                  local.Infrastructure.CreatedBy = global.UserId;
                  local.Infrastructure.CreatedTimestamp = Now();
                  local.CaseUnitFound.Flag = "N";

                  foreach(var item1 in ReadCaseUnit1())
                  {
                    local.Infrastructure.CaseUnitNumber =
                      entities.CaseUnit.CuNumber;
                    local.CaseUnitFound.Flag = "Y";
                    UseSpCabCreateInfrastructure();

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                    }
                    else
                    {
                      return;
                    }
                  }

                  // 07/26/10 JHuss CQ# 376	Added check to create infrastructure
                  // if no case unit found
                  if (AsChar(local.CaseUnitFound.Flag) == 'N')
                  {
                    UseSpCabCreateInfrastructure();
                  }
                }
                else
                {
                  local.TextWorkArea.Text30 = "Non Coop established for AR :";
                  local.Infrastructure.CsePersonNumber =
                    entities.ArCsePerson.Number;
                  local.Infrastructure.Detail =
                    TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                    (entities.ArCsePerson.Number) + "; " + TrimEnd("Type :") + TrimEnd
                    (import.Nc.Item.GncNonCooperation.Code) + "; Reason :" + (
                      import.Nc.Item.GncNonCooperation.Reason ?? "") + "; Date :" +
                    local.TextWorkArea.Text8;
                  local.Infrastructure.CreatedBy = global.UserId;
                  local.Infrastructure.CreatedTimestamp = Now();
                  local.CaseUnitFound.Flag = "N";

                  foreach(var item1 in ReadCaseUnit2())
                  {
                    local.Infrastructure.CaseUnitNumber =
                      entities.CaseUnit.CuNumber;
                    local.CaseUnitFound.Flag = "Y";
                    UseSpCabCreateInfrastructure();

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                    }
                    else
                    {
                      return;
                    }
                  }

                  // 07/26/10 JHuss CQ# 376	Added check to create infrastructure
                  // if no case unit found
                  if (AsChar(local.CaseUnitFound.Flag) == 'N')
                  {
                    UseSpCabCreateInfrastructure();
                  }
                }

                // ***	End Event insertion	***
                // ***	Begin External Alert insertion	***
                if (!IsEmpty(import.Nc.Item.GncNonCooperation.Code))
                {
                  if (AsChar(import.Nc.Item.GncNonCooperation.Code) == 'Y')
                  {
                    local.InterfaceAlert.AlertCode = "40";
                  }
                  else
                  {
                    local.InterfaceAlert.AlertCode = "41";
                  }

                  UseSpCadsExternalAlert();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                // ***	End External Alert insertion	***
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "NON_COOP_ALREADY_EXISTS";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "NON_COOP_PVV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else
          {
            ExitState = "AP_FOR_CASE_NF";

            return;
          }
        }

        // ---------------------------------------------------------
        // 06/22/99 W.Campbell - Modified the properties
        // of the following READ statement to Select Only.
        // ---------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
      }
    }

    import.Nc.CheckIndex();

    // Update ap data if selection was made
    export.Case1.Assign(import.Case1);
    export.Ar.Assign(import.ArCaseRole);

    if (AsChar(import.DataChanged.Flag) == 'B')
    {
      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCase2())
      {
        // ------------------------------------------------------------
        // 10/20/98 Added set statements for service indicators.
        // ------------------------------------------------------------
        try
        {
          UpdateCase();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

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
        ExitState = "CASE_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCaseRole2())
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
              ExitState = "CASE_ROLE_NU";

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
      else
      {
        ExitState = "CASE_ROLE_AR_NF";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else if (AsChar(import.DataChanged.Flag) == 'C')
    {
      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCase2())
      {
        // ------------------------------------------------------------
        // 10/20/98 Added set statements for service indicators.
        // ------------------------------------------------------------
        try
        {
          UpdateCase();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

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
        ExitState = "CASE_NF";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else if (AsChar(import.DataChanged.Flag) == 'R')
    {
      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCaseRole2())
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
              ExitState = "CASE_ROLE_NU";

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
      else
      {
        ExitState = "CASE_ROLE_AR_NF";
      }
    }

    // -------------------------------------------------------------
    // 10/28/98 Added event processing for service type events.
    // -------------------------------------------------------------
    if (AsChar(local.RaiseServTypeEvent.Flag) == 'Y')
    {
      // ***	Begin Event insertion	***
      if (ReadInterstateRequest1())
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
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.CaseNumber = import.Case1.Number;
      local.Infrastructure.UserId = "CADS";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      UseCabConvertDate2String1();
      local.TextWorkArea.Text10 = "; Date :";
      local.Infrastructure.Detail = TrimEnd(local.Text.Text50) + TrimEnd
        (local.TextWorkArea.Text10) + local.TextWorkArea.Text8;

      foreach(var item in ReadCaseUnit3())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }

      if (entities.CaseUnit.CuNumber == 0)
      {
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }

      // ***	End Event insertion	***
    }

    if (AsChar(local.RaiseExpPatEvent.Flag) == 'Y')
    {
      // ***	Begin Event insertion	***
      if (ReadInterstateRequest1())
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
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.CaseNumber = import.Case1.Number;
      local.Infrastructure.UserId = "CADS";
      local.Infrastructure.ReferenceDate = local.Current.Date;

      if (AsChar(local.PreviousExpPat.Flag) == 'Y')
      {
        local.Infrastructure.ReasonCode = "EXPATNOWFULL";
        local.Text.Text50 = "Expedited Paternity Service Discontinued";
      }
      else
      {
        local.Infrastructure.ReasonCode = "EXPPAT";
        local.Text.Text50 = "Expedited Paternity Service Requested";
      }

      UseCabConvertDate2String1();
      local.TextWorkArea.Text10 = "; Date :";
      local.Infrastructure.Detail = TrimEnd(local.Text.Text50) + TrimEnd
        (local.TextWorkArea.Text10) + local.TextWorkArea.Text8;

      foreach(var item in ReadCaseUnit3())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }

      if (entities.CaseUnit.CuNumber == 0)
      {
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }

      // ***	End Event insertion	***
    }

    if (AsChar(local.RaiseNotDuplicateEvent.Flag) == 'Y')
    {
      // ***	Begin Event insertion	***
      local.Infrastructure.InitiatingStateCode = "KS";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 5;
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.CaseNumber = import.Case1.Number;
      local.Infrastructure.UserId = "CADS";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.ReasonCode = "MANUAL_CLS_NOTD";
      local.Infrastructure.Detail =
        "The CSE Case is manually marked as no longer a Duplicate Case.";

      foreach(var item in ReadCaseUnit3())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }

      if (entities.CaseUnit.CuNumber == 0)
      {
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.CreatedTimestamp = Now();
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }

      // ***	End Event insertion	***
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseCabSetMaximumDiscontinueDate();

      if (Equal(export.Ar.AssignmentDate, local.Max.Date))
      {
        export.Ar.AssignmentDate = null;
      }

      if (Equal(export.Ar.AssignmentTerminatedDt, local.Max.Date))
      {
        export.Ar.AssignmentTerminatedDt = null;
      }

      for(export.Gc.Index = 1; export.Gc.Index < export.Gc.Count; ++
        export.Gc.Index)
      {
        if (!export.Gc.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Gc.Item.GgcGoodCause.Code))
        {
          if (Equal(export.Gc.Item.GgcGoodCause.EffectiveDate, local.Max.Date))
          {
            export.Gc.Update.GgcGoodCause.EffectiveDate = null;
          }
        }
        else
        {
          break;
        }
      }

      export.Gc.CheckIndex();

      for(export.Nc.Index = 1; export.Nc.Index < export.Nc.Count; ++
        export.Nc.Index)
      {
        if (!export.Nc.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Nc.Item.GncNonCooperation.Code))
        {
          if (Equal(export.Nc.Item.GncNonCooperation.EffectiveDate,
            local.Max.Date))
          {
            export.Nc.Update.GncNonCooperation.EffectiveDate = null;
          }
        }
        else
        {
          return;
        }
      }

      export.Nc.CheckIndex();
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private void UseCabConvertDate2String1()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabConvertDate2String2()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCadsExternalAlert()
  {
    var useImport = new SpCadsExternalAlert.Import();
    var useExport = new SpCadsExternalAlert.Export();

    useImport.NonCooperation.Reason = entities.NonCooperation.Reason;
    MoveCaseRole2(entities.ApCaseRole, useImport.Ap);
    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    MoveCase1(export.Case1, useImport.Case1);

    Call(SpCadsExternalAlert.Execute, useImport, useExport);
  }

  private void AssociateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.GoodCause.Populated);
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);

    var casNumber1 = entities.ApCaseRole.CasNumber;
    var cspNumber1 = entities.ApCaseRole.CspNumber;
    var croType1 = entities.ApCaseRole.Type1;
    var croIdentifier1 = entities.ApCaseRole.Identifier;

    CheckValid<GoodCause>("CroType1", croType1);
    entities.GoodCause.Populated = false;
    Update("AssociateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.GoodCause.CasNumber);
        db.SetString(command, "cspNumber", entities.GoodCause.CspNumber);
        db.SetString(command, "croType", entities.GoodCause.CroType);
        db.SetInt32(command, "croIdentifier", entities.GoodCause.CroIdentifier);
      });

    entities.GoodCause.CasNumber1 = casNumber1;
    entities.GoodCause.CspNumber1 = cspNumber1;
    entities.GoodCause.CroType1 = croType1;
    entities.GoodCause.CroIdentifier1 = croIdentifier1;
    entities.GoodCause.Populated = true;
  }

  private void AssociateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.NonCooperation.Populated);
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);

    var casNumber1 = entities.ApCaseRole.CasNumber;
    var cspNumber1 = entities.ApCaseRole.CspNumber;
    var croType1 = entities.ApCaseRole.Type1;
    var croIdentifier1 = entities.ApCaseRole.Identifier;

    CheckValid<NonCooperation>("CroType1", croType1);
    entities.NonCooperation.Populated = false;
    Update("AssociateNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NonCooperation.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.NonCooperation.CasNumber);
        db.SetString(command, "cspNumber", entities.NonCooperation.CspNumber);
        db.SetString(command, "croType", entities.NonCooperation.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.NonCooperation.CroIdentifier);
      });

    entities.NonCooperation.CasNumber1 = casNumber1;
    entities.NonCooperation.CspNumber1 = cspNumber1;
    entities.NonCooperation.CroType1 = croType1;
    entities.NonCooperation.CroIdentifier1 = croIdentifier1;
    entities.NonCooperation.Populated = true;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 1);
        entities.Case1.LocateInd = db.GetNullableString(reader, 2);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Case1.Status = db.GetNullableString(reader, 5);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 6);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 8);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 10);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 11);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 12);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 13);
        entities.Case1.Note = db.GetNullableString(reader, 14);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 15);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
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
        entities.Case1.ClosureReason = db.GetNullableString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Case1.Status = db.GetNullableString(reader, 5);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 6);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 8);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 10);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 11);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 12);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 13);
        entities.Case1.Note = db.GetNullableString(reader, 14);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 15);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.AssignmentDate = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 7);
        entities.ApCaseRole.AssignmentOfRights =
          db.GetNullableString(reader, 8);
        entities.ApCaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 9);
        entities.ApCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.ApCaseRole.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ApCaseRole.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ApCaseRole.CreatedBy = db.GetString(reader, 13);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ArCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.ArCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCaseRole.AssignmentDate = db.GetNullableDate(reader, 6);
        entities.ArCaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 7);
        entities.ArCaseRole.AssignmentOfRights =
          db.GetNullableString(reader, 8);
        entities.ArCaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 9);
        entities.ArCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.ArCaseRole.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ArCaseRole.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ArCaseRole.CreatedBy = db.GetString(reader, 13);
        entities.ArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.ApCsePerson.Number);
        db.SetNullableString(command, "cspNoAr", entities.ArCsePerson.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
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
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAr", entities.ArCsePerson.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
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

  private IEnumerable<bool> ReadCaseUnit3()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit3",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
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

  private bool ReadCsePerson1()
  {
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.Gc.Item.GgcApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.Nc.Item.GncApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCsePersonCaseRole()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "numb", import.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ArCsePerson.Number = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ArCsePerson.Type1 = db.GetString(reader, 1);
        entities.ArCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ArCaseRole.CasNumber = db.GetString(reader, 3);
        entities.ArCaseRole.Type1 = db.GetString(reader, 4);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 5);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.ArCaseRole.AssignmentDate = db.GetNullableDate(reader, 8);
        entities.ArCaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 9);
        entities.ArCaseRole.AssignmentOfRights =
          db.GetNullableString(reader, 10);
        entities.ArCaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 11);
        entities.ArCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.ArCaseRole.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.ArCaseRole.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ArCaseRole.CreatedBy = db.GetString(reader, 15);
        entities.ArCaseRole.Populated = true;
        entities.ArCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return ReadEach("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedBy = db.GetNullableString(reader, 3);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.GoodCause.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.GoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.GoodCause.CasNumber = db.GetString(reader, 7);
        entities.GoodCause.CspNumber = db.GetString(reader, 8);
        entities.GoodCause.CroType = db.GetString(reader, 9);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 10);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 11);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 12);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 13);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 14);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);

        return true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNonCooperation()
  {
    entities.NonCooperation.Populated = false;

    return ReadEach("ReadNonCooperation",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.Reason = db.GetNullableString(reader, 1);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 2);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.NonCooperation.CreatedBy = db.GetString(reader, 4);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.NonCooperation.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.NonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.NonCooperation.CasNumber = db.GetString(reader, 8);
        entities.NonCooperation.CspNumber = db.GetString(reader, 9);
        entities.NonCooperation.CroType = db.GetString(reader, 10);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 11);
        entities.NonCooperation.CasNumber1 = db.GetNullableString(reader, 12);
        entities.NonCooperation.CspNumber1 = db.GetNullableString(reader, 13);
        entities.NonCooperation.CroType1 = db.GetNullableString(reader, 14);
        entities.NonCooperation.CroIdentifier1 =
          db.GetNullableInt32(reader, 15);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
        CheckValid<NonCooperation>("CroType1", entities.NonCooperation.CroType1);
          

        return true;
      });
  }

  private void UpdateCase()
  {
    var fullServiceWithoutMedInd = export.Case1.FullServiceWithoutMedInd ?? "";
    var fullServiceWithMedInd = export.Case1.FullServiceWithMedInd ?? "";
    var locateInd = export.Case1.LocateInd ?? "";
    var closureReason = export.Case1.ClosureReason ?? "";
    var status = export.Case1.Status ?? "";
    var statusDate = export.Case1.StatusDate;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var expeditedPaternityInd = export.Case1.ExpeditedPaternityInd ?? "";
    var paMedicalService = export.Case1.PaMedicalService ?? "";
    var closureLetterDate = export.Case1.ClosureLetterDate;
    var duplicateCaseIndicator = export.Case1.DuplicateCaseIndicator ?? "";
    var note = export.Case1.Note ?? "";
    var noJurisdictionCd = export.Case1.NoJurisdictionCd ?? "";

    entities.Case1.Populated = false;
    Update("UpdateCase",
      (db, command) =>
      {
        db.
          SetNullableString(command, "fullSrvWoMedIn", fullServiceWithoutMedInd);
          
        db.SetNullableString(command, "fullServWMedIn", fullServiceWithMedInd);
        db.SetNullableString(command, "locateInd", locateInd);
        db.SetNullableString(command, "closureReason", closureReason);
        db.SetNullableString(command, "status", status);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "expedidedPatInd", expeditedPaternityInd);
        db.SetNullableString(command, "paMedicalService", paMedicalService);
        db.SetNullableDate(command, "closureLetrDate", closureLetterDate);
        db.
          SetNullableString(command, "dupCaseIndicator", duplicateCaseIndicator);
          
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "noJurisdictionCd", noJurisdictionCd);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.FullServiceWithoutMedInd = fullServiceWithoutMedInd;
    entities.Case1.FullServiceWithMedInd = fullServiceWithMedInd;
    entities.Case1.LocateInd = locateInd;
    entities.Case1.ClosureReason = closureReason;
    entities.Case1.Status = status;
    entities.Case1.StatusDate = statusDate;
    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.ExpeditedPaternityInd = expeditedPaternityInd;
    entities.Case1.PaMedicalService = paMedicalService;
    entities.Case1.ClosureLetterDate = closureLetterDate;
    entities.Case1.DuplicateCaseIndicator = duplicateCaseIndicator;
    entities.Case1.Note = note;
    entities.Case1.NoJurisdictionCd = noJurisdictionCd;
    entities.Case1.Populated = true;
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.ArCaseRole.Populated);

    var assignmentTerminationCode =
      import.ArCaseRole.AssignmentTerminationCode ?? "";
    var assignmentTerminatedDt = import.ArCaseRole.AssignmentTerminatedDt;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.ArCaseRole.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.SetNullableString(
          command, "assignmentTermCd", assignmentTerminationCode);
        db.SetNullableDate(command, "assignmentTermDt", assignmentTerminatedDt);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.ArCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ArCaseRole.CspNumber);
        db.SetString(command, "type", entities.ArCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.ArCaseRole.Identifier);
      });

    entities.ArCaseRole.AssignmentTerminationCode = assignmentTerminationCode;
    entities.ArCaseRole.AssignmentTerminatedDt = assignmentTerminatedDt;
    entities.ArCaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ArCaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.ArCaseRole.Populated = true;
  }

  private void UpdateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.GoodCause.Populated);

    var code1 = export.Gc.Item.GgcGoodCause.Code ?? "";
    var effectiveDate = export.Gc.Item.GgcGoodCause.EffectiveDate;
    var lastUpdatedTimestamp = Now();

    entities.GoodCause.Populated = false;
    Update("UpdateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code1);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.GoodCause.CasNumber);
        db.SetString(command, "cspNumber", entities.GoodCause.CspNumber);
        db.SetString(command, "croType", entities.GoodCause.CroType);
        db.SetInt32(command, "croIdentifier", entities.GoodCause.CroIdentifier);
      });

    entities.GoodCause.Code = code1;
    entities.GoodCause.EffectiveDate = effectiveDate;
    entities.GoodCause.LastUpdatedBy = "";
    entities.GoodCause.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.GoodCause.Populated = true;
  }

  private void UpdateInterstateRequest()
  {
    var otherStateCaseStatus = "C";

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.Populated = true;
  }

  private void UpdateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.NonCooperation.Populated);

    var code1 = export.Nc.Item.GncNonCooperation.Code ?? "";
    var reason = export.Nc.Item.GncNonCooperation.Reason ?? "";
    var effectiveDate = export.Nc.Item.GncNonCooperation.EffectiveDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.NonCooperation.Populated = false;
    Update("UpdateNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code1);
        db.SetNullableString(command, "reason", reason);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NonCooperation.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.NonCooperation.CasNumber);
        db.SetString(command, "cspNumber", entities.NonCooperation.CspNumber);
        db.SetString(command, "croType", entities.NonCooperation.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.NonCooperation.CroIdentifier);
      });

    entities.NonCooperation.Code = code1;
    entities.NonCooperation.Reason = reason;
    entities.NonCooperation.EffectiveDate = effectiveDate;
    entities.NonCooperation.LastUpdatedBy = lastUpdatedBy;
    entities.NonCooperation.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NonCooperation.Populated = true;
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
    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of GncRsnPrompt.
      /// </summary>
      [JsonPropertyName("gncRsnPrompt")]
      public Common GncRsnPrompt
      {
        get => gncRsnPrompt ??= new();
        set => gncRsnPrompt = value;
      }

      /// <summary>
      /// A value of GncCdPrmpt.
      /// </summary>
      [JsonPropertyName("gncCdPrmpt")]
      public Common GncCdPrmpt
      {
        get => gncCdPrmpt ??= new();
        set => gncCdPrmpt = value;
      }

      /// <summary>
      /// A value of GncCommon.
      /// </summary>
      [JsonPropertyName("gncCommon")]
      public Common GncCommon
      {
        get => gncCommon ??= new();
        set => gncCommon = value;
      }

      /// <summary>
      /// A value of GncApCaseRole.
      /// </summary>
      [JsonPropertyName("gncApCaseRole")]
      public CaseRole GncApCaseRole
      {
        get => gncApCaseRole ??= new();
        set => gncApCaseRole = value;
      }

      /// <summary>
      /// A value of GncApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gncApCsePersonsWorkSet")]
      public CsePersonsWorkSet GncApCsePersonsWorkSet
      {
        get => gncApCsePersonsWorkSet ??= new();
        set => gncApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GncNonCooperation.
      /// </summary>
      [JsonPropertyName("gncNonCooperation")]
      public NonCooperation GncNonCooperation
      {
        get => gncNonCooperation ??= new();
        set => gncNonCooperation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common gncRsnPrompt;
      private Common gncCdPrmpt;
      private Common gncCommon;
      private CaseRole gncApCaseRole;
      private CsePersonsWorkSet gncApCsePersonsWorkSet;
      private NonCooperation gncNonCooperation;
    }

    /// <summary>A GcGroup group.</summary>
    [Serializable]
    public class GcGroup
    {
      /// <summary>
      /// A value of GgcCdPrmpt.
      /// </summary>
      [JsonPropertyName("ggcCdPrmpt")]
      public Common GgcCdPrmpt
      {
        get => ggcCdPrmpt ??= new();
        set => ggcCdPrmpt = value;
      }

      /// <summary>
      /// A value of GgcCommon.
      /// </summary>
      [JsonPropertyName("ggcCommon")]
      public Common GgcCommon
      {
        get => ggcCommon ??= new();
        set => ggcCommon = value;
      }

      /// <summary>
      /// A value of GgcApCaseRole.
      /// </summary>
      [JsonPropertyName("ggcApCaseRole")]
      public CaseRole GgcApCaseRole
      {
        get => ggcApCaseRole ??= new();
        set => ggcApCaseRole = value;
      }

      /// <summary>
      /// A value of GgcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("ggcApCsePersonsWorkSet")]
      public CsePersonsWorkSet GgcApCsePersonsWorkSet
      {
        get => ggcApCsePersonsWorkSet ??= new();
        set => ggcApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GgcGoodCause.
      /// </summary>
      [JsonPropertyName("ggcGoodCause")]
      public GoodCause GgcGoodCause
      {
        get => ggcGoodCause ??= new();
        set => ggcGoodCause = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ggcCdPrmpt;
      private Common ggcCommon;
      private CaseRole ggcApCaseRole;
      private CsePersonsWorkSet ggcApCsePersonsWorkSet;
      private GoodCause ggcGoodCause;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of DataChanged.
    /// </summary>
    [JsonPropertyName("dataChanged")]
    public Common DataChanged
    {
      get => dataChanged ??= new();
      set => dataChanged = value;
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Gc.
    /// </summary>
    [JsonIgnore]
    public Array<GcGroup> Gc => gc ??= new(GcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Gc for json serialization.
    /// </summary>
    [JsonPropertyName("gc")]
    [Computed]
    public IList<GcGroup> Gc_Json
    {
      get => gc;
      set => Gc.Assign(value);
    }

    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Case1 case1;
    private Program program;
    private CaseRole arCaseRole;
    private Common dataChanged;
    private Array<NcGroup> nc;
    private Array<GcGroup> gc;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GcGroup group.</summary>
    [Serializable]
    public class GcGroup
    {
      /// <summary>
      /// A value of GgcPromptSel.
      /// </summary>
      [JsonPropertyName("ggcPromptSel")]
      public Common GgcPromptSel
      {
        get => ggcPromptSel ??= new();
        set => ggcPromptSel = value;
      }

      /// <summary>
      /// A value of GgcCommon.
      /// </summary>
      [JsonPropertyName("ggcCommon")]
      public Common GgcCommon
      {
        get => ggcCommon ??= new();
        set => ggcCommon = value;
      }

      /// <summary>
      /// A value of GgcApCaseRole.
      /// </summary>
      [JsonPropertyName("ggcApCaseRole")]
      public CaseRole GgcApCaseRole
      {
        get => ggcApCaseRole ??= new();
        set => ggcApCaseRole = value;
      }

      /// <summary>
      /// A value of GgcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("ggcApCsePersonsWorkSet")]
      public CsePersonsWorkSet GgcApCsePersonsWorkSet
      {
        get => ggcApCsePersonsWorkSet ??= new();
        set => ggcApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GgcGoodCause.
      /// </summary>
      [JsonPropertyName("ggcGoodCause")]
      public GoodCause GgcGoodCause
      {
        get => ggcGoodCause ??= new();
        set => ggcGoodCause = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ggcPromptSel;
      private Common ggcCommon;
      private CaseRole ggcApCaseRole;
      private CsePersonsWorkSet ggcApCsePersonsWorkSet;
      private GoodCause ggcGoodCause;
    }

    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of GncRsnPrompt.
      /// </summary>
      [JsonPropertyName("gncRsnPrompt")]
      public Common GncRsnPrompt
      {
        get => gncRsnPrompt ??= new();
        set => gncRsnPrompt = value;
      }

      /// <summary>
      /// A value of GncPromptSel.
      /// </summary>
      [JsonPropertyName("gncPromptSel")]
      public Common GncPromptSel
      {
        get => gncPromptSel ??= new();
        set => gncPromptSel = value;
      }

      /// <summary>
      /// A value of GncCommon.
      /// </summary>
      [JsonPropertyName("gncCommon")]
      public Common GncCommon
      {
        get => gncCommon ??= new();
        set => gncCommon = value;
      }

      /// <summary>
      /// A value of GncApCaseRole.
      /// </summary>
      [JsonPropertyName("gncApCaseRole")]
      public CaseRole GncApCaseRole
      {
        get => gncApCaseRole ??= new();
        set => gncApCaseRole = value;
      }

      /// <summary>
      /// A value of GncApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gncApCsePersonsWorkSet")]
      public CsePersonsWorkSet GncApCsePersonsWorkSet
      {
        get => gncApCsePersonsWorkSet ??= new();
        set => gncApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GncNonCooperation.
      /// </summary>
      [JsonPropertyName("gncNonCooperation")]
      public NonCooperation GncNonCooperation
      {
        get => gncNonCooperation ??= new();
        set => gncNonCooperation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common gncRsnPrompt;
      private Common gncPromptSel;
      private Common gncCommon;
      private CaseRole gncApCaseRole;
      private CsePersonsWorkSet gncApCsePersonsWorkSet;
      private NonCooperation gncNonCooperation;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

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
    /// Gets a value of Gc.
    /// </summary>
    [JsonIgnore]
    public Array<GcGroup> Gc => gc ??= new(GcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Gc for json serialization.
    /// </summary>
    [JsonPropertyName("gc")]
    [Computed]
    public IList<GcGroup> Gc_Json
    {
      get => gc;
      set => Gc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
    }

    private Case1 case1;
    private Program program;
    private CaseRole ar;
    private Array<GcGroup> gc;
    private Array<NcGroup> nc;
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
    /// A value of RaiseServTypeEvent.
    /// </summary>
    [JsonPropertyName("raiseServTypeEvent")]
    public Common RaiseServTypeEvent
    {
      get => raiseServTypeEvent ??= new();
      set => raiseServTypeEvent = value;
    }

    /// <summary>
    /// A value of RaiseNotDuplicateEvent.
    /// </summary>
    [JsonPropertyName("raiseNotDuplicateEvent")]
    public Common RaiseNotDuplicateEvent
    {
      get => raiseNotDuplicateEvent ??= new();
      set => raiseNotDuplicateEvent = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public WorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of PreviousExpPat.
    /// </summary>
    [JsonPropertyName("previousExpPat")]
    public Common PreviousExpPat
    {
      get => previousExpPat ??= new();
      set => previousExpPat = value;
    }

    /// <summary>
    /// A value of RaiseExpPatEvent.
    /// </summary>
    [JsonPropertyName("raiseExpPatEvent")]
    public Common RaiseExpPatEvent
    {
      get => raiseExpPatEvent ??= new();
      set => raiseExpPatEvent = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Ggc.
    /// </summary>
    [JsonPropertyName("ggc")]
    public Common Ggc
    {
      get => ggc ??= new();
      set => ggc = value;
    }

    /// <summary>
    /// A value of Gnc.
    /// </summary>
    [JsonPropertyName("gnc")]
    public Common Gnc
    {
      get => gnc ??= new();
      set => gnc = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ErrOnAdabas.
    /// </summary>
    [JsonPropertyName("errOnAdabas")]
    public Common ErrOnAdabas
    {
      get => errOnAdabas ??= new();
      set => errOnAdabas = value;
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
    /// A value of NcChangedToN.
    /// </summary>
    [JsonPropertyName("ncChangedToN")]
    public Common NcChangedToN
    {
      get => ncChangedToN ??= new();
      set => ncChangedToN = value;
    }

    /// <summary>
    /// A value of NcChangedToY.
    /// </summary>
    [JsonPropertyName("ncChangedToY")]
    public Common NcChangedToY
    {
      get => ncChangedToY ??= new();
      set => ncChangedToY = value;
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

    private Common caseUnitFound;
    private Common raiseServTypeEvent;
    private Common raiseNotDuplicateEvent;
    private WorkArea text;
    private Common previousExpPat;
    private Common raiseExpPatEvent;
    private DateWorkArea current;
    private Code code;
    private CodeValue codeValue;
    private Common common;
    private Common ggc;
    private Common gnc;
    private DateWorkArea max;
    private CaseUnit caseUnit;
    private Infrastructure infrastructure;
    private InterfaceAlert interfaceAlert;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Common errOnAdabas;
    private TextWorkArea textWorkArea;
    private Common ncChangedToN;
    private Common ncChangedToY;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private CaseRole apCaseRole;
    private Case1 case1;
    private CaseRole arCaseRole;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private CsePerson arCsePerson;
    private CsePerson apCsePerson;
  }
#endregion
}
