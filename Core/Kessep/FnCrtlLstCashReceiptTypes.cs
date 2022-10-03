// Program: FN_CRTL_LST_CASH_RECEIPT_TYPES, ID: 371785151, model: 746.
// Short name: SWECRTLP
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
/// A program: FN_CRTL_LST_CASH_RECEIPT_TYPES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrtlLstCashReceiptTypes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRTL_LST_CASH_RECEIPT_TYPES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrtlLstCashReceiptTypes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrtlLstCashReceiptTypes.
  /// </summary>
  public FnCrtlLstCashReceiptTypes(IContext context, Import import,
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
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ----------------------------------------------------------------------------------------
    // Date 	Developer Name	  Description
    // 02/03/96	H. Kennedy-MTW	  Retrofits
    // 02/21/96	H. Kennedy-MTW	  Flow returning to Cash Management Menu cannot 
    // be an auto flow.  A flow also exists to the Cash Management Admin Menu.
    // A flag is passed to this screen from the Admin Menu and is evaluated when
    // PF3 is pressed to determine which menu to flow to.
    // 12/13/96        R. Marchman	  Add new security/next tran
    // 04/28/97        J. Howard         Current date fix
    // 
    // 10/27/98        S. Newman         Added successful display message, added
    // validation for selection code, highlighted multiple selection choices,
    // added logic for invalid command for ENTER and invalid PF Key for PF Keys.
    // Regenerated dialog manager for CAMM, removed escape from next tran
    // logic which prevented a display when you next tran to CRTL.  Revised
    // logic to prevent erasing of code on CREC when you go to CRTL and do not
    // make a choice.
    // 
    // 2/5/99          S. Newman         Added maint to security and added CRTM
    // PF Key to screen
    // ----------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.FromAdminMenu.Flag = import.FromAdminMenu.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Move all IMPORTs to EXPORTs.
    MoveCashReceiptType(import.Search, export.Search);
    export.ShowHistory.Flag = import.ShowHistory.Flag;

    // *****
    // Next Tran Logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // No valid values to set
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (Equal(global.Command, "EXIT") || Equal(global.Command, "ENTER") || Equal
      (global.Command, "MAINT"))
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

    // Check to see if a selection has been made.
    local.Common.Count = 0;

    // *******************************************************
    // 
    // Added validation for Select Char in group view.
    // 
    // *******************************************************
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.CashReceiptType.Assign(
        import.Import1.Item.CashReceiptType);
      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;

      switch(AsChar(import.Import1.Item.Common.SelectChar))
      {
        case 'S':
          ++local.Common.Count;
          export.HiddenSelection.Assign(import.Import1.Item.CashReceiptType);

          break;
        case ' ':
          // ******Continue Processing
          break;
        default:
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.Export1.Next();
    }

    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE"))
    {
      return;
    }

    // *******************************************************
    // 
    // Highlighted multiple selection choices.
    // *******************************************************
    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
      }

      if (IsExitState("ACO_NE0000_INVALID_MULTIPLE_SEL"))
      {
        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "MAINT":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        break;
      case "EXIT":
        // *****
        // Evaluate the value of the from Admin menu flag to determine which 
        // menu to return to.
        // *****
        if (AsChar(export.FromAdminMenu.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";
        }

        break;
      case "DISPLAY":
        // READ EACH for selection list.
        if (AsChar(import.Search.CategoryIndicator) == 'C' || AsChar
          (import.Search.CategoryIndicator) == 'N')
        {
          if (AsChar(import.ShowHistory.Flag) == 'Y')
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadCashReceiptType2())
            {
              export.Export1.Update.CashReceiptType.Assign(
                entities.CashReceiptType);
              local.CompareMaxDateDateWorkArea.Date =
                entities.CashReceiptType.DiscontinueDate;
              export.Export1.Update.CashReceiptType.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
              export.Export1.Next();
            }
          }
          else
          {
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadCashReceiptType1())
            {
              export.Export1.Update.CashReceiptType.Assign(
                entities.CashReceiptType);
              local.CompareMaxDateDateWorkArea.Date =
                entities.CashReceiptType.DiscontinueDate;
              export.Export1.Update.CashReceiptType.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
              export.Export1.Next();
            }
          }
        }
        else if (AsChar(import.ShowHistory.Flag) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptType4())
          {
            export.Export1.Update.CashReceiptType.Assign(
              entities.CashReceiptType);
            local.CompareMaxDateDateWorkArea.Date =
              entities.CashReceiptType.DiscontinueDate;
            export.Export1.Update.CashReceiptType.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptType3())
          {
            export.Export1.Update.CashReceiptType.Assign(
              entities.CashReceiptType);
            local.CompareMaxDateDateWorkArea.Date =
              entities.CashReceiptType.DiscontinueDate;
            export.Export1.Update.CashReceiptType.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            export.Export1.Next();
          }
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (Equal(global.Command, "ENTER"))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

        break;
    }

    // Add any common logic that must occur at
    // the end of every pass.
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.Code = source.Code;
    target.CategoryIndicator = source.CategoryIndicator;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.CompareMaxDateDateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private IEnumerable<bool> ReadCashReceiptType1()
  {
    return ReadEach("ReadCashReceiptType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.Search.Code);
        db.SetString(command, "categoryInd", import.Search.CategoryIndicator);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Name = db.GetString(reader, 3);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptType2()
  {
    return ReadEach("ReadCashReceiptType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.Search.Code);
        db.SetString(command, "categoryInd", import.Search.CategoryIndicator);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Name = db.GetString(reader, 3);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptType3()
  {
    return ReadEach("ReadCashReceiptType3",
      (db, command) =>
      {
        db.SetString(command, "code", import.Search.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Name = db.GetString(reader, 3);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptType4()
  {
    return ReadEach("ReadCashReceiptType4",
      (db, command) =>
      {
        db.SetString(command, "code", import.Search.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Name = db.GetString(reader, 3);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 4);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CashReceiptType.
      /// </summary>
      [JsonPropertyName("cashReceiptType")]
      public CashReceiptType CashReceiptType
      {
        get => cashReceiptType ??= new();
        set => cashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private CashReceiptType cashReceiptType;
    }

    /// <summary>
    /// A value of FromAdminMenu.
    /// </summary>
    [JsonPropertyName("fromAdminMenu")]
    public Common FromAdminMenu
    {
      get => fromAdminMenu ??= new();
      set => fromAdminMenu = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CashReceiptType Search
    {
      get => search ??= new();
      set => search = value;
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

    private Common fromAdminMenu;
    private Common showHistory;
    private CashReceiptType search;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CashReceiptType.
      /// </summary>
      [JsonPropertyName("cashReceiptType")]
      public CashReceiptType CashReceiptType
      {
        get => cashReceiptType ??= new();
        set => cashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private CashReceiptType cashReceiptType;
    }

    /// <summary>
    /// A value of FromAdminMenu.
    /// </summary>
    [JsonPropertyName("fromAdminMenu")]
    public Common FromAdminMenu
    {
      get => fromAdminMenu ??= new();
      set => fromAdminMenu = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CashReceiptType Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public CashReceiptType HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
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

    private Common fromAdminMenu;
    private Common showHistory;
    private CashReceiptType search;
    private Array<ExportGroup> export1;
    private CashReceiptType hiddenSelection;
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
    /// A value of CompareMaxDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("compareMaxDateDateWorkArea")]
    public DateWorkArea CompareMaxDateDateWorkArea
    {
      get => compareMaxDateDateWorkArea ??= new();
      set => compareMaxDateDateWorkArea = value;
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
    /// A value of CompareMaxDateCashReceiptType.
    /// </summary>
    [JsonPropertyName("compareMaxDateCashReceiptType")]
    public CashReceiptType CompareMaxDateCashReceiptType
    {
      get => compareMaxDateCashReceiptType ??= new();
      set => compareMaxDateCashReceiptType = value;
    }

    /// <summary>
    /// A value of CompareInitiationDate.
    /// </summary>
    [JsonPropertyName("compareInitiationDate")]
    public CashReceiptType CompareInitiationDate
    {
      get => compareInitiationDate ??= new();
      set => compareInitiationDate = value;
    }

    private DateWorkArea current;
    private DateWorkArea compareMaxDateDateWorkArea;
    private Common common;
    private CashReceiptType compareMaxDateCashReceiptType;
    private CashReceiptType compareInitiationDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private CashReceiptType cashReceiptType;
  }
#endregion
}
