// Program: SP_PRINT_DATA_RETRIEVAL_AAPPEAL, ID: 372132897, model: 746.
// Short name: SWE02233
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_AAPPEAL.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalAappeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_AAPPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalAappeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalAappeal.
  /// </summary>
  public SpPrintDataRetrievalAappeal(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------
    // 11/13/1998	M Ramirez			Initial Development
    // 05/22/1999	M Ramirez			Pulled common code out of individual fields
    // 05/22/1999	M Ramirez			Changed method for finding legal actions 
    // associated to the administrative_appeal
    // 07/14/1999	M Ramirez			Added the row lock counts
    // 06/20/2001	M Ramirez	118241		Changed the way to calculate the amount
    // ----------------------------------------------------------------------
    local.LiteralMinPeriodsPerYear.Count = 999;
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    export.SpDocKey.Assign(import.SpDocKey);

    // mjr
    // ---------------------------------------------------
    // Extract admin_appeal number from next_tran_info
    // ------------------------------------------------------
    if (!ReadAdministrativeAppeal())
    {
      // mjr---> Admin Appeal not found, but no message is given.
      return;
    }

    foreach(var item in ReadField())
    {
      // mjr--->  For Fields processed in this CAB
      if (Lt(entities.Field.SubroutineName, local.PreviousField.SubroutineName) ||
        Equal
        (entities.Field.SubroutineName, local.PreviousField.SubroutineName) && !
        Lt(local.PreviousField.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.ProcessGroup.Flag = "N";
      MoveField(entities.Field, local.PreviousField);

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case "AAPPEAL":
          // mjr
          // -------------------------------------------------------
          // 05/22/1999
          // Pulled common code out of individual fields
          // Changed method for finding legal actions associated to the
          // administrative_appeal
          // --------------------------------------------------------------------
          if (Equal(entities.Field.Name, "AAPPMAXLA") || Equal
            (entities.Field.Name, "AAPP1CTORD") || Equal
            (entities.Field.Name, "AAPP1FILDT") || Equal
            (entities.Field.Name, "AAPP1PMTAM") || Equal
            (entities.Field.Name, "AAPP1PMMSG") || Equal
            (entities.Field.Name, "AAPP1PMTFQ") || Equal
            (entities.Field.Name, "AAPP1PMTLO") || Equal
            (entities.Field.Name, "AAPP1TRBCO"))
          {
            if (local.Local1.Count <= 0)
            {
              local.MaxLegalActions.Flag = "N";

              // mjr
              // ----------------------------------------------
              // 06/20/2001
              // PR# 118241 - Change calculation for amount
              // Added qualification for 'J' Class legal actions
              // -----------------------------------------------------------
              local.Count.Count = 0;
              local.Local1.Index = -1;

              foreach(var item1 in ReadLegalAction())
              {
                if (local.Local1.Index >= 0)
                {
                  if (Lt(local.Local1.Item.GlegalAction.CourtCaseNumber,
                    entities.LegalAction.CourtCaseNumber))
                  {
                    ++local.Count.Count;
                  }
                }
                else
                {
                  ++local.Count.Count;
                }

                if (local.Local1.Index + 1 >= Local.LocalGroup.Capacity || local
                  .Count.Count > 5)
                {
                  local.MaxLegalActions.Flag = "Y";

                  break;
                }

                ++local.Local1.Index;
                local.Local1.CheckSize();

                local.Local1.Update.GlegalAction.Assign(entities.LegalAction);
              }
            }

            if (Equal(entities.Field.Name, "AAPP1PMTAM") || Equal
              (entities.Field.Name, "AAPP1PMMSG") || Equal
              (entities.Field.Name, "AAPP1PMTFQ") || Equal
              (entities.Field.Name, "AAPP1PMTLO"))
            {
              if (IsEmpty(local.FoundLegalDetails.Flag))
              {
                local.FoundLegalDetails.Flag = "Y";

                for(local.Local1.Index = 0; local.Local1.Index < local
                  .Local1.Count; ++local.Local1.Index)
                {
                  if (!local.Local1.CheckSize())
                  {
                    break;
                  }

                  // mjr
                  // ----------------------------------------------
                  // 06/20/2001
                  // PR# 118241 - Change calculation for amount
                  // Added arrears amount
                  // -----------------------------------------------------------
                  local.Local1.Item.Subordinate.Index = -1;

                  foreach(var item1 in ReadLegalActionDetail())
                  {
                    if (Lt(0, entities.LegalActionDetail.CurrentAmount))
                    {
                    }
                    else if (Lt(0, entities.LegalActionDetail.ArrearsAmount))
                    {
                    }
                    else
                    {
                      continue;
                    }

                    ++local.Local1.Item.Subordinate.Index;
                    local.Local1.Item.Subordinate.CheckSize();

                    local.Local1.Update.Subordinate.Update.GlocalSub.Assign(
                      entities.LegalActionDetail);

                    if (local.Local1.Item.Subordinate.Index + 1 >= Local
                      .SubordinateGroup.Capacity)
                    {
                      goto Next;
                    }
                  }

Next:
                  ;
                }

                local.Local1.CheckIndex();
              }
            }
          }

          // mjr
          // ------------------------------------------------------
          // 05/24/1999
          // Determine fields generic name.
          // The generic name is the field name for all fields except fields
          // which are for the group of legal action information.  For these
          // fields, the generic name is AAPP*xxxxx, where * is the group
          // number of the field, and xxxxx is the last five characters of the
          // field name.
          // -------------------------------------------------------------------
          local.CurrentRoot.Name = Substring(entities.Field.Name, 5, 1);

          if (Lt("1", local.CurrentRoot.Name) && !
            Lt("5", local.CurrentRoot.Name))
          {
            // mjr--->  Don't include 1, because 1 has special processing.
            local.Current.Name = "AAPP*" + Substring
              (entities.Field.Name, Field.Name_MaxLength, 6, 5);
          }
          else
          {
            local.Current.Name = entities.Field.Name;
          }

          switch(TrimEnd(local.Current.Name))
          {
            case "AAPPACTTYP":
              if (ReadAdministrativeAction())
              {
                local.FieldValue.Value =
                  entities.AdministrativeAction.Description;
              }

              break;
            case "AAPPMAXLA":
              if (AsChar(local.MaxLegalActions.Flag) == 'Y')
              {
                local.FieldValue.Value =
                  "More Court Order Numbers exist than can be shown here.";
              }

              break;
            case "AAPPNUMBER":
              local.FieldValue.Value = entities.AdministrativeAppeal.Number;

              break;
            case "AAPPRSN01":
              local.ProcessGroup.Flag = "Y";

              // mjr
              // ------------------------------------------
              // 03/20/1999
              // Reason is a 240 character field.  Split it into 5
              // fifty-eight character fields.
              // Don't leave a space at the end of a field.  (WP will
              // truncate trailing spaces.)
              // -------------------------------------------------------
              local.FieldValue.Value = entities.AdministrativeAppeal.Reason;
              local.Local1.Index = -1;
              local.Count.Count = Length(TrimEnd(local.FieldValue.Value));

              while(local.Count.Count > 0)
              {
                if (local.Count.Count <= 58)
                {
                  local.Format.Value = local.FieldValue.Value ?? "";
                  local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                }
                else
                {
                  local.Format.Value = Substring(local.FieldValue.Value, 1, 58);
                  local.Count.Count = Length(TrimEnd(local.Format.Value));
                  local.FieldValue.Value =
                    Substring(local.FieldValue.Value, local.Count.Count +
                    1, Length(TrimEnd(local.FieldValue.Value)) -
                    local.Count.Count);
                }

                ++local.Local1.Index;
                local.Local1.CheckSize();

                local.Local1.Update.GfieldValue.Value = local.Format.Value ?? ""
                  ;
                local.Count.Count = Length(TrimEnd(local.FieldValue.Value));
              }

              break;
            case "AAPPRELATE":
              local.FieldValue.Value =
                entities.AdministrativeAppeal.AppellantRelationship;

              break;
            case "AAPPSTMT01":
              local.ProcessGroup.Flag = "Y";
              local.Local1.Index = -1;

              foreach(var item1 in ReadPositionStatement())
              {
                ++local.Local1.Index;
                local.Local1.CheckSize();

                local.Local1.Update.GfieldValue.Value =
                  entities.PositionStatement.Explanation;

                if (local.Local1.Index >= 14)
                {
                  break;
                }
              }

              break;
            case "AAPP1CTORD":
              local.Count.Count = 0;
              local.PreviousLegalAction.CourtCaseNumber = "";
              local.Local1.Index = 0;

              for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
                local.Local1.Index)
              {
                if (!local.Local1.CheckSize())
                {
                  break;
                }

                if (Lt(local.PreviousLegalAction.CourtCaseNumber,
                  local.Local1.Item.GlegalAction.CourtCaseNumber))
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);
                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  local.Local1.Update.GlocalCourtCaseNo.Value =
                    local.PreviousLegalAction.CourtCaseNumber ?? "";

                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();
                }
              }

              local.Local1.CheckIndex();

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalCourtCaseNo.Value ?? "";

              break;
            case "AAPP1FILDT":
              local.Count.Count = 0;
              local.PreviousLegalAction.CourtCaseNumber = "";
              local.Local1.Index = 0;

              for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
                local.Local1.Index)
              {
                if (!local.Local1.CheckSize())
                {
                  break;
                }

                if (!Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber))
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  // mjr
                  // ----------------------------------------------
                  // This file date must be put in the group_local in the
                  // position (local_count).  Then processing needs to
                  // continue from the original subscript.
                  // -------------------------------------------------
                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);
                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  if (Lt(local.Null1.Date, local.PreviousLegalAction.FiledDate))
                  {
                    local.DateWorkArea.Date =
                      local.PreviousLegalAction.FiledDate;
                    local.Local1.Update.GlocalFiledDate.Value =
                      UseSpDocFormatDate();
                  }
                  else
                  {
                    local.Local1.Update.GlocalFiledDate.Value =
                      Spaces(FieldValue.Value_MaxLength);
                  }

                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();
                }
              }

              local.Local1.CheckIndex();

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalFiledDate.Value ?? "";

              break;
            case "AAPP1PMTAM":
              local.Count.Count = 0;
              local.CourtCaseTotal.TotalCurrency = 0;
              local.PeriodsPerYear.Count = 0;
              local.MinPeriodsPerYear.Count =
                local.LiteralMinPeriodsPerYear.Count;

              // mjr--> Initiialize prev cc_no to the first in the group.
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.PreviousLegalAction.Assign(local.Local1.Item.GlegalAction);
              local.ExitLoop.Flag = "N";

              do
              {
                do
                {
                  for(local.Local1.Item.Subordinate.Index = 0; local
                    .Local1.Item.Subordinate.Index < local
                    .Local1.Item.Subordinate.Count; ++
                    local.Local1.Item.Subordinate.Index)
                  {
                    if (!local.Local1.Item.Subordinate.CheckSize())
                    {
                      break;
                    }

                    switch(TrimEnd(local.Local1.Item.Subordinate.Item.GlocalSub.
                      FreqPeriodCode ?? ""))
                    {
                      case "W":
                        local.PeriodsPerYear.Count = 52;

                        break;
                      case "SM":
                        local.PeriodsPerYear.Count = 24;

                        break;
                      case "M":
                        local.PeriodsPerYear.Count = 12;

                        break;
                      case "BW":
                        local.PeriodsPerYear.Count = 26;

                        break;
                      default:
                        export.ErrorDocumentField.ScreenPrompt =
                          "Processing Error";
                        export.ErrorFieldValue.Value = "Field:  " + entities
                          .Field.Name + ",  Freq Code:  " + (
                            local.Local1.Item.Subordinate.Item.GlocalSub.
                            FreqPeriodCode ?? "");

                        return;
                    }

                    // mjr
                    // ----------------------------------------------
                    // 06/20/2001
                    // PR# 118241 - Change calculation for amount
                    // Added arrears amount
                    // -----------------------------------------------------------
                    if (local.Local1.Item.Subordinate.Item.GlocalSub.
                      CurrentAmount.GetValueOrDefault() > 0)
                    {
                      local.CourtCaseTotal.TotalCurrency += local.Local1.Item.
                        Subordinate.Item.GlocalSub.CurrentAmount.
                          GetValueOrDefault() * local.PeriodsPerYear.Count;
                    }
                    else
                    {
                      local.CourtCaseTotal.TotalCurrency += local.Local1.Item.
                        Subordinate.Item.GlocalSub.ArrearsAmount.
                          GetValueOrDefault() * local.PeriodsPerYear.Count;
                    }

                    if (local.PeriodsPerYear.Count < local
                      .MinPeriodsPerYear.Count)
                    {
                      local.MinPeriodsPerYear.Count =
                        local.PeriodsPerYear.Count;
                    }
                  }

                  local.Local1.Item.Subordinate.CheckIndex();

                  if (local.Local1.Index + 1 >= local.Local1.Count)
                  {
                    local.ExitLoop.Flag = "Y";
                    local.PreviousLegalAction.CourtCaseNumber = "";

                    break;
                  }

                  ++local.Local1.Index;
                  local.Local1.CheckSize();
                }
                while(Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber));

                if (!Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber))
                {
                  // mjr
                  // --------------------------------------------------------
                  // This is a new court case number.
                  // Prepare sum for g_local, reset sum (including the current
                  // value) for the next g_local.
                  // -----------------------------------------------------------
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  // mjr
                  // ----------------------------------------------
                  // Summed this Court Case Number's LDET's.
                  // The LDET's are summed to annual payments.  Now
                  // adjust amount to the lowest frequency (using
                  // local_min_periods_per_year).
                  // ex:  if the LDET's have Weekly and Monthly, sum
                  // the periodic payments to Monthly.  With all amounts
                  // summed as annual payments, the amount must be
                  // divided by the number of periods of that frequency
                  // in a year.  For Monthly, there are 12 periods, for
                  // Weekly there are 52.
                  // -------------------------------------------------
                  local.CourtCaseTotal.TotalCurrency =
                    Math.Round(
                      local.CourtCaseTotal.TotalCurrency /
                    local.MinPeriodsPerYear.Count, 2,
                    MidpointRounding.AwayFromZero);
                  local.CourtCaseTotal.TotalInteger =
                    (long)(local.CourtCaseTotal.TotalCurrency * 100);
                  local.Format.Value =
                    NumberToString(local.CourtCaseTotal.TotalInteger,
                    Verify(
                      NumberToString(local.CourtCaseTotal.TotalInteger, 15),
                    "0"), 16 -
                    Verify(
                      NumberToString(local.CourtCaseTotal.TotalInteger, 15),
                    "0"));
                  local.FormatPaymentAmtDollars.Value =
                    Substring(TrimEnd(local.Format.Value), 1,
                    Length(TrimEnd(local.Format.Value)) - 2);
                  local.FormatPaymentAmtCents.Value =
                    Substring(TrimEnd(local.Format.Value),
                    Length(TrimEnd(local.Format.Value)) - 1, 2);
                  local.Local1.Update.GlocalPaymentAmount.Value =
                    TrimEnd(local.FormatPaymentAmtDollars.Value) + "." + TrimEnd
                    (local.FormatPaymentAmtCents.Value);

                  // mjr
                  // ----------------------------------------------
                  // Repeat processing for next Legal action.
                  // -------------------------------------------------
                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();

                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);
                  local.CourtCaseTotal.TotalCurrency = 0;
                  local.MinPeriodsPerYear.Count =
                    local.LiteralMinPeriodsPerYear.Count;
                }
              }
              while(AsChar(local.ExitLoop.Flag) != 'Y');

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentAmount.Value ?? "";

              break;
            case "AAPP1PMMSG":
              local.Count.Count = 0;
              local.MinPeriodsPerYear.Count =
                local.LiteralMinPeriodsPerYear.Count;
              local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);

              // mjr--> Initiialize prev cc_no to the first in the group.
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.PreviousLegalAction.Assign(local.Local1.Item.GlegalAction);
              local.ExitLoop.Flag = "N";

              do
              {
                do
                {
                  for(local.Local1.Item.Subordinate.Index = 0; local
                    .Local1.Item.Subordinate.Index < local
                    .Local1.Item.Subordinate.Count; ++
                    local.Local1.Item.Subordinate.Index)
                  {
                    if (!local.Local1.Item.Subordinate.CheckSize())
                    {
                      break;
                    }

                    switch(TrimEnd(local.Local1.Item.Subordinate.Item.GlocalSub.
                      FreqPeriodCode ?? ""))
                    {
                      case "W":
                        local.PeriodsPerYear.Count = 52;

                        break;
                      case "SM":
                        local.PeriodsPerYear.Count = 24;

                        break;
                      case "M":
                        local.PeriodsPerYear.Count = 12;

                        break;
                      case "BW":
                        local.PeriodsPerYear.Count = 26;

                        break;
                      default:
                        export.ErrorDocumentField.ScreenPrompt =
                          "Processing Error";
                        export.ErrorFieldValue.Value = "Field:  " + entities
                          .Field.Name + ",  Freq Code:  " + (
                            local.Local1.Item.Subordinate.Item.GlocalSub.
                            FreqPeriodCode ?? "");

                        return;
                    }

                    if (local.MinPeriodsPerYear.Count == local
                      .LiteralMinPeriodsPerYear.Count)
                    {
                      local.MinPeriodsPerYear.Count =
                        local.PeriodsPerYear.Count;
                    }

                    if (local.PeriodsPerYear.Count != local
                      .MinPeriodsPerYear.Count)
                    {
                      // mjr
                      // ---------------------------------------------
                      // Indicate that this Legal Action has LDET's with
                      // more than one type of frequency code.
                      // ------------------------------------------------
                      local.FieldValue.Value = "*";

                      break;
                    }
                  }

                  local.Local1.Item.Subordinate.CheckIndex();

                  if (local.Local1.Index + 1 >= local.Local1.Count)
                  {
                    local.ExitLoop.Flag = "Y";
                    local.PreviousLegalAction.CourtCaseNumber = "";

                    break;
                  }

                  ++local.Local1.Index;
                  local.Local1.CheckSize();
                }
                while(Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber));

                if (!Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber))
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  local.Local1.Update.GlocalPaymentMessage.Value =
                    local.FieldValue.Value ?? "";

                  // mjr
                  // ----------------------------------------------
                  // Repeat processing for next Legal action.
                  // -------------------------------------------------
                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();

                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);
                  local.MinPeriodsPerYear.Count =
                    local.LiteralMinPeriodsPerYear.Count;
                  local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                }
              }
              while(AsChar(local.ExitLoop.Flag) != 'Y');

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentMessage.Value ?? "";

              break;
            case "AAPP1PMTFQ":
              local.Count.Count = 0;
              local.PeriodsPerYear.Count = 0;
              local.MinPeriodsPerYear.Count =
                local.LiteralMinPeriodsPerYear.Count;

              // mjr--> Initiialize prev cc_no to the first in the group.
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.PreviousLegalAction.Assign(local.Local1.Item.GlegalAction);
              local.ExitLoop.Flag = "N";

              do
              {
                do
                {
                  for(local.Local1.Item.Subordinate.Index = 0; local
                    .Local1.Item.Subordinate.Index < local
                    .Local1.Item.Subordinate.Count; ++
                    local.Local1.Item.Subordinate.Index)
                  {
                    if (!local.Local1.Item.Subordinate.CheckSize())
                    {
                      break;
                    }

                    switch(TrimEnd(local.Local1.Item.Subordinate.Item.GlocalSub.
                      FreqPeriodCode ?? ""))
                    {
                      case "W":
                        local.PeriodsPerYear.Count = 52;

                        break;
                      case "SM":
                        local.PeriodsPerYear.Count = 24;

                        break;
                      case "M":
                        local.PeriodsPerYear.Count = 12;

                        break;
                      case "BW":
                        local.PeriodsPerYear.Count = 26;

                        break;
                      default:
                        export.ErrorDocumentField.ScreenPrompt =
                          "Processing Error";
                        export.ErrorFieldValue.Value = "Field:  " + entities
                          .Field.Name + ",  Freq Code:  " + (
                            local.Local1.Item.Subordinate.Item.GlocalSub.
                            FreqPeriodCode ?? "");

                        return;
                    }

                    // mjr
                    // ---------------------------------------
                    // 06/21/2001
                    // PR# 118241 - Changed calculation for amount
                    // Detected coding error during testing
                    // ----------------------------------------------------
                    if (local.PeriodsPerYear.Count < local
                      .MinPeriodsPerYear.Count)
                    {
                      local.MinPeriodsPerYear.Count =
                        local.PeriodsPerYear.Count;
                    }
                  }

                  local.Local1.Item.Subordinate.CheckIndex();

                  if (local.Local1.Index + 1 >= local.Local1.Count)
                  {
                    local.ExitLoop.Flag = "Y";
                    local.PreviousLegalAction.CourtCaseNumber = "";

                    break;
                  }

                  ++local.Local1.Index;
                  local.Local1.CheckSize();
                }
                while(Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber));

                if (!Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber))
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  // mjr
                  // ---------------------------------------------
                  // This Legal Action's LDETs are summarized to the
                  // local_minimum frequency code.
                  // ------------------------------------------------
                  switch(local.MinPeriodsPerYear.Count)
                  {
                    case 12:
                      local.CodeValue.Cdvalue = "M";

                      break;
                    case 24:
                      local.CodeValue.Cdvalue = "SM";

                      break;
                    case 26:
                      local.CodeValue.Cdvalue = "BW";

                      break;
                    case 52:
                      local.CodeValue.Cdvalue = "W";

                      break;
                    default:
                      break;
                  }

                  local.Code.CodeName = "FREQUENCY";
                  UseCabGetCodeValueDescription();
                  local.Local1.Update.GlocalPaymentFrequency.Value =
                    local.CodeValue.Description;

                  // mjr
                  // ----------------------------------------------
                  // Repeat processing for next Legal action.
                  // -------------------------------------------------
                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();

                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);
                  local.MinPeriodsPerYear.Count =
                    local.LiteralMinPeriodsPerYear.Count;
                }
              }
              while(AsChar(local.ExitLoop.Flag) != 'Y');

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentFrequency.Value ?? "";

              break;
            case "AAPP1PMTLO":
              local.Count.Count = 0;
              local.PreviousLegalAction.CourtCaseNumber = "";
              local.Local1.Index = 0;

              for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
                local.Local1.Index)
              {
                if (!local.Local1.CheckSize())
                {
                  break;
                }

                if (!Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber))
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);

                  if (!IsEmpty(local.Local1.Item.GlegalAction.PaymentLocation))
                  {
                    local.Code.CodeName = "LEGAL ACTION PAYMENT LOCATION";
                    local.CodeValue.Cdvalue =
                      local.Local1.Item.GlegalAction.PaymentLocation ?? Spaces
                      (10);
                    UseCabGetCodeValueDescription();
                  }
                  else if (ReadTribunal())
                  {
                    local.CodeValue.Description = entities.Tribunal.Name;
                  }
                  else
                  {
                    local.CodeValue.Description =
                      Spaces(CodeValue.Description_MaxLength);
                  }

                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  local.Local1.Update.GlocalPaymentLocation.Value =
                    local.CodeValue.Description;

                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();
                }
              }

              local.Local1.CheckIndex();

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentLocation.Value ?? "";

              break;
            case "AAPP1TRBCO":
              local.Count.Count = 0;
              local.PreviousLegalAction.CourtCaseNumber = "";
              local.Local1.Index = 0;

              for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
                local.Local1.Index)
              {
                if (!local.Local1.CheckSize())
                {
                  break;
                }

                if (!Equal(local.Local1.Item.GlegalAction.CourtCaseNumber,
                  local.PreviousLegalAction.CourtCaseNumber))
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 5)
                  {
                    break;
                  }

                  local.PreviousLegalAction.Assign(
                    local.Local1.Item.GlegalAction);

                  if (ReadFipsTribAddress())
                  {
                    if (IsEmpty(entities.FipsTribAddress.County) && IsEmpty
                      (entities.FipsTribAddress.Country))
                    {
                      continue;
                    }

                    if (!IsEmpty(entities.FipsTribAddress.County))
                    {
                      local.Code.CodeName = "COUNTY CODE";
                      local.CodeValue.Cdvalue =
                        entities.FipsTribAddress.County ?? Spaces(10);
                    }
                    else
                    {
                      local.Code.CodeName = "COUNTRY CODE";
                      local.CodeValue.Cdvalue =
                        entities.FipsTribAddress.Country ?? Spaces(10);
                    }

                    UseCabGetCodeValueDescription();
                  }
                  else
                  {
                    local.CodeValue.Description =
                      Spaces(CodeValue.Description_MaxLength);
                  }

                  local.PreviousSubscript.Subscript = local.Local1.Index + 1;

                  local.Local1.Index = local.Count.Count - 1;
                  local.Local1.CheckSize();

                  local.Local1.Update.GlocalTribunal.Value =
                    local.CodeValue.Description;

                  local.Local1.Index = local.PreviousSubscript.Subscript - 1;
                  local.Local1.CheckSize();
                }
              }

              local.Local1.CheckIndex();

              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index = 0;
              local.Local1.CheckSize();

              local.FieldValue.Value =
                local.Local1.Item.GlocalTribunal.Value ?? "";

              break;
            case "AAPP*CTORD":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalCourtCaseNo.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalCourtCaseNo.Value ?? "";

              break;
            case "AAPP*FILDT":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalFiledDate.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalFiledDate.Value ?? "";

              break;
            case "AAPP*PMTAM":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalPaymentAmount.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentAmount.Value ?? "";

              break;
            case "AAPP*PMMSG":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalPaymentMessage.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentMessage.Value ?? "";

              break;
            case "AAPP*PMTFQ":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalPaymentFrequency.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentFrequency.Value ?? "";

              break;
            case "AAPP*PMTLO":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalPaymentLocation.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalPaymentLocation.Value ?? "";

              break;
            case "AAPP*TRBCO":
              // mjr
              // ------------------------------------------------------
              // 05/24/1999
              // The data has already been set in g_local field_value.
              // Move the appropriate occurence to local field_value
              // -------------------------------------------------------------------
              local.Local1.Index =
                (int)(StringToNumber(local.CurrentRoot.Name) - 1);
              local.Local1.CheckSize();

              if (IsEmpty(local.Local1.Item.GlocalTribunal.Value))
              {
                continue;
              }

              local.FieldValue.Value =
                local.Local1.Item.GlocalTribunal.Value ?? "";

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "IMHOUSE":
          if (Equal(entities.Field.Name, "IMHHMBRNM0"))
          {
            local.ProcessGroup.Flag = "Y";

            if (IsEmpty(local.Case1.Number))
            {
              local.Count.Count = 0;

              foreach(var item1 in ReadCase())
              {
                ++local.Count.Count;

                if (local.Count.Count > 1)
                {
                  goto Test;
                }

                local.Case1.Number = entities.Case1.Number;
              }

              if (local.Count.Count < 1)
              {
                break;
              }

              export.SpDocKey.KeyCase = local.Case1.Number;
            }

            if (!ReadCsePerson1())
            {
              break;
            }

            if (!ReadImHouseholdMemberImHousehold())
            {
              break;
            }

            local.Local1.Index = -1;
            local.Local1.Count = 0;

            foreach(var item1 in ReadImHouseholdMember())
            {
              ++local.Local1.Index;
              local.Local1.CheckSize();

              if (local.Local1.Index >= 10)
              {
                break;
              }

              if (ReadCsePerson2())
              {
                local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
              }
              else
              {
                continue;
              }

              if (AsChar(import.Batch.Flag) == 'Y')
              {
                UseSiReadCsePersonBatch();

                if (!IsEmpty(local.AbendData.Type1) && Equal
                  (local.AbendData.AdabasResponseCd, "0148"))
                {
                  export.ErrorDocumentField.ScreenPrompt = "Resource Error";
                  export.ErrorFieldValue.Value = "ADABAS Unavailable";

                  break;
                }
              }
              else
              {
                UseSiReadCsePerson();
              }

              if (!IsEmpty(local.AbendData.Type1) || !
                IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ALL_OK";

                continue;
              }

              local.SpPrintWorkSet.FirstName =
                local.CsePersonsWorkSet.FirstName;
              local.SpPrintWorkSet.MidInitial =
                local.CsePersonsWorkSet.MiddleInitial;
              local.SpPrintWorkSet.LastName = local.CsePersonsWorkSet.LastName;
              local.Local1.Update.GfieldValue.Value = UseSpDocFormatName();
            }
          }
          else
          {
            export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
            export.ErrorFieldValue.Value = "Field:  " + TrimEnd
              (entities.Field.Name) + ",  Subroutine:  " + entities
              .Field.SubroutineName;
          }

          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Subroutine";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;
          ExitState = "FIELD_NF";

          break;
      }

Test:

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      switch(AsChar(local.ProcessGroup.Flag))
      {
        case 'A':
          // mjr
          // ----------------------------------------------
          // Field is an address
          //    Process 1-5 of group_local_address
          // -------------------------------------------------
          local.Position.Count = Length(TrimEnd(entities.Field.Name));
          local.CurrentRoot.Name =
            Substring(entities.Field.Name, 1, local.Position.Count - 1);

          for(local.Address.Index = 0; local.Address.Index < local
            .Address.Count; ++local.Address.Index)
          {
            if (!local.Address.CheckSize())
            {
              break;
            }

            // mjr---> Increment Field Name
            local.Temp.Name = NumberToString(local.Address.Index + 1, 10);
            local.Position.Count = Verify(local.Temp.Name, "0");
            local.Temp.Name =
              Substring(local.Temp.Name, local.Position.Count, 16 -
              local.Position.Count);
            local.Current.Name = TrimEnd(local.CurrentRoot.Name) + local
              .Temp.Name;
            UseSpCabCreateUpdateFieldValue3();

            if (IsExitState("DOCUMENT_FIELD_NF_RB"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.ErrorDocumentField.ScreenPrompt = "Creation Error";
              export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

              return;
            }

            local.Address.Update.GlocalAddress.Value =
              Spaces(FieldValue.Value_MaxLength);
            ++import.ExpImpRowLockFieldValue.Count;
          }

          local.Address.CheckIndex();

          break;
        case 'Y':
          // mjr
          // ----------------------------------------------
          // Field is a group
          //    Process 01-15 of group_local
          // -------------------------------------------------
          // mjr
          // -------------------------------------------------------------------------
          // There are three possible types of groups:
          //     AAPPSTMT01,
          //     AAPPRSN01 , and
          //     AAPP1xxxxx.
          // The first two have the incremented number at the end of the field 
          // name.  The
          // last has the incremented number in the middle of the field name.
          // -----------------------------------------------------------------------------
          if (Equal(entities.Field.Name, "AAPPRSN01") || Equal
            (entities.Field.Name, "AAPPSTMT01"))
          {
            local.Position.Count = Length(TrimEnd(entities.Field.Name));
            local.CurrentRoot.Name =
              Substring(entities.Field.Name, 1, local.Position.Count - 2);

            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              // mjr---> Increment Field Name
              local.Temp.Name = NumberToString(local.Local1.Index + 1, 10);
              local.Position.Count = Verify(local.Temp.Name, "0");

              if (local.Local1.Index >= 9)
              {
                local.Temp.Name =
                  Substring(local.Temp.Name, local.Position.Count, 16 -
                  local.Position.Count);
              }
              else
              {
                local.Temp.Name =
                  Substring(local.Temp.Name, local.Position.Count - 1, 16 -
                  (local.Position.Count - 1));
              }

              local.Current.Name = TrimEnd(local.CurrentRoot.Name) + local
                .Temp.Name;
              UseSpCabCreateUpdateFieldValue2();

              if (IsExitState("DOCUMENT_FIELD_NF_RB"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

                return;
              }

              local.Local1.Update.GfieldValue.Value =
                Spaces(FieldValue.Value_MaxLength);
              ++import.ExpImpRowLockFieldValue.Count;
            }

            local.Local1.CheckIndex();
          }
          else
          {
            local.CurrentRoot.Name = Substring(entities.Field.Name, 6, 5);

            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              // mjr---> Increment Field Name
              local.Temp.Name = NumberToString(local.Local1.Index + 1, 10);
              local.Position.Count = Verify(local.Temp.Name, "0");
              local.Temp.Name =
                Substring(local.Temp.Name, local.Position.Count, 16 -
                local.Position.Count);
              local.Current.Name = "AAPP" + TrimEnd(local.Temp.Name) + local
                .CurrentRoot.Name;
              UseSpCabCreateUpdateFieldValue2();

              if (IsExitState("DOCUMENT_FIELD_NF_RB"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

                return;
              }

              local.Local1.Update.GfieldValue.Value =
                Spaces(FieldValue.Value_MaxLength);
              ++import.ExpImpRowLockFieldValue.Count;
            }

            local.Local1.CheckIndex();
          }

          break;
        default:
          // mjr
          // ----------------------------------------------
          // Field is a single value
          //    Process local field_value
          // -------------------------------------------------
          UseSpCabCreateUpdateFieldValue1();

          if (IsExitState("DOCUMENT_FIELD_NF_RB"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Creation Error";
            export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

            return;
          }

          ++import.ExpImpRowLockFieldValue.Count;

          break;
      }

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      switch(TrimEnd(entities.Field.Name))
      {
        case "AAPPRSN01":
          local.PreviousField.Name = "AAPPRSN05";
          local.Local1.Count = 0;

          break;
        case "AAPPSTMT01":
          local.PreviousField.Name = "AAPPSTMT15";
          local.Local1.Count = 0;

          break;
        case "AAPP1CTORD":
          break;
        case "AAPP1FILDT":
          break;
        case "AAPP1PMTAM":
          break;
        case "AAPP1PMMSG":
          break;
        case "AAPP1PMTFQ":
          break;
        case "AAPP1PMTLO":
          break;
        case "AAPP1TRBCO":
          break;
        case "IMHHMBRNM0":
          local.PreviousField.Name = "IMHHMBRNM9";

          break;
        default:
          break;
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveFieldValue1(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFieldValue2(FieldValue source, FieldValue target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveSpPrintWorkSet(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MidInitial = source.MidInitial;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpCabCreateUpdateFieldValue1()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Field.Name = entities.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue2()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Current.Name;
    useImport.FieldValue.Value = local.Local1.Item.GfieldValue.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue3()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Current.Name;
    useImport.FieldValue.Value = local.Address.Item.GlocalAddress.Value;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private string UseSpDocFormatDate()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    MoveSpPrintWorkSet(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private bool ReadAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(
          command, "tanfCode", entities.AdministrativeAppeal.AacTanfCode ?? ""
          );
        db.SetDate(
          command, "takenDt",
          entities.AdministrativeAppeal.AacRTakenDate.GetValueOrDefault());
        db.SetString(
          command, "cpaType", entities.AdministrativeAppeal.CpaRType ?? "");
        db.SetString(
          command, "type", entities.AdministrativeAppeal.AacRType ?? "");
        db.SetString(
          command, "cspNumber", entities.AdministrativeAppeal.CspRNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "adminAppealId", import.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.RequestDate = db.GetDate(reader, 3);
        entities.AdministrativeAppeal.ReceivedDate = db.GetDate(reader, 4);
        entities.AdministrativeAppeal.Respondent = db.GetString(reader, 5);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.AppellantRelationship =
          db.GetNullableString(reader, 10);
        entities.AdministrativeAppeal.Date = db.GetNullableDate(reader, 11);
        entities.AdministrativeAppeal.AdminOrderDate =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeAppeal.WithdrawDate =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeAppeal.RequestFurtherReviewDate =
          db.GetNullableDate(reader, 14);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 15);
        entities.AdministrativeAppeal.CpaRType =
          db.GetNullableString(reader, 16);
        entities.AdministrativeAppeal.CspRNumber =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.AacRType =
          db.GetNullableString(reader, 18);
        entities.AdministrativeAppeal.AacRTakenDate =
          db.GetNullableDate(reader, 19);
        entities.AdministrativeAppeal.AatType =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 21);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 22);
        entities.AdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 24);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 25);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 26);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 27);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 28);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 29);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 30);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 31);
        entities.AdministrativeAppeal.AacTanfCode =
          db.GetNullableString(reader, 32);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaRType",
          entities.AdministrativeAppeal.CpaRType);
        CheckValid<AdministrativeAppeal>("AacRType",
          entities.AdministrativeAppeal.AacRType);
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.AdministrativeAppeal.CspQNumber ?? ""
          );
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMember.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ImHouseholdMember.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "dependancy", import.Field.Dependancy);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", local.Local1.Item.GlegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 2);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMember()
  {
    entities.ImHouseholdMember.Populated = false;

    return ReadEach("ReadImHouseholdMember",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.
          SetDate(command, "startDate", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.EndDate = db.GetDate(reader, 3);
        entities.ImHouseholdMember.Relationship = db.GetString(reader, 4);
        entities.ImHouseholdMember.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMemberImHousehold()
  {
    entities.ImHouseholdMember.Populated = false;
    entities.ImHousehold.Populated = false;

    return Read("ReadImHouseholdMemberImHousehold",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.
          SetDate(command, "startDate", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.EndDate = db.GetDate(reader, 3);
        entities.ImHouseholdMember.Relationship = db.GetString(reader, 4);
        entities.ImHouseholdMember.Populated = true;
        entities.ImHousehold.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.AdministrativeAppeal.CspRNumber ?? ""
          );
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "cpaRType", entities.AdministrativeAppeal.CpaRType ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", local.Local1.Item.GlegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 2);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 3);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadPositionStatement()
  {
    entities.PositionStatement.Populated = false;

    return ReadEach("ReadPositionStatement",
      (db, command) =>
      {
        db.SetInt32(
          command, "aapIdentifier", entities.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.PositionStatement.AapIdentifier = db.GetInt32(reader, 0);
        entities.PositionStatement.Number = db.GetInt32(reader, 1);
        entities.PositionStatement.CreatedTstamp = db.GetDateTime(reader, 2);
        entities.PositionStatement.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.PositionStatement.Explanation = db.GetString(reader, 4);
        entities.PositionStatement.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", local.Local1.Item.GlegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.Populated = true;
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of ExpImpRowLockFieldValue.
    /// </summary>
    [JsonPropertyName("expImpRowLockFieldValue")]
    public Common ExpImpRowLockFieldValue
    {
      get => expImpRowLockFieldValue ??= new();
      set => expImpRowLockFieldValue = value;
    }

    private SpDocKey spDocKey;
    private Common batch;
    private Infrastructure infrastructure;
    private Document document;
    private Field field;
    private FieldValue fieldValue;
    private DateWorkArea current;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    private SpDocKey spDocKey;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
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
      /// A value of GfieldValue.
      /// </summary>
      [JsonPropertyName("gfieldValue")]
      public FieldValue GfieldValue
      {
        get => gfieldValue ??= new();
        set => gfieldValue = value;
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
      /// A value of GlocalCourtCaseNo.
      /// </summary>
      [JsonPropertyName("glocalCourtCaseNo")]
      public FieldValue GlocalCourtCaseNo
      {
        get => glocalCourtCaseNo ??= new();
        set => glocalCourtCaseNo = value;
      }

      /// <summary>
      /// A value of GlocalFiledDate.
      /// </summary>
      [JsonPropertyName("glocalFiledDate")]
      public FieldValue GlocalFiledDate
      {
        get => glocalFiledDate ??= new();
        set => glocalFiledDate = value;
      }

      /// <summary>
      /// A value of GlocalPaymentAmount.
      /// </summary>
      [JsonPropertyName("glocalPaymentAmount")]
      public FieldValue GlocalPaymentAmount
      {
        get => glocalPaymentAmount ??= new();
        set => glocalPaymentAmount = value;
      }

      /// <summary>
      /// A value of GlocalPaymentMessage.
      /// </summary>
      [JsonPropertyName("glocalPaymentMessage")]
      public FieldValue GlocalPaymentMessage
      {
        get => glocalPaymentMessage ??= new();
        set => glocalPaymentMessage = value;
      }

      /// <summary>
      /// A value of GlocalPaymentFrequency.
      /// </summary>
      [JsonPropertyName("glocalPaymentFrequency")]
      public FieldValue GlocalPaymentFrequency
      {
        get => glocalPaymentFrequency ??= new();
        set => glocalPaymentFrequency = value;
      }

      /// <summary>
      /// A value of GlocalPaymentLocation.
      /// </summary>
      [JsonPropertyName("glocalPaymentLocation")]
      public FieldValue GlocalPaymentLocation
      {
        get => glocalPaymentLocation ??= new();
        set => glocalPaymentLocation = value;
      }

      /// <summary>
      /// A value of GlocalTribunal.
      /// </summary>
      [JsonPropertyName("glocalTribunal")]
      public FieldValue GlocalTribunal
      {
        get => glocalTribunal ??= new();
        set => glocalTribunal = value;
      }

      /// <summary>
      /// Gets a value of Subordinate.
      /// </summary>
      [JsonIgnore]
      public Array<SubordinateGroup> Subordinate => subordinate ??= new(
        SubordinateGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Subordinate for json serialization.
      /// </summary>
      [JsonPropertyName("subordinate")]
      [Computed]
      public IList<SubordinateGroup> Subordinate_Json
      {
        get => subordinate;
        set => Subordinate.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private FieldValue gfieldValue;
      private LegalAction glegalAction;
      private FieldValue glocalCourtCaseNo;
      private FieldValue glocalFiledDate;
      private FieldValue glocalPaymentAmount;
      private FieldValue glocalPaymentMessage;
      private FieldValue glocalPaymentFrequency;
      private FieldValue glocalPaymentLocation;
      private FieldValue glocalTribunal;
      private Array<SubordinateGroup> subordinate;
    }

    /// <summary>A SubordinateGroup group.</summary>
    [Serializable]
    public class SubordinateGroup
    {
      /// <summary>
      /// A value of GlocalSub.
      /// </summary>
      [JsonPropertyName("glocalSub")]
      public LegalActionDetail GlocalSub
      {
        get => glocalSub ??= new();
        set => glocalSub = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalActionDetail glocalSub;
    }

    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
    {
      /// <summary>
      /// A value of GlocalAddress.
      /// </summary>
      [JsonPropertyName("glocalAddress")]
      public FieldValue GlocalAddress
      {
        get => glocalAddress ??= new();
        set => glocalAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalAddress;
    }

    /// <summary>
    /// A value of ExitLoop.
    /// </summary>
    [JsonPropertyName("exitLoop")]
    public Common ExitLoop
    {
      get => exitLoop ??= new();
      set => exitLoop = value;
    }

    /// <summary>
    /// A value of FoundLegalDetails.
    /// </summary>
    [JsonPropertyName("foundLegalDetails")]
    public Common FoundLegalDetails
    {
      get => foundLegalDetails ??= new();
      set => foundLegalDetails = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of LiteralMinPeriodsPerYear.
    /// </summary>
    [JsonPropertyName("literalMinPeriodsPerYear")]
    public Common LiteralMinPeriodsPerYear
    {
      get => literalMinPeriodsPerYear ??= new();
      set => literalMinPeriodsPerYear = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    /// <summary>
    /// A value of PreviousSubscript.
    /// </summary>
    [JsonPropertyName("previousSubscript")]
    public Common PreviousSubscript
    {
      get => previousSubscript ??= new();
      set => previousSubscript = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    /// <summary>
    /// A value of MinPeriodsPerYear.
    /// </summary>
    [JsonPropertyName("minPeriodsPerYear")]
    public Common MinPeriodsPerYear
    {
      get => minPeriodsPerYear ??= new();
      set => minPeriodsPerYear = value;
    }

    /// <summary>
    /// A value of PeriodsPerYear.
    /// </summary>
    [JsonPropertyName("periodsPerYear")]
    public Common PeriodsPerYear
    {
      get => periodsPerYear ??= new();
      set => periodsPerYear = value;
    }

    /// <summary>
    /// A value of CourtCaseTotal.
    /// </summary>
    [JsonPropertyName("courtCaseTotal")]
    public Common CourtCaseTotal
    {
      get => courtCaseTotal ??= new();
      set => courtCaseTotal = value;
    }

    /// <summary>
    /// A value of MaxLegalActions.
    /// </summary>
    [JsonPropertyName("maxLegalActions")]
    public Common MaxLegalActions
    {
      get => maxLegalActions ??= new();
      set => maxLegalActions = value;
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
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
    }

    /// <summary>
    /// A value of PreviousField.
    /// </summary>
    [JsonPropertyName("previousField")]
    public Field PreviousField
    {
      get => previousField ??= new();
      set => previousField = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of ProcessGroup.
    /// </summary>
    [JsonPropertyName("processGroup")]
    public Common ProcessGroup
    {
      get => processGroup ??= new();
      set => processGroup = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Field Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    public Field Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentRoot.
    /// </summary>
    [JsonPropertyName("currentRoot")]
    public Field CurrentRoot
    {
      get => currentRoot ??= new();
      set => currentRoot = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Format.
    /// </summary>
    [JsonPropertyName("format")]
    public FieldValue Format
    {
      get => format ??= new();
      set => format = value;
    }

    /// <summary>
    /// A value of FormatPaymentAmtDollars.
    /// </summary>
    [JsonPropertyName("formatPaymentAmtDollars")]
    public FieldValue FormatPaymentAmtDollars
    {
      get => formatPaymentAmtDollars ??= new();
      set => formatPaymentAmtDollars = value;
    }

    /// <summary>
    /// A value of FormatPaymentAmtCents.
    /// </summary>
    [JsonPropertyName("formatPaymentAmtCents")]
    public FieldValue FormatPaymentAmtCents
    {
      get => formatPaymentAmtCents ??= new();
      set => formatPaymentAmtCents = value;
    }

    private Common exitLoop;
    private Common foundLegalDetails;
    private Case1 case1;
    private DateWorkArea null1;
    private Common literalMinPeriodsPerYear;
    private LegalAction previousLegalAction;
    private Common previousSubscript;
    private AbendData abendData;
    private Common count;
    private DateWorkArea zdelLocalCurrent;
    private Common minPeriodsPerYear;
    private Common periodsPerYear;
    private Common courtCaseTotal;
    private Common maxLegalActions;
    private Array<LocalGroup> local1;
    private Array<AddressGroup> address;
    private Field previousField;
    private FieldValue fieldValue;
    private DateWorkArea dateWorkArea;
    private SpPrintWorkSet spPrintWorkSet;
    private Common processGroup;
    private Field temp;
    private Common position;
    private Field current;
    private Field currentRoot;
    private Code code;
    private CodeValue codeValue;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FieldValue format;
    private FieldValue formatPaymentAmtDollars;
    private FieldValue formatPaymentAmtCents;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of AdminActionCertObligation.
    /// </summary>
    [JsonPropertyName("adminActionCertObligation")]
    public AdminActionCertObligation AdminActionCertObligation
    {
      get => adminActionCertObligation ??= new();
      set => adminActionCertObligation = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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
    /// A value of PositionStatement.
    /// </summary>
    [JsonPropertyName("positionStatement")]
    public PositionStatement PositionStatement
    {
      get => positionStatement ??= new();
      set => positionStatement = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of ImHouseholdMember.
    /// </summary>
    [JsonPropertyName("imHouseholdMember")]
    public ImHouseholdMember ImHouseholdMember
    {
      get => imHouseholdMember ??= new();
      set => imHouseholdMember = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private CsePersonAccount csePersonAccount;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalActionDetail legalActionDetail;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private Obligation obligation;
    private AdminActionCertObligation adminActionCertObligation;
    private LegalAction legalAction;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeAction administrativeAction;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private PositionStatement positionStatement;
    private AdministrativeAppeal administrativeAppeal;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private ImHouseholdMember imHouseholdMember;
    private ImHousehold imHousehold;
  }
#endregion
}
