// Program: FN_DISP_DISB_SUPPRESSIONS_VER2, ID: 373554332, model: 746.
// Short name: SWE00443
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
/// A program: FN_DISP_DISB_SUPPRESSIONS_VER2.
/// </para>
/// <para>
/// RESP: FINANCE
/// This cab will be used to display a group of disbursements for a payee. It 
/// will summarize all disbursment transactions with the same reference number
/// and Disb Type code into one record with the total amount shown.
/// </para>
/// </summary>
[Serializable]
public partial class FnDispDisbSuppressionsVer2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISP_DISB_SUPPRESSIONS_VER2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDispDisbSuppressionsVer2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDispDisbSuppressionsVer2.
  /// </summary>
  public FnDispDisbSuppressionsVer2(IContext context, Import import,
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
    // ----------------------------------------------------
    // Significant changes were required to this CAB. A new
    // version was created by XCopying code from old CAB.
    // ----------------------------------------------------
    // ----------------------------------------------------------------------------
    // 4/10/2000 - PR #92709
    // Read credit disb_tran to determine if debit is URA or Passthru.
    // ---------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // 5/22/2000 - PRWORA Work order # 164-L
    // Flag URA disbursements with an 'X' at the end of disb type.
    // ---------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // 10/17/2000  Fangman  PR 98039  Duplicate Payments
    // Set new export flag for Duplicate Payment suppression.
    // ---------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // 02/22/2001  Fangman  PR 111602  Add 2 additional filters (ending date & 
    // reference number).
    // ---------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // 06/21/2001   V.Madhira     PR# 121266
    // In case of Passthro's the 'disb_tran' system_gen_ID is needed in PSTEP to
    // RELEASE (PF15) it. In Case of RLSE the READ is qualified with '
    // disb_tran' sys_gen_id and this CAB is not passing that value but '
    // Reference_number'. So change the code (add sys_gen_id to
    // Loc_prev_disb_tran view  and MOVE the value to grp_exp view.)
    // -----------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // 05/22/2002  Fangman  PR 147157 Changed logic to no longer skip NAI & AFI 
    // because interstate disbursements can be suppressed.
    // ---------------------------------------------------------------------------
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    // -------------------------------------------------------------------------------
    // 09/04/07  G. Pan   PR197953
    // Changed group view from 50 to 120 to matched import and export
    // from PStep FN_LDSP_LST_MTN_DISB_SUPPR
    // -------------------------------------------------------------------------------
    // GV
    // ------------------------------------------------------------------------------------------------------------
    // 07/02/19  GVandy	CQ65423		Add Suppression Type filter.
    // 					Did some restructuring of this cab and added lots of notes.
    // --------------------------------------------------------------------------------------------------------------
    local.CurrentDate.Date = Now().Date;
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ****************************************************************
    // Modified the Read Each to look for records without a PROCESSED value in 
    // the Reason Text field. It use to go by a Discontinue Date of 12/31/2099(
    // aka active).
    // Also changed the Read to sort by Reference number first and to also to 
    // sort by Disb Code.The Disb Code was necessary to ensure that only one
    // record per Disb Code(per Ref#) was actually seen. It wasn't combining
    // them correctly.
    // ****************************************************************
    export.Export1.Index = -1;
    local.FirstTime.Flag = "Y";

    // GV
    // ------------------------------------------------------------------------------------------------------------
    // Read each suppression sorted by reference number (i.e. cash receipt 
    // detail id),
    // disbursement type, disbursement suppression created timestamp, and excess
    // ura ind.
    // --------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadDisbursementStatusHistoryDisbursementStatus2())
    {
      // GV
      // ------------------------------------------------------------------------------------------------------------
      // Apply reference number filter.
      // --------------------------------------------------------------------------------------------------------------
      if (!IsEmpty(import.SearchRefNbr.ReferenceNumber) && !
        Equal(import.SearchRefNbr.ReferenceNumber,
        entities.DisbursementTransaction.ReferenceNumber))
      {
        continue;
      }

      // GV
      // ------------------------------------------------------------------------------------------------------------
      // This is validating that the disbursement originated from either a non 
      // AF collection, pass thru, or excess ura.
      // --------------------------------------------------------------------------------------------------------------
      // *****************************************************************
      // 4/10/2000 - PR #92709  Read credit disb_tran.
      // *****************************************************************
      if (!ReadDisbursementTransaction())
      {
        ExitState = "FN0000_COLLECTION_NF";

        return;
      }

      local.ThisIsAPassthruOrUra.Flag = "N";

      // *****************************************************************
      // If this is a Passthru or URA then no collection is attached, so bypass.
      // *****************************************************************
      if (AsChar(entities.Credit.Type1) == 'X' || AsChar
        (entities.Credit.Type1) == 'P')
      {
        local.ThisIsAPassthruOrUra.Flag = "Y";
      }
      else
      {
        if (!ReadCollection())
        {
          ExitState = "FN0000_COLLECTION_NF";

          return;
        }

        // ****************************************************************
        // If the Program Applied To code is equal to anything but NA, skip to 
        // the next disbursement. The reason is you can't suppress any but NA.
        // ****************************************************************
        if (Equal(entities.Collection.ProgramAppliedTo, "AF"))
        {
          continue;
        }
      }

      // GV
      // ------------------------------------------------------------------------------------------------------------
      // Below all the criteria is established to group and sum up the 
      // disbursements for display purposes.
      // --------------------------------------------------------------------------------------------------------------
      // --Establish the release date.  If the disbursement is in SUPPressed 
      // status then it is the end date of the suppression.  If the disbursement
      // is in RELEASEd status then it is the effective date of the release.
      if (Equal(entities.DisbursementStatus.Code, "RELEASE"))
      {
        local.ReleaseDate.EffectiveDate =
          entities.DisbursementStatusHistory.EffectiveDate;
      }
      else
      {
        // ****************************************************************
        // Status code must be SUPPRESSED. Set Release Date to scheduled 
        // discontinue_date of Suppression record.
        // ****************************************************************
        if (!Equal(entities.DisbursementStatusHistory.DiscontinueDate,
          local.MaxDate.Date))
        {
          local.ReleaseDate.EffectiveDate =
            entities.DisbursementStatusHistory.DiscontinueDate;
        }
        else
        {
          local.ReleaseDate.EffectiveDate = null;
        }
      }

      // --Establish the disbursement type.  Excess URA is denoted with an "X" 
      // at the end of the disbursement type.
      if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y')
      {
        local.DisbType.Text10 = TrimEnd(entities.DisbursementType.Code) + " X";
      }
      else
      {
        local.DisbType.Text10 = entities.DisbursementType.Code;
      }

      // --Establish the date the suppression or release was created.
      local.ConvertDshTimestamp.Date =
        Date(entities.DisbursementStatusHistory.CreatedTimestamp);

      // GV
      // ------------------------------------------------------------------------------------------------------------
      // Roll up disbursements with same Reference #, Disb Type, Suppression 
      // create date. and Suppression release date.
      // Do not roll up positive and negative amounts together. URA and non-URA 
      // disb will not
      // be rolled up together.
      // When any of these grouping criteria change then move the summed amounts
      // to the export group.
      // --------------------------------------------------------------------------------------------------------------
      if (!Equal(local.DisbType.Text10, local.PrevDisbType.Text10) || !
        Equal(local.PrevDisbursementTransaction.ReferenceNumber,
        entities.DisbursementTransaction.ReferenceNumber) || !
        Equal(local.ConvertDshTimestamp.Date, local.PrevConvertDshTimestamp.Date)
        || !
        Equal(local.ReleaseDate.EffectiveDate,
        local.PrevDisbursementStatusHistory.EffectiveDate) || AsChar
        (local.ThisIsAPassthruOrUra.Flag) == 'Y')
      {
        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          local.FirstTime.Flag = "N";

          goto Test1;
        }

        // GV
        // ------------------------------------------------------------------------------------------------------------
        // Apply Suppression Type filter.
        // --------------------------------------------------------------------------------------------------------------
        local.SuppTypeFilterPassed.Flag = "Y";

        if (!IsEmpty(import.Search.Type1))
        {
          switch(AsChar(import.Search.Type1))
          {
            case 'A':
              if (IsEmpty(local.PrevAutoSupp.Text1))
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'C':
              if (IsEmpty(local.PrevCollSupp.Code))
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'D':
              if (IsEmpty(local.PrevDupPmtSupp.Text1))
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'O':
              if (AsChar(local.PrevPersonSupp.Text1) != 'O')
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'P':
              if (AsChar(local.PrevPersonSupp.Text1) != 'P')
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'X':
              // @@@@ Add excess ura suppression views, local, export, pstep, 
              // screen, etc...
              if (Equal(local.PrevDisbType.Text10,
                Length(TrimEnd(local.PrevDisbType.Text10)) - 1, 2, " X"))
              {
              }
              else
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'Y':
              if (IsEmpty(local.PrevDodSupp.Text1))
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case 'Z':
              if (IsEmpty(local.PrevAddrSupp.Text1))
              {
                local.SuppTypeFilterPassed.Flag = "N";
              }

              break;
            case ' ':
              break;
            default:
              break;
          }
        }

        if (AsChar(local.SuppTypeFilterPassed.Flag) == 'Y')
        {
          // GV
          // ------------------------------------------------------------------------------------------------------------
          // Move positive and negative amounts to the export group seperately 
          // if their values are non zero.
          // --------------------------------------------------------------------------------------------------------------
          if (local.Positive.Amount > 0)
          {
            ++export.Export1.Index;
            export.Export1.CheckSize();

            if (export.Export1.Index >= Export.ExportGroup.Capacity)
            {
              return;
            }

            export.Export1.Update.DisbursementTransaction.Amount =
              local.Positive.Amount;
            export.Export1.Update.DisbursementTransaction.ReferenceNumber =
              local.PrevDisbursementTransaction.ReferenceNumber ?? "";

            // ------------------------------------------------------------------------------
            // 06/21/2001   V.Madhira     PR# 121266
            // Below SET statement is  added to fix the PR.
            // -----------------------------------------------------------------------------
            export.Export1.Update.DisbursementTransaction.
              SystemGeneratedIdentifier =
                local.PrevDisbursementTransaction.SystemGeneratedIdentifier;
            export.Export1.Update.DisbType.Text10 = local.PrevDisbType.Text10;
            export.Export1.Update.DisbursementType.Code =
              local.PrevDisbursementType.Code;
            MoveDisbursementStatus(local.PrevDisbursementStatus,
              export.Export1.Update.DisbursementStatus);
            MoveDisbursementStatusHistory(local.PrevDisbursementStatusHistory,
              export.Export1.Update.DisbursementStatusHistory);
            export.Export1.Update.PersonSupp.Text1 = local.PrevPersonSupp.Text1;
            export.Export1.Update.CollSupp.Code = local.PrevCollSupp.Code;
            export.Export1.Update.AutoSupp.Text1 = local.PrevAutoSupp.Text1;
            export.Export1.Update.DupPmtSupp.Text1 = local.PrevDupPmtSupp.Text1;
            export.Export1.Update.DisbSuppressionStatusHistory.LastUpdatedBy =
              local.PrevDisbSuppressionStatusHistory.LastUpdatedBy;
            export.Export1.Update.AddrSupp.Text1 = local.PrevAddrSupp.Text1;
            export.Export1.Update.DodSupp.Text1 = local.PrevDodSupp.Text1;
            export.Export1.Update.SuppTypes.Text6 =
              export.Export1.Item.PersonSupp.Text1 + Substring
              (export.Export1.Item.CollSupp.Code, CollectionType.Code_MaxLength,
              1, 1) + export.Export1.Item.AutoSupp.Text1 + export
              .Export1.Item.DupPmtSupp.Text1 + export
              .Export1.Item.AddrSupp.Text1 + export.Export1.Item.DodSupp.Text1;
          }

          if (local.Negative.Amount < 0)
          {
            ++export.Export1.Index;
            export.Export1.CheckSize();

            if (export.Export1.Index >= Export.ExportGroup.Capacity)
            {
              return;
            }

            export.Export1.Update.DisbursementTransaction.Amount =
              local.Negative.Amount;
            export.Export1.Update.DisbursementTransaction.ReferenceNumber =
              local.PrevDisbursementTransaction.ReferenceNumber ?? "";

            // ------------------------------------------------------------------------------
            // 06/21/2001   V.Madhira     PR# 121266
            // Below SET statement is  added to fix the PR.
            // -----------------------------------------------------------------------------
            export.Export1.Update.DisbursementTransaction.
              SystemGeneratedIdentifier =
                local.PrevDisbursementTransaction.SystemGeneratedIdentifier;
            export.Export1.Update.DisbType.Text10 = local.PrevDisbType.Text10;
            export.Export1.Update.DisbursementType.Code =
              local.PrevDisbursementType.Code;
            MoveDisbursementStatus(local.PrevDisbursementStatus,
              export.Export1.Update.DisbursementStatus);
            MoveDisbursementStatusHistory(local.PrevDisbursementStatusHistory,
              export.Export1.Update.DisbursementStatusHistory);
            export.Export1.Update.PersonSupp.Text1 = local.PrevPersonSupp.Text1;
            export.Export1.Update.CollSupp.Code = local.PrevCollSupp.Code;
            export.Export1.Update.AutoSupp.Text1 = local.PrevAutoSupp.Text1;
            export.Export1.Update.DupPmtSupp.Text1 = local.PrevDupPmtSupp.Text1;
            export.Export1.Update.DisbSuppressionStatusHistory.LastUpdatedBy =
              local.PrevDisbSuppressionStatusHistory.LastUpdatedBy;
            export.Export1.Update.AddrSupp.Text1 = local.PrevAddrSupp.Text1;
            export.Export1.Update.DodSupp.Text1 = local.PrevDodSupp.Text1;
            export.Export1.Update.SuppTypes.Text6 =
              export.Export1.Item.PersonSupp.Text1 + Substring
              (export.Export1.Item.CollSupp.Code, CollectionType.Code_MaxLength,
              1, 1) + export.Export1.Item.AutoSupp.Text1 + export
              .Export1.Item.DupPmtSupp.Text1 + export
              .Export1.Item.AddrSupp.Text1 + export.Export1.Item.DodSupp.Text1;
          }
        }

        // GV
        // ------------------------------------------------------------------------------------------------------------
        // Re-initialize the local views for the next grouping of disbursements.
        // --------------------------------------------------------------------------------------------------------------
        local.Positive.Amount = 0;
        local.Negative.Amount = 0;
        local.PrevPersonSupp.Text1 = "";
        local.PrevCollSupp.Code = "";
        local.PrevAutoSupp.Text1 = "";
        local.PrevDupPmtSupp.Text1 = "";
        local.PrevAddrSupp.Text1 = "";
        local.PrevDodSupp.Text1 = "";
        local.PrevDisbSuppressionStatusHistory.LastUpdatedBy = "";
      }

Test1:

      // GV
      // ------------------------------------------------------------------------------------------------------------
      // Populate the local_prev views.
      // --------------------------------------------------------------------------------------------------------------
      if (entities.DisbursementTransaction.Amount > 0)
      {
        local.Positive.Amount += entities.DisbursementTransaction.Amount;
      }
      else
      {
        local.Negative.Amount += entities.DisbursementTransaction.Amount;
      }

      local.PrevConvertDshTimestamp.Date = local.ConvertDshTimestamp.Date;
      local.PrevDisbursementTransaction.
        Assign(entities.DisbursementTransaction);
      local.PrevDisbType.Text10 = local.DisbType.Text10;
      local.PrevDisbursementType.Code = entities.DisbursementType.Code;
      MoveDisbursementStatus(entities.DisbursementStatus,
        local.PrevDisbursementStatus);
      local.PrevDisbursementStatusHistory.CreatedTimestamp =
        entities.DisbursementStatusHistory.CreatedTimestamp;
      local.PrevDisbursementStatusHistory.EffectiveDate =
        local.ReleaseDate.EffectiveDate;
      local.PrevDisbursementStatusHistory.SuppressionReason =
        entities.DisbursementStatusHistory.SuppressionReason;

      // ****************************************************************
      // Replace Created Dte of 'Released' record with actual suppression date. 
      // Set by B651.
      // ****************************************************************
      local.DisbursementStatusHistory.SuppressionReason = "";

      if (Equal(entities.DisbursementStatus.Code, "RELEASE"))
      {
        if (ReadDisbursementStatusHistoryDisbursementStatus1())
        {
          local.PrevDisbursementStatusHistory.CreatedTimestamp =
            entities.ReleasedDisbursementStatusHistory.CreatedTimestamp;
          local.DisbursementStatusHistory.SuppressionReason =
            entities.ReleasedDisbursementStatusHistory.SuppressionReason;
        }
      }
      else if (Equal(entities.DisbursementStatus.Code, "SUPP"))
      {
        local.DisbursementStatusHistory.SuppressionReason =
          entities.DisbursementStatusHistory.SuppressionReason;
      }

      // *************************
      // Get suppression type.
      // *************************
      // ****************************************************************
      // The intital setting up of the Suppression is done by SWEFB651, so its 
      // value is in the 'created by' field on DSH. If 651 is found then use the
      // persons id from DSSH:last updated or created by field. Else use the ID
      // of whoever has updated the suppression from Suppressed to Released and
      // /or back again on DSH.
      // ****************************************************************
      if (ReadDisbSuppressionStatusHistory())
      {
        if (Equal(entities.DisbursementStatusHistory.CreatedBy, "SWEFB651"))
        {
          if (IsEmpty(entities.DisbSuppressionStatusHistory.LastUpdatedBy))
          {
            local.PrevDisbSuppressionStatusHistory.LastUpdatedBy =
              entities.DisbSuppressionStatusHistory.CreatedBy;
          }
          else
          {
            local.PrevDisbSuppressionStatusHistory.LastUpdatedBy =
              entities.DisbSuppressionStatusHistory.LastUpdatedBy;
          }
        }
        else
        {
          local.PrevDisbSuppressionStatusHistory.LastUpdatedBy =
            entities.DisbursementStatusHistory.CreatedBy;
        }
      }

      if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y' && IsEmpty
        (local.PrevDisbSuppressionStatusHistory.LastUpdatedBy))
      {
        // For X URA suppression types we do not have to find a matching 
        // suppression rule.
        local.PrevDisbSuppressionStatusHistory.LastUpdatedBy = "SWEFB651";
      }

      if (IsEmpty(local.PrevDisbSuppressionStatusHistory.LastUpdatedBy))
      {
        local.PrevDisbSuppressionStatusHistory.LastUpdatedBy =
          entities.DisbursementStatusHistory.CreatedBy;
      }

      switch(AsChar(local.DisbursementStatusHistory.SuppressionReason))
      {
        case 'A':
          if (AsChar(entities.Collection.AppliedToCode) == 'C' && Equal
            (entities.Collection.ProgramAppliedTo, "NA") && AsChar
            (entities.Collection.AppliedToFuture) == 'Y' && AsChar
            (entities.Collection.AdjustedInd) != 'Y')
          {
            local.PrevAutoSupp.Text1 = "A";
          }

          break;
        case 'C':
          if (AsChar(local.ThisIsAPassthruOrUra.Flag) != 'Y')
          {
            if (ReadCollectionType())
            {
              local.PrevCollSupp.Code = entities.CollectionType.Code;
            }
            else
            {
              // ---------
              // Ok
              // ---------
            }
          }

          break;
        case 'P':
          local.PrevPersonSupp.Text1 = "P";

          break;
        case 'D':
          if (ReadDisbursementStatusHistory())
          {
            local.PrevDupPmtSupp.Text1 = "D";
          }
          else
          {
            // Continue
          }

          // *****  changes for WR 040796
          break;
        case 'O':
          local.PrevPersonSupp.Text1 = "O";

          if (ReadCollectionType())
          {
            local.PrevCollSupp.Code = entities.CollectionType.Code;
          }
          else
          {
            // ---------
            // Ok
            // ---------
          }

          break;
        case 'X':
          break;
        case 'Y':
          local.PrevDodSupp.Text1 = "Y";

          break;
        case 'Z':
          local.PrevAddrSupp.Text1 = "Z";

          break;
        default:
          break;
      }

      // *****  changes for WR 040796
    }

    // GV
    // ------------------------------------------------------------------------------------------------------------
    // Move the last group of summed amounts to the export group.
    // --------------------------------------------------------------------------------------------------------------
    if (AsChar(local.FirstTime.Flag) != 'Y')
    {
      // GV
      // ------------------------------------------------------------------------------------------------------------
      // Apply Suppression Type filter.
      // --------------------------------------------------------------------------------------------------------------
      if (!IsEmpty(import.Search.Type1))
      {
        switch(AsChar(import.Search.Type1))
        {
          case 'A':
            if (IsEmpty(local.PrevAutoSupp.Text1))
            {
              goto Test2;
            }

            break;
          case 'C':
            if (IsEmpty(local.PrevCollSupp.Code))
            {
              goto Test2;
            }

            break;
          case 'D':
            if (IsEmpty(local.PrevDupPmtSupp.Text1))
            {
              goto Test2;
            }

            break;
          case 'O':
            if (AsChar(local.PrevPersonSupp.Text1) != 'O')
            {
              goto Test2;
            }

            break;
          case 'P':
            if (AsChar(local.PrevPersonSupp.Text1) != 'P')
            {
              goto Test2;
            }

            break;
          case 'X':
            if (Equal(local.PrevDisbType.Text10,
              Length(TrimEnd(local.PrevDisbType.Text10)) - 1, 2, " X"))
            {
            }
            else
            {
              goto Test2;
            }

            break;
          case 'Y':
            if (IsEmpty(local.PrevDodSupp.Text1))
            {
              goto Test2;
            }

            break;
          case 'Z':
            if (IsEmpty(local.PrevAddrSupp.Text1))
            {
              goto Test2;
            }

            break;
          case ' ':
            break;
          default:
            break;
        }
      }

      // GV
      // ------------------------------------------------------------------------------------------------------------
      // Move positive and negative amounts to the export group seperately if 
      // their values are non zero.
      // --------------------------------------------------------------------------------------------------------------
      if (local.Positive.Amount > 0)
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          return;
        }

        export.Export1.Update.DisbursementTransaction.Amount =
          local.Positive.Amount;
        export.Export1.Update.DisbursementTransaction.ReferenceNumber =
          local.PrevDisbursementTransaction.ReferenceNumber ?? "";

        // ------------------------------------------------------------------------------
        // 06/21/2001   V.Madhira     PR# 121266
        // Below SET statement is  added to fix the PR.
        // -----------------------------------------------------------------------------
        export.Export1.Update.DisbursementTransaction.
          SystemGeneratedIdentifier =
            local.PrevDisbursementTransaction.SystemGeneratedIdentifier;
        export.Export1.Update.DisbType.Text10 = local.PrevDisbType.Text10;
        export.Export1.Update.DisbursementType.Code =
          local.PrevDisbursementType.Code;
        MoveDisbursementStatus(local.PrevDisbursementStatus,
          export.Export1.Update.DisbursementStatus);
        MoveDisbursementStatusHistory(local.PrevDisbursementStatusHistory,
          export.Export1.Update.DisbursementStatusHistory);
        export.Export1.Update.PersonSupp.Text1 = local.PrevPersonSupp.Text1;
        export.Export1.Update.CollSupp.Code = local.PrevCollSupp.Code;
        export.Export1.Update.AutoSupp.Text1 = local.PrevAutoSupp.Text1;
        export.Export1.Update.DupPmtSupp.Text1 = local.PrevDupPmtSupp.Text1;
        export.Export1.Update.DisbSuppressionStatusHistory.LastUpdatedBy =
          local.PrevDisbSuppressionStatusHistory.LastUpdatedBy;
        export.Export1.Update.AddrSupp.Text1 = local.PrevAddrSupp.Text1;
        export.Export1.Update.DodSupp.Text1 = local.PrevDodSupp.Text1;
        export.Export1.Update.SuppTypes.Text6 =
          export.Export1.Item.PersonSupp.Text1 + Substring
          (export.Export1.Item.CollSupp.Code, CollectionType.Code_MaxLength, 1,
          1) + export.Export1.Item.AutoSupp.Text1 + export
          .Export1.Item.DupPmtSupp.Text1 + export
          .Export1.Item.AddrSupp.Text1 + export.Export1.Item.DodSupp.Text1;
      }

      if (local.Negative.Amount < 0)
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          return;
        }

        export.Export1.Update.DisbursementTransaction.Amount =
          local.Negative.Amount;
        export.Export1.Update.DisbursementTransaction.ReferenceNumber =
          local.PrevDisbursementTransaction.ReferenceNumber ?? "";

        // ------------------------------------------------------------------------------
        // 06/21/2001   V.Madhira     PR# 121266
        // Below SET statement is  added to fix the PR.
        // -----------------------------------------------------------------------------
        export.Export1.Update.DisbursementTransaction.
          SystemGeneratedIdentifier =
            local.PrevDisbursementTransaction.SystemGeneratedIdentifier;
        export.Export1.Update.DisbType.Text10 = local.PrevDisbType.Text10;
        export.Export1.Update.DisbursementType.Code =
          local.PrevDisbursementType.Code;
        MoveDisbursementStatus(local.PrevDisbursementStatus,
          export.Export1.Update.DisbursementStatus);
        MoveDisbursementStatusHistory(local.PrevDisbursementStatusHistory,
          export.Export1.Update.DisbursementStatusHistory);
        export.Export1.Update.PersonSupp.Text1 = local.PrevPersonSupp.Text1;
        export.Export1.Update.CollSupp.Code = local.PrevCollSupp.Code;
        export.Export1.Update.AutoSupp.Text1 = local.PrevAutoSupp.Text1;
        export.Export1.Update.DupPmtSupp.Text1 = local.PrevDupPmtSupp.Text1;
        export.Export1.Update.DisbSuppressionStatusHistory.LastUpdatedBy =
          local.PrevDisbSuppressionStatusHistory.LastUpdatedBy;
        export.Export1.Update.AddrSupp.Text1 = local.PrevAddrSupp.Text1;
        export.Export1.Update.DodSupp.Text1 = local.PrevDodSupp.Text1;
        export.Export1.Update.SuppTypes.Text6 =
          export.Export1.Item.PersonSupp.Text1 + Substring
          (export.Export1.Item.CollSupp.Code, CollectionType.Code_MaxLength, 1,
          1) + export.Export1.Item.AutoSupp.Text1 + export
          .Export1.Item.DupPmtSupp.Text1 + export
          .Export1.Item.AddrSupp.Text1 + export.Export1.Item.DodSupp.Text1;
      }
    }

Test2:

    if (export.Export1.Index < 0)
    {
      ExitState = "FN0000_NO_RECORDS_FOUND";
    }
  }

  private static void MoveDisbursementStatus(DisbursementStatus source,
    DisbursementStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveDisbursementStatusHistory(
    DisbursementStatusHistory source, DisbursementStatusHistory target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.SuppressionReason = source.SuppressionReason;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.
          SetInt32(command, "collId", entities.Credit.ColId.GetValueOrDefault());
          
        db.
          SetInt32(command, "otyId", entities.Credit.OtyId.GetValueOrDefault());
          
        db.
          SetInt32(command, "obgId", entities.Credit.ObgId.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.Credit.CspNumberDisb ?? "");
        db.SetString(command, "cpaType", entities.Credit.CpaTypeDisb ?? "");
        db.
          SetInt32(command, "otrId", entities.Credit.OtrId.GetValueOrDefault());
          
        db.SetString(command, "otrType", entities.Credit.OtrTypeDisb ?? "");
        db.SetInt32(
          command, "crtType", entities.Credit.CrtId.GetValueOrDefault());
        db.
          SetInt32(command, "cstId", entities.Credit.CstId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crvId", entities.Credit.CrvId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
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
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 13);
        entities.Collection.AppliedToFuture = db.GetString(reader, 14);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(
          command, "type",
          local.DisbursementStatusHistory.SuppressionReason ?? "");
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.LookForDSuppression.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
      },
      (db, reader) =>
      {
        entities.LookForDSuppression.DbsGeneratedId = db.GetInt32(reader, 0);
        entities.LookForDSuppression.DtrGeneratedId = db.GetInt32(reader, 1);
        entities.LookForDSuppression.CspNumber = db.GetString(reader, 2);
        entities.LookForDSuppression.CpaType = db.GetString(reader, 3);
        entities.LookForDSuppression.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.LookForDSuppression.SuppressionReason =
          db.GetNullableString(reader, 5);
        entities.LookForDSuppression.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.LookForDSuppression.CpaType);
      });
  }

  private bool ReadDisbursementStatusHistoryDisbursementStatus1()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.ReleasedDisbursementStatusHistory.Populated = false;
    entities.ReleasedDisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatusHistoryDisbursementStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReleasedDisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.ReleasedDisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReleasedDisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.ReleasedDisbursementStatusHistory.CspNumber =
          db.GetString(reader, 2);
        entities.ReleasedDisbursementStatusHistory.CpaType =
          db.GetString(reader, 3);
        entities.ReleasedDisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ReleasedDisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ReleasedDisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ReleasedDisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ReleasedDisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 8);
        entities.ReleasedDisbursementStatus.Code = db.GetString(reader, 9);
        entities.ReleasedDisbursementStatus.CreatedBy =
          db.GetString(reader, 10);
        entities.ReleasedDisbursementStatusHistory.Populated = true;
        entities.ReleasedDisbursementStatus.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.ReleasedDisbursementStatusHistory.CpaType);
      });
  }

  private IEnumerable<bool> ReadDisbursementStatusHistoryDisbursementStatus2()
  {
    entities.DisbursementTransaction.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementStatus.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadDisbursementStatusHistoryDisbursementStatus2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "processDate1",
          import.SearchStarting.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate2",
          import.SearchEnding.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "disbStatusId",
          import.HardcodeProcessed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 10);
        entities.DisbursementStatus.Code = db.GetString(reader, 11);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 12);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 13);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 14);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 16);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 18);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 19);
        entities.DisbursementType.Code = db.GetString(reader, 20);
        entities.DisbursementTransaction.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementStatus.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.Amount = db.GetDecimal(reader, 4);
        entities.Credit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Credit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Credit.DisbursementDate = db.GetNullableDate(reader, 7);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 8);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 9);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 10);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 11);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 12);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 13);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 14);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 15);
        entities.Credit.CstId = db.GetNullableInt32(reader, 16);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 17);
        entities.Credit.ColId = db.GetNullableInt32(reader, 18);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
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
    /// A value of SearchStarting.
    /// </summary>
    [JsonPropertyName("searchStarting")]
    public DisbursementTransaction SearchStarting
    {
      get => searchStarting ??= new();
      set => searchStarting = value;
    }

    /// <summary>
    /// A value of SearchEnding.
    /// </summary>
    [JsonPropertyName("searchEnding")]
    public DisbursementTransaction SearchEnding
    {
      get => searchEnding ??= new();
      set => searchEnding = value;
    }

    /// <summary>
    /// A value of SearchRefNbr.
    /// </summary>
    [JsonPropertyName("searchRefNbr")]
    public DisbursementTransaction SearchRefNbr
    {
      get => searchRefNbr ??= new();
      set => searchRefNbr = value;
    }

    /// <summary>
    /// A value of ImportedHardcodeSuppressed.
    /// </summary>
    [JsonPropertyName("importedHardcodeSuppressed")]
    public DisbursementStatus ImportedHardcodeSuppressed
    {
      get => importedHardcodeSuppressed ??= new();
      set => importedHardcodeSuppressed = value;
    }

    /// <summary>
    /// A value of HardcodeProcessed.
    /// </summary>
    [JsonPropertyName("hardcodeProcessed")]
    public DisbursementStatus HardcodeProcessed
    {
      get => hardcodeProcessed ??= new();
      set => hardcodeProcessed = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public DisbSuppressionStatusHistory Search
    {
      get => search ??= new();
      set => search = value;
    }

    private CsePerson csePerson;
    private DisbursementTransaction searchStarting;
    private DisbursementTransaction searchEnding;
    private DisbursementTransaction searchRefNbr;
    private DisbursementStatus importedHardcodeSuppressed;
    private DisbursementStatus hardcodeProcessed;
    private DisbSuppressionStatusHistory search;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
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
      /// A value of DisbursementStatusHistory.
      /// </summary>
      [JsonPropertyName("disbursementStatusHistory")]
      public DisbursementStatusHistory DisbursementStatusHistory
      {
        get => disbursementStatusHistory ??= new();
        set => disbursementStatusHistory = value;
      }

      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DisbursementStatus.
      /// </summary>
      [JsonPropertyName("disbursementStatus")]
      public DisbursementStatus DisbursementStatus
      {
        get => disbursementStatus ??= new();
        set => disbursementStatus = value;
      }

      /// <summary>
      /// A value of DisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbursementTransaction")]
      public DisbursementTransaction DisbursementTransaction
      {
        get => disbursementTransaction ??= new();
        set => disbursementTransaction = value;
      }

      /// <summary>
      /// A value of PersonSupp.
      /// </summary>
      [JsonPropertyName("personSupp")]
      public TextWorkArea PersonSupp
      {
        get => personSupp ??= new();
        set => personSupp = value;
      }

      /// <summary>
      /// A value of CollSupp.
      /// </summary>
      [JsonPropertyName("collSupp")]
      public CollectionType CollSupp
      {
        get => collSupp ??= new();
        set => collSupp = value;
      }

      /// <summary>
      /// A value of AutoSupp.
      /// </summary>
      [JsonPropertyName("autoSupp")]
      public TextWorkArea AutoSupp
      {
        get => autoSupp ??= new();
        set => autoSupp = value;
      }

      /// <summary>
      /// A value of DupPmtSupp.
      /// </summary>
      [JsonPropertyName("dupPmtSupp")]
      public TextWorkArea DupPmtSupp
      {
        get => dupPmtSupp ??= new();
        set => dupPmtSupp = value;
      }

      /// <summary>
      /// A value of DisbSuppressionStatusHistory.
      /// </summary>
      [JsonPropertyName("disbSuppressionStatusHistory")]
      public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
      {
        get => disbSuppressionStatusHistory ??= new();
        set => disbSuppressionStatusHistory = value;
      }

      /// <summary>
      /// A value of DisbursementType.
      /// </summary>
      [JsonPropertyName("disbursementType")]
      public DisbursementType DisbursementType
      {
        get => disbursementType ??= new();
        set => disbursementType = value;
      }

      /// <summary>
      /// A value of AddrSupp.
      /// </summary>
      [JsonPropertyName("addrSupp")]
      public TextWorkArea AddrSupp
      {
        get => addrSupp ??= new();
        set => addrSupp = value;
      }

      /// <summary>
      /// A value of DodSupp.
      /// </summary>
      [JsonPropertyName("dodSupp")]
      public TextWorkArea DodSupp
      {
        get => dodSupp ??= new();
        set => dodSupp = value;
      }

      /// <summary>
      /// A value of SuppTypes.
      /// </summary>
      [JsonPropertyName("suppTypes")]
      public WorkArea SuppTypes
      {
        get => suppTypes ??= new();
        set => suppTypes = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common common;
      private DisbursementStatusHistory disbursementStatusHistory;
      private TextWorkArea disbType;
      private DisbursementStatus disbursementStatus;
      private DisbursementTransaction disbursementTransaction;
      private TextWorkArea personSupp;
      private CollectionType collSupp;
      private TextWorkArea autoSupp;
      private TextWorkArea dupPmtSupp;
      private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
      private DisbursementType disbursementType;
      private TextWorkArea addrSupp;
      private TextWorkArea dodSupp;
      private WorkArea suppTypes;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SuppTypeFilterPassed.
    /// </summary>
    [JsonPropertyName("suppTypeFilterPassed")]
    public Common SuppTypeFilterPassed
    {
      get => suppTypeFilterPassed ??= new();
      set => suppTypeFilterPassed = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of ThisIsAPassthruOrUra.
    /// </summary>
    [JsonPropertyName("thisIsAPassthruOrUra")]
    public Common ThisIsAPassthruOrUra
    {
      get => thisIsAPassthruOrUra ??= new();
      set => thisIsAPassthruOrUra = value;
    }

    /// <summary>
    /// A value of ReleaseDate.
    /// </summary>
    [JsonPropertyName("releaseDate")]
    public DisbursementStatusHistory ReleaseDate
    {
      get => releaseDate ??= new();
      set => releaseDate = value;
    }

    /// <summary>
    /// A value of DisbType.
    /// </summary>
    [JsonPropertyName("disbType")]
    public TextWorkArea DisbType
    {
      get => disbType ??= new();
      set => disbType = value;
    }

    /// <summary>
    /// A value of ConvertDshTimestamp.
    /// </summary>
    [JsonPropertyName("convertDshTimestamp")]
    public DateWorkArea ConvertDshTimestamp
    {
      get => convertDshTimestamp ??= new();
      set => convertDshTimestamp = value;
    }

    /// <summary>
    /// A value of Positive.
    /// </summary>
    [JsonPropertyName("positive")]
    public DisbursementTransaction Positive
    {
      get => positive ??= new();
      set => positive = value;
    }

    /// <summary>
    /// A value of Negative.
    /// </summary>
    [JsonPropertyName("negative")]
    public DisbursementTransaction Negative
    {
      get => negative ??= new();
      set => negative = value;
    }

    /// <summary>
    /// A value of PrevDisbType.
    /// </summary>
    [JsonPropertyName("prevDisbType")]
    public TextWorkArea PrevDisbType
    {
      get => prevDisbType ??= new();
      set => prevDisbType = value;
    }

    /// <summary>
    /// A value of PrevDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("prevDisbursementTransaction")]
    public DisbursementTransaction PrevDisbursementTransaction
    {
      get => prevDisbursementTransaction ??= new();
      set => prevDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PrevConvertDshTimestamp.
    /// </summary>
    [JsonPropertyName("prevConvertDshTimestamp")]
    public DateWorkArea PrevConvertDshTimestamp
    {
      get => prevConvertDshTimestamp ??= new();
      set => prevConvertDshTimestamp = value;
    }

    /// <summary>
    /// A value of PrevDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("prevDisbursementStatusHistory")]
    public DisbursementStatusHistory PrevDisbursementStatusHistory
    {
      get => prevDisbursementStatusHistory ??= new();
      set => prevDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of PrevDisbursementType.
    /// </summary>
    [JsonPropertyName("prevDisbursementType")]
    public DisbursementType PrevDisbursementType
    {
      get => prevDisbursementType ??= new();
      set => prevDisbursementType = value;
    }

    /// <summary>
    /// A value of PrevDisbursementStatus.
    /// </summary>
    [JsonPropertyName("prevDisbursementStatus")]
    public DisbursementStatus PrevDisbursementStatus
    {
      get => prevDisbursementStatus ??= new();
      set => prevDisbursementStatus = value;
    }

    /// <summary>
    /// A value of PrevPersonSupp.
    /// </summary>
    [JsonPropertyName("prevPersonSupp")]
    public TextWorkArea PrevPersonSupp
    {
      get => prevPersonSupp ??= new();
      set => prevPersonSupp = value;
    }

    /// <summary>
    /// A value of PrevCollSupp.
    /// </summary>
    [JsonPropertyName("prevCollSupp")]
    public CollectionType PrevCollSupp
    {
      get => prevCollSupp ??= new();
      set => prevCollSupp = value;
    }

    /// <summary>
    /// A value of PrevAutoSupp.
    /// </summary>
    [JsonPropertyName("prevAutoSupp")]
    public TextWorkArea PrevAutoSupp
    {
      get => prevAutoSupp ??= new();
      set => prevAutoSupp = value;
    }

    /// <summary>
    /// A value of PrevDupPmtSupp.
    /// </summary>
    [JsonPropertyName("prevDupPmtSupp")]
    public TextWorkArea PrevDupPmtSupp
    {
      get => prevDupPmtSupp ??= new();
      set => prevDupPmtSupp = value;
    }

    /// <summary>
    /// A value of PrevDisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("prevDisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory PrevDisbSuppressionStatusHistory
    {
      get => prevDisbSuppressionStatusHistory ??= new();
      set => prevDisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of PrevAddrSupp.
    /// </summary>
    [JsonPropertyName("prevAddrSupp")]
    public TextWorkArea PrevAddrSupp
    {
      get => prevAddrSupp ??= new();
      set => prevAddrSupp = value;
    }

    /// <summary>
    /// A value of PrevDodSupp.
    /// </summary>
    [JsonPropertyName("prevDodSupp")]
    public TextWorkArea PrevDodSupp
    {
      get => prevDodSupp ??= new();
      set => prevDodSupp = value;
    }

    /// <summary>
    /// A value of DsshFound.
    /// </summary>
    [JsonPropertyName("dsshFound")]
    public Common DsshFound
    {
      get => dsshFound ??= new();
      set => dsshFound = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    private Common suppTypeFilterPassed;
    private DateWorkArea currentDate;
    private DateWorkArea maxDate;
    private Common firstTime;
    private Common thisIsAPassthruOrUra;
    private DisbursementStatusHistory releaseDate;
    private TextWorkArea disbType;
    private DateWorkArea convertDshTimestamp;
    private DisbursementTransaction positive;
    private DisbursementTransaction negative;
    private TextWorkArea prevDisbType;
    private DisbursementTransaction prevDisbursementTransaction;
    private DateWorkArea prevConvertDshTimestamp;
    private DisbursementStatusHistory prevDisbursementStatusHistory;
    private DisbursementType prevDisbursementType;
    private DisbursementStatus prevDisbursementStatus;
    private TextWorkArea prevPersonSupp;
    private CollectionType prevCollSupp;
    private TextWorkArea prevAutoSupp;
    private TextWorkArea prevDupPmtSupp;
    private DisbSuppressionStatusHistory prevDisbSuppressionStatusHistory;
    private TextWorkArea prevAddrSupp;
    private TextWorkArea prevDodSupp;
    private Common dsshFound;
    private DisbursementStatusHistory disbursementStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of ReleasedDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("releasedDisbursementStatusHistory")]
    public DisbursementStatusHistory ReleasedDisbursementStatusHistory
    {
      get => releasedDisbursementStatusHistory ??= new();
      set => releasedDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of ReleasedDisbursementStatus.
    /// </summary>
    [JsonPropertyName("releasedDisbursementStatus")]
    public DisbursementStatus ReleasedDisbursementStatus
    {
      get => releasedDisbursementStatus ??= new();
      set => releasedDisbursementStatus = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of LookForDSuppression.
    /// </summary>
    [JsonPropertyName("lookForDSuppression")]
    public DisbursementStatusHistory LookForDSuppression
    {
      get => lookForDSuppression ??= new();
      set => lookForDSuppression = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private CsePerson csePerson;
    private DisbursementTransaction disbursementTransaction;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransaction credit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private Collection collection;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
    private DisbursementType disbursementType;
    private DisbursementStatusHistory releasedDisbursementStatusHistory;
    private DisbursementStatus releasedDisbursementStatus;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private DisbursementStatusHistory lookForDSuppression;
    private LegalAction legalAction;
  }
#endregion
}
