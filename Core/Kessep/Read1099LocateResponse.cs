// Program: READ_1099_LOCATE_RESPONSE, ID: 372359634, model: 746.
// Short name: SWE01041
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
/// A program: READ_1099_LOCATE_RESPONSE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This BAA process action block reads the 1099 response received.
/// </para>
/// </summary>
[Serializable]
public partial class Read1099LocateResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the READ_1099_LOCATE_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Read1099LocateResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Read1099LocateResponse.
  /// </summary>
  public Read1099LocateResponse(IContext context, Import import, Export export):
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
    // AUTHOR    	 DATE  	            DESCRIPTION
    // MK              9/1998
    // Modified DISPLAY to return when 1099 request NF with
    // 1099 request NF message.
    // ******** END MAINTENANCE LOG ****************
    // ---------------------------------------------
    // For DISPLAY:
    // Read the 1099 request for the given identifier or the latest 1099 
    // request. Read the latest 1099 response received for that request.
    // For NEXT:
    // Read the next 1099 response (i.e. response just prior to current response
    // ). If no more response found, read the next 1099 request (i.e. request
    // earlier to current request) and its corresponding latest response.
    // For PREV:
    // Read the previous response (i.e. response later than current response). 
    // If no more response found, read the previous 1099 request (i.e. request
    // later than current request) and its corresponding earliest response.
    // ---------------------------------------------
    export.Data1099LocateRequest.Identifier =
      import.Data1099LocateRequest.Identifier;
    export.Data1099LocateResponse.Identifier =
      import.Data1099LocateResponse.Identifier;
    export.CsePerson.Number = import.CsePerson.Number;

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
      export.CsePersonsWorkSet.Number = import.CsePerson.Number;
      ExitState = "CSE_PERSON_NF";

      return;
    }

    switch(TrimEnd(ToUpper(import.UserAction.Command)))
    {
      case "DISPLAY":
        local.Local1099RequestFound.Flag = "N";
        local.Local1099ResponseFound.Flag = "N";

        if (Read1099LocateRequest6())
        {
          local.Local1099RequestFound.Flag = "Y";
          export.Data1099LocateRequest.Assign(
            entities.Existingdata1099LocateRequest);

          // ---------------------------------------------
          // Get the latest response or the specified response for this request
          // ---------------------------------------------
          if (Read1099LocateResponse3())
          {
            local.Local1099ResponseFound.Flag = "Y";
            export.Data1099LocateResponse.Assign(
              entities.Existingdata1099LocateResponse);
          }
        }

        if (AsChar(local.Local1099RequestFound.Flag) == 'N')
        {
          // *********************************
          // Changed EXIT STATE and ESCAPE
          // logic to indicate that a 1099 request
          // was not found and exit cab.
          // MK 09/98
          // **********************************
          ExitState = "1099_LOCATE_REQUEST_NF";

          return;
        }

        if (AsChar(local.Local1099ResponseFound.Flag) == 'N')
        {
          export.Data1099LocateResponse.Identifier = 0;
          ExitState = "OE0104_NO_RESPONSE_RECEIVED";

          if (Equal(entities.Existingdata1099LocateRequest.RequestSentDate,
            local.NullDate.Date))
          {
            ExitState = "REQUEST_HAS_NOT_YET_BEEN_SENT";
          }
        }

        break;
      case "NEXT":
        local.Local1099RequestFound.Flag = "N";
        local.Local1099ResponseFound.Flag = "N";

        if (Read1099LocateRequest2())
        {
          local.Local1099RequestFound.Flag = "Y";
          export.Data1099LocateRequest.Assign(
            entities.Existingdata1099LocateRequest);

          // ---------------------------------------------
          // Get the response later than the given response
          // ---------------------------------------------
          if (Read1099LocateResponse1())
          {
            local.Local1099ResponseFound.Flag = "Y";
            export.Data1099LocateResponse.Assign(
              entities.Existingdata1099LocateResponse);
          }

          if (AsChar(local.Local1099ResponseFound.Flag) == 'N')
          {
            // ---------------------------------------------
            // No more responses for the given request. Try to
            // read the response for the request later than the
            // given request.
            // ---------------------------------------------
            local.Local1099RequestFound.Flag = "N";
            local.Local1099ResponseFound.Flag = "N";

            if (Read1099LocateRequest3())
            {
              local.Local1099RequestFound.Flag = "Y";
              export.Data1099LocateRequest.Assign(
                entities.Existingdata1099LocateRequest);

              // ---------------------------------------------
              // Read the earliest response for current request.
              // ---------------------------------------------
              if (Read1099LocateResponse2())
              {
                local.Local1099ResponseFound.Flag = "Y";
                export.Data1099LocateResponse.Assign(
                  entities.Existingdata1099LocateResponse);
              }
            }
          }
        }
        else
        {
          ExitState = "1099_LOCATE_REQUEST_NF";

          return;
        }

        if (AsChar(local.Local1099RequestFound.Flag) == 'N')
        {
          ExitState = "OE0097_NO_MORE_REQUEST";

          break;
        }

        if (AsChar(local.Local1099ResponseFound.Flag) == 'N')
        {
          export.Data1099LocateResponse.Identifier = 0;
          ExitState = "OE0104_NO_RESPONSE_RECEIVED";

          if (false)
          {
            ExitState = "REQUEST_HAS_NOT_YET_BEEN_SENT";
          }

          return;
        }

        break;
      case "PREV":
        local.Local1099RequestFound.Flag = "N";
        local.Local1099ResponseFound.Flag = "N";

        if (Read1099LocateRequest2())
        {
          local.Local1099RequestFound.Flag = "Y";
          export.Data1099LocateRequest.Assign(
            entities.Existingdata1099LocateRequest);

          if (Read1099LocateResponse4())
          {
            local.Local1099ResponseFound.Flag = "Y";
            export.Data1099LocateResponse.Assign(
              entities.Existingdata1099LocateResponse);
          }

          if (AsChar(local.Local1099ResponseFound.Flag) == 'N')
          {
            // ************************************************
            // *No more responses exist for the current request
            // *Try to read a request which is earlier than   *
            // *than the current request and its related      *
            // *response.
            // 
            // *
            // ************************************************
            local.Local1099ResponseFound.Flag = "N";
            local.Local1099RequestFound.Flag = "N";

            if (Read1099LocateRequest5())
            {
              local.Local1099RequestFound.Flag = "Y";
              export.Data1099LocateRequest.Assign(
                entities.Existingdata1099LocateRequest);

              if (Read1099LocateResponse5())
              {
                local.Local1099ResponseFound.Flag = "Y";
                export.Data1099LocateResponse.Assign(
                  entities.Existingdata1099LocateResponse);
              }
            }

            // ************************************************
            // *Test to see if there was a prior request found*
            // *and if not then show the oldest request along *
            // *with the oldest response for that request.    *
            // ************************************************
            if (AsChar(local.Local1099RequestFound.Flag) == 'N')
            {
              // ************************************************
              // *No more prior requests exist for this CSE     *
              // *Person. Display the oldest request and its    *
              // *related response, and a message which states  *
              // *that there are no prior requests.             *
              // ************************************************
              local.Local1099ResponseFound.Flag = "N";

              if (Read1099LocateRequest4())
              {
                export.Data1099LocateRequest.Assign(
                  entities.Existingdata1099LocateRequest);
                local.Local1099RequestFound.Flag = "Y";

                foreach(var item in Read1099LocateResponse8())
                {
                  local.Local1099ResponseFound.Flag = "Y";
                  export.Data1099LocateResponse.Assign(
                    entities.Existingdata1099LocateResponse);
                }
              }

              ExitState = "OE0097_NO_MORE_REQUEST";
            }
          }
        }

        break;
      default:
        break;
    }

    if (AsChar(local.Local1099ResponseFound.Flag) == 'Y')
    {
      local.Code.CodeName = "1099 DOCUMENT CODE";
      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.DocumentCode ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.Export1099DocumentCode.Description = local.CodeValue.Description;
      }
      else
      {
        export.Export1099DocumentCode.Description = "Invalid Doc Code";
      }

      local.Code.CodeName = "1099 RESULT CODE";
      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateRequest.NoMatchCode ?? Spaces(10);

      if (!IsEmpty(entities.Existingdata1099LocateRequest.NoMatchCode))
      {
        UseCabGetCodeValueDescription();

        if (AsChar(local.ErrorInDecoding.Flag) == 'N')
        {
          export.Export1099ResultCode.Description = local.CodeValue.Description;
        }
        else
        {
          export.Export1099ResultCode.Description = "Result Code Invalid";
        }
      }

      local.Code.CodeName = "1099 DOCUMENT IND";
      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd1 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd1.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd2 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd2.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd3 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd3.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd4 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd4.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd5 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd5.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd6 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd6.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd7 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd7.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd8 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd8.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd9 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd9.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd10 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd10.Description = local.CodeValue.Description;
      }

      local.CodeValue.Cdvalue =
        entities.Existingdata1099LocateResponse.AmountInd11 ?? Spaces(10);
      UseCabGetCodeValueDescription();

      if (AsChar(local.ErrorInDecoding.Flag) == 'N')
      {
        export.AmountInd11.Description = local.CodeValue.Description;
      }
    }

    local.CodeValue.Cdvalue =
      entities.Existingdata1099LocateResponse.AmountInd12 ?? Spaces(10);
    UseCabGetCodeValueDescription();

    if (AsChar(local.ErrorInDecoding.Flag) == 'N')
    {
      export.AmountInd12.Description = local.CodeValue.Description;
    }

    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";

    if (Read1099LocateRequest1())
    {
      if (Read1099LocateResponse6())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }
      else if (Read1099LocateRequest7())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }

      if (Read1099LocateResponse7())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }
      else if (Read1099LocateRequest8())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.ErrorInDecoding.Flag = useExport.ErrorInDecoding.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private bool Read1099LocateRequest1()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", export.Data1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.CreatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest2()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.Data1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.CreatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest3()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.Data1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.CreatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest4()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.Data1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.CreatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest5()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.Data1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.CreatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest6()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.Data1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.CreatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest7()
  {
    entities.ExistingPrevOrNextdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest7",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier",
          entities.Existingdata1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNextdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPrevOrNextdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPrevOrNextdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest8()
  {
    entities.ExistingPrevOrNextdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest8",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier",
          entities.Existingdata1099LocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNextdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPrevOrNextdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPrevOrNextdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateResponse1()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse1",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", import.Data1099LocateResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 22);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 23);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 24);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 37);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 39);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.Populated = true;
      });
  }

  private bool Read1099LocateResponse2()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse2",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 22);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 23);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 24);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 37);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 39);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.Populated = true;
      });
  }

  private bool Read1099LocateResponse3()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse3",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", import.Data1099LocateResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 22);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 23);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 24);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 37);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 39);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.Populated = true;
      });
  }

  private bool Read1099LocateResponse4()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse4",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", import.Data1099LocateResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 22);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 23);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 24);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 37);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 39);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.Populated = true;
      });
  }

  private bool Read1099LocateResponse5()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse5",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 22);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 23);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 24);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 37);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 39);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.Populated = true;
      });
  }

  private bool Read1099LocateResponse6()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.ExistingPrevOrNextdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse6",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", export.Data1099LocateResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNextdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrevOrNextdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevOrNextdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevOrNextdata1099LocateResponse.Populated = true;
      });
  }

  private bool Read1099LocateResponse7()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.ExistingPrevOrNextdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse7",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", export.Data1099LocateResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNextdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrevOrNextdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevOrNextdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevOrNextdata1099LocateResponse.Populated = true;
      });
  }

  private IEnumerable<bool> Read1099LocateResponse8()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return ReadEach("Read1099LocateResponse8",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 22);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 23);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 24);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 37);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 39);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.Populated = true;

        return true;
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
        entities.ExistingCsePerson.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    /// <summary>
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
    }

    private Common userAction;
    private CsePerson csePerson;
    private Data1099LocateRequest data1099LocateRequest;
    private Data1099LocateResponse data1099LocateResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Export1099ResultCode.
    /// </summary>
    [JsonPropertyName("export1099ResultCode")]
    public CodeValue Export1099ResultCode
    {
      get => export1099ResultCode ??= new();
      set => export1099ResultCode = value;
    }

    /// <summary>
    /// A value of Export1099DocumentCode.
    /// </summary>
    [JsonPropertyName("export1099DocumentCode")]
    public CodeValue Export1099DocumentCode
    {
      get => export1099DocumentCode ??= new();
      set => export1099DocumentCode = value;
    }

    /// <summary>
    /// A value of AmountInd1.
    /// </summary>
    [JsonPropertyName("amountInd1")]
    public CodeValue AmountInd1
    {
      get => amountInd1 ??= new();
      set => amountInd1 = value;
    }

    /// <summary>
    /// A value of AmountInd2.
    /// </summary>
    [JsonPropertyName("amountInd2")]
    public CodeValue AmountInd2
    {
      get => amountInd2 ??= new();
      set => amountInd2 = value;
    }

    /// <summary>
    /// A value of AmountInd3.
    /// </summary>
    [JsonPropertyName("amountInd3")]
    public CodeValue AmountInd3
    {
      get => amountInd3 ??= new();
      set => amountInd3 = value;
    }

    /// <summary>
    /// A value of AmountInd4.
    /// </summary>
    [JsonPropertyName("amountInd4")]
    public CodeValue AmountInd4
    {
      get => amountInd4 ??= new();
      set => amountInd4 = value;
    }

    /// <summary>
    /// A value of AmountInd5.
    /// </summary>
    [JsonPropertyName("amountInd5")]
    public CodeValue AmountInd5
    {
      get => amountInd5 ??= new();
      set => amountInd5 = value;
    }

    /// <summary>
    /// A value of AmountInd6.
    /// </summary>
    [JsonPropertyName("amountInd6")]
    public CodeValue AmountInd6
    {
      get => amountInd6 ??= new();
      set => amountInd6 = value;
    }

    /// <summary>
    /// A value of AmountInd7.
    /// </summary>
    [JsonPropertyName("amountInd7")]
    public CodeValue AmountInd7
    {
      get => amountInd7 ??= new();
      set => amountInd7 = value;
    }

    /// <summary>
    /// A value of AmountInd8.
    /// </summary>
    [JsonPropertyName("amountInd8")]
    public CodeValue AmountInd8
    {
      get => amountInd8 ??= new();
      set => amountInd8 = value;
    }

    /// <summary>
    /// A value of AmountInd9.
    /// </summary>
    [JsonPropertyName("amountInd9")]
    public CodeValue AmountInd9
    {
      get => amountInd9 ??= new();
      set => amountInd9 = value;
    }

    /// <summary>
    /// A value of AmountInd10.
    /// </summary>
    [JsonPropertyName("amountInd10")]
    public CodeValue AmountInd10
    {
      get => amountInd10 ??= new();
      set => amountInd10 = value;
    }

    /// <summary>
    /// A value of AmountInd11.
    /// </summary>
    [JsonPropertyName("amountInd11")]
    public CodeValue AmountInd11
    {
      get => amountInd11 ??= new();
      set => amountInd11 = value;
    }

    /// <summary>
    /// A value of AmountInd12.
    /// </summary>
    [JsonPropertyName("amountInd12")]
    public CodeValue AmountInd12
    {
      get => amountInd12 ??= new();
      set => amountInd12 = value;
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
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue export1099ResultCode;
    private CodeValue export1099DocumentCode;
    private CodeValue amountInd1;
    private CodeValue amountInd2;
    private CodeValue amountInd3;
    private CodeValue amountInd4;
    private CodeValue amountInd5;
    private CodeValue amountInd6;
    private CodeValue amountInd7;
    private CodeValue amountInd8;
    private CodeValue amountInd9;
    private CodeValue amountInd10;
    private CodeValue amountInd11;
    private CodeValue amountInd12;
    private CsePerson csePerson;
    private Data1099LocateResponse data1099LocateResponse;
    private Data1099LocateRequest data1099LocateRequest;
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
    /// A value of ErrorInDecoding.
    /// </summary>
    [JsonPropertyName("errorInDecoding")]
    public Common ErrorInDecoding
    {
      get => errorInDecoding ??= new();
      set => errorInDecoding = value;
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
    /// A value of Local1099RequestFound.
    /// </summary>
    [JsonPropertyName("local1099RequestFound")]
    public Common Local1099RequestFound
    {
      get => local1099RequestFound ??= new();
      set => local1099RequestFound = value;
    }

    /// <summary>
    /// A value of Local1099ResponseFound.
    /// </summary>
    [JsonPropertyName("local1099ResponseFound")]
    public Common Local1099ResponseFound
    {
      get => local1099ResponseFound ??= new();
      set => local1099ResponseFound = value;
    }

    private DateWorkArea nullDate;
    private Common errorInDecoding;
    private Code code;
    private CodeValue codeValue;
    private Common local1099RequestFound;
    private Common local1099ResponseFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPrevOrNextdata1099LocateResponse.
    /// </summary>
    [JsonPropertyName("existingPrevOrNextdata1099LocateResponse")]
    public Data1099LocateResponse ExistingPrevOrNextdata1099LocateResponse
    {
      get => existingPrevOrNextdata1099LocateResponse ??= new();
      set => existingPrevOrNextdata1099LocateResponse = value;
    }

    /// <summary>
    /// A value of ExistingPrevOrNextdata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("existingPrevOrNextdata1099LocateRequest")]
    public Data1099LocateRequest ExistingPrevOrNextdata1099LocateRequest
    {
      get => existingPrevOrNextdata1099LocateRequest ??= new();
      set => existingPrevOrNextdata1099LocateRequest = value;
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

    /// <summary>
    /// A value of Existingdata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("existingdata1099LocateRequest")]
    public Data1099LocateRequest Existingdata1099LocateRequest
    {
      get => existingdata1099LocateRequest ??= new();
      set => existingdata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of Existingdata1099LocateResponse.
    /// </summary>
    [JsonPropertyName("existingdata1099LocateResponse")]
    public Data1099LocateResponse Existingdata1099LocateResponse
    {
      get => existingdata1099LocateResponse ??= new();
      set => existingdata1099LocateResponse = value;
    }

    private Data1099LocateResponse existingPrevOrNextdata1099LocateResponse;
    private Data1099LocateRequest existingPrevOrNextdata1099LocateRequest;
    private CsePerson existingCsePerson;
    private Data1099LocateRequest existingdata1099LocateRequest;
    private Data1099LocateResponse existingdata1099LocateResponse;
  }
#endregion
}
