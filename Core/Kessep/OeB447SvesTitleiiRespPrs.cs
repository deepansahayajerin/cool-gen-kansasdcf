// Program: OE_B447_SVES_TITLEII_RESP_PRS, ID: 945066134, model: 746.
// Short name: SWE04473
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B447_SVES_TITLEII_RESP_PRS.
/// </para>
/// <para>
/// This Action Block maintains FCR/SVES Title-II information received from FCR 
/// on a daily basis.
/// </para>
/// </summary>
[Serializable]
public partial class OeB447SvesTitleiiRespPrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_SVES_TITLEII_RESP_PRS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447SvesTitleiiRespPrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447SvesTitleiiRespPrs.
  /// </summary>
  public OeB447SvesTitleiiRespPrs(IContext context, Import import, Export export)
    :
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
    // * This Action Block received the SVES Title-II  information from the 
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
    local.Infrastructure.Assign(import.Infrastructure);
    local.Process.Date = import.Infrastructure.ReferenceDate;

    // *******************************************************************************************
    // ** Check whether received Titl-II  record already exists in CSE database 
    // then update     **
    // ** the existing information otherwise create a new Title-II  response 
    // entry to CSE DB.   **
    // *******************************************************************************************
    if (ReadFcrSvesGenInfo())
    {
      if (ReadFcrSvesTitleIi())
      {
        try
        {
          UpdateFcrSvesTitleIi();
          ++import.TotT2Updated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_TITLEII_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_TITLEII_PV";

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
          CreateFcrSvesTitleIi();
          ++import.TotT2Created.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_TITLEII_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_TITLEII_PV";

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
      import.TotT2AlertCreated.Count += local.TotAlertRecsCreated.Count;
      import.TotT2HistCreated.Count += local.TotHistRecsCreated.Count;
      import.TotT2AlertExists.Count += local.TotAlertExistsRecs.Count;
      import.TotT2HistExists.Count += local.TotHistExistsRecs.Count;
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
    useImport.FcrSvesTitleIi.TitleIiSuspendTerminateDt =
      import.FcrSvesTitleIi.TitleIiSuspendTerminateDt;
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ProcessingDate.Date = local.Process.Date;

    Call(OeB447SvesAlertNIwoGen.Execute, useImport, useExport);

    local.TotAlertRecsCreated.Count = useExport.TotAlertRecsCreated.Count;
    local.TotHistRecsCreated.Count = useExport.TotHistRecsCreated.Count;
    local.TotAlertExistsRecs.Count = useExport.TotAlertExistsRecs.Count;
    local.TotHistExistsRecs.Count = useExport.TotHistExistsRecs.Count;
    local.TotArLetterRecs.Count = useExport.TotArLetterRecs.Count;
    local.TotIwoRecs.Count = useExport.TotIwoRecs.Count;
  }

  private void CreateFcrSvesTitleIi()
  {
    var fcgMemberId = entities.ExistingFcrSvesGenInfo.MemberId;
    var fcgLSRspAgy =
      entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo;
    var seqNo = import.FcrSvesTitleIi.SeqNo;
    var canAndBic = import.FcrSvesTitleIi.CanAndBic ?? "";
    var stateCode = import.FcrSvesTitleIi.StateCode ?? "";
    var countyCode = import.FcrSvesTitleIi.CountyCode ?? "";
    var directDepositIndicator =
      import.FcrSvesTitleIi.DirectDepositIndicator ?? "";
    var lafCode = import.FcrSvesTitleIi.LafCode ?? "";
    var deferredPaymentDate = import.FcrSvesTitleIi.DeferredPaymentDate;
    var initialTitleIiEntitlementDt =
      import.FcrSvesTitleIi.InitialTitleIiEntitlementDt;
    var currentTitleIiEntitlementDt =
      import.FcrSvesTitleIi.CurrentTitleIiEntitlementDt;
    var titleIiSuspendTerminateDt =
      import.FcrSvesTitleIi.TitleIiSuspendTerminateDt;
    var netMonthlyTitleIiBenefit =
      import.FcrSvesTitleIi.NetMonthlyTitleIiBenefit.GetValueOrDefault();
    var hiOptionCode = import.FcrSvesTitleIi.HiOptionCode ?? "";
    var hiStartDate = import.FcrSvesTitleIi.HiStartDate;
    var hiStopDate = import.FcrSvesTitleIi.HiStopDate;
    var smiOptionCode = import.FcrSvesTitleIi.SmiOptionCode ?? "";
    var smiStartDate = import.FcrSvesTitleIi.SmiStartDate;
    var smiStopDate = import.FcrSvesTitleIi.SmiStopDate;
    var categoryOfAssistance = import.FcrSvesTitleIi.CategoryOfAssistance ?? "";
    var blackLungEntitlementCode =
      import.FcrSvesTitleIi.BlackLungEntitlementCode ?? "";
    var blackLungPaymentAmount =
      import.FcrSvesTitleIi.BlackLungPaymentAmount.GetValueOrDefault();
    var railroadIndicator = import.FcrSvesTitleIi.RailroadIndicator ?? "";
    var mbcNumberOfEntries =
      import.FcrSvesTitleIi.MbcNumberOfEntries.GetValueOrDefault();
    var mbcDate1 = import.FcrSvesTitleIi.MbcDate1;
    var mbcAmount1 = import.FcrSvesTitleIi.MbcAmount1.GetValueOrDefault();
    var mbcType1 = import.FcrSvesTitleIi.MbcType1 ?? "";
    var mbcDate2 = import.FcrSvesTitleIi.MbcDate2;
    var mbcAmount2 = import.FcrSvesTitleIi.MbcAmount2.GetValueOrDefault();
    var mbcType2 = import.FcrSvesTitleIi.MbcType2 ?? "";
    var mbcDate3 = import.FcrSvesTitleIi.MbcDate3;
    var mbcAmount3 = import.FcrSvesTitleIi.MbcAmount3.GetValueOrDefault();
    var mbcType3 = import.FcrSvesTitleIi.MbcType3 ?? "";
    var mbcDate4 = import.FcrSvesTitleIi.MbcDate4;
    var mbcAmount4 = import.FcrSvesTitleIi.MbcAmount4.GetValueOrDefault();
    var mbcType4 = import.FcrSvesTitleIi.MbcType4 ?? "";
    var mbcDate5 = import.FcrSvesTitleIi.MbcDate5;
    var mbcAmount5 = import.FcrSvesTitleIi.MbcAmount5.GetValueOrDefault();
    var mbcType5 = import.FcrSvesTitleIi.MbcType5 ?? "";
    var mbcDate6 = import.FcrSvesTitleIi.MbcDate6;
    var mbcAmount6 = import.FcrSvesTitleIi.MbcAmount6.GetValueOrDefault();
    var mbcType6 = import.FcrSvesTitleIi.MbcType6 ?? "";
    var mbcDate7 = import.FcrSvesTitleIi.MbcDate7;
    var mbcAmount7 = import.FcrSvesTitleIi.MbcAmount7.GetValueOrDefault();
    var mbcType7 = import.FcrSvesTitleIi.MbcType7 ?? "";
    var mbcDate8 = import.FcrSvesTitleIi.MbcDate8;
    var mbcAmount8 = import.FcrSvesTitleIi.MbcAmount8.GetValueOrDefault();
    var mbcType8 = import.FcrSvesTitleIi.MbcType8 ?? "";
    var createdBy = import.FcrSvesTitleIi.CreatedBy;
    var createdTimestamp = import.FcrSvesTitleIi.CreatedTimestamp;
    var lastUpdatedBy = local.Null1.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Null1.LastUpdatedTimestamp;

    entities.ExistingFcrSvesTitleIi.Populated = false;
    Update("CreateFcrSvesTitleIi",
      (db, command) =>
      {
        db.SetString(command, "fcgMemberId", fcgMemberId);
        db.SetString(command, "fcgLSRspAgy", fcgLSRspAgy);
        db.SetInt32(command, "seqNo", seqNo);
        db.SetNullableString(command, "canAndBic", canAndBic);
        db.SetNullableString(command, "stateCode", stateCode);
        db.SetNullableString(command, "countyCode", countyCode);
        db.
          SetNullableString(command, "directDepositInd", directDepositIndicator);
          
        db.SetNullableString(command, "lafCode", lafCode);
        db.SetNullableDate(command, "deferredPmtDt", deferredPaymentDate);
        db.SetNullableDate(
          command, "initialT2EntlDt", initialTitleIiEntitlementDt);
        db.
          SetNullableDate(command, "curTiiEntlDt", currentTitleIiEntitlementDt);
          
        db.SetNullableDate(command, "t2SuspTrmntDt", titleIiSuspendTerminateDt);
        db.SetNullableDecimal(
          command, "netMoT2Benefit", netMonthlyTitleIiBenefit);
        db.SetNullableString(command, "hiOptionCode", hiOptionCode);
        db.SetNullableDate(command, "hiStartDt", hiStartDate);
        db.SetNullableDate(command, "hiStopDt", hiStopDate);
        db.SetNullableString(command, "smiOptionCode", smiOptionCode);
        db.SetNullableDate(command, "smiStartDt", smiStartDate);
        db.SetNullableDate(command, "smiStopDt", smiStopDate);
        db.SetNullableString(command, "asstCategory", categoryOfAssistance);
        db.SetNullableString(command, "blkLngEntlCd", blackLungEntitlementCode);
        db.SetNullableDecimal(command, "blkLngPmtAmt", blackLungPaymentAmount);
        db.SetNullableString(command, "railroadIndicator", railroadIndicator);
        db.SetNullableInt32(command, "mbcNoEntries", mbcNumberOfEntries);
        db.SetNullableDate(command, "mbc1Dt", mbcDate1);
        db.SetNullableDecimal(command, "mbcAmt1", mbcAmount1);
        db.SetNullableString(command, "mbcType1", mbcType1);
        db.SetNullableDate(command, "mbc2Dt", mbcDate2);
        db.SetNullableDecimal(command, "mbc2Amt", mbcAmount2);
        db.SetNullableString(command, "mbcType2", mbcType2);
        db.SetNullableDate(command, "mbc3Dt", mbcDate3);
        db.SetNullableDecimal(command, "mbc3Amt", mbcAmount3);
        db.SetNullableString(command, "mbcType3", mbcType3);
        db.SetNullableDate(command, "mbc4Dt", mbcDate4);
        db.SetNullableDecimal(command, "mbc4Amt", mbcAmount4);
        db.SetNullableString(command, "mbcType4", mbcType4);
        db.SetNullableDate(command, "mbc5Dt", mbcDate5);
        db.SetNullableDecimal(command, "mbc5Amt", mbcAmount5);
        db.SetNullableString(command, "mbcType5", mbcType5);
        db.SetNullableDate(command, "mbc6Dt", mbcDate6);
        db.SetNullableDecimal(command, "mbc6Amt", mbcAmount6);
        db.SetNullableString(command, "mbcType6", mbcType6);
        db.SetNullableDate(command, "mbc7Dt", mbcDate7);
        db.SetNullableDecimal(command, "mbc7Amt", mbcAmount7);
        db.SetNullableString(command, "mbcType7", mbcType7);
        db.SetNullableDate(command, "mbc8Dt", mbcDate8);
        db.SetNullableDecimal(command, "mbc8Amt", mbcAmount8);
        db.SetNullableString(command, "mbcType8", mbcType8);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
      });

    entities.ExistingFcrSvesTitleIi.FcgMemberId = fcgMemberId;
    entities.ExistingFcrSvesTitleIi.FcgLSRspAgy = fcgLSRspAgy;
    entities.ExistingFcrSvesTitleIi.SeqNo = seqNo;
    entities.ExistingFcrSvesTitleIi.CanAndBic = canAndBic;
    entities.ExistingFcrSvesTitleIi.StateCode = stateCode;
    entities.ExistingFcrSvesTitleIi.CountyCode = countyCode;
    entities.ExistingFcrSvesTitleIi.DirectDepositIndicator =
      directDepositIndicator;
    entities.ExistingFcrSvesTitleIi.LafCode = lafCode;
    entities.ExistingFcrSvesTitleIi.DeferredPaymentDate = deferredPaymentDate;
    entities.ExistingFcrSvesTitleIi.InitialTitleIiEntitlementDt =
      initialTitleIiEntitlementDt;
    entities.ExistingFcrSvesTitleIi.CurrentTitleIiEntitlementDt =
      currentTitleIiEntitlementDt;
    entities.ExistingFcrSvesTitleIi.TitleIiSuspendTerminateDt =
      titleIiSuspendTerminateDt;
    entities.ExistingFcrSvesTitleIi.NetMonthlyTitleIiBenefit =
      netMonthlyTitleIiBenefit;
    entities.ExistingFcrSvesTitleIi.HiOptionCode = hiOptionCode;
    entities.ExistingFcrSvesTitleIi.HiStartDate = hiStartDate;
    entities.ExistingFcrSvesTitleIi.HiStopDate = hiStopDate;
    entities.ExistingFcrSvesTitleIi.SmiOptionCode = smiOptionCode;
    entities.ExistingFcrSvesTitleIi.SmiStartDate = smiStartDate;
    entities.ExistingFcrSvesTitleIi.SmiStopDate = smiStopDate;
    entities.ExistingFcrSvesTitleIi.CategoryOfAssistance = categoryOfAssistance;
    entities.ExistingFcrSvesTitleIi.BlackLungEntitlementCode =
      blackLungEntitlementCode;
    entities.ExistingFcrSvesTitleIi.BlackLungPaymentAmount =
      blackLungPaymentAmount;
    entities.ExistingFcrSvesTitleIi.RailroadIndicator = railroadIndicator;
    entities.ExistingFcrSvesTitleIi.MbcNumberOfEntries = mbcNumberOfEntries;
    entities.ExistingFcrSvesTitleIi.MbcDate1 = mbcDate1;
    entities.ExistingFcrSvesTitleIi.MbcAmount1 = mbcAmount1;
    entities.ExistingFcrSvesTitleIi.MbcType1 = mbcType1;
    entities.ExistingFcrSvesTitleIi.MbcDate2 = mbcDate2;
    entities.ExistingFcrSvesTitleIi.MbcAmount2 = mbcAmount2;
    entities.ExistingFcrSvesTitleIi.MbcType2 = mbcType2;
    entities.ExistingFcrSvesTitleIi.MbcDate3 = mbcDate3;
    entities.ExistingFcrSvesTitleIi.MbcAmount3 = mbcAmount3;
    entities.ExistingFcrSvesTitleIi.MbcType3 = mbcType3;
    entities.ExistingFcrSvesTitleIi.MbcDate4 = mbcDate4;
    entities.ExistingFcrSvesTitleIi.MbcAmount4 = mbcAmount4;
    entities.ExistingFcrSvesTitleIi.MbcType4 = mbcType4;
    entities.ExistingFcrSvesTitleIi.MbcDate5 = mbcDate5;
    entities.ExistingFcrSvesTitleIi.MbcAmount5 = mbcAmount5;
    entities.ExistingFcrSvesTitleIi.MbcType5 = mbcType5;
    entities.ExistingFcrSvesTitleIi.MbcDate6 = mbcDate6;
    entities.ExistingFcrSvesTitleIi.MbcAmount6 = mbcAmount6;
    entities.ExistingFcrSvesTitleIi.MbcType6 = mbcType6;
    entities.ExistingFcrSvesTitleIi.MbcDate7 = mbcDate7;
    entities.ExistingFcrSvesTitleIi.MbcAmount7 = mbcAmount7;
    entities.ExistingFcrSvesTitleIi.MbcType7 = mbcType7;
    entities.ExistingFcrSvesTitleIi.MbcDate8 = mbcDate8;
    entities.ExistingFcrSvesTitleIi.MbcAmount8 = mbcAmount8;
    entities.ExistingFcrSvesTitleIi.MbcType8 = mbcType8;
    entities.ExistingFcrSvesTitleIi.CreatedBy = createdBy;
    entities.ExistingFcrSvesTitleIi.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrSvesTitleIi.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesTitleIi.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesTitleIi.Populated = true;
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

  private bool ReadFcrSvesTitleIi()
  {
    entities.ExistingFcrSvesTitleIi.Populated = false;

    return Read("ReadFcrSvesTitleIi",
      (db, command) =>
      {
        db.SetInt32(command, "seqNo", import.FcrSvesTitleIi.SeqNo);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesTitleIi.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesTitleIi.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesTitleIi.SeqNo = db.GetInt32(reader, 2);
        entities.ExistingFcrSvesTitleIi.CanAndBic =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesTitleIi.StateCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesTitleIi.CountyCode =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesTitleIi.DirectDepositIndicator =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesTitleIi.LafCode =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesTitleIi.DeferredPaymentDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingFcrSvesTitleIi.InitialTitleIiEntitlementDt =
          db.GetNullableDate(reader, 9);
        entities.ExistingFcrSvesTitleIi.CurrentTitleIiEntitlementDt =
          db.GetNullableDate(reader, 10);
        entities.ExistingFcrSvesTitleIi.TitleIiSuspendTerminateDt =
          db.GetNullableDate(reader, 11);
        entities.ExistingFcrSvesTitleIi.NetMonthlyTitleIiBenefit =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingFcrSvesTitleIi.HiOptionCode =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesTitleIi.HiStartDate =
          db.GetNullableDate(reader, 14);
        entities.ExistingFcrSvesTitleIi.HiStopDate =
          db.GetNullableDate(reader, 15);
        entities.ExistingFcrSvesTitleIi.SmiOptionCode =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesTitleIi.SmiStartDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFcrSvesTitleIi.SmiStopDate =
          db.GetNullableDate(reader, 18);
        entities.ExistingFcrSvesTitleIi.CategoryOfAssistance =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrSvesTitleIi.BlackLungEntitlementCode =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrSvesTitleIi.BlackLungPaymentAmount =
          db.GetNullableDecimal(reader, 21);
        entities.ExistingFcrSvesTitleIi.RailroadIndicator =
          db.GetNullableString(reader, 22);
        entities.ExistingFcrSvesTitleIi.MbcNumberOfEntries =
          db.GetNullableInt32(reader, 23);
        entities.ExistingFcrSvesTitleIi.MbcDate1 =
          db.GetNullableDate(reader, 24);
        entities.ExistingFcrSvesTitleIi.MbcAmount1 =
          db.GetNullableDecimal(reader, 25);
        entities.ExistingFcrSvesTitleIi.MbcType1 =
          db.GetNullableString(reader, 26);
        entities.ExistingFcrSvesTitleIi.MbcDate2 =
          db.GetNullableDate(reader, 27);
        entities.ExistingFcrSvesTitleIi.MbcAmount2 =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingFcrSvesTitleIi.MbcType2 =
          db.GetNullableString(reader, 29);
        entities.ExistingFcrSvesTitleIi.MbcDate3 =
          db.GetNullableDate(reader, 30);
        entities.ExistingFcrSvesTitleIi.MbcAmount3 =
          db.GetNullableDecimal(reader, 31);
        entities.ExistingFcrSvesTitleIi.MbcType3 =
          db.GetNullableString(reader, 32);
        entities.ExistingFcrSvesTitleIi.MbcDate4 =
          db.GetNullableDate(reader, 33);
        entities.ExistingFcrSvesTitleIi.MbcAmount4 =
          db.GetNullableDecimal(reader, 34);
        entities.ExistingFcrSvesTitleIi.MbcType4 =
          db.GetNullableString(reader, 35);
        entities.ExistingFcrSvesTitleIi.MbcDate5 =
          db.GetNullableDate(reader, 36);
        entities.ExistingFcrSvesTitleIi.MbcAmount5 =
          db.GetNullableDecimal(reader, 37);
        entities.ExistingFcrSvesTitleIi.MbcType5 =
          db.GetNullableString(reader, 38);
        entities.ExistingFcrSvesTitleIi.MbcDate6 =
          db.GetNullableDate(reader, 39);
        entities.ExistingFcrSvesTitleIi.MbcAmount6 =
          db.GetNullableDecimal(reader, 40);
        entities.ExistingFcrSvesTitleIi.MbcType6 =
          db.GetNullableString(reader, 41);
        entities.ExistingFcrSvesTitleIi.MbcDate7 =
          db.GetNullableDate(reader, 42);
        entities.ExistingFcrSvesTitleIi.MbcAmount7 =
          db.GetNullableDecimal(reader, 43);
        entities.ExistingFcrSvesTitleIi.MbcType7 =
          db.GetNullableString(reader, 44);
        entities.ExistingFcrSvesTitleIi.MbcDate8 =
          db.GetNullableDate(reader, 45);
        entities.ExistingFcrSvesTitleIi.MbcAmount8 =
          db.GetNullableDecimal(reader, 46);
        entities.ExistingFcrSvesTitleIi.MbcType8 =
          db.GetNullableString(reader, 47);
        entities.ExistingFcrSvesTitleIi.CreatedBy = db.GetString(reader, 48);
        entities.ExistingFcrSvesTitleIi.CreatedTimestamp =
          db.GetDateTime(reader, 49);
        entities.ExistingFcrSvesTitleIi.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.ExistingFcrSvesTitleIi.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.ExistingFcrSvesTitleIi.Populated = true;
      });
  }

  private void UpdateFcrSvesTitleIi()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFcrSvesTitleIi.Populated);

    var canAndBic = import.FcrSvesTitleIi.CanAndBic ?? "";
    var stateCode = import.FcrSvesTitleIi.StateCode ?? "";
    var countyCode = import.FcrSvesTitleIi.CountyCode ?? "";
    var directDepositIndicator =
      import.FcrSvesTitleIi.DirectDepositIndicator ?? "";
    var lafCode = import.FcrSvesTitleIi.LafCode ?? "";
    var deferredPaymentDate = import.FcrSvesTitleIi.DeferredPaymentDate;
    var initialTitleIiEntitlementDt =
      import.FcrSvesTitleIi.InitialTitleIiEntitlementDt;
    var currentTitleIiEntitlementDt =
      import.FcrSvesTitleIi.CurrentTitleIiEntitlementDt;
    var titleIiSuspendTerminateDt =
      import.FcrSvesTitleIi.TitleIiSuspendTerminateDt;
    var netMonthlyTitleIiBenefit =
      import.FcrSvesTitleIi.NetMonthlyTitleIiBenefit.GetValueOrDefault();
    var hiOptionCode = import.FcrSvesTitleIi.HiOptionCode ?? "";
    var hiStartDate = import.FcrSvesTitleIi.HiStartDate;
    var hiStopDate = import.FcrSvesTitleIi.HiStopDate;
    var smiOptionCode = import.FcrSvesTitleIi.SmiOptionCode ?? "";
    var smiStartDate = import.FcrSvesTitleIi.SmiStartDate;
    var smiStopDate = import.FcrSvesTitleIi.SmiStopDate;
    var categoryOfAssistance = import.FcrSvesTitleIi.CategoryOfAssistance ?? "";
    var blackLungEntitlementCode =
      import.FcrSvesTitleIi.BlackLungEntitlementCode ?? "";
    var blackLungPaymentAmount =
      import.FcrSvesTitleIi.BlackLungPaymentAmount.GetValueOrDefault();
    var railroadIndicator = import.FcrSvesTitleIi.RailroadIndicator ?? "";
    var mbcNumberOfEntries =
      import.FcrSvesTitleIi.MbcNumberOfEntries.GetValueOrDefault();
    var mbcDate1 = import.FcrSvesTitleIi.MbcDate1;
    var mbcAmount1 = import.FcrSvesTitleIi.MbcAmount1.GetValueOrDefault();
    var mbcType1 = import.FcrSvesTitleIi.MbcType1 ?? "";
    var mbcDate2 = import.FcrSvesTitleIi.MbcDate2;
    var mbcAmount2 = import.FcrSvesTitleIi.MbcAmount2.GetValueOrDefault();
    var mbcType2 = import.FcrSvesTitleIi.MbcType2 ?? "";
    var mbcDate3 = import.FcrSvesTitleIi.MbcDate3;
    var mbcAmount3 = import.FcrSvesTitleIi.MbcAmount3.GetValueOrDefault();
    var mbcType3 = import.FcrSvesTitleIi.MbcType3 ?? "";
    var mbcDate4 = import.FcrSvesTitleIi.MbcDate4;
    var mbcAmount4 = import.FcrSvesTitleIi.MbcAmount4.GetValueOrDefault();
    var mbcType4 = import.FcrSvesTitleIi.MbcType4 ?? "";
    var mbcDate5 = import.FcrSvesTitleIi.MbcDate5;
    var mbcAmount5 = import.FcrSvesTitleIi.MbcAmount5.GetValueOrDefault();
    var mbcType5 = import.FcrSvesTitleIi.MbcType5 ?? "";
    var mbcDate6 = import.FcrSvesTitleIi.MbcDate6;
    var mbcAmount6 = import.FcrSvesTitleIi.MbcAmount6.GetValueOrDefault();
    var mbcType6 = import.FcrSvesTitleIi.MbcType6 ?? "";
    var mbcDate7 = import.FcrSvesTitleIi.MbcDate7;
    var mbcAmount7 = import.FcrSvesTitleIi.MbcAmount7.GetValueOrDefault();
    var mbcType7 = import.FcrSvesTitleIi.MbcType7 ?? "";
    var mbcDate8 = import.FcrSvesTitleIi.MbcDate8;
    var mbcAmount8 = import.FcrSvesTitleIi.MbcAmount8.GetValueOrDefault();
    var mbcType8 = import.FcrSvesTitleIi.MbcType8 ?? "";
    var lastUpdatedBy = import.FcrSvesTitleIi.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.FcrSvesTitleIi.LastUpdatedTimestamp;

    entities.ExistingFcrSvesTitleIi.Populated = false;
    Update("UpdateFcrSvesTitleIi",
      (db, command) =>
      {
        db.SetNullableString(command, "canAndBic", canAndBic);
        db.SetNullableString(command, "stateCode", stateCode);
        db.SetNullableString(command, "countyCode", countyCode);
        db.
          SetNullableString(command, "directDepositInd", directDepositIndicator);
          
        db.SetNullableString(command, "lafCode", lafCode);
        db.SetNullableDate(command, "deferredPmtDt", deferredPaymentDate);
        db.SetNullableDate(
          command, "initialT2EntlDt", initialTitleIiEntitlementDt);
        db.
          SetNullableDate(command, "curTiiEntlDt", currentTitleIiEntitlementDt);
          
        db.SetNullableDate(command, "t2SuspTrmntDt", titleIiSuspendTerminateDt);
        db.SetNullableDecimal(
          command, "netMoT2Benefit", netMonthlyTitleIiBenefit);
        db.SetNullableString(command, "hiOptionCode", hiOptionCode);
        db.SetNullableDate(command, "hiStartDt", hiStartDate);
        db.SetNullableDate(command, "hiStopDt", hiStopDate);
        db.SetNullableString(command, "smiOptionCode", smiOptionCode);
        db.SetNullableDate(command, "smiStartDt", smiStartDate);
        db.SetNullableDate(command, "smiStopDt", smiStopDate);
        db.SetNullableString(command, "asstCategory", categoryOfAssistance);
        db.SetNullableString(command, "blkLngEntlCd", blackLungEntitlementCode);
        db.SetNullableDecimal(command, "blkLngPmtAmt", blackLungPaymentAmount);
        db.SetNullableString(command, "railroadIndicator", railroadIndicator);
        db.SetNullableInt32(command, "mbcNoEntries", mbcNumberOfEntries);
        db.SetNullableDate(command, "mbc1Dt", mbcDate1);
        db.SetNullableDecimal(command, "mbcAmt1", mbcAmount1);
        db.SetNullableString(command, "mbcType1", mbcType1);
        db.SetNullableDate(command, "mbc2Dt", mbcDate2);
        db.SetNullableDecimal(command, "mbc2Amt", mbcAmount2);
        db.SetNullableString(command, "mbcType2", mbcType2);
        db.SetNullableDate(command, "mbc3Dt", mbcDate3);
        db.SetNullableDecimal(command, "mbc3Amt", mbcAmount3);
        db.SetNullableString(command, "mbcType3", mbcType3);
        db.SetNullableDate(command, "mbc4Dt", mbcDate4);
        db.SetNullableDecimal(command, "mbc4Amt", mbcAmount4);
        db.SetNullableString(command, "mbcType4", mbcType4);
        db.SetNullableDate(command, "mbc5Dt", mbcDate5);
        db.SetNullableDecimal(command, "mbc5Amt", mbcAmount5);
        db.SetNullableString(command, "mbcType5", mbcType5);
        db.SetNullableDate(command, "mbc6Dt", mbcDate6);
        db.SetNullableDecimal(command, "mbc6Amt", mbcAmount6);
        db.SetNullableString(command, "mbcType6", mbcType6);
        db.SetNullableDate(command, "mbc7Dt", mbcDate7);
        db.SetNullableDecimal(command, "mbc7Amt", mbcAmount7);
        db.SetNullableString(command, "mbcType7", mbcType7);
        db.SetNullableDate(command, "mbc8Dt", mbcDate8);
        db.SetNullableDecimal(command, "mbc8Amt", mbcAmount8);
        db.SetNullableString(command, "mbcType8", mbcType8);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesTitleIi.FcgMemberId);
        db.SetString(
          command, "fcgLSRspAgy", entities.ExistingFcrSvesTitleIi.FcgLSRspAgy);
        db.SetInt32(command, "seqNo", entities.ExistingFcrSvesTitleIi.SeqNo);
      });

    entities.ExistingFcrSvesTitleIi.CanAndBic = canAndBic;
    entities.ExistingFcrSvesTitleIi.StateCode = stateCode;
    entities.ExistingFcrSvesTitleIi.CountyCode = countyCode;
    entities.ExistingFcrSvesTitleIi.DirectDepositIndicator =
      directDepositIndicator;
    entities.ExistingFcrSvesTitleIi.LafCode = lafCode;
    entities.ExistingFcrSvesTitleIi.DeferredPaymentDate = deferredPaymentDate;
    entities.ExistingFcrSvesTitleIi.InitialTitleIiEntitlementDt =
      initialTitleIiEntitlementDt;
    entities.ExistingFcrSvesTitleIi.CurrentTitleIiEntitlementDt =
      currentTitleIiEntitlementDt;
    entities.ExistingFcrSvesTitleIi.TitleIiSuspendTerminateDt =
      titleIiSuspendTerminateDt;
    entities.ExistingFcrSvesTitleIi.NetMonthlyTitleIiBenefit =
      netMonthlyTitleIiBenefit;
    entities.ExistingFcrSvesTitleIi.HiOptionCode = hiOptionCode;
    entities.ExistingFcrSvesTitleIi.HiStartDate = hiStartDate;
    entities.ExistingFcrSvesTitleIi.HiStopDate = hiStopDate;
    entities.ExistingFcrSvesTitleIi.SmiOptionCode = smiOptionCode;
    entities.ExistingFcrSvesTitleIi.SmiStartDate = smiStartDate;
    entities.ExistingFcrSvesTitleIi.SmiStopDate = smiStopDate;
    entities.ExistingFcrSvesTitleIi.CategoryOfAssistance = categoryOfAssistance;
    entities.ExistingFcrSvesTitleIi.BlackLungEntitlementCode =
      blackLungEntitlementCode;
    entities.ExistingFcrSvesTitleIi.BlackLungPaymentAmount =
      blackLungPaymentAmount;
    entities.ExistingFcrSvesTitleIi.RailroadIndicator = railroadIndicator;
    entities.ExistingFcrSvesTitleIi.MbcNumberOfEntries = mbcNumberOfEntries;
    entities.ExistingFcrSvesTitleIi.MbcDate1 = mbcDate1;
    entities.ExistingFcrSvesTitleIi.MbcAmount1 = mbcAmount1;
    entities.ExistingFcrSvesTitleIi.MbcType1 = mbcType1;
    entities.ExistingFcrSvesTitleIi.MbcDate2 = mbcDate2;
    entities.ExistingFcrSvesTitleIi.MbcAmount2 = mbcAmount2;
    entities.ExistingFcrSvesTitleIi.MbcType2 = mbcType2;
    entities.ExistingFcrSvesTitleIi.MbcDate3 = mbcDate3;
    entities.ExistingFcrSvesTitleIi.MbcAmount3 = mbcAmount3;
    entities.ExistingFcrSvesTitleIi.MbcType3 = mbcType3;
    entities.ExistingFcrSvesTitleIi.MbcDate4 = mbcDate4;
    entities.ExistingFcrSvesTitleIi.MbcAmount4 = mbcAmount4;
    entities.ExistingFcrSvesTitleIi.MbcType4 = mbcType4;
    entities.ExistingFcrSvesTitleIi.MbcDate5 = mbcDate5;
    entities.ExistingFcrSvesTitleIi.MbcAmount5 = mbcAmount5;
    entities.ExistingFcrSvesTitleIi.MbcType5 = mbcType5;
    entities.ExistingFcrSvesTitleIi.MbcDate6 = mbcDate6;
    entities.ExistingFcrSvesTitleIi.MbcAmount6 = mbcAmount6;
    entities.ExistingFcrSvesTitleIi.MbcType6 = mbcType6;
    entities.ExistingFcrSvesTitleIi.MbcDate7 = mbcDate7;
    entities.ExistingFcrSvesTitleIi.MbcAmount7 = mbcAmount7;
    entities.ExistingFcrSvesTitleIi.MbcType7 = mbcType7;
    entities.ExistingFcrSvesTitleIi.MbcDate8 = mbcDate8;
    entities.ExistingFcrSvesTitleIi.MbcAmount8 = mbcAmount8;
    entities.ExistingFcrSvesTitleIi.MbcType8 = mbcType8;
    entities.ExistingFcrSvesTitleIi.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesTitleIi.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingFcrSvesTitleIi.Populated = true;
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
    /// A value of FcrSvesTitleIi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIi")]
    public FcrSvesTitleIi FcrSvesTitleIi
    {
      get => fcrSvesTitleIi ??= new();
      set => fcrSvesTitleIi = value;
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
    /// A value of TotT2Created.
    /// </summary>
    [JsonPropertyName("totT2Created")]
    public Common TotT2Created
    {
      get => totT2Created ??= new();
      set => totT2Created = value;
    }

    /// <summary>
    /// A value of TotT2Updated.
    /// </summary>
    [JsonPropertyName("totT2Updated")]
    public Common TotT2Updated
    {
      get => totT2Updated ??= new();
      set => totT2Updated = value;
    }

    /// <summary>
    /// A value of TotT2AlertCreated.
    /// </summary>
    [JsonPropertyName("totT2AlertCreated")]
    public Common TotT2AlertCreated
    {
      get => totT2AlertCreated ??= new();
      set => totT2AlertCreated = value;
    }

    /// <summary>
    /// A value of TotT2AlertExists.
    /// </summary>
    [JsonPropertyName("totT2AlertExists")]
    public Common TotT2AlertExists
    {
      get => totT2AlertExists ??= new();
      set => totT2AlertExists = value;
    }

    /// <summary>
    /// A value of TotT2HistCreated.
    /// </summary>
    [JsonPropertyName("totT2HistCreated")]
    public Common TotT2HistCreated
    {
      get => totT2HistCreated ??= new();
      set => totT2HistCreated = value;
    }

    /// <summary>
    /// A value of TotT2HistExists.
    /// </summary>
    [JsonPropertyName("totT2HistExists")]
    public Common TotT2HistExists
    {
      get => totT2HistExists ??= new();
      set => totT2HistExists = value;
    }

    private Common alertGenerationSkipFl;
    private Common iwoGenerationSkipFl;
    private DateWorkArea maxDate;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesTitleIi fcrSvesTitleIi;
    private Infrastructure infrastructure;
    private Common totT2Created;
    private Common totT2Updated;
    private Common totT2AlertCreated;
    private Common totT2AlertExists;
    private Common totT2HistCreated;
    private Common totT2HistExists;
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
    /// A value of TotIwoRecs.
    /// </summary>
    [JsonPropertyName("totIwoRecs")]
    public Common TotIwoRecs
    {
      get => totIwoRecs ??= new();
      set => totIwoRecs = value;
    }

    /// <summary>
    /// A value of TotArLetterRecs.
    /// </summary>
    [JsonPropertyName("totArLetterRecs")]
    public Common TotArLetterRecs
    {
      get => totArLetterRecs ??= new();
      set => totArLetterRecs = value;
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

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public FcrSvesTitleIi Null1
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

    private Common totIwoRecs;
    private Common totArLetterRecs;
    private Common totHistExistsRecs;
    private Common totAlertExistsRecs;
    private Common totHistRecsCreated;
    private Common totAlertRecsCreated;
    private FcrSvesTitleIi null1;
    private Infrastructure infrastructure;
    private DateWorkArea process;
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
    /// A value of ExistingFcrSvesTitleIi.
    /// </summary>
    [JsonPropertyName("existingFcrSvesTitleIi")]
    public FcrSvesTitleIi ExistingFcrSvesTitleIi
    {
      get => existingFcrSvesTitleIi ??= new();
      set => existingFcrSvesTitleIi = value;
    }

    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesTitleIi existingFcrSvesTitleIi;
  }
#endregion
}
