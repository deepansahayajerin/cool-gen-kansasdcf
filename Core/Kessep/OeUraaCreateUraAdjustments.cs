// Program: OE_URAA_CREATE_URA_ADJUSTMENTS, ID: 374460006, model: 746.
// Short name: SWE02534
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_URAA_CREATE_URA_ADJUSTMENTS.
/// </summary>
[Serializable]
public partial class OeUraaCreateUraAdjustments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAA_CREATE_URA_ADJUSTMENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUraaCreateUraAdjustments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUraaCreateUraAdjustments.
  /// </summary>
  public OeUraaCreateUraAdjustments(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	      CHG REQ# DESCRIPTION
    // Madhu Kumar      05-16-2000   Initial Code
    // Fangman          08-07-2000   Changed code to add new edit checks and 
    // improve efficiency.
    // January, 2002 - M Brown - Work Order Number: 010504 - Retro Processing.
    // Added check for protected collections when adjustments added, and 
    // populate obligation
    // key info if the condition does exist, so that the user can flow to COLP 
    // if desired.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeMs.SystemGeneratedIdentifier = 3;
    local.HardcodeMj.SystemGeneratedIdentifier = 10;
    local.HardcodeUme.SystemGeneratedIdentifier = 11;
    local.HardcodeMc.SystemGeneratedIdentifier = 19;

    if (!ReadCsePerson1())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadImHousehold())
    {
      ExitState = "IM_HOUSEHOLD_NF";

      return;
    }

    if (import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount < 0)
    {
      // : WO# 010504 - Retro Processing: Check for protected collections 
      // related to
      //  the ura collections being adjusted.  If they exist, give a message 
      // saying
      //  that collections must be unprotected before proceeding.
      if (AsChar(import.ImHouseholdMbrMnthlyAdj.LevelAppliedTo) == 'M')
      {
        if (!ReadImHouseholdMbrMnthlySum2())
        {
          ExitState = "OE0000_IM_HH_MBR_MNTHLY_SUM_NF";

          return;
        }

        if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'M')
        {
          // : Read for member, medical
          foreach(var item in ReadCollectionObligationObligationTypeCsePerson4())
            
          {
            if (entities.Obligation.SystemGeneratedIdentifier != export
              .ProtCollObligation.SystemGeneratedIdentifier || !
              Equal(entities.Obligor.Number, export.ProtCollObligor.Number) || entities
              .ObligationType.SystemGeneratedIdentifier != export
              .ProtCollObligationType.SystemGeneratedIdentifier)
            {
              // : Will use counter to set exitstate.  If more than one 
              // obligation has
              //  protected collections, exitstate will be set to let the worker
              // know
              //  that multiple obligations have protected collections.
              ++local.ProtCollCount.Count;
            }

            if (export.ProtCollObligation.SystemGeneratedIdentifier == 0)
            {
              export.ProtCollObligation.Assign(entities.Obligation);
              MoveObligationType(entities.ObligationType,
                export.ProtCollObligationType);
              export.ProtCollObligor.Number = entities.Obligor.Number;
              export.Obligor.Number = entities.Obligor.Number;
            }

            if (local.ProtCollCount.Count > 1)
            {
              break;
            }
          }

          if (local.ProtCollCount.Count == 0)
          {
          }
          else if (local.ProtCollCount.Count > 1)
          {
            ExitState = "FN000_MUST_UNPROT_MULT_COLLS";

            return;
          }
          else
          {
            ExitState = "FN0000_URAA_MUST_UNPROT_COLLS";

            return;
          }
        }
        else
        {
          // : Read for member, non-medical
          foreach(var item in ReadCollectionObligationObligationTypeCsePerson3())
            
          {
            if (entities.Obligation.SystemGeneratedIdentifier != export
              .ProtCollObligation.SystemGeneratedIdentifier || !
              Equal(entities.Obligor.Number, export.ProtCollObligor.Number) || entities
              .ObligationType.SystemGeneratedIdentifier != export
              .ProtCollObligationType.SystemGeneratedIdentifier)
            {
              // : Will use counter to set exitstate.  If more than one 
              // obligation has
              //  protected collections, exitstate will be set to let the worker
              // know
              //  that multiple obligations have protected collections.
              ++local.ProtCollCount.Count;
            }

            if (export.ProtCollObligation.SystemGeneratedIdentifier == 0)
            {
              export.ProtCollObligation.Assign(entities.Obligation);
              MoveObligationType(entities.ObligationType,
                export.ProtCollObligationType);
              export.ProtCollObligor.Number = entities.Obligor.Number;
              export.Obligor.Number = entities.Obligor.Number;
            }

            if (local.ProtCollCount.Count > 1)
            {
              break;
            }
          }

          if (local.ProtCollCount.Count == 0)
          {
          }
          else if (local.ProtCollCount.Count > 1)
          {
            ExitState = "FN000_MUST_UNPROT_MULT_COLLS";

            return;
          }
          else
          {
            ExitState = "FN0000_URAA_MUST_UNPROT_COLLS";

            return;
          }
        }
      }
      else if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'M')
      {
        // : Read for household, medical
        foreach(var item in ReadCollectionObligationObligationTypeCsePerson1())
        {
          if (entities.Obligation.SystemGeneratedIdentifier != export
            .ProtCollObligation.SystemGeneratedIdentifier || !
            Equal(entities.Obligor.Number, export.ProtCollObligor.Number) || entities
            .ObligationType.SystemGeneratedIdentifier != export
            .ProtCollObligationType.SystemGeneratedIdentifier)
          {
            // : Will use counter to set exitstate.  If more than one obligation
            // has
            //  protected collections, exitstate will be set to let the worker 
            // know
            //  that multiple obligations have protected collections.
            ++local.ProtCollCount.Count;
          }

          if (export.ProtCollObligation.SystemGeneratedIdentifier == 0)
          {
            export.ProtCollObligation.Assign(entities.Obligation);
            MoveObligationType(entities.ObligationType,
              export.ProtCollObligationType);
            export.ProtCollObligor.Number = entities.Obligor.Number;
            export.Obligor.Number = entities.Obligor.Number;
          }

          if (local.ProtCollCount.Count > 1)
          {
            break;
          }
        }

        if (local.ProtCollCount.Count == 0)
        {
        }
        else if (local.ProtCollCount.Count > 1)
        {
          ExitState = "FN000_MUST_UNPROT_MULT_COLLS";

          return;
        }
        else
        {
          ExitState = "FN0000_URAA_MUST_UNPROT_COLLS";

          return;
        }
      }
      else
      {
        // : Read for household, non-medical
        foreach(var item in ReadCollectionObligationObligationTypeCsePerson2())
        {
          if (entities.Obligation.SystemGeneratedIdentifier != export
            .ProtCollObligation.SystemGeneratedIdentifier || !
            Equal(entities.Obligor.Number, export.ProtCollObligor.Number) || entities
            .ObligationType.SystemGeneratedIdentifier != export
            .ProtCollObligationType.SystemGeneratedIdentifier)
          {
            // : Will use counter to set exitstate.  If more than one obligation
            // has
            //  protected collections, exitstate will be set to let the worker 
            // know
            //  that multiple obligations have protected collections.
            ++local.ProtCollCount.Count;
          }

          if (export.ProtCollObligation.SystemGeneratedIdentifier == 0)
          {
            export.ProtCollObligation.Assign(entities.Obligation);
            MoveObligationType(entities.ObligationType,
              export.ProtCollObligationType);
            export.ProtCollObligor.Number = entities.Obligor.Number;
            export.Obligor.Number = entities.Obligor.Number;
          }

          if (local.ProtCollCount.Count > 1)
          {
            break;
          }
        }

        if (local.ProtCollCount.Count == 0)
        {
        }
        else if (local.ProtCollCount.Count > 1)
        {
          ExitState = "FN000_MUST_UNPROT_MULT_COLLS";

          return;
        }
        else
        {
          ExitState = "FN0000_URAA_MUST_UNPROT_COLLS";

          return;
        }
      }
    }

    if (AsChar(import.ImHouseholdMbrMnthlyAdj.LevelAppliedTo) == 'M')
    {
      // ****  Member level  ****
      if (!ReadImHouseholdMbrMnthlySum1())
      {
        ExitState = "OE0000_IM_HH_MBR_MNTHLY_SUM_NF";

        return;
      }

      // *****  Sum up all the adjustments made for this person for this 
      // household for this month and year .  *****
      ReadImHouseholdMbrMnthlyAdj();

      // *****  Calc total adjustments for HH/mbr/yy/mm including the adj about 
      // to be made.  *****
      local.GrandTotAdj.TotalCurrency += import.ImHouseholdMbrMnthlyAdj.
        AdjustmentAmount;

      if (import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount < 0)
      {
        // *****  Edit check to ensure that the adjustment does not bring the 
        // grant + adj below 0.  *****
        if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          if (local.GrandTotAdj.TotalCurrency + entities
            .ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() < 0)
          {
            ExitState = "OE0000_URA_EXCEEDS_GRANT_AMT_RB";

            return;
          }
        }
        else if (local.GrandTotAdj.TotalCurrency + entities
          .ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault() < 0)
        {
          ExitState = "OE0000_URA_EXCEEDS_GRANT_AMT_RB";

          return;
        }
      }
      else
      {
        if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          if (Equal(entities.ImHouseholdMbrMnthlySum.GrantAmount, 0))
          {
            ExitState = "OE0000_ADJ_WITH_ZERO_GRANT_RB";

            return;
          }
        }
        else if (Equal(entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount, 0))
        {
          ExitState = "OE0000_ADJ_WITH_ZERO_MED_GRNT_RB";

          return;
        }

        // *****  Edit check to ensure that the total of all adjustments is not 
        // greater than the grant.  *****
        if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
        {
          if (import.DateWorkArea.Year == import.FirstAfGrant.Year && import
            .DateWorkArea.Month == import.FirstAfGrant.Month)
          {
            // *****  Skip the edit check if this is the 1st grant date.  *****
          }
          else if (local.GrandTotAdj.TotalCurrency > 0)
          {
            ExitState = "OE0000_ADJ_GREATER_THAN_GRANT_RB";

            return;
          }
        }
        else if (import.DateWorkArea.Year == import.FirstMedGrant.Year && import
          .DateWorkArea.Month == import.FirstMedGrant.Month)
        {
          // *****  Skip the edit check if this is the 1st med grant date.  
          // *****
        }
        else if (local.GrandTotAdj.TotalCurrency > 0)
        {
          ExitState = "OE0000_ADJ_GREATER_THAN_GRANT_RB";

          return;
        }
      }

      UseOeCreateUraAdjustment1();
    }
    else
    {
      // ****  Household level  ****
      ReadCsePerson2();

      // ****  The last CSE person in the household receives the amount left as 
      // a result of truncation. ****
      if (local.CountOfCsePersonsInHh.Count == 0)
      {
        ExitState = "OE0000_IM_HH_MBR_MNTHLY_SUM_NF";

        return;
      }
      else
      {
        local.MonthlyAdjustmentAmount.AdjustmentAmount =
          import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount / local
          .CountOfCsePersonsInHh.Count;
        local.LastPerson.AdjustmentAmount =
          import.ImHouseholdMbrMnthlyAdj.AdjustmentAmount - local
          .MonthlyAdjustmentAmount.AdjustmentAmount * (
            (long)local.CountOfCsePersonsInHh.Count - 1);
      }

      // ****  Read all the members for a IM household and their monthly sum for
      // a specific year and month  ****
      local.CountCsePersonRead.Count = 0;

      foreach(var item in ReadImHouseholdCsePersonImHouseholdMbrMnthlySum())
      {
        ++local.CountCsePersonRead.Count;

        if (local.CountCsePersonRead.Count == local.CountOfCsePersonsInHh.Count)
        {
          local.MonthlyAdjustmentAmount.AdjustmentAmount =
            local.LastPerson.AdjustmentAmount;
        }

        // *****  Sum up all the adjustments made for this person for this 
        // household for this month and year .  *****
        ReadImHouseholdMbrMnthlyAdj();

        // ****  Add the current adjustment amount as well.  ****
        local.GrandTotAdj.TotalCurrency += local.MonthlyAdjustmentAmount.
          AdjustmentAmount;

        if (local.MonthlyAdjustmentAmount.AdjustmentAmount < 0)
        {
          // *****  Edit check to ensure that the adjustment does not bring the 
          // grant + adj below 0.  *****
          if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
          {
            if (local.GrandTotAdj.TotalCurrency + entities
              .ImHouseholdMbrMnthlySum.GrantAmount.GetValueOrDefault() < 0)
            {
              ExitState = "OE0000_URA_EXCEEDS_GRANT_AMT_RB";

              return;
            }
          }
          else if (local.GrandTotAdj.TotalCurrency + entities
            .ImHouseholdMbrMnthlySum.GrantMedicalAmount.GetValueOrDefault() < 0
            )
          {
            ExitState = "OE0000_URA_EXCEEDS_GRANT_AMT_RB";

            return;
          }
        }
        else
        {
          if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
          {
            if (Equal(entities.ImHouseholdMbrMnthlySum.GrantAmount, 0))
            {
              ExitState = "OE0000_ADJ_WITH_ZERO_GRANT_RB";

              return;
            }
          }
          else if (Equal(entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount, 0))
            
          {
            ExitState = "OE0000_ADJ_WITH_ZERO_MED_GRNT_RB";

            return;
          }

          if (import.DateWorkArea.Year != import.FirstAfGrant.Year || import
            .DateWorkArea.Month != import.FirstAfGrant.Month)
          {
            // *****  Edit check to ensure that the total of all adjustments is 
            // not greater than the grant unless it is on the first benefit
            // date.  *****
            if (AsChar(import.ImHouseholdMbrMnthlyAdj.Type1) == 'A')
            {
              if (local.GrandTotAdj.TotalCurrency > 0)
              {
                ExitState = "OE0000_ADJ_GREATER_THAN_GRANT_RB";

                return;
              }
            }
            else if (local.GrandTotAdj.TotalCurrency > 0)
            {
              ExitState = "OE0000_ADJ_GREATER_THAN_GRANT_RB";

              return;
            }
          }
        }

        local.ForUpdate.Assign(import.ImHouseholdMbrMnthlyAdj);
        local.ForUpdate.AdjustmentAmount =
          local.MonthlyAdjustmentAmount.AdjustmentAmount;
        UseOeCreateUraAdjustment2();
      }
    }

    // ****  Set triggers  ****
    foreach(var item in ReadCsePersonAccount())
    {
      if (Lt(IntToDate(
        entities.ImHouseholdMbrMnthlySum.Year * 10000 + entities
        .ImHouseholdMbrMnthlySum.Month * 100
        + 1), entities.CsePersonAccount.PgmChgEffectiveDate) || Equal
        (entities.CsePersonAccount.PgmChgEffectiveDate, local.LowValue.Date))
      {
        try
        {
          UpdateCsePersonAccount();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.Relationship = source.Relationship;
    target.GrantAmount = source.GrantAmount;
    target.GrantMedicalAmount = source.GrantMedicalAmount;
    target.UraAmount = source.UraAmount;
    target.UraMedicalAmount = source.UraMedicalAmount;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseOeCreateUraAdjustment1()
  {
    var useImport = new OeCreateUraAdjustment.Import();
    var useExport = new OeCreateUraAdjustment.Export();

    useImport.Per.Assign(entities.ImHouseholdMbrMnthlySum);
    useImport.ImHouseholdMbrMnthlyAdj.Assign(import.ImHouseholdMbrMnthlyAdj);

    Call(OeCreateUraAdjustment.Execute, useImport, useExport);

    MoveImHouseholdMbrMnthlySum(useImport.Per, entities.ImHouseholdMbrMnthlySum);
      
  }

  private void UseOeCreateUraAdjustment2()
  {
    var useImport = new OeCreateUraAdjustment.Import();
    var useExport = new OeCreateUraAdjustment.Export();

    useImport.Per.Assign(entities.ImHouseholdMbrMnthlySum);
    useImport.ImHouseholdMbrMnthlyAdj.Assign(local.ForUpdate);

    Call(OeCreateUraAdjustment.Execute, useImport, useExport);

    MoveImHouseholdMbrMnthlySum(useImport.Per, entities.ImHouseholdMbrMnthlySum);
      
  }

  private IEnumerable<bool> ReadCollectionObligationObligationTypeCsePerson1()
  {
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadCollectionObligationObligationTypeCsePerson1",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId1", local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId2", local.HardcodeMj.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId3", local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId4", local.HardcodeUme.SystemGeneratedIdentifier);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "imsMonth", import.DateWorkArea.Month);
        db.SetInt32(command, "imsYear", import.DateWorkArea.Year);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.DistributionMethod = db.GetString(reader, 13);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 14);
        entities.Obligation.CpaType = db.GetString(reader, 15);
        entities.Obligation.CspNumber = db.GetString(reader, 16);
        entities.Obligor.Number = db.GetString(reader, 16);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 19);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 22);
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationObligationTypeCsePerson2()
  {
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadCollectionObligationObligationTypeCsePerson2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId1", local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId2", local.HardcodeMj.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId3", local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId4", local.HardcodeUme.SystemGeneratedIdentifier);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "imsYear", import.DateWorkArea.Year);
        db.SetInt32(command, "imsMonth", import.DateWorkArea.Month);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.DistributionMethod = db.GetString(reader, 13);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 14);
        entities.Obligation.CpaType = db.GetString(reader, 15);
        entities.Obligation.CspNumber = db.GetString(reader, 16);
        entities.Obligor.Number = db.GetString(reader, 16);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 19);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 22);
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationObligationTypeCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadCollectionObligationObligationTypeCsePerson3",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId1", local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId2", local.HardcodeMj.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId3", local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "debtTypId4", local.HardcodeUme.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber0", entities.ImHouseholdMbrMnthlySum.CspNumber);
        db.SetInt32(command, "imsYear", entities.ImHouseholdMbrMnthlySum.Year);
        db.
          SetInt32(command, "imsMonth", entities.ImHouseholdMbrMnthlySum.Month);
          
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.DistributionMethod = db.GetString(reader, 13);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 14);
        entities.Obligation.CpaType = db.GetString(reader, 15);
        entities.Obligation.CspNumber = db.GetString(reader, 16);
        entities.Obligor.Number = db.GetString(reader, 16);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 19);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 22);
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligor.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationObligationTypeCsePerson4()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadCollectionObligationObligationTypeCsePerson4",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMj.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeUme.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber0", entities.ImHouseholdMbrMnthlySum.CspNumber);
        db.SetInt32(command, "imsYear", entities.ImHouseholdMbrMnthlySum.Year);
        db.
          SetInt32(command, "imsMonth", entities.ImHouseholdMbrMnthlySum.Month);
          
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.DistributionMethod = db.GetString(reader, 13);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 14);
        entities.Obligation.CpaType = db.GetString(reader, 15);
        entities.Obligation.CspNumber = db.GetString(reader, 16);
        entities.Obligor.Number = db.GetString(reader, 16);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 19);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 22);
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligor.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", import.DateWorkArea.Year);
        db.SetInt32(command, "month0", import.DateWorkArea.Month);
      },
      (db, reader) =>
      {
        local.CountOfCsePersonsInHh.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return ReadEach("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", import.DateWorkArea.Year);
        db.SetInt32(command, "month0", import.DateWorkArea.Month);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.CsePersonAccount.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.CsePersonAccount.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.CsePersonAccount.TriggerType = db.GetNullableString(reader, 5);
        entities.CsePersonAccount.Populated = true;

        return true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdCsePersonImHouseholdMbrMnthlySum()
  {
    entities.ImHousehold.Populated = false;
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadImHouseholdCsePersonImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", import.DateWorkArea.Year);
        db.SetInt32(command, "month0", import.DateWorkArea.Month);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 2);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 3);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ImHousehold.Populated = true;
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMbrMnthlyAdj()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    return Read("ReadImHouseholdMbrMnthlyAdj",
      (db, command) =>
      {
        db.SetInt32(command, "imsYear", entities.ImHouseholdMbrMnthlySum.Year);
        db.
          SetInt32(command, "imsMonth", entities.ImHouseholdMbrMnthlySum.Month);
          
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
        db.SetString(command, "type", import.ImHouseholdMbrMnthlyAdj.Type1);
      },
      (db, reader) =>
      {
        local.GrandTotAdj.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadImHouseholdMbrMnthlySum1()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", import.DateWorkArea.Year);
        db.SetInt32(command, "month0", import.DateWorkArea.Month);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 10);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum2()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum2",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year0", import.DateWorkArea.Year);
        db.SetInt32(command, "month0", import.DateWorkArea.Month);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.GrantAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 9);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 10);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate =
      IntToDate(entities.ImHouseholdMbrMnthlySum.Year * 10000 + entities
      .ImHouseholdMbrMnthlySum.Month * 100 + 1);
    var triggerType = "U";

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", triggerType);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(command, "type", entities.CsePersonAccount.Type1);
      });

    entities.CsePersonAccount.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAccount.LastUpdatedTmst = lastUpdatedTmst;
    entities.CsePersonAccount.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.CsePersonAccount.TriggerType = triggerType;
    entities.CsePersonAccount.Populated = true;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
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
    /// A value of FirstAfGrant.
    /// </summary>
    [JsonPropertyName("firstAfGrant")]
    public DateWorkArea FirstAfGrant
    {
      get => firstAfGrant ??= new();
      set => firstAfGrant = value;
    }

    /// <summary>
    /// A value of FirstMedGrant.
    /// </summary>
    [JsonPropertyName("firstMedGrant")]
    public DateWorkArea FirstMedGrant
    {
      get => firstMedGrant ??= new();
      set => firstMedGrant = value;
    }

    private ImHousehold imHousehold;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private DateWorkArea dateWorkArea;
    private DateWorkArea firstAfGrant;
    private DateWorkArea firstMedGrant;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ProtCollObligor.
    /// </summary>
    [JsonPropertyName("protCollObligor")]
    public CsePerson ProtCollObligor
    {
      get => protCollObligor ??= new();
      set => protCollObligor = value;
    }

    /// <summary>
    /// A value of ProtCollObligationType.
    /// </summary>
    [JsonPropertyName("protCollObligationType")]
    public ObligationType ProtCollObligationType
    {
      get => protCollObligationType ??= new();
      set => protCollObligationType = value;
    }

    /// <summary>
    /// A value of ProtCollObligation.
    /// </summary>
    [JsonPropertyName("protCollObligation")]
    public Obligation ProtCollObligation
    {
      get => protCollObligation ??= new();
      set => protCollObligation = value;
    }

    private CsePersonsWorkSet obligor;
    private CsePerson protCollObligor;
    private ObligationType protCollObligationType;
    private Obligation protCollObligation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotMbrAfAdj.
    /// </summary>
    [JsonPropertyName("totMbrAfAdj")]
    public Common TotMbrAfAdj
    {
      get => totMbrAfAdj ??= new();
      set => totMbrAfAdj = value;
    }

    /// <summary>
    /// A value of TotMbrMedAdj.
    /// </summary>
    [JsonPropertyName("totMbrMedAdj")]
    public Common TotMbrMedAdj
    {
      get => totMbrMedAdj ??= new();
      set => totMbrMedAdj = value;
    }

    /// <summary>
    /// A value of GrandTotAdj.
    /// </summary>
    [JsonPropertyName("grandTotAdj")]
    public Common GrandTotAdj
    {
      get => grandTotAdj ??= new();
      set => grandTotAdj = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public ImHouseholdMbrMnthlyAdj ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of LowValue.
    /// </summary>
    [JsonPropertyName("lowValue")]
    public DateWorkArea LowValue
    {
      get => lowValue ??= new();
      set => lowValue = value;
    }

    /// <summary>
    /// A value of CountCsePersonRead.
    /// </summary>
    [JsonPropertyName("countCsePersonRead")]
    public Common CountCsePersonRead
    {
      get => countCsePersonRead ??= new();
      set => countCsePersonRead = value;
    }

    /// <summary>
    /// A value of LastPerson.
    /// </summary>
    [JsonPropertyName("lastPerson")]
    public ImHouseholdMbrMnthlyAdj LastPerson
    {
      get => lastPerson ??= new();
      set => lastPerson = value;
    }

    /// <summary>
    /// A value of MonthlyAdjustmentAmount.
    /// </summary>
    [JsonPropertyName("monthlyAdjustmentAmount")]
    public ImHouseholdMbrMnthlyAdj MonthlyAdjustmentAmount
    {
      get => monthlyAdjustmentAmount ??= new();
      set => monthlyAdjustmentAmount = value;
    }

    /// <summary>
    /// A value of CountOfCsePersonsInHh.
    /// </summary>
    [JsonPropertyName("countOfCsePersonsInHh")]
    public Common CountOfCsePersonsInHh
    {
      get => countOfCsePersonsInHh ??= new();
      set => countOfCsePersonsInHh = value;
    }

    /// <summary>
    /// A value of HardcodeMj.
    /// </summary>
    [JsonPropertyName("hardcodeMj")]
    public ObligationType HardcodeMj
    {
      get => hardcodeMj ??= new();
      set => hardcodeMj = value;
    }

    /// <summary>
    /// A value of HardcodeMs.
    /// </summary>
    [JsonPropertyName("hardcodeMs")]
    public ObligationType HardcodeMs
    {
      get => hardcodeMs ??= new();
      set => hardcodeMs = value;
    }

    /// <summary>
    /// A value of HardcodeMc.
    /// </summary>
    [JsonPropertyName("hardcodeMc")]
    public ObligationType HardcodeMc
    {
      get => hardcodeMc ??= new();
      set => hardcodeMc = value;
    }

    /// <summary>
    /// A value of HardcodeUme.
    /// </summary>
    [JsonPropertyName("hardcodeUme")]
    public ObligationType HardcodeUme
    {
      get => hardcodeUme ??= new();
      set => hardcodeUme = value;
    }

    /// <summary>
    /// A value of ProtCollCount.
    /// </summary>
    [JsonPropertyName("protCollCount")]
    public Common ProtCollCount
    {
      get => protCollCount ??= new();
      set => protCollCount = value;
    }

    private Common totMbrAfAdj;
    private Common totMbrMedAdj;
    private Common grandTotAdj;
    private ImHouseholdMbrMnthlyAdj forUpdate;
    private DateWorkArea lowValue;
    private Common countCsePersonRead;
    private ImHouseholdMbrMnthlyAdj lastPerson;
    private ImHouseholdMbrMnthlyAdj monthlyAdjustmentAmount;
    private Common countOfCsePersonsInHh;
    private ObligationType hardcodeMj;
    private ObligationType hardcodeMs;
    private ObligationType hardcodeMc;
    private ObligationType hardcodeUme;
    private Common protCollCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private UraCollectionApplication uraCollectionApplication;
    private Collection collection;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson obligor;
  }
#endregion
}
