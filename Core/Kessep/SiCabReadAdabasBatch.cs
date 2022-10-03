// Program: SI_CAB_READ_ADABAS_BATCH, ID: 372745443, model: 746.
// Short name: SWE02573
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_READ_ADABAS_BATCH.
/// </summary>
[Serializable]
public partial class SiCabReadAdabasBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_READ_ADABAS_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabReadAdabasBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabReadAdabasBatch.
  /// </summary>
  public SiCabReadAdabasBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.EabFileHandling.Action = "WRITE";
    UseEabReadCsePersonBatch();

    // ************************************************
    // *Interpret the error codes returned from ADABAS*
    // *and set an appropriate exit state.            *
    // ************************************************
    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // ************************************************
        // *Successful Adabas Read Occurred.             *
        // ************************************************
        switch(TrimEnd(export.Obligor.Ssn))
        {
          case "000000000":
            ExitState = "ADABAS_INVALID_SSN_W_RB";
            local.EabReportSend.RptDetail =
              "Social Security Number is zero for person number = " + import
              .Obligor.Number;

            break;
          case "":
            ExitState = "ADABAS_INVALID_SSN_W_RB";
            local.EabReportSend.RptDetail =
              "Social Security Number is spaces for person number = " + import
              .Obligor.Number;

            break;
          default:
            break;
        }

        break;
      case 'A':
        // ************************************************
        // *Unsuccessful ADABAS Read Occurred.           *
        // ************************************************
        switch(TrimEnd(export.AbendData.AdabasResponseCd))
        {
          case "0113":
            ExitState = "ACO_ADABAS_PERSON_NF_113";
            local.EabReportSend.RptDetail =
              "Person not found in Adabas error 113 -- number = " + import
              .Obligor.Number;

            break;
          case "0148":
            ExitState = "ACO_ADABAS_UNAVAILABLE";
            local.EabReportSend.RptDetail =
              "Adabas unavailable fetching person number = " + import
              .Obligor.Number;

            break;
          default:
            ExitState = "ADABAS_READ_UNSUCCESSFUL";
            local.EabReportSend.RptDetail =
              "Adabas read unsuccessful fetching person number = " + import
              .Obligor.Number;

            break;
        }

        break;
      case 'C':
        // ************************************************
        // *CICS action Failed. A reason code should be   *
        // *interpreted.
        // 
        // *
        // ************************************************
        if (IsEmpty(export.AbendData.CicsResponseCd))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
        }

        local.EabReportSend.RptDetail =
          "CICS error fetching person number = " + import.Obligor.Number;

        break;
      default:
        ExitState = "ADABAS_INVALID_RETURN_CODE";
        local.EabReportSend.RptDetail =
          "Unknown error fetching person number = " + import.Obligor.Number;

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseCabErrorReport();
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = import.Obligor.Number;
    useExport.Ae.Flag = local.Ae.Flag;
    useExport.Cse.Flag = local.Cse.Flag;
    useExport.Kanpay.Flag = local.Kanpay.Flag;
    useExport.Kscares.Flag = local.Kscares.Flag;
    MoveCsePersonsWorkSet(export.Obligor, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(export.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.Ae.Flag = useExport.Ae.Flag;
    local.Cse.Flag = useExport.Cse.Flag;
    local.Kanpay.Flag = useExport.Kanpay.Flag;
    local.Kscares.Flag = useExport.Kscares.Flag;
    export.Obligor.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePersonsWorkSet obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private CsePersonsWorkSet obligor;
    private AbendData abendData;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of Kanpay.
    /// </summary>
    [JsonPropertyName("kanpay")]
    public Common Kanpay
    {
      get => kanpay ??= new();
      set => kanpay = value;
    }

    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
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

    private Common ae;
    private Common cse;
    private Common kanpay;
    private Common kscares;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
