// Program: ZDEL_SP_B704_WRITE_REPORT_01, ID: 372987065, model: 746.
// Short name: ZDEL2503
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: ZDEL_SP_B704_WRITE_REPORT_01.
/// </summary>
[Serializable]
public partial class ZdelSpB704WriteReport01: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ZDEL_SP_B704_WRITE_REPORT_01 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ZdelSpB704WriteReport01(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ZdelSpB704WriteReport01.
  /// </summary>
  public ZdelSpB704WriteReport01(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Format lines for report 01
    // -------------------------------------------------------------------
    local.CollectionOverflow.Flag = import.CollectionOverflow.Flag;
    local.NoticeOverflow.Flag = import.NoticeOverflow.Flag;

    // -------------------------------------------------------------------
    // OBLIGOR
    // -------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "OBLIGOR:  " + import
      .CsePersonsWorkSet.Number + "  " + import
      .CsePersonsWorkSet.FormattedName;
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      return;
    }

    // -------------------------------------------------------------------
    // LEGAL ACTION
    // -------------------------------------------------------------------
    local.LegalActionId.Text9 =
      NumberToString(import.LegalAction.Identifier, 7, 9);
    local.EabReportSend.RptDetail = "LEGAL ACTION:  " + (
      import.LegalAction.StandardNumber ?? "") + "  (" + local
      .LegalActionId.Text9 + ")";
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      return;
    }

    // -------------------------------------------------------------------
    // CASH RECEIPT DETAIL AMOUNT
    // -------------------------------------------------------------------
    local.EabConvertNumeric.SendAmount =
      NumberToString((long)(import.CashReceiptDetail.CollectionAmount * 100), 15);
      
    local.EabConvertNumeric.SendNonSuppressPos = 3;
    UseEabConvertNumeric1();
    local.CrdCollAmount.Text12 =
      Substring(local.EabConvertNumeric.ReturnCurrencySigned, 10, 12);

    // -------------------------------------------------------------------
    // CASH RECEIPT DETAIL COLECTION DATE
    // -------------------------------------------------------------------
    if (Lt(local.NullDateWorkArea.Date, import.CashReceiptDetail.CollectionDate))
      
    {
      local.CrdRcvdDate.Text9 =
        NumberToString(DateToInt(import.CashReceiptDetail.CollectionDate), 8, 8);
        
    }

    // -------------------------------------------------------------------
    // CASH RECEIPT DETAIL IDENTIFIERS
    // -------------------------------------------------------------------
    local.CrdId.Text4 =
      NumberToString(import.CashReceiptDetail.SequentialIdentifier, 12, 4);
    local.CrvId.Text9 =
      NumberToString(import.CashReceiptEvent.SystemGeneratedIdentifier, 7, 9);
    local.CrsId.Text3 =
      NumberToString(import.CashReceiptSourceType.SystemGeneratedIdentifier, 13,
      3);
    local.CrtId.Text3 =
      NumberToString(import.CashReceiptType.SystemGeneratedIdentifier, 13, 3);
    local.EabReportSend.RptDetail = "CASH RECEIPT DETL:  " + local
      .CrdCollAmount.Text12 + ",  RCVD:  " + local.CrdRcvdDate.Text9 + "  (" + local
      .CrdId.Text4 + "," + local.CrvId.Text9 + "," + local.CrsId.Text3 + "," + local
      .CrtId.Text3 + ")";
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      return;
    }

    // mjr
    // -------------------------------------------------------
    // Format of report line
    // Collections			Notices
    // 	Amount (9.2)			LACTORDNUM (17)
    // 	Disbursement Date (8)		CASHCOLAMT (9.2)
    // 	Adjusted Ind (1)		OVERCOLDT (10)
    // 	Adjusted Date (8)		CASHCHCKNO (12)
    // 					CASHADJAMT (9.2)
    // 					SYSCURRDT (10)
    // ----------------------------------------------------------
    // 	24				75
    // ----------------------------------------------------------
    local.CollAmt.Text14 = "COLL  AMOUNT";
    local.CollDistDate.Text9 = "DIST DT A";
    local.CollAdjDate.Text14 = " ADJ DATE";
    local.NoticeLactordnum.Text20 = "COURT ORDER NUMBER";
    local.NoticeCashcolamt.Text14 = "COLL AMOUNT";
    local.NoticeOvercoldt.Text11 = "COLL DATE";
    local.NoticeCashchckno.Text13 = "CHECK NUMBER";
    local.NoticeCashadjamt.Text14 = "COLL ADJ AMT";
    local.NoticeSyscurrdt.Text11 = "NOTICE DATE";
    local.EabReportSend.RptDetail = local.CollAmt.Text14 + local
      .CollDistDate.Text9 + "DJ" + local.CollAdjDate.Text14 + local
      .NullWorkArea.Text10 + local.NoticeLactordnum.Text20 + local
      .NoticeCashcolamt.Text14 + local.NoticeOvercoldt.Text11 + local
      .NoticeCashchckno.Text13 + local.NoticeCashadjamt.Text14 + local
      .NoticeSyscurrdt.Text11;
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      return;
    }

    import.Collections.Index = -1;
    import.Notices.Index = -1;
    local.Exit.Flag = "N";

    while(AsChar(local.Exit.Flag) != 'Y')
    {
      local.CollAmt.Text14 = "";
      local.CollDistDate.Text9 = "";
      local.Collection.AdjustedInd = "";
      local.CollAdjDate.Text14 = "";
      local.NullWorkArea.Text10 = "";
      local.NoticeLactordnum.Text20 = "";
      local.NoticeCashcolamt.Text14 = "";
      local.NoticeOvercoldt.Text11 = "";
      local.NoticeCashchckno.Text13 = "";
      local.NoticeCashadjamt.Text14 = "";
      local.NoticeSyscurrdt.Text11 = "";

      // -------------------------------------------------------------------
      // COLLECTION DETAILS
      // -------------------------------------------------------------------
      if (AsChar(local.CollectionOverflow.Flag) == 'N')
      {
        if (import.Collections.Index + 1 < import.Collections.Count)
        {
          ++import.Collections.Index;
          import.Collections.CheckSize();

          if (import.Collections.Item.I.Amount > 0)
          {
            // -------------------------------------------------------------------
            // COLLECTION AMOUNT
            // -------------------------------------------------------------------
            local.EabConvertNumeric.SendAmount =
              NumberToString((long)(import.Collections.Item.I.Amount * 100), 15);
              
            local.EabConvertNumeric.SendNonSuppressPos = 3;
            UseEabConvertNumeric1();
            local.CollAmt.Text14 =
              Substring(local.EabConvertNumeric.ReturnCurrencySigned, 9, 13);

            // -------------------------------------------------------------------
            // COLLECTION DISTRIBUTION DATE
            // -------------------------------------------------------------------
            local.CollDistDate.Text9 =
              NumberToString(DateToInt(
                Date(import.Collections.Item.I.CreatedTmst)), 8, 8);

            // -------------------------------------------------------------------
            // COLLECTION ADJUSTMENT IND
            // -------------------------------------------------------------------
            local.Collection.AdjustedInd =
              import.Collections.Item.I.AdjustedInd ?? "";

            // -------------------------------------------------------------------
            // COLLECTION ADJUSTMENT DATE
            // -------------------------------------------------------------------
            if (Lt(local.NullDateWorkArea.Date,
              import.Collections.Item.I.CollectionAdjustmentDt))
            {
              local.CollAdjDate.Text14 =
                NumberToString(DateToInt(
                  import.Collections.Item.I.CollectionAdjustmentDt), 8, 8);
            }
          }
        }
      }
      else if (AsChar(local.CollectionOverflow.Flag) == 'Y')
      {
        local.CollectionOverflow.Flag = "";
        local.CollAmt.Text14 = "* More collect";
        local.CollDistDate.Text9 = "ions exis";
        local.Collection.AdjustedInd = "t";
        local.CollAdjDate.Text14 = " than can be s";
        local.NullWorkArea.Text10 = "hown here";
      }
      else
      {
      }

      // -------------------------------------------------------------------
      // NOTICE DETAILS
      // -------------------------------------------------------------------
      if (AsChar(import.NoticeOverflow.Flag) == 'N')
      {
        if (import.Notices.Index + 1 < import.Notices.Count)
        {
          ++import.Notices.Index;
          import.Notices.CheckSize();

          if (!IsEmpty(import.Notices.Item.I.PrintSucessfulIndicator))
          {
            for(import.Notices.Item.SubFields.Index = 0; import
              .Notices.Item.SubFields.Index < import
              .Notices.Item.SubFields.Count; ++
              import.Notices.Item.SubFields.Index)
            {
              if (!import.Notices.Item.SubFields.CheckSize())
              {
                break;
              }

              switch(TrimEnd(import.Notices.Item.SubFields.Item.IgroupSubField.
                Name))
              {
                case "LACTORDNUM":
                  local.NoticeLactordnum.Text20 =
                    import.Notices.Item.SubFields.Item.IgroupSubFieldValue.
                      Value ?? Spaces(20);

                  break;
                case "CASHCOLAMT":
                  local.NoticeCashcolamt.Text14 =
                    import.Notices.Item.SubFields.Item.IgroupSubFieldValue.
                      Value ?? Spaces(14);

                  break;
                case "CASHCHCKNO":
                  local.NoticeCashchckno.Text13 =
                    import.Notices.Item.SubFields.Item.IgroupSubFieldValue.
                      Value ?? Spaces(13);

                  break;
                case "CASHADJAMT":
                  local.NoticeCashadjamt.Text14 =
                    import.Notices.Item.SubFields.Item.IgroupSubFieldValue.
                      Value ?? Spaces(14);

                  break;
                case "OVERCOLDT":
                  local.NoticeOvercoldt.Text11 =
                    import.Notices.Item.SubFields.Item.IgroupSubFieldValue.
                      Value ?? Spaces(11);

                  break;
                case "SYSCURRDT":
                  local.NoticeSyscurrdt.Text11 =
                    import.Notices.Item.SubFields.Item.IgroupSubFieldValue.
                      Value ?? Spaces(11);

                  break;
                default:
                  local.NoticeLactordnum.Text20 = "FIELD NAME NOT FOUND";

                  break;
              }
            }

            import.Notices.Item.SubFields.CheckIndex();
          }
        }
      }
      else if (AsChar(local.NoticeOverflow.Flag) == 'Y')
      {
        local.NoticeOverflow.Flag = "";
        local.NoticeLactordnum.Text20 = "* More notices exist";
        local.NoticeCashcolamt.Text14 = " than can be s";
        local.NoticeOvercoldt.Text11 = "hown here";
      }
      else
      {
      }

      if (!IsEmpty(local.CollAmt.Text14) || !
        IsEmpty(local.NoticeLactordnum.Text20))
      {
        local.EabReportSend.RptDetail = local.CollAmt.Text14 + local
          .CollDistDate.Text9 + (local.Collection.AdjustedInd ?? "") + " " + local
          .CollAdjDate.Text14 + local.NullWorkArea.Text10 + local
          .NoticeLactordnum.Text20 + local.NoticeCashcolamt.Text14 + local
          .NoticeOvercoldt.Text11 + local.NoticeCashchckno.Text13 + local
          .NoticeCashadjamt.Text14 + local.NoticeSyscurrdt.Text11;
        UseCabBusinessReport01();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }
      }
      else
      {
        local.Exit.Flag = "Y";
      }
    }

    local.EabReportSend.RptDetail =
      "------------------------------------------------------------------------------------------------------------------------------------";
      
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
    }
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    MoveEabConvertNumeric2(local.EabConvertNumeric, useExport.EabConvertNumeric);
      

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A NoticesGroup group.</summary>
    [Serializable]
    public class NoticesGroup
    {
      /// <summary>
      /// A value of I.
      /// </summary>
      [JsonPropertyName("i")]
      public OutgoingDocument I
      {
        get => i ??= new();
        set => i = value;
      }

      /// <summary>
      /// Gets a value of SubFields.
      /// </summary>
      [JsonIgnore]
      public Array<SubFieldsGroup> SubFields => subFields ??= new(
        SubFieldsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of SubFields for json serialization.
      /// </summary>
      [JsonPropertyName("subFields")]
      [Computed]
      public IList<SubFieldsGroup> SubFields_Json
      {
        get => subFields;
        set => SubFields.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OutgoingDocument i;
      private Array<SubFieldsGroup> subFields;
    }

    /// <summary>A SubFieldsGroup group.</summary>
    [Serializable]
    public class SubFieldsGroup
    {
      /// <summary>
      /// A value of IgroupSubField.
      /// </summary>
      [JsonPropertyName("igroupSubField")]
      public Field IgroupSubField
      {
        get => igroupSubField ??= new();
        set => igroupSubField = value;
      }

      /// <summary>
      /// A value of IgroupSubFieldValue.
      /// </summary>
      [JsonPropertyName("igroupSubFieldValue")]
      public FieldValue IgroupSubFieldValue
      {
        get => igroupSubFieldValue ??= new();
        set => igroupSubFieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Field igroupSubField;
      private FieldValue igroupSubFieldValue;
    }

    /// <summary>A CollectionsGroup group.</summary>
    [Serializable]
    public class CollectionsGroup
    {
      /// <summary>
      /// A value of I.
      /// </summary>
      [JsonPropertyName("i")]
      public Collection I
      {
        get => i ??= new();
        set => i = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Collection i;
    }

    /// <summary>
    /// A value of NoticeOverflow.
    /// </summary>
    [JsonPropertyName("noticeOverflow")]
    public Common NoticeOverflow
    {
      get => noticeOverflow ??= new();
      set => noticeOverflow = value;
    }

    /// <summary>
    /// A value of CollectionOverflow.
    /// </summary>
    [JsonPropertyName("collectionOverflow")]
    public Common CollectionOverflow
    {
      get => collectionOverflow ??= new();
      set => collectionOverflow = value;
    }

    /// <summary>
    /// Gets a value of Notices.
    /// </summary>
    [JsonIgnore]
    public Array<NoticesGroup> Notices => notices ??= new(
      NoticesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Notices for json serialization.
    /// </summary>
    [JsonPropertyName("notices")]
    [Computed]
    public IList<NoticesGroup> Notices_Json
    {
      get => notices;
      set => Notices.Assign(value);
    }

    /// <summary>
    /// Gets a value of Collections.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionsGroup> Collections => collections ??= new(
      CollectionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Collections for json serialization.
    /// </summary>
    [JsonPropertyName("collections")]
    [Computed]
    public IList<CollectionsGroup> Collections_Json
    {
      get => collections;
      set => Collections.Assign(value);
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common noticeOverflow;
    private Common collectionOverflow;
    private Array<NoticesGroup> notices;
    private Array<CollectionsGroup> collections;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoticeOverflow.
    /// </summary>
    [JsonPropertyName("noticeOverflow")]
    public Common NoticeOverflow
    {
      get => noticeOverflow ??= new();
      set => noticeOverflow = value;
    }

    /// <summary>
    /// A value of CollectionOverflow.
    /// </summary>
    [JsonPropertyName("collectionOverflow")]
    public Common CollectionOverflow
    {
      get => collectionOverflow ??= new();
      set => collectionOverflow = value;
    }

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
    /// A value of CrdRcvdDate.
    /// </summary>
    [JsonPropertyName("crdRcvdDate")]
    public WorkArea CrdRcvdDate
    {
      get => crdRcvdDate ??= new();
      set => crdRcvdDate = value;
    }

    /// <summary>
    /// A value of CrdCollAmount.
    /// </summary>
    [JsonPropertyName("crdCollAmount")]
    public WorkArea CrdCollAmount
    {
      get => crdCollAmount ??= new();
      set => crdCollAmount = value;
    }

    /// <summary>
    /// A value of CrvId.
    /// </summary>
    [JsonPropertyName("crvId")]
    public WorkArea CrvId
    {
      get => crvId ??= new();
      set => crvId = value;
    }

    /// <summary>
    /// A value of CrsId.
    /// </summary>
    [JsonPropertyName("crsId")]
    public WorkArea CrsId
    {
      get => crsId ??= new();
      set => crsId = value;
    }

    /// <summary>
    /// A value of CrtId.
    /// </summary>
    [JsonPropertyName("crtId")]
    public WorkArea CrtId
    {
      get => crtId ??= new();
      set => crtId = value;
    }

    /// <summary>
    /// A value of CrdId.
    /// </summary>
    [JsonPropertyName("crdId")]
    public WorkArea CrdId
    {
      get => crdId ??= new();
      set => crdId = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of LegalActionId.
    /// </summary>
    [JsonPropertyName("legalActionId")]
    public WorkArea LegalActionId
    {
      get => legalActionId ??= new();
      set => legalActionId = value;
    }

    /// <summary>
    /// A value of Exit.
    /// </summary>
    [JsonPropertyName("exit")]
    public Common Exit
    {
      get => exit ??= new();
      set => exit = value;
    }

    /// <summary>
    /// A value of NoticeLactordnum.
    /// </summary>
    [JsonPropertyName("noticeLactordnum")]
    public WorkArea NoticeLactordnum
    {
      get => noticeLactordnum ??= new();
      set => noticeLactordnum = value;
    }

    /// <summary>
    /// A value of NoticeSyscurrdt.
    /// </summary>
    [JsonPropertyName("noticeSyscurrdt")]
    public WorkArea NoticeSyscurrdt
    {
      get => noticeSyscurrdt ??= new();
      set => noticeSyscurrdt = value;
    }

    /// <summary>
    /// A value of NoticeCashadjamt.
    /// </summary>
    [JsonPropertyName("noticeCashadjamt")]
    public WorkArea NoticeCashadjamt
    {
      get => noticeCashadjamt ??= new();
      set => noticeCashadjamt = value;
    }

    /// <summary>
    /// A value of NoticeCashchckno.
    /// </summary>
    [JsonPropertyName("noticeCashchckno")]
    public WorkArea NoticeCashchckno
    {
      get => noticeCashchckno ??= new();
      set => noticeCashchckno = value;
    }

    /// <summary>
    /// A value of NoticeOvercoldt.
    /// </summary>
    [JsonPropertyName("noticeOvercoldt")]
    public WorkArea NoticeOvercoldt
    {
      get => noticeOvercoldt ??= new();
      set => noticeOvercoldt = value;
    }

    /// <summary>
    /// A value of NoticeCashcolamt.
    /// </summary>
    [JsonPropertyName("noticeCashcolamt")]
    public WorkArea NoticeCashcolamt
    {
      get => noticeCashcolamt ??= new();
      set => noticeCashcolamt = value;
    }

    /// <summary>
    /// A value of CollAdjDate.
    /// </summary>
    [JsonPropertyName("collAdjDate")]
    public WorkArea CollAdjDate
    {
      get => collAdjDate ??= new();
      set => collAdjDate = value;
    }

    /// <summary>
    /// A value of CollDistDate.
    /// </summary>
    [JsonPropertyName("collDistDate")]
    public WorkArea CollDistDate
    {
      get => collDistDate ??= new();
      set => collDistDate = value;
    }

    /// <summary>
    /// A value of CollAmt.
    /// </summary>
    [JsonPropertyName("collAmt")]
    public WorkArea CollAmt
    {
      get => collAmt ??= new();
      set => collAmt = value;
    }

    /// <summary>
    /// A value of NullWorkArea.
    /// </summary>
    [JsonPropertyName("nullWorkArea")]
    public WorkArea NullWorkArea
    {
      get => nullWorkArea ??= new();
      set => nullWorkArea = value;
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

    private Common noticeOverflow;
    private Common collectionOverflow;
    private Collection collection;
    private WorkArea crdRcvdDate;
    private WorkArea crdCollAmount;
    private WorkArea crvId;
    private WorkArea crsId;
    private WorkArea crtId;
    private WorkArea crdId;
    private DateWorkArea nullDateWorkArea;
    private WorkArea legalActionId;
    private Common exit;
    private WorkArea noticeLactordnum;
    private WorkArea noticeSyscurrdt;
    private WorkArea noticeCashadjamt;
    private WorkArea noticeCashchckno;
    private WorkArea noticeOvercoldt;
    private WorkArea noticeCashcolamt;
    private WorkArea collAdjDate;
    private WorkArea collDistDate;
    private WorkArea collAmt;
    private WorkArea nullWorkArea;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
