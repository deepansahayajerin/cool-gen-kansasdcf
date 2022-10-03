// Program: SI_SET_CLIENT_CASE_ROLE_DETAILS, ID: 371727794, model: 746.
// Short name: SWE01924
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
/// A program: SI_SET_CLIENT_CASE_ROLE_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiSetClientCaseRoleDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_SET_CLIENT_CASE_ROLE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiSetClientCaseRoleDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiSetClientCaseRoleDetails.
  /// </summary>
  public SiSetClientCaseRoleDetails(IContext context, Import import,
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
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // ??/??/??  ???????????	        Initial Development
    // ------------------------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 03/10/00  M. Lachowicz         PRWORA changes.
    // ------------------------------------------------------------
    // 06/06/00  M. Lachowicz         Added extra validation for Paternity 
    // esatblished,
    //                                
    // CSE to Establish Paternity and
    //                                
    // Born out of wedlock.
    // ------------------------------------------------------------
    // 07/03/00  M. Lachowicz         Raised event when Paternity
    //                                
    // Established Indicator was
    // changed to Y.
    //                                
    // PR 98482
    // ------------------------------------------------------------
    // 07/11/00  M. Lachowicz         Do not set Paternity
    //                                
    // Established Indicator if AP sex
    // is
    //                                
    // equal to 'F' or '  '.
    //                                
    // PR 98482.
    // ------------------------------------------------------------
    // 09/17/15  R.Mathews            Convert Referral FC_Rights_Severed value
    //                                
    // of 'Y' to Case Role
    // FC_Parental_Rights
    //                                
    // value of 'B' (Both).  'Y' isn't
    // a valid
    //                                
    // value for the case role field.
    // -------------------------------------------------------------
    // 05/10/17  GVandy	CQ48108.  Do not set paternity established ind based on 
    // PAR1 info.
    // -------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    switch(TrimEnd(import.CaseRole.Type1))
    {
      case "AR":
        // 06/22/99 M.L
        //              Change property of the following READ EACH to OPTIMIZE 
        // FOR 1
        //              ROW
        if (ReadCaseRole2())
        {
          export.ArCaseRole.Assign(entities.ExistingAr);
          MoveCsePerson(entities.ExistingCsePerson, export.ArCsePerson);

          break;
        }

        return;
      case "AP":
        // 06/22/99 M.L
        //              Change property of the following READ EACH to OPTIMIZE 
        // FOR 1
        //              ROW
        if (ReadCaseRole1())
        {
          export.ApCaseRole.Assign(entities.ExistingAp);
          MoveCsePerson(entities.ExistingCsePerson, export.ApCsePerson);

          break;
        }

        return;
      case "CH":
        // 03/10/00 - M.L start
        local.CsePerson.BornOutOfWedlock =
          entities.ExistingCsePerson.BornOutOfWedlock;
        local.CsePerson.CseToEstblPaternity =
          entities.ExistingCsePerson.CseToEstblPaternity;

        // 03/10/00 - M.L end
        // 06/06/00 - M.L start
        local.CsePerson.PaternityEstablishedIndicator =
          entities.ExistingCsePerson.PaternityEstablishedIndicator;
        local.CsePerson.DatePaternEstab =
          entities.ExistingCsePerson.DatePaternEstab;

        // 06/06/00 - M.L end
        // 07/03/00 - M.L start
        local.Org.PaternityEstablishedIndicator =
          entities.ExistingCsePerson.PaternityEstablishedIndicator;

        // 07/03/00 - M.L end
        // 06/22/99 M.L
        //              Change property of the following READ EACH to OPTIMIZE 
        // FOR 1
        //              ROW
        if (ReadCaseRole3())
        {
          // 03/10/00 - M.L start
          export.ChCaseRole.Assign(entities.ExistingCh);

          // 03/10/00 - M.L end
          export.ChCsePerson.Assign(entities.ExistingCsePerson);

          break;
        }

        return;
      default:
        return;
    }

    if (AsChar(import.FromIapi.Flag) == 'Y')
    {
      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadInterstateCase())
      {
        ExitState = "INTERSTATE_CASE_NF";

        return;
      }
    }

    if (AsChar(import.FromPar1.Flag) == 'Y')
    {
      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadPaReferral())
      {
        // 06/22/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        // 07/09/99 M.L
        //              Change property of the following READ as it was 
        // originally
        //              (SELECT and CURSOR).
        if (!ReadPaReferralParticipant())
        {
          return;
        }

        if (Equal(entities.ExistingPaReferral.From, "KAE"))
        {
          export.ArCsePerson.AeCaseNumber =
            entities.ExistingPaReferral.CaseNumber;
          export.ChCsePerson.AeCaseNumber =
            entities.ExistingPaReferral.CaseNumber;
          export.ApCsePerson.AeCaseNumber =
            entities.ExistingPaReferral.CaseNumber;
        }
        else if (Equal(entities.ExistingPaReferral.From, "KSC"))
        {
          export.ArCsePerson.KscaresNumber =
            entities.ExistingPaReferral.CaseNumber;
          export.ChCsePerson.KscaresNumber =
            entities.ExistingPaReferral.CaseNumber;
          export.ApCsePerson.KscaresNumber =
            entities.ExistingPaReferral.CaseNumber;
        }

        if (Equal(import.CaseRole.Type1, "CH"))
        {
          export.ChCaseRole.AbsenceReasonCode =
            entities.ExistingPaReferralParticipant.AbsenceCode;

          // 03/10/00 - M.L Start
          // --05/10/17 GVandy CQ48108 (IV-D PEP) Do not set paternity 
          // established based on the PAR1 info.
          // 07/11/00 M.L Check if it is male AP.
          if (AsChar(export.ChCsePerson.PaternityEstablishedIndicator) == 'Y'
            && AsChar(import.ApSex.Sex) == 'M')
          {
            // 06/06/00 M.L Start
            if (AsChar(local.CsePerson.PaternityEstablishedIndicator) == 'N')
            {
              if (AsChar(local.CsePerson.CseToEstblPaternity) == 'U')
              {
                local.CsePerson.CseToEstblPaternity = "N";
              }

              local.CsePerson.PaternityEstablishedIndicator = "Y";
              local.CsePerson.DatePaternEstab = local.Current.Date;
              export.ChCsePerson.DatePaternEstab = local.Current.Date;
            }

            // 06/06/00 M.L End
          }
          else
          {
            // 05/20/00 M.L Start - Suggested Change
            export.ChCsePerson.PaternityEstablishedIndicator = "N";

            // 05/20/00 M.L End - Suggested Change
            export.ChCsePerson.DatePaternEstab = local.DateWorkArea.Date;

            // 06/06/00 M.L Start
            // 06/06/00 M.L End
          }

          // 03/10/00 - M.L End
          export.ChCaseRole.RelToAr =
            entities.ExistingPaReferralParticipant.Relationship;
          export.ChCaseRole.FcAdoptionDisruptionInd =
            entities.ExistingPaReferral.FcAdoptionDisruptionInd;
          export.ChCaseRole.FcApNotified =
            entities.ExistingPaReferral.FcApNotified;
          export.ChCaseRole.FcCincInd = entities.ExistingPaReferral.FcCincInd;
          export.ChCaseRole.FcCostOfCare =
            entities.ExistingPaReferral.FcCostOfCare;
          export.ChCaseRole.FcCostOfCareFreq =
            entities.ExistingPaReferral.FcCostOfCareFreq;
          export.ChCaseRole.FcCountyChildRemovedFrom =
            entities.ExistingPaReferral.FcCountyChildRemovedFrom;
          export.ChCaseRole.FcDateOfInitialCustody =
            entities.ExistingPaReferral.FcDateOfInitialCustody;
          export.ChCaseRole.FcIvECaseNumber =
            entities.ExistingPaReferral.FcIvECaseNumber;
          export.ChCaseRole.FcJuvenileCourtOrder =
            entities.ExistingPaReferral.FcJuvenileCourtOrder;
          export.ChCaseRole.FcJuvenileOffenderInd =
            entities.ExistingPaReferral.FcJuvenileOffenderInd;
          export.ChCaseRole.FcNextJuvenileCtDt =
            entities.ExistingPaReferral.FcNextJuvenileCtDt;
          export.ChCaseRole.FcOrderEstBy =
            entities.ExistingPaReferral.FcOrderEstBy;
          export.ChCaseRole.FcOtherBenefitInd =
            entities.ExistingPaReferral.FcOtherBenefitInd;

          // 09/17/15 R.M. Translate FC_RIGHTS_SEVERED value of 'Y' for case 
          // role add
          if (AsChar(entities.ExistingPaReferral.FcRightsSevered) == 'Y')
          {
            export.ChCaseRole.FcParentalRights = "B";
          }
          else
          {
            export.ChCaseRole.FcParentalRights =
              entities.ExistingPaReferral.FcRightsSevered;
          }

          export.ChCaseRole.FcPlacementDate =
            entities.ExistingPaReferral.FcPlacementDate;
          export.ChCaseRole.FcPlacementName =
            entities.ExistingPaReferral.FcPlacementName;
          export.ChCaseRole.FcPlacementReason =
            entities.ExistingPaReferral.FcPlacementType;
          export.ChCaseRole.FcPreviousPa =
            entities.ExistingPaReferral.FcPreviousPa;
          export.ChCaseRole.FcSourceOfFunding =
            entities.ExistingPaReferral.FcSourceOfFunding;
          export.ChCaseRole.FcSrsPayee = entities.ExistingPaReferral.FcSrsPayee;
          export.ChCaseRole.FcSsa = entities.ExistingPaReferral.FcSsa;
          export.ChCaseRole.FcSsi = entities.ExistingPaReferral.FcSsi;
          export.ChCaseRole.FcVaInd = entities.ExistingPaReferral.FcVaInd;
          export.ChCaseRole.FcWardsAccount =
            entities.ExistingPaReferral.FcWardsAccount;
          export.ChCaseRole.FcZebInd = entities.ExistingPaReferral.FcZebInd;
        }

        if (Equal(import.CaseRole.Type1, "AP"))
        {
          export.ApCsePerson.HomePhone =
            entities.ExistingPaReferral.ApPhoneNumber;
          export.ApCsePerson.HomePhoneAreaCode =
            entities.ExistingPaReferral.ApAreaCode;
          export.ApCsePerson.WorkPhoneAreaCode =
            (int?)StringToNumber(NumberToString(
              entities.ExistingPaReferral.ApEmployerPhone.GetValueOrDefault(),
            1, 3));
          export.ApCsePerson.WorkPhone =
            (int?)StringToNumber(NumberToString(
              entities.ExistingPaReferral.ApEmployerPhone.GetValueOrDefault(),
            4, 7));
          local.GoodCause.Code = entities.ExistingPaReferral.GoodCauseCode;
          local.GoodCause.EffectiveDate =
            entities.ExistingPaReferral.GoodCauseDate;
        }

        if (Equal(import.CaseRole.Type1, "AR"))
        {
          export.ArCaseRole.AssignmentOfRights = "Y";
          export.ArCaseRole.AssignmentDate =
            entities.ExistingPaReferral.AssignmentDate;
        }
      }
      else
      {
        ExitState = "PA_REFERRAL_NF";

        return;
      }
    }

    if (AsChar(import.FromInrdCommon.Flag) == 'Y')
    {
      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadInformationRequest())
      {
        ExitState = "INQUIRY_NF";

        return;
      }
    }

    switch(TrimEnd(import.CaseRole.Type1))
    {
      case "AR":
        try
        {
          UpdateCsePerson1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          UpdateCaseRole2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "AP":
        try
        {
          UpdateCsePerson3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          UpdateCaseRole1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // ***************************************************
        // 	Create Good Cause record for AR/AP.
        // ***************************************************
        if (AsChar(import.FromPar1.Flag) == 'Y' && !
          IsEmpty(local.GoodCause.Code))
        {
          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //             SELECT ONLY
          if (ReadCaseRole4())
          {
            try
            {
              CreateGoodCause();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        break;
      case "CH":
        // 03/10/00 M.L  Start
        // Add new attribute.
        //  - paternity_established
        try
        {
          UpdateCsePerson2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // 03/10/00 M.L  Start
        // Delete attributes added  CSE_person.
        //  - born_out_of_wedlock
        //  - paternity_established
        //  - birth_cert_fathers_first_name
        //  - birht_cert_fathers_mi
        //  - birth_cert_fathers_last_name
        //  - birth_certificate_siganture
        try
        {
          UpdateCaseRole4();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // 07/03/2000 M.L Start
        // 07/11/00 M.L Check if it is male AP.
        if (AsChar(local.CsePerson.PaternityEstablishedIndicator) == 'Y' && AsChar
          (local.Org.PaternityEstablishedIndicator) != 'Y' && AsChar
          (import.ApSex.Sex) == 'M')
        {
          local.Infrastructure.EventId = 11;
          local.Infrastructure.ReasonCode = "PATESTAB";
          UseSiChdsRaiseEvent();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_ON_EVENT_CREATION";
          }
        }

        // 07/03/2000 M.L End
        // 04/05/01 M.L Start
        if (AsChar(import.FromPar1.Flag) == 'Y')
        {
          foreach(var item in ReadCaseRole5())
          {
            try
            {
              UpdateCaseRole3();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        // 04/05/01 M.L End
        break;
      default:
        break;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
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
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiChdsRaiseEvent()
  {
    var useImport = new SiChdsRaiseEvent.Import();
    var useExport = new SiChdsRaiseEvent.Export();

    useImport.Ch.Number = entities.ExistingCsePerson.Number;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SiChdsRaiseEvent.Execute, useImport, useExport);
  }

  private void CreateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingAr.Populated);

    var code = local.GoodCause.Code ?? "";
    var effectiveDate = local.GoodCause.EffectiveDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var casNumber = entities.ExistingAr.CasNumber;
    var cspNumber = entities.ExistingAr.CspNumber;
    var croType = entities.ExistingAr.Type1;
    var croIdentifier = entities.ExistingAr.Identifier;
    var casNumber1 = entities.ExistingAp.CasNumber;
    var cspNumber1 = entities.ExistingAp.CspNumber;
    var croType1 = entities.ExistingAp.Type1;
    var croIdentifier1 = entities.ExistingAp.Identifier;

    CheckValid<GoodCause>("CroType", croType);
    CheckValid<GoodCause>("CroType1", croType1);
    entities.GoodCause.Populated = false;
    Update("CreateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
      });

    entities.GoodCause.Code = code;
    entities.GoodCause.EffectiveDate = effectiveDate;
    entities.GoodCause.DiscontinueDate = discontinueDate;
    entities.GoodCause.CreatedBy = createdBy;
    entities.GoodCause.CreatedTimestamp = createdTimestamp;
    entities.GoodCause.LastUpdatedBy = createdBy;
    entities.GoodCause.LastUpdatedTimestamp = createdTimestamp;
    entities.GoodCause.CasNumber = casNumber;
    entities.GoodCause.CspNumber = cspNumber;
    entities.GoodCause.CroType = croType;
    entities.GoodCause.CroIdentifier = croIdentifier;
    entities.GoodCause.CasNumber1 = casNumber1;
    entities.GoodCause.CspNumber1 = cspNumber1;
    entities.GoodCause.CroType1 = croType1;
    entities.GoodCause.CroIdentifier1 = croIdentifier1;
    entities.GoodCause.Populated = true;
  }

  private bool ReadCaseRole1()
  {
    entities.ExistingAp.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAp.CasNumber = db.GetString(reader, 0);
        entities.ExistingAp.CspNumber = db.GetString(reader, 1);
        entities.ExistingAp.Type1 = db.GetString(reader, 2);
        entities.ExistingAp.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAp.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAp.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingAp.OnSsInd = db.GetNullableString(reader, 6);
        entities.ExistingAp.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.ExistingAp.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingAp.MothersFirstName = db.GetNullableString(reader, 9);
        entities.ExistingAp.MothersMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.ExistingAp.FathersLastName = db.GetNullableString(reader, 11);
        entities.ExistingAp.FathersMiddleInitial =
          db.GetNullableString(reader, 12);
        entities.ExistingAp.FathersFirstName = db.GetNullableString(reader, 13);
        entities.ExistingAp.MothersMaidenLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingAp.ParentType = db.GetNullableString(reader, 15);
        entities.ExistingAp.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.ExistingAp.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.ExistingAp.LivingWithArIndicator =
          db.GetNullableString(reader, 18);
        entities.ExistingAp.NonpaymentCategory =
          db.GetNullableString(reader, 19);
        entities.ExistingAp.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 20);
        entities.ExistingAp.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.ExistingAp.CreatedTimestamp = db.GetDateTime(reader, 22);
        entities.ExistingAp.CreatedBy = db.GetString(reader, 23);
        entities.ExistingAp.Note = db.GetNullableString(reader, 24);
        entities.ExistingAp.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAp.Type1);
        CheckValid<CaseRole>("ParentType", entities.ExistingAp.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.ExistingAp.LivingWithArIndicator);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ExistingAr.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAr.CasNumber = db.GetString(reader, 0);
        entities.ExistingAr.CspNumber = db.GetString(reader, 1);
        entities.ExistingAr.Type1 = db.GetString(reader, 2);
        entities.ExistingAr.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAr.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAr.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingAr.OnSsInd = db.GetNullableString(reader, 6);
        entities.ExistingAr.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.ExistingAr.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingAr.ContactFirstName = db.GetNullableString(reader, 9);
        entities.ExistingAr.ContactMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.ExistingAr.ContactPhone = db.GetNullableString(reader, 11);
        entities.ExistingAr.ContactLastName = db.GetNullableString(reader, 12);
        entities.ExistingAr.ChildCareExpenses =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingAr.AssignmentDate = db.GetNullableDate(reader, 14);
        entities.ExistingAr.AssignmentTerminationCode =
          db.GetNullableString(reader, 15);
        entities.ExistingAr.AssignmentOfRights =
          db.GetNullableString(reader, 16);
        entities.ExistingAr.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 17);
        entities.ExistingAr.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.ExistingAr.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.ExistingAr.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.ExistingAr.CreatedBy = db.GetString(reader, 21);
        entities.ExistingAr.ArChgProcReqInd = db.GetNullableString(reader, 22);
        entities.ExistingAr.ArChgProcessedDate = db.GetNullableDate(reader, 23);
        entities.ExistingAr.ArInvalidInd = db.GetNullableString(reader, 24);
        entities.ExistingAr.Note = db.GetNullableString(reader, 25);
        entities.ExistingAr.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAr.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.ExistingCh.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCh.CasNumber = db.GetString(reader, 0);
        entities.ExistingCh.CspNumber = db.GetString(reader, 1);
        entities.ExistingCh.Type1 = db.GetString(reader, 2);
        entities.ExistingCh.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCh.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCh.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCh.OnSsInd = db.GetNullableString(reader, 6);
        entities.ExistingCh.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.ExistingCh.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingCh.AbsenceReasonCode = db.GetNullableString(reader, 9);
        entities.ExistingCh.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 10);
        entities.ExistingCh.ArWaivedInsurance =
          db.GetNullableString(reader, 11);
        entities.ExistingCh.DateOfEmancipation = db.GetNullableDate(reader, 12);
        entities.ExistingCh.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 13);
        entities.ExistingCh.FcApNotified = db.GetNullableString(reader, 14);
        entities.ExistingCh.FcCincInd = db.GetNullableString(reader, 15);
        entities.ExistingCh.FcCostOfCare = db.GetNullableDecimal(reader, 16);
        entities.ExistingCh.FcCostOfCareFreq = db.GetNullableString(reader, 17);
        entities.ExistingCh.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 18);
        entities.ExistingCh.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 19);
        entities.ExistingCh.FcInHomeServiceInd =
          db.GetNullableString(reader, 20);
        entities.ExistingCh.FcIvECaseNumber = db.GetNullableString(reader, 21);
        entities.ExistingCh.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 22);
        entities.ExistingCh.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 23);
        entities.ExistingCh.FcLevelOfCare = db.GetNullableString(reader, 24);
        entities.ExistingCh.FcNextJuvenileCtDt = db.GetNullableDate(reader, 25);
        entities.ExistingCh.FcOrderEstBy = db.GetNullableString(reader, 26);
        entities.ExistingCh.FcOtherBenefitInd =
          db.GetNullableString(reader, 27);
        entities.ExistingCh.FcParentalRights = db.GetNullableString(reader, 28);
        entities.ExistingCh.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 29);
        entities.ExistingCh.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 30);
        entities.ExistingCh.FcPlacementDate = db.GetNullableDate(reader, 31);
        entities.ExistingCh.FcPlacementName = db.GetNullableString(reader, 32);
        entities.ExistingCh.FcPlacementReason =
          db.GetNullableString(reader, 33);
        entities.ExistingCh.FcPreviousPa = db.GetNullableString(reader, 34);
        entities.ExistingCh.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 35);
        entities.ExistingCh.FcSourceOfFunding =
          db.GetNullableString(reader, 36);
        entities.ExistingCh.FcSrsPayee = db.GetNullableString(reader, 37);
        entities.ExistingCh.FcSsa = db.GetNullableString(reader, 38);
        entities.ExistingCh.FcSsi = db.GetNullableString(reader, 39);
        entities.ExistingCh.FcVaInd = db.GetNullableString(reader, 40);
        entities.ExistingCh.FcWardsAccount = db.GetNullableString(reader, 41);
        entities.ExistingCh.FcZebInd = db.GetNullableString(reader, 42);
        entities.ExistingCh.Over18AndInSchool =
          db.GetNullableString(reader, 43);
        entities.ExistingCh.ResidesWithArIndicator =
          db.GetNullableString(reader, 44);
        entities.ExistingCh.SpecialtyArea = db.GetNullableString(reader, 45);
        entities.ExistingCh.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 46);
        entities.ExistingCh.LastUpdatedBy = db.GetNullableString(reader, 47);
        entities.ExistingCh.CreatedTimestamp = db.GetDateTime(reader, 48);
        entities.ExistingCh.CreatedBy = db.GetString(reader, 49);
        entities.ExistingCh.RelToAr = db.GetNullableString(reader, 50);
        entities.ExistingCh.Note = db.GetNullableString(reader, 51);
        entities.ExistingCh.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCh.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.ExistingCh.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.ExistingCh.SpecialtyArea);
          
      });
  }

  private bool ReadCaseRole4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);
    entities.ExistingAr.Populated = false;

    return Read("ReadCaseRole4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingAp.CasNumber);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAr.CasNumber = db.GetString(reader, 0);
        entities.ExistingAr.CspNumber = db.GetString(reader, 1);
        entities.ExistingAr.Type1 = db.GetString(reader, 2);
        entities.ExistingAr.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAr.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAr.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingAr.OnSsInd = db.GetNullableString(reader, 6);
        entities.ExistingAr.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.ExistingAr.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingAr.ContactFirstName = db.GetNullableString(reader, 9);
        entities.ExistingAr.ContactMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.ExistingAr.ContactPhone = db.GetNullableString(reader, 11);
        entities.ExistingAr.ContactLastName = db.GetNullableString(reader, 12);
        entities.ExistingAr.ChildCareExpenses =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingAr.AssignmentDate = db.GetNullableDate(reader, 14);
        entities.ExistingAr.AssignmentTerminationCode =
          db.GetNullableString(reader, 15);
        entities.ExistingAr.AssignmentOfRights =
          db.GetNullableString(reader, 16);
        entities.ExistingAr.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 17);
        entities.ExistingAr.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.ExistingAr.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.ExistingAr.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.ExistingAr.CreatedBy = db.GetString(reader, 21);
        entities.ExistingAr.ArChgProcReqInd = db.GetNullableString(reader, 22);
        entities.ExistingAr.ArChgProcessedDate = db.GetNullableDate(reader, 23);
        entities.ExistingAr.ArInvalidInd = db.GetNullableString(reader, 24);
        entities.ExistingAr.Note = db.GetNullableString(reader, 25);
        entities.ExistingAr.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAr.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRole5()
  {
    entities.ExistingCh.Populated = false;

    return ReadEach("ReadCaseRole5",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCh.CasNumber = db.GetString(reader, 0);
        entities.ExistingCh.CspNumber = db.GetString(reader, 1);
        entities.ExistingCh.Type1 = db.GetString(reader, 2);
        entities.ExistingCh.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCh.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCh.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCh.OnSsInd = db.GetNullableString(reader, 6);
        entities.ExistingCh.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.ExistingCh.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingCh.AbsenceReasonCode = db.GetNullableString(reader, 9);
        entities.ExistingCh.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 10);
        entities.ExistingCh.ArWaivedInsurance =
          db.GetNullableString(reader, 11);
        entities.ExistingCh.DateOfEmancipation = db.GetNullableDate(reader, 12);
        entities.ExistingCh.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 13);
        entities.ExistingCh.FcApNotified = db.GetNullableString(reader, 14);
        entities.ExistingCh.FcCincInd = db.GetNullableString(reader, 15);
        entities.ExistingCh.FcCostOfCare = db.GetNullableDecimal(reader, 16);
        entities.ExistingCh.FcCostOfCareFreq = db.GetNullableString(reader, 17);
        entities.ExistingCh.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 18);
        entities.ExistingCh.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 19);
        entities.ExistingCh.FcInHomeServiceInd =
          db.GetNullableString(reader, 20);
        entities.ExistingCh.FcIvECaseNumber = db.GetNullableString(reader, 21);
        entities.ExistingCh.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 22);
        entities.ExistingCh.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 23);
        entities.ExistingCh.FcLevelOfCare = db.GetNullableString(reader, 24);
        entities.ExistingCh.FcNextJuvenileCtDt = db.GetNullableDate(reader, 25);
        entities.ExistingCh.FcOrderEstBy = db.GetNullableString(reader, 26);
        entities.ExistingCh.FcOtherBenefitInd =
          db.GetNullableString(reader, 27);
        entities.ExistingCh.FcParentalRights = db.GetNullableString(reader, 28);
        entities.ExistingCh.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 29);
        entities.ExistingCh.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 30);
        entities.ExistingCh.FcPlacementDate = db.GetNullableDate(reader, 31);
        entities.ExistingCh.FcPlacementName = db.GetNullableString(reader, 32);
        entities.ExistingCh.FcPlacementReason =
          db.GetNullableString(reader, 33);
        entities.ExistingCh.FcPreviousPa = db.GetNullableString(reader, 34);
        entities.ExistingCh.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 35);
        entities.ExistingCh.FcSourceOfFunding =
          db.GetNullableString(reader, 36);
        entities.ExistingCh.FcSrsPayee = db.GetNullableString(reader, 37);
        entities.ExistingCh.FcSsa = db.GetNullableString(reader, 38);
        entities.ExistingCh.FcSsi = db.GetNullableString(reader, 39);
        entities.ExistingCh.FcVaInd = db.GetNullableString(reader, 40);
        entities.ExistingCh.FcWardsAccount = db.GetNullableString(reader, 41);
        entities.ExistingCh.FcZebInd = db.GetNullableString(reader, 42);
        entities.ExistingCh.Over18AndInSchool =
          db.GetNullableString(reader, 43);
        entities.ExistingCh.ResidesWithArIndicator =
          db.GetNullableString(reader, 44);
        entities.ExistingCh.SpecialtyArea = db.GetNullableString(reader, 45);
        entities.ExistingCh.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 46);
        entities.ExistingCh.LastUpdatedBy = db.GetNullableString(reader, 47);
        entities.ExistingCh.CreatedTimestamp = db.GetDateTime(reader, 48);
        entities.ExistingCh.CreatedBy = db.GetString(reader, 49);
        entities.ExistingCh.RelToAr = db.GetNullableString(reader, 50);
        entities.ExistingCh.Note = db.GetNullableString(reader, 51);
        entities.ExistingCh.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCh.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.ExistingCh.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.ExistingCh.SpecialtyArea);
          

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.AeCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.ExistingCsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePerson.CurrentSpouseMi =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.ExistingCsePerson.BirthPlaceState =
          db.GetNullableString(reader, 8);
        entities.ExistingCsePerson.EmergencyPhone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingCsePerson.NameMiddle =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePerson.NameMaiden =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.ExistingCsePerson.OtherNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.ExistingCsePerson.Race = db.GetNullableString(reader, 17);
        entities.ExistingCsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.ExistingCsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.ExistingCsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.ExistingCsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.ExistingCsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.ExistingCsePerson.CreatedBy = db.GetString(reader, 23);
        entities.ExistingCsePerson.CreatedTimestamp =
          db.GetDateTime(reader, 24);
        entities.ExistingCsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.ExistingCsePerson.LastUpdatedBy =
          db.GetNullableString(reader, 26);
        entities.ExistingCsePerson.KscaresNumber =
          db.GetNullableString(reader, 27);
        entities.ExistingCsePerson.OtherAreaCode =
          db.GetNullableInt32(reader, 28);
        entities.ExistingCsePerson.EmergencyAreaCode =
          db.GetNullableInt32(reader, 29);
        entities.ExistingCsePerson.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 30);
        entities.ExistingCsePerson.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 31);
        entities.ExistingCsePerson.WorkPhone = db.GetNullableInt32(reader, 32);
        entities.ExistingCsePerson.WorkPhoneExt =
          db.GetNullableString(reader, 33);
        entities.ExistingCsePerson.OtherPhoneType =
          db.GetNullableString(reader, 34);
        entities.ExistingCsePerson.UnemploymentInd =
          db.GetNullableString(reader, 35);
        entities.ExistingCsePerson.FederalInd =
          db.GetNullableString(reader, 36);
        entities.ExistingCsePerson.OtherIdInfo =
          db.GetNullableString(reader, 37);
        entities.ExistingCsePerson.BornOutOfWedlock =
          db.GetNullableString(reader, 38);
        entities.ExistingCsePerson.CseToEstblPaternity =
          db.GetNullableString(reader, 39);
        entities.ExistingCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 40);
        entities.ExistingCsePerson.DatePaternEstab = db.GetDate(reader, 41);
        entities.ExistingCsePerson.BirthCertFathersLastName =
          db.GetNullableString(reader, 42);
        entities.ExistingCsePerson.BirthCertFathersFirstName =
          db.GetNullableString(reader, 43);
        entities.ExistingCsePerson.BirthCertFathersMi =
          db.GetNullableString(reader, 44);
        entities.ExistingCsePerson.BirthCertificateSignature =
          db.GetNullableString(reader, 45);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadInformationRequest()
  {
    entities.ExistingInformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", import.FromInrdInformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.ExistingInformationRequest.Number = db.GetInt64(reader, 0);
        entities.ExistingInformationRequest.NonparentQuestionnaireSent =
          db.GetNullableString(reader, 1);
        entities.ExistingInformationRequest.ParentQuestionnaireSent =
          db.GetNullableString(reader, 2);
        entities.ExistingInformationRequest.PaternityQuestionnaireSent =
          db.GetNullableString(reader, 3);
        entities.ExistingInformationRequest.ApplicationSentIndicator =
          db.GetString(reader, 4);
        entities.ExistingInformationRequest.QuestionnaireTypeIndicator =
          db.GetString(reader, 5);
        entities.ExistingInformationRequest.DateReceivedByCseComplete =
          db.GetNullableDate(reader, 6);
        entities.ExistingInformationRequest.DateReceivedByCseIncomplete =
          db.GetNullableDate(reader, 7);
        entities.ExistingInformationRequest.DateApplicationRequested =
          db.GetDate(reader, 8);
        entities.ExistingInformationRequest.CallerLastName =
          db.GetNullableString(reader, 9);
        entities.ExistingInformationRequest.CallerFirstName =
          db.GetString(reader, 10);
        entities.ExistingInformationRequest.CallerMiddleInitial =
          db.GetString(reader, 11);
        entities.ExistingInformationRequest.InquirerNameSuffix =
          db.GetNullableString(reader, 12);
        entities.ExistingInformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 13);
        entities.ExistingInformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 14);
        entities.ExistingInformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 15);
        entities.ExistingInformationRequest.ApplicantNameSuffix =
          db.GetString(reader, 16);
        entities.ExistingInformationRequest.ApplicantStreet1 =
          db.GetNullableString(reader, 17);
        entities.ExistingInformationRequest.ApplicantStreet2 =
          db.GetNullableString(reader, 18);
        entities.ExistingInformationRequest.ApplicantCity =
          db.GetNullableString(reader, 19);
        entities.ExistingInformationRequest.ApplicantState =
          db.GetNullableString(reader, 20);
        entities.ExistingInformationRequest.ApplicantZip5 =
          db.GetNullableString(reader, 21);
        entities.ExistingInformationRequest.ApplicantZip4 =
          db.GetNullableString(reader, 22);
        entities.ExistingInformationRequest.ApplicantZip3 =
          db.GetNullableString(reader, 23);
        entities.ExistingInformationRequest.ApplicantPhone =
          db.GetNullableInt32(reader, 24);
        entities.ExistingInformationRequest.DateApplicationSent =
          db.GetNullableDate(reader, 25);
        entities.ExistingInformationRequest.Type1 = db.GetString(reader, 26);
        entities.ExistingInformationRequest.ServiceCode =
          db.GetNullableString(reader, 27);
        entities.ExistingInformationRequest.ReasonIncomplete =
          db.GetNullableString(reader, 28);
        entities.ExistingInformationRequest.Note =
          db.GetNullableString(reader, 29);
        entities.ExistingInformationRequest.CreatedBy =
          db.GetString(reader, 30);
        entities.ExistingInformationRequest.CreatedTimestamp =
          db.GetDateTime(reader, 31);
        entities.ExistingInformationRequest.LastUpdatedBy =
          db.GetNullableString(reader, 32);
        entities.ExistingInformationRequest.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 33);
        entities.ExistingInformationRequest.ReasonDenied =
          db.GetNullableString(reader, 34);
        entities.ExistingInformationRequest.DateDenied =
          db.GetNullableDate(reader, 35);
        entities.ExistingInformationRequest.ApplicantAreaCode =
          db.GetNullableInt32(reader, 36);
        entities.ExistingInformationRequest.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.ExistingInterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr",
          import.FromInterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.FromInterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.ExistingInterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.ExistingInterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.ExistingInterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.ExistingInterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.ExistingInterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.ExistingInterstateCase.TransSerialNumber =
          db.GetInt64(reader, 6);
        entities.ExistingInterstateCase.ActionCode = db.GetString(reader, 7);
        entities.ExistingInterstateCase.FunctionalTypeCode =
          db.GetString(reader, 8);
        entities.ExistingInterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.ExistingInterstateCase.KsCaseId =
          db.GetNullableString(reader, 10);
        entities.ExistingInterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.ExistingInterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.ExistingInterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.ExistingInterstateCase.AttachmentsInd =
          db.GetString(reader, 14);
        entities.ExistingInterstateCase.CaseDataInd =
          db.GetNullableInt32(reader, 15);
        entities.ExistingInterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.ExistingInterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.ExistingInterstateCase.OrderDataInd =
          db.GetNullableInt32(reader, 19);
        entities.ExistingInterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.ExistingInterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.ExistingInterstateCase.SentDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInterstateCase.SentTime =
          db.GetNullableTimeSpan(reader, 23);
        entities.ExistingInterstateCase.DueDate =
          db.GetNullableDate(reader, 24);
        entities.ExistingInterstateCase.OverdueInd =
          db.GetNullableInt32(reader, 25);
        entities.ExistingInterstateCase.DateReceived =
          db.GetNullableDate(reader, 26);
        entities.ExistingInterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.ExistingInterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingInterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.ExistingInterstateCase.CaseType = db.GetString(reader, 30);
        entities.ExistingInterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.ExistingInterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.ExistingInterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.ExistingInterstateCase.PaymentCity =
          db.GetNullableString(reader, 34);
        entities.ExistingInterstateCase.PaymentState =
          db.GetNullableString(reader, 35);
        entities.ExistingInterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.ExistingInterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.ExistingInterstateCase.ContactNameLast =
          db.GetNullableString(reader, 38);
        entities.ExistingInterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 39);
        entities.ExistingInterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 40);
        entities.ExistingInterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 41);
        entities.ExistingInterstateCase.ContactAddressLine1 =
          db.GetString(reader, 42);
        entities.ExistingInterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 43);
        entities.ExistingInterstateCase.ContactCity =
          db.GetNullableString(reader, 44);
        entities.ExistingInterstateCase.ContactState =
          db.GetNullableString(reader, 45);
        entities.ExistingInterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 46);
        entities.ExistingInterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 47);
        entities.ExistingInterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 48);
        entities.ExistingInterstateCase.AssnDeactDt =
          db.GetNullableDate(reader, 49);
        entities.ExistingInterstateCase.AssnDeactInd =
          db.GetNullableString(reader, 50);
        entities.ExistingInterstateCase.LastDeferDt =
          db.GetNullableDate(reader, 51);
        entities.ExistingInterstateCase.Memo = db.GetNullableString(reader, 52);
        entities.ExistingInterstateCase.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.ExistingPaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          import.FromPaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "numb", import.FromPaReferral.Number);
        db.SetString(command, "type", import.FromPaReferral.Type1);
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

  private bool ReadPaReferralParticipant()
  {
    entities.ExistingPaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.ExistingPaReferral.Number);
        db.SetString(command, "pafType", entities.ExistingPaReferral.Type1);
        db.SetNullableString(
          command, "personNum", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPaReferralParticipant.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaReferralParticipant.CreatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingPaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 2);
        entities.ExistingPaReferralParticipant.Relationship =
          db.GetNullableString(reader, 3);
        entities.ExistingPaReferralParticipant.Sex =
          db.GetNullableString(reader, 4);
        entities.ExistingPaReferralParticipant.Dob =
          db.GetNullableDate(reader, 5);
        entities.ExistingPaReferralParticipant.LastName =
          db.GetNullableString(reader, 6);
        entities.ExistingPaReferralParticipant.FirstName =
          db.GetNullableString(reader, 7);
        entities.ExistingPaReferralParticipant.Mi =
          db.GetNullableString(reader, 8);
        entities.ExistingPaReferralParticipant.Ssn =
          db.GetNullableString(reader, 9);
        entities.ExistingPaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingPaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 11);
        entities.ExistingPaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 12);
        entities.ExistingPaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 13);
        entities.ExistingPaReferralParticipant.CreatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingPaReferralParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.ExistingPaReferralParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.ExistingPaReferralParticipant.PreNumber =
          db.GetString(reader, 17);
        entities.ExistingPaReferralParticipant.GoodCauseStatus =
          db.GetNullableString(reader, 18);
        entities.ExistingPaReferralParticipant.PafType =
          db.GetString(reader, 19);
        entities.ExistingPaReferralParticipant.PafTstamp =
          db.GetDateTime(reader, 20);
        entities.ExistingPaReferralParticipant.Role =
          db.GetNullableString(reader, 21);
        entities.ExistingPaReferralParticipant.Populated = true;
      });
  }

  private void UpdateCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);

    var onSsInd = export.ApCaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator =
      export.ApCaseRole.HealthInsuranceIndicator ?? "";
    var medicalSupportIndicator = export.ApCaseRole.MedicalSupportIndicator ?? ""
      ;
    var mothersFirstName = export.ApCaseRole.MothersFirstName ?? "";
    var mothersMiddleInitial = export.ApCaseRole.MothersMiddleInitial ?? "";
    var fathersLastName = export.ApCaseRole.FathersLastName ?? "";
    var fathersMiddleInitial = export.ApCaseRole.FathersMiddleInitial ?? "";
    var fathersFirstName = export.ApCaseRole.FathersFirstName ?? "";
    var mothersMaidenLastName = export.ApCaseRole.MothersMaidenLastName ?? "";
    var parentType = export.ApCaseRole.ParentType ?? "";
    var notifiedDate = export.ApCaseRole.NotifiedDate;
    var numberOfChildren =
      export.ApCaseRole.NumberOfChildren.GetValueOrDefault();
    var livingWithArIndicator = export.ApCaseRole.LivingWithArIndicator ?? "";
    var nonpaymentCategory = export.ApCaseRole.NonpaymentCategory ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var note = export.ApCaseRole.Note ?? "";

    CheckValid<CaseRole>("ParentType", parentType);
    CheckValid<CaseRole>("LivingWithArIndicator", livingWithArIndicator);
    entities.ExistingAp.Populated = false;
    Update("UpdateCaseRole1",
      (db, command) =>
      {
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
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.ExistingAp.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingAp.CspNumber);
        db.SetString(command, "type", entities.ExistingAp.Type1);
        db.SetInt32(command, "caseRoleId", entities.ExistingAp.Identifier);
      });

    entities.ExistingAp.OnSsInd = onSsInd;
    entities.ExistingAp.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.ExistingAp.MedicalSupportIndicator = medicalSupportIndicator;
    entities.ExistingAp.MothersFirstName = mothersFirstName;
    entities.ExistingAp.MothersMiddleInitial = mothersMiddleInitial;
    entities.ExistingAp.FathersLastName = fathersLastName;
    entities.ExistingAp.FathersMiddleInitial = fathersMiddleInitial;
    entities.ExistingAp.FathersFirstName = fathersFirstName;
    entities.ExistingAp.MothersMaidenLastName = mothersMaidenLastName;
    entities.ExistingAp.ParentType = parentType;
    entities.ExistingAp.NotifiedDate = notifiedDate;
    entities.ExistingAp.NumberOfChildren = numberOfChildren;
    entities.ExistingAp.LivingWithArIndicator = livingWithArIndicator;
    entities.ExistingAp.NonpaymentCategory = nonpaymentCategory;
    entities.ExistingAp.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingAp.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingAp.Note = note;
    entities.ExistingAp.Populated = true;
  }

  private void UpdateCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAr.Populated);

    var onSsInd = export.ArCaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator =
      export.ArCaseRole.HealthInsuranceIndicator ?? "";
    var medicalSupportIndicator = export.ArCaseRole.MedicalSupportIndicator ?? ""
      ;
    var contactFirstName = export.ArCaseRole.ContactFirstName ?? "";
    var contactMiddleInitial = export.ArCaseRole.ContactMiddleInitial ?? "";
    var contactPhone = export.ArCaseRole.ContactPhone ?? "";
    var contactLastName = export.ArCaseRole.ContactLastName ?? "";
    var childCareExpenses =
      export.ArCaseRole.ChildCareExpenses.GetValueOrDefault();
    var assignmentDate = export.ArCaseRole.AssignmentDate;
    var assignmentTerminationCode =
      export.ArCaseRole.AssignmentTerminationCode ?? "";
    var assignmentOfRights = export.ArCaseRole.AssignmentOfRights ?? "";
    var assignmentTerminatedDt = export.ArCaseRole.AssignmentTerminatedDt;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var arChgProcReqInd = export.ArCaseRole.ArChgProcReqInd ?? "";
    var arChgProcessedDate = export.ArCaseRole.ArChgProcessedDate;
    var arInvalidInd = export.ArCaseRole.ArInvalidInd ?? "";
    var note = export.ArCaseRole.Note ?? "";

    entities.ExistingAr.Populated = false;
    Update("UpdateCaseRole2",
      (db, command) =>
      {
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
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.ExistingAr.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingAr.CspNumber);
        db.SetString(command, "type", entities.ExistingAr.Type1);
        db.SetInt32(command, "caseRoleId", entities.ExistingAr.Identifier);
      });

    entities.ExistingAr.OnSsInd = onSsInd;
    entities.ExistingAr.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.ExistingAr.MedicalSupportIndicator = medicalSupportIndicator;
    entities.ExistingAr.ContactFirstName = contactFirstName;
    entities.ExistingAr.ContactMiddleInitial = contactMiddleInitial;
    entities.ExistingAr.ContactPhone = contactPhone;
    entities.ExistingAr.ContactLastName = contactLastName;
    entities.ExistingAr.ChildCareExpenses = childCareExpenses;
    entities.ExistingAr.AssignmentDate = assignmentDate;
    entities.ExistingAr.AssignmentTerminationCode = assignmentTerminationCode;
    entities.ExistingAr.AssignmentOfRights = assignmentOfRights;
    entities.ExistingAr.AssignmentTerminatedDt = assignmentTerminatedDt;
    entities.ExistingAr.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingAr.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingAr.ArChgProcReqInd = arChgProcReqInd;
    entities.ExistingAr.ArChgProcessedDate = arChgProcessedDate;
    entities.ExistingAr.ArInvalidInd = arInvalidInd;
    entities.ExistingAr.Note = note;
    entities.ExistingAr.Populated = true;
  }

  private void UpdateCaseRole3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCh.Populated);

    var absenceReasonCode = export.ChCaseRole.AbsenceReasonCode ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var relToAr = export.ChCaseRole.RelToAr ?? "";

    entities.ExistingCh.Populated = false;
    Update("UpdateCaseRole3",
      (db, command) =>
      {
        db.SetNullableString(command, "absenceReasonCd", absenceReasonCode);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetString(command, "casNumber", entities.ExistingCh.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingCh.CspNumber);
        db.SetString(command, "type", entities.ExistingCh.Type1);
        db.SetInt32(command, "caseRoleId", entities.ExistingCh.Identifier);
      });

    entities.ExistingCh.AbsenceReasonCode = absenceReasonCode;
    entities.ExistingCh.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCh.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCh.RelToAr = relToAr;
    entities.ExistingCh.Populated = true;
  }

  private void UpdateCaseRole4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCh.Populated);

    var onSsInd = export.ChCaseRole.OnSsInd ?? "";
    var healthInsuranceIndicator =
      export.ChCaseRole.HealthInsuranceIndicator ?? "";
    var medicalSupportIndicator = export.ChCaseRole.MedicalSupportIndicator ?? ""
      ;
    var absenceReasonCode = export.ChCaseRole.AbsenceReasonCode ?? "";
    var priorMedicalSupport =
      export.ChCaseRole.PriorMedicalSupport.GetValueOrDefault();
    var arWaivedInsurance = export.ChCaseRole.ArWaivedInsurance ?? "";
    var dateOfEmancipation = export.ChCaseRole.DateOfEmancipation;
    var fcAdoptionDisruptionInd = export.ChCaseRole.FcAdoptionDisruptionInd ?? ""
      ;
    var fcApNotified = export.ChCaseRole.FcApNotified ?? "";
    var fcCincInd = export.ChCaseRole.FcCincInd ?? "";
    var fcCostOfCare = export.ChCaseRole.FcCostOfCare.GetValueOrDefault();
    var fcCostOfCareFreq = export.ChCaseRole.FcCostOfCareFreq ?? "";
    var fcCountyChildRemovedFrom =
      export.ChCaseRole.FcCountyChildRemovedFrom ?? "";
    var fcDateOfInitialCustody = export.ChCaseRole.FcDateOfInitialCustody;
    var fcInHomeServiceInd = export.ChCaseRole.FcInHomeServiceInd ?? "";
    var fcIvECaseNumber = export.ChCaseRole.FcIvECaseNumber ?? "";
    var fcJuvenileCourtOrder = export.ChCaseRole.FcJuvenileCourtOrder ?? "";
    var fcJuvenileOffenderInd = export.ChCaseRole.FcJuvenileOffenderInd ?? "";
    var fcLevelOfCare = export.ChCaseRole.FcLevelOfCare ?? "";
    var fcNextJuvenileCtDt = export.ChCaseRole.FcNextJuvenileCtDt;
    var fcOrderEstBy = export.ChCaseRole.FcOrderEstBy ?? "";
    var fcOtherBenefitInd = export.ChCaseRole.FcOtherBenefitInd ?? "";
    var fcParentalRights = export.ChCaseRole.FcParentalRights ?? "";
    var fcPrevPayeeFirstName = export.ChCaseRole.FcPrevPayeeFirstName ?? "";
    var fcPrevPayeeMiddleInitial =
      export.ChCaseRole.FcPrevPayeeMiddleInitial ?? "";
    var fcPlacementDate = export.ChCaseRole.FcPlacementDate;
    var fcPlacementName = export.ChCaseRole.FcPlacementName ?? "";
    var fcPlacementReason = export.ChCaseRole.FcPlacementReason ?? "";
    var fcPreviousPa = export.ChCaseRole.FcPreviousPa ?? "";
    var fcPreviousPayeeLastName = export.ChCaseRole.FcPreviousPayeeLastName ?? ""
      ;
    var fcSourceOfFunding = export.ChCaseRole.FcSourceOfFunding ?? "";
    var fcSrsPayee = export.ChCaseRole.FcSrsPayee ?? "";
    var fcSsa = export.ChCaseRole.FcSsa ?? "";
    var fcSsi = export.ChCaseRole.FcSsi ?? "";
    var fcVaInd = export.ChCaseRole.FcVaInd ?? "";
    var fcWardsAccount = export.ChCaseRole.FcWardsAccount ?? "";
    var fcZebInd = export.ChCaseRole.FcZebInd ?? "";
    var over18AndInSchool = export.ChCaseRole.Over18AndInSchool ?? "";
    var residesWithArIndicator = export.ChCaseRole.ResidesWithArIndicator ?? "";
    var specialtyArea = export.ChCaseRole.SpecialtyArea ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var relToAr = export.ChCaseRole.RelToAr ?? "";
    var note = export.ChCaseRole.Note ?? "";

    CheckValid<CaseRole>("ResidesWithArIndicator", residesWithArIndicator);
    CheckValid<CaseRole>("SpecialtyArea", specialtyArea);
    entities.ExistingCh.Populated = false;
    Update("UpdateCaseRole4",
      (db, command) =>
      {
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.SetNullableString(command, "absenceReasonCd", absenceReasonCode);
        db.SetNullableDecimal(command, "priorMedicalSupp", priorMedicalSupport);
        db.SetNullableString(command, "arWaivedIns", arWaivedInsurance);
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
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "casNumber", entities.ExistingCh.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingCh.CspNumber);
        db.SetString(command, "type", entities.ExistingCh.Type1);
        db.SetInt32(command, "caseRoleId", entities.ExistingCh.Identifier);
      });

    entities.ExistingCh.OnSsInd = onSsInd;
    entities.ExistingCh.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.ExistingCh.MedicalSupportIndicator = medicalSupportIndicator;
    entities.ExistingCh.AbsenceReasonCode = absenceReasonCode;
    entities.ExistingCh.PriorMedicalSupport = priorMedicalSupport;
    entities.ExistingCh.ArWaivedInsurance = arWaivedInsurance;
    entities.ExistingCh.DateOfEmancipation = dateOfEmancipation;
    entities.ExistingCh.FcAdoptionDisruptionInd = fcAdoptionDisruptionInd;
    entities.ExistingCh.FcApNotified = fcApNotified;
    entities.ExistingCh.FcCincInd = fcCincInd;
    entities.ExistingCh.FcCostOfCare = fcCostOfCare;
    entities.ExistingCh.FcCostOfCareFreq = fcCostOfCareFreq;
    entities.ExistingCh.FcCountyChildRemovedFrom = fcCountyChildRemovedFrom;
    entities.ExistingCh.FcDateOfInitialCustody = fcDateOfInitialCustody;
    entities.ExistingCh.FcInHomeServiceInd = fcInHomeServiceInd;
    entities.ExistingCh.FcIvECaseNumber = fcIvECaseNumber;
    entities.ExistingCh.FcJuvenileCourtOrder = fcJuvenileCourtOrder;
    entities.ExistingCh.FcJuvenileOffenderInd = fcJuvenileOffenderInd;
    entities.ExistingCh.FcLevelOfCare = fcLevelOfCare;
    entities.ExistingCh.FcNextJuvenileCtDt = fcNextJuvenileCtDt;
    entities.ExistingCh.FcOrderEstBy = fcOrderEstBy;
    entities.ExistingCh.FcOtherBenefitInd = fcOtherBenefitInd;
    entities.ExistingCh.FcParentalRights = fcParentalRights;
    entities.ExistingCh.FcPrevPayeeFirstName = fcPrevPayeeFirstName;
    entities.ExistingCh.FcPrevPayeeMiddleInitial = fcPrevPayeeMiddleInitial;
    entities.ExistingCh.FcPlacementDate = fcPlacementDate;
    entities.ExistingCh.FcPlacementName = fcPlacementName;
    entities.ExistingCh.FcPlacementReason = fcPlacementReason;
    entities.ExistingCh.FcPreviousPa = fcPreviousPa;
    entities.ExistingCh.FcPreviousPayeeLastName = fcPreviousPayeeLastName;
    entities.ExistingCh.FcSourceOfFunding = fcSourceOfFunding;
    entities.ExistingCh.FcSrsPayee = fcSrsPayee;
    entities.ExistingCh.FcSsa = fcSsa;
    entities.ExistingCh.FcSsi = fcSsi;
    entities.ExistingCh.FcVaInd = fcVaInd;
    entities.ExistingCh.FcWardsAccount = fcWardsAccount;
    entities.ExistingCh.FcZebInd = fcZebInd;
    entities.ExistingCh.Over18AndInSchool = over18AndInSchool;
    entities.ExistingCh.ResidesWithArIndicator = residesWithArIndicator;
    entities.ExistingCh.SpecialtyArea = specialtyArea;
    entities.ExistingCh.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCh.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCh.RelToAr = relToAr;
    entities.ExistingCh.Note = note;
    entities.ExistingCh.Populated = true;
  }

  private void UpdateCsePerson1()
  {
    var occupation = export.ArCsePerson.Occupation ?? "";
    var aeCaseNumber = export.ArCsePerson.AeCaseNumber ?? "";
    var dateOfDeath = export.ArCsePerson.DateOfDeath;
    var illegalAlienIndicator = export.ArCsePerson.IllegalAlienIndicator ?? "";
    var currentSpouseMi = export.ArCsePerson.CurrentSpouseMi ?? "";
    var currentSpouseFirstName = export.ArCsePerson.CurrentSpouseFirstName ?? ""
      ;
    var birthPlaceState = export.ArCsePerson.BirthPlaceState ?? "";
    var emergencyPhone = export.ArCsePerson.EmergencyPhone.GetValueOrDefault();
    var nameMiddle = export.ArCsePerson.NameMiddle ?? "";
    var nameMaiden = export.ArCsePerson.NameMaiden ?? "";
    var homePhone = export.ArCsePerson.HomePhone.GetValueOrDefault();
    var otherNumber = export.ArCsePerson.OtherNumber.GetValueOrDefault();
    var birthPlaceCity = export.ArCsePerson.BirthPlaceCity ?? "";
    var currentMaritalStatus = export.ArCsePerson.CurrentMaritalStatus ?? "";
    var currentSpouseLastName = export.ArCsePerson.CurrentSpouseLastName ?? "";
    var race = export.ArCsePerson.Race ?? "";
    var hairColor = export.ArCsePerson.HairColor ?? "";
    var eyeColor = export.ArCsePerson.EyeColor ?? "";
    var weight = export.ArCsePerson.Weight.GetValueOrDefault();
    var heightFt = export.ArCsePerson.HeightFt.GetValueOrDefault();
    var heightIn = export.ArCsePerson.HeightIn.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var kscaresNumber = export.ArCsePerson.KscaresNumber ?? "";
    var otherAreaCode = export.ArCsePerson.OtherAreaCode.GetValueOrDefault();
    var emergencyAreaCode =
      export.ArCsePerson.EmergencyAreaCode.GetValueOrDefault();
    var homePhoneAreaCode =
      export.ArCsePerson.HomePhoneAreaCode.GetValueOrDefault();
    var workPhoneAreaCode =
      export.ArCsePerson.WorkPhoneAreaCode.GetValueOrDefault();
    var workPhone = export.ArCsePerson.WorkPhone.GetValueOrDefault();
    var workPhoneExt = export.ArCsePerson.WorkPhoneExt ?? "";
    var otherPhoneType = export.ArCsePerson.OtherPhoneType ?? "";
    var unemploymentInd = export.ArCsePerson.UnemploymentInd ?? "";
    var federalInd = export.ArCsePerson.FederalInd ?? "";
    var otherIdInfo = export.ArCsePerson.OtherIdInfo ?? "";

    entities.ExistingCsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableString(command, "aeCaseNumber", aeCaseNumber);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(command, "illegalAlienInd", illegalAlienIndicator);
        db.SetNullableString(command, "currentSpouseMi", currentSpouseMi);
        db.
          SetNullableString(command, "currSpouse1StNm", currentSpouseFirstName);
          
        db.SetNullableString(command, "birthPlaceState", birthPlaceState);
        db.SetNullableInt32(command, "emergencyPhone", emergencyPhone);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "nameMaiden", nameMaiden);
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.SetNullableInt32(command, "otherNumber", otherNumber);
        db.SetNullableString(command, "birthPlaceCity", birthPlaceCity);
        db.SetNullableString(command, "currMaritalSts", currentMaritalStatus);
        db.SetNullableString(command, "curSpouseLastNm", currentSpouseLastName);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "kscaresNumber", kscaresNumber);
        db.SetNullableInt32(command, "otherAreaCode", otherAreaCode);
        db.SetNullableInt32(command, "emergencyAreaCd", emergencyAreaCode);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "otherPhoneType", otherPhoneType);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetNullableString(command, "federalInd", federalInd);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetString(command, "numb", entities.ExistingCsePerson.Number);
      });

    entities.ExistingCsePerson.Occupation = occupation;
    entities.ExistingCsePerson.AeCaseNumber = aeCaseNumber;
    entities.ExistingCsePerson.DateOfDeath = dateOfDeath;
    entities.ExistingCsePerson.IllegalAlienIndicator = illegalAlienIndicator;
    entities.ExistingCsePerson.CurrentSpouseMi = currentSpouseMi;
    entities.ExistingCsePerson.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.ExistingCsePerson.BirthPlaceState = birthPlaceState;
    entities.ExistingCsePerson.EmergencyPhone = emergencyPhone;
    entities.ExistingCsePerson.NameMiddle = nameMiddle;
    entities.ExistingCsePerson.NameMaiden = nameMaiden;
    entities.ExistingCsePerson.HomePhone = homePhone;
    entities.ExistingCsePerson.OtherNumber = otherNumber;
    entities.ExistingCsePerson.BirthPlaceCity = birthPlaceCity;
    entities.ExistingCsePerson.CurrentMaritalStatus = currentMaritalStatus;
    entities.ExistingCsePerson.CurrentSpouseLastName = currentSpouseLastName;
    entities.ExistingCsePerson.Race = race;
    entities.ExistingCsePerson.HairColor = hairColor;
    entities.ExistingCsePerson.EyeColor = eyeColor;
    entities.ExistingCsePerson.Weight = weight;
    entities.ExistingCsePerson.HeightFt = heightFt;
    entities.ExistingCsePerson.HeightIn = heightIn;
    entities.ExistingCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCsePerson.KscaresNumber = kscaresNumber;
    entities.ExistingCsePerson.OtherAreaCode = otherAreaCode;
    entities.ExistingCsePerson.EmergencyAreaCode = emergencyAreaCode;
    entities.ExistingCsePerson.HomePhoneAreaCode = homePhoneAreaCode;
    entities.ExistingCsePerson.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.ExistingCsePerson.WorkPhone = workPhone;
    entities.ExistingCsePerson.WorkPhoneExt = workPhoneExt;
    entities.ExistingCsePerson.OtherPhoneType = otherPhoneType;
    entities.ExistingCsePerson.UnemploymentInd = unemploymentInd;
    entities.ExistingCsePerson.FederalInd = federalInd;
    entities.ExistingCsePerson.OtherIdInfo = otherIdInfo;
    entities.ExistingCsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var occupation = export.ChCsePerson.Occupation ?? "";
    var aeCaseNumber = export.ChCsePerson.AeCaseNumber ?? "";
    var dateOfDeath = export.ChCsePerson.DateOfDeath;
    var illegalAlienIndicator = export.ChCsePerson.IllegalAlienIndicator ?? "";
    var currentSpouseMi = export.ChCsePerson.CurrentSpouseMi ?? "";
    var currentSpouseFirstName = export.ChCsePerson.CurrentSpouseFirstName ?? ""
      ;
    var birthPlaceState = export.ChCsePerson.BirthPlaceState ?? "";
    var emergencyPhone = export.ChCsePerson.EmergencyPhone.GetValueOrDefault();
    var nameMiddle = export.ChCsePerson.NameMiddle ?? "";
    var nameMaiden = export.ChCsePerson.NameMaiden ?? "";
    var homePhone = export.ChCsePerson.HomePhone.GetValueOrDefault();
    var otherNumber = export.ChCsePerson.OtherNumber.GetValueOrDefault();
    var birthPlaceCity = export.ChCsePerson.BirthPlaceCity ?? "";
    var currentMaritalStatus = export.ChCsePerson.CurrentMaritalStatus ?? "";
    var currentSpouseLastName = export.ChCsePerson.CurrentSpouseLastName ?? "";
    var race = export.ChCsePerson.Race ?? "";
    var hairColor = export.ChCsePerson.HairColor ?? "";
    var eyeColor = export.ChCsePerson.EyeColor ?? "";
    var weight = export.ChCsePerson.Weight.GetValueOrDefault();
    var heightFt = export.ChCsePerson.HeightFt.GetValueOrDefault();
    var heightIn = export.ChCsePerson.HeightIn.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var kscaresNumber = export.ChCsePerson.KscaresNumber ?? "";
    var otherAreaCode = export.ChCsePerson.OtherAreaCode.GetValueOrDefault();
    var emergencyAreaCode =
      export.ChCsePerson.EmergencyAreaCode.GetValueOrDefault();
    var homePhoneAreaCode =
      export.ChCsePerson.HomePhoneAreaCode.GetValueOrDefault();
    var workPhoneAreaCode =
      export.ChCsePerson.WorkPhoneAreaCode.GetValueOrDefault();
    var workPhone = export.ChCsePerson.WorkPhone.GetValueOrDefault();
    var workPhoneExt = export.ChCsePerson.WorkPhoneExt ?? "";
    var otherPhoneType = export.ChCsePerson.OtherPhoneType ?? "";
    var unemploymentInd = export.ChCsePerson.UnemploymentInd ?? "";
    var federalInd = export.ChCsePerson.FederalInd ?? "";
    var otherIdInfo = export.ChCsePerson.OtherIdInfo ?? "";
    var bornOutOfWedlock = local.CsePerson.BornOutOfWedlock ?? "";
    var cseToEstblPaternity = local.CsePerson.CseToEstblPaternity ?? "";
    var paternityEstablishedIndicator =
      local.CsePerson.PaternityEstablishedIndicator ?? "";
    var datePaternEstab = local.CsePerson.DatePaternEstab;

    entities.ExistingCsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableString(command, "aeCaseNumber", aeCaseNumber);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(command, "illegalAlienInd", illegalAlienIndicator);
        db.SetNullableString(command, "currentSpouseMi", currentSpouseMi);
        db.
          SetNullableString(command, "currSpouse1StNm", currentSpouseFirstName);
          
        db.SetNullableString(command, "birthPlaceState", birthPlaceState);
        db.SetNullableInt32(command, "emergencyPhone", emergencyPhone);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "nameMaiden", nameMaiden);
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.SetNullableInt32(command, "otherNumber", otherNumber);
        db.SetNullableString(command, "birthPlaceCity", birthPlaceCity);
        db.SetNullableString(command, "currMaritalSts", currentMaritalStatus);
        db.SetNullableString(command, "curSpouseLastNm", currentSpouseLastName);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "kscaresNumber", kscaresNumber);
        db.SetNullableInt32(command, "otherAreaCode", otherAreaCode);
        db.SetNullableInt32(command, "emergencyAreaCd", emergencyAreaCode);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "otherPhoneType", otherPhoneType);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetNullableString(command, "federalInd", federalInd);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetNullableString(command, "outOfWedlock", bornOutOfWedlock);
        db.SetNullableString(command, "cseToEstPatr", cseToEstblPaternity);
        db.SetNullableString(
          command, "patEstabInd", paternityEstablishedIndicator);
        db.SetDate(command, "datePaternEstab", datePaternEstab);
        db.SetString(command, "numb", entities.ExistingCsePerson.Number);
      });

    entities.ExistingCsePerson.Occupation = occupation;
    entities.ExistingCsePerson.AeCaseNumber = aeCaseNumber;
    entities.ExistingCsePerson.DateOfDeath = dateOfDeath;
    entities.ExistingCsePerson.IllegalAlienIndicator = illegalAlienIndicator;
    entities.ExistingCsePerson.CurrentSpouseMi = currentSpouseMi;
    entities.ExistingCsePerson.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.ExistingCsePerson.BirthPlaceState = birthPlaceState;
    entities.ExistingCsePerson.EmergencyPhone = emergencyPhone;
    entities.ExistingCsePerson.NameMiddle = nameMiddle;
    entities.ExistingCsePerson.NameMaiden = nameMaiden;
    entities.ExistingCsePerson.HomePhone = homePhone;
    entities.ExistingCsePerson.OtherNumber = otherNumber;
    entities.ExistingCsePerson.BirthPlaceCity = birthPlaceCity;
    entities.ExistingCsePerson.CurrentMaritalStatus = currentMaritalStatus;
    entities.ExistingCsePerson.CurrentSpouseLastName = currentSpouseLastName;
    entities.ExistingCsePerson.Race = race;
    entities.ExistingCsePerson.HairColor = hairColor;
    entities.ExistingCsePerson.EyeColor = eyeColor;
    entities.ExistingCsePerson.Weight = weight;
    entities.ExistingCsePerson.HeightFt = heightFt;
    entities.ExistingCsePerson.HeightIn = heightIn;
    entities.ExistingCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCsePerson.KscaresNumber = kscaresNumber;
    entities.ExistingCsePerson.OtherAreaCode = otherAreaCode;
    entities.ExistingCsePerson.EmergencyAreaCode = emergencyAreaCode;
    entities.ExistingCsePerson.HomePhoneAreaCode = homePhoneAreaCode;
    entities.ExistingCsePerson.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.ExistingCsePerson.WorkPhone = workPhone;
    entities.ExistingCsePerson.WorkPhoneExt = workPhoneExt;
    entities.ExistingCsePerson.OtherPhoneType = otherPhoneType;
    entities.ExistingCsePerson.UnemploymentInd = unemploymentInd;
    entities.ExistingCsePerson.FederalInd = federalInd;
    entities.ExistingCsePerson.OtherIdInfo = otherIdInfo;
    entities.ExistingCsePerson.BornOutOfWedlock = bornOutOfWedlock;
    entities.ExistingCsePerson.CseToEstblPaternity = cseToEstblPaternity;
    entities.ExistingCsePerson.PaternityEstablishedIndicator =
      paternityEstablishedIndicator;
    entities.ExistingCsePerson.DatePaternEstab = datePaternEstab;
    entities.ExistingCsePerson.Populated = true;
  }

  private void UpdateCsePerson3()
  {
    var occupation = export.ApCsePerson.Occupation ?? "";
    var dateOfDeath = export.ApCsePerson.DateOfDeath;
    var illegalAlienIndicator = export.ApCsePerson.IllegalAlienIndicator ?? "";
    var currentSpouseMi = export.ApCsePerson.CurrentSpouseMi ?? "";
    var currentSpouseFirstName = export.ApCsePerson.CurrentSpouseFirstName ?? ""
      ;
    var birthPlaceState = export.ApCsePerson.BirthPlaceState ?? "";
    var emergencyPhone = export.ApCsePerson.EmergencyPhone.GetValueOrDefault();
    var nameMiddle = export.ApCsePerson.NameMiddle ?? "";
    var nameMaiden = export.ApCsePerson.NameMaiden ?? "";
    var homePhone = export.ApCsePerson.HomePhone.GetValueOrDefault();
    var otherNumber = export.ApCsePerson.OtherNumber.GetValueOrDefault();
    var birthPlaceCity = export.ApCsePerson.BirthPlaceCity ?? "";
    var currentMaritalStatus = export.ApCsePerson.CurrentMaritalStatus ?? "";
    var currentSpouseLastName = export.ApCsePerson.CurrentSpouseLastName ?? "";
    var race = export.ApCsePerson.Race ?? "";
    var hairColor = export.ApCsePerson.HairColor ?? "";
    var eyeColor = export.ApCsePerson.EyeColor ?? "";
    var weight = export.ApCsePerson.Weight.GetValueOrDefault();
    var heightFt = export.ApCsePerson.HeightFt.GetValueOrDefault();
    var heightIn = export.ApCsePerson.HeightIn.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var otherAreaCode = export.ApCsePerson.OtherAreaCode.GetValueOrDefault();
    var emergencyAreaCode =
      export.ApCsePerson.EmergencyAreaCode.GetValueOrDefault();
    var homePhoneAreaCode =
      export.ApCsePerson.HomePhoneAreaCode.GetValueOrDefault();
    var workPhoneAreaCode =
      export.ApCsePerson.WorkPhoneAreaCode.GetValueOrDefault();
    var workPhone = export.ApCsePerson.WorkPhone.GetValueOrDefault();
    var workPhoneExt = export.ApCsePerson.WorkPhoneExt ?? "";
    var otherPhoneType = export.ApCsePerson.OtherPhoneType ?? "";
    var unemploymentInd = export.ApCsePerson.UnemploymentInd ?? "";
    var federalInd = export.ApCsePerson.FederalInd ?? "";
    var otherIdInfo = export.ApCsePerson.OtherIdInfo ?? "";

    entities.ExistingCsePerson.Populated = false;
    Update("UpdateCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "occupation", occupation);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(command, "illegalAlienInd", illegalAlienIndicator);
        db.SetNullableString(command, "currentSpouseMi", currentSpouseMi);
        db.
          SetNullableString(command, "currSpouse1StNm", currentSpouseFirstName);
          
        db.SetNullableString(command, "birthPlaceState", birthPlaceState);
        db.SetNullableInt32(command, "emergencyPhone", emergencyPhone);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "nameMaiden", nameMaiden);
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.SetNullableInt32(command, "otherNumber", otherNumber);
        db.SetNullableString(command, "birthPlaceCity", birthPlaceCity);
        db.SetNullableString(command, "currMaritalSts", currentMaritalStatus);
        db.SetNullableString(command, "curSpouseLastNm", currentSpouseLastName);
        db.SetNullableString(command, "race", race);
        db.SetNullableString(command, "hairColor", hairColor);
        db.SetNullableString(command, "eyeColor", eyeColor);
        db.SetNullableInt32(command, "weight", weight);
        db.SetNullableInt32(command, "heightFt", heightFt);
        db.SetNullableInt32(command, "heightIn", heightIn);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableInt32(command, "otherAreaCode", otherAreaCode);
        db.SetNullableInt32(command, "emergencyAreaCd", emergencyAreaCode);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetNullableString(command, "workPhoneExt", workPhoneExt);
        db.SetNullableString(command, "otherPhoneType", otherPhoneType);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetNullableString(command, "federalInd", federalInd);
        db.SetNullableString(command, "otherIdInfo", otherIdInfo);
        db.SetString(command, "numb", entities.ExistingCsePerson.Number);
      });

    entities.ExistingCsePerson.Occupation = occupation;
    entities.ExistingCsePerson.DateOfDeath = dateOfDeath;
    entities.ExistingCsePerson.IllegalAlienIndicator = illegalAlienIndicator;
    entities.ExistingCsePerson.CurrentSpouseMi = currentSpouseMi;
    entities.ExistingCsePerson.CurrentSpouseFirstName = currentSpouseFirstName;
    entities.ExistingCsePerson.BirthPlaceState = birthPlaceState;
    entities.ExistingCsePerson.EmergencyPhone = emergencyPhone;
    entities.ExistingCsePerson.NameMiddle = nameMiddle;
    entities.ExistingCsePerson.NameMaiden = nameMaiden;
    entities.ExistingCsePerson.HomePhone = homePhone;
    entities.ExistingCsePerson.OtherNumber = otherNumber;
    entities.ExistingCsePerson.BirthPlaceCity = birthPlaceCity;
    entities.ExistingCsePerson.CurrentMaritalStatus = currentMaritalStatus;
    entities.ExistingCsePerson.CurrentSpouseLastName = currentSpouseLastName;
    entities.ExistingCsePerson.Race = race;
    entities.ExistingCsePerson.HairColor = hairColor;
    entities.ExistingCsePerson.EyeColor = eyeColor;
    entities.ExistingCsePerson.Weight = weight;
    entities.ExistingCsePerson.HeightFt = heightFt;
    entities.ExistingCsePerson.HeightIn = heightIn;
    entities.ExistingCsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCsePerson.OtherAreaCode = otherAreaCode;
    entities.ExistingCsePerson.EmergencyAreaCode = emergencyAreaCode;
    entities.ExistingCsePerson.HomePhoneAreaCode = homePhoneAreaCode;
    entities.ExistingCsePerson.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.ExistingCsePerson.WorkPhone = workPhone;
    entities.ExistingCsePerson.WorkPhoneExt = workPhoneExt;
    entities.ExistingCsePerson.OtherPhoneType = otherPhoneType;
    entities.ExistingCsePerson.UnemploymentInd = unemploymentInd;
    entities.ExistingCsePerson.FederalInd = federalInd;
    entities.ExistingCsePerson.OtherIdInfo = otherIdInfo;
    entities.ExistingCsePerson.Populated = true;
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
    /// A value of ApSex.
    /// </summary>
    [JsonPropertyName("apSex")]
    public CsePersonsWorkSet ApSex
    {
      get => apSex ??= new();
      set => apSex = value;
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

    /// <summary>
    /// A value of FromInterstateCase.
    /// </summary>
    [JsonPropertyName("fromInterstateCase")]
    public InterstateCase FromInterstateCase
    {
      get => fromInterstateCase ??= new();
      set => fromInterstateCase = value;
    }

    /// <summary>
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    /// <summary>
    /// A value of FromPaReferral.
    /// </summary>
    [JsonPropertyName("fromPaReferral")]
    public PaReferral FromPaReferral
    {
      get => fromPaReferral ??= new();
      set => fromPaReferral = value;
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
    /// A value of FromInrdInformationRequest.
    /// </summary>
    [JsonPropertyName("fromInrdInformationRequest")]
    public InformationRequest FromInrdInformationRequest
    {
      get => fromInrdInformationRequest ??= new();
      set => fromInrdInformationRequest = value;
    }

    /// <summary>
    /// A value of FromInrdCommon.
    /// </summary>
    [JsonPropertyName("fromInrdCommon")]
    public Common FromInrdCommon
    {
      get => fromInrdCommon ??= new();
      set => fromInrdCommon = value;
    }

    private CsePersonsWorkSet apSex;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CaseRole caseRole;
    private InterstateCase fromInterstateCase;
    private Common fromIapi;
    private PaReferral fromPaReferral;
    private Common fromPar1;
    private InformationRequest fromInrdInformationRequest;
    private Common fromInrdCommon;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    private CaseRole chCaseRole;
    private CaseRole apCaseRole;
    private CaseRole arCaseRole;
    private CsePerson apCsePerson;
    private CsePerson chCsePerson;
    private CsePerson arCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Org.
    /// </summary>
    [JsonPropertyName("org")]
    public CsePerson Org
    {
      get => org ??= new();
      set => org = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CsePerson org;
    private CsePerson csePerson;
    private DateWorkArea dateWorkArea;
    private GoodCause goodCause;
    private DateWorkArea max;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of ExistingCh.
    /// </summary>
    [JsonPropertyName("existingCh")]
    public CaseRole ExistingCh
    {
      get => existingCh ??= new();
      set => existingCh = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CaseRole ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of ExistingAr.
    /// </summary>
    [JsonPropertyName("existingAr")]
    public CaseRole ExistingAr
    {
      get => existingAr ??= new();
      set => existingAr = value;
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

    /// <summary>
    /// A value of ExistingInterstateCase.
    /// </summary>
    [JsonPropertyName("existingInterstateCase")]
    public InterstateCase ExistingInterstateCase
    {
      get => existingInterstateCase ??= new();
      set => existingInterstateCase = value;
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
    /// A value of ExistingInformationRequest.
    /// </summary>
    [JsonPropertyName("existingInformationRequest")]
    public InformationRequest ExistingInformationRequest
    {
      get => existingInformationRequest ??= new();
      set => existingInformationRequest = value;
    }

    private Case1 case1;
    private GoodCause goodCause;
    private CaseRole existingCh;
    private CaseRole existingAp;
    private CaseRole existingAr;
    private CsePerson existingCsePerson;
    private InterstateCase existingInterstateCase;
    private PaReferral existingPaReferral;
    private PaReferralParticipant existingPaReferralParticipant;
    private InformationRequest existingInformationRequest;
  }
#endregion
}
