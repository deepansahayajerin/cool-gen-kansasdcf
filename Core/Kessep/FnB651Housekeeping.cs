// Program: FN_B651_HOUSEKEEPING, ID: 373526174, model: 746.
// Short name: SWE02602
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB651Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651Housekeeping.
  /// </summary>
  public FnB651Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	PR	 Developer          Description
    // 00-02-15  PR 86861  Fangman           Initial code
    // 00-04-05  URA       Fangman          Add logic to get the length of time 
    // for URA disbursement suppressions.
    // 00-09-07  PR 103323  Fangman  Added max date & added exp persistent disb 
    // type views to decrease I/O.  This was put in with the changes to fix the
    // disb suppr with past discontinue dates.
    // 00-09-27  PR 98039  Fangman  Duplicate Payment - added code to read the "
    // Denied" payment status.
    // 19-08-12  CQ65423  GVandy	Open business reports 1 and 2.  Also open <tab>
    // delimited file.
    // ---------------------------------------------
    // ***** Get the run parameters for this program.
    export.ProgramProcessingInfo.Name = "SWEFB651";
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.TestRunInd.Flag =
        Substring(export.ProgramProcessingInfo.ParameterList, 1, 1);
      export.TestDisplayInd.Flag =
        Substring(export.ProgramProcessingInfo.ParameterList, 3, 1);

      if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 5, 10)))
        
      {
        export.TestFirstObligee.Number =
          TrimEnd(Substring(export.ProgramProcessingInfo.ParameterList, 5, 10));
          
      }

      if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 16, 10)))
        
      {
        export.TestLastObligee.Number =
          TrimEnd(Substring(export.ProgramProcessingInfo.ParameterList, 16, 10));
          
      }
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 01
    // **********************************************************
    local.EabReportSend.RptHeading3 =
      "     SRRUN184 - SYSTEM SUPPRESSIONS - DAILY ERROR REPORT";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    UseCabBusinessReport01();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 02
    // **********************************************************
    local.EabReportSend.RptHeading3 =
      "          SRRUN184 - SYSTEM SUPPRESSIONS - ERROR REPORT";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    UseCabBusinessReport02();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT <tab> DELIMITED FILE
    // **********************************************************
    UseEabWrite255CharFile2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // **********************************************************
    // WRITE HEADER ROW TO THE <tab> DELIMITED FILE
    // **********************************************************
    local.Data255CharacterTextRecord.Data =
      "NEW TODAY\tSUPPRESSION REASON\tDISB TRAN ID\tREFERENCE NUMBER\tAMOUNT\tDISB DATE\tPAYEE #\tPAYEE NAME\tDESIGNATED PAYEE #\tCASE\tWORKER\tOFFICE\tCONTRACTOR\t";
      
    UseEabWrite255CharFile1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (AsChar(export.TestRunInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Test run - no updates will be applied to the database.";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.TestDisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Display indicator is on - diagnostic information will be displayed.";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (!IsEmpty(export.TestFirstObligee.Number) || !
      IsEmpty(export.TestLastObligee.Number))
    {
      local.EabReportSend.RptDetail =
        "Processing disbursements for Obligees between: " + export
        .TestFirstObligee.Number + " and " + export.TestLastObligee.Number;
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.TestRunInd.Flag) == 'Y' || AsChar
      (export.TestDisplayInd.Flag) == 'Y' || !
      IsEmpty(export.TestFirstObligee.Number) || !
      IsEmpty(export.TestLastObligee.Number))
    {
      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (!Equal(global.UserId, export.ProgramProcessingInfo.Name))
    {
      local.EabReportSend.RptDetail =
        "Severe Error:  User_ID should be set to SWEFB650 instead of " + global
        .UserId + ".  This is usually due to an error in the generation/installation.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // ***** Get the DB2 commit frequency counts.
    export.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Fatal error occurred, must abort.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }

      return;
    }

    export.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.EabReportSend.RptDetail = "";

    if (!ReadDisbursementType1())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"AF CCS\" Disbursement Type with ID of 1.";
    }

    if (!ReadDisbursementType2())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"AF ACS\" Disbursement Type with ID of 2.";
    }

    if (!ReadDisbursementType3())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"NA CCS\" Disbursement Type with ID of 4.";
    }

    if (!ReadDisbursementType4())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"NA ACS\" Disbursement Type with ID of 5.";
    }

    if (!ReadDisbursementType5())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"CR FEE\" Disbursement Type with ID of 73.";
    }

    if (!ReadDisbursementStatus1())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"RELEASED\" Disbursement Status with ID of 1.";
        
    }

    if (!ReadDisbursementStatus2())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"PROCESSED\" Disbursement Status with ID of 2.";
        
    }

    if (!ReadDisbursementStatus3())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"SUPPRESSED\" Disbursement Status with ID of 3.";
        
    }

    if (!ReadDisbursementTranRlnRsn())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading Disbursement Transaction Relation Reason with ID of 1.";
        
    }

    if (!ReadPaymentStatus())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"Denied\" Payment Status with ID of 23.";
    }

    if (ReadControlTable2())
    {
      export.UraSuppressionLength.LastUsedNumber =
        entities.ControlTable.LastUsedNumber;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading Control table w/ ID of 'URA SUPPRESSION LENGTH'.";
        
    }

    if (ReadControlTable1())
    {
      export.ErrorsBeforeAbend.LastUsedNumber =
        entities.ControlTable.LastUsedNumber;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading Control table w/ ID of 'SWEFB651 ERRORS BEFORE ABEND'.";
        
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabWrite255CharFile1()
  {
    var useImport = new EabWrite255CharFile.Import();
    var useExport = new EabWrite255CharFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Data255CharacterTextRecord.Data =
      local.Data255CharacterTextRecord.Data;
    useExport.EabFileHandling.Status = local.Status.Status;

    Call(EabWrite255CharFile.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWrite255CharFile2()
  {
    var useImport = new EabWrite255CharFile.Import();
    var useExport = new EabWrite255CharFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.Status.Status;

    Call(EabWrite255CharFile.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadControlTable1()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable1",
      null,
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
      });
  }

  private bool ReadControlTable2()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable2",
      null,
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
      });
  }

  private bool ReadDisbursementStatus1()
  {
    export.Per1Released.Populated = false;

    return Read("ReadDisbursementStatus1",
      null,
      (db, reader) =>
      {
        export.Per1Released.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per1Released.Populated = true;
      });
  }

  private bool ReadDisbursementStatus2()
  {
    export.Per2Processed.Populated = false;

    return Read("ReadDisbursementStatus2",
      null,
      (db, reader) =>
      {
        export.Per2Processed.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per2Processed.Populated = true;
      });
  }

  private bool ReadDisbursementStatus3()
  {
    export.Per3Suppressed.Populated = false;

    return Read("ReadDisbursementStatus3",
      null,
      (db, reader) =>
      {
        export.Per3Suppressed.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.Per3Suppressed.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    export.Per1.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      null,
      (db, reader) =>
      {
        export.Per1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per1.Populated = true;
      });
  }

  private bool ReadDisbursementType1()
  {
    export.Per1AfCcs.Populated = false;

    return Read("ReadDisbursementType1",
      null,
      (db, reader) =>
      {
        export.Per1AfCcs.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per1AfCcs.Populated = true;
      });
  }

  private bool ReadDisbursementType2()
  {
    export.Per2AfAcs.Populated = false;

    return Read("ReadDisbursementType2",
      null,
      (db, reader) =>
      {
        export.Per2AfAcs.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per2AfAcs.Populated = true;
      });
  }

  private bool ReadDisbursementType3()
  {
    export.Per4NaCcs.Populated = false;

    return Read("ReadDisbursementType3",
      null,
      (db, reader) =>
      {
        export.Per4NaCcs.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per4NaCcs.Populated = true;
      });
  }

  private bool ReadDisbursementType4()
  {
    export.Per5NaAcs.Populated = false;

    return Read("ReadDisbursementType4",
      null,
      (db, reader) =>
      {
        export.Per5NaAcs.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per5NaAcs.Populated = true;
      });
  }

  private bool ReadDisbursementType5()
  {
    export.Per73CrFee.Populated = false;

    return Read("ReadDisbursementType5",
      null,
      (db, reader) =>
      {
        export.Per73CrFee.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per73CrFee.Populated = true;
      });
  }

  private bool ReadPaymentStatus()
  {
    export.Per23Denied.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        export.Per23Denied.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.Per23Denied.Populated = true;
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
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
    }

    /// <summary>
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    /// <summary>
    /// A value of TestFirstObligee.
    /// </summary>
    [JsonPropertyName("testFirstObligee")]
    public CsePerson TestFirstObligee
    {
      get => testFirstObligee ??= new();
      set => testFirstObligee = value;
    }

    /// <summary>
    /// A value of TestLastObligee.
    /// </summary>
    [JsonPropertyName("testLastObligee")]
    public CsePerson TestLastObligee
    {
      get => testLastObligee ??= new();
      set => testLastObligee = value;
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
    /// A value of Per1AfCcs.
    /// </summary>
    [JsonPropertyName("per1AfCcs")]
    public DisbursementType Per1AfCcs
    {
      get => per1AfCcs ??= new();
      set => per1AfCcs = value;
    }

    /// <summary>
    /// A value of Per2AfAcs.
    /// </summary>
    [JsonPropertyName("per2AfAcs")]
    public DisbursementType Per2AfAcs
    {
      get => per2AfAcs ??= new();
      set => per2AfAcs = value;
    }

    /// <summary>
    /// A value of Per4NaCcs.
    /// </summary>
    [JsonPropertyName("per4NaCcs")]
    public DisbursementType Per4NaCcs
    {
      get => per4NaCcs ??= new();
      set => per4NaCcs = value;
    }

    /// <summary>
    /// A value of Per5NaAcs.
    /// </summary>
    [JsonPropertyName("per5NaAcs")]
    public DisbursementType Per5NaAcs
    {
      get => per5NaAcs ??= new();
      set => per5NaAcs = value;
    }

    /// <summary>
    /// A value of Per73CrFee.
    /// </summary>
    [JsonPropertyName("per73CrFee")]
    public DisbursementType Per73CrFee
    {
      get => per73CrFee ??= new();
      set => per73CrFee = value;
    }

    /// <summary>
    /// A value of Per1Released.
    /// </summary>
    [JsonPropertyName("per1Released")]
    public DisbursementStatus Per1Released
    {
      get => per1Released ??= new();
      set => per1Released = value;
    }

    /// <summary>
    /// A value of Per2Processed.
    /// </summary>
    [JsonPropertyName("per2Processed")]
    public DisbursementStatus Per2Processed
    {
      get => per2Processed ??= new();
      set => per2Processed = value;
    }

    /// <summary>
    /// A value of Per3Suppressed.
    /// </summary>
    [JsonPropertyName("per3Suppressed")]
    public DisbursementStatus Per3Suppressed
    {
      get => per3Suppressed ??= new();
      set => per3Suppressed = value;
    }

    /// <summary>
    /// A value of Per23Denied.
    /// </summary>
    [JsonPropertyName("per23Denied")]
    public PaymentStatus Per23Denied
    {
      get => per23Denied ??= new();
      set => per23Denied = value;
    }

    /// <summary>
    /// A value of Per1.
    /// </summary>
    [JsonPropertyName("per1")]
    public DisbursementTranRlnRsn Per1
    {
      get => per1 ??= new();
      set => per1 = value;
    }

    /// <summary>
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
    }

    /// <summary>
    /// A value of ErrorsBeforeAbend.
    /// </summary>
    [JsonPropertyName("errorsBeforeAbend")]
    public ControlTable ErrorsBeforeAbend
    {
      get => errorsBeforeAbend ??= new();
      set => errorsBeforeAbend = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common testRunInd;
    private Common testDisplayInd;
    private CsePerson testFirstObligee;
    private CsePerson testLastObligee;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DisbursementType per1AfCcs;
    private DisbursementType per2AfAcs;
    private DisbursementType per4NaCcs;
    private DisbursementType per5NaAcs;
    private DisbursementType per73CrFee;
    private DisbursementStatus per1Released;
    private DisbursementStatus per2Processed;
    private DisbursementStatus per3Suppressed;
    private PaymentStatus per23Denied;
    private DisbursementTranRlnRsn per1;
    private ControlTable uraSuppressionLength;
    private ControlTable errorsBeforeAbend;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Data255CharacterTextRecord.
    /// </summary>
    [JsonPropertyName("data255CharacterTextRecord")]
    public Data255CharacterTextRecord Data255CharacterTextRecord
    {
      get => data255CharacterTextRecord ??= new();
      set => data255CharacterTextRecord = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private Data255CharacterTextRecord data255CharacterTextRecord;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
  }
#endregion
}
