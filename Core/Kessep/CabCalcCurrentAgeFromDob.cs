// Program: CAB_CALC_CURRENT_AGE_FROM_DOB, ID: 372638456, model: 746.
// Short name: SWE01937
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_CALC_CURRENT_AGE_FROM_DOB.
/// </para>
/// <para>
/// takes dob and calculates current age.
/// </para>
/// </summary>
[Serializable]
public partial class CabCalcCurrentAgeFromDob: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CALC_CURRENT_AGE_FROM_DOB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCalcCurrentAgeFromDob(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCalcCurrentAgeFromDob.
  /// </summary>
  public CabCalcCurrentAgeFromDob(IContext context, Import import, Export export)
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
    export.Common.TotalInteger =
      (long)Math.Round(
        DaysFromAD(Now().Date.AddDays(-DaysFromAD(import.CsePersonsWorkSet.Dob)))
      / 365.25M, MidpointRounding.AwayFromZero);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
#endregion
}
