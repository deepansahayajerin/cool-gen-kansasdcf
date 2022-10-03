// Program: SI_B277_CREATE_NEWHIRE_REPORT, ID: 372746068, model: 746.
// Short name: SWEI277B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B277_CREATE_NEWHIRE_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB277CreateNewhireReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B277_CREATE_NEWHIRE_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB277CreateNewhireReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB277CreateNewhireReport.
  /// </summary>
  public SiB277CreateNewhireReport(IContext context, Import import,
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
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 09/17/1999  Ed Lyman   H00074376   Print one record per page.  Add  *
    // *
    // 
    // office number to report.  Make   *
    // *
    // 
    // report narrow for screen prints. *
    // *
    // 
    // *
    // * 05/15/2000  Ed Lyman   PR# 88198   Add KS Employer Id and EIN to eab*
    // *
    // 
    // and report.                      *
    // *
    // 
    // *
    // * 05/15/2000  Ed Lyman   PR# 90538   Report is displaying an inactive *
    // *
    // 
    // service provider.                *
    // *
    // 
    // *
    // * 05/15/2000  Ed Lyman   PR# 91162   Report is displaying an inactive *
    // *
    // 
    // case.                            *
    // *
    // 
    // *
    // * 06/12/2000  G Vandy    PR# 88198   Add KS Employer Id and EIN to eab*
    // *
    // 
    // and report.                      *
    // *
    // 
    // *
    // * 06/22/2000  G Vandy    PR# 91162   Report is displaying an inactive *
    // *
    // 
    // case.                            *
    // *
    // 
    // *
    // * 06/22/2000  G Vandy    PR# 97551   Do not process new hires with    *
    // *
    // 
    // date of hire more than 6 months  *
    // *
    // 
    // in the past.                     *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    UseSiB277Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadNewHireFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "ERROR READING DHR INPUT FILE";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;

      if (Lt(local.Start.Date, AddMonths(local.Process.Date, -6)))
      {
        // 06/22/2000  G Vandy    PR# 97551   Do not process new hires with date
        // of hire more than 6 months in the past.
        ++local.RecordsSkipped.Count;
        local.EabReportSend.RptDetail = "Record Skipped - AP # " + local
          .Obligor.Number;
        local.DateOfHire.Text10 =
          NumberToString(Month(local.Start.Date), 14, 2) + "/" + NumberToString
          (Day(local.Start.Date), 14, 2) + "/" + NumberToString
          (Year(local.Start.Date), 12, 4);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " - Hire Date (" + local
          .DateOfHire.Text10 + ") is more than 6 months in the past.";
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "ERROR WRITING ERROR REPORT";
          UseCabErrorReport2();
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

      local.Office.Name = "";
      UseSiB277FindOfficeAndRsp();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        for(local.Counter.Count = 1; local.Counter.Count <= 20; ++
          local.Counter.Count)
        {
          // Write the report.
          local.EabFileHandling.Action = "WRITE";

          switch(local.Counter.Count)
          {
            case 1:
              if (local.Previous.SystemGeneratedId > 0)
              {
                local.EabFileHandling.Action = "NEWPAGE";
              }
              else
              {
                continue;
              }

              break;
            case 2:
              if (local.Office.SystemGeneratedId != local
                .Previous.SystemGeneratedId)
              {
                MoveOffice(local.Office, local.Previous);
                ++local.OfficesRead.Count;
              }

              local.NeededToWrite.RptDetail = "OFFICE/RSP: " + NumberToString
                (local.Office.SystemGeneratedId, 12, 4) + " " + TrimEnd
                (local.Office.Name) + " / " + TrimEnd
                (local.CaseWorker.LastName) + ", " + TrimEnd
                (local.CaseWorker.FirstName) + " " + local
                .CaseWorker.MiddleInitial;

              break;
            case 3:
              local.NeededToWrite.RptDetail =
                "                         AP SSN: " + Substring
                (local.Obligor.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-"
                + Substring
                (local.Obligor.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-"
                + Substring
                (local.Obligor.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4) + " / " +
                "CSE CASE NUMBER:  " + local.Case1.Number;

              break;
            case 4:
              local.NeededToWrite.RptDetail =
                "                                               AP PERSON NUMBER: " +
                local.Obligor.Number;

              break;
            case 5:
              local.NeededToWrite.RptDetail = "AP NAME:" + "           " + TrimEnd
                (local.Obligor.LastName) + ", " + TrimEnd
                (local.Obligor.FirstName) + " " + local.Obligor.MiddleInitial;

              break;
            case 6:
              local.NeededToWrite.RptDetail = "AP ADDRESS:        " + Substring
                (local.ObligorStreetAddr.TextLine80,
                External.TextLine80_MaxLength, 1, 40);

              break;
            case 7:
              if (!IsEmpty(Substring(local.ObligorStreetAddr.TextLine80, 41, 40)))
                
              {
                local.NeededToWrite.RptDetail = "                   " + Substring
                  (local.ObligorStreetAddr.TextLine80,
                  External.TextLine80_MaxLength, 41, 40);
              }
              else
              {
                continue;
              }

              break;
            case 8:
              if (!IsEmpty(local.ObligorAddress.Zip4))
              {
                local.NeededToWrite.RptDetail = "CITY, ST ZIP:      " + TrimEnd
                  (local.ObligorCity.Text30) + ", " + (
                    local.ObligorAddress.State ?? "") + " " + (
                    local.ObligorAddress.ZipCode ?? "") + " - " + (
                    local.ObligorAddress.Zip4 ?? "");
              }
              else
              {
                local.NeededToWrite.RptDetail = "CITY, ST ZIP:      " + TrimEnd
                  (local.ObligorCity.Text30) + ", " + (
                    local.ObligorAddress.State ?? "") + " " + (
                    local.ObligorAddress.ZipCode ?? "") + "" + "";
              }

              break;
            case 9:
              local.NeededToWrite.RptDetail = "";

              break;
            case 10:
              // **********************************************************************
              // * BEGIN EMPLOYER INFORMATION
              // **********************************************************************
              local.NeededToWrite.RptDetail =
                "EMPLOYER NAME AND ADDRESS:                     SEND IWO REQUESTS TO:";
                

              break;
            case 11:
              // ** 06/12/2000  G Vandy    PR# 88198   Add KS Employer Id and 
              // EIN to eab and report.
              local.NeededToWrite.RptDetail = "KSID: " + (
                local.Employer.KansasId ?? "") + "  EIN: " + (
                  local.Employer.Ein ?? "");

              break;
            case 12:
              local.NeededToWrite.RptDetail = local.CompanyName.TextLine80;

              break;
            case 13:
              local.NeededToWrite.RptDetail =
                Substring(local.CompanyStreetAddr.TextLine80, 1, 40);

              break;
            case 14:
              if (!IsEmpty(Substring(local.CompanyStreetAddr.TextLine80, 41, 40)))
                
              {
                local.NeededToWrite.RptDetail =
                  Substring(local.CompanyStreetAddr.TextLine80, 41, 40);
              }
              else
              {
                continue;
              }

              break;
            case 15:
              if (!IsEmpty(local.CompanyAddress.Zip4))
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.CompanyCity.Text30) + ", " + (
                    local.CompanyAddress.State ?? "") + " " + (
                    local.CompanyAddress.ZipCode ?? "") + " - " + (
                    local.CompanyAddress.Zip4 ?? "");
              }
              else
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.CompanyCity.Text30) + ", " + (
                    local.CompanyAddress.State ?? "") + " " + (
                    local.CompanyAddress.ZipCode ?? "") + "" + "";
              }

              break;
            case 16:
              local.NeededToWrite.RptDetail = "";

              break;
            case 17:
              local.DateOfHire.Text10 =
                NumberToString(Month(local.Start.Date), 14, 2) + "/" + NumberToString
                (Day(local.Start.Date), 14, 2) + "/" + NumberToString
                (Year(local.Start.Date), 12, 4);
              local.NeededToWrite.RptDetail = "DATE OF HIRE:   " + local
                .DateOfHire.Text10;

              break;
            case 18:
              local.NeededToWrite.RptDetail = "STATE HIRED IN: " + (
                local.StateHiredIn.State ?? "");

              break;
            case 19:
              local.NeededToWrite.RptDetail =
                "CONFIDENTIAL - ONLY DISCLOSE AS AUTHORIZED BY K.S.A. 39-759.";

              break;
            case 20:
              local.NeededToWrite.RptDetail = "";

              break;
            default:
              break;
          }

          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "ERROR WRITING REPORT";
            UseCabErrorReport2();
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto AfterCycle;
          }
        }
      }
      else
      {
        // An error was encountered.  Write to the error report.
        ++local.RecordsSkipped.Count;
        local.EabReportSend.RptDetail = "Record Skipped - AP # " + local
          .Obligor.Number;

        if (IsExitState("SC0000_CASE_IS_NOT_OPEN"))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Case # " + local
            .Case1.Number + " is not open.";
        }
        else if (IsExitState("CASE_NF"))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Case # " + local
            .Case1.Number + " was not found.";
        }
        else if (IsExitState("OFFICE_SERVICE_PROVIDER_NF"))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Office Service Provider was not found.";
            
        }
        else if (IsExitState("OFFICE_NF"))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Office was not found.";
            
        }
        else if (IsExitState("SERVICE_PROVIDER_NF"))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Service Provider was not found.";
            
        }
        else if (IsExitState("CASE_ASSIGNMENT_NF"))
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Case Assignment was not found.";
            
        }
        else
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " - Unknown error found.";
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "ERROR WRITING ERROR REPORT";
          UseCabErrorReport2();
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB277Close();
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB277Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.State = source.State;
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.KansasId = source.KansasId;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabReadNewHireFile()
  {
    var useImport = new EabReadNewHireFile.Import();
    var useExport = new EabReadNewHireFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;
    useExport.Case1.Number = local.Case1.Number;
    useExport.CsePersonsWorkSet.Assign(local.Obligor);
    useExport.ObligorStreetAddr.TextLine80 = local.ObligorStreetAddr.TextLine80;
    useExport.ObligorCity.Text30 = local.ObligorCity.Text30;
    useExport.ObligorAddress.Assign(local.ObligorAddress);
    MoveCsePersonAddress(local.Business, useExport.Business);
    useExport.Start.Date = local.Start.Date;
    useExport.CompanyName.TextLine80 = local.CompanyName.TextLine80;
    useExport.CompanyStreetAddr.TextLine80 = local.CompanyStreetAddr.TextLine80;
    useExport.CompanyCity.Text30 = local.CompanyCity.Text30;
    useExport.CompanyAddress.Assign(local.CompanyAddress);
    MoveCsePersonAddress(local.StateHiredIn, useExport.StateHiredIn);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    MoveEmployer(local.Employer, useExport.Employer);

    Call(EabReadNewHireFile.Execute, useImport, useExport);

    local.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    local.Case1.Number = useExport.Case1.Number;
    local.Obligor.Assign(useExport.CsePersonsWorkSet);
    local.ObligorStreetAddr.TextLine80 = useExport.ObligorStreetAddr.TextLine80;
    local.ObligorCity.Text30 = useExport.ObligorCity.Text30;
    local.ObligorAddress.Assign(useExport.ObligorAddress);
    MoveCsePersonAddress(useExport.Business, local.Business);
    local.Start.Date = useExport.Start.Date;
    local.CompanyName.TextLine80 = useExport.CompanyName.TextLine80;
    local.CompanyStreetAddr.TextLine80 = useExport.CompanyStreetAddr.TextLine80;
    local.CompanyCity.Text30 = useExport.CompanyCity.Text30;
    local.CompanyAddress.Assign(useExport.CompanyAddress);
    MoveCsePersonAddress(useExport.StateHiredIn, local.StateHiredIn);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveEmployer(useExport.Employer, local.Employer);
  }

  private void UseSiB277Close()
  {
    var useImport = new SiB277Close.Import();
    var useExport = new SiB277Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.OfficesRead.Count = local.OfficesRead.Count;
    useImport.RecordsSkipped.Count = local.RecordsSkipped.Count;

    Call(SiB277Close.Execute, useImport, useExport);
  }

  private void UseSiB277FindOfficeAndRsp()
  {
    var useImport = new SiB277FindOfficeAndRsp.Import();
    var useExport = new SiB277FindOfficeAndRsp.Export();

    useImport.Case1.Number = local.Case1.Number;
    MoveOffice(local.Office, useImport.Office);
    useImport.ProcessDate.Date = local.Process.Date;

    Call(SiB277FindOfficeAndRsp.Execute, useImport, useExport);

    local.CaseWorker.Assign(useExport.CaseWorker);
    MoveOffice(useExport.Office, local.Office);
  }

  private void UseSiB277Housekeeping()
  {
    var useImport = new SiB277Housekeeping.Import();
    var useExport = new SiB277Housekeeping.Export();

    Call(SiB277Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
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
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of RecordsSkipped.
    /// </summary>
    [JsonPropertyName("recordsSkipped")]
    public Common RecordsSkipped
    {
      get => recordsSkipped ??= new();
      set => recordsSkipped = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Office Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of StateHiredIn.
    /// </summary>
    [JsonPropertyName("stateHiredIn")]
    public CsePersonAddress StateHiredIn
    {
      get => stateHiredIn ??= new();
      set => stateHiredIn = value;
    }

    /// <summary>
    /// A value of CaseWorker.
    /// </summary>
    [JsonPropertyName("caseWorker")]
    public CsePersonsWorkSet CaseWorker
    {
      get => caseWorker ??= new();
      set => caseWorker = value;
    }

    /// <summary>
    /// A value of DateOfHire.
    /// </summary>
    [JsonPropertyName("dateOfHire")]
    public TextWorkArea DateOfHire
    {
      get => dateOfHire ??= new();
      set => dateOfHire = value;
    }

    /// <summary>
    /// A value of CompanyAddress.
    /// </summary>
    [JsonPropertyName("companyAddress")]
    public CsePersonAddress CompanyAddress
    {
      get => companyAddress ??= new();
      set => companyAddress = value;
    }

    /// <summary>
    /// A value of CompanyCity.
    /// </summary>
    [JsonPropertyName("companyCity")]
    public TextWorkArea CompanyCity
    {
      get => companyCity ??= new();
      set => companyCity = value;
    }

    /// <summary>
    /// A value of CompanyStreetAddr.
    /// </summary>
    [JsonPropertyName("companyStreetAddr")]
    public External CompanyStreetAddr
    {
      get => companyStreetAddr ??= new();
      set => companyStreetAddr = value;
    }

    /// <summary>
    /// A value of CompanyName.
    /// </summary>
    [JsonPropertyName("companyName")]
    public External CompanyName
    {
      get => companyName ??= new();
      set => companyName = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Business.
    /// </summary>
    [JsonPropertyName("business")]
    public CsePersonAddress Business
    {
      get => business ??= new();
      set => business = value;
    }

    /// <summary>
    /// A value of ObligorAddress.
    /// </summary>
    [JsonPropertyName("obligorAddress")]
    public CsePersonAddress ObligorAddress
    {
      get => obligorAddress ??= new();
      set => obligorAddress = value;
    }

    /// <summary>
    /// A value of ObligorCity.
    /// </summary>
    [JsonPropertyName("obligorCity")]
    public TextWorkArea ObligorCity
    {
      get => obligorCity ??= new();
      set => obligorCity = value;
    }

    /// <summary>
    /// A value of ObligorStreetAddr.
    /// </summary>
    [JsonPropertyName("obligorStreetAddr")]
    public External ObligorStreetAddr
    {
      get => obligorStreetAddr ??= new();
      set => obligorStreetAddr = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ObligorIsNotAnAp.
    /// </summary>
    [JsonPropertyName("obligorIsNotAnAp")]
    public Common ObligorIsNotAnAp
    {
      get => obligorIsNotAnAp ??= new();
      set => obligorIsNotAnAp = value;
    }

    /// <summary>
    /// A value of OfficesRead.
    /// </summary>
    [JsonPropertyName("officesRead")]
    public Common OfficesRead
    {
      get => officesRead ??= new();
      set => officesRead = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of QueriesCreated.
    /// </summary>
    [JsonPropertyName("queriesCreated")]
    public Common QueriesCreated
    {
      get => queriesCreated ??= new();
      set => queriesCreated = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private Common counter;
    private Common recordsSkipped;
    private Employer employer;
    private Office office;
    private Office previous;
    private CsePersonAddress stateHiredIn;
    private CsePersonsWorkSet caseWorker;
    private TextWorkArea dateOfHire;
    private CsePersonAddress companyAddress;
    private TextWorkArea companyCity;
    private External companyStreetAddr;
    private External companyName;
    private DateWorkArea start;
    private CsePersonAddress business;
    private CsePersonAddress obligorAddress;
    private TextWorkArea obligorCity;
    private External obligorStreetAddr;
    private EabReportSend neededToWrite;
    private Case1 case1;
    private Common obligorIsNotAnAp;
    private Common officesRead;
    private Common recordsRead;
    private Common queriesCreated;
    private CsePersonsWorkSet obligor;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private EabReportSend eabReportSend;
  }
#endregion
}
