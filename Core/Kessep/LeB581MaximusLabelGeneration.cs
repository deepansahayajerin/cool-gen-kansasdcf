// Program: LE_B581_MAXIMUS_LABEL_GENERATION, ID: 373548616, model: 746.
// Short name: SWEL581B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B581_MAXIMUS_LABEL_GENERATION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB581MaximusLabelGeneration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B581_MAXIMUS_LABEL_GENERATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB581MaximusLabelGeneration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB581MaximusLabelGeneration.
  /// </summary>
  public LeB581MaximusLabelGeneration(IContext context, Import import,
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
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *** Get DB2 commit frequency counts
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *** No Restart Logic is Included
    local.Total.Count = 0;
    local.ArNotFound.Count = 0;
    local.ApNotFound.Count = 0;
    local.MultipleAps.Count = 0;
    local.InvalidAddress.Count = 0;
    local.AddressNotFound.Count = 0;
    local.Error.Count = 0;

    // * * * * * * * * * *
    // Open CONTROL REPORT
    // * * * * * * * * * *
    local.ReportEabReportSend.ProcessDate = local.Current.Date;
    local.ReportEabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.ReportEabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * *
    // Open ERROR REPORT
    // * * * * * * * * *
    UseCabErrorReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *** Set the EAB Handling Action to WRITE
    local.ReportEabFileHandling.Action = "WRITE";

    foreach(var item in ReadCaseOfficeServiceProvider())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      ++local.Total.Count;
      export.Case1.Number = entities.Case1.Number;
      export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
      export.ServiceProvider.SystemGeneratedId =
        entities.ServiceProvider.SystemGeneratedId;
      export.Ap.Number = "";
      export.Ap.FormattedName = "";
      export.Ar.Number = "";
      export.Ar.FormattedName = "";

      // *** Read the AR
      if (ReadCsePerson2())
      {
        export.Ar.Number = entities.CsePerson.Number;
      }

      if (IsEmpty(export.Ar.Number))
      {
        ++local.ArNotFound.Count;
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No AR exists for the Case";
          
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          // *** Continue
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
      else
      {
        // *** Read the AR Name from ADABAS
        UseSiReadCsePersonBatch1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Continue Processing
        }
        else
        {
          local.ReportEabReportSend.RptDetail = export.Case1.Number + ": ADABAS Error Code -->" +
            TrimEnd(local.AbendData.AdabasResponseCd);
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            ++local.Error.Count;

            continue;

            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      // *** Read the AP
      if (ReadCsePerson1())
      {
        export.Ap.Number = entities.CsePerson.Number;
      }

      if (IsEmpty(export.Ap.Number))
      {
        ++local.ApNotFound.Count;
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No AP exists for the Case";
          
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          continue;
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      local.Ap.Count = 0;

      foreach(var item1 in ReadCsePerson3())
      {
        ++local.Ap.Count;

        if (local.Ap.Count > 1)
        {
          continue;
        }

        local.CsePerson.Number = entities.CsePerson.Number;
        export.Ap.Number = entities.CsePerson.Number;
      }

      // *** Read the AP Name from ADABAS
      UseSiReadCsePersonBatch2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // *** Continue Processing
      }
      else
      {
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": ADABAS Error Code -->" +
          TrimEnd(local.AbendData.AdabasResponseCd);
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          ++local.Error.Count;

          continue;

          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (local.Ap.Count == 2)
      {
        ++local.MultipleAps.Count;
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": Multiple APs exist for the Case";
          
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      UseSiGetCsePersonMailingAddr();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.AddressNotFound.Count;
        local.ReportEabReportSend.RptDetail = export.Case1.Number + ": No Valid Address found for the AP:" +
          local.CsePerson.Number;
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          continue;
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      local.ReportEabReportSend.RptDetail =
        NumberToString(export.Office.SystemGeneratedId, 12, 4);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + NumberToString
        (export.ServiceProvider.SystemGeneratedId, 8, 8);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + export.Case1.Number;
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
        (export.Ap.Number);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
        (export.Ap.FormattedName);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.ReportEabReportSend.RptDetail =
        NumberToString(export.Office.SystemGeneratedId, 12, 4);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + NumberToString
        (export.ServiceProvider.SystemGeneratedId, 8, 8);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + export.Case1.Number;
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
        (export.Ar.Number);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + TrimEnd
        (export.Ar.FormattedName);
      local.ReportEabReportSend.RptDetail =
        TrimEnd(local.ReportEabReportSend.RptDetail) + "";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // * * ** * * * * * * * * * * * * *
    // Write the CONTROL REPORT TOTALs
    // * * ** * * * * * * * * * * * * *
    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases read                                 = " + NumberToString
      (local.Total.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases with no APs                          = " + NumberToString
      (local.ApNotFound.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases with no ARs                          = " + NumberToString
      (local.ArNotFound.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of Cases with Multiple APs                    = " + NumberToString
      (local.MultipleAps.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of APs with no Address found                  = " + NumberToString
      (local.AddressNotFound.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of APs with Invalid Address Type              = " + NumberToString
      (local.InvalidAddress.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of Records in Error                           = " + NumberToString
      (local.Error.Count, 15);
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * * *
    // Close Control Report
    // * * * * * * * * * * *
    local.ReportEabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // * * * * * * * * * *
    // Close Error Report
    // * * * * * * * * * *
    UseCabErrorReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------
    // Set restart indicator to no - successfully finished program
    // -------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart();
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.EndDate = source.EndDate;
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
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ap);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
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
    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private CsePersonsWorkSet ar;
    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private CsePersonsWorkSet ap;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ArNotFound.
    /// </summary>
    [JsonPropertyName("arNotFound")]
    public Common ArNotFound
    {
      get => arNotFound ??= new();
      set => arNotFound = value;
    }

    /// <summary>
    /// A value of AddressNotFound.
    /// </summary>
    [JsonPropertyName("addressNotFound")]
    public Common AddressNotFound
    {
      get => addressNotFound ??= new();
      set => addressNotFound = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of InvalidAddress.
    /// </summary>
    [JsonPropertyName("invalidAddress")]
    public Common InvalidAddress
    {
      get => invalidAddress ??= new();
      set => invalidAddress = value;
    }

    /// <summary>
    /// A value of ApNotFound.
    /// </summary>
    [JsonPropertyName("apNotFound")]
    public Common ApNotFound
    {
      get => apNotFound ??= new();
      set => apNotFound = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of CheckpointRestartKey.
    /// </summary>
    [JsonPropertyName("checkpointRestartKey")]
    public Infrastructure CheckpointRestartKey
    {
      get => checkpointRestartKey ??= new();
      set => checkpointRestartKey = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of ReportEabReportSend.
    /// </summary>
    [JsonPropertyName("reportEabReportSend")]
    public EabReportSend ReportEabReportSend
    {
      get => reportEabReportSend ??= new();
      set => reportEabReportSend = value;
    }

    /// <summary>
    /// A value of ReportEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportEabFileHandling")]
    public EabFileHandling ReportEabFileHandling
    {
      get => reportEabFileHandling ??= new();
      set => reportEabFileHandling = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private Common arNotFound;
    private Common addressNotFound;
    private CsePerson csePerson;
    private Common ap;
    private CsePersonAddress csePersonAddress;
    private Common invalidAddress;
    private Common apNotFound;
    private Common multipleAps;
    private AbendData abendData;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure checkpointRestartKey;
    private Common total;
    private EabReportSend reportEabReportSend;
    private EabFileHandling reportEabFileHandling;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private ServiceProvider serviceProvider;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
