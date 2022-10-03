// Program: SP_PRINT_DATA_RETRIEVAL_MAIN, ID: 372132407, model: 746.
// Short name: SWE02230
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
/// A program: SP_PRINT_DATA_RETRIEVAL_MAIN.
/// </para>
/// <para>
/// Input:	document name
/// 	next_tran_info
/// Output:	database writes to outgoing_document and field_value entities
/// 	error text to display in the group view
/// This cab is used by any procedure that must retrieve data for document 
/// printing.  Normal use is through DDOC Dead Document, and each batch job that
/// prints documents.
/// Data retrieval utilizes the field and document_field entities, and populates
/// the field_value entity (One for each document_field.)
/// </para>
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalMain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_MAIN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalMain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalMain.
  /// </summary>
  public SpPrintDataRetrievalMain(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------
    // 10/01/1998	M Ramirez			Initial Development
    // 05/27/1999	M Ramirez			Removed sp_cab_update_infrastructure
    // 07/14/1999	M Ramirez			Added row lock counts
    // 10/26/1999	M Ramirez	78209		Added ARCLOS60 to check
    // 						for current date
    // 05/08/2000	M Ramirez	93407		Change the preference for populating
    // 						the infrastructure cse_person_number
    // 						attribute
    // 03/24/2006      M J Quinn       PR266315        When printing from GTSC 
    // screen, only print
    // 						attorney information for the specified
    // 						case.  Export Last Tran to
    // 						SP_PRINT_DATA_RETRIEVAL_PERSON.
    // 09/27/2006      J Bahre		PR285545	Replaced Exit State 'Infrastructure 
    // record
    // 						not found' with a new Exit State so problem
    // 						with error on screen can be located more easily.
    // 11/17/2011	G Vandy		CQ30161		Add support for new CASETXFR document 
    // fields.
    // ----------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!Lt(local.Current.Date, import.Batch.ProcessDate))
    {
      // mjr
      // ----------------------------------------------
      // 01/01/1999
      // Batch process date is not populated:  use current date
      // -----------------------------------------------------------
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = import.Batch.ProcessDate;
      local.Batch.Flag = "Y";
    }

    if (IsEmpty(import.Batch.Name))
    {
      // mjr
      // ----------------------------------------------
      // 01/01/1999
      // UserID is not populated:  use current userid
      // -----------------------------------------------------------
      local.FieldValue.CreatedBy = global.UserId;
    }
    else
    {
      local.FieldValue.CreatedBy = import.Batch.Name;
      local.Batch.Flag = "Y";
    }

    local.FieldValue.CreatedTimestamp = Now();

    if (ReadInfrastructure())
    {
      export.Infrastructure.Assign(entities.Infrastructure);
    }
    else
    {
      // ---------------------------------------------------------
      // JLB  PR285545 09/27/2006 Added a new exit state.
      // --------------------------------------------------------
      ExitState = "INFRASTRUCTURE_NF_7";

      return;
    }

    if (ReadDocument())
    {
      local.Document.Assign(entities.Document);

      if (Equal(local.Document.Name, "ARCLOS60") || Equal
        (local.Document.Name, "NOTCCLOS"))
      {
        // mjr---> Override current date
        local.Current.Date = export.Infrastructure.ReferenceDate;
      }
    }
    else
    {
      ExitState = "DOCUMENT_NF";

      return;
    }

    foreach(var item in ReadField())
    {
      // mjr--->  For Fields processed in called CABs
      if (!Lt(local.Previous.Dependancy, entities.Field.Dependancy))
      {
        continue;
      }

      local.Previous.Assign(entities.Field);

      // --------------------------------------------------------------------
      // Process each Field through the sp_print_data_retrieval cabs
      // The main cab simply determines that a subroutine needs to be invoked.
      // --------------------------------------------------------------------
      switch(TrimEnd(entities.Field.Dependancy))
      {
        case " KEY":
          UseSpPrintDataRetrievalKeys();

          break;
        case "AAPPEAL":
          UseSpPrintDataRetrievalAappeal();

          break;
        case "CASE":
          UseSpPrintDataRetrievalCase();

          break;
        case "LA":
          UseSpPrintDataRetrievalLa();

          break;
        case "MISC":
          UseSpPrintDataRetrievalMisc();

          break;
        case "PERSON":
          UseSpPrintDataRetrievalPerson();

          break;
        case "SYSTEM":
          UseSpPrintDataRetrievalSystem();

          break;
        case "WRKSHEET":
          UseSpPrintDataRetrievalWrksheet();

          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Subroutine";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;
          ExitState = "FIELD_NF";

          break;
      }

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // mjr
    // ----------------------------------------------
    // Set other infrastructure denorm id's here
    // -------------------------------------------------
    if (local.SpDocKey.KeyLegalAction > 0)
    {
      export.Infrastructure.DenormNumeric12 = local.SpDocKey.KeyLegalAction;
    }

    if (!IsEmpty(local.SpDocKey.KeyCase))
    {
      if (Equal(import.Document.Name, "CASETXFR"))
      {
        goto Test;
      }

      export.Infrastructure.CaseNumber = local.SpDocKey.KeyCase;
    }

Test:

    // mjr
    // -----------------------------------------------------------
    // 05/08/2000
    // Change the preference for populating the infrastructure
    // cse_person_number attribute
    // ------------------------------------------------------------------------
    if (!IsEmpty(local.SpDocKey.KeyAp))
    {
      export.Infrastructure.CsePersonNumber = local.SpDocKey.KeyAp;
    }
    else if (!IsEmpty(local.SpDocKey.KeyPerson))
    {
      export.Infrastructure.CsePersonNumber = local.SpDocKey.KeyPerson;
    }
    else if (!IsEmpty(local.SpDocKey.KeyChild))
    {
      export.Infrastructure.CsePersonNumber = local.SpDocKey.KeyChild;
    }
    else
    {
      export.Infrastructure.CsePersonNumber = local.SpDocKey.KeyAr;
    }

    export.Infrastructure.LastUpdatedBy = local.FieldValue.CreatedBy;
    export.Infrastructure.LastUpdatedTimestamp =
      local.FieldValue.CreatedTimestamp;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Dependancy = source.Dependancy;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveFieldValue(FieldValue source, FieldValue target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
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
    target.KeyLocateRequestAgency = source.KeyLocateRequestAgency;
    target.KeyLocateRequestSource = source.KeyLocateRequestSource;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpPrintDataRetrievalAappeal()
  {
    var useImport = new SpPrintDataRetrievalAappeal.Import();
    var useExport = new SpPrintDataRetrievalAappeal.Export();

    useImport.SpDocKey.Assign(local.SpDocKey);
    useImport.Batch.Flag = local.Batch.Flag;
    MoveInfrastructure1(export.Infrastructure, useImport.Infrastructure);
    useImport.Document.Assign(local.Document);
    MoveField(entities.Field, useImport.Field);
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Current.Date = local.Current.Date;
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalAappeal.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    MoveSpDocKey(useExport.SpDocKey, local.SpDocKey);
  }

  private void UseSpPrintDataRetrievalCase()
  {
    var useImport = new SpPrintDataRetrievalCase.Import();
    var useExport = new SpPrintDataRetrievalCase.Export();

    useImport.SpDocKey.Assign(local.SpDocKey);
    MoveInfrastructure2(export.Infrastructure, useImport.Infrastructure);
    useImport.Document.Assign(local.Document);
    MoveField(entities.Field, useImport.Field);
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Current.Date = local.Current.Date;
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalCase.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    MoveSpDocKey(useExport.SpDocKey, local.SpDocKey);
  }

  private void UseSpPrintDataRetrievalKeys()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    MoveInfrastructure2(export.Infrastructure, useImport.Infrastructure);
    useImport.Document.Assign(local.Document);
    MoveField(entities.Field, useImport.Field);
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    local.SpDocKey.Assign(useExport.SpDocKey);
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
  }

  private void UseSpPrintDataRetrievalLa()
  {
    var useImport = new SpPrintDataRetrievalLa.Import();
    var useExport = new SpPrintDataRetrievalLa.Export();

    useImport.SpDocKey.Assign(local.SpDocKey);
    useImport.Batch.Flag = local.Batch.Flag;
    MoveInfrastructure2(export.Infrastructure, useImport.Infrastructure);
    MoveField(entities.Field, useImport.Field);
    MoveDocument(local.Document, useImport.Document);
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Current.Date = local.Current.Date;
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalLa.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorInd.Flag = useExport.ErrorInd.Flag;
    MoveSpDocKey(useExport.SpDocKey, local.SpDocKey);
  }

  private void UseSpPrintDataRetrievalMisc()
  {
    var useImport = new SpPrintDataRetrievalMisc.Import();
    var useExport = new SpPrintDataRetrievalMisc.Export();

    useImport.SpDocKey.Assign(local.SpDocKey);
    MoveInfrastructure2(export.Infrastructure, useImport.Infrastructure);
    useImport.Document.Assign(local.Document);
    MoveField(entities.Field, useImport.Field);
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Current.Date = local.Current.Date;
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalMisc.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    MoveSpDocKey(useExport.SpDocKey, local.SpDocKey);
  }

  private void UseSpPrintDataRetrievalPerson()
  {
    var useImport = new SpPrintDataRetrievalPerson.Import();
    var useExport = new SpPrintDataRetrievalPerson.Export();

    useImport.NextTranInfo.LastTran = import.NextTranInfo.LastTran;
    useImport.SpDocKey.Assign(local.SpDocKey);
    useImport.Batch.Flag = local.Batch.Flag;
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Document.Assign(local.Document);
    MoveField(entities.Field, useImport.Field);
    MoveInfrastructure2(export.Infrastructure, useImport.Infrastructure);
    useImport.Current.Date = local.Current.Date;
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalPerson.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    MoveSpDocKey(useExport.SpDocKey, local.SpDocKey);
  }

  private void UseSpPrintDataRetrievalSystem()
  {
    var useImport = new SpPrintDataRetrievalSystem.Import();
    var useExport = new SpPrintDataRetrievalSystem.Export();

    useImport.SpDocKey.Assign(local.SpDocKey);
    MoveInfrastructure1(export.Infrastructure, useImport.Infrastructure);
    useImport.Document.Assign(local.Document);
    MoveField(entities.Field, useImport.Field);
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Current.Date = local.Current.Date;
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalSystem.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    export.Infrastructure.UserId = useExport.Infrastructure.UserId;
  }

  private void UseSpPrintDataRetrievalWrksheet()
  {
    var useImport = new SpPrintDataRetrievalWrksheet.Import();
    var useExport = new SpPrintDataRetrievalWrksheet.Export();

    useImport.SpDocKey.KeyWorksheet = local.SpDocKey.KeyWorksheet;
    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    MoveField(entities.Field, useImport.Field);
    MoveDocument(local.Document, useImport.Document);
    useImport.ExpImpRowLockFieldValue.Count =
      import.ExpImpRowLockFieldValue.Count;

    Call(SpPrintDataRetrievalWrksheet.Execute, useImport, useExport);

    import.ExpImpRowLockFieldValue.Count =
      useImport.ExpImpRowLockFieldValue.Count;
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.BusinessObject = db.GetString(reader, 1);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
        entities.Document.ExpirationDate = db.GetDate(reader, 3);
        entities.Document.Populated = true;
      });
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "docName", local.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          local.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 24);
        entities.Infrastructure.Populated = true;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public ProgramProcessingInfo Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of ExpImpRowLockFieldValue.
    /// </summary>
    [JsonPropertyName("expImpRowLockFieldValue")]
    public Common ExpImpRowLockFieldValue
    {
      get => expImpRowLockFieldValue ??= new();
      set => expImpRowLockFieldValue = value;
    }

    private NextTranInfo nextTranInfo;
    private Infrastructure infrastructure;
    private Document document;
    private ProgramProcessingInfo batch;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Common errorInd;
    private FieldValue errorFieldValue;
    private DocumentField errorDocumentField;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SubscriptDocument.
    /// </summary>
    [JsonPropertyName("subscriptDocument")]
    public Common SubscriptDocument
    {
      get => subscriptDocument ??= new();
      set => subscriptDocument = value;
    }

    /// <summary>
    /// A value of SubscriptDocumentField.
    /// </summary>
    [JsonPropertyName("subscriptDocumentField")]
    public Common SubscriptDocumentField
    {
      get => subscriptDocumentField ??= new();
      set => subscriptDocumentField = value;
    }

    /// <summary>
    /// A value of SubscriptField.
    /// </summary>
    [JsonPropertyName("subscriptField")]
    public Common SubscriptField
    {
      get => subscriptField ??= new();
      set => subscriptField = value;
    }

    /// <summary>
    /// A value of SubscriptFieldValue.
    /// </summary>
    [JsonPropertyName("subscriptFieldValue")]
    public Common SubscriptFieldValue
    {
      get => subscriptFieldValue ??= new();
      set => subscriptFieldValue = value;
    }

    /// <summary>
    /// A value of SubscriptInfrastructure.
    /// </summary>
    [JsonPropertyName("subscriptInfrastructure")]
    public Common SubscriptInfrastructure
    {
      get => subscriptInfrastructure ??= new();
      set => subscriptInfrastructure = value;
    }

    /// <summary>
    /// A value of SubscriptOutgoingDoc.
    /// </summary>
    [JsonPropertyName("subscriptOutgoingDoc")]
    public Common SubscriptOutgoingDoc
    {
      get => subscriptOutgoingDoc ??= new();
      set => subscriptOutgoingDoc = value;
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
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
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Field Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    private Common subscriptDocument;
    private Common subscriptDocumentField;
    private Common subscriptField;
    private Common subscriptFieldValue;
    private Common subscriptInfrastructure;
    private Common subscriptOutgoingDoc;
    private SpDocKey spDocKey;
    private Common batch;
    private Document document;
    private OutDocRtrnAddr outDocRtrnAddr;
    private ServiceProvider serviceProvider;
    private FieldValue fieldValue;
    private Field previous;
    private DateWorkArea max;
    private DateWorkArea current;
    private OutgoingDocument outgoingDocument;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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

    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private Field field;
    private DocumentField documentField;
    private Document document;
  }
#endregion
}
