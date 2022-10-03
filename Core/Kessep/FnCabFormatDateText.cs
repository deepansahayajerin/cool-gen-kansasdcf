// Program: FN_CAB_FORMAT_DATE_TEXT, ID: 372117108, model: 746.
// Short name: SWE02189
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_FORMAT_DATE_TEXT.
/// </para>
/// <para>
/// RESP: FIN
/// This common action block takes a date in ief date format and returns
/// a text representing that date in a selected format.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabFormatDateText: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_FORMAT_DATE_TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabFormatDateText(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabFormatDateText.
  /// </summary>
  public FnCabFormatDateText(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Syed Hasan, MTW    01-11-98    Initial Coding
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    local.InputDateMonth.NumericalMonth = Month(import.DateWorkArea.Date);

    switch(local.InputDateMonth.NumericalMonth)
    {
      case 1:
        local.MonthName.Text10 = "January";

        break;
      case 2:
        local.MonthName.Text10 = "February";

        break;
      case 3:
        local.MonthName.Text10 = "March";

        break;
      case 4:
        local.MonthName.Text10 = "April";

        break;
      case 5:
        local.MonthName.Text10 = "May";

        break;
      case 6:
        local.MonthName.Text10 = "June";

        break;
      case 7:
        local.MonthName.Text10 = "July";

        break;
      case 8:
        local.MonthName.Text10 = "August";

        break;
      case 9:
        local.MonthName.Text10 = "September";

        break;
      case 10:
        local.MonthName.Text10 = "October";

        break;
      case 11:
        local.MonthName.Text10 = "November";

        break;
      case 12:
        local.MonthName.Text10 = "December";

        break;
      default:
        break;
    }

    switch(AsChar(import.DateTextStyle.SelectChar))
    {
      case 'A':
        // ----> Style "A" returns text in the format 'Month DD, YYYY'
        //      -Syed Hasan,MTW 01-18-98
        local.InputDateDay.NumericalDay = Day(import.DateWorkArea.Date);

        if (local.InputDateDay.NumericalDay < 10)
        {
          local.InputDateDay.TextDay =
            NumberToString(local.InputDateDay.NumericalDay, 15, 1);
        }
        else
        {
          local.InputDateDay.TextDay =
            NumberToString(local.InputDateDay.NumericalDay, 14, 2);
        }

        local.InputDateYear.TextYear =
          NumberToString(Year(import.DateWorkArea.Date), 12, 4);
        local.DateTextPart1.Text12 = TrimEnd(local.MonthName.Text10) + " " + TrimEnd
          (local.InputDateDay.TextDay);
        local.DateTextPart2.Text5 = " " + TrimEnd(local.InputDateYear.TextYear);
        export.DateText.TextDate20Char = TrimEnd(local.DateTextPart1.Text12) + ","
          + local.DateTextPart2.Text5;

        break;
      case 'B':
        // ----> Style "B" returns text in the format 'DD Month YYYY'
        //      -Syed Hasan,MTW 01-18-98
        local.InputDateDay.NumericalDay = Day(import.DateWorkArea.Date);

        if (local.InputDateDay.NumericalDay < 10)
        {
          local.InputDateDay.TextDay =
            NumberToString(local.InputDateDay.NumericalDay, 15, 1);
        }
        else
        {
          local.InputDateDay.TextDay =
            NumberToString(local.InputDateDay.NumericalDay, 14, 2);
        }

        local.InputDateYear.TextYear =
          NumberToString(Year(import.DateWorkArea.Date), 12, 4);
        local.DateTextPart1.Text12 = TrimEnd(local.InputDateDay.TextDay) + " " +
          TrimEnd(local.MonthName.Text10);
        local.DateTextPart2.Text5 = " " + TrimEnd(local.InputDateYear.TextYear);
        export.DateText.TextDate20Char = TrimEnd(local.DateTextPart1.Text12) + local
          .DateTextPart2.Text5;

        break;
      case 'C':
        // ----> Style "C" returns text in the format 'MMDDYYYY'
        //      -Syed Hasan,MTW 01-18-98
        export.DateText.TextDate20Char =
          NumberToString(Month(import.DateWorkArea.Date), 14, 2) + NumberToString
          (Day(import.DateWorkArea.Date), 14, 2) + NumberToString
          (Year(import.DateWorkArea.Date), 12, 4);

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
    /// A value of DateTextStyle.
    /// </summary>
    [JsonPropertyName("dateTextStyle")]
    public Common DateTextStyle
    {
      get => dateTextStyle ??= new();
      set => dateTextStyle = value;
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

    private Common dateTextStyle;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DateText.
    /// </summary>
    [JsonPropertyName("dateText")]
    public DateWorkAttributes DateText
    {
      get => dateText ??= new();
      set => dateText = value;
    }

    private DateWorkAttributes dateText;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateTextPart2.
    /// </summary>
    [JsonPropertyName("dateTextPart2")]
    public WorkArea DateTextPart2
    {
      get => dateTextPart2 ??= new();
      set => dateTextPart2 = value;
    }

    /// <summary>
    /// A value of DateTextPart1.
    /// </summary>
    [JsonPropertyName("dateTextPart1")]
    public WorkArea DateTextPart1
    {
      get => dateTextPart1 ??= new();
      set => dateTextPart1 = value;
    }

    /// <summary>
    /// A value of InputDateYear.
    /// </summary>
    [JsonPropertyName("inputDateYear")]
    public DateWorkAttributes InputDateYear
    {
      get => inputDateYear ??= new();
      set => inputDateYear = value;
    }

    /// <summary>
    /// A value of InputDateDay.
    /// </summary>
    [JsonPropertyName("inputDateDay")]
    public DateWorkAttributes InputDateDay
    {
      get => inputDateDay ??= new();
      set => inputDateDay = value;
    }

    /// <summary>
    /// A value of InputDateMonth.
    /// </summary>
    [JsonPropertyName("inputDateMonth")]
    public DateWorkAttributes InputDateMonth
    {
      get => inputDateMonth ??= new();
      set => inputDateMonth = value;
    }

    /// <summary>
    /// A value of MonthName.
    /// </summary>
    [JsonPropertyName("monthName")]
    public TextWorkArea MonthName
    {
      get => monthName ??= new();
      set => monthName = value;
    }

    private WorkArea dateTextPart2;
    private WorkArea dateTextPart1;
    private DateWorkAttributes inputDateYear;
    private DateWorkAttributes inputDateDay;
    private DateWorkAttributes inputDateMonth;
    private TextWorkArea monthName;
  }
#endregion
}
