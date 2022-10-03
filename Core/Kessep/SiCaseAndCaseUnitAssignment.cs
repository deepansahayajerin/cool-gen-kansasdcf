// Program: SI_CASE_AND_CASE_UNIT_ASSIGNMENT, ID: 371727797, model: 746.
// Short name: SWE01948
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
/// A program: SI_CASE_AND_CASE_UNIT_ASSIGNMENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCaseAndCaseUnitAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CASE_AND_CASE_UNIT_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCaseAndCaseUnitAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCaseAndCaseUnitAssignment.
  /// </summary>
  public SiCaseAndCaseUnitAssignment(IContext context, Import import,
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
    // Date	  Developer	        Description
    // 03/09/97  G. Lofton - MTW	Initial development
    // 04/29/97  JF. Caillouet		Change Current Date
    // 12/19/97  J Rookard - MTW       Add routine for using Child name in alpha
    // matching to support Foster Care Cases where no Ap or multiple APs exist
    // and AR is an organization.
    // ------------------------------------------------------------
    // 12/09/98  W. Campbell         Removed any code
    //                               
    // which referenced
    //                               
    // CSE_PERSON_ADDRESS
    //                               
    // attribute START_DATE.
    //                               
    // This work was done on IDCR
    // 454.
    // ---------------------------------------------
    // 06/22/99  M. Lachowicz        Change property of READ
    //                               
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 09/24/99  M. Lachowicz        PR #74593 Changed code to
    //                               
    // reopen CASE where all child
    // roles
    //                               
    // were closed before CASE was
    // closed.
    // ------------------------------------------------------------
    // 11/03/99 W. Campbell          Added qualification
    //                               
    // to several READ EACH and
    //                               
    // READ stmts and additional
    //                               
    // logic checking for an
    //                               
    // Organization as the AR.
    //                               
    // See embeded notes for
    //                               
    // additional details on each.
    //                               
    // Work done on PR# 00077898.
    // -------------------------------------------------------
    // 11/04/99 W.CAMPBELL      Some new code inserted and
    //                          some old code disabled as part of logic
    //                          restructure to fix the case assignment
    //                          problem reported on PR# 00077898.
    //                          Also, a new cab
    //                          SP_GET_PERSON_RES_ADDR_FOR_ASSGN
    //                          was created to get the 'best'
    //                          Residential address for use in the
    //                          county code matching part of the
    //                          logic for case assignment.
    // -----------------------------------
    // 05/10/00 W. Campbell     Added IF and Read and
    //                          encolsed NEXT stmts to fix
    //                          problem with Service Provider
    //                          not found.
    //                          Work done on PR# 00093095
    // ---------------------------------------------
    // 02/24/14 GVandy  CQ42103 Modify for consistency with SRRUN093
    //                          (SP_B300_CASELOAD_REDISTRIBUTION) batch.
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadOffice())
    {
      ExitState = "OFFICE_NF";

      return;
    }

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // -- Determine Case Program Type
    UseSiReadCaseProgramType();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.CaseProgram.Code = "";
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // -- Determine Case Function
    UseSiCabReturnCaseFunction();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.CaseFuncWorkSet.FuncText3 = "";
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // -- Determine Case Alpha Name and Tribunal
    UseSpCabDetrmnCaseNameTribunal();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.CaseTribunal.Identifier = 0;
      local.CaseName.Text30 = "A          A";
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // -- Load the Office Caseload Assignments into a local group.
    local.Local1.Index = -1;

    foreach(var item in ReadOfficeCaseloadAssignmentOfficeServiceProvider())
    {
      if (local.Local1.Index + 1 < Local.LocalGroup.Capacity)
      {
        ++local.Local1.Index;
        local.Local1.CheckSize();
      }
      else
      {
        break;
      }

      MoveOfficeCaseloadAssignment(entities.OfficeCaseloadAssignment,
        local.Local1.Update.OfficeCaseloadAssignment);
      local.Local1.Update.ServiceProvider.Assign(entities.ServiceProvider);
      local.Local1.Update.OfficeServiceProvider.RoleCode =
        entities.OfficeServiceProvider.RoleCode;

      if (ReadProgram())
      {
        local.Local1.Update.Program.Code = entities.Program.Code;
      }

      if (ReadTribunal())
      {
        local.Local1.Update.Tribunal.Identifier = entities.Tribunal.Identifier;
      }
    }

    // -- 02/21/2014 GVandy CQ42103 Change office caseload assignment order to 
    // priority,
    //    program, tribunal, function, and alpha.
    // -- Sort the local group by Priority, Program, Tribunal, Function, and 
    // Alpha.
    //    A value in one of these sort criteria takes precedent over a blank.
    local.I.Count = 1;

    for(var limit = local.Local1.Count; local.I.Count <= limit; ++local.I.Count)
    {
      local.Local1.Index = local.I.Count - 1;
      local.Local1.CheckSize();

      MoveOfficeCaseloadAssignment(local.Local1.Item.OfficeCaseloadAssignment,
        local.Compare.CompareOfficeCaseloadAssignment);
      local.Compare.CompareOfficeServiceProvider.RoleCode =
        local.Local1.Item.OfficeServiceProvider.RoleCode;
      local.Compare.CompareProgram.Code = local.Local1.Item.Program.Code;
      local.Compare.CompareServiceProvider.Assign(
        local.Local1.Item.ServiceProvider);
      local.Compare.CompareTribunal.Identifier =
        local.Local1.Item.Tribunal.Identifier;
      local.J.Count = local.I.Count + 1;

      for(var limit1 = local.Local1.Count; local.J.Count <= limit1; ++
        local.J.Count)
      {
        local.Local1.Index = local.J.Count - 1;
        local.Local1.CheckSize();

        local.Swap1.Flag = "N";

        if (AsChar(local.Swap1.Flag) == 'N')
        {
          // -- Priority is the first sort criteria.
          if (local.Local1.Item.OfficeCaseloadAssignment.Priority < local
            .Compare.CompareOfficeCaseloadAssignment.Priority)
          {
            local.Swap1.Flag = "Y";

            goto Test1;
          }
          else if (local.Local1.Item.OfficeCaseloadAssignment.Priority > local
            .Compare.CompareOfficeCaseloadAssignment.Priority)
          {
            continue;
          }

          // -- Program is the second sort criteria.
          if (IsEmpty(local.Local1.Item.Program.Code))
          {
            if (!IsEmpty(local.Compare.CompareProgram.Code))
            {
              continue;
            }
          }
          else if (Lt(local.Local1.Item.Program.Code,
            local.Compare.CompareProgram.Code) || IsEmpty
            (local.Compare.CompareProgram.Code))
          {
            local.Swap1.Flag = "Y";

            goto Test1;
          }
          else if (Lt(local.Compare.CompareProgram.Code,
            local.Local1.Item.Program.Code))
          {
            continue;
          }

          // -- Tribunal is the third sort criteria.
          if (local.Local1.Item.Tribunal.Identifier == 0)
          {
            if (local.Compare.CompareTribunal.Identifier != 0)
            {
              continue;
            }
          }
          else if (local.Local1.Item.Tribunal.Identifier < local
            .Compare.CompareTribunal.Identifier || local
            .Compare.CompareTribunal.Identifier == 0)
          {
            local.Swap1.Flag = "Y";

            goto Test1;
          }
          else if (local.Local1.Item.Tribunal.Identifier > local
            .Compare.CompareTribunal.Identifier)
          {
            continue;
          }

          // -- Function is the fourth sort criteria.
          if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.Function))
          {
            if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.Function))
              
            {
              continue;
            }
          }
          else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.Function,
            local.Compare.CompareOfficeCaseloadAssignment.Function) || IsEmpty
            (local.Compare.CompareOfficeCaseloadAssignment.Function))
          {
            local.Swap1.Flag = "Y";

            goto Test1;
          }
          else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.Function,
            local.Local1.Item.OfficeCaseloadAssignment.Function))
          {
            continue;
          }

          // -- Beginning Alpha is the fifth sort criteria.
          if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha))
          {
            if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
              BeginingAlpha))
            {
              continue;
            }
          }
          else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha,
            local.Compare.CompareOfficeCaseloadAssignment.BeginingAlpha) || IsEmpty
            (local.Compare.CompareOfficeCaseloadAssignment.BeginingAlpha))
          {
            local.Swap1.Flag = "Y";

            goto Test1;
          }
          else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.
            BeginingAlpha,
            local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha))
          {
            continue;
          }

          // -- Ending Alpha is the sixth sort criteria.
          if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha))
          {
            if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
              EndingAlpha))
            {
              continue;
            }
          }
          else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha,
            local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha) || IsEmpty
            (local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha))
          {
            local.Swap1.Flag = "Y";
          }
          else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha,
            local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha))
          {
            continue;
          }
        }

Test1:

        if (AsChar(local.Swap1.Flag) == 'N')
        {
          continue;
        }

        MoveOfficeCaseloadAssignment(local.Local1.Item.OfficeCaseloadAssignment,
          local.Swap.SwapOfficeCaseloadAssignment);
        local.Swap.SwapOfficeServiceProvider.RoleCode =
          local.Local1.Item.OfficeServiceProvider.RoleCode;
        local.Swap.SwapProgram.Code = local.Local1.Item.Program.Code;
        local.Swap.SwapServiceProvider.
          Assign(local.Local1.Item.ServiceProvider);
        local.Swap.SwapTribunal.Identifier =
          local.Local1.Item.Tribunal.Identifier;
        local.Local1.Update.OfficeCaseloadAssignment.Assign(
          local.Compare.CompareOfficeCaseloadAssignment);
        local.Local1.Update.OfficeServiceProvider.RoleCode =
          local.Compare.CompareOfficeServiceProvider.RoleCode;
        local.Local1.Update.Program.Code = local.Compare.CompareProgram.Code;
        local.Local1.Update.ServiceProvider.Assign(
          local.Compare.CompareServiceProvider);
        local.Local1.Update.Tribunal.Identifier =
          local.Compare.CompareTribunal.Identifier;

        local.Local1.Index = local.I.Count - 1;
        local.Local1.CheckSize();

        local.Local1.Update.OfficeCaseloadAssignment.Assign(
          local.Swap.SwapOfficeCaseloadAssignment);
        local.Local1.Update.OfficeServiceProvider.RoleCode =
          local.Swap.SwapOfficeServiceProvider.RoleCode;
        local.Local1.Update.Program.Code = local.Swap.SwapProgram.Code;
        local.Local1.Update.ServiceProvider.Assign(
          local.Swap.SwapServiceProvider);
        local.Local1.Update.Tribunal.Identifier =
          local.Swap.SwapTribunal.Identifier;
        MoveOfficeCaseloadAssignment(local.Local1.Item.OfficeCaseloadAssignment,
          local.Compare.CompareOfficeCaseloadAssignment);
        local.Compare.CompareOfficeServiceProvider.RoleCode =
          local.Local1.Item.OfficeServiceProvider.RoleCode;
        local.Compare.CompareProgram.Code = local.Local1.Item.Program.Code;
        local.Compare.CompareServiceProvider.Assign(
          local.Local1.Item.ServiceProvider);
        local.Compare.CompareTribunal.Identifier =
          local.Local1.Item.Tribunal.Identifier;
      }
    }

    // -- Set beginning and ending alpha group values for consistency with prior
    // coding.
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      local.Local1.Update.BegAlph.Text30 =
        local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha;
      local.Local1.Update.EndAlpha.Text30 =
        local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha;
    }

    local.Local1.CheckIndex();

    // -- Find the service provider to assign to the case.
    local.MatchFound.Flag = "N";

    if (AsChar(local.MatchFound.Flag) == 'N')
    {
      // **** Program Codes are Priority 1 assignments ****
      if (!IsEmpty(local.CaseProgram.Code))
      {
        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 1)
          {
            continue;
          }

          if (Equal(local.CaseProgram.Code, local.Local1.Item.Program.Code))
          {
            if (local.Local1.Item.Tribunal.Identifier == local
              .CaseTribunal.Identifier && Equal
              (local.Local1.Item.OfficeCaseloadAssignment.Function,
              local.CaseFuncWorkSet.FuncText3) && !
              Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
              Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
            {
              local.MatchFound.Flag = "Y";

              goto Test2;
            }
            else if (local.Local1.Item.Tribunal.Identifier == 0 && Equal
              (local.Local1.Item.OfficeCaseloadAssignment.Function,
              local.CaseFuncWorkSet.FuncText3) && !
              Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
              Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
            {
              local.MatchFound.Flag = "Y";

              goto Test2;
            }
            else if (local.Local1.Item.Tribunal.Identifier == local
              .CaseTribunal.Identifier && IsEmpty
              (local.Local1.Item.OfficeCaseloadAssignment.Function) && !
              Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
              Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
            {
              local.MatchFound.Flag = "Y";

              goto Test2;
            }
            else if (local.Local1.Item.Tribunal.Identifier == 0 && IsEmpty
              (local.Local1.Item.OfficeCaseloadAssignment.Function) && !
              Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
              Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
            {
              local.MatchFound.Flag = "Y";

              goto Test2;
            }
          }
        }

        local.Local1.CheckIndex();
      }

      // **** Function Codes are Priority 2 assignments ****
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 2)
        {
          continue;
        }

        if (!IsEmpty(local.CaseFuncWorkSet.FuncText3) && Equal
          (local.CaseFuncWorkSet.FuncText3,
          local.Local1.Item.OfficeCaseloadAssignment.Function))
        {
          if (local.Local1.Item.Tribunal.Identifier == local
            .CaseTribunal.Identifier && !
            Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
            Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
          {
            local.MatchFound.Flag = "Y";

            goto Test2;
          }
          else if (local.Local1.Item.Tribunal.Identifier == 0 && !
            Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
            Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
          {
            local.MatchFound.Flag = "Y";

            goto Test2;
          }
        }
      }

      local.Local1.CheckIndex();

      // **** Tribunals are Priority 3 assignments ****
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 3)
        {
          continue;
        }

        if (local.CaseTribunal.Identifier == local
          .Local1.Item.Tribunal.Identifier)
        {
          if (!Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
            Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
          {
            local.MatchFound.Flag = "Y";

            goto Test2;
          }
        }
      }

      local.Local1.CheckIndex();

      // **** Alpha Codes are Priority 4 assignments ****
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 4)
        {
          continue;
        }

        if (!Lt(local.CaseName.Text30, local.Local1.Item.BegAlph.Text30) && !
          Lt(local.Local1.Item.EndAlpha.Text30, local.CaseName.Text30))
        {
          local.MatchFound.Flag = "Y";

          goto Test2;
        }
      }

      local.Local1.CheckIndex();

      // **** Alpha Codes are Priority 5 assignments ****
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (local.Local1.Item.OfficeCaseloadAssignment.Priority != 5)
        {
          continue;
        }

        local.MatchFound.Flag = "Y";

        goto Test2;
      }

      local.Local1.CheckIndex();
    }

Test2:

    if (AsChar(local.MatchFound.Flag) == 'N')
    {
      ExitState = "SP0000_OFFICE_CASELOAD_ASSGN_NF";

      return;
    }

    if (!ReadOfficeServiceProviderServiceProvider())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    UseCabSetMaximumDiscontinueDate();

    // ************************************************************
    // Create Case Assignment
    // ************************************************************
    // ---------------------------------------------
    // 05/10/00 W. Campbell - Disabled following Read
    // stmt to fix problem with Service Provider not found.
    // Copied the When Successful logic to below the
    // disabled READ. Work done on PR# 00093095
    // ---------------------------------------------
    // ---------------------------------------------
    // 05/10/00 W. Campbell - Copied Create stmt
    // to fix problem with Service Provider not found.
    // Copied the When Successful logic from the above
    // disabled READ. Work done on PR# 00093095
    // ---------------------------------------------
    try
    {
      CreateCaseAssignment();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_CASE_ASSIGNMENT_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SP0000_CASE_ASSIGNMENT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ************************************************************
    // Read all Case Unit Function Assignments and reassign if
    // necessary
    // ************************************************************
    foreach(var item in ReadCaseUnit())
    {
      try
      {
        CreateCaseUnitFunctionAssignmt2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

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

      try
      {
        CreateCaseUnitFunctionAssignmt4();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

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

      try
      {
        CreateCaseUnitFunctionAssignmt3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

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

      try
      {
        CreateCaseUnitFunctionAssignmt1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

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
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText1 = source.FuncText1;
    target.FuncText3 = source.FuncText3;
  }

  private static void MoveOfficeCaseloadAssignment(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.AssignmentIndicator = source.AssignmentIndicator;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EndingAlpha = source.EndingAlpha;
    target.BeginingAlpha = source.BeginingAlpha;
    target.Priority = source.Priority;
    target.Function = source.Function;
    target.AssignmentType = source.AssignmentType;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text8 = source.Text8;
    target.Text30 = source.Text30;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, local.CaseFuncWorkSet);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.CaseProgram);
  }

  private void UseSpCabDetrmnCaseNameTribunal()
  {
    var useImport = new SpCabDetrmnCaseNameTribunal.Import();
    var useExport = new SpCabDetrmnCaseNameTribunal.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Case1.Number = entities.Case1.Number;

    Call(SpCabDetrmnCaseNameTribunal.Execute, useImport, useExport);

    MoveTextWorkArea(useExport.TextWorkArea, local.CaseName);
    local.CaseTribunal.Identifier = useExport.Tribunal.Identifier;
  }

  private void CreateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var casNo = entities.Case1.Number;

    entities.CaseAssignment.Populated = false;
    Update("CreateCaseAssignment",
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
        db.SetString(command, "casNo", casNo);
      });

    entities.CaseAssignment.ReasonCode = reasonCode;
    entities.CaseAssignment.OverrideInd = overrideInd;
    entities.CaseAssignment.EffectiveDate = effectiveDate;
    entities.CaseAssignment.DiscontinueDate = discontinueDate;
    entities.CaseAssignment.CreatedBy = createdBy;
    entities.CaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.CaseAssignment.LastUpdatedBy = "";
    entities.CaseAssignment.LastUpdatedTimestamp = null;
    entities.CaseAssignment.SpdId = spdId;
    entities.CaseAssignment.OffId = offId;
    entities.CaseAssignment.OspCode = ospCode;
    entities.CaseAssignment.OspDate = ospDate;
    entities.CaseAssignment.CasNo = casNo;
    entities.CaseAssignment.Populated = true;
  }

  private void CreateCaseUnitFunctionAssignmt1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = "ENF";

    entities.New1.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt1",
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

  private void CreateCaseUnitFunctionAssignmt2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = "LOC";

    entities.New1.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt2",
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

  private void CreateCaseUnitFunctionAssignmt3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = "OBG";

    entities.New1.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt3",
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

  private void CreateCaseUnitFunctionAssignmt4()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = "PAT";

    entities.New1.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt4",
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

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.EffectiveDate = db.GetDate(reader, 3);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignmentOfficeServiceProvider()
  {
    entities.OfficeCaseloadAssignment.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 11);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 12);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 13);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 16);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 17);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 19);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 20);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 20);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 21);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 22);
        entities.ServiceProvider.UserId = db.GetString(reader, 23);
        entities.ServiceProvider.LastName = db.GetString(reader, 24);
        entities.ServiceProvider.FirstName = db.GetString(reader, 25);
        entities.OfficeCaseloadAssignment.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          local.Local1.Item.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          entities.OfficeCaseloadAssignment.PrgGeneratedId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.OfficeCaseloadAssignment.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    private Office office;
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
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
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
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>
      /// A value of EndAlpha.
      /// </summary>
      [JsonPropertyName("endAlpha")]
      public TextWorkArea EndAlpha
      {
        get => endAlpha ??= new();
        set => endAlpha = value;
      }

      /// <summary>
      /// A value of BegAlph.
      /// </summary>
      [JsonPropertyName("begAlph")]
      public TextWorkArea BegAlph
      {
        get => begAlph ??= new();
        set => begAlph = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private OfficeServiceProvider officeServiceProvider;
      private Program program;
      private Tribunal tribunal;
      private ServiceProvider serviceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
      private TextWorkArea endAlpha;
      private TextWorkArea begAlph;
    }

    /// <summary>A CompareGroup group.</summary>
    [Serializable]
    public class CompareGroup
    {
      /// <summary>
      /// A value of CompareOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("compareOfficeServiceProvider")]
      public OfficeServiceProvider CompareOfficeServiceProvider
      {
        get => compareOfficeServiceProvider ??= new();
        set => compareOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of CompareProgram.
      /// </summary>
      [JsonPropertyName("compareProgram")]
      public Program CompareProgram
      {
        get => compareProgram ??= new();
        set => compareProgram = value;
      }

      /// <summary>
      /// A value of CompareTribunal.
      /// </summary>
      [JsonPropertyName("compareTribunal")]
      public Tribunal CompareTribunal
      {
        get => compareTribunal ??= new();
        set => compareTribunal = value;
      }

      /// <summary>
      /// A value of CompareServiceProvider.
      /// </summary>
      [JsonPropertyName("compareServiceProvider")]
      public ServiceProvider CompareServiceProvider
      {
        get => compareServiceProvider ??= new();
        set => compareServiceProvider = value;
      }

      /// <summary>
      /// A value of CompareOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("compareOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment CompareOfficeCaseloadAssignment
      {
        get => compareOfficeCaseloadAssignment ??= new();
        set => compareOfficeCaseloadAssignment = value;
      }

      private OfficeServiceProvider compareOfficeServiceProvider;
      private Program compareProgram;
      private Tribunal compareTribunal;
      private ServiceProvider compareServiceProvider;
      private OfficeCaseloadAssignment compareOfficeCaseloadAssignment;
    }

    /// <summary>A SwapGroup group.</summary>
    [Serializable]
    public class SwapGroup
    {
      /// <summary>
      /// A value of SwapOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("swapOfficeServiceProvider")]
      public OfficeServiceProvider SwapOfficeServiceProvider
      {
        get => swapOfficeServiceProvider ??= new();
        set => swapOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of SwapProgram.
      /// </summary>
      [JsonPropertyName("swapProgram")]
      public Program SwapProgram
      {
        get => swapProgram ??= new();
        set => swapProgram = value;
      }

      /// <summary>
      /// A value of SwapTribunal.
      /// </summary>
      [JsonPropertyName("swapTribunal")]
      public Tribunal SwapTribunal
      {
        get => swapTribunal ??= new();
        set => swapTribunal = value;
      }

      /// <summary>
      /// A value of SwapServiceProvider.
      /// </summary>
      [JsonPropertyName("swapServiceProvider")]
      public ServiceProvider SwapServiceProvider
      {
        get => swapServiceProvider ??= new();
        set => swapServiceProvider = value;
      }

      /// <summary>
      /// A value of SwapOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("swapOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment SwapOfficeCaseloadAssignment
      {
        get => swapOfficeCaseloadAssignment ??= new();
        set => swapOfficeCaseloadAssignment = value;
      }

      private OfficeServiceProvider swapOfficeServiceProvider;
      private Program swapProgram;
      private Tribunal swapTribunal;
      private ServiceProvider swapServiceProvider;
      private OfficeCaseloadAssignment swapOfficeCaseloadAssignment;
    }

    /// <summary>A TbdGroup group.</summary>
    [Serializable]
    public class TbdGroup
    {
      /// <summary>
      /// A value of TbdLocalGrpAp.
      /// </summary>
      [JsonPropertyName("tbdLocalGrpAp")]
      public CsePersonsWorkSet TbdLocalGrpAp
      {
        get => tbdLocalGrpAp ??= new();
        set => tbdLocalGrpAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePersonsWorkSet tbdLocalGrpAp;
    }

    /// <summary>
    /// A value of CaseName.
    /// </summary>
    [JsonPropertyName("caseName")]
    public TextWorkArea CaseName
    {
      get => caseName ??= new();
      set => caseName = value;
    }

    /// <summary>
    /// A value of CaseTribunal.
    /// </summary>
    [JsonPropertyName("caseTribunal")]
    public Tribunal CaseTribunal
    {
      get => caseTribunal ??= new();
      set => caseTribunal = value;
    }

    /// <summary>
    /// A value of CaseProgram.
    /// </summary>
    [JsonPropertyName("caseProgram")]
    public Program CaseProgram
    {
      get => caseProgram ??= new();
      set => caseProgram = value;
    }

    /// <summary>
    /// A value of NumberOfCh.
    /// </summary>
    [JsonPropertyName("numberOfCh")]
    public Common NumberOfCh
    {
      get => numberOfCh ??= new();
      set => numberOfCh = value;
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
    public OfficeCaseloadAssignment Use
    {
      get => use ??= new();
      set => use = value;
    }

    /// <summary>
    /// A value of Oca.
    /// </summary>
    [JsonPropertyName("oca")]
    public Program Oca
    {
      get => oca ??= new();
      set => oca = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of ApCnt.
    /// </summary>
    [JsonPropertyName("apCnt")]
    public Common ApCnt
    {
      get => apCnt ??= new();
      set => apCnt = value;
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
    /// A value of Addr.
    /// </summary>
    [JsonPropertyName("addr")]
    public CsePersonAddress Addr
    {
      get => addr ??= new();
      set => addr = value;
    }

    /// <summary>
    /// A value of MatchFound.
    /// </summary>
    [JsonPropertyName("matchFound")]
    public Common MatchFound
    {
      get => matchFound ??= new();
      set => matchFound = value;
    }

    /// <summary>
    /// A value of PgmFound.
    /// </summary>
    [JsonPropertyName("pgmFound")]
    public Common PgmFound
    {
      get => pgmFound ??= new();
      set => pgmFound = value;
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
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// Gets a value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public CompareGroup Compare
    {
      get => compare ?? (compare = new());
      set => compare = value;
    }

    /// <summary>
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of Swap1.
    /// </summary>
    [JsonPropertyName("swap1")]
    public Common Swap1
    {
      get => swap1 ??= new();
      set => swap1 = value;
    }

    /// <summary>
    /// Gets a value of Swap.
    /// </summary>
    [JsonPropertyName("swap")]
    public SwapGroup Swap
    {
      get => swap ?? (swap = new());
      set => swap = value;
    }

    /// <summary>
    /// A value of TbdLocalCase.
    /// </summary>
    [JsonPropertyName("tbdLocalCase")]
    public TextWorkArea TbdLocalCase
    {
      get => tbdLocalCase ??= new();
      set => tbdLocalCase = value;
    }

    /// <summary>
    /// Gets a value of Tbd.
    /// </summary>
    [JsonIgnore]
    public Array<TbdGroup> Tbd => tbd ??= new(TbdGroup.Capacity);

    /// <summary>
    /// Gets a value of Tbd for json serialization.
    /// </summary>
    [JsonPropertyName("tbd")]
    [Computed]
    public IList<TbdGroup> Tbd_Json
    {
      get => tbd;
      set => Tbd.Assign(value);
    }

    private TextWorkArea caseName;
    private Tribunal caseTribunal;
    private Program caseProgram;
    private Common numberOfCh;
    private DateWorkArea current;
    private OfficeCaseloadAssignment use;
    private Program oca;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common apCnt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonAddress addr;
    private Common matchFound;
    private Common pgmFound;
    private DateWorkArea max;
    private Array<LocalGroup> local1;
    private Common i;
    private CompareGroup compare;
    private Common j;
    private Common swap1;
    private SwapGroup swap;
    private TextWorkArea tbdLocalCase;
    private Array<TbdGroup> tbd;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
    }

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
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChPersonProgram.
    /// </summary>
    [JsonPropertyName("chPersonProgram")]
    public PersonProgram ChPersonProgram
    {
      get => chPersonProgram ??= new();
      set => chPersonProgram = value;
    }

    /// <summary>
    /// A value of ChProgram.
    /// </summary>
    [JsonPropertyName("chProgram")]
    public Program ChProgram
    {
      get => chProgram ??= new();
      set => chProgram = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of Oca.
    /// </summary>
    [JsonPropertyName("oca")]
    public Program Oca
    {
      get => oca ??= new();
      set => oca = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseUnitFunctionAssignmt New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private CsePerson arCsePerson;
    private CaseRole arCaseRole;
    private CsePersonAddress arCsePersonAddress;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private PersonProgram chPersonProgram;
    private Program chProgram;
    private CseOrganization cseOrganization;
    private Program oca;
    private Office office;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CaseUnit caseUnit;
    private CaseUnitFunctionAssignmt new1;
    private Program program;
    private Tribunal tribunal;
  }
#endregion
}
