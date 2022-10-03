// Program: LE_B590_OUTGOING_FIPS_UPDATE, ID: 374351731, model: 746.
// Short name: SWEL590B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B590_OUTGOING_FIPS_UPDATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB590OutgoingFipsUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B590_OUTGOING_FIPS_UPDATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB590OutgoingFipsUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB590OutgoingFipsUpdate.
  /// </summary>
  public LeB590OutgoingFipsUpdate(IContext context, Import import, Export export)
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 03/09/2000	M Ramirez	WR 163		Initial Development
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseLeB590Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    foreach(var item in ReadTrigger())
    {
      local.Fips.State = entities.Trigger.DenormNumeric1.GetValueOrDefault();
      local.Fips.County = entities.Trigger.DenormNumeric2.GetValueOrDefault();
      local.Fips.Location = entities.Trigger.DenormNumeric3.GetValueOrDefault();

      if (local.Fips.State > local.Previous.State && local.Previous.State > 0)
      {
        if (!IsEmpty(local.DetailLine1.Text32))
        {
          local.EabReportSend.RptDetail = local.DetailLine1.Text32 + local
            .DetailLine2.Text32 + local.DetailLine3.Text32;
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabReportSend.RptDetail =
              "ABEND:  Writing to report 01; TRIGGER = " + local
              .Trigger.Text9 + "; FIPS = " + local.FipsState.Text3 + local
              .FipsCounty.Text4 + local.FipsLocation.Text3;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ABEND_4_BATCH";

            return;
          }

          local.EabReportSend.RptDetail = "";
          local.DetailLine1.Text32 = "";
          local.DetailLine2.Text32 = "";
          local.DetailLine3.Text32 = "";
        }

        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric1.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsState.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabConvertNumeric.SendNonSuppressPos = 1;
        local.EabConvertNumeric.SendAmount =
          NumberToString(local.TriggersProcessed.Count, 15);
        UseEabConvertNumeric1();
        local.FooterLine.Text37 = local.FipsState.Text3 + local
          .Previous.StateAbbreviation + " " + (
            local.Previous.StateDescription ?? "");
        local.FooterLine.Text9 = "TOTAL";
        local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
          .FooterLine.Text9 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
            local.FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString(local.AddTriggers.Count, 15);
        UseEabConvertNumeric1();
        local.FooterLine.Text37 = "";
        local.FooterLine.Text9 = "ADDED";
        local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
          .FooterLine.Text9 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
            local.FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString(local.UpdateTriggers.Count, 15);
        UseEabConvertNumeric1();
        local.FooterLine.Text37 = "";
        local.FooterLine.Text9 = "UPDATED";
        local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
          .FooterLine.Text9 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
            local.FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        local.EabConvertNumeric.SendAmount =
          NumberToString(local.DeleteTriggers.Count, 15);
        UseEabConvertNumeric1();
        local.FooterLine.Text37 = "";
        local.FooterLine.Text9 = "DELETED";
        local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
          .FooterLine.Text9 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
            local.FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        local.EabReportSend.RptDetail = "";
        local.TriggersRead.Count = 0;
        local.TriggersProcessed.Count = 0;
        local.TriggersErred.Count = 0;
        local.AddTriggers.Count = 0;
        local.UpdateTriggers.Count = 0;
        local.DeleteTriggers.Count = 0;
      }

      ++local.TriggersRead.Count;
      ++local.ControlTotalRead.Count;

      if (Equal(entities.Trigger.Action, "ADD"))
      {
        local.LeAutoFipsUpdate.ActionCode = "A";
      }
      else if (Equal(entities.Trigger.Action, "UPDATE"))
      {
        local.LeAutoFipsUpdate.ActionCode = "C";
      }
      else
      {
        ++local.TriggersErred.Count;
        ++local.ControlTotalErred.Count;
        local.EabConvertNumeric.SendNonSuppressPos = 9;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.Identifier, 15);
        UseEabConvertNumeric1();
        local.Trigger.Text9 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric1.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsState.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric2.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsCounty.Text4 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric3.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsLocation.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabReportSend.RptDetail =
          "WARNING:  DELETE not sent for trigger; TRIGGER = " + local
          .Trigger.Text9 + "; FIPS = " + local.FipsState.Text3 + local
          .FipsCounty.Text4 + local.FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";

        continue;
      }

      if (ReadFips())
      {
        local.Previous.Assign(entities.Fips);
        local.LeAutoFipsUpdate.AddressType1 = "LOC";
        local.LeAutoFipsUpdate.AddressType2 = "RES";
        local.LeAutoFipsUpdate.StateCode = entities.Fips.State;
        local.LeAutoFipsUpdate.LocalCode = entities.Fips.County;
        local.LeAutoFipsUpdate.SubLocalCode = entities.Fips.Location;
        local.LeAutoFipsUpdate.DepartmentName =
          entities.Fips.CountyDescription ?? Spaces(35);
        local.LeAutoFipsUpdate.RecordDate =
          DateToInt(Date(entities.Trigger.CreatedTimestamp));

        if (ReadTribunal())
        {
          local.LeAutoFipsUpdate.Title = entities.Tribunal.Name;
        }

        if (ReadFipsTribAddress())
        {
          local.LeAutoFipsUpdate.Street1 = entities.FipsTribAddress.Street1;
          local.LeAutoFipsUpdate.Street2 = entities.FipsTribAddress.Street2 ?? Spaces
            (35);
          local.LeAutoFipsUpdate.City = entities.FipsTribAddress.City;
          local.LeAutoFipsUpdate.StateOrCountry =
            entities.FipsTribAddress.State;
          local.LeAutoFipsUpdate.ZipCode =
            (int)(StringToNumber(entities.FipsTribAddress.ZipCode) * (
              decimal)10000 + StringToNumber(entities.FipsTribAddress.Zip4));
          local.LeAutoFipsUpdate.AreaCode =
            entities.FipsTribAddress.AreaCode.GetValueOrDefault();
          local.LeAutoFipsUpdate.PhoneNumber =
            entities.FipsTribAddress.PhoneNumber.GetValueOrDefault();
          local.LeAutoFipsUpdate.FaxAreaCode =
            entities.FipsTribAddress.FaxAreaCode.GetValueOrDefault();
          local.LeAutoFipsUpdate.FaxNumber =
            entities.FipsTribAddress.FaxNumber.GetValueOrDefault();
        }
      }
      else
      {
        ++local.TriggersErred.Count;
        ++local.ControlTotalErred.Count;
        local.EabConvertNumeric.SendNonSuppressPos = 9;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.Identifier, 15);
        UseEabConvertNumeric1();
        local.Trigger.Text9 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric1.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsState.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric2.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsCounty.Text4 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric3.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsLocation.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabReportSend.RptDetail =
          "ERROR:  FIPS not found for trigger; TRIGGER = " + local
          .Trigger.Text9 + "; FIPS = " + local.FipsState.Text3 + local
          .FipsCounty.Text4 + local.FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";

        continue;
      }

      UseLeEabWriteFipsUpdate();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabConvertNumeric.SendNonSuppressPos = 9;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.Identifier, 15);
        UseEabConvertNumeric1();
        local.Trigger.Text9 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric1.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsState.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric2.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsCounty.Text4 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
        local.EabConvertNumeric.SendNonSuppressPos = 2;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.DenormNumeric3.GetValueOrDefault(), 15);
          
        UseEabConvertNumeric1();
        local.FipsLocation.Text3 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
        local.EabReportSend.RptDetail =
          "ABEND:  Writing to file; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
          local.FipsState.Text3 + local.FipsCounty.Text4 + local
          .FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      ++local.TriggersProcessed.Count;
      ++local.ControlTotalProcessed.Count;

      if (Equal(entities.Trigger.Action, "ADD"))
      {
        ++local.AddTriggers.Count;
        ++local.ControlTotalAdded.Count;
      }
      else if (Equal(entities.Trigger.Action, "UPDATE"))
      {
        ++local.UpdateTriggers.Count;
        ++local.ControlTotalUpdated.Count;
      }
      else
      {
        ++local.DeleteTriggers.Count;
        ++local.ControlTotalDeleted.Count;
      }

      local.EabConvertNumeric.SendNonSuppressPos = 2;
      local.EabConvertNumeric.SendAmount =
        NumberToString(entities.Trigger.DenormNumeric1.GetValueOrDefault(), 15);
        
      UseEabConvertNumeric1();
      local.FipsState.Text3 =
        Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
      local.EabConvertNumeric.SendNonSuppressPos = 3;
      local.EabConvertNumeric.SendAmount =
        NumberToString(entities.Trigger.DenormNumeric2.GetValueOrDefault(), 15);
        
      UseEabConvertNumeric1();
      local.FipsCounty.Text4 =
        Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
      local.EabConvertNumeric.SendNonSuppressPos = 2;
      local.EabConvertNumeric.SendAmount =
        NumberToString(entities.Trigger.DenormNumeric3.GetValueOrDefault(), 15);
        
      UseEabConvertNumeric1();
      local.FipsLocation.Text3 =
        Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
      local.DetailLine3.Text32 = local.FipsState.Text3 + local
        .FipsCounty.Text4 + local.FipsLocation.Text3 + "  " + entities
        .Trigger.Action;

      if (IsEmpty(local.DetailLine1.Text32))
      {
        local.DetailLine1.Text32 = local.DetailLine3.Text32;
      }
      else if (IsEmpty(local.DetailLine2.Text32))
      {
        local.DetailLine2.Text32 = local.DetailLine3.Text32;
      }
      else
      {
        local.EabReportSend.RptDetail = local.DetailLine1.Text32 + local
          .DetailLine2.Text32 + local.DetailLine3.Text32;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
            local.FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        local.EabReportSend.RptDetail = "";
        local.DetailLine1.Text32 = "";
        local.DetailLine2.Text32 = "";
      }

      local.DetailLine3.Text32 = "";

      // mjr
      // -------------------------------------------------
      // End READ EACH
      // ----------------------------------------------------
    }

    // mjr
    // -------------------------------------------------
    // Finish off report
    // ----------------------------------------------------
    if (local.Previous.State > 0)
    {
      if (!IsEmpty(local.DetailLine1.Text32))
      {
        local.EabReportSend.RptDetail = local.DetailLine1.Text32 + local
          .DetailLine2.Text32 + local.DetailLine3.Text32;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
            local.FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        local.EabReportSend.RptDetail = "";
        local.DetailLine1.Text32 = "";
        local.DetailLine2.Text32 = "";
        local.DetailLine3.Text32 = "";
      }

      local.EabConvertNumeric.SendNonSuppressPos = 2;
      local.EabConvertNumeric.SendAmount =
        NumberToString(entities.Trigger.DenormNumeric1.GetValueOrDefault(), 15);
        
      UseEabConvertNumeric1();
      local.FipsState.Text3 =
        Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
      local.EabConvertNumeric.SendNonSuppressPos = 1;
      local.EabConvertNumeric.SendAmount =
        NumberToString(local.TriggersProcessed.Count, 15);
      UseEabConvertNumeric1();
      local.FooterLine.Text37 = local.FipsState.Text3 + local
        .Previous.StateAbbreviation + " " + (
          local.Previous.StateDescription ?? "");
      local.FooterLine.Text9 = "TOTAL";
      local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
        .FooterLine.Text9 + local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
          local.FipsState.Text3 + local.FipsCounty.Text4 + local
          .FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      local.EabConvertNumeric.SendAmount =
        NumberToString(local.AddTriggers.Count, 15);
      UseEabConvertNumeric1();
      local.FooterLine.Text37 = "";
      local.FooterLine.Text9 = "ADDED";
      local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
        .FooterLine.Text9 + local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
          local.FipsState.Text3 + local.FipsCounty.Text4 + local
          .FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      local.EabConvertNumeric.SendAmount =
        NumberToString(local.UpdateTriggers.Count, 15);
      UseEabConvertNumeric1();
      local.FooterLine.Text37 = "";
      local.FooterLine.Text9 = "UPDATED";
      local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
        .FooterLine.Text9 + local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
          local.FipsState.Text3 + local.FipsCounty.Text4 + local
          .FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      local.EabConvertNumeric.SendAmount =
        NumberToString(local.DeleteTriggers.Count, 15);
      UseEabConvertNumeric1();
      local.FooterLine.Text37 = "";
      local.FooterLine.Text9 = "DELETED";
      local.EabReportSend.RptDetail = local.FooterLine.Text37 + local
        .FooterLine.Text9 + local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Writing to report 01; TRIGGER = " + local.Trigger.Text9 + "; FIPS = " +
          local.FipsState.Text3 + local.FipsCounty.Text4 + local
          .FipsLocation.Text3;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      local.EabReportSend.RptDetail = "";
    }

    // mjr
    // ---------------------------------------------------------
    // Update Checkpoint_Restart Last_Checkpoint_Timestamp.
    // ------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = 1;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Error updating checkpoint_restart";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ---------------------------------------------------------------
    // WRITE CONTROL TOTALS AND CLOSE REPORTS
    // ---------------------------------------------------------------
    UseLeB590WriteControlsAndClose();
  }

  private static void MoveEabConvertNumeric1(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
  }

  private static void MoveEabConvertNumeric3(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric3(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    MoveEabConvertNumeric1(local.EabConvertNumeric, useExport.EabConvertNumeric);
      

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric1(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseLeB590Housekeeping()
  {
    var useImport = new LeB590Housekeeping.Import();
    var useExport = new LeB590Housekeeping.Export();

    Call(LeB590Housekeeping.Execute, useImport, useExport);

    local.Parm.Assign(useExport.Trigger);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
  }

  private void UseLeB590WriteControlsAndClose()
  {
    var useImport = new LeB590WriteControlsAndClose.Import();
    var useExport = new LeB590WriteControlsAndClose.Export();

    useImport.Erred.Count = local.ControlTotalErred.Count;
    useImport.Read.Count = local.ControlTotalRead.Count;
    useImport.Added.Count = local.ControlTotalAdded.Count;
    useImport.Updated.Count = local.ControlTotalUpdated.Count;
    useImport.Deleted.Count = local.ControlTotalDeleted.Count;
    useImport.Processed.Count = local.ControlTotalProcessed.Count;

    Call(LeB590WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseLeEabWriteFipsUpdate()
  {
    var useImport = new LeEabWriteFipsUpdate.Import();
    var useExport = new LeEabWriteFipsUpdate.Export();

    useImport.LeAutoFipsUpdate.Assign(local.LeAutoFipsUpdate);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabWriteFipsUpdate.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.Fips.State);
        db.SetInt32(command, "county", local.Fips.County);
        db.SetInt32(command, "location", local.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.StateAbbreviation = db.GetString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 12);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 13);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 14);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 15);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 16);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTrigger()
  {
    entities.Trigger.Populated = false;

    return ReadEach("ReadTrigger",
      (db, command) =>
      {
        db.SetString(command, "type", local.Parm.Type1);
        db.SetNullableString(command, "status", local.Parm.Status ?? "");
        db.SetNullableDateTime(
          command, "createdTimestamp",
          local.Parm.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Trigger.Identifier = db.GetInt32(reader, 0);
        entities.Trigger.Type1 = db.GetString(reader, 1);
        entities.Trigger.Action = db.GetNullableString(reader, 2);
        entities.Trigger.Status = db.GetNullableString(reader, 3);
        entities.Trigger.DenormNumeric1 = db.GetNullableInt32(reader, 4);
        entities.Trigger.DenormNumeric2 = db.GetNullableInt32(reader, 5);
        entities.Trigger.DenormNumeric3 = db.GetNullableInt32(reader, 6);
        entities.Trigger.DenormText1 = db.GetNullableString(reader, 7);
        entities.Trigger.DenormText2 = db.GetNullableString(reader, 8);
        entities.Trigger.DenormText3 = db.GetNullableString(reader, 9);
        entities.Trigger.CreatedTimestamp = db.GetNullableDateTime(reader, 10);
        entities.Trigger.Populated = true;

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
    /// <summary>
    /// A value of FooterLine.
    /// </summary>
    [JsonPropertyName("footerLine")]
    public WorkArea FooterLine
    {
      get => footerLine ??= new();
      set => footerLine = value;
    }

    /// <summary>
    /// A value of LeAutoFipsUpdate.
    /// </summary>
    [JsonPropertyName("leAutoFipsUpdate")]
    public LeAutoFipsUpdate LeAutoFipsUpdate
    {
      get => leAutoFipsUpdate ??= new();
      set => leAutoFipsUpdate = value;
    }

    /// <summary>
    /// A value of DetailLine3.
    /// </summary>
    [JsonPropertyName("detailLine3")]
    public WorkArea DetailLine3
    {
      get => detailLine3 ??= new();
      set => detailLine3 = value;
    }

    /// <summary>
    /// A value of DetailLine2.
    /// </summary>
    [JsonPropertyName("detailLine2")]
    public WorkArea DetailLine2
    {
      get => detailLine2 ??= new();
      set => detailLine2 = value;
    }

    /// <summary>
    /// A value of DetailLine1.
    /// </summary>
    [JsonPropertyName("detailLine1")]
    public WorkArea DetailLine1
    {
      get => detailLine1 ??= new();
      set => detailLine1 = value;
    }

    /// <summary>
    /// A value of TriggersErred.
    /// </summary>
    [JsonPropertyName("triggersErred")]
    public Common TriggersErred
    {
      get => triggersErred ??= new();
      set => triggersErred = value;
    }

    /// <summary>
    /// A value of ControlTotalErred.
    /// </summary>
    [JsonPropertyName("controlTotalErred")]
    public Common ControlTotalErred
    {
      get => controlTotalErred ??= new();
      set => controlTotalErred = value;
    }

    /// <summary>
    /// A value of ControlTotalRead.
    /// </summary>
    [JsonPropertyName("controlTotalRead")]
    public Common ControlTotalRead
    {
      get => controlTotalRead ??= new();
      set => controlTotalRead = value;
    }

    /// <summary>
    /// A value of ControlTotalAdded.
    /// </summary>
    [JsonPropertyName("controlTotalAdded")]
    public Common ControlTotalAdded
    {
      get => controlTotalAdded ??= new();
      set => controlTotalAdded = value;
    }

    /// <summary>
    /// A value of ControlTotalUpdated.
    /// </summary>
    [JsonPropertyName("controlTotalUpdated")]
    public Common ControlTotalUpdated
    {
      get => controlTotalUpdated ??= new();
      set => controlTotalUpdated = value;
    }

    /// <summary>
    /// A value of ControlTotalDeleted.
    /// </summary>
    [JsonPropertyName("controlTotalDeleted")]
    public Common ControlTotalDeleted
    {
      get => controlTotalDeleted ??= new();
      set => controlTotalDeleted = value;
    }

    /// <summary>
    /// A value of ControlTotalProcessed.
    /// </summary>
    [JsonPropertyName("controlTotalProcessed")]
    public Common ControlTotalProcessed
    {
      get => controlTotalProcessed ??= new();
      set => controlTotalProcessed = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Fips Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public WorkArea Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    /// <summary>
    /// A value of FipsCounty.
    /// </summary>
    [JsonPropertyName("fipsCounty")]
    public WorkArea FipsCounty
    {
      get => fipsCounty ??= new();
      set => fipsCounty = value;
    }

    /// <summary>
    /// A value of FipsLocation.
    /// </summary>
    [JsonPropertyName("fipsLocation")]
    public WorkArea FipsLocation
    {
      get => fipsLocation ??= new();
      set => fipsLocation = value;
    }

    /// <summary>
    /// A value of FipsState.
    /// </summary>
    [JsonPropertyName("fipsState")]
    public WorkArea FipsState
    {
      get => fipsState ??= new();
      set => fipsState = value;
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
    /// A value of Parm.
    /// </summary>
    [JsonPropertyName("parm")]
    public Trigger Parm
    {
      get => parm ??= new();
      set => parm = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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
    /// A value of RowLock.
    /// </summary>
    [JsonPropertyName("rowLock")]
    public Common RowLock
    {
      get => rowLock ??= new();
      set => rowLock = value;
    }

    /// <summary>
    /// A value of TriggersRead.
    /// </summary>
    [JsonPropertyName("triggersRead")]
    public Common TriggersRead
    {
      get => triggersRead ??= new();
      set => triggersRead = value;
    }

    /// <summary>
    /// A value of AddTriggers.
    /// </summary>
    [JsonPropertyName("addTriggers")]
    public Common AddTriggers
    {
      get => addTriggers ??= new();
      set => addTriggers = value;
    }

    /// <summary>
    /// A value of UpdateTriggers.
    /// </summary>
    [JsonPropertyName("updateTriggers")]
    public Common UpdateTriggers
    {
      get => updateTriggers ??= new();
      set => updateTriggers = value;
    }

    /// <summary>
    /// A value of DeleteTriggers.
    /// </summary>
    [JsonPropertyName("deleteTriggers")]
    public Common DeleteTriggers
    {
      get => deleteTriggers ??= new();
      set => deleteTriggers = value;
    }

    /// <summary>
    /// A value of TriggersProcessed.
    /// </summary>
    [JsonPropertyName("triggersProcessed")]
    public Common TriggersProcessed
    {
      get => triggersProcessed ??= new();
      set => triggersProcessed = value;
    }

    private WorkArea footerLine;
    private LeAutoFipsUpdate leAutoFipsUpdate;
    private WorkArea detailLine3;
    private WorkArea detailLine2;
    private WorkArea detailLine1;
    private Common triggersErred;
    private Common controlTotalErred;
    private Common controlTotalRead;
    private Common controlTotalAdded;
    private Common controlTotalUpdated;
    private Common controlTotalDeleted;
    private Common controlTotalProcessed;
    private Fips previous;
    private WorkArea trigger;
    private WorkArea fipsCounty;
    private WorkArea fipsLocation;
    private WorkArea fipsState;
    private Fips fips;
    private Trigger parm;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common debugOn;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private Common rowLock;
    private Common triggersRead;
    private Common addTriggers;
    private Common updateTriggers;
    private Common deleteTriggers;
    private Common triggersProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private Trigger trigger;
  }
#endregion
}
