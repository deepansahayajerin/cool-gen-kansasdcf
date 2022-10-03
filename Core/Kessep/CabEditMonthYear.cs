// Program: CAB_EDIT_MONTH_YEAR, ID: 374454858, model: 746.
// Short name: SWE02699
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_EDIT_MONTH_YEAR.
/// </summary>
[Serializable]
public partial class CabEditMonthYear: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_EDIT_MONTH_YEAR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabEditMonthYear(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabEditMonthYear.
  /// </summary>
  public CabEditMonthYear(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.NumericVerify.Text10 = "0123456789";
    export.DateWorkAttributes.Assign(import.DateWorkAttributes);

    // If text month was passed in edit it and return it as well as the numeric 
    // month.
    if (!IsEmpty(export.DateWorkAttributes.TextMonth))
    {
      if (IsEmpty(Substring(export.DateWorkAttributes.TextMonth, 1, 1)))
      {
        export.DateWorkAttributes.TextMonth = "0" + Substring
          (export.DateWorkAttributes.TextMonth,
          DateWorkAttributes.TextMonth_MaxLength, 2, 1);
      }

      if (Verify(export.DateWorkAttributes.TextMonth, local.NumericVerify.Text10)
        > 0)
      {
        ExitState = "ACO_NON_NUMERIC_DATA_ENTERED";

        return;
      }

      if (Lt(export.DateWorkAttributes.TextMonth, "01") || Lt
        ("12", export.DateWorkAttributes.TextMonth))
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      export.DateWorkAttributes.NumericalMonth =
        (int)StringToNumber(export.DateWorkAttributes.TextMonth);

      return;
    }

    // If text year was passed in edit it and return it as well as the numeric 
    // year.
    if (!IsEmpty(export.DateWorkAttributes.TextYear))
    {
      if (Verify(export.DateWorkAttributes.TextYear, local.NumericVerify.Text10) >
        0)
      {
        ExitState = "ACO_NON_NUMERIC_DATA_ENTERED";

        return;
      }

      if (Lt(export.DateWorkAttributes.TextYear, "0001") || Lt
        ("2099", export.DateWorkAttributes.TextYear))
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      if (Lt(NumberToString(Now().Date.Year, 12, 4),
        export.DateWorkAttributes.TextYear))
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }

      export.DateWorkAttributes.NumericalYear =
        (int)StringToNumber(export.DateWorkAttributes.TextYear);

      return;
    }

    // If text month year was passed in edit it and return it as well as the 
    // numeric month and year.
    if (!IsEmpty(export.DateWorkAttributes.TextMonthYear))
    {
      if (IsEmpty(Substring(export.DateWorkAttributes.TextMonthYear, 6, 1)))
      {
        export.DateWorkAttributes.TextMonthYear = "0" + Substring
          (export.DateWorkAttributes.TextMonthYear,
          DateWorkAttributes.TextMonthYear_MaxLength, 1, 5);
      }

      if (IsEmpty(Substring(export.DateWorkAttributes.TextMonthYear, 1, 1)))
      {
        export.DateWorkAttributes.TextMonthYear = "0" + Substring
          (export.DateWorkAttributes.TextMonthYear,
          DateWorkAttributes.TextMonthYear_MaxLength, 2, 5);
      }

      export.DateWorkAttributes.TextMonth =
        Substring(import.DateWorkAttributes.TextMonthYear, 1, 2);
      export.DateWorkAttributes.TextYear =
        Substring(import.DateWorkAttributes.TextMonthYear, 3, 4);

      if (Verify(export.DateWorkAttributes.TextMonthYear,
        local.NumericVerify.Text10) > 0)
      {
        ExitState = "ACO_NON_NUMERIC_DATA_ENTERED";

        return;
      }

      if (Lt(export.DateWorkAttributes.TextMonth, "01") || Lt
        ("12", export.DateWorkAttributes.TextMonth))
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      if (Lt(export.DateWorkAttributes.TextYear, "0001") || Lt
        ("2099", export.DateWorkAttributes.TextYear))
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      local.Current.TextMonth = NumberToString(Now().Date.Month, 14, 2);
      local.Current.TextYear = NumberToString(Now().Date.Year, 12, 4);

      if (Lt(local.Current.TextYear, export.DateWorkAttributes.TextYear))
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }
      else if (Equal(export.DateWorkAttributes.TextYear, local.Current.TextYear) &&
        Lt(local.Current.TextMonth, export.DateWorkAttributes.TextMonth))
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }

      export.DateWorkAttributes.TextYearMonth =
        Substring(export.DateWorkAttributes.TextMonthYear,
        DateWorkAttributes.TextMonthYear_MaxLength, 3, 4) + Substring
        (export.DateWorkAttributes.TextMonthYear,
        DateWorkAttributes.TextMonthYear_MaxLength, 1, 2);
      export.DateWorkAttributes.NumericalMonth =
        (int)StringToNumber(export.DateWorkAttributes.TextMonth);
      export.DateWorkAttributes.NumericalYear =
        (int)StringToNumber(export.DateWorkAttributes.TextYear);

      return;
    }

    // If text year month was passed in edit it and return it as well as the 
    // numeric year and month.
    if (!IsEmpty(import.DateWorkAttributes.TextYearMonth))
    {
      if (IsEmpty(Substring(export.DateWorkAttributes.TextYearMonth, 5, 1)))
      {
        export.DateWorkAttributes.TextYearMonth =
          Substring(export.DateWorkAttributes.TextYearMonth,
          DateWorkAttributes.TextYearMonth_MaxLength, 1, 4) + "0" + Substring
          (export.DateWorkAttributes.TextYearMonth,
          DateWorkAttributes.TextYearMonth_MaxLength, 6, 1);
      }

      if (Verify(export.DateWorkAttributes.TextYearMonth,
        local.NumericVerify.Text10) > 0)
      {
        ExitState = "ACO_NON_NUMERIC_DATA_ENTERED";

        return;
      }

      if (Lt(export.DateWorkAttributes.TextMonth, "01") || Lt
        ("12", export.DateWorkAttributes.TextMonth))
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      if (Lt(export.DateWorkAttributes.TextYear, "0001") || Lt
        ("2099", export.DateWorkAttributes.TextYear))
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      local.Current.TextMonth = NumberToString(Now().Date.Month, 14, 2);
      local.Current.TextYear = NumberToString(Now().Date.Year, 12, 4);

      if (Lt(local.Current.TextYear, export.DateWorkAttributes.TextYear))
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }
      else if (Equal(export.DateWorkAttributes.TextYear, local.Current.TextYear) &&
        Lt(local.Current.TextMonth, export.DateWorkAttributes.TextMonth))
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }

      export.DateWorkAttributes.TextMonthYear =
        Substring(export.DateWorkAttributes.TextYearMonth,
        DateWorkAttributes.TextYearMonth_MaxLength, 5, 2) + Substring
        (export.DateWorkAttributes.TextYearMonth,
        DateWorkAttributes.TextYearMonth_MaxLength, 1, 4);
      export.DateWorkAttributes.TextYear =
        Substring(import.DateWorkAttributes.TextMonthYear, 1, 4);
      export.DateWorkAttributes.TextMonth =
        Substring(import.DateWorkAttributes.TextMonthYear, 5, 2);

      return;
    }

    // If numeric month was passed in edit it and return it as well as the text 
    // month.
    if (import.DateWorkAttributes.NumericalMonth > 0)
    {
      if (import.DateWorkAttributes.NumericalMonth < 1 || import
        .DateWorkAttributes.NumericalMonth > 12)
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      export.DateWorkAttributes.TextMonth =
        NumberToString(export.DateWorkAttributes.NumericalMonth, 14, 2);

      return;
    }

    // If numeric year was passed in edit it and return it as well as the text 
    // year.
    if (import.DateWorkAttributes.NumericalYear > 0)
    {
      if (import.DateWorkAttributes.NumericalYear < 1 || import
        .DateWorkAttributes.NumericalYear > 2099)
      {
        ExitState = "ACO_NE0000_INVALID_DATE";

        return;
      }

      if (import.DateWorkAttributes.NumericalYear > Now().Date.Year)
      {
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        return;
      }

      export.DateWorkAttributes.TextYear =
        NumberToString(export.DateWorkAttributes.NumericalYear, 12, 4);

      return;
    }

    // No date was passed in.
    ExitState = "ACO_NE0000_INVALID_DATE";
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
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
    }

    private DateWorkAttributes dateWorkAttributes;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
    }

    private DateWorkAttributes dateWorkAttributes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NumericVerify.
    /// </summary>
    [JsonPropertyName("numericVerify")]
    public TextWorkArea NumericVerify
    {
      get => numericVerify ??= new();
      set => numericVerify = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkAttributes Current
    {
      get => current ??= new();
      set => current = value;
    }

    private TextWorkArea numericVerify;
    private DateWorkAttributes current;
  }
#endregion
}
