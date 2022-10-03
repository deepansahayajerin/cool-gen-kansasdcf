// Program: FN_BFX7_CRT_NTC_FIX, ID: 372913690, model: 746.
// Short name: SWEFFX7B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX7_CRT_NTC_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx7CrtNtcFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX7_CRT_NTC_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx7CrtNtcFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx7CrtNtcFix.
  /// </summary>
  public FnBfx7CrtNtcFix(IContext context, Import import, Export export):
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
    local.UserId.Text8 = global.UserId;
    local.Current.Timestamp = Now();
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = local.UserId.Text8;
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ApplyUpdatesInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (IsEmpty(local.ApplyUpdatesInd.Flag))
    {
      local.ApplyUpdatesInd.Flag = "N";
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PARMS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Perform Updates . . . . . . . : " + local
      .ApplyUpdatesInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    foreach(var item in ReadCsePersonCollection())
    {
      ++local.ReadCnt.Count;

      if (ReadCashReceiptSourceType())
      {
        if (AsChar(entities.ExistingCashReceiptSourceType.CourtInd) == 'C')
        {
          continue;
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ERROR - Cash Receipt Source Type Not Found, OB : " + entities
          .ExistingObligor1.Number;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (ReadCashReceiptType())
      {
        if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == 2 || entities
          .ExistingCashReceiptType.SystemGeneratedIdentifier == 7)
        {
          continue;
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ERROR - Cash Receipt Type Not Found, OB : " + entities
          .ExistingObligor1.Number;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (ReadFips())
      {
        if (entities.ExistingFips.State != 20)
        {
          continue;
        }
      }
      else
      {
        local.EabReportSend.RptDetail = "ERROR - FIPS Not Found, OB : " + entities
          .ExistingObligor1.Number;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      // : Set the Court Notice Required Indicator to "Y".
      ++local.UpdateCnt.Count;
      local.EabReportSend.RptDetail = "UPDATE - OB : " + entities
        .ExistingObligor1.Number + ", SRC/CD : " + entities
        .ExistingCashReceiptSourceType.Code + "/" + entities
        .ExistingCashReceiptSourceType.CourtInd + ", CRT ORD : " + entities
        .ExistingCollection.CourtOrderAppliedTo + ", COLL ID : " + NumberToString
        (entities.ExistingCollection.SystemGeneratedIdentifier, 15);
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (AsChar(local.ApplyUpdatesInd.Flag) == 'Y')
      {
        // : Process the Adjustment.
        try
        {
          UpdateCollection();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_COLLECTION_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    local.EabReportSend.RptDetail = "Read Count . . . . . . . . . . . . : " + NumberToString
      (local.ReadCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Update Count . . . . . . . . . . . : " + NumberToString
      (local.UpdateCnt.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(command, "crSrceTypeId", entities.ExistingCollection.CstId);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 2);
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.ExistingCollection.CrtType);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCollection()
  {
    entities.ExistingObligor1.Populated = false;
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCsePersonCollection",
      null,
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 0);
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CpaType = db.GetString(reader, 8);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 9);
        entities.ExistingCollection.OtrType = db.GetString(reader, 10);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 11);
        entities.ExistingCollection.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.ExistingCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 13);
        entities.ExistingCollection.CourtNoticeReqInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCollection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 15);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 16);
        entities.ExistingCollection.CourtNoticeAdjProcessDate =
          db.GetDate(reader, 17);
        entities.ExistingObligor1.Populated = true;
        entities.ExistingCollection.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetInt32(command, "dtyGeneratedId", entities.ExistingCollection.OtyId);
          
        db.SetInt32(command, "obId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.Populated = true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);

    var lastUpdatedBy = local.UserId.Text8;
    var lastUpdatedTmst = local.Current.Timestamp;
    var courtNoticeReqInd = "Y";

    entities.ExistingCollection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "crtNoticeReqInd", courtNoticeReqInd);
        db.SetInt32(
          command, "collId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.ExistingCollection.CrtType);
        db.SetInt32(command, "cstId", entities.ExistingCollection.CstId);
        db.SetInt32(command, "crvId", entities.ExistingCollection.CrvId);
        db.SetInt32(command, "crdId", entities.ExistingCollection.CrdId);
        db.SetInt32(command, "obgId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.SetInt32(command, "otrId", entities.ExistingCollection.OtrId);
        db.SetString(command, "otrType", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "otyId", entities.ExistingCollection.OtyId);
      });

    entities.ExistingCollection.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCollection.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingCollection.CourtNoticeReqInd = courtNoticeReqInd;
    entities.ExistingCollection.Populated = true;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ApplyUpdatesInd.
    /// </summary>
    [JsonPropertyName("applyUpdatesInd")]
    public Common ApplyUpdatesInd
    {
      get => applyUpdatesInd ??= new();
      set => applyUpdatesInd = value;
    }

    /// <summary>
    /// A value of ReadCnt.
    /// </summary>
    [JsonPropertyName("readCnt")]
    public Common ReadCnt
    {
      get => readCnt ??= new();
      set => readCnt = value;
    }

    /// <summary>
    /// A value of UpdateCnt.
    /// </summary>
    [JsonPropertyName("updateCnt")]
    public Common UpdateCnt
    {
      get => updateCnt ??= new();
      set => updateCnt = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private DateWorkArea null1;
    private Common applyUpdatesInd;
    private Common readCnt;
    private Common updateCnt;
    private TextWorkArea userId;
    private DateWorkArea current;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private Collection existingCollection;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceipt existingCashReceipt;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptDetail existingCashReceiptDetail;
    private LegalAction existingLegalAction;
    private Tribunal existingTribunal;
    private Fips existingFips;
  }
#endregion
}
