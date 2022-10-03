// Program: FN_B644_VALIDATE_PARAMETERS, ID: 372692015, model: 746.
// Short name: SWE02458
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B644_VALIDATE_PARAMETERS.
/// </summary>
[Serializable]
public partial class FnB644ValidateParameters: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B644_VALIDATE_PARAMETERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB644ValidateParameters(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB644ValidateParameters.
  /// </summary>
  public FnB644ValidateParameters(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ValidDate.Flag = "N";

    if (Lt("00", Substring(import.ProgramProcessingInfo.ParameterList, 1, 2)) &&
      Lt(Substring(import.ProgramProcessingInfo.ParameterList, 1, 2), "13"))
    {
      local.Work.Count =
        (int)StringToNumber(Substring(
          import.ProgramProcessingInfo.ParameterList, 1, 2));
      local.Conversion.Month = local.Work.Count;

      if (Lt("00", Substring(import.ProgramProcessingInfo.ParameterList, 3, 2)) &&
        Lt(Substring(import.ProgramProcessingInfo.ParameterList, 1, 2), "32"))
      {
        local.Work.Count =
          (int)StringToNumber(Substring(
            import.ProgramProcessingInfo.ParameterList, 3, 2));
        local.Conversion.Day = local.Work.Count;

        if (Lt("1980", Substring(import.ProgramProcessingInfo.ParameterList, 5,
          4)) && Lt
          (Substring(import.ProgramProcessingInfo.ParameterList, 1, 2), "2399"))
        {
          local.Work.Count =
            (int)StringToNumber(Substring(
              import.ProgramProcessingInfo.ParameterList, 5, 4));
          local.Conversion.Year = local.Work.Count;
          export.Conversion.Date = IntToDate(local.Conversion.Year * 10000 + local
            .Conversion.Month * 100 + local.Conversion.Day);
          export.ValidDate.Flag = "Y";
        }
        else
        {
        }
      }
      else
      {
      }
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    /// <summary>
    /// A value of ValidDate.
    /// </summary>
    [JsonPropertyName("validDate")]
    public Common ValidDate
    {
      get => validDate ??= new();
      set => validDate = value;
    }

    private DateWorkArea conversion;
    private Common validDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    private Common work;
    private DateWorkArea conversion;
  }
#endregion
}
