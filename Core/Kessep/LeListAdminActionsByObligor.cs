// Program: LE_LIST_ADMIN_ACTIONS_BY_OBLIGOR, ID: 372601628, model: 746.
// Short name: SWE00796
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
/// A program: LE_LIST_ADMIN_ACTIONS_BY_OBLIGOR.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads and returns all administrative actions for a given 
/// obligor
/// </para>
/// </summary>
[Serializable]
public partial class LeListAdminActionsByObligor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_ADMIN_ACTIONS_BY_OBLIGOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListAdminActionsByObligor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListAdminActionsByObligor.
  /// </summary>
  public LeListAdminActionsByObligor(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Date	By	IDCR#		Description
    // ??????	??????			Initial code.
    // 103097	govind		Fixed to list the admin action
    // certifications that don't have any obligations/ debt details
    // associated with them. (i.e. decertified admin act certs)
    // 03/23/98	Siraj Konkader		ZDEL cleanup
    // 09/24/98   P. Sharp     If option is spaces have display both
    // the auto and manual. Remove unused local and entity action
    // views.
    // 082599	RJEAN	PR612-13	Change read of
    // Admin Act Cert by Current Amt Date.
    // 010500	RJEAN	PR84255	Bypass any FDSO that have
    // date sent equal zero-date.  Populate FDSO current amount
    // date with date sent so it displays.
    // 041800	PMcElderry	PR #79004
    // Downstream effects related to deletion of SDSO associative
    // entities
    // 042400	PMcElderry	PR #91558.
    // Not all current certified SDSO cases are displaying
    // 032201   Madhu Kumar    PR#112209.
    // Display of manual filter not working correctly .
    // --------------------------------------------------------------
    // 12/12/2002    PPhinney  WR 020119
    // Complete Re-Write due to Table Overflow and Screen Changes
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * * * * *    SET LITERALS
    local.LiteralCred.Type1 = "CRED";
    local.LiteralFdso.Type1 = "FDSO";
    local.LiteralSdso.Type1 = "SDSO";
    local.LiteralKdwp.Type1 = "KDWP";
    local.LiteralKdmv.Type1 = "KDMV";
    local.LiteralNo.Flag = "N";
    local.LiteralYes.Flag = "Y";
    local.LiteralA.Indicator = "A";

    if (!IsEmpty(import.Required.Type1))
    {
      if (ReadAdministrativeAction())
      {
        if (!IsEmpty(import.ListOptManualAutoActs.OneChar))
        {
          if (AsChar(entities.ExistingAdministrativeAction.Indicator) != AsChar
            (import.ListOptManualAutoActs.OneChar))
          {
            ExitState = "LE0000_LIST_NOT_MATCH_TYPE";

            return;
          }
        }
      }
      else
      {
        ExitState = "LE0000_INVALID_ADMIN_ACTION_TYPE";

        return;
      }
    }

    if (!ReadCsePersonAccount())
    {
      if (ReadCsePerson())
      {
        ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

        return;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    local.SdsoAlreadyMoved.Flag = local.LiteralNo.Flag;
    local.FdsoAlreadyMoved.Flag = local.LiteralNo.Flag;
    local.CredAlreadyMoved.Flag = local.LiteralNo.Flag;
    local.KdwpAlreadyMoved.Flag = local.LiteralNo.Flag;
    local.KdmvAlreadyMoved.Flag = local.LiteralNo.Flag;

    if (AsChar(import.SelectAll.Flag) == AsChar(local.LiteralNo.Flag))
    {
      if (Equal(import.Required.Type1, local.LiteralFdso.Type1))
      {
        local.SdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.CredAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdwpAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdmvAlreadyMoved.Flag = local.LiteralYes.Flag;
      }
      else if (Equal(import.Required.Type1, local.LiteralSdso.Type1))
      {
        local.FdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.CredAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdwpAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdmvAlreadyMoved.Flag = local.LiteralYes.Flag;
      }
      else if (Equal(import.Required.Type1, local.LiteralCred.Type1))
      {
        local.SdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.FdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdwpAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdmvAlreadyMoved.Flag = local.LiteralYes.Flag;
      }
      else if (Equal(import.Required.Type1, local.LiteralKdwp.Type1))
      {
        local.SdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.FdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.CredAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdmvAlreadyMoved.Flag = local.LiteralYes.Flag;
      }
      else if (Equal(import.Required.Type1, local.LiteralKdmv.Type1))
      {
        local.FdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.SdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.CredAlreadyMoved.Flag = local.LiteralYes.Flag;
        local.KdwpAlreadyMoved.Flag = local.LiteralYes.Flag;
      }
    }

    local.AutoFlag.Flag = local.LiteralNo.Flag;
    local.ManualFlag.Flag = local.LiteralNo.Flag;
    local.MaxTableSize.Subscript = 80;
    local.AutoAdminActions.Index = -1;
    export.AdminActions.Index = -1;

    if (Equal(import.StartDate.Date, local.BlankDate.Date))
    {
      local.StartDate.Date = Now().Date.AddYears(100);
    }
    else
    {
      local.StartDate.Date = import.StartDate.Date;
    }

    // * * * Process IF (A)utomatic or Both (blank)
    if (AsChar(import.ListOptManualAutoActs.OneChar) == 'A' || IsEmpty
      (import.ListOptManualAutoActs.OneChar))
    {
      foreach(var item in ReadAdministrativeActCertificationAdministrativeAction())
        
      {
        // * * * Is a SPECIFIC administrative_action  type  Requested
        if (!IsEmpty(import.Required.Type1))
        {
          if (!Equal(import.Required.Type1,
            entities.ExistingAdministrativeActCertification.Type1))
          {
            continue;
          }
        }

        // * * * Special SDSO Processing
        if (Equal(entities.ExistingAdministrativeActCertification.Type1,
          local.LiteralSdso.Type1))
        {
          // ****************************************************************
          // *12/22/2006 GVandy Bypass any SDSO certifications that have date 
          // sent
          // * equal zero-date.
          // ****************************************************************
          if (Equal(entities.ExistingAdministrativeActCertification.DateSent,
            local.InitializedToZeros.Date))
          {
            continue;
          }
        }

        // * * * Special FDSO Processing
        if (Equal(entities.ExistingAdministrativeActCertification.Type1,
          local.LiteralFdso.Type1))
        {
          // ****************************************************************
          // *010500	RJEAN	PR84255	Bypass any FDSO that have date sent
          // * equal zero-date.
          // ****************************************************************
          if (Equal(entities.ExistingAdministrativeActCertification.DateSent,
            local.InitializedToZeros.Date))
          {
            continue;
          }

          // * * * Verify Existance of Federal DEbt Setoff
          if (!ReadFederalDebtSetoff())
          {
            ExitState = "FEDERAL_DEBT_SETOFF_NF";

            return;
          }
        }

        if (AsChar(import.SelectAll.Flag) == AsChar(local.LiteralNo.Flag))
        {
          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.LiteralSdso.Type1))
          {
            if (AsChar(local.SdsoAlreadyMoved.Flag) == AsChar
              (local.LiteralYes.Flag))
            {
              continue;
            }
            else
            {
              local.SdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
            }
          }

          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.LiteralFdso.Type1))
          {
            if (AsChar(local.FdsoAlreadyMoved.Flag) == AsChar
              (local.LiteralYes.Flag))
            {
              continue;
            }
            else
            {
              local.FdsoAlreadyMoved.Flag = local.LiteralYes.Flag;
            }
          }

          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.LiteralCred.Type1))
          {
            if (AsChar(local.CredAlreadyMoved.Flag) == AsChar
              (local.LiteralYes.Flag))
            {
              continue;
            }
            else
            {
              local.CredAlreadyMoved.Flag = local.LiteralYes.Flag;
            }
          }

          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.LiteralKdwp.Type1))
          {
            if (AsChar(local.KdwpAlreadyMoved.Flag) == AsChar
              (local.LiteralYes.Flag))
            {
              continue;
            }
            else
            {
              local.KdwpAlreadyMoved.Flag = local.LiteralYes.Flag;
            }
          }

          if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.LiteralKdmv.Type1))
          {
            if (AsChar(local.KdmvAlreadyMoved.Flag) == AsChar
              (local.LiteralYes.Flag))
            {
              continue;
            }
            else
            {
              local.KdmvAlreadyMoved.Flag = local.LiteralYes.Flag;
            }
          }
        }

        // * * *  Are there any OBLIGATIONs on the Action (ie is it Certified 
        // for a Case?)
        local.NoOfOblgAssocWithAact.Count = 0;

        foreach(var item1 in ReadObligation())
        {
          if (ReadObligationType())
          {
            MoveObligationType1(entities.ExistingObligationType, local.Current);

            // * *  WR 020119   COMPLETE  Re-Structure of 
            // le_get_det_for_obligation
            UseLeGetWorkerDetForObligation2();

            // ---------------------------------------------------------------
            // Reset exit state so that if the above call returns error, it is
            // ignored.
            // ---------------------------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            local.Current.Assign(local.InitializedToBlanks);
          }

          // * * * Check for Duplicates
          if (local.AutoAdminActions.Count == 0)
          {
          }
          else
          {
            local.SaveSubscript.Subscript = local.AutoAdminActions.Index + 1;
            local.AutoAdminActions.Index = 0;

            for(var limit = local.AutoAdminActions.Count; local
              .AutoAdminActions.Index < limit; ++local.AutoAdminActions.Index)
            {
              if (!local.AutoAdminActions.CheckSize())
              {
                break;
              }

              if (local.AutoAdminActions.Index >= local.MaxTableSize.Subscript)
              {
                break;
              }

              if (Equal(entities.ExistingAdministrativeAction.Type1,
                local.AutoAdminActions.Item.AutoAdministrativeAction.Type1) && Equal
                (entities.ExistingAdministrativeActCertification.TakenDate,
                local.AutoAdminActions.Item.AutoAdministrativeActCertification.
                  TakenDate))
              {
                local.AutoAdminActions.Index = local.SaveSubscript.Subscript - 1
                  ;
                local.AutoAdminActions.CheckSize();

                goto ReadEach1;
              }
            }

            local.AutoAdminActions.CheckIndex();

            local.AutoAdminActions.Index = local.SaveSubscript.Subscript - 1;
            local.AutoAdminActions.CheckSize();
          }

          ++local.AutoAdminActions.Index;
          local.AutoAdminActions.CheckSize();

          if (local.AutoAdminActions.Index >= local.MaxTableSize.Subscript)
          {
            break;
          }

          local.AutoAdminActions.Update.AutoPassType.Type1 = "";
          local.AutoAdminActions.Update.AutoExmp.Flag = local.LiteralNo.Flag;

          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            local.LiteralSdso.Type1) || Equal
            (entities.ExistingAdministrativeActCertification.Type1,
            local.LiteralCred.Type1) || Equal
            (entities.ExistingAdministrativeActCertification.Type1,
            local.LiteralKdwp.Type1) || Equal
            (entities.ExistingAdministrativeActCertification.Type1,
            local.LiteralKdmv.Type1))
          {
            if (ReadObligationAdmActionExemption())
            {
              local.AutoAdminActions.Update.AutoPassType.Type1 =
                entities.ExistingAdministrativeAction.Type1;
              local.AutoAdminActions.Update.AutoExmp.Flag =
                local.LiteralYes.Flag;
            }
          }
          else if (Equal(entities.ExistingAdministrativeAction.Type1,
            local.LiteralFdso.Type1))
          {
            if (ReadObligationAdmActionExemptionAdministrativeAction())
            {
              local.AutoAdminActions.Update.AutoPassType.Type1 =
                entities.ExemptionAdministrativeAction.Type1;
              local.AutoAdminActions.Update.AutoExmp.Flag =
                local.LiteralYes.Flag;
            }
          }

          ++local.NoOfOblgAssocWithAact.Count;
          local.AutoAdminActions.Update.AutoObligation.
            SystemGeneratedIdentifier =
              entities.ExistingObligation.SystemGeneratedIdentifier;
          local.AutoAdminActions.Update.AutoAdministrativeAction.Type1 =
            entities.ExistingAdministrativeAction.Type1;
          local.AutoAdminActions.Update.AutoCertified.SelectChar =
            local.LiteralYes.Flag;
          local.AutoAdminActions.Update.AutoDecertified.SelectChar =
            local.LiteralNo.Flag;
          local.AutoAdminActions.Update.AutoAdministrativeActCertification.
            Assign(entities.ExistingAdministrativeActCertification);

          // * * * *
          // Round Amounts to Whole Dollars
          // * * * *
          local.Round.TotalInteger =
            (long)(local.AutoAdminActions.Item.
              AutoAdministrativeActCertification.CurrentAmount.
              GetValueOrDefault() + 0.5M);
          local.AutoAdminActions.Update.AutoAdministrativeActCertification.
            CurrentAmount = local.Round.TotalInteger;

          // ****************************************************************
          // *010500	RJEAN	PR84255	Populate FDSO current
          // amount date with date sent so it displays.
          // ****************************************************************
          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            local.LiteralFdso.Type1))
          {
            MoveAdministrativeActCertification2(entities.
              ExistingFederalDebtSetoff,
              local.AutoAdminActions.Update.AutoFederalDebtSetoff);

            if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.AdcAmount))
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.NonAdcAmount =
                0;
            }
            else if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.NonAdcAmount))
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.AdcAmount = 0;
            }

            local.AutoAdminActions.Update.AutoAdministrativeActCertification.
              CurrentAmount =
                local.AutoAdminActions.Item.AutoFederalDebtSetoff.AdcAmount.
                GetValueOrDefault();

            // * * * *
            // For FDSO Only
            // * * * *
            // If Amount > 0 and < 25.00  - -  Set the Amount to 25.00
            // * * * *
            if (local.AutoAdminActions.Item.AutoAdministrativeActCertification.
              CurrentAmount.GetValueOrDefault() > 0 && local
              .AutoAdminActions.Item.AutoAdministrativeActCertification.
                CurrentAmount.GetValueOrDefault() < 25M)
            {
              local.AutoAdminActions.Update.AutoAdministrativeActCertification.
                CurrentAmount = 25M;
            }

            if (local.AutoAdminActions.Item.AutoFederalDebtSetoff.AdcAmount.
              GetValueOrDefault() > 0 && local
              .AutoAdminActions.Item.AutoFederalDebtSetoff.AdcAmount.
                GetValueOrDefault() < 25M)
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.AdcAmount =
                25M;
            }

            if (local.AutoAdminActions.Item.AutoFederalDebtSetoff.NonAdcAmount.
              GetValueOrDefault() > 0 && local
              .AutoAdminActions.Item.AutoFederalDebtSetoff.NonAdcAmount.
                GetValueOrDefault() < 25M)
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.NonAdcAmount =
                25M;
            }

            // * * * *
            // Round Amounts to Whole Dollars
            // * * * *
            local.Round.TotalInteger =
              (long)(local.AutoAdminActions.Item.
                AutoAdministrativeActCertification.CurrentAmount.
                GetValueOrDefault() + 0.5M);
            local.AutoAdminActions.Update.AutoAdministrativeActCertification.
              CurrentAmount = local.Round.TotalInteger;
            local.Round.TotalInteger =
              (long)(local.AutoAdminActions.Item.AutoFederalDebtSetoff.
                AdcAmount.GetValueOrDefault() + 0.5M);
            local.AutoAdminActions.Update.AutoFederalDebtSetoff.AdcAmount =
              local.Round.TotalInteger;
            local.Round.TotalInteger =
              (long)(local.AutoAdminActions.Item.AutoFederalDebtSetoff.
                NonAdcAmount.GetValueOrDefault() + 0.5M);
            local.AutoAdminActions.Update.AutoFederalDebtSetoff.NonAdcAmount =
              local.Round.TotalInteger;

            // * * * *
            // IF Amount = 0 - - FDSO is DeCertified
            // PR  158737    10/03/02
            // NADC must also be 0
            // * * * *
            if (local.AutoAdminActions.Item.AutoAdministrativeActCertification.
              CurrentAmount.GetValueOrDefault() == 0 && local
              .AutoAdminActions.Item.AutoFederalDebtSetoff.NonAdcAmount.
                GetValueOrDefault() == 0)
            {
              local.AutoAdminActions.Update.AutoDecertified.SelectChar =
                local.LiteralYes.Flag;
            }
          }

          MoveObligationType2(local.Current,
            local.AutoAdminActions.Update.AutoObligationType);
          local.AutoAdminActions.Update.AutoCommon.SelectChar = "";

ReadEach1:
          ;
        }

        // * * *  NO OBLIGATIONs on the Action (ie it is NOT Certified for a 
        // Case?)
        if (local.NoOfOblgAssocWithAact.Count == 0)
        {
          // ---------------------------------------------------------------
          // This happens when the admin action is decertified
          // completely. It also happens in converted data. Conversion
          // did not assoc the admin act certifications with obligation and
          // debt detail. In that situation, no obligation and no debt is
          // associated with the admin act certification. Only Admin Act
          // Certification exists. We want to list that admin act cert too.
          // ---------------------------------------------------------------
          // IF this is De-Certified (If NOT CONVERSN) - do not display
          if (!Equal(entities.ExistingAdministrativeActCertification.CreatedBy,
            "CONVERSN"))
          {
            goto Test;
          }

          ++local.AutoAdminActions.Index;
          local.AutoAdminActions.CheckSize();

          if (local.AutoAdminActions.Index >= local.MaxTableSize.Subscript)
          {
            goto Test;
          }

          local.AutoAdminActions.Update.AutoExmp.Flag = local.LiteralNo.Flag;
          export.AdminActions.Update.Pass.Type1 = "";
          local.AutoAdminActions.Update.AutoAdministrativeAction.Type1 =
            entities.ExistingAdministrativeAction.Type1;
          local.AutoAdminActions.Update.AutoAdministrativeActCertification.
            Assign(entities.ExistingAdministrativeActCertification);

          // * * * *
          // Round Amounts to Whole Dollars
          // * * * *
          local.Round.TotalInteger =
            (long)(local.AutoAdminActions.Item.
              AutoAdministrativeActCertification.CurrentAmount.
              GetValueOrDefault() + 0.5M);
          local.AutoAdminActions.Update.AutoAdministrativeActCertification.
            CurrentAmount = local.Round.TotalInteger;
          local.AutoAdminActions.Update.AutoDecertified.SelectChar = "X";

          if (Equal(entities.ExistingAdministrativeActCertification.Type1,
            local.LiteralFdso.Type1))
          {
            local.AutoAdminActions.Update.AutoDecertified.SelectChar =
              local.LiteralYes.Flag;
            MoveAdministrativeActCertification2(entities.
              ExistingFederalDebtSetoff,
              local.AutoAdminActions.Update.AutoFederalDebtSetoff);

            if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.AdcAmount))
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.NonAdcAmount =
                0;
            }
            else if (Equal(entities.ExistingAdministrativeActCertification.
              CurrentAmount, entities.ExistingFederalDebtSetoff.NonAdcAmount))
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.AdcAmount = 0;
            }

            local.AutoAdminActions.Update.AutoAdministrativeActCertification.
              CurrentAmount =
                local.AutoAdminActions.Item.AutoFederalDebtSetoff.AdcAmount.
                GetValueOrDefault();

            // * * * *
            // For FDSO Only
            // * * * *
            // If Amount > 0 and < 25.00  - -  Set the Amount to 25.00
            // * * * *
            if (local.AutoAdminActions.Item.AutoAdministrativeActCertification.
              CurrentAmount.GetValueOrDefault() > 0 && local
              .AutoAdminActions.Item.AutoAdministrativeActCertification.
                CurrentAmount.GetValueOrDefault() < 25M)
            {
              local.AutoAdminActions.Update.AutoAdministrativeActCertification.
                CurrentAmount = 25M;
            }

            if (local.AutoAdminActions.Item.AutoFederalDebtSetoff.AdcAmount.
              GetValueOrDefault() > 0 && local
              .AutoAdminActions.Item.AutoFederalDebtSetoff.AdcAmount.
                GetValueOrDefault() < 25M)
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.AdcAmount =
                25M;
            }

            if (local.AutoAdminActions.Item.AutoFederalDebtSetoff.NonAdcAmount.
              GetValueOrDefault() > 0 && local
              .AutoAdminActions.Item.AutoFederalDebtSetoff.NonAdcAmount.
                GetValueOrDefault() < 25M)
            {
              local.AutoAdminActions.Update.AutoFederalDebtSetoff.NonAdcAmount =
                25M;
            }

            // * * * *
            // Round Amounts to Whole Dollars
            // * * * *
            local.Round.TotalInteger =
              (long)(local.AutoAdminActions.Item.
                AutoAdministrativeActCertification.CurrentAmount.
                GetValueOrDefault() + 0.5M);
            local.AutoAdminActions.Update.AutoAdministrativeActCertification.
              CurrentAmount = local.Round.TotalInteger;
            local.Round.TotalInteger =
              (long)(local.AutoAdminActions.Item.AutoFederalDebtSetoff.
                AdcAmount.GetValueOrDefault() + 0.5M);
            local.AutoAdminActions.Update.AutoFederalDebtSetoff.AdcAmount =
              local.Round.TotalInteger;
            local.Round.TotalInteger =
              (long)(local.AutoAdminActions.Item.AutoFederalDebtSetoff.
                NonAdcAmount.GetValueOrDefault() + 0.5M);
            local.AutoAdminActions.Update.AutoFederalDebtSetoff.NonAdcAmount =
              local.Round.TotalInteger;

            // * * * *
            // IF Amount = 0 - - FDSO is DeCertified
            // PR  158737    10/03/02
            // NADC must also be 0
            // * * * *
            if (local.AutoAdminActions.Item.AutoAdministrativeActCertification.
              CurrentAmount.GetValueOrDefault() == 0 && local
              .AutoAdminActions.Item.AutoFederalDebtSetoff.NonAdcAmount.
                GetValueOrDefault() == 0)
            {
              local.AutoAdminActions.Update.AutoDecertified.SelectChar =
                local.LiteralYes.Flag;
            }
          }

          local.AutoAdminActions.Update.AutoCommon.SelectChar = "";
        }

Test:

        if (local.AutoAdminActions.Index + 1 >= Local
          .AutoAdminActionsGroup.Capacity)
        {
          break;
        }
      }

      if (!local.AutoAdminActions.IsEmpty)
      {
        local.AutoFlag.Flag = local.LiteralYes.Flag;
      }
    }

    local.ManualAdminAction.Index = -1;

    if (AsChar(import.ListOptManualAutoActs.OneChar) == 'M' || IsEmpty
      (import.ListOptManualAutoActs.OneChar) && AsChar
      (import.SelectAll.Flag) == AsChar(local.LiteralYes.Flag))
    {
      foreach(var item in ReadAdministrativeActionObligationAdministrativeAction())
        
      {
        // * * * Check for Duplicates
        if (local.ManualAdminAction.Count == 0)
        {
          // * * * Nothing Processed yet - Proceed
        }
        else
        {
          // * * * Save CURRENT Subscript
          local.SaveSubscript.Subscript = local.ManualAdminAction.Index + 1;

          // * * * Check EACH Occurance for Duplicates
          local.ManualAdminAction.Index = 0;

          for(var limit = local.ManualAdminAction.Count; local
            .ManualAdminAction.Index < limit; ++local.ManualAdminAction.Index)
          {
            if (!local.ManualAdminAction.CheckSize())
            {
              break;
            }

            // * * * If End of Table - EXIT
            if (local.ManualAdminAction.Index >= local.MaxTableSize.Subscript)
            {
              break;
            }

            // * * * If Duplicate found - Restore Subscript and Process NEXT 
            // Actions
            if (Equal(entities.ExistingAdministrativeAction.Type1,
              local.ManualAdminAction.Item.ManualAdministrativeAction.Type1) &&
              Equal
              (entities.ExistingObligationAdministrativeAction.TakenDate,
              local.ManualAdminAction.Item.ManualAdministrativeActCertification.
                TakenDate))
            {
              // ---------------------------------------------------------
              // The row contains the
              // same details. So dont display.
              // ---------------------------------------------------------
              local.ManualAdminAction.Index = local.SaveSubscript.Subscript - 1;
              local.ManualAdminAction.CheckSize();

              goto ReadEach2;
            }
          }

          local.ManualAdminAction.CheckIndex();

          // * * * NO Duplicate found - Restore Current Subscript and Proceed
          local.ManualAdminAction.Index = local.SaveSubscript.Subscript - 1;
          local.ManualAdminAction.CheckSize();
        }

        if (ReadObligationType())
        {
          MoveObligationType1(entities.ExistingObligationType, local.Current);

          if (AsChar(entities.ExistingObligationType.SupportedPersonReqInd) == AsChar
            (local.LiteralNo.Flag))
          {
            // ---------------------------------------------------------------
            // No supported person on this obligation type. So no case
            // associated with this obligation
            // ---------------------------------------------------------------
            goto Read;
          }

          // * *  WR 020119   COMPLETE  Re-Write of le_get_det_for_obligation
          UseLeGetWorkerDetForObligation1();

          // ---------------------------------------------------------------
          // Reset exit state so that if the above call returns error, it is
          // ignored.
          // ---------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          local.Current.Assign(local.InitializedToBlanks);
        }

Read:

        ++local.ManualAdminAction.Index;
        local.ManualAdminAction.CheckSize();

        local.ManualAdminAction.Update.ManualExmp.Flag = local.LiteralNo.Flag;
        local.ManualAdminAction.Update.ManualAdministrativeAction.Type1 =
          entities.ExistingAdministrativeAction.Type1;
        local.ManualAdminAction.Update.ManualObligation.
          SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;
        local.ManualAdminAction.Update.ManualCertified.SelectChar =
          local.LiteralNo.Flag;
        MoveObligationType2(local.Current,
          local.ManualAdminAction.Update.ManualObligationType);
        local.ManualAdminAction.Update.ManualObligationAdministrativeAction.
          TakenDate = entities.ExistingObligationAdministrativeAction.TakenDate;
          
        local.ManualAdminAction.Update.ManualAdministrativeActCertification.
          TakenDate = entities.ExistingObligationAdministrativeAction.TakenDate;
          

        if (local.ManualAdminAction.Index + 1 >= Local
          .ManualAdminActionGroup.Capacity)
        {
          break;
        }

ReadEach2:
        ;
      }

      if (!local.ManualAdminAction.IsEmpty)
      {
        local.ManualFlag.Flag = local.LiteralYes.Flag;
      }
    }

    if (AsChar(local.AutoFlag.Flag) == AsChar(local.LiteralYes.Flag) && AsChar
      (local.ManualFlag.Flag) == AsChar(local.LiteralYes.Flag))
    {
      export.AdminActions.Index = -1;

      local.AutoAdminActions.Index = 0;
      local.AutoAdminActions.CheckSize();

      local.ManualAdminAction.Index = 0;
      local.ManualAdminAction.CheckSize();

      export.AdminActions.Index = 0;

      for(var limit =
        local.AutoAdminActions.Count + local.ManualAdminAction.Count; export
        .AdminActions.Index < limit; ++export.AdminActions.Index)
      {
        if (!export.AdminActions.CheckSize())
        {
          break;
        }

        if (export.AdminActions.Index >= local.MaxTableSize.Subscript)
        {
          return;
        }

        if (Lt(local.ManualAdminAction.Item.
          ManualAdministrativeActCertification.TakenDate,
          local.AutoAdminActions.Item.AutoAdministrativeActCertification.
            TakenDate))
        {
          export.AdminActions.Update.SelectOption.SelectChar =
            local.AutoAdminActions.Item.AutoCommon.SelectChar;
          MoveObligationType2(local.AutoAdminActions.Item.AutoObligationType,
            export.AdminActions.Update.ObligationType);
          export.AdminActions.Update.DetailCertified.SelectChar =
            local.AutoAdminActions.Item.AutoCertified.SelectChar;
          export.AdminActions.Update.DetailObligor.Number =
            local.AutoAdminActions.Item.AutoCsePerson.Number;
          export.AdminActions.Update.DetailObligation.
            SystemGeneratedIdentifier =
              local.AutoAdminActions.Item.AutoObligation.
              SystemGeneratedIdentifier;
          export.AdminActions.Update.DetailAdministrativeAction.Type1 =
            local.AutoAdminActions.Item.AutoAdministrativeAction.Type1;
          export.AdminActions.Update.DetailAdministrativeActCertification.
            Assign(local.AutoAdminActions.Item.
              AutoAdministrativeActCertification);
          MoveAdministrativeActCertification2(local.AutoAdminActions.Item.
            AutoFederalDebtSetoff,
            export.AdminActions.Update.DetailFederalDebtSetoff);
          export.AdminActions.Update.DetailObligationAdministrativeAction.
            TakenDate =
              local.AutoAdminActions.Item.AutoObligationAdministrativeAction.
              TakenDate;
          export.AdminActions.Update.ManAutoSw.Flag =
            local.AutoAdminActions.Item.AutoExmp.Flag;
          export.AdminActions.Update.Pass.Type1 =
            local.AutoAdminActions.Item.AutoPassType.Type1;

          if (AsChar(local.AutoAdminActions.Item.
            AutoAdministrativeActCertification.ChangeSsnInd) == AsChar
            (local.LiteralYes.Flag))
          {
            export.AdminActions.Update.SsnChangeFlag.Flag =
              local.AutoAdminActions.Item.AutoAdministrativeActCertification.
                ChangeSsnInd ?? Spaces(1);
          }
          else
          {
            export.AdminActions.Update.SsnChangeFlag.Flag = "";
          }

          if (AsChar(local.AutoAdminActions.Item.AutoDecertified.SelectChar) ==
            AsChar(local.LiteralYes.Flag))
          {
            export.AdminActions.Update.DecertFlag.Flag = local.LiteralYes.Flag;
          }
          else
          {
            export.AdminActions.Update.DecertFlag.Flag = "";
          }

          ++local.AutoAdminActions.Index;
          local.AutoAdminActions.CheckSize();

          continue;
        }

        if (Lt(local.AutoAdminActions.Item.AutoAdministrativeActCertification.
          TakenDate,
          local.ManualAdminAction.Item.ManualAdministrativeActCertification.
            TakenDate))
        {
          export.AdminActions.Update.SelectOption.SelectChar =
            local.ManualAdminAction.Item.ManualCommon.SelectChar;
          MoveObligationType2(local.ManualAdminAction.Item.ManualObligationType,
            export.AdminActions.Update.ObligationType);
          export.AdminActions.Update.DetailCertified.SelectChar =
            local.ManualAdminAction.Item.ManualCertified.SelectChar;
          export.AdminActions.Update.DetailObligor.Number =
            local.ManualAdminAction.Item.ManualCsePerson.Number;
          export.AdminActions.Update.DetailObligation.
            SystemGeneratedIdentifier =
              local.ManualAdminAction.Item.ManualObligation.
              SystemGeneratedIdentifier;
          export.AdminActions.Update.DetailAdministrativeAction.Type1 =
            local.ManualAdminAction.Item.ManualAdministrativeAction.Type1;
          export.AdminActions.Update.DetailAdministrativeActCertification.
            Assign(local.ManualAdminAction.Item.
              ManualAdministrativeActCertification);
          export.AdminActions.Update.DetailObligationAdministrativeAction.
            TakenDate =
              local.ManualAdminAction.Item.ManualObligationAdministrativeAction.
              TakenDate;
          export.AdminActions.Update.ManAutoSw.Flag =
            local.ManualAdminAction.Item.ManualExmp.Flag;
          export.AdminActions.Update.Pass.Type1 = "";

          if (AsChar(local.ManualAdminAction.Item.
            ManualAdministrativeActCertification.ChangeSsnInd) == AsChar
            (local.LiteralYes.Flag))
          {
            export.AdminActions.Update.SsnChangeFlag.Flag =
              local.ManualAdminAction.Item.ManualAdministrativeActCertification.
                ChangeSsnInd ?? Spaces(1);
          }
          else
          {
            export.AdminActions.Update.SsnChangeFlag.Flag = "";
          }

          export.AdminActions.Update.DecertFlag.Flag = "";

          ++local.ManualAdminAction.Index;
          local.ManualAdminAction.CheckSize();

          continue;
        }

        if (Equal(local.AutoAdminActions.Item.
          AutoAdministrativeActCertification.TakenDate,
          local.ManualAdminAction.Item.ManualAdministrativeActCertification.
            TakenDate))
        {
          // * * *  IF Both tables contain NO DATA - Were done - Quit
          if (Equal(local.AutoAdminActions.Item.
            AutoAdministrativeActCertification.TakenDate,
            local.InitializedToZeros.Date) && IsEmpty
            (local.AutoAdminActions.Item.AutoAdministrativeAction.Type1) && Equal
            (local.ManualAdminAction.Item.ManualAdministrativeActCertification.
              TakenDate, local.InitializedToZeros.Date) && IsEmpty
            (local.ManualAdminAction.Item.ManualAdministrativeAction.Type1))
          {
            return;
          }

          if (Lt(local.ManualAdminAction.Item.ManualAdministrativeAction.Type1,
            local.AutoAdminActions.Item.AutoAdministrativeAction.Type1))
          {
            export.AdminActions.Update.SelectOption.SelectChar =
              local.AutoAdminActions.Item.AutoCommon.SelectChar;
            MoveObligationType2(local.AutoAdminActions.Item.AutoObligationType,
              export.AdminActions.Update.ObligationType);
            export.AdminActions.Update.DetailCertified.SelectChar =
              local.AutoAdminActions.Item.AutoCertified.SelectChar;
            export.AdminActions.Update.DetailObligor.Number =
              local.AutoAdminActions.Item.AutoCsePerson.Number;
            export.AdminActions.Update.DetailObligation.
              SystemGeneratedIdentifier =
                local.AutoAdminActions.Item.AutoObligation.
                SystemGeneratedIdentifier;
            export.AdminActions.Update.DetailAdministrativeAction.Type1 =
              local.AutoAdminActions.Item.AutoAdministrativeAction.Type1;
            export.AdminActions.Update.DetailAdministrativeActCertification.
              Assign(local.AutoAdminActions.Item.
                AutoAdministrativeActCertification);
            MoveAdministrativeActCertification2(local.AutoAdminActions.Item.
              AutoFederalDebtSetoff,
              export.AdminActions.Update.DetailFederalDebtSetoff);
            export.AdminActions.Update.DetailObligationAdministrativeAction.
              TakenDate =
                local.AutoAdminActions.Item.AutoObligationAdministrativeAction.
                TakenDate;
            export.AdminActions.Update.ManAutoSw.Flag =
              local.AutoAdminActions.Item.AutoExmp.Flag;
            export.AdminActions.Update.Pass.Type1 =
              local.AutoAdminActions.Item.AutoPassType.Type1;

            if (AsChar(local.AutoAdminActions.Item.
              AutoAdministrativeActCertification.ChangeSsnInd) == AsChar
              (local.LiteralYes.Flag))
            {
              export.AdminActions.Update.SsnChangeFlag.Flag =
                local.AutoAdminActions.Item.AutoAdministrativeActCertification.
                  ChangeSsnInd ?? Spaces(1);
            }
            else
            {
              export.AdminActions.Update.SsnChangeFlag.Flag = "";
            }

            if (AsChar(local.AutoAdminActions.Item.AutoDecertified.SelectChar) ==
              AsChar(local.LiteralYes.Flag))
            {
              export.AdminActions.Update.DecertFlag.Flag =
                local.LiteralYes.Flag;
            }
            else
            {
              export.AdminActions.Update.DecertFlag.Flag = "";
            }

            ++local.AutoAdminActions.Index;
            local.AutoAdminActions.CheckSize();

            continue;
          }

          if (Lt(local.AutoAdminActions.Item.AutoAdministrativeAction.Type1,
            local.ManualAdminAction.Item.ManualAdministrativeAction.Type1))
          {
            export.AdminActions.Update.SelectOption.SelectChar =
              local.ManualAdminAction.Item.ManualCommon.SelectChar;
            MoveObligationType2(local.ManualAdminAction.Item.
              ManualObligationType, export.AdminActions.Update.ObligationType);
            export.AdminActions.Update.DetailCertified.SelectChar =
              local.ManualAdminAction.Item.ManualCertified.SelectChar;
            export.AdminActions.Update.DetailObligor.Number =
              local.ManualAdminAction.Item.ManualCsePerson.Number;
            export.AdminActions.Update.DetailObligation.
              SystemGeneratedIdentifier =
                local.ManualAdminAction.Item.ManualObligation.
                SystemGeneratedIdentifier;
            export.AdminActions.Update.DetailAdministrativeAction.Type1 =
              local.ManualAdminAction.Item.ManualAdministrativeAction.Type1;
            export.AdminActions.Update.DetailAdministrativeActCertification.
              Assign(local.ManualAdminAction.Item.
                ManualAdministrativeActCertification);
            export.AdminActions.Update.DetailObligationAdministrativeAction.
              TakenDate =
                local.ManualAdminAction.Item.
                ManualObligationAdministrativeAction.TakenDate;
            export.AdminActions.Update.ManAutoSw.Flag =
              local.ManualAdminAction.Item.ManualExmp.Flag;
            export.AdminActions.Update.Pass.Type1 = "";

            if (AsChar(local.ManualAdminAction.Item.
              ManualAdministrativeActCertification.ChangeSsnInd) == AsChar
              (local.LiteralYes.Flag))
            {
              export.AdminActions.Update.SsnChangeFlag.Flag =
                local.ManualAdminAction.Item.
                  ManualAdministrativeActCertification.ChangeSsnInd ?? Spaces
                (1);
            }
            else
            {
              export.AdminActions.Update.SsnChangeFlag.Flag = "";
            }

            export.AdminActions.Update.DecertFlag.Flag = "";

            ++local.ManualAdminAction.Index;
            local.ManualAdminAction.CheckSize();

            continue;
          }
        }
      }

      export.AdminActions.CheckIndex();
    }
    else if (AsChar(local.AutoFlag.Flag) == AsChar(local.LiteralYes.Flag) && AsChar
      (local.ManualFlag.Flag) == AsChar(local.LiteralNo.Flag))
    {
      for(local.AutoAdminActions.Index = 0; local.AutoAdminActions.Index < local
        .AutoAdminActions.Count; ++local.AutoAdminActions.Index)
      {
        if (!local.AutoAdminActions.CheckSize())
        {
          break;
        }

        export.AdminActions.Index = local.AutoAdminActions.Index;
        export.AdminActions.CheckSize();

        export.AdminActions.Update.SelectOption.SelectChar =
          local.AutoAdminActions.Item.AutoCommon.SelectChar;
        MoveObligationType2(local.AutoAdminActions.Item.AutoObligationType,
          export.AdminActions.Update.ObligationType);
        export.AdminActions.Update.DetailCertified.SelectChar =
          local.AutoAdminActions.Item.AutoCertified.SelectChar;
        export.AdminActions.Update.DetailObligor.Number =
          local.AutoAdminActions.Item.AutoCsePerson.Number;
        export.AdminActions.Update.DetailObligation.SystemGeneratedIdentifier =
          local.AutoAdminActions.Item.AutoObligation.SystemGeneratedIdentifier;
        export.AdminActions.Update.DetailAdministrativeAction.Type1 =
          local.AutoAdminActions.Item.AutoAdministrativeAction.Type1;
        export.AdminActions.Update.DetailAdministrativeActCertification.Assign(
          local.AutoAdminActions.Item.AutoAdministrativeActCertification);
        MoveAdministrativeActCertification2(local.AutoAdminActions.Item.
          AutoFederalDebtSetoff,
          export.AdminActions.Update.DetailFederalDebtSetoff);
        export.AdminActions.Update.DetailObligationAdministrativeAction.
          TakenDate =
            local.AutoAdminActions.Item.AutoObligationAdministrativeAction.
            TakenDate;
        export.AdminActions.Update.ManAutoSw.Flag =
          local.AutoAdminActions.Item.AutoExmp.Flag;
        export.AdminActions.Update.Pass.Type1 =
          local.AutoAdminActions.Item.AutoPassType.Type1;

        if (AsChar(local.AutoAdminActions.Item.
          AutoAdministrativeActCertification.ChangeSsnInd) == AsChar
          (local.LiteralYes.Flag))
        {
          export.AdminActions.Update.SsnChangeFlag.Flag =
            local.AutoAdminActions.Item.AutoAdministrativeActCertification.
              ChangeSsnInd ?? Spaces(1);
        }
        else
        {
          export.AdminActions.Update.SsnChangeFlag.Flag = "";
        }

        if (AsChar(local.AutoAdminActions.Item.AutoDecertified.SelectChar) == AsChar
          (local.LiteralYes.Flag))
        {
          export.AdminActions.Update.DecertFlag.Flag = local.LiteralYes.Flag;
        }
        else
        {
          export.AdminActions.Update.DecertFlag.Flag = "";
        }

        if (export.AdminActions.Index + 1 >= local.MaxTableSize.Subscript)
        {
          return;
        }
      }

      local.AutoAdminActions.CheckIndex();
    }
    else if (AsChar(local.AutoFlag.Flag) == AsChar(local.LiteralNo.Flag) && AsChar
      (local.ManualFlag.Flag) == AsChar(local.LiteralYes.Flag))
    {
      for(local.ManualAdminAction.Index = 0; local.ManualAdminAction.Index < local
        .ManualAdminAction.Count; ++local.ManualAdminAction.Index)
      {
        if (!local.ManualAdminAction.CheckSize())
        {
          break;
        }

        export.AdminActions.Index = local.ManualAdminAction.Index;
        export.AdminActions.CheckSize();

        export.AdminActions.Update.SelectOption.SelectChar =
          local.ManualAdminAction.Item.ManualCommon.SelectChar;
        MoveObligationType2(local.ManualAdminAction.Item.ManualObligationType,
          export.AdminActions.Update.ObligationType);
        export.AdminActions.Update.DetailCertified.SelectChar =
          local.ManualAdminAction.Item.ManualCertified.SelectChar;
        export.AdminActions.Update.DetailObligor.Number =
          local.ManualAdminAction.Item.ManualCsePerson.Number;
        export.AdminActions.Update.DetailObligation.SystemGeneratedIdentifier =
          local.ManualAdminAction.Item.ManualObligation.
            SystemGeneratedIdentifier;
        export.AdminActions.Update.DetailAdministrativeAction.Type1 =
          local.ManualAdminAction.Item.ManualAdministrativeAction.Type1;
        export.AdminActions.Update.DetailAdministrativeActCertification.Assign(
          local.ManualAdminAction.Item.ManualAdministrativeActCertification);
        export.AdminActions.Update.DetailObligationAdministrativeAction.
          TakenDate =
            local.ManualAdminAction.Item.ManualObligationAdministrativeAction.
            TakenDate;
        export.AdminActions.Update.ManAutoSw.Flag =
          local.ManualAdminAction.Item.ManualExmp.Flag;
        export.AdminActions.Update.Pass.Type1 = "";

        if (AsChar(local.ManualAdminAction.Item.
          ManualAdministrativeActCertification.ChangeSsnInd) == AsChar
          (local.LiteralYes.Flag))
        {
          export.AdminActions.Update.SsnChangeFlag.Flag =
            local.ManualAdminAction.Item.ManualAdministrativeActCertification.
              ChangeSsnInd ?? Spaces(1);
        }
        else
        {
          export.AdminActions.Update.SsnChangeFlag.Flag = "";
        }

        export.AdminActions.Update.DecertFlag.Flag = "";

        if (export.AdminActions.Index + 1 >= local.MaxTableSize.Subscript)
        {
          return;
        }
      }

      local.ManualAdminAction.CheckIndex();
    }
  }

  private static void MoveAdministrativeActCertification1(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.Type1 = source.Type1;
    target.TakenDate = source.TakenDate;
  }

  private static void MoveAdministrativeActCertification2(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
  }

  private static void MoveGroupToCaseNumber(LeGetWorkerDetForObligation.Export.
    GroupGroup source, Local.CaseNumberGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.InactRoleForSupp.SelectChar = source.InactRoleForSupp.SelectChar;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SupportedPersonReqInd = source.SupportedPersonReqInd;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.Classification = source.Classification;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseLeGetWorkerDetForObligation1()
  {
    var useImport = new LeGetWorkerDetForObligation.Import();
    var useExport = new LeGetWorkerDetForObligation.Export();

    useImport.ObligationType.Assign(entities.ExistingObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingObligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.AsOf.Date = local.StartDate.Date;
    useImport.One.Number = export.One.Number;
    useImport.Two.Number = export.Two.Number;
    useImport.Three.Number = export.Three.Number;
    useImport.Four.Number = export.Four.Number;
    useImport.Five.Number = export.Five.Number;
    useImport.Six.Number = export.Six.Number;
    useImport.Seven.Number = export.Seven.Number;
    useImport.Eight.Number = export.Eight.Number;
    useImport.Nine.Number = export.Nine.Number;
    useImport.Ten.Number = export.Ten.Number;
    useImport.Eleven.Number = export.Eleven.Number;
    useImport.Twelve.Number = export.Twelve.Number;
    MoveAdministrativeActCertification1(entities.
      ExistingAdministrativeActCertification,
      useImport.AdministrativeActCertification);
    useImport.ActiveRoleForSupported.SelectChar =
      local.ActiveRoleForSupported.SelectChar;

    Call(LeGetWorkerDetForObligation.Execute, useImport, useExport);

    export.One.Number = useExport.One.Number;
    export.Two.Number = useExport.Two.Number;
    export.Three.Number = useExport.Three.Number;
    export.Four.Number = useExport.Four.Number;
    export.Five.Number = useExport.Five.Number;
    export.Six.Number = useExport.Six.Number;
    export.Seven.Number = useExport.Seven.Number;
    export.Eight.Number = useExport.Eight.Number;
    export.Nine.Number = useExport.Nine.Number;
    export.Ten.Number = useExport.Ten.Number;
    export.Eleven.Number = useExport.Eleven.Number;
    export.Twelve.Number = useExport.Twelve.Number;
    useExport.Group.CopyTo(local.CaseNumber, MoveGroupToCaseNumber);
    local.ActiveRoleForSupported.SelectChar =
      useExport.ActiveRoleForSupported.SelectChar;
  }

  private void UseLeGetWorkerDetForObligation2()
  {
    var useImport = new LeGetWorkerDetForObligation.Import();
    var useExport = new LeGetWorkerDetForObligation.Export();

    useImport.ObligationType.Assign(entities.ExistingObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingObligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.AsOf.Date = local.StartDate.Date;
    useImport.One.Number = export.One.Number;
    useImport.Two.Number = export.Two.Number;
    useImport.Three.Number = export.Three.Number;
    useImport.Four.Number = export.Four.Number;
    useImport.Five.Number = export.Five.Number;
    useImport.Six.Number = export.Six.Number;
    useImport.Seven.Number = export.Seven.Number;
    useImport.Eight.Number = export.Eight.Number;
    useImport.Nine.Number = export.Nine.Number;
    useImport.Ten.Number = export.Ten.Number;
    useImport.Eleven.Number = export.Eleven.Number;
    useImport.Twelve.Number = export.Twelve.Number;
    MoveAdministrativeActCertification1(entities.
      ExistingAdministrativeActCertification,
      useImport.AdministrativeActCertification);
    useImport.ActiveRoleForSupported.SelectChar =
      local.ActiveRoleForSupported.SelectChar;

    Call(LeGetWorkerDetForObligation.Execute, useImport, useExport);

    export.One.Number = useExport.One.Number;
    export.Two.Number = useExport.Two.Number;
    export.Three.Number = useExport.Three.Number;
    export.Four.Number = useExport.Four.Number;
    export.Five.Number = useExport.Five.Number;
    export.Six.Number = useExport.Six.Number;
    export.Seven.Number = useExport.Seven.Number;
    export.Eight.Number = useExport.Eight.Number;
    export.Nine.Number = useExport.Nine.Number;
    export.Ten.Number = useExport.Ten.Number;
    export.Eleven.Number = useExport.Eleven.Number;
    export.Twelve.Number = useExport.Twelve.Number;
    local.ActiveRoleForSupported.SelectChar =
      useExport.ActiveRoleForSupported.SelectChar;
  }

  private IEnumerable<bool>
    ReadAdministrativeActCertificationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligorCsePersonAccount.Populated);
    entities.ExistingAdministrativeActCertification.Populated = false;
    entities.ExistingAdministrativeAction.Populated = false;

    return ReadEach("ReadAdministrativeActCertificationAdministrativeAction",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.ExistingObligorCsePersonAccount.Type1);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligorCsePersonAccount.CspNumber);
        db.
          SetDate(command, "takenDt", local.StartDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.ExistingAdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeActCertification.Type1 =
          db.GetString(reader, 2);
        entities.ExistingAdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.ExistingAdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 4);
        entities.ExistingAdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingAdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingAdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingAdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingAdministrativeActCertification.CreatedBy =
          db.GetString(reader, 9);
        entities.ExistingAdministrativeActCertification.
          CseCentralOfficeApprovalDate = db.GetNullableDate(reader, 10);
        entities.ExistingAdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 11);
        entities.ExistingAdministrativeActCertification.TanfCode =
          db.GetString(reader, 12);
        entities.ExistingAdministrativeActCertification.ChangeSsnInd =
          db.GetNullableString(reader, 13);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 14);
        entities.ExistingAdministrativeActCertification.Populated = true;
        entities.ExistingAdministrativeAction.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingAdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingAdministrativeActCertification.Type1);

        return true;
      });
  }

  private bool ReadAdministrativeAction()
  {
    entities.ExistingAdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.Required.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadAdministrativeActionObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligorCsePersonAccount.Populated);
    entities.ExistingObligationAdministrativeAction.Populated = false;
    entities.ExistingAdministrativeAction.Populated = false;
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadAdministrativeActionObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.ExistingObligorCsePersonAccount.Type1);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligorCsePersonAccount.CspNumber);
        db.
          SetDate(command, "takenDt", local.StartDate.Date.GetValueOrDefault());
          
        db.SetString(command, "type", import.Required.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingObligationAdministrativeAction.AatType =
          db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 1);
        entities.ExistingObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 4);
        entities.ExistingObligationAdministrativeAction.CpaType =
          db.GetString(reader, 5);
        entities.ExistingObligation.CpaType = db.GetString(reader, 5);
        entities.ExistingObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 6);
        entities.ExistingObligationAdministrativeAction.ResponseDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingObligationAdministrativeAction.Response =
          db.GetNullableString(reader, 8);
        entities.ExistingObligationAdministrativeAction.Populated = true;
        entities.ExistingAdministrativeAction.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingObligorCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingObligorCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.ExistingObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligorCsePersonAccount.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingObligorCsePersonAccount.Type1 =
          db.GetString(reader, 1);
        entities.ExistingObligorCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadFederalDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligorCsePersonAccount.Populated);
    entities.ExistingFederalDebtSetoff.Populated = false;

    return Read("ReadFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.ExistingObligorCsePersonAccount.Type1);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligorCsePersonAccount.CspNumber);
        db.SetNullableString(
          command, "aatType", entities.ExistingAdministrativeAction.Type1);
        db.SetDate(
          command, "takenDt",
          entities.ExistingAdministrativeActCertification.TakenDate.
            GetValueOrDefault());
        db.SetString(
          command, "tanfCode",
          entities.ExistingAdministrativeActCertification.TanfCode);
        db.SetString(
          command, "type",
          entities.ExistingAdministrativeActCertification.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingFederalDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.ExistingFederalDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.ExistingFederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.ExistingFederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.ExistingFederalDebtSetoff.AatType =
          db.GetNullableString(reader, 4);
        entities.ExistingFederalDebtSetoff.AdcAmount =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingFederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingFederalDebtSetoff.TanfCode = db.GetString(reader, 7);
        entities.ExistingFederalDebtSetoff.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.ExistingFederalDebtSetoff.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.ExistingFederalDebtSetoff.Type1);
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligorCsePersonAccount.Populated);
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.ExistingObligorCsePersonAccount.Type1);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligorCsePersonAccount.CspNumber);
        db.SetNullableString(
          command, "aatType", entities.ExistingAdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
      });
  }

  private bool ReadObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExemptionObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetString(
          command, "aatType", entities.ExistingAdministrativeAction.Type1);
        db.SetDate(
          command, "endDt",
          entities.ExistingAdministrativeActCertification.TakenDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExemptionObligationAdmActionExemption.OtyType =
          db.GetInt32(reader, 0);
        entities.ExemptionObligationAdmActionExemption.AatType =
          db.GetString(reader, 1);
        entities.ExemptionObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExemptionObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ExemptionObligationAdmActionExemption.CpaType =
          db.GetString(reader, 4);
        entities.ExemptionObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExemptionObligationAdmActionExemption.EndDate =
          db.GetDate(reader, 6);
        entities.ExemptionObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ExemptionObligationAdmActionExemption.CpaType);
      });
  }

  private bool ReadObligationAdmActionExemptionAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExemptionObligationAdmActionExemption.Populated = false;
    entities.ExemptionAdministrativeAction.Populated = false;

    return Read("ReadObligationAdmActionExemptionAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "indicatr", local.LiteralA.Indicator);
        db.SetString(command, "type1", local.LiteralSdso.Type1);
        db.SetString(command, "type2", local.LiteralCred.Type1);
        db.SetString(command, "type3", local.LiteralKdwp.Type1);
        db.SetString(command, "type4", local.LiteralKdmv.Type1);
        db.SetDate(
          command, "endDt",
          entities.ExistingAdministrativeActCertification.TakenDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExemptionObligationAdmActionExemption.OtyType =
          db.GetInt32(reader, 0);
        entities.ExemptionObligationAdmActionExemption.AatType =
          db.GetString(reader, 1);
        entities.ExemptionAdministrativeAction.Type1 = db.GetString(reader, 1);
        entities.ExemptionObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExemptionObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ExemptionObligationAdmActionExemption.CpaType =
          db.GetString(reader, 4);
        entities.ExemptionObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExemptionObligationAdmActionExemption.EndDate =
          db.GetDate(reader, 6);
        entities.ExemptionAdministrativeAction.Indicator =
          db.GetString(reader, 7);
        entities.ExemptionObligationAdmActionExemption.Populated = true;
        entities.ExemptionAdministrativeAction.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ExemptionObligationAdmActionExemption.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", entities.ExistingObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Name = db.GetString(reader, 2);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 3);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 4);
        entities.ExistingObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
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
    /// A value of SelectAll.
    /// </summary>
    [JsonPropertyName("selectAll")]
    public Common SelectAll
    {
      get => selectAll ??= new();
      set => selectAll = value;
    }

    /// <summary>
    /// A value of ListOptManualAutoActs.
    /// </summary>
    [JsonPropertyName("listOptManualAutoActs")]
    public Standard ListOptManualAutoActs
    {
      get => listOptManualAutoActs ??= new();
      set => listOptManualAutoActs = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
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

    private Common selectAll;
    private Standard listOptManualAutoActs;
    private AdministrativeAction required;
    private DateWorkArea startDate;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AdminActionsGroup group.</summary>
    [Serializable]
    public class AdminActionsGroup
    {
      /// <summary>
      /// A value of SelectOption.
      /// </summary>
      [JsonPropertyName("selectOption")]
      public Common SelectOption
      {
        get => selectOption ??= new();
        set => selectOption = value;
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
      /// A value of DetailCertified.
      /// </summary>
      [JsonPropertyName("detailCertified")]
      public Common DetailCertified
      {
        get => detailCertified ??= new();
        set => detailCertified = value;
      }

      /// <summary>
      /// A value of DetailObligor.
      /// </summary>
      [JsonPropertyName("detailObligor")]
      public CsePerson DetailObligor
      {
        get => detailObligor ??= new();
        set => detailObligor = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of SsnChangeFlag.
      /// </summary>
      [JsonPropertyName("ssnChangeFlag")]
      public Common SsnChangeFlag
      {
        get => ssnChangeFlag ??= new();
        set => ssnChangeFlag = value;
      }

      /// <summary>
      /// A value of DecertFlag.
      /// </summary>
      [JsonPropertyName("decertFlag")]
      public Common DecertFlag
      {
        get => decertFlag ??= new();
        set => decertFlag = value;
      }

      /// <summary>
      /// A value of ManAutoSw.
      /// </summary>
      [JsonPropertyName("manAutoSw")]
      public Common ManAutoSw
      {
        get => manAutoSw ??= new();
        set => manAutoSw = value;
      }

      /// <summary>
      /// A value of SelManOption.
      /// </summary>
      [JsonPropertyName("selManOption")]
      public Common SelManOption
      {
        get => selManOption ??= new();
        set => selManOption = value;
      }

      /// <summary>
      /// A value of Pass.
      /// </summary>
      [JsonPropertyName("pass")]
      public AdministrativeAction Pass
      {
        get => pass ??= new();
        set => pass = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common selectOption;
      private ObligationType obligationType;
      private Common detailCertified;
      private CsePerson detailObligor;
      private Obligation detailObligation;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
      private Common ssnChangeFlag;
      private Common decertFlag;
      private Common manAutoSw;
      private Common selManOption;
      private AdministrativeAction pass;
    }

    /// <summary>
    /// Gets a value of AdminActions.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionsGroup> AdminActions => adminActions ??= new(
      AdminActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AdminActions for json serialization.
    /// </summary>
    [JsonPropertyName("adminActions")]
    [Computed]
    public IList<AdminActionsGroup> AdminActions_Json
    {
      get => adminActions;
      set => AdminActions.Assign(value);
    }

    /// <summary>
    /// A value of One.
    /// </summary>
    [JsonPropertyName("one")]
    public Case1 One
    {
      get => one ??= new();
      set => one = value;
    }

    /// <summary>
    /// A value of Two.
    /// </summary>
    [JsonPropertyName("two")]
    public Case1 Two
    {
      get => two ??= new();
      set => two = value;
    }

    /// <summary>
    /// A value of Three.
    /// </summary>
    [JsonPropertyName("three")]
    public Case1 Three
    {
      get => three ??= new();
      set => three = value;
    }

    /// <summary>
    /// A value of Four.
    /// </summary>
    [JsonPropertyName("four")]
    public Case1 Four
    {
      get => four ??= new();
      set => four = value;
    }

    /// <summary>
    /// A value of Five.
    /// </summary>
    [JsonPropertyName("five")]
    public Case1 Five
    {
      get => five ??= new();
      set => five = value;
    }

    /// <summary>
    /// A value of Six.
    /// </summary>
    [JsonPropertyName("six")]
    public Case1 Six
    {
      get => six ??= new();
      set => six = value;
    }

    /// <summary>
    /// A value of Seven.
    /// </summary>
    [JsonPropertyName("seven")]
    public Case1 Seven
    {
      get => seven ??= new();
      set => seven = value;
    }

    /// <summary>
    /// A value of Eight.
    /// </summary>
    [JsonPropertyName("eight")]
    public Case1 Eight
    {
      get => eight ??= new();
      set => eight = value;
    }

    /// <summary>
    /// A value of Nine.
    /// </summary>
    [JsonPropertyName("nine")]
    public Case1 Nine
    {
      get => nine ??= new();
      set => nine = value;
    }

    /// <summary>
    /// A value of Ten.
    /// </summary>
    [JsonPropertyName("ten")]
    public Case1 Ten
    {
      get => ten ??= new();
      set => ten = value;
    }

    /// <summary>
    /// A value of Eleven.
    /// </summary>
    [JsonPropertyName("eleven")]
    public Case1 Eleven
    {
      get => eleven ??= new();
      set => eleven = value;
    }

    /// <summary>
    /// A value of Twelve.
    /// </summary>
    [JsonPropertyName("twelve")]
    public Case1 Twelve
    {
      get => twelve ??= new();
      set => twelve = value;
    }

    private Array<AdminActionsGroup> adminActions;
    private Case1 one;
    private Case1 two;
    private Case1 three;
    private Case1 four;
    private Case1 five;
    private Case1 six;
    private Case1 seven;
    private Case1 eight;
    private Case1 nine;
    private Case1 ten;
    private Case1 eleven;
    private Case1 twelve;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CaseNumberGroup group.</summary>
    [Serializable]
    public class CaseNumberGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Case1 Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of InactRoleForSupp.
      /// </summary>
      [JsonPropertyName("inactRoleForSupp")]
      public Common InactRoleForSupp
      {
        get => inactRoleForSupp ??= new();
        set => inactRoleForSupp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Case1 detail;
      private Common inactRoleForSupp;
    }

    /// <summary>A ManualAdminActionGroup group.</summary>
    [Serializable]
    public class ManualAdminActionGroup
    {
      /// <summary>
      /// A value of ManualSsnChange.
      /// </summary>
      [JsonPropertyName("manualSsnChange")]
      public Common ManualSsnChange
      {
        get => manualSsnChange ??= new();
        set => manualSsnChange = value;
      }

      /// <summary>
      /// A value of ManualCommon.
      /// </summary>
      [JsonPropertyName("manualCommon")]
      public Common ManualCommon
      {
        get => manualCommon ??= new();
        set => manualCommon = value;
      }

      /// <summary>
      /// A value of ManualObligationType.
      /// </summary>
      [JsonPropertyName("manualObligationType")]
      public ObligationType ManualObligationType
      {
        get => manualObligationType ??= new();
        set => manualObligationType = value;
      }

      /// <summary>
      /// A value of ManualCertified.
      /// </summary>
      [JsonPropertyName("manualCertified")]
      public Common ManualCertified
      {
        get => manualCertified ??= new();
        set => manualCertified = value;
      }

      /// <summary>
      /// A value of ManualCsePerson.
      /// </summary>
      [JsonPropertyName("manualCsePerson")]
      public CsePerson ManualCsePerson
      {
        get => manualCsePerson ??= new();
        set => manualCsePerson = value;
      }

      /// <summary>
      /// A value of ManualObligation.
      /// </summary>
      [JsonPropertyName("manualObligation")]
      public Obligation ManualObligation
      {
        get => manualObligation ??= new();
        set => manualObligation = value;
      }

      /// <summary>
      /// A value of ManualAdministrativeAction.
      /// </summary>
      [JsonPropertyName("manualAdministrativeAction")]
      public AdministrativeAction ManualAdministrativeAction
      {
        get => manualAdministrativeAction ??= new();
        set => manualAdministrativeAction = value;
      }

      /// <summary>
      /// A value of ManualAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("manualAdministrativeActCertification")]
      public AdministrativeActCertification ManualAdministrativeActCertification
      {
        get => manualAdministrativeActCertification ??= new();
        set => manualAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of ManualFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("manualFederalDebtSetoff")]
      public AdministrativeActCertification ManualFederalDebtSetoff
      {
        get => manualFederalDebtSetoff ??= new();
        set => manualFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of ManualObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("manualObligationAdministrativeAction")]
      public ObligationAdministrativeAction ManualObligationAdministrativeAction
      {
        get => manualObligationAdministrativeAction ??= new();
        set => manualObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of ManualExmp.
      /// </summary>
      [JsonPropertyName("manualExmp")]
      public Common ManualExmp
      {
        get => manualExmp ??= new();
        set => manualExmp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common manualSsnChange;
      private Common manualCommon;
      private ObligationType manualObligationType;
      private Common manualCertified;
      private CsePerson manualCsePerson;
      private Obligation manualObligation;
      private AdministrativeAction manualAdministrativeAction;
      private AdministrativeActCertification manualAdministrativeActCertification;
        
      private AdministrativeActCertification manualFederalDebtSetoff;
      private ObligationAdministrativeAction manualObligationAdministrativeAction;
        
      private Common manualExmp;
    }

    /// <summary>A AutoAdminActionsGroup group.</summary>
    [Serializable]
    public class AutoAdminActionsGroup
    {
      /// <summary>
      /// A value of AutoCommon.
      /// </summary>
      [JsonPropertyName("autoCommon")]
      public Common AutoCommon
      {
        get => autoCommon ??= new();
        set => autoCommon = value;
      }

      /// <summary>
      /// A value of AutoObligationType.
      /// </summary>
      [JsonPropertyName("autoObligationType")]
      public ObligationType AutoObligationType
      {
        get => autoObligationType ??= new();
        set => autoObligationType = value;
      }

      /// <summary>
      /// A value of AutoCertified.
      /// </summary>
      [JsonPropertyName("autoCertified")]
      public Common AutoCertified
      {
        get => autoCertified ??= new();
        set => autoCertified = value;
      }

      /// <summary>
      /// A value of AutoDecertified.
      /// </summary>
      [JsonPropertyName("autoDecertified")]
      public Common AutoDecertified
      {
        get => autoDecertified ??= new();
        set => autoDecertified = value;
      }

      /// <summary>
      /// A value of AutoCsePerson.
      /// </summary>
      [JsonPropertyName("autoCsePerson")]
      public CsePerson AutoCsePerson
      {
        get => autoCsePerson ??= new();
        set => autoCsePerson = value;
      }

      /// <summary>
      /// A value of AutoObligation.
      /// </summary>
      [JsonPropertyName("autoObligation")]
      public Obligation AutoObligation
      {
        get => autoObligation ??= new();
        set => autoObligation = value;
      }

      /// <summary>
      /// A value of AutoAdministrativeAction.
      /// </summary>
      [JsonPropertyName("autoAdministrativeAction")]
      public AdministrativeAction AutoAdministrativeAction
      {
        get => autoAdministrativeAction ??= new();
        set => autoAdministrativeAction = value;
      }

      /// <summary>
      /// A value of AutoAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("autoAdministrativeActCertification")]
      public AdministrativeActCertification AutoAdministrativeActCertification
      {
        get => autoAdministrativeActCertification ??= new();
        set => autoAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of AutoFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("autoFederalDebtSetoff")]
      public AdministrativeActCertification AutoFederalDebtSetoff
      {
        get => autoFederalDebtSetoff ??= new();
        set => autoFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of AutoObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("autoObligationAdministrativeAction")]
      public ObligationAdministrativeAction AutoObligationAdministrativeAction
      {
        get => autoObligationAdministrativeAction ??= new();
        set => autoObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of AutoExmp.
      /// </summary>
      [JsonPropertyName("autoExmp")]
      public Common AutoExmp
      {
        get => autoExmp ??= new();
        set => autoExmp = value;
      }

      /// <summary>
      /// A value of AutoPassType.
      /// </summary>
      [JsonPropertyName("autoPassType")]
      public AdministrativeAction AutoPassType
      {
        get => autoPassType ??= new();
        set => autoPassType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common autoCommon;
      private ObligationType autoObligationType;
      private Common autoCertified;
      private Common autoDecertified;
      private CsePerson autoCsePerson;
      private Obligation autoObligation;
      private AdministrativeAction autoAdministrativeAction;
      private AdministrativeActCertification autoAdministrativeActCertification;
      private AdministrativeActCertification autoFederalDebtSetoff;
      private ObligationAdministrativeAction autoObligationAdministrativeAction;
      private Common autoExmp;
      private AdministrativeAction autoPassType;
    }

    /// <summary>
    /// A value of KdmvAlreadyMoved.
    /// </summary>
    [JsonPropertyName("kdmvAlreadyMoved")]
    public Common KdmvAlreadyMoved
    {
      get => kdmvAlreadyMoved ??= new();
      set => kdmvAlreadyMoved = value;
    }

    /// <summary>
    /// A value of LiteralKdmv.
    /// </summary>
    [JsonPropertyName("literalKdmv")]
    public AdministrativeAction LiteralKdmv
    {
      get => literalKdmv ??= new();
      set => literalKdmv = value;
    }

    /// <summary>
    /// A value of KdwpAlreadyMoved.
    /// </summary>
    [JsonPropertyName("kdwpAlreadyMoved")]
    public Common KdwpAlreadyMoved
    {
      get => kdwpAlreadyMoved ??= new();
      set => kdwpAlreadyMoved = value;
    }

    /// <summary>
    /// A value of LiteralKdwp.
    /// </summary>
    [JsonPropertyName("literalKdwp")]
    public AdministrativeAction LiteralKdwp
    {
      get => literalKdwp ??= new();
      set => literalKdwp = value;
    }

    /// <summary>
    /// A value of LiteralA.
    /// </summary>
    [JsonPropertyName("literalA")]
    public AdministrativeAction LiteralA
    {
      get => literalA ??= new();
      set => literalA = value;
    }

    /// <summary>
    /// A value of LiteralYes.
    /// </summary>
    [JsonPropertyName("literalYes")]
    public Common LiteralYes
    {
      get => literalYes ??= new();
      set => literalYes = value;
    }

    /// <summary>
    /// A value of LiteralNo.
    /// </summary>
    [JsonPropertyName("literalNo")]
    public Common LiteralNo
    {
      get => literalNo ??= new();
      set => literalNo = value;
    }

    /// <summary>
    /// A value of LiteralSdso.
    /// </summary>
    [JsonPropertyName("literalSdso")]
    public AdministrativeAction LiteralSdso
    {
      get => literalSdso ??= new();
      set => literalSdso = value;
    }

    /// <summary>
    /// A value of LiteralCred.
    /// </summary>
    [JsonPropertyName("literalCred")]
    public AdministrativeAction LiteralCred
    {
      get => literalCred ??= new();
      set => literalCred = value;
    }

    /// <summary>
    /// A value of LiteralFdso.
    /// </summary>
    [JsonPropertyName("literalFdso")]
    public AdministrativeAction LiteralFdso
    {
      get => literalFdso ??= new();
      set => literalFdso = value;
    }

    /// <summary>
    /// A value of MaxTableSize.
    /// </summary>
    [JsonPropertyName("maxTableSize")]
    public Common MaxTableSize
    {
      get => maxTableSize ??= new();
      set => maxTableSize = value;
    }

    /// <summary>
    /// A value of BlankDate.
    /// </summary>
    [JsonPropertyName("blankDate")]
    public DateWorkArea BlankDate
    {
      get => blankDate ??= new();
      set => blankDate = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Round.
    /// </summary>
    [JsonPropertyName("round")]
    public Common Round
    {
      get => round ??= new();
      set => round = value;
    }

    /// <summary>
    /// Gets a value of CaseNumber.
    /// </summary>
    [JsonIgnore]
    public Array<CaseNumberGroup> CaseNumber => caseNumber ??= new(
      CaseNumberGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CaseNumber for json serialization.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    [Computed]
    public IList<CaseNumberGroup> CaseNumber_Json
    {
      get => caseNumber;
      set => CaseNumber.Assign(value);
    }

    /// <summary>
    /// A value of InactiveRoleForSupptd.
    /// </summary>
    [JsonPropertyName("inactiveRoleForSupptd")]
    public Common InactiveRoleForSupptd
    {
      get => inactiveRoleForSupptd ??= new();
      set => inactiveRoleForSupptd = value;
    }

    /// <summary>
    /// A value of ActiveRoleForSupported.
    /// </summary>
    [JsonPropertyName("activeRoleForSupported")]
    public Common ActiveRoleForSupported
    {
      get => activeRoleForSupported ??= new();
      set => activeRoleForSupported = value;
    }

    /// <summary>
    /// A value of SaveAutoAdministrativeAction.
    /// </summary>
    [JsonPropertyName("saveAutoAdministrativeAction")]
    public AdministrativeAction SaveAutoAdministrativeAction
    {
      get => saveAutoAdministrativeAction ??= new();
      set => saveAutoAdministrativeAction = value;
    }

    /// <summary>
    /// A value of SaveAutoAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("saveAutoAdministrativeActCertification")]
    public AdministrativeActCertification SaveAutoAdministrativeActCertification
    {
      get => saveAutoAdministrativeActCertification ??= new();
      set => saveAutoAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of SaveSubscript.
    /// </summary>
    [JsonPropertyName("saveSubscript")]
    public Common SaveSubscript
    {
      get => saveSubscript ??= new();
      set => saveSubscript = value;
    }

    /// <summary>
    /// A value of ManualFlag.
    /// </summary>
    [JsonPropertyName("manualFlag")]
    public Common ManualFlag
    {
      get => manualFlag ??= new();
      set => manualFlag = value;
    }

    /// <summary>
    /// A value of AutoFlag.
    /// </summary>
    [JsonPropertyName("autoFlag")]
    public Common AutoFlag
    {
      get => autoFlag ??= new();
      set => autoFlag = value;
    }

    /// <summary>
    /// Gets a value of ManualAdminAction.
    /// </summary>
    [JsonIgnore]
    public Array<ManualAdminActionGroup> ManualAdminAction =>
      manualAdminAction ??= new(ManualAdminActionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ManualAdminAction for json serialization.
    /// </summary>
    [JsonPropertyName("manualAdminAction")]
    [Computed]
    public IList<ManualAdminActionGroup> ManualAdminAction_Json
    {
      get => manualAdminAction;
      set => ManualAdminAction.Assign(value);
    }

    /// <summary>
    /// Gets a value of AutoAdminActions.
    /// </summary>
    [JsonIgnore]
    public Array<AutoAdminActionsGroup> AutoAdminActions =>
      autoAdminActions ??= new(AutoAdminActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AutoAdminActions for json serialization.
    /// </summary>
    [JsonPropertyName("autoAdminActions")]
    [Computed]
    public IList<AutoAdminActionsGroup> AutoAdminActions_Json
    {
      get => autoAdminActions;
      set => AutoAdminActions.Assign(value);
    }

    /// <summary>
    /// A value of InitializedToBlanks.
    /// </summary>
    [JsonPropertyName("initializedToBlanks")]
    public ObligationType InitializedToBlanks
    {
      get => initializedToBlanks ??= new();
      set => initializedToBlanks = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public ObligationType Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of SdsoAlreadyMoved.
    /// </summary>
    [JsonPropertyName("sdsoAlreadyMoved")]
    public Common SdsoAlreadyMoved
    {
      get => sdsoAlreadyMoved ??= new();
      set => sdsoAlreadyMoved = value;
    }

    /// <summary>
    /// A value of CredAlreadyMoved.
    /// </summary>
    [JsonPropertyName("credAlreadyMoved")]
    public Common CredAlreadyMoved
    {
      get => credAlreadyMoved ??= new();
      set => credAlreadyMoved = value;
    }

    /// <summary>
    /// A value of FdsoAlreadyMoved.
    /// </summary>
    [JsonPropertyName("fdsoAlreadyMoved")]
    public Common FdsoAlreadyMoved
    {
      get => fdsoAlreadyMoved ??= new();
      set => fdsoAlreadyMoved = value;
    }

    /// <summary>
    /// A value of NoOfOblgAssocWithAact.
    /// </summary>
    [JsonPropertyName("noOfOblgAssocWithAact")]
    public Common NoOfOblgAssocWithAact
    {
      get => noOfOblgAssocWithAact ??= new();
      set => noOfOblgAssocWithAact = value;
    }

    /// <summary>
    /// A value of InitializedToZeros.
    /// </summary>
    [JsonPropertyName("initializedToZeros")]
    public DateWorkArea InitializedToZeros
    {
      get => initializedToZeros ??= new();
      set => initializedToZeros = value;
    }

    private Common kdmvAlreadyMoved;
    private AdministrativeAction literalKdmv;
    private Common kdwpAlreadyMoved;
    private AdministrativeAction literalKdwp;
    private AdministrativeAction literalA;
    private Common literalYes;
    private Common literalNo;
    private AdministrativeAction literalSdso;
    private AdministrativeAction literalCred;
    private AdministrativeAction literalFdso;
    private Common maxTableSize;
    private DateWorkArea blankDate;
    private DateWorkArea startDate;
    private Common round;
    private Array<CaseNumberGroup> caseNumber;
    private Common inactiveRoleForSupptd;
    private Common activeRoleForSupported;
    private AdministrativeAction saveAutoAdministrativeAction;
    private AdministrativeActCertification saveAutoAdministrativeActCertification;
      
    private Common saveSubscript;
    private Common manualFlag;
    private Common autoFlag;
    private Array<ManualAdminActionGroup> manualAdminAction;
    private Array<AutoAdminActionsGroup> autoAdminActions;
    private ObligationType initializedToBlanks;
    private ObligationType current;
    private Common sdsoAlreadyMoved;
    private Common credAlreadyMoved;
    private Common fdsoAlreadyMoved;
    private Common noOfOblgAssocWithAact;
    private DateWorkArea initializedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingObligationAdministrativeAction")]
    public ObligationAdministrativeAction ExistingObligationAdministrativeAction
    {
      get => existingObligationAdministrativeAction ??= new();
      set => existingObligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ExistingAdminActionCertObligation.
    /// </summary>
    [JsonPropertyName("existingAdminActionCertObligation")]
    public AdminActionCertObligation ExistingAdminActionCertObligation
    {
      get => existingAdminActionCertObligation ??= new();
      set => existingAdminActionCertObligation = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("existingAdministrativeActCertification")]
    public AdministrativeActCertification ExistingAdministrativeActCertification
    {
      get => existingAdministrativeActCertification ??= new();
      set => existingAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of ExistingFederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("existingFederalDebtSetoff")]
    public AdministrativeActCertification ExistingFederalDebtSetoff
    {
      get => existingFederalDebtSetoff ??= new();
      set => existingFederalDebtSetoff = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAction")]
    public AdministrativeAction ExistingAdministrativeAction
    {
      get => existingAdministrativeAction ??= new();
      set => existingAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingObligorCsePersonAccount")]
    public CsePersonAccount ExistingObligorCsePersonAccount
    {
      get => existingObligorCsePersonAccount ??= new();
      set => existingObligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePerson.
    /// </summary>
    [JsonPropertyName("existingObligorCsePerson")]
    public CsePerson ExistingObligorCsePerson
    {
      get => existingObligorCsePerson ??= new();
      set => existingObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ExemptionObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("exemptionObligationAdmActionExemption")]
    public ObligationAdmActionExemption ExemptionObligationAdmActionExemption
    {
      get => exemptionObligationAdmActionExemption ??= new();
      set => exemptionObligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of ExemptionAdministrativeAction.
    /// </summary>
    [JsonPropertyName("exemptionAdministrativeAction")]
    public AdministrativeAction ExemptionAdministrativeAction
    {
      get => exemptionAdministrativeAction ??= new();
      set => exemptionAdministrativeAction = value;
    }

    private ObligationType existingObligationType;
    private ObligationAdministrativeAction existingObligationAdministrativeAction;
      
    private AdminActionCertObligation existingAdminActionCertObligation;
    private AdministrativeActCertification existingAdministrativeActCertification;
      
    private AdministrativeActCertification existingFederalDebtSetoff;
    private AdministrativeAction existingAdministrativeAction;
    private Obligation existingObligation;
    private CsePersonAccount existingObligorCsePersonAccount;
    private CsePerson existingObligorCsePerson;
    private ObligationAdmActionExemption exemptionObligationAdmActionExemption;
    private AdministrativeAction exemptionAdministrativeAction;
  }
#endregion
}
