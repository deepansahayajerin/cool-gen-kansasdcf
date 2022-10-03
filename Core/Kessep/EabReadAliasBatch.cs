// Program: EAB_READ_ALIAS_BATCH, ID: 372363060, model: 746.
// Short name: SWEXIR15
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_READ_ALIAS_BATCH.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class EabReadAliasBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_ALIAS_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadAliasBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadAliasBatch.
  /// </summary>
  public EabReadAliasBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXIR15", context, import, export, 0);
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet g;
      private Common gkscares;
      private Common gkanpay;
      private Common gcse;
      private Common gae;
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
    public Array<AliasesGroup> Aliases =>
      aliases ??= new(AliasesGroup.Capacity);

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
