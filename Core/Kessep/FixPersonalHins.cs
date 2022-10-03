// Program: FIX_PERSONAL_HINS, ID: 372888543, model: 746.
// Short name: SWFIXITB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FIX_PERSONAL_HINS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FixPersonalHins: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FIX_PERSONAL_HINS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FixPersonalHins(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FixPersonalHins.
  /// </summary>
  public FixPersonalHins(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *** Problem Report # H00072740
    // *** C Fairley 09/16/99
    // ***
    // *** update the Coverage End Date on Personal Health Insurance,
    // *** when it is equal to '2099-12-31',
    // *** with the Policy Expiration Date from Health Insurance Coverage,
    // *** when it is not equal to '2099-12-31'
    // ***
    ExitState = "ACO_NN0000_ALL_OK";
    local.DateWorkArea.Date = new DateTime(2099, 12, 31);

    // ***
    // *** OPEN the Error Report
    // ***
    export.EabFileHandling.Action = "OPEN";
    export.NeededToOpen.ProcessDate = Now().Date;
    export.NeededToOpen.ProgramName = "SWFIXINS";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    foreach(var item in ReadHealthInsuranceCoverage())
    {
      foreach(var item1 in ReadPersonalHealthInsurance())
      {
        try
        {
          UpdatePersonalHealthInsurance();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // ***
              // *** WRITE to the Error Report
              // ***
              export.EabFileHandling.Action = "WRITE";
              export.NeededToWrite.RptDetail =
                "NOT UNIQUE when updating Personal Health Insurance for Health Insurance Coverage with Identifier " +
                NumberToString
                (entities.HealthInsuranceCoverage.Identifier, 15);
              UseCabErrorReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ERROR_WRITING_TO_REPORT_AB";

                return;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              // ***
              // *** WRITE to the Error Report
              // ***
              export.EabFileHandling.Action = "WRITE";
              export.NeededToWrite.RptDetail =
                "PERMITTED VALUE VIOLATION when updating Personal Health Insurance for Health Insurance Coverage with Identifier " +
                NumberToString
                (entities.HealthInsuranceCoverage.Identifier, 15);
              UseCabErrorReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ERROR_WRITING_TO_REPORT_AB";

                return;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // ***
    // *** CLOSE the Error Report
    // ***
    export.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(export.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "policyExpDate",
          local.DateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return ReadEach("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
        db.SetNullableDate(
          command, "coverEndDate", local.DateWorkArea.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.Populated = true;

        return true;
      });
  }

  private void UpdatePersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.PersonalHealthInsurance.Populated);

    var coverageEndDate = entities.HealthInsuranceCoverage.PolicyExpirationDate;

    entities.PersonalHealthInsurance.Populated = false;
    Update("UpdatePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetNullableDate(command, "coverEndDate", coverageEndDate);
        db.SetInt64(command, "hcvId", entities.PersonalHealthInsurance.HcvId);
        db.SetString(
          command, "cspNumber", entities.PersonalHealthInsurance.CspNumber);
      });

    entities.PersonalHealthInsurance.CoverageEndDate = coverageEndDate;
    entities.PersonalHealthInsurance.Populated = true;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private EabFileHandling eabFileHandling;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
