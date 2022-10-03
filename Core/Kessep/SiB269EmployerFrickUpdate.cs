// Program: SI_B269_EMPLOYER_FRICK_UPDATE, ID: 373411262, model: 746.
// Short name: SWEI269B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B269_EMPLOYER_FRICK_UPDATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB269EmployerFrickUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B269_EMPLOYER_FRICK_UPDATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB269EmployerFrickUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB269EmployerFrickUpdate.
  /// </summary>
  public SiB269EmployerFrickUpdate(IContext context, Import import,
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
    // * 05/20/2002  Ed Lyman   WR0020262   Initial Coding.                  *
    // *
    // 
    // *
    // * 07/08/2005  Ed Lyman   PR247899    Check for non numeric addresses. *
    // *
    // 
    // *
    // * 07/08/2005  Ed Lyman   PR159847    Expand note on employer address. *
    // *
    // 
    // *
    // *   /  /
    // 
    // *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseSiB269Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Aging.Month = Month(AddMonths(local.Process.Date, -6));
    local.Aging.Year = Year(AddMonths(local.Process.Date, -6));
    local.Process.Month = Month(local.Process.Date);
    local.Process.Year = Year(local.Process.Date);
    local.Date.Text2 = NumberToString(local.Process.Month, 14, 2);
    local.Date.Text4 = NumberToString(local.Process.Year, 12, 4);
    local.Date.Text8 = local.Date.Text2 + "/" + local.Date.Text4;

    // **********************************************************
    // Read each IWO address record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadFederalEmployerFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "ERROR READING EMPLOYER LOAD FILE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;
      UseSiB269ValidateEmployer();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        break;
      }

      if (AsChar(local.Valid.Flag) == 'N')
      {
        ++local.RecordsSkippedInvalid.Count;

        continue;
      }

      // *** Do we already have this employer's correct IWO address?
      //     If so go on. ***
      // *** If the name or address is different, put it on the mismatch report.
      //     Also if there is a verified income source referencing this 
      // employer,
      //     put it in the infrastructure table, so it can be displayed on the
      //     HIST screen. No alerts will be sent. Then go on. ***
      if (ReadEmployerEmployerAddress1())
      {
        if (Equal(entities.KaecsesEmployer.Name, local.LoadEmployer.Name) && Equal
          (entities.KaecsesEmployerAddress.Street1,
          local.LoadEmployerAddress.Street1) && Equal
          (entities.KaecsesEmployerAddress.Street2,
          local.LoadEmployerAddress.Street2) && Equal
          (entities.KaecsesEmployerAddress.City, local.LoadEmployerAddress.City) &&
          Equal
          (entities.KaecsesEmployerAddress.State,
          local.LoadEmployerAddress.State))
        {
        }
        else
        {
          UseSiB269EmployerInfoMismatch();

          // *** Put old address in History (infrastructure) for any AP that is
          //     currently employed and whose employment has been verified. ***
          foreach(var item in ReadIncomeSourceCsePerson1())
          {
            UseSiCabDetermineActiveIncome();

            if (AsChar(local.Active.Flag) != 'Y')
            {
              continue;
            }

            if (AsChar(local.ReportOnlyNoUpdates.Flag) == 'Y')
            {
              ++local.EventsCreated.Count;

              continue;
            }

            // **** CREATE EVENT HERE (no alert associated with this event) 
            // *********
            local.Infrastructure.Detail =
              TrimEnd(Substring(entities.KaecsesEmployer.Name, 1, 20)) + " " + TrimEnd
              (entities.KaecsesEmployerAddress.Street1) + ", " + TrimEnd("") + ""
              + TrimEnd(entities.KaecsesEmployerAddress.City) + ", " + entities
              .KaecsesEmployerAddress.State + " " + entities
              .KaecsesEmployerAddress.ZipCode;

            foreach(var item1 in ReadCase())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
              local.Infrastructure.ReasonCode = "OCSECHGADDR";
              local.Infrastructure.ProcessStatus = "Q";
              UseOeB412CreateInfrastructure1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto AfterCycle;
              }

              ++local.Commit.Count;
            }
          }

          ++local.EmployersUpdated.Count;

          if (AsChar(local.ReportOnlyNoUpdates.Flag) != 'Y')
          {
            // **** UPDATE EMPLOYER HERE *********
            local.LoadEmployerAddress.Note =
              "Obtained from OCSE as IWO address on " + local.Date.Text8;
            UseSiB269UpdateEmployer();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;
          }
        }

        continue;
      }

      foreach(var item in ReadEmployerEmployerAddress2())
      {
        UseSiB269EmployerInfoMismatch();
        ++local.EmployersUpdated.Count;

        if (AsChar(local.ReportOnlyNoUpdates.Flag) != 'Y')
        {
          // **** UPDATE EMPLOYER HERE *********
          local.LoadEmployerAddress.Note =
            "Obtained from OCSE as IWO address on " + local.Date.Text8;
          UseSiB269UpdateEmployer();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }
        }

        // *** Notify worker of employer address change ***
        foreach(var item1 in ReadIncomeSourceCsePerson2())
        {
          UseSiCabDetermineActiveIncome();

          if (AsChar(local.Active.Flag) != 'Y')
          {
            continue;
          }

          if (AsChar(local.ReportOnlyNoUpdates.Flag) == 'Y')
          {
            ++local.AlertsCreated.Count;
            ++local.EventsCreated.Count;

            continue;
          }

          // **** CREATE EVENT HERE (an alert is associated with this event) 
          // *********
          local.Infrastructure.Detail =
            "Address has been changed to the OCSE Employer IWO address. " + " ";
            

          foreach(var item2 in ReadCase())
          {
            ++local.EventsCreated.Count;
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.ReasonCode = "OCSEIWOAP";
            UseOeB412CreateInfrastructure2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            ++local.Commit.Count;
          }
        }
      }

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.Eab.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.EabReportSend.RptDetail =
          "Commit Taken after Commit Count reached: " + NumberToString
          (local.Commit.Count, 15);
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.Commit.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB269Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB269Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Month = source.Month;
    target.Year = source.Year;
  }

  private static void MoveEmployer1(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveEmployer2(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveEmployerAddress1(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Identifier = source.Identifier;
    target.Note = source.Note;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveEmployerAddress2(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Note = source.Note;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadFederalEmployerFile()
  {
    var useImport = new EabReadFederalEmployerFile.Import();
    var useExport = new EabReadFederalEmployerFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.ImportEmployer.Assign(local.LoadEmployer);
    MoveEmployerAddress2(local.LoadEmployerAddress,
      useExport.ImportEmployerAddress);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFederalEmployerFile.Execute, useImport, useExport);

    MoveEmployer2(useExport.ImportEmployer, local.LoadEmployer);
    MoveEmployerAddress2(useExport.ImportEmployerAddress,
      local.LoadEmployerAddress);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Eab.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Eab.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB412CreateInfrastructure1()
  {
    var useImport = new OeB412CreateInfrastructure.Import();
    var useExport = new OeB412CreateInfrastructure.Export();

    useImport.ItemsCreated.Count = local.EventsCreated.Count;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(OeB412CreateInfrastructure.Execute, useImport, useExport);

    local.EventsCreated.Count = useExport.ItemsCreated.Count;
  }

  private void UseOeB412CreateInfrastructure2()
  {
    var useImport = new OeB412CreateInfrastructure.Import();
    var useExport = new OeB412CreateInfrastructure.Export();

    useImport.ItemsCreated.Count = local.AlertsCreated.Count;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(OeB412CreateInfrastructure.Execute, useImport, useExport);

    local.AlertsCreated.Count = useExport.ItemsCreated.Count;
  }

  private void UseSiB269Close()
  {
    var useImport = new SiB269Close.Import();
    var useExport = new SiB269Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.EmployersCreated.Count = local.EmployersSkippedNf.Count;
    useImport.EmployersUpdated.Count = local.EmployersUpdated.Count;
    useImport.RecordsSkippedInvalid.Count = local.RecordsSkippedInvalid.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.EventsCreated.Count = local.EventsCreated.Count;
    useImport.AlertsCreated.Count = local.AlertsCreated.Count;

    Call(SiB269Close.Execute, useImport, useExport);
  }

  private void UseSiB269EmployerInfoMismatch()
  {
    var useImport = new SiB269EmployerInfoMismatch.Import();
    var useExport = new SiB269EmployerInfoMismatch.Export();

    useImport.KaecsesEmployerAddress.Assign(entities.KaecsesEmployerAddress);
    MoveEmployer1(local.LoadEmployer, useImport.FcrEmployer);
    useImport.FcrEmployerAddress.Assign(local.LoadEmployerAddress);
    MoveEmployer1(entities.KaecsesEmployer, useImport.KaecsesEmployer);

    Call(SiB269EmployerInfoMismatch.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB269Housekeeping()
  {
    var useImport = new SiB269Housekeeping.Import();
    var useExport = new SiB269Housekeeping.Export();

    Call(SiB269Housekeeping.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.Process.Date = useExport.Process.Date;
    local.ReportOnlyNoUpdates.Flag = useExport.ReportOnlyNoUpdates.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseSiB269UpdateEmployer()
  {
    var useImport = new SiB269UpdateEmployer.Import();
    var useExport = new SiB269UpdateEmployer.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.NewEmployer.Assign(local.LoadEmployer);
    useImport.NewEmployerAddress.Assign(local.LoadEmployerAddress);
    useImport.PersEmployer.Assign(entities.KaecsesEmployer);
    useImport.PersEmployerAddress.Assign(entities.KaecsesEmployerAddress);

    Call(SiB269UpdateEmployer.Execute, useImport, useExport);

    entities.KaecsesEmployer.Assign(useImport.PersEmployer);
    MoveEmployerAddress1(useImport.PersEmployerAddress,
      entities.KaecsesEmployerAddress);
  }

  private void UseSiB269ValidateEmployer()
  {
    var useImport = new SiB269ValidateEmployer.Import();
    var useExport = new SiB269ValidateEmployer.Export();

    useImport.EmployerAddress.Assign(local.LoadEmployerAddress);
    useImport.Employer.Assign(local.LoadEmployer);
    useImport.Process.Date = local.Process.Date;

    Call(SiB269ValidateEmployer.Execute, useImport, useExport);

    local.Valid.Flag = useExport.Valid.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiCabDetermineActiveIncome()
  {
    var useImport = new SiCabDetermineActiveIncome.Import();
    var useExport = new SiCabDetermineActiveIncome.Export();

    useImport.IncomeSource.Assign(entities.IncomeSource);
    MoveDateWorkArea(local.Aging, useImport.Aging);

    Call(SiCabDetermineActiveIncome.Execute, useImport, useExport);

    local.Active.Flag = useExport.Active.Flag;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadEmployerEmployerAddress1()
  {
    entities.KaecsesEmployer.Populated = false;
    entities.KaecsesEmployerAddress.Populated = false;

    return Read("ReadEmployerEmployerAddress1",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", local.LoadEmployer.Ein ?? "");
        db.SetNullableString(
          command, "zipCode", local.LoadEmployerAddress.ZipCode ?? "");
      },
      (db, reader) =>
      {
        entities.KaecsesEmployer.Identifier = db.GetInt32(reader, 0);
        entities.KaecsesEmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.KaecsesEmployer.Ein = db.GetNullableString(reader, 1);
        entities.KaecsesEmployer.KansasId = db.GetNullableString(reader, 2);
        entities.KaecsesEmployer.Name = db.GetNullableString(reader, 3);
        entities.KaecsesEmployer.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.KaecsesEmployer.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.KaecsesEmployer.PhoneNo = db.GetNullableString(reader, 6);
        entities.KaecsesEmployer.AreaCode = db.GetNullableInt32(reader, 7);
        entities.KaecsesEmployerAddress.LocationType = db.GetString(reader, 8);
        entities.KaecsesEmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.KaecsesEmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.KaecsesEmployerAddress.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.KaecsesEmployerAddress.CreatedBy = db.GetString(reader, 12);
        entities.KaecsesEmployerAddress.Street1 =
          db.GetNullableString(reader, 13);
        entities.KaecsesEmployerAddress.Street2 =
          db.GetNullableString(reader, 14);
        entities.KaecsesEmployerAddress.City = db.GetNullableString(reader, 15);
        entities.KaecsesEmployerAddress.Identifier = db.GetDateTime(reader, 16);
        entities.KaecsesEmployerAddress.State =
          db.GetNullableString(reader, 17);
        entities.KaecsesEmployerAddress.ZipCode =
          db.GetNullableString(reader, 18);
        entities.KaecsesEmployerAddress.Zip4 = db.GetNullableString(reader, 19);
        entities.KaecsesEmployerAddress.Zip3 = db.GetNullableString(reader, 20);
        entities.KaecsesEmployerAddress.County =
          db.GetNullableString(reader, 21);
        entities.KaecsesEmployerAddress.Note = db.GetNullableString(reader, 22);
        entities.KaecsesEmployer.Populated = true;
        entities.KaecsesEmployerAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEmployerEmployerAddress2()
  {
    entities.KaecsesEmployer.Populated = false;
    entities.KaecsesEmployerAddress.Populated = false;

    return ReadEach("ReadEmployerEmployerAddress2",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", local.LoadEmployer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.KaecsesEmployer.Identifier = db.GetInt32(reader, 0);
        entities.KaecsesEmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.KaecsesEmployer.Ein = db.GetNullableString(reader, 1);
        entities.KaecsesEmployer.KansasId = db.GetNullableString(reader, 2);
        entities.KaecsesEmployer.Name = db.GetNullableString(reader, 3);
        entities.KaecsesEmployer.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.KaecsesEmployer.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.KaecsesEmployer.PhoneNo = db.GetNullableString(reader, 6);
        entities.KaecsesEmployer.AreaCode = db.GetNullableInt32(reader, 7);
        entities.KaecsesEmployerAddress.LocationType = db.GetString(reader, 8);
        entities.KaecsesEmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.KaecsesEmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.KaecsesEmployerAddress.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.KaecsesEmployerAddress.CreatedBy = db.GetString(reader, 12);
        entities.KaecsesEmployerAddress.Street1 =
          db.GetNullableString(reader, 13);
        entities.KaecsesEmployerAddress.Street2 =
          db.GetNullableString(reader, 14);
        entities.KaecsesEmployerAddress.City = db.GetNullableString(reader, 15);
        entities.KaecsesEmployerAddress.Identifier = db.GetDateTime(reader, 16);
        entities.KaecsesEmployerAddress.State =
          db.GetNullableString(reader, 17);
        entities.KaecsesEmployerAddress.ZipCode =
          db.GetNullableString(reader, 18);
        entities.KaecsesEmployerAddress.Zip4 = db.GetNullableString(reader, 19);
        entities.KaecsesEmployerAddress.Zip3 = db.GetNullableString(reader, 20);
        entities.KaecsesEmployerAddress.County =
          db.GetNullableString(reader, 21);
        entities.KaecsesEmployerAddress.Note = db.GetNullableString(reader, 22);
        entities.KaecsesEmployer.Populated = true;
        entities.KaecsesEmployerAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSourceCsePerson1()
  {
    entities.IncomeSource.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadIncomeSourceCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "empId", entities.KaecsesEmployer.Identifier);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 1);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 2);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 3);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 6);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 7);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 8);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 9);
        entities.IncomeSource.CspINumber = db.GetString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 11);
        entities.IncomeSource.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSourceCsePerson2()
  {
    entities.IncomeSource.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadIncomeSourceCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "empId", entities.KaecsesEmployer.Identifier);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 1);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 2);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 3);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 6);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 7);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 8);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 9);
        entities.IncomeSource.CspINumber = db.GetString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 11);
        entities.IncomeSource.Populated = true;
        entities.CsePerson.Populated = true;

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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of Aging.
    /// </summary>
    [JsonPropertyName("aging")]
    public DateWorkArea Aging
    {
      get => aging ??= new();
      set => aging = value;
    }

    /// <summary>
    /// A value of EventsCreated.
    /// </summary>
    [JsonPropertyName("eventsCreated")]
    public Common EventsCreated
    {
      get => eventsCreated ??= new();
      set => eventsCreated = value;
    }

    /// <summary>
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public IncomeSource Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of ReportOnlyNoUpdates.
    /// </summary>
    [JsonPropertyName("reportOnlyNoUpdates")]
    public Common ReportOnlyNoUpdates
    {
      get => reportOnlyNoUpdates ??= new();
      set => reportOnlyNoUpdates = value;
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
    /// A value of EmployersSkippedNf.
    /// </summary>
    [JsonPropertyName("employersSkippedNf")]
    public Common EmployersSkippedNf
    {
      get => employersSkippedNf ??= new();
      set => employersSkippedNf = value;
    }

    /// <summary>
    /// A value of EmployersUpdated.
    /// </summary>
    [JsonPropertyName("employersUpdated")]
    public Common EmployersUpdated
    {
      get => employersUpdated ??= new();
      set => employersUpdated = value;
    }

    /// <summary>
    /// A value of RecordsSkippedInvalid.
    /// </summary>
    [JsonPropertyName("recordsSkippedInvalid")]
    public Common RecordsSkippedInvalid
    {
      get => recordsSkippedInvalid ??= new();
      set => recordsSkippedInvalid = value;
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
    /// A value of EmployerRelationDetected.
    /// </summary>
    [JsonPropertyName("employerRelationDetected")]
    public Common EmployerRelationDetected
    {
      get => employerRelationDetected ??= new();
      set => employerRelationDetected = value;
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
    /// A value of LoadEmployer.
    /// </summary>
    [JsonPropertyName("loadEmployer")]
    public Employer LoadEmployer
    {
      get => loadEmployer ??= new();
      set => loadEmployer = value;
    }

    /// <summary>
    /// A value of LoadEmployerAddress.
    /// </summary>
    [JsonPropertyName("loadEmployerAddress")]
    public EmployerAddress LoadEmployerAddress
    {
      get => loadEmployerAddress ??= new();
      set => loadEmployerAddress = value;
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
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
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

    private TextWorkArea date;
    private Common active;
    private DateWorkArea aging;
    private Common eventsCreated;
    private Common alertsCreated;
    private IncomeSource search;
    private Common reportOnlyNoUpdates;
    private Common recordsRead;
    private Common employersSkippedNf;
    private Common employersUpdated;
    private Common recordsSkippedInvalid;
    private Common recordsAlreadyProcessed;
    private Common employerRelationDetected;
    private ProgramProcessingInfo programProcessingInfo;
    private Employer loadEmployer;
    private EmployerAddress loadEmployerAddress;
    private Common batch;
    private TextWorkArea time;
    private Common valid;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private TextWorkArea recordType;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private Infrastructure infrastructure;
    private External eab;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of KaecsesEmployer.
    /// </summary>
    [JsonPropertyName("kaecsesEmployer")]
    public Employer KaecsesEmployer
    {
      get => kaecsesEmployer ??= new();
      set => kaecsesEmployer = value;
    }

    /// <summary>
    /// A value of KaecsesEmployerAddress.
    /// </summary>
    [JsonPropertyName("kaecsesEmployerAddress")]
    public EmployerAddress KaecsesEmployerAddress
    {
      get => kaecsesEmployerAddress ??= new();
      set => kaecsesEmployerAddress = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private Employer kaecsesEmployer;
    private EmployerAddress kaecsesEmployerAddress;
  }
#endregion
}
