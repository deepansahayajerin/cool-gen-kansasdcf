// Program: FN_B643_STATEMENT_HEADING, ID: 372683787, model: 746.
// Short name: SWE02404
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_STATEMENT_HEADING.
/// </summary>
[Serializable]
public partial class FnB643StatementHeading: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_STATEMENT_HEADING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643StatementHeading(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643StatementHeading.
  /// </summary>
  public FnB643StatementHeading(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * #77219  11-02-1999  Ed Lyman  Find correct caseworker for AR's.  *
    // *
    // 
    // *
    // * #78786  11-15-1999  Ed Lyman  Corrected view matching to eab for *
    // *
    // 
    // caseworker office and phone.
    // *
    // *
    // 
    // *
    // * #84048  01-05-2000  Ed Lyman  Suppress if address not verified.  *
    // ********************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.CaseAssignment.ReasonCode = "RSP";
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;
    export.StmtsCreated.Count = import.StmtsCreated.Count;
    UseEabReadCsePersonBatch();

    // ************************************************
    // *Interpret the error codes returned from ADABAS*
    // *and set an appropriate exit state.            *
    // ************************************************
    switch(AsChar(export.AbendData.Type1))
    {
      case ' ':
        // ************************************************
        // *Successful Adabas Read Occurred.             *
        // ************************************************
        break;
      case 'A':
        // ************************************************
        // *Unsuccessful ADABAS Read Occurred.           *
        // ************************************************
        switch(TrimEnd(export.AbendData.AdabasResponseCd))
        {
          case "0113":
            ExitState = "FN0000_CSE_PERSON_UNKNOWN";
            local.EabReportSend.RptDetail =
              "Adabas response code 113, person not found for number = " + import
              .Obligor2.Number;

            break;
          case "0148":
            ExitState = "ADABAS_UNAVAILABLE_RB";
            local.EabReportSend.RptDetail =
              "Adabas response code 148, unavailable fetching person number = " +
              import.Obligor2.Number;

            break;
          default:
            ExitState = "ADABAS_READ_UNSUCCESSFUL";
            local.EabReportSend.RptDetail = "Adabase error. Type = " + export
              .AbendData.Type1 + " File number = " + export
              .AbendData.AdabasFileNumber + " File action = " + export
              .AbendData.AdabasFileAction + " Response code = " + export
              .AbendData.AdabasResponseCd + " Person number = " + import
              .Obligor2.Number;

            break;
        }

        break;
      case 'C':
        // ************************************************
        // *CICS action Failed. A reason code should be   *
        // *interpreted.
        // 
        // *
        // ************************************************
        if (IsEmpty(export.AbendData.CicsResponseCd))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
        }

        local.EabReportSend.RptDetail =
          "CICS error fetching person number = " + import.Obligor2.Number;

        break;
      default:
        ExitState = "ADABAS_INVALID_RETURN_CODE";
        local.EabReportSend.RptDetail =
          "Unknown error fetching person number = " + import.Obligor2.Number;

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseCabErrorReport();

      return;
    }

    // ****************************************************************
    // IS THE OBLIGOR INCARCERATED
    // ****************************************************************
    if (ReadIncarceration())
    {
      foreach(var item in ReadIncarcerationAddress())
      {
        if (Lt(entities.IncarcerationAddress.EffectiveDate,
          import.ProcessDate.Date))
        {
          local.ActiveAddressFound.Flag = "Y";

          if (!IsEmpty(entities.Incarceration.InmateNumber))
          {
            local.Obligor.Street1 = "inmate " + entities
              .Incarceration.InmateNumber + "";
          }
          else
          {
            local.Obligor.Street1 = "in care of:";
          }

          local.Obligor.Street2 = entities.Incarceration.InstitutionName;
          local.Obligor.Street3 = entities.IncarcerationAddress.Street1;
          local.Obligor.Street4 = entities.IncarcerationAddress.Street2;
          local.Obligor.City = entities.IncarcerationAddress.City;
          local.Obligor.State = entities.IncarcerationAddress.State;
          local.Obligor.Country = entities.IncarcerationAddress.Country;
          local.Obligor.Province = entities.IncarcerationAddress.Province;
          local.Obligor.PostalCode = entities.IncarcerationAddress.PostalCode;
          local.Obligor.ZipCode = entities.IncarcerationAddress.ZipCode5;
          local.Obligor.Zip4 = entities.IncarcerationAddress.ZipCode4;
          local.Obligor.Zip3 = entities.IncarcerationAddress.Zip3;

          break;
        }
      }

      if (AsChar(local.ActiveAddressFound.Flag) != 'Y')
      {
        local.EabReportSend.RptDetail =
          "No active address for INCARCERATED Person number = " + export
          .Obligor.Number + " name= " + TrimEnd(export.Obligor.FirstName) + " " +
          export.Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
        UseCabErrorReport();
        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
      }
    }
    else
    {
      UseFnGetActiveCsePersonAddress();

      if (AsChar(local.ActiveAddressFound.Flag) != 'Y' || Equal
        (local.Obligor.VerifiedDate, local.Null1.Date))
      {
        local.EabReportSend.RptDetail =
          "No active address for Person number = " + export.Obligor.Number + " name= " +
          TrimEnd(export.Obligor.FirstName) + " " + export
          .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
        UseCabErrorReport();
        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
      }
    }

    ++export.StmtsCreated.Count;
    local.VariableLine1.RptDetail = "Statement Date: " + import
      .StatementDate.Text10 + "    Note: Payments received after this date will not appear on this statement.";
      
    local.VariableLine2.RptDetail = "Reporting Month: " + import
      .ReportingMonthYear.Text30;

    // **************************************************************
    // WRITE STATEMENT HEADER PART ONE (REC TYPE = 1)
    // **************************************************************
    local.RecordType.ActionEntry = "01";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;
    UseEabCreateVendorFile1();

    // *** Get the Contact Office, Person and his/her Phone number, This could 
    // be used as the Return Address
    //     
    // -----------------------------------------------------------------
    // ****************************************************************************
    // Find the Office Service Provider who is responsible (RSP) for the case.
    // Find the name of the Service Provider.
    // Determine if a Service Provider Address exists, otherwise use Office 
    // Address.
    // ****************************************************************************
    local.CaseWorkerFound.Flag = "N";

    foreach(var item in ReadCaseAssignmentCase())
    {
      if (Lt(import.ProcessDate.Date, entities.CaseAssignment.EffectiveDate) ||
        Lt(entities.CaseAssignment.DiscontinueDate, import.ProcessDate.Date))
      {
        continue;
      }

      if (ReadOfficeServiceProviderServiceProvider1())
      {
        local.CaseWorkerFound.Flag = "Y";
        local.CaseWorkerCsePersonsWorkSet.FirstName =
          entities.ServiceProvider.FirstName;
        local.CaseWorkerCsePersonsWorkSet.MiddleInitial =
          entities.ServiceProvider.MiddleInitial;
        local.CaseWorkerCsePersonsWorkSet.LastName =
          entities.ServiceProvider.LastName;
        local.CaseWorkerOffice.MainPhoneAreaCode =
          entities.OfficeServiceProvider.WorkPhoneAreaCode;
        local.CaseWorkerOffice.MainPhoneNumber =
          entities.OfficeServiceProvider.WorkPhoneNumber;

        // ****************************************************
        // Determine if a Service Provider Address exists,
        // otherwise use Office Address.
        // ****************************************************
        if (ReadServiceProviderAddress())
        {
          local.CaseWorkerCsePersonAddress.Street1 =
            entities.ServiceProviderAddress.Street1;
          local.CaseWorkerCsePersonAddress.Street2 =
            entities.ServiceProviderAddress.Street2;
          local.CaseWorkerCsePersonAddress.City =
            entities.ServiceProviderAddress.City;
          local.CaseWorkerCsePersonAddress.State =
            entities.ServiceProviderAddress.StateProvince;
          local.CaseWorkerCsePersonAddress.ZipCode =
            entities.ServiceProviderAddress.Zip;
          local.CaseWorkerCsePersonAddress.Zip4 =
            entities.ServiceProviderAddress.Zip4;
          local.CaseWorkerCsePersonAddress.Zip3 =
            entities.ServiceProviderAddress.Zip3;
        }
        else if (ReadOffice())
        {
          if (ReadOfficeAddress())
          {
            local.CaseWorkerCsePersonAddress.Street1 =
              entities.OfficeAddress.Street1;
            local.CaseWorkerCsePersonAddress.Street2 =
              entities.OfficeAddress.Street2;
            local.CaseWorkerCsePersonAddress.City = entities.OfficeAddress.City;
            local.CaseWorkerCsePersonAddress.State =
              entities.OfficeAddress.StateProvince;
            local.CaseWorkerCsePersonAddress.ZipCode =
              entities.OfficeAddress.Zip;
            local.CaseWorkerCsePersonAddress.Zip4 = entities.OfficeAddress.Zip4;
            local.CaseWorkerCsePersonAddress.Zip3 = entities.OfficeAddress.Zip3;
          }
          else
          {
            local.CaseWorkerCsePersonAddress.Street1 =
              "Office Address not found";
            local.EabReportSend.RptDetail =
              "No office address found for case worker for Person number = " + export
              .Obligor.Number + " name= " + TrimEnd
              (export.Obligor.FirstName) + " " + export
              .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
            UseCabErrorReport();
          }
        }
        else
        {
          local.CaseWorkerCsePersonAddress.Street1 = "Office not found";
          local.EabReportSend.RptDetail =
            "No office found for case worker for Person number = " + export
            .Obligor.Number + " name= " + TrimEnd(export.Obligor.FirstName) + " " +
            export.Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
          UseCabErrorReport();
        }

        break;
      }
      else
      {
        continue;
      }
    }

    if (AsChar(local.CaseWorkerFound.Flag) == 'N')
    {
      // *** COULD BE AN AR WHO IS ASSIGNED A CRU WORKER ***
      foreach(var item in ReadCaseAssignment())
      {
        if (ReadOfficeServiceProviderServiceProvider1())
        {
          local.CaseWorkerFound.Flag = "Y";
          local.CaseWorkerCsePersonsWorkSet.FirstName =
            entities.ServiceProvider.FirstName;
          local.CaseWorkerCsePersonsWorkSet.MiddleInitial =
            entities.ServiceProvider.MiddleInitial;
          local.CaseWorkerCsePersonsWorkSet.LastName =
            entities.ServiceProvider.LastName;
          local.CaseWorkerOffice.MainPhoneAreaCode =
            entities.OfficeServiceProvider.WorkPhoneAreaCode;
          local.CaseWorkerOffice.MainPhoneNumber =
            entities.OfficeServiceProvider.WorkPhoneNumber;

          // ****************************************************
          // Determine if a Service Provider Address exists,
          // otherwise use Office Address.
          // ****************************************************
          if (ReadServiceProviderAddress())
          {
            local.CaseWorkerCsePersonAddress.Street1 =
              entities.ServiceProviderAddress.Street1;
            local.CaseWorkerCsePersonAddress.Street2 =
              entities.ServiceProviderAddress.Street2;
            local.CaseWorkerCsePersonAddress.City =
              entities.ServiceProviderAddress.City;
            local.CaseWorkerCsePersonAddress.State =
              entities.ServiceProviderAddress.StateProvince;
            local.CaseWorkerCsePersonAddress.ZipCode =
              entities.ServiceProviderAddress.Zip;
            local.CaseWorkerCsePersonAddress.Zip4 =
              entities.ServiceProviderAddress.Zip4;
            local.CaseWorkerCsePersonAddress.Zip3 =
              entities.ServiceProviderAddress.Zip3;
          }
          else if (ReadOffice())
          {
            if (ReadOfficeAddress())
            {
              local.CaseWorkerCsePersonAddress.Street1 =
                entities.OfficeAddress.Street1;
              local.CaseWorkerCsePersonAddress.Street2 =
                entities.OfficeAddress.Street2;
              local.CaseWorkerCsePersonAddress.City =
                entities.OfficeAddress.City;
              local.CaseWorkerCsePersonAddress.State =
                entities.OfficeAddress.StateProvince;
              local.CaseWorkerCsePersonAddress.ZipCode =
                entities.OfficeAddress.Zip;
              local.CaseWorkerCsePersonAddress.Zip4 =
                entities.OfficeAddress.Zip4;
              local.CaseWorkerCsePersonAddress.Zip3 =
                entities.OfficeAddress.Zip3;
            }
            else
            {
              local.CaseWorkerCsePersonAddress.Street1 =
                "Office Address not found";
              local.EabReportSend.RptDetail =
                "No office address found for case worker for Person number = " +
                export.Obligor.Number + " name= " + TrimEnd
                (export.Obligor.FirstName) + " " + export
                .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
              UseCabErrorReport();
            }
          }
          else
          {
            local.CaseWorkerCsePersonAddress.Street1 = "Office not found";
            local.EabReportSend.RptDetail =
              "No office found for case worker for Person number = " + export
              .Obligor.Number + " name= " + TrimEnd
              (export.Obligor.FirstName) + " " + export
              .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
            UseCabErrorReport();
          }

          break;
        }
        else
        {
          continue;
        }
      }
    }

    if (AsChar(local.CaseWorkerFound.Flag) == 'N')
    {
      // *** COULD BE A NON CASE PERSON ***
      foreach(var item in ReadObligationAssignment())
      {
        if (ReadOfficeServiceProviderServiceProvider2())
        {
          local.CaseWorkerFound.Flag = "Y";
          local.CaseWorkerCsePersonsWorkSet.FirstName =
            entities.ServiceProvider.FirstName;
          local.CaseWorkerCsePersonsWorkSet.MiddleInitial =
            entities.ServiceProvider.MiddleInitial;
          local.CaseWorkerCsePersonsWorkSet.LastName =
            entities.ServiceProvider.LastName;
          local.CaseWorkerOffice.MainPhoneAreaCode =
            entities.OfficeServiceProvider.WorkPhoneAreaCode;
          local.CaseWorkerOffice.MainPhoneNumber =
            entities.OfficeServiceProvider.WorkPhoneNumber;

          // ****************************************************
          // Determine if a Service Provider Address exists,
          // otherwise use Office Address.
          // ****************************************************
          if (ReadServiceProviderAddress())
          {
            local.CaseWorkerCsePersonAddress.Street1 =
              entities.ServiceProviderAddress.Street1;
            local.CaseWorkerCsePersonAddress.Street2 =
              entities.ServiceProviderAddress.Street2;
            local.CaseWorkerCsePersonAddress.City =
              entities.ServiceProviderAddress.City;
            local.CaseWorkerCsePersonAddress.State =
              entities.ServiceProviderAddress.StateProvince;
            local.CaseWorkerCsePersonAddress.ZipCode =
              entities.ServiceProviderAddress.Zip;
            local.CaseWorkerCsePersonAddress.Zip4 =
              entities.ServiceProviderAddress.Zip4;
            local.CaseWorkerCsePersonAddress.Zip3 =
              entities.ServiceProviderAddress.Zip3;
          }
          else if (ReadOffice())
          {
            if (ReadOfficeAddress())
            {
              local.CaseWorkerCsePersonAddress.Street1 =
                entities.OfficeAddress.Street1;
              local.CaseWorkerCsePersonAddress.Street2 =
                entities.OfficeAddress.Street2;
              local.CaseWorkerCsePersonAddress.City =
                entities.OfficeAddress.City;
              local.CaseWorkerCsePersonAddress.State =
                entities.OfficeAddress.StateProvince;
              local.CaseWorkerCsePersonAddress.ZipCode =
                entities.OfficeAddress.Zip;
              local.CaseWorkerCsePersonAddress.Zip4 =
                entities.OfficeAddress.Zip4;
              local.CaseWorkerCsePersonAddress.Zip3 =
                entities.OfficeAddress.Zip3;
            }
            else
            {
              local.CaseWorkerCsePersonAddress.Street1 =
                "Office Address not found";
              local.EabReportSend.RptDetail =
                "No office address found for case worker for Person number = " +
                export.Obligor.Number + " name= " + TrimEnd
                (export.Obligor.FirstName) + " " + export
                .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
              UseCabErrorReport();
            }
          }
          else
          {
            local.CaseWorkerCsePersonAddress.Street1 = "Office not found";
            local.EabReportSend.RptDetail =
              "No office found for case worker for Person number = " + export
              .Obligor.Number + " name= " + TrimEnd
              (export.Obligor.FirstName) + " " + export
              .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
            UseCabErrorReport();
          }

          break;
        }
        else
        {
          continue;
        }
      }
    }

    if (AsChar(local.CaseWorkerFound.Flag) == 'N')
    {
      local.EabReportSend.RptDetail =
        "No active case worker for Person number = " + export.Obligor.Number + " name= " +
        TrimEnd(export.Obligor.FirstName) + " " + export
        .Obligor.MiddleInitial + "." + " " + export.Obligor.LastName;
      UseCabErrorReport();
      local.CaseWorkerCsePersonsWorkSet.FirstName = "No";
      local.CaseWorkerCsePersonsWorkSet.MiddleInitial = "";
      local.CaseWorkerCsePersonsWorkSet.LastName = "Caseworker";
      local.CaseWorkerOffice.MainPhoneAreaCode = 0;
      local.CaseWorkerOffice.MainPhoneNumber = 0;
      local.CaseWorkerCsePersonAddress.Street1 = "";
      local.CaseWorkerCsePersonAddress.Street2 = "";
      local.CaseWorkerCsePersonAddress.City = "";
      local.CaseWorkerCsePersonAddress.State = "";
      local.CaseWorkerCsePersonAddress.ZipCode = "";
      local.CaseWorkerCsePersonAddress.Zip4 = "";
      local.CaseWorkerCsePersonAddress.Zip3 = "";
    }

    // **************************************************************
    // WRITE STATEMENT HEADER PART TWO (REC TYPE = 2)
    // **************************************************************
    local.RecordType.ActionEntry = "02";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;
    UseEabCreateVendorFile3();

    // *** Get the Global Statement Message and Pass it to the EAB ***
    // << The following is RB's Assumption about the Global Statement Messages 
    // >>
    //      If the Global_Statement_Message effective_month = 12 AND 
    // effective_year = 2099,
    //         the message is applicable for all Obligors
    //      Else
    //         Message for each Obligor will be reported in his/her Statements
    //      END-IF
    //  
    // ----------------------------------------------------------
    if (ReadGlobalStatementMessage())
    {
      local.GlobalStatementMessage.TextArea =
        entities.GlobalStatementMessage.TextArea;
    }
    else
    {
      local.GlobalStatementMessage.TextArea =
        import.GlobalStatementMessage.TextArea;
    }

    // **************************************************************
    // WRITE STATEMENT HEADER PART THREE (REC TYPE = 3)
    // **************************************************************
    local.RecordType.ActionEntry = "03";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;
    UseEabCreateVendorFile2();
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.VerifiedDate = source.VerifiedDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile1()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.Client.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.CsePersonsWorkSet.Assign(export.Obligor);
    useImport.CaseWorker.Assign(entities.Office);
    useImport.CsePersonAddress.Assign(local.Obligor);
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.VariableLine2.RptDetail = local.VariableLine2.RptDetail;
    useImport.GlobalStatementMessage.TextArea =
      local.GlobalStatementMessage.TextArea;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile2()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.Client.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.CsePersonsWorkSet.Assign(export.Obligor);
    useImport.CaseWorker.Assign(entities.Office);
    useImport.CsePersonAddress.Assign(local.CaseWorkerCsePersonAddress);
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.VariableLine2.RptDetail = local.VariableLine2.RptDetail;
    useImport.GlobalStatementMessage.TextArea =
      local.GlobalStatementMessage.TextArea;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile3()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.Client.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.CsePersonsWorkSet.Assign(local.CaseWorkerCsePersonsWorkSet);
    useImport.CsePersonAddress.Assign(local.CaseWorkerCsePersonAddress);
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.VariableLine2.RptDetail = local.VariableLine2.RptDetail;
    useImport.GlobalStatementMessage.TextArea =
      local.GlobalStatementMessage.TextArea;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useImport.CaseWorker.Assign(local.CaseWorkerOffice);
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = import.Obligor2.Number;
    useExport.Kscares.Flag = local.Kscares.Flag;
    useExport.Kanpay.Flag = local.Kanpay.Flag;
    useExport.Cse.Flag = local.Cse.Flag;
    useExport.Ae.Flag = local.Ae.Flag;
    MoveCsePersonsWorkSet(export.Obligor, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(export.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.Kscares.Flag = useExport.Kscares.Flag;
    local.Kanpay.Flag = useExport.Kanpay.Flag;
    local.Cse.Flag = useExport.Cse.Flag;
    local.Ae.Flag = useExport.Ae.Flag;
    export.Obligor.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseFnGetActiveCsePersonAddress()
  {
    var useImport = new FnGetActiveCsePersonAddress.Import();
    var useExport = new FnGetActiveCsePersonAddress.Export();

    useImport.CsePerson.Number = import.Client.Number;
    useImport.AsOfDate.Date = import.ProcessDate.Date;
    useImport.CsePersonsWorkSet.Number = import.Obligor2.Number;

    Call(FnGetActiveCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Obligor);
    local.ActiveAddressFound.Flag = useExport.ActiveAddressFound.Flag;
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Client.Number);
        db.SetNullableDate(
          command, "startDate", import.ProcessDate.Date.GetValueOrDefault());
        db.SetString(command, "reasonCode", local.CaseAssignment.ReasonCode);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignmentCase()
  {
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignmentCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Client.Number);
        db.SetNullableDate(
          command, "startDate", import.ProcessDate.Date.GetValueOrDefault());
        db.SetString(command, "reasonCode", local.CaseAssignment.ReasonCode);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.Case1.Number = db.GetString(reader, 9);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadGlobalStatementMessage()
  {
    System.Diagnostics.Debug.Assert(import.Obligor1.Populated);
    entities.GlobalStatementMessage.Populated = false;

    return Read("ReadGlobalStatementMessage",
      (db, command) =>
      {
        db.
          SetDate(command, "date", import.ProcessDate.Date.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", import.Obligor1.CspNumber);
        db.SetString(command, "cpaType", import.Obligor1.Type1);
      },
      (db, reader) =>
      {
        entities.GlobalStatementMessage.EffectiveMonth = db.GetInt32(reader, 0);
        entities.GlobalStatementMessage.EffectiveYear = db.GetInt32(reader, 1);
        entities.GlobalStatementMessage.TextArea =
          db.GetNullableString(reader, 2);
        entities.GlobalStatementMessage.Populated = true;
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Client.Number);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.InmateNumber = db.GetNullableString(reader, 2);
        entities.Incarceration.InstitutionName =
          db.GetNullableString(reader, 3);
        entities.Incarceration.EndDateModInd = db.GetNullableString(reader, 4);
        entities.Incarceration.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncarcerationAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Incarceration.Populated);
    entities.IncarcerationAddress.Populated = false;

    return ReadEach("ReadIncarcerationAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "incIdentifier", entities.Incarceration.Identifier);
          
        db.SetString(command, "cspNumber", entities.Incarceration.CspNumber);
      },
      (db, reader) =>
      {
        entities.IncarcerationAddress.IncIdentifier = db.GetInt32(reader, 0);
        entities.IncarcerationAddress.CspNumber = db.GetString(reader, 1);
        entities.IncarcerationAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.IncarcerationAddress.Street1 = db.GetNullableString(reader, 3);
        entities.IncarcerationAddress.Street2 = db.GetNullableString(reader, 4);
        entities.IncarcerationAddress.City = db.GetNullableString(reader, 5);
        entities.IncarcerationAddress.State = db.GetNullableString(reader, 6);
        entities.IncarcerationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.IncarcerationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.IncarcerationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.IncarcerationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.IncarcerationAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.IncarcerationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.IncarcerationAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.ObligationAssignment.Populated = false;

    return ReadEach("ReadObligationAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 3);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 4);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 5);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 6);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 7);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 8);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 9);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 10);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.TypeCode = db.GetString(reader, 2);
        entities.Office.Name = db.GetString(reader, 3);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 6);
        entities.ServiceProvider.UserId = db.GetString(reader, 7);
        entities.ServiceProvider.LastName = db.GetString(reader, 8);
        entities.ServiceProvider.FirstName = db.GetString(reader, 9);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 10);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", entities.ObligationAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId", entities.ObligationAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.ObligationAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 6);
        entities.ServiceProvider.UserId = db.GetString(reader, 7);
        entities.ServiceProvider.LastName = db.GetString(reader, 8);
        entities.ServiceProvider.FirstName = db.GetString(reader, 9);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 10);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.PostalCode =
          db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.ServiceProviderAddress.Zip3 = db.GetNullableString(reader, 9);
        entities.ServiceProviderAddress.Populated = true;
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
    /// <summary>
    /// A value of StmtsCreated.
    /// </summary>
    [JsonPropertyName("stmtsCreated")]
    public Common StmtsCreated
    {
      get => stmtsCreated ??= new();
      set => stmtsCreated = value;
    }

    /// <summary>
    /// A value of ReportingMonthYear.
    /// </summary>
    [JsonPropertyName("reportingMonthYear")]
    public TextWorkArea ReportingMonthYear
    {
      get => reportingMonthYear ??= new();
      set => reportingMonthYear = value;
    }

    /// <summary>
    /// A value of StatementDate.
    /// </summary>
    [JsonPropertyName("statementDate")]
    public TextWorkArea StatementDate
    {
      get => statementDate ??= new();
      set => statementDate = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonsWorkSet Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    private Common stmtsCreated;
    private TextWorkArea reportingMonthYear;
    private TextWorkArea statementDate;
    private CsePerson client;
    private CsePersonAccount obligor1;
    private Common stmtNumber;
    private DateWorkArea processDate;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private CsePersonsWorkSet obligor2;
    private Obligation obligation;
    private ObligationType obligationType;
    private GlobalStatementMessage globalStatementMessage;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of StmtsCreated.
    /// </summary>
    [JsonPropertyName("stmtsCreated")]
    public Common StmtsCreated
    {
      get => stmtsCreated ??= new();
      set => stmtsCreated = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
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

    private Common sortSequenceNumber;
    private CsePersonsWorkSet obligor;
    private Common stmtsCreated;
    private Common vendorFileRecordCount;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAddress Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CaseWorkerOffice.
    /// </summary>
    [JsonPropertyName("caseWorkerOffice")]
    public Office CaseWorkerOffice
    {
      get => caseWorkerOffice ??= new();
      set => caseWorkerOffice = value;
    }

    /// <summary>
    /// A value of CaseWorkerCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("caseWorkerCsePersonsWorkSet")]
    public CsePersonsWorkSet CaseWorkerCsePersonsWorkSet
    {
      get => caseWorkerCsePersonsWorkSet ??= new();
      set => caseWorkerCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CaseWorkerCsePersonAddress.
    /// </summary>
    [JsonPropertyName("caseWorkerCsePersonAddress")]
    public CsePersonAddress CaseWorkerCsePersonAddress
    {
      get => caseWorkerCsePersonAddress ??= new();
      set => caseWorkerCsePersonAddress = value;
    }

    /// <summary>
    /// A value of VariableLine2.
    /// </summary>
    [JsonPropertyName("variableLine2")]
    public EabReportSend VariableLine2
    {
      get => variableLine2 ??= new();
      set => variableLine2 = value;
    }

    /// <summary>
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of ActiveAddressFound.
    /// </summary>
    [JsonPropertyName("activeAddressFound")]
    public Common ActiveAddressFound
    {
      get => activeAddressFound ??= new();
      set => activeAddressFound = value;
    }

    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
    }

    /// <summary>
    /// A value of Kanpay.
    /// </summary>
    [JsonPropertyName("kanpay")]
    public Common Kanpay
    {
      get => kanpay ??= new();
      set => kanpay = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of CaseWorkerFound.
    /// </summary>
    [JsonPropertyName("caseWorkerFound")]
    public Common CaseWorkerFound
    {
      get => caseWorkerFound ??= new();
      set => caseWorkerFound = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    private DateWorkArea null1;
    private CsePersonAddress obligor;
    private Office caseWorkerOffice;
    private CsePersonsWorkSet caseWorkerCsePersonsWorkSet;
    private CsePersonAddress caseWorkerCsePersonAddress;
    private EabReportSend variableLine2;
    private EabReportSend variableLine1;
    private CaseAssignment caseAssignment;
    private Common recordType;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common activeAddressFound;
    private Common kscares;
    private Common kanpay;
    private Common cse;
    private Common ae;
    private Common caseWorkerFound;
    private GlobalStatementMessage globalStatementMessage;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of IncarcerationAddress.
    /// </summary>
    [JsonPropertyName("incarcerationAddress")]
    public IncarcerationAddress IncarcerationAddress
    {
      get => incarcerationAddress ??= new();
      set => incarcerationAddress = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    /// <summary>
    /// A value of ActivityStatement.
    /// </summary>
    [JsonPropertyName("activityStatement")]
    public ActivityStatement ActivityStatement
    {
      get => activityStatement ??= new();
      set => activityStatement = value;
    }

    private ObligationAssignment obligationAssignment;
    private CaseRole applicantRecipient;
    private IncarcerationAddress incarcerationAddress;
    private Incarceration incarceration;
    private ServiceProviderAddress serviceProviderAddress;
    private CaseRole absentParent;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeAddress officeAddress;
    private GlobalStatementMessage globalStatementMessage;
    private ActivityStatement activityStatement;
  }
#endregion
}
