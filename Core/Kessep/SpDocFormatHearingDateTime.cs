// Program: SP_DOC_FORMAT_HEARING_DATE_TIME, ID: 372134133, model: 746.
// Short name: SWE00691
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_FORMAT_HEARING_DATE_TIME.
/// </para>
/// <para>
/// This action block is used by SP_LEGAL_DATA_RETRIEVAL to format the time and 
/// date of a hearing.  It is used when printing a document.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFormatHearingDateTime: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FORMAT_HEARING_DATE_TIME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFormatHearingDateTime(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFormatHearingDateTime.
  /// </summary>
  public SpDocFormatHearingDateTime(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    // Date		Developer	Description
    // ----------------------------------------------------------------------
    // 04/13/1996	M Ramirez	Initial Development
    // 06/16/1999	M Ramirez	Removed use of sp_eab_concat
    // 06/16/1999	M Ramirez	Added use of local ief_supplied view
    // 06/16/1999	M Ramirez	Changed logic to remove time formatting
    // 				if no time is imported
    // ----------------------------------------------------------------------
    // ----------------------------------------------------------------------
    // Imports:  Date   = 04131996
    //           Time   = 1300
    // Exports:  String = "13th of April, 1996 at 1:00 o'clock pm"
    // ----------------------------------------------------------------------
    // -----------------------------------------------------------------------
    //    DATE FORMATTING
    // -----------------------------------------------------------------------
    // ----------------------------------------------------------------------
    //    LOCAL_SP_PRINT_WORK_SET NUMBER_2 = DAY
    // ----------------------------------------------------------------------
    local.SpPrintWorkSet.Number2 = Day(import.DateWorkArea.Date);
    local.Position.Count =
      Verify(NumberToString(local.SpPrintWorkSet.Number2, 15), "0");
    export.FieldValue.Value =
      NumberToString(local.SpPrintWorkSet.Number2, local.Position.Count, 16 -
      local.Position.Count);

    if (local.SpPrintWorkSet.Number2 == 1 || local.SpPrintWorkSet.Number2 == 21
      || local.SpPrintWorkSet.Number2 == 31)
    {
      local.FieldValue.Value = "st day of";
    }
    else if (local.SpPrintWorkSet.Number2 == 2 || local
      .SpPrintWorkSet.Number2 == 22)
    {
      local.FieldValue.Value = "nd day of";
    }
    else if (local.SpPrintWorkSet.Number2 == 3 || local
      .SpPrintWorkSet.Number2 == 23)
    {
      local.FieldValue.Value = "rd day of";
    }
    else
    {
      local.FieldValue.Value = "th day of";
    }

    export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
      local.FieldValue.Value ?? "");

    // ----------------------------------------------------------------------
    //    LOCAL_SP_PRINT_WORK_SET NUMBER_2 = MONTH
    // ----------------------------------------------------------------------
    local.SpPrintWorkSet.Number2 = Month(import.DateWorkArea.Date);

    switch(local.SpPrintWorkSet.Number2)
    {
      case 1:
        local.FieldValue.Value = " January,";

        break;
      case 2:
        local.FieldValue.Value = " February,";

        break;
      case 3:
        local.FieldValue.Value = " March,";

        break;
      case 4:
        local.FieldValue.Value = " April,";

        break;
      case 5:
        local.FieldValue.Value = " May,";

        break;
      case 6:
        local.FieldValue.Value = " June,";

        break;
      case 7:
        local.FieldValue.Value = " July,";

        break;
      case 8:
        local.FieldValue.Value = " August,";

        break;
      case 9:
        local.FieldValue.Value = " September,";

        break;
      case 10:
        local.FieldValue.Value = " October,";

        break;
      case 11:
        local.FieldValue.Value = " November,";

        break;
      case 12:
        local.FieldValue.Value = " December,";

        break;
      default:
        break;
    }

    export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
      local.FieldValue.Value ?? "");
    local.FieldValue.Value = " " + NumberToString
      (Year(import.DateWorkArea.Date), 12, 4);
    export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
      local.FieldValue.Value ?? "");

    // ----------------------------------------------------------------------
    //      TIME FORMATTING
    // ----------------------------------------------------------------------
    // mjr
    // ------------------------------------------
    // 06/16/1999
    // Added check for no imported time
    // -------------------------------------------------------
    if (import.DateWorkArea.Time > local.Null1.Time)
    {
      local.FieldValue.Value = " at";
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");

      // ----------------------------------------------------------------------
      //    LOCAL_SP_PRINT_WORK_SET NUMBER_2 = HOUR
      // ----------------------------------------------------------------------
      local.SpPrintWorkSet.Number2 = import.DateWorkArea.Time.Hours;

      if (local.SpPrintWorkSet.Number2 >= 12)
      {
        local.AmPmInd.Value = " pm";
      }
      else
      {
        local.AmPmInd.Value = " am";
      }

      if (local.SpPrintWorkSet.Number2 > 12)
      {
        local.SpPrintWorkSet.Number2 -= 12;
      }

      local.Position.Count =
        Verify(NumberToString(local.SpPrintWorkSet.Number2, 15), "0");
      local.FieldValue.Value =
        NumberToString(local.SpPrintWorkSet.Number2, local.Position.Count, 16 -
        local.Position.Count);
      local.FieldValue.Value = " " + (local.FieldValue.Value ?? "");
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");

      // ----------------------------------------------------------------------
      //    LOCAL_SP_PRINT_WORK_SET NUMBER_2 = MINUTES
      // ----------------------------------------------------------------------
      local.SpPrintWorkSet.Number2 = import.DateWorkArea.Time.Minutes;
      local.FieldValue.Value = ":" + NumberToString
        (local.SpPrintWorkSet.Number2, 14, 2);
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");

      if (local.SpPrintWorkSet.Number2 == 0)
      {
        local.FieldValue.Value = " o'clock";
        export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
          local.FieldValue.Value ?? "");
      }

      local.FieldValue.Value = local.AmPmInd.Value ?? "";
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + (
        local.FieldValue.Value ?? "");
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private FieldValue fieldValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of AmPmInd.
    /// </summary>
    [JsonPropertyName("amPmInd")]
    public FieldValue AmPmInd
    {
      get => amPmInd ??= new();
      set => amPmInd = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    private SpPrintWorkSet spPrintWorkSet;
    private FieldValue fieldValue;
    private FieldValue amPmInd;
    private DateWorkArea null1;
    private Common position;
  }
#endregion
}
