// Program: FN_COMBINE_COLLECTIONS_BY_OBLIG, ID: 371738802, model: 746.
// Short name: SWE02083
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COMBINE_COLLECTIONS_BY_OBLIG.
/// </summary>
[Serializable]
public partial class FnCombineCollectionsByOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMBINE_COLLECTIONS_BY_OBLIG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCombineCollectionsByOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCombineCollectionsByOblig.
  /// </summary>
  public FnCombineCollectionsByOblig(IContext context, Import import,
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
    export.Group.Index = -1;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.Index == -1)
      {
        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.CashReceipt.Assign(import.Group.Item.CashReceipt);
        export.Group.Update.Status.Code = import.Group.Item.Status.Code;
        export.Group.Update.DistToOblig.TotalCurrency =
          import.Group.Item.DistToOblig.TotalCurrency;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        MoveCashReceiptType(import.Group.Item.CashReceiptType,
          export.Group.Update.CashReceiptType);
        MoveCashReceiptSourceType(import.Group.Item.CashReceiptSourceType,
          export.Group.Update.CashReceiptSourceType);
        MoveCashReceiptEvent(import.Group.Item.CashReceiptEvent,
          export.Group.Update.CashReceiptEvent);
        export.Group.Update.CashReceiptDetail.Assign(
          import.Group.Item.CashReceiptDetail);
      }
      else if (import.Group.Item.CashReceipt.SequentialNumber == export
        .Group.Item.CashReceipt.SequentialNumber && import
        .Group.Item.CashReceiptDetail.SequentialIdentifier == export
        .Group.Item.CashReceiptDetail.SequentialIdentifier)
      {
        export.Group.Update.DistToOblig.TotalCurrency =
          export.Group.Item.DistToOblig.TotalCurrency + import
          .Group.Item.DistToOblig.TotalCurrency;
      }
      else
      {
        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.CashReceipt.Assign(import.Group.Item.CashReceipt);
        export.Group.Update.Status.Code = import.Group.Item.Status.Code;
        export.Group.Update.DistToOblig.TotalCurrency =
          import.Group.Item.DistToOblig.TotalCurrency;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        MoveCashReceiptType(import.Group.Item.CashReceiptType,
          export.Group.Update.CashReceiptType);
        MoveCashReceiptSourceType(import.Group.Item.CashReceiptSourceType,
          export.Group.Update.CashReceiptSourceType);
        MoveCashReceiptEvent(import.Group.Item.CashReceiptEvent,
          export.Group.Update.CashReceiptEvent);
        export.Group.Update.CashReceiptDetail.Assign(
          import.Group.Item.CashReceiptDetail);
      }
    }
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of CashReceiptEvent.
      /// </summary>
      [JsonPropertyName("cashReceiptEvent")]
      public CashReceiptEvent CashReceiptEvent
      {
        get => cashReceiptEvent ??= new();
        set => cashReceiptEvent = value;
      }

      /// <summary>
      /// A value of CashReceiptType.
      /// </summary>
      [JsonPropertyName("cashReceiptType")]
      public CashReceiptType CashReceiptType
      {
        get => cashReceiptType ??= new();
        set => cashReceiptType = value;
      }

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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
      }

      /// <summary>
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DistToOblig.
      /// </summary>
      [JsonPropertyName("distToOblig")]
      public Common DistToOblig
      {
        get => distToOblig ??= new();
        set => distToOblig = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CashReceiptDetailStatus status;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceiptType cashReceiptType;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private Common distToOblig;
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

    private Array<GroupGroup> group;
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
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of CashReceiptEvent.
      /// </summary>
      [JsonPropertyName("cashReceiptEvent")]
      public CashReceiptEvent CashReceiptEvent
      {
        get => cashReceiptEvent ??= new();
        set => cashReceiptEvent = value;
      }

      /// <summary>
      /// A value of CashReceiptType.
      /// </summary>
      [JsonPropertyName("cashReceiptType")]
      public CashReceiptType CashReceiptType
      {
        get => cashReceiptType ??= new();
        set => cashReceiptType = value;
      }

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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
      }

      /// <summary>
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DistToOblig.
      /// </summary>
      [JsonPropertyName("distToOblig")]
      public Common DistToOblig
      {
        get => distToOblig ??= new();
        set => distToOblig = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CashReceiptDetailStatus status;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceiptType cashReceiptType;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private Common distToOblig;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
#endregion
}
