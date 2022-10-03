// Program: SP_CRMD_DISPLAY_CLOSED_CASE, ID: 372638457, model: 746.
// Short name: SWE02092
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
/// A program: SP_CRMD_DISPLAY_CLOSED_CASE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpCrmdDisplayClosedCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRMD_DISPLAY_CLOSED_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrmdDisplayClosedCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrmdDisplayClosedCase.
  /// </summary>
  public SpCrmdDisplayClosedCase(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      Req #    	Description
    // 07/22/97 R. Grey	IDCR 357 	Initial Code
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ---------------------------------------------
    // 06/16/11  RMathews   CQ27977  Changed infrastructure read to use import 
    // id
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCase())
    {
      local.CaseClosedDate.Date = entities.Closed.StatusDate;
      local.NoOfChOnCase.Count = 0;
      local.ChHinsStatus.CurrChHinsStatus.ActionEntry = "";
      local.ChHinsStatus.PrevChHinsNv.Flag = "N";
      local.ChHinsStatus.PrevChHinsNf.Flag = "N";
      local.ChHinsStatus.PrevChHinsVf.Flag = "N";
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCsePerson1())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      local.Ar.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        return;
      }

      MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ar);
      local.Count.Count = 0;

      foreach(var item in ReadProgram())
      {
        ++local.Count.Count;

        switch(local.Count.Count)
        {
          case 1:
            export.Pgm1.Code = entities.Program.Code;

            break;
          case 2:
            export.Pgm2.Code = entities.Program.Code;

            break;
          case 3:
            export.Pgm3.Code = entities.Program.Code;

            break;
          case 4:
            export.Pgm4.Code = entities.Program.Code;

            break;
          case 5:
            export.Pgm5.Code = entities.Program.Code;

            break;
          case 6:
            export.Pgm6.Code = entities.Program.Code;

            break;
          case 7:
            export.Pgm7.Code = entities.Program.Code;

            break;
          case 8:
            export.Pgm8.Code = entities.Program.Code;

            goto ReadEach1;
          default:
            goto ReadEach1;
        }
      }

ReadEach1:
      ;
    }

    local.Count.Count = 0;

    foreach(var item in ReadCsePersonAbsentParent())
    {
      ++local.Count.Count;
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        return;
      }

      if (local.Count.Count > 0)
      {
        if (IsEmpty(export.Ap1CsePersonsWorkSet.FormattedName))
        {
          MoveCsePersonsWorkSet2(local.CsePersonsWorkSet,
            export.Ap1CsePersonsWorkSet);
          export.Ap1CsePerson.Number = local.CsePersonsWorkSet.Number;
          export.Ap1CsePerson.Type1 = entities.CsePerson.Type1;
          local.Ap1.Number = local.CsePersonsWorkSet.Number;
        }
        else
        {
          export.MoreApsMsg.Text30 = "More AP's exist for this case.";
          export.Ap2CsePersonsWorkSet.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
          export.Ap2CsePerson.Number = local.CsePersonsWorkSet.Number;
          export.Ap2CsePerson.Type1 = entities.CsePerson.Type1;

          break;
        }
      }
    }

    if (local.Count.Count == 0)
    {
      export.MoreApsMsg.Text30 = "Case AP identity unknown.";
      export.Ap1CsePersonsWorkSet.FormattedName = "";
      export.Ap2CsePersonsWorkSet.FormattedName = "";
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCsePerson2())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      ++local.NoOfChOnCase.Count;
      UseSiReadCsePerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        export.Export1.Next();

        return;
      }

      export.Export1.Update.Child1.Assign(local.CsePersonsWorkSet);

      if (ReadChild())
      {
        export.Export1.Update.Child2.Assign(entities.Child);

        // ********************************************
        // DETERMINE Child's AGE
        // ********************************************
        if (Equal(local.CsePersonsWorkSet.Dob, local.Initialized.Date) || Equal
          (local.CsePersonsWorkSet.Dob, local.Max.Date))
        {
          export.Export1.Update.ChAge.TotalInteger = 0;
        }
        else
        {
          local.Age.TotalInteger =
            (long)DaysFromAD(local.CaseClosedDate.Date) - DaysFromAD
            (local.CsePersonsWorkSet.Dob);

          if (local.Age.TotalInteger < 365)
          {
            local.Age.TotalInteger =
              (long)Math.Round(
                local.Age.TotalInteger / 30.6M, MidpointRounding.AwayFromZero);
            export.Export1.Update.AgeText.Text4 = "Mths";
            export.Export1.Update.ChAge.TotalInteger = local.Age.TotalInteger;
          }
          else
          {
            UseCabCalcCurrentAgeFromDob();
            export.Export1.Update.AgeText.Text4 = "Yrs";
          }
        }

        // ********************************************
        // DETERMINE INSURANCE COVERAGE VIABILITY
        // ********************************************
        if (ReadHealthInsuranceViabilityCsePerson())
        {
          export.Export1.Update.HealthInsuranceViability.HinsViableInd =
            entities.HealthInsuranceViability.HinsViableInd;
        }

        // *********************************************
        // SET THE RESPONSIBLE PARTY FLAG - RETRIEVE THE COURT ORDER FOR MEDICAL
        // SUPPORT AND DETERMINE THE OBLIGOR.
        // *********************************************
        // 	
        export.Export1.Update.ResponsibleAp1.Flag = "";
        export.Export1.Update.ResponsibleAr.Flag = "";

        foreach(var item1 in ReadLegalActionPersonLegalActionDetailLegalAction())
          
        {
          // *********************************************
          // Read all obligors ordered to pay for Health Insurance
          // *********************************************
          // 	
          foreach(var item2 in ReadLegalActionPersonCsePerson2())
          {
            if (Equal(entities.ObligorCsePerson.Number, local.Ap1.Number))
            {
              export.Export1.Update.Hic.Text4 = "HIC";
              export.Export1.Update.InsByApOrArNone.Text4 = "AP";
            }
            else if (Equal(entities.ObligorCsePerson.Number, local.Ar.Number))
            {
              export.Export1.Update.InsByApOrArNone.Text4 = "AR";
              export.Export1.Update.Hic.Text4 = "HIC";

              goto ReadEach2;
            }
          }
        }

ReadEach2:

        foreach(var item1 in ReadLegalActionDetailObligationType())
        {
          if (ReadLegalActionPersonCsePerson1())
          {
            if (Equal(entities.ObligorCsePerson.Number, local.Ap1.Number))
            {
              if (Equal(entities.ObligationType.Code, "MC"))
              {
                export.Export1.Update.Mc.Text4 = "MC";
              }
              else
              {
                export.Export1.Update.Ms.Text4 = "MS";
              }
            }
          }
        }
      }

      // ********************************************
      // DETERMINE IF CHILD HAS MEDICAL COVERAGE AND,
      // IF SO, WHO IS PROVIDING HEALTH INSURANCE.
      // ********************************************
      local.CountValidHinsCoverage.Count = 0;

      export.Export1.Item.MedHinsProvider.Index = 0;
      export.Export1.Item.MedHinsProvider.Clear();

      foreach(var item1 in ReadHealthInsuranceCoveragePersonalHealthInsurance())
      {
        ++local.CountValidHinsCoverage.Count;

        if (!IsEmpty(entities.Provider.Number))
        {
          local.CsePersonsWorkSet.Number = entities.Provider.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            export.Export1.Next();
            export.Export1.Item.MedHinsProvider.Next();

            return;
          }

          MoveCsePersonsWorkSet2(local.CsePersonsWorkSet,
            export.Export1.Update.MedHinsProvider.Update.HinsProvider);
        }

        if (local.CountValidHinsCoverage.Count == 0)
        {
          export.Export1.Update.InsuAvailInd.Flag = "N";

          if (local.NoOfChOnCase.Count >= 1)
          {
            local.OneChNoIns.Flag = "Y";
          }
        }
        else
        {
          export.Export1.Update.MoreChWNoCovMsg.Text80 = "";
          export.Export1.Update.InsuAvailInd.Flag = "Y";
        }

        export.Export1.Item.MedHinsProvider.Next();
      }

      export.Export1.Next();
    }

    if (AsChar(local.OneChNoIns.Flag) == 'Y')
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        export.Export1.Update.MoreChWNoCovMsg.Text80 =
          "At least one child is not covered by health insurance";
      }
    }

    if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
    {
      // *** Work request 000170
      // *** 08/08/00 SWSRCHF
      // *** start
      local.Work.Index = -1;

      // CQ27977  Modified read to use import id instead of export id
      foreach(var item in ReadNarrativeDetail())
      {
        ++local.Work.Index;
        local.Work.CheckSize();

        local.Work.Update.Work1.NarrativeText =
          entities.MedicalReview.NarrativeText;

        if (local.Work.Index == 3)
        {
          break;
        }
      }

      if (!local.Work.IsEmpty)
      {
        local.Work.Index = 0;
        local.Work.CheckSize();

        export.MedReview.Text =
          Substring(local.Work.Item.Work1.NarrativeText, 12, 57);

        local.Work.Index = 1;
        local.Work.CheckSize();

        if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
        {
          export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
            (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
        }

        local.Work.Index = 2;
        local.Work.CheckSize();

        if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
        {
          export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
            (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
        }

        local.Work.Index = 3;
        local.Work.CheckSize();

        if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
        {
          export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
            (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
        }
      }

      // *** end
      // *** 08/08/00 SWSRCHF
      // *** Work request 000170
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsEmpty(export.MedReview.Text))
      {
        ExitState = "SP0000_FIRST_REVIEW_4_CLOSD_CASE";
      }
      else
      {
        ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
      }
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseCabCalcCurrentAgeFromDob()
  {
    var useImport = new CabCalcCurrentAgeFromDob.Import();
    var useExport = new CabCalcCurrentAgeFromDob.Export();

    useImport.CsePersonsWorkSet.Dob = local.CsePersonsWorkSet.Dob;

    Call(CabCalcCurrentAgeFromDob.Execute, useImport, useExport);

    export.Export1.Update.ChAge.TotalInteger = useExport.Common.TotalInteger;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadCase()
  {
    entities.Closed.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Closed.Number = db.GetString(reader, 0);
        entities.Closed.StatusDate = db.GetNullableDate(reader, 1);
        entities.Closed.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.Child.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.Child.MedicalSupportIndicator =
          db.GetNullableString(reader, 7);
        entities.Child.ArWaivedInsurance = db.GetNullableString(reader, 8);
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.CsePerson.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.AbsentParent.CasNumber = db.GetString(reader, 3);
        entities.AbsentParent.Type1 = db.GetString(reader, 4);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 5);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 6);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 7);
        entities.AbsentParent.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.CsePerson.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoveragePersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.Child.Populated);

    return ReadEach("ReadHealthInsuranceCoveragePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Child.CspNumber);
        db.SetString(command, "cspNumber2", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.Item.MedHinsProvider.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 2);
        entities.Provider.Number = db.GetString(reader, 2);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 3);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 4);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 5);
        entities.Provider.Type1 = db.GetString(reader, 6);
        entities.Provider.OrganizationName = db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;
        entities.Provider.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Provider.Type1);

        return true;
      });
  }

  private bool ReadHealthInsuranceViabilityCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Child.Populated);
    entities.HinsViable.Populated = false;
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViabilityCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.Child.Identifier);
        db.SetString(command, "croType", entities.Child.Type1);
        db.SetString(command, "casNumber", entities.Child.CasNumber);
        db.SetString(command, "cspNumber", entities.Child.CspNumber);
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
        entities.HealthInsuranceViability.HinsViableIndUpdatedDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceViability.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.HealthInsuranceViability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 9);
        entities.HinsViable.Number = db.GetString(reader, 9);
        entities.HinsViable.Type1 = db.GetString(reader, 10);
        entities.HinsViable.OrganizationName = db.GetNullableString(reader, 11);
        entities.HinsViable.Populated = true;
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
        CheckValid<CsePerson>("Type1", entities.HinsViable.Type1);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDt",
          local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 6);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(
          command, "effectiveDt",
          local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligorLegalActionPerson.Role = db.GetString(reader, 3);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 4);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorLegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(
          command, "effectiveDt",
          local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligorLegalActionPerson.Role = db.GetString(reader, 3);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 4);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonLegalActionDetailLegalAction()
  {
    entities.LegalActionPerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionPersonLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDt",
          local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 11);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 12);
        entities.LegalAction.Type1 = db.GetString(reader, 13);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 14);
        entities.LegalActionPerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.MedicalReview.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          import.HiddenPass.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MedicalReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.MedicalReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.MedicalReview.NarrativeText = db.GetNullableString(reader, 2);
        entities.MedicalReview.LineNumber = db.GetInt32(reader, 3);
        entities.MedicalReview.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    entities.Program.Populated = false;

    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.CaseClosedDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Closed.Number);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of InsuAvailInd.
      /// </summary>
      [JsonPropertyName("insuAvailInd")]
      public Common InsuAvailInd
      {
        get => insuAvailInd ??= new();
        set => insuAvailInd = value;
      }

      /// <summary>
      /// A value of ViableCsePers.
      /// </summary>
      [JsonPropertyName("viableCsePers")]
      public CsePersonsWorkSet ViableCsePers
      {
        get => viableCsePers ??= new();
        set => viableCsePers = value;
      }

      /// <summary>
      /// A value of ResponsibleAr.
      /// </summary>
      [JsonPropertyName("responsibleAr")]
      public Common ResponsibleAr
      {
        get => responsibleAr ??= new();
        set => responsibleAr = value;
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
      /// A value of HealthInsuranceViability.
      /// </summary>
      [JsonPropertyName("healthInsuranceViability")]
      public HealthInsuranceViability HealthInsuranceViability
      {
        get => healthInsuranceViability ??= new();
        set => healthInsuranceViability = value;
      }

      /// <summary>
      /// Gets a value of MedHinsProvider.
      /// </summary>
      [JsonIgnore]
      public Array<MedHinsProviderGroup> MedHinsProvider =>
        medHinsProvider ??= new(MedHinsProviderGroup.Capacity);

      /// <summary>
      /// Gets a value of MedHinsProvider for json serialization.
      /// </summary>
      [JsonPropertyName("medHinsProvider")]
      [Computed]
      public IList<MedHinsProviderGroup> MedHinsProvider_Json
      {
        get => medHinsProvider;
        set => MedHinsProvider.Assign(value);
      }

      /// <summary>
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CsePersonsWorkSet Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp2.
      /// </summary>
      [JsonPropertyName("responsibleAp2")]
      public Common ResponsibleAp2
      {
        get => responsibleAp2 ??= new();
        set => responsibleAp2 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp1.
      /// </summary>
      [JsonPropertyName("responsibleAp1")]
      public Common ResponsibleAp1
      {
        get => responsibleAp1 ??= new();
        set => responsibleAp1 = value;
      }

      /// <summary>
      /// A value of HealthInsurProvided.
      /// </summary>
      [JsonPropertyName("healthInsurProvided")]
      public CsePersonsWorkSet HealthInsurProvided
      {
        get => healthInsurProvided ??= new();
        set => healthInsurProvided = value;
      }

      /// <summary>
      /// A value of Child2.
      /// </summary>
      [JsonPropertyName("child2")]
      public CaseRole Child2
      {
        get => child2 ??= new();
        set => child2 = value;
      }

      /// <summary>
      /// A value of NoCoverageChildMsg.
      /// </summary>
      [JsonPropertyName("noCoverageChildMsg")]
      public SpTextWorkArea NoCoverageChildMsg
      {
        get => noCoverageChildMsg ??= new();
        set => noCoverageChildMsg = value;
      }

      /// <summary>
      /// A value of NoInsForChildMsg.
      /// </summary>
      [JsonPropertyName("noInsForChildMsg")]
      public TextWorkArea NoInsForChildMsg
      {
        get => noInsForChildMsg ??= new();
        set => noInsForChildMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common insuAvailInd;
      private CsePersonsWorkSet viableCsePers;
      private Common responsibleAr;
      private Common common;
      private HealthInsuranceViability healthInsuranceViability;
      private Array<MedHinsProviderGroup> medHinsProvider;
      private CsePersonsWorkSet child1;
      private Common responsibleAp2;
      private Common responsibleAp1;
      private CsePersonsWorkSet healthInsurProvided;
      private CaseRole child2;
      private SpTextWorkArea noCoverageChildMsg;
      private TextWorkArea noInsForChildMsg;
    }

    /// <summary>A MedHinsProviderGroup group.</summary>
    [Serializable]
    public class MedHinsProviderGroup
    {
      /// <summary>
      /// A value of HinsCoverage.
      /// </summary>
      [JsonPropertyName("hinsCoverage")]
      public HealthInsuranceCoverage HinsCoverage
      {
        get => hinsCoverage ??= new();
        set => hinsCoverage = value;
      }

      /// <summary>
      /// A value of PersHins.
      /// </summary>
      [JsonPropertyName("persHins")]
      public PersonalHealthInsurance PersHins
      {
        get => persHins ??= new();
        set => persHins = value;
      }

      /// <summary>
      /// A value of ProviderPerson.
      /// </summary>
      [JsonPropertyName("providerPerson")]
      public CsePerson ProviderPerson
      {
        get => providerPerson ??= new();
        set => providerPerson = value;
      }

      /// <summary>
      /// A value of HinsProvider.
      /// </summary>
      [JsonPropertyName("hinsProvider")]
      public CsePersonsWorkSet HinsProvider
      {
        get => hinsProvider ??= new();
        set => hinsProvider = value;
      }

      /// <summary>
      /// A value of LocalHighlite.
      /// </summary>
      [JsonPropertyName("localHighlite")]
      public Common LocalHighlite
      {
        get => localHighlite ??= new();
        set => localHighlite = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private HealthInsuranceCoverage hinsCoverage;
      private PersonalHealthInsurance persHins;
      private CsePerson providerPerson;
      private CsePersonsWorkSet hinsProvider;
      private Common localHighlite;
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
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePersonsWorkSet Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
    }

    /// <summary>
    /// A value of MedReview.
    /// </summary>
    [JsonPropertyName("medReview")]
    public NarrativeWork MedReview
    {
      get => medReview ??= new();
      set => medReview = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of HiddenPass.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    public Infrastructure HiddenPass
    {
      get => hiddenPass ??= new();
      set => hiddenPass = value;
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap1;
    private NarrativeWork medReview;
    private Array<ImportGroup> import1;
    private Case1 case1;
    private TextWorkArea moreApsMsg;
    private Infrastructure hiddenPass;
    private Common hiddenPassedReviewType;
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
      /// A value of Hic.
      /// </summary>
      [JsonPropertyName("hic")]
      public TextWorkArea Hic
      {
        get => hic ??= new();
        set => hic = value;
      }

      /// <summary>
      /// A value of Mc.
      /// </summary>
      [JsonPropertyName("mc")]
      public TextWorkArea Mc
      {
        get => mc ??= new();
        set => mc = value;
      }

      /// <summary>
      /// A value of Ms.
      /// </summary>
      [JsonPropertyName("ms")]
      public TextWorkArea Ms
      {
        get => ms ??= new();
        set => ms = value;
      }

      /// <summary>
      /// A value of ChAge.
      /// </summary>
      [JsonPropertyName("chAge")]
      public Common ChAge
      {
        get => chAge ??= new();
        set => chAge = value;
      }

      /// <summary>
      /// A value of AgeText.
      /// </summary>
      [JsonPropertyName("ageText")]
      public TextWorkArea AgeText
      {
        get => ageText ??= new();
        set => ageText = value;
      }

      /// <summary>
      /// A value of InsuAvailInd.
      /// </summary>
      [JsonPropertyName("insuAvailInd")]
      public Common InsuAvailInd
      {
        get => insuAvailInd ??= new();
        set => insuAvailInd = value;
      }

      /// <summary>
      /// A value of ViableCsePers.
      /// </summary>
      [JsonPropertyName("viableCsePers")]
      public CsePersonsWorkSet ViableCsePers
      {
        get => viableCsePers ??= new();
        set => viableCsePers = value;
      }

      /// <summary>
      /// A value of ResponsibleAr.
      /// </summary>
      [JsonPropertyName("responsibleAr")]
      public Common ResponsibleAr
      {
        get => responsibleAr ??= new();
        set => responsibleAr = value;
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
      /// A value of HealthInsuranceViability.
      /// </summary>
      [JsonPropertyName("healthInsuranceViability")]
      public HealthInsuranceViability HealthInsuranceViability
      {
        get => healthInsuranceViability ??= new();
        set => healthInsuranceViability = value;
      }

      /// <summary>
      /// Gets a value of MedHinsProvider.
      /// </summary>
      [JsonIgnore]
      public Array<MedHinsProviderGroup> MedHinsProvider =>
        medHinsProvider ??= new(MedHinsProviderGroup.Capacity);

      /// <summary>
      /// Gets a value of MedHinsProvider for json serialization.
      /// </summary>
      [JsonPropertyName("medHinsProvider")]
      [Computed]
      public IList<MedHinsProviderGroup> MedHinsProvider_Json
      {
        get => medHinsProvider;
        set => MedHinsProvider.Assign(value);
      }

      /// <summary>
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CsePersonsWorkSet Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp2.
      /// </summary>
      [JsonPropertyName("responsibleAp2")]
      public Common ResponsibleAp2
      {
        get => responsibleAp2 ??= new();
        set => responsibleAp2 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp1.
      /// </summary>
      [JsonPropertyName("responsibleAp1")]
      public Common ResponsibleAp1
      {
        get => responsibleAp1 ??= new();
        set => responsibleAp1 = value;
      }

      /// <summary>
      /// A value of HealthInsurProvider.
      /// </summary>
      [JsonPropertyName("healthInsurProvider")]
      public CsePersonsWorkSet HealthInsurProvider
      {
        get => healthInsurProvider ??= new();
        set => healthInsurProvider = value;
      }

      /// <summary>
      /// A value of Child2.
      /// </summary>
      [JsonPropertyName("child2")]
      public CaseRole Child2
      {
        get => child2 ??= new();
        set => child2 = value;
      }

      /// <summary>
      /// A value of MoreChWNoCovMsg.
      /// </summary>
      [JsonPropertyName("moreChWNoCovMsg")]
      public SpTextWorkArea MoreChWNoCovMsg
      {
        get => moreChWNoCovMsg ??= new();
        set => moreChWNoCovMsg = value;
      }

      /// <summary>
      /// A value of InsByApOrArNone.
      /// </summary>
      [JsonPropertyName("insByApOrArNone")]
      public TextWorkArea InsByApOrArNone
      {
        get => insByApOrArNone ??= new();
        set => insByApOrArNone = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private TextWorkArea hic;
      private TextWorkArea mc;
      private TextWorkArea ms;
      private Common chAge;
      private TextWorkArea ageText;
      private Common insuAvailInd;
      private CsePersonsWorkSet viableCsePers;
      private Common responsibleAr;
      private Common common;
      private HealthInsuranceViability healthInsuranceViability;
      private Array<MedHinsProviderGroup> medHinsProvider;
      private CsePersonsWorkSet child1;
      private Common responsibleAp2;
      private Common responsibleAp1;
      private CsePersonsWorkSet healthInsurProvider;
      private CaseRole child2;
      private SpTextWorkArea moreChWNoCovMsg;
      private TextWorkArea insByApOrArNone;
    }

    /// <summary>A MedHinsProviderGroup group.</summary>
    [Serializable]
    public class MedHinsProviderGroup
    {
      /// <summary>
      /// A value of HinsCoverage.
      /// </summary>
      [JsonPropertyName("hinsCoverage")]
      public HealthInsuranceCoverage HinsCoverage
      {
        get => hinsCoverage ??= new();
        set => hinsCoverage = value;
      }

      /// <summary>
      /// A value of PersHins.
      /// </summary>
      [JsonPropertyName("persHins")]
      public PersonalHealthInsurance PersHins
      {
        get => persHins ??= new();
        set => persHins = value;
      }

      /// <summary>
      /// A value of ProviderPerson.
      /// </summary>
      [JsonPropertyName("providerPerson")]
      public CsePerson ProviderPerson
      {
        get => providerPerson ??= new();
        set => providerPerson = value;
      }

      /// <summary>
      /// A value of HinsProvider.
      /// </summary>
      [JsonPropertyName("hinsProvider")]
      public CsePersonsWorkSet HinsProvider
      {
        get => hinsProvider ??= new();
        set => hinsProvider = value;
      }

      /// <summary>
      /// A value of LocalHighlite.
      /// </summary>
      [JsonPropertyName("localHighlite")]
      public Common LocalHighlite
      {
        get => localHighlite ??= new();
        set => localHighlite = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2;

      private HealthInsuranceCoverage hinsCoverage;
      private PersonalHealthInsurance persHins;
      private CsePerson providerPerson;
      private CsePersonsWorkSet hinsProvider;
      private Common localHighlite;
    }

    /// <summary>A HiddenPassedGroup group.</summary>
    [Serializable]
    public class HiddenPassedGroup
    {
      /// <summary>
      /// A value of GexportH.
      /// </summary>
      [JsonPropertyName("gexportH")]
      public NarrativeWork GexportH
      {
        get => gexportH ??= new();
        set => gexportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork gexportH;
    }

    /// <summary>
    /// A value of Pgm1.
    /// </summary>
    [JsonPropertyName("pgm1")]
    public Program Pgm1
    {
      get => pgm1 ??= new();
      set => pgm1 = value;
    }

    /// <summary>
    /// A value of Pgm2.
    /// </summary>
    [JsonPropertyName("pgm2")]
    public Program Pgm2
    {
      get => pgm2 ??= new();
      set => pgm2 = value;
    }

    /// <summary>
    /// A value of Pgm3.
    /// </summary>
    [JsonPropertyName("pgm3")]
    public Program Pgm3
    {
      get => pgm3 ??= new();
      set => pgm3 = value;
    }

    /// <summary>
    /// A value of Pgm4.
    /// </summary>
    [JsonPropertyName("pgm4")]
    public Program Pgm4
    {
      get => pgm4 ??= new();
      set => pgm4 = value;
    }

    /// <summary>
    /// A value of Pgm5.
    /// </summary>
    [JsonPropertyName("pgm5")]
    public Program Pgm5
    {
      get => pgm5 ??= new();
      set => pgm5 = value;
    }

    /// <summary>
    /// A value of Pgm6.
    /// </summary>
    [JsonPropertyName("pgm6")]
    public Program Pgm6
    {
      get => pgm6 ??= new();
      set => pgm6 = value;
    }

    /// <summary>
    /// A value of Pgm7.
    /// </summary>
    [JsonPropertyName("pgm7")]
    public Program Pgm7
    {
      get => pgm7 ??= new();
      set => pgm7 = value;
    }

    /// <summary>
    /// A value of Pgm8.
    /// </summary>
    [JsonPropertyName("pgm8")]
    public Program Pgm8
    {
      get => pgm8 ??= new();
      set => pgm8 = value;
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
    /// A value of Ap1CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ap1CsePersonsWorkSet")]
    public CsePersonsWorkSet Ap1CsePersonsWorkSet
    {
      get => ap1CsePersonsWorkSet ??= new();
      set => ap1CsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ap1CsePerson.
    /// </summary>
    [JsonPropertyName("ap1CsePerson")]
    public CsePerson Ap1CsePerson
    {
      get => ap1CsePerson ??= new();
      set => ap1CsePerson = value;
    }

    /// <summary>
    /// A value of Ap2CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ap2CsePersonsWorkSet")]
    public CsePersonsWorkSet Ap2CsePersonsWorkSet
    {
      get => ap2CsePersonsWorkSet ??= new();
      set => ap2CsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ap2CsePerson.
    /// </summary>
    [JsonPropertyName("ap2CsePerson")]
    public CsePerson Ap2CsePerson
    {
      get => ap2CsePerson ??= new();
      set => ap2CsePerson = value;
    }

    /// <summary>
    /// A value of MedReview.
    /// </summary>
    [JsonPropertyName("medReview")]
    public NarrativeWork MedReview
    {
      get => medReview ??= new();
      set => medReview = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of HiddenPass.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    public Infrastructure HiddenPass
    {
      get => hiddenPass ??= new();
      set => hiddenPass = value;
    }

    /// <summary>
    /// Gets a value of HiddenPassed.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassedGroup> HiddenPassed => hiddenPassed ??= new(
      HiddenPassedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPassed for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPassed")]
    [Computed]
    public IList<HiddenPassedGroup> HiddenPassed_Json
    {
      get => hiddenPassed;
      set => HiddenPassed.Assign(value);
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    /// <summary>
    /// A value of HiddenPassed1.
    /// </summary>
    [JsonPropertyName("hiddenPassed1")]
    public Infrastructure HiddenPassed1
    {
      get => hiddenPassed1 ??= new();
      set => hiddenPassed1 = value;
    }

    private Program pgm1;
    private Program pgm2;
    private Program pgm3;
    private Program pgm4;
    private Program pgm5;
    private Program pgm6;
    private Program pgm7;
    private Program pgm8;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap1CsePersonsWorkSet;
    private CsePerson ap1CsePerson;
    private CsePersonsWorkSet ap2CsePersonsWorkSet;
    private CsePerson ap2CsePerson;
    private NarrativeWork medReview;
    private Array<ExportGroup> export1;
    private Case1 case1;
    private TextWorkArea moreApsMsg;
    private Infrastructure hiddenPass;
    private Array<HiddenPassedGroup> hiddenPassed;
    private Common hiddenPassedReviewType;
    private Common caseClosedIndicator;
    private Infrastructure hiddenPassed1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A WorkGroup group.</summary>
    [Serializable]
    public class WorkGroup
    {
      /// <summary>
      /// A value of Work1.
      /// </summary>
      [JsonPropertyName("work1")]
      public NarrativeDetail Work1
      {
        get => work1 ??= new();
        set => work1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private NarrativeDetail work1;
    }

    /// <summary>A ChHinsStatusGroup group.</summary>
    [Serializable]
    public class ChHinsStatusGroup
    {
      /// <summary>
      /// A value of CurrChHinsStatus.
      /// </summary>
      [JsonPropertyName("currChHinsStatus")]
      public Common CurrChHinsStatus
      {
        get => currChHinsStatus ??= new();
        set => currChHinsStatus = value;
      }

      /// <summary>
      /// A value of PrevChHinsVf.
      /// </summary>
      [JsonPropertyName("prevChHinsVf")]
      public Common PrevChHinsVf
      {
        get => prevChHinsVf ??= new();
        set => prevChHinsVf = value;
      }

      /// <summary>
      /// A value of PrevChHinsNv.
      /// </summary>
      [JsonPropertyName("prevChHinsNv")]
      public Common PrevChHinsNv
      {
        get => prevChHinsNv ??= new();
        set => prevChHinsNv = value;
      }

      /// <summary>
      /// A value of PrevChHinsNf.
      /// </summary>
      [JsonPropertyName("prevChHinsNf")]
      public Common PrevChHinsNf
      {
        get => prevChHinsNf ??= new();
        set => prevChHinsNf = value;
      }

      private Common currChHinsStatus;
      private Common prevChHinsVf;
      private Common prevChHinsNv;
      private Common prevChHinsNf;
    }

    /// <summary>
    /// Gets a value of Work.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup> Work => work ??= new(WorkGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Work for json serialization.
    /// </summary>
    [JsonPropertyName("work")]
    [Computed]
    public IList<WorkGroup> Work_Json
    {
      get => work;
      set => Work.Assign(value);
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public DateWorkArea CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
    }

    /// <summary>
    /// A value of NoOfChOnCase.
    /// </summary>
    [JsonPropertyName("noOfChOnCase")]
    public Common NoOfChOnCase
    {
      get => noOfChOnCase ??= new();
      set => noOfChOnCase = value;
    }

    /// <summary>
    /// Gets a value of ChHinsStatus.
    /// </summary>
    [JsonPropertyName("chHinsStatus")]
    public ChHinsStatusGroup ChHinsStatus
    {
      get => chHinsStatus ?? (chHinsStatus = new());
      set => chHinsStatus = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePerson Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
    }

    /// <summary>
    /// A value of CountValidHinsCoverage.
    /// </summary>
    [JsonPropertyName("countValidHinsCoverage")]
    public Common CountValidHinsCoverage
    {
      get => countValidHinsCoverage ??= new();
      set => countValidHinsCoverage = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Age.
    /// </summary>
    [JsonPropertyName("age")]
    public Common Age
    {
      get => age ??= new();
      set => age = value;
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
    /// A value of OneChNoIns.
    /// </summary>
    [JsonPropertyName("oneChNoIns")]
    public Common OneChNoIns
    {
      get => oneChNoIns ??= new();
      set => oneChNoIns = value;
    }

    private Array<WorkGroup> work;
    private Common count;
    private DateWorkArea caseClosedDate;
    private Common noOfChOnCase;
    private ChHinsStatusGroup chHinsStatus;
    private CsePerson ar;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private DateWorkArea current;
    private CsePerson ap1;
    private Common countValidHinsCoverage;
    private DateWorkArea initialized;
    private Common age;
    private DateWorkArea max;
    private Common oneChNoIns;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MedicalReview.
    /// </summary>
    [JsonPropertyName("medicalReview")]
    public NarrativeDetail MedicalReview
    {
      get => medicalReview ??= new();
      set => medicalReview = value;
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

    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Case1 Closed
    {
      get => closed ??= new();
      set => closed = value;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of HinsViable.
    /// </summary>
    [JsonPropertyName("hinsViable")]
    public CsePerson HinsViable
    {
      get => hinsViable ??= new();
      set => hinsViable = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
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
    /// A value of Provider.
    /// </summary>
    [JsonPropertyName("provider")]
    public CsePerson Provider
    {
      get => provider ??= new();
      set => provider = value;
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
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    public CaseRole CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
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

    private NarrativeDetail medicalReview;
    private ObligationType obligationType;
    private PersonProgram personProgram;
    private Program program;
    private Case1 closed;
    private CsePerson csePerson;
    private CaseRole applicantRecipient;
    private Case1 case1;
    private CaseRole absentParent;
    private CaseRole child;
    private CsePerson hinsViable;
    private HealthInsuranceViability healthInsuranceViability;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CsePerson obligorCsePerson;
    private LegalActionPerson obligorLegalActionPerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePerson provider;
    private CaseRole caseRole;
    private CaseRole coveredPerson;
    private Infrastructure infrastructure;
  }
#endregion
}
