// Program: SI_ISTM_INTERSTATE_REFERRAL_MENU, ID: 1902598459, model: 746.
// Short name: SWEISTMP
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
/// A program: SI_ISTM_INTERSTATE_REFERRAL_MENU.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIstmInterstateReferralMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ISTM_INTERSTATE_REFERRAL_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIstmInterstateReferralMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIstmInterstateReferralMenu.
  /// </summary>
  public SiIstmInterstateReferralMenu(IContext context, Import import,
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
    // Date	  Developer Name	Description
    // 03-01-95  J.W. Hays - MTW	Initial Development
    // 05-08-95  Ken Evans - MTW	Continue development
    // 01-23-96  J. Howard - SRS	Retrofit
    // 10/16/96  R. Marchman - MTW	Add new security and next tran
    // 11/20/96  G. Lofton - MTW	Added OSP information to
    // 				screen.
    // 04/29/97  J. Howard - DIR       Current date fix
    // 07/07/99  C. Scroggins - DIR    Modified Reads to Select Only
    // 05/13/02  M.Ashworth     	PR140965 Display details from HIST to ISTM.
    // ------------------------------------------------------------
    // 12/14/00 swsrchf   000238       Alert Navigation changes
    // ------------------------------------------------------------
    // *********************************************
    // This PRAD displays the menu to process
    // incoming CSENet Referrals. Referral number
    // is required for option 4 (viewing). Referral
    // type is required for option 5 (printing).
    // *********************************************
    // *************************************************************
    //              Madhu Kumar
    // *************************************************************
    local.Current.Date = Now().Date;
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);
    export.InterstateCase.Assign(import.InterstateCase);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    MoveOffice(import.Office, export.Office);
    export.OfficeAddress.Assign(import.OfficeAddress);
    export.Next.Number = import.Next.Number;
    MoveFips(import.Fips, export.Fips);
    export.StatePrompt.Flag = import.StatePrompt.Flag;

    // -----------------------------------------------------------
    // if the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. now validate
    // -----------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = import.Next.Number;
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
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);

        // *** Work request  000238
        // *** 12/14/00 swsrchf
        // *** start
        // *******************************************************************
        // 05/13/02  SWSRMCA      PR140965 Display details from HIST to ISTM. 
        // SRPT is the ISTM screen
        // *******************************************************************
        if (Equal(export.Hidden.LastTran, "SRPQ") || Equal
          (export.Hidden.LastTran, "SRPT"))
        {
          export.InterstateCase.TransSerialNumber =
            export.Hidden.MiscNum1.GetValueOrDefault();
        }

        // *** end
        // *** 12/14/00 swsrchf
        // *** Work request  000238
      }
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(export.StatePrompt.Flag))
      {
        export.Fips.StateAbbreviation = import.SelectedCodeValue.Cdvalue;
        export.StatePrompt.Flag = "";
        local.State.State = export.Fips.StateAbbreviation;
        UseSiValidateStateFips();

        if (AsChar(local.FipsError.Flag) == 'Y')
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          return;
        }
        else
        {
          export.InterstateCase.OtherFipsState = export.OtherState.State;
        }
      }

      return;
    }

    if (Equal(global.Command, "SVPO") || Equal(global.Command, "RETSVPO") || Equal
      (global.Command, "EXIT") || Equal(global.Command, "SIGNOFF"))
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
            ExitState = "FN0000_OFFICE_NF";

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
            local.SpAddrFound.Flag = "Y";
          }
        }

        if (AsChar(local.SpAddrFound.Flag) == 'N')
        {
          if (export.Office.SystemGeneratedId > 0)
          {
            if (ReadOfficeAddress1())
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
      }
      else
      {
        // User linked to SVPO and did not select an Office Service
        // Provider to use for document Return Address processing.
      }
    }

    if (!IsEmpty(export.Fips.StateAbbreviation))
    {
      local.State.State = export.Fips.StateAbbreviation;
      UseSiValidateStateFips();

      if (AsChar(local.FipsError.Flag) == 'Y')
      {
        var field = GetField(export.Fips, "stateAbbreviation");

        field.Error = true;

        return;
      }
      else
      {
        export.InterstateCase.OtherFipsState = export.OtherState.State;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "XXNEXTXX":
        break;
      case "XXFMMENU":
        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "SVPO":
        ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";

        break;
      case "RETSVPO":
        break;
      case "LIST":
        if (!IsEmpty(export.StatePrompt.Flag))
        {
          if (AsChar(export.StatePrompt.Flag) == 'S')
          {
            export.Prompt.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.StatePrompt, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
          }
        }

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            if (export.InterstateCase.TransSerialNumber != 0)
            {
              var field1 = GetField(export.InterstateCase, "transSerialNumber");

              field1.Error = true;

              ExitState = "SI0000_REFERRAL_NUMBER_NOT_VALID";

              return;
            }

            export.InterstateCase.TransSerialNumber = 0;
            export.InterstateCase.ActionCode = "R";

            if (!IsEmpty(export.InterstateCase.InterstateCaseId) || export
              .InterstateCase.OtherFipsState > 0)
            {
              local.InterstateCase.InterstateCaseId = "";
              local.Read.InterstateCaseId =
                TrimEnd(export.InterstateCase.InterstateCaseId) + "%%%%%%%%%%%%%%%"
                ;

              if (ReadInterstateCase1())
              {
                local.InterstateCase.InterstateCaseId =
                  entities.InterstateCase.InterstateCaseId;
              }

              if (!IsEmpty(local.InterstateCase.InterstateCaseId))
              {
              }
              else
              {
                if (!IsEmpty(export.InterstateCase.InterstateCaseId))
                {
                  var field1 =
                    GetField(export.InterstateCase, "interstateCaseId");

                  field1.Error = true;
                }

                if (export.InterstateCase.OtherFipsState > 0)
                {
                  var field1 = GetField(export.Fips, "stateAbbreviation");

                  field1.Error = true;
                }

                ExitState = "CSENET_REFERRAL_CASE_NF";

                return;
              }
            }

            break;
          case 2:
            if (export.InterstateCase.TransSerialNumber != 0)
            {
              var field1 = GetField(export.InterstateCase, "transSerialNumber");

              field1.Error = true;

              ExitState = "SI0000_REFERRAL_NUMBER_NOT_VALID";

              return;
            }

            if (!IsEmpty(export.InterstateCase.InterstateCaseId) || export
              .InterstateCase.OtherFipsState > 0)
            {
              if (!IsEmpty(export.InterstateCase.InterstateCaseId))
              {
                var field1 =
                  GetField(export.InterstateCase, "interstateCaseId");

                field1.Error = true;
              }

              if (export.InterstateCase.OtherFipsState > 0)
              {
                var field1 = GetField(export.Fips, "stateAbbreviation");

                field1.Error = true;
              }

              ExitState = "SI0000_CASE_ID_STATE_CODE_INVALI";

              return;
            }

            export.InterstateCase.TransSerialNumber = 0;
            export.InterstateCase.ActionCode = "U";

            break;
          case 3:
            if (export.InterstateCase.TransSerialNumber != 0)
            {
              var field1 = GetField(export.InterstateCase, "transSerialNumber");

              field1.Error = true;

              ExitState = "SI0000_REFERRAL_NUMBER_NOT_VALID";

              return;
            }

            if (!IsEmpty(export.InterstateCase.InterstateCaseId) || export
              .InterstateCase.OtherFipsState > 0)
            {
              if (!IsEmpty(export.InterstateCase.InterstateCaseId))
              {
                var field1 =
                  GetField(export.InterstateCase, "interstateCaseId");

                field1.Error = true;
              }

              if (export.InterstateCase.OtherFipsState > 0)
              {
                var field1 = GetField(export.Fips, "stateAbbreviation");

                field1.Error = true;
              }

              ExitState = "SI0000_CASE_ID_STATE_CODE_INVALI";

              return;
            }

            export.InterstateCase.TransSerialNumber = 0;
            export.InterstateCase.ActionCode = "P";

            break;
          case 4:
            if (import.InterstateCase.TransSerialNumber == 0)
            {
              var field1 = GetField(export.InterstateCase, "transSerialNumber");

              field1.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            if (!IsEmpty(export.InterstateCase.InterstateCaseId) || export
              .InterstateCase.OtherFipsState > 0)
            {
              if (!IsEmpty(export.InterstateCase.InterstateCaseId))
              {
                var field1 =
                  GetField(export.InterstateCase, "interstateCaseId");

                field1.Error = true;
              }

              if (export.InterstateCase.OtherFipsState > 0)
              {
                var field1 = GetField(export.Fips, "stateAbbreviation");

                field1.Error = true;
              }

              ExitState = "SI0000_CASE_ID_STATE_CODE_INVALI";

              return;
            }

            // *****************************************************************
            //    Code changes as per PR# 117267 --  Madhu Kumar
            // *****************************************************************
            local.RefCount.Count = 0;

            if (ReadInterstateCase2())
            {
              ++local.RefCount.Count;
              export.InterstateCase.Assign(entities.InterstateCase);
              export.InterstateCase.ActionCode = "";
            }

            if (local.RefCount.Count == 0)
            {
              var field1 = GetField(export.InterstateCase, "transSerialNumber");

              field1.Error = true;

              ExitState = "SI0000_CSENET_REFERRAL_NUMBER_NF";

              return;
            }

            // *******************************************************************
            //    Code changes as required  for PR# 117267 --  Madhu Kumar
            // *******************************************************************
            break;
          default:
            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            return;
        }

        ExitState = "ECO_XFR_TO_CSENET_REFERRAL_CASE";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
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
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseSiValidateStateFips()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    useImport.Common.State = local.State.State;

    Call(SiValidateStateFips.Execute, useImport, useExport);

    local.FipsError.Flag = useExport.Error.Flag;
    export.OtherState.Assign(useExport.Fips);
  }

  private bool ReadInterstateCase1()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "interstateCaseId1", local.Read.InterstateCaseId ?? "");
        db.SetNullableString(
          command, "interstateCaseId2",
          export.InterstateCase.InterstateCaseId ?? "");
        db.SetInt32(
          command, "otherFipsState", export.InterstateCase.OtherFipsState);
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCase.ActionCode = db.GetString(reader, 2);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 3);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateCase2()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase2",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", export.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCase.ActionCode = db.GetString(reader, 2);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 3);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;
      });
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

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

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    private Fips fips;
    private Common statePrompt;
    private InterstateCase interstateCase;
    private Case1 next;
    private Standard standard;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private OfficeAddress officeAddress;
    private ServiceProvider selectedServiceProvider;
    private OfficeServiceProvider selectedOfficeServiceProvider;
    private Office selectedOffice;
    private CodeValue selectedCodeValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private Fips fips;
    private Common statePrompt;
    private InterstateCase interstateCase;
    private Case1 next;
    private Standard standard;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private OfficeAddress officeAddress;
    private Fips otherState;
    private Code prompt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public InterstateCase Read
    {
      get => read ??= new();
      set => read = value;
    }

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
    /// A value of RefCount.
    /// </summary>
    [JsonPropertyName("refCount")]
    public Common RefCount
    {
      get => refCount ??= new();
      set => refCount = value;
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
    /// A value of SpAddrFound.
    /// </summary>
    [JsonPropertyName("spAddrFound")]
    public Common SpAddrFound
    {
      get => spAddrFound ??= new();
      set => spAddrFound = value;
    }

    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
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
    /// A value of Osp.
    /// </summary>
    [JsonPropertyName("osp")]
    public Common Osp
    {
      get => osp ??= new();
      set => osp = value;
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
    /// A value of FipsError.
    /// </summary>
    [JsonPropertyName("fipsError")]
    public Common FipsError
    {
      get => fipsError ??= new();
      set => fipsError = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Common State
    {
      get => state ??= new();
      set => state = value;
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

    private DateWorkArea max;
    private InterstateCase read;
    private InterstateCase interstateCase;
    private Common refCount;
    private DateWorkArea current;
    private Common spAddrFound;
    private OutDocRtrnAddr outDocRtrnAddr;
    private OfficeServiceProvider officeServiceProvider;
    private Common osp;
    private DateWorkArea null1;
    private Common fipsError;
    private Common state;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
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

    private InterstateCase interstateCase;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProviderAddress serviceProviderAddress;
    private Office office;
    private OfficeAddress officeAddress;
  }
#endregion
}
