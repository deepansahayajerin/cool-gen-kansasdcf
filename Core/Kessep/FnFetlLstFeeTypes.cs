// Program: FN_FETL_LST_FEE_TYPES, ID: 371873739, model: 746.
// Short name: SWEFETLP
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
/// A program: FN_FETL_LST_FEE_TYPES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnFetlLstFeeTypes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_FETL_LST_FEE_TYPES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnFetlLstFeeTypes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnFetlLstFeeTypes.
  /// </summary>
  public FnFetlLstFeeTypes(IContext context, Import import, Export export):
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
    // Date 	  	Developer   Request    Description
    // 01/96	   H. Kennedy    Source
    // 02/19/96   H. Kennedy    Retrofits
    // 02/21/96   H. Kennedy    Flow returning to Cash Management Menu cannot be
    // an auto flow.  A flow also exists to the Cash Management Admin Menu.  A
    // flag is passed to this screen from the Admin Menu and is evaluated when
    // PF3 is pressed to determine which menu to flow to.
    // 12/13/96   R. Marchman   Add new security/next tran
    // 03/31/97   J Caillouet   Display and flow to Maint Scrn.
    // 03/26/98   S. Konkader   ZDEL cleanup
    // 10/02/98   S. Newman     Removed an escape from xxnextxx so that a 
    // display can occur when you use the next tran to FETL
    // 
    // 10/02/98   S. Newman     Replaced the NEXT and PREV command exit states
    // to standard ones and added exit state to FETM command.  Also added an
    // exit state to give a message when the user tries to link to FETM.
    // 
    // 10/5/98   S. Newman     Added Successful Display exit state.
    // 
    // 10/7/98   S. Newman     Rewrote read statement to include both Y and N
    // History Display flags.
    // 
    // 2/5/99   S. Newman      Added maintain to security, made enter an invalid
    // command, changed screen literal to FETM
    // 
    // -------------------------------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.FlowToAdminMenu.Flag = import.FlowToAdminMenu.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ******************************
    // Move all IMPORTs to EXPORTs.
    // ******************************
    export.Search.Code = import.Search.Code;
    export.DisplayHistory.Flag = import.DisplayHistory.Flag;

    if (IsEmpty(import.DisplayHistory.Flag))
    {
      export.DisplayHistory.Flag = "N";
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

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailCashReceiptDetailFeeType.Assign(
        import.Import1.Item.DetailCashReceiptDetailFeeType);

      if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == 's' || AsChar
        (import.Import1.Item.DetailCommon.SelectChar) == 'S')
      {
        MoveCashReceiptDetailFeeType(import.Import1.Item.
          DetailCashReceiptDetailFeeType, export.HiddenSelection);
        MoveCashReceiptDetailFeeType(import.Import1.Item.
          DetailCashReceiptDetailFeeType, export.Search);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.DetailCommon.SelectChar))
      {
        // Do Nothing
      }
      else
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        var field = GetField(export.Common, "selectChar");

        field.Error = true;
      }

      if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        export.Export1.Next();

        return;
      }

      export.Export1.Next();
    }

    // *****
    // Next Tran/Security logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

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

    // *******************************************************************
    // Removed the escape after COMMAND IS display in the IF COMMAND IS
    // EQUAL TO xxnextxx logic.  This escape prevented a display when the
    // next tran to FETL is used.
    // *******************************************************************
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

    // *******************************************************************
    // To validate action level security, an escape is used.  An exit
    // state was added on 10/2/98 to give an error message that the user
    // cannot use this command.
    // 
    // *******************************************************************
    if (Equal(global.Command, "FETM") || Equal(global.Command, "ENTER"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ********************
    // Main Case of COMMAND
    // ********************
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "EXIT":
        // *******************************************************************
        // Evaluate the Admin Menu flag to determine which menu to return to.
        // *******************************************************************
        if (AsChar(export.FlowToAdminMenu.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";
        }

        break;
      case "DISPLAY":
        if (AsChar(export.DisplayHistory.Flag) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailFeeType2())
          {
            export.Export1.Update.DetailCashReceiptDetailFeeType.Assign(
              entities.CashReceiptDetailFeeType);

            // *******************************************************************
            // If the discontinue date contains the maximum date, then display 
            // blanks instead.
            // *******************************************************************
            local.Max.Date = entities.CashReceiptDetailFeeType.DiscontinueDate;
            export.Export1.Update.DetailCashReceiptDetailFeeType.
              DiscontinueDate = UseCabSetMaximumDiscontinueDate();
            export.Export1.Next();
          }

          // *******************************************************************
          // If the group view is empty inform the user of bad selection
          // criteria
          // *******************************************************************
          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            var field = GetField(export.Search, "code");

            field.Error = true;

            return;
          }

          // *******************************************************************
          // If the group view is full inform the user that more data exists.
          // *******************************************************************
          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          // *******************************************************************
          // Only display Active records unless history records are requested.
          // *******************************************************************
        }
        else if (AsChar(export.DisplayHistory.Flag) == 'N')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailFeeType1())
          {
            export.Export1.Update.DetailCashReceiptDetailFeeType.Assign(
              entities.CashReceiptDetailFeeType);
            local.Max.Date = entities.CashReceiptDetailFeeType.DiscontinueDate;
            export.Export1.Update.DetailCashReceiptDetailFeeType.
              DiscontinueDate = UseCabSetMaximumDiscontinueDate();
            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            var field = GetField(export.Search, "code");

            field.Error = true;

            return;
          }

          // *******************************************************************
          // If the group view is full inform the user that more data exists.
          // *******************************************************************
          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RETURN":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.Common.Count <= 1)
        {
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        break;
      case "FETM":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

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
  }

  private static void MoveCashReceiptDetailFeeType(
    CashReceiptDetailFeeType source, CashReceiptDetailFeeType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
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

    useImport.DateWorkArea.Date = local.Max.Date;

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

  private IEnumerable<bool> ReadCashReceiptDetailFeeType1()
  {
    return ReadEach("ReadCashReceiptDetailFeeType1",
      (db, command) =>
      {
        db.SetString(command, "code", export.Search.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailFeeType.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailFeeType.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailFeeType.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailFeeType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailFeeType.Description =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetailFeeType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailFeeType2()
  {
    return ReadEach("ReadCashReceiptDetailFeeType2",
      (db, command) =>
      {
        db.SetString(command, "code", export.Search.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailFeeType.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailFeeType.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailFeeType.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailFeeType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailFeeType.Description =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetailFeeType.Populated = true;

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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailFeeType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailFeeType")]
      public CashReceiptDetailFeeType DetailCashReceiptDetailFeeType
      {
        get => detailCashReceiptDetailFeeType ??= new();
        set => detailCashReceiptDetailFeeType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 196;

      private Common detailCommon;
      private CashReceiptDetailFeeType detailCashReceiptDetailFeeType;
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
    /// A value of FlowToAdminMenu.
    /// </summary>
    [JsonPropertyName("flowToAdminMenu")]
    public Common FlowToAdminMenu
    {
      get => flowToAdminMenu ??= new();
      set => flowToAdminMenu = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CashReceiptDetailFeeType Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of DisplayHistory.
    /// </summary>
    [JsonPropertyName("displayHistory")]
    public Common DisplayHistory
    {
      get => displayHistory ??= new();
      set => displayHistory = value;
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

    private Common common;
    private Common flowToAdminMenu;
    private CashReceiptDetailFeeType search;
    private Common displayHistory;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailFeeType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailFeeType")]
      public CashReceiptDetailFeeType DetailCashReceiptDetailFeeType
      {
        get => detailCashReceiptDetailFeeType ??= new();
        set => detailCashReceiptDetailFeeType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 196;

      private Common detailCommon;
      private CashReceiptDetailFeeType detailCashReceiptDetailFeeType;
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
    /// A value of FlowToAdminMenu.
    /// </summary>
    [JsonPropertyName("flowToAdminMenu")]
    public Common FlowToAdminMenu
    {
      get => flowToAdminMenu ??= new();
      set => flowToAdminMenu = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CashReceiptDetailFeeType Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of DisplayHistory.
    /// </summary>
    [JsonPropertyName("displayHistory")]
    public Common DisplayHistory
    {
      get => displayHistory ??= new();
      set => displayHistory = value;
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public CashReceiptDetailFeeType HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
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

    private Common common;
    private Common flowToAdminMenu;
    private CashReceiptDetailFeeType search;
    private Common displayHistory;
    private CashReceiptDetailFeeType hiddenSelection;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CashReceiptDetailFeeType Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private DateWorkArea current;
    private CashReceiptDetailFeeType null1;
    private DateWorkArea max;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType CashReceiptDetailFeeType
    {
      get => cashReceiptDetailFeeType ??= new();
      set => cashReceiptDetailFeeType = value;
    }

    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
  }
#endregion
}
