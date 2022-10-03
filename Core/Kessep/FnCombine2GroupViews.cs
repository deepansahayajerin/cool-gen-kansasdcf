// Program: FN_COMBINE_2_GROUP_VIEWS, ID: 371966382, model: 746.
// Short name: SWE00324
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_COMBINE_2_GROUP_VIEWS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block is called by the list obligation type relationships 
/// procedure.  It simply combines 2 group views, one containing Obligation
/// Types related to a Distribution Policy Rule, and the other containing
/// Obligation Types not related.
/// </para>
/// </summary>
[Serializable]
public partial class FnCombine2GroupViews: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMBINE_2_GROUP_VIEWS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCombine2GroupViews(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCombine2GroupViews.
  /// </summary>
  public FnCombine2GroupViews(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Export1.Index = -1;

    if (!import.Related.IsEmpty)
    {
      for(import.Related.Index = 0; import.Related.Index < import
        .Related.Count; ++import.Related.Index)
      {
        if (!import.Related.CheckSize())
        {
          break;
        }

        export.Export1.Index = import.Related.Index;
        export.Export1.CheckSize();

        export.Export1.Update.Common.SelectChar =
          import.Related.Item.Common.SelectChar;
        MoveObligationType(import.Related.Item.ObligationType,
          export.Export1.Update.ObligationType);
        export.Export1.Update.Related1.SelectChar =
          import.Related.Item.Related1.SelectChar;
      }

      import.Related.CheckIndex();
    }

    for(import.NotRelated.Index = 0; import.NotRelated.Index < import
      .NotRelated.Count; ++import.NotRelated.Index)
    {
      if (!import.NotRelated.CheckSize())
      {
        break;
      }

      ++export.Export1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Common.SelectChar =
        import.NotRelated.Item.Grp1Common.SelectChar;
      MoveObligationType(import.NotRelated.Item.Grp1ObligationType,
        export.Export1.Update.ObligationType);
      export.Export1.Update.Related1.SelectChar =
        import.NotRelated.Item.Grp1RelFlag.SelectChar;
    }

    import.NotRelated.CheckIndex();
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SupportedPersonReqInd = source.SupportedPersonReqInd;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
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
    /// <summary>A RelatedGroup group.</summary>
    [Serializable]
    public class RelatedGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of Related1.
      /// </summary>
      [JsonPropertyName("related1")]
      public Common Related1
      {
        get => related1 ??= new();
        set => related1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private ObligationType obligationType;
      private Common related1;
    }

    /// <summary>A NotRelatedGroup group.</summary>
    [Serializable]
    public class NotRelatedGroup
    {
      /// <summary>
      /// A value of Grp1Common.
      /// </summary>
      [JsonPropertyName("grp1Common")]
      public Common Grp1Common
      {
        get => grp1Common ??= new();
        set => grp1Common = value;
      }

      /// <summary>
      /// A value of Grp1ObligationType.
      /// </summary>
      [JsonPropertyName("grp1ObligationType")]
      public ObligationType Grp1ObligationType
      {
        get => grp1ObligationType ??= new();
        set => grp1ObligationType = value;
      }

      /// <summary>
      /// A value of Grp1RelFlag.
      /// </summary>
      [JsonPropertyName("grp1RelFlag")]
      public Common Grp1RelFlag
      {
        get => grp1RelFlag ??= new();
        set => grp1RelFlag = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common grp1Common;
      private ObligationType grp1ObligationType;
      private Common grp1RelFlag;
    }

    /// <summary>
    /// Gets a value of Related.
    /// </summary>
    [JsonIgnore]
    public Array<RelatedGroup> Related => related ??= new(
      RelatedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Related for json serialization.
    /// </summary>
    [JsonPropertyName("related")]
    [Computed]
    public IList<RelatedGroup> Related_Json
    {
      get => related;
      set => Related.Assign(value);
    }

    /// <summary>
    /// Gets a value of NotRelated.
    /// </summary>
    [JsonIgnore]
    public Array<NotRelatedGroup> NotRelated => notRelated ??= new(
      NotRelatedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NotRelated for json serialization.
    /// </summary>
    [JsonPropertyName("notRelated")]
    [Computed]
    public IList<NotRelatedGroup> NotRelated_Json
    {
      get => notRelated;
      set => NotRelated.Assign(value);
    }

    private Array<RelatedGroup> related;
    private Array<NotRelatedGroup> notRelated;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of Related1.
      /// </summary>
      [JsonPropertyName("related1")]
      public Common Related1
      {
        get => related1 ??= new();
        set => related1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private ObligationType obligationType;
      private Common related1;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
  }
#endregion
}
