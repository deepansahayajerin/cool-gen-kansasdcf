// Program: SP_EVLS_LIST_EVENTS, ID: 371778651, model: 746.
// Short name: SWEEVLSP
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
/// A program: SP_EVLS_LIST_EVENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpEvlsListEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EVLS_LIST_EVENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEvlsListEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEvlsListEvents.
  /// </summary>
  public SpEvlsListEvents(IContext context, Import import, Export export):
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
    // 09/16/98  Anita Massey               Fixes per the assessment
    //                                      
    // and correction form
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.Hidden.Assign(local.NextTranInfo);

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "EVMN") || Equal(global.Command, "RETEVMN"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Start.Name = import.Start.Name;

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

        export.Group.Update.GrEvent.Assign(import.Group.Item.GrEvent);
        export.Group.Update.GrCommon.SelectChar =
          import.Group.Item.GrCommon.SelectChar;

        if (!IsEmpty(import.Group.Item.GrCommon.SelectChar))
        {
          if (AsChar(import.Group.Item.GrCommon.SelectChar) == 'S')
          {
            ++local.Common.Count;

            if (local.Common.Count > 1)
            {
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field = GetField(export.Group.Item.GrCommon, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
              }
            }
            else
            {
              MoveEvent1(import.Group.Item.GrEvent, export.Selected);
            }
          }
          else
          {
            var field = GetField(export.Group.Item.GrCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            export.Group.Next();

            return;
          }
        }

        export.Group.Next();
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.HiddenFilterEventType.Type1 = import.PassFilterEventType.Type1;
        UseSpCabReadEachEventWStart();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EVMN":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_EVMN";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "RETEVMN":
        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveEvent1(Event1 source, Event1 target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
    target.Type1 = source.Type1;
    target.BusinessObjectCode = source.BusinessObjectCode;
  }

  private static void MoveGroup(SpCabReadEachEventWStart.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.GrCommon.SelectChar = source.GrCommon.SelectChar;
    target.GrEvent.Assign(source.GrEvent);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
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

  private void UseSpCabReadEachEventWStart()
  {
    var useImport = new SpCabReadEachEventWStart.Import();
    var useExport = new SpCabReadEachEventWStart.Export();

    useImport.PassFilterEventType.Type1 = import.PassFilterEventType.Type1;
    useImport.Start.Name = import.Start.Name;

    Call(SpCabReadEachEventWStart.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
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
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrEvent.
      /// </summary>
      [JsonPropertyName("grEvent")]
      public Event1 GrEvent
      {
        get => grEvent ??= new();
        set => grEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common grCommon;
      private Event1 grEvent;
    }

    /// <summary>
    /// A value of PassFilterEventType.
    /// </summary>
    [JsonPropertyName("passFilterEventType")]
    public Event1 PassFilterEventType
    {
      get => passFilterEventType ??= new();
      set => passFilterEventType = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Event1 Start
    {
      get => start ??= new();
      set => start = value;
    }

    private Event1 passFilterEventType;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<GroupGroup> group;
    private Event1 start;
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
      /// A value of GrEvent.
      /// </summary>
      [JsonPropertyName("grEvent")]
      public Event1 GrEvent
      {
        get => grEvent ??= new();
        set => grEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common grCommon;
      private Event1 grEvent;
    }

    /// <summary>
    /// A value of HiddenFilterEventType.
    /// </summary>
    [JsonPropertyName("hiddenFilterEventType")]
    public Event1 HiddenFilterEventType
    {
      get => hiddenFilterEventType ??= new();
      set => hiddenFilterEventType = value;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Event1 Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Event1 Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    private Event1 hiddenFilterEventType;
    private NextTranInfo hidden;
    private Array<GroupGroup> group;
    private Event1 start;
    private Event1 selected;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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

    private NextTranInfo nextTranInfo;
    private Common common;
  }
#endregion
}
