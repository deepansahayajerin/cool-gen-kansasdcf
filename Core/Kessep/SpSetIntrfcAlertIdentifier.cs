// Program: SP_SET_INTRFC_ALERT_IDENTIFIER, ID: 371733800, model: 746.
// Short name: SWE01854
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_SET_INTRFC_ALERT_IDENTIFIER.
/// </para>
/// <para>
/// This AB sets the INTERFACE_ALERT IDENTIFIER to the correct format.
/// </para>
/// </summary>
[Serializable]
public partial class SpSetIntrfcAlertIdentifier: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SET_INTRFC_ALERT_IDENTIFIER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSetIntrfcAlertIdentifier(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSetIntrfcAlertIdentifier.
  /// </summary>
  public SpSetIntrfcAlertIdentifier(IContext context, Import import,
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
    // mjr
    // *******************************************************************
    // The Identifier for the INTERFACE_ALERT is a text timestamp.  The
    // format is:
    //             YYMMDDHHMISSNNNN
    //             where YY = Year
    //                   MM = Month
    //                   DD = Day
    //                   HH = Hour
    //                   MI = Minute
    //                   SS = Second
    //                   NNNN = Microsecond
    // *********************************************************************
    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.InterfaceAlert.Identifier =
      Substring(local.BatchTimestampWorkArea.TextDateYyyy,
      BatchTimestampWorkArea.TextDateYyyy_MaxLength, 3, 2) + local
      .BatchTimestampWorkArea.TextDateMm;
    local.InterfaceAlert.Identifier =
      TrimEnd(local.InterfaceAlert.Identifier) + local
      .BatchTimestampWorkArea.TestDateDd;
    local.InterfaceAlert.Identifier =
      TrimEnd(local.InterfaceAlert.Identifier) + local
      .BatchTimestampWorkArea.TestTimeHh;
    local.InterfaceAlert.Identifier =
      TrimEnd(local.InterfaceAlert.Identifier) + local
      .BatchTimestampWorkArea.TextTimeMm;
    local.InterfaceAlert.Identifier =
      TrimEnd(local.InterfaceAlert.Identifier) + local
      .BatchTimestampWorkArea.TextTimeSs;
    local.InterfaceAlert.Identifier =
      TrimEnd(local.InterfaceAlert.Identifier) + Substring
      (local.BatchTimestampWorkArea.TextMillisecond,
      BatchTimestampWorkArea.TextMillisecond_MaxLength, 1, 4);
    export.InterfaceAlert.Identifier = local.InterfaceAlert.Identifier;
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    local.BatchTimestampWorkArea.Assign(useExport.BatchTimestampWorkArea);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private InterfaceAlert interfaceAlert;
    private BatchTimestampWorkArea batchTimestampWorkArea;
  }
#endregion
}
