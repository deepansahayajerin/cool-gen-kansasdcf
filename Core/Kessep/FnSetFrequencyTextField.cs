// Program: FN_SET_FREQUENCY_TEXT_FIELD, ID: 371994230, model: 746.
// Short name: SWE00607
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SET_FREQUENCY_TEXT_FIELD.
/// </summary>
[Serializable]
public partial class FnSetFrequencyTextField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SET_FREQUENCY_TEXT_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSetFrequencyTextField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSetFrequencyTextField.
  /// </summary>
  public FnSetFrequencyTextField(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.FrequencyWorkSet.FrequencyCode =
      import.ObligationPaymentSchedule.FrequencyCode;

    switch(TrimEnd(import.ObligationPaymentSchedule.FrequencyCode))
    {
      case "BW":
        export.FrequencyWorkSet.FrequencyDescription = "BI-WEEKLY";
        export.FrequencyWorkSet.Dow =
          import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();

        break;
      case "BM":
        export.FrequencyWorkSet.FrequencyDescription = "BI-MONTHLY";
        export.FrequencyWorkSet.Day1 =
          import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
        export.FrequencyWorkSet.Day2 =
          import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();

        break;
      case "SM":
        export.FrequencyWorkSet.FrequencyDescription = "SEMI-MONTHLY";
        export.FrequencyWorkSet.Day1 =
          import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
        export.FrequencyWorkSet.Day2 =
          import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();

        break;
      case "W":
        export.FrequencyWorkSet.FrequencyDescription = "WEEKLY";
        export.FrequencyWorkSet.Dow =
          import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();

        break;
      case "M":
        export.FrequencyWorkSet.FrequencyDescription = "MONTHLY";
        export.FrequencyWorkSet.Day1 =
          import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();

        break;
      default:
        ExitState = "FN0000_INVALID_FREQ_CODE";

        break;
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    private FrequencyWorkSet frequencyWorkSet;
  }
#endregion
}
