// Program: OE_FCRP_PROACTIVE_MATCH_DISPLAY, ID: 373542745, model: 746.
// Short name: SWE00276
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
/// A program: OE_FCRP_PROACTIVE_MATCH_DISPLAY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block is called by PROCESS FPLS REQUEST TRANSACTION Procedures.
/// It Reads the FLPS_REQUEST and FPLS_RESPONSE entities and reacts to
/// DISPLAY, NEXT, and PREV commands.
/// </para>
/// </summary>
[Serializable]
public partial class OeFcrpProactiveMatchDisplay: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCRP_PROACTIVE_MATCH_DISPLAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrpProactiveMatchDisplay(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrpProactiveMatchDisplay.
  /// </summary>
  public OeFcrpProactiveMatchDisplay(IContext context, Import import,
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
    // *****************************************************************************
    // AUTHOR               DATE  	    DESCRIPTION
    // Sree Veettil      02/22/2000
    // E Lyman           10/20/2000        PR105515 Fixed fatal view overflow
    //                                     
    // on number of cases displayed.
    // ********* END MAINTENANCE LOG 
    // ***********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Cases.Index = -1;
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.ServiceProvider.UserId = global.UserId;
    local.FcrFormatPersonNumber.Text15 = "00000" + import.CsePerson.Number;
    export.Scrolling.PlusFlag = "";
    export.Scrolling.MinusFlag = "";
    local.PrevCase.Number = "";
    local.CaseFound.Flag = "N";

    if (ReadCsePerson())
    {
      // ***********************************************************************
      // Check whether the peson accessing the screen is supervisor or not..As 
      // the security has been checked before only CASE WORKER or SUPERVISOR can
      // have access to display.
      // If the person is the assigned case worker display only the cases 
      // assigned to him in which the entered cse_person  plays a case_role.
      // If the person is the supervisor display all the cases i which the 
      // cse_person plays a role.
      // ***********************************************************************
      foreach(var item in ReadCaseRoleCase())
      {
        // *****************************************************************
        // Disply only those cases which are assigned to the CASE WORKER.
        // *****************************************************************
        if (AsChar(import.IsSupervisor.Flag) == 'N')
        {
          if (ReadServiceProvider())
          {
            if (!Equal(entities.Case1.Number, local.PrevCase.Number))
            {
              ++export.Cases.Index;
              export.Cases.CheckSize();

              export.Cases.Update.G.Number = entities.Case1.Number;
              local.PrevCase.Number = entities.Case1.Number;
            }
          }
        }
        else if (AsChar(import.IsSupervisor.Flag) == 'Y')
        {
          // *****************************************************************
          // Display only those cases which are assigned to the SUPERVISOR
          // (including those on which the supervisor plays the role of worker).
          // *****************************************************************
          UseCoCabIsUserAssignedToCase();

          if (AsChar(local.SupervisorOnThisCase.Flag) == 'Y' || AsChar
            (local.WorkerOnThisCase.Flag) == 'Y')
          {
            if (!Equal(entities.Case1.Number, local.PrevCase.Number))
            {
              ++export.Cases.Index;
              export.Cases.CheckSize();

              export.Cases.Update.G.Number = entities.Case1.Number;
              local.PrevCase.Number = entities.Case1.Number;
            }
          }
        }

        if (export.Cases.Index + 1 == Export.CasesGroup.Capacity)
        {
          goto Read;
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

Read:

    local.ResponseFound.Flag = "N";

    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        if (ReadFcrProactiveMatchResponse5())
        {
          local.ResponseFound.Flag = "Y";
          export.Next.Assign(entities.FcrProactiveMatchResponse);
        }

        if (AsChar(local.ResponseFound.Flag) == 'N')
        {
          ExitState = "OE0114_NO_RESPONSE_RECEIVED";

          return;
        }

        break;
      case "NEXT":
        local.ResponseFound.Flag = "N";
        local.FcrProactiveMatchResponse.Identifier =
          import.FcrProactiveMatchResponse.Identifier;

        if (ReadFcrProactiveMatchResponse3())
        {
          local.ResponseFound.Flag = "Y";
          export.Next.Assign(entities.FcrProactiveMatchResponse);
        }

        break;
      case "PREV":
        local.ResponseFound.Flag = "N";
        local.FcrProactiveMatchResponse.Identifier =
          import.FcrProactiveMatchResponse.Identifier;

        if (ReadFcrProactiveMatchResponse1())
        {
          local.ResponseFound.Flag = "Y";
          export.Next.Assign(entities.FcrProactiveMatchResponse);
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
    }

    // ************************************************
    // Format the information obtained from PROACTIVE_MATCH for screen display
    // ************************************************
    if (Equal(export.Next.ResponseCode, "MM"))
    {
      export.Response.Text50 = "Additional Associated persons  exist.";
    }
    else if (Equal(export.Next.ResponseCode, "MA"))
    {
      export.Response.Text50 = "No Additional Associated Persons exist.";
    }

    switch(AsChar(export.Next.ActionTypeCode))
    {
      case 'C':
        switch(TrimEnd(export.Next.UserField ?? ""))
        {
          case "1":
            export.Message.Text30 = "Case Change (Non IV-D to IV-D)";

            break;
          case "2":
            export.Message.Text30 = "Case ID Change";

            break;
          case "3":
            export.Message.Text30 = "Court Order Change (N to Y)";

            break;
          case "4":
            export.Message.Text30 = "Case Closed or Person Removed";

            break;
          default:
            export.Message.Text30 = "Case Change";

            break;
        }

        break;
      case 'D':
        export.Message.Text30 = "Date of Death";

        break;
      case 'F':
        export.Message.Text30 = "FCR Query Response.";

        break;
      case 'P':
        switch(TrimEnd(export.Next.UserField ?? ""))
        {
          case "C":
            export.Message.Text30 = "Case Deleted";

            break;
          case "P":
            export.Message.Text30 = "Person Deleted";

            break;
          default:
            export.Message.Text30 = "Person Change";

            break;
        }

        break;
      default:
        break;
    }

    // *****************************************************************
    // Get the FIPS county name and the stae name to display on the screen.
    // *****************************************************************
    local.CountyFips.Count =
      (int)StringToNumber(export.Next.MatchFcrFipsCountyCd);
    local.StateFips.Count =
      (int)StringToNumber(export.Next.TransmitterStateOrTerrCode);

    if (ReadFips())
    {
      export.Fips.Assign(entities.Fips);
    }

    switch(TrimEnd(export.Next.MatchedParticipantType ?? ""))
    {
      case "CH":
        export.MatchedPersonRole.Text2 = "CH";

        break;
      case "CP":
        export.MatchedPersonRole.Text2 = "AR";

        break;
      case "NP":
        export.MatchedPersonRole.Text2 = "AP";

        break;
      case "PF":
        export.MatchedPersonRole.Text2 = "AP";

        break;
      default:
        break;
    }

    export.MatchedPersonCsePersonsWorkSet.Ssn =
      export.Next.SubmittedOrMatchedSsn ?? Spaces(9);
    export.MatchedPersonCsePerson.Number =
      Substring(export.Next.StateMemberId, 6, 10);
    export.MatchedPersonFcrProactiveMatchResponse.MatchedMemberId =
      export.Next.MatchedMemberId ?? "";
    export.MatchedPersonCsePersonsWorkSet.FormattedName =
      TrimEnd(export.Next.MiddleName) + " " + TrimEnd(export.Next.LastName);
    export.MatchedPersonCsePersonsWorkSet.FormattedName =
      TrimEnd(export.Next.FirstName) + " " + TrimEnd
      (export.MatchedPersonCsePersonsWorkSet.FormattedName);

    for(export.MatchedPersonAliases.Index = 0; export
      .MatchedPersonAliases.Index < 4; ++export.MatchedPersonAliases.Index)
    {
      if (!export.MatchedPersonAliases.CheckSize())
      {
        break;
      }

      switch(export.MatchedPersonAliases.Index + 1)
      {
        case 1:
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddMiddleName1) + " " + TrimEnd
            (export.Next.MatchedPersonAddLastName1);
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddFirstName1) + " " + TrimEnd
            (export.MatchedPersonAliases.Item.Galiases.FormattedName);

          break;
        case 2:
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddMiddleName2) + " " + TrimEnd
            (export.Next.MatchedPersonAddLastName2);
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddFirstName2) + " " + TrimEnd
            (export.MatchedPersonAliases.Item.Galiases.FormattedName);

          break;
        case 3:
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddMiddleName3) + " " + TrimEnd
            (export.Next.MatchedPersonAddLastName3);
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddFirstName3) + " " + TrimEnd
            (export.MatchedPersonAliases.Item.Galiases.FormattedName);

          break;
        case 4:
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddMiddleName4) + " " + TrimEnd
            (export.Next.MatchedPersonAddLastName4);
          export.MatchedPersonAliases.Update.Galiases.FormattedName =
            TrimEnd(export.Next.MatchedPersonAddFirstName4) + " " + TrimEnd
            (export.MatchedPersonAliases.Item.Galiases.FormattedName);

          break;
        default:
          break;
      }
    }

    export.MatchedPersonAliases.CheckIndex();

    for(export.AssociatedPersons.Index = 0; export.AssociatedPersons.Index < 3; ++
      export.AssociatedPersons.Index)
    {
      if (!export.AssociatedPersons.CheckSize())
      {
        break;
      }

      switch(export.AssociatedPersons.Index + 1)
      {
        case 1:
          switch(TrimEnd(export.Next.AssociatedParticipantType1 ?? ""))
          {
            case "CH":
              export.AssociatedPersons.Update.Grole.Text2 = "CH";

              break;
            case "CP":
              export.AssociatedPersons.Update.Grole.Text2 = "AR";

              break;
            case "NP":
              export.AssociatedPersons.Update.Grole.Text2 = "AP";

              break;
            case "PF":
              export.AssociatedPersons.Update.Grole.Text2 = "AP";

              break;
            default:
              break;
          }

          export.AssociatedPersons.Update.G.Ssn =
            export.Next.AssociatedSsn1 ?? Spaces(9);
          export.AssociatedPersons.Update.G.Sex =
            export.Next.AssociatedPersonSexCode1 ?? Spaces(1);

          if (!Equal(export.Next.AssociatedDob1, local.NullDate.Date))
          {
            export.AssociatedPersons.Update.G.Dob = export.Next.AssociatedDob1;
          }

          if (!Equal(export.Next.AssociatedDod1, local.NullDate.Date))
          {
            export.AssociatedPersons.Update.GassociatedCsePerson.DateOfDeath =
              export.Next.AssociatedDod1;
          }

          export.AssociatedPersons.Update.GassociatedFcrProactiveMatchResponse.
            MatchedMemberId = export.Next.AssociatedOthStTerrMembId1 ?? "";
          export.AssociatedPersons.Update.GassociatedCsePerson.Number =
            Substring(export.Next.AssociatedStateMembId1, 6, 10);
          export.AssociatedPersons.Update.G.FormattedName =
            TrimEnd(export.Next.AssociatedMiddleName1) + " " + TrimEnd
            (export.Next.AssociatedLastName1);
          export.AssociatedPersons.Update.G.FormattedName =
            TrimEnd(export.Next.AssociatedFirstName1) + " " + TrimEnd
            (export.AssociatedPersons.Item.G.FormattedName);

          break;
        case 2:
          switch(TrimEnd(export.Next.AssociatedParticipantType2 ?? ""))
          {
            case "CH":
              export.AssociatedPersons.Update.Grole.Text2 = "CH";

              break;
            case "CP":
              export.AssociatedPersons.Update.Grole.Text2 = "AR";

              break;
            case "NP":
              export.AssociatedPersons.Update.Grole.Text2 = "AP";

              break;
            case "PF":
              export.AssociatedPersons.Update.Grole.Text2 = "AP";

              break;
            default:
              break;
          }

          export.AssociatedPersons.Update.G.Ssn =
            export.Next.AssociatedSsn2 ?? Spaces(9);
          export.AssociatedPersons.Update.G.Sex =
            export.Next.AssociatedPersonSexCode2 ?? Spaces(1);

          if (!Equal(export.Next.AssociatedDob2, local.NullDate.Date))
          {
            export.AssociatedPersons.Update.G.Dob = export.Next.AssociatedDob2;
          }

          if (!Equal(export.Next.AssociatedDod2, local.NullDate.Date))
          {
            export.AssociatedPersons.Update.GassociatedCsePerson.DateOfDeath =
              export.Next.AssociatedDod2;
          }

          export.AssociatedPersons.Update.GassociatedFcrProactiveMatchResponse.
            MatchedMemberId = export.Next.AssociatedOthStTerrMembId2 ?? "";
          export.AssociatedPersons.Update.GassociatedCsePerson.Number =
            Substring(export.Next.AssociatedStateMembId2, 6, 10);
          export.AssociatedPersons.Update.G.FormattedName =
            TrimEnd(export.Next.AssociatedMiddleName2) + " " + TrimEnd
            (export.Next.AssociatedLastName2);
          export.AssociatedPersons.Update.G.FormattedName =
            TrimEnd(export.Next.AssociatedFirstName2) + " " + TrimEnd
            (export.AssociatedPersons.Item.G.FormattedName);

          break;
        case 3:
          switch(TrimEnd(export.Next.AssociatedParticipantType3 ?? ""))
          {
            case "CH":
              export.AssociatedPersons.Update.Grole.Text2 = "CH";

              break;
            case "CP":
              export.AssociatedPersons.Update.Grole.Text2 = "AR";

              break;
            case "NP":
              export.AssociatedPersons.Update.Grole.Text2 = "AP";

              break;
            case "PF":
              export.AssociatedPersons.Update.Grole.Text2 = "AP";

              break;
            default:
              break;
          }

          export.AssociatedPersons.Update.G.Ssn =
            export.Next.AssociatedSsn3 ?? Spaces(9);
          export.AssociatedPersons.Update.G.Sex =
            export.Next.AssociatedPersonSexCode3 ?? Spaces(1);

          if (!Equal(export.Next.AssociatedDob3, local.NullDate.Date))
          {
            export.AssociatedPersons.Update.G.Dob = export.Next.AssociatedDob3;
          }

          if (!Equal(export.Next.AssociatedDod3, local.NullDate.Date))
          {
            export.AssociatedPersons.Update.GassociatedCsePerson.DateOfDeath =
              export.Next.AssociatedDod3;
          }

          export.AssociatedPersons.Update.GassociatedFcrProactiveMatchResponse.
            MatchedMemberId = export.Next.AssociatedOthStTerrMembId3 ?? "";
          export.AssociatedPersons.Update.GassociatedCsePerson.Number =
            Substring(export.Next.AssociatedStateMembId3, 6, 10);
          export.AssociatedPersons.Update.G.FormattedName =
            TrimEnd(export.Next.AssociatedMiddleName3) + " " + TrimEnd
            (export.Next.AssociatedLastName3);
          export.AssociatedPersons.Update.G.FormattedName =
            TrimEnd(export.Next.AssociatedFirstName3) + " " + TrimEnd
            (export.AssociatedPersons.Item.G.FormattedName);

          break;
        default:
          break;
      }
    }

    export.AssociatedPersons.CheckIndex();

    // *****************************************************************
    // Set the scrolling attributes.
    // *****************************************************************
    local.Prev.Identifier = export.Next.Identifier;

    if (ReadFcrProactiveMatchResponse4())
    {
      export.Scrolling.PlusFlag = "+";
    }

    if (ReadFcrProactiveMatchResponse2())
    {
      export.Scrolling.MinusFlag = "-";
    }
  }

  private void UseCoCabIsUserAssignedToCase()
  {
    var useImport = new CoCabIsUserAssignedToCase.Import();
    var useExport = new CoCabIsUserAssignedToCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(CoCabIsUserAssignedToCase.Execute, useImport, useExport);

    local.WorkerOnThisCase.Flag = useExport.OnTheCase.Flag;
    local.SupervisorOnThisCase.Flag = useExport.Supervisor.Flag;
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;

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
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFcrProactiveMatchResponse1()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return Read("ReadFcrProactiveMatchResponse1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "stateMemberId", local.FcrFormatPersonNumber.Text15);
        db.SetInt32(
          command, "identifier", local.FcrProactiveMatchResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.ActionTypeCode =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
          db.GetNullableString(reader, 1);
        entities.FcrProactiveMatchResponse.UserField =
          db.GetNullableString(reader, 2);
        entities.FcrProactiveMatchResponse.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.FcrProactiveMatchResponse.FirstName =
          db.GetNullableString(reader, 4);
        entities.FcrProactiveMatchResponse.MiddleName =
          db.GetNullableString(reader, 5);
        entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
          db.GetNullableString(reader, 6);
        entities.FcrProactiveMatchResponse.StateMemberId =
          db.GetNullableString(reader, 7);
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 8);
        entities.FcrProactiveMatchResponse.ResponseCode =
          db.GetNullableString(reader, 9);
        entities.FcrProactiveMatchResponse.MatchedCaseId =
          db.GetNullableString(reader, 10);
        entities.FcrProactiveMatchResponse.MatchedCaseType =
          db.GetNullableString(reader, 11);
        entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
          db.GetNullableString(reader, 12);
        entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
          db.GetNullableDate(reader, 13);
        entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
          db.GetNullableString(reader, 14);
        entities.FcrProactiveMatchResponse.MatchedParticipantType =
          db.GetNullableString(reader, 15);
        entities.FcrProactiveMatchResponse.MatchedMemberId =
          db.GetNullableString(reader, 16);
        entities.FcrProactiveMatchResponse.MatchedPersonDod =
          db.GetNullableDate(reader, 17);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
          db.GetNullableString(reader, 18);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
          db.GetNullableString(reader, 19);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
          db.GetNullableString(reader, 20);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
          db.GetNullableString(reader, 21);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
          db.GetNullableString(reader, 22);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
          db.GetNullableString(reader, 23);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
          db.GetNullableString(reader, 24);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
          db.GetNullableString(reader, 25);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
          db.GetNullableString(reader, 26);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
          db.GetNullableString(reader, 27);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
          db.GetNullableString(reader, 28);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
          db.GetNullableString(reader, 29);
        entities.FcrProactiveMatchResponse.AssociatedSsn1 =
          db.GetNullableString(reader, 30);
        entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
          db.GetNullableString(reader, 31);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
          db.GetNullableString(reader, 32);
        entities.FcrProactiveMatchResponse.AssociatedLastName1 =
          db.GetNullableString(reader, 33);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
          db.GetNullableString(reader, 34);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
          db.GetNullableString(reader, 35);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
          db.GetNullableString(reader, 36);
        entities.FcrProactiveMatchResponse.AssociatedDob1 =
          db.GetNullableDate(reader, 37);
        entities.FcrProactiveMatchResponse.AssociatedDod1 =
          db.GetNullableDate(reader, 38);
        entities.FcrProactiveMatchResponse.AssociatedSsn2 =
          db.GetNullableString(reader, 39);
        entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
          db.GetNullableString(reader, 40);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
          db.GetNullableString(reader, 41);
        entities.FcrProactiveMatchResponse.AssociatedLastName2 =
          db.GetNullableString(reader, 42);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
          db.GetNullableString(reader, 43);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
          db.GetNullableString(reader, 44);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
          db.GetNullableString(reader, 45);
        entities.FcrProactiveMatchResponse.AssociatedDob2 =
          db.GetNullableDate(reader, 46);
        entities.FcrProactiveMatchResponse.AssociatedDod2 =
          db.GetNullableDate(reader, 47);
        entities.FcrProactiveMatchResponse.AssociatedSsn3 =
          db.GetNullableString(reader, 48);
        entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
          db.GetNullableString(reader, 49);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
          db.GetNullableString(reader, 50);
        entities.FcrProactiveMatchResponse.AssociatedLastName3 =
          db.GetNullableString(reader, 51);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
          db.GetNullableString(reader, 52);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
          db.GetNullableString(reader, 53);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
          db.GetNullableString(reader, 54);
        entities.FcrProactiveMatchResponse.AssociatedDob3 =
          db.GetNullableDate(reader, 55);
        entities.FcrProactiveMatchResponse.AssociatedDod3 =
          db.GetNullableDate(reader, 56);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
          db.GetNullableString(reader, 57);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
          db.GetNullableString(reader, 58);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
          db.GetNullableString(reader, 59);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 60);
        entities.FcrProactiveMatchResponse.DateReceived =
          db.GetNullableDate(reader, 61);
        entities.FcrProactiveMatchResponse.LastName =
          db.GetNullableString(reader, 62);
        entities.FcrProactiveMatchResponse.Populated = true;
      });
  }

  private bool ReadFcrProactiveMatchResponse2()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return Read("ReadFcrProactiveMatchResponse2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "stateMemberId", local.FcrFormatPersonNumber.Text15);
        db.SetInt32(command, "identifier", local.Prev.Identifier);
      },
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.ActionTypeCode =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
          db.GetNullableString(reader, 1);
        entities.FcrProactiveMatchResponse.UserField =
          db.GetNullableString(reader, 2);
        entities.FcrProactiveMatchResponse.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.FcrProactiveMatchResponse.FirstName =
          db.GetNullableString(reader, 4);
        entities.FcrProactiveMatchResponse.MiddleName =
          db.GetNullableString(reader, 5);
        entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
          db.GetNullableString(reader, 6);
        entities.FcrProactiveMatchResponse.StateMemberId =
          db.GetNullableString(reader, 7);
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 8);
        entities.FcrProactiveMatchResponse.ResponseCode =
          db.GetNullableString(reader, 9);
        entities.FcrProactiveMatchResponse.MatchedCaseId =
          db.GetNullableString(reader, 10);
        entities.FcrProactiveMatchResponse.MatchedCaseType =
          db.GetNullableString(reader, 11);
        entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
          db.GetNullableString(reader, 12);
        entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
          db.GetNullableDate(reader, 13);
        entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
          db.GetNullableString(reader, 14);
        entities.FcrProactiveMatchResponse.MatchedParticipantType =
          db.GetNullableString(reader, 15);
        entities.FcrProactiveMatchResponse.MatchedMemberId =
          db.GetNullableString(reader, 16);
        entities.FcrProactiveMatchResponse.MatchedPersonDod =
          db.GetNullableDate(reader, 17);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
          db.GetNullableString(reader, 18);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
          db.GetNullableString(reader, 19);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
          db.GetNullableString(reader, 20);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
          db.GetNullableString(reader, 21);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
          db.GetNullableString(reader, 22);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
          db.GetNullableString(reader, 23);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
          db.GetNullableString(reader, 24);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
          db.GetNullableString(reader, 25);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
          db.GetNullableString(reader, 26);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
          db.GetNullableString(reader, 27);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
          db.GetNullableString(reader, 28);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
          db.GetNullableString(reader, 29);
        entities.FcrProactiveMatchResponse.AssociatedSsn1 =
          db.GetNullableString(reader, 30);
        entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
          db.GetNullableString(reader, 31);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
          db.GetNullableString(reader, 32);
        entities.FcrProactiveMatchResponse.AssociatedLastName1 =
          db.GetNullableString(reader, 33);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
          db.GetNullableString(reader, 34);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
          db.GetNullableString(reader, 35);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
          db.GetNullableString(reader, 36);
        entities.FcrProactiveMatchResponse.AssociatedDob1 =
          db.GetNullableDate(reader, 37);
        entities.FcrProactiveMatchResponse.AssociatedDod1 =
          db.GetNullableDate(reader, 38);
        entities.FcrProactiveMatchResponse.AssociatedSsn2 =
          db.GetNullableString(reader, 39);
        entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
          db.GetNullableString(reader, 40);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
          db.GetNullableString(reader, 41);
        entities.FcrProactiveMatchResponse.AssociatedLastName2 =
          db.GetNullableString(reader, 42);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
          db.GetNullableString(reader, 43);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
          db.GetNullableString(reader, 44);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
          db.GetNullableString(reader, 45);
        entities.FcrProactiveMatchResponse.AssociatedDob2 =
          db.GetNullableDate(reader, 46);
        entities.FcrProactiveMatchResponse.AssociatedDod2 =
          db.GetNullableDate(reader, 47);
        entities.FcrProactiveMatchResponse.AssociatedSsn3 =
          db.GetNullableString(reader, 48);
        entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
          db.GetNullableString(reader, 49);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
          db.GetNullableString(reader, 50);
        entities.FcrProactiveMatchResponse.AssociatedLastName3 =
          db.GetNullableString(reader, 51);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
          db.GetNullableString(reader, 52);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
          db.GetNullableString(reader, 53);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
          db.GetNullableString(reader, 54);
        entities.FcrProactiveMatchResponse.AssociatedDob3 =
          db.GetNullableDate(reader, 55);
        entities.FcrProactiveMatchResponse.AssociatedDod3 =
          db.GetNullableDate(reader, 56);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
          db.GetNullableString(reader, 57);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
          db.GetNullableString(reader, 58);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
          db.GetNullableString(reader, 59);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 60);
        entities.FcrProactiveMatchResponse.DateReceived =
          db.GetNullableDate(reader, 61);
        entities.FcrProactiveMatchResponse.LastName =
          db.GetNullableString(reader, 62);
        entities.FcrProactiveMatchResponse.Populated = true;
      });
  }

  private bool ReadFcrProactiveMatchResponse3()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return Read("ReadFcrProactiveMatchResponse3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "stateMemberId", local.FcrFormatPersonNumber.Text15);
        db.SetInt32(
          command, "identifier", local.FcrProactiveMatchResponse.Identifier);
      },
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.ActionTypeCode =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
          db.GetNullableString(reader, 1);
        entities.FcrProactiveMatchResponse.UserField =
          db.GetNullableString(reader, 2);
        entities.FcrProactiveMatchResponse.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.FcrProactiveMatchResponse.FirstName =
          db.GetNullableString(reader, 4);
        entities.FcrProactiveMatchResponse.MiddleName =
          db.GetNullableString(reader, 5);
        entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
          db.GetNullableString(reader, 6);
        entities.FcrProactiveMatchResponse.StateMemberId =
          db.GetNullableString(reader, 7);
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 8);
        entities.FcrProactiveMatchResponse.ResponseCode =
          db.GetNullableString(reader, 9);
        entities.FcrProactiveMatchResponse.MatchedCaseId =
          db.GetNullableString(reader, 10);
        entities.FcrProactiveMatchResponse.MatchedCaseType =
          db.GetNullableString(reader, 11);
        entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
          db.GetNullableString(reader, 12);
        entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
          db.GetNullableDate(reader, 13);
        entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
          db.GetNullableString(reader, 14);
        entities.FcrProactiveMatchResponse.MatchedParticipantType =
          db.GetNullableString(reader, 15);
        entities.FcrProactiveMatchResponse.MatchedMemberId =
          db.GetNullableString(reader, 16);
        entities.FcrProactiveMatchResponse.MatchedPersonDod =
          db.GetNullableDate(reader, 17);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
          db.GetNullableString(reader, 18);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
          db.GetNullableString(reader, 19);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
          db.GetNullableString(reader, 20);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
          db.GetNullableString(reader, 21);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
          db.GetNullableString(reader, 22);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
          db.GetNullableString(reader, 23);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
          db.GetNullableString(reader, 24);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
          db.GetNullableString(reader, 25);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
          db.GetNullableString(reader, 26);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
          db.GetNullableString(reader, 27);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
          db.GetNullableString(reader, 28);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
          db.GetNullableString(reader, 29);
        entities.FcrProactiveMatchResponse.AssociatedSsn1 =
          db.GetNullableString(reader, 30);
        entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
          db.GetNullableString(reader, 31);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
          db.GetNullableString(reader, 32);
        entities.FcrProactiveMatchResponse.AssociatedLastName1 =
          db.GetNullableString(reader, 33);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
          db.GetNullableString(reader, 34);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
          db.GetNullableString(reader, 35);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
          db.GetNullableString(reader, 36);
        entities.FcrProactiveMatchResponse.AssociatedDob1 =
          db.GetNullableDate(reader, 37);
        entities.FcrProactiveMatchResponse.AssociatedDod1 =
          db.GetNullableDate(reader, 38);
        entities.FcrProactiveMatchResponse.AssociatedSsn2 =
          db.GetNullableString(reader, 39);
        entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
          db.GetNullableString(reader, 40);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
          db.GetNullableString(reader, 41);
        entities.FcrProactiveMatchResponse.AssociatedLastName2 =
          db.GetNullableString(reader, 42);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
          db.GetNullableString(reader, 43);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
          db.GetNullableString(reader, 44);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
          db.GetNullableString(reader, 45);
        entities.FcrProactiveMatchResponse.AssociatedDob2 =
          db.GetNullableDate(reader, 46);
        entities.FcrProactiveMatchResponse.AssociatedDod2 =
          db.GetNullableDate(reader, 47);
        entities.FcrProactiveMatchResponse.AssociatedSsn3 =
          db.GetNullableString(reader, 48);
        entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
          db.GetNullableString(reader, 49);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
          db.GetNullableString(reader, 50);
        entities.FcrProactiveMatchResponse.AssociatedLastName3 =
          db.GetNullableString(reader, 51);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
          db.GetNullableString(reader, 52);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
          db.GetNullableString(reader, 53);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
          db.GetNullableString(reader, 54);
        entities.FcrProactiveMatchResponse.AssociatedDob3 =
          db.GetNullableDate(reader, 55);
        entities.FcrProactiveMatchResponse.AssociatedDod3 =
          db.GetNullableDate(reader, 56);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
          db.GetNullableString(reader, 57);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
          db.GetNullableString(reader, 58);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
          db.GetNullableString(reader, 59);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 60);
        entities.FcrProactiveMatchResponse.DateReceived =
          db.GetNullableDate(reader, 61);
        entities.FcrProactiveMatchResponse.LastName =
          db.GetNullableString(reader, 62);
        entities.FcrProactiveMatchResponse.Populated = true;
      });
  }

  private bool ReadFcrProactiveMatchResponse4()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return Read("ReadFcrProactiveMatchResponse4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "stateMemberId", local.FcrFormatPersonNumber.Text15);
        db.SetInt32(command, "identifier", local.Prev.Identifier);
      },
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.ActionTypeCode =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
          db.GetNullableString(reader, 1);
        entities.FcrProactiveMatchResponse.UserField =
          db.GetNullableString(reader, 2);
        entities.FcrProactiveMatchResponse.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.FcrProactiveMatchResponse.FirstName =
          db.GetNullableString(reader, 4);
        entities.FcrProactiveMatchResponse.MiddleName =
          db.GetNullableString(reader, 5);
        entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
          db.GetNullableString(reader, 6);
        entities.FcrProactiveMatchResponse.StateMemberId =
          db.GetNullableString(reader, 7);
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 8);
        entities.FcrProactiveMatchResponse.ResponseCode =
          db.GetNullableString(reader, 9);
        entities.FcrProactiveMatchResponse.MatchedCaseId =
          db.GetNullableString(reader, 10);
        entities.FcrProactiveMatchResponse.MatchedCaseType =
          db.GetNullableString(reader, 11);
        entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
          db.GetNullableString(reader, 12);
        entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
          db.GetNullableDate(reader, 13);
        entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
          db.GetNullableString(reader, 14);
        entities.FcrProactiveMatchResponse.MatchedParticipantType =
          db.GetNullableString(reader, 15);
        entities.FcrProactiveMatchResponse.MatchedMemberId =
          db.GetNullableString(reader, 16);
        entities.FcrProactiveMatchResponse.MatchedPersonDod =
          db.GetNullableDate(reader, 17);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
          db.GetNullableString(reader, 18);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
          db.GetNullableString(reader, 19);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
          db.GetNullableString(reader, 20);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
          db.GetNullableString(reader, 21);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
          db.GetNullableString(reader, 22);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
          db.GetNullableString(reader, 23);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
          db.GetNullableString(reader, 24);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
          db.GetNullableString(reader, 25);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
          db.GetNullableString(reader, 26);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
          db.GetNullableString(reader, 27);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
          db.GetNullableString(reader, 28);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
          db.GetNullableString(reader, 29);
        entities.FcrProactiveMatchResponse.AssociatedSsn1 =
          db.GetNullableString(reader, 30);
        entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
          db.GetNullableString(reader, 31);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
          db.GetNullableString(reader, 32);
        entities.FcrProactiveMatchResponse.AssociatedLastName1 =
          db.GetNullableString(reader, 33);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
          db.GetNullableString(reader, 34);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
          db.GetNullableString(reader, 35);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
          db.GetNullableString(reader, 36);
        entities.FcrProactiveMatchResponse.AssociatedDob1 =
          db.GetNullableDate(reader, 37);
        entities.FcrProactiveMatchResponse.AssociatedDod1 =
          db.GetNullableDate(reader, 38);
        entities.FcrProactiveMatchResponse.AssociatedSsn2 =
          db.GetNullableString(reader, 39);
        entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
          db.GetNullableString(reader, 40);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
          db.GetNullableString(reader, 41);
        entities.FcrProactiveMatchResponse.AssociatedLastName2 =
          db.GetNullableString(reader, 42);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
          db.GetNullableString(reader, 43);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
          db.GetNullableString(reader, 44);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
          db.GetNullableString(reader, 45);
        entities.FcrProactiveMatchResponse.AssociatedDob2 =
          db.GetNullableDate(reader, 46);
        entities.FcrProactiveMatchResponse.AssociatedDod2 =
          db.GetNullableDate(reader, 47);
        entities.FcrProactiveMatchResponse.AssociatedSsn3 =
          db.GetNullableString(reader, 48);
        entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
          db.GetNullableString(reader, 49);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
          db.GetNullableString(reader, 50);
        entities.FcrProactiveMatchResponse.AssociatedLastName3 =
          db.GetNullableString(reader, 51);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
          db.GetNullableString(reader, 52);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
          db.GetNullableString(reader, 53);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
          db.GetNullableString(reader, 54);
        entities.FcrProactiveMatchResponse.AssociatedDob3 =
          db.GetNullableDate(reader, 55);
        entities.FcrProactiveMatchResponse.AssociatedDod3 =
          db.GetNullableDate(reader, 56);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
          db.GetNullableString(reader, 57);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
          db.GetNullableString(reader, 58);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
          db.GetNullableString(reader, 59);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 60);
        entities.FcrProactiveMatchResponse.DateReceived =
          db.GetNullableDate(reader, 61);
        entities.FcrProactiveMatchResponse.LastName =
          db.GetNullableString(reader, 62);
        entities.FcrProactiveMatchResponse.Populated = true;
      });
  }

  private bool ReadFcrProactiveMatchResponse5()
  {
    entities.FcrProactiveMatchResponse.Populated = false;

    return Read("ReadFcrProactiveMatchResponse5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "stateMemberId", local.FcrFormatPersonNumber.Text15);
      },
      (db, reader) =>
      {
        entities.FcrProactiveMatchResponse.ActionTypeCode =
          db.GetNullableString(reader, 0);
        entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
          db.GetNullableString(reader, 1);
        entities.FcrProactiveMatchResponse.UserField =
          db.GetNullableString(reader, 2);
        entities.FcrProactiveMatchResponse.FipsCountyCode =
          db.GetNullableString(reader, 3);
        entities.FcrProactiveMatchResponse.FirstName =
          db.GetNullableString(reader, 4);
        entities.FcrProactiveMatchResponse.MiddleName =
          db.GetNullableString(reader, 5);
        entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
          db.GetNullableString(reader, 6);
        entities.FcrProactiveMatchResponse.StateMemberId =
          db.GetNullableString(reader, 7);
        entities.FcrProactiveMatchResponse.SubmittedCaseId =
          db.GetNullableString(reader, 8);
        entities.FcrProactiveMatchResponse.ResponseCode =
          db.GetNullableString(reader, 9);
        entities.FcrProactiveMatchResponse.MatchedCaseId =
          db.GetNullableString(reader, 10);
        entities.FcrProactiveMatchResponse.MatchedCaseType =
          db.GetNullableString(reader, 11);
        entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
          db.GetNullableString(reader, 12);
        entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
          db.GetNullableDate(reader, 13);
        entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
          db.GetNullableString(reader, 14);
        entities.FcrProactiveMatchResponse.MatchedParticipantType =
          db.GetNullableString(reader, 15);
        entities.FcrProactiveMatchResponse.MatchedMemberId =
          db.GetNullableString(reader, 16);
        entities.FcrProactiveMatchResponse.MatchedPersonDod =
          db.GetNullableDate(reader, 17);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
          db.GetNullableString(reader, 18);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
          db.GetNullableString(reader, 19);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
          db.GetNullableString(reader, 20);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
          db.GetNullableString(reader, 21);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
          db.GetNullableString(reader, 22);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
          db.GetNullableString(reader, 23);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
          db.GetNullableString(reader, 24);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
          db.GetNullableString(reader, 25);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
          db.GetNullableString(reader, 26);
        entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
          db.GetNullableString(reader, 27);
        entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
          db.GetNullableString(reader, 28);
        entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
          db.GetNullableString(reader, 29);
        entities.FcrProactiveMatchResponse.AssociatedSsn1 =
          db.GetNullableString(reader, 30);
        entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
          db.GetNullableString(reader, 31);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
          db.GetNullableString(reader, 32);
        entities.FcrProactiveMatchResponse.AssociatedLastName1 =
          db.GetNullableString(reader, 33);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
          db.GetNullableString(reader, 34);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
          db.GetNullableString(reader, 35);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
          db.GetNullableString(reader, 36);
        entities.FcrProactiveMatchResponse.AssociatedDob1 =
          db.GetNullableDate(reader, 37);
        entities.FcrProactiveMatchResponse.AssociatedDod1 =
          db.GetNullableDate(reader, 38);
        entities.FcrProactiveMatchResponse.AssociatedSsn2 =
          db.GetNullableString(reader, 39);
        entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
          db.GetNullableString(reader, 40);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
          db.GetNullableString(reader, 41);
        entities.FcrProactiveMatchResponse.AssociatedLastName2 =
          db.GetNullableString(reader, 42);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
          db.GetNullableString(reader, 43);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
          db.GetNullableString(reader, 44);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
          db.GetNullableString(reader, 45);
        entities.FcrProactiveMatchResponse.AssociatedDob2 =
          db.GetNullableDate(reader, 46);
        entities.FcrProactiveMatchResponse.AssociatedDod2 =
          db.GetNullableDate(reader, 47);
        entities.FcrProactiveMatchResponse.AssociatedSsn3 =
          db.GetNullableString(reader, 48);
        entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
          db.GetNullableString(reader, 49);
        entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
          db.GetNullableString(reader, 50);
        entities.FcrProactiveMatchResponse.AssociatedLastName3 =
          db.GetNullableString(reader, 51);
        entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
          db.GetNullableString(reader, 52);
        entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
          db.GetNullableString(reader, 53);
        entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
          db.GetNullableString(reader, 54);
        entities.FcrProactiveMatchResponse.AssociatedDob3 =
          db.GetNullableDate(reader, 55);
        entities.FcrProactiveMatchResponse.AssociatedDod3 =
          db.GetNullableDate(reader, 56);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
          db.GetNullableString(reader, 57);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
          db.GetNullableString(reader, 58);
        entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
          db.GetNullableString(reader, 59);
        entities.FcrProactiveMatchResponse.Identifier = db.GetInt32(reader, 60);
        entities.FcrProactiveMatchResponse.DateReceived =
          db.GetNullableDate(reader, 61);
        entities.FcrProactiveMatchResponse.LastName =
          db.GetNullableString(reader, 62);
        entities.FcrProactiveMatchResponse.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "count1", local.CountyFips.Count);
        db.SetInt32(command, "count2", local.StateFips.Count);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.StateAbbreviation = db.GetString(reader, 6);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", local.ServiceProvider.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

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
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
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

    private Common isSupervisor;
    private Common userAction;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A MatchedPersonAliasesGroup group.</summary>
    [Serializable]
    public class MatchedPersonAliasesGroup
    {
      /// <summary>
      /// A value of Galiases.
      /// </summary>
      [JsonPropertyName("galiases")]
      public CsePersonsWorkSet Galiases
      {
        get => galiases ??= new();
        set => galiases = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonsWorkSet galiases;
    }

    /// <summary>A AssociatedPersonsGroup group.</summary>
    [Serializable]
    public class AssociatedPersonsGroup
    {
      /// <summary>
      /// A value of Grole.
      /// </summary>
      [JsonPropertyName("grole")]
      public WorkArea Grole
      {
        get => grole ??= new();
        set => grole = value;
      }

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
      /// A value of GassociatedFcrProactiveMatchResponse.
      /// </summary>
      [JsonPropertyName("gassociatedFcrProactiveMatchResponse")]
      public FcrProactiveMatchResponse GassociatedFcrProactiveMatchResponse
      {
        get => gassociatedFcrProactiveMatchResponse ??= new();
        set => gassociatedFcrProactiveMatchResponse = value;
      }

      /// <summary>
      /// A value of GassociatedCsePerson.
      /// </summary>
      [JsonPropertyName("gassociatedCsePerson")]
      public CsePerson GassociatedCsePerson
      {
        get => gassociatedCsePerson ??= new();
        set => gassociatedCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private WorkArea grole;
      private CsePersonsWorkSet g;
      private FcrProactiveMatchResponse gassociatedFcrProactiveMatchResponse;
      private CsePerson gassociatedCsePerson;
    }

    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Case1 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Case1 g;
    }

    /// <summary>
    /// A value of MatchedPersonRole.
    /// </summary>
    [JsonPropertyName("matchedPersonRole")]
    public WorkArea MatchedPersonRole
    {
      get => matchedPersonRole ??= new();
      set => matchedPersonRole = value;
    }

    /// <summary>
    /// A value of MatchedPersonCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("matchedPersonCsePersonsWorkSet")]
    public CsePersonsWorkSet MatchedPersonCsePersonsWorkSet
    {
      get => matchedPersonCsePersonsWorkSet ??= new();
      set => matchedPersonCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MatchedPersonFcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("matchedPersonFcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse MatchedPersonFcrProactiveMatchResponse
    {
      get => matchedPersonFcrProactiveMatchResponse ??= new();
      set => matchedPersonFcrProactiveMatchResponse = value;
    }

    /// <summary>
    /// A value of MatchedPersonCsePerson.
    /// </summary>
    [JsonPropertyName("matchedPersonCsePerson")]
    public CsePerson MatchedPersonCsePerson
    {
      get => matchedPersonCsePerson ??= new();
      set => matchedPersonCsePerson = value;
    }

    /// <summary>
    /// Gets a value of MatchedPersonAliases.
    /// </summary>
    [JsonIgnore]
    public Array<MatchedPersonAliasesGroup> MatchedPersonAliases =>
      matchedPersonAliases ??= new(MatchedPersonAliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MatchedPersonAliases for json serialization.
    /// </summary>
    [JsonPropertyName("matchedPersonAliases")]
    [Computed]
    public IList<MatchedPersonAliasesGroup> MatchedPersonAliases_Json
    {
      get => matchedPersonAliases;
      set => MatchedPersonAliases.Assign(value);
    }

    /// <summary>
    /// Gets a value of AssociatedPersons.
    /// </summary>
    [JsonIgnore]
    public Array<AssociatedPersonsGroup> AssociatedPersons =>
      associatedPersons ??= new(AssociatedPersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AssociatedPersons for json serialization.
    /// </summary>
    [JsonPropertyName("associatedPersons")]
    [Computed]
    public IList<AssociatedPersonsGroup> AssociatedPersons_Json
    {
      get => associatedPersons;
      set => AssociatedPersons.Assign(value);
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public ScrollingAttributes Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of Response.
    /// </summary>
    [JsonPropertyName("response")]
    public WorkArea Response
    {
      get => response ??= new();
      set => response = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public TextWorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public FcrProactiveMatchResponse Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
    }

    private WorkArea matchedPersonRole;
    private CsePersonsWorkSet matchedPersonCsePersonsWorkSet;
    private FcrProactiveMatchResponse matchedPersonFcrProactiveMatchResponse;
    private CsePerson matchedPersonCsePerson;
    private Array<MatchedPersonAliasesGroup> matchedPersonAliases;
    private Array<AssociatedPersonsGroup> associatedPersons;
    private Fips fips;
    private ScrollingAttributes scrolling;
    private WorkArea response;
    private TextWorkArea message;
    private FcrProactiveMatchResponse next;
    private Array<CasesGroup> cases;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkerOnThisCase.
    /// </summary>
    [JsonPropertyName("workerOnThisCase")]
    public Common WorkerOnThisCase
    {
      get => workerOnThisCase ??= new();
      set => workerOnThisCase = value;
    }

    /// <summary>
    /// A value of SupervisorOnThisCase.
    /// </summary>
    [JsonPropertyName("supervisorOnThisCase")]
    public Common SupervisorOnThisCase
    {
      get => supervisorOnThisCase ??= new();
      set => supervisorOnThisCase = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of StateFips.
    /// </summary>
    [JsonPropertyName("stateFips")]
    public Common StateFips
    {
      get => stateFips ??= new();
      set => stateFips = value;
    }

    /// <summary>
    /// A value of CountyFips.
    /// </summary>
    [JsonPropertyName("countyFips")]
    public Common CountyFips
    {
      get => countyFips ??= new();
      set => countyFips = value;
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
    /// A value of FcrFormatPersonNumber.
    /// </summary>
    [JsonPropertyName("fcrFormatPersonNumber")]
    public WorkArea FcrFormatPersonNumber
    {
      get => fcrFormatPersonNumber ??= new();
      set => fcrFormatPersonNumber = value;
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

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public FcrProactiveMatchResponse Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
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

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private Common workerOnThisCase;
    private Common supervisorOnThisCase;
    private DateWorkArea current;
    private DateWorkArea nullDate;
    private DateWorkArea max;
    private Common caseFound;
    private Case1 prevCase;
    private Common stateFips;
    private Common countyFips;
    private ServiceProvider serviceProvider;
    private WorkArea fcrFormatPersonNumber;
    private Common common;
    private FcrProactiveMatchResponse prev;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private Common responseFound;
    private Code code;
    private CodeValue codeValue;
    private Common invalidCode;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public Case1 Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private Fips fips;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private Case1 zdel;
  }
#endregion
}
