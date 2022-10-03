// Program: OE_B494_MEDICAL_SUPPORT_ORDERS, ID: 372977097, model: 746.
// Short name: SWEE494B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B494_MEDICAL_SUPPORT_ORDERS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB494MedicalSupportOrders: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B494_MEDICAL_SUPPORT_ORDERS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB494MedicalSupportOrders(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB494MedicalSupportOrders.
  /// </summary>
  public OeB494MedicalSupportOrders(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.ReportNeeded.Flag = "Y";
    UseOeB494Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ReportNeeded.Flag) == 'Y')
    {
      // **************************************************************************
      // * The following logic identifies stand alone medical support orders.
      // **************************************************************************
      foreach(var item in ReadLegalAction())
      {
        UseOeB494CreateFileAndReport();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test;
        }

        if (Lt(local.Null1.Date, entities.LegalAction.FiledDate))
        {
        }
      }

      // **************************************************************************
      // * The following logic identifies medical support orders that are
      // * combined with other types of orders.
      // **************************************************************************
      foreach(var item in ReadLegalActionLegalActionDetail())
      {
        UseOeB494CreateFileAndReport();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test;
        }
      }
    }

Test:

    // **********************************************************
    // CLOSE ADABAS IS AVAILABLE
    // **********************************************************
    local.Ar.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB494Closing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeB494Closing();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ar.Number;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseOeB494Closing()
  {
    var useImport = new OeB494Closing.Import();
    var useExport = new OeB494Closing.Export();

    useImport.RecordCount.Count = local.Record.Count;

    Call(OeB494Closing.Execute, useImport, useExport);
  }

  private void UseOeB494CreateFileAndReport()
  {
    var useImport = new OeB494CreateFileAndReport.Import();
    var useExport = new OeB494CreateFileAndReport.Export();

    useImport.Pers.Assign(entities.LegalAction);
    useImport.RecordCount.Count = local.Record.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.Process.Date = local.Process.Date;

    Call(OeB494CreateFileAndReport.Execute, useImport, useExport);

    local.Record.Count = useExport.RecordCount.Count;
  }

  private void UseOeB494Housekeeping()
  {
    var useImport = new OeB494Housekeeping.Import();
    var useExport = new OeB494Housekeeping.Export();

    Call(OeB494Housekeeping.Execute, useImport, useExport);

    local.ReportingMonthYear.Text30 = useExport.ReportingMonthYear.Text30;
    local.PeriodEndTextWorkArea.Text10 = useExport.PeriodEnd.Text10;
    local.PeriodBeginTextWorkArea.Text10 = useExport.PeriodBegin.Text10;
    local.Max.Date = useExport.Max.Date;
    MoveDateWorkArea(useExport.EndPeriod, local.PeriodEndDateWorkArea);
    MoveDateWorkArea(useExport.BeginPeriod, local.PeriodBeginDateWorkArea);
    local.Process.Date = useExport.Process.Date;
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          local.PeriodBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.PeriodEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetail()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          local.PeriodBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.PeriodEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 8);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 10);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 14);
        entities.LegalActionDetail.Description = db.GetString(reader, 15);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;

        return true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CoverageStart.
    /// </summary>
    [JsonPropertyName("coverageStart")]
    public TextWorkArea CoverageStart
    {
      get => coverageStart ??= new();
      set => coverageStart = value;
    }

    /// <summary>
    /// A value of CoverageEnd.
    /// </summary>
    [JsonPropertyName("coverageEnd")]
    public TextWorkArea CoverageEnd
    {
      get => coverageEnd ??= new();
      set => coverageEnd = value;
    }

    /// <summary>
    /// A value of ReportingMonthYear.
    /// </summary>
    [JsonPropertyName("reportingMonthYear")]
    public TextWorkArea ReportingMonthYear
    {
      get => reportingMonthYear ??= new();
      set => reportingMonthYear = value;
    }

    /// <summary>
    /// A value of PeriodEndTextWorkArea.
    /// </summary>
    [JsonPropertyName("periodEndTextWorkArea")]
    public TextWorkArea PeriodEndTextWorkArea
    {
      get => periodEndTextWorkArea ??= new();
      set => periodEndTextWorkArea = value;
    }

    /// <summary>
    /// A value of PeriodBeginTextWorkArea.
    /// </summary>
    [JsonPropertyName("periodBeginTextWorkArea")]
    public TextWorkArea PeriodBeginTextWorkArea
    {
      get => periodBeginTextWorkArea ??= new();
      set => periodBeginTextWorkArea = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of ProgramFound.
    /// </summary>
    [JsonPropertyName("programFound")]
    public Common ProgramFound
    {
      get => programFound ??= new();
      set => programFound = value;
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
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PeriodEndDateWorkArea.
    /// </summary>
    [JsonPropertyName("periodEndDateWorkArea")]
    public DateWorkArea PeriodEndDateWorkArea
    {
      get => periodEndDateWorkArea ??= new();
      set => periodEndDateWorkArea = value;
    }

    /// <summary>
    /// A value of PeriodBeginDateWorkArea.
    /// </summary>
    [JsonPropertyName("periodBeginDateWorkArea")]
    public DateWorkArea PeriodBeginDateWorkArea
    {
      get => periodBeginDateWorkArea ??= new();
      set => periodBeginDateWorkArea = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private TextWorkArea coverageStart;
    private TextWorkArea coverageEnd;
    private TextWorkArea reportingMonthYear;
    private TextWorkArea periodEndTextWorkArea;
    private TextWorkArea periodBeginTextWorkArea;
    private Common reportNeeded;
    private EabReportSend neededToWrite;
    private Common programFound;
    private AbendData abendData;
    private Common record;
    private DateWorkArea max;
    private DateWorkArea current;
    private DateWorkArea periodEndDateWorkArea;
    private DateWorkArea periodBeginDateWorkArea;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
