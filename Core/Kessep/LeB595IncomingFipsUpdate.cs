// Program: LE_B595_INCOMING_FIPS_UPDATE, ID: 374353865, model: 746.
// Short name: SWEL595B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B595_INCOMING_FIPS_UPDATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB595IncomingFipsUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B595_INCOMING_FIPS_UPDATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB595IncomingFipsUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB595IncomingFipsUpdate.
  /// </summary>
  public LeB595IncomingFipsUpdate(IContext context, Import import, Export export)
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
    // 10/19/2006	GVandy		PR292602	Remove 'with ur' from FIPS Read.
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseLeB595Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    do
    {
      local.LeAutoFipsUpdate.Assign(local.NullLeAutoFipsUpdate);
      local.Fips.Assign(local.NullFips);
      local.FipsTribAddress.PhoneExtension = "";
      local.FipsTribAddress.ZipCode = "";
      local.FipsTribAddress.Zip4 = "";
      local.Trigger.Text9 = "";
      local.EabFileHandling.Action = "READ";
      UseLeEabReadFipsUpdate();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
        ++local.ControlTotalRead.Count;
        local.Fips.State = local.LeAutoFipsUpdate.StateCode;
        local.Fips.County = local.LeAutoFipsUpdate.LocalCode;
        local.Fips.Location = local.LeAutoFipsUpdate.SubLocalCode;

        if (local.Fips.State != local.Previous.State && local.Previous.State > 0
          )
        {
          if (local.Previous.State == local.Kansas.State)
          {
            // mjr
            // ----------------------------------------------
            // 04/07/2000
            // Don't write KS to Data Report
            // -----------------------------------------------------------
            goto Test1;
          }

          local.EabFileHandling.Action = "WRITE";

          if (!IsEmpty(local.DetailLine1.Text32))
          {
            local.EabReportSend.RptDetail = local.DetailLine1.Text32 + local
              .DetailLine2.Text32 + local.DetailLine3.Text32;
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabReportSend.RptDetail =
                "ABEND:  Writing to Report 01;  FIPS = " + local
                .FipsState.Text3 + local.FipsCounty.Text4 + local
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
            NumberToString(local.Previous.State, 15);
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
              "ABEND:  Writing to Report 01;  FIPS = " + local
              .FipsState.Text3 + local.FipsCounty.Text4 + local
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
              "ABEND:  Writing to Report 01;  FIPS = " + local
              .FipsState.Text3 + local.FipsCounty.Text4 + local
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
              "ABEND:  Writing to Report 01;  FIPS = " + local
              .FipsState.Text3 + local.FipsCounty.Text4 + local
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
              "ABEND:  Writing to Report 01;  FIPS = " + local
              .FipsState.Text3 + local.FipsCounty.Text4 + local
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
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabReportSend.RptDetail =
              "ABEND:  Writing to Report 01;  FIPS = " + local
              .FipsState.Text3 + local.FipsCounty.Text4 + local
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

          local.TriggersRead.Count = 0;
          local.TriggersProcessed.Count = 0;
          local.TriggersErred.Count = 0;
          local.TriggersWarned.Count = 0;
          local.AddTriggers.Count = 0;
          local.UpdateTriggers.Count = 0;
          local.DeleteTriggers.Count = 0;
          local.Previous.Assign(local.Fips);
        }

Test1:

        if (local.Fips.State == local.Kansas.State)
        {
          // --------------------------------------------------------------
          // Skip all Kansas records
          // --------------------------------------------------------------
          ++local.ControlTotalSkipped.Count;
        }
        else if (!Equal(local.LeAutoFipsUpdate.AddressType1, "LOC"))
        {
          ++local.TriggersErred.Count;
          ++local.ControlTotalErred.Count;
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.State, 15);
          UseEabConvertNumeric1();
          local.FipsState.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.County, 15);
          UseEabConvertNumeric1();
          local.FipsCounty.Text4 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
            
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.Location, 15);
          UseEabConvertNumeric1();
          local.FipsLocation.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabReportSend.RptDetail =
            "ERROR:  Invalid ADDRESS_TYPE_1; ADDRESS_TYPE_1 = " + local
            .LeAutoFipsUpdate.AddressType1 + "; FIPS = " + local
            .FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
        else if (!Equal(local.LeAutoFipsUpdate.AddressType2, "RES"))
        {
          ++local.TriggersErred.Count;
          ++local.ControlTotalErred.Count;
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.State, 15);
          UseEabConvertNumeric1();
          local.FipsState.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.County, 15);
          UseEabConvertNumeric1();
          local.FipsCounty.Text4 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
            
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.Location, 15);
          UseEabConvertNumeric1();
          local.FipsLocation.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabReportSend.RptDetail =
            "ERROR:  Invalid ADDRESS_TYPE_2; ADDRESS_TYPE_2 = " + local
            .LeAutoFipsUpdate.AddressType2 + "; FIPS = " + local
            .FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
        else if (AsChar(local.LeAutoFipsUpdate.ActionCode) != 'A' && AsChar
          (local.LeAutoFipsUpdate.ActionCode) != 'C')
        {
          ++local.TriggersErred.Count;
          ++local.ControlTotalErred.Count;
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.State, 15);
          UseEabConvertNumeric1();
          local.FipsState.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.County, 15);
          UseEabConvertNumeric1();
          local.FipsCounty.Text4 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
            
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.Location, 15);
          UseEabConvertNumeric1();
          local.FipsLocation.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabReportSend.RptDetail =
            "ERROR:  Invalid ACTION_CODE; ACTION_CODE = " + local
            .LeAutoFipsUpdate.ActionCode + "; FIPS = " + local
            .FipsState.Text3 + local.FipsCounty.Text4 + local
            .FipsLocation.Text3;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
        else
        {
          // DETERMINE LOCATION DESCRIPTION
          if (!IsEmpty(local.LeAutoFipsUpdate.DepartmentName))
          {
            local.Fips.LocationDescription =
              local.LeAutoFipsUpdate.DepartmentName;
          }
          else if (!IsEmpty(local.LeAutoFipsUpdate.City))
          {
            local.Fips.LocationDescription = local.LeAutoFipsUpdate.City;
          }
          else
          {
            ++local.TriggersErred.Count;
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.State, 15);
            UseEabConvertNumeric1();
            local.FipsState.Text3 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14,
              2);
            local.EabConvertNumeric.SendNonSuppressPos = 3;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.County, 15);
            UseEabConvertNumeric1();
            local.FipsCounty.Text4 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13,
              3);
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.Location, 15);
            UseEabConvertNumeric1();
            local.FipsLocation.Text3 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14,
              2);
            local.EabReportSend.RptDetail =
              "ERROR: Creating / Updating FIPS - Location description not found; FIPS = " +
              local.FipsState.Text3 + local.FipsCounty.Text4 + local
              .FipsLocation.Text3;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          if (ReadFips4())
          {
            // UPDATE FIPS LOCATION DESCRIPTION
            try
            {
              UpdateFips();
              local.Previous.Assign(entities.Fips);
              local.Trigger.Text9 = "UPDATE";
              ++local.UpdateTriggers.Count;
              ++local.ControlTotalUpdated.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ++local.TriggersErred.Count;
                  ++local.ControlTotalErred.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "ERROR: Updating FIPS - Not Unique; FIPS = " + local
                    .FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";

                  continue;
                case ErrorCode.PermittedValueViolation:
                  ++local.TriggersErred.Count;
                  ++local.ControlTotalErred.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "ERROR: Updating FIPS - Permitted Value Violation; FIPS = " +
                    local.FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";

                  continue;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            if (ReadFips2())
            {
              // ADD FIPS USING EXISTING FIPS COUNTY
              try
              {
                CreateFips1();
                local.Previous.Assign(entities.New1);
                local.Trigger.Text9 = "ADD";
                ++local.AddTriggers.Count;
                ++local.ControlTotalAdded.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Creating FIPS - Already Exists; FIPS = " + local
                      .FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Creating FIPS - Permitted Value Violation; FIPS = " +
                      local.FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    continue;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              goto Read;
            }

            // ADD FIPS AND TRIBUNAL
            // DETERMINE STATE ABBREVIATION AND DESCRIPTION
            if (ReadFips3())
            {
              local.Fips.StateAbbreviation = entities.Fips.StateAbbreviation;
              local.Fips.StateDescription = entities.Fips.StateDescription;
            }

            if (IsEmpty(local.Fips.StateAbbreviation))
            {
              ++local.TriggersErred.Count;
              ++local.ControlTotalErred.Count;
              local.EabConvertNumeric.SendNonSuppressPos = 2;
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Fips.State, 15);
              UseEabConvertNumeric1();
              local.FipsState.Text3 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
                14, 2);
              local.EabConvertNumeric.SendNonSuppressPos = 3;
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Fips.County, 15);
              UseEabConvertNumeric1();
              local.FipsCounty.Text4 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
                13, 3);
              local.EabConvertNumeric.SendNonSuppressPos = 2;
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Fips.Location, 15);
              UseEabConvertNumeric1();
              local.FipsLocation.Text3 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
                14, 2);
              local.EabReportSend.RptDetail =
                "ERROR: Creating FIPS - State abbreviation not found; FIPS = " +
                local.FipsState.Text3 + local.FipsCounty.Text4 + local
                .FipsLocation.Text3;
              local.EabFileHandling.Action = "WRITE";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";

              continue;
            }

            if (IsEmpty(local.Fips.StateDescription))
            {
              ++local.TriggersErred.Count;
              ++local.ControlTotalErred.Count;
              local.EabConvertNumeric.SendNonSuppressPos = 2;
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Fips.State, 15);
              UseEabConvertNumeric1();
              local.FipsState.Text3 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
                14, 2);
              local.EabConvertNumeric.SendNonSuppressPos = 3;
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Fips.County, 15);
              UseEabConvertNumeric1();
              local.FipsCounty.Text4 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
                13, 3);
              local.EabConvertNumeric.SendNonSuppressPos = 2;
              local.EabConvertNumeric.SendAmount =
                NumberToString(local.Fips.Location, 15);
              UseEabConvertNumeric1();
              local.FipsLocation.Text3 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
                14, 2);
              local.EabReportSend.RptDetail =
                "ERROR: Creating FIPS - State description not found; FIPS = " +
                local.FipsState.Text3 + local.FipsCounty.Text4 + local
                .FipsLocation.Text3;
              local.EabFileHandling.Action = "WRITE";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";

              continue;
            }

            // DETERMINE COUNTY DESCRIPTION
            local.Fips.CountyDescription =
              NumberToString(local.LeAutoFipsUpdate.LocalCode, 13, 3);

            // DETERMINE COUNTY ABBREVIATION
            local.Fips.CountyAbbreviation = "";
            local.CountyAbbrevFirstChar.Flag =
              Substring(local.Fips.LocationDescription, 1, 1);
            local.CountyAbbrevNext.CountyAbbreviation =
              local.CountyAbbrevFirstChar.Flag + "1";

            do
            {
              if (ReadFips1())
              {
                local.TempCommon.Flag =
                  Substring(entities.Fips.CountyAbbreviation, 2, 1);

                switch(AsChar(local.TempCommon.Flag))
                {
                  case '1':
                    local.TempCommon.Flag = "2";

                    break;
                  case '2':
                    local.TempCommon.Flag = "3";

                    break;
                  case '3':
                    local.TempCommon.Flag = "4";

                    break;
                  case '4':
                    local.TempCommon.Flag = "5";

                    break;
                  case '5':
                    local.TempCommon.Flag = "6";

                    break;
                  case '6':
                    local.TempCommon.Flag = "7";

                    break;
                  case '7':
                    local.TempCommon.Flag = "8";

                    break;
                  case '8':
                    local.TempCommon.Flag = "9";

                    break;
                  case '9':
                    local.TempCommon.Flag = "A";

                    break;
                  case 'A':
                    local.TempCommon.Flag = "B";

                    break;
                  case 'B':
                    local.TempCommon.Flag = "C";

                    break;
                  case 'C':
                    local.TempCommon.Flag = "D";

                    break;
                  case 'D':
                    local.TempCommon.Flag = "E";

                    break;
                  case 'E':
                    local.TempCommon.Flag = "F";

                    break;
                  case 'F':
                    local.TempCommon.Flag = "G";

                    break;
                  case 'G':
                    local.TempCommon.Flag = "H";

                    break;
                  case 'H':
                    local.TempCommon.Flag = "I";

                    break;
                  case 'I':
                    local.TempCommon.Flag = "J";

                    break;
                  case 'J':
                    local.TempCommon.Flag = "K";

                    break;
                  case 'K':
                    local.TempCommon.Flag = "L";

                    break;
                  case 'L':
                    local.TempCommon.Flag = "M";

                    break;
                  case 'M':
                    local.TempCommon.Flag = "N";

                    break;
                  case 'N':
                    local.TempCommon.Flag = "O";

                    break;
                  case 'O':
                    local.TempCommon.Flag = "P";

                    break;
                  case 'P':
                    local.TempCommon.Flag = "Q";

                    break;
                  case 'Q':
                    local.TempCommon.Flag = "R";

                    break;
                  case 'R':
                    local.TempCommon.Flag = "S";

                    break;
                  case 'S':
                    local.TempCommon.Flag = "T";

                    break;
                  case 'T':
                    local.TempCommon.Flag = "U";

                    break;
                  case 'U':
                    local.TempCommon.Flag = "V";

                    break;
                  case 'V':
                    local.TempCommon.Flag = "W";

                    break;
                  case 'W':
                    local.TempCommon.Flag = "X";

                    break;
                  case 'X':
                    local.TempCommon.Flag = "Y";

                    break;
                  case 'Y':
                    local.TempCommon.Flag = "Z";

                    break;
                  case 'Z':
                    local.TempCommon.Flag = "0";

                    break;
                  default:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Creating FIPS - County Abbreviation full; FIPS = " +
                      local.FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    goto Next;
                }

                local.CountyAbbrevNext.CountyAbbreviation =
                  local.CountyAbbrevFirstChar.Flag + local.TempCommon.Flag;
              }
              else
              {
                local.Fips.CountyAbbreviation =
                  local.CountyAbbrevNext.CountyAbbreviation ?? "";
              }
            }
            while(IsEmpty(local.Fips.CountyAbbreviation));

            try
            {
              CreateFips2();
              local.Previous.Assign(entities.New1);
              local.Trigger.Text9 = "ADD";
              ++local.AddTriggers.Count;
              ++local.ControlTotalAdded.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ++local.TriggersErred.Count;
                  ++local.ControlTotalErred.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "ERROR: Creating FIPS - Already Exists; FIPS = " + local
                    .FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";

                  continue;
                case ErrorCode.PermittedValueViolation:
                  ++local.TriggersErred.Count;
                  ++local.ControlTotalErred.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "ERROR: Creating FIPS - Permitted Value Violation; FIPS = " +
                    local.FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";

                  continue;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

Read:

          if (!IsEmpty(local.LeAutoFipsUpdate.City) && !
            IsEmpty(local.LeAutoFipsUpdate.StateOrCountry) && local
            .LeAutoFipsUpdate.ZipCode > 0 && (
              !IsEmpty(local.LeAutoFipsUpdate.Street1) || !
            IsEmpty(local.LeAutoFipsUpdate.Street2)))
          {
            if (ReadFipsTribAddress2())
            {
              if (local.LeAutoFipsUpdate.Extension <= 0)
              {
                local.FipsTribAddress.PhoneExtension = "";
              }
              else if (local.LeAutoFipsUpdate.Extension < 10)
              {
                local.FipsTribAddress.PhoneExtension = "    " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 15, 1);
              }
              else if (local.LeAutoFipsUpdate.Extension < 100)
              {
                local.FipsTribAddress.PhoneExtension = "   " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 14, 2);
              }
              else if (local.LeAutoFipsUpdate.Extension < 1000)
              {
                local.FipsTribAddress.PhoneExtension = "  " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 13, 3);
              }
              else if (local.LeAutoFipsUpdate.Extension < 10000)
              {
                local.FipsTribAddress.PhoneExtension = " " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 12, 4);
              }
              else
              {
                local.FipsTribAddress.PhoneExtension =
                  NumberToString(local.LeAutoFipsUpdate.Extension, 11, 5);
              }

              local.FipsTribAddress.ZipCode =
                NumberToString(local.LeAutoFipsUpdate.ZipCode / 10000, 5);

              if (Equal(local.FipsTribAddress.ZipCode, "00000"))
              {
                local.FipsTribAddress.ZipCode = "";
              }

              local.FipsTribAddress.Zip4 =
                NumberToString(local.LeAutoFipsUpdate.ZipCode -
                local.LeAutoFipsUpdate.ZipCode / 10000 * (long)10000, 4);

              if (Equal(local.FipsTribAddress.Zip4, "0000"))
              {
                local.FipsTribAddress.Zip4 = "";
              }

              try
              {
                UpdateFipsTribAddress();

                if (IsEmpty(local.Trigger.Text9))
                {
                  local.Trigger.Text9 = "UPDATE";
                  ++local.UpdateTriggers.Count;
                  ++local.ControlTotalUpdated.Count;
                }

                local.TempCommon.Count =
                  Length(TrimEnd(local.LeAutoFipsUpdate.Street1));

                if (local.TempCommon.Count > 25)
                {
                  ++local.TriggersWarned.Count;
                  ++local.ControlTotalWarned.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "WARNING: Updating Address - Address Line 1 greater than 25 characters; FIPS = " +
                    local.FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";
                }

                local.TempCommon.Count =
                  Length(TrimEnd(local.LeAutoFipsUpdate.Street2));

                if (local.TempCommon.Count > 25)
                {
                  ++local.TriggersWarned.Count;
                  ++local.ControlTotalWarned.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "WARNING: Updating Address - Address Line 2 greater than 25 characters; FIPS = " +
                    local.FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";
                }
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Updating Address - Not Unique; FIPS = " + local
                      .FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Updating Address - Permitted Value Violation; FIPS = " +
                      local.FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    continue;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            else if (ReadFips4())
            {
              if (local.LeAutoFipsUpdate.Extension <= 0)
              {
                local.FipsTribAddress.PhoneExtension = "";
              }
              else if (local.LeAutoFipsUpdate.Extension < 10)
              {
                local.FipsTribAddress.PhoneExtension = "    " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 15, 1);
              }
              else if (local.LeAutoFipsUpdate.Extension < 100)
              {
                local.FipsTribAddress.PhoneExtension = "   " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 14, 2);
              }
              else if (local.LeAutoFipsUpdate.Extension < 1000)
              {
                local.FipsTribAddress.PhoneExtension = "  " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 13, 3);
              }
              else if (local.LeAutoFipsUpdate.Extension < 10000)
              {
                local.FipsTribAddress.PhoneExtension = " " + NumberToString
                  (local.LeAutoFipsUpdate.Extension, 12, 4);
              }
              else
              {
                local.FipsTribAddress.PhoneExtension =
                  NumberToString(local.LeAutoFipsUpdate.Extension, 11, 5);
              }

              local.FipsTribAddress.ZipCode =
                NumberToString(local.LeAutoFipsUpdate.ZipCode / 10000, 5);

              if (Equal(local.FipsTribAddress.ZipCode, "00000"))
              {
                local.FipsTribAddress.ZipCode = "";
              }

              local.FipsTribAddress.Zip4 =
                NumberToString(local.LeAutoFipsUpdate.ZipCode -
                local.LeAutoFipsUpdate.ZipCode / 10000 * (long)10000, 4);

              if (Equal(local.FipsTribAddress.Zip4, "0000"))
              {
                local.FipsTribAddress.Zip4 = "";
              }

              ReadFipsTribAddress1();

              try
              {
                CreateFipsTribAddress();

                if (IsEmpty(local.Trigger.Text9))
                {
                  local.Trigger.Text9 = "UPDATE";
                  ++local.UpdateTriggers.Count;
                  ++local.ControlTotalUpdated.Count;
                }

                local.TempCommon.Count =
                  Length(TrimEnd(local.LeAutoFipsUpdate.Street1));

                if (local.TempCommon.Count > 25)
                {
                  ++local.TriggersWarned.Count;
                  ++local.ControlTotalWarned.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "WARNING: Creating Address - Address Line 1 greater than 25 characters; FIPS = " +
                    local.FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";
                }

                local.TempCommon.Count =
                  Length(TrimEnd(local.LeAutoFipsUpdate.Street2));

                if (local.TempCommon.Count > 25)
                {
                  ++local.TriggersWarned.Count;
                  ++local.ControlTotalWarned.Count;
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.State, 15);
                  UseEabConvertNumeric1();
                  local.FipsState.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabConvertNumeric.SendNonSuppressPos = 3;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.County, 15);
                  UseEabConvertNumeric1();
                  local.FipsCounty.Text4 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 13, 3);
                  local.EabConvertNumeric.SendNonSuppressPos = 2;
                  local.EabConvertNumeric.SendAmount =
                    NumberToString(local.Fips.Location, 15);
                  UseEabConvertNumeric1();
                  local.FipsLocation.Text3 =
                    Substring(local.EabConvertNumeric.
                      ReturnNoCommasInNonDecimal, 14, 2);
                  local.EabReportSend.RptDetail =
                    "WARNING: Creating Address - Address Line 2 greater than 25 characters; FIPS = " +
                    local.FipsState.Text3 + local.FipsCounty.Text4 + local
                    .FipsLocation.Text3;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabReportSend.RptDetail = "";
                }
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Creating Address - Already Exists; FIPS = " + local
                      .FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ++local.TriggersErred.Count;
                    ++local.ControlTotalErred.Count;
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.State, 15);
                    UseEabConvertNumeric1();
                    local.FipsState.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabConvertNumeric.SendNonSuppressPos = 3;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.County, 15);
                    UseEabConvertNumeric1();
                    local.FipsCounty.Text4 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 13, 3);
                    local.EabConvertNumeric.SendNonSuppressPos = 2;
                    local.EabConvertNumeric.SendAmount =
                      NumberToString(local.Fips.Location, 15);
                    UseEabConvertNumeric1();
                    local.FipsLocation.Text3 =
                      Substring(local.EabConvertNumeric.
                        ReturnNoCommasInNonDecimal, 14, 2);
                    local.EabReportSend.RptDetail =
                      "ERROR: Creating Address - Permitted Value Violation; FIPS = " +
                      local.FipsState.Text3 + local.FipsCounty.Text4 + local
                      .FipsLocation.Text3;
                    local.EabFileHandling.Action = "WRITE";
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";

                    continue;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else if (!IsEmpty(local.Trigger.Text9))
          {
            ++local.TriggersWarned.Count;
            ++local.ControlTotalWarned.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.State, 15);
            UseEabConvertNumeric1();
            local.FipsState.Text3 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14,
              2);
            local.EabConvertNumeric.SendNonSuppressPos = 3;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.County, 15);
            UseEabConvertNumeric1();
            local.FipsCounty.Text4 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13,
              3);
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.Location, 15);
            UseEabConvertNumeric1();
            local.FipsLocation.Text3 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14,
              2);
            local.EabReportSend.RptDetail =
              "WARNING: Updating Address - Minimum information is missing; FIPS = " +
              local.FipsState.Text3 + local.FipsCounty.Text4 + local
              .FipsLocation.Text3;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }
          else
          {
            ++local.TriggersErred.Count;
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.State, 15);
            UseEabConvertNumeric1();
            local.FipsState.Text3 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14,
              2);
            local.EabConvertNumeric.SendNonSuppressPos = 3;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.County, 15);
            UseEabConvertNumeric1();
            local.FipsCounty.Text4 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13,
              3);
            local.EabConvertNumeric.SendNonSuppressPos = 2;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.Fips.Location, 15);
            UseEabConvertNumeric1();
            local.FipsLocation.Text3 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14,
              2);
            local.EabReportSend.RptDetail =
              "ERROR: Updating Address - Minimum information is missing; FIPS = " +
              local.FipsState.Text3 + local.FipsCounty.Text4 + local
              .FipsLocation.Text3;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          ++local.RowLock.Count;
          ++local.TriggersProcessed.Count;
          ++local.ControlTotalProcessed.Count;
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.State, 15);
          UseEabConvertNumeric1();
          local.FipsState.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.County, 15);
          UseEabConvertNumeric1();
          local.FipsCounty.Text4 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 13, 3);
            
          local.EabConvertNumeric.SendNonSuppressPos = 2;
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Fips.Location, 15);
          UseEabConvertNumeric1();
          local.FipsLocation.Text3 =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 14, 2);
            
          local.DetailLine3.Text32 = local.FipsState.Text3 + local
            .FipsCounty.Text4 + local.FipsLocation.Text3 + "  " + local
            .Trigger.Text9;

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
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.DetailLine1.Text32 + local
              .DetailLine2.Text32 + local.DetailLine3.Text32;
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabReportSend.RptDetail =
                "ABEND:  Writing to Report 01;  FIPS = " + local
                .FipsState.Text3 + local.FipsCounty.Text4 + local
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
        }
      }
      else if (Equal(local.EabFileHandling.Status, "EF"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "NOTICE:  End of file reached";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabFileHandling.Status = "EF";
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "ABEND:  Error READing file; STATUS = " + local
          .EabFileHandling.Status;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      if (local.RowLock.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .RowLock.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.RowLock.Count = 0;
      }

Next:
      ;
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // mjr
    // -------------------------------------------------
    // Finish off report
    // ----------------------------------------------------
    if (local.Previous.State > 0)
    {
      if (local.Previous.State == local.Kansas.State)
      {
        // mjr
        // ----------------------------------------------
        // 04/07/2000
        // Don't write KS to Data Report
        // -----------------------------------------------------------
        goto Test2;
      }

      local.EabFileHandling.Action = "WRITE";

      if (!IsEmpty(local.DetailLine1.Text32))
      {
        local.EabReportSend.RptDetail = local.DetailLine1.Text32 + local
          .DetailLine2.Text32 + local.DetailLine3.Text32;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Writing to Report 01;  FIPS = " + local.FipsState.Text3 + local
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
        NumberToString(local.Previous.State, 15);
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
          "ABEND:  Writing to Report 01;  FIPS = " + local.FipsState.Text3 + local
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
          "ABEND:  Writing to Report 01;  FIPS = " + local.FipsState.Text3 + local
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
          "ABEND:  Writing to Report 01;  FIPS = " + local.FipsState.Text3 + local
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
          "ABEND:  Writing to Report 01;  FIPS = " + local.FipsState.Text3 + local
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
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Writing to Report 01;  FIPS = " + local.FipsState.Text3 + local
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
    }

Test2:

    // ---------------------------------------------------------------
    // WRITE CONTROL TOTALS AND CLOSE REPORTS
    // ---------------------------------------------------------------
    UseLeB595WriteControlsAndClose();
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

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ReportNumber = source.ReportNumber;
    target.RptDetail = source.RptDetail;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeB595Housekeeping()
  {
    var useImport = new LeB595Housekeeping.Import();
    var useExport = new LeB595Housekeeping.Export();

    Call(LeB595Housekeeping.Execute, useImport, useExport);

    local.Current.Timestamp = useExport.Current.Timestamp;
    local.Kansas.State = useExport.Kansas.State;
    local.Parm.Assign(useExport.Trigger);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    MoveEabReportSend(useExport.EabReportSend, local.EabReportSend);
  }

  private void UseLeB595WriteControlsAndClose()
  {
    var useImport = new LeB595WriteControlsAndClose.Import();
    var useExport = new LeB595WriteControlsAndClose.Export();

    useImport.Warned.Count = local.ControlTotalWarned.Count;
    useImport.Skipped.Count = local.ControlTotalSkipped.Count;
    useImport.Erred.Count = local.ControlTotalErred.Count;
    useImport.Read.Count = local.ControlTotalRead.Count;
    useImport.Added.Count = local.ControlTotalAdded.Count;
    useImport.Updated.Count = local.ControlTotalUpdated.Count;
    useImport.Deleted.Count = local.ControlTotalDeleted.Count;
    useImport.Processed.Count = local.ControlTotalProcessed.Count;
    useImport.EabReportSend.ReportNumber = local.EabReportSend.ReportNumber;

    Call(LeB595WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseLeEabReadFipsUpdate()
  {
    var useImport = new LeEabReadFipsUpdate.Import();
    var useExport = new LeEabReadFipsUpdate.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.ReportNumber = local.EabReportSend.ReportNumber;
    useExport.LeAutoFipsUpdate.Assign(local.LeAutoFipsUpdate);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadFipsUpdate.Execute, useImport, useExport);

    local.LeAutoFipsUpdate.Assign(useExport.LeAutoFipsUpdate);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void CreateFips1()
  {
    var state = entities.Fips.State;
    var county = entities.Fips.County;
    var location = local.LeAutoFipsUpdate.SubLocalCode;
    var stateDescription = entities.Fips.StateDescription;
    var countyDescription = entities.Fips.CountyDescription;
    var locationDescription = local.Fips.LocationDescription ?? "";
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTstamp = local.Current.Timestamp;
    var stateAbbreviation = entities.Fips.StateAbbreviation;
    var countyAbbreviation = entities.Fips.CountyAbbreviation;

    entities.New1.Populated = false;
    Update("CreateFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", state);
        db.SetInt32(command, "county", county);
        db.SetInt32(command, "location", location);
        db.SetNullableString(command, "stateDesc", stateDescription);
        db.SetNullableString(command, "countyDesc", countyDescription);
        db.SetNullableString(command, "locationDesc", locationDescription);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetString(command, "stateAbbreviation", stateAbbreviation);
        db.SetNullableString(command, "countyAbbr", countyAbbreviation);
      });

    entities.New1.State = state;
    entities.New1.County = county;
    entities.New1.Location = location;
    entities.New1.StateDescription = stateDescription;
    entities.New1.CountyDescription = countyDescription;
    entities.New1.LocationDescription = locationDescription;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.StateAbbreviation = stateAbbreviation;
    entities.New1.CountyAbbreviation = countyAbbreviation;
    entities.New1.Populated = true;
  }

  private void CreateFips2()
  {
    var state = local.LeAutoFipsUpdate.StateCode;
    var county = local.LeAutoFipsUpdate.LocalCode;
    var location = local.LeAutoFipsUpdate.SubLocalCode;
    var stateDescription = local.Fips.StateDescription ?? "";
    var countyDescription = local.Fips.CountyDescription ?? "";
    var locationDescription = local.Fips.LocationDescription ?? "";
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTstamp = local.Current.Timestamp;
    var stateAbbreviation = local.Fips.StateAbbreviation;
    var countyAbbreviation = local.Fips.CountyAbbreviation ?? "";

    entities.New1.Populated = false;
    Update("CreateFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", state);
        db.SetInt32(command, "county", county);
        db.SetInt32(command, "location", location);
        db.SetNullableString(command, "stateDesc", stateDescription);
        db.SetNullableString(command, "countyDesc", countyDescription);
        db.SetNullableString(command, "locationDesc", locationDescription);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetString(command, "stateAbbreviation", stateAbbreviation);
        db.SetNullableString(command, "countyAbbr", countyAbbreviation);
      });

    entities.New1.State = state;
    entities.New1.County = county;
    entities.New1.Location = location;
    entities.New1.StateDescription = stateDescription;
    entities.New1.CountyDescription = countyDescription;
    entities.New1.LocationDescription = locationDescription;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.StateAbbreviation = stateAbbreviation;
    entities.New1.CountyAbbreviation = countyAbbreviation;
    entities.New1.Populated = true;
  }

  private void CreateFipsTribAddress()
  {
    var identifier = local.TempCommon.Count + 1;
    var faxAreaCode = local.LeAutoFipsUpdate.FaxAreaCode;
    var phoneExtension = local.FipsTribAddress.PhoneExtension ?? "";
    var areaCode = local.LeAutoFipsUpdate.AreaCode;
    var type1 = "M";
    var street1 = Substring(local.LeAutoFipsUpdate.Street1, 1, 25);
    var street2 = Substring(local.LeAutoFipsUpdate.Street2, 1, 25);
    var city = Substring(local.LeAutoFipsUpdate.City, 1, 15);
    var state = Substring(local.LeAutoFipsUpdate.StateOrCountry, 1, 2);
    var zipCode = local.FipsTribAddress.ZipCode;
    var zip4 = local.FipsTribAddress.Zip4 ?? "";
    var county = local.Previous.CountyAbbreviation ?? "";
    var phoneNumber = local.LeAutoFipsUpdate.PhoneNumber;
    var faxNumber = local.LeAutoFipsUpdate.FaxNumber;
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTstamp = local.Current.Timestamp;
    var fipState = entities.Fips.State;
    var fipCounty = entities.Fips.County;
    var fipLocation = entities.Fips.Location;

    entities.FipsTribAddress.Populated = false;
    Update("CreateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "faxExtension", "");
        db.SetNullableInt32(command, "faxAreaCd", faxAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetNullableInt32(command, "areaCd", areaCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetNullableInt32(command, "fipState", fipState);
        db.SetNullableInt32(command, "fipCounty", fipCounty);
        db.SetNullableInt32(command, "fipLocation", fipLocation);
      });

    entities.FipsTribAddress.Identifier = identifier;
    entities.FipsTribAddress.FaxExtension = "";
    entities.FipsTribAddress.FaxAreaCode = faxAreaCode;
    entities.FipsTribAddress.PhoneExtension = phoneExtension;
    entities.FipsTribAddress.AreaCode = areaCode;
    entities.FipsTribAddress.Type1 = type1;
    entities.FipsTribAddress.Street1 = street1;
    entities.FipsTribAddress.Street2 = street2;
    entities.FipsTribAddress.City = city;
    entities.FipsTribAddress.State = state;
    entities.FipsTribAddress.ZipCode = zipCode;
    entities.FipsTribAddress.Zip4 = zip4;
    entities.FipsTribAddress.County = county;
    entities.FipsTribAddress.PhoneNumber = phoneNumber;
    entities.FipsTribAddress.FaxNumber = faxNumber;
    entities.FipsTribAddress.CreatedBy = createdBy;
    entities.FipsTribAddress.CreatedTstamp = createdTstamp;
    entities.FipsTribAddress.LastUpdatedBy = "";
    entities.FipsTribAddress.LastUpdatedTstamp = null;
    entities.FipsTribAddress.FipState = fipState;
    entities.FipsTribAddress.FipCounty = fipCounty;
    entities.FipsTribAddress.FipLocation = fipLocation;
    entities.FipsTribAddress.Populated = true;
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.Fips.State);
        db.SetNullableString(
          command, "countyAbbr", local.CountyAbbrevNext.CountyAbbreviation ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.CreatedBy = db.GetString(reader, 6);
        entities.Fips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Fips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Fips.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.Fips.State);
        db.SetInt32(command, "county", local.Fips.County);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.CreatedBy = db.GetString(reader, 6);
        entities.Fips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Fips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Fips.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.Fips.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.CreatedBy = db.GetString(reader, 6);
        entities.Fips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Fips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Fips.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips4()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips4",
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
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.CreatedBy = db.GetString(reader, 6);
        entities.Fips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Fips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Fips.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    return Read("ReadFipsTribAddress1",
      null,
      (db, reader) =>
      {
        local.TempCommon.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", local.Fips.State);
        db.SetNullableInt32(command, "fipCounty", local.Fips.County);
        db.SetNullableInt32(command, "fipLocation", local.Fips.Location);
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
        entities.FipsTribAddress.County = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 13);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 14);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 15);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 16);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 18);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 21);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private void UpdateFips()
  {
    var locationDescription = local.Fips.LocationDescription ?? "";
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTstamp = local.Current.Timestamp;

    entities.Fips.Populated = false;
    Update("UpdateFips",
      (db, command) =>
      {
        db.SetNullableString(command, "locationDesc", locationDescription);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(command, "state", entities.Fips.State);
        db.SetInt32(command, "county", entities.Fips.County);
        db.SetInt32(command, "location", entities.Fips.Location);
      });

    entities.Fips.LocationDescription = locationDescription;
    entities.Fips.LastUpdatedBy = lastUpdatedBy;
    entities.Fips.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Fips.Populated = true;
  }

  private void UpdateFipsTribAddress()
  {
    var faxAreaCode = local.LeAutoFipsUpdate.FaxAreaCode;
    var phoneExtension = local.FipsTribAddress.PhoneExtension ?? "";
    var areaCode = local.LeAutoFipsUpdate.AreaCode;
    var type1 = "M";
    var street1 = Substring(local.LeAutoFipsUpdate.Street1, 1, 25);
    var street2 = Substring(local.LeAutoFipsUpdate.Street2, 1, 25);
    var city = Substring(local.LeAutoFipsUpdate.City, 1, 15);
    var state = Substring(local.LeAutoFipsUpdate.StateOrCountry, 1, 2);
    var zipCode = local.FipsTribAddress.ZipCode;
    var zip4 = local.FipsTribAddress.Zip4 ?? "";
    var county = local.Previous.CountyAbbreviation ?? "";
    var phoneNumber = local.LeAutoFipsUpdate.PhoneNumber;
    var faxNumber = local.LeAutoFipsUpdate.FaxNumber;
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTstamp = local.Current.Timestamp;

    entities.FipsTribAddress.Populated = false;
    Update("UpdateFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "faxAreaCd", faxAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetNullableInt32(command, "areaCd", areaCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "county", county);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });

    entities.FipsTribAddress.FaxAreaCode = faxAreaCode;
    entities.FipsTribAddress.PhoneExtension = phoneExtension;
    entities.FipsTribAddress.AreaCode = areaCode;
    entities.FipsTribAddress.Type1 = type1;
    entities.FipsTribAddress.Street1 = street1;
    entities.FipsTribAddress.Street2 = street2;
    entities.FipsTribAddress.City = city;
    entities.FipsTribAddress.State = state;
    entities.FipsTribAddress.ZipCode = zipCode;
    entities.FipsTribAddress.Zip4 = zip4;
    entities.FipsTribAddress.County = county;
    entities.FipsTribAddress.PhoneNumber = phoneNumber;
    entities.FipsTribAddress.FaxNumber = faxNumber;
    entities.FipsTribAddress.LastUpdatedBy = lastUpdatedBy;
    entities.FipsTribAddress.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.FipsTribAddress.Populated = true;
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
    /// A value of TempWorkArea.
    /// </summary>
    [JsonPropertyName("tempWorkArea")]
    public WorkArea TempWorkArea
    {
      get => tempWorkArea ??= new();
      set => tempWorkArea = value;
    }

    /// <summary>
    /// A value of TriggersWarned.
    /// </summary>
    [JsonPropertyName("triggersWarned")]
    public Common TriggersWarned
    {
      get => triggersWarned ??= new();
      set => triggersWarned = value;
    }

    /// <summary>
    /// A value of ControlTotalWarned.
    /// </summary>
    [JsonPropertyName("controlTotalWarned")]
    public Common ControlTotalWarned
    {
      get => controlTotalWarned ??= new();
      set => controlTotalWarned = value;
    }

    /// <summary>
    /// A value of CountyAbbrevFirstChar.
    /// </summary>
    [JsonPropertyName("countyAbbrevFirstChar")]
    public Common CountyAbbrevFirstChar
    {
      get => countyAbbrevFirstChar ??= new();
      set => countyAbbrevFirstChar = value;
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
    /// A value of ControlTotalSkipped.
    /// </summary>
    [JsonPropertyName("controlTotalSkipped")]
    public Common ControlTotalSkipped
    {
      get => controlTotalSkipped ??= new();
      set => controlTotalSkipped = value;
    }

    /// <summary>
    /// A value of NullLeAutoFipsUpdate.
    /// </summary>
    [JsonPropertyName("nullLeAutoFipsUpdate")]
    public LeAutoFipsUpdate NullLeAutoFipsUpdate
    {
      get => nullLeAutoFipsUpdate ??= new();
      set => nullLeAutoFipsUpdate = value;
    }

    /// <summary>
    /// A value of NullFips.
    /// </summary>
    [JsonPropertyName("nullFips")]
    public Fips NullFips
    {
      get => nullFips ??= new();
      set => nullFips = value;
    }

    /// <summary>
    /// A value of TempCommon.
    /// </summary>
    [JsonPropertyName("tempCommon")]
    public Common TempCommon
    {
      get => tempCommon ??= new();
      set => tempCommon = value;
    }

    /// <summary>
    /// A value of CountyAbbrevNext.
    /// </summary>
    [JsonPropertyName("countyAbbrevNext")]
    public Fips CountyAbbrevNext
    {
      get => countyAbbrevNext ??= new();
      set => countyAbbrevNext = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Kansas.
    /// </summary>
    [JsonPropertyName("kansas")]
    public Fips Kansas
    {
      get => kansas ??= new();
      set => kansas = value;
    }

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

    private WorkArea tempWorkArea;
    private Common triggersWarned;
    private Common controlTotalWarned;
    private Common countyAbbrevFirstChar;
    private FipsTribAddress fipsTribAddress;
    private Common controlTotalSkipped;
    private LeAutoFipsUpdate nullLeAutoFipsUpdate;
    private Fips nullFips;
    private Common tempCommon;
    private Fips countyAbbrevNext;
    private Code code;
    private CodeValue codeValue;
    private DateWorkArea current;
    private Fips kansas;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Fips New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips new1;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
  }
#endregion
}
