// Program: SI_CREATE_CASE_ROLE, ID: 371727793, model: 746.
// Short name: SWE01121
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
/// A program: SI_CREATE_CASE_ROLE.
/// </para>
/// <para>
/// RESP: SRVINIT	
/// This PAD creates a Case Role for a CSE Person on a specified case.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateCaseRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_CASE_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateCaseRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateCaseRole.
  /// </summary>
  public SiCreateCaseRole(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // This PAD creates a CASE ROLE for a CSE Person
    // within a given CASE.
    // ---------------------------------------------
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // ??/??/??  ??????????	        Initial Development
    // ------------------------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 01/31/01  M. Lachowicz          Use previous information
    //                                 
    // if person already played import
    // role.
    //                                 
    // ------------------------------------------------------------
    // 04/03/01  M. Lachowicz          Use previous information
    //                                 
    // if person already played import
    // role
    //                                 
    // on other cases as well.
    //                                 
    // ------------------------------------------------------------
    // 08/27/01  M. Lachowicz          Changes made due to WR208.
    //                                 
    // ------------------------------------------------------------
    // 11/94/2009  T. Pierce          Made changes to CREATE statement to use a
    //                                
    // new local view instead of the "
    // previous" entity action view
    //                                
    // of Case Role to accommodate
    // scenarios when the
    //                                
    // "previous" view is not
    // populated.
    //                                 
    // ------------------------------------------------------------
    local.MoIsAp.Flag = "N";
    local.FaIsAp.Flag = "N";
    local.CurrentDateWorkArea.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // 08/27/2001 M.Lachowicz Start
    if (Equal(import.CaseRole.EndDate, local.Zero.Date))
    {
      local.CaseRole.EndDate = local.Max.Date;
    }
    else
    {
      local.CaseRole.EndDate = import.CaseRole.EndDate;
    }

    // 08/27/2001 M.Lachowicz End
    // 06/22/99 M.L
    //          Change property of the following READ to generate
    //          SELECT ONLY
    if (ReadCase())
    {
      // 06/22/99 M.L
      //          Change property of the following READ EACH to OPTIMIZE FOR 1 
      // ROW
      if (ReadCaseRole3())
      {
        local.LastCase.Identifier = entities.Related.Identifier;
      }

      // 06/22/99 M.L
      //          Change property of the following READ to generate
      //          SELECT ONLY
      if (ReadCsePerson())
      {
        // 01/31/01 M.L Start
        if (ReadCaseRole1())
        {
          local.Previous.Assign(entities.Previous);

          // 04/03/01 M.L Start
          local.CaseFound.Flag = "Y";

          // 04/03/01 M.L End
        }

        // 04/03/01 M.L Start
        if (AsChar(local.CaseFound.Flag) == 'Y')
        {
        }
        else if (ReadCaseRole2())
        {
          local.Previous.Assign(entities.Previous);
        }

        // 04/03/01 M.L End
        if (IsEmpty(import.CaseRole.RelToAr))
        {
          if (entities.Previous.Populated)
          {
            local.CaseRole.RelToAr = entities.Previous.RelToAr;
          }
        }
        else
        {
          local.CaseRole.RelToAr = import.CaseRole.RelToAr ?? "";
        }

        // 01/31/01 M.L End
        local.CurrentCsePerson.Number = entities.CsePerson.Number;
        ++local.LastCase.Identifier;

        // 04/03/01 M.L Disable two lines.
        try
        {
          CreateCaseRole();
          ExitState = "ACO_NN0000_ALL_OK";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

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
    }
    else
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    if (AsChar(import.FromRole.Flag) == 'Y')
    {
    }
    else
    {
      return;
    }

    // Depending on the type of Case Role successfully created and possible past
    // relationships of the CSE Person filling the new Case Role to the current
    // Case, create the appropriate Infrastructure occurrences.  Note that
    // currency has been obtained at this point for CASE and CSE_PERSON.
    // Populate the non-variable attributes for the local Infrastructure 
    // occurrence.
    // 06/22/99 M.L
    //          Change property of the following READ to generate
    //          CURSOR ONLY
    if (ReadInterstateRequest())
    {
      local.Infrastructure.InitiatingStateCode = "OS";
    }
    else
    {
      local.Infrastructure.InitiatingStateCode = "KS";
    }

    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.CaseNumber = entities.Case1.Number;
    local.Infrastructure.CsePersonNumber = local.CurrentCsePerson.Number;
    local.Infrastructure.ReferenceDate = local.CurrentDateWorkArea.Date;
    local.Infrastructure.UserId = "ROLE";
    local.Infrastructure.BusinessObjectCd = "CAU";
    UseCabConvertDate2String();

    switch(TrimEnd(import.CaseRole.Type1))
    {
      case "AP":
        break;
      case "CH":
        break;
      case "MO":
        // Determine if the new MO role is currently participating on the 
        // current Case as an AP.
        // 06/22/99 M.L
        //          Change property of the following READ to generate
        //          SELECT ONLY
        if (ReadCaseRole4())
        {
          local.MoIsAp.Flag = "Y";
          local.Infrastructure.EventId = 11;
          local.Infrastructure.ReasonCode = "APASSGNMO";

          foreach(var item in ReadCaseUnit1())
          {
            local.WorkCu.Text15 =
              NumberToString(entities.CaseUnit.CuNumber, 15);
            local.WorkCu.Text15 = Substring(local.WorkCu.Text15, 13, 3);
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.Infrastructure.Detail =
              "Mother Role asgn to AP Role for Case " + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ,CU ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + TrimEnd
              (local.WorkCu.Text15);
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " on ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        // Determine if the new MO role is currently participating on the 
        // current Case as an AR.
        // If MO is AP from previous reads, do not read for MO is AR.
        if (AsChar(local.MoIsAp.Flag) == 'Y')
        {
        }
        else
        {
          // 06/22/99 M.L
          //          Change property of the following READ to generate
          //          SELECT ONLY
          if (ReadCaseRole7())
          {
            local.Infrastructure.EventId = 11;
            local.Infrastructure.ReasonCode = "ARISMO";

            foreach(var item in ReadCaseUnit2())
            {
              ++local.CaseUnit.Count;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
              local.WorkCu.Text15 =
                NumberToString(entities.CaseUnit.CuNumber, 15);
              local.WorkCu.Text15 = Substring(local.WorkCu.Text15, 13, 3);
              local.Infrastructure.Detail =
                "Mother Role asgn to AR Role for Case " + entities
                .Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " ,CU ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + TrimEnd
                (local.WorkCu.Text15);
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " on ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // If no Case Unit based Infrastructure occurrences were raised 
            // during the previous Read Each Case Unit,
            // raise a Case level Infrastructure occurrence for the AR is MO 
            // event.
            if (local.CaseUnit.Count == 0)
            {
              local.Infrastructure.BusinessObjectCd = "CAS";
              local.Infrastructure.CaseUnitNumber = 0;
              local.Infrastructure.Detail =
                "Mother Role asgn to AR Role for Case " + entities
                .Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " on ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
            }
          }
        }

        break;
      case "FA":
        // Determine if the new FA role is currently participating on the 
        // current Case as an AP.
        // 06/22/99 M.L
        //          Change property of the following READ to generate
        //          SELECT ONLY
        if (ReadCaseRole5())
        {
          local.FaIsAp.Flag = "Y";
          local.Infrastructure.EventId = 11;
          local.Infrastructure.ReasonCode = "APASSGNFA";

          foreach(var item in ReadCaseUnit1())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            local.WorkCu.Text15 =
              NumberToString(entities.CaseUnit.CuNumber, 15);
            local.WorkCu.Text15 = Substring(local.WorkCu.Text15, 13, 3);
            local.Infrastructure.Detail =
              "Father Role asgn to AP Role for Case " + entities.Case1.Number;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " ,CU ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + TrimEnd
              (local.WorkCu.Text15);
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " on ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        // Determine if the new FA role is currently participating on the 
        // current Case as an AR.
        // If the FA is AP flag is Y, do not check for FA is AR.
        if (AsChar(local.FaIsAp.Flag) == 'Y')
        {
        }
        else
        {
          // 06/22/99 M.L
          //          Change property of the following READ to generate
          //          SELECT ONLY
          if (ReadCaseRole6())
          {
            local.Infrastructure.EventId = 11;
            local.Infrastructure.ReasonCode = "ARISFA";

            foreach(var item in ReadCaseUnit2())
            {
              ++local.CaseUnit.Count;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
              local.WorkCu.Text15 =
                NumberToString(entities.CaseUnit.CuNumber, 15);
              local.WorkCu.Text15 = Substring(local.WorkCu.Text15, 13, 3);
              local.Infrastructure.Detail =
                "Father Role asgn to AR Role for Case " + entities
                .Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " ,CU ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + TrimEnd
                (local.WorkCu.Text15);
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " on ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // If no Case Unit based Infrastructure occurrences were raised 
            // during the previous Read Each Case Unit,
            // raise a Case level Infrastructure occurrence for the AR is FA 
            // event.
            if (local.CaseUnit.Count == 0)
            {
              local.Infrastructure.BusinessObjectCd = "CAS";
              local.Infrastructure.CaseUnitNumber = 0;
              local.Infrastructure.Detail =
                "Father Role asgn to AR Role for Case " + entities
                .Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " on ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
            }
          }
        }

        break;
      default:
        ExitState = "INVALID_CASE_ROLE_TYPE";

        break;
    }
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.CurrentDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void CreateCaseRole()
  {
    var casNumber = entities.Case1.Number;
    var cspNumber = entities.CsePerson.Number;
    var type1 = import.CaseRole.Type1;
    var identifier = local.LastCase.Identifier;
    var startDate = import.CaseRole.StartDate;
    var endDate = local.CaseRole.EndDate;
    var onSsInd = local.Previous.OnSsInd ?? "";
    var healthInsuranceIndicator = local.Previous.HealthInsuranceIndicator ?? ""
      ;
    var medicalSupportIndicator = local.Previous.MedicalSupportIndicator ?? "";
    var mothersFirstName = local.Previous.MothersFirstName ?? "";
    var mothersMiddleInitial = local.Previous.MothersMiddleInitial ?? "";
    var fathersLastName = local.Previous.FathersLastName ?? "";
    var fathersMiddleInitial = local.Previous.FathersMiddleInitial ?? "";
    var fathersFirstName = local.Previous.FathersFirstName ?? "";
    var mothersMaidenLastName = local.Previous.MothersMaidenLastName ?? "";
    var parentType = local.Previous.ParentType ?? "";
    var notifiedDate = local.Previous.NotifiedDate;
    var numberOfChildren = local.Previous.NumberOfChildren.GetValueOrDefault();
    var livingWithArIndicator = local.Previous.LivingWithArIndicator ?? "";
    var nonpaymentCategory = local.Previous.NonpaymentCategory ?? "";
    var contactFirstName = local.Previous.ContactFirstName ?? "";
    var contactMiddleInitial = local.Previous.ContactMiddleInitial ?? "";
    var contactPhone = local.Previous.ContactPhone ?? "";
    var contactLastName = local.Previous.ContactLastName ?? "";
    var childCareExpenses =
      local.Previous.ChildCareExpenses.GetValueOrDefault();
    var assignmentDate = local.Previous.AssignmentDate;
    var assignmentTerminationCode =
      local.Previous.AssignmentTerminationCode ?? "";
    var assignmentOfRights = local.Previous.AssignmentOfRights ?? "";
    var assignmentTerminatedDt = local.Previous.AssignmentTerminatedDt;
    var priorMedicalSupport =
      local.Previous.PriorMedicalSupport.GetValueOrDefault();
    var dateOfEmancipation = local.Previous.DateOfEmancipation;
    var fcAdoptionDisruptionInd = local.Previous.FcAdoptionDisruptionInd ?? "";
    var fcApNotified = local.Previous.FcApNotified ?? "";
    var fcCincInd = local.Previous.FcCincInd ?? "";
    var fcCostOfCare = local.Previous.FcCostOfCare.GetValueOrDefault();
    var fcCostOfCareFreq = local.Previous.FcCostOfCareFreq ?? "";
    var fcCountyChildRemovedFrom = local.Previous.FcCountyChildRemovedFrom ?? ""
      ;
    var fcDateOfInitialCustody = local.Previous.FcDateOfInitialCustody;
    var fcInHomeServiceInd = local.Previous.FcInHomeServiceInd ?? "";
    var fcIvECaseNumber = local.Previous.FcIvECaseNumber ?? "";
    var fcJuvenileCourtOrder = local.Previous.FcJuvenileCourtOrder ?? "";
    var fcJuvenileOffenderInd = local.Previous.FcJuvenileOffenderInd ?? "";
    var fcLevelOfCare = local.Previous.FcLevelOfCare ?? "";
    var fcNextJuvenileCtDt = local.Previous.FcNextJuvenileCtDt;
    var fcOrderEstBy = local.Previous.FcOrderEstBy ?? "";
    var fcOtherBenefitInd = local.Previous.FcOtherBenefitInd ?? "";
    var fcParentalRights = local.Previous.FcParentalRights ?? "";
    var fcPrevPayeeFirstName = local.Previous.FcPrevPayeeFirstName ?? "";
    var fcPrevPayeeMiddleInitial = local.Previous.FcPrevPayeeMiddleInitial ?? ""
      ;
    var fcPlacementDate = local.Previous.FcPlacementDate;
    var fcPlacementName = local.Previous.FcPlacementName ?? "";
    var fcPlacementReason = local.Previous.FcPlacementReason ?? "";
    var fcPreviousPa = local.Previous.FcPreviousPa ?? "";
    var fcPreviousPayeeLastName = local.Previous.FcPreviousPayeeLastName ?? "";
    var fcSourceOfFunding = local.Previous.FcSourceOfFunding ?? "";
    var fcSrsPayee = local.Previous.FcSrsPayee ?? "";
    var fcSsa = local.Previous.FcSsa ?? "";
    var fcSsi = local.Previous.FcSsi ?? "";
    var fcVaInd = local.Previous.FcVaInd ?? "";
    var fcWardsAccount = local.Previous.FcWardsAccount ?? "";
    var fcZebInd = local.Previous.FcZebInd ?? "";
    var over18AndInSchool = local.Previous.Over18AndInSchool ?? "";
    var residesWithArIndicator = local.Previous.ResidesWithArIndicator ?? "";
    var specialtyArea = local.Previous.SpecialtyArea ?? "";
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var confirmedType = local.Previous.ConfirmedType ?? "";
    var relToAr = local.CaseRole.RelToAr ?? "";
    var arChgProcReqInd = local.Previous.ArChgProcReqInd ?? "";
    var arChgProcessedDate = local.Previous.ArChgProcessedDate;
    var arInvalidInd = local.Previous.ArInvalidInd ?? "";
    var note = local.Previous.Note ?? "";

    CheckValid<CaseRole>("Type1", type1);
    CheckValid<CaseRole>("ParentType", parentType);
    CheckValid<CaseRole>("LivingWithArIndicator", livingWithArIndicator);
    CheckValid<CaseRole>("ResidesWithArIndicator", residesWithArIndicator);
    CheckValid<CaseRole>("SpecialtyArea", specialtyArea);
    entities.CaseRole.Populated = false;
    Update("CreateCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetInt32(command, "caseRoleId", identifier);
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.SetNullableString(command, "mothersFirstNm", mothersFirstName);
        db.SetNullableString(command, "mothersMidInit", mothersMiddleInitial);
        db.SetNullableString(command, "fathersLastName", fathersLastName);
        db.SetNullableString(command, "fathersMidInit", fathersMiddleInitial);
        db.SetNullableString(command, "fathersFirstName", fathersFirstName);
        db.
          SetNullableString(command, "motherMaidenLast", mothersMaidenLastName);
          
        db.SetNullableString(command, "parentType", parentType);
        db.SetNullableDate(command, "notifiedDate", notifiedDate);
        db.SetNullableInt32(command, "numberOfChildren", numberOfChildren);
        db.SetNullableString(command, "livingWithArInd", livingWithArIndicator);
        db.SetNullableString(command, "nonpaymentCat", nonpaymentCategory);
        db.SetNullableString(command, "contactFirstName", contactFirstName);
        db.SetNullableString(command, "contactMidInit", contactMiddleInitial);
        db.SetNullableString(command, "contactPhone", contactPhone);
        db.SetNullableString(command, "contactLastName", contactLastName);
        db.SetNullableDecimal(command, "childCareExpense", childCareExpenses);
        db.SetNullableDate(command, "assignmentDate", assignmentDate);
        db.SetNullableString(
          command, "assignmentTermCd", assignmentTerminationCode);
        db.SetNullableString(command, "assignOfRights", assignmentOfRights);
        db.SetNullableDate(command, "assignmentTermDt", assignmentTerminatedDt);
        db.SetNullableString(command, "absenceReasonCd", "");
        db.SetNullableString(command, "bcFathersMi", "");
        db.SetNullableString(command, "bcFatherFirstNm", "");
        db.SetNullableDecimal(command, "priorMedicalSupp", priorMedicalSupport);
        db.SetNullableString(command, "arWaivedIns", "");
        db.SetNullableString(command, "bcFatherLastNm", "");
        db.SetNullableDate(command, "emancipationDt", dateOfEmancipation);
        db.
          SetNullableString(command, "fcAdoptDisrupt", fcAdoptionDisruptionInd);
          
        db.SetNullableString(command, "fcApNotified", fcApNotified);
        db.SetNullableString(command, "fcCincInd", fcCincInd);
        db.SetNullableDecimal(command, "fcCostOfCare", fcCostOfCare);
        db.SetNullableString(command, "fcCareCostFreq", fcCostOfCareFreq);
        db.SetNullableString(
          command, "fcCountyRemFrom", fcCountyChildRemovedFrom);
        db.SetNullableDate(command, "fcInitCustodyDt", fcDateOfInitialCustody);
        db.SetNullableString(command, "fcInHmServInd", fcInHomeServiceInd);
        db.SetNullableString(command, "fcIvECaseNo", fcIvECaseNumber);
        db.SetNullableString(command, "fcJvCrtOrder", fcJuvenileCourtOrder);
        db.SetNullableString(command, "fcJvOffenderInd", fcJuvenileOffenderInd);
        db.SetNullableString(command, "fcLevelOfCare", fcLevelOfCare);
        db.SetNullableDate(command, "fcNextJvCtDt", fcNextJuvenileCtDt);
        db.SetNullableString(command, "fcOrderEstBy", fcOrderEstBy);
        db.SetNullableString(command, "fcOtherBenInd", fcOtherBenefitInd);
        db.SetNullableString(command, "fcParentalRights", fcParentalRights);
        db.SetNullableString(command, "fcPrvPayFrstNm", fcPrevPayeeFirstName);
        db.SetNullableString(command, "fcPrvPayMi", fcPrevPayeeMiddleInitial);
        db.SetNullableDate(command, "fcPlacementDate", fcPlacementDate);
        db.SetNullableString(command, "fcPlacementName", fcPlacementName);
        db.SetNullableString(command, "fcPlacementRsn", fcPlacementReason);
        db.SetNullableString(command, "fcPreviousPa", fcPreviousPa);
        db.
          SetNullableString(command, "fcPrvPayLastNm", fcPreviousPayeeLastName);
          
        db.SetNullableString(command, "fcSrceOfFunding", fcSourceOfFunding);
        db.SetNullableString(command, "fcSrsPayee", fcSrsPayee);
        db.SetNullableString(command, "fcSsa", fcSsa);
        db.SetNullableString(command, "fcSsi", fcSsi);
        db.SetNullableString(command, "fcVaInd", fcVaInd);
        db.SetNullableString(command, "fcWardsAccount", fcWardsAccount);
        db.SetNullableString(command, "fcZebInd", fcZebInd);
        db.SetNullableString(command, "inSchoolOver18", over18AndInSchool);
        db.
          SetNullableString(command, "resideWithArInd", residesWithArIndicator);
          
        db.SetNullableString(command, "specialtyArea", specialtyArea);
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "confirmedType", confirmedType);
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetNullableString(command, "arChgPrcReqInd", arChgProcReqInd);
        db.SetNullableDate(command, "arChgProcDt", arChgProcessedDate);
        db.SetNullableString(command, "arInvalidInd", arInvalidInd);
        db.SetNullableString(command, "note", note);
      });

    entities.CaseRole.CasNumber = casNumber;
    entities.CaseRole.CspNumber = cspNumber;
    entities.CaseRole.Type1 = type1;
    entities.CaseRole.Identifier = identifier;
    entities.CaseRole.StartDate = startDate;
    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.OnSsInd = onSsInd;
    entities.CaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.CaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.CaseRole.MothersFirstName = mothersFirstName;
    entities.CaseRole.MothersMiddleInitial = mothersMiddleInitial;
    entities.CaseRole.FathersLastName = fathersLastName;
    entities.CaseRole.FathersMiddleInitial = fathersMiddleInitial;
    entities.CaseRole.FathersFirstName = fathersFirstName;
    entities.CaseRole.MothersMaidenLastName = mothersMaidenLastName;
    entities.CaseRole.ParentType = parentType;
    entities.CaseRole.NotifiedDate = notifiedDate;
    entities.CaseRole.NumberOfChildren = numberOfChildren;
    entities.CaseRole.LivingWithArIndicator = livingWithArIndicator;
    entities.CaseRole.NonpaymentCategory = nonpaymentCategory;
    entities.CaseRole.ContactFirstName = contactFirstName;
    entities.CaseRole.ContactMiddleInitial = contactMiddleInitial;
    entities.CaseRole.ContactPhone = contactPhone;
    entities.CaseRole.ContactLastName = contactLastName;
    entities.CaseRole.ChildCareExpenses = childCareExpenses;
    entities.CaseRole.AssignmentDate = assignmentDate;
    entities.CaseRole.AssignmentTerminationCode = assignmentTerminationCode;
    entities.CaseRole.AssignmentOfRights = assignmentOfRights;
    entities.CaseRole.AssignmentTerminatedDt = assignmentTerminatedDt;
    entities.CaseRole.AbsenceReasonCode = "";
    entities.CaseRole.PriorMedicalSupport = priorMedicalSupport;
    entities.CaseRole.ArWaivedInsurance = "";
    entities.CaseRole.DateOfEmancipation = dateOfEmancipation;
    entities.CaseRole.FcAdoptionDisruptionInd = fcAdoptionDisruptionInd;
    entities.CaseRole.FcApNotified = fcApNotified;
    entities.CaseRole.FcCincInd = fcCincInd;
    entities.CaseRole.FcCostOfCare = fcCostOfCare;
    entities.CaseRole.FcCostOfCareFreq = fcCostOfCareFreq;
    entities.CaseRole.FcCountyChildRemovedFrom = fcCountyChildRemovedFrom;
    entities.CaseRole.FcDateOfInitialCustody = fcDateOfInitialCustody;
    entities.CaseRole.FcInHomeServiceInd = fcInHomeServiceInd;
    entities.CaseRole.FcIvECaseNumber = fcIvECaseNumber;
    entities.CaseRole.FcJuvenileCourtOrder = fcJuvenileCourtOrder;
    entities.CaseRole.FcJuvenileOffenderInd = fcJuvenileOffenderInd;
    entities.CaseRole.FcLevelOfCare = fcLevelOfCare;
    entities.CaseRole.FcNextJuvenileCtDt = fcNextJuvenileCtDt;
    entities.CaseRole.FcOrderEstBy = fcOrderEstBy;
    entities.CaseRole.FcOtherBenefitInd = fcOtherBenefitInd;
    entities.CaseRole.FcParentalRights = fcParentalRights;
    entities.CaseRole.FcPrevPayeeFirstName = fcPrevPayeeFirstName;
    entities.CaseRole.FcPrevPayeeMiddleInitial = fcPrevPayeeMiddleInitial;
    entities.CaseRole.FcPlacementDate = fcPlacementDate;
    entities.CaseRole.FcPlacementName = fcPlacementName;
    entities.CaseRole.FcPlacementReason = fcPlacementReason;
    entities.CaseRole.FcPreviousPa = fcPreviousPa;
    entities.CaseRole.FcPreviousPayeeLastName = fcPreviousPayeeLastName;
    entities.CaseRole.FcSourceOfFunding = fcSourceOfFunding;
    entities.CaseRole.FcSrsPayee = fcSrsPayee;
    entities.CaseRole.FcSsa = fcSsa;
    entities.CaseRole.FcSsi = fcSsi;
    entities.CaseRole.FcVaInd = fcVaInd;
    entities.CaseRole.FcWardsAccount = fcWardsAccount;
    entities.CaseRole.FcZebInd = fcZebInd;
    entities.CaseRole.Over18AndInSchool = over18AndInSchool;
    entities.CaseRole.ResidesWithArIndicator = residesWithArIndicator;
    entities.CaseRole.SpecialtyArea = specialtyArea;
    entities.CaseRole.LastUpdatedTimestamp = null;
    entities.CaseRole.LastUpdatedBy = "";
    entities.CaseRole.CreatedTimestamp = createdTimestamp;
    entities.CaseRole.CreatedBy = createdBy;
    entities.CaseRole.ConfirmedType = confirmedType;
    entities.CaseRole.RelToAr = relToAr;
    entities.CaseRole.ArChgProcReqInd = arChgProcReqInd;
    entities.CaseRole.ArChgProcessedDate = arChgProcessedDate;
    entities.CaseRole.ArInvalidInd = arInvalidInd;
    entities.CaseRole.Note = note;
    entities.CaseRole.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.Previous.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Previous.CasNumber = db.GetString(reader, 0);
        entities.Previous.CspNumber = db.GetString(reader, 1);
        entities.Previous.Type1 = db.GetString(reader, 2);
        entities.Previous.Identifier = db.GetInt32(reader, 3);
        entities.Previous.StartDate = db.GetNullableDate(reader, 4);
        entities.Previous.EndDate = db.GetNullableDate(reader, 5);
        entities.Previous.OnSsInd = db.GetNullableString(reader, 6);
        entities.Previous.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.Previous.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.Previous.MothersFirstName = db.GetNullableString(reader, 9);
        entities.Previous.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.Previous.FathersLastName = db.GetNullableString(reader, 11);
        entities.Previous.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.Previous.FathersFirstName = db.GetNullableString(reader, 13);
        entities.Previous.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.Previous.ParentType = db.GetNullableString(reader, 15);
        entities.Previous.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.Previous.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.Previous.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.Previous.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.Previous.ContactFirstName = db.GetNullableString(reader, 20);
        entities.Previous.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.Previous.ContactPhone = db.GetNullableString(reader, 22);
        entities.Previous.ContactLastName = db.GetNullableString(reader, 23);
        entities.Previous.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.Previous.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.Previous.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.Previous.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.Previous.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.Previous.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.Previous.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 30);
        entities.Previous.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.Previous.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.Previous.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 33);
        entities.Previous.FcApNotified = db.GetNullableString(reader, 34);
        entities.Previous.FcCincInd = db.GetNullableString(reader, 35);
        entities.Previous.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.Previous.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.Previous.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.Previous.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 39);
        entities.Previous.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.Previous.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.Previous.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.Previous.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.Previous.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.Previous.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.Previous.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.Previous.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.Previous.FcParentalRights = db.GetNullableString(reader, 48);
        entities.Previous.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 49);
        entities.Previous.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.Previous.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.Previous.FcPlacementName = db.GetNullableString(reader, 52);
        entities.Previous.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.Previous.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.Previous.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 55);
        entities.Previous.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.Previous.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.Previous.FcSsa = db.GetNullableString(reader, 58);
        entities.Previous.FcSsi = db.GetNullableString(reader, 59);
        entities.Previous.FcVaInd = db.GetNullableString(reader, 60);
        entities.Previous.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.Previous.FcZebInd = db.GetNullableString(reader, 62);
        entities.Previous.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.Previous.ResidesWithArIndicator =
          db.GetNullableString(reader, 64);
        entities.Previous.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.Previous.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.Previous.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.Previous.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.Previous.CreatedBy = db.GetString(reader, 69);
        entities.Previous.ConfirmedType = db.GetNullableString(reader, 70);
        entities.Previous.RelToAr = db.GetNullableString(reader, 71);
        entities.Previous.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.Previous.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.Previous.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.Previous.Note = db.GetNullableString(reader, 75);
        entities.Previous.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Previous.Type1);
        CheckValid<CaseRole>("ParentType", entities.Previous.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.Previous.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.Previous.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.Previous.SpecialtyArea);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Previous.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Previous.CasNumber = db.GetString(reader, 0);
        entities.Previous.CspNumber = db.GetString(reader, 1);
        entities.Previous.Type1 = db.GetString(reader, 2);
        entities.Previous.Identifier = db.GetInt32(reader, 3);
        entities.Previous.StartDate = db.GetNullableDate(reader, 4);
        entities.Previous.EndDate = db.GetNullableDate(reader, 5);
        entities.Previous.OnSsInd = db.GetNullableString(reader, 6);
        entities.Previous.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.Previous.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.Previous.MothersFirstName = db.GetNullableString(reader, 9);
        entities.Previous.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.Previous.FathersLastName = db.GetNullableString(reader, 11);
        entities.Previous.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.Previous.FathersFirstName = db.GetNullableString(reader, 13);
        entities.Previous.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.Previous.ParentType = db.GetNullableString(reader, 15);
        entities.Previous.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.Previous.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.Previous.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.Previous.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.Previous.ContactFirstName = db.GetNullableString(reader, 20);
        entities.Previous.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.Previous.ContactPhone = db.GetNullableString(reader, 22);
        entities.Previous.ContactLastName = db.GetNullableString(reader, 23);
        entities.Previous.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.Previous.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.Previous.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.Previous.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.Previous.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.Previous.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.Previous.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 30);
        entities.Previous.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.Previous.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.Previous.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 33);
        entities.Previous.FcApNotified = db.GetNullableString(reader, 34);
        entities.Previous.FcCincInd = db.GetNullableString(reader, 35);
        entities.Previous.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.Previous.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.Previous.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.Previous.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 39);
        entities.Previous.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.Previous.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.Previous.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.Previous.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.Previous.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.Previous.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.Previous.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.Previous.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.Previous.FcParentalRights = db.GetNullableString(reader, 48);
        entities.Previous.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 49);
        entities.Previous.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.Previous.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.Previous.FcPlacementName = db.GetNullableString(reader, 52);
        entities.Previous.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.Previous.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.Previous.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 55);
        entities.Previous.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.Previous.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.Previous.FcSsa = db.GetNullableString(reader, 58);
        entities.Previous.FcSsi = db.GetNullableString(reader, 59);
        entities.Previous.FcVaInd = db.GetNullableString(reader, 60);
        entities.Previous.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.Previous.FcZebInd = db.GetNullableString(reader, 62);
        entities.Previous.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.Previous.ResidesWithArIndicator =
          db.GetNullableString(reader, 64);
        entities.Previous.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.Previous.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.Previous.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.Previous.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.Previous.CreatedBy = db.GetString(reader, 69);
        entities.Previous.ConfirmedType = db.GetNullableString(reader, 70);
        entities.Previous.RelToAr = db.GetNullableString(reader, 71);
        entities.Previous.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.Previous.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.Previous.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.Previous.Note = db.GetNullableString(reader, 75);
        entities.Previous.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Previous.Type1);
        CheckValid<CaseRole>("ParentType", entities.Previous.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.Previous.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.Previous.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.Previous.SpecialtyArea);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.Related.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Related.CasNumber = db.GetString(reader, 0);
        entities.Related.CspNumber = db.GetString(reader, 1);
        entities.Related.Type1 = db.GetString(reader, 2);
        entities.Related.Identifier = db.GetInt32(reader, 3);
        entities.Related.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Related.Type1);
      });
  }

  private bool ReadCaseRole4()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.ContactFirstName = db.GetNullableString(reader, 20);
        entities.CaseRole.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.CaseRole.ContactPhone = db.GetNullableString(reader, 22);
        entities.CaseRole.ContactLastName = db.GetNullableString(reader, 23);
        entities.CaseRole.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.CaseRole.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.CaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.CaseRole.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.CaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.CaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 30);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.CaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 33);
        entities.CaseRole.FcApNotified = db.GetNullableString(reader, 34);
        entities.CaseRole.FcCincInd = db.GetNullableString(reader, 35);
        entities.CaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.CaseRole.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.CaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.CaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 39);
        entities.CaseRole.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.CaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.CaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.CaseRole.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.CaseRole.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.CaseRole.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.CaseRole.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.CaseRole.FcParentalRights = db.GetNullableString(reader, 48);
        entities.CaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 49);
        entities.CaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.CaseRole.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.CaseRole.FcPlacementName = db.GetNullableString(reader, 52);
        entities.CaseRole.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.CaseRole.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.CaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 55);
        entities.CaseRole.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.CaseRole.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.CaseRole.FcSsa = db.GetNullableString(reader, 58);
        entities.CaseRole.FcSsi = db.GetNullableString(reader, 59);
        entities.CaseRole.FcVaInd = db.GetNullableString(reader, 60);
        entities.CaseRole.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.CaseRole.FcZebInd = db.GetNullableString(reader, 62);
        entities.CaseRole.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 64);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.CaseRole.CreatedBy = db.GetString(reader, 69);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 70);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 71);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.CaseRole.Note = db.GetNullableString(reader, 75);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private bool ReadCaseRole5()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.ContactFirstName = db.GetNullableString(reader, 20);
        entities.CaseRole.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.CaseRole.ContactPhone = db.GetNullableString(reader, 22);
        entities.CaseRole.ContactLastName = db.GetNullableString(reader, 23);
        entities.CaseRole.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.CaseRole.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.CaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.CaseRole.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.CaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.CaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 30);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.CaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 33);
        entities.CaseRole.FcApNotified = db.GetNullableString(reader, 34);
        entities.CaseRole.FcCincInd = db.GetNullableString(reader, 35);
        entities.CaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.CaseRole.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.CaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.CaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 39);
        entities.CaseRole.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.CaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.CaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.CaseRole.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.CaseRole.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.CaseRole.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.CaseRole.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.CaseRole.FcParentalRights = db.GetNullableString(reader, 48);
        entities.CaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 49);
        entities.CaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.CaseRole.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.CaseRole.FcPlacementName = db.GetNullableString(reader, 52);
        entities.CaseRole.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.CaseRole.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.CaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 55);
        entities.CaseRole.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.CaseRole.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.CaseRole.FcSsa = db.GetNullableString(reader, 58);
        entities.CaseRole.FcSsi = db.GetNullableString(reader, 59);
        entities.CaseRole.FcVaInd = db.GetNullableString(reader, 60);
        entities.CaseRole.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.CaseRole.FcZebInd = db.GetNullableString(reader, 62);
        entities.CaseRole.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 64);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.CaseRole.CreatedBy = db.GetString(reader, 69);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 70);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 71);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.CaseRole.Note = db.GetNullableString(reader, 75);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private bool ReadCaseRole6()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.ContactFirstName = db.GetNullableString(reader, 20);
        entities.CaseRole.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.CaseRole.ContactPhone = db.GetNullableString(reader, 22);
        entities.CaseRole.ContactLastName = db.GetNullableString(reader, 23);
        entities.CaseRole.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.CaseRole.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.CaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.CaseRole.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.CaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.CaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 30);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.CaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 33);
        entities.CaseRole.FcApNotified = db.GetNullableString(reader, 34);
        entities.CaseRole.FcCincInd = db.GetNullableString(reader, 35);
        entities.CaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.CaseRole.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.CaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.CaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 39);
        entities.CaseRole.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.CaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.CaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.CaseRole.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.CaseRole.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.CaseRole.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.CaseRole.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.CaseRole.FcParentalRights = db.GetNullableString(reader, 48);
        entities.CaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 49);
        entities.CaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.CaseRole.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.CaseRole.FcPlacementName = db.GetNullableString(reader, 52);
        entities.CaseRole.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.CaseRole.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.CaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 55);
        entities.CaseRole.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.CaseRole.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.CaseRole.FcSsa = db.GetNullableString(reader, 58);
        entities.CaseRole.FcSsi = db.GetNullableString(reader, 59);
        entities.CaseRole.FcVaInd = db.GetNullableString(reader, 60);
        entities.CaseRole.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.CaseRole.FcZebInd = db.GetNullableString(reader, 62);
        entities.CaseRole.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 64);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.CaseRole.CreatedBy = db.GetString(reader, 69);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 70);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 71);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.CaseRole.Note = db.GetNullableString(reader, 75);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private bool ReadCaseRole7()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole7",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.MothersFirstName = db.GetNullableString(reader, 9);
        entities.CaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.CaseRole.FathersLastName = db.GetNullableString(reader, 11);
        entities.CaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.CaseRole.FathersFirstName = db.GetNullableString(reader, 13);
        entities.CaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.CaseRole.ParentType = db.GetNullableString(reader, 15);
        entities.CaseRole.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.CaseRole.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.CaseRole.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.CaseRole.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.CaseRole.ContactFirstName = db.GetNullableString(reader, 20);
        entities.CaseRole.ContactMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.CaseRole.ContactPhone = db.GetNullableString(reader, 22);
        entities.CaseRole.ContactLastName = db.GetNullableString(reader, 23);
        entities.CaseRole.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.CaseRole.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.CaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.CaseRole.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.CaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 28);
        entities.CaseRole.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.CaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 30);
        entities.CaseRole.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.CaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 33);
        entities.CaseRole.FcApNotified = db.GetNullableString(reader, 34);
        entities.CaseRole.FcCincInd = db.GetNullableString(reader, 35);
        entities.CaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.CaseRole.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.CaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.CaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 39);
        entities.CaseRole.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.CaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 42);
        entities.CaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 43);
        entities.CaseRole.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.CaseRole.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.CaseRole.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.CaseRole.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.CaseRole.FcParentalRights = db.GetNullableString(reader, 48);
        entities.CaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 49);
        entities.CaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.CaseRole.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.CaseRole.FcPlacementName = db.GetNullableString(reader, 52);
        entities.CaseRole.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.CaseRole.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.CaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 55);
        entities.CaseRole.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.CaseRole.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.CaseRole.FcSsa = db.GetNullableString(reader, 58);
        entities.CaseRole.FcSsi = db.GetNullableString(reader, 59);
        entities.CaseRole.FcVaInd = db.GetNullableString(reader, 60);
        entities.CaseRole.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.CaseRole.FcZebInd = db.GetNullableString(reader, 62);
        entities.CaseRole.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.CaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 64);
        entities.CaseRole.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 66);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.CaseRole.CreatedBy = db.GetString(reader, 69);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 70);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 71);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.CaseRole.Note = db.GetNullableString(reader, 75);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CaseRole>("ParentType", entities.CaseRole.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.CaseRole.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.CaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.CaseRole.SpecialtyArea);
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 10);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 11);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAr", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 10);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 11);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
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
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Common fromRole;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 case1;
    private CaseRole caseRole;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A FunctionGroup group.</summary>
    [Serializable]
    public class FunctionGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      public CaseFuncWorkSet Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CaseFuncWorkSet det;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CaseRole Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
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
    /// A value of FaIsAp.
    /// </summary>
    [JsonPropertyName("faIsAp")]
    public Common FaIsAp
    {
      get => faIsAp ??= new();
      set => faIsAp = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public Common CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of MoIsAp.
    /// </summary>
    [JsonPropertyName("moIsAp")]
    public Common MoIsAp
    {
      get => moIsAp ??= new();
      set => moIsAp = value;
    }

    /// <summary>
    /// A value of WorkCu.
    /// </summary>
    [JsonPropertyName("workCu")]
    public WorkArea WorkCu
    {
      get => workCu ??= new();
      set => workCu = value;
    }

    /// <summary>
    /// A value of PrevCaseUnitFound.
    /// </summary>
    [JsonPropertyName("prevCaseUnitFound")]
    public Common PrevCaseUnitFound
    {
      get => prevCaseUnitFound ??= new();
      set => prevCaseUnitFound = value;
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
    /// A value of HoldCaseUnit.
    /// </summary>
    [JsonPropertyName("holdCaseUnit")]
    public CaseUnit HoldCaseUnit
    {
      get => holdCaseUnit ??= new();
      set => holdCaseUnit = value;
    }

    /// <summary>
    /// A value of LastCaseUnit.
    /// </summary>
    [JsonPropertyName("lastCaseUnit")]
    public CaseUnit LastCaseUnit
    {
      get => lastCaseUnit ??= new();
      set => lastCaseUnit = value;
    }

    /// <summary>
    /// A value of ArBecomesAp.
    /// </summary>
    [JsonPropertyName("arBecomesAp")]
    public Common ArBecomesAp
    {
      get => arBecomesAp ??= new();
      set => arBecomesAp = value;
    }

    /// <summary>
    /// Gets a value of Function.
    /// </summary>
    [JsonIgnore]
    public Array<FunctionGroup> Function => function ??= new(
      FunctionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Function for json serialization.
    /// </summary>
    [JsonPropertyName("function")]
    [Computed]
    public IList<FunctionGroup> Function_Json
    {
      get => function;
      set => Function.Assign(value);
    }

    /// <summary>
    /// A value of CaseAssignFound.
    /// </summary>
    [JsonPropertyName("caseAssignFound")]
    public Common CaseAssignFound
    {
      get => caseAssignFound ??= new();
      set => caseAssignFound = value;
    }

    /// <summary>
    /// A value of HoldDateWorkArea.
    /// </summary>
    [JsonPropertyName("holdDateWorkArea")]
    public DateWorkArea HoldDateWorkArea
    {
      get => holdDateWorkArea ??= new();
      set => holdDateWorkArea = value;
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
    /// A value of CurrentCsePerson.
    /// </summary>
    [JsonPropertyName("currentCsePerson")]
    public CsePerson CurrentCsePerson
    {
      get => currentCsePerson ??= new();
      set => currentCsePerson = value;
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
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of LastCase.
    /// </summary>
    [JsonPropertyName("lastCase")]
    public CaseRole LastCase
    {
      get => lastCase ??= new();
      set => lastCase = value;
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
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private CaseRole previous;
    private DateWorkArea zero;
    private Common caseFound;
    private CaseRole caseRole;
    private Common faIsAp;
    private Common caseUnit;
    private Common moIsAp;
    private WorkArea workCu;
    private Common prevCaseUnitFound;
    private Common fromRole;
    private CaseUnit holdCaseUnit;
    private CaseUnit lastCaseUnit;
    private Common arBecomesAp;
    private Array<FunctionGroup> function;
    private Common caseAssignFound;
    private DateWorkArea holdDateWorkArea;
    private DateWorkArea max;
    private CsePerson currentCsePerson;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private DateWorkArea currentDateWorkArea;
    private CaseRole lastCase;
    private Common count;
    private SystemGenerated systemGenerated;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public Case1 Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CaseRole Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Related.
    /// </summary>
    [JsonPropertyName("related")]
    public CaseRole Related
    {
      get => related ??= new();
      set => related = value;
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

    private Case1 other;
    private CaseRole previous;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private CaseRole related;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
