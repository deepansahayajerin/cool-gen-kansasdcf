// Program: EAB_READ_NARRATIVE, ID: 371751690, model: 746.
// Short name: SWEXIR40
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_READ_NARRATIVE.
/// </para>
/// <para>
/// RESP: srvplan	
///   read narrative recs from current cse kaecses system
/// </para>
/// </summary>
[Serializable]
public partial class EabReadNarrative: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_NARRATIVE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadNarrative(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadNarrative.
  /// </summary>
  public EabReadNarrative(IContext context, Import import, Export export):
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
      "SWEXIR40", context, import, export, EabOptions.NoIefParams);
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
    /// A value of NarrWorkData.
    /// </summary>
    [JsonPropertyName("narrWorkData")]
    [Member(Index = 1, Members = new[] { "NarrativeDate", "CseCaseNumber" })]
    public NarrWorkData NarrWorkData
    {
      get => narrWorkData ??= new();
      set => narrWorkData = value;
    }

    private NarrWorkData narrWorkData;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of NarrWorkData.
      /// </summary>
      [JsonPropertyName("narrWorkData")]
      [Member(Index = 1, Members = new[]
      {
        "NarrativeDate",
        "CseCaseNumber",
        "NarrativeType",
        "NarrativeDesc",
        "Region",
        "Team"
      })]
      public NarrWorkData NarrWorkData
      {
        get => narrWorkData ??= new();
        set => narrWorkData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 174;

      private NarrWorkData narrWorkData;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 2)]
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

    private AbendData abendData;
    private Array<GroupGroup> group;
  }
#endregion
}
