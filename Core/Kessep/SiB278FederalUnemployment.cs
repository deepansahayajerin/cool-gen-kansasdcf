// Program: SI_B278_FEDERAL_UNEMPLOYMENT, ID: 373313408, model: 746.
// Short name: SWEI278B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B278_FEDERAL_UNEMPLOYMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB278FederalUnemployment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B278_FEDERAL_UNEMPLOYMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB278FederalUnemployment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB278FederalUnemployment.
  /// </summary>
  public SiB278FederalUnemployment(IContext context, Import import,
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
    // ***********************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // *
    // 
    // *
    // * 02/25/2002  Ed Lyman   PR139866    Initial Coding.                  *
    // *
    // 
    // *
    // * 04/25/2002  Ed Lyman   PR144264    Fix decimal point and check for  *
    // *
    // 
    // duplicates.                      *
    // *
    // 
    // *
    // * 02/02/2009  Raj S        CQ114     Modified to generate worker alert*
    // *
    // 
    // for NDNH response SSN & CSE SSNs *
    // *
    // 
    // mismatches.                      *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);

    local.AlternateSsn.Index = 0;
    local.AlternateSsn.CheckSize();

    local.Names.Index = 0;
    local.Names.CheckSize();

    UseSiB278Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadFedQtrlyUnemplIncome();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING FEDERAL QUARTERLY UNEMPLOYMENT INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;
      local.Kaecses.Number = local.FederalCaseRegistry.Number;

      if (ReadCsePerson())
      {
        UseSiCabReadAdabasBatch();

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ADABAS_INVALID_SSN_W_RB"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          break;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }
      }
      else
      {
        ++local.RecordsPersonNotFound.Count;
        local.NeededToWrite.RptDetail = "Person Not Found - AP # " + local
          .FederalCaseRegistry.Number + "  " + TrimEnd
          (local.FederalCaseRegistry.LastName) + ", " + TrimEnd
          (local.FederalCaseRegistry.FirstName) + " " + "" + local
          .FederalCaseRegistry.MiddleInitial;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

      if (!Equal(local.FederalCaseRegistry.Ssn, local.Kaecses.Ssn))
      {
        local.Batch.Flag = "Y";
        UseCabRetrieveAliasesAndAltSsn();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(local.AlternateSsn.Index = 0; local.AlternateSsn.Index < local
          .AlternateSsn.Count; ++local.AlternateSsn.Index)
        {
          if (!local.AlternateSsn.CheckSize())
          {
            break;
          }

          if (Equal(local.FederalCaseRegistry.Ssn,
            local.AlternateSsn.Item.Gssn.Ssn))
          {
            local.Employment.Note = "Matched alternate ssn: " + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-" + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-" + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            local.Employment.Note = TrimEnd(local.Employment.Note) + " name on W-2: " +
              TrimEnd(local.FederalCaseRegistry.LastName) + ", " + TrimEnd
              (local.FederalCaseRegistry.FirstName) + " " + local
              .FederalCaseRegistry.MiddleInitial;

            goto Test;
          }
        }

        local.AlternateSsn.CheckIndex();
        ++local.RecordsSkippedSsn.Count;

        // ***************************************************************************
        // Write to report of ssn's don't match.  Skip to next person.
        // ***************************************************************************
        UseSiB273SsnMismatchReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        // **************************************************************************************
        // Generate worker alert for mismatched SSNs by using the Action Block
        // SI_B273_SSN_MISMATCH_ALERTS_GEN.    CQ114  Changes Start
        // **************************************************************************************
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReasonCode = "FCRNEWSSNNDNHUI";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.ReferenceDate = local.Process.Date;
        local.Infrastructure.UserId = local.ProgramProcessingInfo.Name;
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.BusinessObjectCd = "FCR";
        local.Infrastructure.CreatedBy = global.UserId;
        local.Infrastructure.LastUpdatedBy = "";
        local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        local.Infrastructure.Detail = "SSN:" + TrimEnd
          (local.FederalCaseRegistry.Ssn) + ", PF16 to view Employer Name (expires in 1 Yr)";
          
        ExitState = "ACO_NN0000_ALL_OK";
        local.NarrativeDetail.NarrativeText =
          "**Employer Record from NDNH UI through SWEIB278 Batch Process**";
        UseSiB273SsnMismatchAlertsGen();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // **************************************************************************************
        // Generate worker alert for mismatched SSNs by using the Action Block
        // SI_B273_SSN_MISMATCH_ALERTS_GEN.    CQ114  Changes End
        // **************************************************************************************
        continue;
      }

Test:

      if (IsEmpty(local.FederalCaseRegistry.Flag))
      {
        ++local.EmployeeNameMismatch.Count;

        if (IsEmpty(local.Employment.Note))
        {
          local.Employment.Note = TrimEnd("") + "Employee name on W-2 is: " + TrimEnd
            (local.FederalCaseRegistry.LastName) + ", " + TrimEnd
            (local.FederalCaseRegistry.FirstName) + " " + local
            .FederalCaseRegistry.MiddleInitial;
        }

        // ***************************************************************************
        // Write to report of names don't match.  Continue processing.
        // ***************************************************************************
        UseSiB273EmployeeNameMismatch();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }
      }

      // ****************************************************************************
      // At this point, for the current record, no creates or updates have been 
      // done.
      // ****************************************************************************
      switch(AsChar(local.ReportingQuarter.Text1))
      {
        case '1':
          local.BeginQuarter.Date = IntToDate(local.ReportingYear.Year * 10000
            + 101);

          break;
        case '2':
          local.BeginQuarter.Date = IntToDate(local.ReportingYear.Year * 10000
            + 401);

          break;
        case '3':
          local.BeginQuarter.Date = IntToDate(local.ReportingYear.Year * 10000
            + 701);

          break;
        case '4':
          local.BeginQuarter.Date = IntToDate(local.ReportingYear.Year * 10000
            + 1001);

          break;
        default:
          ++local.RecordsAlreadyProcessed.Count;

          continue;
      }

      // *******************************************************************
      // ***  See if unemployment reporting quarter is within 6 months before 
      // sending alert ***
      // *******************************************************************
      if (Lt(AddMonths(local.Process.Date, -6), local.BeginQuarter.Date))
      {
        ++local.Commit.Count;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.BenefitAmount.AverageCurrency * 100), 15);
          
        UseEabConvertNumeric1();
        local.TextYear.Text4 = NumberToString(local.ReportingYear.Year, 12, 4);
        local.Search.State =
          (int)StringToNumber(local.ReportingState.ActionEntry);
        local.Search.County = 0;
        local.Search.Location = 0;

        if (ReadFips())
        {
          local.Infrastructure.Detail = local.ReportingQuarter.Text1 + "Q" + local
            .TextYear.Text4 + " " + TrimEnd(entities.Fips.StateDescription) + " BENEFIT AMOUNT" +
            Substring
            (local.EabConvertNumeric.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 12, 10);
        }
        else
        {
          local.Infrastructure.Detail = local.ReportingQuarter.Text1 + "Q" + local
            .TextYear.Text4 + " " + TrimEnd
            (local.ReportingState.ActionEntry) + " BENEFIT AMOUNT" + Substring
            (local.EabConvertNumeric.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 12, 10);
        }

        UseSiB278SendAlertUnemployBene();
      }
      else
      {
        ++local.RecordsOlderThan6Mos.Count;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // *************** Check to see if commit is needed ********************
      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
          (Now().Date.Month, 14, 2) + "-" + NumberToString
          (Now().Date.Day, 14, 2);
        local.Time.Text8 = NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
        local.NeededToWrite.RptDetail = "Commit taken after record number: " + NumberToString
          (local.RecordsRead.Count, 15) + "  Date: " + local.Date.Text10 + "  Time: " +
          local.Time.Text8;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB278Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB278Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveAlternateSsn(CabRetrieveAliasesAndAltSsn.Export.
    AlternateSsnGroup source, Local.AlternateSsnGroup target)
  {
    target.Gssn.Ssn = source.Gssn.Ssn;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnCurrencyNegInParens = source.ReturnCurrencyNegInParens;
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
    target.ReturnOkFlag = source.ReturnOkFlag;
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.Ein = source.Ein;
    target.KansasId = source.KansasId;
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveEmployerAddress(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Identifier = source.Identifier;
    target.Note = source.Note;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.Country = source.Country;
    target.PostalCode = source.PostalCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveNames(CabRetrieveAliasesAndAltSsn.Export.
    NamesGroup source, Local.NamesGroup target)
  {
    target.Gname.Assign(source.Gnames);
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabRetrieveAliasesAndAltSsn()
  {
    var useImport = new CabRetrieveAliasesAndAltSsn.Import();
    var useExport = new CabRetrieveAliasesAndAltSsn.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.CsePersonsWorkSet.Assign(local.Kaecses);

    Call(CabRetrieveAliasesAndAltSsn.Execute, useImport, useExport);

    useExport.Names.CopyTo(local.Names, MoveNames);
    useExport.AlternateSsn.CopyTo(local.AlternateSsn, MoveAlternateSsn);
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.Assign(local.EabConvertNumeric);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadFedQtrlyUnemplIncome()
  {
    var useImport = new EabReadFedQtrlyUnemplIncome.Import();
    var useExport = new EabReadFedQtrlyUnemplIncome.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEmployer(local.Employer, useExport.Employer);
    MoveEmployerAddress(local.EmployerAddress, useExport.EmployerAddress);
    useExport.CsePersonsWorkSet.Assign(local.FederalCaseRegistry);
    useExport.ReportingState.ActionEntry = local.ReportingState.ActionEntry;
    useExport.BenefitAmount.AverageCurrency =
      local.BenefitAmount.AverageCurrency;
    useExport.SsnMatchIndicator.Flag = local.SsnMatchIndicator.Flag;
    useExport.ReportingYear.Year = local.ReportingYear.Year;
    useExport.ReportingQuarter.Text1 = local.ReportingQuarter.Text1;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFedQtrlyUnemplIncome.Execute, useImport, useExport);

    local.Employer.Assign(useExport.Employer);
    local.EmployerAddress.Assign(useExport.EmployerAddress);
    local.FederalCaseRegistry.Assign(useExport.CsePersonsWorkSet);
    local.ReportingState.ActionEntry = useExport.ReportingState.ActionEntry;
    local.BenefitAmount.AverageCurrency =
      useExport.BenefitAmount.AverageCurrency;
    local.SsnMatchIndicator.Flag = useExport.SsnMatchIndicator.Flag;
    local.ReportingYear.Year = useExport.ReportingYear.Year;
    local.ReportingQuarter.Text1 = useExport.ReportingQuarter.Text1;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB273EmployeeNameMismatch()
  {
    var useImport = new SiB273EmployeeNameMismatch.Import();
    var useExport = new SiB273EmployeeNameMismatch.Export();

    useImport.Kaecses.Assign(local.Kaecses);
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);

    Call(SiB273EmployeeNameMismatch.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273SsnMismatchAlertsGen()
  {
    var useImport = new SiB273SsnMismatchAlertsGen.Import();
    var useExport = new SiB273SsnMismatchAlertsGen.Export();

    useImport.WageAmount.AverageCurrency = local.BenefitAmount.AverageCurrency;
    useImport.Max.Date = local.Max.Date;
    useImport.Employer.Assign(local.Employer);
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.EmployerSourceTxt.NarrativeText =
      local.NarrativeDetail.NarrativeText;

    Call(SiB273SsnMismatchAlertsGen.Execute, useImport, useExport);
  }

  private void UseSiB273SsnMismatchReport()
  {
    var useImport = new SiB273SsnMismatchReport.Import();
    var useExport = new SiB273SsnMismatchReport.Export();

    useImport.Kaecses.Assign(local.Kaecses);
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);

    Call(SiB273SsnMismatchReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB278Close()
  {
    var useImport = new SiB278Close.Import();
    var useExport = new SiB278Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.AlertsSent.Count = local.AlertsSent.Count;
    useImport.RecordsSkippedSsn.Count = local.RecordsSkippedSsn.Count;
    useImport.RecordsPersonNotFound.Count = local.RecordsPersonNotFound.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.RecordsOlderThan6Mos.Count = local.RecordsOlderThan6Mos.Count;

    Call(SiB278Close.Execute, useImport, useExport);
  }

  private void UseSiB278Housekeeping()
  {
    var useImport = new SiB278Housekeeping.Import();
    var useExport = new SiB278Housekeeping.Export();

    Call(SiB278Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseSiB278SendAlertUnemployBene()
  {
    var useImport = new SiB278SendAlertUnemployBene.Import();
    var useExport = new SiB278SendAlertUnemployBene.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.AlertsSent.Count = local.AlertsSent.Count;
    useImport.AlreadyProcessed.Count = local.RecordsAlreadyProcessed.Count;

    Call(SiB278SendAlertUnemployBene.Execute, useImport, useExport);

    local.AlertsSent.Count = useExport.AlertsSent.Count;
    local.RecordsAlreadyProcessed.Count = useExport.AlreadyProcessed.Count;
  }

  private void UseSiCabReadAdabasBatch()
  {
    var useImport = new SiCabReadAdabasBatch.Import();
    var useExport = new SiCabReadAdabasBatch.Export();

    useImport.Obligor.Number = local.Kaecses.Number;

    Call(SiCabReadAdabasBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Obligor, local.Kaecses);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.FederalCaseRegistry.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.Search.State);
        db.SetInt32(command, "county", local.Search.County);
        db.SetInt32(command, "location", local.Search.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.Populated = true;
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
    /// <summary>A NamesGroup group.</summary>
    [Serializable]
    public class NamesGroup
    {
      /// <summary>
      /// A value of Gname.
      /// </summary>
      [JsonPropertyName("gname")]
      public CsePersonsWorkSet Gname
      {
        get => gname ??= new();
        set => gname = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gname;
    }

    /// <summary>A AlternateSsnGroup group.</summary>
    [Serializable]
    public class AlternateSsnGroup
    {
      /// <summary>
      /// A value of Gssn.
      /// </summary>
      [JsonPropertyName("gssn")]
      public CsePersonsWorkSet Gssn
      {
        get => gssn ??= new();
        set => gssn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gssn;
    }

    /// <summary>
    /// A value of RecordsOlderThan6Mos.
    /// </summary>
    [JsonPropertyName("recordsOlderThan6Mos")]
    public Common RecordsOlderThan6Mos
    {
      get => recordsOlderThan6Mos ??= new();
      set => recordsOlderThan6Mos = value;
    }

    /// <summary>
    /// A value of TextYear.
    /// </summary>
    [JsonPropertyName("textYear")]
    public TextWorkArea TextYear
    {
      get => textYear ??= new();
      set => textYear = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Fips Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of AlertsSent.
    /// </summary>
    [JsonPropertyName("alertsSent")]
    public Common AlertsSent
    {
      get => alertsSent ??= new();
      set => alertsSent = value;
    }

    /// <summary>
    /// A value of EmployeeNameMismatch.
    /// </summary>
    [JsonPropertyName("employeeNameMismatch")]
    public Common EmployeeNameMismatch
    {
      get => employeeNameMismatch ??= new();
      set => employeeNameMismatch = value;
    }

    /// <summary>
    /// A value of RecordsSkippedSsn.
    /// </summary>
    [JsonPropertyName("recordsSkippedSsn")]
    public Common RecordsSkippedSsn
    {
      get => recordsSkippedSsn ??= new();
      set => recordsSkippedSsn = value;
    }

    /// <summary>
    /// A value of RecordsPersonNotFound.
    /// </summary>
    [JsonPropertyName("recordsPersonNotFound")]
    public Common RecordsPersonNotFound
    {
      get => recordsPersonNotFound ??= new();
      set => recordsPersonNotFound = value;
    }

    /// <summary>
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of ReportingYear.
    /// </summary>
    [JsonPropertyName("reportingYear")]
    public DateWorkArea ReportingYear
    {
      get => reportingYear ??= new();
      set => reportingYear = value;
    }

    /// <summary>
    /// A value of ReportingQuarter.
    /// </summary>
    [JsonPropertyName("reportingQuarter")]
    public WorkArea ReportingQuarter
    {
      get => reportingQuarter ??= new();
      set => reportingQuarter = value;
    }

    /// <summary>
    /// A value of SsnMatchIndicator.
    /// </summary>
    [JsonPropertyName("ssnMatchIndicator")]
    public Common SsnMatchIndicator
    {
      get => ssnMatchIndicator ??= new();
      set => ssnMatchIndicator = value;
    }

    /// <summary>
    /// A value of ReportingState.
    /// </summary>
    [JsonPropertyName("reportingState")]
    public Common ReportingState
    {
      get => reportingState ??= new();
      set => reportingState = value;
    }

    /// <summary>
    /// A value of PreviouslyCompared.
    /// </summary>
    [JsonPropertyName("previouslyCompared")]
    public Employer PreviouslyCompared
    {
      get => previouslyCompared ??= new();
      set => previouslyCompared = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public DateWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of BenefitAmount.
    /// </summary>
    [JsonPropertyName("benefitAmount")]
    public Common BenefitAmount
    {
      get => benefitAmount ??= new();
      set => benefitAmount = value;
    }

    /// <summary>
    /// A value of Quarter.
    /// </summary>
    [JsonPropertyName("quarter")]
    public TextWorkArea Quarter
    {
      get => quarter ??= new();
      set => quarter = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// Gets a value of Names.
    /// </summary>
    [JsonIgnore]
    public Array<NamesGroup> Names => names ??= new(NamesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Names for json serialization.
    /// </summary>
    [JsonPropertyName("names")]
    [Computed]
    public IList<NamesGroup> Names_Json
    {
      get => names;
      set => Names.Assign(value);
    }

    /// <summary>
    /// Gets a value of AlternateSsn.
    /// </summary>
    [JsonIgnore]
    public Array<AlternateSsnGroup> AlternateSsn => alternateSsn ??= new(
      AlternateSsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AlternateSsn for json serialization.
    /// </summary>
    [JsonPropertyName("alternateSsn")]
    [Computed]
    public IList<AlternateSsnGroup> AlternateSsn_Json
    {
      get => alternateSsn;
      set => AlternateSsn.Assign(value);
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of EmployerInfoMismatch.
    /// </summary>
    [JsonPropertyName("employerInfoMismatch")]
    public Common EmployerInfoMismatch
    {
      get => employerInfoMismatch ??= new();
      set => employerInfoMismatch = value;
    }

    /// <summary>
    /// A value of EmployersCreated.
    /// </summary>
    [JsonPropertyName("employersCreated")]
    public Common EmployersCreated
    {
      get => employersCreated ??= new();
      set => employersCreated = value;
    }

    /// <summary>
    /// A value of IncomeSourcesCreated.
    /// </summary>
    [JsonPropertyName("incomeSourcesCreated")]
    public Common IncomeSourcesCreated
    {
      get => incomeSourcesCreated ??= new();
      set => incomeSourcesCreated = value;
    }

    /// <summary>
    /// A value of Valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public Common Valid
    {
      get => valid ??= new();
      set => valid = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of BeginQuarter.
    /// </summary>
    [JsonPropertyName("beginQuarter")]
    public DateWorkArea BeginQuarter
    {
      get => beginQuarter ??= new();
      set => beginQuarter = value;
    }

    /// <summary>
    /// A value of DateOfHire.
    /// </summary>
    [JsonPropertyName("dateOfHire")]
    public TextWorkArea DateOfHire
    {
      get => dateOfHire ??= new();
      set => dateOfHire = value;
    }

    /// <summary>
    /// A value of StateHiredInCsePersonAddress.
    /// </summary>
    [JsonPropertyName("stateHiredInCsePersonAddress")]
    public CsePersonAddress StateHiredInCsePersonAddress
    {
      get => stateHiredInCsePersonAddress ??= new();
      set => stateHiredInCsePersonAddress = value;
    }

    /// <summary>
    /// A value of StateHiredInTextWorkArea.
    /// </summary>
    [JsonPropertyName("stateHiredInTextWorkArea")]
    public TextWorkArea StateHiredInTextWorkArea
    {
      get => stateHiredInTextWorkArea ??= new();
      set => stateHiredInTextWorkArea = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public CsePersonsWorkSet Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
    }

    /// <summary>
    /// A value of FederalCaseRegistry.
    /// </summary>
    [JsonPropertyName("federalCaseRegistry")]
    public CsePersonsWorkSet FederalCaseRegistry
    {
      get => federalCaseRegistry ??= new();
      set => federalCaseRegistry = value;
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
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    private Common recordsOlderThan6Mos;
    private TextWorkArea textYear;
    private EabConvertNumeric2 eabConvertNumeric;
    private Fips search;
    private Common recordsRead;
    private Common alertsSent;
    private Common employeeNameMismatch;
    private Common recordsSkippedSsn;
    private Common recordsPersonNotFound;
    private Common recordsAlreadyProcessed;
    private DateWorkArea reportingYear;
    private WorkArea reportingQuarter;
    private Common ssnMatchIndicator;
    private Common reportingState;
    private Employer previouslyCompared;
    private DateWorkArea year;
    private Common benefitAmount;
    private TextWorkArea quarter;
    private Common batch;
    private Array<NamesGroup> names;
    private Array<AlternateSsnGroup> alternateSsn;
    private TextWorkArea time;
    private TextWorkArea date;
    private Common employerInfoMismatch;
    private Common employersCreated;
    private Common incomeSourcesCreated;
    private Common valid;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private TextWorkArea recordType;
    private DateWorkArea beginQuarter;
    private TextWorkArea dateOfHire;
    private CsePersonAddress stateHiredInCsePersonAddress;
    private TextWorkArea stateHiredInTextWorkArea;
    private Employer employer;
    private EabReportSend neededToWrite;
    private CsePersonsWorkSet kaecses;
    private CsePersonsWorkSet federalCaseRegistry;
    private EabFileHandling eabFileHandling;
    private IncomeSource employment;
    private AbendData abendData;
    private Infrastructure infrastructure;
    private ProgramProcessingInfo programProcessingInfo;
    private EmployerAddress employerAddress;
    private NarrativeDetail narrativeDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Fips fips;
    private CsePerson csePerson;
  }
#endregion
}
