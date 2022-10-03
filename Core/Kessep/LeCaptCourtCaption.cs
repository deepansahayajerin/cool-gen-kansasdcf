// Program: LE_CAPT_COURT_CAPTION, ID: 372010744, model: 746.
// Short name: SWECAPTP
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
/// A program: LE_CAPT_COURT_CAPTION.
/// </para>
/// <para>
/// Resp: Legal				
/// This PRAD allows for Add, Update, and Deletion of Caption Lines on Court 
/// Cases.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeCaptCourtCaption: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAPT_COURT_CAPTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCaptCourtCaption(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCaptCourtCaption.
  /// </summary>
  public LeCaptCourtCaption(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ------------------------------------------------------------
    // 05/10/95  Dave Allen			Initial Code
    // 01/02/96  T.O.Redmond			Added Security and Next Tran
    // 03/23/98  Siraj Konkader		ZDEL cleanup
    // 10/08/98  D.JEAN			Remove unused views, starve views, move import to 
    // export for
    // 					group views, remove validation of classification description
    // 08/02/01  LJBachura			Added caption line empty check int he case "return"
    // command
    // 					section (F9) of the code to fix PR 124013
    // 06/28/02  KCole				Added check for flag that indicates if the user is in 
    // the flow
    // 					to add a legal action.
    // 10/10/02  KDoshi			Fix Screen Help Id.
    // 03/26/03  GVandy	PR136420	Do not require a caption to be entered for U 
    // class actions.
    // ----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.LegalAction.Assign(import.LegalAction);
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.Classification.Text11 = import.Classification.Text11;
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.LegalActionFlow.Flag = import.LegalActionFlow.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      if (AsChar(export.LegalActionFlow.Flag) == 'Y')
      {
        // Cannot exit if part of the flow of adding a legal action
        ExitState = "LE0000_ADD_CAPTION";

        return;
      }
      else
      {
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      }
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

      MoveCourtCaption(import.Import1.Item.Cc, export.Export1.Update.Cc);
      export.Export1.Next();
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // --------------------------------------------------
    // The following statements must be placed after MOVE
    // imports to exports
    // --------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (AsChar(export.LegalActionFlow.Flag) == 'Y')
      {
        // User may not next tran to another screen if they arrived here as part
        // of the flow for adding legal action
        ExitState = "LE0000_ADD_CAPTION";

        return;
      }
      else
      {
        // ---------------------------------------------
        // User is going out of this screen to another
        // ---------------------------------------------
        // ---------------------------------------------
        // Set up local next_tran_info for saving the current values for the 
        // next screen
        // ---------------------------------------------
        local.NextTranInfo.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber ?? "";
        local.NextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        UseScCabNextTranPut();

        return;
      }
    }

    if (Equal(global.Command, "RETLROL"))
    {
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "RETLROL"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "PREV") || Equal(global.Command, "NEXT"))
    {
      if (export.LegalAction.Identifier == 0)
      {
        ExitState = "CO0000_NO_DIRECT_ENTRY_TO_SCREEN";

        return;
      }

      if (import.LegalAction.Identifier != 0)
      {
        if (ReadLegalAction())
        {
          MoveLegalAction2(entities.LegalAction, export.LegalAction);

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            if (ReadTribunalFipsTribAddress())
            {
              export.Tribunal.Assign(entities.Tribunal);
              export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
            }
            else
            {
              ExitState = "TRIBUNAL_NF";

              return;
            }
          }
          else if (ReadTribunalFips())
          {
            export.Tribunal.Assign(entities.Tribunal);
            export.Fips.Assign(entities.Fips);
          }
          else
          {
            ExitState = "TRIBUNAL_NF";

            return;
          }
        }
        else
        {
          ExitState = "TRIBUNAL_NF";

          return;
        }

        UseLeGetPetitionerRespondent();
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          // User must complete the flow to LROL when adding a legal action
          ExitState = "LE0000_ADD_CAPTION";

          return;
        }
        else if (export.Export1.IsEmpty && AsChar
          (export.LegalAction.Classification) != 'U')
        {
          ExitState = "LE0000_NO_CAPT_LINE_ENTERED";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        break;
      case "ADD":
        // -----------------------
        // Add Court Caption Lines
        // -----------------------
        if (export.Export1.IsEmpty)
        {
          ExitState = "LE0000_NO_CAPT_LINE_ENTERED";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field = GetField(export.Export1.Item.Cc, "line");

            field.Error = true;

            return;
          }
        }

        UseLeUpdateCourtCaptionLines();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.LegalActionFlow.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_LROL";

            return;
          }
          else
          {
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
          }
        }
        else
        {
          UseEabRollbackCics();

          return;
        }

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Update Court Caption Lines.
        // ---------------------------------------------
        UseLeUpdateCourtCaptionLines();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_2";
        }
        else
        {
          UseEabRollbackCics();

          return;
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "DELETE":
        UseLeDeleteCourtCaptionLines();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --------------------------------
          // Clear the caption lines deleted.
          // --------------------------------
          export.Export1.Index = 0;
          export.Export1.Clear();

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Next();

            break;

            export.Export1.Next();
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
          UseEabRollbackCics();

          return;
        }

        break;
      case "SIGNOFF":
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          // User may not sign off if part of the flow to add a legal action
          ExitState = "LE0000_ADD_CAPTION";

          return;
        }
        else
        {
          // ---------------------------------------------
          // Sign the user off the KESSEP system
          // ---------------------------------------------
          UseScCabSignoff();

          return;
        }

        break;
      case "ENTER":
        // --------------------------------------------------------------------
        // The ENTER key will not be used for functionality here. If it is
        // pressed, an exit state message should be output.
        // --------------------------------------------------------------------
        ExitState = "ACO_NI0000_SELECT_FUNCTION";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseLeListCourtCaptionLines();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if ((!export.Export1.IsEmpty || AsChar
        (export.LegalAction.Classification) == 'U') && AsChar
        (export.LegalActionFlow.Flag) == 'Y')
      {
        ExitState = "ECO_LNK_TO_LROL";
      }
    }
    else
    {
    }
  }

  private static void MoveCourtCaption(CourtCaption source, CourtCaption target)
  {
    target.Number = source.Number;
    target.Line = source.Line;
  }

  private static void MoveExport2(LeUpdateCourtCaptionLines.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveCourtCaption(source.Cc, target.Cc);
  }

  private static void MoveExport3(LeListCourtCaptionLines.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveCourtCaption(source.Cc, target.Cc);
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    LeUpdateCourtCaptionLines.Import.ImportGroup target)
  {
    MoveCourtCaption(source.Cc, target.Cc);
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeDeleteCourtCaptionLines()
  {
    var useImport = new LeDeleteCourtCaptionLines.Import();
    var useExport = new LeDeleteCourtCaptionLines.Export();

    MoveLegalAction2(export.LegalAction, useImport.LegalAction);

    Call(LeDeleteCourtCaptionLines.Execute, useImport, useExport);
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = import.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeListCourtCaptionLines()
  {
    var useImport = new LeListCourtCaptionLines.Import();
    var useExport = new LeListCourtCaptionLines.Export();

    MoveLegalAction2(export.LegalAction, useImport.LegalAction);

    Call(LeListCourtCaptionLines.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private void UseLeUpdateCourtCaptionLines()
  {
    var useImport = new LeUpdateCourtCaptionLines.Import();
    var useExport = new LeUpdateCourtCaptionLines.Export();

    MoveLegalAction2(export.LegalAction, useImport.LegalAction);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(LeUpdateCourtCaptionLines.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport2);
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
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

    MoveLegalAction1(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Fips.County = db.GetInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Fips.State = db.GetInt32(reader, 6);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 7);
        entities.Fips.StateAbbreviation = db.GetString(reader, 8);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 9);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
      });
  }

  private bool ReadTribunalFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.FipsTribAddress.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier1",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetInt32(command, "identifier2", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 7);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.Populated = true;
        entities.Tribunal.Populated = true;
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
      /// A value of Cc.
      /// </summary>
      [JsonPropertyName("cc")]
      public CourtCaption Cc
      {
        get => cc ??= new();
        set => cc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 105;

      private CourtCaption cc;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Classification.
    /// </summary>
    [JsonPropertyName("classification")]
    public WorkArea Classification
    {
      get => classification ??= new();
      set => classification = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of LegalActionFlow.
    /// </summary>
    [JsonPropertyName("legalActionFlow")]
    public Common LegalActionFlow
    {
      get => legalActionFlow ??= new();
      set => legalActionFlow = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private LegalAction legalAction;
    private WorkArea classification;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Security2 hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Common legalActionFlow;
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
      /// A value of Cc.
      /// </summary>
      [JsonPropertyName("cc")]
      public CourtCaption Cc
      {
        get => cc ??= new();
        set => cc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 105;

      private CourtCaption cc;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Classification.
    /// </summary>
    [JsonPropertyName("classification")]
    public WorkArea Classification
    {
      get => classification ??= new();
      set => classification = value;
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
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of LegalActionFlow.
    /// </summary>
    [JsonPropertyName("legalActionFlow")]
    public Common LegalActionFlow
    {
      get => legalActionFlow ??= new();
      set => legalActionFlow = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
    private WorkArea classification;
    private Array<ExportGroup> export1;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private Security2 hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Common legalActionFlow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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

    private CodeValue codeValue;
    private Common validCode;
    private Code code;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
  }
#endregion
}
