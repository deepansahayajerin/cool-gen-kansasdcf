// Program: SI_PAR1_PA_REFERRAL_PAGE_1, ID: 371759830, model: 746.
// Short name: SWEPAR1P
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
/// A program: SI_PAR1_PA_REFERRAL_PAGE_1.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPar1PaReferralPage1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PAR1_PA_REFERRAL_PAGE_1 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPar1PaReferralPage1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPar1PaReferralPage1.
  /// </summary>
  public SiPar1PaReferralPage1(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------
    //                           M A I N T E N A N C E      L O G
    //   Date	  Developer	Description
    // --------------------------------------------------------------------------------
    // 03-01-95  J.W.Hays	Initial Development
    // 08-29-95  Ken Evans	Continue development
    // 01-24-96  G.P. Kim	Kessep Retrofit
    // 05-03-96  Rao Mulpuri	Changes to Read PA Ref
    // 10/08/96  G. Lofton	Added ab and eab to read ADABAS for
    // 			programs already assigned and
    // 			create those programs on CSE.
    // 11/02/96  G. Lofton	Add new security and removed old.
    // 01/08/96  Sid Chowdhary Add OSP assignment to read.
    // 			Added flow to ASIN.
    // 05/06/97  Sid C		Fixes.
    // 07/04/97  Sid Chowdhary Provide scrolling for REFM option 4
    // 			and allow for search on Agency.
    // 09/30/98  W. Campbell   Modified the scrolling logic for
    //                         PApv and PAnx (PFK 19 and 20) so
    //                         that it would work correctly. This
    //                         also involved changes to CAB
    //                         SI_PAR1_READ_PA_REFERRAL_1.
    // 10/02/98  W. Campbell   Modified the logic so that
    //                         ROLLBACKs will be performed when
    //                         Database updates (for both DB2 and
    //                         ADABAS) don't work properly.
    //                         Changes were made to:
    //                         SI_PAR1_PA_REFERRAL_PAGE_1
    //                         SI_PAR1_DEACT_PA_REFERRAL
    //                         SI_PAR1_ASSOC_PA_REFERRAL_CASE.
    //                         A little more work was done on this
    //                         on 10/05/98.
    // 11/17/98 W. Campbell    - ADABAS debug -
    //                         If an error occurs and you
    //                         need to see the values returned
    //                         from ADABAS then place the
    //                         attributes for export_eab
    //                         abend_data on the screen and
    //                         run again.
    // 05/26/99 M. Lachowicz   Replace zdel exit state by
    //                         a new exit state.
    // 04/02/00 W.Campbell     Inserted code to
    //                         implement the Family Violence
    //                         Indicator logic into this Pstep.
    //                         Work done on WR#000162 for FVI.
    // 06/19/00 M Lachowicz         Changed sort order of PA Referrals.
    //                              PA Referrals should be sorted in
    //                              ascending sequence of Approval Date,
    //                              Referral No. and created timestamp.
    //                              Work done on PR # 97520.
    // 09/11/00 W Campbell          Made changes to move
    //                              the logic associated with
    //                              command retregi from PAR1
    //                              into REGI.  This change puts
    //                              the code to deactivate the
    //                              PA Referral into REGI after a
    //                              case is registered as a result of
    //                              a PA Referral. Work done
    //                              on WR#000205.
    // 01/22/01 C Fairley WR 000274 Added the 'Child to AR' relationship
    //                              code to the group view on the screen.
    // 06/20/01 C Fairley I00122128 Changed the READ EACH PA Referral 
    // Participant "Where"
    //                              clause.
    //                              Added READ EACH of PA Referral Participant 
    // to
    //                              FROMCOMM, PANX(next pa referral) and
    //                              PAPV(previous pa referral) commands.
    //                              Removed OLD commented OUT code.
    // --------------------------------------------------------------------------------
    // ********************************************
    // This PRAD displays part of the PA Referral
    // information (page 1). This screen is display
    // only. Within the referral the children are
    // scrollable.  A group view of the referral
    // types is created and scrolling also occurrs
    // between referrals.
    // ********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // -------------------------------------------
    // 09/11/00 W.Campbell - New code added for changes
    // made to move the logic associated with command
    // retregi from PAR1 into REGI.  This change puts the
    // code to deactivate the PA Referral into REGI after
    // a case is registered as a result of a PA Referral.
    // Work done on WR#000205.
    // -------------------------------------------
    if (Equal(global.Command, "RETREGI"))
    {
      // -------------------------------------------
      // 09/11/00 W.Campbell - If the command is
      // RETREGI change it to DISPLAY.
      // Work done on WR#000205.
      // -------------------------------------------
      global.Command = "DISPLAY";
    }

    // -------------------------------------------
    // 09/11/00 W.Campbell - End of new code added for
    // changes made to move the logic associated with
    // command retregi from PAR1 into REGI.  This change
    // puts the code to deactivate the PA Referral into REGI
    // after a case is registered as a result of a PA Referral.
    // Work done on WR#000205.
    // -------------------------------------------
    export.HiddenNextTranInfo.Assign(import.Hidden);
    export.Ap.Assign(import.Ap);
    export.Ar.Assign(import.Ar);
    export.Other.Assign(import.Other);
    export.PaReferral.Assign(import.PaReferral);
    local.PaReferral.Assign(import.PaReferral);
    export.ApHome.Assign(import.ApHome);
    export.ApMail.Assign(import.ApMail);
    export.ApName.Text33 = import.ApName.Text33;
    export.ArName.Text33 = import.ArName.Text33;
    export.OtherName.Text33 = import.OtherName.Text33;
    export.MoreReferralsMinus.Text1 = import.MoreReferralsMinus.Text1;
    export.MoreReferralsPlus.Text1 = import.MoreReferralsPlus.Text1;
    export.Save.Assign(import.Save);
    export.Screen.Type1 = import.Screen.Type1;
    export.SaveSubscript.Subscript = import.SaveSubscript.Subscript;
    export.BeenThere.Flag = import.BeenThere.Flag;
    export.FromPar1.Flag = import.FromPar1.Flag;
    MoveWorkArea(import.Deact, export.Deact);
    export.WarningMsg.Text60 = import.WarningMsg.Text60;
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.ServiceProviderAddress.City = import.ServiceProviderAddress.City;
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    MoveOffice(import.Office, export.Office);

    export.Children.Index = 0;
    export.Children.Clear();

    for(import.Children.Index = 0; import.Children.Index < import
      .Children.Count; ++import.Children.Index)
    {
      if (export.Children.IsFull)
      {
        break;
      }

      export.Children.Update.GchildName.Text33 =
        import.Children.Item.GchildName.Text33;
      export.Children.Update.GchildPaReferralParticipant.Assign(
        import.Children.Item.GchildPaReferralParticipant);

      // *** Work request 000274
      // *** 01/22/01 swsrchf
      MoveCaseRole(import.Children.Item.GchildCaseRole,
        export.Children.Update.GchildCaseRole);
      export.Children.Next();
    }

    if (!import.ToRegi.IsEmpty)
    {
      for(import.ToRegi.Index = 0; import.ToRegi.Index < import.ToRegi.Count; ++
        import.ToRegi.Index)
      {
        if (!import.ToRegi.CheckSize())
        {
          break;
        }

        export.ToRegi.Index = import.ToRegi.Index;
        export.ToRegi.CheckSize();

        export.ToRegi.Update.Gregi.Assign(import.ToRegi.Item.Gregi);
      }

      import.ToRegi.CheckIndex();
    }

    MoveStandard(import.Standard, export.Standard);

    if (Equal(global.Command, "RETASIN") || Equal(global.Command, "RETPAREF"))
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.Screen.Type1 = export.PaReferral.Type1;
      global.Command = "DISPLAY";
    }

    // *** Validate action level security
    if (Equal(global.Command, "PANX") || Equal(global.Command, "PAPV") || Equal
      (global.Command, "NMSRCH") || Equal(global.Command, "REGI") || Equal
      (global.Command, "PAR2") || Equal(global.Command, "PAR3") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "RETREGI") || Equal
      (global.Command, "ASIN") || Equal(global.Command, "FROMCOMN"))
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

    switch(TrimEnd(global.Command))
    {
      case "FROMCOMN":
        UseSiPar1ReadPaReferral2();

        // *** Problem report I00122128
        // *** 06/20/01 swsrchf
        // *** start
        local.Ch.Role = "CH";

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          if (ReadPaReferralParticipant())
          {
            export.Children.Update.GchildCaseRole.RelToAr =
              entities.ExistingPaReferralParticipant.Relationship;
          }
        }

        // *** end
        // *** 06/20/01 swsrchf
        // *** Problem report I00122128
        if (!IsEmpty(export.PaReferral.Type1))
        {
          export.Screen.Type1 = export.PaReferral.Type1;
        }

        if (AsChar(import.Par1FromComn.Flag) == 'Y')
        {
          // *********************************************
          // Return from COMN.... deactivate the referral.
          // *********************************************
          ExitState = "ACO_NN0000_ALL_OK";
          local.PaReferral.AssignDeactivateInd = "D";
          UseSiPar1DeactPaReferral();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.PaReferral, "number");

            field.Error = true;

            UseEabRollbackCics();
          }
          else
          {
            if (!IsEmpty(import.RegiCase.Number))
            {
              if (Equal(export.PaReferral.Type1, "NEW"))
              {
                UseEabUpdatePaReferral();

                // ------------------------------------------------------------
                // The following CASE statement was inserted
                // into this PRAD on 10/05/98 by W. Campbell
                // in order to provide an error msg to the user
                // if this condition occurs in the logic.
                // ------------------------------------------------------------
                switch(AsChar(local.Eab.Type1))
                {
                  case ' ':
                    // ******************************************************
                    // All OK, keep on going.
                    // ******************************************************
                    break;
                  case 'A':
                    if (Equal(local.Eab.AdabasFileNumber, "0000"))
                    {
                      ExitState = "ACO_ADABAS_UNAVAILABLE";
                    }
                    else
                    {
                      ExitState = "ADABAS_INVALID_RETURN_CODE";
                    }

                    break;
                  default:
                    ExitState = "ADABAS_INVALID_RETURN_CODE";

                    break;
                }
              }
            }

            if (IsExitState("ADABAS_INVALID_RETURN_CODE"))
            {
              // --------------------------------------
              // 11/17/98 W. Campbell - ADABAS debug -
              // If this error occurs and you need to see the
              // values returned from ADABAS then place the
              // attributes for export_eab abend_data on the
              // screen and run again.
              // --------------------------------------
              export.Eab.Assign(local.Eab);
              export.RegiCase.Number = import.RegiCase.Number;
              export.Current.Date = local.Current.Date;
              export.Eab.CicsResponseCd = "COMN";
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "REFERRAL_DEACTIVATE_SUCCESSFUL";
            }
            else
            {
              var field = GetField(export.PaReferral, "number");

              field.Error = true;

              UseEabRollbackCics();
            }
          }
        }
        else
        {
          if (!Equal(export.PaReferral.AssignDeactivateDate, null))
          {
            if (AsChar(export.PaReferral.AssignDeactivateInd) == 'A')
            {
              export.Deact.Text33 = "Assigned and Deactivated Referral";
            }
            else
            {
              export.Deact.Text33 = "Deactivated Referral";
            }
          }
          else
          {
            export.Deact.Text33 = "";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            MovePaReferral4(export.PaReferral, export.Save);
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

        if (AsChar(local.CaseExistsWarning.Flag) == 'Y')
        {
          export.WarningMsg.Text60 =
            "Warning - CSE Case exists - Verify AR/AP/CH relationships.";
        }
        else
        {
          export.WarningMsg.Text60 = "";
        }

        break;
      case "ASIN":
        if (!IsEmpty(export.PaReferral.Number))
        {
          export.AsinObject.Text20 = "PA REFERRAL";
          ExitState = "ECO_LNK_TO_ASIN";
        }
        else
        {
          var field = GetField(export.PaReferral, "number");

          field.Color = "red";
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;

          ExitState = "PA_REFERRAL_MUST_EXIST";
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        local.ZeroFill.Text10 = export.PaReferral.Number;
        UseEabPadLeftWithZeros();
        local.PaReferral.Number = local.ZeroFill.Text10;
        UseSiPar1ReadPaReferral3();

        // *** Work request 000274
        // *** 01/22/01 swsrchf
        // *** start
        local.Ch.Role = "CH";

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          if (ReadPaReferralParticipant())
          {
            export.Children.Update.GchildCaseRole.RelToAr =
              entities.ExistingPaReferralParticipant.Relationship;
          }
        }

        // *** end
        // *** 01/22/01 swsrchf
        // *** Work request 000274
        if (!IsEmpty(export.PaReferral.Type1))
        {
          export.Screen.Type1 = export.PaReferral.Type1;
        }

        if (export.Standard.MenuOption == 4)
        {
          if (!Equal(export.PaReferral.AssignDeactivateDate, null))
          {
            if (AsChar(export.PaReferral.AssignDeactivateInd) == 'A')
            {
              export.Deact.Text33 = "Assigned and Deactivated Referral";
            }
            else
            {
              export.Deact.Text33 = "Deactivated Referral";
            }
          }
          else
          {
            export.Deact.Text33 = "";
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MovePaReferral4(export.PaReferral, export.Save);
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        if (AsChar(local.MorePaReferrals.Flag) == 'Y')
        {
          export.MoreReferralsPlus.Text1 = "+";
          export.MoreReferralsMinus.Text1 = "";
        }

        if (AsChar(local.CaseExistsWarning.Flag) == 'Y')
        {
          export.WarningMsg.Text60 =
            "Warning - CSE Case exists - Verify AR/AP/CH relationships.";
        }
        else
        {
          export.WarningMsg.Text60 = "";
        }

        break;
      case "DEACT":
        local.PaReferral.AssignDeactivateInd = "D";
        local.PaReferral.CaseNumber = "";
        UseSiPar1DeactPaReferral();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "REFERRAL_DEACTIVATE_SUCCESSFUL";
        }
        else
        {
          var field = GetField(export.PaReferral, "number");

          field.Error = true;

          UseEabRollbackCics();
        }

        break;
      case "PANX":
        if (IsEmpty(import.MoreReferralsPlus.Text1))
        {
          ExitState = "NO_MORE_REFERRALS_FOUND";

          return;
        }

        UseSiPar1ReadPaReferral3();

        // *** Problem report I00122128
        // *** 06/20/01 swsrchf
        // *** start
        local.Ch.Role = "CH";

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          if (ReadPaReferralParticipant())
          {
            export.Children.Update.GchildCaseRole.RelToAr =
              entities.ExistingPaReferralParticipant.Relationship;
          }
        }

        // *** end
        // *** 06/20/01 swsrchf
        // *** Problem report I00122128
        if (AsChar(local.MorePaReferrals.Flag) == 'Y')
        {
          export.MoreReferralsPlus.Text1 = "+";
        }
        else
        {
          export.MoreReferralsPlus.Text1 = "";
        }

        export.MoreReferralsMinus.Text1 = "-";

        if (export.Standard.MenuOption == 4)
        {
          export.Screen.Type1 = export.PaReferral.Type1;

          if (!Equal(export.PaReferral.AssignDeactivateDate, null))
          {
            if (AsChar(export.PaReferral.AssignDeactivateInd) == 'A')
            {
              export.Deact.Text33 = "Assigned and Deactivated Referral";
            }
            else
            {
              export.Deact.Text33 = "Deactivated Referral";
            }
          }
          else
          {
            export.Deact.Text33 = "";
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        if (AsChar(local.CaseExistsWarning.Flag) == 'Y')
        {
          export.WarningMsg.Text60 =
            "Warning - CSE Case exists - Verify AR/AP/CH relationships.";
        }
        else
        {
          export.WarningMsg.Text60 = "";
        }

        if (AsChar(local.Found.Flag) == 'Y')
        {
          // *********************************************************
          // Save the pa_referral info.
          // *********************************************************
          MovePaReferral4(export.PaReferral, export.Save);
        }
        else
        {
          // *********************************************************
          // Since no data was found we must re-populate the
          // export views since they were wiped out by the
          // above call to SI_PAR1_READ_PA_REFERRAL_1.
          // *********************************************************
          export.Ap.Assign(import.Ap);
          export.Ar.Assign(import.Ar);
          export.Other.Assign(import.Other);
          export.PaReferral.Assign(import.PaReferral);
          export.ApName.Text33 = import.ApName.Text33;
          export.ArName.Text33 = import.ArName.Text33;
          export.OtherName.Text33 = import.OtherName.Text33;
          export.ApHome.Assign(import.ApHome);
          export.ApMail.Assign(import.ApMail);
          export.WarningMsg.Text60 = import.WarningMsg.Text60;

          export.Children.Index = 0;
          export.Children.Clear();

          for(import.Children.Index = 0; import.Children.Index < import
            .Children.Count; ++import.Children.Index)
          {
            if (export.Children.IsFull)
            {
              break;
            }

            export.Children.Update.GchildName.Text33 =
              import.Children.Item.GchildName.Text33;
            export.Children.Update.GchildPaReferralParticipant.Assign(
              import.Children.Item.GchildPaReferralParticipant);
            MoveCaseRole(import.Children.Item.GchildCaseRole,
              export.Children.Update.GchildCaseRole);
            export.Children.Next();
          }

          if (!import.ToRegi.IsEmpty)
          {
            for(import.ToRegi.Index = 0; import.ToRegi.Index < import
              .ToRegi.Count; ++import.ToRegi.Index)
            {
              if (!import.ToRegi.CheckSize())
              {
                break;
              }

              export.ToRegi.Index = import.ToRegi.Index;
              export.ToRegi.CheckSize();

              export.ToRegi.Update.Gregi.Assign(import.ToRegi.Item.Gregi);
            }

            import.ToRegi.CheckIndex();
          }
        }

        break;
      case "PAPV":
        if (IsEmpty(import.MoreReferralsMinus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        UseSiPar1ReadPaReferral3();

        // *** Problem report I00122128
        // *** 06/20/01 swsrchf
        // *** start
        local.Ch.Role = "CH";

        for(export.Children.Index = 0; export.Children.Index < export
          .Children.Count; ++export.Children.Index)
        {
          if (ReadPaReferralParticipant())
          {
            export.Children.Update.GchildCaseRole.RelToAr =
              entities.ExistingPaReferralParticipant.Relationship;
          }
        }

        // *** end
        // *** 06/20/01 swsrchf
        // *** Problem report I00122128
        if (AsChar(local.MorePaReferrals.Flag) == 'Y')
        {
          export.MoreReferralsMinus.Text1 = "-";
        }
        else
        {
          export.MoreReferralsMinus.Text1 = "";
        }

        if (Equal(export.PaReferral.Number, export.Save.Number) && Equal
          (export.PaReferral.CreatedTimestamp, export.Save.CreatedTimestamp) &&
          Equal(export.PaReferral.Type1, export.Save.Type1))
        {
          export.MoreReferralsMinus.Text1 = "";
        }

        export.MoreReferralsPlus.Text1 = "+";

        if (export.Standard.MenuOption == 4)
        {
          export.Screen.Type1 = export.PaReferral.Type1;

          if (!Equal(export.PaReferral.AssignDeactivateDate, null))
          {
            if (AsChar(export.PaReferral.AssignDeactivateInd) == 'A')
            {
              export.Deact.Text33 = "Assigned and Deactivated Referral";
            }
            else
            {
              export.Deact.Text33 = "Deactivated Referral";
            }
          }
          else
          {
            export.Deact.Text33 = "";
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        if (AsChar(local.CaseExistsWarning.Flag) == 'Y')
        {
          export.WarningMsg.Text60 =
            "Warning - CSE Case exists - Verify AR/AP/CH relationships.";
        }
        else
        {
          export.WarningMsg.Text60 = "";
        }

        if (AsChar(local.Found.Flag) == 'Y')
        {
          // *********************************************************
          // Save the pa_referral info.
          // *********************************************************
          MovePaReferral4(export.PaReferral, export.Save);
        }
        else
        {
          // *********************************************************
          // Since no data was found we must re-populate the
          // export views since they were wiped out by the
          // above call to SI_PAR1_READ_PA_REFERRAL_1.
          // *********************************************************
          export.Ap.Assign(import.Ap);
          export.Ar.Assign(import.Ar);
          export.Other.Assign(import.Other);
          export.PaReferral.Assign(import.PaReferral);
          export.ApName.Text33 = import.ApName.Text33;
          export.ArName.Text33 = import.ArName.Text33;
          export.OtherName.Text33 = import.OtherName.Text33;
          export.ApHome.Assign(import.ApHome);
          export.ApMail.Assign(import.ApMail);
          export.WarningMsg.Text60 = import.WarningMsg.Text60;

          export.Children.Index = 0;
          export.Children.Clear();

          for(import.Children.Index = 0; import.Children.Index < import
            .Children.Count; ++import.Children.Index)
          {
            if (export.Children.IsFull)
            {
              break;
            }

            export.Children.Update.GchildName.Text33 =
              import.Children.Item.GchildName.Text33;
            export.Children.Update.GchildPaReferralParticipant.Assign(
              import.Children.Item.GchildPaReferralParticipant);
            MoveCaseRole(import.Children.Item.GchildCaseRole,
              export.Children.Update.GchildCaseRole);
            export.Children.Next();
          }

          if (!import.ToRegi.IsEmpty)
          {
            for(import.ToRegi.Index = 0; import.ToRegi.Index < import
              .ToRegi.Count; ++import.ToRegi.Index)
            {
              if (!import.ToRegi.CheckSize())
              {
                break;
              }

              export.ToRegi.Index = import.ToRegi.Index;
              export.ToRegi.CheckSize();

              export.ToRegi.Update.Gregi.Assign(import.ToRegi.Item.Gregi);
            }

            import.ToRegi.CheckIndex();
          }
        }

        break;
      case "NMSRCH":
        export.BeenThere.Flag = "Y";
        export.FromPar1.Flag = "Y";

        export.ToRegi.Index = 0;
        export.ToRegi.CheckSize();

        export.SaveSubscript.Subscript = export.ToRegi.Index + 1;
        export.Phonetic.Flag = "N";
        export.Phonetic.Percentage = 100;
        export.InitialExecution.Flag = "N";
        export.Search.Assign(export.ToRegi.Item.Gregi);
        export.Search.Dob = null;
        export.Search.Sex = "";
        export.Search.FormattedName = "";
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        break;
      case "RETNAME":
        // *********************************************
        // Return from NameList.... if there are more
        // names to process, link to NameList.
        // *********************************************
        if (!IsEmpty(import.Search.LastName))
        {
          // *********************************************
          // Save returned data for comparison screens
          // *********************************************
          export.ToRegi.Index = import.SaveSubscript.Subscript - 1;
          export.ToRegi.CheckSize();

          MoveCsePersonsWorkSet(import.Search, export.ToRegi.Update.Gregi);
        }

        if (import.SaveSubscript.Subscript < export.ToRegi.Count)
        {
          export.ToRegi.Index = export.SaveSubscript.Subscript;
          export.ToRegi.CheckSize();

          export.SaveSubscript.Subscript = export.ToRegi.Index + 1;
          export.Search.Assign(export.ToRegi.Item.Gregi);
          export.Phonetic.Flag = "N";
          export.Phonetic.Percentage = 100;
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }

        break;
      case "REGI":
        if (AsChar(export.BeenThere.Flag) != 'Y')
        {
          ExitState = "NAME_SEARCH_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_CASE_REGISTER";

        break;
      case "RETREGI":
        // -------------------------------------------
        // 09/11/00 W.Campbell - Disabled code for
        // the command RETREGI.  Changed it to DISPLAY.
        // Work done on WR#000205.
        // -------------------------------------------
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "PREV":
        // 05/26/99 M. Lachowicz      Replace zdel exit state by
        //                            by new exit state.
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PAR2":
        ExitState = "ECO_LNK_TO_PA_REFERRAL_P2";

        break;
      case "PAR3":
        if (Equal(import.PaReferral.PgmCode, "FC"))
        {
          ExitState = "ECO_LNK_TO_PA_REFERRAL_FC";
        }
        else
        {
          ExitState = "NO_FC_DATA_FOR_REFERRAL";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "":
        for(export.ToRegi.Index = 0; export.ToRegi.Index < Export
          .ToRegiGroup.Capacity; ++export.ToRegi.Index)
        {
          if (!export.ToRegi.CheckSize())
          {
            break;
          }

          export.ToRegi.Update.Gregi.Flag = "X";
        }

        export.ToRegi.CheckIndex();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // -------------------------------------------
    // 04/02/00 W.Campbell - Inserted the following
    // code to implement the Family Violence Indicator
    // logic into this Pstep.  Work done on
    // WR#000162 for FVI.
    // -------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
      ("PA_REFERRAL_AP_NF") || IsExitState("PA_REFERRAL_AR_NF"))
    {
      // -------------------------------------------
      // If the AP, AR or any CH for this
      // Referral has theIr Family Violence
      // Indicator turned on then display a
      // DISPLAY OK MSG with warning
      // about FVI.
      // -------------------------------------------
      if (!IsEmpty(export.Ap.PersonNumber))
      {
        // -------------------------------------------
        // If there is an AP person number then READ
        // the AP's FVI.
        // -------------------------------------------
        if (ReadCsePerson1())
        {
          if (!IsEmpty(entities.ExistingCsePerson.FamilyViolenceIndicator))
          {
            // -------------------------------------------
            // If the AP's FVI is on then turn on
            // the local FVI flag.
            // -------------------------------------------
            local.FamilyViolenceInd.Flag = "Y";
          }
        }
      }

      if (AsChar(local.FamilyViolenceInd.Flag) != 'Y')
      {
        // -------------------------------------------
        // If the local FVI flag is not on then
        // check to see if there is an AR on
        // the Referral.
        // -------------------------------------------
        if (!IsEmpty(export.Ar.PersonNumber))
        {
          // -------------------------------------------
          // If there is an AR person number then READ
          // the AR's FVI.
          // -------------------------------------------
          if (ReadCsePerson2())
          {
            if (!IsEmpty(entities.ExistingCsePerson.FamilyViolenceIndicator))
            {
              local.FamilyViolenceInd.Flag = "Y";
            }
          }
        }

        if (AsChar(local.FamilyViolenceInd.Flag) != 'Y')
        {
          for(export.Children.Index = 0; export.Children.Index < export
            .Children.Count; ++export.Children.Index)
          {
            if (!IsEmpty(export.Children.Item.GchildPaReferralParticipant.
              PersonNumber))
            {
              if (ReadCsePerson3())
              {
                if (!IsEmpty(entities.ExistingCsePerson.FamilyViolenceIndicator))
                  
                {
                  local.FamilyViolenceInd.Flag = "Y";

                  goto Test;
                }
              }
            }
          }
        }
      }

Test:

      if (AsChar(local.FamilyViolenceInd.Flag) == 'Y')
      {
        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          ExitState = "ACO_NI0000_DISPLAY_OK_FV";
        }
        else if (IsExitState("PA_REFERRAL_AR_NF"))
        {
          ExitState = "PA_REFERRAL_AR_NF_FV";
        }
        else if (IsExitState("PA_REFERRAL_AP_NF"))
        {
          ExitState = "PA_REFERRAL_AP_NF_FV";
        }
        else
        {
        }
      }
    }

    // -------------------------------------------
    // 04/02/00 W.Campbell - End of Inserted
    // code to implement the Family Violence Indicator
    // logic into this Pstep.  Work done on
    // WR#000162 for FVI.
    // -------------------------------------------
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.RelToAr = source.RelToAr;
  }

  private static void MoveChildren(SiPar1ReadPaReferral1.Export.
    ChildrenGroup source, Export.ChildrenGroup target)
  {
    target.GchildName.Text33 = source.GchildName.Text33;
    target.GchildPaReferralParticipant.
      Assign(source.GchildPaReferralParticipant);
    MoveCaseRole(source.GchildCaseRole, target.GchildCaseRole);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MovePaReferral1(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.ApPhoneNumber = source.ApPhoneNumber;
    target.ApAreaCode = source.ApAreaCode;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.AssignDeactivateInd = source.AssignDeactivateInd;
    target.AssignDeactivateDate = source.AssignDeactivateDate;
    target.CaseNumber = source.CaseNumber;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AssignmentDate = source.AssignmentDate;
    target.CseReferralRecDate = source.CseReferralRecDate;
    target.PgmCode = source.PgmCode;
    target.CaseWorker = source.CaseWorker;
  }

  private static void MovePaReferral2(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AssignmentDate = source.AssignmentDate;
  }

  private static void MovePaReferral3(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.Number = source.Number;
  }

  private static void MovePaReferral4(PaReferral source, PaReferral target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
  }

  private static void MoveToRegi(SiPar1ReadPaReferral1.Export.
    ToRegiGroup source, Export.ToRegiGroup target)
  {
    target.Gregi.Assign(source.Gregi);
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text32 = source.Text32;
    target.Text33 = source.Text33;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseEabUpdatePaReferral()
  {
    var useImport = new EabUpdatePaReferral.Import();
    var useExport = new EabUpdatePaReferral.Export();

    useImport.Case1.Number = import.RegiCase.Number;
    useImport.Current.Date = local.Current.Date;
    MovePaReferral3(export.PaReferral, useImport.PaReferral);
    useExport.AbendData.Assign(local.Eab);

    Call(EabUpdatePaReferral.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
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

  private void UseSiPar1DeactPaReferral()
  {
    var useImport = new SiPar1DeactPaReferral.Import();
    var useExport = new SiPar1DeactPaReferral.Export();

    useImport.PaReferral.Assign(local.PaReferral);

    Call(SiPar1DeactPaReferral.Execute, useImport, useExport);
  }

  private void UseSiPar1ReadPaReferral2()
  {
    var useImport = new SiPar1ReadPaReferral1.Import();
    var useExport = new SiPar1ReadPaReferral1.Export();

    MovePaReferral2(import.PaReferral, useImport.PaReferral);

    Call(SiPar1ReadPaReferral1.Execute, useImport, useExport);

    useExport.Children.CopyTo(export.Children, MoveChildren);
    local.Found.Flag = useExport.Found.Flag;
    local.CaseExistsWarning.Flag = useExport.CaseExists.Flag;
    local.MorePaReferrals.Flag = useExport.MorePaReferrals.Flag;
    export.OtherName.Text33 = useExport.OtherName.Text33;
    export.ApName.Text33 = useExport.ApName.Text33;
    export.ArName.Text33 = useExport.ArName.Text33;
    MovePaReferral1(useExport.PaReferral, export.PaReferral);
    export.ApMail.Assign(useExport.ApMail);
    export.ApHome.Assign(useExport.ApHome);
    export.Other.Assign(useExport.Other);
    export.Ar.Assign(useExport.Ar);
    export.Ap.Assign(useExport.Ap);
    useExport.ToRegi.CopyTo(export.ToRegi, MoveToRegi);
  }

  private void UseSiPar1ReadPaReferral3()
  {
    var useImport = new SiPar1ReadPaReferral1.Import();
    var useExport = new SiPar1ReadPaReferral1.Export();

    useImport.PaReferral.Assign(local.PaReferral);
    useImport.FromRefm.MenuOption = export.Standard.MenuOption;
    useImport.ServiceProvider.Assign(export.ServiceProvider);
    useImport.OfficeServiceProvider.Assign(export.OfficeServiceProvider);
    MoveOffice(export.Office, useImport.Office);

    Call(SiPar1ReadPaReferral1.Execute, useImport, useExport);

    local.Found.Flag = useExport.Found.Flag;
    local.CaseExistsWarning.Flag = useExport.CaseExists.Flag;
    local.MorePaReferrals.Flag = useExport.MorePaReferrals.Flag;
    export.OtherName.Text33 = useExport.OtherName.Text33;
    export.ApName.Text33 = useExport.ApName.Text33;
    export.ArName.Text33 = useExport.ArName.Text33;
    MovePaReferral1(useExport.PaReferral, export.PaReferral);
    export.ApMail.Assign(useExport.ApMail);
    export.ApHome.Assign(useExport.ApHome);
    export.Other.Assign(useExport.Other);
    export.Ar.Assign(useExport.Ar);
    export.Ap.Assign(useExport.Ap);
    useExport.ToRegi.CopyTo(export.ToRegi, MoveToRegi);
    useExport.Children.CopyTo(export.Children, MoveChildren);
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Ap.PersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Ar.PersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          export.Children.Item.GchildPaReferralParticipant.PersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadPaReferralParticipant()
  {
    entities.ExistingPaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant",
      (db, command) =>
      {
        db.SetString(command, "preNumber", export.PaReferral.Number);
        db.SetString(command, "pafType", export.PaReferral.Type1);
        db.SetDateTime(
          command, "pafTstamp",
          export.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetNullableString(command, "role", local.Ch.Role ?? "");
        db.SetNullableString(
          command, "personNum",
          export.Children.Item.GchildPaReferralParticipant.PersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingPaReferralParticipant.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaReferralParticipant.CreatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingPaReferralParticipant.Relationship =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralParticipant.PreNumber =
          db.GetString(reader, 4);
        entities.ExistingPaReferralParticipant.PafType =
          db.GetString(reader, 5);
        entities.ExistingPaReferralParticipant.PafTstamp =
          db.GetDateTime(reader, 6);
        entities.ExistingPaReferralParticipant.Role =
          db.GetNullableString(reader, 7);
        entities.ExistingPaReferralParticipant.Populated = true;
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
    /// <summary>A ToRegiGroup group.</summary>
    [Serializable]
    public class ToRegiGroup
    {
      /// <summary>
      /// A value of Gregi.
      /// </summary>
      [JsonPropertyName("gregi")]
      public CsePersonsWorkSet Gregi
      {
        get => gregi ??= new();
        set => gregi = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet gregi;
    }

    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GchildName.
      /// </summary>
      [JsonPropertyName("gchildName")]
      public WorkArea GchildName
      {
        get => gchildName ??= new();
        set => gchildName = value;
      }

      /// <summary>
      /// A value of GchildPaReferralParticipant.
      /// </summary>
      [JsonPropertyName("gchildPaReferralParticipant")]
      public PaReferralParticipant GchildPaReferralParticipant
      {
        get => gchildPaReferralParticipant ??= new();
        set => gchildPaReferralParticipant = value;
      }

      /// <summary>
      /// A value of GchildCaseRole.
      /// </summary>
      [JsonPropertyName("gchildCaseRole")]
      public CaseRole GchildCaseRole
      {
        get => gchildCaseRole ??= new();
        set => gchildCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea gchildName;
      private PaReferralParticipant gchildPaReferralParticipant;
      private CaseRole gchildCaseRole;
    }

    /// <summary>
    /// A value of Par1FromComn.
    /// </summary>
    [JsonPropertyName("par1FromComn")]
    public Common Par1FromComn
    {
      get => par1FromComn ??= new();
      set => par1FromComn = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of WarningMsg.
    /// </summary>
    [JsonPropertyName("warningMsg")]
    public WorkArea WarningMsg
    {
      get => warningMsg ??= new();
      set => warningMsg = value;
    }

    /// <summary>
    /// A value of BeenThere.
    /// </summary>
    [JsonPropertyName("beenThere")]
    public Common BeenThere
    {
      get => beenThere ??= new();
      set => beenThere = value;
    }

    /// <summary>
    /// A value of Deact.
    /// </summary>
    [JsonPropertyName("deact")]
    public WorkArea Deact
    {
      get => deact ??= new();
      set => deact = value;
    }

    /// <summary>
    /// A value of Screen.
    /// </summary>
    [JsonPropertyName("screen")]
    public PaReferral Screen
    {
      get => screen ??= new();
      set => screen = value;
    }

    /// <summary>
    /// A value of RegiCase.
    /// </summary>
    [JsonPropertyName("regiCase")]
    public Case1 RegiCase
    {
      get => regiCase ??= new();
      set => regiCase = value;
    }

    /// <summary>
    /// A value of RegiCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("regiCsePersonsWorkSet")]
    public CsePersonsWorkSet RegiCsePersonsWorkSet
    {
      get => regiCsePersonsWorkSet ??= new();
      set => regiCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of MoreReferralsMinus.
    /// </summary>
    [JsonPropertyName("moreReferralsMinus")]
    public WorkArea MoreReferralsMinus
    {
      get => moreReferralsMinus ??= new();
      set => moreReferralsMinus = value;
    }

    /// <summary>
    /// A value of MoreReferralsPlus.
    /// </summary>
    [JsonPropertyName("moreReferralsPlus")]
    public WorkArea MoreReferralsPlus
    {
      get => moreReferralsPlus ??= new();
      set => moreReferralsPlus = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of OtherName.
    /// </summary>
    [JsonPropertyName("otherName")]
    public WorkArea OtherName
    {
      get => otherName ??= new();
      set => otherName = value;
    }

    /// <summary>
    /// A value of ApName.
    /// </summary>
    [JsonPropertyName("apName")]
    public WorkArea ApName
    {
      get => apName ??= new();
      set => apName = value;
    }

    /// <summary>
    /// A value of ArName.
    /// </summary>
    [JsonPropertyName("arName")]
    public WorkArea ArName
    {
      get => arName ??= new();
      set => arName = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of ApMail.
    /// </summary>
    [JsonPropertyName("apMail")]
    public PaParticipantAddress ApMail
    {
      get => apMail ??= new();
      set => apMail = value;
    }

    /// <summary>
    /// A value of ApHome.
    /// </summary>
    [JsonPropertyName("apHome")]
    public PaParticipantAddress ApHome
    {
      get => apHome ??= new();
      set => apHome = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public PaReferral Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public PaReferralParticipant Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public PaReferralParticipant Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public PaReferralParticipant Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// Gets a value of ToRegi.
    /// </summary>
    [JsonIgnore]
    public Array<ToRegiGroup> ToRegi => toRegi ??= new(ToRegiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToRegi for json serialization.
    /// </summary>
    [JsonPropertyName("toRegi")]
    [Computed]
    public IList<ToRegiGroup> ToRegi_Json
    {
      get => toRegi;
      set => ToRegi.Assign(value);
    }

    /// <summary>
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Common par1FromComn;
    private Common fromPar1;
    private ServiceProviderAddress serviceProviderAddress;
    private WorkArea warningMsg;
    private Common beenThere;
    private WorkArea deact;
    private PaReferral screen;
    private Case1 regiCase;
    private CsePersonsWorkSet regiCsePersonsWorkSet;
    private CsePersonsWorkSet search;
    private WorkArea moreReferralsMinus;
    private WorkArea moreReferralsPlus;
    private Common saveSubscript;
    private WorkArea otherName;
    private WorkArea apName;
    private WorkArea arName;
    private PaReferral paReferral;
    private PaParticipantAddress apMail;
    private PaParticipantAddress apHome;
    private PaReferral save;
    private PaReferralParticipant other;
    private PaReferralParticipant ar;
    private PaReferralParticipant ap;
    private Standard standard;
    private Array<ToRegiGroup> toRegi;
    private Array<ChildrenGroup> children;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ToRegiGroup group.</summary>
    [Serializable]
    public class ToRegiGroup
    {
      /// <summary>
      /// A value of Gregi.
      /// </summary>
      [JsonPropertyName("gregi")]
      public CsePersonsWorkSet Gregi
      {
        get => gregi ??= new();
        set => gregi = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet gregi;
    }

    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GchildName.
      /// </summary>
      [JsonPropertyName("gchildName")]
      public WorkArea GchildName
      {
        get => gchildName ??= new();
        set => gchildName = value;
      }

      /// <summary>
      /// A value of GchildPaReferralParticipant.
      /// </summary>
      [JsonPropertyName("gchildPaReferralParticipant")]
      public PaReferralParticipant GchildPaReferralParticipant
      {
        get => gchildPaReferralParticipant ??= new();
        set => gchildPaReferralParticipant = value;
      }

      /// <summary>
      /// A value of GchildCaseRole.
      /// </summary>
      [JsonPropertyName("gchildCaseRole")]
      public CaseRole GchildCaseRole
      {
        get => gchildCaseRole ??= new();
        set => gchildCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WorkArea gchildName;
      private PaReferralParticipant gchildPaReferralParticipant;
      private CaseRole gchildCaseRole;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of Screen.
    /// </summary>
    [JsonPropertyName("screen")]
    public PaReferral Screen
    {
      get => screen ??= new();
      set => screen = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of AsinObject.
    /// </summary>
    [JsonPropertyName("asinObject")]
    public SpTextWorkArea AsinObject
    {
      get => asinObject ??= new();
      set => asinObject = value;
    }

    /// <summary>
    /// A value of InitialExecution.
    /// </summary>
    [JsonPropertyName("initialExecution")]
    public Common InitialExecution
    {
      get => initialExecution ??= new();
      set => initialExecution = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of WarningMsg.
    /// </summary>
    [JsonPropertyName("warningMsg")]
    public WorkArea WarningMsg
    {
      get => warningMsg ??= new();
      set => warningMsg = value;
    }

    /// <summary>
    /// A value of BeenThere.
    /// </summary>
    [JsonPropertyName("beenThere")]
    public Common BeenThere
    {
      get => beenThere ??= new();
      set => beenThere = value;
    }

    /// <summary>
    /// A value of Deact.
    /// </summary>
    [JsonPropertyName("deact")]
    public WorkArea Deact
    {
      get => deact ??= new();
      set => deact = value;
    }

    /// <summary>
    /// A value of RegiCase.
    /// </summary>
    [JsonPropertyName("regiCase")]
    public Case1 RegiCase
    {
      get => regiCase ??= new();
      set => regiCase = value;
    }

    /// <summary>
    /// A value of RegiCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("regiCsePersonsWorkSet")]
    public CsePersonsWorkSet RegiCsePersonsWorkSet
    {
      get => regiCsePersonsWorkSet ??= new();
      set => regiCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of MoreReferralsMinus.
    /// </summary>
    [JsonPropertyName("moreReferralsMinus")]
    public WorkArea MoreReferralsMinus
    {
      get => moreReferralsMinus ??= new();
      set => moreReferralsMinus = value;
    }

    /// <summary>
    /// A value of MoreReferralsPlus.
    /// </summary>
    [JsonPropertyName("moreReferralsPlus")]
    public WorkArea MoreReferralsPlus
    {
      get => moreReferralsPlus ??= new();
      set => moreReferralsPlus = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of OtherName.
    /// </summary>
    [JsonPropertyName("otherName")]
    public WorkArea OtherName
    {
      get => otherName ??= new();
      set => otherName = value;
    }

    /// <summary>
    /// A value of ApName.
    /// </summary>
    [JsonPropertyName("apName")]
    public WorkArea ApName
    {
      get => apName ??= new();
      set => apName = value;
    }

    /// <summary>
    /// A value of ArName.
    /// </summary>
    [JsonPropertyName("arName")]
    public WorkArea ArName
    {
      get => arName ??= new();
      set => arName = value;
    }

    /// <summary>
    /// A value of HiddenCommon.
    /// </summary>
    [JsonPropertyName("hiddenCommon")]
    public Common HiddenCommon
    {
      get => hiddenCommon ??= new();
      set => hiddenCommon = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of ApMail.
    /// </summary>
    [JsonPropertyName("apMail")]
    public PaParticipantAddress ApMail
    {
      get => apMail ??= new();
      set => apMail = value;
    }

    /// <summary>
    /// A value of ApHome.
    /// </summary>
    [JsonPropertyName("apHome")]
    public PaParticipantAddress ApHome
    {
      get => apHome ??= new();
      set => apHome = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public PaReferral Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public PaReferralParticipant Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public PaReferralParticipant Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public PaReferralParticipant Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// Gets a value of ToRegi.
    /// </summary>
    [JsonIgnore]
    public Array<ToRegiGroup> ToRegi => toRegi ??= new(ToRegiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToRegi for json serialization.
    /// </summary>
    [JsonPropertyName("toRegi")]
    [Computed]
    public IList<ToRegiGroup> ToRegi_Json
    {
      get => toRegi;
      set => ToRegi.Assign(value);
    }

    /// <summary>
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private DateWorkArea current;
    private AbendData eab;
    private PaReferral screen;
    private Common fromPar1;
    private ServiceProviderAddress serviceProviderAddress;
    private SpTextWorkArea asinObject;
    private Common initialExecution;
    private Common phonetic;
    private WorkArea warningMsg;
    private Common beenThere;
    private WorkArea deact;
    private Case1 regiCase;
    private CsePersonsWorkSet regiCsePersonsWorkSet;
    private CsePersonsWorkSet search;
    private WorkArea moreReferralsMinus;
    private WorkArea moreReferralsPlus;
    private Common saveSubscript;
    private WorkArea otherName;
    private WorkArea apName;
    private WorkArea arName;
    private Common hiddenCommon;
    private PaReferral paReferral;
    private PaParticipantAddress apMail;
    private PaParticipantAddress apHome;
    private PaReferral save;
    private PaReferralParticipant other;
    private PaReferralParticipant ar;
    private PaReferralParticipant ap;
    private Standard standard;
    private Array<ToRegiGroup> toRegi;
    private Array<ChildrenGroup> children;
    private NextTranInfo hiddenNextTranInfo;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public PaReferralParticipant Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of FamilyViolenceInd.
    /// </summary>
    [JsonPropertyName("familyViolenceInd")]
    public Common FamilyViolenceInd
    {
      get => familyViolenceInd ??= new();
      set => familyViolenceInd = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of CseReferralReceived.
    /// </summary>
    [JsonPropertyName("cseReferralReceived")]
    public DateWorkArea CseReferralReceived
    {
      get => cseReferralReceived ??= new();
      set => cseReferralReceived = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of CaseExistsWarning.
    /// </summary>
    [JsonPropertyName("caseExistsWarning")]
    public Common CaseExistsWarning
    {
      get => caseExistsWarning ??= new();
      set => caseExistsWarning = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of MorePaReferrals.
    /// </summary>
    [JsonPropertyName("morePaReferrals")]
    public Common MorePaReferrals
    {
      get => morePaReferrals ??= new();
      set => morePaReferrals = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private PaReferralParticipant ch;
    private Common familyViolenceInd;
    private Common found;
    private DateWorkArea zero;
    private DateWorkArea cseReferralReceived;
    private DateWorkArea current;
    private TextWorkArea zeroFill;
    private Common caseExistsWarning;
    private AbendData eab;
    private Common morePaReferrals;
    private PaReferral paReferral;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPaReferral.
    /// </summary>
    [JsonPropertyName("existingPaReferral")]
    public PaReferral ExistingPaReferral
    {
      get => existingPaReferral ??= new();
      set => existingPaReferral = value;
    }

    /// <summary>
    /// A value of ExistingPaReferralParticipant.
    /// </summary>
    [JsonPropertyName("existingPaReferralParticipant")]
    public PaReferralParticipant ExistingPaReferralParticipant
    {
      get => existingPaReferralParticipant ??= new();
      set => existingPaReferralParticipant = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private PaReferral existingPaReferral;
    private PaReferralParticipant existingPaReferralParticipant;
    private CsePerson existingCsePerson;
  }
#endregion
}
