// Program: OE_SCHEDULE_GENETIC_TEST, ID: 371797159, model: 746.
// Short name: SWE00964
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_SCHEDULE_GENETIC_TEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This BAA process action block updates genetic test schedule details for a 
/// genetic test.
/// </para>
/// </summary>
[Serializable]
public partial class OeScheduleGeneticTest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_SCHEDULE_GENETIC_TEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeScheduleGeneticTest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeScheduleGeneticTest.
  /// </summary>
  public OeScheduleGeneticTest(IContext context, Import import, Export export):
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
    // AUTHOR	DATE	CHGREQ	DESCRIPTION
    // govind	12-19-94	Initial coding
    // SHERAZ	4/29/97		CHANGE CURRENT_DATE
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG ****************
    local.Current.Date = Now().Date;

    // *********************************************
    // SYSTEM:		KESSEP
    // DESCRIPTION:
    // This BAA Process Action Block creates/ updates genetic test schedule.
    // PROCESSING:
    // This action block is passed the screen input of schedule genetic test.
    // It checks if a GENETIC_TEST record already exists for the given CASE-
    // FATHER-MOTHER-CHILD
    // If one exists, then this is an update/ reschedule of an existing genetic 
    // test. If one does not exist, it creates genetic test schedule.
    // If an existing scheduled test date or time is changed, then a new 
    // PERSON_GENETIC_TEST record is created to keep the history.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CASE			- R - -
    // 	CASE_ROLE		- R - -
    // 		FATHER
    // 		MOTHER
    // 		CHILD
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		C R U -
    // 	PERSON_GENETIC_TEST	C R U -
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj.
    // DATE CREATED:	12-28-1994.
    // *********************************************
    // -------------------------------------------------------------------
    // PR# 81845            Vithal Madhira          03/31/2000
    // It is decided that 'Mother' is optional on GTSC screen. The users want 
    // the ability to do 'Motherless Comparisons'  of the genetic test results.
    // The code is modified accordingly.
    // -------------------------------------------------------------------
    export.GeneticTestInformation.Assign(import.GeneticTestInformation);

    if (!ReadCase())
    {
      ExitState = "OE0020_INVALID_CASE_NO";

      return;
    }

    if (import.LegalAction.Identifier != 0)
    {
      if (!ReadLegalAction1())
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }
    }

    if (!ReadCsePerson2())
    {
      ExitState = "OE0059_NF_FATHER_CSE_PERSON";

      return;
    }

    if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
    {
      if (!ReadCsePerson3())
      {
        ExitState = "OE0062_NF_MOTHER_CSE_PERSON";

        return;
      }
    }

    if (!ReadCsePerson1())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    if (!ReadAbsentParent())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (entities.ExistingMother.Populated)
    {
      if (!ReadMother())
      {
        ExitState = "OE0055_NF_CASE_ROLE_MOTHER";

        return;
      }
    }

    if (!ReadChild())
    {
      ExitState = "OE0065_NF_CASE_ROLE_CHILD";

      return;
    }

    if (!ReadGeneticTestAccount())
    {
      ExitState = "OE0034_INVALID_GENETIC_TEST_AC";

      return;
    }

    local.LatestGeneticTestFound.Flag = "N";
    local.Temp00010101.TestResultReceivedDate = new DateTime(1, 1, 1);

    if (entities.ExistingCaseRoleMother.Populated)
    {
      if (ReadGeneticTest2())
      {
        local.LatestGeneticTestFound.Flag = "Y";
        local.Latest.TestNumber = entities.Scheduled.TestNumber;
      }
    }
    else if (ReadGeneticTest3())
    {
      local.LatestGeneticTestFound.Flag = "Y";
      local.Latest.TestNumber = entities.Scheduled.TestNumber;
    }

    if (AsChar(local.LatestGeneticTestFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // A genetic test has not been scheduled yet. So create one now.
      // ---------------------------------------------
      // *********************************************
      // Code to handle contention for last genetic_test test number to be added
      // here.
      // *********************************************
      local.LastId.TestNumber = 0;

      if (ReadGeneticTest1())
      {
        local.LastId.TestNumber = entities.ExistingLast.TestNumber;
      }

      if (entities.ExistingCaseRoleMother.Populated)
      {
        try
        {
          CreateGeneticTest1();

          if (import.LegalAction.Identifier != 0)
          {
            AssociateGeneticTest();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0014_ERR_CRE_GENETIC_TEST";

              return;
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
        try
        {
          CreateGeneticTest2();

          if (import.LegalAction.Identifier != 0)
          {
            AssociateGeneticTest();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0014_ERR_CRE_GENETIC_TEST";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // A genetic test has already been scheduled. So it is an update to an 
      // existing genetic_test record.
      // ---------------------------------------------
      if (!ReadGeneticTest4())
      {
        ExitState = "OE0103_UNABLE_TO_READ_GT";

        return;
      }

      if (!Equal(entities.Scheduled.TestType,
        export.GeneticTestInformation.TestType))
      {
        try
        {
          UpdateGeneticTest2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0016_ERR_UPD_GENETIC_TEST";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (ReadLegalAction2())
      {
        if (import.LegalAction.Identifier == 0)
        {
          DisassociateGeneticTest();
        }
        else if (import.LegalAction.Identifier != entities
          .ExistingPrevPatEstab.Identifier)
        {
          AssociateGeneticTest();
        }
      }
      else if (import.LegalAction.Identifier != 0)
      {
        AssociateGeneticTest();
      }
    }

    // ---------------------------------------------
    // Process father's person_genetic_test info
    // ---------------------------------------------
    local.FatherPersGenTestFound.Flag = "N";

    if (ReadPersonGeneticTest2())
    {
      local.FatherPersGenTestFound.Flag = "Y";
      local.NextAvailablePersonGeneticTest.Identifier =
        entities.ScheduledFathers.Identifier + 1;
    }

    local.WorkTime.TimeWithAmPm =
      export.GeneticTestInformation.FatherSchedTestTime;

    if (IsEmpty(local.WorkTime.TimeWithAmPm))
    {
      local.WorkTime.Wtime = TimeSpan.Zero;
    }
    else
    {
      UseCabConvertTimeFormat();
    }

    if (AsChar(local.FatherPersGenTestFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // No PERSON_GENETIC_TEST record found. Create a new one irrespective of 
      // whether drawing of sample has been scheduled or not.
      // ---------------------------------------------
      try
      {
        CreatePersonGeneticTest4();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0015_ERR_CRE_PERS_GEN_TEST";

            return;
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
      // ---------------------------------------------
      // Latest PERSON_GENETIC_TEST for father found.
      // ---------------------------------------------
      if (!Equal(entities.ScheduledFathers.ScheduledTestDate,
        export.GeneticTestInformation.FatherSchedTestDate) || !
        Equal(entities.ScheduledFathers.ScheduledTestTime, local.WorkTime.Wtime))
        
      {
        if (!Lt(new DateTime(1, 1, 1),
          entities.ScheduledFathers.ScheduledTestDate))
        {
          // ---------------------------------------------
          // The date was not fixed before. It is being fixed only now.
          // ---------------------------------------------
          try
          {
            UpdatePersonGeneticTest6();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

                return;
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
          // ---------------------------------------------
          // Schedule has been changed. So create a new re-scheduled record.
          // ---------------------------------------------
          try
          {
            CreatePersonGeneticTest3();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0015_ERR_CRE_PERS_GEN_TEST";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        // ---------------------------------------------
        // Scheduled date and time not changed. Check and update if Collect 
        // Sample Indicator has been changed.
        // ---------------------------------------------
        if (AsChar(entities.ScheduledFathers.CollectSampleInd) != AsChar
          (export.GeneticTestInformation.FatherCollectSampleInd))
        {
          try
          {
            UpdatePersonGeneticTest5();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    // ---------------------------------------------
    // Process mother's person_genetic_test info
    // ---------------------------------------------
    if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
    {
      local.MotherPersGenTestFound.Flag = "N";

      if (ReadPersonGeneticTest3())
      {
        local.MotherPersGenTestFound.Flag = "Y";
        local.NextAvailablePersonGeneticTest.Identifier =
          entities.ScheduledMothers.Identifier + 1;
      }

      local.WorkTime.TimeWithAmPm =
        export.GeneticTestInformation.MotherSchedTestTime;

      if (IsEmpty(local.WorkTime.TimeWithAmPm))
      {
        local.WorkTime.Wtime = TimeSpan.Zero;
      }
      else
      {
        UseCabConvertTimeFormat();
      }

      if (AsChar(local.MotherPersGenTestFound.Flag) == 'N')
      {
        // ---------------------------------------------
        // No PERSON_GENETIC_TEST record found. Create a new one.
        // ---------------------------------------------
        try
        {
          CreatePersonGeneticTest6();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0015_ERR_CRE_PERS_GEN_TEST";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else if (!Equal(entities.ScheduledMothers.ScheduledTestDate,
        export.GeneticTestInformation.MotherSchedTestDate) || !
        Equal(entities.ScheduledMothers.ScheduledTestTime, local.WorkTime.Wtime))
        
      {
        if (!Lt(new DateTime(1, 1, 1),
          entities.ScheduledMothers.ScheduledTestDate))
        {
          // ---------------------------------------------
          // The date was not fixed before. It is being fixed only now.
          // ---------------------------------------------
          try
          {
            UpdatePersonGeneticTest10();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

                return;
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
          // ---------------------------------------------
          // Schedule has been changed. So create a new re-scheduled record.
          // ---------------------------------------------
          try
          {
            CreatePersonGeneticTest5();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0015_ERR_CRE_PERS_GEN_TEST";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        // ---------------------------------------------
        // Scheduled date and time are not changed. Check if Collect Sample 
        // Indicator has been changed.
        // ---------------------------------------------
        if (AsChar(entities.ScheduledMothers.CollectSampleInd) != AsChar
          (export.GeneticTestInformation.MotherCollectSampleInd) || AsChar
          (entities.ScheduledMothers.SampleUsableInd) != AsChar
          (export.GeneticTestInformation.MotherReuseSampleInd))
        {
          try
          {
            UpdatePersonGeneticTest9();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    // ---------------------------------------------
    // Process child's person_genetic_test info
    // ---------------------------------------------
    local.ChildPersGenTestFound.Flag = "N";

    if (ReadPersonGeneticTest1())
    {
      local.ChildPersGenTestFound.Flag = "Y";
      local.NextAvailablePersonGeneticTest.Identifier =
        entities.ScheduledChilds.Identifier + 1;
    }

    local.WorkTime.TimeWithAmPm =
      export.GeneticTestInformation.ChildSchedTestTime;

    if (IsEmpty(local.WorkTime.TimeWithAmPm))
    {
      local.WorkTime.Wtime = TimeSpan.Zero;
    }
    else
    {
      UseCabConvertTimeFormat();
    }

    if (AsChar(local.ChildPersGenTestFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // No PERSON_GENETIC_TEST record found. Create a new one.
      // ---------------------------------------------
      try
      {
        CreatePersonGeneticTest2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0015_ERR_CRE_PERS_GEN_TEST";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else if (!Equal(entities.ScheduledChilds.ScheduledTestDate,
      export.GeneticTestInformation.ChildSchedTestDate) || !
      Equal(entities.ScheduledChilds.ScheduledTestTime, local.WorkTime.Wtime))
    {
      if (!Lt(new DateTime(1, 1, 1), entities.ScheduledChilds.ScheduledTestDate))
        
      {
        // ---------------------------------------------
        // The date was not fixed before. It is being fixed only now.
        // ---------------------------------------------
        try
        {
          UpdatePersonGeneticTest2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

              return;
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
        // ---------------------------------------------
        // Schedule has been changed. So create a new re-scheduled record.
        // ---------------------------------------------
        try
        {
          CreatePersonGeneticTest1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0015_ERR_CRE_PERS_GEN_TEST";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // Scheduled date and time are not changed. Check if Collect Sample 
      // Indicator has been changed.
      // ---------------------------------------------
      if (AsChar(entities.ScheduledChilds.CollectSampleInd) != AsChar
        (export.GeneticTestInformation.ChildCollectSampleInd) || AsChar
        (entities.ScheduledChilds.SampleUsableInd) != AsChar
        (export.GeneticTestInformation.ChildReuseSampleInd))
      {
        try
        {
          UpdatePersonGeneticTest1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // ---------------------------------------------
    // Read and associate the vendors with genetic test and person genetic test 
    // for:
    // 	test site vendor
    // 	draw site vendor for father,
    // 	draw site vendor for mother and
    // 	draw site vendor for child.
    // ---------------------------------------------
    if (!IsEmpty(export.GeneticTestInformation.TestSiteVendorId))
    {
      if (Verify(export.GeneticTestInformation.TestSiteVendorId, " 0123456789") !=
        0)
      {
        ExitState = "OE0052_INVALID_TEST_SITE_ID";

        return;
      }

      local.Temp.Identifier =
        (int)StringToNumber(export.GeneticTestInformation.TestSiteVendorId);

      if (ReadVendor7())
      {
        export.GeneticTestInformation.TestSiteVendorName =
          entities.ExistingTestSite.Name;
        UseOeCabGetVendorAddress4();
        export.GeneticTestInformation.TestSiteCity =
          local.VendorAddress.City ?? Spaces(15);
        export.GeneticTestInformation.TestSiteState =
          local.VendorAddress.State ?? Spaces(2);
      }
      else
      {
        ExitState = "OE0052_INVALID_TEST_SITE_ID";

        return;
      }

      try
      {
        UpdateGeneticTest1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0016_ERR_UPD_GENETIC_TEST";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (export.GeneticTestInformation.FatherPrevSampGtestNumber != 0 && export
      .GeneticTestInformation.FatherPrevSampPerGenTestId != 0)
    {
      if (entities.ExistingPrevSampleFatherGeneticTest.TestNumber == entities
        .Scheduled.TestNumber && entities
        .ExistingPrevSampleFatherPersonGeneticTest.Identifier == entities
        .ScheduledFathers.Identifier)
      {
        ExitState = "OE0000_PREV_GTEST_SAMP_NEED_FTHR";

        return;
      }

      if (ReadPersonGeneticTest5())
      {
        if (ReadVendor5())
        {
          export.GeneticTestInformation.FatherDrawSiteId =
            NumberToString(entities.ExistingPrevDrawSite.Identifier, 8, 8);
        }

        export.GeneticTestInformation.FatherSchedTestDate =
          entities.ExistingPrevSampleFatherPersonGeneticTest.ScheduledTestDate;

        if (Equal(entities.ExistingPrevSampleFatherPersonGeneticTest.
          ScheduledTestTime, TimeSpan.Zero))
        {
          export.GeneticTestInformation.MotherSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            entities.ExistingPrevSampleFatherPersonGeneticTest.
              ScheduledTestTime.GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GeneticTestInformation.FatherSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }

        export.GeneticTestInformation.FatherPrevSampSpecimenId =
          entities.ExistingPrevSampleFatherPersonGeneticTest.SpecimenId ?? Spaces
          (10);

        try
        {
          UpdatePersonGeneticTest8();
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
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
    {
      if (export.GeneticTestInformation.MotherPrevSampGtestNumber != 0 && export
        .GeneticTestInformation.ChildPrevSampPerGenTestId != 0)
      {
        if (entities.ExistingPrevSampleMotherGeneticTest.TestNumber == entities
          .Scheduled.TestNumber && entities
          .ExistingPrevSampleMotherPersonGeneticTest.Identifier == entities
          .ScheduledMothers.Identifier)
        {
          ExitState = "OE0000_PREV_GTEST_SAMP_NEED_MTHR";

          return;
        }

        if (ReadPersonGeneticTest6())
        {
          if (ReadVendor6())
          {
            export.GeneticTestInformation.MotherDrawSiteId =
              NumberToString(entities.ExistingPrevDrawSite.Identifier, 8, 8);
          }

          export.GeneticTestInformation.MotherSchedTestDate =
            entities.ExistingPrevSampleMotherPersonGeneticTest.
              ScheduledTestDate;

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

          export.GeneticTestInformation.MotherPrevSampSpecimenId =
            entities.ExistingPrevSampleMotherPersonGeneticTest.SpecimenId ?? Spaces
            (10);

          try
          {
            UpdatePersonGeneticTest12();
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
        }
      }
    }

    if (export.GeneticTestInformation.ChildPrevSampGtestNumber != 0 && export
      .GeneticTestInformation.ChildPrevSampPerGenTestId != 0)
    {
      if (entities.ExistingPrevSampleChildGeneticTest.TestNumber == entities
        .Scheduled.TestNumber && entities
        .ExistingPrevSampleChildPersonGeneticTest.Identifier == entities
        .ScheduledChilds.Identifier)
      {
        ExitState = "OE0000_PREV_GTEST_SAMP_NEED_CHLD";

        return;
      }

      if (ReadPersonGeneticTest4())
      {
        if (ReadVendor4())
        {
          export.GeneticTestInformation.ChildDrawSiteId =
            NumberToString(entities.ExistingPrevDrawSite.Identifier, 8, 8);
        }

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

        export.GeneticTestInformation.ChildPrevSampSpecimenId =
          entities.ExistingPrevSampleChildPersonGeneticTest.SpecimenId ?? Spaces
          (10);

        try
        {
          UpdatePersonGeneticTest4();
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
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherDrawSiteId))
    {
      if (Verify(export.GeneticTestInformation.FatherDrawSiteId, " 0123456789") !=
        0)
      {
        ExitState = "OE0031_INVALID_DRAW_SITE_ID_FA";

        return;
      }

      local.Temp.Identifier =
        (int)StringToNumber(export.GeneticTestInformation.FatherDrawSiteId);

      if (ReadVendor2())
      {
        export.GeneticTestInformation.FatherDrawSiteVendorName =
          entities.ExistingFatherDrawSite.Name;
        UseOeCabGetVendorAddress1();
        export.GeneticTestInformation.FatherDrawSiteCity =
          local.VendorAddress.City ?? Spaces(15);
        export.GeneticTestInformation.FatherDrawSiteState =
          local.VendorAddress.State ?? Spaces(2);

        try
        {
          UpdatePersonGeneticTest7();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

              return;
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
        ExitState = "OE0031_INVALID_DRAW_SITE_ID_FA";

        return;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
    {
      if (!IsEmpty(export.GeneticTestInformation.MotherDrawSiteId))
      {
        if (Verify(export.GeneticTestInformation.MotherDrawSiteId, " 0123456789")
          != 0)
        {
          ExitState = "OE0032_INVALID_DRAW_SITE_ID_MO";

          return;
        }

        local.Temp.Identifier =
          (int)StringToNumber(export.GeneticTestInformation.MotherDrawSiteId);

        if (ReadVendor3())
        {
          export.GeneticTestInformation.MotherDrawSiteVendorName =
            entities.ExistingMotherDrawSite.Name;
          UseOeCabGetVendorAddress2();
          export.GeneticTestInformation.MotherDrawSiteCity =
            local.VendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.MotherDrawSiteState =
            local.VendorAddress.State ?? Spaces(2);

          try
          {
            UpdatePersonGeneticTest11();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

                return;
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
          ExitState = "OE0032_INVALID_DRAW_SITE_ID_MO";

          return;
        }
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildDrawSiteId))
    {
      if (Verify(export.GeneticTestInformation.ChildDrawSiteId, " 0123456789") !=
        0)
      {
        ExitState = "OE0030_INVALID_DRAW_SITE_ID_CH";

        return;
      }

      local.Temp.Identifier =
        (int)StringToNumber(export.GeneticTestInformation.ChildDrawSiteId);

      if (ReadVendor1())
      {
        export.GeneticTestInformation.ChildDrawSiteVendorName =
          entities.ExistingChildDrawSite.Name;
        UseOeCabGetVendorAddress3();
        export.GeneticTestInformation.ChildDrawSiteCity =
          local.VendorAddress.City ?? Spaces(15);
        export.GeneticTestInformation.ChildDrawSiteState =
          local.VendorAddress.State ?? Spaces(2);

        try
        {
          UpdatePersonGeneticTest3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0018_ERR_UPD_PERS_GEN_TEST";

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
        ExitState = "OE0030_INVALID_DRAW_SITE_ID_CH";
      }
    }
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

  private void UseOeCabGetVendorAddress1()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingFatherDrawSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private void UseOeCabGetVendorAddress2()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingMotherDrawSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private void UseOeCabGetVendorAddress3()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingChildDrawSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private void UseOeCabGetVendorAddress4()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingTestSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private void AssociateGeneticTest()
  {
    var lgaIdentifier = entities.ExistingNewPatEstab.Identifier;

    entities.Scheduled.Populated = false;
    Update("AssociateGeneticTest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LgaIdentifier = lgaIdentifier;
    entities.Scheduled.Populated = true;
  }

  private void CreateGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleMother.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);

    var testNumber = local.LastId.TestNumber + 1;
    var testType = export.GeneticTestInformation.TestType;
    var paternityProbability = 0M;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var gtaAccountNumber = entities.ExistingGeneticTestAccount.AccountNumber;
    var casNumber = entities.ExistingCaseRoleChild.CasNumber;
    var cspNumber = entities.ExistingCaseRoleChild.CspNumber;
    var croType = entities.ExistingCaseRoleChild.Type1;
    var croIdentifier = entities.ExistingCaseRoleChild.Identifier;
    var casMNumber = entities.ExistingCaseRoleMother.CasNumber;
    var cspMNumber = entities.ExistingCaseRoleMother.CspNumber;
    var croMType = entities.ExistingCaseRoleMother.Type1;
    var croMIdentifier = entities.ExistingCaseRoleMother.Identifier;
    var casANumber = entities.ExistingFatherAbsentParent.CasNumber;
    var cspANumber = entities.ExistingFatherAbsentParent.CspNumber;
    var croAType = entities.ExistingFatherAbsentParent.Type1;
    var croAIdentifier = entities.ExistingFatherAbsentParent.Identifier;

    CheckValid<GeneticTest>("CroType", croType);
    CheckValid<GeneticTest>("CroMType", croMType);
    CheckValid<GeneticTest>("CroAType", croAType);
    entities.Scheduled.Populated = false;
    Update("CreateGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", testNumber);
        db.SetNullableString(command, "labCaseNo", "");
        db.SetNullableString(command, "testType", testType);
        db.SetNullableDate(command, "actualTestDate", null);
        db.SetNullableDate(command, "resultRcvdDate", null);
        db.SetNullableString(command, "patExclInd", "");
        db.SetNullableDecimal(command, "patProbability", paternityProbability);
        db.SetNullableString(command, "contestRcvdInd", "");
        db.SetNullableDate(command, "startDateOfCont", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "gtaAccountNumber", gtaAccountNumber);
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croIdentifier", croIdentifier);
        db.SetNullableString(command, "casMNumber", casMNumber);
        db.SetNullableString(command, "cspMNumber", cspMNumber);
        db.SetNullableString(command, "croMType", croMType);
        db.SetNullableInt32(command, "croMIdentifier", croMIdentifier);
        db.SetNullableString(command, "casANumber", casANumber);
        db.SetNullableString(command, "cspANumber", cspANumber);
        db.SetNullableString(command, "croAType", croAType);
        db.SetNullableInt32(command, "croAIdentifier", croAIdentifier);
      });

    entities.Scheduled.TestNumber = testNumber;
    entities.Scheduled.LabCaseNo = "";
    entities.Scheduled.TestType = testType;
    entities.Scheduled.ActualTestDate = null;
    entities.Scheduled.TestResultReceivedDate = null;
    entities.Scheduled.PaternityExclusionInd = "";
    entities.Scheduled.PaternityProbability = paternityProbability;
    entities.Scheduled.NoticeOfContestReceivedInd = "";
    entities.Scheduled.CreatedBy = createdBy;
    entities.Scheduled.CreatedTimestamp = createdTimestamp;
    entities.Scheduled.LastUpdatedBy = createdBy;
    entities.Scheduled.LastUpdatedTimestamp = createdTimestamp;
    entities.Scheduled.GtaAccountNumber = gtaAccountNumber;
    entities.Scheduled.VenIdentifier = null;
    entities.Scheduled.CasNumber = casNumber;
    entities.Scheduled.CspNumber = cspNumber;
    entities.Scheduled.CroType = croType;
    entities.Scheduled.CroIdentifier = croIdentifier;
    entities.Scheduled.LgaIdentifier = null;
    entities.Scheduled.CasMNumber = casMNumber;
    entities.Scheduled.CspMNumber = cspMNumber;
    entities.Scheduled.CroMType = croMType;
    entities.Scheduled.CroMIdentifier = croMIdentifier;
    entities.Scheduled.CasANumber = casANumber;
    entities.Scheduled.CspANumber = cspANumber;
    entities.Scheduled.CroAType = croAType;
    entities.Scheduled.CroAIdentifier = croAIdentifier;
    entities.Scheduled.Populated = true;
  }

  private void CreateGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);

    var testNumber = local.LastId.TestNumber + 1;
    var testType = export.GeneticTestInformation.TestType;
    var paternityProbability = 0M;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var gtaAccountNumber = entities.ExistingGeneticTestAccount.AccountNumber;
    var casNumber = entities.ExistingCaseRoleChild.CasNumber;
    var cspNumber = entities.ExistingCaseRoleChild.CspNumber;
    var croType = entities.ExistingCaseRoleChild.Type1;
    var croIdentifier = entities.ExistingCaseRoleChild.Identifier;
    var casANumber = entities.ExistingFatherAbsentParent.CasNumber;
    var cspANumber = entities.ExistingFatherAbsentParent.CspNumber;
    var croAType = entities.ExistingFatherAbsentParent.Type1;
    var croAIdentifier = entities.ExistingFatherAbsentParent.Identifier;

    CheckValid<GeneticTest>("CroType", croType);
    CheckValid<GeneticTest>("CroAType", croAType);
    entities.Scheduled.Populated = false;
    Update("CreateGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", testNumber);
        db.SetNullableString(command, "labCaseNo", "");
        db.SetNullableString(command, "testType", testType);
        db.SetNullableDate(command, "actualTestDate", null);
        db.SetNullableDate(command, "resultRcvdDate", null);
        db.SetNullableString(command, "patExclInd", "");
        db.SetNullableDecimal(command, "patProbability", paternityProbability);
        db.SetNullableString(command, "contestRcvdInd", "");
        db.SetNullableDate(command, "startDateOfCont", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "gtaAccountNumber", gtaAccountNumber);
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croIdentifier", croIdentifier);
        db.SetNullableString(command, "casANumber", casANumber);
        db.SetNullableString(command, "cspANumber", cspANumber);
        db.SetNullableString(command, "croAType", croAType);
        db.SetNullableInt32(command, "croAIdentifier", croAIdentifier);
      });

    entities.Scheduled.TestNumber = testNumber;
    entities.Scheduled.LabCaseNo = "";
    entities.Scheduled.TestType = testType;
    entities.Scheduled.ActualTestDate = null;
    entities.Scheduled.TestResultReceivedDate = null;
    entities.Scheduled.PaternityExclusionInd = "";
    entities.Scheduled.PaternityProbability = paternityProbability;
    entities.Scheduled.NoticeOfContestReceivedInd = "";
    entities.Scheduled.CreatedBy = createdBy;
    entities.Scheduled.CreatedTimestamp = createdTimestamp;
    entities.Scheduled.LastUpdatedBy = createdBy;
    entities.Scheduled.LastUpdatedTimestamp = createdTimestamp;
    entities.Scheduled.GtaAccountNumber = gtaAccountNumber;
    entities.Scheduled.VenIdentifier = null;
    entities.Scheduled.CasNumber = casNumber;
    entities.Scheduled.CspNumber = cspNumber;
    entities.Scheduled.CroType = croType;
    entities.Scheduled.CroIdentifier = croIdentifier;
    entities.Scheduled.LgaIdentifier = null;
    entities.Scheduled.CasMNumber = null;
    entities.Scheduled.CspMNumber = null;
    entities.Scheduled.CroMType = null;
    entities.Scheduled.CroMIdentifier = null;
    entities.Scheduled.CasANumber = casANumber;
    entities.Scheduled.CspANumber = cspANumber;
    entities.Scheduled.CroAType = croAType;
    entities.Scheduled.CroAIdentifier = croAIdentifier;
    entities.Scheduled.Populated = true;
  }

  private void CreatePersonGeneticTest1()
  {
    var gteTestNumber = entities.Scheduled.TestNumber;
    var cspNumber = entities.ExistingChild.Number;
    var identifier = local.NextAvailablePersonGeneticTest.Identifier;
    var sampleUsableInd = export.GeneticTestInformation.ChildReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.ChildCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.ChildSchedTestDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ScheduledChilds.Populated = false;
    Update("CreatePersonGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", gteTestNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "specimenId", "");
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", "");
        db.SetNullableString(command, "sampleCollInd", "");
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ScheduledChilds.GteTestNumber = gteTestNumber;
    entities.ScheduledChilds.CspNumber = cspNumber;
    entities.ScheduledChilds.Identifier = identifier;
    entities.ScheduledChilds.SampleUsableInd = sampleUsableInd;
    entities.ScheduledChilds.CollectSampleInd = collectSampleInd;
    entities.ScheduledChilds.SampleCollectedInd = "";
    entities.ScheduledChilds.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledChilds.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledChilds.CreatedBy = createdBy;
    entities.ScheduledChilds.CreatedTimestamp = createdTimestamp;
    entities.ScheduledChilds.LastUpdatedBy = createdBy;
    entities.ScheduledChilds.LastUpdatedTimestamp = createdTimestamp;
    entities.ScheduledChilds.VenIdentifier = null;
    entities.ScheduledChilds.PgtIdentifier = null;
    entities.ScheduledChilds.CspRNumber = null;
    entities.ScheduledChilds.GteRTestNumber = null;
    entities.ScheduledChilds.Populated = true;
  }

  private void CreatePersonGeneticTest2()
  {
    var gteTestNumber = entities.Scheduled.TestNumber;
    var cspNumber = entities.ExistingChild.Number;
    var identifier = 1;
    var sampleUsableInd = export.GeneticTestInformation.ChildReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.ChildCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.ChildSchedTestDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ScheduledChilds.Populated = false;
    Update("CreatePersonGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", gteTestNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "specimenId", "");
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", "");
        db.SetNullableString(command, "sampleCollInd", "");
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ScheduledChilds.GteTestNumber = gteTestNumber;
    entities.ScheduledChilds.CspNumber = cspNumber;
    entities.ScheduledChilds.Identifier = identifier;
    entities.ScheduledChilds.SampleUsableInd = sampleUsableInd;
    entities.ScheduledChilds.CollectSampleInd = collectSampleInd;
    entities.ScheduledChilds.SampleCollectedInd = "";
    entities.ScheduledChilds.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledChilds.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledChilds.CreatedBy = createdBy;
    entities.ScheduledChilds.CreatedTimestamp = createdTimestamp;
    entities.ScheduledChilds.LastUpdatedBy = createdBy;
    entities.ScheduledChilds.LastUpdatedTimestamp = createdTimestamp;
    entities.ScheduledChilds.VenIdentifier = null;
    entities.ScheduledChilds.PgtIdentifier = null;
    entities.ScheduledChilds.CspRNumber = null;
    entities.ScheduledChilds.GteRTestNumber = null;
    entities.ScheduledChilds.Populated = true;
  }

  private void CreatePersonGeneticTest3()
  {
    var gteTestNumber = entities.Scheduled.TestNumber;
    var cspNumber = entities.ExistingFatherCsePerson.Number;
    var identifier = local.NextAvailablePersonGeneticTest.Identifier;
    var collectSampleInd = export.GeneticTestInformation.FatherCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.FatherSchedTestDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ScheduledFathers.Populated = false;
    Update("CreatePersonGeneticTest3",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", gteTestNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "specimenId", "");
        db.SetNullableString(command, "sampleUsableInd", "");
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", "");
        db.SetNullableString(command, "sampleCollInd", "");
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ScheduledFathers.GteTestNumber = gteTestNumber;
    entities.ScheduledFathers.CspNumber = cspNumber;
    entities.ScheduledFathers.Identifier = identifier;
    entities.ScheduledFathers.SampleUsableInd = "";
    entities.ScheduledFathers.CollectSampleInd = collectSampleInd;
    entities.ScheduledFathers.SampleCollectedInd = "";
    entities.ScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledFathers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledFathers.CreatedBy = createdBy;
    entities.ScheduledFathers.CreatedTimestamp = createdTimestamp;
    entities.ScheduledFathers.LastUpdatedBy = createdBy;
    entities.ScheduledFathers.LastUpdatedTimestamp = createdTimestamp;
    entities.ScheduledFathers.VenIdentifier = null;
    entities.ScheduledFathers.PgtIdentifier = null;
    entities.ScheduledFathers.CspRNumber = null;
    entities.ScheduledFathers.GteRTestNumber = null;
    entities.ScheduledFathers.Populated = true;
  }

  private void CreatePersonGeneticTest4()
  {
    var gteTestNumber = entities.Scheduled.TestNumber;
    var cspNumber = entities.ExistingFatherCsePerson.Number;
    var identifier = 1;
    var collectSampleInd = export.GeneticTestInformation.FatherCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.FatherSchedTestDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ScheduledFathers.Populated = false;
    Update("CreatePersonGeneticTest4",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", gteTestNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "specimenId", "");
        db.SetNullableString(command, "sampleUsableInd", "");
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", "");
        db.SetNullableString(command, "sampleCollInd", "");
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ScheduledFathers.GteTestNumber = gteTestNumber;
    entities.ScheduledFathers.CspNumber = cspNumber;
    entities.ScheduledFathers.Identifier = identifier;
    entities.ScheduledFathers.SampleUsableInd = "";
    entities.ScheduledFathers.CollectSampleInd = collectSampleInd;
    entities.ScheduledFathers.SampleCollectedInd = "";
    entities.ScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledFathers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledFathers.CreatedBy = createdBy;
    entities.ScheduledFathers.CreatedTimestamp = createdTimestamp;
    entities.ScheduledFathers.LastUpdatedBy = createdBy;
    entities.ScheduledFathers.LastUpdatedTimestamp = createdTimestamp;
    entities.ScheduledFathers.VenIdentifier = null;
    entities.ScheduledFathers.PgtIdentifier = null;
    entities.ScheduledFathers.CspRNumber = null;
    entities.ScheduledFathers.GteRTestNumber = null;
    entities.ScheduledFathers.Populated = true;
  }

  private void CreatePersonGeneticTest5()
  {
    var gteTestNumber = entities.Scheduled.TestNumber;
    var cspNumber = entities.ExistingMother.Number;
    var identifier = local.NextAvailablePersonGeneticTest.Identifier;
    var sampleUsableInd = export.GeneticTestInformation.MotherReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.MotherCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.MotherSchedTestDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ScheduledMothers.Populated = false;
    Update("CreatePersonGeneticTest5",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", gteTestNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "specimenId", "");
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", "");
        db.SetNullableString(command, "sampleCollInd", "");
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ScheduledMothers.GteTestNumber = gteTestNumber;
    entities.ScheduledMothers.CspNumber = cspNumber;
    entities.ScheduledMothers.Identifier = identifier;
    entities.ScheduledMothers.SampleUsableInd = sampleUsableInd;
    entities.ScheduledMothers.CollectSampleInd = collectSampleInd;
    entities.ScheduledMothers.SampleCollectedInd = "";
    entities.ScheduledMothers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledMothers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledMothers.CreatedBy = createdBy;
    entities.ScheduledMothers.CreatedTimestamp = createdTimestamp;
    entities.ScheduledMothers.LastUpdatedBy = createdBy;
    entities.ScheduledMothers.LastUpdatedTimestamp = createdTimestamp;
    entities.ScheduledMothers.VenIdentifier = null;
    entities.ScheduledMothers.PgtIdentifier = null;
    entities.ScheduledMothers.CspRNumber = null;
    entities.ScheduledMothers.GteRTestNumber = null;
    entities.ScheduledMothers.Populated = true;
  }

  private void CreatePersonGeneticTest6()
  {
    var gteTestNumber = entities.Scheduled.TestNumber;
    var cspNumber = entities.ExistingMother.Number;
    var identifier = 1;
    var sampleUsableInd = export.GeneticTestInformation.MotherReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.MotherCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.MotherSchedTestDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ScheduledMothers.Populated = false;
    Update("CreatePersonGeneticTest6",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", gteTestNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "specimenId", "");
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", "");
        db.SetNullableString(command, "sampleCollInd", "");
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.ScheduledMothers.GteTestNumber = gteTestNumber;
    entities.ScheduledMothers.CspNumber = cspNumber;
    entities.ScheduledMothers.Identifier = identifier;
    entities.ScheduledMothers.SampleUsableInd = sampleUsableInd;
    entities.ScheduledMothers.CollectSampleInd = collectSampleInd;
    entities.ScheduledMothers.SampleCollectedInd = "";
    entities.ScheduledMothers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledMothers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledMothers.CreatedBy = createdBy;
    entities.ScheduledMothers.CreatedTimestamp = createdTimestamp;
    entities.ScheduledMothers.LastUpdatedBy = createdBy;
    entities.ScheduledMothers.LastUpdatedTimestamp = createdTimestamp;
    entities.ScheduledMothers.VenIdentifier = null;
    entities.ScheduledMothers.PgtIdentifier = null;
    entities.ScheduledMothers.CspRNumber = null;
    entities.ScheduledMothers.GteRTestNumber = null;
    entities.ScheduledMothers.Populated = true;
  }

  private void DisassociateGeneticTest()
  {
    entities.Scheduled.Populated = false;
    Update("DisassociateGeneticTest",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LgaIdentifier = null;
    entities.Scheduled.Populated = true;
  }

  private bool ReadAbsentParent()
  {
    entities.ExistingFatherAbsentParent.Populated = false;

    return Read("ReadAbsentParent",
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
        entities.ExistingFatherAbsentParent.CasNumber = db.GetString(reader, 0);
        entities.ExistingFatherAbsentParent.CspNumber = db.GetString(reader, 1);
        entities.ExistingFatherAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ExistingFatherAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFatherAbsentParent.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingFatherAbsentParent.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingFatherAbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingFatherAbsentParent.Type1);
          
      });
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.GeneticTestInformation.CaseNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.ExistingCaseRoleChild.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", entities.ExistingChild.Number);
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
        entities.ExistingCaseRoleChild.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCaseRoleChild.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingCaseRoleChild.ResidesWithArIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingCaseRoleChild.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleChild.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.ExistingCaseRoleChild.ResidesWithArIndicator);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingChild.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.GeneticTestInformation.ChildPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingChild.Number = db.GetString(reader, 0);
        entities.ExistingChild.Populated = true;
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
    entities.ExistingMother.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.GeneticTestInformation.MotherPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingMother.Number = db.GetString(reader, 0);
        entities.ExistingMother.Populated = true;
      });
  }

  private bool ReadGeneticTest1()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadGeneticTest1",
      null,
      (db, reader) =>
      {
        entities.ExistingLast.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingLast.Populated = true;
      });
  }

  private bool ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleMother.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croAIdentifier",
          entities.ExistingFatherAbsentParent.Identifier);
        db.SetNullableString(
          command, "croAType", entities.ExistingFatherAbsentParent.Type1);
        db.SetNullableString(
          command, "casANumber", entities.ExistingFatherAbsentParent.CasNumber);
          
        db.SetNullableString(
          command, "cspANumber", entities.ExistingFatherAbsentParent.CspNumber);
          
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
        db.SetNullableDate(
          command, "resultRcvdDate",
          local.Temp00010101.TestResultReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Scheduled.TestNumber = db.GetInt32(reader, 0);
        entities.Scheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.Scheduled.TestType = db.GetNullableString(reader, 2);
        entities.Scheduled.ActualTestDate = db.GetNullableDate(reader, 3);
        entities.Scheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.Scheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.Scheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.Scheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.Scheduled.CreatedBy = db.GetString(reader, 8);
        entities.Scheduled.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.Scheduled.LastUpdatedBy = db.GetString(reader, 10);
        entities.Scheduled.LastUpdatedTimestamp = db.GetDateTime(reader, 11);
        entities.Scheduled.GtaAccountNumber = db.GetNullableString(reader, 12);
        entities.Scheduled.VenIdentifier = db.GetNullableInt32(reader, 13);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 15);
        entities.Scheduled.CroType = db.GetNullableString(reader, 16);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 17);
        entities.Scheduled.LgaIdentifier = db.GetNullableInt32(reader, 18);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 19);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 20);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 21);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 22);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 23);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 24);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 25);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 26);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTest3()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croAIdentifier",
          entities.ExistingFatherAbsentParent.Identifier);
        db.SetNullableString(
          command, "croAType", entities.ExistingFatherAbsentParent.Type1);
        db.SetNullableString(
          command, "casANumber", entities.ExistingFatherAbsentParent.CasNumber);
          
        db.SetNullableString(
          command, "cspANumber", entities.ExistingFatherAbsentParent.CspNumber);
          
        db.SetNullableInt32(
          command, "croIdentifier", entities.ExistingCaseRoleChild.Identifier);
        db.SetNullableString(
          command, "croType", entities.ExistingCaseRoleChild.Type1);
        db.SetNullableString(
          command, "casNumber", entities.ExistingCaseRoleChild.CasNumber);
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCaseRoleChild.CspNumber);
        db.SetNullableDate(
          command, "resultRcvdDate",
          local.Temp00010101.TestResultReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Scheduled.TestNumber = db.GetInt32(reader, 0);
        entities.Scheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.Scheduled.TestType = db.GetNullableString(reader, 2);
        entities.Scheduled.ActualTestDate = db.GetNullableDate(reader, 3);
        entities.Scheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.Scheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.Scheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.Scheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.Scheduled.CreatedBy = db.GetString(reader, 8);
        entities.Scheduled.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.Scheduled.LastUpdatedBy = db.GetString(reader, 10);
        entities.Scheduled.LastUpdatedTimestamp = db.GetDateTime(reader, 11);
        entities.Scheduled.GtaAccountNumber = db.GetNullableString(reader, 12);
        entities.Scheduled.VenIdentifier = db.GetNullableInt32(reader, 13);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 15);
        entities.Scheduled.CroType = db.GetNullableString(reader, 16);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 17);
        entities.Scheduled.LgaIdentifier = db.GetNullableInt32(reader, 18);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 19);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 20);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 21);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 22);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 23);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 24);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 25);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 26);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTest4()
  {
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest4",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", local.Latest.TestNumber);
      },
      (db, reader) =>
      {
        entities.Scheduled.TestNumber = db.GetInt32(reader, 0);
        entities.Scheduled.LabCaseNo = db.GetNullableString(reader, 1);
        entities.Scheduled.TestType = db.GetNullableString(reader, 2);
        entities.Scheduled.ActualTestDate = db.GetNullableDate(reader, 3);
        entities.Scheduled.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.Scheduled.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.Scheduled.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.Scheduled.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.Scheduled.CreatedBy = db.GetString(reader, 8);
        entities.Scheduled.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.Scheduled.LastUpdatedBy = db.GetString(reader, 10);
        entities.Scheduled.LastUpdatedTimestamp = db.GetDateTime(reader, 11);
        entities.Scheduled.GtaAccountNumber = db.GetNullableString(reader, 12);
        entities.Scheduled.VenIdentifier = db.GetNullableInt32(reader, 13);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 15);
        entities.Scheduled.CroType = db.GetNullableString(reader, 16);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 17);
        entities.Scheduled.LgaIdentifier = db.GetNullableInt32(reader, 18);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 19);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 20);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 21);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 22);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 23);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 24);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 25);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 26);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTestAccount()
  {
    entities.ExistingGeneticTestAccount.Populated = false;

    return Read("ReadGeneticTestAccount",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber",
          import.GeneticTestInformation.GeneticTestAccountNo);
      },
      (db, reader) =>
      {
        entities.ExistingGeneticTestAccount.AccountNumber =
          db.GetString(reader, 0);
        entities.ExistingGeneticTestAccount.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingNewPatEstab.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingNewPatEstab.Identifier = db.GetInt32(reader, 0);
        entities.ExistingNewPatEstab.Classification = db.GetString(reader, 1);
        entities.ExistingNewPatEstab.Type1 = db.GetString(reader, 2);
        entities.ExistingNewPatEstab.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingNewPatEstab.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingNewPatEstab.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Scheduled.Populated);
    entities.ExistingPrevPatEstab.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Scheduled.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevPatEstab.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrevPatEstab.Classification = db.GetString(reader, 1);
        entities.ExistingPrevPatEstab.Type1 = db.GetString(reader, 2);
        entities.ExistingPrevPatEstab.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingPrevPatEstab.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPrevPatEstab.Populated = true;
      });
  }

  private bool ReadMother()
  {
    entities.ExistingCaseRoleMother.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", entities.ExistingMother.Number);
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
    entities.ScheduledChilds.Populated = false;

    return Read("ReadPersonGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.Scheduled.TestNumber);
        db.SetString(command, "cspNumber", entities.ExistingChild.Number);
      },
      (db, reader) =>
      {
        entities.ScheduledChilds.GteTestNumber = db.GetInt32(reader, 0);
        entities.ScheduledChilds.CspNumber = db.GetString(reader, 1);
        entities.ScheduledChilds.Identifier = db.GetInt32(reader, 2);
        entities.ScheduledChilds.SampleUsableInd =
          db.GetNullableString(reader, 3);
        entities.ScheduledChilds.CollectSampleInd =
          db.GetNullableString(reader, 4);
        entities.ScheduledChilds.SampleCollectedInd =
          db.GetNullableString(reader, 5);
        entities.ScheduledChilds.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 6);
        entities.ScheduledChilds.ScheduledTestDate =
          db.GetNullableDate(reader, 7);
        entities.ScheduledChilds.CreatedBy = db.GetString(reader, 8);
        entities.ScheduledChilds.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.ScheduledChilds.LastUpdatedBy = db.GetString(reader, 10);
        entities.ScheduledChilds.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ScheduledChilds.VenIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ScheduledChilds.PgtIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ScheduledChilds.CspRNumber = db.GetNullableString(reader, 14);
        entities.ScheduledChilds.GteRTestNumber =
          db.GetNullableInt32(reader, 15);
        entities.ScheduledChilds.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest2()
  {
    entities.ScheduledFathers.Populated = false;

    return Read("ReadPersonGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.Scheduled.TestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ScheduledFathers.GteTestNumber = db.GetInt32(reader, 0);
        entities.ScheduledFathers.CspNumber = db.GetString(reader, 1);
        entities.ScheduledFathers.Identifier = db.GetInt32(reader, 2);
        entities.ScheduledFathers.SampleUsableInd =
          db.GetNullableString(reader, 3);
        entities.ScheduledFathers.CollectSampleInd =
          db.GetNullableString(reader, 4);
        entities.ScheduledFathers.SampleCollectedInd =
          db.GetNullableString(reader, 5);
        entities.ScheduledFathers.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 6);
        entities.ScheduledFathers.ScheduledTestDate =
          db.GetNullableDate(reader, 7);
        entities.ScheduledFathers.CreatedBy = db.GetString(reader, 8);
        entities.ScheduledFathers.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.ScheduledFathers.LastUpdatedBy = db.GetString(reader, 10);
        entities.ScheduledFathers.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ScheduledFathers.VenIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ScheduledFathers.PgtIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ScheduledFathers.CspRNumber = db.GetNullableString(reader, 14);
        entities.ScheduledFathers.GteRTestNumber =
          db.GetNullableInt32(reader, 15);
        entities.ScheduledFathers.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest3()
  {
    entities.ScheduledMothers.Populated = false;

    return Read("ReadPersonGeneticTest3",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber", entities.Scheduled.TestNumber);
        db.SetString(command, "cspNumber", entities.ExistingMother.Number);
      },
      (db, reader) =>
      {
        entities.ScheduledMothers.GteTestNumber = db.GetInt32(reader, 0);
        entities.ScheduledMothers.CspNumber = db.GetString(reader, 1);
        entities.ScheduledMothers.Identifier = db.GetInt32(reader, 2);
        entities.ScheduledMothers.SampleUsableInd =
          db.GetNullableString(reader, 3);
        entities.ScheduledMothers.CollectSampleInd =
          db.GetNullableString(reader, 4);
        entities.ScheduledMothers.SampleCollectedInd =
          db.GetNullableString(reader, 5);
        entities.ScheduledMothers.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 6);
        entities.ScheduledMothers.ScheduledTestDate =
          db.GetNullableDate(reader, 7);
        entities.ScheduledMothers.CreatedBy = db.GetString(reader, 8);
        entities.ScheduledMothers.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.ScheduledMothers.LastUpdatedBy = db.GetString(reader, 10);
        entities.ScheduledMothers.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ScheduledMothers.VenIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ScheduledMothers.PgtIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ScheduledMothers.CspRNumber = db.GetNullableString(reader, 14);
        entities.ScheduledMothers.GteRTestNumber =
          db.GetNullableInt32(reader, 15);
        entities.ScheduledMothers.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest4()
  {
    entities.ExistingPrevSampleChildPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingChild.Number);
        db.SetInt32(
          command, "gteTestNumber",
          export.GeneticTestInformation.ChildPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.ChildPrevSampPerGenTestId);
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

  private bool ReadPersonGeneticTest5()
  {
    entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest5",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber",
          export.GeneticTestInformation.FatherPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.FatherPrevSampPerGenTestId);
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

  private bool ReadPersonGeneticTest6()
  {
    entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingMother.Number);
        db.SetInt32(
          command, "gteTestNumber",
          export.GeneticTestInformation.MotherPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.MotherPrevSampPerGenTestId);
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
    entities.ExistingChildDrawSite.Populated = false;

    return Read("ReadVendor1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingChildDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingChildDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingChildDrawSite.Populated = true;
      });
  }

  private bool ReadVendor2()
  {
    entities.ExistingFatherDrawSite.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFatherDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFatherDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingFatherDrawSite.Populated = true;
      });
  }

  private bool ReadVendor3()
  {
    entities.ExistingMotherDrawSite.Populated = false;

    return Read("ReadVendor3",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingMotherDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingMotherDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingMotherDrawSite.Populated = true;
      });
  }

  private bool ReadVendor4()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleChildPersonGeneticTest.Populated);
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadVendor4",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPrevSampleChildPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingPrevDrawSite.Populated = true;
      });
  }

  private bool ReadVendor5()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleFatherPersonGeneticTest.Populated);
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadVendor5",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPrevSampleFatherPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingPrevDrawSite.Populated = true;
      });
  }

  private bool ReadVendor6()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleMotherPersonGeneticTest.Populated);
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadVendor6",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPrevSampleMotherPersonGeneticTest.VenIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingPrevDrawSite.Populated = true;
      });
  }

  private bool ReadVendor7()
  {
    entities.ExistingTestSite.Populated = false;

    return Read("ReadVendor7",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTestSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingTestSite.Name = db.GetString(reader, 1);
        entities.ExistingTestSite.Populated = true;
      });
  }

  private void UpdateGeneticTest1()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var venIdentifier = entities.ExistingTestSite.Identifier;

    entities.Scheduled.Populated = false;
    Update("UpdateGeneticTest1",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LastUpdatedBy = lastUpdatedBy;
    entities.Scheduled.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Scheduled.VenIdentifier = venIdentifier;
    entities.Scheduled.Populated = true;
  }

  private void UpdateGeneticTest2()
  {
    var testType = export.GeneticTestInformation.TestType;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var gtaAccountNumber = entities.ExistingGeneticTestAccount.AccountNumber;

    entities.Scheduled.Populated = false;
    Update("UpdateGeneticTest2",
      (db, command) =>
      {
        db.SetNullableString(command, "testType", testType);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "gtaAccountNumber", gtaAccountNumber);
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.TestType = testType;
    entities.Scheduled.LastUpdatedBy = lastUpdatedBy;
    entities.Scheduled.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Scheduled.GtaAccountNumber = gtaAccountNumber;
    entities.Scheduled.Populated = true;
  }

  private void UpdatePersonGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var sampleUsableInd = export.GeneticTestInformation.ChildReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.ChildCollectSampleInd;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledChilds.Populated = false;
    Update("UpdatePersonGeneticTest1",
      (db, command) =>
      {
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledChilds.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
      });

    entities.ScheduledChilds.SampleUsableInd = sampleUsableInd;
    entities.ScheduledChilds.CollectSampleInd = collectSampleInd;
    entities.ScheduledChilds.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledChilds.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledChilds.Populated = true;
  }

  private void UpdatePersonGeneticTest10()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var sampleUsableInd = export.GeneticTestInformation.MotherReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.MotherCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.MotherSchedTestDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledMothers.Populated = false;
    Update("UpdatePersonGeneticTest10",
      (db, command) =>
      {
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledMothers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    entities.ScheduledMothers.SampleUsableInd = sampleUsableInd;
    entities.ScheduledMothers.CollectSampleInd = collectSampleInd;
    entities.ScheduledMothers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledMothers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledMothers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledMothers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledMothers.Populated = true;
  }

  private void UpdatePersonGeneticTest11()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var venIdentifier = entities.ExistingMotherDrawSite.Identifier;

    entities.ScheduledMothers.Populated = false;
    Update("UpdatePersonGeneticTest11",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledMothers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    entities.ScheduledMothers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledMothers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledMothers.VenIdentifier = venIdentifier;
    entities.ScheduledMothers.Populated = true;
  }

  private void UpdatePersonGeneticTest12()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleMotherPersonGeneticTest.Populated);
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var scheduledTestTime = TimeSpan.Zero;
    var pgtIdentifier =
      entities.ExistingPrevSampleMotherPersonGeneticTest.Identifier;
    var cspRNumber =
      entities.ExistingPrevSampleMotherPersonGeneticTest.CspNumber;
    var gteRTestNumber =
      entities.ExistingPrevSampleMotherPersonGeneticTest.GteTestNumber;

    entities.ScheduledMothers.Populated = false;
    Update("UpdatePersonGeneticTest12",
      (db, command) =>
      {
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", null);
        db.SetNullableInt32(command, "pgtIdentifier", pgtIdentifier);
        db.SetNullableString(command, "cspRNumber", cspRNumber);
        db.SetNullableInt32(command, "gteRTestNumber", gteRTestNumber);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledMothers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    entities.ScheduledMothers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledMothers.ScheduledTestDate = null;
    entities.ScheduledMothers.PgtIdentifier = pgtIdentifier;
    entities.ScheduledMothers.CspRNumber = cspRNumber;
    entities.ScheduledMothers.GteRTestNumber = gteRTestNumber;
    entities.ScheduledMothers.Populated = true;
  }

  private void UpdatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var sampleUsableInd = export.GeneticTestInformation.ChildReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.ChildCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.ChildSchedTestDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledChilds.Populated = false;
    Update("UpdatePersonGeneticTest2",
      (db, command) =>
      {
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledChilds.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
      });

    entities.ScheduledChilds.SampleUsableInd = sampleUsableInd;
    entities.ScheduledChilds.CollectSampleInd = collectSampleInd;
    entities.ScheduledChilds.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledChilds.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledChilds.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledChilds.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledChilds.Populated = true;
  }

  private void UpdatePersonGeneticTest3()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var venIdentifier = entities.ExistingChildDrawSite.Identifier;

    entities.ScheduledChilds.Populated = false;
    Update("UpdatePersonGeneticTest3",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledChilds.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
      });

    entities.ScheduledChilds.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledChilds.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledChilds.VenIdentifier = venIdentifier;
    entities.ScheduledChilds.Populated = true;
  }

  private void UpdatePersonGeneticTest4()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleChildPersonGeneticTest.Populated);
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var scheduledTestTime = TimeSpan.Zero;
    var pgtIdentifier =
      entities.ExistingPrevSampleChildPersonGeneticTest.Identifier;
    var cspRNumber =
      entities.ExistingPrevSampleChildPersonGeneticTest.CspNumber;
    var gteRTestNumber =
      entities.ExistingPrevSampleChildPersonGeneticTest.GteTestNumber;

    entities.ScheduledChilds.Populated = false;
    Update("UpdatePersonGeneticTest4",
      (db, command) =>
      {
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", null);
        db.SetNullableInt32(command, "pgtIdentifier", pgtIdentifier);
        db.SetNullableString(command, "cspRNumber", cspRNumber);
        db.SetNullableInt32(command, "gteRTestNumber", gteRTestNumber);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledChilds.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
      });

    entities.ScheduledChilds.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledChilds.ScheduledTestDate = null;
    entities.ScheduledChilds.PgtIdentifier = pgtIdentifier;
    entities.ScheduledChilds.CspRNumber = cspRNumber;
    entities.ScheduledChilds.GteRTestNumber = gteRTestNumber;
    entities.ScheduledChilds.Populated = true;
  }

  private void UpdatePersonGeneticTest5()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledFathers.Populated);

    var collectSampleInd = export.GeneticTestInformation.FatherCollectSampleInd;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledFathers.Populated = false;
    Update("UpdatePersonGeneticTest5",
      (db, command) =>
      {
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledFathers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledFathers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledFathers.Identifier);
          
      });

    entities.ScheduledFathers.CollectSampleInd = collectSampleInd;
    entities.ScheduledFathers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledFathers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledFathers.Populated = true;
  }

  private void UpdatePersonGeneticTest6()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledFathers.Populated);

    var collectSampleInd = export.GeneticTestInformation.FatherCollectSampleInd;
    var scheduledTestTime = local.WorkTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.FatherSchedTestDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledFathers.Populated = false;
    Update("UpdatePersonGeneticTest6",
      (db, command) =>
      {
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", scheduledTestDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledFathers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledFathers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledFathers.Identifier);
          
      });

    entities.ScheduledFathers.CollectSampleInd = collectSampleInd;
    entities.ScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledFathers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledFathers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledFathers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledFathers.Populated = true;
  }

  private void UpdatePersonGeneticTest7()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledFathers.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var venIdentifier = entities.ExistingFatherDrawSite.Identifier;

    entities.ScheduledFathers.Populated = false;
    Update("UpdatePersonGeneticTest7",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledFathers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledFathers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledFathers.Identifier);
          
      });

    entities.ScheduledFathers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledFathers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledFathers.VenIdentifier = venIdentifier;
    entities.ScheduledFathers.Populated = true;
  }

  private void UpdatePersonGeneticTest8()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPrevSampleFatherPersonGeneticTest.Populated);
    System.Diagnostics.Debug.Assert(entities.ScheduledFathers.Populated);

    var scheduledTestTime = TimeSpan.Zero;
    var pgtIdentifier =
      entities.ExistingPrevSampleFatherPersonGeneticTest.Identifier;
    var cspRNumber =
      entities.ExistingPrevSampleFatherPersonGeneticTest.CspNumber;
    var gteRTestNumber =
      entities.ExistingPrevSampleFatherPersonGeneticTest.GteTestNumber;

    entities.ScheduledFathers.Populated = false;
    Update("UpdatePersonGeneticTest8",
      (db, command) =>
      {
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", null);
        db.SetNullableInt32(command, "pgtIdentifier", pgtIdentifier);
        db.SetNullableString(command, "cspRNumber", cspRNumber);
        db.SetNullableInt32(command, "gteRTestNumber", gteRTestNumber);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledFathers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledFathers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledFathers.Identifier);
          
      });

    entities.ScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledFathers.ScheduledTestDate = null;
    entities.ScheduledFathers.PgtIdentifier = pgtIdentifier;
    entities.ScheduledFathers.CspRNumber = cspRNumber;
    entities.ScheduledFathers.GteRTestNumber = gteRTestNumber;
    entities.ScheduledFathers.Populated = true;
  }

  private void UpdatePersonGeneticTest9()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var sampleUsableInd = export.GeneticTestInformation.MotherReuseSampleInd;
    var collectSampleInd = export.GeneticTestInformation.MotherCollectSampleInd;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledMothers.Populated = false;
    Update("UpdatePersonGeneticTest9",
      (db, command) =>
      {
        db.SetNullableString(command, "sampleUsableInd", sampleUsableInd);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledMothers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    entities.ScheduledMothers.SampleUsableInd = sampleUsableInd;
    entities.ScheduledMothers.CollectSampleInd = collectSampleInd;
    entities.ScheduledMothers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledMothers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledMothers.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private LegalAction legalAction;
    private GeneticTestInformation geneticTestInformation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
    }

    private GeneticTestInformation geneticTestInformation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
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
    /// A value of LastId.
    /// </summary>
    [JsonPropertyName("lastId")]
    public GeneticTest LastId
    {
      get => lastId ??= new();
      set => lastId = value;
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
    /// A value of Temp00010101.
    /// </summary>
    [JsonPropertyName("temp00010101")]
    public GeneticTest Temp00010101
    {
      get => temp00010101 ??= new();
      set => temp00010101 = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Vendor Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of FatherPersGenTestFound.
    /// </summary>
    [JsonPropertyName("fatherPersGenTestFound")]
    public Common FatherPersGenTestFound
    {
      get => fatherPersGenTestFound ??= new();
      set => fatherPersGenTestFound = value;
    }

    /// <summary>
    /// A value of MotherPersGenTestFound.
    /// </summary>
    [JsonPropertyName("motherPersGenTestFound")]
    public Common MotherPersGenTestFound
    {
      get => motherPersGenTestFound ??= new();
      set => motherPersGenTestFound = value;
    }

    /// <summary>
    /// A value of ChildPersGenTestFound.
    /// </summary>
    [JsonPropertyName("childPersGenTestFound")]
    public Common ChildPersGenTestFound
    {
      get => childPersGenTestFound ??= new();
      set => childPersGenTestFound = value;
    }

    /// <summary>
    /// A value of NextAvailableGeneticTest.
    /// </summary>
    [JsonPropertyName("nextAvailableGeneticTest")]
    public GeneticTest NextAvailableGeneticTest
    {
      get => nextAvailableGeneticTest ??= new();
      set => nextAvailableGeneticTest = value;
    }

    /// <summary>
    /// A value of NextAvailablePersonGeneticTest.
    /// </summary>
    [JsonPropertyName("nextAvailablePersonGeneticTest")]
    public PersonGeneticTest NextAvailablePersonGeneticTest
    {
      get => nextAvailablePersonGeneticTest ??= new();
      set => nextAvailablePersonGeneticTest = value;
    }

    private DateWorkArea current;
    private VendorAddress vendorAddress;
    private Common errorInTimeConversion;
    private WorkTime workTime;
    private GeneticTest lastId;
    private GeneticTest latest;
    private GeneticTest temp00010101;
    private Common latestGeneticTestFound;
    private Vendor temp;
    private Common fatherPersGenTestFound;
    private Common motherPersGenTestFound;
    private Common childPersGenTestFound;
    private GeneticTest nextAvailableGeneticTest;
    private PersonGeneticTest nextAvailablePersonGeneticTest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPrevPatEstab.
    /// </summary>
    [JsonPropertyName("existingPrevPatEstab")]
    public LegalAction ExistingPrevPatEstab
    {
      get => existingPrevPatEstab ??= new();
      set => existingPrevPatEstab = value;
    }

    /// <summary>
    /// A value of ExistingNewPatEstab.
    /// </summary>
    [JsonPropertyName("existingNewPatEstab")]
    public LegalAction ExistingNewPatEstab
    {
      get => existingNewPatEstab ??= new();
      set => existingNewPatEstab = value;
    }

    /// <summary>
    /// A value of ExistingPrevDrawSite.
    /// </summary>
    [JsonPropertyName("existingPrevDrawSite")]
    public Vendor ExistingPrevDrawSite
    {
      get => existingPrevDrawSite ??= new();
      set => existingPrevDrawSite = value;
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
    /// A value of ExistingPrevSampleChildPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleChildPersonGeneticTest")]
    public PersonGeneticTest ExistingPrevSampleChildPersonGeneticTest
    {
      get => existingPrevSampleChildPersonGeneticTest ??= new();
      set => existingPrevSampleChildPersonGeneticTest = value;
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
    /// A value of ExistingPrevSampleMotherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleMotherPersonGeneticTest")]
    public PersonGeneticTest ExistingPrevSampleMotherPersonGeneticTest
    {
      get => existingPrevSampleMotherPersonGeneticTest ??= new();
      set => existingPrevSampleMotherPersonGeneticTest = value;
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
    /// A value of ExistingPrevSampleFatherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPrevSampleFatherPersonGeneticTest")]
    public PersonGeneticTest ExistingPrevSampleFatherPersonGeneticTest
    {
      get => existingPrevSampleFatherPersonGeneticTest ??= new();
      set => existingPrevSampleFatherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingFatherAbsentParent.
    /// </summary>
    [JsonPropertyName("existingFatherAbsentParent")]
    public CaseRole ExistingFatherAbsentParent
    {
      get => existingFatherAbsentParent ??= new();
      set => existingFatherAbsentParent = value;
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
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public GeneticTest ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of ExistingFatherDrawSite.
    /// </summary>
    [JsonPropertyName("existingFatherDrawSite")]
    public Vendor ExistingFatherDrawSite
    {
      get => existingFatherDrawSite ??= new();
      set => existingFatherDrawSite = value;
    }

    /// <summary>
    /// A value of ExistingMotherDrawSite.
    /// </summary>
    [JsonPropertyName("existingMotherDrawSite")]
    public Vendor ExistingMotherDrawSite
    {
      get => existingMotherDrawSite ??= new();
      set => existingMotherDrawSite = value;
    }

    /// <summary>
    /// A value of ExistingChildDrawSite.
    /// </summary>
    [JsonPropertyName("existingChildDrawSite")]
    public Vendor ExistingChildDrawSite
    {
      get => existingChildDrawSite ??= new();
      set => existingChildDrawSite = value;
    }

    /// <summary>
    /// A value of ExistingTestSite.
    /// </summary>
    [JsonPropertyName("existingTestSite")]
    public Vendor ExistingTestSite
    {
      get => existingTestSite ??= new();
      set => existingTestSite = value;
    }

    /// <summary>
    /// A value of ScheduledFathers.
    /// </summary>
    [JsonPropertyName("scheduledFathers")]
    public PersonGeneticTest ScheduledFathers
    {
      get => scheduledFathers ??= new();
      set => scheduledFathers = value;
    }

    /// <summary>
    /// A value of ScheduledMothers.
    /// </summary>
    [JsonPropertyName("scheduledMothers")]
    public PersonGeneticTest ScheduledMothers
    {
      get => scheduledMothers ??= new();
      set => scheduledMothers = value;
    }

    /// <summary>
    /// A value of ScheduledChilds.
    /// </summary>
    [JsonPropertyName("scheduledChilds")]
    public PersonGeneticTest ScheduledChilds
    {
      get => scheduledChilds ??= new();
      set => scheduledChilds = value;
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
    /// A value of ExistingMother.
    /// </summary>
    [JsonPropertyName("existingMother")]
    public CsePerson ExistingMother
    {
      get => existingMother ??= new();
      set => existingMother = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

    /// <summary>
    /// A value of Scheduled.
    /// </summary>
    [JsonPropertyName("scheduled")]
    public GeneticTest Scheduled
    {
      get => scheduled ??= new();
      set => scheduled = value;
    }

    private LegalAction existingPrevPatEstab;
    private LegalAction existingNewPatEstab;
    private Vendor existingPrevDrawSite;
    private GeneticTest existingPrevSampleChildGeneticTest;
    private PersonGeneticTest existingPrevSampleChildPersonGeneticTest;
    private GeneticTest existingPrevSampleMotherGeneticTest;
    private PersonGeneticTest existingPrevSampleMotherPersonGeneticTest;
    private GeneticTest existingPrevSampleFatherGeneticTest;
    private PersonGeneticTest existingPrevSampleFatherPersonGeneticTest;
    private CaseRole existingFatherAbsentParent;
    private GeneticTestAccount existingGeneticTestAccount;
    private GeneticTest existingLast;
    private Vendor existingFatherDrawSite;
    private Vendor existingMotherDrawSite;
    private Vendor existingChildDrawSite;
    private Vendor existingTestSite;
    private PersonGeneticTest scheduledFathers;
    private PersonGeneticTest scheduledMothers;
    private PersonGeneticTest scheduledChilds;
    private Case1 existingCase;
    private CaseRole existingCaseRoleMother;
    private CaseRole existingCaseRoleChild;
    private CsePerson existingFatherCsePerson;
    private CsePerson existingMother;
    private CsePerson existingChild;
    private GeneticTest scheduled;
  }
#endregion
}
