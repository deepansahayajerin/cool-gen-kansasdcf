// Program: LE_AACC_ADM_ACT_TAKN_BY_CRT_CASE, ID: 372578614, model: 746.
// Short name: SWEAACCP
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
/// A program: LE_AACC_ADM_ACT_TAKN_BY_CRT_CASE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block lists the administrative action taken by court 
/// case.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAaccAdmActTaknByCrtCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AACC_ADM_ACT_TAKN_BY_CRT_CASE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAaccAdmActTaknByCrtCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAaccAdmActTaknByCrtCase.
  /// </summary>
  public LeAaccAdmActTaknByCrtCase(IContext context, Import import,
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
    // *******************************************************************
    //   Date	  Developer  Request #  Description
    // 09-20-95  Govind		Initial development
    // 09/15/98  P. Sharp		Phase II - Screen assessment changes
    // 10/20/99  R.Jean     PR74995	Move the taken date to the curr amt date (
    // displayed
    // 				on screen) if the type is not FDSO, SDSO, CRED.
    // 12/13/00  GVandy     PR109209	1. Do a display when returning from prompt 
    // to ADAA.
    // 				2. Display blanks in the amount field instead of zeros.
    // 				3. Correct a blank line in the repeating group when both
    // 				   Automatic and Manual actions are displayed.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      // **** begin group F ****
      UseScCabSignoff();

      return;

      // **** end   group F ****
    }

    export.LegalAction.CourtCaseNumber = import.LegalAction.CourtCaseNumber;
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.Foreign.Country = import.Foreign.Country;
    export.PromptTribunal.PromptField = import.PromptTribunal.PromptField;
    export.Required.Type1 = import.Required.Type1;
    export.StartDate.Date = import.StartDate.Date;
    export.PromptAdminActType.PromptField =
      import.PromptAdminActType.PromptField;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.OptionAutoManualAdmAc.OneChar = import.OptionAutoManualAdmAc.OneChar;

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";

      if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        return;
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "RETADAA"))
    {
      export.PromptAdminActType.PromptField = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETADAA") || Equal(global.Command, "RETLTRB"))
    {
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
    if (!Equal(global.Command, "DISPLAY"))
    {
      if (!import.AdminAction.IsEmpty)
      {
        export.AdminAction.Index = -1;

        for(import.AdminAction.Index = 0; import.AdminAction.Index < import
          .AdminAction.Count; ++import.AdminAction.Index)
        {
          ++export.AdminAction.Index;
          export.AdminAction.CheckSize();

          export.AdminAction.Update.DetailHiddenOblgr.Number =
            import.AdminAction.Item.DetailHiddenOblgr.Number;
          export.AdminAction.Update.DetailCsePersonsWorkSet.FormattedName =
            import.AdminAction.Item.DetailCsePersonsWorkSet.FormattedName;
          export.AdminAction.Update.DetailHiddenCrtfd.SelectChar =
            import.AdminAction.Item.DetailHiddenCrtfd.SelectChar;
          export.AdminAction.Update.DetailObligation.SystemGeneratedIdentifier =
            import.AdminAction.Item.DetailObligation.SystemGeneratedIdentifier;
          export.AdminAction.Update.DetailAdministrativeActCertification.Assign(
            import.AdminAction.Item.DetailAdministrativeActCertification);
          export.AdminAction.Update.DetailAdministrativeAction.Type1 =
            import.AdminAction.Item.DetailAdministrativeAction.Type1;
          MoveAdministrativeActCertification(import.AdminAction.Item.
            DetailFederalDebtSetoff,
            export.AdminAction.Update.DetailFederalDebtSetoff);
          export.AdminAction.Update.DetailObligationAdministrativeAction.
            TakenDate =
              import.AdminAction.Item.DetailObligationAdministrativeAction.
              TakenDate;
        }
      }

      if (!import.SelOpt.IsEmpty)
      {
        export.SelOpt.Index = -1;

        for(import.SelOpt.Index = 0; import.SelOpt.Index < import.SelOpt.Count; ++
          import.SelOpt.Index)
        {
          ++export.SelOpt.Index;
          export.SelOpt.CheckSize();

          export.SelOpt.Update.DetailSelOpt.SelectChar =
            import.SelOpt.Item.DetailSelOpt.SelectChar;
        }
      }
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETADAA") && !
      Equal(global.Command, "RETLTRB") && !Equal(global.Command, "RETCDVL"))
    {
      export.PromptAdminActType.PromptField = "";
      export.PromptTribunal.PromptField = "";
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        // *** This is handled at the beginning of the procedure step.
        break;
      case "RETLTRB":
        export.PromptTribunal.PromptField = "";

        if (IsEmpty(import.Fips.StateAbbreviation))
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Protected = false;
          field.Focused = true;
        }
        else if (IsEmpty(import.Fips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.OptionAutoManualAdmAc, "oneChar");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "RETADAA":
        break;
      case "LIST":
        if (!IsEmpty(export.PromptTribunal.PromptField) && AsChar
          (export.PromptTribunal.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptTribunal, "promptField");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.PromptAdminActType.PromptField) && AsChar
          (export.PromptAdminActType.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptAdminActType, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.PromptAdminActType.PromptField) == 'S' && AsChar
          (export.PromptTribunal.PromptField) == 'S')
        {
          ExitState = "ZD_ACO_NE0_INVALID_MULT_PROMPT_S";

          var field1 = GetField(export.PromptAdminActType, "promptField");

          field1.Error = true;

          var field2 = GetField(export.PromptTribunal, "promptField");

          field2.Error = true;

          return;
        }

        if (AsChar(export.PromptAdminActType.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

          return;
        }

        if (AsChar(export.PromptTribunal.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

          return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "DISPLAY":
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_COURT_CASE_NO_RQD";

          return;
        }

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          if (export.Tribunal.Identifier == 0)
          {
            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

            var field = GetField(export.PromptTribunal, "promptField");

            field.Error = true;

            return;
          }
        }
        else
        {
          if (IsEmpty(export.Fips.CountyAbbreviation))
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;

            ExitState = "LE0000_TRIB_COUNTY_REQUIRED";

            return;
          }

          if (!ReadFips1())
          {
            if (ReadFips2())
            {
              var field = GetField(export.Fips, "countyAbbreviation");

              field.Error = true;

              ExitState = "INVALID_COUNTY_CODE";

              return;
            }
            else
            {
              var field = GetField(export.Fips, "stateAbbreviation");

              field.Error = true;

              ExitState = "ZD_CO0000_INVALID_STATE_CODE_2";

              return;
            }
          }
        }

        UseLeListAdminActByCrtCase();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.LegalAction.Identifier != 0)
          {
            UseLeGetPetitionerRespondent();
          }

          if (export.AdminAction.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
          }
          else
          {
            if (export.AdminAction.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_IS_FULL";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }

            export.AdminAction.Index = 0;

            for(var limit = local.Count.Count; export.AdminAction.Index < limit
              ; ++export.AdminAction.Index)
            {
              if (!export.AdminAction.CheckSize())
              {
                break;
              }

              export.SelOpt.Index = export.AdminAction.Index;
              export.SelOpt.CheckSize();

              export.SelOpt.Update.DetailSelOpt.SelectChar = "";

              // ****************************************************************
              // 10/20/99	R.Jean	PR74995 - Move the taken date to the curr amt 
              // date (displayed on screen) if the type is not FDSO, SDSO, CRED.
              // ****************************************************************
              if (!Equal(export.AdminAction.Item.
                DetailAdministrativeActCertification.Type1, "FDSO") && !
                Equal(export.AdminAction.Item.
                  DetailAdministrativeActCertification.Type1, "SDSO") && !
                Equal(export.AdminAction.Item.
                  DetailAdministrativeActCertification.Type1, "CRED"))
              {
                export.AdminAction.Update.DetailAdministrativeActCertification.
                  CurrentAmountDate =
                    export.AdminAction.Item.
                    DetailAdministrativeActCertification.TakenDate;
              }
            }

            export.AdminAction.CheckIndex();
          }
        }
        else if (IsExitState("LE0000_INVALID_CT_CASE_NO_N_TRIB"))
        {
          var field1 = GetField(export.LegalAction, "courtCaseNumber");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.Fips, "countyAbbreviation");

          field3.Error = true;
        }
        else if (IsExitState("LE0000_INVALID_ADMIN_ACTION_TYPE"))
        {
          var field = GetField(export.Required, "type1");

          field.Error = true;
        }
        else if (IsExitState("LE0000_INVALID_OPT_MANUAL_AUTO"))
        {
          var field = GetField(export.OptionAutoManualAdmAc, "oneChar");

          field.Error = true;
        }
        else if (IsExitState("LE0000_LIST_NOT_MATCH_TYPE"))
        {
          var field1 = GetField(export.OptionAutoManualAdmAc, "oneChar");

          field1.Error = true;

          var field2 = GetField(export.Required, "type1");

          field2.Error = true;
        }
        else
        {
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "NO_MORE_ITEMS_TO_SCROLL";

        break;
      case "NEXT":
        ExitState = "NO_MORE_ITEMS_TO_SCROLL";

        break;
      case "DETAIL":
        local.Selected.Count = 0;

        for(export.SelOpt.Index = 0; export.SelOpt.Index < export.SelOpt.Count; ++
          export.SelOpt.Index)
        {
          if (!export.SelOpt.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.SelOpt.Item.DetailSelOpt.SelectChar) && AsChar
            (export.SelOpt.Item.DetailSelOpt.SelectChar) != 'S')
          {
            var field = GetField(export.SelOpt.Item.DetailSelOpt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }

          if (AsChar(export.SelOpt.Item.DetailSelOpt.SelectChar) == 'S')
          {
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field =
                GetField(export.SelOpt.Item.DetailSelOpt, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

              return;
            }

            export.AdminAction.Index = export.SelOpt.Index;
            export.AdminAction.CheckSize();
          }
        }

        export.SelOpt.CheckIndex();

        if (local.Selected.Count == 0)
        {
          ExitState = "LE0000_ADMIN_ACT_MUST_BE_SEL";

          return;
        }

        export.DlgflwSelectedObligor.FormattedName =
          export.AdminAction.Item.DetailCsePersonsWorkSet.FormattedName;
        export.DlgflwSelectedObligor.Number =
          export.AdminAction.Item.DetailHiddenOblgr.Number;
        export.SelectedLegalAction.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber;
        export.SelectedObligation.SystemGeneratedIdentifier =
          export.AdminAction.Item.DetailObligation.SystemGeneratedIdentifier;
        export.SelectedAdministrativeAction.Type1 =
          export.AdminAction.Item.DetailAdministrativeAction.Type1;
        export.SelectedObligationAdministrativeAction.TakenDate =
          export.AdminAction.Item.DetailObligationAdministrativeAction.
            TakenDate;

        if (AsChar(export.AdminAction.Item.DetailHiddenCrtfd.SelectChar) == 'Y')
        {
          export.SelectedAdministrativeActCertification.Assign(
            export.AdminAction.Item.DetailAdministrativeActCertification);
          local.CertifiedAction.Flag = "Y";
        }
        else
        {
          local.CertifiedAction.Flag = "N";
          export.SelectedObligationAdministrativeAction.TakenDate =
            export.AdminAction.Item.DetailAdministrativeActCertification.
              TakenDate;
        }

        if (AsChar(local.CertifiedAction.Flag) == 'Y')
        {
          // ---------------------------------------------
          // Depending on the type, flow to different detail screens.
          // ---------------------------------------------
          UseLeSetExitstateForListObliga();
        }
        else
        {
          export.DlgflwIadaAllObligatns.Flag = "Y";
          ExitState = "ECO_LNK_TO_IADA_IDENTIFY_ADM_ACT";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAdminActions(LeListAdminActByCrtCase.Export.
    AdminActionsGroup source, Export.AdminActionGroup target)
  {
    target.DetailCsePersonsWorkSet.FormattedName =
      source.DetailCsePersonsWorkSet.FormattedName;
    target.DetailHiddenCrtfd.SelectChar = source.DetailCertified.SelectChar;
    target.DetailHiddenOblgr.Number = source.DetailObligor.Number;
    target.DetailObligation.SystemGeneratedIdentifier =
      source.DetailObligation.SystemGeneratedIdentifier;
    target.DetailAdministrativeAction.Type1 =
      source.DetailAdministrativeAction.Type1;
    target.DetailAdministrativeActCertification.Assign(
      source.DetailAdministrativeActCertification);
    MoveAdministrativeActCertification(source.DetailFederalDebtSetoff,
      target.DetailFederalDebtSetoff);
    target.DetailObligationAdministrativeAction.TakenDate =
      source.DetailObligationAdministrativeAction.TakenDate;
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeListAdminActByCrtCase()
  {
    var useImport = new LeListAdminActByCrtCase.Import();
    var useExport = new LeListAdminActByCrtCase.Export();

    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.ListOptManualSystAct.OneChar =
      export.OptionAutoManualAdmAc.OneChar;
    useImport.LegalAction.CourtCaseNumber = export.LegalAction.CourtCaseNumber;
    useImport.Required.Type1 = export.Required.Type1;
    useImport.StartDate.Date = export.StartDate.Date;

    Call(LeListAdminActByCrtCase.Execute, useImport, useExport);

    local.Count.Count = useExport.Common.Count;
    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.Assign(useExport.Fips);
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
    useExport.AdminActions.CopyTo(export.AdminAction, MoveAdminActions);
  }

  private void UseLeSetExitstateForListObliga()
  {
    var useImport = new LeSetExitstateForListObliga.Import();
    var useExport = new LeSetExitstateForListObliga.Export();

    useImport.AdministrativeActCertification.Type1 =
      export.SelectedAdministrativeActCertification.Type1;

    Call(LeSetExitstateForListObliga.Execute, useImport, useExport);

    local.ErrorSettingExitState.Flag = useExport.Error.Flag;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadFips1()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 3);
        entities.Existing.StateAbbreviation = db.GetString(reader, 4);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 3);
        entities.Existing.StateAbbreviation = db.GetString(reader, 4);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Existing.Populated = true;
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
    /// <summary>A SelOptGroup group.</summary>
    [Serializable]
    public class SelOptGroup
    {
      /// <summary>
      /// A value of DetailSelOpt.
      /// </summary>
      [JsonPropertyName("detailSelOpt")]
      public Common DetailSelOpt
      {
        get => detailSelOpt ??= new();
        set => detailSelOpt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelOpt;
    }

    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailHiddenCrtfd.
      /// </summary>
      [JsonPropertyName("detailHiddenCrtfd")]
      public Common DetailHiddenCrtfd
      {
        get => detailHiddenCrtfd ??= new();
        set => detailHiddenCrtfd = value;
      }

      /// <summary>
      /// A value of DetailHiddenOblgr.
      /// </summary>
      [JsonPropertyName("detailHiddenOblgr")]
      public CsePerson DetailHiddenOblgr
      {
        get => detailHiddenOblgr ??= new();
        set => detailHiddenOblgr = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Common detailHiddenCrtfd;
      private CsePerson detailHiddenOblgr;
      private Obligation detailObligation;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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
    /// A value of OptionAutoManualAdmAc.
    /// </summary>
    [JsonPropertyName("optionAutoManualAdmAc")]
    public Standard OptionAutoManualAdmAc
    {
      get => optionAutoManualAdmAc ??= new();
      set => optionAutoManualAdmAc = value;
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
    /// A value of PromptAdminActType.
    /// </summary>
    [JsonPropertyName("promptAdminActType")]
    public Standard PromptAdminActType
    {
      get => promptAdminActType ??= new();
      set => promptAdminActType = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
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
    /// Gets a value of SelOpt.
    /// </summary>
    [JsonIgnore]
    public Array<SelOptGroup> SelOpt => selOpt ??= new(SelOptGroup.Capacity);

    /// <summary>
    /// Gets a value of SelOpt for json serialization.
    /// </summary>
    [JsonPropertyName("selOpt")]
    [Computed]
    public IList<SelOptGroup> SelOpt_Json
    {
      get => selOpt;
      set => SelOpt.Assign(value);
    }

    /// <summary>
    /// Gets a value of AdminAction.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionGroup> AdminAction => adminAction ??= new(
      AdminActionGroup.Capacity);

    /// <summary>
    /// Gets a value of AdminAction for json serialization.
    /// </summary>
    [JsonPropertyName("adminAction")]
    [Computed]
    public IList<AdminActionGroup> AdminAction_Json
    {
      get => adminAction;
      set => AdminAction.Assign(value);
    }

    private FipsTribAddress foreign;
    private Standard standard;
    private Standard promptTribunal;
    private Tribunal tribunal;
    private Fips fips;
    private Standard optionAutoManualAdmAc;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard promptAdminActType;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private LegalAction legalAction;
    private Array<SelOptGroup> selOpt;
    private Array<AdminActionGroup> adminAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SelOptGroup group.</summary>
    [Serializable]
    public class SelOptGroup
    {
      /// <summary>
      /// A value of DetailSelOpt.
      /// </summary>
      [JsonPropertyName("detailSelOpt")]
      public Common DetailSelOpt
      {
        get => detailSelOpt ??= new();
        set => detailSelOpt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelOpt;
    }

    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailHiddenCrtfd.
      /// </summary>
      [JsonPropertyName("detailHiddenCrtfd")]
      public Common DetailHiddenCrtfd
      {
        get => detailHiddenCrtfd ??= new();
        set => detailHiddenCrtfd = value;
      }

      /// <summary>
      /// A value of DetailHiddenOblgr.
      /// </summary>
      [JsonPropertyName("detailHiddenOblgr")]
      public CsePerson DetailHiddenOblgr
      {
        get => detailHiddenOblgr ??= new();
        set => detailHiddenOblgr = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Common detailHiddenCrtfd;
      private CsePerson detailHiddenOblgr;
      private Obligation detailObligation;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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
    /// A value of DlgflwSelectedObligor.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedObligor")]
    public CsePersonsWorkSet DlgflwSelectedObligor
    {
      get => dlgflwSelectedObligor ??= new();
      set => dlgflwSelectedObligor = value;
    }

    /// <summary>
    /// A value of DlgflwIadaAllObligatns.
    /// </summary>
    [JsonPropertyName("dlgflwIadaAllObligatns")]
    public Common DlgflwIadaAllObligatns
    {
      get => dlgflwIadaAllObligatns ??= new();
      set => dlgflwIadaAllObligatns = value;
    }

    /// <summary>
    /// A value of OptionAutoManualAdmAc.
    /// </summary>
    [JsonPropertyName("optionAutoManualAdmAc")]
    public Standard OptionAutoManualAdmAc
    {
      get => optionAutoManualAdmAc ??= new();
      set => optionAutoManualAdmAc = value;
    }

    /// <summary>
    /// A value of PromptAdminActType.
    /// </summary>
    [JsonPropertyName("promptAdminActType")]
    public Standard PromptAdminActType
    {
      get => promptAdminActType ??= new();
      set => promptAdminActType = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of SelectedAdministrativeAction.
    /// </summary>
    [JsonPropertyName("selectedAdministrativeAction")]
    public AdministrativeAction SelectedAdministrativeAction
    {
      get => selectedAdministrativeAction ??= new();
      set => selectedAdministrativeAction = value;
    }

    /// <summary>
    /// A value of SelectedObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("selectedObligationAdministrativeAction")]
    public ObligationAdministrativeAction SelectedObligationAdministrativeAction
    {
      get => selectedObligationAdministrativeAction ??= new();
      set => selectedObligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of SelectedAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("selectedAdministrativeActCertification")]
    public AdministrativeActCertification SelectedAdministrativeActCertification
    {
      get => selectedAdministrativeActCertification ??= new();
      set => selectedAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of SelectedObligation.
    /// </summary>
    [JsonPropertyName("selectedObligation")]
    public Obligation SelectedObligation
    {
      get => selectedObligation ??= new();
      set => selectedObligation = value;
    }

    /// <summary>
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of SelectedObligor.
    /// </summary>
    [JsonPropertyName("selectedObligor")]
    public CsePerson SelectedObligor
    {
      get => selectedObligor ??= new();
      set => selectedObligor = value;
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
    /// Gets a value of SelOpt.
    /// </summary>
    [JsonIgnore]
    public Array<SelOptGroup> SelOpt => selOpt ??= new(SelOptGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SelOpt for json serialization.
    /// </summary>
    [JsonPropertyName("selOpt")]
    [Computed]
    public IList<SelOptGroup> SelOpt_Json
    {
      get => selOpt;
      set => SelOpt.Assign(value);
    }

    /// <summary>
    /// Gets a value of AdminAction.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionGroup> AdminAction => adminAction ??= new(
      AdminActionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AdminAction for json serialization.
    /// </summary>
    [JsonPropertyName("adminAction")]
    [Computed]
    public IList<AdminActionGroup> AdminAction_Json
    {
      get => adminAction;
      set => AdminAction.Assign(value);
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

    private FipsTribAddress foreign;
    private Standard standard;
    private Standard promptTribunal;
    private Tribunal tribunal;
    private Fips fips;
    private CsePersonsWorkSet dlgflwSelectedObligor;
    private Common dlgflwIadaAllObligatns;
    private Standard optionAutoManualAdmAc;
    private Standard promptAdminActType;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private AdministrativeAction selectedAdministrativeAction;
    private ObligationAdministrativeAction selectedObligationAdministrativeAction;
      
    private AdministrativeActCertification selectedAdministrativeActCertification;
      
    private Obligation selectedObligation;
    private LegalAction selectedLegalAction;
    private CsePerson selectedObligor;
    private LegalAction legalAction;
    private Array<SelOptGroup> selOpt;
    private Array<AdminActionGroup> adminAction;
    private PetitionerRespondentDetails petitionerRespondentDetails;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ErrorSettingExitState.
    /// </summary>
    [JsonPropertyName("errorSettingExitState")]
    public Common ErrorSettingExitState
    {
      get => errorSettingExitState ??= new();
      set => errorSettingExitState = value;
    }

    /// <summary>
    /// A value of CertifiedAction.
    /// </summary>
    [JsonPropertyName("certifiedAction")]
    public Common CertifiedAction
    {
      get => certifiedAction ??= new();
      set => certifiedAction = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    private Common count;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ae;
    private AbendData abendData;
    private Common errorSettingExitState;
    private Common certifiedAction;
    private Common selected;
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
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Fips existing;
  }
#endregion
}
