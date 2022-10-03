// Program: SI_INRD_CREATE_INFORMATION_REQ, ID: 371426555, model: 746.
// Short name: SWE01135
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INRD_CREATE_INFORMATION_REQ.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiInrdCreateInformationReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INRD_CREATE_INFORMATION_REQ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInrdCreateInformationReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInrdCreateInformationReq.
  /// </summary>
  public SiInrdCreateInformationReq(IContext context, Import import,
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date   Developer    Description
    // 7-10-95 Ken Evans    Initial Development
    // ---------------------------------------------
    // 08/07/00 W.Campbell             Added new attribute
    //                                 
    // to Import and Entity view and a
    //                                 
    // set stmt in the Create stmt for
    // new
    //                                 
    // attribute Application Processed
    // IND
    //                                 
    // which had been added to
    //                                 
    // entity type information_request.
    //                                 
    // Work done on PR# 100532.
    // ------------------------------------------------------------
    try
    {
      CreateInformationRequest();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INQUIRY_AE";

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

  private void CreateInformationRequest()
  {
    var number = import.InformationRequest.Number;
    var nonparentQuestionnaireSent =
      import.InformationRequest.NonparentQuestionnaireSent ?? "";
    var parentQuestionnaireSent =
      import.InformationRequest.ParentQuestionnaireSent ?? "";
    var paternityQuestionnaireSent =
      import.InformationRequest.PaternityQuestionnaireSent ?? "";
    var applicationSentIndicator =
      import.InformationRequest.ApplicationSentIndicator;
    var questionnaireTypeIndicator =
      import.InformationRequest.QuestionnaireTypeIndicator;
    var dateReceivedByCseComplete =
      import.InformationRequest.DateReceivedByCseComplete;
    var dateReceivedByCseIncomplete =
      import.InformationRequest.DateReceivedByCseIncomplete;
    var dateApplicationRequested =
      import.InformationRequest.DateApplicationRequested;
    var callerLastName = import.InformationRequest.CallerLastName ?? "";
    var callerFirstName = import.InformationRequest.CallerFirstName;
    var callerMiddleInitial = import.InformationRequest.CallerMiddleInitial;
    var inquirerNameSuffix = import.InformationRequest.InquirerNameSuffix ?? "";
    var applicantLastName = import.InformationRequest.ApplicantLastName ?? "";
    var applicantFirstName = import.InformationRequest.ApplicantFirstName ?? "";
    var applicantMiddleInitial =
      import.InformationRequest.ApplicantMiddleInitial ?? "";
    var applicantNameSuffix = import.InformationRequest.ApplicantNameSuffix;
    var applicantStreet1 = import.InformationRequest.ApplicantStreet1 ?? "";
    var applicantStreet2 = import.InformationRequest.ApplicantStreet2 ?? "";
    var applicantCity = import.InformationRequest.ApplicantCity ?? "";
    var applicantState = import.InformationRequest.ApplicantState ?? "";
    var applicantZip5 = import.InformationRequest.ApplicantZip5 ?? "";
    var applicantZip4 = import.InformationRequest.ApplicantZip4 ?? "";
    var applicantZip3 = import.InformationRequest.ApplicantZip3 ?? "";
    var applicantPhone =
      import.InformationRequest.ApplicantPhone.GetValueOrDefault();
    var dateApplicationSent = import.InformationRequest.DateApplicationSent;
    var type1 = import.InformationRequest.Type1;
    var serviceCode = import.InformationRequest.ServiceCode ?? "";
    var reasonIncomplete = import.InformationRequest.ReasonIncomplete ?? "";
    var note = import.InformationRequest.Note ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonDenied = import.InformationRequest.ReasonDenied ?? "";
    var dateDenied = import.InformationRequest.DateDenied;
    var applicantAreaCode =
      import.InformationRequest.ApplicantAreaCode.GetValueOrDefault();
    var applicationProcessedInd =
      import.InformationRequest.ApplicationProcessedInd ?? "";
    var reopenReasonType = import.InformationRequest.ReopenReasonType ?? "";
    var miscellaneousReason = import.InformationRequest.MiscellaneousReason ?? ""
      ;

    entities.InformationRequest.Populated = false;
    Update("CreateInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", number);
        db.SetNullableString(
          command, "nonparentQstSent", nonparentQuestionnaireSent);
        db.SetNullableString(command, "parentQstSent", parentQuestionnaireSent);
        db.SetNullableString(command, "patQstSent", paternityQuestionnaireSent);
        db.SetString(command, "applSentInd", applicationSentIndicator);
        db.SetString(command, "qstTypeInd", questionnaireTypeIndicator);
        db.
          SetNullableDate(command, "dtRcvByCseComp", dateReceivedByCseComplete);
          
        db.SetNullableDate(
          command, "dtRcvCseIncomp", dateReceivedByCseIncomplete);
        db.SetDate(command, "dtApplRequested", dateApplicationRequested);
        db.SetNullableString(command, "callerLastNm", callerLastName);
        db.SetString(command, "callerFirstName", callerFirstName);
        db.SetString(command, "callerMi", callerMiddleInitial);
        db.SetNullableString(command, "inquirerNmSfx", inquirerNameSuffix);
        db.SetNullableString(command, "applLastNm", applicantLastName);
        db.SetNullableString(command, "applFirstNm", applicantFirstName);
        db.SetNullableString(command, "applMi", applicantMiddleInitial);
        db.SetString(command, "applNmSfx", applicantNameSuffix);
        db.SetNullableString(command, "applStreet1", applicantStreet1);
        db.SetNullableString(command, "applStreet2", applicantStreet2);
        db.SetNullableString(command, "applCity", applicantCity);
        db.SetNullableString(command, "applState", applicantState);
        db.SetNullableString(command, "applicantZip5", applicantZip5);
        db.SetNullableString(command, "applZip4", applicantZip4);
        db.SetNullableString(command, "applZip3", applicantZip3);
        db.SetNullableInt32(command, "applPhone", applicantPhone);
        db.SetNullableDate(command, "dtApplSent", dateApplicationSent);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "serviceCode", serviceCode);
        db.SetNullableString(command, "reasonIncomplete", reasonIncomplete);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "reasonDenied", reasonDenied);
        db.SetNullableDate(command, "dateDenied", dateDenied);
        db.SetNullableInt32(command, "applicantAreaCd", applicantAreaCode);
        db.SetNullableString(command, "applProcInd", applicationProcessedInd);
        db.SetNullableString(command, "nameSearchComp", "");
        db.SetNullableString(command, "reopenReasonType", reopenReasonType);
        db.SetNullableString(command, "miscellaneousRsn", miscellaneousReason);
        db.SetString(command, "fkCktCasenumb", "");
      });

    entities.InformationRequest.Number = number;
    entities.InformationRequest.NonparentQuestionnaireSent =
      nonparentQuestionnaireSent;
    entities.InformationRequest.ParentQuestionnaireSent =
      parentQuestionnaireSent;
    entities.InformationRequest.PaternityQuestionnaireSent =
      paternityQuestionnaireSent;
    entities.InformationRequest.ApplicationSentIndicator =
      applicationSentIndicator;
    entities.InformationRequest.QuestionnaireTypeIndicator =
      questionnaireTypeIndicator;
    entities.InformationRequest.DateReceivedByCseComplete =
      dateReceivedByCseComplete;
    entities.InformationRequest.DateReceivedByCseIncomplete =
      dateReceivedByCseIncomplete;
    entities.InformationRequest.DateApplicationRequested =
      dateApplicationRequested;
    entities.InformationRequest.CallerLastName = callerLastName;
    entities.InformationRequest.CallerFirstName = callerFirstName;
    entities.InformationRequest.CallerMiddleInitial = callerMiddleInitial;
    entities.InformationRequest.InquirerNameSuffix = inquirerNameSuffix;
    entities.InformationRequest.ApplicantLastName = applicantLastName;
    entities.InformationRequest.ApplicantFirstName = applicantFirstName;
    entities.InformationRequest.ApplicantMiddleInitial = applicantMiddleInitial;
    entities.InformationRequest.ApplicantNameSuffix = applicantNameSuffix;
    entities.InformationRequest.ApplicantStreet1 = applicantStreet1;
    entities.InformationRequest.ApplicantStreet2 = applicantStreet2;
    entities.InformationRequest.ApplicantCity = applicantCity;
    entities.InformationRequest.ApplicantState = applicantState;
    entities.InformationRequest.ApplicantZip5 = applicantZip5;
    entities.InformationRequest.ApplicantZip4 = applicantZip4;
    entities.InformationRequest.ApplicantZip3 = applicantZip3;
    entities.InformationRequest.ApplicantPhone = applicantPhone;
    entities.InformationRequest.DateApplicationSent = dateApplicationSent;
    entities.InformationRequest.Type1 = type1;
    entities.InformationRequest.ServiceCode = serviceCode;
    entities.InformationRequest.ReasonIncomplete = reasonIncomplete;
    entities.InformationRequest.Note = note;
    entities.InformationRequest.CreatedBy = createdBy;
    entities.InformationRequest.CreatedTimestamp = createdTimestamp;
    entities.InformationRequest.LastUpdatedBy = createdBy;
    entities.InformationRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.InformationRequest.ReasonDenied = reasonDenied;
    entities.InformationRequest.DateDenied = dateDenied;
    entities.InformationRequest.ApplicantAreaCode = applicantAreaCode;
    entities.InformationRequest.ApplicationProcessedInd =
      applicationProcessedInd;
    entities.InformationRequest.ReopenReasonType = reopenReasonType;
    entities.InformationRequest.MiscellaneousReason = miscellaneousReason;
    entities.InformationRequest.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private InformationRequest informationRequest;
  }
#endregion
}
