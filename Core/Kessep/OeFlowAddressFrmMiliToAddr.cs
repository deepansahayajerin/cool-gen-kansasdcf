// Program: OE_FLOW_ADDRESS_FRM_MILI_TO_ADDR, ID: 373323732, model: 746.
// Short name: SWE00912
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FLOW_ADDRESS_FRM_MILI_TO_ADDR.
/// </summary>
[Serializable]
public partial class OeFlowAddressFrmMiliToAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FLOW_ADDRESS_FRM_MILI_TO_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFlowAddressFrmMiliToAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFlowAddressFrmMiliToAddr.
  /// </summary>
  public OeFlowAddressFrmMiliToAddr(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    // Initial Development:
    // 03/12/2001              Vithal Madhira             WR# 261
    // This action block is used to create a new 'cse_person_address' record.
    // --------------------------------------------------------------------
    local.MilitaryService.Assign(import.MilitaryService);
    local.CurrentTimestamp.Timestamp = Now();
    local.CurrentDate.Date = Now().Date;
    local.MaxDate.Date = new DateTime(2099, 12, 31);

    if (AsChar(import.FlowData.Flag) == 'D')
    {
      // ------------------------------------------------------------------------------------
      // If the military record is deleted , end_date the corresponding 
      // cse_person_record on  ADDR screen  with the end_date equal to 'CURRENT
      // DATE'  and end_code equal to 'ME'.
      // -------------------------------------------------------------------------------------
      local.MilitaryService.EndDate = local.CurrentDate.Date;
    }

    switch(TrimEnd(import.FlowData.Command))
    {
      case "CREATE":
        if (ReadCsePerson())
        {
          try
          {
            CreateCsePersonAddress();

            // -------------------------------------------------------------------------------
            // Read each case_role for the CSE_person on all cases. Then  raise 
            // the corresponding events according to business rules (WR# 261).
            // -------------------------------------------------------------------------------
            foreach(var item in ReadCaseRole())
            {
              local.CaseRole.Type1 = entities.CaseRole.Type1;
              local.Infrastructure.UserId = "ADDR";
              local.Infrastructure.EventId = 10;
              local.Infrastructure.BusinessObjectCd = "CAU";
              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.DenormTimestamp =
                entities.CsePersonAddress.Identifier;

              if (Lt(entities.CsePersonAddress.EndDate, local.MaxDate.Date))
              {
                local.Infrastructure.ReferenceDate =
                  entities.CsePersonAddress.EndDate;
              }
              else
              {
                local.Infrastructure.ReferenceDate = local.CurrentDate.Date;
              }

              local.Infrastructure.Detail =
                Spaces(Infrastructure.Detail_MaxLength);

              if (Equal(local.CaseRole.Type1, "AP"))
              {
                local.AparSelection.Text1 = "P";
                local.Infrastructure.ReasonCode = "ADDRVRFDAP";
              }
              else if (Equal(local.CaseRole.Type1, "AR"))
              {
                local.AparSelection.Text1 = "R";
                local.Infrastructure.ReasonCode = "ADDRVRFDAR";
              }

              UseSiAddrRaiseEvent();
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSE_PERSON_ADDRESS_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSE_PERSON_ADDRESS_PV";

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
          ExitState = "CSE_PERSON_NF";
        }

        break;
      case "UPDATE":
        if (ReadCsePersonAddress())
        {
          if (Lt(local.Blank.Date, local.MilitaryService.EndDate))
          {
            // ----------------------------------------------------------------------
            // The address is end_dated. So update the cse_person_address and 
            // also raise the events.
            // ----------------------------------------------------------------------
            try
            {
              UpdateCsePersonAddress1();

              // -------------------------------------------------------------------------------
              // Read each case_role for the CSE_person on all cases. Then  
              // raise the corresponding events according to business rules (WR#
              // 261).
              // -------------------------------------------------------------------------------
              foreach(var item in ReadCaseRole())
              {
                local.CaseRole.Type1 = entities.CaseRole.Type1;
                local.Infrastructure.UserId = "ADDR";
                local.Infrastructure.EventId = 10;
                local.Infrastructure.BusinessObjectCd = "CAU";
                local.Infrastructure.SituationNumber = 0;
                local.Infrastructure.DenormTimestamp =
                  entities.CsePersonAddress.Identifier;

                if (Lt(entities.CsePersonAddress.EndDate, local.MaxDate.Date))
                {
                  local.Infrastructure.ReferenceDate =
                    entities.CsePersonAddress.EndDate;
                }
                else
                {
                  local.Infrastructure.ReferenceDate = local.CurrentDate.Date;
                }

                local.Infrastructure.Detail =
                  Spaces(Infrastructure.Detail_MaxLength);

                if (Equal(local.CaseRole.Type1, "AP"))
                {
                  local.AparSelection.Text1 = "P";
                  local.Infrastructure.ReasonCode = "ADDREXPDAP";
                }
                else if (Equal(local.CaseRole.Type1, "AR"))
                {
                  local.AparSelection.Text1 = "R";
                  local.Infrastructure.ReasonCode = "ADDREXPDAR";
                }

                UseSiAddrRaiseEvent();
              }
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_ADDRESS_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_ADDRESS_PV";

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
            // ----------------------------------------------------------------------
            // The address is not end_dated but other fields are updated. So 
            // update the cse_person_address . No need to raise the events since
            // the address is not end_dated.
            // ----------------------------------------------------------------------
            try
            {
              UpdateCsePersonAddress2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_ADDRESS_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CSE_PERSON_ADDRESS_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        break;
      default:
        break;
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
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

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.AparSelection.Text1 = local.AparSelection.Text1;

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);
  }

  private void CreateCsePersonAddress()
  {
    var identifier = local.CurrentTimestamp.Timestamp;
    var cspNumber = entities.CsePerson.Number;
    var source = "MB";
    var street1 = local.MilitaryService.Street1 ?? "";
    var street2 = local.MilitaryService.Street2 ?? "";
    var city = local.MilitaryService.City ?? "";
    var type1 = "M";
    var workerId = global.UserId;
    var verifiedDate = local.MilitaryService.EffectiveDate;
    var endDate = local.MaxDate.Date;
    var state = local.MilitaryService.State ?? "";
    var zipCode = local.MilitaryService.ZipCode5 ?? "";
    var zip4 = local.MilitaryService.ZipCode4 ?? "";
    var locationType = "D";

    CheckValid<CsePersonAddress>("LocationType", locationType);
    entities.CsePersonAddress.Populated = false;
    Update("CreateCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "zdelStartDate", null);
        db.SetNullableDate(command, "sendDate", null);
        db.SetNullableString(command, "source", source);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", "");
        db.SetNullableString(command, "zdelVerifiedCode", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", workerId);
        db.SetNullableDateTime(command, "createdTimestamp", identifier);
        db.SetNullableString(command, "createdBy", workerId);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "county", "");
      });

    entities.CsePersonAddress.Identifier = identifier;
    entities.CsePersonAddress.CspNumber = cspNumber;
    entities.CsePersonAddress.ZdelStartDate = null;
    entities.CsePersonAddress.SendDate = null;
    entities.CsePersonAddress.Source = source;
    entities.CsePersonAddress.Street1 = street1;
    entities.CsePersonAddress.Street2 = street2;
    entities.CsePersonAddress.City = city;
    entities.CsePersonAddress.Type1 = type1;
    entities.CsePersonAddress.WorkerId = workerId;
    entities.CsePersonAddress.VerifiedDate = verifiedDate;
    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = "";
    entities.CsePersonAddress.ZdelVerifiedCode = "";
    entities.CsePersonAddress.LastUpdatedTimestamp = identifier;
    entities.CsePersonAddress.LastUpdatedBy = workerId;
    entities.CsePersonAddress.CreatedTimestamp = identifier;
    entities.CsePersonAddress.CreatedBy = workerId;
    entities.CsePersonAddress.State = state;
    entities.CsePersonAddress.ZipCode = zipCode;
    entities.CsePersonAddress.Zip4 = zip4;
    entities.CsePersonAddress.Zip3 = "";
    entities.CsePersonAddress.LocationType = locationType;
    entities.CsePersonAddress.County = "";
    entities.CsePersonAddress.Populated = true;
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;

        return true;
      });
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

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 11);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 22);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 23);
        entities.CsePersonAddress.Populated = true;
      });
  }

  private void UpdateCsePersonAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var endDate = local.MilitaryService.EndDate;
    var endCode = "ME";
    var lastUpdatedTimestamp = local.CurrentTimestamp.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress1",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
    entities.CsePersonAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonAddress.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAddress.Populated = true;
  }

  private void UpdateCsePersonAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var source = "MB";
    var street1 = import.MilitaryService.Street1 ?? "";
    var street2 = import.MilitaryService.Street2 ?? "";
    var city = import.MilitaryService.City ?? "";
    var type1 = "M";
    var verifiedDate = import.MilitaryService.EffectiveDate;
    var lastUpdatedTimestamp = local.CurrentTimestamp.Timestamp;
    var lastUpdatedBy = global.UserId;
    var state = import.MilitaryService.State ?? "";
    var zipCode = import.MilitaryService.ZipCode5 ?? "";
    var zip4 = import.MilitaryService.ZipCode4 ?? "";

    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress2",
      (db, command) =>
      {
        db.SetNullableString(command, "source", source);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "type", type1);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

    entities.CsePersonAddress.Source = source;
    entities.CsePersonAddress.Street1 = street1;
    entities.CsePersonAddress.Street2 = street2;
    entities.CsePersonAddress.City = city;
    entities.CsePersonAddress.Type1 = type1;
    entities.CsePersonAddress.VerifiedDate = verifiedDate;
    entities.CsePersonAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonAddress.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAddress.State = state;
    entities.CsePersonAddress.ZipCode = zipCode;
    entities.CsePersonAddress.Zip4 = zip4;
    entities.CsePersonAddress.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of FlowData.
    /// </summary>
    [JsonPropertyName("flowData")]
    public Common FlowData
    {
      get => flowData ??= new();
      set => flowData = value;
    }

    private CsePerson csePerson;
    private MilitaryService militaryService;
    private Common flowData;
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
    /// A value of CurrentTimestamp.
    /// </summary>
    [JsonPropertyName("currentTimestamp")]
    public DateWorkArea CurrentTimestamp
    {
      get => currentTimestamp ??= new();
      set => currentTimestamp = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    private DateWorkArea currentTimestamp;
    private DateWorkArea blank;
    private DateWorkArea maxDate;
    private CaseRole caseRole;
    private Infrastructure infrastructure;
    private DateWorkArea currentDate;
    private WorkArea aparSelection;
    private MilitaryService militaryService;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
