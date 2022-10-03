// Program: LE_CRIT_ADMIN_ACT_CRITERIA, ID: 372615486, model: 746.
// Short name: SWECRITP
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
/// A program: LE_CRIT_ADMIN_ACT_CRITERIA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeCritAdminActCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CRIT_ADMIN_ACT_CRITERIA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCritAdminActCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCritAdminActCriteria.
  /// </summary>
  public LeCritAdminActCriteria(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 05-15-95        S. Benton			Initial development
    // 05-01-96	S. Malik			Retrofit
    // 10/23/98    P. Sharp     Fixed problem report #48096, PF12 not working. 
    // Removed max date action block from Pstep, not used.  Moved setting sel
    // char to spaces inside group view.
    // 11/09/99    R. Jean     PR76898 - Add CREATE to command tested in 
    // security CAB.
    // Remove command ADD.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      // *********************************************
      // Clear out all fields on the screen.
      // *********************************************
      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    MoveAdministrativeAction(import.AdministrativeAction,
      export.AdministrativeAction);
    export.ListAdminActions.PromptField = import.ListAdminActions.PromptField;
    export.Starting.Identifier = import.Starting.Identifier;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!Equal(global.Command, "LIST"))
    {
      export.ListAdminActions.PromptField = "";
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailAdministrativeActionCriteria.Assign(
        import.Import1.Item.DetailAdministrativeActionCriteria);
      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Next();
    }

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    MoveAdministrativeAction(import.HiddenAdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
      import.Hidden.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.DetailHiddenAdministrativeActionCriteria.Assign(
        import.Hidden.Item.DetailHiddenAdministrativeActionCriteria);
      export.Hidden.Next();
    }

    // *********************************************
    //        SECURITY/NEXTTRAN  LOGIC
    // Use the CAB to nexttran to another procedure.
    // *********************************************
    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
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
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);

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
      local.NextTranInfo.Assign(import.HiddenNextTranInfo);
      UseScCabNextTranPut();

      return;
    }

    // *******************************************************************
    // * 11/09/99    R. Jean     PR76898 - Add CREATE to command tested in 
    // security CAB.
    // * Remove command ADD.
    // *******************************************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "CREATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // *********************************************
    // Do not allow scrolling when a selection has
    // been made.
    // *********************************************
    // *********************************************
    // Verify that a display has been performed
    // before the update or delete can take place.
    // *********************************************
    if (Equal(global.Command, "UPDATE") && !
      Equal(import.AdministrativeAction.Type1,
      import.HiddenAdministrativeAction.Type1))
    {
      var field = GetField(export.AdministrativeAction, "type1");

      field.Error = true;

      ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

      return;
    }

    if (Equal(global.Command, "DELETE") && !
      Equal(import.AdministrativeAction.Type1,
      import.HiddenAdministrativeAction.Type1))
    {
      var field = GetField(export.AdministrativeAction, "type1");

      field.Error = true;

      ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

      return;
    }

    // *********************************************
    // Check to see if any criteria has been
    // selected to be added, updated, or deleted.
    // *********************************************
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      local.Common.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Common.Count;
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.Common.Count == 0)
      {
        // *********************************************
        // No record has been selected.  Set the Exit
        // State and Escape.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }
      else
      {
      }
    }

    // *********************************************
    // Perform validations common to both CREATEs
    // and UPDATEs.
    // *********************************************
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(import.AdministrativeAction.Type1))
      {
        var field = GetField(export.AdministrativeAction, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          if (Equal(export.Export1.Item.DetailAdministrativeActionCriteria.
            EffectiveDate, null))
          {
            var field =
              GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
              "effectiveDate");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }

          if (IsEmpty(export.Export1.Item.DetailAdministrativeActionCriteria.
            Description))
          {
            var field =
              GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
              "description");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }

          if (!Equal(export.Export1.Item.DetailAdministrativeActionCriteria.
            EndDate, null))
          {
            if (Lt(export.Export1.Item.DetailAdministrativeActionCriteria.
              EndDate,
              export.Export1.Item.DetailAdministrativeActionCriteria.
                EffectiveDate))
            {
              var field1 =
                GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
                "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
                "endDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_END_LESS_THAN_EFF";

              return;
            }
          }
        }
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // *********************************************
        // DISPLAY Logic is done at the end.
        // *********************************************
        break;
      case "EXIT":
        // ********************************************
        // Allows the user to flow back to the previous
        // screen.
        // ********************************************
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        // *********************************************
        // This command allows the user to link to the
        // Administrative Actions Available selection
        // list and retrieve the appropriate value, not
        // losing any of the data already entered.
        // *********************************************
        if (!IsEmpty(export.ListAdminActions.PromptField) && AsChar
          (export.ListAdminActions.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListAdminActions, "promptField");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.ListAdminActions.PromptField))
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          var field = GetField(export.ListAdminActions, "promptField");

          field.Error = true;

          return;
        }

        ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

        break;
      case "CREATE":
        // *********************************************
        // Insert the USE statement here to call the
        // CREATE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            UseAddAdminActionCriteria();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
            }
            else if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
            {
              var field = GetField(export.AdministrativeAction, "type1");

              field.Error = true;

              return;
            }
            else
            {
              var field1 = GetField(export.AdministrativeAction, "type1");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
                "identifier");

              field2.Error = true;

              return;
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // *********************************************
        // Check to see if any key fields have been
        // changed.
        // *********************************************
        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          return;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // UPDATE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            UseUpdateAdminActionCriteria();
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            var field1 = GetField(export.AdministrativeAction, "type1");

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
              "identifier");

            field2.Error = true;

            return;
          }

          export.Export1.Update.DetailCommon.SelectChar = "";
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "DELETE":
        // *********************************************
        // Insert the USE statement here to call the
        // DELETE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            UseDeleteAdminActionCriteria();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              var field1 = GetField(export.AdministrativeAction, "type1");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailAdministrativeActionCriteria,
                "identifier");

              field2.Error = true;

              return;
            }
          }
        }

        UseCabReadAdminActionCriteria();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "SIGNOFF":
        break;
      case "AACT":
        ExitState = "ECO_XFR_TO_ADMIN_ACTIONS";

        break;
      case "RTLIST":
        if (!IsEmpty(import.AdministrativeAction.Type1))
        {
          global.Command = "DISPLAY";
        }

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // *********************************************
      // Required fields  EDIT LOGIC
      // *********************************************
      if (IsEmpty(export.AdministrativeAction.Type1))
      {
        var field = GetField(export.AdministrativeAction, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      UseCabReadAdminActionCriteria();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.AdministrativeAction, "type1");

        field.Error = true;

        return;
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "NO_ADMIN_ACTION_CRITERIA_ON_FILE";
      }
      else if (export.Export1.IsFull)
      {
        ExitState = "ACO_NI0000_LIST_IS_FULL";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }
    else
    {
    }

    // *********************************************
    // If all processing completed successfully,
    // move all imports to previous exports .
    // *********************************************
    MoveAdministrativeAction(export.AdministrativeAction,
      export.HiddenAdministrativeAction);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.DetailHiddenAdministrativeActionCriteria.Assign(
        export.Export1.Item.DetailAdministrativeActionCriteria);
      export.Hidden.Next();
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveImport1ToExport1(CabReadAdminActionCriteria.Export.
    ImportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.Common.SelectChar;
    target.DetailAdministrativeActionCriteria.Assign(
      source.AdministrativeActionCriteria);
  }

  private void UseAddAdminActionCriteria()
  {
    var useImport = new AddAdminActionCriteria.Import();
    var useExport = new AddAdminActionCriteria.Export();

    useImport.AdministrativeActionCriteria.Assign(
      export.Export1.Item.DetailAdministrativeActionCriteria);
    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    Call(AddAdminActionCriteria.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    export.Export1.Update.DetailAdministrativeActionCriteria.Assign(
      useExport.AdministrativeActionCriteria);
  }

  private void UseCabReadAdminActionCriteria()
  {
    var useImport = new CabReadAdminActionCriteria.Import();
    var useExport = new CabReadAdminActionCriteria.Export();

    useImport.Starting.Identifier = export.Starting.Identifier;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;

    Call(CabReadAdminActionCriteria.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    useExport.Import1.CopyTo(export.Export1, MoveImport1ToExport1);
  }

  private void UseDeleteAdminActionCriteria()
  {
    var useImport = new DeleteAdminActionCriteria.Import();
    var useExport = new DeleteAdminActionCriteria.Export();

    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
    useImport.AdministrativeActionCriteria.Identifier =
      export.Export1.Item.DetailAdministrativeActionCriteria.Identifier;

    Call(DeleteAdminActionCriteria.Execute, useImport, useExport);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
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

  private void UseUpdateAdminActionCriteria()
  {
    var useImport = new UpdateAdminActionCriteria.Import();
    var useExport = new UpdateAdminActionCriteria.Export();

    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
    useImport.AdministrativeActionCriteria.Assign(
      export.Export1.Item.DetailAdministrativeActionCriteria);

    Call(UpdateAdminActionCriteria.Execute, useImport, useExport);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActionCriteria.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActionCriteria")]
      public AdministrativeActionCriteria DetailAdministrativeActionCriteria
      {
        get => detailAdministrativeActionCriteria ??= new();
        set => detailAdministrativeActionCriteria = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Common detailCommon;
      private AdministrativeActionCriteria detailAdministrativeActionCriteria;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of DetailHiddenCommon.
      /// </summary>
      [JsonPropertyName("detailHiddenCommon")]
      public Common DetailHiddenCommon
      {
        get => detailHiddenCommon ??= new();
        set => detailHiddenCommon = value;
      }

      /// <summary>
      /// A value of DetailHiddenAdministrativeActionCriteria.
      /// </summary>
      [JsonPropertyName("detailHiddenAdministrativeActionCriteria")]
      public AdministrativeActionCriteria DetailHiddenAdministrativeActionCriteria
        
      {
        get => detailHiddenAdministrativeActionCriteria ??= new();
        set => detailHiddenAdministrativeActionCriteria = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Common detailHiddenCommon;
      private AdministrativeActionCriteria detailHiddenAdministrativeActionCriteria;
        
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public AdministrativeActionCriteria Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of ListAdminActions.
    /// </summary>
    [JsonPropertyName("listAdminActions")]
    public Standard ListAdminActions
    {
      get => listAdminActions ??= new();
      set => listAdminActions = value;
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    private AdministrativeActionCriteria starting;
    private NextTranInfo hiddenNextTranInfo;
    private Standard listAdminActions;
    private Standard standard;
    private Array<ImportGroup> import1;
    private AdministrativeAction administrativeAction;
    private Array<HiddenGroup> hidden;
    private AdministrativeAction hiddenAdministrativeAction;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActionCriteria.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActionCriteria")]
      public AdministrativeActionCriteria DetailAdministrativeActionCriteria
      {
        get => detailAdministrativeActionCriteria ??= new();
        set => detailAdministrativeActionCriteria = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Common detailCommon;
      private AdministrativeActionCriteria detailAdministrativeActionCriteria;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of DetailHiddenCommon.
      /// </summary>
      [JsonPropertyName("detailHiddenCommon")]
      public Common DetailHiddenCommon
      {
        get => detailHiddenCommon ??= new();
        set => detailHiddenCommon = value;
      }

      /// <summary>
      /// A value of DetailHiddenAdministrativeActionCriteria.
      /// </summary>
      [JsonPropertyName("detailHiddenAdministrativeActionCriteria")]
      public AdministrativeActionCriteria DetailHiddenAdministrativeActionCriteria
        
      {
        get => detailHiddenAdministrativeActionCriteria ??= new();
        set => detailHiddenAdministrativeActionCriteria = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Common detailHiddenCommon;
      private AdministrativeActionCriteria detailHiddenAdministrativeActionCriteria;
        
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public AdministrativeActionCriteria Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ListAdminActions.
    /// </summary>
    [JsonPropertyName("listAdminActions")]
    public Standard ListAdminActions
    {
      get => listAdminActions ??= new();
      set => listAdminActions = value;
    }

    private AdministrativeActionCriteria starting;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Array<ExportGroup> export1;
    private AdministrativeAction administrativeAction;
    private Array<HiddenGroup> hidden;
    private AdministrativeAction hiddenAdministrativeAction;
    private Standard listAdminActions;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private DateWorkArea initialisedToZeros;
    private NextTranInfo nextTranInfo;
    private Common common;
    private Common prompt;
  }
#endregion
}
