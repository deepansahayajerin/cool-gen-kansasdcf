// Program: CO_CDVL_LIST_CODE_VALUE, ID: 371457614, model: 746.
// Short name: SWECDVLP
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
/// A program: CO_CDVL_LIST_CODE_VALUE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure lists occurrences of entity CODE_VALUE
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoCdvlListCodeValue: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CDVL_LIST_CODE_VALUE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCdvlListCodeValue(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCdvlListCodeValue.
  /// </summary>
  public CoCdvlListCodeValue(IContext context, Import import, Export export):
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
    // MAINTENANCE LOG
    // AUTHOR    DATE        CHG REQ#     DESCRIPTION
    // Govindaraj 12-16-1994              Initial coding
    // A. Hackler 01-23-1996              ADD SECURITY AND NEXT/TRAN
    // R. Marchman 10/14/96               Add data level security
    // *********************************************
    // *********************************************
    // SYSTEM:      KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION: This procedure step lists the code values for a given code.
    // PROCESSING:
    // If a combination code name and its value are passed to this procedure 
    // step, then only those code values which are valid for the given
    // combination code value are listed.
    // If input "List active values only" = "Y" then only the current values are
    // listed. Otherwise all the values are listed.
    // If a starting code value is specified, then only those with value greater
    // or equal to the given starting code value are listed.
    // The procedure step tries to read and list a maximum of 100 values at a 
    // time. To list beyond those 100 entries, a new starting value needs to be
    // specified.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // CODE	                 - R - -
    // CODE_VALUE               - R - -
    // CODE_VALUE_COMBINATION   - R - -
    // DATABASE FILES USED:
    // CREATED BY: Govindaraj Kadambi
    // DATE CREATED: 12-16-1994
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.AllowChangingCodeName.Flag = import.AllowChangingCodeName.Flag;
    export.HiddenReturnMultRecs.Flag = import.HiddenReturnMultRecs.Flag;

    if (AsChar(export.AllowChangingCodeName.Flag) == 'Y')
    {
      var field1 = GetField(export.Code, "codeName");

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.ListCodeName, "promptField");

      field2.Color = "green";
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;
    }
    else if (Equal(global.Command, "XXNEXTXX"))
    {
    }
    else
    {
    }

    if (Equal(global.Command, "FROMMENU"))
    {
      return;
    }

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.DisplayActiveCasesOnly.Flag = import.DisplayActiveCasesOnly.Flag;
    export.Code.CodeName = import.Code.CodeName;
    export.Starting.Cdvalue = import.Starting.Cdvalue;
    export.CombinationCode.CodeName = import.CombinationCode.CodeName;
    export.CombinationCodeValue.Cdvalue = import.CombinationCodeValue.Cdvalue;
    export.ListCodeName.PromptField = import.ListCodeName.PromptField;
    export.ListCrossValCodeName.PromptField =
      import.ListCrossValCodeName.PromptField;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailSelection.OneChar =
        import.Import1.Item.DeatilSelection.OneChar;
      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Next();
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
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();

        return;

        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // ****
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
    if (Equal(global.Command, "RLCNAME"))
    {
    }
    else
    {
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
    }

    if (!Equal(global.Command, "RLCNAME") && !Equal(global.Command, "LIST"))
    {
      export.ListCodeName.PromptField = "+";
      export.ListCrossValCodeName.PromptField = "+";
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "LIST":
        if (AsChar(export.ListCodeName.PromptField) != '+' && AsChar
          (export.ListCodeName.PromptField) != 'S' && !
          IsEmpty(export.ListCodeName.PromptField))
        {
          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE3";

          var field = GetField(export.ListCodeName, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListCodeName.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_CODE_NAMES";

          return;
        }

        if (AsChar(export.ListCrossValCodeName.PromptField) != '+' && AsChar
          (export.ListCrossValCodeName.PromptField) != 'S' && !
          IsEmpty(export.ListCrossValCodeName.PromptField))
        {
          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE3";

          var field = GetField(export.ListCrossValCodeName, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListCrossValCodeName.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_CODE_NAMES";
        }

        break;
      case "RLCNAME":
        if (AsChar(export.ListCodeName.PromptField) == 'S')
        {
          export.ListCodeName.PromptField = "+";

          if (IsEmpty(import.Selected.CodeName))
          {
            var field = GetField(export.Code, "codeName");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.Code.CodeName = import.Selected.CodeName;

            var field = GetField(export.Starting, "cdvalue");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.ListCrossValCodeName.PromptField) == 'S')
        {
          export.ListCrossValCodeName.PromptField = "+";

          if (IsEmpty(import.Selected.CodeName))
          {
            var field = GetField(export.CombinationCode, "codeName");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.CombinationCode.CodeName = import.Selected.CodeName;

            var field = GetField(export.CombinationCodeValue, "cdvalue");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "DISPLAY":
        if (!ReadCode())
        {
          ExitState = "CO0000_INVALID_CODE";

          return;
        }

        if (IsEmpty(export.CombinationCode.CodeName))
        {
          // No combination code name, value are given. So list the values of 
          // the given code.
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCodeValue())
          {
            export.Export1.Update.Detail.Assign(entities.ExistingCodeValue);
            export.Export1.Next();
          }
        }
        else
        {
          // List only those code values which are valid with the given 
          // combination code value.
          if (!ReadCodeValueCode())
          {
            ExitState = "CO0000_INVALID_CROSS_VAL_CODE";

            return;
          }

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCodeValueCombinationCodeValue())
          {
            export.Export1.Update.Detail.Assign(entities.ExistingCodeValue);
            export.Export1.Next();
          }
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.DetailSelection, "oneChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        local.Selected.Count = 0;
        export.DlgflwMultSelectn.Index = -1;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailSelection.OneChar) && AsChar
            (export.Export1.Item.DetailSelection.OneChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailSelection, "oneChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

            return;
          }

          if (AsChar(export.Export1.Item.DetailSelection.OneChar) == 'S')
          {
            ++local.Selected.Count;

            if (AsChar(export.HiddenReturnMultRecs.Flag) == 'Y')
            {
              ++export.DlgflwMultSelectn.Index;
              export.DlgflwMultSelectn.CheckSize();

              export.DlgflwMultSelectn.Update.DetailMultSelectn.Assign(
                export.Export1.Item.Detail);
            }
            else
            {
              if (local.Selected.Count > 1)
              {
                var field =
                  GetField(export.Export1.Item.DetailSelection, "oneChar");

                field.Error = true;

                ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

                return;
              }

              export.DlgflwSelected.Assign(export.Export1.Item.Detail);
            }
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingCode.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
        db.SetString(command, "cdvalue", export.Starting.Cdvalue);
        db.SetString(command, "flag", export.DisplayActiveCasesOnly.Flag);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

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

  private bool ReadCodeValueCode()
  {
    entities.ExistingCombinationCodeValue.Populated = false;
    entities.ExistingCombinationCode.Populated = false;

    return Read("ReadCodeValueCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.CombinationCode.CodeName);
        db.SetString(command, "cdvalue", export.CombinationCodeValue.Cdvalue);
        db.SetString(command, "flag", export.DisplayActiveCasesOnly.Flag);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCombinationCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCombinationCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingCombinationCode.Id = db.GetInt32(reader, 1);
        entities.ExistingCombinationCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingCombinationCodeValue.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingCombinationCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingCombinationCodeValue.Description =
          db.GetString(reader, 5);
        entities.ExistingCombinationCode.CodeName = db.GetString(reader, 6);
        entities.ExistingCombinationCode.EffectiveDate = db.GetDate(reader, 7);
        entities.ExistingCombinationCode.ExpirationDate = db.GetDate(reader, 8);
        entities.ExistingCombinationCodeValue.Populated = true;
        entities.ExistingCombinationCode.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValueCombinationCodeValue()
  {
    return ReadEach("ReadCodeValueCombinationCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
        db.SetString(command, "cdvalue", export.Starting.Cdvalue);
        db.
          SetInt32(command, "covSId", entities.ExistingCombinationCodeValue.Id);
          
        db.SetString(command, "flag", export.DisplayActiveCasesOnly.Flag);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCodeValueCombination.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValueCombination.CovId = db.GetInt32(reader, 1);
        entities.ExistingCodeValueCombination.CovSId = db.GetInt32(reader, 2);
        entities.ExistingCodeValueCombination.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingCodeValueCombination.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingCodeValue.Id = db.GetInt32(reader, 5);
        entities.ExistingCodeValue.CodId = db.GetNullableInt32(reader, 6);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 7);
        entities.ExistingCodeValue.EffectiveDate = db.GetDate(reader, 8);
        entities.ExistingCodeValue.ExpirationDate = db.GetDate(reader, 9);
        entities.ExistingCodeValue.Description = db.GetString(reader, 10);
        entities.ExistingCodeValueCombination.Populated = true;
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
      /// A value of DeatilSelection.
      /// </summary>
      [JsonPropertyName("deatilSelection")]
      public Standard DeatilSelection
      {
        get => deatilSelection ??= new();
        set => deatilSelection = value;
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
      public const int Capacity = 100;

      private Standard deatilSelection;
      private CodeValue detail;
    }

    /// <summary>
    /// A value of HiddenReturnMultRecs.
    /// </summary>
    [JsonPropertyName("hiddenReturnMultRecs")]
    public Common HiddenReturnMultRecs
    {
      get => hiddenReturnMultRecs ??= new();
      set => hiddenReturnMultRecs = value;
    }

    /// <summary>
    /// A value of AllowChangingCodeName.
    /// </summary>
    [JsonPropertyName("allowChangingCodeName")]
    public Common AllowChangingCodeName
    {
      get => allowChangingCodeName ??= new();
      set => allowChangingCodeName = value;
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
    /// A value of ListCrossValCodeName.
    /// </summary>
    [JsonPropertyName("listCrossValCodeName")]
    public Standard ListCrossValCodeName
    {
      get => listCrossValCodeName ??= new();
      set => listCrossValCodeName = value;
    }

    /// <summary>
    /// A value of ListCodeName.
    /// </summary>
    [JsonPropertyName("listCodeName")]
    public Standard ListCodeName
    {
      get => listCodeName ??= new();
      set => listCodeName = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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
    /// A value of CombinationCodeValue.
    /// </summary>
    [JsonPropertyName("combinationCodeValue")]
    public CodeValue CombinationCodeValue
    {
      get => combinationCodeValue ??= new();
      set => combinationCodeValue = value;
    }

    /// <summary>
    /// A value of CombinationCode.
    /// </summary>
    [JsonPropertyName("combinationCode")]
    public Code CombinationCode
    {
      get => combinationCode ??= new();
      set => combinationCode = value;
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

    private Common hiddenReturnMultRecs;
    private Common allowChangingCodeName;
    private Code selected;
    private Standard listCrossValCodeName;
    private Standard listCodeName;
    private Common displayActiveCasesOnly;
    private Code code;
    private CodeValue combinationCodeValue;
    private Code combinationCode;
    private CodeValue starting;
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
    /// <summary>A DlgflwMultSelectnGroup group.</summary>
    [Serializable]
    public class DlgflwMultSelectnGroup
    {
      /// <summary>
      /// A value of DetailMultSelectn.
      /// </summary>
      [JsonPropertyName("detailMultSelectn")]
      public CodeValue DetailMultSelectn
      {
        get => detailMultSelectn ??= new();
        set => detailMultSelectn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CodeValue detailMultSelectn;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Standard DetailSelection
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
      public const int Capacity = 100;

      private Standard detailSelection;
      private CodeValue detail;
    }

    /// <summary>
    /// A value of HiddenReturnMultRecs.
    /// </summary>
    [JsonPropertyName("hiddenReturnMultRecs")]
    public Common HiddenReturnMultRecs
    {
      get => hiddenReturnMultRecs ??= new();
      set => hiddenReturnMultRecs = value;
    }

    /// <summary>
    /// A value of AllowChangingCodeName.
    /// </summary>
    [JsonPropertyName("allowChangingCodeName")]
    public Common AllowChangingCodeName
    {
      get => allowChangingCodeName ??= new();
      set => allowChangingCodeName = value;
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
    /// A value of ListCodeName.
    /// </summary>
    [JsonPropertyName("listCodeName")]
    public Standard ListCodeName
    {
      get => listCodeName ??= new();
      set => listCodeName = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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
    /// A value of CombinationCode.
    /// </summary>
    [JsonPropertyName("combinationCode")]
    public Code CombinationCode
    {
      get => combinationCode ??= new();
      set => combinationCode = value;
    }

    /// <summary>
    /// A value of CombinationCodeValue.
    /// </summary>
    [JsonPropertyName("combinationCodeValue")]
    public CodeValue CombinationCodeValue
    {
      get => combinationCodeValue ??= new();
      set => combinationCodeValue = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// Gets a value of DlgflwMultSelectn.
    /// </summary>
    [JsonIgnore]
    public Array<DlgflwMultSelectnGroup> DlgflwMultSelectn =>
      dlgflwMultSelectn ??= new(DlgflwMultSelectnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DlgflwMultSelectn for json serialization.
    /// </summary>
    [JsonPropertyName("dlgflwMultSelectn")]
    [Computed]
    public IList<DlgflwMultSelectnGroup> DlgflwMultSelectn_Json
    {
      get => dlgflwMultSelectn;
      set => DlgflwMultSelectn.Assign(value);
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

    private Common hiddenReturnMultRecs;
    private Common allowChangingCodeName;
    private Standard listCrossValCodeName;
    private Standard listCodeName;
    private Common displayActiveCasesOnly;
    private Code code;
    private Code combinationCode;
    private CodeValue combinationCodeValue;
    private CodeValue starting;
    private CodeValue dlgflwSelected;
    private Array<DlgflwMultSelectnGroup> dlgflwMultSelectn;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    private DateWorkArea current;
    private CursorPosition cursorPosition;
    private Common selected;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCodeValueCombination.
    /// </summary>
    [JsonPropertyName("existingCodeValueCombination")]
    public CodeValueCombination ExistingCodeValueCombination
    {
      get => existingCodeValueCombination ??= new();
      set => existingCodeValueCombination = value;
    }

    /// <summary>
    /// A value of ExistingCombinationCodeValue.
    /// </summary>
    [JsonPropertyName("existingCombinationCodeValue")]
    public CodeValue ExistingCombinationCodeValue
    {
      get => existingCombinationCodeValue ??= new();
      set => existingCombinationCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingCombinationCode.
    /// </summary>
    [JsonPropertyName("existingCombinationCode")]
    public Code ExistingCombinationCode
    {
      get => existingCombinationCode ??= new();
      set => existingCombinationCode = value;
    }

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

    private CodeValueCombination existingCodeValueCombination;
    private CodeValue existingCombinationCodeValue;
    private Code existingCombinationCode;
    private CodeValue existingCodeValue;
    private Code existingCode;
  }
#endregion
}
