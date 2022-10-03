// Program: LE_BFX3_TRIBAL_REPORT, ID: 371308788, model: 746.
// Short name: SWELFX3B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX3_TRIBAL_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx3TribalReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX3_TRIBAL_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx3TribalReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx3TribalReport.
  /// </summary>
  public LeBfx3TribalReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 12/22/06  GVandy			Initial Development.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.Current.Date;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Tribal Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.RptHeading3 = "                          TRIBAL CASES";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the business report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Input File (i.e. the sorted extract file)
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeBfx3ReadExtractDataFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the input file.  Return Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the first record in the Input File (i.e. the sorted extract file
    // )
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "READ";
    UseLeBfx3ReadExtractDataFile1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      if (Equal(local.EabFileHandling.Status, "EF"))
      {
        goto Test1;
      }

      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the input file.  Return Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

Test1:

    while(!Equal(local.EabFileHandling.Status, "EF"))
    {
      ++local.NumberOfCases.Count;

      if (!Equal(local.CsePersonAddress.ZipCode, local.Previous.ZipCode) || !
        Equal(local.CsePersonAddress.City, local.Previous.City))
      {
        if (IsEmpty(local.Previous.City) && IsEmpty(local.Previous.ZipCode))
        {
          // -- First page of the report.  No need to do a page break.
        }
        else
        {
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            if (Equal(local.EabFileHandling.Status, "EF"))
            {
              goto Test2;
            }

            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing page breaking to the business report.  Return Status = " +
              local.EabFileHandling.Status;
            UseCabErrorReport2();
            ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

            return;
          }
        }

Test2:

        local.Previous.Assign(local.CsePersonAddress);

        // -- Write heading on the page.
        for(local.Common.Count = 2; local.Common.Count <= 9; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 3:
              local.EabReportSend.RptDetail = "ZIP CODE  " + (
                local.CsePersonAddress.ZipCode ?? "") + "  " + (
                  local.CsePersonAddress.City ?? "");

              break;
            case 6:
              local.EabReportSend.RptDetail = "IV-D";

              break;
            case 7:
              local.EabReportSend.RptDetail = "CASE";

              break;
            case 8:
              local.EabReportSend.RptDetail =
                "NUMBER     ROLE NAME                                 BIRTH DATE  SSN          ADDRESS";
                

              break;
            case 9:
              local.EabReportSend.RptDetail =
                "---------- ---- -------------------------------      ----------  -----------  -----------------------------------------------------";
                

              break;
            default:
              local.EabReportSend.RptDetail = "";

              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            if (Equal(local.EabFileHandling.Status, "EF"))
            {
              goto Test3;
            }

            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing page headings to the business report.  Return Status = " +
              local.EabFileHandling.Status;
            UseCabErrorReport2();
            ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

            return;
          }

Test3:
          ;
        }
      }

      local.WriteCaseNumberToRpt.Flag = "Y";

      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.CaseRole.Type1 = "AP";

            break;
          case 2:
            local.CaseRole.Type1 = "AR";

            break;
          case 3:
            local.CaseRole.Type1 = "CH";

            break;
          default:
            break;
        }

        foreach(var item in ReadCsePerson())
        {
          if (Equal(local.CaseRole.Type1, "CH"))
          {
            // -- Don't need to get address for children.
          }
          else
          {
            local.CsePersonAddress.Assign(local.Null1);

            // -- Find an active residential address within the city/zip code 
            // being reported.
            if (ReadCsePersonAddress1())
            {
              local.CsePersonAddress.Assign(entities.CsePersonAddress);

              goto Test4;
            }

            // -- We did not find an active residential address within the city/
            // zip code being reported.  Find any active residential address.
            if (ReadCsePersonAddress2())
            {
              local.CsePersonAddress.Assign(entities.CsePersonAddress);
            }
          }

Test4:

          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePerson();

          // -- Build the report line.
          local.EabReportSend.RptDetail = "";

          // -- Only write the case number to the report once.
          if (AsChar(local.WriteCaseNumberToRpt.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = local.Case1.Number;
            local.WriteCaseNumberToRpt.Flag = "N";
          }

          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 12) + local.CaseRole.Type1;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 16) + local
            .CsePersonsWorkSet.FormattedName;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
            (Month(local.CsePersonsWorkSet.Dob), 14, 2);
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 55) + "-";
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 56) + NumberToString
            (Day(local.CsePersonsWorkSet.Dob), 14, 2);
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 58) + "-";
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 59) + NumberToString
            (Year(local.CsePersonsWorkSet.Dob), 12, 4);
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 65) + local
            .CsePersonsWorkSet.Ssn;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 65) + Substring
            (local.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 1, 3);
            
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 68) + "-";
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 69) + Substring
            (local.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2);
            
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 71) + "-";
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 72) + Substring
            (local.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            

          if (Equal(local.CaseRole.Type1, "CH"))
          {
            // -- Don't include an address for children.
          }
          else
          {
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 78) + (
                local.CsePersonAddress.Street1 ?? "");
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 105) + (
                local.CsePersonAddress.City ?? "");
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 121) + (
                local.CsePersonAddress.State ?? "");
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 124) + (
                local.CsePersonAddress.ZipCode ?? "");
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            if (Equal(local.EabFileHandling.Status, "EF"))
            {
              goto Test5;
            }

            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing detail lines to the business report.  Return Status = " +
              local.EabFileHandling.Status;
            UseCabErrorReport2();
            ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

            return;
          }

Test5:
          ;
        }
      }

      // -- Write a blank line between each case.
      local.EabReportSend.RptDetail = "";
      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        if (Equal(local.EabFileHandling.Status, "EF"))
        {
          goto Test6;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing detail lines to the business report.  Return Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

        return;
      }

Test6:

      // -- Get next record from input file.
      local.EabFileHandling.Action = "READ";
      UseLeBfx3ReadExtractDataFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        if (Equal(local.EabFileHandling.Status, "EF"))
        {
          goto Test7;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading the input file.  Return Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

        return;
      }

Test7:
      ;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail =
      "Number of Cases Written to Tribal Report      " + NumberToString
      (local.NumberOfCases.Count, 10, 6);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(01) Error Writing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Input File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeBfx3ReadExtractDataFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Input File...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Tribal Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing the business report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.City = source.City;
    target.ZipCode = source.ZipCode;
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

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseLeBfx3ReadExtractDataFile1()
  {
    var useImport = new LeBfx3ReadExtractDataFile.Import();
    var useExport = new LeBfx3ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.CsePersonAddress.Assign(local.CsePersonAddress);
    useExport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    useExport.Case1.Number = local.Case1.Number;

    Call(LeBfx3ReadExtractDataFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    local.Case1.Number = useExport.Case1.Number;
  }

  private void UseLeBfx3ReadExtractDataFile2()
  {
    var useImport = new LeBfx3ReadExtractDataFile.Import();
    var useExport = new LeBfx3ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx3ReadExtractDataFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "type", local.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "city", local.Previous.City ?? "");
        db.SetNullableString(command, "zipCode", local.Previous.ZipCode ?? "");
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 8);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 8);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonAddress Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of WriteCaseNumberToRpt.
    /// </summary>
    [JsonPropertyName("writeCaseNumberToRpt")]
    public Common WriteCaseNumberToRpt
    {
      get => writeCaseNumberToRpt ??= new();
      set => writeCaseNumberToRpt = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonAddress Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of NumberOfCases.
    /// </summary>
    [JsonPropertyName("numberOfCases")]
    public Common NumberOfCases
    {
      get => numberOfCases ??= new();
      set => numberOfCases = value;
    }

    private CsePersonAddress null1;
    private Common writeCaseNumberToRpt;
    private CaseRole caseRole;
    private Common common;
    private CsePersonAddress previous;
    private Case1 case1;
    private CsePersonAddress csePersonAddress;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common numberOfCases;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private CaseRole caseRole;
    private Case1 case1;
    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
  }
#endregion
}
