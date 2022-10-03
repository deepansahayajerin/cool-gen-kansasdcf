// Program: SI_CLOSE_INCOMING_CSENET_TRANS, ID: 372556573, model: 746.
// Short name: SWE01660
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CLOSE_INCOMING_CSENET_TRANS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB updates the interstate_case assn_deact_ind and related tables.  If 
/// the imported value is R, then it will also create an outgoing transaction to
/// notify the other state that we are rejecting their transction.
/// Valid values for ASSN_DEACT_IND are D (deactivate) and R (reject)
/// </para>
/// </summary>
[Serializable]
public partial class SiCloseIncomingCsenetTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CLOSE_INCOMING_CSENET_TRANS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCloseIncomingCsenetTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCloseIncomingCsenetTrans.
  /// </summary>
  public SiCloseIncomingCsenetTrans(IContext context, Import import,
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
    // -----------------------------------------------------------
    // Date		Developer	Request
    // -----------------------------------------------------------
    // 2002/10/16	M Ramirez	114395
    // Fixing IIOIs outgoing transactions
    // -----------------------------------------------------------
    // -----------------------------------------------------------
    // Processing Description
    // This CAB updates the interstate_case assn_deact_ind and
    // related tables.  If the imported value is R, then it will
    // also create an outgoing transaction to notify the other state
    // that we are rejecting their transaction.
    // Valid values for ASSN_DEACT_IND are D (deactivate) and
    // R (reject)
    // -----------------------------------------------------------
    if (!IsEmpty(import.ProgramProcessingInfo.Name))
    {
      local.Current.Name = import.ProgramProcessingInfo.Name;
    }
    else
    {
      local.Current.Name = global.UserId;
    }

    if (Lt(local.Null1.Date, import.ProgramProcessingInfo.ProcessDate))
    {
      local.Current.ProcessDate = import.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      local.Current.ProcessDate = Now().Date;
    }

    // mjr
    // ----------------------------------------------
    // 2002/10/16
    // First handle the reject transaction
    // -----------------------------------------------------------
    if (ReadInterstateCase())
    {
      switch(AsChar(import.InterstateCase.AssnDeactInd))
      {
        case 'D':
          break;
        case 'R':
          break;
        default:
          ExitState = "INTERSTATE_CASE_PV";

          return;
      }

      local.InterstateCase.Assign(entities.InterstateCase);
      local.InterstateCase.AssnDeactInd =
        import.InterstateCase.AssnDeactInd ?? "";
      local.InterstateCase.AssnDeactDt = local.Current.ProcessDate;
      UseSiUpdateInterstateCase();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      foreach(var item in ReadInterstateCaseAssignment())
      {
        try
        {
          UpdateInterstateCaseAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_CASE_ASSIGNMENT_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ASSIGNMENT_PV";

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
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    // mjr
    // ----------------------------------------------
    // 2002/10/16
    // Now handle the response transaction
    // -----------------------------------------------------------
    if (AsChar(import.InterstateCase.AssnDeactInd) != 'R')
    {
      return;
    }

    // ***   Get the new CSENet transaction serial # and date    ***
    UseSiGenCsenetTransactSerialNo();
    MoveInterstateCase2(export.New1, local.InterstateCase);
    local.InterstateCase.ActionCode = import.Send.ActionCode;
    local.InterstateCase.ActionReasonCode = import.Send.ActionReasonCode ?? "";
    local.InterstateCase.AssnDeactInd = "D";
    local.InterstateCase.AssnDeactDt = local.Null1.Date;
    local.InterstateCase.AttachmentsInd = "N";

    if (!IsEmpty(import.Send.Note))
    {
      local.InterstateCase.InformationInd = 1;
    }

    // ***   CREATE THE CASE_DB   ***
    UseSiCreateInterstateCase();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***   CREATE CSENet Envelop   ***
    UseSiCreateOgCsenetEnvelop();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // ***   CREATE AP_IDENTIFICATION_DB   ***
    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() == 1)
    {
      if (ReadInterstateApIdentification())
      {
        UseSiCreateInterstateApId();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
        ExitState = "CSENET_AP_ID_NF";

        return;
      }

      // ***   CREATE AP_LOCATE_DB   ***
      if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() == 1)
      {
        if (ReadInterstateApLocate())
        {
          UseSiCreateInterstateApLocate();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        else
        {
          ExitState = "CSENET_AP_LOCATE_NF";

          return;
        }
      }
    }

    // ***   CREATE PARTICIPANT_DBs   ***
    if (local.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
    {
      foreach(var item in ReadInterstateParticipant())
      {
        UseSiCreateInterstateParticipant();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          return;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }
    }

    // ***   CREATE ORDER_DBs   ***
    if (local.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
    {
      foreach(var item in ReadInterstateSupportOrder())
      {
        UseSiCreateInterstateOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    // ***   CREATE MISCELLANEOUS_DB    ***
    if (!IsEmpty(import.Send.Note))
    {
      local.Length.Count = Length(import.Send.Note);

      if (local.Length.Count > 80)
      {
        local.InterstateMiscellaneous.InformationTextLine2 =
          Substring(import.Send.Note, 81, 80);

        if (local.Length.Count > 160)
        {
          local.InterstateMiscellaneous.InformationTextLine3 =
            Substring(import.Send.Note, 161, 80);

          if (local.Length.Count > 240)
          {
            local.InterstateMiscellaneous.InformationTextLine4 =
              Substring(import.Send.Note, 241, 80);

            if (local.Length.Count > 320)
            {
              local.InterstateMiscellaneous.InformationTextLine5 =
                Substring(import.Send.Note, 321, 80);
            }
          }
        }
      }

      local.InterstateMiscellaneous.StatusChangeCode = "C";
      local.InterstateMiscellaneous.NewCaseId =
        local.InterstateCase.KsCaseId ?? "";
      local.InterstateMiscellaneous.InformationTextLine1 = import.Send.Note ?? ""
        ;
      UseSiCreateInterstateMisc();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    foreach(var item in ReadInterstateRequestHistory())
    {
      if (ReadInterstateRequest())
      {
        MoveInterstateRequestHistory(import.Send, local.InterstateRequestHistory);
          
        local.InterstateRequestHistory.TransactionSerialNum =
          export.New1.TransSerialNumber;
        local.InterstateRequestHistory.TransactionDate =
          export.New1.TransactionDate;
        local.InterstateRequestHistory.TransactionDirectionInd = "O";
        local.InterstateRequestHistory.AttachmentIndicator =
          local.InterstateCase.AttachmentsInd;
        local.InterstateRequestHistory.ActionResolutionDate =
          local.Current.ProcessDate;
        local.InterstateRequestHistory.CreatedBy = local.Current.Name;
        UseSiCabCreateIsRequestHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        return;
      }
    }
  }

  private static void MoveInterstateApIdentification(
    InterstateApIdentification source, InterstateApIdentification target)
  {
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleName = source.MiddleName;
    target.NameSuffix = source.NameSuffix;
    target.Ssn = source.Ssn;
    target.DateOfBirth = source.DateOfBirth;
    target.Race = source.Race;
    target.Sex = source.Sex;
    target.PlaceOfBirth = source.PlaceOfBirth;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.Weight = source.Weight;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.OtherIdInfo = source.OtherIdInfo;
    target.AliasSsn1 = source.AliasSsn1;
    target.AliasSsn2 = source.AliasSsn2;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.LocalFipsState = source.LocalFipsState;
    target.LocalFipsCounty = source.LocalFipsCounty;
    target.LocalFipsLocation = source.LocalFipsLocation;
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.ActionResolutionDate = source.ActionResolutionDate;
    target.AttachmentsInd = source.AttachmentsInd;
    target.CaseDataInd = source.CaseDataInd;
    target.ApIdentificationInd = source.ApIdentificationInd;
    target.ApLocateDataInd = source.ApLocateDataInd;
    target.ParticipantDataInd = source.ParticipantDataInd;
    target.OrderDataInd = source.OrderDataInd;
    target.CollectionDataInd = source.CollectionDataInd;
    target.InformationInd = source.InformationInd;
    target.SentDate = source.SentDate;
    target.SentTime = source.SentTime;
    target.DueDate = source.DueDate;
    target.OverdueInd = source.OverdueInd;
    target.DateReceived = source.DateReceived;
    target.TimeReceived = source.TimeReceived;
    target.AttachmentsDueDate = source.AttachmentsDueDate;
    target.InterstateFormsPrinted = source.InterstateFormsPrinted;
    target.CaseType = source.CaseType;
    target.CaseStatus = source.CaseStatus;
    target.PaymentMailingAddressLine1 = source.PaymentMailingAddressLine1;
    target.PaymentAddressLine2 = source.PaymentAddressLine2;
    target.PaymentCity = source.PaymentCity;
    target.PaymentState = source.PaymentState;
    target.PaymentZipCode5 = source.PaymentZipCode5;
    target.PaymentZipCode4 = source.PaymentZipCode4;
    target.ContactNameLast = source.ContactNameLast;
    target.ContactNameFirst = source.ContactNameFirst;
    target.ContactNameMiddle = source.ContactNameMiddle;
    target.ContactNameSuffix = source.ContactNameSuffix;
    target.ContactAddressLine1 = source.ContactAddressLine1;
    target.ContactAddressLine2 = source.ContactAddressLine2;
    target.ContactCity = source.ContactCity;
    target.ContactState = source.ContactState;
    target.ContactZipCode5 = source.ContactZipCode5;
    target.ContactZipCode4 = source.ContactZipCode4;
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.AssnDeactDt = source.AssnDeactDt;
    target.AssnDeactInd = source.AssnDeactInd;
    target.LastDeferDt = source.LastDeferDt;
    target.Memo = source.Memo;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.ActionReasonCode = source.ActionReasonCode;
    target.Note = source.Note;
  }

  private void UseSiCabCreateIsRequestHistory()
  {
    var useImport = new SiCabCreateIsRequestHistory.Import();
    var useExport = new SiCabCreateIsRequestHistory.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      entities.InterstateRequest.IntHGeneratedId;
    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);

    Call(SiCabCreateIsRequestHistory.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApId()
  {
    var useImport = new SiCreateInterstateApId.Import();
    var useExport = new SiCreateInterstateApId.Export();

    MoveInterstateApIdentification(entities.InterstateApIdentification,
      useImport.InterstateApIdentification);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApLocate()
  {
    var useImport = new SiCreateInterstateApLocate.Import();
    var useExport = new SiCreateInterstateApLocate.Export();

    useImport.InterstateApLocate.Assign(entities.InterstateApLocate);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateMisc()
  {
    var useImport = new SiCreateInterstateMisc.Import();
    var useExport = new SiCreateInterstateMisc.Export();

    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateOrder()
  {
    var useImport = new SiCreateInterstateOrder.Import();
    var useExport = new SiCreateInterstateOrder.Export();

    useImport.InterstateSupportOrder.Assign(entities.InterstateSupportOrder);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateParticipant()
  {
    var useImport = new SiCreateInterstateParticipant.Import();
    var useExport = new SiCreateInterstateParticipant.Export();

    useImport.InterstateParticipant.Assign(entities.InterstateParticipant);
    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateInterstateParticipant.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase2(useExport.InterstateCase, export.New1);
  }

  private void UseSiUpdateInterstateCase()
  {
    var useImport = new SiUpdateInterstateCase.Import();
    var useExport = new SiUpdateInterstateCase.Export();

    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateInterstateCase.Execute, useImport, useExport);
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 7);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 14);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 16);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 17);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private bool ReadInterstateApLocate()
  {
    entities.InterstateApLocate.Populated = false;

    return Read("ReadInterstateApLocate",
      (db, command) =>
      {
        db.SetDate(
          command, "cncTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "cncTransSerlNbr",
          entities.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateApLocate.CncTransactionDt = db.GetDate(reader, 0);
        entities.InterstateApLocate.CncTransSerlNbr = db.GetInt64(reader, 1);
        entities.InterstateApLocate.EmployerEin =
          db.GetNullableInt32(reader, 2);
        entities.InterstateApLocate.EmployerName =
          db.GetNullableString(reader, 3);
        entities.InterstateApLocate.EmployerPhoneNum =
          db.GetNullableInt32(reader, 4);
        entities.InterstateApLocate.EmployerEffectiveDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateApLocate.EmployerEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstateApLocate.EmployerConfirmedInd =
          db.GetNullableString(reader, 7);
        entities.InterstateApLocate.ResidentialAddressLine1 =
          db.GetNullableString(reader, 8);
        entities.InterstateApLocate.ResidentialAddressLine2 =
          db.GetNullableString(reader, 9);
        entities.InterstateApLocate.ResidentialCity =
          db.GetNullableString(reader, 10);
        entities.InterstateApLocate.ResidentialState =
          db.GetNullableString(reader, 11);
        entities.InterstateApLocate.ResidentialZipCode5 =
          db.GetNullableString(reader, 12);
        entities.InterstateApLocate.ResidentialZipCode4 =
          db.GetNullableString(reader, 13);
        entities.InterstateApLocate.MailingAddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateApLocate.MailingAddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateApLocate.MailingCity =
          db.GetNullableString(reader, 16);
        entities.InterstateApLocate.MailingState =
          db.GetNullableString(reader, 17);
        entities.InterstateApLocate.MailingZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateApLocate.MailingZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateApLocate.ResidentialAddressEffectvDate =
          db.GetNullableDate(reader, 20);
        entities.InterstateApLocate.ResidentialAddressEndDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateApLocate.ResidentialAddressConfirmInd =
          db.GetNullableString(reader, 22);
        entities.InterstateApLocate.MailingAddressEffectiveDate =
          db.GetNullableDate(reader, 23);
        entities.InterstateApLocate.MailingAddressEndDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateApLocate.MailingAddressConfirmedInd =
          db.GetNullableString(reader, 25);
        entities.InterstateApLocate.HomePhoneNumber =
          db.GetNullableInt32(reader, 26);
        entities.InterstateApLocate.WorkPhoneNumber =
          db.GetNullableInt32(reader, 27);
        entities.InterstateApLocate.DriversLicState =
          db.GetNullableString(reader, 28);
        entities.InterstateApLocate.DriversLicenseNum =
          db.GetNullableString(reader, 29);
        entities.InterstateApLocate.Alias1FirstName =
          db.GetNullableString(reader, 30);
        entities.InterstateApLocate.Alias1MiddleName =
          db.GetNullableString(reader, 31);
        entities.InterstateApLocate.Alias1LastName =
          db.GetNullableString(reader, 32);
        entities.InterstateApLocate.Alias1Suffix =
          db.GetNullableString(reader, 33);
        entities.InterstateApLocate.Alias2FirstName =
          db.GetNullableString(reader, 34);
        entities.InterstateApLocate.Alias2MiddleName =
          db.GetNullableString(reader, 35);
        entities.InterstateApLocate.Alias2LastName =
          db.GetNullableString(reader, 36);
        entities.InterstateApLocate.Alias2Suffix =
          db.GetNullableString(reader, 37);
        entities.InterstateApLocate.Alias3FirstName =
          db.GetNullableString(reader, 38);
        entities.InterstateApLocate.Alias3MiddleName =
          db.GetNullableString(reader, 39);
        entities.InterstateApLocate.Alias3LastName =
          db.GetNullableString(reader, 40);
        entities.InterstateApLocate.Alias3Suffix =
          db.GetNullableString(reader, 41);
        entities.InterstateApLocate.CurrentSpouseFirstName =
          db.GetNullableString(reader, 42);
        entities.InterstateApLocate.CurrentSpouseMiddleName =
          db.GetNullableString(reader, 43);
        entities.InterstateApLocate.CurrentSpouseLastName =
          db.GetNullableString(reader, 44);
        entities.InterstateApLocate.CurrentSpouseSuffix =
          db.GetNullableString(reader, 45);
        entities.InterstateApLocate.Occupation =
          db.GetNullableString(reader, 46);
        entities.InterstateApLocate.EmployerAddressLine1 =
          db.GetNullableString(reader, 47);
        entities.InterstateApLocate.EmployerAddressLine2 =
          db.GetNullableString(reader, 48);
        entities.InterstateApLocate.EmployerCity =
          db.GetNullableString(reader, 49);
        entities.InterstateApLocate.EmployerState =
          db.GetNullableString(reader, 50);
        entities.InterstateApLocate.EmployerZipCode5 =
          db.GetNullableString(reader, 51);
        entities.InterstateApLocate.EmployerZipCode4 =
          db.GetNullableString(reader, 52);
        entities.InterstateApLocate.WageQtr = db.GetNullableInt32(reader, 53);
        entities.InterstateApLocate.WageYear = db.GetNullableInt32(reader, 54);
        entities.InterstateApLocate.WageAmount =
          db.GetNullableDecimal(reader, 55);
        entities.InterstateApLocate.InsuranceCarrierName =
          db.GetNullableString(reader, 56);
        entities.InterstateApLocate.InsurancePolicyNum =
          db.GetNullableString(reader, 57);
        entities.InterstateApLocate.LastResAddressLine1 =
          db.GetNullableString(reader, 58);
        entities.InterstateApLocate.LastResAddressLine2 =
          db.GetNullableString(reader, 59);
        entities.InterstateApLocate.LastResCity =
          db.GetNullableString(reader, 60);
        entities.InterstateApLocate.LastResState =
          db.GetNullableString(reader, 61);
        entities.InterstateApLocate.LastResZipCode5 =
          db.GetNullableString(reader, 62);
        entities.InterstateApLocate.LastResZipCode4 =
          db.GetNullableString(reader, 63);
        entities.InterstateApLocate.LastResAddressDate =
          db.GetNullableDate(reader, 64);
        entities.InterstateApLocate.LastMailAddressLine1 =
          db.GetNullableString(reader, 65);
        entities.InterstateApLocate.LastMailAddressLine2 =
          db.GetNullableString(reader, 66);
        entities.InterstateApLocate.LastMailCity =
          db.GetNullableString(reader, 67);
        entities.InterstateApLocate.LastMailState =
          db.GetNullableString(reader, 68);
        entities.InterstateApLocate.LastMailZipCode5 =
          db.GetNullableString(reader, 69);
        entities.InterstateApLocate.LastMailZipCode4 =
          db.GetNullableString(reader, 70);
        entities.InterstateApLocate.LastMailAddressDate =
          db.GetNullableDate(reader, 71);
        entities.InterstateApLocate.LastEmployerName =
          db.GetNullableString(reader, 72);
        entities.InterstateApLocate.LastEmployerDate =
          db.GetNullableDate(reader, 73);
        entities.InterstateApLocate.LastEmployerAddressLine1 =
          db.GetNullableString(reader, 74);
        entities.InterstateApLocate.LastEmployerAddressLine2 =
          db.GetNullableString(reader, 75);
        entities.InterstateApLocate.LastEmployerCity =
          db.GetNullableString(reader, 76);
        entities.InterstateApLocate.LastEmployerState =
          db.GetNullableString(reader, 77);
        entities.InterstateApLocate.LastEmployerZipCode5 =
          db.GetNullableString(reader, 78);
        entities.InterstateApLocate.LastEmployerZipCode4 =
          db.GetNullableString(reader, 79);
        entities.InterstateApLocate.ProfessionalLicenses =
          db.GetNullableString(reader, 80);
        entities.InterstateApLocate.WorkAreaCode =
          db.GetNullableInt32(reader, 81);
        entities.InterstateApLocate.HomeAreaCode =
          db.GetNullableInt32(reader, 82);
        entities.InterstateApLocate.LastEmployerEndDate =
          db.GetNullableDate(reader, 83);
        entities.InterstateApLocate.EmployerAreaCode =
          db.GetNullableInt32(reader, 84);
        entities.InterstateApLocate.Employer2Name =
          db.GetNullableString(reader, 85);
        entities.InterstateApLocate.Employer2Ein =
          db.GetNullableInt32(reader, 86);
        entities.InterstateApLocate.Employer2PhoneNumber =
          db.GetNullableString(reader, 87);
        entities.InterstateApLocate.Employer2AreaCode =
          db.GetNullableString(reader, 88);
        entities.InterstateApLocate.Employer2AddressLine1 =
          db.GetNullableString(reader, 89);
        entities.InterstateApLocate.Employer2AddressLine2 =
          db.GetNullableString(reader, 90);
        entities.InterstateApLocate.Employer2City =
          db.GetNullableString(reader, 91);
        entities.InterstateApLocate.Employer2State =
          db.GetNullableString(reader, 92);
        entities.InterstateApLocate.Employer2ZipCode5 =
          db.GetNullableInt32(reader, 93);
        entities.InterstateApLocate.Employer2ZipCode4 =
          db.GetNullableInt32(reader, 94);
        entities.InterstateApLocate.Employer2ConfirmedIndicator =
          db.GetNullableString(reader, 95);
        entities.InterstateApLocate.Employer2EffectiveDate =
          db.GetNullableDate(reader, 96);
        entities.InterstateApLocate.Employer2EndDate =
          db.GetNullableDate(reader, 97);
        entities.InterstateApLocate.Employer2WageAmount =
          db.GetNullableInt64(reader, 98);
        entities.InterstateApLocate.Employer2WageQuarter =
          db.GetNullableInt32(reader, 99);
        entities.InterstateApLocate.Employer2WageYear =
          db.GetNullableInt32(reader, 100);
        entities.InterstateApLocate.Employer3Name =
          db.GetNullableString(reader, 101);
        entities.InterstateApLocate.Employer3Ein =
          db.GetNullableInt32(reader, 102);
        entities.InterstateApLocate.Employer3PhoneNumber =
          db.GetNullableString(reader, 103);
        entities.InterstateApLocate.Employer3AreaCode =
          db.GetNullableString(reader, 104);
        entities.InterstateApLocate.Employer3AddressLine1 =
          db.GetNullableString(reader, 105);
        entities.InterstateApLocate.Employer3AddressLine2 =
          db.GetNullableString(reader, 106);
        entities.InterstateApLocate.Employer3City =
          db.GetNullableString(reader, 107);
        entities.InterstateApLocate.Employer3State =
          db.GetNullableString(reader, 108);
        entities.InterstateApLocate.Employer3ZipCode5 =
          db.GetNullableInt32(reader, 109);
        entities.InterstateApLocate.Employer3ZipCode4 =
          db.GetNullableInt32(reader, 110);
        entities.InterstateApLocate.Employer3ConfirmedIndicator =
          db.GetNullableString(reader, 111);
        entities.InterstateApLocate.Employer3EffectiveDate =
          db.GetNullableDate(reader, 112);
        entities.InterstateApLocate.Employer3EndDate =
          db.GetNullableDate(reader, 113);
        entities.InterstateApLocate.Employer3WageAmount =
          db.GetNullableInt64(reader, 114);
        entities.InterstateApLocate.Employer3WageQuarter =
          db.GetNullableInt32(reader, 115);
        entities.InterstateApLocate.Employer3WageYear =
          db.GetNullableInt32(reader, 116);
        entities.InterstateApLocate.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 1);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 2);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 3);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 4);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 5);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 6);
        entities.InterstateCase.ActionCode = db.GetString(reader, 7);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 9);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 10);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 11);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 12);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 14);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 15);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 16);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 17);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 18);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 22);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 23);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 24);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 26);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 29);
        entities.InterstateCase.CaseType = db.GetString(reader, 30);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 31);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 32);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 34);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 35);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 40);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 41);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 42);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 48);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 49);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 50);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 51);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 52);
        entities.InterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignment()
  {
    entities.InterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.Current.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 0);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 6);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 8);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 9);
        entities.InterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateParticipant()
  {
    entities.InterstateParticipant.Populated = false;

    return ReadEach("ReadInterstateParticipant",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.InterstateParticipant.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateParticipant.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateParticipant.CcaTransSerNum = db.GetInt64(reader, 2);
        entities.InterstateParticipant.NameLast =
          db.GetNullableString(reader, 3);
        entities.InterstateParticipant.NameFirst =
          db.GetNullableString(reader, 4);
        entities.InterstateParticipant.NameMiddle =
          db.GetNullableString(reader, 5);
        entities.InterstateParticipant.NameSuffix =
          db.GetNullableString(reader, 6);
        entities.InterstateParticipant.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.InterstateParticipant.Ssn = db.GetNullableString(reader, 8);
        entities.InterstateParticipant.Sex = db.GetNullableString(reader, 9);
        entities.InterstateParticipant.Race = db.GetNullableString(reader, 10);
        entities.InterstateParticipant.Relationship =
          db.GetNullableString(reader, 11);
        entities.InterstateParticipant.Status =
          db.GetNullableString(reader, 12);
        entities.InterstateParticipant.DependentRelationCp =
          db.GetNullableString(reader, 13);
        entities.InterstateParticipant.AddressLine1 =
          db.GetNullableString(reader, 14);
        entities.InterstateParticipant.AddressLine2 =
          db.GetNullableString(reader, 15);
        entities.InterstateParticipant.City = db.GetNullableString(reader, 16);
        entities.InterstateParticipant.State = db.GetNullableString(reader, 17);
        entities.InterstateParticipant.ZipCode5 =
          db.GetNullableString(reader, 18);
        entities.InterstateParticipant.ZipCode4 =
          db.GetNullableString(reader, 19);
        entities.InterstateParticipant.EmployerAddressLine1 =
          db.GetNullableString(reader, 20);
        entities.InterstateParticipant.EmployerAddressLine2 =
          db.GetNullableString(reader, 21);
        entities.InterstateParticipant.EmployerCity =
          db.GetNullableString(reader, 22);
        entities.InterstateParticipant.EmployerState =
          db.GetNullableString(reader, 23);
        entities.InterstateParticipant.EmployerZipCode5 =
          db.GetNullableString(reader, 24);
        entities.InterstateParticipant.EmployerZipCode4 =
          db.GetNullableString(reader, 25);
        entities.InterstateParticipant.EmployerName =
          db.GetNullableString(reader, 26);
        entities.InterstateParticipant.EmployerEin =
          db.GetNullableInt32(reader, 27);
        entities.InterstateParticipant.AddressVerifiedDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateParticipant.EmployerVerifiedDate =
          db.GetNullableDate(reader, 29);
        entities.InterstateParticipant.WorkPhone =
          db.GetNullableString(reader, 30);
        entities.InterstateParticipant.WorkAreaCode =
          db.GetNullableString(reader, 31);
        entities.InterstateParticipant.PlaceOfBirth =
          db.GetNullableString(reader, 32);
        entities.InterstateParticipant.ChildStateOfResidence =
          db.GetNullableString(reader, 33);
        entities.InterstateParticipant.ChildPaternityStatus =
          db.GetNullableString(reader, 34);
        entities.InterstateParticipant.EmployerConfirmedInd =
          db.GetNullableString(reader, 35);
        entities.InterstateParticipant.AddressConfirmedInd =
          db.GetNullableString(reader, 36);
        entities.InterstateParticipant.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.InterstateRequestHistory.IntGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt64(
          command, "transactionSerial",
          import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 2);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 3);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateSupportOrder()
  {
    entities.InterstateSupportOrder.Populated = false;

    return ReadEach("ReadInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTranSerNum", entities.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateSupportOrder.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateSupportOrder.CcaTranSerNum = db.GetInt64(reader, 2);
        entities.InterstateSupportOrder.FipsState = db.GetString(reader, 3);
        entities.InterstateSupportOrder.FipsCounty =
          db.GetNullableString(reader, 4);
        entities.InterstateSupportOrder.FipsLocation =
          db.GetNullableString(reader, 5);
        entities.InterstateSupportOrder.Number = db.GetString(reader, 6);
        entities.InterstateSupportOrder.OrderFilingDate = db.GetDate(reader, 7);
        entities.InterstateSupportOrder.Type1 = db.GetNullableString(reader, 8);
        entities.InterstateSupportOrder.DebtType = db.GetString(reader, 9);
        entities.InterstateSupportOrder.PaymentFreq =
          db.GetNullableString(reader, 10);
        entities.InterstateSupportOrder.AmountOrdered =
          db.GetNullableDecimal(reader, 11);
        entities.InterstateSupportOrder.EffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.InterstateSupportOrder.EndDate =
          db.GetNullableDate(reader, 13);
        entities.InterstateSupportOrder.CancelDate =
          db.GetNullableDate(reader, 14);
        entities.InterstateSupportOrder.ArrearsFreq =
          db.GetNullableString(reader, 15);
        entities.InterstateSupportOrder.ArrearsFreqAmount =
          db.GetNullableDecimal(reader, 16);
        entities.InterstateSupportOrder.ArrearsTotalAmount =
          db.GetNullableDecimal(reader, 17);
        entities.InterstateSupportOrder.ArrearsAfdcFromDate =
          db.GetNullableDate(reader, 18);
        entities.InterstateSupportOrder.ArrearsAfdcThruDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateSupportOrder.ArrearsAfdcAmount =
          db.GetNullableDecimal(reader, 20);
        entities.InterstateSupportOrder.ArrearsNonAfdcFromDate =
          db.GetNullableDate(reader, 21);
        entities.InterstateSupportOrder.ArrearsNonAfdcThruDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateSupportOrder.ArrearsNonAfdcAmount =
          db.GetNullableDecimal(reader, 23);
        entities.InterstateSupportOrder.FosterCareFromDate =
          db.GetNullableDate(reader, 24);
        entities.InterstateSupportOrder.FosterCareThruDate =
          db.GetNullableDate(reader, 25);
        entities.InterstateSupportOrder.FosterCareAmount =
          db.GetNullableDecimal(reader, 26);
        entities.InterstateSupportOrder.MedicalFromDate =
          db.GetNullableDate(reader, 27);
        entities.InterstateSupportOrder.MedicalThruDate =
          db.GetNullableDate(reader, 28);
        entities.InterstateSupportOrder.MedicalAmount =
          db.GetNullableDecimal(reader, 29);
        entities.InterstateSupportOrder.MedicalOrderedInd =
          db.GetNullableString(reader, 30);
        entities.InterstateSupportOrder.TribunalCaseNumber =
          db.GetNullableString(reader, 31);
        entities.InterstateSupportOrder.DateOfLastPayment =
          db.GetNullableDate(reader, 32);
        entities.InterstateSupportOrder.ControllingOrderFlag =
          db.GetNullableString(reader, 33);
        entities.InterstateSupportOrder.NewOrderFlag =
          db.GetNullableString(reader, 34);
        entities.InterstateSupportOrder.DocketNumber =
          db.GetNullableString(reader, 35);
        entities.InterstateSupportOrder.LegalActionId =
          db.GetNullableInt32(reader, 36);
        entities.InterstateSupportOrder.Populated = true;

        return true;
      });
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);

    var discontinueDate = local.Current.ProcessDate;
    var lastUpdatedBy = local.Current.Name;
    var lastUpdatedTimestamp = Now();

    entities.InterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.InterstateCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.InterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.InterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.InterstateCaseAssignment.IcsDate.GetValueOrDefault());
        db.SetInt64(command, "icsNo", entities.InterstateCaseAssignment.IcsNo);
      });

    entities.InterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.InterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateCaseAssignment.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public InterstateRequestHistory Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private InterstateCase interstateCase;
    private InterstateRequestHistory send;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateCase New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private InterstateCase new1;
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
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public ProgramProcessingInfo Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
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
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    private DateWorkArea null1;
    private ProgramProcessingInfo current;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateCase interstateCase;
    private Common length;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateCase New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateParticipant.
    /// </summary>
    [JsonPropertyName("interstateParticipant")]
    public InterstateParticipant InterstateParticipant
    {
      get => interstateParticipant ??= new();
      set => interstateParticipant = value;
    }

    /// <summary>
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    private InterstateCase new1;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private InterstateParticipant interstateParticipant;
    private InterstateSupportOrder interstateSupportOrder;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateCaseAssignment interstateCaseAssignment;
  }
#endregion
}
