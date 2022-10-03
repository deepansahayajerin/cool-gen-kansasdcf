// Program: FN_DISL_LST_DSTRBTN_POLICY, ID: 371958556, model: 746.
// Short name: SWEDISLP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DISL_LST_DSTRBTN_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure lists Distribution Policies. Distribution Policy Code, 
/// Collection Type and Show History indicator are the search criteria.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDislLstDstrbtnPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISL_LST_DSTRBTN_POLICY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDislLstDstrbtnPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDislLstDstrbtnPolicy.
  /// </summary>
  public FnDislLstDstrbtnPolicy(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // : Move all IMPORTs to EXPORTs.
    export.Search.Assign(import.Search);
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.CollectionTypePrompt.SelectChar =
      import.CollectionTypePrompt.SelectChar;

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
      export.Export1.Update.DistributionPolicy.Assign(
        import.Import1.Item.DistributionPolicy);
      MoveCollectionType(import.Import1.Item.CollectionType,
        export.Export1.Update.CollectionType);
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
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

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "DISM") || Equal(global.Command, "DISR"))
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

    // : Check for, and validate, selection.
    local.ItemSelected.Flag = "N";

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      switch(AsChar(export.Export1.Item.Common.SelectChar))
      {
        case 'S':
          // : Check for multiple select
          if (AsChar(local.ItemSelected.Flag) == 'Y')
          {
            ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            continue;
          }

          local.ItemSelected.Flag = "Y";
          export.SelectedDistributionPolicy.Assign(
            export.Export1.Item.DistributionPolicy);

          if (Equal(export.Export1.Item.DistributionPolicy.DiscontinueDt,
            local.Blank.Date))
          {
            export.SelectedDistributionPolicy.DiscontinueDt = local.Max.Date;
          }

          MoveCollectionType(export.Export1.Item.CollectionType,
            export.SelectedCollectionType);

          break;
        case ' ':
          // : Continue Processing ...
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // : Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.CollectionTypePrompt.SelectChar = "";

        switch(AsChar(export.ShowHistory.Flag))
        {
          case 'Y':
            break;
          case 'N':
            break;
          case ' ':
            export.ShowHistory.Flag = "N";

            break;
          default:
            ExitState = "ZD_ACO_NE0000_INVALID_SEL_YN";

            var field = GetField(export.ShowHistory, "flag");

            field.Error = true;

            return;
        }

        if (!IsEmpty(import.Search.Code))
        {
          if (ReadCollectionType1())
          {
            export.Search.Assign(entities.CollectionType);

            // : READ EACH for selection list.
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadDistributionPolicy2())
            {
              if (AsChar(export.ShowHistory.Flag) == 'N')
              {
                if (!Lt(Now().Date, entities.DistributionPolicy.DiscontinueDt))
                {
                  export.Export1.Next();

                  continue;
                }
              }

              export.Export1.Update.DistributionPolicy.Assign(
                entities.DistributionPolicy);

              if (Equal(export.Export1.Item.DistributionPolicy.DiscontinueDt,
                local.Max.Date))
              {
                export.Export1.Update.DistributionPolicy.DiscontinueDt =
                  local.Blank.Date;
              }

              MoveCollectionType(entities.CollectionType,
                export.Export1.Update.CollectionType);
              export.Export1.Next();
            }
          }
          else
          {
            ExitState = "FN0000_COLLECTION_TYPE_NF";

            var field = GetField(export.Search, "code");

            field.Error = true;
          }
        }
        else
        {
          // : READ EACH for selection list.  Collection Source Type search 
          // value NOT entered
          foreach(var item in ReadCollectionType2())
          {
            foreach(var item1 in ReadDistributionPolicy1())
            {
              if (AsChar(export.ShowHistory.Flag) == 'N')
              {
                if (!Lt(Now().Date, entities.DistributionPolicy.DiscontinueDt))
                {
                  continue;
                }
              }

              MoveCollectionType(entities.CollectionType,
                export.Export1.Update.CollectionType);
              export.Export1.Update.DistributionPolicy.Assign(
                entities.DistributionPolicy);
            }
          }
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        if (export.Export1.IsFull)
        {
          ExitState = "FN0000_DIST_PLCY_OVERFLOW_RB";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "LIST":
        if (AsChar(import.CollectionTypePrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "DISM":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          global.Command = "DISPLAY";
        }
        else
        {
          global.Command = "";
        }

        ExitState = "ECO_XFR_TO_MTN_DISTRIB_POLICY";

        break;
      case "DISR":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_TO_LST_MTN_DIST_PLCY_RUL";
        }
        else
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }

        break;
      case "RETURN":
        if (AsChar(local.ItemSelected.Flag) == 'Y')
        {
          global.Command = "DISPLAY";
        }
        else
        {
          global.Command = "";
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCollectionType1()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.Search.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionType2()
  {
    entities.CollectionType.Populated = false;

    return ReadEach("ReadCollectionType2",
      null,
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDistributionPolicy1()
  {
    entities.DistributionPolicy.Populated = false;

    return ReadEach("ReadDistributionPolicy1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cltIdentifier",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.DistributionPolicy.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDistributionPolicy2()
  {
    return ReadEach("ReadDistributionPolicy2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cltIdentifier",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.DistributionPolicy.Populated = true;

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
      /// A value of DistributionPolicy.
      /// </summary>
      [JsonPropertyName("distributionPolicy")]
      public DistributionPolicy DistributionPolicy
      {
        get => distributionPolicy ??= new();
        set => distributionPolicy = value;
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
      public const int Capacity = 200;

      private Common common;
      private DistributionPolicy distributionPolicy;
      private CollectionType collectionType;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CollectionType Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of CollectionTypePrompt.
    /// </summary>
    [JsonPropertyName("collectionTypePrompt")]
    public Common CollectionTypePrompt
    {
      get => collectionTypePrompt ??= new();
      set => collectionTypePrompt = value;
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

    private Standard standard;
    private CollectionType search;
    private Common collectionTypePrompt;
    private Common showHistory;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
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
      /// A value of DistributionPolicy.
      /// </summary>
      [JsonPropertyName("distributionPolicy")]
      public DistributionPolicy DistributionPolicy
      {
        get => distributionPolicy ??= new();
        set => distributionPolicy = value;
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
      public const int Capacity = 200;

      private Common common;
      private DistributionPolicy distributionPolicy;
      private CollectionType collectionType;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CollectionType Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of CollectionTypePrompt.
    /// </summary>
    [JsonPropertyName("collectionTypePrompt")]
    public Common CollectionTypePrompt
    {
      get => collectionTypePrompt ??= new();
      set => collectionTypePrompt = value;
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
    /// A value of SelectedDistributionPolicy.
    /// </summary>
    [JsonPropertyName("selectedDistributionPolicy")]
    public DistributionPolicy SelectedDistributionPolicy
    {
      get => selectedDistributionPolicy ??= new();
      set => selectedDistributionPolicy = value;
    }

    /// <summary>
    /// A value of SelectedCollectionType.
    /// </summary>
    [JsonPropertyName("selectedCollectionType")]
    public CollectionType SelectedCollectionType
    {
      get => selectedCollectionType ??= new();
      set => selectedCollectionType = value;
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

    private Standard standard;
    private CollectionType search;
    private Common collectionTypePrompt;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private DistributionPolicy selectedDistributionPolicy;
    private CollectionType selectedCollectionType;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of ItemSelected.
    /// </summary>
    [JsonPropertyName("itemSelected")]
    public Common ItemSelected
    {
      get => itemSelected ??= new();
      set => itemSelected = value;
    }

    private DateWorkArea blank;
    private DateWorkArea max;
    private Common itemSelected;
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

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private CollectionType collectionType;
    private DistributionPolicy distributionPolicy;
  }
#endregion
}
