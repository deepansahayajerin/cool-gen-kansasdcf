// Program: FN_VALIDATE_FREQUENCY_INFO, ID: 371993423, model: 746.
// Short name: SWE00684
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_VALIDATE_FREQUENCY_INFO.
/// </para>
/// <para>
/// This action block sets a literal based on frequency code (ie &quot;W&quot; 
/// input will set literal to &quot;WEEKLY&quot;).
/// It also validates the frequency code, and edits the input day1 and day2 (
/// which represent day of week or day of month1 and day of month2).  If day1 is
/// not entered, a default will be set using the import AS_OF_DATE.
/// As well, the period indicator is set to &quot;O&quot;dd or &quot;E&quot;ven 
/// if the frequency is semi-monthly or semi-weekly.
/// </para>
/// </summary>
[Serializable]
public partial class FnValidateFrequencyInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VALIDATE_FREQUENCY_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnValidateFrequencyInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnValidateFrequencyInfo.
  /// </summary>
  public FnValidateFrequencyInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ObligationPaymentSchedule.FrequencyCode =
      import.ObligationPaymentSchedule.FrequencyCode;
    export.Day1.Count = import.Day1.Count;
    export.Day2.Count = import.Day2.Count;

    // - Validate day of week for Weekly and Semi-weekly frequencies.
    // - Set default day of week based on accrual instructions, if not entered
    if (Equal(import.ObligationPaymentSchedule.FrequencyCode, "W") || Equal
      (import.ObligationPaymentSchedule.FrequencyCode, "SW") || Equal
      (import.ObligationPaymentSchedule.FrequencyCode, "BW"))
    {
      if (import.Day1.Count == 0)
      {
        // : Default day of week to day number of AS_OF_DATE
        local.DayOfWeek.Text11 =
          DayOfWeek(import.ObligationPaymentSchedule.StartDt);

        switch(TrimEnd(local.DayOfWeek.Text11))
        {
          case "SUNDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 1;

            break;
          case "MONDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 2;

            break;
          case "TUESDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 3;

            break;
          case "WEDNESDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 4;

            break;
          case "THURSDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 5;

            break;
          case "FRIDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 6;

            break;
          case "SATURDAY":
            export.ObligationPaymentSchedule.DayOfWeek = 7;

            break;
          default:
            goto Test;
        }

        export.Day1.Count =
          export.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
      }
      else if (import.Day1.Count > 7)
      {
        ExitState = "INVALID_DAY_OF_WEEK";
        export.ErrorDay1.Count = 9;

        return;
      }
      else
      {
        export.ObligationPaymentSchedule.DayOfWeek = import.Day1.Count;
      }

Test:

      // Day 2 must be blank for Weekly and Semi Weekly frequencies
      if (import.Day2.Count != 0)
      {
        ExitState = "INVALID_DAY_2_MUST_BE_ZERO";
        export.ErrorDay2.Count = 9;

        return;
      }
    }
    else
    {
      // Frequency is Monthly, Bi Monthly, Semi Monthly or Quarterly.
      // If Day 1 not entered, set default using Accrual Instructions As Of Date
      if (import.Day1.Count == 0)
      {
        // : Default day of month to DD of AS_OF_DATE
        export.ObligationPaymentSchedule.DayOfMonth1 =
          Day(import.ObligationPaymentSchedule.StartDt);
      }
      else
      {
        export.ObligationPaymentSchedule.DayOfMonth1 = import.Day1.Count;
      }
    }

    // Set the screen literal for the input frequency code.
    // Edit each frequency.  Fields in error are flagged in
    // export views
    switch(TrimEnd(import.ObligationPaymentSchedule.FrequencyCode))
    {
      case "SW":
        export.WorkArea.Text13 = "SEMI-WEEKLY";
        export.ObligationPaymentSchedule.PeriodInd =
          UseFnSupplyOddOrEvenWeekInd();

        break;
      case "BM":
        export.WorkArea.Text13 = "BI-MONTHLY";

        if (export.Day2.Count != 0)
        {
          ExitState = "INVALID_DAY_2_MUST_BE_ZERO";
          export.ErrorDay2.Count = 9;

          return;
        }

        switch(Month(import.ObligationPaymentSchedule.StartDt))
        {
          case 2:
            export.ObligationPaymentSchedule.PeriodInd = "E";

            break;
          case 4:
            export.ObligationPaymentSchedule.PeriodInd = "E";

            break;
          case 6:
            export.ObligationPaymentSchedule.PeriodInd = "E";

            break;
          case 8:
            export.ObligationPaymentSchedule.PeriodInd = "E";

            break;
          case 10:
            export.ObligationPaymentSchedule.PeriodInd = "E";

            break;
          case 12:
            export.ObligationPaymentSchedule.PeriodInd = "E";

            break;
          default:
            export.ObligationPaymentSchedule.PeriodInd = "O";

            break;
        }

        break;
      case "SM":
        export.WorkArea.Text13 = "SEMI-MONTHLY";

        if (export.Day2.Count == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          export.ErrorDay2.Count = 9;
        }
        else if (export.Day2.Count > 31)
        {
          ExitState = "INVALID_DAY_OF_MONTH";
          export.ErrorDay2.Count = 9;
        }

        if (export.Day1.Count > 31)
        {
          ExitState = "INVALID_DAY_OF_MONTH";
          export.ErrorDay1.Count = 9;
        }
        else if (export.Day1.Count == export.Day2.Count)
        {
          ExitState = "DAY1_AND_DAY2_CANNOT_BE_THE_SAME";
          export.ErrorDay1.Count = 9;
          export.ErrorDay2.Count = 9;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ** ALL OK **
          export.ObligationPaymentSchedule.DayOfMonth2 = import.Day2.Count;
        }

        break;
      case "W":
        export.WorkArea.Text13 = "WEEKLY";

        if (export.Day2.Count != 0)
        {
          ExitState = "INVALID_DAY_2_MUST_BE_ZERO";
          export.ErrorDay2.Count = 9;
        }

        break;
      case "Q":
        export.WorkArea.Text13 = "QUARTERLY";

        if (export.Day1.Count > 31)
        {
          ExitState = "INVALID_DAY_OF_MONTH";
          export.ErrorDay1.Count = 9;
        }

        if (export.Day2.Count != 0)
        {
          ExitState = "INVALID_DAY_2_MUST_BE_ZERO";
          export.ErrorDay2.Count = 9;
        }

        break;
      case "M":
        export.WorkArea.Text13 = "MONTHLY";

        if (export.Day1.Count > 31)
        {
          ExitState = "INVALID_DAY_OF_MONTH";
          export.ErrorDay1.Count = 9;
        }

        if (export.Day2.Count != 0)
        {
          ExitState = "INVALID_DAY_2_MUST_BE_ZERO";
          export.ErrorDay2.Count = 9;
        }

        break;
      default:
        ExitState = "FN0000_INVALID_FREQ_CODE";
        export.Error.FrequencyCode = "X";

        break;
    }
  }

  private string UseFnSupplyOddOrEvenWeekInd()
  {
    var useImport = new FnSupplyOddOrEvenWeekInd.Import();
    var useExport = new FnSupplyOddOrEvenWeekInd.Export();

    Call(FnSupplyOddOrEvenWeekInd.Execute, useImport, useExport);

    return useExport.ObligationPaymentSchedule.PeriodInd ?? "";
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of Day1.
    /// </summary>
    [JsonPropertyName("day1")]
    public Common Day1
    {
      get => day1 ??= new();
      set => day1 = value;
    }

    /// <summary>
    /// A value of Day2.
    /// </summary>
    [JsonPropertyName("day2")]
    public Common Day2
    {
      get => day2 ??= new();
      set => day2 = value;
    }

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Common day1;
    private Common day2;
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

    /// <summary>
    /// A value of Day1.
    /// </summary>
    [JsonPropertyName("day1")]
    public Common Day1
    {
      get => day1 ??= new();
      set => day1 = value;
    }

    /// <summary>
    /// A value of Day2.
    /// </summary>
    [JsonPropertyName("day2")]
    public Common Day2
    {
      get => day2 ??= new();
      set => day2 = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of ErrorDay1.
    /// </summary>
    [JsonPropertyName("errorDay1")]
    public Common ErrorDay1
    {
      get => errorDay1 ??= new();
      set => errorDay1 = value;
    }

    /// <summary>
    /// A value of ErrorDay2.
    /// </summary>
    [JsonPropertyName("errorDay2")]
    public Common ErrorDay2
    {
      get => errorDay2 ??= new();
      set => errorDay2 = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public ObligationPaymentSchedule Error
    {
      get => error ??= new();
      set => error = value;
    }

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Common day1;
    private Common day2;
    private WorkArea workArea;
    private Common errorDay1;
    private Common errorDay2;
    private ObligationPaymentSchedule error;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WeekNumber.
    /// </summary>
    [JsonPropertyName("weekNumber")]
    public Common WeekNumber
    {
      get => weekNumber ??= new();
      set => weekNumber = value;
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

    private Common weekNumber;
    private Common juldate1;
    private Common juldate2;
    private DateWorkArea dateWorkArea;
    private WorkArea dayOfWeek;
  }
#endregion
}
