// Program: GB_LPPI_LST_PGM_PROCESSING_INFO, ID: 371743776, model: 746.
// Short name: SWELPPIP
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
/// A program: GB_LPPI_LST_PGM_PROCESSING_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbLppiLstPgmProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_LPPI_LST_PGM_PROCESSING_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbLppiLstPgmProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbLppiLstPgmProcessingInfo.
  /// </summary>
  public GbLppiLstPgmProcessingInfo(IContext context, Import import,
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
    // ---------------------------------------------
    // Every initial development and change to that development needs to be 
    // documented.
    // ---------------------------------------------
    // ---------------------------------------------------------
    // Date      Developer Name    Request #  Description
    // 04/05/96  Burrell - SRS                New Development
    // 12/12/96  R. Marchman                  Add new security/next tran
    // --------------------------------------------------------
    // -----------------------------------------------------------------------
    // 10/22/1998      JF.CAILLOUET  REMOVED FLOW TO LPRN - BATCH PROC CHANGES
    // ------------------------------------------------------------------------
    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // ---------------------------------------------
    // Parameters passed when called by program MPPI
    // ---------------------------------------------
    // *  Program_processing_info_name
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    // ---------------------------------------------
    // 	Move all IMPORTs to EXPORTs.
    // ---------------------------------------------
    export.Search.Name = import.Search.Name;
    export.ShowHistory.Flag = import.ShowHistory.Flag;

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
      MoveProgramProcessingInfo(import.Import1.Item.DetailProgramProcessingInfo,
        export.Export1.Update.DetailProgramProcessingInfo);
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

      ExitState = "ZD_ACO_NE0000_INVALID_INDICATOR";

      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
    }
    else
    {
      // Check to see if a selection has been made.
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case 'S':
            export.Flow.Assign(export.Export1.Item.DetailProgramProcessingInfo);
            ++local.Select.Count;

            if (local.Select.Count > 1)
            {
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }

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

            break;
        }
      }
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;

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
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();
    }

    if (Equal(global.Command, "RETLPPI"))
    {
      if (IsEmpty(import.Flow.Name))
      {
        return;
      }
      else
      {
        export.Search.Name = import.Flow.Name;
        global.Command = "DISPLAY";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *** Continue
    }
    else
    {
      return;
    }

    // ---------------------------------------------
    //          *** Mainline Processing
    // ---------------------------------------------
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "DISPLAY":
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadProgramProcessingInfo())
        {
          if (AsChar(export.ShowHistory.Flag) == 'N' && Equal
            (entities.ProgramProcessingInfo.Name,
            local.ProgramProcessingInfo.Name))
          {
            export.Export1.Next();

            continue;
          }
          else
          {
            local.ProgramProcessingInfo.Name =
              entities.ProgramProcessingInfo.Name;
            export.Export1.Update.DetailProgramProcessingInfo.Assign(
              entities.ProgramProcessingInfo);
            export.Export1.Update.DetailDescription.Text50 =
              entities.ProgramProcessingInfo.Description ?? Spaces(50);
            export.Export1.Update.DetailCreated.Date =
              Date(entities.ProgramProcessingInfo.CreatedTimestamp);
            export.Export1.Update.DetailCommon.SelectChar = "";
          }

          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
        }

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
      case "MPPI":
        ExitState = "ECO_XFR_TO_MTNX";

        break;
      case "LPRN":
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedTimestamp = source.CreatedTimestamp;
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

  private IEnumerable<bool> ReadProgramProcessingInfo()
  {
    return ReadEach("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", import.Search.Name);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.Description =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;

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
      /// A value of DetailProgramProcessingInfo.
      /// </summary>
      [JsonPropertyName("detailProgramProcessingInfo")]
      public ProgramProcessingInfo DetailProgramProcessingInfo
      {
        get => detailProgramProcessingInfo ??= new();
        set => detailProgramProcessingInfo = value;
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
      public const int Capacity = 75;

      private Common detailCommon;
      private ProgramProcessingInfo detailProgramProcessingInfo;
      private DateWorkArea detailCreated;
      private WorkArea detailDescription;
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
    public ProgramProcessingInfo Search
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
    public ProgramProcessingInfo Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    private Common showHistory;
    private ProgramProcessingInfo search;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private ProgramProcessingInfo flow;
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
      /// A value of DetailProgramProcessingInfo.
      /// </summary>
      [JsonPropertyName("detailProgramProcessingInfo")]
      public ProgramProcessingInfo DetailProgramProcessingInfo
      {
        get => detailProgramProcessingInfo ??= new();
        set => detailProgramProcessingInfo = value;
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
      public const int Capacity = 75;

      private Common detailCommon;
      private ProgramProcessingInfo detailProgramProcessingInfo;
      private DateWorkArea detailCreated;
      private WorkArea detailDescription;
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
    public ProgramProcessingInfo Search
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
    public ProgramProcessingInfo Flow
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

    private Common showHistory;
    private ProgramProcessingInfo search;
    private Array<ExportGroup> export1;
    private ProgramProcessingInfo flow;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private Common select;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
