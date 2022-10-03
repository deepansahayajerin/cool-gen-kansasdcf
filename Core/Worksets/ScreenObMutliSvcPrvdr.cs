// The source file: SCREEN_OB_MUTLI_SVC_PRVDR, ID: 371740941, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This Workset will provide the screen with a multiple service provider 
/// indicator for obligations/debts.
/// </summary>
[Serializable]
public partial class ScreenObMutliSvcPrvdr: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ScreenObMutliSvcPrvdr()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ScreenObMutliSvcPrvdr(ScreenObMutliSvcPrvdr that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ScreenObMutliSvcPrvdr Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ScreenObMutliSvcPrvdr that)
  {
    base.Assign(that);
    multiServiceProviderInd = that.multiServiceProviderInd;
  }

  /// <summary>Length of the MULTI_SERVICE_PROVIDER_IND attribute.</summary>
  public const int MultiServiceProviderInd_MaxLength = 1;

  /// <summary>
  /// The value of the MULTI_SERVICE_PROVIDER_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = MultiServiceProviderInd_MaxLength)]
  public string MultiServiceProviderInd
  {
    get => multiServiceProviderInd ?? "";
    set => multiServiceProviderInd =
      TrimEnd(Substring(value, 1, MultiServiceProviderInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MultiServiceProviderInd attribute.</summary>
  [JsonPropertyName("multiServiceProviderInd")]
  [Computed]
  public string MultiServiceProviderInd_Json
  {
    get => NullIf(MultiServiceProviderInd, "");
    set => MultiServiceProviderInd = value;
  }

  private string multiServiceProviderInd;
}
