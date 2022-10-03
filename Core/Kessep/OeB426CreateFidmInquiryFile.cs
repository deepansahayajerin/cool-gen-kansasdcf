// Program: OE_B426_CREATE_FIDM_INQUIRY_FILE, ID: 374401575, model: 746.
// Short name: SWEE426B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B426_CREATE_FIDM_INQUIRY_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB426CreateFidmInquiryFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B426_CREATE_FIDM_INQUIRY_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB426CreateFidmInquiryFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB426CreateFidmInquiryFile.
  /// </summary>
  public OeB426CreateFidmInquiryFile(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB426";
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
    // *Call External to Open the Flat File.          *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabWriteFidmInquiryFile1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'oe_eab_write_fidm_inquiry_file'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.FileOpened.Flag = "Y";
    local.HeaderRecord.Flag = "Y";

    foreach(var item in ReadAdministrativeActCertification())
    {
      if (AsChar(entities.AdministrativeActCertification.
        EtypeFinancialInstitution) == 'Y')
      {
        local.AdministrativeActCertification.CaseNumber =
          entities.AdministrativeActCertification.CaseNumber;

        continue;
      }

      if (Equal(entities.AdministrativeActCertification.CaseNumber,
        local.AdministrativeActCertification.CaseNumber))
      {
        continue;
      }

      if (Lt(0, entities.AdministrativeActCertification.CurrentAmount))
      {
        if (AsChar(local.HeaderRecord.Flag) == 'Y')
        {
          local.PassArea.FileInstruction = "WRITEH";
          export.ExternalFidmHeader.RecordType = "D";
          export.ExternalFidmHeader.DataMatchFi = "M";
          export.ExternalFidmHeader.Yyyymm =
            NumberToString(DateToInt(Now().Date), 8, 6);
          UseOeEabWriteFidmInquiryFile3();

          if (!IsEmpty(local.PassArea.TextReturnCode))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error in writing header record in external file  for 'oe_eab_write_fidm_inquiry_file'.";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
          else
          {
            local.HeaderRecord.Flag = "N";
            export.EabFileHandling.Action = "OPEN";
            export.NeededToOpen.RptHeading3 = "";
            export.NeededToOpen.NumberOfColHeadings = 3;
            export.NeededToOpen.ColHeading1 = "File Generation Date : " + export
              .ExternalFidmHeader.Yyyymm;
            export.NeededToOpen.ColHeading2 = "";
            export.NeededToOpen.ColHeading3 =
              "Social Security Number     Case Pass-Back Information     Inquiry Last Name        Inquiry First Name";
              
            export.NeededToOpen.BlankLineAfterHeading = "Y";
            export.NeededToOpen.BlankLineAfterColHead = "Y";
            export.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
            UseCabBusinessReport1();

            if (!Equal(export.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }

        local.PassArea.FileInstruction = "WRITED";
        export.ExternalFidmDetail.RecordType = "I";
        export.ExternalFidmDetail.Ssn =
          NumberToString(entities.AdministrativeActCertification.Ssn, 9);
        export.ExternalFidmDetail.LastName =
          entities.AdministrativeActCertification.LastName;
        export.ExternalFidmDetail.FirstName =
          entities.AdministrativeActCertification.FirstName;
        export.ExternalFidmDetail.CsePersonNo =
          entities.AdministrativeActCertification.CaseNumber;
        export.ExternalFidmDetail.Fips = "00020";
        UseOeEabWriteFidmInquiryFile4();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in writing detail record in external file  for 'oe_eab_write_fidm_inquiry_file'.";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
        else
        {
          ++export.ExternalFidmTrailer.TotalNoInquiryRec;
          local.AdministrativeActCertification.CurrentAmount =
            local.AdministrativeActCertification.CurrentAmount.
              GetValueOrDefault() + entities
            .AdministrativeActCertification.CurrentAmount.GetValueOrDefault();
          ++local.Record.Count;
          local.AdministrativeActCertification.CaseNumber =
            entities.AdministrativeActCertification.CaseNumber;
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            NumberToString(entities.AdministrativeActCertification.Ssn, 7, 9) +
            "                  " + entities
            .AdministrativeActCertification.CaseNumber + "                " + entities
            .AdministrativeActCertification.LastName + "     " + entities
            .AdministrativeActCertification.FirstName;
          UseCabBusinessReport2();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
      else
      {
        local.AdministrativeActCertification.CaseNumber =
          entities.AdministrativeActCertification.CaseNumber;

        continue;
      }
    }

    if (local.Record.Count >= 1)
    {
      local.PassArea.FileInstruction = "WRITET";
      export.ExternalFidmTrailer.RecordType = "T";
      UseOeEabWriteFidmInquiryFile2();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in writing trailer record in external file  for 'oe_eab_write_fidm_inquiry_file'.";
          
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      export.NeededToWrite.RptDetail =
        "Total Number of FIDM Inquiry Records  :  " + NumberToString
        (export.ExternalFidmTrailer.TotalNoInquiryRec, 15);
      UseCabBusinessReport2();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail =
      "Total Number Of FIDM Records Written  :  " + NumberToString
      (export.ExternalFidmTrailer.TotalNoInquiryRec, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of FIDM Records Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Dollar Amount of FIDM Records Written  :  " + NumberToString
      ((long)local.AdministrativeActCertification.CurrentAmount.
        GetValueOrDefault(), 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of FIDM Records Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************
      // *Close External FIDM File         *
      // ******************************************
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabWriteFidmInquiryFile1();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in closing external file  for 'oe_eab_write_fidm_inquiry_file'.";
          
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_CLOSE_ERROR";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

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

      export.EabFileHandling.Action = "CLOSE";
      UseCabBusinessReport3();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveExternalFidmTrailer(ExternalFidmTrailer source,
    ExternalFidmTrailer target)
  {
    target.RecordType = source.RecordType;
    target.TotalNoInquiryRec = source.TotalNoInquiryRec;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToOpen.Assign(export.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseOeEabWriteFidmInquiryFile1()
  {
    var useImport = new OeEabWriteFidmInquiryFile.Import();
    var useExport = new OeEabWriteFidmInquiryFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteFidmInquiryFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseOeEabWriteFidmInquiryFile2()
  {
    var useImport = new OeEabWriteFidmInquiryFile.Import();
    var useExport = new OeEabWriteFidmInquiryFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveExternalFidmTrailer(export.ExternalFidmTrailer,
      useImport.ExternalFidmTrailer);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteFidmInquiryFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseOeEabWriteFidmInquiryFile3()
  {
    var useImport = new OeEabWriteFidmInquiryFile.Import();
    var useExport = new OeEabWriteFidmInquiryFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.ExternalFidmHeader.Assign(export.ExternalFidmHeader);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteFidmInquiryFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseOeEabWriteFidmInquiryFile4()
  {
    var useImport = new OeEabWriteFidmInquiryFile.Import();
    var useExport = new OeEabWriteFidmInquiryFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.ExternalFidmDetail.Assign(export.ExternalFidmDetail);
    useExport.External.Assign(local.PassArea);

    Call(OeEabWriteFidmInquiryFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadAdministrativeActCertification()
  {
    entities.AdministrativeActCertification.Populated = false;

    return ReadEach("ReadAdministrativeActCertification",
      null,
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.NotificationDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeActCertification.CreatedBy =
          db.GetString(reader, 9);
        entities.AdministrativeActCertification.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.AdministrativeActCertification.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.AdministrativeActCertification.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.AdministrativeActCertification.AdcAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AdministrativeActCertification.NonAdcAmount =
          db.GetNullableDecimal(reader, 14);
        entities.AdministrativeActCertification.InjuredSpouseDate =
          db.GetNullableDate(reader, 15);
        entities.AdministrativeActCertification.NotifiedBy =
          db.GetNullableString(reader, 16);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 17);
        entities.AdministrativeActCertification.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 18);
        entities.AdministrativeActCertification.LocalCode =
          db.GetNullableString(reader, 19);
        entities.AdministrativeActCertification.Ssn = db.GetInt32(reader, 20);
        entities.AdministrativeActCertification.CaseNumber =
          db.GetString(reader, 21);
        entities.AdministrativeActCertification.LastName =
          db.GetString(reader, 22);
        entities.AdministrativeActCertification.FirstName =
          db.GetString(reader, 23);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 24);
        entities.AdministrativeActCertification.TtypeAAddNewCase =
          db.GetNullableString(reader, 25);
        entities.AdministrativeActCertification.CaseType =
          db.GetString(reader, 26);
        entities.AdministrativeActCertification.TransferState =
          db.GetNullableString(reader, 27);
        entities.AdministrativeActCertification.LocalForTransfer =
          db.GetNullableInt32(reader, 28);
        entities.AdministrativeActCertification.ProcessYear =
          db.GetNullableInt32(reader, 29);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 30);
        entities.AdministrativeActCertification.TtypeDDeleteCertification =
          db.GetNullableString(reader, 31);
        entities.AdministrativeActCertification.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 32);
        entities.AdministrativeActCertification.TtypeMModifyAmount =
          db.GetNullableString(reader, 33);
        entities.AdministrativeActCertification.TtypeRModifyExclusion =
          db.GetNullableString(reader, 34);
        entities.AdministrativeActCertification.TtypeSStatePayment =
          db.GetNullableString(reader, 35);
        entities.AdministrativeActCertification.TtypeTTransferAdminReview =
          db.GetNullableString(reader, 36);
        entities.AdministrativeActCertification.EtypeFederalRetirement =
          db.GetNullableString(reader, 37);
        entities.AdministrativeActCertification.EtypeFederalSalary =
          db.GetNullableString(reader, 38);
        entities.AdministrativeActCertification.EtypeTaxRefund =
          db.GetNullableString(reader, 39);
        entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 40);
        entities.AdministrativeActCertification.EtypePassportDenial =
          db.GetNullableString(reader, 41);
        entities.AdministrativeActCertification.EtypeFinancialInstitution =
          db.GetNullableString(reader, 42);
        entities.AdministrativeActCertification.ReturnStatus =
          db.GetNullableString(reader, 43);
        entities.AdministrativeActCertification.ReturnStatusDate =
          db.GetNullableDate(reader, 44);
        entities.AdministrativeActCertification.DecertificationReason =
          db.GetNullableString(reader, 45);
        entities.AdministrativeActCertification.Populated = true;

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
    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ExternalFidmTrailer.
    /// </summary>
    [JsonPropertyName("externalFidmTrailer")]
    public ExternalFidmTrailer ExternalFidmTrailer
    {
      get => externalFidmTrailer ??= new();
      set => externalFidmTrailer = value;
    }

    /// <summary>
    /// A value of ExternalFidmHeader.
    /// </summary>
    [JsonPropertyName("externalFidmHeader")]
    public ExternalFidmHeader ExternalFidmHeader
    {
      get => externalFidmHeader ??= new();
      set => externalFidmHeader = value;
    }

    /// <summary>
    /// A value of ExternalFidmDetail.
    /// </summary>
    [JsonPropertyName("externalFidmDetail")]
    public ExternalFidmDetail ExternalFidmDetail
    {
      get => externalFidmDetail ??= new();
      set => externalFidmDetail = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ExternalFidmTrailer externalFidmTrailer;
    private ExternalFidmHeader externalFidmHeader;
    private ExternalFidmDetail externalFidmDetail;
    private ProgramProcessingInfo programProcessingInfo;
    private External external;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotalFidmErrorCreated.
    /// </summary>
    [JsonPropertyName("totalFidmErrorCreated")]
    public Common TotalFidmErrorCreated
    {
      get => totalFidmErrorCreated ??= new();
      set => totalFidmErrorCreated = value;
    }

    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of TotalDollarAmount.
    /// </summary>
    [JsonPropertyName("totalDollarAmount")]
    public Common TotalDollarAmount
    {
      get => totalDollarAmount ??= new();
      set => totalDollarAmount = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public Common HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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

    private Common totalFidmErrorCreated;
    private Common record;
    private AdministrativeActCertification administrativeActCertification;
    private Common totalDollarAmount;
    private Common headerRecord;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Common fileOpened;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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

    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private AdministrativeActCertification administrativeActCertification;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
