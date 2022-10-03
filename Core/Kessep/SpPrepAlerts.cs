// Program: SP_PREP_ALERTS, ID: 374589423, model: 746.
// Short name: SWE02130
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PREP_ALERTS.
/// </summary>
[Serializable]
public partial class SpPrepAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PREP_ALERTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrepAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrepAlerts.
  /// </summary>
  public SpPrepAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER   REQUEST#	DESCRIPTION
    // 04/15/10  GVandy	CQ 966	Initial Development of this cab.  Logic was 
    // broken out from
    // 				sp_process_infrastructure cab.
    // 			CQ970,
    // 			CQ7095,
    // 			CQ11909	Correct logic implementing case role restrictions.
    // 10/28/10  GVandy	CQ109	Don't create alerts on closed cases.
    // 01/17/13  GVandy	CQ33617	Implement alert distribution to a single office
    // 				service provider entered on DRLM.
    // ------------------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    //  Process Alerts
    // ------------------------------------------------------------------------
    // 10/28/10 GVandy  CQ109  Don't create alerts on closed cases.
    if (!IsEmpty(import.Infrastructure.CaseNumber))
    {
      if (ReadCase())
      {
        return;
      }
      else
      {
        // Continue
      }
    }

    foreach(var item in ReadAlertDistributionRuleAlert())
    {
      local.OfficeServiceProviderAlert.DistributionDate = import.Current.Date;
      local.OfficeServiceProviderAlert.Description = entities.Alert.Description;
      local.OfficeServiceProviderAlert.Message = entities.Alert.Message;
      local.OfficeServiceProviderAlert.OptimizedFlag = "0";
      local.OfficeServiceProviderAlert.OptimizationInd =
        entities.AlertDistributionRule.OptimizationInd;
      local.OfficeServiceProviderAlert.TypeCode = "AUT";
      local.OfficeServiceProviderAlert.PrioritizationCode =
        entities.AlertDistributionRule.PrioritizationCode;
      local.OfficeServiceProviderAlert.SituationIdentifier =
        NumberToString(import.Infrastructure.SituationNumber, 9);

      // ------------------------------------------------------------------------
      // Cross Ref ADR  Role and Infr info
      // ------------------------------------------------------------------------
      if (!IsEmpty(entities.AlertDistributionRule.CaseRoleCode) || !
        IsEmpty(entities.AlertDistributionRule.CsePersonAcctCode) || !
        IsEmpty(entities.AlertDistributionRule.CsenetRoleCode) || !
        IsEmpty(entities.AlertDistributionRule.LaPersonCode))
      {
        if (IsEmpty(import.Infrastructure.CsePersonNumber))
        {
          continue;
        }
        else
        {
          // ------------------------------------------------------------------------
          // We have determined that Person is populated, now see
          // which ADR.Role is selected.
          // ADR Role             Need
          // ------------------------------------------------------
          // Case Role            Case
          // CSE Person Acct      Admin Appeal
          //                      Oblig Admin Action
          //                      Obligation
          // CSEnet Role Code
          // LA Person            Legal Action
          // ------------------------------------------------------------------------
          if (!IsEmpty(entities.AlertDistributionRule.CaseRoleCode) && !
            IsEmpty(import.Infrastructure.CaseNumber))
          {
            // 04/15/10  GVandy  CQ970, CQ7095, CQ11909  Correct logic 
            // implementing case role restrictions.
            if (!ReadCaseRole())
            {
              // -- The person does not have an active case role of the type 
              // specified on the alert distribution rule.  Skip this alert
              // distribution rule.
              continue;
            }
          }
          else if (!IsEmpty(entities.AlertDistributionRule.CsePersonAcctCode))
          {
            local.CsePersonAccount.Type1 =
              entities.AlertDistributionRule.CsePersonAcctCode ?? Spaces(1);

            if (Equal(import.Infrastructure.BusinessObjectCd, "ADA"))
            {
              local.AdministrativeAppeal.Identifier =
                (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

              if (!ReadAdministrativeAppeal())
              {
                continue;
              }
            }
            else if (Equal(import.Infrastructure.BusinessObjectCd, "OAA"))
            {
              if (!ReadObligationAdministrativeAction())
              {
                continue;
              }
            }
            else if (Equal(import.Infrastructure.BusinessObjectCd, "OBL"))
            {
              if (!ReadObligation())
              {
                continue;
              }
            }
            else
            {
              continue;
            }
          }
          else if (!IsEmpty(entities.AlertDistributionRule.CsenetRoleCode))
          {
            // -----------------------------
            // This Left blank intensionally
            // -----------------------------
          }
          else if (!IsEmpty(entities.AlertDistributionRule.LaPersonCode) && Equal
            (import.Infrastructure.BusinessObjectCd, "LEA"))
          {
            local.LegalAction.Identifier =
              (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();
            local.LegalActionPerson.Role =
              entities.AlertDistributionRule.LaPersonCode ?? Spaces(1);

            if (!ReadLegalAction())
            {
              continue;
            }
          }
          else
          {
            continue;
          }
        }
      }

      if (!ReadOfficeServiceProviderOfficeServiceProvider1())
      {
        // -- Continue
      }

      if (entities.OfficeServiceProvider.Populated)
      {
        // -- 01/17/13  GVandy CQ33617  Implement alert distribution to a single
        // office
        // -- service provider entered on DRLM.
        if (Equal(entities.ServiceProvider.UserId,
          import.Infrastructure.CreatedBy) && import.Infrastructure.EventId != 47
          && !IsEmpty(import.Infrastructure.CaseNumber))
        {
          // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the service 
          // provider who
          // initiated the alert is also the service provider to whom it would 
          // be assigned.
          continue;
        }

        export.ImportExportAlerts.Index = export.ImportExportAlerts.Count;
        export.ImportExportAlerts.CheckSize();

        MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
          export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
        export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
          RecipientUserId = entities.ServiceProvider.UserId;
        MoveOfficeServiceProvider(entities.OfficeServiceProvider,
          export.ImportExportAlerts.Update.GofficeServiceProvider);
        export.ImportExportAlerts.Update.GserviceProvider.SystemGeneratedId =
          entities.ServiceProvider.SystemGeneratedId;
        export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
      }
      else if (Equal(entities.AlertDistributionRule.BusinessObjectCode, "CAS") &&
        !IsEmpty(import.Infrastructure.CaseNumber))
      {
        // ------------------------------------------------------------------------
        // This is a Catch-all for the non-assignable business object
        // as alerts for them can be distributed using Case or Case
        // Units only. This is for Case.
        // ------------------------------------------------------------------------
        foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider16())
        {
          if (Equal(entities.ServiceProvider.UserId,
            import.Infrastructure.CreatedBy) && import
            .Infrastructure.EventId != 47 && !
            IsEmpty(import.Infrastructure.CaseNumber))
          {
            // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the service 
            // provider who
            // initiated the alert is also the service provider to whom it would
            // be assigned.
            continue;
          }

          export.ImportExportAlerts.Index = export.ImportExportAlerts.Count;
          export.ImportExportAlerts.CheckSize();

          MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
            export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
          export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
            RecipientUserId = entities.ServiceProvider.UserId;
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.ImportExportAlerts.Update.GofficeServiceProvider);
          export.ImportExportAlerts.Update.GserviceProvider.SystemGeneratedId =
            entities.ServiceProvider.SystemGeneratedId;
          export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
            entities.Office.SystemGeneratedId;
        }
      }
      else if (Equal(entities.AlertDistributionRule.BusinessObjectCode, "CAU") &&
        !IsEmpty(import.Infrastructure.CaseNumber) && import
        .Infrastructure.CaseUnitNumber.GetValueOrDefault() != 0)
      {
        // ------------------------------------------------------------------------
        // This is a Catch-all for the non-assignable business object
        // as alerts for them can be distributed using Case or Case
        // Units only. This is for Case Unit.
        // ------------------------------------------------------------------------
        foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider3())
        {
          if (Equal(entities.ServiceProvider.UserId,
            import.Infrastructure.CreatedBy) && import
            .Infrastructure.EventId != 47 && !
            IsEmpty(import.Infrastructure.CaseNumber))
          {
            // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the service 
            // provider who
            // initiated the alert is also the service provider to whom it would
            // be assigned.
            continue;
          }

          export.ImportExportAlerts.Index = export.ImportExportAlerts.Count;
          export.ImportExportAlerts.CheckSize();

          MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
            export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
          export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
            RecipientUserId = entities.ServiceProvider.UserId;
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.ImportExportAlerts.Update.GofficeServiceProvider);
          export.ImportExportAlerts.Update.GserviceProvider.SystemGeneratedId =
            entities.ServiceProvider.SystemGeneratedId;
          export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
            entities.Office.SystemGeneratedId;
        }
      }
      else
      {
        // ------------------------------------------------------------------------
        // All the other assignable business objects are processed here...
        // ------------------------------------------------------------------------
        switch(TrimEnd(import.Infrastructure.BusinessObjectCd))
        {
          case "CAS":
            if (IsEmpty(import.Infrastructure.CaseNumber))
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record must have a case number.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (Case).";

              continue;
            }

            if (Equal(entities.AlertDistributionRule.BusinessObjectCode, "LRF"))
            {
              foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider9())
                
              {
                if (Equal(entities.ServiceProvider.UserId,
                  import.Infrastructure.CreatedBy) && import
                  .Infrastructure.EventId != 47 && !
                  IsEmpty(import.Infrastructure.CaseNumber))
                {
                  // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                  // service provider who
                  // initiated the alert is also the service provider to whom it
                  // would be assigned.
                  continue;
                }

                export.ImportExportAlerts.Index =
                  export.ImportExportAlerts.Count;
                export.ImportExportAlerts.CheckSize();

                MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
                  
                export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                  RecipientUserId = entities.ServiceProvider.UserId;
                MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                  export.ImportExportAlerts.Update.GofficeServiceProvider);
                export.ImportExportAlerts.Update.GserviceProvider.
                  SystemGeneratedId =
                    entities.ServiceProvider.SystemGeneratedId;
                export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                  entities.Office.SystemGeneratedId;
              }
            }
            else
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Cannot process alert distribution rule.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: ADR.BO cannot be derived from I.BO (Case).";
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                entities.AlertDistributionRule.BusinessObjectCode;
              local.SpPrintWorkSet.Text15 =
                NumberToString(import.EventDetail.SystemGeneratedIdentifier, 15);
                
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));
              local.SpPrintWorkSet.Text15 =
                NumberToString(entities.AlertDistributionRule.
                  SystemGeneratedIdentifier, 15);
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));

              continue;
            }

            break;
          case "CAU":
            if (IsEmpty(import.Infrastructure.CaseNumber))
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record must have a case number.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (Case Unit).";

              continue;
            }
            else if (import.Infrastructure.CaseUnitNumber.GetValueOrDefault() ==
              0)
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record must have a case unit number.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (Case Unit).";

              continue;
            }

            if (Equal(entities.AlertDistributionRule.BusinessObjectCode, "LRF"))
            {
              foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider9())
                
              {
                if (Equal(entities.ServiceProvider.UserId,
                  import.Infrastructure.CreatedBy) && import
                  .Infrastructure.EventId != 47 && !
                  IsEmpty(import.Infrastructure.CaseNumber))
                {
                  // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                  // service provider who
                  // initiated the alert is also the service provider to whom it
                  // would be assigned.
                  continue;
                }

                export.ImportExportAlerts.Index =
                  export.ImportExportAlerts.Count;
                export.ImportExportAlerts.CheckSize();

                MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
                  
                export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                  RecipientUserId = entities.ServiceProvider.UserId;
                MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                  export.ImportExportAlerts.Update.GofficeServiceProvider);
                export.ImportExportAlerts.Update.GserviceProvider.
                  SystemGeneratedId =
                    entities.ServiceProvider.SystemGeneratedId;
                export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                  entities.Office.SystemGeneratedId;
              }
            }
            else
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Cannot process alert distribution rule.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: ADR.BO cannot be derived from I.BO (CU).";
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                entities.AlertDistributionRule.BusinessObjectCode;
              local.SpPrintWorkSet.Text15 =
                NumberToString(import.EventDetail.SystemGeneratedIdentifier, 15);
                
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));
              local.SpPrintWorkSet.Text15 =
                NumberToString(entities.AlertDistributionRule.
                  SystemGeneratedIdentifier, 15);
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));

              continue;
            }

            break;
          case "DOC":
            // --------------
            // Beg PR # 80211
            // --------------
            foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider17())
              
            {
              if (Equal(entities.ServiceProvider.UserId,
                import.Infrastructure.CreatedBy) && import
                .Infrastructure.EventId != 47 && !
                IsEmpty(import.Infrastructure.CaseNumber))
              {
                // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                // service provider who
                // initiated the alert is also the service provider to whom it 
                // would be assigned.
                continue;
              }

              export.ImportExportAlerts.Index = export.ImportExportAlerts.Count;
              export.ImportExportAlerts.CheckSize();

              MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
                export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
              export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                RecipientUserId = entities.ServiceProvider.UserId;
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.ImportExportAlerts.Update.GofficeServiceProvider);
              export.ImportExportAlerts.Update.GserviceProvider.
                SystemGeneratedId = entities.ServiceProvider.SystemGeneratedId;
              export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                entities.Office.SystemGeneratedId;

              break;
            }

            if (!entities.OfficeServiceProvider.Populated)
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "Office service provider was not found.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution I.DOC";
            }

            // --------------
            // End PR # 80211
            // --------------
            break;
          case "LRF":
            if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() == 0)
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record is missing business object's value.";
                
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (LRF).";

              continue;
            }

            if (Equal(entities.AlertDistributionRule.BusinessObjectCode, "LRF"))
            {
              local.LegalReferral.Identifier =
                (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

              foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider8())
                
              {
                if (Equal(entities.ServiceProvider.UserId,
                  import.Infrastructure.CreatedBy) && import
                  .Infrastructure.EventId != 47 && !
                  IsEmpty(import.Infrastructure.CaseNumber))
                {
                  // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                  // service provider who
                  // initiated the alert is also the service provider to whom it
                  // would be assigned.
                  continue;
                }

                export.ImportExportAlerts.Index =
                  export.ImportExportAlerts.Count;
                export.ImportExportAlerts.CheckSize();

                MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
                  
                export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                  RecipientUserId = entities.ServiceProvider.UserId;
                MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                  export.ImportExportAlerts.Update.GofficeServiceProvider);
                export.ImportExportAlerts.Update.GserviceProvider.
                  SystemGeneratedId =
                    entities.ServiceProvider.SystemGeneratedId;
                export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                  entities.Office.SystemGeneratedId;
              }
            }
            else
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Cannot process alert distribution rule.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: ADR.BO cannot be derived from I.BO (LRF).";
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                entities.AlertDistributionRule.BusinessObjectCode;
              local.SpPrintWorkSet.Text15 =
                NumberToString(import.EventDetail.SystemGeneratedIdentifier, 15);
                
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));
              local.SpPrintWorkSet.Text15 =
                NumberToString(entities.AlertDistributionRule.
                  SystemGeneratedIdentifier, 15);
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));

              continue;
            }

            break;
          case "ADA":
            if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() == 0)
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record is missing business object's value.";
                
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (ADA).";

              continue;
            }

            switch(TrimEnd(entities.AlertDistributionRule.BusinessObjectCode))
            {
              case "ADA":
                local.AdministrativeAppeal.Identifier =
                  (int)import.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider15())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              case "OAA":
                local.AdministrativeAppeal.Identifier =
                  (int)import.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider11())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              default:
                export.Error.Index = export.Error.Count;
                export.Error.CheckSize();

                export.Error.Update.ProgramError.ProgramError1 =
                  "SP0000: Cannot process alert distribution rule.";
                export.Error.Update.ProgramError.KeyInfo =
                  "Alerts: ADR.BO cannot be derived from I.BO (ADA).";
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                  entities.AlertDistributionRule.BusinessObjectCode;
                local.SpPrintWorkSet.Text15 =
                  NumberToString(import.EventDetail.SystemGeneratedIdentifier,
                  15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));
                local.SpPrintWorkSet.Text15 =
                  NumberToString(entities.AlertDistributionRule.
                    SystemGeneratedIdentifier, 15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));

                continue;
            }

            break;
          case "INR":
            if (Equal(import.Infrastructure.DenormDate,
              local.Initialized.DenormDate))
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record is missing business object's value.";
                
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (INR) CSENET Trans Date.";

              continue;
            }

            // ---------------------------------------------------
            // Test for the precence of the transaction_serial_num
            // ---------------------------------------------------
            if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() == 0)
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record is missing business object's value.";
                
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (INR) Trans Ser Num.";

              continue;
            }

            if (Equal(entities.AlertDistributionRule.BusinessObjectCode, "INR"))
            {
              foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider7())
                
              {
                if (Equal(entities.ServiceProvider.UserId,
                  import.Infrastructure.CreatedBy) && import
                  .Infrastructure.EventId != 47 && !
                  IsEmpty(import.Infrastructure.CaseNumber))
                {
                  // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                  // service provider who
                  // initiated the alert is also the service provider to whom it
                  // would be assigned.
                  continue;
                }

                export.ImportExportAlerts.Index =
                  export.ImportExportAlerts.Count;
                export.ImportExportAlerts.CheckSize();

                MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert);
                  
                export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                  RecipientUserId = entities.ServiceProvider.UserId;
                MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                  export.ImportExportAlerts.Update.GofficeServiceProvider);
                export.ImportExportAlerts.Update.GserviceProvider.
                  SystemGeneratedId =
                    entities.ServiceProvider.SystemGeneratedId;
                export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                  entities.Office.SystemGeneratedId;
              }
            }
            else
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Cannot process alert distribution rule.";
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: ADR.BO cannot be derived from I.BO (INR).";
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                entities.AlertDistributionRule.BusinessObjectCode;
              local.SpPrintWorkSet.Text15 =
                NumberToString(import.EventDetail.SystemGeneratedIdentifier, 15);
                
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));
              local.SpPrintWorkSet.Text15 =
                NumberToString(entities.AlertDistributionRule.
                  SystemGeneratedIdentifier, 15);
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));

              continue;
            }

            break;
          case "LEA":
            if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() == 0)
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "SP0000: Infrastructure record is missing business object's value.";
                
              export.Error.Update.ProgramError.KeyInfo =
                "Alerts: Specific Distribution (LEA).";

              continue;
            }

            switch(TrimEnd(entities.AlertDistributionRule.BusinessObjectCode))
            {
              case "LEA":
                local.LegalAction.Identifier =
                  (int)import.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider14())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              case "OBL":
                local.LegalAction.Identifier =
                  (int)import.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider12())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              default:
                export.Error.Index = export.Error.Count;
                export.Error.CheckSize();

                export.Error.Update.ProgramError.ProgramError1 =
                  "SP0000: Cannot process alert distribution rule.";
                export.Error.Update.ProgramError.KeyInfo =
                  "Alerts: ADR.BO cannot be derived from I.BO (LEA).";
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                  entities.AlertDistributionRule.BusinessObjectCode;
                local.SpPrintWorkSet.Text15 =
                  NumberToString(import.EventDetail.SystemGeneratedIdentifier,
                  15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));
                local.SpPrintWorkSet.Text15 =
                  NumberToString(entities.AlertDistributionRule.
                    SystemGeneratedIdentifier, 15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));

                continue;
            }

            break;
          case "OBL":
            switch(TrimEnd(entities.AlertDistributionRule.BusinessObjectCode))
            {
              case "OBL":
                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider13())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              case "LEA":
                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider4())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              default:
                export.Error.Index = export.Error.Count;
                export.Error.CheckSize();

                export.Error.Update.ProgramError.ProgramError1 =
                  "SP0000: Cannot process alert distribution rule.";
                export.Error.Update.ProgramError.KeyInfo =
                  "Alerts: ADR.BO cannot be derived from I.BO (OBL).";
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                  entities.AlertDistributionRule.BusinessObjectCode;
                local.SpPrintWorkSet.Text15 =
                  NumberToString(import.EventDetail.SystemGeneratedIdentifier,
                  15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));
                local.SpPrintWorkSet.Text15 =
                  NumberToString(entities.AlertDistributionRule.
                    SystemGeneratedIdentifier, 15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));

                continue;
            }

            break;
          case "OAA":
            switch(TrimEnd(entities.AlertDistributionRule.BusinessObjectCode))
            {
              case "OAA":
                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider10())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              case "ADA":
                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider5())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              case "OBL":
                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider6())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              case "LEA":
                foreach(var item1 in ReadOfficeServiceProviderOfficeServiceProvider2())
                  
                {
                  if (Equal(entities.ServiceProvider.UserId,
                    import.Infrastructure.CreatedBy) && import
                    .Infrastructure.EventId != 47 && !
                    IsEmpty(import.Infrastructure.CaseNumber))
                  {
                    // 02/05/2009 GVandy  CQ#8958 Do not create an alert if the 
                    // service provider who
                    // initiated the alert is also the service provider to whom 
                    // it would be assigned.
                    continue;
                  }

                  export.ImportExportAlerts.Index =
                    export.ImportExportAlerts.Count;
                  export.ImportExportAlerts.CheckSize();

                  MoveOfficeServiceProviderAlert(local.
                    OfficeServiceProviderAlert,
                    export.ImportExportAlerts.Update.
                      GofficeServiceProviderAlert);
                  export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
                    RecipientUserId = entities.ServiceProvider.UserId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportAlerts.Update.GofficeServiceProvider);
                  export.ImportExportAlerts.Update.GserviceProvider.
                    SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                  export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                }

                break;
              default:
                export.Error.Index = export.Error.Count;
                export.Error.CheckSize();

                export.Error.Update.ProgramError.ProgramError1 =
                  "SP0000: Cannot process alert distribution rule.";
                export.Error.Update.ProgramError.KeyInfo =
                  "Alerts: ADR.BO cannot be derived from I.BO (OAA).";
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                  entities.AlertDistributionRule.BusinessObjectCode;
                local.SpPrintWorkSet.Text15 =
                  NumberToString(import.EventDetail.SystemGeneratedIdentifier,
                  15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));
                local.SpPrintWorkSet.Text15 =
                  NumberToString(entities.AlertDistributionRule.
                    SystemGeneratedIdentifier, 15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));

                continue;
            }

            break;
          default:
            export.Error.Index = export.Error.Count;
            export.Error.CheckSize();

            export.Error.Update.ProgramError.ProgramError1 =
              "SP0000: Cannot process alert distribution rule.";
            export.Error.Update.ProgramError.KeyInfo =
              "Alerts: This I.BO is not handled explicitly and couldn't process generically.";
              
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " + entities
              .AlertDistributionRule.BusinessObjectCode;
            local.SpPrintWorkSet.Text15 =
              NumberToString(import.EventDetail.SystemGeneratedIdentifier, 15);
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Event Dtl Id " +
              Substring
              (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
              Verify(local.SpPrintWorkSet.Text15, "0"));
            local.SpPrintWorkSet.Text15 =
              NumberToString(entities.AlertDistributionRule.
                SystemGeneratedIdentifier, 15);
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Alrt Dist Rule Id " +
              Substring
              (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
              Verify(local.SpPrintWorkSet.Text15, "0"));

            continue;
        }
      }
    }
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveOfficeServiceProviderAlert(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.OptimizedFlag = source.OptimizedFlag;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", local.AdministrativeAppeal.Identifier);
        db.SetNullableString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(command, "cpaType", local.CsePersonAccount.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.AatType = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAppeal.CpaType = db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 6);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private IEnumerable<bool> ReadAlertDistributionRuleAlert()
  {
    entities.AlertDistributionRule.Populated = false;
    entities.Alert.Populated = false;

    return ReadEach("ReadAlertDistributionRuleAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", import.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.AlertDistributionRule.CaseUnitFunction =
          db.GetString(reader, 2);
        entities.AlertDistributionRule.PrioritizationCode =
          db.GetInt32(reader, 3);
        entities.AlertDistributionRule.OptimizationInd =
          db.GetString(reader, 4);
        entities.AlertDistributionRule.ReasonCode =
          db.GetNullableString(reader, 5);
        entities.AlertDistributionRule.CaseRoleCode =
          db.GetNullableString(reader, 6);
        entities.AlertDistributionRule.CsePersonAcctCode =
          db.GetNullableString(reader, 7);
        entities.AlertDistributionRule.CsenetRoleCode =
          db.GetNullableString(reader, 8);
        entities.AlertDistributionRule.LaPersonCode =
          db.GetNullableString(reader, 9);
        entities.AlertDistributionRule.EffectiveDate = db.GetDate(reader, 10);
        entities.AlertDistributionRule.DiscontinueDate = db.GetDate(reader, 11);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 12);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 13);
        entities.AlertDistributionRule.AleNo = db.GetNullableInt32(reader, 14);
        entities.Alert.ControlNumber = db.GetInt32(reader, 14);
        entities.AlertDistributionRule.OspGeneratedId =
          db.GetNullableInt32(reader, 15);
        entities.AlertDistributionRule.OffGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.AlertDistributionRule.OspRoleCode =
          db.GetNullableString(reader, 17);
        entities.AlertDistributionRule.OspEffectiveDt =
          db.GetNullableDate(reader, 18);
        entities.Alert.Name = db.GetString(reader, 19);
        entities.Alert.Message = db.GetString(reader, 20);
        entities.Alert.Description = db.GetNullableString(reader, 21);
        entities.Alert.ExternalIndicator = db.GetString(reader, 22);
        entities.AlertDistributionRule.Populated = true;
        entities.Alert.Populated = true;

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.ClosedCaseCheck.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ClosedCaseCheck.Number = db.GetString(reader, 0);
        entities.ClosedCaseCheck.Status = db.GetNullableString(reader, 1);
        entities.ClosedCaseCheck.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", import.Infrastructure.CaseNumber ?? "");
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "type", entities.AlertDistributionRule.CaseRoleCode ?? "");
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "role", local.LegalActionPerson.Role);
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "cpaType", local.CsePersonAccount.Type1);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationAdministrativeAction()
  {
    entities.ObligationAdministrativeAction.Populated = false;

    return Read("ReadObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetString(command, "cpaType", local.CsePersonAccount.Type1);
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
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.AlertDistributionRule.Populated);
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.AlertDistributionRule.OspGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.AlertDistributionRule.OffGeneratedId.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.AlertDistributionRule.OspRoleCode ?? ""
          );
        db.SetDate(
          command, "effectiveDate",
          entities.AlertDistributionRule.OspEffectiveDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider10()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider10",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider11()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider11",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "adminAppealId", local.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider12()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider12",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetNullableInt32(command, "lgaId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider13()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider13",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider14()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider14",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetNullableInt32(
          command, "lgaIdentifier", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider15()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider15",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(command, "aapId", local.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider16()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider16",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider17()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider17",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider3()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetString(
          command, "function", entities.AlertDistributionRule.CaseUnitFunction);
          
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "csuNo",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider4()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider5()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider5",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider6()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider6",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider7()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider7",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCode", entities.AlertDistributionRule.ReasonCode ?? ""
          );
        db.SetInt64(
          command, "icsNo",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          import.Infrastructure.DenormDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider8()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider8",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgrId", local.LegalReferral.Identifier);
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider9()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider9",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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

    private Event1 event1;
    private EventDetail eventDetail;
    private Infrastructure infrastructure;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportExportAlertsGroup group.</summary>
    [Serializable]
    public class ImportExportAlertsGroup
    {
      /// <summary>
      /// A value of Goffice.
      /// </summary>
      [JsonPropertyName("goffice")]
      public Office Goffice
      {
        get => goffice ??= new();
        set => goffice = value;
      }

      /// <summary>
      /// A value of GofficeServiceProvider.
      /// </summary>
      [JsonPropertyName("gofficeServiceProvider")]
      public OfficeServiceProvider GofficeServiceProvider
      {
        get => gofficeServiceProvider ??= new();
        set => gofficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GserviceProvider.
      /// </summary>
      [JsonPropertyName("gserviceProvider")]
      public ServiceProvider GserviceProvider
      {
        get => gserviceProvider ??= new();
        set => gserviceProvider = value;
      }

      /// <summary>
      /// A value of GofficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("gofficeServiceProviderAlert")]
      public OfficeServiceProviderAlert GofficeServiceProviderAlert
      {
        get => gofficeServiceProviderAlert ??= new();
        set => gofficeServiceProviderAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Office goffice;
      private OfficeServiceProvider gofficeServiceProvider;
      private ServiceProvider gserviceProvider;
      private OfficeServiceProviderAlert gofficeServiceProviderAlert;
    }

    /// <summary>A ErrorGroup group.</summary>
    [Serializable]
    public class ErrorGroup
    {
      /// <summary>
      /// A value of ProgramError.
      /// </summary>
      [JsonPropertyName("programError")]
      public ProgramError ProgramError
      {
        get => programError ??= new();
        set => programError = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ProgramError programError;
    }

    /// <summary>
    /// Gets a value of ImportExportAlerts.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportAlertsGroup> ImportExportAlerts =>
      importExportAlerts ??= new(ImportExportAlertsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportAlerts for json serialization.
    /// </summary>
    [JsonPropertyName("importExportAlerts")]
    [Computed]
    public IList<ImportExportAlertsGroup> ImportExportAlerts_Json
    {
      get => importExportAlerts;
      set => ImportExportAlerts.Assign(value);
    }

    /// <summary>
    /// Gets a value of Error.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorGroup> Error => error ??= new(ErrorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Error for json serialization.
    /// </summary>
    [JsonPropertyName("error")]
    [Computed]
    public IList<ErrorGroup> Error_Json
    {
      get => error;
      set => Error.Assign(value);
    }

    private Array<ImportExportAlertsGroup> importExportAlerts;
    private Array<ErrorGroup> error;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public Infrastructure Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private CsePersonAccount csePersonAccount;
    private AdministrativeAppeal administrativeAppeal;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private SpPrintWorkSet spPrintWorkSet;
    private LegalReferral legalReferral;
    private Infrastructure initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ClosedCaseCheck.
    /// </summary>
    [JsonPropertyName("closedCaseCheck")]
    public Case1 ClosedCaseCheck
    {
      get => closedCaseCheck ??= new();
      set => closedCaseCheck = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
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
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
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
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of ObligationAdminActionAssgn.
    /// </summary>
    [JsonPropertyName("obligationAdminActionAssgn")]
    public ObligationAdminActionAssgn ObligationAdminActionAssgn
    {
      get => obligationAdminActionAssgn ??= new();
      set => obligationAdminActionAssgn = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public InterstateCase Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private Case1 closedCaseCheck;
    private Event1 event1;
    private Infrastructure infrastructure;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private AlertDistributionRule alertDistributionRule;
    private AdministrativeAppeal administrativeAppeal;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private Alert alert;
    private CaseAssignment caseAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseUnit caseUnit;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private EventDetail eventDetail;
    private PrinterOutputDestination printerOutputDestination;
    private OutgoingDocument outgoingDocument;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private ObligationAdminActionAssgn obligationAdminActionAssgn;
    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase existing;
    private LegalActionAssigment legalActionAssigment;
    private ObligationAssignment obligationAssignment;
  }
#endregion
}
