// Program: OE_B447_CLOSE, ID: 945066111, model: 746.
// Short name: SWE04479
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B447_CLOSE.
/// </para>
/// <para>
/// This action block closes all the batch fiels and generates the summary 
/// report for the batch process SWEEB447.
/// </para>
/// </summary>
[Serializable]
public partial class OeB447Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447Close.
  /// </summary>
  public OeB447Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************************************
    // * This Action Block received the SVES Title-XVI information from the 
    // calling object and  *
    // * process them by adding/upding to CSE database and create required 
    // worker alert,income  *
    // * source & document generation wherever 
    // required.
    // 
    // *
    // ******************************************************************************************
    // ******************************************************************************************
    // *                                  
    // Maintenance Log
    // 
    // *
    // ******************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------   
    // --------------------------------------------*
    // * 06/03/2011  Raj S              CQ5577      Initial Coding.
    // *
    // *
    // 
    // *
    // ******************************************************************************************
    // *******************************************************************************************
    // ** Check whether received Titl-XVI record already exists in CSE database 
    // then update     **
    // ** the existing information otherwise create a new Title-XVI response 
    // entry to CSE DB.   **
    // *******************************************************************************************
    // **********************************************************
    // CLOSE ADABAS
    // **********************************************************
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    // **********************************************************
    // CLOSE INPUT FCR SVES RESPONSE FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabReadAllFcrSvesTypeRecs();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "ERROR CLOSING SVES RESPONSE INPUT FILE";
      UseCabErrorReport2();
    }

    // **********************************************************
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.Subscript.Count = 1;
    local.EabFileHandling.Action = "WRITE";

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "FCR SVES RECORDS READ.........................................:" +
            "   " + NumberToString(import.TotSvesInputRecords.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "    SVES T2 PENDING RECORDS READ..............................:" +
            "   " + NumberToString(import.TotSvesT2PendRecords.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "    SVES T2 RECORDS READ......................................:" +
            "   " + NumberToString(import.TotSvesT2Records.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "    SVES T16 RECORDS READ.....................................:" +
            "   " + NumberToString(import.TotSvesT16Records.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "    SVES PRISION RECORDS READ.................................:" +
            "   " + NumberToString(import.TotSvesPrisonRecords.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "    SVES NOT FOUND RECORDS READ...............................:" +
            "   " + NumberToString(import.TotSvesNotfoundRecords.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail = "";

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "    SVES RECORDS SKIPPED......................................:" +
            "   " + NumberToString(import.TotSvesSkipRecords.Count, 15);

          break;
        case 10:
          local.EabReportSend.RptDetail = "";

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "    SVES T2 PENDING RECORDS SKIPPED...........................:" +
            "   " + NumberToString(import.TotT2PendSkipRecs.Count, 15);

          break;
        case 12:
          local.EabReportSend.RptDetail =
            "    SVES T2 RECORDS SKIPPED...................................:" +
            "   " + NumberToString(import.TotT2SkipRecs.Count, 15);

          break;
        case 13:
          local.EabReportSend.RptDetail =
            "    SVES T16 RECORDS SKIPPED..................................:" +
            "   " + NumberToString(import.TotT16SkipRecs.Count, 15);

          break;
        case 14:
          local.EabReportSend.RptDetail =
            "    SVES PRISION RECORDS SKIPPED..............................:" +
            "   " + NumberToString(import.TotPrisonSkipRecs.Count, 15);

          break;
        case 15:
          local.EabReportSend.RptDetail =
            "    SVES NOT FOUND RECORDS SKIPPED............................:" +
            "   " + NumberToString(import.TotNfSkipRecs.Count, 15);

          break;
        case 16:
          local.EabReportSend.RptDetail = "";

          break;
        case 17:
          local.EabReportSend.RptDetail =
            "CSE TOTAL SVES GENRAL INFO RECORDS CREATED....................:" +
            "   " + NumberToString(import.TotSvesGeninfoCreated.Count, 15);

          break;
        case 18:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES GENRAL INFO RECORDS UPDATED....................:" +
            "   " + NumberToString(import.TotSvesGeninfoUpdated.Count, 15);

          break;
        case 19:
          local.EabReportSend.RptDetail = "";

          break;
        case 20:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES RESIDENTIAL ADDRESS RECORDS CREATED............:" +
            "   " + NumberToString(import.TotResaddrCreated.Count, 15);

          break;
        case 21:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES RESIDENTIAL ADDRESS RECORDS UPDATED............:" +
            "   " + NumberToString(import.TotResaddrUpdated.Count, 15);

          break;
        case 22:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PERSON ADDRESS RECORDS CREATED.................:" +
            "   " + NumberToString(import.TotPeraddrCreated.Count, 15);

          break;
        case 23:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PERSON ADDRESS RECORDS UPDATED.................:" +
            "   " + NumberToString(import.TotPeraddrUpdated.Count, 15);

          break;
        case 24:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES DISTRICT OFFICE ADDRESS RECORDS CREATED........:" +
            "   " + NumberToString(import.TotDisaddrCreated.Count, 15);

          break;
        case 25:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES DISTRICT OFFICE ADDRESS RECORDS UPDATED........:" +
            "   " + NumberToString(import.TotDisaddrUpdated.Count, 15);

          break;
        case 26:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PAYEE ADDRESS RECORDS CREATED..................:" +
            "   " + NumberToString(import.TotPayaddrCreated.Count, 15);

          break;
        case 27:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PAYEE ADDRESS RECORDS UPDATED..................:" +
            "   " + NumberToString(import.TotPayaddrUpdated.Count, 15);

          break;
        case 28:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON ADDRESS RECORDS CREATED.................:" +
            "   " + NumberToString(import.TotPriaddrCreated.Count, 15);

          break;
        case 29:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON ADDRESS RECORDS UPDATED.................:" +
            "   " + NumberToString(import.TotPriaddrUpdated.Count, 15);

          break;
        case 30:
          local.EabReportSend.RptDetail = "";

          break;
        case 31:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON ADDRESS RECORDS UPDATED.................:" +
            "   " + NumberToString(import.TotPriaddrUpdated.Count, 15);

          break;
        case 32:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING RECORDS CREATED...............:" +
            "   " + NumberToString(import.TotT2PendCreated.Count, 15);

          break;
        case 33:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING RECORDS UPDATED...............:" +
            "   " + NumberToString(import.TotT2PendUpdated.Count, 15);

          break;
        case 34:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING ALERTS CREATED................:" +
            "   " + NumberToString(import.TotT2PendAlertCreated.Count, 15);

          break;
        case 35:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING EXISTING ALERTS...............:" +
            "   " + NumberToString(import.TotT2PendAlertExists.Count, 15);

          break;
        case 36:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING HISTORY CREATED...............:" +
            "   " + NumberToString(import.TotT2PendHistCreated.Count, 15);

          break;
        case 37:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING EXISTING HISTORY RECORDS......:" +
            "   " + NumberToString(import.TotT2PendHistExists.Count, 15);

          break;
        case 38:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING AR LETTERS GENERATED..........:" +
            "   " + NumberToString(import.TotT2PendArletterCreat.Count, 15);

          break;
        case 39:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II PENDING AP IWOs GENERATED.............:" +
            "   " + NumberToString(import.TotT2PendIwoGenerated.Count, 15);

          break;
        case 40:
          local.EabReportSend.RptDetail = "";

          break;
        case 41:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II RECORDS CREATED.......................:" +
            "   " + NumberToString(import.TotT2Created.Count, 15);

          break;
        case 42:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II RECORDS UPDATED.......................:" +
            "   " + NumberToString(import.TotT2Updated.Count, 15);

          break;
        case 43:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II ALERTS CREATED........................:" +
            "   " + NumberToString(import.TotT2AlertCreated.Count, 15);

          break;
        case 44:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II EXISTING ALERTS.......................:" +
            "   " + NumberToString(import.TotT2AlertExists.Count, 15);

          break;
        case 45:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II HISTORY CREATED.......................:" +
            "   " + NumberToString(import.TotT2HistCreated.Count, 15);

          break;
        case 46:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-II EXISTING HISTORY RECORDS..............:" +
            "   " + NumberToString(import.TotT2HistExists.Count, 15);

          break;
        case 47:
          local.EabReportSend.RptDetail = "";

          break;
        case 48:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-16 RECORDS CREATED.......................:" +
            "   " + NumberToString(import.TotT16Created.Count, 15);

          break;
        case 49:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-16 RECORDS UPDATED.......................:" +
            "   " + NumberToString(import.TotT16Updated.Count, 15);

          break;
        case 50:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-16 ALERTS CREATED........................:" +
            "   " + NumberToString(import.TotT16AlertCreated.Count, 15);

          break;
        case 51:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-16 EXISTING ALERTS.......................:" +
            "   " + NumberToString(import.TotT16AlertExists.Count, 15);

          break;
        case 52:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-16 HISTORY CREATED.......................:" +
            "   " + NumberToString(import.TotT2HistCreated.Count, 15);

          break;
        case 53:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES TITLE-16 EXISTING HISTORY RECORDS..............:" +
            "   " + NumberToString(import.TotT16HistExists.Count, 15);

          break;
        case 54:
          local.EabReportSend.RptDetail = "";

          break;
        case 55:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON RECORDS CREATED.........................:" +
            "   " + NumberToString(import.TotPrisonCreated.Count, 15);

          break;
        case 56:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON RECORDS UPDATED.........................:" +
            "   " + NumberToString(import.TotPrisonUpdated.Count, 15);

          break;
        case 57:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON ALERTS CREATED..........................:" +
            "   " + NumberToString(import.TotPrisonAlertCreated.Count, 15);

          break;
        case 58:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON EXISTING ALERTS.........................:" +
            "   " + NumberToString(import.TotPrisonAlertExists.Count, 15);

          break;
        case 59:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON HISTORY CREATED.........................:" +
            "   " + NumberToString(import.TotPrisonHistCreated.Count, 15);

          break;
        case 60:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES PRISON EXISTING HISTORY RECORDS................:" +
            "   " + NumberToString(import.TotPrisonHistExists.Count, 15);

          break;
        case 61:
          local.EabReportSend.RptDetail = "";

          break;
        case 62:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES NOT FOUND HISTORY CREATED......................:" +
            "   " + NumberToString(import.TotNfHistCreated.Count, 15);

          break;
        case 63:
          local.EabReportSend.RptDetail =
            "    TOTAL SVES NOT FOUND EXISTING HISTORY RECORDS.............:" +
            "   " + NumberToString(import.TotNfHistExists.Count, 15);

          break;
        case 64:
          local.EabReportSend.RptDetail = "";

          break;
        case 65:
          local.EabReportSend.RptDetail =
            "    TOTAL PERSON NOT FOUND RECORDS............................:" +
            "   " + NumberToString(import.TotalPersonNfRecords.Count, 15);

          break;
        case 66:
          local.EabReportSend.RptDetail =
            "    TOTAL BAD SSN RECORDS.....................................:" +
            "   " + NumberToString(import.TotalBadSsnRecords.Count, 15);

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 66);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabReadAllFcrSvesTypeRecs()
  {
    var useImport = new EabReadAllFcrSvesTypeRecs.Import();
    var useExport = new EabReadAllFcrSvesTypeRecs.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadAllFcrSvesTypeRecs.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
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
    /// A value of TotSvesInputRecords.
    /// </summary>
    [JsonPropertyName("totSvesInputRecords")]
    public Common TotSvesInputRecords
    {
      get => totSvesInputRecords ??= new();
      set => totSvesInputRecords = value;
    }

    /// <summary>
    /// A value of TotalPersonNfRecords.
    /// </summary>
    [JsonPropertyName("totalPersonNfRecords")]
    public Common TotalPersonNfRecords
    {
      get => totalPersonNfRecords ??= new();
      set => totalPersonNfRecords = value;
    }

    /// <summary>
    /// A value of TotalBadSsnRecords.
    /// </summary>
    [JsonPropertyName("totalBadSsnRecords")]
    public Common TotalBadSsnRecords
    {
      get => totalBadSsnRecords ??= new();
      set => totalBadSsnRecords = value;
    }

    /// <summary>
    /// A value of TotSvesT2PendRecords.
    /// </summary>
    [JsonPropertyName("totSvesT2PendRecords")]
    public Common TotSvesT2PendRecords
    {
      get => totSvesT2PendRecords ??= new();
      set => totSvesT2PendRecords = value;
    }

    /// <summary>
    /// A value of TotSvesT2Records.
    /// </summary>
    [JsonPropertyName("totSvesT2Records")]
    public Common TotSvesT2Records
    {
      get => totSvesT2Records ??= new();
      set => totSvesT2Records = value;
    }

    /// <summary>
    /// A value of TotSvesT16Records.
    /// </summary>
    [JsonPropertyName("totSvesT16Records")]
    public Common TotSvesT16Records
    {
      get => totSvesT16Records ??= new();
      set => totSvesT16Records = value;
    }

    /// <summary>
    /// A value of TotSvesPrisonRecords.
    /// </summary>
    [JsonPropertyName("totSvesPrisonRecords")]
    public Common TotSvesPrisonRecords
    {
      get => totSvesPrisonRecords ??= new();
      set => totSvesPrisonRecords = value;
    }

    /// <summary>
    /// A value of TotSvesNotfoundRecords.
    /// </summary>
    [JsonPropertyName("totSvesNotfoundRecords")]
    public Common TotSvesNotfoundRecords
    {
      get => totSvesNotfoundRecords ??= new();
      set => totSvesNotfoundRecords = value;
    }

    /// <summary>
    /// A value of TotSvesSkipRecords.
    /// </summary>
    [JsonPropertyName("totSvesSkipRecords")]
    public Common TotSvesSkipRecords
    {
      get => totSvesSkipRecords ??= new();
      set => totSvesSkipRecords = value;
    }

    /// <summary>
    /// A value of TotT2PendSkipRecs.
    /// </summary>
    [JsonPropertyName("totT2PendSkipRecs")]
    public Common TotT2PendSkipRecs
    {
      get => totT2PendSkipRecs ??= new();
      set => totT2PendSkipRecs = value;
    }

    /// <summary>
    /// A value of TotT2SkipRecs.
    /// </summary>
    [JsonPropertyName("totT2SkipRecs")]
    public Common TotT2SkipRecs
    {
      get => totT2SkipRecs ??= new();
      set => totT2SkipRecs = value;
    }

    /// <summary>
    /// A value of TotT16SkipRecs.
    /// </summary>
    [JsonPropertyName("totT16SkipRecs")]
    public Common TotT16SkipRecs
    {
      get => totT16SkipRecs ??= new();
      set => totT16SkipRecs = value;
    }

    /// <summary>
    /// A value of TotPrisonSkipRecs.
    /// </summary>
    [JsonPropertyName("totPrisonSkipRecs")]
    public Common TotPrisonSkipRecs
    {
      get => totPrisonSkipRecs ??= new();
      set => totPrisonSkipRecs = value;
    }

    /// <summary>
    /// A value of TotNfSkipRecs.
    /// </summary>
    [JsonPropertyName("totNfSkipRecs")]
    public Common TotNfSkipRecs
    {
      get => totNfSkipRecs ??= new();
      set => totNfSkipRecs = value;
    }

    /// <summary>
    /// A value of TotSvesGeninfoCreated.
    /// </summary>
    [JsonPropertyName("totSvesGeninfoCreated")]
    public Common TotSvesGeninfoCreated
    {
      get => totSvesGeninfoCreated ??= new();
      set => totSvesGeninfoCreated = value;
    }

    /// <summary>
    /// A value of TotSvesGeninfoUpdated.
    /// </summary>
    [JsonPropertyName("totSvesGeninfoUpdated")]
    public Common TotSvesGeninfoUpdated
    {
      get => totSvesGeninfoUpdated ??= new();
      set => totSvesGeninfoUpdated = value;
    }

    /// <summary>
    /// A value of TotResaddrCreated.
    /// </summary>
    [JsonPropertyName("totResaddrCreated")]
    public Common TotResaddrCreated
    {
      get => totResaddrCreated ??= new();
      set => totResaddrCreated = value;
    }

    /// <summary>
    /// A value of TotResaddrUpdated.
    /// </summary>
    [JsonPropertyName("totResaddrUpdated")]
    public Common TotResaddrUpdated
    {
      get => totResaddrUpdated ??= new();
      set => totResaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotPeraddrCreated.
    /// </summary>
    [JsonPropertyName("totPeraddrCreated")]
    public Common TotPeraddrCreated
    {
      get => totPeraddrCreated ??= new();
      set => totPeraddrCreated = value;
    }

    /// <summary>
    /// A value of TotPeraddrUpdated.
    /// </summary>
    [JsonPropertyName("totPeraddrUpdated")]
    public Common TotPeraddrUpdated
    {
      get => totPeraddrUpdated ??= new();
      set => totPeraddrUpdated = value;
    }

    /// <summary>
    /// A value of TotDisaddrCreated.
    /// </summary>
    [JsonPropertyName("totDisaddrCreated")]
    public Common TotDisaddrCreated
    {
      get => totDisaddrCreated ??= new();
      set => totDisaddrCreated = value;
    }

    /// <summary>
    /// A value of TotDisaddrUpdated.
    /// </summary>
    [JsonPropertyName("totDisaddrUpdated")]
    public Common TotDisaddrUpdated
    {
      get => totDisaddrUpdated ??= new();
      set => totDisaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotPayaddrCreated.
    /// </summary>
    [JsonPropertyName("totPayaddrCreated")]
    public Common TotPayaddrCreated
    {
      get => totPayaddrCreated ??= new();
      set => totPayaddrCreated = value;
    }

    /// <summary>
    /// A value of TotPayaddrUpdated.
    /// </summary>
    [JsonPropertyName("totPayaddrUpdated")]
    public Common TotPayaddrUpdated
    {
      get => totPayaddrUpdated ??= new();
      set => totPayaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotPriaddrCreated.
    /// </summary>
    [JsonPropertyName("totPriaddrCreated")]
    public Common TotPriaddrCreated
    {
      get => totPriaddrCreated ??= new();
      set => totPriaddrCreated = value;
    }

    /// <summary>
    /// A value of TotPriaddrUpdated.
    /// </summary>
    [JsonPropertyName("totPriaddrUpdated")]
    public Common TotPriaddrUpdated
    {
      get => totPriaddrUpdated ??= new();
      set => totPriaddrUpdated = value;
    }

    /// <summary>
    /// A value of TotT2PendCreated.
    /// </summary>
    [JsonPropertyName("totT2PendCreated")]
    public Common TotT2PendCreated
    {
      get => totT2PendCreated ??= new();
      set => totT2PendCreated = value;
    }

    /// <summary>
    /// A value of TotT2PendUpdated.
    /// </summary>
    [JsonPropertyName("totT2PendUpdated")]
    public Common TotT2PendUpdated
    {
      get => totT2PendUpdated ??= new();
      set => totT2PendUpdated = value;
    }

    /// <summary>
    /// A value of TotT2PendAlertCreated.
    /// </summary>
    [JsonPropertyName("totT2PendAlertCreated")]
    public Common TotT2PendAlertCreated
    {
      get => totT2PendAlertCreated ??= new();
      set => totT2PendAlertCreated = value;
    }

    /// <summary>
    /// A value of TotT2PendHistCreated.
    /// </summary>
    [JsonPropertyName("totT2PendHistCreated")]
    public Common TotT2PendHistCreated
    {
      get => totT2PendHistCreated ??= new();
      set => totT2PendHistCreated = value;
    }

    /// <summary>
    /// A value of TotT2PendAlertExists.
    /// </summary>
    [JsonPropertyName("totT2PendAlertExists")]
    public Common TotT2PendAlertExists
    {
      get => totT2PendAlertExists ??= new();
      set => totT2PendAlertExists = value;
    }

    /// <summary>
    /// A value of TotT2PendHistExists.
    /// </summary>
    [JsonPropertyName("totT2PendHistExists")]
    public Common TotT2PendHistExists
    {
      get => totT2PendHistExists ??= new();
      set => totT2PendHistExists = value;
    }

    /// <summary>
    /// A value of TotT2PendArletterCreat.
    /// </summary>
    [JsonPropertyName("totT2PendArletterCreat")]
    public Common TotT2PendArletterCreat
    {
      get => totT2PendArletterCreat ??= new();
      set => totT2PendArletterCreat = value;
    }

    /// <summary>
    /// A value of TotT2PendIwoGenerated.
    /// </summary>
    [JsonPropertyName("totT2PendIwoGenerated")]
    public Common TotT2PendIwoGenerated
    {
      get => totT2PendIwoGenerated ??= new();
      set => totT2PendIwoGenerated = value;
    }

    /// <summary>
    /// A value of TotT2Created.
    /// </summary>
    [JsonPropertyName("totT2Created")]
    public Common TotT2Created
    {
      get => totT2Created ??= new();
      set => totT2Created = value;
    }

    /// <summary>
    /// A value of TotT2Updated.
    /// </summary>
    [JsonPropertyName("totT2Updated")]
    public Common TotT2Updated
    {
      get => totT2Updated ??= new();
      set => totT2Updated = value;
    }

    /// <summary>
    /// A value of TotT2AlertCreated.
    /// </summary>
    [JsonPropertyName("totT2AlertCreated")]
    public Common TotT2AlertCreated
    {
      get => totT2AlertCreated ??= new();
      set => totT2AlertCreated = value;
    }

    /// <summary>
    /// A value of TotT2AlertExists.
    /// </summary>
    [JsonPropertyName("totT2AlertExists")]
    public Common TotT2AlertExists
    {
      get => totT2AlertExists ??= new();
      set => totT2AlertExists = value;
    }

    /// <summary>
    /// A value of TotT2HistCreated.
    /// </summary>
    [JsonPropertyName("totT2HistCreated")]
    public Common TotT2HistCreated
    {
      get => totT2HistCreated ??= new();
      set => totT2HistCreated = value;
    }

    /// <summary>
    /// A value of TotT2HistExists.
    /// </summary>
    [JsonPropertyName("totT2HistExists")]
    public Common TotT2HistExists
    {
      get => totT2HistExists ??= new();
      set => totT2HistExists = value;
    }

    /// <summary>
    /// A value of TotT16Created.
    /// </summary>
    [JsonPropertyName("totT16Created")]
    public Common TotT16Created
    {
      get => totT16Created ??= new();
      set => totT16Created = value;
    }

    /// <summary>
    /// A value of TotT16Updated.
    /// </summary>
    [JsonPropertyName("totT16Updated")]
    public Common TotT16Updated
    {
      get => totT16Updated ??= new();
      set => totT16Updated = value;
    }

    /// <summary>
    /// A value of TotT16AlertCreated.
    /// </summary>
    [JsonPropertyName("totT16AlertCreated")]
    public Common TotT16AlertCreated
    {
      get => totT16AlertCreated ??= new();
      set => totT16AlertCreated = value;
    }

    /// <summary>
    /// A value of TotT16AlertExists.
    /// </summary>
    [JsonPropertyName("totT16AlertExists")]
    public Common TotT16AlertExists
    {
      get => totT16AlertExists ??= new();
      set => totT16AlertExists = value;
    }

    /// <summary>
    /// A value of TotT16HistCreated.
    /// </summary>
    [JsonPropertyName("totT16HistCreated")]
    public Common TotT16HistCreated
    {
      get => totT16HistCreated ??= new();
      set => totT16HistCreated = value;
    }

    /// <summary>
    /// A value of TotT16HistExists.
    /// </summary>
    [JsonPropertyName("totT16HistExists")]
    public Common TotT16HistExists
    {
      get => totT16HistExists ??= new();
      set => totT16HistExists = value;
    }

    /// <summary>
    /// A value of TotPrisonCreated.
    /// </summary>
    [JsonPropertyName("totPrisonCreated")]
    public Common TotPrisonCreated
    {
      get => totPrisonCreated ??= new();
      set => totPrisonCreated = value;
    }

    /// <summary>
    /// A value of TotPrisonUpdated.
    /// </summary>
    [JsonPropertyName("totPrisonUpdated")]
    public Common TotPrisonUpdated
    {
      get => totPrisonUpdated ??= new();
      set => totPrisonUpdated = value;
    }

    /// <summary>
    /// A value of TotPrisonAlertCreated.
    /// </summary>
    [JsonPropertyName("totPrisonAlertCreated")]
    public Common TotPrisonAlertCreated
    {
      get => totPrisonAlertCreated ??= new();
      set => totPrisonAlertCreated = value;
    }

    /// <summary>
    /// A value of TotPrisonAlertExists.
    /// </summary>
    [JsonPropertyName("totPrisonAlertExists")]
    public Common TotPrisonAlertExists
    {
      get => totPrisonAlertExists ??= new();
      set => totPrisonAlertExists = value;
    }

    /// <summary>
    /// A value of TotPrisonHistCreated.
    /// </summary>
    [JsonPropertyName("totPrisonHistCreated")]
    public Common TotPrisonHistCreated
    {
      get => totPrisonHistCreated ??= new();
      set => totPrisonHistCreated = value;
    }

    /// <summary>
    /// A value of TotPrisonHistExists.
    /// </summary>
    [JsonPropertyName("totPrisonHistExists")]
    public Common TotPrisonHistExists
    {
      get => totPrisonHistExists ??= new();
      set => totPrisonHistExists = value;
    }

    /// <summary>
    /// A value of TotNfHistCreated.
    /// </summary>
    [JsonPropertyName("totNfHistCreated")]
    public Common TotNfHistCreated
    {
      get => totNfHistCreated ??= new();
      set => totNfHistCreated = value;
    }

    /// <summary>
    /// A value of TotNfHistExists.
    /// </summary>
    [JsonPropertyName("totNfHistExists")]
    public Common TotNfHistExists
    {
      get => totNfHistExists ??= new();
      set => totNfHistExists = value;
    }

    private Common totSvesInputRecords;
    private Common totalPersonNfRecords;
    private Common totalBadSsnRecords;
    private Common totSvesT2PendRecords;
    private Common totSvesT2Records;
    private Common totSvesT16Records;
    private Common totSvesPrisonRecords;
    private Common totSvesNotfoundRecords;
    private Common totSvesSkipRecords;
    private Common totT2PendSkipRecs;
    private Common totT2SkipRecs;
    private Common totT16SkipRecs;
    private Common totPrisonSkipRecs;
    private Common totNfSkipRecs;
    private Common totSvesGeninfoCreated;
    private Common totSvesGeninfoUpdated;
    private Common totResaddrCreated;
    private Common totResaddrUpdated;
    private Common totPeraddrCreated;
    private Common totPeraddrUpdated;
    private Common totDisaddrCreated;
    private Common totDisaddrUpdated;
    private Common totPayaddrCreated;
    private Common totPayaddrUpdated;
    private Common totPriaddrCreated;
    private Common totPriaddrUpdated;
    private Common totT2PendCreated;
    private Common totT2PendUpdated;
    private Common totT2PendAlertCreated;
    private Common totT2PendHistCreated;
    private Common totT2PendAlertExists;
    private Common totT2PendHistExists;
    private Common totT2PendArletterCreat;
    private Common totT2PendIwoGenerated;
    private Common totT2Created;
    private Common totT2Updated;
    private Common totT2AlertCreated;
    private Common totT2AlertExists;
    private Common totT2HistCreated;
    private Common totT2HistExists;
    private Common totT16Created;
    private Common totT16Updated;
    private Common totT16AlertCreated;
    private Common totT16AlertExists;
    private Common totT16HistCreated;
    private Common totT16HistExists;
    private Common totPrisonCreated;
    private Common totPrisonUpdated;
    private Common totPrisonAlertCreated;
    private Common totPrisonAlertExists;
    private Common totPrisonHistCreated;
    private Common totPrisonHistExists;
    private Common totNfHistCreated;
    private Common totNfHistExists;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
  }
#endregion
}
