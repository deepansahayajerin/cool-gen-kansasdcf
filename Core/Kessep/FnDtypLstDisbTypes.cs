// Program: FN_DTYP_LST_DISB_TYPES, ID: 371777825, model: 746.
// Short name: SWEDTYPP
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
/// A program: FN_DTYP_LST_DISB_TYPES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDtypLstDisbTypes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DTYP_LST_DISB_TYPES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDtypLstDisbTypes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDtypLstDisbTypes.
  /// </summary>
  public FnDtypLstDisbTypes(IContext context, Import import, Export export):
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
    // 12/04/96	R. Marchman	Add new security and next tran
    // 05/01/97	A. Kinney	Changed Current_Date
    // 11/02/98  N.Engoor Made changes to the Dialog flows.
    // *********************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Error.Count = 0;
    local.Common.Count = 0;
    export.StartingCode.Code = import.StartingCode.Code;
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

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailDisbursementType.Assign(
        import.Import1.Item.DetailDisbursementType);

      if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          ++local.Common.Count;
          local.Common.SelectChar = export.Export1.Item.DetailCommon.SelectChar;
          MoveDisbursementType(import.Import1.Item.DetailDisbursementType,
            export.Flow);
        }
        else
        {
          ++local.Error.Count;
        }

        if (Equal(export.Flow.DiscontinueDate, null))
        {
          export.Flow.DiscontinueDate = UseCabSetMaximumDiscontinueDate();
        }
      }

      export.Export1.Next();
    }

    switch(AsChar(export.ShowHistory.SelectChar))
    {
      case 'Y':
        break;
      case 'N':
        break;
      case ' ':
        break;
      default:
        var field = GetField(export.ShowHistory, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

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

    // *********************************************
    // Naveen - If selection code other than 'S', highlight corresponding Sel 
    // fields and display error message.
    // *********************************************
    if (local.Error.Count > 0)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
          IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
      }
    }

    // *********************************************
    // Naveen - If multiple Sel fields are entered with a value of 'S' highlight
    // corresponding Sel fields and display error message.
    // *********************************************
    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
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
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (!Equal(global.Command, "DTPM"))
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.MaxDate.DiscontinueDate = UseCabSetMaximumDiscontinueDate();

        if (AsChar(import.ShowHistory.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementType2())
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailDisbursementType.Assign(
              entities.DisbursementType);

            if (Equal(export.Export1.Item.DetailDisbursementType.
              DiscontinueDate, local.MaxDate.DiscontinueDate))
            {
              export.Export1.Update.DetailDisbursementType.DiscontinueDate =
                null;
            }

            export.Export1.Next();
          }
        }
        else
        {
          export.ShowHistory.SelectChar = "N";

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementType1())
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailDisbursementType.Assign(
              entities.DisbursementType);

            if (Equal(export.Export1.Item.DetailDisbursementType.
              DiscontinueDate, local.MaxDate.DiscontinueDate))
            {
              export.Export1.Update.DetailDisbursementType.DiscontinueDate =
                null;
            }

            export.Export1.Next();
          }
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_ITEMS_FOUND";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RETURN":
        if (AsChar(local.Common.SelectChar) == 'S' || local.Common.Count == 0)
        {
          export.Flag.Flag = "Y";
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        break;
      case "DTPM":
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            ExitState = "ECO_XFR_TO_MTN_DISB_TYPE";

            return;
          }
          else if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
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

  private static void MoveDisbursementType(DisbursementType source,
    DisbursementType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.ProgramCode = source.ProgramCode;
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

  private IEnumerable<bool> ReadDisbursementType1()
  {
    return ReadEach("ReadDisbursementType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.StartingCode.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Name = db.GetString(reader, 2);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 3);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 4);
        entities.DisbursementType.EffectiveDate = db.GetDate(reader, 5);
        entities.DisbursementType.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 7);
        entities.DisbursementType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementType2()
  {
    return ReadEach("ReadDisbursementType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.StartingCode.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Name = db.GetString(reader, 2);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 3);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 4);
        entities.DisbursementType.EffectiveDate = db.GetDate(reader, 5);
        entities.DisbursementType.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 7);
        entities.DisbursementType.Populated = true;

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
      /// A value of DetailDisbursementType.
      /// </summary>
      [JsonPropertyName("detailDisbursementType")]
      public DisbursementType DetailDisbursementType
      {
        get => detailDisbursementType ??= new();
        set => detailDisbursementType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common detailCommon;
      private DisbursementType detailDisbursementType;
    }

    /// <summary>
    /// A value of StartingCode.
    /// </summary>
    [JsonPropertyName("startingCode")]
    public DisbursementType StartingCode
    {
      get => startingCode ??= new();
      set => startingCode = value;
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

    private DisbursementType startingCode;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailDisbursementType.
      /// </summary>
      [JsonPropertyName("detailDisbursementType")]
      public DisbursementType DetailDisbursementType
      {
        get => detailDisbursementType ??= new();
        set => detailDisbursementType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common detailCommon;
      private DisbursementType detailDisbursementType;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of StartingCode.
    /// </summary>
    [JsonPropertyName("startingCode")]
    public DisbursementType StartingCode
    {
      get => startingCode ??= new();
      set => startingCode = value;
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
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public DisbursementType Flow
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

    private Common flag;
    private DisbursementType startingCode;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private DisbursementType flow;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DisbursementType MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    private DateWorkArea current;
    private DisbursementType maxDate;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType disbursementType;
  }
#endregion
}
