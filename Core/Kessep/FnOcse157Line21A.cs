// Program: FN_OCSE157_LINE_21A, ID: 371279081, model: 746.
// Short name: SWE02975
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_21A.
/// </summary>
[Serializable]
public partial class FnOcse157Line21A: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_21A program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line21A(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line21A.
  /// </summary>
  public FnOcse157Line21A(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 08/16/06  GVandy	WR00230751	Initial Development.
    // 10/24/06  GVandy	PR294186	Performance changes
    // 04/14/08  GVandy	CQ#2461		Changes to Business Rule #3 per federal data 
    // reliability audit.
    // 					Count case if health insurance is ordered and provided, regardless 
    // of
    // 					who is providing the coverage.  Previously we required the person
    // 					ordered to provide the coverage to actually provide the coverage.
    // -------------------------------------------------------------------------------------------------------------
    // -----------------------------------------
    // BUSINESS RULES for LINE 21a
    // -----------------------------------------
    // Line 21a - Cases Open at the End of the Fiscal Year in Which Medical 
    // Support is Ordered and Provided (due 10/2006)
    // 1.  Line 21a is a subset of Line 21.
    // 2.  Same criteria as Line 21
    // 3.  The same children (or child) on the HIC legal detail LOPS screen, 
    // must be on HICP screen and health insurance
    //     was provided at some point during the FY
    // 	- or-
    // 4.  Collection has applied to an MS or MC obligation within the FY (use 
    // the same criteria for count collections
    //     w/adjustments, t.b.d. by developer).  No HIC legal detail exists.
    //  	- or -
    // 5.  If HIC legal detail exists and there is an MS or MC collection, count
    // only if private insurance is not
    //     viable (HICV screen). HINS_VIABLE_IND = N'.  The same child must 
    // qualify for all criteria.
    // 	- or -
    // 6.  At some point during the FY the child was a Title XIX (19) or XXI (21
    // ) recipient, a MS or MC obligation
    //     exists, and insurance is not viable. The obligor must be the AP or 
    // have the MO or FA role assigned.
    // -------------------------------------------------------------------------------------------------------------------
    export.CountInLine21A.Flag = "N";

    // -- Performance change...
    // Check if any child on the case has a health_insurance_viability record.  
    // (Less than 10% of cases currently have one).
    // Don't do any additional processing for rule #6 if it doesn't exist.
    if (ReadHealthInsuranceViability1())
    {
      local.CheckForRule6.Flag = "Y";
    }

    if (AsChar(local.CheckForRule6.Flag) == 'Y')
    {
      // -- Read each child on the case....
      foreach(var item in ReadCsePersonCaseRole())
      {
        // -- Determine if the child was covered by publicly funded health 
        // insurance at some point during the fiscal year.
        //    (The timeframe must also overlap the case role timeframe.)
        // -- Publicly funded health insurance is Title 19 or 21 health 
        // insurance coverage.
        // ---------------------------------------------------------------------------------------------
        // Title 19 recipients are identified by an active CI, MA, MK, MP, or MS
        // program
        // (i.e. program system gen id = 6, 7, 8, 10, or 11).
        // Title 21 recipients are identified by an active MP program with a 
        // medical subtype beginning with letter "T".
        // ---------------------------------------------------------------------------------------------
        foreach(var item1 in ReadPersonProgram())
        {
          // -- Determine if an MC or MS obligation exists.  The obligation must
          // have been accruing at some point during
          // the fiscal year and also must have been accruing at some point 
          // during the time when the child was on publicly funded health
          // insurance.
          foreach(var item2 in ReadAccrualInstructions())
          {
            // -- The obligor on the debt must be the AP, mother, or father.
            if (!ReadCsePerson1())
            {
              continue;
            }

            // -- Determine if health insurance is viable.
            if (ReadHealthInsuranceViability3())
            {
              if (AsChar(entities.HealthInsuranceViability.HinsViableInd) == 'N'
                )
              {
                // ----------------------------------------------------------------------------------------------------------------
                // Case qualifies for line 21a based on Rule 6.
                // 6.  At some point during the FY the child was a Title XIX (19
                // ) or XXI (21) recipient, a MS or MC
                //     obligation exists, and insurance is not viable. The 
                // obligor must be the AP or have the MO or FA role assigned.
                // ----------------------------------------------------------------------------------------------------------------
                export.CountInLine21A.Flag = "Y";
                export.Ocse157Verification.ObligorPersonNbr =
                  entities.CsePerson.Number;
                export.Ocse157Verification.SuppPersonNumber =
                  entities.ChCsePerson.Number;
                export.Ocse157Verification.Comment = "Business Rule #6";

                return;
              }
            }
          }
        }
      }
    }

    // -- Performance change...
    // Check if any AP or AR has an MS or MC collection during the year.  (For 
    // FY 2006 there were only a total of 1715 collections).
    // If not skip processing for Rule 4 and 5.
    if (ReadCollection1())
    {
      local.CheckForRule4And5.Flag = "Y";
    }

    // -- Performance change...
    // Check if any AP or AR has Health Insurance Coverage during the year.  (
    // For FY 2006 there were only a total of 42,xxx Health Insurance Coverage
    // records).
    // If not skip processing for Rule 3.
    if (ReadHealthInsuranceCoverage())
    {
      local.CheckForRule3.Flag = "Y";
    }

    if (AsChar(export.CountInLine21A.Flag) == 'N')
    {
      if (AsChar(local.CheckForRule3.Flag) == 'Y' || AsChar
        (local.CheckForRule4And5.Flag) == 'Y')
      {
        // -- Continue.
      }
      else
      {
        // -- No need to check for rule 3 or 5.
        goto Test;
      }

      // -- Determine if HIC has been ordered.
      local.HicIsOrdered.Flag = "";
      local.HicIsProvided.Flag = "";

      // -- 10/24/06  GVandy  PR294186 Performance change.  Original code below.
      foreach(var item in ReadCsePersonLegalActionDetailLegalActionPerson())
      {
        if (ReadLaPersonLaCaseRole())
        {
          // -- Continue
        }
        else
        {
          continue;
        }

        // -- End performance change.
        // -- Find all children for whom Health Insurance was ordered.
        foreach(var item1 in ReadCsePerson2())
        {
          local.HicIsOrdered.Flag = "Y";

          if (AsChar(local.HicIsProvided.Flag) == 'N')
          {
            // -- No need to check for health insurance coverage.  Another child
            // for whom health insurance was ordered
            //    has already been determined to not have been covered by health
            // insurance.
            // -- Don't escape or skip this child.  We still want to check 
            // health insurance viability and the existence of a MS/MC
            // collection.
            // Continue...
          }
          else
          {
            // -- Determine if the obligor actually provided insurance for each 
            // child at some point during the FY.
            local.HicIsProvided.Flag = "N";

            // 04/14/08  GVandy	CQ#2461		Changes per federal data reliability 
            // audit.
            // 					Count case if health insurance is ordered and provided, 
            // regardless of
            // 					who is providing the coverage.  Previously we required the 
            // person
            // 					ordered to provide the coverage to actually provide the 
            // coverage.
            if (ReadHealthInsuranceCoveragePersonalHealthInsurance())
            {
              // -- Save the obligor and supported person number in case we 
              // include the case based on Rule 3 below.
              export.Ocse157Verification.ObligorPersonNbr =
                entities.ApOrArCsePerson.Number;
              export.Ocse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.HicIsProvided.Flag = "Y";
            }
          }

          if (AsChar(local.CheckForRule4And5.Flag) == 'Y')
          {
            // -- Check if health insurance is viable.
            if (ReadHealthInsuranceViability2())
            {
              if (AsChar(entities.HealthInsuranceViability.HinsViableInd) != 'N'
                )
              {
                // -- No need to do any additional processing if the health 
                // insurance IS viable.  Get the next child.
                continue;
              }
            }

            // -- Determine if a collection applied to a MC or MS debt during 
            // the fiscal year for this AP/CH combo.
            if (ReadCollection2())
            {
              // ----------------------------------------------------------------------------------------------------------------
              // Case qualifies for line 21a based on Rule 5.
              // 5.  If HIC legal detail exists and there is an MS or MC 
              // collection, count only if private insurance is not
              //     viable (HICV screen). HINS_VIABLE_IND = N'.  The same 
              // child must qualify for all criteria.
              // ----------------------------------------------------------------------------------------------------------------
              export.CountInLine21A.Flag = "Y";
              export.Ocse157Verification.ObligorPersonNbr =
                entities.ApOrArCsePerson.Number;
              export.Ocse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              export.Ocse157Verification.CollectionAmount =
                entities.Collection.Amount;
              export.Ocse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              export.Ocse157Verification.CourtOrderNumber =
                entities.Collection.CourtOrderAppliedTo;
              export.Ocse157Verification.Comment = "Business Rule #5";

              return;
            }
          }
        }
      }

      // IF HIC is ordered and provided then include the case in Line 21a (Rule 
      // 3)
      if (AsChar(local.HicIsOrdered.Flag) == 'Y' && AsChar
        (local.HicIsProvided.Flag) == 'Y')
      {
        // ----------------------------------------------------------------------------------------------------------------
        // Case qualifies for line 21a based on Rule 3.
        // 3.  The same children (or child) on the HIC legal detail LOPS screen,
        // must be on HICP screen and health
        //     insurance was provided at some point during the FY.
        // ----------------------------------------------------------------------------------------------------------------
        export.CountInLine21A.Flag = "Y";
        export.Ocse157Verification.Comment = "Business Rule #3";
      }
    }

Test:

    if (AsChar(export.CountInLine21A.Flag) == 'N')
    {
      // If HIC is NOT ordered and an MS or MC collection exists then include in
      // Line 21a.  (Rule 4)
      if (AsChar(local.HicIsOrdered.Flag) != 'Y')
      {
        if (AsChar(local.CheckForRule4And5.Flag) == 'Y')
        {
          // -- Continue.
        }
        else
        {
          // -- No need to check for rule 4.
          return;
        }

        // -- Determine if a collection applied to a MC or MS debt during the 
        // fiscal year.
        if (ReadCollectionCsePersonCsePerson())
        {
          // ----------------------------------------------------------------------------------------------------------------
          // Case qualifies for line 21a based on Rule 4.
          // 4.  Collection has applied to an MS or MC obligation within the FY 
          // (use the same criteria for count
          //     collections w/adjustments, t.b.d. by developer).  No HIC legal 
          // detail exists.
          // ----------------------------------------------------------------------------------------------------------------
          export.CountInLine21A.Flag = "Y";
          export.Ocse157Verification.ObligorPersonNbr =
            entities.ApOrArCsePerson.Number;
          export.Ocse157Verification.SuppPersonNumber =
            entities.ChCsePerson.Number;
          export.Ocse157Verification.CollectionAmount =
            entities.Collection.Amount;
          export.Ocse157Verification.CollectionDte =
            entities.Collection.CollectionDt;
          export.Ocse157Verification.CourtOrderNumber =
            entities.Collection.CourtOrderAppliedTo;
          export.Ocse157Verification.Comment = "Business Rule #4";
        }
      }
    }
  }

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetDate(
          command, "asOfDt1", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt1",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "asOfDt2",
          entities.PersonProgram.DiscontinueDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt2",
          entities.PersonProgram.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "asOfDt3", entities.ChCaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt3",
          entities.ChCaseRole.StartDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private bool ReadCollection1()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadCollection2()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApOrArCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadCollectionCsePersonCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ApOrArCsePerson.Populated = false;
    entities.Collection.Populated = false;

    return Read("ReadCollectionCsePersonCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.ApOrArCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.ChCsePerson.Number = db.GetString(reader, 20);
        entities.ChCsePerson.Populated = true;
        entities.ApOrArCsePerson.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.AccrualInstructions.CspNumber);
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CasNumber = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalActionDetailLegalActionPerson()
  {
    entities.Obligor1.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.ApOrArCsePerson.Populated = false;

    return ReadEach("ReadCsePersonLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Persistent.Number);
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApOrArCsePerson.Number = db.GetString(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 1);
        entities.Obligor1.LgaRIdentifier = db.GetNullableInt32(reader, 1);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 2);
        entities.Obligor1.LadRNumber = db.GetNullableInt32(reader, 2);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 4);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.Obligor1.Identifier = db.GetInt32(reader, 8);
        entities.Obligor1.CspNumber = db.GetNullableString(reader, 9);
        entities.Obligor1.AccountType = db.GetNullableString(reader, 10);
        entities.Obligor1.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.ApOrArCsePerson.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Persistent.Number);
        db.SetNullableDate(
          command, "policyEffDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "policyExpDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 2);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoveragePersonalHealthInsurance()
  {
    entities.HealthInsuranceCoverage.Populated = false;
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadHealthInsuranceCoveragePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
        db.SetNullableDate(
          command, "coverBeginDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "coverEndDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 2);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 4);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 5);
        entities.PersonalHealthInsurance.CoverageVerifiedDate =
          db.GetNullableDate(reader, 6);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 7);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 8);
        entities.HealthInsuranceCoverage.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;
      });
  }

  private bool ReadHealthInsuranceViability1()
  {
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private bool ReadHealthInsuranceViability2()
  {
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
        db.
          SetNullableString(command, "cspNum", entities.ApOrArCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private bool ReadHealthInsuranceViability3()
  {
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability3",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType", entities.ChCaseRole.Type1);
        db.SetString(command, "casNumber", entities.ChCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ChCaseRole.CspNumber);
        db.SetNullableString(command, "cspNum", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private bool ReadLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.Obligor1.Identifier);
        db.SetInt32(command, "lgaId", entities.LegalActionDetail.LgaIdentifier);
        db.SetString(command, "casNum", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LaPersonLaCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
      });
  }

  private IEnumerable<bool> ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate1",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          entities.ChCaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate2",
          entities.ChCaseRole.StartDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;

        return true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Case1 Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    private Case1 persistent;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of CountInLine21A.
    /// </summary>
    [JsonPropertyName("countInLine21A")]
    public Common CountInLine21A
    {
      get => countInLine21A ??= new();
      set => countInLine21A = value;
    }

    private Ocse157Verification ocse157Verification;
    private Common countInLine21A;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CheckForRule3.
    /// </summary>
    [JsonPropertyName("checkForRule3")]
    public Common CheckForRule3
    {
      get => checkForRule3 ??= new();
      set => checkForRule3 = value;
    }

    /// <summary>
    /// A value of CheckForRule4And5.
    /// </summary>
    [JsonPropertyName("checkForRule4And5")]
    public Common CheckForRule4And5
    {
      get => checkForRule4And5 ??= new();
      set => checkForRule4And5 = value;
    }

    /// <summary>
    /// A value of CheckForRule6.
    /// </summary>
    [JsonPropertyName("checkForRule6")]
    public Common CheckForRule6
    {
      get => checkForRule6 ??= new();
      set => checkForRule6 = value;
    }

    /// <summary>
    /// A value of IncludeInLine21A.
    /// </summary>
    [JsonPropertyName("includeInLine21A")]
    public Common IncludeInLine21A
    {
      get => includeInLine21A ??= new();
      set => includeInLine21A = value;
    }

    /// <summary>
    /// A value of HicIsProvided.
    /// </summary>
    [JsonPropertyName("hicIsProvided")]
    public Common HicIsProvided
    {
      get => hicIsProvided ??= new();
      set => hicIsProvided = value;
    }

    /// <summary>
    /// A value of HicIsOrdered.
    /// </summary>
    [JsonPropertyName("hicIsOrdered")]
    public Common HicIsOrdered
    {
      get => hicIsOrdered ??= new();
      set => hicIsOrdered = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Common checkForRule3;
    private Common checkForRule4And5;
    private Common checkForRule6;
    private Common includeInLine21A;
    private Common hicIsProvided;
    private Common hicIsOrdered;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public LegalActionPerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public LegalActionPerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApOrArCaseRole.
    /// </summary>
    [JsonPropertyName("apOrArCaseRole")]
    public CaseRole ApOrArCaseRole
    {
      get => apOrArCaseRole ??= new();
      set => apOrArCaseRole = value;
    }

    /// <summary>
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ApOrArCsePerson.
    /// </summary>
    [JsonPropertyName("apOrArCsePerson")]
    public CsePerson ApOrArCsePerson
    {
      get => apOrArCsePerson ??= new();
      set => apOrArCsePerson = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private CaseRole caseRole;
    private CsePerson csePerson;
    private AccrualInstructions accrualInstructions;
    private LegalActionPerson supported1;
    private LegalActionPerson obligor1;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private Case1 case1;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole apOrArCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
    private CaseRole chCaseRole;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson chCsePerson;
    private CsePerson apOrArCsePerson;
    private Collection collection;
    private CsePerson ap;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor2;
    private CsePersonAccount supported2;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private HealthInsuranceViability healthInsuranceViability;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}
