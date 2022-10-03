// Program: SI_ASSIGN_PA_REFERRAL_TO_OSP, ID: 371789369, model: 746.
// Short name: SWE01920
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
/// A program: SI_ASSIGN_PA_REFERRAL_TO_OSP.
/// </para>
/// <para>
/// RESP: SRVINIT
/// Determines the appropriate Office-Service-
/// Provider to assign the PA Referral to.
/// </para>
/// </summary>
[Serializable]
public partial class SiAssignPaReferralToOsp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ASSIGN_PA_REFERRAL_TO_OSP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAssignPaReferralToOsp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAssignPaReferralToOsp.
  /// </summary>
  public SiAssignPaReferralToOsp(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************
    // Date      Developer	Description
    // 06/96	  J Howard      Initial development
    // 01/21/97  Sid Chowdhary	Modification to the Assignment logic.
    // 02/25/97  J Howard      Modification to the modification
    // 05/02/97  JeHoward      Changed the assignment logic for FC
    // 		        referrals.
    // 10/26/98  C Ott         Removed erroneous logic for assignment of Foster
    //                         Care.
    // **************************************************
    // ***************************************************************
    // 7/22/1999   C. Ott    'CI' like 'FC' program code does not require an '
    // AR'.
    //                       Added condition to logic.
    // ***************************************************************
    // ***************************************************************
    // 5/3/2001   M.Lachowicz    Use AR last name if you are not
    //                           able to assign on base of AP.
    //                           PR 118834.
    // ***************************************************************
    local.OfficeCount.Count = 0;

    if (ReadCseOrganization())
    {
      if (ReadProgram())
      {
        // ***************************************************************
        // Check the number of Offices for the County Service of type
        // Program for the County and Program specified in the PA Referral.
        // ***************************************************************
        foreach(var item in ReadCountyServiceOffice3())
        {
          ++local.OfficeCount.Count;
        }

        // *********************************************************
        // If there are multiple offices, pick the Field Office.
        // There can be only one field office proving service
        // to one county for one program. If there is no field
        // office, pick the next available office.
        // *********************************************************
        switch(local.OfficeCount.Count)
        {
          case 0:
            export.ErrorCode.Text8 = "NOOFFICE";

            break;
          case 1:
            export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;

            break;
          default:
            if (ReadCountyServiceOffice1())
            {
              export.Office.SystemGeneratedId =
                entities.Office.SystemGeneratedId;
            }
            else if (ReadCountyServiceOffice2())
            {
              export.Office.SystemGeneratedId =
                entities.Office.SystemGeneratedId;
            }
            else
            {
              export.ErrorCode.Text8 = "NOOFFICE";
            }

            break;
        }
      }
      else
      {
        ExitState = "INVALID_PROGRAM";
        export.ErrorCode.Text8 = "NOPGMALP";

        return;
      }
    }
    else
    {
      ExitState = "CSE_ORGANIZATION_NF";
      export.ErrorCode.Text8 = "NOCSEORG";

      return;
    }

    if (!ReadPaReferral())
    {
      ExitState = "PA_REFERRAL_NF";
      export.ErrorCode.Text8 = "PAREF_NF";

      return;
    }

    // *********************************************
    // Alpha assignment rules. Determine the last
    // name to be used for assignment.
    // *********************************************
    if (import.Ap.Count == 1)
    {
      // **********************************************
      // If there is only one AP, this AP's last name is used.
      // **********************************************
      if (ReadPaReferralParticipant1())
      {
        MovePaReferralParticipant(entities.PaReferralParticipant,
          local.AlphaMatch);
      }
      else
      {
        ExitState = "PA_REFERRRAL_PARTICIPANT_NF";
        export.ErrorCode.Text8 = "APPARTNF";
      }
    }
    else
    {
      // **********************************************
      // If there are no APs or more than one APs, the AR's last name is used 
      // unless it's a FC referral, in which case the CH's name is used.
      // **********************************************
      // ***************************************************************
      // 7/22/1999   C. Ott   'CI' program code should be treated the same as '
      // FC'.
      // ***************************************************************
      if (Equal(import.PaReferral.PgmCode, "FC") || Equal
        (import.PaReferral.PgmCode, "CI"))
      {
        if (ReadPaReferralParticipant3())
        {
          MovePaReferralParticipant(entities.PaReferralParticipant,
            local.AlphaMatch);
        }
        else
        {
          ExitState = "PA_REFERRRAL_PARTICIPANT_NF";
          export.ErrorCode.Text8 = "CHPARTNF";
        }
      }
      else if (ReadPaReferralParticipant2())
      {
        MovePaReferralParticipant(entities.PaReferralParticipant,
          local.AlphaMatch);
      }
      else
      {
        ExitState = "PA_REFERRRAL_PARTICIPANT_NF";
        export.ErrorCode.Text8 = "ARPARTNF";
      }
    }

    // 05/03/2001 M.L Start
    local.FirstCharacter.Text1 = Substring(local.AlphaMatch.LastName, 1, 1);

    if (AsChar(local.FirstCharacter.Text1) < 'A' || AsChar
      (local.FirstCharacter.Text1) > 'Z')
    {
      if (ReadPaReferralParticipant2())
      {
        MovePaReferralParticipant(entities.PaReferralParticipant,
          local.AlphaMatch);
        local.FirstCharacter.Text1 = Substring(local.AlphaMatch.LastName, 1, 1);

        if (AsChar(local.FirstCharacter.Text1) < 'A' || AsChar
          (local.FirstCharacter.Text1) > 'Z')
        {
          local.AlphaMatch.LastName = "LLL";
        }
      }
      else
      {
        local.AlphaMatch.LastName = "LLL";
        ExitState = "PA_REFERRRAL_PARTICIPANT_NF";
        export.ErrorCode.Text8 = "ARPARTNF";
      }
    }

    // 05/03/2001 M.L End
    // *********************************************
    // If there were no errors, determine OSP for assignment.
    // *********************************************
    if (!IsEmpty(export.ErrorCode.Text8))
    {
      return;
    }

    if (ReadOffice())
    {
      foreach(var item in ReadOfficeCaseloadAssignment())
      {
        // *********************************************
        // Match first initial, if the last name matches the alpha value.
        // *********************************************
        if (!IsEmpty(entities.OfficeCaseloadAssignment.BeginningFirstIntial))
        {
          if (Equal(local.AlphaMatch.LastName, 1,
            OfficeCaseloadAssignment.BeginingAlpha_MaxLength,
            entities.OfficeCaseloadAssignment.BeginingAlpha))
          {
            if (CharAt(local.AlphaMatch.FirstName, 1) < AsChar
              (entities.OfficeCaseloadAssignment.BeginningFirstIntial))
            {
              continue;
            }
          }
        }

        if (!IsEmpty(entities.OfficeCaseloadAssignment.EndingFirstInitial))
        {
          if (Equal(local.AlphaMatch.LastName, 1,
            OfficeCaseloadAssignment.EndingAlpha_MaxLength,
            entities.OfficeCaseloadAssignment.EndingAlpha))
          {
            if (CharAt(local.AlphaMatch.FirstName, 1) > AsChar
              (entities.OfficeCaseloadAssignment.EndingFirstInitial))
            {
              continue;
            }
          }
        }

        if (ReadOfficeServiceProvider())
        {
          try
          {
            CreatePaReferralAssignment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PA_REF_ASSIGN_CREATE_FAILED";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PA_REF_ASSIGN_CREATE_FAILED";

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
          export.ErrorCode.Text8 = "NOOFSRVP";
        }

        return;
      }
    }
    else
    {
      export.ErrorCode.Text8 = "NOOFFICE";
    }
  }

  private static void MovePaReferralParticipant(PaReferralParticipant source,
    PaReferralParticipant target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.Mi = source.Mi;
  }

  private void CreatePaReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = new DateTime(2099, 12, 31);
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var pafNo = entities.PaReferral.Number;
    var pafType = entities.PaReferral.Type1;
    var pafTstamp = entities.PaReferral.CreatedTimestamp;

    entities.PaReferralAssignment.Populated = false;
    Update("CreatePaReferralAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetString(command, "pafNo", pafNo);
        db.SetString(command, "pafType", pafType);
        db.SetDateTime(command, "pafTstamp", pafTstamp);
      });

    entities.PaReferralAssignment.ReasonCode = reasonCode;
    entities.PaReferralAssignment.OverrideInd = overrideInd;
    entities.PaReferralAssignment.EffectiveDate = effectiveDate;
    entities.PaReferralAssignment.DiscontinueDate = discontinueDate;
    entities.PaReferralAssignment.CreatedBy = createdBy;
    entities.PaReferralAssignment.CreatedTimestamp = createdTimestamp;
    entities.PaReferralAssignment.LastUpdatedBy = createdBy;
    entities.PaReferralAssignment.LastUpdatedTimestamp = createdTimestamp;
    entities.PaReferralAssignment.SpdId = spdId;
    entities.PaReferralAssignment.OffId = offId;
    entities.PaReferralAssignment.OspCode = ospCode;
    entities.PaReferralAssignment.OspDate = ospDate;
    entities.PaReferralAssignment.PafNo = pafNo;
    entities.PaReferralAssignment.PafType = pafType;
    entities.PaReferralAssignment.PafTstamp = pafTstamp;
    entities.PaReferralAssignment.Populated = true;
  }

  private bool ReadCountyServiceOffice1()
  {
    entities.CountyService.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCountyServiceOffice1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Office.TypeCode = db.GetString(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.CountyService.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadCountyServiceOffice2()
  {
    entities.CountyService.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCountyServiceOffice2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Office.TypeCode = db.GetString(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.CountyService.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCountyServiceOffice3()
  {
    entities.CountyService.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadCountyServiceOffice3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetNullableInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Office.TypeCode = db.GetString(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.CountyService.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "organztnId", import.PaReferral.KsCounty ?? "");
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableString(
          command, "lastName", local.AlphaMatch.LastName ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 13);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 16);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 17);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId1", entities.Office.SystemGeneratedId);
        db.SetString(
          command, "roleCode",
          entities.OfficeCaseloadAssignment.OspRoleCode ?? "");
        db.SetDate(
          command, "effectiveDate",
          entities.OfficeCaseloadAssignment.OspEffectiveDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId2",
          entities.OfficeCaseloadAssignment.OffDGeneratedId.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeCaseloadAssignment.SpdGeneratedId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.PaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetString(command, "type", import.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.PaReferral.Number = db.GetString(reader, 0);
        entities.PaReferral.Type1 = db.GetString(reader, 1);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaReferral.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant1()
  {
    entities.PaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.PaReferral.Number);
        db.SetString(command, "pafType", entities.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.PaReferralParticipant.Identifier = db.GetInt32(reader, 0);
        entities.PaReferralParticipant.CreatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.PaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 2);
        entities.PaReferralParticipant.Relationship =
          db.GetNullableString(reader, 3);
        entities.PaReferralParticipant.Sex = db.GetNullableString(reader, 4);
        entities.PaReferralParticipant.Dob = db.GetNullableDate(reader, 5);
        entities.PaReferralParticipant.LastName =
          db.GetNullableString(reader, 6);
        entities.PaReferralParticipant.FirstName =
          db.GetNullableString(reader, 7);
        entities.PaReferralParticipant.Mi = db.GetNullableString(reader, 8);
        entities.PaReferralParticipant.Ssn = db.GetNullableString(reader, 9);
        entities.PaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 10);
        entities.PaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 11);
        entities.PaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 12);
        entities.PaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 13);
        entities.PaReferralParticipant.CreatedBy =
          db.GetNullableString(reader, 14);
        entities.PaReferralParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.PaReferralParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.PaReferralParticipant.PreNumber = db.GetString(reader, 17);
        entities.PaReferralParticipant.GoodCauseStatus =
          db.GetNullableString(reader, 18);
        entities.PaReferralParticipant.PafType = db.GetString(reader, 19);
        entities.PaReferralParticipant.PafTstamp = db.GetDateTime(reader, 20);
        entities.PaReferralParticipant.Role = db.GetNullableString(reader, 21);
        entities.PaReferralParticipant.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant2()
  {
    entities.PaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.PaReferral.Number);
        db.SetString(command, "pafType", entities.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.PaReferralParticipant.Identifier = db.GetInt32(reader, 0);
        entities.PaReferralParticipant.CreatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.PaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 2);
        entities.PaReferralParticipant.Relationship =
          db.GetNullableString(reader, 3);
        entities.PaReferralParticipant.Sex = db.GetNullableString(reader, 4);
        entities.PaReferralParticipant.Dob = db.GetNullableDate(reader, 5);
        entities.PaReferralParticipant.LastName =
          db.GetNullableString(reader, 6);
        entities.PaReferralParticipant.FirstName =
          db.GetNullableString(reader, 7);
        entities.PaReferralParticipant.Mi = db.GetNullableString(reader, 8);
        entities.PaReferralParticipant.Ssn = db.GetNullableString(reader, 9);
        entities.PaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 10);
        entities.PaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 11);
        entities.PaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 12);
        entities.PaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 13);
        entities.PaReferralParticipant.CreatedBy =
          db.GetNullableString(reader, 14);
        entities.PaReferralParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.PaReferralParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.PaReferralParticipant.PreNumber = db.GetString(reader, 17);
        entities.PaReferralParticipant.GoodCauseStatus =
          db.GetNullableString(reader, 18);
        entities.PaReferralParticipant.PafType = db.GetString(reader, 19);
        entities.PaReferralParticipant.PafTstamp = db.GetDateTime(reader, 20);
        entities.PaReferralParticipant.Role = db.GetNullableString(reader, 21);
        entities.PaReferralParticipant.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant3()
  {
    entities.PaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.PaReferral.Number);
        db.SetString(command, "pafType", entities.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.PaReferralParticipant.Identifier = db.GetInt32(reader, 0);
        entities.PaReferralParticipant.CreatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.PaReferralParticipant.AbsenceCode =
          db.GetNullableString(reader, 2);
        entities.PaReferralParticipant.Relationship =
          db.GetNullableString(reader, 3);
        entities.PaReferralParticipant.Sex = db.GetNullableString(reader, 4);
        entities.PaReferralParticipant.Dob = db.GetNullableDate(reader, 5);
        entities.PaReferralParticipant.LastName =
          db.GetNullableString(reader, 6);
        entities.PaReferralParticipant.FirstName =
          db.GetNullableString(reader, 7);
        entities.PaReferralParticipant.Mi = db.GetNullableString(reader, 8);
        entities.PaReferralParticipant.Ssn = db.GetNullableString(reader, 9);
        entities.PaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 10);
        entities.PaReferralParticipant.InsurInd =
          db.GetNullableString(reader, 11);
        entities.PaReferralParticipant.PatEstInd =
          db.GetNullableString(reader, 12);
        entities.PaReferralParticipant.BeneInd =
          db.GetNullableString(reader, 13);
        entities.PaReferralParticipant.CreatedBy =
          db.GetNullableString(reader, 14);
        entities.PaReferralParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.PaReferralParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.PaReferralParticipant.PreNumber = db.GetString(reader, 17);
        entities.PaReferralParticipant.GoodCauseStatus =
          db.GetNullableString(reader, 18);
        entities.PaReferralParticipant.PafType = db.GetString(reader, 19);
        entities.PaReferralParticipant.PafTstamp = db.GetDateTime(reader, 20);
        entities.PaReferralParticipant.Role = db.GetNullableString(reader, 21);
        entities.PaReferralParticipant.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.
          SetNullableString(command, "pgmCode", import.PaReferral.PgmCode ?? "");
          
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;
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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private PaReferral paReferral;
    private Common ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrorCode.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public TextWorkArea ErrorCode
    {
      get => errorCode ??= new();
      set => errorCode = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private TextWorkArea errorCode;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FirstCharacter.
    /// </summary>
    [JsonPropertyName("firstCharacter")]
    public TextWorkArea FirstCharacter
    {
      get => firstCharacter ??= new();
      set => firstCharacter = value;
    }

    /// <summary>
    /// A value of AlphaMatch.
    /// </summary>
    [JsonPropertyName("alphaMatch")]
    public PaReferralParticipant AlphaMatch
    {
      get => alphaMatch ??= new();
      set => alphaMatch = value;
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
    /// A value of OfficeCount.
    /// </summary>
    [JsonPropertyName("officeCount")]
    public Common OfficeCount
    {
      get => officeCount ??= new();
      set => officeCount = value;
    }

    private TextWorkArea firstCharacter;
    private PaReferralParticipant alphaMatch;
    private Common common;
    private Common officeCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
    }

    /// <summary>
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private PaReferralAssignment paReferralAssignment;
    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
    private Program program;
    private CountyService countyService;
    private CseOrganization cseOrganization;
    private ServiceProvider serviceProvider;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }
#endregion
}
