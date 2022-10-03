// Program: SP_OFCL_LIST_OFFICE, ID: 371782751, model: 746.
// Short name: SWEOFCLP
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
/// A program: SP_OFCL_LIST_OFFICE.
/// </para>
/// <para>
/// This procedure is used to add, update, delete, and select CSE offices.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOfclListOffice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_OFCL_LIST_OFFICE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOfclListOffice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOfclListOffice.
  /// </summary>
  public SpOfclListOffice(IContext context, Import import, Export export):
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
    // ** DATE      *  DESCRIPTION
    // ** 05/10/95  a. hackler     initial
    // ** 02/06/96  a. hackler     retro fits
    // ** 04/17/96  J. Rookard     Change office "Type" selection criteria from
    // "equal to or greater than" to "equal to" per discussion with Linda
    // DeMoss, user/tester, on 4/17/96.
    // ** 01/03/97  R. Marchman    Add new security/next tran.
    // ** 08/13/13  GVandy         (CQ38147) Add Show All filter and default to
    // "N" which
    //                             eliminates offices that are not active from 
    // display.
    // *****************************************
    // **
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    MoveOffice(import.StartingOffice, export.StartingOffice);
    export.StartingOfficeAddress.City = import.StartingOfficeAddress.City;
    export.SortBy.SelectChar = import.SortBy.SelectChar;
    export.PromptOfficeType.Flag = import.PromptOfficeType.Flag;
    export.OfficeTypeDesc.Description = import.OfficeTypeDesc.Description;
    export.ShowAll.Text1 = import.ShowAll.Text1;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.Office.Assign(import.Group.Item.Office);
      export.Group.Update.OfficeAddress.City =
        import.Group.Item.OfficeAddress.City;

      if (!IsEmpty(export.Group.Item.Common.SelectChar))
      {
        ++local.Count.Count;
        export.HiddenSelected.Assign(export.Group.Item.Office);
      }

      export.Group.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }
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
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RLCVAL"))
    {
      if (!IsEmpty(import.HiddenFromList.Cdvalue))
      {
        export.StartingOffice.TypeCode = import.HiddenFromList.Cdvalue;
        export.OfficeTypeDesc.Description = import.HiddenFromList.Description;
      }

      export.PromptOfficeType.Flag = "";

      var field = GetField(export.PromptOfficeType, "flag");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }
    else
    {
      // to validate action level security
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

        break;
      case "RLCVAL":
        break;
      case "DISPLAY":
        export.PromptOfficeType.Flag = "";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.Group.Update.Common.SelectChar = "";
        }

        if (IsEmpty(export.SortBy.SelectChar))
        {
          export.SortBy.SelectChar = "N";
        }

        if (IsEmpty(export.StartingOffice.TypeCode))
        {
          export.OfficeTypeDesc.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        switch(AsChar(export.ShowAll.Text1))
        {
          case 'Y':
            break;
          case 'N':
            break;
          case ' ':
            export.ShowAll.Text1 = "N";

            break;
          default:
            var field = GetField(export.ShowAll, "text1");

            field.Error = true;

            ExitState = "INVALID_INDICATOR_Y_N_SPACE";

            return;
        }

        local.Code.CodeName = "OFFICE TYPE";
        local.CodeValue.Cdvalue = export.StartingOffice.TypeCode;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          export.OfficeTypeDesc.Description =
            Spaces(CodeValue.Description_MaxLength);
        }

        if (AsChar(export.SortBy.SelectChar) == 'N' || AsChar
          (export.SortBy.SelectChar) == 'A' || AsChar
          (export.SortBy.SelectChar) == 'C' || AsChar
          (export.SortBy.SelectChar) == 'T')
        {
        }
        else
        {
          var field = GetField(export.SortBy, "selectChar");

          field.Error = true;

          ExitState = "INVALID_SORT_CHR_MUSTBE_N_T_A_C";

          return;
        }

        if (AsChar(export.SortBy.SelectChar) == 'N')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadOfficeOfficeAddress3())
          {
            if (AsChar(export.ShowAll.Text1) == 'N' && (
              Lt(entities.Office.DiscontinueDate, Now().Date) || Lt
              (Now().Date, entities.Office.EffectiveDate)))
            {
              export.Group.Next();

              continue;
            }

            if (IsEmpty(export.StartingOffice.TypeCode))
            {
            }
            else if (AsChar(entities.Office.TypeCode) == AsChar
              (export.StartingOffice.TypeCode))
            {
            }
            else
            {
              export.Group.Next();

              continue;
            }

            if (entities.Office.SystemGeneratedId == local
              .Prev.SystemGeneratedId)
            {
              export.Group.Next();

              continue;
            }
            else
            {
              local.Prev.SystemGeneratedId = entities.Office.SystemGeneratedId;
            }

            export.Group.Update.Office.Assign(entities.Office);
            export.Group.Update.OfficeAddress.City =
              entities.OfficeAddress.City;
            export.Group.Next();
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'T')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadOfficeOfficeAddress4())
          {
            if (AsChar(export.ShowAll.Text1) == 'N' && (
              Lt(entities.Office.DiscontinueDate, Now().Date) || Lt
              (Now().Date, entities.Office.EffectiveDate)))
            {
              export.Group.Next();

              continue;
            }

            if (IsEmpty(export.StartingOffice.TypeCode))
            {
            }
            else if (AsChar(entities.Office.TypeCode) == AsChar
              (export.StartingOffice.TypeCode))
            {
            }
            else
            {
              export.Group.Next();

              continue;
            }

            if (entities.Office.SystemGeneratedId == local
              .Prev.SystemGeneratedId)
            {
              export.Group.Next();

              continue;
            }
            else
            {
              local.Prev.SystemGeneratedId = entities.Office.SystemGeneratedId;
            }

            export.Group.Update.Office.Assign(entities.Office);
            export.Group.Update.OfficeAddress.City =
              entities.OfficeAddress.City;
            export.Group.Next();
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'A')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadOfficeOfficeAddress2())
          {
            if (AsChar(export.ShowAll.Text1) == 'N' && (
              Lt(entities.Office.DiscontinueDate, Now().Date) || Lt
              (Now().Date, entities.Office.EffectiveDate)))
            {
              export.Group.Next();

              continue;
            }

            if (IsEmpty(export.StartingOffice.TypeCode))
            {
            }
            else if (AsChar(entities.Office.TypeCode) == AsChar
              (export.StartingOffice.TypeCode))
            {
            }
            else
            {
              export.Group.Next();

              continue;
            }

            if (entities.Office.SystemGeneratedId == local
              .Prev.SystemGeneratedId)
            {
              export.Group.Next();

              continue;
            }
            else
            {
              local.Prev.SystemGeneratedId = entities.Office.SystemGeneratedId;
            }

            export.Group.Update.Office.Assign(entities.Office);
            export.Group.Update.OfficeAddress.City =
              entities.OfficeAddress.City;
            export.Group.Next();
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'C')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadOfficeOfficeAddress1())
          {
            if (AsChar(export.ShowAll.Text1) == 'N' && (
              Lt(entities.Office.DiscontinueDate, Now().Date) || Lt
              (Now().Date, entities.Office.EffectiveDate)))
            {
              export.Group.Next();

              continue;
            }

            if (IsEmpty(export.StartingOffice.TypeCode))
            {
            }
            else if (AsChar(entities.Office.TypeCode) == AsChar
              (export.StartingOffice.TypeCode))
            {
            }
            else
            {
              export.Group.Next();

              continue;
            }

            if (entities.Office.SystemGeneratedId == local
              .Prev.SystemGeneratedId)
            {
              export.Group.Next();

              continue;
            }
            else
            {
              local.Prev.SystemGeneratedId = entities.Office.SystemGeneratedId;
            }

            export.Group.Update.Office.Assign(entities.Office);
            export.Group.Update.OfficeAddress.City =
              entities.OfficeAddress.City;
            export.Group.Next();
          }
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        if (AsChar(export.PromptOfficeType.Flag) == 'S')
        {
          export.HiddenToCodeTableList.CodeName = "OFFICE TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "RETURN":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          switch(local.Count.Count)
          {
            case 0:
              if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Color = "red";
                  field.Protected = false;
                  field.Focused = true;

                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                  return;
                }
              }

              ExitState = "ACO_NE0000_RETURN";

              break;
            case 1:
              if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
              {
                ExitState = "ACO_NE0000_RETURN";
              }
              else if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                return;
              }

              break;
            default:
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!IsEmpty(export.Group.Item.Common.SelectChar))
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Color = "red";
                  field.Protected = false;
                  field.Focused = true;

                  ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                  return;
                }
              }

              break;
          }
        }

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    local.ReturnCode.Count = useExport.ReturnCode.Count;
    export.OfficeTypeDesc.Description = useExport.CodeValue.Description;
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

  private IEnumerable<bool> ReadOfficeOfficeAddress1()
  {
    return ReadEach("ReadOfficeOfficeAddress1",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingOffice.Name);
        db.SetString(command, "city", import.StartingOfficeAddress.City);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.TypeCode = db.GetString(reader, 2);
        entities.Office.Name = db.GetString(reader, 3);
        entities.Office.EffectiveDate = db.GetDate(reader, 4);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.OfficeAddress.Type1 = db.GetString(reader, 8);
        entities.OfficeAddress.City = db.GetString(reader, 9);
        entities.OfficeAddress.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeAddress2()
  {
    return ReadEach("ReadOfficeOfficeAddress2",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingOffice.Name);
        db.SetString(command, "city", import.StartingOfficeAddress.City);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.TypeCode = db.GetString(reader, 2);
        entities.Office.Name = db.GetString(reader, 3);
        entities.Office.EffectiveDate = db.GetDate(reader, 4);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.OfficeAddress.Type1 = db.GetString(reader, 8);
        entities.OfficeAddress.City = db.GetString(reader, 9);
        entities.OfficeAddress.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeAddress3()
  {
    return ReadEach("ReadOfficeOfficeAddress3",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingOffice.Name);
        db.SetString(command, "city", import.StartingOfficeAddress.City);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.TypeCode = db.GetString(reader, 2);
        entities.Office.Name = db.GetString(reader, 3);
        entities.Office.EffectiveDate = db.GetDate(reader, 4);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.OfficeAddress.Type1 = db.GetString(reader, 8);
        entities.OfficeAddress.City = db.GetString(reader, 9);
        entities.OfficeAddress.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeAddress4()
  {
    return ReadEach("ReadOfficeOfficeAddress4",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingOffice.Name);
        db.SetString(command, "city", import.StartingOfficeAddress.City);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.TypeCode = db.GetString(reader, 2);
        entities.Office.Name = db.GetString(reader, 3);
        entities.Office.EffectiveDate = db.GetDate(reader, 4);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.OfficeAddress.Type1 = db.GetString(reader, 8);
        entities.OfficeAddress.City = db.GetString(reader, 9);
        entities.OfficeAddress.Populated = true;
        entities.Office.Populated = true;

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
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
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
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 165;

      private OfficeAddress officeAddress;
      private Common common;
      private Office office;
    }

    /// <summary>
    /// A value of OfficeTypeDesc.
    /// </summary>
    [JsonPropertyName("officeTypeDesc")]
    public CodeValue OfficeTypeDesc
    {
      get => officeTypeDesc ??= new();
      set => officeTypeDesc = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of StartingOfficeAddress.
    /// </summary>
    [JsonPropertyName("startingOfficeAddress")]
    public OfficeAddress StartingOfficeAddress
    {
      get => startingOfficeAddress ??= new();
      set => startingOfficeAddress = value;
    }

    /// <summary>
    /// A value of StartingOffice.
    /// </summary>
    [JsonPropertyName("startingOffice")]
    public Office StartingOffice
    {
      get => startingOffice ??= new();
      set => startingOffice = value;
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
    /// A value of PromptOfficeType.
    /// </summary>
    [JsonPropertyName("promptOfficeType")]
    public Common PromptOfficeType
    {
      get => promptOfficeType ??= new();
      set => promptOfficeType = value;
    }

    /// <summary>
    /// A value of HiddenFromList.
    /// </summary>
    [JsonPropertyName("hiddenFromList")]
    public CodeValue HiddenFromList
    {
      get => hiddenFromList ??= new();
      set => hiddenFromList = value;
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
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public TextWorkArea ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    private CodeValue officeTypeDesc;
    private Common sortBy;
    private OfficeAddress startingOfficeAddress;
    private Office startingOffice;
    private Array<GroupGroup> group;
    private Common promptOfficeType;
    private CodeValue hiddenFromList;
    private NextTranInfo hidden;
    private Standard standard;
    private TextWorkArea showAll;
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
      /// A value of OfficeAddress.
      /// </summary>
      [JsonPropertyName("officeAddress")]
      public OfficeAddress OfficeAddress
      {
        get => officeAddress ??= new();
        set => officeAddress = value;
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
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 165;

      private OfficeAddress officeAddress;
      private Common common;
      private Office office;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of HiddenListCancel.
    /// </summary>
    [JsonPropertyName("hiddenListCancel")]
    public Common HiddenListCancel
    {
      get => hiddenListCancel ??= new();
      set => hiddenListCancel = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Office HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of StartingOfficeAddress.
    /// </summary>
    [JsonPropertyName("startingOfficeAddress")]
    public OfficeAddress StartingOfficeAddress
    {
      get => startingOfficeAddress ??= new();
      set => startingOfficeAddress = value;
    }

    /// <summary>
    /// A value of StartingOffice.
    /// </summary>
    [JsonPropertyName("startingOffice")]
    public Office StartingOffice
    {
      get => startingOffice ??= new();
      set => startingOffice = value;
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
    /// A value of PromptOfficeType.
    /// </summary>
    [JsonPropertyName("promptOfficeType")]
    public Common PromptOfficeType
    {
      get => promptOfficeType ??= new();
      set => promptOfficeType = value;
    }

    /// <summary>
    /// A value of HiddenToCodeTableList.
    /// </summary>
    [JsonPropertyName("hiddenToCodeTableList")]
    public Code HiddenToCodeTableList
    {
      get => hiddenToCodeTableList ??= new();
      set => hiddenToCodeTableList = value;
    }

    /// <summary>
    /// A value of OfficeTypeDesc.
    /// </summary>
    [JsonPropertyName("officeTypeDesc")]
    public CodeValue OfficeTypeDesc
    {
      get => officeTypeDesc ??= new();
      set => officeTypeDesc = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public TextWorkArea ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    private Common sortBy;
    private Common hiddenListCancel;
    private Office hiddenSelected;
    private OfficeAddress startingOfficeAddress;
    private Office startingOffice;
    private Array<GroupGroup> group;
    private Common promptOfficeType;
    private Code hiddenToCodeTableList;
    private CodeValue officeTypeDesc;
    private Office office;
    private NextTranInfo hidden;
    private Standard standard;
    private TextWorkArea showAll;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Office Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private Office prev;
    private Common count;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common returnCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private OfficeAddress officeAddress;
    private Office office;
  }
#endregion
}
