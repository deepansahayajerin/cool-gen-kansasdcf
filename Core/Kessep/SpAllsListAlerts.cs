// Program: SP_ALLS_LIST_ALERTS, ID: 371747317, model: 746.
// Short name: SWEALLSP
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
/// A program: SP_ALLS_LIST_ALERTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAllsListAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ALLS_LIST_ALERTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAllsListAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAllsListAlerts.
  /// </summary>
  public SpAllsListAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Regan Welborn               Initial Development
    // 10/31/96 Alan Samuels                Complete Development
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.Hidden);
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;

            break;
          }

          return;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        export.Hidden.Assign(local.NextTranInfo);

        return;
      case "DISPLAY":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.GrAlert.Assign(import.Group.Item.GrAlert);
          export.Group.Update.GrCommon.SelectChar =
            import.Group.Item.GrCommon.SelectChar;

          if (AsChar(import.Group.Item.GrCommon.SelectChar) == 'S')
          {
            MoveAlert(import.Group.Item.GrAlert, export.Selected);

            var field = GetField(export.Group.Item.GrCommon, "selectChar");

            field.Error = true;

            ++local.Common.Count;
          }

          export.Group.Next();
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_RETURN";

            return;
          case 1:
            ExitState = "ACO_NE0000_RETURN";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "ALMN":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.GrAlert.Assign(import.Group.Item.GrAlert);
          export.Group.Update.GrCommon.SelectChar =
            import.Group.Item.GrCommon.SelectChar;

          if (AsChar(import.Group.Item.GrCommon.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.GrCommon, "selectChar");

            field.Error = true;

            ++local.Common.Count;
            MoveAlert(import.Group.Item.GrAlert, export.Selected);
          }

          export.Group.Next();
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_XFR_TO_ALMN";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "RETALMN":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.GrAlert.Assign(import.Group.Item.GrAlert);
          export.Group.Update.GrCommon.SelectChar =
            import.Group.Item.GrCommon.SelectChar;

          if (AsChar(import.Group.Item.GrCommon.SelectChar) == 'S')
          {
            export.Group.Update.GrCommon.SelectChar = "";
            MoveAlert(import.Group.Item.GrAlert, export.Selected);

            var field = GetField(export.Group.Item.GrCommon, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          export.Group.Next();
        }

        return;
      case "XXFMMENU":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    export.Start.Name = import.Start.Name;

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
    local.Common.Count = 0;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.GrAlert.Assign(import.Group.Item.GrAlert);
        export.Group.Update.GrCommon.SelectChar =
          import.Group.Item.GrCommon.SelectChar;

        if (AsChar(import.Group.Item.GrCommon.SelectChar) == 'S')
        {
          MoveAlert(import.Group.Item.GrAlert, export.Selected);

          var field = GetField(export.Group.Item.GrCommon, "selectChar");

          field.Error = true;

          ++local.Common.Count;
        }

        export.Group.Next();
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadAlert())
      {
        export.Group.Update.GrAlert.Assign(entities.Alert);
        export.Group.Next();
      }

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
  }

  private static void MoveAlert(Alert source, Alert target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private IEnumerable<bool> ReadAlert()
  {
    return ReadEach("ReadAlert",
      (db, command) =>
      {
        db.SetString(command, "name", import.Start.Name);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Populated = true;

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
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrAlert.
      /// </summary>
      [JsonPropertyName("grAlert")]
      public Alert GrAlert
      {
        get => grAlert ??= new();
        set => grAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common grCommon;
      private Alert grAlert;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Alert Start
    {
      get => start ??= new();
      set => start = value;
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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private Alert start;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrAlert.
      /// </summary>
      [JsonPropertyName("grAlert")]
      public Alert GrAlert
      {
        get => grAlert ??= new();
        set => grAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common grCommon;
      private Alert grAlert;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Alert Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Alert Start
    {
      get => start ??= new();
      set => start = value;
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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Standard standard;
    private Alert selected;
    private Alert start;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private Alert alert;
  }
#endregion
}
