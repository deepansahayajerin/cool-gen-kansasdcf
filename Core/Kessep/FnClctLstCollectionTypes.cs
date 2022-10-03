// Program: FN_CLCT_LST_COLLECTION_TYPES, ID: 372256420, model: 746.
// Short name: SWECLCTP
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
/// A program: FN_CLCT_LST_COLLECTION_TYPES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnClctLstCollectionTypes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CLCT_LST_COLLECTION_TYPES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnClctLstCollectionTypes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnClctLstCollectionTypes.
  /// </summary>
  public FnClctLstCollectionTypes(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    // Date 	 Developer Name		Description
    // 02/19/96 Holly Kennedy-MTW 	Retrofits
    // 02/21/96 Holly Kennedy-MTW      Flow returning to Cash Management Menu 
    // cannot be an auto flow.  A flow also exists to the Cash Management Admin
    // Menu.  A flag is passed to this screen from the Admin Menu and is
    // evaluated when PF3 is pressed to determine which menu to flow to.
    // 12/13/96 R. Marchman		Add new security/next tran
    // 04/28/97 G P KIM                CHANGE CURRENT DATE
    // 02/16/99 N.Engoor               Added code to avoid multiple selection.
    // ----------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.FromAdminMenu.Flag = import.FromAdminMenu.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Move all IMPORTs to EXPORTs.
    MoveCommon(import.ShowHistory, export.ShowHistory);
    export.FromAdminMenu.Flag = "N";

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.CollectionType.Assign(
        import.Import1.Item.CollectionType);
      export.Export1.Next();
    }

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

    // Check to see if a selection has been made.
    local.Common.Count = 0;
    local.Error.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
      {
        ++local.Common.Count;
        export.HiddenSelection.Assign(export.Export1.Item.CollectionType);
      }

      if (!IsEmpty(export.Export1.Item.Common.SelectChar) && AsChar
        (export.Export1.Item.Common.SelectChar) != 'S')
      {
        ++local.Error.Count;
      }
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (local.Common.Count > 1)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (local.Error.Count > 0)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****
    // To validate action level security.
    // *****
    if (Equal(global.Command, "DISPLAY"))
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

    switch(AsChar(export.ShowHistory.SelectChar))
    {
      case 'Y':
        break;
      case 'N':
        break;
      case ' ':
        export.ShowHistory.SelectChar = "N";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

        var field = GetField(export.ShowHistory, "selectChar");

        field.Error = true;

        return;
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "MAINTAIN":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        break;
      case "DISPLAY":
        // READ EACH for selection list.
        if (AsChar(import.ShowHistory.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCollectionType2())
          {
            export.Export1.Update.CollectionType.
              Assign(entities.CollectionType);
            local.DateWorkArea.Date = entities.CollectionType.DiscontinueDate;
            export.Export1.Update.CollectionType.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCollectionType1())
          {
            export.Export1.Update.CollectionType.
              Assign(entities.CollectionType);
            local.DateWorkArea.Date = entities.CollectionType.DiscontinueDate;
            export.Export1.Update.CollectionType.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
            export.Export1.Next();
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "EXIT":
        // ************************************************
        // *Evaluate from admin menu flag to determine to *
        // *which menu to flow.                           *
        // ************************************************
        if (AsChar(export.FromAdminMenu.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (IsEmpty(global.Command))
        {
          return;
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // Add any common logic that must occur at
    // the end of every pass.
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
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

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

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

  private IEnumerable<bool> ReadCollectionType1()
  {
    return ReadEach("ReadCollectionType1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 3);
        entities.CollectionType.DisbursementInd = db.GetString(reader, 4);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 5);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.CollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CollectionType.CashNonCashInd);
        CheckValid<CollectionType>("DisbursementInd",
          entities.CollectionType.DisbursementInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionType2()
  {
    return ReadEach("ReadCollectionType2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 3);
        entities.CollectionType.DisbursementInd = db.GetString(reader, 4);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 5);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.CollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CollectionType.CashNonCashInd);
        CheckValid<CollectionType>("DisbursementInd",
          entities.CollectionType.DisbursementInd);

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
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 98;

      private Common common;
      private CollectionType collectionType;
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
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 98;

      private Common common;
      private CollectionType collectionType;
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
    public CollectionType HiddenSelection
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
    private Array<ExportGroup> export1;
    private CollectionType hiddenSelection;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of CompareMaxDate.
    /// </summary>
    [JsonPropertyName("compareMaxDate")]
    public CollectionType CompareMaxDate
    {
      get => compareMaxDate ??= new();
      set => compareMaxDate = value;
    }

    /// <summary>
    /// A value of CompareInitiationDate.
    /// </summary>
    [JsonPropertyName("compareInitiationDate")]
    public CollectionType CompareInitiationDate
    {
      get => compareInitiationDate ??= new();
      set => compareInitiationDate = value;
    }

    private Common error;
    private DateWorkArea current;
    private DateWorkArea dateWorkArea;
    private Common common;
    private CollectionType compareMaxDate;
    private CollectionType compareInitiationDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CollectionType collectionType;
  }
#endregion
}
