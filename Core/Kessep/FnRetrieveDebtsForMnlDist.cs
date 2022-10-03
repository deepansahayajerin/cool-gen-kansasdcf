// Program: FN_RETRIEVE_DEBTS_FOR_MNL_DIST, ID: 372288305, model: 746.
// Short name: SWE02247
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RETRIEVE_DEBTS_FOR_MNL_DIST.
/// </summary>
[Serializable]
public partial class FnRetrieveDebtsForMnlDist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RETRIEVE_DEBTS_FOR_MNL_DIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRetrieveDebtsForMnlDist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRetrieveDebtsForMnlDist.
  /// </summary>
  public FnRetrieveDebtsForMnlDist(IContext context, Import import,
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
    // : Get Hardcoded values.
    // ----------------------------------------------------------------------------------
    // 05/20/2019   R Mathews   CQ65311   Call FN_CHK_UNPROC_TRAN_MNL_DIST to 
    // obtain
    //                                    
    // shortened error messages for MCOL
    // and filter
    //                                    
    // by program
    // ----------------------------------------------------------------------------------
    UseFnHardcodedDebtDistribution();
    local.Collection.Date = import.CashReceiptDetail.CollectionDate;
    local.Current.Date = Now().Date;

    if (Equal(import.ToFilter.DueDt, local.NullDateWorkArea.Date))
    {
      local.ToFilter.DueDt = UseCabSetMaximumDiscontinueDate1();
    }
    else
    {
      local.ToFilter.DueDt = import.ToFilter.DueDt;
    }

    // : Check for Unprocessed Transactions for this Obligor.
    local.OmitCrdInd.Flag = "Y";
    UseFnChkUnprocTranMnlDist();
    export.Group.Index = -1;

    // : Read each Obligation for the Import CSE Person (Obligor).
    //   Skip all Secondary Obligations.
    local.Group.Index = 0;
    local.Group.Clear();

    foreach(var item in ReadCsePersonObligorObligationTypeObligationDebt())
    {
      if (import.FilterObligationType.SystemGeneratedIdentifier != 0)
      {
        if (import.FilterObligationType.SystemGeneratedIdentifier != entities
          .ExistingObligationType.SystemGeneratedIdentifier)
        {
          local.Group.Next();

          continue;
        }
      }

      if (Equal(import.CollectionType.Code, "V"))
      {
        if (AsChar(entities.ExistingObligationType.Classification) != 'V')
        {
          local.Group.Next();

          continue;
        }
      }

      if (AsChar(import.DisplayAllObligInd.Flag) == 'N')
      {
        if (!ReadManualDistributionAudit())
        {
          // : The obligation is not set for manual distribution - Skip this 
          // Obligaiton.
          local.Group.Next();

          continue;
        }
      }

      if (IsEmpty(import.FilterLegalAction.StandardNumber))
      {
        if (ReadLegalAction2())
        {
          MoveLegalAction(entities.ExistingLegalAction, local.LegalAction);
        }
        else
        {
          // : No Court Order associated to the Obligation - Continue 
          // Processing.
          MoveLegalAction(local.NullLegalAction, local.LegalAction);
        }
      }
      else if (ReadLegalAction1())
      {
        MoveLegalAction(entities.ExistingLegalAction, local.LegalAction);
      }
      else
      {
        // : The Court Order does not match.
        local.Group.Next();

        continue;
      }

      if (AsChar(entities.ExistingObligationType.SupportedPersonReqInd) == 'Y')
      {
        if (ReadCsePerson())
        {
          local.SuppPrsnCsePersonsWorkSet.Number =
            entities.ExistingSupportedKeyOnly.Number;
          UseFnDeterminePgmForDebtDetail();

          if (AsChar(import.TraceInd.Flag) == 'Y')
          {
            local.SuppPrsnCsePerson.Number =
              entities.ExistingSupportedKeyOnly.Number;
            local.SuppPrsnCsePersonsWorkSet.FormattedName = "N/A Trace";
          }
          else
          {
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.SuppPrsnCsePerson.Number =
                entities.ExistingSupportedKeyOnly.Number;
              local.SuppPrsnCsePersonsWorkSet.FormattedName =
                "** Supp Prsn Name Unavailable **";
              ExitState = "ACO_NN0000_ALL_OK";
            }
          }
        }
        else
        {
          ExitState = "FN0000_SUPPORTED_PERSON_NF";
          local.Group.Next();

          return;
        }
      }
      else
      {
        local.DistPgm.Code = "";
      }

      if (!IsEmpty(import.Pgm.Text3))
      {
        if (!Equal(import.Pgm.Text3, local.DistPgm.Code))
        {
          local.Group.Next();

          continue;
        }
      }

      if (entities.ExistingObligation.SystemGeneratedIdentifier != local
        .Hold.SystemGeneratedIdentifier)
      {
        local.Hold.SystemGeneratedIdentifier =
          entities.ExistingObligation.SystemGeneratedIdentifier;

        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
          (local.HardcodedPrimary.PrimarySecondaryCode))
        {
          local.Group.Update.Obligor.Number =
            entities.ExistingObligorKeyOnly.Number;
          local.Group.Update.ObligationType.SystemGeneratedIdentifier =
            entities.ExistingObligationType.SystemGeneratedIdentifier;
          local.Group.Update.Obligation.SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;
        }
      }

      if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
      {
        local.Group.Next();

        break;
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.Obligation.Assign(entities.ExistingObligation);
      export.Group.Update.ObligationType.
        Assign(entities.ExistingObligationType);
      export.Group.Update.LegalAction.StandardNumber =
        local.LegalAction.StandardNumber;
      MoveDebtDetail1(entities.ExistingDebtDetail,
        export.Group.Update.DebtDetail);
      MoveObligationTransaction(entities.ExistingDebt, export.Group.Update.Debt);
        
      export.Group.Update.Pgm.Text3 = local.DistPgm.Code;
      export.Group.Update.SuppPrsnCsePerson.Number =
        local.SuppPrsnCsePerson.Number;
      export.Group.Update.SuppPrsnCsePersonsWorkSet.Number =
        local.SuppPrsnCsePersonsWorkSet.Number;
      export.Group.Update.SuppPrsnCsePersonsWorkSet.FormattedName =
        local.SuppPrsnCsePersonsWorkSet.FormattedName;
      local.Group.Next();
    }

    // : Process Secondary Obligaitons.
    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!ReadCsePersonObligationObligationType1())
      {
        if (!ReadCsePersonObligationObligationType2())
        {
          ExitState = "FN0000_CONCURRENT_OBLIGATION_NF";

          return;
        }
      }

      if (ReadLegalAction3())
      {
        MoveLegalAction(entities.ExistingSecLegalAction, local.Sec);
      }
      else
      {
        // : The Court Order does not match.
      }

      if (AsChar(entities.ExistingSecObligationType.Classification) == AsChar
        (local.HardcodedAccruingClass.Classification))
      {
        // : Process each Accruing type of Obligation with the Due Date of the 
        // Debt Detail due with the filter dates.
        foreach(var item in ReadDebtDetailDebt2())
        {
          if (AsChar(entities.ExistingSecObligationType.SupportedPersonReqInd) ==
            'Y')
          {
            if (ReadCsePerson())
            {
              local.SuppPrsnCsePersonsWorkSet.Number =
                entities.ExistingSupportedKeyOnly.Number;
              UseFnDeterminePgmForDebtDetail();

              if (AsChar(import.TraceInd.Flag) == 'Y')
              {
                local.SuppPrsnCsePerson.Number =
                  entities.ExistingSupportedKeyOnly.Number;
                local.SuppPrsnCsePersonsWorkSet.FormattedName = "N/A Trace";
              }
              else
              {
                UseSiReadCsePerson();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.SuppPrsnCsePerson.Number =
                    entities.ExistingSupportedKeyOnly.Number;
                  local.SuppPrsnCsePersonsWorkSet.FormattedName =
                    "** Supp Prsn Name Unavailable **";
                  ExitState = "ACO_NN0000_ALL_OK";
                }
              }
            }
            else
            {
              ExitState = "FN0000_SUPPORTED_PERSON_NF";

              return;
            }
          }
          else
          {
            local.DistPgm.Code = "";
            local.SuppPrsnCsePerson.Number = "N/A";
            local.SuppPrsnCsePersonsWorkSet.FormattedName =
              "No Supported Person";
          }

          if (!IsEmpty(import.Pgm.Text3))
          {
            if (!Equal(import.Pgm.Text3, local.DistPgm.Code))
            {
              continue;
            }
          }

          if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
          {
            goto Test;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Obligation.Assign(entities.ExistingSecObligation);
          export.Group.Update.ObligationType.Assign(
            entities.ExistingSecObligationType);
          export.Group.Update.LegalAction.StandardNumber =
            local.Sec.StandardNumber;
          MoveDebtDetail1(entities.ExistingDebtDetail,
            export.Group.Update.DebtDetail);
          MoveObligationTransaction(entities.ExistingDebt,
            export.Group.Update.Debt);
          export.Group.Update.Pgm.Text3 = local.DistPgm.Code;
          export.Group.Update.SuppPrsnCsePerson.Number =
            local.SuppPrsnCsePerson.Number;
          export.Group.Update.SuppPrsnCsePersonsWorkSet.Number =
            local.SuppPrsnCsePersonsWorkSet.Number;
          export.Group.Update.SuppPrsnCsePersonsWorkSet.FormattedName =
            local.SuppPrsnCsePersonsWorkSet.FormattedName;
        }
      }
      else
      {
        // : Process the Non-Accruing type of Obligations.
        foreach(var item in ReadDebtDetailDebt1())
        {
          if (AsChar(entities.ExistingSecObligationType.SupportedPersonReqInd) ==
            'Y')
          {
            if (ReadCsePerson())
            {
              local.SuppPrsnCsePersonsWorkSet.Number =
                entities.ExistingSupportedKeyOnly.Number;
              UseFnDeterminePgmForDebtDetail();

              if (AsChar(import.TraceInd.Flag) == 'Y')
              {
                local.SuppPrsnCsePerson.Number =
                  entities.ExistingSupportedKeyOnly.Number;
                local.SuppPrsnCsePersonsWorkSet.FormattedName = "N/A Trace";
              }
              else
              {
                UseSiReadCsePerson();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.SuppPrsnCsePerson.Number =
                    entities.ExistingSupportedKeyOnly.Number;
                  local.SuppPrsnCsePersonsWorkSet.FormattedName =
                    "** Supp Prsn Name Unavailable **";
                  ExitState = "ACO_NN0000_ALL_OK";
                }
              }
            }
            else
            {
              ExitState = "FN0000_SUPPORTED_PERSON_NF";

              return;
            }
          }
          else
          {
            local.DistPgm.Code = "";
            local.SuppPrsnCsePerson.Number = "N/A";
            local.SuppPrsnCsePersonsWorkSet.FormattedName =
              "No Supported Person";
          }

          if (!IsEmpty(import.Pgm.Text3))
          {
            if (!Equal(import.Pgm.Text3, local.DistPgm.Code))
            {
              continue;
            }
          }

          if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
          {
            goto Test;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Obligation.Assign(entities.ExistingSecObligation);
          export.Group.Update.ObligationType.Assign(
            entities.ExistingSecObligationType);
          export.Group.Update.LegalAction.StandardNumber =
            local.Sec.StandardNumber;
          MoveDebtDetail1(entities.ExistingDebtDetail,
            export.Group.Update.DebtDetail);
          MoveObligationTransaction(entities.ExistingDebt,
            export.Group.Update.Debt);
          export.Group.Update.Pgm.Text3 = local.DistPgm.Code;
          export.Group.Update.SuppPrsnCsePerson.Number =
            local.SuppPrsnCsePerson.Number;
          export.Group.Update.SuppPrsnCsePersonsWorkSet.Number =
            local.SuppPrsnCsePersonsWorkSet.Number;
          export.Group.Update.SuppPrsnCsePersonsWorkSet.FormattedName =
            local.SuppPrsnCsePersonsWorkSet.FormattedName;
        }
      }

Test:
      ;
    }

    // : Sort in descending Debt Detail Due Date order.
    if (export.Group.IsEmpty)
    {
      return;
    }

    do
    {
      local.ContinueLoopingInd.Flag = "N";
      export.Group.Index = 0;

      for(var limit = export.Group.Count - 1; export.Group.Index < limit; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        local.Compare.DueDt = export.Group.Item.DebtDetail.DueDt;

        ++export.Group.Index;
        export.Group.CheckSize();

        if (!Lt(local.Compare.DueDt, export.Group.Item.DebtDetail.DueDt))
        {
          --export.Group.Index;
          export.Group.CheckSize();

          continue;
        }

        local.ContinueLoopingInd.Flag = "Y";
        MoveCollection(export.Group.Item.Collection,
          local.Group2.Update.Grp2Collection);
        MoveObligationTransaction(export.Group.Item.Debt,
          local.Group2.Update.Grp2Debt);
        local.Group2.Update.Grp2DebtDetail.Assign(export.Group.Item.DebtDetail);
        local.Group2.Update.Grp2Common.SelectChar =
          export.Group.Item.Common.SelectChar;
        local.Group2.Update.Grp2LegalAction.StandardNumber =
          export.Group.Item.LegalAction.StandardNumber;
        local.Group2.Update.Grp2Obligation.Assign(export.Group.Item.Obligation);
        local.Group2.Update.Grp2ObligationType.Assign(
          export.Group.Item.ObligationType);
        local.Group2.Update.ApplyToCod2.Text1 =
          export.Group.Item.ApplyToCod.Text1;
        local.Group2.Update.SuppPrsn2CsePerson.Number =
          export.Group.Item.SuppPrsnCsePerson.Number;
        MoveCsePersonsWorkSet(export.Group.Item.SuppPrsnCsePersonsWorkSet,
          local.Group2.Update.SuppPrsn2CsePersonsWorkSet);
        local.Group2.Update.Pgm2.Text3 = export.Group.Item.Pgm.Text3;

        --export.Group.Index;
        export.Group.CheckSize();

        MoveCollection(export.Group.Item.Collection,
          local.Group1.Update.Grp1Collection);
        MoveObligationTransaction(export.Group.Item.Debt,
          local.Group1.Update.Grp1Debt);
        local.Group1.Update.Grp1DebtDetail.Assign(export.Group.Item.DebtDetail);
        local.Group1.Update.Grp1Common.SelectChar =
          export.Group.Item.Common.SelectChar;
        local.Group1.Update.Grp1LegalAction.StandardNumber =
          export.Group.Item.LegalAction.StandardNumber;
        local.Group1.Update.Grp1Obligation.Assign(export.Group.Item.Obligation);
        local.Group1.Update.Grp1ObligationType.Assign(
          export.Group.Item.ObligationType);
        local.Group1.Update.ApplyToCod1.Text1 =
          export.Group.Item.ApplyToCod.Text1;
        local.Group1.Update.SuppPrsn1CsePerson.Number =
          export.Group.Item.SuppPrsnCsePerson.Number;
        MoveCsePersonsWorkSet(export.Group.Item.SuppPrsnCsePersonsWorkSet,
          local.Group1.Update.SuppPrsn1CsePersonsWorkSet);
        local.Group1.Update.Pgm1.Text3 = export.Group.Item.Pgm.Text3;
        MoveCollection(local.Group2.Item.Grp2Collection,
          export.Group.Update.Collection);
        MoveObligationTransaction(local.Group2.Item.Grp2Debt,
          export.Group.Update.Debt);
        export.Group.Update.DebtDetail.Assign(local.Group2.Item.Grp2DebtDetail);
        export.Group.Update.Common.SelectChar =
          local.Group2.Item.Grp2Common.SelectChar;
        export.Group.Update.LegalAction.StandardNumber =
          local.Group2.Item.Grp2LegalAction.StandardNumber;
        export.Group.Update.Obligation.Assign(local.Group2.Item.Grp2Obligation);
        export.Group.Update.ObligationType.Assign(
          local.Group2.Item.Grp2ObligationType);
        export.Group.Update.ApplyToCod.Text1 =
          local.Group2.Item.ApplyToCod2.Text1;
        export.Group.Update.SuppPrsnCsePerson.Number =
          local.Group2.Item.SuppPrsn2CsePerson.Number;
        MoveCsePersonsWorkSet(local.Group2.Item.SuppPrsn2CsePersonsWorkSet,
          export.Group.Update.SuppPrsnCsePersonsWorkSet);
        export.Group.Update.Pgm.Text3 = local.Group2.Item.Pgm2.Text3;

        ++export.Group.Index;
        export.Group.CheckSize();

        MoveCollection(local.Group1.Item.Grp1Collection,
          export.Group.Update.Collection);
        MoveObligationTransaction(local.Group1.Item.Grp1Debt,
          export.Group.Update.Debt);
        export.Group.Update.DebtDetail.Assign(local.Group1.Item.Grp1DebtDetail);
        export.Group.Update.Common.SelectChar =
          local.Group1.Item.Grp1Common.SelectChar;
        export.Group.Update.LegalAction.StandardNumber =
          local.Group1.Item.Grp1LegalAction.StandardNumber;
        export.Group.Update.Obligation.Assign(local.Group1.Item.Grp1Obligation);
        export.Group.Update.ObligationType.Assign(
          local.Group1.Item.Grp1ObligationType);
        export.Group.Update.ApplyToCod.Text1 =
          local.Group1.Item.ApplyToCod1.Text1;
        export.Group.Update.SuppPrsnCsePerson.Number =
          local.Group1.Item.SuppPrsn1CsePerson.Number;
        MoveCsePersonsWorkSet(local.Group1.Item.SuppPrsn1CsePersonsWorkSet,
          export.Group.Update.SuppPrsnCsePersonsWorkSet);
        export.Group.Update.Pgm.Text3 = local.Group1.Item.Pgm1.Text3;

        --export.Group.Index;
        export.Group.CheckSize();
      }

      export.Group.CheckIndex();
    }
    while(AsChar(local.ContinueLoopingInd.Flag) != 'N');

    // : Remove the Max Discontinue Date if needed.
    export.Group.Index = 0;

    for(var limit = export.Group.Count; export.Group.Index < limit; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (Equal(export.Group.Item.DebtDetail.CoveredPrdEndDt,
        local.NullDateWorkArea.Date))
      {
        continue;
      }

      local.Tmp.Date = export.Group.Item.DebtDetail.CoveredPrdEndDt;
      export.Group.Update.DebtDetail.CoveredPrdEndDt =
        UseCabSetMaximumDiscontinueDate2();
    }

    export.Group.CheckIndex();
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDebtDetail1(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveDebtDetail2(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Tmp.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnChkUnprocTranMnlDist()
  {
    var useImport = new FnChkUnprocTranMnlDist.Import();
    var useExport = new FnChkUnprocTranMnlDist.Export();

    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.OmitCrdInd.Flag = local.OmitCrdInd.Flag;

    Call(FnChkUnprocTranMnlDist.Execute, useImport, useExport);

    export.ScreenOwedAmounts.ErrorInformationLine =
      useExport.ScreenOwedAmounts.ErrorInformationLine;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.ExistingSupportedKeyOnly.Number;
    MoveObligationType(entities.ExistingObligationType, useImport.ObligationType);
      
    useImport.Obligation.OrderTypeCode =
      entities.ExistingObligation.OrderTypeCode;
    MoveDebtDetail2(entities.ExistingDebtDetail, useImport.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.DistPgm.Assign(useExport.Program);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedVoluntary.SystemGeneratedIdentifier =
      useExport.OtApFees.SystemGeneratedIdentifier;
    local.HardcodedPrimSecRlnRsn.SequentialGeneratedIdentifier =
      useExport.OrrPrimarySecondary.SequentialGeneratedIdentifier;
    local.HardcodedPrimary.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedJointSeveral.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.SuppPrsnCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      local.SuppPrsnCsePersonsWorkSet);
    local.SuppPrsnCsePerson.Number = useExport.CsePerson.Number;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.ExistingSupportedKeyOnly.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingDebt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSupportedKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingSupportedKeyOnly.Populated = true;
      });
  }

  private bool ReadCsePersonObligationObligationType1()
  {
    entities.ExistingSecObligorKeyOnly.Populated = false;
    entities.ExistingSecObligationType.Populated = false;
    entities.ExistingSecObligation.Populated = false;

    return Read("ReadCsePersonObligationObligationType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          local.HardcodedPrimSecRlnRsn.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          local.Group.Item.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otySecondId",
          local.Group.Item.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", local.Group.Item.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingSecObligorKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingSecObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingSecObligation.CpaType = db.GetString(reader, 1);
        entities.ExistingSecObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingSecObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingSecObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingSecObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingSecObligation.HistoryInd =
          db.GetNullableString(reader, 5);
        entities.ExistingSecObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 6);
        entities.ExistingSecObligation.OrderTypeCode = db.GetString(reader, 7);
        entities.ExistingSecObligation.DormantInd =
          db.GetNullableString(reader, 8);
        entities.ExistingSecObligationType.Code = db.GetString(reader, 9);
        entities.ExistingSecObligationType.Classification =
          db.GetString(reader, 10);
        entities.ExistingSecObligationType.SupportedPersonReqInd =
          db.GetString(reader, 11);
        entities.ExistingSecObligorKeyOnly.Populated = true;
        entities.ExistingSecObligationType.Populated = true;
        entities.ExistingSecObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingSecObligation.CpaType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingSecObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingSecObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ExistingSecObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingSecObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadCsePersonObligationObligationType2()
  {
    entities.ExistingSecObligorKeyOnly.Populated = false;
    entities.ExistingSecObligationType.Populated = false;
    entities.ExistingSecObligation.Populated = false;

    return Read("ReadCsePersonObligationObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          local.HardcodedPrimSecRlnRsn.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "obgFGeneratedId",
          local.Group.Item.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyFirstId",
          local.Group.Item.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", local.Group.Item.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingSecObligorKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingSecObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingSecObligation.CpaType = db.GetString(reader, 1);
        entities.ExistingSecObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingSecObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingSecObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingSecObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingSecObligation.HistoryInd =
          db.GetNullableString(reader, 5);
        entities.ExistingSecObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 6);
        entities.ExistingSecObligation.OrderTypeCode = db.GetString(reader, 7);
        entities.ExistingSecObligation.DormantInd =
          db.GetNullableString(reader, 8);
        entities.ExistingSecObligationType.Code = db.GetString(reader, 9);
        entities.ExistingSecObligationType.Classification =
          db.GetString(reader, 10);
        entities.ExistingSecObligationType.SupportedPersonReqInd =
          db.GetString(reader, 11);
        entities.ExistingSecObligorKeyOnly.Populated = true;
        entities.ExistingSecObligationType.Populated = true;
        entities.ExistingSecObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingSecObligation.CpaType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingSecObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingSecObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ExistingSecObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingSecObligationType.SupportedPersonReqInd);
      });
  }

  private IEnumerable<bool> ReadCsePersonObligorObligationTypeObligationDebt()
  {
    return ReadEach("ReadCsePersonObligorObligationTypeObligationDebt",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(
          command, "primarySecondaryCode1",
          local.HardcodedJointSeveral.PrimarySecondaryCode ?? "");
        db.SetNullableString(
          command, "primarySecondaryCode2",
          local.HardcodedPrimary.PrimarySecondaryCode ?? "");
        db.SetNullableDate(
          command, "retiredDt",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt1", import.FromFilter.DueDt.GetValueOrDefault());
        db.SetDate(command, "dueDt2", local.ToFilter.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.ExistingObligorKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingKeyOnlyObligor.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 0);
        entities.ExistingKeyOnlyObligor.Type1 = db.GetString(reader, 1);
        entities.ExistingObligation.CpaType = db.GetString(reader, 1);
        entities.ExistingDebt.CpaType = db.GetString(reader, 1);
        entities.ExistingDebt.CpaType = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 1);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 2);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 2);
        entities.ExistingObligationType.Code = db.GetString(reader, 3);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 4);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 5);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 7);
        entities.ExistingObligation.HistoryInd =
          db.GetNullableString(reader, 8);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 9);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 10);
        entities.ExistingObligation.DormantInd =
          db.GetNullableString(reader, 11);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 12);
        entities.ExistingDebt.Type1 = db.GetString(reader, 13);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 13);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 14);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 16);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 17);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 18);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 19);
        entities.ExistingDebtDetail.AdcDt = db.GetNullableDate(reader, 20);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 21);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 22);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 23);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 24);
        entities.ExistingObligorKeyOnly.Populated = true;
        entities.ExistingKeyOnlyObligor.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingKeyOnlyObligor.Type1);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingSecObligation.Populated);
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailDebt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingSecObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingSecObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ExistingSecObligation.CspNumber);
        db.
          SetString(command, "cpaType", entities.ExistingSecObligation.CpaType);
          
        db.SetDate(command, "dueDt", local.ToFilter.DueDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebt.Type1 = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingDebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 11);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 12);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 14);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 16);
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingSecObligation.Populated);
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailDebt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingSecObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingSecObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ExistingSecObligation.CspNumber);
        db.
          SetString(command, "cpaType", entities.ExistingSecObligation.CpaType);
          
        db.SetDate(
          command, "dueDt1", import.FromFilter.DueDt.GetValueOrDefault());
        db.SetDate(command, "dueDt2", local.ToFilter.DueDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebt.Type1 = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingDebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 11);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 12);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 14);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 16);
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.FilterLegalAction.StandardNumber ?? ""
          );
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingSecObligation.Populated);
    entities.ExistingSecLegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingSecObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingSecLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingSecLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingSecLegalAction.Populated = true;
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
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
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingManualDistributionAudit.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingManualDistributionAudit.CspNumber =
          db.GetString(reader, 2);
        entities.ExistingManualDistributionAudit.CpaType =
          db.GetString(reader, 3);
        entities.ExistingManualDistributionAudit.EffectiveDt =
          db.GetDate(reader, 4);
        entities.ExistingManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingManualDistributionAudit.Instructions =
          db.GetNullableString(reader, 6);
        entities.ExistingManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ExistingManualDistributionAudit.CpaType);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FromFilter.
    /// </summary>
    [JsonPropertyName("fromFilter")]
    public DebtDetail FromFilter
    {
      get => fromFilter ??= new();
      set => fromFilter = value;
    }

    /// <summary>
    /// A value of ToFilter.
    /// </summary>
    [JsonPropertyName("toFilter")]
    public DebtDetail ToFilter
    {
      get => toFilter ??= new();
      set => toFilter = value;
    }

    /// <summary>
    /// A value of FilterObligationType.
    /// </summary>
    [JsonPropertyName("filterObligationType")]
    public ObligationType FilterObligationType
    {
      get => filterObligationType ??= new();
      set => filterObligationType = value;
    }

    /// <summary>
    /// A value of DisplayAllObligInd.
    /// </summary>
    [JsonPropertyName("displayAllObligInd")]
    public Common DisplayAllObligInd
    {
      get => displayAllObligInd ??= new();
      set => displayAllObligInd = value;
    }

    /// <summary>
    /// A value of TraceInd.
    /// </summary>
    [JsonPropertyName("traceInd")]
    public Common TraceInd
    {
      get => traceInd ??= new();
      set => traceInd = value;
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
    /// A value of Pgm.
    /// </summary>
    [JsonPropertyName("pgm")]
    public WorkArea Pgm
    {
      get => pgm ??= new();
      set => pgm = value;
    }

    private CsePerson csePerson;
    private CollectionType collectionType;
    private LegalAction filterLegalAction;
    private DebtDetail fromFilter;
    private DebtDetail toFilter;
    private ObligationType filterObligationType;
    private Common displayAllObligInd;
    private Common traceInd;
    private CashReceiptDetail cashReceiptDetail;
    private WorkArea pgm;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      /// A value of ApplyToCod.
      /// </summary>
      [JsonPropertyName("applyToCod")]
      public TextWorkArea ApplyToCod
      {
        get => applyToCod ??= new();
        set => applyToCod = value;
      }

      /// <summary>
      /// A value of SuppPrsnCsePerson.
      /// </summary>
      [JsonPropertyName("suppPrsnCsePerson")]
      public CsePerson SuppPrsnCsePerson
      {
        get => suppPrsnCsePerson ??= new();
        set => suppPrsnCsePerson = value;
      }

      /// <summary>
      /// A value of SuppPrsnCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("suppPrsnCsePersonsWorkSet")]
      public CsePersonsWorkSet SuppPrsnCsePersonsWorkSet
      {
        get => suppPrsnCsePersonsWorkSet ??= new();
        set => suppPrsnCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 85;

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Collection collection;
      private TextWorkArea applyToCod;
      private CsePerson suppPrsnCsePerson;
      private CsePersonsWorkSet suppPrsnCsePersonsWorkSet;
      private WorkArea pgm;
      private Common common;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private ScreenOwedAmounts screenOwedAmounts;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CsePerson obligor;
      private ObligationType obligationType;
      private Obligation obligation;
    }

    /// <summary>A Group1Group group.</summary>
    [Serializable]
    public class Group1Group
    {
      /// <summary>
      /// A value of Grp1Common.
      /// </summary>
      [JsonPropertyName("grp1Common")]
      public Common Grp1Common
      {
        get => grp1Common ??= new();
        set => grp1Common = value;
      }

      /// <summary>
      /// A value of Grp1ObligationType.
      /// </summary>
      [JsonPropertyName("grp1ObligationType")]
      public ObligationType Grp1ObligationType
      {
        get => grp1ObligationType ??= new();
        set => grp1ObligationType = value;
      }

      /// <summary>
      /// A value of Grp1Obligation.
      /// </summary>
      [JsonPropertyName("grp1Obligation")]
      public Obligation Grp1Obligation
      {
        get => grp1Obligation ??= new();
        set => grp1Obligation = value;
      }

      /// <summary>
      /// A value of Grp1Debt.
      /// </summary>
      [JsonPropertyName("grp1Debt")]
      public ObligationTransaction Grp1Debt
      {
        get => grp1Debt ??= new();
        set => grp1Debt = value;
      }

      /// <summary>
      /// A value of Grp1DebtDetail.
      /// </summary>
      [JsonPropertyName("grp1DebtDetail")]
      public DebtDetail Grp1DebtDetail
      {
        get => grp1DebtDetail ??= new();
        set => grp1DebtDetail = value;
      }

      /// <summary>
      /// A value of Grp1LegalAction.
      /// </summary>
      [JsonPropertyName("grp1LegalAction")]
      public LegalAction Grp1LegalAction
      {
        get => grp1LegalAction ??= new();
        set => grp1LegalAction = value;
      }

      /// <summary>
      /// A value of Grp1Collection.
      /// </summary>
      [JsonPropertyName("grp1Collection")]
      public Collection Grp1Collection
      {
        get => grp1Collection ??= new();
        set => grp1Collection = value;
      }

      /// <summary>
      /// A value of ApplyToCod1.
      /// </summary>
      [JsonPropertyName("applyToCod1")]
      public TextWorkArea ApplyToCod1
      {
        get => applyToCod1 ??= new();
        set => applyToCod1 = value;
      }

      /// <summary>
      /// A value of SuppPrsn1CsePerson.
      /// </summary>
      [JsonPropertyName("suppPrsn1CsePerson")]
      public CsePerson SuppPrsn1CsePerson
      {
        get => suppPrsn1CsePerson ??= new();
        set => suppPrsn1CsePerson = value;
      }

      /// <summary>
      /// A value of SuppPrsn1CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("suppPrsn1CsePersonsWorkSet")]
      public CsePersonsWorkSet SuppPrsn1CsePersonsWorkSet
      {
        get => suppPrsn1CsePersonsWorkSet ??= new();
        set => suppPrsn1CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Pgm1.
      /// </summary>
      [JsonPropertyName("pgm1")]
      public WorkArea Pgm1
      {
        get => pgm1 ??= new();
        set => pgm1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common grp1Common;
      private ObligationType grp1ObligationType;
      private Obligation grp1Obligation;
      private ObligationTransaction grp1Debt;
      private DebtDetail grp1DebtDetail;
      private LegalAction grp1LegalAction;
      private Collection grp1Collection;
      private TextWorkArea applyToCod1;
      private CsePerson suppPrsn1CsePerson;
      private CsePersonsWorkSet suppPrsn1CsePersonsWorkSet;
      private WorkArea pgm1;
    }

    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of Grp2Common.
      /// </summary>
      [JsonPropertyName("grp2Common")]
      public Common Grp2Common
      {
        get => grp2Common ??= new();
        set => grp2Common = value;
      }

      /// <summary>
      /// A value of Grp2ObligationType.
      /// </summary>
      [JsonPropertyName("grp2ObligationType")]
      public ObligationType Grp2ObligationType
      {
        get => grp2ObligationType ??= new();
        set => grp2ObligationType = value;
      }

      /// <summary>
      /// A value of Grp2Obligation.
      /// </summary>
      [JsonPropertyName("grp2Obligation")]
      public Obligation Grp2Obligation
      {
        get => grp2Obligation ??= new();
        set => grp2Obligation = value;
      }

      /// <summary>
      /// A value of Grp2Debt.
      /// </summary>
      [JsonPropertyName("grp2Debt")]
      public ObligationTransaction Grp2Debt
      {
        get => grp2Debt ??= new();
        set => grp2Debt = value;
      }

      /// <summary>
      /// A value of Grp2DebtDetail.
      /// </summary>
      [JsonPropertyName("grp2DebtDetail")]
      public DebtDetail Grp2DebtDetail
      {
        get => grp2DebtDetail ??= new();
        set => grp2DebtDetail = value;
      }

      /// <summary>
      /// A value of Grp2LegalAction.
      /// </summary>
      [JsonPropertyName("grp2LegalAction")]
      public LegalAction Grp2LegalAction
      {
        get => grp2LegalAction ??= new();
        set => grp2LegalAction = value;
      }

      /// <summary>
      /// A value of Grp2Collection.
      /// </summary>
      [JsonPropertyName("grp2Collection")]
      public Collection Grp2Collection
      {
        get => grp2Collection ??= new();
        set => grp2Collection = value;
      }

      /// <summary>
      /// A value of ApplyToCod2.
      /// </summary>
      [JsonPropertyName("applyToCod2")]
      public TextWorkArea ApplyToCod2
      {
        get => applyToCod2 ??= new();
        set => applyToCod2 = value;
      }

      /// <summary>
      /// A value of SuppPrsn2CsePerson.
      /// </summary>
      [JsonPropertyName("suppPrsn2CsePerson")]
      public CsePerson SuppPrsn2CsePerson
      {
        get => suppPrsn2CsePerson ??= new();
        set => suppPrsn2CsePerson = value;
      }

      /// <summary>
      /// A value of SuppPrsn2CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("suppPrsn2CsePersonsWorkSet")]
      public CsePersonsWorkSet SuppPrsn2CsePersonsWorkSet
      {
        get => suppPrsn2CsePersonsWorkSet ??= new();
        set => suppPrsn2CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Pgm2.
      /// </summary>
      [JsonPropertyName("pgm2")]
      public WorkArea Pgm2
      {
        get => pgm2 ??= new();
        set => pgm2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common grp2Common;
      private ObligationType grp2ObligationType;
      private Obligation grp2Obligation;
      private ObligationTransaction grp2Debt;
      private DebtDetail grp2DebtDetail;
      private LegalAction grp2LegalAction;
      private Collection grp2Collection;
      private TextWorkArea applyToCod2;
      private CsePerson suppPrsn2CsePerson;
      private CsePersonsWorkSet suppPrsn2CsePersonsWorkSet;
      private WorkArea pgm2;
    }

    /// <summary>
    /// A value of SuppPrsnCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("suppPrsnCsePersonsWorkSet")]
    public CsePersonsWorkSet SuppPrsnCsePersonsWorkSet
    {
      get => suppPrsnCsePersonsWorkSet ??= new();
      set => suppPrsnCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SuppPrsnCsePerson.
    /// </summary>
    [JsonPropertyName("suppPrsnCsePerson")]
    public CsePerson SuppPrsnCsePerson
    {
      get => suppPrsnCsePerson ??= new();
      set => suppPrsnCsePerson = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Obligation Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
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
    /// A value of Sec.
    /// </summary>
    [JsonPropertyName("sec")]
    public LegalAction Sec
    {
      get => sec ??= new();
      set => sec = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public DebtDetail Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of PlusOne.
    /// </summary>
    [JsonPropertyName("plusOne")]
    public Common PlusOne
    {
      get => plusOne ??= new();
      set => plusOne = value;
    }

    /// <summary>
    /// A value of ContinueLoopingInd.
    /// </summary>
    [JsonPropertyName("continueLoopingInd")]
    public Common ContinueLoopingInd
    {
      get => continueLoopingInd ??= new();
      set => continueLoopingInd = value;
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
    /// A value of ToFilter.
    /// </summary>
    [JsonPropertyName("toFilter")]
    public DebtDetail ToFilter
    {
      get => toFilter ??= new();
      set => toFilter = value;
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
    /// Gets a value of Group1.
    /// </summary>
    [JsonIgnore]
    public Array<Group1Group> Group1 => group1 ??= new(Group1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group1 for json serialization.
    /// </summary>
    [JsonPropertyName("group1")]
    [Computed]
    public IList<Group1Group> Group1_Json
    {
      get => group1;
      set => Group1.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group2.
    /// </summary>
    [JsonIgnore]
    public Array<Group2Group> Group2 => group2 ??= new(Group2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group2 for json serialization.
    /// </summary>
    [JsonPropertyName("group2")]
    [Computed]
    public IList<Group2Group> Group2_Json
    {
      get => group2;
      set => Group2.Assign(value);
    }

    /// <summary>
    /// A value of HardcodedPrimSecRlnRsn.
    /// </summary>
    [JsonPropertyName("hardcodedPrimSecRlnRsn")]
    public ObligationRlnRsn HardcodedPrimSecRlnRsn
    {
      get => hardcodedPrimSecRlnRsn ??= new();
      set => hardcodedPrimSecRlnRsn = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveral.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveral")]
    public Obligation HardcodedJointSeveral
    {
      get => hardcodedJointSeveral ??= new();
      set => hardcodedJointSeveral = value;
    }

    /// <summary>
    /// A value of HardcodedPrimary.
    /// </summary>
    [JsonPropertyName("hardcodedPrimary")]
    public Obligation HardcodedPrimary
    {
      get => hardcodedPrimary ??= new();
      set => hardcodedPrimary = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public DateWorkArea Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of DistPgm.
    /// </summary>
    [JsonPropertyName("distPgm")]
    public Program DistPgm
    {
      get => distPgm ??= new();
      set => distPgm = value;
    }

    private CsePersonsWorkSet suppPrsnCsePersonsWorkSet;
    private CsePerson suppPrsnCsePerson;
    private Obligation hold;
    private Array<GroupGroup> group;
    private LegalAction nullLegalAction;
    private LegalAction legalAction;
    private LegalAction sec;
    private ObligationType hardcodedVoluntary;
    private Common omitCrdInd;
    private DebtDetail compare;
    private Common plusOne;
    private Common continueLoopingInd;
    private DateWorkArea current;
    private DebtDetail toFilter;
    private DateWorkArea nullDateWorkArea;
    private Array<Group1Group> group1;
    private Array<Group2Group> group2;
    private ObligationRlnRsn hardcodedPrimSecRlnRsn;
    private Obligation hardcodedJointSeveral;
    private Obligation hardcodedPrimary;
    private Obligation hardcodedSecondary;
    private ObligationType hardcodedAccruingClass;
    private DateWorkArea collection;
    private DateWorkArea tmp;
    private DprProgram dprProgram;
    private Program distPgm;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligorKeyOnly.
    /// </summary>
    [JsonPropertyName("existingObligorKeyOnly")]
    public CsePerson ExistingObligorKeyOnly
    {
      get => existingObligorKeyOnly ??= new();
      set => existingObligorKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingSupportedKeyOnly.
    /// </summary>
    [JsonPropertyName("existingSupportedKeyOnly")]
    public CsePerson ExistingSupportedKeyOnly
    {
      get => existingSupportedKeyOnly ??= new();
      set => existingSupportedKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlySupported.
    /// </summary>
    [JsonPropertyName("existingKeyOnlySupported")]
    public CsePersonAccount ExistingKeyOnlySupported
    {
      get => existingKeyOnlySupported ??= new();
      set => existingKeyOnlySupported = value;
    }

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
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("existingManualDistributionAudit")]
    public ManualDistributionAudit ExistingManualDistributionAudit
    {
      get => existingManualDistributionAudit ??= new();
      set => existingManualDistributionAudit = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingSecObligorKeyOnly.
    /// </summary>
    [JsonPropertyName("existingSecObligorKeyOnly")]
    public CsePerson ExistingSecObligorKeyOnly
    {
      get => existingSecObligorKeyOnly ??= new();
      set => existingSecObligorKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingSecKeyOnly.
    /// </summary>
    [JsonPropertyName("existingSecKeyOnly")]
    public CsePersonAccount ExistingSecKeyOnly
    {
      get => existingSecKeyOnly ??= new();
      set => existingSecKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingSecObligationType.
    /// </summary>
    [JsonPropertyName("existingSecObligationType")]
    public ObligationType ExistingSecObligationType
    {
      get => existingSecObligationType ??= new();
      set => existingSecObligationType = value;
    }

    /// <summary>
    /// A value of ExistingSecObligation.
    /// </summary>
    [JsonPropertyName("existingSecObligation")]
    public Obligation ExistingSecObligation
    {
      get => existingSecObligation ??= new();
      set => existingSecObligation = value;
    }

    /// <summary>
    /// A value of ExistingPrimSecRlnRsn.
    /// </summary>
    [JsonPropertyName("existingPrimSecRlnRsn")]
    public ObligationRlnRsn ExistingPrimSecRlnRsn
    {
      get => existingPrimSecRlnRsn ??= new();
      set => existingPrimSecRlnRsn = value;
    }

    /// <summary>
    /// A value of ExistingPrimSecRln.
    /// </summary>
    [JsonPropertyName("existingPrimSecRln")]
    public ObligationRln ExistingPrimSecRln
    {
      get => existingPrimSecRln ??= new();
      set => existingPrimSecRln = value;
    }

    /// <summary>
    /// A value of ExistingSecLegalAction.
    /// </summary>
    [JsonPropertyName("existingSecLegalAction")]
    public LegalAction ExistingSecLegalAction
    {
      get => existingSecLegalAction ??= new();
      set => existingSecLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligationType.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligationType")]
    public ObligationType ExistingKeyOnlyObligationType
    {
      get => existingKeyOnlyObligationType ??= new();
      set => existingKeyOnlyObligationType = value;
    }

    private CsePerson existingObligorKeyOnly;
    private CsePersonAccount existingKeyOnlyObligor;
    private CsePerson existingSupportedKeyOnly;
    private CsePersonAccount existingKeyOnlySupported;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ManualDistributionAudit existingManualDistributionAudit;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private LegalAction existingLegalAction;
    private CsePerson existingSecObligorKeyOnly;
    private CsePersonAccount existingSecKeyOnly;
    private ObligationType existingSecObligationType;
    private Obligation existingSecObligation;
    private ObligationRlnRsn existingPrimSecRlnRsn;
    private ObligationRln existingPrimSecRln;
    private LegalAction existingSecLegalAction;
    private ObligationType existingKeyOnlyObligationType;
  }
#endregion
}
