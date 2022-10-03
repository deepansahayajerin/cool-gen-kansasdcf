// Program: LE_BFX9_529_PART_CHARACTERISTICS, ID: 1902452648, model: 746.
// Short name: SWEFX9B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX9_529_PART_CHARACTERISTICS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx9529PartCharacteristics: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX9_529_PART_CHARACTERISTICS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx9529PartCharacteristics(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx9529PartCharacteristics.
  /// </summary>
  public LeBfx9529PartCharacteristics(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 12/12/14  GVandy	CQ42192		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // Pilot Group Data Analysis - added 10/9/14
    // The following data elements will be pulled for the NCPs (unless 
    // otherwise noted).  The data elements will be sorted by Statewide (
    // qualifying NCP/CH participants), Pilot Counties (SG and WY) and Pilot
    // Sample (1,000 chosen by research group).
    // 	26. Date of Birth
    // 	27. Gender
    // 	    a. Male
    // 	    b. Female
    // 	28. Race/ethnicity
    // 	    a. AI-American Indian/Alaskan Native
    // 	    b. AJ-American Indian/Tribal Job
    // 	    c. BL-Black/African American
    // 	    d. DC-Declined
    // 	    e. HI-Hispanic
    // 	    f. HP-Native Hawaiian/Pacific Islander
    // 	    g. OT-Other
    // 	    h. SA-Asian
    // 	    i. WH-White
    // 	    j. Blank
    // 	29. Currently employed (Y/N)
    // 	    a. At least one active (non-end dated) employer (INCS) exists for 
    // the
    // 	       NCP as of the run date.
    // 	    b. Count only records with one of the following types: E(mployment),
    // 	       M(ilitary), O(ther).  Do not count R(esource) type.
    // 	    c. Count records with the following Type/Return Code combinations:
    // 	       E/E, M/A, M/R and O/V. Do not count records with the following
    // 	       Type/Return Code combinations: E/F, E/L, E/N, E/O, E/Q, E/U, E/W,
    // 	       M/I, M/N, M/U, O/N.
    // 	30. Earned income in the past year- use income source for the last four
    // 	    quarters to calculate the total.  Include active and inactive income
    // 	    sources.
    // 	31. Number of child support courts order for the NCP. The court orders
    // 	    will be active and obligated.
    // 	32. Family arrears balance on all qualifying orders.  Included NA and 
    // NAI.
    // 	33. Total arrears owed across all orders for the qualifying NCP.
    // 	34. Date of last incoming withholding (I) payment for qualifying NCP.
    // 	35. Date of last UI withholding (U) payment for qualifying NCP.
    // 	36. Date of most recent payment for qualifying NCP. Include REIP and
    // 	    CSENet payments.
    // 	37. New Order for qualifying NCP. Count if there isnt a payment in the
    // 	    last year and the oldest created timestamp of the debts is within 
    // the
    // 	    previous 12 months. Y/N
    // 	38. Number of minor age children.
    // 	    a. Count all children under the age of 18.
    // 	    b. The child must be tied to the qualifying NCP.
    // 	    c. The NCP must be the NCP or CP on the qualifying case.
    // 	39. NCP ever incarcerated in jail or prison. Include only the date of 
    // the
    // 	    most recent record.
    // 	    a. Most recent start date or verified date entered jail or prison.
    // 	       This may be blank.
    // 	    OR
    // 	    b. Most recent release date from jail or prison. This may be blank.
    // 	
    // The following data elements will be on a separate file.
    // 	40. Federal benefit receipt (Include all open programs). If there are 
    // not
    // 	    any open programs display as blank. This is  for NCP, CP and CH that
    // 	    meet qualifications. Indicate a "Y" for each participant if any of
    // 	    the following programs are open, if the participant isnt open for 
    // any
    // 	    of the programs indicate "N":
    // 	    a. AF    AFDC
    // 	    b. AFI   AFDC INTERSTATE
    // 	    c. CC    CHILD CARE
    // 	    d. CI    MA - FOR CHILD IN AN INSTITUTION
    // 	    e. FC    AFDC FOSTER CARE
    // 	    f. FCI   FOSTER CARE INTERSTATE
    // 	    g. FS    FOOD STAMPS
    // 	    h. MA    MEDICAID RELATED TO TAF
    // 	    i. MAI   MEDICAL ASSISTANCE INTERSTATE
    // 	    j. MK    POVERTY LEVEL MEDICAID (OLD)
    // 	    k. MP    MEDICAL PG/CHILD/HEALTHWAVE
    // 	    l. MS    MEDICAID RELATED TO SSI
    // 	    m. NA    NON - AFDC
    // 	    n. NAI   NON - AFDC INTERSTATE
    // 	    o. NC    JUVENILE JUSTICE AUTHORITY
    // 	    p. NF    GA FOSTER CARE
    // 	    q. SI    MA - CHILD RECEIVES SSI
    // 	41. Count any qualifying NCP/CH combination where there is an accruing
    // 	    child support debt detail for the current month. Y/N
    // 	42. Count if the qualifying NCP/CH combination is arrears only, no
    // 	    current support owed.  Y/N
    // 	43. Birthdate of each qualifying child.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWELBFX9";
    UseReadProgramProcessingInfo();

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
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

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
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

    // -------------------------------------------------------------------------------------
    // --  Open the NCP Address Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeBfx9ReadNcpAddressFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening NCP Address input file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the NCP Info Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeBfx9WriteNcpFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening NCP output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the NCP/Child/CP Info Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeBfx9WriteNcpChildFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening NCP/Child/CP output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Process each NCP Address record creating the appropriate records in 
    // the NCP and
    // -- NCP/CH/CP info files.
    // -------------------------------------------------------------------------------------
    do
    {
      // --  Read a record from the NCP Address Input File.
      local.EabFileHandling.Action = "READ";
      UseLeBfx9ReadNcpAddressFile1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          // -- Continue
          break;
        case "EF":
          continue;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading NCP Address input file.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      ++local.NcpAddressInputRecord.Count;

      // --  Process the NCP creating NCP and NCP/CH/CP output file records.
      local.CsePerson.Number =
        Substring(local.CssiWorkset.NcpAddressFileLayout, 1, 10);
      UseLeBfx9ProcessNcp();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Error Processing NCP " + local
          .CsePerson.Number + ".  " + local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();
        local.EabFileHandling.Action = "CLOSE";
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of NCPs Read from NCP Address File................................." +
            NumberToString(local.NcpAddressInputRecord.Count, 9, 7);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Number of NCPs Written to the NCP Info Output File........................" +
            NumberToString(local.NcpOutputRecord.Count, 9, 7);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Number of NCP/CH/CP Combos Written to the NCP/CH/CP Info Output File......" +
            NumberToString(local.NcpChildCpOutputRecord.Count, 9, 7);

          break;
        default:
          break;
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

    // -------------------------------------------------------------------------------------
    // --  Close the NCP Address Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeBfx9ReadNcpAddressFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing NCP Address input file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the NCP Info Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeBfx9WriteNcpFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing NCP output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the NCP/Child/CP Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeBfx9WriteNcpChildFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing NCP/Child/CP output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
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

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseLeBfx9ProcessNcp()
  {
    var useImport = new LeBfx9ProcessNcp.Import();
    var useExport = new LeBfx9ProcessNcp.Export();

    useImport.Ncp.Number = local.CsePerson.Number;
    useImport.ExportNcpChCpRecord.Count = local.NcpChildCpOutputRecord.Count;
    useImport.ExportNcpRecord.Count = local.NcpOutputRecord.Count;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(LeBfx9ProcessNcp.Execute, useImport, useExport);

    local.NcpChildCpOutputRecord.Count = useImport.ExportNcpChCpRecord.Count;
    local.NcpOutputRecord.Count = useImport.ExportNcpRecord.Count;
  }

  private void UseLeBfx9ReadNcpAddressFile1()
  {
    var useImport = new LeBfx9ReadNcpAddressFile.Import();
    var useExport = new LeBfx9ReadNcpAddressFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.CssiWorkset.NcpAddressFileLayout =
      local.CssiWorkset.NcpAddressFileLayout;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx9ReadNcpAddressFile.Execute, useImport, useExport);

    local.CssiWorkset.NcpAddressFileLayout =
      useExport.CssiWorkset.NcpAddressFileLayout;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeBfx9ReadNcpAddressFile2()
  {
    var useImport = new LeBfx9ReadNcpAddressFile.Import();
    var useExport = new LeBfx9ReadNcpAddressFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx9ReadNcpAddressFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeBfx9WriteNcpChildFile()
  {
    var useImport = new LeBfx9WriteNcpChildFile.Import();
    var useExport = new LeBfx9WriteNcpChildFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx9WriteNcpChildFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeBfx9WriteNcpFile()
  {
    var useImport = new LeBfx9WriteNcpFile.Import();
    var useExport = new LeBfx9WriteNcpFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx9WriteNcpFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CssiWorkset.
    /// </summary>
    [JsonPropertyName("cssiWorkset")]
    public CssiWorkset CssiWorkset
    {
      get => cssiWorkset ??= new();
      set => cssiWorkset = value;
    }

    /// <summary>
    /// A value of NcpChildCpOutputRecord.
    /// </summary>
    [JsonPropertyName("ncpChildCpOutputRecord")]
    public Common NcpChildCpOutputRecord
    {
      get => ncpChildCpOutputRecord ??= new();
      set => ncpChildCpOutputRecord = value;
    }

    /// <summary>
    /// A value of NcpOutputRecord.
    /// </summary>
    [JsonPropertyName("ncpOutputRecord")]
    public Common NcpOutputRecord
    {
      get => ncpOutputRecord ??= new();
      set => ncpOutputRecord = value;
    }

    /// <summary>
    /// A value of NcpAddressInputRecord.
    /// </summary>
    [JsonPropertyName("ncpAddressInputRecord")]
    public Common NcpAddressInputRecord
    {
      get => ncpAddressInputRecord ??= new();
      set => ncpAddressInputRecord = value;
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

    private CsePerson csePerson;
    private CssiWorkset cssiWorkset;
    private Common ncpChildCpOutputRecord;
    private Common ncpOutputRecord;
    private Common ncpAddressInputRecord;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
  }
#endregion
}
