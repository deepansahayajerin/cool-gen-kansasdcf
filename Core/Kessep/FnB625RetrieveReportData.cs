// Program: FN_B625_RETRIEVE_REPORT_DATA, ID: 373021321, model: 746.
// Short name: SWE01010
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
/// A program: FN_B625_RETRIEVE_REPORT_DATA.
/// </para>
/// <para>
///  This cab retrieves data for the Arrearage Affidavit report.
///    It sets up an array of months for each month within the report date 
/// range.
///    It then retrieves all open cases related to the import court order 
/// standard number.
///    It then retrieves debts, collections, and adjustments for each month 
/// within
///    the report date range.
///    The end result is that the date array contains activity, as well as a 
/// month end
///    balance, for each month/year entry in the array.
/// </para>
/// </summary>
[Serializable]
public partial class FnB625RetrieveReportData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B625_RETRIEVE_REPORT_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB625RetrieveReportData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB625RetrieveReportData.
  /// </summary>
  public FnB625RetrieveReportData(IContext context, Import import, Export export)
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
    // : This cab retrieves data for the Arrearage Affidavit report.
    //    It sets up an array of months for each month within the report date 
    // range.
    //    It then retrieves all open cases related to the import court order 
    // standard number.
    //    It then retrieves debts, collections, and adjustments for each month 
    // within
    //    the report date range.
    //    The end result is that the date array contains activity, as well as a 
    // month end
    //    balance, for each month/year entry in the array.
    // : PR# 149349, M. Brown, July, 2002
    //   Get lowest debt detail due date and if it is greater than the 'from' 
    // date
    //   entered by the user, replace the 'from' date with this date.
    // : PR# 158654, M. Brown, July, 2002
    //   Check debt detail due date for null values when setting up table of 
    // dates (local group).
    // : CQ# 14867, J. Huss, January, 2010
    //   Added obligor criteria to collections read.  This corrected situations 
    // where
    //   multiple obligors were tied to the same court order number.  In these 
    // cases,
    //   collections for all obligors were being credited to the obligor in the 
    // report.
    local.ProgramProcessingInfo.ProcessDate = import.Current.Date;
    local.HardcodeInterestJudgement.SystemGeneratedIdentifier = 12;
    UseCabFirstAndLastDateOfMonth2();
    UseCabFirstAndLastDateOfMonth1();
    UseFnCabDetermineRptDateRange1();

    // ***** MAIN-LINE AREA *****
    if (ReadCsePerson())
    {
      if (AsChar(entities.Obligor2.Type1) == 'C')
      {
        export.CsePersonsWorkSet.Number = entities.Obligor2.Number;
        UseSiReadCsePersonBatch();
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.Obligor2.OrganizationName ?? Spaces(33);
      }

      if (!ReadObligor())
      {
        ExitState = "FN0000_OBLIGOR_NF";

        return;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // : Retrieve all open cases for the court order.
    export.Cases.Index = -1;

    foreach(var item in ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'C' && Lt
        (entities.Case1.StatusDate, import.SearchTo.Date))
      {
        continue;
      }

      if (!Equal(entities.Case1.Number, local.PreviousCase.Number))
      {
        local.PreviousCase.Number = entities.Case1.Number;

        ++export.Cases.Index;
        export.Cases.CheckSize();

        export.Cases.Update.Case1.Number = entities.Case1.Number;
      }
    }

    // : Initialize group view with yymm, 1 entry for each yymm within the date 
    // range.
    // : PR# 149349, M. Brown, July, 2002
    //   Get lowest debt detail due date and if it is greater than the 'from' 
    // date
    //   entered by the user, replace the 'from' date with this date.
    ReadDebtDetail();

    if (Lt(local.FromDate.Date, local.DebtDtlMin.Date))
    {
      local.FromDate.Date = local.DebtDtlMin.Date;
    }

    // mlb - PR00162723 - 08/04/2005
    // The date being used must be the first of the month, for the from date 
    // filter. If it is not, the
    // progression for the table will be incorrect. E.G. 04/11/2002 with a month
    // added and a day subtracted will
    // yield 05/10/2002. This is not what is wanted, if it is necessary to find 
    // all amounts within the month.
    local.WorkDate.Month = Month(local.FromDate.Date);
    local.WorkDate.Year = Year(local.FromDate.Date);
    local.WorkDate.Day = 1;
    local.FromDate.Date = IntToDate(local.WorkDate.Year * 10000 + local
      .WorkDate.Month * 100 + local.WorkDate.Day);

    // end
    local.Group.Index = 0;
    local.Group.CheckSize();

    // : Populate group view with year and month literals, as well as from and 
    // to dates,
    //   to help with data retrieval.
    MoveDateWorkArea(local.FromDate, local.SaveFromDate);

    do
    {
      local.Group.Update.From.Date = local.FromDate.Date;
      local.Group.Update.To.Date =
        AddDays(AddMonths(local.Group.Item.From.Date, 1), -1);

      // : Set up from/to timestamps.
      UseFnCabDetermineRptDateRange2();

      switch(Month(local.Group.Item.From.Date))
      {
        case 1:
          local.MonthAbbreviation.Text4 = "Jan";

          break;
        case 2:
          local.MonthAbbreviation.Text4 = "Feb";

          break;
        case 3:
          local.MonthAbbreviation.Text4 = "Mar";

          break;
        case 4:
          local.MonthAbbreviation.Text4 = "Apr";

          break;
        case 5:
          local.MonthAbbreviation.Text4 = "May";

          break;
        case 6:
          local.MonthAbbreviation.Text4 = "Jun";

          break;
        case 7:
          local.MonthAbbreviation.Text4 = "Jul";

          break;
        case 8:
          local.MonthAbbreviation.Text4 = "Aug";

          break;
        case 9:
          local.MonthAbbreviation.Text4 = "Sep";

          break;
        case 10:
          local.MonthAbbreviation.Text4 = "Oct";

          break;
        case 11:
          local.MonthAbbreviation.Text4 = "Nov";

          break;
        case 12:
          local.MonthAbbreviation.Text4 = "Dec";

          break;
        default:
          break;
      }

      local.Group.Update.Yymm.Text8 = local.MonthAbbreviation.Text4 + NumberToString
        (Year(local.Group.Item.From.Date), 12, 4);

      if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
      {
        break;
      }

      ++local.Group.Index;
      local.Group.CheckSize();

      local.FromDate.Date = AddMonths(local.FromDate.Date, 1);
    }
    while(!Lt(local.To.Date, local.FromDate.Date));

    MoveDateWorkArea(local.SaveFromDate, local.FromDate);

    // : Get the current amount owed for the Court Order.
    local.OmitUndistInd.Flag = "Y";
    local.OmitCrdInd.Flag = "Y";
    local.OmitUnprocInd.Flag = "Y";
    UseFnComputeSummaryTotals();
    export.ScreenOwedAmounts.TotalAmountOwed =
      local.BeginBalance.TotalAmountOwed;

    // : Now get the data for the report, and put it in the year/month table.
    // : New Debts, Debt Adjustments and Collections.
    foreach(var item in ReadObligationTransactionObligationTypeDebtDetail())
    {
      local.Found.Flag = "N";

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (!Lt(entities.DebtDetail.DueDt, local.Group.Item.From.Date) && !
          Lt(local.Group.Item.To.Date, entities.DebtDetail.DueDt))
        {
          local.Found.Flag = "Y";

          break;
        }
      }

      local.Group.CheckIndex();

      if (AsChar(local.Found.Flag) == 'N')
      {
        // : This flag should always be 'Y'.
        local.NeededToWrite.RptDetail = "Date entry not found in table.";
        UseCabErrorReport();

        return;
      }

      if (entities.ObligationType.SystemGeneratedIdentifier == local
        .HardcodeInterestJudgement.SystemGeneratedIdentifier)
      {
        local.Group.Update.NetInterest.TotalCurrency =
          local.Group.Item.NetInterest.TotalCurrency + entities
          .ObligationTransaction.Amount;
      }
      else
      {
        local.Group.Update.NewDebts.TotalCurrency =
          local.Group.Item.NewDebts.TotalCurrency + entities
          .ObligationTransaction.Amount;
      }

      export.TotalDebts.TotalCurrency += entities.ObligationTransaction.Amount;

      // : Are there any debt adjustments for the current debt?
      foreach(var item1 in ReadDebtAdjustmentObligationTransactionRln())
      {
        if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
        {
          local.Group.Update.DebtAdj.TotalCurrency =
            local.Group.Item.DebtAdj.TotalCurrency + entities
            .DebtAdjustment.Amount;
          export.TotalAdjustments.TotalCurrency += entities.DebtAdjustment.
            Amount;
        }
        else
        {
          local.Group.Update.DebtAdj.TotalCurrency =
            local.Group.Item.DebtAdj.TotalCurrency - entities
            .DebtAdjustment.Amount;
          export.TotalAdjustments.TotalCurrency -= entities.DebtAdjustment.
            Amount;
        }

        if (AsChar(import.CommentsInd.Flag) == 'Y')
        {
          if (!IsEmpty(entities.ObligationTransactionRln.Description))
          {
            if (local.Group.Item.Comments.Count >= Local.CommentsGroup.Capacity)
            {
              goto Test;
            }

            local.Group.Item.Comments.Index = local.Group.Item.Comments.Count;
            local.Group.Item.Comments.CheckSize();

            local.Group.Update.Comments.Update.Comment.Text24 =
              Substring(entities.ObligationTransactionRln.Description, 1, 20);

            // : Use 2 lines of comments for the description, if necessary.
            if (!IsEmpty(TrimEnd(
              Substring(entities.ObligationTransactionRln.Description, 21, 20))))
              
            {
              if (local.Group.Item.Comments.Count >= Local
                .CommentsGroup.Capacity)
              {
                goto Test;
              }

              local.Group.Item.Comments.Index = local.Group.Item.Comments.Count;
              local.Group.Item.Comments.CheckSize();

              local.Group.Update.Comments.Update.Comment.Text24 =
                Substring(entities.ObligationTransactionRln.Description, 21, 20);
                
            }
          }
        }

Test:
        ;
      }
    }

    // 1/15/2010, CQ# 14867, J. Huss, If the current obligation is not joint/
    // several, then it must be owed by the obligor.
    // : Accumulate Net Collections
    foreach(var item in ReadCollection())
    {
      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (!Lt(entities.Collection.CollectionDt, local.Group.Item.From.Date) &&
          !Lt(local.Group.Item.To.Date, entities.Collection.CollectionDt))
        {
          local.Group.Update.NetCollection.TotalCurrency =
            local.Group.Item.NetCollection.TotalCurrency + entities
            .Collection.Amount;
          export.TotalCollections.TotalCurrency += entities.Collection.Amount;

          goto ReadEach;
        }
      }

      local.Group.CheckIndex();

ReadEach:
      ;
    }

    // : Set export group, and also export group eom balances.
    export.BeginBalance.TotalCurrency =
      export.ScreenOwedAmounts.TotalAmountOwed;

    for(local.Group.Index = local.Group.Count - 1; local.Group.Index >= 0; --
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = local.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.DebtAdj.TotalCurrency =
        local.Group.Item.DebtAdj.TotalCurrency;
      export.Group.Update.NetCollection.TotalCurrency =
        local.Group.Item.NetCollection.TotalCurrency;
      export.Group.Update.NewDebts.TotalCurrency =
        local.Group.Item.NewDebts.TotalCurrency;
      export.Group.Update.NetInterest.TotalCurrency =
        local.Group.Item.NetInterest.TotalCurrency;
      export.Group.Update.Yymm.Text8 = local.Group.Item.Yymm.Text8;
      export.Group.Update.EomBal.TotalCurrency =
        export.BeginBalance.TotalCurrency;
      export.BeginBalance.TotalCurrency = export.BeginBalance.TotalCurrency + local
        .Group.Item.NetCollection.TotalCurrency - local
        .Group.Item.NetInterest.TotalCurrency - local
        .Group.Item.NewDebts.TotalCurrency - local
        .Group.Item.DebtAdj.TotalCurrency;

      if (AsChar(import.CommentsInd.Flag) == 'Y')
      {
        for(local.Group.Item.Comments.Index = 0; local
          .Group.Item.Comments.Index < local.Group.Item.Comments.Count; ++
          local.Group.Item.Comments.Index)
        {
          if (!local.Group.Item.Comments.CheckSize())
          {
            break;
          }

          export.Group.Item.Comments.Index = local.Group.Item.Comments.Index;
          export.Group.Item.Comments.CheckSize();

          export.Group.Update.Comments.Update.Comment.Text24 =
            local.Group.Item.Comments.Item.Comment.Text24;
        }

        local.Group.Item.Comments.CheckIndex();
      }
    }

    local.Group.CheckIndex();

    if (ReadTribunal())
    {
      export.Tribunal.JudicialDivision = entities.Tribunal.JudicialDivision;

      if (ReadFips())
      {
        export.Fips.CountyDescription = entities.Fips.CountyDescription;
      }
    }

    // : Get the court caption.
    export.Caption.Index = 0;
    export.Caption.Clear();

    foreach(var item in ReadCourtCaption())
    {
      if (Equal(entities.CourtCaption.Line, local.PreviousCourtCaption.Line))
      {
        export.Caption.Next();

        continue;
      }

      local.PreviousCourtCaption.Line = entities.CourtCaption.Line;
      MoveCourtCaption(entities.CourtCaption, export.Caption.Update.CourtCaption);
        
      export.Caption.Next();
    }
  }

  private static void MoveCourtCaption(CourtCaption source, CourtCaption target)
  {
    target.Number = source.Number;
    target.Line = source.Line;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.Current.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.To.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = import.SearchFrom.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.FromDate.Date = useExport.First.Date;
  }

  private void UseFnCabDetermineRptDateRange1()
  {
    var useImport = new FnCabDetermineRptDateRange.Import();
    var useExport = new FnCabDetermineRptDateRange.Export();

    useImport.To.Date = local.To.Date;
    useImport.From.Date = local.FromDate.Date;

    Call(FnCabDetermineRptDateRange.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.To, local.To);
    MoveDateWorkArea(useExport.From, local.FromDate);
  }

  private void UseFnCabDetermineRptDateRange2()
  {
    var useImport = new FnCabDetermineRptDateRange.Import();
    var useExport = new FnCabDetermineRptDateRange.Export();

    useImport.From.Date = local.Group.Item.From.Date;
    useImport.To.Date = local.Group.Item.To.Date;

    Call(FnCabDetermineRptDateRange.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.From, local.Group.Update.From);
    MoveDateWorkArea(useExport.To, local.Group.Update.To);
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = entities.Obligor2.Number;
    useImport.FilterByStdNo.StandardNumber = import.Search.StandardNumber;
    useImport.OmitUnprocTrnCheckInd.Flag = local.OmitUnprocInd.Flag;
    useImport.OmitCrdInd.Flag = local.OmitCrdInd.Flag;
    useImport.OmitUndistAmtInd.Flag = local.OmitUndistInd.Flag;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.BeginBalance.TotalAmountOwed =
      useExport.ScreenOwedAmounts.TotalAmountOwed;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor2.Number);
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber", entities.Obligor1.CspNumber);
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCourtCaption()
  {
    return ReadEach("ReadCourtCaption",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Caption.IsFull)
        {
          return false;
        }

        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.CourtCaption.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Obligor2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SearchAp.Number);
      },
      (db, reader) =>
      {
        entities.Obligor2.Number = db.GetString(reader, 0);
        entities.Obligor2.Type1 = db.GetString(reader, 1);
        entities.Obligor2.OrganizationName = db.GetNullableString(reader, 2);
        entities.Obligor2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Obligor2.Type1);
      });
  }

  private IEnumerable<bool> ReadDebtAdjustmentObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtAdjustment.Populated = false;
    entities.ObligationTransactionRln.Populated = false;

    return ReadEach("ReadDebtAdjustmentObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypePrimary", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrPType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaPType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspPNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 7);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 8);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 9);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 9);
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 11);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 12);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 13);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 14);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 15);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.ObligationTransactionRln.CreatedTmst =
          db.GetDateTime(reader, 17);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 18);
        entities.ObligationTransactionRln.Description =
          db.GetNullableString(reader, 19);
        entities.DebtAdjustment.Populated = true;
        entities.ObligationTransactionRln.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);

        return true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber", entities.Obligor1.CspNumber);
        db.SetDate(command, "dueDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        local.DebtDtlMin.Date = db.GetDate(reader, 0);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionObligationTypeDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionObligationTypeDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber", entities.Obligor1.CspNumber);
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
        db.SetDate(command, "date1", local.FromDate.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 6);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetail.DueDt = db.GetDate(reader, 11);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligor()
  {
    entities.Obligor1.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor2.Number);
      },
      (db, reader) =>
      {
        entities.Obligor1.CspNumber = db.GetString(reader, 0);
        entities.Obligor1.Type1 = db.GetString(reader, 1);
        entities.Obligor1.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor1.Type1);
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
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
    /// A value of SearchAp.
    /// </summary>
    [JsonPropertyName("searchAp")]
    public CsePersonsWorkSet SearchAp
    {
      get => searchAp ??= new();
      set => searchAp = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of CommentsInd.
    /// </summary>
    [JsonPropertyName("commentsInd")]
    public Common CommentsInd
    {
      get => commentsInd ??= new();
      set => commentsInd = value;
    }

    private CsePersonsWorkSet searchAp;
    private LegalAction search;
    private DateWorkArea current;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common commentsInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CaptionGroup group.</summary>
    [Serializable]
    public class CaptionGroup
    {
      /// <summary>
      /// A value of CourtCaption.
      /// </summary>
      [JsonPropertyName("courtCaption")]
      public CourtCaption CourtCaption
      {
        get => courtCaption ??= new();
        set => courtCaption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CourtCaption courtCaption;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Yymm.
      /// </summary>
      [JsonPropertyName("yymm")]
      public WorkArea Yymm
      {
        get => yymm ??= new();
        set => yymm = value;
      }

      /// <summary>
      /// A value of NetInterest.
      /// </summary>
      [JsonPropertyName("netInterest")]
      public Common NetInterest
      {
        get => netInterest ??= new();
        set => netInterest = value;
      }

      /// <summary>
      /// A value of DebtAdj.
      /// </summary>
      [JsonPropertyName("debtAdj")]
      public Common DebtAdj
      {
        get => debtAdj ??= new();
        set => debtAdj = value;
      }

      /// <summary>
      /// A value of NetCollection.
      /// </summary>
      [JsonPropertyName("netCollection")]
      public Common NetCollection
      {
        get => netCollection ??= new();
        set => netCollection = value;
      }

      /// <summary>
      /// A value of NewDebts.
      /// </summary>
      [JsonPropertyName("newDebts")]
      public Common NewDebts
      {
        get => newDebts ??= new();
        set => newDebts = value;
      }

      /// <summary>
      /// A value of EomBal.
      /// </summary>
      [JsonPropertyName("eomBal")]
      public Common EomBal
      {
        get => eomBal ??= new();
        set => eomBal = value;
      }

      /// <summary>
      /// Gets a value of Comments.
      /// </summary>
      [JsonIgnore]
      public Array<CommentsGroup> Comments => comments ??= new(
        CommentsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Comments for json serialization.
      /// </summary>
      [JsonPropertyName("comments")]
      [Computed]
      public IList<CommentsGroup> Comments_Json
      {
        get => comments;
        set => Comments.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private WorkArea yymm;
      private Common netInterest;
      private Common debtAdj;
      private Common netCollection;
      private Common newDebts;
      private Common eomBal;
      private Array<CommentsGroup> comments;
    }

    /// <summary>A CommentsGroup group.</summary>
    [Serializable]
    public class CommentsGroup
    {
      /// <summary>
      /// A value of Comment.
      /// </summary>
      [JsonPropertyName("comment")]
      public WorkArea Comment
      {
        get => comment ??= new();
        set => comment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private WorkArea comment;
    }

    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 case1;
    }

    /// <summary>
    /// Gets a value of Caption.
    /// </summary>
    [JsonIgnore]
    public Array<CaptionGroup> Caption =>
      caption ??= new(CaptionGroup.Capacity);

    /// <summary>
    /// Gets a value of Caption for json serialization.
    /// </summary>
    [JsonPropertyName("caption")]
    [Computed]
    public IList<CaptionGroup> Caption_Json
    {
      get => caption;
      set => Caption.Assign(value);
    }

    /// <summary>
    /// A value of BeginBalance.
    /// </summary>
    [JsonPropertyName("beginBalance")]
    public Common BeginBalance
    {
      get => beginBalance ??= new();
      set => beginBalance = value;
    }

    /// <summary>
    /// A value of TotalAdjustments.
    /// </summary>
    [JsonPropertyName("totalAdjustments")]
    public Common TotalAdjustments
    {
      get => totalAdjustments ??= new();
      set => totalAdjustments = value;
    }

    /// <summary>
    /// A value of TotalDebts.
    /// </summary>
    [JsonPropertyName("totalDebts")]
    public Common TotalDebts
    {
      get => totalDebts ??= new();
      set => totalDebts = value;
    }

    /// <summary>
    /// A value of TotalCollections.
    /// </summary>
    [JsonPropertyName("totalCollections")]
    public Common TotalCollections
    {
      get => totalCollections ??= new();
      set => totalCollections = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Array<CaptionGroup> caption;
    private Common beginBalance;
    private Common totalAdjustments;
    private Common totalDebts;
    private Common totalCollections;
    private Array<GroupGroup> group;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ScreenOwedAmounts screenOwedAmounts;
    private Array<CasesGroup> cases;
    private Tribunal tribunal;
    private Fips fips;
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
      /// A value of Yymm.
      /// </summary>
      [JsonPropertyName("yymm")]
      public WorkArea Yymm
      {
        get => yymm ??= new();
        set => yymm = value;
      }

      /// <summary>
      /// A value of NetInterest.
      /// </summary>
      [JsonPropertyName("netInterest")]
      public Common NetInterest
      {
        get => netInterest ??= new();
        set => netInterest = value;
      }

      /// <summary>
      /// A value of DebtAdj.
      /// </summary>
      [JsonPropertyName("debtAdj")]
      public Common DebtAdj
      {
        get => debtAdj ??= new();
        set => debtAdj = value;
      }

      /// <summary>
      /// A value of NetCollection.
      /// </summary>
      [JsonPropertyName("netCollection")]
      public Common NetCollection
      {
        get => netCollection ??= new();
        set => netCollection = value;
      }

      /// <summary>
      /// A value of NewDebts.
      /// </summary>
      [JsonPropertyName("newDebts")]
      public Common NewDebts
      {
        get => newDebts ??= new();
        set => newDebts = value;
      }

      /// <summary>
      /// A value of From.
      /// </summary>
      [JsonPropertyName("from")]
      public DateWorkArea From
      {
        get => from ??= new();
        set => from = value;
      }

      /// <summary>
      /// A value of To.
      /// </summary>
      [JsonPropertyName("to")]
      public DateWorkArea To
      {
        get => to ??= new();
        set => to = value;
      }

      /// <summary>
      /// Gets a value of Comments.
      /// </summary>
      [JsonIgnore]
      public Array<CommentsGroup> Comments => comments ??= new(
        CommentsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Comments for json serialization.
      /// </summary>
      [JsonPropertyName("comments")]
      [Computed]
      public IList<CommentsGroup> Comments_Json
      {
        get => comments;
        set => Comments.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private WorkArea yymm;
      private Common netInterest;
      private Common debtAdj;
      private Common netCollection;
      private Common newDebts;
      private DateWorkArea from;
      private DateWorkArea to;
      private Array<CommentsGroup> comments;
    }

    /// <summary>A CommentsGroup group.</summary>
    [Serializable]
    public class CommentsGroup
    {
      /// <summary>
      /// A value of Comment.
      /// </summary>
      [JsonPropertyName("comment")]
      public WorkArea Comment
      {
        get => comment ??= new();
        set => comment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private WorkArea comment;
    }

    /// <summary>
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
    }

    /// <summary>
    /// A value of OmitUnprocInd.
    /// </summary>
    [JsonPropertyName("omitUnprocInd")]
    public Common OmitUnprocInd
    {
      get => omitUnprocInd ??= new();
      set => omitUnprocInd = value;
    }

    /// <summary>
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of OmitUndistInd.
    /// </summary>
    [JsonPropertyName("omitUndistInd")]
    public Common OmitUndistInd
    {
      get => omitUndistInd ??= new();
      set => omitUndistInd = value;
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
    /// A value of MonthAbbreviation.
    /// </summary>
    [JsonPropertyName("monthAbbreviation")]
    public WorkArea MonthAbbreviation
    {
      get => monthAbbreviation ??= new();
      set => monthAbbreviation = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of HardcodeInterestJudgement.
    /// </summary>
    [JsonPropertyName("hardcodeInterestJudgement")]
    public ObligationType HardcodeInterestJudgement
    {
      get => hardcodeInterestJudgement ??= new();
      set => hardcodeInterestJudgement = value;
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
    /// A value of BeginBalance.
    /// </summary>
    [JsonPropertyName("beginBalance")]
    public ScreenOwedAmounts BeginBalance
    {
      get => beginBalance ??= new();
      set => beginBalance = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of DebtDtlMin.
    /// </summary>
    [JsonPropertyName("debtDtlMin")]
    public DateWorkArea DebtDtlMin
    {
      get => debtDtlMin ??= new();
      set => debtDtlMin = value;
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
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateWorkArea FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of SaveFromDate.
    /// </summary>
    [JsonPropertyName("saveFromDate")]
    public DateWorkArea SaveFromDate
    {
      get => saveFromDate ??= new();
      set => saveFromDate = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of PreviousCourtCaption.
    /// </summary>
    [JsonPropertyName("previousCourtCaption")]
    public CourtCaption PreviousCourtCaption
    {
      get => previousCourtCaption ??= new();
      set => previousCourtCaption = value;
    }

    /// <summary>
    /// A value of WorkDate.
    /// </summary>
    [JsonPropertyName("workDate")]
    public DateWorkArea WorkDate
    {
      get => workDate ??= new();
      set => workDate = value;
    }

    private Case1 previousCase;
    private Common omitUnprocInd;
    private Common omitCrdInd;
    private Common omitUndistInd;
    private DateWorkArea null1;
    private WorkArea monthAbbreviation;
    private Common found;
    private ObligationType hardcodeInterestJudgement;
    private Array<GroupGroup> group;
    private ScreenOwedAmounts beginBalance;
    private DateWorkArea to;
    private DateWorkArea debtDtlMin;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea fromDate;
    private DateWorkArea saveFromDate;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private CourtCaption previousCourtCaption;
    private DateWorkArea workDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
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
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private DebtDetail debtDetail;
    private ObligationTransaction debtAdjustment;
    private CaseRole absentParent;
    private CourtCaption courtCaption;
    private CsePersonAccount obligor1;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private CsePerson obligor2;
    private Obligation obligation;
    private ObligationType obligationType;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private ObligationTransactionRln obligationTransactionRln;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private CaseRole caseRole;
    private Tribunal tribunal;
    private Fips fips;
  }
#endregion
}
