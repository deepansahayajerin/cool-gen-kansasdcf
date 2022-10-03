// Program: OE_GTSC_DISP_FA_MO_CH_COMB, ID: 371797160, model: 746.
// Short name: SWE01531
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
/// A program: OE_GTSC_DISP_FA_MO_CH_COMB.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This BSD common action block reads and populates genetic test details for a 
/// father-mother-child combination for display.
/// </para>
/// </summary>
[Serializable]
public partial class OeGtscDispFaMoChComb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_DISP_FA_MO_CH_COMB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscDispFaMoChComb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscDispFaMoChComb.
  /// </summary>
  public OeGtscDispFaMoChComb(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE		CHGREQ		DESCRIPTION
    // govind		12-19-94			Initial coding
    // Ty Hill-MTW     04/29/97                        Change Current_date
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG ****************
    // *********************************************
    // SYSTEM:		KESSEP
    // DESCRIPTION:
    // This Action Block moves Father, Mother and Child names to screen fields 
    // for display.
    // PROCESSING:
    // This action block is passed a group containing person number, last name, 
    // first name, MI and Role (3 entries giving father, mother and child).
    // It moves the Person Number and Names to corresponding Father, Mother and 
    // Child screen fields.
    // CREATED BY:	Govindaraj.
    // DATE CREATED:	12-28-1994.
    // *********************************************
    // ***********************************************************************
    //    AUTHOR         DATE           DESCRIPTION
    //  Madhu Kumar     08/15/01        PR corrected error message
    //                                  
    // when trying to reschedule
    //                                  
    // dad and child .
    // ***********************************************************************
    // **************************************************************
    //      PR 129 046  abend on gtsc    -  SWSRMXK
    //      Changed the read from select to select with cursor coz
    //  there might be more than one inactive case role records.
    // **************************************************************
    local.Current.Date = Now().Date;
    export.GeneticTestInformation.CaseNumber = import.Case1.Number;
    local.NoOfFathersSelected.Count = 0;
    local.NoOfMothersSelected.Count = 0;
    local.NoOfChildrenSelected.Count = 0;

    for(import.Import1.Index = 0; import.Import1.Index < 3; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AP"))
      {
        ++local.NoOfFathersSelected.Count;
        export.GeneticTestInformation.FatherPersonNo =
          import.Import1.Item.DetailCsePerson.Number;
      }

      if (Equal(import.Import1.Item.DetailCaseRole.Type1, "MO"))
      {
        ++local.NoOfMothersSelected.Count;
        export.GeneticTestInformation.MotherPersonNo =
          import.Import1.Item.DetailCsePerson.Number;
      }

      if (Equal(import.Import1.Item.DetailCaseRole.Type1, "CH"))
      {
        ++local.NoOfChildrenSelected.Count;
        export.GeneticTestInformation.ChildPersonNo =
          import.Import1.Item.DetailCsePerson.Number;
      }
    }

    import.Import1.CheckIndex();

    if (local.NoOfFathersSelected.Count != 1 || local
      .NoOfChildrenSelected.Count != 1)
    {
      // ---------------------------------------------
      // Invalid FATHER-CHILD combination selected. There must be exactly one (
      // and only one) entry for father, ( mother optional)  and child.
      // ---------------------------------------------
      ExitState = "OE0000_INVALID_FATHER_CHILD_COMB";

      return;
    }

    // ---------------------------------------------
    // If a genetic test has already been scheduled for the combination, then 
    // display the genetic test details.
    // Read first CSE_PERSON records for the father, mother and child.
    // Then read CASE_ROLE records for the father, mother and child.
    // Then read GENETIC_TEST associated with the case_role and cse_person 
    // records.
    // Then read PERSON_GENETIC_TEST records for the father, mother and child.
    // ---------------------------------------------
    if (ReadCase())
    {
      if (AsChar(entities.ExistingCase.Status) == 'O')
      {
        export.CaseOpen.Flag = "Y";
      }
      else if (AsChar(entities.ExistingCase.Status) == 'C')
      {
        export.CaseOpen.Flag = "N";
      }
    }
    else
    {
      ExitState = "OE0020_INVALID_CASE_NO";

      return;
    }

    if (ReadCsePerson2())
    {
      UseCabGetClientDetails1();
      export.GeneticTestInformation.FatherFormattedName =
        local.CsePersonsWorkSet.FormattedName;
    }
    else
    {
      ExitState = "OE0059_NF_FATHER_CSE_PERSON";

      return;
    }

    if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
    {
      if (ReadCsePerson3())
      {
        UseCabGetClientDetails2();
        export.GeneticTestInformation.MotherFormattedName =
          local.CsePersonsWorkSet.FormattedName;
        export.GeneticTestInformation.MotherLastName =
          local.CsePersonsWorkSet.LastName;
        export.GeneticTestInformation.MotherMi =
          local.CsePersonsWorkSet.MiddleInitial;
        export.GeneticTestInformation.MotherFirstName =
          local.CsePersonsWorkSet.FirstName;
      }
      else
      {
        ExitState = "OE0062_NF_MOTHER_CSE_PERSON";

        return;
      }
    }

    if (ReadCsePerson1())
    {
      UseCabGetClientDetails3();
      export.GeneticTestInformation.ChildFormattedName =
        local.CsePersonsWorkSet.FormattedName;
      export.GeneticTestInformation.ChildLastName =
        local.CsePersonsWorkSet.LastName;
      export.GeneticTestInformation.ChildMi =
        local.CsePersonsWorkSet.MiddleInitial;
      export.GeneticTestInformation.ChildFirstName =
        local.CsePersonsWorkSet.FirstName;
      export.GeneticTestInformation.ChildDob = local.CsePersonsWorkSet.Dob;
    }
    else
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    if (AsChar(export.CaseOpen.Flag) == 'Y')
    {
      if (!ReadCaseRole3())
      {
        // **************************************************************
        //      PR 129 046  abend on gtsc    -  MXK
        //      Changed the read from select to select with cursor coz
        //  there might be more than one inactive case role records.
        // **************************************************************
        if (ReadCaseRole2())
        {
          export.CaseRoleInactive.Flag = "Y";
          export.ActiveAp.Flag = "N";
        }
        else
        {
          ExitState = "CO0000_ABSENT_PARENT_NF";

          return;
        }
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
      {
        if (!ReadMother3())
        {
          // **************************************************************
          //      PR 129 046  abend on gtsc    -  MXK
          //      Changed the read from select to select with cursor coz
          //  there might be more than one inactive case role records.
          // **************************************************************
          if (ReadMother2())
          {
            export.CaseRoleInactive.Flag = "Y";
          }
          else
          {
            ExitState = "OE0055_NF_CASE_ROLE_MOTHER";

            return;
          }
        }
      }

      if (!ReadChild3())
      {
        // **************************************************************
        //      PR 129 046  abend on gtsc    -  MXK
        //      Changed the read from select to select with cursor coz
        //  there might be more than one inactive case role records.
        // **************************************************************
        if (ReadChild2())
        {
          export.CaseRoleInactive.Flag = "Y";
          export.ActiveChild.Flag = "N";
        }
        else
        {
          ExitState = "OE0065_NF_CASE_ROLE_CHILD";

          return;
        }
      }
    }
    else if (AsChar(export.CaseOpen.Flag) == 'N')
    {
      // **************************************************************
      //      PR159890   --   GTSC abending with SQLCODE = - 811
      //      Changed the read from select to select with cursor since
      //  there might be more than one inactive case role records.
      // **************************************************************
      if (!ReadCaseRole1())
      {
        ExitState = "CO0000_ABSENT_PARENT_NF";

        return;
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
      {
        if (!ReadMother1())
        {
          ExitState = "OE0055_NF_CASE_ROLE_MOTHER";

          return;
        }
      }

      if (!ReadChild1())
      {
        ExitState = "OE0065_NF_CASE_ROLE_CHILD";

        return;
      }
    }

    local.LatestGeneticTestFound.Flag = "N";

    if (entities.ExistingCaseRoleMother.Populated)
    {
      if (ReadGeneticTest1())
      {
        local.LatestGeneticTestFound.Flag = "Y";
        local.Latest.TestNumber = entities.ExistingScheduled.TestNumber;
      }
    }
    else if (ReadGeneticTest2())
    {
      local.LatestGeneticTestFound.Flag = "Y";
      local.Latest.TestNumber = entities.ExistingScheduled.TestNumber;
    }

    if (AsChar(local.LatestGeneticTestFound.Flag) == 'N')
    {
      // Genetic Test has not yet been scheduled. So nothing to display.
      return;
    }

    if (ReadGeneticTest6())
    {
      export.GeneticTest.TestNumber = entities.ExistingScheduled.TestNumber;

      if (ReadLegalAction())
      {
        export.GeneticTestInformation.CourtOrderNo =
          entities.ExistingPatEstab.CourtCaseNumber ?? Spaces(17);
        MoveLegalAction(entities.ExistingPatEstab, export.PatEstab);
      }
    }
    else
    {
      ExitState = "OE0103_UNABLE_TO_READ_GT";

      return;
    }

    export.GeneticTestInformation.LabCaseNo =
      entities.ExistingScheduled.LabCaseNo ?? Spaces(11);
    export.GeneticTestInformation.PaternityExcludedInd =
      entities.ExistingScheduled.PaternityExclusionInd ?? Spaces(1);
    export.GeneticTestInformation.PrevPaternityExcludedInd =
      entities.ExistingScheduled.PaternityExclusionInd ?? Spaces(1);
    export.GeneticTestInformation.PaternityProbability =
      entities.ExistingScheduled.PaternityProbability.GetValueOrDefault();
    export.GeneticTestInformation.ResultContestedInd =
      entities.ExistingScheduled.NoticeOfContestReceivedInd ?? Spaces(1);
    export.GeneticTestInformation.ContestStartedDate =
      entities.ExistingScheduled.StartDateOfContest;
    export.GeneticTestInformation.ContestEndedDate =
      entities.ExistingScheduled.EndDateOfContest;
    export.GeneticTestInformation.ResultReceivedDate =
      entities.ExistingScheduled.TestResultReceivedDate;
    export.GeneticTestInformation.TestType =
      entities.ExistingScheduled.TestType ?? Spaces(2);
    export.GeneticTestInformation.ActualTestDate =
      entities.ExistingScheduled.ActualTestDate;

    if (ReadGeneticTestAccount())
    {
      export.GeneticTestInformation.GeneticTestAccountNo =
        entities.ExistingGeneticTestAccount.AccountNumber;
    }
    else
    {
      ExitState = "OE0034_INVALID_GENETIC_TEST_AC";

      return;
    }

    // ---------------------------------------------
    // Read the latest PERSON_GENETIC_TEST records for the father, mother and 
    // the child.
    // There are two READ statements for each (father, mother and child)
    // The first READ EACH statement checks if the genetic test has been 
    // rescheduled. (i.e. whether there is more than one PERSON GENETIC TEST
    // record exist) so that an indicator is displayed on the screen.
    // The second READ EACH reads the latest PERSON GENETIC TEST record. This is
    // required to keep currency of the record in order to read corresponding
    // vendor subsequently.
    // ---------------------------------------------
    local.NoOfSchedsForFather.Count = 0;
    local.NoOfSchedsForMother.Count = 0;
    local.NoOfSchedsForChild.Count = 0;

    foreach(var item in ReadPersonGeneticTest11())
    {
      ++local.NoOfSchedsForFather.Count;

      if (local.NoOfSchedsForFather.Count > 1)
      {
        export.GeneticTestInformation.FatherRescheduledInd = "Y";

        break;
      }

      if (local.NoOfSchedsForFather.Count == 1)
      {
        // ---------------------------------------------
        // move the latest person genetic test record details for the latest 
        // sample collection schedule.
        // ---------------------------------------------
        local.LatestFather.Identifier =
          entities.ExistingFatherPersonGeneticTest.Identifier;
        export.GeneticTestInformation.FatherCollectSampleInd =
          entities.ExistingFatherPersonGeneticTest.CollectSampleInd ?? Spaces
          (1);
        export.GeneticTestInformation.FatherSampleCollectedInd =
          entities.ExistingFatherPersonGeneticTest.SampleCollectedInd ?? Spaces
          (1);
        export.GeneticTestInformation.FatherReuseSampleInd = "";
        export.GeneticTestInformation.FatherSchedTestDate =
          entities.ExistingFatherPersonGeneticTest.ScheduledTestDate;

        if (Equal(entities.ExistingFatherPersonGeneticTest.ScheduledTestTime,
          TimeSpan.Zero))
        {
          export.GeneticTestInformation.FatherSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            entities.ExistingFatherPersonGeneticTest.ScheduledTestTime.
              GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GeneticTestInformation.FatherSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }

        export.GeneticTestInformation.FatherShowInd =
          entities.ExistingFatherPersonGeneticTest.ShowInd ?? Spaces(1);
        export.GeneticTestInformation.FatherSpecimenId =
          entities.ExistingFatherPersonGeneticTest.SpecimenId ?? Spaces(10);
      }
    }

    if (local.NoOfSchedsForFather.Count < 1)
    {
      ExitState = "OE0060_NF_FATH_PERS_GEN_TEST";

      return;
    }

    if (!ReadPersonGeneticTest5())
    {
      ExitState = "OE0060_NF_FATH_PERS_GEN_TEST";

      return;
    }

    if (entities.ExistingMotherCsePerson.Populated)
    {
      foreach(var item in ReadPersonGeneticTest12())
      {
        ++local.NoOfSchedsForMother.Count;

        if (local.NoOfSchedsForMother.Count > 1)
        {
          export.GeneticTestInformation.MotherRescheduledInd = "Y";

          break;
        }

        if (local.NoOfSchedsForMother.Count == 1)
        {
          // ---------------------------------------------
          // move the latest person genetic test record details for the latest 
          // sample collection schedule.
          // ---------------------------------------------
          local.LatestMother.Identifier =
            entities.ExistingMotherPersonGeneticTest.Identifier;
          export.GeneticTestInformation.MotherCollectSampleInd =
            entities.ExistingMotherPersonGeneticTest.CollectSampleInd ?? Spaces
            (1);
          export.GeneticTestInformation.MotherSampleCollectedInd =
            entities.ExistingMotherPersonGeneticTest.SampleCollectedInd ?? Spaces
            (1);
          export.GeneticTestInformation.MotherReuseSampleInd = "";
          export.GeneticTestInformation.MotherSchedTestDate =
            entities.ExistingMotherPersonGeneticTest.ScheduledTestDate;

          if (Equal(entities.ExistingMotherPersonGeneticTest.ScheduledTestTime,
            TimeSpan.Zero))
          {
            export.GeneticTestInformation.MotherSchedTestTime = "";
          }
          else
          {
            local.WorkTime.TimeWithAmPm = "";
            local.WorkTime.Wtime =
              entities.ExistingMotherPersonGeneticTest.ScheduledTestTime.
                GetValueOrDefault();
            UseCabConvertTimeFormat();
            export.GeneticTestInformation.MotherSchedTestTime =
              local.WorkTime.TimeWithAmPm;
          }

          export.GeneticTestInformation.MotherShowInd =
            entities.ExistingMotherPersonGeneticTest.ShowInd ?? Spaces(1);
          export.GeneticTestInformation.MotherSpecimenId =
            entities.ExistingMotherPersonGeneticTest.SpecimenId ?? Spaces(10);
        }
      }

      if (local.NoOfSchedsForMother.Count < 1)
      {
        ExitState = "OE0063_NF_MOTH_PERS_GEN_TEST";

        return;
      }

      if (!ReadPersonGeneticTest6())
      {
        ExitState = "OE0063_NF_MOTH_PERS_GEN_TEST";

        return;
      }
    }

    foreach(var item in ReadPersonGeneticTest10())
    {
      ++local.NoOfSchedsForChild.Count;

      if (local.NoOfSchedsForChild.Count > 1)
      {
        export.GeneticTestInformation.ChildReschedInd = "Y";

        break;
      }

      if (local.NoOfSchedsForChild.Count == 1)
      {
        local.LatestChild.Identifier =
          entities.ExistingChildPersonGeneticTest.Identifier;
        export.GeneticTestInformation.ChildCollectSampleInd =
          entities.ExistingChildPersonGeneticTest.CollectSampleInd ?? Spaces
          (1);
        export.GeneticTestInformation.ChildSampleCollectedInd =
          entities.ExistingChildPersonGeneticTest.SampleCollectedInd ?? Spaces
          (1);
        export.GeneticTestInformation.ChildReuseSampleInd = "";
        export.GeneticTestInformation.ChildSchedTestDate =
          entities.ExistingChildPersonGeneticTest.ScheduledTestDate;

        if (Equal(entities.ExistingChildPersonGeneticTest.ScheduledTestTime,
          TimeSpan.Zero))
        {
          export.GeneticTestInformation.ChildSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            entities.ExistingChildPersonGeneticTest.ScheduledTestTime.
              GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GeneticTestInformation.ChildSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }

        export.GeneticTestInformation.ChildShowInd =
          entities.ExistingChildPersonGeneticTest.ShowInd ?? Spaces(1);
        export.GeneticTestInformation.ChildSpecimenId =
          entities.ExistingChildPersonGeneticTest.SpecimenId ?? Spaces(10);
      }
    }

    if (local.NoOfSchedsForChild.Count < 1)
    {
      ExitState = "OE0057_NF_CHILD_PERS_GEN_TEST";

      return;
    }

    if (!ReadPersonGeneticTest4())
    {
      ExitState = "OE0057_NF_CHILD_PERS_GEN_TEST";

      return;
    }

    export.GeneticTestInformation.FatherPrevSampExistsInd = "N";

    if (ReadPersonGeneticTest2())
    {
      export.GeneticTestInformation.FatherPrevSampExistsInd = "Y";
    }

    if (entities.ExistingMotherCsePerson.Populated)
    {
      export.GeneticTestInformation.MotherPrevSampExistsInd = "N";

      if (ReadPersonGeneticTest3())
      {
        export.GeneticTestInformation.MotherPrevSampExistsInd = "Y";
      }
    }

    export.GeneticTestInformation.ChildPrevSampExistsInd = "N";

    if (ReadPersonGeneticTest1())
    {
      export.GeneticTestInformation.ChildPrevSampExistsInd = "Y";
    }

    // I00106450   11/27/00   pphinney
    // Removed Expiry Date from READ and PASS it back to PRAD
    // Get most current Address whether it has expired or not.
    if (ReadVendorAddressVendor2())
    {
      export.GeneticTestInformation.TestSiteVendorId =
        NumberToString(entities.ExistingTestSiteVendor.Identifier, 8, 8);
      export.GeneticTestInformation.TestSiteVendorName =
        entities.ExistingTestSiteVendor.Name;
      export.GeneticTestInformation.TestSiteCity =
        entities.ExistingTestSiteVendorAddress.City ?? Spaces(15);
      export.GeneticTestInformation.TestSiteState =
        entities.ExistingTestSiteVendorAddress.State ?? Spaces(2);

      // I00106450   11/27/00   pphinney
      export.PassTestSite.ExpiryDate =
        entities.ExistingTestSiteVendorAddress.ExpiryDate;
    }

    if (ReadPersonGeneticTest8())
    {
      export.GeneticTestInformation.FatherReuseSampleInd = "Y";
      export.GeneticTestInformation.FatherCollectSampleInd = "N";
      export.GeneticTestInformation.FatherSchedTestDate =
        entities.ExistingPrevSampleFatherPersonGeneticTest.ScheduledTestDate;

      if (Equal(entities.ExistingPrevSampleFatherPersonGeneticTest.
        ScheduledTestTime, TimeSpan.Zero))
      {
        export.GeneticTestInformation.FatherSchedTestTime = "";
      }
      else
      {
        local.WorkTime.TimeWithAmPm = "";
        local.WorkTime.Wtime =
          entities.ExistingPrevSampleFatherPersonGeneticTest.ScheduledTestTime.
            GetValueOrDefault();
        UseCabConvertTimeFormat();
        export.GeneticTestInformation.FatherSchedTestTime =
          local.WorkTime.TimeWithAmPm;
      }

      if (ReadGeneticTest4())
      {
        export.GeneticTestInformation.FatherPrevSampGtestNumber =
          entities.ExistingPrevSampleFatherGeneticTest.TestNumber;
        export.GeneticTestInformation.FatherPrevSampleLabCaseNo =
          entities.ExistingPrevSampleFatherGeneticTest.LabCaseNo ?? Spaces(11);
        export.GeneticTestInformation.FatherPrevSampTestType =
          entities.ExistingPrevSampleFatherGeneticTest.TestType ?? Spaces(2);
      }

      export.GeneticTestInformation.FatherPrevSampSpecimenId =
        entities.ExistingPrevSampleFatherPersonGeneticTest.SpecimenId ?? Spaces
        (10);
      export.GeneticTestInformation.FatherPrevSampPerGenTestId =
        entities.ExistingPrevSampleFatherPersonGeneticTest.Identifier;

      if (ReadVendor2())
      {
        export.GeneticTestInformation.FatherDrawSiteId =
          NumberToString(entities.ExistingFatherDrawSite2.Identifier, 8, 8);
        export.GeneticTestInformation.FatherDrawSiteVendorName =
          entities.ExistingFatherDrawSite2.Name;

        if (ReadVendorAddress2())
        {
          export.GeneticTestInformation.FatherDrawSiteCity =
            entities.ExistingFatherDrawsite1.City ?? Spaces(15);
          export.GeneticTestInformation.FatherDrawSiteState =
            entities.ExistingFatherDrawsite1.State ?? Spaces(2);

          // I00106450   11/27/00   pphinney
          export.PassFaDrawSite.ExpiryDate =
            entities.ExistingFatherDrawsite1.ExpiryDate;
        }
      }
    }
    else
    {
      // I00106450   11/27/00   pphinney
      // Removed Expiry Date from READ and PASS it back to PRAD
      if (ReadVendorVendorAddress1())
      {
        export.GeneticTestInformation.FatherDrawSiteId =
          NumberToString(entities.ExistingFatherDrawSite2.Identifier, 8, 8);
        export.GeneticTestInformation.FatherDrawSiteVendorName =
          entities.ExistingFatherDrawSite2.Name;
        export.GeneticTestInformation.FatherDrawSiteCity =
          entities.ExistingFatherDrawsite1.City ?? Spaces(15);
        export.GeneticTestInformation.FatherDrawSiteState =
          entities.ExistingFatherDrawsite1.State ?? Spaces(2);

        // I00106450   11/27/00   pphinney
        export.PassFaDrawSite.ExpiryDate =
          entities.ExistingFatherDrawsite1.ExpiryDate;
      }
    }

    if (entities.ExistingMotherCsePerson.Populated)
    {
      if (ReadPersonGeneticTest9())
      {
        export.GeneticTestInformation.MotherReuseSampleInd = "Y";
        export.GeneticTestInformation.MotherCollectSampleInd = "N";
        export.GeneticTestInformation.MotherSchedTestDate =
          entities.ExistingPrevSampleMotherPersonGeneticTest.ScheduledTestDate;

        if (Equal(entities.ExistingPrevSampleMotherPersonGeneticTest.
          ScheduledTestTime, TimeSpan.Zero))
        {
          export.GeneticTestInformation.MotherSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            entities.ExistingPrevSampleMotherPersonGeneticTest.
              ScheduledTestTime.GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GeneticTestInformation.MotherSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }

        if (ReadGeneticTest5())
        {
          export.GeneticTestInformation.MotherPrevSampGtestNumber =
            entities.ExistingPrevSampleMotherGeneticTest.TestNumber;
          export.GeneticTestInformation.MotherPrevSampLabCaseNo =
            entities.ExistingPrevSampleMotherGeneticTest.LabCaseNo ?? Spaces
            (11);
          export.GeneticTestInformation.MotherPrevSampTestType =
            entities.ExistingPrevSampleMotherGeneticTest.TestType ?? Spaces(2);
        }

        export.GeneticTestInformation.MotherPrevSampSpecimenId =
          entities.ExistingPrevSampleMotherPersonGeneticTest.SpecimenId ?? Spaces
          (10);
        export.GeneticTestInformation.MotherPrevSampPerGenTestId =
          entities.ExistingPrevSampleMotherPersonGeneticTest.Identifier;

        if (ReadVendor3())
        {
          export.GeneticTestInformation.MotherDrawSiteId =
            NumberToString(entities.ExistingMotherDrawSite2.Identifier, 8, 8);
          export.GeneticTestInformation.MotherDrawSiteVendorName =
            entities.ExistingMotherDrawSite2.Name;
        }
        else if (ReadVendorAddress3())
        {
          export.GeneticTestInformation.MotherDrawSiteCity =
            entities.ExistingMotherDrawsite1.City ?? Spaces(15);
          export.GeneticTestInformation.MotherDrawSiteState =
            entities.ExistingMotherDrawsite1.State ?? Spaces(2);

          // I00106450   11/27/00   pphinney
          export.PassMoDrawSite.ExpiryDate =
            entities.ExistingMotherDrawsite1.ExpiryDate;
        }
      }
      else
      {
        // I00106450   11/27/00   pphinney
        // Removed Expiry Date from READ and PASS it back to PRAD
        if (ReadVendorVendorAddress2())
        {
          export.GeneticTestInformation.MotherDrawSiteId =
            NumberToString(entities.ExistingMotherDrawSite2.Identifier, 8, 8);
          export.GeneticTestInformation.MotherDrawSiteVendorName =
            entities.ExistingMotherDrawSite2.Name;
          export.GeneticTestInformation.MotherDrawSiteCity =
            entities.ExistingMotherDrawsite1.City ?? Spaces(15);
          export.GeneticTestInformation.MotherDrawSiteState =
            entities.ExistingMotherDrawsite1.State ?? Spaces(2);

          // I00106450   11/27/00   pphinney
          export.PassMoDrawSite.ExpiryDate =
            entities.ExistingMotherDrawsite1.ExpiryDate;
        }
      }
    }

    if (ReadPersonGeneticTest7())
    {
      export.GeneticTestInformation.ChildReuseSampleInd = "Y";
      export.GeneticTestInformation.ChildCollectSampleInd = "N";
      export.GeneticTestInformation.ChildSchedTestDate =
        entities.ExistingPrevSampleChildPersonGeneticTest.ScheduledTestDate;

      if (Equal(entities.ExistingPrevSampleChildPersonGeneticTest.
        ScheduledTestTime, TimeSpan.Zero))
      {
        export.GeneticTestInformation.ChildSchedTestTime = "";
      }
      else
      {
        local.WorkTime.TimeWithAmPm = "";
        local.WorkTime.Wtime =
          entities.ExistingPrevSampleChildPersonGeneticTest.ScheduledTestTime.
            GetValueOrDefault();
        UseCabConvertTimeFormat();
        export.GeneticTestInformation.ChildSchedTestTime =
          local.WorkTime.TimeWithAmPm;
      }

      if (ReadGeneticTest3())
      {
        export.GeneticTestInformation.ChildPrevSampGtestNumber =
          entities.ExistingPrevSampleChildGeneticTest.TestNumber;
        export.GeneticTestInformation.ChildPrevSampLabCaseNo =
          entities.ExistingPrevSampleChildGeneticTest.LabCaseNo ?? Spaces(11);
        export.GeneticTestInformation.ChildPrevSampTestType =
          entities.ExistingPrevSampleChildGeneticTest.TestType ?? Spaces(2);
      }

      export.GeneticTestInformation.ChildPrevSampSpecimenId =
        entities.ExistingPrevSampleChildPersonGeneticTest.SpecimenId ?? Spaces
        (10);
      export.GeneticTestInformation.ChildPrevSampPerGenTestId =
        entities.ExistingPrevSampleChildPersonGeneticTest.Identifier;

      if (ReadVendor1())
      {
        export.GeneticTestInformation.ChildDrawSiteId =
          NumberToString(entities.ExistingChildDrawSite2.Identifier, 8, 8);
        export.GeneticTestInformation.ChildDrawSiteVendorName =
          entities.ExistingChildDrawSite2.Name;

        if (ReadVendorAddress1())
        {
          export.GeneticTestInformation.ChildDrawSiteCity =
            entities.ExistingChildDrawsite1.City ?? Spaces(15);
          export.GeneticTestInformation.ChildDrawSiteState =
            entities.ExistingChildDrawsite1.State ?? Spaces(2);

          // I00106450   11/27/00   pphinney
          export.PassChDrawSite.ExpiryDate =
            entities.ExistingChildDrawsite1.ExpiryDate;
        }
      }
    }
    else
    {
      // I00106450   11/27/00   pphinney
      // Removed Expiry Date from READ and PASS it back to PRAD
      if (ReadVendorAddressVendor1())
      {
        export.GeneticTestInformation.ChildDrawSiteId =
          NumberToString(entities.ExistingChildDrawSite2.Identifier, 8, 8);
        export.GeneticTestInformation.ChildDrawSiteVendorName =
          entities.ExistingChildDrawSite2.Name;
        export.GeneticTestInformation.ChildDrawSiteCity =
          entities.ExistingChildDrawsite1.City ?? Spaces(15);
        export.GeneticTestInformation.ChildDrawSiteState =
          entities.ExistingChildDrawsite1.State ?? Spaces(2);

        // I00106450   11/27/00   pphinney
        export.PassChDrawSite.ExpiryDate =
          entities.ExistingChildDrawsite1.ExpiryDate;
      }
    }

    ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveWorkTime(WorkTime source, WorkTime target)
  {
    target.Wtime = source.Wtime;
    target.TimeWithAmPm = source.TimeWithAmPm;
  }

  private void UseCabConvertTimeFormat()
  {
    var useImport = new CabConvertTimeFormat.Import();
    var useExport = new CabConvertTimeFormat.Export();

    MoveWorkTime(local.WorkTime, useImport.WorkTime);

    Call(CabConvertTimeFormat.Execute, useImport, useExport);

    local.ErrorInTimeConversion.Flag = useExport.ErrorInConversion.Flag;
    MoveWorkTime(useExport.WorkTime, local.WorkTime);
  }

  private void UseCabGetClientDetails1()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingFatherCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabGetClientDetails2()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingMotherCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabGetClientDetails3()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingChildCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.ExistingFatherCaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFatherCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingFatherCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingFatherCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingFatherCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFatherCaseRole.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingFatherCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingFatherCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingFatherCaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ExistingFatherCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingFatherCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingFatherCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingFatherCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingFatherCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFatherCaseRole.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingFatherCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingFatherCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingFatherCaseRole.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.ExistingFatherCaseRole.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFatherCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingFatherCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingFatherCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingFatherCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFatherCaseRole.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingFatherCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingFatherCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingFatherCaseRole.Type1);
      });
  }

  private bool ReadChild1()
  {
    entities.ExistingCaseRoleChild.Populated = false;

    return Read("ReadChild1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleChild.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleChild.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleChild.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleChild.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleChild.Type1);
      });
  }

  private bool ReadChild2()
  {
    entities.ExistingCaseRoleChild.Populated = false;

    return Read("ReadChild2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleChild.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleChild.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleChild.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleChild.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleChild.Type1);
      });
  }

  private bool ReadChild3()
  {
    entities.ExistingCaseRoleChild.Populated = false;

    return Read("ReadChild3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleChild.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleChild.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleChild.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleChild.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleChild.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingChildCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.GeneticTestInformation.ChildPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChildCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingFatherCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.GeneticTestInformation.FatherPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingFatherCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingFatherCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.ExistingMotherCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.GeneticTestInformation.MotherPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingMotherCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingMotherCsePerson.Populated = true;
      });
  }

  private bool ReadGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFatherCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleMother.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);
    entities.ExistingScheduled.Populated = false;

    return Read("ReadGeneticTest1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croAIdentifier",
          entities.ExistingFatherCaseRole.Identifier);
        db.SetNullableString(
          command, "croAType", entities.ExistingFatherCaseRole.Type1);
        db.SetNullableString(
          command, "casANumber", entities.ExistingFatherCaseRole.CasNumber);
        db.SetNullableString(
          command, "cspANumber", entities.ExistingFatherCaseRole.CspNumber);
        db.SetNullableInt32(
          command, "croMIdentifier",
          entities.ExistingCaseRoleMother.Identifier);
        db.SetNullableString(
          command, "croMType", entities.ExistingCaseRoleMother.Type1);
        db.SetNullableString(
          command, "casMNumber", entities.ExistingCaseRoleMother.CasNumber);
        db.SetNullableString(
          command, "cspMNumber", entities.ExistingCaseRoleMother.CspNumber);
        db.SetNullableInt32(
          command, "croIdentifier", entities.ExistingCaseRoleChild.Identifier);
        db.SetNullableString(
          command, "croType", entities.ExistingCaseRoleChild.Type1);
        db.SetNullableString(
          command, "casNumber", entities.ExistingCaseRoleChild.CasNumber);
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCaseRoleChild.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingScheduled.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingScheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.ExistingScheduled.TestType = db.GetNullableString(reader, 2);
        entities.ExistingScheduled.ActualTestDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingScheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingScheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.ExistingScheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingScheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingScheduled.StartDateOfContest =
          db.GetNullableDate(reader, 8);
        entities.ExistingScheduled.EndDateOfContest =
          db.GetNullableDate(reader, 9);
        entities.ExistingScheduled.GtaAccountNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingScheduled.VenIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingScheduled.CasNumber = db.GetNullableString(reader, 12);
        entities.ExistingScheduled.CspNumber = db.GetNullableString(reader, 13);
        entities.ExistingScheduled.CroType = db.GetNullableString(reader, 14);
        entities.ExistingScheduled.CroIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingScheduled.LgaIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.ExistingScheduled.CasMNumber =
          db.GetNullableString(reader, 17);
        entities.ExistingScheduled.CspMNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingScheduled.CroMType = db.GetNullableString(reader, 19);
        entities.ExistingScheduled.CroMIdentifier =
          db.GetNullableInt32(reader, 20);
        entities.ExistingScheduled.CasANumber =
          db.GetNullableString(reader, 21);
        entities.ExistingScheduled.CspANumber =
          db.GetNullableString(reader, 22);
        entities.ExistingScheduled.CroAType = db.GetNullableString(reader, 23);
        entities.ExistingScheduled.CroAIdentifier =
          db.GetNullableInt32(reader, 24);
        entities.ExistingScheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingScheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.ExistingScheduled.CroMType);
          
        CheckValid<GeneticTest>("CroAType", entities.ExistingScheduled.CroAType);
          
      });
  }

  private bool ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFatherCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);
    entities.ExistingScheduled.Populated = false;

    return Read("ReadGeneticTest2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croAIdentifier",
          entities.ExistingFatherCaseRole.Identifier);
        db.SetNullableString(
          command, "croAType", entities.ExistingFatherCaseRole.Type1);
        db.SetNullableString(
          command, "casANumber", entities.ExistingFatherCaseRole.CasNumber);
        db.SetNullableString(
          command, "cspANumber", entities.ExistingFatherCaseRole.CspNumber);
        db.SetNullableInt32(
          command, "croIdentifier", entities.ExistingCaseRoleChild.Identifier);
        db.SetNullableString(
          command, "croType", entities.ExistingCaseRoleChild.Type1);
        db.SetNullableString(
          command, "casNumber", entities.ExistingCaseRoleChild.CasNumber);
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCaseRoleChild.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingScheduled.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingScheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.ExistingScheduled.TestType = db.GetNullableString(reader, 2);
        entities.ExistingScheduled.ActualTestDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingScheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingScheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.ExistingScheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingScheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingScheduled.StartDateOfContest =
          db.GetNullableDate(reader, 8);
        entities.ExistingScheduled.EndDateOfContest =
          db.GetNullableDate(reader, 9);
        entities.ExistingScheduled.GtaAccountNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingScheduled.VenIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingScheduled.CasNumber = db.GetNullableString(reader, 12);
        entities.ExistingScheduled.CspNumber = db.GetNullableString(reader, 13);
        entities.ExistingScheduled.CroType = db.GetNullableString(reader, 14);
        entities.ExistingScheduled.CroIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingScheduled.LgaIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.ExistingScheduled.CasMNumber =
          db.GetNullableString(reader, 17);
        entities.ExistingScheduled.CspMNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingScheduled.CroMType = db.GetNullableString(reader, 19);
        entities.ExistingScheduled.CroMIdentifier =
          db.GetNullableInt32(reader, 20);
        entities.ExistingScheduled.CasANumber =
          db.GetNullableString(reader, 21);
        entities.ExistingScheduled.CspANumber =
          db.GetNullableString(reader, 22);
        entities.ExistingScheduled.CroAType = db.GetNullableString(reader, 23);
        entities.ExistingScheduled.CroAIdentifier =
          db.GetNullableInt32(reader, 24);
        entities.ExistingScheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingScheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.ExistingScheduled.CroMType);
          
        CheckValid<GeneticTest>("CroAType", entities.ExistingScheduled.CroAType);
          
      });
  }

  private bool ReadGeneticTest3()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleChildPersonGeneticTest.Populated);
    entities.ExistingPrevSampleChildGeneticTest.Populated = false;

    return Read("ReadGeneticTest3",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber",
          entities.ExistingPrevSampleChildPersonGeneticTest.GteTestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleChildGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleChildGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevSampleChildGeneticTest.TestType =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevSampleChildGeneticTest.Populated = true;
      });
  }

  private bool ReadGeneticTest4()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleFatherPersonGeneticTest.Populated);
    entities.ExistingPrevSampleFatherGeneticTest.Populated = false;

    return Read("ReadGeneticTest4",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber",
          entities.ExistingPrevSampleFatherPersonGeneticTest.GteTestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleFatherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleFatherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevSampleFatherGeneticTest.TestType =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevSampleFatherGeneticTest.Populated = true;
      });
  }

  private bool ReadGeneticTest5()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleMotherPersonGeneticTest.Populated);
    entities.ExistingPrevSampleMotherGeneticTest.Populated = false;

    return Read("ReadGeneticTest5",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber",
          entities.ExistingPrevSampleMotherPersonGeneticTest.GteTestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleMotherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleMotherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevSampleMotherGeneticTest.TestType =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevSampleMotherGeneticTest.Populated = true;
      });
  }

  private bool ReadGeneticTest6()
  {
    entities.ExistingScheduled.Populated = false;

    return Read("ReadGeneticTest6",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", local.Latest.TestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingScheduled.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingScheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.ExistingScheduled.TestType = db.GetNullableString(reader, 2);
        entities.ExistingScheduled.ActualTestDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingScheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingScheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.ExistingScheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingScheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingScheduled.StartDateOfContest =
          db.GetNullableDate(reader, 8);
        entities.ExistingScheduled.EndDateOfContest =
          db.GetNullableDate(reader, 9);
        entities.ExistingScheduled.GtaAccountNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingScheduled.VenIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingScheduled.CasNumber = db.GetNullableString(reader, 12);
        entities.ExistingScheduled.CspNumber = db.GetNullableString(reader, 13);
        entities.ExistingScheduled.CroType = db.GetNullableString(reader, 14);
        entities.ExistingScheduled.CroIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingScheduled.LgaIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.ExistingScheduled.CasMNumber =
          db.GetNullableString(reader, 17);
        entities.ExistingScheduled.CspMNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingScheduled.CroMType = db.GetNullableString(reader, 19);
        entities.ExistingScheduled.CroMIdentifier =
          db.GetNullableInt32(reader, 20);
        entities.ExistingScheduled.CasANumber =
          db.GetNullableString(reader, 21);
        entities.ExistingScheduled.CspANumber =
          db.GetNullableString(reader, 22);
        entities.ExistingScheduled.CroAType = db.GetNullableString(reader, 23);
        entities.ExistingScheduled.CroAIdentifier =
          db.GetNullableInt32(reader, 24);
        entities.ExistingScheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingScheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.ExistingScheduled.CroMType);
          
        CheckValid<GeneticTest>("CroAType", entities.ExistingScheduled.CroAType);
          
      });
  }

  private bool ReadGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingScheduled.Populated);
    entities.ExistingGeneticTestAccount.Populated = false;

    return Read("ReadGeneticTestAccount",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber",
          entities.ExistingScheduled.GtaAccountNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingGeneticTestAccount.AccountNumber =
          db.GetString(reader, 0);
        entities.ExistingGeneticTestAccount.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingScheduled.Populated);
    entities.ExistingPatEstab.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingScheduled.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPatEstab.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPatEstab.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingPatEstab.Populated = true;
      });
  }

  private bool ReadMother1()
  {
    entities.ExistingCaseRoleMother.Populated = false;

    return Read("ReadMother1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleMother.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleMother.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleMother.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleMother.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleMother.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleMother.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleMother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleMother.Type1);
      });
  }

  private bool ReadMother2()
  {
    entities.ExistingCaseRoleMother.Populated = false;

    return Read("ReadMother2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleMother.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleMother.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleMother.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleMother.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleMother.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleMother.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleMother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleMother.Type1);
      });
  }

  private bool ReadMother3()
  {
    entities.ExistingCaseRoleMother.Populated = false;

    return Read("ReadMother3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleMother.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleMother.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleMother.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleMother.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleMother.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleMother.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleMother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleMother.Type1);
      });
  }

  private bool ReadPersonGeneticTest1()
  {
    entities.ExistingReusableSampleChild.Populated = false;

    return Read("ReadPersonGeneticTest1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetNullableString(
          command, "specimenId",
          entities.ExistingChildPersonGeneticTest.SpecimenId ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingReusableSampleChild.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingReusableSampleChild.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingReusableSampleChild.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingReusableSampleChild.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingReusableSampleChild.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ExistingReusableSampleChild.ScheduledTestDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingReusableSampleChild.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingReusableSampleChild.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonGeneticTest10()
  {
    entities.ExistingChildPersonGeneticTest.Populated = false;

    return ReadEach("ReadPersonGeneticTest10",
      (db, command) =>
      {
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingChildPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingChildPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingChildPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingChildPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingChildPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingChildPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingChildPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingChildPersonGeneticTest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonGeneticTest11()
  {
    entities.ExistingFatherPersonGeneticTest.Populated = false;

    return ReadEach("ReadPersonGeneticTest11",
      (db, command) =>
      {
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingFatherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingFatherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingFatherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingFatherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingFatherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingFatherPersonGeneticTest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonGeneticTest12()
  {
    entities.ExistingMotherPersonGeneticTest.Populated = false;

    return ReadEach("ReadPersonGeneticTest12",
      (db, command) =>
      {
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingMotherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingMotherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingMotherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingMotherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingMotherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingMotherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingMotherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingMotherPersonGeneticTest.Populated = true;

        return true;
      });
  }

  private bool ReadPersonGeneticTest2()
  {
    entities.ExistingReusableSampleFather.Populated = false;

    return Read("ReadPersonGeneticTest2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetNullableString(
          command, "specimenId",
          entities.ExistingFatherPersonGeneticTest.SpecimenId ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingReusableSampleFather.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingReusableSampleFather.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingReusableSampleFather.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingReusableSampleFather.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingReusableSampleFather.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ExistingReusableSampleFather.ScheduledTestDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingReusableSampleFather.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingReusableSampleFather.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest3()
  {
    entities.ExistingReusableSampleMother.Populated = false;

    return Read("ReadPersonGeneticTest3",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
        db.SetNullableString(
          command, "specimenId",
          entities.ExistingMotherPersonGeneticTest.SpecimenId ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingReusableSampleMother.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingReusableSampleMother.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingReusableSampleMother.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingReusableSampleMother.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingReusableSampleMother.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ExistingReusableSampleMother.ScheduledTestDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingReusableSampleMother.PgtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingReusableSampleMother.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest4()
  {
    entities.ExistingChildPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest4",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetInt32(command, "identifier", local.LatestChild.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingChildPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingChildPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingChildPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingChildPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingChildPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingChildPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingChildPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingChildPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest5()
  {
    entities.ExistingFatherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest5",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetInt32(command, "identifier", local.LatestFather.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingFatherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingFatherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingFatherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingFatherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingFatherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingFatherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest6()
  {
    entities.ExistingMotherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest6",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetInt32(command, "identifier", local.LatestMother.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingMotherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingMotherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingMotherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingMotherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingMotherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingMotherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingMotherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingMotherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest7()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingChildPersonGeneticTest.Populated);
    entities.ExistingPrevSampleChildPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest7",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingChildPersonGeneticTest.PgtIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingChildPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingChildPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevSampleChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevSampleChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleChildPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ExistingPrevSampleChildPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingPrevSampleChildPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPrevSampleChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPrevSampleChildPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest8()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherPersonGeneticTest.Populated);
    entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest8",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingFatherPersonGeneticTest.PgtIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingFatherPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingFatherPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevSampleFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevSampleFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleFatherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ExistingPrevSampleFatherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingPrevSampleFatherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPrevSampleFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest9()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingMotherPersonGeneticTest.Populated);
    entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest9",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingMotherPersonGeneticTest.PgtIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingMotherPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingMotherPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevSampleMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevSampleMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleMotherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ExistingPrevSampleMotherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingPrevSampleMotherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPrevSampleMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadVendor1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleChildPersonGeneticTest.Populated);
    entities.ExistingChildDrawSite2.Populated = false;

    return Read("ReadVendor1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPrevSampleChildPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChildDrawSite2.Identifier = db.GetInt32(reader, 0);
        entities.ExistingChildDrawSite2.Name = db.GetString(reader, 1);
        entities.ExistingChildDrawSite2.Number =
          db.GetNullableString(reader, 2);
        entities.ExistingChildDrawSite2.Populated = true;
      });
  }

  private bool ReadVendor2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleFatherPersonGeneticTest.Populated);
    entities.ExistingFatherDrawSite2.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPrevSampleFatherPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFatherDrawSite2.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFatherDrawSite2.Name = db.GetString(reader, 1);
        entities.ExistingFatherDrawSite2.Number =
          db.GetNullableString(reader, 2);
        entities.ExistingFatherDrawSite2.Populated = true;
      });
  }

  private bool ReadVendor3()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleMotherPersonGeneticTest.Populated);
    entities.ExistingMotherDrawSite2.Populated = false;

    return Read("ReadVendor3",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPrevSampleMotherPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMotherDrawSite2.Identifier = db.GetInt32(reader, 0);
        entities.ExistingMotherDrawSite2.Name = db.GetString(reader, 1);
        entities.ExistingMotherDrawSite2.Number =
          db.GetNullableString(reader, 2);
        entities.ExistingMotherDrawSite2.Populated = true;
      });
  }

  private bool ReadVendorAddress1()
  {
    entities.ExistingChildDrawsite1.Populated = false;

    return Read("ReadVendorAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "venIdentifier", entities.ExistingChildDrawSite2.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingChildDrawsite1.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingChildDrawsite1.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingChildDrawsite1.ExpiryDate = db.GetDate(reader, 2);
        entities.ExistingChildDrawsite1.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingChildDrawsite1.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingChildDrawsite1.City = db.GetNullableString(reader, 5);
        entities.ExistingChildDrawsite1.State = db.GetNullableString(reader, 6);
        entities.ExistingChildDrawsite1.Populated = true;
      });
  }

  private bool ReadVendorAddress2()
  {
    entities.ExistingFatherDrawsite1.Populated = false;

    return Read("ReadVendorAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "venIdentifier",
          entities.ExistingFatherDrawSite2.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFatherDrawsite1.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingFatherDrawsite1.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingFatherDrawsite1.ExpiryDate = db.GetDate(reader, 2);
        entities.ExistingFatherDrawsite1.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingFatherDrawsite1.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingFatherDrawsite1.City = db.GetNullableString(reader, 5);
        entities.ExistingFatherDrawsite1.State =
          db.GetNullableString(reader, 6);
        entities.ExistingFatherDrawsite1.Populated = true;
      });
  }

  private bool ReadVendorAddress3()
  {
    entities.ExistingMotherDrawsite1.Populated = false;

    return Read("ReadVendorAddress3",
      (db, command) =>
      {
        db.SetInt32(
          command, "venIdentifier",
          entities.ExistingMotherDrawSite2.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingMotherDrawsite1.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingMotherDrawsite1.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingMotherDrawsite1.ExpiryDate = db.GetDate(reader, 2);
        entities.ExistingMotherDrawsite1.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingMotherDrawsite1.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingMotherDrawsite1.City = db.GetNullableString(reader, 5);
        entities.ExistingMotherDrawsite1.State =
          db.GetNullableString(reader, 6);
        entities.ExistingMotherDrawsite1.Populated = true;
      });
  }

  private bool ReadVendorAddressVendor1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingChildPersonGeneticTest.Populated);
    entities.ExistingChildDrawsite1.Populated = false;
    entities.ExistingChildDrawSite2.Populated = false;

    return Read("ReadVendorAddressVendor1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingChildPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChildDrawsite1.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingChildDrawSite2.Identifier = db.GetInt32(reader, 0);
        entities.ExistingChildDrawsite1.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingChildDrawsite1.ExpiryDate = db.GetDate(reader, 2);
        entities.ExistingChildDrawsite1.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingChildDrawsite1.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingChildDrawsite1.City = db.GetNullableString(reader, 5);
        entities.ExistingChildDrawsite1.State = db.GetNullableString(reader, 6);
        entities.ExistingChildDrawSite2.Name = db.GetString(reader, 7);
        entities.ExistingChildDrawSite2.Number =
          db.GetNullableString(reader, 8);
        entities.ExistingChildDrawsite1.Populated = true;
        entities.ExistingChildDrawSite2.Populated = true;
      });
  }

  private bool ReadVendorAddressVendor2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingScheduled.Populated);
    entities.ExistingTestSiteVendorAddress.Populated = false;
    entities.ExistingTestSiteVendor.Populated = false;

    return Read("ReadVendorAddressVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingScheduled.VenIdentifier.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTestSiteVendorAddress.VenIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingTestSiteVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingTestSiteVendorAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingTestSiteVendorAddress.ExpiryDate =
          db.GetDate(reader, 2);
        entities.ExistingTestSiteVendorAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingTestSiteVendorAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingTestSiteVendorAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingTestSiteVendorAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingTestSiteVendorAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingTestSiteVendorAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingTestSiteVendorAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingTestSiteVendorAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingTestSiteVendorAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingTestSiteVendorAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingTestSiteVendorAddress.CreatedBy =
          db.GetString(reader, 13);
        entities.ExistingTestSiteVendorAddress.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingTestSiteVendorAddress.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.ExistingTestSiteVendorAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingTestSiteVendor.Name = db.GetString(reader, 17);
        entities.ExistingTestSiteVendor.Number =
          db.GetNullableString(reader, 18);
        entities.ExistingTestSiteVendorAddress.Populated = true;
        entities.ExistingTestSiteVendor.Populated = true;
      });
  }

  private bool ReadVendorVendorAddress1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherPersonGeneticTest.Populated);
    entities.ExistingFatherDrawsite1.Populated = false;
    entities.ExistingFatherDrawSite2.Populated = false;

    return Read("ReadVendorVendorAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingFatherPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFatherDrawSite2.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFatherDrawsite1.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingFatherDrawSite2.Name = db.GetString(reader, 1);
        entities.ExistingFatherDrawSite2.Number =
          db.GetNullableString(reader, 2);
        entities.ExistingFatherDrawsite1.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingFatherDrawsite1.ExpiryDate = db.GetDate(reader, 4);
        entities.ExistingFatherDrawsite1.Street1 =
          db.GetNullableString(reader, 5);
        entities.ExistingFatherDrawsite1.Street2 =
          db.GetNullableString(reader, 6);
        entities.ExistingFatherDrawsite1.City = db.GetNullableString(reader, 7);
        entities.ExistingFatherDrawsite1.State =
          db.GetNullableString(reader, 8);
        entities.ExistingFatherDrawsite1.Populated = true;
        entities.ExistingFatherDrawSite2.Populated = true;
      });
  }

  private bool ReadVendorVendorAddress2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingMotherPersonGeneticTest.Populated);
    entities.ExistingMotherDrawsite1.Populated = false;
    entities.ExistingMotherDrawSite2.Populated = false;

    return Read("ReadVendorVendorAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingMotherPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMotherDrawSite2.Identifier = db.GetInt32(reader, 0);
        entities.ExistingMotherDrawsite1.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingMotherDrawSite2.Name = db.GetString(reader, 1);
        entities.ExistingMotherDrawSite2.Number =
          db.GetNullableString(reader, 2);
        entities.ExistingMotherDrawsite1.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingMotherDrawsite1.ExpiryDate = db.GetDate(reader, 4);
        entities.ExistingMotherDrawsite1.Street1 =
          db.GetNullableString(reader, 5);
        entities.ExistingMotherDrawsite1.Street2 =
          db.GetNullableString(reader, 6);
        entities.ExistingMotherDrawsite1.City = db.GetNullableString(reader, 7);
        entities.ExistingMotherDrawsite1.State =
          db.GetNullableString(reader, 8);
        entities.ExistingMotherDrawsite1.Populated = true;
        entities.ExistingMotherDrawSite2.Populated = true;
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
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailCsePerson;
      private CaseRole detailCaseRole;
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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

    private Case1 case1;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public Common ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
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
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of PatEstab.
    /// </summary>
    [JsonPropertyName("patEstab")]
    public LegalAction PatEstab
    {
      get => patEstab ??= new();
      set => patEstab = value;
    }

    /// <summary>
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
    }

    /// <summary>
    /// A value of PassTestSite.
    /// </summary>
    [JsonPropertyName("passTestSite")]
    public VendorAddress PassTestSite
    {
      get => passTestSite ??= new();
      set => passTestSite = value;
    }

    /// <summary>
    /// A value of PassChDrawSite.
    /// </summary>
    [JsonPropertyName("passChDrawSite")]
    public VendorAddress PassChDrawSite
    {
      get => passChDrawSite ??= new();
      set => passChDrawSite = value;
    }

    /// <summary>
    /// A value of PassMoDrawSite.
    /// </summary>
    [JsonPropertyName("passMoDrawSite")]
    public VendorAddress PassMoDrawSite
    {
      get => passMoDrawSite ??= new();
      set => passMoDrawSite = value;
    }

    /// <summary>
    /// A value of PassFaDrawSite.
    /// </summary>
    [JsonPropertyName("passFaDrawSite")]
    public VendorAddress PassFaDrawSite
    {
      get => passFaDrawSite ??= new();
      set => passFaDrawSite = value;
    }

    private Common activeChild;
    private Common activeAp;
    private Common caseRoleInactive;
    private Common caseOpen;
    private GeneticTest geneticTest;
    private LegalAction patEstab;
    private GeneticTestInformation geneticTestInformation;
    private VendorAddress passTestSite;
    private VendorAddress passChDrawSite;
    private VendorAddress passMoDrawSite;
    private VendorAddress passFaDrawSite;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of LatestChild.
    /// </summary>
    [JsonPropertyName("latestChild")]
    public PersonGeneticTest LatestChild
    {
      get => latestChild ??= new();
      set => latestChild = value;
    }

    /// <summary>
    /// A value of LatestMother.
    /// </summary>
    [JsonPropertyName("latestMother")]
    public PersonGeneticTest LatestMother
    {
      get => latestMother ??= new();
      set => latestMother = value;
    }

    /// <summary>
    /// A value of LatestFather.
    /// </summary>
    [JsonPropertyName("latestFather")]
    public PersonGeneticTest LatestFather
    {
      get => latestFather ??= new();
      set => latestFather = value;
    }

    /// <summary>
    /// A value of ErrorInTimeConversion.
    /// </summary>
    [JsonPropertyName("errorInTimeConversion")]
    public Common ErrorInTimeConversion
    {
      get => errorInTimeConversion ??= new();
      set => errorInTimeConversion = value;
    }

    /// <summary>
    /// A value of WorkTime.
    /// </summary>
    [JsonPropertyName("workTime")]
    public WorkTime WorkTime
    {
      get => workTime ??= new();
      set => workTime = value;
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
    /// A value of Latest.
    /// </summary>
    [JsonPropertyName("latest")]
    public GeneticTest Latest
    {
      get => latest ??= new();
      set => latest = value;
    }

    /// <summary>
    /// A value of LatestGeneticTestFound.
    /// </summary>
    [JsonPropertyName("latestGeneticTestFound")]
    public Common LatestGeneticTestFound
    {
      get => latestGeneticTestFound ??= new();
      set => latestGeneticTestFound = value;
    }

    /// <summary>
    /// A value of NoOfSchedsForFather.
    /// </summary>
    [JsonPropertyName("noOfSchedsForFather")]
    public Common NoOfSchedsForFather
    {
      get => noOfSchedsForFather ??= new();
      set => noOfSchedsForFather = value;
    }

    /// <summary>
    /// A value of NoOfSchedsForMother.
    /// </summary>
    [JsonPropertyName("noOfSchedsForMother")]
    public Common NoOfSchedsForMother
    {
      get => noOfSchedsForMother ??= new();
      set => noOfSchedsForMother = value;
    }

    /// <summary>
    /// A value of NoOfSchedsForChild.
    /// </summary>
    [JsonPropertyName("noOfSchedsForChild")]
    public Common NoOfSchedsForChild
    {
      get => noOfSchedsForChild ??= new();
      set => noOfSchedsForChild = value;
    }

    /// <summary>
    /// A value of NoOfFathersSelected.
    /// </summary>
    [JsonPropertyName("noOfFathersSelected")]
    public Common NoOfFathersSelected
    {
      get => noOfFathersSelected ??= new();
      set => noOfFathersSelected = value;
    }

    /// <summary>
    /// A value of NoOfMothersSelected.
    /// </summary>
    [JsonPropertyName("noOfMothersSelected")]
    public Common NoOfMothersSelected
    {
      get => noOfMothersSelected ??= new();
      set => noOfMothersSelected = value;
    }

    /// <summary>
    /// A value of NoOfChildrenSelected.
    /// </summary>
    [JsonPropertyName("noOfChildrenSelected")]
    public Common NoOfChildrenSelected
    {
      get => noOfChildrenSelected ??= new();
      set => noOfChildrenSelected = value;
    }

    private DateWorkArea null1;
    private DateWorkArea current;
    private PersonGeneticTest latestChild;
    private PersonGeneticTest latestMother;
    private PersonGeneticTest latestFather;
    private Common errorInTimeConversion;
    private WorkTime workTime;
    private CsePersonsWorkSet csePersonsWorkSet;
    private GeneticTest latest;
    private Common latestGeneticTestFound;
    private Common noOfSchedsForFather;
    private Common noOfSchedsForMother;
    private Common noOfSchedsForChild;
    private Common noOfFathersSelected;
    private Common noOfMothersSelected;
    private Common noOfChildrenSelected;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPatEstab.
    /// </summary>
    [JsonPropertyName("existingPatEstab")]
    public LegalAction ExistingPatEstab
    {
      get => existingPatEstab ??= new();
      set => existingPatEstab = value;
    }

    /// <summary>
    /// A value of ExistingReusableSampleChild.
    /// </summary>
    [JsonPropertyName("existingReusableSampleChild")]
    public PersonGeneticTest ExistingReusableSampleChild
    {
      get => existingReusableSampleChild ??= new();
      set => existingReusableSampleChild = value;
    }

    /// <summary>
    /// A value of ExistingReusableSampleMother.
    /// </summary>
    [JsonPropertyName("existingReusableSampleMother")]
    public PersonGeneticTest ExistingReusableSampleMother
    {
      get => existingReusableSampleMother ??= new();
      set => existingReusableSampleMother = value;
    }

    /// <summary>
    /// A value of ExistingReusableSampleFather.
    /// </summary>
    [JsonPropertyName("existingReusableSampleFather")]
    public PersonGeneticTest ExistingReusableSampleFather
    {
      get => existingReusableSampleFather ??= new();
      set => existingReusableSampleFather = value;
    }

    /// <summary>
    /// A value of ExistingFatherCaseRole.
    /// </summary>
    [JsonPropertyName("existingFatherCaseRole")]
    public CaseRole ExistingFatherCaseRole
    {
      get => existingFatherCaseRole ??= new();
      set => existingFatherCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingPrevSampleChildGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleChildGeneticTest")]
    public GeneticTest ExistingPrevSampleChildGeneticTest
    {
      get => existingPrevSampleChildGeneticTest ??= new();
      set => existingPrevSampleChildGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingPrevSampleMotherGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleMotherGeneticTest")]
    public GeneticTest ExistingPrevSampleMotherGeneticTest
    {
      get => existingPrevSampleMotherGeneticTest ??= new();
      set => existingPrevSampleMotherGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingPrevSampleFatherGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleFatherGeneticTest")]
    public GeneticTest ExistingPrevSampleFatherGeneticTest
    {
      get => existingPrevSampleFatherGeneticTest ??= new();
      set => existingPrevSampleFatherGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingPrevSampleChildPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleChildPersonGeneticTest")]
    public PersonGeneticTest ExistingPrevSampleChildPersonGeneticTest
    {
      get => existingPrevSampleChildPersonGeneticTest ??= new();
      set => existingPrevSampleChildPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingPrevSampleMotherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleMotherPersonGeneticTest")]
    public PersonGeneticTest ExistingPrevSampleMotherPersonGeneticTest
    {
      get => existingPrevSampleMotherPersonGeneticTest ??= new();
      set => existingPrevSampleMotherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingPrevSampleFatherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleFatherPersonGeneticTest")]
    public PersonGeneticTest ExistingPrevSampleFatherPersonGeneticTest
    {
      get => existingPrevSampleFatherPersonGeneticTest ??= new();
      set => existingPrevSampleFatherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingGeneticTestAccount.
    /// </summary>
    [JsonPropertyName("existingGeneticTestAccount")]
    public GeneticTestAccount ExistingGeneticTestAccount
    {
      get => existingGeneticTestAccount ??= new();
      set => existingGeneticTestAccount = value;
    }

    /// <summary>
    /// A value of ExistingChildDrawsite1.
    /// </summary>
    [JsonPropertyName("existingChildDrawsite1")]
    public VendorAddress ExistingChildDrawsite1
    {
      get => existingChildDrawsite1 ??= new();
      set => existingChildDrawsite1 = value;
    }

    /// <summary>
    /// A value of ExistingMotherDrawsite1.
    /// </summary>
    [JsonPropertyName("existingMotherDrawsite1")]
    public VendorAddress ExistingMotherDrawsite1
    {
      get => existingMotherDrawsite1 ??= new();
      set => existingMotherDrawsite1 = value;
    }

    /// <summary>
    /// A value of ExistingFatherDrawsite1.
    /// </summary>
    [JsonPropertyName("existingFatherDrawsite1")]
    public VendorAddress ExistingFatherDrawsite1
    {
      get => existingFatherDrawsite1 ??= new();
      set => existingFatherDrawsite1 = value;
    }

    /// <summary>
    /// A value of ExistingTestSiteVendorAddress.
    /// </summary>
    [JsonPropertyName("existingTestSiteVendorAddress")]
    public VendorAddress ExistingTestSiteVendorAddress
    {
      get => existingTestSiteVendorAddress ??= new();
      set => existingTestSiteVendorAddress = value;
    }

    /// <summary>
    /// A value of ExistingTestSiteVendor.
    /// </summary>
    [JsonPropertyName("existingTestSiteVendor")]
    public Vendor ExistingTestSiteVendor
    {
      get => existingTestSiteVendor ??= new();
      set => existingTestSiteVendor = value;
    }

    /// <summary>
    /// A value of ExistingFatherDrawSite2.
    /// </summary>
    [JsonPropertyName("existingFatherDrawSite2")]
    public Vendor ExistingFatherDrawSite2
    {
      get => existingFatherDrawSite2 ??= new();
      set => existingFatherDrawSite2 = value;
    }

    /// <summary>
    /// A value of ExistingMotherDrawSite2.
    /// </summary>
    [JsonPropertyName("existingMotherDrawSite2")]
    public Vendor ExistingMotherDrawSite2
    {
      get => existingMotherDrawSite2 ??= new();
      set => existingMotherDrawSite2 = value;
    }

    /// <summary>
    /// A value of ExistingChildDrawSite2.
    /// </summary>
    [JsonPropertyName("existingChildDrawSite2")]
    public Vendor ExistingChildDrawSite2
    {
      get => existingChildDrawSite2 ??= new();
      set => existingChildDrawSite2 = value;
    }

    /// <summary>
    /// A value of ExistingFatherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingFatherPersonGeneticTest")]
    public PersonGeneticTest ExistingFatherPersonGeneticTest
    {
      get => existingFatherPersonGeneticTest ??= new();
      set => existingFatherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingMotherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingMotherPersonGeneticTest")]
    public PersonGeneticTest ExistingMotherPersonGeneticTest
    {
      get => existingMotherPersonGeneticTest ??= new();
      set => existingMotherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingChildPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingChildPersonGeneticTest")]
    public PersonGeneticTest ExistingChildPersonGeneticTest
    {
      get => existingChildPersonGeneticTest ??= new();
      set => existingChildPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingScheduled.
    /// </summary>
    [JsonPropertyName("existingScheduled")]
    public GeneticTest ExistingScheduled
    {
      get => existingScheduled ??= new();
      set => existingScheduled = value;
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
    /// A value of ExistingCaseRoleMother.
    /// </summary>
    [JsonPropertyName("existingCaseRoleMother")]
    public CaseRole ExistingCaseRoleMother
    {
      get => existingCaseRoleMother ??= new();
      set => existingCaseRoleMother = value;
    }

    /// <summary>
    /// A value of ExistingCaseRoleChild.
    /// </summary>
    [JsonPropertyName("existingCaseRoleChild")]
    public CaseRole ExistingCaseRoleChild
    {
      get => existingCaseRoleChild ??= new();
      set => existingCaseRoleChild = value;
    }

    /// <summary>
    /// A value of ExistingFatherCsePerson.
    /// </summary>
    [JsonPropertyName("existingFatherCsePerson")]
    public CsePerson ExistingFatherCsePerson
    {
      get => existingFatherCsePerson ??= new();
      set => existingFatherCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingMotherCsePerson.
    /// </summary>
    [JsonPropertyName("existingMotherCsePerson")]
    public CsePerson ExistingMotherCsePerson
    {
      get => existingMotherCsePerson ??= new();
      set => existingMotherCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingChildCsePerson.
    /// </summary>
    [JsonPropertyName("existingChildCsePerson")]
    public CsePerson ExistingChildCsePerson
    {
      get => existingChildCsePerson ??= new();
      set => existingChildCsePerson = value;
    }

    private LegalAction existingPatEstab;
    private PersonGeneticTest existingReusableSampleChild;
    private PersonGeneticTest existingReusableSampleMother;
    private PersonGeneticTest existingReusableSampleFather;
    private CaseRole existingFatherCaseRole;
    private GeneticTest existingPrevSampleChildGeneticTest;
    private GeneticTest existingPrevSampleMotherGeneticTest;
    private GeneticTest existingPrevSampleFatherGeneticTest;
    private PersonGeneticTest existingPrevSampleChildPersonGeneticTest;
    private PersonGeneticTest existingPrevSampleMotherPersonGeneticTest;
    private PersonGeneticTest existingPrevSampleFatherPersonGeneticTest;
    private GeneticTestAccount existingGeneticTestAccount;
    private VendorAddress existingChildDrawsite1;
    private VendorAddress existingMotherDrawsite1;
    private VendorAddress existingFatherDrawsite1;
    private VendorAddress existingTestSiteVendorAddress;
    private Vendor existingTestSiteVendor;
    private Vendor existingFatherDrawSite2;
    private Vendor existingMotherDrawSite2;
    private Vendor existingChildDrawSite2;
    private PersonGeneticTest existingFatherPersonGeneticTest;
    private PersonGeneticTest existingMotherPersonGeneticTest;
    private PersonGeneticTest existingChildPersonGeneticTest;
    private GeneticTest existingScheduled;
    private Case1 existingCase;
    private CaseRole existingCaseRoleMother;
    private CaseRole existingCaseRoleChild;
    private CsePerson existingFatherCsePerson;
    private CsePerson existingMotherCsePerson;
    private CsePerson existingChildCsePerson;
  }
#endregion
}
