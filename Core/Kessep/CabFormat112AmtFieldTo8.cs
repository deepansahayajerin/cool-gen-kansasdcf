// Program: CAB_FORMAT_11_2_AMT_FIELD_TO_8, ID: 372950468, model: 746.
// Short name: SWE02497
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FORMAT_11_2_AMT_FIELD_TO_8.
/// </summary>
[Serializable]
public partial class CabFormat112AmtFieldTo8: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FORMAT_11_2_AMT_FIELD_TO_8 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFormat112AmtFieldTo8(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFormat112AmtFieldTo8.
  /// </summary>
  public CabFormat112AmtFieldTo8(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.WorkArea.Text15 =
      NumberToString((long)(import.Import112AmountField.Number112 * 100), 15);
    export.Formatted112AmtField.Text9 =
      Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 8, 6) + "."
      + Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
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
    /// A value of Import112AmountField.
    /// </summary>
    [JsonPropertyName("import112AmountField")]
    public NumericWorkSet Import112AmountField
    {
      get => import112AmountField ??= new();
      set => import112AmountField = value;
    }

    private NumericWorkSet import112AmountField;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Formatted112AmtField.
    /// </summary>
    [JsonPropertyName("formatted112AmtField")]
    public WorkArea Formatted112AmtField
    {
      get => formatted112AmtField ??= new();
      set => formatted112AmtField = value;
    }

    private WorkArea formatted112AmtField;
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
