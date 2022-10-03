// Program: OE_LGRQ_DISPLAY_LEGAL_REQUESTS, ID: 371913154, model: 746.
// Short name: SWE00937
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
/// A program: OE_LGRQ_DISPLAY_LEGAL_REQUESTS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block reads and populates export views of Legal Requests for 
/// display.
/// </para>
/// </summary>
[Serializable]
public partial class OeLgrqDisplayLegalRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_DISPLAY_LEGAL_REQUESTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqDisplayLegalRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqDisplayLegalRequests.
  /// </summary>
  public OeLgrqDisplayLegalRequests(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------------------------------------
    // 		                              
    // P U R P O S E
    // The purpose of this cab is to display legal referrals for a cse case.  
    // The sort order for
    // displayed referrals is as follows:
    //    a. referrals in Sent and Open status display first sorted descending 
    // by identifier.
    //    b. referrals in Closed, Rejected, or Withdrawn status are displayed 
    // next sorted
    //      descending by identifier.
    // This cab processes the Display, Prev, and Next commands for LGRQ.
    // ----------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ------------	--------------	
    // ------------------------------------------------------------------------
    // 01/31/97  Raju - MTW			Display includes reason 5 codes
    // 02/03/97  Raju - MTW			Read attorney assigned to referral
    // 02/13/97  Raju - MTW			Import service view not needed
    // 04/30/97  G P Kim			Change Current Date
    // 06/16/97  SID CHOWDHARY			DISPLAY CLOSED CASES.
    // 09/16/99  David Lowry	PR H00073571	Organization name not displayed
    // 01/26/00  Carl Galka	PR83111 	Allow entry of Court Case number
    // 08/10/00  GVandy	PR100983  	Display inactive APs on open cases.
    // 03/15/01  GVandy	PR112357	Correct read for foreign tribunals.
    // 04/19/01  GVandy	WR 251		Return attorney user id, office, and role code.
    // 04/08/02  GVandy	WR 20184	Display Sent and Open referrals first.  
    // Restructured existing logic.
    // 12/03/10  GVandy	CQ109		Remove referral reason 5 from all views.
    // ----------------------------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // -- Find the CSE case and return a flag indicating the status.
    if (ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'O')
      {
        export.CaseOpen.Flag = "Y";
      }
      else if (AsChar(entities.Case1.Status) == 'C')
      {
        export.CaseOpen.Flag = "N";
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // -- Find the AP on the case.
    if (ReadCsePerson1())
    {
      export.Ap.Number = entities.CsePerson.Number;
    }
    else
    {
      if (ReadCaseRoleCsePerson())
      {
        export.Ap.Number = entities.CsePerson.Number;

        if (AsChar(export.CaseOpen.Flag) == 'Y')
        {
          export.ApInactive.Flag = "Y";
        }

        goto Read1;
      }

      ExitState = "AP_FOR_CASE_NF";

      return;
    }

Read1:

    // -- Retrieve the AP name from Adabas.
    UseSiReadCsePerson1();

    // -- Find the AR on the case.
    if (ReadCsePerson2())
    {
      export.Ar.Number = entities.CsePerson.Number;
    }
    else
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        if (ReadCsePersonCaseRole1())
        {
          export.Ar.Number = entities.CsePerson.Number;

          goto Read2;
        }
      }

      ExitState = "AR_DB_ERROR_NF";

      return;
    }

Read2:

    // -- Retrieve the AR name from Adabas.
    UseSiReadCsePerson2();

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ---------------------------------------------
        // Command DISPLAY displays given legal request
        // ---------------------------------------------
        // -- Read for referrals in Sent or Open status first.
        if (ReadLegalReferral6())
        {
          export.LegalReferral.Assign(entities.LegalReferral);

          break;
        }

        // -- If there are no referrals in Sent or Open status then read for 
        // referrals in
        //    Closed, Rejected, or Withdrawn status.
        if (ReadLegalReferral8())
        {
          export.LegalReferral.Assign(entities.LegalReferral);

          break;
        }

        ExitState = "OE0000_LEGAL_REQUEST_NF_FOR_CASE";

        return;
      case "PREV":
        // ---------------------------------------------
        // Command PREV displays previous legal request
        // ---------------------------------------------
        if (AsChar(import.LegalReferral.Status) == 'S' || AsChar
          (import.LegalReferral.Status) == 'O')
        {
          // -- Read for a previous referral in Sent or Open status first.
          if (ReadLegalReferral4())
          {
            export.LegalReferral.Assign(entities.LegalReferral);

            break;
          }

          // -- If there are no previous referrals in Sent or Open status then 
          // read for referrals in
          //    Closed, Rejected, or Withdrawn status.
          if (ReadLegalReferral7())
          {
            export.LegalReferral.Assign(entities.LegalReferral);

            break;
          }
        }
        else
        {
          // -- Read for a previous referral in Closed, Rejected, or Withdrawn 
          // status.
          if (ReadLegalReferral5())
          {
            export.LegalReferral.Assign(entities.LegalReferral);

            break;
          }
        }

        // *** This should never happen. The calling PrAD checks for +/- scroll 
        // flags before
        //     calling the display cab.
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        // ---------------------------------------------
        // Command NEXT displays next legal request
        // ---------------------------------------------
        if (AsChar(import.LegalReferral.Status) == 'S' || AsChar
          (import.LegalReferral.Status) == 'O')
        {
          // -- Read for a more recent referral in Sent or Open status.
          if (ReadLegalReferral1())
          {
            export.LegalReferral.Assign(entities.LegalReferral);

            break;
          }
        }
        else
        {
          // -- Read for a more recent referral in Closed, Rejected, or 
          // Withdrawn status first.
          if (ReadLegalReferral2())
          {
            export.LegalReferral.Assign(entities.LegalReferral);

            break;
          }

          // -- If there are no more referrals in Closed, Rejected, or Withdrawn
          // status then read for
          //    referrals in Sent or Open status.
          if (ReadLegalReferral3())
          {
            export.LegalReferral.Assign(entities.LegalReferral);

            break;
          }
        }

        // *** This should never happen. The calling PrAD checks for +/- scroll 
        // flags before
        //     calling the display cab.
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      default:
        break;
    }

    if (export.LegalReferral.TribunalId.GetValueOrDefault() != 0)
    {
      // *** Retrieve FIPS State, County and Country.
      if (ReadFips())
      {
        MoveFips(entities.Fips, export.Fips);
      }

      if (ReadFipsTribAddress())
      {
        export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
      }
    }

    // -- Find all the people on the legal request.
    export.CaseRoleReferred.Index = 0;
    export.CaseRoleReferred.Clear();

    foreach(var item in ReadCsePersonCaseRole2())
    {
      export.CaseRoleReferred.Update.CaseRole.Assign(entities.CaseRole);
      export.CaseRoleReferred.Update.CsePerson.Number =
        entities.CsePerson.Number;
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.CsePerson.Type1 = "";

      // -- Retrieve the persons name from Adabas.
      UseSiReadCsePerson3();

      if (Equal(export.CaseRoleReferred.Item.CaseRole.Type1, "AR") && AsChar
        (local.CsePerson.Type1) == 'O')
      {
        // -- Return the organization name if the AR is an organization.
        export.CaseRoleReferred.Update.CsePersonsWorkSet.FirstName =
          local.CsePersonsWorkSet.FormattedName;
      }
      else
      {
        export.CaseRoleReferred.Update.CsePersonsWorkSet.FirstName =
          local.CsePersonsWorkSet.FirstName;
      }

      if (Equal(export.CaseRoleReferred.Item.CaseRole.Type1, "AP"))
      {
        // -- Even though an AP was previously read, move this AP to the export 
        // AP view.  This insures that
        // if there are more than one AP on the case that the AP actually listed
        // on the Referral shows
        // at the top of the screen.
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.Ap);
      }

      export.CaseRoleReferred.Next();
    }

    // -- Find all the comments on the legal referral.
    export.LrCommentLines.Index = 0;
    export.LrCommentLines.Clear();

    foreach(var item in ReadLegalReferralComment())
    {
      MoveLegalReferralComment(entities.LegalReferralComment,
        export.LrCommentLines.Update.Comment);
      export.LrCommentLines.Next();
    }

    // -- Find all the attachment info on the legal referral.
    export.LrAttchmts.Index = 0;
    export.LrAttchmts.Clear();

    foreach(var item in ReadLegalReferralAttachment())
    {
      MoveLegalReferralAttachment(entities.LegalReferralAttachment,
        export.LrAttchmts.Update.DetailLrAttchmts);
      export.LrAttchmts.Next();
    }

    // -- Find the service provider assigned to the legal referral.
    if (ReadOfficeOfficeServiceProviderServiceProvider())
    {
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        export.OfficeServiceProvider);
      export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
      export.ServiceProvider.Assign(entities.ServiceProvider);
    }
    else
    {
      // -- Continue.  Do not set an exit state here.
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalReferralAttachment(
    LegalReferralAttachment source, LegalReferralAttachment target)
  {
    target.LineNo = source.LineNo;
    target.CommentLine = source.CommentLine;
  }

  private static void MoveLegalReferralComment(LegalReferralComment source,
    LegalReferralComment target)
  {
    target.LineNo = source.LineNo;
    target.CommentLine = source.CommentLine;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ap);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.CsePerson.Type1 = useExport.CsePerson.Type1;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    return ReadEach("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        if (export.CaseRoleReferred.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          export.LegalReferral.TribunalId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId",
          export.LegalReferral.TribunalId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalReferral1()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral2()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral3()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral4()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral5()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral6()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral7()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral7",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral8()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral8",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.LegalReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAttachment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    return ReadEach("ReadLegalReferralAttachment",
      (db, command) =>
      {
        db.
          SetInt32(command, "lgrIdentifier", entities.LegalReferral.Identifier);
          
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        if (export.LrAttchmts.IsFull)
        {
          return false;
        }

        entities.LegalReferralAttachment.LgrIdentifier = db.GetInt32(reader, 0);
        entities.LegalReferralAttachment.CasNumber = db.GetString(reader, 1);
        entities.LegalReferralAttachment.LineNo = db.GetInt32(reader, 2);
        entities.LegalReferralAttachment.CommentLine = db.GetString(reader, 3);
        entities.LegalReferralAttachment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralComment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    return ReadEach("ReadLegalReferralComment",
      (db, command) =>
      {
        db.
          SetInt32(command, "lgrIdentifier", entities.LegalReferral.Identifier);
          
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        if (export.LrCommentLines.IsFull)
        {
          return false;
        }

        entities.LegalReferralComment.LgrIdentifier = db.GetInt32(reader, 0);
        entities.LegalReferralComment.CasNumber = db.GetString(reader, 1);
        entities.LegalReferralComment.LineNo = db.GetInt32(reader, 2);
        entities.LegalReferralComment.CommentLine = db.GetString(reader, 3);
        entities.LegalReferralComment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 4);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 5);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ServiceProvider.UserId = db.GetString(reader, 7);
        entities.ServiceProvider.LastName = db.GetString(reader, 8);
        entities.ServiceProvider.FirstName = db.GetString(reader, 9);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 10);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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

    private LegalReferral legalReferral;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CaseRoleReferredGroup group.</summary>
    [Serializable]
    public class CaseRoleReferredGroup
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private CaseRole caseRole;
    }

    /// <summary>A LrCommentLinesGroup group.</summary>
    [Serializable]
    public class LrCommentLinesGroup
    {
      /// <summary>
      /// A value of Comment.
      /// </summary>
      [JsonPropertyName("comment")]
      public LegalReferralComment Comment
      {
        get => comment ??= new();
        set => comment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralComment comment;
    }

    /// <summary>A LrAttchmtsGroup group.</summary>
    [Serializable]
    public class LrAttchmtsGroup
    {
      /// <summary>
      /// A value of DetailLrAttchmts.
      /// </summary>
      [JsonPropertyName("detailLrAttchmts")]
      public LegalReferralAttachment DetailLrAttchmts
      {
        get => detailLrAttchmts ??= new();
        set => detailLrAttchmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralAttachment detailLrAttchmts;
    }

    /// <summary>
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// Gets a value of CaseRoleReferred.
    /// </summary>
    [JsonIgnore]
    public Array<CaseRoleReferredGroup> CaseRoleReferred =>
      caseRoleReferred ??= new(CaseRoleReferredGroup.Capacity);

    /// <summary>
    /// Gets a value of CaseRoleReferred for json serialization.
    /// </summary>
    [JsonPropertyName("caseRoleReferred")]
    [Computed]
    public IList<CaseRoleReferredGroup> CaseRoleReferred_Json
    {
      get => caseRoleReferred;
      set => CaseRoleReferred.Assign(value);
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// Gets a value of LrCommentLines.
    /// </summary>
    [JsonIgnore]
    public Array<LrCommentLinesGroup> LrCommentLines => lrCommentLines ??= new(
      LrCommentLinesGroup.Capacity);

    /// <summary>
    /// Gets a value of LrCommentLines for json serialization.
    /// </summary>
    [JsonPropertyName("lrCommentLines")]
    [Computed]
    public IList<LrCommentLinesGroup> LrCommentLines_Json
    {
      get => lrCommentLines;
      set => LrCommentLines.Assign(value);
    }

    /// <summary>
    /// Gets a value of LrAttchmts.
    /// </summary>
    [JsonIgnore]
    public Array<LrAttchmtsGroup> LrAttchmts => lrAttchmts ??= new(
      LrAttchmtsGroup.Capacity);

    /// <summary>
    /// Gets a value of LrAttchmts for json serialization.
    /// </summary>
    [JsonPropertyName("lrAttchmts")]
    [Computed]
    public IList<LrAttchmtsGroup> LrAttchmts_Json
    {
      get => lrAttchmts;
      set => LrAttchmts.Assign(value);
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    private Common apInactive;
    private Common caseOpen;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private Array<CaseRoleReferredGroup> caseRoleReferred;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private LegalReferral legalReferral;
    private Array<LrCommentLinesGroup> lrCommentLines;
    private Array<LrAttchmtsGroup> lrAttchmts;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LegalReferralAttachment.
    /// </summary>
    [JsonPropertyName("legalReferralAttachment")]
    public LegalReferralAttachment LegalReferralAttachment
    {
      get => legalReferralAttachment ??= new();
      set => legalReferralAttachment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralComment.
    /// </summary>
    [JsonPropertyName("legalReferralComment")]
    public LegalReferralComment LegalReferralComment
    {
      get => legalReferralComment ??= new();
      set => legalReferralComment = value;
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

    private Tribunal tribunal;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private LegalReferralAssignment legalReferralAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private LegalReferralCaseRole legalReferralCaseRole;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private LegalReferralAttachment legalReferralAttachment;
    private LegalReferral legalReferral;
    private LegalReferralComment legalReferralComment;
    private Case1 case1;
  }
#endregion
}
