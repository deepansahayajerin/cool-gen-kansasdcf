// Program: SP_DOC_UPDATE_FAILED_BATCH_DOC, ID: 371339605, model: 746.
// Short name: SWE02242
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_UPDATE_FAILED_BATCH_DOC.
/// </para>
/// <para>
/// Performs actions necessary for when a batch or email document fails to 
/// print.
/// Should be used in SWEPB709 and SWEPB715
/// </para>
/// </summary>
[Serializable]
public partial class SpDocUpdateFailedBatchDoc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_UPDATE_FAILED_BATCH_DOC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocUpdateFailedBatchDoc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocUpdateFailedBatchDoc.
  /// </summary>
  public SpDocUpdateFailedBatchDoc(IContext context, Import import,
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
    // 01/30/2008	M Ramirez	WR 276, 277	Initial Development.
    // 07/23/2008	J Huss		PR 6102		Business_object was not being imported with 
    // document.
    // 							-Added business_object to document import view.
    // 						Modified error message formatting as data was being cut off.
    // 						Added exit state message to error report.
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    MoveInfrastructure1(import.Infrastructure, local.Infrastructure);
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.EabConvertNumeric.SendAmount =
      NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
    UseEabConvertNumeric1();

    // JHuss 7/23/2008 Modified error message formatting as data was being cut 
    // off.
    local.EabReportSend.RptDetail = "Infrastructure ID =" + TrimEnd
      (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + TrimEnd
      (import.Message.Text50);
    local.EabFileHandling.Action = "WRITE";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "     Document Name = " + TrimEnd
      (import.Document.Name) + ", USERID = " + TrimEnd
      (import.Infrastructure.UserId);
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";

    // mjr
    // -----------------------------------------------------------------
    // Update outgoing_document and infrastructure.
    // --------------------------------------------------------------------
    local.OutgoingDocument.PrintSucessfulIndicator = "N";
    local.OutgoingDocument.LastUpdatedBy = import.ProgramProcessingInfo.Name;
    local.OutgoingDocument.LastUpdatdTstamp = import.Current.Timestamp;
    UseUpdateOutgoingDocument();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Unable to update outgoing_document; Inf ID = " + local
        .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    if (ReadIwoAction())
    {
      if (ReadIwoTransactionCsePersonLegalAction())
      {
        local.IwoAction.Assign(entities.IwoAction);
        local.IwoAction.StatusCd = "E";
        local.IwoTransaction.Identifier = entities.IwoTransaction.Identifier;
        UseLeUpdateIwoActionStatus();
      }
    }

    local.Infrastructure.Detail = "Document was not successfully printed.";
    local.Infrastructure.LastUpdatedBy = import.ProgramProcessingInfo.Name;
    local.Infrastructure.LastUpdatedTimestamp = import.Current.Timestamp;
    UseSpCabUpdateInfrastructure();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Unable to update infrastructure; Inf ID = " + local
        .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // mjr
    // -----------------------------------------------------------
    // Document failed to print.  An alert will be sent to the user.
    // The type of alert sent depends on whether the document is monitored or 
    // not.
    // --------------------------------------------------------------
    local.Field.Dependancy = " KEY";
    UseSpPrintDataRetrievalKeys();

    if (!Equal(import.Document.Name, "ARCLOS60") && !
      Equal(import.Document.Name, "NOTCCLOS"))
    {
      local.DateWorkArea.Date = import.Current.Date;
    }
    else
    {
      local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
    }

    if (Equal(import.Document.Name, "CASETXFR"))
    {
      local.Field.Dependancy = "CASE";
      UseSpPrintDataRetrievalCase();
    }

    UseSpDocGetServiceProvider();
    local.ServiceProvider.SystemGeneratedId =
      local.OutDocRtrnAddr.ServProvSysGenId;
    local.Office.SystemGeneratedId = local.OutDocRtrnAddr.OfficeSysGenId;
    local.OfficeServiceProvider.RoleCode = local.OutDocRtrnAddr.OspRoleCode;
    local.OfficeServiceProvider.EffectiveDate =
      local.OutDocRtrnAddr.OspEffectiveDate;

    if (import.Document.RequiredResponseDays > 0)
    {
      local.OfficeServiceProviderAlert.Assign(import.Monitored);
    }
    else
    {
      local.OfficeServiceProviderAlert.Assign(import.UnMonitored);
    }

    local.OfficeServiceProviderAlert.RecipientUserId =
      local.OutDocRtrnAddr.ServProvUserId;
    UseSpCabCreateOfcSrvPrvdAlert();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail =
        "     Unable to send Alert to Service Provider for the previous error.  Exit state message follows:";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // JHuss 7/23/08  Added exit state message to error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "          " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
    target.CaseUnitState = source.CaseUnitState;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveSpDocKey1(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
  }

  private static void MoveSpDocKey2(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyOffice = source.KeyOffice;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
    target.KeyXferFromDate = source.KeyXferFromDate;
    target.KeyXferToDate = source.KeyXferToDate;
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

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseLeUpdateIwoActionStatus()
  {
    var useImport = new LeUpdateIwoActionStatus.Import();
    var useExport = new LeUpdateIwoActionStatus.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    useImport.IwoAction.Assign(local.IwoAction);
    useImport.IwoTransaction.Identifier = local.IwoTransaction.Identifier;

    Call(LeUpdateIwoActionStatus.Execute, useImport, useExport);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.OfficeServiceProviderAlert.
      Assign(local.OfficeServiceProviderAlert);
    useImport.ServiceProvider.SystemGeneratedId =
      local.ServiceProvider.SystemGeneratedId;

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseSpDocGetServiceProvider()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = local.DateWorkArea.Date;
    MoveSpDocKey1(local.SpDocKey, useImport.SpDocKey);

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.OutDocRtrnAddr.Assign(useExport.OutDocRtrnAddr);
  }

  private void UseSpPrintDataRetrievalCase()
  {
    var useImport = new SpPrintDataRetrievalCase.Import();
    var useExport = new SpPrintDataRetrievalCase.Export();

    MoveSpDocKey2(local.SpDocKey, useImport.SpDocKey);
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);
    useImport.Document.Assign(import.Document);
    useImport.Field.Dependancy = local.Field.Dependancy;
    useImport.Current.Date = import.Current.Date;

    Call(SpPrintDataRetrievalCase.Execute, useImport, useExport);

    MoveSpDocKey1(useExport.SpDocKey, local.SpDocKey);
  }

  private void UseSpPrintDataRetrievalKeys()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    useImport.Document.Assign(import.Document);
    useImport.Field.Dependancy = local.Field.Dependancy;
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    local.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.OutgoingDocument.Assign(local.OutgoingDocument);

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private bool ReadIwoAction()
  {
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", local.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 2);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 3);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 4);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 5);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 6);
        entities.IwoAction.CspNumber = db.GetString(reader, 7);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 8);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 9);
        entities.IwoAction.InfId = db.GetNullableInt32(reader, 10);
        entities.IwoAction.Populated = true;
      });
  }

  private bool ReadIwoTransactionCsePersonLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.IwoTransaction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadIwoTransactionCsePersonLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 1);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 2);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.LegalAction.Identifier = db.GetInt32(reader, 3);
        entities.LegalAction.KeyChangeDate = db.GetNullableDate(reader, 4);
        entities.IwoTransaction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public WorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of Monitored.
    /// </summary>
    [JsonPropertyName("monitored")]
    public OfficeServiceProviderAlert Monitored
    {
      get => monitored ??= new();
      set => monitored = value;
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
    /// A value of UnMonitored.
    /// </summary>
    [JsonPropertyName("unMonitored")]
    public OfficeServiceProviderAlert UnMonitored
    {
      get => unMonitored ??= new();
      set => unMonitored = value;
    }

    private DateWorkArea current;
    private Document document;
    private Infrastructure infrastructure;
    private WorkArea message;
    private OfficeServiceProviderAlert monitored;
    private ProgramProcessingInfo programProcessingInfo;
    private OfficeServiceProviderAlert unMonitored;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea dateWorkArea;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Field field;
    private Infrastructure infrastructure;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OutDocRtrnAddr outDocRtrnAddr;
    private OutgoingDocument outgoingDocument;
    private ServiceProvider serviceProvider;
    private SpDocKey spDocKey;
    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
  }
#endregion
}
