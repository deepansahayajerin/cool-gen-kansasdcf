// Program: SP_ALNA_ALERT_SEARCH, ID: 1902639829, model: 746.
// Short name: SWEALNAP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_ALNA_ALERT_SEARCH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAlnaAlertSearch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ALNA_ALERT_SEARCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAlnaAlertSearch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAlnaAlertSearch.
  /// </summary>
  public SpAlnaAlertSearch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // ø Date		Developer	Request #      Description                           ø
    // øææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææø
    // ø 11/09/17      JHarden        CQ56117         New Program
    // ø
    // øþæææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();
    }

    MoveStandard(import.Standard, export.Standard);
    export.Minus.OneChar = import.Minus.OneChar;
    export.Plus.OneChar = import.Plus.OneChar;
    export.Filter.Message = import.Filter.Message;
    export.HiddenFilter.Message = import.HiddenFilter.Message;

    for(import.PageKeys.Index = 0; import.PageKeys.Index < import
      .PageKeys.Count; ++import.PageKeys.Index)
    {
      if (!import.PageKeys.CheckSize())
      {
        break;
      }

      export.PageKeys.Index = import.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.PageKey.Message =
        import.PageKeys.Item.PageKey.Message;
    }

    import.PageKeys.CheckIndex();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.OfficeServiceProviderAlert.Message =
        import.Group.Item.OfficeServiceProviderAlert.Message;
    }

    import.Group.CheckIndex();

    // **** Validate all Commands
    local.Search.Message = TrimEnd(import.Filter.Message) + "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"
      ;

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Group.Index = -1;
        export.Group.Count = 0;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        export.Standard.PageNumber = 0;
        export.Minus.OneChar = "";
        export.Plus.OneChar = "";
        export.PageKeys.Update.PageKey.Message = "";

        foreach(var item in ReadOfficeServiceProviderAlert2())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.OfficeServiceProviderAlert.Message =
            local.OfficeServiceProviderAlert.Message;

          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            ++export.PageKeys.Index;
            export.PageKeys.CheckSize();

            export.PageKeys.Update.PageKey.Message =
              export.Group.Item.OfficeServiceProviderAlert.Message;
            ++export.Standard.PageNumber;
            export.Plus.OneChar = "+";

            break;
          }
        }

        export.HiddenFilter.Message = export.Filter.Message;

        break;
      case "RETURN":
        local.Counter.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            ++local.Counter.Count;
            export.Selected.Message =
              export.Group.Item.OfficeServiceProviderAlert.Message;
          }
        }

        export.Group.CheckIndex();

        if (local.Counter.Count > 1)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          export.Group.CheckIndex();
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else if (local.Counter.Count <= 1)
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "NEXT":
        if (!Equal(export.Filter.Message, export.HiddenFilter.Message))
        {
          var field = GetField(export.Filter, "message");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          return;
        }

        if (IsEmpty(import.Plus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        export.Plus.OneChar = "";
        export.Group.Index = -1;
        export.Group.Count = 0;

        export.PageKeys.Index = export.Standard.PageNumber;
        export.PageKeys.CheckSize();

        foreach(var item in ReadOfficeServiceProviderAlert1())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.OfficeServiceProviderAlert.Message =
            local.OfficeServiceProviderAlert.Message;
          export.Minus.OneChar = "-";

          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            ++export.PageKeys.Index;
            export.PageKeys.CheckSize();

            export.PageKeys.Update.PageKey.Message =
              export.Group.Item.OfficeServiceProviderAlert.Message;
            export.Plus.OneChar = "+";

            break;
          }
        }

        ++export.Standard.PageNumber;

        break;
      case "PREV":
        if (!Equal(export.Filter.Message, export.HiddenFilter.Message))
        {
          var field = GetField(export.Filter, "message");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          return;
        }

        if (IsEmpty(import.Minus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        export.Group.Index = -1;
        export.Group.Count = 0;

        export.PageKeys.Index = export.Standard.PageNumber - 2;
        export.PageKeys.CheckSize();

        foreach(var item in ReadOfficeServiceProviderAlert1())
        {
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.OfficeServiceProviderAlert.Message =
            local.OfficeServiceProviderAlert.Message;

          if (export.Group.IsFull)
          {
            ++export.PageKeys.Index;
            export.PageKeys.CheckSize();

            export.PageKeys.Update.PageKey.Message =
              export.Group.Item.OfficeServiceProviderAlert.Message;
            export.Plus.OneChar = "+";
            --export.Standard.PageNumber;

            break;
          }
        }

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.OneChar = "-";
        }
        else
        {
          export.Minus.OneChar = "";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlert1()
  {
    local.OfficeServiceProviderAlert.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlert1",
      (db, command) =>
      {
        db.SetString(command, "message1", local.Search.Message);
        db.SetString(command, "message2", export.PageKeys.Item.PageKey.Message);
      },
      (db, reader) =>
      {
        local.OfficeServiceProviderAlert.Message = db.GetString(reader, 0);
        local.OfficeServiceProviderAlert.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlert2()
  {
    local.OfficeServiceProviderAlert.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlert2",
      (db, command) =>
      {
        db.SetString(command, "message", local.Search.Message);
      },
      (db, reader) =>
      {
        local.OfficeServiceProviderAlert.Message = db.GetString(reader, 0);
        local.OfficeServiceProviderAlert.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
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
      /// A value of OfficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("officeServiceProviderAlert")]
      public OfficeServiceProviderAlert OfficeServiceProviderAlert
      {
        get => officeServiceProviderAlert ??= new();
        set => officeServiceProviderAlert = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private OfficeServiceProviderAlert officeServiceProviderAlert;
      private Common common;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public OfficeServiceProviderAlert PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OfficeServiceProviderAlert pageKey;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public OfficeServiceProviderAlert Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of HiddenFilter.
    /// </summary>
    [JsonPropertyName("hiddenFilter")]
    public OfficeServiceProviderAlert HiddenFilter
    {
      get => hiddenFilter ??= new();
      set => hiddenFilter = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    private OfficeServiceProviderAlert filter;
    private OfficeServiceProviderAlert hiddenFilter;
    private Array<GroupGroup> group;
    private Array<PageKeysGroup> pageKeys;
    private Standard standard;
    private Standard minus;
    private Standard plus;
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
      /// A value of OfficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("officeServiceProviderAlert")]
      public OfficeServiceProviderAlert OfficeServiceProviderAlert
      {
        get => officeServiceProviderAlert ??= new();
        set => officeServiceProviderAlert = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private OfficeServiceProviderAlert officeServiceProviderAlert;
      private Common common;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public OfficeServiceProviderAlert PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OfficeServiceProviderAlert pageKey;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public OfficeServiceProviderAlert Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of HiddenFilter.
    /// </summary>
    [JsonPropertyName("hiddenFilter")]
    public OfficeServiceProviderAlert HiddenFilter
    {
      get => hiddenFilter ??= new();
      set => hiddenFilter = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public OfficeServiceProviderAlert Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    private OfficeServiceProviderAlert filter;
    private OfficeServiceProviderAlert hiddenFilter;
    private OfficeServiceProviderAlert selected;
    private Array<GroupGroup> group;
    private Array<PageKeysGroup> pageKeys;
    private Standard standard;
    private Standard minus;
    private Standard plus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public OfficeServiceProviderAlert Search
    {
      get => search ??= new();
      set => search = value;
    }

    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Common counter;
    private OfficeServiceProviderAlert search;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public OfficeServiceProviderAlert Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private OfficeServiceProviderAlert existing;
  }
#endregion
}
