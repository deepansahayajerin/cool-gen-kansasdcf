// Program: LE_REFERRED_INTERST_CASE_REPORT, ID: 371125382, model: 746.
// Short name: SWELIN2B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_REFERRED_INTERST_CASE_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeReferredInterstCaseReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_REFERRED_INTERST_CASE_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeReferredInterstCaseReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeReferredInterstCaseReport.
  /// </summary>
  public LeReferredInterstCaseReport(IContext context, Import import,
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
    // **************************************************************************************************************
    // Date      Developer      Description
    // --------  -------------  
    // ---------------------------------------------------------------------
    // 10/05/01  GVandy         Developed as a one time job to report interstate
    // cases with active referrals.
    // **************************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
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
    // *Call External to Open the input File.         *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadInterstateCase2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening input file for 'le_eab_read_interstate_case'. Status = " +
        local.EabFileHandling.Status;
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ************************************************
    // *Call External to Open the output File.        *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.BlankLineAfterColHead = "Y";
    local.EabReportSend.RptHeading3 =
      "                Referrals for Cases in Office 21";
    local.EabReportSend.ProcessDate = Now().Date;
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening output file for 'cab_business_report_01'";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.FirstRecord.Flag = "Y";

    do
    {
      local.EabFileHandling.Action = "READ";
      UseLeEabReadInterstateCase1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.RecordsRead.Count;

          break;
        case "EF":
          local.EndOfFile.Flag = "Y";

          break;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading input file for 'le_eab_read_interstate_case'.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
      }

      if (Equal(local.ServiceProvider.FirstName,
        local.PreviousServiceProvider.FirstName) && Equal
        (local.ServiceProvider.LastName, local.PreviousServiceProvider.LastName) &&
        AsChar(local.ServiceProvider.MiddleInitial) == AsChar
        (local.PreviousServiceProvider.MiddleInitial))
      {
        ++local.NumbOfReferralsForSp.Count;

        if (Equal(local.CsePersonsWorkSet.FormattedName,
          local.PreviousCsePersonsWorkSet.FormattedName) && Equal
          (local.CsePersonsWorkSet.Number,
          local.PreviousCsePersonsWorkSet.Number) && Equal
          (local.Case1.Number, local.PreviousCase.Number))
        {
          ++local.NumbOfReferralsForCase.Count;

          continue;
        }

        if (!Equal(local.Case1.Number, local.PreviousCase.Number) && AsChar
          (local.EndOfFile.Flag) != 'Y')
        {
          ++local.NumberOfCasesForSp.Count;
        }
      }

      if (AsChar(local.FirstRecord.Flag) == 'Y')
      {
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.TextWorkArea.Text4 =
          NumberToString(local.NumbOfReferralsForCase.Count, 12, 4);
        local.TextWorkArea.Text4 =
          Substring(local.TextWorkArea.Text4, Verify(local.TextWorkArea.Text4,
          "0"), 5 - Verify(local.TextWorkArea.Text4, "0"));
        local.EabReportSend.RptDetail =
          local.PreviousCsePersonsWorkSet.FormattedName + "   " + local
          .PreviousCsePersonsWorkSet.Number + "   " + local
          .PreviousCase.Number + "   " + local.TextWorkArea.Text4;
        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        ++local.RecordsWritten.Count;
      }

      if (!Equal(local.ServiceProvider.FirstName,
        local.PreviousServiceProvider.FirstName) || !
        Equal(local.ServiceProvider.LastName,
        local.PreviousServiceProvider.LastName) || AsChar
        (local.ServiceProvider.MiddleInitial) != AsChar
        (local.PreviousServiceProvider.MiddleInitial))
      {
        if (AsChar(local.FirstRecord.Flag) == 'Y')
        {
          local.FirstRecord.Flag = "N";
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          local.EabFileHandling.Action = "WRITE";
          local.TextWorkArea.Text4 =
            NumberToString(local.NumberOfCasesForSp.Count, 12, 4);
          local.TextWorkArea.Text4 =
            Substring(local.TextWorkArea.Text4, Verify(local.TextWorkArea.Text4,
            "0"), 5 - Verify(local.TextWorkArea.Text4, "0"));
          local.EabReportSend.RptDetail = "Number of Interstate Cases : " + local
            .TextWorkArea.Text4;
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          local.EabFileHandling.Action = "WRITE";
          local.TextWorkArea.Text4 =
            NumberToString(local.NumbOfReferralsForSp.Count, 12, 4);
          local.TextWorkArea.Text4 =
            Substring(local.TextWorkArea.Text4, Verify(local.TextWorkArea.Text4,
            "0"), 5 - Verify(local.TextWorkArea.Text4, "0"));
          local.EabReportSend.RptDetail = "Number of Referrals : " + local
            .TextWorkArea.Text4;
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Page Break.
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }
        }

        if (AsChar(local.EndOfFile.Flag) == 'Y')
        {
          goto Test;
        }

        local.NumbOfReferralsForSp.Count = 1;
        local.NumberOfCasesForSp.Count = 1;
        local.EabFileHandling.Action = "WRITE";

        if (IsEmpty(local.ServiceProvider.LastName) && IsEmpty
          (local.ServiceProvider.FirstName) && IsEmpty
          (local.ServiceProvider.MiddleInitial))
        {
          local.EabReportSend.RptDetail = "Legal Service Provider:";
        }
        else if (Equal(local.ServiceProvider.LastName, "**Not Assigned**"))
        {
          local.EabReportSend.RptDetail =
            "Legal Service Provider: ** No Active Assignment **";
        }
        else
        {
          local.EabReportSend.RptDetail = "Legal Service Provider: " + TrimEnd
            (local.ServiceProvider.LastName) + ", " + TrimEnd
            (local.ServiceProvider.FirstName) + " " + local
            .ServiceProvider.MiddleInitial;
        }

        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "";
        local.EabReportSend.RptDetail =
          "                                                              # of Sent/Open";
          
        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "                                                 CSE Case     Referrals for";
          
        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "AP Name                             AP Number    Number       the CSE Case";
          
        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "---------------------------------   ----------   ----------   --------------";
          
        UseCabBusinessReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }
      }

Test:

      local.PreviousCase.Number = local.Case1.Number;
      MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
        local.PreviousCsePersonsWorkSet);
      local.PreviousServiceProvider.Assign(local.ServiceProvider);
      local.NumbOfReferralsForCase.Count = 1;
    }
    while(AsChar(local.EndOfFile.Flag) != 'Y');

    local.EabReportSend.RptDetail =
      "Total Number Of Interstate Case Referral Records Read     :  " + NumberToString
      (local.RecordsRead.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Interstate Case Referral Records Read).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Interstate Case Referral Records Written  :  " + NumberToString
      (local.RecordsWritten.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Interstate Case Referral Records Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // *Close the input file.                         *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadInterstateCase2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing input file for 'le_eab_read_interstate_case'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    // ************************************************
    // * Close the output file.                       *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file 'cab_business_report_01'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
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

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
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

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
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

  private void UseLeEabReadInterstateCase1()
  {
    var useImport = new LeEabReadInterstateCase.Import();
    var useExport = new LeEabReadInterstateCase.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.Case1.Number = local.Case1.Number;
    useExport.ServiceProvider.Assign(local.ServiceProvider);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadInterstateCase.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
    MoveServiceProvider(useExport.ServiceProvider, local.ServiceProvider);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabReadInterstateCase2()
  {
    var useImport = new LeEabReadInterstateCase.Import();
    var useExport = new LeEabReadInterstateCase.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadInterstateCase.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// A value of EndOfFile.
    /// </summary>
    [JsonPropertyName("endOfFile")]
    public Common EndOfFile
    {
      get => endOfFile ??= new();
      set => endOfFile = value;
    }

    /// <summary>
    /// A value of NumbOfReferralsForSp.
    /// </summary>
    [JsonPropertyName("numbOfReferralsForSp")]
    public Common NumbOfReferralsForSp
    {
      get => numbOfReferralsForSp ??= new();
      set => numbOfReferralsForSp = value;
    }

    /// <summary>
    /// A value of NumbOfReferralsForCase.
    /// </summary>
    [JsonPropertyName("numbOfReferralsForCase")]
    public Common NumbOfReferralsForCase
    {
      get => numbOfReferralsForCase ??= new();
      set => numbOfReferralsForCase = value;
    }

    /// <summary>
    /// A value of NumberOfCasesForSp.
    /// </summary>
    [JsonPropertyName("numberOfCasesForSp")]
    public Common NumberOfCasesForSp
    {
      get => numberOfCasesForSp ??= new();
      set => numberOfCasesForSp = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
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
    /// A value of PreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("previousCsePersonsWorkSet")]
    public CsePersonsWorkSet PreviousCsePersonsWorkSet
    {
      get => previousCsePersonsWorkSet ??= new();
      set => previousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PreviousOffice.
    /// </summary>
    [JsonPropertyName("previousOffice")]
    public Office PreviousOffice
    {
      get => previousOffice ??= new();
      set => previousOffice = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public EabReportSend Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of FirstRecord.
    /// </summary>
    [JsonPropertyName("firstRecord")]
    public Common FirstRecord
    {
      get => firstRecord ??= new();
      set => firstRecord = value;
    }

    /// <summary>
    /// A value of DateMonth.
    /// </summary>
    [JsonPropertyName("dateMonth")]
    public EabReportSend DateMonth
    {
      get => dateMonth ??= new();
      set => dateMonth = value;
    }

    /// <summary>
    /// A value of PreviousServiceProvider.
    /// </summary>
    [JsonPropertyName("previousServiceProvider")]
    public ServiceProvider PreviousServiceProvider
    {
      get => previousServiceProvider ??= new();
      set => previousServiceProvider = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of IwglCreatedDate.
    /// </summary>
    [JsonPropertyName("iwglCreatedDate")]
    public DateWorkArea IwglCreatedDate
    {
      get => iwglCreatedDate ??= new();
      set => iwglCreatedDate = value;
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
    /// A value of RecordsWritten.
    /// </summary>
    [JsonPropertyName("recordsWritten")]
    public Common RecordsWritten
    {
      get => recordsWritten ??= new();
      set => recordsWritten = value;
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

    private Common endOfFile;
    private Common numbOfReferralsForSp;
    private Common numbOfReferralsForCase;
    private Common numberOfCasesForSp;
    private TextWorkArea textWorkArea;
    private Case1 previousCase;
    private Case1 case1;
    private CsePersonsWorkSet previousCsePersonsWorkSet;
    private Office previousOffice;
    private EabReportSend header;
    private Common firstRecord;
    private EabReportSend dateMonth;
    private ServiceProvider previousServiceProvider;
    private Office office;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private Employer employer;
    private EmployerAddress employerAddress;
    private ServiceProvider serviceProvider;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea iwglCreatedDate;
    private DateWorkArea end;
    private DateWorkArea start;
    private Common recordsWritten;
    private Common recordsRead;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fileOpened;
  }
#endregion
}
