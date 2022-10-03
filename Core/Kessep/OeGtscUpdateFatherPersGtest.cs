// Program: OE_GTSC_UPDATE_FATHER_PERS_GTEST, ID: 371797739, model: 746.
// Short name: SWE00919
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSC_UPDATE_FATHER_PERS_GTEST.
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
public partial class OeGtscUpdateFatherPersGtest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_UPDATE_FATHER_PERS_GTEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscUpdateFatherPersGtest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscUpdateFatherPersGtest.
  /// </summary>
  public OeGtscUpdateFatherPersGtest(IContext context, Import import,
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
    // 	MAINTENANCE LOG
    // AUTHOR		DATE		CHGREQ		DESCRIPTION
    // govind		10-14-95			Initial coding
    // Ty Hill-MTW     04/28/97                        Change Current_date
    // rcg		03/11/98	H00032901  edits for Show_Ind = 'N'
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // V. Madhira  5/31/2000 PR# 93588  Motherless comparisons.
    // ******* END MAINTENANCE LOG ****************
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This Common Action Block updates genetic_test record.
    // PROCESSING:
    // This action block is passed the screen input of schedule genetic test.
    // It reads and updates GENETIC_TEST.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CASE			- R - -
    // 	CASE_ROLE		- R - -
    // 		FATHER
    // 		MOTHER
    // 		CHILD
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		- R U -
    // DATABASE FILES USED:
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

    // -------------------------------------------------------------------------
    // Per PR# 93588 ====> If mother is not selected (Motherless comparisons) 
    // bypass the READ.
    // -------------------------------------------------------------------------
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

    // ---------------------------------------------
    // Read CASE_ROLE records for father, mother and child.
    // ---------------------------------------------
    if (!ReadAbsentParent())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    // -------------------------------------------------------------------------
    // Per PR# 93588 ====> If mother is not selected (Motherless comparisons) 
    // bypass the READ.
    // -------------------------------------------------------------------------
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

    local.SchedFatherTestTime.TimeWithAmPm =
      export.GeneticTestInformation.FatherSchedTestTime;

    if (IsEmpty(local.SchedFatherTestTime.TimeWithAmPm))
    {
      local.SchedFatherTestTime.Wtime = TimeSpan.Zero;
    }
    else
    {
      UseCabConvertTimeFormat();

      if (AsChar(local.ErrorInTestTime.Flag) == 'Y')
      {
        ExitState = "OE0000_INVALID_FATHER_SCHED_TIME";

        return;
      }
    }

    // ---------------------------------------------
    // Read GENETIC_TEST associated with the three case roles.
    // ---------------------------------------------
    local.LatestGeneticTestFound.Flag = "N";

    if (entities.ExistingMotherCaseRole.Populated)
    {
      if (ReadGeneticTest1())
      {
        local.LatestGeneticTestFound.Flag = "Y";
        local.Latest.TestNumber = entities.ExistingScheduled.TestNumber;
      }
    }
    else
    {
      // -------------------------------------------------------------------------
      // Per PR# 93588 ====> Modified the above READ EACH if mother is not 
      // selected (Motherless comparisons)
      // -------------------------------------------------------------------------
      if (ReadGeneticTest2())
      {
        local.LatestGeneticTestFound.Flag = "Y";
        local.Latest.TestNumber = entities.ExistingScheduled.TestNumber;
      }
    }

    if (AsChar(local.LatestGeneticTestFound.Flag) == 'N')
    {
      // A genetic test has not been scheduled yet. This is an error.
      ExitState = "OE0019_GEN_TEST_NOT_YET_SCHED";

      return;
    }

    if (!ReadGeneticTest3())
    {
      ExitState = "OE0103_UNABLE_TO_READ_GT";

      return;
    }

    // --------------------------------------------
    // Read CSE_PERSON record for father
    // --------------------------------------------
    if (!ReadCsePerson2())
    {
      ExitState = "OE0059_NF_FATHER_CSE_PERSON";

      return;
    }

    local.FatherPersGenTestFound.Flag = "N";

    if (ReadPersonGeneticTest1())
    {
      local.FatherPersGenTestFound.Flag = "Y";
    }

    if (AsChar(local.FatherPersGenTestFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // No PERSON_GENETIC_TEST record found. This is an error.
      // ---------------------------------------------
      ExitState = "OE0060_NF_FATH_PERS_GEN_TEST";
    }
    else
    {
      if (export.GeneticTestInformation.FatherPrevSampGtestNumber != 0 && export
        .GeneticTestInformation.FatherPrevSampPerGenTestId != 0)
      {
        // ---------------------------------------------
        // When reusing a sample, the draw site id cannot be changed
        // ---------------------------------------------
        if (ReadPersonGeneticTestGeneticTestVendor())
        {
          export.GeneticTestInformation.FatherDrawSiteId =
            NumberToString(entities.ExistingPrevDrawSite.Identifier, 8, 8);
          export.GeneticTestInformation.FatherDrawSiteVendorName =
            entities.ExistingPrevDrawSite.Name;
          UseOeCabGetVendorAddress1();
          export.GeneticTestInformation.FatherDrawSiteCity =
            local.VendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.FatherDrawSiteState =
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
      if (AsChar(entities.ExistingScheduledFathers.SampleCollectedInd) != AsChar
        (export.GeneticTestInformation.FatherSampleCollectedInd) || AsChar
        (entities.ExistingScheduledFathers.CollectSampleInd) != AsChar
        (export.GeneticTestInformation.FatherCollectSampleInd) || AsChar
        (entities.ExistingScheduledFathers.ShowInd) != AsChar
        (export.GeneticTestInformation.FatherShowInd) || !
        Equal(entities.ExistingScheduledFathers.SpecimenId,
        export.GeneticTestInformation.FatherSpecimenId) || !
        Equal(entities.ExistingScheduledFathers.ScheduledTestDate,
        export.GeneticTestInformation.FatherSchedTestDate) || !
        Equal(entities.ExistingScheduledFathers.ScheduledTestTime,
        local.SchedFatherTestTime.Wtime))
      {
        if (AsChar(entities.ExistingScheduledFathers.ShowInd) != AsChar
          (export.GeneticTestInformation.FatherShowInd))
        {
          // ********************************************
          // 03/11/98	RCG	H00032901  modify edit for Show_Ind.
          // ********************************************
          if (AsChar(export.GeneticTestInformation.FatherShowInd) == 'N')
          {
            if (Lt(local.Current.Date,
              entities.ExistingScheduledFathers.ScheduledTestDate))
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

      // ---------------------------------------------
      // Associate with the previous sample person genetic test if the previous 
      // sample was reused.
      // ---------------------------------------------
      // ---------------------------------------------
      // Make sure that the current sample itself is not specified as reused. 
      // That would result in circular association with itself.
      // ---------------------------------------------
      if (export.GeneticTestInformation.FatherPrevSampGtestNumber == entities
        .ExistingScheduled.TestNumber && export
        .GeneticTestInformation.FatherPrevSampPerGenTestId == entities
        .ExistingScheduledFathers.Identifier)
      {
        ExitState = "OE0000_PREV_GTEST_SAMP_NEED_FTHR";

        return;
      }

      if (ReadGeneticTestPersonGeneticTest())
      {
        if (export.GeneticTestInformation.FatherPrevSampGtestNumber != entities
          .ExistingPrevSampleFatherGeneticTest.TestNumber || export
          .GeneticTestInformation.FatherPrevSampPerGenTestId != entities
          .ExistingPrevSampleFatherPersonGeneticTest.Identifier)
        {
          // ---------------------------------------------
          // Reuse sample has been changed.
          // ---------------------------------------------
          if (export.GeneticTestInformation.FatherPrevSampGtestNumber == entities
            .ExistingScheduled.TestNumber && export
            .GeneticTestInformation.FatherPrevSampPerGenTestId == entities
            .ExistingScheduledFathers.Identifier)
          {
            ExitState = "OE0000_PREV_GTEST_SAMP_NEED_FTHR";

            return;
          }

          if (export.GeneticTestInformation.FatherPrevSampGtestNumber == 0 || export
            .GeneticTestInformation.FatherPrevSampPerGenTestId == 0)
          {
            DisassociatePersonGeneticTest2();
          }
          else if (ReadPersonGeneticTest2())
          {
            try
            {
              UpdatePersonGeneticTest3();
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
            ExitState = "OE0000_PREV_SAMPLE_FATHER_NF";
          }
        }
        else
        {
          // ---------------------------------------------
          // Reuse sample has not been changed. So no action
          // ---------------------------------------------
          if (!Lt(entities.ExistingScheduledFathers.ScheduledTestDate,
            new DateTime(1, 1, 1)))
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
        }
      }
      else
      {
        // ---------------------------------------------
        // It did not reuse previous sample before.
        // ---------------------------------------------
        if (!IsEmpty(export.GeneticTestInformation.FatherPrevSampSpecimenId))
        {
          // ---------------------------------------------
          // But now it is updated to reuse previous sample.
          // ---------------------------------------------
          if (ReadPersonGeneticTest2())
          {
            try
            {
              UpdatePersonGeneticTest3();
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
            ExitState = "OE0000_PREV_SAMPLE_FATHER_NF";
          }
        }
        else
        {
          // ---------------------------------------------
          // No change in reuse of previous sample. Sample not reused at all. 
          // Allow update of vendor id only if sample is not reused.
          // ---------------------------------------------
          if (!IsEmpty(export.GeneticTestInformation.FatherDrawSiteId))
          {
            if (Verify(export.GeneticTestInformation.FatherDrawSiteId,
              " 0123456789") != 0)
            {
              ExitState = "OE0031_INVALID_DRAW_SITE_ID_FA";

              return;
            }

            local.Temp.Identifier =
              (int)StringToNumber(export.GeneticTestInformation.FatherDrawSiteId);
              
          }
          else
          {
            local.Temp.Identifier = 0;
          }

          if (ReadVendor2())
          {
            local.TempCurrent.Identifier =
              entities.ExistingPrevDrawSite.Identifier;

            if (IsEmpty(export.GeneticTestInformation.FatherDrawSiteId))
            {
              DisassociatePersonGeneticTest1();
              export.GeneticTestInformation.FatherDrawSiteVendorName = "";
              export.GeneticTestInformation.FatherDrawSiteCity = "";
              export.GeneticTestInformation.FatherDrawSiteState = "";
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
              export.GeneticTestInformation.FatherDrawSiteVendorName =
                entities.ExistingNewDrawSite.Name;
              UseOeCabGetVendorAddress2();
              export.GeneticTestInformation.FatherDrawSiteCity =
                local.VendorAddress.City ?? Spaces(15);
              export.GeneticTestInformation.FatherDrawSiteState =
                local.VendorAddress.State ?? Spaces(2);
            }
            else
            {
              ExitState = "OE0031_INVALID_DRAW_SITE_ID_FA";
            }
          }
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

    MoveWorkTime(local.SchedFatherTestTime, useImport.WorkTime);

    Call(CabConvertTimeFormat.Execute, useImport, useExport);

    MoveWorkTime(useExport.WorkTime, local.SchedFatherTestTime);
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
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);

    var venIdentifier = entities.ExistingNewDrawSite.Identifier;

    entities.ExistingScheduledFathers.Populated = false;
    Update("AssociatePersonGeneticTest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingScheduledFathers.GteTestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingScheduledFathers.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingScheduledFathers.Identifier);
      });

    entities.ExistingScheduledFathers.VenIdentifier = venIdentifier;
    entities.ExistingScheduledFathers.Populated = true;
  }

  private void DisassociatePersonGeneticTest1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);

    var gteTestNumber = entities.ExistingScheduledFathers.GteTestNumber;

    entities.ExistingScheduledFathers.Populated = false;

    bool exists;

    Update("DisassociatePersonGeneticTest1#1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber1", gteTestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingScheduledFathers.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingScheduledFathers.Identifier);
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

    entities.ExistingScheduledFathers.VenIdentifier = null;
    entities.ExistingScheduledFathers.Populated = true;
  }

  private void DisassociatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);

    var gteTestNumber = entities.ExistingScheduledFathers.GteTestNumber;

    entities.ExistingScheduledFathers.Populated = false;

    bool exists;

    Update("DisassociatePersonGeneticTest2#1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber1", gteTestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingScheduledFathers.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingScheduledFathers.Identifier);
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

    entities.ExistingScheduledFathers.PgtIdentifier = null;
    entities.ExistingScheduledFathers.CspRNumber = null;
    entities.ExistingScheduledFathers.GteRTestNumber = null;
    entities.ExistingScheduledFathers.Populated = true;
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
    System.Diagnostics.Debug.Assert(entities.ExistingChildCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingMotherCaseRole.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    entities.ExistingScheduled.Populated = false;

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
        entities.ExistingScheduled.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingScheduled.CasNumber = db.GetNullableString(reader, 1);
        entities.ExistingScheduled.CspNumber = db.GetNullableString(reader, 2);
        entities.ExistingScheduled.CroType = db.GetNullableString(reader, 3);
        entities.ExistingScheduled.CroIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingScheduled.CasMNumber = db.GetNullableString(reader, 5);
        entities.ExistingScheduled.CspMNumber = db.GetNullableString(reader, 6);
        entities.ExistingScheduled.CroMType = db.GetNullableString(reader, 7);
        entities.ExistingScheduled.CroMIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingScheduled.CasANumber = db.GetNullableString(reader, 9);
        entities.ExistingScheduled.CspANumber =
          db.GetNullableString(reader, 10);
        entities.ExistingScheduled.CroAType = db.GetNullableString(reader, 11);
        entities.ExistingScheduled.CroAIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ExistingScheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingScheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.ExistingScheduled.CroMType);
          
        CheckValid<GeneticTest>("CroAType", entities.ExistingScheduled.CroAType);
          
      });
  }

  private bool ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingChildCaseRole.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    entities.ExistingScheduled.Populated = false;

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
        entities.ExistingScheduled.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingScheduled.CasNumber = db.GetNullableString(reader, 1);
        entities.ExistingScheduled.CspNumber = db.GetNullableString(reader, 2);
        entities.ExistingScheduled.CroType = db.GetNullableString(reader, 3);
        entities.ExistingScheduled.CroIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingScheduled.CasMNumber = db.GetNullableString(reader, 5);
        entities.ExistingScheduled.CspMNumber = db.GetNullableString(reader, 6);
        entities.ExistingScheduled.CroMType = db.GetNullableString(reader, 7);
        entities.ExistingScheduled.CroMIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingScheduled.CasANumber = db.GetNullableString(reader, 9);
        entities.ExistingScheduled.CspANumber =
          db.GetNullableString(reader, 10);
        entities.ExistingScheduled.CroAType = db.GetNullableString(reader, 11);
        entities.ExistingScheduled.CroAIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ExistingScheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingScheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.ExistingScheduled.CroMType);
          
        CheckValid<GeneticTest>("CroAType", entities.ExistingScheduled.CroAType);
          
      });
  }

  private bool ReadGeneticTest3()
  {
    entities.ExistingScheduled.Populated = false;

    return Read("ReadGeneticTest3",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", local.Latest.TestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingScheduled.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingScheduled.CasNumber = db.GetNullableString(reader, 1);
        entities.ExistingScheduled.CspNumber = db.GetNullableString(reader, 2);
        entities.ExistingScheduled.CroType = db.GetNullableString(reader, 3);
        entities.ExistingScheduled.CroIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingScheduled.CasMNumber = db.GetNullableString(reader, 5);
        entities.ExistingScheduled.CspMNumber = db.GetNullableString(reader, 6);
        entities.ExistingScheduled.CroMType = db.GetNullableString(reader, 7);
        entities.ExistingScheduled.CroMIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingScheduled.CasANumber = db.GetNullableString(reader, 9);
        entities.ExistingScheduled.CspANumber =
          db.GetNullableString(reader, 10);
        entities.ExistingScheduled.CroAType = db.GetNullableString(reader, 11);
        entities.ExistingScheduled.CroAIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ExistingScheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingScheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.ExistingScheduled.CroMType);
          
        CheckValid<GeneticTest>("CroAType", entities.ExistingScheduled.CroAType);
          
      });
  }

  private bool ReadGeneticTestPersonGeneticTest()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);
    entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = false;
    entities.ExistingPrevSampleFatherGeneticTest.Populated = false;

    return Read("ReadGeneticTestPersonGeneticTest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingScheduledFathers.PgtIdentifier.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingScheduledFathers.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingScheduledFathers.GteRTestNumber.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleFatherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleFatherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevSampleFatherGeneticTest.TestType =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevSampleFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 3);
        entities.ExistingPrevSampleFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 4);
        entities.ExistingPrevSampleFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 5);
        entities.ExistingPrevSampleFatherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPrevSampleFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = true;
        entities.ExistingPrevSampleFatherGeneticTest.Populated = true;
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

  private bool ReadPersonGeneticTest1()
  {
    entities.ExistingScheduledFathers.Populated = false;

    return Read("ReadPersonGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingScheduled.TestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingScheduledFathers.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingScheduledFathers.CspNumber = db.GetString(reader, 1);
        entities.ExistingScheduledFathers.Identifier = db.GetInt32(reader, 2);
        entities.ExistingScheduledFathers.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingScheduledFathers.CollectSampleInd =
          db.GetNullableString(reader, 4);
        entities.ExistingScheduledFathers.ShowInd =
          db.GetNullableString(reader, 5);
        entities.ExistingScheduledFathers.SampleCollectedInd =
          db.GetNullableString(reader, 6);
        entities.ExistingScheduledFathers.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 7);
        entities.ExistingScheduledFathers.ScheduledTestDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingScheduledFathers.LastUpdatedBy =
          db.GetString(reader, 9);
        entities.ExistingScheduledFathers.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ExistingScheduledFathers.VenIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingScheduledFathers.PgtIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ExistingScheduledFathers.CspRNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingScheduledFathers.GteRTestNumber =
          db.GetNullableInt32(reader, 14);
        entities.ExistingScheduledFathers.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest2()
  {
    entities.ExistingNewPrevSampleFatherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest2",
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
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingNewPrevSampleFatherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTestGeneticTestVendor()
  {
    entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = false;
    entities.ExistingPrevSampleFatherGeneticTest.Populated = false;
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadPersonGeneticTestGeneticTestVendor",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber",
          export.GeneticTestInformation.FatherPrevSampGtestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.FatherPrevSampPerGenTestId);
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleFatherGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevSampleFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevSampleFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleFatherPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 4);
        entities.ExistingPrevSampleFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPrevSampleFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingPrevSampleFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleFatherGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleFatherGeneticTest.TestType =
          db.GetNullableString(reader, 9);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 10);
        entities.ExistingPrevSampleFatherPersonGeneticTest.Populated = true;
        entities.ExistingPrevSampleFatherGeneticTest.Populated = true;
        entities.ExistingPrevDrawSite.Populated = true;
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
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingScheduledFathers.VenIdentifier.GetValueOrDefault());
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
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);

    var specimenId = export.GeneticTestInformation.FatherSpecimenId;
    var collectSampleInd = export.GeneticTestInformation.FatherCollectSampleInd;
    var showInd = export.GeneticTestInformation.FatherShowInd;
    var sampleCollectedInd =
      export.GeneticTestInformation.FatherSampleCollectedInd;
    var scheduledTestTime = local.SchedFatherTestTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.FatherSchedTestDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingScheduledFathers.Populated = false;
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
          command, "gteTestNumber",
          entities.ExistingScheduledFathers.GteTestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingScheduledFathers.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingScheduledFathers.Identifier);
      });

    entities.ExistingScheduledFathers.SpecimenId = specimenId;
    entities.ExistingScheduledFathers.CollectSampleInd = collectSampleInd;
    entities.ExistingScheduledFathers.ShowInd = showInd;
    entities.ExistingScheduledFathers.SampleCollectedInd = sampleCollectedInd;
    entities.ExistingScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ExistingScheduledFathers.ScheduledTestDate = scheduledTestDate;
    entities.ExistingScheduledFathers.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingScheduledFathers.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingScheduledFathers.Populated = true;
  }

  private void UpdatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);

    var scheduledTestTime = TimeSpan.Zero;

    entities.ExistingScheduledFathers.Populated = false;
    Update("UpdatePersonGeneticTest2",
      (db, command) =>
      {
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", null);
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingScheduledFathers.GteTestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingScheduledFathers.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingScheduledFathers.Identifier);
      });

    entities.ExistingScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ExistingScheduledFathers.ScheduledTestDate = null;
    entities.ExistingScheduledFathers.Populated = true;
  }

  private void UpdatePersonGeneticTest3()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingScheduledFathers.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingNewPrevSampleFatherPersonGeneticTest.Populated);

    var scheduledTestTime = TimeSpan.Zero;
    var pgtIdentifier =
      entities.ExistingNewPrevSampleFatherPersonGeneticTest.Identifier;
    var cspRNumber =
      entities.ExistingNewPrevSampleFatherPersonGeneticTest.CspNumber;
    var gteRTestNumber =
      entities.ExistingNewPrevSampleFatherPersonGeneticTest.GteTestNumber;

    entities.ExistingScheduledFathers.Populated = false;
    Update("UpdatePersonGeneticTest3",
      (db, command) =>
      {
        db.SetNullableTimeSpan(command, "schedTestTime", scheduledTestTime);
        db.SetNullableDate(command, "schedTestDate", null);
        db.SetNullableInt32(command, "pgtIdentifier", pgtIdentifier);
        db.SetNullableString(command, "cspRNumber", cspRNumber);
        db.SetNullableInt32(command, "gteRTestNumber", gteRTestNumber);
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingScheduledFathers.GteTestNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingScheduledFathers.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingScheduledFathers.Identifier);
      });

    entities.ExistingScheduledFathers.ScheduledTestTime = scheduledTestTime;
    entities.ExistingScheduledFathers.ScheduledTestDate = null;
    entities.ExistingScheduledFathers.PgtIdentifier = pgtIdentifier;
    entities.ExistingScheduledFathers.CspRNumber = cspRNumber;
    entities.ExistingScheduledFathers.GteRTestNumber = gteRTestNumber;
    entities.ExistingScheduledFathers.Populated = true;
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
    /// A value of FatherPersGenTestFound.
    /// </summary>
    [JsonPropertyName("fatherPersGenTestFound")]
    public Common FatherPersGenTestFound
    {
      get => fatherPersGenTestFound ??= new();
      set => fatherPersGenTestFound = value;
    }

    /// <summary>
    /// A value of SchedFatherTestTime.
    /// </summary>
    [JsonPropertyName("schedFatherTestTime")]
    public WorkTime SchedFatherTestTime
    {
      get => schedFatherTestTime ??= new();
      set => schedFatherTestTime = value;
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
    private Common fatherPersGenTestFound;
    private WorkTime schedFatherTestTime;
    private Common errorInTestTime;
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
    /// A value of ExistingScheduled.
    /// </summary>
    [JsonPropertyName("existingScheduled")]
    public GeneticTest ExistingScheduled
    {
      get => existingScheduled ??= new();
      set => existingScheduled = value;
    }

    /// <summary>
    /// A value of ExistingScheduledFathers.
    /// </summary>
    [JsonPropertyName("existingScheduledFathers")]
    public PersonGeneticTest ExistingScheduledFathers
    {
      get => existingScheduledFathers ??= new();
      set => existingScheduledFathers = value;
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
    /// A value of ExistingNewPrevSampleFatherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingNewPrevSampleFatherPersonGeneticTest")]
    public PersonGeneticTest ExistingNewPrevSampleFatherPersonGeneticTest
    {
      get => existingNewPrevSampleFatherPersonGeneticTest ??= new();
      set => existingNewPrevSampleFatherPersonGeneticTest = value;
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
    /// A value of ExistingNewPrevSampleFatherGeneticTest.
    /// </summary>
    [JsonPropertyName("existingNewPrevSampleFatherGeneticTest")]
    public GeneticTest ExistingNewPrevSampleFatherGeneticTest
    {
      get => existingNewPrevSampleFatherGeneticTest ??= new();
      set => existingNewPrevSampleFatherGeneticTest = value;
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
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
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
    /// A value of ExistingFatherCsePerson.
    /// </summary>
    [JsonPropertyName("existingFatherCsePerson")]
    public CsePerson ExistingFatherCsePerson
    {
      get => existingFatherCsePerson ??= new();
      set => existingFatherCsePerson = value;
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
    /// A value of ExistingFatherAbsentParent.
    /// </summary>
    [JsonPropertyName("existingFatherAbsentParent")]
    public CaseRole ExistingFatherAbsentParent
    {
      get => existingFatherAbsentParent ??= new();
      set => existingFatherAbsentParent = value;
    }

    private GeneticTest existingScheduled;
    private PersonGeneticTest existingScheduledFathers;
    private PersonGeneticTest existingPrevSampleFatherPersonGeneticTest;
    private PersonGeneticTest existingNewPrevSampleFatherPersonGeneticTest;
    private GeneticTest existingPrevSampleFatherGeneticTest;
    private GeneticTest existingNewPrevSampleFatherGeneticTest;
    private Vendor existingPrevDrawSite;
    private Vendor existingNewDrawSite;
    private Case1 existing;
    private CsePerson existingChild;
    private CsePerson existingMother;
    private CsePerson existingFatherCsePerson;
    private CaseRole existingChildCaseRole;
    private CaseRole existingMotherCaseRole;
    private CaseRole existingFatherAbsentParent;
  }
#endregion
}
