// Program: OE_B487_WRITE_LOCATE_REQUESTS, ID: 374418060, model: 746.
// Short name: SWEE487B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B487_WRITE_LOCATE_REQUESTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB487WriteLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B487_WRITE_LOCATE_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB487WriteLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB487WriteLocateRequests.
  /// </summary>
  public OeB487WriteLocateRequests(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ----------------------------------------
    // 07/??/2001  SWSCBRS	?????		Initial Coding
    // 03/??/2001  SWSRPRM	WR # 291 	Add License Suspension
    // 03/05/2007  GVandy	PR261671	General cleanup and changes to make 
    // compatible with
    // 					re-write to SWEEB486.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // -----------------------------------------------------------------------------------------------
    // Retrieve the PPI info.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB487";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Control Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Output File
    // -----------------------------------------------------------------------------------------------
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabWriteLocateRequests2();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'oe_eab_write_locate_requests'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Extract parameters from the PPI record.
    // -----------------------------------------------------------------------------------------------
    // -- Extract Process License Suspension Only Flag (Position 1)
    local.ProcessLicSuspOnly.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    // -----------------------------------------------------------------------------------------------
    // Retrieve the Department of Health code value information.
    // -----------------------------------------------------------------------------------------------
    if (!ReadCodeValue())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Licensing Agencies Timeframes code value not found.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Write parameters to the control report
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail =
          "Process License Suspension Only Parameter = " + local
          .ProcessLicSuspOnly.Flag;
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writing PPI Parameters to the control report.";
          
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.LocateRequest.AgencyNumber = "0" + Substring
      (entities.CodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 7, 4);

    // -----------------------------------------------------------------------------------------------
    // Process each Locate/License Suspension record that is to be sent to 
    // Department of Health.
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadLocateRequest())
    {
      if (local.NumberOfUpdates.Count == 0)
      {
        // -- Put a HEADER record on the file to indicate if the send was a 
        // regular locate request or only license suspension.
        local.ExternalLocateRequest.RequestDate = local.Current.Date;
        local.ExternalLocateRequest.AgencyNumber =
          entities.LocateRequest.AgencyNumber;
        local.ExternalLocateRequest.DateOfBirth =
          entities.LocateRequest.DateOfBirth;
        local.ExternalLocateRequest.CsePersonNumber = "0000000001";
        local.ExternalLocateRequest.SequenceNumber = 1;

        if (AsChar(local.ProcessLicSuspOnly.Flag) == 'Y')
        {
          local.ExternalLocateRequest.Ssn = "LICENSE";
        }
        else
        {
          local.ExternalLocateRequest.Ssn = "LOCATE";
        }

        local.PassArea.FileInstruction = "WRITED";
        UseOeEabWriteLocateRequests1();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing header record to output file.";
          UseCabErrorReport2();
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // -- Mark the locate request as having been sent.
      try
      {
        UpdateLocateRequest();
        ++local.NumberOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Locate Request record Not Unique.  CSP Number = " + entities
              .LocateRequest.CsePersonNumber;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.PermittedValueViolation:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Locate Request record Permitted Value Violation.  CSP Number = " +
              entities.LocateRequest.CsePersonNumber;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // -- Raise events indicating that the person has been sent for locate/
      // license suspension.
      if (AsChar(local.ProcessLicSuspOnly.Flag) == 'Y')
      {
        // -- Parameter has been set to process ONLY License Suspension LOCATE 
        // REQUEST records.
        if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'W')
        {
          local.Infrastructure.EventId = 10;
          local.Infrastructure.UserId = "LOCATE";
          local.Infrastructure.ReasonCode = "LICSUSPSENT";
          local.Infrastructure.BusinessObjectCd = "LOC";
          local.Infrastructure.ReferenceDate =
            entities.LocateRequest.RequestDate;
          local.Infrastructure.CsePersonNumber =
            entities.LocateRequest.CsePersonNumber;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.Detail = "License Suspension sent to " + TrimEnd
            (entities.CodeValue.Description) + ", CSE Person: " + entities
            .LocateRequest.CsePersonNumber;
          UseSpCabCreateInfrastructure();
        }
        else
        {
          // -- No processing required
        }
      }
      else
      {
        // -- Parameter has been set to process ALL LOCATE REQUEST records.
        if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'W')
        {
          local.Infrastructure.EventId = 10;
          local.Infrastructure.UserId = "LOCATE";
          local.Infrastructure.ReasonCode = "LICSUSPSENT";
          local.Infrastructure.BusinessObjectCd = "LOC";
          local.Infrastructure.ReferenceDate =
            entities.LocateRequest.RequestDate;
          local.Infrastructure.CsePersonNumber =
            entities.LocateRequest.CsePersonNumber;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.Detail = "License Suspension sent to " + TrimEnd
            (entities.CodeValue.Description) + ", CSE Person: " + entities
            .LocateRequest.CsePersonNumber;
        }
        else
        {
          local.Infrastructure.EventId = 10;
          local.Infrastructure.UserId = "LOCATE";
          local.Infrastructure.ReasonCode = "LOCATESENT";
          local.Infrastructure.BusinessObjectCd = "LOC";
          local.Infrastructure.ReferenceDate =
            entities.LocateRequest.RequestDate;
          local.Infrastructure.CsePersonNumber =
            entities.LocateRequest.CsePersonNumber;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.Detail = "Locate Req sent to " + TrimEnd
            (entities.CodeValue.Description) + ", CSE Person: " + entities
            .LocateRequest.CsePersonNumber;
        }

        UseSpCabCreateInfrastructure();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Creating Event for Reason Code = " + local
          .Infrastructure.ReasonCode + ".  " + local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Write locate/license suspension info to the output file.
      local.ExternalLocateRequest.Ssn =
        entities.LocateRequest.SocialSecurityNumber ?? Spaces(9);
      local.ExternalLocateRequest.DateOfBirth =
        entities.LocateRequest.DateOfBirth;
      local.ExternalLocateRequest.RequestDate = local.Current.Date;
      local.ExternalLocateRequest.CsePersonNumber =
        entities.LocateRequest.CsePersonNumber;
      local.ExternalLocateRequest.AgencyNumber =
        entities.LocateRequest.AgencyNumber;
      local.PassArea.FileInstruction = "WRITED";
      UseOeEabWriteLocateRequests1();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in writing external file for 'oe_eab_write_locate_requests'.";
        UseCabErrorReport2();
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // -- fyi... the program has always committed after each output record.  I
      // did not change this.  GV 03/05/07.
      UseExtToDoACommit();

      if (local.PassArea.NumericReturnCode != 0)
      {
        ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Write totals to the control report.
    // -----------------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail =
      "Total Number of Locate Requests written to a file : " + NumberToString
      (local.NumberOfUpdates.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error while writting 'Total Number of Locate Requests written to a file :'";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Output File
    // -----------------------------------------------------------------------------------------------
    local.PassArea.FileInstruction = "CLOSE";
    UseOeEabWriteLocateRequests2();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in closing external file for 'oe_eab_write_locate_requests'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Control Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveExternalLocateRequest(ExternalLocateRequest source,
    ExternalLocateRequest target)
  {
    target.RequestDate = source.RequestDate;
    target.AgencyNumber = source.AgencyNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeEabWriteLocateRequests1()
  {
    var useImport = new OeEabWriteLocateRequests.Import();
    var useExport = new OeEabWriteLocateRequests.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveExternalLocateRequest(local.ExternalLocateRequest,
      useImport.ExternalLocateRequest);
    MoveExternal(local.PassArea, useExport.External);

    Call(OeEabWriteLocateRequests.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabWriteLocateRequests2()
  {
    var useImport = new OeEabWriteLocateRequests.Import();
    var useExport = new OeEabWriteLocateRequests.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveExternal(local.PassArea, useExport.External);

    Call(OeEabWriteLocateRequests.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest()
  {
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "requestDate", local.NullDate.Date.GetValueOrDefault());
        db.SetString(command, "agencyNumber", local.LocateRequest.AgencyNumber);
      },
      (db, reader) =>
      {
        entities.LocateRequest.SocialSecurityNumber =
          db.GetNullableString(reader, 0);
        entities.LocateRequest.DateOfBirth = db.GetNullableDate(reader, 1);
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 2);
        entities.LocateRequest.RequestDate = db.GetNullableDate(reader, 3);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 4);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 5);
        entities.LocateRequest.LastUpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 7);
        entities.LocateRequest.LicenseSuspensionInd =
          db.GetNullableString(reader, 8);
        entities.LocateRequest.Populated = true;

        return true;
      });
  }

  private void UpdateLocateRequest()
  {
    var requestDate = local.Current.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.LocateRequest.Populated = false;
    Update("UpdateLocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(command, "requestDate", requestDate);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(
          command, "csePersonNumber", entities.LocateRequest.CsePersonNumber);
        db.SetString(
          command, "agencyNumber", entities.LocateRequest.AgencyNumber);
        db.SetInt32(
          command, "sequenceNumber", entities.LocateRequest.SequenceNumber);
      });

    entities.LocateRequest.RequestDate = requestDate;
    entities.LocateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.LocateRequest.Populated = true;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ExternalLocateRequest.
    /// </summary>
    [JsonPropertyName("externalLocateRequest")]
    public ExternalLocateRequest ExternalLocateRequest
    {
      get => externalLocateRequest ??= new();
      set => externalLocateRequest = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LocateRequest Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of ProcessLicSuspOnly.
    /// </summary>
    [JsonPropertyName("processLicSuspOnly")]
    public Common ProcessLicSuspOnly
    {
      get => processLicSuspOnly ??= new();
      set => processLicSuspOnly = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private ExternalLocateRequest externalLocateRequest;
    private LocateRequest locateRequest;
    private LocateRequest previous;
    private DateWorkArea current;
    private Common numberOfUpdates;
    private External passArea;
    private DateWorkArea nullDate;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Infrastructure infrastructure;
    private Common processLicSuspOnly;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
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

    private LocateRequest locateRequest;
    private Code code;
    private CodeValue codeValue;
  }
#endregion
}
