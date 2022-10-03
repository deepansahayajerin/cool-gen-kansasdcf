// Program: SI_IAPI_CSENET_REF_AP_ID_DATA, ID: 373416901, model: 746.
// Short name: SWEIAPIP
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
/// A program: SI_IAPI_CSENET_REF_AP_ID_DATA.
/// </para>
/// <para>
/// RESP: SRVINIT
/// See JW
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIapiCsenetRefApIdData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IAPI_CSENET_REF_AP_ID_DATA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIapiCsenetRefApIdData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIapiCsenetRefApIdData.
  /// </summary>
  public SiIapiCsenetRefApIdData(IContext context, Import import, Export export):
    
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
    // 02-01-96  J. Howard - SRS	Retrofit
    // 08-27-96  Sid Chowdhary - MTW	Add call to CAB to create
    // 				interstate request.
    // 11/05/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 07/07/99  C. Scroggins -        DPMSD Modified IIDC option not to force
    //                                 
    // name search and made reads
    // Select Only.
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // 11-25-02  M.Lachowicz	        PR 164188.
    //                                 
    // Always pass sex code and DOB to
    // NAME.
    // ------------------------------------------------------------
    // *********************************************
    // * This PRAD displays AP identification data *
    // * from a CSENet Referral.  There are no     *
    // * enterable or selectable fields on this    *
    // * screen.
    // 
    // *
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.ZeroDate.Date = null;
    export.Hidden.Assign(import.Hidden);
    export.InterstateCase.Assign(import.InterstateCase);
    export.SubscrpSave.Count = import.SubscrpSave.Count;
    export.InterstateApIdentification.Assign(import.InterstateApIdentification);
    export.Ap.Text37 = import.Ap.Text37;
    export.Search.Assign(import.Search);
    export.HiddenApid.Assign(import.HiddenApid);
    export.NamelistInd.Flag = import.NamelistInd.Flag;
    export.Regi.Number = import.Regi.Number;
    export.RegiNewCase.Flag = import.RegiNewCase.Flag;
    export.FromIapi.Flag = import.FromIapi.Flag;
    export.ServiceProvider.Assign(import.ServiceProvider);
    MoveOffice(import.Office, export.Office);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);
    export.PaperFileReceivedDate.ReferenceDate =
      import.PaperFileReceivedDate.ReferenceDate;
    export.PrevPaperFileRecDate.ReferenceDate =
      import.PrevPaperFileRecDate.ReferenceDate;
    local.ApAdded.Flag = "N";
    export.NameGroupList.Index = -1;
    export.ToRegi.Index = -1;

    if (!import.NameGroupList.IsEmpty)
    {
      for(import.NameGroupList.Index = 0; import.NameGroupList.Index < import
        .NameGroupList.Count; ++import.NameGroupList.Index)
      {
        if (!import.NameGroupList.CheckSize())
        {
          break;
        }

        ++export.NameGroupList.Index;
        export.NameGroupList.CheckSize();

        export.NameGroupList.Update.N.Assign(import.NameGroupList.Item.N);
        export.NameGroupList.Update.NdetailCase.Type1 =
          import.NameGroupList.Item.NdetailCase.Type1;

        if (Equal(export.NameGroupList.Item.NdetailCase.Type1, "AP"))
        {
          local.ApAdded.Flag = "Y";
        }

        if (!Equal(export.NameGroupList.Item.NdetailCase.Type1, "AP") && !
          Equal(export.NameGroupList.Item.NdetailCase.Type1, "AR") && !
          Equal(export.NameGroupList.Item.NdetailCase.Type1, "CH"))
        {
          export.NameGroupList.Update.NdetailCase.Type1 = "";
        }

        export.NameGroupList.Update.NdetailFamily.Type1 =
          import.NameGroupList.Item.NdetailFamily.Type1;

        if (!Equal(export.NameGroupList.Item.NdetailFamily.Type1, "MO") && !
          Equal(export.NameGroupList.Item.NdetailFamily.Type1, "FA"))
        {
          export.NameGroupList.Update.NdetailFamily.Type1 = "";
        }
      }

      import.NameGroupList.CheckIndex();
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

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
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "IIDC":
        break;
      case "REGI":
        break;
      case "IAPH":
        break;
      case "ISUP":
        break;
      case "IMIS":
        break;
      case "SCNX":
        break;
      case "NAME":
        break;
      case "RETNAME":
        break;
      case "RETREGI":
        break;
      case "UPDATE":
        break;
      default:
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETREGI":
        if (!IsEmpty(export.Regi.Number))
        {
          export.InterstateCase.KsCaseId = export.Regi.Number;
          export.InterstateCase.AssnDeactInd = "D";
          ExitState = "SI0000_REFERRAL_DEACTIVATED";

          return;
        }

        // ***3/15/99 This code has been commented out . As of this date the 
        // following will be handled in REGI. Do not delete until it has been
        // validated that the process works correctly in REGI.
        break;
      case "DISPLAY":
        UseSiIapiDisplayCsenetData();
        export.PrevPaperFileRecDate.ReferenceDate =
          export.PaperFileReceivedDate.ReferenceDate;

        if (Equal(export.PaperFileReceivedDate.ReferenceDate,
          local.ZeroDate.Date))
        {
          export.PaperFileReceivedDate.ReferenceDate = local.Null1.Date;
        }

        if (IsExitState("CSENET_AP_ID_NF"))
        {
          var field = GetField(export.Ap, "text37");

          field.Error = true;

          return;
        }
        else if (IsExitState("CSENET_REFERRAL_CASE_NF"))
        {
          var field = GetField(export.InterstateCase, "transSerialNumber");

          field.Error = true;

          return;
        }

        switch(AsChar(export.InterstateCase.AssnDeactInd))
        {
          case 'D':
            ExitState = "SI0000_REFERRAL_DEACTIVATED";

            break;
          case 'R':
            ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

            break;
          default:
            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "NAME":
        export.NameGroupList.Index = 0;
        export.NameGroupList.CheckSize();

        export.SubscrpSave.Count = export.NameGroupList.Index + 1;
        export.Search.Assign(export.NameGroupList.Item.N);

        // 11/25/2002 M. Lachowicz Start
        // 11/25/2002 M. Lachowicz End
        export.Search.FormattedName = "";
        export.Phonetic.Flag = "Y";
        export.Phonetic.Percentage = 35;
        export.InitialExecution.Flag = "N";
        export.FromIapi.Flag = "Y";
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        break;
      case "RETNAME":
        if (!IsEmpty(import.Search.LastName))
        {
          if (import.SubscrpSave.Count == export.NameGroupList.Count)
          {
            // *********************************************
            // Save AP data for comparison screens
            // *********************************************
            export.HiddenApid.Assign(import.Search);

            export.NameGroupList.Index = import.SubscrpSave.Count - 1;
            export.NameGroupList.CheckSize();

            export.NameGroupList.Update.N.Assign(import.Search);
            export.NamelistInd.Flag = "Y";
          }
          else
          {
            export.NameGroupList.Index = import.SubscrpSave.Count - 1;
            export.NameGroupList.CheckSize();

            export.NameGroupList.Update.N.Assign(import.Search);
          }
        }
        else if (import.SubscrpSave.Count == export.NameGroupList.Count)
        {
          export.NamelistInd.Flag = "Y";
        }

        if (import.SubscrpSave.Count < export.NameGroupList.Count)
        {
          export.NameGroupList.Index = export.SubscrpSave.Count;
          export.NameGroupList.CheckSize();

          export.SubscrpSave.Count = export.NameGroupList.Index + 1;
          export.Search.Assign(export.NameGroupList.Item.N);
          export.Phonetic.Percentage = 35;
          export.Phonetic.Flag = "Y";
          export.InitialExecution.Flag = "N";
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }

        break;
      case "REGI":
        if (ReadInterstateCase())
        {
          MoveInterstateCase(entities.InterstateCase, export.InterstateCase);
        }
        else
        {
          ExitState = "INTERSTATE_CASE_NF";

          return;
        }

        if (!Lt(local.Null1.Date, export.PaperFileReceivedDate.ReferenceDate))
        {
          var field = GetField(export.PaperFileReceivedDate, "referenceDate");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";

          return;
        }

        if (AsChar(export.NamelistInd.Flag) != 'Y')
        {
          ExitState = "NAME_SEARCH_REQUIRED";

          return;
        }

        switch(AsChar(export.InterstateCase.AssnDeactInd))
        {
          case 'D':
            ExitState = "SI0000_REFERRAL_DEACTIVATED";

            return;
          case 'R':
            ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

            return;
          default:
            break;
        }

        // >>
        // 10/04/2002 T.Bobb PR00159445
        // Check to see if paper receipt exists. The purpose of this
        // check is to prevent the user from exiting the screen
        // and going to REGI to register the case without doing an update first.
        if (!ReadInfrastructure())
        {
          var field = GetField(export.PaperFileReceivedDate, "referenceDate");

          field.Error = true;

          ExitState = "SI0000_RECEIVE_DATE_REQUIRED";

          return;
        }

        for(export.ToRegi.Index = 0; export.ToRegi.Index < export.ToRegi.Count; ++
          export.ToRegi.Index)
        {
          if (!export.ToRegi.CheckSize())
          {
            break;
          }

          export.ToRegi.Update.GregiCsePersonsWorkSet.Flag = "X";
        }

        export.ToRegi.CheckIndex();

        if (!import.NameGroupList.IsEmpty)
        {
          for(import.NameGroupList.Index = 0; import.NameGroupList.Index < import
            .NameGroupList.Count; ++import.NameGroupList.Index)
          {
            if (!import.NameGroupList.CheckSize())
            {
              break;
            }

            export.ToRegi.Index = import.NameGroupList.Index;
            export.ToRegi.CheckSize();

            MoveCsePersonsWorkSet(import.NameGroupList.Item.N,
              export.ToRegi.Update.GregiCsePersonsWorkSet);
            export.ToRegi.Update.GregiCaseRole.Type1 =
              import.NameGroupList.Item.NdetailCase.Type1;
            export.ToRegi.Update.GregiFamilyRelationshi.Type1 =
              import.NameGroupList.Item.NdetailFamily.Type1;

            if (Equal(export.ToRegi.Item.GregiFamilyRelationshi.Type1, "CH"))
            {
              export.ToRegi.Update.GregiFamilyRelationshi.Type1 = "";
            }
          }

          import.NameGroupList.CheckIndex();
        }

        export.FromIapi.Flag = "Y";
        ExitState = "ECO_LNK_TO_CASE_REGISTER";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "IIDC":
        if (import.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_IIDC";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "IAPH":
        if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "SCNX":
        if ((import.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0
          || import.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0) && AsChar
          (export.NamelistInd.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_TO_IIDC";
        }
        else if (import.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_AP_CURRENT";
        }
        else if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else if (import.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "NO_MORE_REFERRAL_SCREENS_FOUND";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "UPDATE":
        if (!Equal(export.PrevPaperFileRecDate.ReferenceDate, local.Null1.Date))
        {
          ExitState = "INVALID_CHANGE";

          return;
        }

        if (Lt(local.Current.Date, export.PaperFileReceivedDate.ReferenceDate))
        {
          var field = GetField(export.PaperFileReceivedDate, "referenceDate");

          field.Error = true;

          ExitState = "SI0000_INVALID_DATE";

          return;
        }

        if (!Lt(local.Null1.Date, export.PaperFileReceivedDate.ReferenceDate))
        {
          var field = GetField(export.PaperFileReceivedDate, "referenceDate");

          field.Error = true;

          ExitState = "OE0014_MANDATORY_FIELD_MISSING";

          return;
        }

        if (!ReadEventEventDetail())
        {
          ExitState = "SP0000_EVENT_DETAIL_NF";

          return;
        }

        local.Infrastructure.ReasonCode = entities.EventDetail.ReasonCode;
        local.Infrastructure.SituationNumber = 1;
        local.Infrastructure.BusinessObjectCd =
          entities.Event1.BusinessObjectCode;
        local.Infrastructure.CsenetInOutCode =
          entities.EventDetail.CsenetInOutCode;
        local.Infrastructure.EventType = entities.Event1.Type1;
        local.Infrastructure.EventDetailName = entities.EventDetail.DetailName;
        local.Infrastructure.EventId = entities.Event1.ControlNumber;
        local.Infrastructure.Function = entities.EventDetail.Function;
        local.Infrastructure.InitiatingStateCode =
          entities.EventDetail.InitiatingStateCode;
        local.Infrastructure.ReferenceDate =
          export.PaperFileReceivedDate.ReferenceDate;
        local.Infrastructure.UserId = export.ServiceProvider.UserId;
        local.Infrastructure.DenormNumeric12 =
          export.InterstateCase.TransSerialNumber;
        local.Infrastructure.DenormDate = export.InterstateCase.TransactionDate;
        local.Infrastructure.ProcessStatus = "Q";
        UseSpCabCreateInfrastructure();

        if (local.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          ExitState = "OE0016_NO_UPDATE_DONE";

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        export.PrevPaperFileRecDate.ReferenceDate =
          export.PaperFileReceivedDate.ReferenceDate;

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.TransactionDate = source.TransactionDate;
    target.AssnDeactInd = source.AssnDeactInd;
  }

  private static void MoveNameGroupList1(SiIapiDisplayCsenetData.Export.
    NameGroupListGroup source, Export.NameGroupListGroup target)
  {
    target.N.Assign(source.N);
    target.NdetailCase.Type1 = source.NdetailCase.Type1;
    target.NdetailFamily.Type1 = source.NdetailFamily.Type1;
  }

  private static void MoveNameGroupList2(Export.NameGroupListGroup source,
    SiIapiDisplayCsenetData.Import.NameGroupListGroup target)
  {
    target.N.Assign(source.N);
    target.NdetailCase.Type1 = source.NdetailCase.Type1;
    target.NdetailFamily.Type1 = source.NdetailFamily.Type1;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiIapiDisplayCsenetData()
  {
    var useImport = new SiIapiDisplayCsenetData.Import();
    var useExport = new SiIapiDisplayCsenetData.Export();

    export.NameGroupList.CopyTo(useImport.NameGroupList, MoveNameGroupList2);
    useImport.InterstateCase.Assign(export.InterstateCase);

    Call(SiIapiDisplayCsenetData.Execute, useImport, useExport);

    useExport.NameGroupList.CopyTo(export.NameGroupList, MoveNameGroupList1);
    export.HiddenApid.Assign(useExport.HiddenApid);
    export.Ap.Text37 = useExport.Ap.Text37;
    export.InterstateApIdentification.Assign(
      useExport.InterstateApIdentification);
    export.PaperFileReceivedDate.ReferenceDate =
      useExport.Infrastructure.ReferenceDate;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadEventEventDetail()
  {
    entities.EventDetail.Populated = false;
    entities.Event1.Populated = false;

    return Read("ReadEventEventDetail",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.Event1.Name = db.GetString(reader, 1);
        entities.Event1.Type1 = db.GetString(reader, 2);
        entities.Event1.Description = db.GetNullableString(reader, 3);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 4);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.EventDetail.DetailName = db.GetString(reader, 6);
        entities.EventDetail.Description = db.GetNullableString(reader, 7);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 8);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 9);
        entities.EventDetail.ReasonCode = db.GetString(reader, 10);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 11);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 12);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 13);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 14);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 15);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 16);
        entities.EventDetail.NextInitiatingState = db.GetString(reader, 17);
        entities.EventDetail.NextCsenetInOut = db.GetNullableString(reader, 18);
        entities.EventDetail.NextReason = db.GetNullableString(reader, 19);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 20);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 21);
        entities.EventDetail.Function = db.GetNullableString(reader, 22);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 23);
        entities.EventDetail.Populated = true;
        entities.Event1.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12", import.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 3);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", export.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          export.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.ActionCode = db.GetString(reader, 1);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 2);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 3);
        entities.InterstateCase.Populated = true;
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
    /// <summary>A NameGroupListGroup group.</summary>
    [Serializable]
    public class NameGroupListGroup
    {
      /// <summary>
      /// A value of N.
      /// </summary>
      [JsonPropertyName("n")]
      public CsePersonsWorkSet N
      {
        get => n ??= new();
        set => n = value;
      }

      /// <summary>
      /// A value of NdetailCase.
      /// </summary>
      [JsonPropertyName("ndetailCase")]
      public CaseRole NdetailCase
      {
        get => ndetailCase ??= new();
        set => ndetailCase = value;
      }

      /// <summary>
      /// A value of NdetailFamily.
      /// </summary>
      [JsonPropertyName("ndetailFamily")]
      public CaseRole NdetailFamily
      {
        get => ndetailFamily ??= new();
        set => ndetailFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet n;
      private CaseRole ndetailCase;
      private CaseRole ndetailFamily;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GcaseRole.
      /// </summary>
      [JsonPropertyName("gcaseRole")]
      public CaseRole GcaseRole
      {
        get => gcaseRole ??= new();
        set => gcaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet gcsePersonsWorkSet;
      private CaseRole gcaseRole;
    }

    /// <summary>A ToRegiGroup group.</summary>
    [Serializable]
    public class ToRegiGroup
    {
      /// <summary>
      /// A value of GregiCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gregiCsePersonsWorkSet")]
      public CsePersonsWorkSet GregiCsePersonsWorkSet
      {
        get => gregiCsePersonsWorkSet ??= new();
        set => gregiCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GregiCaseRole.
      /// </summary>
      [JsonPropertyName("gregiCaseRole")]
      public CaseRole GregiCaseRole
      {
        get => gregiCaseRole ??= new();
        set => gregiCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet gregiCsePersonsWorkSet;
      private CaseRole gregiCaseRole;
    }

    /// <summary>
    /// Gets a value of NameGroupList.
    /// </summary>
    [JsonIgnore]
    public Array<NameGroupListGroup> NameGroupList => nameGroupList ??= new(
      NameGroupListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NameGroupList for json serialization.
    /// </summary>
    [JsonPropertyName("nameGroupList")]
    [Computed]
    public IList<NameGroupListGroup> NameGroupList_Json
    {
      get => nameGroupList;
      set => NameGroupList.Assign(value);
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
    /// A value of RegiNewCase.
    /// </summary>
    [JsonPropertyName("regiNewCase")]
    public Common RegiNewCase
    {
      get => regiNewCase ??= new();
      set => regiNewCase = value;
    }

    /// <summary>
    /// A value of Regi.
    /// </summary>
    [JsonPropertyName("regi")]
    public Case1 Regi
    {
      get => regi ??= new();
      set => regi = value;
    }

    /// <summary>
    /// A value of NamelistInd.
    /// </summary>
    [JsonPropertyName("namelistInd")]
    public Common NamelistInd
    {
      get => namelistInd ??= new();
      set => namelistInd = value;
    }

    /// <summary>
    /// A value of HiddenApid.
    /// </summary>
    [JsonPropertyName("hiddenApid")]
    public CsePersonsWorkSet HiddenApid
    {
      get => hiddenApid ??= new();
      set => hiddenApid = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SubscrpSave.
    /// </summary>
    [JsonPropertyName("subscrpSave")]
    public Common SubscrpSave
    {
      get => subscrpSave ??= new();
      set => subscrpSave = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public WorkArea Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    /// <summary>
    /// Gets a value of ToRegi.
    /// </summary>
    [JsonIgnore]
    public Array<ToRegiGroup> ToRegi => toRegi ??= new(ToRegiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToRegi for json serialization.
    /// </summary>
    [JsonPropertyName("toRegi")]
    [Computed]
    public IList<ToRegiGroup> ToRegi_Json
    {
      get => toRegi;
      set => ToRegi.Assign(value);
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of PaperFileReceivedDate.
    /// </summary>
    [JsonPropertyName("paperFileReceivedDate")]
    public Infrastructure PaperFileReceivedDate
    {
      get => paperFileReceivedDate ??= new();
      set => paperFileReceivedDate = value;
    }

    /// <summary>
    /// A value of PrevPaperFileRecDate.
    /// </summary>
    [JsonPropertyName("prevPaperFileRecDate")]
    public Infrastructure PrevPaperFileRecDate
    {
      get => prevPaperFileRecDate ??= new();
      set => prevPaperFileRecDate = value;
    }

    /// <summary>
    /// A value of ZdelMonitoredActivity.
    /// </summary>
    [JsonPropertyName("zdelMonitoredActivity")]
    public MonitoredActivity ZdelMonitoredActivity
    {
      get => zdelMonitoredActivity ??= new();
      set => zdelMonitoredActivity = value;
    }

    /// <summary>
    /// A value of ZdelMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("zdelMonitoredActivityAssignment")]
    public MonitoredActivityAssignment ZdelMonitoredActivityAssignment
    {
      get => zdelMonitoredActivityAssignment ??= new();
      set => zdelMonitoredActivityAssignment = value;
    }

    private Array<NameGroupListGroup> nameGroupList;
    private Common fromIapi;
    private Common regiNewCase;
    private Case1 regi;
    private Common namelistInd;
    private CsePersonsWorkSet hiddenApid;
    private CsePersonsWorkSet search;
    private Common subscrpSave;
    private WorkArea ap;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private Standard standard;
    private Array<ListGroup> list;
    private Array<ToRegiGroup> toRegi;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProviderAddress serviceProviderAddress;
    private Infrastructure paperFileReceivedDate;
    private Infrastructure prevPaperFileRecDate;
    private MonitoredActivity zdelMonitoredActivity;
    private MonitoredActivityAssignment zdelMonitoredActivityAssignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NameGroupListGroup group.</summary>
    [Serializable]
    public class NameGroupListGroup
    {
      /// <summary>
      /// A value of N.
      /// </summary>
      [JsonPropertyName("n")]
      public CsePersonsWorkSet N
      {
        get => n ??= new();
        set => n = value;
      }

      /// <summary>
      /// A value of NdetailCase.
      /// </summary>
      [JsonPropertyName("ndetailCase")]
      public CaseRole NdetailCase
      {
        get => ndetailCase ??= new();
        set => ndetailCase = value;
      }

      /// <summary>
      /// A value of NdetailFamily.
      /// </summary>
      [JsonPropertyName("ndetailFamily")]
      public CaseRole NdetailFamily
      {
        get => ndetailFamily ??= new();
        set => ndetailFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet n;
      private CaseRole ndetailCase;
      private CaseRole ndetailFamily;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GcaseRole.
      /// </summary>
      [JsonPropertyName("gcaseRole")]
      public CaseRole GcaseRole
      {
        get => gcaseRole ??= new();
        set => gcaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet gcsePersonsWorkSet;
      private CaseRole gcaseRole;
    }

    /// <summary>A ToRegiGroup group.</summary>
    [Serializable]
    public class ToRegiGroup
    {
      /// <summary>
      /// A value of GregiCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gregiCsePersonsWorkSet")]
      public CsePersonsWorkSet GregiCsePersonsWorkSet
      {
        get => gregiCsePersonsWorkSet ??= new();
        set => gregiCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GregiCaseRole.
      /// </summary>
      [JsonPropertyName("gregiCaseRole")]
      public CaseRole GregiCaseRole
      {
        get => gregiCaseRole ??= new();
        set => gregiCaseRole = value;
      }

      /// <summary>
      /// A value of GregiFamilyRelationshi.
      /// </summary>
      [JsonPropertyName("gregiFamilyRelationshi")]
      public CaseRole GregiFamilyRelationshi
      {
        get => gregiFamilyRelationshi ??= new();
        set => gregiFamilyRelationshi = value;
      }

      /// <summary>
      /// A value of CaseConfirm.
      /// </summary>
      [JsonPropertyName("caseConfirm")]
      public Common CaseConfirm
      {
        get => caseConfirm ??= new();
        set => caseConfirm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet gregiCsePersonsWorkSet;
      private CaseRole gregiCaseRole;
      private CaseRole gregiFamilyRelationshi;
      private Common caseConfirm;
    }

    /// <summary>
    /// Gets a value of NameGroupList.
    /// </summary>
    [JsonIgnore]
    public Array<NameGroupListGroup> NameGroupList => nameGroupList ??= new(
      NameGroupListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NameGroupList for json serialization.
    /// </summary>
    [JsonPropertyName("nameGroupList")]
    [Computed]
    public IList<NameGroupListGroup> NameGroupList_Json
    {
      get => nameGroupList;
      set => NameGroupList.Assign(value);
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
    /// A value of InitialExecution.
    /// </summary>
    [JsonPropertyName("initialExecution")]
    public Common InitialExecution
    {
      get => initialExecution ??= new();
      set => initialExecution = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of RegiNewCase.
    /// </summary>
    [JsonPropertyName("regiNewCase")]
    public Common RegiNewCase
    {
      get => regiNewCase ??= new();
      set => regiNewCase = value;
    }

    /// <summary>
    /// A value of Regi.
    /// </summary>
    [JsonPropertyName("regi")]
    public Case1 Regi
    {
      get => regi ??= new();
      set => regi = value;
    }

    /// <summary>
    /// A value of NamelistInd.
    /// </summary>
    [JsonPropertyName("namelistInd")]
    public Common NamelistInd
    {
      get => namelistInd ??= new();
      set => namelistInd = value;
    }

    /// <summary>
    /// A value of HiddenApid.
    /// </summary>
    [JsonPropertyName("hiddenApid")]
    public CsePersonsWorkSet HiddenApid
    {
      get => hiddenApid ??= new();
      set => hiddenApid = value;
    }

    /// <summary>
    /// A value of SubscrpSave.
    /// </summary>
    [JsonPropertyName("subscrpSave")]
    public Common SubscrpSave
    {
      get => subscrpSave ??= new();
      set => subscrpSave = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public WorkArea Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    /// <summary>
    /// Gets a value of ToRegi.
    /// </summary>
    [JsonIgnore]
    public Array<ToRegiGroup> ToRegi => toRegi ??= new(ToRegiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToRegi for json serialization.
    /// </summary>
    [JsonPropertyName("toRegi")]
    [Computed]
    public IList<ToRegiGroup> ToRegi_Json
    {
      get => toRegi;
      set => ToRegi.Assign(value);
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of PaperFileReceivedDate.
    /// </summary>
    [JsonPropertyName("paperFileReceivedDate")]
    public Infrastructure PaperFileReceivedDate
    {
      get => paperFileReceivedDate ??= new();
      set => paperFileReceivedDate = value;
    }

    /// <summary>
    /// A value of PrevPaperFileRecDate.
    /// </summary>
    [JsonPropertyName("prevPaperFileRecDate")]
    public Infrastructure PrevPaperFileRecDate
    {
      get => prevPaperFileRecDate ??= new();
      set => prevPaperFileRecDate = value;
    }

    /// <summary>
    /// A value of ZdelMonitoredActivity.
    /// </summary>
    [JsonPropertyName("zdelMonitoredActivity")]
    public MonitoredActivity ZdelMonitoredActivity
    {
      get => zdelMonitoredActivity ??= new();
      set => zdelMonitoredActivity = value;
    }

    /// <summary>
    /// A value of ZdelMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("zdelMonitoredActivityAssignment")]
    public MonitoredActivityAssignment ZdelMonitoredActivityAssignment
    {
      get => zdelMonitoredActivityAssignment ??= new();
      set => zdelMonitoredActivityAssignment = value;
    }

    private Array<NameGroupListGroup> nameGroupList;
    private Common fromIapi;
    private Common initialExecution;
    private Common phonetic;
    private Common regiNewCase;
    private Case1 regi;
    private Common namelistInd;
    private CsePersonsWorkSet hiddenApid;
    private Common subscrpSave;
    private CsePersonsWorkSet search;
    private WorkArea ap;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private Standard standard;
    private Array<ListGroup> list;
    private Array<ToRegiGroup> toRegi;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProviderAddress serviceProviderAddress;
    private Infrastructure paperFileReceivedDate;
    private Infrastructure prevPaperFileRecDate;
    private MonitoredActivity zdelMonitoredActivity;
    private MonitoredActivityAssignment zdelMonitoredActivityAssignment;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
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
    /// A value of ApAdded.
    /// </summary>
    [JsonPropertyName("apAdded")]
    public Common ApAdded
    {
      get => apAdded ??= new();
      set => apAdded = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of NamelistInd.
    /// </summary>
    [JsonPropertyName("namelistInd")]
    public Common NamelistInd
    {
      get => namelistInd ??= new();
      set => namelistInd = value;
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
    /// A value of ZdelMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("zdelMonitoredActivityAssignment")]
    public MonitoredActivityAssignment ZdelMonitoredActivityAssignment
    {
      get => zdelMonitoredActivityAssignment ??= new();
      set => zdelMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of ZdelMonitoredActivity.
    /// </summary>
    [JsonPropertyName("zdelMonitoredActivity")]
    public MonitoredActivity ZdelMonitoredActivity
    {
      get => zdelMonitoredActivity ??= new();
      set => zdelMonitoredActivity = value;
    }

    private DateWorkArea zeroDate;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private DateWorkArea null1;
    private Common apAdded;
    private CsePerson ap;
    private Common namelistInd;
    private CsePersonsWorkSet csePersonsWorkSet;
    private MonitoredActivityAssignment zdelMonitoredActivityAssignment;
    private MonitoredActivity zdelMonitoredActivity;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
    }

    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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
    /// A value of ZdelInterstateParticipant.
    /// </summary>
    [JsonPropertyName("zdelInterstateParticipant")]
    public InterstateParticipant ZdelInterstateParticipant
    {
      get => zdelInterstateParticipant ??= new();
      set => zdelInterstateParticipant = value;
    }

    /// <summary>
    /// A value of ZdelInterstateApIdentification.
    /// </summary>
    [JsonPropertyName("zdelInterstateApIdentification")]
    public InterstateApIdentification ZdelInterstateApIdentification
    {
      get => zdelInterstateApIdentification ??= new();
      set => zdelInterstateApIdentification = value;
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

    private ActivityStartStop activityStartStop;
    private ActivityDetail activityDetail;
    private Activity activity;
    private EventDetail eventDetail;
    private Event1 event1;
    private InterstateCase interstateCase;
    private InterstateParticipant zdelInterstateParticipant;
    private InterstateApIdentification zdelInterstateApIdentification;
    private Infrastructure infrastructure;
  }
#endregion
}
