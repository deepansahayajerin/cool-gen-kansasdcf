// Program: SP_B714_WRITE_BATCH_DOCUMENTS, ID: 373372099, model: 746.
// Short name: SWEB714P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B714_WRITE_BATCH_DOCUMENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB714WriteBatchDocuments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B714_WRITE_BATCH_DOCUMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB714WriteBatchDocuments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB714WriteBatchDocuments.
  /// </summary>
  public SpB714WriteBatchDocuments(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 02/12/2002	K Cole & M Ramirez			Initial Development
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB714Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // --------------------------------------------------------
      // No message will be given in Error Report because program
      // failed before the Error Report was created.
      // -----------------------------------------------------------
      return;
    }

    local.Current.Date = local.ProgramProcessingInfo.ProcessDate;

    // -----------------------------------------------------------
    // Outgoing_document records with the print_successful_ind = 'Y'
    // are triggers for this PrAD.
    // Look for documents last updated by B709 with last updated timestamp 
    // greater than the last commit timestamp from B709.
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;

    foreach(var item in ReadOutgoingDocumentDocumentInfrastructure())
    {
      if (!IsEmpty(local.DebugOn.Flag))
      {
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Infrastructure.SystemGeneratedIdentifier, 15);
          
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = "READ outgoing_doc; Inf Id = " + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal + ", Doc Name = " + entities
          .Document.Name;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      if (local.Totals.Count > 0)
      {
        for(local.Totals.Index = 0; local.Totals.Index < local.Totals.Count; ++
          local.Totals.Index)
        {
          if (!local.Totals.CheckSize())
          {
            break;
          }

          if (Equal(entities.Document.Name, local.Totals.Item.GlocalTotals.Name) &&
            Equal
            (entities.Document.VersionNumber,
            local.Totals.Item.GlocalTotals.VersionNumber))
          {
            break;
          }
        }

        local.Totals.CheckIndex();

        if (!Equal(entities.Document.Name, local.Totals.Item.GlocalTotals.Name) ||
          !
          Equal(entities.Document.VersionNumber,
          local.Totals.Item.GlocalTotals.VersionNumber))
        {
          local.Totals.Index = local.Totals.Count;
          local.Totals.CheckSize();

          MoveDocument(entities.Document, local.Totals.Update.GlocalTotals);
        }
      }
      else
      {
        local.Totals.Index = 0;
        local.Totals.CheckSize();

        MoveDocument(entities.Document, local.Totals.Update.GlocalTotals);
      }

      ++local.DocsRead.Count;
      local.Totals.Update.GlocalTotalsRead.Count =
        local.Totals.Item.GlocalTotalsRead.Count + 1;

      // mjr
      // ---------------------------------------------------
      // 10/27/1999
      // Document exceptions will not be writen
      // ----------------------------------------------------------------
      local.ExceptionMailing.Flag = "";

      for(local.Exceptions.Index = 0; local.Exceptions.Index < local
        .Exceptions.Count; ++local.Exceptions.Index)
      {
        if (!local.Exceptions.CheckSize())
        {
          break;
        }

        if (Equal(local.Exceptions.Item.GlocalException.Name,
          entities.Document.Name))
        {
          local.ExceptionMailing.Flag = "Y";
          ++local.DocsException.Count;
          local.Totals.Update.GlocalTotalsException.Count =
            local.Totals.Item.GlocalTotalsException.Count + 1;

          break;
        }
      }

      local.Exceptions.CheckIndex();

      // mjr
      // -----------------------------------------------------------------
      // Document printing is successful.
      // 1)  Write document to output dataset;
      // 2)  Update outgoing_document, create monitored document (if
      //     necessary), and update infrastructure record.
      // --------------------------------------------------------------------
      UseSpB709WriteDocument();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Could not print document after a successful generation.  Code = " +
            local.EabFileHandling.Status;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.DocsProcessed.Count;
      local.Totals.Update.GlocalTotalsProcessed.Count =
        local.Totals.Item.GlocalTotalsProcessed.Count + 1;

      if (!IsEmpty(local.DebugOn.Flag))
      {
        local.EabReportSend.RptDetail = "     Successfully Printed";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      // mjr
      // -------------------------------------------------
      // End READ EACH outgoing_document
      // ----------------------------------------------------
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      local.EabConvertNumeric.SendAmount =
        NumberToString(entities.Infrastructure.SystemGeneratedIdentifier, 15);
      UseEabConvertNumeric1();
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; Inf ID = " +
        local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else if (Equal(local.TimeLimit.IefTimestamp, local.Max.IefTimestamp))
    {
      // Only update checkpoint restart if this is a current run - not 
      // recreating past documents
      if (AsChar(local.DebugOn.Flag) != 'P')
      {
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // checkpoint restart timestamp not updated
          local.EabReportSend.RptDetail =
            "ABEND: Unable to update program checkpoint restart after successful run.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
    }

    // ---------------------------------------------------------------
    // WRITE CONTROL TOTALS AND CLOSE REPORTS
    // ---------------------------------------------------------------
    UseSpB714WriteControlsAndClose();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.VersionNumber = source.VersionNumber;
  }

  private static void MoveEabConvertNumeric1(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
  }

  private static void MoveEabConvertNumeric3(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveExceptions(SpB714Housekeeping.Export.
    ExceptionsGroup source, Local.ExceptionsGroup target)
  {
    target.GlocalException.Name = source.G.Name;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTotals(Local.TotalsGroup source,
    SpB714WriteControlsAndClose.Import.DocumentTotalsGroup target)
  {
    MoveDocument(source.GlocalTotals, target.G);
    target.GimportRead.Count = source.GlocalTotalsRead.Count;
    target.GimportProcessed.Count = source.GlocalTotalsProcessed.Count;
    target.GimportFuture.Count = source.GlocalTotalsFuture.Count;
    target.GimportException.Count = source.GlocalTotalsException.Count;
    target.GimportDataError.Count = source.GlocalTotalsDataError.Count;
    target.GimportSystemError.Count = source.GlocalTotalsSystemError.Count;
    target.GimportWarning.Count = source.GlocalTotalsWarning.Count;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric3(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    MoveEabConvertNumeric1(local.EabConvertNumeric, useExport.EabConvertNumeric);
      

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric1(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseSpB709WriteDocument()
  {
    var useImport = new SpB709WriteDocument.Import();
    var useExport = new SpB709WriteDocument.Export();

    useImport.Document.Assign(entities.Document);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;
    useImport.Exception.Flag = local.ExceptionMailing.Flag;

    Call(SpB709WriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpB714Housekeeping()
  {
    var useImport = new SpB714Housekeeping.Import();
    var useExport = new SpB714Housekeeping.Export();

    Call(SpB714Housekeeping.Execute, useImport, useExport);

    local.Max.IefTimestamp = useExport.Max.IefTimestamp;
    local.TimeLimit.IefTimestamp = useExport.TimeLimit.IefTimestamp;
    local.Parm.Assign(useExport.Parm);
    useExport.Exceptions.CopyTo(local.Exceptions, MoveExceptions);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.Current.Timestamp = useExport.Current.Timestamp;
  }

  private void UseSpB714WriteControlsAndClose()
  {
    var useImport = new SpB714WriteControlsAndClose.Import();
    var useExport = new SpB714WriteControlsAndClose.Export();

    local.Totals.CopyTo(useImport.DocumentTotals, MoveTotals);
    useImport.DocsRead.Count = local.DocsRead.Count;
    useImport.DocsException.Count = local.DocsException.Count;
    useImport.DocsUnprocessedFuture.Count = local.DocsFuture.Count;
    useImport.DocsWarning.Count = local.DocsWarning.Count;
    useImport.DocsDataError.Count = local.DocsDataError.Count;
    useImport.DocsSystemError.Count = local.DocsSystemError.Count;
    useImport.DocsProcessed.Count = local.DocsProcessed.Count;

    Call(SpB714WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadOutgoingDocumentDocumentInfrastructure()
  {
    entities.Document.Populated = false;
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentDocumentInfrastructure",
      (db, command) =>
      {
        db.SetString(
          command, "prntSucessfulInd", local.Parm.PrintSucessfulIndicator);
        db.SetNullableString(
          command, "lastUpdatedBy", local.Parm.LastUpdatedBy ?? "");
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp1",
          local.Parm.LastUpdatdTstamp.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp2",
          local.TimeLimit.IefTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 1);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 3);
        entities.Document.Name = db.GetString(reader, 3);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 4);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Document.VersionNumber = db.GetString(reader, 7);
        entities.Document.Populated = true;
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

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
    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of GlocalTotals.
      /// </summary>
      [JsonPropertyName("glocalTotals")]
      public Document GlocalTotals
      {
        get => glocalTotals ??= new();
        set => glocalTotals = value;
      }

      /// <summary>
      /// A value of GlocalTotalsRead.
      /// </summary>
      [JsonPropertyName("glocalTotalsRead")]
      public Common GlocalTotalsRead
      {
        get => glocalTotalsRead ??= new();
        set => glocalTotalsRead = value;
      }

      /// <summary>
      /// A value of GlocalTotalsProcessed.
      /// </summary>
      [JsonPropertyName("glocalTotalsProcessed")]
      public Common GlocalTotalsProcessed
      {
        get => glocalTotalsProcessed ??= new();
        set => glocalTotalsProcessed = value;
      }

      /// <summary>
      /// A value of GlocalTotalsFuture.
      /// </summary>
      [JsonPropertyName("glocalTotalsFuture")]
      public Common GlocalTotalsFuture
      {
        get => glocalTotalsFuture ??= new();
        set => glocalTotalsFuture = value;
      }

      /// <summary>
      /// A value of GlocalTotalsException.
      /// </summary>
      [JsonPropertyName("glocalTotalsException")]
      public Common GlocalTotalsException
      {
        get => glocalTotalsException ??= new();
        set => glocalTotalsException = value;
      }

      /// <summary>
      /// A value of GlocalTotalsDataError.
      /// </summary>
      [JsonPropertyName("glocalTotalsDataError")]
      public Common GlocalTotalsDataError
      {
        get => glocalTotalsDataError ??= new();
        set => glocalTotalsDataError = value;
      }

      /// <summary>
      /// A value of GlocalTotalsSystemError.
      /// </summary>
      [JsonPropertyName("glocalTotalsSystemError")]
      public Common GlocalTotalsSystemError
      {
        get => glocalTotalsSystemError ??= new();
        set => glocalTotalsSystemError = value;
      }

      /// <summary>
      /// A value of GlocalTotalsWarning.
      /// </summary>
      [JsonPropertyName("glocalTotalsWarning")]
      public Common GlocalTotalsWarning
      {
        get => glocalTotalsWarning ??= new();
        set => glocalTotalsWarning = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document glocalTotals;
      private Common glocalTotalsRead;
      private Common glocalTotalsProcessed;
      private Common glocalTotalsFuture;
      private Common glocalTotalsException;
      private Common glocalTotalsDataError;
      private Common glocalTotalsSystemError;
      private Common glocalTotalsWarning;
    }

    /// <summary>A ExceptionsGroup group.</summary>
    [Serializable]
    public class ExceptionsGroup
    {
      /// <summary>
      /// A value of GlocalException.
      /// </summary>
      [JsonPropertyName("glocalException")]
      public Document GlocalException
      {
        get => glocalException ??= new();
        set => glocalException = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document glocalException;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public BatchTimestampWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of TimeLimit.
    /// </summary>
    [JsonPropertyName("timeLimit")]
    public BatchTimestampWorkArea TimeLimit
    {
      get => timeLimit ??= new();
      set => timeLimit = value;
    }

    /// <summary>
    /// A value of Parm.
    /// </summary>
    [JsonPropertyName("parm")]
    public OutgoingDocument Parm
    {
      get => parm ??= new();
      set => parm = value;
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonIgnore]
    public Array<TotalsGroup> Totals => totals ??= new(TotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Totals for json serialization.
    /// </summary>
    [JsonPropertyName("totals")]
    [Computed]
    public IList<TotalsGroup> Totals_Json
    {
      get => totals;
      set => Totals.Assign(value);
    }

    /// <summary>
    /// A value of ExceptionMailing.
    /// </summary>
    [JsonPropertyName("exceptionMailing")]
    public Common ExceptionMailing
    {
      get => exceptionMailing ??= new();
      set => exceptionMailing = value;
    }

    /// <summary>
    /// Gets a value of Exceptions.
    /// </summary>
    [JsonIgnore]
    public Array<ExceptionsGroup> Exceptions => exceptions ??= new(
      ExceptionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Exceptions for json serialization.
    /// </summary>
    [JsonPropertyName("exceptions")]
    [Computed]
    public IList<ExceptionsGroup> Exceptions_Json
    {
      get => exceptions;
      set => Exceptions.Assign(value);
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of ZdelLocalMonitored.
    /// </summary>
    [JsonPropertyName("zdelLocalMonitored")]
    public OfficeServiceProviderAlert ZdelLocalMonitored
    {
      get => zdelLocalMonitored ??= new();
      set => zdelLocalMonitored = value;
    }

    /// <summary>
    /// A value of UnMonitored.
    /// </summary>
    [JsonPropertyName("unMonitored")]
    public OfficeServiceProviderAlert UnMonitored
    {
      get => unMonitored ??= new();
      set => unMonitored = value;
    }

    /// <summary>
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public OfficeServiceProviderAlert Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

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
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of UserinputField.
    /// </summary>
    [JsonPropertyName("userinputField")]
    public Common UserinputField
    {
      get => userinputField ??= new();
      set => userinputField = value;
    }

    /// <summary>
    /// A value of RequiredFieldMissing.
    /// </summary>
    [JsonPropertyName("requiredFieldMissing")]
    public Common RequiredFieldMissing
    {
      get => requiredFieldMissing ??= new();
      set => requiredFieldMissing = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public SpDocLiteral Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Infrastructure Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RowLockFieldValue.
    /// </summary>
    [JsonPropertyName("rowLockFieldValue")]
    public Common RowLockFieldValue
    {
      get => rowLockFieldValue ??= new();
      set => rowLockFieldValue = value;
    }

    /// <summary>
    /// A value of RowLockDocument.
    /// </summary>
    [JsonPropertyName("rowLockDocument")]
    public Common RowLockDocument
    {
      get => rowLockDocument ??= new();
      set => rowLockDocument = value;
    }

    /// <summary>
    /// A value of DocsRead.
    /// </summary>
    [JsonPropertyName("docsRead")]
    public Common DocsRead
    {
      get => docsRead ??= new();
      set => docsRead = value;
    }

    /// <summary>
    /// A value of DocsException.
    /// </summary>
    [JsonPropertyName("docsException")]
    public Common DocsException
    {
      get => docsException ??= new();
      set => docsException = value;
    }

    /// <summary>
    /// A value of DocsFuture.
    /// </summary>
    [JsonPropertyName("docsFuture")]
    public Common DocsFuture
    {
      get => docsFuture ??= new();
      set => docsFuture = value;
    }

    /// <summary>
    /// A value of DocsWarning.
    /// </summary>
    [JsonPropertyName("docsWarning")]
    public Common DocsWarning
    {
      get => docsWarning ??= new();
      set => docsWarning = value;
    }

    /// <summary>
    /// A value of DocsDataError.
    /// </summary>
    [JsonPropertyName("docsDataError")]
    public Common DocsDataError
    {
      get => docsDataError ??= new();
      set => docsDataError = value;
    }

    /// <summary>
    /// A value of DocsSystemError.
    /// </summary>
    [JsonPropertyName("docsSystemError")]
    public Common DocsSystemError
    {
      get => docsSystemError ??= new();
      set => docsSystemError = value;
    }

    /// <summary>
    /// A value of DocsProcessed.
    /// </summary>
    [JsonPropertyName("docsProcessed")]
    public Common DocsProcessed
    {
      get => docsProcessed ??= new();
      set => docsProcessed = value;
    }

    private BatchTimestampWorkArea max;
    private BatchTimestampWorkArea timeLimit;
    private OutgoingDocument parm;
    private Array<TotalsGroup> totals;
    private Common exceptionMailing;
    private Array<ExceptionsGroup> exceptions;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Field field;
    private OutDocRtrnAddr outDocRtrnAddr;
    private SpDocKey spDocKey;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private OfficeServiceProviderAlert zdelLocalMonitored;
    private OfficeServiceProviderAlert unMonitored;
    private OfficeServiceProviderAlert automatic;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private DateWorkArea dateWorkArea;
    private FieldValue fieldValue;
    private DocumentField errorDocumentField;
    private ProgramProcessingInfo programProcessingInfo;
    private FieldValue errorFieldValue;
    private Common errorInd;
    private OutgoingDocument outgoingDocument;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common debugOn;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea current;
    private WorkArea workArea;
    private Common userinputField;
    private Common requiredFieldMissing;
    private SpDocLiteral zdel;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Infrastructure infrastructure;
    private Infrastructure null1;
    private External external;
    private Common rowLockFieldValue;
    private Common rowLockDocument;
    private Common docsRead;
    private Common docsException;
    private Common docsFuture;
    private Common docsWarning;
    private Common docsDataError;
    private Common docsSystemError;
    private Common docsProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CsePerson csePerson;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private EventDetail eventDetail;
    private Document document;
    private DocumentField documentField;
    private Field field;
    private FieldValue fieldValue;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
  }
#endregion
}
