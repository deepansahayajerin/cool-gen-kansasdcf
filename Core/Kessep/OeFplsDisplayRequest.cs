// Program: OE_FPLS_DISPLAY_REQUEST, ID: 372355024, model: 746.
// Short name: SWE00909
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
/// A program: OE_FPLS_DISPLAY_REQUEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block is called by PROCESS FPLS REQUEST TRANSACTION Procedures.
/// It Reads the FLPS_REQUEST and FPLS_RESPONSE entities and reacts to
/// DISPLAY, NEXT, and PREV commands.
/// </para>
/// </summary>
[Serializable]
public partial class OeFplsDisplayRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPLS_DISPLAY_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFplsDisplayRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFplsDisplayRequest.
  /// </summary>
  public OeFplsDisplayRequest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // unknown   	MM/DD/YY	Initial Code
    // T.O.Redmond	05/29/96 ID-130 Add New FPLS attributes.
    // MK              09/14/98
    // Changed Exit State and Escape as noted below.
    // ********* END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    MoveFplsLocateRequest1(import.FplsLocateRequest, export.FplsLocateRequest);
    export.FplsLocateResponse.Assign(import.FplsLocateResponse);
    export.CsePerson.Number = import.CsePerson.Number;
    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";

    if (ReadCsePerson())
    {
      UseCabGetClientDetails();

      if (!Lt("000000000", export.CsePersonsWorkSet.Ssn))
      {
        export.CsePersonsWorkSet.Ssn = "";
      }
    }
    else
    {
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.RequestFound.Flag = "N";
    local.ResponseFound.Flag = "N";
    local.FplsLocateResponse.Identifier = import.FplsLocateResponse.Identifier;

    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        if (ReadFplsLocateRequest8())
        {
          local.RequestFound.Flag = "Y";
          MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
            export.FplsLocateRequest);

          if (AsChar(local.RequestFound.Flag) == 'Y')
          {
            // ---------------------------------------------
            // Get the latest response or the specified response for this 
            // request
            // ---------------------------------------------
            if (ReadFplsLocateResponse4())
            {
              local.ResponseFound.Flag = "Y";
              export.FplsLocateResponse.Assign(
                entities.ExistingFplsLocateResponse);
            }
          }
        }

        // *********************************
        // Changed EXIT STATE from no more request
        // to request nf.  Changed escape to exit CAB.
        // This change was requested to display proper message
        // when no request records exist for the CSE person.
        // MK 9/14/98
        // *********************************
        if (AsChar(local.RequestFound.Flag) == 'N')
        {
          ExitState = "OE0000_FPLS_REQUEST_NF";

          return;
        }

        if (AsChar(local.ResponseFound.Flag) == 'N')
        {
          export.FplsLocateResponse.Identifier = 0;
          ExitState = "OE0104_NO_RESPONSE_RECEIVED";

          if (Equal(export.FplsLocateRequest.RequestSentDate,
            local.NullDate.Date))
          {
            ExitState = "REQUEST_HAS_NOT_YET_BEEN_SENT";
          }
        }

        break;
      case "NEXT":
        local.RequestFound.Flag = "N";
        local.ResponseFound.Flag = "N";

        if (ReadFplsLocateRequest1())
        {
          // ************************************************
          // *We have found some request for this CSE Person*
          // ************************************************
          local.RequestFound.Flag = "Y";
          MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
            export.FplsLocateRequest);

          // ************************************************
          // *Find the next higher Response for this request*
          // ************************************************
          if (ReadFplsLocateResponse2())
          {
            local.ResponseFound.Flag = "Y";
            export.FplsLocateResponse.
              Assign(entities.ExistingFplsLocateResponse);
          }

          if (AsChar(local.ResponseFound.Flag) == 'N')
          {
            // ************************************************
            // *There are no more responses for the current   *
            // *request. Determine if there is another Request*
            // *that has a higher number than the current one *
            // ************************************************
            local.RequestFound.Flag = "N";

            if (ReadFplsLocateRequest5())
            {
              local.RequestFound.Flag = "Y";
              MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
                export.FplsLocateRequest);

              // ************************************************
              // *Look for the earliest response to this Next   *
              // *Request.
              // 
              // *
              // ************************************************
              if (ReadFplsLocateResponse3())
              {
                local.ResponseFound.Flag = "Y";
                export.FplsLocateResponse.Assign(
                  entities.ExistingFplsLocateResponse);

                goto Read;
              }

              export.FplsLocateResponse.Assign(
                entities.ExistingFplsLocateResponse);

              goto Read;
            }
          }
        }
        else
        {
          // ************************************************
          // *There are no requests for this CSE Person     *
          // ************************************************
          ExitState = "OE0000_FPLS_REQUEST_NF";

          break;
        }

Read:

        if (AsChar(local.RequestFound.Flag) == 'N')
        {
          ExitState = "OE0097_NO_MORE_REQUEST";

          break;
        }

        if (AsChar(local.ResponseFound.Flag) == 'N')
        {
          export.FplsLocateResponse.Assign(entities.ExistingFplsLocateResponse);
          export.FplsLocateResponse.Identifier = 0;
          ExitState = "OE0104_NO_RESPONSE_RECEIVED";

          if (Equal(export.FplsLocateRequest.RequestSentDate,
            local.NullDate.Date))
          {
            ExitState = "REQUEST_HAS_NOT_YET_BEEN_SENT";
          }
        }

        break;
      case "PREV":
        local.RequestFound.Flag = "N";
        local.ResponseFound.Flag = "N";

        if (ReadFplsLocateRequest2())
        {
          local.RequestFound.Flag = "Y";
          MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
            export.FplsLocateRequest);

          // ---------------------------------------------
          // Read the earliest response for the given request.
          // ---------------------------------------------
          if (ReadFplsLocateResponse5())
          {
            local.ResponseFound.Flag = "Y";
            export.FplsLocateResponse.
              Assign(entities.ExistingFplsLocateResponse);
          }

          if (AsChar(local.ResponseFound.Flag) == 'N')
          {
            // ************************************************
            // *No more responses exist for the current request
            // *Try to read a request which is earlier than   *
            // *than the current request,then read its related*
            // *response.
            // 
            // *
            // ************************************************
            local.RequestFound.Flag = "N";

            if (ReadFplsLocateRequest7())
            {
              local.RequestFound.Flag = "Y";
              MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
                export.FplsLocateRequest);

              if (AsChar(local.RequestFound.Flag) == 'Y')
              {
                if (ReadFplsLocateResponse6())
                {
                  local.ResponseFound.Flag = "Y";
                  export.FplsLocateResponse.Assign(
                    entities.ExistingFplsLocateResponse);

                  goto Test;
                }

                export.FplsLocateResponse.Assign(
                  entities.ExistingFplsLocateResponse);
              }

              goto Test;
            }
          }

Test:

          // ************************************************
          // *Test to see if there was a prior request found*
          // *and if not then show the oldest request along *
          // *with the oldest response for that request.    *
          // ************************************************
          if (AsChar(local.RequestFound.Flag) == 'N')
          {
            local.ResponseFound.Flag = "N";

            // ************************************************
            // *No more prior requests exist for this CSE     *
            // *Person. Display the oldest request and its    *
            // *related response, and a message which states  *
            // *that there are no prior requests.             *
            // ************************************************
            if (ReadFplsLocateRequest6())
            {
              local.RequestFound.Flag = "Y";
              MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
                export.FplsLocateRequest);

              if (ReadFplsLocateResponse1())
              {
                local.ResponseFound.Flag = "Y";
                export.FplsLocateResponse.Assign(
                  entities.ExistingFplsLocateResponse);

                break;
              }

              export.FplsLocateResponse.Assign(
                entities.ExistingFplsLocateResponse);

              break;
            }

            ExitState = "OE0097_NO_MORE_REQUEST";
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
    }

    // ************************************************
    // *Now we must convert response codes into English
    // *Descriptions and set scrolling attributes for *
    // *Next and Prior.
    // 
    // *
    // ************************************************
    MoveFplsLocateRequest2(entities.ExistingFplsLocateRequest,
      export.FplsLocateRequest);
    export.FplsLocateResponse.Assign(entities.ExistingFplsLocateResponse);

    if (AsChar(local.RequestFound.Flag) == 'Y')
    {
      UseOeFplsSesaStateSubstring();

      if (!IsEmpty(entities.ExistingFplsLocateRequest.TransactionError))
      {
        local.Code.CodeName = "FPLS ERROR";
        local.CodeValue.Cdvalue =
          entities.ExistingFplsLocateRequest.TransactionError ?? Spaces(10);
        UseCabValidateCodeValue1();

        if (AsChar(local.InvalidCode.Flag) != 'Y')
        {
          export.TransactionError.Description = "Invalid Code: " + entities
            .ExistingFplsLocateRequest.TransactionError;
        }
      }
    }

    if (AsChar(local.RequestFound.Flag) == 'N')
    {
      ExitState = "OE0097_NO_MORE_REQUEST";
    }
    else if (AsChar(local.ResponseFound.Flag) == 'N')
    {
      export.FplsLocateResponse.Assign(entities.ExistingFplsLocateResponse);
      export.FplsLocateResponse.Identifier = 0;
      ExitState = "OE0104_NO_RESPONSE_RECEIVED";

      if (Equal(export.FplsLocateRequest.RequestSentDate, local.NullDate.Date))
      {
        ExitState = "REQUEST_HAS_NOT_YET_BEEN_SENT";
      }
    }

    if (AsChar(local.ResponseFound.Flag) != 'Y')
    {
    }
    else
    {
      // ************************************************
      // *Unstring the Address that is returned by FPLS *
      // ************************************************
      UseOeFplsAddressSubstring();

      if (!IsEmpty(entities.ExistingFplsLocateResponse.AgencyCode))
      {
        local.Code.CodeName = "FPLS AGENCY";
        local.CodeValue.Cdvalue =
          entities.ExistingFplsLocateResponse.AgencyCode ?? Spaces(10);
        local.InvalidCode.Flag = "";
        UseCabGetCodeValueDescription1();

        if (AsChar(local.InvalidCode.Flag) == 'Y')
        {
          export.FplsAgency.Description = "Invalid Code: " + entities
            .ExistingFplsLocateResponse.AgencyCode;
        }
        else if (Equal(entities.ExistingFplsLocateResponse.AgencyCode, "B01"))
        {
          export.FplsAgency.Description = "Federal agencies";
        }
      }

      if (!IsEmpty(entities.ExistingFplsLocateResponse.ResponseCode))
      {
        local.Code.CodeName = "FPLS RESPONSE";
        local.CodeValue.Cdvalue =
          entities.ExistingFplsLocateResponse.ResponseCode ?? Spaces(10);
        UseCabGetCodeValueDescription2();

        if (AsChar(local.InvalidCode.Flag) == 'Y')
        {
          export.FplsResp.Description = "Invalid Code: " + entities
            .ExistingFplsLocateResponse.ResponseCode;
        }
      }

      if (!IsEmpty(entities.ExistingFplsLocateResponse.DodStatus))
      {
        local.Code.CodeName = "FPLS DOD STATUS";
        local.CodeValue.Cdvalue =
          entities.ExistingFplsLocateResponse.DodStatus ?? Spaces(10);
        local.InvalidCode.Flag = "";
        UseCabValidateCodeValue3();

        if (AsChar(local.InvalidCode.Flag) == 'Y')
        {
          export.DodStatusDesc.Description = "Invalid Code: " + entities
            .ExistingFplsLocateResponse.DodStatus;
        }
      }

      // FPLS Department of Defense service codes
      if (!IsEmpty(entities.ExistingFplsLocateResponse.DodServiceCode))
      {
        local.Code.CodeName = "FPLS SERVICE";
        local.CodeValue.Cdvalue =
          entities.ExistingFplsLocateResponse.DodServiceCode ?? Spaces(10);
        local.InvalidCode.Flag = "";
        UseCabValidateCodeValue4();

        if (AsChar(local.InvalidCode.Flag) == 'Y')
        {
          export.DodServiceDesc.Description = "Invalid Code: " + entities
            .ExistingFplsLocateResponse.DodServiceCode;
        }
      }

      if (!IsEmpty(entities.ExistingFplsLocateResponse.DodPayGradeCode))
      {
        local.Code.CodeName = "FPLS DOD PAY GRADE";
        local.CodeValue.Cdvalue =
          entities.ExistingFplsLocateResponse.DodPayGradeCode ?? Spaces(10);
        local.InvalidCode.Flag = "";
        UseCabValidateCodeValue5();

        if (AsChar(local.InvalidCode.Flag) == 'Y')
        {
          export.DodPayGradeDesc.Description = "Invalid Code: " + entities
            .ExistingFplsLocateResponse.DodPayGradeCode;
        }
      }

      // ************************************************
      // *Find the code table value for the Responding  *
      // *State Code.
      // 
      // *
      // ************************************************
      if (!IsEmpty(entities.ExistingFplsLocateResponse.SesaRespondingState))
      {
        local.Code.CodeName = "FPLS AGENCY";
        local.CodeValue.Cdvalue = "B" + entities
          .ExistingFplsLocateResponse.SesaRespondingState;
        local.InvalidCode.Flag = "";
        UseCabValidateCodeValue2();

        if (AsChar(local.InvalidCode.Flag) == 'N')
        {
          export.SesaRespondingState.Description = "Invalid Code: " + entities
            .ExistingFplsLocateResponse.SesaRespondingState;
        }
      }
    }

    // *********************************************
    // Set scrolling attributes
    // *********************************************
    if (ReadFplsLocateRequest9())
    {
      local.FplsLocateRequest.Identifier =
        entities.ExistingPrevNextFplsLocateRequest.Identifier;

      if (ReadFplsLocateResponse7())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }
      else if (ReadFplsLocateRequest3())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }

      foreach(var item in ReadFplsLocateRequest10())
      {
      }

      if (ReadFplsLocateResponse8())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }
      else if (ReadFplsLocateRequest4())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }
    }
  }

  private static void MoveFplsAddress(OeFplsAddressSubstring.Export.
    FplsAddressGroup source, Export.FplsAddrGroup target)
  {
    target.FplsAddrGroupDet.Text39 = source.FplsAddr.Text39;
  }

  private static void MoveFplsLocateRequest1(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
  }

  private static void MoveFplsLocateRequest2(FplsLocateRequest source,
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

  private static void MoveFplsLocateResponse(FplsLocateResponse source,
    FplsLocateResponse target)
  {
    target.AgencyCode = source.AgencyCode;
    target.AddressFormatInd = source.AddressFormatInd;
    target.ReturnedAddress = source.ReturnedAddress;
  }

  private static void MoveSesa(OeFplsSesaStateSubstring.Export.SesaGroup source,
    Export.SesaGroup target)
  {
    target.SesaGroupDet.State = source.Sesa1.State;
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabGetCodeValueDescription1()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ErrorInDecoding.Flag;
    export.FplsAgency.Description = useExport.CodeValue.Description;
  }

  private void UseCabGetCodeValueDescription2()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ErrorInDecoding.Flag;
    export.FplsResp.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ValidCode.Flag;
    export.TransactionError.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ValidCode.Flag;
    export.SesaRespondingState.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ValidCode.Flag;
    export.DodStatusDesc.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue4()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ValidCode.Flag;
    export.DodServiceDesc.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue5()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.InvalidCode.Flag = useExport.ValidCode.Flag;
    export.DodPayGradeDesc.Description = useExport.CodeValue.Description;
  }

  private void UseOeFplsAddressSubstring()
  {
    var useImport = new OeFplsAddressSubstring.Import();
    var useExport = new OeFplsAddressSubstring.Export();

    MoveFplsLocateResponse(entities.ExistingFplsLocateResponse,
      useImport.FplsLocateResponse);

    Call(OeFplsAddressSubstring.Execute, useImport, useExport);

    useExport.FplsAddress.CopyTo(export.FplsAddr, MoveFplsAddress);
  }

  private void UseOeFplsSesaStateSubstring()
  {
    var useImport = new OeFplsSesaStateSubstring.Import();
    var useExport = new OeFplsSesaStateSubstring.Export();

    useImport.FplsLocateRequest.SendRequestTo =
      entities.ExistingFplsLocateRequest.SendRequestTo;

    Call(OeFplsSesaStateSubstring.Execute, useImport, useExport);

    useExport.Sesa.CopyTo(export.Sesa, MoveSesa);
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
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest1()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFplsLocateRequest10()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return ReadEach("ReadFplsLocateRequest10",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadFplsLocateRequest2()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest3()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", export.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest4()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", export.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest5()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest6()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest6",
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest7()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest7",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest8()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest8",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
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
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 13);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 17);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest9()
  {
    entities.ExistingPrevNextFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest9",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "identifier", export.FplsLocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevNextFplsLocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPrevNextFplsLocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPrevNextFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse1",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.ExistingFplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.ExistingFplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.ExistingFplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.ExistingFplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 25);
        entities.ExistingFplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.ExistingFplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingFplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 29);
        entities.ExistingFplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 31);
        entities.ExistingFplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 32);
        entities.ExistingFplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 33);
        entities.ExistingFplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 35);
        entities.ExistingFplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 36);
        entities.ExistingFplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 37);
        entities.ExistingFplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 38);
        entities.ExistingFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse2",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.
          SetInt32(command, "identifier", import.FplsLocateResponse.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.ExistingFplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.ExistingFplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.ExistingFplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.ExistingFplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 25);
        entities.ExistingFplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.ExistingFplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingFplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 29);
        entities.ExistingFplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 31);
        entities.ExistingFplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 32);
        entities.ExistingFplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 33);
        entities.ExistingFplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 35);
        entities.ExistingFplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 36);
        entities.ExistingFplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 37);
        entities.ExistingFplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 38);
        entities.ExistingFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse3()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse3",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.ExistingFplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.ExistingFplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.ExistingFplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.ExistingFplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 25);
        entities.ExistingFplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.ExistingFplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingFplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 29);
        entities.ExistingFplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 31);
        entities.ExistingFplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 32);
        entities.ExistingFplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 33);
        entities.ExistingFplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 35);
        entities.ExistingFplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 36);
        entities.ExistingFplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 37);
        entities.ExistingFplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 38);
        entities.ExistingFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse4()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse4",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.
          SetInt32(command, "identifier", import.FplsLocateResponse.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.ExistingFplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.ExistingFplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.ExistingFplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.ExistingFplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 25);
        entities.ExistingFplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.ExistingFplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingFplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 29);
        entities.ExistingFplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 31);
        entities.ExistingFplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 32);
        entities.ExistingFplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 33);
        entities.ExistingFplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 35);
        entities.ExistingFplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 36);
        entities.ExistingFplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 37);
        entities.ExistingFplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 38);
        entities.ExistingFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse5()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse5",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.
          SetInt32(command, "identifier", import.FplsLocateResponse.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.ExistingFplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.ExistingFplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.ExistingFplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.ExistingFplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 25);
        entities.ExistingFplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.ExistingFplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingFplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 29);
        entities.ExistingFplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 31);
        entities.ExistingFplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 32);
        entities.ExistingFplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 33);
        entities.ExistingFplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 35);
        entities.ExistingFplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 36);
        entities.ExistingFplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 37);
        entities.ExistingFplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 38);
        entities.ExistingFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse6()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse6",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFplsLocateResponse.CspNumber = db.GetString(reader, 1);
        entities.ExistingFplsLocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.ExistingFplsLocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.ExistingFplsLocateResponse.AgencyCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateResponse.NameSentInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateResponse.ApNameReturned =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateResponse.AddrDateFormatInd =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateResponse.DateOfAddress =
          db.GetNullableDate(reader, 10);
        entities.ExistingFplsLocateResponse.ResponseCode =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateResponse.AddressFormatInd =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateResponse.DodStatus =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateResponse.DodServiceCode =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateResponse.DodPayGradeCode =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateResponse.SesaRespondingState =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateResponse.SesaWageClaimInd =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateResponse.SesaWageAmount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFplsLocateResponse.IrsNameControl =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateResponse.IrsTaxYear =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFplsLocateResponse.NprcEmpdOrSepd =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateResponse.SsaFederalOrMilitary =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateResponse.SsaCorpDivision =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateResponse.MbrBenefitAmount =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFplsLocateResponse.MbrDateOfDeath =
          db.GetNullableDate(reader, 25);
        entities.ExistingFplsLocateResponse.VaBenefitCode =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateResponse.VaDateOfDeath =
          db.GetNullableDate(reader, 27);
        entities.ExistingFplsLocateResponse.VaAmtOfAwardEffectiveDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingFplsLocateResponse.VaAmountOfAward =
          db.GetNullableInt32(reader, 29);
        entities.ExistingFplsLocateResponse.VaSuspenseCode =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateResponse.VaIncarcerationCode =
          db.GetNullableString(reader, 31);
        entities.ExistingFplsLocateResponse.VaRetirementPayCode =
          db.GetNullableString(reader, 32);
        entities.ExistingFplsLocateResponse.ReturnedAddress =
          db.GetNullableString(reader, 33);
        entities.ExistingFplsLocateResponse.SsnReturned =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateResponse.DodAnnualSalary =
          db.GetNullableInt32(reader, 35);
        entities.ExistingFplsLocateResponse.DodDateOfBirth =
          db.GetNullableDate(reader, 36);
        entities.ExistingFplsLocateResponse.SubmittingOffice =
          db.GetNullableString(reader, 37);
        entities.ExistingFplsLocateResponse.AddressIndType =
          db.GetNullableString(reader, 38);
        entities.ExistingFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse7()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevNextFplsLocateRequest.Populated);
    entities.ExistingPrevNextFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse7",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingPrevNextFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPrevNextFplsLocateRequest.CspNumber);
        db.
          SetInt32(command, "identifier", export.FplsLocateResponse.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingPrevNextFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrevNextFplsLocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevNextFplsLocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevNextFplsLocateResponse.Populated = true;
      });
  }

  private bool ReadFplsLocateResponse8()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevNextFplsLocateRequest.Populated);
    entities.ExistingPrevNextFplsLocateResponse.Populated = false;

    return Read("ReadFplsLocateResponse8",
      (db, command) =>
      {
        db.SetInt32(
          command, "flqIdentifier",
          entities.ExistingPrevNextFplsLocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPrevNextFplsLocateRequest.CspNumber);
        db.
          SetInt32(command, "identifier", export.FplsLocateResponse.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingPrevNextFplsLocateResponse.FlqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrevNextFplsLocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevNextFplsLocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevNextFplsLocateResponse.Populated = true;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public CsePerson Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private Common userAction;
    private FplsLocateResponse fplsLocateResponse;
    private CsePerson csePerson;
    private CsePerson hidden;
    private FplsLocateRequest fplsLocateRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FplsAddrGroup group.</summary>
    [Serializable]
    public class FplsAddrGroup
    {
      /// <summary>
      /// A value of FplsAddrGroupDet.
      /// </summary>
      [JsonPropertyName("fplsAddrGroupDet")]
      public FplsWorkArea FplsAddrGroupDet
      {
        get => fplsAddrGroupDet ??= new();
        set => fplsAddrGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea fplsAddrGroupDet;
    }

    /// <summary>A SesaGroup group.</summary>
    [Serializable]
    public class SesaGroup
    {
      /// <summary>
      /// A value of SesaGroupDet.
      /// </summary>
      [JsonPropertyName("sesaGroupDet")]
      public OblgWork SesaGroupDet
      {
        get => sesaGroupDet ??= new();
        set => sesaGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork sesaGroupDet;
    }

    /// <summary>A AddrGroup group.</summary>
    [Serializable]
    public class AddrGroup
    {
      /// <summary>
      /// A value of AddrGroupDetFplsWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetFplsWorkArea")]
      public FplsWorkArea AddrGroupDetFplsWorkArea
      {
        get => addrGroupDetFplsWorkArea ??= new();
        set => addrGroupDetFplsWorkArea = value;
      }

      /// <summary>
      /// A value of AddrGroupDetWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetWorkArea")]
      public WorkArea AddrGroupDetWorkArea
      {
        get => addrGroupDetWorkArea ??= new();
        set => addrGroupDetWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea addrGroupDetFplsWorkArea;
      private WorkArea addrGroupDetWorkArea;
    }

    /// <summary>
    /// Gets a value of FplsAddr.
    /// </summary>
    [JsonIgnore]
    public Array<FplsAddrGroup> FplsAddr => fplsAddr ??= new(
      FplsAddrGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FplsAddr for json serialization.
    /// </summary>
    [JsonPropertyName("fplsAddr")]
    [Computed]
    public IList<FplsAddrGroup> FplsAddr_Json
    {
      get => fplsAddr;
      set => FplsAddr.Assign(value);
    }

    /// <summary>
    /// A value of TransactionError.
    /// </summary>
    [JsonPropertyName("transactionError")]
    public CodeValue TransactionError
    {
      get => transactionError ??= new();
      set => transactionError = value;
    }

    /// <summary>
    /// A value of SesaRespondingState.
    /// </summary>
    [JsonPropertyName("sesaRespondingState")]
    public CodeValue SesaRespondingState
    {
      get => sesaRespondingState ??= new();
      set => sesaRespondingState = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// Gets a value of Sesa.
    /// </summary>
    [JsonIgnore]
    public Array<SesaGroup> Sesa => sesa ??= new(SesaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Sesa for json serialization.
    /// </summary>
    [JsonPropertyName("sesa")]
    [Computed]
    public IList<SesaGroup> Sesa_Json
    {
      get => sesa;
      set => Sesa.Assign(value);
    }

    /// <summary>
    /// Gets a value of Addr.
    /// </summary>
    [JsonIgnore]
    public Array<AddrGroup> Addr => addr ??= new(AddrGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Addr for json serialization.
    /// </summary>
    [JsonPropertyName("addr")]
    [Computed]
    public IList<AddrGroup> Addr_Json
    {
      get => addr;
      set => Addr.Assign(value);
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

    /// <summary>
    /// A value of FplsAgency.
    /// </summary>
    [JsonPropertyName("fplsAgency")]
    public CodeValue FplsAgency
    {
      get => fplsAgency ??= new();
      set => fplsAgency = value;
    }

    /// <summary>
    /// A value of FplsResp.
    /// </summary>
    [JsonPropertyName("fplsResp")]
    public CodeValue FplsResp
    {
      get => fplsResp ??= new();
      set => fplsResp = value;
    }

    /// <summary>
    /// A value of DodEligDesc.
    /// </summary>
    [JsonPropertyName("dodEligDesc")]
    public CodeValue DodEligDesc
    {
      get => dodEligDesc ??= new();
      set => dodEligDesc = value;
    }

    /// <summary>
    /// A value of DodStatusDesc.
    /// </summary>
    [JsonPropertyName("dodStatusDesc")]
    public CodeValue DodStatusDesc
    {
      get => dodStatusDesc ??= new();
      set => dodStatusDesc = value;
    }

    /// <summary>
    /// A value of DodServiceDesc.
    /// </summary>
    [JsonPropertyName("dodServiceDesc")]
    public CodeValue DodServiceDesc
    {
      get => dodServiceDesc ??= new();
      set => dodServiceDesc = value;
    }

    /// <summary>
    /// A value of DodPayGradeDesc.
    /// </summary>
    [JsonPropertyName("dodPayGradeDesc")]
    public CodeValue DodPayGradeDesc
    {
      get => dodPayGradeDesc ??= new();
      set => dodPayGradeDesc = value;
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

    private Array<FplsAddrGroup> fplsAddr;
    private CodeValue transactionError;
    private CodeValue sesaRespondingState;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<SesaGroup> sesa;
    private Array<AddrGroup> addr;
    private FplsLocateRequest fplsLocateRequest;
    private FplsLocateResponse fplsLocateResponse;
    private CodeValue fplsAgency;
    private CodeValue fplsResp;
    private CodeValue dodEligDesc;
    private CodeValue dodStatusDesc;
    private CodeValue dodServiceDesc;
    private CodeValue dodPayGradeDesc;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RequestFound.
    /// </summary>
    [JsonPropertyName("requestFound")]
    public Common RequestFound
    {
      get => requestFound ??= new();
      set => requestFound = value;
    }

    /// <summary>
    /// A value of ResponseFound.
    /// </summary>
    [JsonPropertyName("responseFound")]
    public Common ResponseFound
    {
      get => responseFound ??= new();
      set => responseFound = value;
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
    /// A value of InvalidCode.
    /// </summary>
    [JsonPropertyName("invalidCode")]
    public Common InvalidCode
    {
      get => invalidCode ??= new();
      set => invalidCode = value;
    }

    private DateWorkArea nullDate;
    private FplsLocateRequest fplsLocateRequest;
    private FplsLocateResponse fplsLocateResponse;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common requestFound;
    private Common responseFound;
    private Code code;
    private CodeValue codeValue;
    private Common invalidCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingPrevNextFplsLocateResponse.
    /// </summary>
    [JsonPropertyName("existingPrevNextFplsLocateResponse")]
    public FplsLocateResponse ExistingPrevNextFplsLocateResponse
    {
      get => existingPrevNextFplsLocateResponse ??= new();
      set => existingPrevNextFplsLocateResponse = value;
    }

    /// <summary>
    /// A value of ExistingPrevNextFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingPrevNextFplsLocateRequest")]
    public FplsLocateRequest ExistingPrevNextFplsLocateRequest
    {
      get => existingPrevNextFplsLocateRequest ??= new();
      set => existingPrevNextFplsLocateRequest = value;
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
    /// A value of ExistingFplsLocateResponse.
    /// </summary>
    [JsonPropertyName("existingFplsLocateResponse")]
    public FplsLocateResponse ExistingFplsLocateResponse
    {
      get => existingFplsLocateResponse ??= new();
      set => existingFplsLocateResponse = value;
    }

    private CsePerson existingCsePerson;
    private FplsLocateResponse existingPrevNextFplsLocateResponse;
    private FplsLocateRequest existingPrevNextFplsLocateRequest;
    private FplsLocateRequest existingFplsLocateRequest;
    private FplsLocateResponse existingFplsLocateResponse;
  }
#endregion
}
