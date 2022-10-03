// Program: SP_DOC_GET_SERVICE_PROVIDER, ID: 371914301, model: 746.
// Short name: SWE02253
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
/// A program: SP_DOC_GET_SERVICE_PROVIDER.
/// </para>
/// <para>
/// This CAB determines which service provider will own the outgoing_document 
/// and the monitored_document.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocGetServiceProvider: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_GET_SERVICE_PROVIDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocGetServiceProvider(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocGetServiceProvider.
  /// </summary>
  public SpDocGetServiceProvider(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // CHANGE LOG:
    // 01/20/99	PMcElderry/MRamirez
    // Allow USER to select Service Provider for NOA's; PR#84668
    // 02/07/00	PMcElderry
    // PR # 77892 - Credit Notification Letter has incorrect SVPO
    // for IV-E (fostercare) case.  Changed code to now look for
    // only the highest case associated for the obligor certified for
    // credit reporting.
    // 02/09/00	PMcElderry
    // PR # 87872 - Rewrote "AAC" logic when printing CRDTNOTF
    // so that the service provider associated to the case with the
    // highest obligation will be represented on the letter
    // 09/05/00	PMcElderry
    // PR # 102336 - Case "AAC":  remove usege of admin_
    // action_cert_obligation and adm_act_cert_debt_detail.  Logic
    // that was being performed in this AB now being performed in
    // LE_CREATE_CRED_CERTIFICATION
    // 04/03/2001	M Ramirez	WR291 Seg C
    // Added locate request
    // 10/15/2001	M Ramirez	WR 10533
    // Added office fax number and sp email address
    // ---------------------------------------------------------------
    // 11/24/2004:  cmj cred problem with getting an office not found due to old
    // document
    // status of 1 instead of document status 2.   CHeck for cse_person number 
    // and read case by cse_person number . Need case to get service provider.
    if (Lt(local.Null1.Date, import.Current.Date))
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    local.ReasonCode.Text3 = "RSP";
    local.OfficeAddress.Type1 = "M";
    export.SpDocKey.Assign(import.SpDocKey);
    local.Document.BusinessObject = import.Document.BusinessObject;

    if (Equal(import.Document.BusinessObject, "ADA") || Equal
      (import.Document.BusinessObject, "APT") || Equal
      (import.Document.BusinessObject, "CAS") || Equal
      (import.Document.BusinessObject, "CAU") || Equal
      (import.Document.BusinessObject, "LEA") || Equal
      (import.Document.BusinessObject, "LOC") || Equal
      (import.Document.BusinessObject, "LRF") || Equal
      (import.Document.BusinessObject, "OAA") || Equal
      (import.Document.BusinessObject, "OBL") || Equal
      (import.Document.BusinessObject, "PAR") || Equal
      (import.Document.BusinessObject, "PER"))
    {
    }
    else
    {
      switch(TrimEnd(import.Document.BusinessObject))
      {
        case "AAC":
          local.Document.BusinessObject = "CAS";

          // ---------------
          // Beg PR # 102336
          // ---------------
          // ---------------
          // End PR # 102336
          // ---------------
          break;
        case "BKR":
          local.Document.BusinessObject = "PER";

          break;
        case "CON":
          local.Document.BusinessObject = "PER";

          break;
        case "CPA":
          local.Document.BusinessObject = "PER";

          break;
        case "CPR":
          local.Document.BusinessObject = "PER";

          break;
        case "CRD":
          local.Document.BusinessObject = "CRD";

          break;
        case "CSW":
          local.Document.BusinessObject = "CAS";

          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (ReadCase4())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
          }

          break;
        case "DOC":
          local.Document.BusinessObject = "PER";

          break;
        case "FPL":
          local.Document.BusinessObject = "PER";

          break;
        case "FTR":
          local.Document.BusinessObject = "PER";

          break;
        case "GNT":
          local.Document.BusinessObject = "CAS";

          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (ReadCase9())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
          }

          break;
        case "HIN":
          local.Document.BusinessObject = "PER";

          break;
        case "ICS":
          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            // mjr----> Find case from a person number
            if (!IsEmpty(export.SpDocKey.KeyAp))
            {
              local.CsePerson.Number = export.SpDocKey.KeyAp;
              local.CaseRole.Type1 = "AP";
            }
            else if (!IsEmpty(export.SpDocKey.KeyAr))
            {
              local.CsePerson.Number = export.SpDocKey.KeyAr;
              local.CaseRole.Type1 = "AR";
            }
            else if (!IsEmpty(export.SpDocKey.KeyChild))
            {
              local.CsePerson.Number = export.SpDocKey.KeyChild;
              local.CaseRole.Type1 = "CH";
            }
            else if (!IsEmpty(export.SpDocKey.KeyPerson))
            {
              local.CsePerson.Number = export.SpDocKey.KeyPerson;

              if (ReadCase7())
              {
                export.SpDocKey.KeyCase = entities.Case1.Number;
              }

              goto Test1;
            }
            else
            {
              goto Test1;
            }

            if (ReadCase6())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
          }

Test1:

          if (!IsEmpty(export.SpDocKey.KeyCase))
          {
            local.Document.BusinessObject = "CAS";
          }

          break;
        case "INC":
          local.Document.BusinessObject = "PER";

          break;
        case "IRQ":
          local.Document.BusinessObject = "PER";

          break;
        case "ISC":
          local.Document.BusinessObject = "CAS";

          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (ReadCase10())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
          }

          break;
        case "LAD":
          local.Document.BusinessObject = "CAS";

          if (!IsEmpty(export.SpDocKey.KeyCase))
          {
            // mjr---> nop
          }
          else if (!IsEmpty(export.SpDocKey.KeyChild))
          {
            if (ReadCase3())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
          }
          else if (!IsEmpty(export.SpDocKey.KeyPerson))
          {
            if (ReadCase8())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
          }
          else if (ReadCase1())
          {
            export.SpDocKey.KeyCase = entities.Case1.Number;
          }

          break;
        case "MIL":
          local.Document.BusinessObject = "PER";

          break;
        case "NOA":
          local.Document.BusinessObject = "PER";

          break;
        case "OBT":
          local.Document.BusinessObject = "PER";

          break;
        case "OVR":
          local.Document.BusinessObject = "OVR";

          break;
        case "PGT":
          local.Document.BusinessObject = "PER";

          break;
        case "PHI":
          // mjr
          // ----------------------------------------------------------
          // PHI - Personal Health Insurance
          // Find case from Child number
          // ---------------------------------------------------------------
          local.Document.BusinessObject = "CAS";

          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (!IsEmpty(export.SpDocKey.KeyChild))
            {
              // mjr--->  It is possible to get two records that satisfy the 
              // READ
              if (ReadCase3())
              {
                export.SpDocKey.KeyCase = entities.Case1.Number;
              }
            }
          }

          break;
        case "PPR":
          local.Document.BusinessObject = "PER";

          break;
        case "RCP":
          break;
        case "TRB":
          local.Document.BusinessObject = "PER";

          break;
        case "":
          return;
        default:
          local.Document.BusinessObject = "PER";

          break;
      }
    }

    // mjr
    // -------------------------------------------------------
    // Assignable business objects
    // ----------------------------------------------------------
    switch(TrimEnd(local.Document.BusinessObject))
    {
      case "ADA":
        if (export.SpDocKey.KeyAdminAppeal <= 0)
        {
          break;
        }

        if (ReadAdministrativeAppealAssignment())
        {
          ReadOfficeServiceProviderServiceProviderOffice3();
        }

        break;
      case "APT":
        if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyAppointment))
        {
          break;
        }

        ReadOfficeServiceProviderServiceProviderOffice1();
        local.OfficeAddress.Type1 = "R";

        // 11/24/2004:  cmj cred problem with getting an office not found due to
        // old document
        // status of 1 instead of document status 2.   CHeck for cse_person 
        // number and read case by cse_person number . Need case to get service
        // provider.
        break;
      case "CAS":
        if (IsEmpty(export.SpDocKey.KeyCase))
        {
          if (!IsEmpty(export.SpDocKey.KeyChild))
          {
            foreach(var item in ReadCase12())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;

              if (IsEmpty(export.SpDocKey.KeyCase))
              {
                goto Test4;
              }
            }
          }
          else if (!IsEmpty(export.SpDocKey.KeyPerson))
          {
            foreach(var item in ReadCase11())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;

              if (IsEmpty(export.SpDocKey.KeyCase))
              {
                goto Test4;
              }
            }
          }
          else if (IsEmpty(export.SpDocKey.KeyCase))
          {
            break;
          }
        }

        if (ReadCaseAssignment1())
        {
          ReadOfficeServiceProviderServiceProviderOffice4();
        }

        break;
      case "CAU":
        break;
      case "CRD":
        if (IsEmpty(export.SpDocKey.KeyCase))
        {
          if (export.SpDocKey.KeyLegalAction <= 0)
          {
            if (ReadCashReceiptDetail())
            {
              if (ReadLegalAction())
              {
                export.SpDocKey.KeyLegalAction =
                  entities.LegalAction.Identifier;
              }
            }

            if (export.SpDocKey.KeyLegalAction <= 0)
            {
              goto Test2;
            }
          }

          if (ReadCase2())
          {
            export.SpDocKey.KeyCase = entities.Case1.Number;
          }

          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (ReadCase5())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }

            if (IsEmpty(export.SpDocKey.KeyCase))
            {
              break;
            }
          }
        }

Test2:

        if (ReadCaseAssignment2())
        {
          ReadOfficeServiceProviderServiceProviderOffice4();
        }

        break;
      case "ICS":
        // mjr
        // ---------------------------------------------------
        // Find the service provider based on the
        // income_source worker_id
        // ------------------------------------------------------
        if (IsEmpty(export.SpDocKey.KeyPerson))
        {
          break;
        }

        if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyIncomeSource))
        {
          break;
        }

        if (!ReadIncomeSource())
        {
          break;
        }

        if (!ReadServiceProvider4())
        {
          break;
        }

        if (ReadOfficeServiceProvider1())
        {
          ReadOffice3();
        }

        break;
      case "IRQ":
        break;
      case "ISC":
        break;
      case "LEA":
        if (export.SpDocKey.KeyLegalAction <= 0)
        {
          break;
        }

        if (ReadLegalActionAssigment())
        {
          ReadOfficeServiceProviderServiceProviderOffice2();
        }

        break;
      case "LOC":
        if (!IsEmpty(import.SpDocKey.KeyPerson))
        {
          local.CsePerson.Number = import.SpDocKey.KeyPerson;
        }
        else if (!IsEmpty(import.SpDocKey.KeyAp))
        {
          local.CsePerson.Number = import.SpDocKey.KeyAp;
        }
        else
        {
          break;
        }

        local.ServiceProvider.SystemGeneratedId = -1;
        local.Office.SystemGeneratedId = -1;
        export.OutDocRtrnAddr.ServProvSysGenId = 0;
        export.OutDocRtrnAddr.ServProvUserId = "";

        foreach(var item in ReadCaseCaseAssignmentOfficeServiceProviderServiceProvider())
          
        {
          // mjr
          // --------------------------------------------
          // 04/03/2001
          // No matter how the return address shows on the
          // document, this is the SP to whom the document is assigned
          // ---------------------------------------------------------
          if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId))
          {
            export.OutDocRtrnAddr.ServProvSysGenId =
              entities.ServiceProvider.SystemGeneratedId;
            export.OutDocRtrnAddr.ServProvUserId =
              entities.ServiceProvider.UserId;
            export.OutDocRtrnAddr.OfficeSysGenId =
              entities.Office.SystemGeneratedId;
          }

          if (local.ServiceProvider.SystemGeneratedId == -1)
          {
            local.ServiceProvider.SystemGeneratedId =
              entities.ServiceProvider.SystemGeneratedId;
          }
          else if (local.ServiceProvider.SystemGeneratedId == entities
            .ServiceProvider.SystemGeneratedId)
          {
          }
          else
          {
            local.ServiceProvider.SystemGeneratedId = 0;
          }

          if (local.Office.SystemGeneratedId == -1)
          {
            local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          }
          else if (local.Office.SystemGeneratedId == entities
            .Office.SystemGeneratedId)
          {
          }
          else
          {
            local.Office.SystemGeneratedId = 0;

            break;
          }
        }

        if (local.Office.SystemGeneratedId < 0)
        {
          // mjr
          // --------------------------------------------
          // 04/03/2001
          // No valid cases were found
          // ---------------------------------------------------------
        }
        else if (local.Office.SystemGeneratedId == 0)
        {
          // mjr
          // --------------------------------------------
          // 04/03/2001
          // Multiple offices were found
          // ---------------------------------------------------------
          if (ReadOffice2())
          {
            export.OutDocRtrnAddr.OspWorkPhoneAreaCode =
              entities.Office.MainPhoneAreaCode.GetValueOrDefault();
            export.OutDocRtrnAddr.OspWorkPhoneNumber =
              entities.Office.MainPhoneNumber.GetValueOrDefault();
            export.Office.MainFaxAreaCode = entities.Office.MainFaxAreaCode;
            export.Office.MainFaxPhoneNumber =
              entities.Office.MainFaxPhoneNumber;
            export.OutDocRtrnAddr.OfficeName = entities.Office.Name;
          }
          else
          {
            return;
          }

          ReadOfficeAddress3();

          if (!IsEmpty(entities.OfficeAddress.Street1))
          {
            export.OutDocRtrnAddr.OffcAddrStreet1 =
              entities.OfficeAddress.Street1;
            export.OutDocRtrnAddr.OffcAddrStreet2 =
              entities.OfficeAddress.Street2;
            export.OutDocRtrnAddr.OffcAddrCity = entities.OfficeAddress.City;
            export.OutDocRtrnAddr.OffcAddrState =
              entities.OfficeAddress.StateProvince;
            export.OutDocRtrnAddr.OffcAddrZip = entities.OfficeAddress.Zip;
            export.OutDocRtrnAddr.OffcAddrZip4 = entities.OfficeAddress.Zip4;
            export.OutDocRtrnAddr.OffcAddrZip3 = entities.OfficeAddress.Zip3;
          }

          return;
        }
        else
        {
          // mjr
          // --------------------------------------------
          // 04/03/2001
          // One office was found
          // ---------------------------------------------------------
          if (local.ServiceProvider.SystemGeneratedId == 0)
          {
            // mjr
            // --------------------------------------------
            // 04/03/2001
            // Multiple service providers were found in the same office
            // ---------------------------------------------------------
            export.OutDocRtrnAddr.OspWorkPhoneAreaCode =
              entities.Office.MainPhoneAreaCode.GetValueOrDefault();
            export.OutDocRtrnAddr.OspWorkPhoneNumber =
              entities.Office.MainPhoneNumber.GetValueOrDefault();
            export.Office.MainFaxAreaCode = entities.Office.MainFaxAreaCode;
            export.Office.MainFaxPhoneNumber =
              entities.Office.MainFaxPhoneNumber;
            export.OutDocRtrnAddr.OfficeName = entities.Office.Name;
            ReadOfficeAddress2();

            if (!IsEmpty(entities.OfficeAddress.Street1))
            {
              export.OutDocRtrnAddr.OffcAddrStreet1 =
                entities.OfficeAddress.Street1;
              export.OutDocRtrnAddr.OffcAddrStreet2 =
                entities.OfficeAddress.Street2;
              export.OutDocRtrnAddr.OffcAddrCity = entities.OfficeAddress.City;
              export.OutDocRtrnAddr.OffcAddrState =
                entities.OfficeAddress.StateProvince;
              export.OutDocRtrnAddr.OffcAddrZip = entities.OfficeAddress.Zip;
              export.OutDocRtrnAddr.OffcAddrZip4 = entities.OfficeAddress.Zip4;
              export.OutDocRtrnAddr.OffcAddrZip3 = entities.OfficeAddress.Zip3;
            }

            return;
          }
          else
          {
            // mjr
            // --------------------------------------------
            // 04/03/2001
            // One service provider in one office
            // ---------------------------------------------------------
          }
        }

        break;
      case "LRF":
        // mjr
        // -------------------------------------------------
        // 08/09/1999
        // Added relationship to case to following READ EACH
        // --------------------------------------------------------------
        if (IsEmpty(export.SpDocKey.KeyCase))
        {
          break;
        }

        if (export.SpDocKey.KeyLegalReferral <= 0)
        {
          break;
        }

        if (ReadLegalReferralAssignment())
        {
          ReadOfficeServiceProviderServiceProviderOffice5();
        }

        break;
      case "OAA":
        // mjr
        // ---------------------------------------------------
        // Find the service provider based on the
        // obligation_admin_action created_by
        // ------------------------------------------------------
        if (IsEmpty(export.SpDocKey.KeyPerson))
        {
          break;
        }

        if (!Lt(local.Null1.Date, export.SpDocKey.KeyObligationAdminAction))
        {
          break;
        }

        if (IsEmpty(export.SpDocKey.KeyAdminAction))
        {
          break;
        }

        if (export.SpDocKey.KeyObligation <= 0)
        {
          break;
        }

        if (export.SpDocKey.KeyObligationType <= 0)
        {
          break;
        }

        if (IsEmpty(export.SpDocKey.KeyPersonAccount))
        {
          export.SpDocKey.KeyPersonAccount = "R";
        }

        if (!ReadObligationAdministrativeAction())
        {
          break;
        }

        if (!ReadServiceProvider3())
        {
          break;
        }

        local.Position.Count = 0;

        foreach(var item in ReadOfficeServiceProvider3())
        {
          ++local.Position.Count;

          if (local.Position.Count > 1)
          {
            return;
          }

          ReadOffice3();
        }

        break;
      case "OBL":
        if (IsEmpty(export.SpDocKey.KeyPerson))
        {
          break;
        }

        if (IsEmpty(export.SpDocKey.KeyPersonAccount))
        {
          export.SpDocKey.KeyPersonAccount = "R";
        }

        if (!ReadCsePersonAccount1())
        {
          break;
        }

        if (export.SpDocKey.KeyObligation > 0 && export
          .SpDocKey.KeyObligationType > 0)
        {
          if (ReadObligationAssignment1())
          {
            ReadOfficeServiceProviderServiceProviderOffice6();
          }
        }
        else
        {
          // mjr
          // ------------------------------------------------------
          // 03/06/1999
          // Since we are not provided with a specific obligation, find the
          // oldest active obligation for the cse_person.
          // Currently this only applies to recovery debts
          // (obligation_type classification = 'R').
          // -------------------------------------------------------------------
          foreach(var item in ReadObligationObligationType())
          {
            // mjr---> Is this debt active?
            if (!ReadObligationTransaction())
            {
              continue;
            }

            if (ReadDebtDetail())
            {
              if (Lt(local.Null1.Date, entities.DebtDetail.RetiredDt))
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

            if (ReadObligationAssignment2())
            {
              ReadOfficeServiceProviderServiceProviderOffice6();

              break;
            }
          }
        }

        break;
      case "OVR":
        if (IsEmpty(export.SpDocKey.KeyCase))
        {
          if (export.SpDocKey.KeyLegalAction <= 0)
          {
            if (ReadCashReceiptDetail())
            {
              if (!IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
              {
                if (ReadLegalActionObligation2())
                {
                  export.SpDocKey.KeyLegalAction =
                    entities.LegalAction.Identifier;
                }
              }
              else if (ReadLegalActionObligation1())
              {
                export.SpDocKey.KeyLegalAction =
                  entities.LegalAction.Identifier;
              }
            }

            if (export.SpDocKey.KeyLegalAction <= 0)
            {
              break;
            }
          }

          if (ReadCase2())
          {
            export.SpDocKey.KeyCase = entities.Case1.Number;
          }

          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (ReadCase5())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }

            if (IsEmpty(export.SpDocKey.KeyCase))
            {
              break;
            }
          }
        }

        if (ReadCaseAssignment2())
        {
          ReadOfficeServiceProviderServiceProviderOffice4();
        }

        break;
      case "PAR":
        break;
      case "PER":
        UseSpReadOutDocRtrnAddr();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------
          // Validate the selected SP, Office and OSP
          // ------------------------------------------
          if (!ReadServiceProvider1())
          {
            goto Test3;
          }

          if (!ReadOffice1())
          {
            goto Test3;
          }

          if (!ReadOfficeServiceProvider2())
          {
            goto Test3;
          }

          // ------------------------------------------
          // The selected SP, Office and OSP are valid
          // Continue normal processing
          // ------------------------------------------
          break;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

Test3:

        // ------------------------------------------
        // The selected SP, Office and/or OSP are not valid
        // Create a new Out Doc Return Address
        // ------------------------------------------
        if (!ReadServiceProvider2())
        {
          break;
        }

        if (ReadOfficeServiceProviderOffice())
        {
          UseSpCreateOutDocRtrnAddr();

          // ------------------------------------------
          // If we can't create an Out Doc Return Address, just return
          // an empty view, not an exitstate
          // ------------------------------------------
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }

        break;
      case "RCP":
        if (export.SpDocKey.KeyRecaptureRule <= 0)
        {
          break;
        }

        if (!ReadCsePersonAccount2())
        {
          break;
        }

        if (export.SpDocKey.KeyObligation > 0 && export
          .SpDocKey.KeyObligationType > 0)
        {
          if (ReadObligationAssignment1())
          {
            ReadOfficeServiceProviderServiceProviderOffice6();
          }
        }
        else
        {
          // mjr
          // ------------------------------------------------------
          // 03/06/1999
          // Since we are not provided with a specific obligation, find the
          // oldest active obligation for the cse_person.
          // Currently this only applies to recovery debts
          // (obligation_type classification = 'R').
          // -------------------------------------------------------------------
          foreach(var item in ReadObligationObligationType())
          {
            // mjr---> Is this debt active?
            if (!ReadObligationTransaction())
            {
              continue;
            }

            if (ReadDebtDetail())
            {
              if (Lt(local.Null1.Date, entities.DebtDetail.RetiredDt))
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

            if (ReadObligationAssignment2())
            {
              ReadOfficeServiceProviderServiceProviderOffice6();

              break;
            }
          }
        }

        break;
      default:
        break;
    }

Test4:

    if (IsEmpty(entities.OfficeServiceProvider.RoleCode))
    {
      return;
    }

    export.OutDocRtrnAddr.ServProvUserId = entities.ServiceProvider.UserId;
    export.OutDocRtrnAddr.ServProvFirstName =
      entities.ServiceProvider.FirstName;
    export.OutDocRtrnAddr.ServProvMi = entities.ServiceProvider.MiddleInitial;
    export.OutDocRtrnAddr.ServProvLastName = entities.ServiceProvider.LastName;
    export.ServiceProvider.EmailAddress = entities.ServiceProvider.EmailAddress;
    export.OutDocRtrnAddr.OspCertificationNumber =
      entities.ServiceProvider.CertificationNumber;
    export.OutDocRtrnAddr.ServProvSysGenId =
      entities.ServiceProvider.SystemGeneratedId;
    export.OutDocRtrnAddr.OspRoleCode = entities.OfficeServiceProvider.RoleCode;
    export.OutDocRtrnAddr.OspLocalContactCode =
      entities.OfficeServiceProvider.LocalContactCodeForIrs;
    export.OutDocRtrnAddr.OspEffectiveDate =
      entities.OfficeServiceProvider.EffectiveDate;
    export.OutDocRtrnAddr.OspWorkPhoneAreaCode =
      entities.OfficeServiceProvider.WorkPhoneAreaCode;
    export.OutDocRtrnAddr.OspWorkPhoneNumber =
      entities.OfficeServiceProvider.WorkPhoneNumber;
    export.OutDocRtrnAddr.OspWorkPhoneExtension =
      entities.OfficeServiceProvider.WorkPhoneExtension;
    export.OutDocRtrnAddr.OfficeSysGenId = entities.Office.SystemGeneratedId;
    export.OutDocRtrnAddr.OfficeName = entities.Office.Name;
    export.Office.MainFaxAreaCode = entities.Office.MainFaxAreaCode;
    export.Office.MainFaxPhoneNumber = entities.Office.MainFaxPhoneNumber;
    ReadServiceProviderAddress();

    if (!IsEmpty(entities.ServiceProviderAddress.Street1))
    {
      export.OutDocRtrnAddr.OffcAddrStreet1 =
        entities.ServiceProviderAddress.Street1;
      export.OutDocRtrnAddr.OffcAddrStreet2 =
        entities.ServiceProviderAddress.Street2;
      export.OutDocRtrnAddr.OffcAddrCity = entities.ServiceProviderAddress.City;
      export.OutDocRtrnAddr.OffcAddrState =
        entities.ServiceProviderAddress.StateProvince;
      export.OutDocRtrnAddr.OffcAddrZip = entities.ServiceProviderAddress.Zip;
      export.OutDocRtrnAddr.OffcAddrZip4 = entities.ServiceProviderAddress.Zip4;
      export.OutDocRtrnAddr.OffcAddrZip3 = entities.ServiceProviderAddress.Zip3;

      return;
    }

    ReadOfficeAddress1();

    if (!IsEmpty(entities.OfficeAddress.Street1))
    {
      export.OutDocRtrnAddr.OffcAddrStreet1 = entities.OfficeAddress.Street1;
      export.OutDocRtrnAddr.OffcAddrStreet2 = entities.OfficeAddress.Street2;
      export.OutDocRtrnAddr.OffcAddrCity = entities.OfficeAddress.City;
      export.OutDocRtrnAddr.OffcAddrState =
        entities.OfficeAddress.StateProvince;
      export.OutDocRtrnAddr.OffcAddrZip = entities.OfficeAddress.Zip;
      export.OutDocRtrnAddr.OffcAddrZip4 = entities.OfficeAddress.Zip4;
      export.OutDocRtrnAddr.OffcAddrZip3 = entities.OfficeAddress.Zip3;
    }
  }

  private static void MoveOutDocRtrnAddr(OutDocRtrnAddr source,
    OutDocRtrnAddr target)
  {
    target.OspWorkPhoneNumber = source.OspWorkPhoneNumber;
    target.OspWorkPhoneAreaCode = source.OspWorkPhoneAreaCode;
    target.OspWorkPhoneExtension = source.OspWorkPhoneExtension;
    target.OspCertificationNumber = source.OspCertificationNumber;
    target.OspLocalContactCode = source.OspLocalContactCode;
    target.OspRoleCode = source.OspRoleCode;
    target.OspEffectiveDate = source.OspEffectiveDate;
    target.OfficeSysGenId = source.OfficeSysGenId;
    target.OfficeName = source.OfficeName;
    target.OffcAddrStreet1 = source.OffcAddrStreet1;
    target.OffcAddrStreet2 = source.OffcAddrStreet2;
    target.OffcAddrCity = source.OffcAddrCity;
    target.OffcAddrState = source.OffcAddrState;
    target.OffcAddrZip = source.OffcAddrZip;
    target.OffcAddrZip4 = source.OffcAddrZip4;
    target.ServProvSysGenId = source.ServProvSysGenId;
    target.ServProvUserId = source.ServProvUserId;
    target.ServProvLastName = source.ServProvLastName;
    target.ServProvFirstName = source.ServProvFirstName;
    target.ServProvMi = source.ServProvMi;
    target.OffcAddrZip3 = source.OffcAddrZip3;
  }

  private void UseSpCreateOutDocRtrnAddr()
  {
    var useImport = new SpCreateOutDocRtrnAddr.Import();
    var useExport = new SpCreateOutDocRtrnAddr.Export();

    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.ServiceProvider.Assign(entities.ServiceProvider);
    useImport.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
    useImport.Current.Date = local.Current.Date;

    Call(SpCreateOutDocRtrnAddr.Execute, useImport, useExport);

    MoveOutDocRtrnAddr(useExport.OutDocRtrnAddr, export.OutDocRtrnAddr);
  }

  private void UseSpReadOutDocRtrnAddr()
  {
    var useImport = new SpReadOutDocRtrnAddr.Import();
    var useExport = new SpReadOutDocRtrnAddr.Export();

    Call(SpReadOutDocRtrnAddr.Execute, useImport, useExport);

    MoveOutDocRtrnAddr(useExport.OutDocRtrnAddr, export.OutDocRtrnAddr);
  }

  private bool ReadAdministrativeAppealAssignment()
  {
    entities.AdministrativeAppealAssignment.Populated = false;

    return Read("ReadAdministrativeAppealAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", export.SpDocKey.KeyAdminAppeal);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppealAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.AdministrativeAppealAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.AdministrativeAppealAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.AdministrativeAppealAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.AdministrativeAppealAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.AdministrativeAppealAssignment.SpdId = db.GetInt32(reader, 5);
        entities.AdministrativeAppealAssignment.OffId = db.GetInt32(reader, 6);
        entities.AdministrativeAppealAssignment.OspCode =
          db.GetString(reader, 7);
        entities.AdministrativeAppealAssignment.OspDate = db.GetDate(reader, 8);
        entities.AdministrativeAppealAssignment.AapId = db.GetInt32(reader, 9);
        entities.AdministrativeAppealAssignment.Populated = true;
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
        db.
          SetInt32(command, "laDetailNo", export.SpDocKey.KeyLegalActionDetail);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase10()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase10",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", export.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase11()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase11",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase12()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase12",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase4()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase4",
      (db, command) =>
      {
        db.SetInt64(command, "cswIdentifier", export.SpDocKey.KeyWorksheet);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase5()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase5",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase6()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase6",
      (db, command) =>
      {
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase7()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase7",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase8()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase8",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase9()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase9",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", export.SpDocKey.KeyGeneticTest);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment1()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment1",
      (db, command) =>
      {
        db.SetString(command, "casNo", export.SpDocKey.KeyCase);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseAssignment2()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment2",
      (db, command) =>
      {
        db.SetString(command, "casNo", export.SpDocKey.KeyCase);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCaseCaseAssignmentOfficeServiceProviderServiceProvider()
  {
    entities.Case1.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach(
      "ReadCaseCaseAssignmentOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 3);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 4);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 8);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 9);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OspCode = db.GetString(reader, 10);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 10);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 11);
        entities.OfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 12);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 13);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 17);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 18);
        entities.ServiceProvider.UserId = db.GetString(reader, 19);
        entities.ServiceProvider.LastName = db.GetString(reader, 20);
        entities.ServiceProvider.FirstName = db.GetString(reader, 21);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 22);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 23);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 24);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 25);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 26);
        entities.Office.TypeCode = db.GetString(reader, 27);
        entities.Office.Name = db.GetString(reader, 28);
        entities.Office.EffectiveDate = db.GetDate(reader, 29);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 30);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 31);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 32);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 33);
        entities.Case1.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
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
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCsePersonAccount1()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(command, "type", export.SpDocKey.KeyPersonAccount);
        db.SetString(command, "cspNumber", export.SpDocKey.KeyPerson);
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
          SetInt32(command, "recaptureRuleId", export.SpDocKey.KeyRecaptureRule);
          
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
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

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "cspINumber", export.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 1);
        entities.IncomeSource.CreatedBy = db.GetString(reader, 2);
        entities.IncomeSource.CspINumber = db.GetString(reader, 3);
        entities.IncomeSource.WorkerId = db.GetNullableString(reader, 4);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private bool ReadLegalActionObligation1()
  {
    entities.Obligation.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionObligation1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.Obligation.CspNumber = db.GetString(reader, 5);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.Obligation.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadLegalActionObligation2()
  {
    entities.Obligation.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionObligation2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.Obligation.CspNumber = db.GetString(reader, 5);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.Obligation.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadLegalReferralAssignment()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", export.SpDocKey.KeyLegalReferral);
        db.SetString(command, "casNo", export.SpDocKey.KeyCase);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 6);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 7);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 8);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 9);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 10);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private bool ReadObligationAdministrativeAction()
  {
    entities.ObligationAdministrativeAction.Populated = false;

    return Read("ReadObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetDate(
          command, "takenDt",
          export.SpDocKey.KeyObligationAdminAction.GetValueOrDefault());
        db.SetString(command, "aatType", export.SpDocKey.KeyAdminAction);
        db.SetInt32(command, "obgGeneratedId", export.SpDocKey.KeyObligation);
        db.SetInt32(command, "otyType", export.SpDocKey.KeyObligationType);
        db.SetString(command, "cpaType", export.SpDocKey.KeyPersonAccount);
        db.SetString(command, "cspNumber", export.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 0);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 1);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 4);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 5);
        entities.ObligationAdministrativeAction.CreatedBy =
          db.GetString(reader, 6);
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
      });
  }

  private bool ReadObligationAssignment1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNo", entities.CsePersonAccount.CspNumber);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "obgId", export.SpDocKey.KeyObligation);
        db.SetInt32(command, "otyId", export.SpDocKey.KeyObligationType);
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 6);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 7);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 8);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 10);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 11);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private bool ReadObligationAssignment2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetString(command, "reasonCode", local.ReasonCode.Text3);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 6);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 7);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 8);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 10);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 11);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
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
        entities.ObligationType.Classification = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction",
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
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadOffice1()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.OutDocRtrnAddr.OfficeSysGenId);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 2);
        entities.Office.TypeCode = db.GetString(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.EffectiveDate = db.GetDate(reader, 5);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice2",
      null,
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 2);
        entities.Office.TypeCode = db.GetString(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.EffectiveDate = db.GetDate(reader, 5);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOffice3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice3",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 2);
        entities.Office.TypeCode = db.GetString(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.EffectiveDate = db.GetDate(reader, 5);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 7);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeAddress1()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress1",
      (db, command) =>
      {
        db.SetString(command, "type", local.OfficeAddress.Type1);
        db.SetInt32(
          command, "offGeneratedId", export.OutDocRtrnAddr.OfficeSysGenId);
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.PostalCode = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Country = db.GetString(reader, 9);
        entities.OfficeAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeAddress2()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress2",
      (db, command) =>
      {
        db.SetString(command, "type", local.OfficeAddress.Type1);
        db.SetInt32(command, "offGeneratedId", local.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.PostalCode = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Country = db.GetString(reader, 9);
        entities.OfficeAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeAddress3()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress3",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetString(command, "type", local.OfficeAddress.Type1);
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.PostalCode = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Country = db.GetString(reader, 9);
        entities.OfficeAddress.Zip3 = db.GetNullableString(reader, 10);
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
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
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
          
        db.SetString(command, "roleCode", export.OutDocRtrnAddr.OspRoleCode);
        db.SetDate(
          command, "effectiveDate",
          export.OutDocRtrnAddr.OspEffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProvider3()
  {
    entities.OfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProvider3",
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
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
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
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 11);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 12);
        entities.Office.TypeCode = db.GetString(reader, 13);
        entities.Office.Name = db.GetString(reader, 14);
        entities.Office.EffectiveDate = db.GetDate(reader, 15);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 16);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 18);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 19);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice1()
  {
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.SpDocKey.KeyAppointment.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.LastName = db.GetString(reader, 12);
        entities.ServiceProvider.FirstName = db.GetString(reader, 13);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 14);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 16);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 23);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 24);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionAssigment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice2",
      (db, command) =>
      {
        db.SetString(
          command, "roleCode", entities.LegalActionAssigment.OspRoleCode ?? ""
          );
        db.SetDate(
          command, "effectiveDate1",
          entities.LegalActionAssigment.OspEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.LegalActionAssigment.OffGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.LegalActionAssigment.SpdGeneratedId.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.LastName = db.GetString(reader, 12);
        entities.ServiceProvider.FirstName = db.GetString(reader, 13);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 14);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 16);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 23);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 24);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice3()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeAppealAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.AdministrativeAppealAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.AdministrativeAppealAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId",
          entities.AdministrativeAppealAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.AdministrativeAppealAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.LastName = db.GetString(reader, 12);
        entities.ServiceProvider.FirstName = db.GetString(reader, 13);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 14);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 16);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 23);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 24);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice4()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.LastName = db.GetString(reader, 12);
        entities.ServiceProvider.FirstName = db.GetString(reader, 13);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 14);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 16);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 23);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 24);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice5()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice5",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.LastName = db.GetString(reader, 12);
        entities.ServiceProvider.FirstName = db.GetString(reader, 13);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 14);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 16);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 23);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 24);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice6()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice6",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", entities.ObligationAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId", entities.ObligationAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.ObligationAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.LastName = db.GetString(reader, 12);
        entities.ServiceProvider.FirstName = db.GetString(reader, 13);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 14);
        entities.ServiceProvider.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 16);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 17);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 18);
        entities.Office.TypeCode = db.GetString(reader, 19);
        entities.Office.Name = db.GetString(reader, 20);
        entities.Office.EffectiveDate = db.GetDate(reader, 21);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 22);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 23);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 24);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 25);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", export.OutDocRtrnAddr.ServProvSysGenId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 5);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 6);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
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
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 5);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 6);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider3()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider3",
      (db, command) =>
      {
        db.SetString(
          command, "userId", entities.ObligationAdministrativeAction.CreatedBy);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 5);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 6);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider4()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider4",
      (db, command) =>
      {
        db.SetString(command, "userId", entities.IncomeSource.WorkerId ?? "");
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 5);
        entities.ServiceProvider.CertificationNumber =
          db.GetNullableString(reader, 6);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetString(command, "type", local.OfficeAddress.Type1);
        db.SetInt32(
          command, "spdGeneratedId", export.OutDocRtrnAddr.ServProvSysGenId);
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
        entities.ServiceProviderAddress.PostalCode =
          db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.ServiceProviderAddress.Country = db.GetString(reader, 9);
        entities.ServiceProviderAddress.Zip3 = db.GetNullableString(reader, 10);
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private SpDocKey spDocKey;
    private Document document;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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

    private OutDocRtrnAddr outDocRtrnAddr;
    private SpDocKey spDocKey;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Case1 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalCertifiedAmount.
      /// </summary>
      [JsonPropertyName("glocalCertifiedAmount")]
      public Common GlocalCertifiedAmount
      {
        get => glocalCertifiedAmount ??= new();
        set => glocalCertifiedAmount = value;
      }

      /// <summary>
      /// A value of GlocalAp.
      /// </summary>
      [JsonPropertyName("glocalAp")]
      public CaseRole GlocalAp
      {
        get => glocalAp ??= new();
        set => glocalAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Case1 g;
      private Common glocalCertifiedAmount;
      private CaseRole glocalAp;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of MaxCertifiedAmount.
    /// </summary>
    [JsonPropertyName("maxCertifiedAmount")]
    public Common MaxCertifiedAmount
    {
      get => maxCertifiedAmount ??= new();
      set => maxCertifiedAmount = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
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
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of ReasonCode.
    /// </summary>
    [JsonPropertyName("reasonCode")]
    public WorkArea ReasonCode
    {
      get => reasonCode ??= new();
      set => reasonCode = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public SpDocLiteral Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of Obl.
    /// </summary>
    [JsonPropertyName("obl")]
    public Common Obl
    {
      get => obl ??= new();
      set => obl = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private Common maxCertifiedAmount;
    private Array<LocalGroup> local1;
    private CaseRole caseRole;
    private CsePersonAccount csePersonAccount;
    private DateWorkArea null1;
    private ChildSupportWorksheet childSupportWorksheet;
    private BatchConvertNumToText batchConvertNumToText;
    private GeneticTest geneticTest;
    private Case1 case1;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private Obligation obligation;
    private AdministrativeAction administrativeAction;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private OfficeAddress officeAddress;
    private WorkArea workArea;
    private Appointment appointment;
    private Common position;
    private DateWorkArea current;
    private WorkArea reasonCode;
    private Document document;
    private SpDocLiteral zdel;
    private Common obl;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of AdminActionCertObligation.
    /// </summary>
    [JsonPropertyName("adminActionCertObligation")]
    public AdminActionCertObligation AdminActionCertObligation
    {
      get => adminActionCertObligation ??= new();
      set => adminActionCertObligation = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of AdmActCertDebtDetail.
    /// </summary>
    [JsonPropertyName("admActCertDebtDetail")]
    public AdmActCertDebtDetail AdmActCertDebtDetail
    {
      get => admActCertDebtDetail ??= new();
      set => admActCertDebtDetail = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of CsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("csePersonSupportWorksheet")]
    public CsePersonSupportWorksheet CsePersonSupportWorksheet
    {
      get => csePersonSupportWorksheet ??= new();
      set => csePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
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
    /// A value of InformationRequestAssignment.
    /// </summary>
    [JsonPropertyName("informationRequestAssignment")]
    public InformationRequestAssignment InformationRequestAssignment
    {
      get => informationRequestAssignment ??= new();
      set => informationRequestAssignment = value;
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

    /// <summary>
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    private LocateRequest locateRequest;
    private Infrastructure infrastructure;
    private AdminActionCertObligation adminActionCertObligation;
    private InterstateRequest interstateRequest;
    private LegalActionDetail legalActionDetail;
    private CaseRole apCaseRole;
    private AdministrativeActCertification administrativeActCertification;
    private AdmActCertDebtDetail admActCertDebtDetail;
    private IncomeSource incomeSource;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private AccrualInstructions accrualInstructions;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private RecaptureRule recaptureRule;
    private CsePersonAccount csePersonAccount;
    private LegalReferral legalReferral;
    private CsePersonSupportWorksheet csePersonSupportWorksheet;
    private ChildSupportWorksheet childSupportWorksheet;
    private CaseRole caseRole;
    private CaseRole absentParent;
    private GeneticTest geneticTest;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private ObligationType obligationType;
    private Obligation obligation;
    private AdministrativeAction administrativeAction;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private Case1 case1;
    private InformationRequestAssignment informationRequestAssignment;
    private InterstateCaseAssignment interstateCaseAssignment;
    private PaReferralAssignment paReferralAssignment;
    private Appointment appointment;
    private AdministrativeAppeal administrativeAppeal;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private OfficeAddress officeAddress;
    private ServiceProviderAddress serviceProviderAddress;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private LegalAction legalAction;
    private ObligationAssignment obligationAssignment;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalActionAssigment legalActionAssigment;
    private CaseAssignment caseAssignment;
    private OutDocRtrnAddr outDocRtrnAddr;
    private LegalActionCaseRole legalActionCaseRole;
    private Collection collection;
    private CsePerson apCsePerson;
  }
#endregion
}
