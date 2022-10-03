// Program: FN_B651_PRINT_ERROR_LINE, ID: 1625344885, model: 746.
// Short name: SWE02652
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_PRINT_ERROR_LINE.
/// </summary>
[Serializable]
public partial class FnB651PrintErrorLine: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_PRINT_ERROR_LINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651PrintErrorLine(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651PrintErrorLine.
  /// </summary>
  public FnB651PrintErrorLine(IContext context, Import import, Export export):
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
    // 07/02/19  GVandy	CQ65423		Print information about system suppressions and
    // 					create a <tab> delimited file containing the suppressions
    // 					which will be emailed to business staff.
    // 					This is fashioned after the logic in SWEFB656 (Warrants)
    // 					which logs info when warrants in REQ status are skipped.
    // 08/20/20  GVandy	CQ66660		Modifying changes added for CQ65423 to now 
    // apply only
    // 					to cases where an AP on the case made the payment
    // 					that resulted in the suppressed disbursement.
    // --------------------------------------------------------------------------------------------------
    // --The following delimiter is set to the <tab> character
    //   (entered by pressing <ctrl> and <tab> at the same time).
    local.TabChar.Text1 = "\t";

    switch(AsChar(import.DisbursementStatusHistory.SuppressionReason))
    {
      case 'Y':
        local.SuppressionReason.Text20 = "DECEASED";

        break;
      case 'Z':
        local.SuppressionReason.Text20 = "NO ACTIVE ADDRESS";

        break;
      default:
        local.SuppressionReason.Text20 = "UNKNOWN VALUE - " + (
          import.DisbursementStatusHistory.SuppressionReason ?? "");

        break;
    }

    if (!IsEmpty(import.DisbursementStatusHistory.SuppressionReason))
    {
      local.CommasRequired.Flag = "Y";
      local.EabFileHandling.Action = "WRITE";

      foreach(var item in ReadCaseCaseRole())
      {
        if (!Lt(entities.ArCaseRole.EndDate, Now().Date))
        {
          local.ActiveRole.Flag = "Y";
        }

        if (AsChar(local.ActiveRole.Flag) == 'Y' && Lt
          (entities.ArCaseRole.EndDate, Now().Date))
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
              local.EabReportSend.RptDetail = "Suppression Type: " + (
                import.DisbursementStatusHistory.SuppressionReason ?? "") + " " +
                local.SuppressionReason.Text20;
              local.Reason.NarrativeText = "REASON: " + (
                import.DisbursementStatusHistory.SuppressionReason ?? "") + " " +
                local.SuppressionReason.Text20;
              local.Data255CharacterTextRecord.Data =
                import.NewSystemSuppression.Flag + local.TabChar.Text1 + TrimEnd
                (local.SuppressionReason.Text20) + local.TabChar.Text1;

              break;
            case 2:
              local.EabReportSend.RptDetail = "Disbursement Tran ID: " + NumberToString
                (import.DisbursementTransaction.SystemGeneratedIdentifier, 7, 9) +
                " Reference Number: " + Substring
                (import.DisbursementTransaction.ReferenceNumber, 1, 12);
              local.CurrencyValueCommon.TotalCurrency =
                import.DisbursementTransaction.Amount;
              UseFnCabCurrencyToText();
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "  Amount: " + local
                .CurrencyValueTextWorkArea.Text10;
              local.Amount.NarrativeText = "Amount: " + local
                .CurrencyValueTextWorkArea.Text10;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "  Disb Date: " + NumberToString
                (Month(import.DisbursementTransaction.DisbursementDate), 14, 2) +
                "-" + NumberToString
                (Day(import.DisbursementTransaction.DisbursementDate), 14, 2) +
                "-" + NumberToString
                (Year(import.DisbursementTransaction.DisbursementDate), 12, 4);
              local.Amount.NarrativeText =
                TrimEnd(local.Amount.NarrativeText) + "  Disb Date: " + NumberToString
                (Month(import.DisbursementTransaction.DisbursementDate), 14, 2) +
                "-" + NumberToString
                (Day(import.DisbursementTransaction.DisbursementDate), 14, 2) +
                "-" + NumberToString
                (Year(import.DisbursementTransaction.DisbursementDate), 12, 4);
              local.Data255CharacterTextRecord.Data =
                TrimEnd(local.Data255CharacterTextRecord.Data) + NumberToString
                (import.DisbursementTransaction.SystemGeneratedIdentifier, 7, 9) +
                local.TabChar.Text1 + Substring
                (import.DisbursementTransaction.ReferenceNumber, 1, 12) + local
                .TabChar.Text1 + local.CurrencyValueTextWorkArea.Text10 + local
                .TabChar.Text1 + NumberToString
                (Month(import.DisbursementTransaction.DisbursementDate), 14, 2) +
                "-" + NumberToString
                (Day(import.DisbursementTransaction.DisbursementDate), 14, 2) +
                "-" + NumberToString
                (Year(import.DisbursementTransaction.DisbursementDate), 12, 4) +
                local.TabChar.Text1;

              break;
            case 3:
              local.CsePersonsWorkSet.Number = import.Ar.Number;
              local.EabReportSend.RptDetail = "Payee Number: " + local
                .CsePersonsWorkSet.Number;
              local.CsePersonsWorkSet.FormattedName = "";
              UseSiReadCsePersonBatch();
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "  Name: " + local
                .CsePersonsWorkSet.FormattedName;
              local.Payee.NarrativeText = local.EabReportSend.RptDetail;
              local.Data255CharacterTextRecord.Data =
                TrimEnd(local.Data255CharacterTextRecord.Data) + local
                .CsePersonsWorkSet.Number + local.TabChar.Text1 + TrimEnd
                (local.CsePersonsWorkSet.FormattedName) + local.TabChar.Text1;

              break;
            case 4:
              if (ReadCsePerson())
              {
                local.EabReportSend.RptDetail = "Designated Payee Number : " + entities
                  .DesignatedPayee.Number;
                local.Data255CharacterTextRecord.Data =
                  TrimEnd(local.Data255CharacterTextRecord.Data) + entities
                  .DesignatedPayee.Number + local.TabChar.Text1;
              }
              else
              {
                local.Data255CharacterTextRecord.Data =
                  TrimEnd(local.Data255CharacterTextRecord.Data) + local
                  .TabChar.Text1;

                continue;
              }

              break;
            case 5:
              local.EabReportSend.RptDetail = "Case: " + entities
                .Case1.Number + " Worker: " + local.Worker.Text30 + " Office: " +
                local.Office.Text4 + " Contractor: " + local.Contractor.Name;
              local.Data255CharacterTextRecord.Data =
                TrimEnd(local.Data255CharacterTextRecord.Data) + entities
                .Case1.Number + local.TabChar.Text1 + TrimEnd
                (local.Worker.Text30) + local.TabChar.Text1 + local
                .Office.Text4 + local.TabChar.Text1 + TrimEnd
                (local.Contractor.Name) + local.TabChar.Text1;

              break;
            case 6:
              local.EabReportSend.RptDetail =
                "------------------------------------------------------------------------------------------------------------------------------------";
                

              break;
            default:
              break;
          }

          UseCabBusinessReport4();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (local.Common.Count == 6)
          {
            // -- Write to the <tab> delimited file.
            UseEabWrite255CharFile1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          if (AsChar(import.NewSystemSuppression.Flag) == 'Y')
          {
            // -- Write to the daily error report.
            UseCabBusinessReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }

        if (Lt(entities.ArCaseRole.EndDate, Now().Date))
        {
          break;
        }
      }
    }

    if (AsChar(import.CloseInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "CLOSE";
      UseCabBusinessReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabBusinessReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseEabWrite255CharFile2();

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

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport4()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWrite255CharFile1()
  {
    var useImport = new EabWrite255CharFile.Import();
    var useExport = new EabWrite255CharFile.Export();

    useImport.Data255CharacterTextRecord.Data =
      local.Data255CharacterTextRecord.Data;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWrite255CharFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWrite255CharFile2()
  {
    var useImport = new EabWrite255CharFile.Import();
    var useExport = new EabWrite255CharFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWrite255CharFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnCabCurrencyToText()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    MoveCommon(local.CurrencyValueCommon, useImport.Common);

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.CurrencyValueTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
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
    entities.ArCaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);

        return true;
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

  private bool ReadCsePerson()
  {
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "csePersNum", import.Ar.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DesignatedPayee.Number = db.GetString(reader, 0);
        entities.DesignatedPayee.Populated = true;
      });
  }

  private bool ReadServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

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
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of NewSystemSuppression.
    /// </summary>
    [JsonPropertyName("newSystemSuppression")]
    public Common NewSystemSuppression
    {
      get => newSystemSuppression ??= new();
      set => newSystemSuppression = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson ar;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementStatusHistory disbursementStatusHistory;
    private Common newSystemSuppression;
    private Common closeInd;
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
    /// A value of SuppressionReason.
    /// </summary>
    [JsonPropertyName("suppressionReason")]
    public WorkArea SuppressionReason
    {
      get => suppressionReason ??= new();
      set => suppressionReason = value;
    }

    /// <summary>
    /// A value of Data255CharacterTextRecord.
    /// </summary>
    [JsonPropertyName("data255CharacterTextRecord")]
    public Data255CharacterTextRecord Data255CharacterTextRecord
    {
      get => data255CharacterTextRecord ??= new();
      set => data255CharacterTextRecord = value;
    }

    /// <summary>
    /// A value of TabChar.
    /// </summary>
    [JsonPropertyName("tabChar")]
    public TextWorkArea TabChar
    {
      get => tabChar ??= new();
      set => tabChar = value;
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
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
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
    /// A value of ActiveRole.
    /// </summary>
    [JsonPropertyName("activeRole")]
    public Common ActiveRole
    {
      get => activeRole ??= new();
      set => activeRole = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public TextWorkArea Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Reason.
    /// </summary>
    [JsonPropertyName("reason")]
    public NarrativeDetail Reason
    {
      get => reason ??= new();
      set => reason = value;
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
    /// A value of CurrencyValueTextWorkArea.
    /// </summary>
    [JsonPropertyName("currencyValueTextWorkArea")]
    public TextWorkArea CurrencyValueTextWorkArea
    {
      get => currencyValueTextWorkArea ??= new();
      set => currencyValueTextWorkArea = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public NarrativeDetail Payee
    {
      get => payee ??= new();
      set => payee = value;
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

    private WorkArea suppressionReason;
    private Data255CharacterTextRecord data255CharacterTextRecord;
    private TextWorkArea tabChar;
    private ExitStateWorkArea exitStateWorkArea;
    private Common commasRequired;
    private EabFileHandling eabFileHandling;
    private Common activeRole;
    private TextWorkArea worker;
    private CseOrganization contractor;
    private TextWorkArea office;
    private EabReportSend eabReportSend;
    private NarrativeDetail reason;
    private Common currencyValueCommon;
    private TextWorkArea currencyValueTextWorkArea;
    private NarrativeDetail amount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private NarrativeDetail payee;
    private Common common;
    private Infrastructure infrastructure;
    private NarrativeDetail narrativeDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

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
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Jc.
    /// </summary>
    [JsonPropertyName("jc")]
    public CseOrganizationRelationship Jc
    {
      get => jc ??= new();
      set => jc = value;
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
    /// A value of Xj.
    /// </summary>
    [JsonPropertyName("xj")]
    public CseOrganizationRelationship Xj
    {
      get => xj ??= new();
      set => xj = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction debit;
    private DisbursementTransaction credit;
    private CsePersonAccount obligee;
    private CsePersonDesigPayee csePersonDesigPayee;
    private CsePerson arCsePerson;
    private CsePerson designatedPayee;
    private CaseRole arCaseRole;
    private ServiceProvider serviceProvider;
    private Office office;
    private CseOrganization county;
    private CseOrganization jd;
    private CseOrganizationRelationship jc;
    private CseOrganization contractor;
    private CseOrganizationRelationship xj;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
  }
#endregion
}
