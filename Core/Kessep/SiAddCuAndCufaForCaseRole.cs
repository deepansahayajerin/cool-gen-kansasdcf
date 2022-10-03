// Program: SI_ADD_CU_AND_CUFA_FOR_CASE_ROLE, ID: 371785569, model: 746.
// Short name: SWE01781
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
/// A program: SI_ADD_CU_AND_CUFA_FOR_CASE_ROLE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block will create new case units and case unit function 
/// assignments when a new CH or AP are added to the case.  AR case unit
/// creation will be handled in a different action block as the new AR will be
/// replacing the current AR.  If the CH or AP existed before in the case, the
/// case units and case unit function assignments will be reactivated instead of
/// creating new records.
/// </para>
/// </summary>
[Serializable]
public partial class SiAddCuAndCufaForCaseRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ADD_CU_AND_CUFA_FOR_CASE_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAddCuAndCufaForCaseRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAddCuAndCufaForCaseRole.
  /// </summary>
  public SiAddCuAndCufaForCaseRole(IContext context, Import import,
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
    // 11-26-96  Ken Evans		Initial Dev
    // 03/16/97  G. Lofton - MTW	Rework, add case unit
    // 				function assignment.
    // 04/29/97  JF. Caillouet		Change Current Date
    // 06-06-97  J. Rookard - MTW      Implement code for determining initial 
    // Case Unit State for newly created Case Units.  Modify assignment logic
    // for reactivated and new Case Units.
    // 05/25/99 M. Lachowicz      Replace zdel exit state by
    //                            by new exit state.
    // ------------------------------------------------------------
    // 06/24/99 M.Lachowicz       Change property of READ
    //                            (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 01/20/2000 W.Campbell      Disabled code for
    //                            READ of case_assignment and
    //                            READ of related office_service_provider.
    //                            Work done on PR# 85055.
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell      Disabled code for
    //                            CREATE of case_unit_function_assignmt
    //                            for CH logic.  Work done on PR# 85055.
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell      Inserted new code for
    //                            CREATE of case_unit_function_assignmt
    //                            for CH logic.  Work done on PR# 85055.
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell      Disabled code for
    //                            CREATE of case_unit_function_assignmt
    //                            for AP logic.  Work done on PR# 85055.
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell      Inserted new code for
    //                            CREATE of case_unit_function_assignmt
    //                            for AP logic.  Work done on PR# 85055.
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell      Disabled code (an IF stmt)
    //                            which does nothing.
    //                            Work done on PR# 85055.
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell      Added start_date to view
    //                            local_hold case_unit and logic to
    //                            set its value to fix problem where
    //                            the start_date was not being passed
    //                            to cab SP_CREATE_CASE_UNIT.
    //                            Work done on PR# 85584.
    // -------------------------------------------------------
    // -------------------------------------------------------
    // 01/21/2000 M.Lachowicz -   Remove condition
    //                            checking if case_assignment
    //                            effective_date is less or equal to current
    //                            date.
    //                            Work done on PR# 85055.
    // -------------------------------------------------------
    // 02/04/2000 M.Lachowicz     Changed code to create case
    //                            untis when AP start date is before
    //                            CH start date.
    //                            Work done on PR# 86511.
    // -------------------------------------------------------
    // 06/02/2000 M.Lachowicz     Changed code to create case
    //                            untis when CH start date is before
    //                            AP start date.
    //                            Work done on PR# 96271.
    // -------------------------------------------------------
    local.Current.Date = Now().Date;

    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      // Currency on Case established.
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // -------------------------------------------------------
    // 01/20/2000 W.Campbell - Disabled following
    // code for READ of case_assignment and
    // READ of related office_service_provider.
    // Work done on PR# 85055.
    // -------------------------------------------------------
    // -------------------------------------------------------
    // 01/20/2000 W.Campbell - End of Disabled
    // code for READ of case_assignment and
    // READ of related office_service_provider.
    // Work done on PR# 85055.
    // -------------------------------------------------------
    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCaseRoleCsePerson1())
    {
      // Currency on AR for the current Case is established.
      local.Ar.Number = entities.ArCsePerson.Number;
    }
    else
    {
      ExitState = "AR_NF_RB";

      return;
    }

    local.Group.Index = 0;
    local.Group.CheckSize();

    local.Group.Update.Det.FuncText3 = "LOC";

    local.Group.Index = 1;
    local.Group.CheckSize();

    local.Group.Update.Det.FuncText3 = "PAT";

    local.Group.Index = 2;
    local.Group.CheckSize();

    local.Group.Update.Det.FuncText3 = "OBG";

    local.Group.Index = 3;
    local.Group.CheckSize();

    local.Group.Update.Det.FuncText3 = "ENF";
    local.End.Date = AddDays(import.CaseRole.StartDate, -1);
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(import.CaseRole.Type1, "CH"))
    {
      // 06/24/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePerson1())
      {
        local.Ch.Number = entities.Ch.Number;
      }
      else
      {
        // 05/25/99 M. Lachowicz      Replace zdel exit state by
        //                            by new exit state.
        ExitState = "CSE_PERSON_NF";

        return;
      }

      // 06/02/2000 M.L Modified READ EACH to pick up all 'AP'
      // who are active on Start Date of new CH or will be active after start 
      // date of new CH. Check ONLY if AP end date is greater
      // than import case role start date.
      foreach(var item in ReadCaseRoleCsePerson2())
      {
        local.Ap.Number = entities.Ap.Number;

        // 06/24/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (ReadCaseUnit1())
        {
          // ------------------------------------------------------------
          // Reactivate the previous Case Unit that already exists. Ensure that 
          // all previously existing Case Unit Function assignments for the Case
          // Unit are discontinued.  Create new Case Unit Function assignments
          // and assign to the OSP currently assigned to the Case.
          // ------------------------------------------------------------
          try
          {
            UpdateCaseUnit();
            UseSpDtrReactivatedCuState();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
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

          // Discontinue all previous Case Unit Function assignments for this 
          // Case Unit that happen to be open.
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
                  ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - Disabled following
          // code for CREATE of case_unit_function_assignmt
          // for CH logic.  Work done on PR# 85055.
          // -------------------------------------------------------
          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - End of Disabled
          // code for CREATE of case_unit_function_assignmt
          // for CH logic.  Work done on PR# 85055.
          // -------------------------------------------------------
        }
        else
        {
          // 06/02/2000 M.L Start
          if (Lt(entities.Re.EndDate, local.Current.Date))
          {
            continue;
          }

          // 06/02/2000 M.L End
          // ------------------------------------------------------------
          // Create new Case Unit and assign to the current Case
          // Assignment OSP.
          // ------------------------------------------------------------
          if (IsEmpty(local.Ap.Number) || IsEmpty(local.Ar.Number) || IsEmpty
            (local.Ch.Number))
          {
            // 05/25/99 M. Lachowicz      Replace zdel exit state by
            //                            by new exit state.
            ExitState = "CSE_PERSON_NF";

            return;
          }

          UseSpDtrCaseSrvcType();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // 06/02/2000 M.L Start
          if (Lt(import.CaseRole.StartDate, entities.Re.StartDate))
          {
            local.Hold.StartDate = entities.Re.StartDate;
          }
          else
          {
            local.Hold.StartDate = import.CaseRole.StartDate;
          }

          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - Added start_date
          // to view local_hold case_unit and logic to
          // set its value to fix problem where
          // the start_date was not being passed
          // to cab SP_CREATE_CASE_UNIT.
          // Work done on PR# 85584.
          // -------------------------------------------------------
          // 06/02/2000 M.L End
          UseSpCreateCaseUnit();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseSpDtrInitialCaseUnitState();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // 06/24/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (!ReadCaseUnit2())
          {
            ExitState = "CASE_UNIT_NF_RB";

            return;
          }

          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - Disabled following
          // code for CREATE of case_unit_function_assignmt
          // for CH logic.  Work done on PR# 85055.
          // -------------------------------------------------------
          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - End of Disabled
          // code for CREATE of case_unit_function_assignmt
          // for CH logic.  Work done on PR# 85055.
          // -------------------------------------------------------
        }

        // -------------------------------------------------------
        // 01/20/2000 W.Campbell - Inserted new code
        // for CREATE of case_unit_function_assignmt
        // for CH logic.  Work done on PR# 85055.
        // -------------------------------------------------------
        // -------------------------------------------------------
        // 01/21/2000 M.Lachowicz - Remove condition
        // checking if case_assignment effective_date
        // is less or equal to current date.
        // Work done on PR# 85055.
        // -------------------------------------------------------
        foreach(var item1 in ReadCaseAssignment())
        {
          // 06/24/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadOfficeServiceProvider())
          {
            // Currency on Case assigned Office Service Provider established.
          }
          else
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            return;
          }

          // Create new Case Unit Function assignments for all functions for the
          // reactivated Case Unit
          // and assign to the Case Coordinator.
          for(local.Group.Index = 0; local.Group.Index < Local
            .GroupGroup.Capacity; ++local.Group.Index)
          {
            if (!local.Group.CheckSize())
            {
              break;
            }

            // -------------------------------------------------------
            // 01/21/2000 M.Lachowicz - Set proper
            // Effective Date and Discontinue Date.
            // Work done on PR# 85055.
            // -------------------------------------------------------
            if (Lt(import.CaseRole.StartDate,
              entities.CaseAssignment.EffectiveDate))
            {
              local.CaseUnitFunctionAssignmt.EffectiveDate =
                entities.CaseAssignment.EffectiveDate;
            }
            else
            {
              local.CaseUnitFunctionAssignmt.EffectiveDate =
                import.CaseRole.StartDate;
            }

            if (Lt(entities.CaseAssignment.DiscontinueDate, local.MaxDate.Date))
            {
              local.CaseUnitFunctionAssignmt.DiscontinueDate =
                entities.CaseAssignment.DiscontinueDate;
            }
            else
            {
              local.CaseUnitFunctionAssignmt.DiscontinueDate =
                local.MaxDate.Date;
            }

            // -------------------------------------------------------
            // 01/21/2000 M.Lachowicz - End.
            // -------------------------------------------------------
            try
            {
              CreateCaseUnitFunctionAssignmt();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

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

          local.Group.CheckIndex();
        }

        // -------------------------------------------------------
        // 01/20/2000 W.Campbell - End of new code
        // for CREATE of case_unit_function_assignmt
        // for CH logic.  Work done on PR# 85055.
        // -------------------------------------------------------
      }
    }
    else if (Equal(import.CaseRole.Type1, "AP"))
    {
      // 06/24/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePerson2())
      {
        local.Ap.Number = entities.Ap.Number;
      }
      else
      {
        // 05/25/99 M. Lachowicz      Replace zdel exit state by
        //                            by new exit state.
        ExitState = "CSE_PERSON_NF";

        return;
      }

      // 02/04/2000 M.L Modified READ EACH to pick up all 'CH'
      // who are active on Start Date of new AP or will be active after start 
      // date of new AP. Check ONLY if child end date is greater
      // than import case role start date.
      foreach(var item in ReadCaseRoleCsePerson3())
      {
        local.Ch.Number = entities.Ch.Number;

        // 06/24/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (ReadCaseUnit1())
        {
          // ------------------------------------------------------------
          // Reactivate the previous Case Unit that already exists.
          // ------------------------------------------------------------
          try
          {
            UpdateCaseUnit();
            UseSpDtrReactivatedCuState();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
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

          // Discontinue any previous Case Unit function assignments for this 
          // Case Unit that happen to be open.
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

          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - Disabled following
          // code for CREATE of case_unit_function_assignmt
          // for AP logic.  Work done on PR# 85055.
          // -------------------------------------------------------
          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - End of Disabled
          // code for CREATE of case_unit_function_assignmt
          // for AP logic.  Work done on PR# 85055.
          // -------------------------------------------------------
        }
        else
        {
          // 02/04/2000 M.L Start
          if (Lt(entities.Re.EndDate, local.Current.Date))
          {
            continue;
          }

          // 02/04/2000 M.L End
          // ------------------------------------------------------------
          // Create new Case Unit and new Case Unit Function assignments, assign
          // to the current Case
          // Assignment OSP.
          // ------------------------------------------------------------
          UseSpDtrCaseSrvcType();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // 02/04/2000 M.L Start
          if (Lt(import.CaseRole.StartDate, entities.Re.StartDate))
          {
            local.Hold.StartDate = entities.Re.StartDate;
          }
          else
          {
            local.Hold.StartDate = import.CaseRole.StartDate;
          }

          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - Added start_date
          // to view local_hold case_unit and logic to
          // set its value to fix problem where
          // the start_date was not being passed
          // to cab SP_CREATE_CASE_UNIT.
          // Work done on PR# 85584.
          // -------------------------------------------------------
          // 02/04/2000 M.L End
          UseSpCreateCaseUnit();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseSpDtrInitialCaseUnitState();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // 06/24/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (!ReadCaseUnit2())
          {
            ExitState = "CASE_UNIT_NF_RB";

            return;
          }

          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - Disabled following
          // code for CREATE of case_unit_function_assignmt
          // for AP logic.  Work done on PR# 85055.
          // -------------------------------------------------------
          // -------------------------------------------------------
          // 01/20/2000 W.Campbell - End of Disabled
          // code for CREATE of case_unit_function_assignmt
          // for AP logic.  Work done on PR# 85055.
          // -------------------------------------------------------
        }

        // -------------------------------------------------------
        // 01/20/2000 W.Campbell - Inserted new code
        // for CREATE of case_unit_function_assignmt
        // for AP logic.  Work done on PR# 85055.
        // -------------------------------------------------------
        // -------------------------------------------------------
        // 01/21/2000 M.Lachowicz - Remove condition
        // checking if case_assignment effective_date
        // is less or equal to current date.
        // Work done on PR# 85055.
        // -------------------------------------------------------
        foreach(var item1 in ReadCaseAssignment())
        {
          // 06/24/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadOfficeServiceProvider())
          {
            // Currency on Case assigned Office Service Provider established.
          }
          else
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            return;
          }

          // Create new Case Unit Function assignments for all functions for the
          // current Case Unit
          // and assign to the Case Coordinator.
          for(local.Group.Index = 0; local.Group.Index < Local
            .GroupGroup.Capacity; ++local.Group.Index)
          {
            if (!local.Group.CheckSize())
            {
              break;
            }

            // -------------------------------------------------------
            // 01/21/2000 M.Lachowicz - Set proper
            // Effective Date and Discontinue Date.
            // Work done on PR# 85055.
            // -------------------------------------------------------
            // 02/04/2000 M.L Start
            if (Lt(local.Hold.StartDate, entities.CaseAssignment.EffectiveDate))
            {
              local.CaseUnitFunctionAssignmt.EffectiveDate =
                entities.CaseAssignment.EffectiveDate;
            }
            else
            {
              local.CaseUnitFunctionAssignmt.EffectiveDate =
                local.Hold.StartDate;
            }

            // 02/04/2000 M.L End
            if (Lt(entities.CaseAssignment.DiscontinueDate, local.MaxDate.Date))
            {
              local.CaseUnitFunctionAssignmt.DiscontinueDate =
                entities.CaseAssignment.DiscontinueDate;
            }
            else
            {
              local.CaseUnitFunctionAssignmt.DiscontinueDate =
                local.MaxDate.Date;
            }

            // -------------------------------------------------------
            // 01/21/2000 M.Lachowicz - End.
            // -------------------------------------------------------
            try
            {
              CreateCaseUnitFunctionAssignmt();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

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

          local.Group.CheckIndex();
        }

        // -------------------------------------------------------
        // 01/20/2000 W.Campbell - End of new code
        // for CREATE of case_unit_function_assignmt
        // for AP logic.  Work done on PR# 85055.
        // -------------------------------------------------------
      }
    }
    else
    {
      // -------------------------------------------------------
      // 01/20/2000 W.Campbell - Disabled following IF stmt
      // code as it does nothing.  Work done on PR# 85055.
      // -------------------------------------------------------
      // -------------------------------------------------------
      // 01/20/2000 W.Campbell - End of Disabled code
      // which does nothing.  Work done on PR# 85055.
      // -------------------------------------------------------
    }
  }

  private static void MoveCaseUnit1(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
    target.StartDate = source.StartDate;
  }

  private static void MoveCaseUnit2(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
    target.StartDate = source.StartDate;
    target.ClosureDate = source.ClosureDate;
    target.ClosureReasonCode = source.ClosureReasonCode;
  }

  private static void MoveCaseUnit3(CaseUnit source, CaseUnit target)
  {
    target.State = source.State;
    target.StartDate = source.StartDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCreateCaseUnit()
  {
    var useImport = new SpCreateCaseUnit.Import();
    var useExport = new SpCreateCaseUnit.Export();

    useImport.Case1.Number = entities.Case1.Number;
    MoveCaseUnit3(local.Hold, useImport.CaseUnit);
    useImport.Ap.Number = local.Ap.Number;
    useImport.Child.Number = local.Ch.Number;
    useImport.Ar.Number = local.Ar.Number;

    Call(SpCreateCaseUnit.Execute, useImport, useExport);

    local.Hold.Assign(useExport.CaseUnit);
  }

  private void UseSpDtrCaseSrvcType()
  {
    var useImport = new SpDtrCaseSrvcType.Import();
    var useExport = new SpDtrCaseSrvcType.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SpDtrCaseSrvcType.Execute, useImport, useExport);

    local.Hold.State = useExport.CaseUnit.State;
  }

  private void UseSpDtrInitialCaseUnitState()
  {
    var useImport = new SpDtrInitialCaseUnitState.Import();
    var useExport = new SpDtrInitialCaseUnitState.Export();

    useImport.Case1.Number = entities.Case1.Number;
    MoveCaseUnit1(local.Hold, useImport.CaseUnit);
    useImport.CsePerson.Number = local.Ap.Number;

    Call(SpDtrInitialCaseUnitState.Execute, useImport, useExport);
  }

  private void UseSpDtrReactivatedCuState()
  {
    var useImport = new SpDtrReactivatedCuState.Import();
    var useExport = new SpDtrReactivatedCuState.Export();

    useImport.Case1.Number = entities.Case1.Number;
    MoveCaseUnit2(entities.CaseUnit, useImport.CaseUnit);

    Call(SpDtrReactivatedCuState.Execute, useImport, useExport);
  }

  private void CreateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    System.Diagnostics.Debug.Assert(entities.Ca.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.CaseUnitFunctionAssignmt.EffectiveDate;
    var discontinueDate = local.CaseUnitFunctionAssignmt.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.Ca.SpdGeneratedId;
    var offId = entities.Ca.OffGeneratedId;
    var ospCode = entities.Ca.RoleCode;
    var ospDate = entities.Ca.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = local.Group.Item.Det.FuncText3;

    entities.New1.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.New1.ReasonCode = reasonCode;
    entities.New1.OverrideInd = overrideInd;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = null;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.CsuNo = csuNo;
    entities.New1.CasNo = casNo;
    entities.New1.Function = function;
    entities.New1.Populated = true;
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

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCsePerson.Number = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCaseRole.Populated = true;
        entities.ArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.Re.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", import.CaseRole.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Re.CasNumber = db.GetString(reader, 0);
        entities.Re.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.Re.Type1 = db.GetString(reader, 2);
        entities.Re.Identifier = db.GetInt32(reader, 3);
        entities.Re.StartDate = db.GetNullableDate(reader, 4);
        entities.Re.EndDate = db.GetNullableDate(reader, 5);
        entities.Re.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Re.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.Re.Populated = false;
    entities.Ch.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", import.CaseRole.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Re.CasNumber = db.GetString(reader, 0);
        entities.Re.CspNumber = db.GetString(reader, 1);
        entities.Ch.Number = db.GetString(reader, 1);
        entities.Re.Type1 = db.GetString(reader, 2);
        entities.Re.Identifier = db.GetInt32(reader, 3);
        entities.Re.StartDate = db.GetNullableDate(reader, 4);
        entities.Re.EndDate = db.GetNullableDate(reader, 5);
        entities.Re.Populated = true;
        entities.Ch.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Re.Type1);

        return true;
      });
  }

  private bool ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAr", entities.ArCsePerson.Number);
        db.SetNullableString(command, "cspNoAp", entities.Ap.Number);
        db.SetNullableString(command, "cspNoChild", entities.Ch.Number);
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

  private bool ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetInt32(command, "cuNumber", local.Hold.CuNumber);
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

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Prev.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Prev.ReasonCode = db.GetString(reader, 0);
        entities.Prev.OverrideInd = db.GetString(reader, 1);
        entities.Prev.EffectiveDate = db.GetDate(reader, 2);
        entities.Prev.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Prev.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Prev.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Prev.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 6);
        entities.Prev.SpdId = db.GetInt32(reader, 7);
        entities.Prev.OffId = db.GetInt32(reader, 8);
        entities.Prev.OspCode = db.GetString(reader, 9);
        entities.Prev.OspDate = db.GetDate(reader, 10);
        entities.Prev.CsuNo = db.GetInt32(reader, 11);
        entities.Prev.CasNo = db.GetString(reader, 12);
        entities.Prev.Function = db.GetString(reader, 13);
        entities.Prev.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ch.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Ca.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.Ca.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Ca.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Ca.RoleCode = db.GetString(reader, 2);
        entities.Ca.EffectiveDate = db.GetDate(reader, 3);
        entities.Ca.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Ca.Populated = true;
      });
  }

  private void UpdateCaseUnit()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var startDate = import.CaseRole.StartDate;
    var closureDate = local.MaxDate.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit",
      (db, command) =>
      {
        db.SetDate(command, "startDate", startDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.StartDate = startDate;
    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.ClosureReasonCode = "";
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.Prev.Populated);

    var discontinueDate = AddDays(local.Current.Date, -1);
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Prev.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Prev.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.Prev.SpdId);
        db.SetInt32(command, "offId", entities.Prev.OffId);
        db.SetString(command, "ospCode", entities.Prev.OspCode);
        db.
          SetDate(command, "ospDate", entities.Prev.OspDate.GetValueOrDefault());
          
        db.SetInt32(command, "csuNo", entities.Prev.CsuNo);
        db.SetString(command, "casNo", entities.Prev.CasNo);
      });

    entities.Prev.DiscontinueDate = discontinueDate;
    entities.Prev.LastUpdatedBy = lastUpdatedBy;
    entities.Prev.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Prev.Populated = true;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private CaseRole caseRole;
    private CsePerson csePerson;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      public CaseFuncWorkSet Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CaseFuncWorkSet det;
    }

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
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public CaseUnit Hold
    {
      get => hold ??= new();
      set => hold = value;
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
    /// A value of Use.
    /// </summary>
    [JsonPropertyName("use")]
    public DateWorkArea Use
    {
      get => use ??= new();
      set => use = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of LastCase.
    /// </summary>
    [JsonPropertyName("lastCase")]
    public CaseUnit LastCase
    {
      get => lastCase ??= new();
      set => lastCase = value;
    }

    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseUnit hold;
    private DateWorkArea current;
    private DateWorkArea use;
    private DateWorkArea end;
    private Array<GroupGroup> group;
    private CsePerson ap;
    private CsePerson ch;
    private CsePerson ar;
    private DateWorkArea maxDate;
    private CaseUnit lastCase;
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
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of Re.
    /// </summary>
    [JsonPropertyName("re")]
    public CaseRole Re
    {
      get => re ??= new();
      set => re = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of Ca.
    /// </summary>
    [JsonPropertyName("ca")]
    public OfficeServiceProvider Ca
    {
      get => ca ??= new();
      set => ca = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CaseUnitFunctionAssignmt Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseUnitFunctionAssignmt New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Case1 case1;
    private CaseRole arCaseRole;
    private CaseRole re;
    private CsePerson arCsePerson;
    private CsePerson ap;
    private CsePerson ch;
    private CaseUnit caseUnit;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider ca;
    private CaseUnitFunctionAssignmt prev;
    private CaseUnitFunctionAssignmt new1;
  }
#endregion
}
