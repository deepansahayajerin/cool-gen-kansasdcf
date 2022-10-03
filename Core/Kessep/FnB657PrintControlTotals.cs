// Program: FN_B657_PRINT_CONTROL_TOTALS, ID: 372723857, model: 746.
// Short name: SWE02334
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B657_PRINT_CONTROL_TOTALS.
/// </summary>
[Serializable]
public partial class FnB657PrintControlTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B657_PRINT_CONTROL_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB657PrintControlTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB657PrintControlTotals.
  /// </summary>
  public FnB657PrintControlTotals(IContext context, Import import, Export export)
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
    // **************************************************************************************************************
    // * 01/07/03  PR # 162884  M Fangman   Removed code to clode the report 
    // files because more detail lines can be *
    // *
    // 
    // produced in the pstep after this AB
    // has been executed.
    // *
    // * 09/24/07  WR # 280422  GVandy      Support disbursements via Debit 
    // Cards. 				*
    // **************************************************************************************************************
    for(local.Common.Count = 1; local.Common.Count <= 19; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Total number of checks PAID.....................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfChecksPaid.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total number of EFTs PAID.......................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfEftsPaid.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Total number of Debit Cards PAID................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfDcsPaid.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Total number of checks REISSUED.................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfChecksReissued.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Total number of EFTs REISSUED...................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfEftsReissued.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Total number of Debit Cards REISSUED............................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfDcsReissued.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "Total number of checks Bypassed.................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfChecksBypassed.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "Total number of EFTs Bypassed...................................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfEftsBypassed.Count, 15);

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "Total number of Debit Cards Bypassed............................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NumberOfDcsBypassed.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "Total number of checks reissued more than once..................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.ChecksReisMoreThan1.Count, 15);

          break;
        case 12:
          local.EabReportSend.RptDetail =
            "Total number of EFTs reissued more than once....................";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.EftsReisMoreThan1.Count, 15);

          break;
        case 13:
          local.EabReportSend.RptDetail =
            "Total number of Debit Cards reissued more than once.............";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.DcsReisMoreThan1.Count, 15);

          break;
        case 14:
          local.EabReportSend.RptDetail =
            "Total number of reissued more than once checks bypassed.........";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NoOfChksBypassedMore1.Count, 15);

          break;
        case 15:
          local.EabReportSend.RptDetail =
            "Total number of reissued more than once EFTs  bypassed..........";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NoEftsBypassedMore1.Count, 15);

          break;
        case 16:
          local.EabReportSend.RptDetail =
            "Total number of reissued more than once Debit Cards bypassed....";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (import.NoDcsBypassedMore1.Count, 15);

          break;
        case 17:
          if (AsChar(import.CheckBelowLastNo.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "The check number has fallen below the last used number.";
          }
          else
          {
            continue;
          }

          break;
        case 18:
          if (AsChar(import.EftBelowLastNo.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "The EFT number has fallen below the last used number.";
          }
          else
          {
            continue;
          }

          break;
        case 19:
          if (AsChar(import.DcBelowLastNo.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "The Debit Card number has fallen below the last used number.";
          }
          else
          {
            continue;
          }

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of EftBelowLastNo.
    /// </summary>
    [JsonPropertyName("eftBelowLastNo")]
    public Common EftBelowLastNo
    {
      get => eftBelowLastNo ??= new();
      set => eftBelowLastNo = value;
    }

    /// <summary>
    /// A value of DcBelowLastNo.
    /// </summary>
    [JsonPropertyName("dcBelowLastNo")]
    public Common DcBelowLastNo
    {
      get => dcBelowLastNo ??= new();
      set => dcBelowLastNo = value;
    }

    /// <summary>
    /// A value of CheckBelowLastNo.
    /// </summary>
    [JsonPropertyName("checkBelowLastNo")]
    public Common CheckBelowLastNo
    {
      get => checkBelowLastNo ??= new();
      set => checkBelowLastNo = value;
    }

    /// <summary>
    /// A value of NoOfChksBypassedMore1.
    /// </summary>
    [JsonPropertyName("noOfChksBypassedMore1")]
    public Common NoOfChksBypassedMore1
    {
      get => noOfChksBypassedMore1 ??= new();
      set => noOfChksBypassedMore1 = value;
    }

    /// <summary>
    /// A value of NoEftsBypassedMore1.
    /// </summary>
    [JsonPropertyName("noEftsBypassedMore1")]
    public Common NoEftsBypassedMore1
    {
      get => noEftsBypassedMore1 ??= new();
      set => noEftsBypassedMore1 = value;
    }

    /// <summary>
    /// A value of NoDcsBypassedMore1.
    /// </summary>
    [JsonPropertyName("noDcsBypassedMore1")]
    public Common NoDcsBypassedMore1
    {
      get => noDcsBypassedMore1 ??= new();
      set => noDcsBypassedMore1 = value;
    }

    /// <summary>
    /// A value of NumberOfChecksReissued.
    /// </summary>
    [JsonPropertyName("numberOfChecksReissued")]
    public Common NumberOfChecksReissued
    {
      get => numberOfChecksReissued ??= new();
      set => numberOfChecksReissued = value;
    }

    /// <summary>
    /// A value of NumberOfEftsReissued.
    /// </summary>
    [JsonPropertyName("numberOfEftsReissued")]
    public Common NumberOfEftsReissued
    {
      get => numberOfEftsReissued ??= new();
      set => numberOfEftsReissued = value;
    }

    /// <summary>
    /// A value of NumberOfDcsReissued.
    /// </summary>
    [JsonPropertyName("numberOfDcsReissued")]
    public Common NumberOfDcsReissued
    {
      get => numberOfDcsReissued ??= new();
      set => numberOfDcsReissued = value;
    }

    /// <summary>
    /// A value of NumberOfChecksPaid.
    /// </summary>
    [JsonPropertyName("numberOfChecksPaid")]
    public Common NumberOfChecksPaid
    {
      get => numberOfChecksPaid ??= new();
      set => numberOfChecksPaid = value;
    }

    /// <summary>
    /// A value of NumberOfEftsPaid.
    /// </summary>
    [JsonPropertyName("numberOfEftsPaid")]
    public Common NumberOfEftsPaid
    {
      get => numberOfEftsPaid ??= new();
      set => numberOfEftsPaid = value;
    }

    /// <summary>
    /// A value of NumberOfDcsPaid.
    /// </summary>
    [JsonPropertyName("numberOfDcsPaid")]
    public Common NumberOfDcsPaid
    {
      get => numberOfDcsPaid ??= new();
      set => numberOfDcsPaid = value;
    }

    /// <summary>
    /// A value of NumberOfChecksBypassed.
    /// </summary>
    [JsonPropertyName("numberOfChecksBypassed")]
    public Common NumberOfChecksBypassed
    {
      get => numberOfChecksBypassed ??= new();
      set => numberOfChecksBypassed = value;
    }

    /// <summary>
    /// A value of NumberOfEftsBypassed.
    /// </summary>
    [JsonPropertyName("numberOfEftsBypassed")]
    public Common NumberOfEftsBypassed
    {
      get => numberOfEftsBypassed ??= new();
      set => numberOfEftsBypassed = value;
    }

    /// <summary>
    /// A value of NumberOfDcsBypassed.
    /// </summary>
    [JsonPropertyName("numberOfDcsBypassed")]
    public Common NumberOfDcsBypassed
    {
      get => numberOfDcsBypassed ??= new();
      set => numberOfDcsBypassed = value;
    }

    /// <summary>
    /// A value of ChecksReisMoreThan1.
    /// </summary>
    [JsonPropertyName("checksReisMoreThan1")]
    public Common ChecksReisMoreThan1
    {
      get => checksReisMoreThan1 ??= new();
      set => checksReisMoreThan1 = value;
    }

    /// <summary>
    /// A value of EftsReisMoreThan1.
    /// </summary>
    [JsonPropertyName("eftsReisMoreThan1")]
    public Common EftsReisMoreThan1
    {
      get => eftsReisMoreThan1 ??= new();
      set => eftsReisMoreThan1 = value;
    }

    /// <summary>
    /// A value of DcsReisMoreThan1.
    /// </summary>
    [JsonPropertyName("dcsReisMoreThan1")]
    public Common DcsReisMoreThan1
    {
      get => dcsReisMoreThan1 ??= new();
      set => dcsReisMoreThan1 = value;
    }

    private Common eftBelowLastNo;
    private Common dcBelowLastNo;
    private Common checkBelowLastNo;
    private Common noOfChksBypassedMore1;
    private Common noEftsBypassedMore1;
    private Common noDcsBypassedMore1;
    private Common numberOfChecksReissued;
    private Common numberOfEftsReissued;
    private Common numberOfDcsReissued;
    private Common numberOfChecksPaid;
    private Common numberOfEftsPaid;
    private Common numberOfDcsPaid;
    private Common numberOfChecksBypassed;
    private Common numberOfEftsBypassed;
    private Common numberOfDcsBypassed;
    private Common checksReisMoreThan1;
    private Common eftsReisMoreThan1;
    private Common dcsReisMoreThan1;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private Common common;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
