// Program: FN_CRDP_LST_DEPOSITS, ID: 371768983, model: 746.
// Short name: SWECRDPP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CRDP_LST_DEPOSITS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This screen will provide a list of all deposits.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrdpLstDeposits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRDP_LST_DEPOSITS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrdpLstDeposits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrdpLstDeposits.
  /// </summary>
  public FnCrdpLstDeposits(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    // Every initial development and change to that development needs
    // to be documented below.
    // ---------------------------------------------------------------------------
    // Date 	    Developer Name	Description
    // ---------   ----------------- 	------------------
    // 02/20/96    Holly Kennedy-MTW	Retrofits
    // 04/04/96    Holly Kennedy-MTW	Logic passing a field in the
    // 				Group view was passing import View.
    // 				The actual logic was targeting the
    // 				Export view.
    // 				Data on the screen was not matched
    // 				properly causing an import value to be
    // 				cleared out with each iteration.
    // 12/03/96    R. Marchman		Add new security and next tran
    // 03/05/97    A Samuels		Test Support
    // 06/18/97    M. D. Wheaton       Deleted datenum
    // 10/19/98    J. Katz		Remove extraneous views.
    // 				Remove RETURN logic.
    // 				Add validation logic for prompt
    // 				selection and status code entry.
    // 				Remove Worker ID from the
    // 				filter values on DISPLAY.
    // 				Correct logic used to populate
    // 				deposit list to remove
    // 				duplicates from list.
    // 10/27/98  J. Katz		Default CLOSED status filter to a
    // 				one-month date range.		
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.FirstTimeProcessed.Flag = import.FirstTimeProcessed.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // -----------------------------------------------------------------
    // Next Tran Logic
    // ----------------
    // If the next tran info is greater than spaces, the user requested
    // a next tran action.   Validate the requested action.
    // -----------------------------------------------------------------
    MoveStandard(import.Standard, export.Standard);

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    // -----------------------------------------------------------------
    // Validate Security
    // -----------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -----------------------------------------------------------------
    // If this program is accessed via CAMM, no import data exists.
    // No additional processing required.
    // -----------------------------------------------------------------
    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // -----------------------------------------------------------------
    // Move import views to export views.
    // -----------------------------------------------------------------
    export.Desired.Code = import.Desired.Code;
    export.From.BusinessDate = import.From.BusinessDate;
    export.To.BusinessDate = import.To.BusinessDate;

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.New1.Index = 0;
      export.New1.Clear();

      for(import.New1.Index = 0; import.New1.Index < import.New1.Count; ++
        import.New1.Index)
      {
        if (export.New1.IsFull)
        {
          break;
        }

        export.New1.Update.NewExportGroupMemberCommon.SelectChar =
          import.New1.Item.NewImportGroupMemberCommon.SelectChar;
        export.New1.Update.NewExportGroupMemberFundTransaction.Assign(
          import.New1.Item.NewImportGroupMemberFundTransaction);
        MoveFundTransactionStatus(import.New1.Item.
          NewImportGroupMemberFundTransactionStatus,
          export.New1.Update.NewExportGroupMemberFundTransactionStatus);
        MoveProgramCostAccount(import.New1.Item.NewImportGroupMemberHidden,
          export.New1.Update.NewExportGroupMemberHidden);
        MoveFundTransactionStatusHistory(import.New1.Item.
          NewImportGroupMemberFundTransactionStatusHistory,
          export.New1.Update.NewExportGroupMemberFundTransactionStatusHistory);
        export.New1.Update.NewExportGroupNoOfItems.Count =
          import.New1.Item.NewImportGroupNoOfItems.Count;
        export.New1.Next();
      }
    }

    // -----------------------------------------------------------------
    // Populate Local Views.
    // -----------------------------------------------------------------
    local.Low.Date = new DateTime(1, 1, 1);
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    UseHardcodedFundingInformation();
    export.Fund.Assign(local.HardcodedClearing);

    // -----------------------------------------------------------------
    // Set up for Display on return from LFTS List Fund Transaction Status.
    //   *  If a value was selected, move that value to the screen.
    //   *  If the selected status is CLOSED, default date range.
    //   *  Set the command to DISPLAY
    // -----------------------------------------------------------------
    if (Equal(global.Command, "RETLINK"))
    {
      export.Standard.PromptField = "+";

      if (!IsEmpty(import.HiddenSelected.Code))
      {
        export.Desired.Code = import.HiddenSelected.Code;
      }

      if (Equal(export.Desired.Code, "CLOSED"))
      {
        export.From.BusinessDate = Now().Date.AddMonths(-1);
      }
      else
      {
        export.From.BusinessDate = local.Null1.Date;
      }

      global.Command = "DISPLAY";
    }

    // -----------------------------------------------------------------
    // 		Main Case of Command
    // There is no existing link from another procedure to CRDP.
    // Therefore, the RETURN command is invlaid and was removed
    // from the logic.    JLK  10/22/98
    // -----------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ------------------------------------------------------------
        // This will display a list of deposits with a given Status.
        // If a Status is not entered, and this is the first time to
        // process this tran, then all 'open' deposits sorted in
        // desending deposit number will be listed.
        // If no status is entered, but this is not the first time to
        // process, then all deposits will be displayed regardless of
        // the active status.
        // ------------------------------------------------------------
        if (IsEmpty(export.Desired.Code))
        {
          if (IsEmpty(export.FirstTimeProcessed.Flag))
          {
            export.Desired.Code = "OPEN";
            export.FirstTimeProcessed.Flag = "N";
          }
        }

        // -------------------------------------------------------
        // Validate Status Code to use for filter, if entered.
        // -------------------------------------------------------
        if (!IsEmpty(export.Desired.Code))
        {
          if (!ReadFundTransactionStatus())
          {
            var field = GetField(export.Desired, "code");

            field.Error = true;

            ExitState = "FN0000_FUND_TRANS_STAT_NF";

            return;
          }
        }

        // ---------------------------------------------------------
        // Search dates are only allowed for CLOSED status.
        //   If TO date is not entered, default to high date.
        //   FROM date must precede TO date.
        // ---------------------------------------------------------
        if (Equal(export.Desired.Code, "CLOSED"))
        {
          if (Equal(export.To.BusinessDate, local.Null1.Date))
          {
            export.To.BusinessDate = local.Maximum.Date;
          }

          if (Lt(export.To.BusinessDate, export.From.BusinessDate))
          {
            var field1 = GetField(export.From, "businessDate");

            field1.Error = true;

            var field2 = GetField(export.To, "businessDate");

            field2.Error = true;

            ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

            return;
          }
        }
        else
        {
          if (Lt(local.Null1.Date, export.To.BusinessDate))
          {
            var field = GetField(export.To, "businessDate");

            field.Error = true;

            ExitState = "FN0000_DATES_ONLY_ON_CLOSED";
          }

          if (Lt(local.Null1.Date, export.From.BusinessDate))
          {
            var field = GetField(export.From, "businessDate");

            field.Error = true;

            ExitState = "FN0000_DATES_ONLY_ON_CLOSED";
          }

          if (IsExitState("FN0000_DATES_ONLY_ON_CLOSED"))
          {
            return;
          }

          export.To.BusinessDate = local.Maximum.Date;
        }

        // -------------------------------------------------------
        // Populate list of Deposits based on filter criteria.
        // -------------------------------------------------------
        if (!ReadFundTransactionType())
        {
          ExitState = "FN0000_FUND_TRANS_TYPE_NF";

          return;
        }

        local.CountGroup.Count = 0;

        export.New1.Index = 0;
        export.New1.Clear();

        foreach(var item in ReadFundTransaction())
        {
          // -------------------------------------------
          // Find active Fund Transaction Status.
          // -------------------------------------------
          ReadFundTransactionStatusHistoryFundTransactionStatus();

          // ----------------------------------------------------------
          // Skip Deposit record if a status filter was used and the
          // record is not the appropriate status value.
          // ----------------------------------------------------------
          if (!IsEmpty(export.Desired.Code) && !
            Equal(entities.FundTransactionStatus.Code, export.Desired.Code))
          {
            export.New1.Next();

            continue;
          }

          if (ReadProgramCostAccount())
          {
            // OK, CONTINUE
          }

          // --------------------------------------------------------------------
          // Determine number of items (cash receipts) within each
          // deposit number.
          // --------------------------------------------------------------------
          ReadCashReceipt();

          // -------------------------------------------------------
          // Move entity action views to NEW group export views.
          // -------------------------------------------------------
          export.New1.Update.NewExportGroupMemberCommon.SelectChar = "";
          export.New1.Update.NewExportGroupMemberFundTransaction.Assign(
            entities.FundTransaction);
          MoveFundTransactionStatus(entities.FundTransactionStatus,
            export.New1.Update.NewExportGroupMemberFundTransactionStatus);
          MoveFundTransactionStatusHistory(entities.
            FundTransactionStatusHistory,
            export.New1.Update.
              NewExportGroupMemberFundTransactionStatusHistory);
          MoveProgramCostAccount(entities.ProgramCostAccount,
            export.New1.Update.NewExportGroupMemberHidden);

          // TEST PDP 11/07/99   added count to keep group from filling up
          ++local.CountGroup.Count;
          export.New1.Next();
        }

        // ----------------------------------------------------------
        // Set informational messages and clean up display.
        // ----------------------------------------------------------
        if (export.New1.IsEmpty)
        {
          ExitState = "FN0000_NO_RECORDS_FOUND";
        }
        else if (export.New1.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        if (Equal(export.From.BusinessDate, local.Low.Date) || Equal
          (export.From.BusinessDate, local.Maximum.Date))
        {
          export.From.BusinessDate = local.Null1.Date;
        }

        if (Equal(export.To.BusinessDate, local.Low.Date) || Equal
          (export.To.BusinessDate, local.Maximum.Date))
        {
          export.To.BusinessDate = local.Null1.Date;
        }

        break;
      case "LIST":
        // --------------------------------------------------
        // PF4 List
        // Initiate flow to LDPS List Deposit Status Codes.
        // --------------------------------------------------
        switch(AsChar(export.Standard.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_LST_FUND_TRANS_STATS";

            break;
          case ' ':
            var field1 = GetField(export.Standard, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          default:
            var field2 = GetField(export.Standard, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "DETAIL":
        // ---------------------------------------------------------
        // PF18  CRDI
        // Link to procedure CRDI - List Deposit items for detail
        // listing of receipts included in selected deposit.
        // ---------------------------------------------------------
        for(export.New1.Index = 0; export.New1.Index < export.New1.Count; ++
          export.New1.Index)
        {
          switch(AsChar(export.New1.Item.NewExportGroupMemberCommon.SelectChar))
          {
            case '*':
              export.New1.Update.NewExportGroupMemberCommon.SelectChar = "";

              break;
            case 'S':
              ++local.Select.Count;

              if (local.Select.Count > 1)
              {
                var field1 =
                  GetField(export.New1.Item.NewExportGroupMemberCommon,
                  "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              export.SelectFundTransaction.Assign(
                export.New1.Item.NewExportGroupMemberFundTransaction);
              MoveFundTransactionStatus(export.New1.Item.
                NewExportGroupMemberFundTransactionStatus,
                export.SelectFundTransactionStatus);

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.New1.Item.NewExportGroupMemberCommon,
                "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        if (local.Select.Count == 1)
        {
          ExitState = "ECO_LNK_TO_DEPOSIT_ITEMS";
        }
        else
        {
          var field = GetField(export.Desired, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }

        break;
      case "EXIT":
        // ---------------------------------------------------------
        // PF3  Exit
        // ---------------------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        // ---------------------------------------------------------
        // PF7  Prev
        // ---------------------------------------------------------
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        // ---------------------------------------------------------
        // PF8  Next
        // ---------------------------------------------------------
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "SIGNOFF":
        // ---------------------------------------------------------
        // PF12  Signoff
        // ---------------------------------------------------------
        UseScCabSignoff();

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveFundTransactionStatus(FundTransactionStatus source,
    FundTransactionStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveFundTransactionStatusHistory(
    FundTransactionStatusHistory source, FundTransactionStatusHistory target)
  {
    target.EffectiveTmst = source.EffectiveTmst;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveProgramCostAccount(ProgramCostAccount source,
    ProgramCostAccount target)
  {
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseHardcodedFundingInformation()
  {
    var useImport = new HardcodedFundingInformation.Import();
    var useExport = new HardcodedFundingInformation.Export();

    Call(HardcodedFundingInformation.Execute, useImport, useExport);

    local.HardcodedFttDeposit.SystemGeneratedIdentifier =
      useExport.Deposit.SystemGeneratedIdentifier;
    local.HardcodedClearing.SystemGeneratedIdentifier =
      useExport.ClearingFund.SystemGeneratedIdentifier;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        export.New1.Update.NewExportGroupNoOfItems.Count =
          db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadFundTransaction()
  {
    return ReadEach("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "fttIdentifier",
          entities.FundTransactionType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "funIdentifier",
          local.HardcodedClearing.SystemGeneratedIdentifier);
        db.SetDate(
          command, "businessDate1",
          export.From.BusinessDate.GetValueOrDefault());
        db.SetDate(
          command, "businessDate2", export.To.BusinessDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        if (export.New1.IsFull)
        {
          return false;
        }

        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.DepositNumber = db.GetNullableInt32(reader, 5);
        entities.FundTransaction.Amount = db.GetDecimal(reader, 6);
        entities.FundTransaction.BusinessDate = db.GetDate(reader, 7);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.FundTransaction.Populated = true;

        return true;
      });
  }

  private bool ReadFundTransactionStatus()
  {
    entities.FundTransactionStatus.Populated = false;

    return Read("ReadFundTransactionStatus",
      (db, command) =>
      {
        db.SetString(command, "code", export.Desired.Code);
      },
      (db, reader) =>
      {
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatus.Code = db.GetString(reader, 1);
        entities.FundTransactionStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.FundTransactionStatus.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.FundTransactionStatus.Populated = true;
      });
  }

  private bool ReadFundTransactionStatusHistoryFundTransactionStatus()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.FundTransactionStatusHistory.Populated = false;
    entities.FundTransactionStatus.Populated = false;

    return Read("ReadFundTransactionStatusHistoryFundTransactionStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        entities.FundTransactionStatusHistory.FtrIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FunIdentifier =
          db.GetInt32(reader, 1);
        entities.FundTransactionStatusHistory.PcaEffectiveDate =
          db.GetDate(reader, 2);
        entities.FundTransactionStatusHistory.PcaCode = db.GetString(reader, 3);
        entities.FundTransactionStatusHistory.FttIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransactionStatusHistory.FtsIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatusHistory.EffectiveTmst =
          db.GetDateTime(reader, 6);
        entities.FundTransactionStatusHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.FundTransactionStatus.Code = db.GetString(reader, 8);
        entities.FundTransactionStatus.EffectiveDate = db.GetDate(reader, 9);
        entities.FundTransactionStatus.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.FundTransactionStatusHistory.Populated = true;
        entities.FundTransactionStatus.Populated = true;
      });
  }

  private bool ReadFundTransactionType()
  {
    entities.FundTransactionType.Populated = false;

    return Read("ReadFundTransactionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransTypeId",
          local.HardcodedFttDeposit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FundTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionType.Code = db.GetString(reader, 1);
        entities.FundTransactionType.Name = db.GetString(reader, 2);
        entities.FundTransactionType.EffectiveDate = db.GetDate(reader, 3);
        entities.FundTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.FundTransactionType.Populated = true;
      });
  }

  private bool ReadProgramCostAccount()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.ProgramCostAccount.Populated = false;

    return Read("ReadProgramCostAccount",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "code", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        entities.ProgramCostAccount.Code = db.GetString(reader, 0);
        entities.ProgramCostAccount.EffectiveDate = db.GetDate(reader, 1);
        entities.ProgramCostAccount.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramCostAccount.Populated = true;
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
    /// <summary>A NewGroup group.</summary>
    [Serializable]
    public class NewGroup
    {
      /// <summary>
      /// A value of NewImportGroupMemberCommon.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberCommon")]
      public Common NewImportGroupMemberCommon
      {
        get => newImportGroupMemberCommon ??= new();
        set => newImportGroupMemberCommon = value;
      }

      /// <summary>
      /// A value of NewImportGroupNoOfItems.
      /// </summary>
      [JsonPropertyName("newImportGroupNoOfItems")]
      public Common NewImportGroupNoOfItems
      {
        get => newImportGroupNoOfItems ??= new();
        set => newImportGroupNoOfItems = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberFundTransaction.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberFundTransaction")]
      public FundTransaction NewImportGroupMemberFundTransaction
      {
        get => newImportGroupMemberFundTransaction ??= new();
        set => newImportGroupMemberFundTransaction = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberFundTransactionStatus.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberFundTransactionStatus")]
      public FundTransactionStatus NewImportGroupMemberFundTransactionStatus
      {
        get => newImportGroupMemberFundTransactionStatus ??= new();
        set => newImportGroupMemberFundTransactionStatus = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberFundTransactionStatusHistory.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberFundTransactionStatusHistory")]
      public FundTransactionStatusHistory NewImportGroupMemberFundTransactionStatusHistory
        
      {
        get => newImportGroupMemberFundTransactionStatusHistory ??= new();
        set => newImportGroupMemberFundTransactionStatusHistory = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberHidden.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberHidden")]
      public ProgramCostAccount NewImportGroupMemberHidden
      {
        get => newImportGroupMemberHidden ??= new();
        set => newImportGroupMemberHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common newImportGroupMemberCommon;
      private Common newImportGroupNoOfItems;
      private FundTransaction newImportGroupMemberFundTransaction;
      private FundTransactionStatus newImportGroupMemberFundTransactionStatus;
      private FundTransactionStatusHistory newImportGroupMemberFundTransactionStatusHistory;
        
      private ProgramCostAccount newImportGroupMemberHidden;
    }

    /// <summary>
    /// A value of Desired.
    /// </summary>
    [JsonPropertyName("desired")]
    public FundTransactionStatus Desired
    {
      get => desired ??= new();
      set => desired = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public FundTransaction From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public FundTransaction To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// Gets a value of New1.
    /// </summary>
    [JsonIgnore]
    public Array<NewGroup> New1 => new1 ??= new(NewGroup.Capacity);

    /// <summary>
    /// Gets a value of New1 for json serialization.
    /// </summary>
    [JsonPropertyName("new1")]
    [Computed]
    public IList<NewGroup> New1_Json
    {
      get => new1;
      set => New1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of FirstTimeProcessed.
    /// </summary>
    [JsonPropertyName("firstTimeProcessed")]
    public Common FirstTimeProcessed
    {
      get => firstTimeProcessed ??= new();
      set => firstTimeProcessed = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public FundTransactionStatus HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    private FundTransactionStatus desired;
    private FundTransaction from;
    private FundTransaction to;
    private Array<NewGroup> new1;
    private NextTranInfo hidden;
    private Standard standard;
    private Common firstTimeProcessed;
    private FundTransactionStatus hiddenSelected;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NewGroup group.</summary>
    [Serializable]
    public class NewGroup
    {
      /// <summary>
      /// A value of NewExportGroupMemberCommon.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberCommon")]
      public Common NewExportGroupMemberCommon
      {
        get => newExportGroupMemberCommon ??= new();
        set => newExportGroupMemberCommon = value;
      }

      /// <summary>
      /// A value of NewExportGroupNoOfItems.
      /// </summary>
      [JsonPropertyName("newExportGroupNoOfItems")]
      public Common NewExportGroupNoOfItems
      {
        get => newExportGroupNoOfItems ??= new();
        set => newExportGroupNoOfItems = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberFundTransaction.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberFundTransaction")]
      public FundTransaction NewExportGroupMemberFundTransaction
      {
        get => newExportGroupMemberFundTransaction ??= new();
        set => newExportGroupMemberFundTransaction = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberFundTransactionStatus.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberFundTransactionStatus")]
      public FundTransactionStatus NewExportGroupMemberFundTransactionStatus
      {
        get => newExportGroupMemberFundTransactionStatus ??= new();
        set => newExportGroupMemberFundTransactionStatus = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberFundTransactionStatusHistory.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberFundTransactionStatusHistory")]
      public FundTransactionStatusHistory NewExportGroupMemberFundTransactionStatusHistory
        
      {
        get => newExportGroupMemberFundTransactionStatusHistory ??= new();
        set => newExportGroupMemberFundTransactionStatusHistory = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberHidden.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberHidden")]
      public ProgramCostAccount NewExportGroupMemberHidden
      {
        get => newExportGroupMemberHidden ??= new();
        set => newExportGroupMemberHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common newExportGroupMemberCommon;
      private Common newExportGroupNoOfItems;
      private FundTransaction newExportGroupMemberFundTransaction;
      private FundTransactionStatus newExportGroupMemberFundTransactionStatus;
      private FundTransactionStatusHistory newExportGroupMemberFundTransactionStatusHistory;
        
      private ProgramCostAccount newExportGroupMemberHidden;
    }

    /// <summary>
    /// A value of SelectFundTransaction.
    /// </summary>
    [JsonPropertyName("selectFundTransaction")]
    public FundTransaction SelectFundTransaction
    {
      get => selectFundTransaction ??= new();
      set => selectFundTransaction = value;
    }

    /// <summary>
    /// A value of SelectFundTransactionStatus.
    /// </summary>
    [JsonPropertyName("selectFundTransactionStatus")]
    public FundTransactionStatus SelectFundTransactionStatus
    {
      get => selectFundTransactionStatus ??= new();
      set => selectFundTransactionStatus = value;
    }

    /// <summary>
    /// A value of Desired.
    /// </summary>
    [JsonPropertyName("desired")]
    public FundTransactionStatus Desired
    {
      get => desired ??= new();
      set => desired = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public FundTransaction From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public FundTransaction To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// Gets a value of New1.
    /// </summary>
    [JsonIgnore]
    public Array<NewGroup> New1 => new1 ??= new(NewGroup.Capacity);

    /// <summary>
    /// Gets a value of New1 for json serialization.
    /// </summary>
    [JsonPropertyName("new1")]
    [Computed]
    public IList<NewGroup> New1_Json
    {
      get => new1;
      set => New1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of FirstTimeProcessed.
    /// </summary>
    [JsonPropertyName("firstTimeProcessed")]
    public Common FirstTimeProcessed
    {
      get => firstTimeProcessed ??= new();
      set => firstTimeProcessed = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    private FundTransaction selectFundTransaction;
    private FundTransactionStatus selectFundTransactionStatus;
    private FundTransactionStatus desired;
    private FundTransaction from;
    private FundTransaction to;
    private Array<NewGroup> new1;
    private NextTranInfo hidden;
    private Standard standard;
    private Common firstTimeProcessed;
    private Fund fund;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CountGroup.
    /// </summary>
    [JsonPropertyName("countGroup")]
    public Common CountGroup
    {
      get => countGroup ??= new();
      set => countGroup = value;
    }

    /// <summary>
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public DateWorkArea Low
    {
      get => low ??= new();
      set => low = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of HardcodedFttDeposit.
    /// </summary>
    [JsonPropertyName("hardcodedFttDeposit")]
    public FundTransactionType HardcodedFttDeposit
    {
      get => hardcodedFttDeposit ??= new();
      set => hardcodedFttDeposit = value;
    }

    /// <summary>
    /// A value of HardcodedClearing.
    /// </summary>
    [JsonPropertyName("hardcodedClearing")]
    public Fund HardcodedClearing
    {
      get => hardcodedClearing ??= new();
      set => hardcodedClearing = value;
    }

    private Common countGroup;
    private DateWorkArea low;
    private DateWorkArea maximum;
    private DateWorkArea null1;
    private Common select;
    private FundTransactionType hardcodedFttDeposit;
    private Fund hardcodedClearing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
    }

    /// <summary>
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    /// <summary>
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private FundTransactionType fundTransactionType;
    private PcaFundExplosionRule pcaFundExplosionRule;
    private Fund fund;
    private FundTransaction fundTransaction;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus fundTransactionStatus;
    private ProgramCostAccount programCostAccount;
    private CashReceipt cashReceipt;
  }
#endregion
}
