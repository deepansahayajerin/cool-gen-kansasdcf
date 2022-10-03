// Program: OE_GTSC_UPDATE_GENETIC_TEST, ID: 371797736, model: 746.
// Short name: SWE00920
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSC_UPDATE_GENETIC_TEST.
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
public partial class OeGtscUpdateGeneticTest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_UPDATE_GENETIC_TEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscUpdateGeneticTest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscUpdateGeneticTest.
  /// </summary>
  public OeGtscUpdateGeneticTest(IContext context, Import import, Export export):
    
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
    // govind	10-14-95	Initial coding
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
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
    // -------------------------------------------------------------------
    // PR# 81845            Vithal Madhira          03/31/2000
    // It is decided that 'Mother' is optional on GTSC screen. The users want 
    // the ability to do 'Motherless Comparisons'  of the genetic test results.
    // The code is modified accordingly.
    // -------------------------------------------------------------------
    export.GeneticTestInformation.Assign(import.GeneticTestInformation);

    // ---------------------------------------------
    // Read GENETIC_TEST associated with the three case roles.
    // ---------------------------------------------
    local.LatestGeneticTestFound.Flag = "N";

    if (!IsEmpty(import.PersistentExistCaseroleMother.Type1))
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

    // ---------------------------------------------
    // Check if any GENETIC_TEST details are changed. If so update GENETIC_TEST
    // ---------------------------------------------
    local.GeneticTestDetChanged.Flag = "N";

    // ---------------------------------------------
    // Check if genetic test account is changed
    // ---------------------------------------------
    if (!ReadGeneticTestAccount2())
    {
      ExitState = "OE0034_INVALID_GENETIC_TEST_AC";

      return;
    }

    if (!Equal(entities.Existing.AccountNumber,
      import.GeneticTestInformation.GeneticTestAccountNo))
    {
      local.GeneticTestDetChanged.Flag = "Y";

      if (!ReadGeneticTestAccount1())
      {
        ExitState = "OE0034_INVALID_GENETIC_TEST_AC";

        return;
      }
    }

    // ---------------------------------------------
    // Check if genetic test result details changed
    // ---------------------------------------------
    if (!Equal(entities.Scheduled.ActualTestDate,
      export.GeneticTestInformation.ActualTestDate) || !
      Equal(entities.Scheduled.EndDateOfContest,
      export.GeneticTestInformation.ContestEndedDate) || !
      Equal(entities.Scheduled.LabCaseNo,
      export.GeneticTestInformation.LabCaseNo) || !
      Equal(entities.Scheduled.NoticeOfContestReceivedInd,
      export.GeneticTestInformation.ResultContestedInd) || AsChar
      (entities.Scheduled.PaternityExclusionInd) != AsChar
      (export.GeneticTestInformation.PaternityExcludedInd) || !
      Equal(entities.Scheduled.PaternityProbability,
      export.GeneticTestInformation.PaternityProbability) || !
      Equal(entities.Scheduled.StartDateOfContest,
      export.GeneticTestInformation.ContestStartedDate) || !
      Equal(entities.Scheduled.TestResultReceivedDate,
      export.GeneticTestInformation.ResultReceivedDate) || !
      Equal(entities.Scheduled.TestType, export.GeneticTestInformation.TestType))
      
    {
      local.GeneticTestDetChanged.Flag = "Y";
    }

    // ---------------------------------------------
    // Check if test site vendor changed
    // ---------------------------------------------
    if (!IsEmpty(export.GeneticTestInformation.TestSiteVendorId))
    {
      if (Verify(export.GeneticTestInformation.TestSiteVendorId, " 0123456789") !=
        0)
      {
        ExitState = "OE0053_INVALID_TEST_SITE_VEND";

        return;
      }

      local.Temp.Identifier =
        (int)StringToNumber(export.GeneticTestInformation.TestSiteVendorId);

      if (ReadVendor2())
      {
        if (local.Temp.Identifier != entities.ExistingTestSite.Identifier)
        {
          // ---------------------------------------------
          // Test vendor ID has changed. Update test site vendor id.
          // ---------------------------------------------
          local.GeneticTestDetChanged.Flag = "Y";
        }
      }
      else
      {
        // ---------------------------------------------
        // Test vendor ID being supplied only now. Associate with the vendor 
        // now.
        // ---------------------------------------------
        local.GeneticTestDetChanged.Flag = "Y";
      }
    }
    else
    {
      // ---------------------------------------------
      // Test vendor not supplied. Might have been blanked out now.
      // ---------------------------------------------
      if (ReadVendor2())
      {
        try
        {
          UpdateGeneticTest1();
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

        export.GeneticTestInformation.TestSiteVendorName = "";
        export.GeneticTestInformation.TestSiteCity = "";
        export.GeneticTestInformation.TestSiteState = "";
      }
      else
      {
        // ---------------------------------------------
        // Test vendor ID not supplied now and was not supplied earlier. So no 
        // action.
        // ---------------------------------------------
      }
    }

    if (AsChar(local.GeneticTestDetChanged.Flag) == 'Y')
    {
      if (!IsEmpty(export.GeneticTestInformation.TestSiteVendorId))
      {
        if (ReadVendor1())
        {
          export.GeneticTestInformation.TestSiteVendorName =
            entities.ExistingTestSite.Name;
        }
        else
        {
          ExitState = "OE0053_INVALID_TEST_SITE_VEND";

          return;
        }

        UseOeCabGetVendorAddress();
        export.GeneticTestInformation.TestSiteCity =
          local.VendorAddress.City ?? Spaces(15);
        export.GeneticTestInformation.TestSiteState =
          local.VendorAddress.State ?? Spaces(2);

        try
        {
          UpdateGeneticTest3();
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
        // ---------------------------------------------
        // Test site vendor id not known.
        // ---------------------------------------------
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

  private void UseOeCabGetVendorAddress()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingTestSite.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    local.VendorAddress.Assign(useExport.VendorAddress);
  }

  private bool ReadGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(import.PersistentExistingFathr.Populated);
    System.Diagnostics.Debug.Assert(
      import.PersistentExistCaseroleMother.Populated);
    System.Diagnostics.Debug.Assert(
      import.PersistentExistCaseroleChild.Populated);
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croAIdentifier", import.PersistentExistingFathr.Identifier);
          
        db.SetNullableString(
          command, "croAType", import.PersistentExistingFathr.Type1);
        db.SetNullableString(
          command, "casANumber", import.PersistentExistingFathr.CasNumber);
        db.SetNullableString(
          command, "cspANumber", import.PersistentExistingFathr.CspNumber);
        db.SetNullableInt32(
          command, "croMIdentifier",
          import.PersistentExistCaseroleMother.Identifier);
        db.SetNullableString(
          command, "croMType", import.PersistentExistCaseroleMother.Type1);
        db.SetNullableString(
          command, "casMNumber",
          import.PersistentExistCaseroleMother.CasNumber);
        db.SetNullableString(
          command, "cspMNumber",
          import.PersistentExistCaseroleMother.CspNumber);
        db.SetNullableInt32(
          command, "croIdentifier",
          import.PersistentExistCaseroleChild.Identifier);
        db.SetNullableString(
          command, "croType", import.PersistentExistCaseroleChild.Type1);
        db.SetNullableString(
          command, "casNumber", import.PersistentExistCaseroleChild.CasNumber);
        db.SetNullableString(
          command, "cspNumber", import.PersistentExistCaseroleChild.CspNumber);
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
        entities.Scheduled.GtaAccountNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.VenIdentifier = db.GetNullableInt32(reader, 15);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 16);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 17);
        entities.Scheduled.CroType = db.GetNullableString(reader, 18);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 19);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 20);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 21);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 22);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 23);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 24);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 25);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 26);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 27);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(import.PersistentExistingFathr.Populated);
    System.Diagnostics.Debug.Assert(
      import.PersistentExistCaseroleChild.Populated);
    entities.Scheduled.Populated = false;

    return Read("ReadGeneticTest2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croAIdentifier", import.PersistentExistingFathr.Identifier);
          
        db.SetNullableString(
          command, "croAType", import.PersistentExistingFathr.Type1);
        db.SetNullableString(
          command, "casANumber", import.PersistentExistingFathr.CasNumber);
        db.SetNullableString(
          command, "cspANumber", import.PersistentExistingFathr.CspNumber);
        db.SetNullableInt32(
          command, "croIdentifier",
          import.PersistentExistCaseroleChild.Identifier);
        db.SetNullableString(
          command, "croType", import.PersistentExistCaseroleChild.Type1);
        db.SetNullableString(
          command, "casNumber", import.PersistentExistCaseroleChild.CasNumber);
        db.SetNullableString(
          command, "cspNumber", import.PersistentExistCaseroleChild.CspNumber);
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
        entities.Scheduled.GtaAccountNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.VenIdentifier = db.GetNullableInt32(reader, 15);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 16);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 17);
        entities.Scheduled.CroType = db.GetNullableString(reader, 18);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 19);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 20);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 21);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 22);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 23);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 24);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 25);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 26);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 27);
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
        entities.Scheduled.GtaAccountNumber = db.GetNullableString(reader, 14);
        entities.Scheduled.VenIdentifier = db.GetNullableInt32(reader, 15);
        entities.Scheduled.CasNumber = db.GetNullableString(reader, 16);
        entities.Scheduled.CspNumber = db.GetNullableString(reader, 17);
        entities.Scheduled.CroType = db.GetNullableString(reader, 18);
        entities.Scheduled.CroIdentifier = db.GetNullableInt32(reader, 19);
        entities.Scheduled.CasMNumber = db.GetNullableString(reader, 20);
        entities.Scheduled.CspMNumber = db.GetNullableString(reader, 21);
        entities.Scheduled.CroMType = db.GetNullableString(reader, 22);
        entities.Scheduled.CroMIdentifier = db.GetNullableInt32(reader, 23);
        entities.Scheduled.CasANumber = db.GetNullableString(reader, 24);
        entities.Scheduled.CspANumber = db.GetNullableString(reader, 25);
        entities.Scheduled.CroAType = db.GetNullableString(reader, 26);
        entities.Scheduled.CroAIdentifier = db.GetNullableInt32(reader, 27);
        entities.Scheduled.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.Scheduled.CroType);
        CheckValid<GeneticTest>("CroMType", entities.Scheduled.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.Scheduled.CroAType);
      });
  }

  private bool ReadGeneticTestAccount1()
  {
    entities.Existing.Populated = false;

    return Read("ReadGeneticTestAccount1",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber",
          import.GeneticTestInformation.GeneticTestAccountNo);
      },
      (db, reader) =>
      {
        entities.Existing.AccountNumber = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadGeneticTestAccount2()
  {
    System.Diagnostics.Debug.Assert(entities.Scheduled.Populated);
    entities.Existing.Populated = false;

    return Read("ReadGeneticTestAccount2",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber", entities.Scheduled.GtaAccountNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Existing.AccountNumber = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadVendor1()
  {
    entities.ExistingTestSite.Populated = false;

    return Read("ReadVendor1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTestSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingTestSite.Name = db.GetString(reader, 1);
        entities.ExistingTestSite.Number = db.GetNullableString(reader, 2);
        entities.ExistingTestSite.Populated = true;
      });
  }

  private bool ReadVendor2()
  {
    System.Diagnostics.Debug.Assert(entities.Scheduled.Populated);
    entities.ExistingTestSite.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.Scheduled.VenIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTestSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingTestSite.Name = db.GetString(reader, 1);
        entities.ExistingTestSite.Number = db.GetNullableString(reader, 2);
        entities.ExistingTestSite.Populated = true;
      });
  }

  private void UpdateGeneticTest1()
  {
    entities.Scheduled.Populated = false;
    Update("UpdateGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.VenIdentifier = null;
    entities.Scheduled.Populated = true;
  }

  private void UpdateGeneticTest2()
  {
    var labCaseNo = export.GeneticTestInformation.LabCaseNo;
    var testType = export.GeneticTestInformation.TestType;
    var actualTestDate = export.GeneticTestInformation.ActualTestDate;
    var testResultReceivedDate =
      export.GeneticTestInformation.ResultReceivedDate;
    var paternityExclusionInd =
      export.GeneticTestInformation.PaternityExcludedInd;
    var paternityProbability =
      export.GeneticTestInformation.PaternityProbability;
    var noticeOfContestReceivedInd =
      export.GeneticTestInformation.ResultContestedInd;
    var startDateOfContest = export.GeneticTestInformation.ContestStartedDate;
    var endDateOfContest = export.GeneticTestInformation.ContestEndedDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var gtaAccountNumber = entities.Existing.AccountNumber;

    entities.Scheduled.Populated = false;
    Update("UpdateGeneticTest2",
      (db, command) =>
      {
        db.SetNullableString(command, "labCaseNo", labCaseNo);
        db.SetNullableString(command, "testType", testType);
        db.SetNullableDate(command, "actualTestDate", actualTestDate);
        db.SetNullableDate(command, "resultRcvdDate", testResultReceivedDate);
        db.SetNullableString(command, "patExclInd", paternityExclusionInd);
        db.SetNullableDecimal(command, "patProbability", paternityProbability);
        db.SetNullableString(
          command, "contestRcvdInd", noticeOfContestReceivedInd);
        db.SetNullableDate(command, "startDateOfCont", startDateOfContest);
        db.SetNullableDate(command, "endDateOfContes", endDateOfContest);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "gtaAccountNumber", gtaAccountNumber);
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LabCaseNo = labCaseNo;
    entities.Scheduled.TestType = testType;
    entities.Scheduled.ActualTestDate = actualTestDate;
    entities.Scheduled.TestResultReceivedDate = testResultReceivedDate;
    entities.Scheduled.PaternityExclusionInd = paternityExclusionInd;
    entities.Scheduled.PaternityProbability = paternityProbability;
    entities.Scheduled.NoticeOfContestReceivedInd = noticeOfContestReceivedInd;
    entities.Scheduled.StartDateOfContest = startDateOfContest;
    entities.Scheduled.EndDateOfContest = endDateOfContest;
    entities.Scheduled.LastUpdatedBy = lastUpdatedBy;
    entities.Scheduled.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Scheduled.GtaAccountNumber = gtaAccountNumber;
    entities.Scheduled.Populated = true;
  }

  private void UpdateGeneticTest3()
  {
    var labCaseNo = export.GeneticTestInformation.LabCaseNo;
    var testType = export.GeneticTestInformation.TestType;
    var actualTestDate = export.GeneticTestInformation.ActualTestDate;
    var testResultReceivedDate =
      export.GeneticTestInformation.ResultReceivedDate;
    var paternityExclusionInd =
      export.GeneticTestInformation.PaternityExcludedInd;
    var paternityProbability =
      export.GeneticTestInformation.PaternityProbability;
    var noticeOfContestReceivedInd =
      export.GeneticTestInformation.ResultContestedInd;
    var startDateOfContest = export.GeneticTestInformation.ContestStartedDate;
    var endDateOfContest = export.GeneticTestInformation.ContestEndedDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var gtaAccountNumber = entities.Existing.AccountNumber;
    var venIdentifier = entities.ExistingTestSite.Identifier;

    entities.Scheduled.Populated = false;
    Update("UpdateGeneticTest3",
      (db, command) =>
      {
        db.SetNullableString(command, "labCaseNo", labCaseNo);
        db.SetNullableString(command, "testType", testType);
        db.SetNullableDate(command, "actualTestDate", actualTestDate);
        db.SetNullableDate(command, "resultRcvdDate", testResultReceivedDate);
        db.SetNullableString(command, "patExclInd", paternityExclusionInd);
        db.SetNullableDecimal(command, "patProbability", paternityProbability);
        db.SetNullableString(
          command, "contestRcvdInd", noticeOfContestReceivedInd);
        db.SetNullableDate(command, "startDateOfCont", startDateOfContest);
        db.SetNullableDate(command, "endDateOfContes", endDateOfContest);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "gtaAccountNumber", gtaAccountNumber);
        db.SetNullableInt32(command, "venIdentifier", venIdentifier);
        db.SetInt32(command, "testNumber", entities.Scheduled.TestNumber);
      });

    entities.Scheduled.LabCaseNo = labCaseNo;
    entities.Scheduled.TestType = testType;
    entities.Scheduled.ActualTestDate = actualTestDate;
    entities.Scheduled.TestResultReceivedDate = testResultReceivedDate;
    entities.Scheduled.PaternityExclusionInd = paternityExclusionInd;
    entities.Scheduled.PaternityProbability = paternityProbability;
    entities.Scheduled.NoticeOfContestReceivedInd = noticeOfContestReceivedInd;
    entities.Scheduled.StartDateOfContest = startDateOfContest;
    entities.Scheduled.EndDateOfContest = endDateOfContest;
    entities.Scheduled.LastUpdatedBy = lastUpdatedBy;
    entities.Scheduled.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Scheduled.GtaAccountNumber = gtaAccountNumber;
    entities.Scheduled.VenIdentifier = venIdentifier;
    entities.Scheduled.Populated = true;
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
    /// A value of PersistentExistingFathr.
    /// </summary>
    [JsonPropertyName("persistentExistingFathr")]
    public CaseRole PersistentExistingFathr
    {
      get => persistentExistingFathr ??= new();
      set => persistentExistingFathr = value;
    }

    /// <summary>
    /// A value of PersistentExistCaseroleMother.
    /// </summary>
    [JsonPropertyName("persistentExistCaseroleMother")]
    public CaseRole PersistentExistCaseroleMother
    {
      get => persistentExistCaseroleMother ??= new();
      set => persistentExistCaseroleMother = value;
    }

    /// <summary>
    /// A value of PersistentExistCaseroleChild.
    /// </summary>
    [JsonPropertyName("persistentExistCaseroleChild")]
    public CaseRole PersistentExistCaseroleChild
    {
      get => persistentExistCaseroleChild ??= new();
      set => persistentExistCaseroleChild = value;
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

    private CaseRole persistentExistingFathr;
    private CaseRole persistentExistCaseroleMother;
    private CaseRole persistentExistCaseroleChild;
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
    /// A value of TempCurrent.
    /// </summary>
    [JsonPropertyName("tempCurrent")]
    public Vendor TempCurrent
    {
      get => tempCurrent ??= new();
      set => tempCurrent = value;
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
    /// A value of ErrorInTestTime.
    /// </summary>
    [JsonPropertyName("errorInTestTime")]
    public Common ErrorInTestTime
    {
      get => errorInTestTime ??= new();
      set => errorInTestTime = value;
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
    /// A value of SchedMotherTestTime.
    /// </summary>
    [JsonPropertyName("schedMotherTestTime")]
    public WorkTime SchedMotherTestTime
    {
      get => schedMotherTestTime ??= new();
      set => schedMotherTestTime = value;
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
    /// A value of GeneticTestDetChanged.
    /// </summary>
    [JsonPropertyName("geneticTestDetChanged")]
    public Common GeneticTestDetChanged
    {
      get => geneticTestDetChanged ??= new();
      set => geneticTestDetChanged = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Vendor Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private Vendor tempCurrent;
    private VendorAddress vendorAddress;
    private Common errorInTestTime;
    private WorkTime schedChildTestTime;
    private WorkTime schedMotherTestTime;
    private WorkTime schedFatherTestTime;
    private Common geneticTestDetChanged;
    private GeneticTest latest;
    private Common latestGeneticTestFound;
    private Common fatherPersGenTestFound;
    private Common motherPersGenTestFound;
    private Common childPersGenTestFound;
    private Vendor temp;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public GeneticTestAccount Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    private GeneticTest scheduled;
    private GeneticTestAccount existing;
    private Vendor existingTestSite;
  }
#endregion
}
