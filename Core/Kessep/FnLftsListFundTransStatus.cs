// Program: FN_LFTS_LIST_FUND_TRANS_STATUS, ID: 371785282, model: 746.
// Short name: SWELFTSP
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
/// A program: FN_LFTS_LIST_FUND_TRANS_STATUS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnLftsListFundTransStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LFTS_LIST_FUND_TRANS_STATUS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLftsListFundTransStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLftsListFundTransStatus.
  /// </summary>
  public FnLftsListFundTransStatus(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date 	  Developer Name	Description
    // --------  -----------------     
    // -----------------------------------
    // 01/96     Holly Kennedy-MTW 	Source
    // 02/16/96  Holly Kennedy-MTW	Retrofits
    // 12/10/96  R. Marchman		Add new security and next tran.
    // 10/23/98  Judy Katz - SRG	Modify Load Module Name, Screen ID,
    // 				and Source Code ID to synch up with
    // 				CICS and Security tables.
    // 				Validate Display History indicator.
    // 				Only test security if command is Display.
    // 				Fix Return logic so that the selected
    // 				record is returned.
    // 				Remove unused xxfmmenu logic.
    // 				Add program documentation.
    // -------------------------------------------------------------------
    // -----------------------------------------------
    // Set initial EXIT STATE.
    // -----------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // --------------------------------------------------
    // Move all IMPORTs to EXPORTs.
    // --------------------------------------------------
    export.Starting.Code = import.Starting.Code;
    export.DisplayHistory.Flag = import.DisplayHistory.Flag;

    if (IsEmpty(import.DisplayHistory.Flag))
    {
      export.DisplayHistory.Flag = "N";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailFundTransactionStatus.Assign(
          import.Import1.Item.DetailFundTransactionStatus);
        export.Export1.Next();
      }
    }

    // -------------------------------------------------------
    //                     Next Tran Logic
    // If the next tran info is not equal to spaces, the user
    // requested a next tran action.  Now validate.
    // -------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

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
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    // -----------------------------------------
    // Validate action level security.
    // -----------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ------------------------------------------
    // Main CASE OF COMMAND.
    // ------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ---------------------------------------
        // Validate History indicator.
        // ---------------------------------------
        if (AsChar(export.DisplayHistory.Flag) != 'Y' && AsChar
          (export.DisplayHistory.Flag) != 'N')
        {
          ExitState = "INVALID_VALUE";

          return;
        }

        // ---------------------------------------
        // READ EACH for selection list.
        // ---------------------------------------
        local.Current.Date = Now().Date;

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadFundTransactionStatus())
        {
          // -----------------------------------------------------------
          // Only display Active records unless history records are
          // requested.
          // -----------------------------------------------------------
          if (AsChar(export.DisplayHistory.Flag) != 'Y' && Lt
            (entities.FundTransactionStatus.DiscontinueDate, local.Current.Date))
            
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.DetailFundTransactionStatus.Assign(
            entities.FundTransactionStatus);

          // -----------------------------------------------------------
          // If the discontinue date contains the maximum date, then
          // display blanks instead.
          // -----------------------------------------------------------
          local.Max.Date = entities.FundTransactionStatus.DiscontinueDate;
          export.Export1.Update.DetailFundTransactionStatus.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Export1.Next();
        }

        // ------------------------------------------------------------
        // Set up informational messages for status of display action.
        // ------------------------------------------------------------
        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RETURN":
        local.Select.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // -----------------------------------------------------------
          // Check to see if a selection has been made, and that valid
          // select character is used and that only one is selected
          // -----------------------------------------------------------
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              MoveFundTransactionStatus(export.Export1.Item.
                DetailFundTransactionStatus, export.HiddenSelected);
              ++local.Select.Count;

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }

          if (local.Select.Count > 1)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveFundTransactionStatus(FundTransactionStatus source,
    FundTransactionStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
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

  private IEnumerable<bool> ReadFundTransactionStatus()
  {
    return ReadEach("ReadFundTransactionStatus",
      (db, command) =>
      {
        db.SetString(command, "code", export.Starting.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatus.Code = db.GetString(reader, 1);
        entities.FundTransactionStatus.Name = db.GetString(reader, 2);
        entities.FundTransactionStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.FundTransactionStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.FundTransactionStatus.Populated = true;

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
      /// A value of DetailFundTransactionStatus.
      /// </summary>
      [JsonPropertyName("detailFundTransactionStatus")]
      public FundTransactionStatus DetailFundTransactionStatus
      {
        get => detailFundTransactionStatus ??= new();
        set => detailFundTransactionStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private Common detailCommon;
      private FundTransactionStatus detailFundTransactionStatus;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public FundTransactionStatus Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private FundTransactionStatus starting;
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
      /// A value of DetailFundTransactionStatus.
      /// </summary>
      [JsonPropertyName("detailFundTransactionStatus")]
      public FundTransactionStatus DetailFundTransactionStatus
      {
        get => detailFundTransactionStatus ??= new();
        set => detailFundTransactionStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private Common detailCommon;
      private FundTransactionStatus detailFundTransactionStatus;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public FundTransactionStatus Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public FundTransactionStatus HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
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

    private FundTransactionStatus starting;
    private Common displayHistory;
    private FundTransactionStatus hiddenSelected;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private DateWorkArea current;
    private DateWorkArea max;
    private Common select;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    private FundTransactionStatus fundTransactionStatus;
  }
#endregion
}
