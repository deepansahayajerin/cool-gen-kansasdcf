// Program: FN_MTRN_MAINTAIN_EFT_TRNSMISSION, ID: 372414469, model: 746.
// Short name: SWEMTRNP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_MTRN_MAINTAIN_EFT_TRNSMISSION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnMtrnMaintainEftTrnsmission: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MTRN_MAINTAIN_EFT_TRNSMISSION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMtrnMaintainEftTrnsmission(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMtrnMaintainEftTrnsmission.
  /// </summary>
  public FnMtrnMaintainEftTrnsmission(IContext context, Import import,
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
    // *********************************************************************************
    // DATE		WHO		DESCRIPTION
    // 01/15/99	M Fangman    	Rewritten
    // 10/01/99  M Fangman             Changed for PR 75329 - On returning from 
    // EFTL with an EFT transmission record in a status of DOA fields should be
    // unprotected.
    // 01/04/00  M Fangman             PR 82289 Bypass the edit checks on the 
    // addendum record when the source is FDSO or the courts.
    // *********************************************************************************
    // If delete access can be obtained delete ALL occurrances of 
    // DFI_ACCOUNT_NUMBER
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        if (AsChar(import.ElectronicFundTransmission.TransmissionType) == 'O')
        {
          ExitState = "ECO_XFR_TO_DISB_MGMNT_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MGMNT_MENU";
        }

        return;
      default:
        break;
    }

    // Move all IMPORTs to EXPORTs.
    export.CsePerson.Number = import.CsePerson.Number;
    export.Type1.Text8 = import.Type1.Text8;
    export.ElectronicFundTransmission.Assign(import.ElectronicFundTransmission);
    export.PaymentRequest.Assign(import.PaymentRequest);
    export.Standard.NextTransaction = import.ApplicationId.NextTransaction;
    export.HiddenElectronicFundTransmission.Assign(
      import.HiddenElectronicFundTransmission);
    export.NextRead.TransmissionStatusCode =
      import.NextRead.TransmissionStatusCode;
    ExitState = "ACO_NN0000_ALL_OK";

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.ApplicationId.NextTransaction))
    {
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(export.ElectronicFundTransmission.TransmissionStatusCode,
      "RECEIPTED") || Equal
      (export.ElectronicFundTransmission.TransmissionStatusCode, "TESTED") || Equal
      (export.ElectronicFundTransmission.TransmissionStatusCode, "SENT") || Equal
      (export.ElectronicFundTransmission.TransmissionStatusCode, "CHWAR") || Equal
      (export.ElectronicFundTransmission.TransmissionStatusCode, "DOA") || export
      .ElectronicFundTransmission.TransmittalAmount == 0)
    {
      // Lock ALL fields.
      var field1 = GetField(export.ElectronicFundTransmission, "companyName");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 =
        GetField(export.ElectronicFundTransmission, "companyIdentificationIcd");
        

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 =
        GetField(export.ElectronicFundTransmission,
        "companyIdentificationNumber");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 =
        GetField(export.ElectronicFundTransmission, "companyEntryDescription");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.CoEntryDesc, "promptField");

      field5.Color = "cyan";
      field5.Highlighting = Highlighting.Normal;
      field5.Protected = true;

      var field6 =
        GetField(export.ElectronicFundTransmission, "companyDescriptiveDate");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 =
        GetField(export.ElectronicFundTransmission, "effectiveEntryDate");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 =
        GetField(export.ElectronicFundTransmission,
        "originatingDfiIdentification");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 =
        GetField(export.ElectronicFundTransmission, "transactionCode");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 =
        GetField(export.ElectronicFundTransmission, "receivingDfiAccountNumber");
        

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 =
        GetField(export.ElectronicFundTransmission, "receivingCompanyName");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.ElectronicFundTransmission, "traceNumber");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 =
        GetField(export.ElectronicFundTransmission, "applicationIdentifier");

      field13.Color = "cyan";
      field13.Protected = true;

      var field14 = GetField(export.ApplicationId, "promptField");

      field14.Color = "cyan";
      field14.Highlighting = Highlighting.Normal;
      field14.Protected = true;

      var field15 = GetField(export.ElectronicFundTransmission, "caseId");

      field15.Color = "cyan";
      field15.Protected = true;

      var field16 = GetField(export.ElectronicFundTransmission, "payDate");

      field16.Color = "cyan";
      field16.Protected = true;

      var field17 =
        GetField(export.ElectronicFundTransmission, "collectionAmount");

      field17.Color = "cyan";
      field17.Protected = true;

      var field18 = GetField(export.ElectronicFundTransmission, "apSsn");

      field18.Color = "cyan";
      field18.Protected = true;

      var field19 =
        GetField(export.ElectronicFundTransmission, "medicalSupportId");

      field19.Color = "cyan";
      field19.Protected = true;

      var field20 = GetField(export.ElectronicFundTransmission, "apName");

      field20.Color = "cyan";
      field20.Protected = true;

      var field21 = GetField(export.ElectronicFundTransmission, "fipsCode");

      field21.Color = "cyan";
      field21.Protected = true;

      var field22 =
        GetField(export.ElectronicFundTransmission, "employmentTerminationId");

      field22.Color = "cyan";
      field22.Protected = true;

      var field23 =
        GetField(export.ElectronicFundTransmission, "receivingDfiIdentification");
        

      field23.Color = "cyan";
      field23.Protected = true;

      var field24 = GetField(export.ElectronicFundTransmission, "checkDigit");

      field24.Color = "cyan";
      field24.Protected = true;
    }
    else
    {
      // Unlock all fields.
      var field1 = GetField(export.ElectronicFundTransmission, "companyName");

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;
      field1.Focused = true;

      var field2 =
        GetField(export.ElectronicFundTransmission, "companyIdentificationIcd");
        

      field2.Color = "green";
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;
      field2.Focused = false;

      var field3 =
        GetField(export.ElectronicFundTransmission,
        "companyIdentificationNumber");

      field3.Color = "green";
      field3.Highlighting = Highlighting.Underscore;
      field3.Protected = false;

      var field4 =
        GetField(export.ElectronicFundTransmission, "companyEntryDescription");

      field4.Color = "green";
      field4.Highlighting = Highlighting.Underscore;
      field4.Protected = false;

      var field5 =
        GetField(export.ElectronicFundTransmission, "companyDescriptiveDate");

      field5.Color = "green";
      field5.Highlighting = Highlighting.Underscore;
      field5.Protected = false;

      var field6 =
        GetField(export.ElectronicFundTransmission, "effectiveEntryDate");

      field6.Color = "green";
      field6.Highlighting = Highlighting.Underscore;
      field6.Protected = false;

      var field7 =
        GetField(export.ElectronicFundTransmission,
        "originatingDfiIdentification");

      field7.Color = "green";
      field7.Highlighting = Highlighting.Underscore;
      field7.Protected = false;

      var field8 =
        GetField(export.ElectronicFundTransmission, "transactionCode");

      field8.Color = "green";
      field8.Highlighting = Highlighting.Underscore;
      field8.Protected = false;

      var field9 =
        GetField(export.ElectronicFundTransmission, "receivingDfiIdentification");
        

      field9.Color = "green";
      field9.Highlighting = Highlighting.Underscore;
      field9.Protected = false;

      var field10 = GetField(export.ElectronicFundTransmission, "checkDigit");

      field10.Color = "green";
      field10.Highlighting = Highlighting.Underscore;
      field10.Protected = false;

      var field11 =
        GetField(export.ElectronicFundTransmission, "receivingDfiAccountNumber");
        

      field11.Color = "green";
      field11.Highlighting = Highlighting.Underscore;
      field11.Protected = false;

      var field12 =
        GetField(export.ElectronicFundTransmission, "receivingCompanyName");

      field12.Color = "green";
      field12.Highlighting = Highlighting.Underscore;
      field12.Protected = false;

      var field13 = GetField(export.ElectronicFundTransmission, "traceNumber");

      field13.Color = "green";
      field13.Highlighting = Highlighting.Underscore;
      field13.Protected = false;

      var field14 =
        GetField(export.ElectronicFundTransmission, "applicationIdentifier");

      field14.Color = "green";
      field14.Highlighting = Highlighting.Underscore;
      field14.Protected = false;

      var field15 = GetField(export.ElectronicFundTransmission, "caseId");

      field15.Color = "green";
      field15.Protected = false;

      var field16 = GetField(export.ElectronicFundTransmission, "payDate");

      field16.Color = "green";
      field16.Highlighting = Highlighting.Underscore;
      field16.Protected = false;

      var field17 =
        GetField(export.ElectronicFundTransmission, "collectionAmount");

      field17.Color = "green";
      field17.Highlighting = Highlighting.Underscore;
      field17.Protected = false;

      var field18 = GetField(export.ElectronicFundTransmission, "apSsn");

      field18.Color = "green";
      field18.Highlighting = Highlighting.Underscore;
      field18.Protected = false;

      var field19 =
        GetField(export.ElectronicFundTransmission, "medicalSupportId");

      field19.Color = "green";
      field19.Highlighting = Highlighting.Underscore;
      field19.Protected = false;

      var field20 = GetField(export.ElectronicFundTransmission, "apName");

      field20.Color = "green";
      field20.Highlighting = Highlighting.Underscore;
      field20.Protected = false;

      var field21 = GetField(export.ElectronicFundTransmission, "fipsCode");

      field21.Color = "green";
      field21.Highlighting = Highlighting.Underscore;
      field21.Protected = false;

      var field22 =
        GetField(export.ElectronicFundTransmission, "employmentTerminationId");

      field22.Color = "green";
      field22.Highlighting = Highlighting.Underscore;
      field22.Protected = false;

      var field23 = GetField(export.ApplicationId, "promptField");

      field23.Color = "green";
      field23.Highlighting = Highlighting.Underscore;
      field23.Protected = false;

      var field24 = GetField(export.CoEntryDesc, "promptField");

      field24.Color = "green";
      field24.Highlighting = Highlighting.Underscore;
      field24.Protected = false;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.ElectronicFundTransmission.TransmissionType = "I";
      export.NextRead.TransmissionStatusCode = "PENDED";
      ExitState = "FN0000_SELECT_EFT_FROM_LTRN";

      return;
    }

    if (Equal(global.Command, "FROMCRSL"))
    {
      if (!IsEmpty(import.CashReceiptSourceType.Code))
      {
        UseFnConvertCrstcToCoEntDescr();
      }

      export.CoEntryDesc.PromptField = "";

      var field =
        GetField(export.ElectronicFundTransmission, "companyEntryDescription");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;

      global.Command = "UPDATE";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(import.Selected.Cdvalue))
      {
        export.ElectronicFundTransmission.ApplicationIdentifier =
          import.Selected.Cdvalue;
      }

      export.ApplicationId.PromptField = "";
      global.Command = "UPDATE";
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "NEXT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ***********************************************************************
        // Flow from list screen causes a display.
        // ***********************************************************************
        if (export.ElectronicFundTransmission.TransmissionIdentifier > 0)
        {
          if (ReadElectronicFundTransmission3())
          {
            export.ElectronicFundTransmission.Assign(
              entities.ElectronicFundTransmission);
            MoveElectronicFundTransmission(export.ElectronicFundTransmission,
              export.HiddenElectronicFundTransmission);

            if (AsChar(entities.ElectronicFundTransmission.TransmissionType) ==
              'O')
            {
              export.Type1.Text8 = "OUTBOUND";

              if (Equal(entities.ElectronicFundTransmission.
                TransmissionStatusCode, "RELEASED"))
              {
                // For Outbound - unlock all fields.
                var field1 =
                  GetField(export.ElectronicFundTransmission, "companyName");

                field1.Color = "green";
                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;
                field1.Focused = true;

                var field2 =
                  GetField(export.ElectronicFundTransmission,
                  "companyIdentificationIcd");

                field2.Color = "green";
                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;

                var field3 =
                  GetField(export.ElectronicFundTransmission,
                  "companyIdentificationNumber");

                field3.Color = "green";
                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                var field4 =
                  GetField(export.ElectronicFundTransmission,
                  "companyEntryDescription");

                field4.Color = "green";
                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                var field5 =
                  GetField(export.ElectronicFundTransmission,
                  "companyDescriptiveDate");

                field5.Color = "green";
                field5.Highlighting = Highlighting.Underscore;
                field5.Protected = false;

                var field6 =
                  GetField(export.ElectronicFundTransmission,
                  "effectiveEntryDate");

                field6.Color = "green";
                field6.Highlighting = Highlighting.Underscore;
                field6.Protected = false;

                var field7 =
                  GetField(export.ElectronicFundTransmission,
                  "originatingDfiIdentification");

                field7.Color = "green";
                field7.Highlighting = Highlighting.Underscore;
                field7.Protected = false;

                var field8 =
                  GetField(export.ElectronicFundTransmission, "transactionCode");
                  

                field8.Color = "green";
                field8.Highlighting = Highlighting.Underscore;
                field8.Protected = false;

                var field9 =
                  GetField(export.ElectronicFundTransmission,
                  "receivingDfiIdentification");

                field9.Color = "green";
                field9.Highlighting = Highlighting.Underscore;
                field9.Protected = false;

                var field10 =
                  GetField(export.ElectronicFundTransmission, "checkDigit");

                field10.Color = "green";
                field10.Highlighting = Highlighting.Underscore;
                field10.Protected = false;

                var field11 =
                  GetField(export.ElectronicFundTransmission,
                  "receivingDfiAccountNumber");

                field11.Color = "green";
                field11.Highlighting = Highlighting.Underscore;
                field11.Protected = false;

                var field12 =
                  GetField(export.ElectronicFundTransmission,
                  "receivingCompanyName");

                field12.Color = "green";
                field12.Highlighting = Highlighting.Underscore;
                field12.Protected = false;

                var field13 =
                  GetField(export.ElectronicFundTransmission, "traceNumber");

                field13.Color = "green";
                field13.Highlighting = Highlighting.Underscore;
                field13.Protected = false;

                var field14 =
                  GetField(export.ElectronicFundTransmission,
                  "applicationIdentifier");

                field14.Color = "green";
                field14.Highlighting = Highlighting.Underscore;
                field14.Protected = false;

                var field15 =
                  GetField(export.ElectronicFundTransmission, "caseId");

                field15.Color = "green";
                field15.Protected = false;

                var field16 =
                  GetField(export.ElectronicFundTransmission, "payDate");

                field16.Color = "green";
                field16.Highlighting = Highlighting.Underscore;
                field16.Protected = false;

                var field17 =
                  GetField(export.ElectronicFundTransmission, "collectionAmount");
                  

                field17.Color = "green";
                field17.Highlighting = Highlighting.Underscore;
                field17.Protected = false;

                var field18 =
                  GetField(export.ElectronicFundTransmission, "apSsn");

                field18.Color = "green";
                field18.Highlighting = Highlighting.Underscore;
                field18.Protected = false;

                var field19 =
                  GetField(export.ElectronicFundTransmission, "medicalSupportId");
                  

                field19.Color = "green";
                field19.Highlighting = Highlighting.Underscore;
                field19.Protected = false;

                var field20 =
                  GetField(export.ElectronicFundTransmission, "apName");

                field20.Color = "green";
                field20.Highlighting = Highlighting.Underscore;
                field20.Protected = false;

                var field21 =
                  GetField(export.ElectronicFundTransmission, "fipsCode");

                field21.Color = "green";
                field21.Highlighting = Highlighting.Underscore;
                field21.Protected = false;

                var field22 =
                  GetField(export.ElectronicFundTransmission,
                  "employmentTerminationId");

                field22.Color = "green";
                field22.Highlighting = Highlighting.Underscore;
                field22.Protected = false;

                var field23 = GetField(export.ApplicationId, "promptField");

                field23.Color = "green";
                field23.Highlighting = Highlighting.Underscore;
                field23.Protected = false;

                var field24 = GetField(export.CoEntryDesc, "promptField");

                field24.Color = "green";
                field24.Highlighting = Highlighting.Underscore;
                field24.Protected = false;
              }
              else
              {
                // If the outbound EFT is not in a released status then it must 
                // be in a DOA or SENT status.
                // The fields should already be protected as needed.
              }

              if (ReadPaymentRequest())
              {
                export.PaymentRequest.Assign(entities.PaymentRequest);

                if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
                  
                {
                  export.PaymentRequest.CsePersonNumber =
                    entities.PaymentRequest.DesignatedPayeeCsePersonNo;
                }

                ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
              }
              else
              {
                ExitState = "FN0000_EFT_WITHOUT_A_PAYMENT_REQ";
              }
            }
            else
            {
              export.Type1.Text8 = "INBOUND";

              if (Equal(entities.ElectronicFundTransmission.
                TransmissionStatusCode, "PENDED") || Equal
                (entities.ElectronicFundTransmission.TransmissionStatusCode,
                "RELEASED"))
              {
                // Unlock all fields.
                var field1 =
                  GetField(export.ElectronicFundTransmission,
                  "companyIdentificationIcd");

                field1.Color = "green";
                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;

                var field2 =
                  GetField(export.ElectronicFundTransmission,
                  "companyIdentificationNumber");

                field2.Color = "green";
                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;

                var field3 =
                  GetField(export.ElectronicFundTransmission,
                  "companyEntryDescription");

                field3.Color = "green";
                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                var field4 =
                  GetField(export.ElectronicFundTransmission,
                  "companyDescriptiveDate");

                field4.Color = "green";
                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                var field5 =
                  GetField(export.ElectronicFundTransmission,
                  "effectiveEntryDate");

                field5.Color = "green";
                field5.Highlighting = Highlighting.Underscore;
                field5.Protected = false;

                var field6 =
                  GetField(export.ElectronicFundTransmission,
                  "originatingDfiIdentification");

                field6.Color = "green";
                field6.Highlighting = Highlighting.Underscore;
                field6.Protected = false;

                var field7 =
                  GetField(export.ElectronicFundTransmission, "transactionCode");
                  

                field7.Color = "green";
                field7.Highlighting = Highlighting.Underscore;
                field7.Protected = false;

                var field8 =
                  GetField(export.ElectronicFundTransmission,
                  "receivingDfiIdentification");

                field8.Color = "green";
                field8.Highlighting = Highlighting.Underscore;
                field8.Protected = false;

                var field9 =
                  GetField(export.ElectronicFundTransmission, "checkDigit");

                field9.Color = "green";
                field9.Highlighting = Highlighting.Underscore;
                field9.Protected = false;

                var field10 =
                  GetField(export.ElectronicFundTransmission,
                  "receivingDfiAccountNumber");

                field10.Color = "green";
                field10.Highlighting = Highlighting.Underscore;
                field10.Protected = false;

                var field11 =
                  GetField(export.ElectronicFundTransmission,
                  "receivingCompanyName");

                field11.Color = "green";
                field11.Highlighting = Highlighting.Underscore;
                field11.Protected = false;

                var field12 =
                  GetField(export.ElectronicFundTransmission, "traceNumber");

                field12.Color = "green";
                field12.Highlighting = Highlighting.Underscore;
                field12.Protected = false;

                var field13 =
                  GetField(export.ElectronicFundTransmission,
                  "applicationIdentifier");

                field13.Color = "green";
                field13.Highlighting = Highlighting.Underscore;
                field13.Protected = false;

                var field14 =
                  GetField(export.ElectronicFundTransmission, "caseId");

                field14.Color = "green";
                field14.Protected = false;

                var field15 =
                  GetField(export.ElectronicFundTransmission, "payDate");

                field15.Color = "green";
                field15.Highlighting = Highlighting.Underscore;
                field15.Protected = false;

                var field16 =
                  GetField(export.ElectronicFundTransmission, "collectionAmount");
                  

                field16.Color = "green";
                field16.Highlighting = Highlighting.Underscore;
                field16.Protected = false;

                var field17 =
                  GetField(export.ElectronicFundTransmission, "apSsn");

                field17.Color = "green";
                field17.Highlighting = Highlighting.Underscore;
                field17.Protected = false;

                var field18 =
                  GetField(export.ElectronicFundTransmission, "medicalSupportId");
                  

                field18.Color = "green";
                field18.Highlighting = Highlighting.Underscore;
                field18.Protected = false;

                var field19 =
                  GetField(export.ElectronicFundTransmission, "apName");

                field19.Color = "green";
                field19.Highlighting = Highlighting.Underscore;
                field19.Protected = false;

                var field20 =
                  GetField(export.ElectronicFundTransmission, "fipsCode");

                field20.Color = "green";
                field20.Highlighting = Highlighting.Underscore;
                field20.Protected = false;

                var field21 =
                  GetField(export.ElectronicFundTransmission,
                  "employmentTerminationId");

                field21.Color = "green";
                field21.Highlighting = Highlighting.Underscore;
                field21.Protected = false;

                var field22 = GetField(export.ApplicationId, "promptField");

                field22.Color = "green";
                field22.Highlighting = Highlighting.Underscore;
                field22.Protected = false;

                var field23 = GetField(export.CoEntryDesc, "promptField");

                field23.Color = "green";
                field23.Highlighting = Highlighting.Underscore;
                field23.Protected = false;
              }

              if (Equal(entities.ElectronicFundTransmission.
                TransmissionStatusCode, "PENDED"))
              {
                export.HiddenElectronicFundTransmission.
                  CompanyEntryDescription = "";

                var field1 =
                  GetField(export.ElectronicFundTransmission, "companyName");

                field1.Color = "green";
                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;
                field1.Focused = false;

                var field2 =
                  GetField(export.ElectronicFundTransmission,
                  "companyEntryDescription");

                field2.Error = true;

                ExitState = "FN0000_CO_ENTRY_DESC_NF";
              }
              else if (Equal(entities.ElectronicFundTransmission.
                TransmissionStatusCode, "RELEASED"))
              {
                var field1 =
                  GetField(export.ElectronicFundTransmission, "companyName");

                field1.Color = "green";
                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;
                field1.Focused = true;

                var field2 =
                  GetField(export.ElectronicFundTransmission,
                  "companyEntryDescription");

                field2.Color = "green";
                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;

                ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
              }
              else
              {
                ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
              }
            }
          }
          else
          {
            export.ElectronicFundTransmission.Assign(
              local.InitializedElectronicFundTransmission);
            export.Type1.Text8 = "";
            ExitState = "FN0000_ELEC_FUND_TRAN_NF";
          }
        }
        else
        {
          ExitState = "FN0000_SELECTION_FROM_LTRN_REQ";
        }

        break;
      case "NEXT":
        // Next is only valid for Pended records per Tim 2/4/99
        if (AsChar(export.ElectronicFundTransmission.TransmissionType) == 'I'
          && Equal(export.NextRead.TransmissionStatusCode, "PENDED"))
        {
          if (ReadElectronicFundTransmission1())
          {
            export.ElectronicFundTransmission.Assign(
              entities.ElectronicFundTransmission);
            MoveElectronicFundTransmission(export.ElectronicFundTransmission,
              export.HiddenElectronicFundTransmission);
            export.HiddenElectronicFundTransmission.CompanyEntryDescription =
              "";
            export.Type1.Text8 = "INBOUND";

            var field =
              GetField(export.ElectronicFundTransmission,
              "companyEntryDescription");

            field.Error = true;

            ExitState = "FN0000_CO_ENTRY_DESC_NF";

            return;
          }

          if (AsChar(import.Pf15Nf.Flag) == 'Y')
          {
            export.Type1.Text8 = "";
            export.ElectronicFundTransmission.Assign(
              local.InitializedElectronicFundTransmission);

            var field1 =
              GetField(export.ElectronicFundTransmission,
              "companyEntryDescription");

            field1.Color = "cyan";
            field1.Highlighting = Highlighting.Normal;
            field1.Protected = true;

            var field2 = GetField(export.CoEntryDesc, "promptField");

            field2.Color = "cyan";
            field2.Highlighting = Highlighting.Normal;
            field2.Protected = true;

            var field3 =
              GetField(export.ElectronicFundTransmission,
              "companyDescriptiveDate");

            field3.Color = "cyan";
            field3.Highlighting = Highlighting.Normal;
            field3.Protected = true;

            var field4 =
              GetField(export.ElectronicFundTransmission,
              "applicationIdentifier");

            field4.Color = "cyan";
            field4.Highlighting = Highlighting.Normal;
            field4.Protected = true;
            field4.Focused = false;

            var field5 = GetField(export.ApplicationId, "promptField");

            field5.Color = "cyan";
            field5.Highlighting = Highlighting.Normal;
            field5.Protected = true;

            var field6 =
              GetField(export.ElectronicFundTransmission, "collectionAmount");

            field6.Color = "cyan";
            field6.Highlighting = Highlighting.Normal;
            field6.Protected = true;

            ExitState = "FN0000_NEXT_EFT_TRAN_REC_NF";
          }
          else
          {
            export.Pf15Nf.Flag = "Y";
            ExitState = "FN0000_NEXT_EFT_TRAN_REC_NF";
          }
        }
        else
        {
          ExitState = "FN0000_NEXT_FOR_PENDED_ONLY";
        }

        break;
      case "UPDATE":
        // ***********************************************************************
        // Must display first before maintenance can be performed.
        // ***********************************************************************
        if (IsEmpty(export.HiddenElectronicFundTransmission.
          TransmissionStatusCode))
        {
          export.ElectronicFundTransmission.Assign(
            local.InitializedElectronicFundTransmission);
          export.Type1.Text8 = "";
          ExitState = "FN0000_SELECTION_FROM_LTRN_REQ";

          return;
        }

        // ***********************************************************************
        // Validate Transaction Status Code Life Cycle.
        // ***********************************************************************
        switch(AsChar(export.ElectronicFundTransmission.TransmissionType))
        {
          case 'I':
            switch(TrimEnd(import.ElectronicFundTransmission.
              TransmissionStatusCode))
            {
              case "RECEIPTED":
                ExitState = "FN0000_STATUS_CANT_BE_UPDATED";

                return;
              case "RELEASED":
                local.ForUpdate.TransmissionStatusCode = "RELEASED";

                break;
              case "PENDED":
                local.ForUpdate.TransmissionStatusCode = "RELEASED";

                break;
              case "TESTED":
                ExitState = "FN0000_STATUS_CANT_BE_UPDATED";

                return;
              default:
                break;
            }

            break;
          case 'O':
            switch(TrimEnd(import.ElectronicFundTransmission.
              TransmissionStatusCode))
            {
              case "CANCELED":
                ExitState = "FN0000_STATUS_CANT_BE_UPDATED";

                return;
              case "SENT":
                ExitState = "FN0000_STATUS_CANT_BE_UPDATED";

                return;
              default:
                local.ForUpdate.TransmissionStatusCode = "RELEASED";

                break;
            }

            break;
          default:
            break;
        }

        // ***********************************************************************
        // Must change some field before maintenance can be performed.
        // ***********************************************************************
        if (Equal(export.ElectronicFundTransmission.CompanyName,
          import.HiddenElectronicFundTransmission.CompanyName) && AsChar
          (export.ElectronicFundTransmission.CompanyIdentificationIcd) == AsChar
          (import.HiddenElectronicFundTransmission.CompanyIdentificationIcd) &&
          Equal
          (export.ElectronicFundTransmission.CompanyIdentificationNumber,
          import.HiddenElectronicFundTransmission.
            CompanyIdentificationNumber) && Equal
          (export.ElectronicFundTransmission.CompanyEntryDescription,
          import.HiddenElectronicFundTransmission.CompanyEntryDescription) && Equal
          (export.ElectronicFundTransmission.CompanyDescriptiveDate,
          import.HiddenElectronicFundTransmission.CompanyDescriptiveDate) && Equal
          (export.ElectronicFundTransmission.EffectiveEntryDate,
          import.HiddenElectronicFundTransmission.EffectiveEntryDate) && export
          .ElectronicFundTransmission.OriginatingDfiIdentification.
            GetValueOrDefault() == import
          .HiddenElectronicFundTransmission.OriginatingDfiIdentification.
            GetValueOrDefault() && Equal
          (export.ElectronicFundTransmission.TransactionCode,
          import.HiddenElectronicFundTransmission.TransactionCode) && Equal
          (export.ElectronicFundTransmission.ReceivingDfiAccountNumber,
          import.HiddenElectronicFundTransmission.ReceivingDfiAccountNumber) &&
          Equal
          (export.ElectronicFundTransmission.ReceivingCompanyName,
          import.HiddenElectronicFundTransmission.ReceivingCompanyName) && export
          .ElectronicFundTransmission.TraceNumber.GetValueOrDefault() == import
          .HiddenElectronicFundTransmission.TraceNumber.GetValueOrDefault() && Equal
          (export.ElectronicFundTransmission.ApplicationIdentifier,
          import.HiddenElectronicFundTransmission.ApplicationIdentifier) && Equal
          (export.ElectronicFundTransmission.CaseId,
          import.HiddenElectronicFundTransmission.CaseId) && Equal
          (export.ElectronicFundTransmission.PayDate,
          import.HiddenElectronicFundTransmission.PayDate) && export
          .ElectronicFundTransmission.CollectionAmount.GetValueOrDefault() == import
          .HiddenElectronicFundTransmission.CollectionAmount.
            GetValueOrDefault() && export.ElectronicFundTransmission.ApSsn == import
          .HiddenElectronicFundTransmission.ApSsn && AsChar
          (export.ElectronicFundTransmission.MedicalSupportId) == AsChar
          (import.HiddenElectronicFundTransmission.MedicalSupportId) && Equal
          (export.ElectronicFundTransmission.ApName,
          import.HiddenElectronicFundTransmission.ApName) && export
          .ElectronicFundTransmission.FipsCode.GetValueOrDefault() == import
          .HiddenElectronicFundTransmission.FipsCode.GetValueOrDefault() && AsChar
          (export.ElectronicFundTransmission.EmploymentTerminationId) == AsChar
          (import.HiddenElectronicFundTransmission.EmploymentTerminationId) && export
          .ElectronicFundTransmission.ReceivingDfiIdentification.
            GetValueOrDefault() == import
          .HiddenElectronicFundTransmission.ReceivingDfiIdentification.
            GetValueOrDefault() && export
          .ElectronicFundTransmission.CheckDigit.GetValueOrDefault() == import
          .HiddenElectronicFundTransmission.CheckDigit.GetValueOrDefault() && Equal
          (export.ElectronicFundTransmission.VendorNumber,
          import.HiddenElectronicFundTransmission.VendorNumber))
        {
          if (AsChar(export.ElectronicFundTransmission.TransmissionType) == 'I')
          {
            var field =
              GetField(export.ElectronicFundTransmission,
              "applicationIdentifier");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field =
              GetField(export.ElectronicFundTransmission, "companyName");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }

          ExitState = "SP0000_DATA_NOT_CHANGED";

          return;
        }

        // ***********************************************************************
        // Edit the Company Description Date.
        // ***********************************************************************
        if (!Lt(export.ElectronicFundTransmission.CompanyDescriptiveDate,
          Now().Date))
        {
          var field =
            GetField(export.ElectronicFundTransmission, "companyDescriptiveDate");
            

          field.Error = true;

          ExitState = "FN0000_DATE_MUST_BE_A_PAST_DATE";
        }

        // ***********************************************************************
        // Edit the Company Entry Description.
        // ***********************************************************************
        UseFnConvertCoEntDescrToCrstc();

        if (AsChar(local.ValidCoEntryDesc.Flag) == 'N')
        {
          var field =
            GetField(export.ElectronicFundTransmission,
            "companyEntryDescription");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }

        if (AsChar(local.SourceUsesAddendum.Flag) == 'Y')
        {
          // ***********************************************************************
          // Edit the Application ID.
          // ***********************************************************************
          local.ValidateCode.CodeName = "EFT APPLICATION ID";
          local.ValidateCodeValue.Cdvalue =
            export.ElectronicFundTransmission.ApplicationIdentifier ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field =
              GetField(export.ElectronicFundTransmission,
              "applicationIdentifier");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }

          // ***********************************************************************
          // Edit the Collection Amt.
          // ***********************************************************************
          if (Equal(export.ElectronicFundTransmission.ApplicationIdentifier,
            "CS") || Equal
            (export.ElectronicFundTransmission.ApplicationIdentifier, "II") || Equal
            (export.ElectronicFundTransmission.ApplicationIdentifier, "IT") || Equal
            (export.ElectronicFundTransmission.ApplicationIdentifier, "IO"))
          {
            if (export.ElectronicFundTransmission.CollectionAmount.
              GetValueOrDefault() != export
              .ElectronicFundTransmission.TransmittalAmount)
            {
              var field =
                GetField(export.ElectronicFundTransmission, "collectionAmount");
                

              field.Error = true;

              ExitState = "FN0000_COLL_AMT_N_AP_ID_INVALID";
            }
          }
          else if (Equal(export.ElectronicFundTransmission.
            ApplicationIdentifier, "RI") || Equal
            (export.ElectronicFundTransmission.ApplicationIdentifier, "RT") || Equal
            (export.ElectronicFundTransmission.ApplicationIdentifier, "RO"))
          {
            if (export.ElectronicFundTransmission.CollectionAmount.
              GetValueOrDefault() <= export
              .ElectronicFundTransmission.TransmittalAmount)
            {
              var field =
                GetField(export.ElectronicFundTransmission, "collectionAmount");
                

              field.Error = true;

              ExitState = "FN0000_COLL_AMT_N_AP_ID_INVALID";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ***********************************************************************
        // Perform update.
        // ***********************************************************************
        if (!ReadElectronicFundTransmission2())
        {
          ExitState = "FN0000_ELEC_FUND_TRAN_NF";

          return;
        }

        try
        {
          UpdateElectronicFundTransmission();
          ExitState = "FN0000_EFT_UPDATE_SUCCESSFUL";
          export.ElectronicFundTransmission.Assign(
            entities.ElectronicFundTransmission);
          MoveElectronicFundTransmission(export.ElectronicFundTransmission,
            export.HiddenElectronicFundTransmission);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_EFT_TRANSMISSION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_EFT_TRANSMISSION_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "LIST":
        if (export.ElectronicFundTransmission.TransmissionIdentifier > 0)
        {
          if (!IsEmpty(import.ApplicationId.PromptField) && !
            IsEmpty(import.CoEntryDesc.PromptField))
          {
            var field1 = GetField(export.CoEntryDesc, "promptField");

            field1.Error = true;

            var field2 = GetField(export.ApplicationId, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }
          else if (!IsEmpty(import.ApplicationId.PromptField))
          {
            if (AsChar(import.ApplicationId.PromptField) == 'S')
            {
              export.ToList.CodeName = "EFT APPLICATION ID";
              ExitState = "ECO_LNK_TO_CDVL";
            }
            else
            {
              var field1 = GetField(export.ApplicationId, "promptField");

              field1.Error = true;

              var field2 = GetField(export.CoEntryDesc, "promptField");

              field2.Color = "green";
              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            }
          }
          else if (!IsEmpty(import.CoEntryDesc.PromptField))
          {
            if (AsChar(import.CoEntryDesc.PromptField) == 'S')
            {
              ExitState = "ECO_LNK_TO_CRSL";
            }
            else
            {
              var field1 = GetField(export.CoEntryDesc, "promptField");

              field1.Error = true;

              var field2 = GetField(export.ApplicationId, "promptField");

              field2.Color = "green";
              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            }
          }
          else
          {
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
          }
        }
        else
        {
          export.ElectronicFundTransmission.Assign(
            local.InitializedElectronicFundTransmission);
          export.Type1.Text8 = "";
          ExitState = "FN0000_SELECTION_FROM_LTRN_REQ";
        }

        break;
      case "CREC":
        if (AsChar(export.ElectronicFundTransmission.TransmissionType) == 'I')
        {
          local.Eft.SystemGeneratedIdentifier = 6;
          local.CashReceipt.CheckNumber =
            NumberToString(import.ElectronicFundTransmission.
              TransmissionIdentifier, 4, 12);

          if (ReadCashReceipt())
          {
            export.ForLink.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
            ExitState = "ECO_LNK_TO_CASH_RECEIPTING";
          }
          else
          {
            ExitState = "FN0000_CASH_RECEIPT_NF";
          }
        }
        else
        {
          ExitState = "FN0000_INBOUND_EFT_REQUIRED";
        }

        break;
      case "CRRC":
        if (AsChar(export.ElectronicFundTransmission.TransmissionType) == 'I')
        {
          local.Eft.SystemGeneratedIdentifier = 6;
          local.CashReceipt.CheckNumber =
            NumberToString(import.ElectronicFundTransmission.
              TransmissionIdentifier, 4, 12);

          if (ReadCashReceipt())
          {
            export.ForLink.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
            ExitState = "ECO_LNK_TO_COLLECTION_DETAILS";
          }
          else
          {
            ExitState = "FN0000_CASH_RECEIPT_NF";
          }
        }
        else
        {
          ExitState = "FN0000_INBOUND_EFT_REQUIRED";
        }

        break;
      case "EDTL":
        if (AsChar(export.ElectronicFundTransmission.TransmissionType) == 'O')
        {
          if (export.PaymentRequest.SystemGeneratedIdentifier > 0)
          {
            ExitState = "ECO_LNK_TO_EDTL";
          }
          else
          {
            ExitState = "FN0000_PAYMENT_REQUEST_NF";
          }
        }
        else
        {
          ExitState = "FN0000_OUTBOUND_EFT_REQUIRED";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.PayDate = source.PayDate;
    target.TransmittalAmount = source.TransmittalAmount;
    target.ApSsn = source.ApSsn;
    target.MedicalSupportId = source.MedicalSupportId;
    target.ApName = source.ApName;
    target.FipsCode = source.FipsCode;
    target.EmploymentTerminationId = source.EmploymentTerminationId;
    target.ReceivingDfiIdentification = source.ReceivingDfiIdentification;
    target.DfiAccountNumber = source.DfiAccountNumber;
    target.TransactionCode = source.TransactionCode;
    target.CaseId = source.CaseId;
    target.TransmissionStatusCode = source.TransmissionStatusCode;
    target.CompanyName = source.CompanyName;
    target.OriginatingDfiIdentification = source.OriginatingDfiIdentification;
    target.CompanyIdentificationIcd = source.CompanyIdentificationIcd;
    target.CompanyIdentificationNumber = source.CompanyIdentificationNumber;
    target.CompanyDescriptiveDate = source.CompanyDescriptiveDate;
    target.EffectiveEntryDate = source.EffectiveEntryDate;
    target.ReceivingCompanyName = source.ReceivingCompanyName;
    target.TraceNumber = source.TraceNumber;
    target.ApplicationIdentifier = source.ApplicationIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.VendorNumber = source.VendorNumber;
    target.CheckDigit = source.CheckDigit;
    target.ReceivingDfiAccountNumber = source.ReceivingDfiAccountNumber;
    target.CompanyEntryDescription = source.CompanyEntryDescription;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.ValidateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseFnConvertCoEntDescrToCrstc()
  {
    var useImport = new FnConvertCoEntDescrToCrstc.Import();
    var useExport = new FnConvertCoEntDescrToCrstc.Export();

    useImport.ElectronicFundTransmission.CompanyEntryDescription =
      export.ElectronicFundTransmission.CompanyEntryDescription;

    Call(FnConvertCoEntDescrToCrstc.Execute, useImport, useExport);

    local.ValidCoEntryDesc.Flag = useExport.ValidCoEntryDesc.Flag;
    local.SourceUsesAddendum.Flag = useExport.SourceUsesAddendum.Flag;
  }

  private void UseFnConvertCrstcToCoEntDescr()
  {
    var useImport = new FnConvertCrstcToCoEntDescr.Import();
    var useExport = new FnConvertCrstcToCoEntDescr.Export();

    useImport.CashReceiptSourceType.Assign(import.CashReceiptSourceType);

    Call(FnConvertCrstcToCoEntDescr.Execute, useImport, useExport);

    export.ElectronicFundTransmission.CompanyEntryDescription =
      useExport.ElectronicFundTransmission.CompanyEntryDescription;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.ApplicationId.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", local.Eft.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "checkNumber", local.CashReceipt.CheckNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 4);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadElectronicFundTransmission1()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission1",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId",
          export.ElectronicFundTransmission.TransmissionIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ElectronicFundTransmission.PayDate =
          db.GetNullableDate(reader, 2);
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 3);
        entities.ElectronicFundTransmission.ApSsn = db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.MedicalSupportId =
          db.GetString(reader, 5);
        entities.ElectronicFundTransmission.ApName = db.GetString(reader, 6);
        entities.ElectronicFundTransmission.FipsCode =
          db.GetNullableInt32(reader, 7);
        entities.ElectronicFundTransmission.EmploymentTerminationId =
          db.GetNullableString(reader, 8);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 9);
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 10);
        entities.ElectronicFundTransmission.DfiAccountNumber =
          db.GetNullableString(reader, 11);
        entities.ElectronicFundTransmission.TransactionCode =
          db.GetString(reader, 12);
        entities.ElectronicFundTransmission.CaseId = db.GetString(reader, 13);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 14);
        entities.ElectronicFundTransmission.CompanyName =
          db.GetNullableString(reader, 15);
        entities.ElectronicFundTransmission.OriginatingDfiIdentification =
          db.GetNullableInt32(reader, 16);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 17);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 18);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 19);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 21);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 22);
        entities.ElectronicFundTransmission.CompanyIdentificationIcd =
          db.GetNullableString(reader, 23);
        entities.ElectronicFundTransmission.CompanyIdentificationNumber =
          db.GetNullableString(reader, 24);
        entities.ElectronicFundTransmission.CompanyDescriptiveDate =
          db.GetNullableDate(reader, 25);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 26);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 27);
        entities.ElectronicFundTransmission.TraceNumber =
          db.GetNullableInt64(reader, 28);
        entities.ElectronicFundTransmission.ApplicationIdentifier =
          db.GetNullableString(reader, 29);
        entities.ElectronicFundTransmission.CollectionAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ElectronicFundTransmission.VendorNumber =
          db.GetNullableString(reader, 31);
        entities.ElectronicFundTransmission.CheckDigit =
          db.GetNullableInt32(reader, 32);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 33);
        entities.ElectronicFundTransmission.CompanyEntryDescription =
          db.GetNullableString(reader, 34);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadElectronicFundTransmission2()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission2",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.ElectronicFundTransmission.TransmissionType);
        db.SetInt32(
          command, "transmissionId",
          export.ElectronicFundTransmission.TransmissionIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ElectronicFundTransmission.PayDate =
          db.GetNullableDate(reader, 2);
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 3);
        entities.ElectronicFundTransmission.ApSsn = db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.MedicalSupportId =
          db.GetString(reader, 5);
        entities.ElectronicFundTransmission.ApName = db.GetString(reader, 6);
        entities.ElectronicFundTransmission.FipsCode =
          db.GetNullableInt32(reader, 7);
        entities.ElectronicFundTransmission.EmploymentTerminationId =
          db.GetNullableString(reader, 8);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 9);
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 10);
        entities.ElectronicFundTransmission.DfiAccountNumber =
          db.GetNullableString(reader, 11);
        entities.ElectronicFundTransmission.TransactionCode =
          db.GetString(reader, 12);
        entities.ElectronicFundTransmission.CaseId = db.GetString(reader, 13);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 14);
        entities.ElectronicFundTransmission.CompanyName =
          db.GetNullableString(reader, 15);
        entities.ElectronicFundTransmission.OriginatingDfiIdentification =
          db.GetNullableInt32(reader, 16);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 17);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 18);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 19);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 21);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 22);
        entities.ElectronicFundTransmission.CompanyIdentificationIcd =
          db.GetNullableString(reader, 23);
        entities.ElectronicFundTransmission.CompanyIdentificationNumber =
          db.GetNullableString(reader, 24);
        entities.ElectronicFundTransmission.CompanyDescriptiveDate =
          db.GetNullableDate(reader, 25);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 26);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 27);
        entities.ElectronicFundTransmission.TraceNumber =
          db.GetNullableInt64(reader, 28);
        entities.ElectronicFundTransmission.ApplicationIdentifier =
          db.GetNullableString(reader, 29);
        entities.ElectronicFundTransmission.CollectionAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ElectronicFundTransmission.VendorNumber =
          db.GetNullableString(reader, 31);
        entities.ElectronicFundTransmission.CheckDigit =
          db.GetNullableInt32(reader, 32);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 33);
        entities.ElectronicFundTransmission.CompanyEntryDescription =
          db.GetNullableString(reader, 34);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadElectronicFundTransmission3()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission3",
      (db, command) =>
      {
        db.SetString(
          command, "transmissionType",
          export.ElectronicFundTransmission.TransmissionType);
        db.SetInt32(
          command, "transmissionId",
          export.ElectronicFundTransmission.TransmissionIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.ElectronicFundTransmission.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ElectronicFundTransmission.PayDate =
          db.GetNullableDate(reader, 2);
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 3);
        entities.ElectronicFundTransmission.ApSsn = db.GetInt32(reader, 4);
        entities.ElectronicFundTransmission.MedicalSupportId =
          db.GetString(reader, 5);
        entities.ElectronicFundTransmission.ApName = db.GetString(reader, 6);
        entities.ElectronicFundTransmission.FipsCode =
          db.GetNullableInt32(reader, 7);
        entities.ElectronicFundTransmission.EmploymentTerminationId =
          db.GetNullableString(reader, 8);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 9);
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 10);
        entities.ElectronicFundTransmission.DfiAccountNumber =
          db.GetNullableString(reader, 11);
        entities.ElectronicFundTransmission.TransactionCode =
          db.GetString(reader, 12);
        entities.ElectronicFundTransmission.CaseId = db.GetString(reader, 13);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 14);
        entities.ElectronicFundTransmission.CompanyName =
          db.GetNullableString(reader, 15);
        entities.ElectronicFundTransmission.OriginatingDfiIdentification =
          db.GetNullableInt32(reader, 16);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 17);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 18);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 19);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 21);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 22);
        entities.ElectronicFundTransmission.CompanyIdentificationIcd =
          db.GetNullableString(reader, 23);
        entities.ElectronicFundTransmission.CompanyIdentificationNumber =
          db.GetNullableString(reader, 24);
        entities.ElectronicFundTransmission.CompanyDescriptiveDate =
          db.GetNullableDate(reader, 25);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 26);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 27);
        entities.ElectronicFundTransmission.TraceNumber =
          db.GetNullableInt64(reader, 28);
        entities.ElectronicFundTransmission.ApplicationIdentifier =
          db.GetNullableString(reader, 29);
        entities.ElectronicFundTransmission.CollectionAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ElectronicFundTransmission.VendorNumber =
          db.GetNullableString(reader, 31);
        entities.ElectronicFundTransmission.CheckDigit =
          db.GetNullableInt32(reader, 32);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 33);
        entities.ElectronicFundTransmission.CompanyEntryDescription =
          db.GetNullableString(reader, 34);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(
      entities.ElectronicFundTransmission.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.ElectronicFundTransmission.PrqGeneratedId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.AchFormatCode = db.GetNullableString(reader, 3);
        entities.PaymentRequest.Type1 = db.GetString(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private void UpdateElectronicFundTransmission()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var payDate = export.ElectronicFundTransmission.PayDate;
    var transmittalAmount = export.ElectronicFundTransmission.TransmittalAmount;
    var apSsn = export.ElectronicFundTransmission.ApSsn;
    var medicalSupportId = export.ElectronicFundTransmission.MedicalSupportId;
    var apName = export.ElectronicFundTransmission.ApName;
    var fipsCode =
      export.ElectronicFundTransmission.FipsCode.GetValueOrDefault();
    var employmentTerminationId =
      export.ElectronicFundTransmission.EmploymentTerminationId ?? "";
    var receivingDfiIdentification =
      export.ElectronicFundTransmission.ReceivingDfiIdentification.
        GetValueOrDefault();
    var dfiAccountNumber =
      export.ElectronicFundTransmission.ReceivingDfiAccountNumber ?? "";
    var transactionCode = export.ElectronicFundTransmission.TransactionCode;
    var caseId = export.ElectronicFundTransmission.CaseId;
    var transmissionStatusCode = local.ForUpdate.TransmissionStatusCode;
    var companyName = export.ElectronicFundTransmission.CompanyName ?? "";
    var originatingDfiIdentification =
      export.ElectronicFundTransmission.OriginatingDfiIdentification.
        GetValueOrDefault();
    var companyIdentificationIcd =
      export.ElectronicFundTransmission.CompanyIdentificationIcd ?? "";
    var companyIdentificationNumber =
      export.ElectronicFundTransmission.CompanyIdentificationNumber ?? "";
    var companyDescriptiveDate =
      export.ElectronicFundTransmission.CompanyDescriptiveDate;
    var effectiveEntryDate =
      export.ElectronicFundTransmission.EffectiveEntryDate;
    var receivingCompanyName =
      export.ElectronicFundTransmission.ReceivingCompanyName ?? "";
    var traceNumber =
      export.ElectronicFundTransmission.TraceNumber.GetValueOrDefault();
    var applicationIdentifier =
      export.ElectronicFundTransmission.ApplicationIdentifier ?? "";
    var collectionAmount =
      export.ElectronicFundTransmission.CollectionAmount.GetValueOrDefault();
    var vendorNumber = export.ElectronicFundTransmission.VendorNumber ?? "";
    var checkDigit =
      export.ElectronicFundTransmission.CheckDigit.GetValueOrDefault();
    var companyEntryDescription =
      export.ElectronicFundTransmission.CompanyEntryDescription ?? "";

    entities.ElectronicFundTransmission.Populated = false;
    Update("UpdateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableDate(command, "payDate", payDate);
        db.SetDecimal(command, "transmittalAmount", transmittalAmount);
        db.SetInt32(command, "apSsn", apSsn);
        db.SetString(command, "medicalSupportId", medicalSupportId);
        db.SetString(command, "apName", apName);
        db.SetNullableInt32(command, "fipsCode", fipsCode);
        db.SetNullableString(
          command, "employmentTermId", employmentTerminationId);
        db.SetNullableInt32(
          command, "receivingDfiIden", receivingDfiIdentification);
        db.SetNullableString(command, "dfiAcctNumber", dfiAccountNumber);
        db.SetString(command, "transactionCode", transactionCode);
        db.SetString(command, "caseId", caseId);
        db.SetString(command, "transStatusCode", transmissionStatusCode);
        db.SetNullableString(command, "companyName", companyName);
        db.SetNullableInt32(
          command, "origDfiIdent", originatingDfiIdentification);
        db.SetNullableString(
          command, "companyIdentIcd", companyIdentificationIcd);
        db.SetNullableString(
          command, "companyIdentNum", companyIdentificationNumber);
        db.SetNullableDate(command, "companyDescDate", companyDescriptiveDate);
        db.SetNullableDate(command, "effectiveEntryDt", effectiveEntryDate);
        db.SetNullableString(command, "recvCompanyName", receivingCompanyName);
        db.SetNullableInt64(command, "traceNumber", traceNumber);
        db.
          SetNullableString(command, "applicationIdent", applicationIdentifier);
          
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableString(command, "vendorNumber", vendorNumber);
        db.SetNullableInt32(command, "checkDigit", checkDigit);
        db.SetNullableString(
          command, "companyEntryDesc", companyEntryDescription);
        db.SetString(
          command, "transmissionType",
          entities.ElectronicFundTransmission.TransmissionType);
        db.SetInt32(
          command, "transmissionId",
          entities.ElectronicFundTransmission.TransmissionIdentifier);
      });

    entities.ElectronicFundTransmission.LastUpdatedBy = lastUpdatedBy;
    entities.ElectronicFundTransmission.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ElectronicFundTransmission.PayDate = payDate;
    entities.ElectronicFundTransmission.TransmittalAmount = transmittalAmount;
    entities.ElectronicFundTransmission.ApSsn = apSsn;
    entities.ElectronicFundTransmission.MedicalSupportId = medicalSupportId;
    entities.ElectronicFundTransmission.ApName = apName;
    entities.ElectronicFundTransmission.FipsCode = fipsCode;
    entities.ElectronicFundTransmission.EmploymentTerminationId =
      employmentTerminationId;
    entities.ElectronicFundTransmission.ReceivingDfiIdentification =
      receivingDfiIdentification;
    entities.ElectronicFundTransmission.DfiAccountNumber = dfiAccountNumber;
    entities.ElectronicFundTransmission.TransactionCode = transactionCode;
    entities.ElectronicFundTransmission.CaseId = caseId;
    entities.ElectronicFundTransmission.TransmissionStatusCode =
      transmissionStatusCode;
    entities.ElectronicFundTransmission.CompanyName = companyName;
    entities.ElectronicFundTransmission.OriginatingDfiIdentification =
      originatingDfiIdentification;
    entities.ElectronicFundTransmission.CompanyIdentificationIcd =
      companyIdentificationIcd;
    entities.ElectronicFundTransmission.CompanyIdentificationNumber =
      companyIdentificationNumber;
    entities.ElectronicFundTransmission.CompanyDescriptiveDate =
      companyDescriptiveDate;
    entities.ElectronicFundTransmission.EffectiveEntryDate = effectiveEntryDate;
    entities.ElectronicFundTransmission.ReceivingCompanyName =
      receivingCompanyName;
    entities.ElectronicFundTransmission.TraceNumber = traceNumber;
    entities.ElectronicFundTransmission.ApplicationIdentifier =
      applicationIdentifier;
    entities.ElectronicFundTransmission.CollectionAmount = collectionAmount;
    entities.ElectronicFundTransmission.VendorNumber = vendorNumber;
    entities.ElectronicFundTransmission.CheckDigit = checkDigit;
    entities.ElectronicFundTransmission.CompanyEntryDescription =
      companyEntryDescription;
    entities.ElectronicFundTransmission.Populated = true;
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
    /// A value of Pf15Nf.
    /// </summary>
    [JsonPropertyName("pf15Nf")]
    public Common Pf15Nf
    {
      get => pf15Nf ??= new();
      set => pf15Nf = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of HiddenElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("hiddenElectronicFundTransmission")]
    public ElectronicFundTransmission HiddenElectronicFundTransmission
    {
      get => hiddenElectronicFundTransmission ??= new();
      set => hiddenElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public TextWorkArea Type1
    {
      get => type1 ??= new();
      set => type1 = value;
    }

    /// <summary>
    /// A value of NextRead.
    /// </summary>
    [JsonPropertyName("nextRead")]
    public ElectronicFundTransmission NextRead
    {
      get => nextRead ??= new();
      set => nextRead = value;
    }

    /// <summary>
    /// A value of ApplicationId.
    /// </summary>
    [JsonPropertyName("applicationId")]
    public Standard ApplicationId
    {
      get => applicationId ??= new();
      set => applicationId = value;
    }

    /// <summary>
    /// A value of CoEntryDesc.
    /// </summary>
    [JsonPropertyName("coEntryDesc")]
    public Standard CoEntryDesc
    {
      get => coEntryDesc ??= new();
      set => coEntryDesc = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private Common pf15Nf;
    private CodeValue selected;
    private CashReceiptSourceType cashReceiptSourceType;
    private PaymentRequest paymentRequest;
    private ElectronicFundTransmission electronicFundTransmission;
    private ElectronicFundTransmission hiddenElectronicFundTransmission;
    private TextWorkArea type1;
    private ElectronicFundTransmission nextRead;
    private Standard applicationId;
    private Standard coEntryDesc;
    private NextTranInfo hiddenNextTranInfo;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Pf15Nf.
    /// </summary>
    [JsonPropertyName("pf15Nf")]
    public Common Pf15Nf
    {
      get => pf15Nf ??= new();
      set => pf15Nf = value;
    }

    /// <summary>
    /// A value of ForLink.
    /// </summary>
    [JsonPropertyName("forLink")]
    public CashReceipt ForLink
    {
      get => forLink ??= new();
      set => forLink = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public TextWorkArea Type1
    {
      get => type1 ??= new();
      set => type1 = value;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of HiddenElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("hiddenElectronicFundTransmission")]
    public ElectronicFundTransmission HiddenElectronicFundTransmission
    {
      get => hiddenElectronicFundTransmission ??= new();
      set => hiddenElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of NextRead.
    /// </summary>
    [JsonPropertyName("nextRead")]
    public ElectronicFundTransmission NextRead
    {
      get => nextRead ??= new();
      set => nextRead = value;
    }

    /// <summary>
    /// A value of ApplicationId.
    /// </summary>
    [JsonPropertyName("applicationId")]
    public Standard ApplicationId
    {
      get => applicationId ??= new();
      set => applicationId = value;
    }

    /// <summary>
    /// A value of CoEntryDesc.
    /// </summary>
    [JsonPropertyName("coEntryDesc")]
    public Standard CoEntryDesc
    {
      get => coEntryDesc ??= new();
      set => coEntryDesc = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of ToList.
    /// </summary>
    [JsonPropertyName("toList")]
    public Code ToList
    {
      get => toList ??= new();
      set => toList = value;
    }

    private Common pf15Nf;
    private CashReceipt forLink;
    private PaymentRequest paymentRequest;
    private Code code;
    private Standard standard;
    private TextWorkArea type1;
    private CsePerson csePerson;
    private ElectronicFundTransmission electronicFundTransmission;
    private ElectronicFundTransmission hiddenElectronicFundTransmission;
    private ElectronicFundTransmission nextRead;
    private Standard applicationId;
    private Standard coEntryDesc;
    private NextTranInfo hiddenNextTranInfo;
    private Code toList;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ValidCoEntryDesc.
    /// </summary>
    [JsonPropertyName("validCoEntryDesc")]
    public Common ValidCoEntryDesc
    {
      get => validCoEntryDesc ??= new();
      set => validCoEntryDesc = value;
    }

    /// <summary>
    /// A value of SourceUsesAddendum.
    /// </summary>
    [JsonPropertyName("sourceUsesAddendum")]
    public Common SourceUsesAddendum
    {
      get => sourceUsesAddendum ??= new();
      set => sourceUsesAddendum = value;
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
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public CashReceiptType Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public ElectronicFundTransmission ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of CreateSuccessful.
    /// </summary>
    [JsonPropertyName("createSuccessful")]
    public Common CreateSuccessful
    {
      get => createSuccessful ??= new();
      set => createSuccessful = value;
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
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of InitializedElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("initializedElectronicFundTransmission")]
    public ElectronicFundTransmission InitializedElectronicFundTransmission
    {
      get => initializedElectronicFundTransmission ??= new();
      set => initializedElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of PassToUpdate.
    /// </summary>
    [JsonPropertyName("passToUpdate")]
    public ElectronicFundTransmission PassToUpdate
    {
      get => passToUpdate ??= new();
      set => passToUpdate = value;
    }

    /// <summary>
    /// A value of ValidateCode.
    /// </summary>
    [JsonPropertyName("validateCode")]
    public Code ValidateCode
    {
      get => validateCode ??= new();
      set => validateCode = value;
    }

    /// <summary>
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    private Common validCoEntryDesc;
    private Common sourceUsesAddendum;
    private CashReceipt cashReceipt;
    private CashReceiptType eft;
    private ElectronicFundTransmission forUpdate;
    private Common createSuccessful;
    private DateWorkArea max;
    private DateWorkArea initializedDateWorkArea;
    private ElectronicFundTransmission initializedElectronicFundTransmission;
    private ElectronicFundTransmission passToUpdate;
    private Code validateCode;
    private CodeValue validateCodeValue;
    private Common validCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PaymentStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public PaymentStatusHistory Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private PaymentRequest paymentRequest;
    private PaymentStatusHistory new1;
    private PaymentStatusHistory old;
    private ElectronicFundTransmission electronicFundTransmission;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
  }
#endregion
}
