// Program: CO_EAB_CONCAT_10_DELIMIT, ID: 372318233, model: 746.
// Short name: SWEXPE20
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CO_EAB_CONCAT_10_DELIMIT.
/// </para>
/// <para>
/// Concat 10 strings with provided delimitter
/// </para>
/// </summary>
[Serializable]
public partial class CoEabConcat10Delimit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_EAB_CONCAT_10_DELIMIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoEabConcat10Delimit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoEabConcat10Delimit.
  /// </summary>
  public CoEabConcat10Delimit(IContext context, Import import, Export export):
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
      "SWEXPE20", context, import, export, EabOptions.NoIefParams);
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
    /// <summary>A StringsGroup group.</summary>
    [Serializable]
    public class StringsGroup
    {
      /// <summary>
      /// A value of String1.
      /// </summary>
      [JsonPropertyName("string1")]
      [Member(Index = 1, Members = new[] { "Text32" })]
      public WorkArea String1
      {
        get => string1 ??= new();
        set => string1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea string1;
    }

    /// <summary>
    /// A value of DelimiterLength.
    /// </summary>
    [JsonPropertyName("delimiterLength")]
    [Member(Index = 1, Members = new[] { "Count" })]
    public Common DelimiterLength
    {
      get => delimiterLength ??= new();
      set => delimiterLength = value;
    }

    /// <summary>
    /// A value of Delimiter.
    /// </summary>
    [JsonPropertyName("delimiter")]
    [Member(Index = 2, Members = new[] { "Text5" })]
    public WorkArea Delimiter
    {
      get => delimiter ??= new();
      set => delimiter = value;
    }

    /// <summary>
    /// Gets a value of Strings.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 3)]
    public Array<StringsGroup> Strings =>
      strings ??= new(StringsGroup.Capacity);

    /// <summary>
    /// Gets a value of Strings for json serialization.
    /// </summary>
    [JsonPropertyName("strings")]
    [Computed]
    public IList<StringsGroup> Strings_Json
    {
      get => strings;
      set => Strings.Assign(value);
    }

    private Common delimiterLength;
    private WorkArea delimiter;
    private Array<StringsGroup> strings;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ResultString.
    /// </summary>
    [JsonPropertyName("resultString")]
    [Member(Index = 1, Members = new[] { "Text80" })]
    public WorkArea ResultString
    {
      get => resultString ??= new();
      set => resultString = value;
    }

    private WorkArea resultString;
  }
#endregion
}
