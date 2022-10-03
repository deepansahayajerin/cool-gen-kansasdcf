// Program: OE_HIPH_INSURANCE_COV_BY_HOLDER, ID: 371183753, model: 746.
// Short name: SWEHIPHP
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
/// A program: OE_HIPH_INSURANCE_COV_BY_HOLDER.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHiphInsuranceCovByHolder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HIPH_INSURANCE_COV_BY_HOLDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiphInsuranceCovByHolder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiphInsuranceCovByHolder.
  /// </summary>
  public OeHiphInsuranceCovByHolder(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // Rebecca Grimes	01/02/95	Initial Code
    // Sid		01/31/95	Rework/Completion
    // T.O.Redmond	02/15/96	Retrofit
    // G.Lofton	04/04/96	Unit test corrections
    // R. Welborn      06/25/96        Added call to EAB for left-padding.
    // Sid		08/14/96	String Test fixes.
    // R. Marchman	11/14/96	Add new security and next tran.
    // 
    // S. Johnson      02/10/99        Fix lost data between PCOL
    // S. Johnson      02/15/99        Clear contact info when # changed.
    // P Phinney       09/27/00      H00104202 - Prevent Create with Carrier = 
    // CONV
    // Do NOT allow RETURN from HICL with Carrier Type = CONV
    // E Lyman       11/06/02        WR20311 - Prevent Create with Carrier = 
    // 0000
    // Do NOT allow RETURN from HICL with Carrier Type = 0000
    // Modify screen to reflect data model changes.
    // M Ashworth       11/18/02     WR20125 - Add start and end dates
    // Joyce Harden     8/2007     pr248185    add street_2 to the HIPH screen
    // J Huss		08/16/10	CQ# 555	Cleared health insurance coverage identifier 
    // when returning from COMP.
    // Gvandy		09/23/10	CQ515 Policy termination events error if the 
    // infrastructure record does not contain a case number.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.CsePerson.Number = import.CsePerson.Number;
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // Move Import Views to Export Views
    // ---------------------------------------------
    export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
    export.Starting.Number = import.Starting.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.CsePersonsWorkSet.Number = export.CsePerson.Number;
    MoveHealthInsuranceCompany3(import.HealthInsuranceCompany,
      export.HealthInsuranceCompany);
    export.HealthInsuranceCoverage.Assign(import.HealthInsuranceCoverage);
    MoveContact2(import.Contact, export.Contact);
    export.HealthInsuranceCompanyAddress.Assign(
      import.HealthInsuranceCompanyAddress);
    MoveCommon(import.WorkPromptCarrier, export.WorkPromptCarrier);
    MoveCommon(import.WorkPromptContact, export.WorkPromptContact);
    MoveCommon(import.WorkPromptCoverage, export.WorkPromptCoverage);
    export.WorkPromptInd.Flag = import.WorkPromptInd.Flag;
    export.SelectedCsePerson.Number = import.SelectedCsePerson.Number;
    export.SelectedCsePersonsWorkSet.Assign(import.SelectedCsePersonsWorkSet);
    export.SelectedContact.Assign(import.SelectedContact);
    export.SelectedHealthInsuranceCoverage.Assign(
      import.SelectedHealthInsuranceCoverage);
    export.SelectedHealthInsuranceCompany.Assign(
      import.SelectedHealthInsuranceCompany);
    export.SelectedHealthInsuranceCompanyAddress.Assign(
      import.SelectedHealthInsuranceCompanyAddress);
    MoveIncomeSource(import.SelectedIncomeSource, export.SelectedIncomeSource);
    export.PromptCode1.SelectChar = import.PromptCode1.SelectChar;
    export.PromptCode2.SelectChar = import.PromptCode2.SelectChar;
    export.PromptCode3.SelectChar = import.PromptCode3.SelectChar;
    export.PromptCode4.SelectChar = import.PromptCode4.SelectChar;
    export.PromptCode5.SelectChar = import.PromptCode5.SelectChar;
    export.PromptCode6.SelectChar = import.PromptCode6.SelectChar;
    export.PromptCode7.SelectChar = import.PromptCode7.SelectChar;
    export.PromptEmployer.SelectChar = import.PromptEmployer.SelectChar;
    export.PromptRelToChild.SelectChar = import.PromptRelToChild.SelectChar;
    export.Employer.Name = import.Employer.Name;
    export.EmpProvidingInsurance.Flag = import.EmpProvidingInsurance.Flag;

    if (!IsEmpty(export.Starting.Number))
    {
      local.TextWorkArea.Text10 = export.Starting.Number;
      UseEabPadLeftWithZeros();
      export.Starting.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.HealthInsuranceCompany.CarrierCode))
    {
      local.TextWorkArea.Text10 = export.HealthInsuranceCompany.CarrierCode ?? Spaces
        (10);
      UseEabPadLeftWithZeros();
      export.HealthInsuranceCompany.CarrierCode =
        Substring(local.TextWorkArea.Text10, 4, 7);
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.Hcase.Number = import.Hcase.Number;
    export.HhealthInsuranceCompany.Assign(import.HhealthInsuranceCompany);
    export.HhealthInsuranceCoverage.Assign(import.HhealthInsuranceCoverage);
    export.Hcontact.Assign(import.Hcontact);

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Hcase.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.Starting.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    local.SearchCase.Number = export.Starting.Number;
    local.SearchCsePerson.Number = export.CsePerson.Number;

    // ************************************************
    // *If the import CSE PERSON is different than the*
    // *CSE Person that we started with, then we must *
    // *Clear out any data from prior screen attribute*
    // ************************************************
    if (!Equal(import.CsePerson.Number, import.HcsePerson.Number) && !
      IsEmpty(import.HcsePerson.Number) && !
      Equal(import.HcsePerson.Number, "0000000000"))
    {
      MoveHealthInsuranceCompany2(local.RefreshHealthInsuranceCompany,
        export.HealthInsuranceCompany);
      export.HealthInsuranceCompanyAddress.Assign(
        local.RefreshHealthInsuranceCompanyAddress);
      export.HealthInsuranceCoverage.
        Assign(local.RefreshHealthInsuranceCoverage);
      MoveContact2(local.RefreshContact, export.Contact);
      export.HhealthInsuranceCompany.
        Assign(local.RefreshHealthInsuranceCompany);
      MoveHealthInsuranceCoverage2(local.RefreshHealthInsuranceCoverage,
        export.HhealthInsuranceCoverage);
      export.Hcontact.Assign(local.RefreshContact);
      UseOeCabCheckCaseMember3();
      global.Command = "DISPLAY";
    }

    if (Equal(import.CsePerson.Number, import.HcsePerson.Number) && !
      IsEmpty(import.HcsePerson.Number) && !
      Equal(import.HcsePerson.Number, "0000000000") && import
      .Contact.ContactNumber > 0)
    {
      var field1 = GetField(export.Contact, "contactNumber");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.WorkPromptContact, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Contact, "nameLast");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.Contact, "nameFirst");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.Contact, "middleInitial");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.Contact, "relationshipToCsePerson");

      field6.Color = "cyan";
      field6.Protected = true;
    }

    export.HcsePerson.Number = import.HcsePerson.Number;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      export.Hidden.CaseNumber = export.Starting.Number;
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

    local.Min.Date = new DateTime(1900, 1, 1);
    UseCabSetMaximumDiscontinueDate();

    // ---------------------------------------------
    // When control is being returned from one
    // of the prompt list screens, take the
    // necessary actions depending on the command
    // returned.
    // ---------------------------------------------
    if (!IsEmpty(import.WorkPromptInd.Flag))
    {
      export.WorkPromptInd.Flag = "";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(export.SelectedCsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.SelectedCsePersonsWorkSet.Number;
        export.CsePersonsWorkSet.FormattedName =
          export.SelectedCsePersonsWorkSet.FormattedName;
        local.SearchCsePerson.Number = export.SelectedCsePersonsWorkSet.Number;
        export.Contact.Assign(local.NullContact);

        // Clear the hic identifier so the screen doesn't try and re-display the
        // hic record for the previous person.
        export.HealthInsuranceCoverage.Identifier = 0;
      }

      export.CsePersonPrompt.SelectChar = "";

      var field = GetField(export.Contact, "contactNumber");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETHIPL"))
    {
      export.WorkPromptCoverage.SelectChar = "";

      if (!IsEmpty(export.WorkPromptCarrier.SelectChar))
      {
        var field = GetField(export.WorkPromptCarrier, "selectChar");

        field.Error = true;
      }

      if (import.SelectedHealthInsuranceCoverage.Identifier > 0)
      {
        export.HealthInsuranceCoverage.Assign(
          import.SelectedHealthInsuranceCoverage);

        var field = GetField(export.Contact, "contactNumber");

        field.Protected = false;
        field.Focused = true;

        export.CsePerson.Number = export.SelectedCsePerson.Number;
        export.CsePersonsWorkSet.Assign(export.SelectedCsePersonsWorkSet);
        MoveContact2(local.RefreshContact, export.Contact);
        export.Hcontact.Assign(local.RefreshContact);
        global.Command = "DISPLAY";
      }
      else
      {
        var field =
          GetField(export.HealthInsuranceCoverage, "insurancePolicyNumber");

        field.Protected = false;
        field.Focused = true;

        global.Command = "";
      }
    }

    if (Equal(global.Command, "RETPCOL"))
    {
      // ************************************************
      // *If the import CSE PERSON is different than the*
      // *CSE Person that we started with, then we must *
      // *Clear out any data from prior screen attribute*
      // ************************************************
      if (!Equal(import.CsePerson.Number, import.HcsePerson.Number) && !
        IsEmpty(import.HcsePerson.Number) && !
        Equal(import.HcsePerson.Number, "0000000000"))
      {
        MoveHealthInsuranceCompany2(local.RefreshHealthInsuranceCompany,
          export.HealthInsuranceCompany);
        export.HealthInsuranceCompanyAddress.Assign(
          local.RefreshHealthInsuranceCompanyAddress);
        export.HealthInsuranceCoverage.Assign(
          local.RefreshHealthInsuranceCoverage);
        export.HhealthInsuranceCompany.Assign(
          local.RefreshHealthInsuranceCompany);
        MoveHealthInsuranceCoverage2(local.RefreshHealthInsuranceCoverage,
          export.HhealthInsuranceCoverage);
        MoveContact2(local.RefreshContact, export.Contact);
        export.Hcontact.Assign(local.RefreshContact);
        export.HcsePerson.Number = import.CsePerson.Number;
      }

      if (export.SelectedContact.ContactNumber > 0)
      {
        MoveContact2(export.SelectedContact, export.Contact);
        export.Hcontact.Assign(export.SelectedContact);
        export.WorkPromptContact.SelectChar = "";
      }

      export.WorkPromptContact.SelectChar = "";

      if (!IsEmpty(export.WorkPromptCoverage.SelectChar))
      {
        var field = GetField(export.WorkPromptCoverage, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.WorkPromptCarrier.SelectChar))
      {
        var field = GetField(export.WorkPromptCarrier, "selectChar");

        field.Error = true;
      }

      UseOeVerifyContactRelationship();

      if (AsChar(local.CodeValueValid.Flag) == 'Y')
      {
        var field = GetField(export.Contact, "relationshipToCsePerson");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.Contact, "relationshipToCsePerson");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE";
      }
    }

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
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

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      // ---------------------------------------------
      // Shift coverage codes to left, if necessary.
      // ---------------------------------------------
      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.CountLoops.Count = 0;

        do
        {
          local.Count.Count = 2;

          do
          {
            switch(local.Count.Count)
            {
              case 2:
                if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode1))
                {
                  export.HealthInsuranceCoverage.CoverageCode1 =
                    export.HealthInsuranceCoverage.CoverageCode2 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode2 =
                    export.HealthInsuranceCoverage.CoverageCode3 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode3 =
                    export.HealthInsuranceCoverage.CoverageCode4 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode4 =
                    export.HealthInsuranceCoverage.CoverageCode5 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode5 =
                    export.HealthInsuranceCoverage.CoverageCode6 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode6 =
                    export.HealthInsuranceCoverage.CoverageCode7 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode7 = "";
                }

                break;
              case 3:
                if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode2))
                {
                  export.HealthInsuranceCoverage.CoverageCode2 =
                    export.HealthInsuranceCoverage.CoverageCode3 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode3 =
                    export.HealthInsuranceCoverage.CoverageCode4 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode4 =
                    export.HealthInsuranceCoverage.CoverageCode5 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode5 =
                    export.HealthInsuranceCoverage.CoverageCode6 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode6 =
                    export.HealthInsuranceCoverage.CoverageCode7 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode7 = "";
                }

                break;
              case 4:
                if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode3))
                {
                  export.HealthInsuranceCoverage.CoverageCode3 =
                    export.HealthInsuranceCoverage.CoverageCode4 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode4 =
                    export.HealthInsuranceCoverage.CoverageCode5 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode5 =
                    export.HealthInsuranceCoverage.CoverageCode6 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode6 =
                    export.HealthInsuranceCoverage.CoverageCode7 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode7 = "";
                }

                break;
              case 5:
                if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode4))
                {
                  export.HealthInsuranceCoverage.CoverageCode4 =
                    export.HealthInsuranceCoverage.CoverageCode5 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode5 =
                    export.HealthInsuranceCoverage.CoverageCode6 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode6 =
                    export.HealthInsuranceCoverage.CoverageCode7 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode7 = "";
                }

                break;
              case 6:
                if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode5))
                {
                  export.HealthInsuranceCoverage.CoverageCode5 =
                    export.HealthInsuranceCoverage.CoverageCode6 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode6 =
                    export.HealthInsuranceCoverage.CoverageCode7 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode7 = "";
                }

                break;
              case 7:
                if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode6))
                {
                  export.HealthInsuranceCoverage.CoverageCode6 =
                    export.HealthInsuranceCoverage.CoverageCode7 ?? "";
                  export.HealthInsuranceCoverage.CoverageCode7 = "";
                }

                break;
              default:
                break;
            }

            ++local.Count.Count;
          }
          while(local.Count.Count != 8);

          ++local.CountLoops.Count;
        }
        while(local.CountLoops.Count != 3);
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "HICP":
        // ---------------------------------------------
        // The CSE Person Number is required.
        // ---------------------------------------------
        if (IsEmpty(import.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          export.CsePersonsWorkSet.FormattedName = "";
          export.CsePersonsWorkSet.Ssn = "";

          return;
        }
        else
        {
          UseOeCabCheckCaseMember3();

          if (!IsEmpty(local.WorkError.Flag))
          {
            export.CsePersonsWorkSet.FormattedName = "";
            export.CsePersonsWorkSet.Ssn = "";

            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";

            return;
          }
        }

        if (IsEmpty(export.Starting.Number))
        {
          var field = GetField(export.Starting, "number");

          field.Error = true;

          ExitState = "OE0000_CASE_NO_REQD_FOR_HICP";

          return;
        }
        else
        {
          UseOeCabCheckCaseMember2();

          if (AsChar(local.WorkError.Flag) == 'R')
          {
            var field9 = GetField(export.CsePerson, "number");

            field9.Error = true;

            var field10 = GetField(export.Starting, "number");

            field10.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";

            return;
          }
          else if (AsChar(local.WorkError.Flag) == 'C')
          {
            var field = GetField(export.Starting, "number");

            field.Error = true;

            ExitState = "CASE_NF";

            return;
          }
        }

        export.WorkPromptInd.Flag = "Y";

        // ************************************************
        // *Flow to Health Insurance Coverage by Person   *
        // *HICP Screen.
        // 
        // *
        // ************************************************
        ExitState = "ECO_XFR_TO_HICP";

        return;
      case "RETPCOL":
        break;
      case "RETINCL":
        if (Lt(local.Blank.Identifier, export.SelectedIncomeSource.Identifier))
        {
          if (ReadEmployerIncomeSource())
          {
            export.Employer.Name = entities.Employer.Name;
            export.SelectedIncomeSource.Type1 = entities.IncomeSource.Type1;
          }
          else
          {
            ExitState = "EMPLOYER_NF";
          }
        }

        export.PromptEmployer.SelectChar = "";

        if (AsChar(entities.IncomeSource.Type1) != 'M' && AsChar
          (entities.IncomeSource.Type1) != 'E')
        {
          var field = GetField(export.PromptEmployer, "selectChar");

          field.Error = true;

          ExitState = "INC_SOURCE_TYPE_OF_M_OR_E_ALLOWD";
        }

        break;
      case "RETHICL":
        if (export.SelectedHealthInsuranceCompany.Identifier > 0)
        {
          MoveHealthInsuranceCompany3(export.SelectedHealthInsuranceCompany,
            export.HealthInsuranceCompany);
          export.HhealthInsuranceCompany.Assign(
            export.SelectedHealthInsuranceCompany);
          export.HealthInsuranceCompanyAddress.Assign(
            export.SelectedHealthInsuranceCompanyAddress);
        }

        export.WorkPromptCarrier.SelectChar = "";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETCDVL":
        if (AsChar(export.PromptCode1.SelectChar) == 'S')
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode2");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode1.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode1 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptCode2.SelectChar) == 'S')
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode3");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode2.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode2 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptCode3.SelectChar) == 'S')
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode4");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode3.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode3 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptCode4.SelectChar) == 'S')
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode5");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode4.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode4 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptCode5.SelectChar) == 'S')
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode6");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode5.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode5 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptCode6.SelectChar) == 'S')
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode7");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode6.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode6 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptCode7.SelectChar) == 'S')
        {
          var field =
            GetField(export.HealthInsuranceCoverage, "otherCoveredPersons");

          field.Protected = false;
          field.Focused = true;

          export.PromptCode7.SelectChar = "";
          export.HealthInsuranceCoverage.CoverageCode7 =
            import.CodeValue.Cdvalue;
        }
        else if (AsChar(export.PromptRelToChild.SelectChar) == 'S')
        {
          var field9 = GetField(export.Contact, "relationshipToCsePerson");

          field9.Color = "cyan";
          field9.Protected = true;
          field9.Focused = false;

          export.PromptRelToChild.SelectChar = "";

          var field10 = GetField(export.PromptRelToChild, "selectChar");

          field10.Protected = false;
          field10.Focused = true;

          export.Contact.RelationshipToCsePerson = import.CodeValue.Cdvalue;
        }

        if (!IsEmpty(export.PromptCode1.SelectChar))
        {
          var field = GetField(export.PromptCode1, "selectChar");

          field.Error = true;
        }

        if (!IsEmpty(export.PromptCode2.SelectChar))
        {
          var field = GetField(export.PromptCode2, "selectChar");

          field.Error = true;
        }

        if (!IsEmpty(export.PromptCode3.SelectChar))
        {
          var field = GetField(export.PromptCode3, "selectChar");

          field.Error = true;
        }

        if (!IsEmpty(export.PromptCode4.SelectChar))
        {
          var field = GetField(export.PromptCode4, "selectChar");

          field.Error = true;
        }

        if (!IsEmpty(export.PromptCode5.SelectChar))
        {
          var field = GetField(export.PromptCode5, "selectChar");

          field.Error = true;
        }

        if (!IsEmpty(export.PromptCode6.SelectChar))
        {
          var field = GetField(export.PromptCode6, "selectChar");

          field.Protected = false;
          field.Focused = true;
        }

        if (!IsEmpty(export.PromptCode7.SelectChar))
        {
          var field = GetField(export.PromptCode7, "selectChar");

          field.Protected = false;
          field.Focused = true;
        }

        if (!IsEmpty(export.HealthInsuranceCompany.CarrierCode))
        {
          var field9 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.WorkPromptCarrier, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;
        }

        break;
      case "LIST":
        // ---------------------------------------------
        // This command allows the user to link to a
        // selection list and retrieve the appropriate
        // value, not losing any of the data already
        // entered.
        // ---------------------------------------------
        if (!IsEmpty(import.CsePersonPrompt.SelectChar) && AsChar
          (import.CsePersonPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(import.WorkPromptContact.SelectChar) && AsChar
          (import.WorkPromptContact.SelectChar) != 'S')
        {
          var field = GetField(export.WorkPromptContact, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(import.PromptRelToChild.SelectChar) && AsChar
          (import.PromptRelToChild.SelectChar) != 'S')
        {
          var field = GetField(export.PromptRelToChild, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(import.WorkPromptCoverage.SelectChar) && AsChar
          (import.WorkPromptCoverage.SelectChar) != 'S')
        {
          var field = GetField(export.WorkPromptCoverage, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(import.WorkPromptCarrier.SelectChar) && AsChar
          (import.WorkPromptCarrier.SelectChar) != 'S')
        {
          var field = GetField(export.WorkPromptCarrier, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode1.SelectChar) && AsChar
          (export.PromptCode1.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode1, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode2.SelectChar) && AsChar
          (export.PromptCode2.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode2, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode3.SelectChar) && AsChar
          (export.PromptCode3.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode3, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode4.SelectChar) && AsChar
          (export.PromptCode4.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode4, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode5.SelectChar) && AsChar
          (export.PromptCode5.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode5, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode6.SelectChar) && AsChar
          (export.PromptCode6.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode6, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptCode7.SelectChar) && AsChar
          (export.PromptCode7.SelectChar) != 'S')
        {
          var field = GetField(export.PromptCode7, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsEmpty(export.PromptEmployer.SelectChar) && AsChar
          (export.PromptEmployer.SelectChar) != 'S')
        {
          var field = GetField(export.PromptEmployer, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (IsEmpty(import.CsePersonPrompt.SelectChar) && IsEmpty
          (import.WorkPromptCarrier.SelectChar) && IsEmpty
          (import.WorkPromptContact.SelectChar) && IsEmpty
          (import.WorkPromptCoverage.SelectChar) && IsEmpty
          (export.PromptCode1.SelectChar) && IsEmpty
          (export.PromptCode2.SelectChar) && IsEmpty
          (export.PromptCode3.SelectChar) && IsEmpty
          (export.PromptCode4.SelectChar) && IsEmpty
          (export.PromptCode5.SelectChar) && IsEmpty
          (export.PromptCode6.SelectChar) && IsEmpty
          (export.PromptCode7.SelectChar) && IsEmpty
          (export.PromptEmployer.SelectChar) && IsEmpty
          (export.PromptRelToChild.SelectChar))
        {
          var field9 = GetField(export.CsePersonPrompt, "selectChar");

          field9.Error = true;

          var field10 = GetField(export.WorkPromptContact, "selectChar");

          field10.Error = true;

          var field11 = GetField(export.WorkPromptCoverage, "selectChar");

          field11.Error = true;

          var field12 = GetField(export.WorkPromptCarrier, "selectChar");

          field12.Error = true;

          var field13 = GetField(export.PromptCode1, "selectChar");

          field13.Error = true;

          var field14 = GetField(export.PromptCode2, "selectChar");

          field14.Error = true;

          var field15 = GetField(export.PromptCode3, "selectChar");

          field15.Error = true;

          var field16 = GetField(export.PromptCode4, "selectChar");

          field16.Error = true;

          var field17 = GetField(export.PromptCode5, "selectChar");

          field17.Error = true;

          var field18 = GetField(export.PromptCode6, "selectChar");

          field18.Error = true;

          var field19 = GetField(export.PromptCode7, "selectChar");

          field19.Error = true;

          var field20 = GetField(export.PromptEmployer, "selectChar");

          field20.Error = true;

          var field21 = GetField(export.PromptRelToChild, "selectChar");

          field21.Error = true;

          if (Equal(import.CsePerson.Number, import.HcsePerson.Number) && !
            IsEmpty(import.HcsePerson.Number) && !
            Equal(import.HcsePerson.Number, "0000000000") && import
            .Contact.ContactNumber > 0)
          {
            var field = GetField(export.WorkPromptContact, "selectChar");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(import.CsePersonPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(export.Starting.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
        }
        else if (AsChar(import.WorkPromptContact.SelectChar) == 'S')
        {
          if (IsEmpty(export.CsePerson.Number))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "OE0000_PERSON_NO_REQD_FOR_PCOL";
          }
          else if (!IsEmpty(export.Employer.Name))
          {
            var field = GetField(export.WorkPromptContact, "selectChar");

            field.Error = true;

            ExitState = "OE0000_EMPLOYER_NO_HINS_CONTACT";
          }
          else
          {
            export.Hcontact.Assign(export.Contact);
            export.HcsePerson.Number = export.CsePerson.Number;
            MoveHealthInsuranceCompany4(export.HealthInsuranceCompany,
              export.HhealthInsuranceCompany);
            MoveHealthInsuranceCoverage2(export.HhealthInsuranceCoverage,
              export.HealthInsuranceCoverage);
            export.WorkPromptInd.Flag = "Y";

            // ************************************************
            // *Link to Person Contact List (PCOL)            *
            // ************************************************
            ExitState = "ECO_LNK_TO_LIST_CONTACT";
          }
        }
        else if (AsChar(import.WorkPromptCoverage.SelectChar) == 'S')
        {
          export.Hcontact.Assign(export.Contact);
          export.HcsePerson.Number = export.CsePerson.Number;
          MoveHealthInsuranceCompany4(export.HealthInsuranceCompany,
            export.HhealthInsuranceCompany);
          MoveHealthInsuranceCoverage2(export.HealthInsuranceCoverage,
            export.HhealthInsuranceCoverage);
          export.WorkPromptInd.Flag = "Y";

          // ************************************************
          // *Flow to Health Insurance Coverage by Person   *
          // *List Screen HIPL.                             *
          // ************************************************
          ExitState = "ECO_LNK_TO_HIPL";
        }
        else if (AsChar(import.WorkPromptCarrier.SelectChar) == 'S')
        {
          export.Hcontact.Assign(export.Contact);
          export.HcsePerson.Number = export.CsePerson.Number;
          MoveHealthInsuranceCompany4(export.HealthInsuranceCompany,
            export.HhealthInsuranceCompany);
          MoveHealthInsuranceCoverage2(export.HealthInsuranceCoverage,
            export.HhealthInsuranceCoverage);

          // P Phinney       09/27/00      H00104202 - Do NOT allow RETURN from 
          // HICL with Carrier Type = CONV
          export.PassToHicl.NextTransaction = "HIPH";
          export.WorkPromptInd.Flag = "Y";

          // ************************************************
          // *Flow to Health Insurance Company List Screen  *
          // *HICL.
          // 
          // *
          // ************************************************
          ExitState = "ECO_LNK_TO_LIST_INSURANCE_CO";
        }
        else if (AsChar(export.PromptCode1.SelectChar) == 'S' || AsChar
          (export.PromptCode2.SelectChar) == 'S' || AsChar
          (export.PromptCode3.SelectChar) == 'S' || AsChar
          (export.PromptCode4.SelectChar) == 'S' || AsChar
          (export.PromptCode5.SelectChar) == 'S' || AsChar
          (export.PromptCode6.SelectChar) == 'S' || AsChar
          (export.PromptCode7.SelectChar) == 'S')
        {
          export.Code.CodeName = "EDS COVERAGES";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else if (AsChar(export.PromptEmployer.SelectChar) == 'S')
        {
          if (!IsEmpty(export.Contact.NameFirst) || !
            IsEmpty(export.Contact.NameLast))
          {
            export.EmpProvidingInsurance.Flag = "N";
            export.PromptEmployer.SelectChar = "";

            var field = GetField(export.PromptEmployer, "selectChar");

            field.Error = true;

            ExitState = "OE0000_HINS_CONTACT_NO_EMPLOYER";
          }
          else
          {
            ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
          }
        }
        else if (AsChar(import.PromptRelToChild.SelectChar) == 'S')
        {
          export.Code.CodeName = "EDS RELATIONSHIPS";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        break;
      case "CREATE":
        // ---------------------------------------------
        // To create a record the required fields are
        // CSE Person Number, Case ID, Relationship to
        // CSE Person if the policy holder is a third
        // party, either the insurance policy number or
        // the group policy number, and the insurance
        // company identified that is to carry the
        // policy.
        // --------------------------------------------
        if (IsEmpty(import.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          export.CsePersonsWorkSet.FormattedName = "";
          export.CsePersonsWorkSet.Ssn = "";
        }
        else
        {
          UseOeCabCheckCaseMember3();

          if (!IsEmpty(local.WorkError.Flag))
          {
            export.CsePersonsWorkSet.FormattedName = "";
            export.CsePersonsWorkSet.Ssn = "";

            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";

            break;
          }
        }

        // ---------------------------------------------
        // If the Contact person details for the CSE
        // Person are entered. If the contact does not
        // exist, CREATE one here.
        // ---------------------------------------------
        if (import.Contact.ContactNumber != 0 || !
          IsEmpty(import.Contact.MiddleInitial) || !
          IsEmpty(import.Contact.NameFirst) || !
          IsEmpty(import.Contact.NameLast) || !
          IsEmpty(import.Contact.RelationshipToCsePerson))
        {
          local.Contact.Flag = "Y";

          if (IsEmpty(export.Contact.NameFirst))
          {
            var field = GetField(export.Contact, "nameFirst");

            field.Error = true;

            local.Contact.Flag = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }

          if (IsEmpty(export.Contact.NameLast))
          {
            var field = GetField(export.Contact, "nameLast");

            field.Error = true;

            local.Contact.Flag = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }

          if (IsEmpty(import.Contact.RelationshipToCsePerson))
          {
            var field = GetField(export.Contact, "relationshipToCsePerson");

            field.Error = true;

            local.Contact.Flag = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }
          else
          {
            UseOeVerifyContactRelationship();

            if (AsChar(local.CodeValueValid.Flag) == 'Y')
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Color = "cyan";
              field.Protected = true;
            }
            else
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";
            }
          }
        }

        if (IsEmpty(import.HealthInsuranceCoverage.InsurancePolicyNumber))
        {
          var field =
            GetField(export.HealthInsuranceCoverage, "insurancePolicyNumber");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INSURANCE_POLICY_NUMB_REQUIRED";
          }
        }

        if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode1))
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "1_COVERAGE_CODE_REQUIRED";
          }
        }
        else
        {
          local.Code.CodeName = "EDS COVERAGES";
          local.CodeValue.Cdvalue =
            export.HealthInsuranceCoverage.CoverageCode1 ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.WorkError.Count != 0)
          {
            var field =
              GetField(export.HealthInsuranceCoverage, "coverageCode1");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0195_INVALID_COVERAGE_CODE";
            }
          }

          if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode2))
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceCoverage.CoverageCode2 ?? Spaces(10);
            UseCabValidateCodeValue();

            if (local.WorkError.Count != 0)
            {
              var field =
                GetField(export.HealthInsuranceCoverage, "coverageCode2");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "OE0195_INVALID_COVERAGE_CODE";
              }
            }

            if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode3))
            {
              local.CodeValue.Cdvalue =
                export.HealthInsuranceCoverage.CoverageCode3 ?? Spaces(10);
              UseCabValidateCodeValue();

              if (local.WorkError.Count != 0)
              {
                var field =
                  GetField(export.HealthInsuranceCoverage, "coverageCode3");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "OE0195_INVALID_COVERAGE_CODE";
                }
              }

              if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode4))
              {
                local.CodeValue.Cdvalue =
                  export.HealthInsuranceCoverage.CoverageCode4 ?? Spaces(10);
                UseCabValidateCodeValue();

                if (local.WorkError.Count != 0)
                {
                  var field =
                    GetField(export.HealthInsuranceCoverage, "coverageCode4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "OE0195_INVALID_COVERAGE_CODE";
                  }
                }

                if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode5))
                {
                  local.CodeValue.Cdvalue =
                    export.HealthInsuranceCoverage.CoverageCode5 ?? Spaces(10);
                  UseCabValidateCodeValue();

                  if (local.WorkError.Count != 0)
                  {
                    var field =
                      GetField(export.HealthInsuranceCoverage, "coverageCode5");
                      

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "OE0195_INVALID_COVERAGE_CODE";
                    }
                  }

                  if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode6))
                  {
                    local.CodeValue.Cdvalue =
                      export.HealthInsuranceCoverage.CoverageCode6 ?? Spaces
                      (10);
                    UseCabValidateCodeValue();

                    if (local.WorkError.Count != 0)
                    {
                      var field =
                        GetField(export.HealthInsuranceCoverage, "coverageCode6");
                        

                      field.Error = true;

                      if (IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        ExitState = "OE0195_INVALID_COVERAGE_CODE";
                      }
                    }

                    if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode7))
                    {
                      local.CodeValue.Cdvalue =
                        export.HealthInsuranceCoverage.CoverageCode7 ?? Spaces
                        (10);
                      UseCabValidateCodeValue();

                      if (local.WorkError.Count != 0)
                      {
                        var field =
                          GetField(export.HealthInsuranceCoverage,
                          "coverageCode7");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "OE0195_INVALID_COVERAGE_CODE";
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }

        // *****************************************************************************
        // **     Effective Date edits.
        // *****************************************************************************
        if (Equal(export.HealthInsuranceCoverage.PolicyEffectiveDate,
          local.NullDateWorkArea.Date))
        {
          var field =
            GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }
        else
        {
          if (Lt(export.HealthInsuranceCoverage.PolicyEffectiveDate,
            local.Min.Date))
          {
            var field =
              GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_INVALID_DATE";
            }
          }

          if (Lt(Now().Date, export.HealthInsuranceCoverage.PolicyEffectiveDate))
            
          {
            var field =
              GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
            }
          }
        }

        if (Equal(export.HealthInsuranceCoverage.PolicyExpirationDate, null))
        {
          export.HealthInsuranceCoverage.PolicyExpirationDate = local.Max.Date;
        }

        if (Lt(export.HealthInsuranceCoverage.PolicyExpirationDate,
          import.HealthInsuranceCoverage.PolicyEffectiveDate))
        {
          var field9 =
            GetField(export.HealthInsuranceCoverage, "policyExpirationDate");

          field9.Error = true;

          var field10 =
            GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

          field10.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CO0000_INVALID_EFF_OR_EXP_DATE";
          }
        }

        if (!Equal(export.HealthInsuranceCoverage.VerifiedDate, null) && Lt
          (export.HealthInsuranceCoverage.VerifiedDate,
          export.HealthInsuranceCoverage.PolicyEffectiveDate) || Lt
          (Now().Date, export.HealthInsuranceCoverage.VerifiedDate))
        {
          var field = GetField(export.HealthInsuranceCoverage, "verifiedDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0182_INVALID_VERIFIED_DATE";
          }
        }

        if (AsChar(export.EmpProvidingInsurance.Flag) == 'Y' || AsChar
          (export.EmpProvidingInsurance.Flag) == 'N' || IsEmpty
          (export.EmpProvidingInsurance.Flag))
        {
          // Continue
        }
        else
        {
          var field = GetField(export.EmpProvidingInsurance, "flag");

          field.Error = true;

          ExitState = "INVALID_INDICATOR_Y_N_SPACE";
        }

        // M Ashworth      12/10/02      WR20311 added link to incl when 
        // creating hic. Export contact last name is included in the if
        // statement. Users added business rule " If "other Policy Holder" field
        // is completed, then no Employer Name of yes/no indicator is needed.
        if (IsExitState("ACO_NN0000_ALL_OK") && IsEmpty
          (export.Contact.NameLast) && IsEmpty(export.Employer.Name))
        {
          if (AsChar(export.EmpProvidingInsurance.Flag) == 'Y' || AsChar
            (export.EmpProvidingInsurance.Flag) == 'N')
          {
            // Continue
          }
          else
          {
            ExitState = "IS_EMP_PROVIDING_HEALTH_INS";

            var field = GetField(export.EmpProvidingInsurance, "flag");

            field.Error = true;
          }

          if (AsChar(export.EmpProvidingInsurance.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
          }
        }

        if (AsChar(export.SelectedIncomeSource.Type1) != 'M' && AsChar
          (export.SelectedIncomeSource.Type1) != 'E' && AsChar
          (export.EmpProvidingInsurance.Flag) == 'Y' && IsEmpty
          (export.Contact.NameLast))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.PromptEmployer, "selectChar");

            field.Error = true;

            ExitState = "INC_SOURCE_TYPE_OF_M_OR_E_ALLOWD";
          }
        }

        if (IsEmpty(export.HealthInsuranceCompany.CarrierCode))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;

          MoveHealthInsuranceCompany2(local.RefreshHealthInsuranceCompany,
            export.HealthInsuranceCompany);
          export.HhealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          export.HealthInsuranceCompanyAddress.Assign(
            local.RefreshHealthInsuranceCompanyAddress);

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsEmpty(export.HealthInsuranceCompany.CarrierCode))
        {
          local.TextWorkArea.Text10 =
            export.HealthInsuranceCompany.CarrierCode ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.HealthInsuranceCompany.CarrierCode =
            Substring(local.TextWorkArea.Text10, 4, 7);
          local.HealthInsuranceCompany.CarrierCode =
            Substring(local.TextWorkArea.Text10, 4, 7);
        }

        // P Phinney       09/27/00      H00104202 - Prevent Create with Carrier
        // = CONV
        if (Equal(export.HealthInsuranceCompany.CarrierCode, "0000000"))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "HEALTH_INS_CARRIER_CONV";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ---------------------------------------------
        // There must be a health insurance company to
        // associate the coverage to.
        // ---------------------------------------------
        UseOeReadHealthInsCompany();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // M Ashworth       11/18/02     WR20125 - Add start and end dates
        if (Lt(export.HealthInsuranceCompany.EndDate,
          export.HealthInsuranceCoverage.PolicyEffectiveDate))
        {
          var field =
            GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INS_CO_END_DATE_PRIOR_TO_EFF_DAT";

            break;
          }
        }

        if (AsChar(local.Contact.Flag) == 'Y')
        {
          UseOeCabCheckPersonContact1();

          if (AsChar(export.WorkContactExist.Flag) != 'Y')
          {
            UseOePconCreateContactDetails();
          }
          else if (!Equal(export.Contact.RelationshipToCsePerson,
            local.Found.RelationshipToCsePerson))
          {
            UseOeUpdateContactRelationship2();
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (IsEmpty(export.Employer.Name))
        {
          export.SelectedIncomeSource.Identifier = local.Blank.Identifier;
        }

        // ---------------------------------------------
        // Insert the USE statement here to call the
        // CREATE action block.
        // ---------------------------------------------
        UseOeCreateInsuranceCoverHolder();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          var field9 = GetField(export.Contact, "contactNumber");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.WorkPromptContact, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.Contact, "nameLast");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Contact, "nameFirst");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.Contact, "middleInitial");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.Contact, "relationshipToCsePerson");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.WorkPromptCarrier, "selectChar");

          field16.Color = "cyan";
          field16.Protected = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else if (IsExitState("HEALTH_INSURANCE_COMPANY_NF_RB"))
        {
          var field = GetField(export.HealthInsuranceCompany, "carrierCode");

          field.Error = true;
        }
        else if (IsExitState("HEALTH_INSURANCE_COVERAGE_AE_RB"))
        {
          var field9 =
            GetField(export.HealthInsuranceCoverage, "insurancePolicyNumber");

          field9.Error = true;

          var field10 =
            GetField(export.HealthInsuranceCoverage, "insuranceGroupNumber");

          field10.Error = true;
        }
        else
        {
        }

        break;
      case "DISPLAY":
        if (!IsEmpty(import.HcsePerson.Number) && !
          Equal(import.CsePerson.Number, import.HcsePerson.Number))
        {
          if (import.Hcontact.ContactNumber != 0 && import
            .Contact.ContactNumber != import.Hcontact.ContactNumber)
          {
            MoveContact2(local.RefreshContact, export.Contact);
            export.Hcontact.Assign(local.RefreshContact);
            export.SelectedContact.Assign(local.RefreshContact);
          }

          if (!IsEmpty(import.HhealthInsuranceCoverage.InsurancePolicyNumber) &&
            !
            Equal(import.HealthInsuranceCoverage.InsurancePolicyNumber,
            import.HhealthInsuranceCoverage.InsurancePolicyNumber))
          {
            MoveHealthInsuranceCompany2(local.RefreshHealthInsuranceCompany,
              export.HealthInsuranceCompany);
            export.HealthInsuranceCompanyAddress.Assign(
              local.RefreshHealthInsuranceCompanyAddress);
            export.HealthInsuranceCoverage.Assign(
              local.RefreshHealthInsuranceCoverage);
            export.HhealthInsuranceCompany.Assign(
              local.RefreshHealthInsuranceCompany);
            MoveHealthInsuranceCoverage2(local.RefreshHealthInsuranceCoverage,
              export.HhealthInsuranceCoverage);
          }
        }

        // ---------------------------------------------
        // On a display the user enteres the Person
        // Number and Case ID.  If the person is a
        // policy holder for more than one insurance
        // policy, the user will automatically be taken
        // to a list screen so that he can select the
        // insurance record that he wants to display.
        // ---------------------------------------------
        // ---------------------------------------------
        // Verify that all mandatory fields for a
        // display have been entered.
        // ---------------------------------------------
        // ---------------------------------------------
        // The CSE Person Number is required.
        // ---------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          export.CsePersonsWorkSet.FormattedName = "";
          export.CsePersonsWorkSet.Ssn = "";
        }
        else
        {
          UseOeCabCheckCaseMember1();

          switch(AsChar(local.WorkError.Flag))
          {
            case 'C':
              var field9 = GetField(export.Starting, "number");

              field9.Error = true;

              ExitState = "CASE_NF";

              break;
            case 'P':
              var field10 = GetField(export.CsePerson, "number");

              field10.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            case 'R':
              var field11 = GetField(export.CsePerson, "number");

              field11.Error = true;

              var field12 = GetField(export.Starting, "number");

              field12.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";

              break;
            default:
              break;
          }

          if (!IsEmpty(local.WorkError.Flag))
          {
            export.CsePersonsWorkSet.FormattedName = "";
            export.CsePersonsWorkSet.Ssn = "";
          }

          // IEF Blank when zero function is NOT working so this temporary patch
          // - kishor
          if (Equal(export.CsePersonsWorkSet.Ssn, "000000000"))
          {
            export.CsePersonsWorkSet.Ssn = "";
          }
        }

        // ---------------------------------------------
        // If the Contact person details for the CSE
        // Person are entered, validate the contact
        // details.
        // ---------------------------------------------
        if (export.Contact.ContactNumber != 0 || !
          IsEmpty(export.Contact.MiddleInitial) || !
          IsEmpty(export.Contact.NameFirst) || !
          IsEmpty(export.Contact.NameLast) || !
          IsEmpty(export.Contact.RelationshipToCsePerson))
        {
          if (export.Contact.ContactNumber == 0)
          {
            if (IsEmpty(export.Contact.NameFirst))
            {
              var field = GetField(export.Contact, "nameFirst");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Contact.NameLast))
            {
              var field = GetField(export.Contact, "nameLast");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Contact.RelationshipToCsePerson))
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          UseOeCabCheckPersonContact2();

          if (AsChar(export.WorkContactExist.Flag) != 'Y')
          {
            var field9 = GetField(export.Contact, "nameFirst");

            field9.Error = true;

            var field10 = GetField(export.Contact, "contactNumber");

            field10.Error = true;

            var field11 = GetField(export.Contact, "nameLast");

            field11.Error = true;

            var field12 = GetField(export.Contact, "middleInitial");

            field12.Error = true;

            var field13 = GetField(export.Contact, "relationshipToCsePerson");

            field13.Error = true;

            ExitState = "CONTACT_NF";
          }
        }

Test1:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveHealthInsuranceCompany2(local.RefreshHealthInsuranceCompany,
            export.HealthInsuranceCompany);
          export.HealthInsuranceCompanyAddress.Assign(
            local.RefreshHealthInsuranceCompanyAddress);
          export.HealthInsuranceCoverage.Assign(
            local.RefreshHealthInsuranceCoverage);
          export.HhealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          MoveHealthInsuranceCoverage2(local.RefreshHealthInsuranceCoverage,
            export.HhealthInsuranceCoverage);
          MoveContact2(local.RefreshContact, export.Contact);
          export.Hcontact.Assign(local.RefreshContact);

          return;
        }

        // ---------------------------------------------
        // Insert the USE statement here that calls the
        // READ action block.
        // ---------------------------------------------
        UseOeReadInsuranceCoverDetails();

        var field1 = GetField(export.Contact, "contactNumber");

        field1.Color = "";
        field1.Protected = false;

        var field2 = GetField(export.WorkPromptContact, "selectChar");

        field2.Color = "";
        field2.Protected = false;

        var field3 = GetField(export.Contact, "nameLast");

        field3.Color = "";
        field3.Protected = false;

        var field4 = GetField(export.Contact, "nameFirst");

        field4.Color = "";
        field4.Protected = false;

        var field5 = GetField(export.Contact, "middleInitial");

        field5.Color = "";
        field5.Protected = false;

        var field6 = GetField(export.Contact, "relationshipToCsePerson");

        field6.Color = "";
        field6.Protected = false;

        if (!IsEmpty(export.Employer.Name))
        {
          export.EmpProvidingInsurance.Flag = "Y";
        }

        if (IsExitState("MORE_THAN_ONE_INSURANCE_COVERAGE"))
        {
          // ************************************************
          // *If there is multiple insurance coverage then  *
          // *we must flow to the HIPL screen to select one.*
          // ************************************************
          export.Hcontact.Assign(export.Contact);
          export.HcsePerson.Number = export.CsePerson.Number;
          MoveHealthInsuranceCompany4(export.HealthInsuranceCompany,
            export.HhealthInsuranceCompany);
          MoveHealthInsuranceCoverage2(export.HealthInsuranceCoverage,
            export.HhealthInsuranceCoverage);
          export.WorkPromptInd.Flag = "Y";

          // ************************************************
          // *FLOW TO INSURANCE COVERAGE BY PERSON TO       *
          // *CARRIER
          // 
          // *
          // ************************************************
          ExitState = "ECO_LNK_TO_HIPL";
        }
        else if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          export.Hcontact.Assign(export.Contact);
          export.HcsePerson.Number = export.CsePerson.Number;
          MoveHealthInsuranceCompany4(export.HealthInsuranceCompany,
            export.HhealthInsuranceCompany);
          MoveHealthInsuranceCoverage2(export.HealthInsuranceCoverage,
            export.HhealthInsuranceCoverage);

          if (export.Contact.ContactNumber != 0)
          {
            var field11 = GetField(export.Contact, "contactNumber");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.WorkPromptContact, "selectChar");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Contact, "nameLast");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 = GetField(export.Contact, "nameFirst");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Contact, "middleInitial");

            field15.Color = "cyan";
            field15.Protected = true;

            UseOeVerifyContactRelationship();

            if (AsChar(local.CodeValueValid.Flag) == 'Y')
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Color = "cyan";
              field.Protected = true;
            }
            else
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";
            }

            var field16 = GetField(export.EmpProvidingInsurance, "flag");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 = GetField(export.Employer, "name");

            field17.Color = "cyan";
            field17.Protected = true;

            var field18 = GetField(export.PromptEmployer, "selectChar");

            field18.Color = "cyan";
            field18.Protected = true;
          }

          var field9 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.WorkPromptCarrier, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        if (!IsExitState("CSE_PERSON_NF"))
        {
          export.HcsePerson.Number = export.CsePerson.Number;
        }

        break;
      case "":
        break;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (import.HealthInsuranceCoverage.Identifier != import
          .HhealthInsuranceCoverage.Identifier || !
          Equal(import.CsePerson.Number, import.HcsePerson.Number) || import
          .HealthInsuranceCoverage.Identifier == 0 || IsEmpty
          (import.CsePerson.Number))
        {
          ExitState = "OE0013_DISP_REC_BEFORE_UPD";

          break;
        }

        // ---------------------------------------------
        // On an update the CSE Person No, and insurance
        // company cannot change.  Required fields are
        // the same as on an update.
        // ---------------------------------------------
        if (!Equal(import.CsePerson.Number, import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
        }

        // ---------------------------------------------
        // If the Contact person details for the CSE
        // Person are entered. If the contact does not
        // exist, CREATE one here.
        // ---------------------------------------------
        if (import.Contact.ContactNumber != 0 || !
          IsEmpty(import.Contact.MiddleInitial) || !
          IsEmpty(import.Contact.NameFirst) || !
          IsEmpty(import.Contact.NameLast) || !
          IsEmpty(import.Contact.RelationshipToCsePerson))
        {
          local.Contact.Flag = "Y";

          if (IsEmpty(import.Contact.NameFirst))
          {
            var field = GetField(export.Contact, "nameFirst");

            field.Error = true;

            local.Contact.Flag = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }

          if (IsEmpty(import.Contact.NameLast))
          {
            var field = GetField(export.Contact, "nameLast");

            field.Error = true;

            local.Contact.Flag = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }

          if (IsEmpty(import.Contact.RelationshipToCsePerson))
          {
            var field = GetField(export.Contact, "relationshipToCsePerson");

            field.Error = true;

            local.Contact.Flag = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }
          else
          {
            UseOeVerifyContactRelationship();

            if (AsChar(local.CodeValueValid.Flag) == 'Y')
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Color = "cyan";
              field.Protected = true;
            }
            else
            {
              var field = GetField(export.Contact, "relationshipToCsePerson");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";
            }
          }
        }

        if (IsEmpty(export.HealthInsuranceCoverage.InsurancePolicyNumber))
        {
          var field =
            GetField(export.HealthInsuranceCoverage, "insurancePolicyNumber");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INSURANCE_POLICY_NUMB_REQUIRED";
          }
        }

        if (IsEmpty(export.HealthInsuranceCoverage.CoverageCode1))
        {
          var field = GetField(export.HealthInsuranceCoverage, "coverageCode1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "1_COVERAGE_CODE_REQUIRED";
          }
        }
        else
        {
          local.Code.CodeName = "EDS COVERAGES";
          local.CodeValue.Cdvalue =
            export.HealthInsuranceCoverage.CoverageCode1 ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.WorkError.Count != 0)
          {
            var field =
              GetField(export.HealthInsuranceCoverage, "coverageCode1");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0195_INVALID_COVERAGE_CODE";
            }
          }

          if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode2))
          {
            local.CodeValue.Cdvalue =
              export.HealthInsuranceCoverage.CoverageCode2 ?? Spaces(10);
            UseCabValidateCodeValue();

            if (local.WorkError.Count != 0)
            {
              var field =
                GetField(export.HealthInsuranceCoverage, "coverageCode2");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "OE0195_INVALID_COVERAGE_CODE";
              }
            }

            if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode3))
            {
              local.CodeValue.Cdvalue =
                export.HealthInsuranceCoverage.CoverageCode3 ?? Spaces(10);
              UseCabValidateCodeValue();

              if (local.WorkError.Count != 0)
              {
                var field =
                  GetField(export.HealthInsuranceCoverage, "coverageCode3");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "OE0195_INVALID_COVERAGE_CODE";
                }
              }

              if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode4))
              {
                local.CodeValue.Cdvalue =
                  export.HealthInsuranceCoverage.CoverageCode4 ?? Spaces(10);
                UseCabValidateCodeValue();

                if (local.WorkError.Count != 0)
                {
                  var field =
                    GetField(export.HealthInsuranceCoverage, "coverageCode4");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "OE0195_INVALID_COVERAGE_CODE";
                  }
                }

                if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode5))
                {
                  local.CodeValue.Cdvalue =
                    export.HealthInsuranceCoverage.CoverageCode5 ?? Spaces(10);
                  UseCabValidateCodeValue();

                  if (local.WorkError.Count != 0)
                  {
                    var field =
                      GetField(export.HealthInsuranceCoverage, "coverageCode5");
                      

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "OE0195_INVALID_COVERAGE_CODE";
                    }
                  }

                  if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode6))
                  {
                    local.CodeValue.Cdvalue =
                      export.HealthInsuranceCoverage.CoverageCode6 ?? Spaces
                      (10);
                    UseCabValidateCodeValue();

                    if (local.WorkError.Count != 0)
                    {
                      var field =
                        GetField(export.HealthInsuranceCoverage, "coverageCode6");
                        

                      field.Error = true;

                      if (IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        ExitState = "OE0195_INVALID_COVERAGE_CODE";
                      }
                    }

                    if (!IsEmpty(export.HealthInsuranceCoverage.CoverageCode7))
                    {
                      local.CodeValue.Cdvalue =
                        export.HealthInsuranceCoverage.CoverageCode7 ?? Spaces
                        (10);
                      UseCabValidateCodeValue();

                      if (local.WorkError.Count != 0)
                      {
                        var field =
                          GetField(export.HealthInsuranceCoverage,
                          "coverageCode7");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "OE0195_INVALID_COVERAGE_CODE";
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }

        // *****************************************************************************
        // **     Effective Date edits.
        // *****************************************************************************
        if (Equal(export.HealthInsuranceCoverage.PolicyEffectiveDate,
          local.NullDateWorkArea.Date))
        {
          var field =
            GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }
        else
        {
          if (Lt(export.HealthInsuranceCoverage.PolicyEffectiveDate,
            local.Min.Date))
          {
            var field =
              GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_INVALID_DATE";
            }
          }

          if (Lt(Now().Date, export.HealthInsuranceCoverage.PolicyEffectiveDate))
            
          {
            var field =
              GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
            }
          }
        }

        if (Equal(export.HealthInsuranceCoverage.PolicyExpirationDate,
          local.NullDateWorkArea.Date))
        {
          export.HealthInsuranceCoverage.PolicyExpirationDate = local.Max.Date;
        }

        if (Lt(export.HealthInsuranceCoverage.PolicyExpirationDate,
          import.HealthInsuranceCoverage.PolicyEffectiveDate))
        {
          var field9 =
            GetField(export.HealthInsuranceCoverage, "policyExpirationDate");

          field9.Error = true;

          var field10 =
            GetField(export.HealthInsuranceCoverage, "policyEffectiveDate");

          field10.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CO0000_INVALID_EFF_OR_EXP_DATE";
          }
        }

        if (!Equal(import.HealthInsuranceCoverage.VerifiedDate, null) && Lt
          (import.HealthInsuranceCoverage.VerifiedDate,
          export.HealthInsuranceCoverage.PolicyEffectiveDate) || Lt
          (Now().Date, export.HealthInsuranceCoverage.VerifiedDate))
        {
          var field = GetField(export.HealthInsuranceCoverage, "verifiedDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0182_INVALID_VERIFIED_DATE";
          }
        }

        if (AsChar(export.EmpProvidingInsurance.Flag) == 'Y' || AsChar
          (export.EmpProvidingInsurance.Flag) == 'N' || IsEmpty
          (export.EmpProvidingInsurance.Flag))
        {
          // Continue
        }
        else
        {
          var field = GetField(export.EmpProvidingInsurance, "flag");

          field.Error = true;

          ExitState = "INVALID_INDICATOR_Y_N_SPACE";
        }

        // M Ashworth      12/10/02      WR20311 added link to incl when 
        // creating hic. Export contact last name is included in the if
        // statement. Users added business rule " If "other Policy Holder" field
        // is completed, then no Employer Name of yes/no indicator is needed.
        if (IsExitState("ACO_NN0000_ALL_OK") && IsEmpty
          (export.Contact.NameLast) && IsEmpty(export.Employer.Name))
        {
          if (AsChar(export.EmpProvidingInsurance.Flag) == 'Y' || AsChar
            (export.EmpProvidingInsurance.Flag) == 'N')
          {
            // Continue
          }
          else
          {
            ExitState = "IS_EMP_PROVIDING_HEALTH_INS";

            var field = GetField(export.EmpProvidingInsurance, "flag");

            field.Error = true;
          }

          if (AsChar(export.EmpProvidingInsurance.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";
          }
        }

        if (AsChar(export.SelectedIncomeSource.Type1) != 'M' && AsChar
          (export.SelectedIncomeSource.Type1) != 'E' && AsChar
          (export.EmpProvidingInsurance.Flag) == 'Y' && IsEmpty
          (export.Contact.NameLast))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.PromptEmployer, "selectChar");

            field.Error = true;

            ExitState = "INC_SOURCE_TYPE_OF_M_OR_E_ALLOWD";
          }
        }

        if (!Equal(import.HealthInsuranceCompany.CarrierCode,
          import.HhealthInsuranceCompany.CarrierCode))
        {
          var field9 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field9.Error = true;

          var field10 =
            GetField(export.HealthInsuranceCompany, "insurancePolicyCarrier");

          field10.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
          }
        }
        else
        {
          var field9 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.WorkPromptCarrier, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.Contact.Flag) == 'Y')
        {
          UseOeCabCheckPersonContact3();

          if (AsChar(export.WorkContactExist.Flag) != 'Y')
          {
            UseOePconCreateContactDetails();
          }
          else if (!Equal(export.Contact.RelationshipToCsePerson,
            export.Hcontact.RelationshipToCsePerson))
          {
            UseOeUpdateContactRelationship1();
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (IsEmpty(export.Employer.Name))
        {
          export.SelectedIncomeSource.Identifier = local.Blank.Identifier;
        }

        // ---------------------------------------------
        // Insert the USE statement here to call the
        // UPDATE action block.
        // ---------------------------------------------
        if (!Equal(export.HealthInsuranceCoverage.PolicyExpirationDate, null) &&
          !
          Equal(export.HealthInsuranceCoverage.PolicyExpirationDate,
          export.HhealthInsuranceCoverage.PolicyExpirationDate))
        {
          local.EndDateCoverage.Flag = "Y";
        }

        if (AsChar(export.EmpProvidingInsurance.Flag) == 'N' && !
          IsEmpty(export.Employer.Name))
        {
          local.DissasociateIncomeSource.Flag = "Y";
        }

        UseOeUpdateInsurancCovByHolder();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          var field9 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.WorkPromptCarrier, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;

          if (export.Contact.ContactNumber > 0)
          {
            var field11 = GetField(export.Contact, "contactNumber");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.WorkPromptContact, "selectChar");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Contact, "nameLast");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 = GetField(export.Contact, "nameFirst");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Contact, "middleInitial");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 = GetField(export.Contact, "relationshipToCsePerson");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 = GetField(export.EmpProvidingInsurance, "flag");

            field17.Color = "cyan";
            field17.Protected = true;

            var field18 = GetField(export.Employer, "name");

            field18.Color = "cyan";
            field18.Protected = true;

            var field19 = GetField(export.PromptEmployer, "selectChar");

            field19.Color = "cyan";
            field19.Protected = true;
          }

          if (AsChar(local.DissasociateIncomeSource.Flag) == 'Y')
          {
            export.Employer.Name = "";
            MoveIncomeSource(local.Blank, export.SelectedIncomeSource);
          }

          // ***************************************************************************
          // **    Build Infrastructure Row upon successful update
          // **    and coverage has been ended.
          // ***************************************************************************
          if (AsChar(local.EndDateCoverage.Flag) == 'Y')
          {
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.EventId = 80;
            local.Infrastructure.ReasonCode = "HLTHINSENDED";
            local.Infrastructure.BusinessObjectCd = "CAS";
            local.Infrastructure.CsePersonNumber = export.CsePerson.Number;
            local.Infrastructure.UserId = global.UserId;
            local.Infrastructure.Detail =
              (export.HealthInsuranceCompany.CarrierCode ?? "") + " " + TrimEnd
              (export.HealthInsuranceCompany.InsurancePolicyCarrier) + " Company ended on " +
              NumberToString
              (DateToInt(export.HealthInsuranceCoverage.PolicyExpirationDate),
              8, 8);
            ExitState = "ACO_NN0000_ALL_OK";

            // 09/23/10 GVandy CQ515 Policy termination events error if the 
            // infrastructure record does not contain a case number.
            if (!IsEmpty(export.Starting.Number))
            {
              local.Infrastructure.CaseNumber = export.Starting.Number;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }
            }

            // ***************************************************************************
            // **    Build Infrastructure Row for all other cases bene tied.
            // **    as AR or AP.
            // ***************************************************************************
            foreach(var item in ReadCaseRoleCase())
            {
              if (Equal(entities.Case1.Number, export.Starting.Number))
              {
                continue;
              }

              local.Infrastructure.CaseNumber = entities.Case1.Number;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test2;
              }
            }
          }
        }
        else if (IsExitState("HEALTH_INSURANCE_COVERAGE_AE_RB"))
        {
          var field9 =
            GetField(export.HealthInsuranceCoverage, "insurancePolicyNumber");

          field9.Error = true;

          var field10 =
            GetField(export.HealthInsuranceCoverage, "insuranceGroupNumber");

          field10.Error = true;
        }
        else
        {
        }

        local.EndDateCoverage.Flag = "";
        local.DissasociateIncomeSource.Flag = "";

        break;
      case "DELETE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the delete can take place.
        // ---------------------------------------------
        if (import.HealthInsuranceCoverage.Identifier != import
          .HhealthInsuranceCoverage.Identifier || !
          Equal(import.CsePerson.Number, import.HcsePerson.Number) || import
          .HealthInsuranceCoverage.Identifier == 0 || IsEmpty
          (import.CsePerson.Number))
        {
          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          break;
        }

        UseOeDeleteHealthInsPolicy();

        if (IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
        {
          MoveContact2(local.RefreshContact, export.Contact);
          export.Hcontact.Assign(local.RefreshContact);
          export.SelectedContact.Assign(local.RefreshContact);
          MoveHealthInsuranceCompany2(local.RefreshHealthInsuranceCompany,
            export.HealthInsuranceCompany);
          export.HealthInsuranceCompanyAddress.Assign(
            local.RefreshHealthInsuranceCompanyAddress);
          export.HealthInsuranceCoverage.Assign(
            local.RefreshHealthInsuranceCoverage);
          export.HhealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          MoveHealthInsuranceCoverage2(local.RefreshHealthInsuranceCoverage,
            export.HhealthInsuranceCoverage);
          export.EmpProvidingInsurance.Flag = "";
          export.Employer.Name = "";
        }
        else
        {
          var field9 = GetField(export.HealthInsuranceCompany, "carrierCode");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.WorkPromptCarrier, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;
        }

        break;
      default:
        var field7 = GetField(export.HealthInsuranceCompany, "carrierCode");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.WorkPromptCarrier, "selectChar");

        field8.Color = "cyan";
        field8.Protected = true;

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

Test2:

    if (Equal(export.HealthInsuranceCoverage.PolicyExpirationDate,
      local.Max.Date))
    {
      export.HealthInsuranceCoverage.PolicyExpirationDate =
        local.NullDateWorkArea.Date;
    }

    global.Command = "";
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveContact1(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
  }

  private static void MoveContact2(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveContact3(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveHealthInsuranceCompany1(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.InsurerPhoneAreaCode = source.InsurerPhoneAreaCode;
    target.InsurerFaxAreaCode = source.InsurerFaxAreaCode;
    target.InsurerFaxExt = source.InsurerFaxExt;
    target.InsurerPhoneExt = source.InsurerPhoneExt;
    target.Identifier = source.Identifier;
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
    target.InsurerPhone = source.InsurerPhone;
    target.InsurerFax = source.InsurerFax;
  }

  private static void MoveHealthInsuranceCompany2(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.InsurerPhoneAreaCode = source.InsurerPhoneAreaCode;
    target.InsurerFaxAreaCode = source.InsurerFaxAreaCode;
    target.InsurerPhoneExt = source.InsurerPhoneExt;
    target.Identifier = source.Identifier;
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
    target.InsurerPhone = source.InsurerPhone;
    target.InsurerFax = source.InsurerFax;
  }

  private static void MoveHealthInsuranceCompany3(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.InsurerPhoneAreaCode = source.InsurerPhoneAreaCode;
    target.InsurerFaxAreaCode = source.InsurerFaxAreaCode;
    target.InsurerPhoneExt = source.InsurerPhoneExt;
    target.Identifier = source.Identifier;
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
    target.InsurerPhone = source.InsurerPhone;
    target.InsurerFax = source.InsurerFax;
    target.EndDate = source.EndDate;
  }

  private static void MoveHealthInsuranceCompany4(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.Identifier = source.Identifier;
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
  }

  private static void MoveHealthInsuranceCompanyAddress(
    HealthInsuranceCompanyAddress source, HealthInsuranceCompanyAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
  }

  private static void MoveHealthInsuranceCoverage1(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.VerifiedDate = source.VerifiedDate;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
    target.CoverageCode1 = source.CoverageCode1;
    target.CoverageCode2 = source.CoverageCode2;
    target.CoverageCode3 = source.CoverageCode3;
    target.CoverageCode4 = source.CoverageCode4;
    target.CoverageCode5 = source.CoverageCode5;
    target.CoverageCode6 = source.CoverageCode6;
    target.CoverageCode7 = source.CoverageCode7;
    target.PolicyEffectiveDate = source.PolicyEffectiveDate;
    target.OtherCoveredPersons = source.OtherCoveredPersons;
  }

  private static void MoveHealthInsuranceCoverage2(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.WorkError.Count = useExport.ReturnCode.Count;
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

  private void UseOeCabCheckCaseMember1()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = local.SearchCsePerson.Number;
    useImport.Case1.Number = export.Starting.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckCaseMember2()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Starting.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckCaseMember3()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckPersonContact1()
  {
    var useImport = new OeCabCheckPersonContact.Import();
    var useExport = new OeCabCheckPersonContact.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Contact.Assign(import.Contact);

    Call(OeCabCheckPersonContact.Execute, useImport, useExport);

    MoveContact1(useExport.Contact, local.Found);
    export.WorkContactExist.Flag = useExport.ContactExist.Flag;
  }

  private void UseOeCabCheckPersonContact2()
  {
    var useImport = new OeCabCheckPersonContact.Import();
    var useExport = new OeCabCheckPersonContact.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Contact.Assign(import.Contact);

    Call(OeCabCheckPersonContact.Execute, useImport, useExport);

    MoveContact3(useExport.Contact, export.Contact);
    export.WorkContactExist.Flag = useExport.ContactExist.Flag;
  }

  private void UseOeCabCheckPersonContact3()
  {
    var useImport = new OeCabCheckPersonContact.Import();
    var useExport = new OeCabCheckPersonContact.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Contact.Assign(import.Contact);

    Call(OeCabCheckPersonContact.Execute, useImport, useExport);

    export.WorkContactExist.Flag = useExport.ContactExist.Flag;
  }

  private void UseOeCreateInsuranceCoverHolder()
  {
    var useImport = new OeCreateInsuranceCoverHolder.Import();
    var useExport = new OeCreateInsuranceCoverHolder.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.IncomeSource.Identifier = export.SelectedIncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Contact.Assign(export.Contact);
    MoveHealthInsuranceCoverage1(export.HealthInsuranceCoverage,
      useImport.HealthInsuranceCoverage);
    useImport.HealthInsuranceCompany.Assign(export.HealthInsuranceCompany);

    Call(OeCreateInsuranceCoverHolder.Execute, useImport, useExport);

    export.CsePerson.Number = useExport.CsePerson.Number;
    MoveContact2(useExport.Contact, export.Contact);
    export.HealthInsuranceCoverage.Assign(useExport.HealthInsuranceCoverage);
    MoveHealthInsuranceCompany1(useExport.HealthInsuranceCompany,
      export.HealthInsuranceCompany);
  }

  private void UseOeDeleteHealthInsPolicy()
  {
    var useImport = new OeDeleteHealthInsPolicy.Import();
    var useExport = new OeDeleteHealthInsPolicy.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.HealthInsuranceCoverage.Identifier =
      export.HealthInsuranceCoverage.Identifier;
    useImport.HealthInsuranceCompany.CarrierCode =
      export.HealthInsuranceCompany.CarrierCode;

    Call(OeDeleteHealthInsPolicy.Execute, useImport, useExport);
  }

  private void UseOePconCreateContactDetails()
  {
    var useImport = new OePconCreateContactDetails.Import();
    var useExport = new OePconCreateContactDetails.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveContact2(import.Contact, useImport.Contact);

    Call(OePconCreateContactDetails.Execute, useImport, useExport);

    MoveContact2(useExport.Contact, export.Contact);
  }

  private void UseOeReadHealthInsCompany()
  {
    var useImport = new OeReadHealthInsCompany.Import();
    var useExport = new OeReadHealthInsCompany.Export();

    useImport.HealthInsuranceCompany.Assign(export.HealthInsuranceCompany);

    Call(OeReadHealthInsCompany.Execute, useImport, useExport);

    export.HealthInsuranceCompany.Assign(useExport.HealthInsuranceCompany);
    MoveHealthInsuranceCompanyAddress(useExport.HealthInsuranceCompanyAddress,
      export.HealthInsuranceCompanyAddress);
  }

  private void UseOeReadInsuranceCoverDetails()
  {
    var useImport = new OeReadInsuranceCoverDetails.Import();
    var useExport = new OeReadInsuranceCoverDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Contact.Assign(export.Contact);
    useImport.HealthInsuranceCoverage.Identifier =
      export.HealthInsuranceCoverage.Identifier;

    Call(OeReadInsuranceCoverDetails.Execute, useImport, useExport);

    export.Employer.Name = useExport.Employer.Name;
    MoveIncomeSource(useExport.IncomeSource, export.SelectedIncomeSource);
    export.CsePerson.Number = useExport.CsePerson.Number;
    MoveContact2(useExport.Contact, export.Contact);
    export.HealthInsuranceCoverage.Assign(useExport.HealthInsuranceCoverage);
    MoveHealthInsuranceCompany1(useExport.HealthInsuranceCompany,
      export.HealthInsuranceCompany);
    export.HealthInsuranceCompanyAddress.Assign(
      useExport.HealthInsuranceCompanyAddress);
  }

  private void UseOeUpdateContactRelationship1()
  {
    var useImport = new OeUpdateContactRelationship.Import();
    var useExport = new OeUpdateContactRelationship.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveContact1(export.Contact, useImport.Contact);

    Call(OeUpdateContactRelationship.Execute, useImport, useExport);
  }

  private void UseOeUpdateContactRelationship2()
  {
    var useImport = new OeUpdateContactRelationship.Import();
    var useExport = new OeUpdateContactRelationship.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveContact1(export.Contact, useImport.Contact);

    Call(OeUpdateContactRelationship.Execute, useImport, useExport);
  }

  private void UseOeUpdateInsurancCovByHolder()
  {
    var useImport = new OeUpdateInsurancCovByHolder.Import();
    var useExport = new OeUpdateInsurancCovByHolder.Export();

    useImport.EndDateCoverage.Flag = local.EndDateCoverage.Flag;
    useImport.DissasociateIncomeSourc.Flag =
      local.DissasociateIncomeSource.Flag;
    useImport.IncomeSource.Identifier = export.SelectedIncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Contact.Assign(export.Contact);
    MoveHealthInsuranceCoverage1(export.HealthInsuranceCoverage,
      useImport.HealthInsuranceCoverage);

    Call(OeUpdateInsurancCovByHolder.Execute, useImport, useExport);

    export.HealthInsuranceCoverage.Assign(useExport.HealthInsuranceCoverage);
  }

  private void UseOeVerifyContactRelationship()
  {
    var useImport = new OeVerifyContactRelationship.Import();
    var useExport = new OeVerifyContactRelationship.Export();

    useImport.Contact.RelationshipToCsePerson =
      export.Contact.RelationshipToCsePerson;

    Call(OeVerifyContactRelationship.Execute, useImport, useExport);

    local.CodeValueValid.Flag = useExport.CodeValueValid.Flag;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    MoveNextTranInfo(useExport.NextTranInfo, export.Hidden);
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

    useImport.Case1.Number = import.Starting.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate",
          export.HealthInsuranceCoverage.PolicyExpirationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadEmployerIncomeSource()
  {
    entities.IncomeSource.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SelectedIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 2);
        entities.IncomeSource.Type1 = db.GetString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.IncomeSource.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
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
    /// A value of SelectedIncomeSource.
    /// </summary>
    [JsonPropertyName("selectedIncomeSource")]
    public IncomeSource SelectedIncomeSource
    {
      get => selectedIncomeSource ??= new();
      set => selectedIncomeSource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmpProvidingInsurance.
    /// </summary>
    [JsonPropertyName("empProvidingInsurance")]
    public Common EmpProvidingInsurance
    {
      get => empProvidingInsurance ??= new();
      set => empProvidingInsurance = value;
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
    /// A value of PromptRelToChild.
    /// </summary>
    [JsonPropertyName("promptRelToChild")]
    public Common PromptRelToChild
    {
      get => promptRelToChild ??= new();
      set => promptRelToChild = value;
    }

    /// <summary>
    /// A value of PromptEmployer.
    /// </summary>
    [JsonPropertyName("promptEmployer")]
    public Common PromptEmployer
    {
      get => promptEmployer ??= new();
      set => promptEmployer = value;
    }

    /// <summary>
    /// A value of PromptCode1.
    /// </summary>
    [JsonPropertyName("promptCode1")]
    public Common PromptCode1
    {
      get => promptCode1 ??= new();
      set => promptCode1 = value;
    }

    /// <summary>
    /// A value of PromptCode2.
    /// </summary>
    [JsonPropertyName("promptCode2")]
    public Common PromptCode2
    {
      get => promptCode2 ??= new();
      set => promptCode2 = value;
    }

    /// <summary>
    /// A value of PromptCode3.
    /// </summary>
    [JsonPropertyName("promptCode3")]
    public Common PromptCode3
    {
      get => promptCode3 ??= new();
      set => promptCode3 = value;
    }

    /// <summary>
    /// A value of PromptCode4.
    /// </summary>
    [JsonPropertyName("promptCode4")]
    public Common PromptCode4
    {
      get => promptCode4 ??= new();
      set => promptCode4 = value;
    }

    /// <summary>
    /// A value of PromptCode5.
    /// </summary>
    [JsonPropertyName("promptCode5")]
    public Common PromptCode5
    {
      get => promptCode5 ??= new();
      set => promptCode5 = value;
    }

    /// <summary>
    /// A value of PromptCode6.
    /// </summary>
    [JsonPropertyName("promptCode6")]
    public Common PromptCode6
    {
      get => promptCode6 ??= new();
      set => promptCode6 = value;
    }

    /// <summary>
    /// A value of PromptCode7.
    /// </summary>
    [JsonPropertyName("promptCode7")]
    public Common PromptCode7
    {
      get => promptCode7 ??= new();
      set => promptCode7 = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Hcase.
    /// </summary>
    [JsonPropertyName("hcase")]
    public Case1 Hcase
    {
      get => hcase ??= new();
      set => hcase = value;
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
    /// A value of HcsePerson.
    /// </summary>
    [JsonPropertyName("hcsePerson")]
    public CsePerson HcsePerson
    {
      get => hcsePerson ??= new();
      set => hcsePerson = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of Hcontact.
    /// </summary>
    [JsonPropertyName("hcontact")]
    public Contact Hcontact
    {
      get => hcontact ??= new();
      set => hcontact = value;
    }

    /// <summary>
    /// A value of SelectedContact.
    /// </summary>
    [JsonPropertyName("selectedContact")]
    public Contact SelectedContact
    {
      get => selectedContact ??= new();
      set => selectedContact = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HhealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("hhealthInsuranceCoverage")]
    public HealthInsuranceCoverage HhealthInsuranceCoverage
    {
      get => hhealthInsuranceCoverage ??= new();
      set => hhealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCoverage")]
    public HealthInsuranceCoverage SelectedHealthInsuranceCoverage
    {
      get => selectedHealthInsuranceCoverage ??= new();
      set => selectedHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HhealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("hhealthInsuranceCompany")]
    public HealthInsuranceCompany HhealthInsuranceCompany
    {
      get => hhealthInsuranceCompany ??= new();
      set => hhealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedHealthInsuranceCompany
    {
      get => selectedHealthInsuranceCompany ??= new();
      set => selectedHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress SelectedHealthInsuranceCompanyAddress
    {
      get => selectedHealthInsuranceCompanyAddress ??= new();
      set => selectedHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of SelectionCount.
    /// </summary>
    [JsonPropertyName("selectionCount")]
    public Common SelectionCount
    {
      get => selectionCount ??= new();
      set => selectionCount = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// A value of WorkPromptInd.
    /// </summary>
    [JsonPropertyName("workPromptInd")]
    public Common WorkPromptInd
    {
      get => workPromptInd ??= new();
      set => workPromptInd = value;
    }

    /// <summary>
    /// A value of WorkContactExist.
    /// </summary>
    [JsonPropertyName("workContactExist")]
    public Common WorkContactExist
    {
      get => workContactExist ??= new();
      set => workContactExist = value;
    }

    /// <summary>
    /// A value of WorkPromptCoverage.
    /// </summary>
    [JsonPropertyName("workPromptCoverage")]
    public Common WorkPromptCoverage
    {
      get => workPromptCoverage ??= new();
      set => workPromptCoverage = value;
    }

    /// <summary>
    /// A value of WorkPromptContact.
    /// </summary>
    [JsonPropertyName("workPromptContact")]
    public Common WorkPromptContact
    {
      get => workPromptContact ??= new();
      set => workPromptContact = value;
    }

    /// <summary>
    /// A value of WorkPromptCarrier.
    /// </summary>
    [JsonPropertyName("workPromptCarrier")]
    public Common WorkPromptCarrier
    {
      get => workPromptCarrier ??= new();
      set => workPromptCarrier = value;
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
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    private IncomeSource selectedIncomeSource;
    private Employer employer;
    private Common empProvidingInsurance;
    private CodeValue codeValue;
    private Common promptRelToChild;
    private Common promptEmployer;
    private Common promptCode1;
    private Common promptCode2;
    private Common promptCode3;
    private Common promptCode4;
    private Common promptCode5;
    private Common promptCode6;
    private Common promptCode7;
    private Case1 starting;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Case1 hcase;
    private CsePerson csePerson;
    private CsePerson hcsePerson;
    private Contact contact;
    private Contact hcontact;
    private Contact selectedContact;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCoverage hhealthInsuranceCoverage;
    private HealthInsuranceCoverage selectedHealthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompany hhealthInsuranceCompany;
    private HealthInsuranceCompany selectedHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompanyAddress selectedHealthInsuranceCompanyAddress;
    private Common selectionCount;
    private Common csePersonPrompt;
    private Common workPromptInd;
    private Common workContactExist;
    private Common workPromptCoverage;
    private Common workPromptContact;
    private Common workPromptCarrier;
    private NextTranInfo hidden;
    private CsePerson selectedCsePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of SelectedIncomeSource.
    /// </summary>
    [JsonPropertyName("selectedIncomeSource")]
    public IncomeSource SelectedIncomeSource
    {
      get => selectedIncomeSource ??= new();
      set => selectedIncomeSource = value;
    }

    /// <summary>
    /// A value of EmpProvidingInsurance.
    /// </summary>
    [JsonPropertyName("empProvidingInsurance")]
    public Common EmpProvidingInsurance
    {
      get => empProvidingInsurance ??= new();
      set => empProvidingInsurance = value;
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
    /// A value of PromptRelToChild.
    /// </summary>
    [JsonPropertyName("promptRelToChild")]
    public Common PromptRelToChild
    {
      get => promptRelToChild ??= new();
      set => promptRelToChild = value;
    }

    /// <summary>
    /// A value of PromptEmployer.
    /// </summary>
    [JsonPropertyName("promptEmployer")]
    public Common PromptEmployer
    {
      get => promptEmployer ??= new();
      set => promptEmployer = value;
    }

    /// <summary>
    /// A value of PromptCode1.
    /// </summary>
    [JsonPropertyName("promptCode1")]
    public Common PromptCode1
    {
      get => promptCode1 ??= new();
      set => promptCode1 = value;
    }

    /// <summary>
    /// A value of PromptCode2.
    /// </summary>
    [JsonPropertyName("promptCode2")]
    public Common PromptCode2
    {
      get => promptCode2 ??= new();
      set => promptCode2 = value;
    }

    /// <summary>
    /// A value of PromptCode3.
    /// </summary>
    [JsonPropertyName("promptCode3")]
    public Common PromptCode3
    {
      get => promptCode3 ??= new();
      set => promptCode3 = value;
    }

    /// <summary>
    /// A value of PromptCode4.
    /// </summary>
    [JsonPropertyName("promptCode4")]
    public Common PromptCode4
    {
      get => promptCode4 ??= new();
      set => promptCode4 = value;
    }

    /// <summary>
    /// A value of PromptCode5.
    /// </summary>
    [JsonPropertyName("promptCode5")]
    public Common PromptCode5
    {
      get => promptCode5 ??= new();
      set => promptCode5 = value;
    }

    /// <summary>
    /// A value of PromptCode6.
    /// </summary>
    [JsonPropertyName("promptCode6")]
    public Common PromptCode6
    {
      get => promptCode6 ??= new();
      set => promptCode6 = value;
    }

    /// <summary>
    /// A value of PromptCode7.
    /// </summary>
    [JsonPropertyName("promptCode7")]
    public Common PromptCode7
    {
      get => promptCode7 ??= new();
      set => promptCode7 = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Hcase.
    /// </summary>
    [JsonPropertyName("hcase")]
    public Case1 Hcase
    {
      get => hcase ??= new();
      set => hcase = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of HcsePerson.
    /// </summary>
    [JsonPropertyName("hcsePerson")]
    public CsePerson HcsePerson
    {
      get => hcsePerson ??= new();
      set => hcsePerson = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of Hcontact.
    /// </summary>
    [JsonPropertyName("hcontact")]
    public Contact Hcontact
    {
      get => hcontact ??= new();
      set => hcontact = value;
    }

    /// <summary>
    /// A value of SelectedContact.
    /// </summary>
    [JsonPropertyName("selectedContact")]
    public Contact SelectedContact
    {
      get => selectedContact ??= new();
      set => selectedContact = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HhealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("hhealthInsuranceCoverage")]
    public HealthInsuranceCoverage HhealthInsuranceCoverage
    {
      get => hhealthInsuranceCoverage ??= new();
      set => hhealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCoverage")]
    public HealthInsuranceCoverage SelectedHealthInsuranceCoverage
    {
      get => selectedHealthInsuranceCoverage ??= new();
      set => selectedHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HhealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("hhealthInsuranceCompany")]
    public HealthInsuranceCompany HhealthInsuranceCompany
    {
      get => hhealthInsuranceCompany ??= new();
      set => hhealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedHealthInsuranceCompany
    {
      get => selectedHealthInsuranceCompany ??= new();
      set => selectedHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress SelectedHealthInsuranceCompanyAddress
    {
      get => selectedHealthInsuranceCompanyAddress ??= new();
      set => selectedHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// A value of WorkPromptInd.
    /// </summary>
    [JsonPropertyName("workPromptInd")]
    public Common WorkPromptInd
    {
      get => workPromptInd ??= new();
      set => workPromptInd = value;
    }

    /// <summary>
    /// A value of WorkContactExist.
    /// </summary>
    [JsonPropertyName("workContactExist")]
    public Common WorkContactExist
    {
      get => workContactExist ??= new();
      set => workContactExist = value;
    }

    /// <summary>
    /// A value of WorkPromptCoverage.
    /// </summary>
    [JsonPropertyName("workPromptCoverage")]
    public Common WorkPromptCoverage
    {
      get => workPromptCoverage ??= new();
      set => workPromptCoverage = value;
    }

    /// <summary>
    /// A value of WorkPromptContact.
    /// </summary>
    [JsonPropertyName("workPromptContact")]
    public Common WorkPromptContact
    {
      get => workPromptContact ??= new();
      set => workPromptContact = value;
    }

    /// <summary>
    /// A value of WorkPromptCarrier.
    /// </summary>
    [JsonPropertyName("workPromptCarrier")]
    public Common WorkPromptCarrier
    {
      get => workPromptCarrier ??= new();
      set => workPromptCarrier = value;
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
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of PassToHicl.
    /// </summary>
    [JsonPropertyName("passToHicl")]
    public Standard PassToHicl
    {
      get => passToHicl ??= new();
      set => passToHicl = value;
    }

    private Employer employer;
    private IncomeSource selectedIncomeSource;
    private Common empProvidingInsurance;
    private Code code;
    private Common promptRelToChild;
    private Common promptEmployer;
    private Common promptCode1;
    private Common promptCode2;
    private Common promptCode3;
    private Common promptCode4;
    private Common promptCode5;
    private Common promptCode6;
    private Common promptCode7;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Case1 hcase;
    private Case1 starting;
    private CsePerson csePerson;
    private CsePerson hcsePerson;
    private Contact contact;
    private Contact hcontact;
    private Contact selectedContact;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCoverage hhealthInsuranceCoverage;
    private HealthInsuranceCoverage selectedHealthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompany hhealthInsuranceCompany;
    private HealthInsuranceCompany selectedHealthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompanyAddress selectedHealthInsuranceCompanyAddress;
    private Common csePersonPrompt;
    private Common workPromptInd;
    private Common workContactExist;
    private Common workPromptCoverage;
    private Common workPromptContact;
    private Common workPromptCarrier;
    private NextTranInfo hidden;
    private CsePerson selectedCsePerson;
    private Standard passToHicl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullContact.
    /// </summary>
    [JsonPropertyName("nullContact")]
    public Contact NullContact
    {
      get => nullContact ??= new();
      set => nullContact = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public DateWorkArea Min
    {
      get => min ??= new();
      set => min = value;
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
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Contact Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of CodeValueValid.
    /// </summary>
    [JsonPropertyName("codeValueValid")]
    public Common CodeValueValid
    {
      get => codeValueValid ??= new();
      set => codeValueValid = value;
    }

    /// <summary>
    /// A value of EndDateCoverage.
    /// </summary>
    [JsonPropertyName("endDateCoverage")]
    public Common EndDateCoverage
    {
      get => endDateCoverage ??= new();
      set => endDateCoverage = value;
    }

    /// <summary>
    /// A value of CountLoops.
    /// </summary>
    [JsonPropertyName("countLoops")]
    public Common CountLoops
    {
      get => countLoops ??= new();
      set => countLoops = value;
    }

    /// <summary>
    /// A value of DissasociateIncomeSource.
    /// </summary>
    [JsonPropertyName("dissasociateIncomeSource")]
    public Common DissasociateIncomeSource
    {
      get => dissasociateIncomeSource ??= new();
      set => dissasociateIncomeSource = value;
    }

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
    /// A value of SaveCoverageCode.
    /// </summary>
    [JsonPropertyName("saveCoverageCode")]
    public TextWorkArea SaveCoverageCode
    {
      get => saveCoverageCode ??= new();
      set => saveCoverageCode = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public IncomeSource Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Common Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    /// <summary>
    /// A value of RefreshName.
    /// </summary>
    [JsonPropertyName("refreshName")]
    public OeWorkGroup RefreshName
    {
      get => refreshName ??= new();
      set => refreshName = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress RefreshHealthInsuranceCompanyAddress
    {
      get => refreshHealthInsuranceCompanyAddress ??= new();
      set => refreshHealthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of RefreshContact.
    /// </summary>
    [JsonPropertyName("refreshContact")]
    public Contact RefreshContact
    {
      get => refreshContact ??= new();
      set => refreshContact = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCompany")]
    public HealthInsuranceCompany RefreshHealthInsuranceCompany
    {
      get => refreshHealthInsuranceCompany ??= new();
      set => refreshHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCoverage")]
    public HealthInsuranceCoverage RefreshHealthInsuranceCoverage
    {
      get => refreshHealthInsuranceCoverage ??= new();
      set => refreshHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of RefreshCsePerson.
    /// </summary>
    [JsonPropertyName("refreshCsePerson")]
    public CsePerson RefreshCsePerson
    {
      get => refreshCsePerson ??= new();
      set => refreshCsePerson = value;
    }

    /// <summary>
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    private Contact nullContact;
    private Infrastructure infrastructure;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea min;
    private DateWorkArea max;
    private Contact found;
    private Common codeValueValid;
    private Common endDateCoverage;
    private Common countLoops;
    private Common dissasociateIncomeSource;
    private Common count;
    private TextWorkArea saveCoverageCode;
    private IncomeSource blank;
    private Code code;
    private CodeValue codeValue;
    private Common contact;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
    private Common promptCount;
    private OeWorkGroup refreshName;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private HealthInsuranceCompanyAddress refreshHealthInsuranceCompanyAddress;
    private Contact refreshContact;
    private HealthInsuranceCompany refreshHealthInsuranceCompany;
    private Case1 refreshCase;
    private HealthInsuranceCoverage refreshHealthInsuranceCoverage;
    private CsePerson refreshCsePerson;
    private Common workError;
    private TextWorkArea textWorkArea;
    private HealthInsuranceCompany healthInsuranceCompany;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private IncomeSource incomeSource;
    private Employer employer;
  }
#endregion
}
