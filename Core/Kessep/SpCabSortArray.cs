// Program: SP_CAB_SORT_ARRAY, ID: 372908529, model: 746.
// Short name: SWE02863
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_SORT_ARRAY.
/// </summary>
[Serializable]
public partial class SpCabSortArray: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_SORT_ARRAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabSortArray(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabSortArray.
  /// </summary>
  public SpCabSortArray(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    // ---->              M A I N T E N A N C E   L O G                       <-
    // ---
    // ----------------------------------------------------------------------------
    // **   Date   Developer    PR#     Description
    // **   ----   ---------    ---     -----------
    // **
    // ** 10/20/99  SWSRCHF  H00077181  This is a NEW action block created for 
    // this
    // **
    // 
    // problem report.
    // **
    // 
    // The IMPORT Group view is sorted
    // into
    // **
    // 
    // Last name/First name order and
    // returned via
    // **
    // 
    // the EXPORT Group view to the
    // PrAD
    // ----------------------------------------------------------------------------
    import.Group.Index = -1;
    local.Group.Index = -1;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      // ***
      // *** Move the IMPORT group to a LOCAL group for processing
      // ***
      local.Group.Index = import.Group.Index;
      local.Group.CheckSize();

      local.Group.Update.ServiceProvider.Assign(
        import.Group.Item.ServiceProvider);
      MoveOfficeServiceProvider(import.Group.Item.OfficeServiceProvider,
        local.Group.Update.OfficeServiceProvider);
      local.Group.Update.OfficeCaseloadAssignment.Assign(
        import.Group.Item.OfficeCaseloadAssignment);
      local.Group.Update.Program.Code = import.Group.Item.Program.Code;
      local.Group.Update.Tribunal.Identifier =
        import.Group.Item.Tribunal.Identifier;
      MoveTextWorkArea(import.Group.Item.BegAlpha, local.Group.Update.BegAlpha);
      MoveTextWorkArea(import.Group.Item.EndAlpha, local.Group.Update.EndAlpha);
      local.Group.Update.Ovrd.Count = import.Group.Item.Ovrd.Count;
      local.Group.Update.Case1.Count = import.Group.Item.Case1.Count;
      local.Group.Update.CaseCnt.Count = import.Group.Item.CaseCnt.Count;
    }

    import.Group.CheckIndex();

    // ***
    // *** Initialise the LOOP flag and the EXPORT group subscript
    // ***
    local.Loop.Flag = "Y";
    export.Group.Index = -1;
    local.Group.Index = -1;

    // ***
    // *** Sort the LOCAL group into the EXPORT group
    // ***
    while(AsChar(local.Loop.Flag) == 'Y')
    {
      local.SavedLow.LastName = "ZZZZZZZZZZZZZZZZZ";
      local.SavedLow.FirstName = "ZZZZZZZZZZZZ";

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (IsEmpty(local.Group.Item.ServiceProvider.LastName) && IsEmpty
          (local.Group.Item.ServiceProvider.FirstName))
        {
          // ***
          // *** bypass, when the Last name and the First name are SPACES
          // ***
          continue;
        }

        if (Equal(local.Group.Item.ServiceProvider.LastName, "ZZZZZZZZZZZZZZZZZ")
          && Equal(local.Group.Item.ServiceProvider.FirstName, "ZZZZZZZZZZZZ"))
        {
          // ***
          // *** bypass, when the Last name and the First name are all "Z"'s
          // ***
          continue;
        }

        if (Equal(local.Group.Item.ServiceProvider.LastName,
          local.SavedLow.LastName) && Lt
          (local.Group.Item.ServiceProvider.FirstName, local.SavedLow.FirstName) ||
          Lt
          (local.Group.Item.ServiceProvider.LastName, local.SavedLow.LastName))
        {
          // ***
          // *** this is currently the lowest Last name/First name combination
          // ***
          local.SavedLow.LastName = local.Group.Item.ServiceProvider.LastName;
          local.SavedLow.FirstName = local.Group.Item.ServiceProvider.FirstName;
        }
      }

      local.Group.CheckIndex();

      if (Equal(local.SavedLow.LastName, "ZZZZZZZZZZZZZZZZZ") && Equal
        (local.SavedLow.FirstName, "ZZZZZZZZZZZZ"))
      {
        // ***
        // *** processing completed, terminate the WHILE loop
        // ***
        local.Loop.Flag = "N";

        continue;
      }

      local.Group.Index = -1;

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (IsEmpty(local.Group.Item.ServiceProvider.LastName) && IsEmpty
          (local.Group.Item.ServiceProvider.FirstName))
        {
          // ***
          // *** bypass, when the Last name and the First name are SPACES
          // ***
          continue;
        }

        if (Equal(local.Group.Item.ServiceProvider.LastName, "ZZZZZZZZZZZZZZZZZ")
          && Equal(local.Group.Item.ServiceProvider.FirstName, "ZZZZZZZZZZZZ"))
        {
          // ***
          // *** bypass, when the Last name and the First name are all "Z"'s
          // ***
          continue;
        }

        if (Equal(local.Group.Item.ServiceProvider.LastName,
          local.SavedLow.LastName) && Equal
          (local.Group.Item.ServiceProvider.FirstName, local.SavedLow.FirstName))
          
        {
          // ***
          // *** increment the EXPORT group subscript
          // ***
          ++export.Group.Index;
          export.Group.CheckSize();

          // ***
          // *** Move the LOCAL group entry to a EXPORT group view
          // ***
          export.Group.Update.ServiceProvider.Assign(
            local.Group.Item.ServiceProvider);
          MoveOfficeServiceProvider(local.Group.Item.OfficeServiceProvider,
            export.Group.Update.OfficeServiceProvider);
          export.Group.Update.OfficeCaseloadAssignment.Assign(
            local.Group.Item.OfficeCaseloadAssignment);
          export.Group.Update.Program.Code = local.Group.Item.Program.Code;
          export.Group.Update.Tribunal.Identifier =
            local.Group.Item.Tribunal.Identifier;
          MoveTextWorkArea(local.Group.Item.BegAlpha,
            export.Group.Update.BegAlpha);
          MoveTextWorkArea(local.Group.Item.EndAlpha,
            export.Group.Update.EndAlpha);
          export.Group.Update.Ovrd.Count = local.Group.Item.Ovrd.Count;
          export.Group.Update.Case1.Count = local.Group.Item.Case1.Count;
          export.Group.Update.CaseCnt.Count = local.Group.Item.CaseCnt.Count;

          // ***
          // *** set the Last name and the First name to all "Z"'s
          // ***
          local.Group.Update.ServiceProvider.LastName = "ZZZZZZZZZZZZZZZZZ";
          local.Group.Update.ServiceProvider.FirstName = "ZZZZZZZZZZZZ";

          break;
        }
      }

      local.Group.CheckIndex();
    }
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text8 = source.Text8;
    target.Text30 = source.Text30;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of BegAlpha.
      /// </summary>
      [JsonPropertyName("begAlpha")]
      public TextWorkArea BegAlpha
      {
        get => begAlpha ??= new();
        set => begAlpha = value;
      }

      /// <summary>
      /// A value of EndAlpha.
      /// </summary>
      [JsonPropertyName("endAlpha")]
      public TextWorkArea EndAlpha
      {
        get => endAlpha ??= new();
        set => endAlpha = value;
      }

      /// <summary>
      /// A value of Ovrd.
      /// </summary>
      [JsonPropertyName("ovrd")]
      public Common Ovrd
      {
        get => ovrd ??= new();
        set => ovrd = value;
      }

      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Common Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of CaseCnt.
      /// </summary>
      [JsonPropertyName("caseCnt")]
      public Common CaseCnt
      {
        get => caseCnt ??= new();
        set => caseCnt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private ServiceProvider serviceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
      private Program program;
      private Tribunal tribunal;
      private TextWorkArea begAlpha;
      private TextWorkArea endAlpha;
      private Common ovrd;
      private Common case1;
      private Common caseCnt;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of BegAlpha.
      /// </summary>
      [JsonPropertyName("begAlpha")]
      public TextWorkArea BegAlpha
      {
        get => begAlpha ??= new();
        set => begAlpha = value;
      }

      /// <summary>
      /// A value of EndAlpha.
      /// </summary>
      [JsonPropertyName("endAlpha")]
      public TextWorkArea EndAlpha
      {
        get => endAlpha ??= new();
        set => endAlpha = value;
      }

      /// <summary>
      /// A value of Ovrd.
      /// </summary>
      [JsonPropertyName("ovrd")]
      public Common Ovrd
      {
        get => ovrd ??= new();
        set => ovrd = value;
      }

      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Common Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of CaseCnt.
      /// </summary>
      [JsonPropertyName("caseCnt")]
      public Common CaseCnt
      {
        get => caseCnt ??= new();
        set => caseCnt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private ServiceProvider serviceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
      private Program program;
      private Tribunal tribunal;
      private TextWorkArea begAlpha;
      private TextWorkArea endAlpha;
      private Common ovrd;
      private Common case1;
      private Common caseCnt;
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

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of BegAlpha.
      /// </summary>
      [JsonPropertyName("begAlpha")]
      public TextWorkArea BegAlpha
      {
        get => begAlpha ??= new();
        set => begAlpha = value;
      }

      /// <summary>
      /// A value of EndAlpha.
      /// </summary>
      [JsonPropertyName("endAlpha")]
      public TextWorkArea EndAlpha
      {
        get => endAlpha ??= new();
        set => endAlpha = value;
      }

      /// <summary>
      /// A value of Ovrd.
      /// </summary>
      [JsonPropertyName("ovrd")]
      public Common Ovrd
      {
        get => ovrd ??= new();
        set => ovrd = value;
      }

      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Common Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of CaseCnt.
      /// </summary>
      [JsonPropertyName("caseCnt")]
      public Common CaseCnt
      {
        get => caseCnt ??= new();
        set => caseCnt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private ServiceProvider serviceProvider;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
      private Program program;
      private Tribunal tribunal;
      private TextWorkArea begAlpha;
      private TextWorkArea endAlpha;
      private Common ovrd;
      private Common case1;
      private Common caseCnt;
    }

    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of SavedLow.
    /// </summary>
    [JsonPropertyName("savedLow")]
    public ServiceProvider SavedLow
    {
      get => savedLow ??= new();
      set => savedLow = value;
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

    private Common loop;
    private ServiceProvider savedLow;
    private Array<GroupGroup> group;
  }
#endregion
}
