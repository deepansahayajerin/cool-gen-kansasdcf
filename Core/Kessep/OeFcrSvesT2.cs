// Program: OE_FCR_SVES_T2, ID: 945077344, model: 746.
// Short name: SWE03662
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_T2.
/// </summary>
[Serializable]
public partial class OeFcrSvesT2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_T2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesT2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesT2.
  /// </summary>
  public OeFcrSvesT2(IContext context, Import import, Export export):
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
      if (ReadFcrSvesTitleIi())
      {
        MoveFcrSvesTitleIi(entities.ExistingFcrSvesTitleIi,
          export.FcrSvesTitleIi);
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleIi.DeferredPaymentDate;
        UseCabFormatDateOnline8();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleIi.InitialTitleIiEntitlementDt;
        UseCabFormatDateOnline7();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleIi.CurrentTitleIiEntitlementDt;
        UseCabFormatDateOnline6();
        local.DateWorkArea.Date =
          entities.ExistingFcrSvesTitleIi.TitleIiSuspendTerminateDt;
        UseCabFormatDateOnline5();
        local.DateWorkArea.Date = entities.ExistingFcrSvesTitleIi.HiStartDate;
        UseCabFormatDateOnline4();
        local.DateWorkArea.Date = entities.ExistingFcrSvesTitleIi.HiStopDate;
        UseCabFormatDateOnline3();
        local.DateWorkArea.Date = entities.ExistingFcrSvesTitleIi.SmiStartDate;
        UseCabFormatDateOnline2();
        local.DateWorkArea.Date = entities.ExistingFcrSvesTitleIi.SmiStopDate;
        UseCabFormatDateOnline1();

        for(export.Mbc.Index = 0; export.Mbc.Index < Export.MbcGroup.Capacity; ++
          export.Mbc.Index)
        {
          if (!export.Mbc.CheckSize())
          {
            break;
          }

          switch(export.Mbc.Index + 1)
          {
            case 1:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType1;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate1;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount1;

              break;
            case 2:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType2;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate2;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount2;

              break;
            case 3:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType3;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate3;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount3;

              break;
            case 4:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType4;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate4;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount4;

              break;
            case 5:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType5;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate5;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount5;

              break;
            case 6:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType6;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate6;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount6;

              break;
            case 7:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType7;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate7;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount7;

              break;
            case 8:
              export.Mbc.Update.GmbcType.MbcType1 =
                entities.ExistingFcrSvesTitleIi.MbcType8;
              local.DateWorkArea.Date =
                entities.ExistingFcrSvesTitleIi.MbcDate8;
              export.Mbc.Update.GmbcAmount.MbcAmount1 =
                entities.ExistingFcrSvesTitleIi.MbcAmount8;

              break;
            default:
              return;
          }

          UseCabFormatDateOnline9();
        }

        export.Mbc.CheckIndex();

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
        else
        {
          export.FcrSvesAddress.AddressLine1 = "Address not available";
          export.FcrSvesAddress.AddressLine2 = "Contact local SSA Office";
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

  private static void MoveFcrSvesTitleIi(FcrSvesTitleIi source,
    FcrSvesTitleIi target)
  {
    target.CanAndBic = source.CanAndBic;
    target.StateCode = source.StateCode;
    target.CountyCode = source.CountyCode;
    target.DirectDepositIndicator = source.DirectDepositIndicator;
    target.LafCode = source.LafCode;
    target.NetMonthlyTitleIiBenefit = source.NetMonthlyTitleIiBenefit;
    target.HiOptionCode = source.HiOptionCode;
    target.SmiOptionCode = source.SmiOptionCode;
    target.CategoryOfAssistance = source.CategoryOfAssistance;
    target.BlackLungEntitlementCode = source.BlackLungEntitlementCode;
    target.BlackLungPaymentAmount = source.BlackLungPaymentAmount;
    target.RailroadIndicator = source.RailroadIndicator;
    target.MbcNumberOfEntries = source.MbcNumberOfEntries;
  }

  private void UseCabFormatDateOnline1()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedSmiStopDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline2()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedSmiStartDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline3()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedHiStopDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline4()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.ExportedFormattedHiStartDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline5()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedSuspDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline6()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedCurEntDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline7()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedInitEntDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline8()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedDefpayDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline9()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.Mbc.Update.FormattedMbcDate.Text10 = useExport.FormattedDate.Text10;
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
        entities.ExistingFcrSvesAddress.AddressScrubIndicator1 =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesAddress.AddressScrubIndicator2 =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrSvesAddress.AddressScrubIndicator3 =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesAddress.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.ExistingFcrSvesAddress.State = db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesAddress.ZipCode5 =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrSvesAddress.ZipCode4 =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrSvesAddress.City = db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesAddress.AddressLine1 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrSvesAddress.AddressLine2 =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesAddress.AddressLine3 =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesAddress.AddressLine4 =
          db.GetNullableString(reader, 14);
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
        entities.ExistingFcrSvesGenInfo.TransmitterStateTerritoryCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesTitleIi()
  {
    entities.ExistingFcrSvesTitleIi.Populated = false;

    return Read("ReadFcrSvesTitleIi",
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
    /// <summary>A MbcGroup group.</summary>
    [Serializable]
    public class MbcGroup
    {
      /// <summary>
      /// A value of GmbcType.
      /// </summary>
      [JsonPropertyName("gmbcType")]
      public FcrSvesTitleIi GmbcType
      {
        get => gmbcType ??= new();
        set => gmbcType = value;
      }

      /// <summary>
      /// A value of GmbcAmount.
      /// </summary>
      [JsonPropertyName("gmbcAmount")]
      public FcrSvesTitleIi GmbcAmount
      {
        get => gmbcAmount ??= new();
        set => gmbcAmount = value;
      }

      /// <summary>
      /// A value of FormattedMbcDate.
      /// </summary>
      [JsonPropertyName("formattedMbcDate")]
      public WorkArea FormattedMbcDate
      {
        get => formattedMbcDate ??= new();
        set => formattedMbcDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private FcrSvesTitleIi gmbcType;
      private FcrSvesTitleIi gmbcAmount;
      private WorkArea formattedMbcDate;
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
    /// A value of FormattedSmiStopDate.
    /// </summary>
    [JsonPropertyName("formattedSmiStopDate")]
    public WorkArea FormattedSmiStopDate
    {
      get => formattedSmiStopDate ??= new();
      set => formattedSmiStopDate = value;
    }

    /// <summary>
    /// A value of FormattedSmiStartDate.
    /// </summary>
    [JsonPropertyName("formattedSmiStartDate")]
    public WorkArea FormattedSmiStartDate
    {
      get => formattedSmiStartDate ??= new();
      set => formattedSmiStartDate = value;
    }

    /// <summary>
    /// A value of FormattedHiStopDate.
    /// </summary>
    [JsonPropertyName("formattedHiStopDate")]
    public WorkArea FormattedHiStopDate
    {
      get => formattedHiStopDate ??= new();
      set => formattedHiStopDate = value;
    }

    /// <summary>
    /// A value of ExportedFormattedHiStartDate.
    /// </summary>
    [JsonPropertyName("exportedFormattedHiStartDate")]
    public WorkArea ExportedFormattedHiStartDate
    {
      get => exportedFormattedHiStartDate ??= new();
      set => exportedFormattedHiStartDate = value;
    }

    /// <summary>
    /// A value of FormattedSuspDate.
    /// </summary>
    [JsonPropertyName("formattedSuspDate")]
    public WorkArea FormattedSuspDate
    {
      get => formattedSuspDate ??= new();
      set => formattedSuspDate = value;
    }

    /// <summary>
    /// A value of FormattedCurEntDate.
    /// </summary>
    [JsonPropertyName("formattedCurEntDate")]
    public WorkArea FormattedCurEntDate
    {
      get => formattedCurEntDate ??= new();
      set => formattedCurEntDate = value;
    }

    /// <summary>
    /// A value of FormattedInitEntDate.
    /// </summary>
    [JsonPropertyName("formattedInitEntDate")]
    public WorkArea FormattedInitEntDate
    {
      get => formattedInitEntDate ??= new();
      set => formattedInitEntDate = value;
    }

    /// <summary>
    /// A value of FormattedDefpayDate.
    /// </summary>
    [JsonPropertyName("formattedDefpayDate")]
    public WorkArea FormattedDefpayDate
    {
      get => formattedDefpayDate ??= new();
      set => formattedDefpayDate = value;
    }

    /// <summary>
    /// Gets a value of Mbc.
    /// </summary>
    [JsonIgnore]
    public Array<MbcGroup> Mbc => mbc ??= new(MbcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Mbc for json serialization.
    /// </summary>
    [JsonPropertyName("mbc")]
    [Computed]
    public IList<MbcGroup> Mbc_Json
    {
      get => mbc;
      set => Mbc.Assign(value);
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
    /// A value of FcrSvesAddress.
    /// </summary>
    [JsonPropertyName("fcrSvesAddress")]
    public FcrSvesAddress FcrSvesAddress
    {
      get => fcrSvesAddress ??= new();
      set => fcrSvesAddress = value;
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

    private WorkArea formattedDate;
    private WorkArea formattedSmiStopDate;
    private WorkArea formattedSmiStartDate;
    private WorkArea formattedHiStopDate;
    private WorkArea exportedFormattedHiStartDate;
    private WorkArea formattedSuspDate;
    private WorkArea formattedCurEntDate;
    private WorkArea formattedInitEntDate;
    private WorkArea formattedDefpayDate;
    private Array<MbcGroup> mbc;
    private WorkArea svesErrorMsgs;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesTitleIi fcrSvesTitleIi;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MbcAmount.
    /// </summary>
    [JsonPropertyName("mbcAmount")]
    public WorkArea MbcAmount
    {
      get => mbcAmount ??= new();
      set => mbcAmount = value;
    }

    /// <summary>
    /// A value of MbcType.
    /// </summary>
    [JsonPropertyName("mbcType")]
    public WorkArea MbcType
    {
      get => mbcType ??= new();
      set => mbcType = value;
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
    /// A value of Person.
    /// </summary>
    [JsonPropertyName("person")]
    public WorkArea Person
    {
      get => person ??= new();
      set => person = value;
    }

    private WorkArea mbcAmount;
    private WorkArea mbcType;
    private DateWorkArea dateWorkArea;
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
    /// A value of ExistingFcrSvesTitleIi.
    /// </summary>
    [JsonPropertyName("existingFcrSvesTitleIi")]
    public FcrSvesTitleIi ExistingFcrSvesTitleIi
    {
      get => existingFcrSvesTitleIi ??= new();
      set => existingFcrSvesTitleIi = value;
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
    private FcrSvesTitleIi existingFcrSvesTitleIi;
    private FcrSvesAddress existingFcrSvesAddress;
  }
#endregion
}
