// Program: SI_CREATE_PA_REFERRAL, ID: 371789370, model: 746.
// Short name: SWE01144
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CREATE_PA_REFERRAL.
/// </summary>
[Serializable]
public partial class SiCreatePaReferral: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_PA_REFERRAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreatePaReferral(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreatePaReferral.
  /// </summary>
  public SiCreatePaReferral(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // Date      Developer	   Description
    // 06/01/96  J Howard	   Initial development
    // 01/07/97  G. Lofton - MTW  Add event logic
    // 04/17/97  R. Grey   - MTW  Set Initiating State Code, CSE Person
    // 			   Exclude 'Change' Type
    // 09/18/97  JeHoward         Pass AR Person Number to Create
    //                            Infrastructure
    // 01/06/98  J. Rookard - MTW Comment out code creating Infrastructure.
    // 01/16/98  J. Rookard - MTW Remove commenting out of Infra code, event 
    // will be raised per T.Barker-CSE instructions.
    // ************************************************************
    // ***************************************************************
    // 8/9/99  C. Ott  Problem report # 69594, If the PI (AR) Person number is 
    // the same as the Person number of the child, do not create AR Referral
    // Participant.  If the Person numbers are different, create the AR Referral
    // Participant.
    // **************************************************************
    try
    {
      CreatePaReferral();
      export.PaReferral.Assign(entities.PaReferral);

      try
      {
        CreatePaReferralParticipant();

        // The following event logic is commented out due to the following:
        // 1.  Receipt of a new or reopen PA Referral does not generate alerts 
        // or monitored activities.
        // 2.  Historical information regarding a given PA Referral is available
        // in the the PA Referral area of the application.
        // 3.  The Infrastructure record created can only be viewed on the HIST 
        // screen if the appropriate CSE Person Number is entered, with no other
        // filtering agents.
        // 4.  The Infrastructure record created does not cause a Case Unit 
        // Lifecycle transformation.
        // As of today, Jan. 6, 1998, we have determined that generation of the 
        // subject Infrastructure occurrence does not provide any specific value
        // to the application.  If it is determined in the future that this
        // event does have value, it can be reactivated by simply moving the
        // code outside of the "If Command is equal to "COMMENT" statement, and
        // removing the "If Command is equal to "COMMENT" statement.
        // Jack Rookard 1/6/98
        // Per Tricia Barker instructions, the following event logic is now "
        // uncommented", and the PA Referral received event WILL be raised.  J.
        // Rookard 1/16/98
        // ************************************************
        // *  Insert Event for PA Referral received
        // *
        // *  Modified to raise Event only for New or Reopen type Referrals
        // *  RCG - 4/17/97
        // ************************************************
        if (Equal(import.PaReferral.Type1, "Change") || Equal
          (import.PaReferral.Type1, "CHANGE"))
        {
          return;
        }
        else
        {
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 5;
          local.Infrastructure.ReasonCode = "BPAREFRL";
          local.Infrastructure.BusinessObjectCd = "PAR";
          local.Infrastructure.InitiatingStateCode = "KS";
          local.Infrastructure.DenormTimestamp =
            entities.PaReferral.CreatedTimestamp;
          local.Infrastructure.DenormNumeric12 =
            StringToNumber(entities.PaReferral.Number);
          local.Infrastructure.DenormText12 = entities.PaReferral.Type1;
          local.Infrastructure.UserId = "PAR1";
          local.Infrastructure.CreatedBy = global.UserId;
          local.Infrastructure.CreatedTimestamp = Now();
          local.Infrastructure.LastUpdatedBy = "";
          local.Infrastructure.ReferenceDate = import.PaReferral.ReceivedDate;

          // ***************************************************************
          // 8/9/99  C. Ott  Problem report # 69594, If the PI (AR) Person 
          // number is the same as the Person number of the child, do not create
          // AR Referral Participant.  If the Person numbers are different,
          // create the AR Referral Participant.
          // **************************************************************
          if (AsChar(import.SuppressArRelationship.Flag) == 'Y')
          {
          }
          else
          {
            local.Infrastructure.CsePersonNumber = import.Ar.PersonNumber ?? "";
          }

          local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (ReadInfrastructure())
            {
              AssociateInfrastructure();
            }
            else
            {
              ExitState = "INFRASTRUCTURE_NF";
            }
          }
        }

        // ***	End Insert Event for PA Referral received	***
        if (Equal(global.Command, "COMMENT"))
        {
        }
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PA_REFERRAL_PARTICIPANT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PA_REFERRAL_PARTICIPANT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PA_REFERRAL_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PA_REFERRAL_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void AssociateInfrastructure()
  {
    var pafNo = entities.PaReferral.Number;
    var pafType = entities.PaReferral.Type1;
    var pafTstamp = entities.PaReferral.CreatedTimestamp;

    entities.Infrastructure.Populated = false;
    Update("AssociateInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "pafNo", pafNo);
        db.SetNullableString(command, "pafType", pafType);
        db.SetNullableDateTime(command, "pafTstamp", pafTstamp);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.PafNo = pafNo;
    entities.Infrastructure.PafType = pafType;
    entities.Infrastructure.PafTstamp = pafTstamp;
    entities.Infrastructure.Populated = true;
  }

  private void CreatePaReferral()
  {
    var number = import.PaReferral.Number;
    var receivedDate = import.PaReferral.ReceivedDate;
    var assignDeactivateInd = import.PaReferral.AssignDeactivateInd ?? "";
    var assignDeactivateDate = import.PaReferral.AssignDeactivateDate;
    var caseNumber = import.PaReferral.CaseNumber ?? "";
    var type1 = import.PaReferral.Type1;
    var medicalPaymentDueDate = import.PaReferral.MedicalPaymentDueDate;
    var medicalAmt = import.PaReferral.MedicalAmt.GetValueOrDefault();
    var medicalFreq = import.PaReferral.MedicalFreq ?? "";
    var medicalLastPayment =
      import.PaReferral.MedicalLastPayment.GetValueOrDefault();
    var medicalLastPaymentDate = import.PaReferral.MedicalLastPaymentDate;
    var medicalOrderEffectiveDate = import.PaReferral.MedicalOrderEffectiveDate;
    var medicalOrderState = import.PaReferral.MedicalOrderState ?? "";
    var medicalOrderPlace = import.PaReferral.MedicalOrderPlace ?? "";
    var medicalArrearage =
      import.PaReferral.MedicalArrearage.GetValueOrDefault();
    var medicalPaidTo = import.PaReferral.MedicalPaidTo ?? "";
    var medicalPaymentType = import.PaReferral.MedicalPaymentType ?? "";
    var medicalInsuranceCo = import.PaReferral.MedicalInsuranceCo ?? "";
    var medicalPolicyNumber = import.PaReferral.MedicalPolicyNumber ?? "";
    var medicalOrderNumber = import.PaReferral.MedicalOrderNumber ?? "";
    var medicalOrderInd = import.PaReferral.MedicalOrderInd ?? "";
    var assignmentDate = import.PaReferral.AssignmentDate;
    var cseRegion = import.PaReferral.CseRegion ?? "";
    var cseReferralRecDate = import.PaReferral.CseReferralRecDate;
    var arRetainedInd = import.PaReferral.ArRetainedInd ?? "";
    var pgmCode = import.PaReferral.PgmCode ?? "";
    var caseWorker = import.PaReferral.CaseWorker ?? "";
    var paymentMadeTo = import.PaReferral.PaymentMadeTo ?? "";
    var csArrearageAmt = import.PaReferral.CsArrearageAmt.GetValueOrDefault();
    var csLastPaymentAmt =
      import.PaReferral.CsLastPaymentAmt.GetValueOrDefault();
    var csPaymentAmount = import.PaReferral.CsPaymentAmount.GetValueOrDefault();
    var lastPaymentDate = import.PaReferral.LastPaymentDate;
    var goodCauseCode = import.PaReferral.GoodCauseCode ?? "";
    var goodCauseDate = import.PaReferral.GoodCauseDate;
    var orderEffectiveDate = import.PaReferral.OrderEffectiveDate;
    var paymentDueDate = import.PaReferral.PaymentDueDate;
    var supportOrderId = import.PaReferral.SupportOrderId ?? "";
    var lastApContactDate = import.PaReferral.LastApContactDate;
    var voluntarySupportInd = import.PaReferral.VoluntarySupportInd ?? "";
    var apEmployerName = import.PaReferral.ApEmployerName ?? "";
    var fcNextJuvenileCtDt = import.PaReferral.FcNextJuvenileCtDt;
    var fcOrderEstBy = import.PaReferral.FcOrderEstBy ?? "";
    var fcJuvenileCourtOrder = import.PaReferral.FcJuvenileCourtOrder ?? "";
    var fcJuvenileOffenderInd = import.PaReferral.FcJuvenileOffenderInd ?? "";
    var fcCincInd = import.PaReferral.FcCincInd ?? "";
    var fcPlacementDate = import.PaReferral.FcPlacementDate;
    var fcSrsPayee = import.PaReferral.FcSrsPayee ?? "";
    var fcCostOfCareFreq = import.PaReferral.FcCostOfCareFreq ?? "";
    var fcCostOfCare = import.PaReferral.FcCostOfCare.GetValueOrDefault();
    var fcAdoptionDisruptionInd = import.PaReferral.FcAdoptionDisruptionInd ?? ""
      ;
    var fcPlacementType = import.PaReferral.FcPlacementType ?? "";
    var fcPreviousPa = import.PaReferral.FcPreviousPa ?? "";
    var fcDateOfInitialCustody = import.PaReferral.FcDateOfInitialCustody;
    var fcRightsSevered = import.PaReferral.FcRightsSevered ?? "";
    var fcIvECaseNumber = import.PaReferral.FcIvECaseNumber ?? "";
    var fcPlacementName = import.PaReferral.FcPlacementName ?? "";
    var fcSourceOfFunding = import.PaReferral.FcSourceOfFunding ?? "";
    var fcOtherBenefitInd = import.PaReferral.FcOtherBenefitInd ?? "";
    var fcZebInd = import.PaReferral.FcZebInd ?? "";
    var fcVaInd = import.PaReferral.FcVaInd ?? "";
    var fcSsi = import.PaReferral.FcSsi ?? "";
    var fcSsa = import.PaReferral.FcSsa ?? "";
    var fcWardsAccount = import.PaReferral.FcWardsAccount ?? "";
    var fcCountyChildRemovedFrom =
      import.PaReferral.FcCountyChildRemovedFrom ?? "";
    var fcApNotified = import.PaReferral.FcApNotified ?? "";
    var lastUpdatedBy = import.PaReferral.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.PaReferral.LastUpdatedTimestamp;
    var createdBy = import.PaReferral.CreatedBy ?? "";
    var createdTimestamp = import.PaReferral.CreatedTimestamp;
    var ksCounty = import.PaReferral.KsCounty ?? "";
    var note = import.PaReferral.Note ?? "";
    var apEmployerPhone = import.PaReferral.ApEmployerPhone.GetValueOrDefault();
    var supportOrderFreq = import.PaReferral.SupportOrderFreq ?? "";
    var csOrderPlace = import.PaReferral.CsOrderPlace ?? "";
    var csOrderState = import.PaReferral.CsOrderState ?? "";
    var csFreq = import.PaReferral.CsFreq ?? "";
    var from = import.PaReferral.From ?? "";
    var apPhoneNumber = import.PaReferral.ApPhoneNumber.GetValueOrDefault();
    var apAreaCode = import.PaReferral.ApAreaCode.GetValueOrDefault();
    var ccStartDate = import.PaReferral.CcStartDate;
    var arEmployerName = import.PaReferral.ArEmployerName ?? "";
    var cseInvolvementInd = import.PaReferral.CseInvolvementInd ?? "";

    entities.PaReferral.Populated = false;
    Update("CreatePaReferral",
      (db, command) =>
      {
        db.SetString(command, "numb", number);
        db.SetNullableDate(command, "receivedDate", receivedDate);
        db.SetNullableString(command, "assignDeactInd", assignDeactivateInd);
        db.SetNullableDate(command, "assignDeactDate", assignDeactivateDate);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetString(command, "type", type1);
        db.SetNullableDate(command, "medPymntDueDt", medicalPaymentDueDate);
        db.SetNullableDecimal(command, "medAmt", medicalAmt);
        db.SetNullableString(command, "medFreq", medicalFreq);
        db.SetNullableDecimal(command, "medicalLastPymt", medicalLastPayment);
        db.SetNullableDate(command, "medLastPymtDt", medicalLastPaymentDate);
        db.SetNullableDate(command, "medOrderEffDt", medicalOrderEffectiveDate);
        db.SetNullableString(command, "medOrderState", medicalOrderState);
        db.SetNullableString(command, "medOrderPlace", medicalOrderPlace);
        db.SetNullableDecimal(command, "medArrearage", medicalArrearage);
        db.SetNullableString(command, "medPaidTo", medicalPaidTo);
        db.SetNullableString(command, "medPaymentType", medicalPaymentType);
        db.SetNullableString(command, "medInsuranceCo", medicalInsuranceCo);
        db.SetNullableString(command, "medPolicyNbr", medicalPolicyNumber);
        db.SetNullableString(command, "medOrderNbr", medicalOrderNumber);
        db.SetNullableString(command, "medOrderInd", medicalOrderInd);
        db.SetNullableDate(command, "assignmentDate", assignmentDate);
        db.SetNullableString(command, "cseRegion", cseRegion);
        db.SetNullableDate(command, "cseRefRecDt", cseReferralRecDate);
        db.SetNullableString(command, "arRetainedInd", arRetainedInd);
        db.SetNullableString(command, "pgmCode", pgmCode);
        db.SetNullableString(command, "caseWorker", caseWorker);
        db.SetNullableString(command, "paymentMadeTo", paymentMadeTo);
        db.SetNullableDecimal(command, "arrearageAmt", csArrearageAmt);
        db.SetNullableDecimal(command, "lastPayAmt", csLastPaymentAmt);
        db.SetNullableDecimal(command, "paymentAmount", csPaymentAmount);
        db.SetNullableDate(command, "lastPaymentDate", lastPaymentDate);
        db.SetNullableString(command, "goodCauseCode", goodCauseCode);
        db.SetNullableDate(command, "goodCauseDate", goodCauseDate);
        db.SetNullableDate(command, "orderEffDate", orderEffectiveDate);
        db.SetNullableDate(command, "paymentDueDate", paymentDueDate);
        db.SetNullableString(command, "supportOrderId", supportOrderId);
        db.SetNullableDate(command, "lastApCtcDate", lastApContactDate);
        db.SetNullableString(command, "volSupportInd", voluntarySupportInd);
        db.SetNullableString(command, "apEmployerName", apEmployerName);
        db.SetNullableDate(command, "fcNextJvCtDt", fcNextJuvenileCtDt);
        db.SetNullableString(command, "fcOrderEstBy", fcOrderEstBy);
        db.SetNullableString(command, "fcJvCourtOrder", fcJuvenileCourtOrder);
        db.SetNullableString(command, "fcJvOffendInd", fcJuvenileOffenderInd);
        db.SetNullableString(command, "fcCincInd", fcCincInd);
        db.SetNullableDate(command, "fcPlacementDate", fcPlacementDate);
        db.SetNullableString(command, "fcSrsPayee", fcSrsPayee);
        db.SetNullableString(command, "fcCareCostFreq", fcCostOfCareFreq);
        db.SetNullableDecimal(command, "fcCostOfCare", fcCostOfCare);
        db.
          SetNullableString(command, "fcAdoptDisrupt", fcAdoptionDisruptionInd);
          
        db.SetNullableString(command, "fcPlacementType", fcPlacementType);
        db.SetNullableString(command, "fcPreviousPa", fcPreviousPa);
        db.SetNullableDate(command, "fcInitCustodyDt", fcDateOfInitialCustody);
        db.SetNullableString(command, "fcRightsSevered", fcRightsSevered);
        db.SetNullableString(command, "fcIvECaseNo", fcIvECaseNumber);
        db.SetNullableString(command, "fcPlacementName", fcPlacementName);
        db.SetNullableString(command, "fcSrceOfFunding", fcSourceOfFunding);
        db.SetNullableString(command, "fcOthBenInd", fcOtherBenefitInd);
        db.SetNullableString(command, "fcZebInd", fcZebInd);
        db.SetNullableString(command, "fcVaInd", fcVaInd);
        db.SetNullableString(command, "fcSsi", fcSsi);
        db.SetNullableString(command, "fcSsa", fcSsa);
        db.SetNullableString(command, "fcWardsAccount", fcWardsAccount);
        db.
          SetNullableString(command, "fcCtyChRmvdFrm", fcCountyChildRemovedFrom);
          
        db.SetNullableString(command, "fcApNotified", fcApNotified);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "ksCounty", ksCounty);
        db.SetNullableString(command, "note", note);
        db.SetNullableInt64(command, "apEmployerPhone", apEmployerPhone);
        db.SetNullableString(command, "supportOrderFreq", supportOrderFreq);
        db.SetNullableString(command, "csOrderPlace", csOrderPlace);
        db.SetNullableString(command, "csOrderState", csOrderState);
        db.SetNullableString(command, "csFreq", csFreq);
        db.SetNullableString(command, "referralFrom", from);
        db.SetNullableInt32(command, "apPhoneNumber", apPhoneNumber);
        db.SetNullableInt32(command, "apAreaCode", apAreaCode);
        db.SetNullableDate(command, "ccStartDate", ccStartDate);
        db.SetNullableString(command, "arEmployerName", arEmployerName);
        db.SetNullableString(command, "cseInvlvmntInd", cseInvolvementInd);
      });

    entities.PaReferral.Number = number;
    entities.PaReferral.ReceivedDate = receivedDate;
    entities.PaReferral.AssignDeactivateInd = assignDeactivateInd;
    entities.PaReferral.AssignDeactivateDate = assignDeactivateDate;
    entities.PaReferral.CaseNumber = caseNumber;
    entities.PaReferral.Type1 = type1;
    entities.PaReferral.MedicalPaymentDueDate = medicalPaymentDueDate;
    entities.PaReferral.MedicalAmt = medicalAmt;
    entities.PaReferral.MedicalFreq = medicalFreq;
    entities.PaReferral.MedicalLastPayment = medicalLastPayment;
    entities.PaReferral.MedicalLastPaymentDate = medicalLastPaymentDate;
    entities.PaReferral.MedicalOrderEffectiveDate = medicalOrderEffectiveDate;
    entities.PaReferral.MedicalOrderState = medicalOrderState;
    entities.PaReferral.MedicalOrderPlace = medicalOrderPlace;
    entities.PaReferral.MedicalArrearage = medicalArrearage;
    entities.PaReferral.MedicalPaidTo = medicalPaidTo;
    entities.PaReferral.MedicalPaymentType = medicalPaymentType;
    entities.PaReferral.MedicalInsuranceCo = medicalInsuranceCo;
    entities.PaReferral.MedicalPolicyNumber = medicalPolicyNumber;
    entities.PaReferral.MedicalOrderNumber = medicalOrderNumber;
    entities.PaReferral.MedicalOrderInd = medicalOrderInd;
    entities.PaReferral.AssignmentDate = assignmentDate;
    entities.PaReferral.CseRegion = cseRegion;
    entities.PaReferral.CseReferralRecDate = cseReferralRecDate;
    entities.PaReferral.ArRetainedInd = arRetainedInd;
    entities.PaReferral.PgmCode = pgmCode;
    entities.PaReferral.CaseWorker = caseWorker;
    entities.PaReferral.PaymentMadeTo = paymentMadeTo;
    entities.PaReferral.CsArrearageAmt = csArrearageAmt;
    entities.PaReferral.CsLastPaymentAmt = csLastPaymentAmt;
    entities.PaReferral.CsPaymentAmount = csPaymentAmount;
    entities.PaReferral.LastPaymentDate = lastPaymentDate;
    entities.PaReferral.GoodCauseCode = goodCauseCode;
    entities.PaReferral.GoodCauseDate = goodCauseDate;
    entities.PaReferral.OrderEffectiveDate = orderEffectiveDate;
    entities.PaReferral.PaymentDueDate = paymentDueDate;
    entities.PaReferral.SupportOrderId = supportOrderId;
    entities.PaReferral.LastApContactDate = lastApContactDate;
    entities.PaReferral.VoluntarySupportInd = voluntarySupportInd;
    entities.PaReferral.ApEmployerName = apEmployerName;
    entities.PaReferral.FcNextJuvenileCtDt = fcNextJuvenileCtDt;
    entities.PaReferral.FcOrderEstBy = fcOrderEstBy;
    entities.PaReferral.FcJuvenileCourtOrder = fcJuvenileCourtOrder;
    entities.PaReferral.FcJuvenileOffenderInd = fcJuvenileOffenderInd;
    entities.PaReferral.FcCincInd = fcCincInd;
    entities.PaReferral.FcPlacementDate = fcPlacementDate;
    entities.PaReferral.FcSrsPayee = fcSrsPayee;
    entities.PaReferral.FcCostOfCareFreq = fcCostOfCareFreq;
    entities.PaReferral.FcCostOfCare = fcCostOfCare;
    entities.PaReferral.FcAdoptionDisruptionInd = fcAdoptionDisruptionInd;
    entities.PaReferral.FcPlacementType = fcPlacementType;
    entities.PaReferral.FcPreviousPa = fcPreviousPa;
    entities.PaReferral.FcDateOfInitialCustody = fcDateOfInitialCustody;
    entities.PaReferral.FcRightsSevered = fcRightsSevered;
    entities.PaReferral.FcIvECaseNumber = fcIvECaseNumber;
    entities.PaReferral.FcPlacementName = fcPlacementName;
    entities.PaReferral.FcSourceOfFunding = fcSourceOfFunding;
    entities.PaReferral.FcOtherBenefitInd = fcOtherBenefitInd;
    entities.PaReferral.FcZebInd = fcZebInd;
    entities.PaReferral.FcVaInd = fcVaInd;
    entities.PaReferral.FcSsi = fcSsi;
    entities.PaReferral.FcSsa = fcSsa;
    entities.PaReferral.FcWardsAccount = fcWardsAccount;
    entities.PaReferral.FcCountyChildRemovedFrom = fcCountyChildRemovedFrom;
    entities.PaReferral.FcApNotified = fcApNotified;
    entities.PaReferral.LastUpdatedBy = lastUpdatedBy;
    entities.PaReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PaReferral.CreatedBy = createdBy;
    entities.PaReferral.CreatedTimestamp = createdTimestamp;
    entities.PaReferral.KsCounty = ksCounty;
    entities.PaReferral.Note = note;
    entities.PaReferral.ApEmployerPhone = apEmployerPhone;
    entities.PaReferral.SupportOrderFreq = supportOrderFreq;
    entities.PaReferral.CsOrderPlace = csOrderPlace;
    entities.PaReferral.CsOrderState = csOrderState;
    entities.PaReferral.CsFreq = csFreq;
    entities.PaReferral.From = from;
    entities.PaReferral.ApPhoneNumber = apPhoneNumber;
    entities.PaReferral.ApAreaCode = apAreaCode;
    entities.PaReferral.CcStartDate = ccStartDate;
    entities.PaReferral.ArEmployerName = arEmployerName;
    entities.PaReferral.CseInvolvementInd = cseInvolvementInd;
    entities.PaReferral.Populated = true;
  }

  private void CreatePaReferralParticipant()
  {
    var identifier = import.PaReferralParticipant.Identifier;
    var createdTimestamp = import.PaReferralParticipant.CreatedTimestamp;
    var absenceCode = import.PaReferralParticipant.AbsenceCode ?? "";
    var relationship = import.PaReferralParticipant.Relationship ?? "";
    var sex = import.PaReferralParticipant.Sex ?? "";
    var dob = import.PaReferralParticipant.Dob;
    var lastName = import.PaReferralParticipant.LastName ?? "";
    var firstName = import.PaReferralParticipant.FirstName ?? "";
    var mi = import.PaReferralParticipant.Mi ?? "";
    var ssn = import.PaReferralParticipant.Ssn ?? "";
    var personNumber = import.PaReferralParticipant.PersonNumber ?? "";
    var insurInd = import.PaReferralParticipant.InsurInd ?? "";
    var patEstInd = import.PaReferralParticipant.PatEstInd ?? "";
    var beneInd = import.PaReferralParticipant.BeneInd ?? "";
    var createdBy = import.PaReferralParticipant.CreatedBy ?? "";
    var lastUpdatedBy = import.PaReferralParticipant.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      import.PaReferralParticipant.LastUpdatedTimestamp;
    var preNumber = entities.PaReferral.Number;
    var goodCauseStatus = import.PaReferralParticipant.GoodCauseStatus ?? "";
    var pafType = entities.PaReferral.Type1;
    var pafTstamp = entities.PaReferral.CreatedTimestamp;
    var role = import.PaReferralParticipant.Role ?? "";

    entities.PaReferralParticipant.Populated = false;
    Update("CreatePaReferralParticipant",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "absenceCode", absenceCode);
        db.SetNullableString(command, "relationship", relationship);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableDate(command, "dob", dob);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleInitial", mi);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "personNum", personNumber);
        db.SetNullableString(command, "insurInd", insurInd);
        db.SetNullableString(command, "patEstInd", patEstInd);
        db.SetNullableString(command, "beneInd", beneInd);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "preNumber", preNumber);
        db.SetNullableString(command, "goodCauseStatus", goodCauseStatus);
        db.SetString(command, "pafType", pafType);
        db.SetDateTime(command, "pafTstamp", pafTstamp);
        db.SetNullableString(command, "role", role);
      });

    entities.PaReferralParticipant.Identifier = identifier;
    entities.PaReferralParticipant.CreatedTimestamp = createdTimestamp;
    entities.PaReferralParticipant.AbsenceCode = absenceCode;
    entities.PaReferralParticipant.Relationship = relationship;
    entities.PaReferralParticipant.Sex = sex;
    entities.PaReferralParticipant.Dob = dob;
    entities.PaReferralParticipant.LastName = lastName;
    entities.PaReferralParticipant.FirstName = firstName;
    entities.PaReferralParticipant.Mi = mi;
    entities.PaReferralParticipant.Ssn = ssn;
    entities.PaReferralParticipant.PersonNumber = personNumber;
    entities.PaReferralParticipant.InsurInd = insurInd;
    entities.PaReferralParticipant.PatEstInd = patEstInd;
    entities.PaReferralParticipant.BeneInd = beneInd;
    entities.PaReferralParticipant.CreatedBy = createdBy;
    entities.PaReferralParticipant.LastUpdatedBy = lastUpdatedBy;
    entities.PaReferralParticipant.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PaReferralParticipant.PreNumber = preNumber;
    entities.PaReferralParticipant.GoodCauseStatus = goodCauseStatus;
    entities.PaReferralParticipant.PafType = pafType;
    entities.PaReferralParticipant.PafTstamp = pafTstamp;
    entities.PaReferralParticipant.Role = role;
    entities.PaReferralParticipant.Populated = true;
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          local.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.PafNo = db.GetNullableString(reader, 1);
        entities.Infrastructure.PafType = db.GetNullableString(reader, 2);
        entities.Infrastructure.PafTstamp = db.GetNullableDateTime(reader, 3);
        entities.Infrastructure.Populated = true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public PaReferralParticipant Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
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
    /// A value of SuppressArRelationship.
    /// </summary>
    [JsonPropertyName("suppressArRelationship")]
    public Common SuppressArRelationship
    {
      get => suppressArRelationship ??= new();
      set => suppressArRelationship = value;
    }

    private PaReferralParticipant ar;
    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
    private Common suppressArRelationship;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    private PaReferral paReferral;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
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

    private Infrastructure infrastructure;
    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
  }
#endregion
}
