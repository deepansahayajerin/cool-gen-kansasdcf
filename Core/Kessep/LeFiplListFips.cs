// Program: LE_FIPL_LIST_FIPS, ID: 372018362, model: 746.
// Short name: SWEFIPLP
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
/// A program: LE_FIPL_LIST_FIPS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeFiplListFips: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_FIPL_LIST_FIPS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeFiplListFips(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeFiplListFips.
  /// </summary>
  public LeFiplListFips(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date     Developer       req #    Description
    // 12/95    H. Hooks                 Initial Development
    // 01/04/97 R. Marchman              Add new security/next tran.
    // 09/25/98 R. Jean                  Increase group view size to 150,
    //                                   
    // delete set command to display
    //                                   
    // in xxfmmenu if test
    // ---------------------------------------------
    // PR134934. Increased number of occurrences of group views from 150 to 180.
    // Asl ochanged the exit state for the prev and next commands, so that the
    // user would have a remidner to change the filtervalue. LJB 12-31-2001
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.SearchFips.Assign(import.SearchFips);
    export.SearchFipsTribAddress.City = import.SearchFipsTribAddress.City;
    export.HiddenSelected.Assign(import.HiddenSelected);
    export.PromptStateCodes.PromptField = import.PromptStateCodes.PromptField;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Fips.IsEmpty)
    {
      export.Fips.Index = -1;

      for(import.Fips.Index = 0; import.Fips.Index < import.Fips.Count; ++
        import.Fips.Index)
      {
        ++export.Fips.Index;
        export.Fips.CheckSize();

        export.Fips.Update.DetailCommon.SelectChar =
          import.Fips.Item.DetailCommon.SelectChar;
        export.Fips.Update.DetailFips.Assign(import.Fips.Item.DetailFips);
      }
    }

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
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "FIPS") || Equal(global.Command, "TRIB") || Equal
      (global.Command, "ORGZ") || Equal(global.Command, "LIST") || Equal
      (global.Command, "RETCDVL"))
    {
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
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

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "LIST":
        switch(AsChar(export.PromptStateCodes.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.DlgflwRequired.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.PromptStateCodes, "promptField");

            field1.Error = true;

            return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        var field = GetField(export.PromptStateCodes, "promptField");

        field.Error = true;

        return;
      case "RETCDVL":
        if (AsChar(export.PromptStateCodes.PromptField) == 'S')
        {
          export.PromptStateCodes.PromptField = "";

          if (!IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            export.SearchFips.StateAbbreviation = import.DlgflwSelected.Cdvalue;

            var field1 = GetField(export.SearchFips, "countyAbbreviation");

            field1.Protected = false;
            field1.Focused = true;
          }
          else
          {
            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Protected = false;
            field1.Focused = true;
          }
        }

        break;
      case "DISPLAY":
        if (!IsEmpty(export.SearchFips.StateAbbreviation))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = export.SearchFips.StateAbbreviation;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Error = true;

            return;
          }

          if (!ReadFips2())
          {
            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Error = true;

            ExitState = "LE0000_NO_FIPS_FOUND_FOR_STATE";

            return;
          }
        }

        if (!IsEmpty(export.SearchFips.CountyAbbreviation))
        {
          if (IsEmpty(export.SearchFips.StateAbbreviation))
          {
            ExitState = "LE0000_STATE_MUST_BE_ENTERED";

            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Error = true;

            return;
          }

          if (!ReadFips1())
          {
            var field1 = GetField(export.SearchFips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.SearchFips, "countyAbbreviation");

            field2.Error = true;

            ExitState = "LE0000_NO_FIPS_FOUND_FOR_COUNTY";

            return;
          }
        }

        local.GroupEntryNo.Count = 0;
        export.Fips.Index = -1;

        foreach(var item in ReadFips3())
        {
          if (!IsEmpty(export.SearchFips.StateAbbreviation))
          {
            if (!Equal(entities.Existing.StateAbbreviation,
              export.SearchFips.StateAbbreviation))
            {
              continue;
            }
          }

          if (!IsEmpty(export.SearchFips.CountyAbbreviation))
          {
            if (!Equal(entities.Existing.CountyAbbreviation,
              export.SearchFips.CountyAbbreviation))
            {
              continue;
            }
          }

          if (!IsEmpty(export.SearchFipsTribAddress.City))
          {
            foreach(var item1 in ReadFipsTribAddress2())
            {
              if (!Lt(entities.FipsTribAddress.City,
                export.SearchFipsTribAddress.City))
              {
                goto Test;
              }
            }

            // --- City not found/ not the one looking for
            continue;
          }
          else
          {
            ReadFipsTribAddress1();
          }

Test:

          if (export.Fips.Index + 1 >= Export.FipsGroup.Capacity)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            break;
          }

          ++export.Fips.Index;
          export.Fips.CheckSize();

          export.Fips.Update.DetailFips.Assign(entities.Existing);

          if (entities.FipsTribAddress.Populated)
          {
            export.Fips.Update.DetailFipsTribAddress.City =
              entities.FipsTribAddress.City;
          }
        }

        if (export.Fips.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else if (export.Fips.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "FIPS":
        ExitState = "ECO_LNK_TO_FIPS";
        global.Command = "RETURN";

        break;
      case "TRIB":
        ExitState = "ECO_LNK_TO_TRIB";
        global.Command = "RETURN";

        break;
      case "ORGZ":
        ExitState = "ECO_LNK_TO_ORGZ";
        global.Command = "RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "RETURN"))
    {
      local.NumberOfSelections.Count = 0;

      for(export.Fips.Index = 0; export.Fips.Index < export.Fips.Count; ++
        export.Fips.Index)
      {
        if (!export.Fips.CheckSize())
        {
          break;
        }

        if (AsChar(export.Fips.Item.DetailCommon.SelectChar) == 'S' || AsChar
          (export.Fips.Item.DetailCommon.SelectChar) == 's')
        {
          ++local.NumberOfSelections.Count;
          export.HiddenSelected.Assign(export.Fips.Item.DetailFips);
        }
        else if (!IsEmpty(export.Fips.Item.DetailCommon.SelectChar))
        {
          var field = GetField(export.Fips.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }
      }

      export.Fips.CheckIndex();

      if (local.NumberOfSelections.Count > 1)
      {
        for(export.Fips.Index = 0; export.Fips.Index < export.Fips.Count; ++
          export.Fips.Index)
        {
          if (!export.Fips.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Fips.Item.DetailCommon.SelectChar))
          {
            var field = GetField(export.Fips.Item.DetailCommon, "selectChar");

            field.Error = true;
          }
        }

        export.Fips.CheckIndex();
        ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";
      }
    }
    else
    {
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private bool ReadFips1()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.SearchFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.SearchFips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.StateAbbreviation = db.GetString(reader, 6);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.SearchFips.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.StateAbbreviation = db.GetString(reader, 6);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Existing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFips3()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.SearchFips.State);
        db.SetInt32(command, "county", export.SearchFips.County);
        db.SetInt32(command, "location", export.SearchFips.Location);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.StateAbbreviation = db.GetString(reader, 6);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.City = db.GetString(reader, 2);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.City = db.GetString(reader, 2);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;

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
    /// <summary>A FipsGroup group.</summary>
    [Serializable]
    public class FipsGroup
    {
      /// <summary>
      /// A value of DetailFipsTribAddress.
      /// </summary>
      [JsonPropertyName("detailFipsTribAddress")]
      public FipsTribAddress DetailFipsTribAddress
      {
        get => detailFipsTribAddress ??= new();
        set => detailFipsTribAddress = value;
      }

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
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 168;

      private FipsTribAddress detailFipsTribAddress;
      private Common detailCommon;
      private Fips detailFips;
    }

    /// <summary>
    /// A value of PromptStateCodes.
    /// </summary>
    [JsonPropertyName("promptStateCodes")]
    public Standard PromptStateCodes
    {
      get => promptStateCodes ??= new();
      set => promptStateCodes = value;
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
    /// A value of SearchFipsTribAddress.
    /// </summary>
    [JsonPropertyName("searchFipsTribAddress")]
    public FipsTribAddress SearchFipsTribAddress
    {
      get => searchFipsTribAddress ??= new();
      set => searchFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Fips HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// Gets a value of Fips.
    /// </summary>
    [JsonIgnore]
    public Array<FipsGroup> Fips => fips ??= new(FipsGroup.Capacity);

    /// <summary>
    /// Gets a value of Fips for json serialization.
    /// </summary>
    [JsonPropertyName("fips")]
    [Computed]
    public IList<FipsGroup> Fips_Json
    {
      get => fips;
      set => Fips.Assign(value);
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

    private Standard promptStateCodes;
    private CodeValue dlgflwSelected;
    private FipsTribAddress searchFipsTribAddress;
    private Fips searchFips;
    private Fips hiddenSelected;
    private Array<FipsGroup> fips;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FipsGroup group.</summary>
    [Serializable]
    public class FipsGroup
    {
      /// <summary>
      /// A value of DetailFipsTribAddress.
      /// </summary>
      [JsonPropertyName("detailFipsTribAddress")]
      public FipsTribAddress DetailFipsTribAddress
      {
        get => detailFipsTribAddress ??= new();
        set => detailFipsTribAddress = value;
      }

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
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 168;

      private FipsTribAddress detailFipsTribAddress;
      private Common detailCommon;
      private Fips detailFips;
    }

    /// <summary>
    /// A value of PromptStateCodes.
    /// </summary>
    [JsonPropertyName("promptStateCodes")]
    public Standard PromptStateCodes
    {
      get => promptStateCodes ??= new();
      set => promptStateCodes = value;
    }

    /// <summary>
    /// A value of DlgflwRequired.
    /// </summary>
    [JsonPropertyName("dlgflwRequired")]
    public Code DlgflwRequired
    {
      get => dlgflwRequired ??= new();
      set => dlgflwRequired = value;
    }

    /// <summary>
    /// A value of SearchFipsTribAddress.
    /// </summary>
    [JsonPropertyName("searchFipsTribAddress")]
    public FipsTribAddress SearchFipsTribAddress
    {
      get => searchFipsTribAddress ??= new();
      set => searchFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Fips HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// Gets a value of Fips.
    /// </summary>
    [JsonIgnore]
    public Array<FipsGroup> Fips => fips ??= new(FipsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Fips for json serialization.
    /// </summary>
    [JsonPropertyName("fips")]
    [Computed]
    public IList<FipsGroup> Fips_Json
    {
      get => fips;
      set => Fips.Assign(value);
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

    private Standard promptStateCodes;
    private Code dlgflwRequired;
    private FipsTribAddress searchFipsTribAddress;
    private Fips searchFips;
    private Fips hiddenSelected;
    private Array<FipsGroup> fips;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of NumberOfSelections.
    /// </summary>
    [JsonPropertyName("numberOfSelections")]
    public Common NumberOfSelections
    {
      get => numberOfSelections ??= new();
      set => numberOfSelections = value;
    }

    /// <summary>
    /// A value of GroupEntryNo.
    /// </summary>
    [JsonPropertyName("groupEntryNo")]
    public Common GroupEntryNo
    {
      get => groupEntryNo ??= new();
      set => groupEntryNo = value;
    }

    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common numberOfSelections;
    private Common groupEntryNo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Fips existing;
  }
#endregion
}
