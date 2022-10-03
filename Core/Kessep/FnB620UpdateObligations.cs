// Program: FN_B620_UPDATE_OBLIGATIONS, ID: 370976660, model: 746.
// Short name: SWE00850
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B620_UPDATE_OBLIGATIONS.
/// </para>
/// <para>
/// This cab updates the delinquent indicator on obligations for a given court 
/// order.  It also increments various program counts, and writes an event when
/// a new delinquency for a court order is found.
/// </para>
/// </summary>
[Serializable]
public partial class FnB620UpdateObligations: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B620_UPDATE_OBLIGATIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB620UpdateObligations(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB620UpdateObligations.
  /// </summary>
  public FnB620UpdateObligations(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // This cab updates the delinquent indicator on obligations for a given 
    // court order.  It also increments various program counts, and writes an
    // event when a new delinquency for a court order is found.
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    export.EventsRaised.Count = import.EventsRaised.Count;
    export.ObsStillN.Count = import.ObsStillN.Count;
    export.ObsStillY.Count = import.ObsStillY.Count;
    export.ObsUpdatedNToY.Count = import.ObsUpdatedNToY.Count;
    export.ObsUpdatedYToN.Count = import.ObsUpdatedYToN.Count;
    export.ProcessCountToCommit.Count = import.ProcessCountToCommit.Count;
    local.HardcodeCs.SystemGeneratedIdentifier = 1;
    local.HardcodeMs.SystemGeneratedIdentifier = 3;
    local.HardcodeMc.SystemGeneratedIdentifier = 19;
    local.HardcodeSp.SystemGeneratedIdentifier = 2;
    local.HardcodeWc.SystemGeneratedIdentifier = 21;

    // : The write event flag will be set to 'N' if any obligation is already 
    // set
    //  to delinquent.  We only want to raise an event if all obligations on
    //  the court order are  currently set to 'N' and are being updated to 'Y'.
    if (AsChar(import.Obligation.DelinquentInd) == 'N')
    {
      local.WriteEvent.Flag = "N";
    }
    else
    {
      local.WriteEvent.Flag = "Y";
    }

    // : Update the delinquency flag on all the accruing obligations
    //   on the court order, if it has changed.
    foreach(var item in ReadObligationCsePerson())
    {
      if (local.Obligor.IsEmpty)
      {
        local.Obligor.Index = 0;
        local.Obligor.CheckSize();

        local.Obligor.Update.Obligor1.Number = entities.Obligor2.Number;
      }
      else
      {
        for(local.Obligor.Index = 0; local.Obligor.Index < local.Obligor.Count; ++
          local.Obligor.Index)
        {
          if (!local.Obligor.CheckSize())
          {
            break;
          }

          if (Equal(entities.Obligor2.Number, local.Obligor.Item.Obligor1.Number))
            
          {
            goto Test;
          }
        }

        local.Obligor.CheckIndex();

        local.Obligor.Index = local.Obligor.Count;
        local.Obligor.CheckSize();

        local.Obligor.Update.Obligor1.Number = entities.Obligor2.Number;
      }

Test:

      // : If ANY obligation on the court order has a delinquent indicator of '
      // Y',
      //  then a HIST record already exists for the court order, so we should 
      // not
      //  write another event.
      if (AsChar(entities.Obligation.DelinquentInd) == 'Y')
      {
        local.WriteEvent.Flag = "N";
      }

      if (AsChar(entities.Obligation.DelinquentInd) != AsChar
        (import.Obligation.DelinquentInd))
      {
        try
        {
          UpdateObligation();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGATION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGATION_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = local.ExitState.Message;
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + "  LA: " + (
              import.LegalAction.StandardNumber ?? "") + "  OB ID:  " + TrimEnd
            (NumberToString(entities.Obligation.SystemGeneratedIdentifier, 15));
            
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // : Accumulate counts
        ++export.ProcessCountToCommit.Count;

        if (AsChar(entities.Obligation.DelinquentInd) == 'N')
        {
          ++export.ObsUpdatedYToN.Count;
        }
        else
        {
          ++export.ObsUpdatedNToY.Count;
        }
      }
      else
      {
        // : No changes in delinquency - the indicator either stayed 'y' or it 
        // stayed 'n'.
        if (AsChar(entities.Obligation.DelinquentInd) == 'Y')
        {
          ++export.ObsStillY.Count;
        }
        else
        {
          ++export.ObsStillN.Count;
        }
      }
    }

    if (AsChar(local.WriteEvent.Flag) == 'Y')
    {
      local.Obligor.Index = 0;

      for(var limit = local.Obligor.Count; local.Obligor.Index < limit; ++
        local.Obligor.Index)
      {
        if (!local.Obligor.CheckSize())
        {
          break;
        }

        // : Raise an event for HIST.
        local.Infrastructure.EventId = 9;
        local.Infrastructure.UserId = global.UserId;
        local.Infrastructure.CsePersonNumber =
          local.Obligor.Item.Obligor1.Number;
        local.Infrastructure.BusinessObjectCd = "ENF";
        local.Infrastructure.ReferenceDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.Infrastructure.DenormText12 =
          TrimEnd(import.LegalAction.StandardNumber);
        local.Infrastructure.ReasonCode = "FNDELINQUENT";
        local.Infrastructure.Detail = "DATE OF DELINQUENCY: " + import
          .DateWorkArea.TextDate + "  Court Order: " + (
            import.LegalAction.StandardNumber ?? "");
        local.Infrastructure.ProcessStatus = "Q";
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.NeededToWrite.RptDetail = TrimEnd(local.ExitState.Message) + "  for L.A. std #: " +
            (import.LegalAction.StandardNumber ?? "");
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++export.EventsRaised.Count;
      }

      local.Obligor.CheckIndex();
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitState.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitState.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadObligationCsePerson()
  {
    entities.Obligation.Populated = false;
    entities.Obligor2.Populated = false;

    return ReadEach("ReadObligationCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeCs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeSp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodeWc.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligor2.Number = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.Obligation.DelinquentInd = db.GetNullableString(reader, 8);
        entities.Obligation.Populated = true;
        entities.Obligor2.Populated = true;

        return true;
      });
  }

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.DateWorkArea.Timestamp;
    var delinquentInd = import.Obligation.DelinquentInd ?? "";

    entities.Obligation.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableString(command, "delinquentInd", delinquentInd);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.DelinquentInd = delinquentInd;
    entities.Obligation.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ObsUpdatedYToN.
    /// </summary>
    [JsonPropertyName("obsUpdatedYToN")]
    public Common ObsUpdatedYToN
    {
      get => obsUpdatedYToN ??= new();
      set => obsUpdatedYToN = value;
    }

    /// <summary>
    /// A value of ObsUpdatedNToY.
    /// </summary>
    [JsonPropertyName("obsUpdatedNToY")]
    public Common ObsUpdatedNToY
    {
      get => obsUpdatedNToY ??= new();
      set => obsUpdatedNToY = value;
    }

    /// <summary>
    /// A value of ObsStillY.
    /// </summary>
    [JsonPropertyName("obsStillY")]
    public Common ObsStillY
    {
      get => obsStillY ??= new();
      set => obsStillY = value;
    }

    /// <summary>
    /// A value of ObsStillN.
    /// </summary>
    [JsonPropertyName("obsStillN")]
    public Common ObsStillN
    {
      get => obsStillN ??= new();
      set => obsStillN = value;
    }

    /// <summary>
    /// A value of EventsRaised.
    /// </summary>
    [JsonPropertyName("eventsRaised")]
    public Common EventsRaised
    {
      get => eventsRaised ??= new();
      set => eventsRaised = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private LegalAction legalAction;
    private Obligation obligation;
    private Common processCountToCommit;
    private Common obsUpdatedYToN;
    private Common obsUpdatedNToY;
    private Common obsStillY;
    private Common obsStillN;
    private Common eventsRaised;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ObsUpdatedYToN.
    /// </summary>
    [JsonPropertyName("obsUpdatedYToN")]
    public Common ObsUpdatedYToN
    {
      get => obsUpdatedYToN ??= new();
      set => obsUpdatedYToN = value;
    }

    /// <summary>
    /// A value of ObsUpdatedNToY.
    /// </summary>
    [JsonPropertyName("obsUpdatedNToY")]
    public Common ObsUpdatedNToY
    {
      get => obsUpdatedNToY ??= new();
      set => obsUpdatedNToY = value;
    }

    /// <summary>
    /// A value of ObsStillY.
    /// </summary>
    [JsonPropertyName("obsStillY")]
    public Common ObsStillY
    {
      get => obsStillY ??= new();
      set => obsStillY = value;
    }

    /// <summary>
    /// A value of ObsStillN.
    /// </summary>
    [JsonPropertyName("obsStillN")]
    public Common ObsStillN
    {
      get => obsStillN ??= new();
      set => obsStillN = value;
    }

    /// <summary>
    /// A value of EventsRaised.
    /// </summary>
    [JsonPropertyName("eventsRaised")]
    public Common EventsRaised
    {
      get => eventsRaised ??= new();
      set => eventsRaised = value;
    }

    private Common processCountToCommit;
    private Common obsUpdatedYToN;
    private Common obsUpdatedNToY;
    private Common obsStillY;
    private Common obsStillN;
    private Common eventsRaised;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ObligorGroup group.</summary>
    [Serializable]
    public class ObligorGroup
    {
      /// <summary>
      /// A value of Obligor1.
      /// </summary>
      [JsonPropertyName("obligor1")]
      public CsePerson Obligor1
      {
        get => obligor1 ??= new();
        set => obligor1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson obligor1;
    }

    /// <summary>
    /// A value of HardcodeWc.
    /// </summary>
    [JsonPropertyName("hardcodeWc")]
    public ObligationType HardcodeWc
    {
      get => hardcodeWc ??= new();
      set => hardcodeWc = value;
    }

    /// <summary>
    /// A value of HardcodeMs.
    /// </summary>
    [JsonPropertyName("hardcodeMs")]
    public ObligationType HardcodeMs
    {
      get => hardcodeMs ??= new();
      set => hardcodeMs = value;
    }

    /// <summary>
    /// A value of HardcodeMc.
    /// </summary>
    [JsonPropertyName("hardcodeMc")]
    public ObligationType HardcodeMc
    {
      get => hardcodeMc ??= new();
      set => hardcodeMc = value;
    }

    /// <summary>
    /// A value of HardcodeCs.
    /// </summary>
    [JsonPropertyName("hardcodeCs")]
    public ObligationType HardcodeCs
    {
      get => hardcodeCs ??= new();
      set => hardcodeCs = value;
    }

    /// <summary>
    /// A value of HardcodeSp.
    /// </summary>
    [JsonPropertyName("hardcodeSp")]
    public ObligationType HardcodeSp
    {
      get => hardcodeSp ??= new();
      set => hardcodeSp = value;
    }

    /// <summary>
    /// A value of WriteEvent.
    /// </summary>
    [JsonPropertyName("writeEvent")]
    public Common WriteEvent
    {
      get => writeEvent ??= new();
      set => writeEvent = value;
    }

    /// <summary>
    /// A value of ExitState.
    /// </summary>
    [JsonPropertyName("exitState")]
    public ExitStateWorkArea ExitState
    {
      get => exitState ??= new();
      set => exitState = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// Gets a value of Obligor.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorGroup> Obligor => obligor ??= new(
      ObligorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Obligor for json serialization.
    /// </summary>
    [JsonPropertyName("obligor")]
    [Computed]
    public IList<ObligorGroup> Obligor_Json
    {
      get => obligor;
      set => Obligor.Assign(value);
    }

    private ObligationType hardcodeWc;
    private ObligationType hardcodeMs;
    private ObligationType hardcodeMc;
    private ObligationType hardcodeCs;
    private ObligationType hardcodeSp;
    private Common writeEvent;
    private ExitStateWorkArea exitState;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private Infrastructure infrastructure;
    private Array<ObligorGroup> obligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

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
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public ObligationType KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    private CsePersonAccount obligor1;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction debt;
    private Obligation obligation;
    private LegalAction legalAction;
    private ObligationType keyOnly;
    private CsePerson obligor2;
  }
#endregion
}
