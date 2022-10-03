// Program: FN_B650_WRITE_OUT_COLLECTION_ERR, ID: 372896450, model: 746.
// Short name: SWE02491
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B650_WRITE_OUT_COLLECTION_ERR.
/// </summary>
[Serializable]
public partial class FnB650WriteOutCollectionErr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_WRITE_OUT_COLLECTION_ERR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650WriteOutCollectionErr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650WriteOutCollectionErr.
  /// </summary>
  public FnB650WriteOutCollectionErr(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ---------------------------------------------
    // 091200  Fangman  103323  Changed code to display the cash non-cash 
    // indicator.  This was put in with the changes to fix the disb suppr with
    // past discontinue dates.
    // 11/12/03  Fangman  302055   Add a space to an error report line so that 
    // the SAS pgm can read it more easily.
    // ---------------------------------------------
    // 12/12/14  LSS  Q45935   Add the associated court order number and 
    // judicial district  to each error message data block in the disbursement
    // credits error report.
    // -----------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";

    if (AsChar(import.Collection.AdjustedInd) == 'Y')
    {
      local.Sign.Text2 = " -";

      // For adjusted collections the supported person & DDDD are not read in 
      // the AB to determine the Obligee.
      if (import.ObligationType.SystemGeneratedIdentifier != import
        .Hardcode.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier && import
        .ObligationType.SystemGeneratedIdentifier != import
        .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier)
      {
        if (ReadCsePerson())
        {
          local.Child.Number = entities.Supported1.Number;
        }
        else
        {
          // Continue
        }
      }

      if (import.ObligationType.SystemGeneratedIdentifier == import
        .Hardcode.HardcodeVoluntary.SystemGeneratedIdentifier)
      {
        local.DdddFormattedDate.Text10 = "Voluntary";
      }
      else if (ReadDebtDetail())
      {
        local.DdddUnformattedDate.Date = entities.DebtDetail.DueDt;
        UseCabFormatDate2();
      }
      else
      {
        // Continue
      }
    }
    else
    {
      local.Sign.Text2 = "";
      local.Child.Number = import.Child.Number;
      local.DdddUnformattedDate.Date = import.DebtDetail.DueDt;
      UseCabFormatDate2();
    }

    UseEabExtractExitStateMessage();
    local.FormattedAmount.Text10 =
      NumberToString((long)import.Collection.Amount, 9, 7) + "." + NumberToString
      ((long)(import.Collection.Amount * 100), 14, 2);
    local.CollUnformattedDate.Date = import.Collection.CollectionDt;
    UseCabFormatDate1();
    local.EabReportSend.RptDetail = "Coll  " + NumberToString
      (import.Collection.SystemGeneratedIdentifier, 7, 9) + "  " + import
      .Collection.ProgramAppliedTo + "  " + (
        import.Collection.DistPgmStateAppldTo ?? "") + "  " + local
      .Sign.Text2 + local.FormattedAmount.Text10 + "  " + local
      .CollFormattedDate.Text10 + "  " + local.ExitStateWorkArea.Message;
    UseCabErrorReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Obligor " + import.Obligor.Number + "  Oblig # " +
      NumberToString(import.Obligation.SystemGeneratedIdentifier, 13, 3) + "  " +
      import.ObligationType.Code + "  Debt # " + NumberToString
      (import.Per.SystemGeneratedIdentifier, 7, 9) + "  DDDD " + local
      .DdddFormattedDate.Text10;
    UseCabErrorReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (AsChar(import.CashReceiptType.CategoryIndicator) == 'C')
    {
      local.CashNonCashText.Text10 = " Cash";
    }
    else
    {
      local.CashNonCashText.Text10 = " Non-Cash";
    }

    local.EabReportSend.RptDetail = "Cash Rcpt Src Type " + NumberToString
      (import.CashReceiptSourceType.SystemGeneratedIdentifier, 13, 3) + "  Event # " +
      NumberToString
      (import.CashReceiptEvent.SystemGeneratedIdentifier, 7, 9) + "  Rcpt # " +
      NumberToString(import.CashReceipt.SequentialNumber, 7, 9) + "  Dtl # " + NumberToString
      (import.CashReceiptDetail.SequentialIdentifier, 12, 4) + local
      .CashNonCashText.Text10;
    UseCabErrorReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (!IsEmpty(import.Obligee.Number) || !IsEmpty(import.Case1.Number) || !
      IsEmpty(import.Child.Number))
    {
      local.EabReportSend.RptDetail = "";
      local.EabReportSend.RptDetail = "Obligee " + import.Obligee.Number + "  Case # " +
        import.Case1.Number + "  Child # " + local.Child.Number;
      UseCabErrorReport();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // CQ49535 - added Court Order Number and Judicial District to error message
    if (ReadTribunalLegalAction())
    {
      local.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
      local.Tribunal.JudicialDistrict = entities.Tribunal.JudicialDistrict;
      local.EabReportSend.RptDetail = "Court Order Number  " + (
        local.LegalAction.StandardNumber ?? "") + "Judicial District  " + local
        .Tribunal.JudicialDistrict;
      UseCabErrorReport();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormatDate1()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.CollUnformattedDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.CollFormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDate2()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DdddUnformattedDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.DdddFormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Per.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported1.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Per.OtyType);
        db.SetInt32(command, "obgGeneratedId", import.Per.ObgGeneratedId);
        db.SetString(command, "otrType", import.Per.Type1);
        db.SetInt32(
          command, "otrGeneratedId", import.Per.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.Per.CpaType);
        db.SetString(command, "cspNumber", import.Per.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadTribunalLegalAction()
  {
    entities.Tribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadTribunalLegalAction",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Identifier = db.GetInt32(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.Tribunal.Populated = true;
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
    /// <summary>A HardcodeGroup group.</summary>
    [Serializable]
    public class HardcodeGroup
    {
      /// <summary>
      /// A value of HardcodeSpousalSupport.
      /// </summary>
      [JsonPropertyName("hardcodeSpousalSupport")]
      public ObligationType HardcodeSpousalSupport
      {
        get => hardcodeSpousalSupport ??= new();
        set => hardcodeSpousalSupport = value;
      }

      /// <summary>
      /// A value of HardcodeSpArrearsJudgmt.
      /// </summary>
      [JsonPropertyName("hardcodeSpArrearsJudgmt")]
      public ObligationType HardcodeSpArrearsJudgmt
      {
        get => hardcodeSpArrearsJudgmt ??= new();
        set => hardcodeSpArrearsJudgmt = value;
      }

      /// <summary>
      /// A value of HardcodeVoluntary.
      /// </summary>
      [JsonPropertyName("hardcodeVoluntary")]
      public ObligationType HardcodeVoluntary
      {
        get => hardcodeVoluntary ??= new();
        set => hardcodeVoluntary = value;
      }

      private ObligationType hardcodeSpousalSupport;
      private ObligationType hardcodeSpArrearsJudgmt;
      private ObligationType hardcodeVoluntary;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public ObligationTransaction Per
    {
      get => per ??= new();
      set => per = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// Gets a value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public HardcodeGroup Hardcode
    {
      get => hardcode ?? (hardcode = new());
      set => hardcode = value;
    }

    private Collection collection;
    private CsePerson obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction per;
    private DebtDetail debtDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson obligee;
    private CsePerson child;
    private Case1 case1;
    private HardcodeGroup hardcode;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public TextWorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
    }

    /// <summary>
    /// A value of CashNonCashText.
    /// </summary>
    [JsonPropertyName("cashNonCashText")]
    public TextWorkArea CashNonCashText
    {
      get => cashNonCashText ??= new();
      set => cashNonCashText = value;
    }

    /// <summary>
    /// A value of DdddUnformattedDate.
    /// </summary>
    [JsonPropertyName("ddddUnformattedDate")]
    public DateWorkArea DdddUnformattedDate
    {
      get => ddddUnformattedDate ??= new();
      set => ddddUnformattedDate = value;
    }

    /// <summary>
    /// A value of DdddFormattedDate.
    /// </summary>
    [JsonPropertyName("ddddFormattedDate")]
    public WorkArea DdddFormattedDate
    {
      get => ddddFormattedDate ??= new();
      set => ddddFormattedDate = value;
    }

    /// <summary>
    /// A value of CollUnformattedDate.
    /// </summary>
    [JsonPropertyName("collUnformattedDate")]
    public DateWorkArea CollUnformattedDate
    {
      get => collUnformattedDate ??= new();
      set => collUnformattedDate = value;
    }

    /// <summary>
    /// A value of CollFormattedDate.
    /// </summary>
    [JsonPropertyName("collFormattedDate")]
    public WorkArea CollFormattedDate
    {
      get => collFormattedDate ??= new();
      set => collFormattedDate = value;
    }

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
    /// A value of FormattedAmount.
    /// </summary>
    [JsonPropertyName("formattedAmount")]
    public TextWorkArea FormattedAmount
    {
      get => formattedAmount ??= new();
      set => formattedAmount = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private LegalAction legalAction;
    private Tribunal tribunal;
    private CsePerson child;
    private TextWorkArea sign;
    private TextWorkArea cashNonCashText;
    private DateWorkArea ddddUnformattedDate;
    private WorkArea ddddFormattedDate;
    private DateWorkArea collUnformattedDate;
    private WorkArea collFormattedDate;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea formattedAmount;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private Tribunal tribunal;
    private LegalAction legalAction;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private DebtDetail debtDetail;
  }
#endregion
}
