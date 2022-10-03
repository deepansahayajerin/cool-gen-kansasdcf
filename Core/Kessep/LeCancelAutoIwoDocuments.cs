// Program: LE_CANCEL_AUTO_IWO_DOCUMENTS, ID: 371066337, model: 746.
// Short name: SWE02068
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CANCEL_AUTO_IWO_DOCUMENTS.
/// </summary>
[Serializable]
public partial class LeCancelAutoIwoDocuments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CANCEL_AUTO_IWO_DOCUMENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCancelAutoIwoDocuments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCancelAutoIwoDocuments.
  /// </summary>
  public LeCancelAutoIwoDocuments(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    // Date	  Author	Reason
    // --------  ---------     
    // ------------------------------------
    // 02/21/01  GVandy	WR187 - Initial Development
    // 07/25/11  GVandy	CQ29166 - Performance change.
    // 			Restructure logic which cancels auto IWO documents.
    // ------------------------------------------------------------
    local.Today.Date = Now().Date;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    foreach(var item in ReadLegalActionTribunal())
    {
      if (Equal(entities.Distinct.CourtCaseNumber,
        local.PreviousLegalAction.CourtCaseNumber) && entities
        .Tribunal.Identifier == local.PreviousTribunal.Identifier)
      {
        continue;
      }
      else
      {
        local.PreviousLegalAction.CourtCaseNumber =
          entities.Distinct.CourtCaseNumber;
        local.PreviousTribunal.Identifier = entities.Tribunal.Identifier;
      }

      foreach(var item1 in ReadLegalAction())
      {
        if (Equal(entities.Iclass.ActionTaken, "IWOTERM") || Equal
          (entities.Iclass.ActionTaken, "IWONOTKT") || Equal
          (entities.Iclass.ActionTaken, "IWOMODO") || Equal
          (entities.Iclass.ActionTaken, "IWONOTKM") || Equal
          (entities.Iclass.ActionTaken, "IWONOTKS") || Equal
          (entities.Iclass.ActionTaken, "IWO"))
        {
          // -- If the most recent I class legal action is not NOIIWON or 
          // ORDIWO2 then skip this court case number.
          goto ReadEach2;
        }
        else if (Equal(entities.Iclass.ActionTaken, "NOIIWON") || Equal
          (entities.Iclass.ActionTaken, "ORDIWO2"))
        {
          if (Equal(entities.Iclass.ActionTaken, "ORDIWO2"))
          {
            local.Document.Name = "ORDIWO2A";
          }
          else if (Equal(entities.Iclass.ActionTaken, "NOIIWON"))
          {
            local.Document.Name = "NOIIWONA";
          }

          foreach(var item2 in ReadCase())
          {
            local.Field.Name = "IDLEGALACT";
            local.FieldValue.Value =
              NumberToString(entities.Iclass.Identifier, 7, 9);

            for(local.PrntSucessful.Count = 1; local.PrntSucessful.Count <= 4; ++
              local.PrntSucessful.Count)
            {
              // -- The read each is done for each possible print_sucessful_ind 
              // value to improve performance.
              switch(local.PrntSucessful.Count)
              {
                case 1:
                  local.OutgoingDocument.PrintSucessfulIndicator = "G";

                  break;
                case 2:
                  local.OutgoingDocument.PrintSucessfulIndicator = "M";

                  break;
                case 3:
                  local.OutgoingDocument.PrintSucessfulIndicator = "P";

                  break;
                case 4:
                  local.OutgoingDocument.PrintSucessfulIndicator = "R";

                  break;
                default:
                  break;
              }

              foreach(var item3 in ReadOutgoingDocument())
              {
                for(local.Common.Count = 1; local.Common.Count <= 3; ++
                  local.Common.Count)
                {
                  switch(local.Common.Count)
                  {
                    case 1:
                      local.Field.Name = "IDAP";
                      local.FieldValue.Value = import.CsePerson.Number;

                      break;
                    case 2:
                      local.Field.Name = "IDINCSOURC";
                      local.BatchTimestampWorkArea.IefTimestamp =
                        import.IncomeSource.Identifier;
                      UseLeCabConvertTimestamp();
                      local.FieldValue.Value =
                        local.BatchTimestampWorkArea.TextTimestamp;

                      break;
                    case 3:
                      if (Equal(local.Document.Name, "NOIIWONA"))
                      {
                        // -- Case is not a key field on the NOIIWONA document.
                        continue;
                      }

                      local.Field.Name = "IDCSECASE";
                      local.FieldValue.Value = entities.Case1.Number;

                      break;
                    default:
                      break;
                  }

                  if (!ReadFieldValue())
                  {
                    // -- This outgoing_document does not match
                    goto ReadEach1;
                  }
                }

                if (ReadInfrastructure())
                {
                  local.Infrastructure.SystemGeneratedIdentifier =
                    entities.Infrastructure.SystemGeneratedIdentifier;
                  UseSpDocCancelOutgoingDoc();
                }

                break;

ReadEach1:
                ;
              }
            }
          }

          goto ReadEach2;
        }
        else
        {
          // -- For this process we are only concerned with actions IWO, 
          // NOIIWON, ORDIWO2, IWOTERM, IWONOTKT, IWOMODO, IWONOTKM, and
          // IWONOTKS.  The current I class legal action is not one of these
          // actions.  Get the next I class legal action for the court case
          // number.
        }
      }

ReadEach2:
      ;
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

  private void UseSpDocCancelOutgoingDoc()
  {
    var useImport = new SpDocCancelOutgoingDoc.Import();
    var useExport = new SpDocCancelOutgoingDoc.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(SpDocCancelOutgoingDoc.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
        db.SetNullableString(command, "valu", local.FieldValue.Value ?? "");
        db.SetString(command, "fldName", local.Field.Name);
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

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", entities.OutgoingDocument.InfId);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.Iclass.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Iclass.Identifier = db.GetInt32(reader, 0);
        entities.Iclass.Classification = db.GetString(reader, 1);
        entities.Iclass.ActionTaken = db.GetString(reader, 2);
        entities.Iclass.Type1 = db.GetString(reader, 3);
        entities.Iclass.FiledDate = db.GetNullableDate(reader, 4);
        entities.Iclass.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.Iclass.EndDate = db.GetNullableDate(reader, 6);
        entities.Iclass.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Iclass.TrbId = db.GetNullableInt32(reader, 8);
        entities.Iclass.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.Distinct.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Distinct.Identifier = db.GetInt32(reader, 0);
        entities.Distinct.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.Distinct.TrbId = db.GetNullableInt32(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Distinct.Populated = true;
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetString(
          command, "prntSucessfulInd",
          local.OutgoingDocument.PrintSucessfulIndicator);
        db.SetNullableString(command, "docName", local.Document.Name);
        db.SetNullableString(command, "valu", local.FieldValue.Value ?? "");
        db.SetString(command, "fldName", local.Field.Name);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 3);
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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

    private IncomeSource incomeSource;
    private CsePerson csePerson;
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
    /// A value of PrntSucessful.
    /// </summary>
    [JsonPropertyName("prntSucessful")]
    public Common PrntSucessful
    {
      get => prntSucessful ??= new();
      set => prntSucessful = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of PreviousTribunal.
    /// </summary>
    [JsonPropertyName("previousTribunal")]
    public Tribunal PreviousTribunal
    {
      get => previousTribunal ??= new();
      set => previousTribunal = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Today.
    /// </summary>
    [JsonPropertyName("today")]
    public DateWorkArea Today
    {
      get => today ??= new();
      set => today = value;
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

    private Common prntSucessful;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common common;
    private FieldValue fieldValue;
    private Field field;
    private Tribunal previousTribunal;
    private LegalAction previousLegalAction;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private Document document;
    private SpDocKey spDocKey;
    private DateWorkArea today;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Distinct.
    /// </summary>
    [JsonPropertyName("distinct")]
    public LegalAction Distinct
    {
      get => distinct ??= new();
      set => distinct = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of Iclass.
    /// </summary>
    [JsonPropertyName("iclass")]
    public LegalAction Iclass
    {
      get => iclass ??= new();
      set => iclass = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private Document document;
    private DocumentField documentField;
    private Field field;
    private FieldValue fieldValue;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private LegalAction distinct;
    private Tribunal tribunal;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole absentParent;
    private CsePerson csePerson;
    private LegalAction iclass;
    private Case1 case1;
    private LegalAction legalAction;
  }
#endregion
}
