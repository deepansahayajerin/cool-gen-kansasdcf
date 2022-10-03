// Program: OE_GTSC_UPDATE_MOTHER_PERS_GTEST, ID: 371797738, model: 746.
// Short name: SWE00921
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSC_UPDATE_MOTHER_PERS_GTEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block updates Confirmation and Results of a genetic test.
/// This has been created by combining the two BAA processes namely Receive
/// Genetic Test Confirmation and Receive Genetic Test Result. The original BAA
/// processes are left intact in case they are required to be used.
/// </para>
/// </summary>
[Serializable]
public partial class OeGtscUpdateMotherPersGtest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_UPDATE_MOTHER_PERS_GTEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscUpdateMotherPersGtest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscUpdateMotherPersGtest.
  /// </summary>
  public OeGtscUpdateMotherPersGtest(IContext context, Import import,
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
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	      DATE  	CHGREQ  	DESCRIPTION
    // govind	    10-14-95                	Initial coding
    // Ty Hill-MTW 04/28/97                    Change Current_date
    // rcg		03/11/98	H00032901  edits for Show_Ind = 'N'
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG ****************
    // *********************************************
    // SYSTEM:		KESSEP
    // DESCRIPTION:
    // This Common Action Block updates genetic_test record.
    // PROCESSING:
    // This action block is passed the screen input of schedule genetic test.
    // It reads and updates GENETIC_TEST.
    // ENTITY TYPES USED:
    // 	CASE			- R - -
    // 	CASE_ROLE		- R - -
    // 		FATHER
    // 		MOTHER
    // 		CHILD
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		- R U -
    // CREATED BY:	Govindaraj.
    // DATE CREATED:	10-14-1995
    // *********************************************
    local.Current.Date = Now().Date;
    export.GeneticTestInformation.Assign(import.GeneticTestInformation);

    if (!ReadCase())
    {
      ExitState = "OE0020_INVALID_CASE_NO";

      return;
    }

    // --------------------------------------------
    // Read CSE_PERSON records for father, mother and child
    // --------------------------------------------
    if (!ReadCsePerson2())
    {
      ExitState = "OE0059_NF_FATHER_CSE_PERSON";

      return;
    }

    if (!ReadCsePerson3())
    {
      ExitState = "OE0062_NF_MOTHER_CSE_PERSON";

      return;
    }

    if (!ReadCsePerson1())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    // ---------------------------------------------
    // Read CASE_ROLE records for father, mother and child.
    // ---------------------------------------------
    if (!ReadAbsentParent())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (!ReadMother())
    {
      ExitState = "OE0055_NF_CASE_ROLE_MOTHER";

      return;
    }

    if (!ReadChild())
    {
      ExitState = "OE0065_NF_CASE_ROLE_CHILD";

      return;
    }

    local.SchedMotherTestTime.TimeWithAmPm =
      import.GeneticTestInformation.MotherSchedTestTime;

    if (IsEmpty(local.SchedMotherTestTime.TimeWithAmPm))
    {
      local.SchedMotherTestTime.Wtime = TimeSpan.Zero;
    }
    else
    {
      UseCabConvertTimeFormat();

      if (AsChar(local.ErrorInTestTime.Flag) == 'Y')
      {
        ExitState = "OE0000_INVALID_MOTHER_SCHED_TM";

        return;
      }
    }

    // ---------------------------------------------
    // Read GENETIC_TEST associated with the three case roles.
    // ---------------------------------------------
    local.LatestGeneticTestFound.Flag = "N";

    if (ReadGeneticTest1())
    {
      local.LatestGeneticTestFound.Flag = "Y";
      local.Latest.TestNumber = entities.Scheduled.TestNumber;
    }

    if (AsChar(local.LatestGeneticTestFound.Flag) == 'N')
    {
      // A genetic test has not been scheduled yet. This is an error.
      ExitState = "OE0019_GEN_TEST_NOT_YET_SCHED";

      return;
    }

    if (!ReadGeneticTest2())
    {
      ExitState = "OE0103_UNABLE_TO_READ_GT";

      return;
    }

    if (!ReadCsePerson3())
    {
      ExitState = "OE0062_NF_MOTHER_CSE_PERSON";

      return;
    }

    local.MotherPersGenTestFound.Flag = "N";

    if (ReadPersonGeneticTest())
    {
      local.MotherPersGenTestFound.Flag = "Y";
    }

    if (AsChar(local.MotherPersGenTestFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // No PERSON_GENETIC_TEST record found. This is an error.
      // ---------------------------------------------
      ExitState = "OE0063_NF_MOTH_PERS_GEN_TEST";

      return;
    }
    else
    {
      if (export.GeneticTestInformation.MotherPrevSampGtestNumber != 0 && export
        .GeneticTestInformation.MotherPrevSampPerGenTestId != 0)
      {
        // ---------------------------------------------
        // When reusing a sample, the draw site id cannot be changed
        // ---------------------------------------------
        if (ReadGeneticTestPersonGeneticTestVendor())
        {
          export.GeneticTestInformation.MotherDrawSiteId =
            NumberToString(entities.ExistingPrevDrawSite.Identifier, 8, 8);
          export.GeneticTestInformation.MotherDrawSiteVendorName =
            entities.ExistingPrevDrawSite.Name;
          UseOeCabGetVendorAddress1();
          export.GeneticTestInformation.MotherDrawSiteCity =
            local.VendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.MotherDrawSiteState =
            local.VendorAddress.State ?? Spaces(2);
        }
      }

      // ---------------------------------------------
      // Update if any of the following data has been changed:
      //  sample collected indicator
      //  sample reusable indicator
      //  show ind
      //  specimen ID
      // ---------------------------------------------
      if (AsChar(entities.ScheduledMothers.CollectSampleInd) != AsChar
        (export.GeneticTestInformation.MotherCollectSampleInd) || AsChar
        (entities.ScheduledMothers.SampleCollectedInd) != AsChar
        (export.GeneticTestInformation.MotherSampleCollectedInd) || AsChar
        (entities.ScheduledMothers.ShowInd) != AsChar
        (export.GeneticTestInformation.MotherShowInd) || !
        Equal(entities.ScheduledMothers.SpecimenId,
        export.GeneticTestInformation.MotherSpecimenId) || !
        Equal(entities.ScheduledMothers.ScheduledTestDate,
        export.GeneticTestInformation.MotherSchedTestDate) || !
        Equal(entities.ScheduledMothers.ScheduledTestTime,
        local.SchedMotherTestTime.Wtime))
      {
        if (AsChar(entities.ScheduledMothers.ShowInd) != AsChar
          (export.GeneticTestInformation.MotherShowInd))
        {
          // ********************************************
          // 03/11/98	RCG	H00032901  modify edit for Show_Ind.
          // ********************************************
          if (AsChar(export.GeneticTestInformation.MotherShowInd) == 'N')
          {
            if (Lt(local.Current.Date,
              export.GeneticTestInformation.MotherSchedTestDate))
            {
              ExitState = "OE0000_SHOW_IND_PREMATURE";

              return;
            }
          }
        }

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
    // Associate with the previous sample person genetic test if the previous 
    // sample was reused.
    // ---------------------------------------------
    if (export.GeneticTestInformation.MotherPrevSampGtestNumber == entities
      .Scheduled.TestNumber && export
      .GeneticTestInformation.MotherPrevSampPerGenTestId == entities
      .ScheduledMothers.Identifier)
    {
      ExitState = "OE0000_PREV_GTEST_SAMP_NEED_MTHR";

      return;
    }

    if (ReadPersonGeneticTestGeneticTest2())
    {
      if (export.GeneticTestInformation.MotherPrevSampGtestNumber != entities
        .ExistingPrevSampleMotherGeneticTest.TestNumber || export
        .GeneticTestInformation.MotherPrevSampPerGenTestId != entities
        .ExistingPrevSampleMotherPersonGeneticTest.Identifier)
      {
        // ---------------------------------------------
        // Reuse sample has been changed.
        // ---------------------------------------------
        if (export.GeneticTestInformation.MotherPrevSampGtestNumber == 0 || export
          .GeneticTestInformation.MotherPrevSampPerGenTestId == 0)
        {
          DisassociatePersonGeneticTest2();
        }
        else if (ReadPersonGeneticTestGeneticTest1())
        {
          try
          {
            UpdatePersonGeneticTest2();
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
        else
        {
          ExitState = "OE0000_PREV_SAMPLE_MOTHER_NF";
        }
      }
      else
      {
        // ---------------------------------------------
        // Reuse sample has not been changed. So no action.
        // ---------------------------------------------
      }
    }
    else if (!IsEmpty(export.GeneticTestInformation.MotherPrevSampSpecimenId))
    {
      // ---------------------------------------------
      // Reuse sample has now been specified.
      // ---------------------------------------------
      if (ReadPersonGeneticTestGeneticTest1())
      {
        try
        {
          UpdatePersonGeneticTest2();
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
      else
      {
        ExitState = "OE0000_PREV_SAMPLE_MOTHER_NF";
      }
    }
    else
    {
      // ---------------------------------------------
      // Reuse sample has not been changed. So no action. But check and update 
      // draw site vendor id. Allow update of vendor id only if sample not
      // reused.
      // ---------------------------------------------
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
      }
      else
      {
        local.Temp.Identifier = 0;
      }

      if (ReadVendor2())
      {
        local.TempCurrent.Identifier = entities.ExistingPrevDrawSite.Identifier;

        if (IsEmpty(export.GeneticTestInformation.MotherDrawSiteId))
        {
          DisassociatePersonGeneticTest1();
          export.GeneticTestInformation.MotherDrawSiteVendorName = "";
          export.GeneticTestInformation.MotherDrawSiteCity = "";
          export.GeneticTestInformation.MotherDrawSiteState = "";
        }
      }
      else
      {
        local.TempCurrent.Identifier = 0;
      }

      if (local.Temp.Identifier != local.TempCurrent.Identifier && local
        .Temp.Identifier != 0)
      {
        if (ReadVendor1())
        {
          AssociatePersonGeneticTest();
          export.GeneticTestInformation.MotherDrawSiteVendorName =
            entities.ExistingNewDrawSite.Name;
          UseOeCabGetVendorAddress2();
          export.GeneticTestInformation.MotherDrawSiteCity =
            local.VendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.MotherDrawSiteState =
            local.VendorAddress.State ?? Spaces(2);
        }
        else
        {
          ExitState = "OE0032_INVALID_DRAW_SITE_ID_MO";
        }
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

    MoveWorkTime(local.SchedMotherTestTime, useImport.WorkTime);

    Call(CabConvertTimeFormat.Execute, useImport, useExport);

    MoveWorkTime(useExport.WorkTime, local.SchedMotherTestTime);
    local.ErrorInTestTime.Flag = useExport.ErrorInConversion.Flag;
  }

  private void UseOeCabGetVendorAddress1()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingPrevDrawSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private void UseOeCabGetVendorAddress2()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingNewDrawSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private void AssociatePersonGeneticTest()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var venIdentifier = entities.ExistingNewDrawSite.Identifier;

    entities.ScheduledMothers.Populated = false;
    Update("AssociatePersonGeneticTest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledMothers.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    entities.ScheduledMothers.VenIdentifier = venIdentifier;
    entities.ScheduledMothers.Populated = true;
  }

  private void DisassociatePersonGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var gteTestNumber = entities.ScheduledMothers.GteTestNumber;

    entities.ScheduledMothers.Populated = false;

    bool exists;

    Update("DisassociatePersonGeneticTest1#1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber1", gteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    exists = Read("DisassociatePersonGeneticTest1#2",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber2", gteTestNumber);
      },
      null);

    if (!exists)
    {
      Update("DisassociatePersonGeneticTest1#3",
        (db, command) =>
        {
          db.SetInt32(command, "gteTestNumber2", gteTestNumber);
        });
    }

    entities.ScheduledMothers.VenIdentifier = null;
    entities.ScheduledMothers.Populated = true;
  }

  private void DisassociatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var gteTestNumber = entities.ScheduledMothers.GteTestNumber;

    entities.ScheduledMothers.Populated = false;

    bool exists;

    Update("DisassociatePersonGeneticTest2#1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber1", gteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledMothers.CspNumber);
        db.
          SetInt32(command, "identifier", entities.ScheduledMothers.Identifier);
          
      });

    exists = Read("DisassociatePersonGeneticTest2#2",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber2", gteTestNumber);
      },
      null);

    if (!exists)
    {
      Update("DisassociatePersonGeneticTest2#3",
        (db, command) =>
        {
          db.SetInt32(command, "gteTestNumber2", gteTestNumber);
        });
    }

    entities.ScheduledMothers.PgtIdentifier = null;
    entities.ScheduledMothers.CspRNumber = null;
    entities.ScheduledMothers.GteRTestNumber = null;
    entities.ScheduledMothers.Populated = true;
  }

  private bool ReadAbsentParent()
  {
    entities.ExistingFatherAbsentParent.Populated = false;

    return Read("ReadAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Existing.Number);
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
    entities.Existing.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.GeneticTestInformation.CaseNumber);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.ExistingChildCaseRole.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Existing.Number);
        db.SetString(command, "cspNumber", entities.ExistingChild.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChildCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingChildCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingChildCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingChildCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChildCaseRole.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingChildCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChildCaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingChildCaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingChildCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChildCaseRole.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.ExistingChildCaseRole.ResidesWithArIndicator);
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
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingMotherCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingChildCaseRole.Populated);
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest1",
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
          entities.ExistingMotherCaseRole.Identifier);
        db.SetNullableString(
          command, "croMType", entities.ExistingMotherCaseRole.Type1);
        db.SetNullableString(
          command, "casMNumber", entities.ExistingMotherCaseRole.CasNumber);
        db.SetNullableString(
          command, "cspMNumber", entities.ExistingMotherCaseRole.CspNumber);
        db.SetNullableInt32(
          command, "croIdentifier", entities.ExistingChildCaseRole.Identifier);
        db.SetNullableString(
          command, "croType", entities.ExistingChildCaseRole.Type1);
        db.SetNullableString(
          command, "casNumber", entities.ExistingChildCaseRole.CasNumber);
        db.SetNullableString(
          command, "cspNumber", entities.ExistingChildCaseRole.CspNumber);
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
        entities.Scheduled.StartDateOfContest = db.GetNullableDate(reader, 8);
        entities.Scheduled.EndDateOfContest = db.GetNullableDate(reader, 9);
        entities.Scheduled.CreatedBy = db.GetString(reader, 10);
        entities.Scheduled.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.Scheduled.LastUpdatedBy = db.GetString(reader, 12);
        entities.Scheduled.LastUpdatedTimestamp = db.GetDateTime(reader, 13);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 15);
        entities.Scheduled.CroType = db.GetNullableString(reader, 16);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 17);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 18);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 19);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 20);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 21);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 22);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 23);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 24);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 25);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTest2()
  {
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest2",
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
        entities.Scheduled.StartDateOfContest = db.GetNullableDate(reader, 8);
        entities.Scheduled.EndDateOfContest = db.GetNullableDate(reader, 9);
        entities.Scheduled.CreatedBy = db.GetString(reader, 10);
        entities.Scheduled.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.Scheduled.LastUpdatedBy = db.GetString(reader, 12);
        entities.Scheduled.LastUpdatedTimestamp = db.GetDateTime(reader, 13);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 15);
        entities.Scheduled.CroType = db.GetNullableString(reader, 16);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 17);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 18);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 19);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 20);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 21);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 22);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 23);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 24);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 25);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTestPersonGeneticTestVendor()
  {
    entities.ExistingPrevSampleMotherGeneticTest.Populated = false;
    entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = false;
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadGeneticTestPersonGeneticTestVendor",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber",
          export.GeneticTestInformation.MotherPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.MotherPrevSampPerGenTestId);
        db.SetNullableString(
          command, "cspMNumber", entities.ExistingMother.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleMotherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleMotherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevSampleMotherGeneticTest.TestType =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevSampleMotherGeneticTest.CasMNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleMotherGeneticTest.CspMNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPrevSampleMotherGeneticTest.CroMType =
          db.GetNullableString(reader, 5);
        entities.ExistingPrevSampleMotherGeneticTest.CroMIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPrevSampleMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingPrevSampleMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 8);
        entities.ExistingPrevSampleMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 9);
        entities.ExistingPrevSampleMotherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 10);
        entities.ExistingPrevSampleMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingPrevSampleMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingPrevSampleMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 14);
        entities.ExistingPrevSampleMotherGeneticTest.Populated = true;
        entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = true;
        entities.ExistingPrevDrawSite.Populated = true;
        CheckValid<GeneticTest>("CroMType",
          entities.ExistingPrevSampleMotherGeneticTest.CroMType);
      });
  }

  private bool ReadMother()
  {
    entities.ExistingMotherCaseRole.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Existing.Number);
        db.SetString(command, "cspNumber", entities.ExistingMother.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMotherCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingMotherCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingMotherCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingMotherCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingMotherCaseRole.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingMotherCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingMotherCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingMotherCaseRole.Type1);
      });
  }

  private bool ReadPersonGeneticTest()
  {
    entities.ScheduledMothers.Populated = false;

    return Read("ReadPersonGeneticTest",
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
        entities.ScheduledMothers.SpecimenId = db.GetNullableString(reader, 3);
        entities.ScheduledMothers.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ScheduledMothers.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ScheduledMothers.ShowInd = db.GetNullableString(reader, 6);
        entities.ScheduledMothers.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ScheduledMothers.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ScheduledMothers.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ScheduledMothers.CreatedBy = db.GetString(reader, 10);
        entities.ScheduledMothers.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.ScheduledMothers.LastUpdatedBy = db.GetString(reader, 12);
        entities.ScheduledMothers.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.ScheduledMothers.VenIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.ScheduledMothers.PgtIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ScheduledMothers.CspRNumber = db.GetNullableString(reader, 16);
        entities.ScheduledMothers.GteRTestNumber =
          db.GetNullableInt32(reader, 17);
        entities.ScheduledMothers.Populated = true;
      });
  }

  private bool ReadPersonGeneticTestGeneticTest1()
  {
    entities.ExistingNewPrevSampleMotherPersonGeneticTest.Populated = false;
    entities.ExistingNewPrevSampleMotherGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTestGeneticTest1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingMother.Number);
        db.SetInt32(
          command, "testNumber",
          export.GeneticTestInformation.MotherPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.MotherPrevSampPerGenTestId);
      },
      (db, reader) =>
      {
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingNewPrevSampleMotherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingNewPrevSampleMotherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 7);
        entities.ExistingNewPrevSampleMotherGeneticTest.TestType =
          db.GetNullableString(reader, 8);
        entities.ExistingNewPrevSampleMotherPersonGeneticTest.Populated = true;
        entities.ExistingNewPrevSampleMotherGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTestGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);
    entities.ExistingPrevSampleMotherGeneticTest.Populated = false;
    entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTestGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ScheduledMothers.PgtIdentifier.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.ScheduledMothers.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ScheduledMothers.GteRTestNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleMotherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevSampleMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevSampleMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleMotherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingPrevSampleMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPrevSampleMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingPrevSampleMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleMotherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleMotherGeneticTest.TestType =
          db.GetNullableString(reader, 9);
        entities.ExistingPrevSampleMotherGeneticTest.CasMNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingPrevSampleMotherGeneticTest.CspMNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingPrevSampleMotherGeneticTest.CroMType =
          db.GetNullableString(reader, 12);
        entities.ExistingPrevSampleMotherGeneticTest.CroMIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPrevSampleMotherGeneticTest.Populated = true;
        entities.ExistingPrevSampleMotherPersonGeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroMType",
          entities.ExistingPrevSampleMotherGeneticTest.CroMType);
      });
  }

  private bool ReadVendor1()
  {
    entities.ExistingNewDrawSite.Populated = false;

    return Read("ReadVendor1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingNewDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingNewDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingNewDrawSite.Populated = true;
      });
  }

  private bool ReadVendor2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ScheduledMothers.VenIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 1);
        entities.ExistingPrevDrawSite.Populated = true;
      });
  }

  private void UpdatePersonGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);

    var specimenId = export.GeneticTestInformation.MotherSpecimenId;
    var collectSampleInd = export.GeneticTestInformation.MotherCollectSampleInd;
    var showInd = export.GeneticTestInformation.MotherShowInd;
    var sampleCollectedInd =
      export.GeneticTestInformation.MotherSampleCollectedInd;
    var scheduledTestTime = local.SchedMotherTestTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.MotherSchedTestDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledMothers.Populated = false;
    Update("UpdatePersonGeneticTest1",
      (db, command) =>
      {
        db.SetNullableString(command, "specimenId", specimenId);
        db.SetNullableString(command, "collectSampleInd", collectSampleInd);
        db.SetNullableString(command, "showInd", showInd);
        db.SetNullableString(command, "sampleCollInd", sampleCollectedInd);
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

    entities.ScheduledMothers.SpecimenId = specimenId;
    entities.ScheduledMothers.CollectSampleInd = collectSampleInd;
    entities.ScheduledMothers.ShowInd = showInd;
    entities.ScheduledMothers.SampleCollectedInd = sampleCollectedInd;
    entities.ScheduledMothers.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledMothers.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledMothers.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledMothers.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledMothers.Populated = true;
  }

  private void UpdatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledMothers.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingNewPrevSampleMotherPersonGeneticTest.Populated);

    var scheduledTestTime = TimeSpan.Zero;
    var pgtIdentifier =
      entities.ExistingNewPrevSampleMotherPersonGeneticTest.Identifier;
    var cspRNumber =
      entities.ExistingNewPrevSampleMotherPersonGeneticTest.CspNumber;
    var gteRTestNumber =
      entities.ExistingNewPrevSampleMotherPersonGeneticTest.GteTestNumber;

    entities.ScheduledMothers.Populated = false;
    Update("UpdatePersonGeneticTest2",
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
    /// A value of SchedMotherTestTime.
    /// </summary>
    [JsonPropertyName("schedMotherTestTime")]
    public WorkTime SchedMotherTestTime
    {
      get => schedMotherTestTime ??= new();
      set => schedMotherTestTime = value;
    }

    /// <summary>
    /// A value of ErrorInTestTime.
    /// </summary>
    [JsonPropertyName("errorInTestTime")]
    public Common ErrorInTestTime
    {
      get => errorInTestTime ??= new();
      set => errorInTestTime = value;
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
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
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
    /// A value of TempCurrent.
    /// </summary>
    [JsonPropertyName("tempCurrent")]
    public Vendor TempCurrent
    {
      get => tempCurrent ??= new();
      set => tempCurrent = value;
    }

    private DateWorkArea current;
    private GeneticTest latest;
    private Common latestGeneticTestFound;
    private WorkTime schedMotherTestTime;
    private Common errorInTestTime;
    private Common motherPersGenTestFound;
    private VendorAddress vendorAddress;
    private Vendor temp;
    private Vendor tempCurrent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Scheduled.
    /// </summary>
    [JsonPropertyName("scheduled")]
    public GeneticTest Scheduled
    {
      get => scheduled ??= new();
      set => scheduled = value;
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
    /// A value of ScheduledMothers.
    /// </summary>
    [JsonPropertyName("scheduledMothers")]
    public PersonGeneticTest ScheduledMothers
    {
      get => scheduledMothers ??= new();
      set => scheduledMothers = value;
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
    /// A value of ExistingPrevDrawSite.
    /// </summary>
    [JsonPropertyName("existingPrevDrawSite")]
    public Vendor ExistingPrevDrawSite
    {
      get => existingPrevDrawSite ??= new();
      set => existingPrevDrawSite = value;
    }

    /// <summary>
    /// A value of ExistingNewPrevSampleMotherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingNewPrevSampleMotherPersonGeneticTest")]
    public PersonGeneticTest ExistingNewPrevSampleMotherPersonGeneticTest
    {
      get => existingNewPrevSampleMotherPersonGeneticTest ??= new();
      set => existingNewPrevSampleMotherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingNewPrevSampleMotherGeneticTest.
    /// </summary>
    [JsonPropertyName("existingNewPrevSampleMotherGeneticTest")]
    public GeneticTest ExistingNewPrevSampleMotherGeneticTest
    {
      get => existingNewPrevSampleMotherGeneticTest ??= new();
      set => existingNewPrevSampleMotherGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingNewDrawSite.
    /// </summary>
    [JsonPropertyName("existingNewDrawSite")]
    public Vendor ExistingNewDrawSite
    {
      get => existingNewDrawSite ??= new();
      set => existingNewDrawSite = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
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
    /// A value of ExistingMotherCaseRole.
    /// </summary>
    [JsonPropertyName("existingMotherCaseRole")]
    public CaseRole ExistingMotherCaseRole
    {
      get => existingMotherCaseRole ??= new();
      set => existingMotherCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingChildCaseRole.
    /// </summary>
    [JsonPropertyName("existingChildCaseRole")]
    public CaseRole ExistingChildCaseRole
    {
      get => existingChildCaseRole ??= new();
      set => existingChildCaseRole = value;
    }

    private GeneticTest scheduled;
    private CsePerson existingMother;
    private PersonGeneticTest scheduledMothers;
    private GeneticTest existingPrevSampleMotherGeneticTest;
    private PersonGeneticTest existingPrevSampleMotherPersonGeneticTest;
    private Vendor existingPrevDrawSite;
    private PersonGeneticTest existingNewPrevSampleMotherPersonGeneticTest;
    private GeneticTest existingNewPrevSampleMotherGeneticTest;
    private Vendor existingNewDrawSite;
    private Case1 existing;
    private CsePerson existingFatherCsePerson;
    private CsePerson existingChild;
    private CaseRole existingFatherAbsentParent;
    private CaseRole existingMotherCaseRole;
    private CaseRole existingChildCaseRole;
  }
#endregion
}
