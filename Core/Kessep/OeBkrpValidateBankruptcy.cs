// Program: OE_BKRP_VALIDATE_BANKRUPTCY, ID: 372034231, model: 746.
// Short name: SWE00868
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
/// A program: OE_BKRP_VALIDATE_BANKRUPTCY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block validates Bankruptcy details.
/// </para>
/// </summary>
[Serializable]
public partial class OeBkrpValidateBankruptcy : Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BKRP_VALIDATE_BANKRUPTCY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBkrpValidateBankruptcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBkrpValidateBankruptcy.
  /// </summary>
  public OeBkrpValidateBankruptcy(IContext context, Import import, Export export)
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
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 12/17/98 R.Jean        Remove extraneous views; remove unneeded 
    // CSE_PERSON read; set local views for datenum(0) and current date then use
    // them throughout CAB in tests; remove USE GET_CLIENT_DETAILS statement;
    // restructure inefficient read statements; add new edits numbered from 46
    // to 61
    // 02/07/00 R. Jean	PR86356 - Qualify edit 43 to to invoke only when date 
    // has changed
    // 05/25/2011 T. Pierce   CQ#27198  Set error when expected discharge
    // date is populated while either the discharge date or dis/with date
    // is populated.
    // ---------------------------------------------
    local.Zero.Date = null;
    local.Current.Date = Now().Date;
    export.Errors.Index = -1;
    export.LastErrorEntryNo.Count = 0;

    // ---------------------------------------------
    // For Add: the latest existing bankruptcy must
    // have been discharged.
    // ---------------------------------------------
    if(Equal(import.UserAction.Command, "CREATE"))
    {
      if(ReadBankruptcy2())
      {
        if(!Lt(local.Zero.Date,
          entities.ExistingBankruptcy.BankruptcyDischargeDate) && Equal
          (entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate,
          local.Zero.Date))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 27;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;

          return;
        }
      }
    }

    // ---------------------------------------------
    // For Update cannot change discharge or dismiss
    // withdraw dates if populated.  Must read current bankruptcy
    // and campare if dates have changed
    // ---------------------------------------------
    if(Equal(import.UserAction.Command, "UPDATE"))
    {
      if(ReadBankruptcy1())
      {
        if(!Equal(entities.ExistingBankruptcy.BankruptcyDischargeDate,
          local.Zero.Date) && !
          Equal(import.Bankruptcy.BankruptcyDischargeDate,
          entities.ExistingBankruptcy.BankruptcyDischargeDate) && !
          Equal(Date(entities.ExistingBankruptcy.CreatedTimestamp),
          local.Current.Date))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 60;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(!Equal(entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate,
          local.Zero.Date) && !
          Equal(import.Bankruptcy.BankruptcyDismissWithdrawDate,
          entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate) && !
          Equal(Date(entities.ExistingBankruptcy.CreatedTimestamp),
          local.Current.Date))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 61;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }
      else
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 2;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;

        return;
      }
    }

    if(Equal(import.UserAction.Command, "DELETE"))
    {
      if(!Equal(Date(import.Bankruptcy.CreatedTimestamp), local.Current.Date))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 45;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      // No further validation required for deletes.
      // ---------------------------------------------
      return;
    }

    // ---------------------------------------------
    // For Create/ Update, validate individual fields.
    // ---------------------------------------------
    if(Equal(import.UserAction.Command, "CREATE") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      // ---------------------------------------------
      local.Code.CodeName = "BANKRUPTCY TYPE";
      local.CodeValue.Cdvalue = import.Bankruptcy.BankruptcyType;
      UseCabValidateCodeValue();

      if(AsChar(local.ValidCode.Flag) == 'N')
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 3;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      if(Equal(import.Bankruptcy.BankruptcyType, "CO"))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 46;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      if(IsEmpty(import.Bankruptcy.BankruptcyCourtActionNo))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 4;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      if(!Lt(local.Zero.Date, import.Bankruptcy.BankruptcyFilingDate))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 50;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      if(Lt(local.Current.Date, import.Bankruptcy.BankruptcyFilingDate))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 47;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.DateRequestedMotionToLift))
      {
        if(Lt(local.Current.Date, import.Bankruptcy.DateRequestedMotionToLift) ||
          Lt
          (import.Bankruptcy.DateRequestedMotionToLift,
          import.Bankruptcy.BankruptcyFilingDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 20;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.DateMotionToLiftGranted))
      {
        if(Lt(local.Current.Date, import.Bankruptcy.DateMotionToLiftGranted) ||
          Lt
          (import.Bankruptcy.DateMotionToLiftGranted,
          import.Bankruptcy.BankruptcyFilingDate) || Lt
          (import.Bankruptcy.DateMotionToLiftGranted,
          import.Bankruptcy.DateRequestedMotionToLift))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 28;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.ProofOfClaimFiledDate))
      {
        if(Lt(local.Current.Date, import.Bankruptcy.ProofOfClaimFiledDate) || Lt
          (import.Bankruptcy.ProofOfClaimFiledDate,
          import.Bankruptcy.BankruptcyFilingDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 21;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyConfirmationDate))
      {
        if(Lt(local.Current.Date, import.Bankruptcy.BankruptcyConfirmationDate) ||
          Lt
          (import.Bankruptcy.BankruptcyConfirmationDate,
          import.Bankruptcy.BankruptcyFilingDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 22;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Equal(import.Bankruptcy.BankruptcyType, "07"))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 62;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDischargeDate) && Lt
          (import.Bankruptcy.BankruptcyDischargeDate,
          import.Bankruptcy.BankruptcyConfirmationDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 58;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDismissWithdrawDate) &&
          Lt
          (import.Bankruptcy.BankruptcyDismissWithdrawDate,
          import.Bankruptcy.BankruptcyConfirmationDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 59;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyFilingDate))
      {
        if(Equal(import.Bankruptcy.ExpectedBkrpDischargeDate, local.Zero.Date) &&
          Equal(import.Bankruptcy.BankruptcyDischargeDate, local.Zero.Date) && Equal
          (import.Bankruptcy.BankruptcyDismissWithdrawDate, local.Zero.Date))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 57;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDischargeDate))
      {
        if(Lt(local.Current.Date, import.Bankruptcy.BankruptcyDischargeDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 48;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Equal(import.Bankruptcy.BankruptcyType, "13"))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 63;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Lt(import.Bankruptcy.BankruptcyDischargeDate,
          import.Bankruptcy.BankruptcyFilingDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 49;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDismissWithdrawDate))
      {
        if(Lt(local.Current.Date,
          import.Bankruptcy.BankruptcyDismissWithdrawDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 51;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Lt(import.Bankruptcy.BankruptcyDismissWithdrawDate,
          import.Bankruptcy.BankruptcyFilingDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 52;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDischargeDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 56;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      // ****************************************************************
      // * 02/07/00R. Jean	PR86356 - Qualify edit 43 to to invoke
      // * only when date has changed
      // ****************************************************************
      if(Lt(local.Zero.Date, import.Bankruptcy.ExpectedBkrpDischargeDate))
      {
        if(!Lt(local.Current.Date, import.Bankruptcy.ExpectedBkrpDischargeDate) &&
          !
          Equal(import.Bankruptcy.ExpectedBkrpDischargeDate,
          entities.ExistingBankruptcy.ExpectedBkrpDischargeDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 43;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      // CQ#27198 05/26/2011  T. Pierce  Before this change, on add or
      // update either a discharge date or an expected discharge date
      // was required. This check is disabled so that a dis/with date
      // can be entered without the expected discharge date.
      // ---------------------------------------------
      // Check that bankruptcy does not overlap.
      // ---------------------------------------------
      // CQ#27198 05/26/2011  T. Pierce  Modify this "IF" statement
      // to include Dis/With date greater than the low-value default.
      if((Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDischargeDate) || Lt
        (local.Zero.Date, import.Bankruptcy.BankruptcyDismissWithdrawDate)) && Lt
        (local.Zero.Date, import.Bankruptcy.BankruptcyFilingDate))
      {
        // ---------------------------------------------
        // Raju : 12/18/1996 ; 10:35 hrs CST
        // bug : read each initially was reading entire
        //       bankruptcy entity.
        // correction : read only records related to
        //              the cse person input
        // ---------------------------------------------
        foreach(var item in ReadBankruptcy3())
        {
          if(Equal(import.UserAction.Command, "UPDATE"))
          {
            if(entities.ExistingBankruptcy.Identifier == import
              .Bankruptcy.Identifier)
            {
              continue;
            }
          }

          // --------------------------------------------
          // Raju : 11:10 hrs CST
          // Completed with analysis : 12:00 hrs CST
          // The following conditional check was changed
          // initial - ( import bankruptcy filing date <
          //           exising bankruptcy filing date  AND
          //           import bankruptcy discharge date
          //           <= existing bankruptcy date )
          //                     OR
          //           ( import bankruptcy filing date
          //             >= existing bankruptcy discharge
          //             date AND
          //             import bankruptcy discharge date
          //             > existing bankruptcy discharge
          //             date )
          // Correction made : < and > changed to
          //                   <= and >= resp. because
          //                   foll. cases
          //  Case a) import  filing and dischg
          // 	 dates = existing filing date
          //  Case b) import filing and dischg
          //          dates = existing dischg date
          //    will be shown as overlaps when they are
          //    quite valid
          // --------------------------------------------
          if(Lt(local.Zero.Date, import.Bankruptcy.BankruptcyDischargeDate))
          {
            if(!Lt(entities.ExistingBankruptcy.BankruptcyFilingDate,
              import.Bankruptcy.BankruptcyFilingDate) && !
              Lt(entities.ExistingBankruptcy.BankruptcyFilingDate,
              import.Bankruptcy.BankruptcyDischargeDate) || !
              Lt(import.Bankruptcy.BankruptcyFilingDate,
              entities.ExistingBankruptcy.BankruptcyDischargeDate) && !
              Lt(import.Bankruptcy.BankruptcyDischargeDate,
              entities.ExistingBankruptcy.BankruptcyDischargeDate))
            {
              // ***	Okay, no overlap exists.
            }
            else
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.Errors.Update.DetailErrorCode.Count = 42;
              export.LastErrorEntryNo.Count = export.Errors.Index + 1;
            }
          }

          // CQ#27198 05/26/2011  T. Pierce  Apply the overlap rule to the Dis/
          // With
          // in addition to the previous evaluation of Discharge.
          if(Lt(local.Zero.Date,
            import.Bankruptcy.BankruptcyDismissWithdrawDate))
          {
            if(!Lt(entities.ExistingBankruptcy.BankruptcyFilingDate,
              import.Bankruptcy.BankruptcyFilingDate) && !
              Lt(entities.ExistingBankruptcy.BankruptcyFilingDate,
              import.Bankruptcy.BankruptcyDismissWithdrawDate) || !
              Lt(import.Bankruptcy.BankruptcyFilingDate,
              entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate) && !
              Lt(import.Bankruptcy.BankruptcyDismissWithdrawDate,
              entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate))
            {
              // ***	Okay, no overlap exists.
            }
            else
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.Errors.Update.DetailErrorCode.Count = 42;
              export.LastErrorEntryNo.Count = export.Errors.Index + 1;
            }
          }
        }
      }

      // ---------------------------------------------
      if(IsEmpty(import.Bankruptcy.BankruptcyDistrictCourt))
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.Errors.Update.DetailErrorCode.Count = 5;
        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      }

      // ---------------------------------------------
      if(import.Bankruptcy.BdcPhoneAreaCode.GetValueOrDefault() != 0 || import
        .Bankruptcy.BdcPhoneNo.GetValueOrDefault() != 0 || !
        IsEmpty(import.Bankruptcy.BdcPhoneExt))
      {
        if((import.Bankruptcy.BdcPhoneNo.GetValueOrDefault() == 0 || import
          .Bankruptcy.BdcPhoneAreaCode.GetValueOrDefault() == 0) && !
          IsEmpty(import.Bankruptcy.BdcPhoneExt))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 53;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.BdcPhoneAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 29;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.BdcPhoneNo.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 30;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(!IsEmpty(import.Bankruptcy.BdcAddrStreet1) || !
        IsEmpty(import.Bankruptcy.BdcAddrStreet2) || !
        IsEmpty(import.Bankruptcy.BdcAddrCity) || !
        IsEmpty(import.Bankruptcy.BdcAddrState) || !
        IsEmpty(import.Bankruptcy.BdcAddrZip5) || !
        IsEmpty(import.Bankruptcy.BdcAddrZip4) || !
        IsEmpty(import.Bankruptcy.BdcAddrZip3))
      {
        if(IsEmpty(import.Bankruptcy.BdcAddrStreet1))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 6;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(IsEmpty(import.Bankruptcy.BdcAddrCity))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 7;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = import.Bankruptcy.BdcAddrState ?? Spaces(10);
        UseCabValidateCodeValue();

        if(AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 8;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(IsEmpty(import.Bankruptcy.BdcAddrZip5))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 24;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
        else
        {
          local.CheckZip.Count = 0;

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.Bankruptcy.BdcAddrZip5, local.CheckZip.Count, 1);


            if(AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.Errors.Update.DetailErrorCode.Count = 39;
              export.LastErrorEntryNo.Count = export.Errors.Index + 1;

              break;
            }
          }
          while(local.CheckZip.Count < 5);
        }
      }

      // ---------------------------------------------
      if(!IsEmpty(import.Bankruptcy.BtoAddrStreet1) || !
        IsEmpty(import.Bankruptcy.BtoAddrStreet2) || !
        IsEmpty(import.Bankruptcy.BtoAddrCity) || !
        IsEmpty(import.Bankruptcy.BtoAddrState) || !
        IsEmpty(import.Bankruptcy.BtoAddrZip5) || !
        IsEmpty(import.Bankruptcy.BtoAddrZip4) || !
        IsEmpty(import.Bankruptcy.BtoAddrZip3))
      {
        if(IsEmpty(import.Bankruptcy.BtoAddrStreet1))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 11;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(IsEmpty(import.Bankruptcy.BtoAddrCity))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 12;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = import.Bankruptcy.BtoAddrState ?? Spaces(10);
        UseCabValidateCodeValue();

        if(AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 13;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(IsEmpty(import.Bankruptcy.BtoAddrZip5))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 25;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
        else
        {
          local.CheckZip.Count = 0;

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.Bankruptcy.BtoAddrZip5, local.CheckZip.Count, 1);


            if(AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.Errors.Update.DetailErrorCode.Count = 40;
              export.LastErrorEntryNo.Count = export.Errors.Index + 1;

              break;
            }
          }
          while(local.CheckZip.Count < 5);
        }
      }

      // ---------------------------------------------
      if(import.Bankruptcy.BtoPhoneAreaCode.GetValueOrDefault() != 0 || import
        .Bankruptcy.BtoPhoneNo.GetValueOrDefault() != 0 || !
        IsEmpty(import.Bankruptcy.BtoPhoneExt))
      {
        if((import.Bankruptcy.BtoPhoneNo.GetValueOrDefault() == 0 || import
          .Bankruptcy.BtoPhoneAreaCode.GetValueOrDefault() == 0) && !
          IsEmpty(import.Bankruptcy.BtoPhoneExt))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 54;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.BtoPhoneAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 31;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.BtoPhoneNo.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 32;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(import.Bankruptcy.BtoFaxAreaCode.GetValueOrDefault() != 0 || import
        .Bankruptcy.BtoFax.GetValueOrDefault() != 0 || !
        IsEmpty(import.Bankruptcy.BtoFaxExt))
      {
        if(import.Bankruptcy.BtoFaxAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 33;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.BtoFax.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 34;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(!IsEmpty(import.Bankruptcy.ApAttrAddrStreet1) || !
        IsEmpty(import.Bankruptcy.ApAttrAddrStreet2) || !
        IsEmpty(import.Bankruptcy.ApAttrAddrCity) || !
        IsEmpty(import.Bankruptcy.ApAttrAddrState) || !
        IsEmpty(import.Bankruptcy.ApAttrAddrZip5) || !
        IsEmpty(import.Bankruptcy.ApAttrAddrZip4) || !
        IsEmpty(import.Bankruptcy.ApAttrAddrZip3))
      {
        if(IsEmpty(import.Bankruptcy.ApAttrAddrStreet1))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 16;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        if(IsEmpty(import.Bankruptcy.ApAttrAddrCity))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 17;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        // ---------------------------------------------
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = import.Bankruptcy.ApAttrAddrState ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if(AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 18;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(IsEmpty(import.Bankruptcy.ApAttrAddrZip5))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 26;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
        else
        {
          local.CheckZip.Count = 0;

          do
          {
            ++local.CheckZip.Count;
            local.CheckZip.Flag =
              Substring(import.Bankruptcy.ApAttrAddrZip5, local.CheckZip.Count,
              1);

            if(AsChar(local.CheckZip.Flag) < '0' || AsChar
              (local.CheckZip.Flag) > '9')
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.Errors.Update.DetailErrorCode.Count = 41;
              export.LastErrorEntryNo.Count = export.Errors.Index + 1;

              break;
            }
          }
          while(local.CheckZip.Count < 5);
        }
      }

      // ---------------------------------------------
      if(import.Bankruptcy.ApAttorneyPhoneAreaCode.GetValueOrDefault() != 0
        || import.Bankruptcy.ApAttorneyPhoneNo.GetValueOrDefault() != 0 || !
        IsEmpty(import.Bankruptcy.ApAttorneyPhoneExt))
      {
        if((import.Bankruptcy.ApAttorneyPhoneNo.GetValueOrDefault() == 0 || import
          .Bankruptcy.ApAttorneyPhoneAreaCode.GetValueOrDefault() == 0) && !
          IsEmpty(import.Bankruptcy.ApAttorneyPhoneExt))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 55;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.ApAttorneyPhoneAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 35;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.ApAttorneyPhoneNo.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 36;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(import.Bankruptcy.ApAttorneyFaxAreaCode.GetValueOrDefault() != 0 || import
        .Bankruptcy.ApAttorneyFax.GetValueOrDefault() != 0 || !
        IsEmpty(import.Bankruptcy.ApAttorneyFaxExt))
      {
        if(import.Bankruptcy.ApAttorneyFaxAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 37;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(import.Bankruptcy.ApAttorneyFax.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 38;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(!IsEmpty(import.Bankruptcy.TrusteeFirstName) || !
        IsEmpty(import.Bankruptcy.TrusteeMiddleInt) || !
        IsEmpty(import.Bankruptcy.TrusteeLastName))
      {
        if(IsEmpty(import.Bankruptcy.TrusteeFirstName))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 10;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(IsEmpty(import.Bankruptcy.TrusteeLastName))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 9;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ---------------------------------------------
      if(!IsEmpty(import.Bankruptcy.ApAttorneyFirstName) || !
        IsEmpty(import.Bankruptcy.ApAttorneyMi) || !
        IsEmpty(import.Bankruptcy.ApAttorneyLastName))
      {
        if(IsEmpty(import.Bankruptcy.ApAttorneyFirstName))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 15;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }

        if(IsEmpty(import.Bankruptcy.ApAttorneyLastName))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 14;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }

      // ------------------------------------------------------------
      // CQ#27198 05/26/2011  T. Pierce  Set an error if the expected
      // discharge date is populated while either the discharge date
      //  or dis/with date is populated.
      // ------------------------------------------------------------
      if(!Equal(import.Bankruptcy.ExpectedBkrpDischargeDate, local.Zero.Date))
      {
        if(!Equal(import.Bankruptcy.BankruptcyDischargeDate, local.Zero.Date) ||
          !
          Equal(import.Bankruptcy.BankruptcyDismissWithdrawDate, local.Zero.Date))

        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.Errors.Update.DetailErrorCode.Count = 64;
          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        }
      }
    }
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadBankruptcy1()
  {
    entities.ExistingBankruptcy.Populated = false;

    return Read("ReadBankruptcy1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", import.Bankruptcy.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingBankruptcy.CspNumber = db.GetString(reader, 0);
        entities.ExistingBankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.ExistingBankruptcy.BankruptcyCourtActionNo =
          db.GetString(reader, 2);
        entities.ExistingBankruptcy.BankruptcyType = db.GetString(reader, 3);
        entities.ExistingBankruptcy.BankruptcyFilingDate =
          db.GetDate(reader, 4);
        entities.ExistingBankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingBankruptcy.BankruptcyConfirmationDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingBankruptcy.ProofOfClaimFiledDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingBankruptcy.TrusteeLastName =
          db.GetNullableString(reader, 8);
        entities.ExistingBankruptcy.TrusteeFirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingBankruptcy.TrusteeMiddleInt =
          db.GetNullableString(reader, 10);
        entities.ExistingBankruptcy.TrusteeSuffix =
          db.GetNullableString(reader, 11);
        entities.ExistingBankruptcy.BtoFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.ExistingBankruptcy.BtoPhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ExistingBankruptcy.BdcPhoneAreaCode =
          db.GetNullableInt32(reader, 14);
        entities.ExistingBankruptcy.ApAttorneyFaxAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingBankruptcy.ApAttorneyPhoneAreaCode =
          db.GetNullableInt32(reader, 16);
        entities.ExistingBankruptcy.BtoPhoneExt =
          db.GetNullableString(reader, 17);
        entities.ExistingBankruptcy.BtoFaxExt =
          db.GetNullableString(reader, 18);
        entities.ExistingBankruptcy.BdcPhoneExt =
          db.GetNullableString(reader, 19);
        entities.ExistingBankruptcy.ApAttorneyFaxExt =
          db.GetNullableString(reader, 20);
        entities.ExistingBankruptcy.ApAttorneyPhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingBankruptcy.DateRequestedMotionToLift =
          db.GetNullableDate(reader, 22);
        entities.ExistingBankruptcy.DateMotionToLiftGranted =
          db.GetNullableDate(reader, 23);
        entities.ExistingBankruptcy.BtoPhoneNo =
          db.GetNullableInt32(reader, 24);
        entities.ExistingBankruptcy.BtoFax = db.GetNullableInt32(reader, 25);
        entities.ExistingBankruptcy.BtoAddrStreet1 =
          db.GetNullableString(reader, 26);
        entities.ExistingBankruptcy.BtoAddrStreet2 =
          db.GetNullableString(reader, 27);
        entities.ExistingBankruptcy.BtoAddrCity =
          db.GetNullableString(reader, 28);
        entities.ExistingBankruptcy.BtoAddrState =
          db.GetNullableString(reader, 29);
        entities.ExistingBankruptcy.BtoAddrZip5 =
          db.GetNullableString(reader, 30);
        entities.ExistingBankruptcy.BtoAddrZip4 =
          db.GetNullableString(reader, 31);
        entities.ExistingBankruptcy.BtoAddrZip3 =
          db.GetNullableString(reader, 32);
        entities.ExistingBankruptcy.BankruptcyDistrictCourt =
          db.GetString(reader, 33);
        entities.ExistingBankruptcy.BdcPhoneNo =
          db.GetNullableInt32(reader, 34);
        entities.ExistingBankruptcy.BdcAddrStreet1 =
          db.GetNullableString(reader, 35);
        entities.ExistingBankruptcy.BdcAddrStreet2 =
          db.GetNullableString(reader, 36);
        entities.ExistingBankruptcy.BdcAddrCity =
          db.GetNullableString(reader, 37);
        entities.ExistingBankruptcy.BdcAddrState =
          db.GetNullableString(reader, 38);
        entities.ExistingBankruptcy.BdcAddrZip5 =
          db.GetNullableString(reader, 39);
        entities.ExistingBankruptcy.BdcAddrZip4 =
          db.GetNullableString(reader, 40);
        entities.ExistingBankruptcy.BdcAddrZip3 =
          db.GetNullableString(reader, 41);
        entities.ExistingBankruptcy.ApAttorneyFirmName =
          db.GetNullableString(reader, 42);
        entities.ExistingBankruptcy.ApAttorneyLastName =
          db.GetNullableString(reader, 43);
        entities.ExistingBankruptcy.ApAttorneyFirstName =
          db.GetNullableString(reader, 44);
        entities.ExistingBankruptcy.ApAttorneyMi =
          db.GetNullableString(reader, 45);
        entities.ExistingBankruptcy.ApAttorneySuffix =
          db.GetNullableString(reader, 46);
        entities.ExistingBankruptcy.ApAttorneyPhoneNo =
          db.GetNullableInt32(reader, 47);
        entities.ExistingBankruptcy.ApAttorneyFax =
          db.GetNullableInt32(reader, 48);
        entities.ExistingBankruptcy.ApAttrAddrStreet1 =
          db.GetNullableString(reader, 49);
        entities.ExistingBankruptcy.ApAttrAddrStreet2 =
          db.GetNullableString(reader, 50);
        entities.ExistingBankruptcy.ApAttrAddrCity =
          db.GetNullableString(reader, 51);
        entities.ExistingBankruptcy.ApAttrAddrState =
          db.GetNullableString(reader, 52);
        entities.ExistingBankruptcy.ApAttrAddrZip5 =
          db.GetNullableString(reader, 53);
        entities.ExistingBankruptcy.ApAttrAddrZip4 =
          db.GetNullableString(reader, 54);
        entities.ExistingBankruptcy.ApAttrAddrZip3 =
          db.GetNullableString(reader, 55);
        entities.ExistingBankruptcy.CreatedBy = db.GetString(reader, 56);
        entities.ExistingBankruptcy.CreatedTimestamp =
          db.GetDateTime(reader, 57);
        entities.ExistingBankruptcy.LastUpdatedBy = db.GetString(reader, 58);
        entities.ExistingBankruptcy.LastUpdatedTimestamp =
          db.GetDateTime(reader, 59);
        entities.ExistingBankruptcy.ExpectedBkrpDischargeDate =
          db.GetNullableDate(reader, 60);
        entities.ExistingBankruptcy.Narrative =
          db.GetNullableString(reader, 61);
        entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 62);
        entities.ExistingBankruptcy.Populated = true;
      });
  }

  private bool ReadBankruptcy2()
  {
    entities.ExistingBankruptcy.Populated = false;

    return Read("ReadBankruptcy2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingBankruptcy.CspNumber = db.GetString(reader, 0);
        entities.ExistingBankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.ExistingBankruptcy.BankruptcyCourtActionNo =
          db.GetString(reader, 2);
        entities.ExistingBankruptcy.BankruptcyType = db.GetString(reader, 3);
        entities.ExistingBankruptcy.BankruptcyFilingDate =
          db.GetDate(reader, 4);
        entities.ExistingBankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingBankruptcy.BankruptcyConfirmationDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingBankruptcy.ProofOfClaimFiledDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingBankruptcy.TrusteeLastName =
          db.GetNullableString(reader, 8);
        entities.ExistingBankruptcy.TrusteeFirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingBankruptcy.TrusteeMiddleInt =
          db.GetNullableString(reader, 10);
        entities.ExistingBankruptcy.TrusteeSuffix =
          db.GetNullableString(reader, 11);
        entities.ExistingBankruptcy.BtoFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.ExistingBankruptcy.BtoPhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ExistingBankruptcy.BdcPhoneAreaCode =
          db.GetNullableInt32(reader, 14);
        entities.ExistingBankruptcy.ApAttorneyFaxAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingBankruptcy.ApAttorneyPhoneAreaCode =
          db.GetNullableInt32(reader, 16);
        entities.ExistingBankruptcy.BtoPhoneExt =
          db.GetNullableString(reader, 17);
        entities.ExistingBankruptcy.BtoFaxExt =
          db.GetNullableString(reader, 18);
        entities.ExistingBankruptcy.BdcPhoneExt =
          db.GetNullableString(reader, 19);
        entities.ExistingBankruptcy.ApAttorneyFaxExt =
          db.GetNullableString(reader, 20);
        entities.ExistingBankruptcy.ApAttorneyPhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingBankruptcy.DateRequestedMotionToLift =
          db.GetNullableDate(reader, 22);
        entities.ExistingBankruptcy.DateMotionToLiftGranted =
          db.GetNullableDate(reader, 23);
        entities.ExistingBankruptcy.BtoPhoneNo =
          db.GetNullableInt32(reader, 24);
        entities.ExistingBankruptcy.BtoFax = db.GetNullableInt32(reader, 25);
        entities.ExistingBankruptcy.BtoAddrStreet1 =
          db.GetNullableString(reader, 26);
        entities.ExistingBankruptcy.BtoAddrStreet2 =
          db.GetNullableString(reader, 27);
        entities.ExistingBankruptcy.BtoAddrCity =
          db.GetNullableString(reader, 28);
        entities.ExistingBankruptcy.BtoAddrState =
          db.GetNullableString(reader, 29);
        entities.ExistingBankruptcy.BtoAddrZip5 =
          db.GetNullableString(reader, 30);
        entities.ExistingBankruptcy.BtoAddrZip4 =
          db.GetNullableString(reader, 31);
        entities.ExistingBankruptcy.BtoAddrZip3 =
          db.GetNullableString(reader, 32);
        entities.ExistingBankruptcy.BankruptcyDistrictCourt =
          db.GetString(reader, 33);
        entities.ExistingBankruptcy.BdcPhoneNo =
          db.GetNullableInt32(reader, 34);
        entities.ExistingBankruptcy.BdcAddrStreet1 =
          db.GetNullableString(reader, 35);
        entities.ExistingBankruptcy.BdcAddrStreet2 =
          db.GetNullableString(reader, 36);
        entities.ExistingBankruptcy.BdcAddrCity =
          db.GetNullableString(reader, 37);
        entities.ExistingBankruptcy.BdcAddrState =
          db.GetNullableString(reader, 38);
        entities.ExistingBankruptcy.BdcAddrZip5 =
          db.GetNullableString(reader, 39);
        entities.ExistingBankruptcy.BdcAddrZip4 =
          db.GetNullableString(reader, 40);
        entities.ExistingBankruptcy.BdcAddrZip3 =
          db.GetNullableString(reader, 41);
        entities.ExistingBankruptcy.ApAttorneyFirmName =
          db.GetNullableString(reader, 42);
        entities.ExistingBankruptcy.ApAttorneyLastName =
          db.GetNullableString(reader, 43);
        entities.ExistingBankruptcy.ApAttorneyFirstName =
          db.GetNullableString(reader, 44);
        entities.ExistingBankruptcy.ApAttorneyMi =
          db.GetNullableString(reader, 45);
        entities.ExistingBankruptcy.ApAttorneySuffix =
          db.GetNullableString(reader, 46);
        entities.ExistingBankruptcy.ApAttorneyPhoneNo =
          db.GetNullableInt32(reader, 47);
        entities.ExistingBankruptcy.ApAttorneyFax =
          db.GetNullableInt32(reader, 48);
        entities.ExistingBankruptcy.ApAttrAddrStreet1 =
          db.GetNullableString(reader, 49);
        entities.ExistingBankruptcy.ApAttrAddrStreet2 =
          db.GetNullableString(reader, 50);
        entities.ExistingBankruptcy.ApAttrAddrCity =
          db.GetNullableString(reader, 51);
        entities.ExistingBankruptcy.ApAttrAddrState =
          db.GetNullableString(reader, 52);
        entities.ExistingBankruptcy.ApAttrAddrZip5 =
          db.GetNullableString(reader, 53);
        entities.ExistingBankruptcy.ApAttrAddrZip4 =
          db.GetNullableString(reader, 54);
        entities.ExistingBankruptcy.ApAttrAddrZip3 =
          db.GetNullableString(reader, 55);
        entities.ExistingBankruptcy.CreatedBy = db.GetString(reader, 56);
        entities.ExistingBankruptcy.CreatedTimestamp =
          db.GetDateTime(reader, 57);
        entities.ExistingBankruptcy.LastUpdatedBy = db.GetString(reader, 58);
        entities.ExistingBankruptcy.LastUpdatedTimestamp =
          db.GetDateTime(reader, 59);
        entities.ExistingBankruptcy.ExpectedBkrpDischargeDate =
          db.GetNullableDate(reader, 60);
        entities.ExistingBankruptcy.Narrative =
          db.GetNullableString(reader, 61);
        entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 62);
        entities.ExistingBankruptcy.Populated = true;
      });
  }

  private IEnumerable<bool> ReadBankruptcy3()
  {
    entities.ExistingBankruptcy.Populated = false;

    return ReadEach("ReadBankruptcy3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingBankruptcy.CspNumber = db.GetString(reader, 0);
        entities.ExistingBankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.ExistingBankruptcy.BankruptcyCourtActionNo =
          db.GetString(reader, 2);
        entities.ExistingBankruptcy.BankruptcyType = db.GetString(reader, 3);
        entities.ExistingBankruptcy.BankruptcyFilingDate =
          db.GetDate(reader, 4);
        entities.ExistingBankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingBankruptcy.BankruptcyConfirmationDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingBankruptcy.ProofOfClaimFiledDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingBankruptcy.TrusteeLastName =
          db.GetNullableString(reader, 8);
        entities.ExistingBankruptcy.TrusteeFirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingBankruptcy.TrusteeMiddleInt =
          db.GetNullableString(reader, 10);
        entities.ExistingBankruptcy.TrusteeSuffix =
          db.GetNullableString(reader, 11);
        entities.ExistingBankruptcy.BtoFaxAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.ExistingBankruptcy.BtoPhoneAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.ExistingBankruptcy.BdcPhoneAreaCode =
          db.GetNullableInt32(reader, 14);
        entities.ExistingBankruptcy.ApAttorneyFaxAreaCode =
          db.GetNullableInt32(reader, 15);
        entities.ExistingBankruptcy.ApAttorneyPhoneAreaCode =
          db.GetNullableInt32(reader, 16);
        entities.ExistingBankruptcy.BtoPhoneExt =
          db.GetNullableString(reader, 17);
        entities.ExistingBankruptcy.BtoFaxExt =
          db.GetNullableString(reader, 18);
        entities.ExistingBankruptcy.BdcPhoneExt =
          db.GetNullableString(reader, 19);
        entities.ExistingBankruptcy.ApAttorneyFaxExt =
          db.GetNullableString(reader, 20);
        entities.ExistingBankruptcy.ApAttorneyPhoneExt =
          db.GetNullableString(reader, 21);
        entities.ExistingBankruptcy.DateRequestedMotionToLift =
          db.GetNullableDate(reader, 22);
        entities.ExistingBankruptcy.DateMotionToLiftGranted =
          db.GetNullableDate(reader, 23);
        entities.ExistingBankruptcy.BtoPhoneNo =
          db.GetNullableInt32(reader, 24);
        entities.ExistingBankruptcy.BtoFax = db.GetNullableInt32(reader, 25);
        entities.ExistingBankruptcy.BtoAddrStreet1 =
          db.GetNullableString(reader, 26);
        entities.ExistingBankruptcy.BtoAddrStreet2 =
          db.GetNullableString(reader, 27);
        entities.ExistingBankruptcy.BtoAddrCity =
          db.GetNullableString(reader, 28);
        entities.ExistingBankruptcy.BtoAddrState =
          db.GetNullableString(reader, 29);
        entities.ExistingBankruptcy.BtoAddrZip5 =
          db.GetNullableString(reader, 30);
        entities.ExistingBankruptcy.BtoAddrZip4 =
          db.GetNullableString(reader, 31);
        entities.ExistingBankruptcy.BtoAddrZip3 =
          db.GetNullableString(reader, 32);
        entities.ExistingBankruptcy.BankruptcyDistrictCourt =
          db.GetString(reader, 33);
        entities.ExistingBankruptcy.BdcPhoneNo =
          db.GetNullableInt32(reader, 34);
        entities.ExistingBankruptcy.BdcAddrStreet1 =
          db.GetNullableString(reader, 35);
        entities.ExistingBankruptcy.BdcAddrStreet2 =
          db.GetNullableString(reader, 36);
        entities.ExistingBankruptcy.BdcAddrCity =
          db.GetNullableString(reader, 37);
        entities.ExistingBankruptcy.BdcAddrState =
          db.GetNullableString(reader, 38);
        entities.ExistingBankruptcy.BdcAddrZip5 =
          db.GetNullableString(reader, 39);
        entities.ExistingBankruptcy.BdcAddrZip4 =
          db.GetNullableString(reader, 40);
        entities.ExistingBankruptcy.BdcAddrZip3 =
          db.GetNullableString(reader, 41);
        entities.ExistingBankruptcy.ApAttorneyFirmName =
          db.GetNullableString(reader, 42);
        entities.ExistingBankruptcy.ApAttorneyLastName =
          db.GetNullableString(reader, 43);
        entities.ExistingBankruptcy.ApAttorneyFirstName =
          db.GetNullableString(reader, 44);
        entities.ExistingBankruptcy.ApAttorneyMi =
          db.GetNullableString(reader, 45);
        entities.ExistingBankruptcy.ApAttorneySuffix =
          db.GetNullableString(reader, 46);
        entities.ExistingBankruptcy.ApAttorneyPhoneNo =
          db.GetNullableInt32(reader, 47);
        entities.ExistingBankruptcy.ApAttorneyFax =
          db.GetNullableInt32(reader, 48);
        entities.ExistingBankruptcy.ApAttrAddrStreet1 =
          db.GetNullableString(reader, 49);
        entities.ExistingBankruptcy.ApAttrAddrStreet2 =
          db.GetNullableString(reader, 50);
        entities.ExistingBankruptcy.ApAttrAddrCity =
          db.GetNullableString(reader, 51);
        entities.ExistingBankruptcy.ApAttrAddrState =
          db.GetNullableString(reader, 52);
        entities.ExistingBankruptcy.ApAttrAddrZip5 =
          db.GetNullableString(reader, 53);
        entities.ExistingBankruptcy.ApAttrAddrZip4 =
          db.GetNullableString(reader, 54);
        entities.ExistingBankruptcy.ApAttrAddrZip3 =
          db.GetNullableString(reader, 55);
        entities.ExistingBankruptcy.CreatedBy = db.GetString(reader, 56);
        entities.ExistingBankruptcy.CreatedTimestamp =
          db.GetDateTime(reader, 57);
        entities.ExistingBankruptcy.LastUpdatedBy = db.GetString(reader, 58);
        entities.ExistingBankruptcy.LastUpdatedTimestamp =
          db.GetDateTime(reader, 59);
        entities.ExistingBankruptcy.ExpectedBkrpDischargeDate =
          db.GetNullableDate(reader, 60);
        entities.ExistingBankruptcy.Narrative =
          db.GetNullableString(reader, 61);
        entities.ExistingBankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 62);
        entities.ExistingBankruptcy.Populated = true;

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
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    private Bankruptcy bankruptcy;
    private CsePerson csePerson;
    private Common userAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
    }

    private Common lastErrorEntryNo;
    private Array<ErrorsGroup> errors;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    private DateWorkArea current;
    private DateWorkArea zero;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common checkZip;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingBankruptcy.
    /// </summary>
    [JsonPropertyName("existingBankruptcy")]
    public Bankruptcy ExistingBankruptcy
    {
      get => existingBankruptcy ??= new();
      set => existingBankruptcy = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private Bankruptcy existingBankruptcy;
    private CsePerson existingCsePerson;
  }
  #endregion
}
