// Program: OE_CREATE_FPLS_LOCATE_REQ_BATCH, ID: 372362508, model: 746.
// Short name: SWE01537
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
/// A program: OE_CREATE_FPLS_LOCATE_REQ_BATCH.
/// </para>
/// <para>
/// Resp: OBLMGMT
/// This action block extracts FPLS-LOCATE-REQUEST's that have been selected for
/// transmission to the Federal Parent Locator Service(FPLS).
/// When initially created,the FPLS Status is set to a 'C'(Created).
/// The Batch job will change FPLS Status to a 'S'(Sent).
/// After we receive a response from FPLS the Status will be changed to 'R'(
/// Response received).
/// The remainder of the values are pulled from CSE-PERSON, CASE-ROLE, and ALIAS
/// Entities.
/// When this action block is called from the batch extract for FPLS 
/// transmission the import batch indicator will be set to &quot;Y&quot; and an
/// Export format will be created that matches the external transmission format
/// for FPLS.
/// </para>
/// </summary>
[Serializable]
public partial class OeCreateFplsLocateReqBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_FPLS_LOCATE_REQ_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateFplsLocateReqBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateFplsLocateReqBatch.
  /// </summary>
  public OeCreateFplsLocateReqBatch(IContext context, Import import,
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
    // ************************************************
    // Date	 Developer	Description
    // 01/13/96 Govind         Initial Creation
    // 03/12/96 T.O.Redmond    Modify Create Logic
    // 05/27/96 T.O.Redmond	Use new Batch Si-Read
    // ************************************************
    local.FplsLocateRequest.RequestSentDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.NullDate.Date = null;

    // ************************************************
    // *Get currency on the CSE Person.               *
    // ************************************************
    if (ReadCsePerson())
    {
      // ************************************************
      // *If a Request already exists with a Status "C" *
      // *then we will update the request with the      *
      // *Request Sent date. Otherwise we will create a *
      // *new request.
      // 
      // *
      // ************************************************
      local.FplsRequestFound.Flag = "N";

      if (ReadFplsLocateRequest1())
      {
        MoveFplsLocateRequest(entities.ExistingFplsLocateRequest,
          export.FplsLocateRequest);
        local.FplsRequestFound.Flag = "Y";
      }

      export.CsePersonsWorkSet.Number = import.CsePerson.Number;

      if (AsChar(local.FplsRequestFound.Flag) != 'Y')
      {
        UseSiReadCsePersonBatch();

        // ***	Sex is mandatory field. Default to "M". ***
        if (IsEmpty(local.CsePersonsWorkSet.Sex))
        {
          local.CsePersonsWorkSet.Sex = "M";
        }

        // ***	Add Alias fields here. ***
        UseEabReadAliasBatch();

        for(local.Alias.Index = 0; local.Alias.Index < 2; ++local.Alias.Index)
        {
          if (!local.Alias.CheckSize())
          {
            break;
          }

          switch(local.Alias.Index + 1)
          {
            case 1:
              local.FplsLocateRequest.ApSecondLastName =
                local.Alias.Item.G.LastName;

              break;
            case 2:
              local.FplsLocateRequest.ApThirdLastName =
                local.Alias.Item.G.LastName;

              break;
            default:
              goto AfterCycle;
          }
        }

AfterCycle:

        local.Alias.CheckIndex();
      }
      else
      {
        return;
      }

      if (ReadCaseCaseRole())
      {
        local.AdcNadcFlag.SelectChar = "";
        UseOeDetermineAdcPgmParticipatn();

        if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
        {
          // IDCR# 39943 -
          // Default Case Programs to ADC, if the Case Program does not exist.
          ExitState = "ACO_NN0000_ALL_OK";
          local.AdcNadcFlag.SelectChar = "A";
        }

        if (AsChar(local.AdcNadcFlag.SelectChar) == 'Y')
        {
          local.AdcNadcFlag.SelectChar = "A";
        }
        else
        {
          local.AdcNadcFlag.SelectChar = "N";
        }

        ReadFplsLocateRequest2();
        export.FplsLocateRequest.Identifier =
          entities.ExistingFplsLocateRequest.Identifier + 1;
        local.Work.Text3 =
          NumberToString(export.FplsLocateRequest.Identifier, 3);
        export.FplsLocateRequest.CaseId = entities.ExistingCsePerson.Number + local
          .Work.Text3;

        // ************************************************
        // *Create a new FPLS Request.                    *
        // ************************************************
        try
        {
          CreateFplsLocateRequest();
          MoveFplsLocateRequest(entities.ExistingFplsLocateRequest,
            export.FplsLocateRequest);
          ExitState = "ACO_NN0000_ALL_OK";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // ************************************************
              // *System error - cannot add a new row           *
              // ************************************************
              ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private static void MoveAlias(Local.AliasGroup source,
    EabReadAliasBatch.Export.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscarea.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases(EabReadAliasBatch.Export.AliasesGroup source,
    Local.AliasGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscarea.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveFplsLocateRequest(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
    target.RequestSentDate = source.RequestSentDate;
    target.StationNumber = source.StationNumber;
    target.TransactionType = source.TransactionType;
    target.Ssn = source.Ssn;
    target.CaseId = source.CaseId;
    target.LocalCode = source.LocalCode;
    target.UsersField = source.UsersField;
    target.TypeOfCase = source.TypeOfCase;
    target.TransactionError = source.TransactionError;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.SendRequestTo = source.SendRequestTo;
  }

  private void UseEabReadAliasBatch()
  {
    var useImport = new EabReadAliasBatch.Import();
    var useExport = new EabReadAliasBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    local.Alias.CopyTo(useExport.Aliases, MoveAlias);

    Call(EabReadAliasBatch.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Alias, MoveAliases);
  }

  private void UseOeDetermineAdcPgmParticipatn()
  {
    var useImport = new OeDetermineAdcPgmParticipatn.Import();
    var useExport = new OeDetermineAdcPgmParticipatn.Export();

    useImport.Ap.Number = entities.ExistingCsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(OeDetermineAdcPgmParticipatn.Execute, useImport, useExport);

    local.AdcNadcFlag.SelectChar = useExport.CaseInAdcProgram.SelectChar;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateFplsLocateRequest()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var identifier = export.FplsLocateRequest.Identifier;
    var transactionStatus = "C";
    var stateAbbr = "KS";
    var stationNumber = "02";
    var transactionType = "A";
    var ssn = local.CsePersonsWorkSet.Ssn;
    var caseId = export.FplsLocateRequest.CaseId ?? "";
    var usersField = "Y";
    var typeOfCase = local.AdcNadcFlag.SelectChar;
    var apFirstName = local.CsePersonsWorkSet.FirstName;
    var apMiddleName = local.CsePersonsWorkSet.MiddleInitial;
    var apFirstLastName = local.CsePersonsWorkSet.LastName;
    var apSecondLastName = local.FplsLocateRequest.ApSecondLastName ?? "";
    var apThirdLastName = local.FplsLocateRequest.ApThirdLastName ?? "";
    var apDateOfBirth = local.CsePersonsWorkSet.Dob;
    var sex = local.CsePersonsWorkSet.Sex;
    var collectAllResponsesTogether = "N";
    var apCityOfBirth = entities.ExistingCsePerson.BirthPlaceCity;
    var apStateOrCountryOfBirth = entities.ExistingCsePerson.BirthPlaceState;
    var apsFathersFirstName = entities.ExistingAp.FathersFirstName;
    var apsFathersMi = entities.ExistingAp.FathersMiddleInitial;
    var apsFathersLastName = entities.ExistingAp.FathersLastName ?? Substring
      (entities.ExistingAp.FathersLastName, 1, 16);
    var apsMothersFirstName = entities.ExistingAp.MothersFirstName;
    var createdBy = global.TranCode;
    var createdTimestamp = Now();
    var requestSentDate = local.NullDate.Date;

    entities.ExistingFplsLocateRequest.Populated = false;
    Update("CreateFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetNullableDate(command, "zdelReqCreatDt", default(DateTime));
        db.SetString(command, "zdelCreatUserId", "");
        db.SetNullableString(command, "stateAbbr", stateAbbr);
        db.SetNullableString(command, "stationNumber", stationNumber);
        db.SetNullableString(command, "transactionType", transactionType);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "caseId", caseId);
        db.SetNullableString(command, "localCode", "");
        db.SetNullableString(command, "usersField", usersField);
        db.SetNullableString(command, "typeOfCase", typeOfCase);
        db.SetNullableString(command, "apFirstName", apFirstName);
        db.SetNullableString(command, "apMiddleName", apMiddleName);
        db.SetNullableString(command, "apFirstLastName", apFirstLastName);
        db.SetNullableString(command, "apSecondLastNam", apSecondLastName);
        db.SetNullableString(command, "apThirdLastName", apThirdLastName);
        db.SetNullableDate(command, "apDateOfBirth", apDateOfBirth);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableString(
          command, "collectAllRespon", collectAllResponsesTogether);
        db.SetNullableString(command, "transactionError", "");
        db.SetNullableString(command, "apCityOfBirth", apCityOfBirth);
        db.
          SetNullableString(command, "apStateOrCountr", apStateOrCountryOfBirth);
          
        db.SetNullableString(command, "apsFathersFirst", apsFathersFirstName);
        db.SetNullableString(command, "apsFathersMi", apsFathersMi);
        db.SetNullableString(command, "apsFathersLastN", apsFathersLastName);
        db.SetNullableString(command, "apsMothersFirst", apsMothersFirstName);
        db.SetNullableString(command, "apsMothersMi", apsFathersMi);
        db.SetNullableString(command, "apsMothersMaiden", apsFathersLastName);
        db.SetNullableString(command, "cpSsn", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "sendRequestTo", "");
      });

    entities.ExistingFplsLocateRequest.CspNumber = cspNumber;
    entities.ExistingFplsLocateRequest.Identifier = identifier;
    entities.ExistingFplsLocateRequest.TransactionStatus = transactionStatus;
    entities.ExistingFplsLocateRequest.StateAbbr = stateAbbr;
    entities.ExistingFplsLocateRequest.StationNumber = stationNumber;
    entities.ExistingFplsLocateRequest.TransactionType = transactionType;
    entities.ExistingFplsLocateRequest.Ssn = ssn;
    entities.ExistingFplsLocateRequest.CaseId = caseId;
    entities.ExistingFplsLocateRequest.LocalCode = "";
    entities.ExistingFplsLocateRequest.UsersField = usersField;
    entities.ExistingFplsLocateRequest.TypeOfCase = typeOfCase;
    entities.ExistingFplsLocateRequest.ApFirstName = apFirstName;
    entities.ExistingFplsLocateRequest.ApMiddleName = apMiddleName;
    entities.ExistingFplsLocateRequest.ApFirstLastName = apFirstLastName;
    entities.ExistingFplsLocateRequest.ApSecondLastName = apSecondLastName;
    entities.ExistingFplsLocateRequest.ApThirdLastName = apThirdLastName;
    entities.ExistingFplsLocateRequest.ApDateOfBirth = apDateOfBirth;
    entities.ExistingFplsLocateRequest.Sex = sex;
    entities.ExistingFplsLocateRequest.CollectAllResponsesTogether =
      collectAllResponsesTogether;
    entities.ExistingFplsLocateRequest.TransactionError = "";
    entities.ExistingFplsLocateRequest.ApCityOfBirth = apCityOfBirth;
    entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth =
      apStateOrCountryOfBirth;
    entities.ExistingFplsLocateRequest.ApsFathersFirstName =
      apsFathersFirstName;
    entities.ExistingFplsLocateRequest.ApsFathersMi = apsFathersMi;
    entities.ExistingFplsLocateRequest.ApsFathersLastName = apsFathersLastName;
    entities.ExistingFplsLocateRequest.ApsMothersFirstName =
      apsMothersFirstName;
    entities.ExistingFplsLocateRequest.ApsMothersMi = apsFathersMi;
    entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
      apsFathersLastName;
    entities.ExistingFplsLocateRequest.CpSsn = "";
    entities.ExistingFplsLocateRequest.CreatedBy = createdBy;
    entities.ExistingFplsLocateRequest.CreatedTimestamp = createdTimestamp;
    entities.ExistingFplsLocateRequest.LastUpdatedBy = createdBy;
    entities.ExistingFplsLocateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.ExistingFplsLocateRequest.RequestSentDate = requestSentDate;
    entities.ExistingFplsLocateRequest.SendRequestTo = "";
    entities.ExistingFplsLocateRequest.Populated = true;
  }

  private bool ReadCaseCaseRole()
  {
    entities.ExistingCase.Populated = false;
    entities.ExistingAp.Populated = false;

    return Read("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCase.ManagementRegion =
          db.GetNullableString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 1);
        entities.ExistingAp.CasNumber = db.GetString(reader, 1);
        entities.ExistingCase.StationId = db.GetNullableString(reader, 2);
        entities.ExistingCase.Status = db.GetNullableString(reader, 3);
        entities.ExistingCase.KsFipsCode = db.GetNullableString(reader, 4);
        entities.ExistingAp.CspNumber = db.GetString(reader, 5);
        entities.ExistingAp.Type1 = db.GetString(reader, 6);
        entities.ExistingAp.Identifier = db.GetInt32(reader, 7);
        entities.ExistingAp.StartDate = db.GetNullableDate(reader, 8);
        entities.ExistingAp.EndDate = db.GetNullableDate(reader, 9);
        entities.ExistingAp.MothersFirstName = db.GetNullableString(reader, 10);
        entities.ExistingAp.MothersMiddleInitial =
          db.GetNullableString(reader, 11);
        entities.ExistingAp.FathersLastName = db.GetNullableString(reader, 12);
        entities.ExistingAp.FathersMiddleInitial =
          db.GetNullableString(reader, 13);
        entities.ExistingAp.FathersFirstName = db.GetNullableString(reader, 14);
        entities.ExistingAp.MothersMaidenLastName =
          db.GetNullableString(reader, 15);
        entities.ExistingCase.Populated = true;
        entities.ExistingAp.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAp.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.BirthPlaceState =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadFplsLocateRequest1()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(
          command, "requestSentDate", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.StateAbbr =
          db.GetNullableString(reader, 3);
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.ApFirstName =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.ApMiddleName =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.ApFirstLastName =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateRequest.ApSecondLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateRequest.ApThirdLastName =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateRequest.ApDateOfBirth =
          db.GetNullableDate(reader, 16);
        entities.ExistingFplsLocateRequest.Sex =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateRequest.CollectAllResponsesTogether =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateRequest.ApCityOfBirth =
          db.GetNullableString(reader, 20);
        entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateRequest.ApsFathersFirstName =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateRequest.ApsFathersMi =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateRequest.ApsFathersLastName =
          db.GetNullableString(reader, 24);
        entities.ExistingFplsLocateRequest.ApsMothersFirstName =
          db.GetNullableString(reader, 25);
        entities.ExistingFplsLocateRequest.ApsMothersMi =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
          db.GetNullableString(reader, 27);
        entities.ExistingFplsLocateRequest.CpSsn =
          db.GetNullableString(reader, 28);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 29);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 30);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 31);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 32);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest2()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.StateAbbr =
          db.GetNullableString(reader, 3);
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.ApFirstName =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.ApMiddleName =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.ApFirstLastName =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateRequest.ApSecondLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateRequest.ApThirdLastName =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateRequest.ApDateOfBirth =
          db.GetNullableDate(reader, 16);
        entities.ExistingFplsLocateRequest.Sex =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateRequest.CollectAllResponsesTogether =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateRequest.ApCityOfBirth =
          db.GetNullableString(reader, 20);
        entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateRequest.ApsFathersFirstName =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateRequest.ApsFathersMi =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateRequest.ApsFathersLastName =
          db.GetNullableString(reader, 24);
        entities.ExistingFplsLocateRequest.ApsMothersFirstName =
          db.GetNullableString(reader, 25);
        entities.ExistingFplsLocateRequest.ApsMothersMi =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
          db.GetNullableString(reader, 27);
        entities.ExistingFplsLocateRequest.CpSsn =
          db.GetNullableString(reader, 28);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 29);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 30);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 31);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 32);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateRequest.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of BatchRequestIndicator.
    /// </summary>
    [JsonPropertyName("batchRequestIndicator")]
    public Common BatchRequestIndicator
    {
      get => batchRequestIndicator ??= new();
      set => batchRequestIndicator = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private Common batchRequestIndicator;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
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
    /// A value of ExternalFplsHeader.
    /// </summary>
    [JsonPropertyName("externalFplsHeader")]
    public ExternalFplsHeader ExternalFplsHeader
    {
      get => externalFplsHeader ??= new();
      set => externalFplsHeader = value;
    }

    private FplsLocateRequest fplsLocateRequest;
    private ExternalFplsRequest externalFplsRequest;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ExternalFplsHeader externalFplsHeader;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AliasGroup group.</summary>
    [Serializable]
    public class AliasGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Gkscarea.
      /// </summary>
      [JsonPropertyName("gkscarea")]
      public Common Gkscarea
      {
        get => gkscarea ??= new();
        set => gkscarea = value;
      }

      /// <summary>
      /// A value of Gkanpay.
      /// </summary>
      [JsonPropertyName("gkanpay")]
      public Common Gkanpay
      {
        get => gkanpay ??= new();
        set => gkanpay = value;
      }

      /// <summary>
      /// A value of Gcse.
      /// </summary>
      [JsonPropertyName("gcse")]
      public Common Gcse
      {
        get => gcse ??= new();
        set => gcse = value;
      }

      /// <summary>
      /// A value of Gae.
      /// </summary>
      [JsonPropertyName("gae")]
      public Common Gae
      {
        get => gae ??= new();
        set => gae = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet g;
      private Common gkscarea;
      private Common gkanpay;
      private Common gcse;
      private Common gae;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public WorkArea Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of FplsRequestFound.
    /// </summary>
    [JsonPropertyName("fplsRequestFound")]
    public Common FplsRequestFound
    {
      get => fplsRequestFound ??= new();
      set => fplsRequestFound = value;
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

    /// <summary>
    /// A value of SesaOnline.
    /// </summary>
    [JsonPropertyName("sesaOnline")]
    public WorkArea SesaOnline
    {
      get => sesaOnline ??= new();
      set => sesaOnline = value;
    }

    /// <summary>
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
    }

    /// <summary>
    /// A value of AdcNadcFlag.
    /// </summary>
    [JsonPropertyName("adcNadcFlag")]
    public Common AdcNadcFlag
    {
      get => adcNadcFlag ??= new();
      set => adcNadcFlag = value;
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
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// Gets a value of Alias.
    /// </summary>
    [JsonIgnore]
    public Array<AliasGroup> Alias => alias ??= new(AliasGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Alias for json serialization.
    /// </summary>
    [JsonPropertyName("alias")]
    [Computed]
    public IList<AliasGroup> Alias_Json
    {
      get => alias;
      set => Alias.Assign(value);
    }

    private WorkArea work;
    private Common fplsRequestFound;
    private DateWorkArea nullDate;
    private WorkArea sesaOnline;
    private ExternalFplsRequest externalFplsRequest;
    private Common adcNadcFlag;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FplsLocateRequest fplsLocateRequest;
    private Array<AliasGroup> alias;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CaseRole ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
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

    private Case1 existingCase;
    private FplsLocateRequest existingFplsLocateRequest;
    private CaseRole existingAp;
    private CsePerson existingCsePerson;
  }
#endregion
}
