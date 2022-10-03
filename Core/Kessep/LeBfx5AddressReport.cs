// Program: LE_BFX5_ADDRESS_REPORT, ID: 371311361, model: 746.
// Short name: SWELFX5B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX5_ADDRESS_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx5AddressReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX5_ADDRESS_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx5AddressReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx5AddressReport.
  /// </summary>
  public LeBfx5AddressReport(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWELBFX4";
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
    // --  Open the Address Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.RptHeading3 =
      "                      ADHOC ADDRESS REPORT";
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
    UseLeBfx5ReadExtractDataFile2();

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
        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing detail lines to the business report.  Return Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }

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

    // -- Write report headings.
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "PERSON";

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "NUMBER      NAME                                ADDRESS";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "----------  ----------------------------------  --------------------------------------------------------------------------------";
            

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
          goto Test1;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing page headings to the business report.  Return Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

Test1:
      ;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the first record in the Input File (i.e. the sorted extract file
    // )
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "READ";
    UseLeBfx5ReadExtractDataFile1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      if (Equal(local.EabFileHandling.Status, "EF"))
      {
        goto Test2;
      }

      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the input file.  Return Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ERROR_READING_FILE_AB";

      return;
    }

Test2:

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Create the report.
    // -------------------------------------------------------------------------------------------------------------------------
    while(!Equal(local.EabFileHandling.Status, "EF"))
    {
      ++local.NumberOfPersons.Count;

      // -- Build the report line.
      local.EabReportSend.RptDetail = "";
      local.EabReportSend.RptDetail = local.CsePersonsWorkSet.Number;
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 12) + local
        .CsePersonsWorkSet.FormattedName;
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 48) + (
          local.CsePersonAddress.Street1 ?? "");
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 75) + (
          local.CsePersonAddress.Street2 ?? "");
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 102) + (
          local.CsePersonAddress.City ?? "");
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 119) + (
          local.CsePersonAddress.State ?? "");
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 123) + (
          local.CsePersonAddress.ZipCode ?? "");
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
          "Error writing detail lines to the business report.  Return Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

Test3:

      // -- Get next record from input file.
      local.EabFileHandling.Action = "READ";
      UseLeBfx5ReadExtractDataFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        if (Equal(local.EabFileHandling.Status, "EF"))
        {
          goto Test4;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading the input file.  Return Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ERROR_READING_FILE_AB";

        return;
      }

Test4:
      ;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail =
      "Number of Persons Written to Address Report      " + NumberToString
      (local.NumberOfPersons.Count, 10, 6);
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
    UseLeBfx5ReadExtractDataFile2();

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
    // --  Close the Address Report.
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

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private void UseLeBfx5ReadExtractDataFile1()
  {
    var useImport = new LeBfx5ReadExtractDataFile.Import();
    var useExport = new LeBfx5ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.CsePersonAddress.Assign(local.CsePersonAddress);

    Call(LeBfx5ReadExtractDataFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseLeBfx5ReadExtractDataFile2()
  {
    var useImport = new LeBfx5ReadExtractDataFile.Import();
    var useExport = new LeBfx5ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx5ReadExtractDataFile.Execute, useImport, useExport);

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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of NumberOfPersons.
    /// </summary>
    [JsonPropertyName("numberOfPersons")]
    public Common NumberOfPersons
    {
      get => numberOfPersons ??= new();
      set => numberOfPersons = value;
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
    /// A value of PpiCaseRole.
    /// </summary>
    [JsonPropertyName("ppiCaseRole")]
    public CaseRole PpiCaseRole
    {
      get => ppiCaseRole ??= new();
      set => ppiCaseRole = value;
    }

    private Common common;
    private CsePersonAddress csePersonAddress;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common numberOfPersons;
    private Office ppiOffice;
    private CaseRole ppiCaseRole;
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

    private Office office;
  }
#endregion
}
