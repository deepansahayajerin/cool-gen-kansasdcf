// Program: FN_CALCULATE_MONTHLY_AMOUNT_DUE, ID: 371739755, model: 746.
// Short name: SWE00309
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CALCULATE_MONTHLY_AMOUNT_DUE.
/// </para>
/// <para>
/// RESP: Finance
/// This CAB will adjust amounts due to a monthly amounts due using the 
/// obligation payment schedule
/// </para>
/// </summary>
[Serializable]
public partial class FnCalculateMonthlyAmountDue: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CALCULATE_MONTHLY_AMOUNT_DUE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCalculateMonthlyAmountDue(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCalculateMonthlyAmountDue.
  /// </summary>
  public FnCalculateMonthlyAmountDue(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // Bi-Weekly Count was not being calculated correctly because it was not 
    // taking the DOW due into consideration.  Restructured logic. - E. Parker
    // 02/02/2000
    // ****************************************************************
    if (Equal(import.Period.Date, local.Zero.Date))
    {
      local.Period.Date = Now().Date;
    }
    else
    {
      local.Period.Date = import.Period.Date;
    }

    switch(TrimEnd(import.ObligationPaymentSchedule.FrequencyCode))
    {
      case "BM":
        // : Every other month
        // : See if month is odd or even
        local.OddEven.Count = Month(local.Period.Date) / 2;
        local.OddEven.Count = (int)(Month(local.Period.Date) - (
          long)local.OddEven.Count * 2);

        // : The count will be 1 for odd month or 0 for even month
        switch(AsChar(import.ObligationPaymentSchedule.PeriodInd))
        {
          case 'O':
            // : Odd month payments
            if (local.OddEven.Count == 1)
            {
              export.MonthlyDue.TotalCurrency =
                import.PeriodAmountDue.TotalCurrency;
            }
            else
            {
              export.MonthlyDue.TotalCurrency = 0;
            }

            break;
          case 'E':
            // : Even month payment
            if (local.OddEven.Count == 0)
            {
              export.MonthlyDue.TotalCurrency =
                import.PeriodAmountDue.TotalCurrency;
            }
            else
            {
              export.MonthlyDue.TotalCurrency = 0;
            }

            break;
          default:
            ExitState = "FN0000_INVALID_FREQ_CODE";

            break;
        }

        break;
      case "BW":
        // : Every other week
        switch(import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault())
        {
          case 1:
            local.Payday.Name = "SUNDAY";

            break;
          case 2:
            local.Payday.Name = "MONDAY";

            break;
          case 3:
            local.Payday.Name = "TUESDAY";

            break;
          case 4:
            local.Payday.Name = "WEDNESDAY";

            break;
          case 5:
            local.Payday.Name = "THURSDAY";

            break;
          case 6:
            local.Payday.Name = "FRIDAY";

            break;
          case 7:
            local.Payday.Name = "SATURDAY";

            break;
          default:
            ExitState = "FN0000_INVALID_FREQ_CODE";

            return;
        }

        local.First.Date = IntToDate(Year(local.Period.Date) * 10000 + Month
          (local.Period.Date) * 100 + 1);
        local.Last.Date = AddDays(AddMonths(local.First.Date, 1), -1);

        if (Lt(local.Last.Date, import.ObligationPaymentSchedule.StartDt))
        {
          export.MonthlyDue.TotalCurrency = 0;
        }
        else
        {
          local.Test.Date = import.ObligationPaymentSchedule.StartDt;

          while(!Lt(local.Last.Date, local.Test.Date))
          {
            if (Equal(TrimEnd(DayOfWeek(local.Test.Date)),
              TrimEnd(local.Payday.Name)))
            {
              break;
            }

            local.Test.Date = AddDays(local.Test.Date, 1);
          }

          local.NumberPaydays.Count = 0;

          while(!Lt(local.Last.Date, local.Test.Date))
          {
            if (!Lt(local.Test.Date, local.First.Date))
            {
              ++local.NumberPaydays.Count;
            }

            local.Test.Date = AddDays(local.Test.Date, 14);
          }

          export.MonthlyDue.TotalCurrency =
            import.PeriodAmountDue.TotalCurrency * local.NumberPaydays.Count;
        }

        break;
      case "M":
        // : Monthly
        export.MonthlyDue.TotalCurrency = import.PeriodAmountDue.TotalCurrency;

        break;
      case "SM":
        // : Twice a month
        export.MonthlyDue.TotalCurrency =
          import.PeriodAmountDue.TotalCurrency * 2;

        break;
      case "W":
        // : Weekly
        switch(import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault())
        {
          case 1:
            local.Payday.Name = "SUNDAY";

            break;
          case 2:
            local.Payday.Name = "MONDAY";

            break;
          case 3:
            local.Payday.Name = "TUESDAY";

            break;
          case 4:
            local.Payday.Name = "WEDNESDAY";

            break;
          case 5:
            local.Payday.Name = "THURSDAY";

            break;
          case 6:
            local.Payday.Name = "FRIDAY";

            break;
          case 7:
            local.Payday.Name = "SATURDAY";

            break;
          default:
            ExitState = "FN0000_INVALID_FREQ_CODE";

            return;
        }

        // : Find out how many paydays this month
        local.First.Date = IntToDate(Year(local.Period.Date) * 10000 + Month
          (local.Period.Date) * 100 + 1);
        local.Last.Date = AddMonths(local.First.Date, 1);
        local.Last.Date = AddDays(local.Last.Date, -1);
        local.NumberPaydays.Count = 0;
        local.Test.Date = local.First.Date;

        // : Increment by 1 until first day in month is found
        local.Increment.Count = 1;

        while(!Lt(local.Last.Date, local.Test.Date))
        {
          if (Equal(TrimEnd(DayOfWeek(local.Test.Date)),
            TrimEnd(local.Payday.Name)))
          {
            ++local.NumberPaydays.Count;

            // : Increment by 1 week after first day is found
            local.Increment.Count = 7;
          }

          local.Test.Date = AddDays(local.Test.Date, local.Increment.Count);
        }

        export.MonthlyDue.TotalCurrency =
          import.PeriodAmountDue.TotalCurrency * local.NumberPaydays.Count;

        break;
      case "":
        break;
      default:
        ExitState = "FN0000_INVALID_FREQ_CODE";

        break;
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of PeriodAmountDue.
    /// </summary>
    [JsonPropertyName("periodAmountDue")]
    public Common PeriodAmountDue
    {
      get => periodAmountDue ??= new();
      set => periodAmountDue = value;
    }

    /// <summary>
    /// A value of Period.
    /// </summary>
    [JsonPropertyName("period")]
    public DateWorkArea Period
    {
      get => period ??= new();
      set => period = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private Common periodAmountDue;
    private DateWorkArea period;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MonthlyDue.
    /// </summary>
    [JsonPropertyName("monthlyDue")]
    public Common MonthlyDue
    {
      get => monthlyDue ??= new();
      set => monthlyDue = value;
    }

    private Common monthlyDue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of ComputedPayPeriod.
    /// </summary>
    [JsonPropertyName("computedPayPeriod")]
    public Common ComputedPayPeriod
    {
      get => computedPayPeriod ??= new();
      set => computedPayPeriod = value;
    }

    /// <summary>
    /// A value of LastPayDayBefore1StDt.
    /// </summary>
    [JsonPropertyName("lastPayDayBefore1StDt")]
    public DateWorkArea LastPayDayBefore1StDt
    {
      get => lastPayDayBefore1StDt ??= new();
      set => lastPayDayBefore1StDt = value;
    }

    /// <summary>
    /// A value of NoOfPayPpAsOf1StDay.
    /// </summary>
    [JsonPropertyName("noOfPayPpAsOf1StDay")]
    public Common NoOfPayPpAsOf1StDay
    {
      get => noOfPayPpAsOf1StDay ??= new();
      set => noOfPayPpAsOf1StDay = value;
    }

    /// <summary>
    /// A value of FirstPayment.
    /// </summary>
    [JsonPropertyName("firstPayment")]
    public DateWorkArea FirstPayment
    {
      get => firstPayment ??= new();
      set => firstPayment = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Period.
    /// </summary>
    [JsonPropertyName("period")]
    public DateWorkArea Period
    {
      get => period ??= new();
      set => period = value;
    }

    /// <summary>
    /// A value of PayPeriod.
    /// </summary>
    [JsonPropertyName("payPeriod")]
    public Common PayPeriod
    {
      get => payPeriod ??= new();
      set => payPeriod = value;
    }

    /// <summary>
    /// A value of CurrentDayNumber.
    /// </summary>
    [JsonPropertyName("currentDayNumber")]
    public Common CurrentDayNumber
    {
      get => currentDayNumber ??= new();
      set => currentDayNumber = value;
    }

    /// <summary>
    /// A value of StartDayNumber.
    /// </summary>
    [JsonPropertyName("startDayNumber")]
    public Common StartDayNumber
    {
      get => startDayNumber ??= new();
      set => startDayNumber = value;
    }

    /// <summary>
    /// A value of Increment.
    /// </summary>
    [JsonPropertyName("increment")]
    public Common Increment
    {
      get => increment ??= new();
      set => increment = value;
    }

    /// <summary>
    /// A value of Payday.
    /// </summary>
    [JsonPropertyName("payday")]
    public DayArea Payday
    {
      get => payday ??= new();
      set => payday = value;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public DateWorkArea Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of NumberPaydays.
    /// </summary>
    [JsonPropertyName("numberPaydays")]
    public Common NumberPaydays
    {
      get => numberPaydays ??= new();
      set => numberPaydays = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DateWorkArea Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public DateWorkArea First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of OddEven.
    /// </summary>
    [JsonPropertyName("oddEven")]
    public Common OddEven
    {
      get => oddEven ??= new();
      set => oddEven = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      computedPayPeriod = null;
      lastPayDayBefore1StDt = null;
      noOfPayPpAsOf1StDay = null;
      firstPayment = null;
      zero = null;
      period = null;
      payPeriod = null;
      currentDayNumber = null;
      startDayNumber = null;
      increment = null;
      test = null;
      numberPaydays = null;
      last = null;
      first = null;
      oddEven = null;
    }

    private Common computedPayPeriod;
    private DateWorkArea lastPayDayBefore1StDt;
    private Common noOfPayPpAsOf1StDay;
    private DateWorkArea firstPayment;
    private DateWorkArea zero;
    private DateWorkArea period;
    private Common payPeriod;
    private Common currentDayNumber;
    private Common startDayNumber;
    private Common increment;
    private DayArea payday;
    private DateWorkArea test;
    private Common numberPaydays;
    private DateWorkArea last;
    private DateWorkArea first;
    private Common oddEven;
  }
#endregion
}
