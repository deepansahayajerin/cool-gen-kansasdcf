// Program: FN_OTRL_LST_OBLIG_TRANS_RLN_RSN, ID: 372613986, model: 746.
// Short name: SWEOTRLP
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
/// A program: FN_OTRL_LST_OBLIG_TRANS_RLN_RSN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOtrlLstObligTransRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OTRL_LST_OBLIG_TRANS_RLN_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOtrlLstObligTransRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOtrlLstObligTransRlnRsn.
  /// </summary>
  public FnOtrlLstObligTransRlnRsn(IContext context, Import import,
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
    // **********************************************
    // 12/27/96  R. Marchman          Add new security/next tran.
    // 09/24/98  G Sharp       Phase 2    Changed to group views from mandatory 
    // to optional.
    // 10/27/98  G Sharp       Phase 2    Clean up zdelete exit state's.
    // **********************************************
    // *******************************************************************************************
    // 10/21/03             B. Lee
    // Made changes so flow from DDAM will go thru security. PR#189075
    // 
    // *******************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // *****
    // Move all IMPORTs to EXPORTs.
    // *****
    export.Search.Code = import.Search.Code;
    export.Hidden.Assign(import.Hidden);
    export.DisplayHistory.Flag = import.DisplayHistory.Flag;
    export.NextTransaction.Command = import.NextTransaction.Command;
    export.PreviousSearch.Code = import.PreviousSearch.Code;
    export.PreviousDisplayHistory.Flag = import.PreviousDisplayHistory.Flag;

    if (IsEmpty(export.DisplayHistory.Flag))
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
      export.Export1.Update.DetailObligationTransactionRlnRsn.Assign(
        import.Import1.Item.DetailObligationTransactionRlnRsn);
      export.Export1.Next();
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

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // **** end   group B ****
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Main CASE OF COMMAND.
    // *****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!IsEmpty(export.Search.Code))
        {
          if (!ReadObligationTransactionRlnRsn1())
          {
            ExitState = "FN0000_OBLIG_RLN_RSN_CODE_NF";

            return;
          }
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadObligationTransactionRlnRsn2())
        {
          if (AsChar(export.DisplayHistory.Flag) != 'Y' && !
            Equal(entities.ObligationTransactionRlnRsn.DiscontinueDt,
            local.Null1.DiscontinueDt) && Lt
            (entities.ObligationTransactionRlnRsn.DiscontinueDt, Now().Date))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.DetailObligationTransactionRlnRsn.Assign(
            entities.ObligationTransactionRlnRsn);

          // *****
          // If the discontinue date contains the maximum date, then display 
          // blanks instead.
          // *****
          if (Equal(export.Export1.Item.DetailObligationTransactionRlnRsn.
            DiscontinueDt, local.Max.Date))
          {
            export.Export1.Update.DetailObligationTransactionRlnRsn.
              DiscontinueDt = local.Null1.DiscontinueDt;
          }

          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          var field = GetField(export.Search, "code");

          field.Error = true;

          return;
        }

        break;
      case "RETURN":
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          local.Common.Count = 0;

          switch(AsChar(import.Import1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              export.HiddenSelection.Assign(
                import.Import1.Item.DetailObligationTransactionRlnRsn);
              ++local.Common.Count;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }

          export.Export1.Next();
        }

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
          export.Export1.Update.DetailObligationTransactionRlnRsn.Assign(
            import.Import1.Item.DetailObligationTransactionRlnRsn);
          export.Export1.Next();
        }

        if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
        {
          return;
        }

        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *****
    // Processing complete set previous views.
    // *****
    export.PreviousDisplayHistory.Flag = export.DisplayHistory.Flag;
    export.PreviousSearch.Code = export.Search.Code;
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
    useImport.NextTranInfo.Assign(export.Hidden);

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

  private bool ReadObligationTransactionRlnRsn1()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn1",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", export.Search.Code);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationTransactionRlnRsn.Name = db.GetString(reader, 2);
        entities.ObligationTransactionRlnRsn.EffectiveDt =
          db.GetDate(reader, 3);
        entities.ObligationTransactionRlnRsn.DiscontinueDt =
          db.GetNullableDate(reader, 4);
        entities.ObligationTransactionRlnRsn.Description =
          db.GetNullableString(reader, 5);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRlnRsn2()
  {
    return ReadEach("ReadObligationTransactionRlnRsn2",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", export.Search.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationTransactionRlnRsn.Name = db.GetString(reader, 2);
        entities.ObligationTransactionRlnRsn.EffectiveDt =
          db.GetDate(reader, 3);
        entities.ObligationTransactionRlnRsn.DiscontinueDt =
          db.GetNullableDate(reader, 4);
        entities.ObligationTransactionRlnRsn.Description =
          db.GetNullableString(reader, 5);
        entities.ObligationTransactionRlnRsn.Populated = true;

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
      /// A value of DetailObligationTransactionRlnRsn.
      /// </summary>
      [JsonPropertyName("detailObligationTransactionRlnRsn")]
      public ObligationTransactionRlnRsn DetailObligationTransactionRlnRsn
      {
        get => detailObligationTransactionRlnRsn ??= new();
        set => detailObligationTransactionRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common detailCommon;
      private ObligationTransactionRlnRsn detailObligationTransactionRlnRsn;
    }

    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
    {
      /// <summary>
      /// A value of ZdelGroupImportDetailCommon.
      /// </summary>
      [JsonPropertyName("zdelGroupImportDetailCommon")]
      public Common ZdelGroupImportDetailCommon
      {
        get => zdelGroupImportDetailCommon ??= new();
        set => zdelGroupImportDetailCommon = value;
      }

      /// <summary>
      /// A value of ZdelGroupImportDetailObligationTransactionRlnRsn.
      /// </summary>
      [JsonPropertyName("zdelGroupImportDetailObligationTransactionRlnRsn")]
      public ObligationTransactionRlnRsn ZdelGroupImportDetailObligationTransactionRlnRsn
        
      {
        get => zdelGroupImportDetailObligationTransactionRlnRsn ??= new();
        set => zdelGroupImportDetailObligationTransactionRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common zdelGroupImportDetailCommon;
      private ObligationTransactionRlnRsn zdelGroupImportDetailObligationTransactionRlnRsn;
        
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ObligationTransactionRlnRsn Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of PreviousDisplayHistory.
    /// </summary>
    [JsonPropertyName("previousDisplayHistory")]
    public Common PreviousDisplayHistory
    {
      get => previousDisplayHistory ??= new();
      set => previousDisplayHistory = value;
    }

    /// <summary>
    /// A value of PreviousSearch.
    /// </summary>
    [JsonPropertyName("previousSearch")]
    public ObligationTransactionRlnRsn PreviousSearch
    {
      get => previousSearch ??= new();
      set => previousSearch = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
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
    /// A value of ZdelImportSearch.
    /// </summary>
    [JsonPropertyName("zdelImportSearch")]
    public ObligationTransactionRlnRsn ZdelImportSearch
    {
      get => zdelImportSearch ??= new();
      set => zdelImportSearch = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroup> Zdel => zdel ??= new(ZdelGroup.Capacity);

    /// <summary>
    /// Gets a value of Zdel for json serialization.
    /// </summary>
    [JsonPropertyName("zdel")]
    [Computed]
    public IList<ZdelGroup> Zdel_Json
    {
      get => zdel;
      set => Zdel.Assign(value);
    }

    private Array<ImportGroup> import1;
    private ObligationTransactionRlnRsn search;
    private Common previousDisplayHistory;
    private ObligationTransactionRlnRsn previousSearch;
    private Common nextTransaction;
    private Common displayHistory;
    private NextTranInfo hidden;
    private Standard standard;
    private ObligationTransactionRlnRsn zdelImportSearch;
    private Array<ZdelGroup> zdel;
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
      /// A value of DetailObligationTransactionRlnRsn.
      /// </summary>
      [JsonPropertyName("detailObligationTransactionRlnRsn")]
      public ObligationTransactionRlnRsn DetailObligationTransactionRlnRsn
      {
        get => detailObligationTransactionRlnRsn ??= new();
        set => detailObligationTransactionRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common detailCommon;
      private ObligationTransactionRlnRsn detailObligationTransactionRlnRsn;
    }

    /// <summary>
    /// A value of PreviousDisplayHistory.
    /// </summary>
    [JsonPropertyName("previousDisplayHistory")]
    public Common PreviousDisplayHistory
    {
      get => previousDisplayHistory ??= new();
      set => previousDisplayHistory = value;
    }

    /// <summary>
    /// A value of PreviousSearch.
    /// </summary>
    [JsonPropertyName("previousSearch")]
    public ObligationTransactionRlnRsn PreviousSearch
    {
      get => previousSearch ??= new();
      set => previousSearch = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ObligationTransactionRlnRsn Search
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
    public ObligationTransactionRlnRsn HiddenSelection
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

    private Common previousDisplayHistory;
    private ObligationTransactionRlnRsn previousSearch;
    private Common nextTransaction;
    private ObligationTransactionRlnRsn search;
    private Common displayHistory;
    private Array<ExportGroup> export1;
    private ObligationTransactionRlnRsn hiddenSelection;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ObligationTransactionRlnRsn Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private DateWorkArea max;
    private ObligationTransactionRlnRsn null1;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
  }
#endregion
}
