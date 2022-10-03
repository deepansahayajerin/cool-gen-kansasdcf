// Program: FN_B651_DUP_PMT_CR_FEE_CALC, ID: 373293805, model: 746.
// Short name: SWE02740
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_DUP_PMT_CR_FEE_CALC.
/// </summary>
[Serializable]
public partial class FnB651DupPmtCrFeeCalc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_DUP_PMT_CR_FEE_CALC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651DupPmtCrFeeCalc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651DupPmtCrFeeCalc.
  /// </summary>
  public FnB651DupPmtCrFeeCalc(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // 10-16-01  PR 118495  Fangman - Add code to calculate the total amount of 
    // CR Fees for disbursable disbursement collections that will be processed
    // in the current run for an AR and reference number.  This amount will be
    // used in the calculation to determine the amount of funds available to
    // disburse from a collection.
    // 11-24-15  CQ50349    GVandy - Default cost recovery fee rate to 0% and 
    // override with REFI values.
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.FirstTimeThruInd.Flag = "Y";
    export.TotArRefNbrCfFees.TotalCurrency = 0;

    foreach(var item in ReadDisbCollectionCollection())
    {
      if (entities.DisbCollection.Amount < 0)
      {
        // *****  Adjustments must have the same CR Fee amount as the original.
        // *****
        if (ReadDisbursementTransaction1())
        {
          if (ReadDisbursementTransaction2())
          {
            local.WorkCrFee.Amount =
              -entities.PreAdjustmentCrFeeDisbDebit.Amount;
          }
          else
          {
            // *****  Ok, just means there's no original CR fee to mimick for 
            // the adjustment.  *****
            local.WorkCrFee.Amount = 0;
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
        if (AsChar(local.FirstTimeThruInd.Flag) == 'Y')
        {
          local.FirstTimeThruInd.Flag = "N";

          // ****************************************************************
          // Determine if CR Fees should be taken for all positive collections 
          // from this AR/Ref #.  This AB can be called once because the same
          // rules/checks will apply for all collections from the AP to the AR
          // for this reference number.
          // ****************************************************************
          UseFnB651DetIfCrFeeNeeded();

          if (AsChar(local.CrFeeNeededInd.Flag) == 'N')
          {
            break;
          }

          // *****  Set the default rate & cap  *****
          local.TribunalFeeInformation.Cap = 0;

          // 11-24-15 CQ50349 GVandy Default cost recovery fee rate to 0% and 
          // override with REFI values.
          if (Lt(new DateTime(2017, 6, 30), entities.Collection.CollectionDt))
          {
            local.TribunalFeeInformation.Rate = 0;
          }
          else if (Lt(new DateTime(1999, 12, 31),
            entities.Collection.CollectionDt))
          {
            local.TribunalFeeInformation.Rate = 4;
          }
          else
          {
            local.TribunalFeeInformation.Rate = 2;
          }

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

                    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
                    {
                      local.EabReportSend.RptDetail =
                        "      Found Trib Fee Info for Legal Action " + NumberToString
                        (entities.LegalAction.Identifier, 15) + "  Tribunal " +
                        NumberToString(entities.Tribunal.Identifier, 15) + "  Rate " +
                        NumberToString
                        ((long)(local.TribunalFeeInformation.Rate.
                          GetValueOrDefault() * 100), 15) + "  Cap " + NumberToString
                        ((long)(local.TribunalFeeInformation.Cap.
                          GetValueOrDefault() * 100), 15);
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

          if (local.TribunalFeeInformation.Cap.GetValueOrDefault() > 0)
          {
            // *****  Check the monthly_court_order_fee cap  *****
            local.CollectionDate.Date = entities.Collection.CollectionDt;
            local.CollectionDateYyMm.YearMonth = UseCabGetYearMonthFromDate();

            if (ReadMonthlyCourtOrderFee())
            {
              local.MonthlyCourtOrderFee.Amount =
                entities.MonthlyCourtOrderFee.Amount;

              if (AsChar(import.TestDisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "       Found Monthly court order fee " + NumberToString
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
            else
            {
              // Continue
            }
          }
        }

        // *****  Since the Rate is stored in the table as X.Y (ex. 1.5), devide
        // it by 100 to get the actual rate  *****
        local.WorkCrFee.Amount = entities.DisbCollection.Amount * local
          .TribunalFeeInformation.Rate.GetValueOrDefault() / 100;

        if (local.WorkCrFee.Amount < 0.01M)
        {
          // ****  We do not process a fraction of a penney  ****
          local.WorkCrFee.Amount = 0;
        }
      }

      export.TotArRefNbrCfFees.TotalCurrency += local.WorkCrFee.Amount;

      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        if (entities.DisbCollection.Amount < 0)
        {
          local.Sign1.Text2 = " -";
        }
        else
        {
          local.Sign1.Text2 = " +";
        }

        if (local.WorkCrFee.Amount < 0)
        {
          local.Sign2.Text2 = " -";
        }
        else
        {
          local.Sign2.Text2 = " +";
        }

        if (export.TotArRefNbrCfFees.TotalCurrency < 0)
        {
          local.Sign3.Text2 = " -";
        }
        else
        {
          local.Sign3.Text2 = " +";
        }

        local.EabReportSend.RptDetail = "  Dup Pmt CR Fee:  Disb Coll ID " + NumberToString
          (entities.DisbCollection.SystemGeneratedIdentifier, 7, 9) + " Coll Amt" +
          local.Sign1.Text2 + NumberToString
          ((long)(entities.DisbCollection.Amount * 100), 8, 8) + " CR Fee" + local
          .Sign2.Text2 + NumberToString
          ((long)(local.WorkCrFee.Amount * 100), 12, 4) + " Tot CR Fees" + local
          .Sign3.Text2 + NumberToString
          ((long)(export.TotArRefNbrCfFees.TotalCurrency * 100), 11, 5) + "."
          + " ";
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (local.TribunalFeeInformation.Cap.GetValueOrDefault() > 0)
      {
        if (entities.MonthlyCourtOrderFee.Amount + export
          .TotArRefNbrCfFees.TotalCurrency >= local
          .TribunalFeeInformation.Cap.GetValueOrDefault())
        {
          // The maximum monthly fee has already been taken so escape.
          export.TotArRefNbrCfFees.TotalCurrency = 0;

          if (AsChar(import.TestDisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  Maximum mo fee was reached so set to max & exit.";
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }
      }
    }

    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      if (export.TotArRefNbrCfFees.TotalCurrency < 0)
      {
        local.Sign1.Text2 = " -";
      }
      else
      {
        local.Sign1.Text2 = " +";
      }

      local.EabReportSend.RptDetail = "    CR Fee for all disb coll is" + local
        .Sign1.Text2 + NumberToString
        ((long)(export.TotArRefNbrCfFees.TotalCurrency * 100), 8, 8);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

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

  private void UseFnB651DetIfCrFeeNeeded()
  {
    var useImport = new FnB651DetIfCrFeeNeeded.Import();
    var useExport = new FnB651DetIfCrFeeNeeded.Export();

    useImport.Collection.CollectionDt = entities.Collection.CollectionDt;
    useImport.Ar.Number = import.PerAr.Number;
    useImport.TestDisplay.Flag = import.TestDisplayInd.Flag;

    Call(FnB651DetIfCrFeeNeeded.Execute, useImport, useExport);

    local.CrFeeNeededInd.Flag = useExport.CrFeeNeededInd.Flag;
  }

  private IEnumerable<bool> ReadDisbCollectionCollection()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);
    entities.DisbCollection.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadDisbCollectionCollection",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PerObligee.Type1);
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetNullableDate(
          command, "processDate", local.Initialized.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "referenceNumber",
          import.DisbursementTransaction.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        entities.DisbCollection.CpaType = db.GetString(reader, 0);
        entities.DisbCollection.CspNumber = db.GetString(reader, 1);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbCollection.Amount = db.GetDecimal(reader, 3);
        entities.DisbCollection.ProcessDate = db.GetNullableDate(reader, 4);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 5);
        entities.Collection.OtyId = db.GetInt32(reader, 5);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 6);
        entities.Collection.OtrType = db.GetString(reader, 6);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 7);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 11);
        entities.Collection.CrdId = db.GetInt32(reader, 11);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 12);
        entities.Collection.CrvId = db.GetInt32(reader, 12);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 13);
        entities.Collection.CstId = db.GetInt32(reader, 13);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 14);
        entities.Collection.CrtType = db.GetInt32(reader, 14);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 15);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 15);
        entities.DisbCollection.ReferenceNumber =
          db.GetNullableString(reader, 16);
        entities.Collection.CollectionDt = db.GetDate(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.DisbCollection.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbCollection.CpaType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbCollection.OtrTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbCollection.CpaTypeDisb);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.PreAdjustmentCredit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", entities.Collection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", entities.Collection.OtyId);
        db.SetNullableInt32(command, "obgId", entities.Collection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", entities.Collection.CspNumber);
        db.
          SetNullableString(command, "cpaTypeDisb", entities.Collection.CpaType);
          
        db.SetNullableInt32(command, "otrId", entities.Collection.OtrId);
        db.
          SetNullableString(command, "otrTypeDisb", entities.Collection.OtrType);
          
        db.SetNullableInt32(command, "crtId", entities.Collection.CrtType);
        db.SetNullableInt32(command, "cstId", entities.Collection.CstId);
        db.SetNullableInt32(command, "crvId", entities.Collection.CrvId);
        db.SetNullableInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbCollection.SystemGeneratedIdentifier);
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
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.PreAdjustmentCredit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.PreAdjustmentCredit.CpaType);
        db.SetString(
          command, "cspPNumber", entities.PreAdjustmentCredit.CspNumber);
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
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Collection.OtyId);
        db.SetInt32(command, "obId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
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
          entities.Collection.CollectionDt.GetValueOrDefault());
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
    /// A value of PerAr.
    /// </summary>
    [JsonPropertyName("perAr")]
    public CsePerson PerAr
    {
      get => perAr ??= new();
      set => perAr = value;
    }

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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private CsePerson perAr;
    private CsePersonAccount perObligee;
    private DisbursementTransaction disbursementTransaction;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotArRefNbrCfFees.
    /// </summary>
    [JsonPropertyName("totArRefNbrCfFees")]
    public Common TotArRefNbrCfFees
    {
      get => totArRefNbrCfFees ??= new();
      set => totArRefNbrCfFees = value;
    }

    private Common totArRefNbrCfFees;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
    }

    /// <summary>
    /// A value of Sign1.
    /// </summary>
    [JsonPropertyName("sign1")]
    public TextWorkArea Sign1
    {
      get => sign1 ??= new();
      set => sign1 = value;
    }

    /// <summary>
    /// A value of Sign2.
    /// </summary>
    [JsonPropertyName("sign2")]
    public TextWorkArea Sign2
    {
      get => sign2 ??= new();
      set => sign2 = value;
    }

    /// <summary>
    /// A value of Sign3.
    /// </summary>
    [JsonPropertyName("sign3")]
    public TextWorkArea Sign3
    {
      get => sign3 ??= new();
      set => sign3 = value;
    }

    /// <summary>
    /// A value of CrFeeNeededInd.
    /// </summary>
    [JsonPropertyName("crFeeNeededInd")]
    public Common CrFeeNeededInd
    {
      get => crFeeNeededInd ??= new();
      set => crFeeNeededInd = value;
    }

    /// <summary>
    /// A value of FirstTimeThruInd.
    /// </summary>
    [JsonPropertyName("firstTimeThruInd")]
    public Common FirstTimeThruInd
    {
      get => firstTimeThruInd ??= new();
      set => firstTimeThruInd = value;
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
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of WorkCrFee.
    /// </summary>
    [JsonPropertyName("workCrFee")]
    public DisbursementTransaction WorkCrFee
    {
      get => workCrFee ??= new();
      set => workCrFee = value;
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

    private MonthlyCourtOrderFee monthlyCourtOrderFee;
    private TextWorkArea sign1;
    private TextWorkArea sign2;
    private TextWorkArea sign3;
    private Common crFeeNeededInd;
    private Common firstTimeThruInd;
    private DateWorkArea initialized;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TribunalFeeInformation tribunalFeeInformation;
    private DisbursementTransaction workCrFee;
    private DateWorkArea collectionDate;
    private DateWorkArea collectionDateYyMm;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of PreAdjustmentCredit.
    /// </summary>
    [JsonPropertyName("preAdjustmentCredit")]
    public DisbursementTransaction PreAdjustmentCredit
    {
      get => preAdjustmentCredit ??= new();
      set => preAdjustmentCredit = value;
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
    /// A value of PreAdjustmentCrFee.
    /// </summary>
    [JsonPropertyName("preAdjustmentCrFee")]
    public DisbursementType PreAdjustmentCrFee
    {
      get => preAdjustmentCrFee ??= new();
      set => preAdjustmentCrFee = value;
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
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
    }

    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private DisbursementTransaction disbCollection;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
    private TribunalFeeInformation tribunalFeeInformation;
    private DisbursementTransaction preAdjustmentCredit;
    private DisbursementTransaction preAdjustmentCrFeeDisbDebit;
    private DisbursementType preAdjustmentCrFee;
    private DisbursementTransactionRln disbursementTransactionRln;
    private MonthlyCourtOrderFee monthlyCourtOrderFee;
  }
#endregion
}
