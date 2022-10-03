// Program: FN_B738_ANNUAL_DA32_REPORT, ID: 1902603725, model: 746.
// Short name: SWEB738P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B738_ANNUAL_DA32_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB738AnnualDa32Report: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B738_ANNUAL_DA32_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB738AnnualDa32Report(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB738AnnualDa32Report.
  /// </summary>
  public FnB738AnnualDa32Report(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 08/1082017      DDupree   	Initial Creation - CQ58694
    // srrun293
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************************************************************
    // ***********************************************************************************************
    UseFnB738Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Spaces.Text15 = "";
    local.Convert.Year = Year(local.ProgramProcessingInfo.ProcessDate);

    // end of yr date
    local.ConvertFrom.TextDateYyyy = NumberToString(local.Convert.Year, 4);
    local.ConvertFrom.TextDateMm = "06";
    local.ConvertFrom.TestDateDd = "30";
    local.EndOfYear.Date = StringToDate(local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd);

    // line 6 date
    local.ConvertFrom.TextDateYyyy = NumberToString(local.Convert.Year, 4);
    local.ConvertFrom.TextDateMm = "06";
    local.ConvertFrom.TestDateDd = "01";
    local.ConvertFrom.TestTimeHh = "00";
    local.ConvertFrom.TextTimeMm = "00";
    local.ConvertFrom.TextTimeSs = "00";
    local.ConvertFrom.TextMillisecond = "000000";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Line6From.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateYyyy = NumberToString(local.Convert.Year, 4);
    local.ConvertTo.TextDateMm = "06";
    local.ConvertTo.TestDateDd = "30";
    local.ConvertTo.TestTimeHh = "23";
    local.ConvertTo.TextTimeMm = "59";
    local.ConvertTo.TextTimeSs = "59";
    local.ConvertTo.TextMillisecond = "999999";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.Line6To.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // current date
    local.ConvertFrom.TextDateMm = "07";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Currrent.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);

    // line 7 date
    local.ConvertFrom.TextDateMm = "05";
    local.ConvertFrom.TestDateDd = "02";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Line7From.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateMm = "05";
    local.ConvertTo.TestDateDd = "31";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.Line7To.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // line 8 date
    local.ConvertFrom.TextDateMm = "04";
    local.ConvertFrom.TestDateDd = "02";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Line8From.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateMm = "05";
    local.ConvertTo.TestDateDd = "01";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.Line8To.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // line 9 date
    local.ConvertFrom.TextDateMm = "03";
    local.ConvertFrom.TestDateDd = "03";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Line9From.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateMm = "04";
    local.ConvertTo.TestDateDd = "01";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.Line9To.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // line 10 date
    local.ConvertFrom.TextDateMm = "02";
    local.ConvertFrom.TestDateDd = "01";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Line10From.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateMm = "03";
    local.ConvertTo.TestDateDd = "02";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.Line10To.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // line 11 date
    local.ConvertFrom.TextDateYyyy = NumberToString(local.Convert.Year - 1, 4);
    local.ConvertFrom.TextDateMm = "07";
    local.ConvertFrom.TestDateDd = "01";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.Line11From.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateMm = "01";
    local.ConvertTo.TestDateDd = "31";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.Line11To.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // base date
    local.ConvertFrom.TextDateYyyy = NumberToString(local.Convert.Year - 2, 4);
    local.ConvertFrom.TextDateMm = "07";
    local.ConvertFrom.TestDateDd = "01";
    local.ConvertFrom.TextTimestamp = local.ConvertFrom.TextDateYyyy + "-" + local
      .ConvertFrom.TextDateMm + "-" + local.ConvertFrom.TestDateDd + "-" + local
      .ConvertFrom.TestTimeHh + "." + local.ConvertFrom.TextTimeMm + "." + local
      .ConvertFrom.TextTimeSs + "." + local.ConvertFrom.TextMillisecond;
    local.BaseDtFrom.Timestamp = Timestamp(local.ConvertFrom.TextTimestamp);
    local.ConvertTo.TextDateYyyy = NumberToString(local.Convert.Year - 1, 4);
    local.ConvertTo.TextDateMm = "06";
    local.ConvertTo.TestDateDd = "30";
    local.ConvertTo.TextTimestamp = local.ConvertTo.TextDateYyyy + "-" + local
      .ConvertTo.TextDateMm + "-" + local.ConvertTo.TestDateDd + "-" + local
      .ConvertTo.TestTimeHh + "." + local.ConvertTo.TextTimeMm + "." + local
      .ConvertTo.TextTimeSs + "." + local.ConvertTo.TextMillisecond;
    local.BaseDtTo.Timestamp = Timestamp(local.ConvertTo.TextTimestamp);

    // column 1
    foreach(var item in ReadDebtDetail1())
    {
      if (!Lt(entities.DebtDetail.CreatedTmst, local.Line6From.Timestamp) && !
        Lt(local.Line6To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 6
        local.Group1.Index = 0;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line7From.Timestamp) &&
        !Lt(local.Line7To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 7
        local.Group1.Index = 1;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line8From.Timestamp) &&
        !Lt(local.Line8To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 8
        local.Group1.Index = 2;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line9From.Timestamp) &&
        !Lt(local.Line9To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 9
        local.Group1.Index = 3;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line10From.Timestamp) &&
        !Lt(local.Line10To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 10
        local.Group1.Index = 4;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line11From.Timestamp) &&
        !Lt(local.Line11To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 11
        local.Group1.Index = 5;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.BaseDtFrom.Timestamp) &&
        !Lt(local.BaseDtTo.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 12
        local.Group1.Index = 6;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -1)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -1),
        entities.DebtDetail.CreatedTmst))
      {
        // line 13
        local.Group1.Index = 7;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -2)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -2),
        entities.DebtDetail.CreatedTmst))
      {
        // line 14
        local.Group1.Index = 8;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -3)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -3),
        entities.DebtDetail.CreatedTmst))
      {
        // line 15
        local.Group1.Index = 9;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -4)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -4),
        entities.DebtDetail.CreatedTmst))
      {
        // line 16
        local.Group1.Index = 10;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -5)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -5),
        entities.DebtDetail.CreatedTmst))
      {
        // line 17
        local.Group1.Index = 11;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -6)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -6),
        entities.DebtDetail.CreatedTmst))
      {
        // line 18
        local.Group1.Index = 12;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -7)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -7),
        entities.DebtDetail.CreatedTmst))
      {
        // line 19
        local.Group1.Index = 13;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -8)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -8),
        entities.DebtDetail.CreatedTmst))
      {
        // line 20
        local.Group1.Index = 14;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -9)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -9),
        entities.DebtDetail.CreatedTmst))
      {
        // line 21
        local.Group1.Index = 15;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -10)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -10),
        entities.DebtDetail.CreatedTmst))
      {
        // line 22
        local.Group1.Index = 16;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -11)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -11),
        entities.DebtDetail.CreatedTmst))
      {
        // line 23
        local.Group1.Index = 17;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -12)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -12),
        entities.DebtDetail.CreatedTmst))
      {
        // line 24
        local.Group1.Index = 18;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -13)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -13),
        entities.DebtDetail.CreatedTmst))
      {
        // line 25
        local.Group1.Index = 19;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -19)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -14),
        entities.DebtDetail.CreatedTmst))
      {
        // line 26
        local.Group1.Index = 20;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else
      {
        continue;
      }

      // total
      local.Group1.Index = 21;
      local.Group1.CheckSize();

      local.Group1.Update.Grp1B.TotalCurrency =
        local.Group1.Item.Grp1B.TotalCurrency + entities
        .DebtDetail.BalanceDueAmt;
    }

    // column 2
    foreach(var item in ReadCollectionDebtDetail3())
    {
      if (!Lt(entities.DebtDetail.CreatedTmst, local.Line6From.Timestamp) && !
        Lt(local.Line6To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 6
        local.Group1.Index = 0;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line7From.Timestamp) &&
        !Lt(local.Line7To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 7
        local.Group1.Index = 1;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line8From.Timestamp) &&
        !Lt(local.Line8To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 8
        local.Group1.Index = 2;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line9From.Timestamp) &&
        !Lt(local.Line9To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 9
        local.Group1.Index = 3;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line10From.Timestamp) &&
        !Lt(local.Line10To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 10
        local.Group1.Index = 4;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line11From.Timestamp) &&
        !Lt(local.Line11To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 11
        local.Group1.Index = 5;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.BaseDtFrom.Timestamp) &&
        !Lt(local.BaseDtTo.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 12
        local.Group1.Index = 6;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -1)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -1),
        entities.DebtDetail.CreatedTmst))
      {
        // line 13
        local.Group1.Index = 7;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -2)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -2),
        entities.DebtDetail.CreatedTmst))
      {
        // line 14
        local.Group1.Index = 8;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -3)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -3),
        entities.DebtDetail.CreatedTmst))
      {
        // line 15
        local.Group1.Index = 9;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -4)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -4),
        entities.DebtDetail.CreatedTmst))
      {
        // line 16
        local.Group1.Index = 10;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -5)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -5),
        entities.DebtDetail.CreatedTmst))
      {
        // line 17
        local.Group1.Index = 11;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -6)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -6),
        entities.DebtDetail.CreatedTmst))
      {
        // line 18
        local.Group1.Index = 12;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -7)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -7),
        entities.DebtDetail.CreatedTmst))
      {
        // line 19
        local.Group1.Index = 13;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -8)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -8),
        entities.DebtDetail.CreatedTmst))
      {
        // line 20
        local.Group1.Index = 14;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -9)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -9),
        entities.DebtDetail.CreatedTmst))
      {
        // line 21
        local.Group1.Index = 15;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -10)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -10),
        entities.DebtDetail.CreatedTmst))
      {
        // line 22
        local.Group1.Index = 16;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -11)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -11),
        entities.DebtDetail.CreatedTmst))
      {
        // line 23
        local.Group1.Index = 17;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -12)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -12),
        entities.DebtDetail.CreatedTmst))
      {
        // line 24
        local.Group1.Index = 18;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -13)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -13),
        entities.DebtDetail.CreatedTmst))
      {
        // line 25
        local.Group1.Index = 19;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -19)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -14),
        entities.DebtDetail.CreatedTmst))
      {
        // line 26
        local.Group1.Index = 20;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else
      {
        continue;
      }

      // total
      local.Group1.Index = 21;
      local.Group1.CheckSize();

      local.Group1.Update.Grp1D.TotalCurrency =
        local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
    }

    // column 3
    foreach(var item in ReadCollectionDebtDetail1())
    {
      if (!Lt(entities.DebtDetail.CreatedTmst, local.Line6From.Timestamp) && !
        Lt(local.Line6To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 6
        local.Group1.Index = 0;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line7From.Timestamp) &&
        !Lt(local.Line7To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 7
        local.Group1.Index = 1;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line8From.Timestamp) &&
        !Lt(local.Line8To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 8
        local.Group1.Index = 2;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line9From.Timestamp) &&
        !Lt(local.Line9To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 9
        local.Group1.Index = 3;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line10From.Timestamp) &&
        !Lt(local.Line10To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 10
        local.Group1.Index = 4;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line11From.Timestamp) &&
        !Lt(local.Line11To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 11
        local.Group1.Index = 5;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.BaseDtFrom.Timestamp) &&
        !Lt(local.BaseDtTo.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 12
        local.Group1.Index = 6;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -1)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -1),
        entities.DebtDetail.CreatedTmst))
      {
        // line 13
        local.Group1.Index = 7;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -2)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -2),
        entities.DebtDetail.CreatedTmst))
      {
        // line 14
        local.Group1.Index = 8;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -3)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -3),
        entities.DebtDetail.CreatedTmst))
      {
        // line 15
        local.Group1.Index = 9;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -4)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -4),
        entities.DebtDetail.CreatedTmst))
      {
        // line 16
        local.Group1.Index = 10;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -5)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -5),
        entities.DebtDetail.CreatedTmst))
      {
        // line 17
        local.Group1.Index = 11;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -6)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -6),
        entities.DebtDetail.CreatedTmst))
      {
        // line 18
        local.Group1.Index = 12;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -7)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -7),
        entities.DebtDetail.CreatedTmst))
      {
        // line 19
        local.Group1.Index = 13;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -8)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -8),
        entities.DebtDetail.CreatedTmst))
      {
        // line 20
        local.Group1.Index = 14;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -9)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -9),
        entities.DebtDetail.CreatedTmst))
      {
        // line 21
        local.Group1.Index = 15;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -10)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -10),
        entities.DebtDetail.CreatedTmst))
      {
        // line 22
        local.Group1.Index = 16;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -11)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -11),
        entities.DebtDetail.CreatedTmst))
      {
        // line 23
        local.Group1.Index = 17;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -12)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -12),
        entities.DebtDetail.CreatedTmst))
      {
        // line 24
        local.Group1.Index = 18;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -13)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -13),
        entities.DebtDetail.CreatedTmst))
      {
        // line 25
        local.Group1.Index = 19;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -19)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -14),
        entities.DebtDetail.CreatedTmst))
      {
        // line 26
        local.Group1.Index = 20;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else
      {
        continue;
      }

      // total
      local.Group1.Index = 21;
      local.Group1.CheckSize();

      local.Group1.Update.Grp1F.TotalCurrency =
        local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
    }

    // column total
    for(local.Group1.Index = 0; local.Group1.Index < local.Group1.Count; ++
      local.Group1.Index)
    {
      if (!local.Group1.CheckSize())
      {
        break;
      }

      local.Group1.Update.Grp1H.TotalCurrency =
        local.Group1.Item.Grp1B.TotalCurrency + local
        .Group1.Item.Grp1D.TotalCurrency - local
        .Group1.Item.Grp1F.TotalCurrency;
    }

    local.Group1.CheckIndex();
    local.CommasRequired.Flag = "Y";
    local.Group1.Index = 0;

    for(var limit = local.Group1.Count; local.Group1.Index < limit; ++
      local.Group1.Index)
    {
      if (!local.Group1.CheckSize())
      {
        break;
      }

      local.Supply.TotalCurrency = local.Group1.Item.Grp1B.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
      local.Supply.TotalCurrency = local.Group1.Item.Grp1D.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
      local.Supply.TotalCurrency = local.Group1.Item.Grp1F.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
      local.Supply.TotalCurrency = local.Group1.Item.Grp1H.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column4.Text20 = local.RetCurrency.Text20;

      switch(local.Group1.Index + 1)
      {
        case 1:
          local.ItemName.Text37 = " Current";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 2:
          local.ItemName.Text37 = " 1 - 30 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 3:
          local.ItemName.Text37 = " 31 - 60 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 4:
          local.ItemName.Text37 = " 61 - 90 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 5:
          local.ItemName.Text37 = " 91 - 120 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 6:
          local.ItemName.Text37 = " 121 - 365 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 7:
          local.ItemName.Text37 = " 1 year but less than 2 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 8:
          local.ItemName.Text37 = " 2 years but less than 3 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 9:
          local.ItemName.Text37 = " 3 years but less than 4 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 10:
          local.ItemName.Text37 = " 4 years but less than 5 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 11:
          local.ItemName.Text37 = " 5 years but less than 6 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 12:
          local.ItemName.Text37 = " 6 years but less than 7 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 13:
          local.ItemName.Text37 = " 7 years but less than 8 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 14:
          local.ItemName.Text37 = " 8 years but less than 9 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 15:
          local.ItemName.Text37 = " 9 years but less than 10 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 16:
          local.ItemName.Text37 = " 10 years but less than 11 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 17:
          local.ItemName.Text37 = " 11 years but less than 12 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 18:
          local.ItemName.Text37 = " 12 years but less than 13 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 19:
          local.ItemName.Text37 = " 13 years but less than 14 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 20:
          local.ItemName.Text37 = " 14 years but less than 15 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 21:
          local.ItemName.Text37 = " 15 years but less than 20 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 22:
          local.ItemName.Text37 = "";
          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ItemName.Text37 = "";
          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ItemName.Text37 = " Total Receivables Balance";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        default:
          break;
      }
    }

    local.Group1.CheckIndex();
    local.ItemName.Text37 = "";
    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.Column1WorkArea.Text20 = "Collections Rec'd";
    local.Column2WorkArea.Text20 = "Prior FY Collections";
    local.Column3WorkArea.Text20 = "Total FY Collection";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.Column1WorkArea.Text20 = "prior to post FY adj";
    local.Column2WorkArea.Text20 = "adjusted in curr FY";
    local.Column3WorkArea.Text20 = "";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "(-)  " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    foreach(var item in ReadCollection18())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection9())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " Collections Received";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    for(local.Group1.Index = 0; local.Group1.Index < local.Group1.Count; ++
      local.Group1.Index)
    {
      if (!local.Group1.CheckSize())
      {
        break;
      }

      // need to clear the group view to get ready for the next section that 
      // needs it
      local.Group1.Update.Grp1B.TotalCurrency = 0;
      local.Group1.Update.Grp1D.TotalCurrency = 0;
      local.Group1.Update.Grp1F.TotalCurrency = 0;
      local.Group1.Update.Grp1H.TotalCurrency = 0;
    }

    local.Group1.CheckIndex();
    local.EabReportSend.RptDetail =
      "                                                 DA32 Annual Report - CSE Non-Recovery";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.Column1WorkArea.Text20 = "Past Due Obligations";
    local.Column2WorkArea.Text20 = "Accnt for collection";
    local.Column3WorkArea.Text20 = "Account for coll adj";
    local.Column4.Text20 = "Total FY CSE";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.Column1WorkArea.Text20 = "prior to post FY adj";
    local.Column2WorkArea.Text20 = "rec'd after FY end";
    local.Column3WorkArea.Text20 = "made after FY end";
    local.Column4.Text20 = "Obligation Due";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "(+)  " +
      local.Column3WorkArea.Text20 + "(-)  " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // lines 46-66
    // column 1
    foreach(var item in ReadDebtDetail2())
    {
      if (!Lt(entities.DebtDetail.CreatedTmst, local.Line6From.Timestamp) && !
        Lt(local.Line6To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 6
        local.Group1.Index = 0;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line7From.Timestamp) &&
        !Lt(local.Line7To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 7
        local.Group1.Index = 1;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line8From.Timestamp) &&
        !Lt(local.Line8To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 8
        local.Group1.Index = 2;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line9From.Timestamp) &&
        !Lt(local.Line9To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 9
        local.Group1.Index = 3;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line10From.Timestamp) &&
        !Lt(local.Line10To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 10
        local.Group1.Index = 4;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line11From.Timestamp) &&
        !Lt(local.Line11To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 11
        local.Group1.Index = 5;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.BaseDtFrom.Timestamp) &&
        !Lt(local.BaseDtTo.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 12
        local.Group1.Index = 6;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -1)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -1),
        entities.DebtDetail.CreatedTmst))
      {
        // line 13
        local.Group1.Index = 7;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -2)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -2),
        entities.DebtDetail.CreatedTmst))
      {
        // line 14
        local.Group1.Index = 8;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -3)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -3),
        entities.DebtDetail.CreatedTmst))
      {
        // line 15
        local.Group1.Index = 9;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -4)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -4),
        entities.DebtDetail.CreatedTmst))
      {
        // line 16
        local.Group1.Index = 10;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -5)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -5),
        entities.DebtDetail.CreatedTmst))
      {
        // line 17
        local.Group1.Index = 11;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -6)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -6),
        entities.DebtDetail.CreatedTmst))
      {
        // line 18
        local.Group1.Index = 12;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -7)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -7),
        entities.DebtDetail.CreatedTmst))
      {
        // line 19
        local.Group1.Index = 13;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -8)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -8),
        entities.DebtDetail.CreatedTmst))
      {
        // line 20
        local.Group1.Index = 14;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -9)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -9),
        entities.DebtDetail.CreatedTmst))
      {
        // line 21
        local.Group1.Index = 15;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -10)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -10),
        entities.DebtDetail.CreatedTmst))
      {
        // line 22
        local.Group1.Index = 16;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -11)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -11),
        entities.DebtDetail.CreatedTmst))
      {
        // line 23
        local.Group1.Index = 17;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -12)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -12),
        entities.DebtDetail.CreatedTmst))
      {
        // line 24
        local.Group1.Index = 18;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -13)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -13),
        entities.DebtDetail.CreatedTmst))
      {
        // line 25
        local.Group1.Index = 19;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -19)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -14),
        entities.DebtDetail.CreatedTmst))
      {
        // line 26
        local.Group1.Index = 20;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1B.TotalCurrency =
          local.Group1.Item.Grp1B.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;
      }
      else
      {
        continue;
      }

      // total
      local.Group1.Index = 21;
      local.Group1.CheckSize();

      local.Group1.Update.Grp1B.TotalCurrency =
        local.Group1.Item.Grp1B.TotalCurrency + entities
        .DebtDetail.BalanceDueAmt;
    }

    // column 2
    foreach(var item in ReadCollectionDebtDetail4())
    {
      if (!Lt(entities.DebtDetail.CreatedTmst, local.Line6From.Timestamp) && !
        Lt(local.Line6To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 6
        local.Group1.Index = 0;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line7From.Timestamp) &&
        !Lt(local.Line7To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 7
        local.Group1.Index = 1;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line8From.Timestamp) &&
        !Lt(local.Line8To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 8
        local.Group1.Index = 2;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line9From.Timestamp) &&
        !Lt(local.Line9To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 9
        local.Group1.Index = 3;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line10From.Timestamp) &&
        !Lt(local.Line10To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 10
        local.Group1.Index = 4;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line11From.Timestamp) &&
        !Lt(local.Line11To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 11
        local.Group1.Index = 5;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.BaseDtFrom.Timestamp) &&
        !Lt(local.BaseDtTo.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 12
        local.Group1.Index = 6;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -1)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -1),
        entities.DebtDetail.CreatedTmst))
      {
        // line 13
        local.Group1.Index = 7;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -2)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -2),
        entities.DebtDetail.CreatedTmst))
      {
        // line 14
        local.Group1.Index = 8;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -3)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -3),
        entities.DebtDetail.CreatedTmst))
      {
        // line 15
        local.Group1.Index = 9;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -4)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -4),
        entities.DebtDetail.CreatedTmst))
      {
        // line 16
        local.Group1.Index = 10;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -5)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -5),
        entities.DebtDetail.CreatedTmst))
      {
        // line 17
        local.Group1.Index = 11;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -6)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -6),
        entities.DebtDetail.CreatedTmst))
      {
        // line 18
        local.Group1.Index = 12;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -7)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -7),
        entities.DebtDetail.CreatedTmst))
      {
        // line 19
        local.Group1.Index = 13;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -8)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -8),
        entities.DebtDetail.CreatedTmst))
      {
        // line 20
        local.Group1.Index = 14;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -9)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -9),
        entities.DebtDetail.CreatedTmst))
      {
        // line 21
        local.Group1.Index = 15;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -10)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -10),
        entities.DebtDetail.CreatedTmst))
      {
        // line 22
        local.Group1.Index = 16;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -11)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -11),
        entities.DebtDetail.CreatedTmst))
      {
        // line 23
        local.Group1.Index = 17;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -12)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -12),
        entities.DebtDetail.CreatedTmst))
      {
        // line 24
        local.Group1.Index = 18;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -13)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -13),
        entities.DebtDetail.CreatedTmst))
      {
        // line 25
        local.Group1.Index = 19;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -19)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -14),
        entities.DebtDetail.CreatedTmst))
      {
        // line 26
        local.Group1.Index = 20;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1D.TotalCurrency =
          local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
      }
      else
      {
        continue;
      }

      // total
      local.Group1.Index = 21;
      local.Group1.CheckSize();

      local.Group1.Update.Grp1D.TotalCurrency =
        local.Group1.Item.Grp1D.TotalCurrency + entities.Collection.Amount;
    }

    // column 3
    foreach(var item in ReadCollectionDebtDetail2())
    {
      if (!Lt(entities.DebtDetail.CreatedTmst, local.Line6From.Timestamp) && !
        Lt(local.Line6To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 6
        local.Group1.Index = 0;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line7From.Timestamp) &&
        !Lt(local.Line7To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 7
        local.Group1.Index = 1;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line8From.Timestamp) &&
        !Lt(local.Line8To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 8
        local.Group1.Index = 2;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line9From.Timestamp) &&
        !Lt(local.Line9To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 9
        local.Group1.Index = 3;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line10From.Timestamp) &&
        !Lt(local.Line10To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 10
        local.Group1.Index = 4;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.Line11From.Timestamp) &&
        !Lt(local.Line11To.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 11
        local.Group1.Index = 5;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst, local.BaseDtFrom.Timestamp) &&
        !Lt(local.BaseDtTo.Timestamp, entities.DebtDetail.CreatedTmst))
      {
        // line 12
        local.Group1.Index = 6;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -1)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -1),
        entities.DebtDetail.CreatedTmst))
      {
        // line 13
        local.Group1.Index = 7;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -2)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -2),
        entities.DebtDetail.CreatedTmst))
      {
        // line 14
        local.Group1.Index = 8;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -3)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -3),
        entities.DebtDetail.CreatedTmst))
      {
        // line 15
        local.Group1.Index = 9;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -4)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -4),
        entities.DebtDetail.CreatedTmst))
      {
        // line 16
        local.Group1.Index = 10;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -5)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -5),
        entities.DebtDetail.CreatedTmst))
      {
        // line 17
        local.Group1.Index = 11;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -6)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -6),
        entities.DebtDetail.CreatedTmst))
      {
        // line 18
        local.Group1.Index = 12;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -7)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -7),
        entities.DebtDetail.CreatedTmst))
      {
        // line 19
        local.Group1.Index = 13;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -8)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -8),
        entities.DebtDetail.CreatedTmst))
      {
        // line 20
        local.Group1.Index = 14;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -9)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -9),
        entities.DebtDetail.CreatedTmst))
      {
        // line 21
        local.Group1.Index = 15;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -10)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -10),
        entities.DebtDetail.CreatedTmst))
      {
        // line 22
        local.Group1.Index = 16;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -11)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -11),
        entities.DebtDetail.CreatedTmst))
      {
        // line 23
        local.Group1.Index = 17;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -12)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -12),
        entities.DebtDetail.CreatedTmst))
      {
        // line 24
        local.Group1.Index = 18;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -13)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -13),
        entities.DebtDetail.CreatedTmst))
      {
        // line 25
        local.Group1.Index = 19;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else if (!Lt(entities.DebtDetail.CreatedTmst,
        AddYears(local.BaseDtFrom.Timestamp, -19)) && !
        Lt(AddYears(local.BaseDtTo.Timestamp, -14),
        entities.DebtDetail.CreatedTmst))
      {
        // line 26
        local.Group1.Index = 20;
        local.Group1.CheckSize();

        local.Group1.Update.Grp1F.TotalCurrency =
          local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
      }
      else
      {
        continue;
      }

      // total
      local.Group1.Index = 21;
      local.Group1.CheckSize();

      local.Group1.Update.Grp1F.TotalCurrency =
        local.Group1.Item.Grp1F.TotalCurrency + entities.Collection.Amount;
    }

    // column total
    for(local.Group1.Index = 0; local.Group1.Index < local.Group1.Count; ++
      local.Group1.Index)
    {
      if (!local.Group1.CheckSize())
      {
        break;
      }

      local.Group1.Update.Grp1H.TotalCurrency =
        local.Group1.Item.Grp1B.TotalCurrency + local
        .Group1.Item.Grp1D.TotalCurrency - local
        .Group1.Item.Grp1F.TotalCurrency;
    }

    local.Group1.CheckIndex();
    local.CommasRequired.Flag = "Y";
    local.Group1.Index = 0;

    for(var limit = local.Group1.Count; local.Group1.Index < limit; ++
      local.Group1.Index)
    {
      if (!local.Group1.CheckSize())
      {
        break;
      }

      local.Supply.TotalCurrency = local.Group1.Item.Grp1B.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
      local.Supply.TotalCurrency = local.Group1.Item.Grp1D.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
      local.Supply.TotalCurrency = local.Group1.Item.Grp1F.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
      local.Supply.TotalCurrency = local.Group1.Item.Grp1H.TotalCurrency;
      UseFnCabCurrencyToTextLarge();
      local.Column4.Text20 = local.RetCurrency.Text20;

      switch(local.Group1.Index + 1)
      {
        case 1:
          local.ItemName.Text37 = " Current";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 2:
          local.ItemName.Text37 = " 1 - 30 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 3:
          local.ItemName.Text37 = " 31 - 60 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 4:
          local.ItemName.Text37 = " 61 - 90 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 5:
          local.ItemName.Text37 = " 91 - 120 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 6:
          local.ItemName.Text37 = " 121 - 365 Days Past Due";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 7:
          local.ItemName.Text37 = " 1 year but less than 2 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 8:
          local.ItemName.Text37 = " 2 years but less than 3 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 9:
          local.ItemName.Text37 = " 3 years but less than 4 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 10:
          local.ItemName.Text37 = " 4 years but less than 5 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 11:
          local.ItemName.Text37 = " 5 years but less than 6 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 12:
          local.ItemName.Text37 = " 6 years but less than 7 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 13:
          local.ItemName.Text37 = " 7 years but less than 8 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 14:
          local.ItemName.Text37 = " 8 years but less than 9 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 15:
          local.ItemName.Text37 = " 9 years but less than 10 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 16:
          local.ItemName.Text37 = " 10 years but less than 11 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 17:
          local.ItemName.Text37 = " 11 years but less than 12 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 18:
          local.ItemName.Text37 = " 12 years but less than 13 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 19:
          local.ItemName.Text37 = " 13 years but less than 14 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 20:
          local.ItemName.Text37 = " 14 years but less than 15 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 21:
          local.ItemName.Text37 = " 15 years but less than 20 years old";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case 22:
          local.ItemName.Text37 = "";
          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ItemName.Text37 = "";
          local.EabReportSend.RptDetail = "";
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.ItemName.Text37 = " Total Receivables Balance";
          local.EabReportSend.RptDetail = local.ItemName.Text37 + local
            .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
            local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseEabExternalReportWriter();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabReportSend.RptDetail = "Error encountered writing report.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        default:
          break;
      }
    }

    local.Group1.CheckIndex();
    local.ItemName.Text37 = "";
    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.Column1WorkArea.Text20 = "Collections Rec'd";
    local.Column2WorkArea.Text20 = "Prior FY Collections";
    local.Column3WorkArea.Text20 = "Total FY Collections";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = "";
    local.Column1WorkArea.Text20 = "prior to post FY adj";
    local.Column2WorkArea.Text20 = "adjusted w/in curr F";
    local.Column3WorkArea.Text20 = "";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "Y (-) " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = " Collections Received";
    local.Column1WorkArea.Text20 = "";
    local.Column2WorkArea.Text20 = "";
    local.Column3WorkArea.Text20 = "";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ItemName.Text37 = " by Program/Obligation type";
    local.Column1WorkArea.Text20 = "";
    local.Column2WorkArea.Text20 = "";
    local.Column3WorkArea.Text20 = "";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 77
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection17())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection8())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " Fee Obligations";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 78
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection15())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection6())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " Medicals Obs - TAF and FC";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 79
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection16())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection7())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " Interstate Obligations";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 80
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection12())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection3())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " NA Program";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 81
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection10())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection1())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " TAF Program";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 82
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection11())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection2())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " FC Program";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 83
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection13())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection4())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " NC Program";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 84
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadCollection14())
    {
      local.Column1Common.TotalCurrency += entities.Collection.Amount;
    }

    foreach(var item in ReadCollection5())
    {
      local.Column2Common.TotalCurrency += entities.Collection.Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " NF Program";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // line 85
    local.Column1Common.TotalCurrency = 0;
    local.Column2Common.TotalCurrency = 0;
    local.Column3Common.TotalCurrency = 0;

    foreach(var item in ReadDisbursementTransaction())
    {
      local.Column1Common.TotalCurrency += entities.DisbursementTransaction.
        Amount;
    }

    foreach(var item in ReadDisbursementTransaction())
    {
      local.Column2Common.TotalCurrency += entities.DisbursementTransaction.
        Amount;
    }

    local.Column3Common.TotalCurrency = local.Column1Common.TotalCurrency - local
      .Column2Common.TotalCurrency;
    local.Column1Total.TotalCurrency += local.Column1Common.TotalCurrency;
    local.Column2Total.TotalCurrency += local.Column2Common.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Common.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = " Cost Recovery Fees";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Column3Total.TotalCurrency = local.Column1Total.TotalCurrency - local
      .Column2Total.TotalCurrency;
    local.Supply.TotalCurrency = local.Column1Total.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column1WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column2Total.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column2WorkArea.Text20 = local.RetCurrency.Text20;
    local.Supply.TotalCurrency = local.Column3Total.TotalCurrency;
    UseFnCabCurrencyToTextLarge();
    local.Column3WorkArea.Text20 = local.RetCurrency.Text20;
    local.ItemName.Text37 = "";
    local.Column4.Text20 = "";
    local.EabReportSend.RptDetail = local.ItemName.Text37 + local
      .Column1WorkArea.Text20 + "     " + local.Column2WorkArea.Text20 + "     " +
      local.Column3WorkArea.Text20 + "     " + local.Column4.Text20;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      " 1. Debt Due Amounts can potentially include Voluntary Obligations types.";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      " 2. Collection Amounts can potentially include Voluntary & Gift payments.";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      " 3. Cost Recovery Fees are the net amount for the FY, this includes any amounts w/in FY.";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseEabExternalReportWriter();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabReportSend.RptDetail = "Error encountered closing report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExternalReportWriter()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.EabReportSend.RptDetail;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseFnB738Housekeeping()
  {
    var useImport = new FnB738Housekeeping.Import();
    var useExport = new FnB738Housekeeping.Export();

    Call(FnB738Housekeeping.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseFnCabCurrencyToTextLarge()
  {
    var useImport = new FnCabCurrencyToTextLarge.Import();
    var useExport = new FnCabCurrencyToTextLarge.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    MoveCommon(local.Supply, useImport.Common);

    Call(FnCabCurrencyToTextLarge.Execute, useImport, useExport);

    local.RetCurrency.Assign(useExport.WorkArea);
    local.ReturnCurrency.Text10 = useExport.TextWorkArea.Text10;
    local.ReturnCode.Text10 = useExport.ReturnCode.Text10;
  }

  private IEnumerable<bool> ReadCollection1()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection10()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection10",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection11()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection11",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection12()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection12",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection13()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection13",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection14()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection14",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection15()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection15",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection16()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection16",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection17()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection17",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp", local.BaseDtFrom.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection18()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection18",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.BaseDtTo.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection5()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection5",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection6()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection6",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection7()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection7",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection8()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection8",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection9()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection9",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionDebtDetail1()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDetail1",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst", local.Currrent.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 17);
        entities.DebtDetail.CspNumber = db.GetString(reader, 18);
        entities.DebtDetail.CpaType = db.GetString(reader, 19);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 20);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 21);
        entities.DebtDetail.OtrType = db.GetString(reader, 22);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 23);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionDebtDetail2()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDetail2",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", local.EndOfYear.Date.GetValueOrDefault());
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst", local.Currrent.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 17);
        entities.DebtDetail.CspNumber = db.GetString(reader, 18);
        entities.DebtDetail.CpaType = db.GetString(reader, 19);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 20);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 21);
        entities.DebtDetail.OtrType = db.GetString(reader, 22);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 23);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionDebtDetail3()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDetail3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst", local.Currrent.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 17);
        entities.DebtDetail.CspNumber = db.GetString(reader, 18);
        entities.DebtDetail.CpaType = db.GetString(reader, 19);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 20);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 21);
        entities.DebtDetail.OtrType = db.GetString(reader, 22);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 23);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionDebtDetail4()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDetail4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst", local.Currrent.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 17);
        entities.DebtDetail.CspNumber = db.GetString(reader, 18);
        entities.DebtDetail.CpaType = db.GetString(reader, 19);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 20);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 21);
        entities.DebtDetail.OtrType = db.GetString(reader, 22);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 23);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 24);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail1()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail1",
      null,
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail2()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail2",
      null,
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.BaseDtFrom.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.BaseDtTo.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

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
    /// <summary>A Group1Group group.</summary>
    [Serializable]
    public class Group1Group
    {
      /// <summary>
      /// A value of Grp1H.
      /// </summary>
      [JsonPropertyName("grp1H")]
      public Common Grp1H
      {
        get => grp1H ??= new();
        set => grp1H = value;
      }

      /// <summary>
      /// A value of Grp1F.
      /// </summary>
      [JsonPropertyName("grp1F")]
      public Common Grp1F
      {
        get => grp1F ??= new();
        set => grp1F = value;
      }

      /// <summary>
      /// A value of Grp1D.
      /// </summary>
      [JsonPropertyName("grp1D")]
      public Common Grp1D
      {
        get => grp1D ??= new();
        set => grp1D = value;
      }

      /// <summary>
      /// A value of Grp1B.
      /// </summary>
      [JsonPropertyName("grp1B")]
      public Common Grp1B
      {
        get => grp1B ??= new();
        set => grp1B = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 22;

      private Common grp1H;
      private Common grp1F;
      private Common grp1D;
      private Common grp1B;
    }

    /// <summary>
    /// A value of Spaces.
    /// </summary>
    [JsonPropertyName("spaces")]
    public WorkArea Spaces
    {
      get => spaces ??= new();
      set => spaces = value;
    }

    /// <summary>
    /// A value of StartPostion.
    /// </summary>
    [JsonPropertyName("startPostion")]
    public Common StartPostion
    {
      get => startPostion ??= new();
      set => startPostion = value;
    }

    /// <summary>
    /// A value of Column3Total.
    /// </summary>
    [JsonPropertyName("column3Total")]
    public Common Column3Total
    {
      get => column3Total ??= new();
      set => column3Total = value;
    }

    /// <summary>
    /// A value of Column2Total.
    /// </summary>
    [JsonPropertyName("column2Total")]
    public Common Column2Total
    {
      get => column2Total ??= new();
      set => column2Total = value;
    }

    /// <summary>
    /// A value of Column1Total.
    /// </summary>
    [JsonPropertyName("column1Total")]
    public Common Column1Total
    {
      get => column1Total ??= new();
      set => column1Total = value;
    }

    /// <summary>
    /// A value of Column3Common.
    /// </summary>
    [JsonPropertyName("column3Common")]
    public Common Column3Common
    {
      get => column3Common ??= new();
      set => column3Common = value;
    }

    /// <summary>
    /// A value of Column2Common.
    /// </summary>
    [JsonPropertyName("column2Common")]
    public Common Column2Common
    {
      get => column2Common ??= new();
      set => column2Common = value;
    }

    /// <summary>
    /// A value of Column1Common.
    /// </summary>
    [JsonPropertyName("column1Common")]
    public Common Column1Common
    {
      get => column1Common ??= new();
      set => column1Common = value;
    }

    /// <summary>
    /// A value of Column4.
    /// </summary>
    [JsonPropertyName("column4")]
    public WorkArea Column4
    {
      get => column4 ??= new();
      set => column4 = value;
    }

    /// <summary>
    /// A value of Column3WorkArea.
    /// </summary>
    [JsonPropertyName("column3WorkArea")]
    public WorkArea Column3WorkArea
    {
      get => column3WorkArea ??= new();
      set => column3WorkArea = value;
    }

    /// <summary>
    /// A value of Column2WorkArea.
    /// </summary>
    [JsonPropertyName("column2WorkArea")]
    public WorkArea Column2WorkArea
    {
      get => column2WorkArea ??= new();
      set => column2WorkArea = value;
    }

    /// <summary>
    /// A value of Column1WorkArea.
    /// </summary>
    [JsonPropertyName("column1WorkArea")]
    public WorkArea Column1WorkArea
    {
      get => column1WorkArea ??= new();
      set => column1WorkArea = value;
    }

    /// <summary>
    /// A value of ItemName.
    /// </summary>
    [JsonPropertyName("itemName")]
    public WorkArea ItemName
    {
      get => itemName ??= new();
      set => itemName = value;
    }

    /// <summary>
    /// A value of RetCurrency.
    /// </summary>
    [JsonPropertyName("retCurrency")]
    public WorkArea RetCurrency
    {
      get => retCurrency ??= new();
      set => retCurrency = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public TextWorkArea ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of ReturnCurrency.
    /// </summary>
    [JsonPropertyName("returnCurrency")]
    public TextWorkArea ReturnCurrency
    {
      get => returnCurrency ??= new();
      set => returnCurrency = value;
    }

    /// <summary>
    /// A value of Supply.
    /// </summary>
    [JsonPropertyName("supply")]
    public Common Supply
    {
      get => supply ??= new();
      set => supply = value;
    }

    /// <summary>
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
    }

    /// <summary>
    /// A value of EndOfYear.
    /// </summary>
    [JsonPropertyName("endOfYear")]
    public DateWorkArea EndOfYear
    {
      get => endOfYear ??= new();
      set => endOfYear = value;
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
    /// A value of Currrent.
    /// </summary>
    [JsonPropertyName("currrent")]
    public DateWorkArea Currrent
    {
      get => currrent ??= new();
      set => currrent = value;
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
    /// A value of Line11From.
    /// </summary>
    [JsonPropertyName("line11From")]
    public DateWorkArea Line11From
    {
      get => line11From ??= new();
      set => line11From = value;
    }

    /// <summary>
    /// A value of Line11To.
    /// </summary>
    [JsonPropertyName("line11To")]
    public DateWorkArea Line11To
    {
      get => line11To ??= new();
      set => line11To = value;
    }

    /// <summary>
    /// A value of BaseDtFrom.
    /// </summary>
    [JsonPropertyName("baseDtFrom")]
    public DateWorkArea BaseDtFrom
    {
      get => baseDtFrom ??= new();
      set => baseDtFrom = value;
    }

    /// <summary>
    /// A value of BaseDtTo.
    /// </summary>
    [JsonPropertyName("baseDtTo")]
    public DateWorkArea BaseDtTo
    {
      get => baseDtTo ??= new();
      set => baseDtTo = value;
    }

    /// <summary>
    /// A value of Line10From.
    /// </summary>
    [JsonPropertyName("line10From")]
    public DateWorkArea Line10From
    {
      get => line10From ??= new();
      set => line10From = value;
    }

    /// <summary>
    /// A value of Line10To.
    /// </summary>
    [JsonPropertyName("line10To")]
    public DateWorkArea Line10To
    {
      get => line10To ??= new();
      set => line10To = value;
    }

    /// <summary>
    /// A value of Line9From.
    /// </summary>
    [JsonPropertyName("line9From")]
    public DateWorkArea Line9From
    {
      get => line9From ??= new();
      set => line9From = value;
    }

    /// <summary>
    /// A value of Line9To.
    /// </summary>
    [JsonPropertyName("line9To")]
    public DateWorkArea Line9To
    {
      get => line9To ??= new();
      set => line9To = value;
    }

    /// <summary>
    /// A value of Line8From.
    /// </summary>
    [JsonPropertyName("line8From")]
    public DateWorkArea Line8From
    {
      get => line8From ??= new();
      set => line8From = value;
    }

    /// <summary>
    /// A value of Line8To.
    /// </summary>
    [JsonPropertyName("line8To")]
    public DateWorkArea Line8To
    {
      get => line8To ??= new();
      set => line8To = value;
    }

    /// <summary>
    /// A value of Line7From.
    /// </summary>
    [JsonPropertyName("line7From")]
    public DateWorkArea Line7From
    {
      get => line7From ??= new();
      set => line7From = value;
    }

    /// <summary>
    /// A value of Line7To.
    /// </summary>
    [JsonPropertyName("line7To")]
    public DateWorkArea Line7To
    {
      get => line7To ??= new();
      set => line7To = value;
    }

    /// <summary>
    /// A value of ConvertFrom.
    /// </summary>
    [JsonPropertyName("convertFrom")]
    public BatchTimestampWorkArea ConvertFrom
    {
      get => convertFrom ??= new();
      set => convertFrom = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of ConvertTo.
    /// </summary>
    [JsonPropertyName("convertTo")]
    public BatchTimestampWorkArea ConvertTo
    {
      get => convertTo ??= new();
      set => convertTo = value;
    }

    /// <summary>
    /// A value of Line6From.
    /// </summary>
    [JsonPropertyName("line6From")]
    public DateWorkArea Line6From
    {
      get => line6From ??= new();
      set => line6From = value;
    }

    /// <summary>
    /// A value of Line6To.
    /// </summary>
    [JsonPropertyName("line6To")]
    public DateWorkArea Line6To
    {
      get => line6To ??= new();
      set => line6To = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public External Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public TextWorkArea Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of StatusDate.
    /// </summary>
    [JsonPropertyName("statusDate")]
    public WorkArea StatusDate
    {
      get => statusDate ??= new();
      set => statusDate = value;
    }

    /// <summary>
    /// A value of EffDate.
    /// </summary>
    [JsonPropertyName("effDate")]
    public WorkArea EffDate
    {
      get => effDate ??= new();
      set => effDate = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    private WorkArea spaces;
    private Common startPostion;
    private Common column3Total;
    private Common column2Total;
    private Common column1Total;
    private Common column3Common;
    private Common column2Common;
    private Common column1Common;
    private WorkArea column4;
    private WorkArea column3WorkArea;
    private WorkArea column2WorkArea;
    private WorkArea column1WorkArea;
    private WorkArea itemName;
    private WorkArea retCurrency;
    private TextWorkArea returnCode;
    private TextWorkArea returnCurrency;
    private Common supply;
    private Common commasRequired;
    private DateWorkArea endOfYear;
    private DateWorkArea null1;
    private DateWorkArea currrent;
    private Array<Group1Group> group1;
    private DateWorkArea line11From;
    private DateWorkArea line11To;
    private DateWorkArea baseDtFrom;
    private DateWorkArea baseDtTo;
    private DateWorkArea line10From;
    private DateWorkArea line10To;
    private DateWorkArea line9From;
    private DateWorkArea line9To;
    private DateWorkArea line8From;
    private DateWorkArea line8To;
    private DateWorkArea line7From;
    private DateWorkArea line7To;
    private BatchTimestampWorkArea convertFrom;
    private DateWorkArea convert;
    private BatchTimestampWorkArea convertTo;
    private DateWorkArea line6From;
    private DateWorkArea line6To;
    private External write;
    private External external;
    private TextWorkArea case1;
    private WorkArea statusDate;
    private WorkArea effDate;
    private AbendData abendData;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private DisbursementType disbursementType;
    private DisbursementTransaction disbursementTransaction;
    private Collection collection;
    private Obligation obligation;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
  }
#endregion
}
