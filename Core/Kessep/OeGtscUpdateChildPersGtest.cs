// Program: OE_GTSC_UPDATE_CHILD_PERS_GTEST, ID: 371797737, model: 746.
// Short name: SWE00918
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSC_UPDATE_CHILD_PERS_GTEST.
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
public partial class OeGtscUpdateChildPersGtest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_UPDATE_CHILD_PERS_GTEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscUpdateChildPersGtest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscUpdateChildPersGtest.
  /// </summary>
  public OeGtscUpdateChildPersGtest(IContext context, Import import,
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
    // CREATED BY:	Govindaraj.
    // DATE CREATED:	10-14-1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE		CHGREQ		DESCRIPTION
    // govind		10-14-95			Initial coding
    // Ty Hill-MTW     04/29/97                        Change Current_date
    // rcg		03-11-98	H00032901  edit for Show_Ind = 'N'
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // V.Madhira  05/31/2000  PR# 93588  Motherless comparisons.
    // ******* END MAINTENANCE LOG ****************
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

    local.SchedChildTestTime.TimeWithAmPm =
      import.GeneticTestInformation.ChildSchedTestTime;

    if (IsEmpty(local.SchedChildTestTime.TimeWithAmPm))
    {
      local.SchedChildTestTime.Wtime = TimeSpan.Zero;
    }
    else
    {
      UseCabConvertTimeFormat();

      if (AsChar(local.ErrorInTestTime.Flag) == 'Y')
      {
        ExitState = "OE0000_INVALID_CHILD_SCHD_TESTTM";

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
        local.Latest.TestNumber = entities.Scheduled.TestNumber;
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
        local.Latest.TestNumber = entities.Scheduled.TestNumber;
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

    if (!ReadCsePerson1())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    local.ChildPersGenTestFound.Flag = "N";

    if (ReadPersonGeneticTest())
    {
      local.ChildPersGenTestFound.Flag = "Y";
    }

    if (AsChar(local.ChildPersGenTestFound.Flag) == 'N')
    {
      // ---------------------------------------------
      // No PERSON_GENETIC_TEST record found. This is an error.
      // ---------------------------------------------
      ExitState = "OE0057_NF_CHILD_PERS_GEN_TEST";

      return;
    }
    else
    {
      if (export.GeneticTestInformation.ChildPrevSampGtestNumber != 0 && export
        .GeneticTestInformation.ChildPrevSampPerGenTestId != 0)
      {
        // ---------------------------------------------
        // When reusing a sample, the draw site id cannot be changed
        // ---------------------------------------------
        if (ReadGeneticTestPersonGeneticTestVendor())
        {
          export.GeneticTestInformation.ChildDrawSiteId =
            NumberToString(entities.ExistingPrevDrawSite.Identifier, 8, 8);
          export.GeneticTestInformation.ChildDrawSiteVendorName =
            entities.ExistingPrevDrawSite.Name;
          UseOeCabGetVendorAddress1();
          export.GeneticTestInformation.ChildDrawSiteCity =
            local.VendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.ChildDrawSiteState =
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
      if (AsChar(entities.ScheduledChilds.CollectSampleInd) != AsChar
        (export.GeneticTestInformation.ChildCollectSampleInd) || AsChar
        (entities.ScheduledChilds.SampleCollectedInd) != AsChar
        (export.GeneticTestInformation.ChildSampleCollectedInd) || AsChar
        (entities.ScheduledChilds.ShowInd) != AsChar
        (export.GeneticTestInformation.ChildShowInd) || !
        Equal(entities.ScheduledChilds.SpecimenId,
        export.GeneticTestInformation.ChildSpecimenId) || !
        Equal(entities.ScheduledChilds.ScheduledTestDate,
        export.GeneticTestInformation.ChildSchedTestDate) || !
        Equal(entities.ScheduledChilds.ScheduledTestTime,
        local.SchedChildTestTime.Wtime))
      {
        if (AsChar(entities.ScheduledChilds.ShowInd) != AsChar
          (export.GeneticTestInformation.ChildShowInd))
        {
          // ********************************************
          // 03/11/98	RCG	H00032901  modify edit for Show_Ind.
          // ********************************************
          if (AsChar(export.GeneticTestInformation.ChildShowInd) == 'N')
          {
            if (Lt(local.Current.Date,
              export.GeneticTestInformation.ChildSchedTestDate))
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
    if (export.GeneticTestInformation.ChildPrevSampGtestNumber == entities
      .Scheduled.TestNumber && export
      .GeneticTestInformation.ChildPrevSampPerGenTestId == entities
      .ScheduledChilds.Identifier)
    {
      ExitState = "OE0000_PREV_GTEST_SAMP_NEED_CHLD";

      return;
    }

    if (ReadPersonGeneticTestGeneticTest2())
    {
      if (export.GeneticTestInformation.ChildPrevSampGtestNumber != entities
        .ExistingPrevSampleChildGeneticTest.TestNumber || entities
        .ExistingPrevSampleChildPersonGeneticTest.Identifier != entities
        .ExistingPrevSampleChildPersonGeneticTest.Identifier)
      {
        // ---------------------------------------------
        // Reuse sample has been changed.
        // ---------------------------------------------
        if (export.GeneticTestInformation.ChildPrevSampGtestNumber == 0 || export
          .GeneticTestInformation.ChildPrevSampPerGenTestId == 0)
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
          ExitState = "OE0000_PREV_SAMPLE_CHILD_NF";
        }
      }
      else
      {
        // ---------------------------------------------
        // Reuse sample has not been changed. So no action.
        // ---------------------------------------------
      }
    }
    else if (!IsEmpty(export.GeneticTestInformation.ChildPrevSampSpecimenId))
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
        ExitState = "OE0000_PREV_SAMPLE_CHILD_NF";
      }
    }
    else
    {
      // ---------------------------------------------
      // Reuse sample has not been changed. So no action.
      // ---------------------------------------------
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
      }
      else
      {
        local.Temp.Identifier = 0;
      }

      if (ReadVendor2())
      {
        local.TempCurrent.Identifier = entities.ExistingPrevDrawSite.Identifier;

        if (IsEmpty(export.GeneticTestInformation.ChildDrawSiteId))
        {
          DisassociatePersonGeneticTest1();
          export.GeneticTestInformation.ChildDrawSiteVendorName = "";
          export.GeneticTestInformation.ChildDrawSiteCity = "";
          export.GeneticTestInformation.ChildDrawSiteState = "";
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
          export.GeneticTestInformation.ChildDrawSiteVendorName =
            entities.ExistingNewDrawSite.Name;
          UseOeCabGetVendorAddress2();
          export.GeneticTestInformation.ChildDrawSiteCity =
            local.VendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.ChildDrawSiteState =
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

    MoveWorkTime(local.SchedChildTestTime, useImport.WorkTime);

    Call(CabConvertTimeFormat.Execute, useImport, useExport);

    MoveWorkTime(useExport.WorkTime, local.SchedChildTestTime);
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
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var venIdentifier = entities.ExistingNewDrawSite.Identifier;

    entities.ScheduledChilds.Populated = false;
    Update("AssociatePersonGeneticTest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(
          command, "gteTestNumber", entities.ScheduledChilds.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
      });

    entities.ScheduledChilds.VenIdentifier = venIdentifier;
    entities.ScheduledChilds.Populated = true;
  }

  private void DisassociatePersonGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var gteTestNumber = entities.ScheduledChilds.GteTestNumber;

    entities.ScheduledChilds.Populated = false;

    bool exists;

    Update("DisassociatePersonGeneticTest1#1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber1", gteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
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

    entities.ScheduledChilds.VenIdentifier = null;
    entities.ScheduledChilds.Populated = true;
  }

  private void DisassociatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var gteTestNumber = entities.ScheduledChilds.GteTestNumber;

    entities.ScheduledChilds.Populated = false;

    bool exists;

    Update("DisassociatePersonGeneticTest2#1",
      (db, command) =>
      {
        db.SetInt32(command, "gteTestNumber1", gteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
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

    entities.ScheduledChilds.PgtIdentifier = null;
    entities.ScheduledChilds.CspRNumber = null;
    entities.ScheduledChilds.GteRTestNumber = null;
    entities.ScheduledChilds.Populated = true;
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
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 1);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 2);
        entities.Scheduled.CroType = db.GetNullableString(reader, 3);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 4);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 5);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 6);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 7);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 8);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 9);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 10);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 11);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 12);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingChildCaseRole.Populated);
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
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 1);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 2);
        entities.Scheduled.CroType = db.GetNullableString(reader, 3);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 4);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 5);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 6);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 7);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 8);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 9);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 10);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 11);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 12);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTest3()
  {
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest3",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", local.Latest.TestNumber);
      },
      (db, reader) =>
      {
        entities.Scheduled.TestNumber = db.GetInt32(reader, 0);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 1);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 2);
        entities.Scheduled.CroType = db.GetNullableString(reader, 3);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 4);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 5);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 6);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 7);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 8);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 9);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 10);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 11);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 12);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTestPersonGeneticTestVendor()
  {
    entities.ExistingPrevSampleChildGeneticTest.Populated = false;
    entities.ExistingPrevSampleChildPersonGeneticTest.Populated = false;
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadGeneticTestPersonGeneticTestVendor",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber",
          export.GeneticTestInformation.ChildPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.ChildPrevSampPerGenTestId);
        db.
          SetNullableString(command, "cspNumber", entities.ExistingChild.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleChildGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleChildGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingPrevSampleChildGeneticTest.TestType =
          db.GetNullableString(reader, 2);
        entities.ExistingPrevSampleChildGeneticTest.CasNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleChildGeneticTest.CspNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingPrevSampleChildGeneticTest.CroType =
          db.GetNullableString(reader, 5);
        entities.ExistingPrevSampleChildGeneticTest.CroIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPrevSampleChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingPrevSampleChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 8);
        entities.ExistingPrevSampleChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 9);
        entities.ExistingPrevSampleChildPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPrevDrawSite.Identifier = db.GetInt32(reader, 10);
        entities.ExistingPrevSampleChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingPrevSampleChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingPrevSampleChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPrevDrawSite.Name = db.GetString(reader, 14);
        entities.ExistingPrevSampleChildGeneticTest.Populated = true;
        entities.ExistingPrevSampleChildPersonGeneticTest.Populated = true;
        entities.ExistingPrevDrawSite.Populated = true;
        CheckValid<GeneticTest>("CroType",
          entities.ExistingPrevSampleChildGeneticTest.CroType);
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
    entities.ScheduledChilds.Populated = false;

    return Read("ReadPersonGeneticTest",
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
        entities.ScheduledChilds.SpecimenId = db.GetNullableString(reader, 3);
        entities.ScheduledChilds.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ScheduledChilds.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ScheduledChilds.ShowInd = db.GetNullableString(reader, 6);
        entities.ScheduledChilds.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ScheduledChilds.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ScheduledChilds.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ScheduledChilds.CreatedBy = db.GetString(reader, 10);
        entities.ScheduledChilds.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.ScheduledChilds.LastUpdatedBy = db.GetString(reader, 12);
        entities.ScheduledChilds.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.ScheduledChilds.VenIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.ScheduledChilds.PgtIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ScheduledChilds.CspRNumber = db.GetNullableString(reader, 16);
        entities.ScheduledChilds.GteRTestNumber =
          db.GetNullableInt32(reader, 17);
        entities.ScheduledChilds.Populated = true;
      });
  }

  private bool ReadPersonGeneticTestGeneticTest1()
  {
    entities.ExistingNewPrevSampleChildPersonGeneticTest.Populated = false;
    entities.ExistingNewPrevSampleChildGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTestGeneticTest1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingChild.Number);
        db.SetInt32(
          command, "testNumber",
          export.GeneticTestInformation.ChildPrevSampGtestNumber);
        db.SetInt32(
          command, "identifier",
          export.GeneticTestInformation.ChildPrevSampPerGenTestId);
      },
      (db, reader) =>
      {
        entities.ExistingNewPrevSampleChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingNewPrevSampleChildGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingNewPrevSampleChildGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 7);
        entities.ExistingNewPrevSampleChildGeneticTest.TestType =
          db.GetNullableString(reader, 8);
        entities.ExistingNewPrevSampleChildPersonGeneticTest.Populated = true;
        entities.ExistingNewPrevSampleChildGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTestGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);
    entities.ExistingPrevSampleChildGeneticTest.Populated = false;
    entities.ExistingPrevSampleChildPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTestGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ScheduledChilds.PgtIdentifier.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.ScheduledChilds.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ScheduledChilds.GteRTestNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPrevSampleChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleChildGeneticTest.TestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingPrevSampleChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrevSampleChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrevSampleChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPrevSampleChildPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingPrevSampleChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPrevSampleChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingPrevSampleChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPrevSampleChildGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 8);
        entities.ExistingPrevSampleChildGeneticTest.TestType =
          db.GetNullableString(reader, 9);
        entities.ExistingPrevSampleChildGeneticTest.CasNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingPrevSampleChildGeneticTest.CspNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingPrevSampleChildGeneticTest.CroType =
          db.GetNullableString(reader, 12);
        entities.ExistingPrevSampleChildGeneticTest.CroIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ExistingPrevSampleChildGeneticTest.Populated = true;
        entities.ExistingPrevSampleChildPersonGeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType",
          entities.ExistingPrevSampleChildGeneticTest.CroType);
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
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);
    entities.ExistingPrevDrawSite.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ScheduledChilds.VenIdentifier.GetValueOrDefault());
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
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);

    var specimenId = export.GeneticTestInformation.ChildSpecimenId;
    var collectSampleInd = export.GeneticTestInformation.ChildCollectSampleInd;
    var showInd = export.GeneticTestInformation.ChildShowInd;
    var sampleCollectedInd =
      export.GeneticTestInformation.ChildSampleCollectedInd;
    var scheduledTestTime = local.SchedChildTestTime.Wtime;
    var scheduledTestDate = export.GeneticTestInformation.ChildSchedTestDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ScheduledChilds.Populated = false;
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
          command, "gteTestNumber", entities.ScheduledChilds.GteTestNumber);
        db.SetString(command, "cspNumber", entities.ScheduledChilds.CspNumber);
        db.SetInt32(command, "identifier", entities.ScheduledChilds.Identifier);
      });

    entities.ScheduledChilds.SpecimenId = specimenId;
    entities.ScheduledChilds.CollectSampleInd = collectSampleInd;
    entities.ScheduledChilds.ShowInd = showInd;
    entities.ScheduledChilds.SampleCollectedInd = sampleCollectedInd;
    entities.ScheduledChilds.ScheduledTestTime = scheduledTestTime;
    entities.ScheduledChilds.ScheduledTestDate = scheduledTestDate;
    entities.ScheduledChilds.LastUpdatedBy = lastUpdatedBy;
    entities.ScheduledChilds.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ScheduledChilds.Populated = true;
  }

  private void UpdatePersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.ScheduledChilds.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingNewPrevSampleChildPersonGeneticTest.Populated);

    var scheduledTestTime = TimeSpan.Zero;
    var pgtIdentifier =
      entities.ExistingNewPrevSampleChildPersonGeneticTest.Identifier;
    var cspRNumber =
      entities.ExistingNewPrevSampleChildPersonGeneticTest.CspNumber;
    var gteRTestNumber =
      entities.ExistingNewPrevSampleChildPersonGeneticTest.GteTestNumber;

    entities.ScheduledChilds.Populated = false;
    Update("UpdatePersonGeneticTest2",
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
    /// A value of SchedChildTestTime.
    /// </summary>
    [JsonPropertyName("schedChildTestTime")]
    public WorkTime SchedChildTestTime
    {
      get => schedChildTestTime ??= new();
      set => schedChildTestTime = value;
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
    /// A value of ChildPersGenTestFound.
    /// </summary>
    [JsonPropertyName("childPersGenTestFound")]
    public Common ChildPersGenTestFound
    {
      get => childPersGenTestFound ??= new();
      set => childPersGenTestFound = value;
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
    private WorkTime schedChildTestTime;
    private Common errorInTestTime;
    private Common childPersGenTestFound;
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
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
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
    /// A value of ExistingPrevDrawSite.
    /// </summary>
    [JsonPropertyName("existingPrevDrawSite")]
    public Vendor ExistingPrevDrawSite
    {
      get => existingPrevDrawSite ??= new();
      set => existingPrevDrawSite = value;
    }

    /// <summary>
    /// A value of ExistingNewPrevSampleChildPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingNewPrevSampleChildPersonGeneticTest")]
    public PersonGeneticTest ExistingNewPrevSampleChildPersonGeneticTest
    {
      get => existingNewPrevSampleChildPersonGeneticTest ??= new();
      set => existingNewPrevSampleChildPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingNewPrevSampleChildGeneticTest.
    /// </summary>
    [JsonPropertyName("existingNewPrevSampleChildGeneticTest")]
    public GeneticTest ExistingNewPrevSampleChildGeneticTest
    {
      get => existingNewPrevSampleChildGeneticTest ??= new();
      set => existingNewPrevSampleChildGeneticTest = value;
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
    /// A value of ExistingMother.
    /// </summary>
    [JsonPropertyName("existingMother")]
    public CsePerson ExistingMother
    {
      get => existingMother ??= new();
      set => existingMother = value;
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
    private CsePerson existingChild;
    private PersonGeneticTest scheduledChilds;
    private GeneticTest existingPrevSampleChildGeneticTest;
    private PersonGeneticTest existingPrevSampleChildPersonGeneticTest;
    private Vendor existingPrevDrawSite;
    private PersonGeneticTest existingNewPrevSampleChildPersonGeneticTest;
    private GeneticTest existingNewPrevSampleChildGeneticTest;
    private Vendor existingNewDrawSite;
    private Case1 existing;
    private CsePerson existingFatherCsePerson;
    private CsePerson existingMother;
    private CaseRole existingFatherAbsentParent;
    private CaseRole existingMotherCaseRole;
    private CaseRole existingChildCaseRole;
  }
#endregion
}
