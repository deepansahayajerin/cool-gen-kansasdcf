// Program: CAB_ZERO_FILL_NUMBER, ID: 371455762, model: 746.
// Short name: SWE00108
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_ZERO_FILL_NUMBER.
/// </para>
/// <para>
/// Pads leading zeros in front of a person or case number, e.g. '1234' will be 
/// returned as '0000001234'.
/// </para>
/// </summary>
[Serializable]
public partial class CabZeroFillNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_ZERO_FILL_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabZeroFillNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabZeroFillNumber.
  /// </summary>
  public CabZeroFillNumber(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.Case1.Number))
    {
      local.WorkArea.Text12 = import.Case1.Number;
      UseCabTestForNumericText();

      if (AsChar(local.NumericText.Flag) == 'Y')
      {
        import.Case1.Number =
          NumberToString(StringToNumber(import.Case1.Number), 10);
      }
      else
      {
        ExitState = "CASE_NUMBER_NOT_NUMERIC";

        return;
      }
    }

    if (!IsEmpty(import.CsePerson.Number))
    {
      local.WorkArea.Text12 = import.CsePerson.Number;
      UseCabTestForNumericText();

      if (AsChar(local.NumericText.Flag) == 'Y')
      {
        import.CsePerson.Number =
          NumberToString(StringToNumber(import.CsePerson.Number), 10);
      }
      else
      {
        ExitState = "PERSON_NUMBER_NOT_NUMERIC";

        return;
      }
    }

    if (!IsEmpty(import.CsePersonsWorkSet.Number))
    {
      local.WorkArea.Text12 = import.CsePersonsWorkSet.Number;
      UseCabTestForNumericText();

      if (AsChar(local.NumericText.Flag) == 'Y')
      {
        import.CsePersonsWorkSet.Number =
          NumberToString(StringToNumber(import.CsePersonsWorkSet.Number), 10);
      }
      else
      {
        ExitState = "PERSON_NUMBER_NOT_NUMERIC";
      }
    }
  }

  private void UseCabTestForNumericText()
  {
    var useImport = new CabTestForNumericText.Import();
    var useExport = new CabTestForNumericText.Export();

    useImport.WorkArea.Text12 = local.WorkArea.Text12;

    Call(CabTestForNumericText.Execute, useImport, useExport);

    local.NumericText.Flag = useExport.NumericText.Flag;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Case1 case1;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    /// <summary>
    /// A value of NumericText.
    /// </summary>
    [JsonPropertyName("numericText")]
    public Common NumericText
    {
      get => numericText ??= new();
      set => numericText = value;
    }

    private WorkArea workArea;
    private Common numericText;
  }
#endregion
}
