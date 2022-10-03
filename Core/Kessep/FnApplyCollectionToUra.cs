// Program: FN_APPLY_COLLECTION_TO_URA, ID: 374486079, model: 746.
// Short name: SWE02529
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_APPLY_COLLECTION_TO_URA.
/// </summary>
[Serializable]
public partial class FnApplyCollectionToUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_APPLY_COLLECTION_TO_URA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnApplyCollectionToUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnApplyCollectionToUra.
  /// </summary>
  public FnApplyCollectionToUra(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // : Extract the year and month into separate attributes.
    local.DueDt.Year = Year(import.DebtDetail.DueDt);
    local.DueDt.Month = Month(import.DebtDetail.DueDt);
    local.CollDt.Year = Year(import.Collection1.Date);
    local.CollDt.Month = Month(import.Collection1.Date);
    MoveDateWorkArea(local.DueDt, local.Oldest);
    local.Collection.Amount = import.Collection2.Amount;

    // : Apply the collection at the member level first.
    for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
      import.HhHist.Index)
    {
      if (Equal(import.SuppPrsn.Number, import.HhHist.Item.HhHistSuppPrsn.Number))
        
      {
        // : Apply to the Debt Detail Due Date Month/Year first, then oldest 
        // first.
        if (local.DueDt.Year > local.CollDt.Year || local.DueDt.Year == local
          .CollDt.Year && local.DueDt.Month > local.CollDt.Month)
        {
          // : If the Debt Detail Due Date is greater than the Collection Date, 
          // we can not apply the Collection to the URA as of the Due Date.
        }
        else
        {
          for(import.HhHist.Item.HhHistDtl.Index = 0; import
            .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
            import.HhHist.Item.HhHistDtl.Index)
          {
            if (local.DueDt.Year == import
              .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                Year && local.DueDt.Month == import
              .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                Month)
            {
              if (import.ObligationType.SystemGeneratedIdentifier == import
                .HardcodedMcType.SystemGeneratedIdentifier || import
                .ObligationType.SystemGeneratedIdentifier == import
                .HardcodedMjType.SystemGeneratedIdentifier || import
                .ObligationType.SystemGeneratedIdentifier == import
                .HardcodedMsType.SystemGeneratedIdentifier)
              {
                if (import.HhHist.Item.HhHistDtl.Item.
                  HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                    GetValueOrDefault() <= 0)
                {
                  continue;
                }

                if (local.Collection.Amount > import
                  .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                    UraMedicalAmount.GetValueOrDefault())
                {
                  if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                  {
                    local.ForUpdate.CollectionAmountApplied =
                      import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault();
                    UseFnBuildUraCollApplUpdUra();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }

                  local.Collection.Amount -= import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault();
                  import.HhHist.Update.HhHistDtl.Update.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount = 0;
                }
                else
                {
                  if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                  {
                    local.ForUpdate.CollectionAmountApplied =
                      local.Collection.Amount;
                    UseFnBuildUraCollApplUpdUra();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }

                  import.HhHist.Update.HhHistDtl.Update.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                      import.HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault() - local.Collection.Amount;

                  return;
                }
              }
              else
              {
                if (import.HhHist.Item.HhHistDtl.Item.
                  HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                    GetValueOrDefault() <= 0)
                {
                  continue;
                }

                if (local.Collection.Amount > import
                  .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                    UraAmount.GetValueOrDefault())
                {
                  if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                  {
                    local.ForUpdate.CollectionAmountApplied =
                      import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault();
                    UseFnBuildUraCollApplUpdUra();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }

                  local.Collection.Amount -= import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault();
                  import.HhHist.Update.HhHistDtl.Update.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount = 0;
                }
                else
                {
                  if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                  {
                    local.ForUpdate.CollectionAmountApplied =
                      local.Collection.Amount;
                    UseFnBuildUraCollApplUpdUra();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }

                  import.HhHist.Update.HhHistDtl.Update.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                      import.HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault() - local.Collection.Amount;

                  return;
                }
              }
            }
          }
        }

        // : Now apply to the oldest first for the member up to the Month/Year 
        // of the Collection Date.
        for(import.HhHist.Item.HhHistDtl.Index = 0; import
          .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
          import.HhHist.Item.HhHistDtl.Index)
        {
          if (import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Year == local.CollDt.Year)
          {
            if (import.HhHist.Item.HhHistDtl.Item.
              HhHistDtlImHouseholdMbrMnthlySum.Month > local.CollDt.Month)
            {
              continue;
            }
          }
          else if (import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Year > local.CollDt.Year)
          {
            continue;
          }

          if (import.ObligationType.SystemGeneratedIdentifier == import
            .HardcodedMcType.SystemGeneratedIdentifier || import
            .ObligationType.SystemGeneratedIdentifier == import
            .HardcodedMjType.SystemGeneratedIdentifier || import
            .ObligationType.SystemGeneratedIdentifier == import
            .HardcodedMsType.SystemGeneratedIdentifier)
          {
            if (import.HhHist.Item.HhHistDtl.Item.
              HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                GetValueOrDefault() <= 0)
            {
              continue;
            }

            if (local.Collection.Amount > import
              .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                UraMedicalAmount.GetValueOrDefault())
            {
              if (AsChar(import.ApplyUpdates.Flag) == 'Y')
              {
                local.ForUpdate.CollectionAmountApplied =
                  import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                    GetValueOrDefault();
                UseFnBuildUraCollApplUpdUra();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              local.Collection.Amount -= import.HhHist.Item.HhHistDtl.Item.
                HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                  GetValueOrDefault();
              import.HhHist.Update.HhHistDtl.Update.
                HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount = 0;
            }
            else
            {
              if (AsChar(import.ApplyUpdates.Flag) == 'Y')
              {
                local.ForUpdate.CollectionAmountApplied =
                  local.Collection.Amount;
                UseFnBuildUraCollApplUpdUra();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              import.HhHist.Update.HhHistDtl.Update.
                HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                  import.HhHist.Item.HhHistDtl.Item.
                  HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                  GetValueOrDefault() - local.Collection.Amount;

              return;
            }
          }
          else
          {
            if (import.HhHist.Item.HhHistDtl.Item.
              HhHistDtlImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault() <=
                0)
            {
              continue;
            }

            if (local.Collection.Amount > import
              .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                UraAmount.GetValueOrDefault())
            {
              if (AsChar(import.ApplyUpdates.Flag) == 'Y')
              {
                local.ForUpdate.CollectionAmountApplied =
                  import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                    GetValueOrDefault();
                UseFnBuildUraCollApplUpdUra();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              local.Collection.Amount -= import.HhHist.Item.HhHistDtl.Item.
                HhHistDtlImHouseholdMbrMnthlySum.UraAmount.GetValueOrDefault();
              import.HhHist.Update.HhHistDtl.Update.
                HhHistDtlImHouseholdMbrMnthlySum.UraAmount = 0;
            }
            else
            {
              if (AsChar(import.ApplyUpdates.Flag) == 'Y')
              {
                local.ForUpdate.CollectionAmountApplied =
                  local.Collection.Amount;
                UseFnBuildUraCollApplUpdUra();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              import.HhHist.Update.HhHistDtl.Update.
                HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                  import.HhHist.Item.HhHistDtl.Item.
                  HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                  GetValueOrDefault() - local.Collection.Amount;

              return;
            }
          }
        }
      }
    }

    // : Now we can apply the collection to all Members related by Court Order (
    // Legal Action).
    // : Get the Legal Actions for the Member & place in a Local Group view.
    for(import.Legal.Index = 0; import.Legal.Index < import.Legal.Count; ++
      import.Legal.Index)
    {
      if (Equal(import.SuppPrsn.Number, import.Legal.Item.LegalSuppPrsn.Number))
      {
        local.Legal.Index = 0;
        local.Legal.Clear();

        for(import.Legal.Item.LegalDtl.Index = 0; import
          .Legal.Item.LegalDtl.Index < import.Legal.Item.LegalDtl.Count; ++
          import.Legal.Item.LegalDtl.Index)
        {
          if (local.Legal.IsFull)
          {
            break;
          }

          if (!IsEmpty(import.LegalAction.StandardNumber))
          {
            if (!Equal(import.LegalAction.StandardNumber,
              import.Legal.Item.LegalDtl.Item.LegalDtl1.StandardNumber))
            {
              local.Legal.Next();

              continue;
            }
          }

          local.Legal.Update.Legal1.StandardNumber =
            import.Legal.Item.LegalDtl.Item.LegalDtl1.StandardNumber;
          local.Legal.Next();
        }

        break;
      }
    }

    // : If there are no Legal Actions, then use ONLY the Member that was passed
    // in.
    if (local.Legal.IsEmpty)
    {
      // : Use only the Supported Person passed in.
      local.SuppPrsnLegal.Index = 0;
      local.SuppPrsnLegal.CheckSize();

      local.SuppPrsnLegal.Update.SuppPrsnLegal1.Number = import.SuppPrsn.Number;
    }
    else
    {
      // : Find all of the Member's associated to the Legal Actions.
      for(local.Legal.Index = 0; local.Legal.Index < local.Legal.Count; ++
        local.Legal.Index)
      {
        for(import.Legal.Index = 0; import.Legal.Index < import.Legal.Count; ++
          import.Legal.Index)
        {
          for(import.Legal.Item.LegalDtl.Index = 0; import
            .Legal.Item.LegalDtl.Index < import.Legal.Item.LegalDtl.Count; ++
            import.Legal.Item.LegalDtl.Index)
          {
            if (Equal(import.Legal.Item.LegalDtl.Item.LegalDtl1.StandardNumber,
              local.Legal.Item.Legal1.StandardNumber))
            {
              if (local.SuppPrsnLegal.IsEmpty)
              {
                local.SuppPrsnLegal.Index = 0;
                local.SuppPrsnLegal.CheckSize();
              }
              else
              {
                for(local.SuppPrsnLegal.Index = 0; local.SuppPrsnLegal.Index < local
                  .SuppPrsnLegal.Count; ++local.SuppPrsnLegal.Index)
                {
                  if (!local.SuppPrsnLegal.CheckSize())
                  {
                    break;
                  }

                  if (Equal(import.Legal.Item.LegalSuppPrsn.Number,
                    local.SuppPrsnLegal.Item.SuppPrsnLegal1.Number))
                  {
                    goto Next1;
                  }
                }

                local.SuppPrsnLegal.CheckIndex();

                local.SuppPrsnLegal.Index = local.SuppPrsnLegal.Count;
                local.SuppPrsnLegal.CheckSize();
              }

              local.SuppPrsnLegal.Update.SuppPrsnLegal1.Number =
                import.Legal.Item.LegalSuppPrsn.Number;

              goto Next1;
            }
          }

Next1:
          ;
        }
      }
    }

    // : Get the oldest Year/Month for processing.
    for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
      import.HhHist.Index)
    {
      for(import.HhHist.Item.HhHistDtl.Index = 0; import
        .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
        import.HhHist.Item.HhHistDtl.Index)
      {
        if (import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
          Year > local.Oldest.Year)
        {
          continue;
        }

        if (import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
          Year == local.Oldest.Year && import
          .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.Month >=
            local.Oldest.Month)
        {
          continue;
        }

        local.Oldest.Month =
          import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
            Month;
        local.Oldest.Year =
          import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
            Year;
      }
    }

    // : Apply the collection at the Court Order level - Current Debt Detail Due
    // Date month/year.
    if (local.DueDt.Year > local.CollDt.Year || local.DueDt.Year == local
      .CollDt.Year && local.DueDt.Month > local.CollDt.Month)
    {
      // : If the Debt Detail Due Date is greater than the Collection Date, we 
      // can not apply the Collection to the URA as of the Due Date.
    }
    else
    {
      local.SuppPrsnLegal.Index = 0;

      for(var limit = local.SuppPrsnLegal.Count; local.SuppPrsnLegal.Index < limit
        ; ++local.SuppPrsnLegal.Index)
      {
        if (!local.SuppPrsnLegal.CheckSize())
        {
          break;
        }

        for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
          import.HhHist.Index)
        {
          if (Equal(local.SuppPrsnLegal.Item.SuppPrsnLegal1.Number,
            import.HhHist.Item.HhHistSuppPrsn.Number))
          {
            // : Apply to the Debt Detail Due Date Month/Year first, then oldest
            // first.
            for(import.HhHist.Item.HhHistDtl.Index = 0; import
              .HhHist.Item.HhHistDtl.Index < import
              .HhHist.Item.HhHistDtl.Count; ++
              import.HhHist.Item.HhHistDtl.Index)
            {
              if (local.DueDt.Year == import
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Year && local.DueDt.Month == import
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Month)
              {
                if (import.ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMcType.SystemGeneratedIdentifier || import
                  .ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMjType.SystemGeneratedIdentifier || import
                  .ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMsType.SystemGeneratedIdentifier)
                {
                  if (import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault() <= 0)
                  {
                    continue;
                  }

                  if (local.Collection.Amount > import
                    .HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault())
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                          GetValueOrDefault();
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                      Item.HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault();
                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount = 0;
                  }
                  else
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        local.Collection.Amount;
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                        import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault() - local.Collection.Amount;

                    return;
                  }
                }
                else
                {
                  if (import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault() <= 0)
                  {
                    continue;
                  }

                  if (local.Collection.Amount > import
                    .HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault())
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                          GetValueOrDefault();
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                      Item.HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault();
                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount = 0;
                  }
                  else
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        local.Collection.Amount;
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                        import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault() - local.Collection.Amount;

                    return;
                  }
                }
              }
            }
          }
        }
      }

      local.SuppPrsnLegal.CheckIndex();
    }

    // : Apply the collection at the Court Order level - Process oldest first up
    // to the month/year of the Collection Date.
    MoveDateWorkArea(local.Oldest, local.ProcIncrement);

    while(local.ProcIncrement.Year < local.CollDt.Year || local
      .ProcIncrement.Year == local.CollDt.Year && local.ProcIncrement.Month <= local
      .CollDt.Month)
    {
      local.SuppPrsnLegal.Index = 0;

      for(var limit = local.SuppPrsnLegal.Count; local.SuppPrsnLegal.Index < limit
        ; ++local.SuppPrsnLegal.Index)
      {
        if (!local.SuppPrsnLegal.CheckSize())
        {
          break;
        }

        for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
          import.HhHist.Index)
        {
          if (Equal(local.SuppPrsnLegal.Item.SuppPrsnLegal1.Number,
            import.HhHist.Item.HhHistSuppPrsn.Number))
          {
            // : Now apply to the oldest first for the member up to the Month/
            // Year of the Collection Date.
            for(import.HhHist.Item.HhHistDtl.Index = 0; import
              .HhHist.Item.HhHistDtl.Index < import
              .HhHist.Item.HhHistDtl.Count; ++
              import.HhHist.Item.HhHistDtl.Index)
            {
              if (import.HhHist.Item.HhHistDtl.Item.
                HhHistDtlImHouseholdMbrMnthlySum.Year == local
                .ProcIncrement.Year && import
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Month == local.ProcIncrement.Month)
              {
                if (import.ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMcType.SystemGeneratedIdentifier || import
                  .ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMjType.SystemGeneratedIdentifier || import
                  .ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMsType.SystemGeneratedIdentifier)
                {
                  if (import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault() <= 0)
                  {
                    continue;
                  }

                  if (local.Collection.Amount > import
                    .HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault())
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                          GetValueOrDefault();
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                      Item.HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault();
                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount = 0;
                  }
                  else
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        local.Collection.Amount;
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                        import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault() - local.Collection.Amount;

                    return;
                  }
                }
                else
                {
                  if (import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault() <= 0)
                  {
                    continue;
                  }

                  if (local.Collection.Amount > import
                    .HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault())
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                          GetValueOrDefault();
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                      Item.HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault();
                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount = 0;
                  }
                  else
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        local.Collection.Amount;
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                        import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault() - local.Collection.Amount;

                    return;
                  }
                }
              }
            }
          }
        }
      }

      local.SuppPrsnLegal.CheckIndex();

      if (local.ProcIncrement.Month == 12)
      {
        local.ProcIncrement.Month = 1;
        ++local.ProcIncrement.Year;
      }
      else
      {
        ++local.ProcIncrement.Month;
      }
    }

    // : Now we apply the Collection at the Household Level.
    // : Get each AE Case No  where the Member (any Legal Action related Member)
    // is a Child "CH" or Primary Individual "PI" Household.
    for(local.SuppPrsnLegal.Index = 0; local.SuppPrsnLegal.Index < local
      .SuppPrsnLegal.Count; ++local.SuppPrsnLegal.Index)
    {
      if (!local.SuppPrsnLegal.CheckSize())
      {
        break;
      }

      for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
        import.HhHist.Index)
      {
        if (!Equal(import.HhHist.Item.HhHistSuppPrsn.Number,
          local.SuppPrsnLegal.Item.SuppPrsnLegal1.Number))
        {
          continue;
        }

        for(import.HhHist.Item.HhHistDtl.Index = 0; import
          .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
          import.HhHist.Item.HhHistDtl.Index)
        {
          if (import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Year > local.CollDt.Year)
          {
            continue;
          }
          else if (import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Year == local.CollDt.Year && import
            .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
              Month > local.CollDt.Month)
          {
            continue;
          }

          if (Equal(import.HhHist.Item.HhHistDtl.Item.
            HhHistDtlImHouseholdMbrMnthlySum.Relationship, "CH") || Equal
            (import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
              Relationship, "PI"))
          {
            if (local.Hh.IsEmpty)
            {
              local.Hh.Index = 0;
              local.Hh.CheckSize();
            }
            else
            {
              for(local.Hh.Index = 0; local.Hh.Index < local.Hh.Count; ++
                local.Hh.Index)
              {
                if (!local.Hh.CheckSize())
                {
                  break;
                }

                if (Equal(import.HhHist.Item.HhHistDtl.Item.
                  HhHistDtlImHousehold.AeCaseNo, local.Hh.Item.Hh1.AeCaseNo))
                {
                  goto Next2;
                }
              }

              local.Hh.CheckIndex();

              local.Hh.Index = local.Hh.Count;
              local.Hh.CheckSize();
            }

            local.Hh.Update.Hh1.AeCaseNo =
              import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo;
          }

Next2:
          ;
        }
      }
    }

    local.SuppPrsnLegal.CheckIndex();

    if (!local.Hh.IsEmpty)
    {
      // : Apply the collection at the Householdr level - Current Debt Detail 
      // Due Date month/year.
      if (local.DueDt.Year > local.CollDt.Year || local.DueDt.Year == local
        .CollDt.Year && local.DueDt.Month > local.CollDt.Month)
      {
        // : If the Debt Detail Due Date is greater than the Collection Date, we
        // can not apply the Collection to the URA as of the Due Date.
      }
      else
      {
        local.Hh.Index = 0;

        for(var limit = local.Hh.Count; local.Hh.Index < limit; ++
          local.Hh.Index)
        {
          if (!local.Hh.CheckSize())
          {
            break;
          }

          for(import.HhHist.Index = 0; import.HhHist.Index < import
            .HhHist.Count; ++import.HhHist.Index)
          {
            for(import.HhHist.Item.HhHistDtl.Index = 0; import
              .HhHist.Item.HhHistDtl.Index < import
              .HhHist.Item.HhHistDtl.Count; ++
              import.HhHist.Item.HhHistDtl.Index)
            {
              // : Apply only to matching Household's & for the Debt Detail Due 
              // Date.
              if (!Equal(local.Hh.Item.Hh1.AeCaseNo,
                import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.
                  AeCaseNo))
              {
                continue;
              }

              if (local.DueDt.Year == import
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Year && local.DueDt.Month == import
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Month)
              {
                if (import.ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMcType.SystemGeneratedIdentifier || import
                  .ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMjType.SystemGeneratedIdentifier || import
                  .ObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMsType.SystemGeneratedIdentifier)
                {
                  if (import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault() <= 0)
                  {
                    continue;
                  }

                  if (local.Collection.Amount > import
                    .HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                      GetValueOrDefault())
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                          GetValueOrDefault();
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                      Item.HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault();
                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount = 0;
                  }
                  else
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        local.Collection.Amount;
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                        import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault() - local.Collection.Amount;

                    return;
                  }
                }
                else
                {
                  if (import.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault() <= 0)
                  {
                    continue;
                  }

                  if (local.Collection.Amount > import
                    .HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                      GetValueOrDefault())
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                          GetValueOrDefault();
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                      Item.HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault();
                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount = 0;
                  }
                  else
                  {
                    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                    {
                      local.ForUpdate.CollectionAmountApplied =
                        local.Collection.Amount;
                      UseFnBuildUraCollApplUpdUra();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    import.HhHist.Update.HhHistDtl.Update.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                        import.HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault() - local.Collection.Amount;

                    return;
                  }
                }
              }
            }
          }
        }

        local.Hh.CheckIndex();
      }

      // : Apply the collection at the Household level - Process oldest first up
      // to the month/year of the Collection Date.
      MoveDateWorkArea(local.Oldest, local.ProcIncrement);

      while(local.ProcIncrement.Year <= local.CollDt.Year || local
        .ProcIncrement.Year == local.CollDt.Year && local
        .ProcIncrement.Month <= local.CollDt.Month)
      {
        for(import.HhHist.Index = 0; import.HhHist.Index < import.HhHist.Count; ++
          import.HhHist.Index)
        {
          // : Apply to the oldest Month/Year first within selected Households.
          for(import.HhHist.Item.HhHistDtl.Index = 0; import
            .HhHist.Item.HhHistDtl.Index < import.HhHist.Item.HhHistDtl.Count; ++
            import.HhHist.Item.HhHistDtl.Index)
          {
            if (import.HhHist.Item.HhHistDtl.Item.
              HhHistDtlImHouseholdMbrMnthlySum.Year == local
              .ProcIncrement.Year && import
              .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                Month == local.ProcIncrement.Month)
            {
              local.Hh.Index = 0;

              for(var limit = local.Hh.Count; local.Hh.Index < limit; ++
                local.Hh.Index)
              {
                if (!local.Hh.CheckSize())
                {
                  break;
                }

                // : Deleted the year = year & month = month logic.
                if (Equal(local.Hh.Item.Hh1.AeCaseNo,
                  import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.
                    AeCaseNo))
                {
                  if (import.ObligationType.SystemGeneratedIdentifier == import
                    .HardcodedMcType.SystemGeneratedIdentifier || import
                    .ObligationType.SystemGeneratedIdentifier == import
                    .HardcodedMjType.SystemGeneratedIdentifier || import
                    .ObligationType.SystemGeneratedIdentifier == import
                    .HardcodedMsType.SystemGeneratedIdentifier)
                  {
                    if (import.HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault() <= 0)
                    {
                      goto Next3;
                    }

                    if (local.Collection.Amount > import
                      .HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                        GetValueOrDefault())
                    {
                      if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                      {
                        local.ForUpdate.CollectionAmountApplied =
                          import.HhHist.Item.HhHistDtl.Item.
                            HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                            GetValueOrDefault();
                        UseFnBuildUraCollApplUpdUra();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                        Item.HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                          GetValueOrDefault();
                      import.HhHist.Update.HhHistDtl.Update.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount = 0;
                    }
                    else
                    {
                      if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                      {
                        local.ForUpdate.CollectionAmountApplied =
                          local.Collection.Amount;
                        UseFnBuildUraCollApplUpdUra();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      import.HhHist.Update.HhHistDtl.Update.
                        HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                          import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                          GetValueOrDefault() - local.Collection.Amount;

                      return;
                    }
                  }
                  else
                  {
                    if (import.HhHist.Item.HhHistDtl.Item.
                      HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault() <= 0)
                    {
                      goto Next3;
                    }

                    if (local.Collection.Amount > import
                      .HhHist.Item.HhHistDtl.Item.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                        GetValueOrDefault())
                    {
                      if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                      {
                        local.ForUpdate.CollectionAmountApplied =
                          import.HhHist.Item.HhHistDtl.Item.
                            HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                            GetValueOrDefault();
                        UseFnBuildUraCollApplUpdUra();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      local.Collection.Amount -= import.HhHist.Item.HhHistDtl.
                        Item.HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                          GetValueOrDefault();
                      import.HhHist.Update.HhHistDtl.Update.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount = 0;
                    }
                    else
                    {
                      if (AsChar(import.ApplyUpdates.Flag) == 'Y')
                      {
                        local.ForUpdate.CollectionAmountApplied =
                          local.Collection.Amount;
                        UseFnBuildUraCollApplUpdUra();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }
                      }

                      import.HhHist.Update.HhHistDtl.Update.
                        HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                          import.HhHist.Item.HhHistDtl.Item.
                          HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                          GetValueOrDefault() - local.Collection.Amount;

                      return;
                    }
                  }

                  goto Next4;
                }
              }

              local.Hh.CheckIndex();

              goto Next4;
            }

Next3:
            ;
          }

Next4:
          ;
        }

        if (local.ProcIncrement.Month == 12)
        {
          local.ProcIncrement.Month = 1;
          ++local.ProcIncrement.Year;
        }
        else
        {
          ++local.ProcIncrement.Month;
        }
      }
    }

    if (local.Collection.Amount == 0)
    {
      return;
    }

    if (AsChar(import.ApplyUpdates.Flag) == 'Y')
    {
      if (local.Collection.Amount < 0)
      {
        ExitState = "FN0000_APPL_EXCDED_AF_COLL_RB";
      }
      else
      {
        ExitState = "FN0000_AF_COLL_EXCDED_URA_BAL_RB";
      }
    }
    else if (local.Collection.Amount < 0)
    {
      ExitState = "FN0000_APPL_EXCDED_AF_COLL";
    }
    else
    {
      ExitState = "FN0000_AF_COLL_EXCDED-1792810318";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Month = source.Month;
    target.Year = source.Year;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnBuildUraCollApplUpdUra()
  {
    var useImport = new FnBuildUraCollApplUpdUra.Import();
    var useExport = new FnBuildUraCollApplUpdUra.Export();

    useImport.Obligor.Number = import.Obligor.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      import.Debt.SystemGeneratedIdentifier;
    useImport.Collection.SystemGeneratedIdentifier =
      import.Collection2.SystemGeneratedIdentifier;
    useImport.Member.Number = import.HhHist.Item.HhHistSuppPrsn.Number;
    useImport.ImHousehold.AeCaseNo =
      import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo;
    useImport.ImHouseholdMbrMnthlySum.Assign(
      import.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum);
    useImport.ForUpdate.CollectionAmountApplied =
      local.ForUpdate.CollectionAmountApplied;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;

    Call(FnBuildUraCollApplUpdUra.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn")]
      public CsePerson LegalSuppPrsn
      {
        get => legalSuppPrsn ??= new();
        set => legalSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of LegalDtl.
      /// </summary>
      [JsonIgnore]
      public Array<LegalDtlGroup> LegalDtl => legalDtl ??= new(
        LegalDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of LegalDtl for json serialization.
      /// </summary>
      [JsonPropertyName("legalDtl")]
      [Computed]
      public IList<LegalDtlGroup> LegalDtl_Json
      {
        get => legalDtl;
        set => LegalDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson legalSuppPrsn;
      private Array<LegalDtlGroup> legalDtl;
    }

    /// <summary>A LegalDtlGroup group.</summary>
    [Serializable]
    public class LegalDtlGroup
    {
      /// <summary>
      /// A value of LegalDtl1.
      /// </summary>
      [JsonPropertyName("legalDtl1")]
      public LegalAction LegalDtl1
      {
        get => legalDtl1 ??= new();
        set => legalDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legalDtl1;
    }

    /// <summary>A HhHistGroup group.</summary>
    [Serializable]
    public class HhHistGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsn")]
      public CsePerson HhHistSuppPrsn
      {
        get => hhHistSuppPrsn ??= new();
        set => hhHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlGroup> HhHistDtl => hhHistDtl ??= new(
        HhHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtl")]
      [Computed]
      public IList<HhHistDtlGroup> HhHistDtl_Json
      {
        get => hhHistDtl;
        set => HhHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsn;
      private Array<HhHistDtlGroup> hhHistDtl;
    }

    /// <summary>A HhHistDtlGroup group.</summary>
    [Serializable]
    public class HhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtlImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHousehold")]
      public ImHousehold HhHistDtlImHousehold
      {
        get => hhHistDtlImHousehold ??= new();
        set => hhHistDtlImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlImHouseholdMbrMnthlySum
      {
        get => hhHistDtlImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlImHouseholdMbrMnthlySum;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Collection1.
    /// </summary>
    [JsonPropertyName("collection1")]
    public DateWorkArea Collection1
    {
      get => collection1 ??= new();
      set => collection1 = value;
    }

    /// <summary>
    /// A value of Collection2.
    /// </summary>
    [JsonPropertyName("collection2")]
    public Collection Collection2
    {
      get => collection2 ??= new();
      set => collection2 = value;
    }

    /// <summary>
    /// A value of SuppPrsn.
    /// </summary>
    [JsonPropertyName("suppPrsn")]
    public CsePerson SuppPrsn
    {
      get => suppPrsn ??= new();
      set => suppPrsn = value;
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
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHist for json serialization.
    /// </summary>
    [JsonPropertyName("hhHist")]
    [Computed]
    public IList<HhHistGroup> HhHist_Json
    {
      get => hhHist;
      set => HhHist.Assign(value);
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMjType.
    /// </summary>
    [JsonPropertyName("hardcodedMjType")]
    public ObligationType HardcodedMjType
    {
      get => hardcodedMjType ??= new();
      set => hardcodedMjType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of ApplyUpdates.
    /// </summary>
    [JsonPropertyName("applyUpdates")]
    public Common ApplyUpdates
    {
      get => applyUpdates ??= new();
      set => applyUpdates = value;
    }

    /// <summary>
    /// A value of PersistantDelMe.
    /// </summary>
    [JsonPropertyName("persistantDelMe")]
    public Collection PersistantDelMe
    {
      get => persistantDelMe ??= new();
      set => persistantDelMe = value;
    }

    private CsePerson obligor;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private DateWorkArea collection1;
    private Collection collection2;
    private CsePerson suppPrsn;
    private LegalAction legalAction;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private Common applyUpdates;
    private Collection persistantDelMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of Legal1.
      /// </summary>
      [JsonPropertyName("legal1")]
      public LegalAction Legal1
      {
        get => legal1 ??= new();
        set => legal1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legal1;
    }

    /// <summary>A SuppPrsnLegalGroup group.</summary>
    [Serializable]
    public class SuppPrsnLegalGroup
    {
      /// <summary>
      /// A value of SuppPrsnLegal1.
      /// </summary>
      [JsonPropertyName("suppPrsnLegal1")]
      public CsePerson SuppPrsnLegal1
      {
        get => suppPrsnLegal1 ??= new();
        set => suppPrsnLegal1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson suppPrsnLegal1;
    }

    /// <summary>A HhGroup group.</summary>
    [Serializable]
    public class HhGroup
    {
      /// <summary>
      /// A value of Hh1.
      /// </summary>
      [JsonPropertyName("hh1")]
      public ImHousehold Hh1
      {
        get => hh1 ??= new();
        set => hh1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2500;

      private ImHousehold hh1;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public UraCollectionApplication ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of CollDt.
    /// </summary>
    [JsonPropertyName("collDt")]
    public DateWorkArea CollDt
    {
      get => collDt ??= new();
      set => collDt = value;
    }

    /// <summary>
    /// A value of DueDt.
    /// </summary>
    [JsonPropertyName("dueDt")]
    public DateWorkArea DueDt
    {
      get => dueDt ??= new();
      set => dueDt = value;
    }

    /// <summary>
    /// A value of Oldest.
    /// </summary>
    [JsonPropertyName("oldest")]
    public DateWorkArea Oldest
    {
      get => oldest ??= new();
      set => oldest = value;
    }

    /// <summary>
    /// A value of ProcIncrement.
    /// </summary>
    [JsonPropertyName("procIncrement")]
    public DateWorkArea ProcIncrement
    {
      get => procIncrement ??= new();
      set => procIncrement = value;
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
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of SuppPrsnLegal.
    /// </summary>
    [JsonIgnore]
    public Array<SuppPrsnLegalGroup> SuppPrsnLegal => suppPrsnLegal ??= new(
      SuppPrsnLegalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SuppPrsnLegal for json serialization.
    /// </summary>
    [JsonPropertyName("suppPrsnLegal")]
    [Computed]
    public IList<SuppPrsnLegalGroup> SuppPrsnLegal_Json
    {
      get => suppPrsnLegal;
      set => SuppPrsnLegal.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hh.
    /// </summary>
    [JsonIgnore]
    public Array<HhGroup> Hh => hh ??= new(HhGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hh for json serialization.
    /// </summary>
    [JsonPropertyName("hh")]
    [Computed]
    public IList<HhGroup> Hh_Json
    {
      get => hh;
      set => Hh.Assign(value);
    }

    private UraCollectionApplication forUpdate;
    private DateWorkArea collDt;
    private DateWorkArea dueDt;
    private DateWorkArea oldest;
    private DateWorkArea procIncrement;
    private Collection collection;
    private Array<LegalGroup> legal;
    private Array<SuppPrsnLegalGroup> suppPrsnLegal;
    private Array<HhGroup> hh;
  }
#endregion
}
