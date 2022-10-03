// Program: SP_B730_MASS_MAILING_DOC_GEN, ID: 373509230, model: 746.
// Short name: SWEB730P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_B730_MASS_MAILING_DOC_GEN.
/// </para>
/// <para>
/// Input : DB2
/// Output : DB2
/// External Commit.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB730MassMailingDocGen: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B730_MASS_MAILING_DOC_GEN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB730MassMailingDocGen(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB730MassMailingDocGen.
  /// </summary>
  public SpB730MassMailingDocGen(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------
    //  Date		Developer	Request #      Description
    // --------------------------------------------------------------------------------------
    // 02/05/2003	A Doty	        WR# 030251     Initial Development
    // 12/05/2011      RMathews        CQ31405        Modified to process legal 
    // action id for 2011 holiday letter.
    //                                                
    // Edits bypassed due to time constraints.
    // 06/10/2013      RMathews        CQ39342        Modified to process child'
    // s person number.
    // --------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB730Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.InputFile.Action = "OPEN";
    UseSpEabB730ProcessInputFile2();

    if (!Equal(local.InputFile.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      return;
    }

    local.InputFile.Action = "READ";
    local.MoreData.Flag = "Y";

    while(AsChar(local.MoreData.Flag) == 'Y')
    {
      UseSpEabB730ProcessInputFile1();

      switch(TrimEnd(local.InputFile.Status))
      {
        case "OK":
          ++local.CheckpointRead.Count;

          if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
          {
            if (IsEmpty(local.RestartCsePerson.Number))
            {
              if (Equal(local.RestartCase.Number, local.Case1.Number) && Equal
                (local.RestartCaseRole.Type1, local.CaseRole.Type1))
              {
                local.ProgramCheckpointRestart.RestartInd = "N";
              }
            }
            else if (IsEmpty(local.RestartCase.Number))
            {
              if (Equal(local.RestartCsePerson.Number, local.CsePerson.Number) &&
                Equal(local.RestartCaseRole.Type1, local.CaseRole.Type1))
              {
                local.ProgramCheckpointRestart.RestartInd = "N";
              }
            }
            else if (IsEmpty(local.RestartCaseRole.Type1))
            {
              if (Equal(local.RestartCsePerson.Number, local.CsePerson.Number) &&
                Equal(local.RestartCase.Number, local.Case1.Number))
              {
                local.ProgramCheckpointRestart.RestartInd = "N";
              }
            }
            else if (Equal(local.RestartCsePerson.Number, local.CsePerson.Number)
              && Equal(local.RestartCase.Number, local.Case1.Number) && Equal
              (local.RestartCaseRole.Type1, local.CaseRole.Type1))
            {
              local.ProgramCheckpointRestart.RestartInd = "N";
            }

            continue;
          }

          ++local.CntCsePersonsRead.Count;

          break;
        case "EOF":
          local.MoreData.Flag = "N";

          goto AfterCycle;
        default:
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
      }

      // : Edit check input record
      if (IsEmpty(local.CsePerson.Number))
      {
        if (IsEmpty(local.Case1.Number))
        {
          if (IsEmpty(local.CaseRole.Type1))
          {
            local.EabReportSend.RptDetail =
              "ERROR: Input record does not contain a person number, case number or case role.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            continue;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "ERROR: Input record does not contain a person number or case number.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            continue;
          }
        }
        else if (IsEmpty(local.CaseRole.Type1))
        {
          local.EabReportSend.RptDetail =
            "ERROR: Input record does not contain a person number or case role, Case Number: " +
            local.Case1.Number;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          continue;
        }
        else if (ReadCsePerson1())
        {
          local.CsePerson.Number = entities.ExistingCsePerson.Number;
        }
        else
        {
          if (ReadCase2())
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid case role, Case Number: " + local.Case1.Number + ", Case Role: " +
              local.CaseRole.Type1;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid case number, Case Number: " + local
              .Case1.Number + ", Case Role: " + local.CaseRole.Type1;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
        }
      }
      else if (IsEmpty(local.Case1.Number))
      {
        if (IsEmpty(local.CaseRole.Type1))
        {
          local.EabReportSend.RptDetail =
            "ERROR: Input record does not contain a case number or case role, Person Number: " +
            local.CsePerson.Number;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          continue;
        }
        else if (ReadCase1())
        {
          local.Case1.Number = entities.ExistingCase.Number;
        }
        else
        {
          if (ReadCsePerson2())
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid case role, Person Number: " + local
              .CsePerson.Number + ", Case Role: " + local.CaseRole.Type1;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid person number, Person Number: " + local
              .CsePerson.Number + ", Case Role: " + local.CaseRole.Type1;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
        }
      }
      else if (IsEmpty(local.CaseRole.Type1))
      {
        if (!ReadCsePersonCase2())
        {
          if (ReadCsePerson2())
          {
            if (ReadCase2())
            {
              local.EabReportSend.RptDetail =
                "ERROR: Person number and case number are not related, Person Number: " +
                local.CsePerson.Number + ", Case Number: " + local
                .Case1.Number;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
            else
            {
              local.EabReportSend.RptDetail =
                "ERROR: Invalid case number, Person Number: " + local
                .CsePerson.Number + ", Case Number: " + local.Case1.Number;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid person number, Person Number: " + local
              .CsePerson.Number + ", Case Number: " + local.Case1.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
        }
      }
      else if (!ReadCsePersonCase1())
      {
        if (ReadCsePerson2())
        {
          if (ReadCase2())
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid case role, Person Number: " + local
              .CsePerson.Number + ", Case Number: " + entities
              .ExistingCase.Number + ", Case Role: " + local.CaseRole.Type1;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
          else
          {
            local.EabReportSend.RptDetail =
              "ERROR: Invalid case number, Person Number: " + local
              .CsePerson.Number + ", Case Number: " + entities
              .ExistingCase.Number + ", Case Role: " + local.CaseRole.Type1;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ERROR: Invalid person number, Person Number: " + local
            .CsePerson.Number + ", Case Number: " + entities
            .ExistingCase.Number + ", Case Role: " + local.CaseRole.Type1;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        continue;
      }

      local.SpDocKey.KeyPerson = local.CsePerson.Number;
      local.SpDocKey.KeyAp = local.CsePerson.Number;
      local.SpDocKey.KeyAr = local.CsePerson.Number;
      local.SpDocKey.KeyChild = local.Child.Number;
      local.SpDocKey.KeyCase = local.Case1.Number;
      local.SpDocKey.KeyLegalAction = local.LegalAction.Identifier;

      if (!ReadCsePersonAddress())
      {
        ++local.CntNoAddress.Count;
        local.EabReportSend.RptDetail =
          "WARNING: Active Verified Address Not Found - Person #: " + local
          .CsePerson.Number;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // -----------------------------------------------------------------
      // Create document_trigger
      // -----------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.ReferenceDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
      local.Infrastructure.CreatedTimestamp = local.Current.Timestamp;
      UseSpCreateDocumentInfrastruct();

      if (IsExitState("OFFICE_NF"))
      {
        local.EabReportSend.RptDetail =
          "ERROR: Error creating Document for Person; Person #: " + local
          .CsePerson.Number + ", Case #: " + local.Case1.Number + ", Case Role: " +
          local.CaseRole.Type1 + " - Office was not found.";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.CntDocsCreated.Count;
      }
      else
      {
        ++local.CntDocCreationError.Count;
        local.EabReportSend.RptDetail =
          "ERROR: Error creating Document for Person; Person #: " + local
          .CsePerson.Number + ", Case #: " + local.Case1.Number + ", Case Role: " +
          local.CaseRole.Type1;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        UseEabExtractExitStateMessage1();

        if (local.Infrastructure.SystemGeneratedIdentifier <= 0)
        {
          // --------------------------------------------------------
          // Errors that occur before an infrastructure record is
          // created, create an ABEND.
          // --------------------------------------------------------
          local.EabReportSend.RptDetail = "SYSTEM ERROR: " + local
            .ExitStateWorkArea.Message;
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
          NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // -----------------------------------------------------------------------
      // Commit processing
      // -----------------------------------------------------------------------
      if (local.CheckpointRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // ----------------------------------------------------------------
        // Restart info in this program is Person Number, Case Number and Role 
        // Type
        // ----------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo = "PRSN: " + local
          .CsePerson.Number + "CASE_NO: " + local.Case1.Number + "CR_TYPE: " + local
          .CaseRole.Type1;
        local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to update program_checkpoint_restart.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
        else
        {
          local.ProgramCheckpointRestart.RestartInd = "";
          local.ProgramCheckpointRestart.RestartInfo = "";
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to commit database.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        // -------------------------------------------------------------
        // Reset checkpoint counter
        // -------------------------------------------------------------
        local.CheckpointRead.Count = 0;
        local.EabReportSend.RptDetail = "-- CHECKPOINT --";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabReportSend.RptDetail =
          "SYSTEM ERROR:  Unable to update program_checkpoint_restart.";
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
    else
    {
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();
      local.EabReportSend.RptDetail = "Error in processing, Exit State: " + local
        .ExitStateWorkArea.Message;

      // -----------------------------------------------------------
      // Ending as an ABEND
      // -----------------------------------------------------------
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.InputFile.Action = "CLOSE";
    UseSpEabB730ProcessInputFile2();

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB730WriteTotalsAndClose();
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyAr = source.KeyAr;
    target.KeyCase = source.KeyCase;
    target.KeyChild = source.KeyChild;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyPerson = source.KeyPerson;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseEabExtractExitStateMessage1()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private string UseEabExtractExitStateMessage2()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpB730Housekeeping()
  {
    var useImport = new SpB730Housekeeping.Import();
    var useExport = new SpB730Housekeeping.Export();

    Call(SpB730Housekeeping.Execute, useImport, useExport);

    local.RestartCase.Number = useExport.RestartCase.Number;
    local.RestartCaseRole.Type1 = useExport.RestartCaseRole.Type1;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.Current.Timestamp = useExport.Current.Timestamp;
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.Document.Name = useExport.Document.Name;

    local.RestartCsePerson.Number = useExport.RestartCsePerson.Number;
  }

  private void UseSpB730WriteTotalsAndClose()
  {
    var useImport = new SpB730WriteTotalsAndClose.Import();
    var useExport = new SpB730WriteTotalsAndClose.Export();

    useImport.CsePersonsRead.Count = local.CntCsePersonsRead.Count;
    useImport.NoAddress.Count = local.CntNoAddress.Count;
    useImport.DocCreates.Count = local.CntDocsCreated.Count;
    useImport.DocErrors.Count = local.CntDocCreationError.Count;

    Call(SpB730WriteTotalsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.Document.Name = local.Document.Name;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpEabB730ProcessInputFile1()
  {
    var useImport = new SpEabB730ProcessInputFile.Import();
    var useExport = new SpEabB730ProcessInputFile.Export();

    useImport.EabFileHandling.Action = local.InputFile.Action;
    useExport.LegalAction.Identifier = local.LegalAction.Identifier;
    useExport.Case1.Number = local.Case1.Number;
    useExport.CaseRole.Type1 = local.CaseRole.Type1;
    useExport.EabFileHandling.Status = local.InputFile.Status;
    useExport.CsePerson.Number = local.CsePerson.Number;
    useExport.Child.Number = local.Child.Number;

    Call(SpEabB730ProcessInputFile.Execute, useImport, useExport);

    local.LegalAction.Identifier = useExport.LegalAction.Identifier;
    local.Case1.Number = useExport.Case1.Number;
    local.CaseRole.Type1 = useExport.CaseRole.Type1;
    local.InputFile.Status = useExport.EabFileHandling.Status;
    local.CsePerson.Number = useExport.CsePerson.Number;
    local.Child.Number = useExport.Child.Number;
  }

  private void UseSpEabB730ProcessInputFile2()
  {
    var useImport = new SpEabB730ProcessInputFile.Import();
    var useExport = new SpEabB730ProcessInputFile.Export();

    useImport.EabFileHandling.Action = local.InputFile.Action;
    useExport.EabFileHandling.Status = local.InputFile.Status;

    Call(SpEabB730ProcessInputFile.Execute, useImport, useExport);

    local.InputFile.Status = useExport.EabFileHandling.Status;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCase1()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.ExistingCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonAddress.Identifier =
          db.GetDateTime(reader, 0);
        entities.ExistingCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ExistingCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingCsePersonAddress.EndDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCsePersonAddress.Populated = true;
      });
  }

  private bool ReadCsePersonCase1()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePersonCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
        db.SetString(command, "casNumber", local.Case1.Number);
        db.SetString(command, "type", local.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 1);
        entities.ExistingCase.Populated = true;
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonCase2()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePersonCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 1);
        entities.ExistingCase.Populated = true;
        entities.ExistingCsePerson.Populated = true;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of RestartCaseRole.
    /// </summary>
    [JsonPropertyName("restartCaseRole")]
    public CaseRole RestartCaseRole
    {
      get => restartCaseRole ??= new();
      set => restartCaseRole = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of InputFile.
    /// </summary>
    [JsonPropertyName("inputFile")]
    public EabFileHandling InputFile
    {
      get => inputFile ??= new();
      set => inputFile = value;
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
    /// A value of MoreData.
    /// </summary>
    [JsonPropertyName("moreData")]
    public Common MoreData
    {
      get => moreData ??= new();
      set => moreData = value;
    }

    /// <summary>
    /// A value of RestartRecordCnt.
    /// </summary>
    [JsonPropertyName("restartRecordCnt")]
    public Common RestartRecordCnt
    {
      get => restartRecordCnt ??= new();
      set => restartRecordCnt = value;
    }

    /// <summary>
    /// A value of CheckpointRead.
    /// </summary>
    [JsonPropertyName("checkpointRead")]
    public Common CheckpointRead
    {
      get => checkpointRead ??= new();
      set => checkpointRead = value;
    }

    /// <summary>
    /// A value of CntCsePersonsRead.
    /// </summary>
    [JsonPropertyName("cntCsePersonsRead")]
    public Common CntCsePersonsRead
    {
      get => cntCsePersonsRead ??= new();
      set => cntCsePersonsRead = value;
    }

    /// <summary>
    /// A value of CntNoAddress.
    /// </summary>
    [JsonPropertyName("cntNoAddress")]
    public Common CntNoAddress
    {
      get => cntNoAddress ??= new();
      set => cntNoAddress = value;
    }

    /// <summary>
    /// A value of CntDocsCreated.
    /// </summary>
    [JsonPropertyName("cntDocsCreated")]
    public Common CntDocsCreated
    {
      get => cntDocsCreated ??= new();
      set => cntDocsCreated = value;
    }

    /// <summary>
    /// A value of CntDocCreationError.
    /// </summary>
    [JsonPropertyName("cntDocCreationError")]
    public Common CntDocCreationError
    {
      get => cntDocCreationError ??= new();
      set => cntDocCreationError = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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

    private CsePerson child;
    private LegalAction legalAction;
    private Case1 restartCase;
    private CaseRole restartCaseRole;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson restartCsePerson;
    private EabFileHandling inputFile;
    private CsePerson csePerson;
    private Common moreData;
    private Common restartRecordCnt;
    private Common checkpointRead;
    private Common cntCsePersonsRead;
    private Common cntNoAddress;
    private Common cntDocsCreated;
    private Common cntDocCreationError;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private DateWorkArea null1;
    private Document document;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common debugOn;
    private EabConvertNumeric2 eabConvertNumeric;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonAddress.
    /// </summary>
    [JsonPropertyName("existingCsePersonAddress")]
    public CsePersonAddress ExistingCsePersonAddress
    {
      get => existingCsePersonAddress ??= new();
      set => existingCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    private CsePerson existingChild;
    private LegalAction existingLegalAction;
    private Case1 existingCase;
    private CaseRole existingCaseRole;
    private CsePerson existingCsePerson;
    private CsePersonAddress existingCsePersonAddress;
    private Infrastructure existingInfrastructure;
    private InterstateRequest existingInterstateRequest;
  }
#endregion
}
