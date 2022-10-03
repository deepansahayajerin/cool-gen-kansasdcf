// Program: LE_EIWL_DISPLAY_LOG, ID: 1902509041, model: 746.
// Short name: SWE00841
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EIWL_DISPLAY_LOG.
/// </summary>
[Serializable]
public partial class LeEiwlDisplayLog: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EIWL_DISPLAY_LOG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEiwlDisplayLog(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEiwlDisplayLog.
  /// </summary>
  public LeEiwlDisplayLog(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 06/09/15  GVandy	CQ22212		Initial Code.  Created from a copy of INCL.
    // -------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    export.Export1.Index = -1;

    if (!IsEmpty(import.SearchContractor.Code))
    {
      foreach(var item in ReadOffice())
      {
        local.ContractorOffice.Index = entities.Office.SystemGeneratedId - 1;
        local.ContractorOffice.CheckSize();

        local.ContractorOffice.Update.GlocalContractorOffice.Flag = "Y";
      }

      foreach(var item in ReadIwoTransactionLegalActionIncomeSourceCsePerson5())
      {
        local.ContractorOffice.Index = entities.Office.SystemGeneratedId - 1;
        local.ContractorOffice.CheckSize();

        if (AsChar(local.ContractorOffice.Item.GlocalContractorOffice.Flag) == 'Y'
          )
        {
        }
        else
        {
          continue;
        }

        if (!IsEmpty(import.SearchIwoTransaction.CurrentStatus))
        {
          if (AsChar(import.SearchIwoTransaction.CurrentStatus) != AsChar
            (entities.IwoTransaction.CurrentStatus))
          {
            continue;
          }
        }

        local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
        local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        local.IwoAction.Assign(local.Null1);

        if (ReadIwoAction())
        {
          local.IwoAction.Assign(entities.IwoAction);
        }

        if (Equal(local.IwoAction.ActionType, "PRINT"))
        {
          continue;
        }

        if (AsChar(local.IwoAction.SeverityClearedInd) == 'Y')
        {
          // -- The severity has been cleared on these IWO actions.
          local.Severity.Text7 = "DEFAULT";
        }
        else
        {
          switch(AsChar(local.IwoAction.StatusCd))
          {
            case 'S':
              // -- These are submitted eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "DEFAULT";
              }

              break;
            case 'N':
              // -- These are sent eIWO actions.  Set color based on the aging 
              // cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'R':
              // -- These are received eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'E':
              // -- These are errored eIWO actions.
              local.Severity.Text7 = "RED";

              break;
            case 'J':
              // -- These rejected eIWO actions.
              if (Equal(entities.IwoAction.StatusReasonCode, "N") || Equal
                (local.IwoAction.StatusReasonCode, "U"))
              {
                local.Severity.Text7 = "DEFAULT";
              }
              else
              {
                local.Severity.Text7 = "RED";
              }

              break;
            default:
              local.Severity.Text7 = "DEFAULT";

              break;
          }
        }

        if (!IsEmpty(import.SearchSeverity.Text7))
        {
          if (!Equal(import.SearchSeverity.Text7, local.Severity.Text7))
          {
            continue;
          }
        }

        MoveIncomeSource(entities.IncomeSource, local.IncomeSource);

        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          UseCoCabIsUserAssignedToCase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.Authorized.Flag) == 'N')
          {
            local.IncomeSource.Name = "** FAMILY VIOLENCE SET **";
          }
        }

        // -- This eIWO passed all the search criteria move to export group and 
        // increment counts.
        if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Goffice.SystemGeneratedId =
            local.Office.SystemGeneratedId;
          export.Export1.Update.GserviceProvider.UserId =
            local.ServiceProvider.UserId;
          MoveIwoTransaction1(entities.IwoTransaction,
            export.Export1.Update.GiwoTransaction);
          MoveIncomeSource(local.IncomeSource,
            export.Export1.Update.GincomeSource);
          export.Export1.Update.GlegalAction.Assign(entities.LegalAction);
          export.Export1.Update.GiwoAction.Assign(local.IwoAction);
          export.Export1.Update.GexportSeverity.Text7 = local.Severity.Text7;
          export.Export1.Update.GcsePerson.Number = entities.CsePerson.Number;
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
          MoveIwoTransaction2(entities.IwoTransaction, export.NextPage);
        }

        ++export.SeverityTotal.Count;

        switch(TrimEnd(local.Severity.Text7))
        {
          case "DEFAULT":
            ++export.SeverityDefault.Count;

            break;
          case "RED":
            ++export.SeverityRed.Count;

            break;
          case "YELLOW":
            ++export.SeverityYellow.Count;

            break;
          default:
            break;
        }

        if (Equal(global.Command, "DISPLAY"))
        {
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
        }
        else
        {
          // -- We filled the export group and populated the key for the next 
          // page.
          //    We're processing a next or prev command so there is no need to 
          // continue counting records.
          return;
        }
      }
    }
    else if (import.SearchOffice.SystemGeneratedId != 0)
    {
      foreach(var item in ReadIwoTransactionLegalActionIncomeSourceCsePerson3())
      {
        if (!IsEmpty(import.SearchIwoTransaction.CurrentStatus))
        {
          if (AsChar(import.SearchIwoTransaction.CurrentStatus) != AsChar
            (entities.IwoTransaction.CurrentStatus))
          {
            continue;
          }
        }

        local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
        local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        local.IwoAction.Assign(local.Null1);

        if (ReadIwoAction())
        {
          local.IwoAction.Assign(entities.IwoAction);
        }

        if (Equal(local.IwoAction.ActionType, "PRINT"))
        {
          continue;
        }

        if (AsChar(local.IwoAction.SeverityClearedInd) == 'Y')
        {
          // -- The severity has been cleared on these IWO actions.
          local.Severity.Text7 = "DEFAULT";
        }
        else
        {
          switch(AsChar(local.IwoAction.StatusCd))
          {
            case 'S':
              // -- These are submitted eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "DEFAULT";
              }

              break;
            case 'N':
              // -- These are sent eIWO actions.  Set color based on the aging 
              // cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'R':
              // -- These are received eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'E':
              // -- These are errored eIWO actions.
              local.Severity.Text7 = "RED";

              break;
            case 'J':
              // -- These rejected eIWO actions.
              if (Equal(entities.IwoAction.StatusReasonCode, "N") || Equal
                (local.IwoAction.StatusReasonCode, "U"))
              {
                local.Severity.Text7 = "DEFAULT";
              }
              else
              {
                local.Severity.Text7 = "RED";
              }

              break;
            default:
              local.Severity.Text7 = "DEFAULT";

              break;
          }
        }

        if (!IsEmpty(import.SearchSeverity.Text7))
        {
          if (!Equal(import.SearchSeverity.Text7, local.Severity.Text7))
          {
            continue;
          }
        }

        MoveIncomeSource(entities.IncomeSource, local.IncomeSource);

        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          UseCoCabIsUserAssignedToCase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.Authorized.Flag) == 'N')
          {
            local.IncomeSource.Name = "** FAMILY VIOLENCE SET **";
          }
        }

        // -- This eIWO passed all the search criteria move to export group and 
        // increment counts.
        if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Goffice.SystemGeneratedId =
            local.Office.SystemGeneratedId;
          export.Export1.Update.GserviceProvider.UserId =
            local.ServiceProvider.UserId;
          MoveIwoTransaction1(entities.IwoTransaction,
            export.Export1.Update.GiwoTransaction);
          MoveIncomeSource(local.IncomeSource,
            export.Export1.Update.GincomeSource);
          export.Export1.Update.GlegalAction.Assign(entities.LegalAction);
          export.Export1.Update.GiwoAction.Assign(local.IwoAction);
          export.Export1.Update.GexportSeverity.Text7 = local.Severity.Text7;
          export.Export1.Update.GcsePerson.Number = entities.CsePerson.Number;
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
          MoveIwoTransaction2(entities.IwoTransaction, export.NextPage);
        }

        ++export.SeverityTotal.Count;

        switch(TrimEnd(local.Severity.Text7))
        {
          case "DEFAULT":
            ++export.SeverityDefault.Count;

            break;
          case "RED":
            ++export.SeverityRed.Count;

            break;
          case "YELLOW":
            ++export.SeverityYellow.Count;

            break;
          default:
            break;
        }

        if (Equal(global.Command, "DISPLAY"))
        {
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
        }
        else
        {
          // -- We filled the export group and populated the key for the next 
          // page.
          //    We're processing a next or prev command so there is no need to 
          // continue counting records.
          return;
        }
      }
    }
    else if (!IsEmpty(import.SearchServiceProvider.UserId))
    {
      foreach(var item in ReadIwoTransactionLegalActionIncomeSourceCsePerson4())
      {
        if (!IsEmpty(import.SearchIwoTransaction.CurrentStatus))
        {
          if (AsChar(import.SearchIwoTransaction.CurrentStatus) != AsChar
            (entities.IwoTransaction.CurrentStatus))
          {
            continue;
          }
        }

        local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
        local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        local.IwoAction.Assign(local.Null1);

        if (ReadIwoAction())
        {
          local.IwoAction.Assign(entities.IwoAction);
        }

        if (Equal(local.IwoAction.ActionType, "PRINT"))
        {
          continue;
        }

        if (AsChar(local.IwoAction.SeverityClearedInd) == 'Y')
        {
          // -- The severity has been cleared on these IWO actions.
          local.Severity.Text7 = "DEFAULT";
        }
        else
        {
          switch(AsChar(local.IwoAction.StatusCd))
          {
            case 'S':
              // -- These are submitted eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "DEFAULT";
              }

              break;
            case 'N':
              // -- These are sent eIWO actions.  Set color based on the aging 
              // cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'R':
              // -- These are received eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'E':
              // -- These are errored eIWO actions.
              local.Severity.Text7 = "RED";

              break;
            case 'J':
              // -- These rejected eIWO actions.
              if (Equal(entities.IwoAction.StatusReasonCode, "N") || Equal
                (local.IwoAction.StatusReasonCode, "U"))
              {
                local.Severity.Text7 = "DEFAULT";
              }
              else
              {
                local.Severity.Text7 = "RED";
              }

              break;
            default:
              local.Severity.Text7 = "DEFAULT";

              break;
          }
        }

        if (!IsEmpty(import.SearchSeverity.Text7))
        {
          if (!Equal(import.SearchSeverity.Text7, local.Severity.Text7))
          {
            continue;
          }
        }

        MoveIncomeSource(entities.IncomeSource, local.IncomeSource);

        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          UseCoCabIsUserAssignedToCase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.Authorized.Flag) == 'N')
          {
            local.IncomeSource.Name = "** FAMILY VIOLENCE SET **";
          }
        }

        // -- This eIWO passed all the search criteria move to export group and 
        // increment counts.
        if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Goffice.SystemGeneratedId =
            local.Office.SystemGeneratedId;
          export.Export1.Update.GserviceProvider.UserId =
            local.ServiceProvider.UserId;
          MoveIwoTransaction1(entities.IwoTransaction,
            export.Export1.Update.GiwoTransaction);
          MoveIncomeSource(local.IncomeSource,
            export.Export1.Update.GincomeSource);
          export.Export1.Update.GlegalAction.Assign(entities.LegalAction);
          export.Export1.Update.GiwoAction.Assign(local.IwoAction);
          export.Export1.Update.GexportSeverity.Text7 = local.Severity.Text7;
          export.Export1.Update.GcsePerson.Number = entities.CsePerson.Number;
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
          MoveIwoTransaction2(entities.IwoTransaction, export.NextPage);
        }

        ++export.SeverityTotal.Count;

        switch(TrimEnd(local.Severity.Text7))
        {
          case "DEFAULT":
            ++export.SeverityDefault.Count;

            break;
          case "RED":
            ++export.SeverityRed.Count;

            break;
          case "YELLOW":
            ++export.SeverityYellow.Count;

            break;
          default:
            break;
        }

        if (Equal(global.Command, "DISPLAY"))
        {
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
        }
        else
        {
          // -- We filled the export group and populated the key for the next 
          // page.
          //    We're processing a next or prev command so there is no need to 
          // continue counting records.
          return;
        }
      }
    }
    else if (!IsEmpty(import.SearchCsePerson.Number))
    {
      foreach(var item in ReadIwoTransactionLegalActionIncomeSourceCsePerson1())
      {
        if (!IsEmpty(import.SearchIwoTransaction.CurrentStatus))
        {
          if (AsChar(import.SearchIwoTransaction.CurrentStatus) != AsChar
            (entities.IwoTransaction.CurrentStatus))
          {
            continue;
          }
        }

        if (ReadServiceProviderOffice())
        {
          local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
          local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        }
        else
        {
          local.Office.SystemGeneratedId = 0;
          local.ServiceProvider.UserId = "";
          local.Contractor.Code = "";
        }

        local.IwoAction.Assign(local.Null1);

        if (ReadIwoAction())
        {
          local.IwoAction.Assign(entities.IwoAction);
        }

        if (Equal(local.IwoAction.ActionType, "PRINT"))
        {
          continue;
        }

        if (AsChar(local.IwoAction.SeverityClearedInd) == 'Y')
        {
          // -- The severity has been cleared on these IWO actions.
          local.Severity.Text7 = "DEFAULT";
        }
        else
        {
          switch(AsChar(local.IwoAction.StatusCd))
          {
            case 'S':
              // -- These are submitted eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "DEFAULT";
              }

              break;
            case 'N':
              // -- These are sent eIWO actions.  Set color based on the aging 
              // cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'R':
              // -- These are received eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'E':
              // -- These are errored eIWO actions.
              local.Severity.Text7 = "RED";

              break;
            case 'J':
              // -- These rejected eIWO actions.
              if (Equal(entities.IwoAction.StatusReasonCode, "N") || Equal
                (local.IwoAction.StatusReasonCode, "U"))
              {
                local.Severity.Text7 = "DEFAULT";
              }
              else
              {
                local.Severity.Text7 = "RED";
              }

              break;
            default:
              local.Severity.Text7 = "DEFAULT";

              break;
          }
        }

        if (!IsEmpty(import.SearchSeverity.Text7))
        {
          if (!Equal(import.SearchSeverity.Text7, local.Severity.Text7))
          {
            continue;
          }
        }

        MoveIncomeSource(entities.IncomeSource, local.IncomeSource);

        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          UseCoCabIsUserAssignedToCase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.Authorized.Flag) == 'N')
          {
            local.IncomeSource.Name = "** FAMILY VIOLENCE SET **";
          }
        }

        // -- This eIWO passed all the search criteria move to export group and 
        // increment counts.
        if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Goffice.SystemGeneratedId =
            local.Office.SystemGeneratedId;
          export.Export1.Update.GserviceProvider.UserId =
            local.ServiceProvider.UserId;
          MoveIwoTransaction1(entities.IwoTransaction,
            export.Export1.Update.GiwoTransaction);
          MoveIncomeSource(local.IncomeSource,
            export.Export1.Update.GincomeSource);
          export.Export1.Update.GlegalAction.Assign(entities.LegalAction);
          export.Export1.Update.GiwoAction.Assign(local.IwoAction);
          export.Export1.Update.GexportSeverity.Text7 = local.Severity.Text7;
          export.Export1.Update.GcsePerson.Number = entities.CsePerson.Number;
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
          MoveIwoTransaction2(entities.IwoTransaction, export.NextPage);
        }

        ++export.SeverityTotal.Count;

        switch(TrimEnd(local.Severity.Text7))
        {
          case "DEFAULT":
            ++export.SeverityDefault.Count;

            break;
          case "RED":
            ++export.SeverityRed.Count;

            break;
          case "YELLOW":
            ++export.SeverityYellow.Count;

            break;
          default:
            break;
        }

        if (Equal(global.Command, "DISPLAY"))
        {
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
        }
        else
        {
          // -- We filled the export group and populated the key for the next 
          // page.
          //    We're processing a next or prev command so there is no need to 
          // continue counting records.
          return;
        }
      }
    }
    else if (!IsEmpty(import.SearchIwoTransaction.TransactionNumber))
    {
      foreach(var item in ReadIwoTransactionLegalActionIncomeSourceCsePerson2())
      {
        if (!IsEmpty(import.SearchIwoTransaction.CurrentStatus))
        {
          if (AsChar(import.SearchIwoTransaction.CurrentStatus) != AsChar
            (entities.IwoTransaction.CurrentStatus))
          {
            continue;
          }
        }

        if (ReadServiceProviderOffice())
        {
          local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
          local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        }
        else
        {
          local.Office.SystemGeneratedId = 0;
          local.ServiceProvider.UserId = "";
          local.Contractor.Code = "";
        }

        local.IwoAction.Assign(local.Null1);

        if (ReadIwoAction())
        {
          local.IwoAction.Assign(entities.IwoAction);
        }

        if (Equal(local.IwoAction.ActionType, "PRINT"))
        {
          continue;
        }

        if (AsChar(local.IwoAction.SeverityClearedInd) == 'Y')
        {
          // -- The severity has been cleared on these IWO actions.
          local.Severity.Text7 = "DEFAULT";
        }
        else
        {
          switch(AsChar(local.IwoAction.StatusCd))
          {
            case 'S':
              // -- These are submitted eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "DEFAULT";
              }

              break;
            case 'N':
              // -- These are sent eIWO actions.  Set color based on the aging 
              // cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'R':
              // -- These are received eIWO actions.  Set color based on the 
              // aging cutoff date.
              if (!Lt(import.EiwoAgingCutoffDate.Date,
                local.IwoAction.StatusDate))
              {
                local.Severity.Text7 = "RED";
              }
              else
              {
                local.Severity.Text7 = "YELLOW";
              }

              break;
            case 'E':
              // -- These are errored eIWO actions.
              local.Severity.Text7 = "RED";

              break;
            case 'J':
              // -- These rejected eIWO actions.
              if (Equal(entities.IwoAction.StatusReasonCode, "N") || Equal
                (local.IwoAction.StatusReasonCode, "U"))
              {
                local.Severity.Text7 = "DEFAULT";
              }
              else
              {
                local.Severity.Text7 = "RED";
              }

              break;
            default:
              local.Severity.Text7 = "DEFAULT";

              break;
          }
        }

        if (!IsEmpty(import.SearchSeverity.Text7))
        {
          if (!Equal(import.SearchSeverity.Text7, local.Severity.Text7))
          {
            continue;
          }
        }

        MoveIncomeSource(entities.IncomeSource, local.IncomeSource);

        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          UseCoCabIsUserAssignedToCase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.Authorized.Flag) == 'N')
          {
            local.IncomeSource.Name = "** FAMILY VIOLENCE SET **";
          }
        }

        // -- This eIWO passed all the search criteria move to export group and 
        // increment counts.
        if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Goffice.SystemGeneratedId =
            local.Office.SystemGeneratedId;
          export.Export1.Update.GserviceProvider.UserId =
            local.ServiceProvider.UserId;
          MoveIwoTransaction1(entities.IwoTransaction,
            export.Export1.Update.GiwoTransaction);
          MoveIncomeSource(local.IncomeSource,
            export.Export1.Update.GincomeSource);
          export.Export1.Update.GlegalAction.Assign(entities.LegalAction);
          export.Export1.Update.GiwoAction.Assign(local.IwoAction);
          export.Export1.Update.GexportSeverity.Text7 = local.Severity.Text7;
          export.Export1.Update.GcsePerson.Number = entities.CsePerson.Number;
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
          MoveIwoTransaction2(entities.IwoTransaction, export.NextPage);
        }

        ++export.SeverityTotal.Count;

        switch(TrimEnd(local.Severity.Text7))
        {
          case "DEFAULT":
            ++export.SeverityDefault.Count;

            break;
          case "RED":
            ++export.SeverityRed.Count;

            break;
          case "YELLOW":
            ++export.SeverityYellow.Count;

            break;
          default:
            break;
        }

        if (Equal(global.Command, "DISPLAY"))
        {
        }
        else if (Equal(export.NextPage.StatusDate, local.Null1.StatusDate))
        {
        }
        else
        {
          // -- We filled the export group and populated the key for the next 
          // page.
          //    We're processing a next or prev command so there is no need to 
          // continue counting records.
          return;
        }
      }
    }
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveIwoTransaction1(IwoTransaction source,
    IwoTransaction target)
  {
    target.Identifier = source.Identifier;
    target.TransactionNumber = source.TransactionNumber;
  }

  private static void MoveIwoTransaction2(IwoTransaction source,
    IwoTransaction target)
  {
    target.StatusDate = source.StatusDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCoCabIsUserAssignedToCase()
  {
    var useImport = new CoCabIsUserAssignedToCase.Import();
    var useExport = new CoCabIsUserAssignedToCase.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(CoCabIsUserAssignedToCase.Execute, useImport, useExport);

    local.Authorized.Flag = useExport.OnTheCase.Flag;
  }

  private bool ReadIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 4);
        entities.IwoAction.SeverityClearedInd = db.GetNullableString(reader, 5);
        entities.IwoAction.CspNumber = db.GetString(reader, 6);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 7);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 8);
        entities.IwoAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadIwoTransactionLegalActionIncomeSourceCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.IncomeSource.Populated = false;
    entities.IwoTransaction.Populated = false;

    return ReadEach("ReadIwoTransactionLegalActionIncomeSourceCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate", import.Paging.StatusDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Paging.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "numb", import.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.CsePerson.Number = db.GetString(reader, 6);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 7);
        entities.IncomeSource.CspINumber = db.GetString(reader, 7);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 8);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 8);
        entities.LegalAction.ActionTaken = db.GetString(reader, 9);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.IncomeSource.Name = db.GetNullableString(reader, 12);
        entities.CsePerson.Type1 = db.GetString(reader, 13);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.IncomeSource.Populated = true;
        entities.IwoTransaction.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadIwoTransactionLegalActionIncomeSourceCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.IncomeSource.Populated = false;
    entities.IwoTransaction.Populated = false;

    return ReadEach("ReadIwoTransactionLegalActionIncomeSourceCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "transactionNumber",
          import.SearchIwoTransaction.TransactionNumber ?? "");
        db.SetNullableDate(
          command, "statusDate", import.Paging.StatusDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Paging.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.CsePerson.Number = db.GetString(reader, 6);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 7);
        entities.IncomeSource.CspINumber = db.GetString(reader, 7);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 8);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 8);
        entities.LegalAction.ActionTaken = db.GetString(reader, 9);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.IncomeSource.Name = db.GetNullableString(reader, 12);
        entities.CsePerson.Type1 = db.GetString(reader, 13);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.IncomeSource.Populated = true;
        entities.IwoTransaction.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadIwoTransactionLegalActionIncomeSourceCsePerson3()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.IncomeSource.Populated = false;
    entities.IwoTransaction.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadIwoTransactionLegalActionIncomeSourceCsePerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate", import.Paging.StatusDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Paging.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "officeId", import.SearchOffice.SystemGeneratedId);
        db.SetString(command, "userId", import.SearchServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.CsePerson.Number = db.GetString(reader, 6);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 7);
        entities.IncomeSource.CspINumber = db.GetString(reader, 7);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 8);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 8);
        entities.LegalAction.ActionTaken = db.GetString(reader, 9);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.IncomeSource.Name = db.GetNullableString(reader, 12);
        entities.CsePerson.Type1 = db.GetString(reader, 13);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 14);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.ServiceProvider.UserId = db.GetString(reader, 16);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 17);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 18);
        entities.Office.CogCode = db.GetNullableString(reader, 19);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 20);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.IncomeSource.Populated = true;
        entities.IwoTransaction.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadIwoTransactionLegalActionIncomeSourceCsePerson4()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.IncomeSource.Populated = false;
    entities.IwoTransaction.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadIwoTransactionLegalActionIncomeSourceCsePerson4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate", import.Paging.StatusDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Paging.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", import.SearchServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.CsePerson.Number = db.GetString(reader, 6);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 7);
        entities.IncomeSource.CspINumber = db.GetString(reader, 7);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 8);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 8);
        entities.LegalAction.ActionTaken = db.GetString(reader, 9);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.IncomeSource.Name = db.GetNullableString(reader, 12);
        entities.CsePerson.Type1 = db.GetString(reader, 13);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 14);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.ServiceProvider.UserId = db.GetString(reader, 16);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 17);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 18);
        entities.Office.CogCode = db.GetNullableString(reader, 19);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 20);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.IncomeSource.Populated = true;
        entities.IwoTransaction.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadIwoTransactionLegalActionIncomeSourceCsePerson5()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;
    entities.IncomeSource.Populated = false;
    entities.IwoTransaction.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadIwoTransactionLegalActionIncomeSourceCsePerson5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate", import.Paging.StatusDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.Paging.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.CsePerson.Number = db.GetString(reader, 6);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 7);
        entities.IncomeSource.CspINumber = db.GetString(reader, 7);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 8);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 8);
        entities.LegalAction.ActionTaken = db.GetString(reader, 9);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.IncomeSource.Name = db.GetNullableString(reader, 12);
        entities.CsePerson.Type1 = db.GetString(reader, 13);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 14);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.ServiceProvider.UserId = db.GetString(reader, 16);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 17);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 18);
        entities.Office.CogCode = db.GetNullableString(reader, 19);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 20);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        entities.IncomeSource.Populated = true;
        entities.IwoTransaction.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.Office.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetString(command, "cogChildCode", import.SearchContractor.Code);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 1);
        entities.Office.CogCode = db.GetNullableString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOffice()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 3);
        entities.Office.CogCode = db.GetNullableString(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of Paging.
    /// </summary>
    [JsonPropertyName("paging")]
    public IwoTransaction Paging
    {
      get => paging ??= new();
      set => paging = value;
    }

    /// <summary>
    /// A value of EiwoAgingCutoffDate.
    /// </summary>
    [JsonPropertyName("eiwoAgingCutoffDate")]
    public DateWorkArea EiwoAgingCutoffDate
    {
      get => eiwoAgingCutoffDate ??= new();
      set => eiwoAgingCutoffDate = value;
    }

    /// <summary>
    /// A value of SearchContractor.
    /// </summary>
    [JsonPropertyName("searchContractor")]
    public CseOrganization SearchContractor
    {
      get => searchContractor ??= new();
      set => searchContractor = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchIwoTransaction.
    /// </summary>
    [JsonPropertyName("searchIwoTransaction")]
    public IwoTransaction SearchIwoTransaction
    {
      get => searchIwoTransaction ??= new();
      set => searchIwoTransaction = value;
    }

    /// <summary>
    /// A value of SearchSeverity.
    /// </summary>
    [JsonPropertyName("searchSeverity")]
    public WorkArea SearchSeverity
    {
      get => searchSeverity ??= new();
      set => searchSeverity = value;
    }

    private IwoTransaction paging;
    private DateWorkArea eiwoAgingCutoffDate;
    private CseOrganization searchContractor;
    private Office searchOffice;
    private ServiceProvider searchServiceProvider;
    private CsePerson searchCsePerson;
    private IwoTransaction searchIwoTransaction;
    private WorkArea searchSeverity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

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
      /// A value of GserviceProvider.
      /// </summary>
      [JsonPropertyName("gserviceProvider")]
      public ServiceProvider GserviceProvider
      {
        get => gserviceProvider ??= new();
        set => gserviceProvider = value;
      }

      /// <summary>
      /// A value of GiwoTransaction.
      /// </summary>
      [JsonPropertyName("giwoTransaction")]
      public IwoTransaction GiwoTransaction
      {
        get => giwoTransaction ??= new();
        set => giwoTransaction = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GlegalAction.
      /// </summary>
      [JsonPropertyName("glegalAction")]
      public LegalAction GlegalAction
      {
        get => glegalAction ??= new();
        set => glegalAction = value;
      }

      /// <summary>
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>
      /// A value of GexportSeverity.
      /// </summary>
      [JsonPropertyName("gexportSeverity")]
      public WorkArea GexportSeverity
      {
        get => gexportSeverity ??= new();
        set => gexportSeverity = value;
      }

      /// <summary>
      /// A value of GcsePerson.
      /// </summary>
      [JsonPropertyName("gcsePerson")]
      public CsePerson GcsePerson
      {
        get => gcsePerson ??= new();
        set => gcsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common gcommon;
      private Office goffice;
      private ServiceProvider gserviceProvider;
      private IwoTransaction giwoTransaction;
      private IncomeSource gincomeSource;
      private LegalAction glegalAction;
      private IwoAction giwoAction;
      private WorkArea gexportSeverity;
      private CsePerson gcsePerson;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of SeverityRed.
    /// </summary>
    [JsonPropertyName("severityRed")]
    public Common SeverityRed
    {
      get => severityRed ??= new();
      set => severityRed = value;
    }

    /// <summary>
    /// A value of SeverityYellow.
    /// </summary>
    [JsonPropertyName("severityYellow")]
    public Common SeverityYellow
    {
      get => severityYellow ??= new();
      set => severityYellow = value;
    }

    /// <summary>
    /// A value of SeverityDefault.
    /// </summary>
    [JsonPropertyName("severityDefault")]
    public Common SeverityDefault
    {
      get => severityDefault ??= new();
      set => severityDefault = value;
    }

    /// <summary>
    /// A value of SeverityTotal.
    /// </summary>
    [JsonPropertyName("severityTotal")]
    public Common SeverityTotal
    {
      get => severityTotal ??= new();
      set => severityTotal = value;
    }

    /// <summary>
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public IwoTransaction NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
    }

    private Array<ExportGroup> export1;
    private Common severityRed;
    private Common severityYellow;
    private Common severityDefault;
    private Common severityTotal;
    private IwoTransaction nextPage;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ContractorOfficeGroup group.</summary>
    [Serializable]
    public class ContractorOfficeGroup
    {
      /// <summary>
      /// A value of GlocalContractorOffice.
      /// </summary>
      [JsonPropertyName("glocalContractorOffice")]
      public Common GlocalContractorOffice
      {
        get => glocalContractorOffice ??= new();
        set => glocalContractorOffice = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private Common glocalContractorOffice;
    }

    /// <summary>
    /// Gets a value of ContractorOffice.
    /// </summary>
    [JsonIgnore]
    public Array<ContractorOfficeGroup> ContractorOffice =>
      contractorOffice ??= new(ContractorOfficeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ContractorOffice for json serialization.
    /// </summary>
    [JsonPropertyName("contractorOffice")]
    [Computed]
    public IList<ContractorOfficeGroup> ContractorOffice_Json
    {
      get => contractorOffice;
      set => ContractorOffice.Assign(value);
    }

    /// <summary>
    /// A value of JudicialDistrict.
    /// </summary>
    [JsonPropertyName("judicialDistrict")]
    public CseOrganizationRelationship JudicialDistrict
    {
      get => judicialDistrict ??= new();
      set => judicialDistrict = value;
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
    /// A value of Authorized.
    /// </summary>
    [JsonPropertyName("authorized")]
    public Common Authorized
    {
      get => authorized ??= new();
      set => authorized = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public IwoAction Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of Severity.
    /// </summary>
    [JsonPropertyName("severity")]
    public WorkArea Severity
    {
      get => severity ??= new();
      set => severity = value;
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

    private Array<ContractorOfficeGroup> contractorOffice;
    private CseOrganizationRelationship judicialDistrict;
    private IncomeSource incomeSource;
    private Common authorized;
    private CseOrganization contractor;
    private ServiceProvider serviceProvider;
    private Office office;
    private IwoAction null1;
    private IwoAction iwoAction;
    private WorkArea severity;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of JudicalDistrict.
    /// </summary>
    [JsonPropertyName("judicalDistrict")]
    public CseOrganizationRelationship JudicalDistrict
    {
      get => judicalDistrict ??= new();
      set => judicalDistrict = value;
    }

    /// <summary>
    /// A value of JudicialDistrict.
    /// </summary>
    [JsonPropertyName("judicialDistrict")]
    public CseOrganization JudicialDistrict
    {
      get => judicialDistrict ??= new();
      set => judicialDistrict = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of ContractorCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("contractorCseOrganizationRelationship")]
    public CseOrganizationRelationship ContractorCseOrganizationRelationship
    {
      get => contractorCseOrganizationRelationship ??= new();
      set => contractorCseOrganizationRelationship = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
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
    /// A value of ContractorCseOrganization.
    /// </summary>
    [JsonPropertyName("contractorCseOrganization")]
    public CseOrganization ContractorCseOrganization
    {
      get => contractorCseOrganization ??= new();
      set => contractorCseOrganization = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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

    private CseOrganizationRelationship judicalDistrict;
    private CseOrganization judicialDistrict;
    private IwoAction iwoAction;
    private CseOrganizationRelationship contractorCseOrganizationRelationship;
    private OfficeServiceProvider officeServiceProvider;
    private LegalActionAssigment legalActionAssigment;
    private CseOrganization county;
    private CsePerson csePerson;
    private CseOrganization contractorCseOrganization;
    private LegalAction legalAction;
    private IncomeSource incomeSource;
    private IwoTransaction iwoTransaction;
    private ServiceProvider serviceProvider;
    private Office office;
  }
#endregion
}
