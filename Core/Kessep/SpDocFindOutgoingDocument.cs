// Program: SP_DOC_FIND_OUTGOING_DOCUMENT, ID: 372173563, model: 746.
// Short name: SWE02308
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
/// A program: SP_DOC_FIND_OUTGOING_DOCUMENT.
/// </para>
/// <para>
/// This action block finds an outgoing_document for a given document and group 
/// of sp_doc_keys.
/// Returned is the outgoing_doc print_suc_ind and the infrastructure 
/// sys_gen_id.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFindOutgoingDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FIND_OUTGOING_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFindOutgoingDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFindOutgoingDocument.
  /// </summary>
  public SpDocFindOutgoingDocument(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ------------------------------------------------------------------
    // 02/01/1999	M Ramirez			Initial development
    // ------------------------------------------------------------------
    if (IsEmpty(import.Document.Name))
    {
      ExitState = "DOCUMENT_NF";

      return;
    }

    // mjr
    // -----------------------------------------------------
    // Set each local_idxxxx field_value using import sp_doc_key
    // ----------------------------------------------------------
    // mjr
    // -----------------------------------------------------
    // Admin Action
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyAdminAction))
    {
      local.Idadminact.Value = import.SpDocKey.KeyAdminAction;
    }

    // mjr
    // -----------------------------------------------------
    // Admin Action Cert
    // --------------------------------------------------------
    if (Lt(local.Null1.Date, import.SpDocKey.KeyAdminActionCert))
    {
      local.Idaacert.Value =
        NumberToString(DateToInt(import.SpDocKey.KeyAdminActionCert), 8, 8);
    }

    // mjr
    // -----------------------------------------------------
    // Admin Appeal
    // --------------------------------------------------------
    if (import.SpDocKey.KeyAdminAppeal > 0)
    {
      local.Idaappeal.Value =
        NumberToString(import.SpDocKey.KeyAdminAppeal, 7, 9);
    }

    // mjr
    // -----------------------------------------------------
    // Appointment
    // --------------------------------------------------------
    if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyAppointment))
    {
      local.BatchTimestampWorkArea.IefTimestamp =
        import.SpDocKey.KeyAppointment;
      UseLeCabConvertTimestamp();
      local.Idappt.Value = local.BatchTimestampWorkArea.TextTimestamp;
    }

    // mjr
    // -----------------------------------------------------
    // Bankruptcy
    // --------------------------------------------------------
    if (import.SpDocKey.KeyBankruptcy > 0)
    {
      local.Idbankrptc.Value =
        NumberToString(import.SpDocKey.KeyBankruptcy, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Case
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyCase))
    {
      local.Idcsecase.Value = import.SpDocKey.KeyCase;
    }

    // mjr
    // -----------------------------------------------------
    // AP Person Number
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyAp))
    {
      local.Idap.Value = import.SpDocKey.KeyAp;
    }

    // mjr
    // -----------------------------------------------------
    // AR Person Number
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyAr))
    {
      local.Idar.Value = import.SpDocKey.KeyAr;
    }

    // mjr
    // -----------------------------------------------------
    // CH Person Number
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyChild))
    {
      local.Idchild.Value = import.SpDocKey.KeyChild;
    }

    // mjr
    // -----------------------------------------------------
    // PR Person Number
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyPerson))
    {
      local.Idperson.Value = import.SpDocKey.KeyPerson;
    }

    // mjr
    // -----------------------------------------------------
    // Cash Receipt Detail
    // --------------------------------------------------------
    if (import.SpDocKey.KeyCashRcptDetail > 0)
    {
      local.Idcashdetl.Value =
        NumberToString(import.SpDocKey.KeyCashRcptDetail, 12, 4);
    }

    // mjr
    // -----------------------------------------------------
    // Cash Receipt Event
    // --------------------------------------------------------
    if (import.SpDocKey.KeyCashRcptEvent > 0)
    {
      local.Idcashevnt.Value =
        NumberToString(import.SpDocKey.KeyCashRcptEvent, 7, 9);
    }

    // mjr
    // -----------------------------------------------------
    // Cash Receipt Source
    // --------------------------------------------------------
    if (import.SpDocKey.KeyCashRcptSource > 0)
    {
      local.Idcashsrce.Value =
        NumberToString(import.SpDocKey.KeyCashRcptSource, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Cash Receipt Type
    // --------------------------------------------------------
    if (import.SpDocKey.KeyCashRcptType > 0)
    {
      local.Idcashtype.Value =
        NumberToString(import.SpDocKey.KeyCashRcptType, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Contact
    // --------------------------------------------------------
    if (import.SpDocKey.KeyContact > 0)
    {
      local.Idcontact.Value = NumberToString(import.SpDocKey.KeyContact, 14, 2);
    }

    // mjr
    // -----------------------------------------------------
    // Genetic Test
    // --------------------------------------------------------
    if (import.SpDocKey.KeyGeneticTest > 0)
    {
      local.Idgenetic.Value =
        NumberToString(import.SpDocKey.KeyGeneticTest, 8, 8);
    }

    // mjr
    // -----------------------------------------------------
    // Health Insurance Coverage
    // --------------------------------------------------------
    if (import.SpDocKey.KeyHealthInsCoverage > 0)
    {
      local.Idhinscov.Value =
        NumberToString(import.SpDocKey.KeyHealthInsCoverage, 6, 10);
    }

    // mjr
    // -----------------------------------------------------
    // Income Source
    // --------------------------------------------------------
    if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyIncomeSource))
    {
      local.BatchTimestampWorkArea.IefTimestamp =
        import.SpDocKey.KeyIncomeSource;
      UseLeCabConvertTimestamp();
      local.Idincsourc.Value = local.BatchTimestampWorkArea.TextTimestamp;
    }

    // mjr
    // -----------------------------------------------------
    // Information Request
    // --------------------------------------------------------
    if (import.SpDocKey.KeyInfoRequest > 0)
    {
      local.Idinforqst.Value =
        NumberToString(import.SpDocKey.KeyInfoRequest, 6, 10);
    }

    // mjr
    // -----------------------------------------------------
    // Interstate Request
    // --------------------------------------------------------
    if (import.SpDocKey.KeyInterstateRequest > 0)
    {
      local.Idintstreq.Value =
        NumberToString(import.SpDocKey.KeyInterstateRequest, 8, 8);
    }

    // mjr
    // -----------------------------------------------------
    // Jail
    // --------------------------------------------------------
    if (import.SpDocKey.KeyIncarceration > 0)
    {
      local.Idincarcer.Value =
        NumberToString(import.SpDocKey.KeyIncarceration, 14, 2);
    }

    // mjr
    // -----------------------------------------------------
    // Legal Action
    // --------------------------------------------------------
    if (import.SpDocKey.KeyLegalAction > 0)
    {
      local.Idlegalact.Value =
        NumberToString(import.SpDocKey.KeyLegalAction, 7, 9);
    }

    // mjr
    // -----------------------------------------------------
    // Legal Action Detail
    // --------------------------------------------------------
    if (import.SpDocKey.KeyLegalActionDetail > 0)
    {
      local.Idlegaldtl.Value =
        NumberToString(import.SpDocKey.KeyLegalActionDetail, 14, 2);
    }

    // mjr
    // -----------------------------------------------------
    // Locate Request Agency
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyLocateRequestAgency))
    {
      local.Idlocrqagn.Value = import.SpDocKey.KeyLocateRequestAgency;
    }

    // mjr
    // -----------------------------------------------------
    // Locate Request Source
    // --------------------------------------------------------
    if (import.SpDocKey.KeyLocateRequestSource > 0)
    {
      local.Idlocrqsrc.Value =
        NumberToString(import.SpDocKey.KeyLocateRequestSource, 14, 2);
    }

    // mjr
    // -----------------------------------------------------
    // Legal Referral
    // --------------------------------------------------------
    if (import.SpDocKey.KeyLegalReferral > 0)
    {
      local.Idlegalref.Value =
        NumberToString(import.SpDocKey.KeyLegalReferral, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Military
    // --------------------------------------------------------
    if (Lt(local.Null1.Date, import.SpDocKey.KeyMilitaryService))
    {
      local.Idmilitary.Value =
        NumberToString(DateToInt(import.SpDocKey.KeyMilitaryService), 8, 8);
    }

    // mjr
    // -----------------------------------------------------
    // Obligation
    // --------------------------------------------------------
    if (import.SpDocKey.KeyObligation > 0)
    {
      local.Idobligatn.Value =
        NumberToString(import.SpDocKey.KeyObligation, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Obligation Admin Action
    // --------------------------------------------------------
    if (Lt(local.Null1.Date, import.SpDocKey.KeyObligationAdminAction))
    {
      local.Idobladact.Value =
        NumberToString(DateToInt(import.SpDocKey.KeyObligationAdminAction), 8, 8);
        
    }

    // mjr
    // -----------------------------------------------------
    // Obligation Type
    // --------------------------------------------------------
    if (import.SpDocKey.KeyObligationType > 0)
    {
      local.Idobltype.Value =
        NumberToString(import.SpDocKey.KeyObligationType, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Person Account (Obligor, Obligee, Supported)
    // --------------------------------------------------------
    if (!IsEmpty(import.SpDocKey.KeyPersonAccount))
    {
      local.Idpersacct.Value = import.SpDocKey.KeyPersonAccount;
    }

    // mjr
    // -----------------------------------------------------
    // Person Address
    // --------------------------------------------------------
    if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyPersonAddress))
    {
      local.BatchTimestampWorkArea.IefTimestamp =
        import.SpDocKey.KeyPersonAddress;
      UseLeCabConvertTimestamp();
      local.Idpersaddr.Value = local.BatchTimestampWorkArea.TextTimestamp;
    }

    // mjr
    // -----------------------------------------------------
    // Recapture Rule
    // --------------------------------------------------------
    if (import.SpDocKey.KeyRecaptureRule > 0)
    {
      local.Idrcapture.Value =
        NumberToString(import.SpDocKey.KeyRecaptureRule, 7, 9);
    }

    // mjr
    // -----------------------------------------------------
    // Resource
    // --------------------------------------------------------
    if (import.SpDocKey.KeyResource > 0)
    {
      local.Idresource.Value =
        NumberToString(import.SpDocKey.KeyResource, 13, 3);
    }

    // mjr
    // -----------------------------------------------------
    // Tribunal
    // --------------------------------------------------------
    if (import.SpDocKey.KeyTribunal > 0)
    {
      local.Idtribunal.Value =
        NumberToString(import.SpDocKey.KeyTribunal, 7, 9);
    }

    // mjr
    // -----------------------------------------------------
    // Worker Comp Insurance (Income Source)
    // --------------------------------------------------------
    if (Lt(local.Null1.Timestamp, import.SpDocKey.KeyWorkerComp))
    {
      local.BatchTimestampWorkArea.IefTimestamp = import.SpDocKey.KeyWorkerComp;
      UseLeCabConvertTimestamp();
      local.Idwrkrcomp.Value = local.BatchTimestampWorkArea.TextTimestamp;
    }

    // mjr
    // -----------------------------------------------------
    // Worksheet
    // --------------------------------------------------------
    if (import.SpDocKey.KeyWorksheet > 0)
    {
      local.Idwrksheet.Value =
        NumberToString(import.SpDocKey.KeyWorksheet, 6, 10);
    }

    // mjr
    // -----------------------------------------------------
    // Get each document (whether expired or not)
    // ----------------------------------------------------------
    foreach(var item in ReadDocument())
    {
      // mjr
      // -----------------------------------------------------------
      // Construct a group of the fields on the current document
      // --------------------------------------------------------------
      local.Local1.Count = 0;
      local.Local1.Index = -1;

      foreach(var item1 in ReadDocumentFieldField())
      {
        ++local.Local1.Index;
        local.Local1.CheckSize();

        local.Local1.Update.G.Name = entities.Field.Name;
      }

      local.Local1.Index = 0;
      local.Local1.CheckSize();

      switch(TrimEnd(local.Local1.Item.G.Name))
      {
        case "IDAACERT":
          local.Current.Value = local.Idaacert.Value ?? "";

          break;
        case "IDAAPPEAL":
          local.Current.Value = local.Idaappeal.Value ?? "";

          break;
        case "IDADMINACT":
          local.Current.Value = local.Idadminact.Value ?? "";

          break;
        case "IDAP":
          local.Current.Value = local.Idap.Value ?? "";

          break;
        case "IDAPPT":
          local.Current.Value = local.Idappt.Value ?? "";

          break;
        case "IDAR":
          local.Current.Value = local.Idar.Value ?? "";

          break;
        case "IDBANKRPTC":
          local.Current.Value = local.Idbankrptc.Value ?? "";

          break;
        case "IDCASHDETL":
          local.Current.Value = local.Idcashdetl.Value ?? "";

          break;
        case "IDCASHEVNT":
          local.Current.Value = local.Idcashevnt.Value ?? "";

          break;
        case "IDCASHSRCE":
          local.Current.Value = local.Idcashsrce.Value ?? "";

          break;
        case "IDCASHTYPE":
          local.Current.Value = local.Idcashtype.Value ?? "";

          break;
        case "IDCHILD":
          local.Current.Value = local.Idchild.Value ?? "";

          break;
        case "IDCONTACT":
          local.Current.Value = local.Idcontact.Value ?? "";

          break;
        case "IDCSECASE":
          local.Current.Value = local.Idcsecase.Value ?? "";

          break;
        case "IDGENETIC":
          local.Current.Value = local.Idgenetic.Value ?? "";

          break;
        case "IDHINSCOV":
          local.Current.Value = local.Idhinscov.Value ?? "";

          break;
        case "IDINCARCER":
          local.Current.Value = local.Idincarcer.Value ?? "";

          break;
        case "IDINCSOURC":
          local.Current.Value = local.Idincsourc.Value ?? "";

          break;
        case "IDINFORQST":
          local.Current.Value = local.Idinforqst.Value ?? "";

          break;
        case "IDINTSTREQ":
          local.Current.Value = local.Idintstreq.Value ?? "";

          break;
        case "IDLEGALACT":
          local.Current.Value = local.Idlegalact.Value ?? "";

          break;
        case "IDLEGALDTL":
          local.Current.Value = local.Idlegaldtl.Value ?? "";

          break;
        case "IDLEGALREF":
          local.Current.Value = local.Idlegalref.Value ?? "";

          break;
        case "IDLOCRQAGN":
          local.Current.Value = local.Idlocrqagn.Value ?? "";

          break;
        case "IDLOCRQSRC":
          local.Current.Value = local.Idlocrqsrc.Value ?? "";

          break;
        case "IDMILITARY":
          local.Current.Value = local.Idmilitary.Value ?? "";

          break;
        case "IDOBLADACT":
          local.Current.Value = local.Idobladact.Value ?? "";

          break;
        case "IDOBLIGATN":
          local.Current.Value = local.Idobligatn.Value ?? "";

          break;
        case "IDOBLTYPE":
          local.Current.Value = local.Idobltype.Value ?? "";

          break;
        case "IDPERSACCT":
          local.Current.Value = local.Idpersacct.Value ?? "";

          break;
        case "IDPERSADDR":
          local.Current.Value = local.Idpersaddr.Value ?? "";

          break;
        case "IDPERSON":
          local.Current.Value = local.Idperson.Value ?? "";

          break;
        case "IDRCAPTURE":
          local.Current.Value = local.Idrcapture.Value ?? "";

          break;
        case "IDRESOURCE":
          local.Current.Value = local.Idresource.Value ?? "";

          break;
        case "IDTRIBUNAL":
          local.Current.Value = local.Idtribunal.Value ?? "";

          break;
        case "IDWRKRCOMP":
          local.Current.Value = local.Idwrkrcomp.Value ?? "";

          break;
        case "IDWRKSHEET":
          local.Current.Value = local.Idwrksheet.Value ?? "";

          break;
        default:
          local.Current.Value = Spaces(FieldValue.Value_MaxLength);

          break;
      }

      foreach(var item1 in ReadOutgoingDocumentInfrastructure())
      {
        if (local.Local1.Count > 1)
        {
          // There are more than one attributes to qualify
          for(local.Local1.Index = 1; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            switch(TrimEnd(local.Local1.Item.G.Name))
            {
              case "IDAACERT":
                local.Current.Value = local.Idaacert.Value ?? "";

                break;
              case "IDAAPPEAL":
                local.Current.Value = local.Idaappeal.Value ?? "";

                break;
              case "IDADMINACT":
                local.Current.Value = local.Idadminact.Value ?? "";

                break;
              case "IDAP":
                local.Current.Value = local.Idap.Value ?? "";

                break;
              case "IDAPPT":
                local.Current.Value = local.Idappt.Value ?? "";

                break;
              case "IDAR":
                local.Current.Value = local.Idar.Value ?? "";

                break;
              case "IDBANKRPTC":
                local.Current.Value = local.Idbankrptc.Value ?? "";

                break;
              case "IDCASHDETL":
                local.Current.Value = local.Idcashdetl.Value ?? "";

                break;
              case "IDCASHEVNT":
                local.Current.Value = local.Idcashevnt.Value ?? "";

                break;
              case "IDCASHSRCE":
                local.Current.Value = local.Idcashsrce.Value ?? "";

                break;
              case "IDCASHTYPE":
                local.Current.Value = local.Idcashtype.Value ?? "";

                break;
              case "IDCHILD":
                local.Current.Value = local.Idchild.Value ?? "";

                break;
              case "IDCONTACT":
                local.Current.Value = local.Idcontact.Value ?? "";

                break;
              case "IDCSECASE":
                local.Current.Value = local.Idcsecase.Value ?? "";

                break;
              case "IDGENETIC":
                local.Current.Value = local.Idgenetic.Value ?? "";

                break;
              case "IDHINSCOV":
                local.Current.Value = local.Idhinscov.Value ?? "";

                break;
              case "IDINCARCER":
                local.Current.Value = local.Idincarcer.Value ?? "";

                break;
              case "IDINCSOURC":
                local.Current.Value = local.Idincsourc.Value ?? "";

                break;
              case "IDINFORQST":
                local.Current.Value = local.Idinforqst.Value ?? "";

                break;
              case "IDINTSTREQ":
                local.Current.Value = local.Idintstreq.Value ?? "";

                break;
              case "IDLEGALACT":
                local.Current.Value = local.Idlegalact.Value ?? "";

                break;
              case "IDLEGALDTL":
                local.Current.Value = local.Idlegaldtl.Value ?? "";

                break;
              case "IDLEGALREF":
                local.Current.Value = local.Idlegalref.Value ?? "";

                break;
              case "IDLOCRQAGN":
                local.Current.Value = local.Idlocrqagn.Value ?? "";

                break;
              case "IDLOCRQSRC":
                local.Current.Value = local.Idlocrqsrc.Value ?? "";

                break;
              case "IDMILITARY":
                local.Current.Value = local.Idmilitary.Value ?? "";

                break;
              case "IDOBLADACT":
                local.Current.Value = local.Idobladact.Value ?? "";

                break;
              case "IDOBLIGATN":
                local.Current.Value = local.Idobligatn.Value ?? "";

                break;
              case "IDOBLTYPE":
                local.Current.Value = local.Idobltype.Value ?? "";

                break;
              case "IDPERSACCT":
                local.Current.Value = local.Idpersacct.Value ?? "";

                break;
              case "IDPERSADDR":
                local.Current.Value = local.Idpersaddr.Value ?? "";

                break;
              case "IDPERSON":
                local.Current.Value = local.Idperson.Value ?? "";

                break;
              case "IDRCAPTURE":
                local.Current.Value = local.Idrcapture.Value ?? "";

                break;
              case "IDRESOURCE":
                local.Current.Value = local.Idresource.Value ?? "";

                break;
              case "IDTRIBUNAL":
                local.Current.Value = local.Idtribunal.Value ?? "";

                break;
              case "IDWRKRCOMP":
                local.Current.Value = local.Idwrkrcomp.Value ?? "";

                break;
              case "IDWRKSHEET":
                local.Current.Value = local.Idwrksheet.Value ?? "";

                break;
              default:
                local.Current.Value = Spaces(FieldValue.Value_MaxLength);

                break;
            }

            if (!ReadFieldValue())
            {
              // mjr
              // -----------------------------------------------
              // This outgoing_document does not match
              // --------------------------------------------------
              goto ReadEach1;
            }
          }

          local.Local1.CheckIndex();
        }

        // mjr
        // -----------------------------------------------------------
        // All ' KEY' fields on outgoing_document match import sp_doc_key 
        // fields.
        // --------------------------------------------------------------
        export.Infrastructure.SystemGeneratedIdentifier =
          entities.Infrastructure.SystemGeneratedIdentifier;
        export.OutgoingDocument.PrintSucessfulIndicator =
          entities.OutgoingDocument.PrintSucessfulIndicator;

        goto ReadEach2;

ReadEach1:
        ;
      }
    }

ReadEach2:

    if (IsEmpty(entities.Document.Name))
    {
      ExitState = "DOCUMENT_NF";
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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

  private IEnumerable<bool> ReadDocument()
  {
    entities.Document.Populated = false;

    return ReadEach("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.BusinessObject = db.GetString(reader, 2);
        entities.Document.EffectiveDate = db.GetDate(reader, 3);
        entities.Document.ExpirationDate = db.GetDate(reader, 4);
        entities.Document.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocumentFieldField()
  {
    entities.Field.Populated = false;
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentFieldField",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.Field.Name = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.Field.Dependancy = db.GetString(reader, 4);
        entities.Field.Populated = true;
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetNullableString(command, "valu", local.Current.Value ?? "");
        db.SetString(command, "fldName", local.Local1.Item.G.Name);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructure()
  {
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructure",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetNullableString(command, "docName", entities.Document.Name);
        db.SetNullableString(command, "valu", local.Current.Value ?? "");
        db.SetString(command, "fldName", local.Local1.Item.G.Name);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 1);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 2);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 4);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
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

    private SpDocKey spDocKey;
    private Document document;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Field G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Field g;
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
    /// A value of Idaacert.
    /// </summary>
    [JsonPropertyName("idaacert")]
    public FieldValue Idaacert
    {
      get => idaacert ??= new();
      set => idaacert = value;
    }

    /// <summary>
    /// A value of Idaappeal.
    /// </summary>
    [JsonPropertyName("idaappeal")]
    public FieldValue Idaappeal
    {
      get => idaappeal ??= new();
      set => idaappeal = value;
    }

    /// <summary>
    /// A value of Idadminact.
    /// </summary>
    [JsonPropertyName("idadminact")]
    public FieldValue Idadminact
    {
      get => idadminact ??= new();
      set => idadminact = value;
    }

    /// <summary>
    /// A value of Idap.
    /// </summary>
    [JsonPropertyName("idap")]
    public FieldValue Idap
    {
      get => idap ??= new();
      set => idap = value;
    }

    /// <summary>
    /// A value of Idappt.
    /// </summary>
    [JsonPropertyName("idappt")]
    public FieldValue Idappt
    {
      get => idappt ??= new();
      set => idappt = value;
    }

    /// <summary>
    /// A value of Idar.
    /// </summary>
    [JsonPropertyName("idar")]
    public FieldValue Idar
    {
      get => idar ??= new();
      set => idar = value;
    }

    /// <summary>
    /// A value of Idbankrptc.
    /// </summary>
    [JsonPropertyName("idbankrptc")]
    public FieldValue Idbankrptc
    {
      get => idbankrptc ??= new();
      set => idbankrptc = value;
    }

    /// <summary>
    /// A value of Idcashdetl.
    /// </summary>
    [JsonPropertyName("idcashdetl")]
    public FieldValue Idcashdetl
    {
      get => idcashdetl ??= new();
      set => idcashdetl = value;
    }

    /// <summary>
    /// A value of Idcashevnt.
    /// </summary>
    [JsonPropertyName("idcashevnt")]
    public FieldValue Idcashevnt
    {
      get => idcashevnt ??= new();
      set => idcashevnt = value;
    }

    /// <summary>
    /// A value of Idcashsrce.
    /// </summary>
    [JsonPropertyName("idcashsrce")]
    public FieldValue Idcashsrce
    {
      get => idcashsrce ??= new();
      set => idcashsrce = value;
    }

    /// <summary>
    /// A value of Idcashtype.
    /// </summary>
    [JsonPropertyName("idcashtype")]
    public FieldValue Idcashtype
    {
      get => idcashtype ??= new();
      set => idcashtype = value;
    }

    /// <summary>
    /// A value of Idchild.
    /// </summary>
    [JsonPropertyName("idchild")]
    public FieldValue Idchild
    {
      get => idchild ??= new();
      set => idchild = value;
    }

    /// <summary>
    /// A value of Idcontact.
    /// </summary>
    [JsonPropertyName("idcontact")]
    public FieldValue Idcontact
    {
      get => idcontact ??= new();
      set => idcontact = value;
    }

    /// <summary>
    /// A value of Idcsecase.
    /// </summary>
    [JsonPropertyName("idcsecase")]
    public FieldValue Idcsecase
    {
      get => idcsecase ??= new();
      set => idcsecase = value;
    }

    /// <summary>
    /// A value of Idgenetic.
    /// </summary>
    [JsonPropertyName("idgenetic")]
    public FieldValue Idgenetic
    {
      get => idgenetic ??= new();
      set => idgenetic = value;
    }

    /// <summary>
    /// A value of Idhinscov.
    /// </summary>
    [JsonPropertyName("idhinscov")]
    public FieldValue Idhinscov
    {
      get => idhinscov ??= new();
      set => idhinscov = value;
    }

    /// <summary>
    /// A value of Idincarcer.
    /// </summary>
    [JsonPropertyName("idincarcer")]
    public FieldValue Idincarcer
    {
      get => idincarcer ??= new();
      set => idincarcer = value;
    }

    /// <summary>
    /// A value of Idincsourc.
    /// </summary>
    [JsonPropertyName("idincsourc")]
    public FieldValue Idincsourc
    {
      get => idincsourc ??= new();
      set => idincsourc = value;
    }

    /// <summary>
    /// A value of Idinforqst.
    /// </summary>
    [JsonPropertyName("idinforqst")]
    public FieldValue Idinforqst
    {
      get => idinforqst ??= new();
      set => idinforqst = value;
    }

    /// <summary>
    /// A value of Idintstreq.
    /// </summary>
    [JsonPropertyName("idintstreq")]
    public FieldValue Idintstreq
    {
      get => idintstreq ??= new();
      set => idintstreq = value;
    }

    /// <summary>
    /// A value of Idlegalact.
    /// </summary>
    [JsonPropertyName("idlegalact")]
    public FieldValue Idlegalact
    {
      get => idlegalact ??= new();
      set => idlegalact = value;
    }

    /// <summary>
    /// A value of Idlegaldtl.
    /// </summary>
    [JsonPropertyName("idlegaldtl")]
    public FieldValue Idlegaldtl
    {
      get => idlegaldtl ??= new();
      set => idlegaldtl = value;
    }

    /// <summary>
    /// A value of Idlegalref.
    /// </summary>
    [JsonPropertyName("idlegalref")]
    public FieldValue Idlegalref
    {
      get => idlegalref ??= new();
      set => idlegalref = value;
    }

    /// <summary>
    /// A value of Idlocrqagn.
    /// </summary>
    [JsonPropertyName("idlocrqagn")]
    public FieldValue Idlocrqagn
    {
      get => idlocrqagn ??= new();
      set => idlocrqagn = value;
    }

    /// <summary>
    /// A value of Idlocrqsrc.
    /// </summary>
    [JsonPropertyName("idlocrqsrc")]
    public FieldValue Idlocrqsrc
    {
      get => idlocrqsrc ??= new();
      set => idlocrqsrc = value;
    }

    /// <summary>
    /// A value of Idmilitary.
    /// </summary>
    [JsonPropertyName("idmilitary")]
    public FieldValue Idmilitary
    {
      get => idmilitary ??= new();
      set => idmilitary = value;
    }

    /// <summary>
    /// A value of Idobladact.
    /// </summary>
    [JsonPropertyName("idobladact")]
    public FieldValue Idobladact
    {
      get => idobladact ??= new();
      set => idobladact = value;
    }

    /// <summary>
    /// A value of Idobligatn.
    /// </summary>
    [JsonPropertyName("idobligatn")]
    public FieldValue Idobligatn
    {
      get => idobligatn ??= new();
      set => idobligatn = value;
    }

    /// <summary>
    /// A value of Idobltype.
    /// </summary>
    [JsonPropertyName("idobltype")]
    public FieldValue Idobltype
    {
      get => idobltype ??= new();
      set => idobltype = value;
    }

    /// <summary>
    /// A value of Idpersacct.
    /// </summary>
    [JsonPropertyName("idpersacct")]
    public FieldValue Idpersacct
    {
      get => idpersacct ??= new();
      set => idpersacct = value;
    }

    /// <summary>
    /// A value of Idpersaddr.
    /// </summary>
    [JsonPropertyName("idpersaddr")]
    public FieldValue Idpersaddr
    {
      get => idpersaddr ??= new();
      set => idpersaddr = value;
    }

    /// <summary>
    /// A value of Idperson.
    /// </summary>
    [JsonPropertyName("idperson")]
    public FieldValue Idperson
    {
      get => idperson ??= new();
      set => idperson = value;
    }

    /// <summary>
    /// A value of Idrcapture.
    /// </summary>
    [JsonPropertyName("idrcapture")]
    public FieldValue Idrcapture
    {
      get => idrcapture ??= new();
      set => idrcapture = value;
    }

    /// <summary>
    /// A value of Idresource.
    /// </summary>
    [JsonPropertyName("idresource")]
    public FieldValue Idresource
    {
      get => idresource ??= new();
      set => idresource = value;
    }

    /// <summary>
    /// A value of Idtribunal.
    /// </summary>
    [JsonPropertyName("idtribunal")]
    public FieldValue Idtribunal
    {
      get => idtribunal ??= new();
      set => idtribunal = value;
    }

    /// <summary>
    /// A value of Idwrkrcomp.
    /// </summary>
    [JsonPropertyName("idwrkrcomp")]
    public FieldValue Idwrkrcomp
    {
      get => idwrkrcomp ??= new();
      set => idwrkrcomp = value;
    }

    /// <summary>
    /// A value of Idwrksheet.
    /// </summary>
    [JsonPropertyName("idwrksheet")]
    public FieldValue Idwrksheet
    {
      get => idwrksheet ??= new();
      set => idwrksheet = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public FieldValue Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    /// <summary>
    /// A value of ZdelLocalMax.
    /// </summary>
    [JsonPropertyName("zdelLocalMax")]
    public DateWorkArea ZdelLocalMax
    {
      get => zdelLocalMax ??= new();
      set => zdelLocalMax = value;
    }

    /// <summary>
    /// A value of ZdelLocalMax2.
    /// </summary>
    [JsonPropertyName("zdelLocalMax2")]
    public DateWorkArea ZdelLocalMax2
    {
      get => zdelLocalMax2 ??= new();
      set => zdelLocalMax2 = value;
    }

    /// <summary>
    /// A value of ZdelLocalEnd.
    /// </summary>
    [JsonPropertyName("zdelLocalEnd")]
    public Infrastructure ZdelLocalEnd
    {
      get => zdelLocalEnd ??= new();
      set => zdelLocalEnd = value;
    }

    /// <summary>
    /// A value of ZdelLocalStart.
    /// </summary>
    [JsonPropertyName("zdelLocalStart")]
    public Infrastructure ZdelLocalStart
    {
      get => zdelLocalStart ??= new();
      set => zdelLocalStart = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private FieldValue idaacert;
    private FieldValue idaappeal;
    private FieldValue idadminact;
    private FieldValue idap;
    private FieldValue idappt;
    private FieldValue idar;
    private FieldValue idbankrptc;
    private FieldValue idcashdetl;
    private FieldValue idcashevnt;
    private FieldValue idcashsrce;
    private FieldValue idcashtype;
    private FieldValue idchild;
    private FieldValue idcontact;
    private FieldValue idcsecase;
    private FieldValue idgenetic;
    private FieldValue idhinscov;
    private FieldValue idincarcer;
    private FieldValue idincsourc;
    private FieldValue idinforqst;
    private FieldValue idintstreq;
    private FieldValue idlegalact;
    private FieldValue idlegaldtl;
    private FieldValue idlegalref;
    private FieldValue idlocrqagn;
    private FieldValue idlocrqsrc;
    private FieldValue idmilitary;
    private FieldValue idobladact;
    private FieldValue idobligatn;
    private FieldValue idobltype;
    private FieldValue idpersacct;
    private FieldValue idpersaddr;
    private FieldValue idperson;
    private FieldValue idrcapture;
    private FieldValue idresource;
    private FieldValue idtribunal;
    private FieldValue idwrkrcomp;
    private FieldValue idwrksheet;
    private FieldValue current;
    private Array<LocalGroup> local1;
    private Document document;
    private DateWorkArea null1;
    private DateWorkArea zdelLocalCurrent;
    private DateWorkArea zdelLocalMax;
    private DateWorkArea zdelLocalMax2;
    private Infrastructure zdelLocalEnd;
    private Infrastructure zdelLocalStart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ZdelArchivedOutgoingDocument.
    /// </summary>
    [JsonPropertyName("zdelArchivedOutgoingDocument")]
    public OutgoingDocument ZdelArchivedOutgoingDocument
    {
      get => zdelArchivedOutgoingDocument ??= new();
      set => zdelArchivedOutgoingDocument = value;
    }

    /// <summary>
    /// A value of ZdelArchivedInfrastructure.
    /// </summary>
    [JsonPropertyName("zdelArchivedInfrastructure")]
    public Infrastructure ZdelArchivedInfrastructure
    {
      get => zdelArchivedInfrastructure ??= new();
      set => zdelArchivedInfrastructure = value;
    }

    /// <summary>
    /// A value of ZdelRecordedDocument.
    /// </summary>
    [JsonPropertyName("zdelRecordedDocument")]
    public ZdelRecordedDocument ZdelRecordedDocument
    {
      get => zdelRecordedDocument ??= new();
      set => zdelRecordedDocument = value;
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

    private OutgoingDocument zdelArchivedOutgoingDocument;
    private Infrastructure zdelArchivedInfrastructure;
    private ZdelRecordedDocument zdelRecordedDocument;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private FieldValue fieldValue;
    private Field field;
    private DocumentField documentField;
    private Document document;
  }
#endregion
}
