// Program: CAB_COMPARE_EFFEC_AND_DISC_DATES, ID: 371725005, model: 746.
// Short name: SWE00027
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_COMPARE_EFFEC_AND_DISC_DATES.
/// </para>
/// <para>
/// This action block will take an effective date and expiration/discontinue 
/// date and compare the two.
/// If the expiration date has a date value, it will determine if the expiration
/// date is greater than the effective date.
/// If the expiration date is empty, it will assume that is o.k.  If expiration 
/// date is required you need to edit that outside of this action block.
/// For effective date it will determine if the effective date is retro-active.
/// For expiration date it will determine if the expiration date is retro 
/// active.
/// The results are returned in a workattribute set of switches since more than 
/// one condition could occur and the acceptance of them is dependent on the
/// business situation.
/// </para>
/// </summary>
[Serializable]
public partial class CabCompareEffecAndDiscDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_COMPARE_EFFEC_AND_DISC_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCompareEffecAndDiscDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCompareEffecAndDiscDates.
  /// </summary>
  public CabCompareEffecAndDiscDates(IContext context, Import import,
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
    if (Equal(import.ExpireEffectiveDateAttributes.EffectiveDate,
      local.ZeroValued.EffectiveDate))
    {
      export.ExpireEffectiveDateAttributes.EffectiveDateIsZero = "Y";
    }
    else
    {
      export.ExpireEffectiveDateAttributes.EffectiveDateIsZero = "N";
    }

    if (Equal(import.ExpireEffectiveDateAttributes.ExpirationDate,
      local.ZeroValued.ExpirationDate))
    {
      export.ExpireEffectiveDateAttributes.ExpirationDateIsZero = "Y";
    }
    else
    {
      export.ExpireEffectiveDateAttributes.ExpirationDateIsZero = "N";
    }

    if (Lt(import.ExpireEffectiveDateAttributes.EffectiveDate, Now().Date))
    {
      export.ExpireEffectiveDateAttributes.EffectiveDateIsLtCurrent = "Y";
    }
    else
    {
      export.ExpireEffectiveDateAttributes.EffectiveDateIsLtCurrent = "N";
    }

    if (Lt(import.ExpireEffectiveDateAttributes.ExpirationDate, Now().Date))
    {
      export.ExpireEffectiveDateAttributes.ExpirationDateIsLtCurrent = "Y";
    }
    else
    {
      export.ExpireEffectiveDateAttributes.ExpirationDateIsLtCurrent = "N";
    }

    if (Lt(import.ExpireEffectiveDateAttributes.ExpirationDate,
      import.ExpireEffectiveDateAttributes.EffectiveDate))
    {
      export.ExpireEffectiveDateAttributes.ExpirationDateLtEffectiveDat = "Y";
    }
    else
    {
      export.ExpireEffectiveDateAttributes.ExpirationDateLtEffectiveDat = "N";
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
    /// A value of ExpireEffectiveDateAttributes.
    /// </summary>
    [JsonPropertyName("expireEffectiveDateAttributes")]
    public ExpireEffectiveDateAttributes ExpireEffectiveDateAttributes
    {
      get => expireEffectiveDateAttributes ??= new();
      set => expireEffectiveDateAttributes = value;
    }

    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExpireEffectiveDateAttributes.
    /// </summary>
    [JsonPropertyName("expireEffectiveDateAttributes")]
    public ExpireEffectiveDateAttributes ExpireEffectiveDateAttributes
    {
      get => expireEffectiveDateAttributes ??= new();
      set => expireEffectiveDateAttributes = value;
    }

    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroValued.
    /// </summary>
    [JsonPropertyName("zeroValued")]
    public ExpireEffectiveDateAttributes ZeroValued
    {
      get => zeroValued ??= new();
      set => zeroValued = value;
    }

    private ExpireEffectiveDateAttributes zeroValued;
  }
#endregion
}
