// Program: SP_DTR_REACTIVATED_CU_STATE, ID: 371761727, model: 746.
// Short name: SWE01929
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
/// A program: SP_DTR_REACTIVATED_CU_STATE.
/// </para>
/// <para>
/// This common action block determines what events should be raised to 
/// manipulate the STATE of a newly created Case Unit.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrReactivatedCuState: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_REACTIVATED_CU_STATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrReactivatedCuState(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrReactivatedCuState.
  /// </summary>
  public SpDtrReactivatedCuState(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial development - September 16, 1997
    // Developer - Jack Rookard, MTW
    // -------------------------------------------
    // 12/04/98 W. Campbell    An ELSE statement
    //                         was added in order to:
    //                         Default the Service Type of
    //                         the imported Case.
    // -----------------------------------------
    // 12/04/98 W. Campbell    Changed the qualificaton
    //                         in a READ EACH for a foreign
    //                         address to an address type = 'M'
    //                         instead of an address type = 'F'.
    //                         Work done on IDCR454.
    // -----------------------------------------
    // 01/11/99 W. Campbell    Changed the qualificaton
    //                         in two READ EACH stmts
    //                         to remove any logic based
    //                         on attributes ZDEL_START_DATE
    //                         and ZDEL_VERIFIED_CODE.
    //                         Work done on IDCR454.
    // -----------------------------------------
    // 04//10/99 M. Brown Changed usage of obligation summary totals in checking
    // to see if an AP is obligated.  Use Debt Detail instead. Comments for
    // this change contain 'mfb'.
    // ------------------------------------------
    // 05/26/99 M. Brown Changed 'is obligated' processing.  We no longer write 
    // finance events, or update the 'is obligated' flag on the case unit.
    // We write an event if an active obligation of the types valid for 'is 
    // obligated' processing is found, or if obligations of those types are
    // found but are paid off.
    // ------------------------------------------
    // 06/24/99 M.Lachowicz  Change property of READ
    //                        (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 09/15/99  M.Brown Problem report number 73312.  The wrong attribute was 
    // being used to check for accrual instructions,  resulting in a 'debt
    // detail not found' message.
    // ------------------------------------------------------------
    // 02/10/99  M. Lachowicz          Fixed Address Alert problem.
    //                                 
    // PR #85883.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.ReferenceDate = Now().Date;
    local.Infrastructure.UserId = "ROLE";
    local.CreateLocateInfra.Flag = "N";
    local.CreatePaternityInfra.Flag = "N";
    local.Incs.Flag = "N";
    local.PaternityRoleFound.Flag = "N";
    local.ApIsLocated.Flag = "N";
    local.ApIsFa.Flag = "N";
    local.ApIsMo.Flag = "N";
    local.ApIsObligated.Flag = "N";
    UseCabConvertDate2String();

    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;

      // Determine the Service Type of the imported Case.
      if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
      {
        local.ImportedCaseSrvcType.Text1 = "E";
      }
      else if (AsChar(entities.Case1.FullServiceWithMedInd) == 'Y')
      {
        local.ImportedCaseSrvcType.Text1 = "F";
      }
      else if (AsChar(entities.Case1.FullServiceWithoutMedInd) == 'Y')
      {
        local.ImportedCaseSrvcType.Text1 = "F";
      }
      else if (AsChar(entities.Case1.LocateInd) == 'Y')
      {
        local.ImportedCaseSrvcType.Text1 = "L";
      }
      else
      {
        // -------------------------------------------
        // 12/04/98 W. Campbell - This ELSE statement
        // added in order to:
        // Default the Service Type of the imported Case.
        // -------------------------------------------
        local.ImportedCaseSrvcType.Text1 = "F";
      }

      if (ReadInterstateRequest())
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      // 06/24/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseUnit())
      {
        local.Hold.State = entities.CaseUnit.State;
        local.ConvCuNum.Text15 = NumberToString(entities.CaseUnit.CuNumber, 15);
        local.HoldCuNum.Text3 = Substring(local.ConvCuNum.Text15, 13, 3);
        local.CaseUnitFunction.FuncText1 =
          Substring(entities.CaseUnit.State, 1, 1);
        local.CaseUnitSrvcType.FuncText1 =
          Substring(entities.CaseUnit.State, 2, 1);
        local.CaseUnitIsLocatedFlag.Text1 =
          Substring(entities.CaseUnit.State, 3, 1);
        local.CaseUnitIsAnApFlag.Text1 =
          Substring(entities.CaseUnit.State, 4, 1);
        local.CaseUnitIsObgFlag.FuncText1 =
          Substring(entities.CaseUnit.State, 5, 1);
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.SituationNumber = 0;
      }
      else
      {
        ExitState = "CASE_UNIT_NF_RB";

        return;
      }

      if (AsChar(local.CaseUnitSrvcType.FuncText1) == AsChar
        (local.ImportedCaseSrvcType.Text1))
      {
        // The current Case Unit state Service Type flag is in sync with the 
        // imported
        // Case Service Type. Do nothing.
      }
      else
      {
        // The current Case Unit state Service Type flag is not in sync with the
        // imported
        // Case Service Type. Update the Case Unit state to reflect the current 
        // Case Service Type.
        local.CaseUnitSrvcType.FuncText1 = local.ImportedCaseSrvcType.Text1;
        local.Hold.State = local.CaseUnitFunction.FuncText1 + local
          .CaseUnitSrvcType.FuncText1;
        local.Hold.State += TrimEnd(local.CaseUnitIsLocatedFlag.Text1);
        local.Hold.State += TrimEnd(local.CaseUnitIsAnApFlag.Text1);
        local.Hold.State += TrimEnd(local.CaseUnitIsObgFlag.FuncText1);

        try
        {
          UpdateCaseUnit();
          local.Infrastructure.EventId = 7;
          local.Infrastructure.BusinessObjectCd = "CAU";
          local.Infrastructure.Detail =
            "Service Type change for reactivated Case Unit " + local
            .HoldCuNum.Text3;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8;

          switch(AsChar(local.CaseUnitSrvcType.FuncText1))
          {
            case 'E':
              local.Infrastructure.ReasonCode = "SRVEXPAT";
              UseSpCabCreateInfrastructure();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              break;
            case 'F':
              local.Infrastructure.ReasonCode = "SRVFULL";
              UseSpCabCreateInfrastructure();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              break;
            case 'L':
              local.Infrastructure.ReasonCode = "SRVLOCY";
              UseSpCabCreateInfrastructure();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_CODE";
              UseEabRollbackCics();

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
    }
    else
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCsePerson1())
    {
      local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;

      // 06/24/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePerson2())
      {
        // 06/24/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (!ReadCsePerson3())
        {
          ExitState = "OE0056_NF_CHILD_CSE_PERSON";

          return;
        }
      }
      else
      {
        ExitState = "AR_NF_RB";

        return;
      }
    }
    else
    {
      ExitState = "CO0000_AP_CSE_PERSON_NF";

      return;
    }

    // Locate determination processing.
    local.Infrastructure.EventId = 10;

    // Perform AP Date of Death inquiry.
    if (!Lt(local.Current.Date, entities.ApCsePerson.DateOfDeath) && !
      Equal(entities.ApCsePerson.DateOfDeath, local.Null1.Date))
    {
      if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
      {
        // The AP for the current Case Unit was located at some time in the 
        // past.  The "is Located" flag
        //  for the current Case Unit is Y, which is correct. Do not raise "Is 
        // Located" events. Move on to Paternity processing.
        local.ApIsLocated.Flag = "Y";

        goto Test1;
      }

      // The imported Case Unit "is located" flag is N, meaning the AP was not 
      // located in the past.
      // The AP has now been located via a date of death. Raise the appropriate 
      // event.
      local.Infrastructure.ReasonCode = "APDEAD";
      local.Infrastructure.UserId = "APDS";
      local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via date of death on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      local.ApIsLocated.Flag = "Y";
      local.CreateLocateInfra.Flag = "Y";
    }

Test1:

    // Perform AP domestic address inquiry.
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      // -----------------------------------------
      // 01/11/99 W. Campbell    Changed the
      // qualificaton in the following READ EACH
      // to remove any logic based on attributes
      // ZDEL_START_DATE and ZDEL_VERIFIED_CODE.
      // -----------------------------------------
      // 06/24/99 M.L
      //              Change property of the following READ EACH to OPTIMIZE FOR
      //              1 ROW
      // 02/10/00 M.L  Add condition to check that verified date is less or 
      // equal to current date.
      if (ReadCsePersonAddress2())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // The AP for the current Case Unit was located at some time in the 
          // past.  The "is Located" flag
          //  for the current Case Unit is Y, which is correct. Do not raise "Is
          // Located" events. Move on to Paternity processing.
          local.ApIsLocated.Flag = "Y";

          goto Test2;
        }

        // The imported Case Unit "is located" flag is N, meaning the AP was not
        // located in the past.
        // The AP has now been located via a verified address. Raise the 
        // appropriate event.
        local.Infrastructure.ReasonCode = "ADDRVRFDAP";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via domestic address on ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test2:

    // Perform AP foreign address inquiry.
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      // -----------------------------------------
      // 12/04/98 W. Campbell    Changed the
      // qualificaton in the following READ EACH
      // to an address type of 'M' instead of an
      // address type 'F'.
      // -----------------------------------------
      // -----------------------------------------
      // 01/11/99 W. Campbell    Changed the
      // qualificaton in the following READ EACH
      // to remove any logic based on attributes
      // ZDEL_START_DATE and ZDEL_VERIFIED_CODE.
      // -----------------------------------------
      // 06/24/99 M.L
      //              Change property of the following READ EACH to OPTIMIZE FOR
      //              1 ROW
      // 02/10/00 M.L  Add condition to check that verified date is less or 
      // equal to current date.
      if (ReadCsePersonAddress1())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // The AP for the current Case Unit was located at some time in the 
          // past.  The "is Located" flag
          //  for the current Case Unit is Y, which is correct. Do not raise "Is
          // Located" events. Move on to Paternity processing.
          local.ApIsLocated.Flag = "Y";

          goto Test3;
        }

        // The imported Case Unit "is located" flag is N, meaning the AP was not
        // located in the past.
        // The AP has now been located via a verified foreign address. Raise the
        // appropriate event.
        local.Infrastructure.ReasonCode = "FADDRVRFDAP";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via foreign address on ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test3:

    // Perform AP income source inquiry.
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      foreach(var item in ReadIncomeSource())
      {
        switch(AsChar(entities.ApIncomeSource.Type1))
        {
          case 'E':
            if (AsChar(entities.ApIncomeSource.ReturnCd) == 'E' || AsChar
              (entities.ApIncomeSource.ReturnCd) == 'W')
            {
              local.Incs.Flag = "Y";
            }

            break;
          case 'M':
            if (AsChar(entities.ApIncomeSource.ReturnCd) == 'A')
            {
              local.Incs.Flag = "Y";
            }

            break;
          default:
            continue;
        }

        if (AsChar(local.Incs.Flag) == 'Y')
        {
          if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
          {
            // The AP for the current Case Unit was located at some time in the 
            // past.  The "is Located" flag
            // for the current Case Unit is Y, which is correct. Do not raise "
            // Is Located" events.
            // Move on to Paternity processing.
            local.ApIsLocated.Flag = "Y";

            goto Test4;
          }

          // The imported Case Unit "is located" flag is N, meaning the AP was 
          // not located in the past.
          // The AP has now been located via a verified income source. Raise the
          // appropriate event.
          local.Infrastructure.ReasonCode = "INCSVRFD";
          local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via income source on ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8;
          local.ApIsLocated.Flag = "Y";
          local.CreateLocateInfra.Flag = "Y";

          goto Test4;
        }
      }
    }

Test4:

    // Perform AP incarceration inquiry.
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      // 06/24/99 M.L
      //              Change property of the following READ EACH to OPTIMIZE FOR
      //              1 ROW
      if (ReadIncarceration())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // The AP for the current Case Unit was located at some time in the 
          // past.  The "is Located" flag
          // for the current Case Unit is Y, which is correct. Do not raise "Is 
          // Located" events.
          // Move on to Paternity processing.
          local.ApIsLocated.Flag = "Y";

          goto Test5;
        }

        // The imported Case Unit "is located" flag is N, meaning the AP was not
        // located in the past.
        // The AP has now been located via a verified incarceration. Raise the 
        // appropriate event.
        local.Infrastructure.ReasonCode = "APINCARC";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via incarceration ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test5:

    // Perform AP military service inquiry.
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      // 06/24/99 M.L
      //              Change property of the following READ EACH to OPTIMIZE FOR
      //              1 ROW
      if (ReadMilitaryService())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // The AP for the current Case Unit was located at some time in the 
          // past.  The "is Located" flag
          // for the current Case Unit is Y, which is correct. Do not raise "Is 
          // Located" events.
          // Move on to Paternity processing.
          local.ApIsLocated.Flag = "Y";

          goto Test6;
        }

        // The imported Case Unit "is located" flag is N, meaning the AP was not
        // located in the past.
        // The AP has now been located via a military service occurrence. Raise 
        // the appropriate event.
        local.Infrastructure.ReasonCode = "APINMIL";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via military on ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test6:

    if (AsChar(local.CreateLocateInfra.Flag) == 'Y')
    {
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // Paternity determination processing.
    local.CaseUnitIsAnApFlag.Text1 = Substring(entities.CaseUnit.State, 4, 1);
    local.Infrastructure.EventId = 11;

    // 06/24/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCaseRole1())
    {
      // 06/24/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseRole2())
      {
        local.PaternityRoleFound.Flag = "Y";
        local.ApIsFa.Flag = "Y";

        if (AsChar(local.CaseUnitIsAnApFlag.Text1) == 'Y')
        {
          // Paternity has already been established between the AP and the CH 
          // for the current Case Unit
          // and the "Is an AP" flag in the Case Unit state is correct. Do not 
          // raise any Paternity events.
          goto Read;
        }

        local.Infrastructure.ReasonCode = "APASSGNFA";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " is FA for CH ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Ch.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + "-";
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        local.CreatePaternityInfra.Flag = "Y";
      }
      else
      {
        // A Case Role of FA where the FA is the AP for the Case is not on the 
        // database.
        // Read for MO Case Role where MO is AP for the Case.
        // 06/24/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (ReadCaseRole3())
        {
          local.PaternityRoleFound.Flag = "Y";
          local.ApIsMo.Flag = "Y";

          if (AsChar(local.CaseUnitIsAnApFlag.Text1) == 'Y')
          {
            // Paternity has already been established between the AP and the CH 
            // for the current Case Unit
            // and the "Is an AP" flag in the Case Unit state is correct. Do not
            // raise any Paternity events.
            goto Read;
          }

          local.Infrastructure.ReasonCode = "APASSGNMO";
          local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " is MO for CH ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Ch.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + "-"
            ;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8;
          local.CreatePaternityInfra.Flag = "Y";
        }
        else
        {
          // A Case Role of MO where the MO is the AP for the Case is not on the
          // database.
        }
      }
    }
    else
    {
      ExitState = "CO0000_AP_NF";

      return;
    }

Read:

    if (AsChar(local.CreatePaternityInfra.Flag) == 'Y')
    {
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // AP Is Obligated determination processing.
    // :mfb - Revision of "is obligated" processing.  Removed case unit updates 
    // of the "is obligated" flag.  Removed generation of finance events.
    // Changed code so that only one SI event is written upon location of an
    // obligation for an AP/child.  This event will either be 'OBLOTHRCASE' (
    // active obligation found) or 'OBLGPAIDOTHRCASE' (obligations found, but
    // all are paid off).
    foreach(var item in ReadObligationObligationTransaction())
    {
      // 06/24/99 M.L
      //              Change property of the following READ to generate SELECT 
      // only
      if (ReadObligationType())
      {
        // : These are the only obligation types to be considered for 'is 
        // obligated' processing.
        switch(TrimEnd(entities.ObligationType.Code))
        {
          case "CS":
            break;
          case "MS":
            break;
          case "SP":
            break;
          case "AJ":
            break;
          case "CRCH":
            break;
          case "MC":
            break;
          case "IJ":
            break;
          case "MJ":
            break;
          case "SAJ":
            break;
          case "%UME":
            break;
          case "718B":
            break;
          default:
            continue;
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

        return;
      }

      local.ObligationFound.Flag = "Y";

      if (AsChar(entities.ObligationType.Classification) == 'A')
      {
        // 09/15/99  M.Brown Problem report number 73312. The attribute '
        // obligation transaction type' was being checked instead of 'obligation
        // transaction debt_type'.
        // ------------------------------------------------------------
        if (AsChar(entities.ObligationTransaction.DebtType) == 'A')
        {
          // : This obligation transaction has accrual instructions tied to it.
          //   Read the accrual  instructions.  If not discontinued, the AP is 
          // obligated.
          // 06/24/99 M.L
          //              Change property of the following READ to generate 
          // SELECT only
          if (ReadAccrualInstructions())
          {
            if (Lt(Now().Date, entities.AccrualInstructions.DiscontinueDt))
            {
              local.ApIsObligated.Flag = "Y";

              break;
            }
          }
          else
          {
            ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";

            return;
          }
        }
        else
        {
          // : This obligation transaction has a debt detail tied to it.
          //   Check balance due on it - if > 0, the AP is obligated.
          // 06/24/99 M.L
          //              Change property of the following READ to generate 
          // SELECT only
          if (ReadDebtDetail())
          {
            if (entities.DebtDetail.BalanceDueAmt + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
            {
              local.ApIsObligated.Flag = "Y";

              break;
            }
          }
          else
          {
            ExitState = "FN0211_DEBT_DETAIL_NF";

            return;
          }
        }
      }
      else
      {
        // : This is a non accruing obligation.
        // 06/24/99 M.L
        //              Change property of the following READ to generate SELECT
        // only
        if (ReadDebtDetail())
        {
          if (entities.DebtDetail.BalanceDueAmt + entities
            .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
          {
            local.ApIsObligated.Flag = "Y";

            break;
          }
          else
          {
          }
        }
        else
        {
          ExitState = "FN0211_DEBT_DETAIL_NF";

          return;
        }
      }
    }

    // : Evaluate flags set in the read of obligations for the AP/Child.
    //   Write events accordingly.
    // Reinitialize the appropriate local Infrastructure views.
    local.Infrastructure.UserId = "ROLE";
    local.Infrastructure.EventId = 11;
    local.Infrastructure.BusinessObjectCd = "CAU";

    if (AsChar(local.ApIsObligated.Flag) == 'Y')
    {
      local.Infrastructure.ReasonCode = "OBLGOTHRCAS";
      local.Infrastructure.DenormText12 = "";
      local.Infrastructure.DenormNumeric12 = 0;
      local.Infrastructure.Detail = "An active oblg found for AP" + entities
        .ApCsePerson.Number;
      local.Infrastructure.Detail = " CH" + entities.Ch.Number;
      local.Infrastructure.Detail = " on Case " + import.Case1.Number;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.InfraCreated.Flag = "Y";
      }
      else
      {
        return;
      }
    }
    else if (AsChar(local.ObligationFound.Flag) == 'Y')
    {
      local.Infrastructure.ReasonCode = "OBLPAIDOTHRCASE";
      local.Infrastructure.Detail = "Disc or paid oblig for AP" + entities
        .ApCsePerson.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " CH";
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Ch.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " found on Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + import
        .Case1.Number;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // 05/25/99 MFB: End of 'is obligated' processing changes.
    local.Infrastructure.EventId = 7;
    local.Infrastructure.BusinessObjectCd = "CAU";
    local.Infrastructure.DenormText12 = "";
    local.Infrastructure.DenormNumeric12 = 0;
    local.Infrastructure.UserId = "ROLE";

    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
      {
        // Case Unit state is out of sync with actual data - data indicates AP 
        // not located.
        // raise event 10, reason code REACTV_NOTLOC
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReasonCode = "REACTV_NOTLOC";
        local.Infrastructure.Detail = "AP notloc on reactivated CU " + local
          .HoldCuNum.Text3;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " ";
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.InfraCreated.Flag = "Y";
        }
        else
        {
          return;
        }

        local.Infrastructure.EventId = 7;
      }

      if (AsChar(local.PaternityRoleFound.Flag) == 'N')
      {
        if (AsChar(local.ApIsObligated.Flag) == 'N')
        {
          // raise event 7, reason code NOTLOC, CAU
          local.Infrastructure.ReasonCode = "NOTLOC";
          local.Infrastructure.Detail =
            "AP notloc-no oblg fnd-no pat on reactivated CU " + local
            .HoldCuNum.Text3;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8;
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.InfraCreated.Flag = "Y";
          }
          else
          {
            return;
          }
        }

        if (AsChar(local.ApIsObligated.Flag) == 'Y')
        {
          // raise event 7, reason code OBLIG, CAU
          local.Infrastructure.ReasonCode = "OBLIG";
          local.Infrastructure.Detail =
            "AP notloc-oblg found-no pat on reactivated CU " + local
            .HoldCuNum.Text3;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8;
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.InfraCreated.Flag = "Y";
          }
          else
          {
            return;
          }
        }
      }
      else
      {
        if (AsChar(local.ApIsFa.Flag) == 'Y')
        {
          if (AsChar(local.ApIsObligated.Flag) == 'Y')
          {
            // raise event 7, reason code NOTLOC_OBLPAFA, CAU
            local.Infrastructure.ReasonCode = "NOTLOC_OBLPAFA";
            local.Infrastructure.Detail =
              "AP notloc-oblg found-AP is FA on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.ApIsObligated.Flag) == 'N')
          {
            // raise event 7, reason code NOTLOC_ISPAFA, CAU
            local.Infrastructure.ReasonCode = "NOTLOC_ISPAFA";
            local.Infrastructure.Detail =
              "AP notloc-oblg not fnd-AP is FA on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }
        }

        if (AsChar(local.ApIsMo.Flag) == 'Y')
        {
          if (AsChar(local.ApIsObligated.Flag) == 'Y')
          {
            // raise event 7, reason code NOTLOC_OBLPAMO, CAU
            local.Infrastructure.ReasonCode = "NOTLOC_OBLPAMO";
            local.Infrastructure.Detail =
              "AP notloc-oblg found-AP is MO on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.ApIsObligated.Flag) == 'N')
          {
            // raise event 7, reason code NOTLOC_ISPAMO, CAU
            local.Infrastructure.ReasonCode = "NOTLOC_ISPAMO";
            local.Infrastructure.Detail =
              "AP notloc-oblg not fnd-AP is MO on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }
        }
      }
    }

    if (AsChar(local.ApIsLocated.Flag) == 'Y')
    {
      if (AsChar(local.PaternityRoleFound.Flag) == 'N')
      {
        if (AsChar(local.CaseUnitIsAnApFlag.Text1) == 'U')
        {
          // raise event 7, reason code LOCATED.
          local.Infrastructure.ReasonCode = "LOCATED";
          local.Infrastructure.Detail =
            "AP located-no paternity on reactivated CU " + local
            .HoldCuNum.Text3;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8;
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.InfraCreated.Flag = "Y";
          }
          else
          {
            return;
          }
        }
        else
        {
          if (AsChar(local.ApIsObligated.Flag) == 'Y')
          {
            if (AsChar(local.CaseUnitIsObgFlag.FuncText1) == 'Y')
            {
              // raise event 7, reason code OBLIG.
              local.Infrastructure.ReasonCode = "OBLIG";
              local.Infrastructure.Detail =
                "AP located-obligated-no pat on reactivated CU " + local
                .HoldCuNum.Text3;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " for Case ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.InfraCreated.Flag = "Y";
              }
              else
              {
                return;
              }
            }
          }

          if (AsChar(local.ApIsObligated.Flag) == 'N')
          {
            if (AsChar(local.CaseUnitIsObgFlag.FuncText1) == 'N')
            {
              // raise event 7, reason code NOTOBLIG.
              local.Infrastructure.ReasonCode = "NOTOBLIG";
              local.Infrastructure.Detail =
                "AP located-no oblg-no pat on reactivated CU " + local
                .HoldCuNum.Text3;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " for Case ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.InfraCreated.Flag = "Y";
              }
              else
              {
                return;
              }
            }
          }
        }
      }

      if (AsChar(local.PaternityRoleFound.Flag) == 'Y')
      {
        if (AsChar(local.ApIsFa.Flag) == 'Y')
        {
          if (AsChar(local.ApIsObligated.Flag) == 'Y')
          {
            // raise event 7, reason code ISAPFA_OBG_ESTB
            local.Infrastructure.ReasonCode = "ISAPFA_OBG_ESTB";
            local.Infrastructure.Detail =
              "AP located-obligated-pat est on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.ApIsObligated.Flag) == 'N')
          {
            // raise event 7, reason code ISAPFA
            local.Infrastructure.ReasonCode = "ISAPFA";
            local.Infrastructure.Detail =
              "AP located-not oblg-pat est on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }
        }

        if (AsChar(local.ApIsMo.Flag) == 'Y')
        {
          if (AsChar(local.ApIsObligated.Flag) == 'Y')
          {
            // raise event 7, reason code ISAPMO_OBG_ESTB
            local.Infrastructure.ReasonCode = "ISAPMO_OBG_ESTB";
            local.Infrastructure.Detail =
              "AP located-obligated-pat est on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
              return;
            }
          }

          if (AsChar(local.ApIsObligated.Flag) == 'N')
          {
            // raise event 7, reason code ISAPMO
            local.Infrastructure.ReasonCode = "ISAPMO";
            local.Infrastructure.Detail =
              "AP located-not oblg-pat est on reactivated CU " + local
              .HoldCuNum.Text3;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " for Case ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.InfraCreated.Flag = "Y";
            }
            else
            {
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
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
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
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 1);
        entities.Case1.LocateInd = db.GetNullableString(reader, 2);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.Case1.Status = db.GetNullableString(reader, 4);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 5);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 6);
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
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Fa.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fa.CasNumber = db.GetString(reader, 0);
        entities.Fa.CspNumber = db.GetString(reader, 1);
        entities.Fa.Type1 = db.GetString(reader, 2);
        entities.Fa.Identifier = db.GetInt32(reader, 3);
        entities.Fa.StartDate = db.GetNullableDate(reader, 4);
        entities.Fa.EndDate = db.GetNullableDate(reader, 5);
        entities.Fa.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Fa.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.Mo.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Mo.CasNumber = db.GetString(reader, 0);
        entities.Mo.CspNumber = db.GetString(reader, 1);
        entities.Mo.Type1 = db.GetString(reader, 2);
        entities.Mo.Identifier = db.GetInt32(reader, 3);
        entities.Mo.StartDate = db.GetNullableDate(reader, 4);
        entities.Mo.EndDate = db.GetNullableDate(reader, 5);
        entities.Mo.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Mo.Type1);
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 8);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 9);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 10);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAp ?? "");
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAr ?? "");
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ch.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoChild ?? "");
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Type1 = db.GetString(reader, 1);
        entities.Ch.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Ch.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ch.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.ApCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ApCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ApCsePersonAddress.ZdelStartDate =
          db.GetNullableDate(reader, 2);
        entities.ApCsePersonAddress.Type1 = db.GetNullableString(reader, 3);
        entities.ApCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 4);
        entities.ApCsePersonAddress.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 6);
        entities.ApCsePersonAddress.LocationType = db.GetString(reader, 7);
        entities.ApCsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.ApCsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.ApCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ApCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ApCsePersonAddress.ZdelStartDate =
          db.GetNullableDate(reader, 2);
        entities.ApCsePersonAddress.Type1 = db.GetNullableString(reader, 3);
        entities.ApCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 4);
        entities.ApCsePersonAddress.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 6);
        entities.ApCsePersonAddress.LocationType = db.GetString(reader, 7);
        entities.ApCsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.ApCsePersonAddress.LocationType);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadIncarceration()
  {
    entities.ApIncarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApIncarceration.CspNumber = db.GetString(reader, 0);
        entities.ApIncarceration.Identifier = db.GetInt32(reader, 1);
        entities.ApIncarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.ApIncarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.ApIncarceration.StartDate = db.GetNullableDate(reader, 4);
        entities.ApIncarceration.Type1 = db.GetNullableString(reader, 5);
        entities.ApIncarceration.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.ApIncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ApIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ApIncomeSource.ReturnCd = db.GetNullableString(reader, 2);
        entities.ApIncomeSource.CspINumber = db.GetString(reader, 3);
        entities.ApIncomeSource.StartDt = db.GetNullableDate(reader, 4);
        entities.ApIncomeSource.EndDt = db.GetNullableDate(reader, 5);
        entities.ApIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ApIncomeSource.Type1);

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

  private bool ReadMilitaryService()
  {
    entities.ApMilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApMilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.ApMilitaryService.CspNumber = db.GetString(reader, 1);
        entities.ApMilitaryService.StartDate = db.GetNullableDate(reader, 2);
        entities.ApMilitaryService.EndDate = db.GetNullableDate(reader, 3);
        entities.ApMilitaryService.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTransaction()
  {
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(command, "cspSupNumber", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 4);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 5);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 6);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 7);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 8);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 10);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 11);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 12);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private void UpdateCaseUnit()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var state = local.Hold.State;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "state", state);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.State = state;
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

    private CaseUnit caseUnit;
    private Case1 case1;
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

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private Infrastructure infrastructure;
    private CaseUnit caseUnit;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ObligationFound.
    /// </summary>
    [JsonPropertyName("obligationFound")]
    public Common ObligationFound
    {
      get => obligationFound ??= new();
      set => obligationFound = value;
    }

    /// <summary>
    /// A value of CreatePaternityInfra.
    /// </summary>
    [JsonPropertyName("createPaternityInfra")]
    public Common CreatePaternityInfra
    {
      get => createPaternityInfra ??= new();
      set => createPaternityInfra = value;
    }

    /// <summary>
    /// A value of CreateLocateInfra.
    /// </summary>
    [JsonPropertyName("createLocateInfra")]
    public Common CreateLocateInfra
    {
      get => createLocateInfra ??= new();
      set => createLocateInfra = value;
    }

    /// <summary>
    /// A value of ApIsLocated.
    /// </summary>
    [JsonPropertyName("apIsLocated")]
    public Common ApIsLocated
    {
      get => apIsLocated ??= new();
      set => apIsLocated = value;
    }

    /// <summary>
    /// A value of ApIsObligated.
    /// </summary>
    [JsonPropertyName("apIsObligated")]
    public Common ApIsObligated
    {
      get => apIsObligated ??= new();
      set => apIsObligated = value;
    }

    /// <summary>
    /// A value of ZdelApIsPreviouslyObligated.
    /// </summary>
    [JsonPropertyName("zdelApIsPreviouslyObligated")]
    public Common ZdelApIsPreviouslyObligated
    {
      get => zdelApIsPreviouslyObligated ??= new();
      set => zdelApIsPreviouslyObligated = value;
    }

    /// <summary>
    /// A value of ApIsMo.
    /// </summary>
    [JsonPropertyName("apIsMo")]
    public Common ApIsMo
    {
      get => apIsMo ??= new();
      set => apIsMo = value;
    }

    /// <summary>
    /// A value of ApIsFa.
    /// </summary>
    [JsonPropertyName("apIsFa")]
    public Common ApIsFa
    {
      get => apIsFa ??= new();
      set => apIsFa = value;
    }

    /// <summary>
    /// A value of CaseUnitIsObgFlag.
    /// </summary>
    [JsonPropertyName("caseUnitIsObgFlag")]
    public CaseFuncWorkSet CaseUnitIsObgFlag
    {
      get => caseUnitIsObgFlag ??= new();
      set => caseUnitIsObgFlag = value;
    }

    /// <summary>
    /// A value of CaseUnitFunction.
    /// </summary>
    [JsonPropertyName("caseUnitFunction")]
    public CaseFuncWorkSet CaseUnitFunction
    {
      get => caseUnitFunction ??= new();
      set => caseUnitFunction = value;
    }

    /// <summary>
    /// A value of ImportedCaseSrvcType.
    /// </summary>
    [JsonPropertyName("importedCaseSrvcType")]
    public TextWorkArea ImportedCaseSrvcType
    {
      get => importedCaseSrvcType ??= new();
      set => importedCaseSrvcType = value;
    }

    /// <summary>
    /// A value of CaseUnitIsAnApFlag.
    /// </summary>
    [JsonPropertyName("caseUnitIsAnApFlag")]
    public TextWorkArea CaseUnitIsAnApFlag
    {
      get => caseUnitIsAnApFlag ??= new();
      set => caseUnitIsAnApFlag = value;
    }

    /// <summary>
    /// A value of CaseUnitIsLocatedFlag.
    /// </summary>
    [JsonPropertyName("caseUnitIsLocatedFlag")]
    public TextWorkArea CaseUnitIsLocatedFlag
    {
      get => caseUnitIsLocatedFlag ??= new();
      set => caseUnitIsLocatedFlag = value;
    }

    /// <summary>
    /// A value of HoldCuNum.
    /// </summary>
    [JsonPropertyName("holdCuNum")]
    public WorkArea HoldCuNum
    {
      get => holdCuNum ??= new();
      set => holdCuNum = value;
    }

    /// <summary>
    /// A value of ConvCuNum.
    /// </summary>
    [JsonPropertyName("convCuNum")]
    public WorkArea ConvCuNum
    {
      get => convCuNum ??= new();
      set => convCuNum = value;
    }

    /// <summary>
    /// A value of GoForInfra.
    /// </summary>
    [JsonPropertyName("goForInfra")]
    public Common GoForInfra
    {
      get => goForInfra ??= new();
      set => goForInfra = value;
    }

    /// <summary>
    /// A value of InfraCreated.
    /// </summary>
    [JsonPropertyName("infraCreated")]
    public Common InfraCreated
    {
      get => infraCreated ??= new();
      set => infraCreated = value;
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
    /// A value of HoldOther.
    /// </summary>
    [JsonPropertyName("holdOther")]
    public Case1 HoldOther
    {
      get => holdOther ??= new();
      set => holdOther = value;
    }

    /// <summary>
    /// A value of OblNetDue.
    /// </summary>
    [JsonPropertyName("oblNetDue")]
    public Common OblNetDue
    {
      get => oblNetDue ??= new();
      set => oblNetDue = value;
    }

    /// <summary>
    /// A value of PaternityRoleFound.
    /// </summary>
    [JsonPropertyName("paternityRoleFound")]
    public Common PaternityRoleFound
    {
      get => paternityRoleFound ??= new();
      set => paternityRoleFound = value;
    }

    /// <summary>
    /// A value of Incs.
    /// </summary>
    [JsonPropertyName("incs")]
    public Common Incs
    {
      get => incs ??= new();
      set => incs = value;
    }

    /// <summary>
    /// A value of CaseUnitSrvcType.
    /// </summary>
    [JsonPropertyName("caseUnitSrvcType")]
    public CaseFuncWorkSet CaseUnitSrvcType
    {
      get => caseUnitSrvcType ??= new();
      set => caseUnitSrvcType = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private Common obligationFound;
    private Common createPaternityInfra;
    private Common createLocateInfra;
    private Common apIsLocated;
    private Common apIsObligated;
    private Common zdelApIsPreviouslyObligated;
    private Common apIsMo;
    private Common apIsFa;
    private CaseFuncWorkSet caseUnitIsObgFlag;
    private CaseFuncWorkSet caseUnitFunction;
    private TextWorkArea importedCaseSrvcType;
    private TextWorkArea caseUnitIsAnApFlag;
    private TextWorkArea caseUnitIsLocatedFlag;
    private WorkArea holdCuNum;
    private WorkArea convCuNum;
    private Common goForInfra;
    private Common infraCreated;
    private CaseUnit hold;
    private Case1 holdOther;
    private Common oblNetDue;
    private Common paternityRoleFound;
    private Common incs;
    private CaseFuncWorkSet caseUnitSrvcType;
    private TextWorkArea textWorkArea;
    private DateWorkArea null1;
    private DateWorkArea max;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of OtherCh.
    /// </summary>
    [JsonPropertyName("otherCh")]
    public CaseRole OtherCh
    {
      get => otherCh ??= new();
      set => otherCh = value;
    }

    /// <summary>
    /// A value of OtherAp.
    /// </summary>
    [JsonPropertyName("otherAp")]
    public CaseRole OtherAp
    {
      get => otherAp ??= new();
      set => otherAp = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public Case1 Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ObligationPaymentSchedule Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ZdelPrevCase.
    /// </summary>
    [JsonPropertyName("zdelPrevCase")]
    public Case1 ZdelPrevCase
    {
      get => zdelPrevCase ??= new();
      set => zdelPrevCase = value;
    }

    /// <summary>
    /// A value of ZdelPrevCaseUnit.
    /// </summary>
    [JsonPropertyName("zdelPrevCaseUnit")]
    public CaseUnit ZdelPrevCaseUnit
    {
      get => zdelPrevCaseUnit ??= new();
      set => zdelPrevCaseUnit = value;
    }

    /// <summary>
    /// A value of Mo.
    /// </summary>
    [JsonPropertyName("mo")]
    public CaseRole Mo
    {
      get => mo ??= new();
      set => mo = value;
    }

    /// <summary>
    /// A value of Fa.
    /// </summary>
    [JsonPropertyName("fa")]
    public CaseRole Fa
    {
      get => fa ??= new();
      set => fa = value;
    }

    /// <summary>
    /// A value of ZdelCh.
    /// </summary>
    [JsonPropertyName("zdelCh")]
    public CaseRole ZdelCh
    {
      get => zdelCh ??= new();
      set => zdelCh = value;
    }

    /// <summary>
    /// A value of ZdelAr.
    /// </summary>
    [JsonPropertyName("zdelAr")]
    public CaseRole ZdelAr
    {
      get => zdelAr ??= new();
      set => zdelAr = value;
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
    /// A value of ApMilitaryService.
    /// </summary>
    [JsonPropertyName("apMilitaryService")]
    public MilitaryService ApMilitaryService
    {
      get => apMilitaryService ??= new();
      set => apMilitaryService = value;
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
    /// A value of ApIncarceration.
    /// </summary>
    [JsonPropertyName("apIncarceration")]
    public Incarceration ApIncarceration
    {
      get => apIncarceration ??= new();
      set => apIncarceration = value;
    }

    /// <summary>
    /// A value of ApIncomeSource.
    /// </summary>
    [JsonPropertyName("apIncomeSource")]
    public IncomeSource ApIncomeSource
    {
      get => apIncomeSource ??= new();
      set => apIncomeSource = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ZdelAp.
    /// </summary>
    [JsonPropertyName("zdelAp")]
    public CsePersonAccount ZdelAp
    {
      get => zdelAp ??= new();
      set => zdelAp = value;
    }

    /// <summary>
    /// A value of ZdelChild.
    /// </summary>
    [JsonPropertyName("zdelChild")]
    public CsePersonAccount ZdelChild
    {
      get => zdelChild ??= new();
      set => zdelChild = value;
    }

    private CsePersonAccount supported;
    private CsePersonAccount obligor;
    private Infrastructure infrastructure;
    private CaseRole otherCh;
    private CaseRole otherAp;
    private Case1 other;
    private ObligationPaymentSchedule zdel;
    private DebtDetail debtDetail;
    private AccrualInstructions accrualInstructions;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private Case1 zdelPrevCase;
    private CaseUnit zdelPrevCaseUnit;
    private CaseRole mo;
    private CaseRole fa;
    private CaseRole zdelCh;
    private CaseRole zdelAr;
    private CaseRole apCaseRole;
    private MilitaryService apMilitaryService;
    private InterstateRequest interstateRequest;
    private Incarceration apIncarceration;
    private IncomeSource apIncomeSource;
    private CsePersonAddress apCsePersonAddress;
    private CsePerson ch;
    private CsePerson ar;
    private CsePerson apCsePerson;
    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePersonAccount zdelAp;
    private CsePersonAccount zdelChild;
  }
#endregion
}
