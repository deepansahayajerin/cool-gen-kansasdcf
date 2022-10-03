// Program: OE_CSWL_CHILD_SUP_WORKSHEET_LIST, ID: 371911684, model: 746.
// Short name: SWECSWLP
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
/// <para>
/// A program: OE_CSWL_CHILD_SUP_WORKSHEET_LIST.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCswlChildSupWorksheetList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSWL_CHILD_SUP_WORKSHEET_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCswlChildSupWorksheetList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCswlChildSupWorksheetList.
  /// </summary>
  public OeCswlChildSupWorksheetList(IContext context, Import import,
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
    // ---------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    	Developer       Description
    // 02/27/95 	Sid Chowdhary   Initial Development
    // 02/28/96 	Sid Chowdhary	Retrofits & Testing
    // 11/14/96	R. Marchman	Add new security and next tran.
    // 09/05/02        KDoshi          Fix Screen Help.
    // 03/05/08        M. Fan          CQ3073 (PR328961) Added '
    // created_timestamp' for the
    //                                 
    // case person support worksheet
    // entity view in the group_import
    //                                 
    // and group_export views. Also
    // added the new field to the
    // screen.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      export.SearchCase.Number = import.SearchCase.Number;
      export.SearchCsePerson.Number = import.SearchCsePerson.Number;
      export.Name.FormattedName = import.Name.FormattedName;

      return;
    }

    // *********************************************
    // Populate the export views prior to any logic
    // MOVE import_search info_view to export_search
    //      info_view
    // MOVE import_search info_view to local_search
    //      info_view
    // MOVE import_next   info_view to export_next
    //      info_view
    // *********************************************
    export.SearchCase.Number = import.SearchCase.Number;
    export.SearchCsePerson.Number = import.SearchCsePerson.Number;
    export.Name.FormattedName = import.Name.FormattedName;
    local.SearchCase.Number = import.SearchCase.Number;
    local.SearchCsePerson.Number = import.SearchCsePerson.Number;
    export.WorkLink.Flag = import.WorkLink.Flag;
    export.FromGldv.Text1 = import.FromGldv.Text1;

    // *************************************************
    // Populate the export group view prior to any logic
    // *************************************************
    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        // ************************************************
        // MOVE group_import_detail to group_export_detail.
        // ************************************************
        export.Export1.Update.DetailScrollingAttributes.Selection =
          import.Import1.Item.DetailScrollingAttributes.Selection;
        export.Export1.Update.DetailCase.Number =
          import.Import1.Item.DetailCase.Number;
        export.Export1.Update.DetailChildSupportWorksheet.Assign(
          import.Import1.Item.DetailChildSupportWorksheet);
        export.Export1.Update.DetailCsePersonSupportWorksheet.Assign(
          import.Import1.Item.DetailCsePersonSupportWorksheet);
        MoveLegalAction(import.Import1.Item.DetailLegalAction,
          export.Export1.Update.DetailLegalAction);
        export.Export1.Update.Date.EffectiveDate =
          import.Import1.Item.Date.EffectiveDate;

        if (!IsEmpty(export.Export1.Item.DetailScrollingAttributes.Selection) &&
          AsChar(export.Export1.Item.DetailScrollingAttributes.Selection) != 'S'
          )
        {
          var field =
            GetField(export.Export1.Item.DetailScrollingAttributes, "selection");
            

          field.Error = true;

          ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";
        }

        export.Export1.Next();
      }
    }

    if (!IsEmpty(export.SearchCase.Number))
    {
      local.TextWorkArea.Text10 = export.SearchCase.Number;
      UseEabPadLeftWithZeros();
      export.SearchCase.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.SearchCsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.SearchCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.SearchCsePerson.Number = local.TextWorkArea.Text10;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      export.Hidden.CaseNumber = import.SearchCase.Number;
      export.Hidden.CsePersonNumber = import.SearchCsePerson.Number;
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      export.SearchCase.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.SearchCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // *********************************************
    // Loop through the export group and check for
    // any line items that have been selected.
    // *********************************************
    if (!export.Export1.IsEmpty)
    {
      if (!Equal(global.Command, "RETURN"))
      {
        goto Test;
      }

      if (AsChar(import.WorkMultipleSelect.Flag) == 'Y')
      {
        export.Select.Index = 0;
        export.Select.CheckSize();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailScrollingAttributes.Selection))
          {
            if (AsChar(export.Export1.Item.DetailScrollingAttributes.Selection) ==
              'S')
            {
              // *********************************************
              // When checking for a selection, if COMMAND
              // does not contain a value of ENTER, then the
              // selection process should not continue.
              // *********************************************
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Select.Update.Selected.Assign(
                  export.Export1.Item.DetailChildSupportWorksheet);

                ++export.Select.Index;
                export.Select.CheckSize();

                ExitState = "ACO_NE0000_RETURN";
              }
            }
            else
            {
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field =
                GetField(export.Export1.Item.DetailScrollingAttributes,
                "selection");

              field.Error = true;
            }
          }
        }
      }
      else
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailScrollingAttributes.Selection))
          {
            if (AsChar(export.Export1.Item.DetailScrollingAttributes.Selection) ==
              'S')
            {
              if (!IsEmpty(export.Export1.Item.DetailCase.Number))
              {
                export.SelectedCase.Number =
                  export.Export1.Item.DetailCase.Number;
                export.SelectedChildSupportWorksheet.Assign(
                  export.Export1.Item.DetailChildSupportWorksheet);
                export.SelectedCsePersonSupportWorksheet.Assign(
                  export.Export1.Item.DetailCsePersonSupportWorksheet);
                export.SelectedCsePerson.Number = export.SearchCsePerson.Number;
                MoveLegalAction(export.Export1.Item.DetailLegalAction,
                  export.SelectedLegalAction);
                export.SelectedDate.EffectiveDate =
                  export.Export1.Item.Date.EffectiveDate;
                ExitState = "ACO_NE0000_RETURN";
              }

              ++local.Select1.Count;

              if (local.Select1.Count > 1)
              {
                // ---------------------------------------------
                // 	Make selection character ERROR.
                // ---------------------------------------------
                var field =
                  GetField(export.Export1.Item.DetailScrollingAttributes,
                  "selection");

                field.Error = true;

                ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
              }
            }
            else
            {
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field =
                GetField(export.Export1.Item.DetailScrollingAttributes,
                "selection");

              field.Error = true;
            }
          }
        }

        if (local.Select1.Count == 0)
        {
          ExitState = "ACO_NE0000_RETURN";
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.Name.Number))
      {
        export.SearchCsePerson.Number = import.Name.Number;
      }

      var field = GetField(export.SearchCase, "number");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "WORK"))
    {
      local.Select1.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailScrollingAttributes.Selection) == 'S'
          )
        {
          if (!IsEmpty(export.Export1.Item.DetailCase.Number))
          {
            export.SelectedCase.Number = export.Export1.Item.DetailCase.Number;
            export.SelectedChildSupportWorksheet.Assign(
              export.Export1.Item.DetailChildSupportWorksheet);
            export.SelectedCsePersonSupportWorksheet.Assign(
              export.Export1.Item.DetailCsePersonSupportWorksheet);
            export.SelectedCsePerson.Number = export.SearchCsePerson.Number;
            MoveLegalAction(export.Export1.Item.DetailLegalAction,
              export.SelectedLegalAction);
            export.SelectedDate.EffectiveDate =
              export.Export1.Item.Date.EffectiveDate;

            if (AsChar(import.FromGldv.Text1) == 'Y')
            {
              ExitState = "ECO_LNK_TO_WORKSHEET";
            }
            else
            {
              ExitState = "ECO_LNK_TO_CS_WORKSHEET_1";
            }
          }

          ++local.Select1.Count;

          if (local.Select1.Count > 1)
          {
            // ---------------------------------------------
            // 	Make selection character ERROR.
            // ---------------------------------------------
            var field =
              GetField(export.Export1.Item.DetailScrollingAttributes,
              "selection");

            field.Error = true;

            ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
          }
        }
      }

      if (local.Select1.Count < 1)
      {
        ExitState = "OE0000_MUST_SELECT_WORKSHEET";
      }

      return;
    }

    // **** begin group C ****
    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        switch(AsChar(import.Prompt.SelectChar))
        {
          case 'S':
            if (IsEmpty(export.SearchCase.Number))
            {
              ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";
            }
            else
            {
            }

            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          case ' ':
            var field1 = GetField(export.Prompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          default:
            var field2 = GetField(export.Prompt, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ZD_ACO_NE0000_INVALID_BACKWARD_2";

        break;
      case "DISPLAY":
        UseOeCswlListChildSupWorksheet();

        if (IsExitState("CSE_PERSON_NO_REQUIRED"))
        {
          var field = GetField(export.SearchCsePerson, "number");

          field.Error = true;
        }
        else if (IsExitState("CASE_NF"))
        {
          var field = GetField(export.SearchCase, "number");

          field.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.SearchCsePerson, "number");

          field.Error = true;
        }
        else if (IsExitState("OE0000_CASE_MEMBER_NE"))
        {
          var field1 = GetField(export.SearchCase, "number");

          field1.Error = true;

          var field2 = GetField(export.SearchCsePerson, "number");

          field2.Error = true;
        }
        else
        {
          if (AsChar(local.Common.Flag) == 'Y')
          {
            var field = GetField(export.SearchCase, "number");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveExport1(OeCswlListChildSupWorksheet.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Date.EffectiveDate = source.Date.EffectiveDate;
    target.DetailScrollingAttributes.Selection =
      source.DetailScrollingAttributes.Selection;
    target.DetailCase.Number = source.DetailCase.Number;
    MoveLegalAction(source.DetailLegalAction, target.DetailLegalAction);
    target.DetailCsePersonSupportWorksheet.Assign(
      source.DetailCsePersonSupportWorksheet);
    target.DetailChildSupportWorksheet.
      Assign(source.DetailChildSupportWorksheet);
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCswlListChildSupWorksheet()
  {
    var useImport = new OeCswlListChildSupWorksheet.Import();
    var useExport = new OeCswlListChildSupWorksheet.Export();

    useImport.CsePerson.Number = export.SearchCsePerson.Number;
    useImport.Case1.Number = export.SearchCase.Number;

    Call(OeCswlListChildSupWorksheet.Execute, useImport, useExport);

    export.Name.FormattedName = useExport.Name.FormattedName;
    export.SearchCsePerson.Number = useExport.CsePerson.Number;
    export.SearchCase.Number = useExport.Case1.Number;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = import.SearchCase.Number;
    useImport.CsePerson.Number = import.SearchCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
      /// A value of Date.
      /// </summary>
      [JsonPropertyName("date")]
      public Code Date
      {
        get => date ??= new();
        set => date = value;
      }

      /// <summary>
      /// A value of DetailChildSupportWorksheet.
      /// </summary>
      [JsonPropertyName("detailChildSupportWorksheet")]
      public ChildSupportWorksheet DetailChildSupportWorksheet
      {
        get => detailChildSupportWorksheet ??= new();
        set => detailChildSupportWorksheet = value;
      }

      /// <summary>
      /// A value of DetailCsePersonSupportWorksheet.
      /// </summary>
      [JsonPropertyName("detailCsePersonSupportWorksheet")]
      public CsePersonSupportWorksheet DetailCsePersonSupportWorksheet
      {
        get => detailCsePersonSupportWorksheet ??= new();
        set => detailCsePersonSupportWorksheet = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailScrollingAttributes.
      /// </summary>
      [JsonPropertyName("detailScrollingAttributes")]
      public ScrollingAttributes DetailScrollingAttributes
      {
        get => detailScrollingAttributes ??= new();
        set => detailScrollingAttributes = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Code date;
      private ChildSupportWorksheet detailChildSupportWorksheet;
      private CsePersonSupportWorksheet detailCsePersonSupportWorksheet;
      private LegalAction detailLegalAction;
      private Case1 detailCase;
      private ScrollingAttributes detailScrollingAttributes;
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

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of WorkMultipleSelect.
    /// </summary>
    [JsonPropertyName("workMultipleSelect")]
    public Common WorkMultipleSelect
    {
      get => workMultipleSelect ??= new();
      set => workMultipleSelect = value;
    }

    /// <summary>
    /// A value of WorkLink.
    /// </summary>
    [JsonPropertyName("workLink")]
    public Common WorkLink
    {
      get => workLink ??= new();
      set => workLink = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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

    /// <summary>
    /// A value of FromGldv.
    /// </summary>
    [JsonPropertyName("fromGldv")]
    public WorkArea FromGldv
    {
      get => fromGldv ??= new();
      set => fromGldv = value;
    }

    private Common prompt;
    private CsePersonsWorkSet name;
    private Common workMultipleSelect;
    private Common workLink;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea fromGldv;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SelectGroup group.</summary>
    [Serializable]
    public class SelectGroup
    {
      /// <summary>
      /// A value of Selected.
      /// </summary>
      [JsonPropertyName("selected")]
      public ChildSupportWorksheet Selected
      {
        get => selected ??= new();
        set => selected = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private ChildSupportWorksheet selected;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Date.
      /// </summary>
      [JsonPropertyName("date")]
      public Code Date
      {
        get => date ??= new();
        set => date = value;
      }

      /// <summary>
      /// A value of DetailScrollingAttributes.
      /// </summary>
      [JsonPropertyName("detailScrollingAttributes")]
      public ScrollingAttributes DetailScrollingAttributes
      {
        get => detailScrollingAttributes ??= new();
        set => detailScrollingAttributes = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailCsePersonSupportWorksheet.
      /// </summary>
      [JsonPropertyName("detailCsePersonSupportWorksheet")]
      public CsePersonSupportWorksheet DetailCsePersonSupportWorksheet
      {
        get => detailCsePersonSupportWorksheet ??= new();
        set => detailCsePersonSupportWorksheet = value;
      }

      /// <summary>
      /// A value of DetailChildSupportWorksheet.
      /// </summary>
      [JsonPropertyName("detailChildSupportWorksheet")]
      public ChildSupportWorksheet DetailChildSupportWorksheet
      {
        get => detailChildSupportWorksheet ??= new();
        set => detailChildSupportWorksheet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Code date;
      private ScrollingAttributes detailScrollingAttributes;
      private Case1 detailCase;
      private LegalAction detailLegalAction;
      private CsePersonSupportWorksheet detailCsePersonSupportWorksheet;
      private ChildSupportWorksheet detailChildSupportWorksheet;
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

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// Gets a value of Select.
    /// </summary>
    [JsonIgnore]
    public Array<SelectGroup> Select => select ??= new(SelectGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Select for json serialization.
    /// </summary>
    [JsonPropertyName("select")]
    [Computed]
    public IList<SelectGroup> Select_Json
    {
      get => select;
      set => Select.Assign(value);
    }

    /// <summary>
    /// A value of SelectedDate.
    /// </summary>
    [JsonPropertyName("selectedDate")]
    public Code SelectedDate
    {
      get => selectedDate ??= new();
      set => selectedDate = value;
    }

    /// <summary>
    /// A value of WorkLink.
    /// </summary>
    [JsonPropertyName("workLink")]
    public Common WorkLink
    {
      get => workLink ??= new();
      set => workLink = value;
    }

    /// <summary>
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("selectedChildSupportWorksheet")]
    public ChildSupportWorksheet SelectedChildSupportWorksheet
    {
      get => selectedChildSupportWorksheet ??= new();
      set => selectedChildSupportWorksheet = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet SelectedCsePersonSupportWorksheet
    {
      get => selectedCsePersonSupportWorksheet ??= new();
      set => selectedCsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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

    /// <summary>
    /// A value of FromGldv.
    /// </summary>
    [JsonPropertyName("fromGldv")]
    public WorkArea FromGldv
    {
      get => fromGldv ??= new();
      set => fromGldv = value;
    }

    private Common prompt;
    private CsePersonsWorkSet name;
    private Array<SelectGroup> select;
    private Code selectedDate;
    private Common workLink;
    private LegalAction selectedLegalAction;
    private CsePerson selectedCsePerson;
    private ChildSupportWorksheet selectedChildSupportWorksheet;
    private CsePersonSupportWorksheet selectedCsePersonSupportWorksheet;
    private Case1 selectedCase;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea fromGldv;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Select1.
    /// </summary>
    [JsonPropertyName("select1")]
    public Common Select1
    {
      get => select1 ??= new();
      set => select1 = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    private Common common;
    private TextWorkArea textWorkArea;
    private Common select1;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
  }
#endregion
}
