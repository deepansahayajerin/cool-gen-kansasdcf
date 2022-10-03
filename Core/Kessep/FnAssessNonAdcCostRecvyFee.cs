// Program: FN_ASSESS_NON_ADC_COST_RECVY_FEE, ID: 372544592, model: 746.
// Short name: SWE00266
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSESS_NON_ADC_COST_RECVY_FEE.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssessNonAdcCostRecvyFee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSESS_NON_ADC_COST_RECVY_FEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssessNonAdcCostRecvyFee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssessNonAdcCostRecvyFee.
  /// </summary>
  public FnAssessNonAdcCostRecvyFee(IContext context, Import import,
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
    // ***************************************************
    // A.Kinney  05/01/97	Changed Current_Date
    // RK  7/22/99  Added code to handle negitive amounts(adjustments).
    // 1999-11-02  PR 77907  Fangman  Set Disbursement Cash_Non_Cash indicator 
    // to 'C'.
    // 1999-12-07  PR 82091  Fangman  Change hardcoded cost recovery fee from 2%
    // to 4% to take effect on 1-1-2000.
    // 2000-01-20  PR 85878  Fangman  Change CR Fee to be based on collection 
    // date instead of process date.
    // 2000-02-14  PR 86861  Fangman  Changed code for determining if cost 
    // recovery fee is to be taken along with some restructuring of code in 651.
    // 2000-03-09  PR 90356 & 90308  Fangman  Fix problem with logic - expecting
    // a monthly court order fee row for non- Johnson County adjustments.
    // 2000-03-16  PR 91083  Fangman  Fix problem with logic - skipping the CR 
    // FEE if the monthly cap has already been met.  We do not want to do this
    // if the CR FEE is negative.
    // 2000-05-09  PRWORA  Fangman  As part of this project I am also changing 
    // the code that looks at adjustments to use amt < 0 instead of the adj
    // indicator to deterimine if the disb being processed is an adjustment.
    // 2000-09-27  PR 98039 - As part of the project to prevent duplicate 
    // payments I changed the AB call to create a disbursement to use the new
    // AB.
    // 2001-01-10  PR 108247 - Changed logic to use Legal_Action Standard_Number
    // instead of Leagal_Action Court_Order_Number.
    // 2015-11-24  CQ50349 GVandy  Default cost recovery fee rate to 0% and 
    // override with REFI values.
    // ***************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.RemainingDisbursement.Amount = import.PerCollection1.Amount;

    if (export.RemainingDisbursement.Amount == 0)
    {
      return;
    }

    // *****  Set the default rate & cap  *****
    local.TribunalFeeInformation.Cap = 0;

    // 2015-11-24  CQ50349 GVandy  Default cost recovery fee rate to 0% and 
    // override with REFI values.
    if (Lt(new DateTime(2017, 6, 30), import.PerCollection1.CollectionDate))
    {
      local.TribunalFeeInformation.Rate = 0;
    }
    else if (Lt(new DateTime(1999, 12, 31), import.PerCollection1.CollectionDate))
      
    {
      local.TribunalFeeInformation.Rate = 4;
    }
    else
    {
      local.TribunalFeeInformation.Rate = 2;
    }

    // *****  Find out if there is a cap  *****
    if (ReadLegalAction())
    {
      if (ReadTribunal())
      {
        if (ReadFips())
        {
          if (entities.Fips.State == 20)
          {
            if (ReadTribunalFeeInformation())
            {
              MoveTribunalFeeInformation(entities.TribunalFeeInformation,
                local.TribunalFeeInformation);

              if (AsChar(import.TestDisplay.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "Found Trib Fee Info for Legal Action " + NumberToString
                  (entities.LegalAction.Identifier, 15) + "  Tribunal " + NumberToString
                  (entities.Tribunal.Identifier, 15) + "  Rate " + NumberToString
                  ((long)(local.TribunalFeeInformation.Rate.
                    GetValueOrDefault() * 100), 15) + "  Cap " + NumberToString
                  ((long)(local.TribunalFeeInformation.Cap.GetValueOrDefault() *
                  100), 15);
              }
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Using default CR Fee rate and cap. (tribunal fee information NF)";
                
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Using default CR Fee rate and cap. (Not the state of KS)";
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "Using default CR Fee rate and cap.  (FIPS NF)";
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "Using default CR Fee rate and cap.  (Tribunal NF)";
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Using default CR Fee rate and cap.  (Legan Action NF)";
    }

    if (AsChar(import.TestDisplay.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // *****  Calculate the CR FEE to be taken  *****
    if (import.PerCollection1.Amount < 0)
    {
      // *****  Adustements must have the same CR Fee amount as the original.  
      // *****
      if (ReadDisbursementTransaction1())
      {
        if (ReadDisbursementTransaction2())
        {
          local.WorkDisbFee.Amount =
            -entities.PreAdjustmentCrFeeDisbDebit.Amount;
        }
        else
        {
          // *****  Ok, just means there's no original CR fee to mimick for the 
          // adjustment.  *****
          return;
        }
      }
      else
      {
        ExitState = "FN0000_DISB_CREDIT_NF_FOR_ADJUST";

        return;
      }
    }
    else
    {
      // *****  Since the Rate is stored in the table as X.Y (ex. 1.5), devide 
      // it by 100 to get the actual rate  *****
      local.WorkDisbFee.Amount = import.PerCollection1.Amount * local
        .TribunalFeeInformation.Rate.GetValueOrDefault() / 100;

      if (local.WorkDisbFee.Amount < 0.01M)
      {
        // ****  We do not process a fraction of a penney  ****
        return;
      }
    }

    // *****  Update or Create the monthly_court_order_fee  *****
    if (local.TribunalFeeInformation.Cap.GetValueOrDefault() > 0)
    {
      local.CollectionDate.Date = import.PerCollection1.CollectionDate;
      local.CollectionDateYyMm.YearMonth = UseCabGetYearMonthFromDate();

      if (ReadMonthlyCourtOrderFee())
      {
        if (AsChar(import.TestDisplay.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "Found Monthly court order fee " + NumberToString
            ((long)(entities.MonthlyCourtOrderFee.Amount * 100), 15);
          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (entities.MonthlyCourtOrderFee.Amount >= local
          .TribunalFeeInformation.Cap.GetValueOrDefault() && local
          .WorkDisbFee.Amount >= 0)
        {
          // The maximum monthly fee has already been taken so escape.
          if (AsChar(import.TestDisplay.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Escaping from AB without creating a CR FEE for disb of" + NumberToString
              ((long)(import.PerCollection1.Amount * 100), 15);
            local.EabFileHandling.Action = "WRITE";
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          return;
        }
        else if (entities.MonthlyCourtOrderFee.Amount + local
          .WorkDisbFee.Amount > local
          .TribunalFeeInformation.Cap.GetValueOrDefault())
        {
          local.WorkDisbFee.Amount =
            local.TribunalFeeInformation.Cap.GetValueOrDefault() - entities
            .MonthlyCourtOrderFee.Amount;
        }

        local.ForUpdate.Amount = entities.MonthlyCourtOrderFee.Amount + local
          .WorkDisbFee.Amount;

        if (local.ForUpdate.Amount < 0)
        {
          local.ForUpdate.Amount = 0;
        }

        try
        {
          UpdateMonthlyCourtOrderFee();
          export.DatabaseUpdated.Flag = "Y";

          if (AsChar(import.TestDisplay.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Updated Monthly court order fee to " + NumberToString
              ((long)(entities.MonthlyCourtOrderFee.Amount * 100), 15);
            local.EabFileHandling.Action = "WRITE";
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_NURB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_PVRB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        if (import.PerCollection1.Amount < 0)
        {
          // ***** Since this is an adjustment with a legal action then the 
          // original cr fee should have created a monthly court order fee which
          // should now be reduced.  *****
          ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_NF";

          return;
        }

        // *****  No cost recovery fees have been taken for this month so create
        // the monthly record.  *****
        if (local.WorkDisbFee.Amount > local
          .TribunalFeeInformation.Cap.GetValueOrDefault())
        {
          local.WorkDisbFee.Amount =
            local.TribunalFeeInformation.Cap.GetValueOrDefault();
        }

        try
        {
          CreateMonthlyCourtOrderFee();
          export.DatabaseUpdated.Flag = "Y";

          if (AsChar(import.TestDisplay.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Created Monthly court order fee " + NumberToString
              ((long)(entities.MonthlyCourtOrderFee.Amount * 100), 15);
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_AERB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTHLY_COURT_ORDR_FEE_PVRB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    export.RemainingDisbursement.Amount -= local.WorkDisbFee.Amount;
    local.WorkDisbFee.ProcessDate = import.ProgramProcessingInfo.ProcessDate;
    local.WorkDisbFee.DisbursementDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.WorkDisbFee.CashNonCashInd = "C";
    UseFnCreateDisbursementNew();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveTribunalFeeInformation(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.Rate = source.Rate;
    target.Cap = source.Cap;
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

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.CollectionDate.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private void UseFnCreateDisbursementNew()
  {
    var useImport = new FnCreateDisbursementNew.Import();
    var useExport = new FnCreateDisbursementNew.Export();

    useImport.PerObligee.Assign(import.PerObligee);
    useImport.PerCredit.Assign(import.PerCollection1);
    useImport.Per73CrFee.Assign(import.Per73CrFee);
    useImport.DisbursementType.SystemGeneratedIdentifier =
      import.Per73CrFee.SystemGeneratedIdentifier;
    useImport.DisbursementStatus.SystemGeneratedIdentifier =
      import.Per2Processed.SystemGeneratedIdentifier;
    useImport.Per2Processed.Assign(import.Per2Processed);
    useImport.Per1.Assign(import.PerDisbursementTranRlnRsn);
    useImport.Max.Date = import.Max.Date;
    useImport.TestDisplayInd.Flag = import.TestDisplay.Flag;
    useImport.New1.Assign(local.WorkDisbFee);
    useImport.HighestSuppressionDate.Date = local.Initialized.Date;

    Call(FnCreateDisbursementNew.Execute, useImport, useExport);

    import.PerObligee.Assign(useImport.PerObligee);
    import.PerCollection1.Assign(useImport.PerCredit);
    import.Per73CrFee.Assign(useImport.Per73CrFee);
    import.Per2Processed.Assign(useImport.Per2Processed);
    import.PerDisbursementTranRlnRsn.Assign(useImport.Per1);
  }

  private void CreateMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var courtOrderNumber = entities.LegalAction.StandardNumber ?? Spaces(20);
    var yearMonth = local.CollectionDateYyMm.YearMonth;
    var amount = local.WorkDisbFee.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    CheckValid<MonthlyCourtOrderFee>("CpaType", cpaType);
    entities.MonthlyCourtOrderFee.Populated = false;
    Update("CreateMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "courtOrderNumber", courtOrderNumber);
        db.SetInt32(command, "yearMonth", yearMonth);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.MonthlyCourtOrderFee.CpaType = cpaType;
    entities.MonthlyCourtOrderFee.CspNumber = cspNumber;
    entities.MonthlyCourtOrderFee.CourtOrderNumber = courtOrderNumber;
    entities.MonthlyCourtOrderFee.YearMonth = yearMonth;
    entities.MonthlyCourtOrderFee.Amount = amount;
    entities.MonthlyCourtOrderFee.CreatedBy = createdBy;
    entities.MonthlyCourtOrderFee.CreatedTimestamp = createdTimestamp;
    entities.MonthlyCourtOrderFee.LastUpdatedBy = "";
    entities.MonthlyCourtOrderFee.LastUpdatedTmst = null;
    entities.MonthlyCourtOrderFee.Populated = true;
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection2.Populated);
    entities.PreAdjustmentCredit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", import.PerCollection2.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", import.PerCollection2.OtyId);
        db.SetNullableInt32(command, "obgId", import.PerCollection2.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", import.PerCollection2.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", import.PerCollection2.CpaType);
        db.SetNullableInt32(command, "otrId", import.PerCollection2.OtrId);
        db.SetNullableString(
          command, "otrTypeDisb", import.PerCollection2.OtrType);
        db.SetNullableInt32(command, "crtId", import.PerCollection2.CrtType);
        db.SetNullableInt32(command, "cstId", import.PerCollection2.CstId);
        db.SetNullableInt32(command, "crvId", import.PerCollection2.CrvId);
        db.SetNullableInt32(command, "crdId", import.PerCollection2.CrdId);
        db.SetInt32(
          command, "disbTranId",
          import.PerCollection1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PreAdjustmentCredit.CpaType = db.GetString(reader, 0);
        entities.PreAdjustmentCredit.CspNumber = db.GetString(reader, 1);
        entities.PreAdjustmentCredit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PreAdjustmentCredit.Type1 = db.GetString(reader, 3);
        entities.PreAdjustmentCredit.OtyId = db.GetNullableInt32(reader, 4);
        entities.PreAdjustmentCredit.OtrTypeDisb =
          db.GetNullableString(reader, 5);
        entities.PreAdjustmentCredit.OtrId = db.GetNullableInt32(reader, 6);
        entities.PreAdjustmentCredit.CpaTypeDisb =
          db.GetNullableString(reader, 7);
        entities.PreAdjustmentCredit.CspNumberDisb =
          db.GetNullableString(reader, 8);
        entities.PreAdjustmentCredit.ObgId = db.GetNullableInt32(reader, 9);
        entities.PreAdjustmentCredit.CrdId = db.GetNullableInt32(reader, 10);
        entities.PreAdjustmentCredit.CrvId = db.GetNullableInt32(reader, 11);
        entities.PreAdjustmentCredit.CstId = db.GetNullableInt32(reader, 12);
        entities.PreAdjustmentCredit.CrtId = db.GetNullableInt32(reader, 13);
        entities.PreAdjustmentCredit.ColId = db.GetNullableInt32(reader, 14);
        entities.PreAdjustmentCredit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.PreAdjustmentCredit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.PreAdjustmentCredit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.PreAdjustmentCredit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.PreAdjustmentCredit.CpaTypeDisb);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.PreAdjustmentCredit.Populated);
    entities.PreAdjustmentCrFeeDisbDebit.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dbtGeneratedId",
          import.Per73CrFee.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.PreAdjustmentCredit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.PreAdjustmentCredit.CpaType);
        db.SetString(
          command, "cspPNumber", entities.PreAdjustmentCredit.CspNumber);
        db.SetInt32(
          command, "dnrGeneratedId",
          import.PerDisbursementTranRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PreAdjustmentCrFeeDisbDebit.CpaType = db.GetString(reader, 0);
        entities.PreAdjustmentCrFeeDisbDebit.CspNumber =
          db.GetString(reader, 1);
        entities.PreAdjustmentCrFeeDisbDebit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PreAdjustmentCrFeeDisbDebit.Type1 = db.GetString(reader, 3);
        entities.PreAdjustmentCrFeeDisbDebit.Amount = db.GetDecimal(reader, 4);
        entities.PreAdjustmentCrFeeDisbDebit.DbtGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PreAdjustmentCrFeeDisbDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.PreAdjustmentCrFeeDisbDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.PreAdjustmentCrFeeDisbDebit.Type1);
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
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(import.PerObligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          import.PerObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);
    entities.MonthlyCourtOrderFee.Populated = false;

    return Read("ReadMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PerObligee.Type1);
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetInt32(command, "yearMonth", local.CollectionDateYyMm.YearMonth);
        db.SetString(
          command, "courtOrderNumber", entities.LegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.MonthlyCourtOrderFee.CpaType = db.GetString(reader, 0);
        entities.MonthlyCourtOrderFee.CspNumber = db.GetString(reader, 1);
        entities.MonthlyCourtOrderFee.CourtOrderNumber =
          db.GetString(reader, 2);
        entities.MonthlyCourtOrderFee.YearMonth = db.GetInt32(reader, 3);
        entities.MonthlyCourtOrderFee.Amount = db.GetDecimal(reader, 4);
        entities.MonthlyCourtOrderFee.CreatedBy = db.GetString(reader, 5);
        entities.MonthlyCourtOrderFee.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MonthlyCourtOrderFee.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.MonthlyCourtOrderFee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.MonthlyCourtOrderFee.Populated = true;
        CheckValid<MonthlyCourtOrderFee>("CpaType",
          entities.MonthlyCourtOrderFee.CpaType);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetDate(
          command, "effectiveDate",
          import.PerCollection1.CollectionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.TribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.TribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TribunalFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.TribunalFeeInformation.Rate = db.GetNullableDecimal(reader, 3);
        entities.TribunalFeeInformation.Cap = db.GetNullableDecimal(reader, 4);
        entities.TribunalFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.TribunalFeeInformation.Populated = true;
      });
  }

  private void UpdateMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(entities.MonthlyCourtOrderFee.Populated);

    var amount = local.ForUpdate.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.MonthlyCourtOrderFee.Populated = false;
    Update("UpdateMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "cpaType", entities.MonthlyCourtOrderFee.CpaType);
        db.SetString(
          command, "cspNumber", entities.MonthlyCourtOrderFee.CspNumber);
        db.SetString(
          command, "courtOrderNumber",
          entities.MonthlyCourtOrderFee.CourtOrderNumber);
        db.SetInt32(
          command, "yearMonth", entities.MonthlyCourtOrderFee.YearMonth);
      });

    entities.MonthlyCourtOrderFee.Amount = amount;
    entities.MonthlyCourtOrderFee.LastUpdatedBy = lastUpdatedBy;
    entities.MonthlyCourtOrderFee.LastUpdatedTmst = lastUpdatedTmst;
    entities.MonthlyCourtOrderFee.Populated = true;
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
    /// A value of PerObligee.
    /// </summary>
    [JsonPropertyName("perObligee")]
    public CsePersonAccount PerObligee
    {
      get => perObligee ??= new();
      set => perObligee = value;
    }

    /// <summary>
    /// A value of PerCollection1.
    /// </summary>
    [JsonPropertyName("perCollection1")]
    public DisbursementTransaction PerCollection1
    {
      get => perCollection1 ??= new();
      set => perCollection1 = value;
    }

    /// <summary>
    /// A value of PerCollection2.
    /// </summary>
    [JsonPropertyName("perCollection2")]
    public Collection PerCollection2
    {
      get => perCollection2 ??= new();
      set => perCollection2 = value;
    }

    /// <summary>
    /// A value of PerObligation.
    /// </summary>
    [JsonPropertyName("perObligation")]
    public Obligation PerObligation
    {
      get => perObligation ??= new();
      set => perObligation = value;
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
    /// A value of Per73CrFee.
    /// </summary>
    [JsonPropertyName("per73CrFee")]
    public DisbursementType Per73CrFee
    {
      get => per73CrFee ??= new();
      set => per73CrFee = value;
    }

    /// <summary>
    /// A value of Per2Processed.
    /// </summary>
    [JsonPropertyName("per2Processed")]
    public DisbursementStatus Per2Processed
    {
      get => per2Processed ??= new();
      set => per2Processed = value;
    }

    /// <summary>
    /// A value of PerDisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("perDisbursementTranRlnRsn")]
    public DisbursementTranRlnRsn PerDisbursementTranRlnRsn
    {
      get => perDisbursementTranRlnRsn ??= new();
      set => perDisbursementTranRlnRsn = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of TestDisplay.
    /// </summary>
    [JsonPropertyName("testDisplay")]
    public Common TestDisplay
    {
      get => testDisplay ??= new();
      set => testDisplay = value;
    }

    private CsePersonAccount perObligee;
    private DisbursementTransaction perCollection1;
    private Collection perCollection2;
    private Obligation perObligation;
    private ObligationType obligationType;
    private DisbursementType per73CrFee;
    private DisbursementStatus per2Processed;
    private DisbursementTranRlnRsn perDisbursementTranRlnRsn;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private Common testDisplay;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RemainingDisbursement.
    /// </summary>
    [JsonPropertyName("remainingDisbursement")]
    public DisbursementTransaction RemainingDisbursement
    {
      get => remainingDisbursement ??= new();
      set => remainingDisbursement = value;
    }

    /// <summary>
    /// A value of DatabaseUpdated.
    /// </summary>
    [JsonPropertyName("databaseUpdated")]
    public Common DatabaseUpdated
    {
      get => databaseUpdated ??= new();
      set => databaseUpdated = value;
    }

    private DisbursementTransaction remainingDisbursement;
    private Common databaseUpdated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public MonthlyCourtOrderFee ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of CollectionDateYyMm.
    /// </summary>
    [JsonPropertyName("collectionDateYyMm")]
    public DateWorkArea CollectionDateYyMm
    {
      get => collectionDateYyMm ??= new();
      set => collectionDateYyMm = value;
    }

    /// <summary>
    /// A value of WorkDisbFee.
    /// </summary>
    [JsonPropertyName("workDisbFee")]
    public DisbursementTransaction WorkDisbFee
    {
      get => workDisbFee ??= new();
      set => workDisbFee = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private MonthlyCourtOrderFee forUpdate;
    private TribunalFeeInformation tribunalFeeInformation;
    private DateWorkArea collectionDate;
    private DateWorkArea collectionDateYyMm;
    private DisbursementTransaction workDisbFee;
    private DateWorkArea initialized;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PreAdjustmentCrFee.
    /// </summary>
    [JsonPropertyName("preAdjustmentCrFee")]
    public DisbursementType PreAdjustmentCrFee
    {
      get => preAdjustmentCrFee ??= new();
      set => preAdjustmentCrFee = value;
    }

    /// <summary>
    /// A value of PreAdjustmentCrFeeDisbDebit.
    /// </summary>
    [JsonPropertyName("preAdjustmentCrFeeDisbDebit")]
    public DisbursementTransaction PreAdjustmentCrFeeDisbDebit
    {
      get => preAdjustmentCrFeeDisbDebit ??= new();
      set => preAdjustmentCrFeeDisbDebit = value;
    }

    /// <summary>
    /// A value of PreAdjustmentCredit.
    /// </summary>
    [JsonPropertyName("preAdjustmentCredit")]
    public DisbursementTransaction PreAdjustmentCredit
    {
      get => preAdjustmentCredit ??= new();
      set => preAdjustmentCredit = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
    }

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementType preAdjustmentCrFee;
    private DisbursementTransaction preAdjustmentCrFeeDisbDebit;
    private DisbursementTransaction preAdjustmentCredit;
    private Fips fips;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private TribunalFeeInformation tribunalFeeInformation;
    private MonthlyCourtOrderFee monthlyCourtOrderFee;
  }
#endregion
}
