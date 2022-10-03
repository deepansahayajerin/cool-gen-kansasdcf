// Program: SI_GET_DATA_INTERSTATE_ORDER_DB, ID: 373331300, model: 746.
// Short name: SWE02744
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_GET_DATA_INTERSTATE_ORDER_DB.
/// </summary>
[Serializable]
public partial class SiGetDataInterstateOrderDb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_DATA_INTERSTATE_ORDER_DB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetDataInterstateOrderDb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetDataInterstateOrderDb.
  /// </summary>
  public SiGetDataInterstateOrderDb(IContext context, Import import,
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
    // -----------------------------------------------------------------
    //                    M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------
    // 03/04/2002	M Ramirez
    // Initial Development
    // 05/02/2002	M Ramirez
    // Revised for receiving specific legal actions
    // 10/3/2003 	S Newman
    // Increased local group view size      PR#189398
    // 09/24/2010      R Mathews            CQ372 & CQ428
    // Initialize current and arrears amount work areas to correct
    // amounts reported on outgoing CSENET support orders
    // -----------------------------------------------------------------
    if (!IsEmpty(import.PrimaryAp.Number))
    {
      local.PrimaryAp.Number = import.PrimaryAp.Number;
    }
    else
    {
      local.PrimaryAp.Number = import.ZdelImportPrimaryAp.Number;
    }

    if (Equal(import.Current.Date, local.NullDateWorkArea.Date))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = import.Current.Date;
    }

    local.FirstOfMonth.Year = Year(local.Current.Date);
    local.FirstOfMonth.Month = Month(local.Current.Date);
    local.FirstOfMonth.Day = 1;
    local.FirstOfMonth.Date = IntToDate(local.FirstOfMonth.Year * 10000 + local
      .FirstOfMonth.Month * 100 + local.FirstOfMonth.Day);
    export.InterstateCase.OrderDataInd = 0;
    export.Group.Index = -1;
    local.Ldets.Index = -1;
    local.Supported.Index = -1;

    if (import.Participants.Count > 0)
    {
      local.Supported.Index = -1;

      for(import.Participants.Index = 0; import.Participants.Index < import
        .Participants.Count; ++import.Participants.Index)
      {
        if (!import.Participants.CheckSize())
        {
          break;
        }

        if (Equal(import.Participants.Item.G.Relationship, "CH") || Equal
          (import.Participants.Item.G.Relationship, "AR"))
        {
          ++local.Supported.Index;
          local.Supported.CheckSize();

          local.Supported.Update.GlocalSupported.Number =
            import.Participants.Item.GimportParticipant.Number;
        }
      }

      import.Participants.CheckIndex();
    }

    if (import.LegalActions.Count > 0)
    {
      for(import.LegalActions.Index = 0; import.LegalActions.Index < import
        .LegalActions.Count; ++import.LegalActions.Index)
      {
        if (!import.LegalActions.CheckSize())
        {
          break;
        }

        // ------------------------------------------------
        // Eliminate duplicate legal actions
        // ------------------------------------------------
        if (local.Ldets.Count > 0)
        {
          for(local.Ldets.Index = 0; local.Ldets.Index < local.Ldets.Count; ++
            local.Ldets.Index)
          {
            if (!local.Ldets.CheckSize())
            {
              break;
            }

            if (import.LegalActions.Item.G.Identifier == local
              .Ldets.Item.GlegalAction.Identifier)
            {
              goto Next;
            }
          }

          local.Ldets.CheckIndex();
        }

        if (ReadLegalAction())
        {
          if (Equal(entities.LegalAction.FiledDate, local.NullDateWorkArea.Date))
            
          {
            continue;
          }

          if (Lt(entities.LegalAction.EndDate, local.Current.Date))
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        if (!ReadFips())
        {
          continue;
        }

        foreach(var item in ReadLegalActionDetail())
        {
          local.IncludeLdet.Flag = "";

          // *** CQ372 & CQ428 Initialize work amounts for each legal action 
          // detail
          local.LegalActionDetail.ArrearsAmount = 0;
          local.LegalActionDetail.CurrentAmount = 0;

          if (!ReadCsePerson1())
          {
            continue;
          }

          if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
          {
            if (ReadObligationType())
            {
              if (AsChar(entities.ObligationType.Classification) != 'A' && AsChar
                (entities.ObligationType.Classification) != 'M' && AsChar
                (entities.ObligationType.Classification) != 'N')
              {
                continue;
              }

              if (Equal(entities.ObligationType.Code, "IJ") || Equal
                (entities.ObligationType.Code, "WA") || Equal
                (entities.ObligationType.Code, "WC"))
              {
                continue;
              }
            }
            else
            {
              continue;
            }

            for(local.Supported.Index = 0; local.Supported.Index < local
              .Supported.Count; ++local.Supported.Index)
            {
              if (!local.Supported.CheckSize())
              {
                break;
              }

              foreach(var item1 in ReadCsePersonLegalActionPerson())
              {
                local.IncludeLdet.Flag = "Y";
                local.LegalActionDetail.ArrearsAmount =
                  local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() + entities
                  .LegalActionPerson.ArrearsAmount.GetValueOrDefault();
                local.LegalActionDetail.CurrentAmount =
                  local.LegalActionDetail.CurrentAmount.GetValueOrDefault() + entities
                  .LegalActionPerson.CurrentAmount.GetValueOrDefault();
              }
            }

            local.Supported.CheckIndex();
          }
          else
          {
            if (!Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") && !
              Equal(entities.LegalActionDetail.NonFinOblgType, "UM"))
            {
              continue;
            }

            local.IncludeLdet.Flag = "Y";
          }

          if (AsChar(local.IncludeLdet.Flag) == 'Y')
          {
            if (local.Ldets.Count >= Local.LdetsGroup.Capacity)
            {
              goto AfterCycle;
            }

            ++local.Ldets.Index;
            local.Ldets.CheckSize();

            local.Ldets.Update.GlegalAction.Assign(entities.LegalAction);
            local.Ldets.Update.Gfips.Assign(entities.Fips);
            local.Ldets.Update.GlegalActionDetail.Assign(
              entities.LegalActionDetail);

            if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
            {
              local.Ldets.Update.GobligationType.Code =
                entities.ObligationType.Code;
              local.Ldets.Update.GlegalActionDetail.ArrearsAmount =
                local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
              local.Ldets.Update.GlegalActionDetail.CurrentAmount =
                local.LegalActionDetail.CurrentAmount.GetValueOrDefault();
            }
            else
            {
              local.Ldets.Update.GobligationType.Code = "";
              local.Ldets.Update.GlegalActionDetail.ArrearsAmount = 0;
              local.Ldets.Update.GlegalActionDetail.CurrentAmount = 0;
            }
          }
        }

Next:
        ;
      }

AfterCycle:

      import.LegalActions.CheckIndex();
    }
    else
    {
      if (IsEmpty(import.Case1.Number))
      {
        goto Test1;
      }

      if (IsEmpty(local.PrimaryAp.Number))
      {
        goto Test1;
      }

      foreach(var item in ReadLegalActionLegalActionDetail())
      {
        local.IncludeLdet.Flag = "";
        local.LegalActionDetail.ArrearsAmount = 0;
        local.LegalActionDetail.CurrentAmount = 0;

        if (!ReadFips())
        {
          continue;
        }

        if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
        {
          if (ReadObligationType())
          {
            if (AsChar(entities.ObligationType.Classification) != 'A' && AsChar
              (entities.ObligationType.Classification) != 'M' && AsChar
              (entities.ObligationType.Classification) != 'N')
            {
              continue;
            }

            if (Equal(entities.ObligationType.Code, "IJ") || Equal
              (entities.ObligationType.Code, "WA") || Equal
              (entities.ObligationType.Code, "WC"))
            {
              continue;
            }
          }
          else
          {
            continue;
          }

          for(local.Supported.Index = 0; local.Supported.Index < local
            .Supported.Count; ++local.Supported.Index)
          {
            if (!local.Supported.CheckSize())
            {
              break;
            }

            foreach(var item1 in ReadCsePersonLegalActionPerson())
            {
              local.IncludeLdet.Flag = "Y";
              local.LegalActionDetail.ArrearsAmount =
                local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() + entities
                .LegalActionPerson.ArrearsAmount.GetValueOrDefault();
              local.LegalActionDetail.CurrentAmount =
                local.LegalActionDetail.CurrentAmount.GetValueOrDefault() + entities
                .LegalActionPerson.CurrentAmount.GetValueOrDefault();
            }
          }

          local.Supported.CheckIndex();
        }
        else
        {
          if (!Equal(entities.LegalActionDetail.NonFinOblgType, "HIC") && !
            Equal(entities.LegalActionDetail.NonFinOblgType, "UM"))
          {
            continue;
          }

          local.IncludeLdet.Flag = "Y";
        }

        if (AsChar(local.IncludeLdet.Flag) == 'Y')
        {
          if (local.Ldets.Count >= Local.LdetsGroup.Capacity)
          {
            break;
          }

          ++local.Ldets.Index;
          local.Ldets.CheckSize();

          local.Ldets.Update.GlegalAction.Assign(entities.LegalAction);
          local.Ldets.Update.Gfips.Assign(entities.Fips);
          local.Ldets.Update.GlegalActionDetail.Assign(
            entities.LegalActionDetail);

          if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
          {
            local.Ldets.Update.GobligationType.Code =
              entities.ObligationType.Code;
            local.Ldets.Update.GlegalActionDetail.ArrearsAmount =
              local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
            local.Ldets.Update.GlegalActionDetail.CurrentAmount =
              local.LegalActionDetail.CurrentAmount.GetValueOrDefault();
          }
          else
          {
            local.Ldets.Update.GobligationType.Code = "";
            local.Ldets.Update.GlegalActionDetail.ArrearsAmount = 0;
            local.Ldets.Update.GlegalActionDetail.CurrentAmount = 0;
          }
        }
      }
    }

Test1:

    if (local.Ldets.Count == 0)
    {
      return;
    }

    local.Ldets.Index = 0;

    for(var limit = local.Ldets.Count; local.Ldets.Index < limit; ++
      local.Ldets.Index)
    {
      if (!local.Ldets.CheckSize())
      {
        break;
      }

      local.New1.Assign(local.NullInterstateSupportOrder);
      local.New1.SystemGeneratedSequenceNum = 0;
      local.New1.FipsState = NumberToString(local.Ldets.Item.Gfips.State, 2);
      local.New1.FipsCounty = NumberToString(local.Ldets.Item.Gfips.County, 3);
      local.New1.FipsLocation =
        NumberToString(local.Ldets.Item.Gfips.Location, 2);
      local.New1.Number = local.Ldets.Item.GlegalAction.CourtCaseNumber ?? Spaces
        (17);

      if (AsChar(local.Ldets.Item.GlegalActionDetail.DetailType) == 'F')
      {
        switch(TrimEnd(local.Ldets.Item.GobligationType.Code))
        {
          case "AJ":
            local.New1.DebtType = "CS";

            break;
          case "CRCH":
            local.New1.DebtType = "CS";

            break;
          case "CS":
            local.New1.DebtType = "CS";

            break;
          case "MC":
            local.New1.DebtType = "MS";

            break;
          case "MJ":
            local.New1.DebtType = "MS";

            break;
          case "MS":
            local.New1.DebtType = "MS";

            break;
          case "SAJ":
            local.New1.DebtType = "SS";

            break;
          case "SP":
            local.New1.DebtType = "SS";

            break;
          case "718B":
            local.New1.DebtType = "CS";

            break;
          default:
            break;
        }
      }
      else
      {
        switch(TrimEnd(local.Ldets.Item.GlegalActionDetail.NonFinOblgType ?? ""))
          
        {
          case "HIC":
            local.New1.DebtType = "MS";

            break;
          case "UM":
            local.New1.DebtType = "MS";

            break;
          default:
            break;
        }
      }

      // ----------------------------------------------------------
      // Find the subscript to use for this LDET
      // Use a new one if these values don't match
      // ----------------------------------------------------------
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (Equal(export.Group.Item.G.FipsState, local.New1.FipsState) && Equal
          (export.Group.Item.G.FipsCounty, local.New1.FipsCounty) && Equal
          (export.Group.Item.G.FipsLocation, local.New1.FipsLocation) && Equal
          (export.Group.Item.G.Number, local.New1.Number) && Equal
          (export.Group.Item.G.DebtType, local.New1.DebtType))
        {
          local.New1.SystemGeneratedSequenceNum = export.Group.Index + 1;

          break;
        }
      }

      export.Group.CheckIndex();

      if (local.New1.SystemGeneratedSequenceNum == 0)
      {
        // ----------------------------------------------------------
        // This is a new order datablock
        // ----------------------------------------------------------
        if (export.InterstateCase.OrderDataInd.GetValueOrDefault() >= 9)
        {
          // ----------------------------------------------------------
          // There is no room for another order, but we could add some
          // other orders to existing orders, so continue looping
          // ----------------------------------------------------------
          continue;
        }

        export.InterstateCase.OrderDataInd =
          export.InterstateCase.OrderDataInd.GetValueOrDefault() + 1;

        export.Group.Index =
          export.InterstateCase.OrderDataInd.GetValueOrDefault() - 1;
        export.Group.CheckSize();

        export.Group.Update.G.SystemGeneratedSequenceNum =
          export.Group.Index + 1;
        export.Group.Update.G.FipsState = local.New1.FipsState;
        export.Group.Update.G.FipsCounty = local.New1.FipsCounty ?? "";
        export.Group.Update.G.FipsLocation = local.New1.FipsLocation ?? "";
        export.Group.Update.G.Number = local.New1.Number;
        export.Group.Update.G.OrderFilingDate =
          local.Ldets.Item.GlegalAction.FiledDate;
        export.Group.Update.G.Type1 =
          local.Ldets.Item.GlegalAction.OrderAuthority;
        export.Group.Update.G.DebtType = local.New1.DebtType;

        if (local.Ldets.Item.GlegalActionDetail.CurrentAmount.
          GetValueOrDefault() > 0)
        {
          export.Group.Update.G.PaymentFreq =
            local.Ldets.Item.GlegalActionDetail.FreqPeriodCode ?? "";
          export.Group.Update.G.AmountOrdered =
            local.Ldets.Item.GlegalActionDetail.CurrentAmount.
              GetValueOrDefault();
        }

        if (Lt(local.NullDateWorkArea.Date,
          local.Ldets.Item.GlegalActionDetail.EffectiveDate))
        {
          export.Group.Update.G.EffectiveDate =
            local.Ldets.Item.GlegalActionDetail.EffectiveDate;
        }
        else
        {
          export.Group.Update.G.EffectiveDate =
            local.Ldets.Item.GlegalAction.FiledDate;
        }

        export.Group.Update.G.EndDate =
          local.Ldets.Item.GlegalActionDetail.EndDate;

        if (local.Ldets.Item.GlegalActionDetail.ArrearsAmount.
          GetValueOrDefault() > 0)
        {
          export.Group.Update.G.ArrearsFreq =
            local.Ldets.Item.GlegalActionDetail.FreqPeriodCode ?? "";
          export.Group.Update.G.ArrearsFreqAmount =
            local.Ldets.Item.GlegalActionDetail.ArrearsAmount.
              GetValueOrDefault();
        }

        if (Equal(export.Group.Item.G.DebtType, "MS"))
        {
          export.Group.Update.G.MedicalOrderedInd = "Y";
        }
        else
        {
          export.Group.Update.G.MedicalOrderedInd = "N";
        }

        export.Group.Update.G.TribunalCaseNumber = local.New1.Number;
      }
      else
      {
        // ----------------------------------------------------------
        // This order datablock was already here, we are just
        // adding to it
        // (A different LDET, same LA has the same obligation type)
        // ----------------------------------------------------------
        export.Group.Index = local.New1.SystemGeneratedSequenceNum - 1;
        export.Group.CheckSize();

        if (Lt(local.Ldets.Item.GlegalAction.FiledDate,
          export.Group.Item.G.OrderFilingDate))
        {
          export.Group.Update.G.OrderFilingDate =
            local.Ldets.Item.GlegalAction.FiledDate;
        }

        if (local.Ldets.Item.GlegalActionDetail.CurrentAmount.
          GetValueOrDefault() > 0)
        {
          if (export.Group.Item.G.AmountOrdered.GetValueOrDefault() == 0)
          {
            export.Group.Update.G.PaymentFreq =
              local.Ldets.Item.GlegalActionDetail.FreqPeriodCode ?? "";
            export.Group.Update.G.AmountOrdered =
              local.Ldets.Item.GlegalActionDetail.CurrentAmount.
                GetValueOrDefault();
          }
          else if (Equal(local.Ldets.Item.GlegalActionDetail.FreqPeriodCode,
            export.Group.Item.G.PaymentFreq))
          {
            export.Group.Update.G.AmountOrdered =
              export.Group.Item.G.AmountOrdered.GetValueOrDefault() + local
              .Ldets.Item.GlegalActionDetail.CurrentAmount.GetValueOrDefault();
          }
          else
          {
            switch(TrimEnd(export.Group.Item.G.PaymentFreq ?? ""))
            {
              case "BW":
                local.ConvertAmt1.Count = 26;

                break;
              case "SM":
                local.ConvertAmt1.Count = 24;

                break;
              case "W":
                local.ConvertAmt1.Count = 52;

                break;
              default:
                local.ConvertAmt1.Count = 12;

                break;
            }

            local.ConvertAmt1.TotalCurrency =
              export.Group.Item.G.AmountOrdered.GetValueOrDefault() * local
              .ConvertAmt1.Count;

            switch(TrimEnd(local.Ldets.Item.GlegalActionDetail.
              FreqPeriodCode ?? ""))
            {
              case "BW":
                local.ConvertAmt2.Count = 26;

                break;
              case "SM":
                local.ConvertAmt2.Count = 24;

                break;
              case "W":
                local.ConvertAmt2.Count = 52;

                break;
              default:
                local.ConvertAmt2.Count = 12;

                break;
            }

            local.ConvertAmt2.TotalCurrency =
              local.Ldets.Item.GlegalActionDetail.CurrentAmount.
                GetValueOrDefault() * local.ConvertAmt2.Count;

            if (local.ConvertAmt1.Count < local.ConvertAmt2.Count)
            {
              export.Group.Update.G.AmountOrdered =
                Math.Round((
                  local.ConvertAmt1.TotalCurrency + local
                .ConvertAmt2.TotalCurrency) /
                local.ConvertAmt1.Count, 2, MidpointRounding.AwayFromZero);

              switch(local.ConvertAmt1.Count)
              {
                case 26:
                  export.Group.Update.G.PaymentFreq = "BW";

                  break;
                case 24:
                  export.Group.Update.G.PaymentFreq = "SM";

                  break;
                case 52:
                  export.Group.Update.G.PaymentFreq = "W";

                  break;
                default:
                  export.Group.Update.G.PaymentFreq = "M";

                  break;
              }
            }
            else
            {
              export.Group.Update.G.AmountOrdered =
                Math.Round((
                  local.ConvertAmt1.TotalCurrency + local
                .ConvertAmt2.TotalCurrency) /
                local.ConvertAmt2.Count, 2, MidpointRounding.AwayFromZero);

              switch(local.ConvertAmt2.Count)
              {
                case 26:
                  export.Group.Update.G.PaymentFreq = "BW";

                  break;
                case 24:
                  export.Group.Update.G.PaymentFreq = "SM";

                  break;
                case 52:
                  export.Group.Update.G.PaymentFreq = "W";

                  break;
                default:
                  export.Group.Update.G.PaymentFreq = "M";

                  break;
              }
            }
          }
        }

        if (Lt(local.NullDateWorkArea.Date,
          local.Ldets.Item.GlegalActionDetail.EffectiveDate))
        {
          if (Lt(local.Ldets.Item.GlegalActionDetail.EffectiveDate,
            export.Group.Item.G.EffectiveDate))
          {
            export.Group.Update.G.EffectiveDate =
              local.Ldets.Item.GlegalActionDetail.EffectiveDate;
          }
        }

        if (Lt(export.Group.Item.G.EndDate,
          local.Ldets.Item.GlegalActionDetail.EndDate))
        {
          export.Group.Update.G.EndDate =
            local.Ldets.Item.GlegalActionDetail.EndDate;
        }

        if (local.Ldets.Item.GlegalActionDetail.ArrearsAmount.
          GetValueOrDefault() > 0)
        {
          if (export.Group.Item.G.ArrearsFreqAmount.GetValueOrDefault() == 0)
          {
            export.Group.Update.G.ArrearsFreq =
              local.Ldets.Item.GlegalActionDetail.FreqPeriodCode ?? "";
            export.Group.Update.G.ArrearsFreqAmount =
              local.Ldets.Item.GlegalActionDetail.ArrearsAmount.
                GetValueOrDefault();
          }
          else if (Equal(local.Ldets.Item.GlegalActionDetail.FreqPeriodCode,
            export.Group.Item.G.ArrearsFreq))
          {
            export.Group.Update.G.ArrearsFreqAmount =
              export.Group.Item.G.ArrearsFreqAmount.GetValueOrDefault() + local
              .Ldets.Item.GlegalActionDetail.ArrearsAmount.GetValueOrDefault();
          }
          else
          {
            switch(TrimEnd(export.Group.Item.G.ArrearsFreq ?? ""))
            {
              case "BW":
                local.ConvertAmt1.Count = 26;

                break;
              case "SM":
                local.ConvertAmt1.Count = 24;

                break;
              case "W":
                local.ConvertAmt1.Count = 52;

                break;
              default:
                local.ConvertAmt1.Count = 12;

                break;
            }

            local.ConvertAmt1.TotalCurrency =
              export.Group.Item.G.ArrearsFreqAmount.GetValueOrDefault() * local
              .ConvertAmt1.Count;

            switch(TrimEnd(local.Ldets.Item.GlegalActionDetail.
              FreqPeriodCode ?? ""))
            {
              case "BW":
                local.ConvertAmt2.Count = 26;

                break;
              case "SM":
                local.ConvertAmt2.Count = 24;

                break;
              case "W":
                local.ConvertAmt2.Count = 52;

                break;
              default:
                local.ConvertAmt2.Count = 12;

                break;
            }

            local.ConvertAmt2.TotalCurrency =
              local.Ldets.Item.GlegalActionDetail.ArrearsAmount.
                GetValueOrDefault() * local.ConvertAmt2.Count;

            if (local.ConvertAmt1.Count < local.ConvertAmt2.Count)
            {
              export.Group.Update.G.ArrearsFreqAmount =
                Math.Round((
                  local.ConvertAmt1.TotalCurrency + local
                .ConvertAmt2.TotalCurrency) /
                local.ConvertAmt1.Count, 2, MidpointRounding.AwayFromZero);

              switch(local.ConvertAmt1.Count)
              {
                case 26:
                  export.Group.Update.G.ArrearsFreq = "BW";

                  break;
                case 24:
                  export.Group.Update.G.ArrearsFreq = "SM";

                  break;
                case 52:
                  export.Group.Update.G.ArrearsFreq = "W";

                  break;
                default:
                  export.Group.Update.G.ArrearsFreq = "M";

                  break;
              }
            }
            else
            {
              export.Group.Update.G.ArrearsFreqAmount =
                Math.Round((
                  local.ConvertAmt1.TotalCurrency + local
                .ConvertAmt2.TotalCurrency) /
                local.ConvertAmt2.Count, 2, MidpointRounding.AwayFromZero);

              switch(local.ConvertAmt2.Count)
              {
                case 26:
                  export.Group.Update.G.ArrearsFreq = "BW";

                  break;
                case 24:
                  export.Group.Update.G.ArrearsFreq = "SM";

                  break;
                case 52:
                  export.Group.Update.G.ArrearsFreq = "W";

                  break;
                default:
                  export.Group.Update.G.ArrearsFreq = "M";

                  break;
              }
            }
          }
        }
      }

      // ----------------------------------------------------------
      // Determine arrears for that legal action detail (only
      // applicable for Financial legal details)
      // This is common code for a first time order datablock
      // and one we are just adding to
      // ----------------------------------------------------------
      if (AsChar(local.Ldets.Item.GlegalActionDetail.DetailType) == 'F')
      {
        foreach(var item in ReadObligationObligationTypeObligationTransaction())
        {
          if (local.Supported.Count > 0)
          {
            if (ReadCsePerson2())
            {
              for(local.Supported.Index = 0; local.Supported.Index < local
                .Supported.Count; ++local.Supported.Index)
              {
                if (!local.Supported.CheckSize())
                {
                  break;
                }

                if (Equal(local.Supported.Item.GlocalSupported.Number,
                  entities.CsePerson.Number))
                {
                  goto Test2;
                }
              }

              local.Supported.CheckIndex();
            }

            continue;
          }

Test2:

          export.Group.Update.G.ArrearsTotalAmount =
            export.Group.Item.G.ArrearsTotalAmount.GetValueOrDefault() + entities
            .DebtDetail.BalanceDueAmt;

          // ----------------------------------------------------
          // The following code is for determining which program
          // category that any arrears should count towards.
          // ----------------------------------------------------
          if (AsChar(entities.ObligationType.Classification) == 'M' || Equal
            (entities.ObligationType.Code, "MC") || Equal
            (entities.ObligationType.Code, "MS"))
          {
            // ----------------------------------------------------
            // Medical amount, start date and end date
            // ----------------------------------------------------
            export.Group.Update.G.MedicalAmount =
              export.Group.Item.G.MedicalAmount.GetValueOrDefault() + entities
              .DebtDetail.BalanceDueAmt;

            if (AsChar(entities.ObligationType.Classification) == 'A')
            {
              // ----------------------------------------------------
              // Accruing obligations
              // ----------------------------------------------------
              if (ReadObligationPaymentSchedule())
              {
                if (Lt(entities.ObligationPaymentSchedule.StartDt,
                  export.Group.Item.G.MedicalFromDate))
                {
                  export.Group.Update.G.MedicalFromDate =
                    entities.ObligationPaymentSchedule.StartDt;
                }

                if (Lt(export.Group.Item.G.MedicalThruDate,
                  entities.ObligationPaymentSchedule.EndDt))
                {
                  export.Group.Update.G.MedicalThruDate =
                    entities.ObligationPaymentSchedule.EndDt;
                }
              }
            }
            else
            {
              if (Lt(entities.DebtDetail.CoveredPrdStartDt,
                export.Group.Item.G.MedicalFromDate))
              {
                export.Group.Update.G.MedicalFromDate =
                  entities.DebtDetail.CoveredPrdStartDt;
              }

              if (Lt(entities.DebtDetail.CoveredPrdEndDt,
                export.Group.Item.G.MedicalFromDate))
              {
                export.Group.Update.G.MedicalThruDate =
                  entities.DebtDetail.CoveredPrdEndDt;
              }
            }
          }
          else
          {
            // ---------------------------------------------------------
            // Non-accruing obligations may have a preconversion program
            // ---------------------------------------------------------
            if (AsChar(entities.ObligationType.Classification) == 'N' && !
              IsEmpty(entities.DebtDetail.PreconversionProgramCode))
            {
              local.Program.Code =
                entities.DebtDetail.PreconversionProgramCode ?? Spaces(3);
            }
            else
            {
              UseFnDeterminePgmForDebtDetail();
            }

            if (Equal(local.Program.Code, "AF") || Equal
              (local.Program.Code, "AFI"))
            {
              export.Group.Update.G.ArrearsAfdcAmount =
                export.Group.Item.G.ArrearsAfdcAmount.GetValueOrDefault() + entities
                .DebtDetail.BalanceDueAmt;

              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (Equal(export.Group.Item.G.ArrearsAfdcFromDate,
                  local.NullDateWorkArea.Date))
                {
                  export.Group.Update.G.ArrearsAfdcFromDate =
                    entities.DebtDetail.DueDt;
                }
                else if (Lt(entities.DebtDetail.DueDt,
                  export.Group.Item.G.ArrearsAfdcFromDate))
                {
                  export.Group.Update.G.ArrearsAfdcFromDate =
                    entities.DebtDetail.DueDt;
                }
                else
                {
                }

                if (Lt(export.Group.Item.G.ArrearsAfdcThruDate,
                  entities.DebtDetail.DueDt))
                {
                  export.Group.Update.G.ArrearsAfdcThruDate =
                    entities.DebtDetail.DueDt;
                }
              }
              else
              {
                if (Equal(export.Group.Item.G.ArrearsAfdcFromDate,
                  local.NullDateWorkArea.Date))
                {
                  export.Group.Update.G.ArrearsAfdcFromDate =
                    entities.DebtDetail.CoveredPrdStartDt;
                }
                else if (Lt(entities.DebtDetail.CoveredPrdStartDt,
                  export.Group.Item.G.ArrearsAfdcFromDate))
                {
                  export.Group.Update.G.ArrearsAfdcFromDate =
                    entities.DebtDetail.CoveredPrdStartDt;
                }
                else
                {
                }

                if (Lt(export.Group.Item.G.ArrearsAfdcThruDate,
                  entities.DebtDetail.CoveredPrdEndDt))
                {
                  export.Group.Update.G.ArrearsAfdcThruDate =
                    entities.DebtDetail.CoveredPrdEndDt;
                }
              }
            }
            else if (Equal(local.Program.Code, "FC") || Equal
              (local.Program.Code, "FCI") || Equal
              (local.Program.Code, "NC") || Equal(local.Program.Code, "NF"))
            {
              export.Group.Update.G.FosterCareAmount =
                export.Group.Item.G.FosterCareAmount.GetValueOrDefault() + entities
                .DebtDetail.BalanceDueAmt;

              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (Equal(export.Group.Item.G.FosterCareFromDate,
                  local.NullDateWorkArea.Date))
                {
                  export.Group.Update.G.FosterCareFromDate =
                    entities.DebtDetail.DueDt;
                }
                else if (Lt(entities.DebtDetail.DueDt,
                  export.Group.Item.G.FosterCareFromDate))
                {
                  export.Group.Update.G.FosterCareFromDate =
                    entities.DebtDetail.DueDt;
                }
                else
                {
                }

                if (Lt(export.Group.Item.G.FosterCareThruDate,
                  entities.DebtDetail.DueDt))
                {
                  export.Group.Update.G.FosterCareThruDate =
                    entities.DebtDetail.DueDt;
                }
              }
              else
              {
                if (Equal(export.Group.Item.G.FosterCareFromDate,
                  local.NullDateWorkArea.Date))
                {
                  export.Group.Update.G.FosterCareFromDate =
                    entities.DebtDetail.CoveredPrdStartDt;
                }
                else if (Lt(entities.DebtDetail.CoveredPrdStartDt,
                  export.Group.Item.G.FosterCareFromDate))
                {
                  export.Group.Update.G.FosterCareFromDate =
                    entities.DebtDetail.CoveredPrdStartDt;
                }
                else
                {
                }

                if (Lt(export.Group.Item.G.FosterCareThruDate,
                  entities.DebtDetail.CoveredPrdEndDt))
                {
                  export.Group.Update.G.FosterCareThruDate =
                    entities.DebtDetail.CoveredPrdEndDt;
                }
              }
            }
            else
            {
              export.Group.Update.G.ArrearsNonAfdcAmount =
                export.Group.Item.G.ArrearsNonAfdcAmount.GetValueOrDefault() + entities
                .DebtDetail.BalanceDueAmt;

              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (Equal(export.Group.Item.G.ArrearsNonAfdcFromDate,
                  local.NullDateWorkArea.Date))
                {
                  export.Group.Update.G.ArrearsNonAfdcFromDate =
                    entities.DebtDetail.DueDt;
                }
                else if (Lt(entities.DebtDetail.DueDt,
                  export.Group.Item.G.ArrearsNonAfdcFromDate))
                {
                  export.Group.Update.G.ArrearsNonAfdcFromDate =
                    entities.DebtDetail.DueDt;
                }
                else
                {
                }

                if (Lt(export.Group.Item.G.ArrearsNonAfdcThruDate,
                  entities.DebtDetail.DueDt))
                {
                  export.Group.Update.G.ArrearsNonAfdcThruDate =
                    entities.DebtDetail.DueDt;
                }
              }
              else
              {
                if (Equal(export.Group.Item.G.ArrearsNonAfdcFromDate,
                  local.NullDateWorkArea.Date))
                {
                  export.Group.Update.G.ArrearsNonAfdcFromDate =
                    entities.DebtDetail.CoveredPrdStartDt;
                }
                else if (Lt(entities.DebtDetail.CoveredPrdStartDt,
                  export.Group.Item.G.ArrearsNonAfdcFromDate))
                {
                  export.Group.Update.G.ArrearsNonAfdcFromDate =
                    entities.DebtDetail.CoveredPrdStartDt;
                }
                else
                {
                }

                if (Lt(export.Group.Item.G.ArrearsNonAfdcThruDate,
                  entities.DebtDetail.CoveredPrdEndDt))
                {
                  export.Group.Update.G.ArrearsNonAfdcThruDate =
                    entities.DebtDetail.CoveredPrdEndDt;
                }
              }
            }
          }

          if (Lt(export.Group.Item.G.DateOfLastPayment,
            entities.Obligation.LastCollDt))
          {
            export.Group.Update.G.DateOfLastPayment =
              entities.Obligation.LastCollDt;
          }
        }
      }
    }

    local.Ldets.CheckIndex();

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      // ----------------------------------------------------------
      // The Arrears Total Amount must be populated if the Arrears
      // Freq Amount is populated.
      // ----------------------------------------------------------
      if (export.Group.Item.G.ArrearsTotalAmount.GetValueOrDefault() == 0 && export
        .Group.Item.G.ArrearsFreqAmount.GetValueOrDefault() > 0)
      {
        export.Group.Update.G.ArrearsFreqAmount = 0;
        export.Group.Update.G.ArrearsFreq = "";
      }

      // ----------------------------------------------------------
      // Arrears Thru Dates cannot be greater than the last day of
      // last month.
      // This could happen for non-accruing obligations, since we
      // use the covered period end date
      // ----------------------------------------------------------
      if (Lt(local.FirstOfMonth.Date, export.Group.Item.G.ArrearsAfdcThruDate))
      {
        export.Group.Update.G.ArrearsAfdcThruDate = local.FirstOfMonth.Date;
      }

      if (Lt(local.FirstOfMonth.Date, export.Group.Item.G.ArrearsNonAfdcThruDate))
        
      {
        export.Group.Update.G.ArrearsNonAfdcThruDate = local.FirstOfMonth.Date;
      }

      if (Lt(local.FirstOfMonth.Date, export.Group.Item.G.FosterCareThruDate))
      {
        export.Group.Update.G.FosterCareThruDate = local.FirstOfMonth.Date;
      }

      if (Lt(local.FirstOfMonth.Date, export.Group.Item.G.MedicalThruDate))
      {
        export.Group.Update.G.MedicalThruDate = local.FirstOfMonth.Date;
      }
    }

    export.Group.CheckIndex();
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.CsePerson.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.DebtDetail.Assign(entities.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetString(command, "numb", local.PrimaryAp.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
        db.SetNullableString(
          command, "cpaSupType", entities.ObligationTransaction.CpaSupType ?? ""
          );
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadCsePersonLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "cspNumber", local.Supported.Item.GlocalSupported.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 0);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", import.LegalActions.Item.G.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 8);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 9);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetail()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", local.PrimaryAp.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.OrderAuthority = db.GetString(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 14);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 15);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 16);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTypeObligationTransaction()
  {
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationTypeObligationTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", local.Ldets.Item.GlegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", local.Ldets.Item.GlegalAction.Identifier);
        db.SetString(command, "cspNumber", local.PrimaryAp.Number);
        db.
          SetDate(command, "dueDt", local.FirstOfMonth.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.LastCollDt = db.GetNullableDate(reader, 4);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 5);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Classification = db.GetString(reader, 8);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 10);
        entities.DebtDetail.OtrType = db.GetString(reader, 10);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 12);
        entities.DebtDetail.DueDt = db.GetDate(reader, 13);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 14);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 15);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "startDt", local.FirstOfMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 5);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 6);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public LegalAction G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private LegalAction g;
    }

    /// <summary>A ParticipantsGroup group.</summary>
    [Serializable]
    public class ParticipantsGroup
    {
      /// <summary>
      /// A value of GimportParticipant.
      /// </summary>
      [JsonPropertyName("gimportParticipant")]
      public CsePerson GimportParticipant
      {
        get => gimportParticipant ??= new();
        set => gimportParticipant = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateParticipant G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePerson gimportParticipant;
      private InterstateParticipant g;
    }

    /// <summary>
    /// A value of PrimaryAp.
    /// </summary>
    [JsonPropertyName("primaryAp")]
    public CsePersonsWorkSet PrimaryAp
    {
      get => primaryAp ??= new();
      set => primaryAp = value;
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
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
    }

    /// <summary>
    /// Gets a value of Participants.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantsGroup> Participants => participants ??= new(
      ParticipantsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Participants for json serialization.
    /// </summary>
    [JsonPropertyName("participants")]
    [Computed]
    public IList<ParticipantsGroup> Participants_Json
    {
      get => participants;
      set => Participants.Assign(value);
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public InterstateCase Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelImportPrimaryAp.
    /// </summary>
    [JsonPropertyName("zdelImportPrimaryAp")]
    public CsePerson ZdelImportPrimaryAp
    {
      get => zdelImportPrimaryAp ??= new();
      set => zdelImportPrimaryAp = value;
    }

    private CsePersonsWorkSet primaryAp;
    private Case1 case1;
    private Array<LegalActionsGroup> legalActions;
    private Array<ParticipantsGroup> participants;
    private DateWorkArea current;
    private InterstateCase zdel;
    private CsePerson zdelImportPrimaryAp;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
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

    private InterstateCase interstateCase;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SupportedGroup group.</summary>
    [Serializable]
    public class SupportedGroup
    {
      /// <summary>
      /// A value of GlocalSupported.
      /// </summary>
      [JsonPropertyName("glocalSupported")]
      public CsePerson GlocalSupported
      {
        get => glocalSupported ??= new();
        set => glocalSupported = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePerson glocalSupported;
    }

    /// <summary>A LdetsGroup group.</summary>
    [Serializable]
    public class LdetsGroup
    {
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
      /// A value of Gfips.
      /// </summary>
      [JsonPropertyName("gfips")]
      public Fips Gfips
      {
        get => gfips ??= new();
        set => gfips = value;
      }

      /// <summary>
      /// A value of GlegalActionDetail.
      /// </summary>
      [JsonPropertyName("glegalActionDetail")]
      public LegalActionDetail GlegalActionDetail
      {
        get => glegalActionDetail ??= new();
        set => glegalActionDetail = value;
      }

      /// <summary>
      /// A value of GobligationType.
      /// </summary>
      [JsonPropertyName("gobligationType")]
      public ObligationType GobligationType
      {
        get => gobligationType ??= new();
        set => gobligationType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction glegalAction;
      private Fips gfips;
      private LegalActionDetail glegalActionDetail;
      private ObligationType gobligationType;
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
    /// A value of PrimaryAp.
    /// </summary>
    [JsonPropertyName("primaryAp")]
    public CsePersonsWorkSet PrimaryAp
    {
      get => primaryAp ??= new();
      set => primaryAp = value;
    }

    /// <summary>
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public DateWorkArea FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
    }

    /// <summary>
    /// A value of ConvertAmt2.
    /// </summary>
    [JsonPropertyName("convertAmt2")]
    public Common ConvertAmt2
    {
      get => convertAmt2 ??= new();
      set => convertAmt2 = value;
    }

    /// <summary>
    /// A value of ConvertAmt1.
    /// </summary>
    [JsonPropertyName("convertAmt1")]
    public Common ConvertAmt1
    {
      get => convertAmt1 ??= new();
      set => convertAmt1 = value;
    }

    /// <summary>
    /// Gets a value of Supported.
    /// </summary>
    [JsonIgnore]
    public Array<SupportedGroup> Supported => supported ??= new(
      SupportedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Supported for json serialization.
    /// </summary>
    [JsonPropertyName("supported")]
    [Computed]
    public IList<SupportedGroup> Supported_Json
    {
      get => supported;
      set => Supported.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ldets.
    /// </summary>
    [JsonIgnore]
    public Array<LdetsGroup> Ldets => ldets ??= new(LdetsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ldets for json serialization.
    /// </summary>
    [JsonPropertyName("ldets")]
    [Computed]
    public IList<LdetsGroup> Ldets_Json
    {
      get => ldets;
      set => Ldets.Assign(value);
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateSupportOrder New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of NullInterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("nullInterstateSupportOrder")]
    public InterstateSupportOrder NullInterstateSupportOrder
    {
      get => nullInterstateSupportOrder ??= new();
      set => nullInterstateSupportOrder = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of IncludeLdet.
    /// </summary>
    [JsonPropertyName("includeLdet")]
    public Common IncludeLdet
    {
      get => includeLdet ??= new();
      set => includeLdet = value;
    }

    private LegalActionDetail legalActionDetail;
    private CsePersonsWorkSet primaryAp;
    private DateWorkArea firstOfMonth;
    private Common convertAmt2;
    private Common convertAmt1;
    private Array<SupportedGroup> supported;
    private Array<LdetsGroup> ldets;
    private InterstateSupportOrder new1;
    private InterstateSupportOrder nullInterstateSupportOrder;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea current;
    private Program program;
    private Common includeLdet;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of ZdelJClass.
    /// </summary>
    [JsonPropertyName("zdelJClass")]
    public LegalAction ZdelJClass
    {
      get => zdelJClass ??= new();
      set => zdelJClass = value;
    }

    /// <summary>
    /// A value of ZdelCsePerson.
    /// </summary>
    [JsonPropertyName("zdelCsePerson")]
    public CsePerson ZdelCsePerson
    {
      get => zdelCsePerson ??= new();
      set => zdelCsePerson = value;
    }

    /// <summary>
    /// A value of ZdelCaseRole.
    /// </summary>
    [JsonPropertyName("zdelCaseRole")]
    public CaseRole ZdelCaseRole
    {
      get => zdelCaseRole ??= new();
      set => zdelCaseRole = value;
    }

    /// <summary>
    /// A value of ZdelCase.
    /// </summary>
    [JsonPropertyName("zdelCase")]
    public Case1 ZdelCase
    {
      get => zdelCase ??= new();
      set => zdelCase = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction obligationTransaction;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private Fips fips;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private LegalAction zdelJClass;
    private CsePerson zdelCsePerson;
    private CaseRole zdelCaseRole;
    private Case1 zdelCase;
  }
#endregion
}
