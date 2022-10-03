// Program: FN_B647_MAINLINE, ID: 372995795, model: 746.
// Short name: SWE02396
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B647_MAINLINE.
/// </summary>
[Serializable]
public partial class FnB647Mainline: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B647_MAINLINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB647Mainline(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB647Mainline.
  /// </summary>
  public FnB647Mainline(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************
    // * Event Id and Reason Code are used to identify EVENT and *
    // * EVENT DETAIL
    // 
    // *
    // ***********************************************************
    local.Infrastructure.EventId = import.Event1.ControlNumber;
    local.Infrastructure.ReasonCode = import.EventDetail.ReasonCode;

    // ***********************************************************
    // * Function, Event Type and Event Detail Name are supplied *
    // * from EVENT and EVENT DETAIL.                            *
    // ***********************************************************
    // ***********************************************************
    // * Process Status can be:
    // 
    // *
    // *  Q = ready to be processed by Event Processor           *
    // *  O = in process
    // 
    // *
    // *  H = for History (will appear on History screen)        *
    // *  P = for Processed (will not appear on History screen)  *
    // *  E = error
    // 
    // *
    // *  X = top secret
    // 
    // *
    // ***********************************************************
    local.Infrastructure.ProcessStatus = "H";
    local.Infrastructure.SituationNumber = 1;
    local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
    local.Infrastructure.ReferenceDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.Infrastructure.BusinessObjectCd = "OBG";

    if (AsChar(import.CouponIndicator.Flag) == 'Y')
    {
      local.CpnText.Text4 = "with";
    }
    else
    {
      local.CpnText.Text4 = "w/o";
    }

    local.Infrastructure.Detail = local.CpnText.Text4 + " coupons, stmt date: " +
      import.Month.Text10 + " " + "" + TrimEnd(import.CourtCase.Text30) + " " +
      NumberToString(DateToInt(local.Infrastructure.ReferenceDate), 8, 8);
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Infrastructure.Function = "O";
    local.Infrastructure.UserId = import.ProgramProcessingInfo.Name;
    UseSpCabCreateInfrastructure();
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
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
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

    /// <summary>
    /// A value of CouponIndicator.
    /// </summary>
    [JsonPropertyName("couponIndicator")]
    public Common CouponIndicator
    {
      get => couponIndicator ??= new();
      set => couponIndicator = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of CourtCase.
    /// </summary>
    [JsonPropertyName("courtCase")]
    public TextWorkArea CourtCase
    {
      get => courtCase ??= new();
      set => courtCase = value;
    }

    private Event1 event1;
    private EventDetail eventDetail;
    private Common couponIndicator;
    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private TextWorkArea month;
    private TextWorkArea year;
    private TextWorkArea courtCase;
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
    /// A value of CpnText.
    /// </summary>
    [JsonPropertyName("cpnText")]
    public TextWorkArea CpnText
    {
      get => cpnText ??= new();
      set => cpnText = value;
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

    private TextWorkArea cpnText;
    private Infrastructure infrastructure;
  }
#endregion
}
