// Program: SP_B704_HOUSEKEEPING, ID: 370986480, model: 746.
// Short name: SWE02501
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B704_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB704Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B704_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB704Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB704Housekeeping.
  /// </summary>
  public SpB704Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB704";
    export.Current.Timestamp = Now();
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      export.ProgramProcessingInfo);

    // mjr---> If ppi process_date is null, set it to current date
    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    export.End.Date = export.ProgramProcessingInfo.ProcessDate;
    local.BatchTimestampWorkArea.TextDateYyyy =
      NumberToString(Year(export.End.Date), 4);
    local.BatchTimestampWorkArea.TextDateMm =
      NumberToString(Month(export.End.Date), 2);
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(Day(export.End.Date), 2);
    export.End.TextDate = local.BatchTimestampWorkArea.TextDateYyyy + local
      .BatchTimestampWorkArea.TextDateMm + local
      .BatchTimestampWorkArea.TestDateDd;

    // mjr
    // ---------------------------------------------------
    // Use the end of the next day in case ran past midnight
    // ------------------------------------------------------
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(Day(AddDays(export.End.Date, 1)), 2);
    export.End.Timestamp =
      Timestamp(local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
      .BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd + "-23.59.59.999999");

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 1);

        if (IsEmpty(export.DebugOn.Flag))
        {
          export.DebugOn.Flag = "Y";
        }
      }
    }

    // --------------------------------------------------------------------
    // DETERMINE IF THIS IS A RESTART SITUATION
    // --------------------------------------------------------------------
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------
    // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
    // --------------------------------------------------------------------
    local.Position.Count =
      Find(export.ProgramCheckpointRestart.RestartInfo, "LAST_PROCESS:");

    if (local.Position.Count > 0)
    {
      export.LastProcess.TextDate =
        Substring(export.ProgramCheckpointRestart.RestartInfo,
        local.Position.Count + 13, 8);
      export.Start.Date =
        AddDays(IntToDate((int)StringToNumber(export.LastProcess.TextDate)), 1);
        
    }
    else
    {
      local.LastProcess.Date =
        AddMonths(export.ProgramProcessingInfo.ProcessDate, -1);
      local.BatchTimestampWorkArea.TextDateYyyy =
        NumberToString(Year(local.LastProcess.Date), 4);
      local.BatchTimestampWorkArea.TextDateMm =
        NumberToString(Month(local.LastProcess.Date), 2);
      local.BatchTimestampWorkArea.TestDateDd =
        NumberToString(Day(local.LastProcess.Date), 2);
      export.LastProcess.TextDate =
        local.BatchTimestampWorkArea.TextDateYyyy + local
        .BatchTimestampWorkArea.TextDateMm + local
        .BatchTimestampWorkArea.TestDateDd;
      export.Start.Date = AddDays(local.LastProcess.Date, 1);
    }

    local.BatchTimestampWorkArea.TextDateYyyy =
      NumberToString(Year(export.Start.Date), 4);
    local.BatchTimestampWorkArea.TextDateMm =
      NumberToString(Month(export.Start.Date), 2);
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(Day(export.Start.Date), 2);
    local.Start.TextDate = local.BatchTimestampWorkArea.TextDateYyyy + local
      .BatchTimestampWorkArea.TextDateMm + local
      .BatchTimestampWorkArea.TestDateDd;
    export.Start.Timestamp =
      Timestamp(local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
      .BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd);

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(export.ProgramCheckpointRestart.RestartInfo, "CASE:");

      if (local.Position.Count > 0)
      {
        export.Restart.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 5, 10);
      }
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // OPEN OUTPUT CONTROL REPORT 98
    // ------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT 99
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "R U N   T I M E   P A R A M E T E R S";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "START DATE:  " + local.Start.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "END DATE:  " + export.End.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    if (AsChar(export.DebugOn.Flag) != 'N')
    {
      local.EabReportSend.RptDetail = "DEBUG:  ON";
    }
    else
    {
      local.EabReportSend.RptDetail = "DEBUG:  OFF";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.EabReportSend.RptDetail = "RESTART:  YES";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      local.EabReportSend.RptDetail = "     CASE:  " + export.Restart.Number;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }
    }
    else
    {
      local.EabReportSend.RptDetail = "RESTART:  NO";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    if (ReadProgram2())
    {
      export.Af.SystemGeneratedIdentifier =
        entities.Program.SystemGeneratedIdentifier;
    }
    else
    {
      local.EabReportSend.RptDetail = "ABEND:  Program not found; Code = AF";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // mjr
    // -----------------------------------------
    // 09/24/2002
    // Retrieve COMPLIANCE PROGRAMS code table.  Each
    // code_value is a valid compliance program/subprogram
    // combination.  They are denoted as:
    // xx-yy; where xx is the program and yy is the subprogram.
    // If included, the dash indicates that not all the
    // subprograms for that program are compliance subprograms.
    // For example:
    // Assume the given the code_values MA-CM, AF, CC-<blank>.
    // This indicates:
    // -  MA-CM is a compliance program/subprogram
    // combination, but MA by itself is not a compliance program,
    // and no other MA subprograms are compliance programs;
    // -  AF and any subprograms are compliance program/
    // subprogram combinations; and
    // -  CC with a <blank> subprogram is a compliance program/
    // subprogram.
    // ------------------------------------------------------
    if (ReadCode())
    {
      foreach(var item in ReadCodeValue())
      {
        local.Program.Code = Substring(entities.CodeValue.Cdvalue, 1, 2);
        local.Dash.Text1 = Substring(entities.CodeValue.Cdvalue, 3, 1);
        local.PersonProgram.MedType =
          Substring(entities.CodeValue.Cdvalue, 4, 2);
        export.Compliance.Index = 0;

        for(var limit = export.Compliance.Count; export.Compliance.Index < limit
          ; ++export.Compliance.Index)
        {
          if (!export.Compliance.CheckSize())
          {
            break;
          }

          if (Equal(local.Program.Code,
            export.Compliance.Item.GexportCompliance.Code))
          {
            if (IsEmpty(local.Dash.Text1))
            {
              goto ReadEach;
            }

            for(export.Compliance.Item.ComplianceSub.Index = 0; export
              .Compliance.Item.ComplianceSub.Index < export
              .Compliance.Item.ComplianceSub.Count; ++
              export.Compliance.Item.ComplianceSub.Index)
            {
              if (!export.Compliance.Item.ComplianceSub.CheckSize())
              {
                break;
              }

              if (Equal(export.Compliance.Item.ComplianceSub.Item.
                GexportComplianceSub.MedType, local.PersonProgram.MedType))
              {
                goto ReadEach;
              }
            }

            export.Compliance.Item.ComplianceSub.CheckIndex();

            if (export.Compliance.Item.ComplianceSub.Count >= Export
              .ComplianceSubGroup.Capacity)
            {
              local.EabReportSend.RptDetail =
                "ABEND:  Group view overflow; View = EXPORT GROUP COMPLIANCE SUB";
                
              UseCabErrorReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            export.Compliance.Item.ComplianceSub.Index =
              export.Compliance.Item.ComplianceSub.Count;
            export.Compliance.Item.ComplianceSub.CheckSize();

            export.Compliance.Update.ComplianceSub.Update.GexportComplianceSub.
              MedType = local.PersonProgram.MedType ?? "";
          }
        }

        export.Compliance.CheckIndex();

        if (export.Compliance.Count >= Export.ComplianceGroup.Capacity)
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Group view overflow; View = EXPORT GROUP COMPLIANCE";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        export.Compliance.Index = export.Compliance.Count;
        export.Compliance.CheckSize();

        export.Compliance.Update.GexportCompliance.Code = local.Program.Code;

        if (IsEmpty(local.Dash.Text1))
        {
          continue;
        }

        export.Compliance.Item.ComplianceSub.Index = 0;
        export.Compliance.Item.ComplianceSub.CheckSize();

        export.Compliance.Update.ComplianceSub.Update.GexportComplianceSub.
          MedType = local.PersonProgram.MedType ?? "";

ReadEach:
        ;
      }

      if (export.Compliance.Count == 0)
      {
        local.EabReportSend.RptDetail =
          "ABEND:  No Code_Values for code; Code = COMPLIANCE PROGRAMS";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      export.Compliance.Index = 0;

      for(var limit = export.Compliance.Count; export.Compliance.Index < limit; ++
        export.Compliance.Index)
      {
        if (!export.Compliance.CheckSize())
        {
          break;
        }

        if (ReadProgram1())
        {
          export.Compliance.Update.GexportCompliance.SystemGeneratedIdentifier =
            entities.Program.SystemGeneratedIdentifier;
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Program not found for Code_Value; Code_Value = " + export
            .Compliance.Item.GexportCompliance.Code;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }

      export.Compliance.CheckIndex();
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Code name not found; Code = COMPLIANCE PROGRAMS";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // mjr
    // ------------------------------------------------------
    // Write COMPLIANCE PROGRAMS to control report
    // ---------------------------------------------------------
    local.EabReportSend.RptDetail = "COMPLIANCE PROGRAMS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    export.Compliance.Index = 0;

    for(var limit = export.Compliance.Count; export.Compliance.Index < limit; ++
      export.Compliance.Index)
    {
      if (!export.Compliance.CheckSize())
      {
        break;
      }

      local.EabReportSend.RptDetail =
        export.Compliance.Item.GexportCompliance.Code;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }

      export.Compliance.Item.ComplianceSub.Index = 0;

      for(var limit1 = export.Compliance.Item.ComplianceSub.Count; export
        .Compliance.Item.ComplianceSub.Index < limit1; ++
        export.Compliance.Item.ComplianceSub.Index)
      {
        if (!export.Compliance.Item.ComplianceSub.CheckSize())
        {
          break;
        }

        local.EabReportSend.RptDetail = "  -" + (
          export.Compliance.Item.ComplianceSub.Item.GexportComplianceSub.
            MedType ?? "");
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabReportSend.RptDetail = "Error writing to Control Report";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }

      export.Compliance.Item.ComplianceSub.CheckIndex();
    }

    export.Compliance.CheckIndex();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCode()
  {
    entities.Code.Populated = false;

    return Read("ReadCode",
      null,
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.Code.Id);
        db.SetDate(
          command, "effectiveDate",
          export.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(
          command, "code", export.Compliance.Item.GexportCompliance.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram2",
      null,
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
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
    /// <summary>A ComplianceGroup group.</summary>
    [Serializable]
    public class ComplianceGroup
    {
      /// <summary>
      /// A value of GexportCompliance.
      /// </summary>
      [JsonPropertyName("gexportCompliance")]
      public Program GexportCompliance
      {
        get => gexportCompliance ??= new();
        set => gexportCompliance = value;
      }

      /// <summary>
      /// Gets a value of ComplianceSub.
      /// </summary>
      [JsonIgnore]
      public Array<ComplianceSubGroup> ComplianceSub => complianceSub ??= new(
        ComplianceSubGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of ComplianceSub for json serialization.
      /// </summary>
      [JsonPropertyName("complianceSub")]
      [Computed]
      public IList<ComplianceSubGroup> ComplianceSub_Json
      {
        get => complianceSub;
        set => ComplianceSub.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program gexportCompliance;
      private Array<ComplianceSubGroup> complianceSub;
    }

    /// <summary>A ComplianceSubGroup group.</summary>
    [Serializable]
    public class ComplianceSubGroup
    {
      /// <summary>
      /// A value of GexportComplianceSub.
      /// </summary>
      [JsonPropertyName("gexportComplianceSub")]
      public PersonProgram GexportComplianceSub
      {
        get => gexportComplianceSub ??= new();
        set => gexportComplianceSub = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private PersonProgram gexportComplianceSub;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of LastProcess.
    /// </summary>
    [JsonPropertyName("lastProcess")]
    public DateWorkArea LastProcess
    {
      get => lastProcess ??= new();
      set => lastProcess = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Program Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// Gets a value of Compliance.
    /// </summary>
    [JsonIgnore]
    public Array<ComplianceGroup> Compliance => compliance ??= new(
      ComplianceGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Compliance for json serialization.
    /// </summary>
    [JsonPropertyName("compliance")]
    [Computed]
    public IList<ComplianceGroup> Compliance_Json
    {
      get => compliance;
      set => Compliance.Assign(value);
    }

    private Case1 restart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea start;
    private DateWorkArea end;
    private Common debugOn;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea lastProcess;
    private Program af;
    private Array<ComplianceGroup> compliance;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Dash.
    /// </summary>
    [JsonPropertyName("dash")]
    public WorkArea Dash
    {
      get => dash ??= new();
      set => dash = value;
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
    /// A value of LastProcess.
    /// </summary>
    [JsonPropertyName("lastProcess")]
    public DateWorkArea LastProcess
    {
      get => lastProcess ??= new();
      set => lastProcess = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private PersonProgram personProgram;
    private WorkArea dash;
    private Program program;
    private DateWorkArea lastProcess;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea start;
    private DateWorkArea end;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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

    private CodeValue codeValue;
    private Code code;
    private Program program;
  }
#endregion
}
