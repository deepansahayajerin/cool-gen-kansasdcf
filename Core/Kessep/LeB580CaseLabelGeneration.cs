// Program: LE_B580_CASE_LABEL_GENERATION, ID: 372962148, model: 746.
// Short name: SWEB580P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B580_CASE_LABEL_GENERATION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB580CaseLabelGeneration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B580_CASE_LABEL_GENERATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB580CaseLabelGeneration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB580CaseLabelGeneration.
  /// </summary>
  public LeB580CaseLabelGeneration(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.DueInDec1999.Date = new DateTime(1999, 12, 1);
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *** Get DB2 commit frequency counts
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.CheckpointRestartKeyCase.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
    }
    else
    {
      local.CheckpointRestartKeyCase.Number = "";
    }

    local.Total.Count = 0;
    local.Open.Count = 0;
    local.Closed.Count = 0;
    local.Na.Count = 0;
    local.AccrThruDec.Count = 0;
    local.MultipleAps.Count = 0;
    local.KansasAsAr.Count = 0;
    local.NonNa.Count = 0;
    local.Error.Count = 0;

    // * * * * * * * * * *
    // Open CONTROL REPORT
    // * * * * * * * * * *
    local.ReportEabReportSend.ProcessDate = local.Current.Date;
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * *
    // Open ERROR REPORT
    // * * * * * * * * *
    local.ReportEabReportSend.ProcessDate = local.Current.Date;
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "OPEN";
    UseCabErrorReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    foreach(var item in ReadCase())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.NoActiveProgramInd.Flag = "";
      ++local.Total.Count;

      if (AsChar(entities.Case1.Status) == 'C')
      {
        ++local.Closed.Count;

        continue;
      }

      ++local.Open.Count;
      MoveCase1(entities.Case1, export.Case1);
      export.ArCsePersonsWorkSet.Number = "";
      export.ArCsePersonsWorkSet.FormattedName = "";
      export.Ap.Number = "";

      if (ReadCsePerson1())
      {
        export.ArCsePersonsWorkSet.Number = entities.CsePerson.Number;
        local.CsePerson.Number = entities.CsePerson.Number;
      }

      if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
      {
        ++local.KansasAsAr.Count;

        continue;
      }

      if (IsEmpty(export.ArCsePersonsWorkSet.Number))
      {
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No AR exists for this Case. Skip to the next Case...";
          
        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          ++local.Error.Count;

          continue;
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      UseSiReadCsePersonBatch();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": ADABAS Error Code -->" +
          TrimEnd(local.AbendData.AdabasResponseCd);
        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          ++local.Error.Count;

          continue;

          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseSiCadsReadCaseDetails();

      if (ReadCaseAssignment())
      {
        if (ReadOffice())
        {
          export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        }
        else
        {
          local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No Office found for this Case. Continue...";
            
          local.ReportEabReportSend.ProgramName =
            local.ProgramProcessingInfo.Name;
          local.ReportEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
      else
      {
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No Case Assignment found for this Case. Continue...";
          
        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      local.Accrual.Flag = "N";

      if (AsChar(local.NoActiveProgramInd.Flag) == 'Y')
      {
        export.Ap.Number = "";

        if (ReadCsePerson2())
        {
          export.Ap.Number = entities.CsePerson.Number;
        }

        local.Ap.Count = 0;

        foreach(var item1 in ReadCsePerson3())
        {
          ++local.Ap.Count;
        }

        if (local.Ap.Count > 1)
        {
          ++local.MultipleAps.Count;
          local.ReportEabReportSend.RptDetail = export.Case1.Number + ": Multiple APs exist for the Case.";
            
          local.ReportEabReportSend.ProgramName =
            local.ProgramProcessingInfo.Name;
          local.ReportEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (IsEmpty(export.Ap.Number))
        {
          local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No AP exists for the Case.";
            
          local.ReportEabReportSend.ProgramName =
            local.ProgramProcessingInfo.Name;
          local.ReportEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            ++local.Error.Count;

            continue;
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        foreach(var item1 in ReadObligation())
        {
          if (ReadDebtDetail())
          {
            local.Accrual.Flag = "Y";
            ++local.AccrThruDec.Count;

            goto Test;
          }
        }

        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": Unable to decide on the Accruals for this Case with no Program Type.";
          
        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          ++local.Error.Count;

          continue;
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

Test:

      if (AsChar(local.Accrual.Flag) == 'Y')
      {
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": Accrual Flag set to 'Y' for the Case.";
          
        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (Equal(export.Program.Code, "NA") || AsChar(local.Accrual.Flag) == 'Y')
      {
        UseSiGetCsePersonMailingAddr();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No Valid Address found for the AR on this Case.";
            
          local.ReportEabReportSend.ProgramName =
            local.ProgramProcessingInfo.Name;
          local.ReportEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            ++local.Error.Count;

            continue;
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        ++local.Na.Count;
        local.ReportEabReportSend.RptDetail =
          NumberToString(export.Office.SystemGeneratedId, 12, 4);
        local.ReportEabReportSend.RptDetail =
          TrimEnd(local.ReportEabReportSend.RptDetail) + export.Case1.Number;
        local.ReportEabReportSend.RptDetail =
          TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
          (export.ArCsePersonsWorkSet.FormattedName);
        local.ReportEabReportSend.RptDetail =
          TrimEnd(local.ReportEabReportSend.RptDetail) + ";";

        if (!IsEmpty(local.CsePersonAddress.Street1))
        {
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
            (local.CsePersonAddress.Street1) + ";";
        }

        if (!IsEmpty(local.CsePersonAddress.Street2))
        {
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
            (local.CsePersonAddress.Street2) + ";";
        }

        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        if (AsChar(local.CsePersonAddress.LocationType) == 'D')
        {
          local.ReportEabReportSend.RptDetail =
            NumberToString(export.Office.SystemGeneratedId, 12, 4);
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + export.Case1.Number;
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + "*D*";
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
            (local.CsePersonAddress.City) + ",";
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + "  " + (
              local.CsePersonAddress.State ?? "") + " " + (
              local.CsePersonAddress.ZipCode ?? "");

          if (!IsEmpty(local.CsePersonAddress.Zip3))
          {
            local.ReportEabReportSend.RptDetail =
              TrimEnd(local.ReportEabReportSend.RptDetail) + " " + (
                local.CsePersonAddress.Zip3 ?? "");
          }

          if (!IsEmpty(local.CsePersonAddress.Zip4))
          {
            local.ReportEabReportSend.RptDetail =
              TrimEnd(local.ReportEabReportSend.RptDetail) + " " + (
                local.CsePersonAddress.Zip4 ?? "");
          }
        }

        if (AsChar(local.CsePersonAddress.LocationType) == 'F')
        {
          local.ReportEabReportSend.RptDetail =
            NumberToString(export.Office.SystemGeneratedId, 12, 4);
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + export.Case1.Number;
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + "*F*";

          if (IsEmpty(local.CsePersonAddress.Street3))
          {
            local.CsePersonAddress.Street3 = "*****";
          }

          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
            (local.CsePersonAddress.Street3) + ";";

          if (IsEmpty(local.CsePersonAddress.Street4))
          {
            local.CsePersonAddress.Street4 = "*****";
          }

          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
            (local.CsePersonAddress.Street4) + ";";
          local.ReportEabReportSend.RptDetail =
            TrimEnd(local.ReportEabReportSend.RptDetail) + "  " + TrimEnd
            (local.CsePersonAddress.Province) + (
              local.CsePersonAddress.PostalCode ?? "") + "  " + (
              local.CsePersonAddress.Country ?? "");
        }

        local.ReportEabReportSend.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
      else
      {
        ++local.NonNa.Count;
      }
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT for total Cases processed
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases read                                 = " + NumberToString
      (local.Total.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT for Open Cases processed
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Open Cases processed                       = " + NumberToString
      (local.Open.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT for Closed Cases processed
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Closed Cases read                          = " + NumberToString
      (local.Closed.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT total NA Cases processed
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of NA Cases processed                         = " + NumberToString
      (local.Na.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT total NA Cases (Accrual Thru Dec'99) processed
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases processed (Accrual thru Dec'1999)    = " + NumberToString
      (local.AccrThruDec.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT for Cases with Multiple APs
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases with Multiple APs                    = " + NumberToString
      (local.MultipleAps.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT for Cases with Kansas as AR
    // * * ** * * * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases with Kansas State as AR              = " + NumberToString
      (local.KansasAsAr.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT totals non-NA Cases
    // * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of non-NA Cases                               = " + NumberToString
      (local.NonNa.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT - spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * *
    // Write the CONTROL REPORT totals for errors
    // * * * * * * * * * * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Records in Error                           = " + NumberToString
      (local.Error.Count, 15);
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * * *
    // Close the CONTROL REPORT
    // * * * * * * * * * * * *
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * *
    // Close Error Report
    // * * * * * * * * * *
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------
    // Set restart indicator to no - successfully finished program
    // -------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart();
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText3 = source.FuncText3;
    target.FuncDate = source.FuncDate;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.EndDate = source.EndDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInterstate(SiCadsReadCaseDetails.Export.
    InterstateGroup source, Export.InterstateGroup target)
  {
    target.Interstate1.Text4 = source.Interstate1.Text4;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiCadsReadCaseDetails()
  {
    var useImport = new SiCadsReadCaseDetails.Import();
    var useExport = new SiCadsReadCaseDetails.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Ap.Number = export.Ap.Number;

    Call(SiCadsReadCaseDetails.Execute, useImport, useExport);

    local.NoActiveProgramInd.Flag = useExport.NoActiveProgramInd.Flag;
    export.Case1.Assign(useExport.Case1);
    export.DesignatedPayeeFnd.Flag = useExport.DesignatedPayeeInd.Flag;
    export.Program.Code = useExport.Program.Code;
    export.ArCaseRole.Assign(useExport.Assign1);
    export.MedProgExists.Flag = useExport.MedProgExists.Flag;
    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, export.CaseFuncWorkSet);
    useExport.Interstate.CopyTo(export.Interstate, MoveInterstate);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.ArCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CheckpointRestartKeyCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", export.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.
          SetDate(command, "dueDt", local.DueInDec1999.Date.GetValueOrDefault());
          
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
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
    /// <summary>A InterstateGroup group.</summary>
    [Serializable]
    public class InterstateGroup
    {
      /// <summary>
      /// A value of Interstate1.
      /// </summary>
      [JsonPropertyName("interstate1")]
      public TextWorkArea Interstate1
      {
        get => interstate1 ??= new();
        set => interstate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private TextWorkArea interstate1;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of DesignatedPayeeFnd.
    /// </summary>
    [JsonPropertyName("designatedPayeeFnd")]
    public Common DesignatedPayeeFnd
    {
      get => designatedPayeeFnd ??= new();
      set => designatedPayeeFnd = value;
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
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of MedProgExists.
    /// </summary>
    [JsonPropertyName("medProgExists")]
    public Common MedProgExists
    {
      get => medProgExists ??= new();
      set => medProgExists = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Interstate.
    /// </summary>
    [JsonIgnore]
    public Array<InterstateGroup> Interstate => interstate ??= new(
      InterstateGroup.Capacity);

    /// <summary>
    /// Gets a value of Interstate for json serialization.
    /// </summary>
    [JsonPropertyName("interstate")]
    [Computed]
    public IList<InterstateGroup> Interstate_Json
    {
      get => interstate;
      set => Interstate.Assign(value);
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private Common designatedPayeeFnd;
    private Program program;
    private CaseRole arCaseRole;
    private Common medProgExists;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Array<InterstateGroup> interstate;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet ap;
    private Common caseOpen;
    private Common hiddenAe;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NonNa.
    /// </summary>
    [JsonPropertyName("nonNa")]
    public Common NonNa
    {
      get => nonNa ??= new();
      set => nonNa = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of DueInDec1999.
    /// </summary>
    [JsonPropertyName("dueInDec1999")]
    public DateWorkArea DueInDec1999
    {
      get => dueInDec1999 ??= new();
      set => dueInDec1999 = value;
    }

    /// <summary>
    /// A value of Accrual.
    /// </summary>
    [JsonPropertyName("accrual")]
    public Common Accrual
    {
      get => accrual ??= new();
      set => accrual = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of KansasAsAr.
    /// </summary>
    [JsonPropertyName("kansasAsAr")]
    public Common KansasAsAr
    {
      get => kansasAsAr ??= new();
      set => kansasAsAr = value;
    }

    /// <summary>
    /// A value of CheckpointRestartKeyCase.
    /// </summary>
    [JsonPropertyName("checkpointRestartKeyCase")]
    public Case1 CheckpointRestartKeyCase
    {
      get => checkpointRestartKeyCase ??= new();
      set => checkpointRestartKeyCase = value;
    }

    /// <summary>
    /// A value of AccrThruDec.
    /// </summary>
    [JsonPropertyName("accrThruDec")]
    public Common AccrThruDec
    {
      get => accrThruDec ??= new();
      set => accrThruDec = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Common Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Common Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Common Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public Case1 End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Case1 Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of NoActiveProgramInd.
    /// </summary>
    [JsonPropertyName("noActiveProgramInd")]
    public Common NoActiveProgramInd
    {
      get => noActiveProgramInd ??= new();
      set => noActiveProgramInd = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of CheckpointRestartKeyInfrastructure.
    /// </summary>
    [JsonPropertyName("checkpointRestartKeyInfrastructure")]
    public Infrastructure CheckpointRestartKeyInfrastructure
    {
      get => checkpointRestartKeyInfrastructure ??= new();
      set => checkpointRestartKeyInfrastructure = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of UpdateLoop.
    /// </summary>
    [JsonPropertyName("updateLoop")]
    public Common UpdateLoop
    {
      get => updateLoop ??= new();
      set => updateLoop = value;
    }

    /// <summary>
    /// A value of ReportEabReportSend.
    /// </summary>
    [JsonPropertyName("reportEabReportSend")]
    public EabReportSend ReportEabReportSend
    {
      get => reportEabReportSend ??= new();
      set => reportEabReportSend = value;
    }

    /// <summary>
    /// A value of ReportEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportEabFileHandling")]
    public EabFileHandling ReportEabFileHandling
    {
      get => reportEabFileHandling ??= new();
      set => reportEabFileHandling = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private Common nonNa;
    private CsePerson csePerson;
    private Common ap;
    private DateWorkArea dueInDec1999;
    private Common accrual;
    private CsePersonAddress csePersonAddress;
    private Common kansasAsAr;
    private Case1 checkpointRestartKeyCase;
    private Common accrThruDec;
    private Common na;
    private Common closed;
    private Common open;
    private Case1 end;
    private Case1 start;
    private Common noActiveProgramInd;
    private Common multipleAps;
    private AbendData abendData;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure checkpointRestartKeyInfrastructure;
    private Common update;
    private Common total;
    private Common updateLoop;
    private EabReportSend reportEabReportSend;
    private EabFileHandling reportEabFileHandling;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private Infrastructure infrastructure;
  }
#endregion
}
