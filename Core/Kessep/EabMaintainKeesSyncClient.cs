// Program: EAB_MAINTAIN_KEES_SYNC_CLIENT, ID: 1625311587, model: 746.
// Short name: SWEXIU50
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_MAINTAIN_KEES_SYNC_CLIENT.
/// </para>
/// <para>
/// This External Action Block will maintain KEES KSD_CLIENT_BASIC 	information 
/// with respect to Primary and Secondary indicator and Preferred Id value.
/// </para>
/// </summary>
[Serializable]
public partial class EabMaintainKeesSyncClient: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_MAINTAIN_KEES_SYNC_CLIENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabMaintainKeesSyncClient(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabMaintainKeesSyncClient.
  /// </summary>
  public EabMaintainKeesSyncClient(IContext context, Import import,
    Export export):
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
      "SWEXIU50", context, import, export, EabOptions.Hpvp |
      EabOptions.NoIefParams);
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
    /// <summary>A IdsGroup group.</summary>
    [Serializable]
    public class IdsGroup
    {
      /// <summary>
      /// A value of GimportInputIds.
      /// </summary>
      [JsonPropertyName("gimportInputIds")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
      public CsePersonsWorkSet GimportInputIds
      {
        get => gimportInputIds ??= new();
        set => gimportInputIds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gimportInputIds;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "SelectChar" })]
        
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimportPersonInfo.
      /// </summary>
      [JsonPropertyName("gimportPersonInfo")]
      [Member(Index = 2, AccessFields = false, Members = new[]
      {
        "ReplicationIndicator",
        "FormattedName",
        "Number",
        "Ssn",
        "Dob",
        "Sex",
        "FirstName",
        "MiddleInitial",
        "LastName"
      })]
      public CsePersonsWorkSet GimportPersonInfo
      {
        get => gimportPersonInfo ??= new();
        set => gimportPersonInfo = value;
      }

      /// <summary>
      /// A value of GimportAeFlag.
      /// </summary>
      [JsonPropertyName("gimportAeFlag")]
      [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
      public Common GimportAeFlag
      {
        get => gimportAeFlag ??= new();
        set => gimportAeFlag = value;
      }

      /// <summary>
      /// A value of GimportCsFlag.
      /// </summary>
      [JsonPropertyName("gimportCsFlag")]
      [Member(Index = 4, AccessFields = false, Members = new[] { "Flag" })]
      public Common GimportCsFlag
      {
        get => gimportCsFlag ??= new();
        set => gimportCsFlag = value;
      }

      /// <summary>
      /// A value of GimportFaFlag.
      /// </summary>
      [JsonPropertyName("gimportFaFlag")]
      [Member(Index = 5, AccessFields = false, Members = new[] { "Flag" })]
      public Common GimportFaFlag
      {
        get => gimportFaFlag ??= new();
        set => gimportFaFlag = value;
      }

      /// <summary>
      /// A value of GimportKmFlag.
      /// </summary>
      [JsonPropertyName("gimportKmFlag")]
      [Member(Index = 6, AccessFields = false, Members = new[] { "Flag" })]
      public Common GimportKmFlag
      {
        get => gimportKmFlag ??= new();
        set => gimportKmFlag = value;
      }

      /// <summary>
      /// A value of GimportPreferredId.
      /// </summary>
      [JsonPropertyName("gimportPreferredId")]
      [Member(Index = 7, AccessFields = false, Members = new[] { "Number" })]
      public CsePersonsWorkSet GimportPreferredId
      {
        get => gimportPreferredId ??= new();
        set => gimportPreferredId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common g;
      private CsePersonsWorkSet gimportPersonInfo;
      private Common gimportAeFlag;
      private Common gimportCsFlag;
      private Common gimportFaFlag;
      private Common gimportKmFlag;
      private CsePersonsWorkSet gimportPreferredId;
    }

    /// <summary>
    /// A value of EabDmlFlag.
    /// </summary>
    [JsonPropertyName("eabDmlFlag")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Flag" })]
    public Common EabDmlFlag
    {
      get => eabDmlFlag ??= new();
      set => eabDmlFlag = value;
    }

    /// <summary>
    /// A value of UpdatePersonWorkSet.
    /// </summary>
    [JsonPropertyName("updatePersonWorkSet")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "ReplicationIndicator",
      "FormattedName",
      "Number",
      "Ssn",
      "Dob",
      "Sex",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet UpdatePersonWorkSet
    {
      get => updatePersonWorkSet ??= new();
      set => updatePersonWorkSet = value;
    }

    /// <summary>
    /// A value of UpdatePreferredId.
    /// </summary>
    [JsonPropertyName("updatePreferredId")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Number" })]
    public CsePersonsWorkSet UpdatePreferredId
    {
      get => updatePreferredId ??= new();
      set => updatePreferredId = value;
    }

    /// <summary>
    /// Gets a value of Ids.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 4)]
    public Array<IdsGroup> Ids => ids ??= new(IdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ids for json serialization.
    /// </summary>
    [JsonPropertyName("ids")]
    [Computed]
    public IList<IdsGroup> Ids_Json
    {
      get => ids;
      set => Ids.Assign(value);
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 5)]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    private Common eabDmlFlag;
    private CsePersonsWorkSet updatePersonWorkSet;
    private CsePersonsWorkSet updatePreferredId;
    private Array<IdsGroup> ids;
    private Array<ListGroup> list;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "SelectChar" })]
        
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportPersonInfo.
      /// </summary>
      [JsonPropertyName("gexportPersonInfo")]
      [Member(Index = 2, AccessFields = false, Members = new[]
      {
        "ReplicationIndicator",
        "FormattedName",
        "Number",
        "Ssn",
        "Dob",
        "Sex",
        "FirstName",
        "MiddleInitial",
        "LastName"
      })]
      public CsePersonsWorkSet GexportPersonInfo
      {
        get => gexportPersonInfo ??= new();
        set => gexportPersonInfo = value;
      }

      /// <summary>
      /// A value of GexportAeFlag.
      /// </summary>
      [JsonPropertyName("gexportAeFlag")]
      [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
      public Common GexportAeFlag
      {
        get => gexportAeFlag ??= new();
        set => gexportAeFlag = value;
      }

      /// <summary>
      /// A value of GexportCsFlag.
      /// </summary>
      [JsonPropertyName("gexportCsFlag")]
      [Member(Index = 4, AccessFields = false, Members = new[] { "Flag" })]
      public Common GexportCsFlag
      {
        get => gexportCsFlag ??= new();
        set => gexportCsFlag = value;
      }

      /// <summary>
      /// A value of GexportFaFlag.
      /// </summary>
      [JsonPropertyName("gexportFaFlag")]
      [Member(Index = 5, AccessFields = false, Members = new[] { "Flag" })]
      public Common GexportFaFlag
      {
        get => gexportFaFlag ??= new();
        set => gexportFaFlag = value;
      }

      /// <summary>
      /// A value of GexportKmFlag.
      /// </summary>
      [JsonPropertyName("gexportKmFlag")]
      [Member(Index = 6, AccessFields = false, Members = new[] { "Flag" })]
      public Common GexportKmFlag
      {
        get => gexportKmFlag ??= new();
        set => gexportKmFlag = value;
      }

      /// <summary>
      /// A value of GexportPreferredId.
      /// </summary>
      [JsonPropertyName("gexportPreferredId")]
      [Member(Index = 7, AccessFields = false, Members = new[] { "Number" })]
      public CsePersonsWorkSet GexportPreferredId
      {
        get => gexportPreferredId ??= new();
        set => gexportPreferredId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common g;
      private CsePersonsWorkSet gexportPersonInfo;
      private Common gexportAeFlag;
      private Common gexportCsFlag;
      private Common gexportFaFlag;
      private Common gexportKmFlag;
      private CsePersonsWorkSet gexportPreferredId;
    }

    /// <summary>
    /// A value of EabDmlFlag.
    /// </summary>
    [JsonPropertyName("eabDmlFlag")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Flag" })]
    public Common EabDmlFlag
    {
      get => eabDmlFlag ??= new();
      set => eabDmlFlag = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 2)]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    /// <summary>
    /// A value of DmlReturnCode.
    /// </summary>
    [JsonPropertyName("dmlReturnCode")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common DmlReturnCode
    {
      get => dmlReturnCode ??= new();
      set => dmlReturnCode = value;
    }

    private Common eabDmlFlag;
    private Array<ListGroup> list;
    private Common dmlReturnCode;
  }
#endregion
}
