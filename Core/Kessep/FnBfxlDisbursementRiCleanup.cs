// Program: FN_BFXL_DISBURSEMENT_RI_CLEANUP, ID: 371413734, model: 746.
// Short name: SWEFFXLB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXL_DISBURSEMENT_RI_CLEANUP.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxlDisbursementRiCleanup: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXL_DISBURSEMENT_RI_CLEANUP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxlDisbursementRiCleanup(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxlDisbursementRiCleanup.
  /// </summary>
  public FnBfxlDisbursementRiCleanup(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 06/04/09  GVandy	CQ11376		Cleanup referential integrity errors between 
    // DISBURSEMENT
    // 					and PAYMENT_REQUEST.
    // -----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "OPEN";

    // --  Read PPI record.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;

    // -- Open error report
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -- Open control report
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // --  Get commit frequency.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // -- Create a dummy payment_request that can be associated to and then 
    // disassociated from disbursements
    //    which currently point to a payment request that does not exist.
    if (ReadPaymentRequest1())
    {
      if (!Equal(entities.Dummy.CreatedBy, global.UserId))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      try
      {
        CreatePaymentRequest();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // -- Find all disbursement records whose payment request foreign key is not
    // null.
    foreach(var item in ReadDisbursementCsePerson())
    {
      // -- Now actually read for the payment request that the disbursement is 
      // pointing to.
      if (ReadPaymentRequest2())
      {
        // -- The payment request exists.  However, if the payment request is 
        // not for the same AR as the
        //    disbursement then disassociate the disbursement from the payment 
        // request.
        if (!Equal(entities.CsePerson.Number,
          entities.PaymentRequest.CsePersonNumber))
        {
          DisassociateDisbursement1();
          ++local.UpdateCount.Count;
          ++local.TotalError.Count;

          local.G.Index = 2010 - Year
            (Date(entities.Disbursement.CreatedTimestamp)) - 1;
          local.G.CheckSize();

          local.G.Update.GlocalError.Count = local.G.Item.GlocalError.Count + 1;
          local.EabReportSend.RptDetail = "Disb ID " + NumberToString
            (entities.Disbursement.SystemGeneratedIdentifier, 7, 9);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "   PRQ ID " + NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9) + "  CSP #s Disb/PR/DP  " +
            entities.CsePerson.Number + "/" + entities
            .PaymentRequest.CsePersonNumber + "/" + entities
            .PaymentRequest.DesignatedPayeeCsePersonNo;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
      else
      {
        // -- The payment request does not exist.  To set the foreign key back 
        // to null we will associate the disbursement
        //    with the dummy payment request and then disassociate the two.
        AssociateDisbursement();
        DisassociateDisbursement2();
        ++local.UpdateCount.Count;
        ++local.TotalPhantom.Count;

        local.G.Index = 2010 - Year
          (Date(entities.Disbursement.CreatedTimestamp)) - 1;
        local.G.CheckSize();

        local.G.Update.GlocalPhantom.Count =
          local.G.Item.GlocalPhantom.Count + 1;
      }

      // -- Determine if it is time to commit.
      if (local.UpdateCount.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.UpdateCount.Count = 0;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    // -- We no longer need the dummy payment request.  Delete it.
    DeletePaymentRequest();

    // -- Log the counts to the control report.
    for(local.I.Count = 1; local.I.Count <= 6; ++local.I.Count)
    {
      switch(local.I.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Total Phantom Count = " + NumberToString
            (local.TotalPhantom.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "Total Error Count   = " + NumberToString
            (local.TotalError.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "YEAR   Phantom Count    Error Count";

          break;
        case 6:
          local.EabReportSend.RptDetail = "----   -------------    -----------";

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.I.Count = 1;

    for(var limit = local.G.Count; local.I.Count <= limit; ++local.I.Count)
    {
      local.G.Index = local.I.Count - 1;
      local.G.CheckSize();

      local.EabReportSend.RptDetail =
        NumberToString((long)2010 - local.I.Count, 12, 4) + "     " + NumberToString
        (local.G.Item.GlocalPhantom.Count, 7, 9) + "       " + NumberToString
        (local.G.Item.GlocalError.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabFileHandling.Action = "CLOSE";

    // -- Close control report
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // -- Close error report
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void AssociateDisbursement()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);

    var prqGeneratedId = entities.Dummy.SystemGeneratedIdentifier;

    entities.Disbursement.Populated = false;
    Update("AssociateDisbursement",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.Disbursement.SystemGeneratedIdentifier);
      });

    entities.Disbursement.PrqGeneratedId = prqGeneratedId;
    entities.Disbursement.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = 999999999;
    var processDate = new DateTime(2009, 1, 1);
    var amount = 0M;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var classification = "SUP";
    var type1 = "WAR";

    CheckValid<PaymentRequest>("Type1", type1);
    entities.Dummy.Populated = false;
    Update("CreatePaymentRequest",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "dpCsePerNum", "");
        db.SetNullableString(command, "imprestFundCode", "");
        db.SetString(command, "classification", classification);
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetNullableDate(command, "printDate", default(DateTime));
        db.SetString(command, "type", type1);
      });

    entities.Dummy.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Dummy.ProcessDate = processDate;
    entities.Dummy.Amount = amount;
    entities.Dummy.CreatedBy = createdBy;
    entities.Dummy.CreatedTimestamp = createdTimestamp;
    entities.Dummy.Classification = classification;
    entities.Dummy.Type1 = type1;
    entities.Dummy.PrqRGeneratedId = null;
    entities.Dummy.Populated = true;
  }

  private void DeletePaymentRequest()
  {
    bool exists;

    Update("DeletePaymentRequest#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId", entities.Dummy.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId", entities.Dummy.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ELEC_FUND_TRAN\".",
        "50001");
    }

    Update("DeletePaymentRequest#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId", entities.Dummy.SystemGeneratedIdentifier);
      });

    Update("DeletePaymentRequest#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId", entities.Dummy.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId", entities.Dummy.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    Update("DeletePaymentRequest#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId", entities.Dummy.SystemGeneratedIdentifier);
      });
  }

  private void DisassociateDisbursement1()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.Disbursement.Populated = false;
    Update("DisassociateDisbursement1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.Disbursement.SystemGeneratedIdentifier);
      });

    entities.Disbursement.PrqGeneratedId = null;
    entities.Disbursement.Populated = true;
  }

  private void DisassociateDisbursement2()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.Disbursement.Populated = false;
    Update("DisassociateDisbursement2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Disbursement.CpaType);
        db.SetString(command, "cspNumber", entities.Disbursement.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.Disbursement.SystemGeneratedIdentifier);
      });

    entities.Disbursement.PrqGeneratedId = null;
    entities.Disbursement.Populated = true;
  }

  private IEnumerable<bool> ReadDisbursementCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Disbursement.Populated = false;

    return ReadEach("ReadDisbursementCsePerson",
      null,
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Type1 = db.GetString(reader, 3);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 5);
        entities.CsePerson.Populated = true;
        entities.Disbursement.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Disbursement.Type1);
          

        return true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.Dummy.Populated = false;

    return Read("ReadPaymentRequest1",
      null,
      (db, reader) =>
      {
        entities.Dummy.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Dummy.ProcessDate = db.GetDate(reader, 1);
        entities.Dummy.Amount = db.GetDecimal(reader, 2);
        entities.Dummy.CreatedBy = db.GetString(reader, 3);
        entities.Dummy.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Dummy.Classification = db.GetString(reader, 5);
        entities.Dummy.Type1 = db.GetString(reader, 6);
        entities.Dummy.PrqRGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Dummy.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Dummy.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Disbursement.PrqGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.PaymentRequest.Populated = true;
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
    /// <summary>A GGroup group.</summary>
    [Serializable]
    public class GGroup
    {
      /// <summary>
      /// A value of GlocalError.
      /// </summary>
      [JsonPropertyName("glocalError")]
      public Common GlocalError
      {
        get => glocalError ??= new();
        set => glocalError = value;
      }

      /// <summary>
      /// A value of GlocalPhantom.
      /// </summary>
      [JsonPropertyName("glocalPhantom")]
      public Common GlocalPhantom
      {
        get => glocalPhantom ??= new();
        set => glocalPhantom = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common glocalError;
      private Common glocalPhantom;
    }

    /// <summary>
    /// A value of UpdateCount.
    /// </summary>
    [JsonPropertyName("updateCount")]
    public Common UpdateCount
    {
      get => updateCount ??= new();
      set => updateCount = value;
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
    /// A value of K.
    /// </summary>
    [JsonPropertyName("k")]
    public Common K
    {
      get => k ??= new();
      set => k = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// Gets a value of G.
    /// </summary>
    [JsonIgnore]
    public Array<GGroup> G => g ??= new(GGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of G for json serialization.
    /// </summary>
    [JsonPropertyName("g")]
    [Computed]
    public IList<GGroup> G_Json
    {
      get => g;
      set => G.Assign(value);
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
    /// A value of TotalError.
    /// </summary>
    [JsonPropertyName("totalError")]
    public Common TotalError
    {
      get => totalError ??= new();
      set => totalError = value;
    }

    /// <summary>
    /// A value of TotalPhantom.
    /// </summary>
    [JsonPropertyName("totalPhantom")]
    public Common TotalPhantom
    {
      get => totalPhantom ??= new();
      set => totalPhantom = value;
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

    private Common updateCount;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common k;
    private Common i;
    private Array<GGroup> g;
    private EabFileHandling eabFileHandling;
    private Common totalError;
    private Common totalPhantom;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public PaymentRequest Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public PaymentRequest KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private PaymentRequest dummy;
    private PaymentRequest keyOnly;
    private PaymentRequest paymentRequest;
    private DisbursementTransaction disbursement;
  }
#endregion
}
