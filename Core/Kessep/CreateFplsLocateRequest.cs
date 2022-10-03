// Program: CREATE_FPLS_LOCATE_REQUEST, ID: 372355015, model: 746.
// Short name: SWE00142
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
/// A program: CREATE_FPLS_LOCATE_REQUEST.
/// </para>
/// <para>
/// Resp: OBLGEST
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
public partial class CreateFplsLocateRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_FPLS_LOCATE_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateFplsLocateRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateFplsLocateRequest.
  /// </summary>
  public CreateFplsLocateRequest(IContext context, Import import, Export export):
    
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
    // 06/05/2009   DDupree     Added check when updating or adding a fpls
    // locate request for the ssn being used for he request against the invalid
    // ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    local.Sesa.Text32 = "";
    local.FplsLocateRequest.ZdelCreatUserId = global.UserId;
    local.FplsLocateRequest.RequestSentDate = null;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!IsEmpty(import.Group.Item.Det.State))
      {
        if (Equal(import.Group.Item.Det.State, "KS"))
        {
          ExitState = "OE0142_KS_IS_NOT_ENTERABLE";

          return;
        }

        local.Sesa.Text32 = TrimEnd(local.Sesa.Text32) + "B" + import
          .Group.Item.Det.State;
      }
    }

    // ************************************************
    // *Check that the "Send to FPLS" directive is ok.*
    // ************************************************
    local.SendDirective.UsersField = import.SendDirective.UsersField;

    if (!Equal(import.SendDirective.UsersField, "N") && !
      Equal(import.SendDirective.UsersField, "Y"))
    {
      local.SendDirective.UsersField = "Y";
    }

    if (IsEmpty(local.Sesa.Text32))
    {
      local.SendDirective.UsersField = "Y";
    }

    // ************************************************
    // *Get currency on the CSE Person.               *
    // ************************************************
    if (ReadCsePerson())
    {
      export.CsePersonsWorkSet.Number = import.CsePerson.Number;
      UseSiReadCsePerson();
      local.Convert.SsnNum9 = (int)StringToNumber(local.CsePersonsWorkSet.Ssn);

      // added this check as part of cq7189.
      if (ReadInvalidSsn())
      {
        ExitState = "INVALID_SSN";

        return;
      }
      else
      {
        // this is fine, there is not invalid ssn record for this combination of
        // cse person number and ssn number
      }

      if (ReadFplsLocateRequest())
      {
        if (AsChar(entities.ExistingFplsLocateRequest.TransactionStatus) == 'C')
        {
          try
          {
            UpdateFplsLocateRequest();
            MoveFplsLocateRequest(entities.ExistingFplsLocateRequest,
              export.FplsLocateRequest);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ExitState = "OE0000_FPLS_ALREADY_REQUESTED";

          return;
        }
      }

      // ***	Sex is a required field. Default to "M". ***
      if (IsEmpty(local.CsePersonsWorkSet.Sex))
      {
        local.CsePersonsWorkSet.Sex = "M";
      }

      // ***	Add Alias fields here. ***
      UseEabReadAlias();

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
            local.FplsLocateRequest.ApSecondLastName =
              local.Alias.Item.G.LastName;

            break;
          default:
            goto AfterCycle;
        }
      }

AfterCycle:

      local.Alias.CheckIndex();
      local.AdcNadcFlag.SelectChar = "X";
      UseOeDetermineAdcPgmParticipatn();

      if (AsChar(local.AdcNadcFlag.SelectChar) == 'Y')
      {
        local.AdcNadcFlag.SelectChar = "A";
      }
      else
      {
        local.AdcNadcFlag.SelectChar = "N";
      }

      local.FplsLocateRequest.Identifier =
        entities.ExistingFplsLocateRequest.Identifier + 1;
      local.Text.Text3 = NumberToString(local.FplsLocateRequest.Identifier, 3);
      local.FplsLocateRequest.CaseId = entities.ExistingCsePerson.Number + local
        .Text.Text3;

      // ************************************************
      // *Create a new FPLS Request.                    *
      // ************************************************
      try
      {
        CreateFplsLocateRequest1();
        MoveFplsLocateRequest(entities.ExistingFplsLocateRequest,
          export.FplsLocateRequest);
        ExitState = "OE0000_FPLS_REQUEST_CREATE_OK";
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
    else
    {
      ExitState = "CSE_PERSON_NF";
    }
  }

  private static void MoveAlias(Local.AliasGroup source,
    EabReadAlias.Export.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscarea.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases(EabReadAlias.Export.AliasesGroup source,
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
    target.ZdelReqCreatDt = source.ZdelReqCreatDt;
    target.ZdelCreatUserId = source.ZdelCreatUserId;
    target.RequestSentDate = source.RequestSentDate;
    target.StateAbbr = source.StateAbbr;
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

  private void UseEabReadAlias()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    local.Alias.CopyTo(useExport.Aliases, MoveAlias);

    Call(EabReadAlias.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Alias, MoveAliases);
  }

  private void UseOeDetermineAdcPgmParticipatn()
  {
    var useImport = new OeDetermineAdcPgmParticipatn.Import();
    var useExport = new OeDetermineAdcPgmParticipatn.Export();

    useImport.Ap.Number = import.CsePerson.Number;

    Call(OeDetermineAdcPgmParticipatn.Execute, useImport, useExport);

    local.AdcNadcFlag.SelectChar = useExport.CaseInAdcProgram.SelectChar;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreateFplsLocateRequest1()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var identifier = local.FplsLocateRequest.Identifier;
    var transactionStatus = "C";
    var zdelReqCreatDt = Now().Date;
    var zdelCreatUserId = local.FplsLocateRequest.ZdelCreatUserId;
    var stateAbbr = "KS";
    var stationNumber = "02";
    var transactionType = "A";
    var ssn = local.CsePersonsWorkSet.Ssn;
    var caseId = local.FplsLocateRequest.CaseId ?? "";
    var localCode = entities.ExistingCase.ManagementRegion;
    var usersField = local.SendDirective.UsersField ?? "";
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
    var apsFathersFirstName = local.CaseRole.FathersFirstName ?? "";
    var apsFathersMi = local.CaseRole.FathersMiddleInitial ?? "";
    var apsFathersLastName = Substring(local.CaseRole.FathersLastName, 1, 16);
    var apsMothersFirstName = local.CaseRole.MothersFirstName ?? "";
    var apsMothersMi = local.CaseRole.MothersMiddleInitial ?? "";
    var apsMothersMaidenName =
      Substring(local.CaseRole.MothersMaidenLastName, 1, 16);
    var createdTimestamp = Now();
    var requestSentDate = local.FplsLocateRequest.RequestSentDate;
    var sendRequestTo = local.Sesa.Text32;

    entities.ExistingFplsLocateRequest.Populated = false;
    Update("CreateFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetNullableDate(command, "zdelReqCreatDt", zdelReqCreatDt);
        db.SetString(command, "zdelCreatUserId", zdelCreatUserId);
        db.SetNullableString(command, "stateAbbr", stateAbbr);
        db.SetNullableString(command, "stationNumber", stationNumber);
        db.SetNullableString(command, "transactionType", transactionType);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "caseId", caseId);
        db.SetNullableString(command, "localCode", localCode);
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
        db.SetNullableString(command, "apsMothersMi", apsMothersMi);
        db.SetNullableString(command, "apsMothersMaiden", apsMothersMaidenName);
        db.SetNullableString(command, "cpSsn", "");
        db.SetString(command, "createdBy", zdelCreatUserId);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", zdelCreatUserId);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
      });

    entities.ExistingFplsLocateRequest.CspNumber = cspNumber;
    entities.ExistingFplsLocateRequest.Identifier = identifier;
    entities.ExistingFplsLocateRequest.TransactionStatus = transactionStatus;
    entities.ExistingFplsLocateRequest.ZdelReqCreatDt = zdelReqCreatDt;
    entities.ExistingFplsLocateRequest.ZdelCreatUserId = zdelCreatUserId;
    entities.ExistingFplsLocateRequest.StateAbbr = stateAbbr;
    entities.ExistingFplsLocateRequest.StationNumber = stationNumber;
    entities.ExistingFplsLocateRequest.TransactionType = transactionType;
    entities.ExistingFplsLocateRequest.Ssn = ssn;
    entities.ExistingFplsLocateRequest.CaseId = caseId;
    entities.ExistingFplsLocateRequest.LocalCode = localCode;
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
    entities.ExistingFplsLocateRequest.ApsMothersMi = apsMothersMi;
    entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
      apsMothersMaidenName;
    entities.ExistingFplsLocateRequest.CpSsn = "";
    entities.ExistingFplsLocateRequest.CreatedBy = zdelCreatUserId;
    entities.ExistingFplsLocateRequest.CreatedTimestamp = createdTimestamp;
    entities.ExistingFplsLocateRequest.LastUpdatedBy = zdelCreatUserId;
    entities.ExistingFplsLocateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.ExistingFplsLocateRequest.RequestSentDate = requestSentDate;
    entities.ExistingFplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.ExistingFplsLocateRequest.Populated = true;
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

  private bool ReadFplsLocateRequest()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
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
        entities.ExistingFplsLocateRequest.ZdelReqCreatDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateRequest.ZdelCreatUserId =
          db.GetString(reader, 4);
        entities.ExistingFplsLocateRequest.StateAbbr =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.ApFirstName =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateRequest.ApMiddleName =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateRequest.ApFirstLastName =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateRequest.ApSecondLastName =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateRequest.ApThirdLastName =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateRequest.ApDateOfBirth =
          db.GetNullableDate(reader, 18);
        entities.ExistingFplsLocateRequest.Sex =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateRequest.CollectAllResponsesTogether =
          db.GetNullableString(reader, 20);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateRequest.ApCityOfBirth =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateRequest.ApsFathersFirstName =
          db.GetNullableString(reader, 24);
        entities.ExistingFplsLocateRequest.ApsFathersMi =
          db.GetNullableString(reader, 25);
        entities.ExistingFplsLocateRequest.ApsFathersLastName =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateRequest.ApsMothersFirstName =
          db.GetNullableString(reader, 27);
        entities.ExistingFplsLocateRequest.ApsMothersMi =
          db.GetNullableString(reader, 28);
        entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
          db.GetNullableString(reader, 29);
        entities.ExistingFplsLocateRequest.CpSsn =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 31);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 32);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 33);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 34);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 36);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);

    var usersField = local.SendDirective.UsersField ?? "";
    var lastUpdatedBy = local.FplsLocateRequest.ZdelCreatUserId;
    var lastUpdatedTimestamp = Now();
    var requestSentDate = local.FplsLocateRequest.RequestSentDate;
    var sendRequestTo = local.Sesa.Text32;

    entities.ExistingFplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "usersField", usersField);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingFplsLocateRequest.Identifier);
          
      });

    entities.ExistingFplsLocateRequest.UsersField = usersField;
    entities.ExistingFplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFplsLocateRequest.RequestSentDate = requestSentDate;
    entities.ExistingFplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.ExistingFplsLocateRequest.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      public OblgWork Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork det;
    }

    /// <summary>
    /// A value of SendDirective.
    /// </summary>
    [JsonPropertyName("sendDirective")]
    public FplsLocateRequest SendDirective
    {
      get => sendDirective ??= new();
      set => sendDirective = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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

    private FplsLocateRequest sendDirective;
    private Common batchRequestIndicator;
    private Array<GroupGroup> group;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private FplsLocateRequest fplsLocateRequest;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public WorkArea Text
    {
      get => text ??= new();
      set => text = value;
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

    /// <summary>
    /// A value of Sesa.
    /// </summary>
    [JsonPropertyName("sesa")]
    public WorkArea Sesa
    {
      get => sesa ??= new();
      set => sesa = value;
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
    /// A value of SendDirective.
    /// </summary>
    [JsonPropertyName("sendDirective")]
    public FplsLocateRequest SendDirective
    {
      get => sendDirective ??= new();
      set => sendDirective = value;
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
    /// A value of SesaBatch.
    /// </summary>
    [JsonPropertyName("sesaBatch")]
    public WorkArea SesaBatch
    {
      get => sesaBatch ??= new();
      set => sesaBatch = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ErrorInDecoding.
    /// </summary>
    [JsonPropertyName("errorInDecoding")]
    public Common ErrorInDecoding
    {
      get => errorInDecoding ??= new();
      set => errorInDecoding = value;
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

    private SsnWorkArea convert;
    private WorkArea text;
    private Array<AliasGroup> alias;
    private WorkArea sesa;
    private ExternalFplsRequest externalFplsRequest;
    private FplsLocateRequest sendDirective;
    private Common adcNadcFlag;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FplsLocateRequest fplsLocateRequest;
    private WorkArea sesaBatch;
    private Code code;
    private CodeValue codeValue;
    private Common errorInDecoding;
    private CaseRole caseRole;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
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

    private InvalidSsn invalidSsn;
    private Case1 existingCase;
    private FplsLocateRequest existingFplsLocateRequest;
    private CaseRole existingCaseRole;
    private CsePerson existingCsePerson;
  }
#endregion
}
