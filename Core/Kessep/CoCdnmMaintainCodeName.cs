// Program: CO_CDNM_MAINTAIN_CODE_NAME, ID: 371823606, model: 746.
// Short name: SWECDNMP
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
/// <para>
/// A program: CO_CDNM_MAINTAIN_CODE_NAME.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure facilitates maintenance of entity CODE
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoCdnmMaintainCodeName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CDNM_MAINTAIN_CODE_NAME program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCdnmMaintainCodeName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCdnmMaintainCodeName.
  /// </summary>
  public CoCdnmMaintainCodeName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // Date      Author              Reason
    // 04-26-95                Initial Dev
    // 01/23/96  a. Hackler    add Security/nexttran
    // 10/14/96  R. Marchman   Add Data Level Sec
    // 06/17/97  M. Wheaton    Remove Datenum(0)
    // 03/25/98  S. Konkader	ZDEL Cleanup
    // 01/27/09  Arun Mathias  CQ#8604 Check the code value timeframes.
    // ********************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Starting.CodeName = import.Starting.CodeName;
    UseCabSetMaximumDiscontinueDate1();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailSelection.SelectChar =
        import.Import1.Item.DetailSelection.SelectChar;

      if (!IsEmpty(export.Export1.Item.DetailSelection.SelectChar))
      {
        local.SelCount.Count = (int)((long)local.SelCount.Count + 1);
      }

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);

      if (Equal(export.Export1.Item.Detail.ExpirationDate, local.MaxDate.Date))
      {
        export.Export1.Update.Detail.ExpirationDate = local.Zero.Date;
      }
    }

    import.Import1.CheckIndex();

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();

        return;

        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // ****
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *** CQ#8604 Changes Begin Here ***
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (local.SelCount.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailSelection.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailSelection, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

    // *** CQ#8604 Changes Ends  Here ***
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        if (local.SelCount.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          var field =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        local.Common.Command = "ADD";
        UseCoUpdateCodeName();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.ErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          // *** CQ#8604 Changes Start Here ***
          // Commented code value id because user could change the value.
          // *** CQ#8604 Changes End   Here ***
          var field1 = GetField(export.Export1.Item.Detail, "codeName");

          field1.Error = true;

          var field2 = GetField(export.Export1.Item.Detail, "displayTitle");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.Export1.Item.Detail, "expirationDate");

          field4.Error = true;

          var field5 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field5.Error = true;

          return;
        }

        // ---------------------------------------------
        // Clear the action field for all entries.
        // ---------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        export.Export1.Index = -1;

        foreach(var item in ReadCode())
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          if (export.Export1.Index >= Export.ExportGroup.Capacity)
          {
            break;
          }

          export.Export1.Update.Detail.Assign(entities.Existing);
          local.DateWorkArea.Date = entities.Existing.ExpirationDate;
          UseCabSetMaximumDiscontinueDate2();
          export.Export1.Update.Detail.ExpirationDate = local.DateWorkArea.Date;
          export.Export1.Update.DetailSelection.SelectChar = "";
        }

        if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        for(++export.Export1.Index; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
          export.Export1.Update.Detail.CodeName = "";
          export.Export1.Update.Detail.DisplayTitle =
            Spaces(Code.DisplayTitle_MaxLength);
          export.Export1.Update.Detail.EffectiveDate = local.Zero.Date;
          export.Export1.Update.Detail.ExpirationDate = local.Zero.Date;
          export.Export1.Update.Detail.Id = 0;
        }

        export.Export1.CheckIndex();

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "UPDATE":
        if (local.SelCount.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          var field =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        local.Common.Command = "UPDATE";
        UseCoUpdateCodeName();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          export.Export1.Index = local.ErrorLineNo.Count - 1;
          export.Export1.CheckSize();

          // *** CQ#8604 Changes Start Here ***
          // Commented code value id because user could change the value.
          // *** CQ#8604 Changes End   Here ***
          var field1 = GetField(export.Export1.Item.Detail, "codeName");

          field1.Error = true;

          var field2 = GetField(export.Export1.Item.Detail, "displayTitle");

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.Detail, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.Export1.Item.Detail, "expirationDate");

          field4.Error = true;

          var field5 =
            GetField(export.Export1.Item.DetailSelection, "selectChar");

          field5.Error = true;

          return;
        }

        // ---------------------------------------------
        // Clear the action field for all entries.
        // ---------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailSelection.SelectChar = "";
        }

        export.Export1.CheckIndex();
        ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_2";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveExport1(CoUpdateCodeName.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.DetailSelection.SelectChar = source.DetailSelection.SelectChar;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    CoUpdateCodeName.Import.ImportGroup target)
  {
    target.DetailSelection.SelectChar = source.DetailSelection.SelectChar;
    target.Detail.Assign(source.Detail);
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

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseCoUpdateCodeName()
  {
    var useImport = new CoUpdateCodeName.Import();
    var useExport = new CoUpdateCodeName.Export();

    useImport.Common.Command = local.Common.Command;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(CoUpdateCodeName.Execute, useImport, useExport);

    local.ErrorLineNo.Count = useExport.ErrorLineNo.Count;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);

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

  private IEnumerable<bool> ReadCode()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadCode",
      (db, command) =>
      {
        db.SetString(command, "codeName", export.Starting.CodeName);
      },
      (db, reader) =>
      {
        entities.Existing.Id = db.GetInt32(reader, 0);
        entities.Existing.CodeName = db.GetString(reader, 1);
        entities.Existing.EffectiveDate = db.GetDate(reader, 2);
        entities.Existing.ExpirationDate = db.GetDate(reader, 3);
        entities.Existing.DisplayTitle = db.GetString(reader, 4);
        entities.Existing.Populated = true;

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
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Code Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private Code detail;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Code Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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

    private Code starting;
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
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Code Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelection;
      private Code detail;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Code Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Code starting;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
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
    /// A value of ZeroInitialised.
    /// </summary>
    [JsonPropertyName("zeroInitialised")]
    public DateWorkArea ZeroInitialised
    {
      get => zeroInitialised ??= new();
      set => zeroInitialised = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of SqlCode.
    /// </summary>
    [JsonPropertyName("sqlCode")]
    public Common SqlCode
    {
      get => sqlCode ??= new();
      set => sqlCode = value;
    }

    /// <summary>
    /// A value of AbendCicsTest.
    /// </summary>
    [JsonPropertyName("abendCicsTest")]
    public AbendCicsTest AbendCicsTest
    {
      get => abendCicsTest ??= new();
      set => abendCicsTest = value;
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
    /// A value of LastEntryNo.
    /// </summary>
    [JsonPropertyName("lastEntryNo")]
    public Common LastEntryNo
    {
      get => lastEntryNo ??= new();
      set => lastEntryNo = value;
    }

    /// <summary>
    /// A value of ErrorLineNo.
    /// </summary>
    [JsonPropertyName("errorLineNo")]
    public Common ErrorLineNo
    {
      get => errorLineNo ??= new();
      set => errorLineNo = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private DateWorkArea zero;
    private Common selCount;
    private Common common;
    private DateWorkArea zeroInitialised;
    private DateWorkArea maxDate;
    private Common sqlCode;
    private AbendCicsTest abendCicsTest;
    private DateWorkArea dateWorkArea;
    private Common lastEntryNo;
    private Common errorLineNo;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Code Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Code existing;
  }
#endregion
}
