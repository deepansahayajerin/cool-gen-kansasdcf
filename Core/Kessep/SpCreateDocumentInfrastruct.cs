// Program: SP_CREATE_DOCUMENT_INFRASTRUCT, ID: 371913168, model: 746.
// Short name: SWE01719
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
/// A program: SP_CREATE_DOCUMENT_INFRASTRUCT.
/// </para>
/// <para>
/// This cab creates an INFRASTRUCTURE, RECORDED_DOC, and OUTGOING_DOC.
/// Imports required:
/// 	document name
/// Imports normally required:
/// 	sp_doc_key (whatever is available)
/// Defaulted, if not passed in:
/// 	document effective_date
/// 	document type
/// 	infrastructure reference_date
/// 	infrastructure created_by
/// 	infrastructure created_timestamp
/// Imports ignored:
/// 	document required_response_days
/// 	infrastructure business_object_cd
/// 	infrastructure denorm_xxx
/// 	infrastructure case_number
/// 	infrastructure cse_person_number
/// 	infrastructure case_unit_number
/// 	infrastructure user_id
/// </para>
/// </summary>
[Serializable]
public partial class SpCreateDocumentInfrastruct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_DOCUMENT_INFRASTRUCT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateDocumentInfrastruct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateDocumentInfrastruct.
  /// </summary>
  public SpCreateDocumentInfrastruct(IContext context, Import import,
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
    // -----------------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // -----------------------------------------------------------------------------
    // 11/20/1996	Siraj Konkader			Initial Dev
    // 10/98-06/99	M Ramirez			Various changes including:
    // -Added creation of outgoing document
    // -Removed creation of monitored_document
    // -Modified for batch documents (TYPE = BTCH)
    // -Included creation of ' KEY' field_values based on import sp_doc_key
    // -Changed denormed infrastructure to use import sp_doc_key
    // -Added update logic to reuse infrastructure/outgoing_doc based on import 
    // infrastructure sys_gen_id
    // -Changed current date to reflect infrastructure reference_date
    // 01/11/00	PMcElderry	80211
    // logic to close monitored activity #55.  Also effects 
    // SP_CREATE_OUTGOING_DOCUMENT and view matching in
    // SP_DOC_GET_SERVICE_PROVIDER
    // 02/12/2001	M Ramirez	WR187, Seg B
    // Added check for 'MAIL' type
    // 05/08/2001	M Ramirez	WR291
    // Added Locate Request key attributes
    // 09/25/2001      T Bobb          PR127590
    // Removed check for expired document
    // 06/24/2002	M Ramirez	148447
    // Added logic for special business objects when an OSP is not found using 
    // SP_DOC_GET_SERVICE_PROVIDER (same as DKEY)
    // 07/30/2008	J Huss		PR 984
    // Changed when print_sucessful_indicator is set.
    // This allows processing to complete before the document is available to be
    // printed.
    // 11/17/2011	G Vandy		CQ30161
    // Add support for new keys for the CASETXFR document; CASXFROFFC, 
    // CASXFRFRDT, and CASXFRTODT.
    // -----------------------------------------------------------------------------
    // ***************************************************
    // mjr--->	Imports required:
    // 		document name
    // 	Imports normally required:
    // 		sp_doc_key (whatever is available)
    // 	Defaulted, if not passed in:
    // 		document effective_date
    // 		document type
    // 		infrastructure reference_date
    // 		infrastructure created_by
    // 		infrastructure created_timestamp
    // 	Imports ignored:
    // 		document required_response_days
    // 		infrastructure business_object_cd
    // 		infrastructure denorm_xxx
    // 		infrastructure case_number
    // 		infrastructure cse_person_number
    // 		infrastructure case_unit_number
    // 		infrastructure user_id
    // ***************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Lt(local.Null1.Date, import.Infrastructure.ReferenceDate))
    {
      local.Current.Date = import.Infrastructure.ReferenceDate;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!IsEmpty(import.Document.Name))
    {
      if (Lt(local.Null1.Date, import.Document.EffectiveDate))
      {
        if (ReadDocument1())
        {
          local.Document.Assign(entities.Document);
        }
      }
      else
      {
        // ****************************************************************
        // 09/25/01 T.Bobb  PR00127590 Get the latest effective document
        // ****************************************************************
        if (ReadDocument2())
        {
          local.Document.Assign(entities.Document);
        }
      }
    }

    if (IsEmpty(local.Document.Type1))
    {
      ExitState = "DOCUMENT_NF";

      return;
    }

    // ****************************************************************
    // 09/25/01 T.Bobb  PR00127590 Removed check for expiration date
    // ****************************************************************
    // mjr--> Override normal document type
    if (!IsEmpty(import.Document.Type1))
    {
      local.Document.Type1 = import.Document.Type1;
    }

    if (!ReadEventDetail())
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";

      return;
    }

    if (!ReadEvent())
    {
      ExitState = "SP0000_EVENT_NF";

      return;
    }

    // Infrastructure
    export.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    export.Infrastructure.SituationNumber = 1;
    export.Infrastructure.ReasonCode = entities.EventDetail.ReasonCode;
    export.Infrastructure.CsenetInOutCode =
      entities.EventDetail.CsenetInOutCode;
    export.Infrastructure.EventDetailName = entities.EventDetail.DetailName;
    export.Infrastructure.InitiatingStateCode =
      entities.EventDetail.InitiatingStateCode;
    export.Infrastructure.Function = entities.EventDetail.Function;
    export.Infrastructure.EventType = entities.Event1.Type1;
    export.Infrastructure.EventId = entities.Event1.ControlNumber;
    export.Infrastructure.BusinessObjectCd = entities.Event1.BusinessObjectCode;
    export.Infrastructure.ProcessStatus = "H";

    // mjr---> denorm identifiers on infrastructure
    export.Infrastructure.CaseNumber = import.SpDocKey.KeyCase;
    export.Infrastructure.DenormNumeric12 = import.SpDocKey.KeyLegalAction;

    if (!IsEmpty(import.SpDocKey.KeyAp))
    {
      export.Infrastructure.CsePersonNumber = import.SpDocKey.KeyAp;
    }
    else if (!IsEmpty(import.SpDocKey.KeyPerson))
    {
      export.Infrastructure.CsePersonNumber = import.SpDocKey.KeyPerson;
    }
    else if (!IsEmpty(import.SpDocKey.KeyAr))
    {
      export.Infrastructure.CsePersonNumber = import.SpDocKey.KeyAr;
    }
    else
    {
      export.Infrastructure.CsePersonNumber = import.SpDocKey.KeyChild;
    }

    if (Lt(local.Null1.Date, import.SpDocKey.KeyAdminActionCert))
    {
      export.Infrastructure.DenormDate = import.SpDocKey.KeyAdminActionCert;
    }
    else if (Lt(local.Null1.Date, import.SpDocKey.KeyMilitaryService))
    {
      export.Infrastructure.DenormDate = import.SpDocKey.KeyMilitaryService;
    }
    else
    {
      export.Infrastructure.DenormDate =
        import.SpDocKey.KeyObligationAdminAction;
    }

    if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyAppointment))
    {
      export.Infrastructure.DenormTimestamp = import.SpDocKey.KeyAppointment;
    }
    else if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyIncomeSource))
    {
      export.Infrastructure.DenormTimestamp = import.SpDocKey.KeyIncomeSource;
    }
    else if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyPersonAddress))
    {
      export.Infrastructure.DenormTimestamp = import.SpDocKey.KeyPersonAddress;
    }
    else
    {
      export.Infrastructure.DenormTimestamp = import.SpDocKey.KeyWorkerComp;
    }

    // mjr---> user_id determines on whose list this shows for HIST, DMON, and 
    // ODCM
    UseSpDocGetServiceProvider();

    if (local.OutDocRtrnAddr.OfficeSysGenId <= 0)
    {
      // mjr
      // ------------------------------------------
      // 06/24/2002
      // PR#148447
      // -------------------------------------------------------
      if (Equal(local.Document.BusinessObject, "OAA"))
      {
        local.Document.BusinessObject = "PER";
        UseSpDocGetServiceProvider();
      }
      else if (Equal(local.Document.Name, "POSTMAST"))
      {
        local.Document.BusinessObject = "OBL";
        UseSpDocGetServiceProvider();
      }
      else if (Equal(local.Document.Name, "CASETXFR"))
      {
        local.OutDocRtrnAddr.OfficeSysGenId = import.SpDocKey.KeyOffice;
      }
      else
      {
      }

      if (local.OutDocRtrnAddr.OfficeSysGenId <= 0)
      {
        ExitState = "OFFICE_NF";

        return;
      }
    }

    if (!IsEmpty(local.OutDocRtrnAddr.ServProvUserId))
    {
      export.Infrastructure.UserId = local.OutDocRtrnAddr.ServProvUserId;
    }
    else
    {
      export.Infrastructure.UserId = global.UserId;
    }

    if (Lt(local.Null1.Date, import.Infrastructure.ReferenceDate))
    {
      export.Infrastructure.ReferenceDate = import.Infrastructure.ReferenceDate;
    }
    else
    {
      export.Infrastructure.ReferenceDate = local.Current.Date;
    }

    if (!IsEmpty(import.Infrastructure.CreatedBy))
    {
      export.Infrastructure.CreatedBy = import.Infrastructure.CreatedBy;
    }
    else
    {
      export.Infrastructure.CreatedBy = global.UserId;
    }

    if (Lt(local.Null1.Timestamp, import.Infrastructure.CreatedTimestamp))
    {
      export.Infrastructure.CreatedTimestamp =
        import.Infrastructure.CreatedTimestamp;
      export.Infrastructure.LastUpdatedTimestamp = local.Current.Timestamp;
    }
    else
    {
      export.Infrastructure.CreatedTimestamp = local.Current.Timestamp;
      export.Infrastructure.LastUpdatedTimestamp = local.Current.Timestamp;
    }

    if (Equal(local.Document.Type1, "BTCH"))
    {
      export.Infrastructure.Detail = "Document will be generated in batch.";
    }
    else if (Equal(local.Document.Type1, "MAIL"))
    {
      export.Infrastructure.Detail = "Document will be generated via eMail.";
    }
    else
    {
      export.Infrastructure.Detail = "Document was not successfully printed.";
    }

    if (import.Infrastructure.SystemGeneratedIdentifier > 0)
    {
      UseSpCabUpdateInfrastructure();
    }
    else
    {
      UseSpCabCreateInfrastructure();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Outgoing Document
    // mjr
    // ---------------------------------------------------
    // 01/04/1998
    // BTCH  => G (processed by SWEPB709)
    // MAIL  => M (processed by SWEPDEML)
    // other => N (processed by SWEPDDOC)
    // ----------------------------------------------------------------
    // 07/30/2008	J Huss	Changed when PRNT_SUCESSFUL_IND is set.
    // 07/30/2008	J Huss	Set PRNT_SUCESSFUL_IND to 'N' initially so it won't be 
    // printed until processing has completed.
    local.OutgoingDocument.PrintSucessfulIndicator = "N";
    local.OutgoingDocument.CreatedBy = export.Infrastructure.CreatedBy;
    local.OutgoingDocument.CreatedTimestamp =
      export.Infrastructure.CreatedTimestamp;
    local.OutgoingDocument.LastUpdatedBy =
      export.Infrastructure.LastUpdatedBy ?? "";
    local.OutgoingDocument.LastUpdatdTstamp =
      export.Infrastructure.LastUpdatedTimestamp;

    if (import.Infrastructure.SystemGeneratedIdentifier > 0)
    {
      UseUpdateOutgoingDocument();
    }
    else
    {
      local.Office.SystemGeneratedId = local.OutDocRtrnAddr.OfficeSysGenId;
      UseCreateOutgoingDocument();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // mjr
    // ------------------------------------------------------
    // 01/20/1999
    // Create all Field_Values for available KEY fields
    // -------------------------------------------------------------------
    local.FieldValue.CreatedBy = export.Infrastructure.CreatedBy;
    local.FieldValue.CreatedTimestamp = export.Infrastructure.CreatedTimestamp;

    foreach(var item in ReadField())
    {
      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);

      switch(TrimEnd(entities.Field.Name))
      {
        case "IDAACERT":
          if (Lt(local.Null1.Date, import.SpDocKey.KeyAdminActionCert))
          {
            local.FieldValue.Value =
              NumberToString(DateToInt(import.SpDocKey.KeyAdminActionCert), 8, 8);
              
          }

          break;
        case "IDAAPPEAL":
          if (import.SpDocKey.KeyAdminAppeal > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyAdminAppeal, 7, 9);
          }

          break;
        case "IDADMINACT":
          if (!IsEmpty(import.SpDocKey.KeyAdminAction))
          {
            local.FieldValue.Value = import.SpDocKey.KeyAdminAction;
          }

          break;
        case "IDAP":
          if (!IsEmpty(import.SpDocKey.KeyAp))
          {
            local.FieldValue.Value = import.SpDocKey.KeyAp;
          }

          break;
        case "IDAPPT":
          if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyAppointment))
          {
            local.BatchTimestampWorkArea.IefTimestamp =
              import.SpDocKey.KeyAppointment;
            UseLeCabConvertTimestamp();
            local.FieldValue.Value = local.BatchTimestampWorkArea.TextTimestamp;
          }

          break;
        case "IDAR":
          if (!IsEmpty(import.SpDocKey.KeyAr))
          {
            local.FieldValue.Value = import.SpDocKey.KeyAr;
          }

          break;
        case "IDBANKRPTC":
          if (import.SpDocKey.KeyBankruptcy > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyBankruptcy, 13, 3);
          }

          break;
        case "IDCASHDETL":
          if (import.SpDocKey.KeyCashRcptDetail > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyCashRcptDetail, 12, 4);
          }

          break;
        case "IDCASHEVNT":
          if (import.SpDocKey.KeyCashRcptEvent > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyCashRcptEvent, 7, 9);
          }

          break;
        case "IDCASHSRCE":
          if (import.SpDocKey.KeyCashRcptSource > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyCashRcptSource, 13, 3);
          }

          break;
        case "IDCASHTYPE":
          if (import.SpDocKey.KeyCashRcptType > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyCashRcptType, 13, 3);
          }

          break;
        case "IDCHILD":
          if (!IsEmpty(import.SpDocKey.KeyChild))
          {
            local.FieldValue.Value = import.SpDocKey.KeyChild;
          }

          break;
        case "IDCONTACT":
          if (import.SpDocKey.KeyContact > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyContact, 14, 2);
          }

          break;
        case "IDCSECASE":
          if (!IsEmpty(import.SpDocKey.KeyCase))
          {
            local.FieldValue.Value = import.SpDocKey.KeyCase;
          }

          break;
        case "IDGENETIC":
          if (import.SpDocKey.KeyGeneticTest > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyGeneticTest, 8, 8);
          }

          break;
        case "IDHINSCOV":
          if (import.SpDocKey.KeyHealthInsCoverage > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyHealthInsCoverage, 6, 10);
          }

          break;
        case "IDINCARCER":
          if (import.SpDocKey.KeyIncarceration > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyIncarceration, 14, 2);
          }

          break;
        case "IDINCSOURC":
          if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyIncomeSource))
          {
            local.BatchTimestampWorkArea.IefTimestamp =
              import.SpDocKey.KeyIncomeSource;
            UseLeCabConvertTimestamp();
            local.FieldValue.Value = local.BatchTimestampWorkArea.TextTimestamp;
          }

          break;
        case "IDINFORQST":
          if (import.SpDocKey.KeyInfoRequest > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyInfoRequest, 6, 10);
          }

          break;
        case "IDINTSTREQ":
          if (import.SpDocKey.KeyInterstateRequest > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyInterstateRequest, 8, 8);
          }

          break;
        case "IDLEGALACT":
          if (import.SpDocKey.KeyLegalAction > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyLegalAction, 7, 9);
          }

          break;
        case "IDLEGALDTL":
          if (import.SpDocKey.KeyLegalActionDetail > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyLegalActionDetail, 14, 2);
          }

          break;
        case "IDLEGALREF":
          if (import.SpDocKey.KeyLegalReferral > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyLegalReferral, 13, 3);
          }

          break;
        case "IDLOCRQAGN":
          if (!IsEmpty(import.SpDocKey.KeyLocateRequestAgency))
          {
            local.FieldValue.Value = import.SpDocKey.KeyLocateRequestAgency;
          }

          break;
        case "IDLOCRQSRC":
          if (import.SpDocKey.KeyLocateRequestSource > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyLocateRequestSource, 14, 2);
          }

          break;
        case "IDMILITARY":
          if (Lt(local.Null1.Date, import.SpDocKey.KeyMilitaryService))
          {
            local.FieldValue.Value =
              NumberToString(DateToInt(import.SpDocKey.KeyMilitaryService), 8, 8);
              
          }

          break;
        case "IDOBLADACT":
          if (Lt(local.Null1.Date, import.SpDocKey.KeyObligationAdminAction))
          {
            local.FieldValue.Value =
              NumberToString(
                DateToInt(import.SpDocKey.KeyObligationAdminAction), 8, 8);
          }

          break;
        case "IDOBLIGATN":
          if (import.SpDocKey.KeyObligation > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyObligation, 13, 3);
          }

          break;
        case "IDOBLTYPE":
          if (import.SpDocKey.KeyObligationType > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyObligationType, 13, 3);
          }

          break;
        case "IDPERSACCT":
          if (!IsEmpty(import.SpDocKey.KeyPersonAccount))
          {
            local.FieldValue.Value = import.SpDocKey.KeyPersonAccount;
          }

          break;
        case "IDPERSADDR":
          if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyPersonAddress))
          {
            local.BatchTimestampWorkArea.IefTimestamp =
              import.SpDocKey.KeyPersonAddress;
            UseLeCabConvertTimestamp();
            local.FieldValue.Value = local.BatchTimestampWorkArea.TextTimestamp;
          }

          break;
        case "IDPERSON":
          if (!IsEmpty(import.SpDocKey.KeyPerson))
          {
            local.FieldValue.Value = import.SpDocKey.KeyPerson;
          }

          break;
        case "IDRCAPTURE":
          if (import.SpDocKey.KeyRecaptureRule > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyRecaptureRule, 7, 9);
          }

          break;
        case "IDRESOURCE":
          if (import.SpDocKey.KeyResource > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyResource, 13, 3);
          }

          break;
        case "IDTRIBUNAL":
          if (import.SpDocKey.KeyTribunal > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyTribunal, 7, 9);
          }

          break;
        case "IDWRKRCOMP":
          if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyWorkerComp))
          {
            local.BatchTimestampWorkArea.IefTimestamp =
              import.SpDocKey.KeyWorkerComp;
            UseLeCabConvertTimestamp();
            local.FieldValue.Value = local.BatchTimestampWorkArea.TextTimestamp;
          }

          break;
        case "IDWRKSHEET":
          if (import.SpDocKey.KeyWorksheet > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyWorksheet, 6, 10);
          }

          break;
        case "CASXFROFFC":
          if (import.SpDocKey.KeyOffice > 0)
          {
            local.FieldValue.Value =
              NumberToString(import.SpDocKey.KeyOffice, 12, 4);
          }

          break;
        case "CASXFRFRDT":
          local.FieldValue.Value =
            NumberToString(DateToInt(import.SpDocKey.KeyXferFromDate), 8, 8);

          break;
        case "CASXFRTODT":
          local.FieldValue.Value =
            NumberToString(DateToInt(import.SpDocKey.KeyXferToDate), 8, 8);

          break;
        default:
          break;
      }

      if (IsEmpty(local.FieldValue.Value))
      {
        continue;
      }

      UseSpCabCreateUpdateFieldValue();

      if (IsExitState("DOCUMENT_FIELD_NF_RB"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // 07/30/2008	J Huss	Set print_sucessful_indicator to proper status to allow
    // for printing.
    if (Equal(local.Document.Type1, "BTCH"))
    {
      local.OutgoingDocument.PrintSucessfulIndicator = "G";
    }
    else if (Equal(local.Document.Type1, "MAIL"))
    {
      local.OutgoingDocument.PrintSucessfulIndicator = "M";
    }
    else
    {
      local.OutgoingDocument.PrintSucessfulIndicator = "N";
    }

    // 07/30/2008	J Huss	Print_sucessful_indicator was set to 'N' originally, no
    // need to set it again.
    if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) != 'N')
    {
      UseUpdateOutgoingDocument();
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveFieldValue(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveInfrastructure(Infrastructure source,
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
  }

  private static void MoveOutDocRtrnAddr(OutDocRtrnAddr source,
    OutDocRtrnAddr target)
  {
    target.OfficeSysGenId = source.OfficeSysGenId;
    target.ServProvUserId = source.ServProvUserId;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCreateOutgoingDocument()
  {
    var useImport = new CreateOutgoingDocument.Import();
    var useExport = new CreateOutgoingDocument.Export();

    MoveDocument(local.Document, useImport.Document);
    useImport.OutgoingDocument.Assign(local.OutgoingDocument);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    useImport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;

    Call(CreateOutgoingDocument.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(export.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    export.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabCreateUpdateFieldValue()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = entities.Field.Name;
    MoveFieldValue(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    MoveInfrastructure(export.Infrastructure, useImport.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseSpDocGetServiceProvider()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.SpDocKey.Assign(import.SpDocKey);
    useImport.Document.BusinessObject = local.Document.BusinessObject;
    useImport.Current.Date = local.Current.Date;

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    MoveOutDocRtrnAddr(useExport.OutDocRtrnAddr, local.OutDocRtrnAddr);
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.OutgoingDocument.Assign(local.OutgoingDocument);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private bool ReadDocument1()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument1",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          import.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.BusinessObject = db.GetString(reader, 2);
        entities.Document.EveNo = db.GetNullableInt32(reader, 3);
        entities.Document.EvdId = db.GetNullableInt32(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.Document.ExpirationDate = db.GetDate(reader, 6);
        entities.Document.Populated = true;
      });
  }

  private bool ReadDocument2()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument2",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.BusinessObject = db.GetString(reader, 2);
        entities.Document.EveNo = db.GetNullableInt32(reader, 3);
        entities.Document.EvdId = db.GetNullableInt32(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.Document.ExpirationDate = db.GetDate(reader, 6);
        entities.Document.Populated = true;
      });
  }

  private bool ReadEvent()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", entities.EventDetail.EveNo);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Type1 = db.GetString(reader, 1);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 2);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetString(command, "name", local.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          local.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 2);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 5);
        entities.EventDetail.EveNo = db.GetInt32(reader, 6);
        entities.EventDetail.Function = db.GetNullableString(reader, 7);
        entities.EventDetail.Populated = true;
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
        entities.Field.Populated = true;

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

    private SpDocKey spDocKey;
    private Document document;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
    private Office office;
    private DateWorkArea current;
    private OutDocRtrnAddr outDocRtrnAddr;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private FieldValue fieldValue;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Document document;
    private OutgoingDocument outgoingDocument;
    private MonitoredDocument monitoredDocument;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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

    private DocumentField documentField;
    private Field field;
    private Document document;
    private Event1 event1;
    private EventDetail eventDetail;
  }
#endregion
}
