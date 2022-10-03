// Program: SI_BUILD_PREV_CSENET_REF_LIST, ID: 372497572, model: 746.
// Short name: SWE01106
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
/// A program: SI_BUILD_PREV_CSENET_REF_LIST.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This block builds a list of CSENet Referral Identifiers based on the Action 
/// code requested by the user.	
/// Code      Action
///  R        Create new Case
///  U        Update existing Case
///  P        Process response from other state
/// </para>
/// </summary>
[Serializable]
public partial class SiBuildPrevCsenetRefList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_PREV_CSENET_REF_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildPrevCsenetRefList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildPrevCsenetRefList.
  /// </summary>
  public SiBuildPrevCsenetRefList(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer         Description
    // ---------------------------------------------
    // 5-22-95  Ken Evans         Initial development
    // 03/10/99 P. SHARP          Changed read to include the worker that is 
    // assigned the
    //                            interstate case.
    // 02/27/01 SWSRCHF I00114722 Added date check to the READ EACH statement.
    //                            Increased the Group view from 350 to 800 
    // occurrences
    // ---------------------------------------------
    // 09/20/01 T.Bobb PR00127696  Bypass CSI-R transactions if processing 
    // option 1 from
    //                             the referral menu.
    // ------------------------------------------------------------------------------
    // *********************************************
    // * This PAD builds a list of CSENet Referral *
    // * Case Id's that are less than the current
    // * referral number.
    // *********************************************
    // *** This read each has been broken out into multiple steps. Tried to do 
    // this as one extended read but it did not work correctly. Split it out and
    // it works fine. If maintenance is performed just take note as to why it
    // is coded this way.
    if (ReadServiceProvider())
    {
      if (ReadOffice())
      {
        if (ReadOfficeServiceProvider())
        {
          // *** Problem report I00114722
          // *** 02/27/01 swsrchf
          local.MaxDate.Date = new DateTime(2099, 12, 31);
          export.RefList.Index = -1;

          // *** Problem report I00114722
          // *** 02/27/01 swsrchf
          // *** Added date check to READ EACH statement
          if (AsChar(import.InterstateCase.ActionCode) == 'U')
          {
            foreach(var item in ReadInterstateCaseAssignmentInterstateCase2())
            {
              // ****************************************************
              // 09/20/01 T.Bobb Pr00127696
              // ****************************************************
              ++export.RefList.Index;
              export.RefList.CheckSize();

              export.RefList.Update.G.TransSerialNumber =
                entities.InterstateCase.TransSerialNumber;
              export.RefList.Update.G.TransactionDate =
                entities.InterstateCase.TransactionDate;

              if (export.RefList.Index + 1 < Export.RefListGroup.Capacity)
              {
              }
              else
              {
                goto Read;
              }
            }
          }
          else
          {
            local.Read.InterstateCaseId =
              TrimEnd(import.InterstateCase.InterstateCaseId) + "%%%%%%%%%%%%%%%"
              ;

            if (!IsEmpty(import.InterstateCase.InterstateCaseId) || import
              .InterstateCase.OtherFipsState > 0)
            {
              foreach(var item in ReadInterstateCaseAssignmentInterstateCase1())
              {
                // ****************************************************
                // 09/20/01 T.Bobb Pr00127696
                // ****************************************************
                if (AsChar(import.InterstateCase.ActionCode) == 'R' && Equal
                  (entities.InterstateCase.FunctionalTypeCode, "CSI"))
                {
                  continue;
                }

                ++export.RefList.Index;
                export.RefList.CheckSize();

                export.RefList.Update.G.TransSerialNumber =
                  entities.InterstateCase.TransSerialNumber;
                export.RefList.Update.G.TransactionDate =
                  entities.InterstateCase.TransactionDate;

                if (export.RefList.Index + 1 < Export.RefListGroup.Capacity)
                {
                }
                else
                {
                  goto Read;
                }
              }
            }
            else
            {
              foreach(var item in ReadInterstateCaseAssignmentInterstateCase3())
              {
                // ****************************************************
                // 09/20/01 T.Bobb Pr00127696
                // ****************************************************
                if (AsChar(import.InterstateCase.ActionCode) == 'R' && Equal
                  (entities.InterstateCase.FunctionalTypeCode, "CSI"))
                {
                  continue;
                }

                ++export.RefList.Index;
                export.RefList.CheckSize();

                export.RefList.Update.G.TransSerialNumber =
                  entities.InterstateCase.TransSerialNumber;
                export.RefList.Update.G.TransactionDate =
                  entities.InterstateCase.TransactionDate;

                if (export.RefList.Index + 1 < Export.RefListGroup.Capacity)
                {
                }
                else
                {
                  goto Read;
                }
              }
            }
          }
        }
        else
        {
          ExitState = "OFFICE_SERVICE_PROVIDER_NF";
        }
      }
      else
      {
        ExitState = "FN0000_OFFICE_NF";
      }
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_NF";
    }

Read:

    // *********************************************
    // * Store actual number of CSENet Referrals   *
    // * found for this Action code.               *
    // *********************************************
    export.HiddenMax.Count = export.RefList.Index + 1;
  }

  private IEnumerable<bool> ReadInterstateCaseAssignmentInterstateCase1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InterstateCaseAssignment.Populated = false;
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCaseAssignmentInterstateCase1",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetNullableString(
          command, "interstateCaseId", local.Read.InterstateCaseId ?? "");
        db.SetInt32(
          command, "otherFipsState", import.InterstateCase.OtherFipsState);
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 0);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 6);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 6);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 7);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 7);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 8);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 9);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 10);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 11);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 12);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 13);
        entities.InterstateCase.ActionCode = db.GetString(reader, 14);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 15);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 16);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 17);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 18);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 20);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 21);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 22);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 23);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 24);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 26);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 27);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 28);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 29);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 30);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 31);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 32);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 33);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 34);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 35);
        entities.InterstateCase.CaseType = db.GetString(reader, 36);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 37);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 40);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 41);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 48);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 56);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 57);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.InterstateCaseAssignment.Populated = true;
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignmentInterstateCase2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InterstateCaseAssignment.Populated = false;
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCaseAssignmentInterstateCase2",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 0);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 6);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 6);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 7);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 7);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 8);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 9);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 10);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 11);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 12);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 13);
        entities.InterstateCase.ActionCode = db.GetString(reader, 14);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 15);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 16);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 17);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 18);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 20);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 21);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 22);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 23);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 24);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 26);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 27);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 28);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 29);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 30);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 31);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 32);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 33);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 34);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 35);
        entities.InterstateCase.CaseType = db.GetString(reader, 36);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 37);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 40);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 41);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 48);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 56);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 57);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.InterstateCaseAssignment.Populated = true;
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignmentInterstateCase3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InterstateCaseAssignment.Populated = false;
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCaseAssignmentInterstateCase3",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 0);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 6);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 6);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 7);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 7);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 8);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 9);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 10);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 11);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 12);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 13);
        entities.InterstateCase.ActionCode = db.GetString(reader, 14);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 15);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 16);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 17);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 18);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 20);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 21);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 22);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 23);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 24);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 26);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 27);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 28);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 29);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 30);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 31);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 32);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 33);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 34);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 35);
        entities.InterstateCase.CaseType = db.GetString(reader, 36);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 37);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 40);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 41);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 48);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 56);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 57);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.InterstateCaseAssignment.Populated = true;
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A RefListGroup group.</summary>
    [Serializable]
    public class RefListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateCase G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 800;

      private InterstateCase g;
    }

    /// <summary>
    /// A value of HiddenMax.
    /// </summary>
    [JsonPropertyName("hiddenMax")]
    public Common HiddenMax
    {
      get => hiddenMax ??= new();
      set => hiddenMax = value;
    }

    /// <summary>
    /// Gets a value of RefList.
    /// </summary>
    [JsonIgnore]
    public Array<RefListGroup> RefList => refList ??= new(
      RefListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of RefList for json serialization.
    /// </summary>
    [JsonPropertyName("refList")]
    [Computed]
    public IList<RefListGroup> RefList_Json
    {
      get => refList;
      set => RefList.Assign(value);
    }

    private Common hiddenMax;
    private Array<RefListGroup> refList;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public InterstateCase Read
    {
      get => read ??= new();
      set => read = value;
    }

    private DateWorkArea maxDate;
    private InterstateCase read;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCaseAssignment interstateCaseAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private InterstateCase interstateCase;
  }
#endregion
}
