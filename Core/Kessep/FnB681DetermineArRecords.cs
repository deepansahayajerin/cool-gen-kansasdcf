// Program: FN_B681_DETERMINE_AR_RECORDS, ID: 374558567, model: 746.
// Short name: SWE03625
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B681_DETERMINE_AR_RECORDS.
/// </summary>
[Serializable]
public partial class FnB681DetermineArRecords: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B681_DETERMINE_AR_RECORDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB681DetermineArRecords(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB681DetermineArRecords.
  /// </summary>
  public FnB681DetermineArRecords(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 05/17/2010  DDupree	PR12681	      Initial Development.  Business rules 
    // for AR GDG.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  Create the AR GDG using the data extracted by B680 which has been 
    // externally sorted/summed.
    // --
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.ProcessDate =
      AddMonths(import.ProgramProcessingInfo.ProcessDate, -1);
    local.Run.Month = Month(local.ProgramProcessingInfo.ProcessDate);
    local.RangeBeg.Year = Year(local.ProgramProcessingInfo.ProcessDate);
    local.RangeBeg.Day = (int)StringToNumber("01");
    local.RangeBeg.TextDate = NumberToString(local.RangeBeg.Year, 12, 4) + NumberToString
      (local.Run.Month, 14, 2) + NumberToString(local.RangeBeg.Day, 14, 2);
    local.RangeBeg.Date =
      IntToDate((int)StringToNumber(local.RangeBeg.TextDate));
    local.ReportingPeriodStarting.Date = local.RangeBeg.Date;
    local.ReportingPeriodEnding.Date =
      AddDays(AddMonths(local.ReportingPeriodStarting.Date, 1), -1);
    local.ReportingPeriodStarting.Timestamp =
      Add(local.Null1.Timestamp, Year(local.ReportingPeriodStarting.Date),
      Month(local.ReportingPeriodStarting.Date),
      Day(local.ReportingPeriodStarting.Date));

    // -- Set the Reporting Period Ending Timestamp.
    local.ReportingPeriodEnding.Timestamp =
      Add(local.Null1.Timestamp, Year(local.ReportingPeriodEnding.Date),
      Month(local.ReportingPeriodEnding.Date),
      Day(local.ReportingPeriodEnding.Date));
    local.ReportingPeriodEnding.Timestamp =
      AddMicroseconds(AddDays(local.ReportingPeriodEnding.Timestamp, 1), -1);

    // --  The export group is only used to view match to the local group in the
    // PrAD so that it is re-initialized after the AR statement is printed, it
    // is otherwise not referenced in this cab.
    // -- If the import group is empty then escape out.  This is used to re-
    // initialize the group in the PrAD when an AR statement exceeded the
    // maximum entries in the group view.
    // -- Load cash/non-cash indicators to a group view for later processing.
    foreach(var item in ReadCashReceiptType())
    {
      local.GlocalCashIndicator.Index =
        entities.CashIndicatorCashReceiptType.SystemGeneratedIdentifier - 1;
      local.GlocalCashIndicator.CheckSize();

      local.GlocalCashIndicator.Update.GcashReceiptType.CategoryIndicator =
        entities.CashIndicatorCashReceiptType.CategoryIndicator;
    }

    foreach(var item in ReadCollectionType2())
    {
      local.GlocalCashIndicator.Index =
        entities.CashIndicatorCollectionType.SequentialIdentifier - 1;
      local.GlocalCashIndicator.CheckSize();

      local.GlocalCashIndicator.Update.GcollectionType.CashNonCashInd =
        entities.CashIndicatorCollectionType.CashNonCashInd;
    }

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.ArCsePerson.Number =
        import.Group.Item.ArCsePerson.Number;
      export.Group.Update.ArCsePersonsWorkSet.Assign(
        import.Group.Item.ArCsePersonsWorkSet);
      export.Group.Update.Ch.Assign(import.Group.Item.Ch);
      export.Group.Update.ForwardedToFamily.Amount =
        import.Group.Item.ForwardedToFamily.Amount;
      export.Group.Update.CashReceiptDetail.SequentialIdentifier =
        import.Group.Item.CashReceiptDetail.SequentialIdentifier;
      export.Group.Update.CashReceiptEvent.SystemGeneratedIdentifier =
        import.Group.Item.CashReceiptEvent.SystemGeneratedIdentifier;
      export.Group.Update.CashReceiptSourceType.SystemGeneratedIdentifier =
        import.Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier;
      export.Group.Update.CashReceiptType.SystemGeneratedIdentifier =
        import.Group.Item.CashReceiptType.SystemGeneratedIdentifier;
      export.Group.Update.Collection.Assign(import.Group.Item.Collection);
      export.Group.Update.Obligation.SystemGeneratedIdentifier =
        import.Group.Item.Obligation.SystemGeneratedIdentifier;
      MoveObligationTransaction(import.Group.Item.ObligationTransaction,
        export.Group.Update.ObligationTransaction);
      export.Group.Update.ObligationType.SystemGeneratedIdentifier =
        import.Group.Item.ObligationType.SystemGeneratedIdentifier;
      export.Group.Update.Obligor.Type1 = import.Group.Item.Obligor.Type1;
      export.Group.Update.ObligorCsePerson.Number =
        import.Group.Item.ObligorCsePerson.Number;
      export.Group.Update.ObligorCsePersonsWorkSet.LastName =
        import.Group.Item.ObligorCsePersonsWorkSet.LastName;
      export.Group.Update.Retained.Amount = import.Group.Item.Retained.Amount;
      export.Group.Update.Taf.SystemGeneratedIdentifier =
        import.Group.Item.Taf.SystemGeneratedIdentifier;
    }

    import.Group.CheckIndex();
    export.OkToSendRecord.Flag = "";

    // -- Default return status to ERRORED.  We'll reset it to SKIPPED or 
    // PRINTED later.
    export.ArStatementStatus.Text8 = "ERRORED";
    local.EabFileHandling.Action = "WRITE";
    import.Group.Index = 0;

    for(var limit = import.Group.Count; import.Group.Index < limit; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      if (import.Non718BCollection.Count <= 0)
      {
        // --  B663 writes obligation type of 0 to the extract file for 718B 
        // collections.  The sort/sum step then sums
        // --  the obligation types on the ARs collections and B664 will not 
        // print a statement if the summed obligation
        // --  types for the AR equal 0 (meaning only 718B collections were 
        // found).
        export.ArStatementStatus.Text8 = "718BONLY";

        // -- Write to error file...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "AR " + import
          .Group.Item.ArCsePerson.Number + " - 718B Collections Only.";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        break;
      }

      if (AsChar(import.OkToSendRecord.Flag) != 'Y')
      {
        break;
      }

      UseFnB680ArExtractData1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "(01) Error writing collection info to extract file...  Returned Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport();
        ExitState = "ERROR_WRITING_TO_FILE_AB";

        return;
      }
    }

    import.Group.CheckIndex();

    if (import.Taf.SystemGeneratedIdentifier == 1 && import
      .Import718B.SystemGeneratedIdentifier == 1)
    {
      export.OkToSendRecord.Flag = "Y";

      return;
    }
    else
    {
      if (!ReadCsePerson1())
      {
        // -- Write to error file...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - CSE Person Not Found.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
        }

        return;
      }

      // Do not send statement if the AR is deceased.
      if (!Equal(entities.Ar.DateOfDeath, local.Null1.Date))
      {
        export.ArStatementStatus.Text8 = "DECEASED";

        // -- Write to error file...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - AR is Deceased.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
        }

        return;
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Find a case for which assigned obligations exist for the AR.
      // -------------------------------------------------------------------------------------------------------------------------
      local.CaseFound.Flag = "N";

      foreach(var item in ReadCase())
      {
        local.CaseFound.Flag = "Y";

        foreach(var item1 in ReadCsePerson3())
        {
          if (AsChar(local.TafMoneyFoundFlag.Flag) == 'Y' && AsChar
            (local.Non718BFound.Flag) == 'Y')
          {
            export.OkToSendRecord.Flag = "Y";

            goto Test;
          }
          else if (AsChar(local.TafMoneyFoundFlag.Flag) == 'Y' && IsEmpty
            (local.Non718BFound.Flag))
          {
            goto Test;
          }

          local.TafMoneyFoundFlag.Flag = "";
          local.Non718BFound.Flag = "";

          // -- Insure there is an assigned obligation to this AR on this case.
          foreach(var item2 in ReadAccrualInstructions())
          {
            if (!ReadDebt1())
            {
              continue;
            }

            if (ReadObligation())
            {
              if (AsChar(entities.Obligation.OrderTypeCode) != 'K')
              {
                continue;
              }
            }
            else
            {
              continue;
            }

            if (!ReadObligationType())
            {
              continue;
            }

            if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
            {
              if (!ReadCsePerson2())
              {
                continue;
              }
            }

            if (ReadDebtDetail1())
            {
              local.DebtDetail.Assign(entities.DebtDetail);
            }

            local.DebtDetail.DueDt = import.ProgramProcessingInfo.ProcessDate;
            UseFnDeterminePgmForDebtDetail2();

            if (Equal(local.Program.Code, "AF") || Equal
              (local.Program.Code, "NA") && Equal
              (local.DprProgram.ProgramState, "CA"))
            {
              // --  Insure the AR for the debt is the same as the AR we are 
              // processing.
              local.DueDate.Date = import.ProgramProcessingInfo.ProcessDate;
              UseFnB664DetermineAr();

              if (Equal(entities.Ar.Number, local.DerivedAr.Number))
              {
                local.TafMoneyFoundFlag.Flag = "Y";

                foreach(var item3 in ReadCollectionCashReceiptDetailCashReceiptEvent1())
                  
                {
                  local.GlocalCashIndicator.Index =
                    entities.CashReceiptType.SystemGeneratedIdentifier - 1;
                  local.GlocalCashIndicator.CheckSize();

                  if (AsChar(local.GlocalCashIndicator.Item.GcashReceiptType.
                    CategoryIndicator) != 'C')
                  {
                    continue;
                  }

                  if (ReadCollectionType1())
                  {
                    local.GlocalCashIndicator.Index =
                      entities.CollectionType.SequentialIdentifier - 1;
                    local.GlocalCashIndicator.CheckSize();

                    if (AsChar(local.GlocalCashIndicator.Item.GcollectionType.
                      CashNonCashInd) != 'C')
                    {
                      continue;
                    }
                  }
                  else
                  {
                    // -- Skip the collection.
                    continue;
                  }

                  if (Lt(entities.Collection.CollectionDt,
                    local.ReportingPeriodStarting.Date))
                  {
                    // --  Check for prior adjusted collections for the cash 
                    // receipt detail.
                    if (ReadCollection())
                    {
                      continue;
                    }
                    else
                    {
                      // -- Continue.
                    }

                    // --  Check for prior adjustment to the cash receipt 
                    // detail.
                    if (ReadCashReceiptDetailBalanceAdj1())
                    {
                      continue;
                    }
                    else
                    {
                      // -- Continue.
                    }

                    if (ReadCashReceiptDetailBalanceAdj2())
                    {
                      continue;
                    }
                    else
                    {
                      // -- Continue.
                    }
                  }

                  if (entities.N2dReadObligationType.
                    SystemGeneratedIdentifier == import
                    .Import718BType.SystemGeneratedIdentifier)
                  {
                    local.Non718BFound.Flag = "N";
                  }
                  else
                  {
                    // -- An assigned obligation exists on this case for our AR.
                    // Use this case.
                    local.Non718BFound.Flag = "Y";

                    goto Test;
                  }
                }
              }
            }
          }

          foreach(var item2 in ReadDebtDetail2())
          {
            if (!ReadDebt2())
            {
              continue;
            }

            if (ReadObligation())
            {
              if (AsChar(entities.Obligation.OrderTypeCode) != 'K')
              {
                continue;
              }
            }
            else
            {
              continue;
            }

            if (!ReadObligationType())
            {
              continue;
            }

            if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
            {
              if (!ReadCsePerson2())
              {
                continue;
              }
            }

            UseFnDeterminePgmForDebtDetail1();

            if (Equal(local.Program.Code, "AF") || Equal
              (local.Program.Code, "NA") && Equal
              (local.DprProgram.ProgramState, "CA"))
            {
              // --  Insure the AR for the debt is the same as the AR we are 
              // processing.
              local.DueDate.Date = entities.DebtDetail.DueDt;
              UseFnB664DetermineAr();

              if (Equal(entities.Ar.Number, local.DerivedAr.Number))
              {
                local.TafMoneyFoundFlag.Flag = "Y";

                foreach(var item3 in ReadCollectionCashReceiptDetailCashReceiptEvent2())
                  
                {
                  local.GlocalCashIndicator.Index =
                    entities.CashReceiptType.SystemGeneratedIdentifier - 1;
                  local.GlocalCashIndicator.CheckSize();

                  if (AsChar(local.GlocalCashIndicator.Item.GcashReceiptType.
                    CategoryIndicator) != 'C')
                  {
                    continue;
                  }

                  if (ReadCollectionType1())
                  {
                    local.GlocalCashIndicator.Index =
                      entities.CollectionType.SequentialIdentifier - 1;
                    local.GlocalCashIndicator.CheckSize();

                    if (AsChar(local.GlocalCashIndicator.Item.GcollectionType.
                      CashNonCashInd) != 'C')
                    {
                      continue;
                    }
                  }
                  else
                  {
                    // -- Skip the collection.
                    continue;
                  }

                  if (Lt(entities.Collection.CollectionDt,
                    local.ReportingPeriodStarting.Date))
                  {
                    // --  Check for prior adjusted collections for the cash 
                    // receipt detail.
                    if (ReadCollection())
                    {
                      continue;
                    }
                    else
                    {
                      // -- Continue.
                    }

                    // --  Check for prior adjustment to the cash receipt 
                    // detail.
                    if (ReadCashReceiptDetailBalanceAdj1())
                    {
                      continue;
                    }
                    else
                    {
                      // -- Continue.
                    }

                    if (ReadCashReceiptDetailBalanceAdj2())
                    {
                      continue;
                    }
                    else
                    {
                      // -- Continue.
                    }
                  }

                  if (entities.N2dReadObligationType.
                    SystemGeneratedIdentifier == import
                    .Import718BType.SystemGeneratedIdentifier)
                  {
                    local.Non718BFound.Flag = "N";
                  }
                  else
                  {
                    // -- An assigned obligation exists on this case for our AR.
                    // Use this case.
                    local.Non718BFound.Flag = "Y";

                    goto Test;
                  }
                }
              }
            }
          }
        }
      }
    }

Test:

    if (AsChar(local.Non718BFound.Flag) == 'Y' || AsChar
      (local.TafMoneyFoundFlag.Flag) == 'Y')
    {
      // Since we do have a TAF debt by some AP that does met quailfication we 
      // will
      // let this AR procede. There is already a process to remove 718b only AP'
      // s in
      // the pstep
      export.OkToSendRecord.Flag = "Y";
    }
    else if (AsChar(local.TafMoneyFoundFlag.Flag) == 'Y' && IsEmpty
      (local.Non718BFound.Flag))
    {
      export.OkToSendRecord.Flag = "Y";
    }
    else
    {
      export.OkToSendRecord.Flag = "";
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Determine if a case was found with assigned obligations.
    // -------------------------------------------------------------------------------------------------------------------------
    if (!entities.Case1.Populated)
    {
      // -- Write to error file...
      local.EabFileHandling.Action = "WRITE";

      if (AsChar(local.CaseFound.Flag) == 'Y')
      {
        // -- A case was found for the AR but there were no assigned 
        // obligations.  Skip the AR statement.
        export.ArStatementStatus.Text8 = "NOASSOB";
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No Assigned Obligations.";
          
      }
      else
      {
        local.EabReportSend.RptDetail = "AR " + import.Ar.Number + " - No Open CSE Case.";
          
      }

      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB664DetermineAr()
  {
    var useImport = new FnB664DetermineAr.Import();
    var useExport = new FnB664DetermineAr.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.Debt);
    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.ReportingPeriodEnding.Date =
      import.ReportingPeriodEndingDateWorkArea.Date;
    useImport.Voluntary.SystemGeneratedIdentifier =
      import.Voluntary.SystemGeneratedIdentifier;
    useImport.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      import.SpousalArrearsJudgement.SystemGeneratedIdentifier;
    useImport.SpousalSupport.SystemGeneratedIdentifier =
      import.SpousalSupport.SystemGeneratedIdentifier;
    useImport.DueDate.Date = local.DueDate.Date;

    Call(FnB664DetermineAr.Execute, useImport, useExport);

    MoveCsePerson(useExport.Ar, local.DerivedAr);
  }

  private void UseFnB680ArExtractData1()
  {
    var useImport = new FnB680ArExtractData1.Import();
    var useExport = new FnB680ArExtractData1.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ArCsePerson.Number = import.Group.Item.ArCsePerson.Number;
    useImport.ObligorCsePersonsWorkSet.LastName =
      import.Group.Item.ObligorCsePersonsWorkSet.LastName;
    useImport.ObligorCsePerson.Number =
      import.Group.Item.ObligorCsePerson.Number;
    useImport.Collection.Assign(import.Group.Item.Collection);
    useImport.Retained.Amount = import.Group.Item.Retained.Amount;
    useImport.ForwardedToFamily.Amount =
      import.Group.Item.ForwardedToFamily.Amount;
    useImport.Obligor.Type1 = import.Group.Item.Obligor.Type1;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.Group.Item.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Group.Item.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(import.Group.Item.ObligationTransaction,
      useImport.ObligationTransaction);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.Group.Item.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.Group.Item.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.Group.Item.CashReceiptDetail.SequentialIdentifier;
    useImport.ChPerson.Assign(import.Group.Item.Ch);
    useImport.ArCsePersonsWorkSet.Assign(import.Group.Item.ArCsePersonsWorkSet);
    useImport.Taf.SystemGeneratedIdentifier =
      import.Group.Item.Taf.SystemGeneratedIdentifier;
    MoveExternal(local.External, useExport.External);

    Call(FnB680ArExtractData1.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnDeterminePgmForDebtDetail1()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.Supported2.Number;
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.DebtDetail.Assign(entities.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private void UseFnDeterminePgmForDebtDetail2()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.Supported2.Number;
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.DebtDetail.Assign(local.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailBalanceAdj1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdj1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailBalanceAdj2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdj2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdSIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvSIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstSIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtSIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptType()
  {
    entities.CashIndicatorCashReceiptType.Populated = false;

    return ReadEach("ReadCashReceiptType",
      null,
      (db, reader) =>
      {
        entities.CashIndicatorCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashIndicatorCashReceiptType.CategoryIndicator =
          db.GetString(reader, 1);
        entities.CashIndicatorCashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashIndicatorCashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjusted.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Adjusted.CrtType = db.GetInt32(reader, 2);
        entities.Adjusted.CstId = db.GetInt32(reader, 3);
        entities.Adjusted.CrvId = db.GetInt32(reader, 4);
        entities.Adjusted.CrdId = db.GetInt32(reader, 5);
        entities.Adjusted.ObgId = db.GetInt32(reader, 6);
        entities.Adjusted.CspNumber = db.GetString(reader, 7);
        entities.Adjusted.CpaType = db.GetString(reader, 8);
        entities.Adjusted.OtrId = db.GetInt32(reader, 9);
        entities.Adjusted.OtrType = db.GetString(reader, 10);
        entities.Adjusted.OtyId = db.GetInt32(reader, 11);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Adjusted.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Adjusted.CpaType);
        CheckValid<Collection>("OtrType", entities.Adjusted.OtrType);
      });
  }

  private IEnumerable<bool> ReadCollectionCashReceiptDetailCashReceiptEvent1()
  {
    entities.N2dReadObligorCsePerson.Populated = false;
    entities.N2dReadObligationType.Populated = false;
    entities.N2dReadDebt.Populated = false;
    entities.N2dReadObligorObligor.Populated = false;
    entities.N2dReadObligation.Populated = false;
    entities.Collection.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptType.Populated = false;

    return ReadEach("ReadCollectionCashReceiptDetailCashReceiptEvent1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
        db.SetDateTime(
          command, "timestamp1",
          local.ReportingPeriodStarting.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.ReportingPeriodEnding.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.N2dReadDebt.ObgGeneratedId = db.GetInt32(reader, 8);
        entities.N2dReadObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.N2dReadDebt.CspNumber = db.GetString(reader, 9);
        entities.N2dReadObligorObligor.CspNumber = db.GetString(reader, 9);
        entities.N2dReadObligation.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.N2dReadDebt.CpaType = db.GetString(reader, 10);
        entities.N2dReadObligorObligor.Type1 = db.GetString(reader, 10);
        entities.N2dReadObligation.CpaType = db.GetString(reader, 10);
        entities.N2dReadObligation.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.N2dReadDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.N2dReadDebt.Type1 = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.N2dReadDebt.OtyType = db.GetInt32(reader, 13);
        entities.N2dReadObligation.DtyGeneratedId = db.GetInt32(reader, 13);
        entities.N2dReadObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 19);
        entities.Collection.ArNumber = db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 21);
        entities.N2dReadObligorCsePerson.Number = db.GetString(reader, 22);
        entities.N2dReadDebt.CspNumber = db.GetString(reader, 22);
        entities.N2dReadObligorObligor.CspNumber = db.GetString(reader, 22);
        entities.N2dReadObligation.CspNumber = db.GetString(reader, 22);
        entities.N2dReadObligation.CspNumber = db.GetString(reader, 22);
        entities.N2dReadDebt.CspSupNumber = db.GetNullableString(reader, 23);
        entities.N2dReadDebt.CpaSupType = db.GetNullableString(reader, 24);
        entities.N2dReadObligation.OrderTypeCode = db.GetString(reader, 25);
        entities.N2dReadObligationType.Classification =
          db.GetString(reader, 26);
        entities.N2dReadObligationType.SupportedPersonReqInd =
          db.GetString(reader, 27);
        entities.N2dReadObligorCsePerson.Populated = true;
        entities.N2dReadObligationType.Populated = true;
        entities.N2dReadDebt.Populated = true;
        entities.N2dReadObligorObligor.Populated = true;
        entities.N2dReadObligation.Populated = true;
        entities.Collection.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.N2dReadDebt.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.N2dReadObligorObligor.Type1);
        CheckValid<Obligation>("CpaType", entities.N2dReadObligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.N2dReadObligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.N2dReadDebt.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.N2dReadDebt.CpaSupType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.N2dReadObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.N2dReadObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.N2dReadObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCashReceiptDetailCashReceiptEvent2()
  {
    entities.N2dReadObligorCsePerson.Populated = false;
    entities.N2dReadObligationType.Populated = false;
    entities.N2dReadDebt.Populated = false;
    entities.N2dReadObligorObligor.Populated = false;
    entities.N2dReadObligation.Populated = false;
    entities.Collection.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptType.Populated = false;

    return ReadEach("ReadCollectionCashReceiptDetailCashReceiptEvent2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
        db.SetDateTime(
          command, "createdTmst1",
          local.ReportingPeriodStarting.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          local.ReportingPeriodEnding.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.N2dReadDebt.ObgGeneratedId = db.GetInt32(reader, 8);
        entities.N2dReadObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.N2dReadDebt.CspNumber = db.GetString(reader, 9);
        entities.N2dReadObligorObligor.CspNumber = db.GetString(reader, 9);
        entities.N2dReadObligation.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.N2dReadDebt.CpaType = db.GetString(reader, 10);
        entities.N2dReadObligorObligor.Type1 = db.GetString(reader, 10);
        entities.N2dReadObligation.CpaType = db.GetString(reader, 10);
        entities.N2dReadObligation.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.N2dReadDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.N2dReadDebt.Type1 = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.N2dReadDebt.OtyType = db.GetInt32(reader, 13);
        entities.N2dReadObligation.DtyGeneratedId = db.GetInt32(reader, 13);
        entities.N2dReadObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 19);
        entities.Collection.ArNumber = db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 21);
        entities.N2dReadObligorCsePerson.Number = db.GetString(reader, 22);
        entities.N2dReadDebt.CspNumber = db.GetString(reader, 22);
        entities.N2dReadObligorObligor.CspNumber = db.GetString(reader, 22);
        entities.N2dReadObligation.CspNumber = db.GetString(reader, 22);
        entities.N2dReadObligation.CspNumber = db.GetString(reader, 22);
        entities.N2dReadDebt.CspSupNumber = db.GetNullableString(reader, 23);
        entities.N2dReadDebt.CpaSupType = db.GetNullableString(reader, 24);
        entities.N2dReadObligation.OrderTypeCode = db.GetString(reader, 25);
        entities.N2dReadObligationType.Classification =
          db.GetString(reader, 26);
        entities.N2dReadObligationType.SupportedPersonReqInd =
          db.GetString(reader, 27);
        entities.N2dReadObligorCsePerson.Populated = true;
        entities.N2dReadObligationType.Populated = true;
        entities.N2dReadDebt.Populated = true;
        entities.N2dReadObligorObligor.Populated = true;
        entities.N2dReadObligation.Populated = true;
        entities.Collection.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.N2dReadDebt.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.N2dReadObligorObligor.Type1);
        CheckValid<Obligation>("CpaType", entities.N2dReadObligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.N2dReadObligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.N2dReadDebt.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.N2dReadDebt.CpaSupType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.N2dReadObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.N2dReadObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.N2dReadObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private bool ReadCollectionType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionType2()
  {
    entities.CashIndicatorCollectionType.Populated = false;

    return ReadEach("ReadCollectionType2",
      null,
      (db, reader) =>
      {
        entities.CashIndicatorCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CashIndicatorCollectionType.CashNonCashInd =
          db.GetString(reader, 1);
        entities.CashIndicatorCollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CashIndicatorCollectionType.CashNonCashInd);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ar.Number);
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

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.Obligor1.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor1.Populated = true;

        return true;
      });
  }

  private bool ReadDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt1",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyType", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obTrnId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "obTrnTyp", entities.DebtDetail.OtrType);
        db.SetInt32(command, "obTrnId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadDebtDetail2()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Debt.OtyType);
        db.SetInt32(command, "obId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
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
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
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
    /// <summary>A GimportExportStatementCountGroup group.</summary>
    [Serializable]
    public class GimportExportStatementCountGroup
    {
      /// <summary>
      /// A value of GimportExportCount.
      /// </summary>
      [JsonPropertyName("gimportExportCount")]
      public Common GimportExportCount
      {
        get => gimportExportCount ??= new();
        set => gimportExportCount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common gimportExportCount;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of ObligorCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("obligorCsePersonsWorkSet")]
      public CsePersonsWorkSet ObligorCsePersonsWorkSet
      {
        get => obligorCsePersonsWorkSet ??= new();
        set => obligorCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ObligorCsePerson.
      /// </summary>
      [JsonPropertyName("obligorCsePerson")]
      public CsePerson ObligorCsePerson
      {
        get => obligorCsePerson ??= new();
        set => obligorCsePerson = value;
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
      /// A value of Retained.
      /// </summary>
      [JsonPropertyName("retained")]
      public Collection Retained
      {
        get => retained ??= new();
        set => retained = value;
      }

      /// <summary>
      /// A value of ForwardedToFamily.
      /// </summary>
      [JsonPropertyName("forwardedToFamily")]
      public Collection ForwardedToFamily
      {
        get => forwardedToFamily ??= new();
        set => forwardedToFamily = value;
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
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
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
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
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
      /// A value of CashReceiptEvent.
      /// </summary>
      [JsonPropertyName("cashReceiptEvent")]
      public CashReceiptEvent CashReceiptEvent
      {
        get => cashReceiptEvent ??= new();
        set => cashReceiptEvent = value;
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
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of Ch.
      /// </summary>
      [JsonPropertyName("ch")]
      public CsePersonsWorkSet Ch
      {
        get => ch ??= new();
        set => ch = value;
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
      /// A value of Taf.
      /// </summary>
      [JsonPropertyName("taf")]
      public ObligationType Taf
      {
        get => taf ??= new();
        set => taf = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson arCsePerson;
      private CsePersonsWorkSet obligorCsePersonsWorkSet;
      private CsePerson obligorCsePerson;
      private Collection collection;
      private Collection retained;
      private Collection forwardedToFamily;
      private CsePersonAccount obligor;
      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction obligationTransaction;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceiptType cashReceiptType;
      private CashReceiptDetail cashReceiptDetail;
      private CsePersonsWorkSet ch;
      private CsePersonsWorkSet arCsePersonsWorkSet;
      private ObligationType taf;
    }

    /// <summary>
    /// A value of TafCollection.
    /// </summary>
    [JsonPropertyName("tafCollection")]
    public Common TafCollection
    {
      get => tafCollection ??= new();
      set => tafCollection = value;
    }

    /// <summary>
    /// A value of OkToSendRecord.
    /// </summary>
    [JsonPropertyName("okToSendRecord")]
    public Common OkToSendRecord
    {
      get => okToSendRecord ??= new();
      set => okToSendRecord = value;
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
    /// A value of Non718BCollection.
    /// </summary>
    [JsonPropertyName("non718BCollection")]
    public Common Non718BCollection
    {
      get => non718BCollection ??= new();
      set => non718BCollection = value;
    }

    /// <summary>
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingDateWorkArea")]
    public DateWorkArea ReportingPeriodEndingDateWorkArea
    {
      get => reportingPeriodEndingDateWorkArea ??= new();
      set => reportingPeriodEndingDateWorkArea = value;
    }

    /// <summary>
    /// Gets a value of GimportExportStatementCount.
    /// </summary>
    [JsonIgnore]
    public Array<GimportExportStatementCountGroup>
      GimportExportStatementCount => gimportExportStatementCount ??= new(
        GimportExportStatementCountGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of GimportExportStatementCount for json serialization.
    /// </summary>
    [JsonPropertyName("gimportExportStatementCount")]
    [Computed]
    public IList<GimportExportStatementCountGroup>
      GimportExportStatementCount_Json
    {
      get => gimportExportStatementCount;
      set => GimportExportStatementCount.Assign(value);
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public TextWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingTextWorkArea")]
    public TextWorkArea ReportingPeriodEndingTextWorkArea
    {
      get => reportingPeriodEndingTextWorkArea ??= new();
      set => reportingPeriodEndingTextWorkArea = value;
    }

    /// <summary>
    /// A value of Voluntary.
    /// </summary>
    [JsonPropertyName("voluntary")]
    public ObligationType Voluntary
    {
      get => voluntary ??= new();
      set => voluntary = value;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of Import718B.
    /// </summary>
    [JsonPropertyName("import718B")]
    public ObligationType Import718B
    {
      get => import718B ??= new();
      set => import718B = value;
    }

    /// <summary>
    /// A value of Taf.
    /// </summary>
    [JsonPropertyName("taf")]
    public ObligationType Taf
    {
      get => taf ??= new();
      set => taf = value;
    }

    /// <summary>
    /// A value of SpousalSupport.
    /// </summary>
    [JsonPropertyName("spousalSupport")]
    public ObligationType SpousalSupport
    {
      get => spousalSupport ??= new();
      set => spousalSupport = value;
    }

    /// <summary>
    /// A value of Import718BType.
    /// </summary>
    [JsonPropertyName("import718BType")]
    public ObligationType Import718BType
    {
      get => import718BType ??= new();
      set => import718BType = value;
    }

    private Common tafCollection;
    private Common okToSendRecord;
    private CsePerson ap;
    private Common non718BCollection;
    private Common createEvents;
    private DateWorkArea reportingPeriodEndingDateWorkArea;
    private Array<GimportExportStatementCountGroup> gimportExportStatementCount;
    private CsePerson ar;
    private Array<GroupGroup> group;
    private ProgramProcessingInfo programProcessingInfo;
    private TextWorkArea reportingPeriodStarting;
    private TextWorkArea reportingPeriodEndingTextWorkArea;
    private ObligationType voluntary;
    private ObligationType spousalArrearsJudgement;
    private ObligationType import718B;
    private ObligationType taf;
    private ObligationType spousalSupport;
    private ObligationType import718BType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of ObligorCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("obligorCsePersonsWorkSet")]
      public CsePersonsWorkSet ObligorCsePersonsWorkSet
      {
        get => obligorCsePersonsWorkSet ??= new();
        set => obligorCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ObligorCsePerson.
      /// </summary>
      [JsonPropertyName("obligorCsePerson")]
      public CsePerson ObligorCsePerson
      {
        get => obligorCsePerson ??= new();
        set => obligorCsePerson = value;
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
      /// A value of Retained.
      /// </summary>
      [JsonPropertyName("retained")]
      public Collection Retained
      {
        get => retained ??= new();
        set => retained = value;
      }

      /// <summary>
      /// A value of ForwardedToFamily.
      /// </summary>
      [JsonPropertyName("forwardedToFamily")]
      public Collection ForwardedToFamily
      {
        get => forwardedToFamily ??= new();
        set => forwardedToFamily = value;
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
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
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
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
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
      /// A value of CashReceiptEvent.
      /// </summary>
      [JsonPropertyName("cashReceiptEvent")]
      public CashReceiptEvent CashReceiptEvent
      {
        get => cashReceiptEvent ??= new();
        set => cashReceiptEvent = value;
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
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of Ch.
      /// </summary>
      [JsonPropertyName("ch")]
      public CsePersonsWorkSet Ch
      {
        get => ch ??= new();
        set => ch = value;
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
      /// A value of Taf.
      /// </summary>
      [JsonPropertyName("taf")]
      public ObligationType Taf
      {
        get => taf ??= new();
        set => taf = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson arCsePerson;
      private CsePersonsWorkSet obligorCsePersonsWorkSet;
      private CsePerson obligorCsePerson;
      private Collection collection;
      private Collection retained;
      private Collection forwardedToFamily;
      private CsePersonAccount obligor;
      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction obligationTransaction;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceiptType cashReceiptType;
      private CashReceiptDetail cashReceiptDetail;
      private CsePersonsWorkSet ch;
      private CsePersonsWorkSet arCsePersonsWorkSet;
      private ObligationType taf;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GexportObligor.
      /// </summary>
      [JsonPropertyName("gexportObligor")]
      public CsePerson GexportObligor
      {
        get => gexportObligor ??= new();
        set => gexportObligor = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Collection G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportRetained.
      /// </summary>
      [JsonPropertyName("gexportRetained")]
      public Collection GexportRetained
      {
        get => gexportRetained ??= new();
        set => gexportRetained = value;
      }

      /// <summary>
      /// A value of GexportForwardedToFamily.
      /// </summary>
      [JsonPropertyName("gexportForwardedToFamily")]
      public Collection GexportForwardedToFamily
      {
        get => gexportForwardedToFamily ??= new();
        set => gexportForwardedToFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson gexportObligor;
      private Collection g;
      private Collection gexportRetained;
      private Collection gexportForwardedToFamily;
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
    /// A value of OkToSendRecord.
    /// </summary>
    [JsonPropertyName("okToSendRecord")]
    public Common OkToSendRecord
    {
      get => okToSendRecord ??= new();
      set => okToSendRecord = value;
    }

    /// <summary>
    /// A value of ArStatementStatus.
    /// </summary>
    [JsonPropertyName("arStatementStatus")]
    public TextWorkArea ArStatementStatus
    {
      get => arStatementStatus ??= new();
      set => arStatementStatus = value;
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

    private Array<GroupGroup> group;
    private Common okToSendRecord;
    private TextWorkArea arStatementStatus;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ArAddressGroup group.</summary>
    [Serializable]
    public class ArAddressGroup
    {
      /// <summary>
      /// A value of GlocalArAddress.
      /// </summary>
      [JsonPropertyName("glocalArAddress")]
      public FieldValue GlocalArAddress
      {
        get => glocalArAddress ??= new();
        set => glocalArAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalArAddress;
    }

    /// <summary>A PrintGroup group.</summary>
    [Serializable]
    public class PrintGroup
    {
      /// <summary>
      /// A value of GlocalReportDetailLine.
      /// </summary>
      [JsonPropertyName("glocalReportDetailLine")]
      public EabReportSend GlocalReportDetailLine
      {
        get => glocalReportDetailLine ??= new();
        set => glocalReportDetailLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private EabReportSend glocalReportDetailLine;
    }

    /// <summary>A OfficeAddressGroup group.</summary>
    [Serializable]
    public class OfficeAddressGroup
    {
      /// <summary>
      /// A value of GlocalOfficeAddress.
      /// </summary>
      [JsonPropertyName("glocalOfficeAddress")]
      public FieldValue GlocalOfficeAddress
      {
        get => glocalOfficeAddress ??= new();
        set => glocalOfficeAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalOfficeAddress;
    }

    /// <summary>A GlocalCashIndicatorGroup group.</summary>
    [Serializable]
    public class GlocalCashIndicatorGroup
    {
      /// <summary>
      /// A value of GcollectionType.
      /// </summary>
      [JsonPropertyName("gcollectionType")]
      public CollectionType GcollectionType
      {
        get => gcollectionType ??= new();
        set => gcollectionType = value;
      }

      /// <summary>
      /// A value of GcashReceiptType.
      /// </summary>
      [JsonPropertyName("gcashReceiptType")]
      public CashReceiptType GcashReceiptType
      {
        get => gcashReceiptType ??= new();
        set => gcashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CollectionType gcollectionType;
      private CashReceiptType gcashReceiptType;
    }

    /// <summary>
    /// A value of Non718BFound.
    /// </summary>
    [JsonPropertyName("non718BFound")]
    public Common Non718BFound
    {
      get => non718BFound ??= new();
      set => non718BFound = value;
    }

    /// <summary>
    /// A value of TafMoneyFoundFlag.
    /// </summary>
    [JsonPropertyName("tafMoneyFoundFlag")]
    public Common TafMoneyFoundFlag
    {
      get => tafMoneyFoundFlag ??= new();
      set => tafMoneyFoundFlag = value;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePersonsWorkSet Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DerivedAr.
    /// </summary>
    [JsonPropertyName("derivedAr")]
    public CsePerson DerivedAr
    {
      get => derivedAr ??= new();
      set => derivedAr = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
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
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
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
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
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
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public SpPrintWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of LastOfImportGroup.
    /// </summary>
    [JsonPropertyName("lastOfImportGroup")]
    public Common LastOfImportGroup
    {
      get => lastOfImportGroup ??= new();
      set => lastOfImportGroup = value;
    }

    /// <summary>
    /// A value of SubscriptForFooter.
    /// </summary>
    [JsonPropertyName("subscriptForFooter")]
    public Common SubscriptForFooter
    {
      get => subscriptForFooter ??= new();
      set => subscriptForFooter = value;
    }

    /// <summary>
    /// A value of Remainder.
    /// </summary>
    [JsonPropertyName("remainder")]
    public Common Remainder
    {
      get => remainder ??= new();
      set => remainder = value;
    }

    /// <summary>
    /// A value of NumberOfPages.
    /// </summary>
    [JsonPropertyName("numberOfPages")]
    public Common NumberOfPages
    {
      get => numberOfPages ??= new();
      set => numberOfPages = value;
    }

    /// <summary>
    /// A value of NumberOfLinesPerPage.
    /// </summary>
    [JsonPropertyName("numberOfLinesPerPage")]
    public Common NumberOfLinesPerPage
    {
      get => numberOfLinesPerPage ??= new();
      set => numberOfLinesPerPage = value;
    }

    /// <summary>
    /// A value of TotalAmountToFamily.
    /// </summary>
    [JsonPropertyName("totalAmountToFamily")]
    public Common TotalAmountToFamily
    {
      get => totalAmountToFamily ??= new();
      set => totalAmountToFamily = value;
    }

    /// <summary>
    /// A value of AmountForCollDate.
    /// </summary>
    [JsonPropertyName("amountForCollDate")]
    public Common AmountForCollDate
    {
      get => amountForCollDate ??= new();
      set => amountForCollDate = value;
    }

    /// <summary>
    /// A value of AmountCollectedOffset.
    /// </summary>
    [JsonPropertyName("amountCollectedOffset")]
    public Common AmountCollectedOffset
    {
      get => amountCollectedOffset ??= new();
      set => amountCollectedOffset = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// Gets a value of ArAddress.
    /// </summary>
    [JsonIgnore]
    public Array<ArAddressGroup> ArAddress => arAddress ??= new(
      ArAddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ArAddress for json serialization.
    /// </summary>
    [JsonPropertyName("arAddress")]
    [Computed]
    public IList<ArAddressGroup> ArAddress_Json
    {
      get => arAddress;
      set => ArAddress.Assign(value);
    }

    /// <summary>
    /// Gets a value of Print.
    /// </summary>
    [JsonIgnore]
    public Array<PrintGroup> Print => print ??= new(PrintGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Print for json serialization.
    /// </summary>
    [JsonPropertyName("print")]
    [Computed]
    public IList<PrintGroup> Print_Json
    {
      get => print;
      set => Print.Assign(value);
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
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// Gets a value of OfficeAddress.
    /// </summary>
    [JsonIgnore]
    public Array<OfficeAddressGroup> OfficeAddress => officeAddress ??= new(
      OfficeAddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OfficeAddress for json serialization.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    [Computed]
    public IList<OfficeAddressGroup> OfficeAddress_Json
    {
      get => officeAddress;
      set => OfficeAddress.Assign(value);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PreviousAr.
    /// </summary>
    [JsonPropertyName("previousAr")]
    public CsePerson PreviousAr
    {
      get => previousAr ??= new();
      set => previousAr = value;
    }

    /// <summary>
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Collection Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public DateWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEnding.
    /// </summary>
    [JsonPropertyName("reportingPeriodEnding")]
    public DateWorkArea ReportingPeriodEnding
    {
      get => reportingPeriodEnding ??= new();
      set => reportingPeriodEnding = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Run.
    /// </summary>
    [JsonPropertyName("run")]
    public DateWorkArea Run
    {
      get => run ??= new();
      set => run = value;
    }

    /// <summary>
    /// A value of RangeBeg.
    /// </summary>
    [JsonPropertyName("rangeBeg")]
    public DateWorkArea RangeBeg
    {
      get => rangeBeg ??= new();
      set => rangeBeg = value;
    }

    /// <summary>
    /// Gets a value of GlocalCashIndicator.
    /// </summary>
    [JsonIgnore]
    public Array<GlocalCashIndicatorGroup> GlocalCashIndicator =>
      glocalCashIndicator ??= new(GlocalCashIndicatorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of GlocalCashIndicator for json serialization.
    /// </summary>
    [JsonPropertyName("glocalCashIndicator")]
    [Computed]
    public IList<GlocalCashIndicatorGroup> GlocalCashIndicator_Json
    {
      get => glocalCashIndicator;
      set => GlocalCashIndicator.Assign(value);
    }

    /// <summary>
    /// A value of NumberOfErrors.
    /// </summary>
    [JsonPropertyName("numberOfErrors")]
    public Common NumberOfErrors
    {
      get => numberOfErrors ??= new();
      set => numberOfErrors = value;
    }

    private Common non718BFound;
    private Common tafMoneyFoundFlag;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet ch;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CsePersonAccount obligor1;
    private ObligationType obligationType;
    private DateWorkArea null1;
    private CsePerson derivedAr;
    private DateWorkArea dueDate;
    private Infrastructure infrastructure;
    private Common caseFound;
    private DebtDetail debtDetail;
    private DprProgram dprProgram;
    private Program program;
    private SpPrintWorkSet name;
    private FieldValue fieldValue;
    private Common lastOfImportGroup;
    private Common subscriptForFooter;
    private Common remainder;
    private Common numberOfPages;
    private Common numberOfLinesPerPage;
    private Common totalAmountToFamily;
    private Common amountForCollDate;
    private Common amountCollectedOffset;
    private Common common;
    private Collection collection;
    private TextWorkArea textWorkArea;
    private Array<ArAddressGroup> arAddress;
    private Array<PrintGroup> print;
    private CsePersonAddress arCsePersonAddress;
    private SpPrintWorkSet spPrintWorkSet;
    private Array<OfficeAddressGroup> officeAddress;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson previousAr;
    private CsePerson previousObligor;
    private Collection previous;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private DateWorkArea reportingPeriodStarting;
    private DateWorkArea reportingPeriodEnding;
    private CsePerson obligor2;
    private CsePerson restart;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea run;
    private DateWorkArea rangeBeg;
    private Array<GlocalCashIndicatorGroup> glocalCashIndicator;
    private Common numberOfErrors;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of N2dReadObligorCsePerson.
    /// </summary>
    [JsonPropertyName("n2dReadObligorCsePerson")]
    public CsePerson N2dReadObligorCsePerson
    {
      get => n2dReadObligorCsePerson ??= new();
      set => n2dReadObligorCsePerson = value;
    }

    /// <summary>
    /// A value of N2dReadObligationType.
    /// </summary>
    [JsonPropertyName("n2dReadObligationType")]
    public ObligationType N2dReadObligationType
    {
      get => n2dReadObligationType ??= new();
      set => n2dReadObligationType = value;
    }

    /// <summary>
    /// A value of N2dReadDebt.
    /// </summary>
    [JsonPropertyName("n2dReadDebt")]
    public ObligationTransaction N2dReadDebt
    {
      get => n2dReadDebt ??= new();
      set => n2dReadDebt = value;
    }

    /// <summary>
    /// A value of N2dReadObligorObligor.
    /// </summary>
    [JsonPropertyName("n2dReadObligorObligor")]
    public CsePersonAccount N2dReadObligorObligor
    {
      get => n2dReadObligorObligor ??= new();
      set => n2dReadObligorObligor = value;
    }

    /// <summary>
    /// A value of N2dReadObligation.
    /// </summary>
    [JsonPropertyName("n2dReadObligation")]
    public Obligation N2dReadObligation
    {
      get => n2dReadObligation ??= new();
      set => n2dReadObligation = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress1.
    /// </summary>
    [JsonPropertyName("officeAddress1")]
    public OfficeAddress OfficeAddress1
    {
      get => officeAddress1 ??= new();
      set => officeAddress1 = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashIndicatorCashReceiptType.
    /// </summary>
    [JsonPropertyName("cashIndicatorCashReceiptType")]
    public CashReceiptType CashIndicatorCashReceiptType
    {
      get => cashIndicatorCashReceiptType ??= new();
      set => cashIndicatorCashReceiptType = value;
    }

    /// <summary>
    /// A value of CashIndicatorCollectionType.
    /// </summary>
    [JsonPropertyName("cashIndicatorCollectionType")]
    public CollectionType CashIndicatorCollectionType
    {
      get => cashIndicatorCollectionType ??= new();
      set => cashIndicatorCollectionType = value;
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
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public Collection Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    private CsePerson n2dReadObligorCsePerson;
    private ObligationType n2dReadObligationType;
    private ObligationTransaction n2dReadDebt;
    private CsePersonAccount n2dReadObligorObligor;
    private Obligation n2dReadObligation;
    private CsePersonAccount supported1;
    private ObligationType obligationType;
    private CsePerson supported2;
    private ObligationTransaction debt;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Obligation obligation;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePerson ar;
    private Case1 case1;
    private CaseRole caseRole;
    private Office office;
    private OfficeAddress officeAddress1;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashIndicatorCashReceiptType;
    private CollectionType cashIndicatorCollectionType;
    private CollectionType collectionType;
    private Collection adjusted;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
  }
#endregion
}
