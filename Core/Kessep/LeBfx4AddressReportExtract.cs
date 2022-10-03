// Program: LE_BFX4_ADDRESS_REPORT_EXTRACT, ID: 371310347, model: 746.
// Short name: SWELFX4B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX4_ADDRESS_REPORT_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx4AddressReportExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX4_ADDRESS_REPORT_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx4AddressReportExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx4AddressReportExtract.
  /// </summary>
  public LeBfx4AddressReportExtract(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 02/05/07  GVandy	WR300810	Initial Development.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

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
    // --  Open the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeBfx4WriteExtractDataFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening output file.  Return Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Extract Office Number and Case Role Type from the PPI record.
    // -------------------------------------------------------------------------------------------------------------------------
    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Office and Case Role Type not entered on PPI Record." + "";
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }
    else
    {
      if (Verify(Substring(local.ProgramProcessingInfo.ParameterList, 1, 4),
        "0123456789") > 0)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Invalid Office ID entered on PPI Record.  Office ID = " + Substring
          (local.ProgramProcessingInfo.ParameterList, 1, 4);
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.PpiOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 4));

      if (!ReadOffice())
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Office ID entered on PPI Record Not Found.  Office ID = " + Substring
          (local.ProgramProcessingInfo.ParameterList, 1, 4);
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.PpiCaseRole.Type1 =
        Substring(local.ProgramProcessingInfo.ParameterList, 5, 2);

      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail = "OFFICE: " + Substring
            (local.ProgramProcessingInfo.ParameterList, 1, 4);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  " + entities
            .Office.Name;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  CASE ROLE TYPE: ";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + local
            .PpiCaseRole.Type1;
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

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
      }
    }

    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Find all distinct cse persons with the specified case role type in 
    // cases assigned to the specified office.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      MoveCsePersonsWorkSet(local.Null1, local.CsePersonsWorkSet);

      // -- Read name from adabas.
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        for(local.Common.Count = 1; local.Common.Count <= 6; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail =
                "Error Reading Adabas...  CSP Number = " + entities
                .CsePerson.Number;

              break;
            case 2:
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "       Exit State... " + local
                .ExitStateWorkArea.Message;

              break;
            case 3:
              local.EabReportSend.RptDetail =
                "       Returned Adabas File Action = " + local
                .AbendData.AdabasFileAction;

              break;
            case 4:
              local.EabReportSend.RptDetail =
                "       Returned Adabas File Number = " + local
                .AbendData.AdabasFileNumber;

              break;
            case 5:
              local.EabReportSend.RptDetail =
                "       Returned Adabas Response Code = " + local
                .AbendData.AdabasResponseCd;

              break;
            default:
              local.EabReportSend.RptDetail = "";

              break;
          }

          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();
        }

        ++local.NumberOfAdabasErrors.Count;

        // -- Skip the person.
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // -- Find an active mailing address.
      ReadCsePersonAddress1();

      if (!entities.CsePersonAddress.Populated)
      {
        // -- No active Mailing address was found.  Look for an active 
        // Residential address.
        ReadCsePersonAddress2();
      }

      if (!entities.CsePersonAddress.Populated)
      {
        // -- Person has no active Mailing or Residential address.  Do not 
        // include the person on the report.
        continue;
      }

      local.EabFileHandling.Action = "WRITE";
      UseLeBfx4WriteExtractDataFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Writing Extract Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.NumberOfCsePersons.Count;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail =
      "Number of Adabas Errors (see error report)     " + NumberToString
      (local.NumberOfAdabasErrors.Count, 10, 6);
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

    local.EabReportSend.RptDetail =
      "Number of CSE Persons Successfully Extracted   " + NumberToString
      (local.NumberOfCsePersons.Count, 10, 6);
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
    // --  Close the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeBfx4WriteExtractDataFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Extract File...  Returned Status = " + local
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

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private void UseLeBfx4WriteExtractDataFile1()
  {
    var useImport = new LeBfx4WriteExtractDataFile.Import();
    var useExport = new LeBfx4WriteExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
    useImport.CsePersonAddress.Assign(entities.CsePersonAddress);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx4WriteExtractDataFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeBfx4WriteExtractDataFile2()
  {
    var useImport = new LeBfx4WriteExtractDataFile.Import();
    var useExport = new LeBfx4WriteExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx4WriteExtractDataFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "type", local.PpiCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
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
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 9);
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
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 9);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", local.PpiOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
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
    /// A value of NumberOfAdabasErrors.
    /// </summary>
    [JsonPropertyName("numberOfAdabasErrors")]
    public Common NumberOfAdabasErrors
    {
      get => numberOfAdabasErrors ??= new();
      set => numberOfAdabasErrors = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of PpiCaseRole.
    /// </summary>
    [JsonPropertyName("ppiCaseRole")]
    public CaseRole PpiCaseRole
    {
      get => ppiCaseRole ??= new();
      set => ppiCaseRole = value;
    }

    /// <summary>
    /// A value of PpiOffice.
    /// </summary>
    [JsonPropertyName("ppiOffice")]
    public Office PpiOffice
    {
      get => ppiOffice ??= new();
      set => ppiOffice = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonsWorkSet Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of NumberOfCsePersons.
    /// </summary>
    [JsonPropertyName("numberOfCsePersons")]
    public Common NumberOfCsePersons
    {
      get => numberOfCsePersons ??= new();
      set => numberOfCsePersons = value;
    }

    private Common numberOfAdabasErrors;
    private AbendData abendData;
    private Common common;
    private CaseRole ppiCaseRole;
    private Office ppiOffice;
    private CsePersonsWorkSet null1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common numberOfCsePersons;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}
