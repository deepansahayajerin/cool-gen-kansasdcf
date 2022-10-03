// Program: SP_DOCM_DOCUMENT_MAINT, ID: 372105789, model: 746.
// Short name: SWEDOCMP
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
/// A program: SP_DOCM_DOCUMENT_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDocmDocumentMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOCM_DOCUMENT_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocmDocumentMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocmDocumentMaint.
  /// </summary>
  public SpDocmDocumentMaint(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // Apr 10 95	ASK		Initial Development
    // 03/15/96	a. hackler	retro fits
    // 01/04/97	R. Marchman	Add new security/next tran.
    // 09/08/1998	M. Ramirez	Post assesment fixes
    // -------------------------------------------------------------------
    // mjr
    // ---------------------------------------------------
    // Program flow
    // Move imports to exports
    // Next Tran and Security
    // Validations
    //      TIRM		(handled by IEF)
    //      Filter
    //      Select characters
    //      PF Keys		(CASE of command)
    //      Detail Lines
    // -------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.ProtectFilter.Flag = import.ProtectFilter.Flag;

    if (AsChar(export.ProtectFilter.Flag) == 'Y')
    {
      var field1 = GetField(export.FilterDocument, "type1");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.FilterPrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    export.FilterPrevious.Type1 = import.FilterPrevious.Type1;
    export.StartNamePrevious.Name = import.StartNamePrevious.Name;
    export.Standard.Assign(import.Scrolling);
    export.StartName.Name = import.StartName.Name;
    export.FilterPrompt.SelectChar = import.FilterPrompt.SelectChar;
    export.FilterDocument.Type1 = import.FilterDocument.Type1;

    // mjr
    // ------------------------------------------------
    // 09/08/1998
    // Moved this check here.
    // -------------------------------------------------------------
    if (!IsEmpty(export.FilterDocument.Type1))
    {
      export.FilterCodeValue.Description = import.FilterCodeValue.Description;
    }
    else
    {
      // Blank out description if type is blank
      export.FilterCodeValue.Description =
        Spaces(CodeValue.Description_MaxLength);
    }

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.G.Assign(import.Import1.Item.G);
      export.Export1.Update.Detail.SelectChar =
        import.Import1.Item.Detail.SelectChar;
    }

    import.Import1.CheckIndex();

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.GexportH.Name =
        import.HiddenPageKeys.Item.GimportH.Name;
    }

    import.HiddenPageKeys.CheckIndex();

    // mjr
    // --------------------------------------
    // Start nexttran and security
    // -----------------------------------------
    MoveStandard(import.HiddenStandard, export.HiddenStandard);

    if (!IsEmpty(import.Scrolling.NextTransaction))
    {
      // The user requested a next tran action.
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // The user is comming into this procedure on a next tran action.
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // Flow from the menu
      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // mjr
    // --------------------------------------
    // End nexttran and security
    // -----------------------------------------
    // mjr
    // ------------------------------------------------
    // 09/08/1998
    // Filter prompt character validation
    // -------------------------------------------------------------
    switch(AsChar(import.FilterPrompt.SelectChar))
    {
      case 'S':
        break;
      case '+':
        export.FilterPrompt.SelectChar = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.FilterPrompt, "selectChar");

        field.Error = true;

        ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";

        return;
    }

    // mjr
    // ------------------------------------------------
    // Filter validation
    // ---------------------------------------------------
    if (!IsEmpty(export.FilterDocument.Type1) && !
      Equal(export.FilterDocument.Type1, export.FilterPrevious.Type1) && !
      Equal(global.Command, "LIST") && !Equal(global.Command, "RLCVAL"))
    {
      local.Code.CodeName = "DOCUMENT TYPE";
      local.CodeValue.Cdvalue = export.FilterDocument.Type1;
      UseCabValidateCodeValue();

      if (AsChar(local.Prompt.Flag) == 'N')
      {
        var field = GetField(export.FilterDocument, "type1");

        field.Error = true;

        export.FilterCodeValue.Description =
          Spaces(CodeValue.Description_MaxLength);
        ExitState = "INVALID_VALUE";

        return;
      }

      local.Code.CodeName = "DOCUMENT TYPE";
      local.Code.Id = 0;
      local.CodeValue.Cdvalue = export.FilterDocument.Type1;
      UseCabGetCodeValueDescription();
      export.HiddenStandard.PageNumber = 1;

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.GexportH.Name = import.StartName.Name;
    }

    // mjr
    // ------------------------------------------------
    // Validate Select Characters
    // ---------------------------------------------------
    local.Common.Count = 0;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      switch(AsChar(export.Export1.Item.Detail.SelectChar))
      {
        case 'S':
          ++local.Common.Count;

          break;
        case ' ':
          break;
        case '*':
          export.Export1.Update.Detail.SelectChar = "";

          break;
        case '+':
          export.Export1.Update.Detail.SelectChar = "";

          break;
        default:
          ++local.Common.Count;

          var field = GetField(export.Export1.Item.Detail, "selectChar");

          field.Error = true;

          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";

          break;
      }
    }

    import.Import1.CheckIndex();

    // mjr
    // -----------------------------------------------
    // 09/08/1998
    // Validate select characters selected
    // ------------------------------------------------------------
    if (local.Common.Count > 0)
    {
      if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT"))
      {
        ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

        return;
      }

      if (local.Common.Count > 1 && (Equal(global.Command, "DUDE") || Equal
        (global.Command, "RETURN")))
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            var field = GetField(export.Export1.Item.Detail, "selectChar");

            field.Error = true;

            break;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

    if (import.HiddenStandard.PageNumber == 0 || Equal
      (global.Command, "DISPLAY"))
    {
      export.HiddenStandard.PageNumber = 1;

      if (!IsEmpty(import.FilterDocument.Type1))
      {
        export.FilterPrevious.Type1 = import.FilterDocument.Type1;
      }

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.GexportH.Name = import.StartName.Name;
    }

    switch(TrimEnd(global.Command))
    {
      case "DUDE":
        if (local.Common.Count == 0)
        {
          if (export.Export1.Count > 0)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;

              break;
            }

            export.Export1.CheckIndex();
          }

          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";

          return;
        }

        export.ReturnDoc.Name = "";
        export.ReturnDoc.Description = "";

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          if (AsChar(import.Import1.Item.Detail.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.G.Name))
            {
              ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
            }
            else
            {
              export.ReturnDoc.Name = export.Export1.Item.G.Name;
              export.ReturnDoc.Description =
                export.Export1.Item.G.Description ?? "";
              ExitState = "ECO_XFR_TO_DUDE";
            }

            break;
          }
        }

        import.Import1.CheckIndex();

        return;
      case "RETLINK":
        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          if (AsChar(import.Import1.Item.Detail.SelectChar) == 'S')
          {
            local.HeldSubscript.Count = import.Import1.Index + 1;
            global.Command = "DISPLAY";
          }
        }

        import.Import1.CheckIndex();

        break;
      case "RETURN":
        export.ReturnDoc.Name = "";
        export.ReturnDoc.Description = "";

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          if (AsChar(import.Import1.Item.Detail.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.G.Name))
            {
              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;

              ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
            }
            else
            {
              export.ReturnDoc.Name = export.Export1.Item.G.Name;
              export.ReturnDoc.Description =
                export.Export1.Item.G.Description ?? "";
            }

            break;
          }
        }

        import.Import1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        return;
      case "NEXT":
        if (import.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        // ---------------------------------------------
        // If page key is equal to SPACES or 0
        //    EXIT STATE IS Scroll forwards invalid
        //    ESCAPE
        // Endif
        // ---------------------------------------------
        if (IsEmpty(export.HiddenPageKeys.Item.GexportH.Name))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "LIST":
        if (AsChar(export.FilterPrompt.SelectChar) == 'S')
        {
          export.Code.CodeName = "DOCUMENT TYPE";
          ExitState = "ECO_LNK_TO_LIST";
        }
        else
        {
          var field = GetField(export.FilterPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        return;
      case "RLCVAL":
        export.FilterPrompt.SelectChar = "";

        if (IsEmpty(import.CodeValue.Cdvalue))
        {
          return;
        }

        export.FilterCodeValue.Description = import.CodeValue.Description;
        export.FilterDocument.Type1 = import.CodeValue.Cdvalue;
        export.HiddenStandard.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportH.Name = import.StartName.Name;
        global.Command = "DISPLAY";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "DISPLAY":
        break;
      default:
        if (!IsEmpty(global.Command))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.FilterPrompt.SelectChar = "";

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      // ---------------------------------------------
      // USE display action block
      // WHICH IMPORTS page_key
      //               page_number
      // WHICH EXPORTS details_group
      //               next_page_key
      //               scrolling_msg
      // ---------------------------------------------
      UseReadDocument();

      if (export.HiddenPageKeys.Index + 1 == Export
        .HiddenPageKeysGroup.Capacity)
      {
        ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
        export.Standard.ScrollingMessage = "MORE -";

        var field = GetField(export.StartName, "name");

        field.Protected = false;
        field.Focused = true;
      }
      else
      {
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        // ---------------------------------------------
        // MOVE next_page_key TO page_key based on page number
        // --------------------------------------------
        export.HiddenPageKeys.Update.GexportH.Name = local.PageKey.Name;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }

    if (local.HeldSubscript.Count > 0)
    {
      export.Export1.Index = local.HeldSubscript.Count - 1;
      export.Export1.CheckSize();

      export.Export1.Update.Detail.SelectChar = "*";

      // mjr
      // -------------------------------------
      // 09/14/1998
      // Make next enterable field containing cursor
      // --------------------------------------------------
      ++export.Export1.Index;
      export.Export1.CheckSize();

      var field = GetField(export.Export1.Item.Detail, "selectChar");

      field.Protected = false;
      field.Focused = true;
    }
    else
    {
      // mjr
      // -------------------------------------
      // 09/14/1998
      // Make next enterable field containing cursor
      // --------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field = GetField(export.Export1.Item.Detail, "selectChar");

        field.Protected = false;
        field.Focused = true;

        return;
      }

      export.Export1.CheckIndex();
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveExport1(ReadDocument.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.G.Assign(source.G);
    target.Detail.SelectChar = source.Detail.SelectChar;
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.Command = source.Command;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    MoveCode(local.Code, useImport.Code);

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.Prompt.Count = useExport.ReturnCode.Count;
    local.Prompt.Flag = useExport.ErrorInDecoding.Flag;
    export.FilterCodeValue.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Prompt.Flag = useExport.ValidCode.Flag;
    local.Prompt.Count = useExport.ReturnCode.Count;
  }

  private void UseReadDocument()
  {
    var useImport = new ReadDocument.Import();
    var useExport = new ReadDocument.Export();

    useImport.Filter.Type1 = export.FilterDocument.Type1;
    useImport.PageStartKey.Name = export.HiddenPageKeys.Item.GexportH.Name;
    useImport.PageNumber.PageNumber = export.HiddenStandard.PageNumber;

    Call(ReadDocument.Execute, useImport, useExport);

    local.PageKey.Name = useExport.PageKey.Name;
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Scrolling.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Common Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Document g;
      private Common detail;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportH.
      /// </summary>
      [JsonPropertyName("gimportH")]
      public Document GimportH
      {
        get => gimportH ??= new();
        set => gimportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Document gimportH;
    }

    /// <summary>A ZdelGroupImportPromptGroup group.</summary>
    [Serializable]
    public class ZdelGroupImportPromptGroup
    {
      /// <summary>
      /// A value of ZdelGrpImportDetailTypeDesc.
      /// </summary>
      [JsonPropertyName("zdelGrpImportDetailTypeDesc")]
      public CodeValue ZdelGrpImportDetailTypeDesc
      {
        get => zdelGrpImportDetailTypeDesc ??= new();
        set => zdelGrpImportDetailTypeDesc = value;
      }

      /// <summary>
      /// A value of ZdelGroupImportPromptDetail.
      /// </summary>
      [JsonPropertyName("zdelGroupImportPromptDetail")]
      public Common ZdelGroupImportPromptDetail
      {
        get => zdelGroupImportPromptDetail ??= new();
        set => zdelGroupImportPromptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private CodeValue zdelGrpImportDetailTypeDesc;
      private Common zdelGroupImportPromptDetail;
    }

    /// <summary>A ZdelGroupImportPreviousGroup group.</summary>
    [Serializable]
    public class ZdelGroupImportPreviousGroup
    {
      /// <summary>
      /// A value of ZdelGImportPrev.
      /// </summary>
      [JsonPropertyName("zdelGImportPrev")]
      public Document ZdelGImportPrev
      {
        get => zdelGImportPrev ??= new();
        set => zdelGImportPrev = value;
      }

      /// <summary>
      /// A value of ZdelGroupImportDetailPrev.
      /// </summary>
      [JsonPropertyName("zdelGroupImportDetailPrev")]
      public Common ZdelGroupImportDetailPrev
      {
        get => zdelGroupImportDetailPrev ??= new();
        set => zdelGroupImportDetailPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Document zdelGImportPrev;
      private Common zdelGroupImportDetailPrev;
    }

    /// <summary>
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
    }

    /// <summary>
    /// A value of ProtectFilter.
    /// </summary>
    [JsonPropertyName("protectFilter")]
    public Common ProtectFilter
    {
      get => protectFilter ??= new();
      set => protectFilter = value;
    }

    /// <summary>
    /// A value of StartName.
    /// </summary>
    [JsonPropertyName("startName")]
    public Document StartName
    {
      get => startName ??= new();
      set => startName = value;
    }

    /// <summary>
    /// A value of StartNamePrevious.
    /// </summary>
    [JsonPropertyName("startNamePrevious")]
    public Document StartNamePrevious
    {
      get => startNamePrevious ??= new();
      set => startNamePrevious = value;
    }

    /// <summary>
    /// A value of FilterPrevious.
    /// </summary>
    [JsonPropertyName("filterPrevious")]
    public Document FilterPrevious
    {
      get => filterPrevious ??= new();
      set => filterPrevious = value;
    }

    /// <summary>
    /// A value of FilterCodeValue.
    /// </summary>
    [JsonPropertyName("filterCodeValue")]
    public CodeValue FilterCodeValue
    {
      get => filterCodeValue ??= new();
      set => filterCodeValue = value;
    }

    /// <summary>
    /// A value of FilterPrompt.
    /// </summary>
    [JsonPropertyName("filterPrompt")]
    public Common FilterPrompt
    {
      get => filterPrompt ??= new();
      set => filterPrompt = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// Gets a value of ZdelGroupImportPrompt.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupImportPromptGroup> ZdelGroupImportPrompt =>
      zdelGroupImportPrompt ??= new(ZdelGroupImportPromptGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelGroupImportPrompt for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupImportPrompt")]
    [Computed]
    public IList<ZdelGroupImportPromptGroup> ZdelGroupImportPrompt_Json
    {
      get => zdelGroupImportPrompt;
      set => ZdelGroupImportPrompt.Assign(value);
    }

    /// <summary>
    /// Gets a value of ZdelGroupImportPrevious.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupImportPreviousGroup> ZdelGroupImportPrevious =>
      zdelGroupImportPrevious ??= new(ZdelGroupImportPreviousGroup.Capacity, 0);
      

    /// <summary>
    /// Gets a value of ZdelGroupImportPrevious for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupImportPrevious")]
    [Computed]
    public IList<ZdelGroupImportPreviousGroup> ZdelGroupImportPrevious_Json
    {
      get => zdelGroupImportPrevious;
      set => ZdelGroupImportPrevious.Assign(value);
    }

    private Document filterDocument;
    private Common protectFilter;
    private Document startName;
    private Document startNamePrevious;
    private Document filterPrevious;
    private CodeValue filterCodeValue;
    private Common filterPrompt;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private CodeValue codeValue;
    private Array<ZdelGroupImportPromptGroup> zdelGroupImportPrompt;
    private Array<ZdelGroupImportPreviousGroup> zdelGroupImportPrevious;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Common Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Document g;
      private Common detail;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportH.
      /// </summary>
      [JsonPropertyName("gexportH")]
      public Document GexportH
      {
        get => gexportH ??= new();
        set => gexportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Document gexportH;
    }

    /// <summary>A ZdelGroupExportPromptGroup group.</summary>
    [Serializable]
    public class ZdelGroupExportPromptGroup
    {
      /// <summary>
      /// A value of ZdelGrpExportDetailTypeDesc.
      /// </summary>
      [JsonPropertyName("zdelGrpExportDetailTypeDesc")]
      public CodeValue ZdelGrpExportDetailTypeDesc
      {
        get => zdelGrpExportDetailTypeDesc ??= new();
        set => zdelGrpExportDetailTypeDesc = value;
      }

      /// <summary>
      /// A value of ZdelGroupExportPromptDetail.
      /// </summary>
      [JsonPropertyName("zdelGroupExportPromptDetail")]
      public Common ZdelGroupExportPromptDetail
      {
        get => zdelGroupExportPromptDetail ??= new();
        set => zdelGroupExportPromptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private CodeValue zdelGrpExportDetailTypeDesc;
      private Common zdelGroupExportPromptDetail;
    }

    /// <summary>A ZdelGroupExportPreviousGroup group.</summary>
    [Serializable]
    public class ZdelGroupExportPreviousGroup
    {
      /// <summary>
      /// A value of ZdelGExportPreviousDocument.
      /// </summary>
      [JsonPropertyName("zdelGExportPreviousDocument")]
      public Document ZdelGExportPreviousDocument
      {
        get => zdelGExportPreviousDocument ??= new();
        set => zdelGExportPreviousDocument = value;
      }

      /// <summary>
      /// A value of ZdelGExportPreviousCommon.
      /// </summary>
      [JsonPropertyName("zdelGExportPreviousCommon")]
      public Common ZdelGExportPreviousCommon
      {
        get => zdelGExportPreviousCommon ??= new();
        set => zdelGExportPreviousCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Document zdelGExportPreviousDocument;
      private Common zdelGExportPreviousCommon;
    }

    /// <summary>
    /// A value of ReturnDoc.
    /// </summary>
    [JsonPropertyName("returnDoc")]
    public Document ReturnDoc
    {
      get => returnDoc ??= new();
      set => returnDoc = value;
    }

    /// <summary>
    /// A value of ProtectFilter.
    /// </summary>
    [JsonPropertyName("protectFilter")]
    public Common ProtectFilter
    {
      get => protectFilter ??= new();
      set => protectFilter = value;
    }

    /// <summary>
    /// A value of StartName.
    /// </summary>
    [JsonPropertyName("startName")]
    public Document StartName
    {
      get => startName ??= new();
      set => startName = value;
    }

    /// <summary>
    /// A value of StartNamePrevious.
    /// </summary>
    [JsonPropertyName("startNamePrevious")]
    public Document StartNamePrevious
    {
      get => startNamePrevious ??= new();
      set => startNamePrevious = value;
    }

    /// <summary>
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
    }

    /// <summary>
    /// A value of FilterPrevious.
    /// </summary>
    [JsonPropertyName("filterPrevious")]
    public Document FilterPrevious
    {
      get => filterPrevious ??= new();
      set => filterPrevious = value;
    }

    /// <summary>
    /// A value of FilterCodeValue.
    /// </summary>
    [JsonPropertyName("filterCodeValue")]
    public CodeValue FilterCodeValue
    {
      get => filterCodeValue ??= new();
      set => filterCodeValue = value;
    }

    /// <summary>
    /// A value of FilterPrompt.
    /// </summary>
    [JsonPropertyName("filterPrompt")]
    public Common FilterPrompt
    {
      get => filterPrompt ??= new();
      set => filterPrompt = value;
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

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// Gets a value of ZdelGroupExportPrompt.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupExportPromptGroup> ZdelGroupExportPrompt =>
      zdelGroupExportPrompt ??= new(ZdelGroupExportPromptGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelGroupExportPrompt for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupExportPrompt")]
    [Computed]
    public IList<ZdelGroupExportPromptGroup> ZdelGroupExportPrompt_Json
    {
      get => zdelGroupExportPrompt;
      set => ZdelGroupExportPrompt.Assign(value);
    }

    /// <summary>
    /// Gets a value of ZdelGroupExportPrevious.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupExportPreviousGroup> ZdelGroupExportPrevious =>
      zdelGroupExportPrevious ??= new(ZdelGroupExportPreviousGroup.Capacity, 0);
      

    /// <summary>
    /// Gets a value of ZdelGroupExportPrevious for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupExportPrevious")]
    [Computed]
    public IList<ZdelGroupExportPreviousGroup> ZdelGroupExportPrevious_Json
    {
      get => zdelGroupExportPrevious;
      set => ZdelGroupExportPrevious.Assign(value);
    }

    private Document returnDoc;
    private Common protectFilter;
    private Document startName;
    private Document startNamePrevious;
    private Document filterDocument;
    private Document filterPrevious;
    private CodeValue filterCodeValue;
    private Common filterPrompt;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Standard hiddenStandard;
    private Code code;
    private Array<ZdelGroupExportPromptGroup> zdelGroupExportPrompt;
    private Array<ZdelGroupExportPreviousGroup> zdelGroupExportPrevious;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HeldSubscript.
    /// </summary>
    [JsonPropertyName("heldSubscript")]
    public Common HeldSubscript
    {
      get => heldSubscript ??= new();
      set => heldSubscript = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of PageKey.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public Document PageKey
    {
      get => pageKey ??= new();
      set => pageKey = value;
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

    private Common heldSubscript;
    private Common prompt;
    private CodeValue codeValue;
    private Code code;
    private Document pageKey;
    private Common common;
  }
#endregion
}
