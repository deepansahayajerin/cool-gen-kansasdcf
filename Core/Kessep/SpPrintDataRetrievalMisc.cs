// Program: SP_PRINT_DATA_RETRIEVAL_MISC, ID: 372132891, model: 746.
// Short name: SWE02266
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_MISC.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalMisc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_MISC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalMisc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalMisc.
  /// </summary>
  public SpPrintDataRetrievalMisc(IContext context, Import import, Export export)
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 12/02/1998	M Ramirez			Initial Development
    // 07/14/1999	M Ramirez			Added row lock counts
    // 07/30/1999	M Ramirez			Incarceration type for
    // 						Probation records
    // 						has changed from B to T
    // 08/31/1999	M Ramirez	PR#518		Added field
    // 						OBLIGATN/FNOBTYPDSC
    // 08/31/1999	M Ramirez			Added appropriate logic to
    // 						skip missing fields
    // 10/12/1999	PMcElderry
    // PRs: 73176, 73815, 77013, 74202, 75305, 76475, 74350
    // (Incorrect INCARCERATION ADDRESS appearing on JAIL
    // documents)
    // 12/08/1999	M Ramirez	81273		Don't count collections that
    // 						are Added and Adjusted in
    // 						the same day
    // 03/16/2000	M Ramirez	WR 000162	Added Family Violence fields
    // 05/10/2000	M Ramirez	91623		Changed CASHRCPT to use the
    // 						legal_action_id for the
    // 						standard_number, rather than a
    // 						specific Legal Action
    // 03/22/2001	M Ramirez	WR 291		Added LOCATREQ subroutine
    // 				Seg B
    // ----------------------------------------------------------------------------
    // 08/29/01        T.Bobb          PR 00118133     Changed read statement to
    // include foreign as well as domestic in case of statement ISCNTCTAD1
    // ----------------------------------------------------------------------------
    // 09/03/01        M.Kumar         PR 00126484	Abending in T region . Added 
    // code to check if the person_genetic_code
    // 						is populated before the related vendor is read .
    // 10/11/02	K Cole		WR20321		Added new functionality to DETERMN to
    // 						find interstate information from the
    // 						interstate request for incoming and
    // 						outgoing interstate involvement						
    // ----------------------------------------------------------------------------
    // --------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // --------------------------------------------------------------------------------
    // 11/02/2006	Raj S		PR#00185181	Modified to fix printing of
    //                                                 
    // current vendor address against
    //                                                 
    // previous vendor by establishing
    //                                                 
    // previous vendor record currency.
    // 09/09/2008	J Huss		CQ# 576		Modified to read from 'EDS COVERAGES' code 
    // value rather than 'MEDICAL COVERAGE'
    // --------------------------------------------------------------------------------
    UseCabSetMaximumDiscontinueDate();
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    export.SpDocKey.Assign(import.SpDocKey);

    foreach(var item in ReadField())
    {
      if (Lt(entities.Field.SubroutineName, local.Previous.SubroutineName) || Equal
        (entities.Field.SubroutineName, local.Previous.SubroutineName) && !
        Lt(local.Previous.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.ProcessGroup.Flag = "N";
      MoveField(entities.Field, local.Previous);
      local.SpPrintWorkSet.Assign(local.NullSpPrintWorkSet);

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case " DETERMN":
          switch(TrimEnd(entities.Field.Name))
          {
            case "INCINTINVL":
              if (local.InterstateRequest.IntHGeneratedId <= 0)
              {
                if (import.SpDocKey.KeyInterstateRequest <= 0)
                {
                  // No interstate request passed in
                  if (IsEmpty(import.SpDocKey.KeyCase))
                  {
                    // No case number passed in
                    if (IsEmpty(import.SpDocKey.KeyAp))
                    {
                      // No ap passed in
                      if (import.SpDocKey.KeyLegalAction <= 0)
                      {
                        // No legal action passed in
                      }
                      else
                      {
                        foreach(var item1 in ReadInterstateRequest4())
                        {
                          if (local.InterstateRequest.IntHGeneratedId == 0)
                          {
                            MoveInterstateRequest(entities.InterstateRequest,
                              local.InterstateRequest);
                          }
                          else
                          {
                            local.InterstateRequest.IntHGeneratedId = 0;

                            break;
                          }
                        }
                      }
                    }
                    else
                    {
                      // We have ap number
                      foreach(var item1 in ReadInterstateRequest8())
                      {
                        if (local.InterstateRequest.IntHGeneratedId == 0)
                        {
                          MoveInterstateRequest(entities.InterstateRequest,
                            local.InterstateRequest);
                        }
                        else
                        {
                          local.InterstateRequest.IntHGeneratedId = 0;

                          break;
                        }
                      }
                    }
                  }
                  else if (IsEmpty(import.SpDocKey.KeyAp))
                  {
                    // We have case number but no ap number
                    foreach(var item1 in ReadInterstateRequest10())
                    {
                      if (local.InterstateRequest.IntHGeneratedId == 0)
                      {
                        MoveInterstateRequest(entities.InterstateRequest,
                          local.InterstateRequest);
                      }
                      else
                      {
                        local.InterstateRequest.IntHGeneratedId = 0;

                        break;
                      }
                    }
                  }
                  else
                  {
                    // We have case number and ap number
                    foreach(var item1 in ReadInterstateRequest6())
                    {
                      if (local.InterstateRequest.IntHGeneratedId == 0)
                      {
                        MoveInterstateRequest(entities.InterstateRequest,
                          local.InterstateRequest);
                      }
                      else
                      {
                        local.InterstateRequest.IntHGeneratedId = 0;

                        break;
                      }
                    }
                  }
                }
                else
                {
                  // Interstate request passed in
                  if (ReadInterstateRequest1())
                  {
                    MoveInterstateRequest(entities.InterstateRequest,
                      local.InterstateRequest);
                  }
                }

                if (local.InterstateRequest.IntHGeneratedId <= 0)
                {
                  // Interstate request not found.
                  local.Previous.SubroutineName = "INTSTREZ";

                  continue;
                }
              }

              break;
            case "MISC FV":
              local.Fv.Flag = "Y";
              local.FieldValue.Value = "Y";

              break;
            case "OUTINTINVL":
              if (local.InterstateRequest.IntHGeneratedId <= 0)
              {
                if (import.SpDocKey.KeyInterstateRequest <= 0)
                {
                  // No interstate request passed in
                  if (IsEmpty(import.SpDocKey.KeyCase))
                  {
                    // No case number passed in
                    if (IsEmpty(import.SpDocKey.KeyAp))
                    {
                      // No ap passed in
                      if (import.SpDocKey.KeyLegalAction <= 0)
                      {
                        // No legal action passed in
                      }
                      else
                      {
                        foreach(var item1 in ReadInterstateRequest5())
                        {
                          if (local.InterstateRequest.IntHGeneratedId == 0)
                          {
                            MoveInterstateRequest(entities.InterstateRequest,
                              local.InterstateRequest);
                          }
                          else
                          {
                            local.InterstateRequest.IntHGeneratedId = 0;

                            break;
                          }
                        }
                      }
                    }
                    else
                    {
                      // We have ap number
                      foreach(var item1 in ReadInterstateRequest9())
                      {
                        if (local.InterstateRequest.IntHGeneratedId == 0)
                        {
                          MoveInterstateRequest(entities.InterstateRequest,
                            local.InterstateRequest);
                        }
                        else
                        {
                          local.InterstateRequest.IntHGeneratedId = 0;

                          break;
                        }
                      }
                    }
                  }
                  else if (IsEmpty(import.SpDocKey.KeyAp))
                  {
                    // We have case number but no ap number
                    foreach(var item1 in ReadInterstateRequest11())
                    {
                      if (local.InterstateRequest.IntHGeneratedId == 0)
                      {
                        MoveInterstateRequest(entities.InterstateRequest,
                          local.InterstateRequest);
                      }
                      else
                      {
                        local.InterstateRequest.IntHGeneratedId = 0;

                        break;
                      }
                    }
                  }
                  else
                  {
                    // We have case number and ap number
                    foreach(var item1 in ReadInterstateRequest7())
                    {
                      if (local.InterstateRequest.IntHGeneratedId == 0)
                      {
                        MoveInterstateRequest(entities.InterstateRequest,
                          local.InterstateRequest);
                      }
                      else
                      {
                        local.InterstateRequest.IntHGeneratedId = 0;

                        break;
                      }
                    }
                  }
                }
                else
                {
                  // Interstate request passed in
                  if (ReadInterstateRequest2())
                  {
                    MoveInterstateRequest(entities.InterstateRequest,
                      local.InterstateRequest);
                  }
                }

                if (local.InterstateRequest.IntHGeneratedId <= 0)
                {
                  // Interstate request not found.
                  local.Previous.SubroutineName = "INTSTREZ";

                  continue;
                }
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "APPT":
          if (!Lt(local.NullDateWorkArea.Timestamp,
            local.Appointment.CreatedTimestamp))
          {
            local.Appointment.CreatedTimestamp = import.SpDocKey.KeyAppointment;

            if (!ReadAppointment())
            {
              // mjr---> Appointment not found, but no message is given.
              local.Previous.SubroutineName = "APPTZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "APTDT/TM":
              local.DateWorkArea.Date = entities.Appointment.Date;
              local.DateWorkArea.Time = entities.Appointment.Time;
              local.FieldValue.Value = UseSpDocFormatHearingDateTime();

              break;
            case "APPTREASON":
              if (!IsEmpty(entities.Appointment.ReasonCode))
              {
                local.Code.CodeName = "APPOINTMENT REASON";
                local.CodeValue.Cdvalue = entities.Appointment.ReasonCode;
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "BANKRPTC":
          if (local.Bankruptcy.Identifier <= 0)
          {
            local.CsePerson.Number = import.SpDocKey.KeyPerson;
            local.Bankruptcy.Identifier = import.SpDocKey.KeyBankruptcy;

            if (!ReadBankruptcy())
            {
              // mjr---> Bankruptcy not found, but no message is given.
              local.Previous.SubroutineName = "BANKRPTZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "BKRPCTADD1":
              local.ProcessGroup.Flag = "A";
              local.SpPrintWorkSet.LocationType = "D";
              local.SpPrintWorkSet.Street1 =
                entities.Bankruptcy.BdcAddrStreet1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                entities.Bankruptcy.BdcAddrStreet2 ?? Spaces(25);
              local.SpPrintWorkSet.Street3 = "";
              local.SpPrintWorkSet.Street4 = "";
              local.SpPrintWorkSet.City = entities.Bankruptcy.BdcAddrCity ?? Spaces
                (15);
              local.SpPrintWorkSet.State = entities.Bankruptcy.BdcAddrState ?? Spaces
                (2);
              local.SpPrintWorkSet.ZipCode =
                entities.Bankruptcy.BdcAddrZip5 ?? Spaces(5);
              local.SpPrintWorkSet.Zip4 = entities.Bankruptcy.BdcAddrZip4 ?? Spaces
                (4);
              local.SpPrintWorkSet.Zip3 = entities.Bankruptcy.BdcAddrZip3 ?? Spaces
                (3);
              UseSpDocFormatAddress();

              break;
            case "BKRPCTNAME":
              local.FieldValue.Value =
                entities.Bankruptcy.BankruptcyDistrictCourt;

              break;
            case "BKRPCTNUM":
              local.FieldValue.Value =
                entities.Bankruptcy.BankruptcyCourtActionNo;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "CASHRCPT":
          if (entities.CashReceiptDetail.SequentialIdentifier <= 0)
          {
            if (export.SpDocKey.KeyCashRcptDetail <= 0)
            {
              local.Previous.SubroutineName = "CASHRCPZ";

              continue;
            }

            if (export.SpDocKey.KeyCashRcptEvent <= 0)
            {
              local.Previous.SubroutineName = "CASHRCPZ";

              continue;
            }

            if (export.SpDocKey.KeyCashRcptSource <= 0)
            {
              local.Previous.SubroutineName = "CASHRCPZ";

              continue;
            }

            if (export.SpDocKey.KeyCashRcptType <= 0)
            {
              local.Previous.SubroutineName = "CASHRCPZ";

              continue;
            }

            if (!ReadCashReceiptDetail())
            {
              local.Previous.SubroutineName = "CASHRCPZ";

              continue;
            }
          }

          // mjr---> Calculate these values once
          if (Equal(entities.Field.Name, "CASHADJAMT") || Equal
            (entities.Field.Name, "CASHADJRSN") || Equal
            (entities.Field.Name, "CASHCOLAMT"))
          {
            if (local.CollectionCommon.TotalCurrency != 0)
            {
              goto Test;
            }

            if (export.SpDocKey.KeyLegalAction > 0)
            {
              if (ReadLegalAction())
              {
                local.LegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;
              }
              else
              {
                goto Test;
              }

              if (IsEmpty(export.SpDocKey.KeyPerson))
              {
                export.SpDocKey.KeyPerson =
                  entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
              }

              if (IsEmpty(export.SpDocKey.KeyPersonAccount))
              {
                export.SpDocKey.KeyPersonAccount = "R";
              }

              foreach(var item1 in ReadCollection())
              {
                // mjr
                // ---------------------------------------------------
                // 12/08/1999
                // Don't count collections that are Added and Adjusted in the 
                // same day
                // ----------------------------------------------------------------
                if (Equal(entities.Collection.CourtNoticeProcessedDate,
                  entities.Collection.CourtNoticeAdjProcessDate))
                {
                  continue;
                }

                // mjr
                // ---------------------------------------------------
                // 03/24/1999
                // For some reason we are supposed to process this collection
                // now (because one of the dates = reference_date).
                // ----------------------------------------------------------------
                if (AsChar(entities.Collection.AdjustedInd) == 'Y')
                {
                  local.CollectionCommon.TotalCurrency -= entities.Collection.
                    Amount;

                  if (IsEmpty(entities.CollectionAdjustmentReason.Code))
                  {
                    if (!ReadCollectionAdjustmentReason())
                    {
                      goto ReadEach2;
                    }
                  }
                }
                else
                {
                  local.CollectionCommon.TotalCurrency += entities.Collection.
                    Amount;
                }
              }
            }
          }

Test:

          switch(TrimEnd(entities.Field.Name))
          {
            case "CASHCHCKNO":
              if (!ReadCashReceipt())
              {
                continue;
              }

              if (!IsEmpty(entities.CashReceipt.CheckNumber))
              {
                local.FieldValue.Value = entities.CashReceipt.CheckNumber;
              }

              break;
            case "CASHADJAMT":
              if (local.CollectionCommon.TotalCurrency < 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.CollectionCommon.TotalCurrency;
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "CASHADJRSN":
              if (!IsEmpty(entities.CollectionAdjustmentReason.Code))
              {
                if (Equal(entities.CollectionAdjustmentReason.Code, "FDSOADJ") ||
                  Equal
                  (entities.CollectionAdjustmentReason.Code, "IRS NEG") || Equal
                  (entities.CollectionAdjustmentReason.Code, "SDSOADJ"))
                {
                  local.FieldValue.Value = "PAYMENT ADJUSTMENT";
                }
                else
                {
                  local.FieldValue.Value =
                    entities.CollectionAdjustmentReason.Name;
                }
              }

              break;
            case "CASHCOLAMT":
              if (local.CollectionCommon.TotalCurrency > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.CollectionCommon.TotalCurrency;
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "CASHCOLDT":
              if (entities.CashReceipt.SequentialNumber <= 0)
              {
                if (!ReadCashReceipt())
                {
                  continue;
                }
              }

              if (Lt(local.NullDateWorkArea.Date,
                entities.CashReceipt.ReceivedDate))
              {
                local.DateWorkArea.Date = entities.CashReceipt.ReceivedDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "CASHCOLTYP":
              if (!ReadCollectionType())
              {
                continue;
              }

              if (Equal(entities.CollectionType.Code, "F") || Equal
                (entities.CollectionType.Code, "K") || Equal
                (entities.CollectionType.Code, "S") || Equal
                (entities.CollectionType.Code, "T") || Equal
                (entities.CollectionType.Code, "U") || Equal
                (entities.CollectionType.Code, "Y") || Equal
                (entities.CollectionType.Code, "Z") || Equal
                (entities.CollectionType.Code, "7") || Equal
                (entities.CollectionType.Code, "8") || Equal
                (entities.CollectionType.Code, "9"))
              {
                local.FieldValue.Value = "PAYMENT";
              }
              else
              {
                local.FieldValue.Value = entities.CollectionType.Name;
              }

              break;
            case "OVERCOLAMT":
              if (entities.CashReceiptDetail.CollectionAmount > 0)
              {
                local.BatchConvertNumToText.Currency =
                  entities.CashReceiptDetail.CollectionAmount;
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "OVERCOLDT":
              if (Lt(local.NullDateWorkArea.Date,
                entities.CashReceiptDetail.CollectionDate))
              {
                local.DateWorkArea.Date =
                  entities.CashReceiptDetail.CollectionDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "OVERPMTAMT":
              local.BatchConvertNumToText.Currency =
                entities.CashReceiptDetail.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();

              if (local.BatchConvertNumToText.Currency > 0)
              {
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "CONTACT":
          if (local.Contact.ContactNumber <= 0)
          {
            local.CsePerson.Number = import.SpDocKey.KeyPerson;
            local.Contact.ContactNumber = import.SpDocKey.KeyContact;

            if (!ReadContact())
            {
              // mjr---> Contact not found, but no message is given.
              local.Previous.SubroutineName = "CONTACTZ";

              continue;
            }
          }

          if (!IsEmpty(local.Fv.Flag))
          {
            if (ReadCsePerson2())
            {
              break;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "CONTCTADD1":
              local.ProcessGroup.Flag = "A";

              if (ReadContactAddress())
              {
                if (Equal(entities.ContactAddress.Country, "US") || IsEmpty
                  (entities.ContactAddress.Country))
                {
                  local.SpPrintWorkSet.LocationType = "D";
                }
                else
                {
                  local.SpPrintWorkSet.LocationType = "F";
                }

                local.SpPrintWorkSet.Street1 =
                  entities.ContactAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.ContactAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City = entities.ContactAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = entities.ContactAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  entities.ContactAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.ContactAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 = entities.ContactAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  entities.ContactAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.ContactAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.ContactAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "CONTCTNAME":
              local.SpPrintWorkSet.FirstName = entities.Contact.NameFirst ?? Spaces
                (12);
              local.SpPrintWorkSet.MidInitial =
                entities.Contact.MiddleInitial ?? Spaces(1);
              local.SpPrintWorkSet.LastName = entities.Contact.NameLast ?? Spaces
                (17);
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "CONTCTCONM":
              local.FieldValue.Value = entities.Contact.CompanyName;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "CREDTRPT":
          if (IsEmpty(local.AdministrativeActCertification.Type1))
          {
            // mjr---> Admin Action Certification
            // 	Requires obligor (CSE_Person_Acct Type = 'R' and Person Number)
            // 	and Admin Action Cert Type (so far only 'CRED') and taken date.
            local.CsePerson.Number = import.SpDocKey.KeyPerson;
            local.AdministrativeActCertification.TakenDate =
              import.SpDocKey.KeyAdminActionCert;
            local.AdministrativeActCertification.Type1 = "CRED";

            if (!IsEmpty(import.SpDocKey.KeyPersonAccount))
            {
              local.CsePersonAccount.Type1 = import.SpDocKey.KeyPersonAccount;
            }
            else
            {
              local.CsePersonAccount.Type1 = "R";
            }

            if (!ReadAdministrativeActCertification())
            {
              // mjr---> Admin Action Cert not found, but no message is given.
              local.Previous.SubroutineName = "CREDTRPZ";

              continue;
            }
          }

          if (Equal(entities.Field.Name, "CRDCERTAMT"))
          {
            local.BatchConvertNumToText.Currency =
              entities.AdministrativeActCertification.CurrentAmount.
                GetValueOrDefault();
            UseFnConvertCurrencyToText();
            local.FieldValue.Value = local.BatchConvertNumToText.TextNumber16;
          }
          else
          {
            export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
            export.ErrorFieldValue.Value = "Field:  " + TrimEnd
              (entities.Field.Name) + ",  Subroutine:  " + entities
              .Field.SubroutineName;
          }

          break;
        case "GENETIC":
          local.Document.Type1 = Substring(import.Document.Name, 1, 2);

          if (Equal(local.Document.Type1, "AR"))
          {
            local.Document.Type1 = "MO";
          }

          if (local.GeneticTest.TestNumber <= 0)
          {
            local.GeneticTest.TestNumber = import.SpDocKey.KeyGeneticTest;

            if (!ReadGeneticTest())
            {
              // mjr---> Genetic Test not found, but no message is given.
              local.Previous.SubroutineName = "GENETICZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "GTACCTNUM":
              if (ReadGeneticTestAccount())
              {
                local.FieldValue.Value =
                  entities.GeneticTestAccount.AccountNumber;
              }

              break;
            case "GTCURAPPT":
              if (Equal(local.Document.Type1, "AP"))
              {
                if (ReadPersonGeneticTest2())
                {
                  local.DateWorkArea.Date =
                    entities.PersonGeneticTest.ScheduledTestDate;
                  local.DateWorkArea.Time =
                    entities.PersonGeneticTest.ScheduledTestTime.
                      GetValueOrDefault();
                  local.FieldValue.Value = UseSpDocFormatHearingDateTime();
                }
              }
              else if (ReadPersonGeneticTest1())
              {
                local.DateWorkArea.Date =
                  entities.PersonGeneticTest.ScheduledTestDate;
                local.DateWorkArea.Time =
                  entities.PersonGeneticTest.ScheduledTestTime.
                    GetValueOrDefault();
                local.FieldValue.Value = UseSpDocFormatHearingDateTime();
              }

              break;
            case "GTCURVENNM":
              if (entities.PersonGeneticTest.Identifier <= 0)
              {
                if (Equal(local.Document.Type1, "AP"))
                {
                  ReadPersonGeneticTest2();
                }
                else
                {
                  ReadPersonGeneticTest1();
                }
              }

              if (entities.PersonGeneticTest.Populated)
              {
                if (ReadVendor1())
                {
                  local.FieldValue.Value = entities.Vendor.Name;
                }
              }

              break;
            case "GTCUVENAD1":
              local.ProcessGroup.Flag = "A";

              if (entities.PersonGeneticTest.Identifier <= 0)
              {
                if (Equal(local.Document.Type1, "AP"))
                {
                  ReadPersonGeneticTest2();
                }
                else
                {
                  ReadPersonGeneticTest1();
                }
              }

              if (IsEmpty(entities.Vendor.Name))
              {
                if (entities.PersonGeneticTest.Populated)
                {
                  if (!ReadVendor1())
                  {
                    break;
                  }
                }
                else
                {
                  break;
                }
              }

              if (entities.Vendor.Populated)
              {
                if (ReadVendorAddress())
                {
                  if (Equal(entities.VendorAddress.Country, "US") || IsEmpty
                    (entities.VendorAddress.Country))
                  {
                    local.SpPrintWorkSet.LocationType = "D";
                  }
                  else
                  {
                    local.SpPrintWorkSet.LocationType = "F";
                  }

                  local.SpPrintWorkSet.Street1 =
                    entities.VendorAddress.Street1 ?? Spaces(25);
                  local.SpPrintWorkSet.Street2 =
                    entities.VendorAddress.Street2 ?? Spaces(25);
                  local.SpPrintWorkSet.Street3 = "";
                  local.SpPrintWorkSet.Street4 = "";
                  local.SpPrintWorkSet.City = entities.VendorAddress.City ?? Spaces
                    (15);
                  local.SpPrintWorkSet.State = entities.VendorAddress.State ?? Spaces
                    (2);
                  local.SpPrintWorkSet.ZipCode =
                    entities.VendorAddress.ZipCode5 ?? Spaces(5);
                  local.SpPrintWorkSet.Zip4 =
                    entities.VendorAddress.ZipCode4 ?? Spaces(4);
                  local.SpPrintWorkSet.Zip3 = entities.VendorAddress.Zip3 ?? Spaces
                    (3);
                  local.SpPrintWorkSet.Province =
                    entities.VendorAddress.Province ?? Spaces(5);
                  local.SpPrintWorkSet.Country =
                    entities.VendorAddress.Country ?? Spaces(2);
                  local.SpPrintWorkSet.PostalCode =
                    entities.VendorAddress.PostalCode ?? Spaces(10);
                  UseSpDocFormatAddress();
                }
              }

              break;
            case "GTPATPROB":
              local.BatchConvertNumToText.Currency =
                entities.GeneticTest.PaternityProbability.GetValueOrDefault();
              UseFnConvertCurrencyToText();
              local.FieldValue.Value = local.BatchConvertNumToText.TextNumber16;

              break;
            case "GTPRVAPPT":
              local.Position.Count = 0;

              if (Equal(local.Document.Type1, "AP"))
              {
                foreach(var item1 in ReadPersonGeneticTest4())
                {
                  ++local.Position.Count;

                  if (local.Position.Count >= 2)
                  {
                    local.PersonGeneticTest.Identifier =
                      entities.PersonGeneticTest.Identifier;
                    local.DateWorkArea.Date =
                      entities.PersonGeneticTest.ScheduledTestDate;
                    local.DateWorkArea.Time =
                      entities.PersonGeneticTest.ScheduledTestTime.
                        GetValueOrDefault();
                    local.FieldValue.Value = UseSpDocFormatHearingDateTime();

                    break;
                  }
                }
              }
              else
              {
                foreach(var item1 in ReadPersonGeneticTest3())
                {
                  ++local.Position.Count;

                  if (local.Position.Count >= 2)
                  {
                    local.PersonGeneticTest.Identifier =
                      entities.PersonGeneticTest.Identifier;
                    local.DateWorkArea.Date =
                      entities.PersonGeneticTest.ScheduledTestDate;
                    local.DateWorkArea.Time =
                      entities.PersonGeneticTest.ScheduledTestTime.
                        GetValueOrDefault();
                    local.FieldValue.Value = UseSpDocFormatHearingDateTime();

                    break;
                  }
                }
              }

              break;
            case "GTPRVENAD1":
              local.ProcessGroup.Flag = "A";

              if (local.PersonGeneticTest.Identifier <= 0)
              {
                if (Equal(local.Document.Type1, "AP"))
                {
                  foreach(var item1 in ReadPersonGeneticTest4())
                  {
                    ++local.Position.Count;

                    if (local.Position.Count >= 2)
                    {
                      if (entities.PersonGeneticTest.Populated)
                      {
                        if (ReadVendor1())
                        {
                          local.Vendor.Identifier = entities.Vendor.Identifier;
                        }
                      }

                      break;
                    }
                  }
                }
                else
                {
                  foreach(var item1 in ReadPersonGeneticTest3())
                  {
                    ++local.Position.Count;

                    if (local.Position.Count >= 2)
                    {
                      if (entities.PersonGeneticTest.Populated)
                      {
                        if (ReadVendor1())
                        {
                          local.Vendor.Identifier = entities.Vendor.Identifier;
                        }
                      }

                      break;
                    }
                  }
                }
              }
              else
              {
                // **********************************************************************
                // Ref:PR#00185181
                // 
                // Change Date:11/02/2006
                // Forced to Read the Vendor record for the previous person 
                // genetic Test,
                // otherwise keeps the current person genetic test vendor record
                // currency
                // and prints the current vendor address on the document.
                // **********************************************************************
                if (entities.PersonGeneticTest.Populated)
                {
                  if (ReadVendor1())
                  {
                    local.Vendor.Identifier = entities.Vendor.Identifier;
                  }
                }
              }

              if (entities.Vendor.Populated)
              {
                if (ReadVendorAddress())
                {
                  if (Equal(entities.VendorAddress.Country, "US") || IsEmpty
                    (entities.VendorAddress.Country))
                  {
                    local.SpPrintWorkSet.LocationType = "D";
                  }
                  else
                  {
                    local.SpPrintWorkSet.LocationType = "F";
                  }

                  local.SpPrintWorkSet.Street1 =
                    entities.VendorAddress.Street1 ?? Spaces(25);
                  local.SpPrintWorkSet.Street2 =
                    entities.VendorAddress.Street2 ?? Spaces(25);
                  local.SpPrintWorkSet.Street3 = "";
                  local.SpPrintWorkSet.Street4 = "";
                  local.SpPrintWorkSet.City = entities.VendorAddress.City ?? Spaces
                    (15);
                  local.SpPrintWorkSet.State = entities.VendorAddress.State ?? Spaces
                    (2);
                  local.SpPrintWorkSet.ZipCode =
                    entities.VendorAddress.ZipCode5 ?? Spaces(5);
                  local.SpPrintWorkSet.Zip4 =
                    entities.VendorAddress.ZipCode4 ?? Spaces(4);
                  local.SpPrintWorkSet.Zip3 = entities.VendorAddress.Zip3 ?? Spaces
                    (3);
                  local.SpPrintWorkSet.Province =
                    entities.VendorAddress.Province ?? Spaces(5);
                  local.SpPrintWorkSet.Country =
                    entities.VendorAddress.Country ?? Spaces(2);
                  local.SpPrintWorkSet.PostalCode =
                    entities.VendorAddress.PostalCode ?? Spaces(10);
                  UseSpDocFormatAddress();
                }
              }

              break;
            case "GTPRVVENNM":
              if (local.Vendor.Identifier <= 0)
              {
                local.Position.Count = 0;

                if (Equal(local.Document.Type1, "AP"))
                {
                  foreach(var item1 in ReadPersonGeneticTest4())
                  {
                    ++local.Position.Count;

                    if (local.Position.Count >= 2)
                    {
                      if (entities.PersonGeneticTest.Populated)
                      {
                        ReadVendor1();
                      }

                      break;
                    }
                  }
                }
                else
                {
                  foreach(var item1 in ReadPersonGeneticTest3())
                  {
                    ++local.Position.Count;

                    if (local.Position.Count >= 2)
                    {
                      if (entities.PersonGeneticTest.Populated)
                      {
                        ReadVendor1();
                      }

                      break;
                    }
                  }
                }
              }

              local.FieldValue.Value = entities.Vendor.Name;

              break;
            case "GTTSTVENNM":
              if (!ReadVendor2())
              {
                break;
              }

              local.FieldValue.Value = entities.Vendor.Name;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "INFORQST":
          if (local.InformationRequest.Number <= 0)
          {
            local.InformationRequest.Number = import.SpDocKey.KeyInfoRequest;

            if (!ReadInformationRequest())
            {
              // mjr---> Information Request not found, but no message is given.
              local.Previous.SubroutineName = "INFORQSZ";

              continue;
            }
          }

          if (Equal(entities.Field.Name, "IRQNUMBER"))
          {
            local.FieldValue.Value =
              NumberToString(entities.InformationRequest.Number, 6, 10);
          }
          else
          {
            export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
            export.ErrorFieldValue.Value = "Field:  " + TrimEnd
              (entities.Field.Name) + ",  Subroutine:  " + entities
              .Field.SubroutineName;
          }

          break;
        case "INSURANC":
          if (entities.HealthInsuranceCoverage.Identifier <= 0)
          {
            local.HealthInsuranceCoverage.Identifier =
              import.SpDocKey.KeyHealthInsCoverage;

            if (!ReadHealthInsuranceCoverage())
            {
              local.Previous.Name = local.CurrentCaseRole.Type1 + "INSZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "APINSCOAD1":
              local.ProcessGroup.Flag = "A";

              if (ReadHealthInsuranceCompanyAddress())
              {
                if (IsEmpty(entities.HealthInsuranceCompanyAddress.Country) || Equal
                  (entities.HealthInsuranceCompanyAddress.Country, "US"))
                {
                  local.SpPrintWorkSet.LocationType = "D";
                }
                else
                {
                  local.SpPrintWorkSet.LocationType = "F";
                }

                local.SpPrintWorkSet.Street1 =
                  entities.HealthInsuranceCompanyAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.HealthInsuranceCompanyAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City =
                  entities.HealthInsuranceCompanyAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.HealthInsuranceCompanyAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.HealthInsuranceCompanyAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.HealthInsuranceCompanyAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.HealthInsuranceCompanyAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Province =
                  entities.HealthInsuranceCompanyAddress.Province ?? Spaces(5);
                local.SpPrintWorkSet.Country =
                  entities.HealthInsuranceCompanyAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.HealthInsuranceCompanyAddress.PostalCode ?? Spaces
                  (10);
                UseSpDocFormatAddress();
              }

              break;
            case "APINSCONM":
              if (ReadHealthInsuranceCompany())
              {
                local.FieldValue.Value =
                  entities.HealthInsuranceCompany.InsurancePolicyCarrier;
              }
              else
              {
                continue;
              }

              break;
            case "APINSCOPH":
              if (IsEmpty(entities.HealthInsuranceCompany.InsurancePolicyCarrier))
                
              {
                if (!ReadHealthInsuranceCompany())
                {
                  continue;
                }
              }

              if (Lt(0, entities.HealthInsuranceCompany.InsurerPhone))
              {
                local.SpPrintWorkSet.PhoneAreaCode =
                  entities.HealthInsuranceCompany.InsurerPhoneAreaCode.
                    GetValueOrDefault();
                local.SpPrintWorkSet.Phone7Digit =
                  entities.HealthInsuranceCompany.InsurerPhone.
                    GetValueOrDefault();
                local.SpPrintWorkSet.PhoneExt =
                  entities.HealthInsuranceCompany.InsurerPhoneExt ?? Spaces(5);
                local.FieldValue.Value = UseSpDocFormatPhoneNumber();
              }

              break;
            case "APINSENDDT":
              if (IsEmpty(local.Ch.Number))
              {
                local.Ch.Number = import.SpDocKey.KeyChild;

                if (!ReadPersonalHealthInsurance())
                {
                  local.Previous.Name = local.CurrentCaseRole.Type1 + "INSZ";

                  continue;
                }
              }

              local.DateWorkArea.Date =
                entities.PersonalHealthInsurance.CoverageEndDate;
              local.FieldValue.Value = UseSpDocFormatDate();

              break;
            case "APINSGRPNO":
              local.FieldValue.Value =
                entities.HealthInsuranceCoverage.InsuranceGroupNumber;

              break;
            case "APINSPOLNO":
              local.FieldValue.Value =
                entities.HealthInsuranceCoverage.InsurancePolicyNumber;

              break;
            case "APINSSTRDT":
              if (IsEmpty(local.Ch.Number))
              {
                local.Ch.Number = import.SpDocKey.KeyChild;

                if (!ReadPersonalHealthInsurance())
                {
                  local.Previous.Name = local.CurrentCaseRole.Type1 + "INSZ";

                  continue;
                }
              }

              local.DateWorkArea.Date =
                entities.PersonalHealthInsurance.CoverageBeginDate;
              local.FieldValue.Value = UseSpDocFormatDate();

              break;
            case "APINSTYPE1":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode1))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode1 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "APINSTYPE2":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode2))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode2 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "APINSTYPE3":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode3))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode3 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "APINSTYPE4":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode4))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode4 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "APINSTYPE5":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode5))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode5 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "APINSTYPE6":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode6))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode6 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "APINSTYPE7":
              if (!IsEmpty(entities.HealthInsuranceCoverage.CoverageCode7))
              {
                // J Huss	09/09/2008	Modified to read from 'EDS COVERAGES' code 
                // value rather than 'MEDICAL COVERAGE'
                local.Code.CodeName = "EDS COVERAGES";
                local.CodeValue.Cdvalue =
                  entities.HealthInsuranceCoverage.CoverageCode7 ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "INTSTREQ":
          if (local.InterstateRequest.IntHGeneratedId <= 0)
          {
            if (ReadInterstateRequest3())
            {
              MoveInterstateRequest(entities.InterstateRequest,
                local.InterstateRequest);
            }
            else
            {
              // Skip the fields as interstate request not found.
              local.Previous.SubroutineName = "INTSTREZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "ISCNTCTAD1":
              local.ProcessGroup.Flag = "A";

              if (ReadInterstateContact())
              {
                local.InterstateContact.Assign(entities.InterstateContact);

                // ****************************************************************
                // Added check for foreign address
                // 08/29/01 T.Bobb PR 00118133
                // ****************************************************************
                if (ReadInterstateContactAddress())
                {
                  local.InterstateContactAddress.Assign(
                    entities.InterstateContactAddress);
                }
              }

              // >>
              // 09/01/01  T.Bobb  Only check for domestic
              if (IsEmpty(local.InterstateContactAddress.Street1) && AsChar
                (entities.InterstateContactAddress.LocationType) == 'D')
              {
                local.Fips.State = entities.InterstateRequest.OtherStateFips;
                UseSiInterstateUnitLookup();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NN0000_ALL_OK";

                  break;
                }
              }

              local.SpPrintWorkSet.LocationType =
                local.InterstateContactAddress.LocationType;
              local.SpPrintWorkSet.Street1 =
                local.InterstateContactAddress.Street1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                local.InterstateContactAddress.Street2 ?? Spaces(25);
              local.SpPrintWorkSet.Street3 =
                local.InterstateContactAddress.Street3 ?? Spaces(25);
              local.SpPrintWorkSet.Street4 =
                local.InterstateContactAddress.Street4 ?? Spaces(25);
              local.SpPrintWorkSet.City =
                local.InterstateContactAddress.City ?? Spaces(15);
              local.SpPrintWorkSet.State =
                local.InterstateContactAddress.State ?? Spaces(2);
              local.SpPrintWorkSet.ZipCode =
                local.InterstateContactAddress.ZipCode ?? Spaces(5);
              local.SpPrintWorkSet.Zip4 =
                local.InterstateContactAddress.Zip4 ?? Spaces(4);
              local.SpPrintWorkSet.Zip3 =
                local.InterstateContactAddress.Zip3 ?? Spaces(3);
              local.SpPrintWorkSet.Country =
                local.InterstateContactAddress.Country ?? Spaces(2);
              local.SpPrintWorkSet.Province =
                local.InterstateContactAddress.Province ?? Spaces(5);
              local.SpPrintWorkSet.PostalCode =
                local.InterstateContactAddress.PostalCode ?? Spaces(10);
              UseSpDocFormatAddress();

              break;
            case "ISCNTCTNM":
              if (IsEmpty(local.InterstateContact.NameLast))
              {
                if (ReadInterstateContact())
                {
                  local.InterstateContact.Assign(entities.InterstateContact);
                }
              }

              if (!IsEmpty(local.InterstateContact.NameLast))
              {
                local.SpPrintWorkSet.FirstName =
                  local.InterstateContact.NameFirst ?? Spaces(12);
                local.SpPrintWorkSet.MidInitial =
                  local.InterstateContact.NameMiddle ?? Spaces(1);
                local.SpPrintWorkSet.LastName =
                  local.InterstateContact.NameLast ?? Spaces(17);
                local.FieldValue.Value = UseSpDocFormatName();
              }

              break;
            case "OOSCASENUM":
              local.FieldValue.Value =
                local.InterstateRequest.OtherStateCaseId ?? "";

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "JAIL":
          if (!IsEmpty(local.Fv.Flag))
          {
            if (ReadCsePerson1())
            {
              break;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "JAILADDR1":
              local.ProcessGroup.Flag = "A";

              if (local.JailPrison.Identifier <= 0)
              {
                if (ReadIncarceration1())
                {
                  local.JailPrison.Identifier =
                    entities.Incarceration.Identifier;
                }

                if (local.JailPrison.Identifier <= 0)
                {
                  local.Previous.Name = "JAILNAMEZ";

                  continue;
                }
              }

              if (ReadIncarcerationAddress())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 =
                  entities.IncarcerationAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.IncarcerationAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City =
                  entities.IncarcerationAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.IncarcerationAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.IncarcerationAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.IncarcerationAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.IncarcerationAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Province =
                  entities.IncarcerationAddress.Province ?? Spaces(5);
                local.SpPrintWorkSet.Country =
                  entities.IncarcerationAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.IncarcerationAddress.PostalCode ?? Spaces(10);
                UseSpDocFormatAddress();
              }

              break;
            case "JAILNAME":
              if (local.JailPrison.Identifier <= 0)
              {
                if (ReadIncarceration1())
                {
                  local.JailPrison.Identifier =
                    entities.Incarceration.Identifier;
                }

                if (local.JailPrison.Identifier <= 0)
                {
                  local.Previous.Name = "JAILNAMEZ";

                  continue;
                }
              }

              local.FieldValue.Value = entities.Incarceration.InstitutionName;

              break;
            case "JAILPOADD1":
              local.ProcessGroup.Flag = "A";

              if (local.ParoleProbation.Identifier <= 0)
              {
                // mjr---> Probation has been changed to T (from B)
                if (ReadIncarceration2())
                {
                  local.ParoleProbation.Identifier =
                    entities.Incarceration.Identifier;
                }

                if (local.ParoleProbation.Identifier <= 0)
                {
                  local.Previous.SubroutineName = "JAILZ";

                  continue;
                }
              }

              if (ReadIncarcerationAddress())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 =
                  entities.IncarcerationAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.IncarcerationAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City =
                  entities.IncarcerationAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.IncarcerationAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.IncarcerationAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.IncarcerationAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.IncarcerationAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Province =
                  entities.IncarcerationAddress.Province ?? Spaces(5);
                local.SpPrintWorkSet.Country =
                  entities.IncarcerationAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.IncarcerationAddress.PostalCode ?? Spaces(10);
                UseSpDocFormatAddress();
              }

              break;
            case "JAILPONAME":
              if (local.ParoleProbation.Identifier <= 0)
              {
                // mjr---> Probation has been changed to T (from B)
                if (ReadIncarceration2())
                {
                  local.ParoleProbation.Identifier =
                    entities.Incarceration.Identifier;
                }

                if (local.ParoleProbation.Identifier <= 0)
                {
                  local.Previous.SubroutineName = "JAILZ";

                  continue;
                }
              }

              local.SpPrintWorkSet.FirstName =
                entities.Incarceration.ParoleOfficerFirstName ?? Spaces(12);
              local.SpPrintWorkSet.MidInitial =
                entities.Incarceration.ParoleOfficerMiddleInitial ?? Spaces(1);
              local.SpPrintWorkSet.LastName =
                entities.Incarceration.ParoleOfficerLastName ?? Spaces(17);
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "LOCATREQ":
          if (Equal(local.LocateRequest.CreatedTimestamp,
            local.NullDateWorkArea.Timestamp))
          {
            if (!IsEmpty(import.SpDocKey.KeyLocateRequestAgency))
            {
              local.LocateRequest.AgencyNumber =
                import.SpDocKey.KeyLocateRequestAgency;
            }
            else
            {
              local.Previous.SubroutineName = "LOCATREZ";

              continue;
            }

            if (import.SpDocKey.KeyLocateRequestSource > 0)
            {
              local.LocateRequest.SequenceNumber =
                import.SpDocKey.KeyLocateRequestSource;
            }
            else
            {
              local.Previous.SubroutineName = "LOCATREZ";

              continue;
            }

            if (!IsEmpty(import.SpDocKey.KeyPerson))
            {
              local.LocateRequest.CsePersonNumber = import.SpDocKey.KeyPerson;
            }
            else if (!IsEmpty(import.SpDocKey.KeyAp))
            {
              local.LocateRequest.CsePersonNumber = import.SpDocKey.KeyPerson;
            }
            else
            {
              local.Previous.SubroutineName = "LOCATREZ";

              continue;
            }

            if (ReadLocateRequest())
            {
              local.LocateRequest.Assign(entities.LocateRequest);
            }
            else
            {
              local.Previous.SubroutineName = "LOCATREZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "LOCRQADDR1":
              local.ProcessGroup.Flag = "A";

              if (!IsEmpty(local.Fv.Flag))
              {
                if (ReadCsePerson1())
                {
                  break;
                }
              }

              if (!IsEmpty(local.LocateRequest.Street1))
              {
                local.SpPrintWorkSet.LocationType =
                  local.LocateRequest.AddressType ?? Spaces(1);
                local.SpPrintWorkSet.Street1 = local.LocateRequest.Street1 ?? Spaces
                  (25);
                local.SpPrintWorkSet.Street2 = local.LocateRequest.Street2 ?? Spaces
                  (25);
                local.SpPrintWorkSet.Street3 = local.LocateRequest.Street3 ?? Spaces
                  (25);
                local.SpPrintWorkSet.Street4 = local.LocateRequest.Street4 ?? Spaces
                  (25);
                local.SpPrintWorkSet.City = local.LocateRequest.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = local.LocateRequest.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode = local.LocateRequest.ZipCode5 ?? Spaces
                  (5);
                local.SpPrintWorkSet.Zip4 = local.LocateRequest.ZipCode4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = local.LocateRequest.ZipCode3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Province =
                  local.LocateRequest.Province ?? Spaces(5);
                local.SpPrintWorkSet.Country = local.LocateRequest.Country ?? Spaces
                  (2);
                local.SpPrintWorkSet.PostalCode =
                  local.LocateRequest.PostalCode ?? Spaces(10);
                UseSpDocFormatAddress();
              }

              break;
            case "LOCRQAGNNM":
              // mjr
              // --------------------------------------------
              // 03/22/2001
              // Agency Name is stored in code value table in the
              // following format:
              // code_value cdvalue = xxxxyy where xxxx is the
              // 	agency_number and yy is the sequence_number;
              // code_value description = <Agency Name>;<Sub Agency Name>
              // ---------------------------------------------------------
              local.Code.CodeName = "LICENSING AGENCY SOURCE(S)";
              local.CodeValue.Cdvalue =
                Substring(local.LocateRequest.AgencyNumber,
                LocateRequest.AgencyNumber_MaxLength, 2, 4) + NumberToString
                (local.LocateRequest.SequenceNumber, 14, 2);
              UseCabGetCodeValueDescription();
              local.Position.Count = Find(local.CodeValue.Description, ";");

              if (local.Position.Count > 0)
              {
                local.FieldValue.Value =
                  Substring(local.CodeValue.Description, 1,
                  local.Position.Count - 1);
              }
              else
              {
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "LOCRQSRCNM":
              local.FieldValue.Value =
                local.LocateRequest.LicenseSourceName ?? "";

              break;
            case "LRLICEXPDT":
              if (Lt(local.NullDateWorkArea.Date,
                local.LocateRequest.LicenseExpirationDate))
              {
                local.DateWorkArea.Date =
                  local.LocateRequest.LicenseExpirationDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "LRLICISSDT":
              if (Lt(local.NullDateWorkArea.Date,
                local.LocateRequest.LicenseIssuedDate))
              {
                local.DateWorkArea.Date = local.LocateRequest.LicenseIssuedDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "LRLICNUMBR":
              local.FieldValue.Value = local.LocateRequest.LicenseNumber ?? "";

              break;
            case "LRLICSUSDT":
              if (Lt(local.NullDateWorkArea.Date,
                local.LocateRequest.LicenseSuspendedDate))
              {
                local.DateWorkArea.Date =
                  local.LocateRequest.LicenseSuspendedDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "LRLICSUSFL":
              local.FieldValue.Value =
                local.LocateRequest.LicenseSuspensionInd ?? "";

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "MILITARY":
          if (!Lt(local.NullDateWorkArea.Date,
            local.MilitaryService.EffectiveDate))
          {
            local.CsePerson.Number = import.SpDocKey.KeyPerson;
            local.MilitaryService.EffectiveDate =
              import.SpDocKey.KeyMilitaryService;

            if (!ReadMilitaryService())
            {
              // mjr---> Military service not found, but no message is given.
              local.Previous.SubroutineName = "MILITARZ";

              continue;
            }
          }

          if (!IsEmpty(local.Fv.Flag))
          {
            if (ReadCsePerson2())
            {
              break;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "MILIADDR1":
              local.ProcessGroup.Flag = "A";

              if (Equal(entities.MilitaryService.Country, "US") || IsEmpty
                (entities.MilitaryService.Country))
              {
                local.SpPrintWorkSet.LocationType = "D";
              }
              else
              {
                local.SpPrintWorkSet.LocationType = "F";
              }

              local.SpPrintWorkSet.Street1 =
                entities.MilitaryService.Street1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                entities.MilitaryService.Street2 ?? Spaces(25);
              local.SpPrintWorkSet.Street3 = "";
              local.SpPrintWorkSet.Street4 = "";
              local.SpPrintWorkSet.City = entities.MilitaryService.City ?? Spaces
                (15);
              local.SpPrintWorkSet.State = entities.MilitaryService.State ?? Spaces
                (2);
              local.SpPrintWorkSet.ZipCode =
                entities.MilitaryService.ZipCode5 ?? Spaces(5);
              local.SpPrintWorkSet.Zip4 = entities.MilitaryService.ZipCode4 ?? Spaces
                (4);
              local.SpPrintWorkSet.Zip3 = entities.MilitaryService.Zip3 ?? Spaces
                (3);
              local.SpPrintWorkSet.Country =
                entities.MilitaryService.Country ?? Spaces(2);
              local.SpPrintWorkSet.PostalCode =
                entities.MilitaryService.PostalCode ?? Spaces(10);
              local.SpPrintWorkSet.Province =
                entities.MilitaryService.Province ?? Spaces(5);
              UseSpDocFormatAddress();

              break;
            case "MILICONM":
              local.SpPrintWorkSet.FirstName =
                entities.MilitaryService.CommandingOfficerFirstName ?? Spaces
                (12);
              local.SpPrintWorkSet.MidInitial =
                entities.MilitaryService.CommandingOfficerMi ?? Spaces(1);
              local.SpPrintWorkSet.LastName =
                entities.MilitaryService.CommandingOfficerLastName ?? Spaces
                (17);
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "MILISTATNM":
              local.FieldValue.Value =
                entities.MilitaryService.CurrentUsDutyStation;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "OBLIGATN":
          local.CurrentField.Name =
            Substring(entities.Field.Name, Field.Name_MaxLength, 1, 9) + "*";
          local.CurrentNumber.Name = Substring(entities.Field.Name, 10, 1);

          if (!Lt(local.CurrentNumber.Name, "0") && !
            Lt("9", local.CurrentNumber.Name))
          {
            if (local.Local1.Count == 0)
            {
              // mjr---> Populate group of obligations
              local.Local1.Index = -1;

              if (!IsEmpty(import.SpDocKey.KeyPerson))
              {
                if (IsEmpty(import.SpDocKey.KeyPersonAccount))
                {
                  local.CsePersonAccount.Type1 = "R";
                }

                ReadCsePersonAccount1();
              }
              else if (import.SpDocKey.KeyRecaptureRule > 0)
              {
                ReadCsePersonAccount2();
              }
              else
              {
              }

              if (IsEmpty(entities.CsePersonAccount.Type1))
              {
                local.Previous.SubroutineName = "OBLIGATZ";

                continue;
              }

              foreach(var item1 in ReadObligationObligationType())
              {
                // mjr
                // ----------------------------------------------
                // 08/31/1999
                // Check that the obligation is active
                // (copied from SP_DOC_GET_SERVICE_PROVIDER)
                // -----------------------------------------------------------
                if (ReadDebtDetail())
                {
                  if (Lt(local.NullDateWorkArea.Date,
                    entities.DebtDetail.RetiredDt))
                  {
                    continue;
                  }

                  if (entities.DebtDetail.BalanceDueAmt + entities
                    .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() <= 0)
                  {
                    continue;
                  }
                }
                else
                {
                  continue;
                }

                local.PaymentScheduleFound.Flag = "N";

                foreach(var item2 in ReadObligationPaymentSchedule())
                {
                  local.PaymentScheduleFound.Flag = "Y";

                  ++local.Local1.Index;
                  local.Local1.CheckSize();

                  local.Local1.Update.Gobligation.SystemGeneratedIdentifier =
                    entities.Obligation.SystemGeneratedIdentifier;
                  local.Local1.Update.GobligationType.Assign(
                    entities.ObligationType);
                  local.Local1.Update.GdebtDetail.Assign(entities.DebtDetail);
                  local.Local1.Update.GobligationPaymentSchedule.Assign(
                    entities.ObligationPaymentSchedule);

                  if (local.Local1.Index + 1 >= Local.LocalGroup.Capacity)
                  {
                    goto ReadEach1;
                  }
                }

                // mjr
                // --------------------------------------------
                // 08/31/1999
                // Even though no payment schedule was found, the obligation
                // still needs to be displayed.
                // ---------------------------------------------------------
                if (AsChar(local.PaymentScheduleFound.Flag) == 'N')
                {
                  ++local.Local1.Index;
                  local.Local1.CheckSize();

                  local.Local1.Update.Gobligation.SystemGeneratedIdentifier =
                    entities.Obligation.SystemGeneratedIdentifier;
                  local.Local1.Update.GobligationType.Assign(
                    entities.ObligationType);
                  local.Local1.Update.GdebtDetail.Assign(entities.DebtDetail);
                }

                if (local.Local1.Index + 1 >= Local.LocalGroup.Capacity)
                {
                  break;
                }
              }

ReadEach1:
              ;
            }

            local.Local1.Index = (int)StringToNumber(local.CurrentNumber.Name);
            local.Local1.CheckSize();

            local.Obligation.SystemGeneratedIdentifier =
              local.Local1.Item.Gobligation.SystemGeneratedIdentifier;
            local.ObligationType.Assign(local.Local1.Item.GobligationType);
            local.DebtDetail.Assign(local.Local1.Item.GdebtDetail);
            local.ObligationPaymentSchedule.Assign(
              local.Local1.Item.GobligationPaymentSchedule);
          }
          else if (local.Obligation.SystemGeneratedIdentifier <= 0)
          {
            // mjr
            // -----------------------------------------------
            // 03/01/1999
            // Uses imports only:
            // 	key_person, key_person_account, key_obligation,
            // 	key_obligation_type.
            // This may need to be changed if key_ap or key_ar is ever
            // used with obligations.
            // ------------------------------------------------------------
            if (!IsEmpty(import.SpDocKey.KeyPersonAccount))
            {
              local.CsePersonAccount.Type1 = import.SpDocKey.KeyPersonAccount;
            }
            else
            {
              local.CsePersonAccount.Type1 = "R";
            }

            if (!ReadCsePersonAccount1())
            {
              local.Previous.SubroutineName = "OBLIGATZ";

              continue;
            }

            if (ReadObligationObligationTypeDebtDetail())
            {
              local.Obligation.SystemGeneratedIdentifier =
                entities.Obligation.SystemGeneratedIdentifier;
              local.ObligationType.Assign(entities.ObligationType);
              local.DebtDetail.Assign(entities.DebtDetail);
            }
          }

          if (local.Obligation.SystemGeneratedIdentifier <= 0)
          {
            local.Previous.Name =
              Substring(local.CurrentField.Name, Field.Name_MaxLength, 1, 9) + "9"
              ;

            continue;
          }

          switch(TrimEnd(local.CurrentField.Name))
          {
            case "FNOBLGAMT*":
              if (local.ObligationPaymentSchedule.Amount.GetValueOrDefault() > 0
                )
              {
                local.BatchConvertNumToText.Currency =
                  local.ObligationPaymentSchedule.Amount.GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "FNOBLGBAL*":
              local.BatchConvertNumToText.Currency =
                local.DebtDetail.BalanceDueAmt + local
                .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
              UseFnConvertCurrencyToText();
              local.FieldValue.Value = local.BatchConvertNumToText.TextNumber16;

              break;
            case "FNOBLGFRQ*":
              if (!IsEmpty(local.ObligationPaymentSchedule.FrequencyCode))
              {
                local.Code.CodeName = "FREQUENCY";
                local.CodeValue.Cdvalue =
                  local.ObligationPaymentSchedule.FrequencyCode;
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "FNOBLGTYP*":
              if (Equal(local.ObligationType.Code, "IRS NEG"))
              {
                local.FieldValue.Value = "RECOVERY";
              }
              else
              {
                local.FieldValue.Value = local.ObligationType.Code;
              }

              break;
            case "FNOBLG1DT*":
              if (Lt(local.NullDateWorkArea.Date,
                local.ObligationPaymentSchedule.StartDt))
              {
                local.DateWorkArea.Date =
                  local.ObligationPaymentSchedule.StartDt;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "FNOBTYPDS*":
              if (Equal(local.ObligationType.Code, "IRS NEG"))
              {
                local.FieldValue.Value = "RECOVERY";
              }
              else
              {
                local.FieldValue.Value = local.ObligationType.Name;
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "RCAPTURE":
          if (local.RecaptureRule.SystemGeneratedIdentifier <= 0)
          {
            if (import.SpDocKey.KeyRecaptureRule > 0)
            {
              if (ReadRecaptureRule())
              {
                if (Lt(local.NullDateWorkArea.Date,
                  entities.RecaptureRule.NegotiatedDate))
                {
                  local.RecaptureRule.Assign(entities.RecaptureRule);
                }
              }
            }
            else
            {
              // mjr---> Require the recapture_rule id
            }
          }

          if (local.RecaptureRule.SystemGeneratedIdentifier <= 0)
          {
            local.Previous.SubroutineName = "RCAPTURZ";

            continue;
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "RCAPARRAMT":
              if (local.RecaptureRule.NonAdcArrearsAmount.
                GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.RecaptureRule.NonAdcArrearsAmount.GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "RCAPARRMAX":
              if (local.RecaptureRule.NonAdcArrearsMaxAmount.
                GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.RecaptureRule.NonAdcArrearsMaxAmount.
                    GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "RCAPARRPCT":
              if (local.RecaptureRule.NonAdcArrearsPercentage.
                GetValueOrDefault() > 0)
              {
                local.FieldValue.Value =
                  NumberToString(local.RecaptureRule.NonAdcArrearsPercentage.
                    GetValueOrDefault(), 13, 3);
              }

              break;
            case "RCAPCURAMT":
              if (local.RecaptureRule.NonAdcCurrentAmount.
                GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.RecaptureRule.NonAdcCurrentAmount.GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "RCAPCURMAX":
              if (local.RecaptureRule.NonAdcCurrentMaxAmount.
                GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.RecaptureRule.NonAdcCurrentMaxAmount.
                    GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "RCAPCURPCT":
              if (local.RecaptureRule.NonAdcCurrentPercentage.
                GetValueOrDefault() > 0)
              {
                local.FieldValue.Value =
                  NumberToString(local.RecaptureRule.NonAdcCurrentPercentage.
                    GetValueOrDefault(), 13, 3);
              }

              break;
            case "RCAPEFFDT":
              if (Lt(local.NullDateWorkArea.Date,
                local.RecaptureRule.EffectiveDate))
              {
                local.DateWorkArea.Date = local.RecaptureRule.EffectiveDate;
                local.FieldValue.Value = UseSpDocFormatDate();
              }

              break;
            case "RCAPPASAMT":
              if (local.RecaptureRule.PassthruAmount.GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.RecaptureRule.PassthruAmount.GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "RCAPPASMAX":
              if (local.RecaptureRule.PassthruMaxAmount.GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.RecaptureRule.PassthruMaxAmount.GetValueOrDefault();
                UseFnConvertCurrencyToText();
                local.FieldValue.Value =
                  local.BatchConvertNumToText.TextNumber16;
              }

              break;
            case "RCAPPASPCT":
              if (local.RecaptureRule.PassthruPercentage.GetValueOrDefault() > 0
                )
              {
                local.FieldValue.Value =
                  NumberToString(local.RecaptureRule.PassthruPercentage.
                    GetValueOrDefault(), 13, 3);
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "RESOURCE":
          if (local.CsePersonResource.ResourceNo <= 0)
          {
            local.CsePerson.Number = import.SpDocKey.KeyPerson;
            local.CsePersonResource.ResourceNo = import.SpDocKey.KeyResource;

            if (!ReadCsePersonResource())
            {
              // mjr---> Resource not found, but no message is given.
              local.Previous.SubroutineName = "RESOURCZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "EXTAGNTAD1":
              local.ProcessGroup.Flag = "A";

              if (ReadExternalAgency())
              {
                if (ReadExternalAgencyAddress())
                {
                  local.SpPrintWorkSet.LocationType = "D";
                  local.SpPrintWorkSet.Street1 =
                    entities.ExternalAgencyAddress.Street1;
                  local.SpPrintWorkSet.Street2 =
                    entities.ExternalAgencyAddress.Street2 ?? Spaces(25);
                  local.SpPrintWorkSet.Street3 = "";
                  local.SpPrintWorkSet.Street4 = "";
                  local.SpPrintWorkSet.City =
                    entities.ExternalAgencyAddress.City;
                  local.SpPrintWorkSet.State =
                    entities.ExternalAgencyAddress.StateProvince;
                  local.SpPrintWorkSet.ZipCode =
                    entities.ExternalAgencyAddress.Zip ?? Spaces(5);
                  local.SpPrintWorkSet.Zip4 =
                    entities.ExternalAgencyAddress.Zip4 ?? Spaces(4);
                  local.SpPrintWorkSet.Zip3 =
                    entities.ExternalAgencyAddress.Zip3 ?? Spaces(3);
                  UseSpDocFormatAddress();
                }
              }
              else
              {
                local.Previous.Name = "EXTAGNTNZ";

                continue;
              }

              break;
            case "EXTAGNTNM":
              if (IsEmpty(entities.ExternalAgency.Name))
              {
                if (!ReadExternalAgency())
                {
                  break;
                }
              }

              local.FieldValue.Value = entities.ExternalAgency.Name;

              break;
            case "RESOADDR1":
              local.ProcessGroup.Flag = "A";

              if (!IsEmpty(local.Fv.Flag))
              {
                if (ReadCsePerson2())
                {
                  break;
                }
              }

              if (ReadResourceLocationAddress())
              {
                if (Equal(entities.ResourceLocationAddress.Country, "US") || IsEmpty
                  (entities.ResourceLocationAddress.Country))
                {
                  local.SpPrintWorkSet.LocationType = "D";
                }
                else
                {
                  local.SpPrintWorkSet.LocationType = "F";
                }

                local.SpPrintWorkSet.Street1 =
                  entities.ResourceLocationAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.ResourceLocationAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City =
                  entities.ResourceLocationAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.ResourceLocationAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.ResourceLocationAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.ResourceLocationAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.ResourceLocationAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Country =
                  entities.ResourceLocationAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.ResourceLocationAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.ResourceLocationAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "RESONAME":
              if (!IsEmpty(local.Fv.Flag))
              {
                if (ReadCsePerson2())
                {
                  break;
                }
              }

              local.FieldValue.Value = entities.CsePersonResource.Location;

              break;
            case "VEHCLCOLOR":
              if (!ReadCsePersonVehicle())
              {
                local.Previous.SubroutineName = "RESOURCZ";

                continue;
              }

              local.FieldValue.Value = entities.CsePersonVehicle.VehicleColor;

              break;
            case "VEHCLMAKE":
              if (entities.CsePersonVehicle.Identifier <= 0)
              {
                if (!ReadCsePersonVehicle())
                {
                  local.Previous.SubroutineName = "RESOURCZ";

                  continue;
                }
              }

              local.FieldValue.Value = entities.CsePersonVehicle.VehicleMake;

              break;
            case "VEHCLMODEL":
              if (entities.CsePersonVehicle.Identifier <= 0)
              {
                if (!ReadCsePersonVehicle())
                {
                  local.Previous.SubroutineName = "RESOURCZ";

                  continue;
                }
              }

              local.FieldValue.Value = entities.CsePersonVehicle.VehicleModel;

              break;
            case "VEHCLTAG":
              if (entities.CsePersonVehicle.Identifier <= 0)
              {
                if (!ReadCsePersonVehicle())
                {
                  local.Previous.SubroutineName = "RESOURCZ";

                  continue;
                }
              }

              local.FieldValue.Value =
                entities.CsePersonVehicle.VehicleLicenseTag;

              break;
            case "VEHCLVIN":
              if (entities.CsePersonVehicle.Identifier <= 0)
              {
                if (!ReadCsePersonVehicle())
                {
                  local.Previous.SubroutineName = "RESOURCZ";

                  continue;
                }
              }

              local.FieldValue.Value =
                entities.CsePersonVehicle.VehicleIdentificationNumber;

              break;
            case "VEHCLYEAR":
              if (entities.CsePersonVehicle.Identifier <= 0)
              {
                if (!ReadCsePersonVehicle())
                {
                  local.Previous.SubroutineName = "RESOURCZ";

                  continue;
                }
              }

              local.FieldValue.Value =
                NumberToString(entities.CsePersonVehicle.VehicleYear.
                  GetValueOrDefault(), 12, 4);

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "TRIBUNAL":
          if (local.Tribunal.Identifier <= 0)
          {
            local.Tribunal.Identifier = import.SpDocKey.KeyTribunal;

            if (!ReadTribunal())
            {
              // mjr---> Tribunal not found, but no message is given.
              local.Previous.SubroutineName = "TRIBUNAZ";

              continue;
            }
          }

          switch(TrimEnd(entities.Field.Name))
          {
            case "TRIBADDR1":
              local.ProcessGroup.Flag = "A";

              if (ReadFipsTribAddress())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 = entities.FipsTribAddress.Street1;
                local.SpPrintWorkSet.Street2 =
                  entities.FipsTribAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  entities.FipsTribAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  entities.FipsTribAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.FipsTribAddress.City;
                local.SpPrintWorkSet.State = entities.FipsTribAddress.State;
                local.SpPrintWorkSet.ZipCode = entities.FipsTribAddress.ZipCode;
                local.SpPrintWorkSet.Zip4 = entities.FipsTribAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.FipsTribAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Province =
                  entities.FipsTribAddress.Province ?? Spaces(5);
                local.SpPrintWorkSet.Country =
                  entities.FipsTribAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.FipsTribAddress.PostalCode ?? Spaces(10);
                UseSpDocFormatAddress();
              }

              break;
            case "TRIBNM":
              local.FieldValue.Value = entities.Tribunal.Name;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "DMV":
          local.ProcessGroup.Flag = "Y";

          // we will do a
          // read each cse_ks_driver's_license
          //        sorted descending 30 day lettter create date
          //       where person number = import person number
          // if 30 day letter create date < local 30 day letter create date
          // <----escape (read each)
          // end if.
          // Read legal action
          //       where legal action id = cse_ks_driver's_license legal id
          // set subscript local group std number to subscript of local group 
          // std number + 1
          // move standard number into local grp view
          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Subroutine";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;
          ExitState = "FIELD_NF";

          break;
      }

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      switch(AsChar(local.ProcessGroup.Flag))
      {
        case 'A':
          // mjr
          // ----------------------------------------------
          // Field is an address
          //    Process 1-5 of group_local_address
          // -------------------------------------------------
          local.Position.Count = Length(TrimEnd(entities.Field.Name));
          local.CurrentRoot.Name =
            Substring(entities.Field.Name, 1, local.Position.Count - 1);

          for(local.Address.Index = 0; local.Address.Index < local
            .Address.Count; ++local.Address.Index)
          {
            if (!local.Address.CheckSize())
            {
              break;
            }

            // mjr---> Increment Field Name
            local.Temp.Name = NumberToString(local.Address.Index + 1, 10);
            local.Position.Count = Verify(local.Temp.Name, "0");
            local.Temp.Name =
              Substring(local.Temp.Name, local.Position.Count, 16 -
              local.Position.Count);
            local.CurrentField.Name = TrimEnd(local.CurrentRoot.Name) + local
              .Temp.Name;
            UseSpCabCreateUpdateFieldValue2();

            if (IsExitState("DOCUMENT_FIELD_NF_RB"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.ErrorDocumentField.ScreenPrompt = "Creation Error";
              export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

              return;
            }

            local.Address.Update.GlocalAddress.Value =
              Spaces(FieldValue.Value_MaxLength);
            ++import.ExpImpRowLockFieldValue.Count;
          }

          local.Address.CheckIndex();

          break;
        case 'Y':
          local.Position.Count = Length(TrimEnd(entities.Field.Name));
          local.CurrentRoot.Name =
            Substring(entities.Field.Name, 1, local.Position.Count - 2);

          for(local.Dmv.Index = 0; local.Dmv.Index < local.Dmv.Count; ++
            local.Dmv.Index)
          {
            if (!local.Dmv.CheckSize())
            {
              break;
            }

            local.Temp.Name = NumberToString(local.Dmv.Index + 1, 10);
            local.Position.Count = Verify(local.Temp.Name, "0");
            local.Temp.Name =
              Substring(local.Temp.Name, local.Position.Count, 16 -
              local.Position.Count);

            if (local.Dmv.Index < 9)
            {
            }
            else
            {
              local.CurrentField.Name = local.CurrentRoot.Name;
            }

            local.CurrentField.Name = TrimEnd(local.CurrentRoot.Name) + local
              .Temp.Name;
            UseSpCabCreateUpdateFieldValue2();

            if (IsExitState("DOCUMENT_FIELD_NF_RB"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.ErrorDocumentField.ScreenPrompt = "Creation Error";
              export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

              return;
            }

            local.Dmv.Update.FieldValue.Value =
              Spaces(FieldValue.Value_MaxLength);
            ++import.ExpImpRowLockFieldValue.Count;
          }

          local.Dmv.CheckIndex();

          break;
        default:
          // mjr
          // ----------------------------------------------
          // Field is a single value
          //    Process local field_value
          // -------------------------------------------------
          UseSpCabCreateUpdateFieldValue1();

          if (IsExitState("DOCUMENT_FIELD_NF_RB"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Creation Error";
            export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

            return;
          }

          ++import.ExpImpRowLockFieldValue.Count;

          break;
      }

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      switch(TrimEnd(entities.Field.Name))
      {
        case "APINSCOAD1":
          local.Previous.Name = "APINSCOAD5";

          break;
        case "BKRPCTADD1":
          local.Previous.Name = "BKRPCTADD5";

          break;
        case "CONTCTADD1":
          local.Previous.Name = "CONTCTADD5";

          break;
        case "EXTAGNTAD1":
          local.Previous.Name = "EXTAGNTAD5";

          break;
        case "GTCUVENAD1":
          local.Previous.Name = "GTCUVENAD5";

          break;
        case "GTPRVENAD1":
          local.Previous.Name = "GTPRVENAD5";

          break;
        case "ISCNTCTAD1":
          local.Previous.Name = "ISCNTCTAD5";

          break;
        case "JAILADDR1":
          local.Previous.Name = "JAILADDR5";

          break;
        case "JAILPOADD1":
          local.Previous.Name = "JAILPOADD5";

          break;
        case "MILIADDR1":
          local.Previous.Name = "MILIADDR5";

          break;
        case "RESOADDR1":
          local.Previous.Name = "RESOADDR5";

          break;
        case "TRIBADDR1":
          local.Previous.Name = "TRIBADDR5";

          break;
        case "DMVSTDNUM0":
          local.Previous.Name = "DMVSTDNUM9";

          break;
        default:
          break;
      }

ReadEach2:
      ;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveExport1ToAddress(SpDocFormatAddress.Export.
    ExportGroup source, Local.AddressGroup target)
  {
    target.GlocalAddress.Value = source.G.Value;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveFieldValue1(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFieldValue2(FieldValue source, FieldValue target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
  }

  private static void MoveInterstateContactAddress(
    InterstateContactAddress source, InterstateContactAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateCaseId = source.OtherStateCaseId;
  }

  private static void MoveSpPrintWorkSet1(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Phone7Digit = source.Phone7Digit;
  }

  private static void MoveSpPrintWorkSet2(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MidInitial = source.MidInitial;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnConvertCurrencyToText()
  {
    var useImport = new FnConvertCurrencyToText.Import();
    var useExport = new FnConvertCurrencyToText.Export();

    useImport.BatchConvertNumToText.Currency =
      local.BatchConvertNumToText.Currency;

    Call(FnConvertCurrencyToText.Execute, useImport, useExport);

    local.BatchConvertNumToText.TextNumber16 =
      useExport.BatchConvertNumToText.TextNumber16;
  }

  private void UseSiInterstateUnitLookup()
  {
    var useImport = new SiInterstateUnitLookup.Import();
    var useExport = new SiInterstateUnitLookup.Export();

    MoveFips(local.Fips, useImport.Fips);

    Call(SiInterstateUnitLookup.Execute, useImport, useExport);

    MoveInterstateContactAddress(useExport.InterstateContactAddress,
      local.InterstateContactAddress);
  }

  private void UseSpCabCreateUpdateFieldValue1()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Field.Name = entities.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue2()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.CurrentField.Name;
    useImport.FieldValue.Value = local.Address.Item.GlocalAddress.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpDocFormatAddress()
  {
    var useImport = new SpDocFormatAddress.Import();
    var useExport = new SpDocFormatAddress.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatAddress.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Address, MoveExport1ToAddress);
  }

  private string UseSpDocFormatDate()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatHearingDateTime()
  {
    var useImport = new SpDocFormatHearingDateTime.Import();
    var useExport = new SpDocFormatHearingDateTime.Export();

    MoveDateWorkArea(local.DateWorkArea, useImport.DateWorkArea);

    Call(SpDocFormatHearingDateTime.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    MoveSpPrintWorkSet2(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatPhoneNumber()
  {
    var useImport = new SpDocFormatPhoneNumber.Import();
    var useExport = new SpDocFormatPhoneNumber.Export();

    MoveSpPrintWorkSet1(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatPhoneNumber.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private bool ReadAdministrativeActCertification()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.
          SetString(command, "type", local.AdministrativeActCertification.Type1);
          
        db.SetDate(
          command, "takenDt",
          local.AdministrativeActCertification.TakenDate.GetValueOrDefault());
        db.SetString(command, "cpaType", local.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 5);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAppointment()
  {
    entities.Appointment.Populated = false;

    return Read("ReadAppointment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.SpDocKey.KeyAppointment.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Appointment.ReasonCode = db.GetString(reader, 0);
        entities.Appointment.Date = db.GetDate(reader, 1);
        entities.Appointment.Time = db.GetTimeSpan(reader, 2);
        entities.Appointment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Appointment.AppTstamp = db.GetNullableDateTime(reader, 4);
        entities.Appointment.Populated = true;
      });
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
        db.SetInt32(command, "identifier", local.Bankruptcy.Identifier);
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyCourtActionNo = db.GetString(reader, 2);
        entities.Bankruptcy.BankruptcyDistrictCourt = db.GetString(reader, 3);
        entities.Bankruptcy.BdcAddrStreet1 = db.GetNullableString(reader, 4);
        entities.Bankruptcy.BdcAddrStreet2 = db.GetNullableString(reader, 5);
        entities.Bankruptcy.BdcAddrCity = db.GetNullableString(reader, 6);
        entities.Bankruptcy.BdcAddrState = db.GetNullableString(reader, 7);
        entities.Bankruptcy.BdcAddrZip5 = db.GetNullableString(reader, 8);
        entities.Bankruptcy.BdcAddrZip4 = db.GetNullableString(reader, 9);
        entities.Bankruptcy.BdcAddrZip3 = db.GetNullableString(reader, 10);
        entities.Bankruptcy.Populated = true;
      });
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 4);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 5);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", export.SpDocKey.KeyCashRcptDetail);
        db.SetInt32(command, "crtIdentifier", export.SpDocKey.KeyCashRcptType);
        db.SetInt32(command, "crvIdentifier", export.SpDocKey.KeyCashRcptEvent);
        db.
          SetInt32(command, "cstIdentifier", export.SpDocKey.KeyCashRcptSource);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 8);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetNullableDate(
          command, "crtNoticeProcDt",
          import.Infrastructure.ReferenceDate.GetValueOrDefault());
        db.SetString(command, "cpaType", export.SpDocKey.KeyPersonAccount);
        db.SetString(command, "cspNumber", export.SpDocKey.KeyPerson);
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.CarId = db.GetNullableInt32(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 16);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 17);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.Collection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.PrintName = db.GetNullableString(reader, 1);
        entities.CollectionType.Code = db.GetString(reader, 2);
        entities.CollectionType.Name = db.GetString(reader, 3);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadContact()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
        db.SetInt32(command, "contactNumber", local.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.CompanyName = db.GetNullableString(reader, 2);
        entities.Contact.NameLast = db.GetNullableString(reader, 3);
        entities.Contact.NameFirst = db.GetNullableString(reader, 4);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 5);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Contact.Populated);
    entities.ContactAddress.Populated = false;

    return Read("ReadContactAddress",
      (db, command) =>
      {
        db.SetInt32(command, "conNumber", entities.Contact.ContactNumber);
        db.SetString(command, "cspNumber", entities.Contact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ContactAddress.ConNumber = db.GetInt32(reader, 0);
        entities.ContactAddress.CspNumber = db.GetString(reader, 1);
        entities.ContactAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ContactAddress.Street1 = db.GetNullableString(reader, 3);
        entities.ContactAddress.Street2 = db.GetNullableString(reader, 4);
        entities.ContactAddress.City = db.GetNullableString(reader, 5);
        entities.ContactAddress.State = db.GetNullableString(reader, 6);
        entities.ContactAddress.Province = db.GetNullableString(reader, 7);
        entities.ContactAddress.PostalCode = db.GetNullableString(reader, 8);
        entities.ContactAddress.ZipCode5 = db.GetNullableString(reader, 9);
        entities.ContactAddress.ZipCode4 = db.GetNullableString(reader, 10);
        entities.ContactAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ContactAddress.Country = db.GetNullableString(reader, 12);
        entities.ContactAddress.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonAccount1()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(command, "type", local.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadCsePersonAccount2()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount2",
      (db, command) =>
      {
        db.
          SetInt32(command, "recaptureRuleId", import.SpDocKey.KeyRecaptureRule);
          
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadCsePersonResource()
  {
    entities.CsePersonResource.Populated = false;

    return Read("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetInt32(command, "resourceNo", local.CsePersonResource.ResourceNo);
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.CsePersonResource.Location = db.GetNullableString(reader, 2);
        entities.CsePersonResource.ExaId = db.GetNullableInt32(reader, 3);
        entities.CsePersonResource.Populated = true;
      });
  }

  private bool ReadCsePersonVehicle()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);
    entities.CsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cprCResourceNo", entities.CsePersonResource.ResourceNo);
        db.SetNullableString(
          command, "cspCNumber", entities.CsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.CsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.CsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.CsePersonVehicle.CspCNumber = db.GetNullableString(reader, 3);
        entities.CsePersonVehicle.VehicleColor =
          db.GetNullableString(reader, 4);
        entities.CsePersonVehicle.VehicleModel =
          db.GetNullableString(reader, 5);
        entities.CsePersonVehicle.VehicleMake = db.GetNullableString(reader, 6);
        entities.CsePersonVehicle.VehicleIdentificationNumber =
          db.GetNullableString(reader, 7);
        entities.CsePersonVehicle.VehicleLicenseTag =
          db.GetNullableString(reader, 8);
        entities.CsePersonVehicle.VehicleYear = db.GetNullableInt32(reader, 9);
        entities.CsePersonVehicle.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadExternalAgency()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);
    entities.ExternalAgency.Populated = false;

    return Read("ReadExternalAgency",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.CsePersonResource.ExaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExternalAgency.Name = db.GetString(reader, 1);
        entities.ExternalAgency.Populated = true;
      });
  }

  private bool ReadExternalAgencyAddress()
  {
    entities.ExternalAgencyAddress.Populated = false;

    return Read("ReadExternalAgencyAddress",
      (db, command) =>
      {
        db.SetInt32(command, "exaId", entities.ExternalAgency.Identifier);
      },
      (db, reader) =>
      {
        entities.ExternalAgencyAddress.Type1 = db.GetString(reader, 0);
        entities.ExternalAgencyAddress.Street1 = db.GetString(reader, 1);
        entities.ExternalAgencyAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExternalAgencyAddress.City = db.GetString(reader, 3);
        entities.ExternalAgencyAddress.StateProvince = db.GetString(reader, 4);
        entities.ExternalAgencyAddress.PostalCode =
          db.GetNullableString(reader, 5);
        entities.ExternalAgencyAddress.Zip = db.GetNullableString(reader, 6);
        entities.ExternalAgencyAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ExternalAgencyAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.ExternalAgencyAddress.Country = db.GetString(reader, 9);
        entities.ExternalAgencyAddress.ExaId = db.GetInt32(reader, 10);
        entities.ExternalAgencyAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "dependancy", import.Field.Dependancy);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 10);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 15);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadGeneticTest()
  {
    entities.GeneticTest.Populated = false;

    return Read("ReadGeneticTest",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", local.GeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.PaternityProbability =
          db.GetNullableDecimal(reader, 1);
        entities.GeneticTest.GtaAccountNumber = db.GetNullableString(reader, 2);
        entities.GeneticTest.VenIdentifier = db.GetNullableInt32(reader, 3);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 4);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 5);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 6);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 7);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 8);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 9);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 10);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 11);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);
      });
  }

  private bool ReadGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.GeneticTestAccount.Populated = false;

    return Read("ReadGeneticTestAccount",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber", entities.GeneticTest.GtaAccountNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.GeneticTestAccount.AccountNumber = db.GetString(reader, 0);
        entities.GeneticTestAccount.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCompany()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.HealthInsuranceCoverage.HicIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 2);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCompanyAddress()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.HealthInsuranceCompanyAddress.Populated = false;

    return Read("ReadHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "hicIdentifier",
          entities.HealthInsuranceCoverage.HicIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCompanyAddress.Province =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompanyAddress.PostalCode =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCompanyAddress.Zip3 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCompanyAddress.Country =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompanyAddress.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", local.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadIncarceration1()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 3);
        entities.Incarceration.InstitutionName =
          db.GetNullableString(reader, 4);
        entities.Incarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 5);
        entities.Incarceration.ParoleOfficerFirstName =
          db.GetNullableString(reader, 6);
        entities.Incarceration.ParoleOfficerMiddleInitial =
          db.GetNullableString(reader, 7);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncarceration2()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 3);
        entities.Incarceration.InstitutionName =
          db.GetNullableString(reader, 4);
        entities.Incarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 5);
        entities.Incarceration.ParoleOfficerFirstName =
          db.GetNullableString(reader, 6);
        entities.Incarceration.ParoleOfficerMiddleInitial =
          db.GetNullableString(reader, 7);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncarcerationAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Incarceration.Populated);
    entities.IncarcerationAddress.Populated = false;

    return Read("ReadIncarcerationAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "incIdentifier", entities.Incarceration.Identifier);
          
        db.SetString(command, "cspNumber", entities.Incarceration.CspNumber);
      },
      (db, reader) =>
      {
        entities.IncarcerationAddress.IncIdentifier = db.GetInt32(reader, 0);
        entities.IncarcerationAddress.CspNumber = db.GetString(reader, 1);
        entities.IncarcerationAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.IncarcerationAddress.Street1 = db.GetNullableString(reader, 3);
        entities.IncarcerationAddress.Street2 = db.GetNullableString(reader, 4);
        entities.IncarcerationAddress.City = db.GetNullableString(reader, 5);
        entities.IncarcerationAddress.State = db.GetNullableString(reader, 6);
        entities.IncarcerationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.IncarcerationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.IncarcerationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.IncarcerationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.IncarcerationAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.IncarcerationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.IncarcerationAddress.Populated = true;
      });
  }

  private bool ReadInformationRequest()
  {
    entities.InformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", local.InformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.Populated = true;
      });
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", local.InterstateRequest.IntHGeneratedId);
        db.
          SetDate(command, "startDate", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 2);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 3);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 4);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 5);
        entities.InterstateContact.Populated = true;
      });
  }

  private bool ReadInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);
    entities.InterstateContactAddress.Populated = false;

    return Read("ReadInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.
          SetDate(command, "startDate", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Type1 =
          db.GetNullableString(reader, 3);
        entities.InterstateContactAddress.Street1 =
          db.GetNullableString(reader, 4);
        entities.InterstateContactAddress.Street2 =
          db.GetNullableString(reader, 5);
        entities.InterstateContactAddress.City =
          db.GetNullableString(reader, 6);
        entities.InterstateContactAddress.EndDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateContactAddress.County =
          db.GetNullableString(reader, 8);
        entities.InterstateContactAddress.State =
          db.GetNullableString(reader, 9);
        entities.InterstateContactAddress.ZipCode =
          db.GetNullableString(reader, 10);
        entities.InterstateContactAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.InterstateContactAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.InterstateContactAddress.Street3 =
          db.GetNullableString(reader, 13);
        entities.InterstateContactAddress.Street4 =
          db.GetNullableString(reader, 14);
        entities.InterstateContactAddress.Province =
          db.GetNullableString(reader, 15);
        entities.InterstateContactAddress.PostalCode =
          db.GetNullableString(reader, 16);
        entities.InterstateContactAddress.Country =
          db.GetNullableString(reader, 17);
        entities.InterstateContactAddress.LocationType =
          db.GetString(reader, 18);
        entities.InterstateContactAddress.Populated = true;
        CheckValid<InterstateContactAddress>("LocationType",
          entities.InterstateContactAddress.LocationType);
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", import.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest10()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest10",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest11()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest11",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", import.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", import.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest4()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest5()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest6()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest6",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
        db.SetNullableString(command, "casNumber", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest7()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest7",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
        db.SetNullableString(command, "casNumber", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest8()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest8",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest9()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest9",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 12);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 15);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLocateRequest()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest",
      (db, command) =>
      {
        db.SetString(
          command, "csePersonNumber", local.LocateRequest.CsePersonNumber);
        db.SetString(command, "agencyNumber", local.LocateRequest.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", local.LocateRequest.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 0);
        entities.LocateRequest.LicenseIssuedDate =
          db.GetNullableDate(reader, 1);
        entities.LocateRequest.LicenseExpirationDate =
          db.GetNullableDate(reader, 2);
        entities.LocateRequest.LicenseSuspendedDate =
          db.GetNullableDate(reader, 3);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 4);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 5);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 6);
        entities.LocateRequest.LicenseSourceName =
          db.GetNullableString(reader, 7);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 8);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 9);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 10);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 11);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 12);
        entities.LocateRequest.City = db.GetNullableString(reader, 13);
        entities.LocateRequest.State = db.GetNullableString(reader, 14);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 15);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 16);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 17);
        entities.LocateRequest.Province = db.GetNullableString(reader, 18);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 19);
        entities.LocateRequest.Country = db.GetNullableString(reader, 20);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 21);
        entities.LocateRequest.LicenseSuspensionInd =
          db.GetNullableString(reader, 22);
        entities.LocateRequest.Populated = true;
      });
  }

  private bool ReadMilitaryService()
  {
    entities.MilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.MilitaryService.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.MilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.MilitaryService.CspNumber = db.GetString(reader, 1);
        entities.MilitaryService.Street1 = db.GetNullableString(reader, 2);
        entities.MilitaryService.Street2 = db.GetNullableString(reader, 3);
        entities.MilitaryService.City = db.GetNullableString(reader, 4);
        entities.MilitaryService.State = db.GetNullableString(reader, 5);
        entities.MilitaryService.Province = db.GetNullableString(reader, 6);
        entities.MilitaryService.PostalCode = db.GetNullableString(reader, 7);
        entities.MilitaryService.ZipCode5 = db.GetNullableString(reader, 8);
        entities.MilitaryService.ZipCode4 = db.GetNullableString(reader, 9);
        entities.MilitaryService.Zip3 = db.GetNullableString(reader, 10);
        entities.MilitaryService.Country = db.GetNullableString(reader, 11);
        entities.MilitaryService.CommandingOfficerLastName =
          db.GetNullableString(reader, 12);
        entities.MilitaryService.CommandingOfficerFirstName =
          db.GetNullableString(reader, 13);
        entities.MilitaryService.CommandingOfficerMi =
          db.GetNullableString(reader, 14);
        entities.MilitaryService.CurrentUsDutyStation =
          db.GetNullableString(reader, 15);
        entities.MilitaryService.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ObligationType.Code = db.GetString(reader, 5);
        entities.ObligationType.Name = db.GetString(reader, 6);
        entities.ObligationType.Classification = db.GetString(reader, 7);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligationObligationTypeDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationObligationTypeDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetInt32(command, "obId", import.SpDocKey.KeyObligation);
        db.SetInt32(command, "debtTypId", import.SpDocKey.KeyObligationType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ObligationType.Code = db.GetString(reader, 5);
        entities.ObligationType.Name = db.GetString(reader, 6);
        entities.ObligationType.Classification = db.GetString(reader, 7);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 8);
        entities.DebtDetail.CspNumber = db.GetString(reader, 9);
        entities.DebtDetail.CpaType = db.GetString(reader, 10);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 12);
        entities.DebtDetail.OtrType = db.GetString(reader, 13);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 14);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 15);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return ReadEach("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "endDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 8);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);

        return true;
      });
  }

  private bool ReadPersonGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.PersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.GeneticTest.TestNumber);
        db.
          SetString(command, "cspNumber", entities.GeneticTest.CspMNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.PersonGeneticTest.GteTestNumber = db.GetInt32(reader, 0);
        entities.PersonGeneticTest.CspNumber = db.GetString(reader, 1);
        entities.PersonGeneticTest.Identifier = db.GetInt32(reader, 2);
        entities.PersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.PersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 4);
        entities.PersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.PersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.PersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.PersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.GeneticTest.TestNumber);
        db.
          SetString(command, "cspNumber", entities.GeneticTest.CspANumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.PersonGeneticTest.GteTestNumber = db.GetInt32(reader, 0);
        entities.PersonGeneticTest.CspNumber = db.GetString(reader, 1);
        entities.PersonGeneticTest.Identifier = db.GetInt32(reader, 2);
        entities.PersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.PersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 4);
        entities.PersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.PersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.PersonGeneticTest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonGeneticTest3()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.PersonGeneticTest.Populated = false;

    return ReadEach("ReadPersonGeneticTest3",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.GeneticTest.TestNumber);
        db.
          SetString(command, "cspNumber", entities.GeneticTest.CspMNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.PersonGeneticTest.GteTestNumber = db.GetInt32(reader, 0);
        entities.PersonGeneticTest.CspNumber = db.GetString(reader, 1);
        entities.PersonGeneticTest.Identifier = db.GetInt32(reader, 2);
        entities.PersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.PersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 4);
        entities.PersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.PersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.PersonGeneticTest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonGeneticTest4()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.PersonGeneticTest.Populated = false;

    return ReadEach("ReadPersonGeneticTest4",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.GeneticTest.TestNumber);
        db.
          SetString(command, "cspNumber", entities.GeneticTest.CspANumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.PersonGeneticTest.GteTestNumber = db.GetInt32(reader, 0);
        entities.PersonGeneticTest.CspNumber = db.GetString(reader, 1);
        entities.PersonGeneticTest.Identifier = db.GetInt32(reader, 2);
        entities.PersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.PersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 4);
        entities.PersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.PersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.PersonGeneticTest.Populated = true;

        return true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
        db.SetString(command, "cspNumber", local.Ch.Number);
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.Populated = true;
      });
  }

  private bool ReadRecaptureRule()
  {
    entities.RecaptureRule.Populated = false;

    return Read("ReadRecaptureRule",
      (db, command) =>
      {
        db.
          SetInt32(command, "recaptureRuleId", import.SpDocKey.KeyRecaptureRule);
          
      },
      (db, reader) =>
      {
        entities.RecaptureRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.RecaptureRule.CpaDType = db.GetNullableString(reader, 1);
        entities.RecaptureRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.RecaptureRule.EffectiveDate = db.GetDate(reader, 3);
        entities.RecaptureRule.NegotiatedDate = db.GetNullableDate(reader, 4);
        entities.RecaptureRule.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.RecaptureRule.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 6);
        entities.RecaptureRule.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.RecaptureRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 8);
        entities.RecaptureRule.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 9);
        entities.RecaptureRule.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.RecaptureRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 11);
        entities.RecaptureRule.PassthruPercentage =
          db.GetNullableInt32(reader, 12);
        entities.RecaptureRule.PassthruAmount =
          db.GetNullableDecimal(reader, 13);
        entities.RecaptureRule.PassthruMaxAmount =
          db.GetNullableDecimal(reader, 14);
        entities.RecaptureRule.Type1 = db.GetString(reader, 15);
        entities.RecaptureRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.RecaptureRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.RecaptureRule.Type1);
      });
  }

  private bool ReadResourceLocationAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonResource.Populated);
    entities.ResourceLocationAddress.Populated = false;

    return Read("ReadResourceLocationAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo", entities.CsePersonResource.ResourceNo);
        db.
          SetString(command, "cspNumber", entities.CsePersonResource.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ResourceLocationAddress.CprResourceNo = db.GetInt32(reader, 0);
        entities.ResourceLocationAddress.CspNumber = db.GetString(reader, 1);
        entities.ResourceLocationAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ResourceLocationAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ResourceLocationAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ResourceLocationAddress.City = db.GetNullableString(reader, 5);
        entities.ResourceLocationAddress.State =
          db.GetNullableString(reader, 6);
        entities.ResourceLocationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ResourceLocationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ResourceLocationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ResourceLocationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ResourceLocationAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ResourceLocationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ResourceLocationAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ResourceLocationAddress.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadVendor1()
  {
    System.Diagnostics.Debug.Assert(entities.PersonGeneticTest.Populated);
    entities.Vendor.Populated = false;

    return Read("ReadVendor1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.PersonGeneticTest.VenIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Vendor.Identifier = db.GetInt32(reader, 0);
        entities.Vendor.Name = db.GetString(reader, 1);
        entities.Vendor.Populated = true;
      });
  }

  private bool ReadVendor2()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Vendor.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.GeneticTest.VenIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Vendor.Identifier = db.GetInt32(reader, 0);
        entities.Vendor.Name = db.GetString(reader, 1);
        entities.Vendor.Populated = true;
      });
  }

  private bool ReadVendorAddress()
  {
    entities.VendorAddress.Populated = false;

    return Read("ReadVendorAddress",
      (db, command) =>
      {
        db.SetInt32(command, "venIdentifier", entities.Vendor.Identifier);
      },
      (db, reader) =>
      {
        entities.VendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.VendorAddress.EffectiveDate = db.GetDate(reader, 1);
        entities.VendorAddress.Street1 = db.GetNullableString(reader, 2);
        entities.VendorAddress.Street2 = db.GetNullableString(reader, 3);
        entities.VendorAddress.City = db.GetNullableString(reader, 4);
        entities.VendorAddress.State = db.GetNullableString(reader, 5);
        entities.VendorAddress.Province = db.GetNullableString(reader, 6);
        entities.VendorAddress.PostalCode = db.GetNullableString(reader, 7);
        entities.VendorAddress.ZipCode5 = db.GetNullableString(reader, 8);
        entities.VendorAddress.ZipCode4 = db.GetNullableString(reader, 9);
        entities.VendorAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.VendorAddress.Country = db.GetNullableString(reader, 11);
        entities.VendorAddress.Populated = true;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of ExpImpRowLockFieldValue.
    /// </summary>
    [JsonPropertyName("expImpRowLockFieldValue")]
    public Common ExpImpRowLockFieldValue
    {
      get => expImpRowLockFieldValue ??= new();
      set => expImpRowLockFieldValue = value;
    }

    private SpDocKey spDocKey;
    private Infrastructure infrastructure;
    private Document document;
    private Field field;
    private FieldValue fieldValue;
    private DateWorkArea current;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    private SpDocKey spDocKey;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A DmvGroup group.</summary>
    [Serializable]
    public class DmvGroup
    {
      /// <summary>
      /// A value of FieldValue.
      /// </summary>
      [JsonPropertyName("fieldValue")]
      public FieldValue FieldValue
      {
        get => fieldValue ??= new();
        set => fieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private FieldValue fieldValue;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Gobligation.
      /// </summary>
      [JsonPropertyName("gobligation")]
      public Obligation Gobligation
      {
        get => gobligation ??= new();
        set => gobligation = value;
      }

      /// <summary>
      /// A value of GobligationType.
      /// </summary>
      [JsonPropertyName("gobligationType")]
      public ObligationType GobligationType
      {
        get => gobligationType ??= new();
        set => gobligationType = value;
      }

      /// <summary>
      /// A value of GdebtDetail.
      /// </summary>
      [JsonPropertyName("gdebtDetail")]
      public DebtDetail GdebtDetail
      {
        get => gdebtDetail ??= new();
        set => gdebtDetail = value;
      }

      /// <summary>
      /// A value of GobligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("gobligationPaymentSchedule")]
      public ObligationPaymentSchedule GobligationPaymentSchedule
      {
        get => gobligationPaymentSchedule ??= new();
        set => gobligationPaymentSchedule = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Obligation gobligation;
      private ObligationType gobligationType;
      private DebtDetail gdebtDetail;
      private ObligationPaymentSchedule gobligationPaymentSchedule;
    }

    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
    {
      /// <summary>
      /// A value of GlocalAddress.
      /// </summary>
      [JsonPropertyName("glocalAddress")]
      public FieldValue GlocalAddress
      {
        get => glocalAddress ??= new();
        set => glocalAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalAddress;
    }

    /// <summary>
    /// Gets a value of Dmv.
    /// </summary>
    [JsonIgnore]
    public Array<DmvGroup> Dmv => dmv ??= new(DmvGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Dmv for json serialization.
    /// </summary>
    [JsonPropertyName("dmv")]
    [Computed]
    public IList<DmvGroup> Dmv_Json
    {
      get => dmv;
      set => Dmv.Assign(value);
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Fv.
    /// </summary>
    [JsonPropertyName("fv")]
    public Common Fv
    {
      get => fv ??= new();
      set => fv = value;
    }

    /// <summary>
    /// A value of PaymentScheduleFound.
    /// </summary>
    [JsonPropertyName("paymentScheduleFound")]
    public Common PaymentScheduleFound
    {
      get => paymentScheduleFound ??= new();
      set => paymentScheduleFound = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of CollectionDateWorkArea.
    /// </summary>
    [JsonPropertyName("collectionDateWorkArea")]
    public DateWorkArea CollectionDateWorkArea
    {
      get => collectionDateWorkArea ??= new();
      set => collectionDateWorkArea = value;
    }

    /// <summary>
    /// A value of CollectionCommon.
    /// </summary>
    [JsonPropertyName("collectionCommon")]
    public Common CollectionCommon
    {
      get => collectionCommon ??= new();
      set => collectionCommon = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

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
    /// A value of ParoleProbation.
    /// </summary>
    [JsonPropertyName("paroleProbation")]
    public Incarceration ParoleProbation
    {
      get => paroleProbation ??= new();
      set => paroleProbation = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    /// <summary>
    /// A value of PersonGeneticTest.
    /// </summary>
    [JsonPropertyName("personGeneticTest")]
    public PersonGeneticTest PersonGeneticTest
    {
      get => personGeneticTest ??= new();
      set => personGeneticTest = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public CaseRole Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of NullSpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("nullSpPrintWorkSet")]
    public SpPrintWorkSet NullSpPrintWorkSet
    {
      get => nullSpPrintWorkSet ??= new();
      set => nullSpPrintWorkSet = value;
    }

    /// <summary>
    /// A value of JailPrison.
    /// </summary>
    [JsonPropertyName("jailPrison")]
    public Incarceration JailPrison
    {
      get => jailPrison ??= new();
      set => jailPrison = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Field Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of ProcessGroup.
    /// </summary>
    [JsonPropertyName("processGroup")]
    public Common ProcessGroup
    {
      get => processGroup ??= new();
      set => processGroup = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Field Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of CurrentField.
    /// </summary>
    [JsonPropertyName("currentField")]
    public Field CurrentField
    {
      get => currentField ??= new();
      set => currentField = value;
    }

    /// <summary>
    /// A value of CurrentRoot.
    /// </summary>
    [JsonPropertyName("currentRoot")]
    public Field CurrentRoot
    {
      get => currentRoot ??= new();
      set => currentRoot = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of CurrentCaseRole.
    /// </summary>
    [JsonPropertyName("currentCaseRole")]
    public CaseRole CurrentCaseRole
    {
      get => currentCaseRole ??= new();
      set => currentCaseRole = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of CurrentNumber.
    /// </summary>
    [JsonPropertyName("currentNumber")]
    public Field CurrentNumber
    {
      get => currentNumber ??= new();
      set => currentNumber = value;
    }

    /// <summary>
    /// A value of Loriginal.
    /// </summary>
    [JsonPropertyName("loriginal")]
    public Collection Loriginal
    {
      get => loriginal ??= new();
      set => loriginal = value;
    }

    /// <summary>
    /// A value of Ltotal.
    /// </summary>
    [JsonPropertyName("ltotal")]
    public Collection Ltotal
    {
      get => ltotal ??= new();
      set => ltotal = value;
    }

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
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private Array<DmvGroup> dmv;
    private LocateRequest locateRequest;
    private LegalAction legalAction;
    private Common fv;
    private Common paymentScheduleFound;
    private DebtDetail debtDetail;
    private RecaptureRule recaptureRule;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
    private DateWorkArea collectionDateWorkArea;
    private Common collectionCommon;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationType obligationType;
    private Obligation obligation;
    private Array<LocalGroup> local1;
    private DateWorkArea max;
    private Incarceration paroleProbation;
    private CsePersonAccount csePersonAccount;
    private AdministrativeActCertification administrativeActCertification;
    private Contact contact;
    private Vendor vendor;
    private PersonGeneticTest personGeneticTest;
    private CaseRole document;
    private GeneticTest geneticTest;
    private MilitaryService militaryService;
    private Tribunal tribunal;
    private SpPrintWorkSet nullSpPrintWorkSet;
    private Incarceration jailPrison;
    private CsePersonResource csePersonResource;
    private InformationRequest informationRequest;
    private DateWorkArea nullDateWorkArea;
    private BatchConvertNumToText batchConvertNumToText;
    private SpPrintWorkSet spPrintWorkSet;
    private CsePerson csePerson;
    private Bankruptcy bankruptcy;
    private Array<AddressGroup> address;
    private Field previous;
    private FieldValue fieldValue;
    private DateWorkArea dateWorkArea;
    private Common processGroup;
    private Field temp;
    private Common position;
    private Field currentField;
    private Field currentRoot;
    private WorkArea workArea;
    private Appointment appointment;
    private CsePerson ch;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CaseRole currentCaseRole;
    private Code code;
    private CodeValue codeValue;
    private Field currentNumber;
    private Collection loriginal;
    private Collection ltotal;
    private Fips fips;
    private InterstateContactAddress interstateContactAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of GeneticTestAccount.
    /// </summary>
    [JsonPropertyName("geneticTestAccount")]
    public GeneticTestAccount GeneticTestAccount
    {
      get => geneticTestAccount ??= new();
      set => geneticTestAccount = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CaseRole Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of PersonGeneticTest.
    /// </summary>
    [JsonPropertyName("personGeneticTest")]
    public PersonGeneticTest PersonGeneticTest
    {
      get => personGeneticTest ??= new();
      set => personGeneticTest = value;
    }

    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    /// <summary>
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ExternalAgencyAddress.
    /// </summary>
    [JsonPropertyName("externalAgencyAddress")]
    public ExternalAgencyAddress ExternalAgencyAddress
    {
      get => externalAgencyAddress ??= new();
      set => externalAgencyAddress = value;
    }

    /// <summary>
    /// A value of ExternalAgency.
    /// </summary>
    [JsonPropertyName("externalAgency")]
    public ExternalAgency ExternalAgency
    {
      get => externalAgency ??= new();
      set => externalAgency = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of IncarcerationAddress.
    /// </summary>
    [JsonPropertyName("incarcerationAddress")]
    public IncarcerationAddress IncarcerationAddress
    {
      get => incarcerationAddress ??= new();
      set => incarcerationAddress = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
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

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
    private LocateRequest locateRequest;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
    private GeneticTestAccount geneticTestAccount;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private LegalAction legalAction;
    private CollectionType collectionType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DebtDetail debtDetail;
    private RecaptureRule recaptureRule;
    private Collection collection;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private Obligation obligation;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CsePersonAccount csePersonAccount;
    private AdministrativeActCertification administrativeActCertification;
    private ContactAddress contactAddress;
    private Contact contact;
    private CaseRole mother;
    private CaseRole absentParent;
    private PersonGeneticTest personGeneticTest;
    private VendorAddress vendorAddress;
    private Vendor vendor;
    private GeneticTest geneticTest;
    private ResourceLocationAddress resourceLocationAddress;
    private ExternalAgencyAddress externalAgencyAddress;
    private ExternalAgency externalAgency;
    private MilitaryService militaryService;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private IncarcerationAddress incarcerationAddress;
    private Incarceration incarceration;
    private CsePersonVehicle csePersonVehicle;
    private CsePersonResource csePersonResource;
    private InformationRequest informationRequest;
    private CsePerson csePerson;
    private Bankruptcy bankruptcy;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private Appointment appointment;
    private Case1 case1;
    private CaseRole caseRole;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
