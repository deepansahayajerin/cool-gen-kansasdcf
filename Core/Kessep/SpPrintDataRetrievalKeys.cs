// Program: SP_PRINT_DATA_RETRIEVAL_KEYS, ID: 372132893, model: 746.
// Short name: SWE02296
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_KEYS.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalKeys: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_KEYS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalKeys(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalKeys.
  /// </summary>
  public SpPrintDataRetrievalKeys(IContext context, Import import, Export export)
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
    // 01/13/1999	M Ramirez	Initial Development
    // 07/14/1999	M Ramirez	Added row lock counts
    // 11/17/2011	G Vandy		Added support for CASETXFR key fields
    // 				(CASXFROFFC, CASXFRFRDT, CASXFRTODT).
    // ----------------------------------------------------------------------
    if (!ReadOutgoingDocument())
    {
      ExitState = "OUTGOING_DOCUMENT_NF";

      return;
    }

    foreach(var item in ReadFieldDocumentField())
    {
      if (ReadFieldValue())
      {
        ++import.ExpImpRowLockFieldValue.Count;
        local.FieldValue.Value = entities.FieldValue.Value;
      }
      else
      {
        local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      }

      if (IsEmpty(local.FieldValue.Value))
      {
        continue;
      }

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case "DATE":
          local.Verify.Count = Verify(local.FieldValue.Value, "0123456789");

          if (local.Verify.Count > 0)
          {
            continue;
          }

          local.DateWorkArea.Date =
            IntToDate((int)StringToNumber(TrimEnd(local.FieldValue.Value)));

          switch(TrimEnd(entities.Field.Name))
          {
            case "IDAACERT":
              export.SpDocKey.KeyAdminActionCert = local.DateWorkArea.Date;

              break;
            case "IDMILITARY":
              export.SpDocKey.KeyMilitaryService = local.DateWorkArea.Date;

              break;
            case "IDOBLADACT":
              export.SpDocKey.KeyObligationAdminAction =
                local.DateWorkArea.Date;

              break;
            case "CASXFRFRDT":
              export.SpDocKey.KeyXferFromDate = local.DateWorkArea.Date;

              break;
            case "CASXFRTODT":
              export.SpDocKey.KeyXferToDate = local.DateWorkArea.Date;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "NUMERIC":
          local.Verify.Count = Verify(local.FieldValue.Value, "0123456789");

          if (local.Verify.Count > 0)
          {
            continue;
          }

          local.BatchConvertNumToText.Number15 =
            StringToNumber(local.FieldValue.Value);

          switch(TrimEnd(entities.Field.Name))
          {
            case "IDAAPPEAL":
              export.SpDocKey.KeyAdminAppeal =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDBANKRPTC":
              export.SpDocKey.KeyBankruptcy =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDCASHDETL":
              export.SpDocKey.KeyCashRcptDetail =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDCASHEVNT":
              export.SpDocKey.KeyCashRcptEvent =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDCASHSRCE":
              export.SpDocKey.KeyCashRcptSource =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDCASHTYPE":
              export.SpDocKey.KeyCashRcptType =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDCONTACT":
              export.SpDocKey.KeyContact =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDGENETIC":
              export.SpDocKey.KeyGeneticTest =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDHINSCOV":
              export.SpDocKey.KeyHealthInsCoverage =
                local.BatchConvertNumToText.Number15;

              break;
            case "IDINCARCER":
              export.SpDocKey.KeyIncarceration =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDINFORQST":
              export.SpDocKey.KeyInfoRequest =
                local.BatchConvertNumToText.Number15;

              break;
            case "IDINTSTREQ":
              export.SpDocKey.KeyInterstateRequest =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDLEGALACT":
              export.SpDocKey.KeyLegalAction =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDLEGALDTL":
              export.SpDocKey.KeyLegalActionDetail =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDLEGALREF":
              export.SpDocKey.KeyLegalReferral =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDLOCRQSRC":
              export.SpDocKey.KeyLocateRequestSource =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDOBLIGATN":
              export.SpDocKey.KeyObligation =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDOBLTYPE":
              export.SpDocKey.KeyObligationType =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDRCAPTURE":
              export.SpDocKey.KeyRecaptureRule =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDRESOURCE":
              export.SpDocKey.KeyResource =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDTRIBUNAL":
              export.SpDocKey.KeyTribunal =
                (int)local.BatchConvertNumToText.Number15;

              break;
            case "IDWRKSHEET":
              export.SpDocKey.KeyWorksheet =
                local.BatchConvertNumToText.Number15;

              break;
            case "CASXFROFFC":
              export.SpDocKey.KeyOffice =
                (int)local.BatchConvertNumToText.Number15;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "TEXT":
          switch(TrimEnd(entities.Field.Name))
          {
            case "IDADMINACT":
              export.SpDocKey.KeyAdminAction = local.FieldValue.Value ?? Spaces
                (4);

              break;
            case "IDAP":
              export.SpDocKey.KeyAp = local.FieldValue.Value ?? Spaces(10);

              break;
            case "IDAR":
              export.SpDocKey.KeyAr = local.FieldValue.Value ?? Spaces(10);

              break;
            case "IDCHILD":
              export.SpDocKey.KeyChild = local.FieldValue.Value ?? Spaces(10);

              break;
            case "IDCSECASE":
              export.SpDocKey.KeyCase = local.FieldValue.Value ?? Spaces(10);

              break;
            case "IDLOCRQAGN":
              export.SpDocKey.KeyLocateRequestAgency =
                local.FieldValue.Value ?? Spaces(5);

              break;
            case "IDPERSACCT":
              export.SpDocKey.KeyPersonAccount = local.FieldValue.Value ?? Spaces
                (1);

              break;
            case "IDPERSON":
              export.SpDocKey.KeyPerson = local.FieldValue.Value ?? Spaces(10);

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "TSTAMP":
          local.Verify.Count = Verify(local.FieldValue.Value, "0123456789-.");

          if (local.Verify.Count > 0)
          {
            continue;
          }

          local.BatchTimestampWorkArea.TextTimestamp =
            local.FieldValue.Value ?? Spaces(26);
          UseLeCabConvertTimestamp();

          switch(TrimEnd(entities.Field.Name))
          {
            case "IDAPPT":
              export.SpDocKey.KeyAppointment =
                local.BatchTimestampWorkArea.IefTimestamp;

              break;
            case "IDINCSOURC":
              export.SpDocKey.KeyIncomeSource =
                local.BatchTimestampWorkArea.IefTimestamp;

              break;
            case "IDPERSADDR":
              export.SpDocKey.KeyPersonAddress =
                local.BatchTimestampWorkArea.IefTimestamp;

              break;
            case "IDWRKRCOMP":
              export.SpDocKey.KeyWorkerComp =
                local.BatchTimestampWorkArea.IefTimestamp;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

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

  private IEnumerable<bool> ReadFieldDocumentField()
  {
    entities.Field.Populated = false;
    entities.DocumentField.Populated = false;

    return ReadEach("ReadFieldDocumentField",
      (db, command) =>
      {
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "dependancy", import.Field.Dependancy);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 3);
        entities.DocumentField.DocName = db.GetString(reader, 4);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 5);
        entities.Field.Populated = true;
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetString(command, "fldName", entities.DocumentField.FldName);
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
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

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.OutgoingDocument.Populated = true;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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

    private Infrastructure infrastructure;
    private Document document;
    private Field field;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
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

    private SpDocKey spDocKey;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private FieldValue fieldValue;
    private BatchConvertNumToText batchConvertNumToText;
    private Common verify;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private FieldValue fieldValue;
    private Field field;
    private DocumentField documentField;
    private Document document;
  }
#endregion
}
