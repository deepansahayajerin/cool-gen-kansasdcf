// Program: FN_CAJR_LST_COLLECTION_ADJ_RSN, ID: 372378218, model: 746.
// Short name: SWECAJRP
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
/// A program: FN_CAJR_LST_COLLECTION_ADJ_RSN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCajrLstCollectionAdjRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAJR_LST_COLLECTION_ADJ_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCajrLstCollectionAdjRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCajrLstCollectionAdjRsn.
  /// </summary>
  public FnCajrLstCollectionAdjRsn(IContext context, Import import,
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
    // Date     Developer Name   Request#      Description
    // 02/16/96 Rick Delgado                	New Development
    // 06/14/96 Holly Kennedy-	MTW		Fix header to be standard and to
    // 					have correct screen ID.
    // 					Added Starting code selection
    // 					criteria and Show History select
    // 					criteria.
    // 11/27/96	R. Marchman		Add new security and next tran
    // 04/10/96  H.Kennedy-MTW                 Added return flow to CAMM
    //                                         
    // Added validation for the
    //                                         
    // select character
    //                                         
    // Changed Show History view
    //                                         
    // To ief select char in order to
    //                                         
    // avoid the permitted value
    //                                         
    // error message and added
    //                                         
    // validation for the field.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.ShowHistory.SelectChar = import.ShowHistory.SelectChar;
    export.Starting.Code = import.Starting.Code;

    if (IsEmpty(import.Starting.Code))
    {
      export.Starting.Code = "A";
    }

    if (IsEmpty(export.ShowHistory.SelectChar))
    {
      export.ShowHistory.SelectChar = "N";
    }

    local.Common.Count = 0;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.CollectionAdjustmentReason.Assign(
        import.Group.Item.CollectionAdjustmentReason);
      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;

      switch(AsChar(export.Group.Item.Common.SelectChar))
      {
        case 'S':
          ++local.Common.Count;

          break;
        case ' ':
          break;
        default:
          ExitState = "ZD_ACO_NE0000_INVALID_SELECT_COD";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          break;
      }

      export.Group.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

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

    if (local.Common.Count > 1)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }
    else if (local.Common.Count == 1)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          MoveCollectionAdjustmentReason(export.Group.Item.
            CollectionAdjustmentReason, export.Pass);

          break;
        }
      }
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
    if (!Equal(global.Command, "ENTER"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

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

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        ExitState = "ECO_LNK_TO_MTN_COLL_ADJMNTS";

        break;
      case "DISPLAY":
        UseFnGetAllCollectionAdjRsn();

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

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
  }

  private static void MoveCollectionAdjustmentReason(
    CollectionAdjustmentReason source, CollectionAdjustmentReason target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveExport1ToGroup(FnGetAllCollectionAdjRsn.Export.
    ExportGroup source, Export.GroupGroup target)
  {
    target.Common.SelectChar = source.Import.SelectChar;
    target.CollectionAdjustmentReason.Assign(source.CollectionAdjustmentReason);
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

  private void UseFnGetAllCollectionAdjRsn()
  {
    var useImport = new FnGetAllCollectionAdjRsn.Import();
    var useExport = new FnGetAllCollectionAdjRsn.Export();

    useImport.ShowHistory.SelectChar = import.ShowHistory.SelectChar;
    useImport.Starting.Code = export.Starting.Code;

    Call(FnGetAllCollectionAdjRsn.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Group, MoveExport1ToGroup);
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CollectionAdjustmentReason.
      /// </summary>
      [JsonPropertyName("collectionAdjustmentReason")]
      public CollectionAdjustmentReason CollectionAdjustmentReason
      {
        get => collectionAdjustmentReason ??= new();
        set => collectionAdjustmentReason = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common common;
      private CollectionAdjustmentReason collectionAdjustmentReason;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CollectionAdjustmentReason Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    private CollectionAdjustmentReason starting;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CollectionAdjustmentReason.
      /// </summary>
      [JsonPropertyName("collectionAdjustmentReason")]
      public CollectionAdjustmentReason CollectionAdjustmentReason
      {
        get => collectionAdjustmentReason ??= new();
        set => collectionAdjustmentReason = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common common;
      private CollectionAdjustmentReason collectionAdjustmentReason;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CollectionAdjustmentReason Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CollectionAdjustmentReason Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private CollectionAdjustmentReason pass;
    private Common showHistory;
    private CollectionAdjustmentReason starting;
    private Array<GroupGroup> group;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }
#endregion
}
