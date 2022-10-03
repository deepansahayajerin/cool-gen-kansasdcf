// Program: SI_BUILD_CSENET_REFERRAL_LIST, ID: 372497573, model: 746.
// Short name: SWE01103
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
/// A program: SI_BUILD_CSENET_REFERRAL_LIST.
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
public partial class SiBuildCsenetReferralList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_CSENET_REFERRAL_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildCsenetReferralList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildCsenetReferralList.
  /// </summary>
  public SiBuildCsenetReferralList(IContext context, Import import,
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
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer         Description
    // ---------------------------------------------
    // 5-22-95  Ken Evans         Initial development
    // 03/10/99 P. Sharp          Added service provider to read each.
    // 06/16/99 C. Scroggins      Added logic for when the service Provider ID
    //                            is not supplied.
    // 09/13/99 C. Scroggins      Added logic to select only active
    //                            Interstate Case Assignments.
    // 02/27/01 SWSRCHF I00114722 Added date check to the second READ EACH 
    // statement.
    //                            Changed the date check on the first READ EACH 
    // statement.
    //                            Increased the Group view from 350 to 800 
    // occurrences
    // ---------------------------------------------
    // ------------------------------------------------------------------------
    // 09/19/01 T.Bobb  PR00127696 If option 1 from Innterstate
    // 
    // referral menu was selected do not display any CSI-R
    // transactions.
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // 04/03/02 M.Ashworth  PR00142172 Problem: referals not showing from ISTM.
    // Solution: The user changed role codes and then moved referals to new
    // role code.  The read for osp was not qualifying on role code so it was
    // reading for the old role code(OC).
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // 07/25/02 V.Madhira   WR# 020332
    // BR: Acknowledgements (A), Cancel (C) and Reminder (M) referral 
    // transactions will appear under option 2 on ISTM.
    // FIX: When option 2 is selected on ISTM, the default action_code is set to
    // 'U' and this action_code value was passed into this CAB from PRAD.  To
    // implement this BR, if  action_code is 'U', change the READEACH for
    // interstate_case to read for action_codes 'U', 'A' , 'C'  and 'M'.
    // ------------------------------------------------------------------------
    // *********************************************
    // * This PAD builds a list of CSENet Referral *
    // * Case Id's.
    // 
    // *
    // *********************************************
    local.MaxDate.Date = new DateTime(2099, 12, 31);

    // *** This read each has been broken out into multiple steps. Tried to do 
    // this as one extended read but it did not work correctly. Split it out and
    // it works fine. If maintenance is performed just take note as to why it
    // is coded this way.
    if (import.ServiceProvider.SystemGeneratedId != 0 && import
      .Office.SystemGeneratedId != 0)
    {
      if (ReadServiceProvider())
      {
        if (ReadOffice())
        {
          // ------------------------------------------------------------------------
          // 04/03/02 M.Ashworth  PR00142172 Problem: referals not showing from 
          // ISTM.  Solution: The user changed role codes and then moved
          // referals to new role code.  The read for osp was not qualifying on
          // role code so it was reading for the old role code(OC).
          // ------------------------------------------------------------------------
          if (!IsEmpty(import.OfficeServiceProvider.RoleCode) && Lt
            (local.Null1.Date, import.OfficeServiceProvider.EffectiveDate))
          {
            if (!ReadOfficeServiceProvider1())
            {
              ExitState = "OFFICE_SERVICE_PROVIDER_NF";

              return;
            }
          }
          else if (!ReadOfficeServiceProvider2())
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            return;
          }

          export.RefList.Index = -1;

          // *** Problem report I00114722
          // *** 02/27/01 swsrchf
          // *** Changed date check on READ EACH statement
          // ***     from GREATER THAN or EQUAL TO
          // ***         to EQUAL TO
          // ------------------------------------------------------------------------
          // 07/25/02 V.Madhira   WR# 020332
          // BR: Acknowledgements (A), Cancel (C) and Reminder (M) referral 
          // transactions will appear under option 2 on ISTM.
          // FIX: When option 2 is selected on ISTM, the default action_code is 
          // set to 'U' and this action_code value was passed into this CAB from
          // PRAD.  To implement this BR, if  action_code is 'U', change the
          // READEACH for interstate_case to read for action_codes 'U', 'A' , '
          // C'  and 'M'.
          // ------------------------------------------------------------------------
          if (AsChar(import.InterstateCase.ActionCode) == 'U')
          {
            foreach(var item in ReadInterstateCaseAssignmentInterstateCase4())
            {
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
                ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

                return;
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
              foreach(var item in ReadInterstateCaseAssignmentInterstateCase2())
              {
                // ------------------------------------------------------------------------
                // 09/19/01 T.Bobb  PR00127696
                // If option 1 from Innterstate referral menu was selected
                // do not display any CSI-R transactions.
                // ------------------------------------------------------------------------
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
                  ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

                  return;
                }
              }
            }
            else
            {
              foreach(var item in ReadInterstateCaseAssignmentInterstateCase3())
              {
                // ------------------------------------------------------------------------
                // 09/19/01 T.Bobb  PR00127696
                // If option 1 from Innterstate referral menu was selected
                // do not display any CSI-R transactions.
                // ------------------------------------------------------------------------
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
                  ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

                  return;
                }
              }
            }
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
    }
    else
    {
      // *********************************************
      // * Code added to read service provider infor *
      // * mation when primary keys are not supplied.*
      // * CLS 06/16/99.                             *
      // *********************************************
      if (import.ServiceProvider.SystemGeneratedId == 0)
      {
        if (!IsEmpty(import.InterstateCase.KsCaseId))
        {
          local.Case1.Number = import.InterstateCase.KsCaseId ?? Spaces(10);
          UseSiReadOfficeOspHeader();
        }
      }

      if (ReadOfficeOfficeServiceProviderServiceProvider())
      {
        export.RefList.Index = -1;

        // *** Problem report I00114722
        // *** 02/27/01 swsrchf
        // *** Added date check to READ EACH statement
        // ------------------------------------------------------------------------
        // 07/25/02 V.Madhira   WR# 020332
        // BR: Acknowledgements (A), Cancel (C) and Reminder (M) referral 
        // transactions will appear under option 2 on ISTM.
        // FIX: When option 2 is selected on ISTM, the default action_code is 
        // set to 'U' and this action_code value was passed into this CAB from
        // PRAD.  To implement this BR, if  action_code is 'U', change the
        // READEACH for interstate_case to read for action_codes 'U', 'A' , 'C'
        // and 'M'.
        // ------------------------------------------------------------------------
        if (AsChar(import.InterstateCase.ActionCode) == 'U')
        {
          foreach(var item in ReadInterstateCaseAssignmentInterstateCase4())
          {
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
              ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

              return;
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
              // ------------------------------------------------------------------------
              // 09/19/01 T.Bobb  PR00127696
              // If option 1 from Innterstate referral menu was selected
              // do not display any CSI-R transactions.
              // ------------------------------------------------------------------------
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
                ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

                return;
              }
            }
          }
          else
          {
            foreach(var item in ReadInterstateCaseAssignmentInterstateCase3())
            {
              // ------------------------------------------------------------------------
              // 09/19/01 T.Bobb  PR00127696
              // If option 1 from Innterstate referral menu was selected
              // do not display any CSI-R transactions.
              // ------------------------------------------------------------------------
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
                ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

                return;
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
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    MoveOffice(useExport.Office, local.Office);
    local.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private IEnumerable<bool> ReadInterstateCaseAssignmentInterstateCase1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InterstateCaseAssignment.Populated = false;
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCaseAssignmentInterstateCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetString(command, "actionCode", import.InterstateCase.ActionCode);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
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
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetString(command, "actionCode", import.InterstateCase.ActionCode);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
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
        db.SetString(command, "actionCode", import.InterstateCase.ActionCode);
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

  private IEnumerable<bool> ReadInterstateCaseAssignmentInterstateCase4()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InterstateCaseAssignment.Populated = false;
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCaseAssignmentInterstateCase4",
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
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "lastName", local.ServiceProvider.LastName);
        db.SetString(command, "name", local.Office.Name);
        db.SetString(command, "typeCode", local.Office.TypeCode);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ServiceProvider.UserId = db.GetString(reader, 8);
        entities.ServiceProvider.LastName = db.GetString(reader, 9);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
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

  private bool ReadOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
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
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
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

    private Array<RefListGroup> refList;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private InterstateCase read;
    private DateWorkArea null1;
    private DateWorkArea maxDate;
    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
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
