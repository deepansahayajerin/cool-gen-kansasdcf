// Program: CAB_SUBMIT_JOB_TO_JES, ID: 371136586, model: 746.
// Short name: SWEXGE04
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_SUBMIT_JOB_TO_JES.
/// </summary>
[Serializable]
public partial class CabSubmitJobToJes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_SUBMIT_JOB_TO_JES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabSubmitJobToJes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabSubmitJobToJes.
  /// </summary>
  public CabSubmitJobToJes(IContext context, Import import, Export export):
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
      "SWEXGE04", context, import, export, EabOptions.NoIefParams |
      EabOptions.NoAS);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of JclTemplate.
      /// </summary>
      [JsonPropertyName("jclTemplate")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "RecordText" })]
        
      public JclTemplate JclTemplate
      {
        get => jclTemplate ??= new();
        set => jclTemplate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private JclTemplate jclTemplate;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 1)]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "NumericReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
