// Program: SI_B273_FEDERAL_NEWHIRE, ID: 371061441, model: 746.
// Short name: SWEI273B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_FEDERAL_NEWHIRE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB273FederalNewhire: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_FEDERAL_NEWHIRE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273FederalNewhire(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273FederalNewhire.
  /// </summary>
  public SiB273FederalNewhire(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 03/19/2001  Ed Lyman           WR0074376   Initial Coding.
    // *
    // *
    // 
    // *
    // * 05/21/2001  Ed Lyman           PR119356    Add Military Logic.
    // *
    // *
    // 
    // *
    // * 02/25/2002  Ed Lyman           PR139868    Add call to create Auto IWO
    // *
    // *
    // 
    // *
    // * 10/01/2002  Ed Lyman           PR157886    Add call to create Auto IWO
    // *
    // *
    // 
    // when updating the hire date and          *
    // *
    // 
    // income source is not verified.           *
    // *
    // 
    // *
    // * 02/01/2003  Ed Lyman           PR147222    Add date routine for hire 
    // date.          *
    // *
    // 
    // *
    // * 02/02/2009  Raj S              CQ114       Modified to generate worker 
    // alert for    *
    // *                                            NDNH response SSN and CSE 
    // SSNs mismatches*
    // ***************************************************************************************
    // 07/31/2009   DDupree     Added check when processing the returning ssn to
    // see
    //  if it is a invalid ssn and person number combination. Part of CQ7189.
    // __________________________________________________________________________________
    // 09/20/2010   LSS   CQ6658 PR342940
    // Modified to set return_cd to 'A' for Military source instead of 'E'
    // *************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);
    local.FederalNewhireIndicator.Flag = "Y";

    local.AlternateSsn.Index = 0;
    local.AlternateSsn.CheckSize();

    local.Names.Index = 0;
    local.Names.CheckSize();

    UseSiB273Housekeeping();

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
            "ERROR READING FEDERAL NEW HIRE INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;
      local.Kaecses.Number = local.FederalCaseRegistry.Number;

      if (AsChar(local.RecordType.Text1) != '1')
      {
        ++local.RecordsSkippedNotType1.Count;

        continue;
      }

      if (ReadCsePerson())
      {
        // **********************************************************
        // 07/29/2009   DDupree   added this check as part of cq7189.
        // **********************************************************
        local.ConvertSsn.SsnNum9 =
          (int)StringToNumber(local.FederalCaseRegistry.Ssn);

        if (ReadInvalidSsn())
        {
          if (Lt(new DateTime(1, 1, 1), local.DateOfHireDateWorkArea.Date))
          {
            local.ConvertDateCommon.Count =
              DateToInt(local.DateOfHireDateWorkArea.Date);
            local.ConvertDateWorkArea.Text15 =
              NumberToString(local.ConvertDateCommon.Count, 15);
            local.Convert.TextDate =
              Substring(local.ConvertDateWorkArea.Text15, 8, 8);
          }
          else
          {
            local.Convert.TextDate = local.DateOfHireDateWorkArea.TextDate;
          }

          local.ConvertMessage.SsnTextPart1 =
            Substring(local.FederalCaseRegistry.Ssn, 1, 3);
          local.ConvertMessage.SsnTextPart2 =
            Substring(local.FederalCaseRegistry.Ssn, 4, 2);
          local.ConvertMessage.SsnTextPart3 =
            Substring(local.FederalCaseRegistry.Ssn, 6, 4);
          local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
            .ConvertMessage.SsnTextPart2 + "-" + local
            .ConvertMessage.SsnTextPart3;
          local.Message1.Text8 = "Bad SSN";
          local.Message1.Text6 = ", Per";
          local.Message1.Text16 = ": Rec not used -";
          local.Message1.Text2 = ",";
          local.Message1.Text1 = "";
          local.Message1.Text80 = TrimEnd(local.Convert.TextDate) + local
            .Message1.Text2 + TrimEnd(local.Employer.Name) + local
            .Message1.Text2 + TrimEnd(local.EmployerAddress.City) + local
            .Message1.Text2 + TrimEnd(local.EmployerAddress.State) + local
            .Message1.Text1 + TrimEnd(local.EmployerAddress.ZipCode) + local
            .Message1.Text2 + (local.EmployerAddress.Street1 ?? "");
          local.NeededToWrite.RptDetail = local.Message1.Text8 + local
            .Message2.Text11 + local.Message1.Text6 + local
            .FederalCaseRegistry.Number + local.Message1.Text16 + local
            .Message1.Text80;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.RecordsPersonNotFound.Count;
          local.Message1.Text8 = "";
          local.Message1.Text6 = "";
          local.Message1.Text16 = "";
          local.Message1.Text2 = "";
          local.Message1.Text1 = "";
          local.Message1.Text80 = "";
          local.Message2.Text11 = "";
          local.Convert.TextDate = "";
          local.NeededToWrite.RptDetail = "";

          continue;
        }
        else
        {
          // this is fine, there is not invalid ssn record for this combination 
          // of cse person number and ssn number
        }

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

      UseSiB273ValidateDateOfHire();

      if (Lt(local.DateOfHireDateWorkArea.Date,
        AddMonths(local.Process.Date, -6)))
      {
        ++local.RecordsSkippedDateHire.Count;

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
            local.Employment.Note = TrimEnd(local.Employment.Note) + " name on W-4: " +
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

        // ***************************************************************************
        // Write to report of ssn's don't match.  Skip to next person.
        // ***************************************************************************
        // **************************************************************************************
        // Generate worker alert for mismatched SSNs by using the Action Block
        // SI_B273_SSN_MISMATCH_ALERTS_GEN.    CQ114 Changes Start
        // **************************************************************************************
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReasonCode = "FCRNEWSSNNDNHNH";
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
          "**Employer Record from New Hire through SWEIB273 Batch Process**";
        UseSiB273SsnMismatchAlertsGen();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // **************************************************************************************
        // Generate worker alert for mismatched SSNs by using the Action Block
        // SI_B273_SSN_MISMATCH_ALERTS_GEN.    CQ114 Changes End
        // **************************************************************************************
        continue;
      }

Test:

      if (IsEmpty(local.FederalCaseRegistry.Flag))
      {
        ++local.EmployeeNameMismatch.Count;

        if (IsEmpty(local.Employment.Note))
        {
          local.Employment.Note = TrimEnd("") + "Employee name on W-4 is: " + TrimEnd
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

      UseSiB273ValidateAndNewhireRpt();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        break;
      }

      if (AsChar(local.Valid.Flag) == 'N')
      {
        // **************************************************************
        // EXAMPLES OF REJECTS:
        //     Missing EIN, Zip Code, Name, Street1 and Street2
        //     Missing or Invalid State Code or Foreign Address
        // **************************************************************
        ++local.RecordsSkippedNuhireRpt.Count;

        continue;
      }

      // ****************************************************************************
      // At this point, for the current record, no creates or updates have been 
      // done.
      // ****************************************************************************
      // ****************************************************************************
      // Action Block Maintain Employer will determine if the address is 
      // suitable
      // for automatically generating an Income Witholding Order.
      // ****************************************************************************
      UseSiB273MaintainEmployer();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      if (IsEmpty(local.MilitaryStatus.Text1) || AsChar
        (local.MilitaryStatus.Text1) == 'C')
      {
        // **************************************************************************
        // Military status of space or C indicates civilian employment.
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
            ++local.RecordsAlreadyProcessed.Count;

            if (AsChar(local.AlreadyProcessRpt.Flag) == 'Y')
            {
              local.NewHire.StartDt = local.DateOfHireDateWorkArea.Date;
              UseSiB273AlreadyProcessedEmpl();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

                goto AfterCycle;
              }
            }

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
            if (Lt(entities.Employment.ReturnDt,
              local.DateOfHireDateWorkArea.Date))
            {
              break;
            }

            goto Next;
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
      }
      else
      {
        // **************************************************************************
        // Military status of A = active duty and  R = reserves.  Military is a
        // subtype of income source.  Does this person already have a record of
        // military service?  Valid return codes:
        //  Space = No verication from the military
        //      A = Active
        //      I = Inactive
        //      N = None
        //      R = Retired
        //      U = Unknown
        // **************************************************************************
        foreach(var item in ReadMilitary())
        {
          if (Lt(entities.Military.EndDt, local.DateOfHireDateWorkArea.Date))
          {
            // **************************************************************************
            // The military employee may have been re-hired.  In this situation,
            // a new military income source must be created.
            // **************************************************************************
            break;
          }

          if (AsChar(entities.Military.ReturnCd) != 'A' && AsChar
            (entities.Military.ReturnCd) != 'R' && !
            IsEmpty(entities.Military.ReturnCd))
          {
            // ***************************************************************
            // The return code indicates the serviceman is not in the service
            // as of the return date, regardless of whether the military has
            // been end dated.  The serviceman may have reenlisted. In this
            // situation, a new military must be created.
            // ***************************************************************
            if (Lt(entities.Military.ReturnDt, local.DateOfHireDateWorkArea.Date))
              
            {
              break;
            }
          }

          local.NewHire.StartDt = local.DateOfHireDateWorkArea.Date;
          UseSiB273UpdateEnlistmentDate();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto AfterCycle;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }

          if (!Equal(local.Employer.Ein, local.PreviouslyCompared.Ein))
          {
            UseSiB273CompareMilitaryInfo();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              goto AfterCycle;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto AfterCycle;
            }

            local.PreviouslyCompared.Ein = local.Employer.Ein ?? "";
          }

          goto Next;
        }
      }

      local.Employment.EndDt = local.Max.Date;
      local.Employment.StartDt = local.DateOfHireDateWorkArea.Date;
      local.Employment.Name = local.Employer.Name ?? "";

      // CQ6658  Make return_cd "A" if military source.
      if (IsEmpty(local.MilitaryStatus.Text1) || AsChar
        (local.MilitaryStatus.Text1) == 'C')
      {
        local.Employment.ReturnCd = "E";
      }
      else
      {
        local.Employment.ReturnCd = "A";
      }

      local.Employment.ReturnDt = local.Process.Date;
      UseSiB273CreateEmpIncomeSource();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      local.Infrastructure.Detail = "Employer: " + (local.Employer.Name ?? "");
      UseSiB273SendAlertNewIncSrce();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      if (!Equal(local.FederalCaseRegistry.Ssn, local.Kaecses.Ssn))
      {
        // ***************************************************************
        // Must have matched on alternate SSN, so send alert.
        // ***************************************************************
        local.Infrastructure.Detail = local.Employment.Note ?? "";
        UseSiB273SendAlertNewIncSrce();
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
      UseSiB273Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB273Close();
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
    useExport.RecordType.Text1 = local.RecordType.Text1;
    useExport.DateOfHire.TextDate = local.DateOfHireDateWorkArea.TextDate;
    useExport.StateHiredIn.Text2 = local.StateHiredInTextWorkArea.Text2;
    useExport.Employer.Assign(local.Employer);
    useExport.EmployerAddress.Assign(local.EmployerAddress);
    useExport.CsePersonsWorkSet.Assign(local.FederalCaseRegistry);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;

    Call(EabReadFederalNewHireFile.Execute, useImport, useExport);

    local.RecordType.Text1 = useExport.RecordType.Text1;
    local.DateOfHireDateWorkArea.TextDate = useExport.DateOfHire.TextDate;
    local.StateHiredInTextWorkArea.Text2 = useExport.StateHiredIn.Text2;
    MoveEmployer1(useExport.Employer, local.Employer);
    MoveEmployerAddress(useExport.EmployerAddress, local.EmployerAddress);
    local.FederalCaseRegistry.Assign(useExport.CsePersonsWorkSet);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.MilitaryStatus.Text1 = useExport.MilitaryStatus.Text1;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB273AlreadyProcessedEmpl()
  {
    var useImport = new SiB273AlreadyProcessedEmpl.Import();
    var useExport = new SiB273AlreadyProcessedEmpl.Export();

    useImport.Cse.Assign(entities.Employment);
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.NewHireIncomeSource.Assign(local.NewHire);
    MoveEmployer2(local.Employer, useImport.NewHireEmployer);
    MoveEmployer2(local.Employer, useImport.Kaecses);

    Call(SiB273AlreadyProcessedEmpl.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273Close()
  {
    var useImport = new SiB273Close.Import();
    var useExport = new SiB273Close.Export();

    useImport.EmployerInfoMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.EmployersCreated.Count = local.EmployersCreated.Count;
    useImport.IncomeSourcesCreated.Count = local.IncomeSourcesCreated.Count;
    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.RecordsSkippedNuhireRp.Count =
      local.RecordsSkippedNuhireRpt.Count;
    useImport.RecordsSkippedDateHire.Count = local.RecordsSkippedDateHire.Count;
    useImport.DateOfHireUpdates.Count = local.DateOfHireUpdates.Count;
    useImport.EmployeeNameMismatch.Count = local.EmployeeNameMismatch.Count;
    useImport.RecordsSkippedSsn.Count = local.RecordsSkippedSsn.Count;
    useImport.RecordsSkippedNotOne.Count = local.RecordsSkippedNotType1.Count;
    useImport.RecordsPersonNotFound.Count = local.RecordsPersonNotFound.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;

    Call(SiB273Close.Execute, useImport, useExport);
  }

  private void UseSiB273CompareMilitaryInfo()
  {
    var useImport = new SiB273CompareMilitaryInfo.Import();
    var useExport = new SiB273CompareMilitaryInfo.Export();

    useImport.Military.Identifier = entities.Military.Identifier;
    useImport.EmployerInfoMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.FcrEmployer.Assign(local.Employer);
    useImport.FcrEmployerAddress.Assign(local.EmployerAddress);

    Call(SiB273CompareMilitaryInfo.Execute, useImport, useExport);

    local.EmployerInfoMismatch.Count = useExport.EmployerInfoMismatch.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273CreateEmpIncomeSource()
  {
    var useImport = new SiB273CreateEmpIncomeSource.Import();
    var useExport = new SiB273CreateEmpIncomeSource.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.IncomeSourceCreated.Count = local.IncomeSourcesCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Employer.Identifier = local.Employer.Identifier;
    useImport.Employment.Assign(local.Employment);
    useImport.MilitaryStatus.Text1 = local.MilitaryStatus.Text1;
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;

    Call(SiB273CreateEmpIncomeSource.Execute, useImport, useExport);

    local.IncomeSourcesCreated.Count = useExport.IncomeSourceCreated.Count;
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

  private void UseSiB273Housekeeping()
  {
    var useImport = new SiB273Housekeeping.Import();
    var useExport = new SiB273Housekeeping.Export();

    Call(SiB273Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AutomaticGenerateIwo.Flag = useExport.AutomaticGenerateIwo.Flag;
    local.AlreadyProcessRpt.Flag = useExport.AlreadyProcessRpt.Flag;
  }

  private void UseSiB273MaintainEmployer()
  {
    var useImport = new SiB273MaintainEmployer.Import();
    var useExport = new SiB273MaintainEmployer.Export();

    useImport.EmployerMismatch.Count = local.EmployerInfoMismatch.Count;
    useImport.EmployersCreated.Count = local.EmployersCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.PreviouslyCompared.Ein = local.PreviouslyCompared.Ein;

    Call(SiB273MaintainEmployer.Execute, useImport, useExport);

    local.EmployerInfoMismatch.Count = useExport.EmployerMismatch.Count;
    local.EmployersCreated.Count = useExport.EmployersCreated.Count;
    local.Employer.Assign(useExport.Employer);
    local.EmployerAddress.Identifier = useExport.EmployerAddress.Identifier;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.PreviouslyCompared.Ein = useExport.PreviouslyCompared.Ein;
    local.AddressSuitableForIwo.Flag = useExport.AddressSuitableForIwo.Flag;
  }

  private void UseSiB273SendAlertNewIncSrce()
  {
    var useImport = new SiB273SendAlertNewIncSrce.Import();
    var useExport = new SiB273SendAlertNewIncSrce.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.Employment.Identifier = local.Employment.Identifier;
    useImport.Infrastructure.Detail = local.Infrastructure.Detail;

    Call(SiB273SendAlertNewIncSrce.Execute, useImport, useExport);
  }

  private void UseSiB273SsnMismatchAlertsGen()
  {
    var useImport = new SiB273SsnMismatchAlertsGen.Import();
    var useExport = new SiB273SsnMismatchAlertsGen.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.FederalCaseRegistry.Assign(local.FederalCaseRegistry);
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.Infrastructure.Assign(local.Infrastructure);
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

  private void UseSiB273UpdateEnlistmentDate()
  {
    var useImport = new SiB273UpdateEnlistmentDate.Import();
    var useExport = new SiB273UpdateEnlistmentDate.Export();

    useImport.Military.Identifier = entities.Military.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.DateOfHireUpdates.Count = local.DateOfHireUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.DateOfHire.Date = local.DateOfHireDateWorkArea.Date;
    MoveEmployer2(local.Employer, useImport.Employer);
    useImport.NewInfo.Note = local.Employment.Note;
    useImport.AlreadyProcessed.Flag = local.AlreadyProcessRpt.Flag;

    Call(SiB273UpdateEnlistmentDate.Execute, useImport, useExport);

    local.DateOfHireUpdates.Count = useExport.DateOfHireUpdates.Count;
    local.RecordsAlreadyProcessed.Count =
      useExport.RecordsAlreadyProcessed.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273ValidateAndNewhireRpt()
  {
    var useImport = new SiB273ValidateAndNewhireRpt.Import();
    var useExport = new SiB273ValidateAndNewhireRpt.Export();

    useImport.Process.Date = local.Process.Date;
    useImport.Employer.Assign(local.Employer);
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.CsePersonsWorkSet.Assign(local.FederalCaseRegistry);

    Call(SiB273ValidateAndNewhireRpt.Execute, useImport, useExport);

    local.Valid.Flag = useExport.Valid.Flag;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB273ValidateDateOfHire()
  {
    var useImport = new SiB273ValidateDateOfHire.Import();
    var useExport = new SiB273ValidateDateOfHire.Export();

    useImport.Hire.TextDate = local.DateOfHireDateWorkArea.TextDate;
    useImport.Process.Date = local.Process.Date;
    useImport.Kaecses.Number = local.Kaecses.Number;
    MoveEmployer2(local.Employer, useImport.Employer);

    Call(SiB273ValidateDateOfHire.Execute, useImport, useExport);

    local.DateOfHireDateWorkArea.Date = useExport.Hire.Date;
  }

  private void UseSiCabHireDateAlertIwo()
  {
    var useImport = new SiCabHireDateAlertIwo.Import();
    var useExport = new SiCabHireDateAlertIwo.Export();

    useImport.Employment.Identifier = entities.Employment.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.FederalNewhireIndicator.Flag = local.FederalNewhireIndicator.Flag;
    useImport.AutomaticGenerateIwo.Flag = local.AutomaticGenerateIwo.Flag;
    useImport.AddressSuitableForIwo.Flag = local.AddressSuitableForIwo.Flag;
    useImport.DateOfHireUpdates.Count = local.DateOfHireUpdates.Count;
    useImport.RecordsAlreadyProcessed.Count =
      local.RecordsAlreadyProcessed.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.Process.Date = local.Process.Date;
    useImport.DateOfHire.Date = local.DateOfHireDateWorkArea.Date;
    MoveEmployer2(local.Employer, useImport.Employer);
    MoveIncomeSource(local.Employment, useImport.NewInfo);

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
        db.SetString(command, "numb", local.FederalCaseRegistry.Number);
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
        entities.Employment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.Employment.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Employment.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Employment.CreatedBy = db.GetString(reader, 7);
        entities.Employment.CspINumber = db.GetString(reader, 8);
        entities.Employment.EmpId = db.GetNullableInt32(reader, 9);
        entities.Employment.WorkerId = db.GetNullableString(reader, 10);
        entities.Employment.StartDt = db.GetNullableDate(reader, 11);
        entities.Employment.EndDt = db.GetNullableDate(reader, 12);
        entities.Employment.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Employment.Type1);

        return true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "ssn", local.ConvertSsn.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMilitary()
  {
    entities.Military.Populated = false;

    return ReadEach("ReadMilitary",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableString(command, "ein", local.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Military.Identifier = db.GetDateTime(reader, 0);
        entities.Military.Type1 = db.GetString(reader, 1);
        entities.Military.ReturnDt = db.GetNullableDate(reader, 2);
        entities.Military.ReturnCd = db.GetNullableString(reader, 3);
        entities.Military.CspINumber = db.GetString(reader, 4);
        entities.Military.EmpId = db.GetNullableInt32(reader, 5);
        entities.Military.StartDt = db.GetNullableDate(reader, 6);
        entities.Military.EndDt = db.GetNullableDate(reader, 7);
        entities.Military.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.Military.Type1);

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
    /// A value of NewHire.
    /// </summary>
    [JsonPropertyName("newHire")]
    public IncomeSource NewHire
    {
      get => newHire ??= new();
      set => newHire = value;
    }

    /// <summary>
    /// A value of AlreadyProcessRpt.
    /// </summary>
    [JsonPropertyName("alreadyProcessRpt")]
    public Common AlreadyProcessRpt
    {
      get => alreadyProcessRpt ??= new();
      set => alreadyProcessRpt = value;
    }

    /// <summary>
    /// A value of ConvertDateCommon.
    /// </summary>
    [JsonPropertyName("convertDateCommon")]
    public Common ConvertDateCommon
    {
      get => convertDateCommon ??= new();
      set => convertDateCommon = value;
    }

    /// <summary>
    /// A value of ConvertDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateWorkArea")]
    public WorkArea ConvertDateWorkArea
    {
      get => convertDateWorkArea ??= new();
      set => convertDateWorkArea = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public WorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public WorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public WorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of ConvertSsn.
    /// </summary>
    [JsonPropertyName("convertSsn")]
    public SsnWorkArea ConvertSsn
    {
      get => convertSsn ??= new();
      set => convertSsn = value;
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
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
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
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of NarrativeDetailLineCnt.
    /// </summary>
    [JsonPropertyName("narrativeDetailLineCnt")]
    public Common NarrativeDetailLineCnt
    {
      get => narrativeDetailLineCnt ??= new();
      set => narrativeDetailLineCnt = value;
    }

    /// <summary>
    /// A value of ConvertMessage.
    /// </summary>
    [JsonPropertyName("convertMessage")]
    public SsnWorkArea ConvertMessage
    {
      get => convertMessage ??= new();
      set => convertMessage = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    private IncomeSource newHire;
    private Common alreadyProcessRpt;
    private Common convertDateCommon;
    private WorkArea convertDateWorkArea;
    private WorkArea year;
    private WorkArea day;
    private WorkArea month;
    private DateWorkArea convert;
    private SsnWorkArea convertSsn;
    private Common federalNewhireIndicator;
    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
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
    private CsePersonsWorkSet federalCaseRegistry;
    private EabFileHandling eabFileHandling;
    private IncomeSource employment;
    private AbendData abendData;
    private IncomeSource clear;
    private Infrastructure infrastructure;
    private Common alertsCreated;
    private ProgramProcessingInfo programProcessingInfo;
    private NarrativeDetail narrativeDetail;
    private Common narrativeDetailLineCnt;
    private SsnWorkArea convertMessage;
    private WorkArea message2;
    private WorkArea message1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private InvalidSsn invalidSsn;
    private IncomeSource employment;
    private IncomeSource military;
    private Employer employer;
    private CsePerson csePerson;
  }
#endregion
}
