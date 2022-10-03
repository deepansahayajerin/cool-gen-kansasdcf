// Program: OE_UPDATE_MILITARY_SERVICE, ID: 371920136, model: 746.
// Short name: SWE00971
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_UPDATE_MILITARY_SERVICE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeUpdateMilitaryService: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_MILITARY_SERVICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateMilitaryService(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateMilitaryService.
  /// </summary>
  public OeUpdateMilitaryService(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date     	Developer	Description
    // 02/19/95 	Sid 		Initial Creation.
    // 04/02/96	T.O.Redmond	Remove reference to BAQ Allotment and Rate of Pay.
    // ---------------------------------------------
    // ---------------------------------------------
    // Verify the CSE Person Number entered on the
    // screen that the military record is being
    // created against.
    // ---------------------------------------------
    export.MilitaryService.Assign(import.MilitaryService);

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;

      // ---------------------------------------------
      // Update the military service record for the
      // CSE Person identified.
      // ---------------------------------------------
      if (ReadMilitaryService())
      {
        try
        {
          UpdateMilitaryService();
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "MILITARY_SERVICE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "MILITARY_SERVICE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "MILITARY_SERVICE_NF";
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
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
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadMilitaryService()
  {
    entities.MilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.MilitaryService.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.MilitaryService.CspNumber = db.GetString(reader, 1);
        entities.MilitaryService.StartDate = db.GetNullableDate(reader, 2);
        entities.MilitaryService.Street1 = db.GetNullableString(reader, 3);
        entities.MilitaryService.Street2 = db.GetNullableString(reader, 4);
        entities.MilitaryService.City = db.GetNullableString(reader, 5);
        entities.MilitaryService.State = db.GetNullableString(reader, 6);
        entities.MilitaryService.Province = db.GetNullableString(reader, 7);
        entities.MilitaryService.PostalCode = db.GetNullableString(reader, 8);
        entities.MilitaryService.ZipCode5 = db.GetNullableString(reader, 9);
        entities.MilitaryService.ZipCode4 = db.GetNullableString(reader, 10);
        entities.MilitaryService.Zip3 = db.GetNullableString(reader, 11);
        entities.MilitaryService.Country = db.GetNullableString(reader, 12);
        entities.MilitaryService.Apo = db.GetNullableString(reader, 13);
        entities.MilitaryService.ExpectedReturnDateToStates =
          db.GetNullableDate(reader, 14);
        entities.MilitaryService.OverseasDutyStation =
          db.GetNullableString(reader, 15);
        entities.MilitaryService.ExpectedDischargeDate =
          db.GetNullableDate(reader, 16);
        entities.MilitaryService.Phone = db.GetNullableInt32(reader, 17);
        entities.MilitaryService.BranchCode = db.GetNullableString(reader, 18);
        entities.MilitaryService.CommandingOfficerLastName =
          db.GetNullableString(reader, 19);
        entities.MilitaryService.CommandingOfficerFirstName =
          db.GetNullableString(reader, 20);
        entities.MilitaryService.CommandingOfficerMi =
          db.GetNullableString(reader, 21);
        entities.MilitaryService.CurrentUsDutyStation =
          db.GetNullableString(reader, 22);
        entities.MilitaryService.DutyStatusCode =
          db.GetNullableString(reader, 23);
        entities.MilitaryService.Rank = db.GetNullableString(reader, 24);
        entities.MilitaryService.EndDate = db.GetNullableDate(reader, 25);
        entities.MilitaryService.CreatedBy = db.GetString(reader, 26);
        entities.MilitaryService.CreatedTimestamp = db.GetDateTime(reader, 27);
        entities.MilitaryService.LastUpdatedBy = db.GetString(reader, 28);
        entities.MilitaryService.LastUpdatedTimestamp =
          db.GetDateTime(reader, 29);
        entities.MilitaryService.PhoneCountryCode =
          db.GetNullableString(reader, 30);
        entities.MilitaryService.PhoneExt = db.GetNullableString(reader, 31);
        entities.MilitaryService.PhoneAreaCode =
          db.GetNullableInt32(reader, 32);
        entities.MilitaryService.Populated = true;
      });
  }

  private void UpdateMilitaryService()
  {
    System.Diagnostics.Debug.Assert(entities.MilitaryService.Populated);

    var startDate = import.MilitaryService.StartDate;
    var street1 = export.MilitaryService.Street1 ?? "";
    var street2 = export.MilitaryService.Street2 ?? "";
    var city = export.MilitaryService.City ?? "";
    var state = export.MilitaryService.State ?? "";
    var zipCode5 = export.MilitaryService.ZipCode5 ?? "";
    var zipCode4 = export.MilitaryService.ZipCode4 ?? "";
    var zip3 = export.MilitaryService.Zip3 ?? "";
    var country = export.MilitaryService.Country ?? "";
    var apo = export.MilitaryService.Apo ?? "";
    var expectedReturnDateToStates =
      export.MilitaryService.ExpectedReturnDateToStates;
    var overseasDutyStation = export.MilitaryService.OverseasDutyStation ?? "";
    var expectedDischargeDate = export.MilitaryService.ExpectedDischargeDate;
    var phone = export.MilitaryService.Phone.GetValueOrDefault();
    var branchCode = export.MilitaryService.BranchCode ?? "";
    var commandingOfficerLastName =
      export.MilitaryService.CommandingOfficerLastName ?? "";
    var commandingOfficerFirstName =
      export.MilitaryService.CommandingOfficerFirstName ?? "";
    var commandingOfficerMi = export.MilitaryService.CommandingOfficerMi ?? "";
    var currentUsDutyStation = export.MilitaryService.CurrentUsDutyStation ?? ""
      ;
    var dutyStatusCode = export.MilitaryService.DutyStatusCode ?? "";
    var rank = export.MilitaryService.Rank ?? "";
    var endDate = export.MilitaryService.EndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var phoneExt = export.MilitaryService.PhoneExt ?? "";
    var phoneAreaCode =
      export.MilitaryService.PhoneAreaCode.GetValueOrDefault();

    entities.MilitaryService.Populated = false;
    Update("UpdateMilitaryService",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "apo", apo);
        db.
          SetNullableDate(command, "expReturnDate", expectedReturnDateToStates);
          
        db.SetNullableString(command, "overseasDutyStn", overseasDutyStation);
        db.SetNullableDate(command, "expDischDate", expectedDischargeDate);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableString(command, "branchCode", branchCode);
        db.SetNullableString(command, "coLastName", commandingOfficerLastName);
        db.
          SetNullableString(command, "coFirstName", commandingOfficerFirstName);
          
        db.SetNullableString(command, "coMi", commandingOfficerMi);
        db.SetNullableString(command, "currUsDutyStn", currentUsDutyStation);
        db.SetNullableString(command, "dutyStatusCode", dutyStatusCode);
        db.SetNullableString(command, "rank", rank);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "phoneExt", phoneExt);
        db.SetNullableInt32(command, "phoneAreaCode", phoneAreaCode);
        db.SetDate(
          command, "effectiveDate",
          entities.MilitaryService.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.MilitaryService.CspNumber);
      });

    entities.MilitaryService.StartDate = startDate;
    entities.MilitaryService.Street1 = street1;
    entities.MilitaryService.Street2 = street2;
    entities.MilitaryService.City = city;
    entities.MilitaryService.State = state;
    entities.MilitaryService.ZipCode5 = zipCode5;
    entities.MilitaryService.ZipCode4 = zipCode4;
    entities.MilitaryService.Zip3 = zip3;
    entities.MilitaryService.Country = country;
    entities.MilitaryService.Apo = apo;
    entities.MilitaryService.ExpectedReturnDateToStates =
      expectedReturnDateToStates;
    entities.MilitaryService.OverseasDutyStation = overseasDutyStation;
    entities.MilitaryService.ExpectedDischargeDate = expectedDischargeDate;
    entities.MilitaryService.Phone = phone;
    entities.MilitaryService.BranchCode = branchCode;
    entities.MilitaryService.CommandingOfficerLastName =
      commandingOfficerLastName;
    entities.MilitaryService.CommandingOfficerFirstName =
      commandingOfficerFirstName;
    entities.MilitaryService.CommandingOfficerMi = commandingOfficerMi;
    entities.MilitaryService.CurrentUsDutyStation = currentUsDutyStation;
    entities.MilitaryService.DutyStatusCode = dutyStatusCode;
    entities.MilitaryService.Rank = rank;
    entities.MilitaryService.EndDate = endDate;
    entities.MilitaryService.LastUpdatedBy = lastUpdatedBy;
    entities.MilitaryService.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MilitaryService.PhoneExt = phoneExt;
    entities.MilitaryService.PhoneAreaCode = phoneAreaCode;
    entities.MilitaryService.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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

    private MilitaryService militaryService;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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

    private MilitaryService militaryService;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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

    private MilitaryService militaryService;
    private CsePerson csePerson;
  }
#endregion
}
