// Program: SI_REFM_PA_REFERRAL_MENU, ID: 371761516, model: 746.
// Short name: SWEREFMP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_REFM_PA_REFERRAL_MENU.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiRefmPaReferralMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REFM_PA_REFERRAL_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRefmPaReferralMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRefmPaReferralMenu.
  /// </summary>
  public SiRefmPaReferralMenu(IContext context, Import import, Export export):
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
    //        M A I N T E N A N C E   L O G
    // Date	  Developer
    // 08/29/95  Alan Hackler		Initial Development
    // 10/16/96  R. Marchman		Add new security and next tran
    // 11/20/96  G. Lofton - MTW	Added OSP information to the Screen.
    // 1/15/97	  Sid Chowdhary		Fixes on OSP display.
    // 04/30/97  JF.Caillouet		Change Current Date  CHDS
    // 07/05/97  H Kennedy             Fixed the display of the Office
    //                                 
    // Service Provider Address error
    //                                 
    // message.
    // ------------------------------------------------------------
    // 03/24/99 W.Campbell             Replaced ZD exit state
    //                                 
    // with a good one.
    // ---------------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.PaReferral.Assign(import.PaReferral);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    MoveOffice(import.Office, export.Office);
    export.OfficeAddress.Assign(import.OfficeAddress);
    export.Next.Number = import.Next.Number;
    MoveStandard(import.Standard, export.Standard);

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // This is not always necessary in the case of a menu procedure..
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "SVPO") || Equal(global.Command, "RETSVPO") || Equal
      (global.Command, "ENTER") || Equal(global.Command, "EXIT") || Equal
      (global.Command, "SIGNOFF"))
    {
    }
    else
    {
      local.SpAddrFound.Flag = "N";

      if (!IsEmpty(export.ServiceProvider.UserId))
      {
      }
      else
      {
        local.Osp.Count = 0;

        if (ReadServiceProvider())
        {
          // Current on Service Provider who is the actual Service
          // Provider presently logged on.
          export.ServiceProvider.Assign(entities.ServiceProvider);
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

        if (ReadServiceProviderAddress2())
        {
          export.ServiceProviderAddress.Assign(entities.ServiceProviderAddress);
          local.SpAddrFound.Flag = "Y";
        }

        foreach(var item in ReadOfficeServiceProvider2())
        {
          ++local.Osp.Count;
        }

        if (local.Osp.Count == 1)
        {
          if (ReadOfficeServiceProvider1())
          {
            export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
          }
          else
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            return;
          }

          if (ReadOffice())
          {
            MoveOffice(entities.Office, export.Office);
          }
          else
          {
            // ---------------------------------------------------
            // 03/24/99 W.Campbell - Replaced ZD exit state
            // with a good one.
            // ---------------------------------------------------
            ExitState = "OFFICE_NF";

            return;
          }

          if (AsChar(local.SpAddrFound.Flag) == 'N')
          {
            if (ReadOfficeAddress2())
            {
              export.OfficeAddress.Assign(entities.OfficeAddress);
              export.ServiceProviderAddress.City = export.OfficeAddress.City;
              export.ServiceProviderAddress.StateProvince =
                export.OfficeAddress.StateProvince;
              export.ServiceProviderAddress.Street1 =
                export.OfficeAddress.Street1;
              export.ServiceProviderAddress.Street2 =
                export.OfficeAddress.Street2 ?? "";
              export.ServiceProviderAddress.Zip = export.OfficeAddress.Zip ?? ""
                ;
              export.ServiceProviderAddress.Zip4 =
                export.OfficeAddress.Zip4 ?? "";
            }
            else
            {
              ExitState = "ADDRESS_NF";
            }
          }
        }
        else if (local.Osp.Count > 1)
        {
          ExitState = "SI0000_MULTI_OSP_EXIST";
        }
        else
        {
          ExitState = "SI0000_NO_OSP_EXISTS";
        }
      }
    }

    if (Equal(global.Command, "RETSVPO"))
    {
      if (!IsEmpty(import.SelectedServiceProvider.UserId))
      {
        export.ServiceProvider.Assign(import.SelectedServiceProvider);
        export.OfficeServiceProvider.
          Assign(import.SelectedOfficeServiceProvider);
        MoveOffice(import.SelectedOffice, export.Office);
        local.SpAddrFound.Flag = "N";

        if (export.ServiceProvider.SystemGeneratedId > 0)
        {
          if (ReadServiceProviderAddress1())
          {
            export.ServiceProviderAddress.
              Assign(entities.ServiceProviderAddress);
          }
          else
          {
            ExitState = "SERVICE_PROVIDER_ADDR_NF";
          }
        }

        if (AsChar(local.SpAddrFound.Flag) == 'N')
        {
          if (export.Office.SystemGeneratedId > 0)
          {
            if (ReadOfficeAddress1())
            {
              ExitState = "ACO_NN0000_ALL_OK";
              export.OfficeAddress.Assign(entities.OfficeAddress);
              export.ServiceProviderAddress.City = export.OfficeAddress.City;
              export.ServiceProviderAddress.StateProvince =
                export.OfficeAddress.StateProvince;
              export.ServiceProviderAddress.Street1 =
                export.OfficeAddress.Street1;
              export.ServiceProviderAddress.Street2 =
                export.OfficeAddress.Street2 ?? "";
              export.ServiceProviderAddress.Zip = export.OfficeAddress.Zip ?? ""
                ;
              export.ServiceProviderAddress.Zip4 =
                export.OfficeAddress.Zip4 ?? "";
            }
            else
            {
              ExitState = "ADDRESS_NF";
            }
          }
        }
      }
      else
      {
        // User linked to SVPO and did not select an Office Service
        // Provider to use for document Return Address processing.
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "XXFMMENU":
        break;
      case "SVPO":
        ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";

        break;
      case "RETSVPO":
        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            export.PaReferral.Number = "";
            export.PaReferral.From = "";
            export.PaReferral.Type1 = "NEW";
            ExitState = "ECO_XFR_TO_PA_REFERRAL_PG1";

            break;
          case 2:
            export.PaReferral.Number = "";
            export.PaReferral.From = "";
            export.PaReferral.Type1 = "REOPEN";
            ExitState = "ECO_XFR_TO_PA_REFERRAL_PG1";

            break;
          case 3:
            export.PaReferral.Number = "";
            export.PaReferral.From = "";
            export.PaReferral.Type1 = "CHANGE";
            ExitState = "ECO_XFR_TO_PA_REFERRAL_PG1";

            break;
          case 4:
            if (IsEmpty(import.PaReferral.Number))
            {
              var field1 = GetField(export.PaReferral, "number");

              field1.Error = true;

              ExitState = "REFERRAL_ID_REQUIRED";

              return;
            }

            if (IsEmpty(import.PaReferral.From) || !
              Equal(import.PaReferral.From, "KAE") && !
              Equal(import.PaReferral.From, "KSC"))
            {
              var field1 = GetField(export.PaReferral, "from");

              field1.Error = true;

              ExitState = "PA_REFERRAL_FROM_REQD";

              return;
            }

            export.PaReferral.Type1 = "";
            ExitState = "ECO_XFR_TO_PA_REFERRAL_PG1";

            break;
          default:
            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeAddress1()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress1",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeAddress2()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress2",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 6);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 8);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 6);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 8);
        entities.OfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress1()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", export.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress2()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Populated = true;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedOfficeServiceProvider")]
    public OfficeServiceProvider SelectedOfficeServiceProvider
    {
      get => selectedOfficeServiceProvider ??= new();
      set => selectedOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    private Standard standard;
    private Security2 security;
    private PaReferral paReferral;
    private Case1 next;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private OfficeAddress officeAddress;
    private ServiceProvider selectedServiceProvider;
    private OfficeServiceProvider selectedOfficeServiceProvider;
    private Office selectedOffice;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private Standard standard;
    private PaReferral paReferral;
    private Case1 next;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private OfficeAddress officeAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Osp.
    /// </summary>
    [JsonPropertyName("osp")]
    public Common Osp
    {
      get => osp ??= new();
      set => osp = value;
    }

    /// <summary>
    /// A value of SpAddrFound.
    /// </summary>
    [JsonPropertyName("spAddrFound")]
    public Common SpAddrFound
    {
      get => spAddrFound ??= new();
      set => spAddrFound = value;
    }

    private DateWorkArea current;
    private DateWorkArea null1;
    private Common osp;
    private Common spAddrFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private OfficeAddress officeAddress;
    private ServiceProviderAddress serviceProviderAddress;
  }
#endregion
}
