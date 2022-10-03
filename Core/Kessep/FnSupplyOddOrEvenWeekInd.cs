// Program: FN_SUPPLY_ODD_OR_EVEN_WEEK_IND, ID: 371994245, model: 746.
// Short name: SWE00615
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_SUPPLY_ODD_OR_EVEN_WEEK_IND.
/// </para>
/// <para>
/// This action block will calculate whether a due week is even or odd, for 
/// frequency period processing.
/// </para>
/// </summary>
[Serializable]
public partial class FnSupplyOddOrEvenWeekInd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SUPPLY_ODD_OR_EVEN_WEEK_IND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSupplyOddOrEvenWeekInd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSupplyOddOrEvenWeekInd.
  /// </summary>
  public FnSupplyOddOrEvenWeekInd(IContext context, Import import, Export export)
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
    // : Find the beginning of the first full week of the year.
    local.DateWorkArea.Date =
      IntToDate(Year(import.AccrualInstructions.AsOfDt) * 10000 + 101);
    local.DayOfWeek.Text11 = DayOfWeek(local.DateWorkArea.Date);

    while(!Equal(local.DayOfWeek.Text11, "SUNDAY"))
    {
      local.DateWorkArea.Date = AddDays(local.DateWorkArea.Date, 1);
      local.DayOfWeek.Text11 = DayOfWeek(local.DateWorkArea.Date);
    }

    // Calculate the number of days from the first week
    // to the AS OF DATE entered
    local.Juldate1.TotalInteger = DateToJulianNumber(local.DateWorkArea.Date);
    local.Juldate2.TotalInteger =
      DateToJulianNumber(import.AccrualInstructions.AsOfDt) - local
      .Juldate1.TotalInteger;

    if (local.Juldate2.TotalInteger < 0)
    {
      export.ObligationPaymentSchedule.PeriodInd = "E";

      return;
    }

    // Calculate the week number in which the AS OF DATE falls
    local.WeekNumber.TotalInteger =
      (long)Math.Round((decimal)local.Juldate2.TotalInteger / 7
      + 0.5M, MidpointRounding.AwayFromZero);

    // Evaluate week number for Odd or Even
    local.WeekNumber.TotalReal = (decimal)local.WeekNumber.TotalInteger / 2;
    local.WeekNumber.TotalInteger = (long)local.WeekNumber.TotalReal;

    if (local.WeekNumber.TotalReal - local.WeekNumber.TotalInteger != 0)
    {
      export.ObligationPaymentSchedule.PeriodInd = "O";
    }
    else
    {
      export.ObligationPaymentSchedule.PeriodInd = "E";
    }
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    private AccrualInstructions accrualInstructions;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of DayOfWeek.
    /// </summary>
    [JsonPropertyName("dayOfWeek")]
    public WorkArea DayOfWeek
    {
      get => dayOfWeek ??= new();
      set => dayOfWeek = value;
    }

    /// <summary>
    /// A value of Juldate1.
    /// </summary>
    [JsonPropertyName("juldate1")]
    public Common Juldate1
    {
      get => juldate1 ??= new();
      set => juldate1 = value;
    }

    /// <summary>
    /// A value of Juldate2.
    /// </summary>
    [JsonPropertyName("juldate2")]
    public Common Juldate2
    {
      get => juldate2 ??= new();
      set => juldate2 = value;
    }

    /// <summary>
    /// A value of WeekNumber.
    /// </summary>
    [JsonPropertyName("weekNumber")]
    public Common WeekNumber
    {
      get => weekNumber ??= new();
      set => weekNumber = value;
    }

    private DateWorkArea dateWorkArea;
    private WorkArea dayOfWeek;
    private Common juldate1;
    private Common juldate2;
    private Common weekNumber;
  }
#endregion
}
