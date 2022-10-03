// Program: GB_LPNI_LST_PGM_NAME_INFO, ID: 373402889, model: 746.
// Short name: SWELPNIP
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
/// A program: GB_LPNI_LST_PGM_NAME_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbLpniLstPgmNameInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_LPNI_LST_PGM_NAME_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbLpniLstPgmNameInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbLpniLstPgmNameInfo.
  /// </summary>
  public GbLpniLstPgmNameInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------
    // Date      Developer Name    Request #  Description
    // 08/14/01  K Cole - SRS                 New Development
    // --------------------------------------------------------
    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // ---------------------------------------------
    // Parameters passed when called by program MPNI
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    // ---------------------------------------------
    // 	Move all IMPORTs to EXPORTs.
    // ---------------------------------------------
    export.Search.PgmName = import.Search.PgmName;

    if (IsEmpty(import.ShowHistory.Flag))
    {
      export.ShowHistory.Flag = "N";
    }
    else
    {
      export.ShowHistory.Flag = import.ShowHistory.Flag;
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
      MovePgmNameTable(import.Import1.Item.DetailPgmNameTable,
        export.Export1.Update.DetailPgmNameTable);
      export.Export1.Update.DetailCreated.Date =
        import.Import1.Item.DetailCreated.Date;
      export.Export1.Update.DetailDescription.Text50 =
        import.Import1.Item.DetailDescription.Text50;
      export.Export1.Next();
    }

    // ---------------------------------------------
    //          *** Edit Screen ***
    // ---------------------------------------------
    if (AsChar(export.ShowHistory.Flag) == 'Y' || AsChar
      (export.ShowHistory.Flag) == 'N')
    {
      // *** valid value entered -- continue
    }
    else
    {
      var field = GetField(export.ShowHistory, "flag");

      field.Error = true;

      ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

      return;
    }

    if (Equal(global.Command, "RETURN") || Equal(global.Command, "MPNI"))
    {
      // Check to see if a selection has been made.
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case 'S':
            ++local.Select.Count;

            if (local.Select.Count > 1)
            {
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              return;
            }

            export.Flow.Assign(export.Export1.Item.DetailPgmNameTable);

            break;
          case ' ':
            // *** No action
            break;
          default:
            ++local.Select.Count;

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
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
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is coming into this procedure on a next tran action.
      // ****
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // *****flowing from the menu, we just want to display the info and wait 
      // for the next command *****************************
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
      else
      {
      }
    }

    // ---------------------------------------------
    //          *** PF Key Processing
    // ---------------------------------------------
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadPgmNameTable())
        {
          if (!IsEmpty(import.Filter.PgmType))
          {
            if (!Equal(entities.PgmNameTable.PgmType, import.Filter.PgmType))
            {
              export.Export1.Next();

              continue;
            }
          }

          if (AsChar(export.ShowHistory.Flag) == 'N' && AsChar
            (entities.PgmNameTable.PgmActive) == 'N')
          {
            export.Export1.Next();

            continue;
          }
          else
          {
            export.Export1.Update.DetailPgmNameTable.Assign(
              entities.PgmNameTable);
            export.Export1.Update.DetailDescription.Text50 =
              entities.PgmNameTable.PgmDescription;
            export.Export1.Update.DetailCreated.Date =
              Date(entities.PgmNameTable.CreatedTimestamp);
            export.Export1.Update.DetailCommon.SelectChar = "";
          }

          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
        }
        else if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "MPNI":
        ExitState = "ECO_LNK_TO_MPNI";

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

  private static void MovePgmNameTable(PgmNameTable source, PgmNameTable target)
  {
    target.PgmName = source.PgmName;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.PgmActive = source.PgmActive;
    target.PgmType = source.PgmType;
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

  private IEnumerable<bool> ReadPgmNameTable()
  {
    return ReadEach("ReadPgmNameTable",
      (db, command) =>
      {
        db.SetString(command, "pgmName", import.Search.PgmName);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PgmNameTable.LastRunDate = db.GetDate(reader, 0);
        entities.PgmNameTable.PgmName = db.GetString(reader, 1);
        entities.PgmNameTable.PgmDescription = db.GetString(reader, 2);
        entities.PgmNameTable.PgmType = db.GetString(reader, 3);
        entities.PgmNameTable.PgmActive = db.GetString(reader, 4);
        entities.PgmNameTable.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PgmNameTable.CreatedBy = db.GetString(reader, 6);
        entities.PgmNameTable.Populated = true;

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
      /// A value of DetailPgmNameTable.
      /// </summary>
      [JsonPropertyName("detailPgmNameTable")]
      public PgmNameTable DetailPgmNameTable
      {
        get => detailPgmNameTable ??= new();
        set => detailPgmNameTable = value;
      }

      /// <summary>
      /// A value of DetailCreated.
      /// </summary>
      [JsonPropertyName("detailCreated")]
      public DateWorkArea DetailCreated
      {
        get => detailCreated ??= new();
        set => detailCreated = value;
      }

      /// <summary>
      /// A value of DetailDescription.
      /// </summary>
      [JsonPropertyName("detailDescription")]
      public WorkArea DetailDescription
      {
        get => detailDescription ??= new();
        set => detailDescription = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 72;

      private Common detailCommon;
      private PgmNameTable detailPgmNameTable;
      private DateWorkArea detailCreated;
      private WorkArea detailDescription;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public PgmNameTable Filter
    {
      get => filter ??= new();
      set => filter = value;
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
    public PgmNameTable Search
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

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public PgmNameTable Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    private PgmNameTable filter;
    private Common showHistory;
    private PgmNameTable search;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private PgmNameTable flow;
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
      /// A value of DetailPgmNameTable.
      /// </summary>
      [JsonPropertyName("detailPgmNameTable")]
      public PgmNameTable DetailPgmNameTable
      {
        get => detailPgmNameTable ??= new();
        set => detailPgmNameTable = value;
      }

      /// <summary>
      /// A value of DetailCreated.
      /// </summary>
      [JsonPropertyName("detailCreated")]
      public DateWorkArea DetailCreated
      {
        get => detailCreated ??= new();
        set => detailCreated = value;
      }

      /// <summary>
      /// A value of DetailDescription.
      /// </summary>
      [JsonPropertyName("detailDescription")]
      public WorkArea DetailDescription
      {
        get => detailDescription ??= new();
        set => detailDescription = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 72;

      private Common detailCommon;
      private PgmNameTable detailPgmNameTable;
      private DateWorkArea detailCreated;
      private WorkArea detailDescription;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public PgmNameTable Filter
    {
      get => filter ??= new();
      set => filter = value;
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
    public PgmNameTable Search
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
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public PgmNameTable Flow
    {
      get => flow ??= new();
      set => flow = value;
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

    private PgmNameTable filter;
    private Common showHistory;
    private PgmNameTable search;
    private Array<ExportGroup> export1;
    private PgmNameTable flow;
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
    /// A value of PgmNameTable.
    /// </summary>
    [JsonPropertyName("pgmNameTable")]
    public PgmNameTable PgmNameTable
    {
      get => pgmNameTable ??= new();
      set => pgmNameTable = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    private PgmNameTable pgmNameTable;
    private Common select;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PgmNameTable.
    /// </summary>
    [JsonPropertyName("pgmNameTable")]
    public PgmNameTable PgmNameTable
    {
      get => pgmNameTable ??= new();
      set => pgmNameTable = value;
    }

    private PgmNameTable pgmNameTable;
  }
#endregion
}
