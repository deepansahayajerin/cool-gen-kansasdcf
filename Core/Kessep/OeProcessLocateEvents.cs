// Program: OE_PROCESS_LOCATE_EVENTS, ID: 374436759, model: 746.
// Short name: SWE02611
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
/// A program: OE_PROCESS_LOCATE_EVENTS.
/// </para>
/// <para>
/// Create the Infostructure records for the FIDM Files.
/// </para>
/// </summary>
[Serializable]
public partial class OeProcessLocateEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PROCESS_LOCATE_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeProcessLocateEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeProcessLocateEvents.
  /// </summary>
  public OeProcessLocateEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------
    // 06/27/00	SWSRPDP
    // Initial Developement
    // CHANGE LOG:
    // 03/2001	SWSRPRM
    // WR # 291 => License Suspension; fixed code dealing with
    // multiple sources so that the sub agency would be included
    // on history record.
    // ----------------------------------------------------------
    // 12/30/2008 Arun Mathias CQ#7432 Populate the correct case number in the 
    // infrastructure table.
    // ---------------------------------------------------------------
    local.Current.Date = Now().Date;

    // * * * * * * * * * * * * * * * ** * * * * * * *
    // Read EACH of the LOCATE RESPONSE Table records
    // created durring this run
    // * * * * * * * * * * * * * * * ** * * * * * * *
    local.Save.CreatedBy = global.UserId;
    local.Current.Date = Now().Date;

    local.ServiceProviderTable.Index = 0;
    local.ServiceProviderTable.Clear();

    do
    {
      if (local.ServiceProviderTable.IsFull)
      {
        break;
      }

      local.ServiceProviderTable.Update.TableSaved.SystemGeneratedId = 0;
      local.ServiceProviderTable.Next();
    }
    while(!local.ServiceProviderTable.IsFull);

    foreach(var item in ReadLocateRequest())
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * * Only PROCESS ONE Table record for each Person Number/Agency
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      if (Equal(entities.LocateRequest.CsePersonNumber,
        local.Save.CsePersonNumber) && AsChar
        (entities.LocateRequest.LicenseSuspensionInd) == 'N')
      {
        continue;
      }

      local.Agency.Cdvalue =
        Substring(entities.LocateRequest.AgencyNumber, 2, 4);

      if (ReadCodeCodeValue2())
      {
        // -------------------------------------------------------------
        // A check must be made here to determine if the agency has
        // mulitple sources w/in it so that the HIST record will contain
        // all pertinent information
        // -------------------------------------------------------------
        local.Sequence.Cdvalue =
          NumberToString(entities.LocateRequest.SequenceNumber, 10);
        local.Sequence.Cdvalue = Substring(local.Sequence.Cdvalue, 9, 2);

        if (ReadCodeCodeValue1())
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          // ----------------------------------------
          // Not an error - Agency has no subagencies
          // ----------------------------------------
        }
      }
      else
      {
        ExitState = "CODE_VALUE_NF";

        return;
      }

      if (AsChar(import.ProcessAllLocateRequst.Flag) == 'Y')
      {
        if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'N')
        {
          local.Pass.Detail = "LOCATE REQUEST RESPONSE - " + entities
            .CodeValue.Description;
          local.Pass.ReasonCode = "LOCATERECD";
        }
        else
        {
          local.Pass.ReasonCode = "LICSUSPRECD";
          local.Pass.Detail = entities.CodeValue.Description + "; License Number is " +
            entities.LocateRequest.LicenseNumber;
        }
      }
      else
      {
        local.Pass.ReasonCode = "LICSUSPRECD";
        local.Pass.Detail = entities.CodeValue.Description + "; License Number is " +
          entities.LocateRequest.LicenseNumber;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * * CREATE an INFRASTRUCTURE record
      // * * for EACH Service Provider on each Open Case the person is an "AP" 
      // on
      // * * * EACH Service Provider should Recieve only ONE Alert per Run(Day)
      // * * * * * * * * * * * * * * * *
      // * * Added SORTED BY to Prevent Duplicate Infrastructure Records
      // * *   IF Multiple case_unit's
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      ExitState = "ACO_NN0000_ALL_OK";
      local.Existing.Number = "";

      foreach(var item1 in ReadCaseCaseUnit())
      {
        if (Equal(entities.ExistingCase.Number, local.Existing.Number))
        {
          continue;
        }

        if (!ReadCaseAssignmentOfficeServiceProviderServiceProvider())
        {
          continue;
        }

        if (AsChar(entities.LocateRequest.LicenseSuspensionInd) == 'Y')
        {
          // -----------------------------------------------------
          // Only HIST record per Service Provider for each unique
          // Agency / Sequency combinsation
          // -----------------------------------------------------
          if (Equal(entities.LocateRequest.CsePersonNumber,
            local.Previous.CsePersonNumber) && Equal
            (entities.LocateRequest.AgencyNumber, local.Previous.AgencyNumber) &&
            entities.LocateRequest.SequenceNumber == local
            .Previous.SequenceNumber)
          {
            continue;
          }
          else
          {
            local.Previous.Assign(entities.LocateRequest);
          }
        }
        else
        {
          // -----------------------------------
          // Only ONE Alert per Service Provider
          // MAX 5000 Service Providers
          // -----------------------------------
          for(local.ServiceProviderTable.Index = 0; local
            .ServiceProviderTable.Index < local.ServiceProviderTable.Count; ++
            local.ServiceProviderTable.Index)
          {
            if (local.ServiceProviderTable.Item.TableSaved.SystemGeneratedId ==
              entities.ServiceProvider.SystemGeneratedId)
            {
              goto ReadEach;
            }

            if (local.ServiceProviderTable.Item.TableSaved.SystemGeneratedId ==
              0)
            {
              local.ServiceProviderTable.Update.TableSaved.SystemGeneratedId =
                entities.ServiceProvider.SystemGeneratedId;

              break;
            }
          }
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * Set up the information to create the INFRASTRUCTURE Record
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // *** CQ7432 Changes Start Here ***
        // *** Always set the existing case number when creating infrastructure 
        // record
        local.Pass.CaseNumber = entities.ExistingCase.Number;

        // *** CQ7432 Changes End   Here ***
        // local_pass infrastructure  function   ----    SET in CAB
        // local_pass infrastructure  system_generated_identifier   ----    SET 
        // in CAB
        local.Pass.SituationNumber = 0;
        local.Pass.ProcessStatus = "Q";
        local.Pass.EventId = 10;

        // local_pass infrastructure  event_type   ----    SET in CAB
        // local_pass infrastructure  event_detail_name   ----    SET in CAB
        local.Pass.BusinessObjectCd = "CAS";

        // denorm_numeric_12
        local.Pass.DenormText12 = "";

        // denorm_date
        // denorm_timestamp
        local.Pass.InitiatingStateCode = "KS";
        local.Pass.CsenetInOutCode = "";

        // CASE_NUMBER =>  SET ABOVE
        local.Pass.CsePersonNumber = entities.LocateRequest.CsePersonNumber;

        // CASE_UNIT_NUMBER => SET ABOVE
        local.Pass.UserId = global.UserId;
        local.Pass.LastUpdatedBy = "";
        local.Pass.CreatedBy = global.UserId;

        // last_updated_timestamp
        // reference_date
        local.Pass.CaseUnitNumber = entities.ExistingCaseUnit.CuNumber;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

          return;
        }

        local.Existing.Number = entities.ExistingCase.Number;

ReadEach:
        ;
      }

      local.Save.CsePersonNumber = entities.LocateRequest.CsePersonNumber;
      local.Save.AgencyNumber = entities.LocateRequest.AgencyNumber;
    }
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Pass);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private bool ReadCaseAssignmentOfficeServiceProviderServiceProvider()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.ExistingCase.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.ExistingCaseUnit.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", entities.LocateRequest.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 3);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.ExistingCaseUnit.Populated = true;
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private bool ReadCodeCodeValue1()
  {
    entities.CodeValue.Populated = false;
    entities.Code.Populated = false;

    return Read("ReadCodeCodeValue1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cdvalue1", local.Sequence.Cdvalue);
        db.SetString(command, "cdvalue2", local.Agency.Cdvalue);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Id = db.GetInt32(reader, 4);
        entities.CodeValue.Cdvalue = db.GetString(reader, 5);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 6);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 7);
        entities.CodeValue.Description = db.GetString(reader, 8);
        entities.CodeValue.Populated = true;
        entities.Code.Populated = true;
      });
  }

  private bool ReadCodeCodeValue2()
  {
    entities.CodeValue.Populated = false;
    entities.Code.Populated = false;

    return Read("ReadCodeCodeValue2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cdvalue", local.Agency.Cdvalue);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Id = db.GetInt32(reader, 4);
        entities.CodeValue.Cdvalue = db.GetString(reader, 5);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 6);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 7);
        entities.CodeValue.Description = db.GetString(reader, 8);
        entities.CodeValue.Populated = true;
        entities.Code.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLocateRequest()
  {
    entities.LocateRequest.Populated = false;

    return ReadEach("ReadLocateRequest",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", import.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.Ending.Timestamp.GetValueOrDefault());
        db.SetString(command, "lastUpdatedBy", global.UserId);
      },
      (db, reader) =>
      {
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 0);
        entities.LocateRequest.LicenseNumber = db.GetNullableString(reader, 1);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 2);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 3);
        entities.LocateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.LocateRequest.CreatedBy = db.GetString(reader, 5);
        entities.LocateRequest.LastUpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.LocateRequest.LastUpdatedBy = db.GetString(reader, 7);
        entities.LocateRequest.LicenseSuspensionInd =
          db.GetNullableString(reader, 8);
        entities.LocateRequest.Populated = true;

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
    /// <summary>
    /// A value of ProcessAllLocateRequst.
    /// </summary>
    [JsonPropertyName("processAllLocateRequst")]
    public Common ProcessAllLocateRequst
    {
      get => processAllLocateRequst ??= new();
      set => processAllLocateRequst = value;
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
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public DateWorkArea Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    private Common processAllLocateRequst;
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
    /// <summary>A ServiceProviderTableGroup group.</summary>
    [Serializable]
    public class ServiceProviderTableGroup
    {
      /// <summary>
      /// A value of TableSaved.
      /// </summary>
      [JsonPropertyName("tableSaved")]
      public ServiceProvider TableSaved
      {
        get => tableSaved ??= new();
        set => tableSaved = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5000;

      private ServiceProvider tableSaved;
    }

    /// <summary>
    /// A value of Sequence.
    /// </summary>
    [JsonPropertyName("sequence")]
    public CodeValue Sequence
    {
      get => sequence ??= new();
      set => sequence = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LocateRequest Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// Gets a value of ServiceProviderTable.
    /// </summary>
    [JsonIgnore]
    public Array<ServiceProviderTableGroup> ServiceProviderTable =>
      serviceProviderTable ??= new(ServiceProviderTableGroup.Capacity);

    /// <summary>
    /// Gets a value of ServiceProviderTable for json serialization.
    /// </summary>
    [JsonPropertyName("serviceProviderTable")]
    [Computed]
    public IList<ServiceProviderTableGroup> ServiceProviderTable_Json
    {
      get => serviceProviderTable;
      set => ServiceProviderTable.Assign(value);
    }

    /// <summary>
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public LocateRequest Save
    {
      get => save ??= new();
      set => save = value;
    }

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
    /// A value of ProcessAllLocateRequest.
    /// </summary>
    [JsonPropertyName("processAllLocateRequest")]
    public Common ProcessAllLocateRequest
    {
      get => processAllLocateRequest ??= new();
      set => processAllLocateRequest = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private CodeValue sequence;
    private LocateRequest previous;
    private Array<ServiceProviderTableGroup> serviceProviderTable;
    private CodeValue agency;
    private LocateRequest save;
    private Case1 existing;
    private Infrastructure pass;
    private DateWorkArea current;
    private Common processAllLocateRequest;
    private CsePersonAddress csePersonAddress;
    private DateWorkArea nullDate;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
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
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private CodeValue codeValue;
    private Code code;
    private LocateRequest locateRequest;
    private CaseUnit existingCaseUnit;
    private CsePerson existingObligor;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
    private CsePerson existingCsePerson;
  }
#endregion
}
