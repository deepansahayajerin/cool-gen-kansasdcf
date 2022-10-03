// Program: CO_CDVA_MAINTAIN_CODE_VALUE, ID: 371824245, model: 746.
// Short name: SWECDVAP
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
/// A program: CO_CDVA_MAINTAIN_CODE_VALUE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure facilitates maintenance of entity CODE_VALUE
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoCdvaMaintainCodeValue: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CDVA_MAINTAIN_CODE_VALUE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCdvaMaintainCodeValue(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCdvaMaintainCodeValue.
  /// </summary>
  public CoCdvaMaintainCodeValue(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // Date      Developer  PR #            Reason
    // 04-26-95                	Initial Dev
    // 01/23/96  a. Hackler    	add Security/nexttran
    // 12/03/96  R. Marchman   	Add new security and next tran
    // 04/28/97  A.Kinney	        Changed Current_Date
    // 06/18/97  M. D. Wheaton         Removed datenum
    // 03/25/98  S. A. Konkader	ZDEL Cleanup
    // 02/03/00  SWSRCHF               Removed Exit States prefixed
    //                                 
    // with "ZD"
    // 02/03/00  SWSRCHF  H00085435 Changed the code to perform
    //                                 
    // a re-display of the data after
    // an
    //                                 
    // ADD or UPDATE
    // 01/22/09  Arun Mathias CQ#8604  Do not allow overlapping timeframes.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Code.Assign(import.Code);
    export.Starting.Cdvalue = import.Starting.Cdvalue;
    export.ListOnlyActiveCases.Flag = import.ListOnlyActiveCases.Flag;
    export.PromptListCodeNames.PromptField =
      import.PromptListCodeNames.PromptField;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    UseCabSetMaximumDiscontinueDate1();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailSelection.SelectChar =
        import.Import1.Item.DetailSelection.SelectChar;

      if (!IsEmpty(export.Export1.Item.DetailSelection.SelectChar))
      {
        local.SelCount.Count = (int)((long)local.SelCount.Count + 1);
      }

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);

      if (Equal(export.Export1.Item.Detail.ExpirationDate, local.MaxDate.Date))
      {
        export.Export1.Update.Detail.ExpirationDate =
          local.ZeroInitialised.Date;
      }
    }

    import.Import1.CheckIndex();

    if (Equal(global.Command, "RLCNAME"))
    {
      if (IsEmpty(import.Selected.CodeName))
      {
        var field = GetField(export.Code, "codeName");

        field.Protected = false;
        field.Focused = true;

        return;

        export.PromptListCodeNames.PromptField = "+";
      }
      else
      {
        export.Code.Assign(import.Selected);
        global.Command = "DISPLAY";
      }
    }

    if (!Equal(global.Command, "LIST"))
    {
      export.PromptListCodeNames.PromptField = "+";
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
    local.Update.Flag = "N";
    local.Add.Flag = "N";

    // *** CQ#8604 Changes Begin Here ***
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (local.SelCount.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailSelection.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailSelection, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

    // *** CQ#8604 Changes Ends  Here ***
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        if (local.SelCount.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          var field =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        local.AddUpdateCommand.Command = "ADD";
        UseCoUpdateCodeValueDetails();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
          IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.LastErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          var field1 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field1.Error = true;

          // *** CQ#8604 Changes Begin Here ***
          // Commented code value id because user could change the value.
          // *** CQ#8604 Changes Ends  Here ***
          var field2 = GetField(export.Export1.Item.Detail, "cdvalue");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.Detail, "description");

          field3.Error = true;

          var field4 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Detail, "expirationDate");

          field5.Error = true;

          return;
        }

        // *** Problem report H00085435
        // *** 02/03/00 SWSRCHF
        // *** start
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
        }

        export.Export1.CheckIndex();
        local.Add.Flag = "Y";
        global.Command = "DISPLAY";

        // *** end
        // *** 02/03/00 SWSRCHF
        // *** Problem report H00085435
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "LIST":
        switch(AsChar(export.PromptListCodeNames.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LIST_CODE_NAMES";

            break;
          case ' ':
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        return;
      case "RLCNAME":
        break;
      case "DISPLAY":
        // *** Problem report H00085435
        // *** 02/03/00 SWSRCHF
        // *** start
        // *** end
        // *** 02/03/00 SWSRCHF
        // *** Problem report H00085435
        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "UPDATE":
        if (local.SelCount.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          var field =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        local.AddUpdateCommand.Command = "UPDATE";
        UseCoUpdateCodeValueDetails();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
          IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.LastErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          var field1 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field1.Error = true;

          // *** CQ#8604 Changes Begin Here ***
          // Commented code value id because user could change the value.
          // *** CQ#8604 Changes  End  Here ***
          var field2 = GetField(export.Export1.Item.Detail, "cdvalue");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.Detail, "description");

          field3.Error = true;

          var field4 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Detail, "expirationDate");

          field5.Error = true;

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
        }

        export.Export1.CheckIndex();

        // *** Problem report H00085435
        // *** 02/03/00 SWSRCHF
        // *** start
        local.Update.Flag = "Y";
        global.Command = "DISPLAY";

        // *** end
        // *** 02/03/00 SWSRCHF
        // *** Problem report H00085435
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *** Problem report H00085435
    // *** 02/03/00 SWSRCHF
    // *** start
    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadCode())
      {
        export.Code.Assign(entities.ExistingCode);
        local.DateWorkArea.Date = entities.ExistingCode.ExpirationDate;
        UseCabSetMaximumDiscontinueDate2();
        export.Code.ExpirationDate = local.DateWorkArea.Date;
      }
      else
      {
        ExitState = "CODE_NF";

        return;
      }

      export.Export1.Index = -1;

      foreach(var item in ReadCodeValue())
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          break;
        }

        export.Export1.Update.Detail.Assign(entities.ExistingCodeValue);
        local.DateWorkArea.Date = entities.ExistingCodeValue.ExpirationDate;
        UseCabSetMaximumDiscontinueDate2();
        export.Export1.Update.Detail.ExpirationDate = local.DateWorkArea.Date;
        export.Export1.Update.DetailSelection.SelectChar = "";
      }

      if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
      {
        if (AsChar(local.Add.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else if (AsChar(local.Update.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }

        return;
      }
      else if (AsChar(local.Add.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      else if (AsChar(local.Update.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      for(++export.Export1.Index; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        export.Export1.Update.DetailSelection.SelectChar = "";
        export.Export1.Update.Detail.Id = 0;
        export.Export1.Update.Detail.Cdvalue = "";
        export.Export1.Update.Detail.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.Export1.Update.Detail.EffectiveDate = local.Zero.Date;
        export.Export1.Update.Detail.ExpirationDate = local.Zero.Date;
      }

      export.Export1.CheckIndex();
    }
    else
    {
    }

    // *** end
    // *** 02/03/00 SWSRCHF
    // *** Problem report H00085435
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    CoUpdateCodeValueDetails.Import.ImportGroup target)
  {
    target.DetailSelection.SelectChar = source.DetailSelection.SelectChar;
    target.Detail.Assign(source.Detail);
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

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseCoUpdateCodeValueDetails()
  {
    var useImport = new CoUpdateCodeValueDetails.Import();
    var useExport = new CoUpdateCodeValueDetails.Export();

    useImport.AddUpdateCommand.Command = local.AddUpdateCommand.Command;
    MoveCode(export.Code, useImport.Code);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(CoUpdateCodeValueDetails.Execute, useImport, useExport);

    local.LastErrorLineNo.Count = useExport.ErrorLineNo.Count;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCode()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Code.CodeName);
      },
      (db, reader) =>
      {
        entities.ExistingCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingCode.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingCode.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.ExistingCodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
        db.SetString(command, "cdvalue", export.Starting.Cdvalue);
        db.SetString(command, "flag", export.ListOnlyActiveCases.Flag);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.ExistingCodeValue.Description = db.GetString(reader, 5);
        entities.ExistingCodeValue.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CodeValue Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private Common detailSelection;
      private CodeValue detail;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Code Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of PromptListCodeNames.
    /// </summary>
    [JsonPropertyName("promptListCodeNames")]
    public Standard PromptListCodeNames
    {
      get => promptListCodeNames ??= new();
      set => promptListCodeNames = value;
    }

    /// <summary>
    /// A value of ListOnlyActiveCases.
    /// </summary>
    [JsonPropertyName("listOnlyActiveCases")]
    public Common ListOnlyActiveCases
    {
      get => listOnlyActiveCases ??= new();
      set => listOnlyActiveCases = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CodeValue Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private Code selected;
    private Standard promptListCodeNames;
    private Common listOnlyActiveCases;
    private CodeValue starting;
    private Code code;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CodeValue Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private Common detailSelection;
      private CodeValue detail;
    }

    /// <summary>
    /// A value of PromptListCodeNames.
    /// </summary>
    [JsonPropertyName("promptListCodeNames")]
    public Standard PromptListCodeNames
    {
      get => promptListCodeNames ??= new();
      set => promptListCodeNames = value;
    }

    /// <summary>
    /// A value of ListOnlyActiveCases.
    /// </summary>
    [JsonPropertyName("listOnlyActiveCases")]
    public Common ListOnlyActiveCases
    {
      get => listOnlyActiveCases ??= new();
      set => listOnlyActiveCases = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CodeValue Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private Standard promptListCodeNames;
    private Common listOnlyActiveCases;
    private CodeValue starting;
    private Code code;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Add.
    /// </summary>
    [JsonPropertyName("add")]
    public Common Add
    {
      get => add ??= new();
      set => add = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
    }

    /// <summary>
    /// A value of AddUpdateCommand.
    /// </summary>
    [JsonPropertyName("addUpdateCommand")]
    public Common AddUpdateCommand
    {
      get => addUpdateCommand ??= new();
      set => addUpdateCommand = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ZeroInitialised.
    /// </summary>
    [JsonPropertyName("zeroInitialised")]
    public DateWorkArea ZeroInitialised
    {
      get => zeroInitialised ??= new();
      set => zeroInitialised = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of CursorPosition.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    public CursorPosition CursorPosition
    {
      get => cursorPosition ??= new();
      set => cursorPosition = value;
    }

    /// <summary>
    /// A value of LastEntryNo.
    /// </summary>
    [JsonPropertyName("lastEntryNo")]
    public Common LastEntryNo
    {
      get => lastEntryNo ??= new();
      set => lastEntryNo = value;
    }

    /// <summary>
    /// A value of LastErrorLineNo.
    /// </summary>
    [JsonPropertyName("lastErrorLineNo")]
    public Common LastErrorLineNo
    {
      get => lastErrorLineNo ??= new();
      set => lastErrorLineNo = value;
    }

    private Common update;
    private Common add;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Common selCount;
    private Common addUpdateCommand;
    private DateWorkArea maxDate;
    private DateWorkArea zeroInitialised;
    private DateWorkArea dateWorkArea;
    private CursorPosition cursorPosition;
    private Common lastEntryNo;
    private Common lastErrorLineNo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCodeValue.
    /// </summary>
    [JsonPropertyName("existingCodeValue")]
    public CodeValue ExistingCodeValue
    {
      get => existingCodeValue ??= new();
      set => existingCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingCode.
    /// </summary>
    [JsonPropertyName("existingCode")]
    public Code ExistingCode
    {
      get => existingCode ??= new();
      set => existingCode = value;
    }

    private CodeValue existingCodeValue;
    private Code existingCode;
  }
#endregion
}
