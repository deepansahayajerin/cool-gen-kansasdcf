// Program: OE_GTSC_VALIDATE_GENETIC_TEST, ID: 371797161, model: 746.
// Short name: SWE00922
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
/// A program: OE_GTSC_VALIDATE_GENETIC_TEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block validates input for Schedule Genetic Test, Receive 
/// Genetic Test Confirmation and Receive Genetic Test Result.
/// </para>
/// </summary>
[Serializable]
public partial class OeGtscValidateGeneticTest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_VALIDATE_GENETIC_TEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscValidateGeneticTest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscValidateGeneticTest.
  /// </summary>
  public OeGtscValidateGeneticTest(IContext context, Import import,
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
    // This action block validate the details of genetic test.
    // PROCESSING:
    // The input to this action block are the command and the screen details. 
    // Based on the command (SCHEDULE/ RCVCNFRM/ RCVRSLT) the screen data are
    // validated.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // GENETIC_TEST_ACCOUNT		- R - -
    // VENDOR				- R - -
    // ADDRESS				- R - -
    // DATABASE FILES USED:
    // CREATED BY:		govindaraj.
    // DATE CREATED:		12-19-1994.
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGREQ	DESCRIPTION
    // govind	12-19-94	Initial coding.
    // raju	01-09-97	added checks for
    // 		pat. ind. and probability
    // rcg	03-11-98	H00032901	edits on show indicator when = 'N'
    // *********************************************
    // ******** SPECIAL MAINTENANCE ********************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG ****************
    // -------------------------------------------------------------------
    // PR# 81845            Vithal Madhira          03/31/2000
    // It is decided that 'Mother' is optional on GTSC screen. The users want 
    // the ability to do 'Motherless Comparisons'  of the genetic test results.
    // The code is modified accordingly.
    // -------------------------------------------------------------------
    export.ErrorCodes.Index = -1;
    export.LastErrorEntryNo.Count = 0;
    export.GeneticTestInformation.Assign(import.GeneticTestInformation);

    if (IsEmpty(export.GeneticTestInformation.CaseNumber))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 1;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (!IsEmpty(export.GeneticTestInformation.CourtOrderNo))
    {
      // ---------------------------------------------
      // The worker must have chosen a legal action using LACN screen. Just 
      // court case no alone is not sufficient
      // ---------------------------------------------
      if (!ReadLegalAction())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 41;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (IsEmpty(export.GeneticTestInformation.GeneticTestAccountNo))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 2;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (!ReadGeneticTestAccount())
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 3;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (IsEmpty(export.GeneticTestInformation.TestType))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 4;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    local.Code.CodeName = "GENETIC TEST TYPE";
    local.CodeValue.Cdvalue = export.GeneticTestInformation.TestType;
    UseCabValidateCodeValue();

    if (AsChar(local.ValidTestType.Flag) == 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 5;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    // ***	Validate Father Genetic Test Information	***
    if (IsEmpty(export.GeneticTestInformation.FatherDrawSiteId))
    {
      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.FatherSchedTestDate) || !
        IsEmpty(export.GeneticTestInformation.FatherSchedTestTime) || !
        IsEmpty(export.GeneticTestInformation.FatherSampleCollectedInd) || !
        IsEmpty(export.GeneticTestInformation.FatherShowInd) || !
        IsEmpty(export.GeneticTestInformation.FatherSpecimenId))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 6;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherDrawSiteId))
    {
      if (Verify(export.GeneticTestInformation.FatherDrawSiteId, " 0123456789") !=
        0)
      {
        // ---------------------------------------------
        // Invalid character in vendor ID
        // ---------------------------------------------
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 6;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
      else
      {
        local.Temp.Identifier =
          (int)StringToNumber(export.GeneticTestInformation.FatherDrawSiteId);

        if (ReadVendorAddressVendor2())
        {
          export.GeneticTestInformation.FatherDrawSiteVendorName =
            entities.ExistingFatherDrawSiteVendor.Name;
          export.GeneticTestInformation.FatherDrawSiteCity =
            entities.ExistingFatherDrawSiteVendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.FatherDrawSiteState =
            entities.ExistingFatherDrawSiteVendorAddress.State ?? Spaces(2);
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 6;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
    }

    local.WorkTime.TimeWithAmPm =
      export.GeneticTestInformation.FatherSchedTestTime;

    if (!IsEmpty(local.WorkTime.TimeWithAmPm))
    {
      UseCabConvertTimeFormat();

      if (AsChar(local.ErrorInTimeConversion.Flag) == 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 38;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (IsEmpty(export.GeneticTestInformation.FatherCollectSampleInd))
    {
      switch(AsChar(export.GeneticTestInformation.FatherReuseSampleInd))
      {
        case ' ':
          export.GeneticTestInformation.FatherCollectSampleInd = "Y";

          break;
        case 'Y':
          export.GeneticTestInformation.FatherCollectSampleInd = "N";

          break;
        case 'N':
          export.GeneticTestInformation.FatherCollectSampleInd = "Y";

          break;
        default:
          break;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherCollectSampleInd) && AsChar
      (export.GeneticTestInformation.FatherCollectSampleInd) != 'Y' && AsChar
      (export.GeneticTestInformation.FatherCollectSampleInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 7;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (IsEmpty(export.GeneticTestInformation.FatherReuseSampleInd))
    {
      switch(AsChar(export.GeneticTestInformation.FatherCollectSampleInd))
      {
        case 'Y':
          export.GeneticTestInformation.FatherReuseSampleInd = "N";

          break;
        case 'N':
          export.GeneticTestInformation.FatherReuseSampleInd = "Y";

          break;
        default:
          break;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherReuseSampleInd) && AsChar
      (export.GeneticTestInformation.FatherReuseSampleInd) != 'Y' && AsChar
      (export.GeneticTestInformation.FatherReuseSampleInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 8;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (AsChar(export.GeneticTestInformation.FatherCollectSampleInd) == 'Y' && AsChar
      (export.GeneticTestInformation.FatherReuseSampleInd) == 'Y' || AsChar
      (export.GeneticTestInformation.FatherCollectSampleInd) == 'N' && AsChar
      (export.GeneticTestInformation.FatherReuseSampleInd) == 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 8;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (AsChar(export.GeneticTestInformation.FatherReuseSampleInd) == 'Y')
    {
      if (export.GeneticTestInformation.FatherPrevSampGtestNumber == 0 || export
        .GeneticTestInformation.FatherPrevSampPerGenTestId == 0 || IsEmpty
        (export.GeneticTestInformation.FatherPrevSampSpecimenId) || IsEmpty
        (export.GeneticTestInformation.FatherPrevSampleLabCaseNo) || IsEmpty
        (export.GeneticTestInformation.FatherPrevSampTestType))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 27;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (AsChar(export.GeneticTestInformation.FatherReuseSampleInd) == 'N' || IsEmpty
      (export.GeneticTestInformation.FatherReuseSampleInd))
    {
      if (export.GeneticTestInformation.FatherPrevSampGtestNumber != 0 || export
        .GeneticTestInformation.FatherPrevSampPerGenTestId != 0 || !
        IsEmpty(export.GeneticTestInformation.FatherPrevSampSpecimenId) || !
        IsEmpty(export.GeneticTestInformation.FatherPrevSampleLabCaseNo) || !
        IsEmpty(export.GeneticTestInformation.FatherPrevSampTestType))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 30;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (IsEmpty(export.GeneticTestInformation.FatherSampleCollectedInd) && AsChar
      (export.GeneticTestInformation.FatherReuseSampleInd) == 'N')
    {
      if (Lt(local.Zero.Date, export.GeneticTestInformation.ActualTestDate) || Lt
        (local.Zero.Date, export.GeneticTestInformation.ResultReceivedDate))
      {
        export.GeneticTestInformation.FatherSampleCollectedInd = "Y";
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherSampleCollectedInd) && AsChar
      (export.GeneticTestInformation.FatherSampleCollectedInd) != 'Y' && AsChar
      (export.GeneticTestInformation.FatherSampleCollectedInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 16;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherShowInd) && AsChar
      (export.GeneticTestInformation.FatherShowInd) != 'Y' && AsChar
      (export.GeneticTestInformation.FatherShowInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 24;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherSpecimenId))
    {
      if (AsChar(export.GeneticTestInformation.FatherSampleCollectedInd) != 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 42;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.FatherSpecimenId) || export
      .GeneticTestInformation.ActualTestDate != null || !
      IsEmpty(export.GeneticTestInformation.PaternityExcludedInd) || export
      .GeneticTestInformation.ResultReceivedDate != null || export
      .GeneticTestInformation.PaternityProbability > 0)
    {
      export.GeneticTestInformation.FatherShowInd = "Y";
    }
    else
    {
      // *********************************************
      // RCG	H00032901	
      if (AsChar(export.GeneticTestInformation.FatherShowInd) == 'Z')
      {
        if (IsEmpty(export.GeneticTestInformation.FatherShowInd))
        {
          export.GeneticTestInformation.FatherShowInd = "N";
        }
      }
    }

    if (Lt(new DateTime(1, 1, 1),
      export.GeneticTestInformation.FatherSchedTestDate))
    {
      if (AsChar(export.GeneticTestInformation.FatherCollectSampleInd) == 'Y'
        && AsChar(export.GeneticTestInformation.FatherSampleCollectedInd) != 'Y'
        )
      {
        if (Equal(import.Standard.Command, "SCHEDULE"))
        {
          if (!Lt(Now().Date, export.GeneticTestInformation.FatherSchedTestDate))
            
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 35;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
        }
      }

      if (AsChar(export.GeneticTestInformation.FatherReuseSampleInd) == 'Y')
      {
        if (Equal(import.Standard.Command, "SCHEDULE"))
        {
          if (Lt(Now().Date, export.GeneticTestInformation.FatherSchedTestDate))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 35;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
        }
      }
    }

    if (Lt(Now().Date, export.GeneticTestInformation.FatherSchedTestDate))
    {
      if (AsChar(export.GeneticTestInformation.FatherSampleCollectedInd) == 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 54;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (!IsEmpty(export.GeneticTestInformation.FatherShowInd))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 57;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    // ***	Validate Mother Genetic Test Information	***
    // -----------------------------------------------------------------
    // PR# 81845    If 'Mother' is not selected on COMP to do 'Motherless 
    // Comparisons' genetic testing, we must bypass the following edits.
    //                                      
    // -- Vithal (03/31/2000)
    // -------------------------------------------------------------------
    if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
    {
      if (IsEmpty(export.GeneticTestInformation.MotherDrawSiteId))
      {
        if (Lt(new DateTime(1, 1, 1),
          export.GeneticTestInformation.MotherSchedTestDate) || !
          IsEmpty(export.GeneticTestInformation.MotherSchedTestTime) || !
          IsEmpty(export.GeneticTestInformation.MotherSampleCollectedInd) || !
          IsEmpty(export.GeneticTestInformation.MotherShowInd) || !
          IsEmpty(export.GeneticTestInformation.MotherSpecimenId))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 9;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherDrawSiteId))
      {
        if (Verify(export.GeneticTestInformation.MotherDrawSiteId, " 0123456789")
          != 0)
        {
          // ---------------------------------------------
          // Invalid character in vendor ID
          // ---------------------------------------------
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 9;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
        else
        {
          local.Temp.Identifier =
            (int)StringToNumber(export.GeneticTestInformation.MotherDrawSiteId);
            

          if (ReadVendorAddressVendor3())
          {
            export.GeneticTestInformation.MotherDrawSiteVendorName =
              entities.ExistingMotherDrawSiteVendor.Name;
            export.GeneticTestInformation.MotherDrawSiteCity =
              entities.ExistingMotherDrawSiteVendorAddress.City ?? Spaces(15);
            export.GeneticTestInformation.MotherDrawSiteState =
              entities.ExistingMotherDrawSiteVendorAddress.State ?? Spaces(2);
          }
          else
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 9;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
        }
      }

      local.WorkTime.TimeWithAmPm =
        export.GeneticTestInformation.MotherSchedTestTime;

      if (!IsEmpty(local.WorkTime.TimeWithAmPm))
      {
        UseCabConvertTimeFormat();

        if (AsChar(local.ErrorInTimeConversion.Flag) == 'Y')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 39;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (IsEmpty(export.GeneticTestInformation.MotherCollectSampleInd))
      {
        switch(AsChar(export.GeneticTestInformation.MotherReuseSampleInd))
        {
          case ' ':
            export.GeneticTestInformation.MotherCollectSampleInd = "Y";

            break;
          case 'Y':
            export.GeneticTestInformation.MotherCollectSampleInd = "N";

            break;
          case 'N':
            export.GeneticTestInformation.MotherCollectSampleInd = "Y";

            break;
          default:
            break;
        }
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherCollectSampleInd) && AsChar
        (export.GeneticTestInformation.MotherCollectSampleInd) != 'Y' && AsChar
        (export.GeneticTestInformation.MotherCollectSampleInd) != 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 10;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (IsEmpty(export.GeneticTestInformation.MotherReuseSampleInd))
      {
        switch(AsChar(export.GeneticTestInformation.MotherCollectSampleInd))
        {
          case 'Y':
            export.GeneticTestInformation.MotherReuseSampleInd = "N";

            break;
          case 'N':
            export.GeneticTestInformation.MotherReuseSampleInd = "Y";

            break;
          default:
            break;
        }
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherReuseSampleInd) && AsChar
        (export.GeneticTestInformation.MotherReuseSampleInd) != 'Y' && AsChar
        (export.GeneticTestInformation.MotherReuseSampleInd) != 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 11;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (AsChar(export.GeneticTestInformation.MotherCollectSampleInd) == 'Y'
        && AsChar(export.GeneticTestInformation.MotherReuseSampleInd) == 'Y'
        || AsChar(export.GeneticTestInformation.MotherCollectSampleInd) == 'N'
        && AsChar(export.GeneticTestInformation.MotherReuseSampleInd) == 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 11;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (AsChar(export.GeneticTestInformation.MotherReuseSampleInd) == 'Y')
      {
        if (export.GeneticTestInformation.MotherPrevSampGtestNumber == 0 || export
          .GeneticTestInformation.MotherPrevSampPerGenTestId == 0 || IsEmpty
          (export.GeneticTestInformation.MotherPrevSampSpecimenId) || IsEmpty
          (export.GeneticTestInformation.MotherPrevSampLabCaseNo) || IsEmpty
          (export.GeneticTestInformation.MotherPrevSampTestType))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 28;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (IsEmpty(export.GeneticTestInformation.MotherReuseSampleInd) || AsChar
        (export.GeneticTestInformation.MotherReuseSampleInd) == 'N')
      {
        if (export.GeneticTestInformation.MotherPrevSampGtestNumber != 0 || export
          .GeneticTestInformation.MotherPrevSampPerGenTestId != 0 || !
          IsEmpty(export.GeneticTestInformation.MotherPrevSampSpecimenId) || !
          IsEmpty(export.GeneticTestInformation.MotherPrevSampLabCaseNo) || !
          IsEmpty(export.GeneticTestInformation.MotherPrevSampTestType))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 31;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (IsEmpty(export.GeneticTestInformation.MotherSampleCollectedInd) && AsChar
        (export.GeneticTestInformation.MotherReuseSampleInd) == 'N')
      {
        if (Lt(local.Zero.Date, export.GeneticTestInformation.ActualTestDate) ||
          Lt
          (local.Zero.Date, export.GeneticTestInformation.ResultReceivedDate))
        {
          export.GeneticTestInformation.MotherSampleCollectedInd = "Y";
        }
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherSampleCollectedInd) && AsChar
        (export.GeneticTestInformation.MotherSampleCollectedInd) != 'Y' && AsChar
        (export.GeneticTestInformation.MotherSampleCollectedInd) != 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 17;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherShowInd) && AsChar
        (export.GeneticTestInformation.MotherShowInd) != 'Y' && AsChar
        (export.GeneticTestInformation.MotherShowInd) != 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 25;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherSpecimenId))
      {
        if (AsChar(export.GeneticTestInformation.MotherSampleCollectedInd) != 'Y'
          )
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 43;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherSpecimenId) || export
        .GeneticTestInformation.ActualTestDate != null || !
        IsEmpty(export.GeneticTestInformation.PaternityExcludedInd) || export
        .GeneticTestInformation.ResultReceivedDate != null || export
        .GeneticTestInformation.PaternityProbability > 0)
      {
        export.GeneticTestInformation.MotherShowInd = "Y";
      }
      else
      {
        // *********************************************
        // RCG	H00032901
        if (AsChar(export.GeneticTestInformation.MotherShowInd) == 'Z')
        {
          if (IsEmpty(export.GeneticTestInformation.MotherShowInd))
          {
            export.GeneticTestInformation.MotherShowInd = "N";
          }
        }
      }

      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.MotherSchedTestDate))
      {
        if (AsChar(export.GeneticTestInformation.MotherCollectSampleInd) == 'Y'
          && AsChar(export.GeneticTestInformation.MotherSampleCollectedInd) != 'Y'
          )
        {
          if (Equal(import.Standard.Command, "SCHEDULE"))
          {
            if (!Lt(Now().Date,
              export.GeneticTestInformation.MotherSchedTestDate))
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 36;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            }
          }
        }

        if (AsChar(export.GeneticTestInformation.MotherReuseSampleInd) == 'Y')
        {
          if (Equal(import.Standard.Command, "SCHEDULE"))
          {
            if (Lt(Now().Date, export.GeneticTestInformation.MotherSchedTestDate))
              
            {
              ++export.ErrorCodes.Index;
              export.ErrorCodes.CheckSize();

              export.ErrorCodes.Update.DetailErrorCode.Count = 36;
              export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            }
          }
        }
      }

      if (Lt(Now().Date, export.GeneticTestInformation.MotherSchedTestDate))
      {
        if (AsChar(export.GeneticTestInformation.MotherSampleCollectedInd) == 'Y'
          )
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 55;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }

        if (!IsEmpty(export.GeneticTestInformation.MotherShowInd))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 58;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
    }

    // ***	Validate Child Genetic Test Information		***
    if (IsEmpty(export.GeneticTestInformation.ChildDrawSiteId))
    {
      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.ChildSchedTestDate) || !
        IsEmpty(export.GeneticTestInformation.ChildSchedTestTime) || !
        IsEmpty(export.GeneticTestInformation.ChildSampleCollectedInd) || !
        IsEmpty(export.GeneticTestInformation.ChildShowInd) || !
        IsEmpty(export.GeneticTestInformation.ChildSpecimenId))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 12;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildDrawSiteId))
    {
      if (Verify(export.GeneticTestInformation.ChildDrawSiteId, " 0123456789") !=
        0)
      {
        // ---------------------------------------------
        // Invalid character in vendor ID
        // ---------------------------------------------
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 12;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
      else
      {
        local.Temp.Identifier =
          (int)StringToNumber(export.GeneticTestInformation.ChildDrawSiteId);

        if (ReadVendorAddressVendor1())
        {
          export.GeneticTestInformation.ChildDrawSiteVendorName =
            entities.ExistingChildDrawSiteVendor.Name;
          export.GeneticTestInformation.ChildDrawSiteCity =
            entities.ExistingChildDrawSiteVendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.ChildDrawSiteState =
            entities.ExistingChildDrawSiteVendorAddress.State ?? Spaces(2);
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 12;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
    }

    local.WorkTime.TimeWithAmPm =
      export.GeneticTestInformation.ChildSchedTestTime;

    if (!IsEmpty(local.WorkTime.TimeWithAmPm))
    {
      UseCabConvertTimeFormat();

      if (AsChar(local.ErrorInTimeConversion.Flag) == 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 40;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (IsEmpty(export.GeneticTestInformation.ChildCollectSampleInd))
    {
      switch(AsChar(export.GeneticTestInformation.ChildReuseSampleInd))
      {
        case ' ':
          export.GeneticTestInformation.ChildCollectSampleInd = "Y";

          break;
        case 'Y':
          export.GeneticTestInformation.ChildCollectSampleInd = "N";

          break;
        case 'N':
          export.GeneticTestInformation.ChildCollectSampleInd = "Y";

          break;
        default:
          break;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildCollectSampleInd) && AsChar
      (export.GeneticTestInformation.ChildCollectSampleInd) != 'Y' && AsChar
      (export.GeneticTestInformation.ChildCollectSampleInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 13;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (IsEmpty(export.GeneticTestInformation.ChildReuseSampleInd))
    {
      switch(AsChar(export.GeneticTestInformation.ChildCollectSampleInd))
      {
        case 'Y':
          export.GeneticTestInformation.ChildReuseSampleInd = "N";

          break;
        case 'N':
          export.GeneticTestInformation.ChildReuseSampleInd = "Y";

          break;
        default:
          break;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildReuseSampleInd) && AsChar
      (export.GeneticTestInformation.ChildReuseSampleInd) != 'Y' && AsChar
      (export.GeneticTestInformation.ChildReuseSampleInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 14;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (AsChar(export.GeneticTestInformation.ChildCollectSampleInd) == 'Y' && AsChar
      (export.GeneticTestInformation.ChildReuseSampleInd) == 'Y' || AsChar
      (export.GeneticTestInformation.ChildCollectSampleInd) == 'N' && AsChar
      (export.GeneticTestInformation.ChildReuseSampleInd) == 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 14;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (AsChar(export.GeneticTestInformation.ChildReuseSampleInd) == 'Y')
    {
      if (export.GeneticTestInformation.ChildPrevSampGtestNumber == 0 || export
        .GeneticTestInformation.ChildPrevSampPerGenTestId == 0 || IsEmpty
        (export.GeneticTestInformation.ChildPrevSampSpecimenId) || IsEmpty
        (export.GeneticTestInformation.ChildPrevSampLabCaseNo) || IsEmpty
        (export.GeneticTestInformation.ChildPrevSampTestType))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 29;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (IsEmpty(export.GeneticTestInformation.ChildReuseSampleInd) || AsChar
      (export.GeneticTestInformation.ChildReuseSampleInd) == 'N')
    {
      if (export.GeneticTestInformation.ChildPrevSampGtestNumber != 0 || export
        .GeneticTestInformation.ChildPrevSampPerGenTestId != 0 || !
        IsEmpty(export.GeneticTestInformation.ChildPrevSampSpecimenId) || !
        IsEmpty(export.GeneticTestInformation.ChildPrevSampLabCaseNo) || !
        IsEmpty(export.GeneticTestInformation.ChildPrevSampTestType))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 32;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (IsEmpty(export.GeneticTestInformation.ChildSampleCollectedInd) && AsChar
      (export.GeneticTestInformation.ChildReuseSampleInd) == 'N')
    {
      if (Lt(local.Zero.Date, export.GeneticTestInformation.ActualTestDate) || Lt
        (local.Zero.Date, export.GeneticTestInformation.ResultReceivedDate))
      {
        export.GeneticTestInformation.ChildSampleCollectedInd = "Y";
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildSampleCollectedInd) && AsChar
      (export.GeneticTestInformation.ChildSampleCollectedInd) != 'Y' && AsChar
      (export.GeneticTestInformation.ChildSampleCollectedInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 18;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildShowInd) && AsChar
      (export.GeneticTestInformation.ChildShowInd) != 'Y' && AsChar
      (export.GeneticTestInformation.ChildShowInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 26;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildSpecimenId))
    {
      if (AsChar(export.GeneticTestInformation.ChildSampleCollectedInd) != 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 44;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.ChildSpecimenId) || export
      .GeneticTestInformation.ActualTestDate != null || !
      IsEmpty(export.GeneticTestInformation.PaternityExcludedInd) || export
      .GeneticTestInformation.ResultReceivedDate != null || export
      .GeneticTestInformation.PaternityProbability > 0)
    {
      export.GeneticTestInformation.ChildShowInd = "Y";
    }
    else
    {
      // *********************************************
      // RCG	H00032901
      if (AsChar(export.GeneticTestInformation.ChildShowInd) == 'Z')
      {
        if (IsEmpty(export.GeneticTestInformation.ChildShowInd))
        {
          export.GeneticTestInformation.ChildShowInd = "N";
        }
      }
    }

    if (Lt(new DateTime(1, 1, 1),
      export.GeneticTestInformation.ChildSchedTestDate))
    {
      if (AsChar(export.GeneticTestInformation.ChildCollectSampleInd) == 'Y'
        && AsChar(export.GeneticTestInformation.ChildSampleCollectedInd) != 'Y'
        )
      {
        if (Equal(import.Standard.Command, "SCHEDULE"))
        {
          if (!Lt(Now().Date, export.GeneticTestInformation.ChildSchedTestDate))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 37;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
        }
      }

      if (AsChar(export.GeneticTestInformation.ChildReuseSampleInd) == 'Y')
      {
        if (Equal(import.Standard.Command, "SCHEDULE"))
        {
          if (Lt(Now().Date, export.GeneticTestInformation.ChildSchedTestDate))
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.ErrorCodes.Update.DetailErrorCode.Count = 37;
            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          }
        }
      }
    }

    if (Lt(Now().Date, export.GeneticTestInformation.ChildSchedTestDate))
    {
      if (AsChar(export.GeneticTestInformation.ChildSampleCollectedInd) == 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 56;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (!IsEmpty(export.GeneticTestInformation.ChildShowInd))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 59;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    // ***	Validate Genetic Test Information		***
    if (!IsEmpty(export.GeneticTestInformation.TestSiteVendorId))
    {
      if (Verify(export.GeneticTestInformation.TestSiteVendorId, " 0123456789") !=
        0)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 15;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
      else
      {
        local.Temp.Identifier =
          (int)StringToNumber(export.GeneticTestInformation.TestSiteVendorId);

        if (!ReadVendor())
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 15;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
    }

    if (Lt(new DateTime(1, 1, 1), export.GeneticTestInformation.ActualTestDate))
    {
      if (Lt(export.GeneticTestInformation.ActualTestDate,
        export.GeneticTestInformation.FatherSchedTestDate) || Lt
        (export.GeneticTestInformation.ActualTestDate,
        export.GeneticTestInformation.MotherSchedTestDate) || Lt
        (export.GeneticTestInformation.ActualTestDate,
        export.GeneticTestInformation.ChildSchedTestDate) || Lt
        (Now().Date, export.GeneticTestInformation.ActualTestDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 19;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (!Lt(new DateTime(1, 1, 1), export.GeneticTestInformation.ActualTestDate))
      
    {
      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.ResultReceivedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 19;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (!IsEmpty(export.GeneticTestInformation.PaternityExcludedInd) && AsChar
      (export.GeneticTestInformation.PaternityExcludedInd) != 'Y' && AsChar
      (export.GeneticTestInformation.PaternityExcludedInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 21;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (export.GeneticTestInformation.ResultReceivedDate != null)
    {
      if (Lt(export.GeneticTestInformation.ResultReceivedDate,
        export.GeneticTestInformation.FatherSchedTestDate) || Lt
        (export.GeneticTestInformation.ResultReceivedDate,
        export.GeneticTestInformation.MotherSchedTestDate) || Lt
        (export.GeneticTestInformation.ResultReceivedDate,
        export.GeneticTestInformation.ChildSchedTestDate) || Lt
        (export.GeneticTestInformation.ResultReceivedDate,
        export.GeneticTestInformation.ActualTestDate) || Lt
        (Now().Date, export.GeneticTestInformation.ResultReceivedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 20;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (IsEmpty(export.GeneticTestInformation.TestSiteVendorId))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 52;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      // ---------------------------------------------
      // Raju : 01/08/1997:1600hrs CST
      //  - check with SID
      //  - pat prob was a non zero +ve value entry
      //  - so this means event result positive will
      //    always be raised. hence we allow 0 value
      //    to be entered.
      // ---------------------------------------------
      if (export.GeneticTestInformation.PaternityProbability < 0)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 53;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      // ---------------------------------------------
      // Raju 01/09/97:1200hrs CST
      // ---------------------------------------------
      // ---------------------------------------------
      // Start of Code
      // ---------------------------------------------
      if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) == 'Y' && export
        .GeneticTestInformation.PaternityProbability != 0)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 60;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
      else if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) != 'Y'
        )
      {
        export.GeneticTestInformation.PaternityExcludedInd = "N";

        if (export.GeneticTestInformation.PaternityProbability == 0)
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 61;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
      else
      {
        export.GeneticTestInformation.PaternityProbability = 0;
      }

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
    }
    else
    {
      // ---------------------------------------------
      // The else clause was placed by Raju
      //   the earlier code did not have else and
      //   checked the else in a seperate if statement
      // ---------------------------------------------
      if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) == 'Y' || export
        .GeneticTestInformation.PaternityProbability > 0 || AsChar
        (export.GeneticTestInformation.ResultContestedInd) == 'Y' || Lt
        (new DateTime(1, 1, 1), export.GeneticTestInformation.ContestStartedDate)
        || Lt
        (new DateTime(1, 1, 1), export.GeneticTestInformation.ContestEndedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 20;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (export.GeneticTestInformation.PaternityProbability >= 100)
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 22;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!IsEmpty(export.GeneticTestInformation.ResultContestedInd) && AsChar
      (export.GeneticTestInformation.ResultContestedInd) != 'Y' && AsChar
      (export.GeneticTestInformation.ResultContestedInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 23;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (AsChar(export.GeneticTestInformation.ResultContestedInd) == 'Y')
    {
      if (!Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.ContestStartedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 33;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.ContestStartedDate))
      {
        if (Lt(Now().Date, export.GeneticTestInformation.ContestStartedDate) ||
          Lt
          (export.GeneticTestInformation.ContestStartedDate,
          export.GeneticTestInformation.ResultReceivedDate) || Lt
          (export.GeneticTestInformation.ContestStartedDate,
          export.GeneticTestInformation.ActualTestDate))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 33;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) != 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 45;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (export.GeneticTestInformation.ActualTestDate == null)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 46;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (export.GeneticTestInformation.ResultReceivedDate == null)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 47;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (export.GeneticTestInformation.PaternityProbability <= 0)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 48;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (AsChar(export.GeneticTestInformation.FatherShowInd) != 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 49;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      // --------------------------------------------------------------------
      // In case of Motherless comparisons this edit is invalid. So check if 
      // Mother is selected ( Mother_cse_person_no> SPACES).
      //                                                   
      // Vithal Madhira (06/21/2000)
      // -------------------------------------------------------------------
      if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
      {
        if (AsChar(export.GeneticTestInformation.MotherShowInd) != 'Y')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 50;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }

      if (AsChar(export.GeneticTestInformation.ChildShowInd) != 'Y')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 51;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (AsChar(export.GeneticTestInformation.ResultContestedInd) == 'N' || IsEmpty
      (export.GeneticTestInformation.ResultContestedInd))
    {
      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.ContestStartedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 33;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (Lt(new DateTime(1, 1, 1),
        export.GeneticTestInformation.ContestEndedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 34;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (Lt(new DateTime(1, 1, 1), export.GeneticTestInformation.ContestEndedDate))
      
    {
      if (Lt(export.GeneticTestInformation.ContestEndedDate,
        export.GeneticTestInformation.ContestStartedDate) || Lt
        (Now().Date, export.GeneticTestInformation.ContestEndedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 34;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
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

    MoveWorkTime(useExport.WorkTime, local.WorkTime);
    local.ErrorInTimeConversion.Flag = useExport.ErrorInConversion.Flag;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidTestType.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadGeneticTestAccount()
  {
    entities.ExistingGeneticTestAccount.Populated = false;

    return Read("ReadGeneticTestAccount",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber",
          export.GeneticTestInformation.GeneticTestAccountNo);
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
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.PatEstab.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadVendor()
  {
    entities.ExistingTestSite.Populated = false;

    return Read("ReadVendor",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTestSite.Identifier = db.GetInt32(reader, 0);
        entities.ExistingTestSite.Name = db.GetString(reader, 1);
        entities.ExistingTestSite.Number = db.GetNullableString(reader, 2);
        entities.ExistingTestSite.PhoneNumber = db.GetNullableInt32(reader, 3);
        entities.ExistingTestSite.ContactPerson =
          db.GetNullableString(reader, 4);
        entities.ExistingTestSite.Populated = true;
      });
  }

  private bool ReadVendorAddressVendor1()
  {
    entities.ExistingChildDrawSiteVendorAddress.Populated = false;
    entities.ExistingChildDrawSiteVendor.Populated = false;

    return Read("ReadVendorAddressVendor1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingChildDrawSiteVendorAddress.VenIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingChildDrawSiteVendor.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingChildDrawSiteVendorAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingChildDrawSiteVendorAddress.City =
          db.GetNullableString(reader, 2);
        entities.ExistingChildDrawSiteVendorAddress.State =
          db.GetNullableString(reader, 3);
        entities.ExistingChildDrawSiteVendor.Name = db.GetString(reader, 4);
        entities.ExistingChildDrawSiteVendor.Number =
          db.GetNullableString(reader, 5);
        entities.ExistingChildDrawSiteVendor.PhoneNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingChildDrawSiteVendor.ContactPerson =
          db.GetNullableString(reader, 7);
        entities.ExistingChildDrawSiteVendorAddress.Populated = true;
        entities.ExistingChildDrawSiteVendor.Populated = true;
      });
  }

  private bool ReadVendorAddressVendor2()
  {
    entities.ExistingFatherDrawSiteVendorAddress.Populated = false;
    entities.ExistingFatherDrawSiteVendor.Populated = false;

    return Read("ReadVendorAddressVendor2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFatherDrawSiteVendorAddress.VenIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingFatherDrawSiteVendor.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingFatherDrawSiteVendorAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingFatherDrawSiteVendorAddress.City =
          db.GetNullableString(reader, 2);
        entities.ExistingFatherDrawSiteVendorAddress.State =
          db.GetNullableString(reader, 3);
        entities.ExistingFatherDrawSiteVendor.Name = db.GetString(reader, 4);
        entities.ExistingFatherDrawSiteVendor.Number =
          db.GetNullableString(reader, 5);
        entities.ExistingFatherDrawSiteVendor.PhoneNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingFatherDrawSiteVendor.ContactPerson =
          db.GetNullableString(reader, 7);
        entities.ExistingFatherDrawSiteVendorAddress.Populated = true;
        entities.ExistingFatherDrawSiteVendor.Populated = true;
      });
  }

  private bool ReadVendorAddressVendor3()
  {
    entities.ExistingMotherDrawSiteVendorAddress.Populated = false;
    entities.ExistingMotherDrawSiteVendor.Populated = false;

    return Read("ReadVendorAddressVendor3",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Temp.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingMotherDrawSiteVendorAddress.VenIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingMotherDrawSiteVendor.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingMotherDrawSiteVendorAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingMotherDrawSiteVendorAddress.City =
          db.GetNullableString(reader, 2);
        entities.ExistingMotherDrawSiteVendorAddress.State =
          db.GetNullableString(reader, 3);
        entities.ExistingMotherDrawSiteVendor.Name = db.GetString(reader, 4);
        entities.ExistingMotherDrawSiteVendor.Number =
          db.GetNullableString(reader, 5);
        entities.ExistingMotherDrawSiteVendor.PhoneNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingMotherDrawSiteVendor.ContactPerson =
          db.GetNullableString(reader, 7);
        entities.ExistingMotherDrawSiteVendorAddress.Populated = true;
        entities.ExistingMotherDrawSiteVendor.Populated = true;
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
    /// A value of PatEstab.
    /// </summary>
    [JsonPropertyName("patEstab")]
    public LegalAction PatEstab
    {
      get => patEstab ??= new();
      set => patEstab = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private LegalAction patEstab;
    private Standard standard;
    private GeneticTestInformation geneticTestInformation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
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
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private Common lastErrorEntryNo;
    private GeneticTestInformation geneticTestInformation;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of ValidTestType.
    /// </summary>
    [JsonPropertyName("validTestType")]
    public Common ValidTestType
    {
      get => validTestType ??= new();
      set => validTestType = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of ErrorInTimeConversion.
    /// </summary>
    [JsonPropertyName("errorInTimeConversion")]
    public Common ErrorInTimeConversion
    {
      get => errorInTimeConversion ??= new();
      set => errorInTimeConversion = value;
    }

    private DateWorkArea zero;
    private Vendor temp;
    private Common validTestType;
    private CodeValue codeValue;
    private Code code;
    private WorkTime workTime;
    private Common errorInTimeConversion;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingFatherDrawSiteVendorAddress.
    /// </summary>
    [JsonPropertyName("existingFatherDrawSiteVendorAddress")]
    public VendorAddress ExistingFatherDrawSiteVendorAddress
    {
      get => existingFatherDrawSiteVendorAddress ??= new();
      set => existingFatherDrawSiteVendorAddress = value;
    }

    /// <summary>
    /// A value of ExistingMotherDrawSiteVendorAddress.
    /// </summary>
    [JsonPropertyName("existingMotherDrawSiteVendorAddress")]
    public VendorAddress ExistingMotherDrawSiteVendorAddress
    {
      get => existingMotherDrawSiteVendorAddress ??= new();
      set => existingMotherDrawSiteVendorAddress = value;
    }

    /// <summary>
    /// A value of ExistingChildDrawSiteVendorAddress.
    /// </summary>
    [JsonPropertyName("existingChildDrawSiteVendorAddress")]
    public VendorAddress ExistingChildDrawSiteVendorAddress
    {
      get => existingChildDrawSiteVendorAddress ??= new();
      set => existingChildDrawSiteVendorAddress = value;
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
    /// A value of ExistingChildDrawSiteVendor.
    /// </summary>
    [JsonPropertyName("existingChildDrawSiteVendor")]
    public Vendor ExistingChildDrawSiteVendor
    {
      get => existingChildDrawSiteVendor ??= new();
      set => existingChildDrawSiteVendor = value;
    }

    /// <summary>
    /// A value of ExistingMotherDrawSiteVendor.
    /// </summary>
    [JsonPropertyName("existingMotherDrawSiteVendor")]
    public Vendor ExistingMotherDrawSiteVendor
    {
      get => existingMotherDrawSiteVendor ??= new();
      set => existingMotherDrawSiteVendor = value;
    }

    /// <summary>
    /// A value of ExistingFatherDrawSiteVendor.
    /// </summary>
    [JsonPropertyName("existingFatherDrawSiteVendor")]
    public Vendor ExistingFatherDrawSiteVendor
    {
      get => existingFatherDrawSiteVendor ??= new();
      set => existingFatherDrawSiteVendor = value;
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

    private LegalAction existingLegalAction;
    private VendorAddress existingFatherDrawSiteVendorAddress;
    private VendorAddress existingMotherDrawSiteVendorAddress;
    private VendorAddress existingChildDrawSiteVendorAddress;
    private Vendor existingTestSite;
    private Vendor existingChildDrawSiteVendor;
    private Vendor existingMotherDrawSiteVendor;
    private Vendor existingFatherDrawSiteVendor;
    private GeneticTestAccount existingGeneticTestAccount;
  }
#endregion
}
