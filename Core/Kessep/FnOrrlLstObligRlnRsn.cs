// Program: FN_ORRL_LST_OBLIG_RLN_RSN, ID: 371740950, model: 746.
// Short name: SWEORRLP
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
/// A program: FN_ORRL_LST_OBLIG_RLN_RSN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOrrlLstObligRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ORRL_LST_OBLIG_RLN_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOrrlLstObligRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOrrlLstObligRlnRsn.
  /// </summary>
  public FnOrrlLstObligRlnRsn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // 12/16/96	R. Marchman	  Add new security/next tran*
    // 04/10/97        H. Kennedy-MTW    Changed flow from DDAM to
    //                                   
    // execute first with command of
    //                                   
    // display
    //                                   
    // Changed show history field to
    //                                   
    // ief select char to avoid
    //                                   
    // permitted value error msg.
    //                                   
    // Added validation logic
    //                                   
    // Added logic to highlight
    //                                   
    // Selection field is the select
    //                                   
    // character is not 'S'.
    // ****************************************************************
    // *******************************************************************************************
    // 10/21/03             B. Lee
    // Made changes so flow from DDAM will go thru security. PR#189075
    // 
    // *******************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *****
    // Move all IMPORTs to EXPORTs.
    // *****
    MoveObligationRlnRsn(import.Start, export.Start);
    export.ShowHistory.SelectChar = import.ShowHistory.SelectChar;

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
      export.Export1.Update.ObligationRlnRsn.Assign(
        import.Import1.Item.ObligationRlnRsn);
      export.Export1.Next();
    }

    if (IsEmpty(export.ShowHistory.SelectChar))
    {
      export.ShowHistory.SelectChar = "N";
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
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
      global.Command = "DISPLAY";

      return;
    }

    // **** end   group B ****
    // *****
    // Main CASE OF COMMAND.
    // *****
    switch(AsChar(export.ShowHistory.SelectChar))
    {
      case 'Y':
        break;
      case 'N':
        break;
      default:
        var field = GetField(export.ShowHistory, "selectChar");

        field.Error = true;

        ExitState = "ZD_ACO_NE0000_INVALID_SEL_YN";

        return;
    }

    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        ExitState = "ZD_CO0000_CLEAR_SUCCESSFUL_1";

        break;
      case "DISPLAY":
        // *****
        // READ EACH for selection list.
        // *****
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadObligationRlnRsn())
        {
          if (AsChar(export.ShowHistory.SelectChar) != 'Y' && !
            Equal(entities.ObligationRlnRsn.DiscontinueDt, local.Null1.Date) &&
            Lt(entities.ObligationRlnRsn.DiscontinueDt, Now().Date))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.ObligationRlnRsn.Assign(
            entities.ObligationRlnRsn);

          // *****
          // If the discontinue date contains the maximum date, then display 
          // blanks instead.
          // *****
          if (Equal(export.Export1.Item.ObligationRlnRsn.DiscontinueDt,
            local.Max.Date))
          {
            export.Export1.Update.ObligationRlnRsn.DiscontinueDt =
              local.Null1.Date;
          }

          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          var field = GetField(export.Start, "code");

          field.Error = true;
        }

        break;
      case "RETURN":
        // Return to link with a selection made
        local.Common.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            ++local.Common.Count;
            export.FlowSelection.Assign(export.Export1.Item.ObligationRlnRsn);
          }
          else if (IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            // *****
            // Do nothing
            // *****
          }
          else
          {
            // *****
            // Users entered selection other than 'S'
            // *****
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S3";

          return;
        }

        if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
        {
          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

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

  private static void MoveObligationRlnRsn(ObligationRlnRsn source,
    ObligationRlnRsn target)
  {
    target.SequentialGeneratedIdentifier = source.SequentialGeneratedIdentifier;
    target.Code = source.Code;
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

  private IEnumerable<bool> ReadObligationRlnRsn()
  {
    return ReadEach("ReadObligationRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "obRlnRsnCd", import.Start.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ObligationRlnRsn.SequentialGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationRlnRsn.Name = db.GetString(reader, 2);
        entities.ObligationRlnRsn.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationRlnRsn.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.ObligationRlnRsn.CreatedTmst = db.GetDateTime(reader, 6);
        entities.ObligationRlnRsn.Description = db.GetString(reader, 7);
        entities.ObligationRlnRsn.Populated = true;

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
      /// A value of ObligationRlnRsn.
      /// </summary>
      [JsonPropertyName("obligationRlnRsn")]
      public ObligationRlnRsn ObligationRlnRsn
      {
        get => obligationRlnRsn ??= new();
        set => obligationRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private ObligationRlnRsn obligationRlnRsn;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public ObligationRlnRsn Start
    {
      get => start ??= new();
      set => start = value;
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

    private Common showHistory;
    private ObligationRlnRsn start;
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
      /// A value of ObligationRlnRsn.
      /// </summary>
      [JsonPropertyName("obligationRlnRsn")]
      public ObligationRlnRsn ObligationRlnRsn
      {
        get => obligationRlnRsn ??= new();
        set => obligationRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private ObligationRlnRsn obligationRlnRsn;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public ObligationRlnRsn Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of FlowSelection.
    /// </summary>
    [JsonPropertyName("flowSelection")]
    public ObligationRlnRsn FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
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
    private ObligationRlnRsn start;
    private Array<ExportGroup> export1;
    private ObligationRlnRsn flowSelection;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
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

    private DateWorkArea null1;
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
    /// A value of ObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationRlnRsn")]
    public ObligationRlnRsn ObligationRlnRsn
    {
      get => obligationRlnRsn ??= new();
      set => obligationRlnRsn = value;
    }

    private ObligationRlnRsn obligationRlnRsn;
  }
#endregion
}
