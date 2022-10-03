// Program: FN_OCSE157_CLEAR_GROUP_VIEW, ID: 371113563, model: 746.
// Short name: SWE02962
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_CLEAR_GROUP_VIEW.
/// </summary>
[Serializable]
public partial class FnOcse157ClearGroupView: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_CLEAR_GROUP_VIEW program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157ClearGroupView(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157ClearGroupView.
  /// </summary>
  public FnOcse157ClearGroupView(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of Ocse157Verification.
      /// </summary>
      [JsonPropertyName("ocse157Verification")]
      public Ocse157Verification Ocse157Verification
      {
        get => ocse157Verification ??= new();
        set => ocse157Verification = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private Ocse157Verification ocse157Verification;
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
    }

    private Array<NullGroup> null1;
  }
#endregion
}
