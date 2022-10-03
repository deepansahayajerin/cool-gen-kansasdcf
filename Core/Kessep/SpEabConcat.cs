// Program: SP_EAB_CONCAT, ID: 372117141, model: 746.
// Short name: SWEXNE02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_EAB_CONCAT.
/// </para>
/// <para>
/// As the IEF concat generates code which is lengthy, this is an attempt to 
/// reduce the same.
/// </para>
/// </summary>
[Serializable]
public partial class SpEabConcat: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EAB_CONCAT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEabConcat(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEabConcat.
  /// </summary>
  public SpEabConcat(IContext context, Import import, Export export):
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
      "SWEXNE02", context, import, export, EabOptions.NoIefParams);
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
    /// A value of Input1.
    /// </summary>
    [JsonPropertyName("input1")]
    [Member(Index = 1, Members = new[] { "Value" })]
    public FieldValue Input1
    {
      get => input1 ??= new();
      set => input1 = value;
    }

    /// <summary>
    /// A value of Input2.
    /// </summary>
    [JsonPropertyName("input2")]
    [Member(Index = 2, Members = new[] { "Value" })]
    public FieldValue Input2
    {
      get => input2 ??= new();
      set => input2 = value;
    }

    private FieldValue input1;
    private FieldValue input2;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    [Member(Index = 1, Members = new[] { "Value" })]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private FieldValue fieldValue;
  }
#endregion
}
