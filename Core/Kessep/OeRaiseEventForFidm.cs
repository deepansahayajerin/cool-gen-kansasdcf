// Program: OE_RAISE_EVENT_FOR_FIDM, ID: 374390130, model: 746.
// Short name: SWE02607
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_RAISE_EVENT_FOR_FIDM.
/// </para>
/// <para>
/// Create the Infostructure records for the FIDM Files.
/// </para>
/// </summary>
[Serializable]
public partial class OeRaiseEventForFidm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RAISE_EVENT_FOR_FIDM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeRaiseEventForFidm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeRaiseEventForFidm.
  /// </summary>
  public OeRaiseEventForFidm(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *********************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  ------------------------
    // ??/??/??  ???????????????                    Initial Development
    // 03/03/03  George Vandy            PR171880   Raise new event detail for 
    // in-state FIDM.
    // *********************************************************************
    local.Current.Date = Now().Date;

    // * * * * * * * * * * * * * * * *
    // * * Read EACH of the FIDM Table records that WE Created durring this run
    // * * * * * * * * * * * * * * * *
    local.Save.CreatedBy = global.UserId;

    foreach(var item in ReadFinancialInstitutionDataMatch())
    {
      // * * * * * * * * * * * * * * * *
      // * * Only PROCESS ONE Table record for each Person Number
      // * * * * * * * * * * * * * * * *
      if (Equal(entities.FinancialInstitutionDataMatch.CsePersonNumber,
        local.Save.CsePersonNumber))
      {
        continue;
      }

      // * * * * * * * * * * * * * * * *
      // * * How many Open cases have this person as the "AP"
      // * * This will determine the detail message used below
      // * * * * * * * * * * * * * * * *
      ReadServiceProvider();

      // * * * * * * * * * * * * * * * *
      // * * Set up the information to create the INFRASTRUCTURE Record
      // * * * * * * * * * * * * * * * *
      // local_pass infrastructure  function   ----    SET in CAB
      // local_pass infrastructure  system_generated_identifier   ----    SET in
      // CAB
      local.Pass.SituationNumber = 0;
      local.Pass.ProcessStatus = "Q";
      local.Pass.EventId = 10;

      // local_pass infrastructure  event_type   ----    SET in CAB
      // local_pass infrastructure  event_detail_name   ----    SET in CAB
      if (Equal(global.UserId, "SWEEB425"))
      {
        // -- Event detail for MSFIDM.
        local.Pass.ReasonCode = "DATAMATCH";
      }
      else
      {
        // -- Event detail for in-state FIDM.
        local.Pass.ReasonCode = "FIDMDATAMATCH";
      }

      local.Pass.BusinessObjectCd = "CAU";

      // denorm_numeric_12
      local.Pass.DenormText12 = "";

      // denorm_date
      // denorm_timestamp
      local.Pass.InitiatingStateCode = "KS";
      local.Pass.CsenetInOutCode = "";

      // CASE_NUMBER   SET BELOW IN READ_EACH
      local.Pass.CsePersonNumber =
        entities.FinancialInstitutionDataMatch.CsePersonNumber;

      // CASE_UNIT_NUMBER   SET BELOW IN READ_EACH
      local.Pass.UserId = global.UserId;
      local.Pass.LastUpdatedBy = "";
      local.Pass.LastUpdatedBy = "";

      // last_updated_timestamp
      // reference_date
      if (local.ServiceProvider.Count > 1)
      {
        local.Pass.Detail =
          "FINANCIAL INSTITUTION DATA MATCH WAS RECEIVED, M-OSP.";
      }
      else
      {
        local.Pass.Detail = "FINANCIAL INSTITUTION DATA MATCH WAS RECEIVED.";
      }

      // * * * * * * * * * * * * * * * *
      // * * CREATE an INFRASTRUCTURE record for EACH Open Case the person is an
      // "AP" on
      // * * * * * * * * * * * * * * * *
      // * * Added SORTED BY to Prevent Duplicate Infrastructure Records
      // * *   IF Multiple case_unit's
      // * * * * * * * * * * * * * * * *
      ExitState = "ACO_NN0000_ALL_OK";
      local.Existing.Number = "";

      foreach(var item1 in ReadCaseCaseUnit())
      {
        if (Equal(entities.ExistingCase.Number, local.Existing.Number))
        {
          continue;
        }

        local.Pass.CaseNumber = entities.ExistingCase.Number;
        UseSpCabCreateInfrastructure();

        // * * * * * * * * * * * * * * * *
        // * * If the CREATE was NOT succesful - ABORT the job
        // * * * * * * * * * * * * * * * *
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        local.Existing.Number = entities.ExistingCase.Number;
      }

      // * * * * * * * * * * * * * * * *
      // * * Save the person number to compare to the NEXT Table record
      // * * * * * * * * * * * * * * * *
      local.Save.CsePersonNumber =
        entities.FinancialInstitutionDataMatch.CsePersonNumber;
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Pass);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.ExistingCaseUnit.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp",
          entities.FinancialInstitutionDataMatch.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.ExistingCaseUnit.Populated = true;
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFinancialInstitutionDataMatch()
  {
    entities.FinancialInstitutionDataMatch.Populated = false;

    return ReadEach("ReadFinancialInstitutionDataMatch",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", import.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.Ending.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "createdBy", local.Save.CreatedBy ?? "");
      },
      (db, reader) =>
      {
        entities.FinancialInstitutionDataMatch.CsePersonNumber =
          db.GetString(reader, 0);
        entities.FinancialInstitutionDataMatch.InstitutionTin =
          db.GetString(reader, 1);
        entities.FinancialInstitutionDataMatch.MatchedPayeeAccountNumber =
          db.GetString(reader, 2);
        entities.FinancialInstitutionDataMatch.MatchRunDate =
          db.GetString(reader, 3);
        entities.FinancialInstitutionDataMatch.AccountType =
          db.GetString(reader, 4);
        entities.FinancialInstitutionDataMatch.CreatedBy =
          db.GetNullableString(reader, 5);
        entities.FinancialInstitutionDataMatch.CreatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.FinancialInstitutionDataMatch.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.FinancialInstitutionDataMatch.CsePersonNumber);
      },
      (db, reader) =>
      {
        local.ServiceProvider.Count = db.GetInt32(reader, 0);
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public DateWorkArea Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    private DateWorkArea start;
    private DateWorkArea ending;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public FinancialInstitutionDataMatch Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Infrastructure Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Common ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private Case1 existing;
    private FinancialInstitutionDataMatch save;
    private Infrastructure pass;
    private DateWorkArea current;
    private Common serviceProvider;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("financialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch FinancialInstitutionDataMatch
    {
      get => financialInstitutionDataMatch ??= new();
      set => financialInstitutionDataMatch = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    private FinancialInstitutionDataMatch financialInstitutionDataMatch;
    private CaseUnit existingCaseUnit;
    private CaseRole existingAbsentParent;
    private CsePerson existingObligor;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
  }
#endregion
}
