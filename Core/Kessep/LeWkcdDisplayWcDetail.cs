// Program: LE_WKCD_DISPLAY_WC_DETAIL, ID: 1625337370, model: 746.
// Short name: SWE00848
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_WKCD_DISPLAY_WC_DETAIL.
/// </summary>
[Serializable]
public partial class LeWkcdDisplayWcDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_WKCD_DISPLAY_WC_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeWkcdDisplayWcDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeWkcdDisplayWcDetail.
  /// </summary>
  public LeWkcdDisplayWcDetail(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 12/01/16  GVandy	CQ51923		Initial Code.
    // -------------------------------------------------------------------------------------
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    export.WorkersCompClaim.Identifier = import.WorkersCompClaim.Identifier;
    local.WorkersCompClaim.Identifier = import.WorkersCompClaim.Identifier;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    UseSiReadCsePerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!IsEmpty(local.CsePersonsWorkSet.Number))
    {
      export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    }

    if (import.WorkersCompClaim.Identifier == 0)
    {
      local.Common.Count = 0;

      // --Determine the number of workers comp claims for the NCP.
      ReadWorkersCompClaim3();

      switch(local.Common.Count)
      {
        case 0:
          return;
        case 1:
          break;
        default:
          ExitState = "ACO_NE0000_MUCT_GO_TO_WKCL";

          return;
      }

      // -- Only one claim exists.  Read the claim.
      if (!ReadWorkersCompClaim2())
      {
        ExitState = "WORKERS_COMP_CLAIM_NF";

        return;
      }
    }
    else
    {
      // -- Read the previously selected claim.
      if (!ReadWorkersCompClaim1())
      {
        ExitState = "WORKERS_COMP_CLAIM_NF";

        return;
      }
    }

    // -- Read all the addresses associated with the workers comp claim.
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      local.WorkersCompAddress.Assign(local.Null1);

      switch(local.Common.Count)
      {
        case 1:
          // --Claimant
          local.WorkersCompAddress.TypeCode = "CMT";

          break;
        case 2:
          // --Claimant Attorney
          local.WorkersCompAddress.TypeCode = "CAT";

          break;
        case 3:
          // --Employer
          local.WorkersCompAddress.TypeCode = "EMP";

          break;
        case 4:
          // --Insurance Carrier
          local.WorkersCompAddress.TypeCode = "INS";

          break;
        case 5:
          // --Insurance Carrier Attorney
          local.WorkersCompAddress.TypeCode = "IAT";

          break;
        default:
          break;
      }

      if (ReadWorkersCompAddress())
      {
        local.WorkersCompAddress.Assign(entities.WorkersCompAddress);
      }

      switch(local.Common.Count)
      {
        case 1:
          // --Claimant
          local.Claimant.Assign(local.WorkersCompAddress);

          break;
        case 2:
          // --Claimant Attorney
          local.ClaimantAttorney.Assign(local.WorkersCompAddress);

          break;
        case 3:
          // --Employer
          local.Employer.Assign(local.WorkersCompAddress);

          break;
        case 4:
          // --Insurance Carrier
          local.InsuranceCarrier.Assign(local.WorkersCompAddress);

          break;
        case 5:
          // --Insurance Carrier Attorney
          local.InsuranceAttorney.Assign(local.WorkersCompAddress);

          break;
        default:
          break;
      }
    }

    // -- Build the export repeating group to display as follows:
    // DATE OF LOSS: MMDDYYYY  CLAIM #: XXXXXXXXXXXXXXXXXXXXXXXXX  DOCKET #: 
    // XXXXXXX
    // CLAIMANT
    //      NAME: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //   ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //            XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // CLAIMANT ATTORNEY
    //      NAME: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //      FIRM: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //   ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //            XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // EMPLOYER
    //      FEIN: XXXXXXXXXX
    //      NAME: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //   ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //            XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // INSURANCE CARRIER
    //      NAME: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //   ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //            XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //  POLICY #: XXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //   CONTACT: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //     PHONE: (999)999-9999
    // INSURANCE CARRIER ATTORNEY
    //      NAME: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //   ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //            XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // THIRD PARTY ADMINISTRATOR
    //      NAME: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // ACCIDENT DETAIL
    //  DATE OF ACCIDENT: MMDDYYYY RETURNED TO WORK: MMDDYYYY DATE OF DEATH: 
    // MMDDYYYY
    //  CITY: XXXXXXXXXXXXXXXXXXXXXXXXX STATE: XXXXXXXXXXXX COUNTY: 
    // XXXXXXXXXXXXXXXXXXXX
    //  SEVERITY: XXXXXXXXXXXX
    //  DESCRIPTION: 
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //               
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //  WAGE AMOUNT: XXXXXXXXXXXX  WEEKLY RATE: XXXXXXXXXXXX
    //  COMPENSATION PAID: X DATE: MMDDYYYY
    //  CLAIM FILED DATE: MMDDYYYY AGENCY CLAIM NUMBER: XXXXXXXXXXXX
    for(export.Export1.Index = 0; export.Export1.Index < 60; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(export.Export1.Index + 1)
      {
        case 1:
          // DATE OF LOSS: MMDDYYYY  CLAIM #: XXXXXXXXXXXXXXXXXXXXXXXXX  DOCKET 
          // #: XXXXXXX
          local.Temp.Text8 =
            NumberToString(Month(entities.WorkersCompClaim.DateOfLoss), 14, 2) +
            NumberToString(Day(entities.WorkersCompClaim.DateOfLoss), 14, 2) + NumberToString
            (Year(entities.WorkersCompClaim.DateOfLoss), 12, 4);
          export.Export1.Update.G.Text80 = "DATE OF LOSS: " + local
            .Temp.Text8 + "  CLAIM #: " + entities
            .WorkersCompClaim.AdministrativeClaimNo + "  DOCKET #: " + entities
            .WorkersCompClaim.DocketNumber;

          break;
        case 2:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 3:
          // CLAIMANT
          export.Export1.Update.G.Text80 = "CLAIMANT";

          break;
        case 4:
          // NAME: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     NAME: " + entities
            .WorkersCompClaim.ClaimantFirstName;
          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + entities
            .WorkersCompClaim.ClaimantMiddleName;
          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + entities
            .WorkersCompClaim.ClaimantLastName;

          break;
        case 5:
          // ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "  ADDRESS: " + (
            local.Claimant.StreetAddress ?? "");

          break;
        case 6:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          if (!IsEmpty(local.Claimant.City))
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.Claimant.City ?? "");
            export.Export1.Update.G.Text80 =
              TrimEnd(export.Export1.Item.G.Text80) + ", " + (
                local.Claimant.State ?? "");
          }
          else
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.Claimant.State ?? "");
          }

          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + (
              local.Claimant.ZipCode ?? "");

          break;
        case 7:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 8:
          // CLAIMANT ATTORNEY
          export.Export1.Update.G.Text80 = "CLAIMANT ATTORNEY";

          break;
        case 9:
          // NAME: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     NAME: " + entities
            .WorkersCompClaim.ClaimantAttorneyFirstName;
          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + entities
            .WorkersCompClaim.ClaimantAttorneyLastName;

          break;
        case 10:
          // FIRM: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     FIRM: " + entities
            .WorkersCompClaim.ClaimantAttorneyFirmName;

          break;
        case 11:
          // ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "  ADDRESS: " + (
            local.ClaimantAttorney.StreetAddress ?? "");

          break;
        case 12:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          if (!IsEmpty(local.ClaimantAttorney.City))
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.ClaimantAttorney.City ?? "");
            export.Export1.Update.G.Text80 =
              TrimEnd(export.Export1.Item.G.Text80) + ", " + (
                local.ClaimantAttorney.State ?? "");
          }
          else
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.ClaimantAttorney.State ?? "");
          }

          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + (
              local.ClaimantAttorney.ZipCode ?? "");

          break;
        case 13:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 14:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 15:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 16:
          // EMPLOYER
          export.Export1.Update.G.Text80 = "EMPLOYER";

          break;
        case 17:
          // FEIN: XXXXXXXXXX
          export.Export1.Update.G.Text80 = "     FEIN: " + entities
            .WorkersCompClaim.EmployerFein;

          break;
        case 18:
          // NAME: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     NAME: " + entities
            .WorkersCompClaim.EmployerName;

          break;
        case 19:
          // ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "  ADDRESS: " + (
            local.Employer.StreetAddress ?? "");

          break;
        case 20:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          if (!IsEmpty(local.Employer.City))
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.Employer.City ?? "");
            export.Export1.Update.G.Text80 =
              TrimEnd(export.Export1.Item.G.Text80) + ", " + (
                local.Employer.State ?? "");
          }
          else
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.Employer.State ?? "");
          }

          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + (
              local.Employer.ZipCode ?? "");

          break;
        case 21:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 22:
          // INSURANCE CARRIER
          export.Export1.Update.G.Text80 = "INSURANCE CARRIER";

          break;
        case 23:
          // NAME: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     NAME: " + entities
            .WorkersCompClaim.InsurerName;

          break;
        case 24:
          // ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "  ADDRESS: " + (
            local.InsuranceCarrier.StreetAddress ?? "");

          break;
        case 25:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          if (!IsEmpty(local.InsuranceCarrier.City))
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.InsuranceCarrier.City ?? "");
            export.Export1.Update.G.Text80 =
              TrimEnd(export.Export1.Item.G.Text80) + ", " + (
                local.InsuranceCarrier.State ?? "");
          }
          else
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.InsuranceCarrier.State ?? "");
          }

          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + (
              local.InsuranceCarrier.ZipCode ?? "");

          break;
        case 26:
          // POLICY #: XXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = " POLICY #: " + entities
            .WorkersCompClaim.PolicyNumber;

          break;
        case 27:
          // CONTACT: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "  CONTACT: " + TrimEnd
            (entities.WorkersCompClaim.InsurerContactName1) + " " + TrimEnd
            (entities.WorkersCompClaim.InsurerContactName2);

          break;
        case 28:
          // PHONE: (999)999-9999
          export.Export1.Update.G.Text80 = "    PHONE: " + entities
            .WorkersCompClaim.InsurerContactPhone;

          break;
        case 29:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 30:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 31:
          // INSURANCE CARRIER ATTORNEY
          export.Export1.Update.G.Text80 = "INSURANCE CARRIER ATTORNEY";

          break;
        case 32:
          // NAME: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     NAME: " + entities
            .WorkersCompClaim.InsurerAttorneyFirmName;

          break;
        case 33:
          // ADDRESS: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "  ADDRESS: " + (
            local.InsuranceAttorney.StreetAddress ?? "");

          break;
        case 34:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          if (!IsEmpty(local.InsuranceAttorney.City))
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.InsuranceAttorney.City ?? "");
            export.Export1.Update.G.Text80 =
              TrimEnd(export.Export1.Item.G.Text80) + ", " + (
                local.InsuranceAttorney.State ?? "");
          }
          else
          {
            export.Export1.Update.G.Text80 = "           " + (
              local.InsuranceAttorney.State ?? "");
          }

          export.Export1.Update.G.Text80 =
            TrimEnd(export.Export1.Item.G.Text80) + " " + (
              local.InsuranceAttorney.ZipCode ?? "");

          break;
        case 35:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 36:
          // THIRD PARTY ADMINISTRATOR
          export.Export1.Update.G.Text80 = "THIRD PARTY ADMINISTRATOR";

          break;
        case 37:
          // NAME: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "     NAME: " + entities
            .WorkersCompClaim.ThirdPartyAdministratorName;

          break;
        case 38:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 39:
          // ACCIDENT DETAIL
          export.Export1.Update.G.Text80 = "ACCIDENT DETAIL";

          break;
        case 40:
          // DATE OF ACCIDENT: MMDDYYYY RETURNED TO WORK: MMDDYYYY DATE OF DEATH
          // : MMDDYYYY
          local.DateOfAccident.Text8 =
            NumberToString(Month(entities.WorkersCompClaim.DateOfAccident), 14,
            2) + NumberToString
            (Day(entities.WorkersCompClaim.DateOfAccident), 14, 2) + NumberToString
            (Year(entities.WorkersCompClaim.DateOfAccident), 12, 4);
          local.ReturnedToWorkDate.Text8 =
            NumberToString(Month(entities.WorkersCompClaim.ReturnedToWorkDate),
            14, 2) + NumberToString
            (Day(entities.WorkersCompClaim.ReturnedToWorkDate), 14, 2) + NumberToString
            (Year(entities.WorkersCompClaim.ReturnedToWorkDate), 12, 4);
          local.DateOfDeath.Text8 =
            NumberToString(Month(entities.WorkersCompClaim.DateOfDeath), 14, 2) +
            NumberToString
            (Day(entities.WorkersCompClaim.DateOfDeath), 14, 2) + NumberToString
            (Year(entities.WorkersCompClaim.DateOfDeath), 12, 4);
          export.Export1.Update.G.Text80 = " DATE OF ACCIDENT: " + local
            .DateOfAccident.Text8 + " RETURNED TO WORK: " + local
            .ReturnedToWorkDate.Text8 + " DATE OF DEATH: " + local
            .DateOfDeath.Text8;

          break;
        case 41:
          // CITY: XXXXXXXXXXXXXXXXXXXXXXXXX STATE: XXXXXXXXXXXX COUNTY: 
          // XXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = " CITY: " + entities
            .WorkersCompClaim.AccidentCity + " STATE: " + entities
            .WorkersCompClaim.AccidentState + " COUNTY: " + entities
            .WorkersCompClaim.AccidentCounty;

          break;
        case 42:
          // SEVERITY: XXXXXXXXXXXX
          export.Export1.Update.G.Text80 = " SEVERITY: " + entities
            .WorkersCompClaim.SeverityCodeDescription;

          break;
        case 43:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 44:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 45:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 46:
          // DESCRIPTION: 
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = " DESCRIPTION: " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 1, 64);

          break;
        case 47:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 65, 64);

          break;
        case 48:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 129, 64);

          break;
        case 49:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 193, 64);

          break;
        case 50:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 257, 64);

          break;
        case 51:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 321, 64);

          break;
        case 52:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 385, 64);

          break;
        case 53:
          // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
          export.Export1.Update.G.Text80 = "              " + Substring
            (entities.WorkersCompClaim.AccidentDescription, 449, 64);

          break;
        case 54:
          // Blank line
          export.Export1.Update.G.Text80 = "";

          break;
        case 55:
          // WAGE AMOUNT: XXXXXXXXXXXX  WEEKLY RATE: XXXXXXXXXXXX
          export.Export1.Update.G.Text80 = " WAGE AMOUNT: " + entities
            .WorkersCompClaim.WageAmount + "  WEEKLY RATE: " + entities
            .WorkersCompClaim.WeeklyRate;

          break;
        case 56:
          // COMPENSATION PAID: X DATE: MMDDYYYY
          local.Temp.Text8 =
            NumberToString(
              Month(entities.WorkersCompClaim.CompensationPaidDate), 14, 2) + NumberToString
            (Day(entities.WorkersCompClaim.CompensationPaidDate), 14, 2) + NumberToString
            (Year(entities.WorkersCompClaim.CompensationPaidDate), 12, 4);
          export.Export1.Update.G.Text80 = " COMPENSATION PAID: " + entities
            .WorkersCompClaim.CompensationPaidFlag + " DATE: " + local
            .Temp.Text8;

          break;
        case 57:
          // CLAIM FILED DATE: MMDDYYYY AGENCY CLAIM NUMBER: XXXXXXXXXXXX
          local.Temp.Text8 =
            NumberToString(Month(entities.WorkersCompClaim.ClaimFiledDate), 14,
            2) + NumberToString
            (Day(entities.WorkersCompClaim.ClaimFiledDate), 14, 2) + NumberToString
            (Year(entities.WorkersCompClaim.ClaimFiledDate), 12, 4);
          export.Export1.Update.G.Text80 = " CLAIM FILED DATE: " + local
            .Temp.Text8 + " AGENCY CLAIM NUMBER: " + entities
            .WorkersCompClaim.AgencyClaimNo;

          break;
        case 58:
          export.Export1.Update.G.Text80 = "";

          break;
        case 59:
          export.Export1.Update.G.Text80 = "";

          break;
        case 60:
          export.Export1.Update.G.Text80 = "";

          break;
        default:
          export.Export1.Update.G.Text80 = "";

          break;
      }
    }

    export.Export1.CheckIndex();
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadWorkersCompAddress()
  {
    System.Diagnostics.Debug.Assert(entities.WorkersCompClaim.Populated);
    entities.WorkersCompAddress.Populated = false;

    return Read("ReadWorkersCompAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.WorkersCompClaim.CspNumber);
        db.SetInt32(
          command, "wccIdentifier", entities.WorkersCompClaim.Identifier);
        db.SetString(command, "typeCode", local.WorkersCompAddress.TypeCode);
      },
      (db, reader) =>
      {
        entities.WorkersCompAddress.CspNumber = db.GetString(reader, 0);
        entities.WorkersCompAddress.WccIdentifier = db.GetInt32(reader, 1);
        entities.WorkersCompAddress.TypeCode = db.GetString(reader, 2);
        entities.WorkersCompAddress.StreetAddress =
          db.GetNullableString(reader, 3);
        entities.WorkersCompAddress.City = db.GetNullableString(reader, 4);
        entities.WorkersCompAddress.State = db.GetNullableString(reader, 5);
        entities.WorkersCompAddress.ZipCode = db.GetNullableString(reader, 6);
        entities.WorkersCompAddress.CreatedBy = db.GetString(reader, 7);
        entities.WorkersCompAddress.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.WorkersCompAddress.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.WorkersCompAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.WorkersCompAddress.Populated = true;
      });
  }

  private bool ReadWorkersCompClaim1()
  {
    entities.WorkersCompClaim.Populated = false;

    return Read("ReadWorkersCompClaim1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "identifier", export.WorkersCompClaim.Identifier);
      },
      (db, reader) =>
      {
        entities.WorkersCompClaim.CspNumber = db.GetString(reader, 0);
        entities.WorkersCompClaim.Identifier = db.GetInt32(reader, 1);
        entities.WorkersCompClaim.ClaimantFirstName =
          db.GetNullableString(reader, 2);
        entities.WorkersCompClaim.ClaimantMiddleName =
          db.GetNullableString(reader, 3);
        entities.WorkersCompClaim.ClaimantLastName =
          db.GetNullableString(reader, 4);
        entities.WorkersCompClaim.ClaimantAttorneyFirstName =
          db.GetNullableString(reader, 5);
        entities.WorkersCompClaim.ClaimantAttorneyLastName =
          db.GetNullableString(reader, 6);
        entities.WorkersCompClaim.ClaimantAttorneyFirmName =
          db.GetNullableString(reader, 7);
        entities.WorkersCompClaim.EmployerName =
          db.GetNullableString(reader, 8);
        entities.WorkersCompClaim.DocketNumber =
          db.GetNullableString(reader, 9);
        entities.WorkersCompClaim.InsurerName =
          db.GetNullableString(reader, 10);
        entities.WorkersCompClaim.InsurerAttorneyFirmName =
          db.GetNullableString(reader, 11);
        entities.WorkersCompClaim.InsurerContactName1 =
          db.GetNullableString(reader, 12);
        entities.WorkersCompClaim.InsurerContactName2 =
          db.GetNullableString(reader, 13);
        entities.WorkersCompClaim.InsurerContactPhone =
          db.GetNullableString(reader, 14);
        entities.WorkersCompClaim.PolicyNumber =
          db.GetNullableString(reader, 15);
        entities.WorkersCompClaim.DateOfLoss = db.GetNullableDate(reader, 16);
        entities.WorkersCompClaim.EmployerFein =
          db.GetNullableString(reader, 17);
        entities.WorkersCompClaim.DateOfAccident =
          db.GetNullableDate(reader, 18);
        entities.WorkersCompClaim.WageAmount = db.GetNullableString(reader, 19);
        entities.WorkersCompClaim.AccidentCity =
          db.GetNullableString(reader, 20);
        entities.WorkersCompClaim.AccidentState =
          db.GetNullableString(reader, 21);
        entities.WorkersCompClaim.AccidentCounty =
          db.GetNullableString(reader, 22);
        entities.WorkersCompClaim.SeverityCodeDescription =
          db.GetNullableString(reader, 23);
        entities.WorkersCompClaim.ReturnedToWorkDate =
          db.GetNullableDate(reader, 24);
        entities.WorkersCompClaim.CompensationPaidFlag =
          db.GetNullableString(reader, 25);
        entities.WorkersCompClaim.CompensationPaidDate =
          db.GetNullableDate(reader, 26);
        entities.WorkersCompClaim.WeeklyRate = db.GetNullableString(reader, 27);
        entities.WorkersCompClaim.DateOfDeath = db.GetNullableDate(reader, 28);
        entities.WorkersCompClaim.ThirdPartyAdministratorName =
          db.GetNullableString(reader, 29);
        entities.WorkersCompClaim.AdministrativeClaimNo =
          db.GetNullableString(reader, 30);
        entities.WorkersCompClaim.ClaimFiledDate =
          db.GetNullableDate(reader, 31);
        entities.WorkersCompClaim.AgencyClaimNo =
          db.GetNullableString(reader, 32);
        entities.WorkersCompClaim.CreatedBy = db.GetString(reader, 33);
        entities.WorkersCompClaim.CreatedTimestamp = db.GetDateTime(reader, 34);
        entities.WorkersCompClaim.LastUpdatedBy =
          db.GetNullableString(reader, 35);
        entities.WorkersCompClaim.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 36);
        entities.WorkersCompClaim.AccidentDescription =
          db.GetNullableString(reader, 37);
        entities.WorkersCompClaim.Populated = true;
      });
  }

  private bool ReadWorkersCompClaim2()
  {
    entities.WorkersCompClaim.Populated = false;

    return Read("ReadWorkersCompClaim2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.WorkersCompClaim.CspNumber = db.GetString(reader, 0);
        entities.WorkersCompClaim.Identifier = db.GetInt32(reader, 1);
        entities.WorkersCompClaim.ClaimantFirstName =
          db.GetNullableString(reader, 2);
        entities.WorkersCompClaim.ClaimantMiddleName =
          db.GetNullableString(reader, 3);
        entities.WorkersCompClaim.ClaimantLastName =
          db.GetNullableString(reader, 4);
        entities.WorkersCompClaim.ClaimantAttorneyFirstName =
          db.GetNullableString(reader, 5);
        entities.WorkersCompClaim.ClaimantAttorneyLastName =
          db.GetNullableString(reader, 6);
        entities.WorkersCompClaim.ClaimantAttorneyFirmName =
          db.GetNullableString(reader, 7);
        entities.WorkersCompClaim.EmployerName =
          db.GetNullableString(reader, 8);
        entities.WorkersCompClaim.DocketNumber =
          db.GetNullableString(reader, 9);
        entities.WorkersCompClaim.InsurerName =
          db.GetNullableString(reader, 10);
        entities.WorkersCompClaim.InsurerAttorneyFirmName =
          db.GetNullableString(reader, 11);
        entities.WorkersCompClaim.InsurerContactName1 =
          db.GetNullableString(reader, 12);
        entities.WorkersCompClaim.InsurerContactName2 =
          db.GetNullableString(reader, 13);
        entities.WorkersCompClaim.InsurerContactPhone =
          db.GetNullableString(reader, 14);
        entities.WorkersCompClaim.PolicyNumber =
          db.GetNullableString(reader, 15);
        entities.WorkersCompClaim.DateOfLoss = db.GetNullableDate(reader, 16);
        entities.WorkersCompClaim.EmployerFein =
          db.GetNullableString(reader, 17);
        entities.WorkersCompClaim.DateOfAccident =
          db.GetNullableDate(reader, 18);
        entities.WorkersCompClaim.WageAmount = db.GetNullableString(reader, 19);
        entities.WorkersCompClaim.AccidentCity =
          db.GetNullableString(reader, 20);
        entities.WorkersCompClaim.AccidentState =
          db.GetNullableString(reader, 21);
        entities.WorkersCompClaim.AccidentCounty =
          db.GetNullableString(reader, 22);
        entities.WorkersCompClaim.SeverityCodeDescription =
          db.GetNullableString(reader, 23);
        entities.WorkersCompClaim.ReturnedToWorkDate =
          db.GetNullableDate(reader, 24);
        entities.WorkersCompClaim.CompensationPaidFlag =
          db.GetNullableString(reader, 25);
        entities.WorkersCompClaim.CompensationPaidDate =
          db.GetNullableDate(reader, 26);
        entities.WorkersCompClaim.WeeklyRate = db.GetNullableString(reader, 27);
        entities.WorkersCompClaim.DateOfDeath = db.GetNullableDate(reader, 28);
        entities.WorkersCompClaim.ThirdPartyAdministratorName =
          db.GetNullableString(reader, 29);
        entities.WorkersCompClaim.AdministrativeClaimNo =
          db.GetNullableString(reader, 30);
        entities.WorkersCompClaim.ClaimFiledDate =
          db.GetNullableDate(reader, 31);
        entities.WorkersCompClaim.AgencyClaimNo =
          db.GetNullableString(reader, 32);
        entities.WorkersCompClaim.CreatedBy = db.GetString(reader, 33);
        entities.WorkersCompClaim.CreatedTimestamp = db.GetDateTime(reader, 34);
        entities.WorkersCompClaim.LastUpdatedBy =
          db.GetNullableString(reader, 35);
        entities.WorkersCompClaim.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 36);
        entities.WorkersCompClaim.AccidentDescription =
          db.GetNullableString(reader, 37);
        entities.WorkersCompClaim.Populated = true;
      });
  }

  private bool ReadWorkersCompClaim3()
  {
    return Read("ReadWorkersCompClaim3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkersCompClaim workersCompClaim;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public WorkArea G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private WorkArea g;
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
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkersCompClaim workersCompClaim;
    private Array<ExportGroup> export1;
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
    /// A value of DateOfDeath.
    /// </summary>
    [JsonPropertyName("dateOfDeath")]
    public WorkArea DateOfDeath
    {
      get => dateOfDeath ??= new();
      set => dateOfDeath = value;
    }

    /// <summary>
    /// A value of ReturnedToWorkDate.
    /// </summary>
    [JsonPropertyName("returnedToWorkDate")]
    public WorkArea ReturnedToWorkDate
    {
      get => returnedToWorkDate ??= new();
      set => returnedToWorkDate = value;
    }

    /// <summary>
    /// A value of DateOfAccident.
    /// </summary>
    [JsonPropertyName("dateOfAccident")]
    public WorkArea DateOfAccident
    {
      get => dateOfAccident ??= new();
      set => dateOfAccident = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public WorkersCompAddress Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of WorkersCompAddress.
    /// </summary>
    [JsonPropertyName("workersCompAddress")]
    public WorkersCompAddress WorkersCompAddress
    {
      get => workersCompAddress ??= new();
      set => workersCompAddress = value;
    }

    /// <summary>
    /// A value of InsuranceAttorney.
    /// </summary>
    [JsonPropertyName("insuranceAttorney")]
    public WorkersCompAddress InsuranceAttorney
    {
      get => insuranceAttorney ??= new();
      set => insuranceAttorney = value;
    }

    /// <summary>
    /// A value of InsuranceCarrier.
    /// </summary>
    [JsonPropertyName("insuranceCarrier")]
    public WorkersCompAddress InsuranceCarrier
    {
      get => insuranceCarrier ??= new();
      set => insuranceCarrier = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public WorkersCompAddress Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of ClaimantAttorney.
    /// </summary>
    [JsonPropertyName("claimantAttorney")]
    public WorkersCompAddress ClaimantAttorney
    {
      get => claimantAttorney ??= new();
      set => claimantAttorney = value;
    }

    /// <summary>
    /// A value of Claimant.
    /// </summary>
    [JsonPropertyName("claimant")]
    public WorkersCompAddress Claimant
    {
      get => claimant ??= new();
      set => claimant = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public WorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea dateOfDeath;
    private WorkArea returnedToWorkDate;
    private WorkArea dateOfAccident;
    private WorkersCompAddress null1;
    private WorkersCompAddress workersCompAddress;
    private WorkersCompAddress insuranceAttorney;
    private WorkersCompAddress insuranceCarrier;
    private WorkersCompAddress employer;
    private WorkersCompAddress claimantAttorney;
    private WorkersCompAddress claimant;
    private WorkArea temp;
    private WorkersCompClaim workersCompClaim;
    private Common common;
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
    /// A value of WorkersCompAddress.
    /// </summary>
    [JsonPropertyName("workersCompAddress")]
    public WorkersCompAddress WorkersCompAddress
    {
      get => workersCompAddress ??= new();
      set => workersCompAddress = value;
    }

    /// <summary>
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
    }

    private CsePerson csePerson;
    private WorkersCompAddress workersCompAddress;
    private WorkersCompClaim workersCompClaim;
  }
#endregion
}
