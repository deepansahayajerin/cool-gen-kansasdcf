// Program: FN_BF13_COLL_AR_CONVERSION, ID: 373375434, model: 746.
// Short name: SWEFF13B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF13_COLL_AR_CONVERSION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBf13CollArConversion: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF13_COLL_AR_CONVERSION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf13CollArConversion(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf13CollArConversion.
  /// </summary>
  public FnBf13CollArConversion(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2001-06-04  WR 010504  Fangman - New pgm to add the AR person number to 
    // the Collection.
    // ***************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.AbendCheckLoop.Flag = "Y";

    if (AsChar(local.AbendCheckLoop.Flag) == 'Y')
    {
      UseFnBf13Housekeeping();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test;
      }

      foreach(var item in ReadCollectionCsePerson())
      {
        ++local.ProcessCountToCommit.Count;
        ++local.CollectionsProcessed.Count;

        if (AsChar(local.Test.TestRunInd.Flag) == 'N')
        {
          try
          {
            UpdateCollection();

            // Continue
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU";

                goto Test;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV";

                goto Test;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (local.ProcessCountToCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProcessCountToCommit.Count = 0;

          if (AsChar(local.Test.TestRunInd.Flag) == 'N')
          {
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              return;
            }
          }
        }

        if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "AR " + entities.Obligee1.Number + "  Coll ID " +
            NumberToString
            (entities.Collection.SystemGeneratedIdentifier, 7, 9);
          UseCabControlReport();
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "AR " + entities.Obligee1.Number + "  Oblig ID " +
        NumberToString(entities.Collection.SystemGeneratedIdentifier, 7, 9) + "  ERROR:  " +
        local.ExitStateMessage.Message;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "Collections read & updated " + NumberToString
      (local.CollectionsProcessed.Count, 15);
    UseCabControlReport();
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    UseCabErrorReport2();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.Test.TestRunInd.Flag) == 'Y')
      {
        ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
      }
      else
      {
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
        }
        else
        {
          ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
        }
      }
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTest(FnBf13Housekeeping.Export.TestGroup source,
    Local.TestGroup target)
  {
    target.TestRunInd.Flag = source.TestRunInd.Flag;
    target.TestDisplayInd.Flag = source.TestDisplayInd.Flag;
    target.TestFirstObligee.Number = source.TestFirstObligee.Number;
    target.TestLastObligee.Number = source.TestLastObligee.Number;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateMessage.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateMessage.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnBf13Housekeeping()
  {
    var useImport = new FnBf13Housekeeping.Import();
    var useExport = new FnBf13Housekeeping.Export();

    Call(FnBf13Housekeeping.Execute, useImport, useExport);

    MoveTest(useExport.Test, local.Test);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private IEnumerable<bool> ReadCollectionCsePerson()
  {
    entities.Collection.Populated = false;
    entities.Obligee1.Populated = false;

    return ReadEach("ReadCollectionCsePerson",
      (db, command) =>
      {
        db.SetString(command, "number1", local.Test.TestFirstObligee.Number);
        db.SetString(command, "number2", local.Test.TestLastObligee.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.ArNumber = db.GetNullableString(reader, 11);
        entities.Obligee1.Number = db.GetString(reader, 12);
        entities.Collection.Populated = true;
        entities.Obligee1.Populated = true;

        return true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var arNumber = entities.Obligee1.Number;

    entities.Collection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "arNumber", arNumber);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.ArNumber = arNumber;
    entities.Collection.Populated = true;
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
    /// <summary>A TestGroup group.</summary>
    [Serializable]
    public class TestGroup
    {
      /// <summary>
      /// A value of TestRunInd.
      /// </summary>
      [JsonPropertyName("testRunInd")]
      public Common TestRunInd
      {
        get => testRunInd ??= new();
        set => testRunInd = value;
      }

      /// <summary>
      /// A value of TestDisplayInd.
      /// </summary>
      [JsonPropertyName("testDisplayInd")]
      public Common TestDisplayInd
      {
        get => testDisplayInd ??= new();
        set => testDisplayInd = value;
      }

      /// <summary>
      /// A value of TestFirstObligee.
      /// </summary>
      [JsonPropertyName("testFirstObligee")]
      public CsePerson TestFirstObligee
      {
        get => testFirstObligee ??= new();
        set => testFirstObligee = value;
      }

      /// <summary>
      /// A value of TestLastObligee.
      /// </summary>
      [JsonPropertyName("testLastObligee")]
      public CsePerson TestLastObligee
      {
        get => testLastObligee ??= new();
        set => testLastObligee = value;
      }

      private Common testRunInd;
      private Common testDisplayInd;
      private CsePerson testFirstObligee;
      private CsePerson testLastObligee;
    }

    /// <summary>
    /// A value of CollectionsProcessed.
    /// </summary>
    [JsonPropertyName("collectionsProcessed")]
    public Common CollectionsProcessed
    {
      get => collectionsProcessed ??= new();
      set => collectionsProcessed = value;
    }

    /// <summary>
    /// Gets a value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public TestGroup Test
    {
      get => test ?? (test = new());
      set => test = value;
    }

    /// <summary>
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
    }

    /// <summary>
    /// A value of PrintMsg.
    /// </summary>
    [JsonPropertyName("printMsg")]
    public WorkArea PrintMsg
    {
      get => printMsg ??= new();
      set => printMsg = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of AbendCheckLoop.
    /// </summary>
    [JsonPropertyName("abendCheckLoop")]
    public Common AbendCheckLoop
    {
      get => abendCheckLoop ??= new();
      set => abendCheckLoop = value;
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
    /// A value of ExitStateMessage.
    /// </summary>
    [JsonPropertyName("exitStateMessage")]
    public ExitStateWorkArea ExitStateMessage
    {
      get => exitStateMessage ??= new();
      set => exitStateMessage = value;
    }

    private Common collectionsProcessed;
    private TestGroup test;
    private Common processCountToCommit;
    private WorkArea printMsg;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common abendCheckLoop;
    private Common common;
    private ExitStateWorkArea exitStateMessage;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    private Collection collection;
    private DisbursementTransaction disbCollection;
    private CsePerson obligee1;
    private CsePersonAccount obligee2;
  }
#endregion
}
