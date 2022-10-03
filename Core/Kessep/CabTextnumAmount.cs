// Program: CAB_TEXTNUM_AMOUNT, ID: 372400444, model: 746.
// Short name: SWE02413
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_TEXTNUM_AMOUNT.
/// </para>
/// <para>
/// This action block will conert an amount to a text string.
/// </para>
/// </summary>
[Serializable]
public partial class CabTextnumAmount: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_TEXTNUM_AMOUNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabTextnumAmount(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabTextnumAmount.
  /// </summary>
  public CabTextnumAmount(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // This action block will convert an amount to a text string with a decimal 
    // point.  The amount being converted is limitted to 99,999,999.99.  An
    // import amount greater than this limit will result in "********.**" being
    // returned back in the export view.
    // This action block was created for printing total amounts for batch 
    // processes on the control report.
    if (import.Common.TotalCurrency > 99999999.99M)
    {
      export.TextAmount.Text11 = "********.**";
    }
    else
    {
      local.WorkArea.Text10 =
        NumberToString((long)(import.Common.TotalCurrency * 100), 10);
      export.TextAmount.Text11 =
        Substring(local.WorkArea.Text10, WorkArea.Text10_MaxLength, 1, 8) + "."
        + Substring(local.WorkArea.Text10, WorkArea.Text10_MaxLength, 9, 2);
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TextAmount.
    /// </summary>
    [JsonPropertyName("textAmount")]
    public WorkArea TextAmount
    {
      get => textAmount ??= new();
      set => textAmount = value;
    }

    private WorkArea textAmount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    private WorkArea workArea;
  }
#endregion
}
