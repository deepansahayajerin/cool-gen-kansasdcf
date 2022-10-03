// Program: CO_CDVC_MAINTAIN_CODE_VALUE_COMB, ID: 371825025, model: 746.
// Short name: SWECDVCP
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
/// A program: CO_CDVC_MAINTAIN_CODE_VALUE_COMB.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure facilitates maintenance of entity CODE_VALUE_COMBINATION
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoCdvcMaintainCodeValueComb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CDVC_MAINTAIN_CODE_VALUE_COMB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCdvcMaintainCodeValueComb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCdvcMaintainCodeValueComb.
  /// </summary>
  public CoCdvcMaintainCodeValueComb(IContext context, Import import,
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
    // *********************************************
    // Date      	Author		Reason
    // 04-26-95                	Initial Dev
    // 01/23/96  	a. Hackler  	add Security/nexttran
    // 10/10/96	R. Marchman	add data level sec.
    //                         	change next tran
    // 04/28/97	A.Kinney	changed Current_Date
    // 06/18/97        M. D. Wheaton   Removed datenum
    // *********************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        UseScCabNextTranGet();

        return;
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        return;
      }
    }
    else
    {
      return;
    }

    // **** end   group B ****
    export.PrimaryCode.Assign(import.PrimaryCode);
    export.PrimaryCodeValue.Assign(import.PrimaryCodeValue);
    export.Secondary.Assign(import.Secondary);
    export.ListOnlyActiveCases.Flag = import.ListOnlyActiveCases.Flag;
    export.Starting.Cdvalue = import.Starting.Cdvalue;
    export.ListPrimaryCodeNames.PromptField =
      import.ListPrimaryCodeNames.PromptField;
    export.ListPrimaryCodeValues.PromptField =
      import.ListPrimaryCodeValues.PromptField;
    export.ListCrossValCodeName.PromptField =
      import.ListCrossValCodeNames.PromptField;
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

      export.Export1.Update.DetailSecondary.Assign(
        import.Import1.Item.DetailSecondary);
      MoveCodeValueCombination(import.Import1.Item.Detail,
        export.Export1.Update.Detail);

      if (Equal(export.Export1.Item.Detail.ExpirationDate, local.MaxDate.Date))
      {
        export.Export1.Update.Detail.ExpirationDate = local.Zero.Date;
      }
    }

    import.Import1.CheckIndex();

    if (Equal(global.Command, "RLCNAME"))
    {
      if (AsChar(export.ListPrimaryCodeNames.PromptField) == 'S')
      {
        export.ListPrimaryCodeNames.PromptField = "";

        if (IsEmpty(import.SelectedFromList.CodeName))
        {
          var field = GetField(export.PrimaryCode, "codeName");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          global.Command = "DISPLAY";
          export.PrimaryCode.Assign(import.SelectedFromList);

          var field = GetField(export.PrimaryCodeValue, "cdvalue");

          field.Protected = false;
          field.Focused = true;
        }
      }

      if (AsChar(export.ListCrossValCodeName.PromptField) == 'S')
      {
        export.ListCrossValCodeName.PromptField = "";

        if (IsEmpty(import.SelectedFromList.CodeName))
        {
          var field = GetField(export.Secondary, "codeName");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          global.Command = "DISPLAY";
          export.Secondary.Assign(import.SelectedFromList);

          var field = GetField(export.Starting, "cdvalue");

          field.Protected = false;
          field.Focused = true;
        }
      }
    }

    if (Equal(global.Command, "RLCVAL"))
    {
      if (AsChar(export.ListPrimaryCodeValues.PromptField) == 'S')
      {
        export.ListPrimaryCodeValues.PromptField = "";

        if (IsEmpty(export.PrimaryCodeValue.Cdvalue))
        {
          var field = GetField(export.PrimaryCodeValue, "cdvalue");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          global.Command = "DISPLAY";

          var field = GetField(export.Secondary, "codeName");

          field.Protected = false;
          field.Focused = true;
        }
      }
    }

    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "RLCNAME") || Equal(global.Command, "RLCVAL"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "DELETE":
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

        local.AddUpdateCommand.Command = "DELETE";
        UseCoUpdateCodeValCombination();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.LastErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          var field1 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.Export1.Item.DetailSecondary, "id");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.DetailSecondary, "cdvalue");

          field3.Error = true;

          var field4 =
            GetField(export.Export1.Item.DetailSecondary, "description");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field5.Error = true;

          var field6 = GetField(export.Export1.Item.Detail, "expirationDate");

          field6.Error = true;

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
        ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";

        break;
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
        UseCoUpdateCodeValCombination();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.LastErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          var field1 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.Export1.Item.DetailSecondary, "id");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.DetailSecondary, "cdvalue");

          field3.Error = true;

          var field4 =
            GetField(export.Export1.Item.DetailSecondary, "description");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field5.Error = true;

          var field6 = GetField(export.Export1.Item.Detail, "expirationDate");

          field6.Error = true;

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
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "LIST":
        if (!IsEmpty(export.ListPrimaryCodeNames.PromptField) && AsChar
          (export.ListPrimaryCodeNames.PromptField) != 'S')
        {
          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE3";

          var field = GetField(export.ListPrimaryCodeNames, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListPrimaryCodeNames.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_CODE_NAMES";

          return;
        }

        if (!IsEmpty(export.ListPrimaryCodeValues.PromptField) && AsChar
          (export.ListPrimaryCodeValues.PromptField) != 'S')
        {
          ExitState = "ZD_ACO_NE00_INVALID_PROMPT_VALUE";

          var field = GetField(export.ListPrimaryCodeValues, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListPrimaryCodeValues.PromptField) == 'S')
        {
          if (IsEmpty(export.PrimaryCode.CodeName))
          {
            ExitState = "CO0000_PRIM_CODE_NAME_REQD";

            var field = GetField(export.PrimaryCode, "codeName");

            field.Error = true;

            return;
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_VALUES";

          return;
        }

        if (!IsEmpty(export.ListCrossValCodeName.PromptField) && AsChar
          (export.ListCrossValCodeName.PromptField) != 'S')
        {
          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE3";

          var field = GetField(export.ListCrossValCodeName, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListCrossValCodeName.PromptField) == 'S')
        {
          if (IsEmpty(export.PrimaryCode.CodeName))
          {
            ExitState = "CO0000_CODE_NAME_N_VALUE_REQD";

            var field = GetField(export.PrimaryCode, "codeName");

            field.Error = true;

            if (IsEmpty(export.PrimaryCodeValue.Cdvalue))
            {
              var field1 = GetField(export.PrimaryCodeValue, "cdvalue");

              field1.Error = true;
            }

            return;
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_NAMES";

          return;
        }

        ExitState = "ZD_ACO_NE00_MUST_SELECT_4_PROMPT";

        break;
      case "RLCNAME":
        break;
      case "RLCVAL":
        break;
      case "DISPLAY":
        if (ReadCode1())
        {
          export.PrimaryCode.Assign(entities.ExistingPrimaryCode);
        }
        else
        {
          var field = GetField(export.PrimaryCode, "codeName");

          field.Error = true;

          ExitState = "CO0000_PRIMARY_CODE_NF";

          return;
        }

        if (ReadCodeValue1())
        {
          export.PrimaryCodeValue.Assign(entities.ExistingPrimaryCodeValue);
        }
        else
        {
          var field = GetField(export.PrimaryCodeValue, "cdvalue");

          field.Error = true;

          ExitState = "CO0000_PRIMARY_CODE_VALUE_NF";

          return;
        }

        if (ReadCode2())
        {
          export.Secondary.Assign(entities.ExistingSecondaryCode);
        }
        else
        {
          var field = GetField(export.Secondary, "codeName");

          field.Error = true;

          ExitState = "CO0000_CROSS_VAL_CODE_NF";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
          export.Export1.Update.DetailSecondary.Id = 0;
          export.Export1.Update.DetailSecondary.Cdvalue = "";
          export.Export1.Update.DetailSecondary.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.Export1.Update.DetailSecondary.EffectiveDate = local.Zero.Date;
          export.Export1.Update.DetailSecondary.ExpirationDate =
            local.Zero.Date;
          export.Export1.Update.Detail.EffectiveDate = local.Zero.Date;
          export.Export1.Update.Detail.ExpirationDate = local.Zero.Date;
        }

        export.Export1.CheckIndex();
        export.Export1.Index = -1;

        foreach(var item in ReadCodeValue2())
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          if (export.Export1.Index >= Export.ExportGroup.Capacity)
          {
            break;
          }

          export.Export1.Update.DetailSecondary.Assign(
            entities.ExistingSecondaryCodeValue);

          if (ReadCodeValueCombination())
          {
            MoveCodeValueCombination(entities.Existing,
              export.Export1.Update.Detail);
            local.DateWorkArea.Date = entities.Existing.ExpirationDate;
            UseCabSetMaximumDiscontinueDate2();
            export.Export1.Update.Detail.ExpirationDate =
              local.DateWorkArea.Date;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
        }

        if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

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
        UseCoUpdateCodeValCombination();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.LastErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          var field1 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.Export1.Item.DetailSecondary, "id");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.DetailSecondary, "cdvalue");

          field3.Error = true;

          var field4 =
            GetField(export.Export1.Item.DetailSecondary, "description");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field5.Error = true;

          var field6 = GetField(export.Export1.Item.Detail, "expirationDate");

          field6.Error = true;

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
        ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_2";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
    target.DisplayTitle = source.DisplayTitle;
  }

  private static void MoveCodeValueCombination(CodeValueCombination source,
    CodeValueCombination target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.ExpirationDate = source.ExpirationDate;
  }

  private static void MoveExport1(CoUpdateCodeValCombination.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailSelection.SelectChar = source.DetailSelection.SelectChar;
    MoveCodeValueCombination(source.Detail, target.Detail);
    target.DetailSecondary.Assign(source.DetailSecondary);
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    CoUpdateCodeValCombination.Import.ImportGroup target)
  {
    target.DetailSelection.SelectChar = source.DetailSelection.SelectChar;
    MoveCodeValueCombination(source.Detail, target.Detail);
    target.DetailSecondary.Assign(source.DetailSecondary);
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

  private void UseCoUpdateCodeValCombination()
  {
    var useImport = new CoUpdateCodeValCombination.Import();
    var useExport = new CoUpdateCodeValCombination.Export();

    useImport.AddUpdateCommand.Command = local.AddUpdateCommand.Command;
    MoveCode(export.PrimaryCode, useImport.PrimaryCode);
    MoveCode(export.Secondary, useImport.Secondary);
    useImport.PrimaryCodeValue.Assign(export.PrimaryCodeValue);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(CoUpdateCodeValCombination.Execute, useImport, useExport);

    local.LastErrorLineNo.Count = useExport.ErrorLineNo.Count;
    export.PrimaryCode.Assign(useExport.PrimaryCode);
    export.Secondary.Assign(useExport.Secondary);
    export.PrimaryCodeValue.Assign(useExport.PrimaryCodeValue);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);

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

  private bool ReadCode1()
  {
    entities.ExistingPrimaryCode.Populated = false;

    return Read("ReadCode1",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.PrimaryCode.CodeName);
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryCode.Id = db.GetInt32(reader, 0);
        entities.ExistingPrimaryCode.CodeName = db.GetString(reader, 1);
        entities.ExistingPrimaryCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingPrimaryCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingPrimaryCode.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingPrimaryCode.Populated = true;
      });
  }

  private bool ReadCode2()
  {
    entities.ExistingSecondaryCode.Populated = false;

    return Read("ReadCode2",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Secondary.CodeName);
      },
      (db, reader) =>
      {
        entities.ExistingSecondaryCode.Id = db.GetInt32(reader, 0);
        entities.ExistingSecondaryCode.CodeName = db.GetString(reader, 1);
        entities.ExistingSecondaryCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingSecondaryCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingSecondaryCode.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingSecondaryCode.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.ExistingPrimaryCodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingPrimaryCode.Id);
        db.SetString(command, "cdvalue", export.PrimaryCodeValue.Cdvalue);
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingPrimaryCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingPrimaryCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingPrimaryCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingPrimaryCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingPrimaryCodeValue.Description = db.GetString(reader, 5);
        entities.ExistingPrimaryCodeValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue2()
  {
    entities.ExistingSecondaryCodeValue.Populated = false;

    return ReadEach("ReadCodeValue2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "codId", entities.ExistingSecondaryCode.Id);
          
        db.SetString(command, "cdvalue", export.Starting.Cdvalue);
        db.SetString(command, "flag", export.ListOnlyActiveCases.Flag);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingSecondaryCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingSecondaryCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingSecondaryCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingSecondaryCodeValue.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingSecondaryCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingSecondaryCodeValue.Description =
          db.GetString(reader, 5);
        entities.ExistingSecondaryCodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadCodeValueCombination()
  {
    entities.Existing.Populated = false;

    return Read("ReadCodeValueCombination",
      (db, command) =>
      {
        db.SetInt32(command, "covId", entities.ExistingPrimaryCodeValue.Id);
        db.SetInt32(command, "covSId", entities.ExistingSecondaryCodeValue.Id);
      },
      (db, reader) =>
      {
        entities.Existing.Id = db.GetInt32(reader, 0);
        entities.Existing.CovId = db.GetInt32(reader, 1);
        entities.Existing.CovSId = db.GetInt32(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.ExpirationDate = db.GetDate(reader, 4);
        entities.Existing.Populated = true;
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
      public CodeValueCombination Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailSecondary.
      /// </summary>
      [JsonPropertyName("detailSecondary")]
      public CodeValue DetailSecondary
      {
        get => detailSecondary ??= new();
        set => detailSecondary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private CodeValueCombination detail;
      private CodeValue detailSecondary;
    }

    /// <summary>
    /// A value of SelectedFromList.
    /// </summary>
    [JsonPropertyName("selectedFromList")]
    public Code SelectedFromList
    {
      get => selectedFromList ??= new();
      set => selectedFromList = value;
    }

    /// <summary>
    /// A value of ListPrimaryCodeValues.
    /// </summary>
    [JsonPropertyName("listPrimaryCodeValues")]
    public Standard ListPrimaryCodeValues
    {
      get => listPrimaryCodeValues ??= new();
      set => listPrimaryCodeValues = value;
    }

    /// <summary>
    /// A value of ListCrossValCodeNames.
    /// </summary>
    [JsonPropertyName("listCrossValCodeNames")]
    public Standard ListCrossValCodeNames
    {
      get => listCrossValCodeNames ??= new();
      set => listCrossValCodeNames = value;
    }

    /// <summary>
    /// A value of ListPrimaryCodeNames.
    /// </summary>
    [JsonPropertyName("listPrimaryCodeNames")]
    public Standard ListPrimaryCodeNames
    {
      get => listPrimaryCodeNames ??= new();
      set => listPrimaryCodeNames = value;
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
    /// A value of PrimaryCode.
    /// </summary>
    [JsonPropertyName("primaryCode")]
    public Code PrimaryCode
    {
      get => primaryCode ??= new();
      set => primaryCode = value;
    }

    /// <summary>
    /// A value of Secondary.
    /// </summary>
    [JsonPropertyName("secondary")]
    public Code Secondary
    {
      get => secondary ??= new();
      set => secondary = value;
    }

    /// <summary>
    /// A value of PrimaryCodeValue.
    /// </summary>
    [JsonPropertyName("primaryCodeValue")]
    public CodeValue PrimaryCodeValue
    {
      get => primaryCodeValue ??= new();
      set => primaryCodeValue = value;
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

    private Code selectedFromList;
    private Standard listPrimaryCodeValues;
    private Standard listCrossValCodeNames;
    private Standard listPrimaryCodeNames;
    private Common listOnlyActiveCases;
    private CodeValue starting;
    private Code primaryCode;
    private Code secondary;
    private CodeValue primaryCodeValue;
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
      public CodeValueCombination Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailSecondary.
      /// </summary>
      [JsonPropertyName("detailSecondary")]
      public CodeValue DetailSecondary
      {
        get => detailSecondary ??= new();
        set => detailSecondary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private CodeValueCombination detail;
      private CodeValue detailSecondary;
    }

    /// <summary>
    /// A value of ListPrimaryCodeValues.
    /// </summary>
    [JsonPropertyName("listPrimaryCodeValues")]
    public Standard ListPrimaryCodeValues
    {
      get => listPrimaryCodeValues ??= new();
      set => listPrimaryCodeValues = value;
    }

    /// <summary>
    /// A value of ListCrossValCodeName.
    /// </summary>
    [JsonPropertyName("listCrossValCodeName")]
    public Standard ListCrossValCodeName
    {
      get => listCrossValCodeName ??= new();
      set => listCrossValCodeName = value;
    }

    /// <summary>
    /// A value of ListPrimaryCodeNames.
    /// </summary>
    [JsonPropertyName("listPrimaryCodeNames")]
    public Standard ListPrimaryCodeNames
    {
      get => listPrimaryCodeNames ??= new();
      set => listPrimaryCodeNames = value;
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
    /// A value of PrimaryCode.
    /// </summary>
    [JsonPropertyName("primaryCode")]
    public Code PrimaryCode
    {
      get => primaryCode ??= new();
      set => primaryCode = value;
    }

    /// <summary>
    /// A value of Secondary.
    /// </summary>
    [JsonPropertyName("secondary")]
    public Code Secondary
    {
      get => secondary ??= new();
      set => secondary = value;
    }

    /// <summary>
    /// A value of PrimaryCodeValue.
    /// </summary>
    [JsonPropertyName("primaryCodeValue")]
    public CodeValue PrimaryCodeValue
    {
      get => primaryCodeValue ??= new();
      set => primaryCodeValue = value;
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

    private Standard listPrimaryCodeValues;
    private Standard listCrossValCodeName;
    private Standard listPrimaryCodeNames;
    private Common listOnlyActiveCases;
    private CodeValue starting;
    private Code primaryCode;
    private Code secondary;
    private CodeValue primaryCodeValue;
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
    /// A value of ZeroInitialised.
    /// </summary>
    [JsonPropertyName("zeroInitialised")]
    public DateWorkArea ZeroInitialised
    {
      get => zeroInitialised ??= new();
      set => zeroInitialised = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private DateWorkArea zero;
    private DateWorkArea current;
    private Common selCount;
    private Common addUpdateCommand;
    private DateWorkArea zeroInitialised;
    private DateWorkArea maxDate;
    private DateWorkArea dateWorkArea;
    private Common lastErrorLineNo;
    private NextTranInfo nextTranInfo;
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
    public CodeValueCombination Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingSecondaryCodeValue.
    /// </summary>
    [JsonPropertyName("existingSecondaryCodeValue")]
    public CodeValue ExistingSecondaryCodeValue
    {
      get => existingSecondaryCodeValue ??= new();
      set => existingSecondaryCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryCodeValue.
    /// </summary>
    [JsonPropertyName("existingPrimaryCodeValue")]
    public CodeValue ExistingPrimaryCodeValue
    {
      get => existingPrimaryCodeValue ??= new();
      set => existingPrimaryCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingSecondaryCode.
    /// </summary>
    [JsonPropertyName("existingSecondaryCode")]
    public Code ExistingSecondaryCode
    {
      get => existingSecondaryCode ??= new();
      set => existingSecondaryCode = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryCode.
    /// </summary>
    [JsonPropertyName("existingPrimaryCode")]
    public Code ExistingPrimaryCode
    {
      get => existingPrimaryCode ??= new();
      set => existingPrimaryCode = value;
    }

    private CodeValueCombination existing;
    private CodeValue existingSecondaryCodeValue;
    private CodeValue existingPrimaryCodeValue;
    private Code existingSecondaryCode;
    private Code existingPrimaryCode;
  }
#endregion
}
