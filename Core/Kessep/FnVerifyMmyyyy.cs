// Program: FN_VERIFY_MMYYYY, ID: 372449319, model: 746.
// Short name: SWE00688
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_VERIFY_MMYYYY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This AB will receive a DATE (Formatted as: MMYYYY).  If the date is NULL, 
/// the AB will set it the current month.  The AB will then return a 6 digit
/// number (YEAR_MONTH, YYYYMM).
/// </para>
/// </summary>
[Serializable]
public partial class FnVerifyMmyyyy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VERIFY_MMYYYY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnVerifyMmyyyy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnVerifyMmyyyy.
  /// </summary>
  public FnVerifyMmyyyy(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Date.Date = import.Date.Date;
    local.WorkDate.Date = Now().Date;
    UseCabGetYearMonthFromDate1();

    if (Equal(export.Date.Date, local.NullDate.Date))
    {
      export.Date.Date = Now().Date;
    }

    UseCabGetYearMonthFromDate2();

    if (export.YearMonth.YearMonth > local.CurrentYearMonth.YearMonth)
    {
      ExitState = "SEARCH_MMYYYY_GRTR_CURR_MMYYYY";
    }
  }

  private void UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.WorkDate.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    local.CurrentYearMonth.YearMonth = useExport.DateWorkArea.YearMonth;
  }

  private void UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = export.Date.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    export.YearMonth.YearMonth = useExport.DateWorkArea.YearMonth;
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    private DateWorkArea date;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of YearMonth.
    /// </summary>
    [JsonPropertyName("yearMonth")]
    public DateWorkArea YearMonth
    {
      get => yearMonth ??= new();
      set => yearMonth = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    private DateWorkArea yearMonth;
    private DateWorkArea date;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentYearMonth.
    /// </summary>
    [JsonPropertyName("currentYearMonth")]
    public DateWorkArea CurrentYearMonth
    {
      get => currentYearMonth ??= new();
      set => currentYearMonth = value;
    }

    /// <summary>
    /// A value of WorkDate.
    /// </summary>
    [JsonPropertyName("workDate")]
    public DateWorkArea WorkDate
    {
      get => workDate ??= new();
      set => workDate = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private DateWorkArea currentYearMonth;
    private DateWorkArea workDate;
    private DateWorkArea nullDate;
  }
#endregion
}
