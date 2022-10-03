// Program: FN_B700_MAINTAIN_LINE_2_TOTALS, ID: 373315986, model: 746.
// Short name: SWE02984
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_MAINTAIN_LINE_2_TOTALS.
/// </summary>
[Serializable]
public partial class FnB700MaintainLine2Totals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_MAINTAIN_LINE_2_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700MaintainLine2Totals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700MaintainLine2Totals.
  /// </summary>
  public FnB700MaintainLine2Totals(IContext context, Import import,
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
    // ****************************************************************************
    // **                 M A I N T E N A N C E   L O G
    // ****************************************************************************
    // ** Date		WR/PR	Developer	Description
    // ****************************************************************************
    // ** 12/05/2003	040134	E.Shirk		Federally mandated OCSE34 report changes.
    // ** 12/03/2007	CQ295	GVandy		Federally mandated changes to OCSE34 report.
    // ** 10/14/12  		GVandy		Emergency fix to expand foreign group view size
    // ***************************************************************************
    export.Ocse157Verification.LineNumber = "";

    // 12/3/2007 GVandy CQ295...
    // For C and I type collections, determine if the payment is for an outgoing
    // Foreign Interstate case.
    // I.E. Check if the cash receipt detail court order number is contained in 
    // the repeating group
    // of foreign interstate standard numbers.
    if (import.CollectionType.SequentialIdentifier == 1 || import
      .CollectionType.SequentialIdentifier == 6)
    {
      if (Equal(import.CashReceiptSourceType.Code, 4, 5, "STATE") && !
        Equal(import.CashReceiptSourceType.Code, 1, 8, "KS STATE"))
      {
        // -- Continue
      }
      else
      {
        local.ForeignReceipt.Flag = "N";

        if (IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
        {
          goto Test;
        }

        for(import.OutgoingForeign.Index = 0; import.OutgoingForeign.Index < import
          .OutgoingForeign.Count; ++import.OutgoingForeign.Index)
        {
          if (!import.OutgoingForeign.CheckSize())
          {
            break;
          }

          if (Equal(import.CashReceiptDetail.CourtOrderNumber,
            import.OutgoingForeign.Item.GimportOutgoingForeign.StandardNumber))
          {
            local.ForeignReceipt.Flag = "Y";

            break;
          }

          if (Lt(import.CashReceiptDetail.CourtOrderNumber,
            import.OutgoingForeign.Item.GimportOutgoingForeign.StandardNumber))
          {
            // --  The group is sorted ascending by standard number.  So if the 
            // group standard number is bigger
            // than the cash receipt detail court order number it means that the
            // cash receipt detail court
            // order number was not found in the group.
            break;
          }
        }

        import.OutgoingForeign.CheckIndex();
      }
    }

Test:

    if (Equal(import.CashReceiptSourceType.Code, 4, 5, "STATE") && !
      Equal(import.CashReceiptSourceType.Code, 1, 8, "KS STATE"))
    {
      import.Group.Index = 7;
      import.Group.CheckSize();

      export.Ocse157Verification.LineNumber = "2F";
    }
    else if (import.CollectionType.SequentialIdentifier == 3)
    {
      import.Group.Index = 2;
      import.Group.CheckSize();

      export.Ocse157Verification.LineNumber = "2A";
    }
    else if (import.CollectionType.SequentialIdentifier == 4)
    {
      import.Group.Index = 3;
      import.Group.CheckSize();

      export.Ocse157Verification.LineNumber = "2B";
    }
    else if (import.CollectionType.SequentialIdentifier == 5)
    {
      import.Group.Index = 4;
      import.Group.CheckSize();

      export.Ocse157Verification.LineNumber = "2C";
    }
    else if (import.CollectionType.SequentialIdentifier == 6)
    {
      if (AsChar(local.ForeignReceipt.Flag) == 'Y')
      {
        import.Group.Index = 63;
        import.Group.CheckSize();

        export.Ocse157Verification.LineNumber = "2G";
      }
      else
      {
        import.Group.Index = 6;
        import.Group.CheckSize();

        export.Ocse157Verification.LineNumber = "2E";
      }
    }
    else if (import.CollectionType.SequentialIdentifier == 0)
    {
      import.Group.Index = 8;
      import.Group.CheckSize();

      export.Ocse157Verification.LineNumber = "2H";
    }
    else if (import.CollectionType.SequentialIdentifier == 15 || import
      .CollectionType.SequentialIdentifier == 1 || import
      .CollectionType.SequentialIdentifier == 10 || import
      .CollectionType.SequentialIdentifier == 19 || import
      .CollectionType.SequentialIdentifier == 23 || import
      .CollectionType.SequentialIdentifier == 25 || import
      .CollectionType.SequentialIdentifier == 26)
    {
      import.Group.Index = 8;
      import.Group.CheckSize();

      export.Ocse157Verification.LineNumber = "2H";

      if (import.CollectionType.SequentialIdentifier == 1)
      {
        if (AsChar(local.ForeignReceipt.Flag) == 'Y')
        {
          import.Group.Index = 63;
          import.Group.CheckSize();

          export.Ocse157Verification.LineNumber = "2G";
        }
      }
    }
    else
    {
      return;
    }

    import.Group.Update.Common.TotalCurrency =
      import.Group.Item.Common.TotalCurrency + import.Common.TotalCurrency;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>A OutgoingForeignGroup group.</summary>
    [Serializable]
    public class OutgoingForeignGroup
    {
      /// <summary>
      /// A value of GimportOutgoingForeign.
      /// </summary>
      [JsonPropertyName("gimportOutgoingForeign")]
      public LegalAction GimportOutgoingForeign
      {
        get => gimportOutgoingForeign ??= new();
        set => gimportOutgoingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction gimportOutgoingForeign;
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

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// Gets a value of OutgoingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<OutgoingForeignGroup> OutgoingForeign =>
      outgoingForeign ??= new(OutgoingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OutgoingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("outgoingForeign")]
    [Computed]
    public IList<OutgoingForeignGroup> OutgoingForeign_Json
    {
      get => outgoingForeign;
      set => OutgoingForeign.Assign(value);
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

    private Common common;
    private Array<GroupGroup> group;
    private CollectionType collectionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private Array<OutgoingForeignGroup> outgoingForeign;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private Ocse157Verification ocse157Verification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForeignReceipt.
    /// </summary>
    [JsonPropertyName("foreignReceipt")]
    public Common ForeignReceipt
    {
      get => foreignReceipt ??= new();
      set => foreignReceipt = value;
    }

    private Common foreignReceipt;
  }
#endregion
}
