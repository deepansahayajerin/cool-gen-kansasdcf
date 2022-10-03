// Program: FN_CHECK_FOR_COURT_ORDER_SUPPR, ID: 371248958, model: 746.
// Short name: SWE02848
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
/// A program: FN_CHECK_FOR_COURT_ORDER_SUPPR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will determine if disbursement suppression is turned on at
/// the person level.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForCourtOrderSuppr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_COURT_ORDER_SUPPR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForCourtOrderSuppr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForCourtOrderSuppr.
  /// </summary>
  public FnCheckForCourtOrderSuppr(IContext context, Import import,
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
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    New ABfor changes for suppression by 
    // court order number.
    // *******************************************************************
    // ******************************************************************
    // 04/22/05   WR 040796  Fangman    Took out Read Each that was not needed.
    // *******************************************************************
    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "     *** In FN_Check_For_Court_Order_Suppr for " + import
        .Per.ReferenceNumber + "  Disb ID " + NumberToString
        (import.Per.SystemGeneratedIdentifier, 7, 9);
      UseCabControlReport();
    }

    export.DisbSuppressionStatusHistory.DiscontinueDate =
      local.Initialized.DiscontinueDate;

    if (ReadCollectionCollectionType())
    {
      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "     *** Collection Ct Order number " + entities
          .Collection.CourtOrderAppliedTo + " " + " ";
        UseCabControlReport();
      }

      foreach(var item in ReadDisbSuppressionStatusHistoryLegalAction())
      {
        if (AsChar(import.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "     *** Found rule.";
          UseCabControlReport();
          local.EabReportSend.RptDetail =
            "     *** Collection Ct Order number " + entities
            .Collection.CourtOrderAppliedTo + "  Legal Court Order # " + entities
            .LegalAction.StandardNumber;
          UseCabControlReport();
          local.EabReportSend.RptDetail =
            "     *** about to check for a relationship to a collection type.";
          UseCabControlReport();
        }

        // Check to see if this O rule is also associated to a collection type
        if (ReadCollectionType())
        {
          if (AsChar(import.TestDisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "     *** Found collection type relationship";
            UseCabControlReport();
            local.EabReportSend.RptDetail = "     *** Disb collection type " + NumberToString
              (entities.CollectionType.SequentialIdentifier, 15) + "  Suppr Rule collection type " +
              NumberToString
              (entities.ReadForSuppRule.SequentialIdentifier, 15);
            UseCabControlReport();
          }

          if (entities.ReadForSuppRule.SequentialIdentifier == entities
            .CollectionType.SequentialIdentifier)
          {
            if (entities.ReadForSuppRule.SequentialIdentifier == 3)
            {
              export.DisbSuppressionStatusHistory.DiscontinueDate =
                AddMonths(import.Per.CollectionDate, 6);
            }
            else
            {
              export.DisbSuppressionStatusHistory.DiscontinueDate =
                entities.DisbSuppressionStatusHistory.DiscontinueDate;
            }

            goto Read;
          }

          continue;
        }
        else
        {
          if (AsChar(import.TestDisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "     *** Did not find collection type relationship";
            UseCabControlReport();
          }

          export.DisbSuppressionStatusHistory.DiscontinueDate =
            entities.DisbSuppressionStatusHistory.DiscontinueDate;

          goto Read;
        }
      }
    }
    else
    {
      // continue
    }

Read:

    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     *** Court Order Suppr date is " + NumberToString
        (DateToInt(export.DisbSuppressionStatusHistory.DiscontinueDate), 15);
      UseCabControlReport();
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCollectionCollectionType()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.Collection.Populated = false;
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "collId", import.Per.ColId.GetValueOrDefault());
        db.SetInt32(command, "otyId", import.Per.OtyId.GetValueOrDefault());
        db.SetInt32(command, "obgId", import.Per.ObgId.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Per.CspNumberDisb ?? "");
        db.SetString(command, "cpaType", import.Per.CpaTypeDisb ?? "");
        db.SetInt32(command, "otrId", import.Per.OtrId.GetValueOrDefault());
        db.SetString(command, "otrType", import.Per.OtrTypeDisb ?? "");
        db.SetInt32(command, "crtType", import.Per.CrtId.GetValueOrDefault());
        db.SetInt32(command, "cstId", import.Per.CstId.GetValueOrDefault());
        db.SetInt32(command, "crvId", import.Per.CrvId.GetValueOrDefault());
        db.SetInt32(command, "crdId", import.Per.CrdId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 11);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 12);
        entities.Collection.Populated = true;
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);
    entities.ReadForSuppRule.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.DisbSuppressionStatusHistory.CltSequentialId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForSuppRule.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.ReadForSuppRule.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistoryLegalAction()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistoryLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PerObligee.Type1);
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", entities.Collection.CourtOrderAppliedTo ?? ""
          );
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.LegalAction.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        entities.LegalAction.Populated = true;

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
    /// A value of PerObligee.
    /// </summary>
    [JsonPropertyName("perObligee")]
    public CsePersonAccount PerObligee
    {
      get => perObligee ??= new();
      set => perObligee = value;
    }

    /// <summary>
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public DisbursementTransaction Per
    {
      get => per ??= new();
      set => per = value;
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
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private CsePersonAccount perObligee;
    private DisbursementTransaction per;
    private ProgramProcessingInfo programProcessingInfo;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DisbSuppressionStatusHistory Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private DisbSuppressionStatusHistory initialized;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of ReadForSuppRule.
    /// </summary>
    [JsonPropertyName("readForSuppRule")]
    public CollectionType ReadForSuppRule
    {
      get => readForSuppRule ??= new();
      set => readForSuppRule = value;
    }

    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private LegalAction legalAction;
    private CollectionType readForSuppRule;
  }
#endregion
}
