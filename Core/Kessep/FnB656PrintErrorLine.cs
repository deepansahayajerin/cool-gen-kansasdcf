// Program: FN_B656_PRINT_ERROR_LINE, ID: 372720553, model: 746.
// Short name: SWE02400
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B656_PRINT_ERROR_LINE.
/// </summary>
[Serializable]
public partial class FnB656PrintErrorLine: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B656_PRINT_ERROR_LINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB656PrintErrorLine(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB656PrintErrorLine.
  /// </summary>
  public FnB656PrintErrorLine(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // ??/??/??  ???????			Initial Development
    // 10/18/01  SWSRGAV	WR# 20183	Create an additional error report to document
    // 'new'
    // 					errors only.
    // 03/08/19  GVandy 	CQ65422		Adding additional data elements reported on an
    // error.
    // --------------------------------------------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      ExitState = "ACO_NN0000_ALL_OK";
      local.CommasRequired.Flag = "Y";
      local.EabFileHandling.Action = "WRITE";

      foreach(var item in ReadCaseCaseRole())
      {
        if (!Lt(entities.CaseRole.EndDate, Now().Date))
        {
          local.ActiveRole.Flag = "Y";
        }

        if (AsChar(local.ActiveRole.Flag) == 'Y' && Lt
          (entities.CaseRole.EndDate, Now().Date))
        {
          break;
        }

        local.Worker.Text30 = "";
        local.Contractor.Name = "";

        if (ReadCaseAssignment())
        {
          if (ReadServiceProviderOffice())
          {
            local.Worker.Text30 = TrimEnd(entities.ServiceProvider.LastName) + ", " +
              entities.ServiceProvider.FirstName;
            local.Office.Text4 =
              NumberToString(entities.Office.SystemGeneratedId, 12, 4);

            if (!ReadCseOrganization2())
            {
              local.Contractor.Name = "County Not Found";

              goto Read;
            }

            if (!ReadCseOrganization3())
            {
              local.Contractor.Name = "JD Not Found";

              goto Read;
            }

            if (ReadCseOrganization1())
            {
              local.Contractor.Name = entities.Contractor.Name;
            }
            else
            {
              local.Contractor.Name = "Contractor Not Found";
            }
          }
          else
          {
            // --Continue
          }

Read:
          ;
        }

        for(local.Common.Count = 1; local.Common.Count <= 6; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              if (IsEmpty(local.ExitStateWorkArea.Message))
              {
                local.EabReportSend.RptDetail =
                  "Exit State: Unknown Error - Exit State Message is Blank.";
                local.Reason.NarrativeText =
                  "REASON: Unknown Error - Exit State Message is Blank.";
              }
              else
              {
                local.EabReportSend.RptDetail = "Exit State: " + local
                  .ExitStateWorkArea.Message + import.TextWorkArea.Text30;
                local.Reason.NarrativeText = "REASON: " + local
                  .ExitStateWorkArea.Message + import.TextWorkArea.Text30;
              }

              break;
            case 2:
              if (import.PaymentRequest.SystemGeneratedIdentifier != 0)
              {
                if (IsEmpty(import.PaymentRequest.Classification))
                {
                  local.EabReportSend.RptDetail = "Payment Request ID : " + NumberToString
                    (import.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Classification : " +
                    "** INVALID VALUE **";
                }
                else
                {
                  local.EabReportSend.RptDetail = "Payment Request ID : " + NumberToString
                    (import.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Classification : " +
                    import.PaymentRequest.Classification;
                }

                local.CurrencyValueCommon.TotalCurrency =
                  import.PaymentRequest.Amount;
                UseFnCabCurrencyToText();
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "  Amount: " + local
                  .CurrencyValueTextWorkArea.Text10;
                local.Amount.NarrativeText = "Amount: " + local
                  .CurrencyValueTextWorkArea.Text10;

                if (ReadDisbursementTransaction())
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "  Disb Date: " + NumberToString
                    (Month(entities.DisbursementTransaction.DisbursementDate),
                    14, 2) + "-" + NumberToString
                    (Day(entities.DisbursementTransaction.DisbursementDate), 14,
                    2) + "-" + NumberToString
                    (Year(entities.DisbursementTransaction.DisbursementDate),
                    12, 4);
                  local.Amount.NarrativeText =
                    TrimEnd(local.Amount.NarrativeText) + "  Disb Date: " + NumberToString
                    (Month(entities.DisbursementTransaction.DisbursementDate),
                    14, 2) + "-" + NumberToString
                    (Day(entities.DisbursementTransaction.DisbursementDate), 14,
                    2) + "-" + NumberToString
                    (Year(entities.DisbursementTransaction.DisbursementDate),
                    12, 4);
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "  Disb Date: Not Found";
                    
                  local.Amount.NarrativeText =
                    TrimEnd(local.Amount.NarrativeText) + "  Disb Date: Not Found";
                    
                }
              }
              else
              {
                continue;
              }

              break;
            case 3:
              if (import.PaymentRequest.SystemGeneratedIdentifier != 0)
              {
                if (IsEmpty(import.PaymentRequest.CsePersonNumber))
                {
                  local.EabReportSend.RptDetail =
                    "Payee Number : **** PAYEE NUMBER IS BLANK ****";
                }
                else
                {
                  local.EabReportSend.RptDetail = "Payee Number : " + (
                    import.PaymentRequest.CsePersonNumber ?? "");
                  local.CsePersonsWorkSet.FormattedName = "";
                  local.CsePersonsWorkSet.Number =
                    import.PaymentRequest.CsePersonNumber ?? Spaces(10);
                  UseSiReadCsePersonBatch();
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "  Name: " + local
                    .CsePersonsWorkSet.FormattedName;
                  local.Payee.NarrativeText = local.EabReportSend.RptDetail;
                }
              }
              else
              {
                continue;
              }

              break;
            case 4:
              if (import.PaymentRequest.SystemGeneratedIdentifier != 0)
              {
                if (!Equal(import.PaymentRequest.CsePersonNumber,
                  import.PaymentRequest.DesignatedPayeeCsePersonNo) && !
                  IsEmpty(import.PaymentRequest.DesignatedPayeeCsePersonNo))
                {
                  local.EabReportSend.RptDetail =
                    "Designated Payee Number : " + (
                      import.PaymentRequest.DesignatedPayeeCsePersonNo ?? "");
                }
                else
                {
                  continue;
                }
              }
              else
              {
                continue;
              }

              break;
            case 5:
              local.EabReportSend.RptDetail = "Case: " + entities
                .Case1.Number + " Worker: " + local.Worker.Text30 + " Office: " +
                local.Office.Text4 + " Contractor: " + local.Contractor.Name;

              break;
            case 6:
              local.EabReportSend.RptDetail =
                "------------------------------------------------------------------------------------------------------------------------------------";
                

              break;
            default:
              break;
          }

          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (AsChar(import.NewWarrant.Flag) == 'Y')
          {
            // -- Write to the daily error report.
            UseCabBusinessReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            if (local.Common.Count == 6)
            {
              // --Raise Event to create HIST record and alert.
              local.Infrastructure.EventId = 9;
              local.Infrastructure.ReasonCode = "WARRANTCANTPROC";
              local.Infrastructure.Detail = local.ExitStateWorkArea.Message;
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.BusinessObjectCd = "CAS";
              local.Infrastructure.CsePersonNumber =
                import.PaymentRequest.CsePersonNumber ?? "";
              local.Infrastructure.CaseNumber = entities.Case1.Number;

              if (!ReadCaseUnit())
              {
                continue;
              }

              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.CsenetInOutCode = "";
              local.Infrastructure.InitiatingStateCode = "KS";
              local.Infrastructure.ReferenceDate = Now().Date;
              local.Infrastructure.UserId = global.UserId;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error Creating Infrastructure... " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport1();

                // --Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              // --Create CSLN entry.
              local.NarrativeDetail.InfrastructureId =
                local.Infrastructure.SystemGeneratedIdentifier;
              local.NarrativeDetail.CaseNumber = entities.Case1.Number;
              local.NarrativeDetail.CreatedBy = global.UserId;
              local.NarrativeDetail.CreatedTimestamp =
                local.Infrastructure.CreatedTimestamp;

              for(local.NarrativeDetail.LineNumber = 1; local
                .NarrativeDetail.LineNumber <= 4; ++
                local.NarrativeDetail.LineNumber)
              {
                // --Set the narrative detail values
                switch(local.NarrativeDetail.LineNumber)
                {
                  case 1:
                    local.NarrativeDetail.NarrativeText =
                      "WARRANT CANNOT BE PROCESSED TO KPC PAY TAPE";

                    break;
                  case 2:
                    local.NarrativeDetail.NarrativeText =
                      local.Reason.NarrativeText ?? "";

                    break;
                  case 3:
                    local.NarrativeDetail.NarrativeText =
                      local.Amount.NarrativeText ?? "";

                    break;
                  case 4:
                    local.NarrativeDetail.NarrativeText =
                      local.Payee.NarrativeText ?? "";

                    break;
                  default:
                    break;
                }

                UseSpCabCreateNarrativeDetail();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabExtractExitStateMessage();
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error Creating Narrative... " + local
                    .ExitStateWorkArea.Message;
                  UseCabErrorReport1();

                  // --Set Abort exit state and escape...
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
            }
          }
        }

        if (Lt(entities.CaseRole.EndDate, Now().Date))
        {
          break;
        }
      }
    }

    if (AsChar(import.CloseInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

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

  private void UseFnCabCurrencyToText()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    MoveCommon(local.CurrencyValueCommon, useImport.Common);
    useImport.CommasRequired.Flag = local.CommasRequired.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.CurrencyValueTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetString(
          command, "cspNumber", import.PaymentRequest.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCseOrganization1()
  {
    entities.Contractor.Populated = false;

    return Read("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "cogParentType", entities.Jd.Type1);
        db.SetString(command, "cogParentCode", entities.Jd.Code);
      },
      (db, reader) =>
      {
        entities.Contractor.Code = db.GetString(reader, 0);
        entities.Contractor.Type1 = db.GetString(reader, 1);
        entities.Contractor.Name = db.GetString(reader, 2);
        entities.Contractor.Populated = true;
      });
  }

  private bool ReadCseOrganization2()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.County.Populated = false;

    return Read("ReadCseOrganization2",
      (db, command) =>
      {
        db.SetString(command, "typeCode", entities.Office.CogTypeCode ?? "");
        db.SetString(command, "organztnId", entities.Office.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.County.Code = db.GetString(reader, 0);
        entities.County.Type1 = db.GetString(reader, 1);
        entities.County.Populated = true;
      });
  }

  private bool ReadCseOrganization3()
  {
    entities.Jd.Populated = false;

    return Read("ReadCseOrganization3",
      (db, command) =>
      {
        db.SetString(command, "cogParentType", entities.County.Type1);
        db.SetString(command, "cogParentCode", entities.County.Code);
      },
      (db, reader) =>
      {
        entities.Jd.Code = db.GetString(reader, 0);
        entities.Jd.Type1 = db.GetString(reader, 1);
        entities.Jd.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
      });
  }

  private bool ReadServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(command, "servicePrvderId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 4);
        entities.Office.CogCode = db.GetNullableString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
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
    /// A value of NewWarrant.
    /// </summary>
    [JsonPropertyName("newWarrant")]
    public Common NewWarrant
    {
      get => newWarrant ??= new();
      set => newWarrant = value;
    }

    private PaymentRequest paymentRequest;
    private Common closeInd;
    private TextWorkArea textWorkArea;
    private Common newWarrant;
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
    /// A value of CurrencyValueTextWorkArea.
    /// </summary>
    [JsonPropertyName("currencyValueTextWorkArea")]
    public TextWorkArea CurrencyValueTextWorkArea
    {
      get => currencyValueTextWorkArea ??= new();
      set => currencyValueTextWorkArea = value;
    }

    /// <summary>
    /// A value of CurrencyValueCommon.
    /// </summary>
    [JsonPropertyName("currencyValueCommon")]
    public Common CurrencyValueCommon
    {
      get => currencyValueCommon ??= new();
      set => currencyValueCommon = value;
    }

    /// <summary>
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
    }

    /// <summary>
    /// A value of ActiveRole.
    /// </summary>
    [JsonPropertyName("activeRole")]
    public Common ActiveRole
    {
      get => activeRole ??= new();
      set => activeRole = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public NarrativeDetail Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public NarrativeDetail Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of Reason.
    /// </summary>
    [JsonPropertyName("reason")]
    public NarrativeDetail Reason
    {
      get => reason ??= new();
      set => reason = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public TextWorkArea Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public TextWorkArea Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea currencyValueTextWorkArea;
    private Common currencyValueCommon;
    private Common commasRequired;
    private Common activeRole;
    private NarrativeDetail payee;
    private NarrativeDetail amount;
    private NarrativeDetail reason;
    private TextWorkArea office;
    private Infrastructure infrastructure;
    private NarrativeDetail narrativeDetail;
    private TextWorkArea worker;
    private CseOrganization contractor;
    private Common common;
    private ExitStateWorkArea exitStateWorkArea;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of Jc.
    /// </summary>
    [JsonPropertyName("jc")]
    public CseOrganizationRelationship Jc
    {
      get => jc ??= new();
      set => jc = value;
    }

    /// <summary>
    /// A value of Xj.
    /// </summary>
    [JsonPropertyName("xj")]
    public CseOrganizationRelationship Xj
    {
      get => xj ??= new();
      set => xj = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of Jd.
    /// </summary>
    [JsonPropertyName("jd")]
    public CseOrganization Jd
    {
      get => jd ??= new();
      set => jd = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    private DisbursementTransaction disbursementTransaction;
    private PaymentRequest paymentRequest;
    private CsePerson csePerson;
    private CseOrganizationRelationship jc;
    private CseOrganizationRelationship xj;
    private CseOrganization county;
    private CseOrganization jd;
    private CseOrganization contractor;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
