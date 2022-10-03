// Program: SI_PAR1_READ_PA_REFERRAL_1, ID: 371759864, model: 746.
// Short name: SWE01229
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PAR1_READ_PA_REFERRAL_1.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiPar1ReadPaReferral1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PAR1_READ_PA_REFERRAL_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPar1ReadPaReferral1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPar1ReadPaReferral1.
  /// </summary>
  public SiPar1ReadPaReferral1(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //      M A I N T E N A N C E   L O G
    //  Date    Developer   	Description
    // 8-29-95  Ken Evans   	Initial development
    // 5-03-96  Rao Mulpuri 	Changes to Read PA Ref
    // 1-08-96	 Sid Chowdhary	Change reads to include OSP assignment.
    // 04-30-97 JF. Caillouet	Change Current Date
    // 07-03-97 Sid Chowdhary  Modify read for option 4 to allow
    // 			scrolling by agency type.
    // 07-16-97 Sid		Display referral on flow from COMN.
    // 09/30/98  W. Campbell   Modified this CAB so that
    //                         it would work with the scrolling
    //                         logic changes made in the calling
    //                         PRAD (SI_PAR1_PA_REFERRAL_PAGE_1)
    //                         in order to fix the problems with
    //                         PApv and PAnx (PFK 19 and 20) so
    //                         that scrolling would work correctly.
    // ------------------------------------------------------------
    // 06/29/99 M.Lachowicz    Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 05/04/00 W.Campbell     Changed exit states for
    //                         REFERRAL_AP_NF
    //                         and REFERRAL_AR_NF.
    //                         Work done on WR# 000162
    //                         for Family Violence.
    // ------------------------------------------------------------
    // 06/19/00 M.Lachowicz    Changed sort order of PA Referrals.
    //                         PA Referrals should be sorted in
    //                         ascending sequence of Approval Date,
    //                         Referral No. and created timestamp.
    //                         Added Assignment Date only to
    //                         local view.
    //                         Work done on PR # 97520.
    // ------------------------------------------------------------
    // 10/10/00 M.Lachowicz    Do not display
    //                         PA referral if the type is 'NEW' and
    //                         exist another PA referral with type 'NEW'
    //                         with greater timestamp.
    //                         Work done on PR # 103514.
    // ------------------------------------------------------------
    // 06/26/01 C Fairley I00122128 Added CASE_ROLE to the group view
    //                              Removed OLD commented OUT code.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    MovePaReferral(import.PaReferral, export.PaReferral);
    export.Found.Flag = "N";

    if (!Equal(global.Command, "FROMCOMN"))
    {
      // *** 06/29/99 M.L         Change property of READ to generate 'Select 
      // Only'
      if (!ReadOfficeServiceProvider())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(import.PaReferral.Number))
      {
        // *** 06/19/00  M.L
        // Changed sort order from number, created timestamp to
        // assignment_date, number and created timestamp.
        foreach(var item in ReadPaReferralPaReferralAssignment2())
        {
          if (AsChar(export.Found.Flag) == 'Y')
          {
            export.MorePaReferrals.Flag = "Y";

            break;
          }

          export.Found.Flag = "Y";
          export.PaReferral.Assign(entities.ExistingPaReferral);
          local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;
        }

        if (AsChar(export.Found.Flag) == 'N')
        {
          ExitState = "NO_PENDING_REFERRALS";

          return;
        }
      }
      else
      {
        foreach(var item in ReadPaReferral4())
        {
          // *** 10/10/00 M.L Start
          if (Equal(entities.ExistingPaReferral.Type1, "NEW"))
          {
            foreach(var item1 in ReadPaReferral5())
            {
              goto ReadEach1;
            }
          }

          // *** 10/10/00 M.L End
          if (AsChar(export.Found.Flag) == 'Y')
          {
            export.MorePaReferrals.Flag = "Y";

            goto Test;
          }

          export.Found.Flag = "Y";
          export.PaReferral.Assign(entities.ExistingPaReferral);
          local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;

ReadEach1:
          ;
        }

        if (AsChar(export.Found.Flag) == 'N')
        {
          ExitState = "PA_REFERRAL_NF";

          return;
        }
      }
    }
    else
    {
      if (import.FromRefm.MenuOption != 4)
      {
        if (Equal(global.Command, "PANX"))
        {
          // *** 06/19/00  M.L  Start
          // Changed sort order from number, created timestamp to
          // assignment_date, number and created timestamp.
          foreach(var item in ReadPaReferralPaReferralAssignment1())
          {
            if (AsChar(export.Found.Flag) == 'Y')
            {
              export.MorePaReferrals.Flag = "Y";

              break;
            }

            export.Found.Flag = "Y";
            export.PaReferral.Assign(entities.ExistingPaReferral);
            local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;
          }

          // 06/19/00  M.L  End
          if (AsChar(export.Found.Flag) == 'N')
          {
            ExitState = "NO_MORE_REFERRALS_FOUND";
          }
        }

        if (Equal(global.Command, "PAPV"))
        {
          // 06/19/00  M.L  Start
          // Changed sort order from number, created timestamp to
          // assignment_date, number and created timestamp.
          foreach(var item in ReadPaReferralPaReferralAssignment3())
          {
            if (AsChar(export.Found.Flag) == 'Y')
            {
              export.MorePaReferrals.Flag = "Y";

              break;
            }

            export.Found.Flag = "Y";
            export.PaReferral.Assign(entities.ExistingPaReferral);
            local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;
          }

          // *** 06/19/00  M.L  End
          if (AsChar(export.Found.Flag) == 'N')
          {
            ExitState = "ACO_NE0000_INVALID_BACKWARD";
          }
        }

        if (Equal(global.Command, "FROMCOMN"))
        {
          // *** 06/29/99 M.L         Change property of READ to generate '
          // Select Only'
          if (ReadPaReferral1())
          {
            export.Found.Flag = "Y";
            export.PaReferral.Assign(entities.ExistingPaReferral);
            local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;
          }
        }
      }
      else
      {
        if (Equal(global.Command, "PANX"))
        {
          foreach(var item in ReadPaReferral2())
          {
            // *** 10/10/00 M.L Start
            if (Equal(entities.ExistingPaReferral.Type1, "NEW"))
            {
              foreach(var item1 in ReadPaReferral5())
              {
                goto ReadEach2;
              }
            }

            // *** 10/10/00 M.L End
            if (AsChar(export.Found.Flag) == 'Y')
            {
              export.MorePaReferrals.Flag = "Y";

              break;
            }

            export.Found.Flag = "Y";
            export.PaReferral.Assign(entities.ExistingPaReferral);
            local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;

ReadEach2:
            ;
          }

          if (AsChar(export.Found.Flag) == 'N')
          {
            ExitState = "NO_MORE_REFERRALS_FOUND";
          }
        }

        if (Equal(global.Command, "PAPV"))
        {
          foreach(var item in ReadPaReferral3())
          {
            // *** 10/10/00 M.L Start
            if (Equal(entities.ExistingPaReferral.Type1, "NEW"))
            {
              foreach(var item1 in ReadPaReferral5())
              {
                goto ReadEach3;
              }
            }

            // *** 10/10/00 M.L End
            if (AsChar(export.Found.Flag) == 'Y')
            {
              export.MorePaReferrals.Flag = "Y";

              break;
            }

            export.Found.Flag = "Y";
            export.PaReferral.Assign(entities.ExistingPaReferral);
            local.Program.PgmCode = entities.ExistingPaReferral.PgmCode;

ReadEach3:
            ;
          }

          if (AsChar(export.Found.Flag) == 'N')
          {
            ExitState = "ACO_NE0000_INVALID_BACKWARD";
          }
        }
      }

      if (AsChar(export.Found.Flag) == 'N')
      {
        return;
      }
    }

Test:

    // *********************************************
    // Find out if there is any CASE already existing
    // with the same AP/AR/Child Combination      *
    // *********************************************
    export.CaseExists.Flag = export.PaReferral.CseInvolvementInd ?? Spaces(1);
    export.ToRegi.Index = -1;

    // *********************************************
    // * After finding a PA Referral, retrieve the *
    // * Participants and AP address to complete   *
    // * the display.
    // 
    // *
    // *********************************************
    // *** 06/29/99 M.L         Change property of READ to generate 'Select 
    // Only'
    if (ReadPaReferralParticipant2())
    {
      MovePaReferralParticipant2(entities.ExistingPaReferralParticipant,
        export.Ar);
      local.CsePersonsWorkSet.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      UseSiFormatCsePersonName();
      export.ArName.Text33 = local.CsePersonsWorkSet.FormattedName;

      ++export.ToRegi.Index;
      export.ToRegi.CheckSize();

      export.ToRegi.Update.Gregi.Number =
        entities.ExistingPaReferralParticipant.PersonNumber ?? Spaces(10);
      export.ToRegi.Update.Gregi.Dob = export.Ar.Dob;
      export.ToRegi.Update.Gregi.Ssn = export.Ar.Ssn ?? Spaces(9);
      export.ToRegi.Update.Gregi.Sex =
        entities.ExistingPaReferralParticipant.Sex ?? Spaces(1);
      export.ToRegi.Update.Gregi.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      export.ToRegi.Update.Gregi.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      export.ToRegi.Update.Gregi.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      export.ToRegi.Update.Gregi.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
      export.ToRegi.Update.Gregi.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
    }
    else
    {
      // ------------------------------------------------------------
      // 05/04/00 W.Campbell - Changed exit states
      // for REFERRAL_AP_NF and REFERRAL_AR_NF.
      // Work done on WR# 000162 for Family Violence.
      // ------------------------------------------------------------
      ExitState = "PA_REFERRAL_AR_NF";
    }

    // *** 06/29/99 M.L         Change property of READ to generate 'Select 
    // Only'
    if (ReadPaReferralParticipant1())
    {
      MovePaReferralParticipant2(entities.ExistingPaReferralParticipant,
        export.Ap);
      local.CsePersonsWorkSet.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      UseSiFormatCsePersonName();
      export.ApName.Text33 = local.CsePersonsWorkSet.FormattedName;

      ++export.ToRegi.Index;
      export.ToRegi.CheckSize();

      export.ToRegi.Update.Gregi.Number =
        entities.ExistingPaReferralParticipant.PersonNumber ?? Spaces(10);
      export.ToRegi.Update.Gregi.Dob =
        entities.ExistingPaReferralParticipant.Dob;
      export.ToRegi.Update.Gregi.Ssn =
        entities.ExistingPaReferralParticipant.Ssn ?? Spaces(9);
      export.ToRegi.Update.Gregi.Sex =
        entities.ExistingPaReferralParticipant.Sex ?? Spaces(1);
      export.ToRegi.Update.Gregi.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      export.ToRegi.Update.Gregi.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      export.ToRegi.Update.Gregi.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      export.ToRegi.Update.Gregi.FormattedName =
        local.CsePersonsWorkSet.FormattedName;

      foreach(var item in ReadPaParticipantAddress())
      {
        if (AsChar(entities.ExistingPaParticipantAddress.Type1) == 'R')
        {
          export.ApHome.Assign(entities.ExistingPaParticipantAddress);
        }
        else if (AsChar(entities.ExistingPaParticipantAddress.Type1) == 'M')
        {
          export.ApMail.Assign(entities.ExistingPaParticipantAddress);
        }
      }
    }
    else
    {
      // ------------------------------------------------------------
      // 05/04/00 W.Campbell - Changed exit states
      // for REFERRAL_AP_NF and REFERRAL_AR_NF.
      // Work done on WR# 000162 for Family Violence.
      // ------------------------------------------------------------
      ExitState = "PA_REFERRAL_AP_NF";
    }

    // *** 06/29/99 M.L         Change property of READ to generate 'Select 
    // Only'
    if (ReadPaReferralParticipant3())
    {
      MovePaReferralParticipant2(entities.ExistingPaReferralParticipant,
        export.Other);
      local.CsePersonsWorkSet.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      UseSiFormatCsePersonName();
      export.OtherName.Text33 = local.CsePersonsWorkSet.FormattedName;

      ++export.ToRegi.Index;
      export.ToRegi.CheckSize();

      export.ToRegi.Update.Gregi.Number =
        entities.ExistingPaReferralParticipant.PersonNumber ?? Spaces(10);
      export.ToRegi.Update.Gregi.Dob =
        entities.ExistingPaReferralParticipant.Dob;
      export.ToRegi.Update.Gregi.Ssn =
        entities.ExistingPaReferralParticipant.Ssn ?? Spaces(9);
      export.ToRegi.Update.Gregi.Sex =
        entities.ExistingPaReferralParticipant.Sex ?? Spaces(1);
      export.ToRegi.Update.Gregi.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      export.ToRegi.Update.Gregi.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      export.ToRegi.Update.Gregi.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      export.ToRegi.Update.Gregi.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
    }
    else
    {
      // *********************************************
      // There may not be a custodial parent.
      // *********************************************
    }

    export.Children.Index = 0;
    export.Children.Clear();

    foreach(var item in ReadPaReferralParticipant4())
    {
      MovePaReferralParticipant1(entities.ExistingPaReferralParticipant,
        export.Children.Update.GchildPaReferralParticipant);
      local.CsePersonsWorkSet.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      local.CsePersonsWorkSet.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      local.CsePersonsWorkSet.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      UseSiFormatCsePersonName();
      export.Children.Update.GchildName.Text33 =
        local.CsePersonsWorkSet.FormattedName;

      ++export.ToRegi.Index;
      export.ToRegi.CheckSize();

      export.ToRegi.Update.Gregi.Number =
        entities.ExistingPaReferralParticipant.PersonNumber ?? Spaces(10);
      export.ToRegi.Update.Gregi.Dob =
        entities.ExistingPaReferralParticipant.Dob;
      export.ToRegi.Update.Gregi.Ssn =
        entities.ExistingPaReferralParticipant.Ssn ?? Spaces(9);
      export.ToRegi.Update.Gregi.Sex =
        entities.ExistingPaReferralParticipant.Sex ?? Spaces(1);
      export.ToRegi.Update.Gregi.FirstName =
        entities.ExistingPaReferralParticipant.FirstName ?? Spaces(12);
      export.ToRegi.Update.Gregi.MiddleInitial =
        entities.ExistingPaReferralParticipant.Mi ?? Spaces(1);
      export.ToRegi.Update.Gregi.LastName =
        entities.ExistingPaReferralParticipant.LastName ?? Spaces(17);
      export.ToRegi.Update.Gregi.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
      export.Children.Next();
    }
  }

  private static void MovePaReferral(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AssignmentDate = source.AssignmentDate;
  }

  private static void MovePaReferralParticipant1(PaReferralParticipant source,
    PaReferralParticipant target)
  {
    target.AbsenceCode = source.AbsenceCode;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.PersonNumber = source.PersonNumber;
    target.InsurInd = source.InsurInd;
    target.PatEstInd = source.PatEstInd;
    target.BeneInd = source.BeneInd;
  }

  private static void MovePaReferralParticipant2(PaReferralParticipant source,
    PaReferralParticipant target)
  {
    target.Relationship = source.Relationship;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.PersonNumber = source.PersonNumber;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaParticipantAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPaReferralParticipant.Populated);
    entities.ExistingPaParticipantAddress.Populated = false;

    return ReadEach("ReadPaParticipantAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.ExistingPaReferralParticipant.PafTstamp.GetValueOrDefault());
          
        db.SetString(
          command, "preNumber",
          entities.ExistingPaReferralParticipant.PreNumber);
        db.SetString(
          command, "pafType", entities.ExistingPaReferralParticipant.PafType);
        db.SetInt32(
          command, "prpIdentifier",
          entities.ExistingPaReferralParticipant.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPaParticipantAddress.Type1 =
          db.GetNullableString(reader, 0);
        entities.ExistingPaParticipantAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.ExistingPaParticipantAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExistingPaParticipantAddress.City =
          db.GetNullableString(reader, 3);
        entities.ExistingPaParticipantAddress.State =
          db.GetNullableString(reader, 4);
        entities.ExistingPaParticipantAddress.Zip =
          db.GetNullableString(reader, 5);
        entities.ExistingPaParticipantAddress.Zip4 =
          db.GetNullableString(reader, 6);
        entities.ExistingPaParticipantAddress.Zip3 =
          db.GetNullableString(reader, 7);
        entities.ExistingPaParticipantAddress.Identifier =
          db.GetInt32(reader, 8);
        entities.ExistingPaParticipantAddress.PrpIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingPaParticipantAddress.PafType =
          db.GetString(reader, 10);
        entities.ExistingPaParticipantAddress.PreNumber =
          db.GetString(reader, 11);
        entities.ExistingPaParticipantAddress.PafTstamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPaParticipantAddress.Populated = true;

        return true;
      });
  }

  private bool ReadPaReferral1()
  {
    entities.ExistingPaReferral.Populated = false;

    return Read("ReadPaReferral1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetString(command, "type", import.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaReferral2()
  {
    entities.ExistingPaReferral.Populated = false;

    return ReadEach("ReadPaReferral2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetNullableString(
          command, "referralFrom", import.PaReferral.From ?? "");
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaReferral3()
  {
    entities.ExistingPaReferral.Populated = false;

    return ReadEach("ReadPaReferral3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetNullableString(
          command, "referralFrom", import.PaReferral.From ?? "");
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaReferral4()
  {
    entities.ExistingPaReferral.Populated = false;

    return ReadEach("ReadPaReferral4",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetNullableString(
          command, "referralFrom", import.PaReferral.From ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaReferral5()
  {
    entities.CheckThis.Populated = false;

    return ReadEach("ReadPaReferral5",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetDateTime(
          command, "createdTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CheckThis.Number = db.GetString(reader, 0);
        entities.CheckThis.Type1 = db.GetString(reader, 1);
        entities.CheckThis.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CheckThis.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaReferralPaReferralAssignment1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingPaReferralAssignment.Populated = false;
    entities.ExistingPaReferral.Populated = false;

    return ReadEach("ReadPaReferralPaReferralAssignment1",
      (db, command) =>
      {
        db.SetString(command, "type", import.PaReferral.Type1);
        db.SetNullableDate(
          command, "assignmentDate",
          import.PaReferral.AssignmentDate.GetValueOrDefault());
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferralAssignment.PafNo = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferralAssignment.PafType = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferralAssignment.PafTstamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferralAssignment.ReasonCode =
          db.GetString(reader, 82);
        entities.ExistingPaReferralAssignment.OverrideInd =
          db.GetString(reader, 83);
        entities.ExistingPaReferralAssignment.EffectiveDate =
          db.GetDate(reader, 84);
        entities.ExistingPaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 85);
        entities.ExistingPaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 86);
        entities.ExistingPaReferralAssignment.SpdId = db.GetInt32(reader, 87);
        entities.ExistingPaReferralAssignment.OffId = db.GetInt32(reader, 88);
        entities.ExistingPaReferralAssignment.OspCode =
          db.GetString(reader, 89);
        entities.ExistingPaReferralAssignment.OspDate = db.GetDate(reader, 90);
        entities.ExistingPaReferralAssignment.Populated = true;
        entities.ExistingPaReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaReferralPaReferralAssignment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingPaReferralAssignment.Populated = false;
    entities.ExistingPaReferral.Populated = false;

    return ReadEach("ReadPaReferralPaReferralAssignment2",
      (db, command) =>
      {
        db.SetString(command, "type", import.PaReferral.Type1);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferralAssignment.PafNo = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferralAssignment.PafType = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferralAssignment.PafTstamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferralAssignment.ReasonCode =
          db.GetString(reader, 82);
        entities.ExistingPaReferralAssignment.OverrideInd =
          db.GetString(reader, 83);
        entities.ExistingPaReferralAssignment.EffectiveDate =
          db.GetDate(reader, 84);
        entities.ExistingPaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 85);
        entities.ExistingPaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 86);
        entities.ExistingPaReferralAssignment.SpdId = db.GetInt32(reader, 87);
        entities.ExistingPaReferralAssignment.OffId = db.GetInt32(reader, 88);
        entities.ExistingPaReferralAssignment.OspCode =
          db.GetString(reader, 89);
        entities.ExistingPaReferralAssignment.OspDate = db.GetDate(reader, 90);
        entities.ExistingPaReferralAssignment.Populated = true;
        entities.ExistingPaReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaReferralPaReferralAssignment3()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingPaReferralAssignment.Populated = false;
    entities.ExistingPaReferral.Populated = false;

    return ReadEach("ReadPaReferralPaReferralAssignment3",
      (db, command) =>
      {
        db.SetString(command, "type", import.PaReferral.Type1);
        db.SetNullableDate(
          command, "assignmentDate",
          import.PaReferral.AssignmentDate.GetValueOrDefault());
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingPaReferral.Number = db.GetString(reader, 0);
        entities.ExistingPaReferralAssignment.PafNo = db.GetString(reader, 0);
        entities.ExistingPaReferral.ReceivedDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingPaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaReferral.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferral.Type1 = db.GetString(reader, 5);
        entities.ExistingPaReferralAssignment.PafType = db.GetString(reader, 5);
        entities.ExistingPaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingPaReferral.MedicalFreq =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingPaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingPaReferral.MedicalOrderState =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingPaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 17);
        entities.ExistingPaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingPaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 20);
        entities.ExistingPaReferral.AssignmentDate =
          db.GetNullableDate(reader, 21);
        entities.ExistingPaReferral.CseRegion =
          db.GetNullableString(reader, 22);
        entities.ExistingPaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 23);
        entities.ExistingPaReferral.ArRetainedInd =
          db.GetNullableString(reader, 24);
        entities.ExistingPaReferral.PgmCode = db.GetNullableString(reader, 25);
        entities.ExistingPaReferral.CaseWorker =
          db.GetNullableString(reader, 26);
        entities.ExistingPaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 27);
        entities.ExistingPaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingPaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingPaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingPaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingPaReferral.GoodCauseCode =
          db.GetNullableString(reader, 32);
        entities.ExistingPaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingPaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 34);
        entities.ExistingPaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingPaReferral.SupportOrderId =
          db.GetNullableString(reader, 36);
        entities.ExistingPaReferral.LastApContactDate =
          db.GetNullableDate(reader, 37);
        entities.ExistingPaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 38);
        entities.ExistingPaReferral.ApEmployerName =
          db.GetNullableString(reader, 39);
        entities.ExistingPaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 40);
        entities.ExistingPaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 41);
        entities.ExistingPaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.ExistingPaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.ExistingPaReferral.FcCincInd =
          db.GetNullableString(reader, 44);
        entities.ExistingPaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 45);
        entities.ExistingPaReferral.FcSrsPayee =
          db.GetNullableString(reader, 46);
        entities.ExistingPaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 47);
        entities.ExistingPaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 48);
        entities.ExistingPaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 49);
        entities.ExistingPaReferral.FcPlacementType =
          db.GetNullableString(reader, 50);
        entities.ExistingPaReferral.FcPreviousPa =
          db.GetNullableString(reader, 51);
        entities.ExistingPaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 52);
        entities.ExistingPaReferral.FcRightsSevered =
          db.GetNullableString(reader, 53);
        entities.ExistingPaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingPaReferral.FcPlacementName =
          db.GetNullableString(reader, 55);
        entities.ExistingPaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 56);
        entities.ExistingPaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 57);
        entities.ExistingPaReferral.FcZebInd = db.GetNullableString(reader, 58);
        entities.ExistingPaReferral.FcVaInd = db.GetNullableString(reader, 59);
        entities.ExistingPaReferral.FcSsi = db.GetNullableString(reader, 60);
        entities.ExistingPaReferral.FcSsa = db.GetNullableString(reader, 61);
        entities.ExistingPaReferral.FcWardsAccount =
          db.GetNullableString(reader, 62);
        entities.ExistingPaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 63);
        entities.ExistingPaReferral.FcApNotified =
          db.GetNullableString(reader, 64);
        entities.ExistingPaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 65);
        entities.ExistingPaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.ExistingPaReferral.CreatedBy =
          db.GetNullableString(reader, 67);
        entities.ExistingPaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferralAssignment.PafTstamp =
          db.GetDateTime(reader, 68);
        entities.ExistingPaReferral.KsCounty = db.GetNullableString(reader, 69);
        entities.ExistingPaReferral.Note = db.GetNullableString(reader, 70);
        entities.ExistingPaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 71);
        entities.ExistingPaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 72);
        entities.ExistingPaReferral.CsOrderPlace =
          db.GetNullableString(reader, 73);
        entities.ExistingPaReferral.CsOrderState =
          db.GetNullableString(reader, 74);
        entities.ExistingPaReferral.CsFreq = db.GetNullableString(reader, 75);
        entities.ExistingPaReferral.From = db.GetNullableString(reader, 76);
        entities.ExistingPaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 77);
        entities.ExistingPaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 78);
        entities.ExistingPaReferral.CcStartDate =
          db.GetNullableDate(reader, 79);
        entities.ExistingPaReferral.ArEmployerName =
          db.GetNullableString(reader, 80);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferralAssignment.ReasonCode =
          db.GetString(reader, 82);
        entities.ExistingPaReferralAssignment.OverrideInd =
          db.GetString(reader, 83);
        entities.ExistingPaReferralAssignment.EffectiveDate =
          db.GetDate(reader, 84);
        entities.ExistingPaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 85);
        entities.ExistingPaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 86);
        entities.ExistingPaReferralAssignment.SpdId = db.GetInt32(reader, 87);
        entities.ExistingPaReferralAssignment.OffId = db.GetInt32(reader, 88);
        entities.ExistingPaReferralAssignment.OspCode =
          db.GetString(reader, 89);
        entities.ExistingPaReferralAssignment.OspDate = db.GetDate(reader, 90);
        entities.ExistingPaReferralAssignment.Populated = true;
        entities.ExistingPaReferral.Populated = true;

        return true;
      });
  }

  private bool ReadPaReferralParticipant1()
  {
    entities.ExistingPaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant1",
      (db, command) =>
      {
        db.SetString(command, "preNumber", export.PaReferral.Number);
        db.SetString(command, "pafType", export.PaReferral.Type1);
        db.SetDateTime(
          command, "pafTstamp",
          export.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferralParticipant.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 1);
        entities.ExistingPaReferralParticipant.Relationship =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferralParticipant.Sex =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralParticipant.Dob =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaReferralParticipant.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPaReferralParticipant.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPaReferralParticipant.Mi =
          db.GetNullableString(reader, 7);
        entities.ExistingPaReferralParticipant.Ssn =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingPaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 10);
        entities.ExistingPaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 11);
        entities.ExistingPaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferralParticipant.PreNumber =
          db.GetString(reader, 13);
        entities.ExistingPaReferralParticipant.PafType =
          db.GetString(reader, 14);
        entities.ExistingPaReferralParticipant.PafTstamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPaReferralParticipant.Role =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferralParticipant.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant2()
  {
    entities.ExistingPaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant2",
      (db, command) =>
      {
        db.SetString(command, "preNumber", export.PaReferral.Number);
        db.SetString(command, "pafType", export.PaReferral.Type1);
        db.SetDateTime(
          command, "pafTstamp",
          export.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferralParticipant.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 1);
        entities.ExistingPaReferralParticipant.Relationship =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferralParticipant.Sex =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralParticipant.Dob =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaReferralParticipant.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPaReferralParticipant.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPaReferralParticipant.Mi =
          db.GetNullableString(reader, 7);
        entities.ExistingPaReferralParticipant.Ssn =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingPaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 10);
        entities.ExistingPaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 11);
        entities.ExistingPaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferralParticipant.PreNumber =
          db.GetString(reader, 13);
        entities.ExistingPaReferralParticipant.PafType =
          db.GetString(reader, 14);
        entities.ExistingPaReferralParticipant.PafTstamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPaReferralParticipant.Role =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferralParticipant.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant3()
  {
    entities.ExistingPaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant3",
      (db, command) =>
      {
        db.SetString(command, "preNumber", export.PaReferral.Number);
        db.SetString(command, "pafType", export.PaReferral.Type1);
        db.SetDateTime(
          command, "pafTstamp",
          export.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaReferralParticipant.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 1);
        entities.ExistingPaReferralParticipant.Relationship =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferralParticipant.Sex =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralParticipant.Dob =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaReferralParticipant.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPaReferralParticipant.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPaReferralParticipant.Mi =
          db.GetNullableString(reader, 7);
        entities.ExistingPaReferralParticipant.Ssn =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingPaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 10);
        entities.ExistingPaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 11);
        entities.ExistingPaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferralParticipant.PreNumber =
          db.GetString(reader, 13);
        entities.ExistingPaReferralParticipant.PafType =
          db.GetString(reader, 14);
        entities.ExistingPaReferralParticipant.PafTstamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPaReferralParticipant.Role =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferralParticipant.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaReferralParticipant4()
  {
    return ReadEach("ReadPaReferralParticipant4",
      (db, command) =>
      {
        db.SetString(command, "preNumber", export.PaReferral.Number);
        db.SetString(command, "pafType", export.PaReferral.Type1);
        db.SetDateTime(
          command, "pafTstamp",
          export.PaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Children.IsFull)
        {
          return false;
        }

        entities.ExistingPaReferralParticipant.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 1);
        entities.ExistingPaReferralParticipant.Relationship =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferralParticipant.Sex =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralParticipant.Dob =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaReferralParticipant.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPaReferralParticipant.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPaReferralParticipant.Mi =
          db.GetNullableString(reader, 7);
        entities.ExistingPaReferralParticipant.Ssn =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingPaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 10);
        entities.ExistingPaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 11);
        entities.ExistingPaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferralParticipant.PreNumber =
          db.GetString(reader, 13);
        entities.ExistingPaReferralParticipant.PafType =
          db.GetString(reader, 14);
        entities.ExistingPaReferralParticipant.PafTstamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPaReferralParticipant.Role =
          db.GetNullableString(reader, 16);
        entities.ExistingPaReferralParticipant.Populated = true;

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
    /// <summary>
    /// A value of FromRefm.
    /// </summary>
    [JsonPropertyName("fromRefm")]
    public Standard FromRefm
    {
      get => fromRefm ??= new();
      set => fromRefm = value;
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

    private Standard fromRefm;
    private PaReferral paReferral;
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
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of CaseExists.
    /// </summary>
    [JsonPropertyName("caseExists")]
    public Common CaseExists
    {
      get => caseExists ??= new();
      set => caseExists = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public PaReferralParticipant Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public PaReferralParticipant Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of ApHome.
    /// </summary>
    [JsonPropertyName("apHome")]
    public PaParticipantAddress ApHome
    {
      get => apHome ??= new();
      set => apHome = value;
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
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public PaReferralParticipant Other
    {
      get => other ??= new();
      set => other = value;
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

    private Common found;
    private Common caseExists;
    private Common morePaReferrals;
    private PaReferral paReferral;
    private PaReferralParticipant ar;
    private WorkArea arName;
    private PaReferralParticipant ap;
    private WorkArea apName;
    private PaParticipantAddress apHome;
    private PaParticipantAddress apMail;
    private PaReferralParticipant other;
    private WorkArea otherName;
    private Array<ToRegiGroup> toRegi;
    private Array<ChildrenGroup> children;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public PaReferral Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea current;
    private PaReferral program;
    private Common found;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CheckThis.
    /// </summary>
    [JsonPropertyName("checkThis")]
    public PaReferral CheckThis
    {
      get => checkThis ??= new();
      set => checkThis = value;
    }

    /// <summary>
    /// A value of ExistingPaReferralAssignment.
    /// </summary>
    [JsonPropertyName("existingPaReferralAssignment")]
    public PaReferralAssignment ExistingPaReferralAssignment
    {
      get => existingPaReferralAssignment ??= new();
      set => existingPaReferralAssignment = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

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
    /// A value of ExistingPaParticipantAddress.
    /// </summary>
    [JsonPropertyName("existingPaParticipantAddress")]
    public PaParticipantAddress ExistingPaParticipantAddress
    {
      get => existingPaParticipantAddress ??= new();
      set => existingPaParticipantAddress = value;
    }

    private PaReferral checkThis;
    private PaReferralAssignment existingPaReferralAssignment;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private PaReferral existingPaReferral;
    private PaReferralParticipant existingPaReferralParticipant;
    private PaParticipantAddress existingPaParticipantAddress;
  }
#endregion
}
