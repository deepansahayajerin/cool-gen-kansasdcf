// Program: LE_CAB_CONVERT_TIMESTAMP, ID: 371734133, model: 746.
// Short name: SWE00725
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CAB_CONVERT_TIMESTAMP.
/// </summary>
[Serializable]
public partial class LeCabConvertTimestamp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_CONVERT_TIMESTAMP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabConvertTimestamp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabConvertTimestamp.
  /// </summary>
  public LeCabConvertTimestamp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // *               MAINTENANCE LOG             *
    // *
    // 
    // *
    // *   DATE    DEVELOPER       DESCRIPTION     *
    // * 01/28/96   H HOOKS    INITIAL DEVELOPMENT *
    // *********************************************
    // ---------------------------------------------
    // This cab, when passed a timestamp in a text
    // attribute domain, will return the same
    // timestamp in an ief timestamp attribute
    // domain.
    // This cab, when passed a timestamp in an ief
    // timestamp attribute domain, will return the
    // same timestamp in a text attribute domain.
    // ---------------------------------------------
    if (IsEmpty(import.BatchTimestampWorkArea.TextTimestamp))
    {
      local.BatchTimestampWorkArea.NumDate =
        DateToInt(Date(import.BatchTimestampWorkArea.IefTimestamp));
      local.BatchTimestampWorkArea.NumTime =
        TimeToInt(TimeOfDay(import.BatchTimestampWorkArea.IefTimestamp));
      local.BatchTimestampWorkArea.NumMillisecond =
        Microsecond(import.BatchTimestampWorkArea.IefTimestamp);
      local.BatchTimestampWorkArea.TextDate =
        NumberToString(local.BatchTimestampWorkArea.NumDate, 8, 8);
      local.BatchTimestampWorkArea.TextTime =
        NumberToString(local.BatchTimestampWorkArea.NumTime, 10, 6);
      local.BatchTimestampWorkArea.TextMillisecond =
        NumberToString(local.BatchTimestampWorkArea.NumMillisecond, 10, 6);
      local.BatchTimestampWorkArea.TextDateYyyy =
        Substring(local.BatchTimestampWorkArea.TextDate, 1, 4);
      local.BatchTimestampWorkArea.TextDateMm =
        Substring(local.BatchTimestampWorkArea.TextDate, 5, 2);
      local.BatchTimestampWorkArea.TestDateDd =
        Substring(local.BatchTimestampWorkArea.TextDate, 7, 2);
      local.BatchTimestampWorkArea.TestTimeHh =
        Substring(local.BatchTimestampWorkArea.TextTime, 1, 2);
      local.BatchTimestampWorkArea.TextTimeMm =
        Substring(local.BatchTimestampWorkArea.TextTime, 3, 2);
      local.BatchTimestampWorkArea.TextTimeSs =
        Substring(local.BatchTimestampWorkArea.TextTime, 5, 2);
      MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
        export.BatchTimestampWorkArea);
      export.BatchTimestampWorkArea.TextTimestamp =
        local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
        .BatchTimestampWorkArea.TextDateMm + "-" + local
        .BatchTimestampWorkArea.TestDateDd + "-" + local
        .BatchTimestampWorkArea.TestTimeHh + "." + local
        .BatchTimestampWorkArea.TextTimeMm + "." + local
        .BatchTimestampWorkArea.TextTimeSs + "." + local
        .BatchTimestampWorkArea.TextMillisecond;

      return;
    }

    export.BatchTimestampWorkArea.IefTimestamp =
      Timestamp(import.BatchTimestampWorkArea.TextTimestamp);
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.TextDate = source.TextDate;
    target.TextDateYyyy = source.TextDateYyyy;
    target.TextDateMm = source.TextDateMm;
    target.TestDateDd = source.TestDateDd;
    target.TextTime = source.TextTime;
    target.TestTimeHh = source.TestTimeHh;
    target.TextTimeMm = source.TextTimeMm;
    target.TextTimeSs = source.TextTimeSs;
    target.TextMillisecond = source.TextMillisecond;
    target.NumDate = source.NumDate;
    target.NumTime = source.NumTime;
    target.NumMillisecond = source.NumMillisecond;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
  }
#endregion
}
