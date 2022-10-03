// Program: OE_LGRQ_VALIDATE_LEGAL_REQUEST, ID: 371913156, model: 746.
// Short name: SWE00939
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
/// A program: OE_LGRQ_VALIDATE_LEGAL_REQUEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block validates Legal Request details
/// </para>
/// </summary>
[Serializable]
public partial class OeLgrqValidateLegalRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_VALIDATE_LEGAL_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqValidateLegalRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqValidateLegalRequest.
  /// </summary>
  public OeLgrqValidateLegalRequest(IContext context, Import import,
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
    // ****************************************************************************************************************
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ------------	--------------	
    // ------------------------------------------------------------------------
    // ??/??/??  ???????			Initial Development
    // 03/15/01  GVandy	PR112357	Correct read for foreign tribunals.
    // 04/24/01  GVandy	WR251		Validate office service provider (attorney).
    // 12/12/01  GVandy	PR 125461	Allow ENF only referrals to be closed, 
    // rejected
    // 					and withdrawn without a court case number.
    // 09/03/02  GVandy	PR154584	Tribunal incorrectly copies forward when no 
    // court case
    // 					number on subsequent referrals.
    // 12/03/10  GVandy	CQ109		Remove referral reason 5 from all views.
    // 08/18/11  RMathews      CQ29634         Revise edit for missing referral 
    // reasons.
    // 05/07/21  Raj           CQ68844         Modified to handle new 
    // modification Legal Request Reason Codes.
    //                                         
    // 1. Add new edit for requirement of Open
    // ENF Legal Referral Reason Code
    //                                            
    // for Modification Request from CP (MOC)
    // reason code, Error Code 37.
    //                                         
    // 2. Restrict ENF reason set to Closure/
    // Withdrawn/Rejected/Denied/
    //                                            
    // Returned when Modification request from CP
    // referral reason with
    //                                            
    // Open Status, Error Code 38.
    // ***************************************************************************************************************
    // ---------------------------------------------
    // Move imports to exports
    // ---------------------------------------------
    export.LegalReferral.Assign(import.LegalReferral);
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.ServiceProvider.Assign(import.ServiceProvider);
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Validate Legal Request Details now.
    // ---------------------------------------------
    export.LastErrorEntryNo.Count = 0;
    export.ErrorCodes.Index = -1;

    if (IsEmpty(import.Case1.Number))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 3;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (!ReadCase())
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 1;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (Equal(global.Command, "CREATE"))
    {
      if (import.CaseRoleReferred.IsEmpty)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 5;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    if (Equal(export.LegalReferral.ReferralReason1, "ENF") || Equal
      (export.LegalReferral.ReferralReason2, "ENF") || Equal
      (export.LegalReferral.ReferralReason3, "ENF") || Equal
      (export.LegalReferral.ReferralReason4, "ENF"))
    {
      // *
      // * One of the five reasons is ENF
      // 
      // *
      local.EnfReasonUsed.Flag = "Y";
    }

    if (!Equal(export.LegalReferral.ReferralReason1, "ENF") && !
      IsEmpty(export.LegalReferral.ReferralReason1) || !
      Equal(export.LegalReferral.ReferralReason2, "ENF") && !
      IsEmpty(export.LegalReferral.ReferralReason2) || !
      Equal(export.LegalReferral.ReferralReason3, "ENF") && !
      IsEmpty(export.LegalReferral.ReferralReason3) || !
      Equal(export.LegalReferral.ReferralReason4, "ENF") && !
      IsEmpty(export.LegalReferral.ReferralReason4))
    {
      // *
      // * One of the five reasons is not ENF
      // 
      // *
      local.NonEnfReasonUsed.Flag = "Y";
    }

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(import.ReasonMocExists.Flag) == 'Y' && Equal
        (global.Command, "CREATE"))
      {
        local.EnfReasonExists.Flag = "";

        // ****************************************************************************************************************
        // CQ68844: When Modification from CP type(MIC) is added, there should 
        // be a open ENF Legal Referral Reason exists
        //          otheriwse Error Message Enforcement Legal Referrals Reason 
        // required to add this reason will be
        //          displayed.
        // ****************************************************************************************************************
        if (AsChar(import.ReasonEnfExists.Flag) == 'Y')
        {
          local.EnfReasonExists.Flag = "Y";
        }
        else if (ReadLegalReferral2())
        {
          local.EnfReasonExists.Flag = "Y";
        }

        if (AsChar(local.EnfReasonExists.Flag) != 'Y')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 37;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }

      // ****************************************************************************************************************
      // CQ68844: When the ENF Legal Referral Reason status is updated to  
      // Closure/Withdrawn /Rejected/Denied/Returned
      //          status and there is an Modification request from CP referral 
      // reason is in open status the system will
      //          display an error message Open Modification Request Exists, 
      // Status change not allowed.
      // ****************************************************************************************************************
      if (AsChar(import.ReasonEnfExists.Flag) == 'Y')
      {
        if (AsChar(entities.ExistingLegalReferral.Status) == 'O' || AsChar
          (export.LegalReferral.Status) == 'S')
        {
          goto Test1;
        }

        if (AsChar(import.ReasonMocExists.Flag) == 'Y')
        {
          goto Test1;
        }

        if (ReadLegalReferral3())
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 38;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }

Test1:

      // *****************************************************************
      // Attempting to create an ENFORCE request. Verify that Court Case is 
      // entered.
      // *****************************************************************
      if (!IsEmpty(import.LegalReferral.CourtCaseNumber))
      {
        // *****************************************************************
        // A Court Case Number has been entered.
        // *****************************************************************
        if (IsEmpty(import.FipsTribAddress.Country) && !
          IsEmpty(import.Fips.CountyAbbreviation) && !
          IsEmpty(import.Fips.StateAbbreviation))
        {
          // *****************************************************************
          // A Court Case has been entered. We will validate the court case.
          // *****************************************************************
        }
        else if (!IsEmpty(import.FipsTribAddress.Country) && IsEmpty
          (import.Fips.CountyAbbreviation) && IsEmpty
          (import.Fips.StateAbbreviation))
        {
          // *****************************************************************
          // A Court Case has been entered. We will validate the court case.
          // *****************************************************************
        }
        else if (AsChar(local.EnfReasonUsed.Flag) == 'Y' && IsEmpty
          (local.NonEnfReasonUsed.Flag))
        {
          // *****************************************************************
          // Do not allow user to Create a referral without Court Case if there
          // is an enforce (ENF) reason code  - added by Carl Galka
          // *****************************************************************
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 30;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
        else
        {
        }
      }
      else if (AsChar(local.EnfReasonUsed.Flag) == 'Y' && IsEmpty
        (local.NonEnfReasonUsed.Flag))
      {
        // *****************************************************************
        // Do not allow user to Create a referral without Court Case if there
        // is only an enforce (ENF) reason code  - added by Carl Galka
        // *****************************************************************
        if (Equal(global.Command, "UPDATE") && (
          AsChar(export.LegalReferral.Status) == 'C' || AsChar
          (export.LegalReferral.Status) == 'R' || AsChar
          (export.LegalReferral.Status) == 'W'))
        {
          // -- Allow an ENF only referral to be Closed, Rejected, or Withdrawn 
          // regardless
          // of whether a court case number is entered.
          goto Test2;
        }

        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 30;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

Test2:

    if ((!IsEmpty(import.FipsTribAddress.Country) || !
      IsEmpty(import.Fips.CountyAbbreviation) || !
      IsEmpty(import.Fips.StateAbbreviation)) && IsEmpty
      (import.LegalReferral.CourtCaseNumber) || !
      IsEmpty(import.LegalReferral.CourtCaseNumber) && (
        (IsEmpty(import.Fips.StateAbbreviation) || IsEmpty
      (import.Fips.CountyAbbreviation)) && IsEmpty
      (import.FipsTribAddress.Country) || (
        !IsEmpty(import.Fips.CountyAbbreviation) || !
      IsEmpty(import.Fips.StateAbbreviation)) && !
      IsEmpty(import.FipsTribAddress.Country)))
    {
      // *****************************************************************
      // Do not allow user to Create a referral without a valid  Court Case
      // combination  - added by Carl Galka
      // *****************************************************************
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 31;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(import.LegalReferral.CourtCaseNumber))
      {
        // 09/03/02 GVandy  PR154584  If court case number is spaces then clear 
        // the tribunal field.
        export.LegalReferral.TribunalId = 0;
      }
      else
      {
        // *****************************************************************
        // Verify the Court Case is related to the CSE Case - added by Carl 
        // Galka
        // *****************************************************************
        if (IsEmpty(import.FipsTribAddress.Country))
        {
          // *****************************************************************
          // Verify the Court Case by using state and city - added by Carl Galka
          // *****************************************************************
          if (ReadCaseTribunal1())
          {
            export.LegalReferral.CourtCaseNumber =
              import.LegalReferral.CourtCaseNumber ?? "";
            export.LegalReferral.TribunalId = entities.Tribunal.Identifier;
          }
          else
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 29;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
        else
        {
          // *****************************************************************
          // Verify the Court Case by using country - added by Carl Galka
          // *****************************************************************
          if (ReadCaseTribunal2())
          {
            export.LegalReferral.CourtCaseNumber =
              import.LegalReferral.CourtCaseNumber ?? "";
            export.LegalReferral.TribunalId = entities.Tribunal.Identifier;
          }
          else
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 29;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
      }
    }

    // -- 04/24/2001 GVandy  WR251 validate the attorney office service 
    // provider.
    if (Equal(global.Command, "CREATE"))
    {
      if (IsEmpty(export.ServiceProvider.UserId))
      {
        export.ServiceProvider.Assign(local.NullServiceProvider);
        export.Office.SystemGeneratedId = local.NullOffice.SystemGeneratedId;
        MoveOfficeServiceProvider(local.NullOfficeServiceProvider,
          export.OfficeServiceProvider);

        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 32;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (export.ServiceProvider.SystemGeneratedId != 0)
      {
        if (ReadServiceProvider1())
        {
          if (!Equal(entities.ServiceProvider.UserId,
            export.ServiceProvider.UserId))
          {
            // --  The user has changed the attorney user id from a previous 
            // value.  Set the export service provider sysgenid to 0.  The new
            // userid will be evaluated below.
            export.ServiceProvider.SystemGeneratedId = 0;
            export.Office.SystemGeneratedId =
              local.NullOffice.SystemGeneratedId;
            MoveOfficeServiceProvider(local.NullOfficeServiceProvider,
              export.OfficeServiceProvider);

            goto Test3;
          }
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 36;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        if (ReadOfficeServiceProviderOffice1())
        {
          local.Code.CodeName = "ATTORNEY CODES";
          local.CodeValue.Cdvalue = entities.OfficeServiceProvider.RoleCode;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
            export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.OfficeServiceProvider);
            export.ServiceProvider.Assign(entities.ServiceProvider);
          }
          else
          {
            // -- The office service provider does not have an "attorney" role.
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 34;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          }
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 36;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }

Test3:

      if (export.ServiceProvider.SystemGeneratedId == 0)
      {
        if (ReadServiceProvider2())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          export.ServiceProvider.SystemGeneratedId = 0;
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 33;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }

        local.Common.Count = 0;

        foreach(var item in ReadOfficeServiceProviderOffice2())
        {
          local.Code.CodeName = "ATTORNEY CODES";
          local.CodeValue.Cdvalue = entities.OfficeServiceProvider.RoleCode;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            // -- The office service provider does not have an "attorney" role.
            // Get the next office service provider record.
            continue;
          }

          ++local.Common.Count;
          local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          local.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
        }

        switch(local.Common.Count)
        {
          case 0:
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 34;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
          case 1:
            export.ServiceProvider.Assign(entities.ServiceProvider);
            export.Office.SystemGeneratedId = local.Office.SystemGeneratedId;
            MoveOfficeServiceProvider(local.OfficeServiceProvider,
              export.OfficeServiceProvider);

            break;
          default:
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 35;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

            return;
        }
      }
    }

    // ---------------------------------------------
    // For UPDATE or DELETE, LEGAL_REFERRAL must exist.
    // ---------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (!ReadLegalReferral1())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 2;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    // ----------------------------------------------------------
    // For Update, If COURT CASE changes, we cannot have another
    // 
    // (S)ent or (O)pen referral with the changed court case number
    // ---------------------------------------------------------
    // ---------------------------------------------
    // For DELETE, no further validation is required.
    // ---------------------------------------------
    // ---------------------------------------------
    // For CREATE and UPDATE, validate the individual fields.
    // ---------------------------------------------
    if (Equal(global.Command, "CREATE"))
    {
      for(import.CaseRoleReferred.Index = 0; import.CaseRoleReferred.Index < import
        .CaseRoleReferred.Count; ++import.CaseRoleReferred.Index)
      {
        if (!ReadCaseRole())
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 11;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
    }

    if (Equal(global.Command, "CREATE"))
    {
      export.LegalReferral.ReferredByUserId = global.UserId;
    }

    if (Equal(export.LegalReferral.ReferralDate, null))
    {
      export.LegalReferral.ReferralDate = Now().Date;
    }

    if (!IsEmpty(import.LegalReferral.Status))
    {
      if (Equal(import.LegalReferral.StatusDate, null))
      {
        export.LegalReferral.StatusDate = Now().Date;
      }
      else if (Lt(Now().Date, import.LegalReferral.StatusDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 12;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      local.Code.CodeName = "LEGAL REFERRAL STATUS";
      local.CodeValue.Cdvalue = export.LegalReferral.Status ?? Spaces(10);
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 10;
        export.ErrorCodes.Update.ErrorItemNo.Count = local.ErrorItemNo.Count;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }
    else if (!Equal(import.LegalReferral.StatusDate, null))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 13;
      export.ErrorCodes.Update.ErrorItemNo.Count = local.ErrorItemNo.Count;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    // ---------------------------------------------
    // Start of Code (Raju 01/29/97:11:00 hrs CST)
    // ---------------------------------------------
    // ---------------------------------------------
    // Referral Reasons must satisfy foll. conditions
    //   - all 5 reasons cannot be spaces
    //   - must exist in code value entity
    //   - cannot be duplicated
    //   - for case of add, no code value can be
    //       CV ( conversion )
    //   - cannot enter a new reason code CV for
    //     update
    // ---------------------------------------------
    local.CountRefReasonSpaces.Count = 0;

    for(local.MultiReason.Index = 0; local.MultiReason.Index < 4; ++
      local.MultiReason.Index)
    {
      if (!local.MultiReason.CheckSize())
      {
        break;
      }

      local.PrevGroupMultiReason.Index = local.MultiReason.Index;
      local.PrevGroupMultiReason.CheckSize();

      switch(local.MultiReason.Index + 1)
      {
        case 1:
          local.MultiReason.Update.CodeValue.Cdvalue =
            export.LegalReferral.ReferralReason1;
          local.PrevGroupMultiReason.Update.Prev.Cdvalue =
            import.PrevRead.ReferralReason1;

          break;
        case 2:
          local.MultiReason.Update.CodeValue.Cdvalue =
            export.LegalReferral.ReferralReason2;
          local.PrevGroupMultiReason.Update.Prev.Cdvalue =
            import.PrevRead.ReferralReason2;

          break;
        case 3:
          local.MultiReason.Update.CodeValue.Cdvalue =
            export.LegalReferral.ReferralReason3;
          local.PrevGroupMultiReason.Update.Prev.Cdvalue =
            import.PrevRead.ReferralReason3;

          break;
        case 4:
          local.MultiReason.Update.CodeValue.Cdvalue =
            export.LegalReferral.ReferralReason4;
          local.PrevGroupMultiReason.Update.Prev.Cdvalue =
            import.PrevRead.ReferralReason4;

          break;
        default:
          break;
      }

      if (Equal(global.Command, "CREATE"))
      {
        if (Equal(local.MultiReason.Item.CodeValue.Cdvalue, "CV"))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 19;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          break;
        }
      }

      if (Equal(global.Command, "UPDATE"))
      {
        if (Equal(local.MultiReason.Item.CodeValue.Cdvalue, "CV") && !
          Equal(local.PrevGroupMultiReason.Item.Prev.Cdvalue, "CV"))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 26;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          break;
        }
      }

      if (IsEmpty(local.MultiReason.Item.CodeValue.Cdvalue))
      {
        local.CountRefReasonSpaces.Count =
          (int)((long)local.CountRefReasonSpaces.Count + 1);
      }
      else
      {
        // ---------------------------------------------
        // Before making an i/o , check whether any
        //    non space referral reason is relicated.
        //    Case yes : just quit with error message
        // ---------------------------------------------
        switch(local.MultiReason.Index + 1)
        {
          case 1:
            if (Equal(local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason2) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason3) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason4))
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 20;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            }

            break;
          case 2:
            if (Equal(local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason1) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason3) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason4))
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 21;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            }

            break;
          case 3:
            if (Equal(local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason1) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason2) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason4))
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 22;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            }

            break;
          case 4:
            if (Equal(local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason1) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason2) || Equal
              (local.MultiReason.Item.CodeValue.Cdvalue,
              export.LegalReferral.ReferralReason3))
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 23;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            }

            break;
          default:
            break;
        }

        local.Code.CodeName = "LEGAL REFERRAL REASON";
        local.CodeValue.Cdvalue = local.MultiReason.Item.CodeValue.Cdvalue;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          // ---------------------------------------------
          // Error code 6 will not be used at all.
          //   Instead the last one being 13,
          //   the new error codes for each reason
          //   will be 13 + subscript
          //   This will allow us to know which
          //   of the 5 reason codes was erroneous
          // ---------------------------------------------
          export.ErrorCodes.Update.DetailErrorCode.Count =
            local.MultiReason.Index + 14;
          export.ErrorCodes.Update.ErrorItemNo.Count = local.ErrorItemNo.Count;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ReasonCode.Text8 = TrimEnd(export.ReasonCode.Text8) + "1";
        }
        else
        {
          export.ReasonCode.Text8 = TrimEnd(export.ReasonCode.Text8) + "0";
        }
      }
    }

    local.MultiReason.CheckIndex();

    if (!Equal(TrimEnd(export.ReasonCode.Text8), "00000") && !
      IsEmpty(export.ReasonCode.Text8))
    {
      return;
    }

    // CQ29634 Changed to check for four referral reason codes instead of five 
    // due to CQ109.
    if (local.CountRefReasonSpaces.Count == 4)
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 7;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (Equal(global.Command, "UPDATE"))
    {
      if (!Equal(entities.ExistingLegalReferral.ReferredByUserId, global.UserId) &&
        AsChar(export.LegalReferral.Status) == 'C')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 25;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    // ---------------------------------------------
    // End   of Code
    // ---------------------------------------------
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.CaseRoleReferred.Item.CsePerson.Number);
        db.SetInt32(
          command, "caseRoleId",
          import.CaseRoleReferred.Item.CaseRole.Identifier);
        db.SetNullableDate(
          command, "startDate",
          import.CaseRoleReferred.Item.CaseRole.StartDate.GetValueOrDefault());
        db.SetString(
          command, "type", import.CaseRoleReferred.Item.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseTribunal1()
  {
    entities.Tribunal.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseTribunal1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalReferral.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", import.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", import.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Populated = true;
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseTribunal2()
  {
    entities.Tribunal.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseTribunal2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalReferral.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "country", import.FipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Populated = true;
        entities.Case1.Populated = true;
      });
  }

  private bool ReadLegalReferral1()
  {
    entities.ExistingLegalReferral.Populated = false;

    return Read("ReadLegalReferral1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalReferral.CasNumber = db.GetString(reader, 0);
        entities.ExistingLegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLegalReferral.ReferredByUserId =
          db.GetString(reader, 2);
        entities.ExistingLegalReferral.StatusDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingLegalReferral.Status = db.GetNullableString(reader, 4);
        entities.ExistingLegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.ExistingLegalReferral.ReferralReason1 =
          db.GetString(reader, 6);
        entities.ExistingLegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral2()
  {
    entities.ExistingValidation.Populated = false;

    return Read("ReadLegalReferral2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalReferral.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingValidation.CasNumber = db.GetString(reader, 0);
        entities.ExistingValidation.Identifier = db.GetInt32(reader, 1);
        entities.ExistingValidation.ReferredByUserId = db.GetString(reader, 2);
        entities.ExistingValidation.StatusDate = db.GetNullableDate(reader, 3);
        entities.ExistingValidation.Status = db.GetNullableString(reader, 4);
        entities.ExistingValidation.ReferralDate = db.GetDate(reader, 5);
        entities.ExistingValidation.ReferralReason1 = db.GetString(reader, 6);
        entities.ExistingValidation.ReferralReason2 = db.GetString(reader, 7);
        entities.ExistingValidation.ReferralReason3 = db.GetString(reader, 8);
        entities.ExistingValidation.ReferralReason4 = db.GetString(reader, 9);
        entities.ExistingValidation.ReferralReason5 = db.GetString(reader, 10);
        entities.ExistingValidation.CourtCaseNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingValidation.Populated = true;
      });
  }

  private bool ReadLegalReferral3()
  {
    entities.ExistingValidation.Populated = false;

    return Read("ReadLegalReferral3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalReferral.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingValidation.CasNumber = db.GetString(reader, 0);
        entities.ExistingValidation.Identifier = db.GetInt32(reader, 1);
        entities.ExistingValidation.ReferredByUserId = db.GetString(reader, 2);
        entities.ExistingValidation.StatusDate = db.GetNullableDate(reader, 3);
        entities.ExistingValidation.Status = db.GetNullableString(reader, 4);
        entities.ExistingValidation.ReferralDate = db.GetDate(reader, 5);
        entities.ExistingValidation.ReferralReason1 = db.GetString(reader, 6);
        entities.ExistingValidation.ReferralReason2 = db.GetString(reader, 7);
        entities.ExistingValidation.ReferralReason3 = db.GetString(reader, 8);
        entities.ExistingValidation.ReferralReason4 = db.GetString(reader, 9);
        entities.ExistingValidation.ReferralReason5 = db.GetString(reader, 10);
        entities.ExistingValidation.CourtCaseNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingValidation.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOffice1()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOffice1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", export.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOffice2()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOffice2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", export.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", export.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A CaseRoleReferredGroup group.</summary>
    [Serializable]
    public class CaseRoleReferredGroup
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private CaseRole caseRole;
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
    /// Gets a value of CaseRoleReferred.
    /// </summary>
    [JsonIgnore]
    public Array<CaseRoleReferredGroup> CaseRoleReferred =>
      caseRoleReferred ??= new(CaseRoleReferredGroup.Capacity);

    /// <summary>
    /// Gets a value of CaseRoleReferred for json serialization.
    /// </summary>
    [JsonPropertyName("caseRoleReferred")]
    [Computed]
    public IList<CaseRoleReferredGroup> CaseRoleReferred_Json
    {
      get => caseRoleReferred;
      set => CaseRoleReferred.Assign(value);
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of PrevRead.
    /// </summary>
    [JsonPropertyName("prevRead")]
    public LegalReferral PrevRead
    {
      get => prevRead ??= new();
      set => prevRead = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of ReasonMicExists.
    /// </summary>
    [JsonPropertyName("reasonMicExists")]
    public Common ReasonMicExists
    {
      get => reasonMicExists ??= new();
      set => reasonMicExists = value;
    }

    /// <summary>
    /// A value of ReasonMinExists.
    /// </summary>
    [JsonPropertyName("reasonMinExists")]
    public Common ReasonMinExists
    {
      get => reasonMinExists ??= new();
      set => reasonMinExists = value;
    }

    /// <summary>
    /// A value of ReasonMocExists.
    /// </summary>
    [JsonPropertyName("reasonMocExists")]
    public Common ReasonMocExists
    {
      get => reasonMocExists ??= new();
      set => reasonMocExists = value;
    }

    /// <summary>
    /// A value of ReasonMonExists.
    /// </summary>
    [JsonPropertyName("reasonMonExists")]
    public Common ReasonMonExists
    {
      get => reasonMonExists ??= new();
      set => reasonMonExists = value;
    }

    /// <summary>
    /// A value of ReasonMooExists.
    /// </summary>
    [JsonPropertyName("reasonMooExists")]
    public Common ReasonMooExists
    {
      get => reasonMooExists ??= new();
      set => reasonMooExists = value;
    }

    /// <summary>
    /// A value of ReasonEnfExists.
    /// </summary>
    [JsonPropertyName("reasonEnfExists")]
    public Common ReasonEnfExists
    {
      get => reasonEnfExists ??= new();
      set => reasonEnfExists = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<CaseRoleReferredGroup> caseRoleReferred;
    private LegalReferral legalReferral;
    private Case1 case1;
    private LegalReferral prevRead;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private Common reasonMicExists;
    private Common reasonMinExists;
    private Common reasonMocExists;
    private Common reasonMonExists;
    private Common reasonMooExists;
    private Common reasonEnfExists;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of ErrorItemNo.
      /// </summary>
      [JsonPropertyName("errorItemNo")]
      public Common ErrorItemNo
      {
        get => errorItemNo ??= new();
        set => errorItemNo = value;
      }

      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common errorItemNo;
      private Common detailErrorCode;
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
    /// A value of ReasonCode.
    /// </summary>
    [JsonPropertyName("reasonCode")]
    public TextWorkArea ReasonCode
    {
      get => reasonCode ??= new();
      set => reasonCode = value;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private TextWorkArea reasonCode;
    private Common lastErrorEntryNo;
    private LegalReferral legalReferral;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A MultiReasonGroup group.</summary>
    [Serializable]
    public class MultiReasonGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CodeValue codeValue;
    }

    /// <summary>A PrevGroupMultiReasonGroup group.</summary>
    [Serializable]
    public class PrevGroupMultiReasonGroup
    {
      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public CodeValue Prev
      {
        get => prev ??= new();
        set => prev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CodeValue prev;
    }

    /// <summary>
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
    }

    /// <summary>
    /// A value of NullOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("nullOfficeServiceProvider")]
    public OfficeServiceProvider NullOfficeServiceProvider
    {
      get => nullOfficeServiceProvider ??= new();
      set => nullOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NullOffice.
    /// </summary>
    [JsonPropertyName("nullOffice")]
    public Office NullOffice
    {
      get => nullOffice ??= new();
      set => nullOffice = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of NonEnfReasonUsed.
    /// </summary>
    [JsonPropertyName("nonEnfReasonUsed")]
    public Common NonEnfReasonUsed
    {
      get => nonEnfReasonUsed ??= new();
      set => nonEnfReasonUsed = value;
    }

    /// <summary>
    /// A value of EnfReasonUsed.
    /// </summary>
    [JsonPropertyName("enfReasonUsed")]
    public Common EnfReasonUsed
    {
      get => enfReasonUsed ??= new();
      set => enfReasonUsed = value;
    }

    /// <summary>
    /// A value of ErrorItemNo.
    /// </summary>
    [JsonPropertyName("errorItemNo")]
    public Common ErrorItemNo
    {
      get => errorItemNo ??= new();
      set => errorItemNo = value;
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
    /// Gets a value of MultiReason.
    /// </summary>
    [JsonIgnore]
    public Array<MultiReasonGroup> MultiReason => multiReason ??= new(
      MultiReasonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MultiReason for json serialization.
    /// </summary>
    [JsonPropertyName("multiReason")]
    [Computed]
    public IList<MultiReasonGroup> MultiReason_Json
    {
      get => multiReason;
      set => MultiReason.Assign(value);
    }

    /// <summary>
    /// A value of CountRefReasonSpaces.
    /// </summary>
    [JsonPropertyName("countRefReasonSpaces")]
    public Common CountRefReasonSpaces
    {
      get => countRefReasonSpaces ??= new();
      set => countRefReasonSpaces = value;
    }

    /// <summary>
    /// Gets a value of PrevGroupMultiReason.
    /// </summary>
    [JsonIgnore]
    public Array<PrevGroupMultiReasonGroup> PrevGroupMultiReason =>
      prevGroupMultiReason ??= new(PrevGroupMultiReasonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrevGroupMultiReason for json serialization.
    /// </summary>
    [JsonPropertyName("prevGroupMultiReason")]
    [Computed]
    public IList<PrevGroupMultiReasonGroup> PrevGroupMultiReason_Json
    {
      get => prevGroupMultiReason;
      set => PrevGroupMultiReason.Assign(value);
    }

    /// <summary>
    /// A value of EnfReasonExists.
    /// </summary>
    [JsonPropertyName("enfReasonExists")]
    public Common EnfReasonExists
    {
      get => enfReasonExists ??= new();
      set => enfReasonExists = value;
    }

    private ServiceProvider nullServiceProvider;
    private OfficeServiceProvider nullOfficeServiceProvider;
    private Office nullOffice;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private DateWorkArea current;
    private Common common;
    private Common nonEnfReasonUsed;
    private Common enfReasonUsed;
    private Common errorItemNo;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Array<MultiReasonGroup> multiReason;
    private Common countRefReasonSpaces;
    private Array<PrevGroupMultiReasonGroup> prevGroupMultiReason;
    private Common enfReasonExists;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of ExistingLegalReferral.
    /// </summary>
    [JsonPropertyName("existingLegalReferral")]
    public LegalReferral ExistingLegalReferral
    {
      get => existingLegalReferral ??= new();
      set => existingLegalReferral = value;
    }

    /// <summary>
    /// A value of ExistingValidation.
    /// </summary>
    [JsonPropertyName("existingValidation")]
    public LegalReferral ExistingValidation
    {
      get => existingValidation ??= new();
      set => existingValidation = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private Tribunal tribunal;
    private Case1 case1;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private LegalReferral existingLegalReferral;
    private LegalReferral existingValidation;
    private Case1 existingCase;
  }
#endregion
}
