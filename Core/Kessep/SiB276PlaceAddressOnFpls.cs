// Program: SI_B276_PLACE_ADDRESS_ON_FPLS, ID: 373399509, model: 746.
// Short name: SWE01299
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B276_PLACE_ADDRESS_ON_FPLS.
/// </summary>
[Serializable]
public partial class SiB276PlaceAddressOnFpls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B276_PLACE_ADDRESS_ON_FPLS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB276PlaceAddressOnFpls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB276PlaceAddressOnFpls.
  /// </summary>
  public SiB276PlaceAddressOnFpls(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.FplsRequestUpdates.Count = import.FplsRequestUpdates.Count;
    export.RecordsAlreadyProcessed.Count = import.RecordsAlreadyProcessed.Count;
    export.AddressesPlacedOnFpls.Count = import.AddressesPlacedOnFpls.Count;
    local.Next.Identifier = 1;

    if (ReadCsePerson())
    {
      if (ReadFplsLocateRequest())
      {
        if (AsChar(entities.FplsLocateRequest.TransactionStatus) != 'R')
        {
          try
          {
            UpdateFplsLocateRequest();
            ++export.FplsRequestUpdates.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0000_FPLS_REQUEST_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OE0000_FPLS_REQUEST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        MoveFplsLocateRequest(entities.FplsLocateRequest,
          local.FplsLocateRequest);

        if (ReadFplsLocateResponse())
        {
          if (Equal(import.FplsLocateResponse.ReturnedAddress,
            entities.FplsLocateResponse.ReturnedAddress))
          {
            ++export.RecordsAlreadyProcessed.Count;

            return;
          }

          local.Next.Identifier = entities.FplsLocateResponse.Identifier + 1;
        }
      }

      if (local.FplsLocateRequest.Identifier == 0)
      {
        local.FplsLocateRequest.Identifier = 1;
        local.FplsLocateRequest.Ssn = import.FplsLocateRequest.Ssn ?? "";
        local.FplsLocateRequest.CaseId = import.CsePerson.Number + NumberToString
          (local.FplsLocateRequest.Identifier, 13, 3);
        local.FplsLocateRequest.CreatedBy = import.ProgramProcessingInfo.Name;
        local.FplsLocateRequest.LastUpdatedBy =
          import.ProgramProcessingInfo.Name;
        local.FplsLocateRequest.RequestSentDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.FplsLocateRequest.UsersField = "Y";
        local.FplsLocateRequest.TransactionStatus = "R";
        local.FplsLocateRequest.StateAbbr = "KS";
        local.FplsLocateRequest.StationNumber = "02";
        local.FplsLocateRequest.TransactionType = "A";
        UseSiB276CreateFplsLocateReqst();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.FplsLocateResponse.Identifier = local.Next.Identifier;
      local.FplsLocateResponse.ApNameReturned =
        import.FplsLocateResponse.ApNameReturned ?? "";
      local.FplsLocateResponse.ReturnedAddress =
        import.FplsLocateResponse.ReturnedAddress ?? "";
      local.FplsLocateResponse.SsnReturned = import.FplsLocateRequest.Ssn ?? "";
      local.FplsLocateResponse.DateReceived =
        import.ProgramProcessingInfo.ProcessDate;
      local.FplsLocateResponse.UsageStatus = "U";
      local.FplsLocateResponse.AgencyCode = "H99";
      local.FplsLocateResponse.NameSentInd = "1";
      local.FplsLocateResponse.AddrDateFormatInd = "0";
      local.FplsLocateResponse.AddressFormatInd = "X";
      local.FplsLocateResponse.Fein = import.FplsLocateResponse.Fein ?? "";
      local.FplsLocateResponse.NdnhResponse = "N";
      local.FplsLocateResponse.CreatedBy = import.ProgramProcessingInfo.Name;
      local.FplsLocateResponse.LastUpdatedBy =
        import.ProgramProcessingInfo.Name;
      local.FplsLocateResponse.DateOfHire =
        import.FplsLocateResponse.DateOfHire;
      local.FplsLocateResponse.ResponseCode = "";
      local.FplsLocateResponse.SesaRespondingState = "20";
      UseSiB276CreateFplsResponse();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++export.AddressesPlacedOnFpls.Count;
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.EventId = 10;
      local.Infrastructure.UserId = import.ProgramProcessingInfo.Name;
      local.Infrastructure.ReasonCode = "KSNEWHIRE";
      local.Infrastructure.BusinessObjectCd = "ICS";
      local.Infrastructure.DenormNumeric12 =
        entities.FplsLocateRequest.Identifier;
      local.Infrastructure.ReferenceDate =
        import.ProgramProcessingInfo.ProcessDate;
      local.Infrastructure.Detail =
        "Address received from State New Hire. Display FPLS.";
      local.Infrastructure.ProcessStatus = "Q";
      UseOeCabRaiseEvent();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private static void MoveFplsLocateRequest(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiB276CreateFplsLocateReqst()
  {
    var useImport = new SiB276CreateFplsLocateReqst.Import();
    var useExport = new SiB276CreateFplsLocateReqst.Export();

    useImport.FplsLocateRequest.Assign(local.FplsLocateRequest);
    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(SiB276CreateFplsLocateReqst.Execute, useImport, useExport);
  }

  private void UseSiB276CreateFplsResponse()
  {
    var useImport = new SiB276CreateFplsResponse.Import();
    var useExport = new SiB276CreateFplsResponse.Export();

    useImport.FplsLocateResponse.Assign(local.FplsLocateResponse);
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.FplsLocateRequest.Identifier = local.FplsLocateRequest.Identifier;

    Call(SiB276CreateFplsResponse.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest()
  {
    entities.FplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.FplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.FplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.FplsLocateRequest.LastUpdatedBy = db.GetString(reader, 3);
        entities.FplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.FplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse()
  {
    System.Diagnostics.Debug.Assert(entities.FplsLocateRequest.Populated);
    entities.FplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier", entities.FplsLocateRequest.Identifier);
        db.
          SetString(command, "cspNumber", entities.FplsLocateRequest.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.FplsLocateResponse.FlqIdentifier = db.GetInt32(reader, 0);
        entities.FplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.FplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.FplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 3);
        entities.FplsLocateResponse.Populated = true;
      });
  }

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.FplsLocateRequest.Populated);

    var transactionStatus = "R";
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    entities.FplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.
          SetString(command, "cspNumber", entities.FplsLocateRequest.CspNumber);
          
        db.
          SetInt32(command, "identifier", entities.FplsLocateRequest.Identifier);
          
      });

    entities.FplsLocateRequest.TransactionStatus = transactionStatus;
    entities.FplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.FplsLocateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.FplsLocateRequest.Populated = true;
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
    /// A value of AddressesPlacedOnFpls.
    /// </summary>
    [JsonPropertyName("addressesPlacedOnFpls")]
    public Common AddressesPlacedOnFpls
    {
      get => addressesPlacedOnFpls ??= new();
      set => addressesPlacedOnFpls = value;
    }

    /// <summary>
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of FplsRequestUpdates.
    /// </summary>
    [JsonPropertyName("fplsRequestUpdates")]
    public Common FplsRequestUpdates
    {
      get => fplsRequestUpdates ??= new();
      set => fplsRequestUpdates = value;
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
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
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

    private Common addressesPlacedOnFpls;
    private Common recordsAlreadyProcessed;
    private Common fplsRequestUpdates;
    private CsePerson csePerson;
    private FplsLocateResponse fplsLocateResponse;
    private FplsLocateRequest fplsLocateRequest;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AddressesPlacedOnFpls.
    /// </summary>
    [JsonPropertyName("addressesPlacedOnFpls")]
    public Common AddressesPlacedOnFpls
    {
      get => addressesPlacedOnFpls ??= new();
      set => addressesPlacedOnFpls = value;
    }

    /// <summary>
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of FplsRequestUpdates.
    /// </summary>
    [JsonPropertyName("fplsRequestUpdates")]
    public Common FplsRequestUpdates
    {
      get => fplsRequestUpdates ??= new();
      set => fplsRequestUpdates = value;
    }

    private Common addressesPlacedOnFpls;
    private Common recordsAlreadyProcessed;
    private Common fplsRequestUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public FplsLocateResponse Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ConvertDate.
    /// </summary>
    [JsonPropertyName("convertDate")]
    public DateWorkArea ConvertDate
    {
      get => convertDate ??= new();
      set => convertDate = value;
    }

    private FplsLocateRequest fplsLocateRequest;
    private FplsLocateResponse fplsLocateResponse;
    private FplsLocateResponse next;
    private Infrastructure infrastructure;
    private DateWorkArea convertDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    private CsePerson csePerson;
    private FplsLocateRequest fplsLocateRequest;
    private FplsLocateResponse fplsLocateResponse;
  }
#endregion
}
