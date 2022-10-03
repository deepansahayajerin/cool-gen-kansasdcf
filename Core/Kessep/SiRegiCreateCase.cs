// Program: SI_REGI_CREATE_CASE, ID: 371727790, model: 746.
// Short name: SWE01120
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_REGI_CREATE_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This pad creates a case
/// </para>
/// </summary>
[Serializable]
public partial class SiRegiCreateCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CREATE_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCreateCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCreateCase.
  /// </summary>
  public SiRegiCreateCase(IContext context, Import import, Export export):
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
    // 02/23/95  Helen Sharland	Initial Development
    // 03/08/97  G. Lofton - MTW	Add association to
    // 				Information Request
    // 05/20/97  Sid Chowdhary		Clean up and check for business level 
    // consistency.
    // ------------------------------------------------------------
    // 06/21/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 08/09/00  W. Campbell           Added new attribute
    //                                 
    // Application Processed Ind to
    //                                 
    // Entity Information Request with
    //                                 
    // new logic to update it when a
    //                                 
    // new Case is created and
    // associated
    //                                 
    // with an existing Information
    // Request.
    //                                 
    // Work done on PR#100532-B.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;

    // ------------------------------------------------------------
    // 08/09/00  W. Campbell - Added new local
    // attribute for timestamp with associated logic
    // to set it to current timestamp and use it in later
    // set statements. Work done on PR#100532-B.
    // ------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate();

    // 06/21/99 M.L Start
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadOffice())
    {
      local.Case1.OfficeIdentifier = entities.ExistingOffice.SystemGeneratedId;
    }
    else
    {
      ExitState = "OFFICE_NF";

      return;
    }

    // 21/06/99 M.L End
    if (AsChar(import.FromIapi.Flag) == 'Y')
    {
      // 21/06/99 M.L Start
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadInterstateCase())
      {
        MoveInterstateCase(entities.ExistingInterstateCase,
          export.InterstateCase);
        local.Case1.IcTransSerialNumber =
          entities.ExistingInterstateCase.TransSerialNumber;
        local.Case1.IcTransactionDate =
          entities.ExistingInterstateCase.TransactionDate;
        local.Case1.InterstateCaseId =
          entities.ExistingInterstateCase.InterstateCaseId;
        local.InformationRequest.Type1 = "R";

        if (Equal(entities.ExistingInterstateCase.FunctionalTypeCode, "LO2"))
        {
          local.Case1.LocateInd = "Y";
        }
        else
        {
          local.Case1.FullServiceWithMedInd = "Y";
        }
      }
      else
      {
        ExitState = "INTERSTATE_CASE_NF";

        return;
      }

      // 21/06/99 M.L End
    }

    if (AsChar(import.FromPar1.Flag) == 'Y')
    {
      // 21/06/99 M.L Start
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadPaReferral())
      {
        local.Case1.PaRefCreatedTimestamp =
          entities.ExistingPaReferral.CreatedTimestamp;
        local.Case1.PaReferralNumber = entities.ExistingPaReferral.Number;
        local.Case1.PaReferralType = entities.ExistingPaReferral.Type1;
        local.Case1.FullServiceWithMedInd = "Y";
        local.InformationRequest.Type1 = "R";

        if (Equal(entities.ExistingPaReferral.PgmCode, "AF"))
        {
          local.Case1.AdcOpenDate = entities.ExistingPaReferral.AssignmentDate;
          local.Case1.AdcCloseDate = local.Max.Date;
        }
      }
      else
      {
        ExitState = "PA_REFERRAL_NF";

        return;
      }

      // 21/06/99 M.L End
    }

    if (AsChar(import.FromInrdCommon.Flag) == 'Y')
    {
      // 21/06/99 M.L Start
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadInformationRequest())
      {
        local.Case1.InformationRequestNumber =
          entities.ExistingInformationRequest.Number;
        local.InformationRequest.Type1 =
          entities.ExistingInformationRequest.Type1;

        switch(TrimEnd(entities.ExistingInformationRequest.ServiceCode))
        {
          case "LO":
            local.Case1.LocateInd = "Y";

            break;
          case "WI":
            local.Case1.FullServiceWithMedInd = "Y";

            break;
          case "WO":
            local.Case1.FullServiceWithoutMedInd = "Y";

            break;
          default:
            break;
        }
      }
      else
      {
        ExitState = "INQUIRY_NF";

        return;
      }

      // 21/06/99 M.L End
    }

    // Default the Service Type to FULL, if none has been set.
    if (IsEmpty(local.Case1.FullServiceWithMedInd) && IsEmpty
      (local.Case1.FullServiceWithoutMedInd) && IsEmpty(local.Case1.LocateInd))
    {
      local.Case1.FullServiceWithMedInd = "Y";
    }

    // ---------------------------------------------
    // Call the action block which produces the next
    // CASE number available for use.
    // ---------------------------------------------
    local.CaseNumber.Identifier = "CASE";
    UseAccessControlTable();
    local.Case1.Number = NumberToString(local.CaseNumber.LastUsedNumber, 10);

    try
    {
      CreateCase();
      export.Case1.Number = entities.ExistingCase.Number;
      AssociateCase1();

      if (AsChar(import.FromPar1.Flag) == 'Y')
      {
        AssociateCase2();
      }

      if (AsChar(import.FromInrdCommon.Flag) == 'Y')
      {
        AssociateCase3();

        // ------------------------------------------------------------
        // 08/09/00  W. Campbell - Added new attribute
        // Application Processed Ind to
        // Entity Information Request with
        // new logic to update it when a
        // new Case is created and associated
        // with an existing Information Request.
        // Work done on PR#100532-B.
        // ------------------------------------------------------------
        try
        {
          UpdateInformationRequest();
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "INQUIRY_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "INQUIRY_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
    target.CaseType = source.CaseType;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.CaseNumber.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.CaseNumber.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void AssociateCase1()
  {
    var offGeneratedId = entities.ExistingOffice.SystemGeneratedId;

    entities.ExistingCase.Populated = false;
    Update("AssociateCase1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetString(command, "numb", entities.ExistingCase.Number);
      });

    entities.ExistingCase.OffGeneratedId = offGeneratedId;
    entities.ExistingCase.Populated = true;
  }

  private void AssociateCase2()
  {
    var casNumber = entities.ExistingCase.Number;

    entities.ExistingPaReferral.Populated = false;
    Update("AssociateCase2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetString(command, "numb", entities.ExistingPaReferral.Number);
        db.SetString(command, "type", entities.ExistingPaReferral.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          entities.ExistingPaReferral.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ExistingPaReferral.CasNumber = casNumber;
    entities.ExistingPaReferral.Populated = true;
  }

  private void AssociateCase3()
  {
    var fkCktCasenumb = entities.ExistingCase.Number;

    entities.ExistingInformationRequest.Populated = false;
    Update("AssociateCase3",
      (db, command) =>
      {
        db.SetString(command, "fkCktCasenumb", fkCktCasenumb);
        db.
          SetInt64(command, "numb", entities.ExistingInformationRequest.Number);
          
      });

    entities.ExistingInformationRequest.FkCktCasenumb = fkCktCasenumb;
    entities.ExistingInformationRequest.Populated = true;
  }

  private void CreateCase()
  {
    var fullServiceWithoutMedInd = local.Case1.FullServiceWithoutMedInd ?? "";
    var fullServiceWithMedInd = local.Case1.FullServiceWithMedInd ?? "";
    var locateInd = local.Case1.LocateInd ?? "";
    var number = local.Case1.Number;
    var informationRequestNumber =
      local.Case1.InformationRequestNumber.GetValueOrDefault();
    var applicantLastName = local.Case1.ApplicantLastName ?? "";
    var applicantFirstName = local.Case1.ApplicantFirstName ?? "";
    var applicantMiddleInitial = local.Case1.ApplicantMiddleInitial ?? "";
    var applicationSentDate = local.Case1.ApplicationSentDate;
    var applicationRequestDate = local.Case1.ApplicationRequestDate;
    var applicationReturnDate = local.Case1.ApplicationReturnDate;
    var deniedRequestDate = local.Case1.DeniedRequestDate;
    var deniedRequestCode = local.Case1.DeniedRequestCode ?? "";
    var deniedRequestReason = local.Case1.DeniedRequestReason ?? "";
    var status = "O";
    var validApplicationReceivedDate = local.Initialized.Date;
    var statusDate = local.Current.Date;
    var officeIdentifier = local.Case1.OfficeIdentifier.GetValueOrDefault();
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var icTransSerialNumber = local.Case1.IcTransSerialNumber;
    var icTransactionDate = local.Case1.IcTransactionDate;
    var paRefCreatedTimestamp = local.Case1.PaRefCreatedTimestamp;
    var paReferralNumber = local.Case1.PaReferralNumber;
    var paReferralType = local.Case1.PaReferralType;
    var interstateCaseId = local.Case1.InterstateCaseId ?? "";
    var adcOpenDate = local.Case1.AdcOpenDate;
    var adcCloseDate = local.Case1.AdcCloseDate;
    var note = Spaces(80);
    var enrollmentType = local.InformationRequest.Type1;

    CheckValid<Case1>("Potential", "");
    entities.ExistingCase.Populated = false;
    Update("CreateCase",
      (db, command) =>
      {
        db.
          SetNullableString(command, "fullSrvWoMedIn", fullServiceWithoutMedInd);
          
        db.SetNullableString(command, "managementArea", "");
        db.SetNullableString(command, "managementRegion", "");
        db.SetNullableString(command, "fullServWMedIn", fullServiceWithMedInd);
        db.SetNullableString(command, "locateInd", locateInd);
        db.SetNullableString(command, "closureReason", "");
        db.SetString(command, "numb", number);
        db.SetNullableInt64(command, "infoRequestNo", informationRequestNumber);
        db.SetNullableString(command, "stationId", "");
        db.SetNullableString(command, "applLastNm", applicantLastName);
        db.SetNullableString(command, "applFirstNm", applicantFirstName);
        db.SetNullableString(command, "applMi", applicantMiddleInitial);
        db.SetNullableDate(command, "applSentDt", applicationSentDate);
        db.SetNullableDate(command, "applRequestDt", applicationRequestDate);
        db.SetNullableDate(command, "applReturnDt", applicationReturnDate);
        db.SetNullableDate(command, "deniedRequestDt", deniedRequestDate);
        db.SetNullableString(command, "deniedRequestCd", deniedRequestCode);
        db.SetNullableString(command, "deniedRequestRsn", deniedRequestReason);
        db.SetNullableString(command, "status", status);
        db.SetNullableString(command, "ksFipsCode", "");
        db.SetNullableDate(
          command, "validApplRcvdDt", validApplicationReceivedDate);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "potential", "");
        db.SetNullableDate(command, "cseOpenDate", statusDate);
        db.SetNullableInt32(command, "officeIdentifier", officeIdentifier);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", lastUpdatedTimestamp);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "expedidedPatInd", "");
        db.SetNullableString(command, "paMedicalService", "");
        db.SetInt64(command, "icTransSerNo", icTransSerialNumber);
        db.SetDate(command, "icTransDate", icTransactionDate);
        db.SetDateTime(command, "paRefCreTstamp", paRefCreatedTimestamp);
        db.SetString(command, "paReferNo", paReferralNumber);
        db.SetString(command, "paReferType", paReferralType);
        db.SetNullableDate(
          command, "closureLetrDate", validApplicationReceivedDate);
        db.SetNullableString(command, "interstateCaseId", interstateCaseId);
        db.SetNullableDate(command, "adcOpenDate", adcOpenDate);
        db.SetNullableDate(command, "adcCloseDate", adcCloseDate);
        db.SetNullableString(command, "dupCaseIndicator", "");
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "noJurisdictionCd", "");
        db.SetNullableString(command, "enrollmentType", enrollmentType);
      });

    entities.ExistingCase.OffGeneratedId = null;
    entities.ExistingCase.FullServiceWithoutMedInd = fullServiceWithoutMedInd;
    entities.ExistingCase.ManagementArea = "";
    entities.ExistingCase.ManagementRegion = "";
    entities.ExistingCase.FullServiceWithMedInd = fullServiceWithMedInd;
    entities.ExistingCase.LocateInd = locateInd;
    entities.ExistingCase.ClosureReason = "";
    entities.ExistingCase.Number = number;
    entities.ExistingCase.InformationRequestNumber = informationRequestNumber;
    entities.ExistingCase.StationId = "";
    entities.ExistingCase.ApplicantLastName = applicantLastName;
    entities.ExistingCase.ApplicantFirstName = applicantFirstName;
    entities.ExistingCase.ApplicantMiddleInitial = applicantMiddleInitial;
    entities.ExistingCase.ApplicationSentDate = applicationSentDate;
    entities.ExistingCase.ApplicationRequestDate = applicationRequestDate;
    entities.ExistingCase.ApplicationReturnDate = applicationReturnDate;
    entities.ExistingCase.DeniedRequestDate = deniedRequestDate;
    entities.ExistingCase.DeniedRequestCode = deniedRequestCode;
    entities.ExistingCase.DeniedRequestReason = deniedRequestReason;
    entities.ExistingCase.Status = status;
    entities.ExistingCase.KsFipsCode = "";
    entities.ExistingCase.ValidApplicationReceivedDate =
      validApplicationReceivedDate;
    entities.ExistingCase.StatusDate = statusDate;
    entities.ExistingCase.Potential = "";
    entities.ExistingCase.CseOpenDate = statusDate;
    entities.ExistingCase.OfficeIdentifier = officeIdentifier;
    entities.ExistingCase.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCase.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCase.CreatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCase.CreatedBy = lastUpdatedBy;
    entities.ExistingCase.ExpeditedPaternityInd = "";
    entities.ExistingCase.PaMedicalService = "";
    entities.ExistingCase.IcTransSerialNumber = icTransSerialNumber;
    entities.ExistingCase.IcTransactionDate = icTransactionDate;
    entities.ExistingCase.PaRefCreatedTimestamp = paRefCreatedTimestamp;
    entities.ExistingCase.PaReferralNumber = paReferralNumber;
    entities.ExistingCase.PaReferralType = paReferralType;
    entities.ExistingCase.ClosureLetterDate = validApplicationReceivedDate;
    entities.ExistingCase.InterstateCaseId = interstateCaseId;
    entities.ExistingCase.AdcOpenDate = adcOpenDate;
    entities.ExistingCase.AdcCloseDate = adcCloseDate;
    entities.ExistingCase.Note = note;
    entities.ExistingCase.EnrollmentType = enrollmentType;
    entities.ExistingCase.Populated = true;
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
        entities.ExistingInformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 37);
        entities.ExistingInformationRequest.FkCktCasenumb =
          db.GetString(reader, 38);
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

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
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
        entities.ExistingPaReferral.CasNumber =
          db.GetNullableString(reader, 81);
        entities.ExistingPaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 82);
        entities.ExistingPaReferral.Populated = true;
      });
  }

  private void UpdateInformationRequest()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var applicationProcessedInd = "Y";

    entities.ExistingInformationRequest.Populated = false;
    Update("UpdateInformationRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "applProcInd", applicationProcessedInd);
        db.
          SetInt64(command, "numb", entities.ExistingInformationRequest.Number);
          
      });

    entities.ExistingInformationRequest.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingInformationRequest.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingInformationRequest.ApplicationProcessedInd =
      applicationProcessedInd;
    entities.ExistingInformationRequest.Populated = true;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of FromInterstateCase.
    /// </summary>
    [JsonPropertyName("fromInterstateCase")]
    public InterstateCase FromInterstateCase
    {
      get => fromInterstateCase ??= new();
      set => fromInterstateCase = value;
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
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
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

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    private Office office;
    private InformationRequest fromInrdInformationRequest;
    private InterstateCase fromInterstateCase;
    private PaReferral fromPaReferral;
    private Common fromIapi;
    private Common fromInrdCommon;
    private Common fromPar1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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

    private InterstateCase interstateCase;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseNumber.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    public ControlTable CaseNumber
    {
      get => caseNumber ??= new();
      set => caseNumber = value;
    }

    private InformationRequest informationRequest;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private DateWorkArea current;
    private Case1 case1;
    private ControlTable caseNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
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

    /// <summary>
    /// A value of ExistingInformationRequest.
    /// </summary>
    [JsonPropertyName("existingInformationRequest")]
    public InformationRequest ExistingInformationRequest
    {
      get => existingInformationRequest ??= new();
      set => existingInformationRequest = value;
    }

    private InterstateCase existingInterstateCase;
    private PaReferral existingPaReferral;
    private Office existingOffice;
    private Case1 existingCase;
    private InformationRequest existingInformationRequest;
  }
#endregion
}
