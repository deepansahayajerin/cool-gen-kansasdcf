// Program: CAB_READ_NARRATIVE, ID: 371751536, model: 746.
// Short name: SWE01882
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_NARRATIVE.
/// </para>
/// <para>
/// This AB calls the EAB which retrieves narrative records for the case which 
/// is passed to it from screen KCAS.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadNarrative: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_NARRATIVE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadNarrative(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadNarrative.
  /// </summary>
  public CabReadNarrative(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    UseEabReadNarrative();

    switch(AsChar(export.AbendData.Type1))
    {
      case 'A':
        switch(TrimEnd(export.AbendData.AdabasFileNumber))
        {
          case "0000":
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            break;
          case "0150":
            ExitState = "ADABAS_READ_UNSUCCESSFUL";

            break;
          default:
            break;
        }

        break;
      case 'C':
        ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

        break;
      default:
        break;
    }
  }

  private static void MoveGroup1(Export.GroupGroup source,
    EabReadNarrative.Export.GroupGroup target)
  {
    target.NarrWorkData.Assign(source.NarrWorkData);
  }

  private static void MoveGroup2(EabReadNarrative.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.NarrWorkData.Assign(source.NarrWorkData);
  }

  private static void MoveNarrWorkData(NarrWorkData source, NarrWorkData target)
  {
    target.CseCaseNumber = source.CseCaseNumber;
    target.NarrativeDate = source.NarrativeDate;
  }

  private void UseEabReadNarrative()
  {
    var useImport = new EabReadNarrative.Import();
    var useExport = new EabReadNarrative.Export();

    MoveNarrWorkData(import.NarrWorkData, useImport.NarrWorkData);
    useExport.AbendData.Assign(export.AbendData);
    export.Group.CopyTo(useExport.Group, MoveGroup1);

    Call(EabReadNarrative.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
    useExport.Group.CopyTo(export.Group, MoveGroup2);
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
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
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
