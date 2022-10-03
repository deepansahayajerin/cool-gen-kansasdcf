// Program: LE_B600_REFRAL_FUNCTION_EXTRACT, ID: 374508593, model: 746.
// Short name: SWEL600B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B600_REFRAL_FUNCTION_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB600RefralFunctionExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B600_REFRAL_FUNCTION_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB600RefralFunctionExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB600RefralFunctionExtract.
  /// </summary>
  public LeB600RefralFunctionExtract(IContext context, Import import,
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
    // *********************************************************************
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 09/24/09  DDupree            CQ12650
    //                             CQ12650    Initial Development
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

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

    // ************************************************
    // *Call External to Open the Extract File.       *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeEabWriteCaseFunctionExtr3();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'le_eab_write_case_functionl_extract'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.FileOpened.Flag = "Y";

    // The reporting period is as fo the last day of the previous month that 
    // this program is being run.
    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8);
    local.Common.TotalInteger =
      StringToNumber(Substring(
        local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
      "01");
    local.FirstDayCurrentMonth.Date = IntToDate((int)local.Common.TotalInteger);
    local.LastDayPrevMonth.Date = AddDays(local.FirstDayCurrentMonth.Date, -1);

    // -- Find all legal referrals created during the reporting month.
    foreach(var item in ReadCase())
    {
      ++local.TotalNumOfRecordsRead.Count;
      local.CaseFunction.Text1 = "";
      UseSiCabReturnCaseFunction2();

      if (IsEmpty(local.CaseFuncWorkSet.FuncText1))
      {
        local.CsePersonsWorkSet.FormattedName = "";

        foreach(var item1 in ReadLegalReferral())
        {
          if (ReadServiceProviderOfficeLegalReferralAssignment())
          {
            local.Legal.Assign(local.Clear);

            if (Lt(entities.LegalReferralAssignment.DiscontinueDate,
              local.LastDayPrevMonth.Date) || Lt
              (entities.LegalOfficeServiceProvider.DiscontinueDate,
              local.LastDayPrevMonth.Date))
            {
              // if a legal service provider is no longer active then we still 
              // want to count the case
              // function but we will do it under a generic record. By setting 
              // the last name and
              // first name to all 'A's then we have assured that after the sort
              // that all service
              // providers not found will be sorted to the top of the file.
              local.CsePersonsWorkSet.FormattedName =
                "No active legal service provider";
            }
            else
            {
              local.CsePersonsWorkSet.FormattedName = "";
              local.CsePersonsWorkSet.FirstName =
                entities.LegalServiceProvider.FirstName;
              local.CsePersonsWorkSet.LastName =
                entities.LegalServiceProvider.LastName;
              UseSiFormatCsePersonName();
            }

            break;
          }
        }

        if (IsEmpty(local.CsePersonsWorkSet.FormattedName))
        {
          local.CsePersonsWorkSet.FormattedName =
            "No active legal service provider";
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Case function cannot be determined for case # " + entities
          .Case1.Number + " for LSPO :" + local
          .CsePersonsWorkSet.FormattedName;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      foreach(var item1 in ReadLegalReferral())
      {
        foreach(var item2 in ReadServiceProviderOfficeServiceProviderLegalReferralAssignment())
          
        {
          local.Legal.Assign(local.Clear);

          if (Lt(entities.LegalReferralAssignment.DiscontinueDate,
            local.LastDayPrevMonth.Date) || Lt
            (entities.LegalOfficeServiceProvider.DiscontinueDate,
            local.LastDayPrevMonth.Date))
          {
            // if a legal service provider is no longer active then we still 
            // want to count the case
            // function but we will do it under a generic record. By setting the
            // last name and
            // first name to all 'A's then we have assured that after the sort 
            // that all service
            // providers not found will be sorted to the top of the file.
            local.Legal.LastName = "AAAAA";
            local.Legal.FirstName = "AAAAA";
            local.Legal.SystemGeneratedId = 99999;
          }
          else
          {
            local.Legal.Assign(entities.LegalServiceProvider);
          }

          local.EabFileHandling.Action = "WRITE";
          UseLeEabWriteCaseFunctionExtr2();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing record in 'le_eab_write_case_function_extract'.";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.RecordProcessed.Count;

          goto ReadEach;
        }

ReadEach:
        ;
      }
    }

    local.EabReportSend.RptDetail = "Total Number Of Cases Read :  " + NumberToString
      (local.TotalNumOfRecordsRead.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Cases Read).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Legal Referrals based on Function of the Case Written :  " +
      NumberToString(local.RecordProcessed.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Legal Referrals Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number of Errors Written :           " + NumberToString
      (local.TotalErrors.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Errors Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************
      // *  Close Referral extract File           *
      // ******************************************
      local.EabFileHandling.Action = "CLOSE";
      UseLeEabWriteCaseFunctionExtr1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in closing external file  for 'le_write_case_function_extract'.";
          
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_CLOSE_ERROR";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

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

      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

  private void UseLeEabWriteCaseFunctionExtr1()
  {
    var useImport = new LeEabWriteCaseFunctionExtr.Import();
    var useExport = new LeEabWriteCaseFunctionExtr.Export();

    useImport.Legal.Assign(entities.LegalServiceProvider);
    useImport.Office.Name = entities.Office.Name;
    useImport.CaseFuncWorkSet.FuncText1 = local.CaseFuncWorkSet.FuncText1;
    useImport.Case1.Number = entities.Case1.Number;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.External);

    Call(LeEabWriteCaseFunctionExtr.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseLeEabWriteCaseFunctionExtr2()
  {
    var useImport = new LeEabWriteCaseFunctionExtr.Import();
    var useExport = new LeEabWriteCaseFunctionExtr.Export();

    useImport.Legal.Assign(local.Legal);
    useImport.Office.Name = entities.Office.Name;
    useImport.CaseFuncWorkSet.FuncText1 = local.CaseFuncWorkSet.FuncText1;
    useImport.Case1.Number = entities.Case1.Number;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.External);

    Call(LeEabWriteCaseFunctionExtr.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseLeEabWriteCaseFunctionExtr3()
  {
    var useImport = new LeEabWriteCaseFunctionExtr.Import();
    var useExport = new LeEabWriteCaseFunctionExtr.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.External);

    Call(LeEabWriteCaseFunctionExtr.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiCabReturnCaseFunction2()
  {
    var useImport = new SiCabReturnCaseFunction2.Import();
    var useExport = new SiCabReturnCaseFunction2.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.DateWorkArea.Date = local.LastDayPrevMonth.Date;

    Call(SiCabReturnCaseFunction2.Execute, useImport, useExport);

    local.CaseFuncWorkSet.Assign(useExport.CaseFuncWorkSet);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate",
          local.LastDayPrevMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "statusDate",
          local.LastDayPrevMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOfficeLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.Office.Populated = false;
    entities.LegalOfficeServiceProvider.Populated = false;
    entities.LegalReferralAssignment.Populated = false;
    entities.LegalServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeLegalReferralAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.LastDayPrevMonth.Date.GetValueOrDefault());
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.LegalOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.LegalServiceProvider.LastName = db.GetString(reader, 1);
        entities.LegalServiceProvider.FirstName = db.GetString(reader, 2);
        entities.LegalServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.LegalOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.Office.Name = db.GetString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 7);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 11);
        entities.LegalOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 11);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 12);
        entities.LegalOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 12);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 13);
        entities.LegalOfficeServiceProvider.RoleCode = db.GetString(reader, 13);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 14);
        entities.LegalOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 14);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 15);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 16);
        entities.LegalOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 17);
        entities.Office.Populated = true;
        entities.LegalOfficeServiceProvider.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
        entities.LegalServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadServiceProviderOfficeServiceProviderLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.Office.Populated = false;
    entities.LegalOfficeServiceProvider.Populated = false;
    entities.LegalReferralAssignment.Populated = false;
    entities.LegalServiceProvider.Populated = false;

    return ReadEach(
      "ReadServiceProviderOfficeServiceProviderLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(
          command, "effectiveDate",
          local.LastDayPrevMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.LegalOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 0);
        entities.LegalServiceProvider.LastName = db.GetString(reader, 1);
        entities.LegalServiceProvider.FirstName = db.GetString(reader, 2);
        entities.LegalServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.LegalOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 4);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.LegalOfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 5);
        entities.LegalOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 6);
        entities.LegalOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 8);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 12);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 13);
        entities.Office.Name = db.GetString(reader, 14);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 15);
        entities.Office.Populated = true;
        entities.LegalOfficeServiceProvider.Populated = true;
        entities.LegalReferralAssignment.Populated = true;
        entities.LegalServiceProvider.Populated = true;

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
    /// A value of TotalNumOfRecordsRead.
    /// </summary>
    [JsonPropertyName("totalNumOfRecordsRead")]
    public Common TotalNumOfRecordsRead
    {
      get => totalNumOfRecordsRead ??= new();
      set => totalNumOfRecordsRead = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public ServiceProvider Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of Legal.
    /// </summary>
    [JsonPropertyName("legal")]
    public ServiceProvider Legal
    {
      get => legal ??= new();
      set => legal = value;
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
    /// A value of CaseFunction.
    /// </summary>
    [JsonPropertyName("caseFunction")]
    public WorkArea CaseFunction
    {
      get => caseFunction ??= new();
      set => caseFunction = value;
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
    /// A value of LastDayPrevMonth.
    /// </summary>
    [JsonPropertyName("lastDayPrevMonth")]
    public DateWorkArea LastDayPrevMonth
    {
      get => lastDayPrevMonth ??= new();
      set => lastDayPrevMonth = value;
    }

    /// <summary>
    /// A value of FirstDayCurrentMonth.
    /// </summary>
    [JsonPropertyName("firstDayCurrentMonth")]
    public DateWorkArea FirstDayCurrentMonth
    {
      get => firstDayCurrentMonth ??= new();
      set => firstDayCurrentMonth = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
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
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public Common TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ServiceProvider Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common totalNumOfRecordsRead;
    private CsePersonsWorkSet csePersonsWorkSet;
    private External external;
    private ServiceProvider clear;
    private ServiceProvider legal;
    private Common common;
    private DateWorkArea dateWorkArea;
    private WorkArea caseFunction;
    private CaseFuncWorkSet caseFuncWorkSet;
    private DateWorkArea lastDayPrevMonth;
    private DateWorkArea firstDayCurrentMonth;
    private Office area;
    private AbendData abendData;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fileOpened;
    private DateWorkArea end;
    private DateWorkArea start;
    private Common totalErrors;
    private ServiceProvider null1;
    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("legalServiceProviderProfile")]
    public ServiceProviderProfile LegalServiceProviderProfile
    {
      get => legalServiceProviderProfile ??= new();
      set => legalServiceProviderProfile = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LegalOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("legalOfficeServiceProvider")]
    public OfficeServiceProvider LegalOfficeServiceProvider
    {
      get => legalOfficeServiceProvider ??= new();
      set => legalOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalServiceProvider.
    /// </summary>
    [JsonPropertyName("legalServiceProvider")]
    public ServiceProvider LegalServiceProvider
    {
      get => legalServiceProvider ??= new();
      set => legalServiceProvider = value;
    }

    private ServiceProviderProfile legalServiceProviderProfile;
    private Office office;
    private Case1 case1;
    private OfficeServiceProvider legalOfficeServiceProvider;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private ServiceProvider legalServiceProvider;
  }
#endregion
}
