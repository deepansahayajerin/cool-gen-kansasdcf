// Program: SI_B276_STATE_NEWHIRE, ID: 373399208, model: 746.
// Short name: SWEI276B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B276_STATE_NEWHIRE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB276StateNewhire: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B276_STATE_NEWHIRE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB276StateNewhire(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB276StateNewhire.
  /// </summary>
  public SiB276StateNewhire(IContext context, Import import, Export export):
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
    // * 07/01/2001  Ed Lyman   WR000290    Initial Coding.                  *
    // *
    // 
    // *
    // * 10/01/2002  Ed Lyman   PR157886    Auto IWO not processed after     *
    // *
    // 
    // KS New Hire Empl Verification.   *
    // *
    // 
    // *
    // * 02/01/2003  Ed Lyman   PR147222    Add date routine for hire date.  *
    // *
    // 
    // *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);
    local.FederalNewhireIndicator.Flag = "";

    local.AlternateSsn.Index = 0;
    local.AlternateSsn.CheckSize();

    local.Names.Index = 0;
    local.Names.CheckSize();

    UseSiB276Housekeeping();

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
      local.Employment.Assign(local.Clear);
      UseEabReadFederalNewHireFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING STATE NEW HIRE INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;
      local.Kaecses.Number = local.StateCaseRegistry.Number;

      if (AsChar(local.RecordType.Text1) != '1' && AsChar
        (local.RecordType.Text1) != '2')
      {
        ++local.RecordsSkippedNotType1.Count;

        continue;
      }

      UseSiB273ValidateDateOfHire();

      if (Equal(local.DateOfHireDateWorkArea.Date, local.Null1.Date) || Lt
        (local.Process.Date, local.DateOfHireDateWorkArea.Date))
      {
        local.DateOfHireDateWorkArea.Date = local.Process.Date;
      }

      if (Lt(local.DateOfHireDateWorkArea.Date,
        AddMonths(local.Process.Date, -6)))
      {
        ++local.RecordsSkippedDateHire.Count;
        local.NeededToWrite.RptDetail = "Record Skipped - Person # " + local
          .Kaecses.Number;
        local.DateOfHireTextWorkArea.Text10 =
          NumberToString(Month(local.DateOfHireDateWorkArea.Date), 14, 2) + "/"
          + NumberToString(Day(local.DateOfHireDateWorkArea.Date), 14, 2) + "/"
          + NumberToString(Year(local.DateOfHireDateWorkArea.Date), 12, 4);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date (" + local
          .DateOfHireTextWorkArea.Text10 + ") is more than 6 months in the past.";
          
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

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
        local.NeededToWrite.RptDetail = "Person Not Found - Person # " + local
          .StateCaseRegistry.Number + "  " + TrimEnd
          (local.StateCaseRegistry.LastName) + ", " + TrimEnd
          (local.StateCaseRegistry.FirstName) + " " + "" + local
          .StateCaseRegistry.MiddleInitial;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          break;
        }

        continue;
      }

      if (AsChar(local.RecordType.Text1) == '2')
      {
        if (IsEmpty(local.EmployerAddress.City) && IsEmpty
          (local.EmployerAddress.ZipCode))
        {
          ++local.EmployeeAddressMissing.Count;
          local.NeededToWrite.RptDetail =
            "Person Address Not Found - Person # " + local
            .StateCaseRegistry.Number + "  " + TrimEnd
            (local.StateCaseRegistry.LastName) + ", " + TrimEnd
            (local.StateCaseRegistry.FirstName) + " " + "" + local
            .StateCaseRegistry.MiddleInitial;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            break;
          }

          continue;
        }

        local.ProgramProcessingInfo.Name =
          local.ProgramCheckpointRestart.ProgramName;
        local.ProgramProcessingInfo.ProcessDate = local.Process.Date;
        local.FplsLocateRequest.ApFirstName = local.StateCaseRegistry.FirstName;
        local.FplsLocateRequest.ApMiddleName =
          local.StateCaseRegistry.MiddleInitial;
        local.FplsLocateRequest.ApFirstLastName =
          local.StateCaseRegistry.LastName;
        local.FplsLocateRequest.SendRequestTo =
          Spaces(FplsLocateRequest.SendRequestTo_MaxLength);
        local.FplsLocateRequest.Ssn = local.StateCaseRegistry.Ssn;
        local.FplsLocateResponse.Fein = local.Employer.Ein ?? "";
        local.FplsLocateResponse.DateOfHire = local.DateOfHireDateWorkArea.Date;
        local.FplsLocateResponse.ApNameReturned =
          (local.FplsLocateRequest.ApFirstName ?? "") + (
            local.FplsLocateRequest.ApMiddleName ?? "") + (
            local.FplsLocateRequest.ApFirstLastName ?? "");
        local.Street1.Text40 = local.EmployerAddress.Street1 ?? Spaces(40);
        local.Street2.Text40 = local.EmployerAddress.Street2 ?? Spaces(40);
        local.Street3.Text40 = local.EmployerAddress.Street3 ?? Spaces(40);
        local.Street4.Text40 = local.EmployerAddress.Street4 ?? Spaces(40);
        local.City.Text30 = local.EmployerAddress.City ?? Spaces(30);
        local.StateZip.Text11 = (local.EmployerAddress.State ?? "") + (
          local.EmployerAddress.ZipCode ?? "") + (
            local.EmployerAddress.Zip4 ?? "");
        local.FplsLocateResponse.ReturnedAddress = local.Street1.Text40 + local
          .Street2.Text40 + local.Street3.Text40 + local.Street4.Text40 + local
          .City.Text30 + local.StateZip.Text11;
        UseSiB276PlaceAddressOnFpls();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        continue;
      }

      if (!Equal(local.StateCaseRegistry.Ssn, local.Kaecses.Ssn))
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

          if (Equal(local.StateCaseRegistry.Ssn,
            local.AlternateSsn.Item.Gssn.Ssn))
          {
            local.Employment.Note = "Matched alternate ssn: " + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-" + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-" + Substring
              (local.AlternateSsn.Item.Gssn.Ssn,
              CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            local.Employment.Note = TrimEnd(local.Employment.Note) + " name on W-4: " +
              TrimEnd(local.StateCaseRegistry.LastName) + ", " + TrimEnd
              (local.StateCaseRegistry.FirstName) + " " + local
              .StateCaseRegistry.MiddleInitial;

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

        continue;
      }

Test:

      if (IsEmpty(local.StateCaseRegistry.Flag))
      {
        ++local.EmployeeNameMismatch.Count;

        if (IsEmpty(local.Employment.Note))
        {
          local.Employment.Note = TrimEnd("") + "Employee name on W-4 is: " + TrimEnd
            (local.StateCaseRegistry.LastName) + ", " + TrimEnd
            (local.StateCaseRegistry.FirstName) + " " + local
            .StateCaseRegistry.MiddleInitial;
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

      UseSiB273ValidateAndNewhireRpt();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        break;
      }

      if (AsChar(local.Valid.Flag) == 'N')
      {
        ++local.RecordsSkippedNuhireRpt.Count;

        continue;
      }

      // ****************************************************************************
      // At this point, for the current record, no creates or updates have been 
      // done.
      // ****************************************************************************
      UseSiB273MaintainEmployer();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // **************************************************************************
      // Employment is a subtype of income source.  Does this person already 
      // have a
      // record of employment for this employer?  Valid return codes:
      //  Space = No verification from employer
      //      E = Employed
      //      F = Fired
      //      L = Laid Off
      //      N = Never Worked
      //      O = Other
      //      Q = Quit
      //      W = Receiving Workmen's Compensation
      // **************************************************************************
      foreach(var item in ReadEmployment())
      {
        if (Lt(entities.Employment.EndDt, local.DateOfHireDateWorkArea.Date))
        {
          // **************************************************************************
          // The employee may have been re-hired.  In this situation,
          // a new employment must be created.
          // **************************************************************************
          break;
        }

        if (AsChar(entities.Employment.ReturnCd) == 'E')
        {
          // ***********************************************************
          // PR 160363  Counter not being incremented, now fixed.
          // ***********************************************************
          ++local.RecordsAlreadyProcessed.Count;

          goto Next;
        }

        if (!IsEmpty(entities.Employment.ReturnCd))
        {
          // ***************************************************************
          // The return code indicates the employee is not employed at this
          // employer as of the return date, regardless of whether the
          // employment has been end dated.  The employee may have been
          // re-hired.  In this situation, a new employment must be created.
          // ***************************************************************
          if (Lt(entities.Employment.ReturnDt, local.DateOfHireDateWorkArea.Date))
            
          {
            break;
          }
        }
        else
        {
          local.Employment.ReturnCd = "E";
          local.Employment.ReturnDt = local.Process.Date;
          UseSiCabHireDateAlertIwo();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }
        }

        goto Next;
      }

      local.Employment.EndDt = local.Max.Date;
      local.Employment.StartDt = local.DateOfHireDateWorkArea.Date;
      local.Employment.Name = local.Employer.Name ?? "";
      local.Employment.ReturnCd = "E";
      local.Employment.ReturnDt = local.Process.Date;
      UseSiB273CreateEmpIncomeSource();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      local.Infrastructure.Detail = "Employer: " + (local.Employer.Name ?? "");
      UseSiB276SendAlertNewIncSrce();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      if (!Equal(local.StateCaseRegistry.Ssn, local.Kaecses.Ssn))
      {
        // ***************************************************************
        // Must have matched on alternate SSN, so send alert.
        // ***************************************************************
        local.Infrastructure.Detail = local.Employment.Note ?? "";
        UseSiB276SendAlertNewIncSrce();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

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

Next:
      ;
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB276Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB276Close();
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

  private static void MoveEmployer1(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.KansasId = source.KansasId;
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveEmployer2(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveEmployerAddress(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
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

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.ReturnDt = source.ReturnDt;
    target.ReturnCd = source.ReturnCd;
    target.Note = source.Note;
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadFederalNewHireFile()
  {
    var useImport = new EabReadFederalNewHireFile.Import();
    var useExport = new EabReadFederalNewHireFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;
    useExport.RecordType.Text1 = local.RecordType.Text1;
    useExport.DateOfHire.TextDate = local.DateOfHireDateWorkArea.TextDate;
    useExport.StateHiredIn.Text2 = local.StateHiredInTextWorkArea.Text2;
    useExport.Employer.Assign(local.Employer);
    useExport.EmployerAddress.Assign(local.EmployerAddress);
    useExport.CsePersonsWorkSet.Assign(local.StateCaseRegistry);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFederalNewHireFile.Execute, useImport, useExport);

    local.MilitaryStatus.Text1 = useExport.MilitaryStatus.Text1;
    local.RecordType.Text1 = useExport.RecordType.Text1;
    local.DateOfHireDateWorkArea.TextDate = useExport.DateOfHire.TextDate;
    local.StateHiredInTextWorkArea.Text2 = useExport.StateHiredIn.Text2;
    MoveEmployer1(useExport.Employer, local.Employer);
    MoveEmployerAddress(useExport.EmployerAddress, local.EmployerAddress);
    local.StateCaseRegistry.Assign(useExport.CsePersonsWorkSet);
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

  private void UseSiB273CreateEmpIncomeSource()
  {
    var useImport = new SiB273CreateEmpIncomeSource.Import();
    var useExport = new SiB273CreateEmpIncomeSource.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;
    useImport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;
    useImport.IncomeSourceCreated.Count = local.IncomeSourcesCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Employer.Identifier = local.Employer.Identifier;
    useImport.Employment.Assign(local.Employment);

    Call(SiB273CreateEmpIncomeSource.Execute, useImport, useExport);

    local.IncomeSourcesCreated.Count = useExport.IncomeSourceCreated.Count;
  }

  private void UseSiB273EmployeeNameMismatch()
  {
    var useImport = new SiB273EmployeeNameMismatch.Import();
    var useExport = new SiB273EmployeeNameMismatch.Export();

    useImport.Kaecses.Assign(local.Kaecses);
    useImport.FederalCaseRegistry.Assign(local.StateCaseRegistry);

    Call(SiB273EmployeeNameMismatch.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273MaintainEmployer()
  {
    var useImport = new SiB273MaintainEmployer.Import();
    var useExport = new SiB273MaintainEmployer.Export();

    useImport.PreviouslyCompared.Ein = local.PreviouslyCompared.Ein;
    useImport.EmployerMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.EmployersCreated.Count = local.EmployersCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);

    Call(SiB273MaintainEmployer.Execute, useImport, useExport);

    local.AddressSuitableForIwo.Flag = useExport.AddressSuitableForIwo.Flag;
    local.PreviouslyCompared.Ein = useExport.PreviouslyCompared.Ein;
    local.EmployerInfoMismatch.Count = useExport.EmployerMismatch.Count;
    local.EmployersCreated.Count = useExport.EmployersCreated.Count;
    local.Employer.Assign(useExport.Employer);
    local.EmployerAddress.Identifier = useExport.EmployerAddress.Identifier;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273SsnMismatchReport()
  {
    var useImport = new SiB273SsnMismatchReport.Import();
    var useExport = new SiB273SsnMismatchReport.Export();

    useImport.Kaecses.Assign(local.Kaecses);
    useImport.FederalCaseRegistry.Assign(local.StateCaseRegistry);

    Call(SiB273SsnMismatchReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273ValidateAndNewhireRpt()
  {
    var useImport = new SiB273ValidateAndNewhireRpt.Import();
    var useExport = new SiB273ValidateAndNewhireRpt.Export();

    useImport.Process.Date = local.Process.Date;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.CsePersonsWorkSet.Assign(local.StateCaseRegistry);

    Call(SiB273ValidateAndNewhireRpt.Execute, useImport, useExport);

    local.Valid.Flag = useExport.Valid.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273ValidateDateOfHire()
  {
    var useImport = new SiB273ValidateDateOfHire.Import();
    var useExport = new SiB273ValidateDateOfHire.Export();

    useImport.Process.Date = local.Process.Date;
    useImport.Hire.TextDate = local.DateOfHireDateWorkArea.TextDate;
    useImport.Kaecses.Number = local.Kaecses.Number;

    Call(SiB273ValidateDateOfHire.Execute, useImport, useExport);

    local.DateOfHireDateWorkArea.Date = useExport.Hire.Date;
  }

  private void UseSiB276Close()
  {
    var useImport = new SiB276Close.Import();
    var useExport = new SiB276Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.IncomeSourcesCreated.Count = local.IncomeSourcesCreated.Count;
    useImport.EmployeeAddrPutOnFpls.Count = local.EmployeeAddrPutOnFpls.Count;
    useImport.EmployeeAddressMissing.Count = local.EmployeeAddressMissing.Count;
    useImport.DateOfHireUpdates.Count = local.DateOfHireUpdates.Count;
    useImport.RecordsSkippedNuhireRp.Count =
      local.RecordsSkippedNuhireRpt.Count;
    useImport.RecordsSkippedDateHire.Count = local.RecordsSkippedDateHire.Count;
    useImport.RecordsSkippedSsn.Count = local.RecordsSkippedSsn.Count;
    useImport.RecordsSkippedNotOne.Count = local.RecordsSkippedNotType1.Count;
    useImport.RecordsPersonNotFound.Count = local.RecordsPersonNotFound.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.EmployersCreated.Count = local.EmployersCreated.Count;
    useImport.FplsRequestsUpdated.Count = local.FplsRequestUpdates.Count;
    useImport.EmployerInfoMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.EmployeeNameMismatch.Count = local.EmployeeNameMismatch.Count;

    Call(SiB276Close.Execute, useImport, useExport);
  }

  private void UseSiB276Housekeeping()
  {
    var useImport = new SiB276Housekeeping.Import();
    var useExport = new SiB276Housekeeping.Export();

    Call(SiB276Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.AutomaticGenerateIwo.Flag = useExport.AutomaticGenerateIwo.Flag;
  }

  private void UseSiB276PlaceAddressOnFpls()
  {
    var useImport = new SiB276PlaceAddressOnFpls.Import();
    var useExport = new SiB276PlaceAddressOnFpls.Export();

    useImport.AddressesPlacedOnFpls.Count = local.EmployeeAddrPutOnFpls.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.FplsRequestUpdates.Count = local.FplsRequestUpdates.Count;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.FplsLocateResponse.Assign(local.FplsLocateResponse);
    useImport.FplsLocateRequest.Assign(local.FplsLocateRequest);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(SiB276PlaceAddressOnFpls.Execute, useImport, useExport);

    local.EmployeeAddrPutOnFpls.Count = useExport.AddressesPlacedOnFpls.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
    local.FplsRequestUpdates.Count = useExport.FplsRequestUpdates.Count;
  }

  private void UseSiB276SendAlertNewIncSrce()
  {
    var useImport = new SiB276SendAlertNewIncSrce.Import();
    var useExport = new SiB276SendAlertNewIncSrce.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;
    useImport.Employment.Identifier = local.Employment.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(SiB276SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private void UseSiCabHireDateAlertIwo()
  {
    var useImport = new SiCabHireDateAlertIwo.Import();
    var useExport = new SiCabHireDateAlertIwo.Export();

    useImport.DateOfHireUpdates.Count = local.DateOfHireUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    MoveIncomeSource(local.Employment, useImport.NewInfo);
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.DateOfHire.Date = local.DateOfHireDateWorkArea.Date;
    MoveEmployer2(local.Employer, useImport.Employer);
    useImport.Employment.Identifier = entities.Employment.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Process.Date = local.Process.Date;
    useImport.FederalNewhireIndicator.Flag = local.FederalNewhireIndicator.Flag;
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;

    Call(SiCabHireDateAlertIwo.Execute, useImport, useExport);

    local.DateOfHireUpdates.Count = useExport.DateOfHireUpdates.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
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
        db.SetString(command, "numb", local.StateCaseRegistry.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEmployment()
  {
    entities.Employment.Populated = false;

    return ReadEach("ReadEmployment",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Employment.Identifier = db.GetDateTime(reader, 0);
        entities.Employment.Type1 = db.GetString(reader, 1);
        entities.Employment.ReturnDt = db.GetNullableDate(reader, 2);
        entities.Employment.ReturnCd = db.GetNullableString(reader, 3);
        entities.Employment.CspINumber = db.GetString(reader, 4);
        entities.Employment.EmpId = db.GetNullableInt32(reader, 5);
        entities.Employment.StartDt = db.GetNullableDate(reader, 6);
        entities.Employment.EndDt = db.GetNullableDate(reader, 7);
        entities.Employment.Populated = true;

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
    /// A value of FederalNewhireIndicator.
    /// </summary>
    [JsonPropertyName("federalNewhireIndicator")]
    public Common FederalNewhireIndicator
    {
      get => federalNewhireIndicator ??= new();
      set => federalNewhireIndicator = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of Street1.
    /// </summary>
    [JsonPropertyName("street1")]
    public WorkArea Street1
    {
      get => street1 ??= new();
      set => street1 = value;
    }

    /// <summary>
    /// A value of Street2.
    /// </summary>
    [JsonPropertyName("street2")]
    public WorkArea Street2
    {
      get => street2 ??= new();
      set => street2 = value;
    }

    /// <summary>
    /// A value of Street3.
    /// </summary>
    [JsonPropertyName("street3")]
    public WorkArea Street3
    {
      get => street3 ??= new();
      set => street3 = value;
    }

    /// <summary>
    /// A value of Street4.
    /// </summary>
    [JsonPropertyName("street4")]
    public WorkArea Street4
    {
      get => street4 ??= new();
      set => street4 = value;
    }

    /// <summary>
    /// A value of City.
    /// </summary>
    [JsonPropertyName("city")]
    public TextWorkArea City
    {
      get => city ??= new();
      set => city = value;
    }

    /// <summary>
    /// A value of StateZip.
    /// </summary>
    [JsonPropertyName("stateZip")]
    public WorkArea StateZip
    {
      get => stateZip ??= new();
      set => stateZip = value;
    }

    /// <summary>
    /// A value of FplsRequestUpdates.
    /// </summary>
    [JsonPropertyName("fplsRequestUpdates")]
    public Common FplsRequestUpdates
    {
      get => fplsRequestUpdates ??= new();
      set => fplsRequestUpdates = value;
    }

    /// <summary>
    /// A value of EmployeeAddrPutOnFpls.
    /// </summary>
    [JsonPropertyName("employeeAddrPutOnFpls")]
    public Common EmployeeAddrPutOnFpls
    {
      get => employeeAddrPutOnFpls ??= new();
      set => employeeAddrPutOnFpls = value;
    }

    /// <summary>
    /// A value of EmployeeAddressMissing.
    /// </summary>
    [JsonPropertyName("employeeAddressMissing")]
    public Common EmployeeAddressMissing
    {
      get => employeeAddressMissing ??= new();
      set => employeeAddressMissing = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
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
    /// A value of PreviouslyCompared.
    /// </summary>
    [JsonPropertyName("previouslyCompared")]
    public Employer PreviouslyCompared
    {
      get => previouslyCompared ??= new();
      set => previouslyCompared = value;
    }

    /// <summary>
    /// A value of MilitaryStatus.
    /// </summary>
    [JsonPropertyName("militaryStatus")]
    public TextWorkArea MilitaryStatus
    {
      get => militaryStatus ??= new();
      set => militaryStatus = value;
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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNuhireRpt.
    /// </summary>
    [JsonPropertyName("recordsSkippedNuhireRpt")]
    public Common RecordsSkippedNuhireRpt
    {
      get => recordsSkippedNuhireRpt ??= new();
      set => recordsSkippedNuhireRpt = value;
    }

    /// <summary>
    /// A value of RecordsSkippedDateHire.
    /// </summary>
    [JsonPropertyName("recordsSkippedDateHire")]
    public Common RecordsSkippedDateHire
    {
      get => recordsSkippedDateHire ??= new();
      set => recordsSkippedDateHire = value;
    }

    /// <summary>
    /// A value of DateOfHireUpdates.
    /// </summary>
    [JsonPropertyName("dateOfHireUpdates")]
    public Common DateOfHireUpdates
    {
      get => dateOfHireUpdates ??= new();
      set => dateOfHireUpdates = value;
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
    /// A value of RecordsSkippedNotType1.
    /// </summary>
    [JsonPropertyName("recordsSkippedNotType1")]
    public Common RecordsSkippedNotType1
    {
      get => recordsSkippedNotType1 ??= new();
      set => recordsSkippedNotType1 = value;
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
    /// A value of DateOfHireDateWorkArea.
    /// </summary>
    [JsonPropertyName("dateOfHireDateWorkArea")]
    public DateWorkArea DateOfHireDateWorkArea
    {
      get => dateOfHireDateWorkArea ??= new();
      set => dateOfHireDateWorkArea = value;
    }

    /// <summary>
    /// A value of DateOfHireTextWorkArea.
    /// </summary>
    [JsonPropertyName("dateOfHireTextWorkArea")]
    public TextWorkArea DateOfHireTextWorkArea
    {
      get => dateOfHireTextWorkArea ??= new();
      set => dateOfHireTextWorkArea = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
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
    /// A value of StateCaseRegistry.
    /// </summary>
    [JsonPropertyName("stateCaseRegistry")]
    public CsePersonsWorkSet StateCaseRegistry
    {
      get => stateCaseRegistry ??= new();
      set => stateCaseRegistry = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public IncomeSource Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private Common federalNewhireIndicator;
    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
    private WorkArea street1;
    private WorkArea street2;
    private WorkArea street3;
    private WorkArea street4;
    private TextWorkArea city;
    private WorkArea stateZip;
    private Common fplsRequestUpdates;
    private Common employeeAddrPutOnFpls;
    private Common employeeAddressMissing;
    private FplsLocateRequest fplsLocateRequest;
    private FplsLocateResponse fplsLocateResponse;
    private ProgramProcessingInfo programProcessingInfo;
    private Employer previouslyCompared;
    private TextWorkArea militaryStatus;
    private Common batch;
    private Array<NamesGroup> names;
    private Array<AlternateSsnGroup> alternateSsn;
    private TextWorkArea time;
    private TextWorkArea date;
    private Common employerInfoMismatch;
    private Common employersCreated;
    private Common incomeSourcesCreated;
    private Common valid;
    private Common recordsRead;
    private Common recordsSkippedNuhireRpt;
    private Common recordsSkippedDateHire;
    private Common dateOfHireUpdates;
    private Common employeeNameMismatch;
    private Common recordsSkippedSsn;
    private Common recordsSkippedNotType1;
    private Common recordsPersonNotFound;
    private Common recordsAlreadyProcessed;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private TextWorkArea recordType;
    private DateWorkArea dateOfHireDateWorkArea;
    private TextWorkArea dateOfHireTextWorkArea;
    private CsePersonAddress stateHiredInCsePersonAddress;
    private TextWorkArea stateHiredInTextWorkArea;
    private Employer employer;
    private EmployerAddress employerAddress;
    private EabReportSend neededToWrite;
    private CsePersonsWorkSet kaecses;
    private CsePersonsWorkSet stateCaseRegistry;
    private EabFileHandling eabFileHandling;
    private IncomeSource employment;
    private AbendData abendData;
    private IncomeSource clear;
    private Infrastructure infrastructure;
    private EabReportSend eabReportSend;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private IncomeSource employment;
    private Employer employer;
    private EmployerAddress employerAddress;
    private CsePerson csePerson;
  }
#endregion
}
