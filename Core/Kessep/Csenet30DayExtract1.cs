// Program: CSENET_30_DAY_EXTRACT, ID: 372943809, model: 746.
// Short name: SWEI720B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CSENET_30_DAY_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class Csenet30DayExtract1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CSENET_30_DAY_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Csenet30DayExtract1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Csenet30DayExtract1.
  /// </summary>
  public Csenet30DayExtract1(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    // *
    // 
    // *
    // * C H A N G E   L O G
    // 
    // *
    // * ===================
    // 
    // *
    // *
    // 
    // *
    // *******************************************************************
    // *
    // 
    // *
    // *   Date   Name      PR#  Reason
    // 
    // *
    // *   ----   ----      ---  ------
    // 
    // *
    // * Sept 99                 
    // Production
    // 
    // *
    // *
    // 
    // *
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.PpiFound.Flag = "N";

    // ***
    // *** get each Program Processing Info for SWEIB720
    // ***
    if (ReadProgramProcessingInfo())
    {
      local.PpiFound.Flag = "Y";
      local.Current.Date = entities.ProgramProcessingInfo.ProcessDate;
      local.Overdue.Date = AddDays(local.Current.Date, -30);
    }

    // ***
    // *** OPEN the Error Report
    // ***
    export.EabFileHandling.Action = "OPEN";
    export.NeededToOpen.ProgramName = "SWEIB720";
    export.NeededToOpen.ProcessDate =
      entities.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    if (AsChar(local.PpiFound.Flag) == 'N')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail = "Program Processing Info not found";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    // ***
    // *** OPEN the CSENET 30 DAY extract file
    // ***
    export.ReportParms.Parm1 = "OF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet30DayFileWriter2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail =
        "Error Opening the CSENET 30 Day Extract File";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    // ***
    // *** get each Interstate Case meeting selection criteria
    // ***
    foreach(var item in ReadInterstateCase())
    {
      export.Csenet30DayExtract.Assign(local.NullCsenet30DayExtract);
      export.Csenet30DayExtract.ReceivedDate =
        entities.InterstateCase.TransactionDate;
      export.Csenet30DayExtract.ReferralNumber =
        entities.InterstateCase.TransSerialNumber;

      if (AsChar(entities.InterstateCase.ActionCode) == 'U')
      {
        export.Csenet30DayExtract.ReferralType = "UPDATE";
      }
      else
      {
        export.Csenet30DayExtract.ReferralType = "RESPONSE";
      }

      // ***
      // *** get each Interstate Case Assignment for current Interstate Case
      // ***
      foreach(var item1 in ReadInterstateCaseAssignment())
      {
        // ***
        // *** get Service Provider through Office Service Provider for
        // *** current Interstate Case Assignment
        // ***
        if (ReadServiceProvider())
        {
          export.Csenet30DayExtract.ServiceProvider =
            TrimEnd(entities.ServiceProvider.LastName) + ", " + TrimEnd
            (entities.ServiceProvider.FirstName) + " " + entities
            .ServiceProvider.MiddleInitial;
        }
        else
        {
          // ***
          // *** WRITE to the Error Report
          // ***
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            "Service Provider not found for Interstate Case Assignment for Interstate Case " +
            NumberToString(entities.InterstateCase.TransSerialNumber, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          goto ReadEach;
        }

        // ***
        // *** get Office through Office Service Provider for
        // *** current Interstate Case Assignment
        // ***
        if (ReadOffice())
        {
          export.Csenet30DayExtract.Office = entities.Office.Name;
        }
        else
        {
          // ***
          // *** WRITE to the Error Report
          // ***
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            "Office not found for Interstate Case Assignment for Interstate Case " +
            NumberToString(entities.InterstateCase.TransSerialNumber, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          goto ReadEach;
        }

        // ***
        // *** WRITE to the CSENET 30 Day extract file
        // ***
        export.ReportParms.Parm1 = "GR";
        export.ReportParms.Parm2 = "";
        UseEabCsenet30DayFileWriter1();

        if (!IsEmpty(local.ReportParms.Parm1))
        {
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          // ***
          // *** WRITE to the Error Report
          // ***
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            "Error Writing to the CSENET 30 Day Extract File";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // ***
          // *** CLOSE the Error Report
          // ***
          export.EabFileHandling.Action = "CLOSE";
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_CLOSING_FILE_AB";
          }

          return;
        }

        goto ReadEach;
      }

ReadEach:
      ;
    }

    // ***
    // *** CLOSE the CSENET 30 DAY extract file
    // ***
    export.ReportParms.Parm1 = "CF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet30DayFileWriter2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail =
        "Error Closing the CSENET 30 Day Extract File";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }
    }

    // ***
    // *** CLOSE the Error Report
    // ***
    export.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(export.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCsenet30DayFileWriter1()
  {
    var useImport = new EabCsenet30DayFileWriter.Import();
    var useExport = new EabCsenet30DayFileWriter.Export();

    useImport.Csenet30DayExtract.Assign(export.Csenet30DayExtract);
    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet30DayFileWriter.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCsenet30DayFileWriter2()
  {
    var useImport = new EabCsenet30DayFileWriter.Import();
    var useExport = new EabCsenet30DayFileWriter.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet30DayFileWriter.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private IEnumerable<bool> ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "assnDeactDt",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "transactionDate", local.Overdue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.ActionCode = db.GetString(reader, 1);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 2);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 3);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignment()
  {
    entities.InterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 6);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 8);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 9);
        entities.InterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.InterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.InterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.InterstateCaseAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.InterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.InterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.InterstateCaseAssignment.SpdId);
        db.SetDate(
          command, "effectiveDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
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
    /// A value of Csenet30DayExtract.
    /// </summary>
    [JsonPropertyName("csenet30DayExtract")]
    public Csenet30DayExtract2 Csenet30DayExtract
    {
      get => csenet30DayExtract ??= new();
      set => csenet30DayExtract = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private Csenet30DayExtract2 csenet30DayExtract;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ReportParms reportParms;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullCsenet30DayExtract.
    /// </summary>
    [JsonPropertyName("nullCsenet30DayExtract")]
    public Csenet30DayExtract2 NullCsenet30DayExtract
    {
      get => nullCsenet30DayExtract ??= new();
      set => nullCsenet30DayExtract = value;
    }

    /// <summary>
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of Overdue.
    /// </summary>
    [JsonPropertyName("overdue")]
    public DateWorkArea Overdue
    {
      get => overdue ??= new();
      set => overdue = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    private Csenet30DayExtract2 nullCsenet30DayExtract;
    private Common ppiFound;
    private EabFileHandling eabFileHandling;
    private ReportParms reportParms;
    private DateWorkArea overdue;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private Office office;
    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase interstateCase;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
  }
#endregion
}
