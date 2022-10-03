// Program: SI_CPAT_CHILD_PATERNITY_MAINT, ID: 374382716, model: 746.
// Short name: SWECPATP
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
/// A program: SI_CPAT_CHILD_PATERNITY_MAINT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists, adds, and updates the role of CSE PERSONs on a 
/// specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiCpatChildPaternityMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CPAT_CHILD_PATERNITY_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCpatChildPaternityMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCpatChildPaternityMaint.
  /// </summary>
  public SiCpatChildPaternityMaint(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    // 			C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 02/23/00  W. Campbell  WR#000160	Initial Development - This Procedure was
    // created by
    // 					copying the ROLE procedure.  The scrolling in this
    // 					procedure is very similar to that in ROLE.
    // 11/21/00  M. Lachowicz	WR 298		Change header line.
    // 08/30/01  M. Lachowicz	PR#126467	Allow to update closed cases.
    // 06/28/02  KCole		WO#20338	The user must change BOW indicator to something
    // other
    // 					than "U" if they entered the screen as part of the
    // 					flow when adding a legal action.
    // 10/10/02  K.Doshi			Prevent father's name being entered when
    // 					BC_signed_flag=SPACES.
    // 09/22/03  GVandy	PR186785	Support new flow from LOPS.
    // 09/24/03  GVandy	PR188676	Protect child paternity info if the AP is 
    // female and
    // 					the child is on another case with a male AP.
    // 05-08-06  GVandy	WR230751	Add Hospital Paternity Establishment Indicator.
    // 05/08/08  Arun Mathias  CQ#421         Automatic flow from CRPA, if the 
    // born out of wedlock
    //                                         
    // or the CSE to establish indicators are
    // set to "U".
    // 05/10/17  GVandy	CQ48108		Changes to support IV-D PEP.
    // 08/01/17  GVandy	CQ58765		Modify IV-D PEP edit to allow paternity
    // 					established ind to be changed "Y" when there
    // 					is an EP legal detail.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    export.LegalActionFlow.Flag = import.LegalActionFlow.Flag;

    // *** CQ#421 Added CRPA flag
    if (Equal(global.Command, "CLEAR") && IsEmpty(import.FromLrol.Flag) && IsEmpty
      (import.FromRegi.Flag) && IsEmpty(import.FromRole.Flag) && IsEmpty
      (export.LegalActionFlow.Flag) && IsEmpty(import.FromLops.Flag) && IsEmpty
      (import.FromCrpa.Flag))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveStandard(import.Standard, export.Standard);
    export.Next.Number = import.Next.Number;
    MoveCase1(import.Case1, export.Case1);
    MoveOffice(import.Office, export.Office);
    export.HeaderLine.Text35 = import.HeaderLine.Text35;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.FromLrol.Flag = import.FromLrol.Flag;
    export.FromRegi.Flag = import.FromRegi.Flag;
    export.FromRole.Flag = import.FromRole.Flag;
    export.FromLops.Flag = import.FromLops.Flag;
    export.SuccessfulDisplay.Flag = import.SuccessfulDisplay.Flag;

    // ***CQ#421 Added CRPA flag and first display from crpa flag
    export.FromCrpa.Flag = import.FromCrpa.Flag;
    export.FirstDisplayFromCrpa.Flag = import.FirstDisplayFromCrpa.Flag;

    if (!Equal(export.Next.Number, export.Case1.Number))
    {
      export.SuccessfulDisplay.Flag = "";
    }

    MoveCsePersonsWorkSet2(import.Ap, export.Ap);
    export.Ar.Assign(import.Ar);

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailCsePerson.Assign(
        import.Import1.Item.DetailCsePerson);
      export.Export1.Update.DetailCsePersonsWorkSet.Assign(
        import.Import1.Item.DetailCsePersonsWorkSet);
      export.Export1.Update.DetailCaseRole.Assign(
        import.Import1.Item.DetailCaseRole);

      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == '*')
      {
        export.Export1.Update.DetailCommon.SelectChar = "";
      }
    }

    import.Import1.CheckIndex();

    // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Protect paternity info when 
    // paternity is locked.
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (AsChar(export.Export1.Item.DetailCsePerson.PaternityLockInd) == 'Y')
      {
        var field1 =
          GetField(export.Export1.Item.DetailCsePerson, "bornOutOfWedlock");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Export1.Item.DetailCsePerson, "cseToEstblPaternity");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.DetailCsePerson,
          "paternityEstablishedIndicator");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Export1.Item.DetailCsePerson,
          "birthCertificateSignature");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.DetailCsePerson, "hospitalPatEstInd");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 =
          GetField(export.Export1.Item.DetailCsePerson,
          "birthCertFathersLastName");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 =
          GetField(export.Export1.Item.DetailCsePerson,
          "birthCertFathersFirstName");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 =
          GetField(export.Export1.Item.DetailCsePerson, "birthCertFathersMi");

        field8.Color = "cyan";
        field8.Protected = true;
      }
    }

    export.Export1.CheckIndex();

    // 09/24/2003  GVandy  PR188676  Protect child paternity info if the AP is 
    // female and the child is on another case with a male AP.
    if (AsChar(export.Ap.Sex) == 'F' && !Equal(global.Command, "DISPLAY"))
    {
      export.Export1.Index = 0;

      for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
        export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        foreach(var item in ReadCsePerson2())
        {
          local.ApOnOtherCase.Number = entities.ApCsePerson.Number;
          local.ApOnOtherCase.Sex = "";
          UseCabReadAdabasPerson();

          if (AsChar(local.ApOnOtherCase.Sex) == 'M')
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePerson, "bornOutOfWedlock");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.DetailCsePerson,
              "cseToEstblPaternity");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.DetailCsePerson,
              "paternityEstablishedIndicator");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertificateSignature");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.DetailCsePerson, "hospitalPatEstInd");
              

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertFathersLastName");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertFathersFirstName");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Export1.Item.DetailCsePerson, "birthCertFathersMi");
              

            field8.Color = "cyan";
            field8.Protected = true;

            break;
          }
        }
      }

      export.Export1.CheckIndex();
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCsePerson.Number =
        import.HiddenPageKeys.Item.HiddenPageKeyCsePerson.Number;
      MoveCaseRole(import.HiddenPageKeys.Item.HiddenPageKeyCaseRole,
        export.HiddenPageKeys.Update.HiddenPageKeyCaseRole);
    }

    import.HiddenPageKeys.CheckIndex();

    if (import.HiddenStandard.PageNumber == 0)
    {
      export.HiddenStandard.PageNumber = 1;

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
        UseCabSetMaximumDiscontinueDate();
    }

    if (Equal(global.Command, "CLEAR"))
    {
      if (AsChar(export.FromLrol.Flag) == 'Y')
      {
        ExitState = "SI0000_MUST_RETURN_TO_LROL";

        return;
      }

      if (AsChar(export.FromRegi.Flag) == 'Y')
      {
        ExitState = "SI0000_MUST_RETURN_TO_REGI";

        return;
      }

      if (AsChar(export.FromRole.Flag) == 'Y')
      {
        ExitState = "SI0000_MUST_RETURN_TO_ROLE";

        return;
      }

      // ***CQ#421 Added CRPA flag
      if (AsChar(export.FromCrpa.Flag) == 'Y')
      {
        ExitState = "SP0000_MUST_RETURN_TO_CRPA";

        return;
      }

      if (AsChar(export.LegalActionFlow.Flag) == 'Y' || AsChar
        (import.FromLops.Flag) == 'Y')
      {
        // If user in process of adding a legal action, must check paternity 
        // indicators and continue with adding the LA.
        ExitState = "LE0000_USE_PF9_TO_RETURN";
      }
    }

    // ---------------------------------------------
    //       N E X T   T R A N  O U T G O I N G
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (AsChar(export.FromLrol.Flag) == 'Y')
      {
        export.Standard.NextTransaction = "";
        ExitState = "SI0000_MUST_RETURN_TO_LROL";

        return;
      }

      if (AsChar(export.FromRegi.Flag) == 'Y')
      {
        export.Standard.NextTransaction = "";
        ExitState = "SI0000_MUST_RETURN_TO_REGI";

        return;
      }

      if (AsChar(export.FromRole.Flag) == 'Y')
      {
        export.Standard.NextTransaction = "";
        ExitState = "SI0000_MUST_RETURN_TO_ROLE";

        return;
      }

      // ***CQ#421 Added CRPA flag
      if (AsChar(export.FromCrpa.Flag) == 'Y')
      {
        export.Standard.NextTransaction = "";
        ExitState = "SP0000_MUST_RETURN_TO_CRPA";

        return;
      }

      if (AsChar(export.LegalActionFlow.Flag) == 'Y' || AsChar
        (export.FromLops.Flag) == 'Y')
      {
        export.Standard.NextTransaction = "";
        ExitState = "LE0000_USE_PF9_TO_RETURN";

        if (Equal(global.Command, "ENTER"))
        {
          // Changing command so enter will get thru the case of command without
          // resulting in invalid command.
          global.Command = "CLEAR";
        }

        goto Test1;
      }

      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

Test1:

    // ------------------------------------------------------------
    // If the case is closed, protect all fields except the
    // selection character.
    // ------------------------------------------------------------
    local.ErrOnAdabasUnavailable.Flag = "Y";
    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK") && !
      IsExitState("LE0000_USE_PF9_TO_RETURN"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //       N E X T   T R A N  I N C O M I N G
    // ---------------------------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "CHDS") || Equal(global.Command, "RETNATE"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK") && !
        IsExitState("LE0000_USE_PF9_TO_RETURN"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // For UPDATE, determine if a row has been selected and modified by the user
    // for processing.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "LOCK") || Equal
      (global.Command, "UNLOCK"))
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (!import.Import1.CheckSize())
        {
          break;
        }

        export.Export1.Index = import.Import1.Index;
        export.Export1.CheckSize();

        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

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
      }

      import.Import1.CheckIndex();

      switch(local.Common.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        case 1:
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // ---------------------------------------------
        // This command allows the user to link to a
        // selection list and retrieve the appropriate
        // value, not losing any of the data already
        // entered.
        // ---------------------------------------------
        if (AsChar(import.ApPrompt.SelectChar) == 'S')
        {
          ++local.Invalid.Count;
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
        }
        else
        {
          var field = GetField(export.ApPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          ++local.Invalid.Count;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ApPrompt, "selectChar");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        return;
      case "EXIT":
        if (AsChar(export.FromRegi.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_REGI";

          return;
        }

        if (AsChar(export.FromLrol.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_LROL";

          return;
        }

        if (AsChar(export.FromRole.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_ROLE";

          return;
        }

        // ***CQ#421 Added CRPA flag
        if (AsChar(export.FromCrpa.Flag) == 'Y')
        {
          ExitState = "SP0000_MUST_RETURN_TO_CRPA";

          return;
        }

        if (AsChar(export.LegalActionFlow.Flag) == 'Y' || AsChar
          (export.FromLops.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          break;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKeyCsePerson.Number))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "UPDATE":
        if (AsChar(export.SuccessfulDisplay.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Cannot update locked 
              // paternity info.
              if (AsChar(export.Export1.Item.DetailCsePerson.PaternityLockInd) ==
                'Y')
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Error = true;

                ExitState = "SI0000_CANNOT_UPDATE_PAT_LOCKED";

                return;
              }

              // -------------------------------------------
              // Validate that Hospital Paternity Established Indicator
              // is Y or N or spaces.
              // -------------------------------------------
              if (AsChar(export.Export1.Item.DetailCsePerson.HospitalPatEstInd) ==
                'Y' || AsChar
                (export.Export1.Item.DetailCsePerson.HospitalPatEstInd) == 'N'
                || IsEmpty
                (export.Export1.Item.DetailCsePerson.HospitalPatEstInd))
              {
                // -------------------------------------------
                // Hospital Paternity Established Indicator OK,
                // Keep on working.
                // -------------------------------------------
              }
              else
              {
                // -------------------------------------------
                // Invalid Hospital Paternity Established Indicator
                // Must be Y or N or spaces.
                // -------------------------------------------
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "hospitalPatEstInd");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                return;
              }

              // -------------------------------------------
              // Validate that Birth Certificate Signature
              // is Y or N or spaces.
              // -------------------------------------------
              if (AsChar(export.Export1.Item.DetailCsePerson.
                BirthCertificateSignature) == 'Y' || AsChar
                (export.Export1.Item.DetailCsePerson.BirthCertificateSignature) ==
                  'N' || IsEmpty
                (export.Export1.Item.DetailCsePerson.BirthCertificateSignature))
              {
                // -------------------------------------------
                // Birth Certificate Signature OK,
                // Keep on working.
                // -------------------------------------------
              }
              else
              {
                // -------------------------------------------
                // Invalid Birth Certificate Signature
                // Must be Y or N or spaces.
                // -------------------------------------------
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "birthCertificateSignature");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                return;
              }

              if (AsChar(export.Export1.Item.DetailCsePerson.
                BirthCertificateSignature) == 'Y' && (
                  IsEmpty(export.Export1.Item.DetailCsePerson.
                  BirthCertFathersFirstName) || IsEmpty
                (export.Export1.Item.DetailCsePerson.BirthCertFathersLastName)))
              {
                if (IsEmpty(export.Export1.Item.DetailCsePerson.
                  BirthCertFathersFirstName))
                {
                  var field1 =
                    GetField(export.Export1.Item.DetailCsePerson,
                    "birthCertFathersFirstName");

                  field1.Error = true;
                }

                if (IsEmpty(export.Export1.Item.DetailCsePerson.
                  BirthCertFathersLastName))
                {
                  var field1 =
                    GetField(export.Export1.Item.DetailCsePerson,
                    "birthCertFathersLastName");

                  field1.Error = true;
                }

                ExitState = "SI0000_NAME_MUST_BE_ENTERED";

                return;
              }

              // ---------------------------------------------------
              // Prevent name being entered when sign_flag is SPACES.
              // Changed 'sign_flag=N' check to 'sign_flag<>Y'.
              // ---------------------------------------------------
              if (AsChar(export.Export1.Item.DetailCsePerson.
                BirthCertificateSignature) != 'Y' && (
                  !IsEmpty(export.Export1.Item.DetailCsePerson.
                  BirthCertFathersFirstName) || !
                IsEmpty(export.Export1.Item.DetailCsePerson.BirthCertFathersMi) ||
                !
                IsEmpty(export.Export1.Item.DetailCsePerson.
                  BirthCertFathersLastName)))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "birthCertificateSignature");

                field1.Error = true;

                ExitState = "SI0000_BC_FLAG_MUST_BE_Y";

                return;
              }

              // -------------------------------------------
              // Validate that Born Out Of Wedlock
              // is (Y)es, (N)o or (U)nknown.
              // -------------------------------------------
              if (AsChar(export.Export1.Item.DetailCsePerson.BornOutOfWedlock) ==
                'Y' || AsChar
                (export.Export1.Item.DetailCsePerson.BornOutOfWedlock) == 'N'
                || AsChar
                (export.Export1.Item.DetailCsePerson.BornOutOfWedlock) == 'U')
              {
                // -------------------------------------------
                // Born Out Of Wedlock OK,
                // Keep on working.
                // -------------------------------------------
              }
              else
              {
                // -------------------------------------------
                // Invalid Born Out Of Wedlock,
                // must be (Y)es, (N)o or (U)nknown..
                // -------------------------------------------
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "bornOutOfWedlock");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YNU";

                return;
              }

              // -------------------------------------------
              // Validate that CSE To Estbl Patern
              // is (Y)es, (N)o or (U)nknown.
              // -------------------------------------------
              if (AsChar(export.Export1.Item.DetailCsePerson.CseToEstblPaternity)
                == 'Y' || AsChar
                (export.Export1.Item.DetailCsePerson.CseToEstblPaternity) == 'N'
                || AsChar
                (export.Export1.Item.DetailCsePerson.CseToEstblPaternity) == 'U'
                )
              {
                // -------------------------------------------
                // CSE To Estbl Patern OK,
                // Keep on working.
                // -------------------------------------------
              }
              else
              {
                // -------------------------------------------
                // Invalid CSE To Estbl Patern,
                // must be (Y)es, (N)o or (U)nknown..
                // -------------------------------------------
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "cseToEstblPaternity");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YNU";

                return;
              }

              // -------------------------------------------
              // Validate that Paternity Established Indicator
              // is (Y)es or (N)o.
              // -------------------------------------------
              if (AsChar(export.Export1.Item.DetailCsePerson.
                PaternityEstablishedIndicator) == 'Y' || AsChar
                (export.Export1.Item.DetailCsePerson.
                  PaternityEstablishedIndicator) == 'N')
              {
                // -------------------------------------------
                // Paternity Established Indicator OK,
                // Keep on working.
                // -------------------------------------------
              }
              else
              {
                // -------------------------------------------
                // Invalid Paternity Established Indicator,
                // must be (Y)es or (N)o.
                // -------------------------------------------
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "paternityEstablishedIndicator");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                return;
              }

              // -------------------------------------------
              // Validate that Born Out Of Wedlock,
              // CSE To Estbl Patern and Paternity Established
              // all have values which are acceptable when
              // taken together as a set.
              // -------------------------------------------
              UseSiCabValidatePaternityComb();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "bornOutOfWedlock");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "cseToEstblPaternity");

                field2.Error = true;

                var field3 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "paternityEstablishedIndicator");

                field3.Error = true;

                return;
              }

              // -------------------------------------------
              // Read the Child's data for update.
              // -------------------------------------------
              if (ReadCsePerson1())
              {
                // 08/01/17  GVandy  CQ58765  Modify IV-D PEP edit to allow 
                // paternity established ind to be changed "Y" when there is an
                // EP legal detail.
                if (AsChar(export.Export1.Item.DetailCsePerson.
                  PaternityEstablishedIndicator) != 'Y')
                {
                  // -- 05/10/2017  GVandy  CQ48108 (IV-D PEP)  Paternity 
                  // Established cannot be "N" if an
                  //    active EP legal detail exists with the child as an 
                  // active LOPS supported person.
                  if (ReadLegalActionPersonLegalAction2())
                  {
                    // -- The child is the supported person on a filed EP legal 
                    // detail.  Paternity Established cannot be "N".
                    var field1 =
                      GetField(export.Export1.Item.DetailCsePerson,
                      "paternityEstablishedIndicator");

                    field1.Error = true;

                    ExitState = "SI0000_PAT_ESTAB_MUST_BE_Y";

                    return;
                  }
                  else
                  {
                    // -- Continue.
                  }
                }

                // -------------------------------------------
                // Determine if any changes to the
                // Paternity type data have occurred.
                // If not, don't update.
                // -------------------------------------------
                // -------------------------------------------
                // Put logic here to detect if the data
                // to be updated has changed or not.
                // -------------------------------------------
                if (Equal(entities.ChCsePerson.BirthCertFathersFirstName,
                  export.Export1.Item.DetailCsePerson.
                    BirthCertFathersFirstName) && Equal
                  (entities.ChCsePerson.BirthCertFathersLastName,
                  export.Export1.Item.DetailCsePerson.
                    BirthCertFathersLastName) && AsChar
                  (entities.ChCsePerson.BirthCertFathersMi) == AsChar
                  (export.Export1.Item.DetailCsePerson.BirthCertFathersMi) && AsChar
                  (entities.ChCsePerson.BirthCertificateSignature) == AsChar
                  (export.Export1.Item.DetailCsePerson.BirthCertificateSignature)
                  && AsChar(entities.ChCsePerson.BornOutOfWedlock) == AsChar
                  (export.Export1.Item.DetailCsePerson.BornOutOfWedlock) && AsChar
                  (entities.ChCsePerson.CseToEstblPaternity) == AsChar
                  (export.Export1.Item.DetailCsePerson.CseToEstblPaternity) && Equal
                  (entities.ChCsePerson.DatePaternEstab,
                  export.Export1.Item.DetailCsePerson.DatePaternEstab) && AsChar
                  (entities.ChCsePerson.PaternityEstablishedIndicator) == AsChar
                  (export.Export1.Item.DetailCsePerson.
                    PaternityEstablishedIndicator))
                {
                  export.Export1.Update.DetailCsePerson.Assign(
                    entities.ChCsePerson);
                  ExitState = "INVALID_UPDATE";

                  goto Test2;
                }

                // -------------------------------------------
                // If the Paternity Established Indicator has changed, then
                // determine the new Date Paternity established value.
                // -------------------------------------------
                export.Export1.Update.DetailCsePerson.DatePaternEstab =
                  entities.ChCsePerson.DatePaternEstab;

                if (AsChar(entities.ChCsePerson.PaternityEstablishedIndicator) ==
                  'Y' && AsChar
                  (export.Export1.Item.DetailCsePerson.
                    PaternityEstablishedIndicator) != 'Y')
                {
                  export.Export1.Update.DetailCsePerson.DatePaternEstab =
                    local.Null1.Date;
                  local.Infrastructure.ReasonCode = "PATNOLONGER";
                }

                if (AsChar(export.Export1.Item.DetailCsePerson.
                  PaternityEstablishedIndicator) == 'Y')
                {
                  // -- 05/10/17 GVandy CQ48108 (IV-D PEP Changes)  Paternity 
                  // can now be established with multiple APs.
                  if (AsChar(entities.ChCsePerson.PaternityEstablishedIndicator) !=
                    'Y')
                  {
                    local.Infrastructure.ReasonCode = "PATESTAB";

                    // 09/24/2003  GVandy  PR186785  Determine the date 
                    // paternity is established.
                    if (ReadLegalActionPersonLegalAction1())
                    {
                      // -- The child is the supported person on a filed EP 
                      // legal detail.  Set the date paternity was established
                      // to the filed date of the EP legal action.
                      export.Export1.Update.DetailCsePerson.DatePaternEstab =
                        entities.LegalAction.FiledDate;
                    }
                    else
                    {
                      // -- The child is NOT a supported person on a filed EP 
                      // legal detail.  Set the date paternity established to
                      // today.
                      export.Export1.Update.DetailCsePerson.DatePaternEstab =
                        local.Current.Date;
                    }
                  }
                }

                try
                {
                  UpdateCsePerson1();
                  export.Export1.Update.DetailCommon.SelectChar = "";

                  // ****************************************************************
                  // Create Life Cycle Infrastructure record for each case where
                  // selected child is active on case when Paternity
                  // Established flag has changed.
                  // ***************************************************************
                  ExitState = "ACO_NN0000_ALL_OK";

                  if (!IsEmpty(local.Infrastructure.ReasonCode))
                  {
                    local.Infrastructure.EventId = 11;
                    UseSiChdsRaiseEvent2();
                  }

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
                  }
                  else
                  {
                    ExitState = "FN0000_ERROR_ON_EVENT_CREATION";
                  }

                  return;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "CSE_PERSON_NU";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "CSE_PERSON_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
              else
              {
                ExitState = "CSE_PERSON_NF";

                return;
              }

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
        }

        export.Export1.CheckIndex();

        break;
      case "CHDS":
        if (AsChar(export.FromRegi.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_REGI";

          return;
        }

        if (AsChar(export.FromLrol.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_LROL";

          return;
        }

        if (AsChar(export.FromRole.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_ROLE";

          return;
        }

        // ***CQ#421 Added CRPA flag
        if (AsChar(export.FromCrpa.Flag) == 'Y')
        {
          ExitState = "SP0000_MUST_RETURN_TO_CRPA";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              export.Export1.Update.DetailCommon.SelectChar = "";
              export.Selected.Number =
                export.Export1.Item.DetailCsePerson.Number;
              ExitState = "ECO_LNK_TO_CHDS";

              return;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      case "DISPLAY":
        break;
      case "SIGNOFF":
        if (AsChar(export.LegalActionFlow.Flag) == 'Y' || AsChar
          (export.FromLops.Flag) == 'Y')
        {
          // Do not allow user to signoff if they came here as part of the flow 
          // to add a legal action
          ExitState = "LE0000_USE_PF9_TO_RETURN";
        }
        else
        {
          UseScCabSignoff();

          return;
        }

        break;
      case "RETURN":
        // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Allow return with BOW=U or 
        // CSE to Estab=U
        // ------------------------------------
        //  Retrofit by RB Mohapatra  01/22/97
        // ------------------------------------
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
              export.Selected.Number =
                export.Export1.Item.DetailCsePerson.Number;

              return;
            }
          }

          export.Export1.CheckIndex();
        }

        break;
      case "CLEAR":
        // case of clear only exists to handle situation where user tried to 
        // next tran away from CPAT while in process of adding a legal action.
        // did not want enter key to give message invalid command - want to have
        // paternity indicators checked to see if valid to continue with adding
        // legal action
        break;
      case "LOCK":
        // -- 05/10/2017 GVandy  CQ48108 (IV-D PEP) Add ability to lock the 
        // paternity information.
        // -- The User must have a role of Supervisor (SS), Program Manager (PM
        // ), or
        //    Trainer (TR) to lock the paternity information.
        if (!ReadOfficeServiceProvider())
        {
          ExitState = "SC0001_USER_NOT_AUTH_COMMAND";

          return;
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            // -- Paternity must be unlocked in order to lock.
            if (AsChar(export.Export1.Item.DetailCsePerson.PaternityLockInd) ==
              'Y')
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "SI0000_CANNOT_LOCK_PATERNITY";

              return;
            }

            // -- Find the selected child.
            if (!ReadCsePerson1())
            {
              ExitState = "CSE_PERSON_NF";

              return;
            }

            // -- Lock the Paternity Info.
            try
            {
              UpdateCsePerson3();
              export.Export1.Update.DetailCsePerson.PaternityLockInd =
                entities.ChCsePerson.PaternityLockInd;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // -- Raise event indicating Paternity was locked.
            local.Infrastructure.EventId = 11;
            local.Infrastructure.ReasonCode = "PATLOCKED";
            UseSiChdsRaiseEvent1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

              return;
            }

            // -- Read infrastructure to pass to NATE.
            if (ReadInfrastructure1())
            {
              export.ToNateInfrastructure.Assign(entities.Infrastructure);
            }
            else
            {
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }

            export.ToNateCsePersonsWorkSet.FormattedName =
              export.Export1.Item.DetailCsePersonsWorkSet.FormattedName;
            export.Previous.Command = global.Command;

            // -- Flow to NATE to add narrative is mandatory for LOCK/UNLOCK 
            // functions.
            export.FlowToNate.Flag = "Y";
            ExitState = "ECO_LNK_TO_NATE";

            return;
          }
          else
          {
          }
        }

        export.Export1.CheckIndex();

        break;
      case "UNLOCK":
        // -- 05/10/2017 GVandy  CQ48108 (IV-D PEP) Add ability to unlock the 
        // paternity information.
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            // -- Paternity must be locked in order to unlock.
            if (AsChar(export.Export1.Item.DetailCsePerson.PaternityLockInd) !=
              'Y')
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "SI0000_CANNOT_UNLOCK_PATERNITY";

              return;
            }

            // -- Find the selected child.
            if (!ReadCsePerson1())
            {
              ExitState = "CSE_PERSON_NF";

              return;
            }

            // -- Lock the Paternity Info.
            try
            {
              UpdateCsePerson2();
              export.Export1.Update.DetailCsePerson.PaternityLockInd =
                entities.ChCsePerson.PaternityLockInd;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // -- Raise event indicating Paternity was unlocked.
            local.Infrastructure.EventId = 11;
            local.Infrastructure.ReasonCode = "PATUNLOCKED";
            UseSiChdsRaiseEvent1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

              return;
            }

            // -- Read infrastructure to pass to NATE.
            if (ReadInfrastructure1())
            {
              export.ToNateInfrastructure.Assign(entities.Infrastructure);
            }
            else
            {
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }

            export.ToNateCsePersonsWorkSet.FormattedName =
              export.Export1.Item.DetailCsePersonsWorkSet.FormattedName;
            export.Previous.Command = global.Command;

            // -- Flow to NATE to add narrative is mandatory for LOCK/UNLOCK 
            // functions.
            export.FlowToNate.Flag = "Y";
            ExitState = "ECO_LNK_TO_NATE";

            return;
          }
          else
          {
          }
        }

        export.Export1.CheckIndex();

        break;
      case "RETNATE":
        // -- 05/10/2017 GVandy  CQ48108 (IV-D PEP) Add ability to unlock the 
        // paternity information.
        // -- The user is returning from the flow to NATE for the LOCK/UNLOCK 
        // functions.
        if (!ReadInfrastructure2())
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCommon.SelectChar = "*";

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Protected = false;
            field.Focused = true;

            local.Previous.CaseNumber = "";

            // -- Add the narrative detail to all other cases for which the LOCK
            // /UNLOCK event was raised.
            foreach(var item in ReadInfrastructure3())
            {
              // --Only create one narrative per case.
              if (Equal(entities.Other.CaseNumber, entities.Original.CaseNumber) ||
                Equal(entities.Other.CaseNumber, local.Previous.CaseNumber))
              {
                continue;
              }

              local.Previous.CaseNumber = entities.Other.CaseNumber;

              foreach(var item1 in ReadNarrativeDetail())
              {
                try
                {
                  CreateNarrativeDetail();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
          }
        }

        export.Export1.CheckIndex();

        switch(TrimEnd(import.Previous.Command))
        {
          case "LOCK":
            ExitState = "SI0000_PATERNITY_LOCKED";

            break;
          case "UNLOCK":
            ExitState = "SI0000_PATERNITY_UNLOCKED";

            break;
          default:
            break;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

Test2:

    if (IsExitState("LE0000_USE_PF9_TO_RETURN"))
    {
      // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Allow return with BOW=U or CSE 
      // to Estab=U
    }

    // ---------------------------------------------
    // If a display is required, call the action
    // block that reads the next group of data based
    // on the page number.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.SuccessfulDisplay.Flag = "";
      export.ApPrompt.SelectChar = "";

      if (!IsEmpty(export.Next.Number))
      {
        UseCabZeroFillNumber();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            return;
          }
        }
      }
      else
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "CASE_NUMBER_REQUIRED";

        return;
      }

      if (IsEmpty(export.Case1.Number))
      {
        export.Case1.Number = export.Next.Number;
      }

      if (!Equal(export.Next.Number, export.Case1.Number))
      {
        export.Case1.Number = export.Next.Number;
        export.Ap.Number = "";
      }

      if (!IsEmpty(export.Ap.Number))
      {
        local.TextWorkArea.Text10 = export.Ap.Number;
        UseEabPadLeftWithZeros();
        export.Ap.Number = local.TextWorkArea.Text10;
      }

      // ---------------------------------------------
      // Call the action block that reads
      // the data required for this screen.
      // --------------------------------------------
      UseSiReadCaseHeaderInformation();

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
      {
      }
      else if (IsExitState("CASE_NF"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailCsePersonsWorkSet.Assign(
            local.BlankCsePersonsWorkSet);
          MoveCsePerson1(local.BlankCsePerson,
            export.Export1.Update.DetailCsePerson);
        }

        export.Export1.CheckIndex();

        return;
      }
      else
      {
        return;
      }

      if (!IsEmpty(local.AbendData.Type1))
      {
        ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";

        return;
      }

      if (AsChar(local.MultipleAps.Flag) == 'Y')
      {
        // -----------------------------------------------
        // 05/03/99 W.Campbell - Added code to send
        // selection needed msg to COMP.  BE SURE
        // TO MATCH next_tran_info ON THE DIALOG
        // FLOW.
        // -----------------------------------------------
        export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        return;
      }

      if (IsExitState("NO_APS_ON_A_CASE"))
      {
        local.NoAp.Flag = "Y";
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        local.NoAp.Flag = "N";
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.Export1.Count = 0;

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      UseSiReadChCaseRolesForCase();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }
        else
        {
          // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Multiple APs are now 
          // allowed even if pat established=Y.
        }

        // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Protect paternity info when 
        // paternity is locked.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCsePerson.PaternityLockInd) == 'Y'
            )
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePerson, "bornOutOfWedlock");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.DetailCsePerson,
              "cseToEstblPaternity");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.DetailCsePerson,
              "paternityEstablishedIndicator");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertificateSignature");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.DetailCsePerson, "hospitalPatEstInd");
              

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertFathersLastName");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertFathersFirstName");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Export1.Item.DetailCsePerson, "birthCertFathersMi");
              

            field8.Color = "cyan";
            field8.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePerson, "bornOutOfWedlock");
              

            field1.Color = "";
            field1.Protected = false;

            var field2 =
              GetField(export.Export1.Item.DetailCsePerson,
              "cseToEstblPaternity");

            field2.Color = "";
            field2.Protected = false;

            var field3 =
              GetField(export.Export1.Item.DetailCsePerson,
              "paternityEstablishedIndicator");

            field3.Color = "";
            field3.Protected = false;

            var field4 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertificateSignature");

            field4.Color = "";
            field4.Protected = false;

            var field5 =
              GetField(export.Export1.Item.DetailCsePerson, "hospitalPatEstInd");
              

            field5.Color = "";
            field5.Protected = false;

            var field6 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertFathersLastName");

            field6.Color = "";
            field6.Protected = false;

            var field7 =
              GetField(export.Export1.Item.DetailCsePerson,
              "birthCertFathersFirstName");

            field7.Color = "";
            field7.Protected = false;

            var field8 =
              GetField(export.Export1.Item.DetailCsePerson, "birthCertFathersMi");
              

            field8.Color = "";
            field8.Protected = false;
          }
        }

        export.Export1.CheckIndex();

        // 09/24/2003  GVandy  PR188676  Protect child paternity info if the AP 
        // is female and the child is on another case with a male AP.
        if (AsChar(export.Ap.Sex) == 'F')
        {
          export.Export1.Index = 0;

          for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
            export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            foreach(var item in ReadCsePerson2())
            {
              local.ApOnOtherCase.Number = entities.ApCsePerson.Number;
              local.ApOnOtherCase.Sex = "";
              UseCabReadAdabasPerson();

              if (AsChar(local.ApOnOtherCase.Sex) == 'M')
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "bornOutOfWedlock");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "cseToEstblPaternity");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "paternityEstablishedIndicator");

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "birthCertificateSignature");

                field4.Color = "cyan";
                field4.Protected = true;

                var field5 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "hospitalPatEstInd");

                field5.Color = "cyan";
                field5.Protected = true;

                var field6 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "birthCertFathersLastName");

                field6.Color = "cyan";
                field6.Protected = true;

                var field7 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "birthCertFathersFirstName");

                field7.Color = "cyan";
                field7.Protected = true;

                var field8 =
                  GetField(export.Export1.Item.DetailCsePerson,
                  "birthCertFathersMi");

                field8.Color = "cyan";
                field8.Protected = true;

                break;
              }
            }
          }

          export.Export1.CheckIndex();
        }
      }
      else if (IsExitState("CASE_NF"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailCsePersonsWorkSet.Assign(
            local.BlankCsePersonsWorkSet);
          MoveCsePerson1(local.BlankCsePerson,
            export.Export1.Update.DetailCsePerson);
        }

        export.Export1.CheckIndex();

        return;
      }
      else
      {
        return;
      }

      if (!IsEmpty(local.AbendData.Type1))
      {
        ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";

        return;
      }

      export.SuccessfulDisplay.Flag = "Y";

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
      export.HiddenPageKeys.CheckSize();

      if (export.HiddenPageKeys.Index >= 30)
      {
        ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";

        return;
      }

      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
        local.NextCaseRole.EndDate;
      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.StartDate =
        local.NextCaseRole.StartDate;
      export.HiddenPageKeys.Update.HiddenPageKeyCsePerson.Number =
        local.NextCsePerson.Number;

      if (!export.Export1.IsEmpty)
      {
        export.Export1.Index = 0;
        export.Export1.CheckSize();

        var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

        field.Protected = false;
        field.Focused = true;

        if (!IsExitState("SI0000_INV_PATERNITY_ESTAB"))
        {
          // *** CQ#421 Display different message for the first time if you come
          // from CRPA screen
          // *** Added If condition for the new display message
          if (AsChar(export.FromCrpa.Flag) == 'Y' && IsEmpty
            (export.FirstDisplayFromCrpa.Flag))
          {
            export.FirstDisplayFromCrpa.Flag = "Y";
            ExitState = "ACO_NI0000_SUCCES_DISP_FROM_CRPA";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }

          if (AsChar(export.Case1.Status) == 'C')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }
      }
      else
      {
        ExitState = "NO_ACTIVE_CHILD";
      }

      if (AsChar(export.LegalActionFlow.Flag) == 'Y' || AsChar
        (export.FromLops.Flag) == 'Y')
      {
        // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Allow return with BOW=U or 
        // CSE to Estab=U
      }

      // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) In a legal action flow display 
      // CPAT if any
      //    child has BOW=U, CSE to Estab=U, or Paternity Locked ind<>'Y' and 
      // Paternity Est=Y.
      //    User can now PF9 (RETURN) back without changing these values.
      if (AsChar(export.LegalActionFlow.Flag) == 'Y')
      {
        ExitState = "SI0000_REVIEW_AND_LOCK_CPAT";

        for(export.Export1.Index = export.Export1.Count - 1; export
          .Export1.Index >= 0; --export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCsePerson.BornOutOfWedlock) == 'U'
            )
          {
            return;
          }

          if (AsChar(export.Export1.Item.DetailCsePerson.CseToEstblPaternity) ==
            'U')
          {
            return;
          }

          if (AsChar(export.Export1.Item.DetailCsePerson.PaternityLockInd) != 'Y'
            && AsChar
            (export.Export1.Item.DetailCsePerson.PaternityEstablishedIndicator) ==
              'Y')
          {
            return;
          }
        }

        export.Export1.CheckIndex();

        if (ReadCaseRoleCsePerson())
        {
          return;
        }

        ExitState = "ACO_NE0000_RETURN";
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
    target.CseOpenDate = source.CseOpenDate;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.BornOutOfWedlock = source.BornOutOfWedlock;
    target.CseToEstblPaternity = source.CseToEstblPaternity;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
    target.DatePaternEstab = source.DatePaternEstab;
    target.BirthCertFathersLastName = source.BirthCertFathersLastName;
    target.BirthCertFathersFirstName = source.BirthCertFathersFirstName;
    target.BirthCertFathersMi = source.BirthCertFathersMi;
    target.BirthCertificateSignature = source.BirthCertificateSignature;
    target.HospitalPatEstInd = source.HospitalPatEstInd;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.BornOutOfWedlock = source.BornOutOfWedlock;
    target.CseToEstblPaternity = source.CseToEstblPaternity;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveExport1(SiReadChCaseRolesForCase.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailCsePerson.Assign(source.DetailCsePerson);
    target.DetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
    target.DetailCaseRole.Assign(source.DetailCaseRole);
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.ApOnOtherCase.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.ApOnOtherCase);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCabValidatePaternityComb()
  {
    var useImport = new SiCabValidatePaternityComb.Import();
    var useExport = new SiCabValidatePaternityComb.Export();

    MoveCsePerson2(export.Export1.Item.DetailCsePerson, useImport.CsePerson);

    Call(SiCabValidatePaternityComb.Execute, useImport, useExport);
  }

  private void UseSiChdsRaiseEvent1()
  {
    var useImport = new SiChdsRaiseEvent.Import();
    var useExport = new SiChdsRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.Ch.Number = export.Export1.Item.DetailCsePerson.Number;

    Call(SiChdsRaiseEvent.Execute, useImport, useExport);

    local.Returned.SituationNumber = useExport.Infrastructure.SituationNumber;
  }

  private void UseSiChdsRaiseEvent2()
  {
    var useImport = new SiChdsRaiseEvent.Import();
    var useExport = new SiChdsRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.Ch.Number = export.Export1.Item.DetailCsePerson.Number;

    Call(SiChdsRaiseEvent.Execute, useImport, useExport);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    export.Ap.Assign(useExport.Ap);
    export.Ar.Assign(useExport.Ar);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseSiReadChCaseRolesForCase()
  {
    var useImport = new SiReadChCaseRolesForCase.Import();
    var useExport = new SiReadChCaseRolesForCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    MoveCaseRole(export.HiddenPageKeys.Item.HiddenPageKeyCaseRole,
      useImport.PageKeyCaseRole);
    useImport.PageKeyCsePerson.Number =
      export.HiddenPageKeys.Item.HiddenPageKeyCsePerson.Number;

    Call(SiReadChCaseRolesForCase.Execute, useImport, useExport);

    MoveCaseRole(useExport.PageKeyCaseRole, local.NextCaseRole);
    local.NextCsePerson.Number = useExport.PageKeyCsePerson.Number;
    local.AbendData.Assign(useExport.AbendData);
    MoveCase1(useExport.Case1, export.Case1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void CreateNarrativeDetail()
  {
    var infrastructureId = entities.Other.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var caseNumber = entities.Other.CaseNumber;
    var narrativeText = entities.Existing.NarrativeText;
    var lineNumber = entities.Existing.LineNumber;

    entities.New1.Populated = false;
    Update("CreateNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(command, "infrastructureId", infrastructureId);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "narrativeText", narrativeText);
        db.SetInt32(command, "lineNumber", lineNumber);
      });

    entities.New1.InfrastructureId = infrastructureId;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.NarrativeText = narrativeText;
    entities.New1.LineNumber = lineNumber;
    entities.New1.Populated = true;
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCsePerson.Number = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCsePerson.Type1 = db.GetString(reader, 6);
        entities.ChCsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ChCsePerson.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ChCsePerson.BornOutOfWedlock = db.GetNullableString(reader, 9);
        entities.ChCsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 10);
        entities.ChCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 11);
        entities.ChCsePerson.DatePaternEstab = db.GetDate(reader, 12);
        entities.ChCsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 13);
        entities.ChCsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 14);
        entities.ChCsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 15);
        entities.ChCsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 16);
        entities.ChCsePerson.HospitalPatEstInd =
          db.GetNullableString(reader, 17);
        entities.ChCsePerson.PaternityLockInd =
          db.GetNullableString(reader, 18);
        entities.ChCsePerson.PaternityLockUpdateDate =
          db.GetNullableDate(reader, 19);
        entities.ChCsePerson.PaternityLockUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ChCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.
          SetString(command, "numb", export.Export1.Item.DetailCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChCsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.ChCsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.ChCsePerson.BornOutOfWedlock = db.GetNullableString(reader, 4);
        entities.ChCsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 5);
        entities.ChCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 6);
        entities.ChCsePerson.DatePaternEstab = db.GetDate(reader, 7);
        entities.ChCsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 8);
        entities.ChCsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 9);
        entities.ChCsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 10);
        entities.ChCsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 11);
        entities.ChCsePerson.HospitalPatEstInd =
          db.GetNullableString(reader, 12);
        entities.ChCsePerson.PaternityLockInd =
          db.GetNullableString(reader, 13);
        entities.ChCsePerson.PaternityLockUpdateDate =
          db.GetNullableDate(reader, 14);
        entities.ChCsePerson.PaternityLockUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(
          command, "cspNumber", export.Export1.Item.DetailCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", local.Returned.SituationNumber);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 3);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 4);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 5);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 6);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 7);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 10);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 11);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 12);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 13);
        entities.Infrastructure.UserId = db.GetString(reader, 14);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 15);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.Infrastructure.Function = db.GetNullableString(reader, 19);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Original.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.HiddenToNate.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Original.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Original.SituationNumber = db.GetInt32(reader, 1);
        entities.Original.CaseNumber = db.GetNullableString(reader, 2);
        entities.Original.CsePersonNumber = db.GetNullableString(reader, 3);
        entities.Original.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure3()
  {
    entities.Other.Populated = false;

    return ReadEach("ReadInfrastructure3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum", export.Export1.Item.DetailCsePerson.Number);
        db.SetInt32(
          command, "situationNumber",
          import.HiddenToNate.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Other.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Other.SituationNumber = db.GetInt32(reader, 1);
        entities.Other.CaseNumber = db.GetNullableString(reader, 2);
        entities.Other.CsePersonNumber = db.GetNullableString(reader, 3);
        entities.Other.CaseUnitNumber = db.GetNullableInt32(reader, 4);
        entities.Other.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPersonLegalAction1()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPersonLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.ChCsePerson.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalAction.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.Classification = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPersonLegalAction2()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPersonLegalAction2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableString(command, "cspNumber", entities.ChCsePerson.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(command, "endDt", date);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalAction.Identifier = db.GetInt32(reader, 7);
        entities.LegalAction.Classification = db.GetString(reader, 8);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          import.HiddenToNate.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.CreatedBy = db.GetNullableString(reader, 2);
        entities.Existing.CaseNumber = db.GetNullableString(reader, 3);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 4);
        entities.Existing.LineNumber = db.GetInt32(reader, 5);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private void UpdateCsePerson1()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var bornOutOfWedlock =
      export.Export1.Item.DetailCsePerson.BornOutOfWedlock ?? "";
    var cseToEstblPaternity =
      export.Export1.Item.DetailCsePerson.CseToEstblPaternity ?? "";
    var paternityEstablishedIndicator =
      export.Export1.Item.DetailCsePerson.PaternityEstablishedIndicator ?? "";
    var datePaternEstab = export.Export1.Item.DetailCsePerson.DatePaternEstab;
    var birthCertFathersLastName =
      export.Export1.Item.DetailCsePerson.BirthCertFathersLastName ?? "";
    var birthCertFathersFirstName =
      export.Export1.Item.DetailCsePerson.BirthCertFathersFirstName ?? "";
    var birthCertFathersMi =
      export.Export1.Item.DetailCsePerson.BirthCertFathersMi ?? "";
    var birthCertificateSignature =
      export.Export1.Item.DetailCsePerson.BirthCertificateSignature ?? "";
    var hospitalPatEstInd =
      export.Export1.Item.DetailCsePerson.HospitalPatEstInd ?? "";

    entities.ChCsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "outOfWedlock", bornOutOfWedlock);
        db.SetNullableString(command, "cseToEstPatr", cseToEstblPaternity);
        db.SetNullableString(
          command, "patEstabInd", paternityEstablishedIndicator);
        db.SetDate(command, "datePaternEstab", datePaternEstab);
        db.
          SetNullableString(command, "bcFatherLastNm", birthCertFathersLastName);
          
        db.SetNullableString(
          command, "bcFatherFirstNm", birthCertFathersFirstName);
        db.SetNullableString(command, "bcFathersMi", birthCertFathersMi);
        db.SetNullableString(command, "bcSignature", birthCertificateSignature);
        db.SetNullableString(command, "hospitalPatEst", hospitalPatEstInd);
        db.SetString(command, "numb", entities.ChCsePerson.Number);
      });

    entities.ChCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ChCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ChCsePerson.BornOutOfWedlock = bornOutOfWedlock;
    entities.ChCsePerson.CseToEstblPaternity = cseToEstblPaternity;
    entities.ChCsePerson.PaternityEstablishedIndicator =
      paternityEstablishedIndicator;
    entities.ChCsePerson.DatePaternEstab = datePaternEstab;
    entities.ChCsePerson.BirthCertFathersLastName = birthCertFathersLastName;
    entities.ChCsePerson.BirthCertFathersFirstName = birthCertFathersFirstName;
    entities.ChCsePerson.BirthCertFathersMi = birthCertFathersMi;
    entities.ChCsePerson.BirthCertificateSignature = birthCertificateSignature;
    entities.ChCsePerson.HospitalPatEstInd = hospitalPatEstInd;
    entities.ChCsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var paternityLockInd = "N";
    var paternityLockUpdateDate = Now().Date;

    entities.ChCsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "patLockInd", paternityLockInd);
        db.SetNullableDate(command, "patLockUpdateDt", paternityLockUpdateDate);
        db.SetNullableString(command, "patLockUpdatdBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.ChCsePerson.Number);
      });

    entities.ChCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ChCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ChCsePerson.PaternityLockInd = paternityLockInd;
    entities.ChCsePerson.PaternityLockUpdateDate = paternityLockUpdateDate;
    entities.ChCsePerson.PaternityLockUpdatedBy = lastUpdatedBy;
    entities.ChCsePerson.Populated = true;
  }

  private void UpdateCsePerson3()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var paternityLockInd = "Y";
    var paternityLockUpdateDate = Now().Date;

    entities.ChCsePerson.Populated = false;
    Update("UpdateCsePerson3",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "patLockInd", paternityLockInd);
        db.SetNullableDate(command, "patLockUpdateDt", paternityLockUpdateDate);
        db.SetNullableString(command, "patLockUpdatdBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.ChCsePerson.Number);
      });

    entities.ChCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ChCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ChCsePerson.PaternityLockInd = paternityLockInd;
    entities.ChCsePerson.PaternityLockUpdateDate = paternityLockUpdateDate;
    entities.ChCsePerson.PaternityLockUpdatedBy = lastUpdatedBy;
    entities.ChCsePerson.Populated = true;
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
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

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
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCaseRole")]
      public CaseRole HiddenPageKeyCaseRole
      {
        get => hiddenPageKeyCaseRole ??= new();
        set => hiddenPageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyCsePerson.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCsePerson")]
      public CsePerson HiddenPageKeyCsePerson
      {
        get => hiddenPageKeyCsePerson ??= new();
        set => hiddenPageKeyCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CaseRole hiddenPageKeyCaseRole;
      private CsePerson hiddenPageKeyCsePerson;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of FromLrol.
    /// </summary>
    [JsonPropertyName("fromLrol")]
    public Common FromLrol
    {
      get => fromLrol ??= new();
      set => fromLrol = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

    /// <summary>
    /// A value of FromRegi.
    /// </summary>
    [JsonPropertyName("fromRegi")]
    public Common FromRegi
    {
      get => fromRegi ??= new();
      set => fromRegi = value;
    }

    /// <summary>
    /// A value of SuccessfulDisplay.
    /// </summary>
    [JsonPropertyName("successfulDisplay")]
    public Common SuccessfulDisplay
    {
      get => successfulDisplay ??= new();
      set => successfulDisplay = value;
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

    /// <summary>
    /// A value of FromLops.
    /// </summary>
    [JsonPropertyName("fromLops")]
    public Common FromLops
    {
      get => fromLops ??= new();
      set => fromLops = value;
    }

    /// <summary>
    /// A value of FromCrpa.
    /// </summary>
    [JsonPropertyName("fromCrpa")]
    public Common FromCrpa
    {
      get => fromCrpa ??= new();
      set => fromCrpa = value;
    }

    /// <summary>
    /// A value of FirstDisplayFromCrpa.
    /// </summary>
    [JsonPropertyName("firstDisplayFromCrpa")]
    public Common FirstDisplayFromCrpa
    {
      get => firstDisplayFromCrpa ??= new();
      set => firstDisplayFromCrpa = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of HiddenToNate.
    /// </summary>
    [JsonPropertyName("hiddenToNate")]
    public Infrastructure HiddenToNate
    {
      get => hiddenToNate ??= new();
      set => hiddenToNate = value;
    }

    private WorkArea headerLine;
    private CsePersonsWorkSet ap;
    private Common apPrompt;
    private CsePersonsWorkSet ar;
    private Case1 next;
    private Case1 case1;
    private Standard hiddenStandard;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Common fromLrol;
    private Common fromRole;
    private Common fromRegi;
    private Common successfulDisplay;
    private Common legalActionFlow;
    private Common fromLops;
    private Common fromCrpa;
    private Common firstDisplayFromCrpa;
    private Common previous;
    private Infrastructure hiddenToNate;
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
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

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
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCaseRole")]
      public CaseRole HiddenPageKeyCaseRole
      {
        get => hiddenPageKeyCaseRole ??= new();
        set => hiddenPageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyCsePerson.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCsePerson")]
      public CsePerson HiddenPageKeyCsePerson
      {
        get => hiddenPageKeyCsePerson ??= new();
        set => hiddenPageKeyCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CaseRole hiddenPageKeyCaseRole;
      private CsePerson hiddenPageKeyCsePerson;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of FromLrol.
    /// </summary>
    [JsonPropertyName("fromLrol")]
    public Common FromLrol
    {
      get => fromLrol ??= new();
      set => fromLrol = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

    /// <summary>
    /// A value of FromRegi.
    /// </summary>
    [JsonPropertyName("fromRegi")]
    public Common FromRegi
    {
      get => fromRegi ??= new();
      set => fromRegi = value;
    }

    /// <summary>
    /// A value of SuccessfulDisplay.
    /// </summary>
    [JsonPropertyName("successfulDisplay")]
    public Common SuccessfulDisplay
    {
      get => successfulDisplay ??= new();
      set => successfulDisplay = value;
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

    /// <summary>
    /// A value of FromLops.
    /// </summary>
    [JsonPropertyName("fromLops")]
    public Common FromLops
    {
      get => fromLops ??= new();
      set => fromLops = value;
    }

    /// <summary>
    /// A value of FromCrpa.
    /// </summary>
    [JsonPropertyName("fromCrpa")]
    public Common FromCrpa
    {
      get => fromCrpa ??= new();
      set => fromCrpa = value;
    }

    /// <summary>
    /// A value of FirstDisplayFromCrpa.
    /// </summary>
    [JsonPropertyName("firstDisplayFromCrpa")]
    public Common FirstDisplayFromCrpa
    {
      get => firstDisplayFromCrpa ??= new();
      set => firstDisplayFromCrpa = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of ToNateInfrastructure.
    /// </summary>
    [JsonPropertyName("toNateInfrastructure")]
    public Infrastructure ToNateInfrastructure
    {
      get => toNateInfrastructure ??= new();
      set => toNateInfrastructure = value;
    }

    /// <summary>
    /// A value of FlowToNate.
    /// </summary>
    [JsonPropertyName("flowToNate")]
    public Common FlowToNate
    {
      get => flowToNate ??= new();
      set => flowToNate = value;
    }

    /// <summary>
    /// A value of ToNateCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("toNateCsePersonsWorkSet")]
    public CsePersonsWorkSet ToNateCsePersonsWorkSet
    {
      get => toNateCsePersonsWorkSet ??= new();
      set => toNateCsePersonsWorkSet = value;
    }

    private WorkArea headerLine;
    private CsePersonsWorkSet ap;
    private Common apPrompt;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet selected;
    private Case1 next;
    private Case1 case1;
    private Standard standard;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Common caseOpen;
    private Common fromLrol;
    private Common fromRole;
    private Common fromRegi;
    private Common successfulDisplay;
    private Common legalActionFlow;
    private Common fromLops;
    private Common fromCrpa;
    private Common firstDisplayFromCrpa;
    private Common previous;
    private Infrastructure toNateInfrastructure;
    private Common flowToNate;
    private CsePersonsWorkSet toNateCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Infrastructure Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public Infrastructure Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of ApOnOtherCase.
    /// </summary>
    [JsonPropertyName("apOnOtherCase")]
    public CsePersonsWorkSet ApOnOtherCase
    {
      get => apOnOtherCase ??= new();
      set => apOnOtherCase = value;
    }

    /// <summary>
    /// A value of BlankCsePerson.
    /// </summary>
    [JsonPropertyName("blankCsePerson")]
    public CsePerson BlankCsePerson
    {
      get => blankCsePerson ??= new();
      set => blankCsePerson = value;
    }

    /// <summary>
    /// A value of ActiveApOnCase.
    /// </summary>
    [JsonPropertyName("activeApOnCase")]
    public Common ActiveApOnCase
    {
      get => activeApOnCase ??= new();
      set => activeApOnCase = value;
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

    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public CsePersonsWorkSet ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ActiveCaseCh.
    /// </summary>
    [JsonPropertyName("activeCaseCh")]
    public Common ActiveCaseCh
    {
      get => activeCaseCh ??= new();
      set => activeCaseCh = value;
    }

    /// <summary>
    /// A value of SuccessfulUpdate.
    /// </summary>
    [JsonPropertyName("successfulUpdate")]
    public Common SuccessfulUpdate
    {
      get => successfulUpdate ??= new();
      set => successfulUpdate = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of BlankCaseRole.
    /// </summary>
    [JsonPropertyName("blankCaseRole")]
    public CaseRole BlankCaseRole
    {
      get => blankCaseRole ??= new();
      set => blankCaseRole = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

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
    /// A value of NextCaseRole.
    /// </summary>
    [JsonPropertyName("nextCaseRole")]
    public CaseRole NextCaseRole
    {
      get => nextCaseRole ??= new();
      set => nextCaseRole = value;
    }

    /// <summary>
    /// A value of NextCsePerson.
    /// </summary>
    [JsonPropertyName("nextCsePerson")]
    public CsePerson NextCsePerson
    {
      get => nextCsePerson ??= new();
      set => nextCsePerson = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of OtherChOnCase.
    /// </summary>
    [JsonPropertyName("otherChOnCase")]
    public Common OtherChOnCase
    {
      get => otherChOnCase ??= new();
      set => otherChOnCase = value;
    }

    /// <summary>
    /// A value of ChOnCase.
    /// </summary>
    [JsonPropertyName("chOnCase")]
    public Common ChOnCase
    {
      get => chOnCase ??= new();
      set => chOnCase = value;
    }

    /// <summary>
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of NoAp.
    /// </summary>
    [JsonPropertyName("noAp")]
    public Common NoAp
    {
      get => noAp ??= new();
      set => noAp = value;
    }

    private Infrastructure previous;
    private Infrastructure returned;
    private CsePersonsWorkSet apOnOtherCase;
    private CsePerson blankCsePerson;
    private Common activeApOnCase;
    private Common select;
    private CsePersonsWorkSet activeAp;
    private Case1 case1;
    private CsePerson arCsePerson;
    private Common cse;
    private Common fromRole;
    private DateWorkArea null1;
    private DateWorkArea current;
    private Common activeCaseCh;
    private Common successfulUpdate;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private DateWorkArea dateWorkArea;
    private CaseRole blankCaseRole;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Common error;
    private CaseRole nextCaseRole;
    private CsePerson nextCsePerson;
    private AbendData abendData;
    private Common common;
    private Infrastructure infrastructure;
    private Common otherChOnCase;
    private Common chOnCase;
    private Common invalid;
    private TextWorkArea textWorkArea;
    private Common multipleAps;
    private Common noAp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public Infrastructure Original
    {
      get => original ??= new();
      set => original = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public Infrastructure Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public NarrativeDetail Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCase.
    /// </summary>
    [JsonPropertyName("apCase")]
    public Case1 ApCase
    {
      get => apCase ??= new();
      set => apCase = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public Case1 Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    private Infrastructure original;
    private Infrastructure other;
    private NarrativeDetail new1;
    private NarrativeDetail existing;
    private Infrastructure infrastructure;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private Case1 case1;
    private CsePerson apCsePerson;
    private Case1 apCase;
    private CaseRole chCaseRole;
    private Case1 zdel;
    private CaseRole apCaseRole;
    private CsePerson chCsePerson;
  }
#endregion
}
