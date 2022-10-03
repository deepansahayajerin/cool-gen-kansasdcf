// Program: OE_B447_SVES_TITLEXVI_RESP_PRS, ID: 945066133, model: 746.
// Short name: SWE04474
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B447_SVES_TITLEXVI_RESP_PRS.
/// </para>
/// <para>
/// This Action Block maintains FCR/SVES Title-XVI information received from FCR
/// on a daily basis.
/// </para>
/// </summary>
[Serializable]
public partial class OeB447SvesTitlexviRespPrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_SVES_TITLEXVI_RESP_PRS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447SvesTitlexviRespPrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447SvesTitlexviRespPrs.
  /// </summary>
  public OeB447SvesTitlexviRespPrs(IContext context, Import import,
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
    // ******************************************************************************************
    // * This Action Block received the SVES Title-XVI information from the 
    // calling object and  *
    // * process them by adding/upding to CSE database and create required 
    // worker alert,income  *
    // * source & document generation wherever 
    // required.
    // 
    // *
    // ******************************************************************************************
    // ******************************************************************************************
    // *                                  
    // Maintenance Log
    // 
    // *
    // ******************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------   
    // --------------------------------------------*
    // * 06/03/2011  Raj S              CQ5577      Initial Coding.
    // *
    // *
    // 
    // *
    // ******************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *******************************************************************************************
    // ** Check whether received Titl-XVI record already exists in CSE database 
    // then update     **
    // ** the existing information otherwise create a new Title-XVI response 
    // entry to CSE DB.   **
    // *******************************************************************************************
    local.Infrastructure.Assign(import.Infrastructure);
    local.Process.Date = import.Infrastructure.ReferenceDate;

    if (ReadFcrSvesGenInfo())
    {
      if (ReadFcrSvesTitleXvi())
      {
        try
        {
          UpdateFcrSvesTitleXvi();
          ++import.TotT16Updated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_TITLEXVI_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_TITLEXVI_PV";

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
        try
        {
          CreateFcrSvesTitleXvi();
          ++import.TotT16Created.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_TITLEXVI_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_TITLEXVI_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "FCR_SVES_GEN_INFO_NF";

      return;
    }

    // ******************************************************************************************
    // * Geenrate alerts, if the person plays a role AP or CH in any of CSE 
    // Cases, the person   *
    // * should be active as well as the case.
    // 
    // *
    // ******************************************************************************************
    UseOeB447SvesAlertNIwoGen();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      import.TotT16AlertCreated.Count += local.TotAlertRecsCreated.Count;
      import.TotT16AlertExists.Count += local.TotAlertExistsRecs.Count;
      import.TotT16HistCreated.Count += local.TotHistRecsCreated.Count;
      import.TotT16HistExists.Count += local.TotHistExistsRecs.Count;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveFcrSvesGenInfo(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.MemberId = source.MemberId;
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
  }

  private void UseOeB447SvesAlertNIwoGen()
  {
    var useImport = new OeB447SvesAlertNIwoGen.Import();
    var useExport = new OeB447SvesAlertNIwoGen.Export();

    useImport.IwoGenerationSkipFl.Flag = import.IwoGenerationSkipFl.Flag;
    useImport.AlertGenerationSkipFl.Flag = import.AlertGenerationSkipFl.Flag;
    useImport.Max.Date = import.MaxDate.Date;
    MoveFcrSvesGenInfo(import.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ProcessingDate.Date = local.Process.Date;

    Call(OeB447SvesAlertNIwoGen.Execute, useImport, useExport);

    local.TotAlertRecsCreated.Count = useExport.TotAlertRecsCreated.Count;
    local.TotHistRecsCreated.Count = useExport.TotHistRecsCreated.Count;
    local.TotAlertExistsRecs.Count = useExport.TotAlertExistsRecs.Count;
    local.TotHistExistsRecs.Count = useExport.TotHistExistsRecs.Count;
  }

  private void CreateFcrSvesTitleXvi()
  {
    var fcgMemberId = entities.ExistingFcrSvesGenInfo.MemberId;
    var fcgLSRspAgy =
      entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo;
    var seqNo = import.FcrSvesTitleXvi.SeqNo;
    var otherName = import.FcrSvesTitleXvi.OtherName ?? "";
    var raceCode = import.FcrSvesTitleXvi.RaceCode ?? "";
    var dateOfDeathSourceCode =
      import.FcrSvesTitleXvi.DateOfDeathSourceCode ?? "";
    var payeeStateOfJurisdiction =
      import.FcrSvesTitleXvi.PayeeStateOfJurisdiction ?? "";
    var payeeCountyOfJurisdiction =
      import.FcrSvesTitleXvi.PayeeCountyOfJurisdiction ?? "";
    var payeeDistrictOfficeCode =
      import.FcrSvesTitleXvi.PayeeDistrictOfficeCode ?? "";
    var typeOfPayeeCode = import.FcrSvesTitleXvi.TypeOfPayeeCode ?? "";
    var typeOfRecipient = import.FcrSvesTitleXvi.TypeOfRecipient ?? "";
    var recordEstablishmentDate =
      import.FcrSvesTitleXvi.RecordEstablishmentDate;
    var dateOfTitleXviEligibility =
      import.FcrSvesTitleXvi.DateOfTitleXviEligibility;
    var titleXviAppealCode = import.FcrSvesTitleXvi.TitleXviAppealCode ?? "";
    var dateOfTitleXviAppeal = import.FcrSvesTitleXvi.DateOfTitleXviAppeal;
    var titleXviLastRedeterminDate =
      import.FcrSvesTitleXvi.TitleXviLastRedeterminDate;
    var titleXviDenialDate = import.FcrSvesTitleXvi.TitleXviDenialDate;
    var currentPaymentStatusCode =
      import.FcrSvesTitleXvi.CurrentPaymentStatusCode ?? "";
    var paymentStatusCode = import.FcrSvesTitleXvi.PaymentStatusCode ?? "";
    var paymentStatusDate = import.FcrSvesTitleXvi.PaymentStatusDate;
    var telephoneNumber = import.FcrSvesTitleXvi.TelephoneNumber ?? "";
    var thirdPartyInsuranceIndicator =
      import.FcrSvesTitleXvi.ThirdPartyInsuranceIndicator ?? "";
    var directDepositIndicator =
      import.FcrSvesTitleXvi.DirectDepositIndicator ?? "";
    var representativePayeeIndicator =
      import.FcrSvesTitleXvi.RepresentativePayeeIndicator ?? "";
    var custodyCode = import.FcrSvesTitleXvi.CustodyCode ?? "";
    var estimatedSelfEmploymentAmount =
      import.FcrSvesTitleXvi.EstimatedSelfEmploymentAmount.GetValueOrDefault();
    var unearnedIncomeNumOfEntries =
      import.FcrSvesTitleXvi.UnearnedIncomeNumOfEntries.GetValueOrDefault();
    var unearnedIncomeTypeCode1 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode1 ?? "";
    var unearnedIncomeVerifiCd1 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd1 ?? "";
    var unearnedIncomeStartDate1 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate1;
    var unearnedIncomeStopDate1 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate1;
    var unearnedIncomeTypeCode2 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode2 ?? "";
    var unearnedIncomeVerifiCd2 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd2 ?? "";
    var unearnedIncomeStartDate2 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate2;
    var unearnedIncomeStopDate2 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate2;
    var unearnedIncomeTypeCode3 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode3 ?? "";
    var unearnedIncomeVerifiCd3 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd3 ?? "";
    var unearnedIncomeStartDate3 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate3;
    var unearnedIncomeStopDate3 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate3;
    var unearnedIncomeTypeCode4 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode4 ?? "";
    var unearnedIncomeVerifiCd4 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd4 ?? "";
    var unearnedIncomeStartDate4 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate4;
    var unearnedIncomeStopDate4 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate4;
    var unearnedIncomeTypeCode5 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode5 ?? "";
    var unearnedIncomeVerifiCd5 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd5 ?? "";
    var unearnedIncomeStartDate5 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate5;
    var unearnedIncomeStopDate5 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate5;
    var unearnedIncomeTypeCode6 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode6 ?? "";
    var unearnedIncomeVerifiCd6 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd6 ?? "";
    var unearnedIncomeStartDate6 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate6;
    var unearnedIncomeStopDate6 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate6;
    var unearnedIncomeTypeCode7 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode7 ?? "";
    var unearnedIncomeVerifiCd7 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd7 ?? "";
    var unearnedIncomeStartDate7 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate7;
    var unearnedIncomeStopDate7 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate7;
    var unearnedIncomeTypeCode8 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode8 ?? "";
    var unearnedIncomeVerifiCd8 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd8 ?? "";
    var unearnedIncomeStartDate8 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate8;
    var unearnedIncomeStopDate8 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate8;
    var unearnedIncomeTypeCode9 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode9 ?? "";
    var unearnedIncomeVerifiCd9 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd9 ?? "";
    var unearnedIncomeStartDate9 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate9;
    var unearnedIncomeStopDate9 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate9;
    var phistNumberOfEntries =
      import.FcrSvesTitleXvi.PhistNumberOfEntries.GetValueOrDefault();
    var phistPaymentDate1 = import.FcrSvesTitleXvi.PhistPaymentDate1;
    var ssiMonthlyAssistanceAmount1 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount1.GetValueOrDefault();
    var phistPaymentPayFlag1 = import.FcrSvesTitleXvi.PhistPaymentPayFlag1 ?? ""
      ;
    var phistPaymentDate2 = import.FcrSvesTitleXvi.PhistPaymentDate2;
    var ssiMonthlyAssistanceAmount2 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount2.GetValueOrDefault();
    var phistPaymentPayFlag2 = import.FcrSvesTitleXvi.PhistPaymentPayFlag2 ?? ""
      ;
    var phistPaymentDate3 = import.FcrSvesTitleXvi.PhistPaymentDate3;
    var ssiMonthlyAssistanceAmount3 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount3.GetValueOrDefault();
    var phistPaymentPayFlag3 = import.FcrSvesTitleXvi.PhistPaymentPayFlag3 ?? ""
      ;
    var phistPaymentDate4 = import.FcrSvesTitleXvi.PhistPaymentDate4;
    var ssiMonthlyAssistanceAmount4 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount4.GetValueOrDefault();
    var phistPaymentPayFlag4 = import.FcrSvesTitleXvi.PhistPaymentPayFlag4 ?? ""
      ;
    var phistPaymentDate5 = import.FcrSvesTitleXvi.PhistPaymentDate5;
    var ssiMonthlyAssistanceAmount5 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount5.GetValueOrDefault();
    var phistPaymentPayFlag5 = import.FcrSvesTitleXvi.PhistPaymentPayFlag5 ?? ""
      ;
    var phistPaymentDate6 = import.FcrSvesTitleXvi.PhistPaymentDate6;
    var ssiMonthlyAssistanceAmount6 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount6.GetValueOrDefault();
    var phistPaymentPayFlag6 = import.FcrSvesTitleXvi.PhistPaymentPayFlag6 ?? ""
      ;
    var phistPaymentDate7 = import.FcrSvesTitleXvi.PhistPaymentDate7;
    var ssiMonthlyAssistanceAmount7 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount7.GetValueOrDefault();
    var phistPaymentPayFlag7 = import.FcrSvesTitleXvi.PhistPaymentPayFlag7 ?? ""
      ;
    var phistPaymentDate8 = import.FcrSvesTitleXvi.PhistPaymentDate8;
    var ssiMonthlyAssistanceAmount8 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount8.GetValueOrDefault();
    var phistPaymentPayFlag8 = import.FcrSvesTitleXvi.PhistPaymentPayFlag8 ?? ""
      ;
    var createdBy = import.FcrSvesTitleXvi.CreatedBy;
    var createdTimestamp = import.FcrSvesTitleXvi.CreatedTimestamp;
    var lastUpdatedBy = local.Null1.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Null1.LastUpdatedTimestamp;

    entities.ExistingFcrSvesTitleXvi.Populated = false;
    Update("CreateFcrSvesTitleXvi",
      (db, command) =>
      {
        db.SetString(command, "fcgMemberId", fcgMemberId);
        db.SetString(command, "fcgLSRspAgy", fcgLSRspAgy);
        db.SetInt32(command, "seqNo", seqNo);
        db.SetNullableString(command, "otherName", otherName);
        db.SetNullableString(command, "raceCode", raceCode);
        db.SetNullableString(command, "dodSourceCode", dateOfDeathSourceCode);
        db.SetNullableString(command, "payeeState", payeeStateOfJurisdiction);
        db.SetNullableString(command, "payeeCounty", payeeCountyOfJurisdiction);
        db.
          SetNullableString(command, "payeeDistOfcCd", payeeDistrictOfficeCode);
          
        db.SetNullableString(command, "payeeTypeCd", typeOfPayeeCode);
        db.SetNullableString(command, "recipientType", typeOfRecipient);
        db.SetNullableDate(command, "recEstDt", recordEstablishmentDate);
        db.SetNullableDate(command, "t16EligDt", dateOfTitleXviEligibility);
        db.SetNullableString(command, "t16AppealCd", titleXviAppealCode);
        db.SetNullableDate(command, "t16AppealDt", dateOfTitleXviAppeal);
        db.
          SetNullableDate(command, "t16LastRedetDt", titleXviLastRedeterminDate);
          
        db.SetNullableDate(command, "t16DenialDt", titleXviDenialDate);
        db.SetNullableString(command, "curPymntStCd", currentPaymentStatusCode);
        db.SetNullableString(command, "paymentStCd", paymentStatusCode);
        db.SetNullableDate(command, "paymentStDt", paymentStatusDate);
        db.SetNullableString(command, "telephoneNo", telephoneNumber);
        db.SetNullableString(
          command, "thirdPrtyInsInd", thirdPartyInsuranceIndicator);
        db.
          SetNullableString(command, "directDepositInd", directDepositIndicator);
          
        db.SetNullableString(
          command, "repPayeeInd", representativePayeeIndicator);
        db.SetNullableString(command, "custodyCode", custodyCode);
        db.SetNullableDecimal(
          command, "estSelfEmpAmt", estimatedSelfEmploymentAmount);
        db.
          SetNullableInt32(command, "uiNumEntries", unearnedIncomeNumOfEntries);
          
        db.SetNullableString(command, "uiTypeCode1", unearnedIncomeTypeCode1);
        db.SetNullableString(command, "uiVerifiCd1", unearnedIncomeVerifiCd1);
        db.SetNullableDate(command, "uiStartDt1", unearnedIncomeStartDate1);
        db.SetNullableDate(command, "uiStopDt1", unearnedIncomeStopDate1);
        db.SetNullableString(command, "uiTypeCd2", unearnedIncomeTypeCode2);
        db.SetNullableString(command, "uiVerifiCd2", unearnedIncomeVerifiCd2);
        db.SetNullableDate(command, "uiStartDt2", unearnedIncomeStartDate2);
        db.SetNullableDate(command, "uiStopDt2", unearnedIncomeStopDate2);
        db.SetNullableString(command, "uiTypeCd3", unearnedIncomeTypeCode3);
        db.SetNullableString(command, "uiVerifiCd3", unearnedIncomeVerifiCd3);
        db.SetNullableDate(command, "uiStartDt3", unearnedIncomeStartDate3);
        db.SetNullableDate(command, "uiStopDt3", unearnedIncomeStopDate3);
        db.SetNullableString(command, "uiTypeCd4", unearnedIncomeTypeCode4);
        db.SetNullableString(command, "uiVerifiCd4", unearnedIncomeVerifiCd4);
        db.SetNullableDate(command, "uiStartDt4", unearnedIncomeStartDate4);
        db.SetNullableDate(command, "uiStopDt4", unearnedIncomeStopDate4);
        db.SetNullableString(command, "uiTypeCd5", unearnedIncomeTypeCode5);
        db.SetNullableString(command, "uiVerifiCd5", unearnedIncomeVerifiCd5);
        db.SetNullableDate(command, "uiStartDt5", unearnedIncomeStartDate5);
        db.SetNullableDate(command, "uiStopDt5", unearnedIncomeStopDate5);
        db.SetNullableString(command, "uiTypeCd6", unearnedIncomeTypeCode6);
        db.SetNullableString(command, "uiVerifiCd6", unearnedIncomeVerifiCd6);
        db.SetNullableDate(command, "uiStartDt6", unearnedIncomeStartDate6);
        db.SetNullableDate(command, "uiStopDt6", unearnedIncomeStopDate6);
        db.SetNullableString(command, "uiTypeCd7", unearnedIncomeTypeCode7);
        db.SetNullableString(command, "uiVerifiCd7", unearnedIncomeVerifiCd7);
        db.SetNullableDate(command, "uiStartDt7", unearnedIncomeStartDate7);
        db.SetNullableDate(command, "uiStopDt7", unearnedIncomeStopDate7);
        db.SetNullableString(command, "uiTypeCd8", unearnedIncomeTypeCode8);
        db.SetNullableString(command, "uiVerifiCd8", unearnedIncomeVerifiCd8);
        db.SetNullableDate(command, "uiStartDt8", unearnedIncomeStartDate8);
        db.SetNullableDate(command, "uiStopDt8", unearnedIncomeStopDate8);
        db.SetNullableString(command, "uiTypeCd9", unearnedIncomeTypeCode9);
        db.SetNullableString(command, "uiVerifiCd9", unearnedIncomeVerifiCd9);
        db.SetNullableDate(command, "uiStartDt9", unearnedIncomeStartDate9);
        db.SetNullableDate(command, "uiStopDt9", unearnedIncomeStopDate9);
        db.SetNullableInt32(command, "phistNoEntries", phistNumberOfEntries);
        db.SetNullableDate(command, "phistPmtDt1", phistPaymentDate1);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt1", ssiMonthlyAssistanceAmount1);
        db.SetNullableString(command, "phistPmtFlag1", phistPaymentPayFlag1);
        db.SetNullableDate(command, "phistPmtDt2", phistPaymentDate2);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt2", ssiMonthlyAssistanceAmount2);
        db.SetNullableString(command, "phistPmtFlag2", phistPaymentPayFlag2);
        db.SetNullableDate(command, "phistPmntDt3", phistPaymentDate3);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt3", ssiMonthlyAssistanceAmount3);
        db.SetNullableString(command, "phistPmtFlag3", phistPaymentPayFlag3);
        db.SetNullableDate(command, "phistPmtDt4", phistPaymentDate4);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt4", ssiMonthlyAssistanceAmount4);
        db.SetNullableString(command, "phistPmtFlag4", phistPaymentPayFlag4);
        db.SetNullableDate(command, "phistPmtDt5", phistPaymentDate5);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt5", ssiMonthlyAssistanceAmount5);
        db.SetNullableString(command, "phistPmtFlag5", phistPaymentPayFlag5);
        db.SetNullableDate(command, "phistPmtDt6", phistPaymentDate6);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt6", ssiMonthlyAssistanceAmount6);
        db.SetNullableString(command, "phistPmtFlag6", phistPaymentPayFlag6);
        db.SetNullableDate(command, "phistPmtDt7", phistPaymentDate7);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt7", ssiMonthlyAssistanceAmount7);
        db.SetNullableString(command, "phistPmtFlag7", phistPaymentPayFlag7);
        db.SetNullableDate(command, "phistPmtDt8", phistPaymentDate8);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt8", ssiMonthlyAssistanceAmount8);
        db.SetNullableString(command, "phistPmtFlag8", phistPaymentPayFlag8);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
      });

    entities.ExistingFcrSvesTitleXvi.FcgMemberId = fcgMemberId;
    entities.ExistingFcrSvesTitleXvi.FcgLSRspAgy = fcgLSRspAgy;
    entities.ExistingFcrSvesTitleXvi.SeqNo = seqNo;
    entities.ExistingFcrSvesTitleXvi.OtherName = otherName;
    entities.ExistingFcrSvesTitleXvi.RaceCode = raceCode;
    entities.ExistingFcrSvesTitleXvi.DateOfDeathSourceCode =
      dateOfDeathSourceCode;
    entities.ExistingFcrSvesTitleXvi.PayeeStateOfJurisdiction =
      payeeStateOfJurisdiction;
    entities.ExistingFcrSvesTitleXvi.PayeeCountyOfJurisdiction =
      payeeCountyOfJurisdiction;
    entities.ExistingFcrSvesTitleXvi.PayeeDistrictOfficeCode =
      payeeDistrictOfficeCode;
    entities.ExistingFcrSvesTitleXvi.TypeOfPayeeCode = typeOfPayeeCode;
    entities.ExistingFcrSvesTitleXvi.TypeOfRecipient = typeOfRecipient;
    entities.ExistingFcrSvesTitleXvi.RecordEstablishmentDate =
      recordEstablishmentDate;
    entities.ExistingFcrSvesTitleXvi.DateOfTitleXviEligibility =
      dateOfTitleXviEligibility;
    entities.ExistingFcrSvesTitleXvi.TitleXviAppealCode = titleXviAppealCode;
    entities.ExistingFcrSvesTitleXvi.DateOfTitleXviAppeal =
      dateOfTitleXviAppeal;
    entities.ExistingFcrSvesTitleXvi.TitleXviLastRedeterminDate =
      titleXviLastRedeterminDate;
    entities.ExistingFcrSvesTitleXvi.TitleXviDenialDate = titleXviDenialDate;
    entities.ExistingFcrSvesTitleXvi.CurrentPaymentStatusCode =
      currentPaymentStatusCode;
    entities.ExistingFcrSvesTitleXvi.PaymentStatusCode = paymentStatusCode;
    entities.ExistingFcrSvesTitleXvi.PaymentStatusDate = paymentStatusDate;
    entities.ExistingFcrSvesTitleXvi.TelephoneNumber = telephoneNumber;
    entities.ExistingFcrSvesTitleXvi.ThirdPartyInsuranceIndicator =
      thirdPartyInsuranceIndicator;
    entities.ExistingFcrSvesTitleXvi.DirectDepositIndicator =
      directDepositIndicator;
    entities.ExistingFcrSvesTitleXvi.RepresentativePayeeIndicator =
      representativePayeeIndicator;
    entities.ExistingFcrSvesTitleXvi.CustodyCode = custodyCode;
    entities.ExistingFcrSvesTitleXvi.EstimatedSelfEmploymentAmount =
      estimatedSelfEmploymentAmount;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeNumOfEntries =
      unearnedIncomeNumOfEntries;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode1 =
      unearnedIncomeTypeCode1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd1 =
      unearnedIncomeVerifiCd1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate1 =
      unearnedIncomeStartDate1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate1 =
      unearnedIncomeStopDate1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode2 =
      unearnedIncomeTypeCode2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd2 =
      unearnedIncomeVerifiCd2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate2 =
      unearnedIncomeStartDate2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate2 =
      unearnedIncomeStopDate2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode3 =
      unearnedIncomeTypeCode3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd3 =
      unearnedIncomeVerifiCd3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate3 =
      unearnedIncomeStartDate3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate3 =
      unearnedIncomeStopDate3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode4 =
      unearnedIncomeTypeCode4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd4 =
      unearnedIncomeVerifiCd4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate4 =
      unearnedIncomeStartDate4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate4 =
      unearnedIncomeStopDate4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode5 =
      unearnedIncomeTypeCode5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd5 =
      unearnedIncomeVerifiCd5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate5 =
      unearnedIncomeStartDate5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate5 =
      unearnedIncomeStopDate5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode6 =
      unearnedIncomeTypeCode6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd6 =
      unearnedIncomeVerifiCd6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate6 =
      unearnedIncomeStartDate6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate6 =
      unearnedIncomeStopDate6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode7 =
      unearnedIncomeTypeCode7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd7 =
      unearnedIncomeVerifiCd7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate7 =
      unearnedIncomeStartDate7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate7 =
      unearnedIncomeStopDate7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode8 =
      unearnedIncomeTypeCode8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd8 =
      unearnedIncomeVerifiCd8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate8 =
      unearnedIncomeStartDate8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate8 =
      unearnedIncomeStopDate8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode9 =
      unearnedIncomeTypeCode9;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd9 =
      unearnedIncomeVerifiCd9;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate9 =
      unearnedIncomeStartDate9;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate9 =
      unearnedIncomeStopDate9;
    entities.ExistingFcrSvesTitleXvi.PhistNumberOfEntries =
      phistNumberOfEntries;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate1 = phistPaymentDate1;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount1 =
      ssiMonthlyAssistanceAmount1;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag1 =
      phistPaymentPayFlag1;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate2 = phistPaymentDate2;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount2 =
      ssiMonthlyAssistanceAmount2;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag2 =
      phistPaymentPayFlag2;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate3 = phistPaymentDate3;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount3 =
      ssiMonthlyAssistanceAmount3;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag3 =
      phistPaymentPayFlag3;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate4 = phistPaymentDate4;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount4 =
      ssiMonthlyAssistanceAmount4;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag4 =
      phistPaymentPayFlag4;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate5 = phistPaymentDate5;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount5 =
      ssiMonthlyAssistanceAmount5;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag5 =
      phistPaymentPayFlag5;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate6 = phistPaymentDate6;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount6 =
      ssiMonthlyAssistanceAmount6;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag6 =
      phistPaymentPayFlag6;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate7 = phistPaymentDate7;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount7 =
      ssiMonthlyAssistanceAmount7;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag7 =
      phistPaymentPayFlag7;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate8 = phistPaymentDate8;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount8 =
      ssiMonthlyAssistanceAmount8;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag8 =
      phistPaymentPayFlag8;
    entities.ExistingFcrSvesTitleXvi.CreatedBy = createdBy;
    entities.ExistingFcrSvesTitleXvi.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrSvesTitleXvi.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesTitleXvi.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFcrSvesTitleXvi.Populated = true;
  }

  private bool ReadFcrSvesGenInfo()
  {
    entities.ExistingFcrSvesGenInfo.Populated = false;

    return Read("ReadFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", import.FcrSvesGenInfo.MemberId);
        db.SetString(
          command, "locSrcRspAgyCd",
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesTitleXvi()
  {
    entities.ExistingFcrSvesTitleXvi.Populated = false;

    return Read("ReadFcrSvesTitleXvi",
      (db, command) =>
      {
        db.SetInt32(command, "seqNo", import.FcrSvesTitleXvi.SeqNo);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesTitleXvi.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesTitleXvi.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesTitleXvi.SeqNo = db.GetInt32(reader, 2);
        entities.ExistingFcrSvesTitleXvi.OtherName =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesTitleXvi.RaceCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesTitleXvi.DateOfDeathSourceCode =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesTitleXvi.PayeeStateOfJurisdiction =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesTitleXvi.PayeeCountyOfJurisdiction =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesTitleXvi.PayeeDistrictOfficeCode =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesTitleXvi.TypeOfPayeeCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesTitleXvi.TypeOfRecipient =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesTitleXvi.RecordEstablishmentDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingFcrSvesTitleXvi.DateOfTitleXviEligibility =
          db.GetNullableDate(reader, 12);
        entities.ExistingFcrSvesTitleXvi.TitleXviAppealCode =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesTitleXvi.DateOfTitleXviAppeal =
          db.GetNullableDate(reader, 14);
        entities.ExistingFcrSvesTitleXvi.TitleXviLastRedeterminDate =
          db.GetNullableDate(reader, 15);
        entities.ExistingFcrSvesTitleXvi.TitleXviDenialDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingFcrSvesTitleXvi.CurrentPaymentStatusCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesTitleXvi.PaymentStatusCode =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesTitleXvi.PaymentStatusDate =
          db.GetNullableDate(reader, 19);
        entities.ExistingFcrSvesTitleXvi.TelephoneNumber =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrSvesTitleXvi.ThirdPartyInsuranceIndicator =
          db.GetNullableString(reader, 21);
        entities.ExistingFcrSvesTitleXvi.DirectDepositIndicator =
          db.GetNullableString(reader, 22);
        entities.ExistingFcrSvesTitleXvi.RepresentativePayeeIndicator =
          db.GetNullableString(reader, 23);
        entities.ExistingFcrSvesTitleXvi.CustodyCode =
          db.GetNullableString(reader, 24);
        entities.ExistingFcrSvesTitleXvi.EstimatedSelfEmploymentAmount =
          db.GetNullableDecimal(reader, 25);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeNumOfEntries =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode1 =
          db.GetNullableString(reader, 27);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd1 =
          db.GetNullableString(reader, 28);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate1 =
          db.GetNullableDate(reader, 29);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate1 =
          db.GetNullableDate(reader, 30);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode2 =
          db.GetNullableString(reader, 31);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd2 =
          db.GetNullableString(reader, 32);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate2 =
          db.GetNullableDate(reader, 33);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate2 =
          db.GetNullableDate(reader, 34);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode3 =
          db.GetNullableString(reader, 35);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd3 =
          db.GetNullableString(reader, 36);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate3 =
          db.GetNullableDate(reader, 37);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate3 =
          db.GetNullableDate(reader, 38);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode4 =
          db.GetNullableString(reader, 39);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd4 =
          db.GetNullableString(reader, 40);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate4 =
          db.GetNullableDate(reader, 41);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate4 =
          db.GetNullableDate(reader, 42);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode5 =
          db.GetNullableString(reader, 43);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd5 =
          db.GetNullableString(reader, 44);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate5 =
          db.GetNullableDate(reader, 45);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate5 =
          db.GetNullableDate(reader, 46);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode6 =
          db.GetNullableString(reader, 47);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd6 =
          db.GetNullableString(reader, 48);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate6 =
          db.GetNullableDate(reader, 49);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate6 =
          db.GetNullableDate(reader, 50);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode7 =
          db.GetNullableString(reader, 51);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd7 =
          db.GetNullableString(reader, 52);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate7 =
          db.GetNullableDate(reader, 53);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate7 =
          db.GetNullableDate(reader, 54);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode8 =
          db.GetNullableString(reader, 55);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd8 =
          db.GetNullableString(reader, 56);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate8 =
          db.GetNullableDate(reader, 57);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate8 =
          db.GetNullableDate(reader, 58);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode9 =
          db.GetNullableString(reader, 59);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd9 =
          db.GetNullableString(reader, 60);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate9 =
          db.GetNullableDate(reader, 61);
        entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate9 =
          db.GetNullableDate(reader, 62);
        entities.ExistingFcrSvesTitleXvi.PhistNumberOfEntries =
          db.GetNullableInt32(reader, 63);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate1 =
          db.GetNullableDate(reader, 64);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount1 =
          db.GetNullableDecimal(reader, 65);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag1 =
          db.GetNullableString(reader, 66);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate2 =
          db.GetNullableDate(reader, 67);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount2 =
          db.GetNullableDecimal(reader, 68);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag2 =
          db.GetNullableString(reader, 69);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate3 =
          db.GetNullableDate(reader, 70);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount3 =
          db.GetNullableDecimal(reader, 71);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag3 =
          db.GetNullableString(reader, 72);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate4 =
          db.GetNullableDate(reader, 73);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount4 =
          db.GetNullableDecimal(reader, 74);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag4 =
          db.GetNullableString(reader, 75);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate5 =
          db.GetNullableDate(reader, 76);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount5 =
          db.GetNullableDecimal(reader, 77);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag5 =
          db.GetNullableString(reader, 78);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate6 =
          db.GetNullableDate(reader, 79);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount6 =
          db.GetNullableDecimal(reader, 80);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag6 =
          db.GetNullableString(reader, 81);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate7 =
          db.GetNullableDate(reader, 82);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount7 =
          db.GetNullableDecimal(reader, 83);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag7 =
          db.GetNullableString(reader, 84);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentDate8 =
          db.GetNullableDate(reader, 85);
        entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount8 =
          db.GetNullableDecimal(reader, 86);
        entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag8 =
          db.GetNullableString(reader, 87);
        entities.ExistingFcrSvesTitleXvi.CreatedBy = db.GetString(reader, 88);
        entities.ExistingFcrSvesTitleXvi.CreatedTimestamp =
          db.GetDateTime(reader, 89);
        entities.ExistingFcrSvesTitleXvi.LastUpdatedBy =
          db.GetNullableString(reader, 90);
        entities.ExistingFcrSvesTitleXvi.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 91);
        entities.ExistingFcrSvesTitleXvi.Populated = true;
      });
  }

  private void UpdateFcrSvesTitleXvi()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFcrSvesTitleXvi.Populated);

    var otherName = import.FcrSvesTitleXvi.OtherName ?? "";
    var raceCode = import.FcrSvesTitleXvi.RaceCode ?? "";
    var dateOfDeathSourceCode =
      import.FcrSvesTitleXvi.DateOfDeathSourceCode ?? "";
    var payeeStateOfJurisdiction =
      import.FcrSvesTitleXvi.PayeeStateOfJurisdiction ?? "";
    var payeeCountyOfJurisdiction =
      import.FcrSvesTitleXvi.PayeeCountyOfJurisdiction ?? "";
    var payeeDistrictOfficeCode =
      import.FcrSvesTitleXvi.PayeeDistrictOfficeCode ?? "";
    var typeOfPayeeCode = import.FcrSvesTitleXvi.TypeOfPayeeCode ?? "";
    var typeOfRecipient = import.FcrSvesTitleXvi.TypeOfRecipient ?? "";
    var recordEstablishmentDate =
      import.FcrSvesTitleXvi.RecordEstablishmentDate;
    var dateOfTitleXviEligibility =
      import.FcrSvesTitleXvi.DateOfTitleXviEligibility;
    var titleXviAppealCode = import.FcrSvesTitleXvi.TitleXviAppealCode ?? "";
    var dateOfTitleXviAppeal = import.FcrSvesTitleXvi.DateOfTitleXviAppeal;
    var titleXviLastRedeterminDate =
      import.FcrSvesTitleXvi.TitleXviLastRedeterminDate;
    var titleXviDenialDate = import.FcrSvesTitleXvi.TitleXviDenialDate;
    var currentPaymentStatusCode =
      import.FcrSvesTitleXvi.CurrentPaymentStatusCode ?? "";
    var paymentStatusCode = import.FcrSvesTitleXvi.PaymentStatusCode ?? "";
    var paymentStatusDate = import.FcrSvesTitleXvi.PaymentStatusDate;
    var telephoneNumber = import.FcrSvesTitleXvi.TelephoneNumber ?? "";
    var thirdPartyInsuranceIndicator =
      import.FcrSvesTitleXvi.ThirdPartyInsuranceIndicator ?? "";
    var directDepositIndicator =
      import.FcrSvesTitleXvi.DirectDepositIndicator ?? "";
    var representativePayeeIndicator =
      import.FcrSvesTitleXvi.RepresentativePayeeIndicator ?? "";
    var custodyCode = import.FcrSvesTitleXvi.CustodyCode ?? "";
    var estimatedSelfEmploymentAmount =
      import.FcrSvesTitleXvi.EstimatedSelfEmploymentAmount.GetValueOrDefault();
    var unearnedIncomeNumOfEntries =
      import.FcrSvesTitleXvi.UnearnedIncomeNumOfEntries.GetValueOrDefault();
    var unearnedIncomeTypeCode1 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode1 ?? "";
    var unearnedIncomeVerifiCd1 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd1 ?? "";
    var unearnedIncomeStartDate1 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate1;
    var unearnedIncomeStopDate1 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate1;
    var unearnedIncomeTypeCode2 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode2 ?? "";
    var unearnedIncomeVerifiCd2 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd2 ?? "";
    var unearnedIncomeStartDate2 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate2;
    var unearnedIncomeStopDate2 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate2;
    var unearnedIncomeTypeCode3 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode3 ?? "";
    var unearnedIncomeVerifiCd3 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd3 ?? "";
    var unearnedIncomeStartDate3 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate3;
    var unearnedIncomeStopDate3 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate3;
    var unearnedIncomeTypeCode4 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode4 ?? "";
    var unearnedIncomeVerifiCd4 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd4 ?? "";
    var unearnedIncomeStartDate4 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate4;
    var unearnedIncomeStopDate4 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate4;
    var unearnedIncomeTypeCode5 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode5 ?? "";
    var unearnedIncomeVerifiCd5 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd5 ?? "";
    var unearnedIncomeStartDate5 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate5;
    var unearnedIncomeStopDate5 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate5;
    var unearnedIncomeTypeCode6 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode6 ?? "";
    var unearnedIncomeVerifiCd6 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd6 ?? "";
    var unearnedIncomeStartDate6 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate6;
    var unearnedIncomeStopDate6 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate6;
    var unearnedIncomeTypeCode7 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode7 ?? "";
    var unearnedIncomeVerifiCd7 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd7 ?? "";
    var unearnedIncomeStartDate7 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate7;
    var unearnedIncomeStopDate7 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate7;
    var unearnedIncomeTypeCode8 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode8 ?? "";
    var unearnedIncomeVerifiCd8 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd8 ?? "";
    var unearnedIncomeStartDate8 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate8;
    var unearnedIncomeStopDate8 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate8;
    var unearnedIncomeTypeCode9 =
      import.FcrSvesTitleXvi.UnearnedIncomeTypeCode9 ?? "";
    var unearnedIncomeVerifiCd9 =
      import.FcrSvesTitleXvi.UnearnedIncomeVerifiCd9 ?? "";
    var unearnedIncomeStartDate9 =
      import.FcrSvesTitleXvi.UnearnedIncomeStartDate9;
    var unearnedIncomeStopDate9 =
      import.FcrSvesTitleXvi.UnearnedIncomeStopDate9;
    var phistNumberOfEntries =
      import.FcrSvesTitleXvi.PhistNumberOfEntries.GetValueOrDefault();
    var phistPaymentDate1 = import.FcrSvesTitleXvi.PhistPaymentDate1;
    var ssiMonthlyAssistanceAmount1 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount1.GetValueOrDefault();
    var phistPaymentPayFlag1 = import.FcrSvesTitleXvi.PhistPaymentPayFlag1 ?? ""
      ;
    var phistPaymentDate2 = import.FcrSvesTitleXvi.PhistPaymentDate2;
    var ssiMonthlyAssistanceAmount2 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount2.GetValueOrDefault();
    var phistPaymentPayFlag2 = import.FcrSvesTitleXvi.PhistPaymentPayFlag2 ?? ""
      ;
    var phistPaymentDate3 = import.FcrSvesTitleXvi.PhistPaymentDate3;
    var ssiMonthlyAssistanceAmount3 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount3.GetValueOrDefault();
    var phistPaymentPayFlag3 = import.FcrSvesTitleXvi.PhistPaymentPayFlag3 ?? ""
      ;
    var phistPaymentDate4 = import.FcrSvesTitleXvi.PhistPaymentDate4;
    var ssiMonthlyAssistanceAmount4 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount4.GetValueOrDefault();
    var phistPaymentPayFlag4 = import.FcrSvesTitleXvi.PhistPaymentPayFlag4 ?? ""
      ;
    var phistPaymentDate5 = import.FcrSvesTitleXvi.PhistPaymentDate5;
    var ssiMonthlyAssistanceAmount5 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount5.GetValueOrDefault();
    var phistPaymentPayFlag5 = import.FcrSvesTitleXvi.PhistPaymentPayFlag5 ?? ""
      ;
    var phistPaymentDate6 = import.FcrSvesTitleXvi.PhistPaymentDate6;
    var ssiMonthlyAssistanceAmount6 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount6.GetValueOrDefault();
    var phistPaymentPayFlag6 = import.FcrSvesTitleXvi.PhistPaymentPayFlag6 ?? ""
      ;
    var phistPaymentDate7 = import.FcrSvesTitleXvi.PhistPaymentDate7;
    var ssiMonthlyAssistanceAmount7 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount7.GetValueOrDefault();
    var phistPaymentPayFlag7 = import.FcrSvesTitleXvi.PhistPaymentPayFlag7 ?? ""
      ;
    var phistPaymentDate8 = import.FcrSvesTitleXvi.PhistPaymentDate8;
    var ssiMonthlyAssistanceAmount8 =
      import.FcrSvesTitleXvi.SsiMonthlyAssistanceAmount8.GetValueOrDefault();
    var phistPaymentPayFlag8 = import.FcrSvesTitleXvi.PhistPaymentPayFlag8 ?? ""
      ;
    var lastUpdatedBy = import.FcrSvesTitleXvi.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.FcrSvesTitleXvi.LastUpdatedTimestamp;

    entities.ExistingFcrSvesTitleXvi.Populated = false;
    Update("UpdateFcrSvesTitleXvi",
      (db, command) =>
      {
        db.SetNullableString(command, "otherName", otherName);
        db.SetNullableString(command, "raceCode", raceCode);
        db.SetNullableString(command, "dodSourceCode", dateOfDeathSourceCode);
        db.SetNullableString(command, "payeeState", payeeStateOfJurisdiction);
        db.SetNullableString(command, "payeeCounty", payeeCountyOfJurisdiction);
        db.
          SetNullableString(command, "payeeDistOfcCd", payeeDistrictOfficeCode);
          
        db.SetNullableString(command, "payeeTypeCd", typeOfPayeeCode);
        db.SetNullableString(command, "recipientType", typeOfRecipient);
        db.SetNullableDate(command, "recEstDt", recordEstablishmentDate);
        db.SetNullableDate(command, "t16EligDt", dateOfTitleXviEligibility);
        db.SetNullableString(command, "t16AppealCd", titleXviAppealCode);
        db.SetNullableDate(command, "t16AppealDt", dateOfTitleXviAppeal);
        db.
          SetNullableDate(command, "t16LastRedetDt", titleXviLastRedeterminDate);
          
        db.SetNullableDate(command, "t16DenialDt", titleXviDenialDate);
        db.SetNullableString(command, "curPymntStCd", currentPaymentStatusCode);
        db.SetNullableString(command, "paymentStCd", paymentStatusCode);
        db.SetNullableDate(command, "paymentStDt", paymentStatusDate);
        db.SetNullableString(command, "telephoneNo", telephoneNumber);
        db.SetNullableString(
          command, "thirdPrtyInsInd", thirdPartyInsuranceIndicator);
        db.
          SetNullableString(command, "directDepositInd", directDepositIndicator);
          
        db.SetNullableString(
          command, "repPayeeInd", representativePayeeIndicator);
        db.SetNullableString(command, "custodyCode", custodyCode);
        db.SetNullableDecimal(
          command, "estSelfEmpAmt", estimatedSelfEmploymentAmount);
        db.
          SetNullableInt32(command, "uiNumEntries", unearnedIncomeNumOfEntries);
          
        db.SetNullableString(command, "uiTypeCode1", unearnedIncomeTypeCode1);
        db.SetNullableString(command, "uiVerifiCd1", unearnedIncomeVerifiCd1);
        db.SetNullableDate(command, "uiStartDt1", unearnedIncomeStartDate1);
        db.SetNullableDate(command, "uiStopDt1", unearnedIncomeStopDate1);
        db.SetNullableString(command, "uiTypeCd2", unearnedIncomeTypeCode2);
        db.SetNullableString(command, "uiVerifiCd2", unearnedIncomeVerifiCd2);
        db.SetNullableDate(command, "uiStartDt2", unearnedIncomeStartDate2);
        db.SetNullableDate(command, "uiStopDt2", unearnedIncomeStopDate2);
        db.SetNullableString(command, "uiTypeCd3", unearnedIncomeTypeCode3);
        db.SetNullableString(command, "uiVerifiCd3", unearnedIncomeVerifiCd3);
        db.SetNullableDate(command, "uiStartDt3", unearnedIncomeStartDate3);
        db.SetNullableDate(command, "uiStopDt3", unearnedIncomeStopDate3);
        db.SetNullableString(command, "uiTypeCd4", unearnedIncomeTypeCode4);
        db.SetNullableString(command, "uiVerifiCd4", unearnedIncomeVerifiCd4);
        db.SetNullableDate(command, "uiStartDt4", unearnedIncomeStartDate4);
        db.SetNullableDate(command, "uiStopDt4", unearnedIncomeStopDate4);
        db.SetNullableString(command, "uiTypeCd5", unearnedIncomeTypeCode5);
        db.SetNullableString(command, "uiVerifiCd5", unearnedIncomeVerifiCd5);
        db.SetNullableDate(command, "uiStartDt5", unearnedIncomeStartDate5);
        db.SetNullableDate(command, "uiStopDt5", unearnedIncomeStopDate5);
        db.SetNullableString(command, "uiTypeCd6", unearnedIncomeTypeCode6);
        db.SetNullableString(command, "uiVerifiCd6", unearnedIncomeVerifiCd6);
        db.SetNullableDate(command, "uiStartDt6", unearnedIncomeStartDate6);
        db.SetNullableDate(command, "uiStopDt6", unearnedIncomeStopDate6);
        db.SetNullableString(command, "uiTypeCd7", unearnedIncomeTypeCode7);
        db.SetNullableString(command, "uiVerifiCd7", unearnedIncomeVerifiCd7);
        db.SetNullableDate(command, "uiStartDt7", unearnedIncomeStartDate7);
        db.SetNullableDate(command, "uiStopDt7", unearnedIncomeStopDate7);
        db.SetNullableString(command, "uiTypeCd8", unearnedIncomeTypeCode8);
        db.SetNullableString(command, "uiVerifiCd8", unearnedIncomeVerifiCd8);
        db.SetNullableDate(command, "uiStartDt8", unearnedIncomeStartDate8);
        db.SetNullableDate(command, "uiStopDt8", unearnedIncomeStopDate8);
        db.SetNullableString(command, "uiTypeCd9", unearnedIncomeTypeCode9);
        db.SetNullableString(command, "uiVerifiCd9", unearnedIncomeVerifiCd9);
        db.SetNullableDate(command, "uiStartDt9", unearnedIncomeStartDate9);
        db.SetNullableDate(command, "uiStopDt9", unearnedIncomeStopDate9);
        db.SetNullableInt32(command, "phistNoEntries", phistNumberOfEntries);
        db.SetNullableDate(command, "phistPmtDt1", phistPaymentDate1);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt1", ssiMonthlyAssistanceAmount1);
        db.SetNullableString(command, "phistPmtFlag1", phistPaymentPayFlag1);
        db.SetNullableDate(command, "phistPmtDt2", phistPaymentDate2);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt2", ssiMonthlyAssistanceAmount2);
        db.SetNullableString(command, "phistPmtFlag2", phistPaymentPayFlag2);
        db.SetNullableDate(command, "phistPmntDt3", phistPaymentDate3);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt3", ssiMonthlyAssistanceAmount3);
        db.SetNullableString(command, "phistPmtFlag3", phistPaymentPayFlag3);
        db.SetNullableDate(command, "phistPmtDt4", phistPaymentDate4);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt4", ssiMonthlyAssistanceAmount4);
        db.SetNullableString(command, "phistPmtFlag4", phistPaymentPayFlag4);
        db.SetNullableDate(command, "phistPmtDt5", phistPaymentDate5);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt5", ssiMonthlyAssistanceAmount5);
        db.SetNullableString(command, "phistPmtFlag5", phistPaymentPayFlag5);
        db.SetNullableDate(command, "phistPmtDt6", phistPaymentDate6);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt6", ssiMonthlyAssistanceAmount6);
        db.SetNullableString(command, "phistPmtFlag6", phistPaymentPayFlag6);
        db.SetNullableDate(command, "phistPmtDt7", phistPaymentDate7);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt7", ssiMonthlyAssistanceAmount7);
        db.SetNullableString(command, "phistPmtFlag7", phistPaymentPayFlag7);
        db.SetNullableDate(command, "phistPmtDt8", phistPaymentDate8);
        db.SetNullableDecimal(
          command, "ssiMoAsstAmt8", ssiMonthlyAssistanceAmount8);
        db.SetNullableString(command, "phistPmtFlag8", phistPaymentPayFlag8);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesTitleXvi.FcgMemberId);
          
        db.SetString(
          command, "fcgLSRspAgy", entities.ExistingFcrSvesTitleXvi.FcgLSRspAgy);
          
        db.SetInt32(command, "seqNo", entities.ExistingFcrSvesTitleXvi.SeqNo);
      });

    entities.ExistingFcrSvesTitleXvi.OtherName = otherName;
    entities.ExistingFcrSvesTitleXvi.RaceCode = raceCode;
    entities.ExistingFcrSvesTitleXvi.DateOfDeathSourceCode =
      dateOfDeathSourceCode;
    entities.ExistingFcrSvesTitleXvi.PayeeStateOfJurisdiction =
      payeeStateOfJurisdiction;
    entities.ExistingFcrSvesTitleXvi.PayeeCountyOfJurisdiction =
      payeeCountyOfJurisdiction;
    entities.ExistingFcrSvesTitleXvi.PayeeDistrictOfficeCode =
      payeeDistrictOfficeCode;
    entities.ExistingFcrSvesTitleXvi.TypeOfPayeeCode = typeOfPayeeCode;
    entities.ExistingFcrSvesTitleXvi.TypeOfRecipient = typeOfRecipient;
    entities.ExistingFcrSvesTitleXvi.RecordEstablishmentDate =
      recordEstablishmentDate;
    entities.ExistingFcrSvesTitleXvi.DateOfTitleXviEligibility =
      dateOfTitleXviEligibility;
    entities.ExistingFcrSvesTitleXvi.TitleXviAppealCode = titleXviAppealCode;
    entities.ExistingFcrSvesTitleXvi.DateOfTitleXviAppeal =
      dateOfTitleXviAppeal;
    entities.ExistingFcrSvesTitleXvi.TitleXviLastRedeterminDate =
      titleXviLastRedeterminDate;
    entities.ExistingFcrSvesTitleXvi.TitleXviDenialDate = titleXviDenialDate;
    entities.ExistingFcrSvesTitleXvi.CurrentPaymentStatusCode =
      currentPaymentStatusCode;
    entities.ExistingFcrSvesTitleXvi.PaymentStatusCode = paymentStatusCode;
    entities.ExistingFcrSvesTitleXvi.PaymentStatusDate = paymentStatusDate;
    entities.ExistingFcrSvesTitleXvi.TelephoneNumber = telephoneNumber;
    entities.ExistingFcrSvesTitleXvi.ThirdPartyInsuranceIndicator =
      thirdPartyInsuranceIndicator;
    entities.ExistingFcrSvesTitleXvi.DirectDepositIndicator =
      directDepositIndicator;
    entities.ExistingFcrSvesTitleXvi.RepresentativePayeeIndicator =
      representativePayeeIndicator;
    entities.ExistingFcrSvesTitleXvi.CustodyCode = custodyCode;
    entities.ExistingFcrSvesTitleXvi.EstimatedSelfEmploymentAmount =
      estimatedSelfEmploymentAmount;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeNumOfEntries =
      unearnedIncomeNumOfEntries;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode1 =
      unearnedIncomeTypeCode1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd1 =
      unearnedIncomeVerifiCd1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate1 =
      unearnedIncomeStartDate1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate1 =
      unearnedIncomeStopDate1;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode2 =
      unearnedIncomeTypeCode2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd2 =
      unearnedIncomeVerifiCd2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate2 =
      unearnedIncomeStartDate2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate2 =
      unearnedIncomeStopDate2;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode3 =
      unearnedIncomeTypeCode3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd3 =
      unearnedIncomeVerifiCd3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate3 =
      unearnedIncomeStartDate3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate3 =
      unearnedIncomeStopDate3;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode4 =
      unearnedIncomeTypeCode4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd4 =
      unearnedIncomeVerifiCd4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate4 =
      unearnedIncomeStartDate4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate4 =
      unearnedIncomeStopDate4;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode5 =
      unearnedIncomeTypeCode5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd5 =
      unearnedIncomeVerifiCd5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate5 =
      unearnedIncomeStartDate5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate5 =
      unearnedIncomeStopDate5;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode6 =
      unearnedIncomeTypeCode6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd6 =
      unearnedIncomeVerifiCd6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate6 =
      unearnedIncomeStartDate6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate6 =
      unearnedIncomeStopDate6;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode7 =
      unearnedIncomeTypeCode7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd7 =
      unearnedIncomeVerifiCd7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate7 =
      unearnedIncomeStartDate7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate7 =
      unearnedIncomeStopDate7;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode8 =
      unearnedIncomeTypeCode8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd8 =
      unearnedIncomeVerifiCd8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate8 =
      unearnedIncomeStartDate8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate8 =
      unearnedIncomeStopDate8;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode9 =
      unearnedIncomeTypeCode9;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd9 =
      unearnedIncomeVerifiCd9;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate9 =
      unearnedIncomeStartDate9;
    entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate9 =
      unearnedIncomeStopDate9;
    entities.ExistingFcrSvesTitleXvi.PhistNumberOfEntries =
      phistNumberOfEntries;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate1 = phistPaymentDate1;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount1 =
      ssiMonthlyAssistanceAmount1;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag1 =
      phistPaymentPayFlag1;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate2 = phistPaymentDate2;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount2 =
      ssiMonthlyAssistanceAmount2;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag2 =
      phistPaymentPayFlag2;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate3 = phistPaymentDate3;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount3 =
      ssiMonthlyAssistanceAmount3;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag3 =
      phistPaymentPayFlag3;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate4 = phistPaymentDate4;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount4 =
      ssiMonthlyAssistanceAmount4;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag4 =
      phistPaymentPayFlag4;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate5 = phistPaymentDate5;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount5 =
      ssiMonthlyAssistanceAmount5;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag5 =
      phistPaymentPayFlag5;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate6 = phistPaymentDate6;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount6 =
      ssiMonthlyAssistanceAmount6;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag6 =
      phistPaymentPayFlag6;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate7 = phistPaymentDate7;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount7 =
      ssiMonthlyAssistanceAmount7;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag7 =
      phistPaymentPayFlag7;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentDate8 = phistPaymentDate8;
    entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount8 =
      ssiMonthlyAssistanceAmount8;
    entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag8 =
      phistPaymentPayFlag8;
    entities.ExistingFcrSvesTitleXvi.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesTitleXvi.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFcrSvesTitleXvi.Populated = true;
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
    /// A value of AlertGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("alertGenerationSkipFl")]
    public Common AlertGenerationSkipFl
    {
      get => alertGenerationSkipFl ??= new();
      set => alertGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of IwoGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("iwoGenerationSkipFl")]
    public Common IwoGenerationSkipFl
    {
      get => iwoGenerationSkipFl ??= new();
      set => iwoGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleXvi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleXvi")]
    public FcrSvesTitleXvi FcrSvesTitleXvi
    {
      get => fcrSvesTitleXvi ??= new();
      set => fcrSvesTitleXvi = value;
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
    /// A value of TotT16Created.
    /// </summary>
    [JsonPropertyName("totT16Created")]
    public Common TotT16Created
    {
      get => totT16Created ??= new();
      set => totT16Created = value;
    }

    /// <summary>
    /// A value of TotT16Updated.
    /// </summary>
    [JsonPropertyName("totT16Updated")]
    public Common TotT16Updated
    {
      get => totT16Updated ??= new();
      set => totT16Updated = value;
    }

    /// <summary>
    /// A value of TotT16AlertCreated.
    /// </summary>
    [JsonPropertyName("totT16AlertCreated")]
    public Common TotT16AlertCreated
    {
      get => totT16AlertCreated ??= new();
      set => totT16AlertCreated = value;
    }

    /// <summary>
    /// A value of TotT16AlertExists.
    /// </summary>
    [JsonPropertyName("totT16AlertExists")]
    public Common TotT16AlertExists
    {
      get => totT16AlertExists ??= new();
      set => totT16AlertExists = value;
    }

    /// <summary>
    /// A value of TotT16HistCreated.
    /// </summary>
    [JsonPropertyName("totT16HistCreated")]
    public Common TotT16HistCreated
    {
      get => totT16HistCreated ??= new();
      set => totT16HistCreated = value;
    }

    /// <summary>
    /// A value of TotT16HistExists.
    /// </summary>
    [JsonPropertyName("totT16HistExists")]
    public Common TotT16HistExists
    {
      get => totT16HistExists ??= new();
      set => totT16HistExists = value;
    }

    private Common alertGenerationSkipFl;
    private Common iwoGenerationSkipFl;
    private DateWorkArea maxDate;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesTitleXvi fcrSvesTitleXvi;
    private Infrastructure infrastructure;
    private Common totT16Created;
    private Common totT16Updated;
    private Common totT16AlertCreated;
    private Common totT16AlertExists;
    private Common totT16HistCreated;
    private Common totT16HistExists;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public FcrSvesTitleXvi Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of TotHistExistsRecs.
    /// </summary>
    [JsonPropertyName("totHistExistsRecs")]
    public Common TotHistExistsRecs
    {
      get => totHistExistsRecs ??= new();
      set => totHistExistsRecs = value;
    }

    /// <summary>
    /// A value of TotAlertExistsRecs.
    /// </summary>
    [JsonPropertyName("totAlertExistsRecs")]
    public Common TotAlertExistsRecs
    {
      get => totAlertExistsRecs ??= new();
      set => totAlertExistsRecs = value;
    }

    /// <summary>
    /// A value of TotHistRecsCreated.
    /// </summary>
    [JsonPropertyName("totHistRecsCreated")]
    public Common TotHistRecsCreated
    {
      get => totHistRecsCreated ??= new();
      set => totHistRecsCreated = value;
    }

    /// <summary>
    /// A value of TotAlertRecsCreated.
    /// </summary>
    [JsonPropertyName("totAlertRecsCreated")]
    public Common TotAlertRecsCreated
    {
      get => totAlertRecsCreated ??= new();
      set => totAlertRecsCreated = value;
    }

    private FcrSvesTitleXvi null1;
    private Infrastructure infrastructure;
    private DateWorkArea process;
    private Common totHistExistsRecs;
    private Common totAlertExistsRecs;
    private Common totHistRecsCreated;
    private Common totAlertRecsCreated;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("existingFcrSvesGenInfo")]
    public FcrSvesGenInfo ExistingFcrSvesGenInfo
    {
      get => existingFcrSvesGenInfo ??= new();
      set => existingFcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesTitleXvi.
    /// </summary>
    [JsonPropertyName("existingFcrSvesTitleXvi")]
    public FcrSvesTitleXvi ExistingFcrSvesTitleXvi
    {
      get => existingFcrSvesTitleXvi ??= new();
      set => existingFcrSvesTitleXvi = value;
    }

    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesTitleXvi existingFcrSvesTitleXvi;
  }
#endregion
}
