// Program: SI_APDS_AP_DETAILS, ID: 371755671, model: 746.
// Short name: SWEAPDSP
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
/// A program: SI_APDS_AP_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiApdsApDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_APDS_AP_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiApdsApDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiApdsApDetails.
  /// </summary>
  public SiApdsApDetails(IContext context, Import import, Export export):
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
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03-30-95  Helen Sharland - MTW	Initial Development
    // 02-25-95  Paul Elie - MTW	Retrofit Security etc.
    // 06/27-96  G. Lofton		Changed ssn to numeric fields.
    // 11/01/96  G. Lofton		Add new security.
    // 12/24/96  Raju			Event insertion
    // 06/17/97  M. D. Wheaton         Deleted Datenum
    // 12/08/97  J. Rookard            Modified call to OE Raise event cab
    //                                 
    // to include a returned export
    // view.
    //                                 
    // In support of removal of control
    //                                 
    // table usage from Infrastructure
    //                                 
    // strategy.
    // 09/28/98  C Deghand             Added SET statements to
    //                                 
    // make prompts spaces on a
    // display.
    // 11/2/98    C Deghand            Added code and modified ESCAPE's to
    //                                 
    // make sure protected fields stay
    //                                 
    // protected.
    // ------------------------------------------------------------
    // 02/05/99 W.Campbell             The logic for update of
    //                                 
    // ADABAS was rearranged so that
    //                                 
    // it would occur after all other
    //                                 
    // DB/2 updates in case a
    //                                 
    // ROLLBACK was needed for DB/2
    //                                 
    // since ADABAS does not have
    //                                 
    // rollback capability.
    // ------------------------------------------------------------
    // 02/08/99 W.Campbell             Added logic to USE
    //                                 
    // EAB_ROLLBACK_CICS to
    //                                 
    // help ensure correct rollback
    //                                 
    // of DB/2 updates.
    // ------------------------------------------------------------
    // 02/11/99 W.Campbell             IF stmt copied and
    //                                 
    // disabled just to keep it in
    //                                 
    // case it needs to be re-enabled
    //                                 
    // again.  The original IF stmt
    //                                 
    // was modified so that it will be
    //                                 
    // true when a SSN is changed
    //                                 
    // from some non-blank (zero)
    //                                 
    // value to a blank(zero) value.
    //                                 
    // This is in the logic which
    //                                 
    // creates a row on the
    // infrastructure
    //                                 
    // table(to log an event).
    // ---------------------------------------------
    // 02/11/99 W.Campbell             IF stmt inserted to
    //                                 
    // log 'Unknown' when the SSN
    //                                 
    // is changed from non-blanks
    //                                 
    // (zeros) to blanks (zeros).
    //                                 
    // This is in the logic which
    //                                 
    // creates a row on the
    // infrastructure
    //                                 
    // table(to log an event).
    // ---------------------------------------------
    // 05/03/99 W.Campbell             Added code to send
    //                                 
    // selection needed msg to COMP.
    //                                 
    // BE SURE TO MATCH
    //                                 
    // next_tran_info ON THE
    //                                 
    // DIALOG FLOW.
    // -----------------------------------------------
    // 07/03/99 M.Lachowicz            Added validation for sex.
    // -----------------------------------------------
    // 10/13/99 W.Campbell             Changed the attribute view
    //                                 
    // being tested in an IF statement
    //                                 
    // so that it is testing the
    //                                 
    // other_phone_type instead of the
    //                                 
    // other_phone_type_prompt.
    //                                 
    // No validation was actually being
    //                                 
    // performed on the
    // other_phone_type.
    //                                 
    // This change fixed that problem
    // on
    //                                 
    // PR# H00077075.
    // --------------------------------------------
    // 03/30/00 W.Campbell             Modified an ESCAPE
    //                                 
    // statement after check for bad
    //                                 
    // return from SECURITY so that
    //                                 
    // it completely leaves the
    //                                 
    // Pstep.  Work done
    //                                 
    // under WR#000162 for
    //                                 
    // Family Violence.
    // ---------------------------------------------
    // 03/30/00 W.Campbell             Changed view matching
    //                                 
    // for the USE of
    //                                 
    // SC_CAB_TEST_SECURITY.
    //                                 
    // Changed view matching for the
    //                                 
    // cab's inport case to the Pstep's
    //                                 
    // export_next case.  It previously
    //                                 
    // was to the Pstep's inport_next
    // case.
    //                                 
    // Work done on WR#000162 for
    //                                 
    // PRWORA Family Violence
    // Indicator.
    // ---------------------------------------------
    // 09/05/00 W.Campbell             Inserted a SET
    //                                 
    // stmt to populate the
    //                                 
    // export_hidden_next_tran
    //                                 
    // field with the AP person number.
    //                                 
    // This is to support the
    //                                 
    // NEXTTRAN feature.
    //                                 
    // Work done on WR#00193-B.
    // ---------------------------------------------
    // 09/05/00 W.Campbell             Inserted an IF
    //                                 
    // and escape stmts to
    //                                 
    // bypass the following
    //                                 
    // logic if this an Infrastructure
    //                                 
    // ID was not
    //                                 
    // passed via NEXTTRAN.
    //                                 
    // Work done on WR#00193-B.
    // ---------------------------------------------
    // 11/22/00 M.Lachowicz            Changed header line.
    //                                 
    // Work done on WR#00298..
    // ---------------------------------------------
    // 02/22/01 M.Lachowicz            Sex code is mandatory.
    //                                 
    // Work done on PR 113332.
    // ---------------------------------------------
    // 03/09/01 M.Lachowicz            Made the following changes
    //                                 
    // 1. Allow to update even case is
    // closed.
    //                                 
    // 2. Address should be most
    // recently
    //                                     
    // verified.
    //                                 
    // Work done on WR 241.
    // ---------------------------------------------
    // ----------------------------------------------------------------------------------
    // 08/27/2001    Vithal Madhira        PR# 121249, 124583, 124584
    // Fixed the code for family violence indicator. The screen is not 
    // displaying data even  if the family violence indicator is not on the AP.
    // It must display data if the family violence indicator is not on AP.
    // Changed code in SWE01082(SC_CAB_TEST_SECURITY)  and SWE00301(
    // SC_SECURITY_VALID_AUTH_FOR_FV) CABs and APDS PSTEP.
    // -------------------------------------------------------------------------------------
    // 11/13/2001      Vithal Madhira      WR# 010339
    // Added new attribute 'Birthplace_Country' and repainted the screen and 
    // changed the code.
    // 08/26/2002     Vithal Madhira        PR# 145609
    // Replaced the poorly worded text  for "APNOTDEAD"
    // ---------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------
    // 10/29/07    LSS    PR# 180608 / CQ406
    // Added verify statement to error out if ssn contains a non-numeric 
    // character.
    // -----------------------------------------------------------------------------
    // 06/05/2009  DDupree     Added check when updating ssn against the invalid
    // ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    // 06/13/11  RMathews  CQ21791 Added edit for start and end dates when 
    // reading for MO and FA roles.
    // 02/11/2016  DDupree     Added email address and text message ind. Part of
    // CQ48843.
    // __________________________________________________________________________________
    // 03/08/2018  JHarden  CQ61455  Add a flow to EMAL from APDS and allow PF9 
    // to flow back to APDS.
    // 09/13/2018   JHarden  CQ64095  Add Tribal Fields to APDS/ARDS/CHDS
    // 10/29/2018  GVandy  CQ64956  Validate that Kansas driver's license 
    // numbers are in the format K99999999 where 9 represents any numeric digit.
    // 03/13/2020  JHarden  CQ65304  End date CP address when date of death is 
    // entered.
    // 08/23/2019  JHarden  CQ66290  Add an indicator for threats made by CP or 
    // NCP on staff.
    // 12/23/2020  GVandy  CQ68785  Add customer service code.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    export.ApCaseRole.Assign(import.ApCaseRole);
    export.CsePersonLicense.Assign(import.CsePersonLicense);
    export.AltSsn.Text13 = import.AltSsn.Text13;
    MoveCsePerson2(import.ApCsePerson, export.ApCsePerson);
    MoveCsePersonsWorkSet1(import.ApCsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
    export.ApSsnWorkArea.Assign(import.ApSsnWorkArea);
    export.ArCsePersonsWorkSet.Assign(import.Ar);
    export.Bankruptcy.Flag = import.Bankruptcy.Flag;
    export.FedBenefits.Flag = import.FedBenefits.Flag;
    export.Incarceration.Flag = import.Incarceration.Flag;
    export.Military.Flag = import.Military.Flag;
    export.MultipleCases.Flag = import.MultipleCases.Flag;
    export.OtherChildren.Flag = import.OtherChildren.Flag;
    export.OtherCsOrders.Flag = import.OtherCsOrders.Flag;
    export.Uci.Flag = import.Uci.Flag;
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.CitizenStatusPrompt.SelectChar =
      import.CitizenStatusPrompt.SelectChar;
    export.DlStatePrompt.SelectChar = import.DlStatePrompt.SelectChar;
    export.EyesPrompt.SelectChar = import.EyesPrompt.SelectChar;
    export.HairPrompt.SelectChar = import.HairPrompt.SelectChar;
    export.MaritalStatusPrompt.SelectChar =
      import.MaritalStatusPrompt.SelectChar;
    export.PobStPrompt.SelectChar = import.PobStPrompt.SelectChar;
    export.PobFcPrompt.SelectChar = import.PobFcPrompt.SelectChar;
    export.RacePrompt.SelectChar = import.RacePrompt.SelectChar;
    MoveCsePersonsWorkSet2(import.ApSelected, export.ApSelected);
    export.PhoneTypePrompt.SelectChar = import.PhoneTypePrompt.SelectChar;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);
    export.ApActive.Flag = import.ApActive.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.CsePersonEmailAddress.EmailAddress =
      import.CsePersonEmailAddress.EmailAddress;
    export.HiddenCsePerson.Assign(import.HiddenCsePerson);
    export.CustomerServicePrompt.SelectChar =
      import.CustomerServicePrompt.SelectChar;

    // CQ64095
    export.TribalFlag.Flag = import.TribalFlag.Flag;
    export.TribalPrompt.SelectChar = import.TribalPrompt.SelectChar;

    // 11/22/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/22/00 M.L End
    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPrevCase.Number = import.HiddenPrevCase.Number;
    export.HiddenPrevCsePersonsWorkSet.Number =
      import.HiddenPrevCsePersonsWorkSet.Number;
    export.HiddenAe.Flag = import.HiddenAe.Flag;

    // 07/30/99 Start
    export.HiddenApSex.Sex = import.HiddenApSex.Sex;

    // 07/30/99 End
    // ---------------------------------------------
    // Begin Code - Raju 12/24/1996 1030 hrs CST
    // ---------------------------------------------
    MoveCsePerson3(import.LastReadHiddenCsePerson,
      export.LastReadHiddenCsePerson);
    export.LastReadHiddenCsePersonLicense.Number =
      import.LastReadHiddenCsePersonLicense.Number;
    export.LastReadHiddenCsePersonsWorkSet.Ssn =
      import.LastReadHiddenCsePersonsWorkSet.Ssn;

    // ---------------------------------------------
    // End   Code - Raju 12/24/1996 1030 hrs CST
    // ---------------------------------------------
    // ---------------------------------------------
    //        	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumberAp =
        export.ApCsePersonsWorkSet.Number;

      // ---------------------------------------------
      // 09/05/00 W.Campbell - Inserted following SET
      // stmt to populate the export_hidden_next_tran
      // field with the AP person number.
      // This is to support the NEXTTRAN feature.
      // Work done on WR#00193-B.
      // ---------------------------------------------
      export.HiddenNextTranInfo.CsePersonNumber =
        export.ApCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);

        // ---------------------------------------------
        // Start of Code (Raju 01/20/97:1035 hrs CST)
        // ---------------------------------------------
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          // ---------------------------------------------
          // 09/05/00 W.Campbell - Inserted following IF
          // and escape stmts to bypass the following
          // logic if this an Infrastructure ID was not
          // passed via NEXTTRAN.
          // Work done on WR#00193-B.
          // ---------------------------------------------
          if (export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault() ==
            0)
          {
            goto Test1;
          }

          local.LastTran.SystemGeneratedIdentifier =
            export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
          UseOeCabReadInfrastructure();
          export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
          export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
          export.ApCsePersonsWorkSet.Number =
            local.LastTran.CsePersonNumber ?? Spaces(10);
        }

Test1:

        // ---------------------------------------------
        // End  of Code
        // ---------------------------------------------
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "UPDATE"))
    {
      // 03/09/01 M.L Start
      // 03/09/01 M.L End
    }

    // ------------------------------------------------------------
    // Protect fields is required
    // ------------------------------------------------------------
    if (Equal(export.Next.Number, export.HiddenPrevCase.Number))
    {
      if (AsChar(export.HiddenAe.Flag) == 'O')
      {
        // ---------------------------------------------
        // This CSE Person is owned by the AE system and may not be changed by 
        // the CSE system.
        // ---------------------------------------------
        var field1 = GetField(export.ApCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ApCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ApCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.ApCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ApCsePersonsWorkSet, "sex");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ApSsnWorkArea, "ssnNumPart1");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ApSsnWorkArea, "ssnNumPart2");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.ApSsnWorkArea, "ssnNumPart3");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

        field11.Color = "cyan";
        field11.Protected = true;
      }
    }

    // ---------------------------------------------
    // When the control is returned from a LIST screen
    // Populate the appropriate prompt fields.
    // ---------------------------------------------
    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(import.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";

        var field = GetField(export.ApPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(import.ApSelected.Number))
      {
        MoveCsePersonsWorkSet2(import.ApSelected, export.ApCsePersonsWorkSet);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(export.PhoneTypePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.OtherPhoneType = import.Selected.Cdvalue;
        }

        export.PhoneTypePrompt.SelectChar = "";

        var field = GetField(export.PhoneTypePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.PobStPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.BirthPlaceState = import.Selected.Cdvalue;
        }

        export.PobStPrompt.SelectChar = "";

        var field = GetField(export.PobStPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.PobFcPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.BirthplaceCountry = import.Selected.Cdvalue;
          export.WorkForeignCountryDesc.Text40 = import.Selected.Description;
        }

        export.PobFcPrompt.SelectChar = "";

        var field = GetField(export.PobFcPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.CitizenStatusPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.IllegalAlienIndicator = import.Selected.Cdvalue;
        }

        export.CitizenStatusPrompt.SelectChar = "";

        var field = GetField(export.CitizenStatusPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.RacePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.Race = import.Selected.Cdvalue;
        }

        export.RacePrompt.SelectChar = "";

        var field = GetField(export.RacePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.EyesPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.EyeColor = import.Selected.Cdvalue;
        }

        export.EyesPrompt.SelectChar = "";

        var field = GetField(export.EyesPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.HairPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.HairColor = import.Selected.Cdvalue;
        }

        export.HairPrompt.SelectChar = "";

        var field = GetField(export.HairPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.DlStatePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.CsePersonLicense.IssuingState = import.Selected.Cdvalue;
        }

        export.DlStatePrompt.SelectChar = "";

        var field = GetField(export.DlStatePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.MaritalStatusPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.CurrentMaritalStatus = import.Selected.Cdvalue;
        }

        export.MaritalStatusPrompt.SelectChar = "";

        var field = GetField(export.MaritalStatusPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      // CQ64095 add tribal
      if (AsChar(export.TribalPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.TribalCode = import.Selected.Cdvalue;
        }

        export.TribalPrompt.SelectChar = "";

        var field = GetField(export.TribalPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(export.ApCsePerson.TribalCode))
      {
        export.TribalFlag.Flag = "Y";
      }
      else
      {
        export.TribalFlag.Flag = "N";
      }

      // 12/23/2020  GVandy  CQ68785  Add customer service code.
      if (AsChar(export.CustomerServicePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ApCsePerson.CustomerServiceCode = import.Selected.Cdvalue;
        }

        export.CustomerServicePrompt.SelectChar = "";

        var field = GetField(export.CustomerServicePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      // PR160844. Changes to highlight SSN display on the screen
      local.SsnPart.Count = 0;

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart1, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart1, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart1, 3, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart2, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart2, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart3, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart3, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart3, 3, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ApSsnWorkArea.SsnTextPart3, 4, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (local.SsnPart.Count > 0)
      {
        if (local.SsnPart.Count > 8)
        {
          goto Test2;
        }

        if (local.SsnPart.Count > 0 && local.SsnPart.Count < 9)
        {
          var field1 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "LE0000_SSN_CONTAINS_NONNUM";

          return;
        }
      }

Test2:

      // PR160844. End changes to highlight SSN display on the screen
      if (export.ApSsnWorkArea.SsnNumPart1 == 0 && export
        .ApSsnWorkArea.SsnNumPart2 == 0 && export.ApSsnWorkArea.SsnNumPart3 == 0
        )
      {
        export.ApCsePersonsWorkSet.Ssn = "";
      }
      else
      {
        MoveSsnWorkArea2(export.ApSsnWorkArea, local.SsnWorkArea);
        local.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();
        export.ApCsePersonsWorkSet.Ssn = export.ApSsnWorkArea.SsnText9;
      }
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "ALTS") || Equal(global.Command, "EMAL"))
    {
    }
    else
    {
      // ---------------------------------------------
      // 03/30/00 W.Campbell - Changed view matching
      // for the USE of SC_CAB_TEST_SECURITY.
      // Changed view matching for the cab's inport case
      // to the Pstep's export_next case.  It previously
      // was to the Pstep's inport_next case.  Work done on
      // WR#000162 for PRWORA Family Violence Indicator.
      // ---------------------------------------------
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ---------------------------------------------
        // 03/30/00 W.Campbell - Modified the following
        // ESCAPE so that it completely leaves the Pstep.
        // Work done under WR#000162 for Family
        // Violence.
        // ---------------------------------------------
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "EMAL":
        ExitState = "ECO_XFR_TO_EMAL";

        return;
      case "ALTS":
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // ---------------------------------------------
        // This command allows the user to link to a
        // selection list and retrieve the appropriate
        // value, not losing any of the data already
        // entered.
        // ---------------------------------------------
        switch(AsChar(import.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field = GetField(export.ApPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PhoneTypePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "OTHER PHONE TYPE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PhoneTypePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PobStPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PobStPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PobFcPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "FIPS COUNTRY CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PobFcPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.CitizenStatusPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "CITIZEN STATUS";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.CitizenStatusPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.RacePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "RACE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.RacePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.EyesPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "EYE COLOR";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.EyesPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.HairPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "HAIR COLOR";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.HairPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.DlStatePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.DlStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.MaritalStatusPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "MARITAL STATUS";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.MaritalStatusPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        // CQ64095
        switch(AsChar(import.TribalPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "TRIBAL NAME";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.TribalPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        if (!IsEmpty(export.ApCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        switch(AsChar(import.CustomerServicePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "CUSTOMER SERVICE INQUIRIES";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.CustomerServicePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ApPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PobStPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PobStPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PobFcPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PobFcPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.CitizenStatusPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.CitizenStatusPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.RacePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.RacePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.EyesPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.EyesPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.HairPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.HairPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.DlStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.DlStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.MaritalStatusPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.MaritalStatusPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.CustomerServicePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.CustomerServicePrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        break;
      case "UPDATE":
        if (!Equal(import.ApCsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ---------------------------------------------
        // All Non-Database validation should be done here.
        // Do all validation as above.
        // Common action blocks (CABs) will be provided
        // for numeric validations on fields such as
        // date, amounts etc.
        // ---------------------------------------------
        UseSiCheckName();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.ApCsePersonsWorkSet, "firstName");

          field1.Error = true;

          var field2 = GetField(export.ApCsePersonsWorkSet, "lastName");

          field2.Error = true;

          var field3 = GetField(export.ApCsePersonsWorkSet, "middleInitial");

          field3.Error = true;

          ExitState = "SI0001_INVALID_NAME";
        }

        // PR# 180608 / CQ406   10/29/07   LSS
        // Moved the SET statements from the end of the validations
        // to place it with the SSN validation.
        local.SsnConcat.Text8 = export.ApSsnWorkArea.SsnTextPart2 + export
          .ApSsnWorkArea.SsnTextPart3;
        export.ApCsePersonsWorkSet.Ssn = export.ApSsnWorkArea.SsnTextPart1 + local
          .SsnConcat.Text8;

        // PR# 180608 / CQ406   10/29/07   LSS   Added verify / ERROR statements
        // for SSN.
        if (Verify(export.ApCsePersonsWorkSet.Ssn, "0123456789") != 0 && !
          IsEmpty(export.ApCsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "LE0000_SSN_CONTAINS_NONNUM";
        }

        if (!IsEmpty(export.ApCsePersonsWorkSet.Ssn) && !
          Equal(export.ApCsePersonsWorkSet.Ssn,
          export.LastReadHiddenCsePersonsWorkSet.Ssn))
        {
          // added this check as part of cq7189.
          local.Convert.SsnNum9 =
            (int)StringToNumber(export.ApCsePersonsWorkSet.Ssn);

          if (ReadInvalidSsn())
          {
            var field1 = GetField(export.ApCsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

            field2.Error = true;

            var field3 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

            field3.Error = true;

            var field4 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

            field4.Error = true;

            ExitState = "INVALID_SSN";

            break;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }
        }

        // --------------------------------------------
        // Validate the Date of Birth
        // --------------------------------------------
        if (Lt(Now().Date, import.ApCsePersonsWorkSet.Dob))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_DATE_OF_BIRTH";
          }

          var field = GetField(export.ApCsePersonsWorkSet, "dob");

          field.Error = true;
        }

        // CQ64095
        if (!IsEmpty(import.ApCsePerson.TribalCode))
        {
          local.Code.CodeName = "TRIBAL NAME";
          local.CodeValue.Cdvalue = import.ApCsePerson.TribalCode ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "tribalCode");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_TRIBAL_NAME";
            }
          }
        }

        if (!IsEmpty(export.ApCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        // --------------------------------------------
        // Validate Other Phone Type
        // --------------------------------------------
        // --------------------------------------------
        // 10/13/99 W.Campbell - Changed the attribute view
        // being tested in the following IF statement so that
        // it is testing the other_phone_type instead of the
        // other_phone_type_prompt.  No validation was
        // actually being performed on the other_phone_type.
        // This change fixed that problem.  This change fixed
        // that problem reported on PR# H00077075.
        // --------------------------------------------
        if (!IsEmpty(import.ApCsePerson.OtherPhoneType))
        {
          local.Code.CodeName = "OTHER PHONE TYPE";
          local.CodeValue.Cdvalue = import.ApCsePerson.OtherPhoneType ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the state table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "otherPhoneType");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_OTHER_PHONE_TYPE";
            }
          }
        }

        // --------------------------------------------
        // Validate the POB State
        // --------------------------------------------
        if (!IsEmpty(import.ApCsePerson.BirthPlaceState))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.ApCsePerson.BirthPlaceState ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the state table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "birthPlaceState");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }

        // --------------------------------------------
        // Validate the POB Foreign Country.
        // --------------------------------------------
        if (!IsEmpty(import.ApCsePerson.BirthplaceCountry))
        {
          local.Code.CodeName = "FIPS COUNTRY CODE";
          local.CodeValue.Cdvalue = import.ApCsePerson.BirthplaceCountry ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the 'FIPS COUNTRY CODE' table
          // --------------------------------------------
          UseCabValidateCodeValue2();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "birthplaceCountry");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_FORGN_COUNTRY";
            }
          }
          else
          {
            export.WorkForeignCountryDesc.Text40 =
              local.DisplayForeignCountry.Description;
          }
        }

        // --------------------------------------------
        // Per WR# 010339, User can not enter both 'State'  and  'Foreign 
        // Country'.
        // --------------------------------------------
        if (!IsEmpty(export.ApCsePerson.BirthPlaceState) && !
          IsEmpty(export.ApCsePerson.BirthplaceCountry))
        {
          var field1 = GetField(export.ApCsePerson, "birthPlaceState");

          field1.Error = true;

          var field2 = GetField(export.ApCsePerson, "birthplaceCountry");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_STATE_COUNTRY_ERROR";
          }
        }

        // --------------------------------------------
        // Validate the Date of Death
        // --------------------------------------------
        if (Lt(Now().Date, import.ApCsePerson.DateOfDeath))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_DATE_OF_DEATH";
          }

          var field = GetField(export.ApCsePerson, "dateOfDeath");

          field.Error = true;
        }

        // 03/09/01 M.L Start
        if (Lt(export.ApCsePerson.DateOfDeath, export.ApCsePersonsWorkSet.Dob) &&
          Lt(local.Zero.Date, export.ApCsePerson.DateOfDeath))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "DOD_LESS_THAN_DOB";
          }

          var field1 = GetField(export.ApCsePersonsWorkSet, "dob");

          field1.Error = true;

          var field2 = GetField(export.ApCsePerson, "dateOfDeath");

          field2.Error = true;
        }

        // CQ65304
        if (Lt(local.Zero.Date, export.ApCsePerson.DateOfDeath) && !
          Lt(local.Zero.Date, export.HiddenCsePerson.DateOfDeath))
        {
          foreach(var item in ReadCsePersonAddress())
          {
            try
            {
              UpdateCsePersonAddress();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_ADDRESS_NF";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_ADDRESS_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        // 03/09/01 M.L End
        if (!IsEmpty(import.ApCsePerson.IllegalAlienIndicator))
        {
          // --------------------------------------------
          // Validate the Citizen Status
          // --------------------------------------------
          local.Code.CodeName = "CITIZEN STATUS";
          local.CodeValue.Cdvalue =
            import.ApCsePerson.IllegalAlienIndicator ?? Spaces(10);

          // ---------------------------------------------
          // Call CAB to validate against the CITIZEN STATUS table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "illegalAlienIndicator");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_CITIZEN_STATUS";
            }
          }
        }

        // --------------------------------------------
        // Validate the Sex
        // --------------------------------------------
        // 02/22/01 M.L Do not allow space for sex code.
        if (AsChar(import.ApCsePersonsWorkSet.Sex) != 'F' && AsChar
          (import.ApCsePersonsWorkSet.Sex) != 'M')
        {
          var field = GetField(export.ApCsePersonsWorkSet, "sex");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SEX";
          }
        }

        // 07/30/99  M.L.  Add extra validation for sex.
        // CQ21791  Added check for role start and end dates in following reads
        if (AsChar(import.ApCsePersonsWorkSet.Sex) != 'F' && AsChar
          (export.HiddenApSex.Sex) == 'F')
        {
          if (ReadCaseRole2())
          {
            var field = GetField(export.ApCsePersonsWorkSet, "sex");

            field.Error = true;

            ExitState = "SI0000_FEMALE_SEX_CHANGE_NOT";
          }
        }

        if (AsChar(import.ApCsePersonsWorkSet.Sex) != 'M' && AsChar
          (export.HiddenApSex.Sex) == 'M')
        {
          if (ReadCaseRole1())
          {
            var field = GetField(export.ApCsePersonsWorkSet, "sex");

            field.Error = true;

            ExitState = "SI0000_MALE_SEX_CHANGE_NOT_ALLOW";
          }
        }

        // 07/30/99  M.L.  End
        if (!IsEmpty(import.ApCsePerson.Race))
        {
          // --------------------------------------------
          // Validate the Race
          // --------------------------------------------
          local.Code.CodeName = "RACE";
          local.CodeValue.Cdvalue = import.ApCsePerson.Race ?? Spaces(10);

          // ---------------------------------------------
          // Call CAB to validate against the RACE table
          // ---------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "race");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_RACE";
            }
          }
        }

        // --------------------------------------------
        // Validate the Height
        // --------------------------------------------
        if (import.ApCsePerson.HeightIn.GetValueOrDefault() > 11)
        {
          var field = GetField(export.ApCsePerson, "heightIn");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_HEIGHT_INCHES";
          }
        }

        if (!IsEmpty(import.ApCsePerson.EyeColor))
        {
          // --------------------------------------------
          // Validate Eye color
          // --------------------------------------------
          local.Code.CodeName = "EYE COLOR";
          local.CodeValue.Cdvalue = import.ApCsePerson.EyeColor ?? Spaces(10);

          // ---------------------------------------------
          // Call CAB to validate against the EYES table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "eyeColor");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_EYE_COLOR";
            }
          }
        }

        if (!IsEmpty(import.ApCsePerson.HairColor))
        {
          // --------------------------------------------
          // Validate Hair color
          // --------------------------------------------
          local.Code.CodeName = "HAIR COLOR";
          local.CodeValue.Cdvalue = import.ApCsePerson.HairColor ?? Spaces(10);

          // ---------------------------------------------
          // Call CAB to validate against the HAIR table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "hairColor");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_HAIR_COLOR";
            }
          }
        }

        // --------------------------------------------
        // Validate Weight
        // --------------------------------------------
        if (import.ApCsePerson.Weight.GetValueOrDefault() < 0)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_WEIGHT";
          }

          var field = GetField(export.ApCsePerson, "weight");

          field.Error = true;
        }

        // 08/02/99 M.L Add validation for area code.
        if (export.ApCsePerson.HomePhoneAreaCode.GetValueOrDefault() == 0 && export
          .ApCsePerson.HomePhone.GetValueOrDefault() != 0)
        {
          var field = GetField(export.ApCsePerson, "homePhoneAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        if (export.ApCsePerson.WorkPhoneAreaCode.GetValueOrDefault() == 0 && export
          .ApCsePerson.WorkPhone.GetValueOrDefault() != 0)
        {
          var field = GetField(export.ApCsePerson, "workPhoneAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        if (export.ApCsePerson.OtherAreaCode.GetValueOrDefault() == 0 && export
          .ApCsePerson.OtherNumber.GetValueOrDefault() != 0)
        {
          var field = GetField(export.ApCsePerson, "otherAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        // 08/02/99 M.L End.
        if (!IsEmpty(import.CsePersonLicense.IssuingState))
        {
          // --------------------------------------------
          // Validate Driver's License State
          // --------------------------------------------
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.CsePersonLicense.IssuingState ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the state table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.CsePersonLicense, "issuingState");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }
        else if (!IsEmpty(import.CsePersonLicense.Number))
        {
          var field = GetField(export.CsePersonLicense, "issuingState");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        // 10/29/2018  GVandy  CQ64956  Validate that Kansas driver's license 
        // numbers are in the format K99999999 where 9 represents any numeric
        // digit.
        if (Equal(import.CsePersonLicense.IssuingState, "KS"))
        {
          if (IsEmpty(import.CsePersonLicense.Number))
          {
            goto Test3;
          }

          if (Length(TrimEnd(import.CsePersonLicense.Number)) != 9)
          {
            var field = GetField(export.CsePersonLicense, "number");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SI0000_KS_DL_FORMAT_ERROR";
            }
          }

          if (CharAt(import.CsePersonLicense.Number, 1) != 'K')
          {
            var field = GetField(export.CsePersonLicense, "number");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SI0000_KS_DL_FORMAT_ERROR";
            }
          }

          if (Verify(Substring(import.CsePersonLicense.Number, 25, 2, 8),
            "0123456789") != 0)
          {
            var field = GetField(export.CsePersonLicense, "number");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SI0000_KS_DL_FORMAT_ERROR";
            }
          }
        }

Test3:

        if (!IsEmpty(import.ApCsePerson.TextMessageIndicator))
        {
          if (AsChar(import.ApCsePerson.TextMessageIndicator) != 'Y' && AsChar
            (import.ApCsePerson.TextMessageIndicator) != 'N')
          {
            var field = GetField(export.ApCsePerson, "textMessageIndicator");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_TEXT_MESSAGE_IND";
            }
          }
        }

        if (IsEmpty(import.ApCsePerson.TextMessageIndicator))
        {
          if (!IsEmpty(import.HiddenCsePerson.TextMessageIndicator))
          {
            var field = GetField(export.ApCsePerson, "textMessageIndicator");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_TEXT_MESSAGE_IND";

              return;
            }
          }
        }

        // 12/23/2020  GVandy  CQ68785  Add customer service code validation.
        if (!IsEmpty(import.ApCsePerson.CustomerServiceCode))
        {
          // --------------------------------------------
          // Validate Customer Service Code
          // --------------------------------------------
          local.Code.CodeName = "CUSTOMER SERVICE INQUIRIES";
          local.CodeValue.Cdvalue = import.ApCsePerson.CustomerServiceCode ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the customer service code table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "customerServiceCode");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SI0000_INVALID_CUST_SERV_CD";
            }
          }
        }

        switch(AsChar(import.ApCsePerson.CustomerServiceCode))
        {
          case 'E':
            if (IsEmpty(export.CsePersonEmailAddress.EmailAddress))
            {
              var field = GetField(export.ApCsePerson, "customerServiceCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "SI0000_MUST_HAVE_EMAIL_ADDRESS";
              }
            }

            break;
          case 'T':
            if (AsChar(export.ApCsePerson.OtherPhoneType) != 'C' || export
              .ApCsePerson.OtherNumber.GetValueOrDefault() == 0)
            {
              var field = GetField(export.ApCsePerson, "customerServiceCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsEmpty(import.HiddenCsePerson.CustomerServiceCode))
                {
                  ExitState = "SI0000_MUST_HAVE_CELL_PHONE";
                }
                else
                {
                  ExitState = "SI0000_CHANGE_CUSTOMER_SERVICE";
                }
              }
            }

            break;
          default:
            break;
        }

        if (!IsEmpty(import.ApCsePerson.CurrentMaritalStatus))
        {
          // --------------------------------------------
          // Validate Marital Status
          // --------------------------------------------
          local.Code.CodeName = "MARITAL STATUS";
          local.CodeValue.Cdvalue = import.ApCsePerson.CurrentMaritalStatus ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the state table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ApCsePerson, "currentMaritalStatus");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_MARITAL_STATUS";
            }
          }
        }

        // CQ 66290 validate threat on staff
        if (!IsEmpty(import.ApCsePerson.ThreatOnStaff))
        {
          if (AsChar(import.ApCsePerson.ThreatOnStaff) != 'Y' && AsChar
            (import.ApCsePerson.ThreatOnStaff) != 'N')
          {
            var field = GetField(export.ApCsePerson, "threatOnStaff");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_THREAT_IND";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        MoveCsePerson2(import.ApCsePerson, local.ApCsePerson);
        local.ApCsePerson.Number = import.ApCsePersonsWorkSet.Number;

        // ----------------------------------------------------------------------
        // 10/29/07     LSS     PR#180608 / CQ406
        // Commented out the SET statements and moved them to the SSN 
        // validation.
        // ----------------------------------------------------------------------
        local.ApCsePerson.TaxId = export.ApCsePersonsWorkSet.Ssn;
        UseSiUpdateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------------------------
          // 02/08/99 W.Campbell - Added logic to USE
          // EAB_ROLLBACK_CICS to help ensure
          // correct rollback of DB/2 updates.
          // ------------------------------------------------------------
          UseEabRollbackCics();

          break;
        }

        // ---------------------------------------------
        // Update details of the driver's license
        // ---------------------------------------------
        if (import.CsePersonLicense.Identifier == 0)
        {
          export.CsePersonLicense.Type1 = "D";
          export.CsePersonLicense.ExpirationDt =
            UseCabSetMaximumDiscontinueDate();
          UseSiCreateCsePersonLicense();
        }
        else
        {
          UseSiUpdateCsePersonLicense();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------------------------
          // 02/08/99 W.Campbell - Added logic to USE
          // EAB_ROLLBACK_CICS to help ensure
          // correct rollback of DB/2 updates.
          // ------------------------------------------------------------
          UseEabRollbackCics();

          break;
        }

        // ---------------------------------------------
        // Update details of the case role
        // --------------------------------------------
        UseSiUpdateCaseRole();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------------------------
          // 02/05/99 W.Campbell - The logic for
          // update of ADABAS was rearranged so that
          // it would occur after all other DB/2 updates
          // in case a ROLLBACK was needed for DB/2
          // since ADABAS does not have
          // rollback capability.
          // ------------------------------------------------------------
          // ------------------------------------------------------------
          // Update adabase
          // ------------------------------------------------------------
          if (AsChar(import.HiddenAe.Flag) != 'O')
          {
            UseCabUpdateAdabasPerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              // ------------------------------------------------------------
              // 02/08/99 W.Campbell - Added logic to USE
              // EAB_ROLLBACK_CICS to help ensure
              // correct rollback of DB/2 updates.
              // ------------------------------------------------------------
              UseEabRollbackCics();

              break;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          // 07/30/99 Start
          export.HiddenApSex.Sex = export.ApCsePersonsWorkSet.Sex;

          // 07/30/99 End
          export.HiddenCsePerson.CustomerServiceCode =
            export.ApCsePerson.CustomerServiceCode ?? "";
        }
        else
        {
          // ------------------------------------------------------------
          // 02/08/99 W.Campbell - Added logic to USE
          // EAB_ROLLBACK_CICS to help ensure
          // correct rollback of DB/2 updates.
          // ------------------------------------------------------------
          UseEabRollbackCics();
        }

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        break;
      case "SIGNOFF":
        // --------------------------------------------
        // Sign the user off the Kessep system
        // --------------------------------------------
        UseScCabSignoff();

        return;
      case "DISPLAY":
        // ---------------------------------------------
        // Begin Code - Raju 12/24/1996 1030 hrs CST
        // ---------------------------------------------
        export.LastReadHiddenCsePerson.DateOfDeath = local.Zero.Date;
        export.LastReadHiddenCsePersonLicense.Number = "";
        export.LastReadHiddenCsePersonsWorkSet.Ssn = "";

        // CQ66290
        export.LastReadHiddenCsePerson.ThreatOnStaff = "";

        // ---------------------------------------------
        // End   Code - Raju 12/24/1996 1030 hrs CST
        // ---------------------------------------------
        export.ApSsnWorkArea.SsnTextPart1 = "";
        export.ApSsnWorkArea.SsnTextPart2 = "";
        export.ApSsnWorkArea.SsnTextPart3 = "";
        export.ApPrompt.SelectChar = "";
        export.PhoneTypePrompt.SelectChar = "";
        export.PobStPrompt.SelectChar = "";
        export.PobFcPrompt.SelectChar = "";
        export.CitizenStatusPrompt.SelectChar = "";
        export.RacePrompt.SelectChar = "";
        export.EyesPrompt.SelectChar = "";
        export.HairPrompt.SelectChar = "";
        export.DlStatePrompt.SelectChar = "";
        export.MaritalStatusPrompt.SelectChar = "";
        export.AltSsn.Text13 = "";
        export.CsePersonEmailAddress.EmailAddress =
          Spaces(CsePersonEmailAddress.EmailAddress_MaxLength);
        export.CustomerServicePrompt.SelectChar = "";

        if (!IsEmpty(export.Next.Number))
        {
          UseCabZeroFillNumber();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;

              break;
            }
          }
        }
        else
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "CASE_NUMBER_REQUIRED";

          break;
        }

        if (IsEmpty(export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
        }

        if (!Equal(export.Next.Number, export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
          export.ApCsePersonsWorkSet.Number = "";
        }

        if (!IsEmpty(export.ApCsePersonsWorkSet.Number))
        {
          local.TextWorkArea.Text10 = export.ApCsePersonsWorkSet.Number;
          UseEabPadLeftWithZeros();
          export.ApCsePersonsWorkSet.Number = local.TextWorkArea.Text10;
        }

        // ---------------------------------------------
        // Call the action block that reads the data
        // required for this screen.
        // --------------------------------------------
        UseSiReadCaseHeaderInformation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!IsEmpty(export.ApCsePersonsWorkSet.Ssn))
        {
          local.SsnWorkArea.SsnText9 = export.ApCsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ApSsnWorkArea.SsnNumPart1 = 0;
            export.ApSsnWorkArea.SsnNumPart2 = 0;
            export.ApSsnWorkArea.SsnNumPart3 = 0;

            var field1 = GetField(export.ApSsnWorkArea, "ssnNumPart3");

            field1.Error = true;

            var field2 = GetField(export.ApSsnWorkArea, "ssnNumPart2");

            field2.Error = true;

            var field3 = GetField(export.ApSsnWorkArea, "ssnNumPart1");

            field3.Error = true;
          }
        }
        else
        {
          export.ApSsnWorkArea.SsnText9 = "";
          export.ApSsnWorkArea.SsnNumPart1 = 0;
          export.ApSsnWorkArea.SsnNumPart2 = 0;
          export.ApSsnWorkArea.SsnNumPart3 = 0;
        }

        // PR160844.  Load the text portion of ssn parts so that the ssn 
        // displays on the screen.
        if (IsEmpty(export.ApSsnWorkArea.SsnTextPart1) && IsEmpty
          (export.ApSsnWorkArea.SsnTextPart2) && IsEmpty
          (export.ApSsnWorkArea.SsnTextPart3))
        {
          MoveSsnWorkArea2(export.ApSsnWorkArea, local.SsnWorkArea);
          local.SsnWorkArea.ConvertOption = "2";
          UseCabSsnConvertNumToText();
          export.ApSsnWorkArea.SsnText9 = export.ApCsePersonsWorkSet.Ssn;

          if (export.ApSsnWorkArea.SsnNumPart1 == 0)
          {
          }
          else
          {
            export.ApSsnWorkArea.SsnTextPart1 =
              Substring(export.ApSsnWorkArea.SsnText9, 1, 3);
          }

          if (export.ApSsnWorkArea.SsnNumPart2 == 0)
          {
          }
          else
          {
            export.ApSsnWorkArea.SsnTextPart2 =
              Substring(export.ApSsnWorkArea.SsnText9, 4, 2);
          }

          if (export.ApSsnWorkArea.SsnNumPart3 == 0)
          {
          }
          else
          {
            export.ApSsnWorkArea.SsnTextPart3 =
              Substring(export.ApSsnWorkArea.SsnText9, 6, 4);
          }
        }

        // PR160844.  End the load of the text portion of ssn parts.
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ApSsnWorkArea.SsnNumPart1 = 0;
          export.ApSsnWorkArea.SsnNumPart2 = 0;
          export.ApSsnWorkArea.SsnNumPart3 = 0;

          var field1 = GetField(export.ApSsnWorkArea, "ssnNumPart3");

          field1.Error = true;

          var field2 = GetField(export.ApSsnWorkArea, "ssnNumPart2");

          field2.Error = true;

          var field3 = GetField(export.ApSsnWorkArea, "ssnNumPart1");

          field3.Error = true;

          var field4 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

          field4.Error = true;

          var field5 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

          field5.Error = true;

          var field6 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

          field6.Error = true;
        }

        UseSiReadCsePerson();

        if (AsChar(export.HiddenAe.Flag) == 'O')
        {
          // ---------------------------------------------
          // This CSE Person is owned by the AE /KSCares
          // and may not be changed by the CSE system.
          // ---------------------------------------------
          var field1 = GetField(export.ApCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ApCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ApCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ApCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.ApCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.ApCsePersonsWorkSet, "sex");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.ApSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.ApSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.ApSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

          field12.Color = "cyan";
          field12.Protected = true;
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

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseSiReadApCaseRoleDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // CQ64095
        if (!IsEmpty(export.ApCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        if (ReadCsePersonEmailAddress())
        {
          export.CsePersonEmailAddress.EmailAddress =
            entities.CsePersonEmailAddress.EmailAddress;
        }

        if (!IsEmpty(export.ApCsePerson.BirthplaceCountry))
        {
          local.Code.CodeName = "FIPS COUNTRY CODE";
          local.CodeValue.Cdvalue = export.ApCsePerson.BirthplaceCountry ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the 'FIPS COUNTRY CODE' table
          // --------------------------------------------
          UseCabValidateCodeValue3();
          export.WorkForeignCountryDesc.Text40 =
            local.DisplayForeignCountry.Description;
        }

        if (AsChar(export.HiddenAe.Flag) == 'O')
        {
          // ---------------------------------------------
          // This CSE Person is owned by the AE system and
          // may not be changed by the CSE system.
          // ---------------------------------------------
          var field1 = GetField(export.ApCsePersonsWorkSet, "dob");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ApCsePersonsWorkSet, "firstName");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ApCsePersonsWorkSet, "lastName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ApCsePersonsWorkSet, "middleInitial");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.ApCsePersonsWorkSet, "sex");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.ApSsnWorkArea, "ssnNumPart1");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.ApSsnWorkArea, "ssnNumPart2");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.ApSsnWorkArea, "ssnNumPart3");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

          field11.Color = "cyan";
          field11.Protected = true;
        }

        UseSiAltsBuildAliasAndSsn();

        if (AsChar(local.ApOccur.Flag) == 'Y')
        {
          export.AltSsn.Text13 = "Alt SSN/Alias";
        }

        // ---------------------------------------------
        // Begin Code - Raju 12/24/1996 1030 hrs CST
        // ---------------------------------------------
        export.LastReadHiddenCsePerson.DateOfDeath =
          export.ApCsePerson.DateOfDeath;
        export.LastReadHiddenCsePersonLicense.Number =
          export.CsePersonLicense.Number ?? "";
        export.LastReadHiddenCsePersonsWorkSet.Ssn =
          export.ApCsePersonsWorkSet.Ssn;

        // CQ66290
        export.LastReadHiddenCsePerson.ThreatOnStaff =
          export.ApCsePerson.ThreatOnStaff ?? "";

        // ---------------------------------------------
        // End   Code - Raju 12/24/1996 1030 hrs CST
        // ---------------------------------------------
        export.HiddenCsePerson.TextMessageIndicator =
          export.ApCsePerson.TextMessageIndicator ?? "";

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ----------------------------------------------------------------------
          // Now call the Family Violence CAB and pass the data to the CAB to 
          // check if the AP has  family violence Flag set.
          //                                                
          // Vithal(08/27/2001)
          // ----------------------------------------------------------------------
          UseScSecurityCheckForFv();

          if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
          {
            export.MultipleCases.Flag = local.MultipleCases.Flag;
            MoveSsnWorkArea2(local.ApSsnWorkArea, export.ApSsnWorkArea);
            export.AltSsn.Text13 = local.AltSsn.Text13;
            export.ApCsePersonsWorkSet.Dob = local.Zero.Date;
            export.ApCsePerson.BirthPlaceCity = "";
            export.ApCsePerson.BirthPlaceState = "";
            export.ApCsePerson.BirthplaceCountry = "";
            export.WorkForeignCountryDesc.Text40 = "";
            export.ApCsePerson.DateOfDeath = local.Zero.Date;
            export.ApCsePerson.IllegalAlienIndicator = "";
            export.ApCsePersonsWorkSet.Sex = "";
            export.ApCsePerson.Race = "";
            export.ApCsePerson.HeightFt = 0;
            export.ApCsePerson.HeightIn = 0;
            export.ApCsePerson.EyeColor = "";
            export.ApCsePerson.HairColor = "";
            export.ApCsePerson.Weight = 0;
            export.ApCsePerson.OtherIdInfo =
              Spaces(CsePerson.OtherIdInfo_MaxLength);
            export.CsePersonLicense.Assign(local.CsePersonLicense);
            export.ApCsePerson.Occupation = "";
            export.Uci.Flag = local.Uci.Flag;
            export.Military.Flag = local.Military.Flag;
            export.Incarceration.Flag = local.Incarceration.Flag;
            export.FedBenefits.Flag = local.FedBenefits.Flag;
            export.Bankruptcy.Flag = local.Bankruptcy.Flag;
            export.ApCsePerson.HomePhoneAreaCode = 0;
            export.ApCsePerson.HomePhone = 0;
            export.ApCsePerson.WorkPhoneAreaCode = 0;
            export.ApCsePerson.WorkPhone = 0;
            export.ApCsePerson.OtherAreaCode = 0;
            export.ApCsePerson.OtherNumber = 0;
            export.ApCsePerson.WorkPhoneExt = "";
            export.ApCsePerson.OtherPhoneType = "";
            export.ApCsePerson.NameMaiden = "";
            export.ApCsePerson.NameMiddle = "";
            export.ApCsePerson.CurrentMaritalStatus = "";
            export.ApCaseRole.MothersMaidenLastName = "";
            export.ApCaseRole.MothersFirstName = "";
            export.ApCaseRole.MothersMiddleInitial = "";
            export.ApCaseRole.FathersLastName = "";
            export.ApCaseRole.FathersFirstName = "";
            export.ApCaseRole.FathersMiddleInitial = "";
            export.ApCsePerson.CurrentSpouseLastName = "";
            export.ApCsePerson.CurrentSpouseFirstName = "";
            export.ApCsePerson.CurrentSpouseMi = "";
            export.OtherChildren.Flag = local.OtherChildren.Flag;
            export.OtherCsOrders.Flag = local.OtherCsOrders.Flag;
            export.ApCsePerson.AeCaseNumber = "";
            export.CsePersonEmailAddress.EmailAddress =
              Spaces(CsePersonEmailAddress.EmailAddress_MaxLength);
            export.ApCaseRole.LivingWithArIndicator = "";
            export.ApCaseRole.Note = Spaces(CaseRole.Note_MaxLength);

            break;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
          else if (AsChar(export.ApActive.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_AP";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

        // 07/30/99 Start
        export.HiddenApSex.Sex = export.ApCsePersonsWorkSet.Sex;

        // 07/30/99 End
        // CQ65304
        export.HiddenCsePerson.DateOfDeath = export.ApCsePerson.DateOfDeath;

        // CQ66290
        export.HiddenCsePerson.ThreatOnStaff =
          export.ApCsePerson.ThreatOnStaff ?? "";
        export.HiddenCsePerson.CustomerServiceCode =
          export.ApCsePerson.CustomerServiceCode ?? "";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // ---------------------------------------------
    // Code added by Raju  Dec 24, 1996:0200 hrs CST
    // The oe cab raise event will be called from
    //   here case of update.
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Infrastructure.UserId = "APDS";
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.ReferenceDate = local.Zero.Date;

      for(local.NumberOfEvents.TotalInteger = 1; local
        .NumberOfEvents.TotalInteger <= 6; ++local.NumberOfEvents.TotalInteger)
      {
        local.RaiseEventFlag.Text1 = "N";

        if (local.NumberOfEvents.TotalInteger == 1)
        {
          if (Equal(export.LastReadHiddenCsePerson.DateOfDeath, local.Zero.Date) &&
            !Equal(export.ApCsePerson.DateOfDeath, local.Zero.Date))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 10;
            local.Infrastructure.ReasonCode = "APDEAD";
            local.Infrastructure.ReferenceDate = export.ApCsePerson.DateOfDeath;
            local.DetailText30.Text30 = "AP's Date of Death :";
            local.Date.Date = local.Infrastructure.ReferenceDate;
            local.DetailText10.Text10 = UseCabConvertDate2String();
            local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + local
              .DetailText10.Text10;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 2)
        {
          // ---------------------------------------------
          // Code change - Raju 01/02/1997 1745 hrs CST
          //  - earlier change in license number was not
          //      considered. only first time change was
          //      an event
          //    now we consider any change to license
          //      number an event
          // ---------------------------------------------
          if (!IsEmpty(export.CsePersonLicense.Number))
          {
            if (!Equal(export.LastReadHiddenCsePersonLicense.Number,
              export.CsePersonLicense.Number))
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.EventId = 46;
              local.Infrastructure.ReasonCode = "DRIVLICNSE";
              local.Infrastructure.Detail =
                "AP's Driver's License Issuing State/# :  " + (
                  export.CsePersonLicense.IssuingState ?? "");
              local.DetailText10.Text10 = "/";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + TrimEnd
                (local.DetailText10.Text10);
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + TrimEnd
                (export.CsePersonLicense.Number);
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 3)
        {
          // ---------------------------------------------
          // 02/11/99 W.Campbell - IF stmt copied and
          // disabled just to keep it in case it needs to
          // be re-enabled again.
          // ---------------------------------------------
          if (Lt("000000000", export.LastReadHiddenCsePersonsWorkSet.Ssn) && !
            Equal(export.ApCsePersonsWorkSet.Ssn,
            export.LastReadHiddenCsePersonsWorkSet.Ssn))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.ReasonCode = "APSSNIDCHNG";
            local.DetailText30.Text30 = "AP's SSN Changed to :";
            local.Infrastructure.Detail = local.DetailText30.Text30;

            // ---------------------------------------------
            // 02/11/99 W.Campbell - IF stmt inserted to
            // log 'Unknown' when the SSN is changed
            // from non-blanks(zeros) to blanks (zeros).
            // ---------------------------------------------
            if (!Lt("000000000", export.ApCsePersonsWorkSet.Ssn))
            {
              local.DetailText30.Text30 = "Unknown";
            }
            else
            {
              local.DetailText10.Text10 =
                Substring(export.ApCsePersonsWorkSet.Ssn, 1, 3);
              local.DetailText10.Text10 =
                Substring(local.DetailText10.Text10,
                TextWorkArea.Text10_MaxLength, 1, 4) + Substring
                (export.ApCsePersonsWorkSet.Ssn,
                CsePersonsWorkSet.Ssn_MaxLength, 4, 2);
              local.DetailText30.Text30 =
                Substring(local.DetailText10.Text10,
                TextWorkArea.Text10_MaxLength, 1, 7) + Substring
                (export.ApCsePersonsWorkSet.Ssn,
                CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            }

            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
            local.DetailText10.Text10 = " From :";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.DetailText10.Text10 =
              Substring(export.LastReadHiddenCsePersonsWorkSet.Ssn, 1, 3);
            local.DetailText10.Text10 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 4) + Substring
              (export.LastReadHiddenCsePersonsWorkSet.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 4, 2);
            local.DetailText30.Text30 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 7) + Substring
              (export.LastReadHiddenCsePersonsWorkSet.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 4)
        {
          if ((IsEmpty(export.LastReadHiddenCsePersonsWorkSet.Ssn) || Equal
            (export.LastReadHiddenCsePersonsWorkSet.Ssn, "000000000")) && !
            IsEmpty(export.ApCsePersonsWorkSet.Ssn))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.ReasonCode = "APSSNID";
            local.DetailText30.Text30 = "AP's SSN Identified as :";
            local.Infrastructure.Detail = local.DetailText30.Text30;
            local.DetailText10.Text10 =
              Substring(export.ApCsePersonsWorkSet.Ssn, 1, 3);
            local.DetailText10.Text10 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 4) + Substring
              (export.ApCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
              4, 2);
            local.DetailText30.Text30 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 7) + Substring
              (export.ApCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
              6, 4);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 5)
        {
          if (Lt(local.Zero.Date, export.LastReadHiddenCsePerson.DateOfDeath) &&
            Equal(export.ApCsePerson.DateOfDeath, local.Zero.Date))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 10;
            local.Infrastructure.ReasonCode = "APNOTDEAD";
            local.Infrastructure.ReferenceDate = export.ApCsePerson.DateOfDeath;
            local.Date.Date = export.LastReadHiddenCsePerson.DateOfDeath;
            local.DetailText10.Text10 = UseCabConvertDate2String();

            // ----------------------------------------------------------------------------------
            // Per PR# 145609, the below code is commented and following code is
            // added .
            //                                                                  
            // Vithal(08/26/2002)
            // ------------------------------------------------------------------------------------
            local.Infrastructure.Detail = "AP's DOD entered as:" + TrimEnd
              (local.DetailText10.Text10) + "; DOD deleted by " + global
              .UserId;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 6)
        {
          if (AsChar(export.ApCsePerson.TextMessageIndicator) != AsChar
            (export.HiddenCsePerson.TextMessageIndicator))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.ReasonCode = "TXTMSGUPDAR";
            local.Message.Text50 =
              "The text message indicator has been changed from:";
            local.Infrastructure.Detail = local.Message.Text50 + " " + (
              export.HiddenCsePerson.TextMessageIndicator ?? "") + " to " + (
                export.ApCsePerson.TextMessageIndicator ?? "");
            export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
            local.AparSelection.Text1 = "R";
            UseSiAddrRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              UseEabRollbackCics();

              return;
            }

            local.AparSelection.Text1 = "P";
            local.Infrastructure.ReasonCode = "TXTMSGUPDAP";
            UseSiAddrRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              break;
            }
            else
            {
              UseEabRollbackCics();

              return;
            }
          }
        }
        else
        {
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          // --------------------------------------------
          // This is to aid the event processor to
          //    gather events from a single situation
          // This is an extremely important piece of code
          // --------------------------------------------
          local.ApForInfrastructure.Number = export.ApCsePersonsWorkSet.Number;
          UseOeCabRaiseEvent();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
          }
          else
          {
            UseEabRollbackCics();

            goto Test4;
          }
        }
      }

      export.LastReadHiddenCsePerson.DateOfDeath =
        export.ApCsePerson.DateOfDeath;
      export.LastReadHiddenCsePersonLicense.Number =
        export.CsePersonLicense.Number ?? "";
      export.LastReadHiddenCsePersonsWorkSet.Ssn =
        export.ApCsePersonsWorkSet.Ssn;
      export.HiddenCsePerson.TextMessageIndicator =
        import.ApCsePerson.TextMessageIndicator ?? "";

      // CQ66290
      export.LastReadHiddenCsePerson.ThreatOnStaff =
        export.ApCsePerson.ThreatOnStaff ?? "";
    }

Test4:

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
    // ------------------------------------------------------------------
    // 11/2/98  Added code to make sure protected fields stay protected when 
    // there is an error.
    // ------------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(export.HiddenAe.Flag) == 'O')
      {
        // ---------------------------------------------
        // This CSE Person is owned by the AE system and
        // may not be changed by the CSE system.
        // ---------------------------------------------
        var field1 = GetField(export.ApCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ApCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ApCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.ApCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ApCsePersonsWorkSet, "sex");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ApSsnWorkArea, "ssnNumPart1");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ApSsnWorkArea, "ssnNumPart2");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.ApSsnWorkArea, "ssnNumPart3");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.ApSsnWorkArea, "ssnTextPart1");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.ApSsnWorkArea, "ssnTextPart2");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.ApSsnWorkArea, "ssnTextPart3");

        field11.Color = "cyan";
        field11.Protected = true;
      }
    }

    // ---------------------------------------------
    // If all processing completed successfully, move
    // all imports to previous exports .
    // --------------------------------------------
    export.HiddenPrevCase.Number = export.Case1.Number;
    export.HiddenPrevCsePersonsWorkSet.Number =
      export.ApCsePersonsWorkSet.Number;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.MothersFirstName = source.MothersFirstName;
    target.MothersMiddleInitial = source.MothersMiddleInitial;
    target.FathersLastName = source.FathersLastName;
    target.FathersMiddleInitial = source.FathersMiddleInitial;
    target.FathersFirstName = source.FathersFirstName;
    target.MothersMaidenLastName = source.MothersMaidenLastName;
    target.ParentType = source.ParentType;
    target.NotifiedDate = source.NotifiedDate;
    target.NumberOfChildren = source.NumberOfChildren;
    target.LivingWithArIndicator = source.LivingWithArIndicator;
    target.NonpaymentCategory = source.NonpaymentCategory;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TribalCode = source.TribalCode;
    target.ThreatOnStaff = source.ThreatOnStaff;
    target.CustomerServiceCode = source.CustomerServiceCode;
    target.TaxId = source.TaxId;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TribalCode = source.TribalCode;
    target.ThreatOnStaff = source.ThreatOnStaff;
    target.CustomerServiceCode = source.CustomerServiceCode;
  }

  private static void MoveCsePerson3(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.DateOfDeath = source.DateOfDeath;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(local.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.ApSsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.ApSsnWorkArea);
  }

  private void UseCabUpdateAdabasPerson()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.ApCsePersonsWorkSet);

    Call(CabUpdateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
    local.DisplayForeignCountry.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.DisplayForeignCountry.Description = useExport.CodeValue.Description;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.ApForInfrastructure.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
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

    useImport.CsePersonsWorkSet.Number = import.ApCsePersonsWorkSet.Number;

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.ApCsePerson.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);
    useImport.AparSelection.Text1 = local.AparSelection.Text1;
    useImport.CsePerson.Number = export.ApCsePerson.Number;

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure2(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    useImport.Ap1.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    local.ApOccur.Flag = useExport.ApOccur.Flag;
  }

  private void UseSiCheckName()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    useImport.CsePersonsWorkSet.Assign(export.ApCsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonLicense()
  {
    var useImport = new SiCreateCsePersonLicense.Import();
    var useExport = new SiCreateCsePersonLicense.Export();

    useImport.CsePerson.Number = local.ApCsePerson.Number;
    useImport.CsePersonLicense.Assign(export.CsePersonLicense);

    Call(SiCreateCsePersonLicense.Execute, useImport, useExport);

    export.CsePersonLicense.Assign(useExport.CsePersonLicense);
  }

  private void UseSiReadApCaseRoleDetails()
  {
    var useImport = new SiReadApCaseRoleDetails.Import();
    var useExport = new SiReadApCaseRoleDetails.Export();

    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadApCaseRoleDetails.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.HiddenAe.Flag = useExport.Ae.Flag;
    export.CsePersonLicense.Assign(useExport.CsePersonLicense);
    export.MultipleCases.Flag = useExport.MultipleCases.Flag;
    export.Uci.Flag = useExport.Uci.Flag;
    export.Military.Flag = useExport.Military.Flag;
    export.Incarceration.Flag = useExport.Incarceration.Flag;
    export.FedBenefits.Flag = useExport.FedBens.Flag;
    export.Bankruptcy.Flag = useExport.Bankruptcy.Flag;
    export.OtherChildren.Flag = useExport.OtherChildren.Flag;
    export.OtherCsOrders.Flag = useExport.OtherCsOrders.Flag;
    MoveCsePersonsWorkSet3(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
    export.ApCaseRole.Assign(useExport.ApCaseRole);
    export.ApCsePerson.Assign(useExport.ApCsePerson);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.HiddenAe.Flag = useExport.Ae.Flag;
    export.ArCsePersonsWorkSet.Assign(useExport.Ar);
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
    export.ApActive.Flag = useExport.ApActive.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.HiddenAe.Flag = useExport.Ae.Flag;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
  }

  private void UseSiUpdateCaseRole()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.Case1.Number = import.Case1.Number;
    MoveCaseRole(import.ApCaseRole, useImport.CaseRole);
    useImport.CsePerson.Number = local.ApCsePerson.Number;

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    MoveCsePerson1(local.ApCsePerson, useImport.CsePerson);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePersonLicense()
  {
    var useImport = new SiUpdateCsePersonLicense.Import();
    var useExport = new SiUpdateCsePersonLicense.Export();

    useImport.CsePerson.Number = local.ApCsePerson.Number;
    useImport.CsePersonLicense.Assign(export.CsePersonLicense);

    Call(SiUpdateCsePersonLicense.Execute, useImport, useExport);
  }

  private bool ReadCaseRole1()
  {
    entities.Father.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Father.CasNumber = db.GetString(reader, 0);
        entities.Father.CspNumber = db.GetString(reader, 1);
        entities.Father.Type1 = db.GetString(reader, 2);
        entities.Father.Identifier = db.GetInt32(reader, 3);
        entities.Father.StartDate = db.GetNullableDate(reader, 4);
        entities.Father.EndDate = db.GetNullableDate(reader, 5);
        entities.Father.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.Father.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Father.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Father.CreatedBy = db.GetString(reader, 9);
        entities.Father.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Father.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Mother.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Mother.CasNumber = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 1);
        entities.Mother.Type1 = db.GetString(reader, 2);
        entities.Mother.Identifier = db.GetInt32(reader, 3);
        entities.Mother.StartDate = db.GetNullableDate(reader, 4);
        entities.Mother.EndDate = db.GetNullableDate(reader, 5);
        entities.Mother.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.Mother.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Mother.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Mother.CreatedBy = db.GetString(reader, 9);
        entities.Mother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "endDate", date);
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 11);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 18);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private bool ReadCsePersonEmailAddress()
  {
    entities.CsePersonEmailAddress.Populated = false;

    return Read("ReadCsePersonEmailAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonEmailAddress.CspNumber = db.GetString(reader, 0);
        entities.CsePersonEmailAddress.EndDate = db.GetNullableDate(reader, 1);
        entities.CsePersonEmailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CsePersonEmailAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CsePersonEmailAddress.EmailAddress =
          db.GetNullableString(reader, 4);
        entities.CsePersonEmailAddress.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private void UpdateCsePersonAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var endDate = Now().Date;
    var endCode = "DC";

    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
    entities.CsePersonAddress.Populated = true;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of TribalFlag.
    /// </summary>
    [JsonPropertyName("tribalFlag")]
    public Common TribalFlag
    {
      get => tribalFlag ??= new();
      set => tribalFlag = value;
    }

    /// <summary>
    /// A value of TribalPrompt.
    /// </summary>
    [JsonPropertyName("tribalPrompt")]
    public Common TribalPrompt
    {
      get => tribalPrompt ??= new();
      set => tribalPrompt = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of TextMessageDelete.
    /// </summary>
    [JsonPropertyName("textMessageDelete")]
    public WorkArea TextMessageDelete
    {
      get => textMessageDelete ??= new();
      set => textMessageDelete = value;
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
    /// A value of HiddenApSex.
    /// </summary>
    [JsonPropertyName("hiddenApSex")]
    public CsePersonsWorkSet HiddenApSex
    {
      get => hiddenApSex ??= new();
      set => hiddenApSex = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of PhoneTypePrompt.
    /// </summary>
    [JsonPropertyName("phoneTypePrompt")]
    public Common PhoneTypePrompt
    {
      get => phoneTypePrompt ??= new();
      set => phoneTypePrompt = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public CsePersonsWorkSet ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of EyesPrompt.
    /// </summary>
    [JsonPropertyName("eyesPrompt")]
    public Common EyesPrompt
    {
      get => eyesPrompt ??= new();
      set => eyesPrompt = value;
    }

    /// <summary>
    /// A value of HairPrompt.
    /// </summary>
    [JsonPropertyName("hairPrompt")]
    public Common HairPrompt
    {
      get => hairPrompt ??= new();
      set => hairPrompt = value;
    }

    /// <summary>
    /// A value of DlStatePrompt.
    /// </summary>
    [JsonPropertyName("dlStatePrompt")]
    public Common DlStatePrompt
    {
      get => dlStatePrompt ??= new();
      set => dlStatePrompt = value;
    }

    /// <summary>
    /// A value of MaritalStatusPrompt.
    /// </summary>
    [JsonPropertyName("maritalStatusPrompt")]
    public Common MaritalStatusPrompt
    {
      get => maritalStatusPrompt ??= new();
      set => maritalStatusPrompt = value;
    }

    /// <summary>
    /// A value of CitizenStatusPrompt.
    /// </summary>
    [JsonPropertyName("citizenStatusPrompt")]
    public Common CitizenStatusPrompt
    {
      get => citizenStatusPrompt ??= new();
      set => citizenStatusPrompt = value;
    }

    /// <summary>
    /// A value of RacePrompt.
    /// </summary>
    [JsonPropertyName("racePrompt")]
    public Common RacePrompt
    {
      get => racePrompt ??= new();
      set => racePrompt = value;
    }

    /// <summary>
    /// A value of PobStPrompt.
    /// </summary>
    [JsonPropertyName("pobStPrompt")]
    public Common PobStPrompt
    {
      get => pobStPrompt ??= new();
      set => pobStPrompt = value;
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
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
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
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of MultipleCases.
    /// </summary>
    [JsonPropertyName("multipleCases")]
    public Common MultipleCases
    {
      get => multipleCases ??= new();
      set => multipleCases = value;
    }

    /// <summary>
    /// A value of AltSsn.
    /// </summary>
    [JsonPropertyName("altSsn")]
    public WorkArea AltSsn
    {
      get => altSsn ??= new();
      set => altSsn = value;
    }

    /// <summary>
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of FedBenefits.
    /// </summary>
    [JsonPropertyName("fedBenefits")]
    public Common FedBenefits
    {
      get => fedBenefits ??= new();
      set => fedBenefits = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of OtherChildren.
    /// </summary>
    [JsonPropertyName("otherChildren")]
    public Common OtherChildren
    {
      get => otherChildren ??= new();
      set => otherChildren = value;
    }

    /// <summary>
    /// A value of OtherCsOrders.
    /// </summary>
    [JsonPropertyName("otherCsOrders")]
    public Common OtherCsOrders
    {
      get => otherCsOrders ??= new();
      set => otherCsOrders = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPrevCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevCase")]
    public Case1 HiddenPrevCase
    {
      get => hiddenPrevCase ??= new();
      set => hiddenPrevCase = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ApSsnWorkArea.
    /// </summary>
    [JsonPropertyName("apSsnWorkArea")]
    public SsnWorkArea ApSsnWorkArea
    {
      get => apSsnWorkArea ??= new();
      set => apSsnWorkArea = value;
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
    /// A value of LastReadHiddenCsePersonLicense.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCsePersonLicense")]
    public CsePersonLicense LastReadHiddenCsePersonLicense
    {
      get => lastReadHiddenCsePersonLicense ??= new();
      set => lastReadHiddenCsePersonLicense = value;
    }

    /// <summary>
    /// A value of LastReadHiddenCsePerson.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCsePerson")]
    public CsePerson LastReadHiddenCsePerson
    {
      get => lastReadHiddenCsePerson ??= new();
      set => lastReadHiddenCsePerson = value;
    }

    /// <summary>
    /// A value of LastReadHiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet LastReadHiddenCsePersonsWorkSet
    {
      get => lastReadHiddenCsePersonsWorkSet ??= new();
      set => lastReadHiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
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
    /// A value of PobFcPrompt.
    /// </summary>
    [JsonPropertyName("pobFcPrompt")]
    public Common PobFcPrompt
    {
      get => pobFcPrompt ??= new();
      set => pobFcPrompt = value;
    }

    /// <summary>
    /// A value of CustomerServicePrompt.
    /// </summary>
    [JsonPropertyName("customerServicePrompt")]
    public Common CustomerServicePrompt
    {
      get => customerServicePrompt ??= new();
      set => customerServicePrompt = value;
    }

    private CsePersonAddress csePersonAddress;
    private Common tribalFlag;
    private Common tribalPrompt;
    private CsePerson hiddenCsePerson;
    private CsePersonEmailAddress csePersonEmailAddress;
    private WorkArea textMessageDelete;
    private WorkArea headerLine;
    private CsePersonsWorkSet hiddenApSex;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common phoneTypePrompt;
    private CsePersonsWorkSet apSelected;
    private CodeValue selected;
    private Common eyesPrompt;
    private Common hairPrompt;
    private Common dlStatePrompt;
    private Common maritalStatusPrompt;
    private Common citizenStatusPrompt;
    private Common racePrompt;
    private Common pobStPrompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet ar;
    private CsePersonLicense csePersonLicense;
    private Case1 case1;
    private Common multipleCases;
    private WorkArea altSsn;
    private Common uci;
    private Common military;
    private Common incarceration;
    private Common fedBenefits;
    private Common bankruptcy;
    private Common otherChildren;
    private Common otherCsOrders;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private Case1 hiddenPrevCase;
    private Case1 next;
    private Standard standard;
    private SsnWorkArea apSsnWorkArea;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonLicense lastReadHiddenCsePersonLicense;
    private CsePerson lastReadHiddenCsePerson;
    private CsePersonsWorkSet lastReadHiddenCsePersonsWorkSet;
    private Common apActive;
    private Common caseOpen;
    private Common pobFcPrompt;
    private Common customerServicePrompt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of TribalFlag.
    /// </summary>
    [JsonPropertyName("tribalFlag")]
    public Common TribalFlag
    {
      get => tribalFlag ??= new();
      set => tribalFlag = value;
    }

    /// <summary>
    /// A value of TribalPrompt.
    /// </summary>
    [JsonPropertyName("tribalPrompt")]
    public Common TribalPrompt
    {
      get => tribalPrompt ??= new();
      set => tribalPrompt = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of TextMessagDelete.
    /// </summary>
    [JsonPropertyName("textMessagDelete")]
    public WorkArea TextMessagDelete
    {
      get => textMessagDelete ??= new();
      set => textMessagDelete = value;
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
    /// A value of HiddenApSex.
    /// </summary>
    [JsonPropertyName("hiddenApSex")]
    public CsePersonsWorkSet HiddenApSex
    {
      get => hiddenApSex ??= new();
      set => hiddenApSex = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of PhoneTypePrompt.
    /// </summary>
    [JsonPropertyName("phoneTypePrompt")]
    public Common PhoneTypePrompt
    {
      get => phoneTypePrompt ??= new();
      set => phoneTypePrompt = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public CsePersonsWorkSet ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of EyesPrompt.
    /// </summary>
    [JsonPropertyName("eyesPrompt")]
    public Common EyesPrompt
    {
      get => eyesPrompt ??= new();
      set => eyesPrompt = value;
    }

    /// <summary>
    /// A value of HairPrompt.
    /// </summary>
    [JsonPropertyName("hairPrompt")]
    public Common HairPrompt
    {
      get => hairPrompt ??= new();
      set => hairPrompt = value;
    }

    /// <summary>
    /// A value of DlStatePrompt.
    /// </summary>
    [JsonPropertyName("dlStatePrompt")]
    public Common DlStatePrompt
    {
      get => dlStatePrompt ??= new();
      set => dlStatePrompt = value;
    }

    /// <summary>
    /// A value of MaritalStatusPrompt.
    /// </summary>
    [JsonPropertyName("maritalStatusPrompt")]
    public Common MaritalStatusPrompt
    {
      get => maritalStatusPrompt ??= new();
      set => maritalStatusPrompt = value;
    }

    /// <summary>
    /// A value of CitizenStatusPrompt.
    /// </summary>
    [JsonPropertyName("citizenStatusPrompt")]
    public Common CitizenStatusPrompt
    {
      get => citizenStatusPrompt ??= new();
      set => citizenStatusPrompt = value;
    }

    /// <summary>
    /// A value of RacePrompt.
    /// </summary>
    [JsonPropertyName("racePrompt")]
    public Common RacePrompt
    {
      get => racePrompt ??= new();
      set => racePrompt = value;
    }

    /// <summary>
    /// A value of PobStPrompt.
    /// </summary>
    [JsonPropertyName("pobStPrompt")]
    public Common PobStPrompt
    {
      get => pobStPrompt ??= new();
      set => pobStPrompt = value;
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
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
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
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of MultipleCases.
    /// </summary>
    [JsonPropertyName("multipleCases")]
    public Common MultipleCases
    {
      get => multipleCases ??= new();
      set => multipleCases = value;
    }

    /// <summary>
    /// A value of AltSsn.
    /// </summary>
    [JsonPropertyName("altSsn")]
    public WorkArea AltSsn
    {
      get => altSsn ??= new();
      set => altSsn = value;
    }

    /// <summary>
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of FedBenefits.
    /// </summary>
    [JsonPropertyName("fedBenefits")]
    public Common FedBenefits
    {
      get => fedBenefits ??= new();
      set => fedBenefits = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of OtherChildren.
    /// </summary>
    [JsonPropertyName("otherChildren")]
    public Common OtherChildren
    {
      get => otherChildren ??= new();
      set => otherChildren = value;
    }

    /// <summary>
    /// A value of OtherCsOrders.
    /// </summary>
    [JsonPropertyName("otherCsOrders")]
    public Common OtherCsOrders
    {
      get => otherCsOrders ??= new();
      set => otherCsOrders = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPrevCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevCase")]
    public Case1 HiddenPrevCase
    {
      get => hiddenPrevCase ??= new();
      set => hiddenPrevCase = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of ApSsnWorkArea.
    /// </summary>
    [JsonPropertyName("apSsnWorkArea")]
    public SsnWorkArea ApSsnWorkArea
    {
      get => apSsnWorkArea ??= new();
      set => apSsnWorkArea = value;
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
    /// A value of LastReadHiddenCsePersonLicense.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCsePersonLicense")]
    public CsePersonLicense LastReadHiddenCsePersonLicense
    {
      get => lastReadHiddenCsePersonLicense ??= new();
      set => lastReadHiddenCsePersonLicense = value;
    }

    /// <summary>
    /// A value of LastReadHiddenCsePerson.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCsePerson")]
    public CsePerson LastReadHiddenCsePerson
    {
      get => lastReadHiddenCsePerson ??= new();
      set => lastReadHiddenCsePerson = value;
    }

    /// <summary>
    /// A value of LastReadHiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet LastReadHiddenCsePersonsWorkSet
    {
      get => lastReadHiddenCsePersonsWorkSet ??= new();
      set => lastReadHiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
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
    /// A value of PobFcPrompt.
    /// </summary>
    [JsonPropertyName("pobFcPrompt")]
    public Common PobFcPrompt
    {
      get => pobFcPrompt ??= new();
      set => pobFcPrompt = value;
    }

    /// <summary>
    /// A value of WorkForeignCountryDesc.
    /// </summary>
    [JsonPropertyName("workForeignCountryDesc")]
    public WorkArea WorkForeignCountryDesc
    {
      get => workForeignCountryDesc ??= new();
      set => workForeignCountryDesc = value;
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
    /// A value of CustomerServicePrompt.
    /// </summary>
    [JsonPropertyName("customerServicePrompt")]
    public Common CustomerServicePrompt
    {
      get => customerServicePrompt ??= new();
      set => customerServicePrompt = value;
    }

    private CsePersonAddress csePersonAddress;
    private Common tribalFlag;
    private Common tribalPrompt;
    private CsePerson hiddenCsePerson;
    private CsePersonEmailAddress csePersonEmailAddress;
    private WorkArea textMessagDelete;
    private WorkArea headerLine;
    private CsePersonsWorkSet hiddenApSex;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common phoneTypePrompt;
    private CsePersonsWorkSet apSelected;
    private Code prompt;
    private Common eyesPrompt;
    private Common hairPrompt;
    private Common dlStatePrompt;
    private Common maritalStatusPrompt;
    private Common citizenStatusPrompt;
    private Common racePrompt;
    private Common pobStPrompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonLicense csePersonLicense;
    private Case1 case1;
    private Common multipleCases;
    private WorkArea altSsn;
    private Common uci;
    private Common military;
    private Common incarceration;
    private Common fedBenefits;
    private Common bankruptcy;
    private Common otherChildren;
    private Common otherCsOrders;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private Case1 hiddenPrevCase;
    private Case1 next;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private SsnWorkArea apSsnWorkArea;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonLicense lastReadHiddenCsePersonLicense;
    private CsePerson lastReadHiddenCsePerson;
    private CsePersonsWorkSet lastReadHiddenCsePersonsWorkSet;
    private Common apActive;
    private Common caseOpen;
    private Common pobFcPrompt;
    private WorkArea workForeignCountryDesc;
    private CsePerson arCsePerson;
    private Common customerServicePrompt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public CsePersonAddress Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of SsnConcat.
    /// </summary>
    [JsonPropertyName("ssnConcat")]
    public TextWorkArea SsnConcat
    {
      get => ssnConcat ??= new();
      set => ssnConcat = value;
    }

    /// <summary>
    /// A value of SsnPart.
    /// </summary>
    [JsonPropertyName("ssnPart")]
    public Common SsnPart
    {
      get => ssnPart ??= new();
      set => ssnPart = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of MultipleCases.
    /// </summary>
    [JsonPropertyName("multipleCases")]
    public Common MultipleCases
    {
      get => multipleCases ??= new();
      set => multipleCases = value;
    }

    /// <summary>
    /// A value of AltSsn.
    /// </summary>
    [JsonPropertyName("altSsn")]
    public WorkArea AltSsn
    {
      get => altSsn ??= new();
      set => altSsn = value;
    }

    /// <summary>
    /// A value of Uci.
    /// </summary>
    [JsonPropertyName("uci")]
    public Common Uci
    {
      get => uci ??= new();
      set => uci = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public Common Military
    {
      get => military ??= new();
      set => military = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Common Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of FedBenefits.
    /// </summary>
    [JsonPropertyName("fedBenefits")]
    public Common FedBenefits
    {
      get => fedBenefits ??= new();
      set => fedBenefits = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Common Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of OtherChildren.
    /// </summary>
    [JsonPropertyName("otherChildren")]
    public Common OtherChildren
    {
      get => otherChildren ??= new();
      set => otherChildren = value;
    }

    /// <summary>
    /// A value of OtherCsOrders.
    /// </summary>
    [JsonPropertyName("otherCsOrders")]
    public Common OtherCsOrders
    {
      get => otherCsOrders ??= new();
      set => otherCsOrders = value;
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
    /// A value of ApSsnWorkArea.
    /// </summary>
    [JsonPropertyName("apSsnWorkArea")]
    public SsnWorkArea ApSsnWorkArea
    {
      get => apSsnWorkArea ??= new();
      set => apSsnWorkArea = value;
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
    /// A value of ApOccur.
    /// </summary>
    [JsonPropertyName("apOccur")]
    public Common ApOccur
    {
      get => apOccur ??= new();
      set => apOccur = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
    }

    /// <summary>
    /// A value of NumberOfEvents.
    /// </summary>
    [JsonPropertyName("numberOfEvents")]
    public Common NumberOfEvents
    {
      get => numberOfEvents ??= new();
      set => numberOfEvents = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
    }

    /// <summary>
    /// A value of ApForInfrastructure.
    /// </summary>
    [JsonPropertyName("apForInfrastructure")]
    public CsePerson ApForInfrastructure
    {
      get => apForInfrastructure ??= new();
      set => apForInfrastructure = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of DisplayForeignCountry.
    /// </summary>
    [JsonPropertyName("displayForeignCountry")]
    public CodeValue DisplayForeignCountry
    {
      get => displayForeignCountry ??= new();
      set => displayForeignCountry = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public WorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
    }

    private CsePersonAddress update;
    private DateWorkArea max;
    private DateWorkArea current;
    private SsnWorkArea convert;
    private TextWorkArea ssnConcat;
    private Common ssnPart;
    private CsePersonLicense csePersonLicense;
    private Common multipleCases;
    private WorkArea altSsn;
    private Common uci;
    private Common military;
    private Common incarceration;
    private Common fedBenefits;
    private Common bankruptcy;
    private Common otherChildren;
    private Common otherCsOrders;
    private CaseRole apCaseRole;
    private SsnWorkArea apSsnWorkArea;
    private DateWorkArea zero;
    private Common apOccur;
    private Common multipleAps;
    private AbendData abendData;
    private CsePerson apCsePerson;
    private Code code;
    private CodeValue codeValue;
    private Common common;
    private Common invalid;
    private NextTranInfo nextTranInfo;
    private SsnWorkArea ssnWorkArea;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private TextWorkArea detailText10;
    private DateWorkArea date;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private TextWorkArea detailText30;
    private CsePerson apForInfrastructure;
    private Infrastructure lastTran;
    private CodeValue displayForeignCountry;
    private WorkArea message;
    private WorkArea aparSelection;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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
    /// A value of Father.
    /// </summary>
    [JsonPropertyName("father")]
    public CaseRole Father
    {
      get => father ??= new();
      set => father = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CaseRole Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    private CsePersonAddress csePersonAddress;
    private CsePersonEmailAddress csePersonEmailAddress;
    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
    private CaseRole father;
    private CaseRole mother;
  }
#endregion
}
