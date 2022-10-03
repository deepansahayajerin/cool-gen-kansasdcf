// Program: OE_FLOW_ADDRESS_FRM_JAIL_TO_ADDR, ID: 373321919, model: 746.
// Short name: SWE02077
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FLOW_ADDRESS_FRM_JAIL_TO_ADDR.
/// </summary>
[Serializable]
public partial class OeFlowAddressFrmJailToAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FLOW_ADDRESS_FRM_JAIL_TO_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFlowAddressFrmJailToAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFlowAddressFrmJailToAddr.
  /// </summary>
  public OeFlowAddressFrmJailToAddr(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------
    // 12/20/2007  LSS   PR204510 CQ448   Added IF statement to case "CREATE" to
    // check
    //                                    
    // if cse_person_address Verified Date
    // is greater than
    //                                    
    // Incarcerated Release Date to avoid
    // end-dating all
    //                                    
    // verified addresses on ADDR.
    // ---------------------------------------------------------------------------------
    local.CurrentDate.Date = Now().Date;
    local.CurrentTimestamp.Timestamp = Now();
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    MoveCommon(import.Common, local.Common);

    // --------------------------------------------------------------------------------------
    // Vithal Madhira                  WR# 261               03/06/2001
    // Initial Development:
    // Per WR# 000261,   if a person is incarcerated (INCARCERATED = Y) and  if 
    // a verified date is entered on the JAIL screen, then read all previous
    // verified mailing and/or residential addresses from 'CSE_Person_Address'
    // table for the person# on JAIL screen and update them with an end_date of
    // 'current_date'  and an end_code of 'IC'. Also raise an event with reason
    // code ADDREXPDAP'  if the person is AP on the case or ADDREXPDAR' if
    // the person is AR on the case. We need to add the new address record to 
    // CSE_Person_Address' table with an address source of JAIL' and address
    // type to M'. The attributes will be set (mapped) according to business
    // rules. Also raise an event with reason code ADDRVRFDAP'  if the person
    // is AP on the case or ADDRVRFDAR' if the person is AR on the case. On
    // ADDR screen, protect the Send Dt' field and give the worker an error
    // message: Not able to send PM letter on a Jail/Prison address.
    // ----------------------------------------------------------------------------------
    // --------------------------------------------------------------
    // Find each verified address for the CSE_Person.
    // ---------------------------------------------------------------
    // JHarden   CQ55280    3/31/17   Inmate number flow to ADDR
    // 
    // ---------------------------------------------------------------
    switch(TrimEnd(local.Common.Command))
    {
      case "CREATE":
        foreach(var item in ReadCsePersonAddress2())
        {
          // 12/20/2007  LSS   PR204510 CQ448   Added IF statement
          if (Lt(import.Incarceration.EndDate,
            entities.CsePersonAddress.VerifiedDate))
          {
            continue;
          }

          // -------------------------------------------------------------------------------
          // Update the each verified address with an End_Date of 'Current_date'
          // and end_code of 'IC'.
          // -------------------------------------------------------------------------------
          try
          {
            UpdateCsePersonAddress1();

            // -------------------------------------------------------------------------------
            // Read each case_role for the CSE_person on all cases. Then  raise 
            // the corresponding events according to business rules (WR# 261).
            // -------------------------------------------------------------------------------
            foreach(var item1 in ReadCaseRoleCase())
            {
              local.CaseRole.Type1 = entities.CaseRole.Type1;
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
              local.Infrastructure.LastUpdatedBy = "";
              local.Infrastructure.ProcessStatus = "Q";
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

              if (ReadInterstateRequest())
              {
                if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
              }
              else
              {
                local.Infrastructure.InitiatingStateCode = "KS";

                // CQ55280 Commited out to help with end date on facilities.
              }

              if (Equal(local.CaseRole.Type1, "AP"))
              {
                local.AparSelection.Text1 = "P";
                local.Infrastructure.ReasonCode = "ADDREXPDAP";

                foreach(var item2 in ReadCaseUnit1())
                {
                  local.Infrastructure.CaseUnitNumber =
                    entities.CaseUnit.CuNumber;
                  UseSpCabCreateInfrastructure();
                }

                // -----------------------------------------------------------------------------------
                // If no case_unit found, raise the event at case level.
                // ----------------------------------------------------------------------------------
                if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0
                  )
                {
                  UseSpCabCreateInfrastructure();
                }
              }
              else if (Equal(local.CaseRole.Type1, "AR"))
              {
                local.AparSelection.Text1 = "R";
                local.Infrastructure.ReasonCode = "ADDREXPDAR";

                foreach(var item2 in ReadCaseUnit2())
                {
                  local.Infrastructure.CaseUnitNumber =
                    entities.CaseUnit.CuNumber;
                  UseSpCabCreateInfrastructure();
                }

                // -----------------------------------------------------------------------------------
                // If no case_unit found, raise the event at case level.
                // ----------------------------------------------------------------------------------
                if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0
                  )
                {
                  UseSpCabCreateInfrastructure();
                }
              }
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

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------------------------------------------------
        // Create a new CSE_Person_Address according to the business rules (WR# 
        // 261).
        // -------------------------------------------------------------------------------
        if (ReadCsePerson())
        {
          if (Lt(local.BlankDate.Date, import.Incarceration.EndDate) && Lt
            (import.Incarceration.EndDate, local.MaxDate.Date))
          {
            // -------------------------------------------------------------------------------
            // If Release_date (End_date) was entered on the JAIL SET that value
            // with an  'End_code' of 'IC'.
            // -------------------------------------------------------------------------------
            // CQ55280 Inmate number flow to ADDR
            try
            {
              CreateCsePersonAddress2();
              ExitState = "ACO_NN0000_ALL_OK";
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
            // CQ55280 Inmate number flow to ADDR
            try
            {
              CreateCsePersonAddress1();
              ExitState = "ACO_NN0000_ALL_OK";
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

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -------------------------------------------------------------------------------
            // Read each case_role for the CSE_person on all cases. Then  raise 
            // the corresponding events according to business rules (WR# 261).
            // -------------------------------------------------------------------------------
            foreach(var item in ReadCaseRoleCase())
            {
              local.CaseRole.Type1 = entities.CaseRole.Type1;
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
              local.Infrastructure.LastUpdatedBy = "";
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.UserId = "ADDR";
              local.Infrastructure.EventId = 10;
              local.Infrastructure.BusinessObjectCd = "CAU";
              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.DenormTimestamp =
                entities.CsePersonAddress.Identifier;

              if (Lt(entities.CsePersonAddress.VerifiedDate, local.MaxDate.Date))
                
              {
                local.Infrastructure.ReferenceDate =
                  entities.CsePersonAddress.VerifiedDate;
              }
              else
              {
                local.Infrastructure.ReferenceDate = local.CurrentDate.Date;
              }

              local.Infrastructure.Detail =
                Spaces(Infrastructure.Detail_MaxLength);

              if (ReadInterstateRequest())
              {
                if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
              }
              else
              {
                local.Infrastructure.InitiatingStateCode = "KS";
              }

              if (Equal(local.CaseRole.Type1, "AP"))
              {
                local.AparSelection.Text1 = "P";
                local.Infrastructure.ReasonCode = "ADDRVRFDAP";

                foreach(var item1 in ReadCaseUnit1())
                {
                  local.Infrastructure.CaseUnitNumber =
                    entities.CaseUnit.CuNumber;
                  UseSpCabCreateInfrastructure();
                }

                // -----------------------------------------------------------------------------------
                // If no case_unit found, raise the event at case level.
                // ----------------------------------------------------------------------------------
                if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0
                  )
                {
                  UseSpCabCreateInfrastructure();
                }
              }
              else if (Equal(local.CaseRole.Type1, "AR"))
              {
                local.AparSelection.Text1 = "R";
                local.Infrastructure.ReasonCode = "ADDRVRFDAR";

                foreach(var item1 in ReadCaseUnit2())
                {
                  local.Infrastructure.CaseUnitNumber =
                    entities.CaseUnit.CuNumber;
                  UseSpCabCreateInfrastructure();
                }

                // -----------------------------------------------------------------------------------
                // If no case_unit found, raise the event at case level.
                // ----------------------------------------------------------------------------------
                if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0
                  )
                {
                  UseSpCabCreateInfrastructure();
                }
              }
            }
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NF";
        }

        break;
      case "UPDATE":
        if (ReadCsePersonAddress1())
        {
          try
          {
            UpdateCsePersonAddress2();

            // -------------------------------------------------------------------------------
            // Read each case_role for the CSE_person on all cases. Then  raise 
            // the corresponding events according to business rules (WR# 261).
            // -------------------------------------------------------------------------------
            foreach(var item in ReadCaseRoleCase())
            {
              local.CaseRole.Type1 = entities.CaseRole.Type1;
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CsePersonNumber = import.CsePerson.Number;
              local.Infrastructure.LastUpdatedBy = "";
              local.Infrastructure.ProcessStatus = "Q";
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

              if (ReadInterstateRequest())
              {
                if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
              }
              else
              {
                local.Infrastructure.InitiatingStateCode = "KS";
              }

              if (Equal(local.CaseRole.Type1, "AP"))
              {
                local.AparSelection.Text1 = "P";
                local.Infrastructure.ReasonCode = "ADDREXPDAP";

                foreach(var item1 in ReadCaseUnit1())
                {
                  local.Infrastructure.CaseUnitNumber =
                    entities.CaseUnit.CuNumber;
                  UseSpCabCreateInfrastructure();
                }

                // -----------------------------------------------------------------------------------
                // If no case_unit found, raise the event at case level.
                // ----------------------------------------------------------------------------------
                if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0
                  )
                {
                  UseSpCabCreateInfrastructure();
                }
              }
              else if (Equal(local.CaseRole.Type1, "AR"))
              {
                local.AparSelection.Text1 = "R";
                local.Infrastructure.ReasonCode = "ADDREXPDAR";

                foreach(var item1 in ReadCaseUnit2())
                {
                  local.Infrastructure.CaseUnitNumber =
                    entities.CaseUnit.CuNumber;
                  UseSpCabCreateInfrastructure();
                }

                // -----------------------------------------------------------------------------------
                // If no case_unit found, raise the event at case level.
                // ----------------------------------------------------------------------------------
                if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() <= 0
                  )
                {
                  UseSpCabCreateInfrastructure();
                }
              }
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

        break;
      default:
        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void CreateCsePersonAddress1()
  {
    var identifier = local.CurrentTimestamp.Timestamp;
    var cspNumber = entities.CsePerson.Number;
    var source = "JAIL";
    var street1 = import.IncarcerationAddress.Street1 ?? "";
    var street2 =
      Substring("INMATE # " + (import.Incarceration.InmateNumber ?? ""), 1, 25);
      
    var city = import.IncarcerationAddress.City ?? "";
    var type1 = "M";
    var workerId = global.UserId;
    var verifiedDate = import.Incarceration.VerifiedDate;
    var endDate = local.MaxDate.Date;
    var state = import.IncarcerationAddress.State ?? "";
    var zipCode = import.IncarcerationAddress.ZipCode5 ?? "";
    var zip4 = import.IncarcerationAddress.ZipCode4 ?? "";
    var locationType = "D";

    CheckValid<CsePersonAddress>("LocationType", locationType);
    entities.CsePersonAddress.Populated = false;
    Update("CreateCsePersonAddress1",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "zdelStartDate", default(DateTime));
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
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "county", "");
      });

    entities.CsePersonAddress.Identifier = identifier;
    entities.CsePersonAddress.CspNumber = cspNumber;
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

  private void CreateCsePersonAddress2()
  {
    var identifier = local.CurrentTimestamp.Timestamp;
    var cspNumber = entities.CsePerson.Number;
    var source = "JAIL";
    var street1 = import.IncarcerationAddress.Street1 ?? "";
    var street2 =
      Substring("INMATE # " + (import.Incarceration.InmateNumber ?? ""), 1, 25);
      
    var city = import.IncarcerationAddress.City ?? "";
    var type1 = "M";
    var workerId = global.UserId;
    var verifiedDate = import.Incarceration.VerifiedDate;
    var endDate = import.Incarceration.EndDate;
    var endCode = "IC";
    var state = import.IncarcerationAddress.State ?? "";
    var zipCode = import.IncarcerationAddress.ZipCode5 ?? "";
    var zip4 = import.IncarcerationAddress.ZipCode4 ?? "";
    var locationType = "D";

    CheckValid<CsePersonAddress>("LocationType", locationType);
    entities.CsePersonAddress.Populated = false;
    Update("CreateCsePersonAddress2",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "zdelStartDate", default(DateTime));
        db.SetNullableDate(command, "sendDate", null);
        db.SetNullableString(command, "source", source);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
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
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "county", "");
      });

    entities.CsePersonAddress.Identifier = identifier;
    entities.CsePersonAddress.CspNumber = cspNumber;
    entities.CsePersonAddress.SendDate = null;
    entities.CsePersonAddress.Source = source;
    entities.CsePersonAddress.Street1 = street1;
    entities.CsePersonAddress.Street2 = street2;
    entities.CsePersonAddress.City = city;
    entities.CsePersonAddress.Type1 = type1;
    entities.CsePersonAddress.WorkerId = workerId;
    entities.CsePersonAddress.VerifiedDate = verifiedDate;
    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
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

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", import.CsePerson.Number);
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAr", import.CsePerson.Number);
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.Populated = true;

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
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
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
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 20);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate", local.BlankDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 20);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private void UpdateCsePersonAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var endDate = local.CurrentDate.Date;
    var endCode = "IC";
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

    var endDate = import.Incarceration.EndDate;
    var endCode = "RL";
    var lastUpdatedTimestamp = local.CurrentTimestamp.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress2",
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of IncarcerationAddress.
    /// </summary>
    [JsonPropertyName("incarcerationAddress")]
    public IncarcerationAddress IncarcerationAddress
    {
      get => incarcerationAddress ??= new();
      set => incarcerationAddress = value;
    }

    private Common common;
    private CsePerson csePerson;
    private Incarceration incarceration;
    private IncarcerationAddress incarcerationAddress;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of BlankDate.
    /// </summary>
    [JsonPropertyName("blankDate")]
    public DateWorkArea BlankDate
    {
      get => blankDate ??= new();
      set => blankDate = value;
    }

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
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
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

    private DateWorkArea maxDate;
    private DateWorkArea blankDate;
    private DateWorkArea currentTimestamp;
    private DateWorkArea currentDate;
    private CaseRole caseRole;
    private Infrastructure infrastructure;
    private WorkArea aparSelection;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private CaseUnit caseUnit;
    private InterstateRequest interstateRequest;
  }
#endregion
}
