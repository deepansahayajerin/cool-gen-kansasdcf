// Program: FN_RECF_LST_COST_RCVRY_FEE_INFO, ID: 371778077, model: 746.
// Short name: SWERECFP
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
/// A program: FN_RECF_LST_COST_RCVRY_FEE_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRecfLstCostRcvryFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RECF_LST_COST_RCVRY_FEE_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRecfLstCostRcvryFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRecfLstCostRcvryFeeInfo.
  /// </summary>
  public FnRecfLstCostRcvryFeeInfo(IContext context, Import import,
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
    // *======================================================================
    // Procedure : List Cost Recovery Fee Information
    // Developed By :
    // Modifications :
    // 03/27/1996  by R.B.Mohapatra - MTW
    //     * Changed the RETURN logic to take care of Single selection with 'S'
    // 12/03/96	R. Marchman		
    // Add new security and next tran
    // *======================================================================
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** end   group A ****
    export.Tribunal.Assign(import.Tribunal);
    export.Fips.CountyDescription = import.Fips.CountyDescription;
    export.PromptCharTextWorkArea.Text1 = import.PromptCharTextWorkArea.Text1;
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;

    if (Equal(global.Command, "RETLTRB"))
    {
      if (import.Tribunal.Identifier == 0)
      {
        export.Tribunal.Identifier = import.HiddenTribunal.Identifier;
      }

      global.Command = "DISPLAY";
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "DISPLAY"))
    {
      // On a DISPLAY we wipe out the screen by skipping the move of imports to 
      // exports.
    }
    else
    {
      local.Common.Count = 0;

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
        export.Export1.Update.DetailTribunalFeeInformation.Assign(
          import.Import1.Item.DetailTribunalFeeInformation);

        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          local.Common.Count = (int)((long)local.Common.Count + 1);
          MoveTribunalFeeInformation(export.Export1.Item.
            DetailTribunalFeeInformation, export.Flow);
          local.Common.SelectChar = export.Export1.Item.DetailCommon.SelectChar;

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.Next();
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

    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
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
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "REFI"))
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

    // **** end   group C ****
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (export.Tribunal.Identifier == 0)
        {
          var field = GetField(export.Tribunal, "identifier");

          field.Error = true;

          export.Tribunal.JudicialDistrict = "";
          export.Tribunal.JudicialDivision = "";
          export.Tribunal.Name = "";
          export.Fips.CountyDescription = "";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailTribunalFeeInformation.Description =
              Spaces(TribunalFeeInformation.Description_MaxLength);
            export.Export1.Update.DetailTribunalFeeInformation.DiscontinueDate =
              null;
            export.Export1.Update.DetailTribunalFeeInformation.EffectiveDate =
              null;
            export.Export1.Update.DetailTribunalFeeInformation.Cap = 0;
            export.Export1.Update.DetailTribunalFeeInformation.Rate = 0;
          }

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (ReadTribunal())
        {
          export.Tribunal.Assign(entities.Tribunal);
          export.HiddenTribunal.Identifier = entities.Tribunal.Identifier;

          if (ReadFips())
          {
            export.Fips.CountyDescription = entities.Fips.CountyDescription;
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }
        }
        else
        {
          ExitState = "TRIBUNAL_NF";

          var field = GetField(export.Tribunal, "identifier");

          field.Error = true;

          return;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadTribunalFeeInformation())
        {
          export.Export1.Update.DetailTribunalFeeInformation.Assign(
            entities.TribunalFeeInformation);

          if (Equal(export.Export1.Item.DetailTribunalFeeInformation.
            DiscontinueDate, local.Max.Date))
          {
            export.Export1.Update.DetailTribunalFeeInformation.DiscontinueDate =
              local.Init1.Date;
          }

          export.Export1.Next();
        }

        // Check to see if any cost recovery fees were found.
        if (export.Export1.IsEmpty)
        {
          ExitState = "TRIBUNAL_FEE_INFORMATION_NF";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "RETURN":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_RETURN";

            break;
          case 1:
            if (AsChar(local.Common.SelectChar) == 'S')
            {
              export.Refi.Flag = "Y";

              // *** Selection is Valid
              ExitState = "ACO_NE0000_RETURN";
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "LIST":
        if (AsChar(import.PromptCharTextWorkArea.Text1) != 'S')
        {
          var field = GetField(export.PromptCharTextWorkArea, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else
        {
          export.PromptCharTextWorkArea.Text1 = "+";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "REFI":
        if (export.Tribunal.Identifier == 0)
        {
          ExitState = "SP0000_MANDATORY_FIELD_NOT_ENTRD";

          var field = GetField(export.Tribunal, "identifier");

          field.Error = true;

          return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            if (AsChar(local.Common.SelectChar) == 'S')
            {
              // *** Selection is Valid
              export.Refi.Flag = "Y";
              ExitState = "ECO_XFR_TO_MTN_COST_REC_FEE_INF";
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      default:
        if (IsEmpty(global.Command))
        {
          return;
        }

        if (export.Tribunal.Identifier == 0)
        {
          export.Fips.CountyDescription = "";
          export.Tribunal.JudicialDistrict = "";
          export.Tribunal.JudicialDivision = "";
          export.Tribunal.Name = "";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.DetailTribunalFeeInformation.Cap = 0;
            export.Export1.Update.DetailTribunalFeeInformation.DiscontinueDate =
              null;
            export.Export1.Update.DetailTribunalFeeInformation.EffectiveDate =
              null;
            export.Export1.Update.DetailTribunalFeeInformation.Rate = 0;
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailTribunalFeeInformation.Description =
              Spaces(TribunalFeeInformation.Description_MaxLength);
          }
        }

        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY_3";

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

  private static void MoveTribunalFeeInformation(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
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

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunalFeeInformation()
  {
    return ReadEach("ReadTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.TribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.TribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TribunalFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.TribunalFeeInformation.Rate = db.GetNullableDecimal(reader, 3);
        entities.TribunalFeeInformation.Cap = db.GetNullableDecimal(reader, 4);
        entities.TribunalFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.TribunalFeeInformation.Description =
          db.GetNullableString(reader, 6);
        entities.TribunalFeeInformation.Populated = true;

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
      /// A value of DetailTribunalFeeInformation.
      /// </summary>
      [JsonPropertyName("detailTribunalFeeInformation")]
      public TribunalFeeInformation DetailTribunalFeeInformation
      {
        get => detailTribunalFeeInformation ??= new();
        set => detailTribunalFeeInformation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private TribunalFeeInformation detailTribunalFeeInformation;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of PromptCharCommon.
    /// </summary>
    [JsonPropertyName("promptCharCommon")]
    public Common PromptCharCommon
    {
      get => promptCharCommon ??= new();
      set => promptCharCommon = value;
    }

    /// <summary>
    /// A value of PromptCharTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptCharTextWorkArea")]
    public TextWorkArea PromptCharTextWorkArea
    {
      get => promptCharTextWorkArea ??= new();
      set => promptCharTextWorkArea = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private Tribunal hiddenTribunal;
    private Tribunal tribunal;
    private Common promptCharCommon;
    private TextWorkArea promptCharTextWorkArea;
    private Fips fips;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of DetailTribunalFeeInformation.
      /// </summary>
      [JsonPropertyName("detailTribunalFeeInformation")]
      public TribunalFeeInformation DetailTribunalFeeInformation
      {
        get => detailTribunalFeeInformation ??= new();
        set => detailTribunalFeeInformation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private TribunalFeeInformation detailTribunalFeeInformation;
    }

    /// <summary>
    /// A value of Refi.
    /// </summary>
    [JsonPropertyName("refi")]
    public Common Refi
    {
      get => refi ??= new();
      set => refi = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of PromptCharCommon.
    /// </summary>
    [JsonPropertyName("promptCharCommon")]
    public Common PromptCharCommon
    {
      get => promptCharCommon ??= new();
      set => promptCharCommon = value;
    }

    /// <summary>
    /// A value of PromptCharTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptCharTextWorkArea")]
    public TextWorkArea PromptCharTextWorkArea
    {
      get => promptCharTextWorkArea ??= new();
      set => promptCharTextWorkArea = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    public TribunalFeeInformation Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private Common refi;
    private Tribunal hiddenTribunal;
    private Tribunal tribunal;
    private Common promptCharCommon;
    private TextWorkArea promptCharTextWorkArea;
    private Fips fips;
    private Array<ExportGroup> export1;
    private TribunalFeeInformation flow;
    private NextTranInfo hiddenNextTranInfo;
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
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public DateWorkArea Init1
    {
      get => init1 ??= new();
      set => init1 = value;
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
    private DateWorkArea init1;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private Tribunal tribunal;
    private Fips fips;
    private TribunalFeeInformation tribunalFeeInformation;
  }
#endregion
}
