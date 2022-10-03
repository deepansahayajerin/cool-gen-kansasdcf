// Program: EAB_ALTS_READ_ALIAS, ID: 373488917, model: 746.
// Short name: SWEXIR12
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_ALTS_READ_ALIAS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class EabAltsReadAlias: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ALTS_READ_ALIAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabAltsReadAlias(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabAltsReadAlias.
  /// </summary>
  public EabAltsReadAlias(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXIR12", context, import, export, EabOptions.NoIefParams);
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
    [Member(Index = 1, Members = new[] { "Number", "UniqueKey" })]
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
    /// <summary>A AliasesGroup group.</summary>
    [Serializable]
    public class AliasesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      [Member(Index = 1, Members = new[]
      {
        "UniqueKey",
        "Ssn",
        "FirstName",
        "MiddleInitial",
        "LastName",
        "Sex",
        "Dob"
      })]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Gkscares.
      /// </summary>
      [JsonPropertyName("gkscares")]
      [Member(Index = 2, Members = new[] { "Flag" })]
      public Common Gkscares
      {
        get => gkscares ??= new();
        set => gkscares = value;
      }

      /// <summary>
      /// A value of Gkanpay.
      /// </summary>
      [JsonPropertyName("gkanpay")]
      [Member(Index = 3, Members = new[] { "Flag" })]
      public Common Gkanpay
      {
        get => gkanpay ??= new();
        set => gkanpay = value;
      }

      /// <summary>
      /// A value of Gcse.
      /// </summary>
      [JsonPropertyName("gcse")]
      [Member(Index = 4, Members = new[] { "Flag" })]
      public Common Gcse
      {
        get => gcse ??= new();
        set => gcse = value;
      }

      /// <summary>
      /// A value of Gae.
      /// </summary>
      [JsonPropertyName("gae")]
      [Member(Index = 5, Members = new[] { "Flag" })]
      public Common Gae
      {
        get => gae ??= new();
        set => gae = value;
      }

      /// <summary>
      /// A value of Gfacts.
      /// </summary>
      [JsonPropertyName("gfacts")]
      [Member(Index = 6, Members = new[] { "Flag" })]
      public Common Gfacts
      {
        get => gfacts ??= new();
        set => gfacts = value;
      }

      /// <summary>
      /// A value of GactiveOnKscares.
      /// </summary>
      [JsonPropertyName("gactiveOnKscares")]
      [Member(Index = 7, Members = new[] { "Flag" })]
      public Common GactiveOnKscares
      {
        get => gactiveOnKscares ??= new();
        set => gactiveOnKscares = value;
      }

      /// <summary>
      /// A value of GactiveOnKanpay.
      /// </summary>
      [JsonPropertyName("gactiveOnKanpay")]
      [Member(Index = 8, Members = new[] { "Flag" })]
      public Common GactiveOnKanpay
      {
        get => gactiveOnKanpay ??= new();
        set => gactiveOnKanpay = value;
      }

      /// <summary>
      /// A value of GactiveOnCse.
      /// </summary>
      [JsonPropertyName("gactiveOnCse")]
      [Member(Index = 9, Members = new[] { "Flag" })]
      public Common GactiveOnCse
      {
        get => gactiveOnCse ??= new();
        set => gactiveOnCse = value;
      }

      /// <summary>
      /// A value of GactiveOnAe.
      /// </summary>
      [JsonPropertyName("gactiveOnAe")]
      [Member(Index = 10, Members = new[] { "Flag" })]
      public Common GactiveOnAe
      {
        get => gactiveOnAe ??= new();
        set => gactiveOnAe = value;
      }

      /// <summary>
      /// A value of GactiveOnFacts.
      /// </summary>
      [JsonPropertyName("gactiveOnFacts")]
      [Member(Index = 11, Members = new[] { "Flag" })]
      public Common GactiveOnFacts
      {
        get => gactiveOnFacts ??= new();
        set => gactiveOnFacts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePersonsWorkSet g;
      private Common gkscares;
      private Common gkanpay;
      private Common gcse;
      private Common gae;
      private Common gfacts;
      private Common gactiveOnKscares;
      private Common gactiveOnKanpay;
      private Common gactiveOnCse;
      private Common gactiveOnAe;
      private Common gactiveOnFacts;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 1, Members = new[]
    {
      "Type1",
      "AdabasFileNumber",
      "AdabasFileAction",
      "AdabasResponseCd",
      "CicsResourceNm",
      "CicsFunctionCd",
      "CicsResponseCd"
    })]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of NextKey.
    /// </summary>
    [JsonPropertyName("nextKey")]
    [Member(Index = 2, Members = new[] { "UniqueKey" })]
    public CsePersonsWorkSet NextKey
    {
      get => nextKey ??= new();
      set => nextKey = value;
    }

    /// <summary>
    /// Gets a value of Aliases.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 3)]
    public Array<AliasesGroup> Aliases => aliases ??= new(
      AliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Aliases for json serialization.
    /// </summary>
    [JsonPropertyName("aliases")]
    [Computed]
    public IList<AliasesGroup> Aliases_Json
    {
      get => aliases;
      set => Aliases.Assign(value);
    }

    private AbendData abendData;
    private CsePersonsWorkSet nextKey;
    private Array<AliasesGroup> aliases;
  }
#endregion
}
