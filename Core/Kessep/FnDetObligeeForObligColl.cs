// Program: FN_DET_OBLIGEE_FOR_OBLIG_COLL, ID: 372544016, model: 746.
// Short name: SWE01608
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
/// A program: FN_DET_OBLIGEE_FOR_OBLIG_COLL.
/// </para>
/// <para>
/// RESP: FNCLMGMT
/// This action block will determine the Obligee for a Collection.
/// </para>
/// </summary>
[Serializable]
public partial class FnDetObligeeForObligColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DET_OBLIGEE_FOR_OBLIG_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetObligeeForObligColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetObligeeForObligColl.
  /// </summary>
  public FnDetObligeeForObligColl(IContext context, Import import, Export export)
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
    // Date	By		Description
    // ---------------------------------------------
    // 9/16/99	Fangman	  Corrected/Restructured for:
    // PR 73627          Implemented new business rules for determining the 
    // Obligee.
    // PR 74158 & 74075  When reading for Case and Obligee to exclude those 
    // cases that are closed.
    // PR 74069          For collections applied to 'NC' do not create a 
    // disbursement, instead set the collection to No processing needed.
    // PR 78963          Change the way the AR is determined for child support 
    // collections to 1st find the case and then find the AR.
    // PR 83163          Read for ARs on closed cases if the collection is non-
    // cash.
    // 01/17/00  PR 84462  Change read for spousal support to not use the debt 
    // detail due date.
    // 01/17/00  PR 84869  Add logic to error out the collection if the AR is 
    // JJA - Juvenile Justice Authority.
    // 05/04/00  PRWORA  Added excess ura ind to import persistent collection.
    // ---------------------------------------------
    // -----------------------------------------------------------------
    // 10/22/99 - SWSRKXD PR#77874
    // NC collections will never be disbursed.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 06/15/01  Fangman  WR 010507
    //      Added the AR Number to the entity view of the Collection table.
    // 06/11/02  Fangman  PR 125226
    //      Added code to check for new "State of Kansas" AR numbers.  This code
    // will go in w/ WR 010507
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 06/24/03  Fangman  WR 302055
    //      Made changes for applying certain collections in error to potential 
    // recovery obligations
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 12/04/03  Fangman  PR 194026
    //      Change reads to read eachs and sort on case status descending so 
    // that open cases are used prior to closed cases.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 01/12/04  Fangman  PR 197465
    //      Change read eachs to reads so that only open cases are looked at & 
    // if no open case (at any time) is found then closed cases will be looked
    // at.
    // -----------------------------------------------------------------
    local.TheStatesPersonNumber.Number = "000000017O";
    local.TheStatesPersonNumber2.Number = "000000181O";
    local.TheStatesPersonNumber3.Number = "000000182O";
    local.JuvenileJusticeAuthority.Number = "000004029O";
    local.NumericString.Text10 = "0123456789";
    export.Obligee.Number = "";
    export.DebtDetail.DueDt = local.Initialized.Date;
    export.StateCollectionInd.Flag = "N";
    local.EabFileHandling.Action = "WRITE";
    export.AppliedApCollToRcvInd.Flag = "N";

    if (AsChar(import.DisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "  *******  In Det Obligee w/ AP: " + import
        .Obligor.Number + " coll id: " + NumberToString
        (import.PerCollection.SystemGeneratedIdentifier, 15);
      UseCabErrorReport();
    }

    if (AsChar(import.PerCollection.AdjustedInd) == 'Y')
    {
      // If the obligation is adjusted then use the AR on the original 
      // disbursement.
      if (ReadCsePerson1())
      {
        export.Obligee.Number = entities.Ar.Number;

        return;
      }
      else
      {
        ExitState = "FN0000_DISB_CREDIT_NF_FOR_ADJUST";

        return;
      }
    }

    if (ReadCsePerson3())
    {
      // Continue
    }
    else
    {
      ExitState = "FN0000_SUPP_PERSON_NF";

      return;
    }

    if (import.ObligationType.SystemGeneratedIdentifier == import
      .Hardcode.HardcodeVoluntary.SystemGeneratedIdentifier)
    {
      // All voluntary obligations will use the current date to find the most 
      // recent Obligee.
      if (ReadCaseCsePersonApplicantRecipientChild())
      {
        export.Case1.Number = entities.Case1.Number;
        export.Obligee.Number = entities.Ar.Number;
        export.Child.Number = entities.Supported2.Number;

        if (AsChar(entities.Case1.Status) == 'C' && AsChar
          (import.PerCashReceiptType.CategoryIndicator) == 'C')
        {
          ExitState = "FN0000_AR_IS_ON_CLOSED_CASE";

          return;
        }
      }
      else if (ReadCaseApplicantRecipientCsePerson())
      {
        export.Case1.Number = entities.Case1.Number;
        export.Obligee.Number = entities.Ar.Number;

        if (AsChar(entities.Case1.Status) == 'C' && AsChar
          (import.PerCashReceiptType.CategoryIndicator) == 'C')
        {
          ExitState = "FN0000_AR_IS_ON_CLOSED_CASE";

          return;
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "Voluntary - case nf for supported person # " + entities
          .Supported2.Number;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }
    }
    else
    {
      // All non-voluntary obligations will have a debt_detail due_date 
      // available to use to find the Obligee.
      if (ReadDebtDetail())
      {
        export.DebtDetail.DueDt = entities.DebtDetail.DueDt;
      }
      else
      {
        ExitState = "FN0211_DEBT_DETAIL_NF";

        return;
      }

      // If the obligation is for spousal support then the supported person is 
      // the AR.
      if (import.ObligationType.SystemGeneratedIdentifier == import
        .Hardcode.HardcodeSpousalSupport.SystemGeneratedIdentifier || import
        .ObligationType.SystemGeneratedIdentifier == import
        .Hardcode.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier)
      {
        export.Obligee.Number = entities.Supported2.Number;

        if (AsChar(import.PerCashReceiptType.CategoryIndicator) == 'C')
        {
          // For CASH cash receipts the case must be OPEN.
          // If found on a closed case then check for RCV
          foreach(var item in ReadCaseApplicantRecipient2())
          {
            if (AsChar(entities.Case1.Status) == 'C')
            {
              UseFnB650ApplyToPotRcv2();

              // If there are no potential recoveries to apply the money to then
              // set the error msg.
              if (AsChar(export.AppliedApCollToRcvInd.Flag) != 'Y')
              {
                ExitState = "FN0000_AR_IS_ON_CLOSED_CASE";
              }

              return;
            }
            else
            {
              // Continue
              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "  **** Found open case in new Read Each. SS<<";
                UseCabErrorReport();
              }

              goto Test;
            }
          }

          local.EabReportSend.RptDetail = "Spousal support - case nf";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }

          ExitState = "CASE_NF";

          return;
        }
        else
        {
          // For NON-CASH cash receipts the case may be OPEN or CLOSE.
          if (ReadCaseApplicantRecipient1())
          {
            // Continue
          }
          else
          {
            local.EabReportSend.RptDetail = "Spousal support - case nf";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }

            ExitState = "CASE_NF";

            return;
          }
        }

        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        // Supported person is child.
        export.Child.Number = entities.Supported2.Number;

        // Try to find the case by the debt detail due date.
        if (AsChar(import.PerCashReceiptType.CategoryIndicator) == 'C')
        {
          // For CASH cash receipts first look for an OPEN case then a CLOSED 
          // case.
          if (ReadCase2())
          {
            export.Case1.Number = entities.Case1.Number;

            // Now find the AR for the case & debt detail due date.
            if (ReadCsePersonApplicantRecipient1())
            {
              export.Obligee.Number = entities.Ar.Number;

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "  **** Found open case in new Read Each. CS<<";
                UseCabErrorReport();
              }

              goto Test;
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Child support - AR nf on Debt Detail Due Date.";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }

            // Continue
          }
          else
          {
            // The case was not found so try to find the case by going BACK IN 
            // TIME from the debt detail due date using the child's case role
            // end date.
            foreach(var item in ReadCaseChild6())
            {
              export.Case1.Number = entities.Case1.Number;

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "  **** Found open case in new Read Each. CS back in time<<";
                UseCabErrorReport();
              }

              // Now find the AR for the case & child's end date.
              if (ReadCsePersonApplicantRecipient2())
              {
                export.Obligee.Number = entities.Ar.Number;

                goto Test;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Child support - AR nf going backward in time.";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }

                goto Test;
              }
            }

            // The case was not found so try to find the case by going FORWARD 
            // IN TIME from the debt detail due date using the child's case role
            // start date.
            foreach(var item in ReadCaseChild4())
            {
              export.Case1.Number = entities.Case1.Number;

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "  **** Found open case in new Read Each. CS forward in time<<";
                  
                UseCabErrorReport();
              }

              // Now find the AR for the case & child's start date.
              if (ReadCsePersonApplicantRecipient3())
              {
                export.Obligee.Number = entities.Ar.Number;

                goto Test;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Child support - AR nf going forward in time.";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }

                goto Test;
              }
            }
          }

          // If found on a closed case then check for RCV
          if (ReadCase1())
          {
            export.Case1.Number = entities.Case1.Number;

            // Now find the AR for the case & debt detail due date.
            if (ReadCsePersonApplicantRecipient1())
            {
              export.Obligee.Number = entities.Ar.Number;

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "  **** Found AR on closed case in new Read. CS<<";
                UseCabErrorReport();
              }

              UseFnB650ApplyToPotRcv1();

              // If there are no potential recoveries to apply the money to then
              // set the error msg.
              if (AsChar(export.AppliedApCollToRcvInd.Flag) != 'Y')
              {
                ExitState = "FN0000_AR_IS_ON_CLOSED_CASE";
              }

              return;
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Child support - AR nf on Debt Detail Due Date.";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }

            // Continue
          }
          else
          {
            // The case was not found so try to find the case by going BACK IN 
            // TIME from the debt detail due date using the child's case role
            // end date.
            foreach(var item in ReadCaseChild5())
            {
              export.Case1.Number = entities.Case1.Number;

              // Now find the AR for the case & child's end date.
              if (ReadCsePersonApplicantRecipient2())
              {
                export.Obligee.Number = entities.Ar.Number;

                if (AsChar(import.DisplayInd.Flag) == 'Y')
                {
                  local.EabReportSend.RptDetail =
                    "  **** Found AR on closed case in new Read. CS back in time<<<<";
                    
                  UseCabErrorReport();
                }

                UseFnB650ApplyToPotRcv1();

                // If there are no potential recoveries to apply the money to 
                // then set the error msg.
                if (AsChar(export.AppliedApCollToRcvInd.Flag) != 'Y')
                {
                  ExitState = "FN0000_AR_IS_ON_CLOSED_CASE";
                }

                return;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Child support - AR nf going backward in time.";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }

            // The case was not found so try to find the case by going FORWARD 
            // IN TIME from the debt detail due date using the child's case role
            // start date.
            foreach(var item in ReadCaseChild3())
            {
              export.Case1.Number = entities.Case1.Number;

              // Now find the AR for the case & child's start date.
              if (ReadCsePersonApplicantRecipient3())
              {
                export.Obligee.Number = entities.Ar.Number;

                if (AsChar(import.DisplayInd.Flag) == 'Y')
                {
                  local.EabReportSend.RptDetail =
                    "  **** Found AR on closed case in new Read Each. CS forward in time<<<<";
                    
                  UseCabErrorReport();
                }

                UseFnB650ApplyToPotRcv1();

                // If there are no potential recoveries to apply the money to 
                // then set the error msg.
                if (AsChar(export.AppliedApCollToRcvInd.Flag) != 'Y')
                {
                  ExitState = "FN0000_AR_IS_ON_CLOSED_CASE";
                }

                return;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Child support - AR nf going forward in time.";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }
          }
        }
        else
        {
          // For NON-CASH cash receipts the case may be OPEN or CLOSE.
          if (ReadCase3())
          {
            export.Case1.Number = entities.Case1.Number;

            // Now find the AR for the case & debt detail due date.
            if (ReadCsePersonApplicantRecipient1())
            {
              export.Obligee.Number = entities.Ar.Number;
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Child support - AR nf on Debt Detail Due Date.";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }
          }
          else
          {
            // The case was not found so try to find the case by going BACK IN 
            // TIME from the debt detail due date using the child's case role
            // end date.
            if (ReadCaseChild2())
            {
              export.Case1.Number = entities.Case1.Number;

              // Now find the AR for the case & child's end date.
              if (ReadCsePersonApplicantRecipient2())
              {
                export.Obligee.Number = entities.Ar.Number;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Child support - AR nf going backward in time.";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }
              }

              goto Test;
            }

            // The case was not found so try to find the case by going FORWARD 
            // IN TIME from the debt detail due date using the child's case role
            // start date.
            if (ReadCaseChild1())
            {
              export.Case1.Number = entities.Case1.Number;

              // Now find the AR for the case & child's start date.
              if (ReadCsePersonApplicantRecipient3())
              {
                export.Obligee.Number = entities.Ar.Number;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Child support - AR nf going forward in time.";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }
              }

              goto Test;
            }

            local.EabReportSend.RptDetail = "Child support - case nf";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }

            return;
          }
        }
      }
    }

Test:

    if (!IsEmpty(export.Obligee.Number))
    {
      if (Equal(entities.Ar.Number, local.TheStatesPersonNumber.Number) || Equal
        (entities.Ar.Number, local.TheStatesPersonNumber2.Number) || Equal
        (entities.Ar.Number, local.TheStatesPersonNumber3.Number))
      {
        ExitState = "FN0000_STATE_CANT_BE_AR_ON_NA";

        // -----------------------------------------------------------------
        // 10/22/99 - SWSRKXD PR#77874
        // NC collections are errored off in B650 Pstep.They will never
        // make it this far. Code has been disabled and views left here
        // just incase business change their mind.
        // -----------------------------------------------------------------
      }
      else if (Equal(entities.Ar.Number, local.JuvenileJusticeAuthority.Number))
      {
        ExitState = "FN0000_JJA_CANT_BE_AN_AR";
      }
      else if (Verify(entities.Ar.Number, local.NumericString.Text10) > 0)
      {
        if (entities.Ar.Populated)
        {
          if (ReadCsePerson2())
          {
            if (Equal(entities.ArOrganization.OrganizationName,
              "STATE OF KANSAS") && AsChar(entities.ArOrganization.Type1) == 'O'
              )
            {
              ExitState = "FN0000_STATE_CANT_BE_AR_ON_NA";
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "AR nf reading for Organization name for AR " + entities
              .Ar.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";
            }
          }
        }
      }
    }
    else
    {
      ExitState = "FN0000_COULD_NOT_DET_OBLGEE";
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB650ApplyToPotRcv1()
  {
    var useImport = new FnB650ApplyToPotRcv.Import();
    var useExport = new FnB650ApplyToPotRcv.Export();

    useImport.Ar.Number = entities.Ar.Number;
    useImport.ExpCollsEligForRcvCnt.Count = import.ExpCollsEligForRcvCnt.Count;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptType.CategoryIndicator =
      import.PerCashReceiptType.CategoryIndicator;
    useImport.Per.Assign(import.PerCollection);
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Ap.Number = import.Obligor.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    useImport.ApplyApCollToRcv.Flag = import.ApplyApCollToRcvInd.Flag;
    useImport.TheStatesPersonNumber.Number = local.TheStatesPersonNumber.Number;
    useImport.TheStatesPersonNumber2.Number =
      local.TheStatesPersonNumber2.Number;
    useImport.TheStatesPersonNumber3.Number =
      local.TheStatesPersonNumber3.Number;
    useImport.JuvenileJusticeAuthority.Number =
      local.JuvenileJusticeAuthority.Number;

    Call(FnB650ApplyToPotRcv.Execute, useImport, useExport);

    import.ExpCollsEligForRcvCnt.Count = useImport.ExpCollsEligForRcvCnt.Count;
    import.PerCollection.Assign(useImport.Per);
    export.Abort.Flag = useExport.Abort.Flag;
    export.AppliedApCollToRcvInd.Flag = useExport.AppliedApCollToRcvInd.Flag;
  }

  private void UseFnB650ApplyToPotRcv2()
  {
    var useImport = new FnB650ApplyToPotRcv.Import();
    var useExport = new FnB650ApplyToPotRcv.Export();

    useImport.ExpCollsEligForRcvCnt.Count = import.ExpCollsEligForRcvCnt.Count;
    useImport.Per.Assign(import.PerCollection);
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptType.CategoryIndicator =
      import.PerCashReceiptType.CategoryIndicator;
    useImport.Ar.Number = export.Obligee.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.TheStatesPersonNumber.Number = local.TheStatesPersonNumber.Number;
    useImport.TheStatesPersonNumber2.Number =
      local.TheStatesPersonNumber2.Number;
    useImport.TheStatesPersonNumber3.Number =
      local.TheStatesPersonNumber3.Number;
    useImport.JuvenileJusticeAuthority.Number =
      local.JuvenileJusticeAuthority.Number;
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    useImport.ApplyApCollToRcv.Flag = import.ApplyApCollToRcvInd.Flag;
    useImport.Ap.Number = import.Obligor.Number;

    Call(FnB650ApplyToPotRcv.Execute, useImport, useExport);

    import.ExpCollsEligForRcvCnt.Count = useImport.ExpCollsEligForRcvCnt.Count;
    import.PerCollection.Assign(useImport.Per);
    export.AppliedApCollToRcvInd.Flag = useExport.AppliedApCollToRcvInd.Flag;
    export.Abort.Flag = useExport.Abort.Flag;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseApplicantRecipient1()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCaseApplicantRecipient1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseApplicantRecipient2()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return ReadEach("ReadCaseApplicantRecipient2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);

        return true;
      });
  }

  private bool ReadCaseApplicantRecipientCsePerson()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCaseApplicantRecipientCsePerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCaseChild1()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseChild1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadCaseChild2()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseChild2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "endDate", entities.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseChild3()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseChild3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseChild4()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseChild4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseChild5()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseChild5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "endDate", entities.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseChild6()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseChild6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supported2.Number);
        db.SetNullableDate(
          command, "endDate", entities.DebtDetail.DueDt.GetValueOrDefault());
        db.SetString(command, "cspNumber2", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Child.CspNumber = db.GetString(reader, 2);
        entities.Child.Type1 = db.GetString(reader, 3);
        entities.Child.Identifier = db.GetInt32(reader, 4);
        entities.Child.StartDate = db.GetNullableDate(reader, 5);
        entities.Child.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private bool ReadCaseCsePersonApplicantRecipientChild()
  {
    entities.Case1.Populated = false;
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseCsePersonApplicantRecipientChild",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber1", import.Obligor.Number);
        db.SetString(command, "cspNumber2", entities.Supported2.Number);
        db.SetNullableDate(command, "startDate", date);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 2);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.Child.CspNumber = db.GetString(reader, 7);
        entities.Child.Type1 = db.GetString(reader, 8);
        entities.Child.Identifier = db.GetInt32(reader, 9);
        entities.Child.StartDate = db.GetNullableDate(reader, 10);
        entities.Child.EndDate = db.GetNullableDate(reader, 11);
        entities.Case1.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection.Populated);
    entities.Ar.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", import.PerCollection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", import.PerCollection.OtyId);
        db.SetNullableInt32(command, "obgId", import.PerCollection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", import.PerCollection.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", import.PerCollection.CpaType);
        db.SetNullableInt32(command, "otrId", import.PerCollection.OtrId);
        db.SetNullableString(
          command, "otrTypeDisb", import.PerCollection.OtrType);
        db.SetNullableInt32(command, "crtId", import.PerCollection.CrtType);
        db.SetNullableInt32(command, "cstId", import.PerCollection.CstId);
        db.SetNullableInt32(command, "crvId", import.PerCollection.CrvId);
        db.SetNullableInt32(command, "crdId", import.PerCollection.CrdId);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ArOrganization.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Ar.Number);
      },
      (db, reader) =>
      {
        entities.ArOrganization.Number = db.GetString(reader, 0);
        entities.ArOrganization.Type1 = db.GetString(reader, 1);
        entities.ArOrganization.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ArOrganization.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ArOrganization.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(import.PerDebt.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PerDebt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Populated = true;
      });
  }

  private bool ReadCsePersonApplicantRecipient1()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonApplicantRecipient1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", entities.DebtDetail.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCsePersonApplicantRecipient2()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonApplicantRecipient2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", entities.Child.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCsePersonApplicantRecipient3()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadCsePersonApplicantRecipient3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", entities.Child.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.PerDebt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PerDebt.OtyType);
        db.SetInt32(command, "obgGeneratedId", import.PerDebt.ObgGeneratedId);
        db.SetString(command, "otrType", import.PerDebt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", import.PerDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.PerDebt.CpaType);
        db.SetString(command, "cspNumber", import.PerDebt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
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
    /// <summary>A HardcodeGroup group.</summary>
    [Serializable]
    public class HardcodeGroup
    {
      /// <summary>
      /// A value of HardcodeSpousalSupport.
      /// </summary>
      [JsonPropertyName("hardcodeSpousalSupport")]
      public ObligationType HardcodeSpousalSupport
      {
        get => hardcodeSpousalSupport ??= new();
        set => hardcodeSpousalSupport = value;
      }

      /// <summary>
      /// A value of HardcodeSpArrearsJudgmt.
      /// </summary>
      [JsonPropertyName("hardcodeSpArrearsJudgmt")]
      public ObligationType HardcodeSpArrearsJudgmt
      {
        get => hardcodeSpArrearsJudgmt ??= new();
        set => hardcodeSpArrearsJudgmt = value;
      }

      /// <summary>
      /// A value of HardcodeVoluntary.
      /// </summary>
      [JsonPropertyName("hardcodeVoluntary")]
      public ObligationType HardcodeVoluntary
      {
        get => hardcodeVoluntary ??= new();
        set => hardcodeVoluntary = value;
      }

      private ObligationType hardcodeSpousalSupport;
      private ObligationType hardcodeSpArrearsJudgmt;
      private ObligationType hardcodeVoluntary;
    }

    /// <summary>
    /// A value of ExpCollsEligForRcvCnt.
    /// </summary>
    [JsonPropertyName("expCollsEligForRcvCnt")]
    public Common ExpCollsEligForRcvCnt
    {
      get => expCollsEligForRcvCnt ??= new();
      set => expCollsEligForRcvCnt = value;
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
    /// A value of PerCashReceiptType.
    /// </summary>
    [JsonPropertyName("perCashReceiptType")]
    public CashReceiptType PerCashReceiptType
    {
      get => perCashReceiptType ??= new();
      set => perCashReceiptType = value;
    }

    /// <summary>
    /// A value of PerCollection.
    /// </summary>
    [JsonPropertyName("perCollection")]
    public Collection PerCollection
    {
      get => perCollection ??= new();
      set => perCollection = value;
    }

    /// <summary>
    /// A value of PerDebt.
    /// </summary>
    [JsonPropertyName("perDebt")]
    public ObligationTransaction PerDebt
    {
      get => perDebt ??= new();
      set => perDebt = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// Gets a value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public HardcodeGroup Hardcode
    {
      get => hardcode ?? (hardcode = new());
      set => hardcode = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ApplyApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("applyApCollToRcvInd")]
    public Common ApplyApCollToRcvInd
    {
      get => applyApCollToRcvInd ??= new();
      set => applyApCollToRcvInd = value;
    }

    private Common expCollsEligForRcvCnt;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType perCashReceiptType;
    private Collection perCollection;
    private ObligationTransaction perDebt;
    private ObligationType obligationType;
    private CsePerson obligor;
    private ProgramProcessingInfo programProcessingInfo;
    private HardcodeGroup hardcode;
    private Common displayInd;
    private Common applyApCollToRcvInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of StateCollectionInd.
    /// </summary>
    [JsonPropertyName("stateCollectionInd")]
    public Common StateCollectionInd
    {
      get => stateCollectionInd ??= new();
      set => stateCollectionInd = value;
    }

    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    /// <summary>
    /// A value of AppliedApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("appliedApCollToRcvInd")]
    public Common AppliedApCollToRcvInd
    {
      get => appliedApCollToRcvInd ??= new();
      set => appliedApCollToRcvInd = value;
    }

    private CsePerson obligee;
    private CsePerson child;
    private Case1 case1;
    private DebtDetail debtDetail;
    private Common stateCollectionInd;
    private Common abort;
    private Common appliedApCollToRcvInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NumericString.
    /// </summary>
    [JsonPropertyName("numericString")]
    public TextWorkArea NumericString
    {
      get => numericString ??= new();
      set => numericString = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber")]
    public CsePerson TheStatesPersonNumber
    {
      get => theStatesPersonNumber ??= new();
      set => theStatesPersonNumber = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber2.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber2")]
    public CsePerson TheStatesPersonNumber2
    {
      get => theStatesPersonNumber2 ??= new();
      set => theStatesPersonNumber2 = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber3.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber3")]
    public CsePerson TheStatesPersonNumber3
    {
      get => theStatesPersonNumber3 ??= new();
      set => theStatesPersonNumber3 = value;
    }

    /// <summary>
    /// A value of JuvenileJusticeAuthority.
    /// </summary>
    [JsonPropertyName("juvenileJusticeAuthority")]
    public CsePerson JuvenileJusticeAuthority
    {
      get => juvenileJusticeAuthority ??= new();
      set => juvenileJusticeAuthority = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private TextWorkArea numericString;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private CsePerson theStatesPersonNumber;
    private CsePerson theStatesPersonNumber2;
    private CsePerson theStatesPersonNumber3;
    private CsePerson juvenileJusticeAuthority;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of OriginalCredit.
    /// </summary>
    [JsonPropertyName("originalCredit")]
    public DisbursementTransaction OriginalCredit
    {
      get => originalCredit ??= new();
      set => originalCredit = value;
    }

    /// <summary>
    /// A value of ArOrganization.
    /// </summary>
    [JsonPropertyName("arOrganization")]
    public CsePerson ArOrganization
    {
      get => arOrganization ??= new();
      set => arOrganization = value;
    }

    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson obligor;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private CsePersonAccount obligee;
    private CaseRole child;
    private CsePersonAccount supported1;
    private CsePerson supported2;
    private DebtDetail debtDetail;
    private DisbursementTransaction originalCredit;
    private CsePerson arOrganization;
  }
#endregion
}
