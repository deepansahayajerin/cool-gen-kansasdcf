// Program: SP_DOC_CANCEL_OUTGOING_DOC, ID: 372155521, model: 746.
// Short name: SWE02307
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_CANCEL_OUTGOING_DOC.
/// </para>
/// <para>
/// Updates the infrastructure record and the associated outgoing_doc.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocCancelOutgoingDoc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_CANCEL_OUTGOING_DOC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocCancelOutgoingDoc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocCancelOutgoingDoc.
  /// </summary>
  public SpDocCancelOutgoingDoc(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // mjr
    // ---------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ------------------------------------------------------------------------------
    // 02/01/1999	M Ramirez			Initial development
    // ------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (import.Infrastructure.SystemGeneratedIdentifier > 0)
    {
      if (ReadInfrastructure())
      {
        local.Infrastructure.Assign(entities.Infrastructure);
      }
    }

    if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
    {
      ExitState = "INFRASTRUCTURE_NF";

      return;
    }

    local.Infrastructure.Detail = "Document printing canceled.";

    if (IsEmpty(import.Infrastructure.LastUpdatedBy))
    {
      local.Infrastructure.LastUpdatedBy = global.UserId;
    }

    if (!Lt(local.Null1.Timestamp, import.Infrastructure.LastUpdatedTimestamp))
    {
      local.Infrastructure.LastUpdatedTimestamp = Now();
    }

    UseSpCabUpdateInfrastructure();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.OutgoingDocument.PrintSucessfulIndicator = "C";
    local.OutgoingDocument.LastUpdatedBy =
      local.Infrastructure.LastUpdatedBy ?? "";
    local.OutgoingDocument.LastUpdatdTstamp =
      local.Infrastructure.LastUpdatedTimestamp;
    UseUpdateOutgoingDocument();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (ReadIwoAction())
    {
      if (ReadIwoTransactionCsePersonLegalAction())
      {
        local.IwoAction.Assign(entities.IwoAction);
        local.IwoAction.StatusCd = "C";
        local.IwoTransaction.Identifier = entities.IwoTransaction.Identifier;
        UseLeUpdateIwoActionStatus();
      }
    }
  }

  private void UseLeUpdateIwoActionStatus()
  {
    var useImport = new LeUpdateIwoActionStatus.Import();
    var useExport = new LeUpdateIwoActionStatus.Export();

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.IwoAction.Assign(local.IwoAction);
    useImport.IwoTransaction.Identifier = local.IwoTransaction.Identifier;

    Call(LeUpdateIwoActionStatus.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.OutgoingDocument.Assign(local.OutgoingDocument);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
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
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
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
    entities.LegalAction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.IwoTransaction.Populated = false;

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
        entities.LegalAction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.IwoTransaction.Populated = true;
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

    private Infrastructure infrastructure;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    private DateWorkArea null1;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private OutgoingDocument outgoingDocument;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private IwoTransaction iwoTransaction;
    private IwoAction iwoAction;
    private Infrastructure infrastructure;
  }
#endregion
}
