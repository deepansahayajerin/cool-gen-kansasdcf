// Program: FN_B643_DETERMINE_COUPONS, ID: 372764860, model: 746.
// Short name: SWE02465
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_DETERMINE_COUPONS.
/// </summary>
[Serializable]
public partial class FnB643DetermineCoupons: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_DETERMINE_COUPONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643DetermineCoupons(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643DetermineCoupons.
  /// </summary>
  public FnB643DetermineCoupons(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.DateValidation.Year = Year(import.CouponEnd.Date);
    local.DateValidation.Month = Month(import.CouponEnd.Date);
    local.DateValidation.MaximumNumberOfDaysInMonth =
      Day(import.CouponEnd.Date);
    local.Finished.Flag = "N";

    switch(TrimEnd(import.ObligationPaymentSchedule.FrequencyCode))
    {
      case "BW":
        // <<<<< Bi Weekly >>>>>
        // *****  Set the Text of the day of the week that the obligation is 
        // due.
        switch(import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault())
        {
          case 1:
            local.AccrualWorkArea.DayOfWeekText = "SUNDAY";

            break;
          case 2:
            local.AccrualWorkArea.DayOfWeekText = "MONDAY";

            break;
          case 3:
            local.AccrualWorkArea.DayOfWeekText = "TUESDAY";

            break;
          case 4:
            local.AccrualWorkArea.DayOfWeekText = "WEDNESDAY";

            break;
          case 5:
            local.AccrualWorkArea.DayOfWeekText = "THURSDAY";

            break;
          case 6:
            local.AccrualWorkArea.DayOfWeekText = "FRIDAY";

            break;
          case 7:
            local.AccrualWorkArea.DayOfWeekText = "SATURDAY";

            break;
          default:
            local.AccrualWorkArea.DayOfWeekText = "FRIDAY";

            break;
        }

        // *****  Find the date of each "Day of the Week" during the processing 
        // month.
        local.DateValidation.Day = 1;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          local.Due.Date = IntToDate(local.DateValidation.Year * 10000 + local
            .DateValidation.Month * 100 + local.DateValidation.Day);

          if (Equal(DayOfWeek(local.Due.Date),
            local.AccrualWorkArea.DayOfWeekText))
          {
            if (!Lt(local.Due.Date, import.CouponBegin.Date) && !
              Lt(import.CouponEnd.Date, local.Due.Date))
            {
              export.Export1.Update.DebtDetail.DueDt = local.Due.Date;
            }

            // *****  Once the first due date is found, increment by a week to 
            // the next due date.
            local.DateValidation.Day += 14;
          }
          else
          {
            ++local.DateValidation.Day;
          }

          if (local.DateValidation.Day > local
            .DateValidation.MaximumNumberOfDaysInMonth)
          {
            local.Finished.Flag = "Y";
          }

          export.Export1.Next();
        }
        while(AsChar(local.Finished.Flag) != 'Y');

        break;
      case "W":
        // <<<<< Weekly >>>>>
        // *****  Set the Text of the day of the week that the obligation is 
        // due.
        switch(import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault())
        {
          case 1:
            local.AccrualWorkArea.DayOfWeekText = "SUNDAY";

            break;
          case 2:
            local.AccrualWorkArea.DayOfWeekText = "MONDAY";

            break;
          case 3:
            local.AccrualWorkArea.DayOfWeekText = "TUESDAY";

            break;
          case 4:
            local.AccrualWorkArea.DayOfWeekText = "WEDNESDAY";

            break;
          case 5:
            local.AccrualWorkArea.DayOfWeekText = "THURSDAY";

            break;
          case 6:
            local.AccrualWorkArea.DayOfWeekText = "FRIDAY";

            break;
          case 7:
            local.AccrualWorkArea.DayOfWeekText = "SATURDAY";

            break;
          default:
            local.AccrualWorkArea.DayOfWeekText = "FRIDAY";

            break;
        }

        // *****  Find the date of each "Day of the Week" during the processing 
        // month.
        local.DateValidation.Day = 1;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          local.Due.Date = IntToDate(local.DateValidation.Year * 10000 + local
            .DateValidation.Month * 100 + local.DateValidation.Day);

          if (Equal(DayOfWeek(local.Due.Date),
            local.AccrualWorkArea.DayOfWeekText))
          {
            if (!Lt(local.Due.Date, import.CouponBegin.Date) && !
              Lt(import.CouponEnd.Date, local.Due.Date))
            {
              export.Export1.Update.DebtDetail.DueDt = local.Due.Date;
            }

            // *****  Once the first due date is found, increment by a week to 
            // the next due date.
            local.DateValidation.Day += 7;
          }
          else
          {
            ++local.DateValidation.Day;
          }

          if (local.DateValidation.Day > local
            .DateValidation.MaximumNumberOfDaysInMonth)
          {
            local.Finished.Flag = "Y";
          }

          export.Export1.Next();
        }
        while(AsChar(local.Finished.Flag) != 'Y');

        break;
      default:
        break;
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
    /// A value of CouponBegin.
    /// </summary>
    [JsonPropertyName("couponBegin")]
    public DateWorkArea CouponBegin
    {
      get => couponBegin ??= new();
      set => couponBegin = value;
    }

    /// <summary>
    /// A value of CouponEnd.
    /// </summary>
    [JsonPropertyName("couponEnd")]
    public DateWorkArea CouponEnd
    {
      get => couponEnd ??= new();
      set => couponEnd = value;
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

    private DateWorkArea couponBegin;
    private DateWorkArea couponEnd;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private DebtDetail debtDetail;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateValidation.
    /// </summary>
    [JsonPropertyName("dateValidation")]
    public DateValidation DateValidation
    {
      get => dateValidation ??= new();
      set => dateValidation = value;
    }

    /// <summary>
    /// A value of LastDateOfMonth.
    /// </summary>
    [JsonPropertyName("lastDateOfMonth")]
    public DateWorkArea LastDateOfMonth
    {
      get => lastDateOfMonth ??= new();
      set => lastDateOfMonth = value;
    }

    /// <summary>
    /// A value of Finished.
    /// </summary>
    [JsonPropertyName("finished")]
    public Common Finished
    {
      get => finished ??= new();
      set => finished = value;
    }

    /// <summary>
    /// A value of AccrualWorkArea.
    /// </summary>
    [JsonPropertyName("accrualWorkArea")]
    public AccrualWorkArea AccrualWorkArea
    {
      get => accrualWorkArea ??= new();
      set => accrualWorkArea = value;
    }

    /// <summary>
    /// A value of Due.
    /// </summary>
    [JsonPropertyName("due")]
    public DateWorkArea Due
    {
      get => due ??= new();
      set => due = value;
    }

    private DateValidation dateValidation;
    private DateWorkArea lastDateOfMonth;
    private Common finished;
    private AccrualWorkArea accrualWorkArea;
    private DateWorkArea due;
  }
#endregion
}
