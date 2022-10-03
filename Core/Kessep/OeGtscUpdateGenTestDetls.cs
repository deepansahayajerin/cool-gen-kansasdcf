// Program: OE_GTSC_UPDATE_GEN_TEST_DETLS, ID: 371797165, model: 746.
// Short name: SWE00923
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSC_UPDATE_GEN_TEST_DETLS.
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
public partial class OeGtscUpdateGenTestDetls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_UPDATE_GEN_TEST_DETLS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscUpdateGenTestDetls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscUpdateGenTestDetls.
  /// </summary>
  public OeGtscUpdateGenTestDetls(IContext context, Import import, Export export)
    :
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
    // DESCRIPTION:
    // This BAA Process Action Block updates receipt of genetic test 
    // confirmation.
    // PROCESSING:
    // This action block is passed the screen input of schedule genetic test.
    // It reads and updates PERSON_GENETIC_TEST for SAMPLE_COLLECTED_IND and 
    // SAMPLE_USABLE_IND.
    // ENTITY TYPES USED:
    // 	CASE			- R - -
    // 	CASE_ROLE		- R - -
    // 		FATHER
    // 		MOTHER
    // 		CHILD
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		- R - -
    // 	PERSON_GENETIC_TEST	- R U -
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

    // ---------------------------------------------
    // Read GENETIC_TEST associated with the three case roles.
    // ---------------------------------------------
    local.LatestGeneticTestFound.Flag = "N";

    if (entities.ExistingCaseRoleMother.Populated)
    {
      if (ReadGeneticTest1())
      {
        local.LatestGeneticTestFound.Flag = "Y";
        local.Latest.TestNumber = entities.Scheduled.TestNumber;
      }
    }
    else if (ReadGeneticTest2())
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

    if (!ReadGeneticTest3())
    {
      ExitState = "OE0103_UNABLE_TO_READ_GT";

      return;
    }

    UseOeGtscAssocLegActWithGtest();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.GeneticTestInformation.Assign(export.GeneticTestInformation);
    UseOeGtscUpdateFatherPersGtest();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (entities.ExistingMother.Populated)
    {
      local.GeneticTestInformation.Assign(export.GeneticTestInformation);
      UseOeGtscUpdateMotherPersGtest();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    local.GeneticTestInformation.Assign(export.GeneticTestInformation);
    UseOeGtscUpdateChildPersGtest();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Check if any GENETIC_TEST details are changed. If so update GENETIC_TEST
    // ---------------------------------------------
    local.GeneticTestInformation.Assign(export.GeneticTestInformation);
    UseOeGtscUpdateGeneticTest();
  }

  private static void MoveGeneticTestInformation(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CourtOrderNo = source.CourtOrderNo;
    target.GeneticTestAccountNo = source.GeneticTestAccountNo;
    target.LabCaseNo = source.LabCaseNo;
    target.TestType = source.TestType;
    target.FatherPersonNo = source.FatherPersonNo;
    target.FatherFormattedName = source.FatherFormattedName;
    target.FatherLastName = source.FatherLastName;
    target.FatherMi = source.FatherMi;
    target.FatherFirstName = source.FatherFirstName;
    target.FatherDrawSiteId = source.FatherDrawSiteId;
    target.FatherDrawSiteVendorName = source.FatherDrawSiteVendorName;
    target.FatherDrawSiteCity = source.FatherDrawSiteCity;
    target.FatherDrawSiteState = source.FatherDrawSiteState;
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.FatherSchedTestTime = source.FatherSchedTestTime;
    target.FatherCollectSampleInd = source.FatherCollectSampleInd;
    target.FatherReuseSampleInd = source.FatherReuseSampleInd;
    target.FatherShowInd = source.FatherShowInd;
    target.FatherSampleCollectedInd = source.FatherSampleCollectedInd;
    target.FatherPrevSampExistsInd = source.FatherPrevSampExistsInd;
    target.FatherPrevSampGtestNumber = source.FatherPrevSampGtestNumber;
    target.FatherPrevSampTestType = source.FatherPrevSampTestType;
    target.FatherPrevSampleLabCaseNo = source.FatherPrevSampleLabCaseNo;
    target.FatherPrevSampSpecimenId = source.FatherPrevSampSpecimenId;
    target.FatherPrevSampPerGenTestId = source.FatherPrevSampPerGenTestId;
    target.FatherSpecimenId = source.FatherSpecimenId;
    target.FatherRescheduledInd = source.FatherRescheduledInd;
    target.MotherPersonNo = source.MotherPersonNo;
    target.MotherFormattedName = source.MotherFormattedName;
    target.MotherLastName = source.MotherLastName;
    target.MotherMi = source.MotherMi;
    target.MotherFirstName = source.MotherFirstName;
    target.MotherDrawSiteId = source.MotherDrawSiteId;
    target.MotherDrawSiteVendorName = source.MotherDrawSiteVendorName;
    target.MotherDrawSiteCity = source.MotherDrawSiteCity;
    target.MotherDrawSiteState = source.MotherDrawSiteState;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.MotherSchedTestTime = source.MotherSchedTestTime;
    target.MotherCollectSampleInd = source.MotherCollectSampleInd;
    target.MotherReuseSampleInd = source.MotherReuseSampleInd;
    target.MotherShowInd = source.MotherShowInd;
    target.MotherSampleCollectedInd = source.MotherSampleCollectedInd;
    target.MotherPrevSampExistsInd = source.MotherPrevSampExistsInd;
    target.MotherPrevSampGtestNumber = source.MotherPrevSampGtestNumber;
    target.MotherPrevSampTestType = source.MotherPrevSampTestType;
    target.MotherPrevSampLabCaseNo = source.MotherPrevSampLabCaseNo;
    target.MotherPrevSampSpecimenId = source.MotherPrevSampSpecimenId;
    target.MotherPrevSampPerGenTestId = source.MotherPrevSampPerGenTestId;
    target.MotherSpecimenId = source.MotherSpecimenId;
    target.MotherRescheduledInd = source.MotherRescheduledInd;
    target.ChildPersonNo = source.ChildPersonNo;
    target.ChildFormattedName = source.ChildFormattedName;
    target.ChildLastName = source.ChildLastName;
    target.ChildMi = source.ChildMi;
    target.ChildFirstName = source.ChildFirstName;
    target.ChildDrawSiteId = source.ChildDrawSiteId;
    target.ChildDrawSiteVendorName = source.ChildDrawSiteVendorName;
    target.ChildDrawSiteCity = source.ChildDrawSiteCity;
    target.ChildDrawSiteState = source.ChildDrawSiteState;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
    target.ChildSchedTestTime = source.ChildSchedTestTime;
    target.ChildCollectSampleInd = source.ChildCollectSampleInd;
    target.ChildReuseSampleInd = source.ChildReuseSampleInd;
    target.ChildShowInd = source.ChildShowInd;
    target.ChildSampleCollectedInd = source.ChildSampleCollectedInd;
    target.ChildPrevSampExistsInd = source.ChildPrevSampExistsInd;
    target.ChildPrevSampGtestNumber = source.ChildPrevSampGtestNumber;
    target.ChildPrevSampTestType = source.ChildPrevSampTestType;
    target.ChildPrevSampLabCaseNo = source.ChildPrevSampLabCaseNo;
    target.ChildPrevSampSpecimenId = source.ChildPrevSampSpecimenId;
    target.ChildPrevSampPerGenTestId = source.ChildPrevSampPerGenTestId;
    target.ChildSpecimenId = source.ChildSpecimenId;
    target.ChildReschedInd = source.ChildReschedInd;
    target.TestSiteVendorId = source.TestSiteVendorId;
    target.TestSiteVendorName = source.TestSiteVendorName;
    target.TestSiteCity = source.TestSiteCity;
    target.TestSiteState = source.TestSiteState;
    target.ActualTestDate = source.ActualTestDate;
    target.ScheduledTestDate = source.ScheduledTestDate;
    target.ResultReceivedDate = source.ResultReceivedDate;
    target.PaternityExcludedInd = source.PaternityExcludedInd;
    target.PaternityProbability = source.PaternityProbability;
    target.ResultContestedInd = source.ResultContestedInd;
    target.ContestStartedDate = source.ContestStartedDate;
    target.ContestEndedDate = source.ContestEndedDate;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseOeGtscAssocLegActWithGtest()
  {
    var useImport = new OeGtscAssocLegActWithGtest.Import();
    var useExport = new OeGtscAssocLegActWithGtest.Export();

    useImport.Scheduled.TestNumber = entities.Scheduled.TestNumber;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);

    Call(OeGtscAssocLegActWithGtest.Execute, useImport, useExport);
  }

  private void UseOeGtscUpdateChildPersGtest()
  {
    var useImport = new OeGtscUpdateChildPersGtest.Import();
    var useExport = new OeGtscUpdateChildPersGtest.Export();

    useImport.GeneticTestInformation.Assign(local.GeneticTestInformation);

    Call(OeGtscUpdateChildPersGtest.Execute, useImport, useExport);

    export.GeneticTestInformation.Assign(useExport.GeneticTestInformation);
  }

  private void UseOeGtscUpdateFatherPersGtest()
  {
    var useImport = new OeGtscUpdateFatherPersGtest.Import();
    var useExport = new OeGtscUpdateFatherPersGtest.Export();

    useImport.GeneticTestInformation.Assign(local.GeneticTestInformation);
    useExport.GeneticTestInformation.Assign(export.GeneticTestInformation);

    Call(OeGtscUpdateFatherPersGtest.Execute, useImport, useExport);

    MoveGeneticTestInformation(useExport.GeneticTestInformation,
      export.GeneticTestInformation);
  }

  private void UseOeGtscUpdateGeneticTest()
  {
    var useImport = new OeGtscUpdateGeneticTest.Import();
    var useExport = new OeGtscUpdateGeneticTest.Export();

    useImport.PersistentExistingFathr.
      Assign(entities.ExistingFatherAbsentParent);
    useImport.PersistentExistCaseroleMother.Assign(
      entities.ExistingCaseRoleMother);
    useImport.PersistentExistCaseroleChild.
      Assign(entities.ExistingCaseRoleChild);
    useImport.GeneticTestInformation.Assign(local.GeneticTestInformation);

    Call(OeGtscUpdateGeneticTest.Execute, useImport, useExport);

    export.GeneticTestInformation.Assign(useExport.GeneticTestInformation);
  }

  private void UseOeGtscUpdateMotherPersGtest()
  {
    var useImport = new OeGtscUpdateMotherPersGtest.Import();
    var useExport = new OeGtscUpdateMotherPersGtest.Export();

    useImport.GeneticTestInformation.Assign(local.GeneticTestInformation);

    Call(OeGtscUpdateMotherPersGtest.Execute, useImport, useExport);

    MoveGeneticTestInformation(useExport.GeneticTestInformation,
      export.GeneticTestInformation);
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
    entities.ExistingCaseRoleChild.Populated = false;

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
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleMother.Populated);
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);
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
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherAbsentParent.Populated);
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

  private bool ReadMother()
  {
    entities.ExistingCaseRoleMother.Populated = false;

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
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
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

    private DateWorkArea current;
    private GeneticTestInformation geneticTestInformation;
    private GeneticTest latest;
    private Common latestGeneticTestFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    private CaseRole existingFatherAbsentParent;
    private Case1 existing;
    private CaseRole existingCaseRoleMother;
    private CaseRole existingCaseRoleChild;
    private CsePerson existingFatherCsePerson;
    private CsePerson existingMother;
    private CsePerson existingChild;
    private GeneticTest scheduled;
  }
#endregion
}
