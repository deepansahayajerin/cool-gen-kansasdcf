﻿// Program: FN_PHST_LST_MAX_PASSTHRU_HISTORY, ID: 371809364, model: 746.
// Short name: SWEPHSTP
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
/// A program: FN_PHST_LST_MAX_PASSTHRU_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPhstLstMaxPassthruHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PHST_LST_MAX_PASSTHRU_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPhstLstMaxPassthruHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPhstLstMaxPassthruHistory.
  /// </summary>
  public FnPhstLstMaxPassthruHistory(IContext context, Import import,
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
    // *********************************************
    // 12/10/96	R. Marchman	Add new security and next tran
    // *********************************************
    // -----------------------------------------------------------------------------------------------
    // 10/17/2003          Bonnie Lee               PR# 189075
    // Added the command of DISPLAY to security check.
    // -----------------------------------------------------------------------------------------------
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Common.Count = 0;
    local.Error.Count = 0;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // Move all IMPORTs to EXPORTs.
    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
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
      export.Export1.Update.MaximumPassthru.Assign(
        import.Import1.Item.MaximumPassthru);

      if (AsChar(import.Import1.Item.Common.SelectChar) == 'S')
      {
        MoveMaximumPassthru(import.Import1.Item.MaximumPassthru,
          export.FlowSelection);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.Common.SelectChar))
      {
        // Do nothing
      }
      else
      {
        ++local.Error.Count;

        // User's selection is other than 'S'
      }

      export.Export1.Next();
    }

    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
      }
    }

    if (local.Error.Count >= 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // -------------------------------------------------------------
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // -------------------------------------------------------------
        UseScCabNextTranGet();
        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

    // -----------------------------------------------
    //               Main CASE OF COMMAND.
    // -----------------------------------------------
    // **** begin group C ****
    // to validate action level security
    // -----------------------------------------------------------------------------------------------
    // 10/17/2003          Bonnie Lee               PR# 189075
    // Added the command of DISPLAY to security check.
    // -----------------------------------------------------------------------------------------------
    if (Equal(global.Command, "MXPT"))
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

    if (local.Error.Count > 0)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
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

    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "MXPT":
        if (local.Common.Count == 0 && local.Error.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        ExitState = "ECO_XFR_TO_MTN_MAX_PASSTHRU";

        break;
      case "DISPLAY":
        // READ EACH for selection list.
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadMaximumPassthru())
        {
          export.Export1.Update.MaximumPassthru.
            Assign(entities.MaximumPassthru);

          if (Equal(entities.MaximumPassthru.DiscontinueDate, local.Max.Date))
          {
            export.Export1.Update.MaximumPassthru.DiscontinueDate =
              local.Zero.Date;
          }

          export.Export1.Next();
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "RETURN":
        if (local.Common.Count <= 1)
        {
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

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

  private static void MoveMaximumPassthru(MaximumPassthru source,
    MaximumPassthru target)
  {
    target.Amount = source.Amount;
    target.Description = source.Description;
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

  private IEnumerable<bool> ReadMaximumPassthru()
  {
    return ReadEach("ReadMaximumPassthru",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.MaximumPassthru.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MaximumPassthru.Amount = db.GetDecimal(reader, 1);
        entities.MaximumPassthru.EffectiveDate = db.GetDate(reader, 2);
        entities.MaximumPassthru.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.MaximumPassthru.Description = db.GetNullableString(reader, 4);
        entities.MaximumPassthru.Populated = true;

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
      /// A value of MaximumPassthru.
      /// </summary>
      [JsonPropertyName("maximumPassthru")]
      public MaximumPassthru MaximumPassthru
      {
        get => maximumPassthru ??= new();
        set => maximumPassthru = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private MaximumPassthru maximumPassthru;
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
      /// A value of MaximumPassthru.
      /// </summary>
      [JsonPropertyName("maximumPassthru")]
      public MaximumPassthru MaximumPassthru
      {
        get => maximumPassthru ??= new();
        set => maximumPassthru = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private MaximumPassthru maximumPassthru;
    }

    /// <summary>
    /// A value of Mxpt.
    /// </summary>
    [JsonPropertyName("mxpt")]
    public Common Mxpt
    {
      get => mxpt ??= new();
      set => mxpt = value;
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
    public MaximumPassthru FlowSelection
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

    private Common mxpt;
    private Array<ExportGroup> export1;
    private MaximumPassthru flowSelection;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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

    private Common error;
    private DateWorkArea zero;
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
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    private MaximumPassthru maximumPassthru;
  }
#endregion
}
