// Program: SI_PADS_PARENTAL_DETAILS, ID: 371758818, model: 746.
// Short name: SWEPADSP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PADS_PARENTAL_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPadsParentalDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PADS_PARENTAL_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPadsParentalDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPadsParentalDetails.
  /// </summary>
  public SiPadsParentalDetails(IContext context, Import import, Export export):
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
    //                    M A I N T E N A N C E    L O G
    // Date	  Developer	 Description
    // 04-24-95  Helen Sharland Init Dev
    // 06/28/96  G. Lofton	Changed ssn to numeric fields
    // 09/24/96  G. Lofton	Add county code.
    // 11/01/96  G. Lofton	Add new security and removed old.
    // 05/02/97  Sid		Problem Fixes
    // 09/18/98  C Deghand  Modified Exit state after header to
    //                      continue processing if there is no AP.
    // 09/28/98  C Deghand  Added SET statements to set prompts to spaces.
    // 11/04/98  C Deghand  Added code to make sure protected fields stay
    //                      protected on errors.
    // ------------------------------------------------------------
    // 01/13/99 W.Campbell  Removed set statements
    //                      setting CSE_PERSON_ADDRESS
    //                      zdel_start_date to CURRENT_DATE.
    //                      Work done on IDCR454.
    // -----------------------------------------------------------
    // 01/14/99 W.Campbell  Modified USE'd CAB
    //                      SI_PADS_READ_PARENTAL_CASE_ROLE
    //                      in order to use new logic to obtain
    //                      the CSE_PERSON_ADDRESS.
    // ------------------------------------------------------
    // 01/14/99 W.Campbell  Changed two set statements.
    //                      One to use export_father instead
    //                      of import_father. And one to use
    //                      export_mother instead of
    //                      import_mother.
    // ---------------------------------------------
    // 01/16/99 W.Campbell  Inserted an
    //                      IF statement to check for
    //                      the state of KS and
    //                      placed the county logic inside it.
    // ---------------------------------------------
    // 01/16/99 W.Campbell  Inserted an
    //                      IF statement to check for
    //                      KS county and modified
    //                      some of the logic which
    //                      was placed inside the IF statement.
    // ---------------------------------------------
    // 01/16/99 W.Campbell  Inserted a set
    //                      statement to initialize the
    //                      local_current date to CURRENT_DATE.
    // ---------------------------------------------
    // 01/16/99 W.Campbell  Inserted a set
    //                      statement to set the
    //                      CSE_PERSON_ADDRESS
    //                      VERIFIED_DATE
    //                      to local_current date.
    // ---------------------------------------------
    // 01/19/99 W.Campbell  Rematched some USE'd
    //                      CABs views to match EXPORT views
    //                      instead of IMPORT views to fix
    //                      view matching problems.
    // ---------------------------------------------
    // 01/20/99 W.Campbell  Inserted logic to allow
    //                      for update of ADABAS person
    //                      information (DOB and SSN)
    //                      on a CREATE.
    // ---------------------------------------------
    // 01/21/99 W.Campbell  Rematched USE'd
    //                      CABs views for ADABAS update
    //                      to match EXPORT views
    //                      instead of IMPORT views to fix
    //                      view matching problems for
    //                      SSN update.
    // ---------------------------------------------
    // 01/22/99 W.Campbell  Inserted logic to
    //                      USE EAB_ROLLBACK_CICS
    //                      to handle errors from all
    //                      CREATE or UPDATE CABS.
    // ---------------------------------------------
    // 01/22/99 W.Campbell  Disabled logic to
    //                      prevent the update of CASE_ROLE
    //                      in the CREATE logic for both
    //                      mother and father.
    // ---------------------------------------------
    // 01/25/99 W.Campbell  Changed an
    //                      IF statement to include
    //                      no_aps_on_a_case in order to get
    //                      the SSN displayed on the screen
    //                      for a father.
    // ------------------------------------------------------------
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
    // 11/27/00 M.Lachowicz            Changed header line.
    //                                 
    // WR #298.
    // -----------------------------------------------
    // 12/04/2001    Vithal Madhira        PR# 121249, 124583, 124584
    // Fixed the code for family violence indicator. Changed code in SWE01082(
    // SC_CAB_TEST_SECURITY)  and SWE00301(SC_SECURITY_VALID_AUTH_FOR_FV) CABs
    // also.
    // --------------------------------------------------------------------------------------
    // 12/23/2002 LBachura. PR162108. Install enterable Date of Death for the 
    // Mother on the PADS screen.
    // 06/01/07 LSS  PR#209846 Inserted logic to check length of zip5 and zip4 
    // for CASE create and CASE update.
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    MoveNextTranInfo(import.Hidden, export.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // 01/16/99 W.Campbell - Inserted the following
    // set statement to initialize the
    // local_current date to CURRENT_DATE.
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    export.MotherCsePerson.Assign(import.MotherCsePerson);
    export.Ap.Assign(import.Ap);
    export.Ar.Assign(import.Ar);
    export.FatherCaseRole.Assign(import.FatherCaseRole);
    export.FatherCsePersonAddress.Assign(import.FatherCsePersonAddress);
    export.FatherCommon.SelectChar = import.FatherCommon.SelectChar;
    export.FatherCsePersonsWorkSet.Assign(import.FatherCsePersonsWorkSet);
    export.FatherSsnWorkArea.Assign(import.FatherSsnWorkArea);
    export.MotherCaseRole.Assign(import.MotherCaseRole);
    export.MotherCsePersonAddress.Assign(import.MotherCsePersonAddress);
    export.MotherCsePersonsWorkSet.Assign(import.MotherCsePersonsWorkSet);
    export.MotherSsnWorkArea.Assign(import.MotherSsnWorkArea);
    export.MotherCommon.SelectChar = import.MotherCommon.SelectChar;
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.MoCntyPrompt.SelectChar = import.MoCntyPrompt.SelectChar;
    export.FaCntyPrompt.SelectChar = import.FaCntyPrompt.SelectChar;
    export.FaStatePrompt.SelectChar = import.FaStatePrompt.SelectChar;
    export.MoStatePrompt.SelectChar = import.MoStatePrompt.SelectChar;
    export.ApSelected.Assign(import.ApSelected);
    export.FaHiddenAe.Flag = import.FaHiddenAe.Flag;
    export.FaHiddenOtherCr.Flag = import.FaHiddenOtherCr.Flag;
    export.FaHiddenOtherCase.Flag = import.FaHiddenOtherCase.Flag;
    export.MoHiddenAe.Flag = import.MoHiddenAe.Flag;
    export.MoHiddenOtherCr.Flag = import.MoHiddenOtherCr.Flag;
    export.MoHiddenOtherCase.Flag = import.MoHiddenOtherCase.Flag;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);

    // 11/27/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/27/00 M.L End
    export.CaseOpen.Flag = import.CaseOpen.Flag;

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPrevFather.Identifier = import.HiddenPrevFather.Identifier;
    export.HiddenPrevMother.Identifier = import.HiddenPrevMother.Identifier;

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.Ap.Number;
      export.Hidden.CaseNumber = export.Next.Number;
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
        export.Ap.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
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

    if (!Equal(global.Command, "DISPLAY") && !Equal(global.Command, "RETCOMP"))
    {
      // ------------------------------------------------------------
      // Protect fields if required
      // ------------------------------------------------------------
      if (AsChar(export.MoHiddenAe.Flag) == 'O')
      {
        var field1 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePerson, "dateOfDeath");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonsWorkSet, "number");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field6.Color = "cyan";
        field6.Protected = true;
      }

      if (AsChar(export.MoHiddenOtherCr.Flag) == 'Y')
      {
        var field1 = GetField(export.MotherCsePersonAddress, "street1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePersonAddress, "street2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonAddress, "city");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherCsePersonAddress, "state");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherCsePersonAddress, "zipCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherCsePersonAddress, "zip4");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.MotherCsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.MoStatePrompt, "selectChar");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.MoCntyPrompt, "selectChar");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.MotherCsePerson, "dateOfDeath");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.MotherCsePersonsWorkSet, "number");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field15.Color = "cyan";
        field15.Protected = true;
      }

      if (AsChar(export.MoHiddenOtherCase.Flag) == 'Y')
      {
        var field1 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePersonsWorkSet, "number");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      if (IsEmpty(export.MotherCsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.MotherCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePersonsWorkSet, "lastName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherCsePersonAddress, "street1");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherCsePersonAddress, "street2");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.MotherCsePersonAddress, "city");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.MotherCsePersonAddress, "state");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.MotherCsePersonAddress, "zipCode");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.MotherCsePersonAddress, "zip4");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.MotherCsePersonAddress, "county");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.MotherCsePerson, "dateOfDeath");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.MotherCsePersonsWorkSet, "number");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.MotherCaseRole, "note");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.MoStatePrompt, "selectChar");

        field19.Color = "cyan";
        field19.Protected = true;

        var field20 = GetField(export.MoCntyPrompt, "selectChar");

        field20.Color = "cyan";
        field20.Protected = true;
      }

      if (AsChar(export.FaHiddenAe.Flag) == 'O')
      {
        var field1 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "number");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      if (AsChar(export.FaHiddenOtherCr.Flag) == 'Y')
      {
        var field1 = GetField(export.FatherCsePersonAddress, "street1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonAddress, "street2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherCsePersonAddress, "city");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherCsePersonAddress, "state");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherCsePersonAddress, "zipCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.FatherCsePersonAddress, "zip4");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.FatherCsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.FaStatePrompt, "selectChar");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.FaCntyPrompt, "selectChar");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.FatherCsePersonsWorkSet, "number");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field14.Color = "cyan";
        field14.Protected = true;
      }

      if (AsChar(export.FaHiddenOtherCase.Flag) == 'Y')
      {
        var field1 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "number");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      if (IsEmpty(export.FatherCsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.FatherCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherCsePersonAddress, "street1");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.FatherCsePersonAddress, "street2");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.FatherCsePersonAddress, "city");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.FatherCsePersonAddress, "state");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.FatherCsePersonAddress, "zipCode");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.FatherCsePersonAddress, "zip4");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.FatherCsePersonAddress, "county");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.FatherCsePersonsWorkSet, "number");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.FatherCaseRole, "note");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.FaStatePrompt, "selectChar");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.FaCntyPrompt, "selectChar");

        field19.Color = "cyan";
        field19.Protected = true;
      }

      if (export.FatherSsnWorkArea.SsnNumPart1 == 0 && export
        .FatherSsnWorkArea.SsnNumPart2 == 0 && export
        .FatherSsnWorkArea.SsnNumPart3 == 0)
      {
        export.FatherCsePersonsWorkSet.Ssn = "";
      }
      else
      {
        MoveSsnWorkArea(export.FatherSsnWorkArea, local.SsnWorkArea);
        local.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText2();
        export.FatherCsePersonsWorkSet.Ssn = export.FatherSsnWorkArea.SsnText9;
      }

      if (export.MotherSsnWorkArea.SsnNumPart1 == 0 && export
        .MotherSsnWorkArea.SsnNumPart2 == 0 && export
        .MotherSsnWorkArea.SsnNumPart3 == 0)
      {
        export.MotherCsePersonsWorkSet.Ssn = "";
      }
      else
      {
        MoveSsnWorkArea(export.MotherSsnWorkArea, local.SsnWorkArea);
        local.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText1();
        export.MotherCsePersonsWorkSet.Ssn = export.MotherSsnWorkArea.SsnText9;
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
      // ---------------------------------------------
      // When the control is returned from a LIST screen
      // Populate the appropriate prompt fields.
      // ---------------------------------------------
      if (AsChar(export.MoStatePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.MotherCsePersonAddress.State = import.Selected.Cdvalue;
        }

        export.MoStatePrompt.SelectChar = "";

        var field = GetField(export.MoStatePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.MoCntyPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.MotherCsePersonAddress.County = import.Selected.Cdvalue;
        }

        export.MoCntyPrompt.SelectChar = "";

        var field = GetField(export.MoCntyPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.FaStatePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FatherCsePersonAddress.State = import.Selected.Cdvalue;
        }

        export.FaStatePrompt.SelectChar = "";

        var field = GetField(export.FaStatePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.FaCntyPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.FatherCsePersonAddress.County = import.Selected.Cdvalue;
        }

        export.FaCntyPrompt.SelectChar = "";

        var field = GetField(export.FaCntyPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      return;
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";

        var field = GetField(export.ApPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(import.ApSelected.Number))
      {
        MoveCsePersonsWorkSet(import.ApSelected, export.Ap);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(AsChar(import.FatherCommon.SelectChar))
    {
      case ' ':
        break;
      case 'S':
        break;
      default:
        var field = GetField(export.FatherCommon, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        break;
    }

    switch(AsChar(import.MotherCommon.SelectChar))
    {
      case ' ':
        break;
      case 'S':
        break;
      default:
        var field = GetField(export.MotherCommon, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        break;
    }

    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
    {
      return;
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE"))
    {
      if (IsEmpty(import.FatherCommon.SelectChar) && IsEmpty
        (import.MotherCommon.SelectChar))
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        var field1 = GetField(export.FatherCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.MotherCommon, "selectChar");

        field2.Error = true;
      }

      if (AsChar(import.FaHiddenOtherCr.Flag) == 'Y')
      {
        var field1 = GetField(export.FatherCsePersonAddress, "street1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonAddress, "street2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherCsePersonAddress, "city");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherCsePersonAddress, "state");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherCsePersonAddress, "zipCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.FatherCsePersonAddress, "zip4");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.FatherCsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.FatherCsePersonsWorkSet, "number");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field12.Color = "cyan";
        field12.Protected = true;
      }

      if (AsChar(import.MoHiddenOtherCr.Flag) == 'Y')
      {
        var field1 = GetField(export.MotherCsePersonAddress, "street1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePersonAddress, "street2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonAddress, "city");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherCsePersonAddress, "state");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherCsePersonAddress, "zipCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherCsePersonAddress, "zip4");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.MotherCsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.MotherCsePerson, "dateOfDeath");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.MotherCsePersonsWorkSet, "number");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field13.Color = "cyan";
        field13.Protected = true;
      }

      if (AsChar(import.MoHiddenAe.Flag) == 'O' || AsChar
        (import.MoHiddenOtherCase.Flag) == 'Y')
      {
        var field1 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePerson, "dateOfDeath");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonsWorkSet, "number");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field6.Color = "cyan";
        field6.Protected = true;
      }

      if (AsChar(import.FaHiddenAe.Flag) == 'O' || AsChar
        (import.FaHiddenOtherCase.Flag) == 'Y')
      {
        var field1 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "number");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
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

        switch(AsChar(import.MoStatePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.MoStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.MoCntyPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "COUNTY CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.MoCntyPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.FaStatePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.FaStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.FaCntyPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "COUNTY CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.FaCntyPrompt, "selectChar");

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

            if (AsChar(export.MoStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.MoStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.MoCntyPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.MoCntyPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.FaStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.FaStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.FaCntyPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.FaCntyPrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        break;
      case "CREATE":
        // ---------------------------------------------
        // Validate and add the mother's address
        // ---------------------------------------------
        switch(AsChar(import.MotherCommon.SelectChar))
        {
          case 'S':
            if (AsChar(export.MoHiddenAe.Flag) == 'O' && AsChar
              (export.MoHiddenOtherCr.Flag) == 'Y')
            {
              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test;
            }

            // ---------------------------------------------
            // 01/20/99 W.Campbell - Inserted logic to allow
            // for update of ADABAS person information
            // (DOB and SSN) on a CREATE.  Have to validate
            // the DOB.
            // ---------------------------------------------
            // ---------------------------------------------
            // Validate the Mother's Date of Birth
            // ---------------------------------------------
            if (!Lt(import.MotherCsePersonsWorkSet.Dob, local.Current.Date))
            {
              var field = GetField(export.MotherCsePersonsWorkSet, "dob");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_DATE_OF_BIRTH";
              }
            }

            // ---------------------------------------------
            // Validate the Mother's Date of Death
            // ---------------------------------------------
            if (!Lt(export.MotherCsePerson.DateOfDeath, local.Current.Date))
            {
              var field = GetField(export.MotherCsePerson, "dateOfDeath");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_DATE_OF_DEATH";
              }
            }

            if (IsEmpty(import.MotherCsePersonAddress.Street1) || IsEmpty
              (import.MotherCsePersonAddress.City) || IsEmpty
              (import.MotherCsePersonAddress.State) || IsEmpty
              (import.MotherCsePersonAddress.ZipCode))
            {
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ADDRESS_INCOMPLETE";
              }

              var field1 = GetField(export.MotherCsePersonAddress, "street1");

              field1.Error = true;

              var field2 = GetField(export.MotherCsePersonAddress, "street2");

              field2.Error = true;

              var field3 = GetField(export.MotherCsePersonAddress, "city");

              field3.Error = true;

              var field4 = GetField(export.MotherCsePersonAddress, "state");

              field4.Error = true;

              var field5 = GetField(export.MotherCsePersonAddress, "zipCode");

              field5.Error = true;

              var field6 = GetField(export.MotherCsePersonAddress, "zip4");

              field6.Error = true;
            }
            else
            {
              // ---------------------------------------------
              // Validate the State
              // ---------------------------------------------
              local.Code.CodeName = "STATE CODE";
              local.CodeValue.Cdvalue = import.MotherCsePersonAddress.State ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.Invalid.Flag) == 'N')
              {
                var field = GetField(export.MotherCsePersonAddress, "state");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_INVALID_STATE_CODE";
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for the state of KS and
              // placed the county logic inside it.
              // ---------------------------------------------
              if (Equal(export.MotherCsePersonAddress.State, "KS"))
              {
                if (IsEmpty(import.MotherCsePersonAddress.County))
                {
                  UseEabReturnKsCountyByZip1();
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // logic to validate the ZIP CODE and ZIP4 for numeric.
              // ---------------------------------------------
              // ---------------------------------------------
              // PR#209846  06/01/07 L.Smith - Added to the ZIP CODE and ZIP4
              // logic to also validtae ZIP CODE to not be less than
              // 5 digits and ZIP4 to not be less than 4 digits or be left 
              // blank.
              // ---------------------------------------------
              if (Length(TrimEnd(export.MotherCsePersonAddress.ZipCode)) < 5)
              {
                var field = GetField(export.MotherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";
                }
              }
              else if (Verify(export.MotherCsePersonAddress.ZipCode,
                "0123456789") != 0)
              {
                var field = GetField(export.MotherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }

              if (Length(TrimEnd(export.MotherCsePersonAddress.ZipCode)) > 0
                && Length(TrimEnd(export.MotherCsePersonAddress.Zip4)) > 0)
              {
                if (Length(TrimEnd(import.MotherCsePersonAddress.Zip4)) < 4)
                {
                  var field = GetField(export.MotherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
                  }
                }
                else if (Verify(export.MotherCsePersonAddress.Zip4, "0123456789")
                  != 0)
                {
                  var field = GetField(export.MotherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                  }
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for KS county and modified
              // some of the logic which was moved inside
              // the IF statement.
              // ---------------------------------------------
              if (Equal(export.MotherCsePersonAddress.State, "KS"))
              {
                if (!IsEmpty(export.MotherCsePersonAddress.County))
                {
                  // ---------------------------------------------
                  // Validate the County, if the state is Kansas.
                  // ---------------------------------------------
                  local.Code.CodeName = "COUNTY CODE";

                  // ---------------------------------------------
                  // 01/14/99 W.Campbell - Changed the following set stmt
                  // to use export_mother instead of import_mother.
                  // ---------------------------------------------
                  local.CodeValue.Cdvalue =
                    export.MotherCsePersonAddress.County ?? Spaces(10);
                  UseCabValidateCodeValue();

                  if (AsChar(local.Invalid.Flag) == 'N')
                  {
                    var field =
                      GetField(export.MotherCsePersonAddress, "county");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "INVALID_COUNTY_CODE";
                    }
                  }
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (!IsEmpty(import.MotherCsePersonsWorkSet.Number))
                {
                  local.TextWorkArea.Text10 =
                    import.MotherCsePersonsWorkSet.Number;
                  UseEabPadLeftWithZeros();
                  export.MotherCsePersonsWorkSet.Number =
                    local.TextWorkArea.Text10;
                }

                MoveCsePersonAddress1(export.MotherCsePersonAddress,
                  local.CsePersonAddress);
                local.CsePersonAddress.Type1 = "M";
                local.CsePersonAddress.LocationType = "D";
                local.CsePerson.Number = export.MotherCsePersonsWorkSet.Number;

                // ---------------------------------------------
                // 01/13/99 W.Campbell - Removed set statement
                // setting CSE_PERSON_ADDRESS zdel_start_date
                // to CURRENT_DATE.
                // ---------------------------------------------
                local.CsePersonAddress.EndDate =
                  UseCabSetMaximumDiscontinueDate();

                // ---------------------------------------------
                // 01/16/99 W.Campbell - Inserted the following
                // set statement to set the VERIFIED_DATE
                // to local_current date.
                // ---------------------------------------------
                local.CsePersonAddress.VerifiedDate = local.Current.Date;

                if (AsChar(export.MoHiddenAe.Flag) != 'O')
                {
                  UseSiCreateCsePersonAddress1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // ---------------------------------------------
                    // 01/22/99 W.Campbell - Inserted logic to
                    // USE EAB_ROLLBACK_CICS
                    // to handle errors from all
                    // CREATE or UPDATE CABS.
                    // ---------------------------------------------
                    UseEabRollbackCics();

                    goto Test;
                  }
                }

                // ---------------------------------------------
                // 01/22/99 W.Campbell - Disabled logic to
                // prevent the update of CASE_ROLE
                // in the CREATE logic for both
                // mother and father.
                // ---------------------------------------------
                UseSiUpdateCaseRole3();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // ---------------------------------------------
                  // 01/22/99 W.Campbell - Inserted logic to
                  // USE EAB_ROLLBACK_CICS
                  // to handle errors from all
                  // CREATE or UPDATE CABS.
                  // ---------------------------------------------
                  UseEabRollbackCics();

                  goto Test;
                }

                // ---------------------------------------------
                // 01/20/99 W.Campbell - Inserted logic to allow
                // for update of ADABAS person information
                // (DOB and SSN) on a CREATE.
                // ---------------------------------------------
                if (AsChar(export.MoHiddenAe.Flag) != 'O')
                {
                  UseCabUpdateAdabasPerson1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // ---------------------------------------------
                    // 01/22/99 W.Campbell - Inserted logic to
                    // USE EAB_ROLLBACK_CICS
                    // to handle errors from all
                    // CREATE or UPDATE CABS.
                    // ---------------------------------------------
                    UseEabRollbackCics();

                    goto Test;
                  }
                }

                export.MotherCommon.SelectChar = "";
              }
            }

            break;
          case ' ':
            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ---------------------------------------------
        // Validate and add the father's address
        // ---------------------------------------------
        switch(AsChar(import.FatherCommon.SelectChar))
        {
          case 'S':
            if (AsChar(export.FaHiddenAe.Flag) == 'O' && AsChar
              (export.FaHiddenOtherCr.Flag) == 'Y')
            {
              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test;
            }

            // ---------------------------------------------
            // 01/20/99 W.Campbell - Inserted logic to allow
            // for update of ADABAS person information
            // (DOB and SSN) on a CREATE.  Have to validate
            // the DOB.
            // ---------------------------------------------
            // ---------------------------------------------
            // Validate the Father's Date of Birth
            // ---------------------------------------------
            if (!Lt(import.FatherCsePersonsWorkSet.Dob, local.Current.Date))
            {
              var field = GetField(export.FatherCsePersonsWorkSet, "dob");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_DATE_OF_BIRTH";
              }
            }

            if (IsEmpty(import.FatherCsePersonAddress.Street1) || IsEmpty
              (import.FatherCsePersonAddress.City) || IsEmpty
              (import.FatherCsePersonAddress.State) || IsEmpty
              (import.FatherCsePersonAddress.ZipCode))
            {
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ADDRESS_INCOMPLETE";
              }

              var field1 = GetField(export.FatherCsePersonAddress, "street1");

              field1.Error = true;

              var field2 = GetField(export.FatherCsePersonAddress, "street2");

              field2.Error = true;

              var field3 = GetField(export.FatherCsePersonAddress, "city");

              field3.Error = true;

              var field4 = GetField(export.FatherCsePersonAddress, "state");

              field4.Error = true;

              var field5 = GetField(export.FatherCsePersonAddress, "zipCode");

              field5.Error = true;

              var field6 = GetField(export.FatherCsePersonAddress, "zip4");

              field6.Error = true;
            }
            else
            {
              // ---------------------------------------------
              // Validate the State
              // ---------------------------------------------
              local.Code.CodeName = "STATE CODE";
              local.CodeValue.Cdvalue = import.FatherCsePersonAddress.State ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.Invalid.Flag) == 'N')
              {
                var field = GetField(export.FatherCsePersonAddress, "state");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_INVALID_STATE_CODE";
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for the state of KS and
              // placed the county logic inside it.
              // ---------------------------------------------
              if (Equal(export.FatherCsePersonAddress.State, "KS"))
              {
                if (IsEmpty(import.FatherCsePersonAddress.County))
                {
                  UseEabReturnKsCountyByZip2();
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // logic to validate the ZIP CODE and ZIP4 for numeric.
              // ---------------------------------------------
              // ---------------------------------------------
              // PR#209846  06/01/07 L.Smith - Added to the ZIP CODE and ZIP4
              // logic to also validtae ZIP CODE to not be less than
              // 5 digits and ZIP4 to not be less than 4 digits or be left 
              // blank.
              // ---------------------------------------------
              if (Length(TrimEnd(export.FatherCsePersonAddress.ZipCode)) < 5)
              {
                var field = GetField(export.FatherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";
                }
              }
              else if (Verify(export.FatherCsePersonAddress.ZipCode,
                "0123456789") != 0)
              {
                var field = GetField(export.FatherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }

              if (Length(TrimEnd(export.FatherCsePersonAddress.ZipCode)) > 0
                && Length(TrimEnd(export.FatherCsePersonAddress.Zip4)) > 0)
              {
                if (Length(TrimEnd(export.FatherCsePersonAddress.Zip4)) < 4)
                {
                  var field = GetField(export.FatherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
                  }
                }
                else if (Verify(export.FatherCsePersonAddress.Zip4, "0123456789")
                  != 0)
                {
                  var field = GetField(export.FatherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                  }
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for KS county and modified
              // some of the logic which was moved inside
              // the IF statement.
              // ---------------------------------------------
              if (Equal(export.FatherCsePersonAddress.State, "KS"))
              {
                if (!IsEmpty(export.FatherCsePersonAddress.County))
                {
                  // ---------------------------------------------
                  // Validate the County, if the state is Kansas.
                  // ---------------------------------------------
                  local.Code.CodeName = "COUNTY CODE";

                  // ---------------------------------------------
                  // 01/14/99 W.Campbell - Changed the following set stmt
                  // to use export_father instead of import_father.
                  // ---------------------------------------------
                  local.CodeValue.Cdvalue =
                    export.FatherCsePersonAddress.County ?? Spaces(10);
                  UseCabValidateCodeValue();

                  if (AsChar(local.Invalid.Flag) == 'N')
                  {
                    var field =
                      GetField(export.FatherCsePersonAddress, "county");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "INVALID_COUNTY_CODE";
                    }
                  }
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (!IsEmpty(import.FatherCsePersonsWorkSet.Number))
                {
                  local.TextWorkArea.Text10 =
                    import.FatherCsePersonsWorkSet.Number;
                  UseEabPadLeftWithZeros();
                  export.FatherCsePersonsWorkSet.Number =
                    local.TextWorkArea.Text10;
                }

                MoveCsePersonAddress1(export.FatherCsePersonAddress,
                  local.CsePersonAddress);
                local.CsePersonAddress.Type1 = "M";
                local.CsePersonAddress.LocationType = "D";
                local.CsePerson.Number = export.FatherCsePersonsWorkSet.Number;

                // ---------------------------------------------
                // 01/13/99 W.Campbell - Removed set statement
                // setting CSE_PERSON_ADDRESS zdel_start_date
                // to CURRENT_DATE.
                // ---------------------------------------------
                local.CsePersonAddress.EndDate =
                  UseCabSetMaximumDiscontinueDate();

                // ---------------------------------------------
                // 01/16/99 W.Campbell - Inserted the following
                // set statement to set the VERIFIED_DATE
                // to local_current date.
                // ---------------------------------------------
                local.CsePersonAddress.VerifiedDate = local.Current.Date;

                if (AsChar(export.FaHiddenAe.Flag) != 'O')
                {
                  UseSiCreateCsePersonAddress2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // ---------------------------------------------
                    // 01/22/99 W.Campbell - Inserted logic to
                    // USE EAB_ROLLBACK_CICS
                    // to handle errors from all
                    // CREATE or UPDATE CABS.
                    // ---------------------------------------------
                    UseEabRollbackCics();

                    goto Test;
                  }
                }

                // ---------------------------------------------
                // 01/22/99 W.Campbell - Disabled logic to
                // prevent the update of CASE_ROLE
                // in the CREATE logic for both
                // mother and father.
                // ---------------------------------------------
                UseSiUpdateCaseRole4();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // ---------------------------------------------
                  // 01/22/99 W.Campbell - Inserted logic to
                  // USE EAB_ROLLBACK_CICS
                  // to handle errors from all
                  // CREATE or UPDATE CABS.
                  // ---------------------------------------------
                  UseEabRollbackCics();

                  goto Test;
                }

                // ---------------------------------------------
                // 01/20/99 W.Campbell - Inserted logic to allow
                // for update of ADABAS person information
                // (DOB and SSN) on a CREATE.
                // ---------------------------------------------
                if (AsChar(export.FaHiddenAe.Flag) != 'O')
                {
                  UseCabUpdateAdabasPerson2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // ---------------------------------------------
                    // 01/22/99 W.Campbell - Inserted logic to
                    // USE EAB_ROLLBACK_CICS
                    // to handle errors from all
                    // CREATE or UPDATE CABS.
                    // ---------------------------------------------
                    UseEabRollbackCics();

                    goto Test;
                  }
                }

                export.FatherCommon.SelectChar = "";
              }
            }

            break;
          case ' ':
            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        switch(AsChar(export.MotherCommon.SelectChar))
        {
          case 'S':
            if (IsEmpty(export.MotherCsePersonsWorkSet.Number))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

              return;
            }

            // ---------------------------------------------
            // Validate the Mother's Date of Birth
            // ---------------------------------------------
            if (!Lt(import.MotherCsePersonsWorkSet.Dob, local.Current.Date))
            {
              var field = GetField(export.MotherCsePersonsWorkSet, "dob");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_DATE_OF_BIRTH";
              }
            }

            // ---------------------------------------------
            // Validate the Mother's Date of Death
            // ---------------------------------------------
            if (!Lt(export.MotherCsePerson.DateOfDeath, local.Current.Date))
            {
              var field = GetField(export.MotherCsePerson, "dateOfDeath");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_DATE_OF_DEATH";
              }
            }

            // ---------------------------------------------
            // Verify that a display has been performed
            // before the update can take place.
            // ---------------------------------------------
            if (!Equal(import.MotherCsePersonAddress.Identifier,
              import.HiddenPrevMother.Identifier))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

              return;
            }

            // ---------------------------------------------
            // Validate and update the mother's address
            // ---------------------------------------------
            if (!Equal(import.MotherCsePersonAddress.Identifier,
              local.Null1.Identifier))
            {
              if (IsEmpty(import.MotherCsePersonAddress.Street1))
              {
                var field = GetField(export.MotherCsePersonAddress, "street1");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ADDRESS_INCOMPLETE";
                }
              }

              if (IsEmpty(import.MotherCsePersonAddress.City))
              {
                var field = GetField(export.MotherCsePersonAddress, "city");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ADDRESS_INCOMPLETE";
                }
              }

              if (IsEmpty(import.MotherCsePersonAddress.State))
              {
                var field = GetField(export.MotherCsePersonAddress, "state");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ADDRESS_INCOMPLETE";
                }
              }
              else
              {
                // ---------------------------------------------
                // Validate the State
                // ---------------------------------------------
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue =
                  import.MotherCsePersonAddress.State ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.Invalid.Flag) == 'N')
                {
                  var field = GetField(export.MotherCsePersonAddress, "state");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_STATE_CODE";
                  }
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for the state of KS and
              // placed the county logic inside it.
              // ---------------------------------------------
              if (Equal(export.MotherCsePersonAddress.State, "KS"))
              {
                if (IsEmpty(import.MotherCsePersonAddress.County))
                {
                  UseEabReturnKsCountyByZip1();
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // logic to validate the ZIP CODE and ZIP4 for numeric.
              // ---------------------------------------------
              // ---------------------------------------------
              // PR#209846  06/01/07 L.Smith - Added to the ZIP CODE and ZIP4
              // logic to also validtae ZIP CODE to not be less than
              // 5 digits and ZIP4 to not be less than 4 digits or be left 
              // blank.
              // ---------------------------------------------
              if (Length(TrimEnd(export.MotherCsePersonAddress.ZipCode)) < 5)
              {
                var field = GetField(export.MotherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";
                }
              }
              else if (Verify(export.MotherCsePersonAddress.ZipCode,
                "0123456789") != 0)
              {
                var field = GetField(export.MotherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }

              if (Length(TrimEnd(export.MotherCsePersonAddress.ZipCode)) > 0
                && Length(TrimEnd(export.MotherCsePersonAddress.Zip4)) > 0)
              {
                if (Length(TrimEnd(export.MotherCsePersonAddress.Zip4)) < 4)
                {
                  var field = GetField(export.MotherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
                  }
                }
                else if (Verify(export.MotherCsePersonAddress.Zip4, "0123456789")
                  != 0)
                {
                  var field = GetField(export.MotherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                  }
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for KS county and modified
              // some of the logic which was moved inside
              // the IF statement.
              // ---------------------------------------------
              if (Equal(export.MotherCsePersonAddress.State, "KS"))
              {
                if (!IsEmpty(export.MotherCsePersonAddress.County))
                {
                  // ---------------------------------------------
                  // Validate the County, if the state is Kansas.
                  // ---------------------------------------------
                  local.Code.CodeName = "COUNTY CODE";
                  local.CodeValue.Cdvalue =
                    export.MotherCsePersonAddress.County ?? Spaces(10);
                  UseCabValidateCodeValue();

                  if (AsChar(local.Invalid.Flag) == 'N')
                  {
                    var field =
                      GetField(export.MotherCsePersonAddress, "county");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "INVALID_COUNTY_CODE";
                    }
                  }
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.MotherAddressUpd.Flag = "Y";
              }
            }

            break;
          case ' ':
            break;
          default:
            break;
        }

        switch(AsChar(export.FatherCommon.SelectChar))
        {
          case 'S':
            if (IsEmpty(export.FatherCsePersonsWorkSet.Number))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

              return;
            }

            // ---------------------------------------------
            // Validate the Father's Date of Birth
            // ---------------------------------------------
            if (!Lt(import.FatherCsePersonsWorkSet.Dob, local.Current.Date))
            {
              var field = GetField(export.FatherCsePersonsWorkSet, "dob");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_DATE_OF_BIRTH";
              }
            }

            // ---------------------------------------------
            // Verify that a display has been performed
            // before the update can take place.
            // ---------------------------------------------
            if (!Equal(import.FatherCsePersonAddress.Identifier,
              import.HiddenPrevFather.Identifier))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

              return;
            }

            // ---------------------------------------------
            // Validate and update the father's address
            // ---------------------------------------------
            if (!Equal(import.FatherCsePersonAddress.Identifier,
              local.Null1.Identifier))
            {
              if (IsEmpty(import.FatherCsePersonAddress.Street1))
              {
                var field = GetField(export.FatherCsePersonAddress, "street1");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ADDRESS_INCOMPLETE";
                }
              }

              if (IsEmpty(import.FatherCsePersonAddress.City))
              {
                var field = GetField(export.FatherCsePersonAddress, "city");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ADDRESS_INCOMPLETE";
                }
              }

              if (IsEmpty(import.FatherCsePersonAddress.State))
              {
                var field = GetField(export.FatherCsePersonAddress, "state");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ADDRESS_INCOMPLETE";
                }
              }
              else
              {
                // ---------------------------------------------
                // Validate the State
                // ---------------------------------------------
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue =
                  import.FatherCsePersonAddress.State ?? Spaces(10);
                UseCabValidateCodeValue();

                if (AsChar(local.Invalid.Flag) == 'N')
                {
                  var field = GetField(export.FatherCsePersonAddress, "state");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_STATE_CODE";
                  }
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for the state of KS and
              // placed the county logic inside it.
              // ---------------------------------------------
              if (Equal(export.FatherCsePersonAddress.State, "KS"))
              {
                if (IsEmpty(import.FatherCsePersonAddress.County))
                {
                  UseEabReturnKsCountyByZip2();
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // logic to validate the ZIP CODE and ZIP4 for numeric.
              // ---------------------------------------------
              // ---------------------------------------------
              // PR#209846  06/01/07 L.Smith - Added to the ZIP CODE and ZIP4
              // logic to also validtae ZIP CODE to not be less than
              // 5 digits and ZIP4 to not be less than 4 digits or be left 
              // blank.
              // ---------------------------------------------
              if (Length(TrimEnd(export.FatherCsePersonAddress.ZipCode)) < 5)
              {
                var field = GetField(export.FatherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";
                }
              }
              else if (Verify(export.FatherCsePersonAddress.ZipCode,
                "0123456789") != 0)
              {
                var field = GetField(export.FatherCsePersonAddress, "zipCode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }

              if (Length(TrimEnd(export.FatherCsePersonAddress.ZipCode)) > 0
                && Length(TrimEnd(export.FatherCsePersonAddress.Zip4)) > 0)
              {
                if (Length(TrimEnd(export.FatherCsePersonAddress.Zip4)) < 4)
                {
                  var field = GetField(export.FatherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
                  }
                }
                else if (Verify(export.FatherCsePersonAddress.Zip4, "0123456789")
                  != 0)
                {
                  var field = GetField(export.FatherCsePersonAddress, "zip4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                  }
                }
              }

              // ---------------------------------------------
              // 01/16/99 W.Campbell - Inserted the following
              // IF statement to check for KS county and modified
              // some of the logic which was moved inside
              // the IF statement.
              // ---------------------------------------------
              if (Equal(export.FatherCsePersonAddress.State, "KS"))
              {
                if (!IsEmpty(export.FatherCsePersonAddress.County))
                {
                  // ---------------------------------------------
                  // Validate the County, if the state is Kansas.
                  // ---------------------------------------------
                  local.Code.CodeName = "COUNTY CODE";
                  local.CodeValue.Cdvalue =
                    export.FatherCsePersonAddress.County ?? Spaces(10);
                  UseCabValidateCodeValue();

                  if (AsChar(local.Invalid.Flag) == 'N')
                  {
                    var field =
                      GetField(export.FatherCsePersonAddress, "county");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "INVALID_COUNTY_CODE";
                    }
                  }
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.FatherAddressUpd.Flag = "Y";
              }
            }

            break;
          case ' ':
            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(import.MotherCommon.SelectChar) == 'S')
        {
          local.CsePerson.Number = export.MotherCsePersonsWorkSet.Number;

          if (AsChar(local.MotherAddressUpd.Flag) == 'Y')
          {
            // ---------------------------------------------
            // Update the mother's address
            // --------------------------------------------
            UseSiUpdateCsePersonAddress1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ---------------------------------------------
              // 01/22/99 W.Campbell - Inserted logic to
              // USE EAB_ROLLBACK_CICS
              // to handle errors from all
              // CREATE or UPDATE CABS.
              // ---------------------------------------------
              UseEabRollbackCics();

              break;
            }
          }

          UseSiUpdateCaseRole1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ---------------------------------------------
            // 01/22/99 W.Campbell - Inserted logic to
            // USE EAB_ROLLBACK_CICS
            // to handle errors from all
            // CREATE or UPDATE CABS.
            // ---------------------------------------------
            UseEabRollbackCics();

            break;
          }

          if (AsChar(export.MoHiddenAe.Flag) != 'O')
          {
            UseCabUpdateAdabasPerson1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ---------------------------------------------
              // 01/22/99 W.Campbell - Inserted logic to
              // USE EAB_ROLLBACK_CICS
              // to handle errors from all
              // CREATE or UPDATE CABS.
              // ---------------------------------------------
              UseEabRollbackCics();

              break;
            }
          }

          export.MotherCommon.SelectChar = "";
        }

        if (AsChar(import.FatherCommon.SelectChar) == 'S')
        {
          local.CsePerson.Number = import.FatherCsePersonsWorkSet.Number;

          if (AsChar(local.FatherAddressUpd.Flag) == 'Y')
          {
            // ---------------------------------------------
            // Update the father's address
            // --------------------------------------------
            UseSiUpdateCsePersonAddress2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ---------------------------------------------
              // 01/22/99 W.Campbell - Inserted logic to
              // USE EAB_ROLLBACK_CICS
              // to handle errors from all
              // CREATE or UPDATE CABS.
              // ---------------------------------------------
              UseEabRollbackCics();

              break;
            }
          }

          UseSiUpdateCaseRole2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ---------------------------------------------
            // 01/22/99 W.Campbell - Inserted logic to
            // USE EAB_ROLLBACK_CICS
            // to handle errors from all
            // CREATE or UPDATE CABS.
            // ---------------------------------------------
            UseEabRollbackCics();

            break;
          }

          if (AsChar(export.FaHiddenAe.Flag) != 'O')
          {
            UseCabUpdateAdabasPerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ---------------------------------------------
              // 01/22/99 W.Campbell - Inserted logic to
              // USE EAB_ROLLBACK_CICS
              // to handle errors from all
              // CREATE or UPDATE CABS.
              // ---------------------------------------------
              UseEabRollbackCics();

              break;
            }
          }

          export.FatherCommon.SelectChar = "";
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DISPLAY":
        export.FatherCommon.SelectChar = "";
        export.MotherCommon.SelectChar = "";
        export.ApPrompt.SelectChar = "";
        export.MoCntyPrompt.SelectChar = "";
        export.MoStatePrompt.SelectChar = "";
        export.FaCntyPrompt.SelectChar = "";
        export.FaStatePrompt.SelectChar = "";

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
          export.Ap.Number = "";
        }

        if (!IsEmpty(export.Ap.Number))
        {
          local.TextWorkArea.Text10 = export.Ap.Number;
          UseEabPadLeftWithZeros();
          export.Ap.Number = local.TextWorkArea.Text10;
        }

        UseSiReadCaseHeaderInformation();

        // *** If no AP is found on the case this is okay.  Continue processing.
        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.Hidden.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        UseSiReadOfficeOspHeader();

        // *** If no AP is found on the case this is okay.  Continue processing.
        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        // ***		Read Mother Details		***
        local.CaseRole.Type1 = "MO";
        UseSiPadsReadParentalCaseRole2();

        // Read to determine if Mother is deceased.
        if (ReadCsePerson())
        {
          export.MotherCsePerson.DateOfDeath = entities.Mother.DateOfDeath;
        }

        // *** If no AP is found on the case this is okay.  Continue processing.
        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        if (!IsEmpty(export.MotherCsePersonsWorkSet.Ssn))
        {
          local.SsnWorkArea.SsnText9 = export.MotherCsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum1();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("NO_APS_ON_A_CASE"))
          {
          }
          else
          {
            export.MotherSsnWorkArea.SsnNumPart1 = 0;
            export.MotherSsnWorkArea.SsnNumPart2 = 0;
            export.MotherSsnWorkArea.SsnNumPart3 = 0;
          }
        }
        else
        {
          export.MotherSsnWorkArea.SsnText9 = "";
          export.MotherSsnWorkArea.SsnNumPart1 = 0;
          export.MotherSsnWorkArea.SsnNumPart2 = 0;
          export.MotherSsnWorkArea.SsnNumPart3 = 0;
        }

        if (IsEmpty(export.MotherCsePersonsWorkSet.Number) || AsChar
          (export.CaseOpen.Flag) == 'N')
        {
          var field1 = GetField(export.MotherCommon, "selectChar");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.MotherCsePersonsWorkSet, "lastName");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.MotherCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 =
            GetField(export.MotherCsePersonsWorkSet, "middleInitial");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.MotherCsePersonAddress, "street1");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.MotherCsePersonAddress, "street2");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.MotherCsePersonAddress, "city");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.MotherCsePersonAddress, "state");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.MotherCsePersonAddress, "zipCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.MotherCsePersonAddress, "zip4");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.MotherCsePersonAddress, "county");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.MotherCsePersonsWorkSet, "dob");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.MotherCsePerson, "dateOfDeath");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.MotherCsePersonsWorkSet, "number");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 = GetField(export.MotherCaseRole, "note");

          field18.Color = "cyan";
          field18.Protected = true;

          var field19 = GetField(export.MoStatePrompt, "selectChar");

          field19.Color = "cyan";
          field19.Protected = true;

          var field20 = GetField(export.MoCntyPrompt, "selectChar");

          field20.Color = "cyan";
          field20.Protected = true;
        }

        if (AsChar(export.MoHiddenAe.Flag) == 'O')
        {
          var field1 = GetField(export.MotherCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.MotherCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.MotherCsePerson, "dateOfDeath");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.MotherCsePersonsWorkSet, "firstName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.MotherCsePersonsWorkSet, "lastName");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.MotherCsePersonsWorkSet, "middleInitial");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;
        }

        // ***		Read Father Details		***
        local.CaseRole.Type1 = "FA";
        UseSiPadsReadParentalCaseRole1();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("PARENT_IS_AR_OR_AP") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        if (!IsEmpty(export.FatherCsePersonsWorkSet.Ssn))
        {
          local.SsnWorkArea.SsnText9 = export.FatherCsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum2();

          // ------------------------------------------------------------
          // 01/25/99 W.Campbell - Changed the
          // following IF statement to include
          // no_aps_on_a_case in order to get
          // the SSN displayed on the screen
          // for a father.
          // ------------------------------------------------------------
          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("PARENT_IS_AR_OR_AP") || IsExitState("NO_APS_ON_A_CASE"))
          {
          }
          else
          {
            export.FatherSsnWorkArea.SsnNumPart1 = 0;
            export.FatherSsnWorkArea.SsnNumPart2 = 0;
            export.FatherSsnWorkArea.SsnNumPart3 = 0;
          }
        }
        else
        {
          export.FatherSsnWorkArea.SsnText9 = "";
          export.FatherSsnWorkArea.SsnNumPart1 = 0;
          export.FatherSsnWorkArea.SsnNumPart2 = 0;
          export.FatherSsnWorkArea.SsnNumPart3 = 0;
        }

        if (IsEmpty(export.FatherCsePersonsWorkSet.Number) || AsChar
          (export.CaseOpen.Flag) == 'N')
        {
          var field1 = GetField(export.FatherCommon, "selectChar");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.FatherCsePersonsWorkSet, "firstName");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.FatherCsePersonsWorkSet, "lastName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 =
            GetField(export.FatherCsePersonsWorkSet, "middleInitial");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.FatherCsePersonAddress, "street1");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.FatherCsePersonAddress, "street2");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.FatherCsePersonAddress, "city");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.FatherCsePersonAddress, "state");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.FatherCsePersonAddress, "zipCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.FatherCsePersonAddress, "zip4");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.FatherCsePersonAddress, "county");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.FatherCsePersonsWorkSet, "dob");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.FatherCsePersonsWorkSet, "number");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.FatherCaseRole, "note");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 = GetField(export.FaStatePrompt, "selectChar");

          field18.Color = "cyan";
          field18.Protected = true;

          var field19 = GetField(export.FaCntyPrompt, "selectChar");

          field19.Color = "cyan";
          field19.Protected = true;
        }

        if (AsChar(export.FaHiddenAe.Flag) == 'O')
        {
          var field1 = GetField(export.FatherCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.FatherCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.FatherCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.FatherCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.FatherCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

          field8.Color = "cyan";
          field8.Protected = true;
        }

        // ---------------------------------------------
        // Check whether the Mother is an active AR or AP
        // on any case.  Protect the address information
        // and output an informational message.
        // ---------------------------------------------
        local.CsePerson.Number = export.MotherCsePersonsWorkSet.Number;
        UseSiPadsValidateParentRole2();

        if (AsChar(export.MoHiddenOtherCr.Flag) == 'Y')
        {
          var field1 = GetField(export.MotherCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.MotherCsePerson, "dateOfDeath");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.MotherCsePersonsWorkSet, "dob");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.MotherCsePersonsWorkSet, "firstName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.MotherCsePersonsWorkSet, "lastName");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.MotherCsePersonsWorkSet, "middleInitial");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.MotherCsePersonAddress, "street1");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.MotherCsePersonAddress, "street2");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.MotherCsePersonAddress, "city");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.MotherCsePersonAddress, "state");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.MotherCsePersonAddress, "zipCode");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.MotherCsePersonAddress, "zip4");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.MotherCsePersonAddress, "county");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.MoStatePrompt, "selectChar");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 = GetField(export.MoCntyPrompt, "selectChar");

          field18.Color = "cyan";
          field18.Protected = true;

          ExitState = "PARENT_IS_AR_OR_AP";
        }

        // ---------------------------------------------
        // Check whether the Father is an active AR or AP
        // on any case.  Protect the address information
        // and output an informational message.
        // ---------------------------------------------
        local.CsePerson.Number = export.FatherCsePersonsWorkSet.Number;
        UseSiPadsValidateParentRole1();

        if (AsChar(export.FaHiddenOtherCr.Flag) == 'Y')
        {
          var field1 = GetField(export.FatherCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.FatherCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.FatherCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.FatherCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.FatherCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.FatherCsePersonAddress, "street1");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.FatherCsePersonAddress, "street2");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.FatherCsePersonAddress, "city");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.FatherCsePersonAddress, "state");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.FatherCsePersonAddress, "zipCode");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.FatherCsePersonAddress, "zip4");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.FatherCsePersonAddress, "county");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.FaStatePrompt, "selectChar");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.FaCntyPrompt, "selectChar");

          field17.Color = "cyan";
          field17.Protected = true;

          ExitState = "PARENT_IS_AR_OR_AP";
        }

        // ************************************************************
        // CHECK IF THE MOTHER IS AN AP/AR ON ANY OTHER CASE
        // ************************************************************
        local.CsePerson.Number = export.MotherCsePersonsWorkSet.Number;
        UseSiPadsValidateRoleOtherCase1();

        if (AsChar(export.MoHiddenOtherCase.Flag) == 'Y')
        {
          var field1 = GetField(export.MotherCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.MotherCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.MotherCsePerson, "dateOfDeath");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.MotherCsePersonsWorkSet, "firstName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.MotherCsePersonsWorkSet, "lastName");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.MotherCsePersonsWorkSet, "middleInitial");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.MotherCsePersonAddress, "street1");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.MotherCsePersonAddress, "street2");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.MotherCsePersonAddress, "city");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.MotherCsePersonAddress, "state");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.MotherCsePersonAddress, "zipCode");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.MotherCsePersonAddress, "zip4");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.MotherCsePersonAddress, "county");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.MoStatePrompt, "selectChar");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 = GetField(export.MoCntyPrompt, "selectChar");

          field18.Color = "cyan";
          field18.Protected = true;

          ExitState = "PARENT_IS_AR_OR_AP";
        }

        // ************************************************************
        // CHECK IF THE FATHER IS AN AP/AF ON ANY OTHER CASE
        // ************************************************************
        local.CsePerson.Number = export.FatherCsePersonsWorkSet.Number;
        UseSiPadsValidateRoleOtherCase2();

        if (AsChar(export.FaHiddenOtherCase.Flag) == 'Y')
        {
          var field1 = GetField(export.FatherCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.FatherCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.FatherCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.FatherCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.FatherCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.FatherCsePersonAddress, "street1");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.FatherCsePersonAddress, "street2");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.FatherCsePersonAddress, "city");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.FatherCsePersonAddress, "state");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.FatherCsePersonAddress, "zipCode");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.FatherCsePersonAddress, "zip4");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.FatherCsePersonAddress, "county");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.FaStatePrompt, "selectChar");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.FaCntyPrompt, "selectChar");

          field17.Color = "cyan";
          field17.Protected = true;

          ExitState = "PARENT_IS_AR_OR_AP";
        }

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("PARENT_IS_AR_OR_AP"))
        {
          // ------------------------------------------------------------------------
          // Now call the Family Violence CAB and pass the FATHER'S data to the 
          // CAB to check if the person  has  family violence Flag set.
          //                                                
          // Vithal(12/03/2001)
          // -----------------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseScSecurityCheckForFv2();

          if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
          {
            export.FatherCsePersonAddress.Assign(local.InitFather);
            local.FatherFvInd.Flag = "Y";
          }

          // ------------------------------------------------------------------------
          // Now call the Family Violence CAB and pass the MOTHER'S data to the 
          // CAB to check if the person  has  family violence Flag set.
          //                                                
          // Vithal(12/03/2001)
          // -----------------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseScSecurityCheckForFv1();

          if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
          {
            export.MotherCsePersonAddress.Assign(local.InitMother);
            local.MotherFvInd.Flag = "Y";
          }

          if (AsChar(local.FatherFvInd.Flag) == 'Y' || AsChar
            (local.MotherFvInd.Flag) == 'Y')
          {
            ExitState = "SC0000_DATA_NOT_DISPLAY_FOR_FV";
            local.FatherFvInd.Flag = "";
            local.MotherFvInd.Flag = "";
          }
        }

        if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
        {
          break;
        }

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }
        else if (IsEmpty(export.FatherCsePersonsWorkSet.Number) && IsEmpty
          (export.MotherCsePersonsWorkSet.Number))
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ------------------------------------------------------------
      // Make sure protected fields stay protected.
      // ------------------------------------------------------------
      if (AsChar(export.MoHiddenAe.Flag) == 'O')
      {
        var field1 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePerson, "dateOfDeath");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonsWorkSet, "number");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field6.Color = "cyan";
        field6.Protected = true;
      }

      if (AsChar(export.MoHiddenOtherCr.Flag) == 'Y')
      {
        var field1 = GetField(export.MotherCsePersonAddress, "street1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePersonAddress, "street2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonAddress, "city");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherCsePersonAddress, "state");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherCsePersonAddress, "zipCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherCsePersonAddress, "zip4");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.MotherCsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.MoStatePrompt, "selectChar");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.MoCntyPrompt, "selectChar");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.MotherCsePerson, "dateOfDeath");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.MotherCsePersonsWorkSet, "number");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field15.Color = "cyan";
        field15.Protected = true;
      }

      if (AsChar(export.MoHiddenOtherCase.Flag) == 'Y')
      {
        var field1 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePerson, "dateOfDeath");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonsWorkSet, "number");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field6.Color = "cyan";
        field6.Protected = true;
      }

      if (IsEmpty(export.MotherCsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.MotherCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.MotherCsePersonsWorkSet, "lastName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.MotherCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.MotherCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.MotherCsePersonAddress, "street1");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.MotherCsePersonAddress, "street2");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.MotherCsePersonAddress, "city");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.MotherCsePersonAddress, "state");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.MotherCsePersonAddress, "zipCode");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.MotherCsePersonAddress, "zip4");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.MotherCsePersonAddress, "county");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.MotherCsePersonsWorkSet, "dob");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.MotherCsePerson, "dateOfDeath");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.MotherCsePersonsWorkSet, "number");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.MotherSsnWorkArea, "ssnNumPart1");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.MotherSsnWorkArea, "ssnNumPart2");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.MotherSsnWorkArea, "ssnNumPart3");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.MotherCaseRole, "note");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.MoStatePrompt, "selectChar");

        field19.Color = "cyan";
        field19.Protected = true;

        var field20 = GetField(export.MoCntyPrompt, "selectChar");

        field20.Color = "cyan";
        field20.Protected = true;
      }

      if (AsChar(export.FaHiddenAe.Flag) == 'O')
      {
        var field1 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "number");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      if (AsChar(export.FaHiddenOtherCr.Flag) == 'Y')
      {
        var field1 = GetField(export.FatherCsePersonAddress, "street1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonAddress, "street2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherCsePersonAddress, "city");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherCsePersonAddress, "state");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherCsePersonAddress, "zipCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.FatherCsePersonAddress, "zip4");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.FatherCsePersonAddress, "county");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.FaStatePrompt, "selectChar");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.FaCntyPrompt, "selectChar");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.FatherCsePersonsWorkSet, "number");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field14.Color = "cyan";
        field14.Protected = true;
      }

      if (AsChar(export.FaHiddenOtherCase.Flag) == 'Y')
      {
        var field1 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "number");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      if (IsEmpty(export.FatherCsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.FatherCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.FatherCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.FatherCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.FatherCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.FatherCsePersonAddress, "street1");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.FatherCsePersonAddress, "street2");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.FatherCsePersonAddress, "city");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.FatherCsePersonAddress, "state");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.FatherCsePersonAddress, "zipCode");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.FatherCsePersonAddress, "zip4");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.FatherCsePersonAddress, "county");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.FatherCsePersonsWorkSet, "dob");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.FatherCsePersonsWorkSet, "number");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.FatherSsnWorkArea, "ssnNumPart1");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.FatherSsnWorkArea, "ssnNumPart2");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.FatherSsnWorkArea, "ssnNumPart3");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.FatherCaseRole, "note");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.FaStatePrompt, "selectChar");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.FaCntyPrompt, "selectChar");

        field19.Color = "cyan";
        field19.Protected = true;
      }
    }

    export.HiddenPrevFather.Identifier =
      export.FatherCsePersonAddress.Identifier;
    export.HiddenPrevMother.Identifier =
      export.MotherCsePersonAddress.Identifier;
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.ConfirmedType = source.ConfirmedType;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.County = source.County;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
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
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabSsnConvertNumToText1()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    export.MotherSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabSsnConvertNumToText2()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    export.FatherSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum1()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.MotherSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum2()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.FatherSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabUpdateAdabasPerson1()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.MotherCsePersonsWorkSet);

    Call(CabUpdateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabUpdateAdabasPerson2()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.FatherCsePersonsWorkSet);

    Call(CabUpdateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Invalid.Flag = useExport.ValidCode.Flag;
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

  private void UseEabReturnKsCountyByZip1()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress4(import.MotherCsePersonAddress,
      useImport.CsePersonAddress);
    MoveCsePersonAddress3(export.MotherCsePersonAddress,
      useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress3(useExport.CsePersonAddress,
      export.MotherCsePersonAddress);
  }

  private void UseEabReturnKsCountyByZip2()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress4(import.FatherCsePersonAddress,
      useImport.CsePersonAddress);
    MoveCsePersonAddress3(export.FatherCsePersonAddress,
      useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress3(useExport.CsePersonAddress,
      export.FatherCsePersonAddress);
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

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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
    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv1()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.MotherCsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv2()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.FatherCsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress1()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    MoveCsePersonAddress1(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress,
      export.MotherCsePersonAddress);
  }

  private void UseSiCreateCsePersonAddress2()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    MoveCsePersonAddress1(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress,
      export.FatherCsePersonAddress);
  }

  private void UseSiPadsReadParentalCaseRole1()
  {
    var useImport = new SiPadsReadParentalCaseRole.Import();
    var useExport = new SiPadsReadParentalCaseRole.Export();

    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiPadsReadParentalCaseRole.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.FaHiddenAe.Flag = useExport.Ae.Flag;
    export.FatherCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.FatherCaseRole.Assign(useExport.CaseRole);
    export.FatherCsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiPadsReadParentalCaseRole2()
  {
    var useImport = new SiPadsReadParentalCaseRole.Import();
    var useExport = new SiPadsReadParentalCaseRole.Export();

    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiPadsReadParentalCaseRole.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.MoHiddenAe.Flag = useExport.Ae.Flag;
    export.MotherCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.MotherCaseRole.Assign(useExport.CaseRole);
    export.MotherCsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiPadsValidateParentRole1()
  {
    var useImport = new SiPadsValidateParentRole.Import();
    var useExport = new SiPadsValidateParentRole.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiPadsValidateParentRole.Execute, useImport, useExport);

    export.FaHiddenOtherCr.Flag = useExport.Common.Flag;
  }

  private void UseSiPadsValidateParentRole2()
  {
    var useImport = new SiPadsValidateParentRole.Import();
    var useExport = new SiPadsValidateParentRole.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiPadsValidateParentRole.Execute, useImport, useExport);

    export.MoHiddenOtherCr.Flag = useExport.Common.Flag;
  }

  private void UseSiPadsValidateRoleOtherCase1()
  {
    var useImport = new SiPadsValidateRoleOtherCase.Import();
    var useExport = new SiPadsValidateRoleOtherCase.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiPadsValidateRoleOtherCase.Execute, useImport, useExport);

    export.MoHiddenOtherCase.Flag = useExport.Common.Flag;
  }

  private void UseSiPadsValidateRoleOtherCase2()
  {
    var useImport = new SiPadsValidateRoleOtherCase.Import();
    var useExport = new SiPadsValidateRoleOtherCase.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiPadsValidateRoleOtherCase.Execute, useImport, useExport);

    export.FaHiddenOtherCase.Flag = useExport.Common.Flag;
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.Ap.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.Ap.Assign(useExport.Ap);
    export.Ar.Assign(useExport.Ar);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
  }

  private void UseSiUpdateCaseRole1()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.Case1.Number = import.Case1.Number;
    MoveCaseRole1(import.MotherCaseRole, useImport.CaseRole);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCaseRole2()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.Case1.Number = import.Case1.Number;
    MoveCaseRole2(import.FatherCaseRole, useImport.CaseRole);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCaseRole3()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    MoveCaseRole1(export.MotherCaseRole, useImport.CaseRole);

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCaseRole4()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;
    MoveCaseRole2(export.FatherCaseRole, useImport.CaseRole);

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePersonAddress1()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress2(export.MotherCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePersonAddress2()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress2(export.FatherCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.Mother.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.MotherCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Mother.Number = db.GetString(reader, 0);
        entities.Mother.Type1 = db.GetString(reader, 1);
        entities.Mother.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Mother.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Mother.Type1);
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
    /// A value of MotherCsePerson.
    /// </summary>
    [JsonPropertyName("motherCsePerson")]
    public CsePerson MotherCsePerson
    {
      get => motherCsePerson ??= new();
      set => motherCsePerson = value;
    }

    /// <summary>
    /// A value of MoHiddenOtherCase.
    /// </summary>
    [JsonPropertyName("moHiddenOtherCase")]
    public Common MoHiddenOtherCase
    {
      get => moHiddenOtherCase ??= new();
      set => moHiddenOtherCase = value;
    }

    /// <summary>
    /// A value of FaHiddenOtherCase.
    /// </summary>
    [JsonPropertyName("faHiddenOtherCase")]
    public Common FaHiddenOtherCase
    {
      get => faHiddenOtherCase ??= new();
      set => faHiddenOtherCase = value;
    }

    /// <summary>
    /// A value of FaHiddenAe.
    /// </summary>
    [JsonPropertyName("faHiddenAe")]
    public Common FaHiddenAe
    {
      get => faHiddenAe ??= new();
      set => faHiddenAe = value;
    }

    /// <summary>
    /// A value of MoHiddenAe.
    /// </summary>
    [JsonPropertyName("moHiddenAe")]
    public Common MoHiddenAe
    {
      get => moHiddenAe ??= new();
      set => moHiddenAe = value;
    }

    /// <summary>
    /// A value of FaHiddenOtherCr.
    /// </summary>
    [JsonPropertyName("faHiddenOtherCr")]
    public Common FaHiddenOtherCr
    {
      get => faHiddenOtherCr ??= new();
      set => faHiddenOtherCr = value;
    }

    /// <summary>
    /// A value of MoHiddenOtherCr.
    /// </summary>
    [JsonPropertyName("moHiddenOtherCr")]
    public Common MoHiddenOtherCr
    {
      get => moHiddenOtherCr ??= new();
      set => moHiddenOtherCr = value;
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
    /// A value of MoStatePrompt.
    /// </summary>
    [JsonPropertyName("moStatePrompt")]
    public Common MoStatePrompt
    {
      get => moStatePrompt ??= new();
      set => moStatePrompt = value;
    }

    /// <summary>
    /// A value of FaStatePrompt.
    /// </summary>
    [JsonPropertyName("faStatePrompt")]
    public Common FaStatePrompt
    {
      get => faStatePrompt ??= new();
      set => faStatePrompt = value;
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
    /// A value of HiddenPrevFather.
    /// </summary>
    [JsonPropertyName("hiddenPrevFather")]
    public CsePersonAddress HiddenPrevFather
    {
      get => hiddenPrevFather ??= new();
      set => hiddenPrevFather = value;
    }

    /// <summary>
    /// A value of HiddenPrevMother.
    /// </summary>
    [JsonPropertyName("hiddenPrevMother")]
    public CsePersonAddress HiddenPrevMother
    {
      get => hiddenPrevMother ??= new();
      set => hiddenPrevMother = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of MotherCommon.
    /// </summary>
    [JsonPropertyName("motherCommon")]
    public Common MotherCommon
    {
      get => motherCommon ??= new();
      set => motherCommon = value;
    }

    /// <summary>
    /// A value of MotherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("motherCsePersonsWorkSet")]
    public CsePersonsWorkSet MotherCsePersonsWorkSet
    {
      get => motherCsePersonsWorkSet ??= new();
      set => motherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MotherCaseRole.
    /// </summary>
    [JsonPropertyName("motherCaseRole")]
    public CaseRole MotherCaseRole
    {
      get => motherCaseRole ??= new();
      set => motherCaseRole = value;
    }

    /// <summary>
    /// A value of MotherCsePersonAddress.
    /// </summary>
    [JsonPropertyName("motherCsePersonAddress")]
    public CsePersonAddress MotherCsePersonAddress
    {
      get => motherCsePersonAddress ??= new();
      set => motherCsePersonAddress = value;
    }

    /// <summary>
    /// A value of FatherCommon.
    /// </summary>
    [JsonPropertyName("fatherCommon")]
    public Common FatherCommon
    {
      get => fatherCommon ??= new();
      set => fatherCommon = value;
    }

    /// <summary>
    /// A value of FatherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fatherCsePersonsWorkSet")]
    public CsePersonsWorkSet FatherCsePersonsWorkSet
    {
      get => fatherCsePersonsWorkSet ??= new();
      set => fatherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FatherCaseRole.
    /// </summary>
    [JsonPropertyName("fatherCaseRole")]
    public CaseRole FatherCaseRole
    {
      get => fatherCaseRole ??= new();
      set => fatherCaseRole = value;
    }

    /// <summary>
    /// A value of FatherCsePersonAddress.
    /// </summary>
    [JsonPropertyName("fatherCsePersonAddress")]
    public CsePersonAddress FatherCsePersonAddress
    {
      get => fatherCsePersonAddress ??= new();
      set => fatherCsePersonAddress = value;
    }

    /// <summary>
    /// A value of MotherSsnWorkArea.
    /// </summary>
    [JsonPropertyName("motherSsnWorkArea")]
    public SsnWorkArea MotherSsnWorkArea
    {
      get => motherSsnWorkArea ??= new();
      set => motherSsnWorkArea = value;
    }

    /// <summary>
    /// A value of FatherSsnWorkArea.
    /// </summary>
    [JsonPropertyName("fatherSsnWorkArea")]
    public SsnWorkArea FatherSsnWorkArea
    {
      get => fatherSsnWorkArea ??= new();
      set => fatherSsnWorkArea = value;
    }

    /// <summary>
    /// A value of MoCntyPrompt.
    /// </summary>
    [JsonPropertyName("moCntyPrompt")]
    public Common MoCntyPrompt
    {
      get => moCntyPrompt ??= new();
      set => moCntyPrompt = value;
    }

    /// <summary>
    /// A value of FaCntyPrompt.
    /// </summary>
    [JsonPropertyName("faCntyPrompt")]
    public Common FaCntyPrompt
    {
      get => faCntyPrompt ??= new();
      set => faCntyPrompt = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private CsePerson motherCsePerson;
    private Common moHiddenOtherCase;
    private Common faHiddenOtherCase;
    private Common faHiddenAe;
    private Common moHiddenAe;
    private Common faHiddenOtherCr;
    private Common moHiddenOtherCr;
    private CsePersonsWorkSet apSelected;
    private CodeValue selected;
    private Common moStatePrompt;
    private Common faStatePrompt;
    private Common apPrompt;
    private CsePersonAddress hiddenPrevFather;
    private CsePersonAddress hiddenPrevMother;
    private Standard standard;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Common motherCommon;
    private CsePersonsWorkSet motherCsePersonsWorkSet;
    private CaseRole motherCaseRole;
    private CsePersonAddress motherCsePersonAddress;
    private Common fatherCommon;
    private CsePersonsWorkSet fatherCsePersonsWorkSet;
    private CaseRole fatherCaseRole;
    private CsePersonAddress fatherCsePersonAddress;
    private SsnWorkArea motherSsnWorkArea;
    private SsnWorkArea fatherSsnWorkArea;
    private Common moCntyPrompt;
    private Common faCntyPrompt;
    private ServiceProvider serviceProvider;
    private Office office;
    private NextTranInfo hidden;
    private Common caseOpen;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MotherCsePerson.
    /// </summary>
    [JsonPropertyName("motherCsePerson")]
    public CsePerson MotherCsePerson
    {
      get => motherCsePerson ??= new();
      set => motherCsePerson = value;
    }

    /// <summary>
    /// A value of MoHiddenOtherCase.
    /// </summary>
    [JsonPropertyName("moHiddenOtherCase")]
    public Common MoHiddenOtherCase
    {
      get => moHiddenOtherCase ??= new();
      set => moHiddenOtherCase = value;
    }

    /// <summary>
    /// A value of FaHiddenOtherCase.
    /// </summary>
    [JsonPropertyName("faHiddenOtherCase")]
    public Common FaHiddenOtherCase
    {
      get => faHiddenOtherCase ??= new();
      set => faHiddenOtherCase = value;
    }

    /// <summary>
    /// A value of FaHiddenAe.
    /// </summary>
    [JsonPropertyName("faHiddenAe")]
    public Common FaHiddenAe
    {
      get => faHiddenAe ??= new();
      set => faHiddenAe = value;
    }

    /// <summary>
    /// A value of MoHiddenAe.
    /// </summary>
    [JsonPropertyName("moHiddenAe")]
    public Common MoHiddenAe
    {
      get => moHiddenAe ??= new();
      set => moHiddenAe = value;
    }

    /// <summary>
    /// A value of FaHiddenOtherCr.
    /// </summary>
    [JsonPropertyName("faHiddenOtherCr")]
    public Common FaHiddenOtherCr
    {
      get => faHiddenOtherCr ??= new();
      set => faHiddenOtherCr = value;
    }

    /// <summary>
    /// A value of MoHiddenOtherCr.
    /// </summary>
    [JsonPropertyName("moHiddenOtherCr")]
    public Common MoHiddenOtherCr
    {
      get => moHiddenOtherCr ??= new();
      set => moHiddenOtherCr = value;
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
    /// A value of MoStatePrompt.
    /// </summary>
    [JsonPropertyName("moStatePrompt")]
    public Common MoStatePrompt
    {
      get => moStatePrompt ??= new();
      set => moStatePrompt = value;
    }

    /// <summary>
    /// A value of FaStatePrompt.
    /// </summary>
    [JsonPropertyName("faStatePrompt")]
    public Common FaStatePrompt
    {
      get => faStatePrompt ??= new();
      set => faStatePrompt = value;
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
    /// A value of HiddenPrevFather.
    /// </summary>
    [JsonPropertyName("hiddenPrevFather")]
    public CsePersonAddress HiddenPrevFather
    {
      get => hiddenPrevFather ??= new();
      set => hiddenPrevFather = value;
    }

    /// <summary>
    /// A value of HiddenPrevMother.
    /// </summary>
    [JsonPropertyName("hiddenPrevMother")]
    public CsePersonAddress HiddenPrevMother
    {
      get => hiddenPrevMother ??= new();
      set => hiddenPrevMother = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of MotherCommon.
    /// </summary>
    [JsonPropertyName("motherCommon")]
    public Common MotherCommon
    {
      get => motherCommon ??= new();
      set => motherCommon = value;
    }

    /// <summary>
    /// A value of MotherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("motherCsePersonsWorkSet")]
    public CsePersonsWorkSet MotherCsePersonsWorkSet
    {
      get => motherCsePersonsWorkSet ??= new();
      set => motherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MotherCaseRole.
    /// </summary>
    [JsonPropertyName("motherCaseRole")]
    public CaseRole MotherCaseRole
    {
      get => motherCaseRole ??= new();
      set => motherCaseRole = value;
    }

    /// <summary>
    /// A value of MotherCsePersonAddress.
    /// </summary>
    [JsonPropertyName("motherCsePersonAddress")]
    public CsePersonAddress MotherCsePersonAddress
    {
      get => motherCsePersonAddress ??= new();
      set => motherCsePersonAddress = value;
    }

    /// <summary>
    /// A value of FatherCommon.
    /// </summary>
    [JsonPropertyName("fatherCommon")]
    public Common FatherCommon
    {
      get => fatherCommon ??= new();
      set => fatherCommon = value;
    }

    /// <summary>
    /// A value of FatherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fatherCsePersonsWorkSet")]
    public CsePersonsWorkSet FatherCsePersonsWorkSet
    {
      get => fatherCsePersonsWorkSet ??= new();
      set => fatherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FatherCaseRole.
    /// </summary>
    [JsonPropertyName("fatherCaseRole")]
    public CaseRole FatherCaseRole
    {
      get => fatherCaseRole ??= new();
      set => fatherCaseRole = value;
    }

    /// <summary>
    /// A value of FatherCsePersonAddress.
    /// </summary>
    [JsonPropertyName("fatherCsePersonAddress")]
    public CsePersonAddress FatherCsePersonAddress
    {
      get => fatherCsePersonAddress ??= new();
      set => fatherCsePersonAddress = value;
    }

    /// <summary>
    /// A value of MotherSsnWorkArea.
    /// </summary>
    [JsonPropertyName("motherSsnWorkArea")]
    public SsnWorkArea MotherSsnWorkArea
    {
      get => motherSsnWorkArea ??= new();
      set => motherSsnWorkArea = value;
    }

    /// <summary>
    /// A value of FatherSsnWorkArea.
    /// </summary>
    [JsonPropertyName("fatherSsnWorkArea")]
    public SsnWorkArea FatherSsnWorkArea
    {
      get => fatherSsnWorkArea ??= new();
      set => fatherSsnWorkArea = value;
    }

    /// <summary>
    /// A value of MoCntyPrompt.
    /// </summary>
    [JsonPropertyName("moCntyPrompt")]
    public Common MoCntyPrompt
    {
      get => moCntyPrompt ??= new();
      set => moCntyPrompt = value;
    }

    /// <summary>
    /// A value of FaCntyPrompt.
    /// </summary>
    [JsonPropertyName("faCntyPrompt")]
    public Common FaCntyPrompt
    {
      get => faCntyPrompt ??= new();
      set => faCntyPrompt = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private CsePerson motherCsePerson;
    private Common moHiddenOtherCase;
    private Common faHiddenOtherCase;
    private Common faHiddenAe;
    private Common moHiddenAe;
    private Common faHiddenOtherCr;
    private Common moHiddenOtherCr;
    private CsePersonsWorkSet apSelected;
    private Code prompt;
    private Common moStatePrompt;
    private Common faStatePrompt;
    private Common apPrompt;
    private CsePersonAddress hiddenPrevFather;
    private CsePersonAddress hiddenPrevMother;
    private Standard standard;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Common motherCommon;
    private CsePersonsWorkSet motherCsePersonsWorkSet;
    private CaseRole motherCaseRole;
    private CsePersonAddress motherCsePersonAddress;
    private Common fatherCommon;
    private CsePersonsWorkSet fatherCsePersonsWorkSet;
    private CaseRole fatherCaseRole;
    private CsePersonAddress fatherCsePersonAddress;
    private SsnWorkArea motherSsnWorkArea;
    private SsnWorkArea fatherSsnWorkArea;
    private Common moCntyPrompt;
    private Common faCntyPrompt;
    private ServiceProvider serviceProvider;
    private Office office;
    private NextTranInfo hidden;
    private Common caseOpen;
    private WorkArea headerLine;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonAddress Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of FatherAddressUpd.
    /// </summary>
    [JsonPropertyName("fatherAddressUpd")]
    public Common FatherAddressUpd
    {
      get => fatherAddressUpd ??= new();
      set => fatherAddressUpd = value;
    }

    /// <summary>
    /// A value of MotherAddressUpd.
    /// </summary>
    [JsonPropertyName("motherAddressUpd")]
    public Common MotherAddressUpd
    {
      get => motherAddressUpd ??= new();
      set => motherAddressUpd = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of InitFather.
    /// </summary>
    [JsonPropertyName("initFather")]
    public CsePersonAddress InitFather
    {
      get => initFather ??= new();
      set => initFather = value;
    }

    /// <summary>
    /// A value of InitMother.
    /// </summary>
    [JsonPropertyName("initMother")]
    public CsePersonAddress InitMother
    {
      get => initMother ??= new();
      set => initMother = value;
    }

    /// <summary>
    /// A value of MotherFvInd.
    /// </summary>
    [JsonPropertyName("motherFvInd")]
    public Common MotherFvInd
    {
      get => motherFvInd ??= new();
      set => motherFvInd = value;
    }

    /// <summary>
    /// A value of FatherFvInd.
    /// </summary>
    [JsonPropertyName("fatherFvInd")]
    public Common FatherFvInd
    {
      get => fatherFvInd ??= new();
      set => fatherFvInd = value;
    }

    private DateWorkArea current;
    private Common multipleAps;
    private CsePersonAddress null1;
    private AbendData abendData;
    private Common error;
    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private Common fatherAddressUpd;
    private Common motherAddressUpd;
    private Common invalid;
    private CodeValue codeValue;
    private Code code;
    private CaseRole caseRole;
    private Common common;
    private SsnWorkArea ssnWorkArea;
    private TextWorkArea textWorkArea;
    private CsePersonAddress initFather;
    private CsePersonAddress initMother;
    private Common motherFvInd;
    private Common fatherFvInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CsePerson Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    private CsePerson mother;
  }
#endregion
}
