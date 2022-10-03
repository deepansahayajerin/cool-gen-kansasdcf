// Program: SI_RMAD_UPDATE_CASE_ROLE, ID: 373476815, model: 746.
// Short name: SWE02087
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
/// A program: SI_RMAD_UPDATE_CASE_ROLE.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This PAD adds additional information about a CSE Person's role in a case
/// </para>
/// </summary>
[Serializable]
public partial class SiRmadUpdateCaseRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_UPDATE_CASE_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadUpdateCaseRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadUpdateCaseRole.
  /// </summary>
  public SiRmadUpdateCaseRole(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 02/23/95  Helen Sharland                   Initial Development
    // 11/6/98    C Deghand                       Added READ EACH's to the
    //                                            
    // AP and the Child in
    //                                            
    // order to update these
    //                                            
    // details for any case the
    //                                            
    // child  or AP is on.
    //                                            
    // These pertain to the
    //                                            
    // child or the AP and not
    //                                            
    // the case.
    // -----------------------------------------------------------------------------------------------
    // 06/21/99 W.Campbell         Modified the properties
    //                             of 3 READ statements to
    //                             Select Only.
    // -----------------------------------------------------------------------------------------------
    // 03/03/00  C. Ott            Modified for PRWORA Paternity redesign.
    //                             Removed Paternity attributes from Case
    //                             Role.
    // -----------------------------------------------------------------------------------------------
    // 04/02/01  M.Lachowicz       Updates Case Roles on every case
    //                             where updated person plays case role from
    //                             import view.
    // -----------------------------------------------------------------------------------------------
    // 01/24/03  GVandy            Do not update the case_role note on every 
    // case role to which the
    // 			    AR is assigned.  For State of Kansas this is 36,000+ case roles.
    // -----------------------------------------------------------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(import.CaseRole.EndDate, null))
    {
      local.CaseRole.EndDate = local.Max.Date;
    }
    else
    {
      local.CaseRole.EndDate = import.CaseRole.EndDate;
    }

    // ---------------------------------------------
    // This PAD updates a CASE ROLE for a CSE
    // Person on a given CASE.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCaseRole())
    {
      // ---------------------------------------------------------
      // 06/21/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (!ReadCase())
      {
        ExitState = "CASE_NF";

        return;
      }

      // ---------------------------------------------------------
      // 06/21/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCsePerson())
      {
        ExitState = "CASE_ROLE_NF";

        return;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    // --------------------------------------------
    // Depending upon the subtype, update the case
    // role details.
    // --------------------------------------------
    switch(TrimEnd(entities.CaseRole.Type1))
    {
      case "AP":
        try
        {
          UpdateCaseRole5();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        foreach(var item in ReadCsePersonCaseRole1())
        {
          try
          {
            UpdateCaseRole6();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_ROLE_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_ROLE_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        break;
      case "AR":
        // 12/09/2002 M. Lachowicz Start
        local.CaseRole.ArChgProcReqInd = "Y";
        local.CaseRole.ArChgProcessedDate = local.Zero.Date;
        local.CaseRole.ArInvalidInd = "Y";

        // 12/09/2002 M. Lachowicz End
        try
        {
          UpdateCaseRole4();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "CH":
        try
        {
          UpdateCaseRole1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        foreach(var item in ReadCsePersonCaseRole2())
        {
          // 04/03/01 M.L Start
          // Added Absence Code
          if (Equal(entities.ChildCaseRole.EndDate, local.Max.Date))
          {
            MoveCaseRole(import.CaseRole, local.Child);
          }
          else
          {
            MoveCaseRole(entities.ChildCaseRole, local.Child);
          }

          // 04/03/01 M.L End
          // 04/03/01 M.L Start
          // Added Absence Code, Rel_to_AR and AR_Waived_Insurance.
          try
          {
            UpdateCaseRole7();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_ROLE_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_ROLE_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        break;
      case "FA":
        try
        {
          UpdateCaseRole3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "MO":
        try
        {
          UpdateCaseRole2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ROLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ROLE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      default:
        break;
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.AbsenceReasonCode = source.AbsenceReasonCode;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.RelToAr = source.RelToAr;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "caseRoleId", import.CaseRole.Identifier);
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
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 68);
        entities.CaseRole.RelToAr = db.GetNullableString(reader, 69);
        entities.CaseRole.ArChgProcReqInd = db.GetNullableString(reader, 70);
        entities.CaseRole.ArChgProcessedDate = db.GetNullableDate(reader, 71);
        entities.CaseRole.ArInvalidInd = db.GetNullableString(reader, 72);
        entities.CaseRole.Note = db.GetNullableString(reader, 73);
        entities.CaseRole.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole1()
  {
    entities.ApCsePerson.Populated = false;
    entities.ApCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.MothersFirstName = db.GetNullableString(reader, 4);
        entities.ApCaseRole.MothersMiddleInitial =
          db.GetNullableString(reader, 5);
        entities.ApCaseRole.FathersLastName = db.GetNullableString(reader, 6);
        entities.ApCaseRole.FathersMiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ApCaseRole.FathersFirstName = db.GetNullableString(reader, 8);
        entities.ApCaseRole.MothersMaidenLastName =
          db.GetNullableString(reader, 9);
        entities.ApCaseRole.Note = db.GetNullableString(reader, 10);
        entities.ApCsePerson.Populated = true;
        entities.ApCaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole2()
  {
    entities.ChildCsePerson.Populated = false;
    entities.ChildCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.ChildCsePerson.Number = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 1);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ChildCaseRole.AbsenceReasonCode =
          db.GetNullableString(reader, 5);
        entities.ChildCaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 6);
        entities.ChildCaseRole.ArWaivedInsurance =
          db.GetNullableString(reader, 7);
        entities.ChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 8);
        entities.ChildCaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 9);
        entities.ChildCaseRole.FcApNotified = db.GetNullableString(reader, 10);
        entities.ChildCaseRole.FcCincInd = db.GetNullableString(reader, 11);
        entities.ChildCaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 12);
        entities.ChildCaseRole.FcCostOfCareFreq =
          db.GetNullableString(reader, 13);
        entities.ChildCaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 14);
        entities.ChildCaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 15);
        entities.ChildCaseRole.FcInHomeServiceInd =
          db.GetNullableString(reader, 16);
        entities.ChildCaseRole.FcIvECaseNumber =
          db.GetNullableString(reader, 17);
        entities.ChildCaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 18);
        entities.ChildCaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 19);
        entities.ChildCaseRole.FcLevelOfCare = db.GetNullableString(reader, 20);
        entities.ChildCaseRole.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 21);
        entities.ChildCaseRole.FcOrderEstBy = db.GetNullableString(reader, 22);
        entities.ChildCaseRole.FcOtherBenefitInd =
          db.GetNullableString(reader, 23);
        entities.ChildCaseRole.FcParentalRights =
          db.GetNullableString(reader, 24);
        entities.ChildCaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 25);
        entities.ChildCaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 26);
        entities.ChildCaseRole.FcPlacementDate = db.GetNullableDate(reader, 27);
        entities.ChildCaseRole.FcPlacementName =
          db.GetNullableString(reader, 28);
        entities.ChildCaseRole.FcPlacementReason =
          db.GetNullableString(reader, 29);
        entities.ChildCaseRole.FcPreviousPa = db.GetNullableString(reader, 30);
        entities.ChildCaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 31);
        entities.ChildCaseRole.FcSourceOfFunding =
          db.GetNullableString(reader, 32);
        entities.ChildCaseRole.FcSrsPayee = db.GetNullableString(reader, 33);
        entities.ChildCaseRole.FcSsa = db.GetNullableString(reader, 34);
        entities.ChildCaseRole.FcSsi = db.GetNullableString(reader, 35);
        entities.ChildCaseRole.FcVaInd = db.GetNullableString(reader, 36);
        entities.ChildCaseRole.FcWardsAccount =
          db.GetNullableString(reader, 37);
        entities.ChildCaseRole.FcZebInd = db.GetNullableString(reader, 38);
        entities.ChildCaseRole.Over18AndInSchool =
          db.GetNullableString(reader, 39);
        entities.ChildCaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 40);
        entities.ChildCaseRole.SpecialtyArea = db.GetNullableString(reader, 41);
        entities.ChildCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 42);
        entities.ChildCaseRole.LastUpdatedBy = db.GetNullableString(reader, 43);
        entities.ChildCaseRole.RelToAr = db.GetNullableString(reader, 44);
        entities.ChildCaseRole.Note = db.GetNullableString(reader, 45);
        entities.ChildCsePerson.Populated = true;
        entities.ChildCaseRole.Populated = true;

        return true;
      });
  }

  private void UpdateCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var startDate = import.CaseRole.StartDate;
    var endDate = local.CaseRole.EndDate;
    var onSsInd = import.CaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator = import.CaseRole.HealthInsuranceIndicator ?? ""
      ;
    var medicalSupportIndicator = import.CaseRole.MedicalSupportIndicator ?? "";
    var absenceReasonCode = import.CaseRole.AbsenceReasonCode ?? "";
    var arWaivedInsurance = import.CaseRole.ArWaivedInsurance ?? "";
    var fcApNotified = import.CaseRole.FcApNotified ?? "";
    var specialtyArea = import.CaseRole.SpecialtyArea ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var relToAr = import.CaseRole.RelToAr ?? "";
    var note = import.CaseRole.Note ?? "";

    CheckValid<CaseRole>("SpecialtyArea", specialtyArea);
    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.SetNullableString(command, "absenceReasonCd", absenceReasonCode);
        db.SetNullableString(command, "arWaivedIns", arWaivedInsurance);
        db.SetNullableString(command, "fcApNotified", fcApNotified);
        db.SetNullableString(command, "specialtyArea", specialtyArea);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.StartDate = startDate;
    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.OnSsInd = onSsInd;
    entities.CaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.CaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.CaseRole.AbsenceReasonCode = absenceReasonCode;
    entities.CaseRole.ArWaivedInsurance = arWaivedInsurance;
    entities.CaseRole.FcApNotified = fcApNotified;
    entities.CaseRole.SpecialtyArea = specialtyArea;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.RelToAr = relToAr;
    entities.CaseRole.Note = note;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var startDate = import.CaseRole.StartDate;
    var endDate = local.CaseRole.EndDate;
    var onSsInd = import.CaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator = import.CaseRole.HealthInsuranceIndicator ?? ""
      ;
    var medicalSupportIndicator = import.CaseRole.MedicalSupportIndicator ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var note = import.CaseRole.Note ?? "";

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole2",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.StartDate = startDate;
    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.OnSsInd = onSsInd;
    entities.CaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.CaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.Note = note;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCaseRole3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var startDate = import.CaseRole.StartDate;
    var endDate = local.CaseRole.EndDate;
    var onSsInd = import.CaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator = import.CaseRole.HealthInsuranceIndicator ?? ""
      ;
    var medicalSupportIndicator = import.CaseRole.MedicalSupportIndicator ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var confirmedType = import.CaseRole.ConfirmedType ?? "";
    var note = import.CaseRole.Note ?? "";

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole3",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "confirmedType", confirmedType);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.StartDate = startDate;
    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.OnSsInd = onSsInd;
    entities.CaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.CaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.ConfirmedType = confirmedType;
    entities.CaseRole.Note = note;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCaseRole4()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var startDate = import.CaseRole.StartDate;
    var endDate = local.CaseRole.EndDate;
    var onSsInd = import.CaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator = import.CaseRole.HealthInsuranceIndicator ?? ""
      ;
    var medicalSupportIndicator = import.CaseRole.MedicalSupportIndicator ?? "";
    var contactFirstName = import.CaseRole.ContactFirstName ?? "";
    var contactMiddleInitial = import.CaseRole.ContactMiddleInitial ?? "";
    var contactPhone = import.CaseRole.ContactPhone ?? "";
    var contactLastName = import.CaseRole.ContactLastName ?? "";
    var childCareExpenses =
      import.CaseRole.ChildCareExpenses.GetValueOrDefault();
    var assignmentDate = import.CaseRole.AssignmentDate;
    var assignmentTerminationCode =
      import.CaseRole.AssignmentTerminationCode ?? "";
    var assignmentOfRights = import.CaseRole.AssignmentOfRights ?? "";
    var assignmentTerminatedDt = import.CaseRole.AssignmentTerminatedDt;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var arChgProcReqInd = local.CaseRole.ArChgProcReqInd ?? "";
    var arChgProcessedDate = local.CaseRole.ArChgProcessedDate;
    var arInvalidInd = local.CaseRole.ArInvalidInd ?? "";

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole4",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
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
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "arChgPrcReqInd", arChgProcReqInd);
        db.SetNullableDate(command, "arChgProcDt", arChgProcessedDate);
        db.SetNullableString(command, "arInvalidInd", arInvalidInd);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.StartDate = startDate;
    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.OnSsInd = onSsInd;
    entities.CaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.CaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.CaseRole.ContactFirstName = contactFirstName;
    entities.CaseRole.ContactMiddleInitial = contactMiddleInitial;
    entities.CaseRole.ContactPhone = contactPhone;
    entities.CaseRole.ContactLastName = contactLastName;
    entities.CaseRole.ChildCareExpenses = childCareExpenses;
    entities.CaseRole.AssignmentDate = assignmentDate;
    entities.CaseRole.AssignmentTerminationCode = assignmentTerminationCode;
    entities.CaseRole.AssignmentOfRights = assignmentOfRights;
    entities.CaseRole.AssignmentTerminatedDt = assignmentTerminatedDt;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.ArChgProcReqInd = arChgProcReqInd;
    entities.CaseRole.ArChgProcessedDate = arChgProcessedDate;
    entities.CaseRole.ArInvalidInd = arInvalidInd;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCaseRole5()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var startDate = import.CaseRole.StartDate;
    var endDate = local.CaseRole.EndDate;
    var onSsInd = import.CaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator = import.CaseRole.HealthInsuranceIndicator ?? ""
      ;
    var medicalSupportIndicator = import.CaseRole.MedicalSupportIndicator ?? "";
    var parentType = import.CaseRole.ParentType ?? "";
    var notifiedDate = import.CaseRole.NotifiedDate;
    var numberOfChildren = import.CaseRole.NumberOfChildren.GetValueOrDefault();
    var livingWithArIndicator = import.CaseRole.LivingWithArIndicator ?? "";
    var nonpaymentCategory = import.CaseRole.NonpaymentCategory ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var note = import.CaseRole.Note ?? "";

    CheckValid<CaseRole>("ParentType", parentType);
    CheckValid<CaseRole>("LivingWithArIndicator", livingWithArIndicator);
    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole5",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.SetNullableString(command, "parentType", parentType);
        db.SetNullableDate(command, "notifiedDate", notifiedDate);
        db.SetNullableInt32(command, "numberOfChildren", numberOfChildren);
        db.SetNullableString(command, "livingWithArInd", livingWithArIndicator);
        db.SetNullableString(command, "nonpaymentCat", nonpaymentCategory);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.StartDate = startDate;
    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.OnSsInd = onSsInd;
    entities.CaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.CaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.CaseRole.ParentType = parentType;
    entities.CaseRole.NotifiedDate = notifiedDate;
    entities.CaseRole.NumberOfChildren = numberOfChildren;
    entities.CaseRole.LivingWithArIndicator = livingWithArIndicator;
    entities.CaseRole.NonpaymentCategory = nonpaymentCategory;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.Note = note;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCaseRole6()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);

    var mothersFirstName = import.CaseRole.MothersFirstName ?? "";
    var mothersMiddleInitial = import.CaseRole.MothersMiddleInitial ?? "";
    var fathersLastName = import.CaseRole.FathersLastName ?? "";
    var fathersMiddleInitial = import.CaseRole.FathersMiddleInitial ?? "";
    var fathersFirstName = import.CaseRole.FathersFirstName ?? "";
    var mothersMaidenLastName = import.CaseRole.MothersMaidenLastName ?? "";
    var note = import.CaseRole.Note ?? "";

    entities.ApCaseRole.Populated = false;
    Update("UpdateCaseRole6",
      (db, command) =>
      {
        db.SetNullableString(command, "mothersFirstNm", mothersFirstName);
        db.SetNullableString(command, "mothersMidInit", mothersMiddleInitial);
        db.SetNullableString(command, "fathersLastName", fathersLastName);
        db.SetNullableString(command, "fathersMidInit", fathersMiddleInitial);
        db.SetNullableString(command, "fathersFirstName", fathersFirstName);
        db.
          SetNullableString(command, "motherMaidenLast", mothersMaidenLastName);
          
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.ApCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ApCaseRole.CspNumber);
        db.SetString(command, "type", entities.ApCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.ApCaseRole.Identifier);
      });

    entities.ApCaseRole.MothersFirstName = mothersFirstName;
    entities.ApCaseRole.MothersMiddleInitial = mothersMiddleInitial;
    entities.ApCaseRole.FathersLastName = fathersLastName;
    entities.ApCaseRole.FathersMiddleInitial = fathersMiddleInitial;
    entities.ApCaseRole.FathersFirstName = fathersFirstName;
    entities.ApCaseRole.MothersMaidenLastName = mothersMaidenLastName;
    entities.ApCaseRole.Note = note;
    entities.ApCaseRole.Populated = true;
  }

  private void UpdateCaseRole7()
  {
    System.Diagnostics.Debug.Assert(entities.ChildCaseRole.Populated);

    var absenceReasonCode = local.Child.AbsenceReasonCode ?? "";
    var priorMedicalSupport =
      import.CaseRole.PriorMedicalSupport.GetValueOrDefault();
    var arWaivedInsurance = local.Child.ArWaivedInsurance ?? "";
    var dateOfEmancipation = import.CaseRole.DateOfEmancipation;
    var fcAdoptionDisruptionInd = import.CaseRole.FcAdoptionDisruptionInd ?? "";
    var fcCincInd = import.CaseRole.FcCincInd ?? "";
    var fcCostOfCare = import.CaseRole.FcCostOfCare.GetValueOrDefault();
    var fcCostOfCareFreq = import.CaseRole.FcCostOfCareFreq ?? "";
    var fcCountyChildRemovedFrom = import.CaseRole.FcCountyChildRemovedFrom ?? ""
      ;
    var fcDateOfInitialCustody = import.CaseRole.FcDateOfInitialCustody;
    var fcInHomeServiceInd = import.CaseRole.FcInHomeServiceInd ?? "";
    var fcIvECaseNumber = import.CaseRole.FcIvECaseNumber ?? "";
    var fcJuvenileCourtOrder = import.CaseRole.FcJuvenileCourtOrder ?? "";
    var fcJuvenileOffenderInd = import.CaseRole.FcJuvenileOffenderInd ?? "";
    var fcLevelOfCare = import.CaseRole.FcLevelOfCare ?? "";
    var fcNextJuvenileCtDt = import.CaseRole.FcNextJuvenileCtDt;
    var fcOrderEstBy = import.CaseRole.FcOrderEstBy ?? "";
    var fcOtherBenefitInd = import.CaseRole.FcOtherBenefitInd ?? "";
    var fcParentalRights = import.CaseRole.FcParentalRights ?? "";
    var fcPrevPayeeFirstName = import.CaseRole.FcPrevPayeeFirstName ?? "";
    var fcPrevPayeeMiddleInitial = import.CaseRole.FcPrevPayeeMiddleInitial ?? ""
      ;
    var fcPlacementDate = import.CaseRole.FcPlacementDate;
    var fcPlacementName = import.CaseRole.FcPlacementName ?? "";
    var fcPlacementReason = import.CaseRole.FcPlacementReason ?? "";
    var fcPreviousPa = import.CaseRole.FcPreviousPa ?? "";
    var fcPreviousPayeeLastName = import.CaseRole.FcPreviousPayeeLastName ?? "";
    var fcSourceOfFunding = import.CaseRole.FcSourceOfFunding ?? "";
    var fcSrsPayee = import.CaseRole.FcSrsPayee ?? "";
    var fcSsa = import.CaseRole.FcSsa ?? "";
    var fcSsi = import.CaseRole.FcSsi ?? "";
    var fcVaInd = import.CaseRole.FcVaInd ?? "";
    var fcWardsAccount = import.CaseRole.FcWardsAccount ?? "";
    var fcZebInd = import.CaseRole.FcZebInd ?? "";
    var over18AndInSchool = import.CaseRole.Over18AndInSchool ?? "";
    var residesWithArIndicator = import.CaseRole.ResidesWithArIndicator ?? "";
    var relToAr = local.Child.RelToAr ?? "";
    var note = import.CaseRole.Note ?? "";

    CheckValid<CaseRole>("ResidesWithArIndicator", residesWithArIndicator);
    entities.ChildCaseRole.Populated = false;
    Update("UpdateCaseRole7",
      (db, command) =>
      {
        db.SetNullableString(command, "absenceReasonCd", absenceReasonCode);
        db.SetNullableDecimal(command, "priorMedicalSupp", priorMedicalSupport);
        db.SetNullableString(command, "arWaivedIns", arWaivedInsurance);
        db.SetNullableDate(command, "emancipationDt", dateOfEmancipation);
        db.
          SetNullableString(command, "fcAdoptDisrupt", fcAdoptionDisruptionInd);
          
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
          
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.ChildCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ChildCaseRole.CspNumber);
        db.SetString(command, "type", entities.ChildCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.ChildCaseRole.Identifier);
      });

    entities.ChildCaseRole.AbsenceReasonCode = absenceReasonCode;
    entities.ChildCaseRole.PriorMedicalSupport = priorMedicalSupport;
    entities.ChildCaseRole.ArWaivedInsurance = arWaivedInsurance;
    entities.ChildCaseRole.DateOfEmancipation = dateOfEmancipation;
    entities.ChildCaseRole.FcAdoptionDisruptionInd = fcAdoptionDisruptionInd;
    entities.ChildCaseRole.FcCincInd = fcCincInd;
    entities.ChildCaseRole.FcCostOfCare = fcCostOfCare;
    entities.ChildCaseRole.FcCostOfCareFreq = fcCostOfCareFreq;
    entities.ChildCaseRole.FcCountyChildRemovedFrom = fcCountyChildRemovedFrom;
    entities.ChildCaseRole.FcDateOfInitialCustody = fcDateOfInitialCustody;
    entities.ChildCaseRole.FcInHomeServiceInd = fcInHomeServiceInd;
    entities.ChildCaseRole.FcIvECaseNumber = fcIvECaseNumber;
    entities.ChildCaseRole.FcJuvenileCourtOrder = fcJuvenileCourtOrder;
    entities.ChildCaseRole.FcJuvenileOffenderInd = fcJuvenileOffenderInd;
    entities.ChildCaseRole.FcLevelOfCare = fcLevelOfCare;
    entities.ChildCaseRole.FcNextJuvenileCtDt = fcNextJuvenileCtDt;
    entities.ChildCaseRole.FcOrderEstBy = fcOrderEstBy;
    entities.ChildCaseRole.FcOtherBenefitInd = fcOtherBenefitInd;
    entities.ChildCaseRole.FcParentalRights = fcParentalRights;
    entities.ChildCaseRole.FcPrevPayeeFirstName = fcPrevPayeeFirstName;
    entities.ChildCaseRole.FcPrevPayeeMiddleInitial = fcPrevPayeeMiddleInitial;
    entities.ChildCaseRole.FcPlacementDate = fcPlacementDate;
    entities.ChildCaseRole.FcPlacementName = fcPlacementName;
    entities.ChildCaseRole.FcPlacementReason = fcPlacementReason;
    entities.ChildCaseRole.FcPreviousPa = fcPreviousPa;
    entities.ChildCaseRole.FcPreviousPayeeLastName = fcPreviousPayeeLastName;
    entities.ChildCaseRole.FcSourceOfFunding = fcSourceOfFunding;
    entities.ChildCaseRole.FcSrsPayee = fcSrsPayee;
    entities.ChildCaseRole.FcSsa = fcSsa;
    entities.ChildCaseRole.FcSsi = fcSsi;
    entities.ChildCaseRole.FcVaInd = fcVaInd;
    entities.ChildCaseRole.FcWardsAccount = fcWardsAccount;
    entities.ChildCaseRole.FcZebInd = fcZebInd;
    entities.ChildCaseRole.Over18AndInSchool = over18AndInSchool;
    entities.ChildCaseRole.ResidesWithArIndicator = residesWithArIndicator;
    entities.ChildCaseRole.RelToAr = relToAr;
    entities.ChildCaseRole.Note = note;
    entities.ChildCaseRole.Populated = true;
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

    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private DateWorkArea zero;
    private CaseRole child;
    private DateWorkArea max;
    private CaseRole caseRole;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
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

    private CsePerson apCsePerson;
    private CsePerson childCsePerson;
    private CsePerson arCsePerson;
    private CaseRole apCaseRole;
    private CaseRole childCaseRole;
    private CaseRole arCaseRole;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
