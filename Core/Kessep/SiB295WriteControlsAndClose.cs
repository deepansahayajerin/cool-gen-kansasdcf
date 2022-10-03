// Program: SI_B295_WRITE_CONTROLS_AND_CLOSE, ID: 372763991, model: 746.
// Short name: SWE02469
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B295_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SiB295WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B295_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB295WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB295WriteControlsAndClose.
  /// </summary>
  public SiB295WriteControlsAndClose(IContext context, Import import,
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
    local.MaxControlTotal.Count = 99;

    // -----------------------------------------------------------------
    // WRITE OUTPUT CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.Label.Text60 = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Label.Text60 = "Records read from CSENet IN file";
          local.Textnum.Text15 = NumberToString(import.Read.Count, 15);

          break;
        case 2:
          local.Label.Text60 =
            "Interstate Cases created from CSENet Incoming transactions";
          local.Textnum.Text15 =
            NumberToString(import.InterstateCaseCreates.Count, 15);

          break;
        case 3:
          local.Label.Text60 = "Non IV-D Referrals rejected";
          local.Textnum.Text15 = NumberToString(import.NonIvdReject.Count, 15);

          break;
        case 4:
          local.Label.Text60 = "Interstate AP Identification records created";
          local.Textnum.Text15 =
            NumberToString(import.ApIdentCreates.Count, 15);

          break;
        case 5:
          local.Label.Text60 = "Interstate AP Locate records created";
          local.Textnum.Text15 =
            NumberToString(import.ApLocateCreates.Count, 15);

          break;
        case 6:
          local.Label.Text60 = "Interstate Participant records created";
          local.Textnum.Text15 =
            NumberToString(import.ParticipantCreates.Count, 15);

          break;
        case 7:
          local.Label.Text60 = "Interstate Support Order records created";
          local.Textnum.Text15 = NumberToString(import.OrderCreates.Count, 15);

          break;
        case 8:
          local.Label.Text60 = "Interstate Collection records created";
          local.Textnum.Text15 =
            NumberToString(import.CollectionCreates.Count, 15);

          break;
        case 9:
          local.Label.Text60 = "Interstate Miscellaneous records created";
          local.Textnum.Text15 = NumberToString(import.MiscCreates.Count, 15);

          break;
        case 10:
          local.Label.Text60 = "Interstate Contact records created";
          local.Textnum.Text15 =
            NumberToString(import.InterstContactCreates.Count, 15);

          break;
        case 11:
          local.Label.Text60 = "Interstate Contact records updated";
          local.Textnum.Text15 =
            NumberToString(import.InterstContactUpdates.Count, 15);

          break;
        case 12:
          local.Label.Text60 = "Interstate Contact Address records created";
          local.Textnum.Text15 =
            NumberToString(import.ContactAddrCreates.Count, 15);

          break;
        case 13:
          local.Label.Text60 = "Interstate Contact Address records updated";
          local.Textnum.Text15 =
            NumberToString(import.ContactAddrUpdates.Count, 15);

          break;
        case 14:
          local.Label.Text60 = "Interstate Payment Address records created";
          local.Textnum.Text15 =
            NumberToString(import.PaymentAddrCreates.Count, 15);

          break;
        case 15:
          local.Label.Text60 = "Interstate Payment Address records updated";
          local.Textnum.Text15 =
            NumberToString(import.PaymentAddrUpdates.Count, 15);

          break;
        case 16:
          local.Label.Text60 = "Interstate Case Assignments created";
          local.Textnum.Text15 =
            NumberToString(import.IntCaseAssgnmntCreates.Count, 15);

          break;
        case 17:
          local.Label.Text60 = "Incoming CSENet transaction errors";
          local.Textnum.Text15 = NumberToString(import.Errors.Count, 15);

          break;
        case 18:
          local.Label.Text60 = "Previously rejected outgoing transactions";
          local.Textnum.Text15 =
            NumberToString(import.PreviousRejects.Count, 15);

          break;
        default:
          goto AfterCycle;
      }

      local.EabReportSend.RptDetail = local.Label.Text60 + local.Textnum.Text15;
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error encountered writing to Control Report";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    // -----------------------------------------------------------------
    // CLOSE CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    // -----------------------------------------------------------------
    // CLOSE ERROR REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public Common Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of InterstateCaseCreates.
    /// </summary>
    [JsonPropertyName("interstateCaseCreates")]
    public Common InterstateCaseCreates
    {
      get => interstateCaseCreates ??= new();
      set => interstateCaseCreates = value;
    }

    /// <summary>
    /// A value of NonIvdReject.
    /// </summary>
    [JsonPropertyName("nonIvdReject")]
    public Common NonIvdReject
    {
      get => nonIvdReject ??= new();
      set => nonIvdReject = value;
    }

    /// <summary>
    /// A value of ApIdentCreates.
    /// </summary>
    [JsonPropertyName("apIdentCreates")]
    public Common ApIdentCreates
    {
      get => apIdentCreates ??= new();
      set => apIdentCreates = value;
    }

    /// <summary>
    /// A value of ApLocateCreates.
    /// </summary>
    [JsonPropertyName("apLocateCreates")]
    public Common ApLocateCreates
    {
      get => apLocateCreates ??= new();
      set => apLocateCreates = value;
    }

    /// <summary>
    /// A value of ParticipantCreates.
    /// </summary>
    [JsonPropertyName("participantCreates")]
    public Common ParticipantCreates
    {
      get => participantCreates ??= new();
      set => participantCreates = value;
    }

    /// <summary>
    /// A value of OrderCreates.
    /// </summary>
    [JsonPropertyName("orderCreates")]
    public Common OrderCreates
    {
      get => orderCreates ??= new();
      set => orderCreates = value;
    }

    /// <summary>
    /// A value of CollectionCreates.
    /// </summary>
    [JsonPropertyName("collectionCreates")]
    public Common CollectionCreates
    {
      get => collectionCreates ??= new();
      set => collectionCreates = value;
    }

    /// <summary>
    /// A value of MiscCreates.
    /// </summary>
    [JsonPropertyName("miscCreates")]
    public Common MiscCreates
    {
      get => miscCreates ??= new();
      set => miscCreates = value;
    }

    /// <summary>
    /// A value of InterstContactCreates.
    /// </summary>
    [JsonPropertyName("interstContactCreates")]
    public Common InterstContactCreates
    {
      get => interstContactCreates ??= new();
      set => interstContactCreates = value;
    }

    /// <summary>
    /// A value of InterstContactUpdates.
    /// </summary>
    [JsonPropertyName("interstContactUpdates")]
    public Common InterstContactUpdates
    {
      get => interstContactUpdates ??= new();
      set => interstContactUpdates = value;
    }

    /// <summary>
    /// A value of ContactAddrCreates.
    /// </summary>
    [JsonPropertyName("contactAddrCreates")]
    public Common ContactAddrCreates
    {
      get => contactAddrCreates ??= new();
      set => contactAddrCreates = value;
    }

    /// <summary>
    /// A value of ContactAddrUpdates.
    /// </summary>
    [JsonPropertyName("contactAddrUpdates")]
    public Common ContactAddrUpdates
    {
      get => contactAddrUpdates ??= new();
      set => contactAddrUpdates = value;
    }

    /// <summary>
    /// A value of PaymentAddrCreates.
    /// </summary>
    [JsonPropertyName("paymentAddrCreates")]
    public Common PaymentAddrCreates
    {
      get => paymentAddrCreates ??= new();
      set => paymentAddrCreates = value;
    }

    /// <summary>
    /// A value of PaymentAddrUpdates.
    /// </summary>
    [JsonPropertyName("paymentAddrUpdates")]
    public Common PaymentAddrUpdates
    {
      get => paymentAddrUpdates ??= new();
      set => paymentAddrUpdates = value;
    }

    /// <summary>
    /// A value of IntCaseAssgnmntCreates.
    /// </summary>
    [JsonPropertyName("intCaseAssgnmntCreates")]
    public Common IntCaseAssgnmntCreates
    {
      get => intCaseAssgnmntCreates ??= new();
      set => intCaseAssgnmntCreates = value;
    }

    /// <summary>
    /// A value of Errors.
    /// </summary>
    [JsonPropertyName("errors")]
    public Common Errors
    {
      get => errors ??= new();
      set => errors = value;
    }

    /// <summary>
    /// A value of PreviousRejects.
    /// </summary>
    [JsonPropertyName("previousRejects")]
    public Common PreviousRejects
    {
      get => previousRejects ??= new();
      set => previousRejects = value;
    }

    private Common read;
    private Common interstateCaseCreates;
    private Common nonIvdReject;
    private Common apIdentCreates;
    private Common apLocateCreates;
    private Common participantCreates;
    private Common orderCreates;
    private Common collectionCreates;
    private Common miscCreates;
    private Common interstContactCreates;
    private Common interstContactUpdates;
    private Common contactAddrCreates;
    private Common contactAddrUpdates;
    private Common paymentAddrCreates;
    private Common paymentAddrUpdates;
    private Common intCaseAssgnmntCreates;
    private Common errors;
    private Common previousRejects;
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
    /// A value of Textnum.
    /// </summary>
    [JsonPropertyName("textnum")]
    public WorkArea Textnum
    {
      get => textnum ??= new();
      set => textnum = value;
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

    /// <summary>
    /// A value of Label.
    /// </summary>
    [JsonPropertyName("label")]
    public WorkArea Label
    {
      get => label ??= new();
      set => label = value;
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

    /// <summary>
    /// A value of MaxControlTotal.
    /// </summary>
    [JsonPropertyName("maxControlTotal")]
    public Common MaxControlTotal
    {
      get => maxControlTotal ??= new();
      set => maxControlTotal = value;
    }

    private WorkArea textnum;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private WorkArea label;
    private Common subscript;
    private Common maxControlTotal;
  }
#endregion
}
