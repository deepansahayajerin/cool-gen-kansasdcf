// Program: FN_B634_PROCESS_PROGRAM_CHANGES, ID: 372264525, model: 746.
// Short name: SWEF634B
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
/// A program: FN_B634_PROCESS_PROGRAM_CHANGES.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This nightly batch procedure provides a means of removing all 
/// collections for a obligor that have previously been distributed to various
/// obligations back to the date of the retroactive program change.  This will
/// allow the distribution policy to redistribute all collections, thus ensuring
/// that the distribution policy rules are followed correctly.
/// Note:  A retroactive program change is a Person_ Program with a null 
/// Program_Change_Date that has changed from a Non-ADC program to an ADC
/// program.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB634ProcessProgramChanges: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B634_PROCESS_PROGRAM_CHANGES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB634ProcessProgramChanges(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB634ProcessProgramChanges.
  /// </summary>
  public FnB634ProcessProgramChanges(IContext context, Import import,
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
    // ***********************************************************************
    //   DATE      DEVELOPER  REQUEST #  DESCRIPTION
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/29/1998  Ed Lyman              Initial Development
    // ----------  ---------  ---------  
    // -------------------------------------
    // 01/15/1999  Ed Lyman              When changing program on a manual
    //                                   
    // collection, create a new
    // collection.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 08/02/1999  Ed Lyman              When CSE Person Account not found,
    //                                   
    // do not abend, report error and go
    // on.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 10/05/1999  Ed Lyman   H00074221  The trigger is now the existence of a
    //                                   
    // date in the new attribute PGM CHG
    //                                   
    // EFFECTIVE DATE. All references to
    // the
    //                                   
    // old triggers, CHANGED IND and
    // CHANGED
    //                                   
    // DT have been removed.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/02/1999  Ed Lyman   PR# 81189  Added program start timestamp.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 06/01/2000  Ed Lyman   WR# 164-G  Added new attribute (trigger type) as
    //                                   
    // part of PRWORA changes.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 09/01/2000  Ed Lyman   PR#102554  Added new trigger type for court order
    //                                   
    // number changes.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/15/2001  Ed Lyman   WR#010504  Added new trigger type for deactivated
    //                                   
    // collection protection.
    // ----------  ---------  ---------  
    // -------------------------------------
    // ***********************************************************************
    // ***********************************************************************
    // This batch procedure reverses all the automatically distributed
    // collections of the supported person when:
    //      there is a change in the person program
    //      there is a grant or Un-Reimbursed Amount (URA) adjustment
    //      there is a court order number change
    //      there is a deactivation of protected collections.
    // If the collection were manually distributed, no action is taken.
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramStart.Timestamp = Now();
    UseFnB634Housekeeping();
    local.Supported.LastUpdatedTmst = Now();
    local.Supported.LastUpdatedBy = local.ProgramProcessingInfo.Name;
    local.Collection.CollectionAdjustmentDt =
      local.ProgramProcessingInfo.ProcessDate;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
    }
    else if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
    {
      // *** The client number being supplied in the ppi parameter should be the
      // supported persons number.
      if (ReadSupportedClient1())
      {
        // ***  Trigger type is a new attribute added for PRWORA ***
        switch(AsChar(entities.Supported.TriggerType))
        {
          case ' ':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.ProgramChange.Description ?? "";
            local.Selected.Assign(local.ProgramChange);
            local.TypeOfChange.SelectChar = "P";
            UseFnB634ProtCollForAfPgmChg();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "cse person number= " + entities
                .ObligorPersons.Number;
              UseCabErrorReport();
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              goto Test;
            }

            break;
          case 'U':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.UraAdj.Description ?? "";
            local.Selected.Assign(local.UraAdj);
            local.TypeOfChange.SelectChar = "U";

            break;
          case 'C':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.CourtOrderNumberChange.Description ?? "";
            local.Selected.Assign(local.CourtOrderNumberChange);
            local.TypeOfChange.SelectChar = "N";

            break;
          case 'P':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.DeactivateCollProtection.Description ?? "";
            local.Selected.Assign(local.DeactivateCollProtection);
            local.TypeOfChange.SelectChar = "X";

            break;
          default:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Invalid Trigger Type for Supported Person number= " + (
                local.PpiParameter.ObligorPersonNumber ?? "");
            UseCabErrorReport();

            goto Test;
        }

        if (AsChar(local.ReportNeeded.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";
          UseCabBusinessReport01();
          local.FormatDate.Text10 =
            NumberToString(Month(entities.Supported.PgmChgEffectiveDate), 14, 2) +
            "-" + NumberToString
            (Day(entities.Supported.PgmChgEffectiveDate), 14, 2) + "-" + NumberToString
            (Year(entities.Supported.PgmChgEffectiveDate), 12, 4);

          switch(AsChar(local.TypeOfChange.SelectChar))
          {
            case 'P':
              local.EabReportSend.RptDetail = "PGM CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            case 'U':
              local.EabReportSend.RptDetail = "URA CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            case 'N':
              local.EabReportSend.RptDetail = "CON CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            case 'X':
              local.EabReportSend.RptDetail = "DCP CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            default:
              break;
          }

          UseCabBusinessReport01();
        }

        foreach(var item in ReadClient2())
        {
          local.CashReceiptDetail.CollectionDate =
            entities.Supported.PgmChgEffectiveDate;
          local.CashReceiptDetail.ObligorPersonNumber =
            entities.ObligorPersons.Number;

          // *****  Read each collection that must be backed off.  These are 
          // collections that have been applied to the obligations which
          // supported the CSE Person  and that have a collection date greater
          // than the program effective date.
          // If the debt has been distributed in advance by a 'future' 
          // collection then reverse the collections that have been applied
          // against those debts due after the program change date ( even though
          // the collection was received prior to the program change effective
          // date).
          // It should process the manually distributed collections also. But 
          // reverse the manually distributed collection only if it has been
          // disbursed.
          UseFnCabReverseAllCshRcptDtls();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "cse person number= " + (
              local.CashReceiptDetail.ObligorPersonNumber ?? "");
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            ExitState = "ACO_NN0000_ALL_OK";
            UseCabErrorReport();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ***  Do not process the program change ***
              continue;
            }
            else
            {
              return;
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "cse person number= " + (
              local.CashReceiptDetail.ObligorPersonNumber ?? "");
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
            }

            return;
          }
        }

        // *****  Update Supported (Person Account)
        UseFnUpdatePgmChgEffectiveDt();

        switch(AsChar(local.TypeOfChange.SelectChar))
        {
          case 'P':
            ++local.NoProgramsChanged.Count;

            break;
          case 'U':
            ++local.NoUraAdjustments.Count;

            break;
          case 'N':
            ++local.NoConChanges.Count;

            break;
          case 'X':
            ++local.NoDeactivatedCollProt.Count;

            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "cse person number= " + entities
            .ObligorPersons.Number;
          UseCabErrorReport();
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
          UseCabErrorReport();
        }
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Supported Person with Pgm Change Not Found for cse person number= " +
          (local.PpiParameter.ObligorPersonNumber ?? "");
        UseCabErrorReport();
      }
    }
    else
    {
      foreach(var item in ReadSupportedClient2())
      {
        // ***  Trigger type is a new attribute added for PRWORA ***
        switch(AsChar(entities.Supported.TriggerType))
        {
          case ' ':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.ProgramChange.Description ?? "";
            local.Selected.Assign(local.ProgramChange);
            local.TypeOfChange.SelectChar = "P";
            UseFnB634ProtCollForAfPgmChg();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "cse person number= " + entities
                .ObligorPersons.Number;
              UseCabErrorReport();
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              goto Test;
            }

            break;
          case 'U':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.UraAdj.Description ?? "";
            local.Selected.Assign(local.UraAdj);
            local.TypeOfChange.SelectChar = "U";

            break;
          case 'C':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.CourtOrderNumberChange.Description ?? "";
            local.Selected.Assign(local.CourtOrderNumberChange);
            local.TypeOfChange.SelectChar = "N";

            break;
          case 'P':
            local.Collection.CollectionAdjustmentReasonTxt =
              local.DeactivateCollProtection.Description ?? "";
            local.Selected.Assign(local.DeactivateCollProtection);
            local.TypeOfChange.SelectChar = "X";

            break;
          default:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Invalid Trigger Type for Supported Person number= " + entities
              .SupportedPersons.Number;
            UseCabErrorReport();

            continue;
        }

        if (AsChar(local.ReportNeeded.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";
          UseCabBusinessReport01();
          local.FormatDate.Text10 =
            NumberToString(Month(entities.Supported.PgmChgEffectiveDate), 14, 2) +
            "-" + NumberToString
            (Day(entities.Supported.PgmChgEffectiveDate), 14, 2) + "-" + NumberToString
            (Year(entities.Supported.PgmChgEffectiveDate), 12, 4);

          switch(AsChar(local.TypeOfChange.SelectChar))
          {
            case 'P':
              local.EabReportSend.RptDetail = "PGM CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            case 'U':
              local.EabReportSend.RptDetail = "URA CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            case 'N':
              local.EabReportSend.RptDetail = "CON CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            case 'X':
              local.EabReportSend.RptDetail = "DCP CHANGE" + "  " + entities
                .SupportedPersons.Number + "   " + local.FormatDate.Text10 + " " +
                " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
                

              break;
            default:
              break;
          }

          UseCabBusinessReport01();
        }

        foreach(var item1 in ReadClient1())
        {
          local.CashReceiptDetail.CollectionDate =
            entities.Supported.PgmChgEffectiveDate;
          local.CashReceiptDetail.ObligorPersonNumber =
            entities.ObligorPersons.Number;

          // *****  Read each collection that must be backed off.  These are 
          // collections that have been applied to the obligations which
          // supported the CSE Person  and that have a collection date greater
          // than the program change effective date.
          // If the debt has been distributed in advance by a 'future' 
          // collection then reverse the collections that have been applied
          // against those debts due after the program change date ( even though
          // the collection was received prior to the program change effective
          // date).
          UseFnCabReverseAllCshRcptDtls();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "cse person number= " + (
              local.CashReceiptDetail.ObligorPersonNumber ?? "");
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            ExitState = "ACO_NN0000_ALL_OK";
            UseCabErrorReport();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ***  Do not process the program change ***
              continue;
            }
            else
            {
              goto Test;
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "cse person number= " + (
              local.CashReceiptDetail.ObligorPersonNumber ?? "");
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
            }

            goto Test;
          }
        }

        // *****  Update Supported (Person Account)
        UseFnUpdatePgmChgEffectiveDt();

        switch(AsChar(local.TypeOfChange.SelectChar))
        {
          case 'P':
            ++local.NoProgramsChanged.Count;

            break;
          case 'U':
            ++local.NoUraAdjustments.Count;

            break;
          case 'N':
            ++local.NoConChanges.Count;

            break;
          case 'X':
            ++local.NoDeactivatedCollProt.Count;

            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "cse person number= " + entities
            .ObligorPersons.Number;
          UseCabErrorReport();
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
          UseCabErrorReport();

          goto Test;
        }

        if (local.UpdatesSinceLastCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          // ***** Call an external that does a DB2 commit using a Cobol 
          // program.
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail = "Error attempting to commit.";
            UseCabErrorReport();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            goto Test;
          }
          else
          {
            local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
              (Now().Date.Month, 14, 2) + "-" + NumberToString
              (Now().Date.Day, 14, 2);
            local.Time.Text8 =
              NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
            local.EabReportSend.RptDetail =
              "Checkpoint Taken after person number: " + entities
              .ObligorPersons.Number + " Date: " + local.Date.Text10 + " Time: " +
              local.Time.Text8;
            UseCabControlReport();
            local.UpdatesSinceLastCommit.Count = 0;
          }
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB634Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB634Close();

      if (!IsExitState("ACO_NN0000_ALL_OK") || !
        Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.PgmChgEffectiveDate = source.PgmChgEffectiveDate;
    target.TriggerType = source.TriggerType;
  }

  private static void MoveProgramProcessingInfo1(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveProgramProcessingInfo2(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB634Close()
  {
    var useImport = new FnB634Close.Import();
    var useExport = new FnB634Close.Export();

    useImport.NoConChanges.Count = local.NoConChanges.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.NoCashRcptDtlsUpdated.Count = local.NoCashRcptDtlUpdated.Count;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useImport.NoProgramsChanged.Count = local.NoProgramsChanged.Count;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.NoDeactivatedCollProt.Count = local.NoDeactivatedCollProt.Count;
    useImport.NoUraAdjustments.Count = local.NoUraAdjustments.Count;

    Call(FnB634Close.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB634Housekeeping()
  {
    var useImport = new FnB634Housekeeping.Import();
    var useExport = new FnB634Housekeeping.Export();

    Call(FnB634Housekeeping.Execute, useImport, useExport);

    local.CourtOrderNumberChange.Assign(useExport.RetroCourtOrderNumber);
    local.RestartCashReceiptDetail.ObligorPersonNumber =
      useExport.Restart.ObligorPersonNumber;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramChange.Assign(useExport.RetroPgmChange);
    local.Max.Date = useExport.Max.Date;
    MoveProgramProcessingInfo2(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.ReleasedStatus.SystemGeneratedIdentifier =
      useExport.ReleasedStatus.SystemGeneratedIdentifier;
    local.DistributedStatus.SystemGeneratedIdentifier =
      useExport.DistributedStatus.SystemGeneratedIdentifier;
    local.RefundedStatus.SystemGeneratedIdentifier =
      useExport.Refunded.SystemGeneratedIdentifier;
    local.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.SuspendedStatus.SystemGeneratedIdentifier;
    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
    local.ReportNeeded.Flag = useExport.ReportNeeded.Flag;
    local.DeactivateCollProtection.Assign(useExport.RetroDeactCollProtect);
    local.UraAdj.Assign(useExport.RetroUraAdj);
  }

  private void UseFnB634ProtCollForAfPgmChg()
  {
    var useImport = new FnB634ProtCollForAfPgmChg.Import();
    var useExport = new FnB634ProtCollForAfPgmChg.Export();

    useImport.PersSupportedPersons.Assign(entities.SupportedPersons);
    useImport.Pers.Assign(entities.Supported);

    Call(FnB634ProtCollForAfPgmChg.Execute, useImport, useExport);
  }

  private void UseFnCabReverseAllCshRcptDtls()
  {
    var useImport = new FnCabReverseAllCshRcptDtls.Import();
    var useExport = new FnCabReverseAllCshRcptDtls.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.InterfaceIndicator =
      local.CashReceiptSourceType.InterfaceIndicator;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber = local.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.Assign(local.CashReceiptDetail);
    useImport.TypeOfChange.SelectChar = local.TypeOfChange.SelectChar;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRcptDtlUpdated.Count;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    MoveCollection(local.Collection, useImport.Collection);
    useImport.Max.Date = local.Max.Date;
    useImport.ReleasedStatus.SystemGeneratedIdentifier =
      local.ReleasedStatus.SystemGeneratedIdentifier;
    useImport.RefundedStatus.SystemGeneratedIdentifier =
      local.RefundedStatus.SystemGeneratedIdentifier;
    useImport.DistributedStatus.SystemGeneratedIdentifier =
      local.DistributedStatus.SystemGeneratedIdentifier;
    useImport.SuspendedStatus.SystemGeneratedIdentifier =
      local.SuspendedStatus.SystemGeneratedIdentifier;
    useImport.NoOfIncrementalUpdates.Count = local.UpdatesSinceLastCommit.Count;
    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramStart.Timestamp = local.ProgramStart.Timestamp;
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.Selected.SystemGeneratedIdentifier;
    useExport.NoCashRecptDtlUpdated.Count = local.NoCashRcptDtlUpdated.Count;
    useExport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useExport.NoOfIncrementalUpdates.Count = local.UpdatesSinceLastCommit.Count;

    Call(FnCabReverseAllCshRcptDtls.Execute, useImport, useExport);

    local.NoCashRcptDtlUpdated.Count = useImport.NoCashRecptDtlUpdated.Count;
    local.NoCollectionsReversed.Count = useImport.NoCollectionsReversed.Count;
    local.UpdatesSinceLastCommit.Count = useImport.NoOfIncrementalUpdates.Count;
    local.NoCashRcptDtlUpdated.Count = useExport.NoCashRecptDtlUpdated.Count;
    local.NoCollectionsReversed.Count = useExport.NoCollectionsReversed.Count;
    local.UpdatesSinceLastCommit.Count = useExport.NoOfIncrementalUpdates.Count;
  }

  private void UseFnUpdatePgmChgEffectiveDt()
  {
    var useImport = new FnUpdatePgmChgEffectiveDt.Import();
    var useExport = new FnUpdatePgmChgEffectiveDt.Export();

    useImport.Pers.Assign(entities.Supported);
    useImport.Supported.Assign(local.Supported);

    Call(FnUpdatePgmChgEffectiveDt.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.Pers, entities.Supported);
  }

  private IEnumerable<bool> ReadClient1()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.ObligorPersons.Populated = false;

    return ReadEach("ReadClient1",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
      },
      (db, reader) =>
      {
        entities.ObligorPersons.Number = db.GetString(reader, 0);
        entities.ObligorPersons.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadClient2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.ObligorPersons.Populated = false;

    return ReadEach("ReadClient2",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
      },
      (db, reader) =>
      {
        entities.ObligorPersons.Number = db.GetString(reader, 0);
        entities.ObligorPersons.Populated = true;

        return true;
      });
  }

  private bool ReadSupportedClient1()
  {
    entities.SupportedPersons.Populated = false;
    entities.Supported.Populated = false;

    return Read("ReadSupportedClient1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.PpiParameter.ObligorPersonNumber ?? "");
        db.SetNullableDate(
          command, "recompBalFromDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.SupportedPersons.Number = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Supported.LastUpdatedTmst = db.GetNullableDateTime(reader, 3);
        entities.Supported.PgmChgEffectiveDate = db.GetNullableDate(reader, 4);
        entities.Supported.TriggerType = db.GetNullableString(reader, 5);
        entities.SupportedPersons.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
      });
  }

  private IEnumerable<bool> ReadSupportedClient2()
  {
    entities.SupportedPersons.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadSupportedClient2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "recompBalFromDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.SupportedPersons.Number = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Supported.LastUpdatedTmst = db.GetNullableDateTime(reader, 3);
        entities.Supported.PgmChgEffectiveDate = db.GetNullableDate(reader, 4);
        entities.Supported.TriggerType = db.GetNullableString(reader, 5);
        entities.SupportedPersons.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

        return true;
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
    /// A value of DeactivateCollProtection.
    /// </summary>
    [JsonPropertyName("deactivateCollProtection")]
    public CollectionAdjustmentReason DeactivateCollProtection
    {
      get => deactivateCollProtection ??= new();
      set => deactivateCollProtection = value;
    }

    /// <summary>
    /// A value of CourtOrderNumberChange.
    /// </summary>
    [JsonPropertyName("courtOrderNumberChange")]
    public CollectionAdjustmentReason CourtOrderNumberChange
    {
      get => courtOrderNumberChange ??= new();
      set => courtOrderNumberChange = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CollectionAdjustmentReason Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of UraAdj.
    /// </summary>
    [JsonPropertyName("uraAdj")]
    public CollectionAdjustmentReason UraAdj
    {
      get => uraAdj ??= new();
      set => uraAdj = value;
    }

    /// <summary>
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

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
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of NoProgramsChanged.
    /// </summary>
    [JsonPropertyName("noProgramsChanged")]
    public Common NoProgramsChanged
    {
      get => noProgramsChanged ??= new();
      set => noProgramsChanged = value;
    }

    /// <summary>
    /// A value of NoUraAdjustments.
    /// </summary>
    [JsonPropertyName("noUraAdjustments")]
    public Common NoUraAdjustments
    {
      get => noUraAdjustments ??= new();
      set => noUraAdjustments = value;
    }

    /// <summary>
    /// A value of NoConChanges.
    /// </summary>
    [JsonPropertyName("noConChanges")]
    public Common NoConChanges
    {
      get => noConChanges ??= new();
      set => noConChanges = value;
    }

    /// <summary>
    /// A value of NoDeactivatedCollProt.
    /// </summary>
    [JsonPropertyName("noDeactivatedCollProt")]
    public Common NoDeactivatedCollProt
    {
      get => noDeactivatedCollProt ??= new();
      set => noDeactivatedCollProt = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("restartCashReceiptDetail")]
    public CashReceiptDetail RestartCashReceiptDetail
    {
      get => restartCashReceiptDetail ??= new();
      set => restartCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of RefundedStatus.
    /// </summary>
    [JsonPropertyName("refundedStatus")]
    public CashReceiptDetailStatus RefundedStatus
    {
      get => refundedStatus ??= new();
      set => refundedStatus = value;
    }

    /// <summary>
    /// A value of DistributedStatus.
    /// </summary>
    [JsonPropertyName("distributedStatus")]
    public CashReceiptDetailStatus DistributedStatus
    {
      get => distributedStatus ??= new();
      set => distributedStatus = value;
    }

    /// <summary>
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
    }

    /// <summary>
    /// A value of SuspendedStatus.
    /// </summary>
    [JsonPropertyName("suspendedStatus")]
    public CashReceiptDetailStatus SuspendedStatus
    {
      get => suspendedStatus ??= new();
      set => suspendedStatus = value;
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
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

    /// <summary>
    /// A value of NoCashRcptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRcptDtlUpdated")]
    public Common NoCashRcptDtlUpdated
    {
      get => noCashRcptDtlUpdated ??= new();
      set => noCashRcptDtlUpdated = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NoOfDebtsToCommit.
    /// </summary>
    [JsonPropertyName("noOfDebtsToCommit")]
    public Common NoOfDebtsToCommit
    {
      get => noOfDebtsToCommit ??= new();
      set => noOfDebtsToCommit = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ProgramChange.
    /// </summary>
    [JsonPropertyName("programChange")]
    public CollectionAdjustmentReason ProgramChange
    {
      get => programChange ??= new();
      set => programChange = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of UpdatesSinceLastCommit.
    /// </summary>
    [JsonPropertyName("updatesSinceLastCommit")]
    public Common UpdatesSinceLastCommit
    {
      get => updatesSinceLastCommit ??= new();
      set => updatesSinceLastCommit = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    private CollectionAdjustmentReason deactivateCollProtection;
    private CollectionAdjustmentReason courtOrderNumberChange;
    private CollectionAdjustmentReason selected;
    private CollectionAdjustmentReason uraAdj;
    private DateWorkArea programStart;
    private CsePersonAccount supported;
    private CashReceiptDetail ppiParameter;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private Common noProgramsChanged;
    private Common noUraAdjustments;
    private Common noConChanges;
    private Common noDeactivatedCollProt;
    private EabReportSend eabReportSend;
    private Common reportNeeded;
    private Common typeOfChange;
    private CashReceiptDetail restartCashReceiptDetail;
    private CashReceiptDetailStatus refundedStatus;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus releasedStatus;
    private CashReceiptDetailStatus suspendedStatus;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Common noCollectionsReversed;
    private Common noCashRcptDtlUpdated;
    private DateWorkArea max;
    private EabFileHandling eabFileHandling;
    private Common noOfDebtsToCommit;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Collection collection;
    private CollectionAdjustmentReason programChange;
    private CsePerson restartCsePerson;
    private External passArea;
    private DateWorkArea null1;
    private Common updatesSinceLastCommit;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea formatDate;
    private TextWorkArea date;
    private TextWorkArea time;
    private Common noOfIncrementalUpdates;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupportedPersons.
    /// </summary>
    [JsonPropertyName("supportedPersons")]
    public CsePerson SupportedPersons
    {
      get => supportedPersons ??= new();
      set => supportedPersons = value;
    }

    /// <summary>
    /// A value of ObligorPersons.
    /// </summary>
    [JsonPropertyName("obligorPersons")]
    public CsePerson ObligorPersons
    {
      get => obligorPersons ??= new();
      set => obligorPersons = value;
    }

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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    private CsePerson supportedPersons;
    private CsePerson obligorPersons;
    private CsePersonAccount supported;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationTransaction debt;
  }
#endregion
}
