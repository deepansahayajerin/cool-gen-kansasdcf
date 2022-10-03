// Program: OE_FCR_SVES_T16, ID: 945076066, model: 746.
// Short name: SWE03663
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_T16.
/// </summary>
[Serializable]
public partial class OeFcrSvesT16: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_T16 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesT16(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesT16.
  /// </summary>
  public OeFcrSvesT16(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // * ----------  -----------------  ---------   -----------------------
    // * 07/08/11    LSS                CQ5577      Initial Coding.
    // *
    // ***********************************************************************
    // Fields needing formatted (dates, ssn, phone) are being exported as 
    // formatted work views.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Person.Text15 = "00000" + import.CsePersonsWorkSet.Number;

    if (ReadFcrSvesGenInfo())
    {
      if (ReadFcrSvesTitleXvi())
      {
        if (Equal(entities.ExistingFcrSvesTitleXvi.TelephoneNumber, "9999999999")
          || Equal
          (entities.ExistingFcrSvesTitleXvi.TelephoneNumber, "0000000000"))
        {
          export.FormattedPhone.Text12 = "";
        }
        else
        {
          local.Phone.Text10 =
            entities.ExistingFcrSvesTitleXvi.TelephoneNumber ?? Spaces(10);
          UseCabFormatPhoneOnline();
        }

        MoveFcrSvesTitleXvi(entities.ExistingFcrSvesTitleXvi,
          export.FcrSvesTitleXvi);
        export.FcrSvesGenInfo.ParticipantType =
          entities.ExistingFcrSvesGenInfo.ParticipantType;
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleXvi.RecordEstablishmentDate;
        UseCabFormatDateOnline6();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleXvi.DateOfTitleXviEligibility;
        UseCabFormatDateOnline5();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleXvi.TitleXviDenialDate;
        UseCabFormatDateOnline4();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleXvi.TitleXviLastRedeterminDate;
        UseCabFormatDateOnline3();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleXvi.DateOfTitleXviAppeal;
        UseCabFormatDateOnline2();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleXvi.PaymentStatusDate;
        UseCabFormatDateOnline1();

        for(export.Ui.Index = 0; export.Ui.Index < Export.UiGroup.Capacity; ++
          export.Ui.Index)
        {
          if (!export.Ui.CheckSize())
          {
            break;
          }

          switch(export.Ui.Index + 1)
          {
            case 1:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode1;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd1;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate1;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate1;

              break;
            case 2:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode2;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd2;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate2;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate2;

              break;
            case 3:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode3;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd3;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate3;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate3;

              break;
            case 4:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode4;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd4;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate4;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate4;

              break;
            case 5:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode5;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd5;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate5;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate5;

              break;
            case 6:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode6;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd6;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate6;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate6;

              break;
            case 7:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode7;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd7;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate7;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate7;

              break;
            case 8:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode8;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd8;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate8;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate8;

              break;
            case 9:
              export.Ui.Update.GuiType.UnearnedIncomeTypeCode1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeTypeCode9;
              export.Ui.Update.GuiVerifCode.UnearnedIncomeVerifiCd1 =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeVerifiCd9;
              local.StartDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStartDate9;
              local.EndDate.Date =
                entities.ExistingFcrSvesTitleXvi.UnearnedIncomeStopDate9;

              break;
            default:
              return;
          }

          UseCabFormatDateOnline8();
          UseCabFormatDateOnline9();
        }

        export.Ui.CheckIndex();

        for(export.Phist.Index = 0; export.Phist.Index < Export
          .PhistGroup.Capacity; ++export.Phist.Index)
        {
          if (!export.Phist.CheckSize())
          {
            break;
          }

          switch(export.Phist.Index + 1)
          {
            case 1:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag1;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate1;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount1;

              break;
            case 2:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag2;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate2;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount2;

              break;
            case 3:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag3;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate3;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount3;

              break;
            case 4:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag4;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate4;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount4;

              break;
            case 5:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag5;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate5;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount5;

              break;
            case 6:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag6;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate6;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount6;

              break;
            case 7:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag7;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate7;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount7;

              break;
            case 8:
              export.Phist.Update.GphistType.PhistPaymentPayFlag1 =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentPayFlag8;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleXvi.PhistPaymentDate8;
              export.Phist.Update.GphistAmount.SsiMonthlyAssistanceAmount1 =
                entities.ExistingFcrSvesTitleXvi.SsiMonthlyAssistanceAmount8;

              break;
            default:
              return;
          }

          UseCabFormatDateOnline7();
        }

        export.Phist.CheckIndex();

        // ***********************************************
        // Address types:
        // 01 - Residential Address
        // 02 - Person/Claimant Address
        // 03 - District Office Address
        // 04 - Payee Mailing Address
        // 05 - Prison Address
        // ***********************************************
        if (ReadFcrSvesAddress())
        {
          export.FcrSvesAddress.Assign(entities.ExistingFcrSvesAddress);
        }
      }
      else
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
    }
    else
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
  }

  private static void MoveFcrSvesTitleXvi(FcrSvesTitleXvi source,
    FcrSvesTitleXvi target)
  {
    target.OtherName = source.OtherName;
    target.RaceCode = source.RaceCode;
    target.DateOfDeathSourceCode = source.DateOfDeathSourceCode;
    target.PayeeStateOfJurisdiction = source.PayeeStateOfJurisdiction;
    target.PayeeCountyOfJurisdiction = source.PayeeCountyOfJurisdiction;
    target.PayeeDistrictOfficeCode = source.PayeeDistrictOfficeCode;
    target.TypeOfPayeeCode = source.TypeOfPayeeCode;
    target.TypeOfRecipient = source.TypeOfRecipient;
    target.TitleXviAppealCode = source.TitleXviAppealCode;
    target.CurrentPaymentStatusCode = source.CurrentPaymentStatusCode;
    target.PaymentStatusCode = source.PaymentStatusCode;
    target.ThirdPartyInsuranceIndicator = source.ThirdPartyInsuranceIndicator;
    target.DirectDepositIndicator = source.DirectDepositIndicator;
    target.RepresentativePayeeIndicator = source.RepresentativePayeeIndicator;
    target.CustodyCode = source.CustodyCode;
    target.EstimatedSelfEmploymentAmount = source.EstimatedSelfEmploymentAmount;
  }

  private void UseCabFormatDateOnline1()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedPayStatDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline2()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedAppealDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline3()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedRedeterDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline4()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedDenialDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline5()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedEligDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline6()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedEstabDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline7()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.Phist.Update.FormattedPhPmtDate.Text10 =
      useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline8()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.StartDate.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.Ui.Update.FormattedUiStartDate.Text10 =
      useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline9()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.EndDate.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.Ui.Update.FormattedUiEndDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatPhoneOnline()
  {
    var useImport = new CabFormatPhoneOnline.Import();
    var useExport = new CabFormatPhoneOnline.Export();

    useImport.Phone.Text10 = local.Phone.Text10;

    Call(CabFormatPhoneOnline.Execute, useImport, useExport);

    export.FormattedPhone.Text12 = useExport.FormattedPhone.Text12;
  }

  private bool ReadFcrSvesAddress()
  {
    entities.ExistingFcrSvesAddress.Populated = false;

    return Read("ReadFcrSvesAddress",
      (db, command) =>
      {
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesAddress.FcgMemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesAddress.FcgLSRspAgy = db.GetString(reader, 1);
        entities.ExistingFcrSvesAddress.SvesAddressTypeCode =
          db.GetString(reader, 2);
        entities.ExistingFcrSvesAddress.CreatedBy = db.GetString(reader, 3);
        entities.ExistingFcrSvesAddress.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingFcrSvesAddress.State = db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesAddress.ZipCode5 =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesAddress.ZipCode4 =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesAddress.City = db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesAddress.AddressLine1 =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesAddress.AddressLine2 =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesAddress.AddressLine3 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesAddress.AddressLine4 =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesAddress.Populated = true;
      });
  }

  private bool ReadFcrSvesGenInfo()
  {
    entities.ExistingFcrSvesGenInfo.Populated = false;

    return Read("ReadFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", local.Person.Text15);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.SexCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrSvesGenInfo.ReturnedDateOfBirth =
          db.GetNullableDate(reader, 3);
        entities.ExistingFcrSvesGenInfo.Ssn = db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesGenInfo.FipsCountyCode =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesGenInfo.LocateResponseCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesGenInfo.MultipleSsn =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesGenInfo.ParticipantType =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesGenInfo.ReturnedFirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesGenInfo.ReturnedMiddleName =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesGenInfo.ReturnedLastName =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesGenInfo.UserField =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesTitleXvi()
  {
    entities.ExistingFcrSvesTitleXvi.Populated = false;

    return Read("ReadFcrSvesTitleXvi",
      (db, command) =>
      {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private FcrSvesGenInfo fcrSvesGenInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PhistGroup group.</summary>
    [Serializable]
    public class PhistGroup
    {
      /// <summary>
      /// A value of GphistType.
      /// </summary>
      [JsonPropertyName("gphistType")]
      public FcrSvesTitleXvi GphistType
      {
        get => gphistType ??= new();
        set => gphistType = value;
      }

      /// <summary>
      /// A value of GphistAmount.
      /// </summary>
      [JsonPropertyName("gphistAmount")]
      public FcrSvesTitleXvi GphistAmount
      {
        get => gphistAmount ??= new();
        set => gphistAmount = value;
      }

      /// <summary>
      /// A value of FormattedPhPmtDate.
      /// </summary>
      [JsonPropertyName("formattedPhPmtDate")]
      public WorkArea FormattedPhPmtDate
      {
        get => formattedPhPmtDate ??= new();
        set => formattedPhPmtDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private FcrSvesTitleXvi gphistType;
      private FcrSvesTitleXvi gphistAmount;
      private WorkArea formattedPhPmtDate;
    }

    /// <summary>A UiGroup group.</summary>
    [Serializable]
    public class UiGroup
    {
      /// <summary>
      /// A value of GuiType.
      /// </summary>
      [JsonPropertyName("guiType")]
      public FcrSvesTitleXvi GuiType
      {
        get => guiType ??= new();
        set => guiType = value;
      }

      /// <summary>
      /// A value of GuiVerifCode.
      /// </summary>
      [JsonPropertyName("guiVerifCode")]
      public FcrSvesTitleXvi GuiVerifCode
      {
        get => guiVerifCode ??= new();
        set => guiVerifCode = value;
      }

      /// <summary>
      /// A value of FormattedUiStartDate.
      /// </summary>
      [JsonPropertyName("formattedUiStartDate")]
      public WorkArea FormattedUiStartDate
      {
        get => formattedUiStartDate ??= new();
        set => formattedUiStartDate = value;
      }

      /// <summary>
      /// A value of FormattedUiEndDate.
      /// </summary>
      [JsonPropertyName("formattedUiEndDate")]
      public WorkArea FormattedUiEndDate
      {
        get => formattedUiEndDate ??= new();
        set => formattedUiEndDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private FcrSvesTitleXvi guiType;
      private FcrSvesTitleXvi guiVerifCode;
      private WorkArea formattedUiStartDate;
      private WorkArea formattedUiEndDate;
    }

    /// <summary>
    /// A value of FormattedPhone.
    /// </summary>
    [JsonPropertyName("formattedPhone")]
    public TextWorkArea FormattedPhone
    {
      get => formattedPhone ??= new();
      set => formattedPhone = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of FormattedStartDate.
    /// </summary>
    [JsonPropertyName("formattedStartDate")]
    public WorkArea FormattedStartDate
    {
      get => formattedStartDate ??= new();
      set => formattedStartDate = value;
    }

    /// <summary>
    /// A value of FormattedEndDate.
    /// </summary>
    [JsonPropertyName("formattedEndDate")]
    public WorkArea FormattedEndDate
    {
      get => formattedEndDate ??= new();
      set => formattedEndDate = value;
    }

    /// <summary>
    /// Gets a value of Phist.
    /// </summary>
    [JsonIgnore]
    public Array<PhistGroup> Phist => phist ??= new(PhistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Phist for json serialization.
    /// </summary>
    [JsonPropertyName("phist")]
    [Computed]
    public IList<PhistGroup> Phist_Json
    {
      get => phist;
      set => Phist.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ui.
    /// </summary>
    [JsonIgnore]
    public Array<UiGroup> Ui => ui ??= new(UiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ui for json serialization.
    /// </summary>
    [JsonPropertyName("ui")]
    [Computed]
    public IList<UiGroup> Ui_Json
    {
      get => ui;
      set => Ui.Assign(value);
    }

    /// <summary>
    /// A value of FormattedPayStatDate.
    /// </summary>
    [JsonPropertyName("formattedPayStatDate")]
    public WorkArea FormattedPayStatDate
    {
      get => formattedPayStatDate ??= new();
      set => formattedPayStatDate = value;
    }

    /// <summary>
    /// A value of FormattedAppealDate.
    /// </summary>
    [JsonPropertyName("formattedAppealDate")]
    public WorkArea FormattedAppealDate
    {
      get => formattedAppealDate ??= new();
      set => formattedAppealDate = value;
    }

    /// <summary>
    /// A value of FormattedRedeterDate.
    /// </summary>
    [JsonPropertyName("formattedRedeterDate")]
    public WorkArea FormattedRedeterDate
    {
      get => formattedRedeterDate ??= new();
      set => formattedRedeterDate = value;
    }

    /// <summary>
    /// A value of FormattedDenialDate.
    /// </summary>
    [JsonPropertyName("formattedDenialDate")]
    public WorkArea FormattedDenialDate
    {
      get => formattedDenialDate ??= new();
      set => formattedDenialDate = value;
    }

    /// <summary>
    /// A value of FormattedEligDate.
    /// </summary>
    [JsonPropertyName("formattedEligDate")]
    public WorkArea FormattedEligDate
    {
      get => formattedEligDate ??= new();
      set => formattedEligDate = value;
    }

    /// <summary>
    /// A value of FormattedEstabDate.
    /// </summary>
    [JsonPropertyName("formattedEstabDate")]
    public WorkArea FormattedEstabDate
    {
      get => formattedEstabDate ??= new();
      set => formattedEstabDate = value;
    }

    /// <summary>
    /// A value of SvesErrorMsgs.
    /// </summary>
    [JsonPropertyName("svesErrorMsgs")]
    public WorkArea SvesErrorMsgs
    {
      get => svesErrorMsgs ??= new();
      set => svesErrorMsgs = value;
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
    /// A value of FcrSvesAddress.
    /// </summary>
    [JsonPropertyName("fcrSvesAddress")]
    public FcrSvesAddress FcrSvesAddress
    {
      get => fcrSvesAddress ??= new();
      set => fcrSvesAddress = value;
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

    private TextWorkArea formattedPhone;
    private WorkArea formattedDate;
    private WorkArea formattedStartDate;
    private WorkArea formattedEndDate;
    private Array<PhistGroup> phist;
    private Array<UiGroup> ui;
    private WorkArea formattedPayStatDate;
    private WorkArea formattedAppealDate;
    private WorkArea formattedRedeterDate;
    private WorkArea formattedDenialDate;
    private WorkArea formattedEligDate;
    private WorkArea formattedEstabDate;
    private WorkArea svesErrorMsgs;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesTitleXvi fcrSvesTitleXvi;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public TextWorkArea Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    /// <summary>
    /// A value of Person.
    /// </summary>
    [JsonPropertyName("person")]
    public WorkArea Person
    {
      get => person ??= new();
      set => person = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkArea startDate;
    private DateWorkArea endDate;
    private TextWorkArea phone;
    private WorkArea person;
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

    /// <summary>
    /// A value of ExistingFcrSvesAddress.
    /// </summary>
    [JsonPropertyName("existingFcrSvesAddress")]
    public FcrSvesAddress ExistingFcrSvesAddress
    {
      get => existingFcrSvesAddress ??= new();
      set => existingFcrSvesAddress = value;
    }

    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesTitleXvi existingFcrSvesTitleXvi;
    private FcrSvesAddress existingFcrSvesAddress;
  }
#endregion
}
