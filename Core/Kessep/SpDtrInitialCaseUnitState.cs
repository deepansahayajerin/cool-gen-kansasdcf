// Program: SP_DTR_INITIAL_CASE_UNIT_STATE, ID: 371728368, model: 746.
// Short name: SWE02045
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
/// A program: SP_DTR_INITIAL_CASE_UNIT_STATE.
/// </para>
/// <para>
/// This common action block determines what events should be raised to 
/// manipulate the STATE of a newly created Case Unit.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrInitialCaseUnitState: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_INITIAL_CASE_UNIT_STATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrInitialCaseUnitState(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrInitialCaseUnitState.
  /// </summary>
  public SpDtrInitialCaseUnitState(IContext context, Import import,
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
    // Initial development - May 30, 1997
    // Developer - Jack Rookard, MTW
    // *****************************************************************
    // cse_person_address type changed from "F" => "M" in the following
    // READ EACH for Foreign Address
    // ***********************************************
    // Crook 07Dec98 ***
    // ---------------------------------------------
    // 12/09/98  W. Campbell       Removed any code
    //                             which referenced
    //                             CSE_PERSON_ADDRESS
    //                             attributes: START_DATE and
    //                             VERIFIED_CODE.  This work
    //                             was done for IDCR 454.
    // ---------------------------------------------
    // 02/26/99 W.Campbell         Changed the literal in a
    //                             set statement to " set to LENUU on ".
    //                             It was previously " set to PENUU on ".
    // ------------------------------------------
    // 04/10/99 M. Brown Changed usage of obligation summary totals in checking 
    // to see if an AP is obligated.  Use Debt Detail instead. Comments for this
    // change contain 'mfb'.
    // ------------------------------------------
    // 05/25/99 M. Lachowicz      Replace zdel exit state by
    //                            by new exit state.
    // ------------------------------------------
    // 05/26/99 M. Brown Changed 'is obligated' processing.  We no longer write 
    // finance events, or update the 'is obligated' flag on the case unit.
    // We write an event if an active obligation of the types valid for 'is 
    // obligated' processing is found, or if obligations of those types are
    // found but are paid off.
    // ------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 02/10/99  M. Lachowicz          Fixed Address Alert problem.
    //                                 
    // PR #85883.
    // ------------------------------------------------------------
    // 03/10/99  M. Lachowicz          PRWORA changes.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.ReferenceDate = Now().Date;
    local.Infrastructure.UserId = "ROLE";
    local.Infrastructure.SituationNumber = 0;
    local.Incs.Flag = "N";
    local.PaternityRoleFound.Flag = "N";
    local.AddrFound.Flag = "N";
    UseCabConvertDate2String();

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;

      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              CURSOR ONLY
      if (ReadInterstateRequest())
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseUnit())
      {
        local.Hold.State = entities.CaseUnit.State;
        local.ConvCuNum.Text15 = NumberToString(entities.CaseUnit.CuNumber, 15);
        local.HoldCuNum.Text3 = Substring(local.ConvCuNum.Text15, 13, 3);
        local.CaseSrvcType.FuncText1 = Substring(entities.CaseUnit.State, 2, 1);
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      }
      else
      {
        ExitState = "CASE_UNIT_NF_RB";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCsePerson1())
    {
      local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
    }
    else
    {
      ExitState = "CO0000_AP_CSE_PERSON_NF";

      return;
    }

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson2())
    {
      ExitState = "AR_NF_RB";

      return;
    }

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson3())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    // Begin Case Service Type historical record processing.
    local.Infrastructure.EventId = 7;
    local.Infrastructure.BusinessObjectCd = "CAU";

    if (AsChar(local.CaseSrvcType.FuncText1) == 'E')
    {
      local.Infrastructure.ReasonCode = "INITEXPPAT";
      local.Infrastructure.Detail = "Initial state for CU " + local
        .HoldCuNum.Text3;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;

      // ------------------------------------
      // 02/26/99 W.Campbell - Changed the literal in the
      // following set statement to " set to LENUU on ".
      // It was previously  " set to PENUU on ".
      // ------------------------------------
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " set to LENUU on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }
    else if (AsChar(local.CaseSrvcType.FuncText1) == 'F')
    {
      local.Infrastructure.ReasonCode = "FULLNOTLOC";
      local.Infrastructure.Detail = "Initial state for CU " + local
        .HoldCuNum.Text3;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " set to LFNUU on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }
    else if (AsChar(local.CaseSrvcType.FuncText1) == 'L')
    {
      local.Infrastructure.ReasonCode = "LOCONLYNOTLOC";
      local.Infrastructure.Detail = "Initial state for CU " + local
        .HoldCuNum.Text3;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " for Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " set to LLNUU on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }
    else
    {
      // 05/25/99 M. Lachowicz      Replace zdel exit state by
      //                            by new exit state.
      ExitState = "ACO_NE0000_INVALID_CODE";

      return;
    }

    // Begin Locate determination processing.
    local.Infrastructure.EventId = 10;

    if (!Lt(local.Current.Date, entities.ApCsePerson.DateOfDeath) && !
      Equal(entities.ApCsePerson.DateOfDeath, local.Null1.Date))
    {
      // The AP is located by virtue of the fact that CSE knows he is DEAD and 
      // in the grave.
      local.Infrastructure.ReasonCode = "APDEAD";
      local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via date of death on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // 06/22/99 M.L
    //              Change property of the following READ EACH to
    //              OPTIMIZE FOR 1 ROW
    // 02/10/00 M.L  Add condition to check that verified date is less or equal 
    // to current date.
    if (ReadCsePersonAddress2())
    {
      local.AddrFound.Flag = "Y";
      local.Infrastructure.ReasonCode = "ADDRVRFDAP";
      local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via domestic address on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (AsChar(local.AddrFound.Flag) == 'Y')
    {
    }
    else
    {
      // *****************************************************************
      // cse_person_address type changed from "F" => "M" in the following
      // READ EACH for Foreign Address
      // ***********************************************
      // Crook 07Dec98 ***
      // 06/22/99 M.L
      //              Change property of the following READ EACH to
      //              OPTIMIZE FOR 1 ROW
      // 02/10/00 M.L  Add condition to check that verified date is less or 
      // equal to current date.
      if (ReadCsePersonAddress1())
      {
        local.AddrFound.Flag = "Y";
        local.Infrastructure.ReasonCode = "FADDRVRFDAP";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via foreign address on ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
    }

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
        local.Infrastructure.ReasonCode = "INCSVRFD";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
          .Case1.Number;
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via income source on ";
          
        local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
          .TextWorkArea.Text8;
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
    }

    // 06/22/99 M.L
    //              Change property of the following READ EACH to
    //              OPTIMIZE FOR 1 ROW
    if (ReadIncarceration())
    {
      local.Infrastructure.ReasonCode = "APINCARC";
      local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via incarceration ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // 06/22/99 M.L
    //              Change property of the following READ EACH to
    //              OPTIMIZE FOR 1 ROW
    if (ReadMilitaryService())
    {
      local.Infrastructure.ReasonCode = "APINMIL";
      local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " on Case ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
        .Case1.Number;
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " located via military on ";
        
      local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
        .TextWorkArea.Text8;
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // Begin Paternity determination processing.
    local.Infrastructure.EventId = 11;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCaseRole1())
    {
      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseRole2())
      {
        local.PaternityRoleFound.Flag = "Y";
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
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
      else
      {
        // A Case Role of FA where the FA is the AP for the Case is not on the 
        // database.  Read for MO Case Role where MO is AP for the Case.
        // 06/22/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (ReadCaseRole3())
        {
          local.PaternityRoleFound.Flag = "Y";
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
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }
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

    // Begin "AP Is Obligated" determination processing.
    // :mfb - Revision of "is obligated" processing.  Removed case unit updates 
    // of the "is obligated" flag.  Removed generation of finance events.
    // Changed code so that only one SI event is written upon location of an
    // obligation for an AP/child.  This event will either be 'OBLOTHRCASE' (
    // active obligation found) or 'OBLGPAIDOTHRCASE' (obligations found, but
    // all are paid off).
    foreach(var item in ReadObligationObligationTransaction())
    {
      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
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

      ++local.Obligation.Count;

      if (AsChar(entities.ObligationType.Classification) == 'A')
      {
        // mfb: Moved the following check to be done just after obligation type 
        // read, to avoid unnecessary I/O.
        if (AsChar(entities.ObligationTransaction.DebtType) == 'A')
        {
          // : This obligation transaction has accrual instructions tied to it.
          //   Read the accrual  instructions.  If not discontinued, the AP is 
          // obligated.
          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadAccrualInstructions())
          {
            if (Lt(local.Current.Date,
              entities.AccrualInstructions.DiscontinueDt))
            {
              // : AP is obligated - no need to read further.
              local.ApObligated.Flag = "Y";

              break;
            }
            else
            {
              // : Continue
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
          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadDebtDetail2())
          {
            if (entities.DebtDetail.BalanceDueAmt + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
            {
              // : AP is obligated - no need to read further.
              local.ApObligated.Flag = "Y";

              break;
            }
            else
            {
              // : Continue
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
        // This is a non accruing obligation.
        // 06/22/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (ReadDebtDetail1())
        {
          if (entities.DebtDetail.BalanceDueAmt + entities
            .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
          {
            // : AP is obligated - no need to read further.
            local.ApObligated.Flag = "Y";

            break;
          }
          else
          {
            // : Continue
          }
        }
        else
        {
          ExitState = "FN0211_DEBT_DETAIL_NF";

          return;
        }
      }
    }

    // : Write an event if the AP has an active obligation, or if the AP
    //   has obligations, but they are all paid off.
    if (AsChar(local.ApObligated.Flag) == 'Y')
    {
      local.Infrastructure.EventId = 11;
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.ReasonCode = "OBLGOTHRCAS";
      local.Infrastructure.DenormText12 = "";
      local.Infrastructure.DenormNumeric12 = 0;
      local.Infrastructure.UserId = "ROLE";
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
    else if (local.Obligation.Count > 0)
    {
      local.Infrastructure.EventId = 11;
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.ReasonCode = "OBLPAIDOTHRCASE";
      local.Infrastructure.UserId = "ROLE";
      local.Infrastructure.Detail = "Disc or pd obl for AP" + entities
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

    // 03/10/00 M.L Start
    if (AsChar(entities.Ch.PaternityEstablishedIndicator) == 'Y')
    {
      local.Infrastructure.CsePersonNumber = entities.Ch.Number;
      local.Infrastructure.EventId = 11;
      local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.ReasonCode = "PATESTAB";
      UseSpCabCreateInfrastructure();
    }

    // 03/10/00 M.L End
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
        entities.Ch.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
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

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
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

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
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

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Common Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ApObligated.
    /// </summary>
    [JsonPropertyName("apObligated")]
    public Common ApObligated
    {
      get => apObligated ??= new();
      set => apObligated = value;
    }

    /// <summary>
    /// A value of AddrFound.
    /// </summary>
    [JsonPropertyName("addrFound")]
    public Common AddrFound
    {
      get => addrFound ??= new();
      set => addrFound = value;
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
    /// A value of CaseSrvcType.
    /// </summary>
    [JsonPropertyName("caseSrvcType")]
    public CaseFuncWorkSet CaseSrvcType
    {
      get => caseSrvcType ??= new();
      set => caseSrvcType = value;
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

    private Common obligation;
    private Common apObligated;
    private Common addrFound;
    private WorkArea holdCuNum;
    private WorkArea convCuNum;
    private Common goForInfra;
    private Common infraCreated;
    private CaseUnit hold;
    private Case1 holdOther;
    private Common oblNetDue;
    private Common paternityRoleFound;
    private Common incs;
    private CaseFuncWorkSet caseSrvcType;
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
    /// A value of ZdelInfrastructure.
    /// </summary>
    [JsonPropertyName("zdelInfrastructure")]
    public Infrastructure ZdelInfrastructure
    {
      get => zdelInfrastructure ??= new();
      set => zdelInfrastructure = value;
    }

    /// <summary>
    /// A value of ZdelOtherCh.
    /// </summary>
    [JsonPropertyName("zdelOtherCh")]
    public CaseRole ZdelOtherCh
    {
      get => zdelOtherCh ??= new();
      set => zdelOtherCh = value;
    }

    /// <summary>
    /// A value of ZdelOtherAp.
    /// </summary>
    [JsonPropertyName("zdelOtherAp")]
    public CaseRole ZdelOtherAp
    {
      get => zdelOtherAp ??= new();
      set => zdelOtherAp = value;
    }

    /// <summary>
    /// A value of ZdelOther.
    /// </summary>
    [JsonPropertyName("zdelOther")]
    public Case1 ZdelOther
    {
      get => zdelOther ??= new();
      set => zdelOther = value;
    }

    /// <summary>
    /// A value of ZdelObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("zdelObligationPaymentSchedule")]
    public ObligationPaymentSchedule ZdelObligationPaymentSchedule
    {
      get => zdelObligationPaymentSchedule ??= new();
      set => zdelObligationPaymentSchedule = value;
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
    private Infrastructure zdelInfrastructure;
    private CaseRole zdelOtherCh;
    private CaseRole zdelOtherAp;
    private Case1 zdelOther;
    private ObligationPaymentSchedule zdelObligationPaymentSchedule;
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
