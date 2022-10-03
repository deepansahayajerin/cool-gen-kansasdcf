// Program: FN_CAB_FORMAT_REFERENCE_NUMBER, ID: 371038163, model: 746.
// Short name: SWE02728
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_FORMAT_REFERENCE_NUMBER.
/// </summary>
[Serializable]
public partial class FnCabFormatReferenceNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_FORMAT_REFERENCE_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabFormatReferenceNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabFormatReferenceNumber.
  /// </summary>
  public FnCabFormatReferenceNumber(IContext context, Import import,
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
    // -----------------------------------------------------------------
    // 03/02/2001 Fangman PR#111602
    // Created this AB to format the reference number on disbursement 
    // transaction.
    // -----------------------------------------------------------------
    export.RefNbr.ReferenceNumber = import.RefNbr.ReferenceNumber ?? "";
    local.RefNbr.ReferenceNumber = import.RefNbr.ReferenceNumber ?? "";

    // Left justify the reference number in case they entered spaces prior to 
    // numbers.
    local.LoopCount.TotalInteger = 0;

    do
    {
      ++local.LoopCount.TotalInteger;

      if (IsEmpty(Substring(
        local.RefNbr.ReferenceNumber, (int)local.LoopCount.TotalInteger, 1)))
      {
        local.RefNbr.ReferenceNumber =
          Substring(local.RefNbr.ReferenceNumber, 2, 11);
      }
      else
      {
        break;
      }
    }
    while(local.LoopCount.TotalInteger < 12);

    // Find the position of the dash.
    local.PositionOfDash.TotalInteger =
      Verify(local.RefNbr.ReferenceNumber, " 0123456789");

    if (CharAt(local.RefNbr.ReferenceNumber,
      (int)local.PositionOfDash.TotalInteger) != '-')
    {
      ExitState = "FN0000_INVALID_REFERENCE_NUMBER";

      return;
    }
    else if (local.PositionOfDash.TotalInteger < 2 || local
      .PositionOfDash.TotalInteger > 11)
    {
      ExitState = "FN0000_INVALID_REFERENCE_NUMBER";

      return;
    }

    // Build the prefix
    local.RefNbrPrefix.Text7 =
      Substring(local.RefNbr.ReferenceNumber, 1,
      (int)(local.PositionOfDash.TotalInteger - 1));
    local.StringLength.TotalInteger = Length(TrimEnd(local.RefNbrPrefix.Text7));

    if (local.StringLength.TotalInteger == 1)
    {
      local.RefNbrPrefix.Text7 = "000000" + local.RefNbrPrefix.Text7;
    }
    else if (local.StringLength.TotalInteger == 2)
    {
      local.RefNbrPrefix.Text7 = "00000" + local.RefNbrPrefix.Text7;
    }
    else if (local.StringLength.TotalInteger == 3)
    {
      local.RefNbrPrefix.Text7 = "0000" + local.RefNbrPrefix.Text7;
    }
    else if (local.StringLength.TotalInteger == 4)
    {
      local.RefNbrPrefix.Text7 = "000" + local.RefNbrPrefix.Text7;
    }
    else if (local.StringLength.TotalInteger == 5)
    {
      local.RefNbrPrefix.Text7 = "00" + local.RefNbrPrefix.Text7;
    }
    else if (local.StringLength.TotalInteger == 6)
    {
      local.RefNbrPrefix.Text7 = "0" + local.RefNbrPrefix.Text7;
    }
    else
    {
    }

    // Build the suffix
    local.RefNbrSuffix.Text4 =
      Substring(local.RefNbr.ReferenceNumber,
      (int)(local.PositionOfDash.TotalInteger + 1), (int)(12 - local
      .PositionOfDash.TotalInteger));
    local.StringLength.TotalInteger = Length(TrimEnd(local.RefNbrSuffix.Text4));

    if (local.StringLength.TotalInteger == 1)
    {
      local.RefNbrSuffix.Text4 = "000" + local.RefNbrSuffix.Text4;
    }
    else if (local.StringLength.TotalInteger == 2)
    {
      local.RefNbrSuffix.Text4 = "00" + local.RefNbrSuffix.Text4;
    }
    else if (local.StringLength.TotalInteger == 3)
    {
      local.RefNbrSuffix.Text4 = "0" + local.RefNbrSuffix.Text4;
    }
    else if (local.StringLength.TotalInteger == 4)
    {
      // Continue
    }
    else
    {
      ExitState = "FN0000_INVALID_REFERENCE_NUMBER";
    }

    export.RefNbr.ReferenceNumber = local.RefNbrPrefix.Text7 + "-" + local
      .RefNbrSuffix.Text4 + "  ";
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
    /// A value of RefNbr.
    /// </summary>
    [JsonPropertyName("refNbr")]
    public DisbursementTransaction RefNbr
    {
      get => refNbr ??= new();
      set => refNbr = value;
    }

    private DisbursementTransaction refNbr;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RefNbr.
    /// </summary>
    [JsonPropertyName("refNbr")]
    public DisbursementTransaction RefNbr
    {
      get => refNbr ??= new();
      set => refNbr = value;
    }

    private DisbursementTransaction refNbr;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RefNbr.
    /// </summary>
    [JsonPropertyName("refNbr")]
    public DisbursementTransaction RefNbr
    {
      get => refNbr ??= new();
      set => refNbr = value;
    }

    /// <summary>
    /// A value of LoopCount.
    /// </summary>
    [JsonPropertyName("loopCount")]
    public Common LoopCount
    {
      get => loopCount ??= new();
      set => loopCount = value;
    }

    /// <summary>
    /// A value of PositionOfDash.
    /// </summary>
    [JsonPropertyName("positionOfDash")]
    public Common PositionOfDash
    {
      get => positionOfDash ??= new();
      set => positionOfDash = value;
    }

    /// <summary>
    /// A value of RefNbrPrefix.
    /// </summary>
    [JsonPropertyName("refNbrPrefix")]
    public WorkArea RefNbrPrefix
    {
      get => refNbrPrefix ??= new();
      set => refNbrPrefix = value;
    }

    /// <summary>
    /// A value of RefNbrSuffix.
    /// </summary>
    [JsonPropertyName("refNbrSuffix")]
    public WorkArea RefNbrSuffix
    {
      get => refNbrSuffix ??= new();
      set => refNbrSuffix = value;
    }

    /// <summary>
    /// A value of StringLength.
    /// </summary>
    [JsonPropertyName("stringLength")]
    public Common StringLength
    {
      get => stringLength ??= new();
      set => stringLength = value;
    }

    private DisbursementTransaction refNbr;
    private Common loopCount;
    private Common positionOfDash;
    private WorkArea refNbrPrefix;
    private WorkArea refNbrSuffix;
    private Common stringLength;
  }
#endregion
}
