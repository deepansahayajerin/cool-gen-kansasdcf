// Program: OE_RESO_PERSON_RESOURCE, ID: 371817404, model: 746.
// Short name: SWERESOP
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
/// A program: OE_RESO_PERSON_RESOURCE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Provides for adding or changing information relating to the AP or AR's 
/// resources.  This is used in the Locate function and also in the
/// determination of support requirements.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeResoPersonResource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RESO_PERSON_RESOURCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeResoPersonResource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeResoPersonResource.
  /// </summary>
  public OeResoPersonResource(IContext context, Import import, Export export):
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
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // unknown   	MM/DD/YY	Initial Code
    // T.O.Redmond	12/28/95        Add Next Tran Logic
    // T.O.Redmond	02/01/96	Add INCS/Signoff
    // T.O.Redmond	02/06/96	Retrofit
    // G.Lofton	02/29/96	Made corrections to problems.
    // G.Lofton	03/18/96	Added logic to associate and
    // 			        disassociate external agency.
    // Siraj Konkader	11 June 96  	Add print
    // Regan Welborn   24 June 96  	Add line to make Lien Holder
    //                                 
    // St of Kansas ind default to "N"
    // R. Marchman	11 November 96  Add new security and next tran.
    // Sid		1/2/97		Fixes to flows with INCS & CARS.
    // Sid		3/20/97		IDCR # 311 - Add Other Lien Placed
    // 				and Removed dates.
    // M Ramirez	12/30/1998	Revised print process.
    // M Ramirez	12/30/1998	Changed security to check CRUD
    // 			        actions only.                       S Johnson       03/01/1999
    // Cleared views upon command RETCARS
    // 			        when different id number.
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // 04/04/00 W.Campbell            Disabled existing call to
    //                                
    // Security Cab and added a
    //                                
    // new call with view matching
    //                                
    // changed to match the export
    //                                
    // views of case and cse_person.
    //                                
    // Work done on WR#000162
    //                                
    // for PRWORA - Family Violence.
    // 04/25/01      Madhu Kumar      Edit checks for 4 and 5
    // 
    // digit zip code  .
    // -------------------------------------------------------------------------------------
    // 01/02/2002             Vitha Madhira      PR # 00135378.
    // Fixed 'Resource Type'  field edit. Now the screen will not accept invalid
    // 'Resource Type'.
    // -------------------------------------------------------------------------------------
    // 01/03/02       Vithal Madhira              PR# 121249 Segment-D
    // Fixed the code for Family violence. If the CSE_Person has 
    // family_violence, do not display the locate (address)  and employer info.
    // --------------------------------------------------------------------------------------
    // ******* END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.CsePerson.Number = import.CsePerson.Number;
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    UseOeCabSetMnemonics();
    local.InitialisedToMaxDate.Date = local.Max.ExpirationDate;
    local.NullDate.Date = null;
    export.HiddenPrevUserAction.Command = global.Command;

    // ************************************************
    // *F10-Delete will delete the resource and       *
    // *address records if they have been displayed   *
    // *first and will disassociate any vehicle record*
    // *Processing is done here instead of in the CASE*
    // *OF COMMAND because of not needing to move the *
    // *export views.
    // 
    // *
    // ************************************************
    // ---------------------------------------------
    // Move imports to exports.
    // ---------------------------------------------
    export.FromIncomeSource.Identifier = import.FromIncomeSource.Identifier;
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;
    export.ListLienHolderStates.PromptField =
      import.ListLienHolderStates.PromptField;
    export.ListResLocnAddrStates.PromptField =
      import.ListResLocnAddrStates.PromptField;
    export.ListResourceTypes.PromptField = import.ListResourceTypes.PromptField;
    export.ListExternalAgencies.PromptField =
      import.ListExternalAgencies.PromptField;
    MoveExternalAgency(import.ExternalAgency, export.ExternalAgency);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.CsePersonResource.Assign(import.CsePersonResource);
    export.CsePersonVehicle.Identifier = import.CsePersonVehicle.Identifier;
    export.ResourceLocationAddress.Assign(import.ResourceLocationAddress);
    export.ResourceLienHolderAddress.Assign(import.ResourceLienHolderAddress);
    export.HiddenPreviousCsePerson.Number =
      import.HiddenPreviousCsePerson.Number;
    MoveCsePersonResource1(import.HiddenPreviousCsePersonResource,
      export.HiddenPreviousCsePersonResource);
    export.HiddenPreviousExternalAgency.Identifier =
      import.HiddenPreviousExternalAgency.Identifier;
    export.HiddenDisplaySuccessful.Flag = import.HiddenDisplaySuccessful.Flag;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.CseActionCodeDesc.Description = import.CseActionCodeDesc.Description;
    export.LegalActionLienType.Description =
      import.LegalActionLienType.Description;
    export.LegalActionPersonResource.Assign(import.LegalActionPersonResource);
    MoveCsePersonResource2(import.LastUpdated, export.LastUpdated);
    export.ResourceTypeDesc.Description = import.ResourceTypeDesc.Description;
    export.HiddenSelectedCsePersonVehicle.Identifier =
      import.HiddenSelectedCsePersonVehicle.Identifier;
    MoveExternalAgency(import.HiddenSelectedExternalAgency,
      export.HiddenSelectedExternalAgency);

    // ASK added 1 statement below
    export.HiddenResourceLocationAddress.Assign(
      import.HiddenResourceLocationAddress);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.ZeroFill.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.ZeroFill.Text10;
    }

    if (!IsEmpty(export.Case1.Number))
    {
      local.ZeroFill.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.ZeroFill.Text10;
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.CaseNumber = export.Case1.Number;
      export.HiddenNextTranInfo.CsePersonNumber = export.CsePerson.Number;
      UseScCabNextTranPut1();

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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.HiddenNextTranInfo.CaseNumber))
      {
        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
      }

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
        export.CsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
      export.HiddenPrevUserAction.Command = global.Command;
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME") && !Equal(global.Command, "RETCDVL") && !
      Equal(global.Command, "RETINCS"))
    {
      export.ListCsePersons.PromptField = "";
      export.ListResourceTypes.PromptField = "";
      export.ListExternalAgencies.PromptField = "";

      // ------------------------------------------------------------
      // Beginning of Change
      // Following Statements are commented
      // out for  4.14.100 TC # 15.
      // ------------------------------------------------------------
      // ------------------------------------------------------------
      // End of Change
      // 4.14.100 TC # 15.
      // ------------------------------------------------------------
    }

    if (export.ExternalAgency.Identifier <= 0)
    {
      export.ExternalAgency.Name = "";
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "RETINCS"))
    {
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
      }

      export.ListCsePersons.PromptField = "";
      export.HiddenPrevUserAction.Command = "DISPLAY";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETRESL"))
    {
      if (import.CsePersonResource.ResourceNo > 0)
      {
        export.CsePersonResource.ResourceNo =
          import.CsePersonResource.ResourceNo;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCARS"))
    {
      if (export.HiddenSelectedCsePersonVehicle.Identifier > 0)
      {
        if (export.CsePersonVehicle.Identifier != export
          .HiddenPreviousCsePersonVehicle.Identifier)
        {
          export.ResourceLienHolderAddress.Assign(
            local.ZerosNBlanksInitialisedResourceLienHolderAddress);
          export.CsePersonResource.Assign(
            local.ZerosNBlanksInitialisedCsePersonResource);
          MoveCsePersonResource2(local.ZerosNBlanksInitialisedCsePersonResource,
            export.LastUpdated);
          export.ResourceTypeDesc.Description = local.CodeValue.Description;
          export.ResourceLocationAddress.Assign(
            local.ZerosNBlanksInitialisedResourceLocationAddress);
          export.CseActionCodeDesc.Description = local.CodeValue.Description;
          export.LegalActionLienType.Description = local.CodeValue.Description;
          MoveExternalAgency(local.ExternalAgency, export.ExternalAgency);
          export.LegalActionPersonResource.Assign(
            local.LegalActionPersonResource);
        }

        export.CsePersonVehicle.Identifier =
          export.HiddenSelectedCsePersonVehicle.Identifier;
        export.HiddenPrevUserAction.Command = "DISPLAY";
      }

      return;
    }

    if (Equal(global.Command, "RETEXAL"))
    {
      if (export.HiddenSelectedExternalAgency.Identifier > 0)
      {
        export.ExternalAgency.Identifier =
          export.HiddenSelectedExternalAgency.Identifier;
        export.ExternalAgency.Name = export.HiddenSelectedExternalAgency.Name;
      }
    }

    if (Equal(global.Command, "UPDATE"))
    {
      if (!Equal(import.CsePerson.Number, import.HiddenPreviousCsePerson.Number) ||
        import.CsePersonResource.ResourceNo != import
        .HiddenPreviousCsePersonResource.ResourceNo || !
        Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
        Equal(import.HiddenPrevUserAction.Command, "UPDATE") && !
        Equal(import.HiddenPrevUserAction.Command, "PREV") && !
        Equal(import.HiddenPrevUserAction.Command, "NEXT") || AsChar
        (import.HiddenDisplaySuccessful.Flag) != 'Y')
      {
        var field1 = GetField(export.CsePerson, "number");

        field1.Error = true;

        var field2 = GetField(export.CsePersonResource, "resourceNo");

        field2.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        export.HiddenDisplaySuccessful.Flag = "N";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // --------------------------------------------------
      // Lien holder name must be completed if any of the
      // lien fields are filled in.
      // --------------------------------------------------
      if (!IsEmpty(import.ResourceLienHolderAddress.Street1) || !
        IsEmpty(import.ResourceLienHolderAddress.Street2) || !
        IsEmpty(import.ResourceLienHolderAddress.City) || !
        IsEmpty(import.ResourceLienHolderAddress.State) || !
        IsEmpty(import.ResourceLienHolderAddress.ZipCode5) || !
        IsEmpty(import.ResourceLienHolderAddress.ZipCode4))
      {
        if (IsEmpty(import.CsePersonResource.OtherLienHolderName))
        {
          var field = GetField(export.CsePersonResource, "otherLienHolderName");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }
      }

      if (Length(TrimEnd(export.ResourceLocationAddress.ZipCode5)) > 0 && Length
        (TrimEnd(export.ResourceLocationAddress.ZipCode5)) < 5)
      {
        var field = GetField(export.ResourceLocationAddress, "zipCode5");

        field.Error = true;

        ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

        return;
      }

      if (Length(TrimEnd(export.ResourceLocationAddress.ZipCode5)) > 0 && Verify
        (export.ResourceLocationAddress.ZipCode5, "0123456789") != 0)
      {
        var field = GetField(export.ResourceLocationAddress, "zipCode5");

        field.Error = true;

        ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

        return;
      }

      if (Length(TrimEnd(export.ResourceLocationAddress.ZipCode5)) == 0 && Length
        (TrimEnd(export.ResourceLocationAddress.ZipCode4)) > 0)
      {
        var field = GetField(export.ResourceLocationAddress, "zipCode5");

        field.Error = true;

        ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

        return;
      }

      if (Length(TrimEnd(export.ResourceLocationAddress.ZipCode5)) > 0 && Length
        (TrimEnd(export.ResourceLocationAddress.ZipCode4)) > 0)
      {
        if (Length(TrimEnd(export.ResourceLocationAddress.ZipCode4)) < 4)
        {
          var field = GetField(export.ResourceLocationAddress, "zipCode4");

          field.Error = true;

          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

          return;
        }
        else if (Verify(export.ResourceLocationAddress.ZipCode4, "0123456789") !=
          0)
        {
          var field = GetField(export.ResourceLocationAddress, "zipCode4");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }
      }

      if (Length(TrimEnd(export.ResourceLienHolderAddress.ZipCode5)) > 0 && Length
        (TrimEnd(export.ResourceLienHolderAddress.ZipCode5)) < 5)
      {
        var field = GetField(export.ResourceLienHolderAddress, "zipCode5");

        field.Error = true;

        ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

        return;
      }

      if (Length(TrimEnd(export.ResourceLienHolderAddress.ZipCode5)) > 0 && Verify
        (export.ResourceLienHolderAddress.ZipCode5, "0123456789") != 0)
      {
        var field = GetField(export.ResourceLienHolderAddress, "zipCode5");

        field.Error = true;

        ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

        return;
      }

      if (Length(TrimEnd(export.ResourceLienHolderAddress.ZipCode5)) == 0 && Length
        (TrimEnd(export.ResourceLienHolderAddress.ZipCode4)) > 0)
      {
        var field = GetField(export.ResourceLienHolderAddress, "zipCode5");

        field.Error = true;

        ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

        return;
      }

      if (Length(TrimEnd(export.ResourceLienHolderAddress.ZipCode5)) > 0 && Length
        (TrimEnd(export.ResourceLienHolderAddress.ZipCode4)) > 0)
      {
        if (Length(TrimEnd(export.ResourceLienHolderAddress.ZipCode4)) < 4)
        {
          var field = GetField(export.ResourceLienHolderAddress, "zipCode4");

          field.Error = true;

          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

          return;
        }
        else if (Verify(export.ResourceLienHolderAddress.ZipCode4, "0123456789") !=
          0)
        {
          var field = GetField(export.ResourceLienHolderAddress, "zipCode4");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }
      }

      // --------------------------------------------------
      // On the lien holder address, if any of CITY, STATE,
      // ZIP are specified then each of the three fields
      // must be specified.
      // --------------------------------------------------
      if (!IsEmpty(export.ResourceLienHolderAddress.City) || !
        IsEmpty(export.ResourceLienHolderAddress.Street1) || !
        IsEmpty(export.ResourceLienHolderAddress.Street2) || !
        IsEmpty(export.ResourceLienHolderAddress.State) || !
        IsEmpty(export.ResourceLienHolderAddress.ZipCode5) || !
        IsEmpty(export.ResourceLienHolderAddress.ZipCode4))
      {
        if (IsEmpty(export.ResourceLienHolderAddress.ZipCode5))
        {
          var field = GetField(export.ResourceLienHolderAddress, "zipCode5");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.ResourceLienHolderAddress.State))
        {
          var field = GetField(export.ResourceLienHolderAddress, "state");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = export.ResourceLienHolderAddress.State ?? Spaces
          (10);
        UseCabValidateCodeValue2();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.ResourceLienHolderAddress, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";
        }

        if (IsEmpty(export.ResourceLienHolderAddress.City))
        {
          var field = GetField(export.ResourceLienHolderAddress, "city");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.ResourceLienHolderAddress.Street1))
        {
          var field = GetField(export.ResourceLienHolderAddress, "street1");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }
      }

      // ------------------------------------------------
      // If there is any lien, set indicator.
      // ------------------------------------------------
      if (!IsEmpty(export.CsePersonResource.OtherLienHolderName))
      {
        export.CsePersonResource.LienIndicator = "Y";
      }
      else
      {
        export.CsePersonResource.LienIndicator = "N";
      }

      if (Lt(local.NullDate.Date, export.CsePersonResource.ResourceDisposalDate))
        
      {
        if (Lt(Now().Date, export.CsePersonResource.ResourceDisposalDate))
        {
          var field =
            GetField(export.CsePersonResource, "resourceDisposalDate");

          field.Error = true;

          // ---------------------------------------------------------------
          // Beginning Of Change
          // 4.14.100 TC # 59
          // ---------------------------------------------------------------
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          // ---------------------------------------------------------------
          // End Of Change
          // 4.14.100 TC # 59
          // ---------------------------------------------------------------
        }
      }
      else
      {
        export.CsePersonResource.ResourceDisposalDate =
          local.Max.ExpirationDate;
      }

      if (Lt(local.NullDate.Date, export.CsePersonResource.OtherLienRemovedDate))
        
      {
        // ---------------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 59
        // ---------------------------------------------------------------
        if (Lt(Now().Date, export.CsePersonResource.OtherLienRemovedDate))
        {
          var field =
            GetField(export.CsePersonResource, "otherLienRemovedDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
        }

        // ---------------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 59
        // ---------------------------------------------------------------
        if (!Lt(local.NullDate.Date,
          export.CsePersonResource.OtherLienPlacedDate))
        {
          var field = GetField(export.CsePersonResource, "otherLienPlacedDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        if (!Lt(export.CsePersonResource.OtherLienPlacedDate,
          export.CsePersonResource.OtherLienRemovedDate))
        {
          var field =
            GetField(export.CsePersonResource, "otherLienRemovedDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }
      }
      else
      {
        export.CsePersonResource.OtherLienRemovedDate =
          local.Max.ExpirationDate;
      }

      if (Lt(local.NullDate.Date, export.CsePersonResource.OtherLienPlacedDate) &&
        IsEmpty(export.CsePersonResource.OtherLienHolderName))
      {
        var field = GetField(export.CsePersonResource, "otherLienHolderName");

        field.Error = true;

        ExitState = "OE0014_MANDATORY_FIELD_MISSING";

        // ---------------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 59
        // ---------------------------------------------------------------
        if (Equal(export.CsePersonResource.OtherLienRemovedDate,
          local.Max.ExpirationDate))
        {
          export.CsePersonResource.OtherLienRemovedDate = local.NullDate.Date;
        }

        if (Equal(export.CsePersonResource.ResourceDisposalDate,
          local.Max.ExpirationDate))
        {
          export.CsePersonResource.ResourceDisposalDate = local.NullDate.Date;
        }

        // ---------------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 59
        // ---------------------------------------------------------------
        return;
      }

      // --------------------------------------------------
      // On the location address, if any of CITY, STATE,
      // ZIP are specified then each of the three fields
      // must be specified.
      // --------------------------------------------------
      if (Lt(Now().Date, export.CsePersonResource.VerifiedDate))
      {
        var field = GetField(export.CsePersonResource, "verifiedDate");

        field.Error = true;

        // ---------------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 59
        // ---------------------------------------------------------------
        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

        // ---------------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 59
        // ---------------------------------------------------------------
      }

      // ---------------------------------------------------------------
      // Beginning Of Change
      // 4.14.100 TC # 59
      // ---------------------------------------------------------------
      if (Lt(Now().Date, export.CsePersonResource.OtherLienPlacedDate) && !
        IsEmpty(export.CsePersonResource.OtherLienHolderName))
      {
        var field = GetField(export.CsePersonResource, "otherLienPlacedDate");

        field.Error = true;

        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
      }

      // ---------------------------------------------------------------
      // End Of Change
      // 4.14.100 TC # 59
      // ---------------------------------------------------------------
      // ------------------------------------------------
      // Validate the type of resource.
      // ------------------------------------------------
      local.Code.CodeName = "RESOURCE TYPE";
      local.CodeValue.Cdvalue = export.CsePersonResource.Type1 ?? Spaces(10);
      UseCabValidateCodeValue1();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        export.ResourceTypeDesc.Description = local.CodeValue.Description;
      }
      else
      {
        // --------------------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 38
        // -------------------------------------------------------------------
        if (AsChar(local.ValidCode.Flag) == 'N' && local.ReturnCode.Count == 2)
        {
          if (IsEmpty(export.CsePerson.Number) && IsEmpty
            (export.CsePersonResource.Type1))
          {
            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            var field2 = GetField(export.CsePersonResource, "type1");

            field2.Color = "red";
            field2.Intensity = Intensity.High;
            field2.Highlighting = Highlighting.ReverseVideo;
            field2.Protected = false;

            ExitState = "OE0014_MANDATORY_FIELD_MISSING";

            goto Test1;
          }

          if (IsEmpty(export.CsePerson.Number) && !
            IsEmpty(export.CsePersonResource.Type1))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "OE0014_MANDATORY_FIELD_MISSING";

            goto Test1;
          }

          if (IsEmpty(export.CsePersonResource.Type1) && !
            IsEmpty(export.CsePerson.Number))
          {
            var field = GetField(export.CsePersonResource, "type1");

            field.Error = true;

            ExitState = "OE0014_MANDATORY_FIELD_MISSING";

            goto Test1;
          }

          // ------------------------------------------------------------------------------------
          // Per PR# 135378,  the following edit is added to stop user from 
          // adding/updating invalid 'Resource Type' value.
          //                                                 
          // Vithal (01/02/2002)
          // --------------------------------------------------------------------------------------
          if (!IsEmpty(export.CsePersonResource.Type1) && !
            IsEmpty(export.CsePerson.Number))
          {
            var field = GetField(export.CsePersonResource, "type1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            goto Test1;
          }
        }
        else if (AsChar(local.ValidCode.Flag) == 'N')
        {
          // --------------------------------------------------------------------
          // End Of Change
          // 4.14.100 TC # 38
          // -------------------------------------------------------------------
          var field = GetField(export.CsePersonResource, "type1");

          field.Error = true;

          export.ResourceTypeDesc.Description =
            Spaces(CodeValue.Description_MaxLength);
          ExitState = "ACO_NE0000_INVALID_CODE";
        }
      }

      if (import.CsePersonVehicle.Identifier > 0)
      {
        if (Equal(import.CsePersonResource.Type1, "CR") || Equal
          (import.CsePersonResource.Type1, "MV") || Equal
          (import.CsePersonResource.Type1, "TR") || Equal
          (import.CsePersonResource.Type1, "BT") || Equal
          (import.CsePersonResource.Type1, "RV") || Equal
          (import.CsePersonResource.Type1, "SI") || Equal
          (import.CsePersonResource.Type1, "MT"))
        {
        }
        else
        {
          var field = GetField(export.CsePersonResource, "type1");

          field.Error = true;

          ExitState = "INVALID_VEHICLE_FOR_TYPE";
        }
      }
    }

Test1:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "PRINT") || Equal
      (global.Command, "UPDATE"))
    {
      if (!IsEmpty(export.ResourceLocationAddress.City) || !
        IsEmpty(export.ResourceLocationAddress.Street1) || !
        IsEmpty(export.ResourceLocationAddress.Street2) || !
        IsEmpty(export.ResourceLocationAddress.State) || !
        IsEmpty(export.ResourceLocationAddress.ZipCode5) || !
        IsEmpty(export.ResourceLocationAddress.ZipCode4))
      {
        if (IsEmpty(export.ResourceLocationAddress.ZipCode5))
        {
          var field = GetField(export.ResourceLocationAddress, "zipCode5");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.ResourceLocationAddress.State))
        {
          var field = GetField(export.ResourceLocationAddress, "state");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }
        else
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = export.ResourceLocationAddress.State ?? Spaces
            (10);
          UseCabValidateCodeValue2();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.ResourceLocationAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        if (IsEmpty(export.ResourceLocationAddress.City))
        {
          var field = GetField(export.ResourceLocationAddress, "city");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.ResourceLocationAddress.Street1))
        {
          var field = GetField(export.ResourceLocationAddress, "street1");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }

        if (IsEmpty(export.CsePersonResource.LocationCounty))
        {
          var field = GetField(export.CsePersonResource, "locationCounty");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(export.CsePersonResource.OtherLienRemovedDate,
        local.Max.ExpirationDate))
      {
        export.CsePersonResource.OtherLienRemovedDate = local.NullDate.Date;
      }

      if (Equal(export.CsePersonResource.ResourceDisposalDate,
        local.Max.ExpirationDate))
      {
        export.CsePersonResource.ResourceDisposalDate = local.NullDate.Date;
      }

      return;
    }

    // mjr
    // -------------------------------------------------
    // 12/30/1998
    // Changed security to check CRUD actions only.
    // --------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PRINT"))
    {
      // --------------------------------------------------------
      // 04/04/00 W.Campbell - Disabled existing call to
      // Security Cab and added a new call with view
      // matching changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
      UseScCabTestSecurity();

      // --------------------------------------------------------
      // 04/04/00 W.Campbell - End of change to
      // disable existing call to Security Cab and
      // added a new call with view matching
      // changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RESL":
        ExitState = "ECO_LNK_TO_RESOURCE_LIST1";

        return;
      case "INCS":
        if (IsEmpty(export.ResourceLocationAddress.Street1))
        {
          ExitState = "OE0000_RESOURCE_ADDRESS_REQUIRED";

          return;
        }

        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        ExitState = "ECO_XFR_TO_INCOME_SOURCE_DETAIL1";

        return;
      case "PRINT":
        if (!Equal(import.CsePerson.Number,
          import.HiddenPreviousCsePerson.Number) || import
          .CsePersonResource.ResourceNo != import
          .HiddenPreviousCsePersonResource.ResourceNo || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE") || AsChar
          (import.HiddenDisplaySuccessful.Flag) != 'Y' || !
          Equal(export.CsePersonResource.Location,
          export.HiddenPreviousCsePersonResource.Location) || !
          Equal(export.ResourceLocationAddress.City,
          export.HiddenResourceLocationAddress.City) || !
          Equal(export.ResourceLocationAddress.Country,
          export.HiddenResourceLocationAddress.Country) || !
          Equal(export.ResourceLocationAddress.PostalCode,
          export.HiddenResourceLocationAddress.PostalCode) || !
          Equal(export.ResourceLocationAddress.Province,
          export.HiddenResourceLocationAddress.Province) || !
          Equal(export.ResourceLocationAddress.State,
          export.ResourceLocationAddress.State) || !
          Equal(export.ResourceLocationAddress.Street1,
          export.HiddenResourceLocationAddress.Street1) || !
          Equal(export.ResourceLocationAddress.Street2,
          export.HiddenResourceLocationAddress.Street2) || !
          Equal(export.ResourceLocationAddress.Zip3,
          export.HiddenResourceLocationAddress.Zip3) || !
          Equal(export.ResourceLocationAddress.ZipCode4,
          export.HiddenResourceLocationAddress.ZipCode4) || !
          Equal(export.ResourceLocationAddress.ZipCode5,
          export.HiddenResourceLocationAddress.ZipCode5))
        {
          export.HiddenDisplaySuccessful.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
        }
        else
        {
          export.DocmProtectFilter.Flag = "Y";
          export.DocmFilter.Type1 = "RESO";
          ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";

          return;
        }

        break;
      case "RETDOCM":
        if (IsEmpty(import.Print.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          break;
        }

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before invoking print process
        // -------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.Null1);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + import.Print.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdPrNumber) + export.CsePerson.Number;
        local.WorkArea.Text50 = TrimEnd(local.SpDocLiteral.IdResource) + NumberToString
          (export.CsePersonResource.ResourceNo, 13, 3);
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(export.HiddenNextTranInfo.MiscText1) + ";" + local
          .WorkArea.Text50;
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/30/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ----------------------------------------------------
        // Extract identifiers from next tran
        // -------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.CsePerson.Number =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          7, 10);
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdResource));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.HiddenNextTranInfo.MiscText1, 50, local.Position.Count +
          9, 3));
        export.CsePersonResource.ResourceNo =
          (int)local.BatchConvertNumToText.Number15;

        if (ReadCsePersonVehicle())
        {
          export.CsePersonVehicle.Identifier =
            entities.CsePersonVehicle.Identifier;
        }

        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        break;
      case "PREV":
        local.UserAction.Command = global.Command;
        export.HiddenDisplaySuccessful.Flag = "N";

        if (AsChar(export.ScrollingAttributes.MinusFlag) != '-')
        {
          ExitState = "OE0000_NO_MORE_RESOURCE_2_SCROLL";

          break;
        }

        UseOeResoDisplayResourceDetails2();

        if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          break;
        }

        if (Equal(export.CsePersonResource.ResourceDisposalDate,
          local.InitialisedToMaxDate.Date))
        {
          local.DateWorkArea.Date =
            export.CsePersonResource.ResourceDisposalDate;
          UseCabSetMaximumDiscontinueDate();
          export.CsePersonResource.ResourceDisposalDate =
            local.DateWorkArea.Date;
        }

        export.HiddenPreviousCsePerson.Number = export.CsePerson.Number;
        export.HiddenPreviousCsePersonResource.ResourceNo =
          export.CsePersonResource.ResourceNo;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenDisplaySuccessful.Flag = "Y";

          // ASK added two statements below
          MoveCsePersonResource1(export.CsePersonResource,
            export.HiddenPreviousCsePersonResource);
          export.HiddenResourceLocationAddress.Assign(
            export.ResourceLocationAddress);
        }
        else
        {
          export.HiddenDisplaySuccessful.Flag = "N";
        }

        if (IsExitState("OE0000_NO_MORE_RESOURCE_2_SCROLL"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Protected = false;
          field.Focused = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Case1.Number = "";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "NEXT":
        local.UserAction.Command = global.Command;
        export.HiddenDisplaySuccessful.Flag = "N";

        if (AsChar(export.ScrollingAttributes.PlusFlag) != '+')
        {
          ExitState = "OE0000_NO_MORE_RESOURCE_2_SCROLL";

          break;
        }

        UseOeResoDisplayResourceDetails2();

        if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          break;
        }

        if (Equal(export.CsePersonResource.ResourceDisposalDate,
          local.InitialisedToMaxDate.Date))
        {
          local.DateWorkArea.Date =
            export.CsePersonResource.ResourceDisposalDate;
          UseCabSetMaximumDiscontinueDate();
          export.CsePersonResource.ResourceDisposalDate =
            local.DateWorkArea.Date;
        }

        export.HiddenPreviousCsePerson.Number = export.CsePerson.Number;
        export.HiddenPreviousCsePersonResource.ResourceNo =
          export.CsePersonResource.ResourceNo;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenDisplaySuccessful.Flag = "Y";

          // ASK added two statements below
          MoveCsePersonResource1(export.CsePersonResource,
            export.HiddenPreviousCsePersonResource);
          export.HiddenResourceLocationAddress.Assign(
            export.ResourceLocationAddress);
        }
        else
        {
          export.HiddenDisplaySuccessful.Flag = "N";
        }

        if (IsExitState("OE0000_NO_MORE_RESOURCE_2_SCROLL"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Protected = false;
          field.Focused = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          export.HiddenResourceLocationAddress.Assign(
            export.ResourceLocationAddress);
        }

        break;
      case "LIST":
        if (!IsEmpty(export.ListCsePersons.PromptField) && AsChar
          (export.ListCsePersons.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListCsePersons, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.ListResourceTypes.PromptField) && AsChar
          (export.ListResourceTypes.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListResourceTypes, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.ListExternalAgencies.PromptField) && AsChar
          (export.ListExternalAgencies.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListExternalAgencies, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.ListResLocnAddrStates.PromptField) && AsChar
          (export.ListResLocnAddrStates.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListResLocnAddrStates, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.ListLienHolderStates.PromptField) && AsChar
          (export.ListLienHolderStates.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListLienHolderStates, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          break;
        }

        if (AsChar(export.ListResourceTypes.PromptField) == 'S')
        {
          export.Code.CodeName = "RESOURCE TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.ListExternalAgencies.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_EXTERNAL_AGENCY";

          break;
        }

        if (AsChar(export.ListResLocnAddrStates.PromptField) == 'S')
        {
          export.Code.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.ListLienHolderStates.PromptField) == 'S')
        {
          export.Code.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        // --------------------------------------------------------------
        // Beginning of Change
        // 4.14.100 TC # 14
        // --------------------------------------------------------------
        var field1 = GetField(export.ListCsePersons, "promptField");

        field1.Error = true;

        // --------------------------------------------------------------
        // End of Change
        // 4.14.100 TC # 14
        // --------------------------------------------------------------
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        export.HiddenPrevUserAction.Command = "DISPLAY";

        if (AsChar(export.ListResourceTypes.PromptField) == 'S')
        {
          export.ListResourceTypes.PromptField = "";

          if (IsEmpty(import.CodeValue.Cdvalue))
          {
            var field = GetField(export.CsePersonResource, "type1");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.CsePersonResource.Type1 = import.CodeValue.Cdvalue;
            export.ResourceTypeDesc.Description = import.CodeValue.Description;

            var field = GetField(export.CsePersonResource, "verifiedDate");

            field.Protected = false;
            field.Focused = true;
          }

          break;
        }

        if (AsChar(export.ListResLocnAddrStates.PromptField) == 'S')
        {
          export.ListResLocnAddrStates.PromptField = "";

          if (IsEmpty(import.CodeValue.Cdvalue))
          {
            var field = GetField(export.ResourceLocationAddress, "state");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.ResourceLocationAddress.State = import.CodeValue.Cdvalue;

            var field = GetField(export.ResourceLocationAddress, "zipCode5");

            field.Protected = false;
            field.Focused = true;
          }

          // ------------------------------------------------------------
          // Beginning of Change
          // 4.14.100 TC # 15.
          // ------------------------------------------------------------
          break;

          // ------------------------------------------------------------
          // End of Change
          // 4.14.100 TC # 15.
          // ------------------------------------------------------------
        }

        if (AsChar(export.ListLienHolderStates.PromptField) == 'S')
        {
          export.ListLienHolderStates.PromptField = "";

          if (IsEmpty(import.CodeValue.Cdvalue))
          {
            var field = GetField(export.ResourceLienHolderAddress, "state");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.ResourceLienHolderAddress.State = import.CodeValue.Cdvalue;

            var field = GetField(export.ResourceLienHolderAddress, "zipCode5");

            field.Protected = false;
            field.Focused = true;
          }

          // ------------------------------------------------------------
          // Beginning of Change
          // 4.14.100 TC # 15.
          // ------------------------------------------------------------
          // ------------------------------------------------------------
          // End of Change
          // 4.14.100 TC # 15.
          // ------------------------------------------------------------
        }

        break;
      case "RETCARS":
        export.HiddenPrevUserAction.Command = "DISPLAY";

        break;
      case "RETEXAL":
        export.HiddenPrevUserAction.Command = "DISPLAY";

        var field2 = GetField(export.ResourceLocationAddress, "street1");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case "ADD":
        // --------------------------------------------------
        // F5-Add will add another resource for the person.
        // --------------------------------------------------
        UseOeResoCreateResourceDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }

        if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          break;
        }

        if (IsExitState("CSE_PERSON_RESOURCE_AE") || IsExitState
          ("CSE_PERSON_RESOURCE_PV"))
        {
          var field = GetField(export.CsePersonResource, "resourceNo");

          field.Error = true;

          break;
        }

        if (IsExitState("RESOURCE_LOCATION_ADDRESS_AE") || IsExitState
          ("RESOURCE_LOCATION_ADDRESS_PV"))
        {
          var field3 = GetField(export.ResourceLocationAddress, "street1");

          field3.Error = true;

          var field4 = GetField(export.ResourceLocationAddress, "street2");

          field4.Error = true;

          var field5 = GetField(export.ResourceLocationAddress, "city");

          field5.Error = true;

          var field6 = GetField(export.ResourceLocationAddress, "state");

          field6.Error = true;

          var field7 = GetField(export.ResourceLocationAddress, "zipCode5");

          field7.Error = true;

          var field8 = GetField(export.ResourceLocationAddress, "zipCode4");

          field8.Error = true;

          break;
        }

        if (IsExitState("RESOURCE_LIEN_HOLDER_ADDRESS_AE") || IsExitState
          ("RESOURCE_LIEN_HOLDER_ADDRESS_PV"))
        {
          var field3 = GetField(export.ResourceLienHolderAddress, "street1");

          field3.Error = true;

          var field4 = GetField(export.ResourceLienHolderAddress, "street2");

          field4.Error = true;

          var field5 = GetField(export.ResourceLienHolderAddress, "city");

          field5.Error = true;

          var field6 = GetField(export.ResourceLienHolderAddress, "state");

          field6.Error = true;

          var field7 = GetField(export.ResourceLienHolderAddress, "zipCode5");

          field7.Error = true;

          var field8 = GetField(export.ResourceLienHolderAddress, "zipCode4");

          field8.Error = true;

          break;
        }

        if (IsExitState("CSE_PERSON_VEHICLE_NF"))
        {
          var field = GetField(export.CsePersonVehicle, "identifier");

          field.Error = true;

          ExitState = "INVALID_VEHICLE_NUMBER";

          break;
        }

        if (IsExitState("OE0000_ANOTHR_RESOURCE_ALRD_ASSC"))
        {
          var field = GetField(export.CsePersonVehicle, "identifier");

          field.Error = true;

          break;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenPreviousCsePerson.Number = export.CsePerson.Number;
          export.HiddenPreviousCsePersonResource.ResourceNo =
            export.CsePersonResource.ResourceNo;
          export.HiddenPreviousExternalAgency.Identifier =
            export.ExternalAgency.Identifier;
          export.HiddenDisplaySuccessful.Flag = "Y";

          // ASK added two statements below
          MoveCsePersonResource1(export.CsePersonResource,
            export.HiddenPreviousCsePersonResource);
          export.HiddenResourceLocationAddress.Assign(
            export.ResourceLocationAddress);
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        // --------------------------------------------------
        // F6-Update the person's resource after verifying
        // that a display of the resource has been done.
        // --------------------------------------------------
        UseOeResoUpdateResourceDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }
          else if (IsExitState("CSE_PERSON_RESOURCE_PV"))
          {
            var field = GetField(export.CsePersonResource, "resourceNo");

            field.Error = true;
          }
          else if (IsExitState("CSE_PERSON_VEHICLE_NF"))
          {
            var field = GetField(export.CsePersonVehicle, "identifier");

            field.Error = true;

            ExitState = "INVALID_VEHICLE_NUMBER";
          }
          else if (IsExitState("OE0000_ANOTHR_RESOURCE_ALRD_ASSC"))
          {
            var field = GetField(export.CsePersonVehicle, "identifier");

            field.Error = true;
          }
          else
          {
          }

          break;
        }

        export.HiddenPreviousCsePerson.Number = export.CsePerson.Number;
        export.HiddenPreviousCsePersonResource.ResourceNo =
          export.CsePersonResource.ResourceNo;
        export.HiddenPreviousExternalAgency.Identifier =
          export.ExternalAgency.Identifier;
        export.HiddenDisplaySuccessful.Flag = "Y";

        // ASK added two statements below
        MoveCsePersonResource1(export.CsePersonResource,
          export.HiddenPreviousCsePersonResource);
        export.HiddenResourceLocationAddress.Assign(
          export.ResourceLocationAddress);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        // ------------------------------------------------
        // The resource must have been displayed
        // immediately before attempting to delete it.
        // ------------------------------------------------
        if (!Equal(import.CsePerson.Number,
          import.HiddenPreviousCsePerson.Number) || import
          .CsePersonResource.ResourceNo != import
          .HiddenPreviousCsePersonResource.ResourceNo || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE") || AsChar
          (import.HiddenDisplaySuccessful.Flag) != 'Y')
        {
          // ------------------------------------------------
          // Set error conditions.
          // ------------------------------------------------
          var field3 = GetField(export.CsePerson, "number");

          field3.Error = true;

          var field4 = GetField(export.CsePersonResource, "resourceNo");

          field4.Error = true;

          export.HiddenDisplaySuccessful.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          break;
        }

        export.HiddenDisplaySuccessful.Flag = "N";
        UseOeResoDeleteResourceDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }
          else if (IsExitState("CSE_PERSON_VEHICLE_NF"))
          {
            var field = GetField(export.CsePersonVehicle, "identifier");

            field.Error = true;

            ExitState = "INVALID_VEHICLE_NUMBER";
          }
          else
          {
          }

          break;
        }

        export.CsePersonResource.Assign(
          local.ZerosNBlanksInitialisedCsePersonResource);
        MoveCsePersonResource1(local.ZerosNBlanksInitialisedCsePersonResource,
          export.HiddenPreviousCsePersonResource);
        MoveCsePersonResource2(local.ZerosNBlanksInitialisedCsePersonResource,
          export.LastUpdated);
        export.ExternalAgency.Identifier = 0;
        export.ExternalAgency.Name = "";
        export.CsePersonVehicle.Identifier = 0;
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        export.CseActionCodeDesc.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.ResourceTypeDesc.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.ResourceLocationAddress.Assign(
          local.ZerosNBlanksInitialisedResourceLocationAddress);

        // ASK added 1 statement below
        export.HiddenResourceLocationAddress.Assign(
          local.ZerosNBlanksInitialisedResourceLocationAddress);
        export.ResourceLienHolderAddress.Assign(
          local.ZerosNBlanksInitialisedResourceLienHolderAddress);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        // ------- end of delete --------
        break;
      case "CARS":
        export.AllowChangePersonNo.Flag = "N";
        ExitState = "ECO_LNK_TO_CARS_LIST_VEHICLES";

        return;
      case "FROMINCS":
        export.HiddenPrevUserAction.Command = "DISPLAY";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // mjr
    // ---------------------------------------------------
    // 12/30/1998
    // Pulled Display out of main case of command for after return from Print.
    // ----------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // F2-Display selected person's resource.
      // ---------------------------------------------
      local.UserAction.Command = global.Command;
      export.HiddenDisplaySuccessful.Flag = "N";

      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NO_REQUIRED";

        goto Test2;
      }

      // --------------------------------------------------------------------------
      // Call the CAB to check Family Violence.
      //                                                   
      // Vithal (01/03/02)
      // --------------------------------------------------------------------------
      UseScSecurityValidAuthForFv();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.CsePersonResource.Assign(local.BlankCsePersonResource);
        MoveExternalAgency(local.BlankExternalAgency, export.ExternalAgency);
        export.ResourceLocationAddress.
          Assign(local.BlankResourceLocationAddress);
        export.CsePersonVehicle.Identifier =
          local.BlankCsePersonVehicle.Identifier;
        export.ResourceLienHolderAddress.Assign(
          local.BlankResourceLienHolderAddress);
        export.LegalActionPersonResource.Assign(
          local.BlankLegalActionPersonResource);
        export.ResourceTypeDesc.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CseActionCodeDesc.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.LegalActionLienType.Description =
          Spaces(CodeValue.Description_MaxLength);
        export.CsePersonsWorkSet.FormattedName = "";
        export.LastUpdated.LastUpdatedBy = "";
        export.LastUpdated.LastUpdatedTimestamp =
          local.BlankDateWorkArea.Timestamp;

        goto Test2;
      }

      if (!IsEmpty(export.Case1.Number))
      {
        UseOeCabCheckCaseMember();

        switch(AsChar(local.Work.Flag))
        {
          case 'C':
            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            ExitState = "CASE_NF";

            break;
          case 'P':
            var field2 = GetField(export.CsePerson, "number");

            field2.Error = true;

            export.CsePersonsWorkSet.FormattedName = "";
            ExitState = "CSE_PERSON_NF";

            break;
          case 'R':
            var field3 = GetField(export.Case1, "number");

            field3.Error = true;

            var field4 = GetField(export.CsePerson, "number");

            field4.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";

            break;
          default:
            break;
        }

        if (!IsEmpty(local.Work.Flag))
        {
          return;
        }
      }

      UseOeResoDisplayResourceDetails1();

      if (IsExitState("CSE_PERSON_NF"))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        goto Test2;
      }

      if (IsExitState("OE0000_NO_RESO_FOR_CARS"))
      {
        var field1 = GetField(export.CsePerson, "number");

        field1.Error = true;

        var field2 = GetField(export.CsePersonVehicle, "identifier");

        field2.Error = true;

        goto Test2;
      }

      export.HiddenPreviousCsePerson.Number = export.CsePerson.Number;
      export.HiddenPreviousCsePersonResource.ResourceNo =
        export.CsePersonResource.ResourceNo;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.HiddenDisplaySuccessful.Flag = "Y";
      }
      else
      {
        export.HiddenDisplaySuccessful.Flag = "N";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.FromIncomeSource.Identifier =
          local.RefreshIncomeSource.Identifier;
        export.HiddenSelectedCsePersonVehicle.Identifier =
          local.RefreshCsePersonVehicle.Identifier;

        // ASK added two statements below
        MoveCsePersonResource1(export.CsePersonResource,
          export.HiddenPreviousCsePersonResource);
        export.HiddenResourceLocationAddress.Assign(
          export.ResourceLocationAddress);

        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText2,
          NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count > 0)
        {
          local.WorkArea.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
            (50);
          UseSpPrintDecodeReturnCode();
          export.HiddenNextTranInfo.MiscText2 = local.WorkArea.Text50;
        }
      }
    }

Test2:

    if (Equal(export.CsePersonResource.OtherLienRemovedDate,
      local.Max.ExpirationDate))
    {
      export.CsePersonResource.OtherLienRemovedDate = local.NullDate.Date;
    }

    if (Equal(export.CsePersonResource.ResourceDisposalDate,
      local.Max.ExpirationDate))
    {
      export.CsePersonResource.ResourceDisposalDate = local.NullDate.Date;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonResource1(CsePersonResource source,
    CsePersonResource target)
  {
    target.ResourceNo = source.ResourceNo;
    target.Location = source.Location;
  }

  private static void MoveCsePersonResource2(CsePersonResource source,
    CsePersonResource target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExternalAgency(ExternalAgency source,
    ExternalAgency target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
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

  private static void MoveResourceLienHolderAddress(
    ResourceLienHolderAddress source, ResourceLienHolderAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveResourceLocationAddress1(
    ResourceLocationAddress source, ResourceLocationAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.AddressType = source.AddressType;
  }

  private static void MoveResourceLocationAddress2(
    ResourceLocationAddress source, ResourceLocationAddress target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.AddressType = source.AddressType;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdPrNumber = source.IdPrNumber;
    target.IdResource = source.IdResource;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.Max.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeResoCreateResourceDetails()
  {
    var useImport = new OeResoCreateResourceDetails.Import();
    var useExport = new OeResoCreateResourceDetails.Export();

    useImport.FromExternalAgency.Identifier = export.ExternalAgency.Identifier;
    useImport.FromIncomeSource.Identifier = import.FromIncomeSource.Identifier;
    useImport.CsePersonVehicle.Identifier = export.CsePersonVehicle.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonResource.Assign(export.CsePersonResource);
    useImport.ResourceLocationAddress.Assign(export.ResourceLocationAddress);
    useImport.ResourceLienHolderAddress.
      Assign(export.ResourceLienHolderAddress);

    Call(OeResoCreateResourceDetails.Execute, useImport, useExport);

    MoveCsePersonResource2(useExport.UpdatedStamp, export.LastUpdated);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonVehicle.Identifier = useExport.CsePersonVehicle.Identifier;
    export.CsePersonResource.Assign(useExport.CsePersonResource);
    MoveResourceLocationAddress1(useExport.ResourceLocationAddress,
      export.ResourceLocationAddress);
    export.ResourceLienHolderAddress.EffectiveDate =
      useExport.ResourceLienHolderAddress.EffectiveDate;
  }

  private void UseOeResoDeleteResourceDetails()
  {
    var useImport = new OeResoDeleteResourceDetails.Import();
    var useExport = new OeResoDeleteResourceDetails.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonResource.ResourceNo =
      export.CsePersonResource.ResourceNo;

    Call(OeResoDeleteResourceDetails.Execute, useImport, useExport);
  }

  private void UseOeResoDisplayResourceDetails1()
  {
    var useImport = new OeResoDisplayResourceDetails.Import();
    var useExport = new OeResoDisplayResourceDetails.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.StartIncomeSource.Identifier = export.FromIncomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonResource.ResourceNo =
      export.CsePersonResource.ResourceNo;
    useImport.StartCsePersonVehicle.Identifier =
      export.CsePersonVehicle.Identifier;

    Call(OeResoDisplayResourceDetails.Execute, useImport, useExport);

    export.LegalActionLienType.Description =
      useExport.LegalActionLienType.Description;
    MoveExternalAgency(useExport.ExternalAgency, export.ExternalAgency);
    export.LegalActionPersonResource.
      Assign(useExport.LegalActionPersonResource);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    MoveCsePersonResource2(useExport.LastUpdated, export.LastUpdated);
    export.ResourceTypeDesc.Description =
      useExport.ResourceTypeDesc.Description;
    export.CseActionCodeDesc.Description = useExport.CseActionDesc.Description;
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.CsePersonResource.Assign(useExport.CsePersonResource);
    export.CsePersonVehicle.Identifier = useExport.CsePersonVehicle.Identifier;
    export.ResourceLocationAddress.Assign(useExport.ResourceLocationAddress);
    export.ResourceLienHolderAddress.
      Assign(useExport.ResourceLienHolderAddress);
  }

  private void UseOeResoDisplayResourceDetails2()
  {
    var useImport = new OeResoDisplayResourceDetails.Import();
    var useExport = new OeResoDisplayResourceDetails.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonResource.ResourceNo =
      export.CsePersonResource.ResourceNo;

    Call(OeResoDisplayResourceDetails.Execute, useImport, useExport);

    export.LegalActionLienType.Description =
      useExport.LegalActionLienType.Description;
    MoveExternalAgency(useExport.ExternalAgency, export.ExternalAgency);
    export.LegalActionPersonResource.
      Assign(useExport.LegalActionPersonResource);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    MoveCsePersonResource2(useExport.LastUpdated, export.LastUpdated);
    export.ResourceTypeDesc.Description =
      useExport.ResourceTypeDesc.Description;
    export.CseActionCodeDesc.Description = useExport.CseActionDesc.Description;
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.CsePersonResource.Assign(useExport.CsePersonResource);
    export.CsePersonVehicle.Identifier = useExport.CsePersonVehicle.Identifier;
    export.ResourceLocationAddress.Assign(useExport.ResourceLocationAddress);
    export.ResourceLienHolderAddress.
      Assign(useExport.ResourceLienHolderAddress);
  }

  private void UseOeResoUpdateResourceDetails()
  {
    var useImport = new OeResoUpdateResourceDetails.Import();
    var useExport = new OeResoUpdateResourceDetails.Export();

    useImport.Previous.Identifier =
      import.HiddenPreviousExternalAgency.Identifier;
    useImport.FromExternalAgency.Identifier = import.ExternalAgency.Identifier;
    useImport.FromIncomeSource.Identifier = import.FromIncomeSource.Identifier;
    useImport.CsePersonVehicle.Identifier = import.CsePersonVehicle.Identifier;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonResource.Assign(export.CsePersonResource);
    useImport.ResourceLocationAddress.Assign(import.ResourceLocationAddress);
    useImport.ResourceLienHolderAddress.
      Assign(import.ResourceLienHolderAddress);

    Call(OeResoUpdateResourceDetails.Execute, useImport, useExport);

    MoveCsePersonResource2(useExport.LastUpdated, export.LastUpdated);
    export.CsePersonResource.Assign(useExport.CsePersonResource);
    MoveResourceLocationAddress2(useExport.ResourceLocationAddress,
      export.ResourceLocationAddress);
    MoveResourceLienHolderAddress(useExport.ResourceLienHolderAddress,
      export.ResourceLienHolderAddress);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.WorkArea.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadCsePersonVehicle()
  {
    entities.CsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cprCResourceNo", export.CsePersonResource.ResourceNo);
        db.SetNullableString(command, "cspCNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.CsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.CsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.CsePersonVehicle.CspCNumber = db.GetNullableString(reader, 3);
        entities.CsePersonVehicle.Populated = true;
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
    /// A value of LegalActionLienType.
    /// </summary>
    [JsonPropertyName("legalActionLienType")]
    public CodeValue LegalActionLienType
    {
      get => legalActionLienType ??= new();
      set => legalActionLienType = value;
    }

    /// <summary>
    /// A value of HiddenResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("hiddenResourceLocationAddress")]
    public ResourceLocationAddress HiddenResourceLocationAddress
    {
      get => hiddenResourceLocationAddress ??= new();
      set => hiddenResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of HiddenPreviousExternalAgency.
    /// </summary>
    [JsonPropertyName("hiddenPreviousExternalAgency")]
    public ExternalAgency HiddenPreviousExternalAgency
    {
      get => hiddenPreviousExternalAgency ??= new();
      set => hiddenPreviousExternalAgency = value;
    }

    /// <summary>
    /// A value of HiddenSelectedExternalAgency.
    /// </summary>
    [JsonPropertyName("hiddenSelectedExternalAgency")]
    public ExternalAgency HiddenSelectedExternalAgency
    {
      get => hiddenSelectedExternalAgency ??= new();
      set => hiddenSelectedExternalAgency = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonVehicle")]
    public CsePersonVehicle HiddenSelectedCsePersonVehicle
    {
      get => hiddenSelectedCsePersonVehicle ??= new();
      set => hiddenSelectedCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of FromIncomeSource.
    /// </summary>
    [JsonPropertyName("fromIncomeSource")]
    public IncomeSource FromIncomeSource
    {
      get => fromIncomeSource ??= new();
      set => fromIncomeSource = value;
    }

    /// <summary>
    /// A value of ExternalAgency.
    /// </summary>
    [JsonPropertyName("externalAgency")]
    public ExternalAgency ExternalAgency
    {
      get => externalAgency ??= new();
      set => externalAgency = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of ListResourceTypes.
    /// </summary>
    [JsonPropertyName("listResourceTypes")]
    public Standard ListResourceTypes
    {
      get => listResourceTypes ??= new();
      set => listResourceTypes = value;
    }

    /// <summary>
    /// A value of ListResLocnAddrStates.
    /// </summary>
    [JsonPropertyName("listResLocnAddrStates")]
    public Standard ListResLocnAddrStates
    {
      get => listResLocnAddrStates ??= new();
      set => listResLocnAddrStates = value;
    }

    /// <summary>
    /// A value of ListExternalAgencies.
    /// </summary>
    [JsonPropertyName("listExternalAgencies")]
    public Standard ListExternalAgencies
    {
      get => listExternalAgencies ??= new();
      set => listExternalAgencies = value;
    }

    /// <summary>
    /// A value of ListLienHolderStates.
    /// </summary>
    [JsonPropertyName("listLienHolderStates")]
    public Standard ListLienHolderStates
    {
      get => listLienHolderStates ??= new();
      set => listLienHolderStates = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public CsePersonResource LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
    }

    /// <summary>
    /// A value of ResourceTypeDesc.
    /// </summary>
    [JsonPropertyName("resourceTypeDesc")]
    public CodeValue ResourceTypeDesc
    {
      get => resourceTypeDesc ??= new();
      set => resourceTypeDesc = value;
    }

    /// <summary>
    /// A value of CseActionCodeDesc.
    /// </summary>
    [JsonPropertyName("cseActionCodeDesc")]
    public CodeValue CseActionCodeDesc
    {
      get => cseActionCodeDesc ??= new();
      set => cseActionCodeDesc = value;
    }

    /// <summary>
    /// A value of HiddenDisplaySuccessful.
    /// </summary>
    [JsonPropertyName("hiddenDisplaySuccessful")]
    public Common HiddenDisplaySuccessful
    {
      get => hiddenDisplaySuccessful ??= new();
      set => hiddenDisplaySuccessful = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("resourceLienHolderAddress")]
    public ResourceLienHolderAddress ResourceLienHolderAddress
    {
      get => resourceLienHolderAddress ??= new();
      set => resourceLienHolderAddress = value;
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
    /// A value of HiddenPreviousCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePerson")]
    public CsePerson HiddenPreviousCsePerson
    {
      get => hiddenPreviousCsePerson ??= new();
      set => hiddenPreviousCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonResource.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonResource")]
    public CsePersonResource HiddenPreviousCsePersonResource
    {
      get => hiddenPreviousCsePersonResource ??= new();
      set => hiddenPreviousCsePersonResource = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonVehicle")]
    public CsePersonVehicle HiddenPreviousCsePersonVehicle
    {
      get => hiddenPreviousCsePersonVehicle ??= new();
      set => hiddenPreviousCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedPf24Lett.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedPf24Lett")]
    public CodeValue DlgflwSelectedPf24Lett
    {
      get => dlgflwSelectedPf24Lett ??= new();
      set => dlgflwSelectedPf24Lett = value;
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

    private CodeValue legalActionLienType;
    private ResourceLocationAddress hiddenResourceLocationAddress;
    private Document print;
    private ExternalAgency hiddenPreviousExternalAgency;
    private ExternalAgency hiddenSelectedExternalAgency;
    private CsePersonVehicle hiddenSelectedCsePersonVehicle;
    private Common hiddenDisplayPerformed;
    private IncomeSource fromIncomeSource;
    private ExternalAgency externalAgency;
    private LegalActionPersonResource legalActionPersonResource;
    private Case1 case1;
    private Standard listCsePersons;
    private Standard listResourceTypes;
    private Standard listResLocnAddrStates;
    private Standard listExternalAgencies;
    private Standard listLienHolderStates;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonResource lastUpdated;
    private CodeValue resourceTypeDesc;
    private CodeValue cseActionCodeDesc;
    private Common hiddenDisplaySuccessful;
    private Common hiddenPrevUserAction;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonResource csePersonResource;
    private CsePersonVehicle csePersonVehicle;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
    private Code code;
    private CodeValue codeValue;
    private CsePerson hiddenPreviousCsePerson;
    private CsePersonResource hiddenPreviousCsePersonResource;
    private CsePersonVehicle hiddenPreviousCsePersonVehicle;
    private CodeValue dlgflwSelectedPf24Lett;
    private AbendData abendData;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DocmProtectFilter.
    /// </summary>
    [JsonPropertyName("docmProtectFilter")]
    public Common DocmProtectFilter
    {
      get => docmProtectFilter ??= new();
      set => docmProtectFilter = value;
    }

    /// <summary>
    /// A value of DocmFilter.
    /// </summary>
    [JsonPropertyName("docmFilter")]
    public Document DocmFilter
    {
      get => docmFilter ??= new();
      set => docmFilter = value;
    }

    /// <summary>
    /// A value of LegalActionLienType.
    /// </summary>
    [JsonPropertyName("legalActionLienType")]
    public CodeValue LegalActionLienType
    {
      get => legalActionLienType ??= new();
      set => legalActionLienType = value;
    }

    /// <summary>
    /// A value of HiddenResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("hiddenResourceLocationAddress")]
    public ResourceLocationAddress HiddenResourceLocationAddress
    {
      get => hiddenResourceLocationAddress ??= new();
      set => hiddenResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of HiddenPreviousExternalAgency.
    /// </summary>
    [JsonPropertyName("hiddenPreviousExternalAgency")]
    public ExternalAgency HiddenPreviousExternalAgency
    {
      get => hiddenPreviousExternalAgency ??= new();
      set => hiddenPreviousExternalAgency = value;
    }

    /// <summary>
    /// A value of HiddenSelectedExternalAgency.
    /// </summary>
    [JsonPropertyName("hiddenSelectedExternalAgency")]
    public ExternalAgency HiddenSelectedExternalAgency
    {
      get => hiddenSelectedExternalAgency ??= new();
      set => hiddenSelectedExternalAgency = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonVehicle")]
    public CsePersonVehicle HiddenSelectedCsePersonVehicle
    {
      get => hiddenSelectedCsePersonVehicle ??= new();
      set => hiddenSelectedCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of FromIncomeSource.
    /// </summary>
    [JsonPropertyName("fromIncomeSource")]
    public IncomeSource FromIncomeSource
    {
      get => fromIncomeSource ??= new();
      set => fromIncomeSource = value;
    }

    /// <summary>
    /// A value of ExternalAgency.
    /// </summary>
    [JsonPropertyName("externalAgency")]
    public ExternalAgency ExternalAgency
    {
      get => externalAgency ??= new();
      set => externalAgency = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of AllowChangePersonNo.
    /// </summary>
    [JsonPropertyName("allowChangePersonNo")]
    public Common AllowChangePersonNo
    {
      get => allowChangePersonNo ??= new();
      set => allowChangePersonNo = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of ListResourceTypes.
    /// </summary>
    [JsonPropertyName("listResourceTypes")]
    public Standard ListResourceTypes
    {
      get => listResourceTypes ??= new();
      set => listResourceTypes = value;
    }

    /// <summary>
    /// A value of ListResLocnAddrStates.
    /// </summary>
    [JsonPropertyName("listResLocnAddrStates")]
    public Standard ListResLocnAddrStates
    {
      get => listResLocnAddrStates ??= new();
      set => listResLocnAddrStates = value;
    }

    /// <summary>
    /// A value of ListExternalAgencies.
    /// </summary>
    [JsonPropertyName("listExternalAgencies")]
    public Standard ListExternalAgencies
    {
      get => listExternalAgencies ??= new();
      set => listExternalAgencies = value;
    }

    /// <summary>
    /// A value of ListLienHolderStates.
    /// </summary>
    [JsonPropertyName("listLienHolderStates")]
    public Standard ListLienHolderStates
    {
      get => listLienHolderStates ??= new();
      set => listLienHolderStates = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public CsePersonResource LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
    }

    /// <summary>
    /// A value of ResourceTypeDesc.
    /// </summary>
    [JsonPropertyName("resourceTypeDesc")]
    public CodeValue ResourceTypeDesc
    {
      get => resourceTypeDesc ??= new();
      set => resourceTypeDesc = value;
    }

    /// <summary>
    /// A value of CseActionCodeDesc.
    /// </summary>
    [JsonPropertyName("cseActionCodeDesc")]
    public CodeValue CseActionCodeDesc
    {
      get => cseActionCodeDesc ??= new();
      set => cseActionCodeDesc = value;
    }

    /// <summary>
    /// A value of HiddenDisplaySuccessful.
    /// </summary>
    [JsonPropertyName("hiddenDisplaySuccessful")]
    public Common HiddenDisplaySuccessful
    {
      get => hiddenDisplaySuccessful ??= new();
      set => hiddenDisplaySuccessful = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("resourceLienHolderAddress")]
    public ResourceLienHolderAddress ResourceLienHolderAddress
    {
      get => resourceLienHolderAddress ??= new();
      set => resourceLienHolderAddress = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePerson")]
    public CsePerson HiddenPreviousCsePerson
    {
      get => hiddenPreviousCsePerson ??= new();
      set => hiddenPreviousCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonResource.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonResource")]
    public CsePersonResource HiddenPreviousCsePersonResource
    {
      get => hiddenPreviousCsePersonResource ??= new();
      set => hiddenPreviousCsePersonResource = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonVehicle")]
    public CsePersonVehicle HiddenPreviousCsePersonVehicle
    {
      get => hiddenPreviousCsePersonVehicle ??= new();
      set => hiddenPreviousCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of Pf24Source.
    /// </summary>
    [JsonPropertyName("pf24Source")]
    public Code Pf24Source
    {
      get => pf24Source ??= new();
      set => pf24Source = value;
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
    /// A value of DocumentResponse.
    /// </summary>
    [JsonPropertyName("documentResponse")]
    public Common DocumentResponse
    {
      get => documentResponse ??= new();
      set => documentResponse = value;
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

    private Common docmProtectFilter;
    private Document docmFilter;
    private CodeValue legalActionLienType;
    private ResourceLocationAddress hiddenResourceLocationAddress;
    private Document document;
    private ExternalAgency hiddenPreviousExternalAgency;
    private ExternalAgency hiddenSelectedExternalAgency;
    private CsePersonVehicle hiddenSelectedCsePersonVehicle;
    private IncomeSource fromIncomeSource;
    private ExternalAgency externalAgency;
    private LegalActionPersonResource legalActionPersonResource;
    private Common allowChangePersonNo;
    private Case1 case1;
    private Standard listCsePersons;
    private Standard listResourceTypes;
    private Standard listResLocnAddrStates;
    private Standard listExternalAgencies;
    private Standard listLienHolderStates;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonResource lastUpdated;
    private CodeValue resourceTypeDesc;
    private CodeValue cseActionCodeDesc;
    private Common hiddenDisplaySuccessful;
    private Common hiddenPrevUserAction;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonResource csePersonResource;
    private CsePersonVehicle csePersonVehicle;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private CsePerson hiddenPreviousCsePerson;
    private CsePersonResource hiddenPreviousCsePersonResource;
    private CsePersonVehicle hiddenPreviousCsePersonVehicle;
    private Common hiddenDisplayPerformed;
    private Code pf24Source;
    private AbendData abendData;
    private Common documentResponse;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of ExternalAgency.
    /// </summary>
    [JsonPropertyName("externalAgency")]
    public ExternalAgency ExternalAgency
    {
      get => externalAgency ??= new();
      set => externalAgency = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Code Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("refreshCsePersonVehicle")]
    public CsePersonVehicle RefreshCsePersonVehicle
    {
      get => refreshCsePersonVehicle ??= new();
      set => refreshCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of RefreshIncomeSource.
    /// </summary>
    [JsonPropertyName("refreshIncomeSource")]
    public IncomeSource RefreshIncomeSource
    {
      get => refreshIncomeSource ??= new();
      set => refreshIncomeSource = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of ZerosNBlanksInitialisedResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("zerosNBlanksInitialisedResourceLienHolderAddress")]
    public ResourceLienHolderAddress ZerosNBlanksInitialisedResourceLienHolderAddress
      
    {
      get => zerosNBlanksInitialisedResourceLienHolderAddress ??= new();
      set => zerosNBlanksInitialisedResourceLienHolderAddress = value;
    }

    /// <summary>
    /// A value of ZerosNBlanksInitialisedResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("zerosNBlanksInitialisedResourceLocationAddress")]
    public ResourceLocationAddress ZerosNBlanksInitialisedResourceLocationAddress
      
    {
      get => zerosNBlanksInitialisedResourceLocationAddress ??= new();
      set => zerosNBlanksInitialisedResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ZerosNBlanksInitialisedCsePersonResource.
    /// </summary>
    [JsonPropertyName("zerosNBlanksInitialisedCsePersonResource")]
    public CsePersonResource ZerosNBlanksInitialisedCsePersonResource
    {
      get => zerosNBlanksInitialisedCsePersonResource ??= new();
      set => zerosNBlanksInitialisedCsePersonResource = value;
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
    /// A value of InitialisedToMaxDate.
    /// </summary>
    [JsonPropertyName("initialisedToMaxDate")]
    public DateWorkArea InitialisedToMaxDate
    {
      get => initialisedToMaxDate ??= new();
      set => initialisedToMaxDate = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of TempReadDescending.
    /// </summary>
    [JsonPropertyName("tempReadDescending")]
    public Common TempReadDescending
    {
      get => tempReadDescending ??= new();
      set => tempReadDescending = value;
    }

    /// <summary>
    /// A value of MultDocsPossFromScrn.
    /// </summary>
    [JsonPropertyName("multDocsPossFromScrn")]
    public Standard MultDocsPossFromScrn
    {
      get => multDocsPossFromScrn ??= new();
      set => multDocsPossFromScrn = value;
    }

    /// <summary>
    /// A value of PrintableDocument.
    /// </summary>
    [JsonPropertyName("printableDocument")]
    public CodeValue PrintableDocument
    {
      get => printableDocument ??= new();
      set => printableDocument = value;
    }

    /// <summary>
    /// A value of BlankCsePersonResource.
    /// </summary>
    [JsonPropertyName("blankCsePersonResource")]
    public CsePersonResource BlankCsePersonResource
    {
      get => blankCsePersonResource ??= new();
      set => blankCsePersonResource = value;
    }

    /// <summary>
    /// A value of BlankCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("blankCsePersonVehicle")]
    public CsePersonVehicle BlankCsePersonVehicle
    {
      get => blankCsePersonVehicle ??= new();
      set => blankCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of BlankResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("blankResourceLocationAddress")]
    public ResourceLocationAddress BlankResourceLocationAddress
    {
      get => blankResourceLocationAddress ??= new();
      set => blankResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of BlankResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("blankResourceLienHolderAddress")]
    public ResourceLienHolderAddress BlankResourceLienHolderAddress
    {
      get => blankResourceLienHolderAddress ??= new();
      set => blankResourceLienHolderAddress = value;
    }

    /// <summary>
    /// A value of BlankExternalAgency.
    /// </summary>
    [JsonPropertyName("blankExternalAgency")]
    public ExternalAgency BlankExternalAgency
    {
      get => blankExternalAgency ??= new();
      set => blankExternalAgency = value;
    }

    /// <summary>
    /// A value of BlankLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("blankLegalActionPersonResource")]
    public LegalActionPersonResource BlankLegalActionPersonResource
    {
      get => blankLegalActionPersonResource ??= new();
      set => blankLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of BlankDateWorkArea.
    /// </summary>
    [JsonPropertyName("blankDateWorkArea")]
    public DateWorkArea BlankDateWorkArea
    {
      get => blankDateWorkArea ??= new();
      set => blankDateWorkArea = value;
    }

    private NextTranInfo null1;
    private LegalActionPersonResource legalActionPersonResource;
    private ExternalAgency externalAgency;
    private Common work;
    private BatchConvertNumToText batchConvertNumToText;
    private WorkArea workArea;
    private Common position;
    private SpDocLiteral spDocLiteral;
    private Common returnCode;
    private Code max;
    private CsePersonVehicle refreshCsePersonVehicle;
    private IncomeSource refreshIncomeSource;
    private TextWorkArea zeroFill;
    private DateWorkArea nullDate;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private ResourceLienHolderAddress zerosNBlanksInitialisedResourceLienHolderAddress;
      
    private ResourceLocationAddress zerosNBlanksInitialisedResourceLocationAddress;
      
    private CsePersonResource zerosNBlanksInitialisedCsePersonResource;
    private DateWorkArea dateWorkArea;
    private DateWorkArea initialisedToMaxDate;
    private Common userAction;
    private Common tempReadDescending;
    private Standard multDocsPossFromScrn;
    private CodeValue printableDocument;
    private CsePersonResource blankCsePersonResource;
    private CsePersonVehicle blankCsePersonVehicle;
    private ResourceLocationAddress blankResourceLocationAddress;
    private ResourceLienHolderAddress blankResourceLienHolderAddress;
    private ExternalAgency blankExternalAgency;
    private LegalActionPersonResource blankLegalActionPersonResource;
    private DateWorkArea blankDateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
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

    private CsePersonVehicle csePersonVehicle;
    private CsePersonResource csePersonResource;
    private CsePerson csePerson;
  }
#endregion
}
