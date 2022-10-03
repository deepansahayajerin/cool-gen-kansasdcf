// Program: CAB_INITIALIZE_ARRAYS, ID: 372819902, model: 746.
// Short name: SWEFG750
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_INITIALIZE_ARRAYS.
/// </summary>
[Serializable]
public partial class CabInitializeArrays: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_INITIALIZE_ARRAYS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabInitializeArrays(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabInitializeArrays.
  /// </summary>
  public CabInitializeArrays(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***
    // *** This CAB has export views only and is used to initialize
    // *** the following 2 dimensional arrays in the Prad
    // *** SRRUN156_COLLECTIONS_REPORT:
    // ***
    // *** Collection Officer (CO)
    // *** Section Supervisor (SS)
    // *** Region (REGION)
    // ***
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
    /// <summary>A AcrossCoGroup group.</summary>
    [Serializable]
    public class AcrossCoGroup
    {
      /// <summary>
      /// Gets a value of DownCo.
      /// </summary>
      [JsonIgnore]
      public Array<DownCoGroup> DownCo =>
        downCo ??= new(DownCoGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownCo for json serialization.
      /// </summary>
      [JsonPropertyName("downCo")]
      [Computed]
      public IList<DownCoGroup> DownCo_Json
      {
        get => downCo;
        set => DownCo.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownCoGroup> downCo;
    }

    /// <summary>A DownCoGroup group.</summary>
    [Serializable]
    public class DownCoGroup
    {
      /// <summary>
      /// A value of DtlCoCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlCoCollectionsExtract")]
      public CollectionsExtract DtlCoCollectionsExtract
      {
        get => dtlCoCollectionsExtract ??= new();
        set => dtlCoCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlCoCommon.
      /// </summary>
      [JsonPropertyName("dtlCoCommon")]
      public Common DtlCoCommon
      {
        get => dtlCoCommon ??= new();
        set => dtlCoCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlCoCollectionsExtract;
      private Common dtlCoCommon;
    }

    /// <summary>A AcrossSsGroup group.</summary>
    [Serializable]
    public class AcrossSsGroup
    {
      /// <summary>
      /// Gets a value of DownSs.
      /// </summary>
      [JsonIgnore]
      public Array<DownSsGroup> DownSs =>
        downSs ??= new(DownSsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownSs for json serialization.
      /// </summary>
      [JsonPropertyName("downSs")]
      [Computed]
      public IList<DownSsGroup> DownSs_Json
      {
        get => downSs;
        set => DownSs.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownSsGroup> downSs;
    }

    /// <summary>A DownSsGroup group.</summary>
    [Serializable]
    public class DownSsGroup
    {
      /// <summary>
      /// A value of DtlSsCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlSsCollectionsExtract")]
      public CollectionsExtract DtlSsCollectionsExtract
      {
        get => dtlSsCollectionsExtract ??= new();
        set => dtlSsCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlSsCommon.
      /// </summary>
      [JsonPropertyName("dtlSsCommon")]
      public Common DtlSsCommon
      {
        get => dtlSsCommon ??= new();
        set => dtlSsCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlSsCollectionsExtract;
      private Common dtlSsCommon;
    }

    /// <summary>A AcrossRegionGroup group.</summary>
    [Serializable]
    public class AcrossRegionGroup
    {
      /// <summary>
      /// Gets a value of DownRegion.
      /// </summary>
      [JsonIgnore]
      public Array<DownRegionGroup> DownRegion => downRegion ??= new(
        DownRegionGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownRegion for json serialization.
      /// </summary>
      [JsonPropertyName("downRegion")]
      [Computed]
      public IList<DownRegionGroup> DownRegion_Json
      {
        get => downRegion;
        set => DownRegion.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownRegionGroup> downRegion;
    }

    /// <summary>A DownRegionGroup group.</summary>
    [Serializable]
    public class DownRegionGroup
    {
      /// <summary>
      /// A value of DtlRegionCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlRegionCollectionsExtract")]
      public CollectionsExtract DtlRegionCollectionsExtract
      {
        get => dtlRegionCollectionsExtract ??= new();
        set => dtlRegionCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlRegionCommon.
      /// </summary>
      [JsonPropertyName("dtlRegionCommon")]
      public Common DtlRegionCommon
      {
        get => dtlRegionCommon ??= new();
        set => dtlRegionCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlRegionCollectionsExtract;
      private Common dtlRegionCommon;
    }

    /// <summary>
    /// Gets a value of AcrossCo.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossCoGroup> AcrossCo => acrossCo ??= new(
      AcrossCoGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossCo for json serialization.
    /// </summary>
    [JsonPropertyName("acrossCo")]
    [Computed]
    public IList<AcrossCoGroup> AcrossCo_Json
    {
      get => acrossCo;
      set => AcrossCo.Assign(value);
    }

    /// <summary>
    /// Gets a value of AcrossSs.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossSsGroup> AcrossSs => acrossSs ??= new(
      AcrossSsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossSs for json serialization.
    /// </summary>
    [JsonPropertyName("acrossSs")]
    [Computed]
    public IList<AcrossSsGroup> AcrossSs_Json
    {
      get => acrossSs;
      set => AcrossSs.Assign(value);
    }

    /// <summary>
    /// Gets a value of AcrossRegion.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossRegionGroup> AcrossRegion => acrossRegion ??= new(
      AcrossRegionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossRegion for json serialization.
    /// </summary>
    [JsonPropertyName("acrossRegion")]
    [Computed]
    public IList<AcrossRegionGroup> AcrossRegion_Json
    {
      get => acrossRegion;
      set => AcrossRegion.Assign(value);
    }

    private Array<AcrossCoGroup> acrossCo;
    private Array<AcrossSsGroup> acrossSs;
    private Array<AcrossRegionGroup> acrossRegion;
  }
#endregion
}
