// Program: CAB_FDSO_RETURN_MESSAGE, ID: 372668118, model: 746.
// Short name: SWE02373
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_FDSO_RETURN_MESSAGE.
/// </para>
/// <para>
/// Provide the return error code from OCSE and this CAB will send back the 
/// error message.
/// </para>
/// </summary>
[Serializable]
public partial class CabFdsoReturnMessage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FDSO_RETURN_MESSAGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFdsoReturnMessage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFdsoReturnMessage.
  /// </summary>
  public CabFdsoReturnMessage(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    switch(TrimEnd(import.FdsoReturnH.Errcode1))
    {
      case "01":
        export.EabReportSend.RptDetail = "Invalid Submitting State Code";

        break;
      case "02":
        export.EabReportSend.RptDetail = "Invalid SSN";

        break;
      case "03":
        export.EabReportSend.RptDetail = "Invalid Last Name";

        break;
      case "04":
        export.EabReportSend.RptDetail = "Invalid First Name";

        break;
      case "05":
        // *** Invalid/No change in Arrearage Amount
        export.EabReportSend.RptDetail = " Invalid Arrearage Amount";

        break;
      case "06":
        export.EabReportSend.RptDetail = "Case Already Exists for an ADD";

        break;
      case "07":
        export.EabReportSend.RptDetail = "Invalid Transaction Type code";

        break;
      case "08":
        // *** Duplicate Transaction Request for Same Cycle
        export.EabReportSend.RptDetail =
          "Duplicate Transaction Request for Same Cycle";

        break;
      case "09":
        export.EabReportSend.RptDetail = "Invalid Case Type Indicator";

        break;
      case "11":
        export.EabReportSend.RptDetail =
          "State Payment Amount Submitted is equal to the OCSE Case Master File";
          

        break;
      case "12":
        export.EabReportSend.RptDetail = "SSN not on OCSE master file";

        break;
      case "17":
        export.EabReportSend.RptDetail = "Last Name Does Not Match SSN";

        break;
      case "19":
        export.EabReportSend.RptDetail =
          "FMS cannot decrease a debt with existing balance of $0";

        break;
      case "23":
        export.EabReportSend.RptDetail = "Invalid Transfer State";

        break;
      case "26":
        export.EabReportSend.RptDetail =
          "Invalid Processing Year for State Payment";

        break;
      case "29":
        export.EabReportSend.RptDetail =
          "Invalid Local Code Change for action code \"L\"";

        break;
      case "32":
        export.EabReportSend.RptDetail = "Invalid Pre-Offset Notice Issue Date";

        break;
      case "33":
        export.EabReportSend.RptDetail = "Invalid Exclusion Indicators";

        break;
      case "34":
        export.EabReportSend.RptDetail =
          "State Payment Submitted, But No Offset Payment Found for the Offset Year";
          

        break;
      case "35":
        export.EabReportSend.RptDetail = "State Payment Amount Exceeds Offset";

        break;
      case "38":
        export.EabReportSend.RptDetail = "Case Was Previously Deleted";

        break;
      default:
        export.EabReportSend.RptDetail = "*** Invalid Error Code = " + import
          .FdsoReturnH.Errcode1;

        break;
    }

    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = export.EabReportSend.RptDetail;
    UseCabErrorReport();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// A value of FdsoReturnH.
    /// </summary>
    [JsonPropertyName("fdsoReturnH")]
    public FdsoReturnH FdsoReturnH
    {
      get => fdsoReturnH ??= new();
      set => fdsoReturnH = value;
    }

    private FdsoReturnH fdsoReturnH;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
