// Program: SI_ICAS_CSENET_REF_CASE, ID: 372497194, model: 746.
// Short name: SWEICASP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ICAS_CSENET_REF_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIcasCsenetRefCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ICAS_CSENET_REF_CASE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIcasCsenetRefCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIcasCsenetRefCase.
  /// </summary>
  public SiIcasCsenetRefCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	  Developer Name	Description
    // ------------------------------------------------------------
    // 03-01-95  J.W. Hays - MTW	Initial Development
    // 05-08-95  Ken Evans - MTW	Continue development
    // 01-31-96  J. Howard - SRS	Retrofit
    // 09-24-96  Sid Chowdhary		Add link to IIOI.
    // 11/02/96  G. Lofton - MTW	Add new security and removed
    // 05/10/99  C. Scroggins - SRS    Added PFKey to generate acknowledgement, 
    // add Kansas
    //                                 
    // Case #, and set duplicate switch
    // to "Y".
    // 06/16/99  C. Scroggins          Added data flow for ASIN.
    // 02/27/01  SWSRCHF   I00114722   Deleted OLD commented out code.
    //                                 
    // Deleted ZDEL attributes and
    // entities from the Export
    //                                 
    // and Import views.
    //                                 
    // Increased the Group view from
    // 350 to 800 occurrences
    // 10/03/01  T.Bobb PR00128608     Allow a referral to be rejected even 
    // though ks case has
    //                                 
    // been assigned.
    // ---------------------------------------------------------------------------------------
    // 07/25/02  V.Madhira-WR# 020332  The restriction to not allow deactivation
    // of referrals
    //                                  
    // using option 4 on ISTM will be
    // removed.
    // --------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // 07/26/02 V.Madhira   WR# 020332
    // BR: Acknowledgements (A), Cancel (C) and Reminder (M) referral 
    // transactions will appear under option 2 on ISTM.
    // FIX: When option 2 is selected on ISTM, the default action_code is set to
    // 'U' and this action_code value was passed into
    // SI_BULID_CSENET_REFERRAL_LIST  CAB from PRAD.  To implement this BR, if
    // action_code is 'U', change the READEACH for interstate_case to read for
    // action_codes 'U', 'A' , 'C'  and 'M'.
    // ------------------------------------------------------------------------
    // _________________________________________________________
    // 
    // 08-09-06      A. Hockman      pr 00281645  changed so pf17 reg dup will
    // only work for certain transactions.
    //  - - - - - - - - - -  - - - - - - - - - - - - - - - - - - - - -  -
    // ----------------------------------------------------------------------------------------
    // G. Pan     12/18/2007   CQ352
    // 			Added logic to not allow worker to reject/deact more than once.  Also
    //                         there is no reason to allow worker to deactivate 
    // if the record has
    //                         been Rejected, or to reject if the record has 
    // been deactived.
    // ----------------------------------------------------------------------------------------
    // *********************************************
    // * This PRAD retrieves and displays all data *
    // * from the CSENet Case Data Block and some  *
    // * fields from the CSENet Transaction Header.*
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenApId.Number = import.HiddenApId.Number;
    export.InterstateCase.Assign(import.InterstateCase);
    export.Cp.FormattedName = import.Cp.FormattedName;
    export.Ctc.FormattedName = import.Ctc.FormattedName;
    export.Minus.Text1 = import.Minus.Text1;
    export.Plus.Text1 = import.Plus.Text1;
    export.Save.Assign(import.Save);
    export.SaveSubscript.Subscript = import.SaveSubscript.Subscript;
    MoveCommon(import.BuildList, export.BuildList);
    export.ApidInd.Flag = import.ApidInd.Flag;
    export.MiscInd.Flag = import.MiscInd.Flag;
    export.AplocInd.Flag = import.AplocInd.Flag;
    export.PartInd.Flag = import.PartInd.Flag;
    export.SupordInd.Flag = import.SupordInd.Flag;
    export.RegiNewCase.Flag = import.RegiNewCase.Flag;
    export.ToIioi.Flag = import.ToIioi.Flag;
    export.CodeValue.Assign(import.CodeValue);
    MoveOffice(import.Office, export.Office);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.FromComn.Flag = import.FromComn.Flag;
    export.Regi.Number = import.Regi.Number;
    export.HeaderObjTitle1.Text80 = import.HeaderObjTitle1.Text80;
    export.HeaderObjTitle2.Text80 = import.HeaderObjTitle2.Text80;
    export.HeaderObject.Text20 = import.HeaderObject.Text20;
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);
    export.HiddenInterstateCase.OtherFipsState =
      import.HiddenInterstateCase.OtherFipsState;

    if (!import.RefList.IsEmpty)
    {
      export.RefList.Index = -1;

      for(import.RefList.Index = 0; import.RefList.Index < import
        .RefList.Count; ++import.RefList.Index)
      {
        if (!import.RefList.CheckSize())
        {
          break;
        }

        ++export.RefList.Index;
        export.RefList.CheckSize();

        MoveInterstateCase6(import.RefList.Item.Gkey, export.RefList.Update.Gkey);
          
      }

      import.RefList.CheckIndex();
    }

    if (Equal(global.Command, "RETASIN"))
    {
      return;
    }

    if (Equal(global.Command, "FROMCOMN"))
    {
      export.FromComn.Flag = "Y";

      if (!IsEmpty(export.Regi.Number))
      {
        export.InterstateCase.KsCaseId = export.Regi.Number;
      }

      global.Command = "DISPLAY";
    }
    else
    {
      export.FromComn.Flag = "N";
    }

    MoveStandard(import.Standard, export.Standard);

    // ------------------------------------------------------------
    // if the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. now validate
    // ------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";

        // ----------------------------------------------------------------------------------
        // WR# 020332:  When PF15 (Detail) pressed on HIST  by selecting a 
        // CSENET record, the system will flow to this (ICAS) screen with
        // next_tran info.
        //  The Infrastructure ID is passed from HIST to this screen. So, read 
        // Infrastructure record based on next_tran_info 'Infrastructure ID' and
        // then extract the Interstate_case identifiers from the Infrastructure
        // record ( ie denorm_text_12/denorm_numeric_12 and denorm_date).
        // The following code is added as part of this work request.
        //                                                   
        // Vithal (07/25/2002)
        // ------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------
        // WR# 020332:  When PF18 (Detail) pressed on ALRT  by selecting a 
        // CSENET record, the system will flow to this (ICAS) screen with
        // next_tran info.
        //  The Infrastructure ID is passed from ALRT to this screen. So, read 
        // Infrastructure record based on next_tran_info 'Infrastructure ID' and
        // then extract the Interstate_case identifiers from the Infrastructure
        // record ( ie denorm_text_12/denorm_numeric_12 and denorm_date).
        // The following code is added as part of this work request.
        //                                                   
        // Vithal (07/25/2002)
        // ------------------------------------------------------------------------------------
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPQ"))
        {
          if (ReadInfrastructure())
          {
            if (!IsEmpty(entities.Existing.DenormText12) && Verify
              (entities.Existing.DenormText12, " 1234567890") == 0)
            {
              export.InterstateCase.TransSerialNumber =
                StringToNumber(entities.Existing.DenormText12);
              export.InterstateCase.TransactionDate =
                entities.Existing.DenormDate;
            }
            else if (!Equal(entities.Existing.DenormNumeric12, 0))
            {
              export.InterstateCase.TransSerialNumber =
                entities.Existing.DenormNumeric12.GetValueOrDefault();
              export.InterstateCase.TransactionDate =
                entities.Existing.DenormDate;
            }
            else
            {
              ExitState = "SP_INFR_REC_HAS_NO_TRAN_SERAL_NO";

              return;
            }
          }
          else
          {
            ExitState = "SP0000_INVALID_DETAIL_LINK";

            return;
          }
        }
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.HiddenInterstateCase.OtherFipsState =
        import.InterstateCase.OtherFipsState;
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        break;
      case "ASIN":
        break;
      case "RETIIOI":
        break;
      case "IIOI":
        break;
      case "IAPI":
        break;
      case "IAPC":
        break;
      case "REPV":
        break;
      case "RENX":
        break;
      case "SCNX":
        break;
      case "REGDUP":
        break;
      default:
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
    }

    // ------------------------------------------------------------
    // If this is a return from REGI and a case has been created,
    // go deactivate this referral.
    // ------------------------------------------------------------
    if (AsChar(import.RegiNewCase.Flag) == 'Y')
    {
      export.RegiNewCase.Flag = "";
      global.Command = "DEACT";
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "RETURN":
        switch(TrimEnd(export.HiddenNextTranInfo.LastTran ?? ""))
        {
          case "SRPT":
            export.Standard.NextTransaction = "HIST";
            UseScCabNextTranPut2();

            return;
          case "SRPQ":
            export.Standard.NextTransaction = "ALRT";
            UseScCabNextTranPut2();

            return;
          default:
            break;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "RETIIOI":
        UseSiReadCsenetCaseData1();

        switch(AsChar(export.InterstateCase.AssnDeactInd))
        {
          case 'D':
            ExitState = "SI0000_REFERRAL_DEACTIVATED";

            break;
          case 'R':
            ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

            break;
          default:
            break;
        }

        break;
      case "IIOI":
        // ----------------------------------------------------------------------------------------
        // G. Pan     12/18/2007   CQ352
        // 1. When the referral has been deactivated (have a "D" in 
        // assn_deact_ind)
        // and worker tries to reject, send an erro message.
        // 2. When the referral has been rejected once (have a "R" in 
        // assn_deact_ind)
        // and worker tries to reject again , send an error message.
        // ----------------------------------------------------------------------------------------
        if (AsChar(export.InterstateCase.AssnDeactInd) == 'D')
        {
          ExitState = "REFERRAL_DEACTED_CANNOT_REJECT";

          return;
        }

        if (AsChar(export.InterstateCase.AssnDeactInd) == 'R')
        {
          ExitState = "REFERRAL_ALREADY_BEEN_REJECTED";

          return;
        }

        // *******************************************************************
        // 10/03/01 T.Bobb  PR00128608 Allow incoming csenet transaction to be
        // rejected even though ks case id has been assigned.
        // A check has to be made to make sure the serial number
        // does already exist on the interstate request history in the
        // case that this screen was entered from IREC.
        // *******************************************************************
        UseSiCheckHistSerial();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "SI0000_CANT_REJECT_KS_CASE";

          return;
        }

        export.ToIioi.Flag = "Y";
        ExitState = "ECO_LNK_TO_IIOI";

        break;
      case "DISPLAY":
        // *********************************************
        // * If a User has entered a specific CSENet   *
        // * Transaction Serial Number on the CSENet   *
        // * Menu, the PRAD will retrieve and display  *
        // * that Referral.  Otherwise, the PRAD will  *
        // * display CSENet Referrals in the queue.    *
        // *********************************************
        if (export.InterstateCase.TransSerialNumber > 0)
        {
          // *********************************************
          // View referral - option 4 or redisplay upon
          // return from another screen
          // *********************************************
          UseSiReadCsenetCaseData2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveInterstateCase5(export.InterstateCase, export.Save);

            switch(AsChar(export.InterstateCase.AssnDeactInd))
            {
              case 'D':
                ExitState = "SI0000_REFERRAL_DEACTIVATED";

                break;
              case 'R':
                ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

                break;
              default:
                break;
            }

            if (export.Standard.MenuOption != 4)
            {
              if (import.SaveSubscript.Subscript > 0)
              {
                if (import.SaveSubscript.Subscript < export.RefList.Count)
                {
                  export.Plus.Text1 = "+";
                }

                if (import.SaveSubscript.Subscript > 1)
                {
                  export.Minus.Text1 = "-";
                }

                return;
              }
            }

            export.Plus.Text1 = "";
            export.Minus.Text1 = "";
          }
          else
          {
            var field = GetField(export.InterstateCase, "transSerialNumber");

            field.Error = true;
          }

          if (AsChar(export.FromComn.Flag) == 'Y')
          {
            ExitState = "SI0000_EXISTING_KANSAS_CASE";
          }

          if (StringToNumber(export.InterstateCase.ContactZipCode4) == 0)
          {
            export.InterstateCase.ContactZipCode4 = "";
          }

          if (StringToNumber(export.InterstateCase.PaymentZipCode4) == 0)
          {
            export.InterstateCase.PaymentZipCode4 = "";
          }

          if (Equal(export.InterstateCase.SendPaymentsBankAccount,
            "00000000000000000000"))
          {
            export.InterstateCase.SendPaymentsBankAccount = "";
          }

          if (StringToNumber(export.InterstateCase.PaymentFipsState) == 0)
          {
            export.InterstateCase.PaymentFipsState = "";
          }

          if (StringToNumber(export.InterstateCase.PaymentFipsCounty) == 0)
          {
            export.InterstateCase.PaymentFipsCounty = "";
          }

          if (StringToNumber(export.InterstateCase.PaymentFipsLocation) == 0)
          {
            export.InterstateCase.PaymentFipsLocation = "";
          }

          return;
        }

        export.InterstateCase.TransSerialNumber = 0;
        UseSiBuildCsenetReferralList();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Problem report I00114722
          // *** 02/27/01 swsrchf
          // *** start
          if (IsExitState("ACO_NI0000_MORE_ROWS_EXIST"))
          {
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test;
          }

          // *** end
          // *** 02/27/01 swsrchf
          // *** Problem report I00114722
          if (export.InterstateCase.TransSerialNumber == 0)
          {
            export.InterstateCase.ActionCode = "";
          }

          return;
        }

Test:

        if (export.RefList.IsEmpty)
        {
          export.InterstateCase.ActionCode = "";
          ExitState = "NO_MORE_REFERRALS_FOUND";

          return;
        }

        export.RefList.Index = 0;
        export.RefList.CheckSize();

        export.BuildList.Count = 1;
        export.SaveSubscript.Subscript = export.RefList.Index + 1;
        export.InterstateCase.TransSerialNumber =
          export.RefList.Item.Gkey.TransSerialNumber;
        export.InterstateCase.TransactionDate =
          export.RefList.Item.Gkey.TransactionDate;
        UseSiReadCsenetCaseData2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveInterstateCase5(export.InterstateCase, export.Save);

          switch(AsChar(export.InterstateCase.AssnDeactInd))
          {
            case 'D':
              ExitState = "SI0000_REFERRAL_DEACTIVATED";

              break;
            case 'R':
              ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

              break;
            default:
              break;
          }
        }
        else
        {
          var field = GetField(export.InterstateCase, "transSerialNumber");

          field.Error = true;

          export.InterstateCase.ActionCode = "";

          return;
        }

        if (AsChar(export.FromComn.Flag) == 'Y')
        {
          ExitState = "SI0000_EXISTING_KANSAS_CASE";
        }

        if (export.RefList.Index + 1 < export.RefList.Count)
        {
          export.Plus.Text1 = "+";
        }
        else
        {
          export.Plus.Text1 = "";
        }

        if (Equal(export.InterstateCase.SendPaymentsBankAccount,
          "00000000000000000000"))
        {
          export.InterstateCase.SendPaymentsBankAccount = "";
        }

        if (StringToNumber(export.InterstateCase.PaymentFipsCounty) == 0)
        {
          export.InterstateCase.PaymentFipsCounty = "";
        }

        if (StringToNumber(export.InterstateCase.PaymentFipsLocation) == 0)
        {
          export.InterstateCase.PaymentFipsLocation = "";
        }

        if (StringToNumber(export.InterstateCase.ContactZipCode5) == 0)
        {
          export.InterstateCase.ContactZipCode5 = "";
        }

        if (StringToNumber(export.InterstateCase.ContactZipCode4) == 0)
        {
          export.InterstateCase.ContactZipCode4 = "";
        }

        if (export.InterstateCase.TransSerialNumber == 0)
        {
          export.InterstateCase.ActionCode = "";
        }

        break;
      case "RENX":
        switch(import.Standard.MenuOption)
        {
          case 1:
            break;
          case 2:
            break;
          case 3:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_ACTION";

            return;
        }

        if (IsEmpty(export.Plus.Text1))
        {
          ExitState = "NO_MORE_REFERRALS_FOUND";

          return;
        }

        if (import.SaveSubscript.Subscript == export.RefList.Count || import
          .SaveSubscript.Subscript == 0)
        {
          export.InterstateCase.TransSerialNumber =
            import.Save.TransSerialNumber;

          if (!IsEmpty(import.HiddenInterstateCase.InterstateCaseId))
          {
            export.InterstateCase.InterstateCaseId =
              import.HiddenInterstateCase.InterstateCaseId ?? "";
          }
          else
          {
            export.InterstateCase.InterstateCaseId = "";
          }

          if (export.HiddenInterstateCase.OtherFipsState > 0)
          {
            export.InterstateCase.OtherFipsState =
              import.HiddenInterstateCase.OtherFipsState;
          }
          else
          {
            export.InterstateCase.OtherFipsState = 0;
          }

          UseSiBuildCsenetReferralList();

          if (export.RefList.IsEmpty)
          {
            ExitState = "NO_MORE_REFERRALS_FOUND";

            return;
          }

          export.RefList.Index = 0;
          export.RefList.CheckSize();

          export.BuildList.Flag = "Y";
          ++export.BuildList.Count;
          export.SaveSubscript.Subscript = export.RefList.Index + 1;
          export.InterstateCase.TransSerialNumber =
            export.RefList.Item.Gkey.TransSerialNumber;
          export.InterstateCase.TransactionDate =
            export.RefList.Item.Gkey.TransactionDate;
          export.Minus.Text1 = "";

          if (export.RefList.Index + 1 < export.RefList.Count)
          {
            export.Plus.Text1 = "+";
          }
          else
          {
            export.Plus.Text1 = "";
          }

          UseSiReadCsenetCaseData2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveInterstateCase5(export.InterstateCase, export.Save);

            switch(AsChar(export.InterstateCase.AssnDeactInd))
            {
              case 'D':
                ExitState = "SI0000_REFERRAL_DEACTIVATED";

                break;
              case 'R':
                ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

                break;
              default:
                break;
            }
          }
          else
          {
            var field = GetField(export.InterstateCase, "transSerialNumber");

            field.Error = true;

            return;
          }
        }
        else
        {
          export.RefList.Index = import.SaveSubscript.Subscript;
          export.RefList.CheckSize();

          // *********************************************
          // * When a Referral is marked 'Assign' or     *
          // * 'Deactivated', its key (trans_serial_nbr) *
          // * is set to zero in the key table.  This is *
          // * to prevent its being processed again.     *
          // *********************************************
          for(export.RefList.Index = export.RefList.Index; export
            .RefList.Index < export.RefList.Count; ++export.RefList.Index)
          {
            if (!export.RefList.CheckSize())
            {
              break;
            }

            export.SaveSubscript.Subscript = export.RefList.Index + 1;

            if (export.RefList.Item.Gkey.TransSerialNumber == 0)
            {
            }
            else
            {
              export.InterstateCase.TransSerialNumber =
                export.RefList.Item.Gkey.TransSerialNumber;
              export.InterstateCase.TransactionDate =
                export.RefList.Item.Gkey.TransactionDate;

              break;
            }

            if (export.RefList.Index + 1 == Export.RefListGroup.Capacity)
            {
              export.Plus.Text1 = "";
              ExitState = "NO_MORE_REFERRALS_FOUND";

              return;
            }
          }

          export.RefList.CheckIndex();
          UseSiReadCsenetCaseData2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveInterstateCase5(export.InterstateCase, export.Save);

            switch(AsChar(export.InterstateCase.AssnDeactInd))
            {
              case 'D':
                ExitState = "SI0000_REFERRAL_DEACTIVATED";

                break;
              case 'R':
                ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

                break;
              default:
                break;
            }
          }
          else
          {
            ExitState = "CSENET_REFERRAL_CASE_NF";

            var field = GetField(export.InterstateCase, "transSerialNumber");

            field.Error = true;

            return;
          }
        }

        if (export.RefList.Index + 1 < export.RefList.Count)
        {
          export.Plus.Text1 = "+";
        }
        else if (export.RefList.Index + 1 == Export.RefListGroup.Capacity)
        {
          export.Plus.Text1 = "+";
        }
        else
        {
          export.Plus.Text1 = "";
        }

        if (export.RefList.Index >= 1)
        {
          export.Minus.Text1 = "-";
        }
        else if (AsChar(export.BuildList.Flag) == 'Y' && export
          .BuildList.Count > 1)
        {
          export.Minus.Text1 = "-";
        }
        else
        {
          export.Minus.Text1 = "";
        }

        break;
      case "REPV":
        switch(import.Standard.MenuOption)
        {
          case 1:
            break;
          case 2:
            break;
          case 3:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_ACTION";

            return;
        }

        if (IsEmpty(export.Minus.Text1))
        {
          ExitState = "NO_MORE_REFERRALS_FOUND";

          return;
        }

        export.RefList.Index = import.SaveSubscript.Subscript - 2;
        export.RefList.CheckSize();

        if (export.RefList.Index < 0)
        {
          --export.BuildList.Count;

          if (export.BuildList.Count > 0)
          {
          }
          else
          {
            export.Save.TransSerialNumber = 0;
            export.SaveSubscript.Subscript = 0;
            export.RefList.Index = -1;
            export.Minus.Text1 = "";
            ExitState = "NO_MORE_REFERRALS_FOUND";

            return;
          }

          if (!IsEmpty(import.HiddenInterstateCase.InterstateCaseId))
          {
            export.InterstateCase.InterstateCaseId =
              import.HiddenInterstateCase.InterstateCaseId ?? "";
          }
          else
          {
            export.InterstateCase.InterstateCaseId = "";
          }

          if (export.HiddenInterstateCase.OtherFipsState > 0)
          {
            export.InterstateCase.OtherFipsState =
              import.HiddenInterstateCase.OtherFipsState;
          }
          else
          {
            export.InterstateCase.OtherFipsState = 0;
          }

          UseSiBuildPrevCsenetRefList();

          if (export.RefList.IsEmpty)
          {
            ExitState = "NO_MORE_REFERRALS_FOUND";

            return;
          }

          export.RefList.Index = export.RefList.Count - 1;
          export.RefList.CheckSize();

          export.SaveSubscript.Subscript = export.RefList.Index + 1;
          export.InterstateCase.TransSerialNumber =
            export.RefList.Item.Gkey.TransSerialNumber;
          export.InterstateCase.TransactionDate =
            export.RefList.Item.Gkey.TransactionDate;
        }
        else
        {
          if (IsEmpty(import.Minus.Text1))
          {
            ExitState = "NO_MORE_REFERRALS_FOUND";

            return;
          }

          for(export.RefList.Index = export.RefList.Index; export
            .RefList.Index >= 0; --export.RefList.Index)
          {
            if (!export.RefList.CheckSize())
            {
              break;
            }

            if (export.RefList.Item.Gkey.TransSerialNumber == 0)
            {
            }
            else
            {
              export.InterstateCase.TransSerialNumber =
                export.RefList.Item.Gkey.TransSerialNumber;
              export.InterstateCase.TransactionDate =
                export.RefList.Item.Gkey.TransactionDate;
              export.SaveSubscript.Subscript = export.RefList.Index + 1;

              break;
            }

            if (export.RefList.Index < 0)
            {
              if (!IsEmpty(import.HiddenInterstateCase.InterstateCaseId))
              {
                export.InterstateCase.InterstateCaseId =
                  import.HiddenInterstateCase.InterstateCaseId ?? "";
              }
              else
              {
                export.InterstateCase.InterstateCaseId = "";
              }

              if (export.HiddenInterstateCase.OtherFipsState > 0)
              {
                export.InterstateCase.OtherFipsState =
                  import.HiddenInterstateCase.OtherFipsState;
              }
              else
              {
                export.InterstateCase.OtherFipsState = 0;
              }

              UseSiBuildPrevCsenetRefList();

              if (export.RefList.IsEmpty)
              {
                ExitState = "NO_MORE_REFERRALS_FOUND";

                return;
              }

              export.RefList.Index = 0;
              export.RefList.CheckSize();

              export.SaveSubscript.Subscript = export.RefList.Index + 1;
              export.InterstateCase.TransSerialNumber =
                export.RefList.Item.Gkey.TransSerialNumber;
              export.InterstateCase.TransactionDate =
                export.RefList.Item.Gkey.TransactionDate;
            }
          }

          export.RefList.CheckIndex();
        }

        UseSiReadCsenetCaseData2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveInterstateCase5(export.InterstateCase, export.Save);

          switch(AsChar(export.InterstateCase.AssnDeactInd))
          {
            case 'D':
              ExitState = "SI0000_REFERRAL_DEACTIVATED";

              break;
            case 'R':
              ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

              break;
            default:
              break;
          }
        }
        else
        {
          ExitState = "CSENET_REFERRAL_CASE_NF";

          var field = GetField(export.InterstateCase, "transSerialNumber");

          field.Error = true;

          return;
        }

        if (export.RefList.Index + 1 < export.RefList.Count)
        {
          export.Plus.Text1 = "+";
        }
        else
        {
          export.Plus.Text1 = "";
        }

        if (export.RefList.Index >= 1)
        {
          export.Minus.Text1 = "-";
        }
        else
        {
          export.Minus.Text1 = "";
        }

        break;
      case "SCNX":
        if (import.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_REF_PARTICIPANT";
        }
        else if (import.InterstateCase.ApIdentificationInd.
          GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_AP_ID";
        }
        else if (import.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApCurrentInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_CURRENT";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
          }
        }
        else if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else if (import.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "NO_MORE_REFERRAL_SCREENS_FOUND";
        }

        break;
      case "DEACT":
        // ----------------------------------------------------------------------------------------
        // G. Pan     12/18/2007   CQ352
        // 1. When the referral has been rejected (have a "R" in assn_deact_ind)
        // and worker tries to deact, send an error message.
        // 2. When the referral has been deactivated once (have a "D" in 
        // assn_deact_ind)
        // and worker tries to deactivate again, send an error message.
        // ----------------------------------------------------------------------------------------
        if (AsChar(export.InterstateCase.AssnDeactInd) == 'R')
        {
          ExitState = "REFERRAL_REJECTED_CANNOT_DEACT";

          return;
        }

        if (AsChar(export.InterstateCase.AssnDeactInd) == 'D')
        {
          ExitState = "REFERRAL_ALREADY_BEEN_DEACTVATED";

          return;
        }

        // ------------------------------------------------------------------------------------
        // WR# 020332  07/25/2002                 Vithal Madhira
        // BR: The restriction to not allow deactivation of referrals using 
        // option 4 on ISTM will be removed.
        // The following code is commented to implement the above BR.
        // ------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------
        // PR00125413
        // 08/24/01 Tom Bobb
        // ------------------------------------------------------------------------------------
        UseSiCheckOutgoingCsenetTran();

        if (IsExitState("SI0000_CANNOT_DEACT_OG_TRANS"))
        {
          return;
        }

        UseSiUpdateReferralDeactStatus();

        if (IsExitState("CSENET_CASE_NF") || IsExitState("CSENET_CASE_NU") || IsExitState
          ("SI0000_CANNOT_DEACT_OG_TRANS"))
        {
          var field = GetField(export.InterstateCase, "transSerialNumber");

          field.Error = true;

          return;
        }

        if (import.SaveSubscript.Subscript > 0)
        {
          export.RefList.Index = import.SaveSubscript.Subscript - 1;
          export.RefList.CheckSize();

          export.RefList.Update.Gkey.TransSerialNumber = 0;
        }

        switch(AsChar(export.InterstateCase.AssnDeactInd))
        {
          case 'D':
            ExitState = "SI0000_REFERRAL_DEACTIVATED";

            break;
          case 'R':
            ExitState = "SI0000_CSENET_REFERRAL_REJECTED";

            break;
          default:
            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "IAPI":
        if (export.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_AP_ID";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "IAPC":
        if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApCurrentInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_CURRENT";
          }
          else
          {
            ExitState = "CSENET_DATA_DOES_NOT_EXIST";
          }
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "REGDUP":
        local.Ap.Number = export.HiddenApId.Number;

        // ***** pr Ticket #00281645    Anita Hockman.   They dont want reg dup 
        // to work on a LO1 type.     Second change to this 6/19/06 now instead
        // of just not working with LO1 type they only want it to work with ENF-
        // R, EST-R or PAT-R and only on a new transaction type.
        if ((Equal(export.InterstateCase.FunctionalTypeCode, "ENF") || Equal
          (export.InterstateCase.FunctionalTypeCode, "EST") || Equal
          (export.InterstateCase.FunctionalTypeCode, "PAT")) && AsChar
          (export.InterstateCase.ActionCode) == 'R')
        {
          if (IsEmpty(export.Regi.Number))
          {
            if (!IsEmpty(export.InterstateCase.KsCaseId))
            {
              export.Regi.Number = export.InterstateCase.KsCaseId ?? Spaces(10);
            }
          }

          UseSiIcasRegisterDuplicateCase();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0000_DUPLICATE_CASE_REGISTRATI";
          }
        }
        else
        {
          ExitState = "SI0000_REG_DUP_NOT_ALLOWED";
        }

        // *****  end pr Ticket #00281645
        break;
      case "ASIN":
        // *********************************************
        // Set up export views to ASIN.
        // *********************************************
        export.HeaderObject.Text20 = "INTERSTATE REFERRAL";
        export.HeaderObjTitle1.Text80 = "Referral Number:" + NumberToString
          (export.InterstateCase.TransSerialNumber, 4, 12);
        export.HeaderObjTitle2.Text80 = "KS Case Number:";
        export.HeaderObjTitle2.Text80 =
          TrimEnd(export.HeaderObjTitle2.Text80) + (
            export.InterstateCase.KsCaseId ?? "");
        export.HeaderObjTitle2.Text80 =
          TrimEnd(export.HeaderObjTitle2.Text80) + " Other State Case Number: ";
          
        export.HeaderObjTitle2.Text80 =
          TrimEnd(export.HeaderObjTitle2.Text80) + (
            export.InterstateCase.InterstateCaseId ?? "");
        export.InterstateTransaction.EffectiveDate =
          export.InterstateCase.TransactionDate;
        ExitState = "ECO_LNK_TO_ASIN";

        break;
      case "UPDATE":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Flag = source.Flag;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
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
    target.DateReceived = source.DateReceived;
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
    target.AssnDeactInd = source.AssnDeactInd;
    target.Memo = source.Memo;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
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
    target.DateReceived = source.DateReceived;
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
    target.AssnDeactInd = source.AssnDeactInd;
    target.Memo = source.Memo;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
    target.InitiatingDocketNumber = source.InitiatingDocketNumber;
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
    target.NondisclosureFinding = source.NondisclosureFinding;
    target.RespondingDocketNumber = source.RespondingDocketNumber;
    target.StateWithCej = source.StateWithCej;
    target.PaymentFipsCounty = source.PaymentFipsCounty;
    target.PaymentFipsState = source.PaymentFipsState;
    target.PaymentFipsLocation = source.PaymentFipsLocation;
    target.ContactAreaCode = source.ContactAreaCode;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
  }

  private static void MoveInterstateCase4(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.KsCaseId = source.KsCaseId;
    target.InterstateCaseId = source.InterstateCaseId;
  }

  private static void MoveInterstateCase5(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.TransSerialNumber = source.TransSerialNumber;
    target.ActionCode = source.ActionCode;
    target.InterstateCaseId = source.InterstateCaseId;
  }

  private static void MoveInterstateCase6(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase7(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
    target.CaseType = source.CaseType;
  }

  private static void MoveInterstateCase8(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.AssnDeactInd = source.AssnDeactInd;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveRefList1(SiBuildPrevCsenetRefList.Export.
    RefListGroup source, Export.RefListGroup target)
  {
    MoveInterstateCase6(source.G, target.Gkey);
  }

  private static void MoveRefList2(SiBuildCsenetReferralList.Export.
    RefListGroup source, Export.RefListGroup target)
  {
    MoveInterstateCase6(source.G, target.Gkey);
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private void UseSiBuildCsenetReferralList()
  {
    var useImport = new SiBuildCsenetReferralList.Import();
    var useExport = new SiBuildCsenetReferralList.Export();

    useImport.ServiceProvider.Assign(export.ServiceProvider);
    useImport.OfficeServiceProvider.Assign(export.OfficeServiceProvider);
    MoveOffice(export.Office, useImport.Office);
    MoveInterstateCase4(export.InterstateCase, useImport.InterstateCase);

    Call(SiBuildCsenetReferralList.Execute, useImport, useExport);

    useExport.RefList.CopyTo(export.RefList, MoveRefList2);
  }

  private void UseSiBuildPrevCsenetRefList()
  {
    var useImport = new SiBuildPrevCsenetRefList.Import();
    var useExport = new SiBuildPrevCsenetRefList.Export();

    useImport.ServiceProvider.Assign(export.ServiceProvider);
    useImport.OfficeServiceProvider.Assign(export.OfficeServiceProvider);
    MoveOffice(export.Office, useImport.Office);
    MoveInterstateCase3(export.InterstateCase, useImport.InterstateCase);

    Call(SiBuildPrevCsenetRefList.Execute, useImport, useExport);

    export.SaveSubscript.Subscript = useExport.HiddenMax.Subscript;
    useExport.RefList.CopyTo(export.RefList, MoveRefList1);
  }

  private void UseSiCheckApCurrHist()
  {
    var useImport = new SiCheckApCurrHist.Import();
    var useExport = new SiCheckApCurrHist.Export();

    MoveInterstateCase6(export.InterstateCase, useImport.InterstateCase);

    Call(SiCheckApCurrHist.Execute, useImport, useExport);

    local.ApCurrentInd.Flag = useExport.ApCurrentInd.Flag;
    local.ApHistoryInd.Flag = useExport.ApHistoryInd.Flag;
  }

  private void UseSiCheckHistSerial()
  {
    var useImport = new SiCheckHistSerial.Import();
    var useExport = new SiCheckHistSerial.Export();

    MoveInterstateCase6(import.InterstateCase, useImport.InterstateCase);

    Call(SiCheckHistSerial.Execute, useImport, useExport);
  }

  private void UseSiCheckOutgoingCsenetTran()
  {
    var useImport = new SiCheckOutgoingCsenetTran.Import();
    var useExport = new SiCheckOutgoingCsenetTran.Export();

    MoveInterstateCase6(import.InterstateCase, useImport.InterstateCase);

    Call(SiCheckOutgoingCsenetTran.Execute, useImport, useExport);
  }

  private void UseSiIcasRegisterDuplicateCase()
  {
    var useImport = new SiIcasRegisterDuplicateCase.Import();
    var useExport = new SiIcasRegisterDuplicateCase.Export();

    useImport.CsePerson.Number = local.Ap.Number;
    useImport.Case1.Number = export.Regi.Number;
    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);

    Call(SiIcasRegisterDuplicateCase.Execute, useImport, useExport);

    export.Regi.Number = useExport.Case1.Number;
    MoveInterstateCase2(useExport.InterstateCase, export.InterstateCase);
    MoveInterstateCase7(useExport.From, export.InterstateCase);
  }

  private void UseSiReadCsenetCaseData1()
  {
    var useImport = new SiReadCsenetCaseData.Import();
    var useExport = new SiReadCsenetCaseData.Export();

    MoveInterstateCase1(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadCsenetCaseData.Execute, useImport, useExport);

    export.MiscInd.Flag = useExport.MiscInd.Flag;
    export.SupordInd.Flag = useExport.SupordInd.Flag;
    export.AplocInd.Flag = useExport.AplocInd.Flag;
    export.ApidInd.Flag = useExport.ApidInd.Flag;
    export.PartInd.Flag = useExport.PartInd.Flag;
    export.Ctc.FormattedName = useExport.Ctc.FormattedName;
    MoveInterstateCase2(useExport.InterstateCase, export.InterstateCase);
    export.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseSiReadCsenetCaseData2()
  {
    var useImport = new SiReadCsenetCaseData.Import();
    var useExport = new SiReadCsenetCaseData.Export();

    MoveInterstateCase1(export.InterstateCase, useImport.InterstateCase);

    Call(SiReadCsenetCaseData.Execute, useImport, useExport);

    export.Ctc.FormattedName = useExport.Ctc.FormattedName;
    MoveInterstateCase2(useExport.InterstateCase, export.InterstateCase);
    export.ApidInd.Flag = useExport.ApidInd.Flag;
    export.AplocInd.Flag = useExport.AplocInd.Flag;
    export.MiscInd.Flag = useExport.MiscInd.Flag;
    export.SupordInd.Flag = useExport.SupordInd.Flag;
    export.PartInd.Flag = useExport.PartInd.Flag;
    export.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseSiUpdateReferralDeactStatus()
  {
    var useImport = new SiUpdateReferralDeactStatus.Import();
    var useExport = new SiUpdateReferralDeactStatus.Export();

    MoveInterstateCase6(import.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateReferralDeactStatus.Execute, useImport, useExport);

    MoveInterstateCase8(useExport.InterstateCase, export.InterstateCase);
  }

  private bool ReadInfrastructure()
  {
    entities.Existing.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.DenormNumeric12 = db.GetNullableInt64(reader, 1);
        entities.Existing.DenormText12 = db.GetNullableString(reader, 2);
        entities.Existing.DenormDate = db.GetNullableDate(reader, 3);
        entities.Existing.Populated = true;
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
    /// <summary>A RefListGroup group.</summary>
    [Serializable]
    public class RefListGroup
    {
      /// <summary>
      /// A value of Gkey.
      /// </summary>
      [JsonPropertyName("gkey")]
      public InterstateCase Gkey
      {
        get => gkey ??= new();
        set => gkey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 800;

      private InterstateCase gkey;
    }

    /// <summary>
    /// A value of HiddenInterstateCase.
    /// </summary>
    [JsonPropertyName("hiddenInterstateCase")]
    public InterstateCase HiddenInterstateCase
    {
      get => hiddenInterstateCase ??= new();
      set => hiddenInterstateCase = value;
    }

    /// <summary>
    /// A value of ToIioi.
    /// </summary>
    [JsonPropertyName("toIioi")]
    public Common ToIioi
    {
      get => toIioi ??= new();
      set => toIioi = value;
    }

    /// <summary>
    /// A value of RegiNewCase.
    /// </summary>
    [JsonPropertyName("regiNewCase")]
    public Common RegiNewCase
    {
      get => regiNewCase ??= new();
      set => regiNewCase = value;
    }

    /// <summary>
    /// A value of Regi.
    /// </summary>
    [JsonPropertyName("regi")]
    public Case1 Regi
    {
      get => regi ??= new();
      set => regi = value;
    }

    /// <summary>
    /// A value of MiscInd.
    /// </summary>
    [JsonPropertyName("miscInd")]
    public Common MiscInd
    {
      get => miscInd ??= new();
      set => miscInd = value;
    }

    /// <summary>
    /// A value of SupordInd.
    /// </summary>
    [JsonPropertyName("supordInd")]
    public Common SupordInd
    {
      get => supordInd ??= new();
      set => supordInd = value;
    }

    /// <summary>
    /// A value of AplocInd.
    /// </summary>
    [JsonPropertyName("aplocInd")]
    public Common AplocInd
    {
      get => aplocInd ??= new();
      set => aplocInd = value;
    }

    /// <summary>
    /// A value of ApidInd.
    /// </summary>
    [JsonPropertyName("apidInd")]
    public Common ApidInd
    {
      get => apidInd ??= new();
      set => apidInd = value;
    }

    /// <summary>
    /// A value of PartInd.
    /// </summary>
    [JsonPropertyName("partInd")]
    public Common PartInd
    {
      get => partInd ??= new();
      set => partInd = value;
    }

    /// <summary>
    /// A value of BuildList.
    /// </summary>
    [JsonPropertyName("buildList")]
    public Common BuildList
    {
      get => buildList ??= new();
      set => buildList = value;
    }

    /// <summary>
    /// A value of Ctc.
    /// </summary>
    [JsonPropertyName("ctc")]
    public CsePersonsWorkSet Ctc
    {
      get => ctc ??= new();
      set => ctc = value;
    }

    /// <summary>
    /// A value of Cp.
    /// </summary>
    [JsonPropertyName("cp")]
    public CsePersonsWorkSet Cp
    {
      get => cp ??= new();
      set => cp = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
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
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public InterstateCase Save
    {
      get => save ??= new();
      set => save = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of HiddenApId.
    /// </summary>
    [JsonPropertyName("hiddenApId")]
    public CsePersonsWorkSet HiddenApId
    {
      get => hiddenApId ??= new();
      set => hiddenApId = value;
    }

    /// <summary>
    /// A value of FromComn.
    /// </summary>
    [JsonPropertyName("fromComn")]
    public Common FromComn
    {
      get => fromComn ??= new();
      set => fromComn = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle1.
    /// </summary>
    [JsonPropertyName("headerObjTitle1")]
    public SpTextWorkArea HeaderObjTitle1
    {
      get => headerObjTitle1 ??= new();
      set => headerObjTitle1 = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle2.
    /// </summary>
    [JsonPropertyName("headerObjTitle2")]
    public SpTextWorkArea HeaderObjTitle2
    {
      get => headerObjTitle2 ??= new();
      set => headerObjTitle2 = value;
    }

    /// <summary>
    /// A value of HeaderObject.
    /// </summary>
    [JsonPropertyName("headerObject")]
    public SpTextWorkArea HeaderObject
    {
      get => headerObject ??= new();
      set => headerObject = value;
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

    private InterstateCase hiddenInterstateCase;
    private Common toIioi;
    private Common regiNewCase;
    private Case1 regi;
    private Common miscInd;
    private Common supordInd;
    private Common aplocInd;
    private Common apidInd;
    private Common partInd;
    private Common buildList;
    private CsePersonsWorkSet ctc;
    private CsePersonsWorkSet cp;
    private WorkArea minus;
    private WorkArea plus;
    private Common saveSubscript;
    private InterstateCase save;
    private InterstateCase interstateCase;
    private Standard standard;
    private Array<RefListGroup> refList;
    private NextTranInfo hiddenNextTranInfo;
    private CodeValue codeValue;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CsePersonsWorkSet hiddenApId;
    private Common fromComn;
    private SpTextWorkArea headerObjTitle1;
    private SpTextWorkArea headerObjTitle2;
    private SpTextWorkArea headerObject;
    private ServiceProviderAddress serviceProviderAddress;
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
      /// A value of Gkey.
      /// </summary>
      [JsonPropertyName("gkey")]
      public InterstateCase Gkey
      {
        get => gkey ??= new();
        set => gkey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 800;

      private InterstateCase gkey;
    }

    /// <summary>
    /// A value of HiddenInterstateCase.
    /// </summary>
    [JsonPropertyName("hiddenInterstateCase")]
    public InterstateCase HiddenInterstateCase
    {
      get => hiddenInterstateCase ??= new();
      set => hiddenInterstateCase = value;
    }

    /// <summary>
    /// A value of ToIioi.
    /// </summary>
    [JsonPropertyName("toIioi")]
    public Common ToIioi
    {
      get => toIioi ??= new();
      set => toIioi = value;
    }

    /// <summary>
    /// A value of RegiNewCase.
    /// </summary>
    [JsonPropertyName("regiNewCase")]
    public Common RegiNewCase
    {
      get => regiNewCase ??= new();
      set => regiNewCase = value;
    }

    /// <summary>
    /// A value of Regi.
    /// </summary>
    [JsonPropertyName("regi")]
    public Case1 Regi
    {
      get => regi ??= new();
      set => regi = value;
    }

    /// <summary>
    /// A value of MiscInd.
    /// </summary>
    [JsonPropertyName("miscInd")]
    public Common MiscInd
    {
      get => miscInd ??= new();
      set => miscInd = value;
    }

    /// <summary>
    /// A value of SupordInd.
    /// </summary>
    [JsonPropertyName("supordInd")]
    public Common SupordInd
    {
      get => supordInd ??= new();
      set => supordInd = value;
    }

    /// <summary>
    /// A value of AplocInd.
    /// </summary>
    [JsonPropertyName("aplocInd")]
    public Common AplocInd
    {
      get => aplocInd ??= new();
      set => aplocInd = value;
    }

    /// <summary>
    /// A value of ApidInd.
    /// </summary>
    [JsonPropertyName("apidInd")]
    public Common ApidInd
    {
      get => apidInd ??= new();
      set => apidInd = value;
    }

    /// <summary>
    /// A value of PartInd.
    /// </summary>
    [JsonPropertyName("partInd")]
    public Common PartInd
    {
      get => partInd ??= new();
      set => partInd = value;
    }

    /// <summary>
    /// A value of BuildList.
    /// </summary>
    [JsonPropertyName("buildList")]
    public Common BuildList
    {
      get => buildList ??= new();
      set => buildList = value;
    }

    /// <summary>
    /// A value of Ctc.
    /// </summary>
    [JsonPropertyName("ctc")]
    public CsePersonsWorkSet Ctc
    {
      get => ctc ??= new();
      set => ctc = value;
    }

    /// <summary>
    /// A value of Cp.
    /// </summary>
    [JsonPropertyName("cp")]
    public CsePersonsWorkSet Cp
    {
      get => cp ??= new();
      set => cp = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
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
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public InterstateCase Save
    {
      get => save ??= new();
      set => save = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of HiddenApId.
    /// </summary>
    [JsonPropertyName("hiddenApId")]
    public CsePersonsWorkSet HiddenApId
    {
      get => hiddenApId ??= new();
      set => hiddenApId = value;
    }

    /// <summary>
    /// A value of FromComn.
    /// </summary>
    [JsonPropertyName("fromComn")]
    public Common FromComn
    {
      get => fromComn ??= new();
      set => fromComn = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle1.
    /// </summary>
    [JsonPropertyName("headerObjTitle1")]
    public SpTextWorkArea HeaderObjTitle1
    {
      get => headerObjTitle1 ??= new();
      set => headerObjTitle1 = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle2.
    /// </summary>
    [JsonPropertyName("headerObjTitle2")]
    public SpTextWorkArea HeaderObjTitle2
    {
      get => headerObjTitle2 ??= new();
      set => headerObjTitle2 = value;
    }

    /// <summary>
    /// A value of HeaderObject.
    /// </summary>
    [JsonPropertyName("headerObject")]
    public SpTextWorkArea HeaderObject
    {
      get => headerObject ??= new();
      set => headerObject = value;
    }

    /// <summary>
    /// A value of InterstateTransaction.
    /// </summary>
    [JsonPropertyName("interstateTransaction")]
    public OfficeServiceProvider InterstateTransaction
    {
      get => interstateTransaction ??= new();
      set => interstateTransaction = value;
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

    private InterstateCase hiddenInterstateCase;
    private Common toIioi;
    private Common regiNewCase;
    private Case1 regi;
    private Common miscInd;
    private Common supordInd;
    private Common aplocInd;
    private Common apidInd;
    private Common partInd;
    private Common buildList;
    private CsePersonsWorkSet ctc;
    private CsePersonsWorkSet cp;
    private WorkArea minus;
    private WorkArea plus;
    private Common saveSubscript;
    private InterstateCase save;
    private InterstateCase interstateCase;
    private Standard standard;
    private Array<RefListGroup> refList;
    private NextTranInfo hiddenNextTranInfo;
    private CodeValue codeValue;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CsePersonsWorkSet hiddenApId;
    private Common fromComn;
    private SpTextWorkArea headerObjTitle1;
    private SpTextWorkArea headerObjTitle2;
    private SpTextWorkArea headerObject;
    private OfficeServiceProvider interstateTransaction;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApCurrentInd.
    /// </summary>
    [JsonPropertyName("apCurrentInd")]
    public Common ApCurrentInd
    {
      get => apCurrentInd ??= new();
      set => apCurrentInd = value;
    }

    /// <summary>
    /// A value of ApHistoryInd.
    /// </summary>
    [JsonPropertyName("apHistoryInd")]
    public Common ApHistoryInd
    {
      get => apHistoryInd ??= new();
      set => apHistoryInd = value;
    }

    private CsePerson ap;
    private Common apCurrentInd;
    private Common apHistoryInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Infrastructure Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Infrastructure existing;
  }
#endregion
}
